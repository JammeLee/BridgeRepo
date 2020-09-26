using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Security;
using System.Security.Authentication.ExtendedProtection;

namespace System.Net
{
	[SuppressUnmanagedCodeSecurity]
	internal abstract class SafeFreeContextBufferChannelBinding : ChannelBinding
	{
		private int size;

		public override int Size => size;

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		internal void Set(IntPtr value)
		{
			handle = value;
		}

		internal static SafeFreeContextBufferChannelBinding CreateEmptyHandle(SecurDll dll)
		{
			return dll switch
			{
				SecurDll.SECURITY => new SafeFreeContextBufferChannelBinding_SECURITY(), 
				SecurDll.SECUR32 => new SafeFreeContextBufferChannelBinding_SECUR32(), 
				SecurDll.SCHANNEL => new SafeFreeContextBufferChannelBinding_SCHANNEL(), 
				_ => throw new ArgumentException(SR.GetString("net_invalid_enum", "SecurDll"), "dll"), 
			};
		}

		public unsafe static int QueryContextChannelBinding(SecurDll dll, SafeDeleteContext phContext, ContextAttribute contextAttribute, Bindings* buffer, SafeFreeContextBufferChannelBinding refHandle)
		{
			return dll switch
			{
				SecurDll.SECURITY => QueryContextChannelBinding_SECURITY(phContext, contextAttribute, buffer, refHandle), 
				SecurDll.SECUR32 => QueryContextChannelBinding_SECUR32(phContext, contextAttribute, buffer, refHandle), 
				SecurDll.SCHANNEL => QueryContextChannelBinding_SCHANNEL(phContext, contextAttribute, buffer, refHandle), 
				_ => -1, 
			};
		}

		private unsafe static int QueryContextChannelBinding_SECURITY(SafeDeleteContext phContext, ContextAttribute contextAttribute, Bindings* buffer, SafeFreeContextBufferChannelBinding refHandle)
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
					refHandle.Set(buffer->pBindings);
					refHandle.size = buffer->BindingsLength;
				}
				if (num != 0)
				{
					refHandle?.SetHandleAsInvalid();
				}
			}
			return num;
		}

		private unsafe static int QueryContextChannelBinding_SECUR32(SafeDeleteContext phContext, ContextAttribute contextAttribute, Bindings* buffer, SafeFreeContextBufferChannelBinding refHandle)
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
					refHandle.Set(buffer->pBindings);
					refHandle.size = buffer->BindingsLength;
				}
				if (num != 0)
				{
					refHandle?.SetHandleAsInvalid();
				}
			}
			return num;
		}

		private unsafe static int QueryContextChannelBinding_SCHANNEL(SafeDeleteContext phContext, ContextAttribute contextAttribute, Bindings* buffer, SafeFreeContextBufferChannelBinding refHandle)
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
					refHandle.Set(buffer->pBindings);
					refHandle.size = buffer->BindingsLength;
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
