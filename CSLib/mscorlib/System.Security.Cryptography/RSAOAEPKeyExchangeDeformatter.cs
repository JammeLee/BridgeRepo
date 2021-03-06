using System.Runtime.InteropServices;

namespace System.Security.Cryptography
{
	[ComVisible(true)]
	public class RSAOAEPKeyExchangeDeformatter : AsymmetricKeyExchangeDeformatter
	{
		private RSA _rsaKey;

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

		public RSAOAEPKeyExchangeDeformatter()
		{
		}

		public RSAOAEPKeyExchangeDeformatter(AsymmetricAlgorithm key)
		{
			if (key == null)
			{
				throw new ArgumentNullException("key");
			}
			_rsaKey = (RSA)key;
		}

		public override byte[] DecryptKeyExchange(byte[] rgbData)
		{
			if (_rsaKey == null)
			{
				throw new CryptographicUnexpectedOperationException(Environment.GetResourceString("Cryptography_MissingKey"));
			}
			if (_rsaKey is RSACryptoServiceProvider)
			{
				return ((RSACryptoServiceProvider)_rsaKey).Decrypt(rgbData, fOAEP: true);
			}
			return Utils.RsaOaepDecrypt(_rsaKey, SHA1.Create(), new PKCS1MaskGenerationMethod(), rgbData);
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
