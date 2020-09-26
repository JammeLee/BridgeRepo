using System.Runtime.Serialization;

namespace System.Runtime.InteropServices
{
	[Serializable]
	[ComVisible(true)]
	public class ExternalException : SystemException
	{
		public virtual int ErrorCode => base.HResult;

		public ExternalException()
			: base(Environment.GetResourceString("Arg_ExternalException"))
		{
			SetErrorCode(-2147467259);
		}

		public ExternalException(string message)
			: base(message)
		{
			SetErrorCode(-2147467259);
		}

		public ExternalException(string message, Exception inner)
			: base(message, inner)
		{
			SetErrorCode(-2147467259);
		}

		public ExternalException(string message, int errorCode)
			: base(message)
		{
			SetErrorCode(errorCode);
		}

		protected ExternalException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}
