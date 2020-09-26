using System.Collections;
using System.Globalization;
using System.Runtime.InteropServices;

namespace System.Reflection.Emit
{
	[ComVisible(true)]
	public sealed class GenericTypeParameterBuilder : Type
	{
		internal TypeBuilder m_type;

		public override Type DeclaringType => m_type.DeclaringType;

		public override Type ReflectedType => m_type.ReflectedType;

		public override string Name => m_type.Name;

		public override Module Module => m_type.Module;

		internal override int MetadataTokenInternal => m_type.MetadataTokenInternal;

		public override Guid GUID
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		public override Assembly Assembly => m_type.Assembly;

		public override RuntimeTypeHandle TypeHandle
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		public override string FullName => null;

		public override string Namespace => null;

		public override string AssemblyQualifiedName => null;

		public override Type BaseType => m_type.BaseType;

		public override Type UnderlyingSystemType => this;

		public override bool IsGenericTypeDefinition => false;

		public override bool IsGenericType => false;

		public override bool IsGenericParameter => true;

		public override int GenericParameterPosition => m_type.GenericParameterPosition;

		public override bool ContainsGenericParameters => m_type.ContainsGenericParameters;

		public override MethodBase DeclaringMethod => m_type.DeclaringMethod;

		internal GenericTypeParameterBuilder(TypeBuilder type)
		{
			m_type = type;
		}

		public override string ToString()
		{
			return m_type.Name;
		}

		public override bool Equals(object o)
		{
			GenericTypeParameterBuilder genericTypeParameterBuilder = o as GenericTypeParameterBuilder;
			if (genericTypeParameterBuilder == null)
			{
				return false;
			}
			return genericTypeParameterBuilder.m_type == m_type;
		}

		public override int GetHashCode()
		{
			return m_type.GetHashCode();
		}

		public override Type MakePointerType()
		{
			return SymbolType.FormCompoundType("*".ToCharArray(), this, 0);
		}

		public override Type MakeByRefType()
		{
			return SymbolType.FormCompoundType("&".ToCharArray(), this, 0);
		}

		public override Type MakeArrayType()
		{
			return SymbolType.FormCompoundType("[]".ToCharArray(), this, 0);
		}

		public override Type MakeArrayType(int rank)
		{
			if (rank <= 0)
			{
				throw new IndexOutOfRangeException();
			}
			string text = "";
			if (rank == 1)
			{
				text = "*";
			}
			else
			{
				for (int i = 1; i < rank; i++)
				{
					text += ",";
				}
			}
			string text2 = string.Format(CultureInfo.InvariantCulture, "[{0}]", text);
			return SymbolType.FormCompoundType(text2.ToCharArray(), this, 0) as SymbolType;
		}

		public override object InvokeMember(string name, BindingFlags invokeAttr, Binder binder, object target, object[] args, ParameterModifier[] modifiers, CultureInfo culture, string[] namedParameters)
		{
			throw new NotSupportedException();
		}

