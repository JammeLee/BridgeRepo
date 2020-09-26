using System.Runtime.InteropServices;

namespace System.Net
{
	[StructLayout(LayoutKind.Sequential)]
	internal class StreamSizes
	{
		public int header;

		public int trailer;

		public int maximumMessage;

		public int buffersCount;

		public int blockSize;

		public static readonly int SizeOf = Marshal.SizeOf(typeof(StreamSizes));

		internal unsafe StreamSizes(byte[] memory)
		{
			fixed (void* value = memory)
			{
				IntPtr ptr = new IntPtr(value);
				try
				{
					header = (int)checked((uint)Marshal.ReadInt32(ptr));
					trailer = (int)checked((uint)Marshal.ReadInt32(ptr, 4));
					maximumMessage = (int)checked((uint)Marshal.ReadInt32(ptr, 8));
					buffersCount = (int)checked((uint)Marshal.ReadInt32(ptr, 12));
					blockSize = (int)checked((uint)Marshal.ReadInt32(ptr, 16));
				}
				catch (OverflowException)
				{
					throw;
				}
			}
		}
	}
}
