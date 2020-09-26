using System.Runtime.Serialization;

namespace System.Threading
{
	[Serializable]
	public sealed class ThreadStartException : SystemException
	{
		private ThreadStartException()
			: base(Environment.GetResourceString("Arg_ThreadStartException"))
		{
			SetErrorCode(-2146233051);
		}

		private ThreadStartException(Exception reason)
			: base(Environment.GetResourceString("Arg_ThreadStartException"), reason)
		{
			SetErrorCode(-2146233051);
		}

		internal ThreadStartException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}
