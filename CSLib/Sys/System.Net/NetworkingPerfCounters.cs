using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using System.Text;

namespace System.Net
{
	internal static class NetworkingPerfCounters
	{
		private const string CategoryName = ".NET CLR Networking";

		private const string ConnectionsEstablishedName = "Connections Established";

		private const string BytesReceivedName = "Bytes Received";

		private const string BytesSentName = "Bytes Sent";

		private const string DatagramsReceivedName = "Datagrams Received";

		private const string DatagramsSentName = "Datagrams Sent";

		private const string GlobalInstanceName = "_Global_";

		private static PerformanceCounter ConnectionsEstablished;

		private static PerformanceCounter BytesReceived;

		private static PerformanceCounter BytesSent;

		private static PerformanceCounter DatagramsReceived;

		private static PerformanceCounter DatagramsSent;

		private static PerformanceCounter globalConnectionsEstablished;

		private static PerformanceCounter globalBytesReceived;

		private static PerformanceCounter globalBytesSent;

		private static PerformanceCounter globalDatagramsReceived;

		private static PerformanceCounter globalDatagramsSent;

		private static object syncObject = new object();

		private static bool initialized = false;

		internal static void Initialize()
		{
			if (initialized)
			{
				return;
			}
			lock (syncObject)
			{
				if (initialized)
				{
					return;
				}
				if (ComNetOS.IsWin2K)
				{
					PerformanceCounterPermission performanceCounterPermission = new PerformanceCounterPermission(PermissionState.Unrestricted);
					performanceCounterPermission.Assert();
					try
					{
						string instanceName = GetInstanceName();
						ConnectionsEstablished = new PerformanceCounter();
						ConnectionsEstablished.CategoryName = ".NET CLR Networking";
						ConnectionsEstablished.CounterName = "Connections Established";
						ConnectionsEstablished.InstanceName = instanceName;
						ConnectionsEstablished.InstanceLifetime = PerformanceCounterInstanceLifetime.Process;
						ConnectionsEstablished.ReadOnly = false;
						ConnectionsEstablished.RawValue = 0L;
						BytesReceived = new PerformanceCounter();
						BytesReceived.CategoryName = ".NET CLR Networking";
						BytesReceived.CounterName = "Bytes Received";
						BytesReceived.InstanceName = instanceName;
						BytesReceived.InstanceLifetime = PerformanceCounterInstanceLifetime.Process;
						BytesReceived.ReadOnly = false;
						BytesReceived.RawValue = 0L;
						BytesSent = new PerformanceCounter();
						BytesSent.CategoryName = ".NET CLR Networking";
						BytesSent.CounterName = "Bytes Sent";
						BytesSent.InstanceName = instanceName;
						BytesSent.InstanceLifetime = PerformanceCounterInstanceLifetime.Process;
						BytesSent.ReadOnly = false;
						BytesSent.RawValue = 0L;
						DatagramsReceived = new PerformanceCounter();
						DatagramsReceived.CategoryName = ".NET CLR Networking";
						DatagramsReceived.CounterName = "Datagrams Received";
						DatagramsReceived.InstanceName = instanceName;
						DatagramsReceived.InstanceLifetime = PerformanceCounterInstanceLifetime.Process;
						DatagramsReceived.ReadOnly = false;
						DatagramsReceived.RawValue = 0L;
						DatagramsSent = new PerformanceCounter();
						DatagramsSent.CategoryName = ".NET CLR Networking";
						DatagramsSent.CounterName = "Datagrams Sent";
						DatagramsSent.InstanceName = instanceName;
						DatagramsSent.InstanceLifetime = PerformanceCounterInstanceLifetime.Process;
						DatagramsSent.ReadOnly = false;
						DatagramsSent.RawValue = 0L;
						globalConnectionsEstablished = new PerformanceCounter(".NET CLR Networking", "Connections Established", "_Global_", readOnly: false);
						globalBytesReceived = new PerformanceCounter(".NET CLR Networking", "Bytes Received", "_Global_", readOnly: false);
						globalBytesSent = new PerformanceCounter(".NET CLR Networking", "Bytes Sent", "_Global_", readOnly: false);
						globalDatagramsReceived = new PerformanceCounter(".NET CLR Networking", "Datagrams Received", "_Global_", readOnly: false);
						globalDatagramsSent = new PerformanceCounter(".NET CLR Networking", "Datagrams Sent", "_Global_", readOnly: false);
						AppDomain.CurrentDomain.DomainUnload += ExitOrUnloadEventHandler;
						AppDomain.CurrentDomain.ProcessExit += ExitOrUnloadEventHandler;
						AppDomain.CurrentDomain.UnhandledException += ExceptionEventHandler;
					}
					catch (Win32Exception)
					{
					}
					catch (InvalidOperationException)
					{
					}
					finally
					{
						CodeAccessPermission.RevertAssert();
					}
				}
				initialized = true;
			}
		}

