namespace System.Runtime.InteropServices
{
	[Serializable]
	[Flags]
	[Obsolete("Use System.Runtime.InteropServices.ComTypes.VARFLAGS instead. http://go.microsoft.com/fwlink/?linkid=14202", false)]
	public enum VARFLAGS : short
	{
		VARFLAG_FREADONLY = 0x1,
		VARFLAG_FSOURCE = 0x2,
		VARFLAG_FBINDABLE = 0x4,
		VARFLAG_FREQUESTEDIT = 0x8,
		VARFLAG_FDISPLAYBIND = 0x10,
		VARFLAG_FDEFAULTBIND = 0x20,
		VARFLAG_FHIDDEN = 0x40,
		VARFLAG_FRESTRICTED = 0x80,
		VARFLAG_FDEFAULTCOLLELEM = 0x100,
		VARFLAG_FUIDEFAULT = 0x200,
		VARFLAG_FNONBROWSABLE = 0x400,
		VARFLAG_FREPLACEABLE = 0x800,
		VARFLAG_FIMMEDIATEBIND = 0x1000
	}
}
