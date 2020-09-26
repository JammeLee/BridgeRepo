using System.Globalization;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace System
{
	[Serializable]
	[ComVisible(true)]
	public class ObjectDisposedException : InvalidOperationException
	{
		private string objectName;

		public override string Message
		{
			get
			{
				string text = ObjectName;
				if (text == null || text.Length == 0)
				{
					return base.Message;
				}
				return base.Message + Environment.NewLine + string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("ObjectDisposed_ObjectName_Name"), text);
			}
		}

		public string ObjectName
		{
			get
			{
				if (objectName == null)
				{
					return string.Empty;
				}
				return objectName;
			}
		}

		private ObjectDisposedException()
			: this(null, Environment.GetResourceString("ObjectDisposed_Generic"))
		{
		}

		public ObjectDisposedException(string objectName)
			: this(objectName, Environment.GetResourceString("ObjectDisposed_Generic"))
		{
		}

		public ObjectDisposedException(string objectName, string message)
			: base(message)
		{
			SetErrorCode(-2146232798);
			this.objectName = objectName;
		}

		public ObjectDisposedException(string message, Exception innerException)
			: base(message, innerException)
		{
			SetErrorCode(-2146232798);
		}

		protected ObjectDisposedException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
			objectName = info.GetString("ObjectName");
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);
			info.AddValue("ObjectName", ObjectName, typeof(string));
		}
	}
}
