#define TRACE
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Threading;

namespace System.Net
{
	internal class Logging
	{
		private class NclTraceSource : TraceSource
		{
			internal NclTraceSource(string name)
				: base(name)
			{
			}

			protected internal override string[] GetSupportedAttributes()
			{
				return SupportedAttributes;
			}
		}

		private const int DefaultMaxDumpSize = 1024;

		private const bool DefaultUseProtocolTextOnly = false;

		private const string AttributeNameMaxSize = "maxdatasize";

		private const string AttributeNameTraceMode = "tracemode";

		private const string AttributeValueProtocolOnly = "protocolonly";

		private const string TraceSourceWebName = "System.Net";

		private const string TraceSourceHttpListenerName = "System.Net.HttpListener";

		private const string TraceSourceSocketsName = "System.Net.Sockets";

		private const string TraceSourceCacheName = "System.Net.Cache";

		private static bool s_LoggingEnabled = true;

		private static bool s_LoggingInitialized;

		private static bool s_AppDomainShutdown;

		private static readonly string[] SupportedAttributes = new string[2]
		{
			"maxdatasize",
			"tracemode"
		};

		private static TraceSource s_WebTraceSource;

		private static TraceSource s_HttpListenerTraceSource;

		private static TraceSource s_SocketsTraceSource;

		private static TraceSource s_CacheTraceSource;

		private static object s_InternalSyncObject;

		private static object InternalSyncObject
		{
			get
			{
				if (s_InternalSyncObject == null)
				{
					object value = new object();
					Interlocked.CompareExchange(ref s_InternalSyncObject, value, null);
				}
				return s_InternalSyncObject;
			}
		}

		internal static bool On
		{
			get
			{
				if (!s_LoggingInitialized)
				{
					InitializeLogging();
				}
				return s_LoggingEnabled;
			}
		}

		internal static TraceSource Web
		{
			get
			{
				if (!s_LoggingInitialized)
				{
					InitializeLogging();
				}
				if (!s_LoggingEnabled)
				{
					return null;
				}
				return s_WebTraceSource;
			}
		}

		internal static TraceSource HttpListener
		{
			get
			{
				if (!s_LoggingInitialized)
				{
					InitializeLogging();
				}
				if (!s_LoggingEnabled)
				{
					return null;
				}
				return s_HttpListenerTraceSource;
			}
		}

		internal static TraceSource Sockets
		{
			get
			{
				if (!s_LoggingInitialized)
				{
					InitializeLogging();
				}
				if (!s_LoggingEnabled)
				{
					return null;
				}
				return s_SocketsTraceSource;
			}
		}

		internal static TraceSource RequestCache
		{
			get
			{
				if (!s_LoggingInitialized)
				{
					InitializeLogging();
				}
				if (!s_LoggingEnabled)
				{
					return null;
				}
				return s_CacheTraceSource;
			}
		}

		private Logging()
		{
		}

		internal static bool IsVerbose(TraceSource traceSource)
		{
			return ValidateSettings(traceSource, TraceEventType.Verbose);
		}

		private static bool GetUseProtocolTextSetting(TraceSource traceSource)
		{
			bool result = false;
			if (traceSource.Attributes["tracemode"] == "protocolonly")
			{
				result = true;
			}
			return result;
		}

		private static int GetMaxDumpSizeSetting(TraceSource traceSource)
		{
			int result = 1024;
			if (traceSource.Attributes.ContainsKey("maxdatasize"))
			{
				try
				{
					result = int.Parse(traceSource.Attributes["maxdatasize"], NumberFormatInfo.InvariantInfo);
					return result;
				}
				catch (Exception ex)
				{
					if (ex is ThreadAbortException || ex is StackOverflowException || ex is OutOfMemoryException)
					{
						throw;
					}
					traceSource.Attributes["maxdatasize"] = result.ToString(NumberFormatInfo.InvariantInfo);
					return result;
				}
			}
			return result;
		}

