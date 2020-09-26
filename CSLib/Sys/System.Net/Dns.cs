using System.Collections;
using System.Globalization;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Text;
using System.Threading;

namespace System.Net
{
	public static class Dns
	{
		private class ResolveAsyncResult : ContextAwareResult
		{
			internal readonly string hostName;

			internal bool includeIPv6;

			internal IPAddress address;

			internal ResolveAsyncResult(string hostName, object myObject, bool includeIPv6, object myState, AsyncCallback myCallBack)
				: base(myObject, myState, myCallBack)
			{
				this.hostName = hostName;
				this.includeIPv6 = includeIPv6;
			}

			internal ResolveAsyncResult(IPAddress address, object myObject, bool includeIPv6, object myState, AsyncCallback myCallBack)
				: base(myObject, myState, myCallBack)
			{
				this.includeIPv6 = includeIPv6;
				this.address = address;
			}
		}

		private const int HostNameBufferLength = 256;

		private const int MaxHostName = 126;

		private static DnsPermission s_DnsPermission = new DnsPermission(PermissionState.Unrestricted);

		private static WaitCallback resolveCallback = ResolveCallback;

		private static IPHostEntry NativeToHostEntry(IntPtr nativePointer)
		{
			hostent hostent = (hostent)Marshal.PtrToStructure(nativePointer, typeof(hostent));
			IPHostEntry iPHostEntry = new IPHostEntry();
			if (hostent.h_name != IntPtr.Zero)
			{
				iPHostEntry.HostName = Marshal.PtrToStringAnsi(hostent.h_name);
			}
			ArrayList arrayList = new ArrayList();
			IntPtr intPtr = hostent.h_addr_list;
			nativePointer = Marshal.ReadIntPtr(intPtr);
			while (nativePointer != IntPtr.Zero)
			{
				int newAddress = Marshal.ReadInt32(nativePointer);
				arrayList.Add(new IPAddress(newAddress));
				intPtr = IntPtrHelper.Add(intPtr, IntPtr.Size);
				nativePointer = Marshal.ReadIntPtr(intPtr);
			}
			iPHostEntry.AddressList = new IPAddress[arrayList.Count];
			arrayList.CopyTo(iPHostEntry.AddressList, 0);
			arrayList.Clear();
			intPtr = hostent.h_aliases;
			nativePointer = Marshal.ReadIntPtr(intPtr);
			while (nativePointer != IntPtr.Zero)
			{
				string value = Marshal.PtrToStringAnsi(nativePointer);
				arrayList.Add(value);
				intPtr = IntPtrHelper.Add(intPtr, IntPtr.Size);
				nativePointer = Marshal.ReadIntPtr(intPtr);
			}
			iPHostEntry.Aliases = new string[arrayList.Count];
			arrayList.CopyTo(iPHostEntry.Aliases, 0);
			return iPHostEntry;
		}

		[Obsolete("GetHostByName is obsoleted for this type, please use GetHostEntry instead. http://go.microsoft.com/fwlink/?linkid=14202")]
		public static IPHostEntry GetHostByName(string hostName)
		{
			if (hostName == null)
			{
				throw new ArgumentNullException("hostName");
			}
			s_DnsPermission.Demand();
			return InternalGetHostByName(hostName, includeIPv6: false);
		}

		internal static IPHostEntry InternalGetHostByName(string hostName)
		{
			return InternalGetHostByName(hostName, includeIPv6: true);
		}

