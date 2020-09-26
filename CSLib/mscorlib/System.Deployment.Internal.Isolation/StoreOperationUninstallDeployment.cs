using System.Runtime.InteropServices;

namespace System.Deployment.Internal.Isolation
{
	internal struct StoreOperationUninstallDeployment
	{
		[Flags]
		public enum OpFlags
		{
			Nothing = 0x0
		}

		public enum Disposition
		{
			Failed,
			DidNotExist,
			Uninstalled
		}

		[MarshalAs(UnmanagedType.U4)]
		public uint Size;

		[MarshalAs(UnmanagedType.U4)]
		public OpFlags Flags;

		[MarshalAs(UnmanagedType.Interface)]
		public IDefinitionAppId Application;

		public IntPtr Reference;

		public StoreOperationUninstallDeployment(IDefinitionAppId appid, StoreApplicationReference AppRef)
		{
			Size = (uint)Marshal.SizeOf(typeof(StoreOperationUninstallDeployment));
			Flags = OpFlags.Nothing;
			Application = appid;
			Reference = AppRef.ToIntPtr();
		}

		public void Destroy()
		{
			StoreApplicationReference.Destroy(Reference);
		}
	}
}
