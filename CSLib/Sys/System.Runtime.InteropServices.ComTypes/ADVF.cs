namespace System.Runtime.InteropServices.ComTypes
{
	[Flags]
	public enum ADVF
	{
		ADVF_NODATA = 0x1,
		ADVF_PRIMEFIRST = 0x2,
		ADVF_ONLYONCE = 0x4,
		ADVF_DATAONSTOP = 0x40,
		ADVFCACHE_NOHANDLER = 0x8,
		ADVFCACHE_FORCEBUILTIN = 0x10,
		ADVFCACHE_ONSAVE = 0x20
	}
}
