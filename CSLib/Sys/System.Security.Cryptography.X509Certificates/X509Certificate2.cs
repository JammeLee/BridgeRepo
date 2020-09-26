using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Text;

namespace System.Security.Cryptography.X509Certificates
{
	public class X509Certificate2 : X509Certificate
	{
		private int m_version;

		private DateTime m_notBefore;

		private DateTime m_notAfter;

		private AsymmetricAlgorithm m_privateKey;

		private PublicKey m_publicKey;

		private X509ExtensionCollection m_extensions;

		private Oid m_signatureAlgorithm;

		private X500DistinguishedName m_subjectName;

		private X500DistinguishedName m_issuerName;

		private System.Security.Cryptography.SafeCertContextHandle m_safeCertContext = System.Security.Cryptography.SafeCertContextHandle.InvalidHandle;

		private bool m_randomKeyContainer;

		private static int s_publicKeyOffset;

		private static uint randomKeyContainerFlag = uint.MaxValue;

		public bool Archived
		{
			get
			{
				if (m_safeCertContext.IsInvalid)
				{
					throw new CryptographicException(SR.GetString("Cryptography_InvalidHandle"), "m_safeCertContext");
				}
				uint pcbData = 0u;
				return CAPISafe.CertGetCertificateContextProperty(m_safeCertContext, 19u, SafeLocalAllocHandle.InvalidHandle, ref pcbData);
			}
			set
			{
				SafeLocalAllocHandle safeLocalAllocHandle = SafeLocalAllocHandle.InvalidHandle;
				if (value)
				{
					safeLocalAllocHandle = CAPI.LocalAlloc(64u, new IntPtr(Marshal.SizeOf(typeof(CAPIBase.CRYPTOAPI_BLOB))));
				}
				if (!CAPI.CertSetCertificateContextProperty(m_safeCertContext, 19u, 0u, safeLocalAllocHandle))
				{
					throw new CryptographicException(Marshal.GetLastWin32Error());
				}
				safeLocalAllocHandle.Dispose();
			}
		}

		public X509ExtensionCollection Extensions
		{
			get
			{
				if (m_safeCertContext.IsInvalid)
				{
					throw new CryptographicException(SR.GetString("Cryptography_InvalidHandle"), "m_safeCertContext");
				}
				if (m_extensions == null)
				{
					m_extensions = new X509ExtensionCollection(m_safeCertContext);
				}
				return m_extensions;
			}
		}

		public string FriendlyName
		{
			get
			{
				if (m_safeCertContext.IsInvalid)
				{
					throw new CryptographicException(SR.GetString("Cryptography_InvalidHandle"), "m_safeCertContext");
				}
				SafeLocalAllocHandle invalidHandle = SafeLocalAllocHandle.InvalidHandle;
				uint pcbData = 0u;
				if (!CAPISafe.CertGetCertificateContextProperty(m_safeCertContext, 11u, invalidHandle, ref pcbData))
				{
					return string.Empty;
				}
				invalidHandle = CAPI.LocalAlloc(0u, new IntPtr(pcbData));
				if (!CAPISafe.CertGetCertificateContextProperty(m_safeCertContext, 11u, invalidHandle, ref pcbData))
				{
					return string.Empty;
				}
				string result = Marshal.PtrToStringUni(invalidHandle.DangerousGetHandle());
				invalidHandle.Dispose();
				return result;
			}
			set
			{
				if (m_safeCertContext.IsInvalid)
				{
					throw new CryptographicException(SR.GetString("Cryptography_InvalidHandle"), "m_safeCertContext");
				}
				if (value == null)
				{
					value = string.Empty;
				}
				SetFriendlyNameExtendedProperty(m_safeCertContext, value);
			}
		}

		public unsafe X500DistinguishedName IssuerName
		{
			get
			{
				if (m_safeCertContext.IsInvalid)
				{
					throw new CryptographicException(SR.GetString("Cryptography_InvalidHandle"), "m_safeCertContext");
				}
				if (m_issuerName == null)
				{
					CAPIBase.CERT_CONTEXT cERT_CONTEXT = *(CAPIBase.CERT_CONTEXT*)(void*)m_safeCertContext.DangerousGetHandle();
					m_issuerName = new X500DistinguishedName(((CAPIBase.CERT_INFO)Marshal.PtrToStructure(cERT_CONTEXT.pCertInfo, typeof(CAPIBase.CERT_INFO))).Issuer);
				}
				return m_issuerName;
			}
		}

		public unsafe DateTime NotAfter
		{
			get
			{
				if (m_safeCertContext.IsInvalid)
				{
					throw new CryptographicException(SR.GetString("Cryptography_InvalidHandle"), "m_safeCertContext");
				}
				if (m_notAfter == DateTime.MinValue)
				{
					CAPIBase.CERT_CONTEXT cERT_CONTEXT = *(CAPIBase.CERT_CONTEXT*)(void*)m_safeCertContext.DangerousGetHandle();
					CAPIBase.CERT_INFO cERT_INFO = (CAPIBase.CERT_INFO)Marshal.PtrToStructure(cERT_CONTEXT.pCertInfo, typeof(CAPIBase.CERT_INFO));
					long fileTime = (long)(((ulong)(uint)cERT_INFO.NotAfter.dwHighDateTime << 32) | (uint)cERT_INFO.NotAfter.dwLowDateTime);
					m_notAfter = DateTime.FromFileTime(fileTime);
				}
				return m_notAfter;
			}
		}

