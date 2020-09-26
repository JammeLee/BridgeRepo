using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Reflection.Cache;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Remoting;
using System.Security.Permissions;
using System.Threading;

namespace System
{
	[Serializable]
	[ComVisible(true)]
	[ComDefaultInterface(typeof(_Type))]
	[ClassInterface(ClassInterfaceType.None)]
	public abstract class Type : MemberInfo, _Type, IReflect
	{
		private const BindingFlags DefaultLookup = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public;

		public static readonly MemberFilter FilterAttribute;

		public static readonly MemberFilter FilterName;

		public static readonly MemberFilter FilterNameIgnoreCase;

		public static readonly object Missing;

		public static readonly char Delimiter;

		public static readonly Type[] EmptyTypes;

		private static object defaultBinder;

		private static readonly Type valueType;

		private static readonly Type enumType;

		private static readonly Type objectType;

		public override MemberTypes MemberType => MemberTypes.TypeInfo;

		public override Type DeclaringType => this;

		public virtual MethodBase DeclaringMethod => null;

		public override Type ReflectedType => this;

		public virtual StructLayoutAttribute StructLayoutAttribute
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		public abstract Guid GUID
		{
			get;
		}

		public static Binder DefaultBinder
		{
			get
			{
				if (defaultBinder == null)
				{
					CreateBinder();
				}
				return defaultBinder as Binder;
			}
		}

		public new abstract Module Module
		{
			get;
		}

		public abstract Assembly Assembly
		{
			get;
		}

		public virtual RuntimeTypeHandle TypeHandle
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			get;
		}

		public abstract string FullName
		{
			get;
		}

		public abstract string Namespace
		{
			get;
		}

		public abstract string AssemblyQualifiedName
		{
			get;
		}

		public abstract Type BaseType
		{
			get;
		}

		[ComVisible(true)]
		public ConstructorInfo TypeInitializer => GetConstructorImpl(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, CallingConventions.Any, EmptyTypes, null);

		public bool IsNested => DeclaringType != null;

		public TypeAttributes Attributes => GetAttributeFlagsImpl();

		public virtual GenericParameterAttributes GenericParameterAttributes
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		public bool IsVisible => GetTypeHandleInternal().IsVisible();

		public bool IsNotPublic => (GetAttributeFlagsImpl() & TypeAttributes.VisibilityMask) == 0;

		public bool IsPublic => (GetAttributeFlagsImpl() & TypeAttributes.VisibilityMask) == TypeAttributes.Public;

		public bool IsNestedPublic => (GetAttributeFlagsImpl() & TypeAttributes.VisibilityMask) == TypeAttributes.NestedPublic;

		public bool IsNestedPrivate => (GetAttributeFlagsImpl() & TypeAttributes.VisibilityMask) == TypeAttributes.NestedPrivate;

		public bool IsNestedFamily => (GetAttributeFlagsImpl() & TypeAttributes.VisibilityMask) == TypeAttributes.NestedFamily;

		public bool IsNestedAssembly => (GetAttributeFlagsImpl() & TypeAttributes.VisibilityMask) == TypeAttributes.NestedAssembly;

		public bool IsNestedFamANDAssem => (GetAttributeFlagsImpl() & TypeAttributes.VisibilityMask) == TypeAttributes.NestedFamANDAssem;

		public bool IsNestedFamORAssem => (GetAttributeFlagsImpl() & TypeAttributes.VisibilityMask) == TypeAttributes.VisibilityMask;

		public bool IsAutoLayout => (GetAttributeFlagsImpl() & TypeAttributes.LayoutMask) == 0;

		public bool IsLayoutSequential => (GetAttributeFlagsImpl() & TypeAttributes.LayoutMask) == TypeAttributes.SequentialLayout;

		public bool IsExplicitLayout => (GetAttributeFlagsImpl() & TypeAttributes.LayoutMask) == TypeAttributes.ExplicitLayout;

		public bool IsClass
		{
			get
			{
				if ((GetAttributeFlagsImpl() & TypeAttributes.ClassSemanticsMask) == 0)
				{
					return !IsSubclassOf(valueType);
				}
				return false;
			}
		}

		public bool IsInterface
		{
			get
			{
				if (this is RuntimeType)
				{
					return GetTypeHandleInternal().IsInterface();
				}
				return (GetAttributeFlagsImpl() & TypeAttributes.ClassSemanticsMask) == TypeAttributes.ClassSemanticsMask;
			}
		}

