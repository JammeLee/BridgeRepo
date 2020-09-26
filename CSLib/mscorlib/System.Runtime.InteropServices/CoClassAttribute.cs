namespace System.Runtime.InteropServices
{
	[ComVisible(true)]
	[AttributeUsage(AttributeTargets.Interface, Inherited = false)]
	public sealed class CoClassAttribute : Attribute
	{
		internal Type _CoClass;

		public Type CoClass => _CoClass;

		public CoClassAttribute(Type coClass)
		{
			_CoClass = coClass;
		}
	}
}
