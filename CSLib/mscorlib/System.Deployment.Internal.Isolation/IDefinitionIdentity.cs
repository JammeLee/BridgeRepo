using System.Runtime.InteropServices;

namespace System.Deployment.Internal.Isolation
{
	[ComImport]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("587bf538-4d90-4a3c-9ef1-58a200a8a9e7")]
	internal interface IDefinitionIdentity
	{
		[return: MarshalAs(UnmanagedType.LPWStr)]
		string GetAttribute([In][MarshalAs(UnmanagedType.LPWStr)] string Namespace, [In][MarshalAs(UnmanagedType.LPWStr)] string Name);

		void SetAttribute([In][MarshalAs(UnmanagedType.LPWStr)] string Namespace, [In][MarshalAs(UnmanagedType.LPWStr)] string Name, [In][MarshalAs(UnmanagedType.LPWStr)] string Value);

		IEnumIDENTITY_ATTRIBUTE EnumAttributes();

		IDefinitionIdentity Clone([In] IntPtr cDeltas, [In][MarshalAs(UnmanagedType.LPArray)] IDENTITY_ATTRIBUTE[] Deltas);
	}
}