		public bool IsValueType => IsValueTypeImpl();

		public bool IsAbstract => (GetAttributeFlagsImpl() & TypeAttributes.Abstract) != 0;

		public bool IsSealed => (GetAttributeFlagsImpl() & TypeAttributes.Sealed) != 0;

		public bool IsEnum => IsSubclassOf(enumType);

		public bool IsSpecialName => (GetAttributeFlagsImpl() & TypeAttributes.SpecialName) != 0;

		public bool IsImport => (GetAttributeFlagsImpl() & TypeAttributes.Import) != 0;

		public bool IsSerializable
		{
			get
			{
				if ((GetAttributeFlagsImpl() & TypeAttributes.Serializable) == 0)
				{
					return QuickSerializationCastCheck();
				}
				return true;
			}
		}

		public bool IsAnsiClass => (GetAttributeFlagsImpl() & TypeAttributes.StringFormatMask) == 0;

		public bool IsUnicodeClass => (GetAttributeFlagsImpl() & TypeAttributes.StringFormatMask) == TypeAttributes.UnicodeClass;

		public bool IsAutoClass => (GetAttributeFlagsImpl() & TypeAttributes.StringFormatMask) == TypeAttributes.AutoClass;

		public bool IsArray => IsArrayImpl();

		internal virtual bool IsSzArray => false;

		public virtual bool IsGenericType => false;

		public virtual bool IsGenericTypeDefinition => false;

		public virtual bool IsGenericParameter => false;

		public virtual int GenericParameterPosition
		{
			get
			{
				throw new InvalidOperationException(Environment.GetResourceString("Arg_NotGenericParameter"));
			}
		}

		public virtual bool ContainsGenericParameters
		{
			get
			{
				if (HasElementType)
				{
					return GetRootElementType().ContainsGenericParameters;
				}
				if (IsGenericParameter)
				{
					return true;
				}
				if (!IsGenericType)
				{
					return false;
				}
				Type[] genericArguments = GetGenericArguments();
				for (int i = 0; i < genericArguments.Length; i++)
				{
					if (genericArguments[i].ContainsGenericParameters)
					{
						return true;
					}
				}
				return false;
			}
		}

		public bool IsByRef => IsByRefImpl();

		public bool IsPointer => IsPointerImpl();

		public bool IsPrimitive => IsPrimitiveImpl();

		public bool IsCOMObject => IsCOMObjectImpl();

		public bool HasElementType => HasElementTypeImpl();

		public bool IsContextful => IsContextfulImpl();

		public bool IsMarshalByRef => IsMarshalByRefImpl();

		internal bool HasProxyAttribute => HasProxyAttributeImpl();

		public abstract Type UnderlyingSystemType
		{
			get;
		}

		static Type()
		{
			Missing = System.Reflection.Missing.Value;
			Delimiter = '.';
			EmptyTypes = new Type[0];
			valueType = typeof(ValueType);
			enumType = typeof(Enum);
			objectType = typeof(object);
			__Filters _Filters = new __Filters();
			FilterAttribute = _Filters.FilterAttribute;
			FilterName = _Filters.FilterName;
			FilterNameIgnoreCase = _Filters.FilterIgnoreCase;
		}

