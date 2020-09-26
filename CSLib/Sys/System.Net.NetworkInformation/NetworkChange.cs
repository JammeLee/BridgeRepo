using System.Collections;
using System.Collections.Specialized;
using System.Net.Sockets;
using System.Threading;

namespace System.Net.NetworkInformation
{
	public sealed class NetworkChange
	{
		internal static class AvailabilityChangeListener
		{
			private static object syncObject = new object();

			private static ListDictionary s_availabilityCallerArray = null;

			private static NetworkAddressChangedEventHandler addressChange = null;

			private static bool isAvailable = false;

			private static ContextCallback s_RunHandlerCallback = RunHandlerCallback;

			private static void RunHandlerCallback(object state)
			{
				((NetworkAvailabilityChangedEventHandler)state)(null, new NetworkAvailabilityEventArgs(isAvailable));
			}

			private static void ChangedAddress(object sender, EventArgs eventArgs)
			{
				lock (syncObject)
				{
					bool flag = SystemNetworkInterface.InternalGetIsNetworkAvailable();
					if (flag == isAvailable)
					{
						return;
					}
					isAvailable = flag;
					DictionaryEntry[] array = new DictionaryEntry[s_availabilityCallerArray.Count];
					s_availabilityCallerArray.CopyTo(array, 0);
					for (int i = 0; i < array.Length; i++)
					{
						NetworkAvailabilityChangedEventHandler networkAvailabilityChangedEventHandler = (NetworkAvailabilityChangedEventHandler)array[i].Key;
						ExecutionContext executionContext = (ExecutionContext)array[i].Value;
						if (executionContext == null)
						{
							networkAvailabilityChangedEventHandler(null, new NetworkAvailabilityEventArgs(isAvailable));
						}
						else
						{
							ExecutionContext.Run(executionContext.CreateCopy(), s_RunHandlerCallback, networkAvailabilityChangedEventHandler);
						}
					}
				}
			}

			internal static void Start(NetworkAvailabilityChangedEventHandler caller)
			{
				lock (syncObject)
				{
					if (s_availabilityCallerArray == null)
					{
						s_availabilityCallerArray = new ListDictionary();
						addressChange = ChangedAddress;
					}
					if (s_availabilityCallerArray.Count == 0)
					{
						isAvailable = NetworkInterface.GetIsNetworkAvailable();
						AddressChangeListener.UnsafeStart(addressChange);
					}
					if (caller != null && !s_availabilityCallerArray.Contains(caller))
					{
						s_availabilityCallerArray.Add(caller, ExecutionContext.Capture());
					}
				}
			}

			internal static void Stop(NetworkAvailabilityChangedEventHandler caller)
			{
				lock (syncObject)
				{
					s_availabilityCallerArray.Remove(caller);
					if (s_availabilityCallerArray.Count == 0)
					{
						AddressChangeListener.Stop(addressChange);
					}
				}
			}
		}

		internal static class AddressChangeListener
		{
			private static ListDictionary s_callerArray = new ListDictionary();

			private static ContextCallback s_runHandlerCallback = RunHandlerCallback;

			private static RegisteredWaitHandle s_registeredWait;

			private static bool s_isListening = false;

			private static bool s_isPending = false;

			private static SafeCloseSocketAndEvent s_ipv4Socket = null;

			private static SafeCloseSocketAndEvent s_ipv6Socket = null;

			private static WaitHandle s_ipv4WaitHandle = null;

			private static WaitHandle s_ipv6WaitHandle = null;

			private static void AddressChangedCallback(object stateObject, bool signaled)
			{
				lock (s_callerArray)
				{
					s_isPending = false;
					if (!s_isListening)
					{
						return;
					}
					s_isListening = false;
					DictionaryEntry[] array = new DictionaryEntry[s_callerArray.Count];
					s_callerArray.CopyTo(array, 0);
					StartHelper(null, captureContext: false, (StartIPOptions)stateObject);
					for (int i = 0; i < array.Length; i++)
					{
						NetworkAddressChangedEventHandler networkAddressChangedEventHandler = (NetworkAddressChangedEventHandler)array[i].Key;
						ExecutionContext executionContext = (ExecutionContext)array[i].Value;
						if (executionContext == null)
						{
							networkAddressChangedEventHandler(null, EventArgs.Empty);
						}
						else
						{
							ExecutionContext.Run(executionContext.CreateCopy(), s_runHandlerCallback, networkAddressChangedEventHandler);
						}
					}
				}
			}

			private static void RunHandlerCallback(object state)
			{
				((NetworkAddressChangedEventHandler)state)(null, EventArgs.Empty);
			}

			internal static void Start(NetworkAddressChangedEventHandler caller)
			{
				StartHelper(caller, captureContext: true, StartIPOptions.Both);
			}

			internal static void UnsafeStart(NetworkAddressChangedEventHandler caller)
			{
				StartHelper(caller, captureContext: false, StartIPOptions.Both);
			}

