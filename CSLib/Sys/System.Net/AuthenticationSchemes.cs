namespace System.Net
{
	[Flags]
	public enum AuthenticationSchemes
	{
		None = 0x0,
		Digest = 0x1,
		Negotiate = 0x2,
		Ntlm = 0x4,
		Basic = 0x8,
		Anonymous = 0x8000,
		IntegratedWindowsAuthentication = 0x6
	}
}
