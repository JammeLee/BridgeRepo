using System.Runtime.InteropServices;

namespace System.Security.Permissions
{
	[Serializable]
	[ComVisible(true)]
	public enum IsolatedStorageContainment
	{
		None = 0,
		DomainIsolationByUser = 0x10,
		ApplicationIsolationByUser = 21,
		AssemblyIsolationByUser = 0x20,
		DomainIsolationByMachine = 48,
		AssemblyIsolationByMachine = 0x40,
		ApplicationIsolationByMachine = 69,
		DomainIsolationByRoamingUser = 80,
		AssemblyIsolationByRoamingUser = 96,
		ApplicationIsolationByRoamingUser = 101,
		AdministerIsolatedStorageByUser = 112,
		UnrestrictedIsolatedStorage = 240
	}
}
