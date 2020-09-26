using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace System.Collections.Generic
{
	[Serializable]
	[DebuggerDisplay("Count = {Count}")]
	[ComVisible(false)]
	[DebuggerTypeProxy(typeof(System_DictionaryDebugView<, >))]
	public class SortedList<TKey, TValue> : IDictionary<TKey, TValue>, ICollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>, IDictionary, ICollection, IEnumerable
	{
		[Serializable]
		private struct Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>, IDisposable, IDictionaryEnumerator, IEnumerator
		{
			internal const int KeyValuePair = 1;

			internal const int DictEntry = 2;

			private SortedList<TKey, TValue> _sortedList;

			private TKey key;

			private TValue value;

			private int index;

			private int version;

			private int getEnumeratorRetType;

			object IDictionaryEnumerator.Key
			{
				get
				{
					if (index == 0 || index == _sortedList.Count + 1)
					{
						ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_EnumOpCantHappen);
					}
					return key;
				}
			}

			DictionaryEntry IDictionaryEnumerator.Entry
			{
				get
				{
					if (index == 0 || index == _sortedList.Count + 1)
					{
						ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_EnumOpCantHappen);
					}
					return new DictionaryEntry(key, value);
				}
			}

			public KeyValuePair<TKey, TValue> Current => new KeyValuePair<TKey, TValue>(key, value);

			object IEnumerator.Current
			{
				get
				{
					if (index == 0 || index == _sortedList.Count + 1)
					{
						ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_EnumOpCantHappen);
					}
					if (getEnumeratorRetType == 2)
					{
						return new DictionaryEntry(key, value);
					}
					return new KeyValuePair<TKey, TValue>(key, value);
				}
			}

			object IDictionaryEnumerator.Value
			{
				get
				{
					if (index == 0 || index == _sortedList.Count + 1)
					{
						ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_EnumOpCantHappen);
					}
					return value;
				}
			}

			internal Enumerator(SortedList<TKey, TValue> sortedList, int getEnumeratorRetType)
			{
				_sortedList = sortedList;
				index = 0;
				version = _sortedList.version;
				this.getEnumeratorRetType = getEnumeratorRetType;
				key = default(TKey);
				value = default(TValue);
			}

			public void Dispose()
			{
				index = 0;
				key = default(TKey);
				value = default(TValue);
			}

			public bool MoveNext()
			{
				if (version != _sortedList.version)
				{
					ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_EnumFailedVersion);
				}
				if ((uint)index < (uint)_sortedList.Count)
				{
					key = _sortedList.keys[index];
					value = _sortedList.values[index];
					index++;
					return true;
				}
				index = _sortedList.Count + 1;
				key = default(TKey);
				value = default(TValue);
				return false;
			}

			void IEnumerator.Reset()
			{
				if (version != _sortedList.version)
				{
					ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_EnumFailedVersion);
				}
				index = 0;
				key = default(TKey);
				value = default(TValue);
			}
		}

		[Serializable]
		private sealed class SortedListKeyEnumerator : IEnumerator<TKey>, IDisposable, IEnumerator
		{
			private SortedList<TKey, TValue> _sortedList;

			private int index;

			private int version;

			private TKey currentKey;

			public TKey Current => currentKey;

			object IEnumerator.Current
			{
				get
				{
					if (index == 0 || index == _sortedList.Count + 1)
					{
						ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_EnumOpCantHappen);
					}
					return currentKey;
				}
			}

			internal SortedListKeyEnumerator(SortedList<TKey, TValue> sortedList)
			{
				_sortedList = sortedList;
				version = sortedList.version;
			}

			public void Dispose()
			{
				index = 0;
				currentKey = default(TKey);
			}

			public bool MoveNext()
			{
				if (version != _sortedList.version)
				{
					ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_EnumFailedVersion);
				}
				if ((uint)index < (uint)_sortedList.Count)
				{
					currentKey = _sortedList.keys[index];
					index++;
					return true;
				}
				index = _sortedList.Count + 1;
				currentKey = default(TKey);
				return false;
			}

			void IEnumerator.Reset()
			{
				if (version != _sortedList.version)
				{
					ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_EnumFailedVersion);
				}
				index = 0;
				currentKey = default(TKey);
			}
		}

		[Serializable]
		private sealed class SortedListValueEnumerator : IEnumerator<TValue>, IDisposable, IEnumerator
		{
			private SortedList<TKey, TValue> _sortedList;

			private int index;

			private int version;

			private TValue currentValue;

			public TValue Current => currentValue;

			object IEnumerator.Current
			{
				get
				{
					if (index == 0 || index == _sortedList.Count + 1)
					{
						ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_EnumOpCantHappen);
					}
					return currentValue;
				}
			}

			internal SortedListValueEnumerator(SortedList<TKey, TValue> sortedList)
			{
				_sortedList = sortedList;
				version = sortedList.version;
			}

			public void Dispose()
			{
				index = 0;
				currentValue = default(TValue);
			}

			public bool MoveNext()
			{
				if (version != _sortedList.version)
				{
					ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_EnumFailedVersion);
				}
				if ((uint)index < (uint)_sortedList.Count)
				{
					currentValue = _sortedList.values[index];
					index++;
					return true;
				}
				index = _sortedList.Count + 1;
				currentValue = default(TValue);
				return false;
			}

			void IEnumerator.Reset()
			{
				if (version != _sortedList.version)
				{
					ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_EnumFailedVersion);
				}
				index = 0;
				currentValue = default(TValue);
			}
		}

		[Serializable]
		[DebuggerTypeProxy(typeof(System_DictionaryKeyCollectionDebugView<, >))]
		[DebuggerDisplay("Count = {Count}")]
		private sealed class KeyList : IList<TKey>, ICollection<TKey>, IEnumerable<TKey>, ICollection, IEnumerable
		{
			private SortedList<TKey, TValue> _dict;

			public int Count => _dict._size;

			public bool IsReadOnly => true;

			bool ICollection.IsSynchronized => false;

			object ICollection.SyncRoot => ((ICollection)_dict).SyncRoot;

			public TKey this[int index]
			{
				get
				{
					return _dict.GetKey(index);
				}
				set
				{
					ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_KeyCollectionSet);
				}
			}

			internal KeyList(SortedList<TKey, TValue> dictionary)
			{
				_dict = dictionary;
			}

			public void Add(TKey key)
			{
				ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_SortedListNestedWrite);
			}

			public void Clear()
			{
				ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_SortedListNestedWrite);
			}

			public bool Contains(TKey key)
			{
				return _dict.ContainsKey(key);
			}

			public void CopyTo(TKey[] array, int arrayIndex)
			{
				Array.Copy(_dict.keys, 0, array, arrayIndex, _dict.Count);
			}

			void ICollection.CopyTo(Array array, int arrayIndex)
			{
				if (array != null && array.Rank != 1)
				{
					ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_RankMultiDimNotSupported);
				}
				try
				{
					Array.Copy(_dict.keys, 0, array, arrayIndex, _dict.Count);
				}
				catch (ArrayTypeMismatchException)
				{
					ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_InvalidArrayType);
				}
			}

			public void Insert(int index, TKey value)
			{
				ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_SortedListNestedWrite);
			}

			public IEnumerator<TKey> GetEnumerator()
			{
				return new SortedListKeyEnumerator(_dict);
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return new SortedListKeyEnumerator(_dict);
			}

			public int IndexOf(TKey key)
			{
				if (key == null)
				{
					ThrowHelper.ThrowArgumentNullException(ExceptionArgument.key);
				}
				int num = Array.BinarySearch(_dict.keys, 0, _dict.Count, key, _dict.comparer);
				if (num >= 0)
				{
					return num;
				}
				return -1;
			}

			public bool Remove(TKey key)
			{
				ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_SortedListNestedWrite);
				return false;
			}

			public void RemoveAt(int index)
			{
				ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_SortedListNestedWrite);
			}
		}

		[Serializable]
		[DebuggerTypeProxy(typeof(System_DictionaryValueCollectionDebugView<, >))]
		[DebuggerDisplay("Count = {Count}")]
		private sealed class ValueList : IList<TValue>, ICollection<TValue>, IEnumerable<TValue>, ICollection, IEnumerable
		{
			private SortedList<TKey, TValue> _dict;

			public int Count => _dict._size;

			public bool IsReadOnly => true;

			bool ICollection.IsSynchronized => false;

			object ICollection.SyncRoot => ((ICollection)_dict).SyncRoot;

			public TValue this[int index]
			{
				get
				{
					return _dict.GetByIndex(index);
				}
				set
				{
					ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_SortedListNestedWrite);
				}
			}

			internal ValueList(SortedList<TKey, TValue> dictionary)
			{
				_dict = dictionary;
			}

			public void Add(TValue key)
			{
				ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_SortedListNestedWrite);
			}

			public void Clear()
			{
				ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_SortedListNestedWrite);
			}

			public bool Contains(TValue value)
			{
				return _dict.ContainsValue(value);
			}

			public void CopyTo(TValue[] array, int arrayIndex)
			{
				Array.Copy(_dict.values, 0, array, arrayIndex, _dict.Count);
			}

			void ICollection.CopyTo(Array array, int arrayIndex)
			{
				if (array != null && array.Rank != 1)
				{
					ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_RankMultiDimNotSupported);
				}
				try
				{
					Array.Copy(_dict.values, 0, array, arrayIndex, _dict.Count);
				}
				catch (ArrayTypeMismatchException)
				{
					ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_InvalidArrayType);
				}
			}

			public void Insert(int index, TValue value)
			{
				ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_SortedListNestedWrite);
			}

			public IEnumerator<TValue> GetEnumerator()
			{
				return new SortedListValueEnumerator(_dict);
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return new SortedListValueEnumerator(_dict);
			}

			public int IndexOf(TValue value)
			{
				return Array.IndexOf(_dict.values, value, 0, _dict.Count);
			}

			public bool Remove(TValue value)
			{
				ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_SortedListNestedWrite);
				return false;
			}

			public void RemoveAt(int index)
			{
				ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_SortedListNestedWrite);
			}
		}

		private const int _defaultCapacity = 4;

		private TKey[] keys;

		private TValue[] values;

		private int _size;

		private int version;

		private IComparer<TKey> comparer;

		private KeyList keyList;

		private ValueList valueList;

		[NonSerialized]
		private object _syncRoot;

		private static TKey[] emptyKeys = new TKey[0];

		private static TValue[] emptyValues = new TValue[0];

		public int Capacity
		{
			get
			{
				return keys.Length;
			}
			set
			{
				if (value == keys.Length)
				{
					return;
				}
				if (value < _size)
				{
					ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.value, ExceptionResource.ArgumentOutOfRange_SmallCapacity);
				}
				if (value > 0)
				{
					TKey[] destinationArray = new TKey[value];
					TValue[] destinationArray2 = new TValue[value];
					if (_size > 0)
					{
						Array.Copy(keys, 0, destinationArray, 0, _size);
						Array.Copy(values, 0, destinationArray2, 0, _size);
					}
					keys = destinationArray;
					values = destinationArray2;
				}
				else
				{
					keys = emptyKeys;
					values = emptyValues;
				}
			}
		}

		public IComparer<TKey> Comparer => comparer;

		public int Count => _size;

		public IList<TKey> Keys => GetKeyListHelper();

		ICollection<TKey> IDictionary<TKey, TValue>.Keys => GetKeyListHelper();

		ICollection IDictionary.Keys => GetKeyListHelper();

		public IList<TValue> Values => GetValueListHelper();

		ICollection<TValue> IDictionary<TKey, TValue>.Values => GetValueListHelper();

		ICollection IDictionary.Values => GetValueListHelper();

		bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => false;

		bool IDictionary.IsReadOnly => false;

		bool IDictionary.IsFixedSize => false;

		bool ICollection.IsSynchronized => false;

		object ICollection.SyncRoot
		{
			get
			{
				if (_syncRoot == null)
				{
					Interlocked.CompareExchange(ref _syncRoot, new object(), null);
				}
				return _syncRoot;
			}
		}

		public TValue this[TKey key]
		{
			get
			{
				int num = IndexOfKey(key);
				if (num >= 0)
				{
					return values[num];
				}
				ThrowHelper.ThrowKeyNotFoundException();
				return default(TValue);
			}
			set
			{
				if (key == null)
				{
					ThrowHelper.ThrowArgumentNullException(ExceptionArgument.key);
				}
				int num = Array.BinarySearch(keys, 0, _size, key, comparer);
				if (num >= 0)
				{
					values[num] = value;
					version++;
				}
				else
				{
					Insert(~num, key, value);
				}
			}
		}

		object IDictionary.this[object key]
		{
			get
			{
				if (IsCompatibleKey(key))
				{
					int num = IndexOfKey((TKey)key);
					if (num >= 0)
					{
						return values[num];
					}
				}
				return null;
			}
			set
			{
				VerifyKey(key);
				VerifyValueType(value);
				this[(TKey)key] = (TValue)value;
			}
		}

		public SortedList()
		{
			keys = emptyKeys;
			values = emptyValues;
			_size = 0;
			comparer = Comparer<TKey>.Default;
		}

		public SortedList(int capacity)
		{
			if (capacity < 0)
			{
				ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.capacity, ExceptionResource.ArgumentOutOfRange_NeedNonNegNumRequired);
			}
			keys = new TKey[capacity];
			values = new TValue[capacity];
			comparer = Comparer<TKey>.Default;
		}

		public SortedList(IComparer<TKey> comparer)
			: this()
		{
			if (comparer != null)
			{
				this.comparer = comparer;
			}
		}

		public SortedList(int capacity, IComparer<TKey> comparer)
			: this(comparer)
		{
			Capacity = capacity;
		}

		public SortedList(IDictionary<TKey, TValue> dictionary)
			: this(dictionary, (IComparer<TKey>)null)
		{
		}

		public SortedList(IDictionary<TKey, TValue> dictionary, IComparer<TKey> comparer)
			: this(dictionary?.Count ?? 0, comparer)
		{
			if (dictionary == null)
			{
				ThrowHelper.ThrowArgumentNullException(ExceptionArgument.dictionary);
			}
			dictionary.Keys.CopyTo(keys, 0);
			dictionary.Values.CopyTo(values, 0);
			Array.Sort(keys, values, comparer);
			_size = dictionary.Count;
		}

		public void Add(TKey key, TValue value)
		{
			if (key == null)
			{
				ThrowHelper.ThrowArgumentNullException(ExceptionArgument.key);
			}
			int num = Array.BinarySearch(keys, 0, _size, key, comparer);
			if (num >= 0)
			{
				ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_AddingDuplicate);
			}
			Insert(~num, key, value);
		}

		void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> keyValuePair)
		{
			Add(keyValuePair.Key, keyValuePair.Value);
		}

		bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> keyValuePair)
		{
			int num = IndexOfKey(keyValuePair.Key);
			if (num >= 0 && EqualityComparer<TValue>.Default.Equals(values[num], keyValuePair.Value))
			{
				return true;
			}
			return false;
		}

		bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> keyValuePair)
		{
			int num = IndexOfKey(keyValuePair.Key);
			if (num >= 0 && EqualityComparer<TValue>.Default.Equals(values[num], keyValuePair.Value))
			{
				RemoveAt(num);
				return true;
			}
			return false;
		}

		void IDictionary.Add(object key, object value)
		{
			VerifyKey(key);
			VerifyValueType(value);
			Add((TKey)key, (TValue)value);
		}

		private KeyList GetKeyListHelper()
		{
			if (keyList == null)
			{
				keyList = new KeyList(this);
			}
			return keyList;
		}

		private ValueList GetValueListHelper()
		{
			if (valueList == null)
			{
				valueList = new ValueList(this);
			}
			return valueList;
		}

		public void Clear()
		{
			version++;
			Array.Clear(keys, 0, _size);
			Array.Clear(values, 0, _size);
			_size = 0;
		}

		bool IDictionary.Contains(object key)
		{
			if (IsCompatibleKey(key))
			{
				return ContainsKey((TKey)key);
			}
			return false;
		}

		public bool ContainsKey(TKey key)
		{
			return IndexOfKey(key) >= 0;
		}

		public bool ContainsValue(TValue value)
		{
			return IndexOfValue(value) >= 0;
		}

		void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
		{
			if (array == null)
			{
				ThrowHelper.ThrowArgumentNullException(ExceptionArgument.array);
			}
			if (arrayIndex < 0 || arrayIndex > array.Length)
			{
				ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.arrayIndex, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
			}
			if (array.Length - arrayIndex < Count)
			{
				ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_ArrayPlusOffTooSmall);
			}
			for (int i = 0; i < Count; i++)
			{
				KeyValuePair<TKey, TValue> keyValuePair = new KeyValuePair<TKey, TValue>(keys[i], values[i]);
				array[arrayIndex + i] = keyValuePair;
			}
		}

		void ICollection.CopyTo(Array array, int arrayIndex)
		{
			if (array == null)
			{
				ThrowHelper.ThrowArgumentNullException(ExceptionArgument.array);
			}
			if (array.Rank != 1)
			{
				ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_RankMultiDimNotSupported);
			}
			if (array.GetLowerBound(0) != 0)
			{
				ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_NonZeroLowerBound);
			}
			if (arrayIndex < 0 || arrayIndex > array.Length)
			{
				ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.arrayIndex, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
			}
			if (array.Length - arrayIndex < Count)
			{
				ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_ArrayPlusOffTooSmall);
			}
			KeyValuePair<TKey, TValue>[] array2 = array as KeyValuePair<TKey, TValue>[];
			if (array2 != null)
			{
				for (int i = 0; i < Count; i++)
				{
					ref KeyValuePair<TKey, TValue> reference = ref array2[i + arrayIndex];
					reference = new KeyValuePair<TKey, TValue>(keys[i], values[i]);
				}
				return;
			}
			object[] array3 = array as object[];
			if (array3 == null)
			{
				ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_InvalidArrayType);
			}
			try
			{
				for (int j = 0; j < Count; j++)
				{
					array3[j + arrayIndex] = new KeyValuePair<TKey, TValue>(keys[j], values[j]);
				}
			}
			catch (ArrayTypeMismatchException)
			{
				ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_InvalidArrayType);
			}
		}

		private void EnsureCapacity(int min)
		{
			int num = ((keys.Length == 0) ? 4 : (keys.Length * 2));
			if (num < min)
			{
				num = min;
			}
			Capacity = num;
		}

		private TValue GetByIndex(int index)
		{
			if (index < 0 || index >= _size)
			{
				ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.index, ExceptionResource.ArgumentOutOfRange_Index);
			}
			return values[index];
		}

		public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
		{
			return new Enumerator(this, 1);
		}

		IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
		{
			return new Enumerator(this, 1);
		}

		IDictionaryEnumerator IDictionary.GetEnumerator()
		{
			return new Enumerator(this, 2);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return new Enumerator(this, 1);
		}

		private TKey GetKey(int index)
		{
			if (index < 0 || index >= _size)
			{
				ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.index, ExceptionResource.ArgumentOutOfRange_Index);
			}
			return keys[index];
		}

		public int IndexOfKey(TKey key)
		{
			if (key == null)
			{
				ThrowHelper.ThrowArgumentNullException(ExceptionArgument.key);
			}
			int num = Array.BinarySearch(keys, 0, _size, key, comparer);
			if (num < 0)
			{
				return -1;
			}
			return num;
		}

		public int IndexOfValue(TValue value)
		{
			return Array.IndexOf(values, value, 0, _size);
		}

		private void Insert(int index, TKey key, TValue value)
		{
			if (_size == keys.Length)
			{
				EnsureCapacity(_size + 1);
			}
			if (index < _size)
			{
				Array.Copy(keys, index, keys, index + 1, _size - index);
				Array.Copy(values, index, values, index + 1, _size - index);
			}
			keys[index] = key;
			values[index] = value;
			_size++;
			version++;
		}

		public bool TryGetValue(TKey key, out TValue value)
		{
			int num = IndexOfKey(key);
			if (num >= 0)
			{
				value = values[num];
				return true;
			}
			value = default(TValue);
			return false;
		}

		public void RemoveAt(int index)
		{
			if (index < 0 || index >= _size)
			{
				ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.index, ExceptionResource.ArgumentOutOfRange_Index);
			}
			_size--;
			if (index < _size)
			{
				Array.Copy(keys, index + 1, keys, index, _size - index);
				Array.Copy(values, index + 1, values, index, _size - index);
			}
			keys[_size] = default(TKey);
			values[_size] = default(TValue);
			version++;
		}

		public bool Remove(TKey key)
		{
			int num = IndexOfKey(key);
			if (num >= 0)
			{
				RemoveAt(num);
			}
			return num >= 0;
		}

		void IDictionary.Remove(object key)
		{
			if (IsCompatibleKey(key))
			{
				Remove((TKey)key);
			}
		}

		public void TrimExcess()
		{
			int num = (int)((double)keys.Length * 0.9);
			if (_size < num)
			{
				Capacity = _size;
			}
		}

		private static void VerifyKey(object key)
		{
			if (key == null)
			{
				ThrowHelper.ThrowArgumentNullException(ExceptionArgument.key);
			}
			if (!(key is TKey))
			{
				ThrowHelper.ThrowWrongKeyTypeArgumentException(key, typeof(TKey));
			}
		}

		private static bool IsCompatibleKey(object key)
		{
			if (key == null)
			{
				ThrowHelper.ThrowArgumentNullException(ExceptionArgument.key);
			}
			return key is TKey;
		}

		private static void VerifyValueType(object value)
		{
			if (!(value is TValue) && (value != null || typeof(TValue).IsValueType))
			{
				ThrowHelper.ThrowWrongValueTypeArgumentException(value, typeof(TValue));
			}
		}
	}
}
