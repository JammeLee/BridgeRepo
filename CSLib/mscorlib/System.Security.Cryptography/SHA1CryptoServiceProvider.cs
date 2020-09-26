using System.Runtime.InteropServices;

namespace System.Security.Cryptography
{
	[ComVisible(true)]
	public sealed class SHA1CryptoServiceProvider : SHA1
	{
		private SafeHashHandle _safeHashHandle;

		public SHA1CryptoServiceProvider()
		{
			SafeHashHandle hKey = SafeHashHandle.InvalidHandle;
			Utils._CreateHash(Utils.StaticProvHandle, 32772, ref hKey);
			_safeHashHandle = hKey;
		}

		protected override void Dispose(bool disposing)
		{
			if (_safeHashHandle != null && !_safeHashHandle.IsClosed)
			{
				_safeHashHandle.Dispose();
			}
			base.Dispose(disposing);
		}

		public override void Initialize()
		{
			if (_safeHashHandle != null && !_safeHashHandle.IsClosed)
			{
				_safeHashHandle.Dispose();
			}
			SafeHashHandle hKey = SafeHashHandle.InvalidHandle;
			Utils._CreateHash(Utils.StaticProvHandle, 32772, ref hKey);
			_safeHashHandle = hKey;
		}

		protected override void HashCore(byte[] rgb, int ibStart, int cbSize)
		{
			Utils._HashData(_safeHashHandle, rgb, ibStart, cbSize);
		}

		protected override byte[] HashFinal()
		{
			return Utils._EndHash(_safeHashHandle);
		}
	}
}
