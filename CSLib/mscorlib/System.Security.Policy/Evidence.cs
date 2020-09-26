using System.Collections;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Permissions;

namespace System.Security.Policy
{
	[Serializable]
	[ComVisible(true)]
	public sealed class Evidence : ICollection, IEnumerable
	{
		private IList m_hostList;

		private IList m_assemblyList;

		private bool m_locked;

		public bool Locked
		{
			get
			{
				return m_locked;
			}
			set
			{
				if (!value)
				{
					new SecurityPermission(SecurityPermissionFlag.ControlEvidence).Demand();
					m_locked = false;
				}
				else
				{
					m_locked = true;
				}
			}
		}

		public int Count => ((m_hostList != null) ? m_hostList.Count : 0) + ((m_assemblyList != null) ? m_assemblyList.Count : 0);

		public object SyncRoot => this;

		public bool IsSynchronized => false;

		public bool IsReadOnly => false;

		public Evidence()
		{
			m_hostList = null;
			m_assemblyList = null;
			m_locked = false;
		}

		public Evidence(Evidence evidence)
		{
			if (evidence != null)
			{
				m_locked = false;
				Merge(evidence);
			}
		}

		public Evidence(object[] hostEvidence, object[] assemblyEvidence)
		{
			m_locked = false;
			if (hostEvidence != null)
			{
				m_hostList = ArrayList.Synchronized(new ArrayList(hostEvidence));
			}
			if (assemblyEvidence != null)
			{
				m_assemblyList = ArrayList.Synchronized(new ArrayList(assemblyEvidence));
			}
		}

		internal Evidence(char[] buffer)
		{
			int num = 0;
			while (num < buffer.Length)
			{
				switch (buffer[num++])
				{
				case '\0':
				{
					IBuiltInEvidence builtInEvidence9 = new ApplicationDirectory();
					num = builtInEvidence9.InitFromBuffer(buffer, num);
					AddAssembly(builtInEvidence9);
					break;
				}
				case '\u0001':
				{
					IBuiltInEvidence builtInEvidence8 = new Publisher();
					num = builtInEvidence8.InitFromBuffer(buffer, num);
					AddHost(builtInEvidence8);
					break;
				}
				case '\u0002':
				{
					IBuiltInEvidence builtInEvidence7 = new StrongName();
					num = builtInEvidence7.InitFromBuffer(buffer, num);
					AddHost(builtInEvidence7);
					break;
				}
				case '\u0003':
				{
					IBuiltInEvidence builtInEvidence6 = new Zone();
					num = builtInEvidence6.InitFromBuffer(buffer, num);
					AddHost(builtInEvidence6);
					break;
				}
				case '\u0004':
				{
					IBuiltInEvidence builtInEvidence5 = new Url();
					num = builtInEvidence5.InitFromBuffer(buffer, num);
					AddHost(builtInEvidence5);
					break;
				}
				case '\u0006':
				{
					IBuiltInEvidence builtInEvidence4 = new Site();
					num = builtInEvidence4.InitFromBuffer(buffer, num);
					AddHost(builtInEvidence4);
					break;
				}
				case '\a':
				{
					IBuiltInEvidence builtInEvidence3 = new PermissionRequestEvidence();
					num = builtInEvidence3.InitFromBuffer(buffer, num);
					AddHost(builtInEvidence3);
					break;
				}
				case '\b':
				{
					IBuiltInEvidence builtInEvidence2 = new Hash();
					num = builtInEvidence2.InitFromBuffer(buffer, num);
					AddHost(builtInEvidence2);
					break;
				}
				case '\t':
				{
					IBuiltInEvidence builtInEvidence = new GacInstalled();
					num = builtInEvidence.InitFromBuffer(buffer, num);
					AddHost(builtInEvidence);
					break;
				}
				default:
					throw new SerializationException(Environment.GetResourceString("Serialization_UnableToFixup"));
				}
			}
		}

		public void AddHost(object id)
		{
			if (m_hostList == null)
			{
				m_hostList = ArrayList.Synchronized(new ArrayList());
			}
			if (m_locked)
			{
				new SecurityPermission(SecurityPermissionFlag.ControlEvidence).Demand();
			}
			m_hostList.Add(id);
		}

