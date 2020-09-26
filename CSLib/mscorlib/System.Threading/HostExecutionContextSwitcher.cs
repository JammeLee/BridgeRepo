using System.Runtime.ConstrainedExecution;

namespace System.Threading
{
	internal class HostExecutionContextSwitcher
	{
		internal ExecutionContext executionContext;

		internal HostExecutionContext previousHostContext;

		internal HostExecutionContext currentHostContext;

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		public static void Undo(object switcherObject)
		{
			if (switcherObject != null)
			{
				HostExecutionContextManager.GetCurrentHostExecutionContextManager()?.Revert(switcherObject);
			}
		}
	}
}
