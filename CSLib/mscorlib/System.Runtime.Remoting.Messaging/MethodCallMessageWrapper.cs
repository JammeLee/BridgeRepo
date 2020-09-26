using System.Collections;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace System.Runtime.Remoting.Messaging
{
	[ComVisible(true)]
	[SecurityPermission(SecurityAction.InheritanceDemand, Flags = SecurityPermissionFlag.Infrastructure)]
	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
	public class MethodCallMessageWrapper : InternalMessageWrapper, IMethodCallMessage, IMethodMessage, IMessage
	{
		private class MCMWrapperDictionary : Hashtable
		{
			private IMethodCallMessage _mcmsg;

			private IDictionary _idict;

			public override object this[object key]
			{
				get
				{
					string text = key as string;
					if (text != null)
					{
						switch (text)
						{
						case "__Uri":
							return _mcmsg.Uri;
						case "__MethodName":
							return _mcmsg.MethodName;
						case "__MethodSignature":
							return _mcmsg.MethodSignature;
						case "__TypeName":
							return _mcmsg.TypeName;
						case "__Args":
							return _mcmsg.Args;
						}
					}
					return _idict[key];
				}
				set
				{
					string text = key as string;
					if (text != null)
					{
						switch (text)
						{
						case "__MethodName":
						case "__MethodSignature":
						case "__TypeName":
						case "__Args":
							throw new RemotingException(Environment.GetResourceString("Remoting_Default"));
						}
						_idict[key] = value;
					}
				}
			}

			public MCMWrapperDictionary(IMethodCallMessage msg, IDictionary idict)
			{
				_mcmsg = msg;
				_idict = idict;
			}
		}

		private IMethodCallMessage _msg;

		private IDictionary _properties;

		private ArgMapper _argMapper;

		private object[] _args;

		public virtual string Uri
		{
			get
			{
				return _msg.Uri;
			}
			set
			{
				_msg.Properties[Message.UriKey] = value;
			}
		}

		public virtual string MethodName => _msg.MethodName;

		public virtual string TypeName => _msg.TypeName;

		public virtual object MethodSignature => _msg.MethodSignature;

		public virtual LogicalCallContext LogicalCallContext => _msg.LogicalCallContext;

		public virtual MethodBase MethodBase => _msg.MethodBase;

		public virtual int ArgCount
		{
			get
			{
				if (_args != null)
				{
					return _args.Length;
				}
				return 0;
			}
		}

		public virtual object[] Args
		{
			get
			{
				return _args;
			}
			set
			{
				_args = value;
			}
		}

		public virtual bool HasVarArgs => _msg.HasVarArgs;

		public virtual int InArgCount
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

		public virtual object[] InArgs
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

		public virtual IDictionary Properties
		{
			get
			{
				if (_properties == null)
				{
					_properties = new MCMWrapperDictionary(this, _msg.Properties);
				}
				return _properties;
			}
		}

		public MethodCallMessageWrapper(IMethodCallMessage msg)
			: base(msg)
		{
			_msg = msg;
			_args = _msg.Args;
		}

		public virtual string GetArgName(int index)
		{
			return _msg.GetArgName(index);
		}

		public virtual object GetArg(int argNum)
		{
			return _args[argNum];
		}

		public virtual object GetInArg(int argNum)
		{
			if (_argMapper == null)
			{
				_argMapper = new ArgMapper(this, fOut: false);
			}
			return _argMapper.GetArg(argNum);
		}

		public virtual string GetInArgName(int index)
		{
			if (_argMapper == null)
			{
				_argMapper = new ArgMapper(this, fOut: false);
			}
			return _argMapper.GetArgName(index);
		}
	}
}
