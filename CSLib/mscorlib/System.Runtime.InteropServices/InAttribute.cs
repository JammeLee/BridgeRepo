using System.Reflection;

namespace System.Runtime.InteropServices
{
	[AttributeUsage(AttributeTargets.Parameter, Inherited = false)]
	[ComVisible(true)]
	public sealed class InAttribute : Attribute
	{
		internal static Attribute GetCustomAttribute(ParameterInfo parameter)
		{
			if (!parameter.IsIn)
			{
				return null;
			}
			return new InAttribute();
		}

		internal static bool IsDefined(ParameterInfo parameter)
		{
			return parameter.IsIn;
		}
	}
}