		public void AddAssembly(object id)
		{
			if (m_assemblyList == null)
			{
				m_assemblyList = ArrayList.Synchronized(new ArrayList());
			}
			m_assemblyList.Add(id);
		}

		public void Merge(Evidence evidence)
		{
			if (evidence == null)
			{
				return;
			}
			if (evidence.m_hostList != null)
			{
				if (m_hostList == null)
				{
					m_hostList = ArrayList.Synchronized(new ArrayList());
				}
				if (evidence.m_hostList.Count != 0 && m_locked)
				{
					new SecurityPermission(SecurityPermissionFlag.ControlEvidence).Demand();
				}
				IEnumerator enumerator = evidence.m_hostList.GetEnumerator();
				while (enumerator.MoveNext())
				{
					m_hostList.Add(enumerator.Current);
				}
			}
			if (evidence.m_assemblyList != null)
			{
				if (m_assemblyList == null)
				{
					m_assemblyList = ArrayList.Synchronized(new ArrayList());
				}
				IEnumerator enumerator = evidence.m_assemblyList.GetEnumerator();
				while (enumerator.MoveNext())
				{
					m_assemblyList.Add(enumerator.Current);
				}
			}
		}

		internal void MergeWithNoDuplicates(Evidence evidence)
		{
			if (evidence == null)
			{
				return;
			}
			IEnumerator enumerator;
			if (evidence.m_hostList != null)
			{
				if (m_hostList == null)
				{
					m_hostList = ArrayList.Synchronized(new ArrayList());
				}
				enumerator = evidence.m_hostList.GetEnumerator();
				while (enumerator.MoveNext())
				{
					Type type = enumerator.Current.GetType();
					IEnumerator enumerator2 = m_hostList.GetEnumerator();
					while (enumerator2.MoveNext())
					{
						if (enumerator2.Current.GetType() == type)
						{
							m_hostList.Remove(enumerator2.Current);
							break;
						}
					}
					m_hostList.Add(enumerator.Current);
				}
			}
			if (evidence.m_assemblyList == null)
			{
				return;
			}
			if (m_assemblyList == null)
			{
				m_assemblyList = ArrayList.Synchronized(new ArrayList());
			}
			enumerator = evidence.m_assemblyList.GetEnumerator();
			while (enumerator.MoveNext())
			{
				Type type2 = enumerator.Current.GetType();
				IEnumerator enumerator2 = m_assemblyList.GetEnumerator();
				while (enumerator2.MoveNext())
				{
					if (enumerator2.Current.GetType() == type2)
					{
						m_assemblyList.Remove(enumerator2.Current);
						break;
					}
				}
				m_assemblyList.Add(enumerator.Current);
			}
		}

		public void CopyTo(Array array, int index)
		{
			int num = index;
			if (m_hostList != null)
			{
				m_hostList.CopyTo(array, num);
				num += m_hostList.Count;
			}
			if (m_assemblyList != null)
			{
				m_assemblyList.CopyTo(array, num);
			}
		}

		public IEnumerator GetHostEnumerator()
		{
			if (m_hostList == null)
			{
				m_hostList = ArrayList.Synchronized(new ArrayList());
			}
			return m_hostList.GetEnumerator();
		}

		public IEnumerator GetAssemblyEnumerator()
		{
			if (m_assemblyList == null)
			{
				m_assemblyList = ArrayList.Synchronized(new ArrayList());
			}
			return m_assemblyList.GetEnumerator();
		}

		public IEnumerator GetEnumerator()
		{
			return new EvidenceEnumerator(this);
		}

		internal Evidence Copy()
		{
			char[] array = PolicyManager.MakeEvidenceArray(this, verbose: true);
			if (array != null)
			{
				return new Evidence(array);
			}
			new PermissionSet(fUnrestricted: true).Assert();
			MemoryStream memoryStream = new MemoryStream();
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			binaryFormatter.Serialize(memoryStream, this);
			memoryStream.Position = 0L;
			return (Evidence)binaryFormatter.Deserialize(memoryStream);
		}

