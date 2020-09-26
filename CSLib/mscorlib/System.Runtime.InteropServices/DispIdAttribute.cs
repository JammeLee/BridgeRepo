namespace System.Runtime.InteropServices
{
	[ComVisible(true)]
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Event, Inherited = false)]
	public sealed class DispIdAttribute : Attribute
	{
		internal int _val;

		public int Value => _val;

		public DispIdAttribute(int dispId)
		{
			_val = dispId;
		}
	}
}
