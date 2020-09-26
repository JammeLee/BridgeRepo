using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace System
{
	[Serializable]
	[ComVisible(true)]
	public class MethodAccessException : MemberAccessException
	{
		public MethodAccessException()
			: base(Environment.GetResourceString("Arg_MethodAccessException"))
		{
			SetErrorCode(-2146233072);
		}

		public MethodAccessException(string message)
			: base(message)
		{
			SetErrorCode(-2146233072);
		}

		public MethodAccessException(string message, Exception inner)
			: base(message, inner)
		{
			SetErrorCode(-2146233072);
		}

		protected MethodAccessException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}
