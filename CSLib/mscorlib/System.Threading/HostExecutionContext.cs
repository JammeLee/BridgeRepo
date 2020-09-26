namespace System.Threading
{
	public class HostExecutionContext
	{
		private object state;

		protected internal object State
		{
			get
			{
				return state;
			}
			set
			{
				state = value;
			}
		}

		public HostExecutionContext()
		{
		}

		public HostExecutionContext(object state)
		{
			this.state = state;
		}

		public virtual HostExecutionContext CreateCopy()
		{
			if (state is IUnknownSafeHandle)
			{
				((IUnknownSafeHandle)state).Clone();
			}
			return new HostExecutionContext(state);
		}
	}
}
