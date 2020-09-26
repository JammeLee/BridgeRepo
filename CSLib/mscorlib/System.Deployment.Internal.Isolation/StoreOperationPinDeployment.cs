using System.Runtime.InteropServices;

namespace System.Deployment.Internal.Isolation
{
	internal struct StoreOperationPinDeployment
	{
		[Flags]
		public enum OpFlags
		{
			Nothing = 0x0,
			NeverExpires = 0x1
		}

		public enum Disposition
		{
			Failed,
			Pinned
		}

		[MarshalAs(UnmanagedType.U4)]
		public uint Size;

		[MarshalAs(UnmanagedType.U4)]
		public OpFlags Flags;

		[MarshalAs(UnmanagedType.Interface)]
		public IDefinitionAppId Application;

		[MarshalAs(UnmanagedType.I8)]
		public long ExpirationTime;

		public IntPtr Reference;

		public StoreOperationPinDeployment(IDefinitionAppId AppId, StoreApplicationReference Ref)
		{
			Size = (uint)Marshal.SizeOf(typeof(StoreOperationPinDeployment));
			Flags = OpFlags.NeverExpires;
			Application = AppId;
			Reference = Ref.ToIntPtr();
			ExpirationTime = 0L;
		}

		public StoreOperationPinDeployment(IDefinitionAppId AppId, DateTime Expiry, StoreApplicationReference Ref)
			: this(AppId, Ref)
		{
			Flags |= OpFlags.NeverExpires;
		}

		public void Destroy()
		{
			StoreApplicationReference.Destroy(Reference);
		}
	}
}
