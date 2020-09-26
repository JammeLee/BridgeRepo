using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace System
{
	[Serializable]
	[ComVisible(true)]
	public class ArgumentNullException : ArgumentException
	{
		public ArgumentNullException()
			: base(Environment.GetResourceString("ArgumentNull_Generic"))
		{
			SetErrorCode(-2147467261);
		}

		public ArgumentNullException(string paramName)
			: base(Environment.GetResourceString("ArgumentNull_Generic"), paramName)
		{
			SetErrorCode(-2147467261);
		}

		public ArgumentNullException(string message, Exception innerException)
			: base(message, innerException)
		{
			SetErrorCode(-2147467261);
		}

		public ArgumentNullException(string paramName, string message)
			: base(message, paramName)
		{
			SetErrorCode(-2147467261);
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
		protected ArgumentNullException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}
