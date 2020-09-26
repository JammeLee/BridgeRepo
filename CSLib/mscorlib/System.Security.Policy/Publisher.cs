using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Security.Permissions;

namespace System.Security.Policy
{
	[Serializable]
	[ComVisible(true)]
	public sealed class Publisher : IIdentityPermissionFactory, IBuiltInEvidence
	{
		private X509Certificate m_cert;

		public X509Certificate Certificate
		{
			get
			{
				if (m_cert == null)
				{
					return null;
				}
				return new X509Certificate(m_cert);
			}
		}

		internal Publisher()
		{
		}

		public Publisher(X509Certificate cert)
		{
			if (cert == null)
			{
				throw new ArgumentNullException("cert");
			}
			m_cert = cert;
		}

		public IPermission CreateIdentityPermission(Evidence evidence)
		{
			return new PublisherIdentityPermission(m_cert);
		}

		public override bool Equals(object o)
		{
			Publisher publisher = o as Publisher;
			if (publisher != null)
			{
				return PublicKeyEquals(m_cert, publisher.m_cert);
			}
			return false;
		}

		internal static bool PublicKeyEquals(X509Certificate cert1, X509Certificate cert2)
		{
			if (cert1 == null)
			{
				return cert2 == null;
			}
			if (cert2 == null)
			{
				return false;
			}
			byte[] publicKey = cert1.GetPublicKey();
			string keyAlgorithm = cert1.GetKeyAlgorithm();
			byte[] keyAlgorithmParameters = cert1.GetKeyAlgorithmParameters();
			byte[] publicKey2 = cert2.GetPublicKey();
			string keyAlgorithm2 = cert2.GetKeyAlgorithm();
			byte[] keyAlgorithmParameters2 = cert2.GetKeyAlgorithmParameters();
			int num = publicKey.Length;
			if (num != publicKey2.Length)
			{
				return false;
			}
			for (int i = 0; i < num; i++)
			{
				if (publicKey[i] != publicKey2[i])
				{
					return false;
				}
			}
			if (!keyAlgorithm.Equals(keyAlgorithm2))
			{
				return false;
			}
			num = keyAlgorithmParameters.Length;
			if (keyAlgorithmParameters2.Length != num)
			{
				return false;
			}
			for (int j = 0; j < num; j++)
			{
				if (keyAlgorithmParameters[j] != keyAlgorithmParameters2[j])
				{
					return false;
				}
			}
			return true;
		}

		public override int GetHashCode()
		{
			return m_cert.GetHashCode();
		}

		public object Copy()
		{
			Publisher publisher = new Publisher();
			if (m_cert != null)
			{
				publisher.m_cert = new X509Certificate(m_cert);
			}
			return publisher;
		}

		internal SecurityElement ToXml()
		{
			SecurityElement securityElement = new SecurityElement("System.Security.Policy.Publisher");
			securityElement.AddAttribute("version", "1");
			securityElement.AddChild(new SecurityElement("X509v3Certificate", (m_cert != null) ? m_cert.GetRawCertDataString() : ""));
			return securityElement;
		}

		int IBuiltInEvidence.OutputToBuffer(char[] buffer, int position, bool verbose)
		{
			buffer[position++] = '\u0001';
			byte[] rawCertData = Certificate.GetRawCertData();
			int num = rawCertData.Length;
			if (verbose)
			{
				BuiltInEvidenceHelper.CopyIntToCharArray(num, buffer, position);
				position += 2;
			}
			Buffer.InternalBlockCopy(rawCertData, 0, buffer, position * 2, num);
			return (num - 1) / 2 + 1 + position;
		}

		int IBuiltInEvidence.GetRequiredSize(bool verbose)
		{
			int num = (Certificate.GetRawCertData().Length - 1) / 2 + 1;
			if (verbose)
			{
				return num + 3;
			}
			return num + 1;
		}

		int IBuiltInEvidence.InitFromBuffer(char[] buffer, int position)
		{
			int intFromCharArray = BuiltInEvidenceHelper.GetIntFromCharArray(buffer, position);
			position += 2;
			byte[] array = new byte[intFromCharArray];
			int num = (intFromCharArray - 1) / 2 + 1;
			Buffer.InternalBlockCopy(buffer, position * 2, array, 0, intFromCharArray);
			m_cert = new X509Certificate(array);
			return position + num;
		}

		public override string ToString()
		{
			return ToXml().ToString();
		}

		internal object Normalize()
		{
			MemoryStream memoryStream = new MemoryStream(m_cert.GetRawCertData());
			memoryStream.Position = 0L;
			return memoryStream;
		}
	}
}
