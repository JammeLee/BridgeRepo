using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Microsoft.Win32
{
	[ComImport]
	[Guid("7b8a2d94-0ac9-11d1-896c-00c04Fb6bfc4")]
	[ComVisible(false)]
	internal class InternetSecurityManager
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		public extern InternetSecurityManager();
	}
}
