namespace System.Runtime.InteropServices
{
	[Serializable]
	[Flags]
	[ComVisible(true)]
	public enum TypeLibTypeFlags
	{
		FAppObject = 0x1,
		FCanCreate = 0x2,
		FLicensed = 0x4,
		FPreDeclId = 0x8,
		FHidden = 0x10,
		FControl = 0x20,
		FDual = 0x40,
		FNonExtensible = 0x80,
		FOleAutomation = 0x100,
		FRestricted = 0x200,
		FAggregatable = 0x400,
		FReplaceable = 0x800,
		FDispatchable = 0x1000,
		FReverseBind = 0x2000
	}
}
