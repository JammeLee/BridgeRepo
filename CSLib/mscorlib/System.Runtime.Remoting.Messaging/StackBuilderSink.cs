using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Metadata;
using System.Security.Principal;

namespace System.Runtime.Remoting.Messaging
{
	[Serializable]
	internal class StackBuilderSink : IMessageSink
	{
		private object _server;

		private static string sIRemoteDispatch = "System.EnterpriseServices.IRemoteDispatch";

		private static string sIRemoteDispatchAssembly = "System.EnterpriseServices";

		private bool _bStatic;

		public IMessageSink NextSink => null;

		internal object ServerObject => _server;

		public StackBuilderSink(MarshalByRefObject server)
		{
			_server = server;
		}

		public StackBuilderSink(object server)
		{
			_server = server;
			if (_server == null)
			{
				_bStatic = true;
			}
		}

		public virtual IMessage SyncProcessMessage(IMessage msg)
		{
			return SyncProcessMessage(msg, 0, fExecuteInContext: false);
		}

		internal virtual IMessage SyncProcessMessage(IMessage msg, int methodPtr, bool fExecuteInContext)
		{
			IMessage message = InternalSink.ValidateMessage(msg);
			if (message != null)
			{
				return message;
			}
			IMethodCallMessage methodCallMessage = msg as IMethodCallMessage;
			LogicalCallContext logicalCallContext = null;
			LogicalCallContext logicalCallContext2 = CallContext.GetLogicalCallContext();
			object data = logicalCallContext2.GetData("__xADCall");
			bool flag = false;
			try
			{
				object server = _server;
				VerifyIsOkToCallMethod(server, methodCallMessage);
				LogicalCallContext logicalCallContext3 = null;
				logicalCallContext3 = ((methodCallMessage == null) ? ((LogicalCallContext)msg.Properties["__CallContext"]) : methodCallMessage.LogicalCallContext);
				logicalCallContext = CallContext.SetLogicalCallContext(logicalCallContext3);
				flag = true;
				logicalCallContext3.PropagateIncomingHeadersToCallContext(msg);
				PreserveThreadPrincipalIfNecessary(logicalCallContext3, logicalCallContext);
				IMessage message2;
				if (IsOKToStackBlt(methodCallMessage, server) && ((Message)methodCallMessage).Dispatch(server, fExecuteInContext))
				{
					message2 = new StackBasedReturnMessage();
					((StackBasedReturnMessage)message2).InitFields((Message)methodCallMessage);
					LogicalCallContext logicalCallContext4 = CallContext.GetLogicalCallContext();
					logicalCallContext4.PropagateOutgoingHeadersToMessage(message2);
					((StackBasedReturnMessage)message2).SetLogicalCallContext(logicalCallContext4);
					return message2;
				}
				MethodBase methodBase = GetMethodBase(methodCallMessage);
				object[] outArgs = null;
				object obj = null;
				RemotingMethodCachedData reflectionCachedData = InternalRemotingServices.GetReflectionCachedData(methodBase);
				object[] args = Message.CoerceArgs(methodCallMessage, reflectionCachedData.Parameters);
				obj = PrivateProcessMessage(methodBase.MethodHandle, args, server, methodPtr, fExecuteInContext, out outArgs);
				CopyNonByrefOutArgsFromOriginalArgs(reflectionCachedData, args, ref outArgs);
				LogicalCallContext logicalCallContext5 = CallContext.GetLogicalCallContext();
				if (data != null && (bool)data)
				{
					logicalCallContext5?.RemovePrincipalIfNotSerializable();
				}
				message2 = new ReturnMessage(obj, outArgs, (outArgs != null) ? outArgs.Length : 0, logicalCallContext5, methodCallMessage);
				logicalCallContext5.PropagateOutgoingHeadersToMessage(message2);
				CallContext.SetLogicalCallContext(logicalCallContext);
				return message2;
			}
			catch (Exception e)
			{
				IMessage message2 = new ReturnMessage(e, methodCallMessage);
				((ReturnMessage)message2).SetLogicalCallContext(methodCallMessage.LogicalCallContext);
				if (flag)
				{
					CallContext.SetLogicalCallContext(logicalCallContext);
					return message2;
				}
				return message2;
			}
		}

		public virtual IMessageCtrl AsyncProcessMessage(IMessage msg, IMessageSink replySink)
		{
			IMethodCallMessage methodCallMessage = (IMethodCallMessage)msg;
			IMessageCtrl result = null;
			IMessage message = null;
			LogicalCallContext logicalCallContext = null;
			bool flag = false;
			try
			{
				try
				{
					LogicalCallContext logicalCallContext2 = (LogicalCallContext)methodCallMessage.Properties[Message.CallContextKey];
					object server = _server;
					VerifyIsOkToCallMethod(server, methodCallMessage);
					logicalCallContext = CallContext.SetLogicalCallContext(logicalCallContext2);
					flag = true;
					logicalCallContext2.PropagateIncomingHeadersToCallContext(msg);
					PreserveThreadPrincipalIfNecessary(logicalCallContext2, logicalCallContext);
					ServerChannelSinkStack serverChannelSinkStack = msg.Properties["__SinkStack"] as ServerChannelSinkStack;
					if (serverChannelSinkStack != null)
					{
						serverChannelSinkStack.ServerObject = server;
					}
					MethodBase methodBase = GetMethodBase(methodCallMessage);
					object[] outArgs = null;
					object obj = null;
					RemotingMethodCachedData reflectionCachedData = InternalRemotingServices.GetReflectionCachedData(methodBase);
					object[] args = Message.CoerceArgs(methodCallMessage, reflectionCachedData.Parameters);
					obj = PrivateProcessMessage(methodBase.MethodHandle, args, server, 0, fExecuteInContext: false, out outArgs);
					CopyNonByrefOutArgsFromOriginalArgs(reflectionCachedData, args, ref outArgs);
					if (replySink != null)
					{
						LogicalCallContext logicalCallContext3 = CallContext.GetLogicalCallContext();
						logicalCallContext3?.RemovePrincipalIfNotSerializable();
						message = new ReturnMessage(obj, outArgs, (outArgs != null) ? outArgs.Length : 0, logicalCallContext3, methodCallMessage);
						logicalCallContext3.PropagateOutgoingHeadersToMessage(message);
						return result;
					}
					return result;
				}
				catch (Exception e)
				{
					if (replySink != null)
					{
						message = new ReturnMessage(e, methodCallMessage);
						((ReturnMessage)message).SetLogicalCallContext((LogicalCallContext)methodCallMessage.Properties[Message.CallContextKey]);
						return result;
					}
					return result;
				}
				finally
				{
					replySink?.SyncProcessMessage(message);
				}
			}
			finally
			{
				if (flag)
				{
					CallContext.SetLogicalCallContext(logicalCallContext);
				}
			}
		}

