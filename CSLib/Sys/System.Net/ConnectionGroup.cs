using System.Collections;
using System.Diagnostics;
using System.Threading;

namespace System.Net
{
	internal class ConnectionGroup
	{
		private const int DefaultConnectionListSize = 3;

		private ServicePoint m_ServicePoint;

		private string m_Name;

		private int m_ConnectionLimit;

		private ArrayList m_ConnectionList;

		private object m_Event;

		private Queue m_AuthenticationRequestQueue;

		internal bool m_AuthenticationGroup;

		private HttpAbortDelegate m_AbortDelegate;

		private bool m_NtlmNegGroup;

		private int m_IISVersion = -1;

		internal ServicePoint ServicePoint => m_ServicePoint;

		internal int CurrentConnections => m_ConnectionList.Count;

		internal int ConnectionLimit
		{
			get
			{
				return m_ConnectionLimit;
			}
			set
			{
				m_ConnectionLimit = value;
				PruneExcesiveConnections();
			}
		}

		private ManualResetEvent AsyncWaitHandle
		{
			get
			{
				if (m_Event == null)
				{
					Interlocked.CompareExchange(ref m_Event, new ManualResetEvent(initialState: false), null);
				}
				return (ManualResetEvent)m_Event;
			}
		}

		private Queue AuthenticationRequestQueue
		{
			get
			{
				if (m_AuthenticationRequestQueue == null)
				{
					lock (m_ConnectionList)
					{
						if (m_AuthenticationRequestQueue == null)
						{
							m_AuthenticationRequestQueue = new Queue();
						}
					}
				}
				return m_AuthenticationRequestQueue;
			}
			set
			{
				m_AuthenticationRequestQueue = value;
			}
		}

		internal ConnectionGroup(ServicePoint servicePoint, string connName)
		{
			m_ServicePoint = servicePoint;
			m_ConnectionLimit = servicePoint.ConnectionLimit;
			m_ConnectionList = new ArrayList(3);
			m_Name = MakeQueryStr(connName);
			m_AbortDelegate = Abort;
		}

		internal static string MakeQueryStr(string connName)
		{
			if (connName != null)
			{
				return connName;
			}
			return "";
		}

		internal void Associate(Connection connection)
		{
			lock (m_ConnectionList)
			{
				m_ConnectionList.Add(connection);
			}
		}

		internal void Disassociate(Connection connection)
		{
			lock (m_ConnectionList)
			{
				m_ConnectionList.Remove(connection);
			}
		}

		internal void ConnectionGoneIdle()
		{
			if (m_AuthenticationGroup)
			{
				lock (m_ConnectionList)
				{
					AsyncWaitHandle.Set();
				}
			}
		}

		private bool Abort(HttpWebRequest request, WebException webException)
		{
			lock (m_ConnectionList)
			{
				AsyncWaitHandle.Set();
			}
			return true;
		}

		private void PruneAbortedRequests()
		{
			lock (m_ConnectionList)
			{
				Queue queue = new Queue();
				foreach (HttpWebRequest item in AuthenticationRequestQueue)
				{
					if (!item.Aborted)
					{
						queue.Enqueue(item);
					}
				}
				AuthenticationRequestQueue = queue;
			}
		}

		private void PruneExcesiveConnections()
		{
			ArrayList arrayList = new ArrayList();
			lock (m_ConnectionList)
			{
				int connectionLimit = ConnectionLimit;
				if (CurrentConnections > connectionLimit)
				{
					int num = CurrentConnections - connectionLimit;
					for (int i = 0; i < num; i++)
					{
						arrayList.Add(m_ConnectionList[i]);
					}
					m_ConnectionList.RemoveRange(0, num);
				}
			}
			foreach (Connection item in arrayList)
			{
				item.CloseOnIdle();
			}
		}

		internal void DisableKeepAliveOnConnections()
		{
			ArrayList arrayList = new ArrayList();
			lock (m_ConnectionList)
			{
				foreach (Connection connection2 in m_ConnectionList)
				{
					arrayList.Add(connection2);
				}
				m_ConnectionList.Clear();
			}
			foreach (Connection item in arrayList)
			{
				item.CloseOnIdle();
			}
		}

