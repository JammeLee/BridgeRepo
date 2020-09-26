using System.Collections;

namespace System.Net
{
	internal class PrefixLookup
	{
		private Hashtable m_Store = new Hashtable();

		internal void Add(string prefix, object value)
		{
			lock (m_Store)
			{
				m_Store[prefix] = value;
			}
		}

		internal object Lookup(string lookupKey)
		{
			if (lookupKey == null)
			{
				return null;
			}
			object result = null;
			int num = 0;
			lock (m_Store)
			{
				foreach (DictionaryEntry item in m_Store)
				{
					string text = (string)item.Key;
					if (lookupKey.StartsWith(text))
					{
						int length = text.Length;
						if (length > num)
						{
							num = length;
							result = item.Value;
						}
					}
				}
				return result;
			}
		}
	}
}
