using System.Globalization;
using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace System.Reflection.Emit
{
	[ComVisible(true)]
	[ComDefaultInterface(typeof(_PropertyBuilder))]
	[ClassInterface(ClassInterfaceType.None)]
	[HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort = true)]
	public sealed class PropertyBuilder : PropertyInfo, _PropertyBuilder
	{
		private string m_name;

		private PropertyToken m_prToken;

		private int m_tkProperty;

		private Module m_module;

		private SignatureHelper m_signature;

		private PropertyAttributes m_attributes;

		private Type m_returnType;

		private MethodInfo m_getMethod;

		private MethodInfo m_setMethod;

		private TypeBuilder m_containingType;

		public PropertyToken PropertyToken => m_prToken;

		internal override int MetadataTokenInternal => m_tkProperty;

		public override Module Module => m_containingType.Module;

		public override Type PropertyType => m_returnType;

		public override PropertyAttributes Attributes => m_attributes;

		public override bool CanRead
		{
			get
			{
				if (m_getMethod != null)
				{
					return true;
				}
				return false;
			}
		}

		public override bool CanWrite
		{
			get
			{
				if (m_setMethod != null)
				{
					return true;
				}
				return false;
			}
		}

		public override string Name => m_name;

		public override Type DeclaringType => m_containingType;

		public override Type ReflectedType => m_containingType;

		private PropertyBuilder()
		{
		}

		internal PropertyBuilder(Module mod, string name, SignatureHelper sig, PropertyAttributes attr, Type returnType, PropertyToken prToken, TypeBuilder containingType)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			if (name.Length == 0)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_EmptyName"), "name");
			}
			if (name[0] == '\0')
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_IllegalName"), "name");
			}
			m_name = name;
			m_module = mod;
			m_signature = sig;
			m_attributes = attr;
			m_returnType = returnType;
			m_prToken = prToken;
			m_tkProperty = prToken.Token;
			m_getMethod = null;
			m_setMethod = null;
			m_containingType = containingType;
		}

		public void SetConstant(object defaultValue)
		{
			m_containingType.ThrowIfCreated();
			TypeBuilder.SetConstantValue(m_module, m_prToken.Token, m_returnType, defaultValue);
		}

		public void SetGetMethod(MethodBuilder mdBuilder)
		{
			if (mdBuilder == null)
			{
				throw new ArgumentNullException("mdBuilder");
			}
			m_containingType.ThrowIfCreated();
			TypeBuilder.InternalDefineMethodSemantics(m_module, m_prToken.Token, MethodSemanticsAttributes.Getter, mdBuilder.GetToken().Token);
			m_getMethod = mdBuilder;
		}

		public void SetSetMethod(MethodBuilder mdBuilder)
		{
			if (mdBuilder == null)
			{
				throw new ArgumentNullException("mdBuilder");
			}
			m_containingType.ThrowIfCreated();
			TypeBuilder.InternalDefineMethodSemantics(m_module, m_prToken.Token, MethodSemanticsAttributes.Setter, mdBuilder.GetToken().Token);
			m_setMethod = mdBuilder;
		}

		public void AddOtherMethod(MethodBuilder mdBuilder)
		{
			if (mdBuilder == null)
			{
				throw new ArgumentNullException("mdBuilder");
			}
			m_containingType.ThrowIfCreated();
			TypeBuilder.InternalDefineMethodSemantics(m_module, m_prToken.Token, MethodSemanticsAttributes.Other, mdBuilder.GetToken().Token);
		}

		[ComVisible(true)]
		public void SetCustomAttribute(ConstructorInfo con, byte[] binaryAttribute)
		{
			if (con == null)
			{
				throw new ArgumentNullException("con");
			}
			if (binaryAttribute == null)
			{
				throw new ArgumentNullException("binaryAttribute");
			}
			m_containingType.ThrowIfCreated();
			TypeBuilder.InternalCreateCustomAttribute(m_prToken.Token, ((ModuleBuilder)m_module).GetConstructorToken(con).Token, binaryAttribute, m_module, toDisk: false);
		}

		public void SetCustomAttribute(CustomAttributeBuilder customBuilder)
		{
			if (customBuilder == null)
			{
				throw new ArgumentNullException("customBuilder");
			}
			m_containingType.ThrowIfCreated();
			customBuilder.CreateCustomAttribute((ModuleBuilder)m_module, m_prToken.Token);
		}

		public override object GetValue(object obj, object[] index)
		{
			throw new NotSupportedException(Environment.GetResourceString("NotSupported_DynamicModule"));
		}

		public override object GetValue(object obj, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture)
		{
			throw new NotSupportedException(Environment.GetResourceString("NotSupported_DynamicModule"));
		}

		public override void SetValue(object obj, object value, object[] index)
		{
			throw new NotSupportedException(Environment.GetResourceString("NotSupported_DynamicModule"));
		}

		public override void SetValue(object obj, object value, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture)
		{
			throw new NotSupportedException(Environment.GetResourceString("NotSupported_DynamicModule"));
		}

		public override MethodInfo[] GetAccessors(bool nonPublic)
		{
			throw new NotSupportedException(Environment.GetResourceString("NotSupported_DynamicModule"));
		}

		public override MethodInfo GetGetMethod(bool nonPublic)
		{
			if (nonPublic || m_getMethod == null)
			{
				return m_getMethod;
			}
			if ((m_getMethod.Attributes & MethodAttributes.Public) == MethodAttributes.Public)
			{
				return m_getMethod;
			}
			return null;
		}

		public override MethodInfo GetSetMethod(bool nonPublic)
		{
			if (nonPublic || m_setMethod == null)
			{
				return m_setMethod;
			}
			if ((m_setMethod.Attributes & MethodAttributes.Public) == MethodAttributes.Public)
			{
				return m_setMethod;
			}
			return null;
		}

		public override ParameterInfo[] GetIndexParameters()
		{
			throw new NotSupportedException(Environment.GetResourceString("NotSupported_DynamicModule"));
		}

		public override object[] GetCustomAttributes(bool inherit)
		{
			throw new NotSupportedException(Environment.GetResourceString("NotSupported_DynamicModule"));
		}

		public override object[] GetCustomAttributes(Type attributeType, bool inherit)
		{
			throw new NotSupportedException(Environment.GetResourceString("NotSupported_DynamicModule"));
		}

		public override bool IsDefined(Type attributeType, bool inherit)
		{
			throw new NotSupportedException(Environment.GetResourceString("NotSupported_DynamicModule"));
		}

		void _PropertyBuilder.GetTypeInfoCount(out uint pcTInfo)
		{
			throw new NotImplementedException();
		}

		void _PropertyBuilder.GetTypeInfo(uint iTInfo, uint lcid, IntPtr ppTInfo)
		{
			throw new NotImplementedException();
		}

		void _PropertyBuilder.GetIDsOfNames([In] ref Guid riid, IntPtr rgszNames, uint cNames, uint lcid, IntPtr rgDispId)
		{
			throw new NotImplementedException();
		}

		void _PropertyBuilder.Invoke(uint dispIdMember, [In] ref Guid riid, uint lcid, short wFlags, IntPtr pDispParams, IntPtr pVarResult, IntPtr pExcepInfo, IntPtr puArgErr)
		{
			throw new NotImplementedException();
		}
	}
}
