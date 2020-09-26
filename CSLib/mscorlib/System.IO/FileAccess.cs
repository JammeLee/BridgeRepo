using System.Runtime.InteropServices;

namespace System.IO
{
	[Serializable]
	[Flags]
	[ComVisible(true)]
	public enum FileAccess
	{
		Read = 0x1,
		Write = 0x2,
		ReadWrite = 0x3
	}
}
