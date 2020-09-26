using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Security.Permissions;

namespace System.Runtime.Remoting.Messaging
{
	[Serializable]
	internal class SerializationMonkey : ISerializable, IFieldInfo
	{
		internal ISerializationRootObject _obj;

		internal string[] fieldNames;

		internal Type[] fieldTypes;

		public string[] FieldNames
		{
			get
			{
				return fieldNames;
			}
			set
			{
				fieldNames = value;
			}
		}

		public Type[] FieldTypes
		{
			get
			{
				return fieldTypes;
			}
			set
			{
				fieldTypes = value;
			}
		}

		internal SerializationMonkey(SerializationInfo info, StreamingContext ctx)
		{
			_obj.RootSetObjectData(info, ctx);
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			throw new NotSupportedException(Environment.GetResourceString("NotSupported_Method"));
		}
	}
}
