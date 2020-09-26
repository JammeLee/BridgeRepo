using System.Runtime.CompilerServices;

namespace System.Runtime.InteropServices
{
	[ComVisible(true)]
	public sealed class ExtensibleClassFactory
	{
		private ExtensibleClassFactory()
		{
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		public static extern void RegisterObjectCreationCallback(ObjectCreationDelegate callback);
	}
}
