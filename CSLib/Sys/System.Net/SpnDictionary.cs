using System.Collections;
using System.Collections.Specialized;
using System.Security.Permissions;

namespace System.Net
{
	internal class SpnDictionary : StringDictionary
	{
		private Hashtable m_SyncTable = Hashtable.Synchronized(new Hashtable());

		public override int Count
		{
			get
			{
				ExceptionHelper.WebPermissionUnrestricted.Demand();
				return m_SyncTable.Count;
			}
		}

		public override bool IsSynchronized => true;

		public override string this[string key]
		{
			get
			{
				key = GetCanonicalKey(key);
				return InternalGet(key);
			}
			set
			{
				key = GetCanonicalKey(key);
				InternalSet(key, value);
			}
		}

		public override ICollection Keys
		{
			get
			{
				ExceptionHelper.WebPermissionUnrestricted.Demand();
				return m_SyncTable.Keys;
			}
		}

		public override object SyncRoot
		{
			[HostProtection(SecurityAction.LinkDemand, Synchronization = true)]
			get
			{
				ExceptionHelper.WebPermissionUnrestricted.Demand();
				return m_SyncTable;
			}
		}

		public override ICollection Values
		{
			get
			{
				ExceptionHelper.WebPermissionUnrestricted.Demand();
				return m_SyncTable.Values;
			}
		}

		internal SpnDictionary()
		{
		}

		internal string InternalGet(string canonicalKey)
		{
			int num = 0;
			string text = null;
			lock (m_SyncTable.SyncRoot)
			{
				foreach (object key in m_SyncTable.Keys)
				{
					string text2 = (string)key;
					if (text2 != null && text2.Length > num && string.Compare(text2, 0, canonicalKey, 0, text2.Length, StringComparison.OrdinalIgnoreCase) == 0)
					{
						num = text2.Length;
						text = text2;
					}
				}
			}
			if (text == null)
			{
				return null;
			}
			return (string)m_SyncTable[text];
		}

		internal void InternalSet(string canonicalKey, string spn)
		{
			m_SyncTable[canonicalKey] = spn;
		}

		public override void Add(string key, string value)
		{
			key = GetCanonicalKey(key);
			m_SyncTable.Add(key, value);
		}

		public override void Clear()
		{
			ExceptionHelper.WebPermissionUnrestricted.Demand();
			m_SyncTable.Clear();
		}

		public override bool ContainsKey(string key)
		{
			key = GetCanonicalKey(key);
			return m_SyncTable.ContainsKey(key);
		}

		public override bool ContainsValue(string value)
		{
			ExceptionHelper.WebPermissionUnrestricted.Demand();
			return m_SyncTable.ContainsValue(value);
		}

		public override void CopyTo(Array array, int index)
		{
			ExceptionHelper.WebPermissionUnrestricted.Demand();
			m_SyncTable.CopyTo(array, index);
		}

		public override IEnumerator GetEnumerator()
		{
			ExceptionHelper.WebPermissionUnrestricted.Demand();
			return m_SyncTable.GetEnumerator();
		}

		public override void Remove(string key)
		{
			key = GetCanonicalKey(key);
			m_SyncTable.Remove(key);
		}

		private static string GetCanonicalKey(string key)
		{
			if (key == null)
			{
				throw new ArgumentNullException("key");
			}
			try
			{
				Uri uri = new Uri(key);
				key = uri.GetParts(UriComponents.SchemeAndServer | UriComponents.Path, UriFormat.SafeUnescaped);
				new WebPermission(NetworkAccess.Connect, new Uri(key)).Demand();
				return key;
			}
			catch (UriFormatException innerException)
			{
				throw new ArgumentException(SR.GetString("net_mustbeuri", "key"), "key", innerException);
			}
		}
	}
}
