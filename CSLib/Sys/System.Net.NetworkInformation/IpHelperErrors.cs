using System.Net.Sockets;

namespace System.Net.NetworkInformation
{
	internal class IpHelperErrors
	{
		internal const uint Success = 0u;

		internal const uint ErrorInvalidFunction = 1u;

		internal const uint ErrorNoSuchDevice = 2u;

		internal const uint ErrorInvalidData = 13u;

		internal const uint ErrorInvalidParameter = 87u;

		internal const uint ErrorBufferOverflow = 111u;

		internal const uint ErrorInsufficientBuffer = 122u;

		internal const uint ErrorNoData = 232u;

		internal const uint Pending = 997u;

		internal const uint ErrorNotFound = 1168u;

		internal static void CheckFamilyUnspecified(AddressFamily family)
		{
			if (family != AddressFamily.InterNetwork && family != AddressFamily.InterNetworkV6 && family != 0)
			{
				throw new ArgumentException(SR.GetString("net_invalidversion"), "family");
			}
		}
	}
}
