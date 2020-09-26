using System.Runtime.InteropServices;

namespace System.Deployment.Internal.Isolation
{
	internal struct StoreApplicationReference
	{
		[Flags]
		public enum RefFlags
		{
			Nothing = 0x0
		}

		[MarshalAs(UnmanagedType.U4)]
		public uint Size;

		[MarshalAs(UnmanagedType.U4)]
		public RefFlags Flags;

		public Guid GuidScheme;

		[MarshalAs(UnmanagedType.LPWStr)]
		public string Identifier;

		[MarshalAs(UnmanagedType.LPWStr)]
		public string NonCanonicalData;

		public StoreApplicationReference(Guid RefScheme, string Id, string NcData)
		{
			Size = (uint)Marshal.SizeOf(typeof(StoreApplicationReference));
			Flags = RefFlags.Nothing;
			GuidScheme = RefScheme;
			Identifier = Id;
			NonCanonicalData = NcData;
		}

		public IntPtr ToIntPtr()
		{
			IntPtr intPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(this));
			Marshal.StructureToPtr(this, intPtr, fDeleteOld: false);
			return intPtr;
		}

		public static void Destroy(IntPtr ip)
		{
			if (ip != IntPtr.Zero)
			{
				Marshal.DestroyStructure(ip, typeof(StoreApplicationReference));
				Marshal.FreeCoTaskMem(ip);
			}
		}
	}
}
