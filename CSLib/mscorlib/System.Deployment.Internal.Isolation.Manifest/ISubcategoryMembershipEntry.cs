using System.Runtime.InteropServices;

namespace System.Deployment.Internal.Isolation.Manifest
{
	[ComImport]
	[Guid("5A7A54D7-5AD5-418e-AB7A-CF823A8D48D0")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	internal interface ISubcategoryMembershipEntry
	{
		SubcategoryMembershipEntry AllData
		{
			get;
		}

		string Subcategory
		{
			[return: MarshalAs(UnmanagedType.LPWStr)]
			get;
		}

		ISection CategoryMembershipData
		{
			get;
		}
	}
}
