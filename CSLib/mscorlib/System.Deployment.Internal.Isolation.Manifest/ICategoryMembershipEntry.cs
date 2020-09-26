using System.Runtime.InteropServices;

namespace System.Deployment.Internal.Isolation.Manifest
{
	[ComImport]
	[Guid("97FDCA77-B6F2-4718-A1EB-29D0AECE9C03")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	internal interface ICategoryMembershipEntry
	{
		CategoryMembershipEntry AllData
		{
			get;
		}

		IDefinitionIdentity Identity
		{
			get;
		}

		ISection SubcategoryMembership
		{
			get;
		}
	}
}
