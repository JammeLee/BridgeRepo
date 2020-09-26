using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;
using Microsoft.Win32.SafeHandles;

namespace System.Net
{
	[SuppressUnmanagedCodeSecurity]
	internal abstract class SafeFreeContextBuffer : SafeHandleZeroOrMinusOneIsInvalid
	{
		protected SafeFreeContextBuffer()
			: base(ownsHandle: true)
		{
		}

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		internal void Set(IntPtr value)
		{
			handle = value;
		}

		internal static int EnumeratePackages(SecurDll Dll, out int pkgnum, out SafeFreeContextBuffer pkgArray)
		{
			int num = -1;
			switch (Dll)
			{
			case SecurDll.SECURITY:
			{
				SafeFreeContextBuffer_SECURITY handle3 = null;
				num = UnsafeNclNativeMethods.SafeNetHandles_SECURITY.EnumerateSecurityPackagesW(out pkgnum, out handle3);
				pkgArray = handle3;
				break;
			}
			case SecurDll.SECUR32:
			{
				SafeFreeContextBuffer_SECUR32 handle2 = null;
				num = UnsafeNclNativeMethods.SafeNetHandles_SECUR32.EnumerateSecurityPackagesA(out pkgnum, out handle2);
				pkgArray = handle2;
				break;
			}
			case SecurDll.SCHANNEL:
			{
				SafeFreeContextBuffer_SCHANNEL handle = null;
				num = UnsafeNclNativeMethods.SafeNetHandles_SCHANNEL.EnumerateSecurityPackagesA(out pkgnum, out handle);
				pkgArray = handle;
				break;
			}
			default:
				throw new ArgumentException(SR.GetString("net_invalid_enum", "SecurDll"), "Dll");
			}
			if (num != 0 && pkgArray != null)
			{
				pkgArray.SetHandleAsInvalid();
			}
			return num;
		}

		internal static SafeFreeContextBuffer CreateEmptyHandle(SecurDll dll)
		{
			return dll switch
			{
				SecurDll.SECURITY => new SafeFreeContextBuffer_SECURITY(), 
				SecurDll.SECUR32 => new SafeFreeContextBuffer_SECUR32(), 
				SecurDll.SCHANNEL => new SafeFreeContextBuffer_SCHANNEL(), 
				_ => throw new ArgumentException(SR.GetString("net_invalid_enum", "SecurDll"), "dll"), 
			};
		}

		public unsafe static int QueryContextAttributes(SecurDll dll, SafeDeleteContext phContext, ContextAttribute contextAttribute, byte* buffer, SafeHandle refHandle)
		{
			return dll switch
			{
				SecurDll.SECURITY => QueryContextAttributes_SECURITY(phContext, contextAttribute, buffer, refHandle), 
				SecurDll.SECUR32 => QueryContextAttributes_SECUR32(phContext, contextAttribute, buffer, refHandle), 
				SecurDll.SCHANNEL => QueryContextAttributes_SCHANNEL(phContext, contextAttribute, buffer, refHandle), 
				_ => -1, 
			};
		}

		private unsafe static int QueryContextAttributes_SECURITY(SafeDeleteContext phContext, ContextAttribute contextAttribute, byte* buffer, SafeHandle refHandle)
		{
			int num = -2146893055;
			bool success = false;
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
					num = UnsafeNclNativeMethods.SafeNetHandles_SECURITY.QueryContextAttributesW(ref phContext._handle, contextAttribute, buffer);
					phContext.DangerousRelease();
				}
				if (num == 0 && refHandle != null)
				{
					if (refHandle is SafeFreeContextBuffer)
					{
						((SafeFreeContextBuffer)refHandle).Set(*(IntPtr*)buffer);
					}
					else
					{
						((SafeFreeCertContext)refHandle).Set(*(IntPtr*)buffer);
					}
				}
				if (num != 0)
				{
					refHandle?.SetHandleAsInvalid();
				}
			}
			return num;
		}

		private unsafe static int QueryContextAttributes_SECUR32(SafeDeleteContext phContext, ContextAttribute contextAttribute, byte* buffer, SafeHandle refHandle)
		{
			int num = -2146893055;
			bool success = false;
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
					num = UnsafeNclNativeMethods.SafeNetHandles_SECUR32.QueryContextAttributesA(ref phContext._handle, contextAttribute, buffer);
					phContext.DangerousRelease();
				}
				if (num == 0 && refHandle != null)
				{
					if (refHandle is SafeFreeContextBuffer)
					{
						((SafeFreeContextBuffer)refHandle).Set(*(IntPtr*)buffer);
					}
					else
					{
						((SafeFreeCertContext)refHandle).Set(*(IntPtr*)buffer);
					}
				}
				if (num != 0)
				{
					refHandle?.SetHandleAsInvalid();
				}
			}
			return num;
		}

		private unsafe static int QueryContextAttributes_SCHANNEL(SafeDeleteContext phContext, ContextAttribute contextAttribute, byte* buffer, SafeHandle refHandle)
		{
			int num = -2146893055;
			bool success = false;
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
					num = UnsafeNclNativeMethods.SafeNetHandles_SCHANNEL.QueryContextAttributesA(ref phContext._handle, contextAttribute, buffer);
					phContext.DangerousRelease();
				}
				if (num == 0 && refHandle != null)
				{
					if (refHandle is SafeFreeContextBuffer)
					{
						((SafeFreeContextBuffer)refHandle).Set(*(IntPtr*)buffer);
					}
					else
					{
						((SafeFreeCertContext)refHandle).Set(*(IntPtr*)buffer);
					}
				}
				if (num != 0)
				{
					refHandle?.SetHandleAsInvalid();
				}
			}
			return num;
		}
	}
}
