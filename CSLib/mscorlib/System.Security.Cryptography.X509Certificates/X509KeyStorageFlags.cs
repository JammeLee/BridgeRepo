using System.Runtime.InteropServices;

namespace System.Security.Cryptography.X509Certificates
{
	[Serializable]
	[Flags]
	[ComVisible(true)]
	public enum X509KeyStorageFlags
	{
		DefaultKeySet = 0x0,
		UserKeySet = 0x1,
		MachineKeySet = 0x2,
		Exportable = 0x4,
		UserProtected = 0x8,
		PersistKeySet = 0x10
	}
}
