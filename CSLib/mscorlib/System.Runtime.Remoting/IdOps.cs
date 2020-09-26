using System.Runtime.InteropServices;

namespace System.Runtime.Remoting
{
	[StructLayout(LayoutKind.Sequential, Size = 1)]
	internal struct IdOps
	{
		internal const int None = 0;

		internal const int GenerateURI = 1;

		internal const int StrongIdentity = 2;

		internal static bool bStrongIdentity(int flags)
		{
			return (flags & 2) != 0;
		}
	}
}
