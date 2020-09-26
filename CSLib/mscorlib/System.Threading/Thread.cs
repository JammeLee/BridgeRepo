using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Contexts;
using System.Runtime.Remoting.Messaging;
using System.Security.Permissions;
using System.Security.Principal;

namespace System.Threading
{
	[ComVisible(true)]
	[ComDefaultInterface(typeof(_Thread))]
	[ClassInterface(ClassInterfaceType.None)]
	public sealed class Thread : CriticalFinalizerObject, _Thread
	{
		private const int STATICS_BUCKET_SIZE = 32;

		private Context m_Context;

		private ExecutionContext m_ExecutionContext;

		private string m_Name;

		private Delegate m_Delegate;

		private object[][] m_ThreadStaticsBuckets;

		private int[] m_ThreadStaticsBits;

		private CultureInfo m_CurrentCulture;

		private CultureInfo m_CurrentUICulture;

		private object m_ThreadStartArg;

		private IntPtr DONT_USE_InternalThread;

		private int m_Priority;

		private int m_ManagedThreadId;

		private static LocalDataStoreMgr s_LocalDataStoreMgr = null;

		private static object s_SyncObject = new object();

		public int ManagedThreadId
		{
			[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
			get
			{
				return m_ManagedThreadId;
			}
		}

		public ExecutionContext ExecutionContext
		{
			[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
			get
			{
				if (m_ExecutionContext == null && this == CurrentThread)
				{
					m_ExecutionContext = new ExecutionContext();
					m_ExecutionContext.Thread = this;
				}
				return m_ExecutionContext;
			}
		}

		public ThreadPriority Priority
		{
			get
			{
				return (ThreadPriority)GetPriorityNative();
			}
			[HostProtection(SecurityAction.LinkDemand, SelfAffectingThreading = true)]
			set
			{
				SetPriorityNative((int)value);
			}
		}

		public bool IsAlive => IsAliveNative();

		public bool IsThreadPoolThread => IsThreadpoolThreadNative();

		public static Thread CurrentThread
		{
			[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
			get
			{
				Thread thread = GetFastCurrentThreadNative();
				if (thread == null)
				{
					thread = GetCurrentThreadNative();
				}
				return thread;
			}
		}

		public bool IsBackground
		{
			get
			{
				return IsBackgroundNative();
			}
			[HostProtection(SecurityAction.LinkDemand, SelfAffectingThreading = true)]
			set
			{
				SetBackgroundNative(value);
			}
		}

		public ThreadState ThreadState => (ThreadState)GetThreadStateNative();

		[Obsolete("The ApartmentState property has been deprecated.  Use GetApartmentState, SetApartmentState or TrySetApartmentState instead.", false)]
		public ApartmentState ApartmentState
		{
			get
			{
				return (ApartmentState)GetApartmentStateNative();
			}
			[HostProtection(SecurityAction.LinkDemand, Synchronization = true, SelfAffectingThreading = true)]
			set
			{
				SetApartmentStateNative((int)value, fireMDAOnMismatch: true);
			}
		}

		public CultureInfo CurrentUICulture
		{
			get
			{
				if (m_CurrentUICulture == null)
				{
					return CultureInfo.UserDefaultUICulture;
				}
				CultureInfo safeCulture = null;
				if (!nativeGetSafeCulture(this, GetDomainID(), isUI: true, ref safeCulture) || safeCulture == null)
				{
					return CultureInfo.UserDefaultUICulture;
				}
				return safeCulture;
			}
			[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException("value");
				}
				CultureInfo.VerifyCultureName(value, throwException: true);
				if (!nativeSetThreadUILocale(value.LCID))
				{
					throw new ArgumentException(Environment.GetResourceString("Argument_InvalidResourceCultureName", value.Name));
				}
				value.StartCrossDomainTracking();
				m_CurrentUICulture = value;
			}
		}

		public CultureInfo CurrentCulture
		{
			get
			{
				if (m_CurrentCulture == null)
				{
					return CultureInfo.UserDefaultCulture;
				}
				CultureInfo safeCulture = null;
				if (!nativeGetSafeCulture(this, GetDomainID(), isUI: false, ref safeCulture) || safeCulture == null)
				{
					return CultureInfo.UserDefaultCulture;
				}
				return safeCulture;
			}
			[SecurityPermission(SecurityAction.Demand, ControlThread = true)]
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException("value");
				}
				CultureInfo.CheckNeutral(value);
				CultureInfo.nativeSetThreadLocale(value.LCID);
				value.StartCrossDomainTracking();
				m_CurrentCulture = value;
			}
		}

