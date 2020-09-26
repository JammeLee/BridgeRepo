using System.Runtime.InteropServices;
using System.Security;

namespace System.Net.NetworkInformation
{
	[SuppressUnmanagedCodeSecurity]
	internal static class UnsafeWinINetNativeMethods
	{
		private const string WININET = "wininet.dll";

		[DllImport("wininet.dll")]
		internal static extern bool InternetGetConnectedState(ref uint flags, uint dwReserved);
	}
}
