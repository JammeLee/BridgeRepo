using System.Runtime.InteropServices;

namespace System.Security.Cryptography
{
	[ComVisible(true)]
	public sealed class RijndaelManaged : Rijndael
	{
		public RijndaelManaged()
		{
			if (Utils.FipsAlgorithmPolicy == 1)
			{
				throw new InvalidOperationException(Environment.GetResourceString("Cryptography_NonCompliantFIPSAlgorithm"));
			}
		}

		public override ICryptoTransform CreateEncryptor(byte[] rgbKey, byte[] rgbIV)
		{
			return NewEncryptor(rgbKey, ModeValue, rgbIV, FeedbackSizeValue, RijndaelManagedTransformMode.Encrypt);
		}

		public override ICryptoTransform CreateDecryptor(byte[] rgbKey, byte[] rgbIV)
		{
			return NewEncryptor(rgbKey, ModeValue, rgbIV, FeedbackSizeValue, RijndaelManagedTransformMode.Decrypt);
		}

		public override void GenerateKey()
		{
			KeyValue = new byte[KeySizeValue / 8];
			Utils.StaticRandomNumberGenerator.GetBytes(KeyValue);
		}

		public override void GenerateIV()
		{
			IVValue = new byte[BlockSizeValue / 8];
			Utils.StaticRandomNumberGenerator.GetBytes(IVValue);
		}

		private ICryptoTransform NewEncryptor(byte[] rgbKey, CipherMode mode, byte[] rgbIV, int feedbackSize, RijndaelManagedTransformMode encryptMode)
		{
			if (rgbKey == null)
			{
				rgbKey = new byte[KeySizeValue / 8];
				Utils.StaticRandomNumberGenerator.GetBytes(rgbKey);
			}
			if (mode != CipherMode.ECB && rgbIV == null)
			{
				rgbIV = new byte[BlockSizeValue / 8];
				Utils.StaticRandomNumberGenerator.GetBytes(rgbIV);
			}
			return new RijndaelManagedTransform(rgbKey, mode, rgbIV, BlockSizeValue, feedbackSize, PaddingValue, encryptMode);
		}
	}
}
