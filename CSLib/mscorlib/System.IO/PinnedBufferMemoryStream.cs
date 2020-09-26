using System.Runtime.InteropServices;

namespace System.IO
{
	internal sealed class PinnedBufferMemoryStream : UnmanagedMemoryStream
	{
		private byte[] _array;

		private GCHandle _pinningHandle;

		internal unsafe PinnedBufferMemoryStream(byte[] array)
		{
			int num = array.Length;
			if (num == 0)
			{
				array = new byte[1];
				num = 0;
			}
			_array = array;
			_pinningHandle = new GCHandle(array, GCHandleType.Pinned);
			fixed (byte* pointer = _array)
			{
				Initialize(pointer, num, num, FileAccess.Read, skipSecurityCheck: true);
			}
		}

		~PinnedBufferMemoryStream()
		{
			Dispose(disposing: false);
		}

		protected override void Dispose(bool disposing)
		{
			if (_isOpen)
			{
				_pinningHandle.Free();
				_isOpen = false;
			}
			base.Dispose(disposing);
		}
	}
}