		private static void ExceptionEventHandler(object sender, UnhandledExceptionEventArgs e)
		{
			if (e.IsTerminating)
			{
				Cleanup();
			}
		}

		private static void ExitOrUnloadEventHandler(object sender, EventArgs e)
		{
			Cleanup();
		}

		private static void Cleanup()
		{
			ConnectionsEstablished?.RemoveInstance();
			BytesReceived?.RemoveInstance();
			BytesSent?.RemoveInstance();
			DatagramsReceived?.RemoveInstance();
			DatagramsSent?.RemoveInstance();
		}

		[FileIOPermission(SecurityAction.Assert, Unrestricted = true)]
		private static string GetAssemblyName()
		{
			string result = null;
			Assembly entryAssembly = Assembly.GetEntryAssembly();
			if (entryAssembly != null)
			{
				AssemblyName name = entryAssembly.GetName();
				if (name != null)
				{
					result = name.Name;
				}
			}
			return result;
		}

		[SecurityPermission(SecurityAction.Assert, Unrestricted = true)]
		private static string GetInstanceName()
		{
			string text = null;
			string text2 = GetAssemblyName();
			if (text2 == null || text2.Length == 0)
			{
				text2 = AppDomain.CurrentDomain.FriendlyName;
			}
			StringBuilder stringBuilder = new StringBuilder(text2);
			for (int i = 0; i < stringBuilder.Length; i++)
			{
				switch (stringBuilder[i])
				{
				case '(':
					stringBuilder[i] = '[';
					break;
				case ')':
					stringBuilder[i] = ']';
					break;
				case '#':
				case '/':
				case '\\':
					stringBuilder[i] = '_';
					break;
				}
			}
			return string.Format(CultureInfo.CurrentCulture, "{0}[{1}]", stringBuilder.ToString(), Process.GetCurrentProcess().Id);
		}

		internal static void IncrementConnectionsEstablished()
		{
			if (ConnectionsEstablished != null)
			{
				ConnectionsEstablished.Increment();
			}
			if (globalConnectionsEstablished != null)
			{
				globalConnectionsEstablished.Increment();
			}
		}

		internal static void AddBytesReceived(int increment)
		{
			if (BytesReceived != null)
			{
				BytesReceived.IncrementBy(increment);
			}
			if (globalBytesReceived != null)
			{
				globalBytesReceived.IncrementBy(increment);
			}
		}

		internal static void AddBytesSent(int increment)
		{
			if (BytesSent != null)
			{
				BytesSent.IncrementBy(increment);
			}
			if (globalBytesSent != null)
			{
				globalBytesSent.IncrementBy(increment);
			}
		}

		internal static void IncrementDatagramsReceived()
		{
			if (DatagramsReceived != null)
			{
				DatagramsReceived.Increment();
			}
			if (globalDatagramsReceived != null)
			{
				globalDatagramsReceived.Increment();
			}
		}

		internal static void IncrementDatagramsSent()
		{
			if (DatagramsSent != null)
			{
				DatagramsSent.Increment();
			}
			if (globalDatagramsSent != null)
			{
				globalDatagramsSent.Increment();
			}
		}
	}
}
