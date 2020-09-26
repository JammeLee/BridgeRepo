using System.Globalization;
using System.Runtime.InteropServices;

namespace System.Reflection
{
	[Serializable]
	[ComVisible(true)]
	public struct CustomAttributeNamedArgument
	{
		private MemberInfo m_memberInfo;

		private CustomAttributeTypedArgument m_value;

		internal Type ArgumentType
		{
			get
			{
				if (!(m_memberInfo is FieldInfo))
				{
					return ((PropertyInfo)m_memberInfo).PropertyType;
				}
				return ((FieldInfo)m_memberInfo).FieldType;
			}
		}

		public MemberInfo MemberInfo => m_memberInfo;

		public CustomAttributeTypedArgument TypedValue => m_value;

		public static bool operator ==(CustomAttributeNamedArgument left, CustomAttributeNamedArgument right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(CustomAttributeNamedArgument left, CustomAttributeNamedArgument right)
		{
			return !left.Equals(right);
		}

		internal CustomAttributeNamedArgument(MemberInfo memberInfo, object value)
		{
			m_memberInfo = memberInfo;
			m_value = new CustomAttributeTypedArgument(value);
		}

		internal CustomAttributeNamedArgument(MemberInfo memberInfo, CustomAttributeTypedArgument value)
		{
			m_memberInfo = memberInfo;
			m_value = value;
		}

		public override string ToString()
		{
			return string.Format(CultureInfo.CurrentCulture, "{0} = {1}", MemberInfo.Name, TypedValue.ToString(ArgumentType != typeof(object)));
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			return obj == (object)this;
		}
	}
}
