using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Cryptography.X509Certificates;
using System.Security.Permissions;
using System.Threading;
using Microsoft.Win32;

namespace System.Security.Cryptography
{
	internal static class Utils
	{
		private static object s_InternalSyncObject;

		private static int _defaultRsaProviderType = -1;

		private static SafeProvHandle _safeProvHandle = null;

		private static SafeProvHandle _safeDssProvHandle = null;

		private static RNGCryptoServiceProvider _rng = null;

		private static int s_hasEnhProv = -1;

		private static int s_fipsAlgorithmPolicy = -1;

		private static int s_win2KCrypto = -1;

		private static object InternalSyncObject
		{
			get
			{
				if (s_InternalSyncObject == null)
				{
					object value = new object();
					Interlocked.CompareExchange(ref s_InternalSyncObject, value, null);
				}
				return s_InternalSyncObject;
			}
		}

		internal static int DefaultRsaProviderType
		{
			get
			{
				if (_defaultRsaProviderType == -1)
				{
					_defaultRsaProviderType = ((Environment.OSVersion.Version.Major <= 5 && (Environment.OSVersion.Version.Major != 5 || Environment.OSVersion.Version.Minor < 1)) ? 1 : 24);
				}
				return _defaultRsaProviderType;
			}
		}

		internal static SafeProvHandle StaticProvHandle
		{
			get
			{
				if (_safeProvHandle == null)
				{
					lock (InternalSyncObject)
					{
						if (_safeProvHandle == null)
						{
							SafeProvHandle safeProvHandle = AcquireProvHandle(new CspParameters(DefaultRsaProviderType));
							Thread.MemoryBarrier();
							_safeProvHandle = safeProvHandle;
						}
					}
				}
				return _safeProvHandle;
			}
		}

		internal static SafeProvHandle StaticDssProvHandle
		{
			get
			{
				if (_safeDssProvHandle == null)
				{
					lock (InternalSyncObject)
					{
						if (_safeDssProvHandle == null)
						{
							SafeProvHandle safeDssProvHandle = AcquireProvHandle(new CspParameters(13));
							Thread.MemoryBarrier();
							_safeDssProvHandle = safeDssProvHandle;
						}
					}
				}
				return _safeDssProvHandle;
			}
		}

		internal static RNGCryptoServiceProvider StaticRandomNumberGenerator
		{
			get
			{
				if (_rng == null)
				{
					_rng = new RNGCryptoServiceProvider();
				}
				return _rng;
			}
		}

		internal static int HasEnhProv
		{
			get
			{
				if (s_hasEnhProv == -1)
				{
					s_hasEnhProv = (HasAlgorithm(41984, 2048) ? 1 : 0);
				}
				return s_hasEnhProv;
			}
		}

		internal static int FipsAlgorithmPolicy
		{
			[RegistryPermission(SecurityAction.Assert, Unrestricted = true)]
			get
			{
				if (s_fipsAlgorithmPolicy == -1)
				{
					if (!_GetEnforceFipsPolicySetting())
					{
						s_fipsAlgorithmPolicy = 0;
					}
					else if (Environment.OSVersion.Version.Major >= 6)
					{
						bool pfEnabled;
						uint num = Win32Native.BCryptGetFipsAlgorithmMode(out pfEnabled);
						if ((num != 0 && num != 3221225524u) || pfEnabled)
						{
							s_fipsAlgorithmPolicy = 1;
						}
						else
						{
							s_fipsAlgorithmPolicy = 0;
						}
					}
					else
					{
						using RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("System\\CurrentControlSet\\Control\\Lsa", writable: false);
						if (registryKey != null)
						{
							object value = registryKey.GetValue("FIPSAlgorithmPolicy");
							if (value != null)
							{
								s_fipsAlgorithmPolicy = (int)value;
							}
						}
					}
				}
				return s_fipsAlgorithmPolicy;
			}
		}

		internal static int Win2KCrypto
		{
			get
			{
				if (s_win2KCrypto == -1)
				{
					Win32Native.OSVERSIONINFO oSVERSIONINFO = new Win32Native.OSVERSIONINFO();
					s_win2KCrypto = ((Win32Native.GetVersionEx(oSVERSIONINFO) && oSVERSIONINFO.PlatformId == 2 && oSVERSIONINFO.MajorVersion >= 5) ? 1 : 0);
				}
				return s_win2KCrypto;
			}
		}

