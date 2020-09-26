using System.Security.Permissions;

namespace System.ComponentModel
{
	[HostProtection(SecurityAction.LinkDemand, SharedState = true)]
	public class DoWorkEventArgs : CancelEventArgs
	{
		private object result;

		private object argument;

		[SRDescription("BackgroundWorker_DoWorkEventArgs_Argument")]
		public object Argument => argument;

		[SRDescription("BackgroundWorker_DoWorkEventArgs_Result")]
		public object Result
		{
			get
			{
				return result;
			}
			set
			{
				result = value;
			}
		}

		public DoWorkEventArgs(object argument)
		{
			this.argument = argument;
		}
	}
}