		private Connection FindMatchingConnection(HttpWebRequest request, string connName, out Connection leastbusyConnection)
		{
			int num = int.MaxValue;
			bool flag = false;
			leastbusyConnection = null;
			lock (m_ConnectionList)
			{
				num = int.MaxValue;
				foreach (Connection connection in m_ConnectionList)
				{
					if (connection.LockedRequest == request)
					{
						leastbusyConnection = connection;
						return connection;
					}
					if (connection.BusyCount < num && connection.LockedRequest == null)
					{
						leastbusyConnection = connection;
						num = connection.BusyCount;
						if (num == 0)
						{
							flag = true;
						}
					}
				}
				if (!flag && CurrentConnections < ConnectionLimit)
				{
					leastbusyConnection = new Connection(this);
				}
			}
			return null;
		}

		private Connection FindConnectionAuthenticationGroup(HttpWebRequest request, string connName)
		{
			Connection leastbusyConnection = null;
			lock (m_ConnectionList)
			{
				Connection connection = FindMatchingConnection(request, connName, out leastbusyConnection);
				if (connection != null)
				{
					connection.MarkAsReserved();
					return connection;
				}
				if (AuthenticationRequestQueue.Count == 0)
				{
					if (leastbusyConnection != null)
					{
						if (request.LockConnection)
						{
							m_NtlmNegGroup = true;
							m_IISVersion = leastbusyConnection.IISVersion;
						}
						if (request.LockConnection || (m_NtlmNegGroup && !request.Pipelined && request.UnsafeOrProxyAuthenticatedConnectionSharing && m_IISVersion >= 6))
						{
							leastbusyConnection.LockedRequest = request;
						}
						leastbusyConnection.MarkAsReserved();
						return leastbusyConnection;
					}
				}
				else if (leastbusyConnection != null)
				{
					AsyncWaitHandle.Set();
				}
				AuthenticationRequestQueue.Enqueue(request);
			}
			while (true)
			{
				request.AbortDelegate = m_AbortDelegate;
				if (!request.Aborted)
				{
					AsyncWaitHandle.WaitOne();
				}
				lock (m_ConnectionList)
				{
					if (request.Aborted)
					{
						PruneAbortedRequests();
						return null;
					}
					FindMatchingConnection(request, connName, out leastbusyConnection);
					if (AuthenticationRequestQueue.Peek() == request)
					{
						AuthenticationRequestQueue.Dequeue();
						if (leastbusyConnection != null)
						{
							if (request.LockConnection)
							{
								m_NtlmNegGroup = true;
								m_IISVersion = leastbusyConnection.IISVersion;
							}
							if (request.LockConnection || (m_NtlmNegGroup && !request.Pipelined && request.UnsafeOrProxyAuthenticatedConnectionSharing && m_IISVersion >= 6))
							{
								leastbusyConnection.LockedRequest = request;
							}
							leastbusyConnection.MarkAsReserved();
							return leastbusyConnection;
						}
						AuthenticationRequestQueue.Enqueue(request);
					}
					if (leastbusyConnection == null)
					{
						AsyncWaitHandle.Reset();
					}
				}
			}
		}

		internal Connection FindConnection(HttpWebRequest request, string connName)
		{
			Connection connection = null;
			Connection connection2 = null;
			bool flag = false;
			if (m_AuthenticationGroup || request.LockConnection)
			{
				m_AuthenticationGroup = true;
				return FindConnectionAuthenticationGroup(request, connName);
			}
			lock (m_ConnectionList)
			{
				int num = int.MaxValue;
				foreach (Connection connection3 in m_ConnectionList)
				{
					if (connection3.BusyCount < num)
					{
						connection = connection3;
						num = connection3.BusyCount;
						if (num == 0)
						{
							flag = true;
							break;
						}
					}
				}
				connection2 = ((flag || CurrentConnections >= ConnectionLimit) ? connection : new Connection(this));
				connection2.MarkAsReserved();
				return connection2;
			}
		}

		[Conditional("DEBUG")]
		internal void Debug(int requestHash)
		{
			foreach (Connection connection in m_ConnectionList)
			{
				_ = connection;
			}
		}
	}
}
