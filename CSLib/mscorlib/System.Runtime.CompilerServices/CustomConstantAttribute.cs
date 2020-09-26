using System.Runtime.InteropServices;

namespace System.Runtime.CompilerServices
{
	[Serializable]
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Parameter, Inherited = false)]
	[ComVisible(true)]
	public abstract class CustomConstantAttribute : Attribute
	{
		public abstract object Value
		{
			get;
		}
	}
}
