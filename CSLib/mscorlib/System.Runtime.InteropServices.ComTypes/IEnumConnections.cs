namespace System.Runtime.InteropServices.ComTypes
{
	[ComImport]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("B196B287-BAB4-101A-B69C-00AA00341D07")]
	public interface IEnumConnections
	{
		[PreserveSig]
		int Next(int celt, [Out][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] CONNECTDATA[] rgelt, IntPtr pceltFetched);

		[PreserveSig]
		int Skip(int celt);

		void Reset();

		void Clone(out IEnumConnections ppenum);
	}
}
