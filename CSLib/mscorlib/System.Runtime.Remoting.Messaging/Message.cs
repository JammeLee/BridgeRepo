using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Metadata;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Runtime.Remoting.Proxies;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Threading;

namespace System.Runtime.Remoting.Messaging
{
	[Serializable]
	internal class Message : IMethodCallMessage, IMethodMessage, IMessage, IInternalMessage, ISerializable
	{
		internal const int Sync = 0;

		internal const int BeginAsync = 1;

		internal const int EndAsync = 2;

		internal const int Ctor = 4;

		internal const int OneWay = 8;

		internal const int CallMask = 15;

		internal const int FixedArgs = 16;

		internal const int VarArgs = 32;

		private string _MethodName;

		private Type[] _MethodSignature;

		private MethodBase _MethodBase;

		private object _properties;

		private string _URI;

		private string _typeName;

		private Exception _Fault;

		private Identity _ID;

		private ServerIdentity _srvID;

		private ArgMapper _argMapper;

		private LogicalCallContext _callContext;

		private IntPtr _frame;

		private IntPtr _methodDesc;

		private IntPtr _metaSigHolder;

		private IntPtr _delegateMD;

		private IntPtr _governingType;

		private int _flags;

		private bool _initDone;

		internal static string CallContextKey = "__CallContext";

		internal static string UriKey = "__Uri";

		ServerIdentity IInternalMessage.ServerIdentityObject
		{
			get
			{
				return _srvID;
			}
			set
			{
				_srvID = value;
			}
		}

		Identity IInternalMessage.IdentityObject
		{
			get
			{
				return _ID;
			}
			set
			{
				_ID = value;
			}
		}

		public IDictionary Properties
		{
			get
			{
				if (_properties == null)
				{
					Interlocked.CompareExchange(ref _properties, new MCMDictionary(this, null), null);
				}
				return (IDictionary)_properties;
			}
		}

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

		public bool HasVarArgs
		{
			get
			{
				if ((_flags & 0x10) == 0 && (_flags & 0x20) == 0)
				{
					if (!InternalHasVarArgs())
					{
						_flags |= 16;
					}
					else
					{
						_flags |= 32;
					}
				}
				return 1 == (_flags & 0x20);
			}
		}

		public int ArgCount => InternalGetArgCount();

		public object[] Args => InternalGetArgs();

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

		public string MethodName
		{
			get
			{
				if (_MethodName == null)
				{
					UpdateNames();
				}
				return _MethodName;
			}
		}

		public string TypeName
		{
			get
			{
				if (_typeName == null)
				{
					UpdateNames();
				}
				return _typeName;
			}
		}

		public object MethodSignature
		{
			get
			{
				if (_MethodSignature == null)
				{
					_MethodSignature = GenerateMethodSignature(GetMethodBase());
				}
				return _MethodSignature;
			}
		}

		public LogicalCallContext LogicalCallContext => GetLogicalCallContext();

		public MethodBase MethodBase => GetMethodBase();

		public virtual Exception GetFault()
		{
			return _Fault;
		}

		public virtual void SetFault(Exception e)
		{
			_Fault = e;
		}

		internal virtual void SetOneWay()
		{
			_flags |= 8;
		}

		public virtual int GetCallType()
		{
			InitIfNecessary();
			return _flags;
		}

