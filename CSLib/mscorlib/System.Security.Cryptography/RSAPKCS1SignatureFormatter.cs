using System.Runtime.InteropServices;

namespace System.Security.Cryptography
{
	[ComVisible(true)]
	public class RSAPKCS1SignatureFormatter : AsymmetricSignatureFormatter
	{
		private RSA _rsaKey;

		private string _strOID;

		public RSAPKCS1SignatureFormatter()
		{
		}

		public RSAPKCS1SignatureFormatter(AsymmetricAlgorithm key)
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

		public override void SetHashAlgorithm(string strName)
		{
			_strOID = CryptoConfig.MapNameToOID(strName);
		}

		public override byte[] CreateSignature(byte[] rgbHash)
		{
			if (_strOID == null)
			{
				throw new CryptographicUnexpectedOperationException(Environment.GetResourceString("Cryptography_MissingOID"));
			}
			if (_rsaKey == null)
			{
				throw new CryptographicUnexpectedOperationException(Environment.GetResourceString("Cryptography_MissingKey"));
			}
			if (rgbHash == null)
			{
				throw new ArgumentNullException("rgbHash");
			}
			if (_rsaKey is RSACryptoServiceProvider)
			{
				return ((RSACryptoServiceProvider)_rsaKey).SignHash(rgbHash, _strOID);
			}
			byte[] rgb = Utils.RsaPkcs1Padding(_rsaKey, CryptoConfig.EncodeOID(_strOID), rgbHash);
			return _rsaKey.DecryptValue(rgb);
		}
	}
}
