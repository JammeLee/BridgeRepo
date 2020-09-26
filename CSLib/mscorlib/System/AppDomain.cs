using System.Collections;
using System.Collections.Generic;
using System.Configuration.Assemblies;
using System.Deployment.Internal.Isolation.Manifest;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Runtime.Hosting;
using System.Runtime.InteropServices;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Contexts;
using System.Security;
using System.Security.Permissions;
using System.Security.Policy;
using System.Security.Principal;
using System.Security.Util;
using System.Text;
using System.Threading;

namespace System
{
	[ComVisible(true)]
	[ComDefaultInterface(typeof(_AppDomain))]
	[ClassInterface(ClassInterfaceType.None)]
	public sealed class AppDomain : MarshalByRefObject, _AppDomain, IEvidenceFactory
	{
		internal class AssemblyBuilderLock
		{
		}

		[Serializable]
		private class EvidenceCollection
		{
			public Evidence ProvidedSecurityInfo;

			public Evidence CreatorsSecurityInfo;
		}

		private AppDomainManager _domainManager;

		private Hashtable _LocalStore;

		private AppDomainSetup _FusionStore;

		private Evidence _SecurityIdentity;

		private object[] _Policies;

		private Context _DefaultContext;

		private ActivationContext _activationContext;

		private ApplicationIdentity _applicationIdentity;

		private ApplicationTrust _applicationTrust;

		private IPrincipal _DefaultPrincipal;

		private DomainSpecificRemotingData _RemotingData;

		private EventHandler _processExit;

		private EventHandler _domainUnload;

		private UnhandledExceptionEventHandler _unhandledException;

		private IntPtr _dummyField;

		private PrincipalPolicy _PrincipalPolicy;

		private bool _HasSetPolicy;

		public AppDomainManager DomainManager
		{
			[SecurityPermission(SecurityAction.LinkDemand, ControlDomainPolicy = true)]
			get
			{
				return _domainManager;
			}
		}

		internal HostSecurityManager HostSecurityManager
		{
			get
			{
				HostSecurityManager hostSecurityManager = null;
				AppDomainManager domainManager = CurrentDomain.DomainManager;
				if (domainManager != null)
				{
					hostSecurityManager = domainManager.HostSecurityManager;
				}
				if (hostSecurityManager == null)
				{
					hostSecurityManager = new HostSecurityManager();
				}
				return hostSecurityManager;
			}
		}

		public static AppDomain CurrentDomain => Thread.GetDomain();

		public Evidence Evidence
		{
			[SecurityPermission(SecurityAction.Demand, ControlEvidence = true)]
			get
			{
				if (_SecurityIdentity == null)
				{
					if (IsDefaultAppDomain())
					{
						Assembly entryAssembly = Assembly.GetEntryAssembly();
						if (entryAssembly != null)
						{
							return entryAssembly.Evidence;
						}
						return new Evidence();
					}
					if (nIsDefaultAppDomainForSecurity())
					{
						return GetDefaultDomain().Evidence;
					}
				}
				Evidence internalEvidence = InternalEvidence;
				if (internalEvidence != null)
				{
					return internalEvidence.Copy();
				}
				return internalEvidence;
			}
		}

		internal Evidence InternalEvidence => _SecurityIdentity;

		public string FriendlyName => nGetFriendlyName();

		public string BaseDirectory => FusionStore.ApplicationBase;

		public string RelativeSearchPath => FusionStore.PrivateBinPath;

		public bool ShadowCopyFiles
		{
			get
			{
				string shadowCopyFiles = FusionStore.ShadowCopyFiles;
				if (shadowCopyFiles != null && string.Compare(shadowCopyFiles, "true", StringComparison.OrdinalIgnoreCase) == 0)
				{
					return true;
				}
				return false;
			}
		}

		public ActivationContext ActivationContext
		{
			[SecurityPermission(SecurityAction.LinkDemand, ControlDomainPolicy = true)]
			get
			{
				return _activationContext;
			}
		}

		public ApplicationIdentity ApplicationIdentity
		{
			[SecurityPermission(SecurityAction.LinkDemand, ControlDomainPolicy = true)]
			get
			{
				return _applicationIdentity;
			}
		}

		public ApplicationTrust ApplicationTrust
		{
			[SecurityPermission(SecurityAction.LinkDemand, ControlDomainPolicy = true)]
			get
			{
				return _applicationTrust;
			}
		}

		public string DynamicDirectory
		{
			get
			{
				string dynamicDir = GetDynamicDir();
				if (dynamicDir != null)
				{
					new FileIOPermission(FileIOPermissionAccess.PathDiscovery, dynamicDir).Demand();
				}
				return dynamicDir;
			}
		}

		internal DomainSpecificRemotingData RemotingData
		{
			get
			{
				if (_RemotingData == null)
				{
					CreateRemotingData();
				}
				return _RemotingData;
			}
		}

		internal AppDomainSetup FusionStore => _FusionStore;

		private Hashtable LocalStore
		{
			get
			{
				if (_LocalStore != null)
				{
					return _LocalStore;
				}
				_LocalStore = Hashtable.Synchronized(new Hashtable());
				return _LocalStore;
			}
		}

		public AppDomainSetup SetupInformation => new AppDomainSetup(FusionStore, copyDomainBoundData: true);

