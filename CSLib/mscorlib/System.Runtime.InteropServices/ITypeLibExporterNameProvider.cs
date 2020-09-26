namespace System.Runtime.InteropServices
{
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("FA1F3615-ACB9-486d-9EAC-1BEF87E36B09")]
	[ComVisible(true)]
	public interface ITypeLibExporterNameProvider
	{
		[return: MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_BSTR)]
		string[] GetNames();
	}
}
