using System.Runtime.InteropServices;

namespace System.Diagnostics
{
	[Serializable]
	[AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Method, Inherited = false)]
	[ComVisible(true)]
	public sealed class DebuggerStepperBoundaryAttribute : Attribute
	{
	}
}
