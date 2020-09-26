using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
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
	[MonitoringDescription("EventLogDesc")]
	[DefaultEvent("EntryWritten")]
	[InstallerType("System.Diagnostics.EventLogInstaller, System.Configuration.Install, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
	public class EventLog : Component, ISupportInitialize
	{
		private class LogListeningInfo
		{
			public EventLog handleOwner;

			public RegisteredWaitHandle registeredWaitHandle;

			public WaitHandle waitHandle;

			public ArrayList listeningComponents = new ArrayList();
		}

		private class EventLogWaitHandle : WaitHandle
		{
			public EventLogWaitHandle(SafeEventHandle eventLogNativeHandle)
			{
				base.SafeWaitHandle = new SafeWaitHandle(eventLogNativeHandle.DangerousGetHandle(), ownsHandle: true);
				eventLogNativeHandle.SetHandleAsInvalid();
			}
		}

		private const int BUF_SIZE = 40000;

		private const string EventLogKey = "SYSTEM\\CurrentControlSet\\Services\\EventLog";

		internal const string DllName = "EventLogMessages.dll";

		private const string eventLogMutexName = "netfxeventlog.1.0";

		private const int SecondsPerDay = 86400;

		private const int DefaultMaxSize = 524288;

		private const int DefaultRetention = 604800;

		private const int Flag_notifying = 1;

		private const int Flag_forwards = 2;

		private const int Flag_initializing = 4;

		private const int Flag_monitoring = 8;

		private const int Flag_registeredAsListener = 16;

		private const int Flag_writeGranted = 32;

		private const int Flag_disposed = 256;

		private const int Flag_sourceVerified = 512;

		private EventLogEntryCollection entriesCollection;

		private string logName;

		private int lastSeenCount;

		private string machineName;

		private EntryWrittenEventHandler onEntryWrittenHandler;

		private SafeEventLogReadHandle readHandle;

		private string sourceName;

		private SafeEventLogWriteHandle writeHandle;

		private string logDisplayName;

		private int bytesCached;

		private byte[] cache;

		private int firstCachedEntry = -1;

		private int lastSeenEntry;

		private int lastSeenPos;

		private ISynchronizeInvoke synchronizingObject;

		private BitVector32 boolFlags = default(BitVector32);

		private Hashtable messageLibraries;

		private static Hashtable listenerInfos = new Hashtable(StringComparer.OrdinalIgnoreCase);

		private static object s_InternalSyncObject;

		private static bool s_CheckedOsVersion;

		private static bool s_SkipRegPatch;

		private static readonly bool s_dontFilterRegKeys = GetDisableEventLogRegistryKeysFilteringSwitchValue();

		private static object InternalSyncObject
		{
			get
			{
				if (s_InternalSyncObject == null)
				{
					object value = new object();
					Interlocked.CompareExchange(ref s_InternalSyncObject, value, null);
				}
				return s_InternalSyncObject;
			}
		}

		private static bool SkipRegPatch
		{
			get
			{
				if (!s_CheckedOsVersion)
				{
					OperatingSystem oSVersion = Environment.OSVersion;
					s_SkipRegPatch = oSVersion.Platform == PlatformID.Win32NT && oSVersion.Version.Major > 5;
					s_CheckedOsVersion = true;
				}
				return s_SkipRegPatch;
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[MonitoringDescription("LogEntries")]
		public EventLogEntryCollection Entries
		{
			get
			{
				string text = machineName;
				EventLogPermission eventLogPermission = new EventLogPermission(EventLogPermissionAccess.Administer, text);
				eventLogPermission.Demand();
				if (entriesCollection == null)
				{
					entriesCollection = new EventLogEntryCollection(this);
				}
				return entriesCollection;
			}
		}

		internal int EntryCount
		{
			get
			{
				if (!IsOpenForRead)
				{
					OpenForRead(machineName);
				}
				if (!Microsoft.Win32.UnsafeNativeMethods.GetNumberOfEventLogRecords(readHandle, out var count))
				{
					throw SharedUtils.CreateSafeWin32Exception();
				}
				return count;
			}
		}

		private bool IsOpen
		{
			get
			{
				if (readHandle == null)
				{
					return writeHandle != null;
				}
				return true;
			}
		}

		private bool IsOpenForRead => readHandle != null;

		private bool IsOpenForWrite => writeHandle != null;

		[Browsable(false)]
		public string LogDisplayName
		{
			get
			{
				if (logDisplayName == null)
				{
					string text = machineName;
					if (GetLogName(text) != null)
					{
						EventLogPermission eventLogPermission = new EventLogPermission(EventLogPermissionAccess.Administer, text);
						eventLogPermission.Demand();
						SharedUtils.CheckEnvironment();
						PermissionSet permissionSet = _GetAssertPermSet();
						permissionSet.Assert();
						RegistryKey registryKey = null;
						try
						{
							registryKey = GetLogRegKey(text, writable: false);
							if (registryKey == null)
							{
								throw new InvalidOperationException(SR.GetString("MissingLog", GetLogName(text), text));
							}
							string text2 = (string)registryKey.GetValue("DisplayNameFile");
							if (text2 == null)
							{
								logDisplayName = GetLogName(text);
							}
							else
							{
								int messageNum = (int)registryKey.GetValue("DisplayNameID");
								logDisplayName = FormatMessageWrapper(text2, (uint)messageNum, null);
								if (logDisplayName == null)
								{
									logDisplayName = GetLogName(text);
								}
							}
						}
						finally
						{
							registryKey?.Close();
							CodeAccessPermission.RevertAssert();
						}
					}
				}
				return logDisplayName;
			}
		}

		[RecommendedAsConfigurable(true)]
		[ReadOnly(true)]
		[MonitoringDescription("LogLog")]
		[DefaultValue("")]
		[TypeConverter("System.Diagnostics.Design.LogConverter, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
		public string Log
		{
			get
			{
				return GetLogName(machineName);
			}
			set
			{
				SetLogName(machineName, value);
			}
		}

		[RecommendedAsConfigurable(true)]
		[DefaultValue(".")]
		[ReadOnly(true)]
		[MonitoringDescription("LogMachineName")]
		public string MachineName
		{
			get
			{
				string result = machineName;
				EventLogPermission eventLogPermission = new EventLogPermission(EventLogPermissionAccess.Write, result);
				eventLogPermission.Demand();
				return result;
			}
			set
			{
				if (!SyntaxCheck.CheckMachineName(value))
				{
					throw new ArgumentException(SR.GetString("InvalidProperty", "MachineName", value));
				}
				EventLogPermission eventLogPermission = new EventLogPermission(EventLogPermissionAccess.Write, value);
				eventLogPermission.Demand();
				string text = machineName;
				if (text != null)
				{
					if (string.Compare(text, value, StringComparison.OrdinalIgnoreCase) == 0)
					{
						return;
					}
					boolFlags[32] = false;
					if (IsOpen)
					{
						Close(text);
					}
				}
				machineName = value;
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		[ComVisible(false)]
		public long MaximumKilobytes
		{
			get
			{
				string currentMachineName = machineName;
				EventLogPermission eventLogPermission = new EventLogPermission(EventLogPermissionAccess.Administer, currentMachineName);
				eventLogPermission.Demand();
				object logRegValue = GetLogRegValue(currentMachineName, "MaxSize");
				if (logRegValue != null)
				{
					int num = (int)logRegValue;
					return (uint)num / 1024u;
				}
				return 512L;
			}
			set
			{
				string currentMachineName = machineName;
				EventLogPermission eventLogPermission = new EventLogPermission(EventLogPermissionAccess.Administer, currentMachineName);
				eventLogPermission.Demand();
				if (value < 64 || value > 4194240 || value % 64 != 0)
				{
					throw new ArgumentOutOfRangeException("MaximumKilobytes", SR.GetString("MaximumKilobytesOutOfRange"));
				}
				PermissionSet permissionSet = _GetAssertPermSet();
				permissionSet.Assert();
				long num = value * 1024;
				int num2 = (int)num;
				using RegistryKey registryKey = GetLogRegKey(currentMachineName, writable: true);
				registryKey.SetValue("MaxSize", num2, RegistryValueKind.DWord);
			}
		}

		internal Hashtable MessageLibraries
		{
			get
			{
				if (messageLibraries == null)
				{
					messageLibraries = new Hashtable(StringComparer.OrdinalIgnoreCase);
				}
				return messageLibraries;
			}
		}

		[ComVisible(false)]
		[Browsable(false)]
		public OverflowAction OverflowAction
		{
			get
			{
				string currentMachineName = machineName;
				EventLogPermission eventLogPermission = new EventLogPermission(EventLogPermissionAccess.Administer, currentMachineName);
				eventLogPermission.Demand();
				object logRegValue = GetLogRegValue(currentMachineName, "Retention");
				if (logRegValue != null)
				{
					return (int)logRegValue switch
					{
						0 => OverflowAction.OverwriteAsNeeded, 
						-1 => OverflowAction.DoNotOverwrite, 
						_ => OverflowAction.OverwriteOlder, 
					};
				}
				return OverflowAction.OverwriteOlder;
			}
		}

		[ComVisible(false)]
		[Browsable(false)]
		public int MinimumRetentionDays
		{
			get
			{
				string currentMachineName = machineName;
				EventLogPermission eventLogPermission = new EventLogPermission(EventLogPermissionAccess.Administer, currentMachineName);
				eventLogPermission.Demand();
				object logRegValue = GetLogRegValue(currentMachineName, "Retention");
				if (logRegValue != null)
				{
					int num = (int)logRegValue;
					if (num == 0 || num == -1)
					{
						return num;
					}
					return (int)((double)num / 86400.0);
				}
				return 7;
			}
		}

		[Browsable(false)]
		[MonitoringDescription("LogMonitoring")]
		[DefaultValue(false)]
		public bool EnableRaisingEvents
		{
			get
			{
				string text = machineName;
				EventLogPermission eventLogPermission = new EventLogPermission(EventLogPermissionAccess.Write, text);
				eventLogPermission.Demand();
				return boolFlags[8];
			}
			set
			{
				string currentMachineName = machineName;
				EventLogPermission eventLogPermission = new EventLogPermission(EventLogPermissionAccess.Write, currentMachineName);
				eventLogPermission.Demand();
				if (base.DesignMode)
				{
					boolFlags[8] = value;
				}
				else if (value)
				{
					StartRaisingEvents(currentMachineName, GetLogName(currentMachineName));
				}
				else
				{
					StopRaisingEvents(GetLogName(currentMachineName));
				}
			}
		}

		private int OldestEntryNumber
		{
			get
			{
				if (!IsOpenForRead)
				{
					OpenForRead(machineName);
				}
				int[] array = new int[1];
				if (!Microsoft.Win32.UnsafeNativeMethods.GetOldestEventLogRecord(readHandle, array))
				{
					throw SharedUtils.CreateSafeWin32Exception();
				}
				int num = array[0];
				if (num == 0)
				{
					num = 1;
				}
				return num;
			}
		}

		internal SafeEventLogReadHandle ReadHandle
		{
			get
			{
				if (!IsOpenForRead)
				{
					OpenForRead(machineName);
				}
				return readHandle;
			}
		}

		[Browsable(false)]
		[MonitoringDescription("LogSynchronizingObject")]
		[DefaultValue(null)]
		public ISynchronizeInvoke SynchronizingObject
		{
			[HostProtection(SecurityAction.LinkDemand, Synchronization = true)]
			get
			{
				string text = machineName;
				EventLogPermission eventLogPermission = new EventLogPermission(EventLogPermissionAccess.Write, text);
				eventLogPermission.Demand();
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

		[RecommendedAsConfigurable(true)]
		[ReadOnly(true)]
		[TypeConverter("System.Diagnostics.Design.StringValueConverter, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
		[MonitoringDescription("LogSource")]
		[DefaultValue("")]
		public string Source
		{
			get
			{
				string text = machineName;
				EventLogPermission eventLogPermission = new EventLogPermission(EventLogPermissionAccess.Write, text);
				eventLogPermission.Demand();
				return sourceName;
			}
			set
			{
				if (value == null)
				{
					value = string.Empty;
				}
				if (value.Length + "SYSTEM\\CurrentControlSet\\Services\\EventLog".Length > 254)
				{
					throw new ArgumentException(SR.GetString("ParameterTooLong", "source", 254 - "SYSTEM\\CurrentControlSet\\Services\\EventLog".Length));
				}
				string currentMachineName = machineName;
				EventLogPermission eventLogPermission = new EventLogPermission(EventLogPermissionAccess.Write, currentMachineName);
				eventLogPermission.Demand();
				if (sourceName == null)
				{
					sourceName = value;
				}
				else if (string.Compare(sourceName, value, StringComparison.OrdinalIgnoreCase) != 0)
				{
					sourceName = value;
					if (IsOpen)
					{
						bool enableRaisingEvents = EnableRaisingEvents;
						Close(currentMachineName);
						EnableRaisingEvents = enableRaisingEvents;
					}
				}
			}
		}

		[MonitoringDescription("LogEntryWritten")]
		public event EntryWrittenEventHandler EntryWritten
		{
			add
			{
				string text = machineName;
				EventLogPermission eventLogPermission = new EventLogPermission(EventLogPermissionAccess.Administer, text);
				eventLogPermission.Demand();
				onEntryWrittenHandler = (EntryWrittenEventHandler)Delegate.Combine(onEntryWrittenHandler, value);
			}
			remove
			{
				string text = machineName;
				EventLogPermission eventLogPermission = new EventLogPermission(EventLogPermissionAccess.Administer, text);
				eventLogPermission.Demand();
				onEntryWrittenHandler = (EntryWrittenEventHandler)Delegate.Remove(onEntryWrittenHandler, value);
			}
		}

		public EventLog()
			: this("", ".", "")
		{
		}

		public EventLog(string logName)
			: this(logName, ".", "")
		{
		}

		public EventLog(string logName, string machineName)
			: this(logName, machineName, "")
		{
		}

		public EventLog(string logName, string machineName, string source)
		{
			if (logName == null)
			{
				throw new ArgumentNullException("logName");
			}
			if (!ValidLogName(logName, ignoreEmpty: true))
			{
				throw new ArgumentException(SR.GetString("BadLogName"));
			}
			if (!SyntaxCheck.CheckMachineName(machineName))
			{
				throw new ArgumentException(SR.GetString("InvalidParameter", "machineName", machineName));
			}
			EventLogPermission eventLogPermission = new EventLogPermission(EventLogPermissionAccess.Write, machineName);
			eventLogPermission.Demand();
			this.machineName = machineName;
			this.logName = logName;
			sourceName = source;
			readHandle = null;
			writeHandle = null;
			boolFlags[2] = true;
		}

		private static PermissionSet _GetAssertPermSet()
		{
			PermissionSet permissionSet = new PermissionSet(PermissionState.None);
			RegistryPermission perm = new RegistryPermission(PermissionState.Unrestricted);
			permissionSet.AddPermission(perm);
			EnvironmentPermission perm2 = new EnvironmentPermission(PermissionState.Unrestricted);
			permissionSet.AddPermission(perm2);
			return permissionSet;
		}

		private string GetLogName(string currentMachineName)
		{
			if ((logName == null || logName.Length == 0) && sourceName != null && sourceName.Length != 0)
			{
				logName = LogNameFromSourceName(sourceName, currentMachineName);
			}
			return logName;
		}

		private void SetLogName(string currentMachineName, string value)
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			if (!ValidLogName(value, ignoreEmpty: true))
			{
				throw new ArgumentException(SR.GetString("BadLogName"));
			}
			EventLogPermission eventLogPermission = new EventLogPermission(EventLogPermissionAccess.Write, currentMachineName);
			eventLogPermission.Demand();
			if (value == null)
			{
				value = string.Empty;
			}
			if (logName == null)
			{
				logName = value;
			}
			else if (string.Compare(logName, value, StringComparison.OrdinalIgnoreCase) != 0)
			{
				logDisplayName = null;
				logName = value;
				if (IsOpen)
				{
					bool enableRaisingEvents = EnableRaisingEvents;
					Close(currentMachineName);
					EnableRaisingEvents = enableRaisingEvents;
				}
			}
		}

		[HostProtection(SecurityAction.LinkDemand, Synchronization = true)]
		private static void AddListenerComponent(EventLog component, string compMachineName, string compLogName)
		{
			lock (InternalSyncObject)
			{
				LogListeningInfo logListeningInfo = (LogListeningInfo)listenerInfos[compLogName];
				if (logListeningInfo != null)
				{
					logListeningInfo.listeningComponents.Add(component);
					return;
				}
				logListeningInfo = new LogListeningInfo();
				logListeningInfo.listeningComponents.Add(component);
				logListeningInfo.handleOwner = new EventLog();
				logListeningInfo.handleOwner.MachineName = compMachineName;
				logListeningInfo.handleOwner.Log = compLogName;
				SafeEventHandle safeEventHandle = SafeEventHandle.CreateEvent(NativeMethods.NullHandleRef, bManualReset: false, bInitialState: false, null);
				if (safeEventHandle.IsInvalid)
				{
					Win32Exception innerException = null;
					if (Marshal.GetLastWin32Error() != 0)
					{
						innerException = SharedUtils.CreateSafeWin32Exception();
					}
					throw new InvalidOperationException(SR.GetString("NotifyCreateFailed"), innerException);
				}
				if (!Microsoft.Win32.UnsafeNativeMethods.NotifyChangeEventLog(logListeningInfo.handleOwner.ReadHandle, safeEventHandle))
				{
					throw new InvalidOperationException(SR.GetString("CantMonitorEventLog"), SharedUtils.CreateSafeWin32Exception());
				}
				logListeningInfo.waitHandle = new EventLogWaitHandle(safeEventHandle);
				logListeningInfo.registeredWaitHandle = ThreadPool.RegisterWaitForSingleObject(logListeningInfo.waitHandle, StaticCompletionCallback, logListeningInfo, -1, executeOnlyOnce: false);
				listenerInfos[compLogName] = logListeningInfo;
			}
		}

		public void BeginInit()
		{
			string currentMachineName = machineName;
			EventLogPermission eventLogPermission = new EventLogPermission(EventLogPermissionAccess.Write, currentMachineName);
			eventLogPermission.Demand();
			if (boolFlags[4])
			{
				throw new InvalidOperationException(SR.GetString("InitTwice"));
			}
			boolFlags[4] = true;
			if (boolFlags[8])
			{
				StopListening(GetLogName(currentMachineName));
			}
		}

		public void Clear()
		{
			string currentMachineName = machineName;
			EventLogPermission eventLogPermission = new EventLogPermission(EventLogPermissionAccess.Administer, currentMachineName);
			eventLogPermission.Demand();
			if (!IsOpenForRead)
			{
				OpenForRead(currentMachineName);
			}
			if (!Microsoft.Win32.UnsafeNativeMethods.ClearEventLog(readHandle, NativeMethods.NullHandleRef))
			{
				int lastWin32Error = Marshal.GetLastWin32Error();
				if (lastWin32Error != 2)
				{
					throw SharedUtils.CreateSafeWin32Exception();
				}
			}
			Reset(currentMachineName);
		}

		public void Close()
		{
			Close(machineName);
		}

		private void Close(string currentMachineName)
		{
			EventLogPermission eventLogPermission = new EventLogPermission(EventLogPermissionAccess.Write, currentMachineName);
			eventLogPermission.Demand();
			if (readHandle != null)
			{
				try
				{
					readHandle.Close();
				}
				catch (IOException)
				{
					throw SharedUtils.CreateSafeWin32Exception();
				}
				readHandle = null;
			}
			if (writeHandle != null)
			{
				try
				{
					writeHandle.Close();
				}
				catch (IOException)
				{
					throw SharedUtils.CreateSafeWin32Exception();
				}
				writeHandle = null;
			}
			if (boolFlags[8])
			{
				StopRaisingEvents(GetLogName(currentMachineName));
			}
			if (messageLibraries != null)
			{
				foreach (SafeLibraryHandle value in messageLibraries.Values)
				{
					value.Close();
				}
				messageLibraries = null;
			}
			boolFlags[512] = false;
		}

		private void CompletionCallback(object context)
		{
			if (boolFlags[256])
			{
				return;
			}
			lock (this)
			{
				if (boolFlags[1])
				{
					return;
				}
				boolFlags[1] = true;
			}
			int i = lastSeenCount;
			try
			{
				int oldestEntryNumber = OldestEntryNumber;
				int num = EntryCount + oldestEntryNumber;
				while (i < num)
				{
					for (; i < num; i++)
					{
						EventLogEntry entryWithOldest = GetEntryWithOldest(i);
						if (SynchronizingObject != null && SynchronizingObject.InvokeRequired)
						{
							SynchronizingObject.BeginInvoke(onEntryWrittenHandler, new object[2]
							{
								this,
								new EntryWrittenEventArgs(entryWithOldest)
							});
						}
						else
						{
							onEntryWrittenHandler(this, new EntryWrittenEventArgs(entryWithOldest));
						}
					}
					oldestEntryNumber = OldestEntryNumber;
					num = EntryCount + oldestEntryNumber;
				}
			}
			catch (Exception)
			{
			}
			catch
			{
			}
			try
			{
				int num2 = EntryCount + OldestEntryNumber;
				if (i > num2)
				{
					lastSeenCount = num2;
				}
				else
				{
					lastSeenCount = i;
				}
			}
			catch (Win32Exception)
			{
			}
			lock (this)
			{
				boolFlags[1] = false;
			}
		}

		public static void CreateEventSource(string source, string logName)
		{
			CreateEventSource(new EventSourceCreationData(source, logName, "."));
		}

		[Obsolete("This method has been deprecated.  Please use System.Diagnostics.EventLog.CreateEventSource(EventSourceCreationData sourceData) instead.  http://go.microsoft.com/fwlink/?linkid=14202")]
		public static void CreateEventSource(string source, string logName, string machineName)
		{
			CreateEventSource(new EventSourceCreationData(source, logName, machineName));
		}

		public static void CreateEventSource(EventSourceCreationData sourceData)
		{
			if (sourceData == null)
			{
				throw new ArgumentNullException("sourceData");
			}
			string text = sourceData.LogName;
			string source = sourceData.Source;
			string text2 = sourceData.MachineName;
			if (!SyntaxCheck.CheckMachineName(text2))
			{
				throw new ArgumentException(SR.GetString("InvalidParameter", "machineName", text2));
			}
			if (text == null || text.Length == 0)
			{
				text = "Application";
			}
			if (!ValidLogName(text, ignoreEmpty: false))
			{
				throw new ArgumentException(SR.GetString("BadLogName"));
			}
			if (source == null || source.Length == 0)
			{
				throw new ArgumentException(SR.GetString("MissingParameter", "source"));
			}
			if (source.Length + "SYSTEM\\CurrentControlSet\\Services\\EventLog".Length > 254)
			{
				throw new ArgumentException(SR.GetString("ParameterTooLong", "source", 254 - "SYSTEM\\CurrentControlSet\\Services\\EventLog".Length));
			}
			EventLogPermission eventLogPermission = new EventLogPermission(EventLogPermissionAccess.Administer, text2);
			eventLogPermission.Demand();
			Mutex mutex = null;
			RuntimeHelpers.PrepareConstrainedRegions();
			try
			{
				SharedUtils.EnterMutex("netfxeventlog.1.0", ref mutex);
				if (SourceExists(source, text2))
				{
					if (".".Equals(text2))
					{
						throw new ArgumentException(SR.GetString("LocalSourceAlreadyExists", source));
					}
					throw new ArgumentException(SR.GetString("SourceAlreadyExists", source, text2));
				}
				PermissionSet permissionSet = _GetAssertPermSet();
				permissionSet.Assert();
				RegistryKey registryKey = null;
				RegistryKey registryKey2 = null;
				RegistryKey registryKey3 = null;
				RegistryKey registryKey4 = null;
				RegistryKey registryKey5 = null;
				try
				{
					registryKey = ((!(text2 == ".")) ? RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine, text2) : Registry.LocalMachine);
					registryKey2 = registryKey.OpenSubKey("SYSTEM\\CurrentControlSet\\Services\\EventLog", writable: true);
					if (registryKey2 == null)
					{
						if (!".".Equals(text2))
						{
							throw new InvalidOperationException(SR.GetString("RegKeyMissing", "SYSTEM\\CurrentControlSet\\Services\\EventLog", text, source, text2));
						}
						throw new InvalidOperationException(SR.GetString("LocalRegKeyMissing", "SYSTEM\\CurrentControlSet\\Services\\EventLog", text, source));
					}
					registryKey3 = registryKey2.OpenSubKey(text, writable: true);
					if (registryKey3 == null && text.Length >= 8)
					{
						string strA = text.Substring(0, 8);
						if (string.Compare(strA, "AppEvent", StringComparison.OrdinalIgnoreCase) == 0 || string.Compare(strA, "SecEvent", StringComparison.OrdinalIgnoreCase) == 0 || string.Compare(strA, "SysEvent", StringComparison.OrdinalIgnoreCase) == 0)
						{
							throw new ArgumentException(SR.GetString("InvalidCustomerLogName", text));
						}
						string text3 = FindSame8FirstCharsLog(registryKey2, text);
						if (text3 != null)
						{
							throw new ArgumentException(SR.GetString("DuplicateLogName", text, text3));
						}
					}
					bool flag = registryKey3 == null;
					if (flag)
					{
						if (SourceExists(text, text2))
						{
							if (".".Equals(text2))
							{
								throw new ArgumentException(SR.GetString("LocalLogAlreadyExistsAsSource", text));
							}
							throw new ArgumentException(SR.GetString("LogAlreadyExistsAsSource", text, text2));
						}
						registryKey3 = registryKey2.CreateSubKey(text);
						if (!SkipRegPatch)
						{
							registryKey3.SetValue("Sources", new string[2]
							{
								text,
								source
							}, RegistryValueKind.MultiString);
						}
						SetSpecialLogRegValues(registryKey3, text);
						registryKey4 = registryKey3.CreateSubKey(text);
						SetSpecialSourceRegValues(registryKey4, sourceData);
					}
					if (!(text != source))
					{
						return;
					}
					if (!flag)
					{
						SetSpecialLogRegValues(registryKey3, text);
						if (!SkipRegPatch)
						{
							string[] array = registryKey3.GetValue("Sources") as string[];
							if (array == null)
							{
								registryKey3.SetValue("Sources", new string[2]
								{
									text,
									source
								}, RegistryValueKind.MultiString);
							}
							else if (Array.IndexOf(array, source) == -1)
							{
								string[] array2 = new string[array.Length + 1];
								Array.Copy(array, array2, array.Length);
								array2[array.Length] = source;
								registryKey3.SetValue("Sources", array2, RegistryValueKind.MultiString);
							}
						}
					}
					registryKey5 = registryKey3.CreateSubKey(source);
					SetSpecialSourceRegValues(registryKey5, sourceData);
				}
				finally
				{
					registryKey?.Close();
					registryKey2?.Close();
					if (registryKey3 != null)
					{
						registryKey3.Flush();
						registryKey3.Close();
					}
					if (registryKey4 != null)
					{
						registryKey4.Flush();
						registryKey4.Close();
					}
					if (registryKey5 != null)
					{
						registryKey5.Flush();
						registryKey5.Close();
					}
					CodeAccessPermission.RevertAssert();
				}
			}
			finally
			{
				if (mutex != null)
				{
					mutex.ReleaseMutex();
					mutex.Close();
				}
			}
		}

		public static void Delete(string logName)
		{
			Delete(logName, ".");
		}

		public static void Delete(string logName, string machineName)
		{
			if (!SyntaxCheck.CheckMachineName(machineName))
			{
				throw new ArgumentException(SR.GetString("InvalidParameterFormat", "machineName"));
			}
			if (logName == null || logName.Length == 0)
			{
				throw new ArgumentException(SR.GetString("NoLogName"));
			}
			if (!ValidLogName(logName, ignoreEmpty: false))
			{
				throw new InvalidOperationException(SR.GetString("BadLogName"));
			}
			EventLogPermission eventLogPermission = new EventLogPermission(EventLogPermissionAccess.Administer, machineName);
			eventLogPermission.Demand();
			SharedUtils.CheckEnvironment();
			PermissionSet permissionSet = _GetAssertPermSet();
			permissionSet.Assert();
			RegistryKey registryKey = null;
			Mutex mutex = null;
			RuntimeHelpers.PrepareConstrainedRegions();
			try
			{
				SharedUtils.EnterMutex("netfxeventlog.1.0", ref mutex);
				try
				{
					registryKey = GetEventLogRegKey(machineName, writable: true);
					if (registryKey == null)
					{
						throw new InvalidOperationException(SR.GetString("RegKeyNoAccess", "HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Services\\EventLog", machineName));
					}
					using (RegistryKey registryKey2 = registryKey.OpenSubKey(logName))
					{
						if (registryKey2 == null)
						{
							throw new InvalidOperationException(SR.GetString("MissingLog", logName, machineName));
						}
						EventLog eventLog = new EventLog();
						try
						{
							eventLog.Log = logName;
							eventLog.MachineName = machineName;
							eventLog.Clear();
						}
						finally
						{
							eventLog.Close();
						}
						string text = null;
						try
						{
							text = (string)registryKey2.GetValue("File");
						}
						catch
						{
						}
						if (text != null)
						{
							try
							{
								File.Delete(text);
							}
							catch
							{
							}
						}
					}
					registryKey.DeleteSubKeyTree(logName);
				}
				finally
				{
					registryKey?.Close();
					CodeAccessPermission.RevertAssert();
				}
			}
			finally
			{
				mutex?.ReleaseMutex();
			}
		}

		public static void DeleteEventSource(string source)
		{
			DeleteEventSource(source, ".");
		}

		public static void DeleteEventSource(string source, string machineName)
		{
			if (!SyntaxCheck.CheckMachineName(machineName))
			{
				throw new ArgumentException(SR.GetString("InvalidParameter", "machineName", machineName));
			}
			EventLogPermission eventLogPermission = new EventLogPermission(EventLogPermissionAccess.Administer, machineName);
			eventLogPermission.Demand();
			SharedUtils.CheckEnvironment();
			PermissionSet permissionSet = _GetAssertPermSet();
			permissionSet.Assert();
			Mutex mutex = null;
			RuntimeHelpers.PrepareConstrainedRegions();
			try
			{
				SharedUtils.EnterMutex("netfxeventlog.1.0", ref mutex);
				RegistryKey registryKey = null;
				using (registryKey = FindSourceRegistration(source, machineName, readOnly: true))
				{
					if (registryKey == null)
					{
						if (machineName == null)
						{
							throw new ArgumentException(SR.GetString("LocalSourceNotRegistered", source));
						}
						throw new ArgumentException(SR.GetString("SourceNotRegistered", source, machineName, "HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Services\\EventLog"));
					}
					string name = registryKey.Name;
					int num = name.LastIndexOf('\\');
					if (string.Compare(name, num + 1, source, 0, name.Length - num, StringComparison.Ordinal) == 0)
					{
						throw new InvalidOperationException(SR.GetString("CannotDeleteEqualSource", source));
					}
				}
				try
				{
					registryKey = FindSourceRegistration(source, machineName, readOnly: false);
					registryKey.DeleteSubKeyTree(source);
					if (SkipRegPatch)
					{
						return;
					}
					string[] array = (string[])registryKey.GetValue("Sources");
					ArrayList arrayList = new ArrayList(array.Length - 1);
					for (int i = 0; i < array.Length; i++)
					{
						if (array[i] != source)
						{
							arrayList.Add(array[i]);
						}
					}
					string[] array2 = new string[arrayList.Count];
					arrayList.CopyTo(array2);
					registryKey.SetValue("Sources", array2, RegistryValueKind.MultiString);
				}
				finally
				{
					if (registryKey != null)
					{
						registryKey.Flush();
						registryKey.Close();
					}
					CodeAccessPermission.RevertAssert();
				}
			}
			finally
			{
				mutex?.ReleaseMutex();
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (IsOpen)
				{
					Close();
				}
			}
			else
			{
				if (readHandle != null)
				{
					readHandle.Close();
				}
				if (writeHandle != null)
				{
					writeHandle.Close();
				}
				messageLibraries = null;
			}
			boolFlags[256] = true;
			base.Dispose(disposing);
		}

		public void EndInit()
		{
			string currentMachineName = machineName;
			EventLogPermission eventLogPermission = new EventLogPermission(EventLogPermissionAccess.Write, currentMachineName);
			eventLogPermission.Demand();
			boolFlags[4] = false;
			if (boolFlags[8])
			{
				StartListening(currentMachineName, GetLogName(currentMachineName));
			}
		}

		public static bool Exists(string logName)
		{
			return Exists(logName, ".");
		}

		public static bool Exists(string logName, string machineName)
		{
			if (!SyntaxCheck.CheckMachineName(machineName))
			{
				throw new ArgumentException(SR.GetString("InvalidParameterFormat", "machineName"));
			}
			EventLogPermission eventLogPermission = new EventLogPermission(EventLogPermissionAccess.Administer, machineName);
			eventLogPermission.Demand();
			if (logName == null || logName.Length == 0)
			{
				return false;
			}
			SharedUtils.CheckEnvironment();
			PermissionSet permissionSet = _GetAssertPermSet();
			permissionSet.Assert();
			RegistryKey registryKey = null;
			RegistryKey registryKey2 = null;
			try
			{
				registryKey = GetEventLogRegKey(machineName, writable: false);
				if (registryKey == null)
				{
					return false;
				}
				registryKey2 = registryKey.OpenSubKey(logName, writable: false);
				return registryKey2 != null;
			}
			finally
			{
				registryKey?.Close();
				registryKey2?.Close();
				CodeAccessPermission.RevertAssert();
			}
		}

		private static string FindSame8FirstCharsLog(RegistryKey keyParent, string logName)
		{
			string strB = logName.Substring(0, 8);
			string[] subKeyNames = keyParent.GetSubKeyNames();
			foreach (string text in subKeyNames)
			{
				if (text.Length >= 8 && string.Compare(text.Substring(0, 8), strB, StringComparison.OrdinalIgnoreCase) == 0)
				{
					return text;
				}
			}
			return null;
		}

		private static RegistryKey FindSourceRegistration(string source, string machineName, bool readOnly)
		{
			if (source != null && source.Length != 0)
			{
				SharedUtils.CheckEnvironment();
				PermissionSet permissionSet = _GetAssertPermSet();
				permissionSet.Assert();
				RegistryKey registryKey = null;
				try
				{
					registryKey = GetEventLogRegKey(machineName, !readOnly);
					if (registryKey == null)
					{
						return null;
					}
					StringBuilder stringBuilder = null;
					string[] subKeyNames = registryKey.GetSubKeyNames();
					for (int i = 0; i < subKeyNames.Length; i++)
					{
						RegistryKey registryKey2 = null;
						try
						{
							RegistryKey registryKey3 = registryKey.OpenSubKey(subKeyNames[i], !readOnly);
							if (registryKey3 != null)
							{
								registryKey2 = registryKey3.OpenSubKey(source, !readOnly);
								if (registryKey2 != null)
								{
									return registryKey3;
								}
							}
						}
						catch (UnauthorizedAccessException)
						{
							if (stringBuilder == null)
							{
								stringBuilder = new StringBuilder(subKeyNames[i]);
								continue;
							}
							stringBuilder.Append(", ");
							stringBuilder.Append(subKeyNames[i]);
						}
						catch (SecurityException)
						{
							if (stringBuilder == null)
							{
								stringBuilder = new StringBuilder(subKeyNames[i]);
								continue;
							}
							stringBuilder.Append(", ");
							stringBuilder.Append(subKeyNames[i]);
						}
						finally
						{
							registryKey2?.Close();
						}
					}
					if (stringBuilder != null)
					{
						throw new SecurityException(SR.GetString("SomeLogsInaccessible", stringBuilder.ToString()));
					}
				}
				finally
				{
					registryKey?.Close();
					CodeAccessPermission.RevertAssert();
				}
			}
			return null;
		}

		internal string FormatMessageWrapper(string dllNameList, uint messageNum, string[] insertionStrings)
		{
			if (dllNameList == null)
			{
				return null;
			}
			if (insertionStrings == null)
			{
				insertionStrings = new string[0];
			}
			string[] array = dllNameList.Split(';');
			string[] array2 = array;
			foreach (string text in array2)
			{
				if (text == null || text.Length == 0)
				{
					continue;
				}
				SafeLibraryHandle safeLibraryHandle = null;
				if (IsOpen)
				{
					safeLibraryHandle = MessageLibraries[text] as SafeLibraryHandle;
					if (safeLibraryHandle == null || safeLibraryHandle.IsInvalid)
					{
						safeLibraryHandle = SafeLibraryHandle.LoadLibraryEx(text, IntPtr.Zero, 2);
						MessageLibraries[text] = safeLibraryHandle;
					}
				}
				else
				{
					safeLibraryHandle = SafeLibraryHandle.LoadLibraryEx(text, IntPtr.Zero, 2);
				}
				if (safeLibraryHandle.IsInvalid)
				{
					continue;
				}
				string text2 = null;
				try
				{
					text2 = TryFormatMessage(safeLibraryHandle, messageNum, insertionStrings);
				}
				finally
				{
					if (!IsOpen)
					{
						safeLibraryHandle.Close();
					}
				}
				if (text2 != null)
				{
					return text2;
				}
			}
			return null;
		}

		internal EventLogEntry[] GetAllEntries()
		{
			string currentMachineName = machineName;
			if (!IsOpenForRead)
			{
				OpenForRead(currentMachineName);
			}
			EventLogEntry[] array = new EventLogEntry[EntryCount];
			int num = 0;
			int oldestEntryNumber = OldestEntryNumber;
			int[] array2 = new int[1];
			int[] array3 = new int[1]
			{
				40000
			};
			int num2 = 0;
			while (num < array.Length)
			{
				byte[] array4 = new byte[40000];
				if (!Microsoft.Win32.UnsafeNativeMethods.ReadEventLog(readHandle, 6, oldestEntryNumber + num, array4, array4.Length, array2, array3))
				{
					num2 = Marshal.GetLastWin32Error();
					if (num2 != 122 && num2 != 1503)
					{
						break;
					}
					if (num2 == 1503)
					{
						Reset(currentMachineName);
					}
					else if (array3[0] > array4.Length)
					{
						array4 = new byte[array3[0]];
					}
					if (!Microsoft.Win32.UnsafeNativeMethods.ReadEventLog(readHandle, 6, oldestEntryNumber + num, array4, array4.Length, array2, array3))
					{
						break;
					}
					num2 = 0;
				}
				array[num] = new EventLogEntry(array4, 0, this);
				int num3 = IntFrom(array4, 0);
				num++;
				while (num3 < array2[0] && num < array.Length)
				{
					array[num] = new EventLogEntry(array4, num3, this);
					num3 += IntFrom(array4, num3);
					num++;
				}
			}
			if (num != array.Length)
			{
				if (num2 != 0)
				{
					throw new InvalidOperationException(SR.GetString("CantRetrieveEntries"), SharedUtils.CreateSafeWin32Exception(num2));
				}
				throw new InvalidOperationException(SR.GetString("CantRetrieveEntries"));
			}
			return array;
		}

		public static EventLog[] GetEventLogs()
		{
			return GetEventLogs(".");
		}

		public static EventLog[] GetEventLogs(string machineName)
		{
			if (!SyntaxCheck.CheckMachineName(machineName))
			{
				throw new ArgumentException(SR.GetString("InvalidParameter", "machineName", machineName));
			}
			EventLogPermission eventLogPermission = new EventLogPermission(EventLogPermissionAccess.Administer, machineName);
			eventLogPermission.Demand();
			SharedUtils.CheckEnvironment();
			string[] array = new string[0];
			PermissionSet permissionSet = _GetAssertPermSet();
			permissionSet.Assert();
			RegistryKey registryKey = null;
			try
			{
				registryKey = GetEventLogRegKey(machineName, writable: false);
				if (registryKey == null)
				{
					throw new InvalidOperationException(SR.GetString("RegKeyMissingShort", "SYSTEM\\CurrentControlSet\\Services\\EventLog", machineName));
				}
				array = registryKey.GetSubKeyNames();
			}
			finally
			{
				registryKey?.Close();
				CodeAccessPermission.RevertAssert();
			}
			if (s_dontFilterRegKeys || machineName != ".")
			{
				EventLog[] array2 = new EventLog[array.Length];
				for (int i = 0; i < array.Length; i++)
				{
					EventLog eventLog = new EventLog();
					eventLog.Log = array[i];
					eventLog.MachineName = machineName;
					array2[i] = eventLog;
				}
				return array2;
			}
			List<EventLog> list = new List<EventLog>(array.Length);
			for (int j = 0; j < array.Length; j++)
			{
				EventLog eventLog2 = new EventLog();
				eventLog2.Log = array[j];
				eventLog2.MachineName = machineName;
				SafeEventLogReadHandle safeEventLogReadHandle = SafeEventLogReadHandle.OpenEventLog(machineName, array[j]);
				if (!safeEventLogReadHandle.IsInvalid)
				{
					safeEventLogReadHandle.Close();
					list.Add(eventLog2);
				}
				else if (Marshal.GetLastWin32Error() != 87)
				{
					list.Add(eventLog2);
				}
			}
			return list.ToArray();
		}

		private static bool GetDisableEventLogRegistryKeysFilteringSwitchValue()
		{
			try
			{
				using RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\.NETFramework\\AppContext", writable: false);
				if (registryKey == null)
				{
					return false;
				}
				string text = registryKey.GetValue("Switch.System.Diagnostics.EventLog.DisableEventLogRegistryKeysFiltering", null) as string;
				return text != null && (text.Equals("true", StringComparison.OrdinalIgnoreCase) ? true : false);
			}
			catch
			{
			}
			return false;
		}

		private int GetCachedEntryPos(int entryIndex)
		{
			if (cache == null || (boolFlags[2] && entryIndex < firstCachedEntry) || (!boolFlags[2] && entryIndex > firstCachedEntry) || firstCachedEntry == -1)
			{
				return -1;
			}
			while (lastSeenEntry < entryIndex)
			{
				lastSeenEntry++;
				if (boolFlags[2])
				{
					lastSeenPos = GetNextEntryPos(lastSeenPos);
					if (lastSeenPos >= bytesCached)
					{
						break;
					}
				}
				else
				{
					lastSeenPos = GetPreviousEntryPos(lastSeenPos);
					if (lastSeenPos < 0)
					{
						break;
					}
				}
			}
			while (lastSeenEntry > entryIndex)
			{
				lastSeenEntry--;
				if (boolFlags[2])
				{
					lastSeenPos = GetPreviousEntryPos(lastSeenPos);
					if (lastSeenPos < 0)
					{
						break;
					}
				}
				else
				{
					lastSeenPos = GetNextEntryPos(lastSeenPos);
					if (lastSeenPos >= bytesCached)
					{
						break;
					}
				}
			}
			if (lastSeenPos >= bytesCached)
			{
				lastSeenPos = GetPreviousEntryPos(lastSeenPos);
				if (boolFlags[2])
				{
					lastSeenEntry--;
				}
				else
				{
					lastSeenEntry++;
				}
				return -1;
			}
			if (lastSeenPos < 0)
			{
				lastSeenPos = 0;
				if (boolFlags[2])
				{
					lastSeenEntry++;
				}
				else
				{
					lastSeenEntry--;
				}
				return -1;
			}
			return lastSeenPos;
		}

		internal EventLogEntry GetEntryAt(int index)
		{
			EventLogEntry entryAtNoThrow = GetEntryAtNoThrow(index);
			if (entryAtNoThrow == null)
			{
				throw new ArgumentException(SR.GetString("IndexOutOfBounds", index.ToString(CultureInfo.CurrentCulture)));
			}
			return entryAtNoThrow;
		}

		internal EventLogEntry GetEntryAtNoThrow(int index)
		{
			if (!IsOpenForRead)
			{
				OpenForRead(machineName);
			}
			if (index < 0 || index >= EntryCount)
			{
				return null;
			}
			index += OldestEntryNumber;
			return GetEntryWithOldest(index);
		}

		private EventLogEntry GetEntryWithOldest(int index)
		{
			EventLogEntry eventLogEntry = null;
			int cachedEntryPos = GetCachedEntryPos(index);
			if (cachedEntryPos >= 0)
			{
				return new EventLogEntry(cache, cachedEntryPos, this);
			}
			string currentMachineName = machineName;
			int num = 0;
			if (GetCachedEntryPos(index + 1) < 0)
			{
				num = 6;
				boolFlags[2] = true;
			}
			else
			{
				num = 10;
				boolFlags[2] = false;
			}
			cache = new byte[40000];
			int[] array = new int[1];
			int[] array2 = new int[1]
			{
				cache.Length
			};
			bool flag = Microsoft.Win32.UnsafeNativeMethods.ReadEventLog(readHandle, num, index, cache, cache.Length, array, array2);
			if (!flag)
			{
				int lastWin32Error = Marshal.GetLastWin32Error();
				if (lastWin32Error == 122 || lastWin32Error == 1503)
				{
					if (lastWin32Error == 1503)
					{
						byte[] array3 = cache;
						Reset(currentMachineName);
						cache = array3;
					}
					else if (array2[0] > cache.Length)
					{
						cache = new byte[array2[0]];
					}
					flag = Microsoft.Win32.UnsafeNativeMethods.ReadEventLog(readHandle, 6, index, cache, cache.Length, array, array2);
				}
				if (!flag)
				{
					throw new InvalidOperationException(SR.GetString("CantReadLogEntryAt", index.ToString(CultureInfo.CurrentCulture)), SharedUtils.CreateSafeWin32Exception());
				}
			}
			bytesCached = array[0];
			firstCachedEntry = index;
			lastSeenEntry = index;
			lastSeenPos = 0;
			return new EventLogEntry(cache, 0, this);
		}

		internal static RegistryKey GetEventLogRegKey(string machine, bool writable)
		{
			RegistryKey registryKey = null;
			try
			{
				registryKey = ((!machine.Equals(".")) ? RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine, machine) : Registry.LocalMachine);
				if (registryKey != null)
				{
					return registryKey.OpenSubKey("SYSTEM\\CurrentControlSet\\Services\\EventLog", writable);
				}
			}
			finally
			{
				registryKey?.Close();
			}
			return null;
		}

		private RegistryKey GetLogRegKey(string currentMachineName, bool writable)
		{
			string text = GetLogName(currentMachineName);
			if (!ValidLogName(text, ignoreEmpty: false))
			{
				throw new InvalidOperationException(SR.GetString("BadLogName"));
			}
			RegistryKey registryKey = null;
			RegistryKey registryKey2 = null;
			try
			{
				registryKey = GetEventLogRegKey(currentMachineName, writable: false);
				if (registryKey == null)
				{
					throw new InvalidOperationException(SR.GetString("RegKeyMissingShort", "SYSTEM\\CurrentControlSet\\Services\\EventLog", currentMachineName));
				}
				registryKey2 = registryKey.OpenSubKey(text, writable);
				if (registryKey2 == null)
				{
					throw new InvalidOperationException(SR.GetString("MissingLog", text, currentMachineName));
				}
				return registryKey2;
			}
			finally
			{
				registryKey?.Close();
			}
		}

		private object GetLogRegValue(string currentMachineName, string valuename)
		{
			PermissionSet permissionSet = _GetAssertPermSet();
			permissionSet.Assert();
			RegistryKey registryKey = null;
			try
			{
				registryKey = GetLogRegKey(currentMachineName, writable: false);
				if (registryKey == null)
				{
					throw new InvalidOperationException(SR.GetString("MissingLog", GetLogName(currentMachineName), currentMachineName));
				}
				return registryKey.GetValue(valuename);
			}
			finally
			{
				registryKey?.Close();
				CodeAccessPermission.RevertAssert();
			}
		}

		private int GetNextEntryPos(int pos)
		{
			return pos + IntFrom(cache, pos);
		}

		private int GetPreviousEntryPos(int pos)
		{
			return pos - IntFrom(cache, pos - 4);
		}

		internal static string GetDllPath(string machineName)
		{
			return SharedUtils.GetLatestBuildDllDirectory(machineName) + "\\EventLogMessages.dll";
		}

		private static int IntFrom(byte[] buf, int offset)
		{
			return (-16777216 & (buf[offset + 3] << 24)) | (0xFF0000 & (buf[offset + 2] << 16)) | (0xFF00 & (buf[offset + 1] << 8)) | (0xFF & buf[offset]);
		}

		public static bool SourceExists(string source)
		{
			return SourceExists(source, ".");
		}

		public static bool SourceExists(string source, string machineName)
		{
			if (!SyntaxCheck.CheckMachineName(machineName))
			{
				throw new ArgumentException(SR.GetString("InvalidParameter", "machineName", machineName));
			}
			EventLogPermission eventLogPermission = new EventLogPermission(EventLogPermissionAccess.Write, machineName);
			eventLogPermission.Demand();
			using RegistryKey registryKey = FindSourceRegistration(source, machineName, readOnly: true);
			return registryKey != null;
		}

		public static string LogNameFromSourceName(string source, string machineName)
		{
			EventLogPermission eventLogPermission = new EventLogPermission(EventLogPermissionAccess.Administer, machineName);
			eventLogPermission.Demand();
			using RegistryKey registryKey = FindSourceRegistration(source, machineName, readOnly: true);
			if (registryKey == null)
			{
				return "";
			}
			string name = registryKey.Name;
			int num = name.LastIndexOf('\\');
			return name.Substring(num + 1);
		}

		[ComVisible(false)]
		public void ModifyOverflowPolicy(OverflowAction action, int retentionDays)
		{
			string currentMachineName = machineName;
			EventLogPermission eventLogPermission = new EventLogPermission(EventLogPermissionAccess.Administer, currentMachineName);
			eventLogPermission.Demand();
			if (action < OverflowAction.DoNotOverwrite || action > OverflowAction.OverwriteOlder)
			{
				throw new InvalidEnumArgumentException("action", (int)action, typeof(OverflowAction));
			}
			long num = (long)action;
			if (action == OverflowAction.OverwriteOlder)
			{
				if (retentionDays < 1 || retentionDays > 365)
				{
					throw new ArgumentOutOfRangeException(SR.GetString("RentionDaysOutOfRange"));
				}
				num = (long)retentionDays * 86400L;
			}
			PermissionSet permissionSet = _GetAssertPermSet();
			permissionSet.Assert();
			using RegistryKey registryKey = GetLogRegKey(currentMachineName, writable: true);
			registryKey.SetValue("Retention", num, RegistryValueKind.DWord);
		}

		private void OpenForRead(string currentMachineName)
		{
			if (boolFlags[256])
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			string text = GetLogName(currentMachineName);
			if (text == null || text.Length == 0)
			{
				throw new ArgumentException(SR.GetString("MissingLogProperty"));
			}
			if (!Exists(text, currentMachineName))
			{
				throw new InvalidOperationException(SR.GetString("LogDoesNotExists", text, currentMachineName));
			}
			SharedUtils.CheckEnvironment();
			lastSeenEntry = 0;
			lastSeenPos = 0;
			bytesCached = 0;
			firstCachedEntry = -1;
			readHandle = SafeEventLogReadHandle.OpenEventLog(currentMachineName, text);
			if (readHandle.IsInvalid)
			{
				Win32Exception innerException = null;
				if (Marshal.GetLastWin32Error() != 0)
				{
					innerException = SharedUtils.CreateSafeWin32Exception();
				}
				throw new InvalidOperationException(SR.GetString("CantOpenLog", text.ToString(), currentMachineName), innerException);
			}
		}

		private void OpenForWrite(string currentMachineName)
		{
			if (boolFlags[256])
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			if (sourceName == null || sourceName.Length == 0)
			{
				throw new ArgumentException(SR.GetString("NeedSourceToOpen"));
			}
			SharedUtils.CheckEnvironment();
			writeHandle = SafeEventLogWriteHandle.RegisterEventSource(currentMachineName, sourceName);
			if (writeHandle.IsInvalid)
			{
				Win32Exception innerException = null;
				if (Marshal.GetLastWin32Error() != 0)
				{
					innerException = SharedUtils.CreateSafeWin32Exception();
				}
				throw new InvalidOperationException(SR.GetString("CantOpenLogAccess", sourceName), innerException);
			}
		}

		[ComVisible(false)]
		public void RegisterDisplayName(string resourceFile, long resourceId)
		{
			string currentMachineName = machineName;
			EventLogPermission eventLogPermission = new EventLogPermission(EventLogPermissionAccess.Administer, currentMachineName);
			eventLogPermission.Demand();
			PermissionSet permissionSet = _GetAssertPermSet();
			permissionSet.Assert();
			using RegistryKey registryKey = GetLogRegKey(currentMachineName, writable: true);
			registryKey.SetValue("DisplayNameFile", resourceFile, RegistryValueKind.ExpandString);
			registryKey.SetValue("DisplayNameID", resourceId, RegistryValueKind.DWord);
		}

		private void Reset(string currentMachineName)
		{
			bool isOpenForRead = IsOpenForRead;
			bool isOpenForWrite = IsOpenForWrite;
			bool value = boolFlags[8];
			bool flag = boolFlags[16];
			Close(currentMachineName);
			cache = null;
			if (isOpenForRead)
			{
				OpenForRead(currentMachineName);
			}
			if (isOpenForWrite)
			{
				OpenForWrite(currentMachineName);
			}
			if (flag)
			{
				StartListening(currentMachineName, GetLogName(currentMachineName));
			}
			boolFlags[8] = value;
		}

		[HostProtection(SecurityAction.LinkDemand, Synchronization = true)]
		private static void RemoveListenerComponent(EventLog component, string compLogName)
		{
			lock (InternalSyncObject)
			{
				LogListeningInfo logListeningInfo = (LogListeningInfo)listenerInfos[compLogName];
				logListeningInfo.listeningComponents.Remove(component);
				if (logListeningInfo.listeningComponents.Count == 0)
				{
					logListeningInfo.handleOwner.Dispose();
					logListeningInfo.registeredWaitHandle.Unregister(logListeningInfo.waitHandle);
					logListeningInfo.waitHandle.Close();
					listenerInfos[compLogName] = null;
				}
			}
		}

		private static void SetSpecialLogRegValues(RegistryKey logKey, string logName)
		{
			if (logKey.GetValue("MaxSize") == null)
			{
				logKey.SetValue("MaxSize", 524288, RegistryValueKind.DWord);
			}
			if (logKey.GetValue("AutoBackupLogFiles") == null)
			{
				logKey.SetValue("AutoBackupLogFiles", 0, RegistryValueKind.DWord);
			}
			if (!SkipRegPatch)
			{
				if (logKey.GetValue("Retention") == null)
				{
					logKey.SetValue("Retention", 604800, RegistryValueKind.DWord);
				}
				if (logKey.GetValue("File") == null)
				{
					string value = ((logName.Length <= 8) ? ("%SystemRoot%\\System32\\config\\" + logName + ".evt") : ("%SystemRoot%\\System32\\config\\" + logName.Substring(0, 8) + ".evt"));
					logKey.SetValue("File", value, RegistryValueKind.ExpandString);
				}
			}
		}

		private static void SetSpecialSourceRegValues(RegistryKey sourceLogKey, EventSourceCreationData sourceData)
		{
			if (string.IsNullOrEmpty(sourceData.MessageResourceFile))
			{
				sourceLogKey.SetValue("EventMessageFile", GetDllPath(sourceData.MachineName), RegistryValueKind.ExpandString);
			}
			else
			{
				sourceLogKey.SetValue("EventMessageFile", FixupPath(sourceData.MessageResourceFile), RegistryValueKind.ExpandString);
			}
			if (!string.IsNullOrEmpty(sourceData.ParameterResourceFile))
			{
				sourceLogKey.SetValue("ParameterMessageFile", FixupPath(sourceData.ParameterResourceFile), RegistryValueKind.ExpandString);
			}
			if (!string.IsNullOrEmpty(sourceData.CategoryResourceFile))
			{
				sourceLogKey.SetValue("CategoryMessageFile", FixupPath(sourceData.CategoryResourceFile), RegistryValueKind.ExpandString);
				sourceLogKey.SetValue("CategoryCount", sourceData.CategoryCount, RegistryValueKind.DWord);
			}
		}

		private static string FixupPath(string path)
		{
			if (path[0] == '%')
			{
				return path;
			}
			return Path.GetFullPath(path);
		}

		[HostProtection(SecurityAction.LinkDemand, Synchronization = true, ExternalThreading = true)]
		private void StartListening(string currentMachineName, string currentLogName)
		{
			lastSeenCount = EntryCount + OldestEntryNumber;
			AddListenerComponent(this, currentMachineName, currentLogName);
			boolFlags[16] = true;
		}

		private void StartRaisingEvents(string currentMachineName, string currentLogName)
		{
			if (!boolFlags[4] && !boolFlags[8] && !base.DesignMode)
			{
				StartListening(currentMachineName, currentLogName);
			}
			boolFlags[8] = true;
		}

		private static void StaticCompletionCallback(object context, bool wasSignaled)
		{
			LogListeningInfo logListeningInfo = (LogListeningInfo)context;
			EventLog[] array = (EventLog[])logListeningInfo.listeningComponents.ToArray(typeof(EventLog));
			for (int i = 0; i < array.Length; i++)
			{
				try
				{
					if (array[i] != null)
					{
						array[i].CompletionCallback(null);
					}
				}
				catch (ObjectDisposedException)
				{
				}
			}
		}

		[HostProtection(SecurityAction.LinkDemand, Synchronization = true, ExternalThreading = true)]
		private void StopListening(string currentLogName)
		{
			RemoveListenerComponent(this, currentLogName);
			boolFlags[16] = false;
		}

		private void StopRaisingEvents(string currentLogName)
		{
			if (!boolFlags[4] && boolFlags[8] && !base.DesignMode)
			{
				StopListening(currentLogName);
			}
			boolFlags[8] = false;
		}

		internal static string TryFormatMessage(SafeLibraryHandle hModule, uint messageNum, string[] insertionStrings)
		{
			string text = null;
			int num = 0;
			StringBuilder stringBuilder = new StringBuilder(1024);
			int num2 = 10240;
			IntPtr[] array = new IntPtr[insertionStrings.Length];
			GCHandle[] array2 = new GCHandle[insertionStrings.Length];
			GCHandle gCHandle = GCHandle.Alloc(array, GCHandleType.Pinned);
			if (insertionStrings.Length == 0)
			{
				num2 |= 0x200;
			}
			try
			{
				for (int i = 0; i < array2.Length; i++)
				{
					ref GCHandle reference = ref array2[i];
					reference = GCHandle.Alloc(insertionStrings[i], GCHandleType.Pinned);
					ref IntPtr reference2 = ref array[i];
					reference2 = array2[i].AddrOfPinnedObject();
				}
				int num3 = 122;
				while (num == 0 && num3 == 122)
				{
					num = Microsoft.Win32.SafeNativeMethods.FormatMessage(num2, hModule, messageNum, 0, stringBuilder, stringBuilder.Capacity, array);
					if (num == 0)
					{
						num3 = Marshal.GetLastWin32Error();
						if (num3 == 122)
						{
							stringBuilder.Capacity *= 2;
						}
					}
				}
			}
			catch
			{
				num = 0;
			}
			finally
			{
				for (int j = 0; j < array2.Length; j++)
				{
					if (array2[j].IsAllocated)
					{
						array2[j].Free();
					}
				}
				gCHandle.Free();
			}
			if (num > 0)
			{
				text = stringBuilder.ToString();
				if (text.Length > 1 && text[text.Length - 1] == '\n')
				{
					text = text.Substring(0, text.Length - 2);
				}
			}
			return text;
		}

		private static bool CharIsPrintable(char c)
		{
			UnicodeCategory unicodeCategory = char.GetUnicodeCategory(c);
			if (unicodeCategory == UnicodeCategory.Control && unicodeCategory != UnicodeCategory.Format && unicodeCategory != UnicodeCategory.LineSeparator && unicodeCategory != UnicodeCategory.ParagraphSeparator)
			{
				return unicodeCategory == UnicodeCategory.OtherNotAssigned;
			}
			return true;
		}

		internal static bool ValidLogName(string logName, bool ignoreEmpty)
		{
			if (logName.Length == 0 && !ignoreEmpty)
			{
				return false;
			}
			foreach (char c in logName)
			{
				if (!CharIsPrintable(c) || c == '\\' || c == '*' || c == '?')
				{
					return false;
				}
			}
			return true;
		}

		private void VerifyAndCreateSource(string sourceName, string currentMachineName)
		{
			if (boolFlags[512])
			{
				return;
			}
			if (!SourceExists(sourceName, currentMachineName))
			{
				Mutex mutex = null;
				RuntimeHelpers.PrepareConstrainedRegions();
				try
				{
					SharedUtils.EnterMutex("netfxeventlog.1.0", ref mutex);
					if (!SourceExists(sourceName, currentMachineName))
					{
						if (GetLogName(currentMachineName) == null)
						{
							SetLogName(currentMachineName, "Application");
						}
						CreateEventSource(new EventSourceCreationData(sourceName, GetLogName(currentMachineName), currentMachineName));
						Reset(currentMachineName);
					}
					else
					{
						string text = LogNameFromSourceName(sourceName, currentMachineName);
						string text2 = GetLogName(currentMachineName);
						if (text != null && text2 != null && string.Compare(text, text2, StringComparison.OrdinalIgnoreCase) != 0)
						{
							throw new ArgumentException(SR.GetString("LogSourceMismatch", Source.ToString(), text2, text));
						}
					}
				}
				finally
				{
					if (mutex != null)
					{
						mutex.ReleaseMutex();
						mutex.Close();
					}
				}
			}
			else
			{
				string text3 = LogNameFromSourceName(sourceName, currentMachineName);
				string text4 = GetLogName(currentMachineName);
				if (text3 != null && text4 != null && string.Compare(text3, text4, StringComparison.OrdinalIgnoreCase) != 0)
				{
					throw new ArgumentException(SR.GetString("LogSourceMismatch", Source.ToString(), text4, text3));
				}
			}
			boolFlags[512] = true;
		}

		public void WriteEntry(string message)
		{
			WriteEntry(message, EventLogEntryType.Information, 0, 0, null);
		}

		public static void WriteEntry(string source, string message)
		{
			WriteEntry(source, message, EventLogEntryType.Information, 0, 0, null);
		}

		public void WriteEntry(string message, EventLogEntryType type)
		{
			WriteEntry(message, type, 0, 0, null);
		}

		public static void WriteEntry(string source, string message, EventLogEntryType type)
		{
			WriteEntry(source, message, type, 0, 0, null);
		}

		public void WriteEntry(string message, EventLogEntryType type, int eventID)
		{
			WriteEntry(message, type, eventID, 0, null);
		}

		public static void WriteEntry(string source, string message, EventLogEntryType type, int eventID)
		{
			WriteEntry(source, message, type, eventID, 0, null);
		}

		public void WriteEntry(string message, EventLogEntryType type, int eventID, short category)
		{
			WriteEntry(message, type, eventID, category, null);
		}

		public static void WriteEntry(string source, string message, EventLogEntryType type, int eventID, short category)
		{
			WriteEntry(source, message, type, eventID, category, null);
		}

		public static void WriteEntry(string source, string message, EventLogEntryType type, int eventID, short category, byte[] rawData)
		{
			EventLog eventLog = new EventLog();
			try
			{
				eventLog.Source = source;
				eventLog.WriteEntry(message, type, eventID, category, rawData);
			}
			finally
			{
				eventLog.Dispose(disposing: true);
			}
		}

		public void WriteEntry(string message, EventLogEntryType type, int eventID, short category, byte[] rawData)
		{
			if (eventID < 0 || eventID > 65535)
			{
				throw new ArgumentException(SR.GetString("EventID", eventID, 0, 65535));
			}
			if (Source.Length == 0)
			{
				throw new ArgumentException(SR.GetString("NeedSourceToWrite"));
			}
			if (!Enum.IsDefined(typeof(EventLogEntryType), type))
			{
				throw new InvalidEnumArgumentException("type", (int)type, typeof(EventLogEntryType));
			}
			string currentMachineName = machineName;
			if (!boolFlags[32])
			{
				EventLogPermission eventLogPermission = new EventLogPermission(EventLogPermissionAccess.Write, currentMachineName);
				eventLogPermission.Demand();
				boolFlags[32] = true;
			}
			VerifyAndCreateSource(sourceName, currentMachineName);
			InternalWriteEvent((uint)eventID, (ushort)category, type, new string[1]
			{
				message
			}, rawData, currentMachineName);
		}

		[ComVisible(false)]
		public void WriteEvent(EventInstance instance, params object[] values)
		{
			WriteEvent(instance, null, values);
		}

		[ComVisible(false)]
		public void WriteEvent(EventInstance instance, byte[] data, params object[] values)
		{
			if (instance == null)
			{
				throw new ArgumentNullException("instance");
			}
			if (Source.Length == 0)
			{
				throw new ArgumentException(SR.GetString("NeedSourceToWrite"));
			}
			string currentMachineName = machineName;
			if (!boolFlags[32])
			{
				EventLogPermission eventLogPermission = new EventLogPermission(EventLogPermissionAccess.Write, currentMachineName);
				eventLogPermission.Demand();
				boolFlags[32] = true;
			}
			VerifyAndCreateSource(Source, currentMachineName);
			string[] array = null;
			if (values != null)
			{
				array = new string[values.Length];
				for (int i = 0; i < values.Length; i++)
				{
					if (values[i] != null)
					{
						array[i] = values[i].ToString();
					}
					else
					{
						array[i] = string.Empty;
					}
				}
			}
			InternalWriteEvent((uint)instance.InstanceId, (ushort)instance.CategoryId, instance.EntryType, array, data, currentMachineName);
		}

		public static void WriteEvent(string source, EventInstance instance, params object[] values)
		{
			using EventLog eventLog = new EventLog();
			eventLog.Source = source;
			eventLog.WriteEvent(instance, null, values);
		}

		public static void WriteEvent(string source, EventInstance instance, byte[] data, params object[] values)
		{
			using EventLog eventLog = new EventLog();
			eventLog.Source = source;
			eventLog.WriteEvent(instance, data, values);
		}

		private void InternalWriteEvent(uint eventID, ushort category, EventLogEntryType type, string[] strings, byte[] rawData, string currentMachineName)
		{
			if (strings == null)
			{
				strings = new string[0];
			}
			if (strings.Length >= 256)
			{
				throw new ArgumentException(SR.GetString("TooManyReplacementStrings"));
			}
			for (int i = 0; i < strings.Length; i++)
			{
				if (strings[i] == null)
				{
					strings[i] = string.Empty;
				}
				if (strings[i].Length > 32766)
				{
					throw new ArgumentException(SR.GetString("LogEntryTooLong"));
				}
			}
			if (rawData == null)
			{
				rawData = new byte[0];
			}
			if (Source.Length == 0)
			{
				throw new ArgumentException(SR.GetString("NeedSourceToWrite"));
			}
			if (!IsOpenForWrite)
			{
				OpenForWrite(currentMachineName);
			}
			IntPtr[] array = new IntPtr[strings.Length];
			GCHandle[] array2 = new GCHandle[strings.Length];
			GCHandle gCHandle = GCHandle.Alloc(array, GCHandleType.Pinned);
			try
			{
				for (int j = 0; j < strings.Length; j++)
				{
					ref GCHandle reference = ref array2[j];
					reference = GCHandle.Alloc(strings[j], GCHandleType.Pinned);
					ref IntPtr reference2 = ref array[j];
					reference2 = array2[j].AddrOfPinnedObject();
				}
				byte[] userSID = null;
				if (!Microsoft.Win32.UnsafeNativeMethods.ReportEvent(writeHandle, (short)type, category, eventID, userSID, (short)strings.Length, rawData.Length, new HandleRef(this, gCHandle.AddrOfPinnedObject()), rawData))
				{
					throw SharedUtils.CreateSafeWin32Exception();
				}
			}
			finally
			{
				for (int k = 0; k < strings.Length; k++)
				{
					if (array2[k].IsAllocated)
					{
						array2[k].Free();
					}
				}
				gCHandle.Free();
			}
		}
	}
}
