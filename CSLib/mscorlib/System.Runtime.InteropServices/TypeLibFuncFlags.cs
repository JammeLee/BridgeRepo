namespace System.Runtime.InteropServices
{
	[Serializable]
	[Flags]
	[ComVisible(true)]
	public enum TypeLibFuncFlags
	{
		FRestricted = 0x1,
		FSource = 0x2,
		FBindable = 0x4,
		FRequestEdit = 0x8,
		FDisplayBind = 0x10,
		FDefaultBind = 0x20,
		FHidden = 0x40,
		FUsesGetLastError = 0x80,
		FDefaultCollelem = 0x100,
		FUiDefault = 0x200,
		FNonBrowsable = 0x400,
		FReplaceable = 0x800,
		FImmediateBind = 0x1000
	}
}
