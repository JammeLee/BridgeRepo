using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace System
{
	[Serializable]
	[ComVisible(true)]
	public class MissingMemberException : MemberAccessException, ISerializable
	{
		protected string ClassName;

		protected string MemberName;

		protected byte[] Signature;

		public override string Message
		{
			get
			{
				if (ClassName == null)
				{
					return base.Message;
				}
				return string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("MissingMember_Name", ClassName + "." + MemberName + ((Signature != null) ? (" " + FormatSignature(Signature)) : "")));
			}
		}

		public MissingMemberException()
			: base(Environment.GetResourceString("Arg_MissingMemberException"))
		{
			SetErrorCode(-2146233070);
		}

		public MissingMemberException(string message)
			: base(message)
		{
			SetErrorCode(-2146233070);
		}

		public MissingMemberException(string message, Exception inner)
			: base(message, inner)
		{
			SetErrorCode(-2146233070);
		}

		protected MissingMemberException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
			ClassName = info.GetString("MMClassName");
			MemberName = info.GetString("MMMemberName");
			Signature = (byte[])info.GetValue("MMSignature", typeof(byte[]));
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern string FormatSignature(byte[] signature);

		private MissingMemberException(string className, string memberName, byte[] signature)
		{
			ClassName = className;
			MemberName = memberName;
			Signature = signature;
		}

		public MissingMemberException(string className, string memberName)
		{
			ClassName = className;
			MemberName = memberName;
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			base.GetObjectData(info, context);
			info.AddValue("MMClassName", ClassName, typeof(string));
			info.AddValue("MMMemberName", MemberName, typeof(string));
			info.AddValue("MMSignature", Signature, typeof(byte[]));
		}
	}
}
