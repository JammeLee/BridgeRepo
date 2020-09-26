using System.Globalization;
using System.IO;
using System.Reflection;
using System.Security.Permissions;
using System.Text;
using Microsoft.Win32;

namespace System.Diagnostics
{
	[HostProtection(SecurityAction.LinkDemand, Synchronization = true)]
	public class DefaultTraceListener : TraceListener
	{
		private const int internalWriteSize = 16384;

		private bool assertUIEnabled;

		private string logFileName;

		private bool settingsInitialized;

		public bool AssertUiEnabled
		{
			get
			{
				if (!settingsInitialized)
				{
					InitializeSettings();
				}
				return assertUIEnabled;
			}
			set
			{
				if (!settingsInitialized)
				{
					InitializeSettings();
				}
				assertUIEnabled = value;
			}
		}

		public string LogFileName
		{
			get
			{
				if (!settingsInitialized)
				{
					InitializeSettings();
				}
				return logFileName;
			}
			set
			{
				if (!settingsInitialized)
				{
					InitializeSettings();
				}
				logFileName = value;
			}
		}

		private static bool UiPermission
		{
			get
			{
				bool result = false;
				try
				{
					new UIPermission(UIPermissionWindow.SafeSubWindows).Demand();
					result = true;
					return result;
				}
				catch
				{
					return result;
				}
			}
		}

		public DefaultTraceListener()
			: base("Default")
		{
		}

		public override void Fail(string message)
		{
			Fail(message, null);
		}

		public override void Fail(string message, string detailMessage)
		{
			StackTrace stackTrace = new StackTrace(fNeedFileInfo: true);
			int num = 0;
			bool uiPermission = UiPermission;
			string stackTrace2;
			try
			{
				stackTrace2 = StackTraceToString(stackTrace, num, stackTrace.FrameCount - 1);
			}
			catch
			{
				stackTrace2 = "";
			}
			WriteAssert(stackTrace2, message, detailMessage);
			if (AssertUiEnabled && uiPermission)
			{
				AssertWrapper.ShowAssert(stackTrace2, stackTrace.GetFrame(num), message, detailMessage);
			}
		}

		private void InitializeSettings()
		{
			assertUIEnabled = DiagnosticsConfiguration.AssertUIEnabled;
			logFileName = DiagnosticsConfiguration.LogFileName;
			settingsInitialized = true;
		}

		private void WriteAssert(string stackTrace, string message, string detailMessage)
		{
			string message2 = SR.GetString("DebugAssertBanner") + "\r\n" + SR.GetString("DebugAssertShortMessage") + "\r\n" + message + "\r\n" + SR.GetString("DebugAssertLongMessage") + "\r\n" + detailMessage + "\r\n" + stackTrace;
			WriteLine(message2);
		}

		private void WriteToLogFile(string message, bool useWriteLine)
		{
			try
			{
				FileInfo fileInfo = new FileInfo(LogFileName);
				using Stream stream = fileInfo.Open(FileMode.OpenOrCreate);
				using StreamWriter streamWriter = new StreamWriter(stream);
				stream.Position = stream.Length;
				if (useWriteLine)
				{
					streamWriter.WriteLine(message);
				}
				else
				{
					streamWriter.Write(message);
				}
			}
			catch (Exception ex)
			{
				WriteLine(SR.GetString("ExceptionOccurred", LogFileName, ex.ToString()), useLogFile: false);
			}
			catch
			{
				WriteLine(SR.GetString("ExceptionOccurred", LogFileName, ""), useLogFile: false);
			}
		}

		private string StackTraceToString(StackTrace trace, int startFrameIndex, int endFrameIndex)
		{
			StringBuilder stringBuilder = new StringBuilder(512);
			for (int i = startFrameIndex; i <= endFrameIndex; i++)
			{
				StackFrame frame = trace.GetFrame(i);
				MethodBase method = frame.GetMethod();
				stringBuilder.Append("\r\n    at ");
				if (method.ReflectedType != null)
				{
					stringBuilder.Append(method.ReflectedType.Name);
				}
				else
				{
					stringBuilder.Append("<Module>");
				}
				stringBuilder.Append(".");
				stringBuilder.Append(method.Name);
				stringBuilder.Append("(");
				ParameterInfo[] parameters = method.GetParameters();
				for (int j = 0; j < parameters.Length; j++)
				{
					ParameterInfo parameterInfo = parameters[j];
					if (j > 0)
					{
						stringBuilder.Append(", ");
					}
					stringBuilder.Append(parameterInfo.ParameterType.Name);
					stringBuilder.Append(" ");
					stringBuilder.Append(parameterInfo.Name);
				}
				stringBuilder.Append(")  ");
				stringBuilder.Append(frame.GetFileName());
				int fileLineNumber = frame.GetFileLineNumber();
				if (fileLineNumber > 0)
				{
					stringBuilder.Append("(");
					stringBuilder.Append(fileLineNumber.ToString(CultureInfo.InvariantCulture));
					stringBuilder.Append(")");
				}
			}
			stringBuilder.Append("\r\n");
			return stringBuilder.ToString();
		}

		public override void Write(string message)
		{
			Write(message, useLogFile: true);
		}

		private void Write(string message, bool useLogFile)
		{
			if (base.NeedIndent)
			{
				WriteIndent();
			}
			if (message == null || message.Length <= 16384)
			{
				internalWrite(message);
			}
			else
			{
				int i;
				for (i = 0; i < message.Length - 16384; i += 16384)
				{
					internalWrite(message.Substring(i, 16384));
				}
				internalWrite(message.Substring(i));
			}
			if (useLogFile && LogFileName.Length != 0)
			{
				WriteToLogFile(message, useWriteLine: false);
			}
		}

		private void internalWrite(string message)
		{
			if (Debugger.IsLogging())
			{
				Debugger.Log(0, null, message);
			}
			else if (message == null)
			{
				Microsoft.Win32.SafeNativeMethods.OutputDebugString(string.Empty);
			}
			else
			{
				Microsoft.Win32.SafeNativeMethods.OutputDebugString(message);
			}
		}

		public override void WriteLine(string message)
		{
			WriteLine(message, useLogFile: true);
		}

		private void WriteLine(string message, bool useLogFile)
		{
			if (base.NeedIndent)
			{
				WriteIndent();
			}
			Write(message + "\r\n", useLogFile);
			base.NeedIndent = true;
		}
	}
}