		internal Evidence ShallowCopy()
		{
			Evidence evidence = new Evidence();
			IEnumerator hostEnumerator = GetHostEnumerator();
			while (hostEnumerator.MoveNext())
			{
				evidence.AddHost(hostEnumerator.Current);
			}
			hostEnumerator = GetAssemblyEnumerator();
			while (hostEnumerator.MoveNext())
			{
				evidence.AddAssembly(hostEnumerator.Current);
			}
			return evidence;
		}

		[ComVisible(false)]
		public void Clear()
		{
			m_hostList = null;
			m_assemblyList = null;
		}

		[ComVisible(false)]
		public void RemoveType(Type t)
		{
			for (int i = 0; i < ((m_hostList != null) ? m_hostList.Count : 0); i++)
			{
				if (m_hostList[i].GetType() == t)
				{
					m_hostList.RemoveAt(i--);
				}
			}
			for (int i = 0; i < ((m_assemblyList != null) ? m_assemblyList.Count : 0); i++)
			{
				if (m_assemblyList[i].GetType() == t)
				{
					m_assemblyList.RemoveAt(i--);
				}
			}
		}

		[ComVisible(false)]
		public override bool Equals(object obj)
		{
			Evidence evidence = obj as Evidence;
			if (evidence == null)
			{
				return false;
			}
			if (m_hostList != null && evidence.m_hostList != null)
			{
				if (m_hostList.Count != evidence.m_hostList.Count)
				{
					return false;
				}
				int count = m_hostList.Count;
				for (int i = 0; i < count; i++)
				{
					bool flag = false;
					for (int j = 0; j < count; j++)
					{
						if (object.Equals(m_hostList[i], evidence.m_hostList[j]))
						{
							flag = true;
							break;
						}
					}
					if (!flag)
					{
						return false;
					}
				}
			}
			else if (m_hostList != null || evidence.m_hostList != null)
			{
				return false;
			}
			if (m_assemblyList != null && evidence.m_assemblyList != null)
			{
				if (m_assemblyList.Count != evidence.m_assemblyList.Count)
				{
					return false;
				}
				int count2 = m_assemblyList.Count;
				for (int k = 0; k < count2; k++)
				{
					bool flag2 = false;
					for (int l = 0; l < count2; l++)
					{
						if (object.Equals(m_assemblyList[k], evidence.m_assemblyList[l]))
						{
							flag2 = true;
							break;
						}
					}
					if (!flag2)
					{
						return false;
					}
				}
			}
			else if (m_assemblyList != null || evidence.m_assemblyList != null)
			{
				return false;
			}
			return true;
		}

		[ComVisible(false)]
		public override int GetHashCode()
		{
			int num = 0;
			if (m_hostList != null)
			{
				int count = m_hostList.Count;
				for (int i = 0; i < count; i++)
				{
					num ^= m_hostList[i].GetHashCode();
				}
			}
			if (m_assemblyList != null)
			{
				int count2 = m_assemblyList.Count;
				for (int j = 0; j < count2; j++)
				{
					num ^= m_assemblyList[j].GetHashCode();
				}
			}
			return num;
		}

		internal object FindType(Type t)
		{
			for (int i = 0; i < ((m_hostList != null) ? m_hostList.Count : 0); i++)
			{
				if (m_hostList[i].GetType() == t)
				{
					return m_hostList[i];
				}
			}
			for (int i = 0; i < ((m_assemblyList != null) ? m_assemblyList.Count : 0); i++)
			{
				if (m_assemblyList[i].GetType() == t)
				{
					return m_hostList[i];
				}
			}
			return null;
		}

		internal void MarkAllEvidenceAsUsed()
		{
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					object current = enumerator.Current;
					(current as IDelayEvaluatedEvidence)?.MarkUsed();
				}
			}
			finally
			{
				IDisposable disposable = enumerator as IDisposable;
				if (disposable != null)
				{
					disposable.Dispose();
				}
			}
		}

		private bool WasStrongNameEvidenceUsed()
		{
			return (FindType(typeof(StrongName)) as IDelayEvaluatedEvidence)?.WasUsed ?? false;
		}
	}
}
