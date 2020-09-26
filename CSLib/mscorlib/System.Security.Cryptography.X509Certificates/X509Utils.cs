using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace System.Security.Cryptography.X509Certificates
{
	internal static class X509Utils
	{
		internal static int OidToAlgId(string oid)
		{
			return OidToAlgId(oid, OidGroup.AllGroups);
		}

		internal static int OidToAlgId(string oid, OidGroup group)
		{
			if (oid == null)
			{
				return 32772;
			}
			string text = CryptoConfig.MapNameToOID(oid, group);
			if (text == null)
			{
				text = oid;
			}
			return OidToAlgIdStrict(text, group);
		}

		internal static int OidToAlgIdStrict(string oid, OidGroup group)
		{
			int num = 0;
			num = (string.Equals(oid, "2.16.840.1.101.3.4.2.1", StringComparison.Ordinal) ? 32780 : (string.Equals(oid, "2.16.840.1.101.3.4.2.2", StringComparison.Ordinal) ? 32781 : ((!string.Equals(oid, "2.16.840.1.101.3.4.2.3", StringComparison.Ordinal)) ? _GetAlgIdFromOid(oid, group) : 32782)));
			if (num == 0 || num == -1)
			{
				throw new CryptographicException(Environment.GetResourceString("Cryptography_InvalidOID"));
			}
			return num;
		}

		internal static X509ContentType MapContentType(uint contentType)
		{
			switch (contentType)
			{
			case 1u:
				return X509ContentType.Cert;
			case 4u:
				return X509ContentType.SerializedStore;
			case 5u:
				return X509ContentType.SerializedCert;
			case 8u:
			case 9u:
				return X509ContentType.Pkcs7;
			case 10u:
				return X509ContentType.Authenticode;
			case 12u:
				return X509ContentType.Pfx;
			default:
				return X509ContentType.Unknown;
			}
		}

		internal static uint MapKeyStorageFlags(X509KeyStorageFlags keyStorageFlags)
		{
			uint num = 0u;
			if ((keyStorageFlags & X509KeyStorageFlags.UserKeySet) == X509KeyStorageFlags.UserKeySet)
			{
				num |= 0x1000u;
			}
			else if ((keyStorageFlags & X509KeyStorageFlags.MachineKeySet) == X509KeyStorageFlags.MachineKeySet)
			{
				num |= 0x20u;
			}
			if ((keyStorageFlags & X509KeyStorageFlags.Exportable) == X509KeyStorageFlags.Exportable)
			{
				num |= 1u;
			}
			if ((keyStorageFlags & X509KeyStorageFlags.UserProtected) == X509KeyStorageFlags.UserProtected)
			{
				num |= 2u;
			}
			return num;
		}

		internal static SafeCertStoreHandle ExportCertToMemoryStore(X509Certificate certificate)
		{
			SafeCertStoreHandle safeCertStoreHandle = SafeCertStoreHandle.InvalidHandle;
			_OpenX509Store(2u, 8704u, null, ref safeCertStoreHandle);
			_AddCertificateToStore(safeCertStoreHandle, certificate.CertContext);
			return safeCertStoreHandle;
		}

		internal static IntPtr PasswordToCoTaskMemUni(object password)
		{
			if (password != null)
			{
				string text = password as string;
				if (text != null)
				{
					return Marshal.StringToCoTaskMemUni(text);
				}
				SecureString secureString = password as SecureString;
				if (secureString != null)
				{
					return Marshal.SecureStringToCoTaskMemUnicode(secureString);
				}
			}
			return IntPtr.Zero;
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern void _AddCertificateToStore(SafeCertStoreHandle safeCertStoreHandle, SafeCertContextHandle safeCertContext);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern void _DuplicateCertContext(IntPtr handle, ref SafeCertContextHandle safeCertContext);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern byte[] _ExportCertificatesToBlob(SafeCertStoreHandle safeCertStoreHandle, X509ContentType contentType, IntPtr password);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern int _GetAlgIdFromOid(string oid, OidGroup group);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern byte[] _GetCertRawData(SafeCertContextHandle safeCertContext);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern void _GetDateNotAfter(SafeCertContextHandle safeCertContext, ref Win32Native.FILE_TIME fileTime);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern void _GetDateNotBefore(SafeCertContextHandle safeCertContext, ref Win32Native.FILE_TIME fileTime);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern string _GetFriendlyNameFromOid(string oid);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern string _GetIssuerName(SafeCertContextHandle safeCertContext, bool legacyV1Mode);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern string _GetOidFromFriendlyName(string oid, OidGroup group);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern string _GetPublicKeyOid(SafeCertContextHandle safeCertContext);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern byte[] _GetPublicKeyParameters(SafeCertContextHandle safeCertContext);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern byte[] _GetPublicKeyValue(SafeCertContextHandle safeCertContext);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern string _GetSubjectInfo(SafeCertContextHandle safeCertContext, uint displayType, bool legacyV1Mode);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern byte[] _GetSerialNumber(SafeCertContextHandle safeCertContext);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern byte[] _GetThumbprint(SafeCertContextHandle safeCertContext);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern void _LoadCertFromBlob(byte[] rawData, IntPtr password, uint dwFlags, bool persistKeySet, ref SafeCertContextHandle pCertCtx);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern void _LoadCertFromFile(string fileName, IntPtr password, uint dwFlags, bool persistKeySet, ref SafeCertContextHandle pCertCtx);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern void _OpenX509Store(uint storeType, uint flags, string storeName, ref SafeCertStoreHandle safeCertStoreHandle);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern uint _QueryCertBlobType(byte[] rawData);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern uint _QueryCertFileType(string fileName);
	}
}
