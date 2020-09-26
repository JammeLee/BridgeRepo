using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;

namespace System
{
	[Serializable]
	[ComDefaultInterface(typeof(_Exception))]
	[ClassInterface(ClassInterfaceType.None)]
	[ComVisible(true)]
	public class Exception : ISerializable, _Exception
	{
		internal enum ExceptionMessageKind
		{
			ThreadAbort = 1,
			ThreadInterrupted,
			OutOfMemory
		}

		private const int _COMPlusExceptionCode = -532459699;

		private string _className;

		private MethodBase _exceptionMethod;

		private string _exceptionMethodString;

		internal string _message;

		private IDictionary _data;

		private Exception _innerException;

		private string _helpURL;

		private object _stackTrace;

		private string _stackTraceString;

		private string _remoteStackTraceString;

		private int _remoteStackIndex;

		private object _dynamicMethods;

		internal int _HResult;

		private string _source;

		private IntPtr _xptrs;

		private int _xcode;

		public virtual string Message
		{
			get
			{
				if (_message == null)
				{
					if (_className == null)
					{
						_className = GetClassName();
					}
					return string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Exception_WasThrown"), _className);
				}
				return _message;
			}
		}

		public virtual IDictionary Data => GetDataInternal();

		public Exception InnerException => _innerException;

		public MethodBase TargetSite => GetTargetSiteInternal();

		public virtual string StackTrace
		{
			get
			{
				if (_stackTraceString != null)
				{
					return _remoteStackTraceString + _stackTraceString;
				}
				if (_stackTrace == null)
				{
					return _remoteStackTraceString;
				}
				string stackTrace = Environment.GetStackTrace(this, needFileInfo: true);
				return _remoteStackTraceString + stackTrace;
			}
		}

		public virtual string HelpLink
		{
			get
			{
				return _helpURL;
			}
			set
			{
				_helpURL = value;
			}
		}

		public virtual string Source
		{
			get
			{
				if (_source == null)
				{
					StackTrace stackTrace = new StackTrace(this, fNeedFileInfo: true);
					if (stackTrace.FrameCount > 0)
					{
						StackFrame frame = stackTrace.GetFrame(0);
						MethodBase method = frame.GetMethod();
						_source = method.Module.Assembly.nGetSimpleName();
					}
				}
				return _source;
			}
			set
			{
				_source = value;
			}
		}

		protected int HResult
		{
			get
			{
				return _HResult;
			}
			set
			{
				_HResult = value;
			}
		}

		internal bool IsTransient => nIsTransient(_HResult);

		public Exception()
		{
			_message = null;
			_stackTrace = null;
			_dynamicMethods = null;
			HResult = -2146233088;
			_xcode = -532459699;
			_xptrs = (IntPtr)0;
		}

		public Exception(string message)
		{
			_message = message;
			_stackTrace = null;
			_dynamicMethods = null;
			HResult = -2146233088;
			_xcode = -532459699;
			_xptrs = (IntPtr)0;
		}

		public Exception(string message, Exception innerException)
		{
			_message = message;
			_stackTrace = null;
			_dynamicMethods = null;
			_innerException = innerException;
			HResult = -2146233088;
			_xcode = -532459699;
			_xptrs = (IntPtr)0;
		}

		protected Exception(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			_className = info.GetString("ClassName");
			_message = info.GetString("Message");
			_data = (IDictionary)info.GetValueNoThrow("Data", typeof(IDictionary));
			_innerException = (Exception)info.GetValue("InnerException", typeof(Exception));
			_helpURL = info.GetString("HelpURL");
			_stackTraceString = info.GetString("StackTraceString");
			_remoteStackTraceString = info.GetString("RemoteStackTraceString");
			_remoteStackIndex = info.GetInt32("RemoteStackIndex");
			_exceptionMethodString = (string)info.GetValue("ExceptionMethod", typeof(string));
			HResult = info.GetInt32("HResult");
			_source = info.GetString("Source");
			if (_className == null || HResult == 0)
			{
				throw new SerializationException(Environment.GetResourceString("Serialization_InsufficientState"));
			}
			if (context.State == StreamingContextStates.CrossAppDomain)
			{
				_remoteStackTraceString += _stackTraceString;
				_stackTraceString = null;
			}
		}