		private static void InitializeLogging()
		{
			lock (InternalSyncObject)
			{
				if (!s_LoggingInitialized)
				{
					bool flag = false;
					s_WebTraceSource = new NclTraceSource("System.Net");
					s_HttpListenerTraceSource = new NclTraceSource("System.Net.HttpListener");
					s_SocketsTraceSource = new NclTraceSource("System.Net.Sockets");
					s_CacheTraceSource = new NclTraceSource("System.Net.Cache");
					if (s_WebTraceSource.Switch.ShouldTrace(TraceEventType.Critical) || s_HttpListenerTraceSource.Switch.ShouldTrace(TraceEventType.Critical) || s_SocketsTraceSource.Switch.ShouldTrace(TraceEventType.Critical) || s_CacheTraceSource.Switch.ShouldTrace(TraceEventType.Critical))
					{
						flag = true;
						AppDomain currentDomain = AppDomain.CurrentDomain;
						currentDomain.UnhandledException += UnhandledExceptionHandler;
						currentDomain.DomainUnload += AppDomainUnloadEvent;
						currentDomain.ProcessExit += ProcessExitEvent;
					}
					s_LoggingEnabled = flag;
					s_LoggingInitialized = true;
				}
			}
		}

		private static void Close()
		{
			if (s_WebTraceSource != null)
			{
				s_WebTraceSource.Close();
			}
			if (s_HttpListenerTraceSource != null)
			{
				s_HttpListenerTraceSource.Close();
			}
			if (s_SocketsTraceSource != null)
			{
				s_SocketsTraceSource.Close();
			}
			if (s_CacheTraceSource != null)
			{
				s_CacheTraceSource.Close();
			}
		}

		private static void UnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs args)
		{
			Exception e = (Exception)args.ExceptionObject;
			Exception(Web, sender, "UnhandledExceptionHandler", e);
		}

		private static void ProcessExitEvent(object sender, EventArgs e)
		{
			Close();
			s_AppDomainShutdown = true;
		}

		private static void AppDomainUnloadEvent(object sender, EventArgs e)
		{
			Close();
			s_AppDomainShutdown = true;
		}

		private static bool ValidateSettings(TraceSource traceSource, TraceEventType traceLevel)
		{
			if (!s_LoggingEnabled)
			{
				return false;
			}
			if (!s_LoggingInitialized)
			{
				InitializeLogging();
			}
			if (traceSource == null || !traceSource.Switch.ShouldTrace(traceLevel))
			{
				return false;
			}
			if (s_AppDomainShutdown)
			{
				return false;
			}
			return true;
		}

		private static string GetObjectName(object obj)
		{
			string text = obj.ToString();
			try
			{
				if (!(obj is Uri))
				{
					int num = text.LastIndexOf('.') + 1;
					return text.Substring(num, text.Length - num);
				}
				return text;
			}
			catch (Exception ex)
			{
				if (ex is ThreadAbortException || ex is StackOverflowException || ex is OutOfMemoryException)
				{
					throw;
				}
				return text;
			}
		}

		internal static uint GetThreadId()
		{
			uint num = UnsafeNclNativeMethods.GetCurrentThreadId();
			if (num == 0)
			{
				num = (uint)Thread.CurrentThread.GetHashCode();
			}
			return num;
		}

		internal static void PrintLine(TraceSource traceSource, TraceEventType eventType, int id, string msg)
		{
			string str = "[" + GetThreadId().ToString("d4", CultureInfo.InvariantCulture) + "] ";
			traceSource.TraceEvent(eventType, id, str + msg);
		}

		internal static void Associate(TraceSource traceSource, object objA, object objB)
		{
			if (ValidateSettings(traceSource, TraceEventType.Information))
			{
				string str = GetObjectName(objA) + "#" + ValidationHelper.HashString(objA);
				string str2 = GetObjectName(objB) + "#" + ValidationHelper.HashString(objB);
				PrintLine(traceSource, TraceEventType.Information, 0, "Associating " + str + " with " + str2);
			}
		}

		internal static void Enter(TraceSource traceSource, object obj, string method, string param)
		{
			if (ValidateSettings(traceSource, TraceEventType.Information))
			{
				Enter(traceSource, GetObjectName(obj) + "#" + ValidationHelper.HashString(obj), method, param);
			}
		}

		internal static void Enter(TraceSource traceSource, object obj, string method, object paramObject)
		{
			if (ValidateSettings(traceSource, TraceEventType.Information))
			{
				Enter(traceSource, GetObjectName(obj) + "#" + ValidationHelper.HashString(obj), method, paramObject);
			}
		}