		public new Type GetType()
		{
			return base.GetType();
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public static Type GetType(string typeName, bool throwOnError, bool ignoreCase)
		{
			StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
			return RuntimeType.PrivateGetType(typeName, throwOnError, ignoreCase, ref stackMark);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public static Type GetType(string typeName, bool throwOnError)
		{
			StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
			return RuntimeType.PrivateGetType(typeName, throwOnError, ignoreCase: false, ref stackMark);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public static Type GetType(string typeName)
		{
			StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
			return RuntimeType.PrivateGetType(typeName, throwOnError: false, ignoreCase: false, ref stackMark);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public static Type ReflectionOnlyGetType(string typeName, bool throwIfNotFound, bool ignoreCase)
		{
			StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
			return RuntimeType.PrivateGetType(typeName, throwIfNotFound, ignoreCase, reflectionOnly: true, ref stackMark);
		}

		public virtual Type MakePointerType()
		{
			throw new NotSupportedException();
		}

		public virtual Type MakeByRefType()
		{
			throw new NotSupportedException();
		}

		public virtual Type MakeArrayType()
		{
			throw new NotSupportedException();
		}

		public virtual Type MakeArrayType(int rank)
		{
			throw new NotSupportedException();
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static Type GetTypeFromProgID(string progID)
		{
			return RuntimeType.GetTypeFromProgIDImpl(progID, null, throwOnError: false);
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static Type GetTypeFromProgID(string progID, bool throwOnError)
		{
			return RuntimeType.GetTypeFromProgIDImpl(progID, null, throwOnError);
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static Type GetTypeFromProgID(string progID, string server)
		{
			return RuntimeType.GetTypeFromProgIDImpl(progID, server, throwOnError: false);
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static Type GetTypeFromProgID(string progID, string server, bool throwOnError)
		{
			return RuntimeType.GetTypeFromProgIDImpl(progID, server, throwOnError);
		}

		public static Type GetTypeFromCLSID(Guid clsid)
		{
			return RuntimeType.GetTypeFromCLSIDImpl(clsid, null, throwOnError: false);
		}

		public static Type GetTypeFromCLSID(Guid clsid, bool throwOnError)
		{
			return RuntimeType.GetTypeFromCLSIDImpl(clsid, null, throwOnError);
		}

		public static Type GetTypeFromCLSID(Guid clsid, string server)
		{
			return RuntimeType.GetTypeFromCLSIDImpl(clsid, server, throwOnError: false);
		}

		public static Type GetTypeFromCLSID(Guid clsid, string server, bool throwOnError)
		{
			return RuntimeType.GetTypeFromCLSIDImpl(clsid, server, throwOnError);
		}

		internal string SigToString()
		{
			Type type = this;
			while (type.HasElementType)
			{
				type = type.GetElementType();
			}
			if (type.IsNested)
			{
				return Name;
			}
			string text = ToString();
			if (type.IsPrimitive || type == typeof(void) || type == typeof(TypedReference))
			{
				text = text.Substring("System.".Length);
			}
			return text;
		}

		public static TypeCode GetTypeCode(Type type)
		{
			return type?.GetTypeCodeInternal() ?? TypeCode.Empty;
		}

		internal virtual TypeCode GetTypeCodeInternal()
		{
			if (this is SymbolType)
			{
				return TypeCode.Object;
			}
			if (this is TypeBuilder)
			{
				TypeBuilder typeBuilder = (TypeBuilder)this;
				if (!typeBuilder.IsEnum)
				{
					return TypeCode.Object;
				}
			}
			if (this != UnderlyingSystemType)
			{
				return GetTypeCode(UnderlyingSystemType);
			}
			return TypeCode.Object;
		}

		private static void CreateBinder()
		{
			if (defaultBinder == null)
			{
				object value = new DefaultBinder();
				Interlocked.CompareExchange(ref defaultBinder, value, null);
			}
		}

		public abstract object InvokeMember(string name, BindingFlags invokeAttr, Binder binder, object target, object[] args, ParameterModifier[] modifiers, CultureInfo culture, string[] namedParameters);

		[DebuggerStepThrough]
		[DebuggerHidden]
		public object InvokeMember(string name, BindingFlags invokeAttr, Binder binder, object target, object[] args, CultureInfo culture)
		{
			return InvokeMember(name, invokeAttr, binder, target, args, null, culture, null);
		}

		[DebuggerHidden]
		[DebuggerStepThrough]
		public object InvokeMember(string name, BindingFlags invokeAttr, Binder binder, object target, object[] args)
		{
			return InvokeMember(name, invokeAttr, binder, target, args, null, null, null);
		}

		internal virtual RuntimeTypeHandle GetTypeHandleInternal()
		{
			return TypeHandle;
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		public static extern RuntimeTypeHandle GetTypeHandle(object o);

		[MethodImpl(MethodImplOptions.InternalCall)]
		public static extern Type GetTypeFromHandle(RuntimeTypeHandle handle);

		public virtual int GetArrayRank()
		{
			throw new NotSupportedException(Environment.GetResourceString("NotSupported_SubclassOverride"));
		}

		[ComVisible(true)]
		public ConstructorInfo GetConstructor(BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
		{
			if (types == null)
			{
				throw new ArgumentNullException("types");
			}
			for (int i = 0; i < types.Length; i++)
			{
				if (types[i] == null)
				{
					throw new ArgumentNullException("types");
				}
			}
			return GetConstructorImpl(bindingAttr, binder, callConvention, types, modifiers);
		}

		[ComVisible(true)]
		public ConstructorInfo GetConstructor(BindingFlags bindingAttr, Binder binder, Type[] types, ParameterModifier[] modifiers)
		{
			if (types == null)
			{
				throw new ArgumentNullException("types");
			}
			for (int i = 0; i < types.Length; i++)
			{
				if (types[i] == null)
				{
					throw new ArgumentNullException("types");
				}
			}
			return GetConstructorImpl(bindingAttr, binder, CallingConventions.Any, types, modifiers);
		}

		[ComVisible(true)]
		public ConstructorInfo GetConstructor(Type[] types)
		{
			return GetConstructor(BindingFlags.Instance | BindingFlags.Public, null, types, null);
		}

		protected abstract ConstructorInfo GetConstructorImpl(BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers);

		[ComVisible(true)]
		public ConstructorInfo[] GetConstructors()
		{
			return GetConstructors(BindingFlags.Instance | BindingFlags.Public);
		}

		[ComVisible(true)]
		public abstract ConstructorInfo[] GetConstructors(BindingFlags bindingAttr);

		public MethodInfo GetMethod(string name, BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			if (types == null)
			{
				throw new ArgumentNullException("types");
			}
			for (int i = 0; i < types.Length; i++)
			{
				if (types[i] == null)
				{
					throw new ArgumentNullException("types");
				}
			}
			return GetMethodImpl(name, bindingAttr, binder, callConvention, types, modifiers);
		}

		public MethodInfo GetMethod(string name, BindingFlags bindingAttr, Binder binder, Type[] types, ParameterModifier[] modifiers)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			if (types == null)
			{
				throw new ArgumentNullException("types");
			}
			for (int i = 0; i < types.Length; i++)
			{
				if (types[i] == null)
				{
					throw new ArgumentNullException("types");
				}
			}
			return GetMethodImpl(name, bindingAttr, binder, CallingConventions.Any, types, modifiers);
		}

		public MethodInfo GetMethod(string name, Type[] types, ParameterModifier[] modifiers)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			if (types == null)
			{
				throw new ArgumentNullException("types");
			}
			for (int i = 0; i < types.Length; i++)
			{
				if (types[i] == null)
				{
					throw new ArgumentNullException("types");
				}
			}
			return GetMethodImpl(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public, null, CallingConventions.Any, types, modifiers);
		}

		public MethodInfo GetMethod(string name, Type[] types)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			if (types == null)
			{
				throw new ArgumentNullException("types");
			}
			for (int i = 0; i < types.Length; i++)
			{
				if (types[i] == null)
				{
					throw new ArgumentNullException("types");
				}
			}
			return GetMethodImpl(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public, null, CallingConventions.Any, types, null);
		}

		public MethodInfo GetMethod(string name, BindingFlags bindingAttr)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			return GetMethodImpl(name, bindingAttr, null, CallingConventions.Any, null, null);
		}

		public MethodInfo GetMethod(string name)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			return GetMethodImpl(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public, null, CallingConventions.Any, null, null);
		}

		protected abstract MethodInfo GetMethodImpl(string name, BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers);

		public MethodInfo[] GetMethods()
		{
			return GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public);
		}

		public abstract MethodInfo[] GetMethods(BindingFlags bindingAttr);

		public abstract FieldInfo GetField(string name, BindingFlags bindingAttr);

		public FieldInfo GetField(string name)
		{
			return GetField(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public);
		}

		public FieldInfo[] GetFields()
		{
			return GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public);
		}

		public abstract FieldInfo[] GetFields(BindingFlags bindingAttr);

		public Type GetInterface(string name)
		{
			return GetInterface(name, ignoreCase: false);
		}

		public abstract Type GetInterface(string name, bool ignoreCase);

		public abstract Type[] GetInterfaces();

		public virtual Type[] FindInterfaces(TypeFilter filter, object filterCriteria)
		{
			if (filter == null)
			{
				throw new ArgumentNullException("filter");
			}
			Type[] interfaces = GetInterfaces();
			int num = 0;
			for (int i = 0; i < interfaces.Length; i++)
			{
				if (!filter(interfaces[i], filterCriteria))
				{
					interfaces[i] = null;
				}
				else
				{
					num++;
				}
			}
			if (num == interfaces.Length)
			{
				return interfaces;
			}
			Type[] array = new Type[num];
			num = 0;
			for (int j = 0; j < interfaces.Length; j++)
			{
				if (interfaces[j] != null)
				{
					array[num++] = interfaces[j];
				}
			}
			return array;
		}

		public EventInfo GetEvent(string name)
		{
			return GetEvent(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public);
		}

		public abstract EventInfo GetEvent(string name, BindingFlags bindingAttr);

		public virtual EventInfo[] GetEvents()
		{
			return GetEvents(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public);
		}

		public abstract EventInfo[] GetEvents(BindingFlags bindingAttr);

		public PropertyInfo GetProperty(string name, BindingFlags bindingAttr, Binder binder, Type returnType, Type[] types, ParameterModifier[] modifiers)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			if (types == null)
			{
				throw new ArgumentNullException("types");
			}
			return GetPropertyImpl(name, bindingAttr, binder, returnType, types, modifiers);
		}

		public PropertyInfo GetProperty(string name, Type returnType, Type[] types, ParameterModifier[] modifiers)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			if (types == null)
			{
				throw new ArgumentNullException("types");
			}
			return GetPropertyImpl(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public, null, returnType, types, modifiers);
		}

		public PropertyInfo GetProperty(string name, BindingFlags bindingAttr)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			return GetPropertyImpl(name, bindingAttr, null, null, null, null);
		}

