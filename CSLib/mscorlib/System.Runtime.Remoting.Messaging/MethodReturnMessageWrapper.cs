using System.Collections;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace System.Runtime.Remoting.Messaging
{
	[ComVisible(true)]
	[SecurityPermission(SecurityAction.InheritanceDemand, Flags = SecurityPermissionFlag.Infrastructure)]
	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
	public class MethodReturnMessageWrapper : InternalMessageWrapper, IMethodReturnMessage, IMethodMessage, IMessage
	{
		private class MRMWrapperDictionary : Hashtable
		{
			private IMethodReturnMessage _mrmsg;

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
							return _mrmsg.Uri;
						case "__MethodName":
							return _mrmsg.MethodName;
						case "__MethodSignature":
							return _mrmsg.MethodSignature;
						case "__TypeName":
							return _mrmsg.TypeName;
						case "__Return":
							return _mrmsg.ReturnValue;
						case "__OutArgs":
							return _mrmsg.OutArgs;
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
						case "__Return":
						case "__OutArgs":
							throw new RemotingException(Environment.GetResourceString("Remoting_Default"));
						}
						_idict[key] = value;
					}
				}
			}

			public MRMWrapperDictionary(IMethodReturnMessage msg, IDictionary idict)
			{
				_mrmsg = msg;
				_idict = idict;
			}
		}

		private IMethodReturnMessage _msg;

		private IDictionary _properties;

		private ArgMapper _argMapper;

		private object[] _args;

		private object _returnValue;

		private Exception _exception;

		public string Uri
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

		public virtual int OutArgCount
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

		public virtual object[] OutArgs
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

		public virtual Exception Exception
		{
			get
			{
				return _exception;
			}
			set
			{
				_exception = value;
			}
		}

		public virtual object ReturnValue
		{
			get
			{
				return _returnValue;
			}
			set
			{
				_returnValue = value;
			}
		}

		public virtual IDictionary Properties
		{
			get
			{
				if (_properties == null)
				{
					_properties = new MRMWrapperDictionary(this, _msg.Properties);
				}
				return _properties;
			}
		}

		public MethodReturnMessageWrapper(IMethodReturnMessage msg)
			: base(msg)
		{
			_msg = msg;
			_args = _msg.Args;
			_returnValue = _msg.ReturnValue;
			_exception = _msg.Exception;
		}

		public virtual string GetArgName(int index)
		{
			return _msg.GetArgName(index);
		}

		public virtual object GetArg(int argNum)
		{
			return _args[argNum];
		}

		public virtual object GetOutArg(int argNum)
		{
			if (_argMapper == null)
			{
				_argMapper = new ArgMapper(this, fOut: true);
			}
			return _argMapper.GetArg(argNum);
		}

		public virtual string GetOutArgName(int index)
		{
			if (_argMapper == null)
			{
				_argMapper = new ArgMapper(this, fOut: true);
			}
			return _argMapper.GetArgName(index);
		}
	}
}
