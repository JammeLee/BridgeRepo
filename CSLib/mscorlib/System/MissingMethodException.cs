using System.Globalization;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace System
{
	[Serializable]
	[ComVisible(true)]
	public class MissingMethodException : MissingMemberException, ISerializable
	{
		public override string Message
		{
			get
			{
				if (ClassName == null)
				{
					return base.Message;
				}
				return string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("MissingMethod_Name", ClassName + "." + MemberName + ((Signature != null) ? (" " + MissingMemberException.FormatSignature(Signature)) : "")));
			}
		}

		public MissingMethodException()
			: base(Environment.GetResourceString("Arg_MissingMethodException"))
		{
			SetErrorCode(-2146233069);
		}

		public MissingMethodException(string message)
			: base(message)
		{
			SetErrorCode(-2146233069);
		}

		public MissingMethodException(string message, Exception inner)
			: base(message, inner)
		{
			SetErrorCode(-2146233069);
		}

		protected MissingMethodException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

		private MissingMethodException(string className, string methodName, byte[] signature)
		{
			ClassName = className;
			MemberName = methodName;
			Signature = signature;
		}

		public MissingMethodException(string className, string methodName)
		{
			ClassName = className;
			MemberName = methodName;
		}
	}
}
