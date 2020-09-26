using System.Runtime.InteropServices;

namespace System.IO.IsolatedStorage
{
	[Serializable]
	[ComVisible(true)]
	[Flags]
	public enum IsolatedStorageScope
	{
		None = 0x0,
		User = 0x1,
		Domain = 0x2,
		Assembly = 0x4,
		Roaming = 0x8,
		Machine = 0x10,
		Application = 0x20
	}
}
