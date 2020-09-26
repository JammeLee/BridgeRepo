using System.ComponentModel;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Threading;

namespace System.Net.NetworkInformation
{
	public class Ping : Component, IDisposable
	{
		internal class AsyncStateObject
		{
			internal byte[] buffer;

			internal string hostName;

			internal int timeout;

			internal PingOptions options;

			internal object userToken;

			internal AsyncStateObject(string hostName, byte[] buffer, int timeout, PingOptions options, object userToken)
			{
				this.hostName = hostName;
				this.buffer = buffer;
				this.timeout = timeout;
				this.options = options;
				this.userToken = userToken;
			}
		}

		private const int MaxUdpPacket = 65791;

		private const int MaxBufferSize = 65500;

		private const int DefaultTimeout = 5000;

		private const int DefaultSendBufferSize = 32;

		private const int TimeoutErrorCode = 10060;

		private const int PacketTooBigErrorCode = 10040;

		private const int Free = 0;

		private const int InProgress = 1;

		private new const int Disposed = 2;

		private byte[] defaultSendBuffer;

		private bool ipv6;

		private bool cancelled;

		private bool disposeRequested;

		internal ManualResetEvent pingEvent;

		private RegisteredWaitHandle registeredWait;

		private SafeLocalFree requestBuffer;

		private SafeLocalFree replyBuffer;

		private int sendSize;

		private Socket pingSocket;

		private byte[] downlevelReplyBuffer;

		private SafeCloseIcmpHandle handlePingV4;

		private SafeCloseIcmpHandle handlePingV6;

		private int startTime;

		private IcmpPacket packet;

		private int llTimeout;

		private AsyncOperation asyncOp;

		private SendOrPostCallback onPingCompletedDelegate;

		private ManualResetEvent asyncFinished;

		private int status;

		private bool InAsyncCall
		{
			get
			{
				if (asyncFinished == null)
				{
					return false;
				}
				return !asyncFinished.WaitOne(0);
			}
			set
			{
				if (asyncFinished == null)
				{
					asyncFinished = new ManualResetEvent(!value);
				}
				else if (value)
				{
					asyncFinished.Reset();
				}
				else
				{
					asyncFinished.Set();
				}
			}
		}

		private byte[] DefaultSendBuffer
		{
			get
			{
				if (defaultSendBuffer == null)
				{
					defaultSendBuffer = new byte[32];
					for (int i = 0; i < 32; i++)
					{
						defaultSendBuffer[i] = (byte)(97 + i % 23);
					}
				}
				return defaultSendBuffer;
			}
		}

		public event PingCompletedEventHandler PingCompleted;

		private void CheckStart(bool async)
		{
			if (disposeRequested)
			{
				throw new ObjectDisposedException(GetType().FullName);
			}
			switch (Interlocked.CompareExchange(ref status, 1, 0))
			{
			case 1:
				throw new InvalidOperationException(SR.GetString("net_inasync"));
			case 2:
				throw new ObjectDisposedException(GetType().FullName);
			}
			if (async)
			{
				InAsyncCall = true;
			}
		}

		private void Finish(bool async)
		{
			status = 0;
			if (async)
			{
				InAsyncCall = false;
			}
			if (disposeRequested)
			{
				InternalDispose();
			}
		}

		protected void OnPingCompleted(PingCompletedEventArgs e)
		{
			if (this.PingCompleted != null)
			{
				this.PingCompleted(this, e);
			}
		}

		private void PingCompletedWaitCallback(object operationState)
		{
			OnPingCompleted((PingCompletedEventArgs)operationState);
		}

		public Ping()
		{
			onPingCompletedDelegate = PingCompletedWaitCallback;
		}

		private void InternalDispose()
		{
			disposeRequested = true;
			if (Interlocked.CompareExchange(ref status, 2, 0) == 0)
			{
				if (pingSocket != null)
				{
					pingSocket.Close();
					pingSocket = null;
				}
				if (handlePingV4 != null)
				{
					handlePingV4.Close();
					handlePingV4 = null;
				}
				if (handlePingV6 != null)
				{
					handlePingV6.Close();
					handlePingV6 = null;
				}
				if (registeredWait != null)
				{
					registeredWait.Unregister(null);
				}
				if (pingEvent != null)
				{
					pingEvent.Close();
				}
				if (replyBuffer != null)
				{
					replyBuffer.Close();
				}
			}
		}

