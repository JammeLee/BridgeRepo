using System.Collections;
using System.Globalization;

namespace System.Reflection.Emit
{
	internal sealed class FieldOnTypeBuilderInstantiation : FieldInfo
	{
		private struct Entry
		{
			public FieldInfo m_field;

			public TypeBuilderInstantiation m_type;

			public Entry(FieldInfo Field, TypeBuilderInstantiation type)
			{
				m_field = Field;
				m_type = type;
			}

			public override int GetHashCode()
			{
				return m_field.GetHashCode();
			}

			public override bool Equals(object o)
			{
				if (o is Entry)
				{
					return Equals((Entry)o);
				}
				return false;
			}

			public bool Equals(Entry obj)
			{
				if (obj.m_field == m_field)
				{
					return obj.m_type == m_type;
				}
				return false;
			}
		}

		private static Hashtable m_hashtable = new Hashtable();

		private FieldInfo m_field;

		private TypeBuilderInstantiation m_type;

		internal FieldInfo FieldInfo => m_field;

		public override MemberTypes MemberType => MemberTypes.Field;

		public override string Name => m_field.Name;

		public override Type DeclaringType => m_type;

		public override Type ReflectedType => m_type;

		internal override int MetadataTokenInternal
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public override Module Module => m_field.Module;

		public override RuntimeFieldHandle FieldHandle
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public override Type FieldType
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public override FieldAttributes Attributes => m_field.Attributes;

		internal static FieldInfo GetField(FieldInfo Field, TypeBuilderInstantiation type)
		{
			Entry entry = new Entry(Field, type);
			if (m_hashtable.Contains(entry))
			{
				return m_hashtable[entry] as FieldInfo;
			}
			FieldInfo fieldInfo = new FieldOnTypeBuilderInstantiation(Field, type);
			m_hashtable[entry] = fieldInfo;
			return fieldInfo;
		}

		internal FieldOnTypeBuilderInstantiation(FieldInfo field, TypeBuilderInstantiation type)
		{
			m_field = field;
			m_type = type;
		}

		public override object[] GetCustomAttributes(bool inherit)
		{
			return m_field.GetCustomAttributes(inherit);
		}

		public override object[] GetCustomAttributes(Type attributeType, bool inherit)
		{
			return m_field.GetCustomAttributes(attributeType, inherit);
		}

		public override bool IsDefined(Type attributeType, bool inherit)
		{
			return m_field.IsDefined(attributeType, inherit);
		}

		public new Type GetType()
		{
			return base.GetType();
		}

		public override Type[] GetRequiredCustomModifiers()
		{
			return m_field.GetRequiredCustomModifiers();
		}

		public override Type[] GetOptionalCustomModifiers()
		{
			return m_field.GetOptionalCustomModifiers();
		}

		public override void SetValueDirect(TypedReference obj, object value)
		{
			throw new NotImplementedException();
		}

		public override object GetValueDirect(TypedReference obj)
		{
			throw new NotImplementedException();
		}

		public override object GetValue(object obj)
		{
			throw new InvalidOperationException();
		}

		public override void SetValue(object obj, object value, BindingFlags invokeAttr, Binder binder, CultureInfo culture)
		{
			throw new InvalidOperationException();
		}
	}
}
