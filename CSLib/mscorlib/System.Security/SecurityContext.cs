using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Security.Permissions;
using System.Security.Principal;
using System.Threading;
using Microsoft.Win32.SafeHandles;

namespace System.Security
{
	public sealed class SecurityContext
	{
		internal class SecurityContextRunData
		{
			internal SecurityContext sc;

			internal ContextCallback callBack;

			internal object state;

			internal SecurityContextSwitcher scsw;

			internal SecurityContextRunData(SecurityContext securityContext, ContextCallback cb, object state)
			{
				sc = securityContext;
				callBack = cb;
				this.state = state;
				scsw = default(SecurityContextSwitcher);
			}
		}

		private static bool _LegacyImpersonationPolicy = GetImpersonationFlowMode() == WindowsImpersonationFlowMode.IMP_NOFLOW;

		private static bool _alwaysFlowImpersonationPolicy = GetImpersonationFlowMode() == WindowsImpersonationFlowMode.IMP_ALWAYSFLOW;

		private ExecutionContext _executionContext;

		private WindowsIdentity _windowsIdentity;

		private CompressedStack _compressedStack;

		private static SecurityContext _fullTrustSC;

		internal bool isNewCapture;

		internal SecurityContextDisableFlow _disableFlow;

		internal static RuntimeHelpers.TryCode tryCode;

		internal static RuntimeHelpers.CleanupCode cleanupCode;

		internal static SecurityContext FullTrustSecurityContext
		{
			get
			{
				if (_fullTrustSC == null)
				{
					_fullTrustSC = CreateFullTrustSecurityContext();
				}
				return _fullTrustSC;
			}
		}

