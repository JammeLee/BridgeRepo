using System.Configuration.Assemblies;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Activation;
using System.Security.Permissions;
using System.Security.Policy;
using System.Threading;

namespace System
{
	[ComVisible(true)]
	[ComDefaultInterface(typeof(_Activator))]
	[ClassInterface(ClassInterfaceType.None)]
	public sealed class Activator : _Activator
	{
		internal const int LookupMask = 255;

		internal const BindingFlags ConLookup = BindingFlags.Instance | BindingFlags.Public;

		internal const BindingFlags ConstructorDefault = BindingFlags.Instance | BindingFlags.Public | BindingFlags.CreateInstance;

		private Activator()
		{
		}

		public static object CreateInstance(Type type, BindingFlags bindingAttr, Binder binder, object[] args, CultureInfo culture)
		{
			return CreateInstance(type, bindingAttr, binder, args, culture, null);
		}

		public static object CreateInstance(Type type, BindingFlags bindingAttr, Binder binder, object[] args, CultureInfo culture, object[] activationAttributes)
		{
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}
			if (type is TypeBuilder)
			{
				throw new NotSupportedException(Environment.GetResourceString("NotSupported_CreateInstanceWithTypeBuilder"));
			}
			if ((bindingAttr & (BindingFlags)255) == 0)
			{
				bindingAttr |= BindingFlags.Instance | BindingFlags.Public | BindingFlags.CreateInstance;
			}
			if (activationAttributes != null && activationAttributes.Length > 0)
			{
				if (!type.IsMarshalByRef)
				{
					throw new NotSupportedException(Environment.GetResourceString("NotSupported_ActivAttrOnNonMBR"));
				}
				if (!type.IsContextful && (activationAttributes.Length > 1 || !(activationAttributes[0] is UrlAttribute)))
				{
					throw new NotSupportedException(Environment.GetResourceString("NotSupported_NonUrlAttrOnMBR"));
				}
			}
			RuntimeType runtimeType = type.UnderlyingSystemType as RuntimeType;
			if (runtimeType == null)
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_MustBeType"), "type");
			}
			return runtimeType.CreateInstanceImpl(bindingAttr, binder, args, culture, activationAttributes);
		}

		public static object CreateInstance(Type type, params object[] args)
		{
			return CreateInstance(type, BindingFlags.Instance | BindingFlags.Public | BindingFlags.CreateInstance, null, args, null, null);
		}

		public static object CreateInstance(Type type, object[] args, object[] activationAttributes)
		{
			return CreateInstance(type, BindingFlags.Instance | BindingFlags.Public | BindingFlags.CreateInstance, null, args, null, activationAttributes);
		}

		public static object CreateInstance(Type type)
		{
			return CreateInstance(type, nonPublic: false);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public static ObjectHandle CreateInstance(string assemblyName, string typeName)
		{
			StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
			return CreateInstance(assemblyName, typeName, ignoreCase: false, BindingFlags.Instance | BindingFlags.Public | BindingFlags.CreateInstance, null, null, null, null, null, ref stackMark);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public static ObjectHandle CreateInstance(string assemblyName, string typeName, object[] activationAttributes)
		{
			StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
			return CreateInstance(assemblyName, typeName, ignoreCase: false, BindingFlags.Instance | BindingFlags.Public | BindingFlags.CreateInstance, null, null, null, activationAttributes, null, ref stackMark);
		}

		public static object CreateInstance(Type type, bool nonPublic)
		{
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}
			RuntimeType runtimeType = type.UnderlyingSystemType as RuntimeType;
			if (runtimeType == null)
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_MustBeType"), "type");
			}
			return runtimeType.CreateInstanceImpl(!nonPublic);
		}

		internal static object InternalCreateInstanceWithNoMemberAccessCheck(Type type, bool nonPublic)
		{
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}
			RuntimeType runtimeType = type.UnderlyingSystemType as RuntimeType;
			if (runtimeType == null)
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_MustBeType"), "type");
			}
			return runtimeType.CreateInstanceImpl(!nonPublic, skipVisibilityChecks: false, fillCache: false);
		}

		public static T CreateInstance<T>()
		{
			bool bNeedSecurityCheck = true;
			bool canBeCached = false;
			RuntimeMethodHandle ctor = RuntimeMethodHandle.EmptyHandle;
			return (T)RuntimeTypeHandle.CreateInstance(typeof(T) as RuntimeType, publicOnly: true, noCheck: true, ref canBeCached, ref ctor, ref bNeedSecurityCheck);
		}

		public static ObjectHandle CreateInstanceFrom(string assemblyFile, string typeName)
		{
			return CreateInstanceFrom(assemblyFile, typeName, null);
		}

		public static ObjectHandle CreateInstanceFrom(string assemblyFile, string typeName, object[] activationAttributes)
		{
			return CreateInstanceFrom(assemblyFile, typeName, ignoreCase: false, BindingFlags.Instance | BindingFlags.Public | BindingFlags.CreateInstance, null, null, null, activationAttributes, null);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public static ObjectHandle CreateInstance(string assemblyName, string typeName, bool ignoreCase, BindingFlags bindingAttr, Binder binder, object[] args, CultureInfo culture, object[] activationAttributes, Evidence securityInfo)
		{
			StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
			return CreateInstance(assemblyName, typeName, ignoreCase, bindingAttr, binder, args, culture, activationAttributes, securityInfo, ref stackMark);
		}

		internal static ObjectHandle CreateInstance(string assemblyName, string typeName, bool ignoreCase, BindingFlags bindingAttr, Binder binder, object[] args, CultureInfo culture, object[] activationAttributes, Evidence securityInfo, ref StackCrawlMark stackMark)
		{
			Assembly assembly = ((assemblyName != null) ? Assembly.InternalLoad(assemblyName, securityInfo, ref stackMark, forIntrospection: false) : Assembly.nGetExecutingAssembly(ref stackMark));
			if (assembly == null)
			{
				return null;
			}
			Type type = assembly.GetType(typeName, throwOnError: true, ignoreCase);
			object obj = CreateInstance(type, bindingAttr, binder, args, culture, activationAttributes);
			if (obj == null)
			{
				return null;
			}
			return new ObjectHandle(obj);
		}

		public static ObjectHandle CreateInstanceFrom(string assemblyFile, string typeName, bool ignoreCase, BindingFlags bindingAttr, Binder binder, object[] args, CultureInfo culture, object[] activationAttributes, Evidence securityInfo)
		{
			Assembly assembly = Assembly.LoadFrom(assemblyFile, securityInfo);
			Type type = assembly.GetType(typeName, throwOnError: true, ignoreCase);
			object obj = CreateInstance(type, bindingAttr, binder, args, culture, activationAttributes);
			if (obj == null)
			{
				return null;
			}
			return new ObjectHandle(obj);
		}

		[PermissionSet(SecurityAction.LinkDemand, Unrestricted = true)]
		public static ObjectHandle CreateInstance(AppDomain domain, string assemblyName, string typeName)
		{
			if (domain == null)
			{
				throw new ArgumentNullException("domain");
			}
			return domain.InternalCreateInstanceWithNoSecurity(assemblyName, typeName);
		}

		[PermissionSet(SecurityAction.LinkDemand, Unrestricted = true)]
		public static ObjectHandle CreateInstance(AppDomain domain, string assemblyName, string typeName, bool ignoreCase, BindingFlags bindingAttr, Binder binder, object[] args, CultureInfo culture, object[] activationAttributes, Evidence securityAttributes)
		{
			if (domain == null)
			{
				throw new ArgumentNullException("domain");
			}
			return domain.InternalCreateInstanceWithNoSecurity(assemblyName, typeName, ignoreCase, bindingAttr, binder, args, culture, activationAttributes, securityAttributes);
		}

		[PermissionSet(SecurityAction.LinkDemand, Unrestricted = true)]
		public static ObjectHandle CreateInstanceFrom(AppDomain domain, string assemblyFile, string typeName)
		{
			if (domain == null)
			{
				throw new ArgumentNullException("domain");
			}
			return domain.InternalCreateInstanceFromWithNoSecurity(assemblyFile, typeName);
		}

		[PermissionSet(SecurityAction.LinkDemand, Unrestricted = true)]
		public static ObjectHandle CreateInstanceFrom(AppDomain domain, string assemblyFile, string typeName, bool ignoreCase, BindingFlags bindingAttr, Binder binder, object[] args, CultureInfo culture, object[] activationAttributes, Evidence securityAttributes)
		{
			if (domain == null)
			{
				throw new ArgumentNullException("domain");
			}
			return domain.InternalCreateInstanceFromWithNoSecurity(assemblyFile, typeName, ignoreCase, bindingAttr, binder, args, culture, activationAttributes, securityAttributes);
		}

		public static ObjectHandle CreateInstance(ActivationContext activationContext)
		{
			AppDomainManager appDomainManager = AppDomain.CurrentDomain.DomainManager;
			if (appDomainManager == null)
			{
				appDomainManager = new AppDomainManager();
			}
			return appDomainManager.ApplicationActivator.CreateInstance(activationContext);
		}

		public static ObjectHandle CreateInstance(ActivationContext activationContext, string[] activationCustomData)
		{
			AppDomainManager appDomainManager = AppDomain.CurrentDomain.DomainManager;
			if (appDomainManager == null)
			{
				appDomainManager = new AppDomainManager();
			}
			return appDomainManager.ApplicationActivator.CreateInstance(activationContext, activationCustomData);
		}

		public static ObjectHandle CreateComInstanceFrom(string assemblyName, string typeName)
		{
			return CreateComInstanceFrom(assemblyName, typeName, null, AssemblyHashAlgorithm.None);
		}

		public static ObjectHandle CreateComInstanceFrom(string assemblyName, string typeName, byte[] hashValue, AssemblyHashAlgorithm hashAlgorithm)
		{
			Assembly assembly = Assembly.LoadFrom(assemblyName, null, hashValue, hashAlgorithm);
			Type type = assembly.GetType(typeName, throwOnError: true, ignoreCase: false);
			object[] customAttributes = type.GetCustomAttributes(typeof(ComVisibleAttribute), inherit: false);
			if (customAttributes.Length > 0 && !((ComVisibleAttribute)customAttributes[0]).Value)
			{
				throw new TypeLoadException(Environment.GetResourceString("Argument_TypeMustBeVisibleFromCom"));
			}
			if (assembly == null)
			{
				return null;
			}
			object obj = CreateInstance(type, BindingFlags.Instance | BindingFlags.Public | BindingFlags.CreateInstance, null, null, null, null);
			if (obj == null)
			{
				return null;
			}
			return new ObjectHandle(obj);
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.RemotingConfiguration)]
		public static object GetObject(Type type, string url)
		{
			return GetObject(type, url, null);
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.RemotingConfiguration)]
		public static object GetObject(Type type, string url, object state)
		{
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}
			return RemotingServices.Connect(type, url, state);
		}

		[Conditional("_DEBUG")]
		private static void Log(bool test, string title, string success, string failure)
		{
			if (!test)
			{
			}
		}

		void _Activator.GetTypeInfoCount(out uint pcTInfo)
		{
			throw new NotImplementedException();
		}

		void _Activator.GetTypeInfo(uint iTInfo, uint lcid, IntPtr ppTInfo)
		{
			throw new NotImplementedException();
		}

		void _Activator.GetIDsOfNames([In] ref Guid riid, IntPtr rgszNames, uint cNames, uint lcid, IntPtr rgDispId)
		{
			throw new NotImplementedException();
		}

		void _Activator.Invoke(uint dispIdMember, [In] ref Guid riid, uint lcid, short wFlags, IntPtr pDispParams, IntPtr pVarResult, IntPtr pExcepInfo, IntPtr puArgErr)
		{
			throw new NotImplementedException();
		}
	}
}
