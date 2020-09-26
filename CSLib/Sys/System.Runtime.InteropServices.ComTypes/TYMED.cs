namespace System.Runtime.InteropServices.ComTypes
{
	[Flags]
	public enum TYMED
	{
		TYMED_HGLOBAL = 0x1,
		TYMED_FILE = 0x2,
		TYMED_ISTREAM = 0x4,
		TYMED_ISTORAGE = 0x8,
		TYMED_GDI = 0x10,
		TYMED_MFPICT = 0x20,
		TYMED_ENHMF = 0x40,
		TYMED_NULL = 0x0
	}
}
