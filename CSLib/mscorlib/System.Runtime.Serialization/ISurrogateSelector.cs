using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace System.Runtime.Serialization
{
	[ComVisible(true)]
	public interface ISurrogateSelector
	{
		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
		void ChainSelector(ISurrogateSelector selector);

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
		ISerializationSurrogate GetSurrogate(Type type, StreamingContext context, out ISurrogateSelector selector);

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
		ISurrogateSelector GetNextSelector();
	}
}
