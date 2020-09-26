using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace System.Threading
{
	[Serializable]
	[ComVisible(true)]
	public class SynchronizationLockException : SystemException
	{
		public SynchronizationLockException()
			: base(Environment.GetResourceString("Arg_SynchronizationLockException"))
		{
			SetErrorCode(-2146233064);
		}

		public SynchronizationLockException(string message)
			: base(message)
		{
			SetErrorCode(-2146233064);
		}

		public SynchronizationLockException(string message, Exception innerException)
			: base(message, innerException)
		{
			SetErrorCode(-2146233064);
		}

		protected SynchronizationLockException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}
