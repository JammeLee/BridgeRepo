using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Permissions;

namespace System
{
	[Serializable]
	[ComVisible(true)]
	public class BadImageFormatException : SystemException
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

		public BadImageFormatException()
			: base(Environment.GetResourceString("Arg_BadImageFormatException"))
		{
			SetErrorCode(-2147024885);
		}

		public BadImageFormatException(string message)
			: base(message)
		{
			SetErrorCode(-2147024885);
		}

		public BadImageFormatException(string message, Exception inner)
			: base(message, inner)
		{
			SetErrorCode(-2147024885);
		}

		public BadImageFormatException(string message, string fileName)
			: base(message)
		{
			SetErrorCode(-2147024885);
			_fileName = fileName;
		}

		public BadImageFormatException(string message, string fileName, Exception inner)
			: base(message, inner)
		{
			SetErrorCode(-2147024885);
			_fileName = fileName;
		}

		private void SetMessageField()
		{
			if (_message == null)
			{
				if (_fileName == null && base.HResult == -2146233088)
				{
					_message = Environment.GetResourceString("Arg_BadImageFormatException");
				}
				else
				{
					_message = FileLoadException.FormatFileLoadExceptionMessage(_fileName, base.HResult);
				}
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

		protected BadImageFormatException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
			_fileName = info.GetString("BadImageFormat_FileName");
			try
			{
				_fusionLog = info.GetString("BadImageFormat_FusionLog");
			}
			catch
			{
				_fusionLog = null;
			}
		}

		private BadImageFormatException(string fileName, string fusionLog, int hResult)
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
			info.AddValue("BadImageFormat_FileName", _fileName, typeof(string));
			try
			{
				info.AddValue("BadImageFormat_FusionLog", FusionLog, typeof(string));
			}
			catch (SecurityException)
			{
			}
		}
	}
}
