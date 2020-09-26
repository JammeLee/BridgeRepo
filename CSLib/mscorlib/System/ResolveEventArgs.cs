using System.Runtime.InteropServices;

namespace System
{
	[ComVisible(true)]
	public class ResolveEventArgs : EventArgs
	{
		private string _Name;

		public string Name => _Name;

		public ResolveEventArgs(string name)
		{
			_Name = name;
		}
	}
}