		internal static SafeProvHandle AcquireProvHandle(CspParameters parameters)
		{
			if (parameters == null)
			{
				parameters = new CspParameters(DefaultRsaProviderType);
			}
			SafeProvHandle hProv = SafeProvHandle.InvalidHandle;
			if (Win2KCrypto == 1)
			{
				_AcquireCSP(parameters, ref hProv);
			}
			else
			{
				if (parameters.KeyContainerName == null && (parameters.Flags & CspProviderFlags.UseDefaultKeyContainer) == 0)
				{
					parameters.KeyContainerName = _GetRandomKeyContainer();
				}
				hProv = CreateProvHandle(parameters, randomKeyContainer: true);
			}
			return hProv;
		}

		internal static SafeProvHandle CreateProvHandle(CspParameters parameters, bool randomKeyContainer)
		{
			SafeProvHandle hProv = SafeProvHandle.InvalidHandle;
			int num = _OpenCSP(parameters, 0u, ref hProv);
			KeyContainerPermission keyContainerPermission = new KeyContainerPermission(KeyContainerPermissionFlags.NoFlags);
			if (num != 0)
			{
				if ((parameters.Flags & CspProviderFlags.UseExistingKey) != 0 || (num != -2146893799 && num != -2146893802))
				{
					throw new CryptographicException(num);
				}
				if (!randomKeyContainer)
				{
					KeyContainerPermissionAccessEntry accessEntry = new KeyContainerPermissionAccessEntry(parameters, KeyContainerPermissionFlags.Create);
					keyContainerPermission.AccessEntries.Add(accessEntry);
					keyContainerPermission.Demand();
				}
				_CreateCSP(parameters, randomKeyContainer, ref hProv);
			}
			else if (!randomKeyContainer)
			{
				KeyContainerPermissionAccessEntry accessEntry2 = new KeyContainerPermissionAccessEntry(parameters, KeyContainerPermissionFlags.Open);
				keyContainerPermission.AccessEntries.Add(accessEntry2);
				keyContainerPermission.Demand();
			}
			return hProv;
		}

		internal static CryptoKeySecurity GetKeySetSecurityInfo(SafeProvHandle hProv, AccessControlSections accessControlSections)
		{
			if (Win2KCrypto != 1)
			{
				throw new PlatformNotSupportedException(Environment.GetResourceString("PlatformNotSupported_RequiresNT"));
			}
			SecurityInfos securityInfos = (SecurityInfos)0;
			Privilege privilege = null;
			if ((accessControlSections & AccessControlSections.Owner) != 0)
			{
				securityInfos |= SecurityInfos.Owner;
			}
			if ((accessControlSections & AccessControlSections.Group) != 0)
			{
				securityInfos |= SecurityInfos.Group;
			}
			if ((accessControlSections & AccessControlSections.Access) != 0)
			{
				securityInfos |= SecurityInfos.DiscretionaryAcl;
			}
			byte[] array = null;
			RuntimeHelpers.PrepareConstrainedRegions();
			int error;
			try
			{
				if ((accessControlSections & AccessControlSections.Audit) != 0)
				{
					securityInfos |= SecurityInfos.SystemAcl;
					privilege = new Privilege("SeSecurityPrivilege");
					privilege.Enable();
				}
				array = _GetKeySetSecurityInfo(hProv, securityInfos, out error);
			}
			finally
			{
				privilege?.Revert();
			}
			if (error == 0 && (array == null || array.Length == 0))
			{
				throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_NoSecurityDescriptor"));
			}
			switch (error)
			{
			case 8:
				throw new OutOfMemoryException();
			case 5:
				throw new UnauthorizedAccessException();
			case 1314:
				throw new PrivilegeNotHeldException("SeSecurityPrivilege");
			default:
				throw new CryptographicException(error);
			case 0:
			{
				CommonSecurityDescriptor securityDescriptor = new CommonSecurityDescriptor(isContainer: false, isDS: false, new RawSecurityDescriptor(array, 0), trusted: true);
				return new CryptoKeySecurity(securityDescriptor);
			}
			}
		}

