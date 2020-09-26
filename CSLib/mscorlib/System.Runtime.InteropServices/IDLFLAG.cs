namespace System.Runtime.InteropServices
{
	[Serializable]
	[Obsolete("Use System.Runtime.InteropServices.ComTypes.IDLFLAG instead. http://go.microsoft.com/fwlink/?linkid=14202", false)]
	[Flags]
	public enum IDLFLAG : short
	{
		IDLFLAG_NONE = 0x0,
		IDLFLAG_FIN = 0x1,
		IDLFLAG_FOUT = 0x2,
		IDLFLAG_FLCID = 0x4,
		IDLFLAG_FRETVAL = 0x8
	}
}
