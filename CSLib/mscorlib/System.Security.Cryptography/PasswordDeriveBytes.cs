using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;

namespace System.Security.Cryptography
{
	[ComVisible(true)]
	public class PasswordDeriveBytes : DeriveBytes
	{
		private int _extraCount;

		private int _prefix;

		private int _iterations;

		private byte[] _baseValue;

		private byte[] _extra;

		private byte[] _salt;

		private string _hashName;

		private byte[] _password;

		private HashAlgorithm _hash;

		private CspParameters _cspParams;

		private SafeProvHandle _safeProvHandle;

		private SafeProvHandle ProvHandle
		{
			get
			{
				if (_safeProvHandle == null)
				{
					lock (this)
					{
						if (_safeProvHandle == null)
						{
							SafeProvHandle safeProvHandle = Utils.AcquireProvHandle(_cspParams);
							Thread.MemoryBarrier();
							_safeProvHandle = safeProvHandle;
						}
					}
				}
				return _safeProvHandle;
			}
		}

		public string HashName
		{
			get
			{
				return _hashName;
			}
			set
			{
				if (_baseValue != null)
				{
					throw new CryptographicException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Cryptography_PasswordDerivedBytes_ValuesFixed"), "HashName"));
				}
				_hashName = value;
				_hash = (HashAlgorithm)CryptoConfig.CreateFromName(_hashName);
			}
		}

