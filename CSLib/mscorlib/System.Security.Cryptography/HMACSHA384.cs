using System.Runtime.InteropServices;

namespace System.Security.Cryptography
{
	[ComVisible(true)]
	public class HMACSHA384 : HMAC
	{
		private bool m_useLegacyBlockSize = Utils._ProduceLegacyHmacValues();

		private int BlockSize
		{
			get
			{
				if (!m_useLegacyBlockSize)
				{
					return 128;
				}
				return 64;
			}
		}

		public bool ProduceLegacyHmacValues
		{
			get
			{
				return m_useLegacyBlockSize;
			}
			set
			{
				m_useLegacyBlockSize = value;
				base.BlockSizeValue = BlockSize;
				InitializeKey(KeyValue);
			}
		}

		public HMACSHA384()
			: this(Utils.GenerateRandom(128))
		{
		}

		public HMACSHA384(byte[] key)
		{
			Utils._ShowLegacyHmacWarning();
			m_hashName = "SHA384";
			m_hash1 = new SHA384Managed();
			m_hash2 = new SHA384Managed();
			HashSizeValue = 384;
			base.BlockSizeValue = BlockSize;
			InitializeKey(key);
		}
	}
}
