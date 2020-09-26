using System.Runtime.InteropServices;
using System.Runtime.Remoting.Lifetime;
using System.Security.Permissions;

namespace System.Runtime.Remoting
{
	[ComVisible(true)]
	[ClassInterface(ClassInterfaceType.AutoDual)]
	public class ObjectHandle : MarshalByRefObject, IObjectHandle
	{
		private object WrappedObject;

		private ObjectHandle()
		{
		}

		public ObjectHandle(object o)
		{
			WrappedObject = o;
		}

		public object Unwrap()
		{
			return WrappedObject;
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
		public override object InitializeLifetimeService()
		{
			MarshalByRefObject marshalByRefObject = WrappedObject as MarshalByRefObject;
			if (marshalByRefObject != null)
			{
				object obj = marshalByRefObject.InitializeLifetimeService();
				if (obj == null)
				{
					return null;
				}
			}
			return (ILease)base.InitializeLifetimeService();
		}
	}
}
