using System.Collections;
using System.Reflection;
using System.Runtime.Remoting.Activation;
using System.Runtime.Remoting.Proxies;
using System.Threading;

namespace System.Runtime.Remoting.Messaging
{
	internal class ConstructorCallMessage : IConstructionCallMessage, IMethodCallMessage, IMethodMessage, IMessage
	{
		private const int CCM_ACTIVATEINCONTEXT = 1;

		private object[] _callSiteActivationAttributes;

		private object[] _womGlobalAttributes;

		private object[] _typeAttributes;

		[NonSerialized]
		private Type _activationType;

		private string _activationTypeName;

		private IList _contextProperties;

		private int _iFlags;

		private Message _message;

		private object _properties;

		private ArgMapper _argMapper;

		private IActivator _activator;

		public object[] CallSiteActivationAttributes => _callSiteActivationAttributes;

		public Type ActivationType
		{
			get
			{
				if (_activationType == null && _activationTypeName != null)
				{
					_activationType = RemotingServices.InternalGetTypeFromQualifiedTypeName(_activationTypeName, partialFallback: false);
				}
				return _activationType;
			}
		}

		public string ActivationTypeName => _activationTypeName;

		public IList ContextProperties
		{
			get
			{
				if (_contextProperties == null)
				{
					_contextProperties = new ArrayList();
				}
				return _contextProperties;
			}
		}

		public string Uri
		{
			get
			{
				if (_message != null)
				{
					return _message.Uri;
				}
				throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_InternalState"));
			}
			set
			{
				if (_message != null)
				{
					_message.Uri = value;
					return;
				}
				throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_InternalState"));
			}
		}

		public string MethodName
		{
			get
			{
				if (_message != null)
				{
					return _message.MethodName;
				}
				throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_InternalState"));
			}
		}

		public string TypeName
		{
			get
			{
				if (_message != null)
				{
					return _message.TypeName;
				}
				throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_InternalState"));
			}
		}

		public object MethodSignature
		{
			get
			{
				if (_message != null)
				{
					return _message.MethodSignature;
				}
				throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_InternalState"));
			}
		}

		public MethodBase MethodBase
		{
			get
			{
				if (_message != null)
				{
					return _message.MethodBase;
				}
				throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_InternalState"));
			}
		}

		public int InArgCount
		{
			get
			{
				if (_argMapper == null)
				{
					_argMapper = new ArgMapper(this, fOut: false);
				}
				return _argMapper.ArgCount;
			}
		}

		public object[] InArgs
		{
			get
			{
				if (_argMapper == null)
				{
					_argMapper = new ArgMapper(this, fOut: false);
				}
				return _argMapper.Args;
			}
		}

		public int ArgCount
		{
			get
			{
				if (_message != null)
				{
					return _message.ArgCount;
				}
				throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_InternalState"));
			}
		}

		public bool HasVarArgs
		{
			get
			{
				if (_message != null)
				{
					return _message.HasVarArgs;
				}
				throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_InternalState"));
			}
		}

		public object[] Args
		{
			get
			{
				if (_message != null)
				{
					return _message.Args;
				}
				throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_InternalState"));
			}
		}

		public IDictionary Properties
		{
			get
			{
				if (_properties == null)
				{
					object value = new CCMDictionary(this, new Hashtable());
					Interlocked.CompareExchange(ref _properties, value, null);
				}
				return (IDictionary)_properties;
			}
		}

		public IActivator Activator
		{
			get
			{
				return _activator;
			}
			set
			{
				_activator = value;
			}
		}

		public LogicalCallContext LogicalCallContext => GetLogicalCallContext();

		internal bool ActivateInContext
		{
			get
			{
				return (_iFlags & 1) != 0;
			}
			set
			{
				_iFlags = (value ? (_iFlags | 1) : (_iFlags & -2));
			}
		}

		private ConstructorCallMessage()
		{
		}

		internal ConstructorCallMessage(object[] callSiteActivationAttributes, object[] womAttr, object[] typeAttr, Type serverType)
		{
			_activationType = serverType;
			_activationTypeName = RemotingServices.GetDefaultQualifiedTypeName(_activationType);
			_callSiteActivationAttributes = callSiteActivationAttributes;
			_womGlobalAttributes = womAttr;
			_typeAttributes = typeAttr;
		}

		public object GetThisPtr()
		{
			if (_message != null)
			{
				return _message.GetThisPtr();
			}
			throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_InternalState"));
		}

		internal object[] GetWOMAttributes()
		{
			return _womGlobalAttributes;
		}

		internal object[] GetTypeAttributes()
		{
			return _typeAttributes;
		}

		public object GetInArg(int argNum)
		{
			if (_argMapper == null)
			{
				_argMapper = new ArgMapper(this, fOut: false);
			}
			return _argMapper.GetArg(argNum);
		}

		public string GetInArgName(int index)
		{
			if (_argMapper == null)
			{
				_argMapper = new ArgMapper(this, fOut: false);
			}
			return _argMapper.GetArgName(index);
		}

		public object GetArg(int argNum)
		{
			if (_message != null)
			{
				return _message.GetArg(argNum);
			}
			throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_InternalState"));
		}

		public string GetArgName(int index)
		{
			if (_message != null)
			{
				return _message.GetArgName(index);
			}
			throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_InternalState"));
		}

		internal void SetFrame(MessageData msgData)
		{
			_message = new Message();
			_message.InitFields(msgData);
		}

		internal LogicalCallContext GetLogicalCallContext()
		{
			if (_message != null)
			{
				return _message.GetLogicalCallContext();
			}
			throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_InternalState"));
		}

		internal LogicalCallContext SetLogicalCallContext(LogicalCallContext ctx)
		{
			if (_message != null)
			{
				return _message.SetLogicalCallContext(ctx);
			}
			throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_InternalState"));
		}

		internal Message GetMessage()
		{
			return _message;
		}
	}
}
