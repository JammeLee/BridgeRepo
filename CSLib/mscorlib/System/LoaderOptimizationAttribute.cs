using System.Runtime.InteropServices;

namespace System
{
	[ComVisible(true)]
	[AttributeUsage(AttributeTargets.Method)]
	public sealed class LoaderOptimizationAttribute : Attribute
	{
		internal byte _val;

		public LoaderOptimization Value => (LoaderOptimization)_val;

		public LoaderOptimizationAttribute(byte value)
		{
			_val = value;
		}

		public LoaderOptimizationAttribute(LoaderOptimization value)
		{
			_val = (byte)value;
		}
	}
}
