using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace System
{
	[Serializable]
	[ComVisible(true)]
	public class OperationCanceledException : SystemException
	{
		public OperationCanceledException()
			: base(Environment.GetResourceString("OperationCanceled"))
		{
			SetErrorCode(-2146233029);
		}

		public OperationCanceledException(string message)
			: base(message)
		{
			SetErrorCode(-2146233029);
		}

		public OperationCanceledException(string message, Exception innerException)
			: base(message, innerException)
		{
			SetErrorCode(-2146233029);
		}

		protected OperationCanceledException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}