		internal static IPHostEntry InternalGetHostByName(string hostName, bool includeIPv6)
		{
			if (Logging.On)
			{
				Logging.Enter(Logging.Sockets, "DNS", "GetHostByName", hostName);
			}
			IPHostEntry iPHostEntry = null;
			if (hostName.Length > 126)
			{
				throw new ArgumentOutOfRangeException(SR.GetString("net_toolong", "hostName", 126.ToString(NumberFormatInfo.CurrentInfo)));
			}
			if (Socket.LegacySupportsIPv6 || (includeIPv6 && ComNetOS.IsPostWin2K))
			{
				iPHostEntry = GetAddrInfo(hostName);
			}
			else
			{
				IntPtr intPtr = UnsafeNclNativeMethods.OSSOCK.gethostbyname(hostName);
				if (intPtr == IntPtr.Zero)
				{
					SocketException ex = new SocketException();
					if (IPAddress.TryParse(hostName, out var address))
					{
						iPHostEntry = new IPHostEntry();
						iPHostEntry.HostName = address.ToString();
						iPHostEntry.Aliases = new string[0];
						iPHostEntry.AddressList = new IPAddress[1]
						{
							address
						};
						if (Logging.On)
						{
							Logging.Exit(Logging.Sockets, "DNS", "GetHostByName", iPHostEntry);
						}
						return iPHostEntry;
					}
					throw ex;
				}
				iPHostEntry = NativeToHostEntry(intPtr);
			}
			if (Logging.On)
			{
				Logging.Exit(Logging.Sockets, "DNS", "GetHostByName", iPHostEntry);
			}
			return iPHostEntry;
		}

		[Obsolete("GetHostByAddress is obsoleted for this type, please use GetHostEntry instead. http://go.microsoft.com/fwlink/?linkid=14202")]
		public static IPHostEntry GetHostByAddress(string address)
		{
			if (Logging.On)
			{
				Logging.Enter(Logging.Sockets, "DNS", "GetHostByAddress", address);
			}
			s_DnsPermission.Demand();
			if (address == null)
			{
				throw new ArgumentNullException("address");
			}
			IPHostEntry iPHostEntry = InternalGetHostByAddress(IPAddress.Parse(address), includeIPv6: false, throwOnFailure: true);
			if (Logging.On)
			{
				Logging.Exit(Logging.Sockets, "DNS", "GetHostByAddress", iPHostEntry);
			}
			return iPHostEntry;
		}

		[Obsolete("GetHostByAddress is obsoleted for this type, please use GetHostEntry instead. http://go.microsoft.com/fwlink/?linkid=14202")]
		public static IPHostEntry GetHostByAddress(IPAddress address)
		{
			if (Logging.On)
			{
				Logging.Enter(Logging.Sockets, "DNS", "GetHostByAddress", "");
			}
			s_DnsPermission.Demand();
			if (address == null)
			{
				throw new ArgumentNullException("address");
			}
			IPHostEntry iPHostEntry = InternalGetHostByAddress(address, includeIPv6: false, throwOnFailure: true);
			if (Logging.On)
			{
				Logging.Exit(Logging.Sockets, "DNS", "GetHostByAddress", iPHostEntry);
			}
			return iPHostEntry;
		}

		internal static IPHostEntry InternalGetHostByAddress(IPAddress address, bool includeIPv6, bool throwOnFailure)
		{
			SocketError errorCode = SocketError.Success;
			Exception ex = null;
			if (Socket.LegacySupportsIPv6 || (includeIPv6 && ComNetOS.IsPostWin2K))
			{
				string name = TryGetNameInfo(address, out errorCode);
				if (errorCode == SocketError.Success)
				{
					return GetAddrInfo(name);
				}
				ex = new SocketException();
			}
			else
			{
				if (address.AddressFamily == AddressFamily.InterNetworkV6)
				{
					throw new SocketException(SocketError.ProtocolNotSupported);
				}
				int addr = (int)address.m_Address;
				IntPtr intPtr = UnsafeNclNativeMethods.OSSOCK.gethostbyaddr(ref addr, Marshal.SizeOf(typeof(int)), ProtocolFamily.InterNetwork);
				if (intPtr != IntPtr.Zero)
				{
					return NativeToHostEntry(intPtr);
				}
				ex = new SocketException();
			}
			if (throwOnFailure)
			{
				throw ex;
			}
			IPHostEntry iPHostEntry = new IPHostEntry();
			try
			{
				iPHostEntry.HostName = address.ToString();
				iPHostEntry.Aliases = new string[0];
				iPHostEntry.AddressList = new IPAddress[1]
				{
					address
				};
				return iPHostEntry;
			}
			catch
			{
				throw ex;
			}
		}

