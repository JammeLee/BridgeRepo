using System.Runtime.InteropServices;
using System.Runtime.Remoting.Activation;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;
using System.Security.Permissions;

namespace System.Runtime.Remoting.Services
{
	[ComVisible(true)]
	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
	public sealed class EnterpriseServicesHelper
	{
		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static object WrapIUnknownWithComObject(IntPtr punk)
		{
			return Marshal.InternalWrapIUnknownWithComObject(punk);
		}

		[ComVisible(true)]
		public static IConstructionReturnMessage CreateConstructionReturnMessage(IConstructionCallMessage ctorMsg, MarshalByRefObject retObj)
		{
			IConstructionReturnMessage constructionReturnMessage = null;
			return new ConstructorReturnMessage(retObj, null, 0, null, ctorMsg);
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static void SwitchWrappers(RealProxy oldcp, RealProxy newcp)
		{
			object transparentProxy = oldcp.GetTransparentProxy();
			object transparentProxy2 = newcp.GetTransparentProxy();
			RemotingServices.GetServerContextForProxy(transparentProxy);
			RemotingServices.GetServerContextForProxy(transparentProxy2);
			Marshal.InternalSwitchCCW(transparentProxy, transparentProxy2);
		}
	}
}
