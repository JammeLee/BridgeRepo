using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;

namespace System.Diagnostics
{
	[Designer("System.Diagnostics.Design.ProcessDesigner, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
	[DefaultEvent("Exited")]
	[DefaultProperty("StartInfo")]
	[MonitoringDescription("ProcessDesc")]
	[PermissionSet(SecurityAction.InheritanceDemand, Name = "FullTrust")]
	[HostProtection(SecurityAction.LinkDemand, SharedState = true, Synchronization = true, ExternalProcessMgmt = true, SelfAffectingProcessMgmt = true)]
	[PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
	public class Process : Component
	{
		private enum StreamReadMode
		{
			undefined,
			syncMode,
			asyncMode
		}

		private enum State
		{
			HaveId = 1,
			IsLocal = 2,
			IsNt = 4,
			HaveProcessInfo = 8,
			Exited = 0x10,
			Associated = 0x20,
			IsWin2k = 0x40,
			HaveNtProcessInfo = 12
		}

		private bool haveProcessId;

		private int processId;

		private bool haveProcessHandle;

		private Microsoft.Win32.SafeHandles.SafeProcessHandle m_processHandle;

		private bool isRemoteMachine;

		private string machineName;

		private ProcessInfo processInfo;

		private ProcessThreadCollection threads;

		private ProcessModuleCollection modules;

		private bool haveMainWindow;

		private IntPtr mainWindowHandle;

		private string mainWindowTitle;

		private bool haveWorkingSetLimits;

		private IntPtr minWorkingSet;

		private IntPtr maxWorkingSet;

		private bool haveProcessorAffinity;

		private IntPtr processorAffinity;

		private bool havePriorityClass;

		private ProcessPriorityClass priorityClass;

		private ProcessStartInfo startInfo;

		private bool watchForExit;

		private bool watchingForExit;

		private EventHandler onExited;

		private bool exited;

		private int exitCode;

		private bool signaled;

		private DateTime exitTime;

		private bool haveExitTime;

		private bool responding;

		private bool haveResponding;

		private bool priorityBoostEnabled;

		private bool havePriorityBoostEnabled;

		private bool raisedOnExited;

		private RegisteredWaitHandle registeredWaitHandle;

		private WaitHandle waitHandle;

		private ISynchronizeInvoke synchronizingObject;

		private StreamReader standardOutput;

		private StreamWriter standardInput;

		private StreamReader standardError;

		private OperatingSystem operatingSystem;

		private bool disposed;

		private StreamReadMode outputStreamReadMode;

		private StreamReadMode errorStreamReadMode;

		internal AsyncStreamReader output;

		internal AsyncStreamReader error;

		internal bool pendingOutputRead;

		internal bool pendingErrorRead;

		private static SafeFileHandle InvalidPipeHandle = new SafeFileHandle(IntPtr.Zero, ownsHandle: false);

		internal static TraceSwitch processTracing = null;

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[MonitoringDescription("ProcessAssociated")]
		[Browsable(false)]
		private bool Associated
		{
			get
			{
				if (!haveProcessId)
				{
					return haveProcessHandle;
				}
				return true;
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[MonitoringDescription("ProcessBasePriority")]
		public int BasePriority
		{
			get
			{
				EnsureState(State.HaveProcessInfo);
				return processInfo.basePriority;
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		[MonitoringDescription("ProcessExitCode")]
		public int ExitCode
		{
			get
			{
				EnsureState(State.Exited);
				return exitCode;
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		[MonitoringDescription("ProcessTerminated")]
		public bool HasExited
		{
			get
			{
				if (!exited)
				{
					EnsureState(State.Associated);
					Microsoft.Win32.SafeHandles.SafeProcessHandle safeProcessHandle = null;
					try
					{
						safeProcessHandle = GetProcessHandle(1049600, throwIfExited: false);
						int num;
						if (safeProcessHandle.IsInvalid)
						{
							exited = true;
						}
						else if (NativeMethods.GetExitCodeProcess(safeProcessHandle, out num) && num != 259)
						{
							exited = true;
							exitCode = num;
						}
						else
						{
							if (!signaled)
							{
								ProcessWaitHandle processWaitHandle = null;
								try
								{
									processWaitHandle = new ProcessWaitHandle(safeProcessHandle);
									signaled = processWaitHandle.WaitOne(0, exitContext: false);
								}
								finally
								{
									processWaitHandle?.Close();
								}
							}
							if (signaled)
							{
								if (!NativeMethods.GetExitCodeProcess(safeProcessHandle, out num))
								{
									throw new Win32Exception();
								}
								exited = true;
								exitCode = num;
							}
						}
					}
					finally
					{
						ReleaseProcessHandle(safeProcessHandle);
					}
					if (exited)
					{
						RaiseOnExited();
					}
				}
				return exited;
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[MonitoringDescription("ProcessExitTime")]
		public DateTime ExitTime
		{
			get
			{
				if (!haveExitTime)
				{
					EnsureState((State)20);
					exitTime = GetProcessTimes().ExitTime;
					haveExitTime = true;
				}
				return exitTime;
			}
		}

		[MonitoringDescription("ProcessHandle")]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public IntPtr Handle
		{
			get
			{
				EnsureState(State.Associated);
				return OpenProcessHandle().DangerousGetHandle();
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[MonitoringDescription("ProcessHandleCount")]
		public int HandleCount
		{
			get
			{
				EnsureState(State.HaveProcessInfo);
				return processInfo.handleCount;
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[MonitoringDescription("ProcessId")]
		public int Id
		{
			get
			{
				EnsureState(State.HaveId);
				return processId;
			}
		}

		[MonitoringDescription("ProcessMachineName")]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public string MachineName
		{
			get
			{
				EnsureState(State.Associated);
				return machineName;
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[MonitoringDescription("ProcessMainWindowHandle")]
		public IntPtr MainWindowHandle
		{
			get
			{
				if (!haveMainWindow)
				{
					EnsureState((State)10);
					mainWindowHandle = ProcessManager.GetMainWindowHandle(processInfo);
					haveMainWindow = true;
				}
				return mainWindowHandle;
			}
		}

		[MonitoringDescription("ProcessMainWindowTitle")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public string MainWindowTitle
		{
			get
			{
				if (mainWindowTitle == null)
				{
					IntPtr intPtr = MainWindowHandle;
					if (intPtr == (IntPtr)0)
					{
						mainWindowTitle = string.Empty;
					}
					else
					{
						int capacity = NativeMethods.GetWindowTextLength(new HandleRef(this, intPtr)) * 2;
						StringBuilder stringBuilder = new StringBuilder(capacity);
						NativeMethods.GetWindowText(new HandleRef(this, intPtr), stringBuilder, stringBuilder.Capacity);
						mainWindowTitle = stringBuilder.ToString();
					}
				}
				return mainWindowTitle;
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		[MonitoringDescription("ProcessMainModule")]
		public ProcessModule MainModule
		{
			get
			{
				if (OperatingSystem.Platform == PlatformID.Win32NT)
				{
					EnsureState((State)3);
					ModuleInfo firstModuleInfo = NtProcessManager.GetFirstModuleInfo(processId);
					return new ProcessModule(firstModuleInfo);
				}
				ProcessModuleCollection processModuleCollection = Modules;
				EnsureState(State.HaveProcessInfo);
				foreach (ProcessModule item in processModuleCollection)
				{
					if (item.moduleInfo.Id == processInfo.mainModuleId)
					{
						return item;
					}
				}
				return null;
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[MonitoringDescription("ProcessMaxWorkingSet")]
		public IntPtr MaxWorkingSet
		{
			get
			{
				EnsureWorkingSetLimits();
				return maxWorkingSet;
			}
			set
			{
				SetWorkingSetLimits(null, value);
			}
		}

		[MonitoringDescription("ProcessMinWorkingSet")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public IntPtr MinWorkingSet
		{
			get
			{
				EnsureWorkingSetLimits();
				return minWorkingSet;
			}
			set
			{
				SetWorkingSetLimits(value, null);
			}
		}

		[MonitoringDescription("ProcessModules")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		public ProcessModuleCollection Modules
		{
			get
			{
				if (modules == null)
				{
					EnsureState((State)3);
					ModuleInfo[] moduleInfos = ProcessManager.GetModuleInfos(processId);
					ProcessModule[] array = new ProcessModule[moduleInfos.Length];
					for (int i = 0; i < moduleInfos.Length; i++)
					{
						array[i] = new ProcessModule(moduleInfos[i]);
					}
					ProcessModuleCollection processModuleCollection = (modules = new ProcessModuleCollection(array));
				}
				return modules;
			}
		}

		[MonitoringDescription("ProcessNonpagedSystemMemorySize")]
		[Obsolete("This property has been deprecated.  Please use System.Diagnostics.Process.NonpagedSystemMemorySize64 instead.  http://go.microsoft.com/fwlink/?linkid=14202")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public int NonpagedSystemMemorySize
		{
			get
			{
				EnsureState(State.HaveNtProcessInfo);
				return (int)processInfo.poolNonpagedBytes;
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[ComVisible(false)]
		[MonitoringDescription("ProcessNonpagedSystemMemorySize")]
		public long NonpagedSystemMemorySize64
		{
			get
			{
				EnsureState(State.HaveNtProcessInfo);
				return processInfo.poolNonpagedBytes;
			}
		}

		[MonitoringDescription("ProcessPagedMemorySize")]
		[Obsolete("This property has been deprecated.  Please use System.Diagnostics.Process.PagedMemorySize64 instead.  http://go.microsoft.com/fwlink/?linkid=14202")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public int PagedMemorySize
		{
			get
			{
				EnsureState(State.HaveNtProcessInfo);
				return (int)processInfo.pageFileBytes;
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[ComVisible(false)]
		[MonitoringDescription("ProcessPagedMemorySize")]
		public long PagedMemorySize64
		{
			get
			{
				EnsureState(State.HaveNtProcessInfo);
				return processInfo.pageFileBytes;
			}
		}

		[MonitoringDescription("ProcessPagedSystemMemorySize")]
		[Obsolete("This property has been deprecated.  Please use System.Diagnostics.Process.PagedSystemMemorySize64 instead.  http://go.microsoft.com/fwlink/?linkid=14202")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public int PagedSystemMemorySize
		{
			get
			{
				EnsureState(State.HaveNtProcessInfo);
				return (int)processInfo.poolPagedBytes;
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[ComVisible(false)]
		[MonitoringDescription("ProcessPagedSystemMemorySize")]
		public long PagedSystemMemorySize64
		{
			get
			{
				EnsureState(State.HaveNtProcessInfo);
				return processInfo.poolPagedBytes;
			}
		}

		[Obsolete("This property has been deprecated.  Please use System.Diagnostics.Process.PeakPagedMemorySize64 instead.  http://go.microsoft.com/fwlink/?linkid=14202")]
		[MonitoringDescription("ProcessPeakPagedMemorySize")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public int PeakPagedMemorySize
		{
			get
			{
				EnsureState(State.HaveNtProcessInfo);
				return (int)processInfo.pageFileBytesPeak;
			}
		}

		[ComVisible(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[MonitoringDescription("ProcessPeakPagedMemorySize")]
		public long PeakPagedMemorySize64
		{
			get
			{
				EnsureState(State.HaveNtProcessInfo);
				return processInfo.pageFileBytesPeak;
			}
		}

		[Obsolete("This property has been deprecated.  Please use System.Diagnostics.Process.PeakWorkingSet64 instead.  http://go.microsoft.com/fwlink/?linkid=14202")]
		[MonitoringDescription("ProcessPeakWorkingSet")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public int PeakWorkingSet
		{
			get
			{
				EnsureState(State.HaveNtProcessInfo);
				return (int)processInfo.workingSetPeak;
			}
		}

		[ComVisible(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[MonitoringDescription("ProcessPeakWorkingSet")]
		public long PeakWorkingSet64
		{
			get
			{
				EnsureState(State.HaveNtProcessInfo);
				return processInfo.workingSetPeak;
			}
		}

		[Obsolete("This property has been deprecated.  Please use System.Diagnostics.Process.PeakVirtualMemorySize64 instead.  http://go.microsoft.com/fwlink/?linkid=14202")]
		[MonitoringDescription("ProcessPeakVirtualMemorySize")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public int PeakVirtualMemorySize
		{
			get
			{
				EnsureState(State.HaveNtProcessInfo);
				return (int)processInfo.virtualBytesPeak;
			}
		}

		[ComVisible(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[MonitoringDescription("ProcessPeakVirtualMemorySize")]
		public long PeakVirtualMemorySize64
		{
			get
			{
				EnsureState(State.HaveNtProcessInfo);
				return processInfo.virtualBytesPeak;
			}
		}

		private OperatingSystem OperatingSystem
		{
			get
			{
				if (operatingSystem == null)
				{
					operatingSystem = Environment.OSVersion;
				}
				return operatingSystem;
			}
		}

		[MonitoringDescription("ProcessPriorityBoostEnabled")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool PriorityBoostEnabled
		{
			get
			{
				EnsureState(State.IsNt);
				if (!havePriorityBoostEnabled)
				{
					Microsoft.Win32.SafeHandles.SafeProcessHandle handle = null;
					try
					{
						handle = GetProcessHandle(1024);
						bool disabled = false;
						if (!NativeMethods.GetProcessPriorityBoost(handle, out disabled))
						{
							throw new Win32Exception();
						}
						priorityBoostEnabled = !disabled;
						havePriorityBoostEnabled = true;
					}
					finally
					{
						ReleaseProcessHandle(handle);
					}
				}
				return priorityBoostEnabled;
			}
			set
			{
				EnsureState(State.IsNt);
				Microsoft.Win32.SafeHandles.SafeProcessHandle handle = null;
				try
				{
					handle = GetProcessHandle(512);
					if (!NativeMethods.SetProcessPriorityBoost(handle, !value))
					{
						throw new Win32Exception();
					}
					priorityBoostEnabled = value;
					havePriorityBoostEnabled = true;
				}
				finally
				{
					ReleaseProcessHandle(handle);
				}
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[MonitoringDescription("ProcessPriorityClass")]
		public ProcessPriorityClass PriorityClass
		{
			get
			{
				if (!havePriorityClass)
				{
					Microsoft.Win32.SafeHandles.SafeProcessHandle handle = null;
					try
					{
						handle = GetProcessHandle(1024);
						int num = NativeMethods.GetPriorityClass(handle);
						if (num == 0)
						{
							throw new Win32Exception();
						}
						priorityClass = (ProcessPriorityClass)num;
						havePriorityClass = true;
					}
					finally
					{
						ReleaseProcessHandle(handle);
					}
				}
				return priorityClass;
			}
			set
			{
				if (!Enum.IsDefined(typeof(ProcessPriorityClass), value))
				{
					throw new InvalidEnumArgumentException("value", (int)value, typeof(ProcessPriorityClass));
				}
				if ((value & (ProcessPriorityClass)49152) != 0 && (OperatingSystem.Platform != PlatformID.Win32NT || OperatingSystem.Version.Major < 5))
				{
					throw new PlatformNotSupportedException(SR.GetString("PriorityClassNotSupported"), null);
				}
				Microsoft.Win32.SafeHandles.SafeProcessHandle handle = null;
				try
				{
					handle = GetProcessHandle(512);
					if (!NativeMethods.SetPriorityClass(handle, (int)value))
					{
						throw new Win32Exception();
					}
					priorityClass = value;
					havePriorityClass = true;
				}
				finally
				{
					ReleaseProcessHandle(handle);
				}
			}
		}

		[MonitoringDescription("ProcessPrivateMemorySize")]
		[Obsolete("This property has been deprecated.  Please use System.Diagnostics.Process.PrivateMemorySize64 instead.  http://go.microsoft.com/fwlink/?linkid=14202")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public int PrivateMemorySize
		{
			get
			{
				EnsureState(State.HaveNtProcessInfo);
				return (int)processInfo.privateBytes;
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[ComVisible(false)]
		[MonitoringDescription("ProcessPrivateMemorySize")]
		public long PrivateMemorySize64
		{
			get
			{
				EnsureState(State.HaveNtProcessInfo);
				return processInfo.privateBytes;
			}
		}

		[MonitoringDescription("ProcessPrivilegedProcessorTime")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public TimeSpan PrivilegedProcessorTime
		{
			get
			{
				EnsureState(State.IsNt);
				return GetProcessTimes().PrivilegedProcessorTime;
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[MonitoringDescription("ProcessProcessName")]
		public string ProcessName
		{
			get
			{
				EnsureState(State.HaveProcessInfo);
				string processName = processInfo.processName;
				if (processName.Length == 15 && ProcessManager.IsNt && ProcessManager.IsOSOlderThanXP && !isRemoteMachine)
				{
					try
					{
						string moduleName = MainModule.ModuleName;
						if (moduleName != null)
						{
							processInfo.processName = Path.ChangeExtension(Path.GetFileName(moduleName), null);
						}
					}
					catch (Exception)
					{
					}
				}
				return processInfo.processName;
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[MonitoringDescription("ProcessProcessorAffinity")]
		public IntPtr ProcessorAffinity
		{
			get
			{
				if (!haveProcessorAffinity)
				{
					Microsoft.Win32.SafeHandles.SafeProcessHandle handle = null;
					try
					{
						handle = GetProcessHandle(1024);
						if (!NativeMethods.GetProcessAffinityMask(handle, out var processMask, out var _))
						{
							throw new Win32Exception();
						}
						processorAffinity = processMask;
					}
					finally
					{
						ReleaseProcessHandle(handle);
					}
					haveProcessorAffinity = true;
				}
				return processorAffinity;
			}
			set
			{
				Microsoft.Win32.SafeHandles.SafeProcessHandle handle = null;
				try
				{
					handle = GetProcessHandle(512);
					if (!NativeMethods.SetProcessAffinityMask(handle, value))
					{
						throw new Win32Exception();
					}
					processorAffinity = value;
					haveProcessorAffinity = true;
				}
				finally
				{
					ReleaseProcessHandle(handle);
				}
			}
		}

		[MonitoringDescription("ProcessResponding")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool Responding
		{
			get
			{
				if (!haveResponding)
				{
					IntPtr intPtr = MainWindowHandle;
					if (intPtr == (IntPtr)0)
					{
						responding = true;
					}
					else
					{
						responding = NativeMethods.SendMessageTimeout(new HandleRef(this, intPtr), 0, IntPtr.Zero, IntPtr.Zero, 2, 5000, out var _) != (IntPtr)0;
					}
				}
				return responding;
			}
		}

		[MonitoringDescription("ProcessSessionId")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public int SessionId
		{
			get
			{
				EnsureState(State.HaveNtProcessInfo);
				return processInfo.sessionId;
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[MonitoringDescription("ProcessStartInfo")]
		public ProcessStartInfo StartInfo
		{
			get
			{
				if (startInfo == null)
				{
					startInfo = new ProcessStartInfo(this);
				}
				return startInfo;
			}
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException("value");
				}
				startInfo = value;
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[MonitoringDescription("ProcessStartTime")]
		public DateTime StartTime
		{
			get
			{
				EnsureState(State.IsNt);
				return GetProcessTimes().StartTime;
			}
		}

		[MonitoringDescription("ProcessSynchronizingObject")]
		[Browsable(false)]
		[DefaultValue(null)]
		public ISynchronizeInvoke SynchronizingObject
		{
			get
			{
				if (synchronizingObject == null && base.DesignMode)
				{
					IDesignerHost designerHost = (IDesignerHost)GetService(typeof(IDesignerHost));
					if (designerHost != null)
					{
						object rootComponent = designerHost.RootComponent;
						if (rootComponent != null && rootComponent is ISynchronizeInvoke)
						{
							synchronizingObject = (ISynchronizeInvoke)rootComponent;
						}
					}
				}
				return synchronizingObject;
			}
			set
			{
				synchronizingObject = value;
			}
		}

		[MonitoringDescription("ProcessThreads")]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public ProcessThreadCollection Threads
		{
			get
			{
				if (threads == null)
				{
					EnsureState(State.HaveProcessInfo);
					int count = processInfo.threadInfoList.Count;
					ProcessThread[] array = new ProcessThread[count];
					for (int i = 0; i < count; i++)
					{
						array[i] = new ProcessThread(isRemoteMachine, (ThreadInfo)processInfo.threadInfoList[i]);
					}
					ProcessThreadCollection processThreadCollection = (threads = new ProcessThreadCollection(array));
				}
				return threads;
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[MonitoringDescription("ProcessTotalProcessorTime")]
		public TimeSpan TotalProcessorTime
		{
			get
			{
				EnsureState(State.IsNt);
				return GetProcessTimes().TotalProcessorTime;
			}
		}

		[MonitoringDescription("ProcessUserProcessorTime")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public TimeSpan UserProcessorTime
		{
			get
			{
				EnsureState(State.IsNt);
				return GetProcessTimes().UserProcessorTime;
			}
		}

		[Obsolete("This property has been deprecated.  Please use System.Diagnostics.Process.VirtualMemorySize64 instead.  http://go.microsoft.com/fwlink/?linkid=14202")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[MonitoringDescription("ProcessVirtualMemorySize")]
		public int VirtualMemorySize
		{
			get
			{
				EnsureState(State.HaveNtProcessInfo);
				return (int)processInfo.virtualBytes;
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[ComVisible(false)]
		[MonitoringDescription("ProcessVirtualMemorySize")]
		public long VirtualMemorySize64
		{
			get
			{
				EnsureState(State.HaveNtProcessInfo);
				return processInfo.virtualBytes;
			}
		}

		[DefaultValue(false)]
		[MonitoringDescription("ProcessEnableRaisingEvents")]
		[Browsable(false)]
		public bool EnableRaisingEvents
		{
			get
			{
				return watchForExit;
			}
			set
			{
				if (value == watchForExit)
				{
					return;
				}
				if (Associated)
				{
					if (value)
					{
						OpenProcessHandle();
						EnsureWatchingForExit();
					}
					else
					{
						StopWatchingForExit();
					}
				}
				watchForExit = value;
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[MonitoringDescription("ProcessStandardInput")]
		[Browsable(false)]
		public StreamWriter StandardInput
		{
			get
			{
				if (standardInput == null)
				{
					throw new InvalidOperationException(SR.GetString("CantGetStandardIn"));
				}
				return standardInput;
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		[MonitoringDescription("ProcessStandardOutput")]
		public StreamReader StandardOutput
		{
			get
			{
				if (standardOutput == null)
				{
					throw new InvalidOperationException(SR.GetString("CantGetStandardOut"));
				}
				if (outputStreamReadMode == StreamReadMode.undefined)
				{
					outputStreamReadMode = StreamReadMode.syncMode;
				}
				else if (outputStreamReadMode != StreamReadMode.syncMode)
				{
					throw new InvalidOperationException(SR.GetString("CantMixSyncAsyncOperation"));
				}
				return standardOutput;
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[MonitoringDescription("ProcessStandardError")]
		[Browsable(false)]
		public StreamReader StandardError
		{
			get
			{
				if (standardError == null)
				{
					throw new InvalidOperationException(SR.GetString("CantGetStandardError"));
				}
				if (errorStreamReadMode == StreamReadMode.undefined)
				{
					errorStreamReadMode = StreamReadMode.syncMode;
				}
				else if (errorStreamReadMode != StreamReadMode.syncMode)
				{
					throw new InvalidOperationException(SR.GetString("CantMixSyncAsyncOperation"));
				}
				return standardError;
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Obsolete("This property has been deprecated.  Please use System.Diagnostics.Process.WorkingSet64 instead.  http://go.microsoft.com/fwlink/?linkid=14202")]
		[MonitoringDescription("ProcessWorkingSet")]
		public int WorkingSet
		{
			get
			{
				EnsureState(State.HaveNtProcessInfo);
				return (int)processInfo.workingSet;
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[ComVisible(false)]
		[MonitoringDescription("ProcessWorkingSet")]
		public long WorkingSet64
		{
			get
			{
				EnsureState(State.HaveNtProcessInfo);
				return processInfo.workingSet;
			}
		}

		[MonitoringDescription("ProcessAssociated")]
		[Browsable(true)]
		public event DataReceivedEventHandler OutputDataReceived;

		[Browsable(true)]
		[MonitoringDescription("ProcessAssociated")]
		public event DataReceivedEventHandler ErrorDataReceived;

		[Category("Behavior")]
		[MonitoringDescription("ProcessExited")]
		public event EventHandler Exited
		{
			add
			{
				onExited = (EventHandler)Delegate.Combine(onExited, value);
			}
			remove
			{
				onExited = (EventHandler)Delegate.Remove(onExited, value);
			}
		}

		public Process()
		{
			machineName = ".";
			outputStreamReadMode = StreamReadMode.undefined;
			errorStreamReadMode = StreamReadMode.undefined;
		}

		private Process(string machineName, bool isRemoteMachine, int processId, ProcessInfo processInfo)
		{
			this.processInfo = processInfo;
			this.machineName = machineName;
			this.isRemoteMachine = isRemoteMachine;
			this.processId = processId;
			haveProcessId = true;
			outputStreamReadMode = StreamReadMode.undefined;
			errorStreamReadMode = StreamReadMode.undefined;
		}

		private ProcessThreadTimes GetProcessTimes()
		{
			ProcessThreadTimes processThreadTimes = new ProcessThreadTimes();
			Microsoft.Win32.SafeHandles.SafeProcessHandle safeProcessHandle = null;
			try
			{
				safeProcessHandle = GetProcessHandle(1024, throwIfExited: false);
				if (safeProcessHandle.IsInvalid)
				{
					throw new InvalidOperationException(SR.GetString("ProcessHasExited", processId.ToString(CultureInfo.CurrentCulture)));
				}
				if (!NativeMethods.GetProcessTimes(safeProcessHandle, out processThreadTimes.create, out processThreadTimes.exit, out processThreadTimes.kernel, out processThreadTimes.user))
				{
					throw new Win32Exception();
				}
				return processThreadTimes;
			}
			finally
			{
				ReleaseProcessHandle(safeProcessHandle);
			}
		}

		public bool CloseMainWindow()
		{
			IntPtr intPtr = MainWindowHandle;
			if (intPtr == (IntPtr)0)
			{
				return false;
			}
			int windowLong = NativeMethods.GetWindowLong(new HandleRef(this, intPtr), -16);
			if (((uint)windowLong & 0x8000000u) != 0)
			{
				return false;
			}
			NativeMethods.PostMessage(new HandleRef(this, intPtr), 16, IntPtr.Zero, IntPtr.Zero);
			return true;
		}

		private void ReleaseProcessHandle(Microsoft.Win32.SafeHandles.SafeProcessHandle handle)
		{
			if (handle != null && (!haveProcessHandle || handle != m_processHandle))
			{
				handle.Close();
			}
		}

		private void CompletionCallback(object context, bool wasSignaled)
		{
			StopWatchingForExit();
			RaiseOnExited();
		}

		protected override void Dispose(bool disposing)
		{
			if (!disposed)
			{
				if (disposing)
				{
					Close();
				}
				disposed = true;
				base.Dispose(disposing);
			}
		}

		public void Close()
		{
			if (Associated)
			{
				if (haveProcessHandle)
				{
					StopWatchingForExit();
					m_processHandle.Close();
					m_processHandle = null;
					haveProcessHandle = false;
				}
				haveProcessId = false;
				isRemoteMachine = false;
				machineName = ".";
				raisedOnExited = false;
				standardOutput = null;
				standardInput = null;
				standardError = null;
				Refresh();
			}
		}

		private void EnsureState(State state)
		{
			if ((state & State.IsWin2k) != 0 && (OperatingSystem.Platform != PlatformID.Win32NT || OperatingSystem.Version.Major < 5))
			{
				throw new PlatformNotSupportedException(SR.GetString("Win2kRequired"));
			}
			if ((state & State.IsNt) != 0 && OperatingSystem.Platform != PlatformID.Win32NT)
			{
				throw new PlatformNotSupportedException(SR.GetString("WinNTRequired"));
			}
			if ((state & State.Associated) != 0 && !Associated)
			{
				throw new InvalidOperationException(SR.GetString("NoAssociatedProcess"));
			}
			if ((state & State.HaveId) != 0 && !haveProcessId)
			{
				if (!haveProcessHandle)
				{
					EnsureState(State.Associated);
					throw new InvalidOperationException(SR.GetString("ProcessIdRequired"));
				}
				SetProcessId(ProcessManager.GetProcessIdFromHandle(m_processHandle));
			}
			if ((state & State.IsLocal) != 0 && isRemoteMachine)
			{
				throw new NotSupportedException(SR.GetString("NotSupportedRemote"));
			}
			if ((state & State.HaveProcessInfo) != 0 && processInfo == null)
			{
				if ((state & State.HaveId) == 0)
				{
					EnsureState(State.HaveId);
				}
				ProcessInfo[] processInfos = ProcessManager.GetProcessInfos(machineName);
				for (int i = 0; i < processInfos.Length; i++)
				{
					if (processInfos[i].processId == processId)
					{
						processInfo = processInfos[i];
						break;
					}
				}
				if (processInfo == null)
				{
					throw new InvalidOperationException(SR.GetString("NoProcessInfo"));
				}
			}
			if ((state & State.Exited) != 0)
			{
				if (!HasExited)
				{
					throw new InvalidOperationException(SR.GetString("WaitTillExit"));
				}
				if (!haveProcessHandle)
				{
					throw new InvalidOperationException(SR.GetString("NoProcessHandle"));
				}
			}
		}

		private void EnsureWatchingForExit()
		{
			if (watchingForExit)
			{
				return;
			}
			lock (this)
			{
				if (!watchingForExit)
				{
					watchingForExit = true;
					try
					{
						waitHandle = new ProcessWaitHandle(m_processHandle);
						registeredWaitHandle = ThreadPool.RegisterWaitForSingleObject(waitHandle, CompletionCallback, null, -1, executeOnlyOnce: true);
					}
					catch
					{
						watchingForExit = false;
						throw;
					}
				}
			}
		}

		private void EnsureWorkingSetLimits()
		{
			EnsureState(State.IsNt);
			if (haveWorkingSetLimits)
			{
				return;
			}
			Microsoft.Win32.SafeHandles.SafeProcessHandle handle = null;
			try
			{
				handle = GetProcessHandle(1024);
				if (!NativeMethods.GetProcessWorkingSetSize(handle, out var min, out var max))
				{
					throw new Win32Exception();
				}
				minWorkingSet = min;
				maxWorkingSet = max;
				haveWorkingSetLimits = true;
			}
			finally
			{
				ReleaseProcessHandle(handle);
			}
		}

		public static void EnterDebugMode()
		{
			if (ProcessManager.IsNt)
			{
				SetPrivilege("SeDebugPrivilege", 2);
			}
		}

		private static void SetPrivilege(string privilegeName, int attrib)
		{
			IntPtr TokenHandle = (IntPtr)0;
			NativeMethods.LUID lpLuid = default(NativeMethods.LUID);
			IntPtr currentProcess = NativeMethods.GetCurrentProcess();
			if (!NativeMethods.OpenProcessToken(new HandleRef(null, currentProcess), 32, out TokenHandle))
			{
				throw new Win32Exception();
			}
			try
			{
				if (!NativeMethods.LookupPrivilegeValue(null, privilegeName, out lpLuid))
				{
					throw new Win32Exception();
				}
				NativeMethods.TokenPrivileges tokenPrivileges = new NativeMethods.TokenPrivileges();
				tokenPrivileges.Luid = lpLuid;
				tokenPrivileges.Attributes = attrib;
				NativeMethods.AdjustTokenPrivileges(new HandleRef(null, TokenHandle), DisableAllPrivileges: false, tokenPrivileges, 0, IntPtr.Zero, IntPtr.Zero);
				if (Marshal.GetLastWin32Error() != 0)
				{
					throw new Win32Exception();
				}
			}
			finally
			{
				Microsoft.Win32.SafeNativeMethods.CloseHandle(new HandleRef(null, TokenHandle));
			}
		}

		public static void LeaveDebugMode()
		{
			if (ProcessManager.IsNt)
			{
				SetPrivilege("SeDebugPrivilege", 0);
			}
		}

		public static Process GetProcessById(int processId, string machineName)
		{
			if (!ProcessManager.IsProcessRunning(processId, machineName))
			{
				throw new ArgumentException(SR.GetString("MissingProccess", processId.ToString(CultureInfo.CurrentCulture)));
			}
			return new Process(machineName, ProcessManager.IsRemoteMachine(machineName), processId, null);
		}

		public static Process GetProcessById(int processId)
		{
			return GetProcessById(processId, ".");
		}

		public static Process[] GetProcessesByName(string processName)
		{
			return GetProcessesByName(processName, ".");
		}

		public static Process[] GetProcessesByName(string processName, string machineName)
		{
			if (processName == null)
			{
				processName = string.Empty;
			}
			Process[] processes = GetProcesses(machineName);
			ArrayList arrayList = new ArrayList();
			for (int i = 0; i < processes.Length; i++)
			{
				if (string.Equals(processName, processes[i].ProcessName, StringComparison.OrdinalIgnoreCase))
				{
					arrayList.Add(processes[i]);
				}
			}
			Process[] array = new Process[arrayList.Count];
			arrayList.CopyTo(array, 0);
			return array;
		}

		public static Process[] GetProcesses()
		{
			return GetProcesses(".");
		}

		public static Process[] GetProcesses(string machineName)
		{
			bool flag = ProcessManager.IsRemoteMachine(machineName);
			ProcessInfo[] processInfos = ProcessManager.GetProcessInfos(machineName);
			Process[] array = new Process[processInfos.Length];
			for (int i = 0; i < processInfos.Length; i++)
			{
				ProcessInfo processInfo = processInfos[i];
				array[i] = new Process(machineName, flag, processInfo.processId, processInfo);
			}
			return array;
		}

		public static Process GetCurrentProcess()
		{
			return new Process(".", isRemoteMachine: false, NativeMethods.GetCurrentProcessId(), null);
		}

		protected void OnExited()
		{
			EventHandler eventHandler = onExited;
			if (eventHandler != null)
			{
				if (SynchronizingObject != null && SynchronizingObject.InvokeRequired)
				{
					SynchronizingObject.BeginInvoke(eventHandler, new object[2]
					{
						this,
						EventArgs.Empty
					});
				}
				else
				{
					eventHandler(this, EventArgs.Empty);
				}
			}
		}

		private Microsoft.Win32.SafeHandles.SafeProcessHandle GetProcessHandle(int access, bool throwIfExited)
		{
			if (haveProcessHandle)
			{
				if (throwIfExited)
				{
					ProcessWaitHandle processWaitHandle = null;
					try
					{
						processWaitHandle = new ProcessWaitHandle(m_processHandle);
						if (processWaitHandle.WaitOne(0, exitContext: false))
						{
							if (haveProcessId)
							{
								throw new InvalidOperationException(SR.GetString("ProcessHasExited", processId.ToString(CultureInfo.CurrentCulture)));
							}
							throw new InvalidOperationException(SR.GetString("ProcessHasExitedNoId"));
						}
					}
					finally
					{
						processWaitHandle?.Close();
					}
				}
				return m_processHandle;
			}
			EnsureState((State)3);
			Microsoft.Win32.SafeHandles.SafeProcessHandle invalidHandle = Microsoft.Win32.SafeHandles.SafeProcessHandle.InvalidHandle;
			invalidHandle = ProcessManager.OpenProcess(processId, access, throwIfExited);
			if (throwIfExited && ((uint)access & 0x400u) != 0 && NativeMethods.GetExitCodeProcess(invalidHandle, out exitCode) && exitCode != 259)
			{
				throw new InvalidOperationException(SR.GetString("ProcessHasExited", processId.ToString(CultureInfo.CurrentCulture)));
			}
			return invalidHandle;
		}

		private Microsoft.Win32.SafeHandles.SafeProcessHandle GetProcessHandle(int access)
		{
			return GetProcessHandle(access, throwIfExited: true);
		}

		private Microsoft.Win32.SafeHandles.SafeProcessHandle OpenProcessHandle()
		{
			if (!haveProcessHandle)
			{
				if (disposed)
				{
					throw new ObjectDisposedException(GetType().Name);
				}
				SetProcessHandle(GetProcessHandle(2035711));
			}
			return m_processHandle;
		}

		private void RaiseOnExited()
		{
			if (raisedOnExited)
			{
				return;
			}
			lock (this)
			{
				if (!raisedOnExited)
				{
					raisedOnExited = true;
					OnExited();
				}
			}
		}

		public void Refresh()
		{
			processInfo = null;
			threads = null;
			modules = null;
			mainWindowTitle = null;
			exited = false;
			signaled = false;
			haveMainWindow = false;
			haveWorkingSetLimits = false;
			haveProcessorAffinity = false;
			havePriorityClass = false;
			haveExitTime = false;
			haveResponding = false;
			havePriorityBoostEnabled = false;
		}

		private void SetProcessHandle(Microsoft.Win32.SafeHandles.SafeProcessHandle processHandle)
		{
			m_processHandle = processHandle;
			haveProcessHandle = true;
			if (watchForExit)
			{
				EnsureWatchingForExit();
			}
		}

		private void SetProcessId(int processId)
		{
			this.processId = processId;
			haveProcessId = true;
		}

		private void SetWorkingSetLimits(object newMin, object newMax)
		{
			EnsureState(State.IsNt);
			Microsoft.Win32.SafeHandles.SafeProcessHandle handle = null;
			try
			{
				handle = GetProcessHandle(1280);
				if (!NativeMethods.GetProcessWorkingSetSize(handle, out var min, out var max))
				{
					throw new Win32Exception();
				}
				if (newMin != null)
				{
					min = (IntPtr)newMin;
				}
				if (newMax != null)
				{
					max = (IntPtr)newMax;
				}
				if ((long)min > (long)max)
				{
					if (newMin != null)
					{
						throw new ArgumentException(SR.GetString("BadMinWorkset"));
					}
					throw new ArgumentException(SR.GetString("BadMaxWorkset"));
				}
				if (!NativeMethods.SetProcessWorkingSetSize(handle, min, max))
				{
					throw new Win32Exception();
				}
				if (!NativeMethods.GetProcessWorkingSetSize(handle, out min, out max))
				{
					throw new Win32Exception();
				}
				minWorkingSet = min;
				maxWorkingSet = max;
				haveWorkingSetLimits = true;
			}
			finally
			{
				ReleaseProcessHandle(handle);
			}
		}

		public bool Start()
		{
			Close();
			ProcessStartInfo processStartInfo = StartInfo;
			if (processStartInfo.FileName.Length == 0)
			{
				throw new InvalidOperationException(SR.GetString("FileNameMissing"));
			}
			if (processStartInfo.UseShellExecute)
			{
				return StartWithShellExecuteEx(processStartInfo);
			}
			return StartWithCreateProcess(processStartInfo);
		}

		private static void CreatePipeWithSecurityAttributes(out SafeFileHandle hReadPipe, out SafeFileHandle hWritePipe, NativeMethods.SECURITY_ATTRIBUTES lpPipeAttributes, int nSize)
		{
			if (!NativeMethods.CreatePipe(out hReadPipe, out hWritePipe, lpPipeAttributes, nSize) || hReadPipe.IsInvalid || hWritePipe.IsInvalid)
			{
				throw new Win32Exception();
			}
		}

		private void CreatePipe(out SafeFileHandle parentHandle, out SafeFileHandle childHandle, bool parentInputs)
		{
			NativeMethods.SECURITY_ATTRIBUTES sECURITY_ATTRIBUTES = new NativeMethods.SECURITY_ATTRIBUTES();
			sECURITY_ATTRIBUTES.bInheritHandle = true;
			SafeFileHandle hWritePipe = null;
			try
			{
				if (parentInputs)
				{
					CreatePipeWithSecurityAttributes(out childHandle, out hWritePipe, sECURITY_ATTRIBUTES, 0);
				}
				else
				{
					CreatePipeWithSecurityAttributes(out hWritePipe, out childHandle, sECURITY_ATTRIBUTES, 0);
				}
				if (!NativeMethods.DuplicateHandle(new HandleRef(this, NativeMethods.GetCurrentProcess()), (SafeHandle)hWritePipe, new HandleRef(this, NativeMethods.GetCurrentProcess()), out parentHandle, 0, bInheritHandle: false, 2))
				{
					throw new Win32Exception();
				}
			}
			finally
			{
				if (hWritePipe != null && !hWritePipe.IsInvalid)
				{
					hWritePipe.Close();
				}
			}
		}

		private static StringBuilder BuildCommandLine(string executableFileName, string arguments)
		{
			StringBuilder stringBuilder = new StringBuilder();
			string text = executableFileName.Trim();
			bool flag = text.StartsWith("\"", StringComparison.Ordinal) && text.EndsWith("\"", StringComparison.Ordinal);
			if (!flag)
			{
				stringBuilder.Append("\"");
			}
			stringBuilder.Append(text);
			if (!flag)
			{
				stringBuilder.Append("\"");
			}
			if (!string.IsNullOrEmpty(arguments))
			{
				stringBuilder.Append(" ");
				stringBuilder.Append(arguments);
			}
			return stringBuilder;
		}

		private bool StartWithCreateProcess(ProcessStartInfo startInfo)
		{
			if (startInfo.StandardOutputEncoding != null && !startInfo.RedirectStandardOutput)
			{
				throw new InvalidOperationException(SR.GetString("StandardOutputEncodingNotAllowed"));
			}
			if (startInfo.StandardErrorEncoding != null && !startInfo.RedirectStandardError)
			{
				throw new InvalidOperationException(SR.GetString("StandardErrorEncodingNotAllowed"));
			}
			if (disposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			StringBuilder stringBuilder = BuildCommandLine(startInfo.FileName, startInfo.Arguments);
			NativeMethods.STARTUPINFO sTARTUPINFO = new NativeMethods.STARTUPINFO();
			Microsoft.Win32.SafeNativeMethods.PROCESS_INFORMATION pROCESS_INFORMATION = new Microsoft.Win32.SafeNativeMethods.PROCESS_INFORMATION();
			Microsoft.Win32.SafeHandles.SafeProcessHandle safeProcessHandle = new Microsoft.Win32.SafeHandles.SafeProcessHandle();
			Microsoft.Win32.SafeHandles.SafeThreadHandle safeThreadHandle = new Microsoft.Win32.SafeHandles.SafeThreadHandle();
			int num = 0;
			SafeFileHandle parentHandle = null;
			SafeFileHandle parentHandle2 = null;
			SafeFileHandle parentHandle3 = null;
			GCHandle gCHandle = default(GCHandle);
			try
			{
				if (startInfo.RedirectStandardInput || startInfo.RedirectStandardOutput || startInfo.RedirectStandardError)
				{
					if (startInfo.RedirectStandardInput)
					{
						CreatePipe(out parentHandle, out sTARTUPINFO.hStdInput, parentInputs: true);
					}
					else
					{
						sTARTUPINFO.hStdInput = new SafeFileHandle(NativeMethods.GetStdHandle(-10), ownsHandle: false);
					}
					if (startInfo.RedirectStandardOutput)
					{
						CreatePipe(out parentHandle2, out sTARTUPINFO.hStdOutput, parentInputs: false);
					}
					else
					{
						sTARTUPINFO.hStdOutput = new SafeFileHandle(NativeMethods.GetStdHandle(-11), ownsHandle: false);
					}
					if (startInfo.RedirectStandardError)
					{
						CreatePipe(out parentHandle3, out sTARTUPINFO.hStdError, parentInputs: false);
					}
					else
					{
						sTARTUPINFO.hStdError = new SafeFileHandle(NativeMethods.GetStdHandle(-12), ownsHandle: false);
					}
					sTARTUPINFO.dwFlags = 256;
				}
				int num2 = 0;
				if (startInfo.CreateNoWindow)
				{
					num2 |= 0x8000000;
				}
				IntPtr intPtr = (IntPtr)0;
				if (startInfo.environmentVariables != null)
				{
					bool unicode = false;
					if (ProcessManager.IsNt)
					{
						num2 |= 0x400;
						unicode = true;
					}
					byte[] value = EnvironmentBlock.ToByteArray(startInfo.environmentVariables, unicode);
					gCHandle = GCHandle.Alloc(value, GCHandleType.Pinned);
					intPtr = gCHandle.AddrOfPinnedObject();
				}
				string text = startInfo.WorkingDirectory;
				if (text == string.Empty)
				{
					text = Environment.CurrentDirectory;
				}
				if (startInfo.UserName.Length != 0)
				{
					NativeMethods.LogonFlags logonFlags = (NativeMethods.LogonFlags)0;
					if (startInfo.LoadUserProfile)
					{
						logonFlags = NativeMethods.LogonFlags.LOGON_WITH_PROFILE;
					}
					IntPtr intPtr2 = IntPtr.Zero;
					try
					{
						intPtr2 = ((startInfo.Password != null) ? Marshal.SecureStringToCoTaskMemUnicode(startInfo.Password) : Marshal.StringToCoTaskMemUni(string.Empty));
						RuntimeHelpers.PrepareConstrainedRegions();
						bool flag;
						try
						{
						}
						finally
						{
							flag = NativeMethods.CreateProcessWithLogonW(startInfo.UserName, startInfo.Domain, intPtr2, logonFlags, null, stringBuilder, num2, intPtr, text, sTARTUPINFO, pROCESS_INFORMATION);
							if (!flag)
							{
								num = Marshal.GetLastWin32Error();
							}
							if (pROCESS_INFORMATION.hProcess != (IntPtr)0 && pROCESS_INFORMATION.hProcess != NativeMethods.INVALID_HANDLE_VALUE)
							{
								safeProcessHandle.InitialSetHandle(pROCESS_INFORMATION.hProcess);
							}
							if (pROCESS_INFORMATION.hThread != (IntPtr)0 && pROCESS_INFORMATION.hThread != NativeMethods.INVALID_HANDLE_VALUE)
							{
								safeThreadHandle.InitialSetHandle(pROCESS_INFORMATION.hThread);
							}
						}
						if (!flag)
						{
							if (num == 193)
							{
								throw new Win32Exception(num, SR.GetString("InvalidApplication"));
							}
							throw new Win32Exception(num);
						}
					}
					finally
					{
						if (intPtr2 != IntPtr.Zero)
						{
							Marshal.ZeroFreeCoTaskMemUnicode(intPtr2);
						}
					}
				}
				else
				{
					RuntimeHelpers.PrepareConstrainedRegions();
					bool flag;
					try
					{
					}
					finally
					{
						flag = NativeMethods.CreateProcess(null, stringBuilder, null, null, bInheritHandles: true, num2, intPtr, text, sTARTUPINFO, pROCESS_INFORMATION);
						if (!flag)
						{
							num = Marshal.GetLastWin32Error();
						}
						if (pROCESS_INFORMATION.hProcess != (IntPtr)0 && pROCESS_INFORMATION.hProcess != NativeMethods.INVALID_HANDLE_VALUE)
						{
							safeProcessHandle.InitialSetHandle(pROCESS_INFORMATION.hProcess);
						}
						if (pROCESS_INFORMATION.hThread != (IntPtr)0 && pROCESS_INFORMATION.hThread != NativeMethods.INVALID_HANDLE_VALUE)
						{
							safeThreadHandle.InitialSetHandle(pROCESS_INFORMATION.hThread);
						}
					}
					if (!flag)
					{
						if (num == 193)
						{
							throw new Win32Exception(num, SR.GetString("InvalidApplication"));
						}
						throw new Win32Exception(num);
					}
				}
			}
			finally
			{
				if (gCHandle.IsAllocated)
				{
					gCHandle.Free();
				}
				sTARTUPINFO.Dispose();
			}
			if (startInfo.RedirectStandardInput)
			{
				standardInput = new StreamWriter(new FileStream(parentHandle, FileAccess.Write, 4096, isAsync: false), Encoding.GetEncoding(NativeMethods.GetConsoleCP()), 4096);
				standardInput.AutoFlush = true;
			}
			if (startInfo.RedirectStandardOutput)
			{
				Encoding encoding = ((startInfo.StandardOutputEncoding != null) ? startInfo.StandardOutputEncoding : Encoding.GetEncoding(NativeMethods.GetConsoleOutputCP()));
				standardOutput = new StreamReader(new FileStream(parentHandle2, FileAccess.Read, 4096, isAsync: false), encoding, detectEncodingFromByteOrderMarks: true, 4096);
			}
			if (startInfo.RedirectStandardError)
			{
				Encoding encoding2 = ((startInfo.StandardErrorEncoding != null) ? startInfo.StandardErrorEncoding : Encoding.GetEncoding(NativeMethods.GetConsoleOutputCP()));
				standardError = new StreamReader(new FileStream(parentHandle3, FileAccess.Read, 4096, isAsync: false), encoding2, detectEncodingFromByteOrderMarks: true, 4096);
			}
			bool result = false;
			if (!safeProcessHandle.IsInvalid)
			{
				SetProcessHandle(safeProcessHandle);
				SetProcessId(pROCESS_INFORMATION.dwProcessId);
				safeThreadHandle.Close();
				result = true;
			}
			return result;
		}

		private bool StartWithShellExecuteEx(ProcessStartInfo startInfo)
		{
			if (disposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			if (!string.IsNullOrEmpty(startInfo.UserName) || startInfo.Password != null)
			{
				throw new InvalidOperationException(SR.GetString("CantStartAsUser"));
			}
			if (startInfo.RedirectStandardInput || startInfo.RedirectStandardOutput || startInfo.RedirectStandardError)
			{
				throw new InvalidOperationException(SR.GetString("CantRedirectStreams"));
			}
			if (startInfo.StandardErrorEncoding != null)
			{
				throw new InvalidOperationException(SR.GetString("StandardErrorEncodingNotAllowed"));
			}
			if (startInfo.StandardOutputEncoding != null)
			{
				throw new InvalidOperationException(SR.GetString("StandardOutputEncodingNotAllowed"));
			}
			if (startInfo.environmentVariables != null)
			{
				throw new InvalidOperationException(SR.GetString("CantUseEnvVars"));
			}
			NativeMethods.ShellExecuteInfo shellExecuteInfo = new NativeMethods.ShellExecuteInfo();
			shellExecuteInfo.fMask = 64;
			if (startInfo.ErrorDialog)
			{
				shellExecuteInfo.hwnd = startInfo.ErrorDialogParentHandle;
			}
			else
			{
				shellExecuteInfo.fMask |= 1024;
			}
			switch (startInfo.WindowStyle)
			{
			case ProcessWindowStyle.Hidden:
				shellExecuteInfo.nShow = 0;
				break;
			case ProcessWindowStyle.Minimized:
				shellExecuteInfo.nShow = 2;
				break;
			case ProcessWindowStyle.Maximized:
				shellExecuteInfo.nShow = 3;
				break;
			default:
				shellExecuteInfo.nShow = 1;
				break;
			}
			try
			{
				if (startInfo.FileName.Length != 0)
				{
					shellExecuteInfo.lpFile = Marshal.StringToHGlobalAuto(startInfo.FileName);
				}
				if (startInfo.Verb.Length != 0)
				{
					shellExecuteInfo.lpVerb = Marshal.StringToHGlobalAuto(startInfo.Verb);
				}
				if (startInfo.Arguments.Length != 0)
				{
					shellExecuteInfo.lpParameters = Marshal.StringToHGlobalAuto(startInfo.Arguments);
				}
				if (startInfo.WorkingDirectory.Length != 0)
				{
					shellExecuteInfo.lpDirectory = Marshal.StringToHGlobalAuto(startInfo.WorkingDirectory);
				}
				shellExecuteInfo.fMask |= 256;
				ShellExecuteHelper shellExecuteHelper = new ShellExecuteHelper(shellExecuteInfo);
				int num;
				if (!shellExecuteHelper.ShellExecuteOnSTAThread())
				{
					num = shellExecuteHelper.ErrorCode;
					if (num == 0)
					{
						long num2 = (long)shellExecuteInfo.hInstApp;
						if (num2 <= 8)
						{
							if (num2 < 2)
							{
								goto IL_0276;
							}
							switch (num2 - 2)
							{
							case 0L:
								goto IL_0249;
							case 1L:
								goto IL_024d;
							case 3L:
								goto IL_0251;
							case 6L:
								goto IL_0255;
							case 2L:
							case 4L:
							case 5L:
								goto IL_0276;
							}
						}
						if (num2 > 32 || num2 < 26)
						{
							goto IL_0276;
						}
						switch (num2 - 26)
						{
						case 2L:
						case 3L:
						case 4L:
							break;
						case 0L:
							goto IL_0261;
						case 5L:
							goto IL_0266;
						case 6L:
							goto IL_026e;
						default:
							goto IL_0276;
						}
						num = 1156;
					}
					goto IL_0282;
				}
				goto end_IL_0125;
				IL_0266:
				num = 1155;
				goto IL_0282;
				IL_0276:
				num = (int)shellExecuteInfo.hInstApp;
				goto IL_0282;
				IL_0255:
				num = 8;
				goto IL_0282;
				IL_0282:
				throw new Win32Exception(num);
				IL_024d:
				num = 3;
				goto IL_0282;
				IL_0251:
				num = 5;
				goto IL_0282;
				IL_026e:
				num = 1157;
				goto IL_0282;
				IL_0249:
				num = 2;
				goto IL_0282;
				IL_0261:
				num = 32;
				goto IL_0282;
				end_IL_0125:;
			}
			finally
			{
				if (shellExecuteInfo.lpFile != (IntPtr)0)
				{
					Marshal.FreeHGlobal(shellExecuteInfo.lpFile);
				}
				if (shellExecuteInfo.lpVerb != (IntPtr)0)
				{
					Marshal.FreeHGlobal(shellExecuteInfo.lpVerb);
				}
				if (shellExecuteInfo.lpParameters != (IntPtr)0)
				{
					Marshal.FreeHGlobal(shellExecuteInfo.lpParameters);
				}
				if (shellExecuteInfo.lpDirectory != (IntPtr)0)
				{
					Marshal.FreeHGlobal(shellExecuteInfo.lpDirectory);
				}
			}
			if (shellExecuteInfo.hProcess != (IntPtr)0)
			{
				Microsoft.Win32.SafeHandles.SafeProcessHandle processHandle = new Microsoft.Win32.SafeHandles.SafeProcessHandle(shellExecuteInfo.hProcess);
				SetProcessHandle(processHandle);
				return true;
			}
			return false;
		}

		public static Process Start(string fileName, string userName, SecureString password, string domain)
		{
			ProcessStartInfo processStartInfo = new ProcessStartInfo(fileName);
			processStartInfo.UserName = userName;
			processStartInfo.Password = password;
			processStartInfo.Domain = domain;
			processStartInfo.UseShellExecute = false;
			return Start(processStartInfo);
		}

		public static Process Start(string fileName, string arguments, string userName, SecureString password, string domain)
		{
			ProcessStartInfo processStartInfo = new ProcessStartInfo(fileName, arguments);
			processStartInfo.UserName = userName;
			processStartInfo.Password = password;
			processStartInfo.Domain = domain;
			processStartInfo.UseShellExecute = false;
			return Start(processStartInfo);
		}

		public static Process Start(string fileName)
		{
			return Start(new ProcessStartInfo(fileName));
		}

		public static Process Start(string fileName, string arguments)
		{
			return Start(new ProcessStartInfo(fileName, arguments));
		}

		public static Process Start(ProcessStartInfo startInfo)
		{
			Process process = new Process();
			if (startInfo == null)
			{
				throw new ArgumentNullException("startInfo");
			}
			process.StartInfo = startInfo;
			if (process.Start())
			{
				return process;
			}
			return null;
		}

		public void Kill()
		{
			Microsoft.Win32.SafeHandles.SafeProcessHandle safeProcessHandle = null;
			try
			{
				safeProcessHandle = GetProcessHandle(1);
				if (!NativeMethods.TerminateProcess(safeProcessHandle, -1))
				{
					throw new Win32Exception();
				}
			}
			finally
			{
				ReleaseProcessHandle(safeProcessHandle);
			}
		}

		private void StopWatchingForExit()
		{
			if (!watchingForExit)
			{
				return;
			}
			lock (this)
			{
				if (watchingForExit)
				{
					watchingForExit = false;
					registeredWaitHandle.Unregister(null);
					waitHandle.Close();
					waitHandle = null;
					registeredWaitHandle = null;
				}
			}
		}

		public override string ToString()
		{
			if (Associated)
			{
				string text = string.Empty;
				try
				{
					text = ProcessName;
				}
				catch (PlatformNotSupportedException)
				{
				}
				if (text.Length != 0)
				{
					return string.Format(CultureInfo.CurrentCulture, "{0} ({1})", base.ToString(), text);
				}
				return base.ToString();
			}
			return base.ToString();
		}

		public bool WaitForExit(int milliseconds)
		{
			Microsoft.Win32.SafeHandles.SafeProcessHandle safeProcessHandle = null;
			ProcessWaitHandle processWaitHandle = null;
			bool flag;
			try
			{
				safeProcessHandle = GetProcessHandle(1048576, throwIfExited: false);
				if (safeProcessHandle.IsInvalid)
				{
					flag = true;
				}
				else
				{
					processWaitHandle = new ProcessWaitHandle(safeProcessHandle);
					if (processWaitHandle.WaitOne(milliseconds, exitContext: false))
					{
						flag = true;
						signaled = true;
					}
					else
					{
						flag = false;
						signaled = false;
					}
				}
			}
			finally
			{
				processWaitHandle?.Close();
				if (output != null && milliseconds == int.MaxValue)
				{
					output.WaitUtilEOF();
				}
				if (error != null && milliseconds == int.MaxValue)
				{
					error.WaitUtilEOF();
				}
				ReleaseProcessHandle(safeProcessHandle);
			}
			if (flag && watchForExit)
			{
				RaiseOnExited();
			}
			return flag;
		}

		public void WaitForExit()
		{
			WaitForExit(int.MaxValue);
		}

		public bool WaitForInputIdle(int milliseconds)
		{
			Microsoft.Win32.SafeHandles.SafeProcessHandle handle = null;
			try
			{
				handle = GetProcessHandle(1049600);
				return NativeMethods.WaitForInputIdle(handle, milliseconds) switch
				{
					0 => true, 
					258 => false, 
					_ => throw new InvalidOperationException(SR.GetString("InputIdleUnkownError")), 
				};
			}
			finally
			{
				ReleaseProcessHandle(handle);
			}
		}

		public bool WaitForInputIdle()
		{
			return WaitForInputIdle(int.MaxValue);
		}

		[ComVisible(false)]
		public void BeginOutputReadLine()
		{
			if (outputStreamReadMode == StreamReadMode.undefined)
			{
				outputStreamReadMode = StreamReadMode.asyncMode;
			}
			else if (outputStreamReadMode != StreamReadMode.asyncMode)
			{
				throw new InvalidOperationException(SR.GetString("CantMixSyncAsyncOperation"));
			}
			if (pendingOutputRead)
			{
				throw new InvalidOperationException(SR.GetString("PendingAsyncOperation"));
			}
			pendingOutputRead = true;
			if (output == null)
			{
				if (standardOutput == null)
				{
					throw new InvalidOperationException(SR.GetString("CantGetStandardOut"));
				}
				Stream baseStream = standardOutput.BaseStream;
				output = new AsyncStreamReader(this, baseStream, OutputReadNotifyUser, standardOutput.CurrentEncoding);
			}
			output.BeginReadLine();
		}

		[ComVisible(false)]
		public void BeginErrorReadLine()
		{
			if (errorStreamReadMode == StreamReadMode.undefined)
			{
				errorStreamReadMode = StreamReadMode.asyncMode;
			}
			else if (errorStreamReadMode != StreamReadMode.asyncMode)
			{
				throw new InvalidOperationException(SR.GetString("CantMixSyncAsyncOperation"));
			}
			if (pendingErrorRead)
			{
				throw new InvalidOperationException(SR.GetString("PendingAsyncOperation"));
			}
			pendingErrorRead = true;
			if (error == null)
			{
				if (standardError == null)
				{
					throw new InvalidOperationException(SR.GetString("CantGetStandardError"));
				}
				Stream baseStream = standardError.BaseStream;
				error = new AsyncStreamReader(this, baseStream, ErrorReadNotifyUser, standardError.CurrentEncoding);
			}
			error.BeginReadLine();
		}

		[ComVisible(false)]
		public void CancelOutputRead()
		{
			if (output != null)
			{
				output.CancelOperation();
				pendingOutputRead = false;
				return;
			}
			throw new InvalidOperationException(SR.GetString("NoAsyncOperation"));
		}

		[ComVisible(false)]
		public void CancelErrorRead()
		{
			if (error != null)
			{
				error.CancelOperation();
				pendingErrorRead = false;
				return;
			}
			throw new InvalidOperationException(SR.GetString("NoAsyncOperation"));
		}

		internal void OutputReadNotifyUser(string data)
		{
			DataReceivedEventHandler outputDataReceived = this.OutputDataReceived;
			if (outputDataReceived != null)
			{
				DataReceivedEventArgs dataReceivedEventArgs = new DataReceivedEventArgs(data);
				if (SynchronizingObject != null && SynchronizingObject.InvokeRequired)
				{
					SynchronizingObject.Invoke(outputDataReceived, new object[2]
					{
						this,
						dataReceivedEventArgs
					});
				}
				else
				{
					outputDataReceived(this, dataReceivedEventArgs);
				}
			}
		}

		internal void ErrorReadNotifyUser(string data)
		{
			DataReceivedEventHandler errorDataReceived = this.ErrorDataReceived;
			if (errorDataReceived != null)
			{
				DataReceivedEventArgs dataReceivedEventArgs = new DataReceivedEventArgs(data);
				if (SynchronizingObject != null && SynchronizingObject.InvokeRequired)
				{
					SynchronizingObject.Invoke(errorDataReceived, new object[2]
					{
						this,
						dataReceivedEventArgs
					});
				}
				else
				{
					errorDataReceived(this, dataReceivedEventArgs);
				}
			}
		}
	}
}
