using System.Collections;
using System.Diagnostics.SymbolStore;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Text;

namespace System.Reflection.Emit
{
	[ComVisible(true)]
	[ClassInterface(ClassInterfaceType.None)]
	[ComDefaultInterface(typeof(_MethodBuilder))]
	[HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort = true)]
	public sealed class MethodBuilder : MethodInfo, _MethodBuilder
	{
		private struct SymCustomAttr
		{
			public string m_name;

			public byte[] m_data;

			public SymCustomAttr(string name, byte[] data)
			{
				m_name = name;
				m_data = data;
			}
		}

		internal string m_strName;

		private MethodToken m_tkMethod;

		internal Module m_module;

		internal TypeBuilder m_containingType;

		private MethodBuilder m_link;

		private int[] m_RVAFixups;

		private int[] m_mdMethodFixups;

		private SignatureHelper m_localSignature;

		internal LocalSymInfo m_localSymInfo;

		internal ILGenerator m_ilGenerator;

		private byte[] m_ubBody;

		private int m_numExceptions;

		private __ExceptionInstance[] m_exceptions;

		internal bool m_bIsBaked;

		private bool m_bIsGlobalMethod;

		private bool m_fInitLocals;

		private MethodAttributes m_iAttributes;

		private CallingConventions m_callingConvention;

		private MethodImplAttributes m_dwMethodImplFlags;

		private SignatureHelper m_signature;

		internal Type[] m_parameterTypes;

		private ParameterBuilder m_retParam;

		internal Type m_returnType;

		private Type[] m_returnTypeRequiredCustomModifiers;

		private Type[] m_returnTypeOptionalCustomModifiers;

		private Type[][] m_parameterTypeRequiredCustomModifiers;

		private Type[][] m_parameterTypeOptionalCustomModifiers;

		private GenericTypeParameterBuilder[] m_inst;

		private bool m_bIsGenMethDef;

		private ArrayList m_symCustomAttrs;

		internal bool m_canBeRuntimeImpl;

		internal bool m_isDllImport;

		public override string Name => m_strName;

		internal override int MetadataTokenInternal => GetToken().Token;

		public override Module Module => m_containingType.Module;

		public override Type DeclaringType
		{
			get
			{
				if (m_containingType.m_isHiddenGlobalType)
				{
					return null;
				}
				return m_containingType;
			}
		}

		public override ICustomAttributeProvider ReturnTypeCustomAttributes => null;

		public override Type ReflectedType
		{
			get
			{
				if (m_containingType.m_isHiddenGlobalType)
				{
					return null;
				}
				return m_containingType;
			}
		}

		public override MethodAttributes Attributes => m_iAttributes;

		public override CallingConventions CallingConvention => m_callingConvention;

		public override RuntimeMethodHandle MethodHandle
		{
			get
			{
				throw new NotSupportedException(Environment.GetResourceString("NotSupported_DynamicModule"));
			}
		}

		public override ParameterInfo ReturnParameter
		{
			get
			{
				if (!m_bIsBaked)
				{
					throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_TypeNotCreated"));
				}
				Type runtimeType = m_containingType.m_runtimeType;
				MethodInfo method = runtimeType.GetMethod(m_strName, m_parameterTypes);
				return method.ReturnParameter;
			}
		}

		public override bool IsGenericMethodDefinition => m_bIsGenMethDef;

		public override bool ContainsGenericParameters
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		public override bool IsGenericMethod => m_inst != null;

		public bool InitLocals
		{
			get
			{
				ThrowIfGeneric();
				return m_fInitLocals;
			}
			set
			{
				ThrowIfGeneric();
				m_fInitLocals = value;
			}
		}

		public string Signature => GetMethodSignature().ToString();

		internal MethodBuilder(string name, MethodAttributes attributes, CallingConventions callingConvention, Type returnType, Type[] parameterTypes, Module mod, TypeBuilder type, bool bIsGlobalMethod)
		{
			Init(name, attributes, callingConvention, returnType, null, null, parameterTypes, null, null, mod, type, bIsGlobalMethod);
		}

		internal MethodBuilder(string name, MethodAttributes attributes, CallingConventions callingConvention, Type returnType, Type[] returnTypeRequiredCustomModifiers, Type[] returnTypeOptionalCustomModifiers, Type[] parameterTypes, Type[][] parameterTypeRequiredCustomModifiers, Type[][] parameterTypeOptionalCustomModifiers, Module mod, TypeBuilder type, bool bIsGlobalMethod)
		{
			Init(name, attributes, callingConvention, returnType, returnTypeRequiredCustomModifiers, returnTypeOptionalCustomModifiers, parameterTypes, parameterTypeRequiredCustomModifiers, parameterTypeOptionalCustomModifiers, mod, type, bIsGlobalMethod);
		}

		private void Init(string name, MethodAttributes attributes, CallingConventions callingConvention, Type returnType, Type[] returnTypeRequiredCustomModifiers, Type[] returnTypeOptionalCustomModifiers, Type[] parameterTypes, Type[][] parameterTypeRequiredCustomModifiers, Type[][] parameterTypeOptionalCustomModifiers, Module mod, TypeBuilder type, bool bIsGlobalMethod)
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
			if (mod == null)
			{
				throw new ArgumentNullException("mod");
			}
			if (parameterTypes != null)
			{
				foreach (Type type2 in parameterTypes)
				{
					if (type2 == null)
					{
						throw new ArgumentNullException("parameterTypes");
					}
				}
			}
			m_link = type.m_currentMethod;
			type.m_currentMethod = this;
			m_strName = name;
			m_module = mod;
			m_containingType = type;
			m_localSignature = SignatureHelper.GetLocalVarSigHelper(mod);
			m_returnType = returnType;
			if ((attributes & MethodAttributes.Static) == 0)
			{
				callingConvention |= CallingConventions.HasThis;
			}
			else if ((attributes & MethodAttributes.Virtual) != 0)
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_NoStaticVirtual"));
			}
			if ((attributes & MethodAttributes.SpecialName) != MethodAttributes.SpecialName && (type.Attributes & TypeAttributes.ClassSemanticsMask) == TypeAttributes.ClassSemanticsMask && (attributes & (MethodAttributes.Virtual | MethodAttributes.Abstract)) != (MethodAttributes.Virtual | MethodAttributes.Abstract) && (attributes & MethodAttributes.Static) == 0)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_BadAttributeOnInterfaceMethod"));
			}
			m_callingConvention = callingConvention;
			if (parameterTypes != null)
			{
				m_parameterTypes = new Type[parameterTypes.Length];
				Array.Copy(parameterTypes, m_parameterTypes, parameterTypes.Length);
			}
			else
			{
				m_parameterTypes = null;
			}
			m_returnTypeRequiredCustomModifiers = returnTypeRequiredCustomModifiers;
			m_returnTypeOptionalCustomModifiers = returnTypeOptionalCustomModifiers;
			m_parameterTypeRequiredCustomModifiers = parameterTypeRequiredCustomModifiers;
			m_parameterTypeOptionalCustomModifiers = parameterTypeOptionalCustomModifiers;
			m_iAttributes = attributes;
			m_bIsGlobalMethod = bIsGlobalMethod;
			m_bIsBaked = false;
			m_fInitLocals = true;
			m_localSymInfo = new LocalSymInfo();
			m_ubBody = null;
			m_ilGenerator = null;
			m_dwMethodImplFlags = MethodImplAttributes.IL;
		}

		internal void CheckContext(params Type[][] typess)
		{
			((AssemblyBuilder)Module.Assembly).CheckContext(typess);
		}

		internal void CheckContext(params Type[] types)
		{
			((AssemblyBuilder)Module.Assembly).CheckContext(types);
		}

		internal void CreateMethodBodyHelper(ILGenerator il)
		{
			int num = 0;
			ModuleBuilder moduleBuilder = (ModuleBuilder)m_module;
			m_containingType.ThrowIfCreated();
			if (m_bIsBaked)
			{
				throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_MethodHasBody"));
			}
			if (il == null)
			{
				throw new ArgumentNullException("il");
			}
			if (il.m_methodBuilder != this && il.m_methodBuilder != null)
			{
				throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_BadILGeneratorUsage"));
			}
			ThrowIfShouldNotHaveBody();
			if (il.m_ScopeTree.m_iOpenScopeCount != 0)
			{
				throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_OpenLocalVariableScope"));
			}
			m_ubBody = il.BakeByteArray();
			m_RVAFixups = il.GetRVAFixups();
			m_mdMethodFixups = il.GetTokenFixups();
			__ExceptionInfo[] exceptions = il.GetExceptions();
			m_numExceptions = CalculateNumberOfExceptions(exceptions);
			if (m_numExceptions > 0)
			{
				m_exceptions = new __ExceptionInstance[m_numExceptions];
				for (int i = 0; i < exceptions.Length; i++)
				{
					int[] filterAddresses = exceptions[i].GetFilterAddresses();
					int[] catchAddresses = exceptions[i].GetCatchAddresses();
					int[] catchEndAddresses = exceptions[i].GetCatchEndAddresses();
					Type[] catchClass = exceptions[i].GetCatchClass();
					for (int j = 0; j < catchClass.Length; j++)
					{
						if (catchClass[j] != null)
						{
							moduleBuilder.GetTypeTokenInternal(catchClass[j]);
						}
					}
					int numberOfCatches = exceptions[i].GetNumberOfCatches();
					int startAddress = exceptions[i].GetStartAddress();
					int endAddress = exceptions[i].GetEndAddress();
					int[] exceptionTypes = exceptions[i].GetExceptionTypes();
					for (int k = 0; k < numberOfCatches; k++)
					{
						int exceptionClass = 0;
						if (catchClass[k] != null)
						{
							exceptionClass = moduleBuilder.GetTypeTokenInternal(catchClass[k]).Token;
						}
						switch (exceptionTypes[k])
						{
						case 0:
						case 1:
						case 4:
						{
							ref __ExceptionInstance reference2 = ref m_exceptions[num++];
							reference2 = new __ExceptionInstance(startAddress, endAddress, filterAddresses[k], catchAddresses[k], catchEndAddresses[k], exceptionTypes[k], exceptionClass);
							break;
						}
						case 2:
						{
							ref __ExceptionInstance reference = ref m_exceptions[num++];
							reference = new __ExceptionInstance(startAddress, exceptions[i].GetFinallyEndAddress(), filterAddresses[k], catchAddresses[k], catchEndAddresses[k], exceptionTypes[k], exceptionClass);
							break;
						}
						}
					}
				}
			}
			m_bIsBaked = true;
			if (moduleBuilder.GetSymWriter() == null)
			{
				return;
			}
			SymbolToken method = new SymbolToken(MetadataTokenInternal);
			ISymbolWriter symWriter = moduleBuilder.GetSymWriter();
			symWriter.OpenMethod(method);
			symWriter.OpenScope(0);
			if (m_symCustomAttrs != null)
			{
				foreach (SymCustomAttr symCustomAttr in m_symCustomAttrs)
				{
					moduleBuilder.GetSymWriter().SetSymAttribute(new SymbolToken(MetadataTokenInternal), symCustomAttr.m_name, symCustomAttr.m_data);
				}
			}
			if (m_localSymInfo != null)
			{
				m_localSymInfo.EmitLocalSymInfo(symWriter);
			}
			il.m_ScopeTree.EmitScopeTree(symWriter);
			il.m_LineNumberInfo.EmitLineNumberInfo(symWriter);
			symWriter.CloseScope(il.m_length);
			symWriter.CloseMethod();
		}

		internal void ReleaseBakedStructures()
		{
			if (m_bIsBaked)
			{
				m_ubBody = null;
				m_localSymInfo = null;
				m_RVAFixups = null;
				m_mdMethodFixups = null;
				m_exceptions = null;
			}
		}

		internal override Type[] GetParameterTypes()
		{
			if (m_parameterTypes == null)
			{
				m_parameterTypes = new Type[0];
			}
			return m_parameterTypes;
		}

		internal void SetToken(MethodToken token)
		{
			m_tkMethod = token;
		}

		internal byte[] GetBody()
		{
			return m_ubBody;
		}

		internal int[] GetTokenFixups()
		{
			return m_mdMethodFixups;
		}

		internal int[] GetRVAFixups()
		{
			return m_RVAFixups;
		}

		internal SignatureHelper GetMethodSignature()
		{
			if (m_parameterTypes == null)
			{
				m_parameterTypes = new Type[0];
			}
			m_signature = SignatureHelper.GetMethodSigHelper(m_module, m_callingConvention, (m_inst != null) ? m_inst.Length : 0, (m_returnType == null) ? typeof(void) : m_returnType, m_returnTypeRequiredCustomModifiers, m_returnTypeOptionalCustomModifiers, m_parameterTypes, m_parameterTypeRequiredCustomModifiers, m_parameterTypeOptionalCustomModifiers);
			return m_signature;
		}

		internal SignatureHelper GetLocalsSignature()
		{
			if (m_ilGenerator != null && m_ilGenerator.m_localCount != 0)
			{
				return m_ilGenerator.m_localSignature;
			}
			return m_localSignature;
		}

		internal int GetNumberOfExceptions()
		{
			return m_numExceptions;
		}

		internal __ExceptionInstance[] GetExceptionInstances()
		{
			return m_exceptions;
		}

		internal int CalculateNumberOfExceptions(__ExceptionInfo[] excp)
		{
			int num = 0;
			if (excp == null)
			{
				return 0;
			}
			for (int i = 0; i < excp.Length; i++)
			{
				num += excp[i].GetNumberOfCatches();
			}
			return num;
		}

		internal bool IsTypeCreated()
		{
			if (m_containingType != null)
			{
				return m_containingType.m_hasBeenCreated;
			}
			return false;
		}

		internal TypeBuilder GetTypeBuilder()
		{
			return m_containingType;
		}

		public override bool Equals(object obj)
		{
			if (!(obj is MethodBuilder))
			{
				return false;
			}
			if (!m_strName.Equals(((MethodBuilder)obj).m_strName))
			{
				return false;
			}
			if (m_iAttributes != ((MethodBuilder)obj).m_iAttributes)
			{
				return false;
			}
			SignatureHelper methodSignature = ((MethodBuilder)obj).GetMethodSignature();
			if (methodSignature.Equals(GetMethodSignature()))
			{
				return true;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return m_strName.GetHashCode();
		}

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder(1000);
			stringBuilder.Append("Name: " + m_strName + " " + Environment.NewLine);
			stringBuilder.Append("Attributes: " + (int)m_iAttributes + Environment.NewLine);
			stringBuilder.Append(string.Concat("Method Signature: ", GetMethodSignature(), Environment.NewLine));
			stringBuilder.Append(Environment.NewLine);
			return stringBuilder.ToString();
		}

		public override object Invoke(object obj, BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture)
		{
			throw new NotSupportedException(Environment.GetResourceString("NotSupported_DynamicModule"));
		}

		public override MethodImplAttributes GetMethodImplementationFlags()
		{
			return m_dwMethodImplFlags;
		}

		public override MethodInfo GetBaseDefinition()
		{
			return this;
		}

		internal override Type GetReturnType()
		{
			return m_returnType;
		}

		public override ParameterInfo[] GetParameters()
		{
			if (!m_bIsBaked)
			{
				throw new NotSupportedException(Environment.GetResourceString("InvalidOperation_TypeNotCreated"));
			}
			Type runtimeType = m_containingType.m_runtimeType;
			MethodInfo method = runtimeType.GetMethod(m_strName, m_parameterTypes);
			return method.GetParameters();
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

		public override MethodInfo GetGenericMethodDefinition()
		{
			if (!IsGenericMethod)
			{
				throw new InvalidOperationException();
			}
			return this;
		}

		public override Type[] GetGenericArguments()
		{
			return m_inst;
		}

		public override MethodInfo MakeGenericMethod(params Type[] typeArguments)
		{
			return new MethodBuilderInstantiation(this, typeArguments);
		}

		public GenericTypeParameterBuilder[] DefineGenericParameters(params string[] names)
		{
			if (m_inst != null)
			{
				throw new InvalidOperationException();
			}
			if (names == null)
			{
				throw new ArgumentNullException("names");
			}
			for (int i = 0; i < names.Length; i++)
			{
				if (names[i] == null)
				{
					throw new ArgumentNullException("names");
				}
			}
			if (m_tkMethod.Token != 0)
			{
				throw new InvalidOperationException();
			}
			if (names.Length == 0)
			{
				throw new ArgumentException();
			}
			m_bIsGenMethDef = true;
			m_inst = new GenericTypeParameterBuilder[names.Length];
			for (int j = 0; j < names.Length; j++)
			{
				m_inst[j] = new GenericTypeParameterBuilder(new TypeBuilder(names[j], j, this));
			}
			return m_inst;
		}

		internal void ThrowIfGeneric()
		{
			if (IsGenericMethod && !IsGenericMethodDefinition)
			{
				throw new InvalidOperationException();
			}
		}

		public MethodToken GetToken()
		{
			if (m_tkMethod.Token == 0)
			{
				if (m_link != null)
				{
					m_link.GetToken();
				}
				int length;
				byte[] signature = GetMethodSignature().InternalGetSignature(out length);
				m_tkMethod = new MethodToken(TypeBuilder.InternalDefineMethod(m_containingType.MetadataTokenInternal, m_strName, signature, length, Attributes, m_module));
				if (m_inst != null)
				{
					GenericTypeParameterBuilder[] inst = m_inst;
					foreach (GenericTypeParameterBuilder genericTypeParameterBuilder in inst)
					{
						if (!genericTypeParameterBuilder.m_type.IsCreated())
						{
							genericTypeParameterBuilder.m_type.CreateType();
						}
					}
				}
				TypeBuilder.InternalSetMethodImpl(m_module, MetadataTokenInternal, m_dwMethodImplFlags);
			}
			return m_tkMethod;
		}

		public void SetParameters(params Type[] parameterTypes)
		{
			CheckContext(parameterTypes);
			SetSignature(null, null, null, parameterTypes, null, null);
		}

		public void SetReturnType(Type returnType)
		{
			CheckContext(returnType);
			SetSignature(returnType, null, null, null, null, null);
		}

		public void SetSignature(Type returnType, Type[] returnTypeRequiredCustomModifiers, Type[] returnTypeOptionalCustomModifiers, Type[] parameterTypes, Type[][] parameterTypeRequiredCustomModifiers, Type[][] parameterTypeOptionalCustomModifiers)
		{
			CheckContext(returnType);
			CheckContext(returnTypeRequiredCustomModifiers, returnTypeOptionalCustomModifiers, parameterTypes);
			CheckContext(parameterTypeRequiredCustomModifiers);
			CheckContext(parameterTypeOptionalCustomModifiers);
			ThrowIfGeneric();
			if (returnType != null)
			{
				m_returnType = returnType;
			}
			if (parameterTypes != null)
			{
				m_parameterTypes = new Type[parameterTypes.Length];
				Array.Copy(parameterTypes, m_parameterTypes, parameterTypes.Length);
			}
			m_returnTypeRequiredCustomModifiers = returnTypeRequiredCustomModifiers;
			m_returnTypeOptionalCustomModifiers = returnTypeOptionalCustomModifiers;
			m_parameterTypeRequiredCustomModifiers = parameterTypeRequiredCustomModifiers;
			m_parameterTypeOptionalCustomModifiers = parameterTypeOptionalCustomModifiers;
		}

		public ParameterBuilder DefineParameter(int position, ParameterAttributes attributes, string strParamName)
		{
			ThrowIfGeneric();
			m_containingType.ThrowIfCreated();
			if (position < 0)
			{
				throw new ArgumentOutOfRangeException(Environment.GetResourceString("ArgumentOutOfRange_ParamSequence"));
			}
			if (position > 0 && (m_parameterTypes == null || position > m_parameterTypes.Length))
			{
				throw new ArgumentOutOfRangeException(Environment.GetResourceString("ArgumentOutOfRange_ParamSequence"));
			}
			attributes &= ~ParameterAttributes.ReservedMask;
			return new ParameterBuilder(this, position, attributes, strParamName);
		}

		[Obsolete("An alternate API is available: Emit the MarshalAs custom attribute instead. http://go.microsoft.com/fwlink/?linkid=14202")]
		public void SetMarshal(UnmanagedMarshal unmanagedMarshal)
		{
			ThrowIfGeneric();
			m_containingType.ThrowIfCreated();
			if (m_retParam == null)
			{
				m_retParam = new ParameterBuilder(this, 0, ParameterAttributes.None, null);
			}
			m_retParam.SetMarshal(unmanagedMarshal);
		}

		public void SetSymCustomAttribute(string name, byte[] data)
		{
			ThrowIfGeneric();
			m_containingType.ThrowIfCreated();
			ModuleBuilder moduleBuilder = (ModuleBuilder)m_module;
			if (moduleBuilder.GetSymWriter() == null)
			{
				throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_NotADebugModule"));
			}
			if (m_symCustomAttrs == null)
			{
				m_symCustomAttrs = new ArrayList();
			}
			m_symCustomAttrs.Add(new SymCustomAttr(name, data));
		}

		public void AddDeclarativeSecurity(SecurityAction action, PermissionSet pset)
		{
			ThrowIfGeneric();
			if (pset == null)
			{
				throw new ArgumentNullException("pset");
			}
			if (!Enum.IsDefined(typeof(SecurityAction), action) || action == SecurityAction.RequestMinimum || action == SecurityAction.RequestOptional || action == SecurityAction.RequestRefuse)
			{
				throw new ArgumentOutOfRangeException("action");
			}
			m_containingType.ThrowIfCreated();
			byte[] blob = null;
			if (!pset.IsEmpty())
			{
				blob = pset.EncodeXml();
			}
			TypeBuilder.InternalAddDeclarativeSecurity(m_module, MetadataTokenInternal, action, blob);
		}

		public void CreateMethodBody(byte[] il, int count)
		{
			ThrowIfGeneric();
			if (m_bIsBaked)
			{
				throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_MethodBaked"));
			}
			m_containingType.ThrowIfCreated();
			if (il != null && (count < 0 || count > il.Length))
			{
				throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("ArgumentOutOfRange_Index"));
			}
			if (il == null)
			{
				m_ubBody = null;
				return;
			}
			m_ubBody = new byte[count];
			Array.Copy(il, m_ubBody, count);
			m_bIsBaked = true;
		}

		public void SetImplementationFlags(MethodImplAttributes attributes)
		{
			ThrowIfGeneric();
			m_containingType.ThrowIfCreated();
			m_dwMethodImplFlags = attributes;
			m_canBeRuntimeImpl = true;
			TypeBuilder.InternalSetMethodImpl(m_module, MetadataTokenInternal, attributes);
		}

		public ILGenerator GetILGenerator()
		{
			ThrowIfGeneric();
			ThrowIfShouldNotHaveBody();
			if (m_ilGenerator == null)
			{
				m_ilGenerator = new ILGenerator(this);
			}
			return m_ilGenerator;
		}

		public ILGenerator GetILGenerator(int size)
		{
			ThrowIfGeneric();
			ThrowIfShouldNotHaveBody();
			if (m_ilGenerator == null)
			{
				m_ilGenerator = new ILGenerator(this, size);
			}
			return m_ilGenerator;
		}

		private void ThrowIfShouldNotHaveBody()
		{
			if ((m_dwMethodImplFlags & MethodImplAttributes.CodeTypeMask) != 0 || (m_dwMethodImplFlags & MethodImplAttributes.ManagedMask) != 0 || (m_iAttributes & MethodAttributes.PinvokeImpl) != 0 || m_isDllImport)
			{
				throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_ShouldNotHaveMethodBody"));
			}
		}

		public Module GetModule()
		{
			return m_module;
		}

		[ComVisible(true)]
		public void SetCustomAttribute(ConstructorInfo con, byte[] binaryAttribute)
		{
			ThrowIfGeneric();
			if (con == null)
			{
				throw new ArgumentNullException("con");
			}
			if (binaryAttribute == null)
			{
				throw new ArgumentNullException("binaryAttribute");
			}
			TypeBuilder.InternalCreateCustomAttribute(MetadataTokenInternal, ((ModuleBuilder)m_module).GetConstructorToken(con).Token, binaryAttribute, m_module, toDisk: false);
			if (IsKnownCA(con))
			{
				ParseCA(con, binaryAttribute);
			}
		}

		public void SetCustomAttribute(CustomAttributeBuilder customBuilder)
		{
			ThrowIfGeneric();
			if (customBuilder == null)
			{
				throw new ArgumentNullException("customBuilder");
			}
			customBuilder.CreateCustomAttribute((ModuleBuilder)m_module, MetadataTokenInternal);
			if (IsKnownCA(customBuilder.m_con))
			{
				ParseCA(customBuilder.m_con, customBuilder.m_blob);
			}
		}

		private bool IsKnownCA(ConstructorInfo con)
		{
			Type declaringType = con.DeclaringType;
			if (declaringType == typeof(MethodImplAttribute))
			{
				return true;
			}
			if (declaringType == typeof(DllImportAttribute))
			{
				return true;
			}
			return false;
		}

		private void ParseCA(ConstructorInfo con, byte[] blob)
		{
			Type declaringType = con.DeclaringType;
			if (declaringType == typeof(MethodImplAttribute))
			{
				m_canBeRuntimeImpl = true;
			}
			else if (declaringType == typeof(DllImportAttribute))
			{
				m_canBeRuntimeImpl = true;
				m_isDllImport = true;
			}
		}

		void _MethodBuilder.GetTypeInfoCount(out uint pcTInfo)
		{
			throw new NotImplementedException();
		}

		void _MethodBuilder.GetTypeInfo(uint iTInfo, uint lcid, IntPtr ppTInfo)
		{
			throw new NotImplementedException();
		}

		void _MethodBuilder.GetIDsOfNames([In] ref Guid riid, IntPtr rgszNames, uint cNames, uint lcid, IntPtr rgDispId)
		{
			throw new NotImplementedException();
		}

		void _MethodBuilder.Invoke(uint dispIdMember, [In] ref Guid riid, uint lcid, short wFlags, IntPtr pDispParams, IntPtr pVarResult, IntPtr pExcepInfo, IntPtr puArgErr)
		{
			throw new NotImplementedException();
		}
	}
}
