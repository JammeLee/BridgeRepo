using System.Runtime.InteropServices;

namespace System.Security.Policy
{
	[Serializable]
	[Flags]
	[ComVisible(true)]
	public enum PolicyStatementAttribute
	{
		Nothing = 0x0,
		Exclusive = 0x1,
		LevelFinal = 0x2,
		All = 0x3
	}
}
