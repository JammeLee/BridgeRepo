using System.Runtime.InteropServices;

namespace System.IO
{
	[Serializable]
	[Flags]
	[ComVisible(true)]
	public enum FileShare
	{
		None = 0x0,
		Read = 0x1,
		Write = 0x2,
		ReadWrite = 0x3,
		Delete = 0x4,
		Inheritable = 0x10
	}
}
