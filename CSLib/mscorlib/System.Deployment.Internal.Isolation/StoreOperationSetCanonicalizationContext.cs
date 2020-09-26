using System.Runtime.InteropServices;

namespace System.Deployment.Internal.Isolation
{
	internal struct StoreOperationSetCanonicalizationContext
	{
		[Flags]
		public enum OpFlags
		{
			Nothing = 0x0
		}

		[MarshalAs(UnmanagedType.U4)]
		public uint Size;

		[MarshalAs(UnmanagedType.U4)]
		public OpFlags Flags;

		[MarshalAs(UnmanagedType.LPWStr)]
		public string BaseAddressFilePath;

		[MarshalAs(UnmanagedType.LPWStr)]
		public string ExportsFilePath;

		public StoreOperationSetCanonicalizationContext(string Bases, string Exports)
		{
			Size = (uint)Marshal.SizeOf(typeof(StoreOperationSetCanonicalizationContext));
			Flags = OpFlags.Nothing;
			BaseAddressFilePath = Bases;
			ExportsFilePath = Exports;
		}

		public void Destroy()
		{
		}
	}
}
