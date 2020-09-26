using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace System.Threading
{
	public class HostExecutionContextManager
	{
		private static bool _fIsHostedChecked;

		private static bool _fIsHosted;

		private static HostExecutionContextManager _hostExecutionContextManager;

		[MethodImpl(MethodImplOptions.InternalCall)]
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		private static extern bool HostSecurityManagerPresent();

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern int ReleaseHostSecurityContext(IntPtr context);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern int CloneHostSecurityContext(SafeHandle context, SafeHandle clonedContext);

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern int CaptureHostSecurityContext(SafeHandle capturedContext);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		private static extern int SetHostSecurityContext(SafeHandle context, bool fReturnPrevious, SafeHandle prevContext);

		internal static bool CheckIfHosted()
		{
			if (!_fIsHostedChecked)
			{
				_fIsHosted = HostSecurityManagerPresent();
				_fIsHostedChecked = true;
			}
			return _fIsHosted;
		}

		public virtual HostExecutionContext Capture()
		{
			HostExecutionContext result = null;
			if (CheckIfHosted())
			{
				IUnknownSafeHandle unknownSafeHandle = new IUnknownSafeHandle();
				result = new HostExecutionContext(unknownSafeHandle);
				CaptureHostSecurityContext(unknownSafeHandle);
			}
			return result;
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
		public virtual object SetHostExecutionContext(HostExecutionContext hostExecutionContext)
		{
			if (hostExecutionContext == null)
			{
				throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_NotNewCaptureContext"));
			}
			HostExecutionContextSwitcher hostExecutionContextSwitcher = new HostExecutionContextSwitcher();
			ExecutionContext executionContext = (hostExecutionContextSwitcher.executionContext = Thread.CurrentThread.ExecutionContext);
			hostExecutionContextSwitcher.currentHostContext = hostExecutionContext;
			hostExecutionContextSwitcher.previousHostContext = null;
			if (CheckIfHosted() && hostExecutionContext.State is IUnknownSafeHandle)
			{
				IUnknownSafeHandle unknownSafeHandle = new IUnknownSafeHandle();
				hostExecutionContextSwitcher.previousHostContext = new HostExecutionContext(unknownSafeHandle);
				IUnknownSafeHandle context = (IUnknownSafeHandle)hostExecutionContext.State;
				SetHostSecurityContext(context, fReturnPrevious: true, unknownSafeHandle);
			}
			executionContext.HostExecutionContext = hostExecutionContext;
			return hostExecutionContextSwitcher;
		}

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
		public virtual void Revert(object previousState)
		{
			HostExecutionContextSwitcher hostExecutionContextSwitcher = previousState as HostExecutionContextSwitcher;
			if (hostExecutionContextSwitcher == null)
			{
				throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_CannotOverrideSetWithoutRevert"));
			}
			ExecutionContext executionContext = Thread.CurrentThread.ExecutionContext;
			if (executionContext != hostExecutionContextSwitcher.executionContext)
			{
				throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_CannotUseSwitcherOtherThread"));
			}
			hostExecutionContextSwitcher.executionContext = null;
			HostExecutionContext hostExecutionContext = executionContext.HostExecutionContext;
			if (hostExecutionContext != hostExecutionContextSwitcher.currentHostContext)
			{
				throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_CannotUseSwitcherOtherThread"));
			}
			HostExecutionContext previousHostContext = hostExecutionContextSwitcher.previousHostContext;
			if (CheckIfHosted() && previousHostContext != null && previousHostContext.State is IUnknownSafeHandle)
			{
				IUnknownSafeHandle context = (IUnknownSafeHandle)previousHostContext.State;
				SetHostSecurityContext(context, fReturnPrevious: false, null);
			}
			executionContext.HostExecutionContext = previousHostContext;
		}

		internal static HostExecutionContext CaptureHostExecutionContext()
		{
			HostExecutionContext result = null;
			HostExecutionContextManager currentHostExecutionContextManager = GetCurrentHostExecutionContextManager();
			if (currentHostExecutionContextManager != null)
			{
				result = currentHostExecutionContextManager.Capture();
			}
			return result;
		}

		internal static object SetHostExecutionContextInternal(HostExecutionContext hostContext)
		{
			HostExecutionContextManager currentHostExecutionContextManager = GetCurrentHostExecutionContextManager();
			object result = null;
			if (currentHostExecutionContextManager != null)
			{
				result = currentHostExecutionContextManager.SetHostExecutionContext(hostContext);
			}
			return result;
		}

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		internal static HostExecutionContextManager GetCurrentHostExecutionContextManager()
		{
			if (AppDomainManager.CurrentAppDomainManager != null)
			{
				return AppDomainManager.CurrentAppDomainManager.HostExecutionContextManager;
			}
			return null;
		}

		internal static HostExecutionContextManager GetInternalHostExecutionContextManager()
		{
			if (_hostExecutionContextManager == null)
			{
				_hostExecutionContextManager = new HostExecutionContextManager();
			}
			return _hostExecutionContextManager;
		}
	}
}
