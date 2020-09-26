using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace System.Collections.Generic
{
	[Serializable]
	[ComVisible(true)]
	public class KeyNotFoundException : SystemException, ISerializable
	{
		public KeyNotFoundException()
			: base(Environment.GetResourceString("Arg_KeyNotFound"))
		{
			SetErrorCode(-2146232969);
		}

		public KeyNotFoundException(string message)
			: base(message)
		{
			SetErrorCode(-2146232969);
		}

		public KeyNotFoundException(string message, Exception innerException)
			: base(message, innerException)
		{
			SetErrorCode(-2146232969);
		}

		protected KeyNotFoundException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}
