using System.Globalization;
using System.Runtime.Serialization;

namespace System.Runtime.InteropServices
{
	[Serializable]
	[ComVisible(true)]
	public class COMException : ExternalException
	{
		public COMException()
			: base(Environment.GetResourceString("Arg_COMException"))
		{
			SetErrorCode(-2147467259);
		}

		public COMException(string message)
			: base(message)
		{
			SetErrorCode(-2147467259);
		}

		public COMException(string message, Exception inner)
			: base(message, inner)
		{
			SetErrorCode(-2147467259);
		}

		public COMException(string message, int errorCode)
			: base(message)
		{
			SetErrorCode(errorCode);
		}

		protected COMException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

		public override string ToString()
		{
			string message = Message;
			string str = GetType().ToString();
			string text = str + " (0x" + base.HResult.ToString("X8", CultureInfo.InvariantCulture) + ")";
			if (message != null && message.Length > 0)
			{
				text = text + ": " + message;
			}
			Exception innerException = base.InnerException;
			if (innerException != null)
			{
				text = text + " ---> " + innerException.ToString();
			}
			if (StackTrace != null)
			{
				text = text + Environment.NewLine + StackTrace;
			}
			return text;
		}
	}
}
