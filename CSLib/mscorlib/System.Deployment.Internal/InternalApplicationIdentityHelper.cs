using System.Runtime.InteropServices;

namespace System.Deployment.Internal
{
	[ComVisible(false)]
	public static class InternalApplicationIdentityHelper
	{
		public static object GetInternalAppId(ApplicationIdentity id)
		{
			return id.Identity;
		}
	}
}
