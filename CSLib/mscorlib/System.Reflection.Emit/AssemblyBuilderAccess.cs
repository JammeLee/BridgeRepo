using System.Runtime.InteropServices;

namespace System.Reflection.Emit
{
	[Serializable]
	[ComVisible(true)]
	[Flags]
	public enum AssemblyBuilderAccess
	{
		Run = 0x1,
		Save = 0x2,
		RunAndSave = 0x3,
		ReflectionOnly = 0x6
	}
}
