using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace System.Runtime.Serialization.Formatters
{
	[ComVisible(true)]
	public interface IFieldInfo
	{
		string[] FieldNames
		{
			[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
			get;
			[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
			set;
		}

		Type[] FieldTypes
		{
			[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
			get;
			[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
			set;
		}
	}
}
