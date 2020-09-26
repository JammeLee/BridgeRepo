namespace System
{
	[Serializable]
	public sealed class ConsoleCancelEventArgs : EventArgs
	{
		private ConsoleSpecialKey _type;

		private bool _cancel;

		public bool Cancel
		{
			get
			{
				return _cancel;
			}
			set
			{
				if (_type == ConsoleSpecialKey.ControlBreak && value)
				{
					throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_CantCancelCtrlBreak"));
				}
				_cancel = value;
			}
		}

		public ConsoleSpecialKey SpecialKey => _type;

		internal ConsoleCancelEventArgs(ConsoleSpecialKey type)
		{
			_type = type;
			_cancel = false;
		}
	}
}
