using System.Runtime.InteropServices;

namespace System.Reflection
{
	[Serializable]
	[ComVisible(true)]
	[Flags]
	public enum CallingConventions
	{
		Standard = 0x1,
		VarArgs = 0x2,
		Any = 0x3,
		HasThis = 0x20,
		ExplicitThis = 0x40
	}
}
