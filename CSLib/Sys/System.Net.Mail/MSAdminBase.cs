using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System.Net.Mail
{
	[ComImport]
	[Guid("a9e69610-b80d-11d0-b9b9-00a0c922e750")]
	[ClassInterface(ClassInterfaceType.None)]
	[TypeLibType(TypeLibTypeFlags.FCanCreate)]
	internal class MSAdminBase
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		public extern MSAdminBase();
	}
}
