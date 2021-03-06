using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace System
{
	[Serializable]
	[ComVisible(true)]
	public class TypeLoadException : SystemException, ISerializable
	{
		private string ClassName;

		private string AssemblyName;

		private string MessageArg;

		internal int ResourceId;

		public override string Message
		{
			get
			{
				SetMessageField();
				return _message;
			}
		}

		public string TypeName
		{
			get
			{
				if (ClassName == null)
				{
					return string.Empty;
				}
				return ClassName;
			}
		}

		public TypeLoadException()
			: base(Environment.GetResourceString("Arg_TypeLoadException"))
		{
			SetErrorCode(-2146233054);
		}

		public TypeLoadException(string message)
			: base(message)
		{
			SetErrorCode(-2146233054);
		}

		public TypeLoadException(string message, Exception inner)
			: base(message, inner)
		{
			SetErrorCode(-2146233054);
		}

		private void SetMessageField()
		{
			if (_message != null)
			{
				return;
			}
			if (ClassName == null && ResourceId == 0)
			{
				_message = Environment.GetResourceString("Arg_TypeLoadException");
				return;
			}
			if (AssemblyName == null)
			{
				AssemblyName = Environment.GetResourceString("IO_UnknownFileName");
			}
			if (ClassName == null)
			{
				ClassName = Environment.GetResourceString("IO_UnknownFileName");
			}
			_message = string.Format(CultureInfo.CurrentCulture, GetTypeLoadExceptionMessage(ResourceId), ClassName, AssemblyName, MessageArg);
		}

		private TypeLoadException(string className, string assemblyName, string messageArg, int resourceId)
			: base(null)
		{
			SetErrorCode(-2146233054);
			ClassName = className;
			AssemblyName = assemblyName;
			MessageArg = messageArg;
			ResourceId = resourceId;
			SetMessageField();
		}

		protected TypeLoadException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			ClassName = info.GetString("TypeLoadClassName");
			AssemblyName = info.GetString("TypeLoadAssemblyName");
			MessageArg = info.GetString("TypeLoadMessageArg");
			ResourceId = info.GetInt32("TypeLoadResourceID");
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern string GetTypeLoadExceptionMessage(int resourceId);

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			base.GetObjectData(info, context);
			info.AddValue("TypeLoadClassName", ClassName, typeof(string));
			info.AddValue("TypeLoadAssemblyName", AssemblyName, typeof(string));
			info.AddValue("TypeLoadMessageArg", MessageArg, typeof(string));
			info.AddValue("TypeLoadResourceID", ResourceId);
		}
	}
}
