using System.Collections;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Metadata;
using System.Security.Permissions;

namespace System.Runtime.Remoting.Messaging
{
	[ComVisible(true)]
	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
	[SecurityPermission(SecurityAction.InheritanceDemand, Flags = SecurityPermissionFlag.Infrastructure)]
	public class ReturnMessage : IMethodReturnMessage, IMethodMessage, IMessage
	{
		internal object _ret;

		internal object _properties;

		internal string _URI;

		internal Exception _e;

		internal object[] _outArgs;

		internal int _outArgsCount;

		internal string _methodName;

		internal string _typeName;

		internal Type[] _methodSignature;

		internal bool _hasVarArgs;

		internal LogicalCallContext _callContext;

		internal ArgMapper _argMapper;

		internal MethodBase _methodBase;

		public string Uri
		{
			get
			{
				return _URI;
			}
			set
			{
				_URI = value;
			}
		}

		public string MethodName => _methodName;

		public string TypeName => _typeName;

		public object MethodSignature
		{
			get
			{
				if (_methodSignature == null && _methodBase != null)
				{
					_methodSignature = Message.GenerateMethodSignature(_methodBase);
				}
				return _methodSignature;
			}
		}

		public MethodBase MethodBase => _methodBase;

		public bool HasVarArgs => _hasVarArgs;

		public int ArgCount
		{
			get
			{
				if (_outArgs == null)
				{
					return _outArgsCount;
				}
				return _outArgs.Length;
			}
		}

		public object[] Args
		{
			get
			{
				if (_outArgs == null)
				{
					return new object[_outArgsCount];
				}
				return _outArgs;
			}
		}

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

		public Exception Exception => _e;

		public virtual object ReturnValue => _ret;

		public virtual IDictionary Properties
		{
			get
			{
				if (_properties == null)
				{
					_properties = new MRMDictionary(this, null);
				}
				return (MRMDictionary)_properties;
			}
		}

		public LogicalCallContext LogicalCallContext => GetLogicalCallContext();

		public ReturnMessage(object ret, object[] outArgs, int outArgsCount, LogicalCallContext callCtx, IMethodCallMessage mcm)
		{
			_ret = ret;
			_outArgs = outArgs;
			_outArgsCount = outArgsCount;
			if (callCtx != null)
			{
				_callContext = callCtx;
			}
			else
			{
				_callContext = CallContext.GetLogicalCallContext();
			}
			if (mcm != null)
			{
				_URI = mcm.Uri;
				_methodName = mcm.MethodName;
				_methodSignature = null;
				_typeName = mcm.TypeName;
				_hasVarArgs = mcm.HasVarArgs;
				_methodBase = mcm.MethodBase;
			}
		}

		public ReturnMessage(Exception e, IMethodCallMessage mcm)
		{
			_e = (IsCustomErrorEnabled() ? new RemotingException(Environment.GetResourceString("Remoting_InternalError")) : e);
			_callContext = CallContext.GetLogicalCallContext();
			if (mcm != null)
			{
				_URI = mcm.Uri;
				_methodName = mcm.MethodName;
				_methodSignature = null;
				_typeName = mcm.TypeName;
				_hasVarArgs = mcm.HasVarArgs;
				_methodBase = mcm.MethodBase;
			}
		}

		public object GetArg(int argNum)
		{
			if (_outArgs == null)
			{
				if (argNum < 0 || argNum >= _outArgsCount)
				{
					throw new ArgumentOutOfRangeException("argNum");
				}
				return null;
			}
			if (argNum < 0 || argNum >= _outArgs.Length)
			{
				throw new ArgumentOutOfRangeException("argNum");
			}
			return _outArgs[argNum];
		}

		public string GetArgName(int index)
		{
			if (_outArgs == null)
			{
				if (index < 0 || index >= _outArgsCount)
				{
					throw new ArgumentOutOfRangeException("index");
				}
			}
			else if (index < 0 || index >= _outArgs.Length)
			{
				throw new ArgumentOutOfRangeException("index");
			}
			if (_methodBase != null)
			{
				RemotingMethodCachedData reflectionCachedData = InternalRemotingServices.GetReflectionCachedData(_methodBase);
				return reflectionCachedData.Parameters[index].Name;
			}
			return "__param" + index;
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

		internal LogicalCallContext GetLogicalCallContext()
		{
			if (_callContext == null)
			{
				_callContext = new LogicalCallContext();
			}
			return _callContext;
		}

		internal LogicalCallContext SetLogicalCallContext(LogicalCallContext ctx)
		{
			LogicalCallContext callContext = _callContext;
			_callContext = ctx;
			return callContext;
		}

		internal bool HasProperties()
		{
			return _properties != null;
		}

		internal static bool IsCustomErrorEnabled()
		{
			object data = CallContext.GetData("__CustomErrorsEnabled");
			if (data != null)
			{
				return (bool)data;
			}
			return false;
		}
	}
}
