namespace System.Runtime.InteropServices
{
	[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class, Inherited = false)]
	[ComVisible(true)]
	[Obsolete("This attribute is deprecated and will be removed in a future version.", false)]
	public sealed class IDispatchImplAttribute : Attribute
	{
		internal IDispatchImplType _val;

		public IDispatchImplType Value => _val;

		public IDispatchImplAttribute(IDispatchImplType implType)
		{
			_val = implType;
		}

		public IDispatchImplAttribute(short implType)
		{
			_val = (IDispatchImplType)implType;
		}
	}
}
