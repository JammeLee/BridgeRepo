using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Security.Permissions;

namespace System.Security.Cryptography
{
	[ComVisible(true)]
	public sealed class RSACryptoServiceProvider : RSA, ICspAsymmetricAlgorithm
	{
		private const uint RandomKeyContainerFlag = 2147483648u;

		private int _dwKeySize;

		private CspParameters _parameters;

		private bool _randomKeyContainer;

		private SafeProvHandle _safeProvHandle;

		private SafeKeyHandle _safeKeyHandle;

		private static CspProviderFlags s_UseMachineKeyStore;

		[ComVisible(false)]
		public bool PublicOnly
		{
			get
			{
				GetKeyPair();
				byte[] array = Utils._GetKeyParameter(_safeKeyHandle, 2u);
				return array[0] == 1;
			}
		}

		[ComVisible(false)]
		public CspKeyContainerInfo CspKeyContainerInfo
		{
			get
			{
				GetKeyPair();
				return new CspKeyContainerInfo(_parameters, _randomKeyContainer);
			}
		}

		public override int KeySize
		{
			get
			{
				GetKeyPair();
				byte[] array = Utils._GetKeyParameter(_safeKeyHandle, 1u);
				_dwKeySize = array[0] | (array[1] << 8) | (array[2] << 16) | (array[3] << 24);
				return _dwKeySize;
			}
		}

		public override string KeyExchangeAlgorithm
		{
			get
			{
				if (_parameters.KeyNumber == 1)
				{
					return "RSA-PKCS1-KeyEx";
				}
				return null;
			}
		}

		public override string SignatureAlgorithm => "http://www.w3.org/2000/09/xmldsig#rsa-sha1";

		public static bool UseMachineKeyStore
		{
			get
			{
				return s_UseMachineKeyStore == CspProviderFlags.UseMachineKeyStore;
			}
			set
			{
				s_UseMachineKeyStore = (value ? CspProviderFlags.UseMachineKeyStore : CspProviderFlags.NoFlags);
			}
		}

		public bool PersistKeyInCsp
		{
			get
			{
				if (_safeProvHandle == null)
				{
					lock (this)
					{
						if (_safeProvHandle == null)
						{
							_safeProvHandle = Utils.CreateProvHandle(_parameters, _randomKeyContainer);
						}
					}
				}
				return Utils._GetPersistKeyInCsp(_safeProvHandle);
			}
			set
			{
				bool persistKeyInCsp = PersistKeyInCsp;
				if (value != persistKeyInCsp)
				{
					KeyContainerPermission keyContainerPermission = new KeyContainerPermission(KeyContainerPermissionFlags.NoFlags);
					if (!value)
					{
						KeyContainerPermissionAccessEntry accessEntry = new KeyContainerPermissionAccessEntry(_parameters, KeyContainerPermissionFlags.Delete);
						keyContainerPermission.AccessEntries.Add(accessEntry);
					}
					else
					{
						KeyContainerPermissionAccessEntry accessEntry2 = new KeyContainerPermissionAccessEntry(_parameters, KeyContainerPermissionFlags.Create);
						keyContainerPermission.AccessEntries.Add(accessEntry2);
					}
					keyContainerPermission.Demand();
					Utils._SetPersistKeyInCsp(_safeProvHandle, value);
				}
			}
		}

		public RSACryptoServiceProvider()
			: this(0, new CspParameters(Utils.DefaultRsaProviderType, null, null, s_UseMachineKeyStore), useDefaultKeySize: true)
		{
		}

		public RSACryptoServiceProvider(int dwKeySize)
			: this(dwKeySize, new CspParameters(Utils.DefaultRsaProviderType, null, null, s_UseMachineKeyStore), useDefaultKeySize: false)
		{
		}

		public RSACryptoServiceProvider(CspParameters parameters)
			: this(0, parameters, useDefaultKeySize: true)
		{
		}

		public RSACryptoServiceProvider(int dwKeySize, CspParameters parameters)
			: this(dwKeySize, parameters, useDefaultKeySize: false)
		{
		}

