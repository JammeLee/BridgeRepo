using System.Runtime.InteropServices;

namespace System.Deployment.Internal.Isolation
{
	internal struct StoreOperationUnpinDeployment
	{
		[Flags]
		public enum OpFlags
		{
			Nothing = 0x0
		}

		public enum Disposition
		{
			Failed,
			Unpinned
		}

		[MarshalAs(UnmanagedType.U4)]
		public uint Size;

		[MarshalAs(UnmanagedType.U4)]
		public OpFlags Flags;

		[MarshalAs(UnmanagedType.Interface)]
		public IDefinitionAppId Application;

		public IntPtr Reference;

		public StoreOperationUnpinDeployment(IDefinitionAppId app, StoreApplicationReference reference)
		{
			Size = (uint)Marshal.SizeOf(typeof(StoreOperationUnpinDeployment));
			Flags = OpFlags.Nothing;
			Application = app;
			Reference = reference.ToIntPtr();
		}

		public void Destroy()
		{
			StoreApplicationReference.Destroy(Reference);
		}
	}
}
