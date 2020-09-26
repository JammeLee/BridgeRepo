using System.Collections;
using System.Diagnostics.SymbolStore;
using System.Globalization;
using System.IO;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security.Cryptography.X509Certificates;
using System.Security.Permissions;
using System.Threading;

namespace System.Reflection
{
	[Serializable]
	[ComVisible(true)]
	[ClassInterface(ClassInterfaceType.None)]
	[ComDefaultInterface(typeof(_Module))]
	public class Module : _Module, ISerializable, ICustomAttributeProvider
	{
		private const BindingFlags DefaultLookup = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public;

		public static readonly TypeFilter FilterTypeName;

		public static readonly TypeFilter FilterTypeNameIgnoreCase;

		internal ArrayList m__TypeBuilderList;

		internal ISymbolWriter m__iSymWriter;

		internal ModuleBuilderData m__moduleData;

		private RuntimeType m__runtimeType;

		private IntPtr m__pRefClass;

		internal IntPtr m__pData;

		internal IntPtr m__pInternalSymWriter;

		private IntPtr m__pGlobals;

		private IntPtr m__pFields;

		internal MethodToken m__EntryPoint;

		public int MDStreamVersion => GetModuleHandle().MDStreamVersion;

		internal virtual Module InternalModule => this;

		internal ArrayList m_TypeBuilderList
		{
			get
			{
				return InternalModule.m__TypeBuilderList;
			}
			set
			{
				InternalModule.m__TypeBuilderList = value;
			}
		}

		internal ISymbolWriter m_iSymWriter
		{
			get
			{
				return InternalModule.m__iSymWriter;
			}
			set
			{
				InternalModule.m__iSymWriter = value;
			}
		}

		internal ModuleBuilderData m_moduleData
		{
			get
			{
				return InternalModule.m__moduleData;
			}
			set
			{
				InternalModule.m__moduleData = value;
			}
		}

		private RuntimeType m_runtimeType
		{
			get
			{
				return InternalModule.m__runtimeType;
			}
			set
			{
				InternalModule.m__runtimeType = value;
			}
		}

		private IntPtr m_pRefClass => InternalModule.m__pRefClass;

		internal IntPtr m_pData => InternalModule.m__pData;

		internal IntPtr m_pInternalSymWriter => InternalModule.m__pInternalSymWriter;

		private IntPtr m_pGlobals => InternalModule.m__pGlobals;

		private IntPtr m_pFields => InternalModule.m__pFields;

		internal MethodToken m_EntryPoint
		{
			get
			{
				return InternalModule.m__EntryPoint;
			}
			set
			{
				InternalModule.m__EntryPoint = value;
			}
		}

		internal RuntimeType RuntimeType
		{
			get
			{
				if (m_runtimeType == null)
				{
					m_runtimeType = GetModuleHandle().GetModuleTypeHandle().GetRuntimeType();
				}
				return m_runtimeType;
			}
		}

		internal MetadataImport MetadataImport => ModuleHandle.GetMetadataImport();

		public virtual string FullyQualifiedName
		{
			get
			{
				string text = InternalGetFullyQualifiedName();
				if (text != null)
				{
					bool flag = true;
					try
					{
						Path.GetFullPathInternal(text);
					}
					catch (ArgumentException)
					{
						flag = false;
					}
					if (flag)
					{
						new FileIOPermission(FileIOPermissionAccess.PathDiscovery, text).Demand();
					}
				}
				return text;
			}
		}

		public Guid ModuleVersionId
		{
			get
			{
				MetadataImport.GetScopeProps(out var mvid);
				return mvid;
			}
		}

		public int MetadataToken => GetModuleHandle().GetToken();

		public string ScopeName => InternalGetName();

		public string Name
		{
			get
			{
				string text = InternalGetFullyQualifiedName();
				int num = text.LastIndexOf('\\');
				if (num == -1)
				{
					return text;
				}
				return new string(text.ToCharArray(), num + 1, text.Length - num - 1);
			}
		}

		public Assembly Assembly => GetAssemblyInternal();

