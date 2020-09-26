using System.Runtime.InteropServices;

namespace System.Net
{
	[StructLayout(LayoutKind.Sequential)]
	internal class SecSizes
	{
		public readonly int MaxToken;

		public readonly int MaxSignature;

		public readonly int BlockSize;

		public readonly int SecurityTrailer;

		public static readonly int SizeOf = Marshal.SizeOf(typeof(SecSizes));

		internal unsafe SecSizes(byte[] memory)
		{
			fixed (void* value = memory)
			{
				IntPtr ptr = new IntPtr(value);
				try
				{
					MaxToken = (int)checked((uint)Marshal.ReadInt32(ptr));
					MaxSignature = (int)checked((uint)Marshal.ReadInt32(ptr, 4));
					BlockSize = (int)checked((uint)Marshal.ReadInt32(ptr, 8));
					SecurityTrailer = (int)checked((uint)Marshal.ReadInt32(ptr, 12));
				}
				catch (OverflowException)
				{
					throw;
				}
			}
		}
	}
}