		internal static void SetKeySetSecurityInfo(SafeProvHandle hProv, CryptoKeySecurity cryptoKeySecurity, AccessControlSections accessControlSections)
		{
			if (Win2KCrypto != 1)
			{
				throw new PlatformNotSupportedException(Environment.GetResourceString("PlatformNotSupported_RequiresNT"));
			}
			SecurityInfos securityInfos = (SecurityInfos)0;
			Privilege privilege = null;
			if ((accessControlSections & AccessControlSections.Owner) != 0 && cryptoKeySecurity._securityDescriptor.Owner != null)
			{
				securityInfos |= SecurityInfos.Owner;
			}
			if ((accessControlSections & AccessControlSections.Group) != 0 && cryptoKeySecurity._securityDescriptor.Group != null)
			{
				securityInfos |= SecurityInfos.Group;
			}
			if ((accessControlSections & AccessControlSections.Audit) != 0)
			{
				securityInfos |= SecurityInfos.SystemAcl;
			}
			if ((accessControlSections & AccessControlSections.Access) != 0 && cryptoKeySecurity._securityDescriptor.IsDiscretionaryAclPresent)
			{
				securityInfos |= SecurityInfos.DiscretionaryAcl;
			}
			if (securityInfos == (SecurityInfos)0)
			{
				return;
			}
			int num = 0;
			RuntimeHelpers.PrepareConstrainedRegions();
			try
			{
				if ((securityInfos & SecurityInfos.SystemAcl) != 0)
				{
					privilege = new Privilege("SeSecurityPrivilege");
					privilege.Enable();
				}
				byte[] securityDescriptorBinaryForm = cryptoKeySecurity.GetSecurityDescriptorBinaryForm();
				if (securityDescriptorBinaryForm != null && securityDescriptorBinaryForm.Length > 0)
				{
					num = _SetKeySetSecurityInfo(hProv, securityInfos, securityDescriptorBinaryForm);
				}
			}
			finally
			{
				privilege?.Revert();
			}
			switch (num)
			{
			case 5:
			case 1307:
			case 1308:
				throw new UnauthorizedAccessException();
			case 1314:
				throw new PrivilegeNotHeldException("SeSecurityPrivilege");
			case 6:
				throw new NotSupportedException(Environment.GetResourceString("AccessControl_InvalidHandle"));
			default:
				throw new CryptographicException(num);
			case 0:
				break;
			}
		}

		internal static byte[] ExportCspBlobHelper(bool includePrivateParameters, CspParameters parameters, SafeKeyHandle safeKeyHandle)
		{
			if (includePrivateParameters)
			{
				KeyContainerPermission keyContainerPermission = new KeyContainerPermission(KeyContainerPermissionFlags.NoFlags);
				KeyContainerPermissionAccessEntry accessEntry = new KeyContainerPermissionAccessEntry(parameters, KeyContainerPermissionFlags.Export);
				keyContainerPermission.AccessEntries.Add(accessEntry);
				keyContainerPermission.Demand();
			}
			return _ExportCspBlob(safeKeyHandle, includePrivateParameters ? 7 : 6);
		}

		internal static void GetKeyPairHelper(CspAlgorithmType keyType, CspParameters parameters, bool randomKeyContainer, int dwKeySize, ref SafeProvHandle safeProvHandle, ref SafeKeyHandle safeKeyHandle)
		{
			SafeProvHandle safeProvHandle2 = CreateProvHandle(parameters, randomKeyContainer);
			if (parameters.CryptoKeySecurity != null)
			{
				KeyContainerPermission keyContainerPermission = new KeyContainerPermission(KeyContainerPermissionFlags.NoFlags);
				KeyContainerPermissionAccessEntry accessEntry = new KeyContainerPermissionAccessEntry(parameters, KeyContainerPermissionFlags.ChangeAcl);
				keyContainerPermission.AccessEntries.Add(accessEntry);
				keyContainerPermission.Demand();
				SetKeySetSecurityInfo(safeProvHandle2, parameters.CryptoKeySecurity, parameters.CryptoKeySecurity.ChangedAccessControlSections);
			}
			if (parameters.ParentWindowHandle != IntPtr.Zero)
			{
				_SetProviderParameter(safeProvHandle2, parameters.KeyNumber, 10u, parameters.ParentWindowHandle);
			}
			else if (parameters.KeyPassword != null)
			{
				IntPtr intPtr = Marshal.SecureStringToCoTaskMemAnsi(parameters.KeyPassword);
				try
				{
					_SetProviderParameter(safeProvHandle2, parameters.KeyNumber, 11u, intPtr);
				}
				finally
				{
					if (intPtr != IntPtr.Zero)
					{
						Marshal.ZeroFreeCoTaskMemAnsi(intPtr);
					}
				}
			}
			safeProvHandle = safeProvHandle2;
			SafeKeyHandle hKey = SafeKeyHandle.InvalidHandle;
			int num = _GetUserKey(safeProvHandle, parameters.KeyNumber, ref hKey);
			if (num != 0)
			{
				if ((parameters.Flags & CspProviderFlags.UseExistingKey) != 0 || num != -2146893811)
				{
					throw new CryptographicException(num);
				}
				_GenerateKey(safeProvHandle, parameters.KeyNumber, parameters.Flags, dwKeySize, ref hKey);
			}
			byte[] array = _GetKeyParameter(hKey, 9u);
			int num2 = array[0] | (array[1] << 8) | (array[2] << 16) | (array[3] << 24);
			if ((keyType == CspAlgorithmType.Rsa && num2 != 41984 && num2 != 9216) || (keyType == CspAlgorithmType.Dss && num2 != 8704))
			{
				hKey.Dispose();
				throw new CryptographicException(Environment.GetResourceString("Cryptography_CSP_WrongKeySpec"));
			}
			safeKeyHandle = hKey;
		}