		public unsafe ModuleHandle ModuleHandle => new ModuleHandle((void*)m_pData);

		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern Type _GetTypeInternal(string className, bool ignoreCase, bool throwOnError);

		internal Type GetTypeInternal(string className, bool ignoreCase, bool throwOnError)
		{
			return InternalModule._GetTypeInternal(className, ignoreCase, throwOnError);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern IntPtr _GetHINSTANCE();

		internal IntPtr GetHINSTANCE()
		{
			return InternalModule._GetHINSTANCE();
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern string _InternalGetName();

		private string InternalGetName()
		{
			return InternalModule._InternalGetName();
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern string _InternalGetFullyQualifiedName();

		internal string InternalGetFullyQualifiedName()
		{
			return InternalModule._InternalGetFullyQualifiedName();
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern Type[] _GetTypesInternal(ref StackCrawlMark stackMark);

		internal Type[] GetTypesInternal(ref StackCrawlMark stackMark)
		{
			return InternalModule._GetTypesInternal(ref stackMark);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal extern Assembly _GetAssemblyInternal();

		internal virtual Assembly GetAssemblyInternal()
		{
			return InternalModule._GetAssemblyInternal();
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern int _InternalGetTypeToken(string strFullName, Module refedModule, string strRefedModuleFileName, int tkResolution);

		internal int InternalGetTypeToken(string strFullName, Module refedModule, string strRefedModuleFileName, int tkResolution)
		{
			return InternalModule._InternalGetTypeToken(strFullName, refedModule.InternalModule, strRefedModuleFileName, tkResolution);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern Type _InternalLoadInMemoryTypeByName(string className);

		internal Type InternalLoadInMemoryTypeByName(string className)
		{
			return InternalModule._InternalLoadInMemoryTypeByName(className);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern int _InternalGetMemberRef(Module refedModule, int tr, int defToken);

		internal int InternalGetMemberRef(Module refedModule, int tr, int defToken)
		{
			return InternalModule._InternalGetMemberRef(refedModule.InternalModule, tr, defToken);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern int _InternalGetMemberRefFromSignature(int tr, string methodName, byte[] signature, int length);

		internal int InternalGetMemberRefFromSignature(int tr, string methodName, byte[] signature, int length)
		{
			return InternalModule._InternalGetMemberRefFromSignature(tr, methodName, signature, length);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern int _InternalGetMemberRefOfMethodInfo(int tr, IntPtr method);

		internal int InternalGetMemberRefOfMethodInfo(int tr, RuntimeMethodHandle method)
		{
			return InternalModule._InternalGetMemberRefOfMethodInfo(tr, method.Value);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern int _InternalGetMemberRefOfFieldInfo(int tkType, IntPtr interfaceHandle, int tkField);

		internal int InternalGetMemberRefOfFieldInfo(int tkType, RuntimeTypeHandle declaringType, int tkField)
		{
			return InternalModule._InternalGetMemberRefOfFieldInfo(tkType, declaringType.Value, tkField);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern int _InternalGetTypeSpecTokenWithBytes(byte[] signature, int length);

		internal int InternalGetTypeSpecTokenWithBytes(byte[] signature, int length)
		{
			return InternalModule._InternalGetTypeSpecTokenWithBytes(signature, length);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern int _nativeGetArrayMethodToken(int tkTypeSpec, string methodName, byte[] signature, int sigLength, int baseToken);

		internal int nativeGetArrayMethodToken(int tkTypeSpec, string methodName, byte[] signature, int sigLength, int baseToken)
		{
			return InternalModule._nativeGetArrayMethodToken(tkTypeSpec, methodName, signature, sigLength, baseToken);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern void _InternalSetFieldRVAContent(int fdToken, byte[] data, int length);

		internal void InternalSetFieldRVAContent(int fdToken, byte[] data, int length)
		{
			InternalModule._InternalSetFieldRVAContent(fdToken, data, length);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern int _InternalGetStringConstant(string str);

		internal int InternalGetStringConstant(string str)
		{
			return InternalModule._InternalGetStringConstant(str);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern void _InternalPreSavePEFile(int portableExecutableKind, int imageFileMachine);

		internal void InternalPreSavePEFile(int portableExecutableKind, int imageFileMachine)
		{
			InternalModule._InternalPreSavePEFile(portableExecutableKind, imageFileMachine);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern void _InternalSavePEFile(string fileName, int entryPoint, int isExe, bool isManifestFile);

		internal void InternalSavePEFile(string fileName, MethodToken entryPoint, int isExe, bool isManifestFile)
		{
			InternalModule._InternalSavePEFile(fileName, entryPoint.Token, isExe, isManifestFile);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern void _InternalSetResourceCounts(int resCount);

		internal void InternalSetResourceCounts(int resCount)
		{
			InternalModule._InternalSetResourceCounts(resCount);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern void _InternalAddResource(string strName, byte[] resBytes, int resByteCount, int tkFile, int attribute, int portableExecutableKind, int imageFileMachine);

		internal void InternalAddResource(string strName, byte[] resBytes, int resByteCount, int tkFile, int attribute, int portableExecutableKind, int imageFileMachine)
		{
			InternalModule._InternalAddResource(strName, resBytes, resByteCount, tkFile, attribute, portableExecutableKind, imageFileMachine);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern void _InternalSetModuleProps(string strModuleName);

		internal void InternalSetModuleProps(string strModuleName)
		{
			InternalModule._InternalSetModuleProps(strModuleName);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern bool _IsResourceInternal();

		internal bool IsResourceInternal()
		{
			return InternalModule._IsResourceInternal();
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern X509Certificate _GetSignerCertificateInternal();

		internal X509Certificate GetSignerCertificateInternal()
		{
			return InternalModule._GetSignerCertificateInternal();
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern void _InternalDefineNativeResourceFile(string strFilename, int portableExecutableKind, int ImageFileMachine);

		internal void InternalDefineNativeResourceFile(string strFilename, int portableExecutableKind, int ImageFileMachine)
		{
			InternalModule._InternalDefineNativeResourceFile(strFilename, portableExecutableKind, ImageFileMachine);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern void _InternalDefineNativeResourceBytes(byte[] resource, int portableExecutableKind, int imageFileMachine);

		internal void InternalDefineNativeResourceBytes(byte[] resource, int portableExecutableKind, int imageFileMachine)
		{
			InternalModule._InternalDefineNativeResourceBytes(resource, portableExecutableKind, imageFileMachine);
		}

		static Module()
		{
			__Filters _Filters = new __Filters();
			FilterTypeName = _Filters.FilterTypeName;
			FilterTypeNameIgnoreCase = _Filters.FilterTypeNameIgnoreCase;
		}

		public MethodBase ResolveMethod(int metadataToken)
		{
			return ResolveMethod(metadataToken, null, null);
		}

		private static RuntimeTypeHandle[] ConvertToTypeHandleArray(Type[] genericArguments)
		{
			if (genericArguments == null)
			{
				return null;
			}
			int num = genericArguments.Length;
			RuntimeTypeHandle[] array = new RuntimeTypeHandle[num];
			for (int i = 0; i < num; i++)
			{
				Type type = genericArguments[i];
				if (type == null)
				{
					throw new ArgumentException(Environment.GetResourceString("Argument_InvalidGenericInstArray"));
				}
				type = type.UnderlyingSystemType;
				if (type == null)
				{
					throw new ArgumentException(Environment.GetResourceString("Argument_InvalidGenericInstArray"));
				}
				if (!(type is RuntimeType))
				{
					throw new ArgumentException(Environment.GetResourceString("Argument_InvalidGenericInstArray"));
				}
				ref RuntimeTypeHandle reference = ref array[i];
				reference = type.GetTypeHandleInternal();
			}
			return array;
		}

		public byte[] ResolveSignature(int metadataToken)
		{
			MetadataToken metadataToken2 = new MetadataToken(metadataToken);
			if (!MetadataImport.IsValidToken(metadataToken2))
			{
				throw new ArgumentOutOfRangeException("metadataToken", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Argument_InvalidToken", metadataToken2, this)));
			}
			if (!metadataToken2.IsMemberRef && !metadataToken2.IsMethodDef && !metadataToken2.IsTypeSpec && !metadataToken2.IsSignature && !metadataToken2.IsFieldDef)
			{
				throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Argument_InvalidToken", metadataToken2, this)), "metadataToken");
			}
			ConstArray constArray = ((!metadataToken2.IsMemberRef) ? MetadataImport.GetSignatureFromToken(metadataToken) : MetadataImport.GetMemberRefProps(metadataToken));
			byte[] array = new byte[constArray.Length];
			for (int i = 0; i < constArray.Length; i++)
			{
				array[i] = constArray[i];
			}
			return array;
		}

		public unsafe MethodBase ResolveMethod(int metadataToken, Type[] genericTypeArguments, Type[] genericMethodArguments)
		{
			MetadataToken metadataToken2 = new MetadataToken(metadataToken);
			if (!MetadataImport.IsValidToken(metadataToken2))
			{
				throw new ArgumentOutOfRangeException("metadataToken", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Argument_InvalidToken", metadataToken2, this)));
			}
			RuntimeTypeHandle[] typeInstantiationContext = ConvertToTypeHandleArray(genericTypeArguments);
			RuntimeTypeHandle[] methodInstantiationContext = ConvertToTypeHandleArray(genericMethodArguments);
			try
			{
				if (!metadataToken2.IsMethodDef && !metadataToken2.IsMethodSpec)
				{
					if (!metadataToken2.IsMemberRef)
					{
						throw new ArgumentException("metadataToken", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Argument_ResolveMethod", metadataToken2, this)));
					}
					if (*(byte*)MetadataImport.GetMemberRefProps(metadataToken2).Signature.ToPointer() == 6)
					{
						throw new ArgumentException("metadataToken", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Argument_ResolveMethod"), metadataToken2, this));
					}
				}
				RuntimeMethodHandle methodHandle = GetModuleHandle().ResolveMethodHandle(metadataToken2, typeInstantiationContext, methodInstantiationContext);
				Type type = methodHandle.GetDeclaringType().GetRuntimeType();
				if (type.IsGenericType || type.IsArray)
				{
					MetadataToken token = new MetadataToken(MetadataImport.GetParentToken(metadataToken2));
					if (metadataToken2.IsMethodSpec)
					{
						token = new MetadataToken(MetadataImport.GetParentToken(token));
					}
					type = ResolveType(token, genericTypeArguments, genericMethodArguments);
				}
				return RuntimeType.GetMethodBase(type.GetTypeHandleInternal(), methodHandle);
			}
			catch (BadImageFormatException innerException)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_BadImageFormatExceptionResolve"), innerException);
			}
		}

		internal FieldInfo ResolveLiteralField(int metadataToken, Type[] genericTypeArguments, Type[] genericMethodArguments)
		{
			MetadataToken metadataToken2 = new MetadataToken(metadataToken);
			if (!MetadataImport.IsValidToken(metadataToken2))
			{
				throw new ArgumentOutOfRangeException("metadataToken", string.Format(CultureInfo.CurrentUICulture, Environment.GetResourceString("Argument_InvalidToken", metadataToken2, this)));
			}
			string name = MetadataImport.GetName(metadataToken2).ToString();
			int parentToken = MetadataImport.GetParentToken(metadataToken2);
			Type type = ResolveType(parentToken, genericTypeArguments, genericMethodArguments);
			type.GetFields();
			try
			{
				return type.GetField(name, BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			}
			catch
			{
				throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Argument_ResolveField"), metadataToken2, this), "metadataToken");
			}
		}

		public FieldInfo ResolveField(int metadataToken)
		{
			return ResolveField(metadataToken, null, null);
		}

		public unsafe FieldInfo ResolveField(int metadataToken, Type[] genericTypeArguments, Type[] genericMethodArguments)
		{
			MetadataToken metadataToken2 = new MetadataToken(metadataToken);
			if (!MetadataImport.IsValidToken(metadataToken2))
			{
				throw new ArgumentOutOfRangeException("metadataToken", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Argument_InvalidToken", metadataToken2, this)));
			}
			RuntimeTypeHandle[] typeInstantiationContext = ConvertToTypeHandleArray(genericTypeArguments);
			RuntimeTypeHandle[] methodInstantiationContext = ConvertToTypeHandleArray(genericMethodArguments);
			try
			{
				RuntimeFieldHandle runtimeFieldHandle = default(RuntimeFieldHandle);
				if (!metadataToken2.IsFieldDef)
				{
					if (!metadataToken2.IsMemberRef)
					{
						throw new ArgumentException("metadataToken", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Argument_ResolveField"), metadataToken2, this));
					}
					if (*(byte*)MetadataImport.GetMemberRefProps(metadataToken2).Signature.ToPointer() != 6)
					{
						throw new ArgumentException("metadataToken", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Argument_ResolveField"), metadataToken2, this));
					}
					runtimeFieldHandle = GetModuleHandle().ResolveFieldHandle(metadataToken2, typeInstantiationContext, methodInstantiationContext);
				}
				runtimeFieldHandle = GetModuleHandle().ResolveFieldHandle(metadataToken, typeInstantiationContext, methodInstantiationContext);
				Type type = runtimeFieldHandle.GetApproxDeclaringType().GetRuntimeType();
				if (type.IsGenericType || type.IsArray)
				{
					int parentToken = GetModuleHandle().GetMetadataImport().GetParentToken(metadataToken);
					type = ResolveType(parentToken, genericTypeArguments, genericMethodArguments);
				}
				return RuntimeType.GetFieldInfo(type.GetTypeHandleInternal(), runtimeFieldHandle);
			}
			catch (MissingFieldException)
			{
				return ResolveLiteralField(metadataToken2, genericTypeArguments, genericMethodArguments);
			}
			catch (BadImageFormatException innerException)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_BadImageFormatExceptionResolve"), innerException);
			}
		}

		public Type ResolveType(int metadataToken)
		{
			return ResolveType(metadataToken, null, null);
		}

		public Type ResolveType(int metadataToken, Type[] genericTypeArguments, Type[] genericMethodArguments)
		{
			MetadataToken metadataToken2 = new MetadataToken(metadataToken);
			if (metadataToken2.IsGlobalTypeDefToken)
			{
				throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Argument_ResolveModuleType"), metadataToken2), "metadataToken");
			}
			if (!MetadataImport.IsValidToken(metadataToken2))
			{
				throw new ArgumentOutOfRangeException("metadataToken", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Argument_InvalidToken", metadataToken2, this)));
			}
			if (!metadataToken2.IsTypeDef && !metadataToken2.IsTypeSpec && !metadataToken2.IsTypeRef)
			{
				throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Argument_ResolveType"), metadataToken2, this), "metadataToken");
			}
			RuntimeTypeHandle[] typeInstantiationContext = ConvertToTypeHandleArray(genericTypeArguments);
			RuntimeTypeHandle[] methodInstantiationContext = ConvertToTypeHandleArray(genericMethodArguments);
			try
			{
				Type runtimeType = GetModuleHandle().ResolveTypeHandle(metadataToken, typeInstantiationContext, methodInstantiationContext).GetRuntimeType();
				if (runtimeType == null)
				{
					throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Argument_ResolveType"), metadataToken2, this), "metadataToken");
				}
				return runtimeType;
			}
			catch (BadImageFormatException innerException)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_BadImageFormatExceptionResolve"), innerException);
			}
		}

		public MemberInfo ResolveMember(int metadataToken)
		{
			return ResolveMember(metadataToken, null, null);
		}

		public unsafe MemberInfo ResolveMember(int metadataToken, Type[] genericTypeArguments, Type[] genericMethodArguments)
		{
			MetadataToken metadataToken2 = new MetadataToken(metadataToken);
			if (metadataToken2.IsProperty)
			{
				throw new ArgumentException(Environment.GetResourceString("InvalidOperation_PropertyInfoNotAvailable"));
			}
			if (metadataToken2.IsEvent)
			{
				throw new ArgumentException(Environment.GetResourceString("InvalidOperation_EventInfoNotAvailable"));
			}
			if (metadataToken2.IsMethodSpec || metadataToken2.IsMethodDef)
			{
				return ResolveMethod(metadataToken, genericTypeArguments, genericMethodArguments);
			}
			if (metadataToken2.IsFieldDef)
			{
				return ResolveField(metadataToken, genericTypeArguments, genericMethodArguments);
			}
			if (metadataToken2.IsTypeRef || metadataToken2.IsTypeDef || metadataToken2.IsTypeSpec)
			{
				return ResolveType(metadataToken, genericTypeArguments, genericMethodArguments);
			}
			if (metadataToken2.IsMemberRef)
			{
				if (!MetadataImport.IsValidToken(metadataToken2))
				{
					throw new ArgumentOutOfRangeException("metadataToken", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Argument_InvalidToken", metadataToken2, this)));
				}
				if (*(byte*)MetadataImport.GetMemberRefProps(metadataToken2).Signature.ToPointer() == 6)
				{
					return ResolveField(metadataToken2, genericTypeArguments, genericMethodArguments);
				}
				return ResolveMethod(metadataToken2, genericTypeArguments, genericMethodArguments);
			}
			throw new ArgumentException("metadataToken", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Argument_ResolveMember", metadataToken2, this)));
		}

		public string ResolveString(int metadataToken)
		{
			MetadataToken metadataToken2 = new MetadataToken(metadataToken);
			if (!metadataToken2.IsString)
			{
				throw new ArgumentException(string.Format(CultureInfo.CurrentUICulture, Environment.GetResourceString("Argument_ResolveString"), metadataToken, ToString()));
			}
			if (!MetadataImport.IsValidToken(metadataToken2))
			{
				throw new ArgumentOutOfRangeException("metadataToken", string.Format(CultureInfo.CurrentUICulture, Environment.GetResourceString("Argument_InvalidToken", metadataToken2, this)));
			}
			string userString = MetadataImport.GetUserString(metadataToken);
			if (userString == null)
			{
				throw new ArgumentException(string.Format(CultureInfo.CurrentUICulture, Environment.GetResourceString("Argument_ResolveString"), metadataToken, ToString()));
			}
			return userString;
		}

		public void GetPEKind(out PortableExecutableKinds peKind, out ImageFileMachine machine)
		{
			GetModuleHandle().GetPEKind(out peKind, out machine);
		}

		public override bool Equals(object o)
		{
			if (o == null)
			{
				return false;
			}
			if (!(o is Module))
			{
				return false;
			}
			Module module = o as Module;
			module = module.InternalModule;
			return InternalModule == module;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		internal Module()
		{
		}

		private FieldInfo InternalGetField(string name, BindingFlags bindingAttr)
		{
			if (RuntimeType == null)
			{
				return null;
			}
			return RuntimeType.GetField(name, bindingAttr);
		}

		internal virtual bool IsDynamic()
		{
			return false;
		}

		protected virtual MethodInfo GetMethodImpl(string name, BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
		{
			if (RuntimeType == null)
			{
				return null;
			}
			if (types == null)
			{
				return RuntimeType.GetMethod(name, bindingAttr);
			}
			return RuntimeType.GetMethod(name, bindingAttr, binder, callConvention, types, modifiers);
		}

		public virtual object[] GetCustomAttributes(bool inherit)
		{
			return CustomAttribute.GetCustomAttributes(this, typeof(object) as RuntimeType);
		}

		public virtual object[] GetCustomAttributes(Type attributeType, bool inherit)
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

		public virtual bool IsDefined(Type attributeType, bool inherit)
		{
			if (attributeType == null)
			{
				throw new ArgumentNullException("attributeType");
			}
			RuntimeType runtimeType = attributeType.UnderlyingSystemType as RuntimeType;
			if (runtimeType == null)
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_MustBeType"), "caType");
			}
			return CustomAttribute.IsDefined(this, runtimeType);
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
		public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			UnitySerializationHolder.GetUnitySerializationInfo(info, 5, ScopeName, GetAssemblyInternal());
		}

		[ComVisible(true)]
		public virtual Type GetType(string className, bool ignoreCase)
		{
			return GetType(className, throwOnError: false, ignoreCase);
		}

		[ComVisible(true)]
		public virtual Type GetType(string className)
		{
			return GetType(className, throwOnError: false, ignoreCase: false);
		}

		[ComVisible(true)]
		public virtual Type GetType(string className, bool throwOnError, bool ignoreCase)
		{
			return GetTypeInternal(className, throwOnError, ignoreCase);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public virtual Type[] FindTypes(TypeFilter filter, object filterCriteria)
		{
			StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
			Type[] typesInternal = GetTypesInternal(ref stackMark);
			int num = 0;
			for (int i = 0; i < typesInternal.Length; i++)
			{
				if (filter != null && !filter(typesInternal[i], filterCriteria))
				{
					typesInternal[i] = null;
				}
				else
				{
					num++;
				}
			}
			if (num == typesInternal.Length)
			{
				return typesInternal;
			}
			Type[] array = new Type[num];
			num = 0;
			for (int j = 0; j < typesInternal.Length; j++)
			{
				if (typesInternal[j] != null)
				{
					array[num++] = typesInternal[j];
				}
			}
			return array;
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public virtual Type[] GetTypes()
		{
			StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
			return GetTypesInternal(ref stackMark);
		}

		public bool IsResource()
		{
			return IsResourceInternal();
		}

		public FieldInfo[] GetFields()
		{
			if (RuntimeType == null)
			{
				return new FieldInfo[0];
			}
			return RuntimeType.GetFields();
		}

		public FieldInfo[] GetFields(BindingFlags bindingFlags)
		{
			if (RuntimeType == null)
			{
				return new FieldInfo[0];
			}
			return RuntimeType.GetFields(bindingFlags);
		}

		public FieldInfo GetField(string name)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			return GetField(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public);
		}

		public FieldInfo GetField(string name, BindingFlags bindingAttr)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			return InternalGetField(name, bindingAttr);
		}

		public MethodInfo[] GetMethods()
		{
			if (RuntimeType == null)
			{
				return new MethodInfo[0];
			}
			return RuntimeType.GetMethods();
		}

		public MethodInfo[] GetMethods(BindingFlags bindingFlags)
		{
			if (RuntimeType == null)
			{
				return new MethodInfo[0];
			}
			return RuntimeType.GetMethods(bindingFlags);
		}

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

		public MethodInfo GetMethod(string name)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			return GetMethodImpl(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public, null, CallingConventions.Any, null, null);
		}

		public override string ToString()
		{
			return ScopeName;
		}

		internal unsafe ModuleHandle GetModuleHandle()
		{
			return new ModuleHandle((void*)m_pData);
		}

		public X509Certificate GetSignerCertificate()
		{
			return GetSignerCertificateInternal();
		}

		void _Module.GetTypeInfoCount(out uint pcTInfo)
		{
			throw new NotImplementedException();
		}

		void _Module.GetTypeInfo(uint iTInfo, uint lcid, IntPtr ppTInfo)
		{
			throw new NotImplementedException();
		}

		void _Module.GetIDsOfNames([In] ref Guid riid, IntPtr rgszNames, uint cNames, uint lcid, IntPtr rgDispId)
		{
			throw new NotImplementedException();
		}

		void _Module.Invoke(uint dispIdMember, [In] ref Guid riid, uint lcid, short wFlags, IntPtr pDispParams, IntPtr pVarResult, IntPtr pExcepInfo, IntPtr puArgErr)
		{
			throw new NotImplementedException();
		}
	}
}
