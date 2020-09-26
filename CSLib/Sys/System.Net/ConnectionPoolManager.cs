using System.Collections;
using System.Globalization;
using System.Threading;

namespace System.Net
{
	internal class ConnectionPoolManager
	{
		private static Hashtable m_ConnectionPools = new Hashtable();

		private static object s_InternalSyncObject;

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

		private ConnectionPoolManager()
		{
		}

		private static string GenerateKey(string hostName, int port, string groupName)
		{
			return hostName + "\r" + port.ToString(NumberFormatInfo.InvariantInfo) + "\r" + groupName;
		}

		internal static ConnectionPool GetConnectionPool(ServicePoint servicePoint, string groupName, CreateConnectionDelegate createConnectionCallback)
		{
			string key = GenerateKey(servicePoint.Host, servicePoint.Port, groupName);
			lock (InternalSyncObject)
			{
				ConnectionPool connectionPool = (ConnectionPool)m_ConnectionPools[key];
				if (connectionPool == null)
				{
					connectionPool = new ConnectionPool(servicePoint, servicePoint.ConnectionLimit, 0, servicePoint.MaxIdleTime, createConnectionCallback);
					m_ConnectionPools[key] = connectionPool;
				}
				return connectionPool;
			}
		}

		internal static bool RemoveConnectionPool(ServicePoint servicePoint, string groupName)
		{
			string key = GenerateKey(servicePoint.Host, servicePoint.Port, groupName);
			lock (InternalSyncObject)
			{
				ConnectionPool connectionPool = (ConnectionPool)m_ConnectionPools[key];
				if (connectionPool != null)
				{
					m_ConnectionPools[key] = null;
					m_ConnectionPools.Remove(key);
					return true;
				}
			}
			return false;
		}
	}
}
