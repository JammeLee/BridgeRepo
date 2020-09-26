namespace System.Runtime.InteropServices.ComTypes
{
	[Serializable]
	[Flags]
	public enum INVOKEKIND
	{
		INVOKE_FUNC = 0x1,
		INVOKE_PROPERTYGET = 0x2,
		INVOKE_PROPERTYPUT = 0x4,
		INVOKE_PROPERTYPUTREF = 0x8
	}
}
