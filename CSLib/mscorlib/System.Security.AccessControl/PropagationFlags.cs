namespace System.Security.AccessControl
{
	[Flags]
	public enum PropagationFlags
	{
		None = 0x0,
		NoPropagateInherit = 0x1,
		InheritOnly = 0x2
	}
}
