using System.Collections;

namespace System.Runtime.Remoting.Messaging
{
	internal class IllogicalCallContext : ICloneable
	{
		private Hashtable m_Datastore;

		private object m_HostContext;

		private Hashtable Datastore
		{
			get
			{
				if (m_Datastore == null)
				{
					m_Datastore = new Hashtable();
				}
				return m_Datastore;
			}
		}

		internal object HostContext
		{
			get
			{
				return m_HostContext;
			}
			set
			{
				m_HostContext = value;
			}
		}

		internal bool HasUserData
		{
			get
			{
				if (m_Datastore != null)
				{
					return m_Datastore.Count > 0;
				}
				return false;
			}
		}

		public void FreeNamedDataSlot(string name)
		{
			Datastore.Remove(name);
		}

		public object GetData(string name)
		{
			return Datastore[name];
		}

		public void SetData(string name, object data)
		{
			Datastore[name] = data;
		}

		public object Clone()
		{
			IllogicalCallContext illogicalCallContext = new IllogicalCallContext();
			if (HasUserData)
			{
				IDictionaryEnumerator enumerator = m_Datastore.GetEnumerator();
				while (enumerator.MoveNext())
				{
					illogicalCallContext.Datastore[(string)enumerator.Key] = enumerator.Value;
				}
			}
			return illogicalCallContext;
		}
	}
}
