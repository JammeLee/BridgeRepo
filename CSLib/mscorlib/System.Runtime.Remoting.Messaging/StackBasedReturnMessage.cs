using System.Collections;
using System.Reflection;

namespace System.Runtime.Remoting.Messaging
{
	internal class StackBasedReturnMessage : IMethodReturnMessage, IMethodMessage, IMessage, IInternalMessage
	{
		private Message _m;

		private Hashtable _h;

		private MRMDictionary _d;

		private ArgMapper _argMapper;

		public string Uri => _m.Uri;

		public string MethodName => _m.MethodName;

		public string TypeName => _m.TypeName;

		public object MethodSignature => _m.MethodSignature;

		public MethodBase MethodBase => _m.MethodBase;

		public bool HasVarArgs => _m.HasVarArgs;

		public int ArgCount => _m.ArgCount;

		public object[] Args => _m.Args;

		public LogicalCallContext LogicalCallContext => _m.GetLogicalCallContext();

		public int OutArgCount
		{
			get
			{
				if (_argMapper == null)
				{
					_argMapper = new ArgMapper(this, fOut: true);
				}
				return _argMapper.ArgCount;
			}
		}

		public object[] OutArgs
		{
			get
			{
				if (_argMapper == null)
				{
					_argMapper = new ArgMapper(this, fOut: true);
				}
				return _argMapper.Args;
			}
		}

		public Exception Exception => null;

		public object ReturnValue => _m.GetReturnValue();

		public IDictionary Properties
		{
			get
			{
				lock (this)
				{
					if (_h == null)
					{
						_h = new Hashtable();
					}
					if (_d == null)
					{
						_d = new MRMDictionary(this, _h);
					}
					return _d;
				}
			}
		}

		ServerIdentity IInternalMessage.ServerIdentityObject
		{
			get
			{
				return null;
			}
			set
			{
			}
		}

		Identity IInternalMessage.IdentityObject
		{
			get
			{
				return null;
			}
			set
			{
			}
		}

		internal StackBasedReturnMessage()
		{
		}

		internal void InitFields(Message m)
		{
			_m = m;
			if (_h != null)
			{
				_h.Clear();
			}
			if (_d != null)
			{
				_d.Clear();
			}
		}

		public object GetArg(int argNum)
		{
			return _m.GetArg(argNum);
		}

		public string GetArgName(int index)
		{
			return _m.GetArgName(index);
		}

		internal LogicalCallContext GetLogicalCallContext()
		{
			return _m.GetLogicalCallContext();
		}

		internal LogicalCallContext SetLogicalCallContext(LogicalCallContext callCtx)
		{
			return _m.SetLogicalCallContext(callCtx);
		}

		public object GetOutArg(int argNum)
		{
			if (_argMapper == null)
			{
				_argMapper = new ArgMapper(this, fOut: true);
			}
			return _argMapper.GetArg(argNum);
		}

		public string GetOutArgName(int index)
		{
			if (_argMapper == null)
			{
				_argMapper = new ArgMapper(this, fOut: true);
			}
			return _argMapper.GetArgName(index);
		}

		void IInternalMessage.SetURI(string val)
		{
			_m.Uri = val;
		}

		void IInternalMessage.SetCallContext(LogicalCallContext newCallContext)
		{
			_m.SetLogicalCallContext(newCallContext);
		}

		bool IInternalMessage.HasProperties()
		{
			return _h != null;
		}
	}
}