		public static string GetHostName()
		{
			s_DnsPermission.Demand();
			Socket.InitializeSockets();
			StringBuilder stringBuilder = new StringBuilder(256);
			if (UnsafeNclNativeMethods.OSSOCK.gethostname(stringBuilder, 256) != 0)
			{
				throw new SocketException();
			}
			return stringBuilder.ToString();
		}

		[Obsolete("Resolve is obsoleted for this type, please use GetHostEntry instead. http://go.microsoft.com/fwlink/?linkid=14202")]
		public static IPHostEntry Resolve(string hostName)
		{
			if (Logging.On)
			{
				Logging.Enter(Logging.Sockets, "DNS", "Resolve", hostName);
			}
			s_DnsPermission.Demand();
			if (hostName == null)
			{
				throw new ArgumentNullException("hostName");
			}
			IPAddress ip;
			IPHostEntry iPHostEntry = ((!TryParseAsIP(hostName, out ip) || (ip.AddressFamily == AddressFamily.InterNetworkV6 && !Socket.LegacySupportsIPv6)) ? InternalGetHostByName(hostName, includeIPv6: false) : InternalGetHostByAddress(ip, includeIPv6: false, throwOnFailure: false));
			if (Logging.On)
			{
				Logging.Exit(Logging.Sockets, "DNS", "Resolve", iPHostEntry);
			}
			return iPHostEntry;
		}

		internal static IPHostEntry InternalResolveFast(string hostName, int timeout, out bool timedOut)
		{
			timedOut = false;
			if (hostName.Length > 0 && hostName.Length <= 126)
			{
				if (TryParseAsIP(hostName, out var ip))
				{
					IPHostEntry iPHostEntry = new IPHostEntry();
					iPHostEntry.HostName = ip.ToString();
					iPHostEntry.Aliases = new string[0];
					iPHostEntry.AddressList = new IPAddress[1]
					{
						ip
					};
					return iPHostEntry;
				}
				if (Socket.OSSupportsIPv6)
				{
					try
					{
						return GetAddrInfo(hostName);
					}
					catch (Exception)
					{
					}
				}
				else
				{
					IntPtr intPtr = UnsafeNclNativeMethods.OSSOCK.gethostbyname(hostName);
					if (intPtr != IntPtr.Zero)
					{
						return NativeToHostEntry(intPtr);
					}
				}
			}
			if (Logging.On)
			{
				Logging.Exit(Logging.Sockets, "DNS", "InternalResolveFast", null);
			}
			return null;
		}

		private static void ResolveCallback(object context)
		{
			ResolveAsyncResult resolveAsyncResult = (ResolveAsyncResult)context;
			IPHostEntry result;
			try
			{
				result = ((resolveAsyncResult.address == null) ? InternalGetHostByName(resolveAsyncResult.hostName, resolveAsyncResult.includeIPv6) : InternalGetHostByAddress(resolveAsyncResult.address, resolveAsyncResult.includeIPv6, throwOnFailure: false));
			}
			catch (Exception ex)
			{
				if (ex is OutOfMemoryException || ex is ThreadAbortException || ex is StackOverflowException)
				{
					throw;
				}
				resolveAsyncResult.InvokeCallback(ex);
				return;
			}
			resolveAsyncResult.InvokeCallback(result);
		}

