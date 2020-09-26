using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Security.Principal;
using System.Threading;

namespace System.Runtime.Remoting.Messaging
{
	[Serializable]
	[ComVisible(true)]
	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
	public sealed class CallContext
	{
		internal static CallContextSecurityData SecurityData => Thread.CurrentThread.GetLogicalCallContext().SecurityData;

		internal static CallContextRemotingData RemotingData => Thread.CurrentThread.GetLogicalCallContext().RemotingData;

		internal static IPrincipal Principal
		{
			get
			{
				LogicalCallContext logicalCallContext = GetLogicalCallContext();
				return logicalCallContext.Principal;
			}
			set
			{
				LogicalCallContext logicalCallContext = GetLogicalCallContext();
				logicalCallContext.Principal = value;
			}
		}

		public static object HostContext
		{
			get
			{
				IllogicalCallContext illogicalCallContext = Thread.CurrentThread.GetIllogicalCallContext();
				object hostContext = illogicalCallContext.HostContext;
				if (hostContext == null)
				{
					LogicalCallContext logicalCallContext = GetLogicalCallContext();
					hostContext = logicalCallContext.HostContext;
				}
				return hostContext;
			}
			[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
			set
			{
				if (value is ILogicalThreadAffinative)
				{
					IllogicalCallContext illogicalCallContext = Thread.CurrentThread.GetIllogicalCallContext();
					illogicalCallContext.HostContext = null;
					LogicalCallContext logicalCallContext = GetLogicalCallContext();
					logicalCallContext.HostContext = value;
				}
				else
				{
					LogicalCallContext logicalCallContext2 = GetLogicalCallContext();
					logicalCallContext2.HostContext = null;
					IllogicalCallContext illogicalCallContext2 = Thread.CurrentThread.GetIllogicalCallContext();
					illogicalCallContext2.HostContext = value;
				}
			}
		}

		private CallContext()
		{
		}

		internal static LogicalCallContext GetLogicalCallContext()
		{
			return Thread.CurrentThread.GetLogicalCallContext();
		}

		internal static LogicalCallContext SetLogicalCallContext(LogicalCallContext callCtx)
		{
			return Thread.CurrentThread.SetLogicalCallContext(callCtx);
		}

		internal static LogicalCallContext SetLogicalCallContext(Thread currThread, LogicalCallContext callCtx)
		{
			return currThread.SetLogicalCallContext(callCtx);
		}

		public static void FreeNamedDataSlot(string name)
		{
			Thread.CurrentThread.GetLogicalCallContext().FreeNamedDataSlot(name);
			Thread.CurrentThread.GetIllogicalCallContext().FreeNamedDataSlot(name);
		}

		public static object LogicalGetData(string name)
		{
			LogicalCallContext logicalCallContext = Thread.CurrentThread.GetLogicalCallContext();
			return logicalCallContext.GetData(name);
		}

		private static object IllogicalGetData(string name)
		{
			IllogicalCallContext illogicalCallContext = Thread.CurrentThread.GetIllogicalCallContext();
			return illogicalCallContext.GetData(name);
		}

		public static object GetData(string name)
		{
			object obj = LogicalGetData(name);
			if (obj == null)
			{
				return IllogicalGetData(name);
			}
			return obj;
		}

		public static void SetData(string name, object data)
		{
			if (data is ILogicalThreadAffinative)
			{
				LogicalSetData(name, data);
				return;
			}
			LogicalCallContext logicalCallContext = Thread.CurrentThread.GetLogicalCallContext();
			logicalCallContext.FreeNamedDataSlot(name);
			IllogicalCallContext illogicalCallContext = Thread.CurrentThread.GetIllogicalCallContext();
			illogicalCallContext.SetData(name, data);
		}

		public static void LogicalSetData(string name, object data)
		{
			IllogicalCallContext illogicalCallContext = Thread.CurrentThread.GetIllogicalCallContext();
			illogicalCallContext.FreeNamedDataSlot(name);
			LogicalCallContext logicalCallContext = Thread.CurrentThread.GetLogicalCallContext();
			logicalCallContext.SetData(name, data);
		}

		public static Header[] GetHeaders()
		{
			LogicalCallContext logicalCallContext = Thread.CurrentThread.GetLogicalCallContext();
			return logicalCallContext.InternalGetHeaders();
		}

		public static void SetHeaders(Header[] headers)
		{
			LogicalCallContext logicalCallContext = Thread.CurrentThread.GetLogicalCallContext();
			logicalCallContext.InternalSetHeaders(headers);
		}
	}
}
