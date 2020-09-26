using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace System
{
	[Serializable]
	[ComVisible(true)]
	public class InvalidOperationException : SystemException
	{
		public InvalidOperationException()
			: base(Environment.GetResourceString("Arg_InvalidOperationException"))
		{
			SetErrorCode(-2146233079);
		}

		public InvalidOperationException(string message)
			: base(message)
		{
			SetErrorCode(-2146233079);
		}

		public InvalidOperationException(string message, Exception innerException)
			: base(message, innerException)
		{
			SetErrorCode(-2146233079);
		}

		protected InvalidOperationException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}