		internal ExecutionContext ExecutionContext
		{
			[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
			set
			{
				_executionContext = value;
			}
		}

		internal WindowsIdentity WindowsIdentity
		{
			get
			{
				return _windowsIdentity;
			}
			set
			{
				_windowsIdentity = value;
			}
		}

		internal CompressedStack CompressedStack
		{
			get
			{
				return _compressedStack;
			}
			[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
			set
			{
				_compressedStack = value;
			}
		}

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		internal SecurityContext()
		{
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
		public static AsyncFlowControl SuppressFlow()
		{
			return SuppressFlow(SecurityContextDisableFlow.All);
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
		public static AsyncFlowControl SuppressFlowWindowsIdentity()
		{
			return SuppressFlow(SecurityContextDisableFlow.WI);
		}

		internal static AsyncFlowControl SuppressFlow(SecurityContextDisableFlow flags)
		{
			if (IsFlowSuppressed(flags))
			{
				throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_CannotSupressFlowMultipleTimes"));
			}
			if (Thread.CurrentThread.ExecutionContext.SecurityContext == null)
			{
				Thread.CurrentThread.ExecutionContext.SecurityContext = new SecurityContext();
			}
			AsyncFlowControl result = default(AsyncFlowControl);
			result.Setup(flags);
			return result;
		}

		public static void RestoreFlow()
		{
			SecurityContext currentSecurityContextNoCreate = GetCurrentSecurityContextNoCreate();
			if (currentSecurityContextNoCreate == null || currentSecurityContextNoCreate._disableFlow == SecurityContextDisableFlow.Nothing)
			{
				throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_CannotRestoreUnsupressedFlow"));
			}
			currentSecurityContextNoCreate._disableFlow = SecurityContextDisableFlow.Nothing;
		}

		public static bool IsFlowSuppressed()
		{
			return IsFlowSuppressed(SecurityContextDisableFlow.All);
		}

		public static bool IsWindowsIdentityFlowSuppressed()
		{
			if (!_LegacyImpersonationPolicy)
			{
				return IsFlowSuppressed(SecurityContextDisableFlow.WI);
			}
			return true;
		}

		internal static bool IsFlowSuppressed(SecurityContextDisableFlow flags)
		{
			SecurityContext currentSecurityContextNoCreate = GetCurrentSecurityContextNoCreate();
			if (currentSecurityContextNoCreate != null)
			{
				return (currentSecurityContextNoCreate._disableFlow & flags) == flags;
			}
			return false;
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
		public static void Run(SecurityContext securityContext, ContextCallback callback, object state)
		{
			if (securityContext == null)
			{
				throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_NullContext"));
			}
			if (!securityContext.isNewCapture)
			{
				throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_NotNewCaptureContext"));
			}
			securityContext.isNewCapture = false;
			ExecutionContext executionContextNoCreate = Thread.CurrentThread.GetExecutionContextNoCreate();
			if (CurrentlyInDefaultFTSecurityContext(executionContextNoCreate) && securityContext.IsDefaultFTSecurityContext())
			{
				callback(state);
			}
			else
			{
				RunInternal(securityContext, callback, state);
			}
		}

		internal static void RunInternal(SecurityContext securityContext, ContextCallback callBack, object state)
		{
			if (cleanupCode == null)
			{
				tryCode = runTryCode;
				cleanupCode = runFinallyCode;
			}
			SecurityContextRunData userData = new SecurityContextRunData(securityContext, callBack, state);
			RuntimeHelpers.ExecuteCodeWithGuaranteedCleanup(tryCode, cleanupCode, userData);
		}

		internal static void runTryCode(object userData)
		{
			SecurityContextRunData securityContextRunData = (SecurityContextRunData)userData;
			securityContextRunData.scsw = SetSecurityContext(securityContextRunData.sc, Thread.CurrentThread.ExecutionContext.SecurityContext);
			securityContextRunData.callBack(securityContextRunData.state);
		}

		[PrePrepareMethod]
		internal static void runFinallyCode(object userData, bool exceptionThrown)
		{
			SecurityContextRunData securityContextRunData = (SecurityContextRunData)userData;
			securityContextRunData.scsw.Undo();
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		internal static SecurityContextSwitcher SetSecurityContext(SecurityContext sc, SecurityContext prevSecurityContext)
		{
			StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
			return SetSecurityContext(sc, prevSecurityContext, ref stackMark);
		}

		internal static SecurityContextSwitcher SetSecurityContext(SecurityContext sc, SecurityContext prevSecurityContext, ref StackCrawlMark stackMark)
		{
			SecurityContextDisableFlow disableFlow = sc._disableFlow;
			sc._disableFlow = SecurityContextDisableFlow.Nothing;
			SecurityContextSwitcher result = default(SecurityContextSwitcher);
			result.currSC = sc;
			ExecutionContext executionContext = (result.currEC = Thread.CurrentThread.ExecutionContext);
			result.prevSC = prevSecurityContext;
			executionContext.SecurityContext = sc;
			if (sc != null)
			{
				RuntimeHelpers.PrepareConstrainedRegions();
				try
				{
					result.wic = null;
					if (!_LegacyImpersonationPolicy)
					{
						if (sc.WindowsIdentity != null)
						{
							result.wic = sc.WindowsIdentity.Impersonate(ref stackMark);
						}
						else if ((disableFlow & SecurityContextDisableFlow.WI) == 0 && prevSecurityContext != null && prevSecurityContext.WindowsIdentity != null)
						{
							result.wic = WindowsIdentity.SafeImpersonate(SafeTokenHandle.InvalidHandle, null, ref stackMark);
						}
					}
					result.cssw = CompressedStack.SetCompressedStack(sc.CompressedStack, prevSecurityContext?.CompressedStack);
					return result;
				}
				catch
				{
					result.UndoNoThrow();
					throw;
				}
			}
			return result;
		}

		public SecurityContext CreateCopy()
		{
			if (!isNewCapture)
			{
				throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_NotNewCaptureContext"));
			}
			SecurityContext securityContext = new SecurityContext();
			securityContext.isNewCapture = true;
			securityContext._disableFlow = _disableFlow;
			securityContext._windowsIdentity = WindowsIdentity;
			if (_compressedStack != null)
			{
				securityContext._compressedStack = _compressedStack.CreateCopy();
			}
			return securityContext;
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public static SecurityContext Capture()
		{
			if (IsFlowSuppressed() || !SecurityManager._IsSecurityOn())
			{
				return null;
			}
			StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
			SecurityContext securityContext = Capture(Thread.CurrentThread.GetExecutionContextNoCreate(), ref stackMark);
			if (securityContext == null)
			{
				securityContext = CreateFullTrustSecurityContext();
			}
			return securityContext;
		}

		internal static SecurityContext Capture(ExecutionContext currThreadEC, ref StackCrawlMark stackMark)
		{
			if (IsFlowSuppressed() || !SecurityManager._IsSecurityOn())
			{
				return null;
			}
			if (CurrentlyInDefaultFTSecurityContext(currThreadEC))
			{
				return null;
			}
			SecurityContext securityContext = new SecurityContext();
			securityContext.isNewCapture = true;
			if (!IsWindowsIdentityFlowSuppressed())
			{
				securityContext._windowsIdentity = GetCurrentWI(currThreadEC);
			}
			else
			{
				securityContext._disableFlow = SecurityContextDisableFlow.WI;
			}
			securityContext.CompressedStack = CompressedStack.GetCompressedStack(ref stackMark);
			return securityContext;
		}

		internal static SecurityContext CreateFullTrustSecurityContext()
		{
			SecurityContext securityContext = new SecurityContext();
			securityContext.isNewCapture = true;
			if (IsWindowsIdentityFlowSuppressed())
			{
				securityContext._disableFlow = SecurityContextDisableFlow.WI;
			}
			securityContext.CompressedStack = new CompressedStack(null);
			return securityContext;
		}

		internal static SecurityContext GetCurrentSecurityContextNoCreate()
		{
			return Thread.CurrentThread.GetExecutionContextNoCreate()?.SecurityContext;
		}

		internal static WindowsIdentity GetCurrentWI(ExecutionContext threadEC)
		{
			if (_alwaysFlowImpersonationPolicy)
			{
				return WindowsIdentity.GetCurrentInternal(TokenAccessLevels.MaximumAllowed, threadOnly: true);
			}
			return (threadEC?.SecurityContext)?.WindowsIdentity;
		}

		internal bool IsDefaultFTSecurityContext()
		{
			if (WindowsIdentity == null)
			{
				if (CompressedStack != null)
				{
					return CompressedStack.CompressedStackHandle == null;
				}
				return true;
			}
			return false;
		}

		internal static bool CurrentlyInDefaultFTSecurityContext(ExecutionContext threadEC)
		{
			if (IsDefaultThreadSecurityInfo())
			{
				return GetCurrentWI(threadEC) == null;
			}
			return false;
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		internal static extern WindowsImpersonationFlowMode GetImpersonationFlowMode();

		[MethodImpl(MethodImplOptions.InternalCall)]
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		internal static extern bool IsDefaultThreadSecurityInfo();
	}
}
