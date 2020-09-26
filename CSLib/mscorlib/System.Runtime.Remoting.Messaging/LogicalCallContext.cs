using System.Collections;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Security.Principal;

namespace System.Runtime.Remoting.Messaging
{
	[Serializable]
	[ComVisible(true)]
	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
	public sealed class LogicalCallContext : ISerializable, ICloneable
	{
		private const string s_CorrelationMgrSlotName = "System.Diagnostics.Trace.CorrelationManagerSlot";

		private static Type s_callContextType = typeof(LogicalCallContext);

		private Hashtable m_Datastore;

		private CallContextRemotingData m_RemotingData;

		private CallContextSecurityData m_SecurityData;

		private object m_HostContext;

		private bool m_IsCorrelationMgr;

		private Header[] _sendHeaders;

		private Header[] _recvHeaders;

		public bool HasInfo
		{
			get
			{
				bool result = false;
				if ((m_RemotingData != null && m_RemotingData.HasInfo) || (m_SecurityData != null && m_SecurityData.HasInfo) || m_HostContext != null || HasUserData)
				{
					result = true;
				}
				return result;
			}
		}

		private bool HasUserData
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

		internal CallContextRemotingData RemotingData
		{
			get
			{
				if (m_RemotingData == null)
				{
					m_RemotingData = new CallContextRemotingData();
				}
				return m_RemotingData;
			}
		}

