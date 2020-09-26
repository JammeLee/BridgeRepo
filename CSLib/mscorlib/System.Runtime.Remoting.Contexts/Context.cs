using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Activation;
using System.Runtime.Remoting.Messaging;
using System.Security.Permissions;
using System.Threading;

namespace System.Runtime.Remoting.Contexts
{
	[ComVisible(true)]
	public class Context
	{
		internal const int CTX_DEFAULT_CONTEXT = 1;

		internal const int CTX_FROZEN = 2;

		internal const int CTX_THREADPOOL_AWARE = 4;

		private const int GROW_BY = 8;

		private const int STATICS_BUCKET_SIZE = 8;

		private IContextProperty[] _ctxProps;

		private DynamicPropertyHolder _dphCtx;

		private LocalDataStore _localDataStore;

		private IMessageSink _serverContextChain;

		private IMessageSink _clientContextChain;

		private AppDomain _appDomain;

		private object[] _ctxStatics;

		private IntPtr _internalContext;

		private int _ctxID;

		private int _ctxFlags;

		private int _numCtxProps;

		private int _ctxStaticsCurrentBucket;

		private int _ctxStaticsFreeIndex;

		private static DynamicPropertyHolder _dphGlobal = new DynamicPropertyHolder();

		private static LocalDataStoreMgr _localDataStoreMgr = new LocalDataStoreMgr();

		private static int _ctxIDCounter = 0;

