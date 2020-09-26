using System.Security.Permissions;

namespace System.ComponentModel
{
	[HostProtection(SecurityAction.LinkDemand, SharedState = true)]
	public class ProgressChangedEventArgs : EventArgs
	{
		private readonly int progressPercentage;

		private readonly object userState;

		[SRDescription("Async_ProgressChangedEventArgs_ProgressPercentage")]
		public int ProgressPercentage => progressPercentage;

		[SRDescription("Async_ProgressChangedEventArgs_UserState")]
		public object UserState => userState;

		public ProgressChangedEventArgs(int progressPercentage, object userState)
		{
			this.progressPercentage = progressPercentage;
			this.userState = userState;
		}
	}
}
