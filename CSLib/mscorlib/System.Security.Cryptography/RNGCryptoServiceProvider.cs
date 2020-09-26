using System.Runtime.InteropServices;

namespace System.Security.Cryptography
{
	[ComVisible(true)]
	public sealed class RNGCryptoServiceProvider : RandomNumberGenerator
	{
		private SafeProvHandle m_safeProvHandle;

		public RNGCryptoServiceProvider()
			: this((CspParameters)null)
		{
		}

		public RNGCryptoServiceProvider(string str)
			: this((CspParameters)null)
		{
		}

		public RNGCryptoServiceProvider(byte[] rgb)
			: this((CspParameters)null)
		{
		}

		public RNGCryptoServiceProvider(CspParameters cspParams)
		{
			if (cspParams != null)
			{
				m_safeProvHandle = Utils.AcquireProvHandle(cspParams);
			}
			else
			{
				m_safeProvHandle = Utils.StaticProvHandle;
			}
		}

		public override void GetBytes(byte[] data)
		{
			if (data == null)
			{
				throw new ArgumentNullException("data");
			}
			Utils._GetBytes(m_safeProvHandle, data);
		}

		public override void GetNonZeroBytes(byte[] data)
		{
			if (data == null)
			{
				throw new ArgumentNullException("data");
			}
			Utils._GetNonZeroBytes(m_safeProvHandle, data);
		}
	}
}
