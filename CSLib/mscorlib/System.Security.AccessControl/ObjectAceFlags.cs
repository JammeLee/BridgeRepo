namespace System.Security.AccessControl
{
	[Flags]
	public enum ObjectAceFlags
	{
		None = 0x0,
		ObjectAceTypePresent = 0x1,
		InheritedObjectAceTypePresent = 0x2
	}
}
