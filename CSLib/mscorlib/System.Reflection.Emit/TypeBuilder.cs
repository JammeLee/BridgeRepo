using System.Collections;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;

namespace System.Reflection.Emit
{
	[ClassInterface(ClassInterfaceType.None)]
	[ComVisible(true)]
	[ComDefaultInterface(typeof(_TypeBuilder))]
	[HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort = true)]
	public sealed class TypeBuilder : Type, _TypeBuilder
	{
		internal class CustAttr
		{
			private ConstructorInfo m_con;

			private byte[] m_binaryAttribute;

			private CustomAttributeBuilder m_customBuilder;

			public CustAttr(ConstructorInfo con, byte[] binaryAttribute)
			{
				if (con == null)
				{
					throw new ArgumentNullException("con");
				}
				if (binaryAttribute == null)
				{
					throw new ArgumentNullException("binaryAttribute");
				}
				m_con = con;
				m_binaryAttribute = binaryAttribute;
			}

			public CustAttr(CustomAttributeBuilder customBuilder)
			{
				if (customBuilder == null)
				{
					throw new ArgumentNullException("customBuilder");
				}
				m_customBuilder = customBuilder;
			}

			public void Bake(ModuleBuilder module, int token)
			{
				if (m_customBuilder == null)
				{
					InternalCreateCustomAttribute(token, module.GetConstructorToken(m_con).Token, m_binaryAttribute, module, toDisk: false);
				}
				else
				{
					m_customBuilder.CreateCustomAttribute(module, token);
				}
			}
		}

		public const int UnspecifiedTypeSize = 0;

		internal ArrayList m_ca;

		internal MethodBuilder m_currentMethod;

		private TypeToken m_tdType;

		private ModuleBuilder m_module;

		internal string m_strName;

		private string m_strNameSpace;

		private string m_strFullQualName;

		private Type m_typeParent;

		private Type[] m_typeInterfaces;

		internal TypeAttributes m_iAttr;

		internal GenericParameterAttributes m_genParamAttributes;

		internal ArrayList m_listMethods;

		private int m_constructorCount;

		private int m_iTypeSize;

		private PackingSize m_iPackingSize;

		private TypeBuilder m_DeclaringType;

		private Type m_underlyingSystemType;

		internal bool m_isHiddenGlobalType;

		internal bool m_isHiddenType;

		internal bool m_hasBeenCreated;

		internal RuntimeType m_runtimeType;

		private int m_genParamPos;

		private GenericTypeParameterBuilder[] m_inst;

		private bool m_bIsGenParam;

		private bool m_bIsGenTypeDef;

		private MethodBuilder m_declMeth;

		private TypeBuilder m_genTypeDef;

		public override Type DeclaringType => m_DeclaringType;

		public override Type ReflectedType => m_DeclaringType;

		public override string Name => m_strName;

		public override Module Module => m_module;

		internal override int MetadataTokenInternal => m_tdType.Token;

		public override Guid GUID
		{
			get
			{
				if (m_runtimeType == null)
				{
					throw new NotSupportedException(Environment.GetResourceString("NotSupported_DynamicModule"));
				}
				return m_runtimeType.GUID;
			}
		}

		public override Assembly Assembly => m_module.Assembly;

		public override RuntimeTypeHandle TypeHandle
		{
			get
			{
				throw new NotSupportedException(Environment.GetResourceString("NotSupported_DynamicModule"));
			}
		}

		public override string FullName
		{
			get
			{
				if (m_strFullQualName == null)
				{
					m_strFullQualName = TypeNameBuilder.ToString(this, TypeNameBuilder.Format.FullName);
				}
				return m_strFullQualName;
			}
		}

		public override string Namespace => m_strNameSpace;

		public override string AssemblyQualifiedName => TypeNameBuilder.ToString(this, TypeNameBuilder.Format.AssemblyQualifiedName);

		public override Type BaseType => m_typeParent;

		public override Type UnderlyingSystemType
		{
			get
			{
				if (m_runtimeType != null)
				{
					return m_runtimeType.UnderlyingSystemType;
				}
				if (!base.IsEnum)
				{
					return this;
				}
				if (m_underlyingSystemType == null)
				{
					throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_NoUnderlyingTypeOnEnum"));
				}
				return m_underlyingSystemType;
			}
		}

		public override GenericParameterAttributes GenericParameterAttributes => m_genParamAttributes;

		public override bool IsGenericTypeDefinition => m_bIsGenTypeDef;

		public override bool IsGenericType => m_inst != null;

		public override bool IsGenericParameter => m_bIsGenParam;

		public override int GenericParameterPosition => m_genParamPos;

		public override MethodBase DeclaringMethod => m_declMeth;

		public int Size => m_iTypeSize;

		public PackingSize PackingSize => m_iPackingSize;

		public TypeToken TypeToken
		{
			get
			{
				if (IsGenericParameter)
				{
					ThrowIfCreated();
				}
				return m_tdType;
			}
		}

		public static MethodInfo GetMethod(Type type, MethodInfo method)
		{
			if (!(type is TypeBuilder) && !(type is TypeBuilderInstantiation))
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_MustBeTypeBuilder"));
			}
			if (method.IsGenericMethod && !method.IsGenericMethodDefinition)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_NeedGenericMethodDefinition"), "method");
			}
			if (method.DeclaringType == null || !method.DeclaringType.IsGenericTypeDefinition)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_MethodNeedGenericDeclaringType"), "method");
			}
			if (type.GetGenericTypeDefinition() != method.DeclaringType)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_InvalidMethodDeclaringType"), "type");
			}
			if (type.IsGenericTypeDefinition)
			{
				type = type.MakeGenericType(type.GetGenericArguments());
			}
			if (!(type is TypeBuilderInstantiation))
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_NeedNonGenericType"), "type");
			}
			return MethodOnTypeBuilderInstantiation.GetMethod(method, type as TypeBuilderInstantiation);
		}

		public static ConstructorInfo GetConstructor(Type type, ConstructorInfo constructor)
		{
			if (!(type is TypeBuilder) && !(type is TypeBuilderInstantiation))
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_MustBeTypeBuilder"));
			}
			if (!constructor.DeclaringType.IsGenericTypeDefinition)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_ConstructorNeedGenericDeclaringType"), "constructor");
			}
			if (!(type is TypeBuilderInstantiation))
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_NeedNonGenericType"), "type");
			}
			if (type is TypeBuilder && type.IsGenericTypeDefinition)
			{
				type = type.MakeGenericType(type.GetGenericArguments());
			}
			if (type.GetGenericTypeDefinition() != constructor.DeclaringType)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_InvalidConstructorDeclaringType"), "type");
			}
			return ConstructorOnTypeBuilderInstantiation.GetConstructor(constructor, type as TypeBuilderInstantiation);
		}

		public static FieldInfo GetField(Type type, FieldInfo field)
		{
			if (!(type is TypeBuilder) && !(type is TypeBuilderInstantiation))
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_MustBeTypeBuilder"));
			}
			if (!field.DeclaringType.IsGenericTypeDefinition)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_FieldNeedGenericDeclaringType"), "field");
			}
			if (!(type is TypeBuilderInstantiation))
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_NeedNonGenericType"), "type");
			}
			if (type is TypeBuilder && type.IsGenericTypeDefinition)
			{
				type = type.MakeGenericType(type.GetGenericArguments());
			}
			if (type.GetGenericTypeDefinition() != field.DeclaringType)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_InvalidFieldDeclaringType"), "type");
			}
			return FieldOnTypeBuilderInstantiation.GetField(field, type as TypeBuilderInstantiation);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void _InternalSetParentType(int tdTypeDef, int tkParent, Module module);

		private static void InternalSetParentType(int tdTypeDef, int tkParent, Module module)
		{
			_InternalSetParentType(tdTypeDef, tkParent, module.InternalModule);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void _InternalAddInterfaceImpl(int tdTypeDef, int tkInterface, Module module);

		private static void InternalAddInterfaceImpl(int tdTypeDef, int tkInterface, Module module)
		{
			_InternalAddInterfaceImpl(tdTypeDef, tkInterface, module.InternalModule);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern int _InternalDefineMethod(int handle, string name, byte[] signature, int sigLength, MethodAttributes attributes, Module module);

		internal static int InternalDefineMethod(int handle, string name, byte[] signature, int sigLength, MethodAttributes attributes, Module module)
		{
			return _InternalDefineMethod(handle, name, signature, sigLength, attributes, module.InternalModule);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern int _InternalDefineMethodSpec(int handle, byte[] signature, int sigLength, Module module);

		internal static int InternalDefineMethodSpec(int handle, byte[] signature, int sigLength, Module module)
		{
			return _InternalDefineMethodSpec(handle, signature, sigLength, module.InternalModule);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern int _InternalDefineField(int handle, string name, byte[] signature, int sigLength, FieldAttributes attributes, Module module);

		internal static int InternalDefineField(int handle, string name, byte[] signature, int sigLength, FieldAttributes attributes, Module module)
		{
			return _InternalDefineField(handle, name, signature, sigLength, attributes, module.InternalModule);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void _InternalSetMethodIL(int methodHandle, bool isInitLocals, byte[] body, byte[] LocalSig, int sigLength, int maxStackSize, int numExceptions, __ExceptionInstance[] exceptions, int[] tokenFixups, int[] rvaFixups, Module module);

		internal static void InternalSetMethodIL(int methodHandle, bool isInitLocals, byte[] body, byte[] LocalSig, int sigLength, int maxStackSize, int numExceptions, __ExceptionInstance[] exceptions, int[] tokenFixups, int[] rvaFixups, Module module)
		{
			_InternalSetMethodIL(methodHandle, isInitLocals, body, LocalSig, sigLength, maxStackSize, numExceptions, exceptions, tokenFixups, rvaFixups, module.InternalModule);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void _InternalCreateCustomAttribute(int tkAssociate, int tkConstructor, byte[] attr, Module module, bool toDisk, bool updateCompilerFlags);

		internal static void InternalCreateCustomAttribute(int tkAssociate, int tkConstructor, byte[] attr, Module module, bool toDisk, bool updateCompilerFlags)
		{
			_InternalCreateCustomAttribute(tkAssociate, tkConstructor, attr, module.InternalModule, toDisk, updateCompilerFlags);
		}

		internal static void InternalCreateCustomAttribute(int tkAssociate, int tkConstructor, byte[] attr, Module module, bool toDisk)
		{
			byte[] array = null;
			if (attr != null)
			{
				array = new byte[attr.Length];
				Array.Copy(attr, array, attr.Length);
			}
			InternalCreateCustomAttribute(tkAssociate, tkConstructor, array, module.InternalModule, toDisk, updateCompilerFlags: false);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void _InternalSetPInvokeData(Module module, string DllName, string name, int token, int linkType, int linkFlags);

		internal static void InternalSetPInvokeData(Module module, string DllName, string name, int token, int linkType, int linkFlags)
		{
			_InternalSetPInvokeData(module.InternalModule, DllName, name, token, linkType, linkFlags);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern int _InternalDefineProperty(Module module, int handle, string name, int attributes, byte[] signature, int sigLength, int notifyChanging, int notifyChanged);

		internal static int InternalDefineProperty(Module module, int handle, string name, int attributes, byte[] signature, int sigLength, int notifyChanging, int notifyChanged)
		{
			return _InternalDefineProperty(module.InternalModule, handle, name, attributes, signature, sigLength, notifyChanging, notifyChanged);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern int _InternalDefineEvent(Module module, int handle, string name, int attributes, int tkEventType);

		internal static int InternalDefineEvent(Module module, int handle, string name, int attributes, int tkEventType)
		{
			return _InternalDefineEvent(module.InternalModule, handle, name, attributes, tkEventType);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void _InternalDefineMethodSemantics(Module module, int tkAssociation, MethodSemanticsAttributes semantics, int tkMethod);

		internal static void InternalDefineMethodSemantics(Module module, int tkAssociation, MethodSemanticsAttributes semantics, int tkMethod)
		{
			_InternalDefineMethodSemantics(module.InternalModule, tkAssociation, semantics, tkMethod);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void _InternalDefineMethodImpl(Module module, int tkType, int tkBody, int tkDecl);

		internal static void InternalDefineMethodImpl(Module module, int tkType, int tkBody, int tkDecl)
		{
			_InternalDefineMethodImpl(module.InternalModule, tkType, tkBody, tkDecl);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void _InternalSetMethodImpl(Module module, int tkMethod, MethodImplAttributes MethodImplAttributes);

		internal static void InternalSetMethodImpl(Module module, int tkMethod, MethodImplAttributes MethodImplAttributes)
		{
			_InternalSetMethodImpl(module.InternalModule, tkMethod, MethodImplAttributes);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern int _InternalSetParamInfo(Module module, int tkMethod, int iSequence, ParameterAttributes iParamAttributes, string strParamName);

		internal static int InternalSetParamInfo(Module module, int tkMethod, int iSequence, ParameterAttributes iParamAttributes, string strParamName)
		{
			return _InternalSetParamInfo(module.InternalModule, tkMethod, iSequence, iParamAttributes, strParamName);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern int _InternalGetTokenFromSig(Module module, byte[] signature, int sigLength);

		internal static int InternalGetTokenFromSig(Module module, byte[] signature, int sigLength)
		{
			return _InternalGetTokenFromSig(module.InternalModule, signature, sigLength);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void _InternalSetFieldOffset(Module module, int fdToken, int iOffset);

		internal static void InternalSetFieldOffset(Module module, int fdToken, int iOffset)
		{
			_InternalSetFieldOffset(module.InternalModule, fdToken, iOffset);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void _InternalSetClassLayout(Module module, int tdToken, PackingSize iPackingSize, int iTypeSize);

		internal static void InternalSetClassLayout(Module module, int tdToken, PackingSize iPackingSize, int iTypeSize)
		{
			_InternalSetClassLayout(module.InternalModule, tdToken, iPackingSize, iTypeSize);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void _InternalSetMarshalInfo(Module module, int tk, byte[] ubMarshal, int ubSize);

		internal static void InternalSetMarshalInfo(Module module, int tk, byte[] ubMarshal, int ubSize)
		{
			_InternalSetMarshalInfo(module.InternalModule, tk, ubMarshal, ubSize);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void _InternalSetConstantValue(Module module, int tk, ref Variant var);

		private static void InternalSetConstantValue(Module module, int tk, ref Variant var)
		{
			_InternalSetConstantValue(module.InternalModule, tk, ref var);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void _InternalAddDeclarativeSecurity(Module module, int parent, SecurityAction action, byte[] blob);

		internal static void InternalAddDeclarativeSecurity(Module module, int parent, SecurityAction action, byte[] blob)
		{
			_InternalAddDeclarativeSecurity(module.InternalModule, parent, action, blob);
		}

		private static bool IsPublicComType(Type type)
		{
			Type declaringType = type.DeclaringType;
			if (declaringType != null)
			{
				if (IsPublicComType(declaringType) && (type.Attributes & TypeAttributes.VisibilityMask) == TypeAttributes.NestedPublic)
				{
					return true;
				}
			}
			else if ((type.Attributes & TypeAttributes.VisibilityMask) == TypeAttributes.Public)
			{
				return true;
			}
			return false;
		}

		internal static bool IsTypeEqual(Type t1, Type t2)
		{
			if (t1 == t2)
			{
				return true;
			}
			TypeBuilder typeBuilder = null;
			TypeBuilder typeBuilder2 = null;
			Type type = null;
			Type type2 = null;
			if (t1 is TypeBuilder)
			{
				typeBuilder = (TypeBuilder)t1;
				type = typeBuilder.m_runtimeType;
			}
			else
			{
				type = t1;
			}
			if (t2 is TypeBuilder)
			{
				typeBuilder2 = (TypeBuilder)t2;
				type2 = typeBuilder2.m_runtimeType;
			}
			else
			{
				type2 = t2;
			}
			if (typeBuilder != null && typeBuilder2 != null && typeBuilder == typeBuilder2)
			{
				return true;
			}
			if (type != null && type2 != null && type == type2)
			{
				return true;
			}
			return false;
		}

		internal static void SetConstantValue(Module module, int tk, Type destType, object value)
		{
			if (value == null)
			{
				if (destType.IsValueType)
				{
					throw new ArgumentException(Environment.GetResourceString("Argument_ConstantNull"));
				}
			}
			else
			{
				Type type = value.GetType();
				if (!destType.IsEnum)
				{
					if (destType != type)
					{
						throw new ArgumentException(Environment.GetResourceString("Argument_ConstantDoesntMatch"));
					}
					switch (Type.GetTypeCode(type))
					{
					default:
						if (type != typeof(DateTime))
						{
							throw new ArgumentException(Environment.GetResourceString("Argument_ConstantNotSupported"));
						}
						break;
					case TypeCode.Boolean:
					case TypeCode.Char:
					case TypeCode.SByte:
					case TypeCode.Byte:
					case TypeCode.Int16:
					case TypeCode.UInt16:
					case TypeCode.Int32:
					case TypeCode.UInt32:
					case TypeCode.Int64:
					case TypeCode.UInt64:
					case TypeCode.Single:
					case TypeCode.Double:
					case TypeCode.Decimal:
					case TypeCode.String:
						break;
					}
				}
				else if (destType.UnderlyingSystemType != type)
				{
					throw new ArgumentException(Environment.GetResourceString("Argument_ConstantDoesntMatch"));
				}
			}
			Variant var = new Variant(value);
			InternalSetConstantValue(module.InternalModule, tk, ref var);
		}

		private TypeBuilder(TypeBuilder genTypeDef, GenericTypeParameterBuilder[] inst)
		{
			m_genTypeDef = genTypeDef;
			m_DeclaringType = genTypeDef.m_DeclaringType;
			m_typeParent = genTypeDef.m_typeParent;
			m_runtimeType = genTypeDef.m_runtimeType;
			m_tdType = genTypeDef.m_tdType;
			m_strName = genTypeDef.m_strName;
			m_bIsGenParam = false;
			m_bIsGenTypeDef = false;
			m_module = genTypeDef.m_module;
			m_inst = inst;
			m_hasBeenCreated = true;
		}

		internal TypeBuilder(string szName, int genParamPos, MethodBuilder declMeth)
		{
			m_declMeth = declMeth;
			m_DeclaringType = (TypeBuilder)m_declMeth.DeclaringType;
			m_module = (ModuleBuilder)declMeth.Module;
			InitAsGenericParam(szName, genParamPos);
		}

		private TypeBuilder(string szName, int genParamPos, TypeBuilder declType)
		{
			m_DeclaringType = declType;
			m_module = (ModuleBuilder)declType.Module;
			InitAsGenericParam(szName, genParamPos);
		}

		private void InitAsGenericParam(string szName, int genParamPos)
		{
			m_strName = szName;
			m_genParamPos = genParamPos;
			m_bIsGenParam = true;
			m_bIsGenTypeDef = false;
			m_typeInterfaces = new Type[0];
		}

		internal TypeBuilder(string name, TypeAttributes attr, Type parent, Module module, PackingSize iPackingSize, int iTypeSize, TypeBuilder enclosingType)
		{
			Init(name, attr, parent, null, module, iPackingSize, iTypeSize, enclosingType);
		}

		internal TypeBuilder(string name, TypeAttributes attr, Type parent, Type[] interfaces, Module module, PackingSize iPackingSize, TypeBuilder enclosingType)
		{
			Init(name, attr, parent, interfaces, module, iPackingSize, 0, enclosingType);
		}

		internal TypeBuilder(ModuleBuilder module)
		{
			m_tdType = new TypeToken(33554432);
			m_isHiddenGlobalType = true;
			m_module = module;
			m_listMethods = new ArrayList();
		}

		private void Init(string fullname, TypeAttributes attr, Type parent, Type[] interfaces, Module module, PackingSize iPackingSize, int iTypeSize, TypeBuilder enclosingType)
		{
			m_bIsGenTypeDef = false;
			int[] array = null;
			m_bIsGenParam = false;
			m_hasBeenCreated = false;
			m_runtimeType = null;
			m_isHiddenGlobalType = false;
			m_isHiddenType = false;
			m_module = (ModuleBuilder)module;
			m_DeclaringType = enclosingType;
			Assembly assembly = m_module.Assembly;
			m_underlyingSystemType = null;
			if (fullname == null)
			{
				throw new ArgumentNullException("fullname");
			}
			if (fullname.Length == 0)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_EmptyName"), "fullname");
			}
			if (fullname[0] == '\0')
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_IllegalName"), "fullname");
			}
			if (fullname.Length > 1023)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_TypeNameTooLong"), "fullname");
			}
			assembly.m_assemblyData.CheckTypeNameConflict(fullname, enclosingType);
			if (enclosingType != null && ((attr & TypeAttributes.VisibilityMask) == TypeAttributes.Public || (attr & TypeAttributes.VisibilityMask) == 0))
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_BadNestedTypeFlags"), "attr");
			}
			if (interfaces != null)
			{
				for (int i = 0; i < interfaces.Length; i++)
				{
					if (interfaces[i] == null)
					{
						throw new ArgumentNullException("interfaces");
					}
				}
				array = new int[interfaces.Length];
				for (int i = 0; i < interfaces.Length; i++)
				{
					array[i] = m_module.GetTypeTokenInternal(interfaces[i]).Token;
				}
			}
			int num = fullname.LastIndexOf('.');
			if (num == -1 || num == 0)
			{
				m_strNameSpace = string.Empty;
				m_strName = fullname;
			}
			else
			{
				m_strNameSpace = fullname.Substring(0, num);
				m_strName = fullname.Substring(num + 1);
			}
			VerifyTypeAttributes(attr);
			m_iAttr = attr;
			SetParent(parent);
			m_listMethods = new ArrayList();
			SetInterfaces(interfaces);
			m_constructorCount = 0;
			int tkParent = 0;
			if (m_typeParent != null)
			{
				tkParent = m_module.GetTypeTokenInternal(m_typeParent).Token;
			}
			int tkEnclosingType = 0;
			if (enclosingType != null)
			{
				tkEnclosingType = enclosingType.m_tdType.Token;
			}
			m_tdType = new TypeToken(InternalDefineClass(fullname, tkParent, array, m_iAttr, m_module, Guid.Empty, tkEnclosingType, 0));
			m_iPackingSize = iPackingSize;
			m_iTypeSize = iTypeSize;
			if (m_iPackingSize != 0 || m_iTypeSize != 0)
			{
				InternalSetClassLayout(Module, m_tdType.Token, m_iPackingSize, m_iTypeSize);
			}
			if (IsPublicComType(this) && assembly is AssemblyBuilder)
			{
				AssemblyBuilder assemblyBuilder = (AssemblyBuilder)assembly;
				if (assemblyBuilder.IsPersistable() && !m_module.IsTransient())
				{
					assemblyBuilder.m_assemblyData.AddPublicComType(this);
				}
			}
		}

		private MethodBuilder DefinePInvokeMethodHelper(string name, string dllName, string importName, MethodAttributes attributes, CallingConventions callingConvention, Type returnType, Type[] returnTypeRequiredCustomModifiers, Type[] returnTypeOptionalCustomModifiers, Type[] parameterTypes, Type[][] parameterTypeRequiredCustomModifiers, Type[][] parameterTypeOptionalCustomModifiers, CallingConvention nativeCallConv, CharSet nativeCharSet)
		{
			CheckContext(returnType);
			CheckContext(returnTypeRequiredCustomModifiers, returnTypeOptionalCustomModifiers, parameterTypes);
			CheckContext(parameterTypeRequiredCustomModifiers);
			CheckContext(parameterTypeOptionalCustomModifiers);
			if (Module.Assembly.m_assemblyData.m_isSynchronized)
			{
				lock (Module.Assembly.m_assemblyData)
				{
					return DefinePInvokeMethodHelperNoLock(name, dllName, importName, attributes, callingConvention, returnType, returnTypeRequiredCustomModifiers, returnTypeOptionalCustomModifiers, parameterTypes, parameterTypeRequiredCustomModifiers, parameterTypeOptionalCustomModifiers, nativeCallConv, nativeCharSet);
				}
			}
			return DefinePInvokeMethodHelperNoLock(name, dllName, importName, attributes, callingConvention, returnType, returnTypeRequiredCustomModifiers, returnTypeOptionalCustomModifiers, parameterTypes, parameterTypeRequiredCustomModifiers, parameterTypeOptionalCustomModifiers, nativeCallConv, nativeCharSet);
		}

		private MethodBuilder DefinePInvokeMethodHelperNoLock(string name, string dllName, string importName, MethodAttributes attributes, CallingConventions callingConvention, Type returnType, Type[] returnTypeRequiredCustomModifiers, Type[] returnTypeOptionalCustomModifiers, Type[] parameterTypes, Type[][] parameterTypeRequiredCustomModifiers, Type[][] parameterTypeOptionalCustomModifiers, CallingConvention nativeCallConv, CharSet nativeCharSet)
		{
			ThrowIfCreated();
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			if (name.Length == 0)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_EmptyName"), "name");
			}
			if (dllName == null)
			{
				throw new ArgumentNullException("dllName");
			}
			if (dllName.Length == 0)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_EmptyName"), "dllName");
			}
			if (importName == null)
			{
				throw new ArgumentNullException("importName");
			}
			if (importName.Length == 0)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_EmptyName"), "importName");
			}
			if ((m_iAttr & TypeAttributes.ClassSemanticsMask) == TypeAttributes.ClassSemanticsMask)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_BadPInvokeOnInterface"));
			}
			if ((attributes & MethodAttributes.Abstract) != 0)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_BadPInvokeMethod"));
			}
			attributes |= MethodAttributes.PinvokeImpl;
			MethodBuilder methodBuilder = new MethodBuilder(name, attributes, callingConvention, returnType, returnTypeRequiredCustomModifiers, returnTypeOptionalCustomModifiers, parameterTypes, parameterTypeRequiredCustomModifiers, parameterTypeOptionalCustomModifiers, m_module, this, bIsGlobalMethod: false);
			methodBuilder.GetMethodSignature().InternalGetSignature(out var _);
			if (m_listMethods.Contains(methodBuilder))
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_MethodRedefined"));
			}
			m_listMethods.Add(methodBuilder);
			MethodToken token = methodBuilder.GetToken();
			int num = 0;
			switch (nativeCallConv)
			{
			case CallingConvention.Winapi:
				num = 256;
				break;
			case CallingConvention.Cdecl:
				num = 512;
				break;
			case CallingConvention.StdCall:
				num = 768;
				break;
			case CallingConvention.ThisCall:
				num = 1024;
				break;
			case CallingConvention.FastCall:
				num = 1280;
				break;
			}
			switch (nativeCharSet)
			{
			case CharSet.None:
				num = num;
				break;
			case CharSet.Ansi:
				num |= 2;
				break;
			case CharSet.Unicode:
				num |= 4;
				break;
			case CharSet.Auto:
				num |= 6;
				break;
			}
			InternalSetPInvokeData(m_module, dllName, importName, token.Token, 0, num);
			methodBuilder.SetToken(token);
			return methodBuilder;
		}

		private FieldBuilder DefineDataHelper(string name, byte[] data, int size, FieldAttributes attributes)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			if (name.Length == 0)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_EmptyName"), "name");
			}
			if (size <= 0 || size >= 4128768)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_BadSizeForData"));
			}
			ThrowIfCreated();
			string text = "$ArrayType$" + size;
			Type type = m_module.FindTypeBuilderWithName(text, ignoreCase: false);
			TypeBuilder typeBuilder = type as TypeBuilder;
			if (typeBuilder == null)
			{
				TypeAttributes attr = TypeAttributes.Public | TypeAttributes.ExplicitLayout | TypeAttributes.Sealed;
				typeBuilder = m_module.DefineType(text, attr, typeof(ValueType), PackingSize.Size1, size);
				typeBuilder.m_isHiddenType = true;
				typeBuilder.CreateType();
			}
			FieldBuilder fieldBuilder = DefineField(name, typeBuilder, attributes | FieldAttributes.Static);
			fieldBuilder.SetData(data, size);
			return fieldBuilder;
		}

		private void VerifyTypeAttributes(TypeAttributes attr)
		{
			if (DeclaringType == null)
			{
				if ((attr & TypeAttributes.VisibilityMask) != 0 && (attr & TypeAttributes.VisibilityMask) != TypeAttributes.Public)
				{
					throw new ArgumentException(Environment.GetResourceString("Argument_BadTypeAttrNestedVisibilityOnNonNestedType"));
				}
			}
			else if ((attr & TypeAttributes.VisibilityMask) == 0 || (attr & TypeAttributes.VisibilityMask) == TypeAttributes.Public)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_BadTypeAttrNonNestedVisibilityNestedType"));
			}
			if ((attr & TypeAttributes.LayoutMask) != 0 && (attr & TypeAttributes.LayoutMask) != TypeAttributes.SequentialLayout && (attr & TypeAttributes.LayoutMask) != TypeAttributes.ExplicitLayout)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_BadTypeAttrInvalidLayout"));
			}
			if ((attr & TypeAttributes.ReservedMask) != 0)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_BadTypeAttrReservedBitsSet"));
			}
		}

		public bool IsCreated()
		{
			return m_hasBeenCreated;
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern int _InternalDefineClass(string fullname, int tkParent, int[] interfaceTokens, TypeAttributes attr, Module module, Guid guid, int tkEnclosingType, int tkTypeDef);

		private int InternalDefineClass(string fullname, int tkParent, int[] interfaceTokens, TypeAttributes attr, Module module, Guid guid, int tkEnclosingType, int tkTypeDef)
		{
			return _InternalDefineClass(fullname, tkParent, interfaceTokens, attr, module.InternalModule, guid, tkEnclosingType, tkTypeDef);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern int _InternalDefineGenParam(string name, int tkParent, int position, int attributes, int[] interfaceTokens, Module module, int tkTypeDef);

		private int InternalDefineGenParam(string name, int tkParent, int position, int attributes, int[] interfaceTokens, Module module, int tkTypeDef)
		{
			return _InternalDefineGenParam(name, tkParent, position, attributes, interfaceTokens, module.InternalModule, tkTypeDef);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern Type _TermCreateClass(int handle, Module module);

		private Type TermCreateClass(int handle, Module module)
		{
			return _TermCreateClass(handle, module.InternalModule);
		}

		internal void ThrowIfCreated()
		{
			if (IsCreated())
			{
				throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_TypeHasBeenCreated"));
			}
		}

		public override string ToString()
		{
			return TypeNameBuilder.ToString(this, TypeNameBuilder.Format.ToString);
		}

		public override object InvokeMember(string name, BindingFlags invokeAttr, Binder binder, object target, object[] args, ParameterModifier[] modifiers, CultureInfo culture, string[] namedParameters)
		{
			if (m_runtimeType == null)
			{
				throw new NotSupportedException(Environment.GetResourceString("NotSupported_DynamicModule"));
			}
			return m_runtimeType.InvokeMember(name, invokeAttr, binder, target, args, modifiers, culture, namedParameters);
		}

		protected override ConstructorInfo GetConstructorImpl(BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
		{
			if (m_runtimeType == null)
			{
				throw new NotSupportedException(Environment.GetResourceString("NotSupported_DynamicModule"));
			}
			return m_runtimeType.GetConstructor(bindingAttr, binder, callConvention, types, modifiers);
		}

		[ComVisible(true)]
		public override ConstructorInfo[] GetConstructors(BindingFlags bindingAttr)
		{
			if (m_runtimeType == null)
			{
				throw new NotSupportedException(Environment.GetResourceString("NotSupported_DynamicModule"));
			}
			return m_runtimeType.GetConstructors(bindingAttr);
		}

		protected override MethodInfo GetMethodImpl(string name, BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
		{
			if (m_runtimeType == null)
			{
				throw new NotSupportedException(Environment.GetResourceString("NotSupported_DynamicModule"));
			}
			if (types == null)
			{
				return m_runtimeType.GetMethod(name, bindingAttr);
			}
			return m_runtimeType.GetMethod(name, bindingAttr, binder, callConvention, types, modifiers);
		}

		public override MethodInfo[] GetMethods(BindingFlags bindingAttr)
		{
			if (m_runtimeType == null)
			{
				throw new NotSupportedException(Environment.GetResourceString("NotSupported_DynamicModule"));
			}
			return m_runtimeType.GetMethods(bindingAttr);
		}

		public override FieldInfo GetField(string name, BindingFlags bindingAttr)
		{
			if (m_runtimeType == null)
			{
				throw new NotSupportedException(Environment.GetResourceString("NotSupported_DynamicModule"));
			}
			return m_runtimeType.GetField(name, bindingAttr);
		}

		public override FieldInfo[] GetFields(BindingFlags bindingAttr)
		{
			if (m_runtimeType == null)
			{
				throw new NotSupportedException(Environment.GetResourceString("NotSupported_DynamicModule"));
			}
			return m_runtimeType.GetFields(bindingAttr);
		}

		public override Type GetInterface(string name, bool ignoreCase)
		{
			if (m_runtimeType == null)
			{
				throw new NotSupportedException(Environment.GetResourceString("NotSupported_DynamicModule"));
			}
			return m_runtimeType.GetInterface(name, ignoreCase);
		}

		public override Type[] GetInterfaces()
		{
			if (m_runtimeType != null)
			{
				return m_runtimeType.GetInterfaces();
			}
			if (m_typeInterfaces == null)
			{
				return new Type[0];
			}
			Type[] array = new Type[m_typeInterfaces.Length];
			Array.Copy(m_typeInterfaces, array, m_typeInterfaces.Length);
			return array;
		}

		public override EventInfo GetEvent(string name, BindingFlags bindingAttr)
		{
			if (m_runtimeType == null)
			{
				throw new NotSupportedException(Environment.GetResourceString("NotSupported_DynamicModule"));
			}
			return m_runtimeType.GetEvent(name, bindingAttr);
		}

		public override EventInfo[] GetEvents()
		{
			if (m_runtimeType == null)
			{
				throw new NotSupportedException(Environment.GetResourceString("NotSupported_DynamicModule"));
			}
			return m_runtimeType.GetEvents();
		}

		protected override PropertyInfo GetPropertyImpl(string name, BindingFlags bindingAttr, Binder binder, Type returnType, Type[] types, ParameterModifier[] modifiers)
		{
			throw new NotSupportedException(Environment.GetResourceString("NotSupported_DynamicModule"));
		}

		public override PropertyInfo[] GetProperties(BindingFlags bindingAttr)
		{
			if (m_runtimeType == null)
			{
				throw new NotSupportedException(Environment.GetResourceString("NotSupported_DynamicModule"));
			}
			return m_runtimeType.GetProperties(bindingAttr);
		}

		public override Type[] GetNestedTypes(BindingFlags bindingAttr)
		{
			if (m_runtimeType == null)
			{
				throw new NotSupportedException(Environment.GetResourceString("NotSupported_DynamicModule"));
			}
			return m_runtimeType.GetNestedTypes(bindingAttr);
		}

		public override Type GetNestedType(string name, BindingFlags bindingAttr)
		{
			if (m_runtimeType == null)
			{
				throw new NotSupportedException(Environment.GetResourceString("NotSupported_DynamicModule"));
			}
			return m_runtimeType.GetNestedType(name, bindingAttr);
		}

		public override MemberInfo[] GetMember(string name, MemberTypes type, BindingFlags bindingAttr)
		{
			if (m_runtimeType == null)
			{
				throw new NotSupportedException(Environment.GetResourceString("NotSupported_DynamicModule"));
			}
			return m_runtimeType.GetMember(name, type, bindingAttr);
		}

		[ComVisible(true)]
		public override InterfaceMapping GetInterfaceMap(Type interfaceType)
		{
			if (m_runtimeType == null)
			{
				throw new NotSupportedException(Environment.GetResourceString("NotSupported_DynamicModule"));
			}
			return m_runtimeType.GetInterfaceMap(interfaceType);
		}

		public override EventInfo[] GetEvents(BindingFlags bindingAttr)
		{
			if (m_runtimeType == null)
			{
				throw new NotSupportedException(Environment.GetResourceString("NotSupported_DynamicModule"));
			}
			return m_runtimeType.GetEvents(bindingAttr);
		}

		public override MemberInfo[] GetMembers(BindingFlags bindingAttr)
		{
			if (m_runtimeType == null)
			{
				throw new NotSupportedException(Environment.GetResourceString("NotSupported_DynamicModule"));
			}
			return m_runtimeType.GetMembers(bindingAttr);
		}

		public override bool IsAssignableFrom(Type c)
		{
			if (IsTypeEqual(c, this))
			{
				return true;
			}
			RuntimeType runtimeType = c as RuntimeType;
			TypeBuilder typeBuilder = c as TypeBuilder;
			if (typeBuilder != null && typeBuilder.m_runtimeType != null)
			{
				runtimeType = typeBuilder.m_runtimeType;
			}
			if (runtimeType != null)
			{
				if (m_runtimeType == null)
				{
					return false;
				}
				return m_runtimeType.IsAssignableFrom(runtimeType);
			}
			if (typeBuilder == null)
			{
				return false;
			}
			if (typeBuilder.IsSubclassOf(this))
			{
				return true;
			}
			if (!base.IsInterface)
			{
				return false;
			}
			Type[] interfaces = typeBuilder.GetInterfaces();
			for (int i = 0; i < interfaces.Length; i++)
			{
				if (IsTypeEqual(interfaces[i], this))
				{
					return true;
				}
				if (interfaces[i].IsSubclassOf(this))
				{
					return true;
				}
			}
			return false;
		}

		protected override TypeAttributes GetAttributeFlagsImpl()
		{
			return m_iAttr;
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
			if ((GetAttributeFlagsImpl() & TypeAttributes.Import) == 0)
			{
				return false;
			}
			return true;
		}

		public override Type GetElementType()
		{
			throw new NotSupportedException(Environment.GetResourceString("NotSupported_DynamicModule"));
		}

		protected override bool HasElementTypeImpl()
		{
			return false;
		}

		[ComVisible(true)]
		public override bool IsSubclassOf(Type c)
		{
			Type type = this;
			if (IsTypeEqual(type, c))
			{
				return false;
			}
			for (type = type.BaseType; type != null; type = type.BaseType)
			{
				if (IsTypeEqual(type, c))
				{
					return true;
				}
			}
			return false;
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
			return SymbolType.FormCompoundType(text2.ToCharArray(), this, 0);
		}

		public override object[] GetCustomAttributes(bool inherit)
		{
			if (m_runtimeType == null)
			{
				throw new NotSupportedException(Environment.GetResourceString("NotSupported_DynamicModule"));
			}
			return CustomAttribute.GetCustomAttributes(m_runtimeType, typeof(object) as RuntimeType, inherit);
		}

		public override object[] GetCustomAttributes(Type attributeType, bool inherit)
		{
			if (m_runtimeType == null)
			{
				throw new NotSupportedException(Environment.GetResourceString("NotSupported_DynamicModule"));
			}
			if (attributeType == null)
			{
				throw new ArgumentNullException("attributeType");
			}
			RuntimeType runtimeType = attributeType.UnderlyingSystemType as RuntimeType;
			if (runtimeType == null)
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_MustBeType"), "attributeType");
			}
			return CustomAttribute.GetCustomAttributes(m_runtimeType, runtimeType, inherit);
		}

		public override bool IsDefined(Type attributeType, bool inherit)
		{
			if (m_runtimeType == null)
			{
				throw new NotSupportedException(Environment.GetResourceString("NotSupported_DynamicModule"));
			}
			RuntimeType runtimeType = attributeType.UnderlyingSystemType as RuntimeType;
			if (runtimeType == null)
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_MustBeType"), "caType");
			}
			return CustomAttribute.IsDefined(m_runtimeType, runtimeType, inherit);
		}

		internal void ThrowIfGeneric()
		{
			if (IsGenericType && !IsGenericTypeDefinition)
			{
				throw new InvalidOperationException();
			}
		}

		internal void SetInterfaces(params Type[] interfaces)
		{
			ThrowIfCreated();
			if (interfaces == null)
			{
				m_typeInterfaces = new Type[0];
				return;
			}
			m_typeInterfaces = new Type[interfaces.Length];
			Array.Copy(interfaces, m_typeInterfaces, interfaces.Length);
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
			if (names.Length == 0)
			{
				throw new ArgumentException();
			}
			m_bIsGenTypeDef = true;
			m_inst = new GenericTypeParameterBuilder[names.Length];
			for (int j = 0; j < names.Length; j++)
			{
				m_inst[j] = new GenericTypeParameterBuilder(new TypeBuilder(names[j], j, this));
			}
			return m_inst;
		}

		public override Type MakeGenericType(params Type[] typeArguments)
		{
			CheckContext(typeArguments);
			if (!IsGenericTypeDefinition)
			{
				throw new InvalidOperationException();
			}
			return new TypeBuilderInstantiation(this, typeArguments);
		}

		public override Type[] GetGenericArguments()
		{
			return m_inst;
		}

		public override Type GetGenericTypeDefinition()
		{
			if (IsGenericTypeDefinition)
			{
				return this;
			}
			if (m_genTypeDef == null)
			{
				throw new InvalidOperationException();
			}
			return m_genTypeDef;
		}

		public void DefineMethodOverride(MethodInfo methodInfoBody, MethodInfo methodInfoDeclaration)
		{
			if (Module.Assembly.m_assemblyData.m_isSynchronized)
			{
				lock (Module.Assembly.m_assemblyData)
				{
					DefineMethodOverrideNoLock(methodInfoBody, methodInfoDeclaration);
				}
			}
			else
			{
				DefineMethodOverrideNoLock(methodInfoBody, methodInfoDeclaration);
			}
		}

		private void DefineMethodOverrideNoLock(MethodInfo methodInfoBody, MethodInfo methodInfoDeclaration)
		{
			ThrowIfGeneric();
			ThrowIfCreated();
			if (methodInfoBody == null)
			{
				throw new ArgumentNullException("methodInfoBody");
			}
			if (methodInfoDeclaration == null)
			{
				throw new ArgumentNullException("methodInfoDeclaration");
			}
			if (methodInfoBody.DeclaringType != this)
			{
				throw new ArgumentException(Environment.GetResourceString("ArgumentException_BadMethodImplBody"));
			}
			MethodToken methodTokenInternal = m_module.GetMethodTokenInternal(methodInfoBody);
			InternalDefineMethodImpl(tkDecl: m_module.GetMethodTokenInternal(methodInfoDeclaration).Token, module: m_module, tkType: m_tdType.Token, tkBody: methodTokenInternal.Token);
		}

		public MethodBuilder DefineMethod(string name, MethodAttributes attributes, Type returnType, Type[] parameterTypes)
		{
			return DefineMethod(name, attributes, CallingConventions.Standard, returnType, parameterTypes);
		}

		public MethodBuilder DefineMethod(string name, MethodAttributes attributes)
		{
			return DefineMethod(name, attributes, CallingConventions.Standard, null, null);
		}

		public MethodBuilder DefineMethod(string name, MethodAttributes attributes, CallingConventions callingConvention)
		{
			return DefineMethod(name, attributes, callingConvention, null, null);
		}

		public MethodBuilder DefineMethod(string name, MethodAttributes attributes, CallingConventions callingConvention, Type returnType, Type[] parameterTypes)
		{
			return DefineMethod(name, attributes, callingConvention, returnType, null, null, parameterTypes, null, null);
		}

		public MethodBuilder DefineMethod(string name, MethodAttributes attributes, CallingConventions callingConvention, Type returnType, Type[] returnTypeRequiredCustomModifiers, Type[] returnTypeOptionalCustomModifiers, Type[] parameterTypes, Type[][] parameterTypeRequiredCustomModifiers, Type[][] parameterTypeOptionalCustomModifiers)
		{
			if (Module.Assembly.m_assemblyData.m_isSynchronized)
			{
				lock (Module.Assembly.m_assemblyData)
				{
					return DefineMethodNoLock(name, attributes, callingConvention, returnType, returnTypeRequiredCustomModifiers, returnTypeOptionalCustomModifiers, parameterTypes, parameterTypeRequiredCustomModifiers, parameterTypeOptionalCustomModifiers);
				}
			}
			return DefineMethodNoLock(name, attributes, callingConvention, returnType, returnTypeRequiredCustomModifiers, returnTypeOptionalCustomModifiers, parameterTypes, parameterTypeRequiredCustomModifiers, parameterTypeOptionalCustomModifiers);
		}

		private MethodBuilder DefineMethodNoLock(string name, MethodAttributes attributes, CallingConventions callingConvention, Type returnType, Type[] returnTypeRequiredCustomModifiers, Type[] returnTypeOptionalCustomModifiers, Type[] parameterTypes, Type[][] parameterTypeRequiredCustomModifiers, Type[][] parameterTypeOptionalCustomModifiers)
		{
			CheckContext(returnType);
			CheckContext(returnTypeRequiredCustomModifiers, returnTypeOptionalCustomModifiers, parameterTypes);
			CheckContext(parameterTypeRequiredCustomModifiers);
			CheckContext(parameterTypeOptionalCustomModifiers);
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			if (name.Length == 0)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_EmptyName"), "name");
			}
			if (parameterTypes != null)
			{
				if (parameterTypeOptionalCustomModifiers != null && parameterTypeOptionalCustomModifiers.Length != parameterTypes.Length)
				{
					throw new ArgumentException(Environment.GetResourceString("Argument_MismatchedArrays", "parameterTypeOptionalCustomModifiers", "parameterTypes"));
				}
				if (parameterTypeRequiredCustomModifiers != null && parameterTypeRequiredCustomModifiers.Length != parameterTypes.Length)
				{
					throw new ArgumentException(Environment.GetResourceString("Argument_MismatchedArrays", "parameterTypeRequiredCustomModifiers", "parameterTypes"));
				}
			}
			ThrowIfGeneric();
			ThrowIfCreated();
			if (!m_isHiddenGlobalType && (m_iAttr & TypeAttributes.ClassSemanticsMask) == TypeAttributes.ClassSemanticsMask && (attributes & MethodAttributes.Abstract) == 0 && (attributes & MethodAttributes.Static) == 0)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_BadAttributeOnInterfaceMethod"));
			}
			MethodBuilder methodBuilder = new MethodBuilder(name, attributes, callingConvention, returnType, returnTypeRequiredCustomModifiers, returnTypeOptionalCustomModifiers, parameterTypes, parameterTypeRequiredCustomModifiers, parameterTypeOptionalCustomModifiers, m_module, this, bIsGlobalMethod: false);
			if (!m_isHiddenGlobalType && (methodBuilder.Attributes & MethodAttributes.SpecialName) != 0 && methodBuilder.Name.Equals(ConstructorInfo.ConstructorName))
			{
				m_constructorCount++;
			}
			m_listMethods.Add(methodBuilder);
			return methodBuilder;
		}

		[ComVisible(true)]
		public ConstructorBuilder DefineTypeInitializer()
		{
			if (Module.Assembly.m_assemblyData.m_isSynchronized)
			{
				lock (Module.Assembly.m_assemblyData)
				{
					return DefineTypeInitializerNoLock();
				}
			}
			return DefineTypeInitializerNoLock();
		}

		private ConstructorBuilder DefineTypeInitializerNoLock()
		{
			ThrowIfGeneric();
			ThrowIfCreated();
			MethodAttributes attributes = MethodAttributes.Private | MethodAttributes.Static | MethodAttributes.SpecialName;
			return new ConstructorBuilder(ConstructorInfo.TypeConstructorName, attributes, CallingConventions.Standard, null, m_module, this);
		}

		[ComVisible(true)]
		public ConstructorBuilder DefineDefaultConstructor(MethodAttributes attributes)
		{
			if (Module.Assembly.m_assemblyData.m_isSynchronized)
			{
				lock (Module.Assembly.m_assemblyData)
				{
					return DefineDefaultConstructorNoLock(attributes);
				}
			}
			return DefineDefaultConstructorNoLock(attributes);
		}

		private ConstructorBuilder DefineDefaultConstructorNoLock(MethodAttributes attributes)
		{
			ThrowIfGeneric();
			ConstructorInfo constructorInfo = null;
			if (m_typeParent is TypeBuilderInstantiation)
			{
				Type type = m_typeParent.GetGenericTypeDefinition();
				if (type is TypeBuilder)
				{
					type = ((TypeBuilder)type).m_runtimeType;
				}
				if (type == null)
				{
					throw new NotSupportedException(Environment.GetResourceString("NotSupported_DynamicModule"));
				}
				Type type2 = type.MakeGenericType(m_typeParent.GetGenericArguments());
				constructorInfo = ((!(type2 is TypeBuilderInstantiation)) ? type2.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null) : GetConstructor(type2, type.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null)));
			}
			if (constructorInfo == null)
			{
				constructorInfo = m_typeParent.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null);
			}
			if (constructorInfo == null)
			{
				throw new NotSupportedException(Environment.GetResourceString("NotSupported_NoParentDefaultConstructor"));
			}
			ConstructorBuilder constructorBuilder = DefineConstructor(attributes, CallingConventions.Standard, null);
			m_constructorCount++;
			ILGenerator iLGenerator = constructorBuilder.GetILGenerator();
			iLGenerator.Emit(OpCodes.Ldarg_0);
			iLGenerator.Emit(OpCodes.Call, constructorInfo);
			iLGenerator.Emit(OpCodes.Ret);
			constructorBuilder.m_ReturnILGen = false;
			return constructorBuilder;
		}

		[ComVisible(true)]
		public ConstructorBuilder DefineConstructor(MethodAttributes attributes, CallingConventions callingConvention, Type[] parameterTypes)
		{
			return DefineConstructor(attributes, callingConvention, parameterTypes, null, null);
		}

		[ComVisible(true)]
		public ConstructorBuilder DefineConstructor(MethodAttributes attributes, CallingConventions callingConvention, Type[] parameterTypes, Type[][] requiredCustomModifiers, Type[][] optionalCustomModifiers)
		{
			if (Module.Assembly.m_assemblyData.m_isSynchronized)
			{
				lock (Module.Assembly.m_assemblyData)
				{
					return DefineConstructorNoLock(attributes, callingConvention, parameterTypes, requiredCustomModifiers, optionalCustomModifiers);
				}
			}
			return DefineConstructorNoLock(attributes, callingConvention, parameterTypes, requiredCustomModifiers, optionalCustomModifiers);
		}

		private ConstructorBuilder DefineConstructorNoLock(MethodAttributes attributes, CallingConventions callingConvention, Type[] parameterTypes, Type[][] requiredCustomModifiers, Type[][] optionalCustomModifiers)
		{
			CheckContext(parameterTypes);
			CheckContext(requiredCustomModifiers);
			CheckContext(optionalCustomModifiers);
			ThrowIfGeneric();
			ThrowIfCreated();
			string name = (((attributes & MethodAttributes.Static) != 0) ? ConstructorInfo.TypeConstructorName : ConstructorInfo.ConstructorName);
			attributes |= MethodAttributes.SpecialName;
			ConstructorBuilder result = new ConstructorBuilder(name, attributes, callingConvention, parameterTypes, requiredCustomModifiers, optionalCustomModifiers, m_module, this);
			m_constructorCount++;
			return result;
		}

		public MethodBuilder DefinePInvokeMethod(string name, string dllName, MethodAttributes attributes, CallingConventions callingConvention, Type returnType, Type[] parameterTypes, CallingConvention nativeCallConv, CharSet nativeCharSet)
		{
			ThrowIfGeneric();
			return DefinePInvokeMethodHelper(name, dllName, name, attributes, callingConvention, returnType, null, null, parameterTypes, null, null, nativeCallConv, nativeCharSet);
		}

		public MethodBuilder DefinePInvokeMethod(string name, string dllName, string entryName, MethodAttributes attributes, CallingConventions callingConvention, Type returnType, Type[] parameterTypes, CallingConvention nativeCallConv, CharSet nativeCharSet)
		{
			return DefinePInvokeMethodHelper(name, dllName, entryName, attributes, callingConvention, returnType, null, null, parameterTypes, null, null, nativeCallConv, nativeCharSet);
		}

		public MethodBuilder DefinePInvokeMethod(string name, string dllName, string entryName, MethodAttributes attributes, CallingConventions callingConvention, Type returnType, Type[] returnTypeRequiredCustomModifiers, Type[] returnTypeOptionalCustomModifiers, Type[] parameterTypes, Type[][] parameterTypeRequiredCustomModifiers, Type[][] parameterTypeOptionalCustomModifiers, CallingConvention nativeCallConv, CharSet nativeCharSet)
		{
			ThrowIfGeneric();
			return DefinePInvokeMethodHelper(name, dllName, entryName, attributes, callingConvention, returnType, returnTypeRequiredCustomModifiers, returnTypeOptionalCustomModifiers, parameterTypes, parameterTypeRequiredCustomModifiers, parameterTypeOptionalCustomModifiers, nativeCallConv, nativeCharSet);
		}

		public TypeBuilder DefineNestedType(string name)
		{
			if (Module.Assembly.m_assemblyData.m_isSynchronized)
			{
				lock (Module.Assembly.m_assemblyData)
				{
					return DefineNestedTypeNoLock(name);
				}
			}
			return DefineNestedTypeNoLock(name);
		}

		private TypeBuilder DefineNestedTypeNoLock(string name)
		{
			ThrowIfGeneric();
			TypeBuilder typeBuilder = new TypeBuilder(name, TypeAttributes.NestedPrivate, null, null, m_module, PackingSize.Unspecified, this);
			m_module.m_TypeBuilderList.Add(typeBuilder);
			return typeBuilder;
		}

		[ComVisible(true)]
		public TypeBuilder DefineNestedType(string name, TypeAttributes attr, Type parent, Type[] interfaces)
		{
			if (Module.Assembly.m_assemblyData.m_isSynchronized)
			{
				lock (Module.Assembly.m_assemblyData)
				{
					return DefineNestedTypeNoLock(name, attr, parent, interfaces);
				}
			}
			return DefineNestedTypeNoLock(name, attr, parent, interfaces);
		}

		private TypeBuilder DefineNestedTypeNoLock(string name, TypeAttributes attr, Type parent, Type[] interfaces)
		{
			CheckContext(parent);
			CheckContext(interfaces);
			ThrowIfGeneric();
			TypeBuilder typeBuilder = new TypeBuilder(name, attr, parent, interfaces, m_module, PackingSize.Unspecified, this);
			m_module.m_TypeBuilderList.Add(typeBuilder);
			return typeBuilder;
		}

		public TypeBuilder DefineNestedType(string name, TypeAttributes attr, Type parent)
		{
			if (Module.Assembly.m_assemblyData.m_isSynchronized)
			{
				lock (Module.Assembly.m_assemblyData)
				{
					return DefineNestedTypeNoLock(name, attr, parent);
				}
			}
			return DefineNestedTypeNoLock(name, attr, parent);
		}

		private TypeBuilder DefineNestedTypeNoLock(string name, TypeAttributes attr, Type parent)
		{
			ThrowIfGeneric();
			TypeBuilder typeBuilder = new TypeBuilder(name, attr, parent, null, m_module, PackingSize.Unspecified, this);
			m_module.m_TypeBuilderList.Add(typeBuilder);
			return typeBuilder;
		}

		public TypeBuilder DefineNestedType(string name, TypeAttributes attr)
		{
			if (Module.Assembly.m_assemblyData.m_isSynchronized)
			{
				lock (Module.Assembly.m_assemblyData)
				{
					return DefineNestedTypeNoLock(name, attr);
				}
			}
			return DefineNestedTypeNoLock(name, attr);
		}

		private TypeBuilder DefineNestedTypeNoLock(string name, TypeAttributes attr)
		{
			ThrowIfGeneric();
			TypeBuilder typeBuilder = new TypeBuilder(name, attr, null, null, m_module, PackingSize.Unspecified, this);
			m_module.m_TypeBuilderList.Add(typeBuilder);
			return typeBuilder;
		}

		public TypeBuilder DefineNestedType(string name, TypeAttributes attr, Type parent, int typeSize)
		{
			if (Module.Assembly.m_assemblyData.m_isSynchronized)
			{
				lock (Module.Assembly.m_assemblyData)
				{
					return DefineNestedTypeNoLock(name, attr, parent, typeSize);
				}
			}
			return DefineNestedTypeNoLock(name, attr, parent, typeSize);
		}

		private TypeBuilder DefineNestedTypeNoLock(string name, TypeAttributes attr, Type parent, int typeSize)
		{
			TypeBuilder typeBuilder = new TypeBuilder(name, attr, parent, m_module, PackingSize.Unspecified, typeSize, this);
			m_module.m_TypeBuilderList.Add(typeBuilder);
			return typeBuilder;
		}

		public TypeBuilder DefineNestedType(string name, TypeAttributes attr, Type parent, PackingSize packSize)
		{
			if (Module.Assembly.m_assemblyData.m_isSynchronized)
			{
				lock (Module.Assembly.m_assemblyData)
				{
					return DefineNestedTypeNoLock(name, attr, parent, packSize);
				}
			}
			return DefineNestedTypeNoLock(name, attr, parent, packSize);
		}

		private TypeBuilder DefineNestedTypeNoLock(string name, TypeAttributes attr, Type parent, PackingSize packSize)
		{
			ThrowIfGeneric();
			TypeBuilder typeBuilder = new TypeBuilder(name, attr, parent, null, m_module, packSize, this);
			m_module.m_TypeBuilderList.Add(typeBuilder);
			return typeBuilder;
		}

		public FieldBuilder DefineField(string fieldName, Type type, FieldAttributes attributes)
		{
			return DefineField(fieldName, type, null, null, attributes);
		}

		public FieldBuilder DefineField(string fieldName, Type type, Type[] requiredCustomModifiers, Type[] optionalCustomModifiers, FieldAttributes attributes)
		{
			if (Module.Assembly.m_assemblyData.m_isSynchronized)
			{
				lock (Module.Assembly.m_assemblyData)
				{
					return DefineFieldNoLock(fieldName, type, requiredCustomModifiers, optionalCustomModifiers, attributes);
				}
			}
			return DefineFieldNoLock(fieldName, type, requiredCustomModifiers, optionalCustomModifiers, attributes);
		}

		private FieldBuilder DefineFieldNoLock(string fieldName, Type type, Type[] requiredCustomModifiers, Type[] optionalCustomModifiers, FieldAttributes attributes)
		{
			ThrowIfGeneric();
			ThrowIfCreated();
			CheckContext(type);
			CheckContext(requiredCustomModifiers);
			if (m_underlyingSystemType == null && base.IsEnum && (attributes & FieldAttributes.Static) == 0)
			{
				m_underlyingSystemType = type;
			}
			return new FieldBuilder(this, fieldName, type, requiredCustomModifiers, optionalCustomModifiers, attributes);
		}

		public FieldBuilder DefineInitializedData(string name, byte[] data, FieldAttributes attributes)
		{
			if (Module.Assembly.m_assemblyData.m_isSynchronized)
			{
				lock (Module.Assembly.m_assemblyData)
				{
					return DefineInitializedDataNoLock(name, data, attributes);
				}
			}
			return DefineInitializedDataNoLock(name, data, attributes);
		}

		private FieldBuilder DefineInitializedDataNoLock(string name, byte[] data, FieldAttributes attributes)
		{
			ThrowIfGeneric();
			if (data == null)
			{
				throw new ArgumentNullException("data");
			}
			return DefineDataHelper(name, data, data.Length, attributes);
		}

		public FieldBuilder DefineUninitializedData(string name, int size, FieldAttributes attributes)
		{
			if (Module.Assembly.m_assemblyData.m_isSynchronized)
			{
				lock (Module.Assembly.m_assemblyData)
				{
					return DefineUninitializedDataNoLock(name, size, attributes);
				}
			}
			return DefineUninitializedDataNoLock(name, size, attributes);
		}

		private FieldBuilder DefineUninitializedDataNoLock(string name, int size, FieldAttributes attributes)
		{
			ThrowIfGeneric();
			return DefineDataHelper(name, null, size, attributes);
		}

		public PropertyBuilder DefineProperty(string name, PropertyAttributes attributes, Type returnType, Type[] parameterTypes)
		{
			return DefineProperty(name, attributes, returnType, null, null, parameterTypes, null, null);
		}

		public PropertyBuilder DefineProperty(string name, PropertyAttributes attributes, Type returnType, Type[] returnTypeRequiredCustomModifiers, Type[] returnTypeOptionalCustomModifiers, Type[] parameterTypes, Type[][] parameterTypeRequiredCustomModifiers, Type[][] parameterTypeOptionalCustomModifiers)
		{
			return DefineProperty(name, attributes, (CallingConventions)0, returnType, returnTypeRequiredCustomModifiers, returnTypeOptionalCustomModifiers, parameterTypes, parameterTypeRequiredCustomModifiers, parameterTypeOptionalCustomModifiers);
		}

		public PropertyBuilder DefineProperty(string name, PropertyAttributes attributes, CallingConventions callingConvention, Type returnType, Type[] returnTypeRequiredCustomModifiers, Type[] returnTypeOptionalCustomModifiers, Type[] parameterTypes, Type[][] parameterTypeRequiredCustomModifiers, Type[][] parameterTypeOptionalCustomModifiers)
		{
			if (Module.Assembly.m_assemblyData.m_isSynchronized)
			{
				lock (Module.Assembly.m_assemblyData)
				{
					return DefinePropertyNoLock(name, attributes, callingConvention, returnType, returnTypeRequiredCustomModifiers, returnTypeOptionalCustomModifiers, parameterTypes, parameterTypeRequiredCustomModifiers, parameterTypeOptionalCustomModifiers);
				}
			}
			return DefinePropertyNoLock(name, attributes, callingConvention, returnType, returnTypeRequiredCustomModifiers, returnTypeOptionalCustomModifiers, parameterTypes, parameterTypeRequiredCustomModifiers, parameterTypeOptionalCustomModifiers);
		}

		private PropertyBuilder DefinePropertyNoLock(string name, PropertyAttributes attributes, CallingConventions callingConvention, Type returnType, Type[] returnTypeRequiredCustomModifiers, Type[] returnTypeOptionalCustomModifiers, Type[] parameterTypes, Type[][] parameterTypeRequiredCustomModifiers, Type[][] parameterTypeOptionalCustomModifiers)
		{
			ThrowIfGeneric();
			CheckContext(returnType);
			CheckContext(returnTypeRequiredCustomModifiers, returnTypeOptionalCustomModifiers, parameterTypes);
			CheckContext(parameterTypeRequiredCustomModifiers);
			CheckContext(parameterTypeOptionalCustomModifiers);
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			if (name.Length == 0)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_EmptyName"), "name");
			}
			ThrowIfCreated();
			SignatureHelper propertySigHelper = SignatureHelper.GetPropertySigHelper(m_module, callingConvention, returnType, returnTypeRequiredCustomModifiers, returnTypeOptionalCustomModifiers, parameterTypes, parameterTypeRequiredCustomModifiers, parameterTypeOptionalCustomModifiers);
			int length;
			byte[] signature = propertySigHelper.InternalGetSignature(out length);
			return new PropertyBuilder(prToken: new PropertyToken(InternalDefineProperty(m_module, m_tdType.Token, name, (int)attributes, signature, length, 0, 0)), mod: m_module, name: name, sig: propertySigHelper, attr: attributes, returnType: returnType, containingType: this);
		}

		public EventBuilder DefineEvent(string name, EventAttributes attributes, Type eventtype)
		{
			if (Module.Assembly.m_assemblyData.m_isSynchronized)
			{
				lock (Module.Assembly.m_assemblyData)
				{
					return DefineEventNoLock(name, attributes, eventtype);
				}
			}
			return DefineEventNoLock(name, attributes, eventtype);
		}

		private EventBuilder DefineEventNoLock(string name, EventAttributes attributes, Type eventtype)
		{
			CheckContext(eventtype);
			ThrowIfGeneric();
			ThrowIfCreated();
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
			int token = m_module.GetTypeTokenInternal(eventtype).Token;
			return new EventBuilder(evToken: new EventToken(InternalDefineEvent(m_module, m_tdType.Token, name, (int)attributes, token)), mod: m_module, name: name, attr: attributes, eventType: token, type: this);
		}

		public Type CreateType()
		{
			if (Module.Assembly.m_assemblyData.m_isSynchronized)
			{
				lock (Module.Assembly.m_assemblyData)
				{
					return CreateTypeNoLock();
				}
			}
			return CreateTypeNoLock();
		}

		internal void CheckContext(params Type[][] typess)
		{
			((AssemblyBuilder)Module.Assembly).CheckContext(typess);
		}

		internal void CheckContext(params Type[] types)
		{
			((AssemblyBuilder)Module.Assembly).CheckContext(types);
		}

		private Type CreateTypeNoLock()
		{
			if (IsCreated())
			{
				return m_runtimeType;
			}
			ThrowIfGeneric();
			ThrowIfCreated();
			if (m_typeInterfaces == null)
			{
				m_typeInterfaces = new Type[0];
			}
			int[] array = new int[m_typeInterfaces.Length];
			for (int i = 0; i < m_typeInterfaces.Length; i++)
			{
				array[i] = m_module.GetTypeTokenInternal(m_typeInterfaces[i]).Token;
			}
			int num = 0;
			if (m_typeParent != null)
			{
				num = m_module.GetTypeTokenInternal(m_typeParent).Token;
			}
			if (IsGenericParameter)
			{
				int[] array2 = new int[m_typeInterfaces.Length];
				if (m_typeParent != null)
				{
					array2 = new int[m_typeInterfaces.Length + 1];
					array2[array2.Length - 1] = num;
				}
				for (int j = 0; j < m_typeInterfaces.Length; j++)
				{
					array2[j] = m_module.GetTypeTokenInternal(m_typeInterfaces[j]).Token;
				}
				int tkParent = ((m_declMeth == null) ? m_DeclaringType.m_tdType.Token : m_declMeth.GetToken().Token);
				m_tdType = new TypeToken(InternalDefineGenParam(m_strName, tkParent, m_genParamPos, (int)m_genParamAttributes, array2, m_module, 0));
				if (m_ca != null)
				{
					foreach (CustAttr item in m_ca)
					{
						item.Bake(m_module, MetadataTokenInternal);
					}
				}
				m_hasBeenCreated = true;
				return this;
			}
			if (((uint)m_tdType.Token & 0xFFFFFFu) != 0 && ((uint)num & 0xFFFFFFu) != 0)
			{
				InternalSetParentType(m_tdType.Token, num, m_module);
			}
			if (m_inst != null)
			{
				GenericTypeParameterBuilder[] inst = m_inst;
				foreach (Type type in inst)
				{
					if (type is GenericTypeParameterBuilder)
					{
						((GenericTypeParameterBuilder)type).m_type.CreateType();
					}
				}
			}
			if (!m_isHiddenGlobalType && m_constructorCount == 0 && (m_iAttr & TypeAttributes.ClassSemanticsMask) == 0 && !base.IsValueType && (m_iAttr & (TypeAttributes.Abstract | TypeAttributes.Sealed)) != (TypeAttributes.Abstract | TypeAttributes.Sealed))
			{
				DefineDefaultConstructor(MethodAttributes.Public);
			}
			int count = m_listMethods.Count;
			for (int l = 0; l < count; l++)
			{
				MethodBuilder methodBuilder = (MethodBuilder)m_listMethods[l];
				if (methodBuilder.IsGenericMethodDefinition)
				{
					methodBuilder.GetToken();
				}
				MethodAttributes attributes = methodBuilder.Attributes;
				if ((methodBuilder.GetMethodImplementationFlags() & (MethodImplAttributes)135) != 0 || (attributes & MethodAttributes.PinvokeImpl) != 0)
				{
					continue;
				}
				int length;
				byte[] localSig = methodBuilder.GetLocalsSignature().InternalGetSignature(out length);
				if ((attributes & MethodAttributes.Abstract) != 0 && (m_iAttr & TypeAttributes.Abstract) == 0)
				{
					throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_BadTypeAttributesNotAbstract"));
				}
				byte[] body = methodBuilder.GetBody();
				if ((attributes & MethodAttributes.Abstract) != 0)
				{
					if (body != null)
					{
						throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_BadMethodBody"));
					}
				}
				else if (body == null || body.Length == 0)
				{
					if (methodBuilder.m_ilGenerator != null)
					{
						methodBuilder.CreateMethodBodyHelper(methodBuilder.GetILGenerator());
					}
					body = methodBuilder.GetBody();
					if ((body == null || body.Length == 0) && !methodBuilder.m_canBeRuntimeImpl)
					{
						throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("InvalidOperation_BadEmptyMethodBody"), methodBuilder.Name));
					}
				}
				int maxStackSize = ((methodBuilder.m_ilGenerator == null) ? 16 : methodBuilder.m_ilGenerator.GetMaxStackSize());
				__ExceptionInstance[] exceptionInstances = methodBuilder.GetExceptionInstances();
				int[] tokenFixups = methodBuilder.GetTokenFixups();
				int[] rVAFixups = methodBuilder.GetRVAFixups();
				__ExceptionInstance[] array3 = null;
				int[] array4 = null;
				int[] array5 = null;
				if (exceptionInstances != null)
				{
					array3 = new __ExceptionInstance[exceptionInstances.Length];
					Array.Copy(exceptionInstances, array3, exceptionInstances.Length);
				}
				if (tokenFixups != null)
				{
					array4 = new int[tokenFixups.Length];
					Array.Copy(tokenFixups, array4, tokenFixups.Length);
				}
				if (rVAFixups != null)
				{
					array5 = new int[rVAFixups.Length];
					Array.Copy(rVAFixups, array5, rVAFixups.Length);
				}
				InternalSetMethodIL(methodBuilder.GetToken().Token, methodBuilder.InitLocals, body, localSig, length, maxStackSize, methodBuilder.GetNumberOfExceptions(), array3, array4, array5, m_module);
				if (Assembly.m_assemblyData.m_access == AssemblyBuilderAccess.Run)
				{
					methodBuilder.ReleaseBakedStructures();
				}
			}
			m_hasBeenCreated = true;
			Type type2 = TermCreateClass(m_tdType.Token, m_module);
			if (!m_isHiddenGlobalType)
			{
				m_runtimeType = (RuntimeType)type2;
				if (m_DeclaringType != null && m_DeclaringType.m_runtimeType != null)
				{
					m_DeclaringType.m_runtimeType.InvalidateCachedNestedType();
				}
				return type2;
			}
			return null;
		}

		public void SetParent(Type parent)
		{
			ThrowIfGeneric();
			ThrowIfCreated();
			CheckContext(parent);
			if (parent != null)
			{
				m_typeParent = parent;
				return;
			}
			if ((m_iAttr & TypeAttributes.ClassSemanticsMask) != TypeAttributes.ClassSemanticsMask)
			{
				m_typeParent = typeof(object);
				return;
			}
			if ((m_iAttr & TypeAttributes.Abstract) == 0)
			{
				throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_BadInterfaceNotAbstract"));
			}
			m_typeParent = null;
		}

		[ComVisible(true)]
		public void AddInterfaceImplementation(Type interfaceType)
		{
			ThrowIfGeneric();
			CheckContext(interfaceType);
			if (interfaceType == null)
			{
				throw new ArgumentNullException("interfaceType");
			}
			ThrowIfCreated();
			InternalAddInterfaceImpl(tkInterface: m_module.GetTypeTokenInternal(interfaceType).Token, tdTypeDef: m_tdType.Token, module: m_module);
		}

		public void AddDeclarativeSecurity(SecurityAction action, PermissionSet pset)
		{
			if (Module.Assembly.m_assemblyData.m_isSynchronized)
			{
				lock (Module.Assembly.m_assemblyData)
				{
					AddDeclarativeSecurityNoLock(action, pset);
				}
			}
			else
			{
				AddDeclarativeSecurityNoLock(action, pset);
			}
		}

		private void AddDeclarativeSecurityNoLock(SecurityAction action, PermissionSet pset)
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
			ThrowIfCreated();
			byte[] blob = null;
			if (!pset.IsEmpty())
			{
				blob = pset.EncodeXml();
			}
			InternalAddDeclarativeSecurity(m_module, m_tdType.Token, action, blob);
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
			InternalCreateCustomAttribute(m_tdType.Token, m_module.GetConstructorToken(con).Token, binaryAttribute, m_module, toDisk: false);
		}

		public void SetCustomAttribute(CustomAttributeBuilder customBuilder)
		{
			ThrowIfGeneric();
			if (customBuilder == null)
			{
				throw new ArgumentNullException("customBuilder");
			}
			customBuilder.CreateCustomAttribute(m_module, m_tdType.Token);
		}

		void _TypeBuilder.GetTypeInfoCount(out uint pcTInfo)
		{
			throw new NotImplementedException();
		}

		void _TypeBuilder.GetTypeInfo(uint iTInfo, uint lcid, IntPtr ppTInfo)
		{
			throw new NotImplementedException();
		}

		void _TypeBuilder.GetIDsOfNames([In] ref Guid riid, IntPtr rgszNames, uint cNames, uint lcid, IntPtr rgDispId)
		{
			throw new NotImplementedException();
		}

		void _TypeBuilder.Invoke(uint dispIdMember, [In] ref Guid riid, uint lcid, short wFlags, IntPtr pDispParams, IntPtr pVarResult, IntPtr pExcepInfo, IntPtr puArgErr)
		{
			throw new NotImplementedException();
		}
	}
}
