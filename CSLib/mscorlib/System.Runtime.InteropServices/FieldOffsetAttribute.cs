using System.Reflection;

namespace System.Runtime.InteropServices
{
	[AttributeUsage(AttributeTargets.Field, Inherited = false)]
	[ComVisible(true)]
	public sealed class FieldOffsetAttribute : Attribute
	{
		internal int _val;

		public int Value => _val;

		internal static Attribute GetCustomAttribute(RuntimeFieldInfo field)
		{
			if (field.DeclaringType != null && field.Module.MetadataImport.GetFieldOffset(field.DeclaringType.MetadataToken, field.MetadataToken, out var offset))
			{
				return new FieldOffsetAttribute(offset);
			}
			return null;
		}

		internal static bool IsDefined(RuntimeFieldInfo field)
		{
			return GetCustomAttribute(field) != null;
		}

		public FieldOffsetAttribute(int offset)
		{
			_val = offset;
		}
	}
}
