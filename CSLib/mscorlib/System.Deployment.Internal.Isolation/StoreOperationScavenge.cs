using System.Runtime.InteropServices;

namespace System.Deployment.Internal.Isolation
{
	internal struct StoreOperationScavenge
	{
		[Flags]
		public enum OpFlags
		{
			Nothing = 0x0,
			Light = 0x1,
			LimitSize = 0x2,
			LimitTime = 0x4,
			LimitCount = 0x8
		}

		[MarshalAs(UnmanagedType.U4)]
		public uint Size;

		[MarshalAs(UnmanagedType.U4)]
		public OpFlags Flags;

		[MarshalAs(UnmanagedType.U8)]
		public ulong SizeReclaimationLimit;

		[MarshalAs(UnmanagedType.U8)]
		public ulong RuntimeLimit;

		[MarshalAs(UnmanagedType.U4)]
		public uint ComponentCountLimit;

		public StoreOperationScavenge(bool Light, ulong SizeLimit, ulong RunLimit, uint ComponentLimit)
		{
			Size = (uint)Marshal.SizeOf(typeof(StoreOperationScavenge));
			Flags = OpFlags.Nothing;
			if (Light)
			{
				Flags |= OpFlags.Light;
			}
			SizeReclaimationLimit = SizeLimit;
			if (SizeLimit != 0)
			{
				Flags |= OpFlags.LimitSize;
			}
			RuntimeLimit = RunLimit;
			if (RunLimit != 0)
			{
				Flags |= OpFlags.LimitTime;
			}
			ComponentCountLimit = ComponentLimit;
			if (ComponentLimit != 0)
			{
				Flags |= OpFlags.LimitCount;
			}
		}

		public StoreOperationScavenge(bool Light)
			: this(Light, 0uL, 0uL, 0u)
		{
		}

		public void Destroy()
		{
		}
	}
}
