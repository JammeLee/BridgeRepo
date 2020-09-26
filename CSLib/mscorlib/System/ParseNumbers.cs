using System.Runtime.CompilerServices;

namespace System
{
	internal static class ParseNumbers
	{
		internal const int PrintAsI1 = 64;

		internal const int PrintAsI2 = 128;

		internal const int PrintAsI4 = 256;

		internal const int TreatAsUnsigned = 512;

		internal const int TreatAsI1 = 1024;

		internal const int TreatAsI2 = 2048;

		internal const int IsTight = 4096;

		internal const int NoSpace = 8192;

		public static long StringToLong(string s, int radix, int flags)
		{
			return StringToLong(s, radix, flags, null);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		public unsafe static extern long StringToLong(string s, int radix, int flags, int* currPos);

		public unsafe static long StringToLong(string s, int radix, int flags, ref int currPos)
		{
			fixed (int* currPos2 = &currPos)
			{
				return StringToLong(s, radix, flags, currPos2);
			}
		}

		public static int StringToInt(string s, int radix, int flags)
		{
			return StringToInt(s, radix, flags, null);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		public unsafe static extern int StringToInt(string s, int radix, int flags, int* currPos);

		public unsafe static int StringToInt(string s, int radix, int flags, ref int currPos)
		{
			fixed (int* currPos2 = &currPos)
			{
				return StringToInt(s, radix, flags, currPos2);
			}
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		public static extern string IntToString(int l, int radix, int width, char paddingChar, int flags);

		[MethodImpl(MethodImplOptions.InternalCall)]
		public static extern string LongToString(long l, int radix, int width, char paddingChar, int flags);
	}
}
