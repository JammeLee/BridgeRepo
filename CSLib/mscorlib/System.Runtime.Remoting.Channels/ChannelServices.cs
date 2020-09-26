using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;
using System.Security.Permissions;
using System.Threading;

namespace System.Runtime.Remoting.Channels
{
	[ComVisible(true)]
	public sealed class ChannelServices
	{
		private static object[] s_currentChannelData = null;

		private static object s_channelLock = new object();

		private static RegisteredChannelList s_registeredChannels = new RegisteredChannelList();

		private static IMessageSink xCtxChannel;

		private unsafe static Perf_Contexts* perf_Contexts = GetPrivateContextsPerfCounters();

		private static bool unloadHandlerRegistered = false;

		internal static object[] CurrentChannelData
		{
			get
			{
				if (s_currentChannelData == null)
				{
					RefreshChannelData();
				}
				return s_currentChannelData;
			}
		}

		private static long remoteCalls
		{
			get
			{
				return Thread.GetDomain().RemotingData.ChannelServicesData.remoteCalls;
			}
			set
			{
				Thread.GetDomain().RemotingData.ChannelServicesData.remoteCalls = value;
			}
		}

		public static IChannel[] RegisteredChannels
		{
			[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
			get
			{
				RegisteredChannelList registeredChannelList = s_registeredChannels;
				int count = registeredChannelList.Count;
				if (count == 0)
				{
					return new IChannel[0];
				}
				int num = count - 1;
				int num2 = 0;
				IChannel[] array = new IChannel[num];
				for (int i = 0; i < count; i++)
				{
					IChannel channel = registeredChannelList.GetChannel(i);
					if (!(channel is CrossAppDomainChannel))
					{
						array[num2++] = channel;
					}
				}
				return array;
			}
		}

		private ChannelServices()
		{
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private unsafe static extern Perf_Contexts* GetPrivateContextsPerfCounters();

		[SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.RemotingConfiguration)]
		public static void RegisterChannel(IChannel chnl, bool ensureSecurity)
		{
			RegisterChannelInternal(chnl, ensureSecurity);
		}

		[Obsolete("Use System.Runtime.Remoting.ChannelServices.RegisterChannel(IChannel chnl, bool ensureSecurity) instead.", false)]
		[SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.RemotingConfiguration)]
		public static void RegisterChannel(IChannel chnl)
		{
			RegisterChannelInternal(chnl, ensureSecurity: false);
		}

		internal unsafe static void RegisterChannelInternal(IChannel chnl, bool ensureSecurity)
		{
			if (chnl == null)
			{
				throw new ArgumentNullException("chnl");
			}
			bool tookLock = false;
			RuntimeHelpers.PrepareConstrainedRegions();
			try
			{
				Monitor.ReliableEnter(s_channelLock, ref tookLock);
				string channelName = chnl.ChannelName;
				RegisteredChannelList registeredChannelList = s_registeredChannels;
				if (channelName == null || channelName.Length == 0 || -1 == registeredChannelList.FindChannelIndex(chnl.ChannelName))
				{
					if (ensureSecurity)
					{
						ISecurableChannel securableChannel = chnl as ISecurableChannel;
						if (securableChannel == null)
						{
							throw new RemotingException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Remoting_Channel_CannotBeSecured"), chnl.ChannelName ?? chnl.ToString()));
						}
						securableChannel.IsSecured = ensureSecurity;
					}
					RegisteredChannel[] registeredChannels = registeredChannelList.RegisteredChannels;
					RegisteredChannel[] array = null;
					array = ((registeredChannels != null) ? new RegisteredChannel[registeredChannels.Length + 1] : new RegisteredChannel[1]);
					if (!unloadHandlerRegistered && !(chnl is CrossAppDomainChannel))
					{
						AppDomain.CurrentDomain.DomainUnload += UnloadHandler;
						unloadHandlerRegistered = true;
					}
					int channelPriority = chnl.ChannelPriority;
					int i;
					for (i = 0; i < registeredChannels.Length; i++)
					{
						RegisteredChannel registeredChannel = registeredChannels[i];
						if (channelPriority > registeredChannel.Channel.ChannelPriority)
						{
							array[i] = new RegisteredChannel(chnl);
							break;
						}
						array[i] = registeredChannel;
					}
					if (i == registeredChannels.Length)
					{
						array[registeredChannels.Length] = new RegisteredChannel(chnl);
					}
					else
					{
						for (; i < registeredChannels.Length; i++)
						{
							array[i + 1] = registeredChannels[i];
						}
					}
					if (perf_Contexts != null)
					{
						perf_Contexts->cChannels++;
					}
					s_registeredChannels = new RegisteredChannelList(array);
					RefreshChannelData();
					return;
				}
				throw new RemotingException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Remoting_ChannelNameAlreadyRegistered"), chnl.ChannelName));
			}
			finally
			{
				if (tookLock)
				{
					Monitor.Exit(s_channelLock);
				}
			}
		}

