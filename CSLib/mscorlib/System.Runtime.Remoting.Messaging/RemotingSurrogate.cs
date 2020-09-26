using System.Runtime.Remoting.Proxies;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace System.Runtime.Remoting.Messaging
{
	internal class RemotingSurrogate : ISerializationSurrogate
	{
		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
		public virtual void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
		{
			if (obj == null)
			{
				throw new ArgumentNullException("obj");
			}
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			if (RemotingServices.IsTransparentProxy(obj))
			{
				RealProxy realProxy = RemotingServices.GetRealProxy(obj);
				realProxy.GetObjectData(info, context);
			}
			else
			{
				RemotingServices.GetObjectData(obj, info, context);
			}
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
		public virtual object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
		{
			throw new NotSupportedException(Environment.GetResourceString("NotSupported_PopulateData"));
		}
	}
}