		public int Id
		{
			[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
			get
			{
				return GetId();
			}
		}

		[method: SecurityPermission(SecurityAction.LinkDemand, ControlAppDomain = true)]
		public event AssemblyLoadEventHandler AssemblyLoad;

		[method: SecurityPermission(SecurityAction.LinkDemand, ControlAppDomain = true)]
		public event ResolveEventHandler TypeResolve;

		[method: SecurityPermission(SecurityAction.LinkDemand, ControlAppDomain = true)]
		public event ResolveEventHandler ResourceResolve;

		[method: SecurityPermission(SecurityAction.LinkDemand, ControlAppDomain = true)]
		public event ResolveEventHandler AssemblyResolve;

		[method: SecurityPermission(SecurityAction.LinkDemand, ControlAppDomain = true)]
		public event ResolveEventHandler ReflectionOnlyAssemblyResolve;

		public event EventHandler ProcessExit
		{
			[SecurityPermission(SecurityAction.LinkDemand, ControlAppDomain = true)]
			add
			{
				if (value != null)
				{
					RuntimeHelpers.PrepareDelegate(value);
					lock (this)
					{
						_processExit = (EventHandler)Delegate.Combine(_processExit, value);
					}
				}
			}
			[SecurityPermission(SecurityAction.LinkDemand, ControlAppDomain = true)]
			remove
			{
				lock (this)
				{
					_processExit = (EventHandler)Delegate.Remove(_processExit, value);
				}
			}
		}

		public event EventHandler DomainUnload
		{
			[SecurityPermission(SecurityAction.LinkDemand, ControlAppDomain = true)]
			add
			{
				if (value != null)
				{
					RuntimeHelpers.PrepareDelegate(value);
					lock (this)
					{
						_domainUnload = (EventHandler)Delegate.Combine(_domainUnload, value);
					}
				}
			}
			[SecurityPermission(SecurityAction.LinkDemand, ControlAppDomain = true)]
			remove
			{
				lock (this)
				{
					_domainUnload = (EventHandler)Delegate.Remove(_domainUnload, value);
				}
			}
		}

		public event UnhandledExceptionEventHandler UnhandledException
		{
			[SecurityPermission(SecurityAction.LinkDemand, ControlAppDomain = true)]
			add
			{
				if (value != null)
				{
					RuntimeHelpers.PrepareDelegate(value);
					lock (this)
					{
						_unhandledException = (UnhandledExceptionEventHandler)Delegate.Combine(_unhandledException, value);
					}
				}
			}
			[SecurityPermission(SecurityAction.LinkDemand, ControlAppDomain = true)]
			remove
			{
				lock (this)
				{
					_unhandledException = (UnhandledExceptionEventHandler)Delegate.Remove(_unhandledException, value);
				}
			}
		}

		public new Type GetType()
		{
			return base.GetType();
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern string nGetDomainManagerAsm();

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern string nGetDomainManagerType();

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void nSetHostSecurityManagerFlags(HostSecurityManagerOptions flags);

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void nSetSecurityHomogeneousFlag();

		private void SetDefaultDomainManager(string fullName, string[] manifestPaths, string[] activationData)
		{
			if (fullName != null)
			{
				FusionStore.ActivationArguments = new ActivationArguments(fullName, manifestPaths, activationData);
			}
			SetDomainManager(null, null, IntPtr.Zero, publishAppDomain: false);
		}

		private void SetDomainManager(Evidence providedSecurityInfo, Evidence creatorsSecurityInfo, IntPtr parentSecurityDescriptor, bool publishAppDomain)
		{
			string text = nGetDomainManagerAsm();
			string text2 = nGetDomainManagerType();
			if (text != null && text2 != null)
			{
				_domainManager = CreateDomainManager(text, text2);
			}
			AppDomainSetup fusionStore = FusionStore;
			if (_domainManager != null)
			{
				_domainManager.InitializeNewDomain(fusionStore);
				AppDomainManagerInitializationOptions initializationFlags = _domainManager.InitializationFlags;
				if ((initializationFlags & AppDomainManagerInitializationOptions.RegisterWithHost) == AppDomainManagerInitializationOptions.RegisterWithHost)
				{
					_domainManager.nRegisterWithHost();
				}
			}
			if (fusionStore.ActivationArguments != null)
			{
				ActivationContext activationContext = null;
				ApplicationIdentity applicationIdentity = null;
				string[] array = null;
				CmsUtils.CreateActivationContext(fusionStore.ActivationArguments.ApplicationFullName, fusionStore.ActivationArguments.ApplicationManifestPaths, fusionStore.ActivationArguments.UseFusionActivationContext, out applicationIdentity, out activationContext);
				array = fusionStore.ActivationArguments.ActivationData;
				providedSecurityInfo = CmsUtils.MergeApplicationEvidence(providedSecurityInfo, applicationIdentity, activationContext, array, fusionStore.ApplicationTrust);
				SetupApplicationHelper(providedSecurityInfo, creatorsSecurityInfo, applicationIdentity, activationContext, array);
			}
			else
			{
				ApplicationTrust applicationTrust = fusionStore.ApplicationTrust;
				if (applicationTrust != null)
				{
					SetupDomainSecurityForApplication(applicationTrust.ApplicationIdentity, applicationTrust);
				}
			}
			Evidence evidence = ((providedSecurityInfo != null) ? providedSecurityInfo : creatorsSecurityInfo);
			if (_domainManager != null)
			{
				HostSecurityManager hostSecurityManager = _domainManager.HostSecurityManager;
				if (hostSecurityManager != null)
				{
					nSetHostSecurityManagerFlags(hostSecurityManager.Flags);
					if ((hostSecurityManager.Flags & HostSecurityManagerOptions.HostAppDomainEvidence) == HostSecurityManagerOptions.HostAppDomainEvidence)
					{
						evidence = hostSecurityManager.ProvideAppDomainEvidence(evidence);
					}
				}
			}
			_SecurityIdentity = evidence;
			nSetupDomainSecurity(evidence, parentSecurityDescriptor, publishAppDomain);
			if (_domainManager != null)
			{
				RunDomainManagerPostInitialization(_domainManager);
			}
		}

		private AppDomainManager CreateDomainManager(string domainManagerAssemblyName, string domainManagerTypeName)
		{
			AppDomainManager appDomainManager = null;
			try
			{
				appDomainManager = CreateInstanceAndUnwrap(domainManagerAssemblyName, domainManagerTypeName) as AppDomainManager;
			}
			catch (FileNotFoundException)
			{
			}
			catch (TypeLoadException)
			{
			}
			finally
			{
				if (appDomainManager == null)
				{
					throw new TypeLoadException(Environment.GetResourceString("Argument_NoDomainManager"));
				}
			}
			return appDomainManager;
		}

		private void RunDomainManagerPostInitialization(AppDomainManager domainManager)
		{
			_ = domainManager.HostExecutionContextManager;
			HostSecurityManager hostSecurityManager = domainManager.HostSecurityManager;
			if (hostSecurityManager != null && (hostSecurityManager.Flags & HostSecurityManagerOptions.HostPolicyLevel) == HostSecurityManagerOptions.HostPolicyLevel)
			{
				PolicyLevel domainPolicy = hostSecurityManager.DomainPolicy;
				if (domainPolicy != null)
				{
					SetAppDomainPolicy(domainPolicy);
				}
			}
		}

		private void SetupApplicationHelper(Evidence providedSecurityInfo, Evidence creatorsSecurityInfo, ApplicationIdentity appIdentity, ActivationContext activationContext, string[] activationData)
		{
			HostSecurityManager hostSecurityManager = CurrentDomain.HostSecurityManager;
			ApplicationTrust applicationTrust = hostSecurityManager.DetermineApplicationTrust(providedSecurityInfo, creatorsSecurityInfo, new TrustManagerContext());
			if (applicationTrust == null || !applicationTrust.IsApplicationTrustedToRun)
			{
				throw new PolicyException(Environment.GetResourceString("Policy_NoExecutionPermission"), -2146233320, null);
			}
			if (activationContext != null)
			{
				SetupDomainForApplication(activationContext, activationData);
			}
			SetupDomainSecurityForApplication(appIdentity, applicationTrust);
		}

		private void SetupDomainForApplication(ActivationContext activationContext, string[] activationData)
		{
			if (IsDefaultAppDomain())
			{
				AppDomainSetup fusionStore = FusionStore;
				fusionStore.ActivationArguments = new ActivationArguments(activationContext, activationData);
				string entryPointFullPath = CmsUtils.GetEntryPointFullPath(activationContext);
				if (!string.IsNullOrEmpty(entryPointFullPath))
				{
					fusionStore.SetupDefaultApplicationBase(entryPointFullPath);
				}
				else
				{
					fusionStore.ApplicationBase = activationContext.ApplicationDirectory;
				}
				SetupFusionStore(fusionStore);
			}
			activationContext.PrepareForExecution();
			activationContext.SetApplicationState(ActivationContext.ApplicationState.Starting);
			activationContext.SetApplicationState(ActivationContext.ApplicationState.Running);
			IPermission permission = null;
			string dataDirectory = activationContext.DataDirectory;
			if (dataDirectory != null && dataDirectory.Length > 0)
			{
				permission = new FileIOPermission(FileIOPermissionAccess.PathDiscovery, dataDirectory);
			}
			SetData("DataDirectory", dataDirectory, permission);
			_activationContext = activationContext;
		}

		private void SetupDomainSecurityForApplication(ApplicationIdentity appIdentity, ApplicationTrust appTrust)
		{
			_applicationIdentity = appIdentity;
			_applicationTrust = appTrust;
			nSetSecurityHomogeneousFlag();
		}

		private int ActivateApplication()
		{
			ObjectHandle objectHandle = Activator.CreateInstance(CurrentDomain.ActivationContext);
			return (int)objectHandle.Unwrap();
		}

		private Assembly ResolveAssemblyForIntrospection(object sender, ResolveEventArgs args)
		{
			return Assembly.ReflectionOnlyLoad(ApplyPolicy(args.Name));
		}

		private void EnableResolveAssembliesForIntrospection()
		{
			AppDomain currentDomain = CurrentDomain;
			currentDomain.ReflectionOnlyAssemblyResolve = (ResolveEventHandler)Delegate.Combine(currentDomain.ReflectionOnlyAssemblyResolve, new ResolveEventHandler(ResolveAssemblyForIntrospection));
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public AssemblyBuilder DefineDynamicAssembly(AssemblyName name, AssemblyBuilderAccess access)
		{
			StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
			return InternalDefineDynamicAssembly(name, access, null, null, null, null, null, ref stackMark, null);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public AssemblyBuilder DefineDynamicAssembly(AssemblyName name, AssemblyBuilderAccess access, IEnumerable<CustomAttributeBuilder> assemblyAttributes)
		{
			StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
			return InternalDefineDynamicAssembly(name, access, null, null, null, null, null, ref stackMark, assemblyAttributes);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public AssemblyBuilder DefineDynamicAssembly(AssemblyName name, AssemblyBuilderAccess access, string dir)
		{
			StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
			return InternalDefineDynamicAssembly(name, access, dir, null, null, null, null, ref stackMark, null);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public AssemblyBuilder DefineDynamicAssembly(AssemblyName name, AssemblyBuilderAccess access, Evidence evidence)
		{
			StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
			return InternalDefineDynamicAssembly(name, access, null, evidence, null, null, null, ref stackMark, null);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public AssemblyBuilder DefineDynamicAssembly(AssemblyName name, AssemblyBuilderAccess access, PermissionSet requiredPermissions, PermissionSet optionalPermissions, PermissionSet refusedPermissions)
		{
			StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
			return InternalDefineDynamicAssembly(name, access, null, null, requiredPermissions, optionalPermissions, refusedPermissions, ref stackMark, null);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public AssemblyBuilder DefineDynamicAssembly(AssemblyName name, AssemblyBuilderAccess access, string dir, Evidence evidence)
		{
			StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
			return InternalDefineDynamicAssembly(name, access, dir, evidence, null, null, null, ref stackMark, null);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public AssemblyBuilder DefineDynamicAssembly(AssemblyName name, AssemblyBuilderAccess access, string dir, PermissionSet requiredPermissions, PermissionSet optionalPermissions, PermissionSet refusedPermissions)
		{
			StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
			return InternalDefineDynamicAssembly(name, access, dir, null, requiredPermissions, optionalPermissions, refusedPermissions, ref stackMark, null);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public AssemblyBuilder DefineDynamicAssembly(AssemblyName name, AssemblyBuilderAccess access, Evidence evidence, PermissionSet requiredPermissions, PermissionSet optionalPermissions, PermissionSet refusedPermissions)
		{
			StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
			return InternalDefineDynamicAssembly(name, access, null, evidence, requiredPermissions, optionalPermissions, refusedPermissions, ref stackMark, null);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public AssemblyBuilder DefineDynamicAssembly(AssemblyName name, AssemblyBuilderAccess access, string dir, Evidence evidence, PermissionSet requiredPermissions, PermissionSet optionalPermissions, PermissionSet refusedPermissions)
		{
			StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
			return InternalDefineDynamicAssembly(name, access, dir, evidence, requiredPermissions, optionalPermissions, refusedPermissions, ref stackMark, null);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public AssemblyBuilder DefineDynamicAssembly(AssemblyName name, AssemblyBuilderAccess access, string dir, Evidence evidence, PermissionSet requiredPermissions, PermissionSet optionalPermissions, PermissionSet refusedPermissions, bool isSynchronized)
		{
			StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
			return InternalDefineDynamicAssembly(name, access, dir, evidence, requiredPermissions, optionalPermissions, refusedPermissions, ref stackMark, null);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public AssemblyBuilder DefineDynamicAssembly(AssemblyName name, AssemblyBuilderAccess access, string dir, Evidence evidence, PermissionSet requiredPermissions, PermissionSet optionalPermissions, PermissionSet refusedPermissions, bool isSynchronized, IEnumerable<CustomAttributeBuilder> assemblyAttributes)
		{
			StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
			return InternalDefineDynamicAssembly(name, access, dir, evidence, requiredPermissions, optionalPermissions, refusedPermissions, ref stackMark, assemblyAttributes);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern string nApplyPolicy(AssemblyName an);

		[ComVisible(false)]
		public string ApplyPolicy(string assemblyName)
		{
			AssemblyName assemblyName2 = new AssemblyName(assemblyName);
			byte[] array = assemblyName2.GetPublicKeyToken();
			if (array == null)
			{
				array = assemblyName2.GetPublicKey();
			}
			if (array == null || array.Length == 0)
			{
				return assemblyName;
			}
			return nApplyPolicy(assemblyName2);
		}

		public ObjectHandle CreateInstance(string assemblyName, string typeName)
		{
			if (this == null)
			{
				throw new NullReferenceException();
			}
			if (assemblyName == null)
			{
				throw new ArgumentNullException("assemblyName");
			}
			return Activator.CreateInstance(assemblyName, typeName);
		}

		internal ObjectHandle InternalCreateInstanceWithNoSecurity(string assemblyName, string typeName)
		{
			PermissionSet.s_fullTrust.Assert();
			return CreateInstance(assemblyName, typeName);
		}

		public ObjectHandle CreateInstanceFrom(string assemblyFile, string typeName)
		{
			if (this == null)
			{
				throw new NullReferenceException();
			}
			return Activator.CreateInstanceFrom(assemblyFile, typeName);
		}

		internal ObjectHandle InternalCreateInstanceFromWithNoSecurity(string assemblyName, string typeName)
		{
			PermissionSet.s_fullTrust.Assert();
			return CreateInstanceFrom(assemblyName, typeName);
		}

		public ObjectHandle CreateComInstanceFrom(string assemblyName, string typeName)
		{
			if (this == null)
			{
				throw new NullReferenceException();
			}
			return Activator.CreateComInstanceFrom(assemblyName, typeName);
		}

		public ObjectHandle CreateComInstanceFrom(string assemblyFile, string typeName, byte[] hashValue, AssemblyHashAlgorithm hashAlgorithm)
		{
			if (this == null)
			{
				throw new NullReferenceException();
			}
			return Activator.CreateComInstanceFrom(assemblyFile, typeName, hashValue, hashAlgorithm);
		}

		public ObjectHandle CreateInstance(string assemblyName, string typeName, object[] activationAttributes)
		{
			if (this == null)
			{
				throw new NullReferenceException();
			}
			if (assemblyName == null)
			{
				throw new ArgumentNullException("assemblyName");
			}
			return Activator.CreateInstance(assemblyName, typeName, activationAttributes);
		}

		public ObjectHandle CreateInstanceFrom(string assemblyFile, string typeName, object[] activationAttributes)
		{
			if (this == null)
			{
				throw new NullReferenceException();
			}
			return Activator.CreateInstanceFrom(assemblyFile, typeName, activationAttributes);
		}

		public ObjectHandle CreateInstance(string assemblyName, string typeName, bool ignoreCase, BindingFlags bindingAttr, Binder binder, object[] args, CultureInfo culture, object[] activationAttributes, Evidence securityAttributes)
		{
			if (this == null)
			{
				throw new NullReferenceException();
			}
			if (assemblyName == null)
			{
				throw new ArgumentNullException("assemblyName");
			}
			return Activator.CreateInstance(assemblyName, typeName, ignoreCase, bindingAttr, binder, args, culture, activationAttributes, securityAttributes);
		}

		internal ObjectHandle InternalCreateInstanceWithNoSecurity(string assemblyName, string typeName, bool ignoreCase, BindingFlags bindingAttr, Binder binder, object[] args, CultureInfo culture, object[] activationAttributes, Evidence securityAttributes)
		{
			PermissionSet.s_fullTrust.Assert();
			return CreateInstance(assemblyName, typeName, ignoreCase, bindingAttr, binder, args, culture, activationAttributes, securityAttributes);
		}

		public ObjectHandle CreateInstanceFrom(string assemblyFile, string typeName, bool ignoreCase, BindingFlags bindingAttr, Binder binder, object[] args, CultureInfo culture, object[] activationAttributes, Evidence securityAttributes)
		{
			if (this == null)
			{
				throw new NullReferenceException();
			}
			return Activator.CreateInstanceFrom(assemblyFile, typeName, ignoreCase, bindingAttr, binder, args, culture, activationAttributes, securityAttributes);
		}

		internal ObjectHandle InternalCreateInstanceFromWithNoSecurity(string assemblyName, string typeName, bool ignoreCase, BindingFlags bindingAttr, Binder binder, object[] args, CultureInfo culture, object[] activationAttributes, Evidence securityAttributes)
		{
			PermissionSet.s_fullTrust.Assert();
			return CreateInstanceFrom(assemblyName, typeName, ignoreCase, bindingAttr, binder, args, culture, activationAttributes, securityAttributes);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public Assembly Load(AssemblyName assemblyRef)
		{
			StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
			return Assembly.InternalLoad(assemblyRef, null, ref stackMark, forIntrospection: false);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public Assembly Load(string assemblyString)
		{
			StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
			return Assembly.InternalLoad(assemblyString, null, ref stackMark, forIntrospection: false);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public Assembly Load(byte[] rawAssembly)
		{
			StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
			return Assembly.nLoadImage(rawAssembly, null, null, ref stackMark, fIntrospection: false);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public Assembly Load(byte[] rawAssembly, byte[] rawSymbolStore)
		{
			StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
			return Assembly.nLoadImage(rawAssembly, rawSymbolStore, null, ref stackMark, fIntrospection: false);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		[SecurityPermission(SecurityAction.Demand, ControlEvidence = true)]
		public Assembly Load(byte[] rawAssembly, byte[] rawSymbolStore, Evidence securityEvidence)
		{
			StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
			return Assembly.nLoadImage(rawAssembly, rawSymbolStore, securityEvidence, ref stackMark, fIntrospection: false);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public Assembly Load(AssemblyName assemblyRef, Evidence assemblySecurity)
		{
			StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
			return Assembly.InternalLoad(assemblyRef, assemblySecurity, ref stackMark, forIntrospection: false);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public Assembly Load(string assemblyString, Evidence assemblySecurity)
		{
			StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
			return Assembly.InternalLoad(assemblyString, assemblySecurity, ref stackMark, forIntrospection: false);
		}

		public int ExecuteAssembly(string assemblyFile)
		{
			return ExecuteAssembly(assemblyFile, null, null);
		}

		public int ExecuteAssembly(string assemblyFile, Evidence assemblySecurity)
		{
			return ExecuteAssembly(assemblyFile, assemblySecurity, null);
		}

		public int ExecuteAssembly(string assemblyFile, Evidence assemblySecurity, string[] args)
		{
			Assembly assembly = Assembly.LoadFrom(assemblyFile, assemblySecurity);
			if (args == null)
			{
				args = new string[0];
			}
			return nExecuteAssembly(assembly, args);
		}

		public int ExecuteAssembly(string assemblyFile, Evidence assemblySecurity, string[] args, byte[] hashValue, AssemblyHashAlgorithm hashAlgorithm)
		{
			Assembly assembly = Assembly.LoadFrom(assemblyFile, assemblySecurity, hashValue, hashAlgorithm);
			if (args == null)
			{
				args = new string[0];
			}
			return nExecuteAssembly(assembly, args);
		}

		public int ExecuteAssemblyByName(string assemblyName)
		{
			return ExecuteAssemblyByName(assemblyName, (Evidence)null, (string[])null);
		}

		public int ExecuteAssemblyByName(string assemblyName, Evidence assemblySecurity)
		{
			return ExecuteAssemblyByName(assemblyName, assemblySecurity, (string[])null);
		}

		public int ExecuteAssemblyByName(string assemblyName, Evidence assemblySecurity, params string[] args)
		{
			Assembly assembly = Assembly.Load(assemblyName, assemblySecurity);
			if (args == null)
			{
				args = new string[0];
			}
			return nExecuteAssembly(assembly, args);
		}

		public int ExecuteAssemblyByName(AssemblyName assemblyName, Evidence assemblySecurity, params string[] args)
		{
			Assembly assembly = Assembly.Load(assemblyName, assemblySecurity);
			if (args == null)
			{
				args = new string[0];
			}
			return nExecuteAssembly(assembly, args);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal extern uint GetAppDomainId();

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			string text = nGetFriendlyName();
			if (text != null)
			{
				stringBuilder.Append(Environment.GetResourceString("Loader_Name") + text);
				stringBuilder.Append(Environment.NewLine);
			}
			if (_Policies == null || _Policies.Length == 0)
			{
				stringBuilder.Append(Environment.GetResourceString("Loader_NoContextPolicies") + Environment.NewLine);
			}
			else
			{
				stringBuilder.Append(Environment.GetResourceString("Loader_ContextPolicies") + Environment.NewLine);
				for (int i = 0; i < _Policies.Length; i++)
				{
					stringBuilder.Append(_Policies[i]);
					stringBuilder.Append(Environment.NewLine);
				}
			}
			return stringBuilder.ToString();
		}

		public Assembly[] GetAssemblies()
		{
			return nGetAssemblies(forIntrospection: false);
		}

		public Assembly[] ReflectionOnlyGetAssemblies()
		{
			return nGetAssemblies(forIntrospection: true);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern Assembly[] nGetAssemblies(bool forIntrospection);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal extern bool IsUnloadingForcedFinalize();

		[MethodImpl(MethodImplOptions.InternalCall)]
		public extern bool IsFinalizingForUnload();

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal extern void PublishAnonymouslyHostedDynamicMethodsAssembly(Assembly assembly);

		[Obsolete("AppDomain.AppendPrivatePath has been deprecated. Please investigate the use of AppDomainSetup.PrivateBinPath instead. http://go.microsoft.com/fwlink/?linkid=14202")]
		[SecurityPermission(SecurityAction.LinkDemand, ControlAppDomain = true)]
		public void AppendPrivatePath(string path)
		{
			if (path == null || path.Length == 0)
			{
				return;
			}
			string text = FusionStore.Value[5];
			StringBuilder stringBuilder = new StringBuilder();
			if (text != null && text.Length > 0)
			{
				stringBuilder.Append(text);
				if (text[text.Length - 1] != Path.PathSeparator && path[0] != Path.PathSeparator)
				{
					stringBuilder.Append(Path.PathSeparator);
				}
			}
			stringBuilder.Append(path);
			string path2 = stringBuilder.ToString();
			InternalSetPrivateBinPath(path2);
		}

		[Obsolete("AppDomain.ClearPrivatePath has been deprecated. Please investigate the use of AppDomainSetup.PrivateBinPath instead. http://go.microsoft.com/fwlink/?linkid=14202")]
		[SecurityPermission(SecurityAction.LinkDemand, ControlAppDomain = true)]
		public void ClearPrivatePath()
		{
			InternalSetPrivateBinPath(string.Empty);
		}

		[Obsolete("AppDomain.ClearShadowCopyPath has been deprecated. Please investigate the use of AppDomainSetup.ShadowCopyDirectories instead. http://go.microsoft.com/fwlink/?linkid=14202")]
		[SecurityPermission(SecurityAction.LinkDemand, ControlAppDomain = true)]
		public void ClearShadowCopyPath()
		{
			InternalSetShadowCopyPath(string.Empty);
		}

		[Obsolete("AppDomain.SetCachePath has been deprecated. Please investigate the use of AppDomainSetup.CachePath instead. http://go.microsoft.com/fwlink/?linkid=14202")]
		[SecurityPermission(SecurityAction.LinkDemand, ControlAppDomain = true)]
		public void SetCachePath(string path)
		{
			InternalSetCachePath(path);
		}

		[SecurityPermission(SecurityAction.LinkDemand, ControlAppDomain = true)]
		public void SetData(string name, object data)
		{
			SetDataHelper(name, data, null);
		}

		[SecurityPermission(SecurityAction.LinkDemand, ControlAppDomain = true)]
		public void SetData(string name, object data, IPermission permission)
		{
			SetDataHelper(name, data, permission);
		}

		private void SetDataHelper(string name, object data, IPermission permission)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			if (name.Equals("IgnoreSystemPolicy"))
			{
				lock (this)
				{
					if (!_HasSetPolicy)
					{
						throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_SetData"));
					}
				}
				new PermissionSet(PermissionState.Unrestricted).Demand();
			}
			int num = AppDomainSetup.Locate(name);
			if (num == -1)
			{
				LocalStore[name] = new object[2]
				{
					data,
					permission
				};
				return;
			}
			if (permission != null)
			{
				throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_SetData"));
			}
			switch (num)
			{
			case 2:
				FusionStore.DynamicBase = (string)data;
				break;
			case 3:
				FusionStore.DeveloperPath = (string)data;
				break;
			case 7:
				FusionStore.ShadowCopyDirectories = (string)data;
				break;
			case 11:
				if (data != null)
				{
					FusionStore.DisallowPublisherPolicy = true;
				}
				else
				{
					FusionStore.DisallowPublisherPolicy = false;
				}
				break;
			case 12:
				if (data != null)
				{
					FusionStore.DisallowCodeDownload = true;
				}
				else
				{
					FusionStore.DisallowCodeDownload = false;
				}
				break;
			case 13:
				if (data != null)
				{
					FusionStore.DisallowBindingRedirects = true;
				}
				else
				{
					FusionStore.DisallowBindingRedirects = false;
				}
				break;
			case 14:
				if (data != null)
				{
					FusionStore.DisallowApplicationBaseProbing = true;
				}
				else
				{
					FusionStore.DisallowApplicationBaseProbing = false;
				}
				break;
			case 15:
				FusionStore.SetConfigurationBytes((byte[])data);
				break;
			default:
				FusionStore.Value[num] = (string)data;
				break;
			}
		}

		public object GetData(string name)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			switch (AppDomainSetup.Locate(name))
			{
			case -1:
			{
				if (name.Equals(AppDomainSetup.LoaderOptimizationKey))
				{
					return FusionStore.LoaderOptimization;
				}
				object[] array = (object[])LocalStore[name];
				if (array == null)
				{
					return null;
				}
				if (array[1] != null)
				{
					IPermission permission = (IPermission)array[1];
					permission.Demand();
				}
				return array[0];
			}
			case 0:
				return FusionStore.ApplicationBase;
			case 1:
				return FusionStore.ConfigurationFile;
			case 2:
				return FusionStore.DynamicBase;
			case 3:
				return FusionStore.DeveloperPath;
			case 4:
				return FusionStore.ApplicationName;
			case 5:
				return FusionStore.PrivateBinPath;
			case 6:
				return FusionStore.PrivateBinPathProbe;
			case 7:
				return FusionStore.ShadowCopyDirectories;
			case 8:
				return FusionStore.ShadowCopyFiles;
			case 9:
				return FusionStore.CachePath;
			case 10:
				return FusionStore.LicenseFile;
			case 11:
				return FusionStore.DisallowPublisherPolicy;
			case 12:
				return FusionStore.DisallowCodeDownload;
			case 13:
				return FusionStore.DisallowBindingRedirects;
			case 14:
				return FusionStore.DisallowApplicationBaseProbing;
			case 15:
				return FusionStore.GetConfigurationBytes();
			default:
				return null;
			}
		}

		[DllImport("kernel32.dll")]
		[Obsolete("AppDomain.GetCurrentThreadId has been deprecated because it does not provide a stable Id when managed threads are running on fibers (aka lightweight threads). To get a stable identifier for a managed thread, use the ManagedThreadId property on Thread.  http://go.microsoft.com/fwlink/?linkid=14202", false)]
		public static extern int GetCurrentThreadId();

		[ReliabilityContract(Consistency.MayCorruptAppDomain, Cer.MayFail)]
		[SecurityPermission(SecurityAction.Demand, ControlAppDomain = true)]
		public static void Unload(AppDomain domain)
		{
			if (domain == null)
			{
				throw new ArgumentNullException("domain");
			}
			try
			{
				int idForUnload = GetIdForUnload(domain);
				if (idForUnload == 0)
				{
					throw new CannotUnloadAppDomainException();
				}
				nUnload(idForUnload);
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		[SecurityPermission(SecurityAction.LinkDemand, ControlDomainPolicy = true)]
		public void SetAppDomainPolicy(PolicyLevel domainPolicy)
		{
			if (domainPolicy == null)
			{
				throw new ArgumentNullException("domainPolicy");
			}
			lock (this)
			{
				if (_HasSetPolicy)
				{
					throw new PolicyException(Environment.GetResourceString("Policy_PolicyAlreadySet"));
				}
				_HasSetPolicy = true;
				nChangeSecurityPolicy();
			}
			SecurityManager.PolicyManager.AddLevel(domainPolicy);
		}

		[SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.ControlPrincipal)]
		public void SetThreadPrincipal(IPrincipal principal)
		{
			if (principal == null)
			{
				throw new ArgumentNullException("principal");
			}
			lock (this)
			{
				if (_DefaultPrincipal != null)
				{
					throw new PolicyException(Environment.GetResourceString("Policy_PrincipalTwice"));
				}
				_DefaultPrincipal = principal;
			}
		}

		[SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.ControlPrincipal)]
		public void SetPrincipalPolicy(PrincipalPolicy policy)
		{
			_PrincipalPolicy = policy;
		}

		public override object InitializeLifetimeService()
		{
			return null;
		}

		public void DoCallBack(CrossAppDomainDelegate callBackDelegate)
		{
			if (callBackDelegate == null)
			{
				throw new ArgumentNullException("callBackDelegate");
			}
			callBackDelegate();
		}

		public static AppDomain CreateDomain(string friendlyName, Evidence securityInfo)
		{
			return CreateDomain(friendlyName, securityInfo, null);
		}

		public static AppDomain CreateDomain(string friendlyName, Evidence securityInfo, string appBasePath, string appRelativeSearchPath, bool shadowCopyFiles)
		{
			AppDomainSetup appDomainSetup = new AppDomainSetup();
			appDomainSetup.ApplicationBase = appBasePath;
			appDomainSetup.PrivateBinPath = appRelativeSearchPath;
			if (shadowCopyFiles)
			{
				appDomainSetup.ShadowCopyFiles = "true";
			}
			return CreateDomain(friendlyName, securityInfo, appDomainSetup);
		}

		public static AppDomain CreateDomain(string friendlyName)
		{
			return CreateDomain(friendlyName, null, null);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern string GetDynamicDir();

		private static byte[] MarshalObject(object o)
		{
			CodeAccessPermission.AssertAllPossible();
			return Serialize(o);
		}

		private static byte[] MarshalObjects(object o1, object o2, out byte[] blob2)
		{
			CodeAccessPermission.AssertAllPossible();
			byte[] result = Serialize(o1);
			blob2 = Serialize(o2);
			return result;
		}

		private static object UnmarshalObject(byte[] blob)
		{
			CodeAccessPermission.AssertAllPossible();
			return Deserialize(blob);
		}

		private static object UnmarshalObjects(byte[] blob1, byte[] blob2, out object o2)
		{
			CodeAccessPermission.AssertAllPossible();
			object result = Deserialize(blob1);
			o2 = Deserialize(blob2);
			return result;
		}

		private static byte[] Serialize(object o)
		{
			if (o == null)
			{
				return null;
			}
			if (o is ISecurityEncodable)
			{
				SecurityElement securityElement = ((ISecurityEncodable)o).ToXml();
				MemoryStream memoryStream = new MemoryStream(4096);
				memoryStream.WriteByte(0);
				StreamWriter streamWriter = new StreamWriter(memoryStream, Encoding.UTF8);
				securityElement.ToWriter(streamWriter);
				streamWriter.Flush();
				return memoryStream.ToArray();
			}
			MemoryStream memoryStream2 = new MemoryStream();
			memoryStream2.WriteByte(1);
			CrossAppDomainSerializer.SerializeObject(o, memoryStream2);
			return memoryStream2.ToArray();
		}

		private static object Deserialize(byte[] blob)
		{
			if (blob == null)
			{
				return null;
			}
			if (blob[0] == 0)
			{
				Parser parser = new Parser(blob, Tokenizer.ByteTokenEncoding.UTF8Tokens, 1);
				SecurityElement topElement = parser.GetTopElement();
				if (topElement.Tag.Equals("IPermission") || topElement.Tag.Equals("Permission"))
				{
					IPermission permission = XMLUtil.CreatePermission(topElement, PermissionState.None, ignoreTypeLoadFailures: false);
					if (permission == null)
					{
						return null;
					}
					permission.FromXml(topElement);
					return permission;
				}
				if (topElement.Tag.Equals("PermissionSet"))
				{
					PermissionSet permissionSet = new PermissionSet();
					permissionSet.FromXml(topElement, allowInternalOnly: false, ignoreTypeLoadFailures: false);
					return permissionSet;
				}
				if (topElement.Tag.Equals("PermissionToken"))
				{
					PermissionToken permissionToken = new PermissionToken();
					permissionToken.FromXml(topElement);
					return permissionToken;
				}
				return null;
			}
			object obj = null;
			using MemoryStream stm = new MemoryStream(blob, 1, blob.Length - 1);
			return CrossAppDomainSerializer.DeserializeObject(stm);
		}

		private AppDomain()
		{
			throw new NotSupportedException(Environment.GetResourceString("NotSupported_Constructor"));
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern Assembly nCreateDynamicAssembly(AssemblyName name, Evidence identity, ref StackCrawlMark stackMark, PermissionSet requiredPermissions, PermissionSet optionalPermissions, PermissionSet refusedPermissions, AssemblyBuilderAccess access, DynamicAssemblyFlags flags);

		internal AssemblyBuilder InternalDefineDynamicAssembly(AssemblyName name, AssemblyBuilderAccess access, string dir, Evidence evidence, PermissionSet requiredPermissions, PermissionSet optionalPermissions, PermissionSet refusedPermissions, ref StackCrawlMark stackMark, IEnumerable<CustomAttributeBuilder> unsafeAssemblyAttributes)
		{
			lock (typeof(AssemblyBuilderLock))
			{
				if (name == null)
				{
					throw new ArgumentNullException("name");
				}
				if (access != AssemblyBuilderAccess.Run && access != AssemblyBuilderAccess.Save && access != AssemblyBuilderAccess.RunAndSave && access != AssemblyBuilderAccess.ReflectionOnly)
				{
					throw new ArgumentException(Environment.GetResourceString("Arg_EnumIllegalVal", (int)access), "access");
				}
				if (name.KeyPair != null)
				{
					name.SetPublicKey(name.KeyPair.PublicKey);
				}
				if (evidence != null)
				{
					new SecurityPermission(SecurityPermissionFlag.ControlEvidence).Demand();
				}
				List<CustomAttributeBuilder> list = null;
				DynamicAssemblyFlags dynamicAssemblyFlags = DynamicAssemblyFlags.None;
				if (unsafeAssemblyAttributes != null)
				{
					list = new List<CustomAttributeBuilder>(unsafeAssemblyAttributes);
					foreach (CustomAttributeBuilder item in list)
					{
						if (item.m_con.DeclaringType == typeof(SecurityTransparentAttribute))
						{
							dynamicAssemblyFlags |= DynamicAssemblyFlags.Transparent;
						}
					}
				}
				AssemblyBuilder assemblyBuilder = new AssemblyBuilder((AssemblyBuilder)nCreateDynamicAssembly(name, evidence, ref stackMark, requiredPermissions, optionalPermissions, refusedPermissions, access, dynamicAssemblyFlags));
				assemblyBuilder.m_assemblyData = new AssemblyBuilderData(assemblyBuilder, name.Name, access, dir);
				assemblyBuilder.m_assemblyData.AddPermissionRequests(requiredPermissions, optionalPermissions, refusedPermissions);
				if (list != null)
				{
					foreach (CustomAttributeBuilder item2 in list)
					{
						assemblyBuilder.SetCustomAttribute(item2);
					}
				}
				assemblyBuilder.m_assemblyData.GetInMemoryAssemblyModule();
				return assemblyBuilder;
			}
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern int _nExecuteAssembly(Assembly assembly, string[] args);

		internal int nExecuteAssembly(Assembly assembly, string[] args)
		{
			return _nExecuteAssembly(assembly.InternalAssembly, args);
		}

		internal void CreateRemotingData()
		{
			lock (this)
			{
				if (_RemotingData == null)
				{
					_RemotingData = new DomainSpecificRemotingData();
				}
			}
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern string nGetFriendlyName();

		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern bool nIsDefaultAppDomainForSecurity();

		private void OnAssemblyLoadEvent(Assembly LoadedAssembly)
		{
			AssemblyLoadEventHandler assemblyLoad = this.AssemblyLoad;
			if (assemblyLoad != null)
			{
				AssemblyLoadEventArgs args = new AssemblyLoadEventArgs(LoadedAssembly);
				assemblyLoad(this, args);
			}
		}

		private Assembly OnResourceResolveEvent(string resourceName)
		{
			ResolveEventHandler resourceResolve = this.ResourceResolve;
			if (resourceResolve == null)
			{
				return null;
			}
			Delegate[] invocationList = resourceResolve.GetInvocationList();
			int num = invocationList.Length;
			for (int i = 0; i < num; i++)
			{
				Assembly assembly = ((ResolveEventHandler)invocationList[i])(this, new ResolveEventArgs(resourceName));
				if (assembly != null)
				{
					return assembly.InternalAssembly;
				}
			}
			return null;
		}

		private Assembly OnTypeResolveEvent(string typeName)
		{
			ResolveEventHandler typeResolve = this.TypeResolve;
			if (typeResolve == null)
			{
				return null;
			}
			Delegate[] invocationList = typeResolve.GetInvocationList();
			int num = invocationList.Length;
			for (int i = 0; i < num; i++)
			{
				Assembly assembly = ((ResolveEventHandler)invocationList[i])(this, new ResolveEventArgs(typeName));
				if (assembly != null)
				{
					return assembly.InternalAssembly;
				}
			}
			return null;
		}

		private Assembly OnAssemblyResolveEvent(string assemblyFullName)
		{
			ResolveEventHandler assemblyResolve = this.AssemblyResolve;
			if (assemblyResolve == null)
			{
				return null;
			}
			Delegate[] invocationList = assemblyResolve.GetInvocationList();
			int num = invocationList.Length;
			for (int i = 0; i < num; i++)
			{
				Assembly assembly = ((ResolveEventHandler)invocationList[i])(this, new ResolveEventArgs(assemblyFullName));
				if (assembly != null)
				{
					return assembly.InternalAssembly;
				}
			}
			return null;
		}

		private Assembly OnReflectionOnlyAssemblyResolveEvent(string assemblyFullName)
		{
			ResolveEventHandler reflectionOnlyAssemblyResolve = this.ReflectionOnlyAssemblyResolve;
			if (reflectionOnlyAssemblyResolve != null)
			{
				Delegate[] invocationList = reflectionOnlyAssemblyResolve.GetInvocationList();
				int num = invocationList.Length;
				for (int i = 0; i < num; i++)
				{
					Assembly assembly = ((ResolveEventHandler)invocationList[i])(this, new ResolveEventArgs(assemblyFullName));
					if (assembly != null)
					{
						return assembly.InternalAssembly;
					}
				}
			}
			return null;
		}

		private void TurnOnBindingRedirects()
		{
			_FusionStore.DisallowBindingRedirects = false;
		}

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		internal static int GetIdForUnload(AppDomain domain)
		{
			if (RemotingServices.IsTransparentProxy(domain))
			{
				return RemotingServices.GetServerDomainIdForProxy(domain);
			}
			return domain.Id;
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		internal static extern bool IsDomainIdValid(int id);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern AppDomain GetDefaultDomain();

		internal IPrincipal GetThreadPrincipal()
		{
			IPrincipal principal = null;
			lock (this)
			{
				if (_DefaultPrincipal == null)
				{
					return _PrincipalPolicy switch
					{
						PrincipalPolicy.NoPrincipal => null, 
						PrincipalPolicy.UnauthenticatedPrincipal => new GenericPrincipal(new GenericIdentity("", ""), new string[1]
						{
							""
						}), 
						PrincipalPolicy.WindowsPrincipal => new WindowsPrincipal(WindowsIdentity.GetCurrent()), 
						_ => null, 
					};
				}
				return _DefaultPrincipal;
			}
		}

		internal void CreateDefaultContext()
		{
			lock (this)
			{
				if (_DefaultContext == null)
				{
					_DefaultContext = Context.CreateDefaultContext();
				}
			}
		}

		internal Context GetDefaultContext()
		{
			if (_DefaultContext == null)
			{
				CreateDefaultContext();
			}
			return _DefaultContext;
		}

		[SecurityPermission(SecurityAction.Demand, ControlAppDomain = true)]
		public static AppDomain CreateDomain(string friendlyName, Evidence securityInfo, AppDomainSetup info)
		{
			AppDomainManager domainManager = CurrentDomain.DomainManager;
			if (domainManager != null)
			{
				return domainManager.CreateDomain(friendlyName, securityInfo, info);
			}
			if (friendlyName == null)
			{
				throw new ArgumentNullException(Environment.GetResourceString("ArgumentNull_String"));
			}
			if (securityInfo != null)
			{
				new SecurityPermission(SecurityPermissionFlag.ControlEvidence).Demand();
			}
			return nCreateDomain(friendlyName, info, securityInfo, (securityInfo == null) ? CurrentDomain.InternalEvidence : null, CurrentDomain.GetSecurityDescriptor());
		}

		public static AppDomain CreateDomain(string friendlyName, Evidence securityInfo, AppDomainSetup info, PermissionSet grantSet, params StrongName[] fullTrustAssemblies)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			if (info.ApplicationBase == null)
			{
				throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_AppDomainSandboxAPINeedsExplicitAppBase"));
			}
			info.ApplicationTrust = new ApplicationTrust(grantSet, fullTrustAssemblies);
			return CreateDomain(friendlyName, securityInfo, info);
		}

		public static AppDomain CreateDomain(string friendlyName, Evidence securityInfo, string appBasePath, string appRelativeSearchPath, bool shadowCopyFiles, AppDomainInitializer adInit, string[] adInitArgs)
		{
			AppDomainSetup appDomainSetup = new AppDomainSetup();
			appDomainSetup.ApplicationBase = appBasePath;
			appDomainSetup.PrivateBinPath = appRelativeSearchPath;
			appDomainSetup.AppDomainInitializer = adInit;
			appDomainSetup.AppDomainInitializerArguments = adInitArgs;
			if (shadowCopyFiles)
			{
				appDomainSetup.ShadowCopyFiles = "true";
			}
			return CreateDomain(friendlyName, securityInfo, appDomainSetup);
		}

		private void SetupFusionStore(AppDomainSetup info)
		{
			if (info.Value[0] == null || info.Value[1] == null)
			{
				AppDomain defaultDomain = GetDefaultDomain();
				if (this == defaultDomain)
				{
					info.SetupDefaultApplicationBase(RuntimeEnvironment.GetModuleFileName());
				}
				else
				{
					if (info.Value[1] == null)
					{
						info.ConfigurationFile = defaultDomain.FusionStore.Value[1];
					}
					if (info.Value[0] == null)
					{
						info.ApplicationBase = defaultDomain.FusionStore.Value[0];
					}
					if (info.Value[4] == null)
					{
						info.ApplicationName = defaultDomain.FusionStore.Value[4];
					}
				}
			}
			if (info.Value[5] == null)
			{
				info.PrivateBinPath = Environment.nativeGetEnvironmentVariable(AppDomainSetup.PrivateBinPathEnvironmentVariable);
			}
			if (info.DeveloperPath == null)
			{
				info.DeveloperPath = RuntimeEnvironment.GetDeveloperPath();
			}
			IntPtr fusionContext = GetFusionContext();
			info.SetupFusionContext(fusionContext);
			if (info.LoaderOptimization != 0)
			{
				UpdateLoaderOptimization((int)info.LoaderOptimization);
			}
			_FusionStore = info;
		}

		private static void RunInitializer(AppDomainSetup setup)
		{
			if (setup.AppDomainInitializer != null)
			{
				string[] args = null;
				if (setup.AppDomainInitializerArguments != null)
				{
					args = (string[])setup.AppDomainInitializerArguments.Clone();
				}
				setup.AppDomainInitializer(args);
			}
		}

		private static object RemotelySetupRemoteDomain(AppDomain appDomainProxy, string friendlyName, AppDomainSetup setup, Evidence providedSecurityInfo, Evidence creatorsSecurityInfo, IntPtr parentSecurityDescriptor)
		{
			RemotingServices.GetServerContextAndDomainIdForProxy(appDomainProxy, out var contextId, out var domainId);
			if (contextId == IntPtr.Zero)
			{
				throw new AppDomainUnloadedException();
			}
			EvidenceCollection evidenceCollection = null;
			if (providedSecurityInfo != null || creatorsSecurityInfo != null)
			{
				evidenceCollection = new EvidenceCollection();
				evidenceCollection.ProvidedSecurityInfo = providedSecurityInfo;
				evidenceCollection.CreatorsSecurityInfo = creatorsSecurityInfo;
			}
			bool flag = false;
			char[] array = null;
			char[] array2 = null;
			byte[] serializedEvidence = null;
			AppDomainInitializerInfo initializerInfo = null;
			if (providedSecurityInfo != null)
			{
				array = PolicyManager.MakeEvidenceArray(providedSecurityInfo, verbose: true);
				if (array == null)
				{
					flag = true;
				}
			}
			if (creatorsSecurityInfo != null && !flag)
			{
				array2 = PolicyManager.MakeEvidenceArray(creatorsSecurityInfo, verbose: true);
				if (array2 == null)
				{
					flag = true;
				}
			}
			if (evidenceCollection != null && flag)
			{
				array = (array2 = null);
				serializedEvidence = CrossAppDomainSerializer.SerializeObject(evidenceCollection).GetBuffer();
			}
			if (setup != null && setup.AppDomainInitializer != null)
			{
				initializerInfo = new AppDomainInitializerInfo(setup.AppDomainInitializer);
			}
			return InternalRemotelySetupRemoteDomain(contextId, domainId, friendlyName, setup, parentSecurityDescriptor, array, array2, serializedEvidence, initializerInfo);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		private static object InternalRemotelySetupRemoteDomainHelper(object[] args)
		{
			string friendlyName = (string)args[0];
			AppDomainSetup appDomainSetup = (AppDomainSetup)args[1];
			IntPtr parentSecurityDescriptor = (IntPtr)args[2];
			char[] array = (char[])args[3];
			char[] array2 = (char[])args[4];
			byte[] array3 = (byte[])args[5];
			AppDomainInitializerInfo appDomainInitializerInfo = (AppDomainInitializerInfo)args[6];
			AppDomain appDomain = Thread.CurrentContext.AppDomain;
			AppDomainSetup appDomainSetup2 = new AppDomainSetup(appDomainSetup, copyDomainBoundData: false);
			appDomain.SetupFusionStore(appDomainSetup2);
			Evidence providedSecurityInfo = null;
			Evidence creatorsSecurityInfo = null;
			if (array3 == null)
			{
				if (array != null)
				{
					providedSecurityInfo = new Evidence(array);
				}
				if (array2 != null)
				{
					creatorsSecurityInfo = new Evidence(array2);
				}
			}
			else
			{
				EvidenceCollection evidenceCollection = (EvidenceCollection)CrossAppDomainSerializer.DeserializeObject(new MemoryStream(array3));
				providedSecurityInfo = evidenceCollection.ProvidedSecurityInfo;
				creatorsSecurityInfo = evidenceCollection.CreatorsSecurityInfo;
			}
			appDomain.nSetupFriendlyName(friendlyName);
			if (appDomainSetup != null && appDomainSetup.SandboxInterop)
			{
				appDomain.nSetDisableInterfaceCache();
			}
			appDomain.SetDomainManager(providedSecurityInfo, creatorsSecurityInfo, parentSecurityDescriptor, publishAppDomain: true);
			if (appDomainInitializerInfo != null)
			{
				appDomainSetup2.AppDomainInitializer = appDomainInitializerInfo.Unwrap();
			}
			RunInitializer(appDomainSetup2);
			ObjectHandle obj = null;
			AppDomainSetup fusionStore = appDomain.FusionStore;
			if (fusionStore.ActivationArguments != null && fusionStore.ActivationArguments.ActivateInstance)
			{
				obj = Activator.CreateInstance(appDomain.ActivationContext);
			}
			return RemotingServices.MarshalInternal(obj, null, null);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		private static object InternalRemotelySetupRemoteDomain(IntPtr contextId, int domainId, string friendlyName, AppDomainSetup setup, IntPtr parentSecurityDescriptor, char[] serProvidedEvidence, char[] serCreatorEvidence, byte[] serializedEvidence, AppDomainInitializerInfo initializerInfo)
		{
			InternalCrossContextDelegate ftnToCall = InternalRemotelySetupRemoteDomainHelper;
			object[] args = new object[7]
			{
				friendlyName,
				setup,
				parentSecurityDescriptor,
				serProvidedEvidence,
				serCreatorEvidence,
				serializedEvidence,
				initializerInfo
			};
			return Thread.CurrentThread.InternalCrossContextCallback(null, contextId, domainId, ftnToCall, args);
		}

		private void SetupDomain(bool allowRedirects, string path, string configFile)
		{
			lock (this)
			{
				if (_FusionStore == null)
				{
					AppDomainSetup appDomainSetup = new AppDomainSetup();
					if (path != null)
					{
						appDomainSetup.Value[0] = path;
					}
					if (configFile != null)
					{
						appDomainSetup.Value[1] = configFile;
					}
					if (!allowRedirects)
					{
						appDomainSetup.DisallowBindingRedirects = true;
					}
					SetupFusionStore(appDomainSetup);
				}
			}
		}

		private void SetupLoaderOptimization(LoaderOptimization policy)
		{
			if (policy != 0)
			{
				FusionStore.LoaderOptimization = policy;
				UpdateLoaderOptimization((int)FusionStore.LoaderOptimization);
			}
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal extern IntPtr GetFusionContext();

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal extern IntPtr GetSecurityDescriptor();

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern AppDomain nCreateDomain(string friendlyName, AppDomainSetup setup, Evidence providedSecurityInfo, Evidence creatorsSecurityInfo, IntPtr parentSecurityDescriptor);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern ObjRef nCreateInstance(string friendlyName, AppDomainSetup setup, Evidence providedSecurityInfo, Evidence creatorsSecurityInfo, IntPtr parentSecurityDescriptor);

		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern void nSetupDomainSecurity(Evidence appDomainEvidence, IntPtr creatorsSecurityDescriptor, bool publishAppDomain);

		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern void nSetupFriendlyName(string friendlyName);

		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern void nSetDisableInterfaceCache();

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal extern void UpdateLoaderOptimization(int optimization);

		[Obsolete("AppDomain.SetShadowCopyPath has been deprecated. Please investigate the use of AppDomainSetup.ShadowCopyDirectories instead. http://go.microsoft.com/fwlink/?linkid=14202")]
		[SecurityPermission(SecurityAction.LinkDemand, ControlAppDomain = true)]
		public void SetShadowCopyPath(string path)
		{
			InternalSetShadowCopyPath(path);
		}

		[Obsolete("AppDomain.SetShadowCopyFiles has been deprecated. Please investigate the use of AppDomainSetup.ShadowCopyFiles instead. http://go.microsoft.com/fwlink/?linkid=14202")]
		[SecurityPermission(SecurityAction.LinkDemand, ControlAppDomain = true)]
		public void SetShadowCopyFiles()
		{
			InternalSetShadowCopyFiles();
		}

		[Obsolete("AppDomain.SetDynamicBase has been deprecated. Please investigate the use of AppDomainSetup.DynamicBase instead. http://go.microsoft.com/fwlink/?linkid=14202")]
		[SecurityPermission(SecurityAction.LinkDemand, ControlAppDomain = true)]
		public void SetDynamicBase(string path)
		{
			InternalSetDynamicBase(path);
		}

		internal void InternalSetShadowCopyPath(string path)
		{
			IntPtr fusionContext = GetFusionContext();
			AppDomainSetup.UpdateContextProperty(fusionContext, AppDomainSetup.ShadowCopyDirectoriesKey, path);
			FusionStore.ShadowCopyDirectories = path;
		}

		internal void InternalSetShadowCopyFiles()
		{
			IntPtr fusionContext = GetFusionContext();
			AppDomainSetup.UpdateContextProperty(fusionContext, AppDomainSetup.ShadowCopyFilesKey, "true");
			FusionStore.ShadowCopyFiles = "true";
		}

		internal void InternalSetCachePath(string path)
		{
			IntPtr fusionContext = GetFusionContext();
			FusionStore.CachePath = path;
			AppDomainSetup.UpdateContextProperty(fusionContext, AppDomainSetup.CachePathKey, FusionStore.Value[9]);
		}

		internal void InternalSetPrivateBinPath(string path)
		{
			IntPtr fusionContext = GetFusionContext();
			AppDomainSetup.UpdateContextProperty(fusionContext, AppDomainSetup.PrivateBinPathKey, path);
			FusionStore.PrivateBinPath = path;
		}

		internal void InternalSetDynamicBase(string path)
		{
			IntPtr fusionContext = GetFusionContext();
			FusionStore.DynamicBase = path;
			AppDomainSetup.UpdateContextProperty(fusionContext, AppDomainSetup.DynamicBaseKey, FusionStore.Value[2]);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal extern string IsStringInterned(string str);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal extern string GetOrInternString(string str);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal extern void nGetGrantSet(out PermissionSet granted, out PermissionSet denied);

		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern void nChangeSecurityPolicy();

		[MethodImpl(MethodImplOptions.InternalCall)]
		[ReliabilityContract(Consistency.MayCorruptAppDomain, Cer.MayFail)]
		internal static extern void nUnload(int domainInternal);

		public object CreateInstanceAndUnwrap(string assemblyName, string typeName)
		{
			return CreateInstance(assemblyName, typeName)?.Unwrap();
		}

		public object CreateInstanceAndUnwrap(string assemblyName, string typeName, object[] activationAttributes)
		{
			return CreateInstance(assemblyName, typeName, activationAttributes)?.Unwrap();
		}

		public object CreateInstanceAndUnwrap(string assemblyName, string typeName, bool ignoreCase, BindingFlags bindingAttr, Binder binder, object[] args, CultureInfo culture, object[] activationAttributes, Evidence securityAttributes)
		{
			return CreateInstance(assemblyName, typeName, ignoreCase, bindingAttr, binder, args, culture, activationAttributes, securityAttributes)?.Unwrap();
		}

		public object CreateInstanceFromAndUnwrap(string assemblyName, string typeName)
		{
			return CreateInstanceFrom(assemblyName, typeName)?.Unwrap();
		}

		public object CreateInstanceFromAndUnwrap(string assemblyName, string typeName, object[] activationAttributes)
		{
			return CreateInstanceFrom(assemblyName, typeName, activationAttributes)?.Unwrap();
		}

		public object CreateInstanceFromAndUnwrap(string assemblyName, string typeName, bool ignoreCase, BindingFlags bindingAttr, Binder binder, object[] args, CultureInfo culture, object[] activationAttributes, Evidence securityAttributes)
		{
			return CreateInstanceFrom(assemblyName, typeName, ignoreCase, bindingAttr, binder, args, culture, activationAttributes, securityAttributes)?.Unwrap();
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		internal extern int GetId();

		public bool IsDefaultAppDomain()
		{
			if (this == GetDefaultDomain())
			{
				return true;
			}
			return false;
		}

		private static AppDomainSetup InternalCreateDomainSetup(string imageLocation)
		{
			int num = imageLocation.LastIndexOf('\\');
			AppDomainSetup appDomainSetup = new AppDomainSetup();
			appDomainSetup.ApplicationBase = imageLocation.Substring(0, num + 1);
			StringBuilder stringBuilder = new StringBuilder(imageLocation.Substring(num + 1));
			stringBuilder.Append(AppDomainSetup.ConfigurationExtension);
			appDomainSetup.ConfigurationFile = stringBuilder.ToString();
			return appDomainSetup;
		}

		private static AppDomain InternalCreateDomain(string imageLocation)
		{
			AppDomainSetup info = InternalCreateDomainSetup(imageLocation);
			return CreateDomain("Validator", null, info);
		}

		private void InternalSetDomainContext(string imageLocation)
		{
			SetupFusionStore(InternalCreateDomainSetup(imageLocation));
		}

		void _AppDomain.GetTypeInfoCount(out uint pcTInfo)
		{
			throw new NotImplementedException();
		}

		void _AppDomain.GetTypeInfo(uint iTInfo, uint lcid, IntPtr ppTInfo)
		{
			throw new NotImplementedException();
		}

		void _AppDomain.GetIDsOfNames([In] ref Guid riid, IntPtr rgszNames, uint cNames, uint lcid, IntPtr rgDispId)
		{
			throw new NotImplementedException();
		}

		void _AppDomain.Invoke(uint dispIdMember, [In] ref Guid riid, uint lcid, short wFlags, IntPtr pDispParams, IntPtr pVarResult, IntPtr pExcepInfo, IntPtr puArgErr)
		{
			throw new NotImplementedException();
		}
	}
}
