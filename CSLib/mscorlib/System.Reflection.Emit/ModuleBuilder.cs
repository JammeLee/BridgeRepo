using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
using System.Globalization;
using System.IO;
using System.Resources;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;

namespace System.Reflection.Emit
{
	[ComDefaultInterface(typeof(_ModuleBuilder))]
	[ComVisible(true)]
	[ClassInterface(ClassInterfaceType.None)]
	[HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort = true)]
	public class ModuleBuilder : Module, _ModuleBuilder
	{
		internal ModuleBuilder m_internalModuleBuilder;

		private AssemblyBuilder m_assemblyBuilder;

		private static readonly Dictionary<ModuleBuilder, ModuleBuilder> s_moduleBuilders = new Dictionary<ModuleBuilder, ModuleBuilder>();

		private bool IsInternal => m_internalModuleBuilder == null;

		internal override Module InternalModule
		{
			get
			{
				if (IsInternal)
				{
					return this;
				}
				return m_internalModuleBuilder;
			}
		}

		public override string FullyQualifiedName
		{
			get
			{
				string text = base.m_moduleData.m_strFileName;
				if (text == null)
				{
					return null;
				}
				if (base.Assembly.m_assemblyData.m_strDir != null)
				{
					text = Path.Combine(base.Assembly.m_assemblyData.m_strDir, text);
					text = Path.GetFullPath(text);
				}
				if (base.Assembly.m_assemblyData.m_strDir != null && text != null)
				{
					new FileIOPermission(FileIOPermissionAccess.PathDiscovery, text).Demand();
				}
				return text;
			}
		}

		internal static string UnmangleTypeName(string typeName)
		{
			int startIndex = typeName.Length - 1;
			while (true)
			{
				startIndex = typeName.LastIndexOf('+', startIndex);
				if (startIndex == -1)
				{
					break;
				}
				bool flag = true;
				int num = startIndex;
				while (typeName[--num] == '\\')
				{
					flag = !flag;
				}
				if (flag)
				{
					break;
				}
				startIndex = num;
			}
			return typeName.Substring(startIndex + 1);
		}

		internal static Module GetModuleBuilder(Module module)
		{
			ModuleBuilder moduleBuilder = module.InternalModule as ModuleBuilder;
			if (moduleBuilder == null)
			{
				return module;
			}
			ModuleBuilder value = null;
			lock (s_moduleBuilders)
			{
				if (s_moduleBuilders.TryGetValue(moduleBuilder, out value))
				{
					return value;
				}
				return moduleBuilder;
			}
		}

		internal ModuleBuilder(AssemblyBuilder assemblyBuilder, ModuleBuilder internalModuleBuilder)
		{
			m_internalModuleBuilder = internalModuleBuilder;
			m_assemblyBuilder = assemblyBuilder;
			lock (s_moduleBuilders)
			{
				s_moduleBuilders[internalModuleBuilder] = this;
			}
		}

		private Type GetType(string strFormat, Type baseType)
		{
			if (strFormat == null || strFormat.Equals(string.Empty))
			{
				return baseType;
			}
			char[] bFormat = strFormat.ToCharArray();
			return SymbolType.FormCompoundType(bFormat, baseType, 0);
		}

		internal void CheckContext(params Type[][] typess)
		{
			((AssemblyBuilder)base.Assembly).CheckContext(typess);
		}

		internal void CheckContext(params Type[] types)
		{
			((AssemblyBuilder)base.Assembly).CheckContext(types);
		}

		private void DemandGrantedAssemblyPermission()
		{
			AssemblyBuilder assemblyBuilder = (AssemblyBuilder)base.Assembly;
			assemblyBuilder.DemandGrantedPermission();
		}

		internal virtual Type FindTypeBuilderWithName(string strTypeName, bool ignoreCase)
		{
			int count = base.m_TypeBuilderList.Count;
			Type type = null;
			int i;
			for (i = 0; i < count; i++)
			{
				type = (Type)base.m_TypeBuilderList[i];
				if (ignoreCase)
				{
					if (string.Compare(type.FullName, strTypeName, ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal) == 0)
					{
						break;
					}
				}
				else if (type.FullName.Equals(strTypeName))
				{
					break;
				}
			}
			if (i == count)
			{
				type = null;
			}
			return type;
		}

		internal Type GetRootElementType(Type type)
		{
			if (!type.IsByRef && !type.IsPointer && !type.IsArray)
			{
				return type;
			}
			return GetRootElementType(type.GetElementType());
		}

		internal void SetEntryPoint(MethodInfo entryPoint)
		{
			base.m_EntryPoint = GetMethodTokenInternal(entryPoint);
		}

		internal void PreSave(string fileName, PortableExecutableKinds portableExecutableKind, ImageFileMachine imageFileMachine)
		{
			if (base.Assembly.m_assemblyData.m_isSynchronized)
			{
				lock (base.Assembly.m_assemblyData)
				{
					PreSaveNoLock(fileName, portableExecutableKind, imageFileMachine);
				}
			}
			else
			{
				PreSaveNoLock(fileName, portableExecutableKind, imageFileMachine);
			}
		}

		private void PreSaveNoLock(string fileName, PortableExecutableKinds portableExecutableKind, ImageFileMachine imageFileMachine)
		{
			if (base.m_moduleData.m_isSaved)
			{
				throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, Environment.GetResourceString("InvalidOperation_ModuleHasBeenSaved"), base.m_moduleData.m_strModuleName));
			}
			if (!base.m_moduleData.m_fGlobalBeenCreated && base.m_moduleData.m_fHasGlobal)
			{
				throw new NotSupportedException(Environment.GetResourceString("NotSupported_GlobalFunctionNotBaked"));
			}
			int count = base.m_TypeBuilderList.Count;
			for (int i = 0; i < count; i++)
			{
				object obj = base.m_TypeBuilderList[i];
				TypeBuilder typeBuilder;
				if (obj is TypeBuilder)
				{
					typeBuilder = (TypeBuilder)obj;
				}
				else
				{
					EnumBuilder enumBuilder = (EnumBuilder)obj;
					typeBuilder = enumBuilder.m_typeBuilder;
				}
				if (!typeBuilder.m_hasBeenCreated && !typeBuilder.m_isHiddenType)
				{
					throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, Environment.GetResourceString("NotSupported_NotAllTypesAreBaked"), typeBuilder.FullName));
				}
			}
			InternalPreSavePEFile((int)portableExecutableKind, (int)imageFileMachine);
		}

		internal void Save(string fileName, bool isAssemblyFile, PortableExecutableKinds portableExecutableKind, ImageFileMachine imageFileMachine)
		{
			if (base.Assembly.m_assemblyData.m_isSynchronized)
			{
				lock (base.Assembly.m_assemblyData)
				{
					SaveNoLock(fileName, isAssemblyFile, portableExecutableKind, imageFileMachine);
				}
			}
			else
			{
				SaveNoLock(fileName, isAssemblyFile, portableExecutableKind, imageFileMachine);
			}
		}

		private void SaveNoLock(string fileName, bool isAssemblyFile, PortableExecutableKinds portableExecutableKind, ImageFileMachine imageFileMachine)
		{
			if (base.m_moduleData.m_embeddedRes != null)
			{
				ResWriterData resWriterData = base.m_moduleData.m_embeddedRes;
				int num = 0;
				while (resWriterData != null)
				{
					resWriterData = resWriterData.m_nextResWriter;
					num++;
				}
				InternalSetResourceCounts(num);
				for (resWriterData = base.m_moduleData.m_embeddedRes; resWriterData != null; resWriterData = resWriterData.m_nextResWriter)
				{
					if (resWriterData.m_resWriter != null)
					{
						resWriterData.m_resWriter.Generate();
					}
					byte[] array = new byte[resWriterData.m_memoryStream.Length];
					resWriterData.m_memoryStream.Flush();
					resWriterData.m_memoryStream.Position = 0L;
					resWriterData.m_memoryStream.Read(array, 0, array.Length);
					InternalAddResource(resWriterData.m_strName, array, array.Length, base.m_moduleData.m_tkFile, (int)resWriterData.m_attribute, (int)portableExecutableKind, (int)imageFileMachine);
				}
			}
			if (base.m_moduleData.m_strResourceFileName != null)
			{
				InternalDefineNativeResourceFile(base.m_moduleData.m_strResourceFileName, (int)portableExecutableKind, (int)imageFileMachine);
			}
			else if (base.m_moduleData.m_resourceBytes != null)
			{
				InternalDefineNativeResourceBytes(base.m_moduleData.m_resourceBytes, (int)portableExecutableKind, (int)imageFileMachine);
			}
			if (isAssemblyFile)
			{
				InternalSavePEFile(fileName, base.m_EntryPoint, (int)base.Assembly.m_assemblyData.m_peFileKind, isManifestFile: true);
			}
			else
			{
				InternalSavePEFile(fileName, base.m_EntryPoint, 1, isManifestFile: false);
			}
			base.m_moduleData.m_isSaved = true;
		}

		internal int GetTypeRefNested(Type type, Module refedModule, string strRefedModuleFileName)
		{
			Type declaringType = type.DeclaringType;
			int tkResolution = 0;
			string text = type.FullName;
			if (declaringType != null)
			{
				tkResolution = GetTypeRefNested(declaringType, refedModule, strRefedModuleFileName);
				text = UnmangleTypeName(text);
			}
			return InternalGetTypeToken(text, refedModule, strRefedModuleFileName, tkResolution);
		}

		internal MethodToken InternalGetConstructorToken(ConstructorInfo con, bool usingRef)
		{
			int num = 0;
			if (con == null)
			{
				throw new ArgumentNullException("con");
			}
			if (con is ConstructorBuilder)
			{
				ConstructorBuilder constructorBuilder = con as ConstructorBuilder;
				if (!usingRef && constructorBuilder.ReflectedType.Module.InternalModule.Equals(InternalModule))
				{
					return constructorBuilder.GetToken();
				}
				int token = GetTypeTokenInternal(con.ReflectedType).Token;
				num = InternalGetMemberRef(con.ReflectedType.Module, token, constructorBuilder.GetToken().Token);
			}
			else if (con is ConstructorOnTypeBuilderInstantiation)
			{
				ConstructorOnTypeBuilderInstantiation constructorOnTypeBuilderInstantiation = con as ConstructorOnTypeBuilderInstantiation;
				if (usingRef)
				{
					throw new InvalidOperationException();
				}
				int token = GetTypeTokenInternal(con.DeclaringType).Token;
				num = InternalGetMemberRef(con.DeclaringType.Module, token, constructorOnTypeBuilderInstantiation.m_ctor.MetadataTokenInternal);
			}
			else if (con is RuntimeConstructorInfo && !con.ReflectedType.IsArray)
			{
				int token = GetTypeTokenInternal(con.ReflectedType).Token;
				num = InternalGetMemberRefOfMethodInfo(token, con.GetMethodHandle());
			}
			else
			{
				ParameterInfo[] parameters = con.GetParameters();
				Type[] array = new Type[parameters.Length];
				Type[][] array2 = new Type[array.Length][];
				Type[][] array3 = new Type[array.Length][];
				for (int i = 0; i < parameters.Length; i++)
				{
					array[i] = parameters[i].ParameterType;
					array2[i] = parameters[i].GetRequiredCustomModifiers();
					array3[i] = parameters[i].GetOptionalCustomModifiers();
				}
				int token = GetTypeTokenInternal(con.ReflectedType).Token;
				SignatureHelper methodSigHelper = SignatureHelper.GetMethodSigHelper(this, con.CallingConvention, null, null, null, array, array2, array3);
				int length;
				byte[] signature = methodSigHelper.InternalGetSignature(out length);
				num = InternalGetMemberRefFromSignature(token, con.Name, signature, length);
			}
			return new MethodToken(num);
		}

		internal void Init(string strModuleName, string strFileName, ISymbolWriter writer)
		{
			base.m_moduleData = new ModuleBuilderData(this, strModuleName, strFileName);
			base.m_TypeBuilderList = new ArrayList();
			base.m_iSymWriter = writer;
			if (writer != null)
			{
				new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Assert();
				writer.SetUnderlyingWriter(base.m_pInternalSymWriter);
			}
		}

		internal int GetMemberRefToken(MethodBase method, Type[] optionalParameterTypes)
		{
			int cGenericParameters = 0;
			if (method.IsGenericMethod)
			{
				if (!method.IsGenericMethodDefinition)
				{
					throw new InvalidOperationException();
				}
				cGenericParameters = method.GetGenericArguments().Length;
			}
			if (optionalParameterTypes != null && (method.CallingConvention & CallingConventions.VarArgs) == 0)
			{
				throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_NotAVarArgCallingConvention"));
			}
			MethodInfo methodInfo = method as MethodInfo;
			Type[] parameterTypes;
			Type returnType;
			if (method.DeclaringType.IsGenericType)
			{
				MethodBase methodBase = null;
				MethodOnTypeBuilderInstantiation methodOnTypeBuilderInstantiation;
				ConstructorOnTypeBuilderInstantiation constructorOnTypeBuilderInstantiation;
				if ((methodOnTypeBuilderInstantiation = method as MethodOnTypeBuilderInstantiation) != null)
				{
					methodBase = methodOnTypeBuilderInstantiation.m_method;
				}
				else if ((constructorOnTypeBuilderInstantiation = method as ConstructorOnTypeBuilderInstantiation) != null)
				{
					methodBase = constructorOnTypeBuilderInstantiation.m_ctor;
				}
				else if (method is MethodBuilder || method is ConstructorBuilder)
				{
					methodBase = method;
				}
				else if (method.IsGenericMethod)
				{
					methodBase = methodInfo.GetGenericMethodDefinition();
					methodBase = methodBase.Module.ResolveMethod(methodBase.MetadataTokenInternal, methodBase.GetGenericArguments(), (methodBase.DeclaringType != null) ? methodBase.DeclaringType.GetGenericArguments() : null);
				}
				else
				{
					methodBase = method;
					methodBase = method.Module.ResolveMethod(method.MetadataTokenInternal, null, (methodBase.DeclaringType != null) ? methodBase.DeclaringType.GetGenericArguments() : null);
				}
				parameterTypes = methodBase.GetParameterTypes();
				returnType = methodBase.GetReturnType();
			}
			else
			{
				parameterTypes = method.GetParameterTypes();
				returnType = method.GetReturnType();
			}
			int tr;
			if (!method.DeclaringType.IsGenericType)
			{
				tr = ((method.Module.InternalModule != InternalModule) ? GetTypeToken(method.DeclaringType).Token : ((methodInfo == null) ? GetConstructorToken(method as ConstructorInfo).Token : GetMethodToken(method as MethodInfo).Token));
			}
			else
			{
				int length;
				byte[] signature = SignatureHelper.GetTypeSigToken(this, method.DeclaringType).InternalGetSignature(out length);
				tr = InternalGetTypeSpecTokenWithBytes(signature, length);
			}
			int length2;
			byte[] signature2 = GetMemberRefSignature(method.CallingConvention, returnType, parameterTypes, optionalParameterTypes, cGenericParameters).InternalGetSignature(out length2);
			return InternalGetMemberRefFromSignature(tr, method.Name, signature2, length2);
		}

		internal SignatureHelper GetMemberRefSignature(CallingConventions call, Type returnType, Type[] parameterTypes, Type[] optionalParameterTypes, int cGenericParameters)
		{
			int num = ((parameterTypes != null) ? parameterTypes.Length : 0);
			SignatureHelper methodSigHelper = SignatureHelper.GetMethodSigHelper(this, call, returnType, cGenericParameters);
			for (int i = 0; i < num; i++)
			{
				methodSigHelper.AddArgument(parameterTypes[i]);
			}
			if (optionalParameterTypes != null && optionalParameterTypes.Length != 0)
			{
				methodSigHelper.AddSentinel();
				for (int i = 0; i < optionalParameterTypes.Length; i++)
				{
					methodSigHelper.AddArgument(optionalParameterTypes[i]);
				}
			}
			return methodSigHelper;
		}

		internal override bool IsDynamic()
		{
			return true;
		}

		internal override Assembly GetAssemblyInternal()
		{
			if (!IsInternal)
			{
				return m_assemblyBuilder;
			}
			return _GetAssemblyInternal();
		}

		public override Type[] GetTypes()
		{
			if (base.Assembly.m_assemblyData.m_isSynchronized)
			{
				lock (base.Assembly.m_assemblyData)
				{
					return GetTypesNoLock();
				}
			}
			return GetTypesNoLock();
		}

		internal Type[] GetTypesNoLock()
		{
			int count = base.m_TypeBuilderList.Count;
			List<Type> list = new List<Type>(count);
			bool flag = false;
			if (IsInternal)
			{
				try
				{
					DemandGrantedAssemblyPermission();
					flag = true;
				}
				catch (SecurityException)
				{
					flag = false;
				}
			}
			else
			{
				flag = true;
			}
			for (int i = 0; i < count; i++)
			{
				EnumBuilder enumBuilder = base.m_TypeBuilderList[i] as EnumBuilder;
				TypeBuilder typeBuilder = ((enumBuilder == null) ? (base.m_TypeBuilderList[i] as TypeBuilder) : enumBuilder.m_typeBuilder);
				if (typeBuilder != null)
				{
					if (typeBuilder.m_hasBeenCreated)
					{
						list.Add(typeBuilder.UnderlyingSystemType);
					}
					else if (flag)
					{
						list.Add(typeBuilder);
					}
				}
				else
				{
					list.Add((Type)base.m_TypeBuilderList[i]);
				}
			}
			return list.ToArray();
		}

		[ComVisible(true)]
		public override Type GetType(string className)
		{
			return GetType(className, throwOnError: false, ignoreCase: false);
		}

		[ComVisible(true)]
		public override Type GetType(string className, bool ignoreCase)
		{
			return GetType(className, throwOnError: false, ignoreCase);
		}

		[ComVisible(true)]
		public override Type GetType(string className, bool throwOnError, bool ignoreCase)
		{
			if (base.Assembly.m_assemblyData.m_isSynchronized)
			{
				lock (base.Assembly.m_assemblyData)
				{
					return GetTypeNoLock(className, throwOnError, ignoreCase);
				}
			}
			return GetTypeNoLock(className, throwOnError, ignoreCase);
		}

		private Type GetTypeNoLock(string className, bool throwOnError, bool ignoreCase)
		{
			Type type = base.GetType(className, throwOnError, ignoreCase);
			if (type != null)
			{
				return type;
			}
			string text = null;
			string text2 = null;
			int num = 0;
			while (num <= className.Length)
			{
				int num2 = className.IndexOfAny(new char[3]
				{
					'[',
					'*',
					'&'
				}, num);
				if (num2 == -1)
				{
					text = className;
					text2 = null;
					break;
				}
				int num3 = 0;
				int num4 = num2 - 1;
				while (num4 >= 0 && className[num4] == '\\')
				{
					num3++;
					num4--;
				}
				if (num3 % 2 == 1)
				{
					num = num2 + 1;
					continue;
				}
				text = className.Substring(0, num2);
				text2 = className.Substring(num2);
				break;
			}
			if (text == null)
			{
				text = className;
				text2 = null;
			}
			text = text.Replace("\\\\", "\\").Replace("\\[", "[").Replace("\\*", "*")
				.Replace("\\&", "&");
			if (text2 != null)
			{
				type = base.GetType(text, throwOnError: false, ignoreCase);
			}
			bool flag = false;
			if (IsInternal)
			{
				try
				{
					DemandGrantedAssemblyPermission();
					flag = true;
				}
				catch (SecurityException)
				{
					flag = false;
				}
			}
			else
			{
				flag = true;
			}
			if (type == null && flag)
			{
				type = FindTypeBuilderWithName(text, ignoreCase);
				if (type == null && base.Assembly is AssemblyBuilder)
				{
					ArrayList moduleBuilderList = base.Assembly.m_assemblyData.m_moduleBuilderList;
					int count = moduleBuilderList.Count;
					for (int i = 0; i < count; i++)
					{
						if (type != null)
						{
							break;
						}
						ModuleBuilder moduleBuilder = (ModuleBuilder)moduleBuilderList[i];
						type = moduleBuilder.FindTypeBuilderWithName(text, ignoreCase);
					}
				}
			}
			if (type == null)
			{
				return null;
			}
			if (text2 == null)
			{
				return type;
			}
			return GetType(text2, type);
		}

		public TypeBuilder DefineType(string name)
		{
			if (IsInternal)
			{
				DemandGrantedAssemblyPermission();
			}
			if (base.Assembly.m_assemblyData.m_isSynchronized)
			{
				lock (base.Assembly.m_assemblyData)
				{
					return DefineTypeNoLock(name);
				}
			}
			return DefineTypeNoLock(name);
		}

		private TypeBuilder DefineTypeNoLock(string name)
		{
			TypeBuilder typeBuilder = new TypeBuilder(name, TypeAttributes.NotPublic, null, null, this, PackingSize.Unspecified, null);
			base.m_TypeBuilderList.Add(typeBuilder);
			return typeBuilder;
		}

		public TypeBuilder DefineType(string name, TypeAttributes attr)
		{
			if (IsInternal)
			{
				DemandGrantedAssemblyPermission();
			}
			if (base.Assembly.m_assemblyData.m_isSynchronized)
			{
				lock (base.Assembly.m_assemblyData)
				{
					return DefineTypeNoLock(name, attr);
				}
			}
			return DefineTypeNoLock(name, attr);
		}

		private TypeBuilder DefineTypeNoLock(string name, TypeAttributes attr)
		{
			TypeBuilder typeBuilder = new TypeBuilder(name, attr, null, null, this, PackingSize.Unspecified, null);
			base.m_TypeBuilderList.Add(typeBuilder);
			return typeBuilder;
		}

		public TypeBuilder DefineType(string name, TypeAttributes attr, Type parent)
		{
			if (IsInternal)
			{
				DemandGrantedAssemblyPermission();
			}
			if (base.Assembly.m_assemblyData.m_isSynchronized)
			{
				lock (base.Assembly.m_assemblyData)
				{
					return DefineTypeNoLock(name, attr, parent);
				}
			}
			return DefineTypeNoLock(name, attr, parent);
		}

		private TypeBuilder DefineTypeNoLock(string name, TypeAttributes attr, Type parent)
		{
			CheckContext(parent);
			TypeBuilder typeBuilder = new TypeBuilder(name, attr, parent, null, this, PackingSize.Unspecified, null);
			base.m_TypeBuilderList.Add(typeBuilder);
			return typeBuilder;
		}

		public TypeBuilder DefineType(string name, TypeAttributes attr, Type parent, int typesize)
		{
			if (IsInternal)
			{
				DemandGrantedAssemblyPermission();
			}
			if (base.Assembly.m_assemblyData.m_isSynchronized)
			{
				lock (base.Assembly.m_assemblyData)
				{
					return DefineTypeNoLock(name, attr, parent, typesize);
				}
			}
			return DefineTypeNoLock(name, attr, parent, typesize);
		}

		private TypeBuilder DefineTypeNoLock(string name, TypeAttributes attr, Type parent, int typesize)
		{
			TypeBuilder typeBuilder = new TypeBuilder(name, attr, parent, this, PackingSize.Unspecified, typesize, null);
			base.m_TypeBuilderList.Add(typeBuilder);
			return typeBuilder;
		}

		public TypeBuilder DefineType(string name, TypeAttributes attr, Type parent, PackingSize packingSize, int typesize)
		{
			if (IsInternal)
			{
				DemandGrantedAssemblyPermission();
			}
			if (base.Assembly.m_assemblyData.m_isSynchronized)
			{
				lock (base.Assembly.m_assemblyData)
				{
					return DefineTypeNoLock(name, attr, parent, packingSize, typesize);
				}
			}
			return DefineTypeNoLock(name, attr, parent, packingSize, typesize);
		}

		private TypeBuilder DefineTypeNoLock(string name, TypeAttributes attr, Type parent, PackingSize packingSize, int typesize)
		{
			TypeBuilder typeBuilder = new TypeBuilder(name, attr, parent, this, packingSize, typesize, null);
			base.m_TypeBuilderList.Add(typeBuilder);
			return typeBuilder;
		}

		[ComVisible(true)]
		public TypeBuilder DefineType(string name, TypeAttributes attr, Type parent, Type[] interfaces)
		{
			if (IsInternal)
			{
				DemandGrantedAssemblyPermission();
			}
			if (base.Assembly.m_assemblyData.m_isSynchronized)
			{
				lock (base.Assembly.m_assemblyData)
				{
					return DefineTypeNoLock(name, attr, parent, interfaces);
				}
			}
			return DefineTypeNoLock(name, attr, parent, interfaces);
		}

		private TypeBuilder DefineTypeNoLock(string name, TypeAttributes attr, Type parent, Type[] interfaces)
		{
			TypeBuilder typeBuilder = new TypeBuilder(name, attr, parent, interfaces, this, PackingSize.Unspecified, null);
			base.m_TypeBuilderList.Add(typeBuilder);
			return typeBuilder;
		}

		public TypeBuilder DefineType(string name, TypeAttributes attr, Type parent, PackingSize packsize)
		{
			if (IsInternal)
			{
				DemandGrantedAssemblyPermission();
			}
			if (base.Assembly.m_assemblyData.m_isSynchronized)
			{
				lock (base.Assembly.m_assemblyData)
				{
					return DefineTypeNoLock(name, attr, parent, packsize);
				}
			}
			return DefineTypeNoLock(name, attr, parent, packsize);
		}

		private TypeBuilder DefineTypeNoLock(string name, TypeAttributes attr, Type parent, PackingSize packsize)
		{
			TypeBuilder typeBuilder = new TypeBuilder(name, attr, parent, null, this, packsize, null);
			base.m_TypeBuilderList.Add(typeBuilder);
			return typeBuilder;
		}

		public EnumBuilder DefineEnum(string name, TypeAttributes visibility, Type underlyingType)
		{
			if (IsInternal)
			{
				DemandGrantedAssemblyPermission();
			}
			CheckContext(underlyingType);
			if (base.Assembly.m_assemblyData.m_isSynchronized)
			{
				lock (base.Assembly.m_assemblyData)
				{
					return DefineEnumNoLock(name, visibility, underlyingType);
				}
			}
			return DefineEnumNoLock(name, visibility, underlyingType);
		}

		private EnumBuilder DefineEnumNoLock(string name, TypeAttributes visibility, Type underlyingType)
		{
			EnumBuilder enumBuilder = new EnumBuilder(name, underlyingType, visibility, this);
			base.m_TypeBuilderList.Add(enumBuilder);
			return enumBuilder;
		}

		public IResourceWriter DefineResource(string name, string description)
		{
			return DefineResource(name, description, ResourceAttributes.Public);
		}

		public IResourceWriter DefineResource(string name, string description, ResourceAttributes attribute)
		{
			if (IsInternal)
			{
				DemandGrantedAssemblyPermission();
			}
			if (base.Assembly.m_assemblyData.m_isSynchronized)
			{
				lock (base.Assembly.m_assemblyData)
				{
					return DefineResourceNoLock(name, description, attribute);
				}
			}
			return DefineResourceNoLock(name, description, attribute);
		}

		private IResourceWriter DefineResourceNoLock(string name, string description, ResourceAttributes attribute)
		{
			if (IsTransient())
			{
				throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_BadResourceContainer"));
			}
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			if (name.Length == 0)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_EmptyName"), "name");
			}
			Assembly assembly = base.Assembly;
			if (assembly is AssemblyBuilder)
			{
				AssemblyBuilder assemblyBuilder = (AssemblyBuilder)assembly;
				if (assemblyBuilder.IsPersistable())
				{
					assemblyBuilder.m_assemblyData.CheckResNameConflict(name);
					MemoryStream memoryStream = new MemoryStream();
					ResourceWriter resourceWriter = new ResourceWriter(memoryStream);
					ResWriterData resWriterData = new ResWriterData(resourceWriter, memoryStream, name, string.Empty, string.Empty, attribute);
					resWriterData.m_nextResWriter = base.m_moduleData.m_embeddedRes;
					base.m_moduleData.m_embeddedRes = resWriterData;
					return resourceWriter;
				}
				throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_BadResourceContainer"));
			}
			throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_BadResourceContainer"));
		}

		public void DefineManifestResource(string name, Stream stream, ResourceAttributes attribute)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			if (stream == null)
			{
				throw new ArgumentNullException("stream");
			}
			if (IsInternal)
			{
				DemandGrantedAssemblyPermission();
			}
			if (base.Assembly.m_assemblyData.m_isSynchronized)
			{
				lock (base.Assembly.m_assemblyData)
				{
					DefineManifestResourceNoLock(name, stream, attribute);
				}
			}
			else
			{
				DefineManifestResourceNoLock(name, stream, attribute);
			}
		}

		private void DefineManifestResourceNoLock(string name, Stream stream, ResourceAttributes attribute)
		{
			if (IsTransient())
			{
				throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_BadResourceContainer"));
			}
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			if (name.Length == 0)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_EmptyName"), "name");
			}
			Assembly assembly = base.Assembly;
			if (assembly is AssemblyBuilder)
			{
				AssemblyBuilder assemblyBuilder = (AssemblyBuilder)assembly;
				if (assemblyBuilder.IsPersistable())
				{
					assemblyBuilder.m_assemblyData.CheckResNameConflict(name);
					ResWriterData resWriterData = new ResWriterData(null, stream, name, string.Empty, string.Empty, attribute);
					resWriterData.m_nextResWriter = base.m_moduleData.m_embeddedRes;
					base.m_moduleData.m_embeddedRes = resWriterData;
					return;
				}
				throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_BadResourceContainer"));
			}
			throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_BadResourceContainer"));
		}

		public void DefineUnmanagedResource(byte[] resource)
		{
			if (IsInternal)
			{
				DemandGrantedAssemblyPermission();
			}
			if (base.Assembly.m_assemblyData.m_isSynchronized)
			{
				lock (base.Assembly.m_assemblyData)
				{
					DefineUnmanagedResourceInternalNoLock(resource);
				}
			}
			else
			{
				DefineUnmanagedResourceInternalNoLock(resource);
			}
		}

		internal void DefineUnmanagedResourceInternalNoLock(byte[] resource)
		{
			if (base.m_moduleData.m_strResourceFileName != null || base.m_moduleData.m_resourceBytes != null)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_NativeResourceAlreadyDefined"));
			}
			if (resource == null)
			{
				throw new ArgumentNullException("resource");
			}
			base.m_moduleData.m_resourceBytes = new byte[resource.Length];
			Array.Copy(resource, base.m_moduleData.m_resourceBytes, resource.Length);
		}

		public void DefineUnmanagedResource(string resourceFileName)
		{
			if (IsInternal)
			{
				DemandGrantedAssemblyPermission();
			}
			if (base.Assembly.m_assemblyData.m_isSynchronized)
			{
				lock (base.Assembly.m_assemblyData)
				{
					DefineUnmanagedResourceFileInternalNoLock(resourceFileName);
				}
			}
			else
			{
				DefineUnmanagedResourceFileInternalNoLock(resourceFileName);
			}
		}

		internal void DefineUnmanagedResourceFileInternalNoLock(string resourceFileName)
		{
			if (base.m_moduleData.m_resourceBytes != null || base.m_moduleData.m_strResourceFileName != null)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_NativeResourceAlreadyDefined"));
			}
			if (resourceFileName == null)
			{
				throw new ArgumentNullException("resourceFileName");
			}
			string fullPath = Path.GetFullPath(resourceFileName);
			new FileIOPermission(FileIOPermissionAccess.Read, fullPath).Demand();
			new EnvironmentPermission(PermissionState.Unrestricted).Assert();
			try
			{
				if (!File.Exists(resourceFileName))
				{
					throw new FileNotFoundException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("IO.FileNotFound_FileName"), resourceFileName), resourceFileName);
				}
			}
			finally
			{
				CodeAccessPermission.RevertAssert();
			}
			base.m_moduleData.m_strResourceFileName = fullPath;
		}

		public MethodBuilder DefineGlobalMethod(string name, MethodAttributes attributes, Type returnType, Type[] parameterTypes)
		{
			return DefineGlobalMethod(name, attributes, CallingConventions.Standard, returnType, parameterTypes);
		}

		public MethodBuilder DefineGlobalMethod(string name, MethodAttributes attributes, CallingConventions callingConvention, Type returnType, Type[] parameterTypes)
		{
			return DefineGlobalMethod(name, attributes, callingConvention, returnType, null, null, parameterTypes, null, null);
		}

		public MethodBuilder DefineGlobalMethod(string name, MethodAttributes attributes, CallingConventions callingConvention, Type returnType, Type[] requiredReturnTypeCustomModifiers, Type[] optionalReturnTypeCustomModifiers, Type[] parameterTypes, Type[][] requiredParameterTypeCustomModifiers, Type[][] optionalParameterTypeCustomModifiers)
		{
			if (IsInternal)
			{
				DemandGrantedAssemblyPermission();
			}
			if (base.Assembly.m_assemblyData.m_isSynchronized)
			{
				lock (base.Assembly.m_assemblyData)
				{
					return DefineGlobalMethodNoLock(name, attributes, callingConvention, returnType, requiredReturnTypeCustomModifiers, optionalReturnTypeCustomModifiers, parameterTypes, requiredParameterTypeCustomModifiers, optionalParameterTypeCustomModifiers);
				}
			}
			return DefineGlobalMethodNoLock(name, attributes, callingConvention, returnType, requiredReturnTypeCustomModifiers, optionalReturnTypeCustomModifiers, parameterTypes, requiredParameterTypeCustomModifiers, optionalParameterTypeCustomModifiers);
		}

		private MethodBuilder DefineGlobalMethodNoLock(string name, MethodAttributes attributes, CallingConventions callingConvention, Type returnType, Type[] requiredReturnTypeCustomModifiers, Type[] optionalReturnTypeCustomModifiers, Type[] parameterTypes, Type[][] requiredParameterTypeCustomModifiers, Type[][] optionalParameterTypeCustomModifiers)
		{
			CheckContext(returnType);
			CheckContext(requiredReturnTypeCustomModifiers, optionalReturnTypeCustomModifiers, parameterTypes);
			CheckContext(requiredParameterTypeCustomModifiers);
			CheckContext(optionalParameterTypeCustomModifiers);
			if (base.m_moduleData.m_fGlobalBeenCreated)
			{
				throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_GlobalsHaveBeenCreated"));
			}
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			if (name.Length == 0)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_EmptyName"), "name");
			}
			if ((attributes & MethodAttributes.Static) == 0)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_GlobalFunctionHasToBeStatic"));
			}
			base.m_moduleData.m_fHasGlobal = true;
			return base.m_moduleData.m_globalTypeBuilder.DefineMethod(name, attributes, callingConvention, returnType, requiredReturnTypeCustomModifiers, optionalReturnTypeCustomModifiers, parameterTypes, requiredParameterTypeCustomModifiers, optionalParameterTypeCustomModifiers);
		}

		public MethodBuilder DefinePInvokeMethod(string name, string dllName, MethodAttributes attributes, CallingConventions callingConvention, Type returnType, Type[] parameterTypes, CallingConvention nativeCallConv, CharSet nativeCharSet)
		{
			return DefinePInvokeMethod(name, dllName, name, attributes, callingConvention, returnType, parameterTypes, nativeCallConv, nativeCharSet);
		}

		public MethodBuilder DefinePInvokeMethod(string name, string dllName, string entryName, MethodAttributes attributes, CallingConventions callingConvention, Type returnType, Type[] parameterTypes, CallingConvention nativeCallConv, CharSet nativeCharSet)
		{
			if (IsInternal)
			{
				DemandGrantedAssemblyPermission();
			}
			if (base.Assembly.m_assemblyData.m_isSynchronized)
			{
				lock (base.Assembly.m_assemblyData)
				{
					return DefinePInvokeMethodNoLock(name, dllName, entryName, attributes, callingConvention, returnType, parameterTypes, nativeCallConv, nativeCharSet);
				}
			}
			return DefinePInvokeMethodNoLock(name, dllName, entryName, attributes, callingConvention, returnType, parameterTypes, nativeCallConv, nativeCharSet);
		}

		private MethodBuilder DefinePInvokeMethodNoLock(string name, string dllName, string entryName, MethodAttributes attributes, CallingConventions callingConvention, Type returnType, Type[] parameterTypes, CallingConvention nativeCallConv, CharSet nativeCharSet)
		{
			CheckContext(returnType);
			CheckContext(parameterTypes);
			if ((attributes & MethodAttributes.Static) == 0)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_GlobalFunctionHasToBeStatic"));
			}
			base.m_moduleData.m_fHasGlobal = true;
			return base.m_moduleData.m_globalTypeBuilder.DefinePInvokeMethod(name, dllName, entryName, attributes, callingConvention, returnType, parameterTypes, nativeCallConv, nativeCharSet);
		}

		public void CreateGlobalFunctions()
		{
			if (IsInternal)
			{
				DemandGrantedAssemblyPermission();
			}
			if (base.Assembly.m_assemblyData.m_isSynchronized)
			{
				lock (base.Assembly.m_assemblyData)
				{
					CreateGlobalFunctionsNoLock();
				}
			}
			else
			{
				CreateGlobalFunctionsNoLock();
			}
		}

		private void CreateGlobalFunctionsNoLock()
		{
			if (base.m_moduleData.m_fGlobalBeenCreated)
			{
				throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_NotADebugModule"));
			}
			base.m_moduleData.m_globalTypeBuilder.CreateType();
			base.m_moduleData.m_fGlobalBeenCreated = true;
		}

		public FieldBuilder DefineInitializedData(string name, byte[] data, FieldAttributes attributes)
		{
			if (IsInternal)
			{
				DemandGrantedAssemblyPermission();
			}
			if (base.Assembly.m_assemblyData.m_isSynchronized)
			{
				lock (base.Assembly.m_assemblyData)
				{
					return DefineInitializedDataNoLock(name, data, attributes);
				}
			}
			return DefineInitializedDataNoLock(name, data, attributes);
		}

		private FieldBuilder DefineInitializedDataNoLock(string name, byte[] data, FieldAttributes attributes)
		{
			if (base.m_moduleData.m_fGlobalBeenCreated)
			{
				throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_GlobalsHaveBeenCreated"));
			}
			base.m_moduleData.m_fHasGlobal = true;
			return base.m_moduleData.m_globalTypeBuilder.DefineInitializedData(name, data, attributes);
		}

		public FieldBuilder DefineUninitializedData(string name, int size, FieldAttributes attributes)
		{
			if (IsInternal)
			{
				DemandGrantedAssemblyPermission();
			}
			if (base.Assembly.m_assemblyData.m_isSynchronized)
			{
				lock (base.Assembly.m_assemblyData)
				{
					return DefineUninitializedDataNoLock(name, size, attributes);
				}
			}
			return DefineUninitializedDataNoLock(name, size, attributes);
		}

		private FieldBuilder DefineUninitializedDataNoLock(string name, int size, FieldAttributes attributes)
		{
			if (base.m_moduleData.m_fGlobalBeenCreated)
			{
				throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_GlobalsHaveBeenCreated"));
			}
			base.m_moduleData.m_fHasGlobal = true;
			return base.m_moduleData.m_globalTypeBuilder.DefineUninitializedData(name, size, attributes);
		}

		internal TypeToken GetTypeTokenInternal(Type type)
		{
			return GetTypeTokenInternal(type, getGenericDefinition: false);
		}

		internal TypeToken GetTypeTokenInternal(Type type, bool getGenericDefinition)
		{
			if (base.Assembly.m_assemblyData.m_isSynchronized)
			{
				lock (base.Assembly.m_assemblyData)
				{
					return GetTypeTokenWorkerNoLock(type, getGenericDefinition);
				}
			}
			return GetTypeTokenWorkerNoLock(type, getGenericDefinition);
		}

		public TypeToken GetTypeToken(Type type)
		{
			return GetTypeTokenInternal(type, getGenericDefinition: true);
		}

		private TypeToken GetTypeTokenWorkerNoLock(Type type, bool getGenericDefinition)
		{
			CheckContext(type);
			string empty = string.Empty;
			bool flag = false;
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}
			Module moduleBuilder = GetModuleBuilder(type.Module);
			bool flag2 = moduleBuilder.Equals(this);
			if (type.IsByRef)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_CannotGetTypeTokenForByRef"));
			}
			if ((type.IsGenericType && (!type.IsGenericTypeDefinition || !getGenericDefinition)) || type.IsGenericParameter || type.IsArray || type.IsPointer)
			{
				int length;
				byte[] signature = SignatureHelper.GetTypeSigToken(this, type).InternalGetSignature(out length);
				return new TypeToken(InternalGetTypeSpecTokenWithBytes(signature, length));
			}
			if (flag2)
			{
				EnumBuilder enumBuilder = type as EnumBuilder;
				TypeBuilder typeBuilder = ((enumBuilder == null) ? (type as TypeBuilder) : enumBuilder.m_typeBuilder);
				if (typeBuilder != null)
				{
					return typeBuilder.TypeToken;
				}
				if (type is GenericTypeParameterBuilder)
				{
					return new TypeToken(type.MetadataTokenInternal);
				}
				return new TypeToken(GetTypeRefNested(type, this, string.Empty));
			}
			ModuleBuilder moduleBuilder2 = moduleBuilder as ModuleBuilder;
			if (moduleBuilder2 != null)
			{
				if (moduleBuilder2.IsTransient())
				{
					flag = true;
				}
				empty = moduleBuilder2.m_moduleData.m_strFileName;
			}
			else
			{
				empty = moduleBuilder.ScopeName;
			}
			if (!IsTransient() && flag)
			{
				throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_BadTransientModuleReference"));
			}
			return new TypeToken(GetTypeRefNested(type, moduleBuilder, empty));
		}

		public TypeToken GetTypeToken(string name)
		{
			return GetTypeToken(base.GetType(name, throwOnError: false, ignoreCase: true));
		}

		public MethodToken GetMethodToken(MethodInfo method)
		{
			if (base.Assembly.m_assemblyData.m_isSynchronized)
			{
				lock (base.Assembly.m_assemblyData)
				{
					return GetMethodTokenNoLock(method, getGenericTypeDefinition: true);
				}
			}
			return GetMethodTokenNoLock(method, getGenericTypeDefinition: true);
		}

		internal MethodToken GetMethodTokenInternal(MethodInfo method)
		{
			if (base.Assembly.m_assemblyData.m_isSynchronized)
			{
				lock (base.Assembly.m_assemblyData)
				{
					return GetMethodTokenNoLock(method, getGenericTypeDefinition: false);
				}
			}
			return GetMethodTokenNoLock(method, getGenericTypeDefinition: false);
		}

		private MethodToken GetMethodTokenNoLock(MethodInfo method, bool getGenericTypeDefinition)
		{
			int num = 0;
			if (method == null)
			{
				throw new ArgumentNullException("method");
			}
			if (method is MethodBuilder)
			{
				if (method.Module.InternalModule == InternalModule)
				{
					return new MethodToken(method.MetadataTokenInternal);
				}
				if (method.DeclaringType == null)
				{
					throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_CannotImportGlobalFromDifferentModule"));
				}
				int tr = (getGenericTypeDefinition ? GetTypeToken(method.DeclaringType).Token : GetTypeTokenInternal(method.DeclaringType).Token);
				num = InternalGetMemberRef(method.DeclaringType.Module, tr, method.MetadataTokenInternal);
			}
			else
			{
				if (method is MethodOnTypeBuilderInstantiation)
				{
					return new MethodToken(GetMemberRefToken(method, null));
				}
				if (method is SymbolMethod)
				{
					SymbolMethod symbolMethod = method as SymbolMethod;
					if (symbolMethod.GetModule() == this)
					{
						return symbolMethod.GetToken();
					}
					return symbolMethod.GetToken(this);
				}
				Type declaringType = method.DeclaringType;
				if (declaringType == null)
				{
					throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_CannotImportGlobalFromDifferentModule"));
				}
				if (declaringType.IsArray)
				{
					ParameterInfo[] parameters = method.GetParameters();
					Type[] array = new Type[parameters.Length];
					for (int i = 0; i < parameters.Length; i++)
					{
						array[i] = parameters[i].ParameterType;
					}
					return GetArrayMethodToken(declaringType, method.Name, method.CallingConvention, method.ReturnType, array);
				}
				if (method is RuntimeMethodInfo)
				{
					int tr = (getGenericTypeDefinition ? GetTypeToken(method.DeclaringType).Token : GetTypeTokenInternal(method.DeclaringType).Token);
					num = InternalGetMemberRefOfMethodInfo(tr, method.GetMethodHandle());
				}
				else
				{
					ParameterInfo[] parameters2 = method.GetParameters();
					Type[] array2 = new Type[parameters2.Length];
					Type[][] array3 = new Type[array2.Length][];
					Type[][] array4 = new Type[array2.Length][];
					for (int j = 0; j < parameters2.Length; j++)
					{
						array2[j] = parameters2[j].ParameterType;
						array3[j] = parameters2[j].GetRequiredCustomModifiers();
						array4[j] = parameters2[j].GetOptionalCustomModifiers();
					}
					int tr = (getGenericTypeDefinition ? GetTypeToken(method.DeclaringType).Token : GetTypeTokenInternal(method.DeclaringType).Token);
					SignatureHelper methodSigHelper;
					try
					{
						methodSigHelper = SignatureHelper.GetMethodSigHelper(this, method.CallingConvention, method.ReturnType, method.ReturnParameter.GetRequiredCustomModifiers(), method.ReturnParameter.GetOptionalCustomModifiers(), array2, array3, array4);
					}
					catch (NotImplementedException)
					{
						methodSigHelper = SignatureHelper.GetMethodSigHelper(this, method.ReturnType, array2);
					}
					int length;
					byte[] signature = methodSigHelper.InternalGetSignature(out length);
					num = InternalGetMemberRefFromSignature(tr, method.Name, signature, length);
				}
			}
			return new MethodToken(num);
		}

		public MethodToken GetArrayMethodToken(Type arrayClass, string methodName, CallingConventions callingConvention, Type returnType, Type[] parameterTypes)
		{
			if (base.Assembly.m_assemblyData.m_isSynchronized)
			{
				lock (base.Assembly.m_assemblyData)
				{
					return GetArrayMethodTokenNoLock(arrayClass, methodName, callingConvention, returnType, parameterTypes);
				}
			}
			return GetArrayMethodTokenNoLock(arrayClass, methodName, callingConvention, returnType, parameterTypes);
		}

		private MethodToken GetArrayMethodTokenNoLock(Type arrayClass, string methodName, CallingConventions callingConvention, Type returnType, Type[] parameterTypes)
		{
			CheckContext(returnType, arrayClass);
			CheckContext(parameterTypes);
			if (arrayClass == null)
			{
				throw new ArgumentNullException("arrayClass");
			}
			if (methodName == null)
			{
				throw new ArgumentNullException("methodName");
			}
			if (methodName.Length == 0)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_EmptyName"), "methodName");
			}
			if (!arrayClass.IsArray)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_HasToBeArrayClass"));
			}
			SignatureHelper methodSigHelper = SignatureHelper.GetMethodSigHelper(this, callingConvention, returnType, null, null, parameterTypes, null, null);
			int length;
			byte[] signature = methodSigHelper.InternalGetSignature(out length);
			Type type = arrayClass;
			while (type.IsArray)
			{
				type = type.GetElementType();
			}
			int token = GetTypeTokenInternal(type).Token;
			return new MethodToken(nativeGetArrayMethodToken(GetTypeTokenInternal(arrayClass).Token, methodName, signature, length, token));
		}

		public MethodInfo GetArrayMethod(Type arrayClass, string methodName, CallingConventions callingConvention, Type returnType, Type[] parameterTypes)
		{
			CheckContext(returnType, arrayClass);
			CheckContext(parameterTypes);
			MethodToken arrayMethodToken = GetArrayMethodToken(arrayClass, methodName, callingConvention, returnType, parameterTypes);
			return new SymbolMethod(this, arrayMethodToken, arrayClass, methodName, callingConvention, returnType, parameterTypes);
		}

		[ComVisible(true)]
		public MethodToken GetConstructorToken(ConstructorInfo con)
		{
			return InternalGetConstructorToken(con, usingRef: false);
		}

		public FieldToken GetFieldToken(FieldInfo field)
		{
			if (base.Assembly.m_assemblyData.m_isSynchronized)
			{
				lock (base.Assembly.m_assemblyData)
				{
					return GetFieldTokenNoLock(field);
				}
			}
			return GetFieldTokenNoLock(field);
		}

		private FieldToken GetFieldTokenNoLock(FieldInfo field)
		{
			int num = 0;
			if (field == null)
			{
				throw new ArgumentNullException("con");
			}
			if (field is FieldBuilder)
			{
				FieldBuilder fieldBuilder = (FieldBuilder)field;
				if (field.DeclaringType != null && field.DeclaringType.IsGenericType)
				{
					int length;
					byte[] signature = SignatureHelper.GetTypeSigToken(this, field.DeclaringType).InternalGetSignature(out length);
					int tr = InternalGetTypeSpecTokenWithBytes(signature, length);
					num = InternalGetMemberRef(this, tr, fieldBuilder.GetToken().Token);
				}
				else
				{
					if (fieldBuilder.GetTypeBuilder().Module.InternalModule.Equals(InternalModule))
					{
						return fieldBuilder.GetToken();
					}
					if (field.DeclaringType == null)
					{
						throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_CannotImportGlobalFromDifferentModule"));
					}
					int tr = GetTypeTokenInternal(field.DeclaringType).Token;
					num = InternalGetMemberRef(field.ReflectedType.Module, tr, fieldBuilder.GetToken().Token);
				}
			}
			else if (field is RuntimeFieldInfo)
			{
				if (field.DeclaringType == null)
				{
					throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_CannotImportGlobalFromDifferentModule"));
				}
				if (field.DeclaringType != null && field.DeclaringType.IsGenericType)
				{
					int length2;
					byte[] signature2 = SignatureHelper.GetTypeSigToken(this, field.DeclaringType).InternalGetSignature(out length2);
					int tr = InternalGetTypeSpecTokenWithBytes(signature2, length2);
					num = InternalGetMemberRefOfFieldInfo(tr, field.DeclaringType.GetTypeHandleInternal(), field.MetadataTokenInternal);
				}
				else
				{
					int tr = GetTypeTokenInternal(field.DeclaringType).Token;
					num = InternalGetMemberRefOfFieldInfo(tr, field.DeclaringType.GetTypeHandleInternal(), field.MetadataTokenInternal);
				}
			}
			else if (field is FieldOnTypeBuilderInstantiation)
			{
				FieldInfo fieldInfo = ((FieldOnTypeBuilderInstantiation)field).FieldInfo;
				int length3;
				byte[] signature3 = SignatureHelper.GetTypeSigToken(this, field.DeclaringType).InternalGetSignature(out length3);
				int tr = InternalGetTypeSpecTokenWithBytes(signature3, length3);
				num = InternalGetMemberRef(fieldInfo.ReflectedType.Module, tr, fieldInfo.MetadataTokenInternal);
			}
			else
			{
				int tr = GetTypeTokenInternal(field.ReflectedType).Token;
				SignatureHelper fieldSigHelper = SignatureHelper.GetFieldSigHelper(this);
				fieldSigHelper.AddArgument(field.FieldType, field.GetRequiredCustomModifiers(), field.GetOptionalCustomModifiers());
				int length4;
				byte[] signature4 = fieldSigHelper.InternalGetSignature(out length4);
				num = InternalGetMemberRefFromSignature(tr, field.Name, signature4, length4);
			}
			return new FieldToken(num, field.GetType());
		}

		public StringToken GetStringConstant(string str)
		{
			return new StringToken(InternalGetStringConstant(str));
		}

		public SignatureToken GetSignatureToken(SignatureHelper sigHelper)
		{
			if (sigHelper == null)
			{
				throw new ArgumentNullException("sigHelper");
			}
			int length;
			byte[] signature = sigHelper.InternalGetSignature(out length);
			return new SignatureToken(TypeBuilder.InternalGetTokenFromSig(this, signature, length), this);
		}

		public SignatureToken GetSignatureToken(byte[] sigBytes, int sigLength)
		{
			byte[] array = null;
			if (sigBytes == null)
			{
				throw new ArgumentNullException("sigBytes");
			}
			array = new byte[sigBytes.Length];
			Array.Copy(sigBytes, array, sigBytes.Length);
			return new SignatureToken(TypeBuilder.InternalGetTokenFromSig(this, array, sigLength), this);
		}

		[ComVisible(true)]
		public void SetCustomAttribute(ConstructorInfo con, byte[] binaryAttribute)
		{
			if (IsInternal)
			{
				DemandGrantedAssemblyPermission();
			}
			if (con == null)
			{
				throw new ArgumentNullException("con");
			}
			if (binaryAttribute == null)
			{
				throw new ArgumentNullException("binaryAttribute");
			}
			TypeBuilder.InternalCreateCustomAttribute(1, GetConstructorToken(con).Token, binaryAttribute, this, toDisk: false);
		}

		public void SetCustomAttribute(CustomAttributeBuilder customBuilder)
		{
			if (IsInternal)
			{
				DemandGrantedAssemblyPermission();
			}
			if (customBuilder == null)
			{
				throw new ArgumentNullException("customBuilder");
			}
			customBuilder.CreateCustomAttribute(this, 1);
		}

		public ISymbolWriter GetSymWriter()
		{
			if (IsInternal)
			{
				DemandGrantedAssemblyPermission();
			}
			return base.m_iSymWriter;
		}

		public ISymbolDocumentWriter DefineDocument(string url, Guid language, Guid languageVendor, Guid documentType)
		{
			if (IsInternal)
			{
				DemandGrantedAssemblyPermission();
			}
			if (base.Assembly.m_assemblyData.m_isSynchronized)
			{
				lock (base.Assembly.m_assemblyData)
				{
					return DefineDocumentNoLock(url, language, languageVendor, documentType);
				}
			}
			return DefineDocumentNoLock(url, language, languageVendor, documentType);
		}

		private ISymbolDocumentWriter DefineDocumentNoLock(string url, Guid language, Guid languageVendor, Guid documentType)
		{
			if (base.m_iSymWriter == null)
			{
				throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_NotADebugModule"));
			}
			return base.m_iSymWriter.DefineDocument(url, language, languageVendor, documentType);
		}

		public void SetUserEntryPoint(MethodInfo entryPoint)
		{
			if (IsInternal)
			{
				DemandGrantedAssemblyPermission();
			}
			if (base.Assembly.m_assemblyData.m_isSynchronized)
			{
				lock (base.Assembly.m_assemblyData)
				{
					SetUserEntryPointNoLock(entryPoint);
				}
			}
			else
			{
				SetUserEntryPointNoLock(entryPoint);
			}
		}

		private void SetUserEntryPointNoLock(MethodInfo entryPoint)
		{
			if (entryPoint == null)
			{
				throw new ArgumentNullException("entryPoint");
			}
			if (base.m_iSymWriter == null)
			{
				throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_NotADebugModule"));
			}
			if (entryPoint.DeclaringType != null)
			{
				if (entryPoint.Module.InternalModule != InternalModule)
				{
					throw new InvalidOperationException(Environment.GetResourceString("Argument_NotInTheSameModuleBuilder"));
				}
			}
			else
			{
				MethodBuilder methodBuilder = entryPoint as MethodBuilder;
				if (methodBuilder != null && methodBuilder.GetModule().InternalModule != InternalModule)
				{
					throw new InvalidOperationException(Environment.GetResourceString("Argument_NotInTheSameModuleBuilder"));
				}
			}
			SymbolToken userEntryPoint = new SymbolToken(GetMethodTokenInternal(entryPoint).Token);
			base.m_iSymWriter.SetUserEntryPoint(userEntryPoint);
		}

		public void SetSymCustomAttribute(string name, byte[] data)
		{
			if (IsInternal)
			{
				DemandGrantedAssemblyPermission();
			}
			if (base.Assembly.m_assemblyData.m_isSynchronized)
			{
				lock (base.Assembly.m_assemblyData)
				{
					SetSymCustomAttributeNoLock(name, data);
				}
			}
			else
			{
				SetSymCustomAttributeNoLock(name, data);
			}
		}

		private void SetSymCustomAttributeNoLock(string name, byte[] data)
		{
			if (base.m_iSymWriter == null)
			{
				throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_NotADebugModule"));
			}
		}

		public bool IsTransient()
		{
			return base.m_moduleData.IsTransient();
		}

		void _ModuleBuilder.GetTypeInfoCount(out uint pcTInfo)
		{
			throw new NotImplementedException();
		}

		void _ModuleBuilder.GetTypeInfo(uint iTInfo, uint lcid, IntPtr ppTInfo)
		{
			throw new NotImplementedException();
		}

		void _ModuleBuilder.GetIDsOfNames([In] ref Guid riid, IntPtr rgszNames, uint cNames, uint lcid, IntPtr rgDispId)
		{
			throw new NotImplementedException();
		}

		void _ModuleBuilder.Invoke(uint dispIdMember, [In] ref Guid riid, uint lcid, short wFlags, IntPtr pDispParams, IntPtr pVarResult, IntPtr pExcepInfo, IntPtr puArgErr)
		{
			throw new NotImplementedException();
		}
	}
}
