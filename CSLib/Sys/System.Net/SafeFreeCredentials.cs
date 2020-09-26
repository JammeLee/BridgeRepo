using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System.Net
{
	internal abstract class SafeFreeCredentials : SafeHandle
	{
		internal SSPIHandle _handle;

		public override bool IsInvalid
		{
			get
			{
				if (!base.IsClosed)
				{
					return _handle.IsZero;
				}
				return true;
			}
		}

		protected SafeFreeCredentials()
			: base(IntPtr.Zero, ownsHandle: true)
		{
			_handle = default(SSPIHandle);
		}

		public static int AcquireCredentialsHandle(SecurDll dll, string package, CredentialUse intent, ref AuthIdentity authdata, out SafeFreeCredentials outCredential)
		{
			int num = -1;
			long timeStamp;
			switch (dll)
			{
			case SecurDll.SECURITY:
				outCredential = new SafeFreeCredential_SECURITY();
				RuntimeHelpers.PrepareConstrainedRegions();
				try
				{
				}
				finally
				{
					num = UnsafeNclNativeMethods.SafeNetHandles_SECURITY.AcquireCredentialsHandleW(null, package, (int)intent, null, ref authdata, null, null, ref outCredential._handle, out timeStamp);
				}
				break;
			case SecurDll.SECUR32:
				outCredential = new SafeFreeCredential_SECUR32();
				RuntimeHelpers.PrepareConstrainedRegions();
				try
				{
				}
				finally
				{
					num = UnsafeNclNativeMethods.SafeNetHandles_SECUR32.AcquireCredentialsHandleA(null, package, (int)intent, null, ref authdata, null, null, ref outCredential._handle, out timeStamp);
				}
				break;
			default:
				throw new ArgumentException(SR.GetString("net_invalid_enum", "SecurDll"), "Dll");
			}
			if (num != 0)
			{
				outCredential.SetHandleAsInvalid();
			}
			return num;
		}

		public static int AcquireDefaultCredential(SecurDll dll, string package, CredentialUse intent, out SafeFreeCredentials outCredential)
		{
			int num = -1;
			long timeStamp;
			switch (dll)
			{
			case SecurDll.SECURITY:
				outCredential = new SafeFreeCredential_SECURITY();
				RuntimeHelpers.PrepareConstrainedRegions();
				try
				{
				}
				finally
				{
					num = UnsafeNclNativeMethods.SafeNetHandles_SECURITY.AcquireCredentialsHandleW(null, package, (int)intent, null, IntPtr.Zero, null, null, ref outCredential._handle, out timeStamp);
				}
				break;
			case SecurDll.SECUR32:
				outCredential = new SafeFreeCredential_SECUR32();
				RuntimeHelpers.PrepareConstrainedRegions();
				try
				{
				}
				finally
				{
					num = UnsafeNclNativeMethods.SafeNetHandles_SECUR32.AcquireCredentialsHandleA(null, package, (int)intent, null, IntPtr.Zero, null, null, ref outCredential._handle, out timeStamp);
				}
				break;
			default:
				throw new ArgumentException(SR.GetString("net_invalid_enum", "SecurDll"), "Dll");
			}
			if (num != 0)
			{
				outCredential.SetHandleAsInvalid();
			}
			return num;
		}

		public unsafe static int AcquireCredentialsHandle(SecurDll dll, string package, CredentialUse intent, ref SecureCredential authdata, out SafeFreeCredentials outCredential)
		{
			int num = -1;
			IntPtr certContextArray = authdata.certContextArray;
			try
			{
				IntPtr certContextArray2 = new IntPtr(&certContextArray);
				if (certContextArray != IntPtr.Zero)
				{
					authdata.certContextArray = certContextArray2;
				}
				long timeStamp;
				switch (dll)
				{
				case SecurDll.SECURITY:
					outCredential = new SafeFreeCredential_SECURITY();
					RuntimeHelpers.PrepareConstrainedRegions();
					try
					{
					}
					finally
					{
						num = UnsafeNclNativeMethods.SafeNetHandles_SECURITY.AcquireCredentialsHandleW(null, package, (int)intent, null, ref authdata, null, null, ref outCredential._handle, out timeStamp);
					}
					break;
				case SecurDll.SCHANNEL:
					outCredential = new SafeFreeCredential_SCHANNEL();
					RuntimeHelpers.PrepareConstrainedRegions();
					try
					{
					}
					finally
					{
						num = UnsafeNclNativeMethods.SafeNetHandles_SCHANNEL.AcquireCredentialsHandleA(null, package, (int)intent, null, ref authdata, null, null, ref outCredential._handle, out timeStamp);
					}
					break;
				default:
					throw new ArgumentException(SR.GetString("net_invalid_enum", "SecurDll"), "Dll");
				}
			}
			finally
			{
				authdata.certContextArray = certContextArray;
			}
			if (num != 0)
			{
				outCredential.SetHandleAsInvalid();
			}
			return num;
		}
	}
}
