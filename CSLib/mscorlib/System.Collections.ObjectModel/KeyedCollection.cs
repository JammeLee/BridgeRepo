using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace System.Collections.ObjectModel
{
	[Serializable]
	[ComVisible(false)]
	[DebuggerTypeProxy(typeof(Mscorlib_KeyedCollectionDebugView<, >))]
	[DebuggerDisplay("Count = {Count}")]
	public abstract class KeyedCollection<TKey, TItem> : Collection<TItem>
	{
		private const int defaultThreshold = 0;

		private IEqualityComparer<TKey> comparer;

		private Dictionary<TKey, TItem> dict;

		private int keyCount;

		private int threshold;

		public IEqualityComparer<TKey> Comparer => comparer;

		public TItem this[TKey key]
		{
			get
			{
				if (key == null)
				{
					ThrowHelper.ThrowArgumentNullException(ExceptionArgument.key);
				}
				if (dict != null)
				{
					return dict[key];
				}
				foreach (TItem item in base.Items)
				{
					if (comparer.Equals(GetKeyForItem(item), key))
					{
						return item;
					}
				}
				ThrowHelper.ThrowKeyNotFoundException();
				return default(TItem);
			}
		}

		protected IDictionary<TKey, TItem> Dictionary => dict;

		protected KeyedCollection()
			: this((IEqualityComparer<TKey>)null, 0)
		{
		}

		protected KeyedCollection(IEqualityComparer<TKey> comparer)
			: this(comparer, 0)
		{
		}

		protected KeyedCollection(IEqualityComparer<TKey> comparer, int dictionaryCreationThreshold)
		{
			if (comparer == null)
			{
				comparer = EqualityComparer<TKey>.Default;
			}
			if (dictionaryCreationThreshold == -1)
			{
				dictionaryCreationThreshold = int.MaxValue;
			}
			if (dictionaryCreationThreshold < -1)
			{
				ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.dictionaryCreationThreshold, ExceptionResource.ArgumentOutOfRange_InvalidThreshold);
			}
			this.comparer = comparer;
			threshold = dictionaryCreationThreshold;
		}

		public bool Contains(TKey key)
		{
			if (key == null)
			{
				ThrowHelper.ThrowArgumentNullException(ExceptionArgument.key);
			}
			if (dict != null)
			{
				return dict.ContainsKey(key);
			}
			if (key != null)
			{
				foreach (TItem item in base.Items)
				{
					if (comparer.Equals(GetKeyForItem(item), key))
					{
						return true;
					}
				}
			}
			return false;
		}

		private bool ContainsItem(TItem item)
		{
			TKey keyForItem;
			if (dict == null || (keyForItem = GetKeyForItem(item)) == null)
			{
				return base.Items.Contains(item);
			}
			if (dict.TryGetValue(keyForItem, out var value))
			{
				return EqualityComparer<TItem>.Default.Equals(value, item);
			}
			return false;
		}

		public bool Remove(TKey key)
		{
			if (key == null)
			{
				ThrowHelper.ThrowArgumentNullException(ExceptionArgument.key);
			}
			if (dict != null)
			{
				if (dict.ContainsKey(key))
				{
					return Remove(dict[key]);
				}
				return false;
			}
			if (key != null)
			{
				for (int i = 0; i < base.Items.Count; i++)
				{
					if (comparer.Equals(GetKeyForItem(base.Items[i]), key))
					{
						RemoveItem(i);
						return true;
					}
				}
			}
			return false;
		}

		protected void ChangeItemKey(TItem item, TKey newKey)
		{
			if (!ContainsItem(item))
			{
				ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_ItemNotExist);
			}
			TKey keyForItem = GetKeyForItem(item);
			if (!comparer.Equals(keyForItem, newKey))
			{
				if (newKey != null)
				{
					AddKey(newKey, item);
				}
				if (keyForItem != null)
				{
					RemoveKey(keyForItem);
				}
			}
		}

		protected override void ClearItems()
		{
			base.ClearItems();
			if (dict != null)
			{
				dict.Clear();
			}
			keyCount = 0;
		}

		protected abstract TKey GetKeyForItem(TItem item);

		protected override void InsertItem(int index, TItem item)
		{
			TKey keyForItem = GetKeyForItem(item);
			if (keyForItem != null)
			{
				AddKey(keyForItem, item);
			}
			base.InsertItem(index, item);
		}

		protected override void RemoveItem(int index)
		{
			TKey keyForItem = GetKeyForItem(base.Items[index]);
			if (keyForItem != null)
			{
				RemoveKey(keyForItem);
			}
			base.RemoveItem(index);
		}

		protected override void SetItem(int index, TItem item)
		{
			TKey keyForItem = GetKeyForItem(item);
			TKey keyForItem2 = GetKeyForItem(base.Items[index]);
			if (comparer.Equals(keyForItem2, keyForItem))
			{
				if (keyForItem != null && dict != null)
				{
					dict[keyForItem] = item;
				}
			}
			else
			{
				if (keyForItem != null)
				{
					AddKey(keyForItem, item);
				}
				if (keyForItem2 != null)
				{
					RemoveKey(keyForItem2);
				}
			}
			base.SetItem(index, item);
		}

		private void AddKey(TKey key, TItem item)
		{
			if (dict != null)
			{
				dict.Add(key, item);
				return;
			}
			if (keyCount == threshold)
			{
				CreateDictionary();
				dict.Add(key, item);
				return;
			}
			if (Contains(key))
			{
				ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_AddingDuplicate);
			}
			keyCount++;
		}

		private void CreateDictionary()
		{
			dict = new Dictionary<TKey, TItem>(comparer);
			foreach (TItem item in base.Items)
			{
				TKey keyForItem = GetKeyForItem(item);
				if (keyForItem != null)
				{
					dict.Add(keyForItem, item);
				}
			}
		}

		private void RemoveKey(TKey key)
		{
			if (dict != null)
			{
				dict.Remove(key);
			}
			else
			{
				keyCount--;
			}
		}
	}
}