		public unsafe DateTime NotBefore
		{
			get
			{
				if (m_safeCertContext.IsInvalid)
				{
					throw new CryptographicException(SR.GetString("Cryptography_InvalidHandle"), "m_safeCertContext");
				}
				if (m_notBefore == DateTime.MinValue)
				{
					CAPIBase.CERT_CONTEXT cERT_CONTEXT = *(CAPIBase.CERT_CONTEXT*)(void*)m_safeCertContext.DangerousGetHandle();
					CAPIBase.CERT_INFO cERT_INFO = (CAPIBase.CERT_INFO)Marshal.PtrToStructure(cERT_CONTEXT.pCertInfo, typeof(CAPIBase.CERT_INFO));
					long fileTime = (long)(((ulong)(uint)cERT_INFO.NotBefore.dwHighDateTime << 32) | (uint)cERT_INFO.NotBefore.dwLowDateTime);
					m_notBefore = DateTime.FromFileTime(fileTime);
				}
				return m_notBefore;
			}
		}

		public bool HasPrivateKey
		{
			get
			{
				if (m_safeCertContext.IsInvalid)
				{
					throw new CryptographicException(SR.GetString("Cryptography_InvalidHandle"), "m_safeCertContext");
				}
				uint pcbData = 0u;
				return CAPISafe.CertGetCertificateContextProperty(m_safeCertContext, 2u, SafeLocalAllocHandle.InvalidHandle, ref pcbData);
			}
		}

		private static uint RandomKeyContainerFlag
		{
			get
			{
				if (randomKeyContainerFlag == uint.MaxValue)
				{
					FieldInfo field = typeof(RSACryptoServiceProvider).GetField("RandomKeyContainerFlag", BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.NonPublic);
					if (field != null)
					{
						randomKeyContainerFlag = (uint)field.GetValue(null);
					}
					else
					{
						randomKeyContainerFlag = 0u;
					}
				}
				return randomKeyContainerFlag;
			}
		}

		public AsymmetricAlgorithm PrivateKey
		{
			get
			{
				if (!HasPrivateKey)
				{
					return null;
				}
				if (m_privateKey == null)
				{
					CspParameters parameters = new CspParameters();
					if (!GetPrivateKeyInfo(m_safeCertContext, ref parameters))
					{
						return null;
					}
					parameters.Flags |= CspProviderFlags.UseExistingKey;
					switch (PublicKey.AlgorithmId)
					{
					case 9216u:
					case 41984u:
						if (m_randomKeyContainer)
						{
							KeyContainerPermission keyContainerPermission = new KeyContainerPermission(PermissionState.None);
							keyContainerPermission.AccessEntries.Add(new KeyContainerPermissionAccessEntry(parameters, KeyContainerPermissionFlags.Open));
							keyContainerPermission.Assert();
							parameters.Flags |= (CspProviderFlags)RandomKeyContainerFlag;
						}
						m_privateKey = new RSACryptoServiceProvider(parameters);
						if (m_randomKeyContainer)
						{
							CodeAccessPermission.RevertAssert();
						}
						break;
					case 8704u:
						m_privateKey = new DSACryptoServiceProvider(parameters);
						break;
					default:
						throw new NotSupportedException(SR.GetString("NotSupported_KeyAlgorithm"));
					}
				}
				return m_privateKey;
			}
			set
			{
				if (m_safeCertContext.IsInvalid)
				{
					throw new CryptographicException(SR.GetString("Cryptography_InvalidHandle"), "m_safeCertContext");
				}
				ICspAsymmetricAlgorithm cspAsymmetricAlgorithm = value as ICspAsymmetricAlgorithm;
				if (value != null && cspAsymmetricAlgorithm == null)
				{
					throw new NotSupportedException(SR.GetString("NotSupported_InvalidKeyImpl"));
				}
				if (cspAsymmetricAlgorithm != null)
				{
					if (cspAsymmetricAlgorithm.CspKeyContainerInfo == null)
					{
						throw new ArgumentException("CspKeyContainerInfo");
					}
					if (s_publicKeyOffset == 0)
					{
						s_publicKeyOffset = Marshal.SizeOf(typeof(CAPIBase.BLOBHEADER));
					}
					ICspAsymmetricAlgorithm cspAsymmetricAlgorithm2 = PublicKey.Key as ICspAsymmetricAlgorithm;
					byte[] array = cspAsymmetricAlgorithm2.ExportCspBlob(includePrivateParameters: false);
					byte[] array2 = cspAsymmetricAlgorithm.ExportCspBlob(includePrivateParameters: false);
					if (array == null || array2 == null || array.Length != array2.Length || array.Length <= s_publicKeyOffset)
					{
						throw new CryptographicUnexpectedOperationException(SR.GetString("Cryptography_X509_KeyMismatch"));
					}
					for (int i = s_publicKeyOffset; i < array.Length; i++)
					{
						if (array[i] != array2[i])
						{
							throw new CryptographicUnexpectedOperationException(SR.GetString("Cryptography_X509_KeyMismatch"));
						}
					}
				}
				SetPrivateKeyProperty(m_safeCertContext, cspAsymmetricAlgorithm);
				m_privateKey = value;
			}
		}

