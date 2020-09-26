using System.Runtime.InteropServices;

namespace System.Runtime.Serialization
{
	[Serializable]
	[Flags]
	[ComVisible(true)]
	public enum StreamingContextStates
	{
		CrossProcess = 0x1,
		CrossMachine = 0x2,
		File = 0x4,
		Persistence = 0x8,
		Remoting = 0x10,
		Other = 0x20,
		Clone = 0x40,
		CrossAppDomain = 0x80,
		All = 0xFF
	}
}
