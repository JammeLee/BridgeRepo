using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace System
{
	[Serializable]
	[ComVisible(true)]
	public class ArrayTypeMismatchException : SystemException
	{
		public ArrayTypeMismatchException()
			: base(Environment.GetResourceString("Arg_ArrayTypeMismatchException"))
		{
			SetErrorCode(-2146233085);
		}

		public ArrayTypeMismatchException(string message)
			: base(message)
		{
			SetErrorCode(-2146233085);
		}

		public ArrayTypeMismatchException(string message, Exception innerException)
			: base(message, innerException)
		{
			SetErrorCode(-2146233085);
		}

		protected ArrayTypeMismatchException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}