		internal static void Enter(TraceSource traceSource, string obj, string method, string param)
		{
			if (ValidateSettings(traceSource, TraceEventType.Information))
			{
				Enter(traceSource, obj + "::" + method + "(" + param + ")");
			}
		}

		internal static void Enter(TraceSource traceSource, string obj, string method, object paramObject)
		{
			if (ValidateSettings(traceSource, TraceEventType.Information))
			{
				string text = "";
				if (paramObject != null)
				{
					text = GetObjectName(paramObject) + "#" + ValidationHelper.HashString(paramObject);
				}
				Enter(traceSource, obj + "::" + method + "(" + text + ")");
			}
		}

		internal static void Enter(TraceSource traceSource, string method, string parameters)
		{
			if (ValidateSettings(traceSource, TraceEventType.Information))
			{
				Enter(traceSource, method + "(" + parameters + ")");
			}
		}

		internal static void Enter(TraceSource traceSource, string msg)
		{
			if (ValidateSettings(traceSource, TraceEventType.Information))
			{
				PrintLine(traceSource, TraceEventType.Verbose, 0, msg);
			}
		}

		internal static void Exit(TraceSource traceSource, object obj, string method, object retObject)
		{
			if (ValidateSettings(traceSource, TraceEventType.Information))
			{
				string retValue = "";
				if (retObject != null)
				{
					retValue = GetObjectName(retObject) + "#" + ValidationHelper.HashString(retObject);
				}
				Exit(traceSource, obj, method, retValue);
			}
		}

		internal static void Exit(TraceSource traceSource, string obj, string method, object retObject)
		{
			if (ValidateSettings(traceSource, TraceEventType.Information))
			{
				string retValue = "";
				if (retObject != null)
				{
					retValue = GetObjectName(retObject) + "#" + ValidationHelper.HashString(retObject);
				}
				Exit(traceSource, obj, method, retValue);
			}
		}

		internal static void Exit(TraceSource traceSource, object obj, string method, string retValue)
		{
			if (ValidateSettings(traceSource, TraceEventType.Information))
			{
				Exit(traceSource, GetObjectName(obj) + "#" + ValidationHelper.HashString(obj), method, retValue);
			}
		}

		internal static void Exit(TraceSource traceSource, string obj, string method, string retValue)
		{
			if (ValidateSettings(traceSource, TraceEventType.Information))
			{
				if (!ValidationHelper.IsBlankString(retValue))
				{
					retValue = "\t-> " + retValue;
				}
				Exit(traceSource, obj + "::" + method + "() " + retValue);
			}
		}

		internal static void Exit(TraceSource traceSource, string method, string parameters)
		{
			if (ValidateSettings(traceSource, TraceEventType.Information))
			{
				Exit(traceSource, method + "() " + parameters);
			}
		}

		internal static void Exit(TraceSource traceSource, string msg)
		{
			if (ValidateSettings(traceSource, TraceEventType.Information))
			{
				PrintLine(traceSource, TraceEventType.Verbose, 0, "Exiting " + msg);
			}
		}

		internal static void Exception(TraceSource traceSource, object obj, string method, Exception e)
		{
			if (ValidateSettings(traceSource, TraceEventType.Error))
			{
				string str = "Exception in the " + GetObjectName(obj) + "#" + ValidationHelper.HashString(obj) + "::" + method + " - ";
				PrintLine(traceSource, TraceEventType.Error, 0, str + e.Message);
				if (!ValidationHelper.IsBlankString(e.StackTrace))
				{
					PrintLine(traceSource, TraceEventType.Error, 0, e.StackTrace);
				}
			}
		}

		internal static void PrintInfo(TraceSource traceSource, string msg)
		{
			if (ValidateSettings(traceSource, TraceEventType.Information))
			{
				PrintLine(traceSource, TraceEventType.Information, 0, msg);
			}
		}

		internal static void PrintInfo(TraceSource traceSource, object obj, string msg)
		{
			if (ValidateSettings(traceSource, TraceEventType.Information))
			{
				PrintLine(traceSource, TraceEventType.Information, 0, GetObjectName(obj) + "#" + ValidationHelper.HashString(obj) + " - " + msg);
			}
		}

