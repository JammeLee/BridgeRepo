using System.Globalization;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace System
{
	[Serializable]
	[ComVisible(true)]
	public class MissingFieldException : MissingMemberException, ISerializable
	{
		public override string Message
		{
			get
			{
				if (ClassName == null)
				{
					return base.Message;
				}
				return string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("MissingField_Name", ((Signature != null) ? (MissingMemberException.FormatSignature(Signature) + " ") : "") + ClassName + "." + MemberName));
			}
		}

		public MissingFieldException()
			: base(Environment.GetResourceString("Arg_MissingFieldException"))
		{
			SetErrorCode(-2146233071);
		}

		public MissingFieldException(string message)
			: base(message)
		{
			SetErrorCode(-2146233071);
		}

		public MissingFieldException(string message, Exception inner)
			: base(message, inner)
		{
			SetErrorCode(-2146233071);
		}

		protected MissingFieldException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

		private MissingFieldException(string className, string fieldName, byte[] signature)
		{
			ClassName = className;
			MemberName = fieldName;
			Signature = signature;
		}

		public MissingFieldException(string className, string fieldName)
		{
			ClassName = className;
			MemberName = fieldName;
		}
	}
}
