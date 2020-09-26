using System.Globalization;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace System
{
	[Serializable]
	[ComVisible(true)]
	public class ArgumentException : SystemException, ISerializable
	{
		private string m_paramName;

		public override string Message
		{
			get
			{
				string message = base.Message;
				if (m_paramName != null && m_paramName.Length != 0)
				{
					return message + Environment.NewLine + string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Arg_ParamName_Name"), m_paramName);
				}
				return message;
			}
		}

		public virtual string ParamName => m_paramName;

		public ArgumentException()
			: base(Environment.GetResourceString("Arg_ArgumentException"))
		{
			SetErrorCode(-2147024809);
		}

		public ArgumentException(string message)
			: base(message)
		{
			SetErrorCode(-2147024809);
		}

		public ArgumentException(string message, Exception innerException)
			: base(message, innerException)
		{
			SetErrorCode(-2147024809);
		}

		public ArgumentException(string message, string paramName, Exception innerException)
			: base(message, innerException)
		{
			m_paramName = paramName;
			SetErrorCode(-2147024809);
		}

		public ArgumentException(string message, string paramName)
			: base(message)
		{
			m_paramName = paramName;
			SetErrorCode(-2147024809);
		}

		protected ArgumentException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
			m_paramName = info.GetString("ParamName");
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			base.GetObjectData(info, context);
			info.AddValue("ParamName", m_paramName, typeof(string));
		}
	}
}
