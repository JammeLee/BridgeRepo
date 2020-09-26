using System.Diagnostics;
using System.Diagnostics.SymbolStore;
using System.Globalization;
using System.IO;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Threading;

namespace System.Reflection.Emit
{
	[ComVisible(true)]
	[ClassInterface(ClassInterfaceType.None)]
	[ComDefaultInterface(typeof(_AssemblyBuilder))]
	[HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort = true)]
	public sealed class AssemblyBuilder : Assembly, _AssemblyBuilder
	{
		private AssemblyBuilder m_internalAssemblyBuilder;

		private PermissionSet m_grantedPermissionSet;

		private PermissionSet GrantedPermissionSet
		{
			get
			{
				AssemblyBuilder assemblyBuilder = (AssemblyBuilder)InternalAssembly;
				if (assemblyBuilder.m_grantedPermissionSet == null)
				{
					PermissionSet newDenied = null;
					InternalAssembly.nGetGrantSet(out assemblyBuilder.m_grantedPermissionSet, out newDenied);
					if (assemblyBuilder.m_grantedPermissionSet == null)
					{
						assemblyBuilder.m_grantedPermissionSet = new PermissionSet(PermissionState.Unrestricted);
					}
				}
				return assemblyBuilder.m_grantedPermissionSet;
			}
		}

		private bool IsInternal => m_internalAssemblyBuilder == null;

		internal override Assembly InternalAssembly
		{
			get
			{
				if (IsInternal)
				{
					return this;
				}
				return m_internalAssemblyBuilder;
			}
		}

		public override string Location
		{
			get
			{
				throw new NotSupportedException(Environment.GetResourceString("NotSupported_DynamicAssembly"));
			}
		}

		public override string ImageRuntimeVersion => RuntimeEnvironment.GetSystemVersion();

		public override string CodeBase
		{
			get
			{
				throw new NotSupportedException(Environment.GetResourceString("NotSupported_DynamicAssembly"));
			}
		}

		public override MethodInfo EntryPoint
		{
			get
			{
				if (IsInternal)
				{
					DemandGrantedPermission();
				}
				return base.m_assemblyData.m_entryPointMethod;
			}
		}

		internal void DemandGrantedPermission()
		{
			GrantedPermissionSet.Demand();
		}

		internal override Module[] nGetModules(bool loadIfNotFound, bool getResourceModules)
		{
			Module[] array = InternalAssembly._nGetModules(loadIfNotFound, getResourceModules);
			if (!IsInternal)
			{
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = ModuleBuilder.GetModuleBuilder(array[i]);
				}
			}
			return array;
		}

		internal override Module GetModuleInternal(string name)
		{
			Module module = InternalAssembly._GetModule(name);
			if (module == null)
			{
				return null;
			}
			if (!IsInternal)
			{
				return ModuleBuilder.GetModuleBuilder(module);
			}
			return module;
		}

		internal AssemblyBuilder(AssemblyBuilder internalAssemblyBuilder)
		{
			m_internalAssemblyBuilder = internalAssemblyBuilder;
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public ModuleBuilder DefineDynamicModule(string name)
		{
			if (IsInternal)
			{
				DemandGrantedPermission();
			}
			StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
			return DefineDynamicModuleInternal(name, emitSymbolInfo: false, ref stackMark);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public ModuleBuilder DefineDynamicModule(string name, bool emitSymbolInfo)
		{
			if (IsInternal)
			{
				DemandGrantedPermission();
			}
			StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
			return DefineDynamicModuleInternal(name, emitSymbolInfo, ref stackMark);
		}

		private ModuleBuilder DefineDynamicModuleInternal(string name, bool emitSymbolInfo, ref StackCrawlMark stackMark)
		{
			if (base.m_assemblyData.m_isSynchronized)
			{
				lock (base.m_assemblyData)
				{
					return DefineDynamicModuleInternalNoLock(name, emitSymbolInfo, ref stackMark);
				}
			}
			return DefineDynamicModuleInternalNoLock(name, emitSymbolInfo, ref stackMark);
		}

		private ModuleBuilder DefineDynamicModuleInternalNoLock(string name, bool emitSymbolInfo, ref StackCrawlMark stackMark)
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
				throw new ArgumentException(Environment.GetResourceString("Argument_InvalidName"), "name");
			}
			base.m_assemblyData.CheckNameConflict(name);
			ModuleBuilder internalModuleBuilder = (ModuleBuilder)Assembly.nDefineDynamicModule(this, emitSymbolInfo, name, ref stackMark);
			internalModuleBuilder = new ModuleBuilder(this, internalModuleBuilder);
			ISymbolWriter writer = null;
			if (emitSymbolInfo)
			{
				Assembly assembly = LoadISymWrapper();
				Type type = assembly.GetType("System.Diagnostics.SymbolStore.SymWriter", throwOnError: true, ignoreCase: false);
				if (type != null && !type.IsVisible)
				{
					type = null;
				}
				if (type == null)
				{
					throw new ExecutionEngineException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("MissingType"), "SymWriter"));
				}
				new ReflectionPermission(ReflectionPermissionFlag.ReflectionEmit).Demand();
				try
				{
					new PermissionSet(PermissionState.Unrestricted).Assert();
					writer = (ISymbolWriter)Activator.CreateInstance(type);
				}
				finally
				{
					CodeAccessPermission.RevertAssert();
				}
			}
			internalModuleBuilder.Init(name, null, writer);
			base.m_assemblyData.AddModule(internalModuleBuilder);
			return internalModuleBuilder;
		}

		private Assembly LoadISymWrapper()
		{
			if (base.m_assemblyData.m_ISymWrapperAssembly != null)
			{
				return base.m_assemblyData.m_ISymWrapperAssembly;
			}
			Assembly assembly = Assembly.Load("ISymWrapper, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
			base.m_assemblyData.m_ISymWrapperAssembly = assembly;
			return assembly;
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public ModuleBuilder DefineDynamicModule(string name, string fileName)
		{
			if (IsInternal)
			{
				DemandGrantedPermission();
			}
			StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
			return DefineDynamicModuleInternal(name, fileName, emitSymbolInfo: false, ref stackMark);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public ModuleBuilder DefineDynamicModule(string name, string fileName, bool emitSymbolInfo)
		{
			if (IsInternal)
			{
				DemandGrantedPermission();
			}
			StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
			return DefineDynamicModuleInternal(name, fileName, emitSymbolInfo, ref stackMark);
		}

		private ModuleBuilder DefineDynamicModuleInternal(string name, string fileName, bool emitSymbolInfo, ref StackCrawlMark stackMark)
		{
			if (base.m_assemblyData.m_isSynchronized)
			{
				lock (base.m_assemblyData)
				{
					return DefineDynamicModuleInternalNoLock(name, fileName, emitSymbolInfo, ref stackMark);
				}
			}
			return DefineDynamicModuleInternalNoLock(name, fileName, emitSymbolInfo, ref stackMark);
		}

		internal void CheckContext(params Type[][] typess)
		{
			if (typess == null)
			{
				return;
			}
			foreach (Type[] array in typess)
			{
				if (array != null)
				{
					CheckContext(array);
				}
			}
		}

		internal void CheckContext(params Type[] types)
		{
			if (types == null)
			{
				return;
			}
			foreach (Type type in types)
			{
				if (type == null || type.Module.Assembly == typeof(object).Module.Assembly)
				{
					break;
				}
				if (type.Module.Assembly.ReflectionOnly && !ReflectionOnly)
				{
					throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Arugment_EmitMixedContext1"), type.AssemblyQualifiedName));
				}
				if (!type.Module.Assembly.ReflectionOnly && ReflectionOnly)
				{
					throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Arugment_EmitMixedContext2"), type.AssemblyQualifiedName));
				}
			}
		}

		private ModuleBuilder DefineDynamicModuleInternalNoLock(string name, string fileName, bool emitSymbolInfo, ref StackCrawlMark stackMark)
		{
			if (base.m_assemblyData.m_access == AssemblyBuilderAccess.Run)
			{
				throw new NotSupportedException(Environment.GetResourceString("Argument_BadPersistableModuleInTransientAssembly"));
			}
			if (base.m_assemblyData.m_isSaved)
			{
				throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_CannotAlterAssembly"));
			}
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
				throw new ArgumentException(Environment.GetResourceString("Argument_InvalidName"), "name");
			}
			if (fileName == null)
			{
				throw new ArgumentNullException("fileName");
			}
			if (fileName.Length == 0)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_EmptyFileName"), "fileName");
			}
			if (!string.Equals(fileName, Path.GetFileName(fileName)))
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_NotSimpleFileName"), "fileName");
			}
			base.m_assemblyData.CheckNameConflict(name);
			base.m_assemblyData.CheckFileNameConflict(fileName);
			ModuleBuilder internalModuleBuilder = (ModuleBuilder)Assembly.nDefineDynamicModule(this, emitSymbolInfo, fileName, ref stackMark);
			internalModuleBuilder = new ModuleBuilder(this, internalModuleBuilder);
			ISymbolWriter writer = null;
			if (emitSymbolInfo)
			{
				Assembly assembly = LoadISymWrapper();
				Type type = assembly.GetType("System.Diagnostics.SymbolStore.SymWriter", throwOnError: true, ignoreCase: false);
				if (type != null && !type.IsVisible)
				{
					type = null;
				}
				if (type == null)
				{
					throw new ExecutionEngineException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("MissingType"), "SymWriter"));
				}
				new ReflectionPermission(ReflectionPermissionFlag.ReflectionEmit).Demand();
				try
				{
					new PermissionSet(PermissionState.Unrestricted).Assert();
					writer = (ISymbolWriter)Activator.CreateInstance(type);
				}
				finally
				{
					CodeAccessPermission.RevertAssert();
				}
			}
			internalModuleBuilder.Init(name, fileName, writer);
			base.m_assemblyData.AddModule(internalModuleBuilder);
			return internalModuleBuilder;
		}

		public IResourceWriter DefineResource(string name, string description, string fileName)
		{
			return DefineResource(name, description, fileName, ResourceAttributes.Public);
		}

		public IResourceWriter DefineResource(string name, string description, string fileName, ResourceAttributes attribute)
		{
			if (IsInternal)
			{
				DemandGrantedPermission();
			}
			if (base.m_assemblyData.m_isSynchronized)
			{
				lock (base.m_assemblyData)
				{
					return DefineResourceNoLock(name, description, fileName, attribute);
				}
			}
			return DefineResourceNoLock(name, description, fileName, attribute);
		}

		private IResourceWriter DefineResourceNoLock(string name, string description, string fileName, ResourceAttributes attribute)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			if (name.Length == 0)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_EmptyName"), name);
			}
			if (fileName == null)
			{
				throw new ArgumentNullException("fileName");
			}
			if (fileName.Length == 0)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_EmptyFileName"), "fileName");
			}
			if (!string.Equals(fileName, Path.GetFileName(fileName)))
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_NotSimpleFileName"), "fileName");
			}
			base.m_assemblyData.CheckResNameConflict(name);
			base.m_assemblyData.CheckFileNameConflict(fileName);
			ResourceWriter resourceWriter;
			string text;
			if (base.m_assemblyData.m_strDir == null)
			{
				text = Path.Combine(Environment.CurrentDirectory, fileName);
				resourceWriter = new ResourceWriter(text);
			}
			else
			{
				text = Path.Combine(base.m_assemblyData.m_strDir, fileName);
				resourceWriter = new ResourceWriter(text);
			}
			text = Path.GetFullPath(text);
			fileName = Path.GetFileName(text);
			base.m_assemblyData.AddResWriter(new ResWriterData(resourceWriter, null, name, fileName, text, attribute));
			return resourceWriter;
		}

		public void AddResourceFile(string name, string fileName)
		{
			AddResourceFile(name, fileName, ResourceAttributes.Public);
		}

		public void AddResourceFile(string name, string fileName, ResourceAttributes attribute)
		{
			if (IsInternal)
			{
				DemandGrantedPermission();
			}
			if (base.m_assemblyData.m_isSynchronized)
			{
				lock (base.m_assemblyData)
				{
					AddResourceFileNoLock(name, fileName, attribute);
				}
			}
			else
			{
				AddResourceFileNoLock(name, fileName, attribute);
			}
		}

		private void AddResourceFileNoLock(string name, string fileName, ResourceAttributes attribute)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			if (name.Length == 0)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_EmptyName"), name);
			}
			if (fileName == null)
			{
				throw new ArgumentNullException("fileName");
			}
			if (fileName.Length == 0)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_EmptyFileName"), fileName);
			}
			if (!string.Equals(fileName, Path.GetFileName(fileName)))
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_NotSimpleFileName"), "fileName");
			}
			base.m_assemblyData.CheckResNameConflict(name);
			base.m_assemblyData.CheckFileNameConflict(fileName);
			string path = ((base.m_assemblyData.m_strDir != null) ? Path.Combine(base.m_assemblyData.m_strDir, fileName) : Path.Combine(Environment.CurrentDirectory, fileName));
			path = Path.GetFullPath(path);
			fileName = Path.GetFileName(path);
			if (!File.Exists(path))
			{
				throw new FileNotFoundException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("IO.FileNotFound_FileName"), fileName), fileName);
			}
			base.m_assemblyData.AddResWriter(new ResWriterData(null, null, name, fileName, path, attribute));
		}

		public override string[] GetManifestResourceNames()
		{
			throw new NotSupportedException(Environment.GetResourceString("NotSupported_DynamicAssembly"));
		}

		public override FileStream GetFile(string name)
		{
			throw new NotSupportedException(Environment.GetResourceString("NotSupported_DynamicAssembly"));
		}

		public override FileStream[] GetFiles(bool getResourceModules)
		{
			throw new NotSupportedException(Environment.GetResourceString("NotSupported_DynamicAssembly"));
		}

		public override Stream GetManifestResourceStream(Type type, string name)
		{
			throw new NotSupportedException(Environment.GetResourceString("NotSupported_DynamicAssembly"));
		}

		public override Stream GetManifestResourceStream(string name)
		{
			throw new NotSupportedException(Environment.GetResourceString("NotSupported_DynamicAssembly"));
		}

		public override ManifestResourceInfo GetManifestResourceInfo(string resourceName)
		{
			throw new NotSupportedException(Environment.GetResourceString("NotSupported_DynamicAssembly"));
		}

		public override Type[] GetExportedTypes()
		{
			throw new NotSupportedException(Environment.GetResourceString("NotSupported_DynamicAssembly"));
		}

		public void DefineVersionInfoResource(string product, string productVersion, string company, string copyright, string trademark)
		{
			if (IsInternal)
			{
				DemandGrantedPermission();
			}
			if (base.m_assemblyData.m_isSynchronized)
			{
				lock (base.m_assemblyData)
				{
					DefineVersionInfoResourceNoLock(product, productVersion, company, copyright, trademark);
				}
			}
			else
			{
				DefineVersionInfoResourceNoLock(product, productVersion, company, copyright, trademark);
			}
		}

		private void DefineVersionInfoResourceNoLock(string product, string productVersion, string company, string copyright, string trademark)
		{
			if (base.m_assemblyData.m_strResourceFileName != null || base.m_assemblyData.m_resourceBytes != null || base.m_assemblyData.m_nativeVersion != null)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_NativeResourceAlreadyDefined"));
			}
			base.m_assemblyData.m_nativeVersion = new NativeVersionInfo();
			base.m_assemblyData.m_nativeVersion.m_strCopyright = copyright;
			base.m_assemblyData.m_nativeVersion.m_strTrademark = trademark;
			base.m_assemblyData.m_nativeVersion.m_strCompany = company;
			base.m_assemblyData.m_nativeVersion.m_strProduct = product;
			base.m_assemblyData.m_nativeVersion.m_strProductVersion = productVersion;
			base.m_assemblyData.m_hasUnmanagedVersionInfo = true;
			base.m_assemblyData.m_OverrideUnmanagedVersionInfo = true;
		}

		public void DefineVersionInfoResource()
		{
			if (IsInternal)
			{
				DemandGrantedPermission();
			}
			if (base.m_assemblyData.m_isSynchronized)
			{
				lock (base.m_assemblyData)
				{
					DefineVersionInfoResourceNoLock();
				}
			}
			else
			{
				DefineVersionInfoResourceNoLock();
			}
		}

		private void DefineVersionInfoResourceNoLock()
		{
			if (base.m_assemblyData.m_strResourceFileName != null || base.m_assemblyData.m_resourceBytes != null || base.m_assemblyData.m_nativeVersion != null)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_NativeResourceAlreadyDefined"));
			}
			base.m_assemblyData.m_hasUnmanagedVersionInfo = true;
			base.m_assemblyData.m_nativeVersion = new NativeVersionInfo();
		}

		public void DefineUnmanagedResource(byte[] resource)
		{
			if (IsInternal)
			{
				DemandGrantedPermission();
			}
			if (resource == null)
			{
				throw new ArgumentNullException("resource");
			}
			if (base.m_assemblyData.m_isSynchronized)
			{
				lock (base.m_assemblyData)
				{
					DefineUnmanagedResourceNoLock(resource);
				}
			}
			else
			{
				DefineUnmanagedResourceNoLock(resource);
			}
		}

		private void DefineUnmanagedResourceNoLock(byte[] resource)
		{
			if (base.m_assemblyData.m_strResourceFileName != null || base.m_assemblyData.m_resourceBytes != null || base.m_assemblyData.m_nativeVersion != null)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_NativeResourceAlreadyDefined"));
			}
			base.m_assemblyData.m_resourceBytes = new byte[resource.Length];
			Array.Copy(resource, base.m_assemblyData.m_resourceBytes, resource.Length);
		}

		public void DefineUnmanagedResource(string resourceFileName)
		{
			if (IsInternal)
			{
				DemandGrantedPermission();
			}
			if (resourceFileName == null)
			{
				throw new ArgumentNullException("resourceFileName");
			}
			if (base.m_assemblyData.m_isSynchronized)
			{
				lock (base.m_assemblyData)
				{
					DefineUnmanagedResourceNoLock(resourceFileName);
				}
			}
			else
			{
				DefineUnmanagedResourceNoLock(resourceFileName);
			}
		}

		private void DefineUnmanagedResourceNoLock(string resourceFileName)
		{
			if (base.m_assemblyData.m_strResourceFileName != null || base.m_assemblyData.m_resourceBytes != null || base.m_assemblyData.m_nativeVersion != null)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_NativeResourceAlreadyDefined"));
			}
			string text;
			if (base.m_assemblyData.m_strDir == null)
			{
				text = Path.Combine(Environment.CurrentDirectory, resourceFileName);
			}
			else
			{
				text = Path.Combine(base.m_assemblyData.m_strDir, resourceFileName);
			}
			text = Path.GetFullPath(resourceFileName);
			new FileIOPermission(FileIOPermissionAccess.Read, text).Demand();
			if (!File.Exists(text))
			{
				throw new FileNotFoundException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("IO.FileNotFound_FileName"), resourceFileName), resourceFileName);
			}
			base.m_assemblyData.m_strResourceFileName = text;
		}

		public ModuleBuilder GetDynamicModule(string name)
		{
			if (IsInternal)
			{
				DemandGrantedPermission();
			}
			if (base.m_assemblyData.m_isSynchronized)
			{
				lock (base.m_assemblyData)
				{
					return GetDynamicModuleNoLock(name);
				}
			}
			return GetDynamicModuleNoLock(name);
		}

		private ModuleBuilder GetDynamicModuleNoLock(string name)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			if (name.Length == 0)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_EmptyName"), "name");
			}
			int count = base.m_assemblyData.m_moduleBuilderList.Count;
			for (int i = 0; i < count; i++)
			{
				ModuleBuilder moduleBuilder = (ModuleBuilder)base.m_assemblyData.m_moduleBuilderList[i];
				if (moduleBuilder.m_moduleData.m_strModuleName.Equals(name))
				{
					return moduleBuilder;
				}
			}
			return null;
		}

		public void SetEntryPoint(MethodInfo entryMethod)
		{
			SetEntryPoint(entryMethod, PEFileKinds.ConsoleApplication);
		}

		public void SetEntryPoint(MethodInfo entryMethod, PEFileKinds fileKind)
		{
			if (IsInternal)
			{
				DemandGrantedPermission();
			}
			if (base.m_assemblyData.m_isSynchronized)
			{
				lock (base.m_assemblyData)
				{
					SetEntryPointNoLock(entryMethod, fileKind);
				}
			}
			else
			{
				SetEntryPointNoLock(entryMethod, fileKind);
			}
		}

		private void SetEntryPointNoLock(MethodInfo entryMethod, PEFileKinds fileKind)
		{
			if (entryMethod == null)
			{
				throw new ArgumentNullException("entryMethod");
			}
			Module internalModule = entryMethod.Module.InternalModule;
			if (!(internalModule is ModuleBuilder) || !InternalAssembly.Equals(internalModule.Assembly.InternalAssembly))
			{
				throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_EntryMethodNotDefinedInAssembly"));
			}
			base.m_assemblyData.m_entryPointModule = (ModuleBuilder)ModuleBuilder.GetModuleBuilder(internalModule);
			base.m_assemblyData.m_entryPointMethod = entryMethod;
			base.m_assemblyData.m_peFileKind = fileKind;
			base.m_assemblyData.m_entryPointModule.SetEntryPoint(entryMethod);
		}

		[ComVisible(true)]
		public void SetCustomAttribute(ConstructorInfo con, byte[] binaryAttribute)
		{
			if (IsInternal)
			{
				DemandGrantedPermission();
			}
			if (con == null)
			{
				throw new ArgumentNullException("con");
			}
			if (binaryAttribute == null)
			{
				throw new ArgumentNullException("binaryAttribute");
			}
			if (base.m_assemblyData.m_isSynchronized)
			{
				lock (base.m_assemblyData)
				{
					SetCustomAttributeNoLock(con, binaryAttribute);
				}
			}
			else
			{
				SetCustomAttributeNoLock(con, binaryAttribute);
			}
		}

		private void SetCustomAttributeNoLock(ConstructorInfo con, byte[] binaryAttribute)
		{
			ModuleBuilder inMemoryAssemblyModule = base.m_assemblyData.GetInMemoryAssemblyModule();
			TypeBuilder.InternalCreateCustomAttribute(536870913, inMemoryAssemblyModule.GetConstructorToken(con).Token, binaryAttribute, inMemoryAssemblyModule, toDisk: false, typeof(DebuggableAttribute) == con.DeclaringType);
			if (base.m_assemblyData.m_access != AssemblyBuilderAccess.Run)
			{
				base.m_assemblyData.AddCustomAttribute(con, binaryAttribute);
			}
		}

		public void SetCustomAttribute(CustomAttributeBuilder customBuilder)
		{
			if (IsInternal)
			{
				DemandGrantedPermission();
			}
			if (customBuilder == null)
			{
				throw new ArgumentNullException("customBuilder");
			}
			if (base.m_assemblyData.m_isSynchronized)
			{
				lock (base.m_assemblyData)
				{
					SetCustomAttributeNoLock(customBuilder);
				}
			}
			else
			{
				SetCustomAttributeNoLock(customBuilder);
			}
		}

		private void SetCustomAttributeNoLock(CustomAttributeBuilder customBuilder)
		{
			ModuleBuilder inMemoryAssemblyModule = base.m_assemblyData.GetInMemoryAssemblyModule();
			customBuilder.CreateCustomAttribute(inMemoryAssemblyModule, 536870913);
			if (base.m_assemblyData.m_access != AssemblyBuilderAccess.Run)
			{
				base.m_assemblyData.AddCustomAttribute(customBuilder);
			}
		}

		public void Save(string assemblyFileName)
		{
			Save(assemblyFileName, PortableExecutableKinds.ILOnly, ImageFileMachine.I386);
		}

		public void Save(string assemblyFileName, PortableExecutableKinds portableExecutableKind, ImageFileMachine imageFileMachine)
		{
			if (IsInternal)
			{
				DemandGrantedPermission();
			}
			if (base.m_assemblyData.m_isSynchronized)
			{
				lock (base.m_assemblyData)
				{
					SaveNoLock(assemblyFileName, portableExecutableKind, imageFileMachine);
				}
			}
			else
			{
				SaveNoLock(assemblyFileName, portableExecutableKind, imageFileMachine);
			}
		}

		private void SaveNoLock(string assemblyFileName, PortableExecutableKinds portableExecutableKind, ImageFileMachine imageFileMachine)
		{
			int[] array = null;
			int[] array2 = null;
			string text = null;
			try
			{
				if (base.m_assemblyData.m_iCABuilder != 0)
				{
					array = new int[base.m_assemblyData.m_iCABuilder];
				}
				if (base.m_assemblyData.m_iCAs != 0)
				{
					array2 = new int[base.m_assemblyData.m_iCAs];
				}
				if (base.m_assemblyData.m_isSaved)
				{
					throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, Environment.GetResourceString("InvalidOperation_AssemblyHasBeenSaved"), nGetSimpleName()));
				}
				if ((base.m_assemblyData.m_access & AssemblyBuilderAccess.Save) != AssemblyBuilderAccess.Save)
				{
					throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_CantSaveTransientAssembly"));
				}
				if (assemblyFileName == null)
				{
					throw new ArgumentNullException("assemblyFileName");
				}
				if (assemblyFileName.Length == 0)
				{
					throw new ArgumentException(Environment.GetResourceString("Argument_EmptyFileName"), "assemblyFileName");
				}
				if (!string.Equals(assemblyFileName, Path.GetFileName(assemblyFileName)))
				{
					throw new ArgumentException(Environment.GetResourceString("Argument_NotSimpleFileName"), "assemblyFileName");
				}
				ModuleBuilder moduleBuilder = base.m_assemblyData.FindModuleWithFileName(assemblyFileName);
				if (moduleBuilder != null)
				{
					base.m_assemblyData.SetOnDiskAssemblyModule(moduleBuilder);
				}
				if (moduleBuilder == null)
				{
					base.m_assemblyData.CheckFileNameConflict(assemblyFileName);
				}
				if (base.m_assemblyData.m_strDir == null)
				{
					base.m_assemblyData.m_strDir = Environment.CurrentDirectory;
				}
				else if (!Directory.Exists(base.m_assemblyData.m_strDir))
				{
					throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, Environment.GetResourceString("Argument_InvalidDirectory"), base.m_assemblyData.m_strDir));
				}
				assemblyFileName = Path.Combine(base.m_assemblyData.m_strDir, assemblyFileName);
				assemblyFileName = Path.GetFullPath(assemblyFileName);
				new FileIOPermission(FileIOPermissionAccess.Write | FileIOPermissionAccess.Append, assemblyFileName).Demand();
				if (moduleBuilder != null)
				{
					for (int i = 0; i < base.m_assemblyData.m_iCABuilder; i++)
					{
						array[i] = base.m_assemblyData.m_CABuilders[i].PrepareCreateCustomAttributeToDisk(moduleBuilder);
					}
					for (int i = 0; i < base.m_assemblyData.m_iCAs; i++)
					{
						array2[i] = moduleBuilder.InternalGetConstructorToken(base.m_assemblyData.m_CACons[i], usingRef: true).Token;
					}
					moduleBuilder.PreSave(assemblyFileName, portableExecutableKind, imageFileMachine);
				}
				nPrepareForSavingManifestToDisk(moduleBuilder);
				ModuleBuilder onDiskAssemblyModule = base.m_assemblyData.GetOnDiskAssemblyModule();
				if (base.m_assemblyData.m_strResourceFileName != null)
				{
					onDiskAssemblyModule.DefineUnmanagedResourceFileInternalNoLock(base.m_assemblyData.m_strResourceFileName);
				}
				else if (base.m_assemblyData.m_resourceBytes != null)
				{
					onDiskAssemblyModule.DefineUnmanagedResourceInternalNoLock(base.m_assemblyData.m_resourceBytes);
				}
				else if (base.m_assemblyData.m_hasUnmanagedVersionInfo)
				{
					base.m_assemblyData.FillUnmanagedVersionInfo();
					string text2 = base.m_assemblyData.m_nativeVersion.m_strFileVersion;
					if (text2 == null)
					{
						text2 = GetVersion().ToString();
					}
					text = Assembly.nDefineVersionInfoResource(assemblyFileName, base.m_assemblyData.m_nativeVersion.m_strTitle, null, base.m_assemblyData.m_nativeVersion.m_strDescription, base.m_assemblyData.m_nativeVersion.m_strCopyright, base.m_assemblyData.m_nativeVersion.m_strTrademark, base.m_assemblyData.m_nativeVersion.m_strCompany, base.m_assemblyData.m_nativeVersion.m_strProduct, base.m_assemblyData.m_nativeVersion.m_strProductVersion, text2, base.m_assemblyData.m_nativeVersion.m_lcid, base.m_assemblyData.m_peFileKind == PEFileKinds.Dll);
					onDiskAssemblyModule.DefineUnmanagedResourceFileInternalNoLock(text);
				}
				if (moduleBuilder == null)
				{
					for (int i = 0; i < base.m_assemblyData.m_iCABuilder; i++)
					{
						array[i] = base.m_assemblyData.m_CABuilders[i].PrepareCreateCustomAttributeToDisk(onDiskAssemblyModule);
					}
					for (int i = 0; i < base.m_assemblyData.m_iCAs; i++)
					{
						array2[i] = onDiskAssemblyModule.InternalGetConstructorToken(base.m_assemblyData.m_CACons[i], usingRef: true).Token;
					}
				}
				int count = base.m_assemblyData.m_moduleBuilderList.Count;
				for (int i = 0; i < count; i++)
				{
					ModuleBuilder moduleBuilder2 = (ModuleBuilder)base.m_assemblyData.m_moduleBuilderList[i];
					if (!moduleBuilder2.IsTransient() && moduleBuilder2 != moduleBuilder)
					{
						string text3 = moduleBuilder2.m_moduleData.m_strFileName;
						if (base.m_assemblyData.m_strDir != null)
						{
							text3 = Path.Combine(base.m_assemblyData.m_strDir, text3);
							text3 = Path.GetFullPath(text3);
						}
						new FileIOPermission(FileIOPermissionAccess.Write | FileIOPermissionAccess.Append, text3).Demand();
						moduleBuilder2.m_moduleData.m_tkFile = nSaveToFileList(moduleBuilder2.m_moduleData.m_strFileName);
						moduleBuilder2.PreSave(text3, portableExecutableKind, imageFileMachine);
						moduleBuilder2.Save(text3, isAssemblyFile: false, portableExecutableKind, imageFileMachine);
						nSetHashValue(moduleBuilder2.m_moduleData.m_tkFile, text3);
					}
				}
				for (int i = 0; i < base.m_assemblyData.m_iPublicComTypeCount; i++)
				{
					Type type = base.m_assemblyData.m_publicComTypeList[i];
					ModuleBuilder moduleBuilder3;
					if (type is RuntimeType)
					{
						moduleBuilder3 = base.m_assemblyData.FindModuleWithName(type.Module.m_moduleData.m_strModuleName);
						if (moduleBuilder3 != moduleBuilder)
						{
							DefineNestedComType(type, moduleBuilder3.m_moduleData.m_tkFile, type.MetadataTokenInternal);
						}
						continue;
					}
					TypeBuilder typeBuilder = (TypeBuilder)type;
					moduleBuilder3 = (ModuleBuilder)type.Module;
					if (moduleBuilder3 != moduleBuilder)
					{
						DefineNestedComType(type, moduleBuilder3.m_moduleData.m_tkFile, typeBuilder.MetadataTokenInternal);
					}
				}
				for (int i = 0; i < base.m_assemblyData.m_iCABuilder; i++)
				{
					base.m_assemblyData.m_CABuilders[i].CreateCustomAttribute(onDiskAssemblyModule, 536870913, array[i], toDisk: true);
				}
				for (int i = 0; i < base.m_assemblyData.m_iCAs; i++)
				{
					TypeBuilder.InternalCreateCustomAttribute(536870913, array2[i], base.m_assemblyData.m_CABytes[i], onDiskAssemblyModule, toDisk: true);
				}
				if (base.m_assemblyData.m_RequiredPset != null || base.m_assemblyData.m_OptionalPset != null || base.m_assemblyData.m_RefusedPset != null)
				{
					byte[] required = null;
					byte[] optional = null;
					byte[] refused = null;
					if (base.m_assemblyData.m_RequiredPset != null)
					{
						required = base.m_assemblyData.m_RequiredPset.EncodeXml();
					}
					if (base.m_assemblyData.m_OptionalPset != null)
					{
						optional = base.m_assemblyData.m_OptionalPset.EncodeXml();
					}
					if (base.m_assemblyData.m_RefusedPset != null)
					{
						refused = base.m_assemblyData.m_RefusedPset.EncodeXml();
					}
					nSavePermissionRequests(required, optional, refused);
				}
				count = base.m_assemblyData.m_resWriterList.Count;
				for (int i = 0; i < count; i++)
				{
					ResWriterData resWriterData = null;
					try
					{
						resWriterData = (ResWriterData)base.m_assemblyData.m_resWriterList[i];
						if (resWriterData.m_resWriter != null)
						{
							new FileIOPermission(FileIOPermissionAccess.Write | FileIOPermissionAccess.Append, resWriterData.m_strFullFileName).Demand();
						}
					}
					finally
					{
						if (resWriterData != null && resWriterData.m_resWriter != null)
						{
							resWriterData.m_resWriter.Close();
						}
					}
					nAddStandAloneResource(resWriterData.m_strName, resWriterData.m_strFileName, resWriterData.m_strFullFileName, (int)resWriterData.m_attribute);
				}
				if (moduleBuilder == null)
				{
					if (onDiskAssemblyModule.m_moduleData.m_strResourceFileName != null)
					{
						onDiskAssemblyModule.InternalDefineNativeResourceFile(onDiskAssemblyModule.m_moduleData.m_strResourceFileName, (int)portableExecutableKind, (int)imageFileMachine);
					}
					else if (onDiskAssemblyModule.m_moduleData.m_resourceBytes != null)
					{
						onDiskAssemblyModule.InternalDefineNativeResourceBytes(onDiskAssemblyModule.m_moduleData.m_resourceBytes, (int)portableExecutableKind, (int)imageFileMachine);
					}
					if (base.m_assemblyData.m_entryPointModule != null)
					{
						nSaveManifestToDisk(assemblyFileName, base.m_assemblyData.m_entryPointModule.m_moduleData.m_tkFile, (int)base.m_assemblyData.m_peFileKind, (int)portableExecutableKind, (int)imageFileMachine);
					}
					else
					{
						nSaveManifestToDisk(assemblyFileName, 0, (int)base.m_assemblyData.m_peFileKind, (int)portableExecutableKind, (int)imageFileMachine);
					}
				}
				else
				{
					if (base.m_assemblyData.m_entryPointModule != null && base.m_assemblyData.m_entryPointModule != moduleBuilder)
					{
						moduleBuilder.m_EntryPoint = new MethodToken(base.m_assemblyData.m_entryPointModule.m_moduleData.m_tkFile);
					}
					moduleBuilder.Save(assemblyFileName, isAssemblyFile: true, portableExecutableKind, imageFileMachine);
				}
				base.m_assemblyData.m_isSaved = true;
			}
			finally
			{
				if (text != null)
				{
					File.Delete(text);
				}
			}
		}

		internal bool IsPersistable()
		{
			if ((base.m_assemblyData.m_access & AssemblyBuilderAccess.Save) == AssemblyBuilderAccess.Save)
			{
				return true;
			}
			return false;
		}

		private int DefineNestedComType(Type type, int tkResolutionScope, int tkTypeDef)
		{
			Type declaringType = type.DeclaringType;
			if (declaringType == null)
			{
				return nSaveExportedType(type.FullName, tkResolutionScope, tkTypeDef, type.Attributes);
			}
			tkResolutionScope = DefineNestedComType(declaringType, tkResolutionScope, tkTypeDef);
			return nSaveExportedType(type.FullName, tkResolutionScope, tkTypeDef, type.Attributes);
		}

		private AssemblyBuilder()
		{
		}

		void _AssemblyBuilder.GetTypeInfoCount(out uint pcTInfo)
		{
			throw new NotImplementedException();
		}

		void _AssemblyBuilder.GetTypeInfo(uint iTInfo, uint lcid, IntPtr ppTInfo)
		{
			throw new NotImplementedException();
		}

		void _AssemblyBuilder.GetIDsOfNames([In] ref Guid riid, IntPtr rgszNames, uint cNames, uint lcid, IntPtr rgDispId)
		{
			throw new NotImplementedException();
		}

		void _AssemblyBuilder.Invoke(uint dispIdMember, [In] ref Guid riid, uint lcid, short wFlags, IntPtr pDispParams, IntPtr pVarResult, IntPtr pExcepInfo, IntPtr puArgErr)
		{
			throw new NotImplementedException();
		}
	}
}
