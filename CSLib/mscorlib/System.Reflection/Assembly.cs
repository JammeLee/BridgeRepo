using System.Collections;
using System.Configuration.Assemblies;
using System.Globalization;
using System.IO;
using System.Reflection.Cache;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using System.Security.Permissions;
using System.Security.Policy;
using System.Security.Util;
using System.Text;
using System.Threading;
using Microsoft.Win32;

namespace System.Reflection
{
	[Serializable]
	[ComVisible(true)]
	[ClassInterface(ClassInterfaceType.None)]
	[ComDefaultInterface(typeof(_Assembly))]
	public class Assembly : _Assembly, IEvidenceFactory, ICustomAttributeProvider, ISerializable
	{
		private const string s_localFilePrefix = "file:";

		internal AssemblyBuilderData m__assemblyData;

		private InternalCache m__cachedData;

		private IntPtr m__assembly;

		internal virtual Assembly InternalAssembly => this;

		internal AssemblyBuilderData m_assemblyData
		{
			get
			{
				return InternalAssembly.m__assemblyData;
			}
			set
			{
				InternalAssembly.m__assemblyData = value;
			}
		}

		private ModuleResolveEventHandler ModuleResolveEvent => InternalAssembly._ModuleResolve;

		internal InternalCache m_cachedData
		{
			get
			{
				return InternalAssembly.m__cachedData;
			}
			set
			{
				InternalAssembly.m__cachedData = value;
			}
		}

		internal IntPtr m_assembly
		{
			get
			{
				return InternalAssembly.m__assembly;
			}
			set
			{
				InternalAssembly.m__assembly = value;
			}
		}

		public virtual string CodeBase
		{
			get
			{
				string text = nGetCodeBase(fCopiedName: false);
				VerifyCodeBaseDiscovery(text);
				return text;
			}
		}

		public virtual string EscapedCodeBase => AssemblyName.EscapeCodeBase(CodeBase);

		internal unsafe AssemblyHandle AssemblyHandle => new AssemblyHandle((void*)m_assembly);

		public virtual string FullName
		{
			get
			{
				string result;
				if ((result = (string)Cache[CacheObjType.AssemblyName]) != null)
				{
					return result;
				}
				result = GetFullName();
				if (result != null)
				{
					Cache[CacheObjType.AssemblyName] = result;
				}
				return result;
			}
		}

		public virtual MethodInfo EntryPoint
		{
			get
			{
				RuntimeMethodHandle methodHandle = nGetEntryPoint();
				if (!methodHandle.IsNullHandle())
				{
					return (MethodInfo)RuntimeType.GetMethodBase(methodHandle);
				}
				return null;
			}
		}

		public virtual Evidence Evidence
		{
			[SecurityPermission(SecurityAction.Demand, ControlEvidence = true)]
			get
			{
				return nGetEvidence().Copy();
			}
		}

		[ComVisible(false)]
		public Module ManifestModule
		{
			get
			{
				ModuleHandle manifestModule = AssemblyHandle.GetManifestModule();
				if (manifestModule.IsNullHandle())
				{
					return null;
				}
				return manifestModule.GetModule();
			}
		}

		[ComVisible(false)]
		public virtual bool ReflectionOnly => nReflection();

		public virtual string Location
		{
			get
			{
				string location = GetLocation();
				if (location != null)
				{
					new FileIOPermission(FileIOPermissionAccess.PathDiscovery, location).Demand();
				}
				return location;
			}
		}

		[ComVisible(false)]
		public virtual string ImageRuntimeVersion => nGetImageRuntimeVersion();

		public bool GlobalAssemblyCache => nGlobalAssemblyCache();

		[ComVisible(false)]
		public long HostContext => GetHostContext();

		internal InternalCache Cache
		{
			get
			{
				InternalCache internalCache = m_cachedData;
				if (internalCache == null)
				{
					internalCache = (m_cachedData = new InternalCache("Assembly"));
					GC.ClearCache += OnCacheClear;
				}
				return internalCache;
			}
		}

		[method: SecurityPermission(SecurityAction.LinkDemand, ControlAppDomain = true)]
		private event ModuleResolveEventHandler _ModuleResolve;

		public event ModuleResolveEventHandler ModuleResolve
		{
			[SecurityPermission(SecurityAction.LinkDemand, ControlAppDomain = true)]
			add
			{
				Assembly internalAssembly = InternalAssembly;
				internalAssembly._ModuleResolve = (ModuleResolveEventHandler)Delegate.Combine(internalAssembly._ModuleResolve, value);
			}
			[SecurityPermission(SecurityAction.LinkDemand, ControlAppDomain = true)]
			remove
			{
				Assembly internalAssembly = InternalAssembly;
				internalAssembly._ModuleResolve = (ModuleResolveEventHandler)Delegate.Remove(internalAssembly._ModuleResolve, value);
			}
		}

