using System.Collections;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading;

namespace System.Net
{
	internal static class NclUtilities
	{
		private static ContextCallback s_ContextRelativeDemandCallback;

		private static IPAddress[] _LocalAddresses;

		private static object _LocalAddressesLock;

		private static NetworkAddressChangePolled s_AddressChange;

		internal static bool HasShutdownStarted
		{
			get
			{
				if (!Environment.HasShutdownStarted)
				{
					return AppDomain.CurrentDomain.IsFinalizingForUnload();
				}
				return true;
			}
		}

		internal static ContextCallback ContextRelativeDemandCallback
		{
			get
			{
				if (s_ContextRelativeDemandCallback == null)
				{
					s_ContextRelativeDemandCallback = DemandCallback;
				}
				return s_ContextRelativeDemandCallback;
			}
		}

		internal static IPAddress[] LocalAddresses
		{
			get
			{
				if (s_AddressChange != null && s_AddressChange.CheckAndReset())
				{
					return _LocalAddresses = GetLocalAddresses();
				}
				if (_LocalAddresses != null)
				{
					return _LocalAddresses;
				}
				lock (LocalAddressesLock)
				{
					if (_LocalAddresses != null)
					{
						return _LocalAddresses;
					}
					s_AddressChange = new NetworkAddressChangePolled();
					return _LocalAddresses = GetLocalAddresses();
				}
			}
		}

		private static object LocalAddressesLock
		{
			get
			{
				if (_LocalAddressesLock == null)
				{
					Interlocked.CompareExchange(ref _LocalAddressesLock, new object(), null);
				}
				return _LocalAddressesLock;
			}
		}

		internal static bool IsThreadPoolLow()
		{
			if (ComNetOS.IsAspNetServer)
			{
				return false;
			}
			ThreadPool.GetAvailableThreads(out var workerThreads, out var completionPortThreads);
			if (workerThreads >= 2)
			{
				if (ComNetOS.IsWinNt)
				{
					return completionPortThreads < 2;
				}
				return false;
			}
			return true;
		}

		internal static bool IsCredentialFailure(SecurityStatus error)
		{
			if (error != SecurityStatus.LogonDenied && error != SecurityStatus.UnknownCredentials && error != SecurityStatus.NoImpersonation && error != SecurityStatus.NoAuthenticatingAuthority && error != SecurityStatus.UntrustedRoot && error != SecurityStatus.CertExpired && error != SecurityStatus.SmartcardLogonRequired)
			{
				return error == SecurityStatus.BadBinding;
			}
			return true;
		}

		internal static bool IsClientFault(SecurityStatus error)
		{
			if (error != SecurityStatus.InvalidToken && error != SecurityStatus.CannotPack && error != SecurityStatus.QopNotSupported && error != SecurityStatus.NoCredentials && error != SecurityStatus.MessageAltered && error != SecurityStatus.OutOfSequence && error != SecurityStatus.IncompleteMessage && error != SecurityStatus.IncompleteCredentials && error != SecurityStatus.WrongPrincipal && error != SecurityStatus.TimeSkew && error != SecurityStatus.IllegalMessage && error != SecurityStatus.CertUnknown && error != SecurityStatus.AlgorithmMismatch && error != SecurityStatus.SecurityQosFailed)
			{
				return error == SecurityStatus.UnsupportedPreauth;
			}
			return true;
		}

		private static void DemandCallback(object state)
		{
			((CodeAccessPermission)state).Demand();
		}

		internal static bool GuessWhetherHostIsLoopback(string host)
		{
			string a = host.ToLowerInvariant();
			if (a == "localhost" || a == "loopback")
			{
				return true;
			}
			IPGlobalProperties iPGlobalProperties = IPGlobalProperties.InternalGetIPGlobalProperties();
			string text = iPGlobalProperties.HostName.ToLowerInvariant();
			if (!(a == text))
			{
				return a == text + "." + iPGlobalProperties.DomainName.ToLowerInvariant();
			}
			return true;
		}

		internal static bool IsFatal(Exception exception)
		{
			if (exception != null)
			{
				if (!(exception is OutOfMemoryException) && !(exception is StackOverflowException))
				{
					return exception is ThreadAbortException;
				}
				return true;
			}
			return false;
		}