		internal CallContextSecurityData SecurityData
		{
			get
			{
				if (m_SecurityData == null)
				{
					m_SecurityData = new CallContextSecurityData();
				}
				return m_SecurityData;
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

		internal IPrincipal Principal
		{
			get
			{
				if (m_SecurityData != null)
				{
					return m_SecurityData.Principal;
				}
				return null;
			}
			set
			{
				SecurityData.Principal = value;
			}
		}

		internal LogicalCallContext()
		{
		}

		internal LogicalCallContext(SerializationInfo info, StreamingContext context)
		{
			SerializationInfoEnumerator enumerator = info.GetEnumerator();
			while (enumerator.MoveNext())
			{
				if (enumerator.Name.Equals("__RemotingData"))
				{
					m_RemotingData = (CallContextRemotingData)enumerator.Value;
				}
				else if (enumerator.Name.Equals("__SecurityData"))
				{
					if (context.State == StreamingContextStates.CrossAppDomain)
					{
						m_SecurityData = (CallContextSecurityData)enumerator.Value;
					}
				}
				else if (enumerator.Name.Equals("__HostContext"))
				{
					m_HostContext = enumerator.Value;
				}
				else if (enumerator.Name.Equals("__CorrelationMgrSlotPresent"))
				{
					m_IsCorrelationMgr = (bool)enumerator.Value;
				}
				else
				{
					Datastore[enumerator.Name] = enumerator.Value;
				}
			}
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			info.SetType(s_callContextType);
			if (m_RemotingData != null)
			{
				info.AddValue("__RemotingData", m_RemotingData);
			}
			if (m_SecurityData != null && context.State == StreamingContextStates.CrossAppDomain)
			{
				info.AddValue("__SecurityData", m_SecurityData);
			}
			if (m_HostContext != null)
			{
				info.AddValue("__HostContext", m_HostContext);
			}
			if (m_IsCorrelationMgr)
			{
				info.AddValue("__CorrelationMgrSlotPresent", m_IsCorrelationMgr);
			}
			if (HasUserData)
			{
				IDictionaryEnumerator enumerator = m_Datastore.GetEnumerator();
				while (enumerator.MoveNext())
				{
					info.AddValue((string)enumerator.Key, enumerator.Value);
				}
			}
		}

		public object Clone()
		{
			LogicalCallContext logicalCallContext = new LogicalCallContext();
			if (m_RemotingData != null)
			{
				logicalCallContext.m_RemotingData = (CallContextRemotingData)m_RemotingData.Clone();
			}
			if (m_SecurityData != null)
			{
				logicalCallContext.m_SecurityData = (CallContextSecurityData)m_SecurityData.Clone();
			}
			if (m_HostContext != null)
			{
				logicalCallContext.m_HostContext = m_HostContext;
			}
			logicalCallContext.m_IsCorrelationMgr = m_IsCorrelationMgr;
			if (HasUserData)
			{
				IDictionaryEnumerator enumerator = m_Datastore.GetEnumerator();
				if (!m_IsCorrelationMgr)
				{
					while (enumerator.MoveNext())
					{
						logicalCallContext.Datastore[(string)enumerator.Key] = enumerator.Value;
					}
				}
				else
				{
					while (enumerator.MoveNext())
					{
						string text = (string)enumerator.Key;
						if (text.Equals("System.Diagnostics.Trace.CorrelationManagerSlot"))
						{
							logicalCallContext.Datastore[text] = ((ICloneable)enumerator.Value).Clone();
						}
						else
						{
							logicalCallContext.Datastore[text] = enumerator.Value;
						}
					}
				}
			}
			return logicalCallContext;
		}

		internal void Merge(LogicalCallContext lc)
		{
			if (lc != null && this != lc && lc.HasUserData)
			{
				IDictionaryEnumerator enumerator = lc.Datastore.GetEnumerator();
				while (enumerator.MoveNext())
				{
					Datastore[(string)enumerator.Key] = enumerator.Value;
				}
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
			if (name.Equals("System.Diagnostics.Trace.CorrelationManagerSlot"))
			{
				m_IsCorrelationMgr = true;
			}
		}

		private Header[] InternalGetOutgoingHeaders()
		{
			Header[] sendHeaders = _sendHeaders;
			_sendHeaders = null;
			_recvHeaders = null;
			return sendHeaders;
		}

		internal void InternalSetHeaders(Header[] headers)
		{
			_sendHeaders = headers;
			_recvHeaders = null;
		}

		internal Header[] InternalGetHeaders()
		{
			if (_sendHeaders != null)
			{
				return _sendHeaders;
			}
			return _recvHeaders;
		}

		internal IPrincipal RemovePrincipalIfNotSerializable()
		{
			IPrincipal principal = Principal;
			if (principal != null && !principal.GetType().IsSerializable)
			{
				Principal = null;
			}
			return principal;
		}

		internal void PropagateOutgoingHeadersToMessage(IMessage msg)
		{
			Header[] array = InternalGetOutgoingHeaders();
			if (array == null)
			{
				return;
			}
			IDictionary properties = msg.Properties;
			Header[] array2 = array;
			foreach (Header header in array2)
			{
				if (header != null)
				{
					string propertyKeyForHeader = GetPropertyKeyForHeader(header);
					properties[propertyKeyForHeader] = header;
				}
			}
		}

		internal static string GetPropertyKeyForHeader(Header header)
		{
			if (header == null)
			{
				return null;
			}
			if (header.HeaderNamespace != null)
			{
				return header.Name + ", " + header.HeaderNamespace;
			}
			return header.Name;
		}

		internal void PropagateIncomingHeadersToCallContext(IMessage msg)
		{
			IInternalMessage internalMessage = msg as IInternalMessage;
			if (internalMessage != null && !internalMessage.HasProperties())
			{
				return;
			}
			IDictionary properties = msg.Properties;
			IDictionaryEnumerator enumerator = properties.GetEnumerator();
			int num = 0;
			while (enumerator.MoveNext())
			{
				string text = (string)enumerator.Key;
				if (!text.StartsWith("__", StringComparison.Ordinal) && enumerator.Value is Header)
				{
					num++;
				}
			}
			Header[] array = null;
			if (num > 0)
			{
				array = new Header[num];
				num = 0;
				enumerator.Reset();
				while (enumerator.MoveNext())
				{
					string text2 = (string)enumerator.Key;
					if (!text2.StartsWith("__", StringComparison.Ordinal))
					{
						Header header = enumerator.Value as Header;
						if (header != null)
						{
							array[num++] = header;
						}
					}
				}
			}
			_recvHeaders = array;
			_sendHeaders = null;
		}
	}
}