		[SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.RemotingConfiguration)]
		public unsafe static void UnregisterChannel(IChannel chnl)
		{
			bool tookLock = false;
			RuntimeHelpers.PrepareConstrainedRegions();
			try
			{
				Monitor.ReliableEnter(s_channelLock, ref tookLock);
				if (chnl != null)
				{
					RegisteredChannelList registeredChannelList = s_registeredChannels;
					int num = registeredChannelList.FindChannelIndex(chnl);
					if (-1 == num)
					{
						throw new RemotingException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Remoting_ChannelNotRegistered"), chnl.ChannelName));
					}
					RegisteredChannel[] registeredChannels = registeredChannelList.RegisteredChannels;
					RegisteredChannel[] array = null;
					array = new RegisteredChannel[registeredChannels.Length - 1];
					(chnl as IChannelReceiver)?.StopListening(null);
					int num2 = 0;
					int num3 = 0;
					while (num3 < registeredChannels.Length)
					{
						if (num3 == num)
						{
							num3++;
							continue;
						}
						array[num2] = registeredChannels[num3];
						num2++;
						num3++;
					}
					if (perf_Contexts != null)
					{
						perf_Contexts->cChannels--;
					}
					s_registeredChannels = new RegisteredChannelList(array);
				}
				RefreshChannelData();
			}
			finally
			{
				if (tookLock)
				{
					Monitor.Exit(s_channelLock);
				}
			}
		}

		internal static IMessageSink CreateMessageSink(string url, object data, out string objectURI)
		{
			IMessageSink messageSink = null;
			objectURI = null;
			RegisteredChannelList registeredChannelList = s_registeredChannels;
			int count = registeredChannelList.Count;
			for (int i = 0; i < count; i++)
			{
				if (registeredChannelList.IsSender(i))
				{
					IChannelSender channelSender = (IChannelSender)registeredChannelList.GetChannel(i);
					messageSink = channelSender.CreateMessageSink(url, data, out objectURI);
					if (messageSink != null)
					{
						break;
					}
				}
			}
			if (objectURI == null)
			{
				objectURI = url;
			}
			return messageSink;
		}