			private static void StartHelper(NetworkAddressChangedEventHandler caller, bool captureContext, StartIPOptions startIPOptions)
			{
				lock (s_callerArray)
				{
					if (s_ipv4Socket == null)
					{
						Socket.InitializeSockets();
						if (Socket.SupportsIPv4)
						{
							int argp = -1;
							s_ipv4Socket = SafeCloseSocketAndEvent.CreateWSASocketWithEvent(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.IP, autoReset: true, signaled: false);
							UnsafeNclNativeMethods.OSSOCK.ioctlsocket(s_ipv4Socket, -2147195266, ref argp);
							s_ipv4WaitHandle = s_ipv4Socket.GetEventHandle();
						}
						if (Socket.OSSupportsIPv6)
						{
							int argp = -1;
							s_ipv6Socket = SafeCloseSocketAndEvent.CreateWSASocketWithEvent(AddressFamily.InterNetworkV6, SocketType.Dgram, ProtocolType.IP, autoReset: true, signaled: false);
							UnsafeNclNativeMethods.OSSOCK.ioctlsocket(s_ipv6Socket, -2147195266, ref argp);
							s_ipv6WaitHandle = s_ipv6Socket.GetEventHandle();
						}
					}
					if (caller != null && !s_callerArray.Contains(caller))
					{
						s_callerArray.Add(caller, captureContext ? ExecutionContext.Capture() : null);
					}
					if (s_isListening || s_callerArray.Count == 0)
					{
						return;
					}
					if (!s_isPending)
					{
						int bytesTransferred;
						if (Socket.SupportsIPv4 && (startIPOptions & StartIPOptions.StartIPv4) != 0)
						{
							s_registeredWait = ThreadPool.UnsafeRegisterWaitForSingleObject(s_ipv4WaitHandle, AddressChangedCallback, StartIPOptions.StartIPv4, -1, executeOnlyOnce: true);
							if (UnsafeNclNativeMethods.OSSOCK.WSAIoctl_Blocking(s_ipv4Socket.DangerousGetHandle(), 671088663, null, 0, null, 0, out bytesTransferred, SafeNativeOverlapped.Zero, IntPtr.Zero) != 0)
							{
								NetworkInformationException ex = new NetworkInformationException();
								if ((long)ex.ErrorCode != 10035)
								{
									throw ex;
								}
							}
							if (UnsafeNclNativeMethods.OSSOCK.WSAEventSelect(s_ipv4Socket, s_ipv4Socket.GetEventHandle().SafeWaitHandle, AsyncEventBits.FdAddressListChange) != 0)
							{
								throw new NetworkInformationException();
							}
						}
						if (Socket.OSSupportsIPv6 && (startIPOptions & StartIPOptions.StartIPv6) != 0)
						{
							s_registeredWait = ThreadPool.UnsafeRegisterWaitForSingleObject(s_ipv6WaitHandle, AddressChangedCallback, StartIPOptions.StartIPv6, -1, executeOnlyOnce: true);
							if (UnsafeNclNativeMethods.OSSOCK.WSAIoctl_Blocking(s_ipv6Socket.DangerousGetHandle(), 671088663, null, 0, null, 0, out bytesTransferred, SafeNativeOverlapped.Zero, IntPtr.Zero) != 0)
							{
								NetworkInformationException ex2 = new NetworkInformationException();
								if ((long)ex2.ErrorCode != 10035)
								{
									throw ex2;
								}
							}
							if (UnsafeNclNativeMethods.OSSOCK.WSAEventSelect(s_ipv6Socket, s_ipv6Socket.GetEventHandle().SafeWaitHandle, AsyncEventBits.FdAddressListChange) != 0)
							{
								throw new NetworkInformationException();
							}
						}
					}
					s_isListening = true;
					s_isPending = true;
				}
			}

			internal static void Stop(object caller)
			{
				lock (s_callerArray)
				{
					s_callerArray.Remove(caller);
					if (s_callerArray.Count == 0 && s_isListening)
					{
						s_isListening = false;
					}
				}
			}
		}

		internal static bool CanListenForNetworkChanges
		{
			get
			{
				if (!ComNetOS.IsWin2K)
				{
					return false;
				}
				return true;
			}
		}

		public static event NetworkAvailabilityChangedEventHandler NetworkAvailabilityChanged
		{
			add
			{
				if (!ComNetOS.IsWin2K)
				{
					throw new PlatformNotSupportedException(SR.GetString("Win2000Required"));
				}
				AvailabilityChangeListener.Start(value);
			}
			remove
			{
				AvailabilityChangeListener.Stop(value);
			}
		}

		public static event NetworkAddressChangedEventHandler NetworkAddressChanged
		{
			add
			{
				if (!ComNetOS.IsWin2K)
				{
					throw new PlatformNotSupportedException(SR.GetString("Win2000Required"));
				}
				AddressChangeListener.Start(value);
			}
			remove
			{
				AddressChangeListener.Stop(value);
			}
		}

		private NetworkChange()
		{
		}
	}
}
