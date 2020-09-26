using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace System
{
	[Serializable]
	[ComVisible(true)]
	public class FieldAccessException : MemberAccessException
	{
		public FieldAccessException()
			: base(Environment.GetResourceString("Arg_FieldAccessException"))
		{
			SetErrorCode(-2146233081);
		}

		public FieldAccessException(string message)
			: base(message)
		{
			SetErrorCode(-2146233081);
		}

		public FieldAccessException(string message, Exception inner)
			: base(message, inner)
		{
			SetErrorCode(-2146233081);
		}

		protected FieldAccessException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}