		private RSACryptoServiceProvider(int dwKeySize, CspParameters parameters, bool useDefaultKeySize)
		{
			if (dwKeySize < 0)
			{
				throw new ArgumentOutOfRangeException("dwKeySize", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
			}
			bool flag = (parameters.Flags & (CspProviderFlags)(-2147483648)) != 0;
			parameters.Flags &= (CspProviderFlags)2147483647;
			_parameters = Utils.SaveCspParameters(CspAlgorithmType.Rsa, parameters, s_UseMachineKeyStore, ref _randomKeyContainer);
			if (_parameters.KeyNumber == 2 || Utils.HasEnhProv == 1)
			{
				LegalKeySizesValue = new KeySizes[1]
				{
					new KeySizes(384, 16384, 8)
				};
				if (useDefaultKeySize)
				{
					_dwKeySize = 1024;
				}
			}
			else
			{
				LegalKeySizesValue = new KeySizes[1]
				{
					new KeySizes(384, 512, 8)
				};
				if (useDefaultKeySize)
				{
					_dwKeySize = 512;
				}
			}
			if (!useDefaultKeySize)
			{
				_dwKeySize = dwKeySize;
			}
			if (!_randomKeyContainer || Environment.GetCompatibilityFlag(CompatibilityFlag.EagerlyGenerateRandomAsymmKeys))
			{
				GetKeyPair();
			}
			_randomKeyContainer |= flag;
		}

		private void GetKeyPair()
		{
			if (_safeKeyHandle != null)
			{
				return;
			}
			lock (this)
			{
				if (_safeKeyHandle == null)
				{
					Utils.GetKeyPairHelper(CspAlgorithmType.Rsa, _parameters, _randomKeyContainer, _dwKeySize, ref _safeProvHandle, ref _safeKeyHandle);
				}
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (_safeKeyHandle != null && !_safeKeyHandle.IsClosed)
			{
				_safeKeyHandle.Dispose();
			}
			if (_safeProvHandle != null && !_safeProvHandle.IsClosed)
			{
				_safeProvHandle.Dispose();
			}
		}

		public override RSAParameters ExportParameters(bool includePrivateParameters)
		{
			GetKeyPair();
			if (includePrivateParameters)
			{
				KeyContainerPermission keyContainerPermission = new KeyContainerPermission(KeyContainerPermissionFlags.NoFlags);
				KeyContainerPermissionAccessEntry accessEntry = new KeyContainerPermissionAccessEntry(_parameters, KeyContainerPermissionFlags.Export);
				keyContainerPermission.AccessEntries.Add(accessEntry);
				keyContainerPermission.Demand();
			}
			RSACspObject rSACspObject = new RSACspObject();
			int blobType = (includePrivateParameters ? 7 : 6);
			Utils._ExportKey(_safeKeyHandle, blobType, rSACspObject);
			return RSAObjectToStruct(rSACspObject);
		}

		[ComVisible(false)]
		public byte[] ExportCspBlob(bool includePrivateParameters)
		{
			GetKeyPair();
			return Utils.ExportCspBlobHelper(includePrivateParameters, _parameters, _safeKeyHandle);
		}

		public override void ImportParameters(RSAParameters parameters)
		{
			RSACspObject cspObject = RSAStructToObject(parameters);
			if (_safeKeyHandle != null && !_safeKeyHandle.IsClosed)
			{
				_safeKeyHandle.Dispose();
			}
			_safeKeyHandle = SafeKeyHandle.InvalidHandle;
			if (IsPublic(parameters))
			{
				Utils._ImportKey(Utils.StaticProvHandle, 41984, CspProviderFlags.NoFlags, cspObject, ref _safeKeyHandle);
				return;
			}
			KeyContainerPermission keyContainerPermission = new KeyContainerPermission(KeyContainerPermissionFlags.NoFlags);
			KeyContainerPermissionAccessEntry accessEntry = new KeyContainerPermissionAccessEntry(_parameters, KeyContainerPermissionFlags.Import);
			keyContainerPermission.AccessEntries.Add(accessEntry);
			keyContainerPermission.Demand();
			if (_safeProvHandle == null)
			{
				_safeProvHandle = Utils.CreateProvHandle(_parameters, _randomKeyContainer);
			}
			Utils._ImportKey(_safeProvHandle, 41984, _parameters.Flags, cspObject, ref _safeKeyHandle);
		}

		[ComVisible(false)]
		public void ImportCspBlob(byte[] keyBlob)
		{
			Utils.ImportCspBlobHelper(CspAlgorithmType.Rsa, keyBlob, IsPublic(keyBlob), ref _parameters, _randomKeyContainer, ref _safeProvHandle, ref _safeKeyHandle);
		}

		public byte[] SignData(Stream inputStream, object halg)
		{
			string str = Utils.ObjToOidValue(halg);
			HashAlgorithm hashAlgorithm = Utils.ObjToHashAlgorithm(halg);
			byte[] rgbHash = hashAlgorithm.ComputeHash(inputStream);
			return SignHash(rgbHash, str);
		}

		public byte[] SignData(byte[] buffer, object halg)
		{
			string str = Utils.ObjToOidValue(halg);
			HashAlgorithm hashAlgorithm = Utils.ObjToHashAlgorithm(halg);
			byte[] rgbHash = hashAlgorithm.ComputeHash(buffer);
			return SignHash(rgbHash, str);
		}

		public byte[] SignData(byte[] buffer, int offset, int count, object halg)
		{
			string str = Utils.ObjToOidValue(halg);
			HashAlgorithm hashAlgorithm = Utils.ObjToHashAlgorithm(halg);
			byte[] rgbHash = hashAlgorithm.ComputeHash(buffer, offset, count);
			return SignHash(rgbHash, str);
		}

		public bool VerifyData(byte[] buffer, object halg, byte[] signature)
		{
			string str = Utils.ObjToOidValue(halg);
			HashAlgorithm hashAlgorithm = Utils.ObjToHashAlgorithm(halg);
			byte[] rgbHash = hashAlgorithm.ComputeHash(buffer);
			return VerifyHash(rgbHash, str, signature);
		}

		public byte[] SignHash(byte[] rgbHash, string str)
		{
			if (rgbHash == null)
			{
				throw new ArgumentNullException("rgbHash");
			}
			if (PublicOnly)
			{
				throw new CryptographicException(Environment.GetResourceString("Cryptography_CSP_NoPrivateKey"));
			}
			int calgHash = X509Utils.OidToAlgId(str);
			GetKeyPair();
			if (!_randomKeyContainer)
			{
				KeyContainerPermission keyContainerPermission = new KeyContainerPermission(KeyContainerPermissionFlags.NoFlags);
				KeyContainerPermissionAccessEntry accessEntry = new KeyContainerPermissionAccessEntry(_parameters, KeyContainerPermissionFlags.Sign);
				keyContainerPermission.AccessEntries.Add(accessEntry);
				keyContainerPermission.Demand();
			}
			return Utils._SignValue(_safeKeyHandle, _parameters.KeyNumber, 9216, calgHash, rgbHash, 0);
		}

		public bool VerifyHash(byte[] rgbHash, string str, byte[] rgbSignature)
		{
			if (rgbHash == null)
			{
				throw new ArgumentNullException("rgbHash");
			}
			if (rgbSignature == null)
			{
				throw new ArgumentNullException("rgbSignature");
			}
			int calgHash = X509Utils.OidToAlgId(str, OidGroup.HashAlgorithm);
			return VerifyHash(rgbHash, calgHash, rgbSignature);
		}

		internal bool VerifyHash(byte[] rgbHash, int calgHash, byte[] rgbSignature)
		{
			if (rgbHash == null)
			{
				throw new ArgumentNullException("rgbHash");
			}
			if (rgbSignature == null)
			{
				throw new ArgumentNullException("rgbSignature");
			}
			GetKeyPair();
			return Utils._VerifySign(_safeKeyHandle, 9216, calgHash, rgbHash, rgbSignature, 0);
		}

		public byte[] Encrypt(byte[] rgb, bool fOAEP)
		{
			if (rgb == null)
			{
				throw new ArgumentNullException("rgb");
			}
			GetKeyPair();
			byte[] array = null;
			int hr = 0;
			if (fOAEP)
			{
				if (Utils.HasEnhProv != 1 || Utils.Win2KCrypto != 1)
				{
					throw new CryptographicException(Environment.GetResourceString("Cryptography_Padding_Win2KEnhOnly"));
				}
				array = Utils._EncryptPKWin2KEnh(_safeKeyHandle, rgb, fOAEP: true, out hr);
				if (hr != 0)
				{
					throw new CryptographicException(hr);
				}
			}
			else
			{
				array = Utils._EncryptPKWin2KEnh(_safeKeyHandle, rgb, fOAEP: false, out hr);
				if (hr != 0)
				{
					array = Utils._EncryptKey(_safeKeyHandle, rgb);
				}
			}
			return array;
		}

		public byte[] Decrypt(byte[] rgb, bool fOAEP)
		{
			if (rgb == null)
			{
				throw new ArgumentNullException("rgb");
			}
			GetKeyPair();
			if (rgb.Length > KeySize / 8)
			{
				throw new CryptographicException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Cryptography_Padding_DecDataTooBig"), KeySize / 8));
			}
			if (!_randomKeyContainer)
			{
				KeyContainerPermission keyContainerPermission = new KeyContainerPermission(KeyContainerPermissionFlags.NoFlags);
				KeyContainerPermissionAccessEntry accessEntry = new KeyContainerPermissionAccessEntry(_parameters, KeyContainerPermissionFlags.Decrypt);
				keyContainerPermission.AccessEntries.Add(accessEntry);
				keyContainerPermission.Demand();
			}
			byte[] array = null;
			int hr = 0;
			if (fOAEP)
			{
				if (Utils.HasEnhProv != 1 || Utils.Win2KCrypto != 1)
				{
					throw new CryptographicException(Environment.GetResourceString("Cryptography_Padding_Win2KEnhOnly"));
				}
				array = Utils._DecryptPKWin2KEnh(_safeKeyHandle, rgb, fOAEP: true, out hr);
				if (hr != 0)
				{
					throw new CryptographicException(hr);
				}
			}
			else
			{
				array = Utils._DecryptPKWin2KEnh(_safeKeyHandle, rgb, fOAEP: false, out hr);
				if (hr != 0)
				{
					array = Utils._DecryptKey(_safeKeyHandle, rgb, 0);
				}
			}
			return array;
		}

		public override byte[] DecryptValue(byte[] rgb)
		{
			throw new NotSupportedException(Environment.GetResourceString("NotSupported_Method"));
		}

		public override byte[] EncryptValue(byte[] rgb)
		{
			throw new NotSupportedException(Environment.GetResourceString("NotSupported_Method"));
		}

		private static RSAParameters RSAObjectToStruct(RSACspObject rsaCspObject)
		{
			RSAParameters result = default(RSAParameters);
			result.Exponent = rsaCspObject.Exponent;
			result.Modulus = rsaCspObject.Modulus;
			result.P = rsaCspObject.P;
			result.Q = rsaCspObject.Q;
			result.DP = rsaCspObject.DP;
			result.DQ = rsaCspObject.DQ;
			result.InverseQ = rsaCspObject.InverseQ;
			result.D = rsaCspObject.D;
			return result;
		}

		private static RSACspObject RSAStructToObject(RSAParameters rsaParams)
		{
			RSACspObject rSACspObject = new RSACspObject();
			rSACspObject.Exponent = rsaParams.Exponent;
			rSACspObject.Modulus = rsaParams.Modulus;
			rSACspObject.P = rsaParams.P;
			rSACspObject.Q = rsaParams.Q;
			rSACspObject.DP = rsaParams.DP;
			rSACspObject.DQ = rsaParams.DQ;
			rSACspObject.InverseQ = rsaParams.InverseQ;
			rSACspObject.D = rsaParams.D;
			return rSACspObject;
		}

		private static bool IsPublic(RSAParameters rsaParams)
		{
			return rsaParams.P == null;
		}

		private static bool IsPublic(byte[] keyBlob)
		{
			if (keyBlob == null)
			{
				throw new ArgumentNullException("keyBlob");
			}
			if (keyBlob[0] != 6)
			{
				return false;
			}
			if (keyBlob[11] != 49 || keyBlob[10] != 65 || keyBlob[9] != 83 || keyBlob[8] != 82)
			{
				return false;
			}
			return true;
		}
	}
}