		private static IAsyncResult HostResolutionBeginHelper(string hostName, bool useGetHostByName, bool flowContext, bool includeIPv6, bool throwOnIPAny, AsyncCallback requestCallback, object state)
		{
			s_DnsPermission.Demand();
			if (hostName == null)
			{
				throw new ArgumentNullException("hostName");
			}
			ResolveAsyncResult resolveAsyncResult;
			if (TryParseAsIP(hostName, out var ip))
			{
				if (throwOnIPAny && (ip.Equals(IPAddress.Any) || ip.Equals(IPAddress.IPv6Any)))
				{
					throw new ArgumentException(SR.GetString("net_invalid_ip_addr"), "hostNameOrAddress");
				}
				resolveAsyncResult = new ResolveAsyncResult(ip, null, includeIPv6, state, requestCallback);
				if (useGetHostByName)
				{
					IPHostEntry iPHostEntry = new IPHostEntry();
					iPHostEntry.AddressList = new IPAddress[1]
					{
						ip
					};
					iPHostEntry.Aliases = new string[0];
					iPHostEntry.HostName = ip.ToString();
					resolveAsyncResult.StartPostingAsyncOp(lockCapture: false);
					resolveAsyncResult.InvokeCallback(iPHostEntry);
					resolveAsyncResult.FinishPostingAsyncOp();
					return resolveAsyncResult;
				}
			}
			else
			{
				resolveAsyncResult = new ResolveAsyncResult(hostName, null, includeIPv6, state, requestCallback);
			}
			if (flowContext)
			{
				resolveAsyncResult.StartPostingAsyncOp(lockCapture: false);
			}
			ThreadPool.UnsafeQueueUserWorkItem(resolveCallback, resolveAsyncResult);
			resolveAsyncResult.FinishPostingAsyncOp();
			return resolveAsyncResult;
		}

		private static IAsyncResult HostResolutionBeginHelper(IPAddress address, bool flowContext, bool includeIPv6, AsyncCallback requestCallback, object state)
		{
			s_DnsPermission.Demand();
			if (address == null)
			{
				throw new ArgumentNullException("address");
			}
			if (address.Equals(IPAddress.Any) || address.Equals(IPAddress.IPv6Any))
			{
				throw new ArgumentException(SR.GetString("net_invalid_ip_addr"), "address");
			}
			ResolveAsyncResult resolveAsyncResult = new ResolveAsyncResult(address, null, includeIPv6, state, requestCallback);
			if (flowContext)
			{
				resolveAsyncResult.StartPostingAsyncOp(lockCapture: false);
			}
			ThreadPool.UnsafeQueueUserWorkItem(resolveCallback, resolveAsyncResult);
			resolveAsyncResult.FinishPostingAsyncOp();
			return resolveAsyncResult;
		}

		private static IPHostEntry HostResolutionEndHelper(IAsyncResult asyncResult)
		{
			if (asyncResult == null)
			{
				throw new ArgumentNullException("asyncResult");
			}
			ResolveAsyncResult resolveAsyncResult = asyncResult as ResolveAsyncResult;
			if (resolveAsyncResult == null)
			{
				throw new ArgumentException(SR.GetString("net_io_invalidasyncresult"), "asyncResult");
			}
			if (resolveAsyncResult.EndCalled)
			{
				throw new InvalidOperationException(SR.GetString("net_io_invalidendcall", "EndResolve"));
			}
			resolveAsyncResult.InternalWaitForCompletion();
			resolveAsyncResult.EndCalled = true;
			Exception ex = resolveAsyncResult.Result as Exception;
			if (ex != null)
			{
				throw ex;
			}
			return (IPHostEntry)resolveAsyncResult.Result;
		}

