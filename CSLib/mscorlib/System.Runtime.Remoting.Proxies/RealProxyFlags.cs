namespace System.Runtime.Remoting.Proxies
{
	[Flags]
	internal enum RealProxyFlags
	{
		None = 0x0,
		RemotingProxy = 0x1,
		Initialized = 0x2
	}
}
