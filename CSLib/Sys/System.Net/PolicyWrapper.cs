using System.Collections;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace System.Net
{
	internal class PolicyWrapper
	{
		private const uint IgnoreUnmatchedCN = 4096u;

		private ICertificatePolicy fwdPolicy;

		private ServicePoint srvPoint;

		private WebRequest request;

		internal PolicyWrapper(ICertificatePolicy policy, ServicePoint sp, WebRequest wr)
		{
			fwdPolicy = policy;
			srvPoint = sp;
			request = wr;
		}

		public bool Accept(X509Certificate Certificate, int CertificateProblem)
		{
			return fwdPolicy.CheckValidationResult(srvPoint, Certificate, request, CertificateProblem);
		}

		internal static uint VerifyChainPolicy(SafeFreeCertChain chainContext, ref ChainPolicyParameter cpp)
		{
			ChainPolicyStatus ps = default(ChainPolicyStatus);
			ps.cbSize = ChainPolicyStatus.StructSize;
			UnsafeNclNativeMethods.NativePKI.CertVerifyCertificateChainPolicy((IntPtr)4L, chainContext, ref cpp, ref ps);
			return ps.dwError;
		}

		private static IgnoreCertProblem MapErrorCode(uint errorCode)
		{
			switch (errorCode)
			{
			case 2148204815u:
			case 2148204820u:
				return IgnoreCertProblem.invalid_name;
			case 2148204806u:
			case 2148204819u:
				return IgnoreCertProblem.invalid_policy;
			case 2148204801u:
				return (IgnoreCertProblem)3;
			case 2148204802u:
				return IgnoreCertProblem.not_time_nested;
			case 2148204809u:
			case 2148204810u:
			case 2148204818u:
				return IgnoreCertProblem.allow_unknown_ca;
			case 2148081682u:
			case 2148081683u:
			case 2148204812u:
			case 2148204814u:
				return IgnoreCertProblem.all_rev_unknown;
			case 2148098073u:
			case 2148204803u:
				return IgnoreCertProblem.invalid_basic_constraints;
			case 2148204816u:
				return IgnoreCertProblem.wrong_usage;
			default:
				return (IgnoreCertProblem)0;
			}
		}

		private unsafe uint[] GetChainErrors(string hostName, X509Chain chain, ref bool fatalError)
		{
			fatalError = false;
			SafeFreeCertChain chainContext = new SafeFreeCertChain(chain.ChainContext);
			ArrayList arrayList = new ArrayList();
			uint num = 0u;
			ChainPolicyParameter cpp = default(ChainPolicyParameter);
			cpp.cbSize = ChainPolicyParameter.StructSize;
			cpp.dwFlags = 0u;
			SSL_EXTRA_CERT_CHAIN_POLICY_PARA sSL_EXTRA_CERT_CHAIN_POLICY_PARA = new SSL_EXTRA_CERT_CHAIN_POLICY_PARA(amIServer: false);
			cpp.pvExtraPolicyPara = &sSL_EXTRA_CERT_CHAIN_POLICY_PARA;
			fixed (char* pwszServerName = hostName)
			{
				if (ServicePointManager.CheckCertificateName)
				{
					sSL_EXTRA_CERT_CHAIN_POLICY_PARA.pwszServerName = pwszServerName;
				}
				while (true)
				{
					num = VerifyChainPolicy(chainContext, ref cpp);
					uint num2 = (uint)MapErrorCode(num);
					arrayList.Add(num);
					if (num == 0)
					{
						break;
					}
					if (num2 == 0)
					{
						fatalError = true;
						break;
					}
					cpp.dwFlags |= num2;
					if (num == 2148204815u && ServicePointManager.CheckCertificateName)
					{
						sSL_EXTRA_CERT_CHAIN_POLICY_PARA.fdwChecks = 4096u;
					}
				}
			}
			return (uint[])arrayList.ToArray(typeof(uint));
		}

		internal bool CheckErrors(string hostName, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
		{
			if (sslPolicyErrors == SslPolicyErrors.None)
			{
				return Accept(certificate, 0);
			}
			if ((sslPolicyErrors & SslPolicyErrors.RemoteCertificateNotAvailable) != 0)
			{
				return Accept(certificate, -2146762491);
			}
			if ((sslPolicyErrors & SslPolicyErrors.RemoteCertificateChainErrors) != 0 || (sslPolicyErrors & SslPolicyErrors.RemoteCertificateNameMismatch) != 0)
			{
				bool fatalError = false;
				uint[] chainErrors = GetChainErrors(hostName, chain, ref fatalError);
				if (fatalError)
				{
					Accept(certificate, -2146893052);
					return false;
				}
				if (chainErrors.Length == 0)
				{
					return Accept(certificate, 0);
				}
				uint[] array = chainErrors;
				foreach (uint certificateProblem in array)
				{
					if (!Accept(certificate, (int)certificateProblem))
					{
						return false;
					}
				}
			}
			return true;
		}
	}
}
