using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace System.Runtime.Serialization
{
	[ComVisible(true)]
	public interface IObjectReference
	{
		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
		object GetRealObject(StreamingContext context);
	}
}