		void IDisposable.Dispose()
		{
			InternalDispose();
		}

		public void SendAsyncCancel()
		{
			lock (this)
			{
				if (!InAsyncCall)
				{
					return;
				}
				cancelled = true;
				if (pingSocket != null)
				{
					pingSocket.Close();
					pingSocket = null;
				}
			}
			asyncFinished.WaitOne();
		}

		private static void PingCallback(object state, bool signaled)
		{
			Ping ping = (Ping)state;
			PingCompletedEventArgs arg = null;
			bool flag = false;
			AsyncOperation asyncOperation = null;
			SendOrPostCallback d = null;
			try
			{
				lock (ping)
				{
					flag = ping.cancelled;
					asyncOperation = ping.asyncOp;
					d = ping.onPingCompletedDelegate;
					if (!flag)
					{
						SafeLocalFree safeLocalFree = ping.replyBuffer;
						if (ping.ipv6)
						{
							UnsafeNetInfoNativeMethods.Icmp6ParseReplies(safeLocalFree.DangerousGetHandle(), 65791u);
						}
						else if (ComNetOS.IsPostWin2K)
						{
							UnsafeNetInfoNativeMethods.IcmpParseReplies(safeLocalFree.DangerousGetHandle(), 65791u);
						}
						else
						{
							UnsafeIcmpNativeMethods.IcmpParseReplies(safeLocalFree.DangerousGetHandle(), 65791u);
						}
						PingReply reply2;
						if (ping.ipv6)
						{
							Icmp6EchoReply reply = (Icmp6EchoReply)Marshal.PtrToStructure(safeLocalFree.DangerousGetHandle(), typeof(Icmp6EchoReply));
							reply2 = new PingReply(reply, safeLocalFree.DangerousGetHandle(), ping.sendSize);
						}
						else
						{
							IcmpEchoReply reply3 = (IcmpEchoReply)Marshal.PtrToStructure(safeLocalFree.DangerousGetHandle(), typeof(IcmpEchoReply));
							reply2 = new PingReply(reply3);
						}
						arg = new PingCompletedEventArgs(reply2, null, cancelled: false, asyncOperation.UserSuppliedState);
					}
				}
			}
			catch (Exception innerException)
			{
				PingException error = new PingException(SR.GetString("net_ping"), innerException);
				arg = new PingCompletedEventArgs(null, error, cancelled: false, asyncOperation.UserSuppliedState);
			}
			catch
			{
				PingException error2 = new PingException(SR.GetString("net_ping"), new Exception(SR.GetString("net_nonClsCompliantException")));
				arg = new PingCompletedEventArgs(null, error2, cancelled: false, asyncOperation.UserSuppliedState);
			}
			finally
			{
				ping.FreeUnmanagedStructures();
				ping.Finish(async: true);
			}
			if (flag)
			{
				arg = new PingCompletedEventArgs(null, null, cancelled: true, asyncOperation.UserSuppliedState);
			}
			asyncOperation.PostOperationCompleted(d, arg);
		}

		private static void PingSendCallback(IAsyncResult result)
		{
			Ping ping = (Ping)result.AsyncState;
			PingCompletedEventArgs arg = null;
			try
			{
				ping.pingSocket.EndSendTo(result);
				PingReply pingReply = null;
				if (!ping.cancelled)
				{
					EndPoint remoteEP = new IPEndPoint(0L, 0);
					int num = 0;
					while (true)
					{
						num = ping.pingSocket.ReceiveFrom(ping.downlevelReplyBuffer, ref remoteEP);
						if (CorrectPacket(ping.downlevelReplyBuffer, ping.packet))
						{
							break;
						}
						if (Environment.TickCount - ping.startTime > ping.llTimeout)
						{
							pingReply = new PingReply(IPStatus.TimedOut);
							break;
						}
					}
					int time = Environment.TickCount - ping.startTime;
					if (pingReply == null)
					{
						pingReply = new PingReply(ping.downlevelReplyBuffer, num, ((IPEndPoint)remoteEP).Address, time);
					}
					arg = new PingCompletedEventArgs(pingReply, null, cancelled: false, ping.asyncOp.UserSuppliedState);
				}
			}
			catch (Exception ex)
			{
				PingReply pingReply2 = null;
				PingException error = null;
				SocketException ex2 = ex as SocketException;
				if (ex2 != null)
				{
					if (ex2.ErrorCode == 10060)
					{
						pingReply2 = new PingReply(IPStatus.TimedOut);
					}
					else if (ex2.ErrorCode == 10040)
					{
						pingReply2 = new PingReply(IPStatus.PacketTooBig);
					}
				}
				if (pingReply2 == null)
				{
					error = new PingException(SR.GetString("net_ping"), ex);
				}
				arg = new PingCompletedEventArgs(pingReply2, error, cancelled: false, ping.asyncOp.UserSuppliedState);
			}
			catch
			{
				PingException error2 = new PingException(SR.GetString("net_ping"), new Exception(SR.GetString("net_nonClsCompliantException")));
				arg = new PingCompletedEventArgs(null, error2, cancelled: false, ping.asyncOp.UserSuppliedState);
			}
			try
			{
				if (ping.cancelled)
				{
					arg = new PingCompletedEventArgs(null, null, cancelled: true, ping.asyncOp.UserSuppliedState);
				}
				ping.asyncOp.PostOperationCompleted(ping.onPingCompletedDelegate, arg);
			}
			finally
			{
				ping.Finish(async: true);
			}
		}