		private static IPAddress[] GetLocalAddresses()
		{
			if (ComNetOS.IsPostWin2K)
			{
				ArrayList arrayList = new ArrayList(16);
				int num = 0;
				SafeLocalFree safeLocalFree = null;
				GetAdaptersAddressesFlags flags = GetAdaptersAddressesFlags.SkipAnycast | GetAdaptersAddressesFlags.SkipMulticast | GetAdaptersAddressesFlags.SkipDnsServer | GetAdaptersAddressesFlags.SkipFriendlyName;
				uint outBufLen = 0u;
				uint adaptersAddresses = UnsafeNetInfoNativeMethods.GetAdaptersAddresses(AddressFamily.Unspecified, (uint)flags, IntPtr.Zero, SafeLocalFree.Zero, ref outBufLen);
				while (true)
				{
					switch (adaptersAddresses)
					{
					case 111u:
						try
						{
							safeLocalFree = SafeLocalFree.LocalAlloc((int)outBufLen);
							adaptersAddresses = UnsafeNetInfoNativeMethods.GetAdaptersAddresses(AddressFamily.Unspecified, (uint)flags, IntPtr.Zero, safeLocalFree, ref outBufLen);
							if (adaptersAddresses != 0)
							{
								break;
							}
							IpAdapterAddresses ipAdapterAddresses = (IpAdapterAddresses)Marshal.PtrToStructure(safeLocalFree.DangerousGetHandle(), typeof(IpAdapterAddresses));
							while (true)
							{
								if (ipAdapterAddresses.FirstUnicastAddress != IntPtr.Zero)
								{
									UnicastIPAddressInformationCollection unicastIPAddressInformationCollection2 = SystemUnicastIPAddressInformation.ToAddressInformationCollection(ipAdapterAddresses.FirstUnicastAddress);
									num += unicastIPAddressInformationCollection2.Count;
									arrayList.Add(unicastIPAddressInformationCollection2);
								}
								if (!(ipAdapterAddresses.next == IntPtr.Zero))
								{
									ipAdapterAddresses = (IpAdapterAddresses)Marshal.PtrToStructure(ipAdapterAddresses.next, typeof(IpAdapterAddresses));
									continue;
								}
								break;
							}
						}
						finally
						{
							safeLocalFree?.Close();
							safeLocalFree = null;
						}
						break;
					default:
						throw new NetworkInformationException((int)adaptersAddresses);
					case 0u:
					case 232u:
					{
						IPAddress[] array = new IPAddress[num];
						uint num2 = 0u;
						{
							foreach (UnicastIPAddressInformationCollection item in arrayList)
							{
								foreach (UnicastIPAddressInformation item2 in item)
								{
									array[num2++] = item2.Address;
								}
							}
							return array;
						}
					}
					}
				}
			}
			ArrayList arrayList2 = new ArrayList(16);
			int num3 = 0;
			SafeLocalFree safeLocalFree2 = null;
			uint pOutBufLen = 0u;
			uint adaptersInfo = UnsafeNetInfoNativeMethods.GetAdaptersInfo(SafeLocalFree.Zero, ref pOutBufLen);
			while (true)
			{
				switch (adaptersInfo)
				{
				case 111u:
					try
					{
						safeLocalFree2 = SafeLocalFree.LocalAlloc((int)pOutBufLen);
						adaptersInfo = UnsafeNetInfoNativeMethods.GetAdaptersInfo(safeLocalFree2, ref pOutBufLen);
						if (adaptersInfo != 0)
						{
							break;
						}
						IpAdapterInfo ipAdapterInfo = (IpAdapterInfo)Marshal.PtrToStructure(safeLocalFree2.DangerousGetHandle(), typeof(IpAdapterInfo));
						while (true)
						{
							IPAddressCollection iPAddressCollection2 = ipAdapterInfo.ipAddressList.ToIPAddressCollection();
							num3 += iPAddressCollection2.Count;
							arrayList2.Add(iPAddressCollection2);
							if (!(ipAdapterInfo.Next == IntPtr.Zero))
							{
								ipAdapterInfo = (IpAdapterInfo)Marshal.PtrToStructure(ipAdapterInfo.Next, typeof(IpAdapterInfo));
								continue;
							}
							break;
						}
					}
					finally
					{
						safeLocalFree2?.Close();
					}
					break;
				default:
					throw new NetworkInformationException((int)adaptersInfo);
				case 0u:
				case 232u:
				{
					IPAddress[] array = new IPAddress[num3];
					uint num4 = 0u;
					{
						foreach (IPAddressCollection item3 in arrayList2)
						{
							foreach (IPAddress item4 in item3)
							{
								array[num4++] = item4;
							}
						}
						return array;
					}
				}
				}
			}
		}

		internal static bool IsAddressLocal(IPAddress ipAddress)
		{
			IPAddress[] localAddresses = LocalAddresses;
			for (int i = 0; i < localAddresses.Length; i++)
			{
				if (ipAddress.Equals(localAddresses[i], compareScopeId: false))
				{
					return true;
				}
			}
			return false;
		}
	}
}