		internal IDictionary GetDataInternal()
		{
			if (_data == null)
			{
				if (IsImmutableAgileException(this))
				{
					_data = new EmptyReadOnlyDictionaryInternal();
				}
				else
				{
					_data = new ListDictionaryInternal();
				}
			}
			return _data;
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern bool IsImmutableAgileException(Exception e);

		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern string GetClassName();

		public virtual Exception GetBaseException()
		{
			Exception innerException = InnerException;
			Exception result = this;
			while (innerException != null)
			{
				result = innerException;
				innerException = innerException.InnerException;
			}
			return result;
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private unsafe static extern void* _InternalGetMethod(object stackTrace);

		private unsafe static RuntimeMethodHandle InternalGetMethod(object stackTrace)
		{
			return new RuntimeMethodHandle(_InternalGetMethod(stackTrace));
		}

		private MethodBase GetTargetSiteInternal()
		{
			if (_exceptionMethod != null)
			{
				return _exceptionMethod;
			}
			if (_stackTrace == null)
			{
				return null;
			}
			if (_exceptionMethodString != null)
			{
				_exceptionMethod = GetExceptionMethodFromString();
			}
			else
			{
				RuntimeMethodHandle typicalMethodDefinition = InternalGetMethod(_stackTrace).GetTypicalMethodDefinition();
				_exceptionMethod = RuntimeType.GetMethodBase(typicalMethodDefinition);
			}
			return _exceptionMethod;
		}

		internal void SetErrorCode(int hr)
		{
			HResult = hr;
		}

		public override string ToString()
		{
			string message = Message;
			if (_className == null)
			{
				_className = GetClassName();
			}
			string text = ((message != null && message.Length > 0) ? (_className + ": " + message) : _className);
			if (_innerException != null)
			{
				text = text + " ---> " + _innerException.ToString() + Environment.NewLine + "   " + Environment.GetResourceString("Exception_EndOfInnerExceptionStack");
			}
			if (StackTrace != null)
			{
				text = text + Environment.NewLine + StackTrace;
			}
			return text;
		}

		private string GetExceptionMethodString()
		{
			MethodBase targetSiteInternal = GetTargetSiteInternal();
			if (targetSiteInternal == null)
			{
				return null;
			}
			if (targetSiteInternal is DynamicMethod.RTDynamicMethod)
			{
				return null;
			}
			char value = '\n';
			StringBuilder stringBuilder = new StringBuilder();
			if (targetSiteInternal is ConstructorInfo)
			{
				RuntimeConstructorInfo runtimeConstructorInfo = (RuntimeConstructorInfo)targetSiteInternal;
				Type reflectedType = runtimeConstructorInfo.ReflectedType;
				stringBuilder.Append(1);
				stringBuilder.Append(value);
				stringBuilder.Append(runtimeConstructorInfo.Name);
				if (reflectedType != null)
				{
					stringBuilder.Append(value);
					stringBuilder.Append(reflectedType.Assembly.FullName);
					stringBuilder.Append(value);
					stringBuilder.Append(reflectedType.FullName);
				}
				stringBuilder.Append(value);
				stringBuilder.Append(runtimeConstructorInfo.ToString());
			}
			else
			{
				RuntimeMethodInfo runtimeMethodInfo = (RuntimeMethodInfo)targetSiteInternal;
				Type declaringType = runtimeMethodInfo.DeclaringType;
				stringBuilder.Append(8);
				stringBuilder.Append(value);
				stringBuilder.Append(runtimeMethodInfo.Name);
				stringBuilder.Append(value);
				stringBuilder.Append(runtimeMethodInfo.Module.Assembly.FullName);
				stringBuilder.Append(value);
				if (declaringType != null)
				{
					stringBuilder.Append(declaringType.FullName);
					stringBuilder.Append(value);
				}
				stringBuilder.Append(runtimeMethodInfo.ToString());
			}
			return stringBuilder.ToString();
		}

		private MethodBase GetExceptionMethodFromString()
		{
			string[] array = _exceptionMethodString.Split('\0', '\n');
			if (array.Length != 5)
			{
				throw new SerializationException();
			}
			SerializationInfo serializationInfo = new SerializationInfo(typeof(MemberInfoSerializationHolder), new FormatterConverter());
			serializationInfo.AddValue("MemberType", int.Parse(array[0], CultureInfo.InvariantCulture), typeof(int));
			serializationInfo.AddValue("Name", array[1], typeof(string));
			serializationInfo.AddValue("AssemblyName", array[2], typeof(string));
			serializationInfo.AddValue("ClassName", array[3]);
			serializationInfo.AddValue("Signature", array[4]);
			StreamingContext context = new StreamingContext(StreamingContextStates.All);
			try
			{
				return (MethodBase)new MemberInfoSerializationHolder(serializationInfo, context).GetRealObject(context);
			}
			catch (SerializationException)
			{
				return null;
			}
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
		public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			string text = _stackTraceString;
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			if (_className == null)
			{
				_className = GetClassName();
			}
			if (_stackTrace != null)
			{
				if (text == null)
				{
					text = Environment.GetStackTrace(this, needFileInfo: true);
				}
				if (_exceptionMethod == null)
				{
					RuntimeMethodHandle typicalMethodDefinition = InternalGetMethod(_stackTrace).GetTypicalMethodDefinition();
					_exceptionMethod = RuntimeType.GetMethodBase(typicalMethodDefinition);
				}
			}
			if (_source == null)
			{
				_source = Source;
			}
			info.AddValue("ClassName", _className, typeof(string));
			info.AddValue("Message", _message, typeof(string));
			info.AddValue("Data", _data, typeof(IDictionary));
			info.AddValue("InnerException", _innerException, typeof(Exception));
			info.AddValue("HelpURL", _helpURL, typeof(string));
			info.AddValue("StackTraceString", text, typeof(string));
			info.AddValue("RemoteStackTraceString", _remoteStackTraceString, typeof(string));
			info.AddValue("RemoteStackIndex", _remoteStackIndex, typeof(int));
			info.AddValue("ExceptionMethod", GetExceptionMethodString(), typeof(string));
			info.AddValue("HResult", HResult);
			info.AddValue("Source", _source, typeof(string));
		}

		internal Exception PrepForRemoting()
		{
			string text = null;
			text = (_remoteStackTraceString = ((_remoteStackIndex != 0) ? (StackTrace + Environment.NewLine + Environment.NewLine + "Exception rethrown at [" + _remoteStackIndex + "]: " + Environment.NewLine) : (Environment.NewLine + "Server stack trace: " + Environment.NewLine + StackTrace + Environment.NewLine + Environment.NewLine + "Exception rethrown at [" + _remoteStackIndex + "]: " + Environment.NewLine)));
			_remoteStackIndex++;
			return this;
		}

		internal void InternalPreserveStackTrace()
		{
			string stackTrace = StackTrace;
			if (stackTrace != null && stackTrace.Length > 0)
			{
				_remoteStackTraceString = stackTrace + Environment.NewLine;
			}
			_stackTrace = null;
			_stackTraceString = null;
		}

		internal virtual string InternalToString()
		{
			try
			{
				SecurityPermission securityPermission = new SecurityPermission(SecurityPermissionFlag.ControlEvidence | SecurityPermissionFlag.ControlPolicy);
				securityPermission.Assert();
			}
			catch
			{
			}
			return ToString();
		}

		public new Type GetType()
		{
			return base.GetType();
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern bool nIsTransient(int hr);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern string GetMessageFromNativeResources(ExceptionMessageKind kind);
	}
}
