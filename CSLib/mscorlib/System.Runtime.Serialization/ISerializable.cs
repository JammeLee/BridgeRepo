using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace System.Runtime.Serialization
{
	[ComVisible(true)]
	public interface ISerializable
	{
		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
		void GetObjectData(SerializationInfo info, StreamingContext context);
	}
}