		internal static void PrintInfo(TraceSource traceSource, object obj, string method, string param)
		{
			if (ValidateSettings(traceSource, TraceEventType.Information))
			{
				PrintLine(traceSource, TraceEventType.Information, 0, GetObjectName(obj) + "#" + ValidationHelper.HashString(obj) + "::" + method + "(" + param + ")");
			}
		}

		internal static void PrintWarning(TraceSource traceSource, string msg)
		{
			if (ValidateSettings(traceSource, TraceEventType.Warning))
			{
				PrintLine(traceSource, TraceEventType.Warning, 0, msg);
			}
		}

		internal static void PrintWarning(TraceSource traceSource, object obj, string method, string msg)
		{
			if (ValidateSettings(traceSource, TraceEventType.Warning))
			{
				PrintLine(traceSource, TraceEventType.Warning, 0, GetObjectName(obj) + "#" + ValidationHelper.HashString(obj) + "::" + method + "() - " + msg);
			}
		}

		internal static void PrintError(TraceSource traceSource, string msg)
		{
			if (ValidateSettings(traceSource, TraceEventType.Error))
			{
				PrintLine(traceSource, TraceEventType.Error, 0, msg);
			}
		}

		internal static void PrintError(TraceSource traceSource, object obj, string method, string msg)
		{
			if (ValidateSettings(traceSource, TraceEventType.Error))
			{
				PrintLine(traceSource, TraceEventType.Error, 0, GetObjectName(obj) + "#" + ValidationHelper.HashString(obj) + "::" + method + "() - " + msg);
			}
		}

		internal static void Dump(TraceSource traceSource, object obj, string method, IntPtr bufferPtr, int length)
		{
			if (ValidateSettings(traceSource, TraceEventType.Verbose) && !(bufferPtr == IntPtr.Zero) && length >= 0)
			{
				byte[] array = new byte[length];
				Marshal.Copy(bufferPtr, array, 0, length);
				Dump(traceSource, obj, method, array, 0, length);
			}
		}

		internal static void Dump(TraceSource traceSource, object obj, string method, byte[] buffer, int offset, int length)
		{
			if (!ValidateSettings(traceSource, TraceEventType.Verbose))
			{
				return;
			}
			if (buffer == null)
			{
				PrintLine(traceSource, TraceEventType.Verbose, 0, "(null)");
				return;
			}
			if (offset > buffer.Length)
			{
				PrintLine(traceSource, TraceEventType.Verbose, 0, "(offset out of range)");
				return;
			}
			PrintLine(traceSource, TraceEventType.Verbose, 0, "Data from " + GetObjectName(obj) + "#" + ValidationHelper.HashString(obj) + "::" + method);
			int maxDumpSizeSetting = GetMaxDumpSizeSetting(traceSource);
			if (length > maxDumpSizeSetting)
			{
				PrintLine(traceSource, TraceEventType.Verbose, 0, "(printing " + maxDumpSizeSetting.ToString(NumberFormatInfo.InvariantInfo) + " out of " + length.ToString(NumberFormatInfo.InvariantInfo) + ")");
				length = maxDumpSizeSetting;
			}
			if (length < 0 || length > buffer.Length - offset)
			{
				length = buffer.Length - offset;
			}
			if (GetUseProtocolTextSetting(traceSource))
			{
				string msg = "<<" + WebHeaderCollection.HeaderEncoding.GetString(buffer, offset, length) + ">>";
				PrintLine(traceSource, TraceEventType.Verbose, 0, msg);
				return;
			}
			do
			{
				int num = Math.Min(length, 16);
				string text = string.Format(CultureInfo.CurrentCulture, "{0:X8} : ", offset);
				for (int i = 0; i < num; i++)
				{
					text = text + string.Format(CultureInfo.CurrentCulture, "{0:X2}", buffer[offset + i]) + ((i == 7) ? '-' : ' ');
				}
				for (int j = num; j < 16; j++)
				{
					text += "   ";
				}
				text += ": ";
				for (int k = 0; k < num; k++)
				{
					text += (char)((buffer[offset + k] < 32 || buffer[offset + k] > 126) ? 46 : buffer[offset + k]);
				}
				PrintLine(traceSource, TraceEventType.Verbose, 0, text);
				offset += num;
				length -= num;
			}
			while (length > 0);
		}
	}
}
