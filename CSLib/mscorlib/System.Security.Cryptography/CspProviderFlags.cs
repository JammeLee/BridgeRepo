using System.Runtime.InteropServices;

namespace System.Security.Cryptography
{
	[Serializable]
	[Flags]
	[ComVisible(true)]
	public enum CspProviderFlags
	{
		NoFlags = 0x0,
		UseMachineKeyStore = 0x1,
		UseDefaultKeyContainer = 0x2,
		UseNonExportableKey = 0x4,
		UseExistingKey = 0x8,
		UseArchivableKey = 0x10,
		UseUserProtectedKey = 0x20,
		NoPrompt = 0x40
	}
}