		internal IntPtr GetFramePtr()
		{
			return _frame;
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		public extern void GetAsyncBeginInfo(out AsyncCallback acbd, out object state);

		[MethodImpl(MethodImplOptions.InternalCall)]
		public extern object GetThisPtr();

		[MethodImpl(MethodImplOptions.InternalCall)]
		public extern IAsyncResult GetAsyncResult();

		public void Init()
		{
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		public extern object GetReturnValue();

		internal Message()
		{
		}

		internal void InitFields(MessageData msgData)
		{
			_frame = msgData.pFrame;
			_delegateMD = msgData.pDelegateMD;
			_methodDesc = msgData.pMethodDesc;
			_flags = msgData.iFlags;
			_initDone = true;
			_metaSigHolder = msgData.pSig;
			_governingType = msgData.thGoverningType;
			_MethodName = null;
			_MethodSignature = null;
			_MethodBase = null;
			_URI = null;
			_Fault = null;
			_ID = null;
			_srvID = null;
			_callContext = null;
			if (_properties != null)
			{
				((IDictionary)_properties).Clear();
			}
		}

		private void InitIfNecessary()
		{
			if (!_initDone)
			{
				Init();
				_initDone = true;
			}
		}

		void IInternalMessage.SetURI(string URI)
		{
			_URI = URI;
		}

		void IInternalMessage.SetCallContext(LogicalCallContext callContext)
		{
			_callContext = callContext;
		}

		bool IInternalMessage.HasProperties()
		{
			return _properties != null;
		}

		public object GetArg(int argNum)
		{
			return InternalGetArg(argNum);
		}

		public string GetArgName(int index)
		{
			if (index >= ArgCount)
			{
				throw new ArgumentOutOfRangeException("index");
			}
			RemotingMethodCachedData reflectionCachedData = InternalRemotingServices.GetReflectionCachedData(GetMethodBase());
			ParameterInfo[] parameters = reflectionCachedData.Parameters;
			if (index < parameters.Length)
			{
				return parameters[index].Name;
			}
			return "VarArg" + (index - parameters.Length);
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

		private void UpdateNames()
		{
			RemotingMethodCachedData reflectionCachedData = InternalRemotingServices.GetReflectionCachedData(GetMethodBase());
			_typeName = reflectionCachedData.TypeAndAssemblyName;
			_MethodName = reflectionCachedData.MethodName;
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			throw new NotSupportedException(Environment.GetResourceString("NotSupported_Method"));
		}

		internal unsafe MethodBase GetMethodBase()
		{
			if (_MethodBase == null)
			{
				RuntimeMethodHandle methodHandle = new RuntimeMethodHandle((void*)_methodDesc);
				RuntimeTypeHandle reflectedTypeHandle = new RuntimeTypeHandle((void*)_governingType);
				_MethodBase = RuntimeType.GetMethodBase(reflectedTypeHandle, methodHandle);
			}
			return _MethodBase;
		}

		internal LogicalCallContext SetLogicalCallContext(LogicalCallContext callCtx)
		{
			LogicalCallContext callContext = _callContext;
			_callContext = callCtx;
			return callContext;
		}

		internal LogicalCallContext GetLogicalCallContext()
		{
			if (_callContext == null)
			{
				_callContext = new LogicalCallContext();
			}
			return _callContext;
		}

		internal static Type[] GenerateMethodSignature(MethodBase mb)
		{
			RemotingMethodCachedData reflectionCachedData = InternalRemotingServices.GetReflectionCachedData(mb);
			ParameterInfo[] parameters = reflectionCachedData.Parameters;
			Type[] array = new Type[parameters.Length];
			for (int i = 0; i < parameters.Length; i++)
			{
				array[i] = parameters[i].ParameterType;
			}
			return array;
		}

		internal static object[] CoerceArgs(IMethodMessage m)
		{
			MethodBase methodBase = m.MethodBase;
			RemotingMethodCachedData reflectionCachedData = InternalRemotingServices.GetReflectionCachedData(methodBase);
			return CoerceArgs(m, reflectionCachedData.Parameters);
		}

		internal static object[] CoerceArgs(IMethodMessage m, ParameterInfo[] pi)
		{
			return CoerceArgs(m.MethodBase, m.Args, pi);
		}

		internal static object[] CoerceArgs(MethodBase mb, object[] args, ParameterInfo[] pi)
		{
			if (pi == null)
			{
				throw new ArgumentNullException("pi");
			}
			if (pi.Length != args.Length)
			{
				throw new RemotingException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Remoting_Message_ArgMismatch"), mb.DeclaringType.FullName, mb.Name, args.Length, pi.Length));
			}
			for (int i = 0; i < pi.Length; i++)
			{
				ParameterInfo parameterInfo = pi[i];
				Type parameterType = parameterInfo.ParameterType;
				object obj = args[i];
				if (obj != null)
				{
					args[i] = CoerceArg(obj, parameterType);
				}
				else if (parameterType.IsByRef)
				{
					Type elementType = parameterType.GetElementType();
					if (elementType.IsValueType)
					{
						if (parameterInfo.IsOut)
						{
							args[i] = Activator.CreateInstance(elementType, nonPublic: true);
						}
						else if (!elementType.IsGenericType || elementType.GetGenericTypeDefinition() != typeof(Nullable<>))
						{
							throw new RemotingException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Remoting_Message_MissingArgValue"), elementType.FullName, i));
						}
					}
				}
				else if (parameterType.IsValueType && (!parameterType.IsGenericType || parameterType.GetGenericTypeDefinition() != typeof(Nullable<>)))
				{
					throw new RemotingException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Remoting_Message_MissingArgValue"), parameterType.FullName, i));
				}
			}
			return args;
		}

		internal static object CoerceArg(object value, Type pt)
		{
			object obj = null;
			if (value != null)
			{
				Exception innerException = null;
				try
				{
					if (pt.IsByRef)
					{
						pt = pt.GetElementType();
					}
					obj = ((!pt.IsInstanceOfType(value)) ? Convert.ChangeType(value, pt, CultureInfo.InvariantCulture) : value);
				}
				catch (Exception ex)
				{
					innerException = ex;
				}
				if (obj == null)
				{
					string text = null;
					text = ((!RemotingServices.IsTransparentProxy(value)) ? value.ToString() : typeof(MarshalByRefObject).ToString());
					throw new RemotingException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Remoting_Message_CoercionFailed"), text, pt), innerException);
				}
			}
			return obj;
		}

		internal static object SoapCoerceArg(object value, Type pt, Hashtable keyToNamespaceTable)
		{
			object obj = null;
			if (value != null)
			{
				try
				{
					if (pt.IsByRef)
					{
						pt = pt.GetElementType();
					}
					if (pt.IsInstanceOfType(value))
					{
						obj = value;
					}
					else
					{
						string text = value as string;
						if (text != null)
						{
							if (pt == typeof(double))
							{
								obj = ((text == "INF") ? ((object)double.PositiveInfinity) : ((!(text == "-INF")) ? ((object)double.Parse(text, CultureInfo.InvariantCulture)) : ((object)double.NegativeInfinity)));
							}
							else if (pt == typeof(float))
							{
								obj = ((text == "INF") ? ((object)float.PositiveInfinity) : ((!(text == "-INF")) ? ((object)float.Parse(text, CultureInfo.InvariantCulture)) : ((object)float.NegativeInfinity)));
							}
							else if (SoapType.typeofISoapXsd.IsAssignableFrom(pt))
							{
								if (pt == SoapType.typeofSoapTime)
								{
									obj = SoapTime.Parse(text);
								}
								else if (pt == SoapType.typeofSoapDate)
								{
									obj = SoapDate.Parse(text);
								}
								else if (pt == SoapType.typeofSoapYearMonth)
								{
									obj = SoapYearMonth.Parse(text);
								}
								else if (pt == SoapType.typeofSoapYear)
								{
									obj = SoapYear.Parse(text);
								}
								else if (pt == SoapType.typeofSoapMonthDay)
								{
									obj = SoapMonthDay.Parse(text);
								}
								else if (pt == SoapType.typeofSoapDay)
								{
									obj = SoapDay.Parse(text);
								}
								else if (pt == SoapType.typeofSoapMonth)
								{
									obj = SoapMonth.Parse(text);
								}
								else if (pt == SoapType.typeofSoapHexBinary)
								{
									obj = SoapHexBinary.Parse(text);
								}
								else if (pt == SoapType.typeofSoapBase64Binary)
								{
									obj = SoapBase64Binary.Parse(text);
								}
								else if (pt == SoapType.typeofSoapInteger)
								{
									obj = SoapInteger.Parse(text);
								}
								else if (pt == SoapType.typeofSoapPositiveInteger)
								{
									obj = SoapPositiveInteger.Parse(text);
								}
								else if (pt == SoapType.typeofSoapNonPositiveInteger)
								{
									obj = SoapNonPositiveInteger.Parse(text);
								}
								else if (pt == SoapType.typeofSoapNonNegativeInteger)
								{
									obj = SoapNonNegativeInteger.Parse(text);
								}
								else if (pt == SoapType.typeofSoapNegativeInteger)
								{
									obj = SoapNegativeInteger.Parse(text);
								}
								else if (pt == SoapType.typeofSoapAnyUri)
								{
									obj = SoapAnyUri.Parse(text);
								}
								else if (pt == SoapType.typeofSoapQName)
								{
									obj = SoapQName.Parse(text);
									SoapQName soapQName = (SoapQName)obj;
									if (soapQName.Key.Length == 0)
									{
										soapQName.Namespace = (string)keyToNamespaceTable["xmlns"];
									}
									else
									{
										soapQName.Namespace = (string)keyToNamespaceTable["xmlns:" + soapQName.Key];
									}
								}
								else if (pt == SoapType.typeofSoapNotation)
								{
									obj = SoapNotation.Parse(text);
								}
								else if (pt == SoapType.typeofSoapNormalizedString)
								{
									obj = SoapNormalizedString.Parse(text);
								}
								else if (pt == SoapType.typeofSoapToken)
								{
									obj = SoapToken.Parse(text);
								}
								else if (pt == SoapType.typeofSoapLanguage)
								{
									obj = SoapLanguage.Parse(text);
								}
								else if (pt == SoapType.typeofSoapName)
								{
									obj = SoapName.Parse(text);
								}
								else if (pt == SoapType.typeofSoapIdrefs)
								{
									obj = SoapIdrefs.Parse(text);
								}
								else if (pt == SoapType.typeofSoapEntities)
								{
									obj = SoapEntities.Parse(text);
								}
								else if (pt == SoapType.typeofSoapNmtoken)
								{
									obj = SoapNmtoken.Parse(text);
								}
								else if (pt == SoapType.typeofSoapNmtokens)
								{
									obj = SoapNmtokens.Parse(text);
								}
								else if (pt == SoapType.typeofSoapNcName)
								{
									obj = SoapNcName.Parse(text);
								}
								else if (pt == SoapType.typeofSoapId)
								{
									obj = SoapId.Parse(text);
								}
								else if (pt == SoapType.typeofSoapIdref)
								{
									obj = SoapIdref.Parse(text);
								}
								else if (pt == SoapType.typeofSoapEntity)
								{
									obj = SoapEntity.Parse(text);
								}
							}
							else if (pt != typeof(bool))
							{
								obj = ((pt == typeof(DateTime)) ? ((object)SoapDateTime.Parse(text)) : (pt.IsPrimitive ? Convert.ChangeType(value, pt, CultureInfo.InvariantCulture) : ((pt == typeof(TimeSpan)) ? ((object)SoapDuration.Parse(text)) : ((pt != typeof(char)) ? Convert.ChangeType(value, pt, CultureInfo.InvariantCulture) : ((object)text[0])))));
							}
							else
							{
								switch (text)
								{
								case "1":
								case "true":
									obj = true;
									break;
								case "0":
								case "false":
									obj = false;
									break;
								default:
									throw new RemotingException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Remoting_Message_CoercionFailed"), text, pt));
								}
							}
						}
						else
						{
							obj = Convert.ChangeType(value, pt, CultureInfo.InvariantCulture);
						}
					}
				}
				catch (Exception)
				{
				}
				if (obj == null)
				{
					string text2 = null;
					text2 = ((!RemotingServices.IsTransparentProxy(value)) ? value.ToString() : typeof(MarshalByRefObject).ToString());
					throw new RemotingException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Remoting_Message_CoercionFailed"), text2, pt));
				}
			}
			return obj;
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal extern bool InternalHasVarArgs();

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal extern int InternalGetArgCount();

		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern object InternalGetArg(int argNum);

		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern object[] InternalGetArgs();

		[MethodImpl(MethodImplOptions.InternalCall)]
		public extern void PropagateOutParameters(object[] OutArgs, object retVal);

		[MethodImpl(MethodImplOptions.InternalCall)]
		public extern bool Dispatch(object target, bool fExecuteInContext);

		[Conditional("_REMOTING_DEBUG")]
		public static void DebugOut(string s)
		{
			OutToUnmanagedDebugger("\nRMTING: Thrd " + Thread.CurrentThread.GetHashCode() + " : " + s);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern void OutToUnmanagedDebugger(string s);

		internal static LogicalCallContext PropagateCallContextFromMessageToThread(IMessage msg)
		{
			return CallContext.SetLogicalCallContext((LogicalCallContext)msg.Properties[CallContextKey]);
		}

		internal static void PropagateCallContextFromThreadToMessage(IMessage msg)
		{
			LogicalCallContext logicalCallContext = CallContext.GetLogicalCallContext();
			msg.Properties[CallContextKey] = logicalCallContext;
		}

		internal static void PropagateCallContextFromThreadToMessage(IMessage msg, LogicalCallContext oldcctx)
		{
			PropagateCallContextFromThreadToMessage(msg);
			CallContext.SetLogicalCallContext(oldcctx);
		}
	}
}