		public virtual int ContextID
		{
			[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
			get
			{
				return _ctxID;
			}
		}

		internal virtual IntPtr InternalContextID => _internalContext;

		internal virtual AppDomain AppDomain => _appDomain;

		internal bool IsDefaultContext => _ctxID == 0;

		public static Context DefaultContext
		{
			[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
			get
			{
				return Thread.GetDomain().GetDefaultContext();
			}
		}

		internal virtual bool IsThreadPoolAware => (_ctxFlags & 4) == 4;

		public virtual IContextProperty[] ContextProperties
		{
			[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
			get
			{
				if (_ctxProps == null)
				{
					return null;
				}
				lock (this)
				{
					IContextProperty[] array = new IContextProperty[_numCtxProps];
					Array.Copy(_ctxProps, array, _numCtxProps);
					return array;
				}
			}
		}

		private LocalDataStore MyLocalStore
		{
			get
			{
				if (_localDataStore == null)
				{
					lock (_localDataStoreMgr)
					{
						if (_localDataStore == null)
						{
							_localDataStore = _localDataStoreMgr.CreateLocalDataStore();
						}
					}
				}
				return _localDataStore;
			}
		}

		internal virtual IDynamicProperty[] PerContextDynamicProperties
		{
			get
			{
				if (_dphCtx == null)
				{
					return null;
				}
				return _dphCtx.DynamicProperties;
			}
		}

		internal static ArrayWithSize GlobalDynamicSinks => _dphGlobal.DynamicSinks;

		internal virtual ArrayWithSize DynamicSinks
		{
			get
			{
				if (_dphCtx == null)
				{
					return null;
				}
				return _dphCtx.DynamicSinks;
			}
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
		public Context()
			: this(0)
		{
		}

		private Context(int flags)
		{
			_ctxFlags = flags;
			if (((uint)_ctxFlags & (true ? 1u : 0u)) != 0)
			{
				_ctxID = 0;
			}
			else
			{
				_ctxID = Interlocked.Increment(ref _ctxIDCounter);
			}
			DomainSpecificRemotingData remotingData = Thread.GetDomain().RemotingData;
			if (remotingData != null)
			{
				IContextProperty[] appDomainContextProperties = remotingData.AppDomainContextProperties;
				if (appDomainContextProperties != null)
				{
					for (int i = 0; i < appDomainContextProperties.Length; i++)
					{
						SetProperty(appDomainContextProperties[i]);
					}
				}
			}
			if (((uint)_ctxFlags & (true ? 1u : 0u)) != 0)
			{
				Freeze();
			}
			SetupInternalContext((_ctxFlags & 1) == 1);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern void SetupInternalContext(bool bDefault);

		~Context()
		{
			if (_internalContext != IntPtr.Zero && (_ctxFlags & 1) == 0)
			{
				CleanupInternalContext();
			}
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern void CleanupInternalContext();

		internal static Context CreateDefaultContext()
		{
			return new Context(1);
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
		public virtual IContextProperty GetProperty(string name)
		{
			if (_ctxProps == null || name == null)
			{
				return null;
			}
			IContextProperty result = null;
			for (int i = 0; i < _numCtxProps; i++)
			{
				if (_ctxProps[i].Name.Equals(name))
				{
					result = _ctxProps[i];
					break;
				}
			}
			return result;
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
		public virtual void SetProperty(IContextProperty prop)
		{
			if (prop == null || prop.Name == null)
			{
				throw new ArgumentNullException((prop == null) ? "prop" : "property name");
			}
			if (((uint)_ctxFlags & 2u) != 0)
			{
				throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_AddContextFrozen"));
			}
			lock (this)
			{
				CheckPropertyNameClash(prop.Name, _ctxProps, _numCtxProps);
				if (_ctxProps == null || _numCtxProps == _ctxProps.Length)
				{
					_ctxProps = GrowPropertiesArray(_ctxProps);
				}
				_ctxProps[_numCtxProps++] = prop;
			}
		}

		internal virtual void InternalFreeze()
		{
			_ctxFlags |= 2;
			for (int i = 0; i < _numCtxProps; i++)
			{
				_ctxProps[i].Freeze(this);
			}
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
		public virtual void Freeze()
		{
			lock (this)
			{
				if (((uint)_ctxFlags & 2u) != 0)
				{
					throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_ContextAlreadyFrozen"));
				}
				InternalFreeze();
			}
		}

		internal virtual void SetThreadPoolAware()
		{
			_ctxFlags |= 4;
		}

		internal static void CheckPropertyNameClash(string name, IContextProperty[] props, int count)
		{
			for (int i = 0; i < count; i++)
			{
				if (props[i].Name.Equals(name))
				{
					throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_DuplicatePropertyName"));
				}
			}
		}

		internal static IContextProperty[] GrowPropertiesArray(IContextProperty[] props)
		{
			int num = ((props != null) ? props.Length : 0) + 8;
			IContextProperty[] array = new IContextProperty[num];
			if (props != null)
			{
				Array.Copy(props, array, props.Length);
			}
			return array;
		}

		internal virtual IMessageSink GetServerContextChain()
		{
			if (_serverContextChain == null)
			{
				IMessageSink messageSink = ServerContextTerminatorSink.MessageSink;
				object obj = null;
				int numCtxProps = _numCtxProps;
				while (numCtxProps-- > 0)
				{
					obj = _ctxProps[numCtxProps];
					IContributeServerContextSink contributeServerContextSink = obj as IContributeServerContextSink;
					if (contributeServerContextSink != null)
					{
						messageSink = contributeServerContextSink.GetServerContextSink(messageSink);
						if (messageSink == null)
						{
							throw new RemotingException(Environment.GetResourceString("Remoting_Contexts_BadProperty"));
						}
					}
				}
				lock (this)
				{
					if (_serverContextChain == null)
					{
						_serverContextChain = messageSink;
					}
				}
			}
			return _serverContextChain;
		}

		internal virtual IMessageSink GetClientContextChain()
		{
			if (_clientContextChain == null)
			{
				IMessageSink messageSink = ClientContextTerminatorSink.MessageSink;
				object obj = null;
				for (int i = 0; i < _numCtxProps; i++)
				{
					obj = _ctxProps[i];
					IContributeClientContextSink contributeClientContextSink = obj as IContributeClientContextSink;
					if (contributeClientContextSink != null)
					{
						messageSink = contributeClientContextSink.GetClientContextSink(messageSink);
						if (messageSink == null)
						{
							throw new RemotingException(Environment.GetResourceString("Remoting_Contexts_BadProperty"));
						}
					}
				}
				lock (this)
				{
					if (_clientContextChain == null)
					{
						_clientContextChain = messageSink;
					}
				}
			}
			return _clientContextChain;
		}

		internal virtual IMessageSink CreateServerObjectChain(MarshalByRefObject serverObj)
		{
			IMessageSink messageSink = new ServerObjectTerminatorSink(serverObj);
			object obj = null;
			int numCtxProps = _numCtxProps;
			while (numCtxProps-- > 0)
			{
				obj = _ctxProps[numCtxProps];
				IContributeObjectSink contributeObjectSink = obj as IContributeObjectSink;
				if (contributeObjectSink != null)
				{
					messageSink = contributeObjectSink.GetObjectSink(serverObj, messageSink);
					if (messageSink == null)
					{
						throw new RemotingException(Environment.GetResourceString("Remoting_Contexts_BadProperty"));
					}
				}
			}
			return messageSink;
		}

		internal virtual IMessageSink CreateEnvoyChain(MarshalByRefObject objectOrProxy)
		{
			IMessageSink messageSink = EnvoyTerminatorSink.MessageSink;
			object obj = null;
			for (int i = 0; i < _numCtxProps; i++)
			{
				obj = _ctxProps[i];
				IContributeEnvoySink contributeEnvoySink = obj as IContributeEnvoySink;
				if (contributeEnvoySink != null)
				{
					messageSink = contributeEnvoySink.GetEnvoySink(objectOrProxy, messageSink);
					if (messageSink == null)
					{
						throw new RemotingException(Environment.GetResourceString("Remoting_Contexts_BadProperty"));
					}
				}
			}
			return messageSink;
		}

		internal IMessage NotifyActivatorProperties(IMessage msg, bool bServerSide)
		{
			IMessage result = null;
			try
			{
				int numCtxProps = _numCtxProps;
				object obj = null;
				while (numCtxProps-- != 0)
				{
					obj = _ctxProps[numCtxProps];
					IContextPropertyActivator contextPropertyActivator = obj as IContextPropertyActivator;
					if (contextPropertyActivator == null)
					{
						continue;
					}
					IConstructionCallMessage constructionCallMessage = msg as IConstructionCallMessage;
					if (constructionCallMessage != null)
					{
						if (!bServerSide)
						{
							contextPropertyActivator.CollectFromClientContext(constructionCallMessage);
						}
						else
						{
							contextPropertyActivator.DeliverClientContextToServerContext(constructionCallMessage);
						}
					}
					else if (bServerSide)
					{
						contextPropertyActivator.CollectFromServerContext((IConstructionReturnMessage)msg);
					}
					else
					{
						contextPropertyActivator.DeliverServerContextToClientContext((IConstructionReturnMessage)msg);
					}
				}
				return result;
			}
			catch (Exception e)
			{
				IMethodCallMessage methodCallMessage = null;
				methodCallMessage = ((!(msg is IConstructionCallMessage)) ? new ErrorMessage() : ((IMethodCallMessage)msg));
				result = new ReturnMessage(e, methodCallMessage);
				if (msg != null)
				{
					((ReturnMessage)result).SetLogicalCallContext((LogicalCallContext)msg.Properties[Message.CallContextKey]);
					return result;
				}
				return result;
			}
		}

		public override string ToString()
		{
			return "ContextID: " + _ctxID;
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
		public void DoCallBack(CrossContextDelegate deleg)
		{
			if (deleg == null)
			{
				throw new ArgumentNullException("deleg");
			}
			if ((_ctxFlags & 2) == 0)
			{
				throw new RemotingException(Environment.GetResourceString("Remoting_Contexts_ContextNotFrozenForCallBack"));
			}
			Context currentContext = Thread.CurrentContext;
			if (currentContext == this)
			{
				deleg();
				return;
			}
			currentContext.DoCallBackGeneric(InternalContextID, deleg);
			GC.KeepAlive(this);
		}

		internal static void DoCallBackFromEE(IntPtr targetCtxID, IntPtr privateData, int targetDomainID)
		{
			if (targetDomainID == 0)
			{
				CallBackHelper @object = new CallBackHelper(privateData, bFromEE: true, targetDomainID);
				CrossContextDelegate deleg = @object.Func;
				Thread.CurrentContext.DoCallBackGeneric(targetCtxID, deleg);
				return;
			}
			TransitionCall msg = new TransitionCall(targetCtxID, privateData, targetDomainID);
			Message.PropagateCallContextFromThreadToMessage(msg);
			IMessage message = Thread.CurrentContext.GetClientContextChain().SyncProcessMessage(msg);
			Message.PropagateCallContextFromMessageToThread(message);
			IMethodReturnMessage methodReturnMessage = message as IMethodReturnMessage;
			if (methodReturnMessage == null || methodReturnMessage.Exception == null)
			{
				return;
			}
			throw methodReturnMessage.Exception;
		}

		internal void DoCallBackGeneric(IntPtr targetCtxID, CrossContextDelegate deleg)
		{
			TransitionCall msg = new TransitionCall(targetCtxID, deleg);
			Message.PropagateCallContextFromThreadToMessage(msg);
			IMessage message = GetClientContextChain().SyncProcessMessage(msg);
			if (message != null)
			{
				Message.PropagateCallContextFromMessageToThread(message);
			}
			IMethodReturnMessage methodReturnMessage = message as IMethodReturnMessage;
			if (methodReturnMessage != null && methodReturnMessage.Exception != null)
			{
				throw methodReturnMessage.Exception;
			}
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern void ExecuteCallBackInEE(IntPtr privateData);

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
		public static LocalDataStoreSlot AllocateDataSlot()
		{
			return _localDataStoreMgr.AllocateDataSlot();
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
		public static LocalDataStoreSlot AllocateNamedDataSlot(string name)
		{
			return _localDataStoreMgr.AllocateNamedDataSlot(name);
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
		public static LocalDataStoreSlot GetNamedDataSlot(string name)
		{
			return _localDataStoreMgr.GetNamedDataSlot(name);
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
		public static void FreeNamedDataSlot(string name)
		{
			_localDataStoreMgr.FreeNamedDataSlot(name);
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
		public static void SetData(LocalDataStoreSlot slot, object data)
		{
			Thread.CurrentContext.MyLocalStore.SetData(slot, data);
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
		public static object GetData(LocalDataStoreSlot slot)
		{
			return Thread.CurrentContext.MyLocalStore.GetData(slot);
		}

		private int ReserveSlot()
		{
			if (_ctxStatics == null)
			{
				_ctxStatics = new object[8];
				_ctxStatics[0] = null;
				_ctxStaticsFreeIndex = 1;
				_ctxStaticsCurrentBucket = 0;
			}
			if (_ctxStaticsFreeIndex == 8)
			{
				object[] array = new object[8];
				object[] array2 = _ctxStatics;
				while (array2[0] != null)
				{
					array2 = (object[])array2[0];
				}
				array2[0] = array;
				_ctxStaticsFreeIndex = 1;
				_ctxStaticsCurrentBucket++;
			}
			return _ctxStaticsFreeIndex++ | (_ctxStaticsCurrentBucket << 16);
		}

		[SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.Infrastructure)]
		public static bool RegisterDynamicProperty(IDynamicProperty prop, ContextBoundObject obj, Context ctx)
		{
			bool flag = false;
			if (prop == null || prop.Name == null || !(prop is IContributeDynamicSink))
			{
				throw new ArgumentNullException("prop");
			}
			if (obj != null && ctx != null)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_NonNullObjAndCtx"));
			}
			if (obj != null)
			{
				return IdentityHolder.AddDynamicProperty(obj, prop);
			}
			return AddDynamicProperty(ctx, prop);
		}

		[SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.Infrastructure)]
		public static bool UnregisterDynamicProperty(string name, ContextBoundObject obj, Context ctx)
		{
			bool flag = false;
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			if (obj != null && ctx != null)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_NonNullObjAndCtx"));
			}
			if (obj != null)
			{
				return IdentityHolder.RemoveDynamicProperty(obj, name);
			}
			return RemoveDynamicProperty(ctx, name);
		}

		internal static bool AddDynamicProperty(Context ctx, IDynamicProperty prop)
		{
			return ctx?.AddPerContextDynamicProperty(prop) ?? AddGlobalDynamicProperty(prop);
		}

		private bool AddPerContextDynamicProperty(IDynamicProperty prop)
		{
			if (_dphCtx == null)
			{
				DynamicPropertyHolder dphCtx = new DynamicPropertyHolder();
				lock (this)
				{
					if (_dphCtx == null)
					{
						_dphCtx = dphCtx;
					}
				}
			}
			return _dphCtx.AddDynamicProperty(prop);
		}

		private static bool AddGlobalDynamicProperty(IDynamicProperty prop)
		{
			return _dphGlobal.AddDynamicProperty(prop);
		}

		internal static bool RemoveDynamicProperty(Context ctx, string name)
		{
			return ctx?.RemovePerContextDynamicProperty(name) ?? RemoveGlobalDynamicProperty(name);
		}

		private bool RemovePerContextDynamicProperty(string name)
		{
			if (_dphCtx == null)
			{
				throw new RemotingException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Remoting_Contexts_NoProperty"), name));
			}
			return _dphCtx.RemoveDynamicProperty(name);
		}

		private static bool RemoveGlobalDynamicProperty(string name)
		{
			return _dphGlobal.RemoveDynamicProperty(name);
		}

		internal virtual bool NotifyDynamicSinks(IMessage msg, bool bCliSide, bool bStart, bool bAsync, bool bNotifyGlobals)
		{
			bool result = false;
			if (bNotifyGlobals && _dphGlobal.DynamicProperties != null)
			{
				ArrayWithSize globalDynamicSinks = GlobalDynamicSinks;
				if (globalDynamicSinks != null)
				{
					DynamicPropertyHolder.NotifyDynamicSinks(msg, globalDynamicSinks, bCliSide, bStart, bAsync);
					result = true;
				}
			}
			ArrayWithSize dynamicSinks = DynamicSinks;
			if (dynamicSinks != null)
			{
				DynamicPropertyHolder.NotifyDynamicSinks(msg, dynamicSinks, bCliSide, bStart, bAsync);
				result = true;
			}
			return result;
		}
	}
}