		public PublicKey PublicKey
		{
			get
			{
				if (m_safeCertContext.IsInvalid)
				{
					throw new CryptographicException(SR.GetString("Cryptography_InvalidHandle"), "m_safeCertContext");
				}
				if (m_publicKey == null)
				{
					string keyAlgorithm = GetKeyAlgorithm();
					byte[] keyAlgorithmParameters = GetKeyAlgorithmParameters();
					byte[] publicKey = GetPublicKey();
					Oid oid = new Oid(keyAlgorithm, System.Security.Cryptography.OidGroup.PublicKeyAlgorithm, lookupFriendlyName: true);
					m_publicKey = new PublicKey(oid, new AsnEncodedData(oid, keyAlgorithmParameters), new AsnEncodedData(oid, publicKey));
				}
				return m_publicKey;
			}
		}

		public byte[] RawData => GetRawCertData();

		public string SerialNumber => GetSerialNumberString();

		public unsafe X500DistinguishedName SubjectName
		{
			get
			{
				if (m_safeCertContext.IsInvalid)
				{
					throw new CryptographicException(SR.GetString("Cryptography_InvalidHandle"), "m_safeCertContext");
				}
				if (m_subjectName == null)
				{
					CAPIBase.CERT_CONTEXT cERT_CONTEXT = *(CAPIBase.CERT_CONTEXT*)(void*)m_safeCertContext.DangerousGetHandle();
					m_subjectName = new X500DistinguishedName(((CAPIBase.CERT_INFO)Marshal.PtrToStructure(cERT_CONTEXT.pCertInfo, typeof(CAPIBase.CERT_INFO))).Subject);
				}
				return m_subjectName;
			}
		}

		public Oid SignatureAlgorithm
		{
			get
			{
				if (m_safeCertContext.IsInvalid)
				{
					throw new CryptographicException(SR.GetString("Cryptography_InvalidHandle"), "m_safeCertContext");
				}
				if (m_signatureAlgorithm == null)
				{
					m_signatureAlgorithm = GetSignatureAlgorithm(m_safeCertContext);
				}
				return m_signatureAlgorithm;
			}
		}

		public string Thumbprint => GetCertHashString();

		public int Version
		{
			get
			{
				if (m_safeCertContext.IsInvalid)
				{
					throw new CryptographicException(SR.GetString("Cryptography_InvalidHandle"), "m_safeCertContext");
				}
				if (m_version == 0)
				{
					m_version = (int)GetVersion(m_safeCertContext);
				}
				return m_version;
			}
		}

		internal System.Security.Cryptography.SafeCertContextHandle CertContext => m_safeCertContext;

		public X509Certificate2()
		{
		}

		public X509Certificate2(byte[] rawData)
			: base(rawData)
		{
			m_safeCertContext = CAPI.CertDuplicateCertificateContext(base.Handle);
			m_randomKeyContainer = true;
		}

		public X509Certificate2(byte[] rawData, string password)
			: base(rawData, password)
		{
			m_safeCertContext = CAPI.CertDuplicateCertificateContext(base.Handle);
			m_randomKeyContainer = true;
		}

		public X509Certificate2(byte[] rawData, SecureString password)
			: base(rawData, password)
		{
			m_safeCertContext = CAPI.CertDuplicateCertificateContext(base.Handle);
			m_randomKeyContainer = true;
		}

		public X509Certificate2(byte[] rawData, string password, X509KeyStorageFlags keyStorageFlags)
			: base(rawData, password, keyStorageFlags)
		{
			m_safeCertContext = CAPI.CertDuplicateCertificateContext(base.Handle);
			m_randomKeyContainer = true;
		}

		public X509Certificate2(byte[] rawData, SecureString password, X509KeyStorageFlags keyStorageFlags)
			: base(rawData, password, keyStorageFlags)
		{
			m_safeCertContext = CAPI.CertDuplicateCertificateContext(base.Handle);
			m_randomKeyContainer = true;
		}

		public X509Certificate2(string fileName)
			: base(fileName)
		{
			m_safeCertContext = CAPI.CertDuplicateCertificateContext(base.Handle);
			m_randomKeyContainer = true;
		}

		public X509Certificate2(string fileName, string password)
			: base(fileName, password)
		{
			m_safeCertContext = CAPI.CertDuplicateCertificateContext(base.Handle);
			m_randomKeyContainer = true;
		}

		public X509Certificate2(string fileName, SecureString password)
			: base(fileName, password)
		{
			m_safeCertContext = CAPI.CertDuplicateCertificateContext(base.Handle);
			m_randomKeyContainer = true;
		}

		public X509Certificate2(string fileName, string password, X509KeyStorageFlags keyStorageFlags)
			: base(fileName, password, keyStorageFlags)
		{
			m_safeCertContext = CAPI.CertDuplicateCertificateContext(base.Handle);
			m_randomKeyContainer = true;
		}