		public PropertyInfo GetProperty(string name, Type returnType, Type[] types)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			if (types == null)
			{
				throw new ArgumentNullException("types");
			}
			return GetPropertyImpl(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public, null, returnType, types, null);
		}

		public PropertyInfo GetProperty(string name, Type[] types)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			if (types == null)
			{
				throw new ArgumentNullException("types");
			}
			return GetPropertyImpl(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public, null, null, types, null);
		}

		public PropertyInfo GetProperty(string name, Type returnType)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			if (returnType == null)
			{
				throw new ArgumentNullException("returnType");
			}
			return GetPropertyImpl(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public, null, returnType, null, null);
		}

		public PropertyInfo GetProperty(string name)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			return GetPropertyImpl(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public, null, null, null, null);
		}

		protected abstract PropertyInfo GetPropertyImpl(string name, BindingFlags bindingAttr, Binder binder, Type returnType, Type[] types, ParameterModifier[] modifiers);

		public abstract PropertyInfo[] GetProperties(BindingFlags bindingAttr);

		public PropertyInfo[] GetProperties()
		{
			return GetProperties(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public);
		}

		public Type[] GetNestedTypes()
		{
			return GetNestedTypes(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public);
		}

		public abstract Type[] GetNestedTypes(BindingFlags bindingAttr);

		public Type GetNestedType(string name)
		{
			return GetNestedType(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public);
		}

		public abstract Type GetNestedType(string name, BindingFlags bindingAttr);

		public MemberInfo[] GetMember(string name)
		{
			return GetMember(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public);
		}

		public virtual MemberInfo[] GetMember(string name, BindingFlags bindingAttr)
		{
			return GetMember(name, MemberTypes.All, bindingAttr);
		}

		public virtual MemberInfo[] GetMember(string name, MemberTypes type, BindingFlags bindingAttr)
		{
			throw new NotSupportedException(Environment.GetResourceString("NotSupported_SubclassOverride"));
		}

		public MemberInfo[] GetMembers()
		{
			return GetMembers(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public);
		}

		public abstract MemberInfo[] GetMembers(BindingFlags bindingAttr);

		public virtual MemberInfo[] GetDefaultMembers()
		{
			string text = (string)base.Cache[CacheObjType.DefaultMember];
			if (text == null)
			{
				CustomAttributeData customAttributeData = null;
				for (Type type = this; type != null; type = type.BaseType)
				{
					IList<CustomAttributeData> customAttributes = CustomAttributeData.GetCustomAttributes(type);
					for (int i = 0; i < customAttributes.Count; i++)
					{
						if (customAttributes[i].Constructor.DeclaringType == typeof(DefaultMemberAttribute))
						{
							customAttributeData = customAttributes[i];
							break;
						}
					}
					if (customAttributeData != null)
					{
						break;
					}
				}
				if (customAttributeData == null)
				{
					return new MemberInfo[0];
				}
				text = customAttributeData.ConstructorArguments[0].Value as string;
				base.Cache[CacheObjType.DefaultMember] = text;
			}
			MemberInfo[] array = GetMember(text);
			if (array == null)
			{
				array = new MemberInfo[0];
			}
			return array;
		}

		internal virtual string GetDefaultMemberName()
		{
			string text = (string)base.Cache[CacheObjType.DefaultMember];
			if (text == null)
			{
				object[] customAttributes = GetCustomAttributes(typeof(DefaultMemberAttribute), inherit: true);
				if (customAttributes.Length > 1)
				{
					throw new ExecutionEngineException(Environment.GetResourceString("ExecutionEngine_InvalidAttribute"));
				}
				if (customAttributes.Length == 0)
				{
					return null;
				}
				text = ((DefaultMemberAttribute)customAttributes[0]).MemberName;
				base.Cache[CacheObjType.DefaultMember] = text;
			}
			return text;
		}

		public virtual MemberInfo[] FindMembers(MemberTypes memberType, BindingFlags bindingAttr, MemberFilter filter, object filterCriteria)
		{
			MethodInfo[] array = null;
			ConstructorInfo[] array2 = null;
			FieldInfo[] array3 = null;
			PropertyInfo[] array4 = null;
			EventInfo[] array5 = null;
			Type[] array6 = null;
			int num = 0;
			int num2 = 0;
			if ((memberType & MemberTypes.Method) != 0)
			{
				array = GetMethods(bindingAttr);
				if (filter != null)
				{
					for (num = 0; num < array.Length; num++)
					{
						if (!filter(array[num], filterCriteria))
						{
							array[num] = null;
						}
						else
						{
							num2++;
						}
					}
				}
				else
				{
					num2 += array.Length;
				}
			}
			if ((memberType & MemberTypes.Constructor) != 0)
			{
				array2 = GetConstructors(bindingAttr);
				if (filter != null)
				{
					for (num = 0; num < array2.Length; num++)
					{
						if (!filter(array2[num], filterCriteria))
						{
							array2[num] = null;
						}
						else
						{
							num2++;
						}
					}
				}
				else
				{
					num2 += array2.Length;
				}
			}
			if ((memberType & MemberTypes.Field) != 0)
			{
				array3 = GetFields(bindingAttr);
				if (filter != null)
				{
					for (num = 0; num < array3.Length; num++)
					{
						if (!filter(array3[num], filterCriteria))
						{
							array3[num] = null;
						}
						else
						{
							num2++;
						}
					}
				}
				else
				{
					num2 += array3.Length;
				}
			}
			if ((memberType & MemberTypes.Property) != 0)
			{
				array4 = GetProperties(bindingAttr);
				if (filter != null)
				{
					for (num = 0; num < array4.Length; num++)
					{
						if (!filter(array4[num], filterCriteria))
						{
							array4[num] = null;
						}
						else
						{
							num2++;
						}
					}
				}
				else
				{
					num2 += array4.Length;
				}
			}
			if ((memberType & MemberTypes.Event) != 0)
			{
				array5 = GetEvents();
				if (filter != null)
				{
					for (num = 0; num < array5.Length; num++)
					{
						if (!filter(array5[num], filterCriteria))
						{
							array5[num] = null;
						}
						else
						{
							num2++;
						}
					}
				}
				else
				{
					num2 += array5.Length;
				}
			}
			if ((memberType & MemberTypes.NestedType) != 0)
			{
				array6 = GetNestedTypes(bindingAttr);
				if (filter != null)
				{
					for (num = 0; num < array6.Length; num++)
					{
						if (!filter(array6[num], filterCriteria))
						{
							array6[num] = null;
						}
						else
						{
							num2++;
						}
					}
				}
				else
				{
					num2 += array6.Length;
				}
			}
			MemberInfo[] array7 = new MemberInfo[num2];
			num2 = 0;
			if (array != null)
			{
				for (num = 0; num < array.Length; num++)
				{
					if (array[num] != null)
					{
						array7[num2++] = array[num];
					}
				}
			}
			if (array2 != null)
			{
				for (num = 0; num < array2.Length; num++)
				{
					if (array2[num] != null)
					{
						array7[num2++] = array2[num];
					}
				}
			}
			if (array3 != null)
			{
				for (num = 0; num < array3.Length; num++)
				{
					if (array3[num] != null)
					{
						array7[num2++] = array3[num];
					}
				}
			}
			if (array4 != null)
			{
				for (num = 0; num < array4.Length; num++)
				{
					if (array4[num] != null)
					{
						array7[num2++] = array4[num];
					}
				}
			}
			if (array5 != null)
			{
				for (num = 0; num < array5.Length; num++)
				{
					if (array5[num] != null)
					{
						array7[num2++] = array5[num];
					}
				}
			}
			if (array6 != null)
			{
				for (num = 0; num < array6.Length; num++)
				{
					if (array6[num] != null)
					{
						array7[num2++] = array6[num];
					}
				}
			}
			return array7;
		}

		private bool QuickSerializationCastCheck()
		{
			for (Type type = UnderlyingSystemType; type != null; type = type.BaseType)
			{
				if (type == typeof(Enum) || type == typeof(Delegate))
				{
					return true;
				}
			}
			return false;
		}

		public virtual Type[] GetGenericParameterConstraints()
		{
			if (!IsGenericParameter)
			{
				throw new InvalidOperationException(Environment.GetResourceString("Arg_NotGenericParameter"));
			}
			throw new InvalidOperationException();
		}

		protected virtual bool IsValueTypeImpl()
		{
			if (this == valueType || this == enumType)
			{
				return false;
			}
			return IsSubclassOf(valueType);
		}

		protected abstract TypeAttributes GetAttributeFlagsImpl();

		protected abstract bool IsArrayImpl();

		protected abstract bool IsByRefImpl();

		protected abstract bool IsPointerImpl();

		protected abstract bool IsPrimitiveImpl();

		protected abstract bool IsCOMObjectImpl();

		public virtual Type MakeGenericType(params Type[] typeArguments)
		{
			throw new NotSupportedException(Environment.GetResourceString("NotSupported_SubclassOverride"));
		}

		protected virtual bool IsContextfulImpl()
		{
			return typeof(ContextBoundObject).IsAssignableFrom(this);
		}

		protected virtual bool IsMarshalByRefImpl()
		{
			return typeof(MarshalByRefObject).IsAssignableFrom(this);
		}

		internal virtual bool HasProxyAttributeImpl()
		{
			return false;
		}

		public abstract Type GetElementType();

		public virtual Type[] GetGenericArguments()
		{
			throw new NotSupportedException(Environment.GetResourceString("NotSupported_SubclassOverride"));
		}

		public virtual Type GetGenericTypeDefinition()
		{
			throw new NotSupportedException(Environment.GetResourceString("NotSupported_SubclassOverride"));
		}

		protected abstract bool HasElementTypeImpl();

		internal virtual Type GetRootElementType()
		{
			Type type = this;
			while (type.HasElementType)
			{
				type = type.GetElementType();
			}
			return type;
		}

		[ComVisible(true)]
		public virtual bool IsSubclassOf(Type c)
		{
			Type type = this;
			if (type == c)
			{
				return false;
			}
			while (type != null)
			{
				if (type == c)
				{
					return true;
				}
				type = type.BaseType;
			}
			return false;
		}

		public virtual bool IsInstanceOfType(object o)
		{
			if (this is RuntimeType)
			{
				return IsInstanceOfType(o);
			}
			if (o == null)
			{
				return false;
			}
			if (RemotingServices.IsTransparentProxy(o))
			{
				return null != RemotingServices.CheckCast(o, this);
			}
			if (IsInterface && o.GetType().IsCOMObject && this is RuntimeType)
			{
				return ((RuntimeType)this).SupportsInterface(o);
			}
			return IsAssignableFrom(o.GetType());
		}

		public virtual bool IsAssignableFrom(Type c)
		{
			if (c == null)
			{
				return false;
			}
			try
			{
				RuntimeType runtimeType = c.UnderlyingSystemType as RuntimeType;
				RuntimeType runtimeType2 = UnderlyingSystemType as RuntimeType;
				if (runtimeType == null || runtimeType2 == null)
				{
					TypeBuilder typeBuilder = c as TypeBuilder;
					if (typeBuilder == null)
					{
						return false;
					}
					if (TypeBuilder.IsTypeEqual(this, c))
					{
						return true;
					}
					if (typeBuilder.IsSubclassOf(this))
					{
						return true;
					}
					if (!IsInterface)
					{
						return false;
					}
					Type[] interfaces = typeBuilder.GetInterfaces();
					for (int i = 0; i < interfaces.Length; i++)
					{
						if (TypeBuilder.IsTypeEqual(interfaces[i], this))
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
				return RuntimeType.CanCastTo(runtimeType, runtimeType2);
			}
			catch (ArgumentException)
			{
			}
			if (IsInterface)
			{
				Type[] interfaces2 = c.GetInterfaces();
				for (int j = 0; j < interfaces2.Length; j++)
				{
					if (this == interfaces2[j])
					{
						return true;
					}
				}
			}
			else
			{
				if (IsGenericParameter)
				{
					Type[] genericParameterConstraints = GetGenericParameterConstraints();
					for (int k = 0; k < genericParameterConstraints.Length; k++)
					{
						if (!genericParameterConstraints[k].IsAssignableFrom(c))
						{
							return false;
						}
					}
					return true;
				}
				while (c != null)
				{
					if (c == this)
					{
						return true;
					}
					c = c.BaseType;
				}
			}
			return false;
		}

		public override string ToString()
		{
			return "Type: " + Name;
		}

		public static Type[] GetTypeArray(object[] args)
		{
			if (args == null)
			{
				throw new ArgumentNullException("args");
			}
			Type[] array = new Type[args.Length];
			for (int i = 0; i < array.Length; i++)
			{
				if (args[i] == null)
				{
					throw new ArgumentNullException();
				}
				array[i] = args[i].GetType();
			}
			return array;
		}

		public override bool Equals(object o)
		{
			if (o == null)
			{
				return false;
			}
			if (!(o is Type))
			{
				return false;
			}
			return UnderlyingSystemType == ((Type)o).UnderlyingSystemType;
		}

		public bool Equals(Type o)
		{
			if (o == null)
			{
				return false;
			}
			return UnderlyingSystemType == o.UnderlyingSystemType;
		}

		public override int GetHashCode()
		{
			Type underlyingSystemType = UnderlyingSystemType;
			if (underlyingSystemType != this)
			{
				return underlyingSystemType.GetHashCode();
			}
			return base.GetHashCode();
		}

		[ComVisible(true)]
		public virtual InterfaceMapping GetInterfaceMap(Type interfaceType)
		{
			throw new NotSupportedException(Environment.GetResourceString("NotSupported_SubclassOverride"));
		}

		internal static Type ResolveTypeRelativeTo(string typeName, int offset, int count, Type serverType)
		{
			Type type = ResolveTypeRelativeToBaseTypes(typeName, offset, count, serverType);
			if (type == null)
			{
				Type[] interfaces = serverType.GetInterfaces();
				Type[] array = interfaces;
				foreach (Type type2 in array)
				{
					string fullName = type2.FullName;
					if (fullName.Length == count && string.CompareOrdinal(typeName, offset, fullName, 0, count) == 0)
					{
						return type2;
					}
				}
			}
			return type;
		}

		internal static Type ResolveTypeRelativeToBaseTypes(string typeName, int offset, int count, Type serverType)
		{
			if (typeName == null || serverType == null)
			{
				return null;
			}
			string fullName = serverType.FullName;
			if (fullName.Length == count && string.CompareOrdinal(typeName, offset, fullName, 0, count) == 0)
			{
				return serverType;
			}
			return ResolveTypeRelativeToBaseTypes(typeName, offset, count, serverType.BaseType);
		}

		void _Type.GetTypeInfoCount(out uint pcTInfo)
		{
			throw new NotImplementedException();
		}

		void _Type.GetTypeInfo(uint iTInfo, uint lcid, IntPtr ppTInfo)
		{
			throw new NotImplementedException();
		}

		void _Type.GetIDsOfNames([In] ref Guid riid, IntPtr rgszNames, uint cNames, uint lcid, IntPtr rgDispId)
		{
			throw new NotImplementedException();
		}

		void _Type.Invoke(uint dispIdMember, [In] ref Guid riid, uint lcid, short wFlags, IntPtr pDispParams, IntPtr pVarResult, IntPtr pExcepInfo, IntPtr puArgErr)
		{
			throw new NotImplementedException();
		}
	}
}
