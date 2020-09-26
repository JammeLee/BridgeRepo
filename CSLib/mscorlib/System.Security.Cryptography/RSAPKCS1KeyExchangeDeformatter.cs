using System.Runtime.InteropServices;

namespace System.Security.Cryptography
{
	[ComVisible(true)]
	public class RSAPKCS1KeyExchangeDeformatter : AsymmetricKeyExchangeDeformatter
	{
		private RSA _rsaKey;

		private RandomNumberGenerator RngValue;

		public RandomNumberGenerator RNG
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

		public override string Parameters
		{
			get
			{
				return null;
			}
			set
			{
			}
		}

		public RSAPKCS1KeyExchangeDeformatter()
		{
		}

		public RSAPKCS1KeyExchangeDeformatter(AsymmetricAlgorithm key)
		{
			if (key == null)
			{
				throw new ArgumentNullException("key");
			}
			_rsaKey = (RSA)key;
		}

		public override byte[] DecryptKeyExchange(byte[] rgbIn)
		{
			if (_rsaKey == null)
			{
				throw new CryptographicUnexpectedOperationException(Environment.GetResourceString("Cryptography_MissingKey"));
			}
			byte[] array;
			if (_rsaKey is RSACryptoServiceProvider)
			{
				array = ((RSACryptoServiceProvider)_rsaKey).Decrypt(rgbIn, fOAEP: false);
			}
			else
			{
				byte[] array2 = _rsaKey.DecryptValue(rgbIn);
				int i;
				for (i = 2; i < array2.Length && array2[i] != 0; i++)
				{
				}
				if (i >= array2.Length)
				{
					throw new CryptographicUnexpectedOperationException(Environment.GetResourceString("Cryptography_PKCS1Decoding"));
				}
				i++;
				array = new byte[array2.Length - i];
				Buffer.InternalBlockCopy(array2, i, array, 0, array.Length);
			}
			return array;
		}

		public override void SetKey(AsymmetricAlgorithm key)
		{
			if (key == null)
			{
				throw new ArgumentNullException("key");
			}
			_rsaKey = (RSA)key;
		}
	}
}
