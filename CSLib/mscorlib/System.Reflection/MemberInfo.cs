using System.Reflection.Cache;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Threading;

namespace System.Reflection
{
	[Serializable]
	[ClassInterface(ClassInterfaceType.None)]
	[ComDefaultInterface(typeof(_MemberInfo))]
	[ComVisible(true)]
	[PermissionSet(SecurityAction.InheritanceDemand, Name = "FullTrust")]
	public abstract class MemberInfo : ICustomAttributeProvider, _MemberInfo
	{
		internal const uint INVOCATION_FLAGS_UNKNOWN = 0u;

		internal const uint INVOCATION_FLAGS_INITIALIZED = 1u;

		internal const uint INVOCATION_FLAGS_NO_INVOKE = 2u;

		internal const uint INVOCATION_FLAGS_NEED_SECURITY = 4u;

		internal const uint INVOCATION_FLAGS_NO_CTOR_INVOKE = 8u;

		internal const uint INVOCATION_FLAGS_IS_CTOR = 16u;

		internal const uint INVOCATION_FLAGS_RISKY_METHOD = 32u;

		internal const uint INVOCATION_FLAGS_SECURITY_IMPOSED = 64u;

		internal const uint INVOCATION_FLAGS_IS_DELEGATE_CTOR = 128u;

		internal const uint INVOCATION_FLAGS_CONTAINS_STACK_POINTERS = 256u;

		internal const uint INVOCATION_FLAGS_SPECIAL_FIELD = 16u;

		internal const uint INVOCATION_FLAGS_FIELD_SPECIAL_CAST = 32u;

		internal const uint INVOCATION_FLAGS_CONSTRUCTOR_INVOKE = 268435456u;

		private InternalCache m_cachedData;

		internal InternalCache Cache
		{
			get
			{
				InternalCache internalCache = m_cachedData;
				if (internalCache == null)
				{
					internalCache = new InternalCache("MemberInfo");
					InternalCache internalCache2 = Interlocked.CompareExchange(ref m_cachedData, internalCache, null);
					if (internalCache2 != null)
					{
						internalCache = internalCache2;
					}
					GC.ClearCache += OnCacheClear;
				}
				return internalCache;
			}
		}

		public abstract MemberTypes MemberType
		{
			get;
		}

		public abstract string Name
		{
			get;
		}

		public abstract Type DeclaringType
		{
			get;
		}

		public abstract Type ReflectedType
		{
			get;
		}

		public virtual int MetadataToken
		{
			get
			{
				throw new InvalidOperationException();
			}
		}

		internal virtual int MetadataTokenInternal => MetadataToken;

		public virtual Module Module
		{
			get
			{
				if (this is Type)
				{
					return ((Type)this).Module;
				}
				throw new NotImplementedException();
			}
		}

		internal virtual bool CacheEquals(object o)
		{
			throw new NotImplementedException();
		}

		internal void OnCacheClear(object sender, ClearCacheEventArgs cacheEventArgs)
		{
			m_cachedData = null;
		}

		public abstract object[] GetCustomAttributes(bool inherit);

		public abstract object[] GetCustomAttributes(Type attributeType, bool inherit);

		public abstract bool IsDefined(Type attributeType, bool inherit);

		Type _MemberInfo.GetType()
		{
			return GetType();
		}

		void _MemberInfo.GetTypeInfoCount(out uint pcTInfo)
		{
			throw new NotImplementedException();
		}

		void _MemberInfo.GetTypeInfo(uint iTInfo, uint lcid, IntPtr ppTInfo)
		{
			throw new NotImplementedException();
		}

		void _MemberInfo.GetIDsOfNames([In] ref Guid riid, IntPtr rgszNames, uint cNames, uint lcid, IntPtr rgDispId)
		{
			throw new NotImplementedException();
		}

		void _MemberInfo.Invoke(uint dispIdMember, [In] ref Guid riid, uint lcid, short wFlags, IntPtr pDispParams, IntPtr pVarResult, IntPtr pExcepInfo, IntPtr puArgErr)
		{
			throw new NotImplementedException();
		}
	}
}