		protected override ConstructorInfo GetConstructorImpl(BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
		{
			throw new NotSupportedException();
		}

		[ComVisible(true)]
		public override ConstructorInfo[] GetConstructors(BindingFlags bindingAttr)
		{
			throw new NotSupportedException();
		}

		protected override MethodInfo GetMethodImpl(string name, BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
		{
			throw new NotSupportedException();
		}

		public override MethodInfo[] GetMethods(BindingFlags bindingAttr)
		{
			throw new NotSupportedException();
		}

		public override FieldInfo GetField(string name, BindingFlags bindingAttr)
		{
			throw new NotSupportedException();
		}

		public override FieldInfo[] GetFields(BindingFlags bindingAttr)
		{
			throw new NotSupportedException();
		}

		public override Type GetInterface(string name, bool ignoreCase)
		{
			throw new NotSupportedException();
		}

		public override Type[] GetInterfaces()
		{
			throw new NotSupportedException();
		}

		public override EventInfo GetEvent(string name, BindingFlags bindingAttr)
		{
			throw new NotSupportedException();
		}

		public override EventInfo[] GetEvents()
		{
			throw new NotSupportedException();
		}

		protected override PropertyInfo GetPropertyImpl(string name, BindingFlags bindingAttr, Binder binder, Type returnType, Type[] types, ParameterModifier[] modifiers)
		{
			throw new NotSupportedException();
		}

		public override PropertyInfo[] GetProperties(BindingFlags bindingAttr)
		{
			throw new NotSupportedException();
		}

		public override Type[] GetNestedTypes(BindingFlags bindingAttr)
		{
			throw new NotSupportedException();
		}

		public override Type GetNestedType(string name, BindingFlags bindingAttr)
		{
			throw new NotSupportedException();
		}

		public override MemberInfo[] GetMember(string name, MemberTypes type, BindingFlags bindingAttr)
		{
			throw new NotSupportedException();
		}

		[ComVisible(true)]
		public override InterfaceMapping GetInterfaceMap(Type interfaceType)
		{
			throw new NotSupportedException();
		}

		public override EventInfo[] GetEvents(BindingFlags bindingAttr)
		{
			throw new NotSupportedException();
		}

		public override MemberInfo[] GetMembers(BindingFlags bindingAttr)
		{
			throw new NotSupportedException();
		}

		protected override TypeAttributes GetAttributeFlagsImpl()
		{
			throw new NotSupportedException();
		}

		protected override bool IsArrayImpl()
		{
			return false;
		}

		protected override bool IsByRefImpl()
		{
			return false;
		}

		protected override bool IsPointerImpl()
		{
			return false;
		}

		protected override bool IsPrimitiveImpl()
		{
			return false;
		}

		protected override bool IsCOMObjectImpl()
		{
			return false;
		}

		public override Type GetElementType()
		{
			throw new NotSupportedException();
		}

		protected override bool HasElementTypeImpl()
		{
			return false;
		}

		public override Type[] GetGenericArguments()
		{
			throw new InvalidOperationException();
		}

		public override Type GetGenericTypeDefinition()
		{
			throw new InvalidOperationException();
		}

		public override Type MakeGenericType(params Type[] typeArguments)
		{
			throw new InvalidOperationException(Environment.GetResourceString("Arg_NotGenericTypeDefinition"));
		}

		protected override bool IsValueTypeImpl()
		{
			return false;
		}

		public override bool IsAssignableFrom(Type c)
		{
			throw new NotSupportedException();
		}

		[ComVisible(true)]
		public override bool IsSubclassOf(Type c)
		{
			throw new NotSupportedException();
		}

		public override object[] GetCustomAttributes(bool inherit)
		{
			throw new NotSupportedException();
		}

		public override object[] GetCustomAttributes(Type attributeType, bool inherit)
		{
			throw new NotSupportedException();
		}

		public override bool IsDefined(Type attributeType, bool inherit)
		{
			throw new NotSupportedException();
		}

		public void SetCustomAttribute(ConstructorInfo con, byte[] binaryAttribute)
		{
			if (m_type.m_ca == null)
			{
				m_type.m_ca = new ArrayList();
			}
			m_type.m_ca.Add(new TypeBuilder.CustAttr(con, binaryAttribute));
		}

		public void SetCustomAttribute(CustomAttributeBuilder customBuilder)
		{
			if (m_type.m_ca == null)
			{
				m_type.m_ca = new ArrayList();
			}
			m_type.m_ca.Add(new TypeBuilder.CustAttr(customBuilder));
		}

		public void SetBaseTypeConstraint(Type baseTypeConstraint)
		{
			m_type.CheckContext(baseTypeConstraint);
			m_type.SetParent(baseTypeConstraint);
		}

		[ComVisible(true)]
		public void SetInterfaceConstraints(params Type[] interfaceConstraints)
		{
			m_type.CheckContext(interfaceConstraints);
			m_type.SetInterfaces(interfaceConstraints);
		}

		public void SetGenericParameterAttributes(GenericParameterAttributes genericParameterAttributes)
		{
			m_type.m_genParamAttributes = genericParameterAttributes;
		}
	}
}
