using System.Runtime.InteropServices;

namespace System.Reflection.Emit
{
	[Serializable]
	[ComVisible(true)]
	public enum OpCodeType
	{
		[Obsolete("This API has been deprecated. http://go.microsoft.com/fwlink/?linkid=14202")]
		Annotation,
		Macro,
		Nternal,
		Objmodel,
		Prefix,
		Primitive
	}
}
