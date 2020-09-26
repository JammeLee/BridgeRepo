using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Permissions;

namespace System.IO
{
	[Serializable]
	[ComVisible(true)]
	public class FileLoadException : IOException
	{
		private string _fileName;

		private string _fusionLog;

		public override string Message
		{
			get
			{
				SetMessageField();
				return _message;
			}
		}

		public string FileName => _fileName;

		public string FusionLog
		{
			[SecurityPermission(SecurityAction.Demand, Flags = (SecurityPermissionFlag.ControlEvidence | SecurityPermissionFlag.ControlPolicy))]
			get
			{
				return _fusionLog;
			}
		}

		public FileLoadException()
			: base(Environment.GetResourceString("IO.FileLoad"))
		{
			SetErrorCode(-2146232799);
		}

		public FileLoadException(string message)
			: base(message)
		{
			SetErrorCode(-2146232799);
		}

		public FileLoadException(string message, Exception inner)
			: base(message, inner)
		{
			SetErrorCode(-2146232799);
		}

		public FileLoadException(string message, string fileName)
			: base(message)
		{
			SetErrorCode(-2146232799);
			_fileName = fileName;
		}

		public FileLoadException(string message, string fileName, Exception inner)
			: base(message, inner)
		{
			SetErrorCode(-2146232799);
			_fileName = fileName;
		}

		private void SetMessageField()
		{
			if (_message == null)
			{
				_message = FormatFileLoadExceptionMessage(_fileName, base.HResult);
			}
		}

		public override string ToString()
		{
			string text = GetType().FullName + ": " + Message;
			if (_fileName != null && _fileName.Length != 0)
			{
				text = text + Environment.NewLine + string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("IO.FileName_Name"), _fileName);
			}
			if (base.InnerException != null)
			{
				text = text + " ---> " + base.InnerException.ToString();
			}
			if (StackTrace != null)
			{
				text = text + Environment.NewLine + StackTrace;
			}
			try
			{
				if (FusionLog != null)
				{
					if (text == null)
					{
						text = " ";
					}
					text += Environment.NewLine;
					text += Environment.NewLine;
					text += FusionLog;
					return text;
				}
				return text;
			}
			catch (SecurityException)
			{
				return text;
			}
		}

		protected FileLoadException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
			_fileName = info.GetString("FileLoad_FileName");
			try
			{
				_fusionLog = info.GetString("FileLoad_FusionLog");
			}
			catch
			{
				_fusionLog = null;
			}
		}

		private FileLoadException(string fileName, string fusionLog, int hResult)
			: base(null)
		{
			SetErrorCode(hResult);
			_fileName = fileName;
			_fusionLog = fusionLog;
			SetMessageField();
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);
			info.AddValue("FileLoad_FileName", _fileName, typeof(string));
			try
			{
				info.AddValue("FileLoad_FusionLog", FusionLog, typeof(string));
			}
			catch (SecurityException)
			{
			}
		}

		internal static string FormatFileLoadExceptionMessage(string fileName, int hResult)
		{
			return string.Format(CultureInfo.CurrentCulture, GetFileLoadExceptionMessage(hResult), fileName, GetMessageForHR(hResult));
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern string GetFileLoadExceptionMessage(int hResult);

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern string GetMessageForHR(int hresult);
	}
}
