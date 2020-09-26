using System.Runtime.InteropServices;

namespace System.Deployment.Internal.Isolation
{
	internal struct StoreOperationInstallDeployment
	{
		[Flags]
		public enum OpFlags
		{
			Nothing = 0x0,
			UninstallOthers = 0x1
		}

		public enum Disposition
		{
			Failed,
			AlreadyInstalled,
			Installed
		}

		[MarshalAs(UnmanagedType.U4)]
		public uint Size;

		[MarshalAs(UnmanagedType.U4)]
		public OpFlags Flags;

		[MarshalAs(UnmanagedType.Interface)]
		public IDefinitionAppId Application;

		public IntPtr Reference;

		public StoreOperationInstallDeployment(IDefinitionAppId App, StoreApplicationReference reference)
			: this(App, UninstallOthers: true, reference)
		{
		}

		public StoreOperationInstallDeployment(IDefinitionAppId App, bool UninstallOthers, StoreApplicationReference reference)
		{
			Size = (uint)Marshal.SizeOf(typeof(StoreOperationInstallDeployment));
			Flags = OpFlags.Nothing;
			Application = App;
			if (UninstallOthers)
			{
				Flags |= OpFlags.UninstallOthers;
			}
			Reference = reference.ToIntPtr();
		}

		public void Destroy()
		{
			StoreApplicationReference.Destroy(Reference);
		}
	}
}
