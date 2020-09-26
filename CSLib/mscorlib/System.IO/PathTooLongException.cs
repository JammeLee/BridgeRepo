using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace System.IO
{
	[Serializable]
	[ComVisible(true)]
	public class PathTooLongException : IOException
	{
		public PathTooLongException()
			: base(Environment.GetResourceString("IO.PathTooLong"))
		{
			SetErrorCode(-2147024690);
		}

		public PathTooLongException(string message)
			: base(message)
		{
			SetErrorCode(-2147024690);
		}

		public PathTooLongException(string message, Exception innerException)
			: base(message, innerException)
		{
			SetErrorCode(-2147024690);
		}

		protected PathTooLongException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}