		public PingReply Send(string hostNameOrAddress)
		{
			return Send(hostNameOrAddress, 5000, DefaultSendBuffer, null);
		}

		public PingReply Send(string hostNameOrAddress, int timeout)
		{
			return Send(hostNameOrAddress, timeout, DefaultSendBuffer, null);
		}

		public PingReply Send(IPAddress address)
		{
			return Send(address, 5000, DefaultSendBuffer, null);
		}

		public PingReply Send(IPAddress address, int timeout)
		{
			return Send(address, timeout, DefaultSendBuffer, null);
		}

		public PingReply Send(string hostNameOrAddress, int timeout, byte[] buffer)
		{
			return Send(hostNameOrAddress, timeout, buffer, null);
		}

		public PingReply Send(IPAddress address, int timeout, byte[] buffer)
		{
			return Send(address, timeout, buffer, null);
		}

		public PingReply Send(string hostNameOrAddress, int timeout, byte[] buffer, PingOptions options)
		{
			if (ValidationHelper.IsBlankString(hostNameOrAddress))
			{
				throw new ArgumentNullException("hostNameOrAddress");
			}
			IPAddress address;
			try
			{
				address = Dns.GetHostAddresses(hostNameOrAddress)[0];
			}
			catch (ArgumentException)
			{
				throw;
			}
			catch (Exception innerException)
			{
				throw new PingException(SR.GetString("net_ping"), innerException);
			}
			return Send(address, timeout, buffer, options);
		}

		public PingReply Send(IPAddress address, int timeout, byte[] buffer, PingOptions options)
		{
			if (buffer == null)
			{
				throw new ArgumentNullException("buffer");
			}
			if (buffer.Length > 65500)
			{
				throw new ArgumentException(SR.GetString("net_invalidPingBufferSize"), "buffer");
			}
			if (timeout < 0)
			{
				throw new ArgumentOutOfRangeException("timeout");
			}
			if (address == null)
			{
				throw new ArgumentNullException("address");
			}
			if (address.Equals(IPAddress.Any) || address.Equals(IPAddress.IPv6Any))
			{
				throw new ArgumentException(SR.GetString("net_invalid_ip_addr"), "address");
			}
			IPAddress address2 = ((address.AddressFamily != AddressFamily.InterNetwork) ? new IPAddress(address.GetAddressBytes(), address.ScopeId) : new IPAddress(address.GetAddressBytes()));
			new NetworkInformationPermission(NetworkInformationAccess.Ping).Demand();
			CheckStart(async: false);
			try
			{
				return InternalSend(address2, buffer, timeout, options, async: false);
			}
			catch (Exception innerException)
			{
				throw new PingException(SR.GetString("net_ping"), innerException);
			}
			catch
			{
				throw new PingException(SR.GetString("net_ping"), new Exception(SR.GetString("net_nonClsCompliantException")));
			}
			finally
			{
				Finish(async: false);
			}
		}

