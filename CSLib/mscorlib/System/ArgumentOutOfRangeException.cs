using System.Globalization;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace System
{
	[Serializable]
	[ComVisible(true)]
	public class ArgumentOutOfRangeException : ArgumentException, ISerializable
	{
		private static string _rangeMessage;

		private object m_actualValue;

		private static string RangeMessage
		{
			get
			{
				if (_rangeMessage == null)
				{
					_rangeMessage = Environment.GetResourceString("Arg_ArgumentOutOfRangeException");
				}
				return _rangeMessage;
			}
		}

		public override string Message
		{
			get
			{
				string message = base.Message;
				if (m_actualValue != null)
				{
					string text = string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("ArgumentOutOfRange_ActualValue"), m_actualValue.ToString());
					if (message == null)
					{
						return text;
					}
					return message + Environment.NewLine + text;
				}
				return message;
			}
		}

		public virtual object ActualValue => m_actualValue;

		public ArgumentOutOfRangeException()
			: base(RangeMessage)
		{
			SetErrorCode(-2146233086);
		}

		public ArgumentOutOfRangeException(string paramName)
			: base(RangeMessage, paramName)
		{
			SetErrorCode(-2146233086);
		}

		public ArgumentOutOfRangeException(string paramName, string message)
			: base(message, paramName)
		{
			SetErrorCode(-2146233086);
		}

		public ArgumentOutOfRangeException(string message, Exception innerException)
			: base(message, innerException)
		{
			SetErrorCode(-2146233086);
		}

		public ArgumentOutOfRangeException(string paramName, object actualValue, string message)
			: base(message, paramName)
		{
			m_actualValue = actualValue;
			SetErrorCode(-2146233086);
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			base.GetObjectData(info, context);
			info.AddValue("ActualValue", m_actualValue, typeof(object));
		}

		protected ArgumentOutOfRangeException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
			m_actualValue = info.GetValue("ActualValue", typeof(object));
		}
	}
}
