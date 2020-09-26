using System.Collections;
using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace System.Runtime.Remoting.Channels
{
	[ComVisible(true)]
	[SecurityPermission(SecurityAction.InheritanceDemand, Flags = SecurityPermissionFlag.Infrastructure)]
	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
	public abstract class BaseChannelObjectWithProperties : IDictionary, ICollection, IEnumerable
	{
		public virtual IDictionary Properties
		{
			[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
			get
			{
				return this;
			}
		}

		public virtual object this[object key]
		{
			get
			{
				return null;
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		public virtual ICollection Keys => null;

		public virtual ICollection Values
		{
			get
			{
				ICollection keys = Keys;
				if (keys == null)
				{
					return null;
				}
				ArrayList arrayList = new ArrayList();
				foreach (object item in keys)
				{
					arrayList.Add(this[item]);
				}
				return arrayList;
			}
		}

		public virtual bool IsReadOnly => false;

		public virtual bool IsFixedSize => true;

		public virtual int Count => Keys?.Count ?? 0;

		public virtual object SyncRoot => this;

		public virtual bool IsSynchronized => false;

		public virtual bool Contains(object key)
		{
			if (key == null)
			{
				return false;
			}
			ICollection keys = Keys;
			if (keys == null)
			{
				return false;
			}
			string text = key as string;
			foreach (object item in keys)
			{
				if (text != null)
				{
					string text2 = item as string;
					if (text2 != null)
					{
						if (string.Compare(text, text2, StringComparison.OrdinalIgnoreCase) == 0)
						{
							return true;
						}
						continue;
					}
				}
				if (key.Equals(item))
				{
					return true;
				}
			}
			return false;
		}

		public virtual void Add(object key, object value)
		{
			throw new NotSupportedException();
		}

		public virtual void Clear()
		{
			throw new NotSupportedException();
		}

		public virtual void Remove(object key)
		{
			throw new NotSupportedException();
		}

		public virtual IDictionaryEnumerator GetEnumerator()
		{
			return new DictionaryEnumeratorByKeys(this);
		}

		public virtual void CopyTo(Array array, int index)
		{
			throw new NotSupportedException();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return new DictionaryEnumeratorByKeys(this);
		}
	}
}