		[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
		public void SendAsync(string hostNameOrAddress, object userToken)
		{
			SendAsync(hostNameOrAddress, 5000, DefaultSendBuffer, userToken);
		}

		[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
		public void SendAsync(string hostNameOrAddress, int timeout, object userToken)
		{
			SendAsync(hostNameOrAddress, timeout, DefaultSendBuffer, userToken);
		}

		[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
		public void SendAsync(IPAddress address, object userToken)
		{
			SendAsync(address, 5000, DefaultSendBuffer, userToken);
		}

		[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
		public void SendAsync(IPAddress address, int timeout, object userToken)
		{
			SendAsync(address, timeout, DefaultSendBuffer, userToken);
		}

		[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
		public void SendAsync(string hostNameOrAddress, int timeout, byte[] buffer, object userToken)
		{
			SendAsync(hostNameOrAddress, timeout, buffer, null, userToken);
		}

		[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
		public void SendAsync(IPAddress address, int timeout, byte[] buffer, object userToken)
		{
			SendAsync(address, timeout, buffer, null, userToken);
		}

		[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
		public void SendAsync(string hostNameOrAddress, int timeout, byte[] buffer, PingOptions options, object userToken)
		{
			if (ValidationHelper.IsBlankString(hostNameOrAddress))
			{
				throw new ArgumentNullException("hostNameOrAddress");
			}
			if (buffer == null)
			{
				throw new ArgumentNullException("buffer");
			}
			if (buffer.Length > 65500)
			{
				throw new ArgumentException(SR.GetString("net_invalidPingBufferSize"), "buffer");
			}
			if (timeout < 0)
			{
				throw new ArgumentOutOfRangeException("timeout");
			}
			if (IPAddress.TryParse(hostNameOrAddress, out var address))
			{
				SendAsync(address, timeout, buffer, options, userToken);
				return;
			}
			CheckStart(async: true);
			try
			{
				asyncOp = AsyncOperationManager.CreateOperation(userToken);
				AsyncStateObject state = new AsyncStateObject(hostNameOrAddress, buffer, timeout, options, userToken);
				ThreadPool.QueueUserWorkItem(ContinueAsyncSend, state);
			}
			catch (Exception innerException)
			{
				Finish(async: true);
				throw new PingException(SR.GetString("net_ping"), innerException);
			}
		}

		[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
		public void SendAsync(IPAddress address, int timeout, byte[] buffer, PingOptions options, object userToken)
		{
			if (buffer == null)
			{
				throw new ArgumentNullException("buffer");
			}
			if (buffer.Length > 65500)
			{
				throw new ArgumentException(SR.GetString("net_invalidPingBufferSize"), "buffer");
			}
			if (timeout < 0)
			{
				throw new ArgumentOutOfRangeException("timeout");
			}
			if (address == null)
			{
				throw new ArgumentNullException("address");
			}
			if (address.Equals(IPAddress.Any) || address.Equals(IPAddress.IPv6Any))
			{
				throw new ArgumentException(SR.GetString("net_invalid_ip_addr"), "address");
			}
			IPAddress address2 = ((address.AddressFamily != AddressFamily.InterNetwork) ? new IPAddress(address.GetAddressBytes(), address.ScopeId) : new IPAddress(address.GetAddressBytes()));
			new NetworkInformationPermission(NetworkInformationAccess.Ping).Demand();
			CheckStart(async: true);
			try
			{
				asyncOp = AsyncOperationManager.CreateOperation(userToken);
				InternalSend(address2, buffer, timeout, options, async: true);
			}
			catch (Exception innerException)
			{
				Finish(async: true);
				throw new PingException(SR.GetString("net_ping"), innerException);
			}
			catch
			{
				Finish(async: true);
				throw new PingException(SR.GetString("net_ping"), new Exception(SR.GetString("net_nonClsCompliantException")));
			}
		}

		private void ContinueAsyncSend(object state)
		{
			AsyncStateObject asyncStateObject = (AsyncStateObject)state;
			try
			{
				IPAddress address = Dns.GetHostAddresses(asyncStateObject.hostName)[0];
				new NetworkInformationPermission(NetworkInformationAccess.Ping).Demand();
				InternalSend(address, asyncStateObject.buffer, asyncStateObject.timeout, asyncStateObject.options, async: true);
			}
			catch (Exception innerException)
			{
				PingException error = new PingException(SR.GetString("net_ping"), innerException);
				PingCompletedEventArgs arg = new PingCompletedEventArgs(null, error, cancelled: false, asyncOp.UserSuppliedState);
				Finish(async: true);
				asyncOp.PostOperationCompleted(onPingCompletedDelegate, arg);
			}
			catch
			{
				PingException error2 = new PingException(SR.GetString("net_ping"), new Exception(SR.GetString("net_nonClsCompliantException")));
				PingCompletedEventArgs arg2 = new PingCompletedEventArgs(null, error2, cancelled: false, asyncOp.UserSuppliedState);
				Finish(async: true);
				asyncOp.PostOperationCompleted(onPingCompletedDelegate, arg2);
			}
		}

		private PingReply InternalSend(IPAddress address, byte[] buffer, int timeout, PingOptions options, bool async)
		{
			cancelled = false;
			if (address.AddressFamily == AddressFamily.InterNetworkV6 && !ComNetOS.IsPostWin2K)
			{
				throw new PlatformNotSupportedException(SR.GetString("WinXPRequired"));
			}
			if (!ComNetOS.IsWin2K)
			{
				return InternalDownLevelSend(address, buffer, timeout, options, async);
			}
			ipv6 = ((address.AddressFamily == AddressFamily.InterNetworkV6) ? true : false);
			sendSize = buffer.Length;
			if (!ipv6 && handlePingV4 == null)
			{
				if (ComNetOS.IsPostWin2K)
				{
					handlePingV4 = UnsafeNetInfoNativeMethods.IcmpCreateFile();
				}
				else
				{
					handlePingV4 = UnsafeIcmpNativeMethods.IcmpCreateFile();
				}
			}
			else if (ipv6 && handlePingV6 == null)
			{
				handlePingV6 = UnsafeNetInfoNativeMethods.Icmp6CreateFile();
			}
			IPOptions options2 = new IPOptions(options);
			if (replyBuffer == null)
			{
				replyBuffer = SafeLocalFree.LocalAlloc(65791);
			}
			if (registeredWait != null)
			{
				registeredWait.Unregister(null);
				registeredWait = null;
			}
			int num;
			try
			{
				if (async)
				{
					if (pingEvent == null)
					{
						pingEvent = new ManualResetEvent(initialState: false);
					}
					else
					{
						pingEvent.Reset();
					}
					registeredWait = ThreadPool.RegisterWaitForSingleObject(pingEvent, PingCallback, this, -1, executeOnlyOnce: true);
				}
				SetUnmanagedStructures(buffer);
				if (!ipv6)
				{
					num = (int)(ComNetOS.IsPostWin2K ? ((!async) ? UnsafeNetInfoNativeMethods.IcmpSendEcho2(handlePingV4, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, (uint)address.m_Address, requestBuffer, (ushort)buffer.Length, ref options2, replyBuffer, 65791u, (uint)timeout) : UnsafeNetInfoNativeMethods.IcmpSendEcho2(handlePingV4, pingEvent.SafeWaitHandle, IntPtr.Zero, IntPtr.Zero, (uint)address.m_Address, requestBuffer, (ushort)buffer.Length, ref options2, replyBuffer, 65791u, (uint)timeout)) : ((!async) ? UnsafeIcmpNativeMethods.IcmpSendEcho2(handlePingV4, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, (uint)address.m_Address, requestBuffer, (ushort)buffer.Length, ref options2, replyBuffer, 65791u, (uint)timeout) : UnsafeIcmpNativeMethods.IcmpSendEcho2(handlePingV4, pingEvent.SafeWaitHandle, IntPtr.Zero, IntPtr.Zero, (uint)address.m_Address, requestBuffer, (ushort)buffer.Length, ref options2, replyBuffer, 65791u, (uint)timeout)));
				}
				else
				{
					IPEndPoint iPEndPoint = new IPEndPoint(address, 0);
					SocketAddress socketAddress = iPEndPoint.Serialize();
					byte[] sourceSocketAddress = new byte[28];
					num = (int)((!async) ? UnsafeNetInfoNativeMethods.Icmp6SendEcho2(handlePingV6, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, sourceSocketAddress, socketAddress.m_Buffer, requestBuffer, (ushort)buffer.Length, ref options2, replyBuffer, 65791u, (uint)timeout) : UnsafeNetInfoNativeMethods.Icmp6SendEcho2(handlePingV6, pingEvent.SafeWaitHandle, IntPtr.Zero, IntPtr.Zero, sourceSocketAddress, socketAddress.m_Buffer, requestBuffer, (ushort)buffer.Length, ref options2, replyBuffer, 65791u, (uint)timeout));
				}
			}
			catch
			{
				if (registeredWait != null)
				{
					registeredWait.Unregister(null);
				}
				throw;
			}
			if (num == 0)
			{
				num = Marshal.GetLastWin32Error();
				if (num != 0)
				{
					FreeUnmanagedStructures();
					return new PingReply((IPStatus)num);
				}
			}
			if (async)
			{
				return null;
			}
			FreeUnmanagedStructures();
			PingReply result;
			if (ipv6)
			{
				Icmp6EchoReply reply = (Icmp6EchoReply)Marshal.PtrToStructure(replyBuffer.DangerousGetHandle(), typeof(Icmp6EchoReply));
				result = new PingReply(reply, replyBuffer.DangerousGetHandle(), sendSize);
			}
			else
			{
				IcmpEchoReply reply2 = (IcmpEchoReply)Marshal.PtrToStructure(replyBuffer.DangerousGetHandle(), typeof(IcmpEchoReply));
				result = new PingReply(reply2);
			}
			GC.KeepAlive(replyBuffer);
			return result;
		}

		private PingReply InternalDownLevelSend(IPAddress address, byte[] buffer, int timeout, PingOptions options, bool async)
		{
			try
			{
				if (options == null)
				{
					options = new PingOptions();
				}
				if (downlevelReplyBuffer == null)
				{
					downlevelReplyBuffer = new byte[64000];
				}
				llTimeout = timeout;
				packet = new IcmpPacket(buffer);
				byte[] bytes = packet.GetBytes();
				IPEndPoint remoteEP = new IPEndPoint(address, 0);
				EndPoint remoteEP2 = new IPEndPoint(IPAddress.Any, 0);
				if (pingSocket == null)
				{
					pingSocket = new Socket(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.Icmp);
				}
				pingSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, timeout);
				pingSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, timeout);
				pingSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.ReuseAddress, options.Ttl);
				pingSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.DontFragment, options.DontFragment);
				int num = 0;
				int num2 = 0;
				startTime = Environment.TickCount;
				if (async)
				{
					pingSocket.BeginSendTo(bytes, 0, bytes.Length, SocketFlags.None, remoteEP, PingSendCallback, this);
					return null;
				}
				pingSocket.SendTo(bytes, bytes.Length, SocketFlags.None, remoteEP);
				while (true)
				{
					num = pingSocket.ReceiveFrom(downlevelReplyBuffer, ref remoteEP2);
					if (CorrectPacket(downlevelReplyBuffer, packet))
					{
						break;
					}
					if (Environment.TickCount - startTime > llTimeout)
					{
						return new PingReply(IPStatus.TimedOut);
					}
				}
				num2 = Environment.TickCount - startTime;
				return new PingReply(downlevelReplyBuffer, num, ((IPEndPoint)remoteEP2).Address, num2);
			}
			catch (SocketException ex)
			{
				if (ex.ErrorCode == 10060)
				{
					return new PingReply(IPStatus.TimedOut);
				}
				if (ex.ErrorCode == 10040)
				{
					PingReply pingReply = new PingReply(IPStatus.PacketTooBig);
					if (!async)
					{
						return pingReply;
					}
					PingCompletedEventArgs arg = new PingCompletedEventArgs(pingReply, null, cancelled: false, asyncOp.UserSuppliedState);
					asyncOp.PostOperationCompleted(onPingCompletedDelegate, arg);
					return null;
				}
				throw ex;
			}
		}

		private unsafe void SetUnmanagedStructures(byte[] buffer)
		{
			requestBuffer = SafeLocalFree.LocalAlloc(buffer.Length);
			byte* ptr = (byte*)(void*)requestBuffer.DangerousGetHandle();
			for (int i = 0; i < buffer.Length; i++)
			{
				ptr[i] = buffer[i];
			}
		}

		private void FreeUnmanagedStructures()
		{
			if (requestBuffer != null)
			{
				requestBuffer.Close();
				requestBuffer = null;
			}
		}

		internal static bool CorrectPacket(byte[] buffer, IcmpPacket packet)
		{
			if (buffer[20] == 0 && buffer[21] == 0)
			{
				if (((buffer[25] << 8) | buffer[24]) == packet.Identifier && ((buffer[27] << 8) | buffer[26]) == packet.sequenceNumber)
				{
					return true;
				}
			}
			else if (((buffer[53] << 8) | buffer[52]) == packet.Identifier && ((buffer[55] << 8) | buffer[54]) == packet.sequenceNumber)
			{
				return true;
			}
			return false;
		}
	}
}
