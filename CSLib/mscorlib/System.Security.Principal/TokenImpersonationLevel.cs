using System.Runtime.InteropServices;

namespace System.Security.Principal
{
	[Serializable]
	[ComVisible(true)]
	public enum TokenImpersonationLevel
	{
		None,
		Anonymous,
		Identification,
		Impersonation,
		Delegation
	}
}