		public static Context CurrentContext
		{
			[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
			get
			{
				return CurrentThread.GetCurrentContextInternal();
			}
		}

		public static IPrincipal CurrentPrincipal
		{
			get
			{
				lock (CurrentThread)
				{
					IPrincipal principal = CallContext.Principal;
					if (principal == null)
					{
						principal = (CallContext.Principal = GetDomain().GetThreadPrincipal());
					}
					return principal;
				}
			}
			[SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.ControlPrincipal)]
			set
			{
				CallContext.Principal = value;
			}
		}

		public string Name
		{
			get
			{
				return m_Name;
			}
			[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
			set
			{
				lock (this)
				{
					if (m_Name != null)
					{
						throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_WriteOnce"));
					}
					m_Name = value;
					InformThreadNameChangeEx(this, m_Name);
				}
			}
		}

		internal object AbortReason
		{
			get
			{
				object obj = null;
				try
				{
					return GetAbortReason();
				}
				catch (Exception innerException)
				{
					throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_ExceptionStateCrossAppDomain"), innerException);
				}
			}
			set
			{
				SetAbortReason(value);
			}
		}

		private static LocalDataStoreMgr LocalDataStoreManager
		{
			get
			{
				if (s_LocalDataStoreMgr == null)
				{
					lock (s_SyncObject)
					{
						if (s_LocalDataStoreMgr == null)
						{
							s_LocalDataStoreMgr = new LocalDataStoreMgr();
						}
					}
				}
				return s_LocalDataStoreMgr;
			}
		}

		public Thread(ThreadStart start)
		{
			if (start == null)
			{
				throw new ArgumentNullException("start");
			}
			SetStartHelper(start, 0);
		}

		public Thread(ThreadStart start, int maxStackSize)
		{
			if (start == null)
			{
				throw new ArgumentNullException("start");
			}
			if (0 > maxStackSize)
			{
				throw new ArgumentOutOfRangeException("maxStackSize", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
			}
			SetStartHelper(start, maxStackSize);
		}

		public Thread(ParameterizedThreadStart start)
		{
			if (start == null)
			{
				throw new ArgumentNullException("start");
			}
			SetStartHelper(start, 0);
		}

		public Thread(ParameterizedThreadStart start, int maxStackSize)
		{
			if (start == null)
			{
				throw new ArgumentNullException("start");
			}
			if (0 > maxStackSize)
			{
				throw new ArgumentOutOfRangeException("maxStackSize", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
			}
			SetStartHelper(start, maxStackSize);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[ComVisible(false)]
		public override extern int GetHashCode();

		[MethodImpl(MethodImplOptions.NoInlining)]
		[HostProtection(SecurityAction.LinkDemand, Synchronization = true, ExternalThreading = true)]
		public void Start()
		{
			StartupSetApartmentStateInternal();
			if ((object)m_Delegate != null)
			{
				ThreadHelper threadHelper = (ThreadHelper)m_Delegate.Target;
				ExecutionContext executionContext = ExecutionContext.Capture();
				ExecutionContext.ClearSyncContext(executionContext);
				threadHelper.SetExecutionContextHelper(executionContext);
			}
			IPrincipal principal = CallContext.Principal;
			StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
			StartInternal(principal, ref stackMark);
		}

		[HostProtection(SecurityAction.LinkDemand, Synchronization = true, ExternalThreading = true)]
		public void Start(object parameter)
		{
			if (m_Delegate is ThreadStart)
			{
				throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_ThreadWrongThreadStart"));
			}
			m_ThreadStartArg = parameter;
			Start();
		}

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		internal ExecutionContext GetExecutionContextNoCreate()
		{
			return m_ExecutionContext;
		}

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		internal void SetExecutionContext(ExecutionContext value)
		{
			m_ExecutionContext = value;
			if (value != null)
			{
				m_ExecutionContext.Thread = this;
			}
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern void StartInternal(IPrincipal principal, ref StackCrawlMark stackMark);

		[Obsolete("Thread.SetCompressedStack is no longer supported. Please use the System.Threading.CompressedStack class")]
		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		[StrongNameIdentityPermission(SecurityAction.LinkDemand, PublicKey = "0x00000000000000000400000000000000")]
		public void SetCompressedStack(CompressedStack stack)
		{
			throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_ThreadAPIsNotSupported"));
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		internal extern IntPtr SetAppDomainStack(SafeCompressedStackHandle csHandle);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		internal extern void RestoreAppDomainStack(IntPtr appDomainStack);

		[Obsolete("Thread.GetCompressedStack is no longer supported. Please use the System.Threading.CompressedStack class")]
		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		[StrongNameIdentityPermission(SecurityAction.LinkDemand, PublicKey = "0x00000000000000000400000000000000")]
		public CompressedStack GetCompressedStack()
		{
			throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_ThreadAPIsNotSupported"));
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern IntPtr InternalGetCurrentThread();

		[SecurityPermission(SecurityAction.Demand, ControlThread = true)]
		public void Abort(object stateInfo)
		{
			AbortReason = stateInfo;
			AbortInternal();
		}

		[SecurityPermission(SecurityAction.Demand, ControlThread = true)]
		public void Abort()
		{
			AbortInternal();
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern void AbortInternal();

		[SecurityPermission(SecurityAction.Demand, ControlThread = true)]
		public static void ResetAbort()
		{
			Thread currentThread = CurrentThread;
			if ((currentThread.ThreadState & ThreadState.AbortRequested) == 0)
			{
				throw new ThreadStateException(Environment.GetResourceString("ThreadState_NoAbortRequested"));
			}
			currentThread.ResetAbortNative();
			currentThread.ClearAbortReason();
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern void ResetAbortNative();

		[Obsolete("Thread.Suspend has been deprecated.  Please use other classes in System.Threading, such as Monitor, Mutex, Event, and Semaphore, to synchronize Threads or protect resources.  http://go.microsoft.com/fwlink/?linkid=14202", false)]
		[SecurityPermission(SecurityAction.Demand, ControlThread = true)]
		[SecurityPermission(SecurityAction.Demand, ControlThread = true)]
		public void Suspend()
		{
			SuspendInternal();
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern void SuspendInternal();

		[Obsolete("Thread.Resume has been deprecated.  Please use other classes in System.Threading, such as Monitor, Mutex, Event, and Semaphore, to synchronize Threads or protect resources.  http://go.microsoft.com/fwlink/?linkid=14202", false)]
		[SecurityPermission(SecurityAction.Demand, ControlThread = true)]
		public void Resume()
		{
			ResumeInternal();
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern void ResumeInternal();

		[SecurityPermission(SecurityAction.Demand, ControlThread = true)]
		public void Interrupt()
		{
			InterruptInternal();
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern void InterruptInternal();

		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern int GetPriorityNative();

		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern void SetPriorityNative(int priority);

		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern bool IsAliveNative();

		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern bool IsThreadpoolThreadNative();

		[MethodImpl(MethodImplOptions.InternalCall)]
		[HostProtection(SecurityAction.LinkDemand, Synchronization = true, ExternalThreading = true)]
		private extern void JoinInternal();

		[HostProtection(SecurityAction.LinkDemand, Synchronization = true, ExternalThreading = true)]
		public void Join()
		{
			JoinInternal();
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[HostProtection(SecurityAction.LinkDemand, Synchronization = true, ExternalThreading = true)]
		private extern bool JoinInternal(int millisecondsTimeout);

		[HostProtection(SecurityAction.LinkDemand, Synchronization = true, ExternalThreading = true)]
		public bool Join(int millisecondsTimeout)
		{
			return JoinInternal(millisecondsTimeout);
		}

		[HostProtection(SecurityAction.LinkDemand, Synchronization = true, ExternalThreading = true)]
		public bool Join(TimeSpan timeout)
		{
			long num = (long)timeout.TotalMilliseconds;
			if (num < -1 || num > int.MaxValue)
			{
				throw new ArgumentOutOfRangeException("timeout", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegOrNegative1"));
			}
			return Join((int)num);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void SleepInternal(int millisecondsTimeout);

		public static void Sleep(int millisecondsTimeout)
		{
			SleepInternal(millisecondsTimeout);
		}

		public static void Sleep(TimeSpan timeout)
		{
			long num = (long)timeout.TotalMilliseconds;
			if (num < -1 || num > int.MaxValue)
			{
				throw new ArgumentOutOfRangeException("timeout", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegOrNegative1"));
			}
			Sleep((int)num);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		[HostProtection(SecurityAction.LinkDemand, Synchronization = true, ExternalThreading = true)]
		private static extern void SpinWaitInternal(int iterations);

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		[HostProtection(SecurityAction.LinkDemand, Synchronization = true, ExternalThreading = true)]
		public static void SpinWait(int iterations)
		{
			SpinWaitInternal(iterations);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		private static extern Thread GetCurrentThreadNative();

		[MethodImpl(MethodImplOptions.InternalCall)]
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		private static extern Thread GetFastCurrentThreadNative();

		private void SetStartHelper(Delegate start, int maxStackSize)
		{
			ThreadHelper @object = new ThreadHelper(start);
			if (start is ThreadStart)
			{
				SetStart(new ThreadStart(@object.ThreadStart), maxStackSize);
			}
			else
			{
				SetStart(new ParameterizedThreadStart(@object.ThreadStart), maxStackSize);
			}
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern void SetStart(Delegate start, int maxStackSize);

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		~Thread()
		{
			InternalFinalize();
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		private extern void InternalFinalize();

		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern bool IsBackgroundNative();

		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern void SetBackgroundNative(bool isBackground);

		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern int GetThreadStateNative();

		public ApartmentState GetApartmentState()
		{
			return (ApartmentState)GetApartmentStateNative();
		}

		[HostProtection(SecurityAction.LinkDemand, Synchronization = true, SelfAffectingThreading = true)]
		public bool TrySetApartmentState(ApartmentState state)
		{
			return SetApartmentStateHelper(state, fireMDAOnMismatch: false);
		}

		[HostProtection(SecurityAction.LinkDemand, Synchronization = true, SelfAffectingThreading = true)]
		public void SetApartmentState(ApartmentState state)
		{
			if (!SetApartmentStateHelper(state, fireMDAOnMismatch: true))
			{
				throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_ApartmentStateSwitchFailed"));
			}
		}

		private bool SetApartmentStateHelper(ApartmentState state, bool fireMDAOnMismatch)
		{
			ApartmentState apartmentState = (ApartmentState)SetApartmentStateNative((int)state, fireMDAOnMismatch);
			if (state == ApartmentState.Unknown && apartmentState == ApartmentState.MTA)
			{
				return true;
			}
			if (apartmentState != state)
			{
				return false;
			}
			return true;
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern int GetApartmentStateNative();

		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern int SetApartmentStateNative(int state, bool fireMDAOnMismatch);

		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern int StartupSetApartmentStateInternal();

		[HostProtection(SecurityAction.LinkDemand, SharedState = true, ExternalThreading = true)]
		public static LocalDataStoreSlot AllocateDataSlot()
		{
			return LocalDataStoreManager.AllocateDataSlot();
		}

		[HostProtection(SecurityAction.LinkDemand, SharedState = true, ExternalThreading = true)]
		public static LocalDataStoreSlot AllocateNamedDataSlot(string name)
		{
			return LocalDataStoreManager.AllocateNamedDataSlot(name);
		}

		[HostProtection(SecurityAction.LinkDemand, SharedState = true, ExternalThreading = true)]
		public static LocalDataStoreSlot GetNamedDataSlot(string name)
		{
			return LocalDataStoreManager.GetNamedDataSlot(name);
		}

		[HostProtection(SecurityAction.LinkDemand, SharedState = true, ExternalThreading = true)]
		public static void FreeNamedDataSlot(string name)
		{
			LocalDataStoreManager.FreeNamedDataSlot(name);
		}

		[HostProtection(SecurityAction.LinkDemand, SharedState = true, ExternalThreading = true)]
		public static object GetData(LocalDataStoreSlot slot)
		{
			LocalDataStoreManager.ValidateSlot(slot);
			return GetDomainLocalStore()?.GetData(slot);
		}

		[HostProtection(SecurityAction.LinkDemand, SharedState = true, ExternalThreading = true)]
		public static void SetData(LocalDataStoreSlot slot, object data)
		{
			LocalDataStore localDataStore = GetDomainLocalStore();
			if (localDataStore == null)
			{
				localDataStore = LocalDataStoreManager.CreateLocalDataStore();
				SetDomainLocalStore(localDataStore);
			}
			localDataStore.SetData(slot, data);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern LocalDataStore GetDomainLocalStore();

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void SetDomainLocalStore(LocalDataStore dls);

		private static void RemoveDomainLocalStore(LocalDataStore dls)
		{
			if (dls != null)
			{
				LocalDataStoreManager.DeleteLocalDataStore(dls);
			}
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern bool nativeGetSafeCulture(Thread t, int appDomainId, bool isUI, ref CultureInfo safeCulture);

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern bool nativeSetThreadUILocale(int LCID);

		private int ReserveSlot()
		{
			if (m_ThreadStaticsBuckets == null)
			{
				object[][] array = new object[1][];
				SetIsThreadStaticsArray(array);
				array[0] = new object[32];
				SetIsThreadStaticsArray(array[0]);
				int[] array2 = new int[array.Length * 32 / 32];
				for (int i = 0; i < array2.Length; i++)
				{
					array2[i] = -1;
				}
				array2[0] &= -2;
				array2[0] &= -3;
				m_ThreadStaticsBits = array2;
				m_ThreadStaticsBuckets = array;
				return 1;
			}
			int num = FindSlot();
			if (num == 0)
			{
				int num2 = m_ThreadStaticsBuckets.Length;
				int num3 = m_ThreadStaticsBits.Length;
				int num4 = m_ThreadStaticsBuckets.Length + 1;
				object[][] array3 = new object[num4][];
				SetIsThreadStaticsArray(array3);
				int num5 = num4 * 32 / 32;
				int[] array4 = new int[num5];
				Array.Copy(m_ThreadStaticsBuckets, array3, m_ThreadStaticsBuckets.Length);
				for (int j = num2; j < num4; j++)
				{
					array3[j] = new object[32];
					SetIsThreadStaticsArray(array3[j]);
				}
				Array.Copy(m_ThreadStaticsBits, array4, m_ThreadStaticsBits.Length);
				for (int k = num3; k < num5; k++)
				{
					array4[k] = -1;
				}
				array4[num3] &= -2;
				m_ThreadStaticsBits = array4;
				m_ThreadStaticsBuckets = array3;
				return num2 * 32;
			}
			return num;
		}

		private int FindSlot()
		{
			int num = 0;
			int num2 = 0;
			bool flag = false;
			if (m_ThreadStaticsBits.Length != 0 && m_ThreadStaticsBits.Length != m_ThreadStaticsBuckets.Length * 32 / 32)
			{
				return 0;
			}
			int i;
			for (i = 0; i < m_ThreadStaticsBits.Length; i++)
			{
				num2 = m_ThreadStaticsBits[i];
				if (num2 == 0)
				{
					continue;
				}
				if (((uint)num2 & 0xFFFFu) != 0)
				{
					num2 &= 0xFFFF;
				}
				else
				{
					num2 = (num2 >> 16) & 0xFFFF;
					num += 16;
				}
				if (((uint)num2 & 0xFFu) != 0)
				{
					num2 &= 0xFF;
				}
				else
				{
					num += 8;
					num2 = (num2 >> 8) & 0xFF;
				}
				int j;
				for (j = 0; j < 8; j++)
				{
					if ((num2 & (1 << j)) != 0)
					{
						flag = true;
						break;
					}
				}
				num += j;
				m_ThreadStaticsBits[i] &= ~(1 << num);
				break;
			}
			if (flag)
			{
				num += 32 * i;
			}
			return num;
		}

		internal Context GetCurrentContextInternal()
		{
			if (m_Context == null)
			{
				m_Context = Context.DefaultContext;
			}
			return m_Context;
		}

		[HostProtection(SecurityAction.LinkDemand, SharedState = true, ExternalThreading = true)]
		internal LogicalCallContext GetLogicalCallContext()
		{
			return ExecutionContext.LogicalCallContext;
		}

		[HostProtection(SecurityAction.LinkDemand, SharedState = true, ExternalThreading = true)]
		internal LogicalCallContext SetLogicalCallContext(LogicalCallContext callCtx)
		{
			LogicalCallContext logicalCallContext = ExecutionContext.LogicalCallContext;
			ExecutionContext.LogicalCallContext = callCtx;
			return logicalCallContext;
		}

		internal IllogicalCallContext GetIllogicalCallContext()
		{
			return ExecutionContext.IllogicalCallContext;
		}

		private void SetPrincipalInternal(IPrincipal principal)
		{
			GetLogicalCallContext().SecurityData.Principal = principal;
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern Context GetContextInternal(IntPtr id);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal extern object InternalCrossContextCallback(Context ctx, IntPtr ctxID, int appDomainID, InternalCrossContextDelegate ftnToCall, object[] args);

		internal object InternalCrossContextCallback(Context ctx, InternalCrossContextDelegate ftnToCall, object[] args)
		{
			return InternalCrossContextCallback(ctx, ctx.InternalContextID, 0, ftnToCall, args);
		}

		private static object CompleteCrossContextCallback(InternalCrossContextDelegate ftnToCall, object[] args)
		{
			return ftnToCall(args);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern AppDomain GetDomainInternal();

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern AppDomain GetFastDomainInternal();

		public static AppDomain GetDomain()
		{
			if (CurrentThread.m_Context == null)
			{
				AppDomain appDomain = GetFastDomainInternal();
				if (appDomain == null)
				{
					appDomain = GetDomainInternal();
				}
				return appDomain;
			}
			return CurrentThread.m_Context.AppDomain;
		}

		public static int GetDomainID()
		{
			return GetDomain().GetId();
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern void InformThreadNameChangeEx(Thread t, string name);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		[HostProtection(SecurityAction.LinkDemand, Synchronization = true, ExternalThreading = true)]
		public static extern void BeginCriticalRegion();

		[MethodImpl(MethodImplOptions.InternalCall)]
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		[HostProtection(SecurityAction.LinkDemand, Synchronization = true, ExternalThreading = true)]
		public static extern void EndCriticalRegion();

		[MethodImpl(MethodImplOptions.InternalCall)]
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		[SecurityPermission(SecurityAction.LinkDemand, ControlThread = true)]
		public static extern void BeginThreadAffinity();

		[MethodImpl(MethodImplOptions.InternalCall)]
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		[SecurityPermission(SecurityAction.LinkDemand, ControlThread = true)]
		public static extern void EndThreadAffinity();

		[MethodImpl(MethodImplOptions.NoInlining)]
		public static byte VolatileRead(ref byte address)
		{
			byte result = address;
			MemoryBarrier();
			return result;
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public static short VolatileRead(ref short address)
		{
			short result = address;
			MemoryBarrier();
			return result;
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public static int VolatileRead(ref int address)
		{
			int result = address;
			MemoryBarrier();
			return result;
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public static long VolatileRead(ref long address)
		{
			long result = address;
			MemoryBarrier();
			return result;
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		[CLSCompliant(false)]
		public static sbyte VolatileRead(ref sbyte address)
		{
			sbyte result = address;
			MemoryBarrier();
			return result;
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		[CLSCompliant(false)]
		public static ushort VolatileRead(ref ushort address)
		{
			ushort result = address;
			MemoryBarrier();
			return result;
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		[CLSCompliant(false)]
		public static uint VolatileRead(ref uint address)
		{
			uint result = address;
			MemoryBarrier();
			return result;
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public static IntPtr VolatileRead(ref IntPtr address)
		{
			IntPtr result = address;
			MemoryBarrier();
			return result;
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		[CLSCompliant(false)]
		public static UIntPtr VolatileRead(ref UIntPtr address)
		{
			UIntPtr result = address;
			MemoryBarrier();
			return result;
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		[CLSCompliant(false)]
		public static ulong VolatileRead(ref ulong address)
		{
			ulong result = address;
			MemoryBarrier();
			return result;
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public static float VolatileRead(ref float address)
		{
			float result = address;
			MemoryBarrier();
			return result;
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public static double VolatileRead(ref double address)
		{
			double result = address;
			MemoryBarrier();
			return result;
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public static object VolatileRead(ref object address)
		{
			object result = address;
			MemoryBarrier();
			return result;
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public static void VolatileWrite(ref byte address, byte value)
		{
			MemoryBarrier();
			address = value;
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public static void VolatileWrite(ref short address, short value)
		{
			MemoryBarrier();
			address = value;
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public static void VolatileWrite(ref int address, int value)
		{
			MemoryBarrier();
			address = value;
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public static void VolatileWrite(ref long address, long value)
		{
			MemoryBarrier();
			address = value;
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		[CLSCompliant(false)]
		public static void VolatileWrite(ref sbyte address, sbyte value)
		{
			MemoryBarrier();
			address = value;
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		[CLSCompliant(false)]
		public static void VolatileWrite(ref ushort address, ushort value)
		{
			MemoryBarrier();
			address = value;
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		[CLSCompliant(false)]
		public static void VolatileWrite(ref uint address, uint value)
		{
			MemoryBarrier();
			address = value;
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public static void VolatileWrite(ref IntPtr address, IntPtr value)
		{
			MemoryBarrier();
			address = value;
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		[CLSCompliant(false)]
		public static void VolatileWrite(ref UIntPtr address, UIntPtr value)
		{
			MemoryBarrier();
			address = value;
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		[CLSCompliant(false)]
		public static void VolatileWrite(ref ulong address, ulong value)
		{
			MemoryBarrier();
			address = value;
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public static void VolatileWrite(ref float address, float value)
		{
			MemoryBarrier();
			address = value;
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public static void VolatileWrite(ref double address, double value)
		{
			MemoryBarrier();
			address = value;
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public static void VolatileWrite(ref object address, object value)
		{
			MemoryBarrier();
			address = value;
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		public static extern void MemoryBarrier();

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void SetIsThreadStaticsArray(object o);

		void _Thread.GetTypeInfoCount(out uint pcTInfo)
		{
			throw new NotImplementedException();
		}

		void _Thread.GetTypeInfo(uint iTInfo, uint lcid, IntPtr ppTInfo)
		{
			throw new NotImplementedException();
		}

		void _Thread.GetIDsOfNames([In] ref Guid riid, IntPtr rgszNames, uint cNames, uint lcid, IntPtr rgDispId)
		{
			throw new NotImplementedException();
		}

		void _Thread.Invoke(uint dispIdMember, [In] ref Guid riid, uint lcid, short wFlags, IntPtr pDispParams, IntPtr pVarResult, IntPtr pExcepInfo, IntPtr puArgErr)
		{
			throw new NotImplementedException();
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal extern void SetAbortReason(object o);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal extern object GetAbortReason();

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal extern void ClearAbortReason();
	}
}