		internal static IMessageSink CreateMessageSink(object data)
		{
			string objectURI;
			return CreateMessageSink(null, data, out objectURI);
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
		public static IChannel GetChannel(string name)
		{
			RegisteredChannelList registeredChannelList = s_registeredChannels;
			int num = registeredChannelList.FindChannelIndex(name);
			if (0 <= num)
			{
				IChannel channel = registeredChannelList.GetChannel(num);
				if (channel is CrossAppDomainChannel || channel is CrossContextChannel)
				{
					return null;
				}
				return channel;
			}
			return null;
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
		public static string[] GetUrlsForObject(MarshalByRefObject obj)
		{
			if (obj == null)
			{
				return null;
			}
			RegisteredChannelList registeredChannelList = s_registeredChannels;
			int count = registeredChannelList.Count;
			Hashtable hashtable = new Hashtable();
			bool fServer;
			Identity identity = MarshalByRefObject.GetIdentity(obj, out fServer);
			if (identity != null)
			{
				string objURI = identity.ObjURI;
				if (objURI != null)
				{
					for (int i = 0; i < count; i++)
					{
						if (!registeredChannelList.IsReceiver(i))
						{
							continue;
						}
						try
						{
							string[] urlsForUri = ((IChannelReceiver)registeredChannelList.GetChannel(i)).GetUrlsForUri(objURI);
							for (int j = 0; j < urlsForUri.Length; j++)
							{
								hashtable.Add(urlsForUri[j], urlsForUri[j]);
							}
						}
						catch (NotSupportedException)
						{
						}
					}
				}
			}
			ICollection keys = hashtable.Keys;
			string[] array = new string[keys.Count];
			int num = 0;
			foreach (string item in keys)
			{
				array[num++] = item;
			}
			return array;
		}

		internal static IMessageSink GetChannelSinkForProxy(object obj)
		{
			IMessageSink result = null;
			if (RemotingServices.IsTransparentProxy(obj))
			{
				RealProxy realProxy = RemotingServices.GetRealProxy(obj);
				RemotingProxy remotingProxy = realProxy as RemotingProxy;
				if (remotingProxy != null)
				{
					Identity identityObject = remotingProxy.IdentityObject;
					result = identityObject.ChannelSink;
				}
			}
			return result;
		}

		[SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.RemotingConfiguration)]
		public static IDictionary GetChannelSinkProperties(object obj)
		{
			IMessageSink channelSinkForProxy = GetChannelSinkForProxy(obj);
			IClientChannelSink clientChannelSink = channelSinkForProxy as IClientChannelSink;
			if (clientChannelSink != null)
			{
				ArrayList arrayList = new ArrayList();
				do
				{
					IDictionary properties = clientChannelSink.Properties;
					if (properties != null)
					{
						arrayList.Add(properties);
					}
					clientChannelSink = clientChannelSink.NextChannelSink;
				}
				while (clientChannelSink != null);
				return new AggregateDictionary(arrayList);
			}
			IDictionary dictionary = channelSinkForProxy as IDictionary;
			if (dictionary != null)
			{
				return dictionary;
			}
			return null;
		}

		internal static IMessageSink GetCrossContextChannelSink()
		{
			if (xCtxChannel == null)
			{
				xCtxChannel = CrossContextChannel.MessageSink;
			}
			return xCtxChannel;
		}

		internal unsafe static void IncrementRemoteCalls(long cCalls)
		{
			remoteCalls += cCalls;
			if (perf_Contexts != null)
			{
				perf_Contexts->cRemoteCalls += (int)cCalls;
			}
		}

		internal static void IncrementRemoteCalls()
		{
			IncrementRemoteCalls(1L);
		}

		internal static void RefreshChannelData()
		{
			bool tookLock = false;
			RuntimeHelpers.PrepareConstrainedRegions();
			try
			{
				Monitor.ReliableEnter(s_channelLock, ref tookLock);
				s_currentChannelData = CollectChannelDataFromChannels();
			}
			finally
			{
				if (tookLock)
				{
					Monitor.Exit(s_channelLock);
				}
			}
		}

		private static object[] CollectChannelDataFromChannels()
		{
			RemotingServices.RegisterWellKnownChannels();
			RegisteredChannelList registeredChannelList = s_registeredChannels;
			int count = registeredChannelList.Count;
			int receiverCount = registeredChannelList.ReceiverCount;
			object[] array = new object[receiverCount];
			int num = 0;
			int i = 0;
			int num2 = 0;
			for (; i < count; i++)
			{
				IChannel channel = registeredChannelList.GetChannel(i);
				if (channel == null)
				{
					throw new RemotingException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Remoting_ChannelNotRegistered"), ""));
				}
				if (registeredChannelList.IsReceiver(i))
				{
					if ((array[num2] = ((IChannelReceiver)channel).ChannelData) != null)
					{
						num++;
					}
					num2++;
				}
			}
			if (num != receiverCount)
			{
				object[] array2 = new object[num];
				int num3 = 0;
				for (int j = 0; j < receiverCount; j++)
				{
					object obj = array[j];
					if (obj != null)
					{
						array2[num3++] = obj;
					}
				}
				array = array2;
			}
			return array;
		}

		private static bool IsMethodReallyPublic(MethodInfo mi)
		{
			if (!mi.IsPublic || mi.IsStatic)
			{
				return false;
			}
			if (!mi.IsGenericMethod)
			{
				return true;
			}
			Type[] genericArguments = mi.GetGenericArguments();
			foreach (Type type in genericArguments)
			{
				if (!type.IsVisible)
				{
					return false;
				}
			}
			return true;
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
		public static ServerProcessing DispatchMessage(IServerChannelSinkStack sinkStack, IMessage msg, out IMessage replyMsg)
		{
			ServerProcessing serverProcessing = ServerProcessing.Complete;
			replyMsg = null;
			try
			{
				if (msg == null)
				{
					throw new ArgumentNullException("msg");
				}
				IncrementRemoteCalls();
				ServerIdentity serverIdentity = CheckDisconnectedOrCreateWellKnownObject(msg);
				if (serverIdentity.ServerType == typeof(AppDomain))
				{
					throw new RemotingException(Environment.GetResourceString("Remoting_AppDomainsCantBeCalledRemotely"));
				}
				IMethodCallMessage methodCallMessage = msg as IMethodCallMessage;
				if (methodCallMessage == null)
				{
					if (!typeof(IMessageSink).IsAssignableFrom(serverIdentity.ServerType))
					{
						throw new RemotingException(Environment.GetResourceString("Remoting_AppDomainsCantBeCalledRemotely"));
					}
					serverProcessing = ServerProcessing.Complete;
					replyMsg = GetCrossContextChannelSink().SyncProcessMessage(msg);
					return serverProcessing;
				}
				MethodInfo methodInfo = (MethodInfo)methodCallMessage.MethodBase;
				if (!IsMethodReallyPublic(methodInfo) && !RemotingServices.IsMethodAllowedRemotely(methodInfo))
				{
					throw new RemotingException(Environment.GetResourceString("Remoting_NonPublicOrStaticCantBeCalledRemotely"));
				}
				InternalRemotingServices.GetReflectionCachedData(methodInfo);
				if (RemotingServices.IsOneWay(methodInfo))
				{
					serverProcessing = ServerProcessing.OneWay;
					GetCrossContextChannelSink().AsyncProcessMessage(msg, null);
					return serverProcessing;
				}
				serverProcessing = ServerProcessing.Complete;
				if (!serverIdentity.ServerType.IsContextful)
				{
					object[] args = new object[2]
					{
						msg,
						serverIdentity.ServerContext
					};
					replyMsg = (IMessage)CrossContextChannel.SyncProcessMessageCallback(args);
					return serverProcessing;
				}
				replyMsg = GetCrossContextChannelSink().SyncProcessMessage(msg);
				return serverProcessing;
			}
			catch (Exception e)
			{
				if (serverProcessing != ServerProcessing.OneWay)
				{
					try
					{
						IMethodCallMessage mcm = (IMethodCallMessage)((msg != null) ? msg : new ErrorMessage());
						replyMsg = new ReturnMessage(e, mcm);
						if (msg == null)
						{
							return serverProcessing;
						}
						((ReturnMessage)replyMsg).SetLogicalCallContext((LogicalCallContext)msg.Properties[Message.CallContextKey]);
						return serverProcessing;
					}
					catch (Exception)
					{
						return serverProcessing;
					}
				}
				return serverProcessing;
			}
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
		public static IMessage SyncDispatchMessage(IMessage msg)
		{
			IMessage message = null;
			bool flag = false;
			try
			{
				if (msg == null)
				{
					throw new ArgumentNullException("msg");
				}
				IncrementRemoteCalls();
				if (!(msg is TransitionCall))
				{
					CheckDisconnectedOrCreateWellKnownObject(msg);
					MethodBase methodBase = ((IMethodMessage)msg).MethodBase;
					flag = RemotingServices.IsOneWay(methodBase);
				}
				IMessageSink crossContextChannelSink = GetCrossContextChannelSink();
				if (!flag)
				{
					message = crossContextChannelSink.SyncProcessMessage(msg);
					return message;
				}
				crossContextChannelSink.AsyncProcessMessage(msg, null);
				return message;
			}
			catch (Exception e)
			{
				if (!flag)
				{
					try
					{
						IMethodCallMessage methodCallMessage = (IMethodCallMessage)((msg != null) ? msg : new ErrorMessage());
						message = new ReturnMessage(e, methodCallMessage);
						if (msg == null)
						{
							return message;
						}
						((ReturnMessage)message).SetLogicalCallContext(methodCallMessage.LogicalCallContext);
						return message;
					}
					catch (Exception)
					{
						return message;
					}
				}
				return message;
			}
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
		public static IMessageCtrl AsyncDispatchMessage(IMessage msg, IMessageSink replySink)
		{
			IMessageCtrl result = null;
			try
			{
				if (msg == null)
				{
					throw new ArgumentNullException("msg");
				}
				IncrementRemoteCalls();
				if (!(msg is TransitionCall))
				{
					CheckDisconnectedOrCreateWellKnownObject(msg);
				}
				result = GetCrossContextChannelSink().AsyncProcessMessage(msg, replySink);
				return result;
			}
			catch (Exception e)
			{
				if (replySink != null)
				{
					try
					{
						IMethodCallMessage methodCallMessage = (IMethodCallMessage)msg;
						ReturnMessage returnMessage = new ReturnMessage(e, (IMethodCallMessage)msg);
						if (msg != null)
						{
							returnMessage.SetLogicalCallContext(methodCallMessage.LogicalCallContext);
						}
						replySink.SyncProcessMessage(returnMessage);
						return result;
					}
					catch (Exception)
					{
						return result;
					}
				}
				return result;
			}
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
		public static IServerChannelSink CreateServerChannelSinkChain(IServerChannelSinkProvider provider, IChannelReceiver channel)
		{
			if (provider == null)
			{
				return new DispatchChannelSink();
			}
			IServerChannelSinkProvider serverChannelSinkProvider = provider;
			while (serverChannelSinkProvider.Next != null)
			{
				serverChannelSinkProvider = serverChannelSinkProvider.Next;
			}
			serverChannelSinkProvider.Next = new DispatchChannelSinkProvider();
			IServerChannelSink result = provider.CreateSink(channel);
			serverChannelSinkProvider.Next = null;
			return result;
		}

		internal static ServerIdentity CheckDisconnectedOrCreateWellKnownObject(IMessage msg)
		{
			ServerIdentity serverIdentity = InternalSink.GetServerIdentity(msg);
			if (serverIdentity == null || serverIdentity.IsRemoteDisconnected())
			{
				string uRI = InternalSink.GetURI(msg);
				if (uRI != null)
				{
					ServerIdentity serverIdentity2 = RemotingConfigHandler.CreateWellKnownObject(uRI);
					if (serverIdentity2 != null)
					{
						serverIdentity = serverIdentity2;
					}
				}
			}
			if (serverIdentity == null || serverIdentity.IsRemoteDisconnected())
			{
				string uRI2 = InternalSink.GetURI(msg);
				throw new RemotingException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Remoting_Disconnected"), uRI2));
			}
			return serverIdentity;
		}

		internal static void UnloadHandler(object sender, EventArgs e)
		{
			StopListeningOnAllChannels();
		}

		private static void StopListeningOnAllChannels()
		{
			try
			{
				RegisteredChannelList registeredChannelList = s_registeredChannels;
				int count = registeredChannelList.Count;
				for (int i = 0; i < count; i++)
				{
					if (registeredChannelList.IsReceiver(i))
					{
						IChannelReceiver channelReceiver = (IChannelReceiver)registeredChannelList.GetChannel(i);
						channelReceiver.StopListening(null);
					}
				}
			}
			catch (Exception)
			{
			}
		}

		internal static void NotifyProfiler(IMessage msg, RemotingProfilerEvent profilerEvent)
		{
			switch (profilerEvent)
			{
			case RemotingProfilerEvent.ClientSend:
				if (RemotingServices.CORProfilerTrackRemoting())
				{
					RemotingServices.CORProfilerRemotingClientSendingMessage(out var id2, fIsAsync: false);
					if (RemotingServices.CORProfilerTrackRemotingCookie())
					{
						msg.Properties["CORProfilerCookie"] = id2;
					}
				}
				break;
			case RemotingProfilerEvent.ClientReceive:
			{
				if (!RemotingServices.CORProfilerTrackRemoting())
				{
					break;
				}
				Guid id = Guid.Empty;
				if (RemotingServices.CORProfilerTrackRemotingCookie())
				{
					object obj = msg.Properties["CORProfilerCookie"];
					if (obj != null)
					{
						id = (Guid)obj;
					}
				}
				RemotingServices.CORProfilerRemotingClientReceivingReply(id, fIsAsync: false);
				break;
			}
			}
		}

		internal static string FindFirstHttpUrlForObject(string objectUri)
		{
			if (objectUri == null)
			{
				return null;
			}
			RegisteredChannelList registeredChannelList = s_registeredChannels;
			int count = registeredChannelList.Count;
			for (int i = 0; i < count; i++)
			{
				if (!registeredChannelList.IsReceiver(i))
				{
					continue;
				}
				IChannelReceiver channelReceiver = (IChannelReceiver)registeredChannelList.GetChannel(i);
				string fullName = channelReceiver.GetType().FullName;
				if (string.CompareOrdinal(fullName, "System.Runtime.Remoting.Channels.Http.HttpChannel") == 0 || string.CompareOrdinal(fullName, "System.Runtime.Remoting.Channels.Http.HttpServerChannel") == 0)
				{
					string[] urlsForUri = channelReceiver.GetUrlsForUri(objectUri);
					if (urlsForUri != null && urlsForUri.Length > 0)
					{
						return urlsForUri[0];
					}
				}
			}
			return null;
		}
	}
}