		internal static void ImportCspBlobHelper(CspAlgorithmType keyType, byte[] keyBlob, bool publicOnly, ref CspParameters parameters, bool randomKeyContainer, ref SafeProvHandle safeProvHandle, ref SafeKeyHandle safeKeyHandle)
		{
			if (safeKeyHandle != null && !safeKeyHandle.IsClosed)
			{
				safeKeyHandle.Dispose();
			}
			safeKeyHandle = SafeKeyHandle.InvalidHandle;
			if (publicOnly)
			{
				parameters.KeyNumber = _ImportCspBlob(keyBlob, (keyType == CspAlgorithmType.Dss) ? StaticDssProvHandle : StaticProvHandle, CspProviderFlags.NoFlags, ref safeKeyHandle);
				return;
			}
			KeyContainerPermission keyContainerPermission = new KeyContainerPermission(KeyContainerPermissionFlags.NoFlags);
			KeyContainerPermissionAccessEntry accessEntry = new KeyContainerPermissionAccessEntry(parameters, KeyContainerPermissionFlags.Import);
			keyContainerPermission.AccessEntries.Add(accessEntry);
			keyContainerPermission.Demand();
			if (safeProvHandle == null)
			{
				safeProvHandle = CreateProvHandle(parameters, randomKeyContainer);
			}
			parameters.KeyNumber = _ImportCspBlob(keyBlob, safeProvHandle, parameters.Flags, ref safeKeyHandle);
		}

		internal static CspParameters SaveCspParameters(CspAlgorithmType keyType, CspParameters userParameters, CspProviderFlags defaultFlags, ref bool randomKeyContainer)
		{
			CspParameters cspParameters;
			if (userParameters == null)
			{
				cspParameters = new CspParameters((keyType == CspAlgorithmType.Dss) ? 13 : DefaultRsaProviderType, null, null, defaultFlags);
			}
			else
			{
				ValidateCspFlags(userParameters.Flags);
				cspParameters = new CspParameters(userParameters);
			}
			if (cspParameters.KeyNumber == -1)
			{
				cspParameters.KeyNumber = ((keyType != CspAlgorithmType.Dss) ? 1 : 2);
			}
			else if (cspParameters.KeyNumber == 8704 || cspParameters.KeyNumber == 9216)
			{
				cspParameters.KeyNumber = 2;
			}
			else if (cspParameters.KeyNumber == 41984)
			{
				cspParameters.KeyNumber = 1;
			}
			randomKeyContainer = false;
			if (cspParameters.KeyContainerName == null && (cspParameters.Flags & CspProviderFlags.UseDefaultKeyContainer) == 0)
			{
				cspParameters.KeyContainerName = _GetRandomKeyContainer();
				randomKeyContainer = true;
			}
			return cspParameters;
		}

		private static void ValidateCspFlags(CspProviderFlags flags)
		{
			if ((flags & CspProviderFlags.UseExistingKey) != 0)
			{
				CspProviderFlags cspProviderFlags = CspProviderFlags.UseNonExportableKey | CspProviderFlags.UseArchivableKey | CspProviderFlags.UseUserProtectedKey;
				if ((flags & cspProviderFlags) != 0)
				{
					throw new ArgumentException(Environment.GetResourceString("Argument_InvalidFlag"));
				}
			}
			if ((flags & CspProviderFlags.UseUserProtectedKey) != 0)
			{
				if (!Environment.UserInteractive)
				{
					throw new InvalidOperationException(Environment.GetResourceString("Cryptography_NotInteractive"));
				}
				UIPermission uIPermission = new UIPermission(UIPermissionWindow.SafeTopLevelWindows);
				uIPermission.Demand();
			}
		}

		internal static byte[] GenerateRandom(int keySize)
		{
			byte[] array = new byte[keySize];
			StaticRandomNumberGenerator.GetBytes(array);
			return array;
		}

		internal static bool HasAlgorithm(int dwCalg, int dwKeySize)
		{
			bool flag = false;
			lock (InternalSyncObject)
			{
				return _SearchForAlgorithm(StaticProvHandle, dwCalg, dwKeySize);
			}
		}

