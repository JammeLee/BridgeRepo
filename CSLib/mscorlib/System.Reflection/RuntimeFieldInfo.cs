using System.Runtime.Serialization;

namespace System.Reflection
{
	[Serializable]
	internal abstract class RuntimeFieldInfo : FieldInfo
	{
		private BindingFlags m_bindingFlags;

		protected RuntimeType.RuntimeTypeCache m_reflectedTypeCache;

		protected RuntimeType m_declaringType;

		internal BindingFlags BindingFlags => m_bindingFlags;

		private RuntimeTypeHandle ReflectedTypeHandle => m_reflectedTypeCache.RuntimeTypeHandle;

		internal RuntimeTypeHandle DeclaringTypeHandle => DeclaringType?.GetTypeHandleInternal() ?? Module.GetModuleHandle().GetModuleTypeHandle();

		public override MemberTypes MemberType => MemberTypes.Field;

		public override Type ReflectedType
		{
			get
			{
				if (!m_reflectedTypeCache.IsGlobal)
				{
					return m_reflectedTypeCache.RuntimeType;
				}
				return null;
			}
		}

		public override Type DeclaringType
		{
			get
			{
				if (!m_reflectedTypeCache.IsGlobal)
				{
					return m_declaringType;
				}
				return null;
			}
		}

		protected RuntimeFieldInfo()
		{
		}

		protected RuntimeFieldInfo(RuntimeType.RuntimeTypeCache reflectedTypeCache, RuntimeType declaringType, BindingFlags bindingFlags)
		{
			m_bindingFlags = bindingFlags;
			m_declaringType = declaringType;
			m_reflectedTypeCache = reflectedTypeCache;
		}

		internal virtual RuntimeFieldHandle GetFieldHandle()
		{
			return FieldHandle;
		}

		public override string ToString()
		{
			return FieldType.SigToString() + " " + Name;
		}

		public override object[] GetCustomAttributes(bool inherit)
		{
			return CustomAttribute.GetCustomAttributes(this, typeof(object) as RuntimeType);
		}

		public override object[] GetCustomAttributes(Type attributeType, bool inherit)
		{
			if (attributeType == null)
			{
				throw new ArgumentNullException("attributeType");
			}
			RuntimeType runtimeType = attributeType.UnderlyingSystemType as RuntimeType;
			if (runtimeType == null)
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_MustBeType"), "attributeType");
			}
			return CustomAttribute.GetCustomAttributes(this, runtimeType);
		}

		public override bool IsDefined(Type attributeType, bool inherit)
		{
			if (attributeType == null)
			{
				throw new ArgumentNullException("attributeType");
			}
			RuntimeType runtimeType = attributeType.UnderlyingSystemType as RuntimeType;
			if (runtimeType == null)
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_MustBeType"), "attributeType");
			}
			return CustomAttribute.IsDefined(this, runtimeType);
		}

		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			MemberInfoSerializationHolder.GetSerializationInfo(info, Name, ReflectedType, ToString(), MemberTypes.Field);
		}
	}
}
