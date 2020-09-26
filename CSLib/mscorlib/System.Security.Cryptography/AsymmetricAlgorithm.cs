using System.Runtime.InteropServices;

namespace System.Security.Cryptography
{
	[ComVisible(true)]
	public abstract class AsymmetricAlgorithm : IDisposable
	{
		protected int KeySizeValue;

		protected KeySizes[] LegalKeySizesValue;

		public virtual int KeySize
		{
			get
			{
				return KeySizeValue;
			}
			set
			{
				for (int i = 0; i < LegalKeySizesValue.Length; i++)
				{
					if (LegalKeySizesValue[i].SkipSize == 0)
					{
						if (LegalKeySizesValue[i].MinSize == value)
						{
							KeySizeValue = value;
							return;
						}
						continue;
					}
					for (int j = LegalKeySizesValue[i].MinSize; j <= LegalKeySizesValue[i].MaxSize; j += LegalKeySizesValue[i].SkipSize)
					{
						if (j == value)
						{
							KeySizeValue = value;
							return;
						}
					}
				}
				throw new CryptographicException(Environment.GetResourceString("Cryptography_InvalidKeySize"));
			}
		}

		public virtual KeySizes[] LegalKeySizes => (KeySizes[])LegalKeySizesValue.Clone();

		public abstract string SignatureAlgorithm
		{
			get;
		}

		public abstract string KeyExchangeAlgorithm
		{
			get;
		}

		void IDisposable.Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}

		public void Clear()
		{
			((IDisposable)this).Dispose();
		}

		protected abstract void Dispose(bool disposing);

		public static AsymmetricAlgorithm Create()
		{
			return Create("System.Security.Cryptography.AsymmetricAlgorithm");
		}

		public static AsymmetricAlgorithm Create(string algName)
		{
			return (AsymmetricAlgorithm)CryptoConfig.CreateFromName(algName);
		}

		public abstract void FromXmlString(string xmlString);

		public abstract string ToXmlString(bool includePrivateParameters);
	}
}
