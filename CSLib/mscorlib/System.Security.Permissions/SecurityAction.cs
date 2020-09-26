using System.Runtime.InteropServices;

namespace System.Security.Permissions
{
	[Serializable]
	[ComVisible(true)]
	public enum SecurityAction
	{
		Demand = 2,
		Assert,
		Deny,
		PermitOnly,
		LinkDemand,
		InheritanceDemand,
		RequestMinimum,
		RequestOptional,
		RequestRefuse
	}
}
