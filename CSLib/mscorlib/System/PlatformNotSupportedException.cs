using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace System
{
	[Serializable]
	[ComVisible(true)]
	public class PlatformNotSupportedException : NotSupportedException
	{
		public PlatformNotSupportedException()
			: base(Environment.GetResourceString("Arg_PlatformNotSupported"))
		{
			SetErrorCode(-2146233031);
		}

		public PlatformNotSupportedException(string message)
			: base(message)
		{
			SetErrorCode(-2146233031);
		}

		public PlatformNotSupportedException(string message, Exception inner)
			: base(message, inner)
		{
			SetErrorCode(-2146233031);
		}

		protected PlatformNotSupportedException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}
