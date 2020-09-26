using System.Runtime.InteropServices;

namespace System.Globalization
{
	[StructLayout(LayoutKind.Sequential, Pack = 2)]
	internal struct EndianessHeader
	{
		internal uint leOffset;

		internal uint beOffset;
	}
}
