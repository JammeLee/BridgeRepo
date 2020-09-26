using System.Diagnostics;
using System.Reflection;
using System.Reflection.Cache;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Metadata;
using System.Security.Permissions;

namespace System.Runtime.Remoting
{
	[ComVisible(true)]
	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
	public class InternalRemotingServices
	{
		[Conditional("_LOGGING")]
		public static void DebugOutChnl(string s)
		{
			Message.OutToUnmanagedDebugger("CHNL:" + s + "\n");
		}

		[Conditional("_LOGGING")]
		public static void RemotingTrace(params object[] messages)
		{
		}

		[Conditional("_DEBUG")]
		public static void RemotingAssert(bool condition, string message)
		{
		}

		[CLSCompliant(false)]
		public static void SetServerIdentity(MethodCall m, object srvID)
		{
			((IInternalMessage)m).ServerIdentityObject = (ServerIdentity)srvID;
		}

		internal static RemotingMethodCachedData GetReflectionCachedData(MethodBase mi)
		{
			RemotingMethodCachedData remotingMethodCachedData = null;
			remotingMethodCachedData = (RemotingMethodCachedData)mi.Cache[CacheObjType.RemotingData];
			if (remotingMethodCachedData == null)
			{
				remotingMethodCachedData = new RemotingMethodCachedData(mi);
				mi.Cache[CacheObjType.RemotingData] = remotingMethodCachedData;
			}
			return remotingMethodCachedData;
		}

		internal static RemotingTypeCachedData GetReflectionCachedData(Type mi)
		{
			RemotingTypeCachedData remotingTypeCachedData = null;
			remotingTypeCachedData = (RemotingTypeCachedData)mi.Cache[CacheObjType.RemotingData];
			if (remotingTypeCachedData == null)
			{
				remotingTypeCachedData = new RemotingTypeCachedData(mi);
				mi.Cache[CacheObjType.RemotingData] = remotingTypeCachedData;
			}
			return remotingTypeCachedData;
		}

		internal static RemotingCachedData GetReflectionCachedData(MemberInfo mi)
		{
			RemotingCachedData remotingCachedData = null;
			remotingCachedData = (RemotingCachedData)mi.Cache[CacheObjType.RemotingData];
			if (remotingCachedData == null)
			{
				remotingCachedData = ((mi is MethodBase) ? new RemotingMethodCachedData(mi) : ((!(mi is Type)) ? new RemotingCachedData(mi) : new RemotingTypeCachedData(mi)));
				mi.Cache[CacheObjType.RemotingData] = remotingCachedData;
			}
			return remotingCachedData;
		}

		internal static RemotingCachedData GetReflectionCachedData(ParameterInfo reflectionObject)
		{
			RemotingCachedData remotingCachedData = null;
			remotingCachedData = (RemotingCachedData)reflectionObject.Cache[CacheObjType.RemotingData];
			if (remotingCachedData == null)
			{
				remotingCachedData = new RemotingCachedData(reflectionObject);
				reflectionObject.Cache[CacheObjType.RemotingData] = remotingCachedData;
			}
			return remotingCachedData;
		}

		public static SoapAttribute GetCachedSoapAttribute(object reflectionObject)
		{
			MemberInfo memberInfo = reflectionObject as MemberInfo;
			if (memberInfo != null)
			{
				return GetReflectionCachedData(memberInfo).GetSoapAttribute();
			}
			return GetReflectionCachedData((ParameterInfo)reflectionObject).GetSoapAttribute();
		}
	}
}