		internal static string ObjToOidValue(object hashAlg)
		{
			if (hashAlg == null)
			{
				throw new ArgumentNullException("hashAlg");
			}
			string text = null;
			if (hashAlg is string)
			{
				text = CryptoConfig.MapNameToOID((string)hashAlg);
				if (text == null)
				{
					text = (string)hashAlg;
				}
			}
			else if (hashAlg is HashAlgorithm)
			{
				text = CryptoConfig.MapNameToOID(hashAlg.GetType().ToString());
			}
			else if (hashAlg is Type)
			{
				text = CryptoConfig.MapNameToOID(hashAlg.ToString());
			}
			if (text == null)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_InvalidValue"));
			}
			return text;
		}

		internal static HashAlgorithm ObjToHashAlgorithm(object hashAlg)
		{
			if (hashAlg == null)
			{
				throw new ArgumentNullException("hashAlg");
			}
			HashAlgorithm hashAlgorithm = null;
			if (hashAlg is string)
			{
				hashAlgorithm = (HashAlgorithm)CryptoConfig.CreateFromName((string)hashAlg);
				if (hashAlgorithm == null)
				{
					string text = X509Utils._GetFriendlyNameFromOid((string)hashAlg);
					if (text != null)
					{
						hashAlgorithm = (HashAlgorithm)CryptoConfig.CreateFromName(text);
					}
				}
			}
			else if (hashAlg is HashAlgorithm)
			{
				hashAlgorithm = (HashAlgorithm)hashAlg;
			}
			else if (hashAlg is Type)
			{
				hashAlgorithm = (HashAlgorithm)CryptoConfig.CreateFromName(hashAlg.ToString());
			}
			if (hashAlgorithm == null)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_InvalidValue"));
			}
			return hashAlgorithm;
		}

		internal static string DiscardWhiteSpaces(string inputBuffer)
		{
			return DiscardWhiteSpaces(inputBuffer, 0, inputBuffer.Length);
		}

		internal static string DiscardWhiteSpaces(string inputBuffer, int inputOffset, int inputCount)
		{
			int num = 0;
			for (int i = 0; i < inputCount; i++)
			{
				if (char.IsWhiteSpace(inputBuffer[inputOffset + i]))
				{
					num++;
				}
			}
			char[] array = new char[inputCount - num];
			num = 0;
			for (int i = 0; i < inputCount; i++)
			{
				if (!char.IsWhiteSpace(inputBuffer[inputOffset + i]))
				{
					array[num++] = inputBuffer[inputOffset + i];
				}
			}
			return new string(array);
		}

		internal static int ConvertByteArrayToInt(byte[] input)
		{
			int num = 0;
			for (int i = 0; i < input.Length; i++)
			{
				num *= 256;
				num += input[i];
			}
			return num;
		}

		internal static byte[] ConvertIntToByteArray(int dwInput)
		{
			byte[] array = new byte[8];
			int num = 0;
			if (dwInput == 0)
			{
				return new byte[1];
			}
			int num2 = dwInput;
			while (num2 > 0)
			{
				int num3 = num2 % 256;
				array[num] = (byte)num3;
				num2 = (num2 - num3) / 256;
				num++;
			}
			byte[] array2 = new byte[num];
			for (int i = 0; i < num; i++)
			{
				array2[i] = array[num - i - 1];
			}
			return array2;
		}

		internal static void ConvertIntToByteArray(uint dwInput, ref byte[] counter)
		{
			uint num = dwInput;
			int num2 = 0;
			Array.Clear(counter, 0, counter.Length);
			if (dwInput != 0)
			{
				while (num != 0)
				{
					uint num3 = num % 256u;
					counter[3 - num2] = (byte)num3;
					num = (num - num3) / 256u;
					num2++;
				}
			}
		}

		internal static byte[] FixupKeyParity(byte[] key)
		{
			byte[] array = new byte[key.Length];
			for (int i = 0; i < key.Length; i++)
			{
				array[i] = (byte)(key[i] & 0xFEu);
				byte b = (byte)((array[i] & 0xFu) ^ (uint)(array[i] >> 4));
				byte b2 = (byte)((b & 3u) ^ (uint)(b >> 2));
				if ((byte)((b2 & 1) ^ (b2 >> 1)) == 0)
				{
					array[i] |= 1;
				}
			}
			return array;
		}

		internal unsafe static void DWORDFromLittleEndian(uint* x, int digits, byte* block)
		{
			int num = 0;
			int num2 = 0;
			while (num < digits)
			{
				x[num] = (uint)(block[num2] | (block[num2 + 1] << 8) | (block[num2 + 2] << 16) | (block[num2 + 3] << 24));
				num++;
				num2 += 4;
			}
		}

		internal static void DWORDToLittleEndian(byte[] block, uint[] x, int digits)
		{
			int num = 0;
			int num2 = 0;
			while (num < digits)
			{
				block[num2] = (byte)(x[num] & 0xFFu);
				block[num2 + 1] = (byte)((x[num] >> 8) & 0xFFu);
				block[num2 + 2] = (byte)((x[num] >> 16) & 0xFFu);
				block[num2 + 3] = (byte)((x[num] >> 24) & 0xFFu);
				num++;
				num2 += 4;
			}
		}

		internal unsafe static void DWORDFromBigEndian(uint* x, int digits, byte* block)
		{
			int num = 0;
			int num2 = 0;
			while (num < digits)
			{
				x[num] = (uint)((block[num2] << 24) | (block[num2 + 1] << 16) | (block[num2 + 2] << 8) | block[num2 + 3]);
				num++;
				num2 += 4;
			}
		}

		internal static void DWORDToBigEndian(byte[] block, uint[] x, int digits)
		{
			int num = 0;
			int num2 = 0;
			while (num < digits)
			{
				block[num2] = (byte)((x[num] >> 24) & 0xFFu);
				block[num2 + 1] = (byte)((x[num] >> 16) & 0xFFu);
				block[num2 + 2] = (byte)((x[num] >> 8) & 0xFFu);
				block[num2 + 3] = (byte)(x[num] & 0xFFu);
				num++;
				num2 += 4;
			}
		}

		internal unsafe static void QuadWordFromBigEndian(ulong* x, int digits, byte* block)
		{
			int num = 0;
			int num2 = 0;
			while (num < digits)
			{
				x[num] = ((ulong)block[num2] << 56) | ((ulong)block[num2 + 1] << 48) | ((ulong)block[num2 + 2] << 40) | ((ulong)block[num2 + 3] << 32) | ((ulong)block[num2 + 4] << 24) | ((ulong)block[num2 + 5] << 16) | ((ulong)block[num2 + 6] << 8) | block[num2 + 7];
				num++;
				num2 += 8;
			}
		}

		internal static void QuadWordToBigEndian(byte[] block, ulong[] x, int digits)
		{
			int num = 0;
			int num2 = 0;
			while (num < digits)
			{
				block[num2] = (byte)((x[num] >> 56) & 0xFF);
				block[num2 + 1] = (byte)((x[num] >> 48) & 0xFF);
				block[num2 + 2] = (byte)((x[num] >> 40) & 0xFF);
				block[num2 + 3] = (byte)((x[num] >> 32) & 0xFF);
				block[num2 + 4] = (byte)((x[num] >> 24) & 0xFF);
				block[num2 + 5] = (byte)((x[num] >> 16) & 0xFF);
				block[num2 + 6] = (byte)((x[num] >> 8) & 0xFF);
				block[num2 + 7] = (byte)(x[num] & 0xFF);
				num++;
				num2 += 8;
			}
		}

		internal static byte[] Int(uint i)
		{
			byte[] bytes = BitConverter.GetBytes(i);
			byte[] result = new byte[4]
			{
				bytes[3],
				bytes[2],
				bytes[1],
				bytes[0]
			};
			if (!BitConverter.IsLittleEndian)
			{
				return bytes;
			}
			return result;
		}

		internal unsafe static void BlockCopy(byte* src, int srcOffset, byte* dst, int dstOffset, int count)
		{
			for (int i = 0; i < count; i++)
			{
				dst[dstOffset + i] = src[srcOffset + i];
			}
		}

		internal unsafe static void BlockCopy(byte[] src, int srcOffset, int* dst, int dstOffset, int count)
		{
			fixed (byte* src2 = src)
			{
				BlockCopy(src2, srcOffset, (byte*)dst, dstOffset, count);
			}
		}

		internal unsafe static void BlockCopy(int* src, int srcOffset, int[] dst, int dstOffset, int count)
		{
			fixed (int* dst2 = &dst[dstOffset])
			{
				BlockCopy((byte*)(src + srcOffset), srcOffset, (byte*)dst2, 0, count);
			}
		}

		internal static byte[] RsaOaepEncrypt(RSA rsa, HashAlgorithm hash, PKCS1MaskGenerationMethod mgf, RandomNumberGenerator rng, byte[] data)
		{
			int num = rsa.KeySize / 8;
			int num2 = hash.HashSize / 8;
			if (data.Length + 2 + 2 * num2 > num)
			{
				throw new CryptographicException(string.Format(null, Environment.GetResourceString("Cryptography_Padding_EncDataTooBig"), num - 2 - 2 * num2));
			}
			hash.ComputeHash(new byte[0]);
			byte[] array = new byte[num - num2];
			Buffer.InternalBlockCopy(hash.Hash, 0, array, 0, num2);
			array[array.Length - data.Length - 1] = 1;
			Buffer.InternalBlockCopy(data, 0, array, array.Length - data.Length, data.Length);
			byte[] array2 = new byte[num2];
			rng.GetBytes(array2);
			byte[] array3 = mgf.GenerateMask(array2, array.Length);
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = (byte)(array[i] ^ array3[i]);
			}
			array3 = mgf.GenerateMask(array, num2);
			for (int j = 0; j < array2.Length; j++)
			{
				array2[j] ^= array3[j];
			}
			byte[] array4 = new byte[num];
			Buffer.InternalBlockCopy(array2, 0, array4, 0, array2.Length);
			Buffer.InternalBlockCopy(array, 0, array4, array2.Length, array.Length);
			return rsa.EncryptValue(array4);
		}

		internal static byte[] RsaOaepDecrypt(RSA rsa, HashAlgorithm hash, PKCS1MaskGenerationMethod mgf, byte[] encryptedData)
		{
			int num = rsa.KeySize / 8;
			byte[] array = null;
			try
			{
				array = rsa.DecryptValue(encryptedData);
			}
			catch (CryptographicException)
			{
				throw new CryptographicException(Environment.GetResourceString("Cryptography_OAEPDecoding"));
			}
			int num2 = hash.HashSize / 8;
			int num3 = num - array.Length;
			if (num3 < 0 || num3 >= num2)
			{
				throw new CryptographicException(Environment.GetResourceString("Cryptography_OAEPDecoding"));
			}
			byte[] array2 = new byte[num2];
			Buffer.InternalBlockCopy(array, 0, array2, num3, array2.Length - num3);
			byte[] array3 = new byte[array.Length - array2.Length + num3];
			Buffer.InternalBlockCopy(array, array2.Length - num3, array3, 0, array3.Length);
			byte[] array4 = mgf.GenerateMask(array3, array2.Length);
			int num4 = 0;
			for (num4 = 0; num4 < array2.Length; num4++)
			{
				array2[num4] ^= array4[num4];
			}
			array4 = mgf.GenerateMask(array2, array3.Length);
			for (num4 = 0; num4 < array3.Length; num4++)
			{
				array3[num4] = (byte)(array3[num4] ^ array4[num4]);
			}
			hash.ComputeHash(new byte[0]);
			byte[] hash2 = hash.Hash;
			for (num4 = 0; num4 < num2; num4++)
			{
				if (array3[num4] != hash2[num4])
				{
					throw new CryptographicException(Environment.GetResourceString("Cryptography_OAEPDecoding"));
				}
			}
			for (; num4 < array3.Length && array3[num4] != 1; num4++)
			{
				if (array3[num4] != 0)
				{
					throw new CryptographicException(Environment.GetResourceString("Cryptography_OAEPDecoding"));
				}
			}
			if (num4 == array3.Length)
			{
				throw new CryptographicException(Environment.GetResourceString("Cryptography_OAEPDecoding"));
			}
			num4++;
			byte[] array5 = new byte[array3.Length - num4];
			Buffer.InternalBlockCopy(array3, num4, array5, 0, array5.Length);
			return array5;
		}

		internal static byte[] RsaPkcs1Padding(RSA rsa, byte[] oid, byte[] hash)
		{
			int num = rsa.KeySize / 8;
			byte[] array = new byte[num];
			byte[] array2 = new byte[oid.Length + 8 + hash.Length];
			array2[0] = 48;
			int num2 = array2.Length - 2;
			array2[1] = (byte)num2;
			array2[2] = 48;
			num2 = oid.Length + 2;
			array2[3] = (byte)num2;
			Buffer.InternalBlockCopy(oid, 0, array2, 4, oid.Length);
			array2[4 + oid.Length] = 5;
			array2[4 + oid.Length + 1] = 0;
			array2[4 + oid.Length + 2] = 4;
			array2[4 + oid.Length + 3] = (byte)hash.Length;
			Buffer.InternalBlockCopy(hash, 0, array2, oid.Length + 8, hash.Length);
			int num3 = num - array2.Length;
			if (num3 <= 2)
			{
				throw new CryptographicUnexpectedOperationException(Environment.GetResourceString("Cryptography_InvalidOID"));
			}
			array[0] = 0;
			array[1] = 1;
			for (int i = 2; i < num3 - 1; i++)
			{
				array[i] = byte.MaxValue;
			}
			array[num3 - 1] = 0;
			Buffer.InternalBlockCopy(array2, 0, array, num3, array2.Length);
			return array;
		}

		internal static bool CompareBigIntArrays(byte[] lhs, byte[] rhs)
		{
			if (lhs == null)
			{
				return rhs == null;
			}
			int i = 0;
			int j = 0;
			for (; i < lhs.Length && lhs[i] == 0; i++)
			{
			}
			for (; j < rhs.Length && rhs[j] == 0; j++)
			{
			}
			int num = lhs.Length - i;
			if (rhs.Length - j != num)
			{
				return false;
			}
			for (int k = 0; k < num; k++)
			{
				if (lhs[i + k] != rhs[j + k])
				{
					return false;
				}
			}
			return true;
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern void _AcquireCSP(CspParameters param, ref SafeProvHandle hProv);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern void _CreateCSP(CspParameters param, bool randomKeyContainer, ref SafeProvHandle hProv);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern void _CreateHash(SafeProvHandle hProv, int algid, ref SafeHashHandle hKey);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern byte[] _CryptDeriveKey(SafeProvHandle hProv, int algid, int algidHash, byte[] password, int dwFlags, byte[] IV);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern int _DecryptData(SafeKeyHandle hKey, byte[] data, int ib, int cb, ref byte[] outputBuffer, int outputOffset, PaddingMode PaddingMode, bool fDone);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern byte[] _DecryptKey(SafeKeyHandle hPubKey, byte[] key, int dwFlags);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern byte[] _DecryptPKWin2KEnh(SafeKeyHandle hPubKey, byte[] key, bool fOAEP, out int hr);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern int _EncryptData(SafeKeyHandle hKey, byte[] data, int ib, int cb, ref byte[] outputBuffer, int outputOffset, PaddingMode PaddingMode, bool fDone);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern byte[] _EncryptKey(SafeKeyHandle hPubKey, byte[] key);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern byte[] _EncryptPKWin2KEnh(SafeKeyHandle hPubKey, byte[] key, bool fOAEP, out int hr);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern byte[] _EndHash(SafeHashHandle hHash);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern byte[] _ExportCspBlob(SafeKeyHandle hKey, int blobType);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern void _ExportKey(SafeKeyHandle hKey, int blobType, object cspObject);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern void _GenerateKey(SafeProvHandle hProv, int algid, CspProviderFlags flags, int keySize, ref SafeKeyHandle hKey);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern void _GetBytes(SafeProvHandle hProv, byte[] randomBytes);

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern bool _GetEnforceFipsPolicySetting();

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern byte[] _GetKeyParameter(SafeKeyHandle hKey, uint paramID);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern byte[] _GetKeySetSecurityInfo(SafeProvHandle hProv, SecurityInfos securityInfo, out int error);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern void _GetNonZeroBytes(SafeProvHandle hProv, byte[] randomBytes);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern bool _GetPersistKeyInCsp(SafeProvHandle hProv);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern object _GetProviderParameter(SafeProvHandle hProv, int keyNumber, uint paramID);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern string _GetRandomKeyContainer();

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern int _GetUserKey(SafeProvHandle hProv, int keyNumber, ref SafeKeyHandle hKey);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern void _HashData(SafeHashHandle hHash, byte[] data, int ibStart, int cbSize);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern void _ImportBulkKey(SafeProvHandle hProv, int algid, bool useSalt, byte[] key, ref SafeKeyHandle hKey);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern int _ImportCspBlob(byte[] keyBlob, SafeProvHandle hProv, CspProviderFlags flags, ref SafeKeyHandle hKey);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern void _ImportKey(SafeProvHandle hCSP, int keyNumber, CspProviderFlags flags, object cspObject, ref SafeKeyHandle hKey);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern int _OpenCSP(CspParameters param, uint flags, ref SafeProvHandle hProv);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern bool _ProduceLegacyHmacValues();

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern bool _SearchForAlgorithm(SafeProvHandle hProv, int algID, int keyLength);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern void _SetKeyParamDw(SafeKeyHandle hKey, int param, int dwValue);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern void _SetKeyParamRgb(SafeKeyHandle hKey, int param, byte[] value);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern int _SetKeySetSecurityInfo(SafeProvHandle hProv, SecurityInfos securityInfo, byte[] sd);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern void _SetPersistKeyInCsp(SafeProvHandle hProv, bool fPersistKeyInCsp);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern void _SetProviderParameter(SafeProvHandle hProv, int keyNumber, uint paramID, IntPtr pbData);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern void _ShowLegacyHmacWarning();

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern byte[] _SignValue(SafeKeyHandle hKey, int keyNumber, int calgKey, int calgHash, byte[] hash, int dwFlags);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern bool _VerifySign(SafeKeyHandle hKey, int calgKey, int calgHash, byte[] hash, byte[] signature, int dwFlags);
	}
}