		internal bool IsOKToStackBlt(IMethodMessage mcMsg, object server)
		{
			bool result = false;
			Message message = mcMsg as Message;
			if (message != null)
			{
				IInternalMessage internalMessage = message;
				if (message.GetFramePtr() != IntPtr.Zero && message.GetThisPtr() == server && (internalMessage.IdentityObject == null || (internalMessage.IdentityObject != null && internalMessage.IdentityObject == internalMessage.ServerIdentityObject)))
				{
					result = true;
				}
			}
			return result;
		}

		private static MethodBase GetMethodBase(IMethodMessage msg)
		{
			MethodBase methodBase = msg.MethodBase;
			if (methodBase == null)
			{
				throw new RemotingException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Remoting_Message_MethodMissing"), msg.MethodName, msg.TypeName));
			}
			return methodBase;
		}

		private static void VerifyIsOkToCallMethod(object server, IMethodMessage msg)
		{
			bool flag = false;
			MarshalByRefObject marshalByRefObject = server as MarshalByRefObject;
			if (marshalByRefObject == null)
			{
				return;
			}
			bool fServer;
			Identity identity = MarshalByRefObject.GetIdentity(marshalByRefObject, out fServer);
			if (identity != null)
			{
				ServerIdentity serverIdentity = identity as ServerIdentity;
				if (serverIdentity != null && serverIdentity.MarshaledAsSpecificType)
				{
					Type serverType = serverIdentity.ServerType;
					if (serverType != null)
					{
						MethodBase methodBase = GetMethodBase(msg);
						Type declaringType = methodBase.DeclaringType;
						if (declaringType != serverType && !declaringType.IsAssignableFrom(serverType))
						{
							throw new RemotingException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Remoting_InvalidCallingType"), methodBase.DeclaringType.FullName, serverType.FullName));
						}
						if (declaringType.IsInterface)
						{
							VerifyNotIRemoteDispatch(declaringType);
						}
						flag = true;
					}
				}
			}
			if (flag)
			{
				return;
			}
			MethodBase methodBase2 = GetMethodBase(msg);
			Type reflectedType = methodBase2.ReflectedType;
			if (!reflectedType.IsInterface)
			{
				if (!reflectedType.IsInstanceOfType(marshalByRefObject))
				{
					throw new RemotingException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Remoting_InvalidCallingType"), reflectedType.FullName, marshalByRefObject.GetType().FullName));
				}
			}
			else
			{
				VerifyNotIRemoteDispatch(reflectedType);
			}
		}

		private static void VerifyNotIRemoteDispatch(Type reflectedType)
		{
			if (reflectedType.FullName.Equals(sIRemoteDispatch) && reflectedType.Module.Assembly.nGetSimpleName().Equals(sIRemoteDispatchAssembly))
			{
				throw new RemotingException(Environment.GetResourceString("Remoting_CantInvokeIRemoteDispatch"));
			}
		}

		internal void CopyNonByrefOutArgsFromOriginalArgs(RemotingMethodCachedData methodCache, object[] args, ref object[] marshalResponseArgs)
		{
			int[] nonRefOutArgMap = methodCache.NonRefOutArgMap;
			if (nonRefOutArgMap.Length > 0)
			{
				if (marshalResponseArgs == null)
				{
					marshalResponseArgs = new object[methodCache.Parameters.Length];
				}
				int[] array = nonRefOutArgMap;
				foreach (int num in array)
				{
					marshalResponseArgs[num] = args[num];
				}
			}
		}

		internal static void PreserveThreadPrincipalIfNecessary(LogicalCallContext messageCallContext, LogicalCallContext threadCallContext)
		{
			if (threadCallContext != null && messageCallContext.Principal == null)
			{
				IPrincipal principal = threadCallContext.Principal;
				if (principal != null)
				{
					messageCallContext.Principal = principal;
				}
			}
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern object _PrivateProcessMessage(IntPtr md, object[] args, object server, int methodPtr, bool fExecuteInContext, out object[] outArgs);

		public object PrivateProcessMessage(RuntimeMethodHandle md, object[] args, object server, int methodPtr, bool fExecuteInContext, out object[] outArgs)
		{
			return _PrivateProcessMessage(md.Value, args, server, methodPtr, fExecuteInContext, out outArgs);
		}
	}
}
