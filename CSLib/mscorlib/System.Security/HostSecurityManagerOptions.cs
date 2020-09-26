using System.Runtime.InteropServices;

namespace System.Security
{
	[Serializable]
	[ComVisible(true)]
	[Flags]
	public enum HostSecurityManagerOptions
	{
		None = 0x0,
		HostAppDomainEvidence = 0x1,
		HostPolicyLevel = 0x2,
		HostAssemblyEvidence = 0x4,
		HostDetermineApplicationTrust = 0x8,
		HostResolvePolicy = 0x10,
		AllFlags = 0x1F
	}
}
