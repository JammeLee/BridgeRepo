namespace System.Runtime.InteropServices
{
	[Serializable]
	[Obsolete("Use System.Runtime.InteropServices.ComTypes.IMPLTYPEFLAGS instead. http://go.microsoft.com/fwlink/?linkid=14202", false)]
	[Flags]
	public enum IMPLTYPEFLAGS
	{
		IMPLTYPEFLAG_FDEFAULT = 0x1,
		IMPLTYPEFLAG_FSOURCE = 0x2,
		IMPLTYPEFLAG_FRESTRICTED = 0x4,
		IMPLTYPEFLAG_FDEFAULTVTABLE = 0x8
	}
}
