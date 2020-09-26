using System.Globalization;
using System.Runtime.InteropServices;

namespace System.Security.Cryptography
{
	[ComVisible(true)]
	public class RSAPKCS1KeyExchangeFormatter : AsymmetricKeyExchangeFormatter
	{
		private RandomNumberGenerator RngValue;

		private RSA _rsaKey;

		public override string Parameters => "<enc:KeyEncryptionMethod enc:Algorithm=\"http://www.microsoft.com/xml/security/algorithm/PKCS1-v1.5-KeyEx\" xmlns:enc=\"http://www.microsoft.com/xml/security/encryption/v1.0\" />";

		public RandomNumberGenerator Rng
		{
			get
			{
				return RngValue;
			}
			set
			{
				RngValue = value;
			}
		}

		public RSAPKCS1KeyExchangeFormatter()
		{
		}

		public RSAPKCS1KeyExchangeFormatter(AsymmetricAlgorithm key)
		{
			if (key == null)
			{
				throw new ArgumentNullException("key");
			}
			_rsaKey = (RSA)key;
		}

		public override void SetKey(AsymmetricAlgorithm key)
		{
			if (key == null)
			{
				throw new ArgumentNullException("key");
			}
			_rsaKey = (RSA)key;
		}

		public override byte[] CreateKeyExchange(byte[] rgbData)
		{
			if (_rsaKey == null)
			{
				throw new CryptographicUnexpectedOperationException(Environment.GetResourceString("Cryptography_MissingKey"));
			}
			if (_rsaKey is RSACryptoServiceProvider)
			{
				return ((RSACryptoServiceProvider)_rsaKey).Encrypt(rgbData, fOAEP: false);
			}
			int num = _rsaKey.KeySize / 8;
			if (rgbData.Length + 11 > num)
			{
				throw new CryptographicException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Cryptography_Padding_EncDataTooBig"), num - 11));
			}
			byte[] array = new byte[num];
			if (RngValue == null)
			{
				RngValue = RandomNumberGenerator.Create();
			}
			Rng.GetNonZeroBytes(array);
			array[0] = 0;
			array[1] = 2;
			array[num - rgbData.Length - 1] = 0;
			Buffer.InternalBlockCopy(rgbData, 0, array, num - rgbData.Length, rgbData.Length);
			return _rsaKey.EncryptValue(array);
		}

		public override byte[] CreateKeyExchange(byte[] rgbData, Type symAlgType)
		{
			return CreateKeyExchange(rgbData);
		}
	}
}
