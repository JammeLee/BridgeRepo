using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace System.Runtime.Remoting.Activation
{
	[ComVisible(true)]
	public interface IActivator
	{
		IActivator NextActivator
		{
			[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
			get;
			[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
			set;
		}

		ActivatorLevel Level
		{
			[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
			get;
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
		IConstructionReturnMessage Activate(IConstructionCallMessage msg);
	}
}