		public int IterationCount
		{
			get
			{
				return _iterations;
			}
			set
			{
				if (_baseValue != null)
				{
					throw new CryptographicException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Cryptography_PasswordDerivedBytes_ValuesFixed"), "IterationCount"));
				}
				if (value <= 0)
				{
					throw new ArgumentOutOfRangeException("value", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
				}
				_iterations = value;
			}
		}

		public byte[] Salt
		{
			get
			{
				if (_salt == null)
				{
					return null;
				}
				return (byte[])_salt.Clone();
			}
			set
			{
				if (_baseValue != null)
				{
					throw new CryptographicException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Cryptography_PasswordDerivedBytes_ValuesFixed"), "Salt"));
				}
				if (value == null)
				{
					_salt = null;
				}
				else
				{
					_salt = (byte[])value.Clone();
				}
			}
		}

		public PasswordDeriveBytes(string strPassword, byte[] rgbSalt)
			: this(strPassword, rgbSalt, new CspParameters())
		{
		}

		public PasswordDeriveBytes(byte[] password, byte[] salt)
			: this(password, salt, new CspParameters())
		{
		}

		public PasswordDeriveBytes(string strPassword, byte[] rgbSalt, string strHashName, int iterations)
			: this(strPassword, rgbSalt, strHashName, iterations, new CspParameters())
		{
		}

		public PasswordDeriveBytes(byte[] password, byte[] salt, string hashName, int iterations)
			: this(password, salt, hashName, iterations, new CspParameters())
		{
		}

		public PasswordDeriveBytes(string strPassword, byte[] rgbSalt, CspParameters cspParams)
			: this(strPassword, rgbSalt, "SHA1", 100, cspParams)
		{
		}

		public PasswordDeriveBytes(byte[] password, byte[] salt, CspParameters cspParams)
			: this(password, salt, "SHA1", 100, cspParams)
		{
		}

		public PasswordDeriveBytes(string strPassword, byte[] rgbSalt, string strHashName, int iterations, CspParameters cspParams)
			: this(new UTF8Encoding(encoderShouldEmitUTF8Identifier: false).GetBytes(strPassword), rgbSalt, strHashName, iterations, cspParams)
		{
		}

		public PasswordDeriveBytes(byte[] password, byte[] salt, string hashName, int iterations, CspParameters cspParams)
		{
			IterationCount = iterations;
			Salt = salt;
			HashName = hashName;
			_password = password;
			_cspParams = cspParams;
		}

		[Obsolete("Rfc2898DeriveBytes replaces PasswordDeriveBytes for deriving key material from a password and is preferred in new applications.")]
		public override byte[] GetBytes(int cb)
		{
			int num = 0;
			byte[] array = new byte[cb];
			if (_baseValue == null)
			{
				ComputeBaseValue();
			}
			else if (_extra != null)
			{
				num = _extra.Length - _extraCount;
				if (num >= cb)
				{
					Buffer.InternalBlockCopy(_extra, _extraCount, array, 0, cb);
					if (num > cb)
					{
						_extraCount += cb;
					}
					else
					{
						_extra = null;
					}
					return array;
				}
				Buffer.InternalBlockCopy(_extra, num, array, 0, num);
				_extra = null;
			}
			byte[] array2 = ComputeBytes(cb - num);
			Buffer.InternalBlockCopy(array2, 0, array, num, cb - num);
			if (array2.Length + num > cb)
			{
				_extra = array2;
				_extraCount = cb - num;
			}
			return array;
		}

		public override void Reset()
		{
			_prefix = 0;
			_extra = null;
			_baseValue = null;
		}

		public byte[] CryptDeriveKey(string algname, string alghashname, int keySize, byte[] rgbIV)
		{
			if (keySize < 0)
			{
				throw new CryptographicException(Environment.GetResourceString("Cryptography_InvalidKeySize"));
			}
			int num = X509Utils.OidToAlgId(alghashname);
			if (num == 0)
			{
				throw new CryptographicException(Environment.GetResourceString("Cryptography_PasswordDerivedBytes_InvalidAlgorithm"));
			}
			int num2 = X509Utils.OidToAlgId(algname);
			if (num2 == 0)
			{
				throw new CryptographicException(Environment.GetResourceString("Cryptography_PasswordDerivedBytes_InvalidAlgorithm"));
			}
			if (rgbIV == null)
			{
				throw new CryptographicException(Environment.GetResourceString("Cryptography_PasswordDerivedBytes_InvalidIV"));
			}
			return Utils._CryptDeriveKey(ProvHandle, num2, num, _password, keySize << 16, rgbIV);
		}

		private byte[] ComputeBaseValue()
		{
			_hash.Initialize();
			_hash.TransformBlock(_password, 0, _password.Length, _password, 0);
			if (_salt != null)
			{
				_hash.TransformBlock(_salt, 0, _salt.Length, _salt, 0);
			}
			_hash.TransformFinalBlock(new byte[0], 0, 0);
			_baseValue = _hash.Hash;
			_hash.Initialize();
			for (int i = 1; i < _iterations - 1; i++)
			{
				_hash.ComputeHash(_baseValue);
				_baseValue = _hash.Hash;
			}
			return _baseValue;
		}

		private byte[] ComputeBytes(int cb)
		{
			int num = 0;
			_hash.Initialize();
			int num2 = _hash.HashSize / 8;
			byte[] array = new byte[(cb + num2 - 1) / num2 * num2];
			CryptoStream cryptoStream = new CryptoStream(Stream.Null, _hash, CryptoStreamMode.Write);
			HashPrefix(cryptoStream);
			cryptoStream.Write(_baseValue, 0, _baseValue.Length);
			cryptoStream.Close();
			Buffer.InternalBlockCopy(_hash.Hash, 0, array, num, num2);
			for (num += num2; cb > num; num += num2)
			{
				_hash.Initialize();
				cryptoStream = new CryptoStream(Stream.Null, _hash, CryptoStreamMode.Write);
				HashPrefix(cryptoStream);
				cryptoStream.Write(_baseValue, 0, _baseValue.Length);
				cryptoStream.Close();
				Buffer.InternalBlockCopy(_hash.Hash, 0, array, num, num2);
			}
			return array;
		}

		private void HashPrefix(CryptoStream cs)
		{
			int num = 0;
			byte[] array = new byte[3]
			{
				48,
				48,
				48
			};
			if (_prefix > 999)
			{
				throw new CryptographicException(Environment.GetResourceString("Cryptography_PasswordDerivedBytes_TooManyBytes"));
			}
			if (_prefix >= 100)
			{
				array[0] += (byte)(_prefix / 100);
				num++;
			}
			if (_prefix >= 10)
			{
				array[num] += (byte)(_prefix % 100 / 10);
				num++;
			}
			if (_prefix > 0)
			{
				array[num] += (byte)(_prefix % 10);
				num++;
				cs.Write(array, 0, num);
			}
			_prefix++;
		}
	}
}
