using System.Runtime.CompilerServices;

namespace System
{
	internal static class ConfigServer
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern void RunParser(IConfigHandler factory, string fileName);
	}
}
