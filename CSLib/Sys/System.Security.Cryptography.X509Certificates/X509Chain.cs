using System.Net;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Security.Permissions;

namespace System.Security.Cryptography.X509Certificates
{
	public class X509Chain
	{
		private uint m_status;

		private X509ChainPolicy m_chainPolicy;

		private X509ChainStatus[] m_chainStatus;

		private X509ChainElementCollection m_chainElementCollection;

		private SafeCertChainHandle m_safeCertChainHandle;

		private bool m_useMachineContext;

		private readonly object m_syncRoot = new object();

		public IntPtr ChainContext
		{
			[SecurityPermission(SecurityAction.InheritanceDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
			[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
			get
			{
				return m_safeCertChainHandle.DangerousGetHandle();
			}
		}

		public X509ChainPolicy ChainPolicy
		{
			get
			{
				if (m_chainPolicy == null)
				{
					m_chainPolicy = new X509ChainPolicy();
				}
				return m_chainPolicy;
			}
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException("value");
				}
				m_chainPolicy = value;
			}
		}

		public X509ChainStatus[] ChainStatus
		{
			get
			{
				if (m_chainStatus == null)
				{
					if (m_status == 0)
					{
						m_chainStatus = new X509ChainStatus[0];
					}
					else
					{
						m_chainStatus = GetChainStatusInformation(m_status);
					}
				}
				return m_chainStatus;
			}
		}

		public X509ChainElementCollection ChainElements => m_chainElementCollection;

		public static X509Chain Create()
		{
			return (X509Chain)CryptoConfig.CreateFromName("X509Chain");
		}

		public X509Chain()
			: this(useMachineContext: false)
		{
		}

		public X509Chain(bool useMachineContext)
		{
			m_status = 0u;
			m_chainPolicy = null;
			m_chainStatus = null;
			m_chainElementCollection = new X509ChainElementCollection();
			m_safeCertChainHandle = SafeCertChainHandle.InvalidHandle;
			m_useMachineContext = useMachineContext;
		}

		[SecurityPermission(SecurityAction.InheritanceDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public X509Chain(IntPtr chainContext)
		{
			if (chainContext == IntPtr.Zero)
			{
				throw new ArgumentNullException("chainContext");
			}
			m_safeCertChainHandle = CAPISafe.CertDuplicateCertificateChain(chainContext);
			if (m_safeCertChainHandle == null || m_safeCertChainHandle == SafeCertChainHandle.InvalidHandle)
			{
				throw new CryptographicException(SR.GetString("Cryptography_InvalidContextHandle"), "chainContext");
			}
			Init();
		}

		[PermissionSet(SecurityAction.InheritanceDemand, Unrestricted = true)]
		[PermissionSet(SecurityAction.LinkDemand, Unrestricted = true)]
		public bool Build(X509Certificate2 certificate)
		{
			lock (m_syncRoot)
			{
				if (certificate == null || certificate.CertContext.IsInvalid)
				{
					throw new ArgumentException(SR.GetString("Cryptography_InvalidContextHandle"), "certificate");
				}
				StorePermission storePermission = new StorePermission(StorePermissionFlags.OpenStore | StorePermissionFlags.EnumerateCertificates);
				storePermission.Demand();
				X509ChainPolicy chainPolicy = ChainPolicy;
				if (chainPolicy.RevocationMode == X509RevocationMode.Online && (certificate.Extensions["2.5.29.31"] != null || certificate.Extensions["1.3.6.1.5.5.7.1.1"] != null))
				{
					PermissionSet permissionSet = new PermissionSet(PermissionState.None);
					permissionSet.AddPermission(new WebPermission(PermissionState.Unrestricted));
					permissionSet.AddPermission(new StorePermission(StorePermissionFlags.AddToStore));
					permissionSet.Demand();
				}
				Reset();
				if (BuildChain(m_useMachineContext ? new IntPtr(1L) : new IntPtr(0L), certificate.CertContext, chainPolicy.ExtraStore, chainPolicy.ApplicationPolicy, chainPolicy.CertificatePolicy, chainPolicy.RevocationMode, chainPolicy.RevocationFlag, chainPolicy.VerificationTime, chainPolicy.UrlRetrievalTimeout, ref m_safeCertChainHandle) != 0)
				{
					return false;
				}
				Init();
				CAPIBase.CERT_CHAIN_POLICY_PARA pPolicyPara = new CAPIBase.CERT_CHAIN_POLICY_PARA(Marshal.SizeOf(typeof(CAPIBase.CERT_CHAIN_POLICY_PARA)));
				CAPIBase.CERT_CHAIN_POLICY_STATUS pPolicyStatus = new CAPIBase.CERT_CHAIN_POLICY_STATUS(Marshal.SizeOf(typeof(CAPIBase.CERT_CHAIN_POLICY_STATUS)));
				pPolicyPara.dwFlags = (uint)chainPolicy.VerificationFlags;
				if (!CAPISafe.CertVerifyCertificateChainPolicy(new IntPtr(1L), m_safeCertChainHandle, ref pPolicyPara, ref pPolicyStatus))
				{
					throw new CryptographicException(Marshal.GetLastWin32Error());
				}
				CAPISafe.SetLastError(pPolicyStatus.dwError);
				return pPolicyStatus.dwError == 0;
			}
		}

		[PermissionSet(SecurityAction.LinkDemand, Unrestricted = true)]
		[PermissionSet(SecurityAction.InheritanceDemand, Unrestricted = true)]
		public void Reset()
		{
			m_status = 0u;
			m_chainStatus = null;
			m_chainElementCollection = new X509ChainElementCollection();
			if (!m_safeCertChainHandle.IsInvalid)
			{
				m_safeCertChainHandle.Dispose();
				m_safeCertChainHandle = SafeCertChainHandle.InvalidHandle;
			}
		}

		private unsafe void Init()
		{
			using SafeCertChainHandle safeCertChainHandle = CAPISafe.CertDuplicateCertificateChain(m_safeCertChainHandle);
			CAPIBase.CERT_CHAIN_CONTEXT cERT_CHAIN_CONTEXT = new CAPIBase.CERT_CHAIN_CONTEXT(Marshal.SizeOf(typeof(CAPIBase.CERT_CHAIN_CONTEXT)));
			uint num = (uint)Marshal.ReadInt32(safeCertChainHandle.DangerousGetHandle());
			if (num > Marshal.SizeOf(cERT_CHAIN_CONTEXT))
			{
				num = (uint)Marshal.SizeOf(cERT_CHAIN_CONTEXT);
			}
			X509Utils.memcpy(m_safeCertChainHandle.DangerousGetHandle(), new IntPtr(&cERT_CHAIN_CONTEXT), num);
			m_status = cERT_CHAIN_CONTEXT.dwErrorStatus;
			m_chainElementCollection = new X509ChainElementCollection(Marshal.ReadIntPtr(cERT_CHAIN_CONTEXT.rgpChain));
		}

		internal static X509ChainStatus[] GetChainStatusInformation(uint dwStatus)
		{
			if (dwStatus == 0)
			{
				return new X509ChainStatus[0];
			}
			int num = 0;
			for (uint num2 = dwStatus; num2 != 0; num2 >>= 1)
			{
				if ((num2 & (true ? 1u : 0u)) != 0)
				{
					num++;
				}
			}
			X509ChainStatus[] array = new X509ChainStatus[num];
			int num3 = 0;
			if ((dwStatus & 8u) != 0)
			{
				array[num3].StatusInformation = X509Utils.GetSystemErrorString(-2146869244);
				array[num3].Status = X509ChainStatusFlags.NotSignatureValid;
				num3++;
				dwStatus &= 0xFFFFFFF7u;
			}
			if ((dwStatus & 0x40000u) != 0)
			{
				array[num3].StatusInformation = X509Utils.GetSystemErrorString(-2146869244);
				array[num3].Status = X509ChainStatusFlags.CtlNotSignatureValid;
				num3++;
				dwStatus &= 0xFFFBFFFFu;
			}
			if ((dwStatus & 0x20u) != 0)
			{
				array[num3].StatusInformation = X509Utils.GetSystemErrorString(-2146762487);
				array[num3].Status = X509ChainStatusFlags.UntrustedRoot;
				num3++;
				dwStatus &= 0xFFFFFFDFu;
			}
			if ((dwStatus & 0x10000u) != 0)
			{
				array[num3].StatusInformation = X509Utils.GetSystemErrorString(-2146762486);
				array[num3].Status = X509ChainStatusFlags.PartialChain;
				num3++;
				dwStatus &= 0xFFFEFFFFu;
			}
			if ((dwStatus & 4u) != 0)
			{
				array[num3].StatusInformation = X509Utils.GetSystemErrorString(-2146885616);
				array[num3].Status = X509ChainStatusFlags.Revoked;
				num3++;
				dwStatus &= 0xFFFFFFFBu;
			}
			if ((dwStatus & 0x10u) != 0)
			{
				array[num3].StatusInformation = X509Utils.GetSystemErrorString(-2146762480);
				array[num3].Status = X509ChainStatusFlags.NotValidForUsage;
				num3++;
				dwStatus &= 0xFFFFFFEFu;
			}
			if ((dwStatus & 0x80000u) != 0)
			{
				array[num3].StatusInformation = X509Utils.GetSystemErrorString(-2146762480);
				array[num3].Status = X509ChainStatusFlags.CtlNotValidForUsage;
				num3++;
				dwStatus &= 0xFFF7FFFFu;
			}
			if ((dwStatus & (true ? 1u : 0u)) != 0)
			{
				array[num3].StatusInformation = X509Utils.GetSystemErrorString(-2146762495);
				array[num3].Status = X509ChainStatusFlags.NotTimeValid;
				num3++;
				dwStatus &= 0xFFFFFFFEu;
			}
			if ((dwStatus & 0x20000u) != 0)
			{
				array[num3].StatusInformation = X509Utils.GetSystemErrorString(-2146762495);
				array[num3].Status = X509ChainStatusFlags.CtlNotTimeValid;
				num3++;
				dwStatus &= 0xFFFDFFFFu;
			}
			if ((dwStatus & 0x800u) != 0)
			{
				array[num3].StatusInformation = X509Utils.GetSystemErrorString(-2146762476);
				array[num3].Status = X509ChainStatusFlags.InvalidNameConstraints;
				num3++;
				dwStatus &= 0xFFFFF7FFu;
			}
			if ((dwStatus & 0x1000u) != 0)
			{
				array[num3].StatusInformation = X509Utils.GetSystemErrorString(-2146762476);
				array[num3].Status = X509ChainStatusFlags.HasNotSupportedNameConstraint;
				num3++;
				dwStatus &= 0xFFFFEFFFu;
			}
			if ((dwStatus & 0x2000u) != 0)
			{
				array[num3].StatusInformation = X509Utils.GetSystemErrorString(-2146762476);
				array[num3].Status = X509ChainStatusFlags.HasNotDefinedNameConstraint;
				num3++;
				dwStatus &= 0xFFFFDFFFu;
			}
			if ((dwStatus & 0x4000u) != 0)
			{
				array[num3].StatusInformation = X509Utils.GetSystemErrorString(-2146762476);
				array[num3].Status = X509ChainStatusFlags.HasNotPermittedNameConstraint;
				num3++;
				dwStatus &= 0xFFFFBFFFu;
			}
			if ((dwStatus & 0x8000u) != 0)
			{
				array[num3].StatusInformation = X509Utils.GetSystemErrorString(-2146762476);
				array[num3].Status = X509ChainStatusFlags.HasExcludedNameConstraint;
				num3++;
				dwStatus &= 0xFFFF7FFFu;
			}
			if ((dwStatus & 0x200u) != 0)
			{
				array[num3].StatusInformation = X509Utils.GetSystemErrorString(-2146762477);
				array[num3].Status = X509ChainStatusFlags.InvalidPolicyConstraints;
				num3++;
				dwStatus &= 0xFFFFFDFFu;
			}
			if ((dwStatus & 0x2000000u) != 0)
			{
				array[num3].StatusInformation = X509Utils.GetSystemErrorString(-2146762477);
				array[num3].Status = X509ChainStatusFlags.NoIssuanceChainPolicy;
				num3++;
				dwStatus &= 0xFDFFFFFFu;
			}
			if ((dwStatus & 0x400u) != 0)
			{
				array[num3].StatusInformation = X509Utils.GetSystemErrorString(-2146869223);
				array[num3].Status = X509ChainStatusFlags.InvalidBasicConstraints;
				num3++;
				dwStatus &= 0xFFFFFBFFu;
			}
			if ((dwStatus & 2u) != 0)
			{
				array[num3].StatusInformation = X509Utils.GetSystemErrorString(-2146762494);
				array[num3].Status = X509ChainStatusFlags.NotTimeNested;
				num3++;
				dwStatus &= 0xFFFFFFFDu;
			}
			if ((dwStatus & 0x40u) != 0)
			{
				array[num3].StatusInformation = X509Utils.GetSystemErrorString(-2146885614);
				array[num3].Status = X509ChainStatusFlags.RevocationStatusUnknown;
				num3++;
				dwStatus &= 0xFFFFFFBFu;
			}
			if ((dwStatus & 0x1000000u) != 0)
			{
				array[num3].StatusInformation = X509Utils.GetSystemErrorString(-2146885613);
				array[num3].Status = X509ChainStatusFlags.OfflineRevocation;
				num3++;
				dwStatus &= 0xFEFFFFFFu;
			}
			int num4 = 0;
			for (uint num5 = dwStatus; num5 != 0; num5 >>= 1)
			{
				if ((num5 & (true ? 1u : 0u)) != 0)
				{
					array[num3].Status = (X509ChainStatusFlags)(1 << num4);
					array[num3].StatusInformation = SR.GetString("Unknown_Error");
					num3++;
				}
				num4++;
			}
			return array;
		}

		internal unsafe static int BuildChain(IntPtr hChainEngine, System.Security.Cryptography.SafeCertContextHandle pCertContext, X509Certificate2Collection extraStore, OidCollection applicationPolicy, OidCollection certificatePolicy, X509RevocationMode revocationMode, X509RevocationFlag revocationFlag, DateTime verificationTime, TimeSpan timeout, ref SafeCertChainHandle ppChainContext)
		{
			if (pCertContext == null || pCertContext.IsInvalid)
			{
				throw new ArgumentException(SR.GetString("Cryptography_InvalidContextHandle"), "pCertContext");
			}
			System.Security.Cryptography.SafeCertStoreHandle hAdditionalStore = System.Security.Cryptography.SafeCertStoreHandle.InvalidHandle;
			if (extraStore != null && extraStore.Count > 0)
			{
				hAdditionalStore = X509Utils.ExportToMemoryStore(extraStore);
			}
			CAPIBase.CERT_CHAIN_PARA pChainPara = default(CAPIBase.CERT_CHAIN_PARA);
			pChainPara.cbSize = (uint)Marshal.SizeOf(pChainPara);
			SafeLocalAllocHandle safeLocalAllocHandle = SafeLocalAllocHandle.InvalidHandle;
			if (applicationPolicy != null && applicationPolicy.Count > 0)
			{
				pChainPara.RequestedUsage.dwType = 0u;
				pChainPara.RequestedUsage.Usage.cUsageIdentifier = (uint)applicationPolicy.Count;
				safeLocalAllocHandle = X509Utils.CopyOidsToUnmanagedMemory(applicationPolicy);
				pChainPara.RequestedUsage.Usage.rgpszUsageIdentifier = safeLocalAllocHandle.DangerousGetHandle();
			}
			SafeLocalAllocHandle safeLocalAllocHandle2 = SafeLocalAllocHandle.InvalidHandle;
			if (certificatePolicy != null && certificatePolicy.Count > 0)
			{
				pChainPara.RequestedIssuancePolicy.dwType = 0u;
				pChainPara.RequestedIssuancePolicy.Usage.cUsageIdentifier = (uint)certificatePolicy.Count;
				safeLocalAllocHandle2 = X509Utils.CopyOidsToUnmanagedMemory(certificatePolicy);
				pChainPara.RequestedIssuancePolicy.Usage.rgpszUsageIdentifier = safeLocalAllocHandle2.DangerousGetHandle();
			}
			pChainPara.dwUrlRetrievalTimeout = (uint)timeout.Milliseconds;
			System.Runtime.InteropServices.ComTypes.FILETIME pTime = default(System.Runtime.InteropServices.ComTypes.FILETIME);
			*(long*)(&pTime) = verificationTime.ToFileTime();
			uint dwFlags = X509Utils.MapRevocationFlags(revocationMode, revocationFlag);
			if (!CAPISafe.CertGetCertificateChain(hChainEngine, pCertContext, ref pTime, hAdditionalStore, ref pChainPara, dwFlags, IntPtr.Zero, ref ppChainContext))
			{
				return Marshal.GetHRForLastWin32Error();
			}
			safeLocalAllocHandle.Dispose();
			safeLocalAllocHandle2.Dispose();
			return 0;
		}
	}
}
