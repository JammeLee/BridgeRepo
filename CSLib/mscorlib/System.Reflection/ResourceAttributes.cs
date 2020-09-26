using System.Runtime.InteropServices;

namespace System.Reflection
{
	[Serializable]
	[Flags]
	[ComVisible(true)]
	public enum ResourceAttributes
	{
		Public = 0x1,
		Private = 0x2
	}
}