		public X509Certificate2(string fileName, SecureString password, X509KeyStorageFlags keyStorageFlags)
			: base(fileName, password, keyStorageFlags)
		{
			m_safeCertContext = CAPI.CertDuplicateCertificateContext(base.Handle);
			m_randomKeyContainer = true;
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		[SecurityPermission(SecurityAction.InheritanceDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public X509Certificate2(IntPtr handle)
			: base(handle)
		{
			m_safeCertContext = CAPI.CertDuplicateCertificateContext(base.Handle);
		}

		public X509Certificate2(X509Certificate certificate)
			: base(certificate)
		{
			m_safeCertContext = CAPI.CertDuplicateCertificateContext(base.Handle);
			X509Certificate2 x509Certificate = certificate as X509Certificate2;
			if (x509Certificate != null)
			{
				m_randomKeyContainer = x509Certificate.m_randomKeyContainer;
			}
		}

		public override string ToString()
		{
			return base.ToString(fVerbose: true);
		}

		public override string ToString(bool verbose)
		{
			if (!verbose || m_safeCertContext.IsInvalid)
			{
				return ToString();
			}
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("[Version]" + Environment.NewLine + "  ");
			stringBuilder.Append("V" + Version);
			stringBuilder.Append(Environment.NewLine + Environment.NewLine + "[Subject]" + Environment.NewLine + "  ");
			stringBuilder.Append(SubjectName.Name);
			string nameInfo = GetNameInfo(X509NameType.SimpleName, forIssuer: false);
			if (nameInfo.Length > 0)
			{
				stringBuilder.Append(Environment.NewLine + "  Simple Name: ");
				stringBuilder.Append(nameInfo);
			}
			string nameInfo2 = GetNameInfo(X509NameType.EmailName, forIssuer: false);
			if (nameInfo2.Length > 0)
			{
				stringBuilder.Append(Environment.NewLine + "  Email Name: ");
				stringBuilder.Append(nameInfo2);
			}
			string nameInfo3 = GetNameInfo(X509NameType.UpnName, forIssuer: false);
			if (nameInfo3.Length > 0)
			{
				stringBuilder.Append(Environment.NewLine + "  UPN Name: ");
				stringBuilder.Append(nameInfo3);
			}
			string nameInfo4 = GetNameInfo(X509NameType.DnsName, forIssuer: false);
			if (nameInfo4.Length > 0)
			{
				stringBuilder.Append(Environment.NewLine + "  DNS Name: ");
				stringBuilder.Append(nameInfo4);
			}
			stringBuilder.Append(Environment.NewLine + Environment.NewLine + "[Issuer]" + Environment.NewLine + "  ");
			stringBuilder.Append(IssuerName.Name);
			nameInfo = GetNameInfo(X509NameType.SimpleName, forIssuer: true);
			if (nameInfo.Length > 0)
			{
				stringBuilder.Append(Environment.NewLine + "  Simple Name: ");
				stringBuilder.Append(nameInfo);
			}
			nameInfo2 = GetNameInfo(X509NameType.EmailName, forIssuer: true);
			if (nameInfo2.Length > 0)
			{
				stringBuilder.Append(Environment.NewLine + "  Email Name: ");
				stringBuilder.Append(nameInfo2);
			}
			nameInfo3 = GetNameInfo(X509NameType.UpnName, forIssuer: true);
			if (nameInfo3.Length > 0)
			{
				stringBuilder.Append(Environment.NewLine + "  UPN Name: ");
				stringBuilder.Append(nameInfo3);
			}
			nameInfo4 = GetNameInfo(X509NameType.DnsName, forIssuer: true);
			if (nameInfo4.Length > 0)
			{
				stringBuilder.Append(Environment.NewLine + "  DNS Name: ");
				stringBuilder.Append(nameInfo4);
			}
			stringBuilder.Append(Environment.NewLine + Environment.NewLine + "[Serial Number]" + Environment.NewLine + "  ");
			stringBuilder.Append(SerialNumber);
			stringBuilder.Append(Environment.NewLine + Environment.NewLine + "[Not Before]" + Environment.NewLine + "  ");
			stringBuilder.Append(NotBefore);
			stringBuilder.Append(Environment.NewLine + Environment.NewLine + "[Not After]" + Environment.NewLine + "  ");
			stringBuilder.Append(NotAfter);
			stringBuilder.Append(Environment.NewLine + Environment.NewLine + "[Thumbprint]" + Environment.NewLine + "  ");
			stringBuilder.Append(Thumbprint);
			stringBuilder.Append(Environment.NewLine + Environment.NewLine + "[Signature Algorithm]" + Environment.NewLine + "  ");
			stringBuilder.Append(SignatureAlgorithm.FriendlyName + "(" + SignatureAlgorithm.Value + ")");
			PublicKey publicKey = PublicKey;
			stringBuilder.Append(Environment.NewLine + Environment.NewLine + "[Public Key]" + Environment.NewLine + "  Algorithm: ");
			stringBuilder.Append(publicKey.Oid.FriendlyName);
			stringBuilder.Append(Environment.NewLine + "  Length: ");
			stringBuilder.Append(publicKey.Key.KeySize);
			stringBuilder.Append(Environment.NewLine + "  Key Blob: ");
			stringBuilder.Append(publicKey.EncodedKeyValue.Format(multiLine: true));
			stringBuilder.Append(Environment.NewLine + "  Parameters: ");
			stringBuilder.Append(publicKey.EncodedParameters.Format(multiLine: true));
			AppendPrivateKeyInfo(stringBuilder);
			X509ExtensionCollection extensions = Extensions;
			if (extensions.Count > 0)
			{
				stringBuilder.Append(Environment.NewLine + Environment.NewLine + "[Extensions]");
				X509ExtensionEnumerator enumerator = extensions.GetEnumerator();
				while (enumerator.MoveNext())
				{
					X509Extension current = enumerator.Current;
					stringBuilder.Append(Environment.NewLine + "* " + current.Oid.FriendlyName + "(" + current.Oid.Value + "):" + Environment.NewLine + "  " + current.Format(multiLine: true));
				}
			}
			stringBuilder.Append(Environment.NewLine);
			return stringBuilder.ToString();
		}

		public unsafe string GetNameInfo(X509NameType nameType, bool forIssuer)
		{
			uint dwFlags = (forIssuer ? 1u : 0u);
			uint num = X509Utils.MapNameType(nameType);
			switch (num)
			{
			case 4u:
				return CAPI.GetCertNameInfo(m_safeCertContext, dwFlags, num);
			case 1u:
				return CAPI.GetCertNameInfo(m_safeCertContext, dwFlags, num);
			default:
			{
				string text = string.Empty;
				CAPIBase.CERT_CONTEXT cERT_CONTEXT = *(CAPIBase.CERT_CONTEXT*)(void*)m_safeCertContext.DangerousGetHandle();
				CAPIBase.CERT_INFO cERT_INFO = (CAPIBase.CERT_INFO)Marshal.PtrToStructure(cERT_CONTEXT.pCertInfo, typeof(CAPIBase.CERT_INFO));
				IntPtr[] array = new IntPtr[2]
				{
					CAPISafe.CertFindExtension(forIssuer ? "2.5.29.8" : "2.5.29.7", cERT_INFO.cExtension, cERT_INFO.rgExtension),
					CAPISafe.CertFindExtension(forIssuer ? "2.5.29.18" : "2.5.29.17", cERT_INFO.cExtension, cERT_INFO.rgExtension)
				};
				for (int i = 0; i < array.Length; i++)
				{
					if (!(array[i] != IntPtr.Zero))
					{
						continue;
					}
					CAPIBase.CERT_EXTENSION cERT_EXTENSION = (CAPIBase.CERT_EXTENSION)Marshal.PtrToStructure(array[i], typeof(CAPIBase.CERT_EXTENSION));
					byte[] array2 = new byte[cERT_EXTENSION.Value.cbData];
					Marshal.Copy(cERT_EXTENSION.Value.pbData, array2, 0, array2.Length);
					uint cbDecodedValue = 0u;
					SafeLocalAllocHandle decodedValue = null;
					SafeLocalAllocHandle safeLocalAllocHandle = X509Utils.StringToAnsiPtr(cERT_EXTENSION.pszObjId);
					bool flag = CAPI.DecodeObject(safeLocalAllocHandle.DangerousGetHandle(), array2, out decodedValue, out cbDecodedValue);
					safeLocalAllocHandle.Dispose();
					if (!flag)
					{
						continue;
					}
					CAPIBase.CERT_ALT_NAME_INFO cERT_ALT_NAME_INFO = (CAPIBase.CERT_ALT_NAME_INFO)Marshal.PtrToStructure(decodedValue.DangerousGetHandle(), typeof(CAPIBase.CERT_ALT_NAME_INFO));
					for (int j = 0; j < cERT_ALT_NAME_INFO.cAltEntry; j++)
					{
						IntPtr ptr = new IntPtr((long)cERT_ALT_NAME_INFO.rgAltEntry + j * Marshal.SizeOf(typeof(CAPIBase.CERT_ALT_NAME_ENTRY)));
						CAPIBase.CERT_ALT_NAME_ENTRY cERT_ALT_NAME_ENTRY = (CAPIBase.CERT_ALT_NAME_ENTRY)Marshal.PtrToStructure(ptr, typeof(CAPIBase.CERT_ALT_NAME_ENTRY));
						switch (num)
						{
						case 8u:
						{
							if (cERT_ALT_NAME_ENTRY.dwAltNameChoice != 1)
							{
								break;
							}
							CAPIBase.CERT_OTHER_NAME cERT_OTHER_NAME = (CAPIBase.CERT_OTHER_NAME)Marshal.PtrToStructure(cERT_ALT_NAME_ENTRY.Value.pOtherName, typeof(CAPIBase.CERT_OTHER_NAME));
							if (!(cERT_OTHER_NAME.pszObjId == "1.3.6.1.4.1.311.20.2.3"))
							{
								break;
							}
							uint cbDecodedValue2 = 0u;
							SafeLocalAllocHandle decodedValue2 = null;
							if (CAPI.DecodeObject(new IntPtr(24L), X509Utils.PtrToByte(cERT_OTHER_NAME.Value.pbData, cERT_OTHER_NAME.Value.cbData), out decodedValue2, out cbDecodedValue2))
							{
								CAPIBase.CERT_NAME_VALUE cERT_NAME_VALUE = (CAPIBase.CERT_NAME_VALUE)Marshal.PtrToStructure(decodedValue2.DangerousGetHandle(), typeof(CAPIBase.CERT_NAME_VALUE));
								if (X509Utils.IsCertRdnCharString(cERT_NAME_VALUE.dwValueType))
								{
									text = Marshal.PtrToStringUni(cERT_NAME_VALUE.Value.pbData);
								}
								decodedValue2.Dispose();
							}
							break;
						}
						case 6u:
							if (cERT_ALT_NAME_ENTRY.dwAltNameChoice == 3)
							{
								text = Marshal.PtrToStringUni(cERT_ALT_NAME_ENTRY.Value.pwszDNSName);
							}
							break;
						case 7u:
							if (cERT_ALT_NAME_ENTRY.dwAltNameChoice == 7)
							{
								text = Marshal.PtrToStringUni(cERT_ALT_NAME_ENTRY.Value.pwszURL);
							}
							break;
						}
					}
					decodedValue.Dispose();
				}
				if (nameType == X509NameType.DnsName && (text == null || text.Length == 0))
				{
					text = CAPI.GetCertNameInfo(m_safeCertContext, dwFlags, 3u);
				}
				return text;
			}
			}
		}

		[PermissionSet(SecurityAction.InheritanceDemand, Unrestricted = true)]
		[PermissionSet(SecurityAction.LinkDemand, Unrestricted = true)]
		public override void Import(byte[] rawData)
		{
			Reset();
			base.Import(rawData);
			m_safeCertContext = CAPI.CertDuplicateCertificateContext(base.Handle);
		}

		[PermissionSet(SecurityAction.LinkDemand, Unrestricted = true)]
		[PermissionSet(SecurityAction.InheritanceDemand, Unrestricted = true)]
		public override void Import(byte[] rawData, string password, X509KeyStorageFlags keyStorageFlags)
		{
			Reset();
			base.Import(rawData, password, keyStorageFlags);
			m_safeCertContext = CAPI.CertDuplicateCertificateContext(base.Handle);
		}

		[PermissionSet(SecurityAction.InheritanceDemand, Unrestricted = true)]
		[PermissionSet(SecurityAction.LinkDemand, Unrestricted = true)]
		public override void Import(byte[] rawData, SecureString password, X509KeyStorageFlags keyStorageFlags)
		{
			Reset();
			base.Import(rawData, password, keyStorageFlags);
			m_safeCertContext = CAPI.CertDuplicateCertificateContext(base.Handle);
		}

		[PermissionSet(SecurityAction.LinkDemand, Unrestricted = true)]
		[PermissionSet(SecurityAction.InheritanceDemand, Unrestricted = true)]
		public override void Import(string fileName)
		{
			Reset();
			base.Import(fileName);
			m_safeCertContext = CAPI.CertDuplicateCertificateContext(base.Handle);
		}

		[PermissionSet(SecurityAction.LinkDemand, Unrestricted = true)]
		[PermissionSet(SecurityAction.InheritanceDemand, Unrestricted = true)]
		public override void Import(string fileName, string password, X509KeyStorageFlags keyStorageFlags)
		{
			Reset();
			base.Import(fileName, password, keyStorageFlags);
			m_safeCertContext = CAPI.CertDuplicateCertificateContext(base.Handle);
		}

		[PermissionSet(SecurityAction.InheritanceDemand, Unrestricted = true)]
		[PermissionSet(SecurityAction.LinkDemand, Unrestricted = true)]
		public override void Import(string fileName, SecureString password, X509KeyStorageFlags keyStorageFlags)
		{
			Reset();
			base.Import(fileName, password, keyStorageFlags);
			m_safeCertContext = CAPI.CertDuplicateCertificateContext(base.Handle);
		}

		[PermissionSet(SecurityAction.LinkDemand, Unrestricted = true)]
		[PermissionSet(SecurityAction.InheritanceDemand, Unrestricted = true)]
		public override void Reset()
		{
			m_version = 0;
			m_notBefore = DateTime.MinValue;
			m_notAfter = DateTime.MinValue;
			m_privateKey = null;
			m_publicKey = null;
			m_extensions = null;
			m_signatureAlgorithm = null;
			m_subjectName = null;
			m_issuerName = null;
			if (!m_safeCertContext.IsInvalid)
			{
				m_safeCertContext.Dispose();
				m_safeCertContext = System.Security.Cryptography.SafeCertContextHandle.InvalidHandle;
			}
			base.Reset();
		}

		public bool Verify()
		{
			if (m_safeCertContext.IsInvalid)
			{
				throw new CryptographicException(SR.GetString("Cryptography_InvalidHandle"), "m_safeCertContext");
			}
			int num = X509Utils.VerifyCertificate(CertContext, null, null, X509RevocationMode.Online, X509RevocationFlag.ExcludeRoot, DateTime.Now, new TimeSpan(0, 0, 0), null, new IntPtr(1L), IntPtr.Zero);
			return num == 0;
		}

		public static X509ContentType GetCertContentType(byte[] rawData)
		{
			if (rawData == null || rawData.Length == 0)
			{
				throw new ArgumentException(SR.GetString("Arg_EmptyOrNullArray"), "rawData");
			}
			uint contentType = QueryCertBlobType(rawData);
			return X509Utils.MapContentType(contentType);
		}

		public static X509ContentType GetCertContentType(string fileName)
		{
			if (fileName == null)
			{
				throw new ArgumentNullException("fileName");
			}
			string fullPath = Path.GetFullPath(fileName);
			new FileIOPermission(FileIOPermissionAccess.Read, fullPath).Demand();
			uint contentType = QueryCertFileType(fileName);
			return X509Utils.MapContentType(contentType);
		}

		internal static bool GetPrivateKeyInfo(System.Security.Cryptography.SafeCertContextHandle safeCertContext, ref CspParameters parameters)
		{
			SafeLocalAllocHandle invalidHandle = SafeLocalAllocHandle.InvalidHandle;
			uint pcbData = 0u;
			if (!CAPISafe.CertGetCertificateContextProperty(safeCertContext, 2u, invalidHandle, ref pcbData))
			{
				int lastWin32Error = Marshal.GetLastWin32Error();
				if (lastWin32Error == -2146885628)
				{
					return false;
				}
				throw new CryptographicException(Marshal.GetLastWin32Error());
			}
			invalidHandle = CAPI.LocalAlloc(0u, new IntPtr(pcbData));
			if (!CAPISafe.CertGetCertificateContextProperty(safeCertContext, 2u, invalidHandle, ref pcbData))
			{
				int lastWin32Error2 = Marshal.GetLastWin32Error();
				if (lastWin32Error2 == -2146885628)
				{
					return false;
				}
				throw new CryptographicException(Marshal.GetLastWin32Error());
			}
			CAPIBase.CRYPT_KEY_PROV_INFO cRYPT_KEY_PROV_INFO = (CAPIBase.CRYPT_KEY_PROV_INFO)Marshal.PtrToStructure(invalidHandle.DangerousGetHandle(), typeof(CAPIBase.CRYPT_KEY_PROV_INFO));
			parameters.ProviderName = cRYPT_KEY_PROV_INFO.pwszProvName;
			parameters.KeyContainerName = cRYPT_KEY_PROV_INFO.pwszContainerName;
			parameters.ProviderType = (int)cRYPT_KEY_PROV_INFO.dwProvType;
			parameters.KeyNumber = (int)cRYPT_KEY_PROV_INFO.dwKeySpec;
			parameters.Flags = (((cRYPT_KEY_PROV_INFO.dwFlags & 0x20) == 32) ? CspProviderFlags.UseMachineKeyStore : CspProviderFlags.NoFlags);
			invalidHandle.Dispose();
			return true;
		}

		private void AppendPrivateKeyInfo(StringBuilder sb)
		{
			CspKeyContainerInfo cspKeyContainerInfo = null;
			try
			{
				if (HasPrivateKey)
				{
					CspParameters parameters = new CspParameters();
					if (GetPrivateKeyInfo(m_safeCertContext, ref parameters))
					{
						cspKeyContainerInfo = new CspKeyContainerInfo(parameters);
					}
				}
			}
			catch (SecurityException)
			{
			}
			catch (CryptographicException)
			{
			}
			if (cspKeyContainerInfo != null)
			{
				sb.Append(Environment.NewLine + Environment.NewLine + "[Private Key]");
				sb.Append(Environment.NewLine + "  Key Store: ");
				sb.Append(cspKeyContainerInfo.MachineKeyStore ? "Machine" : "User");
				sb.Append(Environment.NewLine + "  Provider Name: ");
				sb.Append(cspKeyContainerInfo.ProviderName);
				sb.Append(Environment.NewLine + "  Provider type: ");
				sb.Append(cspKeyContainerInfo.ProviderType);
				sb.Append(Environment.NewLine + "  Key Spec: ");
				sb.Append(cspKeyContainerInfo.KeyNumber);
				sb.Append(Environment.NewLine + "  Key Container Name: ");
				sb.Append(cspKeyContainerInfo.KeyContainerName);
				try
				{
					string uniqueKeyContainerName = cspKeyContainerInfo.UniqueKeyContainerName;
					sb.Append(Environment.NewLine + "  Unique Key Container Name: ");
					sb.Append(uniqueKeyContainerName);
				}
				catch (CryptographicException)
				{
				}
				catch (NotSupportedException)
				{
				}
				bool flag = false;
				try
				{
					flag = cspKeyContainerInfo.HardwareDevice;
					sb.Append(Environment.NewLine + "  Hardware Device: ");
					sb.Append(flag);
				}
				catch (CryptographicException)
				{
				}
				try
				{
					flag = cspKeyContainerInfo.Removable;
					sb.Append(Environment.NewLine + "  Removable: ");
					sb.Append(flag);
				}
				catch (CryptographicException)
				{
				}
				try
				{
					flag = cspKeyContainerInfo.Protected;
					sb.Append(Environment.NewLine + "  Protected: ");
					sb.Append(flag);
				}
				catch (CryptographicException)
				{
				}
				catch (NotSupportedException)
				{
				}
			}
		}

		private unsafe static Oid GetSignatureAlgorithm(System.Security.Cryptography.SafeCertContextHandle safeCertContextHandle)
		{
			CAPIBase.CERT_CONTEXT cERT_CONTEXT = *(CAPIBase.CERT_CONTEXT*)(void*)safeCertContextHandle.DangerousGetHandle();
			return new Oid(((CAPIBase.CERT_INFO)Marshal.PtrToStructure(cERT_CONTEXT.pCertInfo, typeof(CAPIBase.CERT_INFO))).SignatureAlgorithm.pszObjId, System.Security.Cryptography.OidGroup.SignatureAlgorithm, lookupFriendlyName: false);
		}

		private unsafe static uint GetVersion(System.Security.Cryptography.SafeCertContextHandle safeCertContextHandle)
		{
			CAPIBase.CERT_CONTEXT cERT_CONTEXT = *(CAPIBase.CERT_CONTEXT*)(void*)safeCertContextHandle.DangerousGetHandle();
			return ((CAPIBase.CERT_INFO)Marshal.PtrToStructure(cERT_CONTEXT.pCertInfo, typeof(CAPIBase.CERT_INFO))).dwVersion + 1;
		}

		private unsafe static uint QueryCertBlobType(byte[] rawData)
		{
			uint result = 0u;
			if (!CAPI.CryptQueryObject(2u, rawData, 16382u, 14u, 0u, IntPtr.Zero, new IntPtr(&result), IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero))
			{
				throw new CryptographicException(Marshal.GetLastWin32Error());
			}
			return result;
		}

		private unsafe static uint QueryCertFileType(string fileName)
		{
			uint result = 0u;
			if (!CAPI.CryptQueryObject(1u, fileName, 16382u, 14u, 0u, IntPtr.Zero, new IntPtr(&result), IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero))
			{
				throw new CryptographicException(Marshal.GetLastWin32Error());
			}
			return result;
		}

		private unsafe static void SetFriendlyNameExtendedProperty(System.Security.Cryptography.SafeCertContextHandle safeCertContextHandle, string name)
		{
			SafeLocalAllocHandle safeLocalAllocHandle = X509Utils.StringToUniPtr(name);
			using (safeLocalAllocHandle)
			{
				CAPIBase.CRYPTOAPI_BLOB cRYPTOAPI_BLOB = default(CAPIBase.CRYPTOAPI_BLOB);
				cRYPTOAPI_BLOB.cbData = (uint)(2 * (name.Length + 1));
				cRYPTOAPI_BLOB.pbData = safeLocalAllocHandle.DangerousGetHandle();
				if (!CAPI.CertSetCertificateContextProperty(safeCertContextHandle, 11u, 0u, new IntPtr(&cRYPTOAPI_BLOB)))
				{
					throw new CryptographicException(Marshal.GetLastWin32Error());
				}
			}
		}

		private static void SetPrivateKeyProperty(System.Security.Cryptography.SafeCertContextHandle safeCertContextHandle, ICspAsymmetricAlgorithm asymmetricAlgorithm)
		{
			SafeLocalAllocHandle safeLocalAllocHandle = SafeLocalAllocHandle.InvalidHandle;
			if (asymmetricAlgorithm != null)
			{
				CAPIBase.CRYPT_KEY_PROV_INFO cRYPT_KEY_PROV_INFO = default(CAPIBase.CRYPT_KEY_PROV_INFO);
				cRYPT_KEY_PROV_INFO.pwszContainerName = asymmetricAlgorithm.CspKeyContainerInfo.KeyContainerName;
				cRYPT_KEY_PROV_INFO.pwszProvName = asymmetricAlgorithm.CspKeyContainerInfo.ProviderName;
				cRYPT_KEY_PROV_INFO.dwProvType = (uint)asymmetricAlgorithm.CspKeyContainerInfo.ProviderType;
				cRYPT_KEY_PROV_INFO.dwFlags = (asymmetricAlgorithm.CspKeyContainerInfo.MachineKeyStore ? 32u : 0u);
				cRYPT_KEY_PROV_INFO.cProvParam = 0u;
				cRYPT_KEY_PROV_INFO.rgProvParam = IntPtr.Zero;
				cRYPT_KEY_PROV_INFO.dwKeySpec = (uint)asymmetricAlgorithm.CspKeyContainerInfo.KeyNumber;
				safeLocalAllocHandle = CAPI.LocalAlloc(64u, new IntPtr(Marshal.SizeOf(typeof(CAPIBase.CRYPT_KEY_PROV_INFO))));
				Marshal.StructureToPtr(cRYPT_KEY_PROV_INFO, safeLocalAllocHandle.DangerousGetHandle(), fDeleteOld: false);
			}
			try
			{
				if (!CAPI.CertSetCertificateContextProperty(safeCertContextHandle, 2u, 0u, safeLocalAllocHandle))
				{
					throw new CryptographicException(Marshal.GetLastWin32Error());
				}
			}
			finally
			{
				if (!safeLocalAllocHandle.IsInvalid)
				{
					Marshal.DestroyStructure(safeLocalAllocHandle.DangerousGetHandle(), typeof(CAPIBase.CRYPT_KEY_PROV_INFO));
					safeLocalAllocHandle.Dispose();
				}
			}
		}
	}
}
