using System.Runtime.Serialization;
using System.Security.Permissions;

namespace System.Runtime.Remoting.Messaging
{
	internal interface ISerializationRootObject
	{
		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
		void RootSetObjectData(SerializationInfo info, StreamingContext ctx);
	}
}