		[Obsolete("BeginGetHostByName is obsoleted for this type, please use BeginGetHostEntry instead. http://go.microsoft.com/fwlink/?linkid=14202")]
		[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
		public static IAsyncResult BeginGetHostByName(string hostName, AsyncCallback requestCallback, object stateObject)
		{
			if (Logging.On)
			{
				Logging.Enter(Logging.Sockets, "DNS", "BeginGetHostByName", hostName);
			}
			IAsyncResult asyncResult = HostResolutionBeginHelper(hostName, useGetHostByName: true, flowContext: true, includeIPv6: false, throwOnIPAny: false, requestCallback, stateObject);
			if (Logging.On)
			{
				Logging.Exit(Logging.Sockets, "DNS", "BeginGetHostByName", asyncResult);
			}
			return asyncResult;
		}

		[Obsolete("EndGetHostByName is obsoleted for this type, please use EndGetHostEntry instead. http://go.microsoft.com/fwlink/?linkid=14202")]
		public static IPHostEntry EndGetHostByName(IAsyncResult asyncResult)
		{
			if (Logging.On)
			{
				Logging.Enter(Logging.Sockets, "DNS", "EndGetHostByName", asyncResult);
			}
			IPHostEntry iPHostEntry = HostResolutionEndHelper(asyncResult);
			if (Logging.On)
			{
				Logging.Exit(Logging.Sockets, "DNS", "EndGetHostByName", iPHostEntry);
			}
			return iPHostEntry;
		}

		public static IPHostEntry GetHostEntry(string hostNameOrAddress)
		{
			if (Logging.On)
			{
				Logging.Enter(Logging.Sockets, "DNS", "GetHostEntry", hostNameOrAddress);
			}
			s_DnsPermission.Demand();
			if (hostNameOrAddress == null)
			{
				throw new ArgumentNullException("hostNameOrAddress");
			}
			IPHostEntry iPHostEntry;
			if (TryParseAsIP(hostNameOrAddress, out var ip))
			{
				if (ip.Equals(IPAddress.Any) || ip.Equals(IPAddress.IPv6Any))
				{
					throw new ArgumentException(SR.GetString("net_invalid_ip_addr"), "hostNameOrAddress");
				}
				iPHostEntry = InternalGetHostByAddress(ip, includeIPv6: true, throwOnFailure: false);
			}
			else
			{
				iPHostEntry = InternalGetHostByName(hostNameOrAddress, includeIPv6: true);
			}
			if (Logging.On)
			{
				Logging.Exit(Logging.Sockets, "DNS", "GetHostEntry", iPHostEntry);
			}
			return iPHostEntry;
		}

		public static IPHostEntry GetHostEntry(IPAddress address)
		{
			if (Logging.On)
			{
				Logging.Enter(Logging.Sockets, "DNS", "GetHostEntry", "");
			}
			s_DnsPermission.Demand();
			if (address == null)
			{
				throw new ArgumentNullException("address");
			}
			if (address.Equals(IPAddress.Any) || address.Equals(IPAddress.IPv6Any))
			{
				throw new ArgumentException(SR.GetString("net_invalid_ip_addr"), "address");
			}
			IPHostEntry iPHostEntry = InternalGetHostByAddress(address, includeIPv6: true, throwOnFailure: false);
			if (Logging.On)
			{
				Logging.Exit(Logging.Sockets, "DNS", "GetHostEntry", iPHostEntry);
			}
			return iPHostEntry;
		}

		public static IPAddress[] GetHostAddresses(string hostNameOrAddress)
		{
			if (Logging.On)
			{
				Logging.Enter(Logging.Sockets, "DNS", "GetHostAddresses", hostNameOrAddress);
			}
			s_DnsPermission.Demand();
			if (hostNameOrAddress == null)
			{
				throw new ArgumentNullException("hostNameOrAddress");
			}
			IPAddress[] array;
			if (TryParseAsIP(hostNameOrAddress, out var ip))
			{
				if (ip.Equals(IPAddress.Any) || ip.Equals(IPAddress.IPv6Any))
				{
					throw new ArgumentException(SR.GetString("net_invalid_ip_addr"), "hostNameOrAddress");
				}
				array = new IPAddress[1]
				{
					ip
				};
			}
			else
			{
				array = InternalGetHostByName(hostNameOrAddress, includeIPv6: true).AddressList;
			}
			if (Logging.On)
			{
				Logging.Exit(Logging.Sockets, "DNS", "GetHostAddresses", array);
			}
			return array;
		}

		[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
		public static IAsyncResult BeginGetHostEntry(string hostNameOrAddress, AsyncCallback requestCallback, object stateObject)
		{
			if (Logging.On)
			{
				Logging.Enter(Logging.Sockets, "DNS", "BeginGetHostEntry", hostNameOrAddress);
			}
			IAsyncResult asyncResult = HostResolutionBeginHelper(hostNameOrAddress, useGetHostByName: false, flowContext: true, includeIPv6: true, throwOnIPAny: true, requestCallback, stateObject);
			if (Logging.On)
			{
				Logging.Exit(Logging.Sockets, "DNS", "BeginGetHostEntry", asyncResult);
			}
			return asyncResult;
		}

		[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
		public static IAsyncResult BeginGetHostEntry(IPAddress address, AsyncCallback requestCallback, object stateObject)
		{
			if (Logging.On)
			{
				Logging.Enter(Logging.Sockets, "DNS", "BeginGetHostEntry", address);
			}
			IAsyncResult asyncResult = HostResolutionBeginHelper(address, flowContext: true, includeIPv6: true, requestCallback, stateObject);
			if (Logging.On)
			{
				Logging.Exit(Logging.Sockets, "DNS", "BeginGetHostEntry", asyncResult);
			}
			return asyncResult;
		}

		public static IPHostEntry EndGetHostEntry(IAsyncResult asyncResult)
		{
			if (Logging.On)
			{
				Logging.Enter(Logging.Sockets, "DNS", "EndGetHostEntry", asyncResult);
			}
			IPHostEntry iPHostEntry = HostResolutionEndHelper(asyncResult);
			if (Logging.On)
			{
				Logging.Exit(Logging.Sockets, "DNS", "EndGetHostEntry", iPHostEntry);
			}
			return iPHostEntry;
		}

		[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
		public static IAsyncResult BeginGetHostAddresses(string hostNameOrAddress, AsyncCallback requestCallback, object state)
		{
			if (Logging.On)
			{
				Logging.Enter(Logging.Sockets, "DNS", "BeginGetHostAddresses", hostNameOrAddress);
			}
			IAsyncResult asyncResult = HostResolutionBeginHelper(hostNameOrAddress, useGetHostByName: true, flowContext: true, includeIPv6: true, throwOnIPAny: true, requestCallback, state);
			if (Logging.On)
			{
				Logging.Exit(Logging.Sockets, "DNS", "BeginGetHostAddresses", asyncResult);
			}
			return asyncResult;
		}

		public static IPAddress[] EndGetHostAddresses(IAsyncResult asyncResult)
		{
			if (Logging.On)
			{
				Logging.Enter(Logging.Sockets, "DNS", "EndGetHostAddresses", asyncResult);
			}
			IPHostEntry iPHostEntry = HostResolutionEndHelper(asyncResult);
			if (Logging.On)
			{
				Logging.Exit(Logging.Sockets, "DNS", "EndGetHostAddresses", iPHostEntry);
			}
			return iPHostEntry.AddressList;
		}

		internal static IAsyncResult UnsafeBeginGetHostAddresses(string hostName, AsyncCallback requestCallback, object state)
		{
			if (Logging.On)
			{
				Logging.Enter(Logging.Sockets, "DNS", "UnsafeBeginGetHostAddresses", hostName);
			}
			IAsyncResult asyncResult = HostResolutionBeginHelper(hostName, useGetHostByName: true, flowContext: false, includeIPv6: true, throwOnIPAny: true, requestCallback, state);
			if (Logging.On)
			{
				Logging.Exit(Logging.Sockets, "DNS", "UnsafeBeginGetHostAddresses", asyncResult);
			}
			return asyncResult;
		}

		[Obsolete("BeginResolve is obsoleted for this type, please use BeginGetHostEntry instead. http://go.microsoft.com/fwlink/?linkid=14202")]
		[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
		public static IAsyncResult BeginResolve(string hostName, AsyncCallback requestCallback, object stateObject)
		{
			if (Logging.On)
			{
				Logging.Enter(Logging.Sockets, "DNS", "BeginResolve", hostName);
			}
			IAsyncResult asyncResult = HostResolutionBeginHelper(hostName, useGetHostByName: false, flowContext: true, includeIPv6: false, throwOnIPAny: false, requestCallback, stateObject);
			if (Logging.On)
			{
				Logging.Exit(Logging.Sockets, "DNS", "BeginResolve", asyncResult);
			}
			return asyncResult;
		}

		[Obsolete("EndResolve is obsoleted for this type, please use EndGetHostEntry instead. http://go.microsoft.com/fwlink/?linkid=14202")]
		public static IPHostEntry EndResolve(IAsyncResult asyncResult)
		{
			if (Logging.On)
			{
				Logging.Enter(Logging.Sockets, "DNS", "EndResolve", asyncResult);
			}
			IPHostEntry iPHostEntry = HostResolutionEndHelper(asyncResult);
			if (Logging.On)
			{
				Logging.Exit(Logging.Sockets, "DNS", "EndResolve", iPHostEntry);
			}
			return iPHostEntry;
		}

		private unsafe static IPHostEntry GetAddrInfo(string name)
		{
			if (!ComNetOS.IsPostWin2K)
			{
				throw new SocketException(SocketError.OperationNotSupported);
			}
			SafeFreeAddrInfo outAddrInfo = null;
			ArrayList arrayList = new ArrayList();
			string text = null;
			AddressInfo hints = default(AddressInfo);
			hints.ai_flags = AddressInfoHints.AI_CANONNAME;
			hints.ai_family = AddressFamily.Unspecified;
			try
			{
				if (SafeFreeAddrInfo.GetAddrInfo(name, null, ref hints, out outAddrInfo) != 0)
				{
					throw new SocketException();
				}
				for (AddressInfo* ptr = (AddressInfo*)(void*)outAddrInfo.DangerousGetHandle(); ptr != null; ptr = ptr->ai_next)
				{
					if (text == null && ptr->ai_canonname != null)
					{
						text = new string(ptr->ai_canonname);
					}
					if ((ptr->ai_family == AddressFamily.InterNetwork && Socket.SupportsIPv4) || (ptr->ai_family == AddressFamily.InterNetworkV6 && Socket.OSSupportsIPv6))
					{
						SocketAddress socketAddress = new SocketAddress(ptr->ai_family, ptr->ai_addrlen);
						for (int i = 0; i < ptr->ai_addrlen; i++)
						{
							socketAddress.m_Buffer[i] = ptr->ai_addr[i];
						}
						if (ptr->ai_family == AddressFamily.InterNetwork)
						{
							arrayList.Add(((IPEndPoint)IPEndPoint.Any.Create(socketAddress)).Address);
						}
						else
						{
							arrayList.Add(((IPEndPoint)IPEndPoint.IPv6Any.Create(socketAddress)).Address);
						}
					}
				}
			}
			finally
			{
				outAddrInfo?.Close();
			}
			IPHostEntry iPHostEntry = new IPHostEntry();
			iPHostEntry.HostName = ((text != null) ? text : name);
			iPHostEntry.Aliases = new string[0];
			iPHostEntry.AddressList = new IPAddress[arrayList.Count];
			arrayList.CopyTo(iPHostEntry.AddressList);
			return iPHostEntry;
		}

		internal static string TryGetNameInfo(IPAddress addr, out SocketError errorCode)
		{
			if (!ComNetOS.IsPostWin2K)
			{
				throw new SocketException(SocketError.OperationNotSupported);
			}
			SocketAddress socketAddress = new IPEndPoint(addr, 0).Serialize();
			StringBuilder stringBuilder = new StringBuilder(1025);
			Socket.InitializeSockets();
			errorCode = UnsafeNclNativeMethods.OSSOCK.getnameinfo(socketAddress.m_Buffer, socketAddress.m_Size, stringBuilder, stringBuilder.Capacity, null, 0, 4);
			if (errorCode != 0)
			{
				return null;
			}
			return stringBuilder.ToString();
		}

		private static bool TryParseAsIP(string address, out IPAddress ip)
		{
			if (IPAddress.TryParse(address, out ip))
			{
				if (ip.AddressFamily != AddressFamily.InterNetwork || !Socket.SupportsIPv4)
				{
					if (ip.AddressFamily == AddressFamily.InterNetworkV6)
					{
						return Socket.OSSupportsIPv6;
					}
					return false;
				}
				return true;
			}
			return false;
		}
	}
}
