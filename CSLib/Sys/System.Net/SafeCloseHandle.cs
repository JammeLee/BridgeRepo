using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Security;
using System.Threading;
using Microsoft.Win32.SafeHandles;

namespace System.Net
{
	[SuppressUnmanagedCodeSecurity]
	internal sealed class SafeCloseHandle : CriticalHandleZeroOrMinusOneIsInvalid
	{
		private const string SECURITY = "security.dll";

		private const string ADVAPI32 = "advapi32.dll";

		private const string HTTPAPI = "httpapi.dll";

		private int _disposed;

		private SafeCloseHandle()
		{
		}

		internal IntPtr DangerousGetHandle()
		{
			return handle;
		}

		protected override bool ReleaseHandle()
		{
			if (!IsInvalid && Interlocked.Increment(ref _disposed) == 1)
			{
				return UnsafeNclNativeMethods.SafeNetHandles.CloseHandle(handle);
			}
			return true;
		}

		internal static int GetSecurityContextToken(SafeDeleteContext phContext, out SafeCloseHandle safeHandle)
		{
			int result = -2146893055;
			bool success = false;
			safeHandle = null;
			RuntimeHelpers.PrepareConstrainedRegions();
			try
			{
				phContext.DangerousAddRef(ref success);
			}
			catch (Exception ex)
			{
				if (success)
				{
					phContext.DangerousRelease();
					success = false;
				}
				if (!(ex is ObjectDisposedException))
				{
					throw;
				}
			}
			finally
			{
				if (success)
				{
					result = UnsafeNclNativeMethods.SafeNetHandles.QuerySecurityContextToken(ref phContext._handle, out safeHandle);
					phContext.DangerousRelease();
				}
			}
			return result;
		}

		internal static SafeCloseHandle CreateRequestQueueHandle()
		{
			SafeCloseHandle pReqQueueHandle = null;
			uint num = UnsafeNclNativeMethods.SafeNetHandles.HttpCreateHttpHandle(out pReqQueueHandle, 0u);
			if (pReqQueueHandle != null && num != 0)
			{
				pReqQueueHandle.SetHandleAsInvalid();
				throw new HttpListenerException((int)num);
			}
			return pReqQueueHandle;
		}

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		internal void Abort()
		{
			ReleaseHandle();
			SetHandleAsInvalid();
		}
	}
}
