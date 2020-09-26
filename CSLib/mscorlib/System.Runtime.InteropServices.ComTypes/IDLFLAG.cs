namespace System.Runtime.InteropServices.ComTypes
{
	[Serializable]
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
