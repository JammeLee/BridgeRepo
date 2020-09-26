using System.ComponentModel.Design.Serialization;
using System.Globalization;

namespace System.Collections.Specialized
{
	[Serializable]
	[DesignerSerializer("System.Diagnostics.Design.StringDictionaryCodeDomSerializer, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.ComponentModel.Design.Serialization.CodeDomSerializer, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
	public class StringDictionary : IEnumerable
	{
		internal Hashtable contents = new Hashtable();

		public virtual int Count => contents.Count;

		public virtual bool IsSynchronized => contents.IsSynchronized;

		public virtual string this[string key]
		{
			get
			{
				if (key == null)
				{
					throw new ArgumentNullException("key");
				}
				return (string)contents[key.ToLower(CultureInfo.InvariantCulture)];
			}
			set
			{
				if (key == null)
				{
					throw new ArgumentNullException("key");
				}
				contents[key.ToLower(CultureInfo.InvariantCulture)] = value;
			}
		}

		public virtual ICollection Keys => contents.Keys;

		public virtual object SyncRoot => contents.SyncRoot;

		public virtual ICollection Values => contents.Values;

		public virtual void Add(string key, string value)
		{
			if (key == null)
			{
				throw new ArgumentNullException("key");
			}
			contents.Add(key.ToLower(CultureInfo.InvariantCulture), value);
		}

		public virtual void Clear()
		{
			contents.Clear();
		}

		public virtual bool ContainsKey(string key)
		{
			if (key == null)
			{
				throw new ArgumentNullException("key");
			}
			return contents.ContainsKey(key.ToLower(CultureInfo.InvariantCulture));
		}

		public virtual bool ContainsValue(string value)
		{
			return contents.ContainsValue(value);
		}

		public virtual void CopyTo(Array array, int index)
		{
			contents.CopyTo(array, index);
		}

		public virtual IEnumerator GetEnumerator()
		{
			return contents.GetEnumerator();
		}

		public virtual void Remove(string key)
		{
			if (key == null)
			{
				throw new ArgumentNullException("key");
			}
			contents.Remove(key.ToLower(CultureInfo.InvariantCulture));
		}
	}
}
