using System.Runtime.InteropServices;

namespace System
{
	[Serializable]
	[ComVisible(true)]
	[AttributeUsage(AttributeTargets.Field, Inherited = false)]
	public class ThreadStaticAttribute : Attribute
	{
	}
}
