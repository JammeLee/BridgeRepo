using System.Runtime.InteropServices;

namespace System.Reflection.Emit
{
	[Serializable]
	[ComVisible(true)]
	public enum FlowControl
	{
		Branch,
		Break,
		Call,
		Cond_Branch,
		Meta,
		Next,
		[Obsolete("This API has been deprecated. http://go.microsoft.com/fwlink/?linkid=14202")]
		Phi,
		Return,
		Throw
	}
}
