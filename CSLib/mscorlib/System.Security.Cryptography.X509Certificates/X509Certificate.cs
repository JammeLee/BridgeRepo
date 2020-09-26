using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Security.Util;
using System.Text;
using Microsoft.Win32;

namespace System.Security.Cryptography.X509Certificates
{
	[Serializable]
	[ComVisible(true)]
	public class X509Certificate : IDeserializationCallback, ISerializable
	{
		private const string m_format = "X509";

		private string m_subjectName;

		private string m_issuerName;

		private byte[] m_serialNumber;

		private byte[] m_publicKeyParameters;

		private byte[] m_publicKeyValue;

		private string m_publicKeyOid;

		private byte[] m_rawData;

		private byte[] m_thumbprint;

		private DateTime m_notBefore;

		private DateTime m_notAfter;

		private SafeCertContextHandle m_safeCertContext = SafeCertContextHandle.InvalidHandle;

		[ComVisible(false)]
		public IntPtr Handle
		{
			[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
			[SecurityPermission(SecurityAction.InheritanceDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
			get
			{
				return m_safeCertContext.pCertContext;
			}
		}

		public string Issuer
		{
			get
			{
				if (m_safeCertContext.IsInvalid)
				{
					throw new CryptographicException(Environment.GetResourceString("Cryptography_InvalidHandle"), "m_safeCertContext");
				}
				if (m_issuerName == null)
				{
					m_issuerName = X509Utils._GetIssuerName(m_safeCertContext, legacyV1Mode: false);
				}
				return m_issuerName;
			}
		}

		public string Subject
		{
			get
			{
				if (m_safeCertContext.IsInvalid)
				{
					throw new CryptographicException(Environment.GetResourceString("Cryptography_InvalidHandle"), "m_safeCertContext");
				}
				if (m_subjectName == null)
				{
					m_subjectName = X509Utils._GetSubjectInfo(m_safeCertContext, 2u, legacyV1Mode: false);
				}
				return m_subjectName;
			}
		}

		internal SafeCertContextHandle CertContext => m_safeCertContext;

		private DateTime NotAfter
		{
			get
			{
				if (m_safeCertContext.IsInvalid)
				{
					throw new CryptographicException(Environment.GetResourceString("Cryptography_InvalidHandle"), "m_safeCertContext");
				}
				if (m_notAfter == DateTime.MinValue)
				{
					Win32Native.FILE_TIME fileTime = default(Win32Native.FILE_TIME);
					X509Utils._GetDateNotAfter(m_safeCertContext, ref fileTime);
					m_notAfter = DateTime.FromFileTime(fileTime.ToTicks());
				}
				return m_notAfter;
			}
		}

		private DateTime NotBefore
		{
			get
			{
				if (m_safeCertContext.IsInvalid)
				{
					throw new CryptographicException(Environment.GetResourceString("Cryptography_InvalidHandle"), "m_safeCertContext");
				}
				if (m_notBefore == DateTime.MinValue)
				{
					Win32Native.FILE_TIME fileTime = default(Win32Native.FILE_TIME);
					X509Utils._GetDateNotBefore(m_safeCertContext, ref fileTime);
					m_notBefore = DateTime.FromFileTime(fileTime.ToTicks());
				}
				return m_notBefore;
			}
		}

		private byte[] RawData
		{
			get
			{
				if (m_safeCertContext.IsInvalid)
				{
					throw new CryptographicException(Environment.GetResourceString("Cryptography_InvalidHandle"), "m_safeCertContext");
				}
				if (m_rawData == null)
				{
					m_rawData = X509Utils._GetCertRawData(m_safeCertContext);
				}
				return (byte[])m_rawData.Clone();
			}
		}

		private string SerialNumber
		{
			get
			{
				if (m_safeCertContext.IsInvalid)
				{
					throw new CryptographicException(Environment.GetResourceString("Cryptography_InvalidHandle"), "m_safeCertContext");
				}
				if (m_serialNumber == null)
				{
					m_serialNumber = X509Utils._GetSerialNumber(m_safeCertContext);
				}
				return Hex.EncodeHexStringFromInt(m_serialNumber);
			}
		}

		public X509Certificate()
		{
		}

		public X509Certificate(byte[] data)
		{
			if (data != null && data.Length != 0)
			{
				LoadCertificateFromBlob(data, null, X509KeyStorageFlags.DefaultKeySet);
			}
		}

		public X509Certificate(byte[] rawData, string password)
		{
			LoadCertificateFromBlob(rawData, password, X509KeyStorageFlags.DefaultKeySet);
		}

		public X509Certificate(byte[] rawData, SecureString password)
		{
			LoadCertificateFromBlob(rawData, password, X509KeyStorageFlags.DefaultKeySet);
		}

		public X509Certificate(byte[] rawData, string password, X509KeyStorageFlags keyStorageFlags)
		{
			LoadCertificateFromBlob(rawData, password, keyStorageFlags);
		}

		public X509Certificate(byte[] rawData, SecureString password, X509KeyStorageFlags keyStorageFlags)
		{
			LoadCertificateFromBlob(rawData, password, keyStorageFlags);
		}

		public X509Certificate(string fileName)
		{
			LoadCertificateFromFile(fileName, null, X509KeyStorageFlags.DefaultKeySet);
		}

		public X509Certificate(string fileName, string password)
		{
			LoadCertificateFromFile(fileName, password, X509KeyStorageFlags.DefaultKeySet);
		}

		public X509Certificate(string fileName, SecureString password)
		{
			LoadCertificateFromFile(fileName, password, X509KeyStorageFlags.DefaultKeySet);
		}

		public X509Certificate(string fileName, string password, X509KeyStorageFlags keyStorageFlags)
		{
			LoadCertificateFromFile(fileName, password, keyStorageFlags);
		}

		public X509Certificate(string fileName, SecureString password, X509KeyStorageFlags keyStorageFlags)
		{
			LoadCertificateFromFile(fileName, password, keyStorageFlags);
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		[SecurityPermission(SecurityAction.InheritanceDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public X509Certificate(IntPtr handle)
		{
			if (handle == IntPtr.Zero)
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_InvalidHandle"), "handle");
			}
			X509Utils._DuplicateCertContext(handle, ref m_safeCertContext);
		}

		public X509Certificate(X509Certificate cert)
		{
			if (cert == null)
			{
				throw new ArgumentNullException("cert");
			}
			if (cert.m_safeCertContext.pCertContext != IntPtr.Zero)
			{
				X509Utils._DuplicateCertContext(cert.m_safeCertContext.pCertContext, ref m_safeCertContext);
			}
			GC.KeepAlive(cert.m_safeCertContext);
		}

		public X509Certificate(SerializationInfo info, StreamingContext context)
		{
			byte[] array = (byte[])info.GetValue("RawData", typeof(byte[]));
			if (array != null)
			{
				LoadCertificateFromBlob(array, null, X509KeyStorageFlags.DefaultKeySet);
			}
		}

		public static X509Certificate CreateFromCertFile(string filename)
		{
			return new X509Certificate(filename);
		}

		public static X509Certificate CreateFromSignedFile(string filename)
		{
			return new X509Certificate(filename);
		}

		[Obsolete("This method has been deprecated.  Please use the Subject property instead.  http://go.microsoft.com/fwlink/?linkid=14202")]
		public virtual string GetName()
		{
			if (m_safeCertContext.IsInvalid)
			{
				throw new CryptographicException(Environment.GetResourceString("Cryptography_InvalidHandle"), "m_safeCertContext");
			}
			return X509Utils._GetSubjectInfo(m_safeCertContext, 2u, legacyV1Mode: true);
		}

		[Obsolete("This method has been deprecated.  Please use the Issuer property instead.  http://go.microsoft.com/fwlink/?linkid=14202")]
		public virtual string GetIssuerName()
		{
			if (m_safeCertContext.IsInvalid)
			{
				throw new CryptographicException(Environment.GetResourceString("Cryptography_InvalidHandle"), "m_safeCertContext");
			}
			return X509Utils._GetIssuerName(m_safeCertContext, legacyV1Mode: true);
		}

		public virtual byte[] GetSerialNumber()
		{
			if (m_safeCertContext.IsInvalid)
			{
				throw new CryptographicException(Environment.GetResourceString("Cryptography_InvalidHandle"), "m_safeCertContext");
			}
			if (m_serialNumber == null)
			{
				m_serialNumber = X509Utils._GetSerialNumber(m_safeCertContext);
			}
			return (byte[])m_serialNumber.Clone();
		}

		public virtual string GetSerialNumberString()
		{
			return SerialNumber;
		}

		public virtual byte[] GetKeyAlgorithmParameters()
		{
			if (m_safeCertContext.IsInvalid)
			{
				throw new CryptographicException(Environment.GetResourceString("Cryptography_InvalidHandle"), "m_safeCertContext");
			}
			if (m_publicKeyParameters == null)
			{
				m_publicKeyParameters = X509Utils._GetPublicKeyParameters(m_safeCertContext);
			}
			return (byte[])m_publicKeyParameters.Clone();
		}

		public virtual string GetKeyAlgorithmParametersString()
		{
			if (m_safeCertContext.IsInvalid)
			{
				throw new CryptographicException(Environment.GetResourceString("Cryptography_InvalidHandle"), "m_safeCertContext");
			}
			return Hex.EncodeHexString(GetKeyAlgorithmParameters());
		}

		public virtual string GetKeyAlgorithm()
		{
			if (m_safeCertContext.IsInvalid)
			{
				throw new CryptographicException(Environment.GetResourceString("Cryptography_InvalidHandle"), "m_safeCertContext");
			}
			if (m_publicKeyOid == null)
			{
				m_publicKeyOid = X509Utils._GetPublicKeyOid(m_safeCertContext);
			}
			return m_publicKeyOid;
		}

		public virtual byte[] GetPublicKey()
		{
			if (m_safeCertContext.IsInvalid)
			{
				throw new CryptographicException(Environment.GetResourceString("Cryptography_InvalidHandle"), "m_safeCertContext");
			}
			if (m_publicKeyValue == null)
			{
				m_publicKeyValue = X509Utils._GetPublicKeyValue(m_safeCertContext);
			}
			return (byte[])m_publicKeyValue.Clone();
		}

		public virtual string GetPublicKeyString()
		{
			return Hex.EncodeHexString(GetPublicKey());
		}

		public virtual byte[] GetRawCertData()
		{
			return RawData;
		}

		public virtual string GetRawCertDataString()
		{
			return Hex.EncodeHexString(GetRawCertData());
		}

		public virtual byte[] GetCertHash()
		{
			SetThumbprint();
			return (byte[])m_thumbprint.Clone();
		}

		public virtual string GetCertHashString()
		{
			SetThumbprint();
			return Hex.EncodeHexString(m_thumbprint);
		}

		public virtual string GetEffectiveDateString()
		{
			return NotBefore.ToString();
		}

		public virtual string GetExpirationDateString()
		{
			return NotAfter.ToString();
		}

		[ComVisible(false)]
		public override bool Equals(object obj)
		{
			if (!(obj is X509Certificate))
			{
				return false;
			}
			X509Certificate other = (X509Certificate)obj;
			return Equals(other);
		}

		public virtual bool Equals(X509Certificate other)
		{
			if (other == null)
			{
				return false;
			}
			if (m_safeCertContext.IsInvalid)
			{
				return other.m_safeCertContext.IsInvalid;
			}
			if (!Issuer.Equals(other.Issuer))
			{
				return false;
			}
			if (!SerialNumber.Equals(other.SerialNumber))
			{
				return false;
			}
			return true;
		}

		public override int GetHashCode()
		{
			if (m_safeCertContext.IsInvalid)
			{
				return 0;
			}
			SetThumbprint();
			int num = 0;
			for (int i = 0; i < m_thumbprint.Length && i < 4; i++)
			{
				num = (num << 8) | m_thumbprint[i];
			}
			return num;
		}

		public override string ToString()
		{
			return ToString(fVerbose: false);
		}

		public virtual string ToString(bool fVerbose)
		{
			if (!fVerbose || m_safeCertContext.IsInvalid)
			{
				return GetType().FullName;
			}
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("[Subject]" + Environment.NewLine + "  ");
			stringBuilder.Append(Subject);
			stringBuilder.Append(Environment.NewLine + Environment.NewLine + "[Issuer]" + Environment.NewLine + "  ");
			stringBuilder.Append(Issuer);
			stringBuilder.Append(Environment.NewLine + Environment.NewLine + "[Serial Number]" + Environment.NewLine + "  ");
			stringBuilder.Append(SerialNumber);
			stringBuilder.Append(Environment.NewLine + Environment.NewLine + "[Not Before]" + Environment.NewLine + "  ");
			stringBuilder.Append(NotBefore);
			stringBuilder.Append(Environment.NewLine + Environment.NewLine + "[Not After]" + Environment.NewLine + "  ");
			stringBuilder.Append(NotAfter);
			stringBuilder.Append(Environment.NewLine + Environment.NewLine + "[Thumbprint]" + Environment.NewLine + "  ");
			stringBuilder.Append(GetCertHashString());
			stringBuilder.Append(Environment.NewLine);
			return stringBuilder.ToString();
		}

		public virtual string GetFormat()
		{
			return "X509";
		}

		[ComVisible(false)]
		[PermissionSet(SecurityAction.InheritanceDemand, Unrestricted = true)]
		[PermissionSet(SecurityAction.LinkDemand, Unrestricted = true)]
		public virtual void Import(byte[] rawData)
		{
			Reset();
			LoadCertificateFromBlob(rawData, null, X509KeyStorageFlags.DefaultKeySet);
		}

		[ComVisible(false)]
		[PermissionSet(SecurityAction.InheritanceDemand, Unrestricted = true)]
		[PermissionSet(SecurityAction.LinkDemand, Unrestricted = true)]
		public virtual void Import(byte[] rawData, string password, X509KeyStorageFlags keyStorageFlags)
		{
			Reset();
			LoadCertificateFromBlob(rawData, password, keyStorageFlags);
		}

		[PermissionSet(SecurityAction.InheritanceDemand, Unrestricted = true)]
		[PermissionSet(SecurityAction.LinkDemand, Unrestricted = true)]
		public virtual void Import(byte[] rawData, SecureString password, X509KeyStorageFlags keyStorageFlags)
		{
			Reset();
			LoadCertificateFromBlob(rawData, password, keyStorageFlags);
		}

		[ComVisible(false)]
		[PermissionSet(SecurityAction.LinkDemand, Unrestricted = true)]
		[PermissionSet(SecurityAction.InheritanceDemand, Unrestricted = true)]
		public virtual void Import(string fileName)
		{
			Reset();
			LoadCertificateFromFile(fileName, null, X509KeyStorageFlags.DefaultKeySet);
		}

		[ComVisible(false)]
		[PermissionSet(SecurityAction.LinkDemand, Unrestricted = true)]
		[PermissionSet(SecurityAction.InheritanceDemand, Unrestricted = true)]
		public virtual void Import(string fileName, string password, X509KeyStorageFlags keyStorageFlags)
		{
			Reset();
			LoadCertificateFromFile(fileName, password, keyStorageFlags);
		}

		[PermissionSet(SecurityAction.InheritanceDemand, Unrestricted = true)]
		[PermissionSet(SecurityAction.LinkDemand, Unrestricted = true)]
		public virtual void Import(string fileName, SecureString password, X509KeyStorageFlags keyStorageFlags)
		{
			Reset();
			LoadCertificateFromFile(fileName, password, keyStorageFlags);
		}

		[ComVisible(false)]
		public virtual byte[] Export(X509ContentType contentType)
		{
			return ExportHelper(contentType, null);
		}

		[ComVisible(false)]
		public virtual byte[] Export(X509ContentType contentType, string password)
		{
			return ExportHelper(contentType, password);
		}

		public virtual byte[] Export(X509ContentType contentType, SecureString password)
		{
			return ExportHelper(contentType, password);
		}

		[ComVisible(false)]
		[PermissionSet(SecurityAction.InheritanceDemand, Unrestricted = true)]
		[PermissionSet(SecurityAction.LinkDemand, Unrestricted = true)]
		public virtual void Reset()
		{
			m_subjectName = null;
			m_issuerName = null;
			m_serialNumber = null;
			m_publicKeyParameters = null;
			m_publicKeyValue = null;
			m_publicKeyOid = null;
			m_rawData = null;
			m_thumbprint = null;
			m_notBefore = DateTime.MinValue;
			m_notAfter = DateTime.MinValue;
			if (!m_safeCertContext.IsInvalid)
			{
				m_safeCertContext.Dispose();
				m_safeCertContext = SafeCertContextHandle.InvalidHandle;
			}
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
		void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if (m_safeCertContext.IsInvalid)
			{
				info.AddValue("RawData", null);
			}
			else
			{
				info.AddValue("RawData", RawData);
			}
		}

		void IDeserializationCallback.OnDeserialization(object sender)
		{
		}

		private void SetThumbprint()
		{
			if (m_safeCertContext.IsInvalid)
			{
				throw new CryptographicException(Environment.GetResourceString("Cryptography_InvalidHandle"), "m_safeCertContext");
			}
			if (m_thumbprint == null)
			{
				m_thumbprint = X509Utils._GetThumbprint(m_safeCertContext);
			}
		}

		private byte[] ExportHelper(X509ContentType contentType, object password)
		{
			switch (contentType)
			{
			case X509ContentType.Pfx:
			{
				KeyContainerPermission keyContainerPermission = new KeyContainerPermission(KeyContainerPermissionFlags.Open | KeyContainerPermissionFlags.Export);
				keyContainerPermission.Demand();
				break;
			}
			default:
				throw new CryptographicException(Environment.GetResourceString("Cryptography_X509_InvalidContentType"));
			case X509ContentType.Cert:
			case X509ContentType.SerializedCert:
				break;
			}
			IntPtr intPtr = IntPtr.Zero;
			byte[] array = null;
			SafeCertStoreHandle safeCertStoreHandle = X509Utils.ExportCertToMemoryStore(this);
			RuntimeHelpers.PrepareConstrainedRegions();
			try
			{
				intPtr = X509Utils.PasswordToCoTaskMemUni(password);
				array = X509Utils._ExportCertificatesToBlob(safeCertStoreHandle, contentType, intPtr);
			}
			finally
			{
				if (intPtr != IntPtr.Zero)
				{
					Marshal.ZeroFreeCoTaskMemUnicode(intPtr);
				}
				safeCertStoreHandle.Dispose();
			}
			if (array == null)
			{
				throw new CryptographicException(Environment.GetResourceString("Cryptography_X509_ExportFailed"));
			}
			return array;
		}

		private void LoadCertificateFromBlob(byte[] rawData, object password, X509KeyStorageFlags keyStorageFlags)
		{
			if (rawData == null || rawData.Length == 0)
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_EmptyOrNullArray"), "rawData");
			}
			X509ContentType x509ContentType = X509Utils.MapContentType(X509Utils._QueryCertBlobType(rawData));
			if (x509ContentType == X509ContentType.Pfx && (keyStorageFlags & X509KeyStorageFlags.PersistKeySet) == X509KeyStorageFlags.PersistKeySet)
			{
				KeyContainerPermission keyContainerPermission = new KeyContainerPermission(KeyContainerPermissionFlags.Create);
				keyContainerPermission.Demand();
			}
			uint dwFlags = X509Utils.MapKeyStorageFlags(keyStorageFlags);
			IntPtr intPtr = IntPtr.Zero;
			RuntimeHelpers.PrepareConstrainedRegions();
			try
			{
				intPtr = X509Utils.PasswordToCoTaskMemUni(password);
				X509Utils._LoadCertFromBlob(rawData, intPtr, dwFlags, ((keyStorageFlags & X509KeyStorageFlags.PersistKeySet) != 0) ? true : false, ref m_safeCertContext);
			}
			finally
			{
				if (intPtr != IntPtr.Zero)
				{
					Marshal.ZeroFreeCoTaskMemUnicode(intPtr);
				}
			}
		}

		private void LoadCertificateFromFile(string fileName, object password, X509KeyStorageFlags keyStorageFlags)
		{
			if (fileName == null)
			{
				throw new ArgumentNullException("fileName");
			}
			string fullPathInternal = Path.GetFullPathInternal(fileName);
			new FileIOPermission(FileIOPermissionAccess.Read, fullPathInternal).Demand();
			X509ContentType x509ContentType = X509Utils.MapContentType(X509Utils._QueryCertFileType(fileName));
			if (x509ContentType == X509ContentType.Pfx && (keyStorageFlags & X509KeyStorageFlags.PersistKeySet) == X509KeyStorageFlags.PersistKeySet)
			{
				KeyContainerPermission keyContainerPermission = new KeyContainerPermission(KeyContainerPermissionFlags.Create);
				keyContainerPermission.Demand();
			}
			uint dwFlags = X509Utils.MapKeyStorageFlags(keyStorageFlags);
			IntPtr intPtr = IntPtr.Zero;
			RuntimeHelpers.PrepareConstrainedRegions();
			try
			{
				intPtr = X509Utils.PasswordToCoTaskMemUni(password);
				X509Utils._LoadCertFromFile(fileName, intPtr, dwFlags, ((keyStorageFlags & X509KeyStorageFlags.PersistKeySet) != 0) ? true : false, ref m_safeCertContext);
			}
			finally
			{
				if (intPtr != IntPtr.Zero)
				{
					Marshal.ZeroFreeCoTaskMemUnicode(intPtr);
				}
			}
		}
	}
}