		public override bool Equals(object o)
		{
			if (o == null)
			{
				return false;
			}
			if (!(o is Assembly))
			{
				return false;
			}
			Assembly assembly = o as Assembly;
			assembly = assembly.InternalAssembly;
			return InternalAssembly == assembly;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		public virtual AssemblyName GetName()
		{
			return GetName(copiedName: false);
		}

		public virtual AssemblyName GetName(bool copiedName)
		{
			AssemblyName assemblyName = new AssemblyName();
			string codeBase = nGetCodeBase(copiedName);
			VerifyCodeBaseDiscovery(codeBase);
			assemblyName.Init(nGetSimpleName(), nGetPublicKey(), null, GetVersion(), GetLocale(), nGetHashAlgorithm(), AssemblyVersionCompatibility.SameMachine, codeBase, nGetFlags() | AssemblyNameFlags.PublicKey, null);
			assemblyName.ProcessorArchitecture = ComputeProcArchIndex();
			return assemblyName;
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		public static extern string CreateQualifiedName(string assemblyName, string typeName);

		public static Assembly GetAssembly(Type type)
		{
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}
			return type.Module?.Assembly;
		}

		Type _Assembly.GetType()
		{
			return GetType();
		}

		public virtual Type GetType(string name)
		{
			return GetType(name, throwOnError: false, ignoreCase: false);
		}

		public virtual Type GetType(string name, bool throwOnError)
		{
			return GetType(name, throwOnError, ignoreCase: false);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern Type _GetType(string name, bool throwOnError, bool ignoreCase);

		public Type GetType(string name, bool throwOnError, bool ignoreCase)
		{
			return InternalAssembly._GetType(name, throwOnError, ignoreCase);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern Type[] _GetExportedTypes();

		public virtual Type[] GetExportedTypes()
		{
			return InternalAssembly._GetExportedTypes();
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public virtual Type[] GetTypes()
		{
			Module[] array = nGetModules(loadIfNotFound: true, getResourceModules: false);
			int num = array.Length;
			int num2 = 0;
			StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
			Type[][] array2 = new Type[num][];
			for (int i = 0; i < num; i++)
			{
				array2[i] = array[i].GetTypesInternal(ref stackMark);
				num2 += array2[i].Length;
			}
			int num3 = 0;
			Type[] array3 = new Type[num2];
			for (int j = 0; j < num; j++)
			{
				int num4 = array2[j].Length;
				Array.Copy(array2[j], 0, array3, num3, num4);
				num3 += num4;
			}
			return array3;
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public virtual Stream GetManifestResourceStream(Type type, string name)
		{
			StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
			return GetManifestResourceStream(type, name, skipSecurityCheck: false, ref stackMark);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public virtual Stream GetManifestResourceStream(string name)
		{
			StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
			return GetManifestResourceStream(name, ref stackMark, skipSecurityCheck: false);
		}

		public Assembly GetSatelliteAssembly(CultureInfo culture)
		{
			return InternalGetSatelliteAssembly(culture, null, throwOnFileNotFound: true);
		}

		public Assembly GetSatelliteAssembly(CultureInfo culture, Version version)
		{
			return InternalGetSatelliteAssembly(culture, version, throwOnFileNotFound: true);
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
		public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			UnitySerializationHolder.GetUnitySerializationInfo(info, 6, FullName, this);
		}

		internal bool AptcaCheck(Assembly sourceAssembly)
		{
			return AssemblyHandle.AptcaCheck(sourceAssembly.AssemblyHandle);
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

		[MethodImpl(MethodImplOptions.NoInlining)]
		public static Assembly LoadFrom(string assemblyFile)
		{
			StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
			return InternalLoadFrom(assemblyFile, null, null, AssemblyHashAlgorithm.None, forIntrospection: false, ref stackMark);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public static Assembly ReflectionOnlyLoadFrom(string assemblyFile)
		{
			StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
			return InternalLoadFrom(assemblyFile, null, null, AssemblyHashAlgorithm.None, forIntrospection: true, ref stackMark);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public static Assembly LoadFrom(string assemblyFile, Evidence securityEvidence)
		{
			StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
			return InternalLoadFrom(assemblyFile, securityEvidence, null, AssemblyHashAlgorithm.None, forIntrospection: false, ref stackMark);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public static Assembly LoadFrom(string assemblyFile, Evidence securityEvidence, byte[] hashValue, AssemblyHashAlgorithm hashAlgorithm)
		{
			StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
			return InternalLoadFrom(assemblyFile, securityEvidence, hashValue, hashAlgorithm, forIntrospection: false, ref stackMark);
		}

		private static Assembly InternalLoadFrom(string assemblyFile, Evidence securityEvidence, byte[] hashValue, AssemblyHashAlgorithm hashAlgorithm, bool forIntrospection, ref StackCrawlMark stackMark)
		{
			if (assemblyFile == null)
			{
				throw new ArgumentNullException("assemblyFile");
			}
			AssemblyName assemblyName = new AssemblyName();
			assemblyName.CodeBase = assemblyFile;
			assemblyName.SetHashControl(hashValue, hashAlgorithm);
			return InternalLoad(assemblyName, securityEvidence, ref stackMark, forIntrospection);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public static Assembly Load(string assemblyString)
		{
			StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
			return InternalLoad(assemblyString, null, ref stackMark, forIntrospection: false);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public static Assembly ReflectionOnlyLoad(string assemblyString)
		{
			StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
			return InternalLoad(assemblyString, null, ref stackMark, forIntrospection: true);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public static Assembly Load(string assemblyString, Evidence assemblySecurity)
		{
			StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
			return InternalLoad(assemblyString, assemblySecurity, ref stackMark, forIntrospection: false);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public static Assembly Load(AssemblyName assemblyRef)
		{
			StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
			return InternalLoad(assemblyRef, null, ref stackMark, forIntrospection: false);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public static Assembly Load(AssemblyName assemblyRef, Evidence assemblySecurity)
		{
			StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
			return InternalLoad(assemblyRef, assemblySecurity, ref stackMark, forIntrospection: false);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		private unsafe static IntPtr LoadWithPartialNameHack(string partialName, bool cropPublicKey)
		{
			StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
			Assembly assembly = null;
			AssemblyName assemblyName = new AssemblyName(partialName);
			if (!IsSimplyNamed(assemblyName))
			{
				if (cropPublicKey)
				{
					assemblyName.SetPublicKey(null);
					assemblyName.SetPublicKeyToken(null);
				}
				AssemblyName assemblyName2 = EnumerateCache(assemblyName);
				if (assemblyName2 != null)
				{
					assembly = InternalLoad(assemblyName2, null, ref stackMark, forIntrospection: false);
				}
			}
			if (assembly == null)
			{
				return (IntPtr)0;
			}
			return (IntPtr)assembly.AssemblyHandle.Value;
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		[Obsolete("This method has been deprecated. Please use Assembly.Load() instead. http://go.microsoft.com/fwlink/?linkid=14202")]
		public static Assembly LoadWithPartialName(string partialName)
		{
			StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
			return LoadWithPartialNameInternal(partialName, null, ref stackMark);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		[Obsolete("This method has been deprecated. Please use Assembly.Load() instead. http://go.microsoft.com/fwlink/?linkid=14202")]
		public static Assembly LoadWithPartialName(string partialName, Evidence securityEvidence)
		{
			StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
			return LoadWithPartialNameInternal(partialName, securityEvidence, ref stackMark);
		}

		internal static Assembly LoadWithPartialNameInternal(string partialName, Evidence securityEvidence, ref StackCrawlMark stackMark)
		{
			if (securityEvidence != null)
			{
				new SecurityPermission(SecurityPermissionFlag.ControlEvidence).Demand();
			}
			Assembly result = null;
			AssemblyName assemblyName = new AssemblyName(partialName);
			try
			{
				result = nLoad(assemblyName, null, securityEvidence, null, ref stackMark, throwOnFileNotFound: true, forIntrospection: false);
				return result;
			}
			catch (Exception ex)
			{
				if (ex.IsTransient)
				{
					throw ex;
				}
				if (IsSimplyNamed(assemblyName))
				{
					return null;
				}
				AssemblyName assemblyName2 = EnumerateCache(assemblyName);
				if (assemblyName2 != null)
				{
					return InternalLoad(assemblyName2, securityEvidence, ref stackMark, forIntrospection: false);
				}
				return result;
			}
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern bool _nReflection();

		internal bool nReflection()
		{
			return InternalAssembly._nReflection();
		}

		private static AssemblyName EnumerateCache(AssemblyName partialName)
		{
			new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Assert();
			partialName.Version = null;
			ArrayList arrayList = new ArrayList();
			Fusion.ReadCache(arrayList, partialName.FullName, 2u);
			IEnumerator enumerator = arrayList.GetEnumerator();
			AssemblyName assemblyName = null;
			CultureInfo cultureInfo = partialName.CultureInfo;
			while (enumerator.MoveNext())
			{
				AssemblyName assemblyName2 = new AssemblyName((string)enumerator.Current);
				if (CulturesEqual(cultureInfo, assemblyName2.CultureInfo))
				{
					if (assemblyName == null)
					{
						assemblyName = assemblyName2;
					}
					else if (assemblyName2.Version > assemblyName.Version)
					{
						assemblyName = assemblyName2;
					}
				}
			}
			return assemblyName;
		}

		private static bool CulturesEqual(CultureInfo refCI, CultureInfo defCI)
		{
			bool flag = defCI.Equals(CultureInfo.InvariantCulture);
			if (refCI == null || refCI.Equals(CultureInfo.InvariantCulture))
			{
				return flag;
			}
			if (flag || !defCI.Equals(refCI))
			{
				return false;
			}
			return true;
		}

		private static bool IsSimplyNamed(AssemblyName partialName)
		{
			byte[] publicKeyToken = partialName.GetPublicKeyToken();
			if (publicKeyToken != null && publicKeyToken.Length == 0)
			{
				return true;
			}
			publicKeyToken = partialName.GetPublicKey();
			if (publicKeyToken != null && publicKeyToken.Length == 0)
			{
				return true;
			}
			return false;
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public static Assembly Load(byte[] rawAssembly)
		{
			StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
			return nLoadImage(rawAssembly, null, null, ref stackMark, fIntrospection: false);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public static Assembly ReflectionOnlyLoad(byte[] rawAssembly)
		{
			StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
			return nLoadImage(rawAssembly, null, null, ref stackMark, fIntrospection: true);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public static Assembly Load(byte[] rawAssembly, byte[] rawSymbolStore)
		{
			StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
			return nLoadImage(rawAssembly, rawSymbolStore, null, ref stackMark, fIntrospection: false);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		[SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.ControlEvidence)]
		public static Assembly Load(byte[] rawAssembly, byte[] rawSymbolStore, Evidence securityEvidence)
		{
			StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
			return nLoadImage(rawAssembly, rawSymbolStore, securityEvidence, ref stackMark, fIntrospection: false);
		}

		public static Assembly LoadFile(string path)
		{
			new FileIOPermission(FileIOPermissionAccess.Read | FileIOPermissionAccess.PathDiscovery, path).Demand();
			return nLoadFile(path, null);
		}

		[SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.ControlEvidence)]
		public static Assembly LoadFile(string path, Evidence securityEvidence)
		{
			new FileIOPermission(FileIOPermissionAccess.Read | FileIOPermissionAccess.PathDiscovery, path).Demand();
			return nLoadFile(path, securityEvidence);
		}

		public Module LoadModule(string moduleName, byte[] rawModule)
		{
			return nLoadModule(moduleName, rawModule, null, Evidence);
		}

		public Module LoadModule(string moduleName, byte[] rawModule, byte[] rawSymbolStore)
		{
			return nLoadModule(moduleName, rawModule, rawSymbolStore, Evidence);
		}

		public object CreateInstance(string typeName)
		{
			return CreateInstance(typeName, ignoreCase: false, BindingFlags.Instance | BindingFlags.Public, null, null, null, null);
		}

		public object CreateInstance(string typeName, bool ignoreCase)
		{
			return CreateInstance(typeName, ignoreCase, BindingFlags.Instance | BindingFlags.Public, null, null, null, null);
		}

		public object CreateInstance(string typeName, bool ignoreCase, BindingFlags bindingAttr, Binder binder, object[] args, CultureInfo culture, object[] activationAttributes)
		{
			Type type = GetType(typeName, throwOnError: false, ignoreCase);
			if (type == null)
			{
				return null;
			}
			return Activator.CreateInstance(type, bindingAttr, binder, args, culture, activationAttributes);
		}

		public Module[] GetLoadedModules()
		{
			return nGetModules(loadIfNotFound: false, getResourceModules: false);
		}

		public Module[] GetLoadedModules(bool getResourceModules)
		{
			return nGetModules(loadIfNotFound: false, getResourceModules);
		}

		public Module[] GetModules()
		{
			return nGetModules(loadIfNotFound: true, getResourceModules: false);
		}

		public Module[] GetModules(bool getResourceModules)
		{
			return nGetModules(loadIfNotFound: true, getResourceModules);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal extern Module _GetModule(string name);

		public Module GetModule(string name)
		{
			return GetModuleInternal(name);
		}

		internal virtual Module GetModuleInternal(string name)
		{
			return InternalAssembly._GetModule(name);
		}

		public virtual FileStream GetFile(string name)
		{
			Module module = GetModule(name);
			if (module == null)
			{
				return null;
			}
			return new FileStream(module.InternalGetFullyQualifiedName(), FileMode.Open, FileAccess.Read, FileShare.Read);
		}

		public virtual FileStream[] GetFiles()
		{
			return GetFiles(getResourceModules: false);
		}

		public virtual FileStream[] GetFiles(bool getResourceModules)
		{
			Module[] array = nGetModules(loadIfNotFound: true, getResourceModules);
			int num = array.Length;
			FileStream[] array2 = new FileStream[num];
			for (int i = 0; i < num; i++)
			{
				array2[i] = new FileStream(array[i].InternalGetFullyQualifiedName(), FileMode.Open, FileAccess.Read, FileShare.Read);
			}
			return array2;
		}

		public virtual string[] GetManifestResourceNames()
		{
			return nGetManifestResourceNames();
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern string[] _nGetManifestResourceNames();

		internal string[] nGetManifestResourceNames()
		{
			return InternalAssembly._nGetManifestResourceNames();
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public static Assembly GetExecutingAssembly()
		{
			StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
			return nGetExecutingAssembly(ref stackMark);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public static Assembly GetCallingAssembly()
		{
			StackCrawlMark stackMark = StackCrawlMark.LookForMyCallersCaller;
			return nGetExecutingAssembly(ref stackMark);
		}

		public static Assembly GetEntryAssembly()
		{
			AppDomainManager appDomainManager = AppDomain.CurrentDomain.DomainManager;
			if (appDomainManager == null)
			{
				appDomainManager = new AppDomainManager();
			}
			return appDomainManager.EntryAssembly;
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern AssemblyName[] _GetReferencedAssemblies();

		public AssemblyName[] GetReferencedAssemblies()
		{
			return InternalAssembly._GetReferencedAssemblies();
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public virtual ManifestResourceInfo GetManifestResourceInfo(string resourceName)
		{
			StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
			Assembly assemblyRef;
			string fileName;
			int num = nGetManifestResourceInfo(resourceName, out assemblyRef, out fileName, ref stackMark);
			if (num == -1)
			{
				return null;
			}
			return new ManifestResourceInfo(assemblyRef, fileName, (ResourceLocation)num);
		}

		public override string ToString()
		{
			string fullName = FullName;
			if (fullName == null)
			{
				return base.ToString();
			}
			return fullName;
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern long _GetHostContext();

		private long GetHostContext()
		{
			return InternalAssembly._GetHostContext();
		}

		internal static string VerifyCodeBase(string codebase)
		{
			if (codebase == null)
			{
				return null;
			}
			int length = codebase.Length;
			if (length == 0)
			{
				return null;
			}
			int num = codebase.IndexOf(':');
			if (num != -1 && num + 2 < length && (codebase[num + 1] == '/' || codebase[num + 1] == '\\') && (codebase[num + 2] == '/' || codebase[num + 2] == '\\'))
			{
				return codebase;
			}
			if (length > 2 && codebase[0] == '\\' && codebase[1] == '\\')
			{
				return "file://" + codebase;
			}
			return "file:///" + Path.GetFullPathInternal(codebase);
		}

		internal virtual Stream GetManifestResourceStream(Type type, string name, bool skipSecurityCheck, ref StackCrawlMark stackMark)
		{
			StringBuilder stringBuilder = new StringBuilder();
			if (type == null)
			{
				if (name == null)
				{
					throw new ArgumentNullException("type");
				}
			}
			else
			{
				string @namespace = type.Namespace;
				if (@namespace != null)
				{
					stringBuilder.Append(@namespace);
					if (name != null)
					{
						stringBuilder.Append(Type.Delimiter);
					}
				}
			}
			if (name != null)
			{
				stringBuilder.Append(name);
			}
			return GetManifestResourceStream(stringBuilder.ToString(), ref stackMark, skipSecurityCheck);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern Module _nLoadModule(string moduleName, byte[] rawModule, byte[] rawSymbolStore, Evidence securityEvidence);

		private Module nLoadModule(string moduleName, byte[] rawModule, byte[] rawSymbolStore, Evidence securityEvidence)
		{
			return InternalAssembly._nLoadModule(moduleName, rawModule, rawSymbolStore, securityEvidence);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern bool _nGlobalAssemblyCache();

		private bool nGlobalAssemblyCache()
		{
			return InternalAssembly._nGlobalAssemblyCache();
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern string _nGetImageRuntimeVersion();

		private string nGetImageRuntimeVersion()
		{
			return InternalAssembly._nGetImageRuntimeVersion();
		}

		internal Assembly()
		{
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern Module _nDefineDynamicModule(Assembly containingAssembly, bool emitSymbolInfo, string filename, ref StackCrawlMark stackMark);

		internal static Module nDefineDynamicModule(Assembly containingAssembly, bool emitSymbolInfo, string filename, ref StackCrawlMark stackMark)
		{
			return _nDefineDynamicModule(containingAssembly.InternalAssembly, emitSymbolInfo, filename, ref stackMark);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern void _nPrepareForSavingManifestToDisk(Module assemblyModule);

		internal void nPrepareForSavingManifestToDisk(Module assemblyModule)
		{
			if (assemblyModule != null)
			{
				assemblyModule = assemblyModule.InternalModule;
			}
			InternalAssembly._nPrepareForSavingManifestToDisk(assemblyModule);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern int _nSaveToFileList(string strFileName);

		internal int nSaveToFileList(string strFileName)
		{
			return InternalAssembly._nSaveToFileList(strFileName);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern int _nSetHashValue(int tkFile, string strFullFileName);

		internal int nSetHashValue(int tkFile, string strFullFileName)
		{
			return InternalAssembly._nSetHashValue(tkFile, strFullFileName);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern int _nSaveExportedType(string strComTypeName, int tkAssemblyRef, int tkTypeDef, TypeAttributes flags);

		internal int nSaveExportedType(string strComTypeName, int tkAssemblyRef, int tkTypeDef, TypeAttributes flags)
		{
			return InternalAssembly._nSaveExportedType(strComTypeName, tkAssemblyRef, tkTypeDef, flags);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern void _nSavePermissionRequests(byte[] required, byte[] optional, byte[] refused);

		internal void nSavePermissionRequests(byte[] required, byte[] optional, byte[] refused)
		{
			InternalAssembly._nSavePermissionRequests(required, optional, refused);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern void _nSaveManifestToDisk(string strFileName, int entryPoint, int fileKind, int portableExecutableKind, int ImageFileMachine);

		internal void nSaveManifestToDisk(string strFileName, int entryPoint, int fileKind, int portableExecutableKind, int ImageFileMachine)
		{
			InternalAssembly._nSaveManifestToDisk(strFileName, entryPoint, fileKind, portableExecutableKind, ImageFileMachine);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern int _nAddFileToInMemoryFileList(string strFileName, Module module);

		internal int nAddFileToInMemoryFileList(string strFileName, Module module)
		{
			if (module != null)
			{
				module = module.InternalModule;
			}
			return InternalAssembly._nAddFileToInMemoryFileList(strFileName, module);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern Module _nGetOnDiskAssemblyModule();

		internal Module nGetOnDiskAssemblyModule()
		{
			return InternalAssembly._nGetOnDiskAssemblyModule();
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern Module _nGetInMemoryAssemblyModule();

		internal Module nGetInMemoryAssemblyModule()
		{
			return InternalAssembly._nGetInMemoryAssemblyModule();
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern string nDefineVersionInfoResource(string filename, string title, string iconFilename, string description, string copyright, string trademark, string company, string product, string productVersion, string fileVersion, int lcid, bool isDll);

		private static void DecodeSerializedEvidence(Evidence evidence, byte[] serializedEvidence)
		{
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			Evidence evidence2 = null;
			PermissionSet permissionSet = new PermissionSet(fUnrestricted: false);
			permissionSet.SetPermission(new SecurityPermission(SecurityPermissionFlag.SerializationFormatter));
			permissionSet.PermitOnly();
			permissionSet.Assert();
			try
			{
				using MemoryStream serializationStream = new MemoryStream(serializedEvidence);
				evidence2 = (Evidence)binaryFormatter.Deserialize(serializationStream);
			}
			catch
			{
			}
			if (evidence2 != null)
			{
				IEnumerator assemblyEnumerator = evidence2.GetAssemblyEnumerator();
				while (assemblyEnumerator.MoveNext())
				{
					object current = assemblyEnumerator.Current;
					evidence.AddAssembly(current);
				}
			}
		}

		private static void AddX509Certificate(Evidence evidence, byte[] cert)
		{
			evidence.AddHost(new Publisher(new X509Certificate(cert)));
		}

		private static void AddStrongName(Evidence evidence, byte[] blob, string strSimpleName, int major, int minor, int build, int revision, Assembly assembly)
		{
			StrongName id = new StrongName(new StrongNamePublicKeyBlob(blob), strSimpleName, new Version(major, minor, build, revision), assembly);
			evidence.AddHost(id);
		}

		private static Evidence CreateSecurityIdentity(Assembly asm, string strUrl, int zone, byte[] cert, byte[] publicKeyBlob, string strSimpleName, int major, int minor, int build, int revision, byte[] serializedEvidence, Evidence additionalEvidence)
		{
			Evidence evidence = new Evidence();
			if (zone != -1)
			{
				evidence.AddHost(new Zone((SecurityZone)zone));
			}
			if (strUrl != null)
			{
				evidence.AddHost(new Url(strUrl, parsed: true));
				if (string.Compare(strUrl, 0, "file:", 0, 5, StringComparison.OrdinalIgnoreCase) != 0)
				{
					evidence.AddHost(Site.CreateFromUrl(strUrl));
				}
			}
			if (cert != null)
			{
				AddX509Certificate(evidence, cert);
			}
			if (asm != null && RuntimeEnvironment.FromGlobalAccessCache(asm))
			{
				evidence.AddHost(new GacInstalled());
			}
			if (serializedEvidence != null)
			{
				DecodeSerializedEvidence(evidence, serializedEvidence);
			}
			if (publicKeyBlob != null && publicKeyBlob.Length != 0)
			{
				AddStrongName(evidence, publicKeyBlob, strSimpleName, major, minor, build, revision, asm);
			}
			if (asm != null && !asm.nIsDynamic())
			{
				evidence.AddHost(new Hash(asm));
			}
			if (additionalEvidence != null)
			{
				evidence.MergeWithNoDuplicates(additionalEvidence);
			}
			if (asm != null)
			{
				HostSecurityManager hostSecurityManager = AppDomain.CurrentDomain.HostSecurityManager;
				if ((hostSecurityManager.Flags & HostSecurityManagerOptions.HostAssemblyEvidence) == HostSecurityManagerOptions.HostAssemblyEvidence)
				{
					return hostSecurityManager.ProvideAssemblyEvidence(asm, evidence);
				}
			}
			return evidence;
		}

		[FileIOPermission(SecurityAction.Assert, Unrestricted = true)]
		private bool IsAssemblyUnderAppBase()
		{
			string location = GetLocation();
			if (string.IsNullOrEmpty(location))
			{
				return true;
			}
			FileIOAccess fileIOAccess = new FileIOAccess(Path.GetFullPathInternal(location));
			FileIOAccess operand = new FileIOAccess(Path.GetFullPathInternal(AppDomain.CurrentDomain.BaseDirectory));
			return fileIOAccess.IsSubsetOf(operand);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal extern bool IsStrongNameVerified();

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern Assembly nGetExecutingAssembly(ref StackCrawlMark stackMark);

		internal unsafe virtual Stream GetManifestResourceStream(string name, ref StackCrawlMark stackMark, bool skipSecurityCheck)
		{
			ulong length = 0uL;
			byte* resource = GetResource(name, out length, ref stackMark, skipSecurityCheck);
			if (resource != null)
			{
				if (length > long.MaxValue)
				{
					throw new NotImplementedException(Environment.GetResourceString("NotImplemented_ResourcesLongerThan2^63"));
				}
				return new UnmanagedMemoryStream(resource, (long)length, (long)length, FileAccess.Read, skipSecurityCheck: true);
			}
			return null;
		}

		internal Version GetVersion()
		{
			nGetVersion(out var majVer, out var minVer, out var buildNum, out var revNum);
			return new Version(majVer, minVer, buildNum, revNum);
		}

		internal CultureInfo GetLocale()
		{
			string text = nGetLocale();
			if (text == null)
			{
				return CultureInfo.InvariantCulture;
			}
			return new CultureInfo(text);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern string _nGetLocale();

		private string nGetLocale()
		{
			return InternalAssembly._nGetLocale();
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern void _nGetVersion(out int majVer, out int minVer, out int buildNum, out int revNum);

		internal void nGetVersion(out int majVer, out int minVer, out int buildNum, out int revNum)
		{
			InternalAssembly._nGetVersion(out majVer, out minVer, out buildNum, out revNum);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern bool _nIsDynamic();

		internal bool nIsDynamic()
		{
			return InternalAssembly._nIsDynamic();
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern int _nGetManifestResourceInfo(string resourceName, out Assembly assemblyRef, out string fileName, ref StackCrawlMark stackMark);

		private int nGetManifestResourceInfo(string resourceName, out Assembly assemblyRef, out string fileName, ref StackCrawlMark stackMark)
		{
			return InternalAssembly._nGetManifestResourceInfo(resourceName, out assemblyRef, out fileName, ref stackMark);
		}

		private void VerifyCodeBaseDiscovery(string codeBase)
		{
			if (codeBase != null && string.Compare(codeBase, 0, "file:", 0, 5, StringComparison.OrdinalIgnoreCase) == 0)
			{
				URLString uRLString = new URLString(codeBase, parsed: true);
				new FileIOPermission(FileIOPermissionAccess.PathDiscovery, uRLString.GetFileName()).Demand();
			}
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern string _GetLocation();

		internal string GetLocation()
		{
			return InternalAssembly._GetLocation();
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern byte[] _nGetPublicKey();

		internal byte[] nGetPublicKey()
		{
			return InternalAssembly._nGetPublicKey();
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern string _nGetSimpleName();

		internal string nGetSimpleName()
		{
			return InternalAssembly._nGetSimpleName();
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern string _nGetCodeBase(bool fCopiedName);

		internal string nGetCodeBase(bool fCopiedName)
		{
			return InternalAssembly._nGetCodeBase(fCopiedName);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern AssemblyHashAlgorithm _nGetHashAlgorithm();

		internal AssemblyHashAlgorithm nGetHashAlgorithm()
		{
			return InternalAssembly._nGetHashAlgorithm();
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern AssemblyNameFlags _nGetFlags();

		internal AssemblyNameFlags nGetFlags()
		{
			return InternalAssembly._nGetFlags();
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal extern void _nGetGrantSet(out PermissionSet newGrant, out PermissionSet newDenied);

		internal void nGetGrantSet(out PermissionSet newGrant, out PermissionSet newDenied)
		{
			InternalAssembly._nGetGrantSet(out newGrant, out newDenied);
		}

		internal PermissionSet GetPermissionSet()
		{
			nGetGrantSet(out var newGrant, out var _);
			if (newGrant == null)
			{
				return new PermissionSet(PermissionState.Unrestricted);
			}
			return newGrant;
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern string _GetFullName();

		internal string GetFullName()
		{
			return InternalAssembly._GetFullName();
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private unsafe extern void* _nGetEntryPoint();

		private unsafe RuntimeMethodHandle nGetEntryPoint()
		{
			return new RuntimeMethodHandle(_nGetEntryPoint());
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal extern Evidence _nGetEvidence();

		internal Evidence nGetEvidence()
		{
			return InternalAssembly._nGetEvidence();
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private unsafe extern byte* _GetResource(string resourceName, out ulong length, ref StackCrawlMark stackMark, bool skipSecurityCheck);

		private unsafe byte* GetResource(string resourceName, out ulong length, ref StackCrawlMark stackMark, bool skipSecurityCheck)
		{
			return InternalAssembly._GetResource(resourceName, out length, ref stackMark, skipSecurityCheck);
		}

		internal static Assembly InternalLoad(string assemblyString, Evidence assemblySecurity, ref StackCrawlMark stackMark, bool forIntrospection)
		{
			if (assemblyString == null)
			{
				throw new ArgumentNullException("assemblyString");
			}
			if (assemblyString.Length == 0 || assemblyString[0] == '\0')
			{
				throw new ArgumentException(Environment.GetResourceString("Format_StringZeroLength"));
			}
			AssemblyName assemblyName = new AssemblyName();
			Assembly assembly = null;
			assemblyName.Name = assemblyString;
			int num = assemblyName.nInit(out assembly, forIntrospection, raiseResolveEvent: true);
			if (num == -2146234297)
			{
				return assembly;
			}
			return InternalLoad(assemblyName, assemblySecurity, ref stackMark, forIntrospection);
		}

		internal static Assembly InternalLoad(AssemblyName assemblyRef, Evidence assemblySecurity, ref StackCrawlMark stackMark, bool forIntrospection)
		{
			if (assemblyRef == null)
			{
				throw new ArgumentNullException("assemblyRef");
			}
			assemblyRef = (AssemblyName)assemblyRef.Clone();
			if (assemblySecurity != null)
			{
				new SecurityPermission(SecurityPermissionFlag.ControlEvidence).Demand();
			}
			string text = VerifyCodeBase(assemblyRef.CodeBase);
			if (text != null)
			{
				if (string.Compare(text, 0, "file:", 0, 5, StringComparison.OrdinalIgnoreCase) != 0)
				{
					IPermission permission = CreateWebPermission(assemblyRef.EscapedCodeBase);
					permission.Demand();
				}
				else
				{
					URLString uRLString = new URLString(text, parsed: true);
					new FileIOPermission(FileIOPermissionAccess.Read | FileIOPermissionAccess.PathDiscovery, uRLString.GetFileName()).Demand();
				}
			}
			return nLoad(assemblyRef, text, assemblySecurity, null, ref stackMark, throwOnFileNotFound: true, forIntrospection);
		}

		private static void DemandPermission(string codeBase, bool havePath, int demandFlag)
		{
			FileIOPermissionAccess access = FileIOPermissionAccess.PathDiscovery;
			switch (demandFlag)
			{
			case 1:
				access = FileIOPermissionAccess.Read;
				break;
			case 2:
				access = FileIOPermissionAccess.Read | FileIOPermissionAccess.PathDiscovery;
				break;
			case 3:
			{
				IPermission permission = CreateWebPermission(AssemblyName.EscapeCodeBase(codeBase));
				permission.Demand();
				return;
			}
			}
			if (!havePath)
			{
				URLString uRLString = new URLString(codeBase, parsed: true);
				codeBase = uRLString.GetFileName();
			}
			codeBase = Path.GetFullPathInternal(codeBase);
			new FileIOPermission(access, codeBase).Demand();
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern Assembly _nLoad(AssemblyName fileName, string codeBase, Evidence assemblySecurity, Assembly locationHint, ref StackCrawlMark stackMark, bool throwOnFileNotFound, bool forIntrospection);

		private static Assembly nLoad(AssemblyName fileName, string codeBase, Evidence assemblySecurity, Assembly locationHint, ref StackCrawlMark stackMark, bool throwOnFileNotFound, bool forIntrospection)
		{
			if (locationHint != null)
			{
				locationHint = locationHint.InternalAssembly;
			}
			return _nLoad(fileName, codeBase, assemblySecurity, locationHint, ref stackMark, throwOnFileNotFound, forIntrospection);
		}

		private static IPermission CreateWebPermission(string codeBase)
		{
			Assembly assembly = Load("System, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
			Type type = assembly.GetType("System.Net.NetworkAccess", throwOnError: true);
			IPermission permission = null;
			if (type.IsEnum && type.IsVisible)
			{
				object[] array = new object[2]
				{
					(Enum)Enum.Parse(type, "Connect", ignoreCase: true),
					null
				};
				if (array[0] != null)
				{
					array[1] = codeBase;
					type = assembly.GetType("System.Net.WebPermission", throwOnError: true);
					if (type.IsVisible)
					{
						permission = (IPermission)Activator.CreateInstance(type, array);
					}
				}
			}
			if (permission == null)
			{
				throw new ExecutionEngineException();
			}
			return permission;
		}

		private Module OnModuleResolveEvent(string moduleName)
		{
			ModuleResolveEventHandler moduleResolveEvent = ModuleResolveEvent;
			if (moduleResolveEvent == null)
			{
				return null;
			}
			Delegate[] invocationList = moduleResolveEvent.GetInvocationList();
			int num = invocationList.Length;
			for (int i = 0; i < num; i++)
			{
				Module module = ((ModuleResolveEventHandler)invocationList[i])(this, new ResolveEventArgs(moduleName));
				if (module != null)
				{
					return module.InternalModule;
				}
			}
			return null;
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		internal Assembly InternalGetSatelliteAssembly(CultureInfo culture, Version version, bool throwOnFileNotFound)
		{
			if (culture == null)
			{
				throw new ArgumentNullException("culture");
			}
			AssemblyName assemblyName = new AssemblyName();
			assemblyName.SetPublicKey(nGetPublicKey());
			assemblyName.Flags = nGetFlags() | AssemblyNameFlags.PublicKey;
			if (version == null)
			{
				assemblyName.Version = GetVersion();
			}
			else
			{
				assemblyName.Version = version;
			}
			assemblyName.CultureInfo = culture;
			assemblyName.Name = nGetSimpleName() + ".resources";
			StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
			Assembly assembly = nLoad(assemblyName, null, null, this, ref stackMark, throwOnFileNotFound, forIntrospection: false);
			if (assembly == this)
			{
				throw new FileNotFoundException(string.Format(culture, Environment.GetResourceString("IO.FileNotFound_FileName"), assemblyName.Name));
			}
			return assembly;
		}

		internal void OnCacheClear(object sender, ClearCacheEventArgs cacheEventArgs)
		{
			m_cachedData = null;
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern Assembly nLoadFile(string path, Evidence evidence);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern Assembly nLoadImage(byte[] rawAssembly, byte[] rawSymbolStore, Evidence evidence, ref StackCrawlMark stackMark, bool fIntrospection);

		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern void _nAddStandAloneResource(string strName, string strFileName, string strFullFileName, int attribute);

		internal void nAddStandAloneResource(string strName, string strFileName, string strFullFileName, int attribute)
		{
			InternalAssembly._nAddStandAloneResource(strName, strFileName, strFullFileName, attribute);
		}

		internal virtual Module[] nGetModules(bool loadIfNotFound, bool getResourceModules)
		{
			return InternalAssembly._nGetModules(loadIfNotFound, getResourceModules);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal extern Module[] _nGetModules(bool loadIfNotFound, bool getResourceModules);

		internal ProcessorArchitecture ComputeProcArchIndex()
		{
			Module manifestModule = ManifestModule;
			if (manifestModule != null && manifestModule.MDStreamVersion > 65536)
			{
				ManifestModule.GetPEKind(out var peKind, out var machine);
				if ((peKind & PortableExecutableKinds.PE32Plus) == PortableExecutableKinds.PE32Plus)
				{
					switch (machine)
					{
					case ImageFileMachine.IA64:
						return ProcessorArchitecture.IA64;
					case ImageFileMachine.AMD64:
						return ProcessorArchitecture.Amd64;
					case ImageFileMachine.I386:
						if ((peKind & PortableExecutableKinds.ILOnly) == PortableExecutableKinds.ILOnly)
						{
							return ProcessorArchitecture.MSIL;
						}
						break;
					}
				}
				else if (machine == ImageFileMachine.I386)
				{
					if ((peKind & PortableExecutableKinds.Required32Bit) == PortableExecutableKinds.Required32Bit)
					{
						return ProcessorArchitecture.X86;
					}
					if ((peKind & PortableExecutableKinds.ILOnly) == PortableExecutableKinds.ILOnly)
					{
						return ProcessorArchitecture.MSIL;
					}
					return ProcessorArchitecture.X86;
				}
			}
			return ProcessorArchitecture.None;
		}
	}
}
