using System.Globalization;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;

namespace System.Reflection.Emit
{
	[ClassInterface(ClassInterfaceType.None)]
	[ComVisible(true)]
	[ComDefaultInterface(typeof(_ConstructorBuilder))]
	[HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort = true)]
	public sealed class ConstructorBuilder : ConstructorInfo, _ConstructorBuilder
	{
		internal MethodBuilder m_methodBuilder;

		internal bool m_ReturnILGen;

		internal override int MetadataTokenInternal => m_methodBuilder.MetadataTokenInternal;

		public override Module Module => m_methodBuilder.Module;

		public override Type ReflectedType => m_methodBuilder.ReflectedType;

		public override Type DeclaringType => m_methodBuilder.DeclaringType;

		public override string Name => m_methodBuilder.Name;

		public override MethodAttributes Attributes => m_methodBuilder.Attributes;

		public override RuntimeMethodHandle MethodHandle => m_methodBuilder.MethodHandle;

		public override CallingConventions CallingConvention
		{
			get
			{
				if (DeclaringType.IsGenericType)
				{
					return CallingConventions.HasThis;
				}
				return CallingConventions.Standard;
			}
		}

		public Type ReturnType => m_methodBuilder.GetReturnType();

		public string Signature => m_methodBuilder.Signature;

		public bool InitLocals
		{
			get
			{
				return m_methodBuilder.InitLocals;
			}
			set
			{
				m_methodBuilder.InitLocals = value;
			}
		}

		private ConstructorBuilder()
		{
		}

		internal ConstructorBuilder(string name, MethodAttributes attributes, CallingConventions callingConvention, Type[] parameterTypes, Type[][] requiredCustomModifiers, Type[][] optionalCustomModifiers, Module mod, TypeBuilder type)
		{
			m_methodBuilder = new MethodBuilder(name, attributes, callingConvention, null, null, null, parameterTypes, requiredCustomModifiers, optionalCustomModifiers, mod, type, bIsGlobalMethod: false);
			type.m_listMethods.Add(m_methodBuilder);
			m_methodBuilder.GetMethodSignature().InternalGetSignature(out var _);
			m_methodBuilder.GetToken();
			m_ReturnILGen = true;
		}

		internal ConstructorBuilder(string name, MethodAttributes attributes, CallingConventions callingConvention, Type[] parameterTypes, Module mod, TypeBuilder type)
			: this(name, attributes, callingConvention, parameterTypes, null, null, mod, type)
		{
		}

		internal override Type[] GetParameterTypes()
		{
			return m_methodBuilder.GetParameterTypes();
		}

		public override string ToString()
		{
			return m_methodBuilder.ToString();
		}

		public override object Invoke(object obj, BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture)
		{
			throw new NotSupportedException(Environment.GetResourceString("NotSupported_DynamicModule"));
		}

		public override ParameterInfo[] GetParameters()
		{
			if (!m_methodBuilder.m_bIsBaked)
			{
				throw new NotSupportedException(Environment.GetResourceString("InvalidOperation_TypeNotCreated"));
			}
			Type runtimeType = m_methodBuilder.GetTypeBuilder().m_runtimeType;
			ConstructorInfo constructor = runtimeType.GetConstructor(m_methodBuilder.m_parameterTypes);
			return constructor.GetParameters();
		}

		public override MethodImplAttributes GetMethodImplementationFlags()
		{
			return m_methodBuilder.GetMethodImplementationFlags();
		}

		public override object Invoke(BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture)
		{
			throw new NotSupportedException(Environment.GetResourceString("NotSupported_DynamicModule"));
		}

		public override object[] GetCustomAttributes(bool inherit)
		{
			return m_methodBuilder.GetCustomAttributes(inherit);
		}

		public override object[] GetCustomAttributes(Type attributeType, bool inherit)
		{
			return m_methodBuilder.GetCustomAttributes(attributeType, inherit);
		}

		public override bool IsDefined(Type attributeType, bool inherit)
		{
			return m_methodBuilder.IsDefined(attributeType, inherit);
		}

		public MethodToken GetToken()
		{
			return m_methodBuilder.GetToken();
		}

		public ParameterBuilder DefineParameter(int iSequence, ParameterAttributes attributes, string strParamName)
		{
			attributes &= ~ParameterAttributes.ReservedMask;
			return m_methodBuilder.DefineParameter(iSequence, attributes, strParamName);
		}

		public void SetSymCustomAttribute(string name, byte[] data)
		{
			m_methodBuilder.SetSymCustomAttribute(name, data);
		}

		public ILGenerator GetILGenerator()
		{
			if (!m_ReturnILGen)
			{
				throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_DefaultConstructorILGen"));
			}
			return m_methodBuilder.GetILGenerator();
		}

		public ILGenerator GetILGenerator(int streamSize)
		{
			if (!m_ReturnILGen)
			{
				throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_DefaultConstructorILGen"));
			}
			return m_methodBuilder.GetILGenerator(streamSize);
		}

		public void AddDeclarativeSecurity(SecurityAction action, PermissionSet pset)
		{
			if (pset == null)
			{
				throw new ArgumentNullException("pset");
			}
			if (!Enum.IsDefined(typeof(SecurityAction), action) || action == SecurityAction.RequestMinimum || action == SecurityAction.RequestOptional || action == SecurityAction.RequestRefuse)
			{
				throw new ArgumentOutOfRangeException("action");
			}
			if (m_methodBuilder.IsTypeCreated())
			{
				throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_TypeHasBeenCreated"));
			}
			byte[] blob = pset.EncodeXml();
			TypeBuilder.InternalAddDeclarativeSecurity(GetModule(), GetToken().Token, action, blob);
		}

		public Module GetModule()
		{
			return m_methodBuilder.GetModule();
		}

		internal override Type GetReturnType()
		{
			return m_methodBuilder.GetReturnType();
		}

		[ComVisible(true)]
		public void SetCustomAttribute(ConstructorInfo con, byte[] binaryAttribute)
		{
			m_methodBuilder.SetCustomAttribute(con, binaryAttribute);
		}

		public void SetCustomAttribute(CustomAttributeBuilder customBuilder)
		{
			m_methodBuilder.SetCustomAttribute(customBuilder);
		}

		public void SetImplementationFlags(MethodImplAttributes attributes)
		{
			m_methodBuilder.SetImplementationFlags(attributes);
		}

		void _ConstructorBuilder.GetTypeInfoCount(out uint pcTInfo)
		{
			throw new NotImplementedException();
		}

		void _ConstructorBuilder.GetTypeInfo(uint iTInfo, uint lcid, IntPtr ppTInfo)
		{
			throw new NotImplementedException();
		}

		void _ConstructorBuilder.GetIDsOfNames([In] ref Guid riid, IntPtr rgszNames, uint cNames, uint lcid, IntPtr rgDispId)
		{
			throw new NotImplementedException();
		}

		void _ConstructorBuilder.Invoke(uint dispIdMember, [In] ref Guid riid, uint lcid, short wFlags, IntPtr pDispParams, IntPtr pVarResult, IntPtr pExcepInfo, IntPtr puArgErr)
		{
			throw new NotImplementedException();
		}
	}
}
