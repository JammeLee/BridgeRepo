using System.Runtime.InteropServices;

namespace System.Runtime.CompilerServices
{
	[Serializable]
	[ComVisible(true)]
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Parameter, Inherited = false)]
	public sealed class IDispatchConstantAttribute : CustomConstantAttribute
	{
		public override object Value => new DispatchWrapper(null);
	}
}
