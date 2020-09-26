using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace System
{
	[Serializable]
	[ComVisible(true)]
	public class NotSupportedException : SystemException
	{
		public NotSupportedException()
			: base(Environment.GetResourceString("Arg_NotSupportedException"))
		{
			SetErrorCode(-2146233067);
		}

		public NotSupportedException(string message)
			: base(message)
		{
			SetErrorCode(-2146233067);
		}

		public NotSupportedException(string message, Exception innerException)
			: base(message, innerException)
		{
			SetErrorCode(-2146233067);
		}

		protected NotSupportedException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}
