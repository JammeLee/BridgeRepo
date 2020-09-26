using System.Runtime.InteropServices;

namespace System.CodeDom
{
	[Serializable]
	[ComVisible(true)]
	[Flags]
	public enum CodeTypeReferenceOptions
	{
		GlobalReference = 0x1,
		GenericTypeParameter = 0x2
	}
}
