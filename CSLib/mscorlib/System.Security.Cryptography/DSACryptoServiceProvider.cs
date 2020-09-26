using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Security.Permissions;

namespace System.Security.Cryptography
{
	[ComVisible(true)]
	public sealed class DSACryptoServiceProvider : DSA, ICspAsymmetricAlgorithm
	{
		private int _dwKeySize;

		private CspParameters _parameters;

		private bool _randomKeyContainer;

		private SafeProvHandle _safeProvHandle;

		private SafeKeyHandle _safeKeyHandle;

		private SHA1CryptoServiceProvider _sha1;

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

		public override string KeyExchangeAlgorithm => null;

		public override string SignatureAlgorithm => "http://www.w3.org/2000/09/xmldsig#dsa-sha1";

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

		public DSACryptoServiceProvider()
			: this(0, new CspParameters(13, null, null, s_UseMachineKeyStore))
		{
		}

		public DSACryptoServiceProvider(int dwKeySize)
			: this(dwKeySize, new CspParameters(13, null, null, s_UseMachineKeyStore))
		{
		}

		public DSACryptoServiceProvider(CspParameters parameters)
			: this(0, parameters)
		{
		}

		public DSACryptoServiceProvider(int dwKeySize, CspParameters parameters)
		{
			if (dwKeySize < 0)
			{
				throw new ArgumentOutOfRangeException("dwKeySize", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
			}
			_parameters = Utils.SaveCspParameters(CspAlgorithmType.Dss, parameters, s_UseMachineKeyStore, ref _randomKeyContainer);
			LegalKeySizesValue = new KeySizes[1]
			{
				new KeySizes(512, 1024, 64)
			};
			_dwKeySize = dwKeySize;
			_sha1 = new SHA1CryptoServiceProvider();
			if (!_randomKeyContainer || Environment.GetCompatibilityFlag(CompatibilityFlag.EagerlyGenerateRandomAsymmKeys))
			{
				GetKeyPair();
			}
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
					Utils.GetKeyPairHelper(CspAlgorithmType.Dss, _parameters, _randomKeyContainer, _dwKeySize, ref _safeProvHandle, ref _safeKeyHandle);
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

		public override DSAParameters ExportParameters(bool includePrivateParameters)
		{
			GetKeyPair();
			if (includePrivateParameters)
			{
				KeyContainerPermission keyContainerPermission = new KeyContainerPermission(KeyContainerPermissionFlags.NoFlags);
				KeyContainerPermissionAccessEntry accessEntry = new KeyContainerPermissionAccessEntry(_parameters, KeyContainerPermissionFlags.Export);
				keyContainerPermission.AccessEntries.Add(accessEntry);
				keyContainerPermission.Demand();
			}
			DSACspObject dSACspObject = new DSACspObject();
			int blobType = (includePrivateParameters ? 7 : 6);
			Utils._ExportKey(_safeKeyHandle, blobType, dSACspObject);
			return DSAObjectToStruct(dSACspObject);
		}

		[ComVisible(false)]
		public byte[] ExportCspBlob(bool includePrivateParameters)
		{
			GetKeyPair();
			return Utils.ExportCspBlobHelper(includePrivateParameters, _parameters, _safeKeyHandle);
		}

		public override void ImportParameters(DSAParameters parameters)
		{
			DSACspObject cspObject = DSAStructToObject(parameters);
			if (_safeKeyHandle != null && !_safeKeyHandle.IsClosed)
			{
				_safeKeyHandle.Dispose();
			}
			_safeKeyHandle = SafeKeyHandle.InvalidHandle;
			if (IsPublic(parameters))
			{
				Utils._ImportKey(Utils.StaticDssProvHandle, 8704, CspProviderFlags.NoFlags, cspObject, ref _safeKeyHandle);
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
			Utils._ImportKey(_safeProvHandle, 8704, _parameters.Flags, cspObject, ref _safeKeyHandle);
		}

		[ComVisible(false)]
		public void ImportCspBlob(byte[] keyBlob)
		{
			Utils.ImportCspBlobHelper(CspAlgorithmType.Dss, keyBlob, IsPublic(keyBlob), ref _parameters, _randomKeyContainer, ref _safeProvHandle, ref _safeKeyHandle);
		}

		public byte[] SignData(Stream inputStream)
		{
			byte[] rgbHash = _sha1.ComputeHash(inputStream);
			return SignHash(rgbHash, null);
		}

		public byte[] SignData(byte[] buffer)
		{
			byte[] rgbHash = _sha1.ComputeHash(buffer);
			return SignHash(rgbHash, null);
		}

		public byte[] SignData(byte[] buffer, int offset, int count)
		{
			byte[] rgbHash = _sha1.ComputeHash(buffer, offset, count);
			return SignHash(rgbHash, null);
		}

		public bool VerifyData(byte[] rgbData, byte[] rgbSignature)
		{
			byte[] rgbHash = _sha1.ComputeHash(rgbData);
			return VerifyHash(rgbHash, null, rgbSignature);
		}

		public override byte[] CreateSignature(byte[] rgbHash)
		{
			return SignHash(rgbHash, null);
		}

		public override bool VerifySignature(byte[] rgbHash, byte[] rgbSignature)
		{
			return VerifyHash(rgbHash, null, rgbSignature);
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
			if (rgbHash.Length != _sha1.HashSize / 8)
			{
				throw new CryptographicException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Cryptography_InvalidHashSize"), "SHA1", _sha1.HashSize / 8));
			}
			GetKeyPair();
			if (!CspKeyContainerInfo.RandomlyGenerated)
			{
				KeyContainerPermission keyContainerPermission = new KeyContainerPermission(KeyContainerPermissionFlags.NoFlags);
				KeyContainerPermissionAccessEntry accessEntry = new KeyContainerPermissionAccessEntry(_parameters, KeyContainerPermissionFlags.Sign);
				keyContainerPermission.AccessEntries.Add(accessEntry);
				keyContainerPermission.Demand();
			}
			return Utils._SignValue(_safeKeyHandle, _parameters.KeyNumber, 8704, calgHash, rgbHash, 0);
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
			int calgHash = X509Utils.OidToAlgId(str);
			if (rgbHash.Length != _sha1.HashSize / 8)
			{
				throw new CryptographicException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Cryptography_InvalidHashSize"), "SHA1", _sha1.HashSize / 8));
			}
			GetKeyPair();
			return Utils._VerifySign(_safeKeyHandle, 8704, calgHash, rgbHash, rgbSignature, 0);
		}

		private static DSAParameters DSAObjectToStruct(DSACspObject dsaCspObject)
		{
			DSAParameters result = default(DSAParameters);
			result.P = dsaCspObject.P;
			result.Q = dsaCspObject.Q;
			result.G = dsaCspObject.G;
			result.Y = dsaCspObject.Y;
			result.J = dsaCspObject.J;
			result.X = dsaCspObject.X;
			result.Seed = dsaCspObject.Seed;
			result.Counter = dsaCspObject.Counter;
			return result;
		}

		private static DSACspObject DSAStructToObject(DSAParameters dsaParams)
		{
			DSACspObject dSACspObject = new DSACspObject();
			dSACspObject.P = dsaParams.P;
			dSACspObject.Q = dsaParams.Q;
			dSACspObject.G = dsaParams.G;
			dSACspObject.Y = dsaParams.Y;
			dSACspObject.J = dsaParams.J;
			dSACspObject.X = dsaParams.X;
			dSACspObject.Seed = dsaParams.Seed;
			dSACspObject.Counter = dsaParams.Counter;
			return dSACspObject;
		}

		private static bool IsPublic(DSAParameters dsaParams)
		{
			return dsaParams.X == null;
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
			if ((keyBlob[11] != 49 && keyBlob[11] != 51) || keyBlob[10] != 83 || keyBlob[9] != 83 || keyBlob[8] != 68)
			{
				return false;
			}
			return true;
		}
	}
}
