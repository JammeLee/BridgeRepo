using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace System.Reflection
{
	[Serializable]
	[ComVisible(true)]
	public class TargetException : ApplicationException
	{
		public TargetException()
		{
			SetErrorCode(-2146232829);
		}

		public TargetException(string message)
			: base(message)
		{
			SetErrorCode(-2146232829);
		}

		public TargetException(string message, Exception inner)
			: base(message, inner)
		{
			SetErrorCode(-2146232829);
		}

		protected TargetException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}
