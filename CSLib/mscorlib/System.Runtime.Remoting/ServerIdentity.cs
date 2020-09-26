using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Contexts;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;
using System.Threading;

namespace System.Runtime.Remoting
{
	internal class ServerIdentity : Identity
	{
		private class LastCalledType
		{
			public string typeName;

			public Type type;
		}

		internal Context _srvCtx;

		internal IMessageSink _serverObjectChain;

		internal StackBuilderSink _stackBuilderSink;

		internal DynamicPropertyHolder _dphSrv;

		internal Type _srvType;

		private LastCalledType _lastCalledType;

		internal bool _bMarshaledAsSpecificType;

		internal int _firstCallDispatched;

		internal GCHandle _srvIdentityHandle;

		internal Context ServerContext
		{
			[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
			get
			{
				return _srvCtx;
			}
		}

		internal Type ServerType
		{
			get
			{
				return _srvType;
			}
			set
			{
				_srvType = value;
			}
		}

		internal bool MarshaledAsSpecificType
		{
			get
			{
				return _bMarshaledAsSpecificType;
			}
			set
			{
				_bMarshaledAsSpecificType = value;
			}
		}

		internal ArrayWithSize ServerSideDynamicSinks
		{
			get
			{
				if (_dphSrv == null)
				{
					return null;
				}
				return _dphSrv.DynamicSinks;
			}
		}

		internal Type GetLastCalledType(string newTypeName)
		{
			LastCalledType lastCalledType = _lastCalledType;
			if (lastCalledType == null)
			{
				return null;
			}
			string typeName = lastCalledType.typeName;
			Type type = lastCalledType.type;
			if (typeName == null || type == null)
			{
				return null;
			}
			if (typeName.Equals(newTypeName))
			{
				return type;
			}
			return null;
		}

		internal void SetLastCalledType(string newTypeName, Type newType)
		{
			LastCalledType lastCalledType = new LastCalledType();
			lastCalledType.typeName = newTypeName;
			lastCalledType.type = newType;
			_lastCalledType = lastCalledType;
		}

		internal void SetHandle()
		{
			bool tookLock = false;
			RuntimeHelpers.PrepareConstrainedRegions();
			try
			{
				Monitor.ReliableEnter(this, ref tookLock);
				if (!_srvIdentityHandle.IsAllocated)
				{
					_srvIdentityHandle = new GCHandle(this, GCHandleType.Normal);
				}
				else
				{
					_srvIdentityHandle.Target = this;
				}
			}
			finally
			{
				if (tookLock)
				{
					Monitor.Exit(this);
				}
			}
		}

		internal void ResetHandle()
		{
			bool tookLock = false;
			RuntimeHelpers.PrepareConstrainedRegions();
			try
			{
				Monitor.ReliableEnter(this, ref tookLock);
				_srvIdentityHandle.Target = null;
			}
			finally
			{
				if (tookLock)
				{
					Monitor.Exit(this);
				}
			}
		}

		internal GCHandle GetHandle()
		{
			return _srvIdentityHandle;
		}

		internal ServerIdentity(MarshalByRefObject obj, Context serverCtx)
			: base(obj is ContextBoundObject)
		{
			if (obj != null)
			{
				if (!RemotingServices.IsTransparentProxy(obj))
				{
					_srvType = obj.GetType();
				}
				else
				{
					RealProxy realProxy = RemotingServices.GetRealProxy(obj);
					_srvType = realProxy.GetProxiedType();
				}
			}
			_srvCtx = serverCtx;
			_serverObjectChain = null;
			_stackBuilderSink = null;
		}

		internal ServerIdentity(MarshalByRefObject obj, Context serverCtx, string uri)
			: this(obj, serverCtx)
		{
			SetOrCreateURI(uri, bIdCtor: true);
		}

		internal void SetSingleCallObjectMode()
		{
			_flags |= 512;
		}

		internal void SetSingletonObjectMode()
		{
			_flags |= 1024;
		}

		internal bool IsSingleCall()
		{
			return (_flags & 0x200) != 0;
		}

		internal bool IsSingleton()
		{
			return (_flags & 0x400) != 0;
		}

		internal IMessageSink GetServerObjectChain(out MarshalByRefObject obj)
		{
			obj = null;
			if (!IsSingleCall())
			{
				if (_serverObjectChain == null)
				{
					bool tookLock = false;
					RuntimeHelpers.PrepareConstrainedRegions();
					try
					{
						Monitor.ReliableEnter(this, ref tookLock);
						if (_serverObjectChain == null)
						{
							MarshalByRefObject tPOrObject = base.TPOrObject;
							_serverObjectChain = _srvCtx.CreateServerObjectChain(tPOrObject);
						}
					}
					finally
					{
						if (tookLock)
						{
							Monitor.Exit(this);
						}
					}
				}
				return _serverObjectChain;
			}
			MarshalByRefObject marshalByRefObject = null;
			IMessageSink messageSink = null;
			if (_tpOrObject != null && _firstCallDispatched == 0 && Interlocked.CompareExchange(ref _firstCallDispatched, 1, 0) == 0)
			{
				marshalByRefObject = (MarshalByRefObject)_tpOrObject;
				messageSink = _serverObjectChain;
				if (messageSink == null)
				{
					messageSink = _srvCtx.CreateServerObjectChain(marshalByRefObject);
				}
			}
			else
			{
				marshalByRefObject = (MarshalByRefObject)Activator.CreateInstance(_srvType, nonPublic: true);
				string objectUri = RemotingServices.GetObjectUri(marshalByRefObject);
				if (objectUri != null)
				{
					throw new RemotingException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Remoting_WellKnown_CtorCantMarshal"), base.URI));
				}
				if (!RemotingServices.IsTransparentProxy(marshalByRefObject))
				{
					marshalByRefObject.__RaceSetServerIdentity(this);
				}
				else
				{
					RealProxy realProxy = null;
					realProxy = RemotingServices.GetRealProxy(marshalByRefObject);
					realProxy.IdentityObject = this;
				}
				messageSink = _srvCtx.CreateServerObjectChain(marshalByRefObject);
			}
			obj = marshalByRefObject;
			return messageSink;
		}

		internal IMessageSink RaceSetServerObjectChain(IMessageSink serverObjectChain)
		{
			if (_serverObjectChain == null)
			{
				bool tookLock = false;
				RuntimeHelpers.PrepareConstrainedRegions();
				try
				{
					Monitor.ReliableEnter(this, ref tookLock);
					if (_serverObjectChain == null)
					{
						_serverObjectChain = serverObjectChain;
					}
				}
				finally
				{
					if (tookLock)
					{
						Monitor.Exit(this);
					}
				}
			}
			return _serverObjectChain;
		}

		internal bool AddServerSideDynamicProperty(IDynamicProperty prop)
		{
			if (_dphSrv == null)
			{
				DynamicPropertyHolder dphSrv = new DynamicPropertyHolder();
				bool tookLock = false;
				RuntimeHelpers.PrepareConstrainedRegions();
				try
				{
					Monitor.ReliableEnter(this, ref tookLock);
					if (_dphSrv == null)
					{
						_dphSrv = dphSrv;
					}
				}
				finally
				{
					if (tookLock)
					{
						Monitor.Exit(this);
					}
				}
			}
			return _dphSrv.AddDynamicProperty(prop);
		}

		internal bool RemoveServerSideDynamicProperty(string name)
		{
			if (_dphSrv == null)
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_PropNotFound"));
			}
			return _dphSrv.RemoveDynamicProperty(name);
		}

		internal override void AssertValid()
		{
			if (base.TPOrObject != null)
			{
				RemotingServices.IsTransparentProxy(base.TPOrObject);
			}
		}
	}
}
