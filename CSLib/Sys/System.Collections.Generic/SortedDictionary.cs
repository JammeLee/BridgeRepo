using System.Diagnostics;

namespace System.Collections.Generic
{
	[Serializable]
	[DebuggerTypeProxy(typeof(System_DictionaryDebugView<, >))]
	[DebuggerDisplay("Count = {Count}")]
	public class SortedDictionary<TKey, TValue> : IDictionary<TKey, TValue>, ICollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>, IDictionary, ICollection, IEnumerable
	{
		public struct Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>, IDisposable, IDictionaryEnumerator, IEnumerator
		{
			internal const int KeyValuePair = 1;

			internal const int DictEntry = 2;

			private TreeSet<KeyValuePair<TKey, TValue>>.Enumerator treeEnum;

			private int getEnumeratorRetType;

			public KeyValuePair<TKey, TValue> Current => treeEnum.Current;

			internal bool NotStartedOrEnded => treeEnum.NotStartedOrEnded;

			object IEnumerator.Current
			{
				get
				{
					if (NotStartedOrEnded)
					{
						ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_EnumOpCantHappen);
					}
					if (getEnumeratorRetType == 2)
					{
						return new DictionaryEntry(Current.Key, Current.Value);
					}
					return new KeyValuePair<TKey, TValue>(Current.Key, Current.Value);
				}
			}

			object IDictionaryEnumerator.Key
			{
				get
				{
					if (NotStartedOrEnded)
					{
						ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_EnumOpCantHappen);
					}
					return Current.Key;
				}
			}

			object IDictionaryEnumerator.Value
			{
				get
				{
					if (NotStartedOrEnded)
					{
						ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_EnumOpCantHappen);
					}
					return Current.Value;
				}
			}

			DictionaryEntry IDictionaryEnumerator.Entry
			{
				get
				{
					if (NotStartedOrEnded)
					{
						ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_EnumOpCantHappen);
					}
					return new DictionaryEntry(Current.Key, Current.Value);
				}
			}

			internal Enumerator(SortedDictionary<TKey, TValue> dictionary, int getEnumeratorRetType)
			{
				treeEnum = dictionary._set.GetEnumerator();
				this.getEnumeratorRetType = getEnumeratorRetType;
			}

			public bool MoveNext()
			{
				return treeEnum.MoveNext();
			}

			public void Dispose()
			{
				treeEnum.Dispose();
			}

			internal void Reset()
			{
				treeEnum.Reset();
			}

			void IEnumerator.Reset()
			{
				treeEnum.Reset();
			}
		}

		[Serializable]
		[DebuggerTypeProxy(typeof(System_DictionaryKeyCollectionDebugView<, >))]
		[DebuggerDisplay("Count = {Count}")]
		public sealed class KeyCollection : ICollection<TKey>, IEnumerable<TKey>, ICollection, IEnumerable
		{
			public struct Enumerator : IEnumerator<TKey>, IDisposable, IEnumerator
			{
				private SortedDictionary<TKey, TValue>.Enumerator dictEnum;

				public TKey Current => dictEnum.Current.Key;

				object IEnumerator.Current
				{
					get
					{
						if (dictEnum.NotStartedOrEnded)
						{
							ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_EnumOpCantHappen);
						}
						return Current;
					}
				}

				internal Enumerator(SortedDictionary<TKey, TValue> dictionary)
				{
					dictEnum = dictionary.GetEnumerator();
				}

				public void Dispose()
				{
					dictEnum.Dispose();
				}

				public bool MoveNext()
				{
					return dictEnum.MoveNext();
				}

				void IEnumerator.Reset()
				{
					dictEnum.Reset();
				}
			}

			private SortedDictionary<TKey, TValue> dictionary;

			public int Count => dictionary.Count;

			bool ICollection<TKey>.IsReadOnly => true;

			bool ICollection.IsSynchronized => false;

			object ICollection.SyncRoot => ((ICollection)dictionary).SyncRoot;

			public KeyCollection(SortedDictionary<TKey, TValue> dictionary)
			{
				if (dictionary == null)
				{
					ThrowHelper.ThrowArgumentNullException(ExceptionArgument.dictionary);
				}
				this.dictionary = dictionary;
			}

			public Enumerator GetEnumerator()
			{
				return new Enumerator(dictionary);
			}

			IEnumerator<TKey> IEnumerable<TKey>.GetEnumerator()
			{
				return new Enumerator(dictionary);
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return new Enumerator(dictionary);
			}

			public void CopyTo(TKey[] array, int index)
			{
				if (array == null)
				{
					ThrowHelper.ThrowArgumentNullException(ExceptionArgument.array);
				}
				if (index < 0)
				{
					ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.index);
				}
				if (array.Length - index < Count)
				{
					ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_ArrayPlusOffTooSmall);
				}
				dictionary._set.InOrderTreeWalk(delegate(TreeSet<KeyValuePair<TKey, TValue>>.Node node)
				{
					array[index++] = node.Item.Key;
					return true;
				});
			}

			void ICollection.CopyTo(Array array, int index)
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
				if (index < 0)
				{
					ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.arrayIndex, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
				}
				if (array.Length - index < dictionary.Count)
				{
					ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_ArrayPlusOffTooSmall);
				}
				TKey[] array2 = array as TKey[];
				if (array2 != null)
				{
					CopyTo(array2, index);
					return;
				}
				object[] objects = (object[])array;
				if (objects == null)
				{
					ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_InvalidArrayType);
				}
				try
				{
					dictionary._set.InOrderTreeWalk(delegate(TreeSet<KeyValuePair<TKey, TValue>>.Node node)
					{
						objects[index++] = node.Item.Key;
						return true;
					});
				}
				catch (ArrayTypeMismatchException)
				{
					ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_InvalidArrayType);
				}
			}

			void ICollection<TKey>.Add(TKey item)
			{
				ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_KeyCollectionSet);
			}

			void ICollection<TKey>.Clear()
			{
				ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_KeyCollectionSet);
			}

			bool ICollection<TKey>.Contains(TKey item)
			{
				return dictionary.ContainsKey(item);
			}

			bool ICollection<TKey>.Remove(TKey item)
			{
				ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_KeyCollectionSet);
				return false;
			}
		}

		[Serializable]
		[DebuggerDisplay("Count = {Count}")]
		[DebuggerTypeProxy(typeof(System_DictionaryValueCollectionDebugView<, >))]
		public sealed class ValueCollection : ICollection<TValue>, IEnumerable<TValue>, ICollection, IEnumerable
		{
			public struct Enumerator : IEnumerator<TValue>, IDisposable, IEnumerator
			{
				private SortedDictionary<TKey, TValue>.Enumerator dictEnum;

				public TValue Current => dictEnum.Current.Value;

				object IEnumerator.Current
				{
					get
					{
						if (dictEnum.NotStartedOrEnded)
						{
							ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_EnumOpCantHappen);
						}
						return Current;
					}
				}

				internal Enumerator(SortedDictionary<TKey, TValue> dictionary)
				{
					dictEnum = dictionary.GetEnumerator();
				}

				public void Dispose()
				{
					dictEnum.Dispose();
				}

				public bool MoveNext()
				{
					return dictEnum.MoveNext();
				}

				void IEnumerator.Reset()
				{
					dictEnum.Reset();
				}
			}

			private SortedDictionary<TKey, TValue> dictionary;

			public int Count => dictionary.Count;

			bool ICollection<TValue>.IsReadOnly => true;

			bool ICollection.IsSynchronized => false;

			object ICollection.SyncRoot => ((ICollection)dictionary).SyncRoot;

			public ValueCollection(SortedDictionary<TKey, TValue> dictionary)
			{
				if (dictionary == null)
				{
					ThrowHelper.ThrowArgumentNullException(ExceptionArgument.dictionary);
				}
				this.dictionary = dictionary;
			}

			public Enumerator GetEnumerator()
			{
				return new Enumerator(dictionary);
			}

			IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator()
			{
				return new Enumerator(dictionary);
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return new Enumerator(dictionary);
			}

			public void CopyTo(TValue[] array, int index)
			{
				if (array == null)
				{
					ThrowHelper.ThrowArgumentNullException(ExceptionArgument.array);
				}
				if (index < 0)
				{
					ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.index);
				}
				if (array.Length - index < Count)
				{
					ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_ArrayPlusOffTooSmall);
				}
				dictionary._set.InOrderTreeWalk(delegate(TreeSet<KeyValuePair<TKey, TValue>>.Node node)
				{
					array[index++] = node.Item.Value;
					return true;
				});
			}

			void ICollection.CopyTo(Array array, int index)
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
				if (index < 0)
				{
					ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.arrayIndex, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
				}
				if (array.Length - index < dictionary.Count)
				{
					ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_ArrayPlusOffTooSmall);
				}
				TValue[] array2 = array as TValue[];
				if (array2 != null)
				{
					CopyTo(array2, index);
					return;
				}
				object[] objects = (object[])array;
				if (objects == null)
				{
					ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_InvalidArrayType);
				}
				try
				{
					dictionary._set.InOrderTreeWalk(delegate(TreeSet<KeyValuePair<TKey, TValue>>.Node node)
					{
						objects[index++] = node.Item.Value;
						return true;
					});
				}
				catch (ArrayTypeMismatchException)
				{
					ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_InvalidArrayType);
				}
			}

			void ICollection<TValue>.Add(TValue item)
			{
				ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_ValueCollectionSet);
			}

			void ICollection<TValue>.Clear()
			{
				ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_ValueCollectionSet);
			}

			bool ICollection<TValue>.Contains(TValue item)
			{
				return dictionary.ContainsValue(item);
			}

			bool ICollection<TValue>.Remove(TValue item)
			{
				ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_ValueCollectionSet);
				return false;
			}
		}

		[Serializable]
		internal class KeyValuePairComparer : Comparer<KeyValuePair<TKey, TValue>>
		{
			internal IComparer<TKey> keyComparer;

			public KeyValuePairComparer(IComparer<TKey> keyComparer)
			{
				if (keyComparer == null)
				{
					this.keyComparer = Comparer<TKey>.Default;
				}
				else
				{
					this.keyComparer = keyComparer;
				}
			}

			public override int Compare(KeyValuePair<TKey, TValue> x, KeyValuePair<TKey, TValue> y)
			{
				return keyComparer.Compare(x.Key, y.Key);
			}
		}

		[NonSerialized]
		private KeyCollection keys;

		[NonSerialized]
		private ValueCollection values;

		private TreeSet<KeyValuePair<TKey, TValue>> _set;

		bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => false;

		public TValue this[TKey key]
		{
			get
			{
				if (key == null)
				{
					ThrowHelper.ThrowArgumentNullException(ExceptionArgument.key);
				}
				TreeSet<KeyValuePair<TKey, TValue>>.Node node = _set.FindNode(new KeyValuePair<TKey, TValue>(key, default(TValue)));
				if (node == null)
				{
					ThrowHelper.ThrowKeyNotFoundException();
				}
				return node.Item.Value;
			}
			set
			{
				if (key == null)
				{
					ThrowHelper.ThrowArgumentNullException(ExceptionArgument.key);
				}
				TreeSet<KeyValuePair<TKey, TValue>>.Node node = _set.FindNode(new KeyValuePair<TKey, TValue>(key, default(TValue)));
				if (node == null)
				{
					_set.Add(new KeyValuePair<TKey, TValue>(key, value));
					return;
				}
				node.Item = new KeyValuePair<TKey, TValue>(node.Item.Key, value);
				_set.UpdateVersion();
			}
		}

		public int Count => _set.Count;

		public IComparer<TKey> Comparer => ((KeyValuePairComparer)_set.Comparer).keyComparer;

		public KeyCollection Keys
		{
			get
			{
				if (keys == null)
				{
					keys = new KeyCollection(this);
				}
				return keys;
			}
		}

		ICollection<TKey> IDictionary<TKey, TValue>.Keys => Keys;

		public ValueCollection Values
		{
			get
			{
				if (values == null)
				{
					values = new ValueCollection(this);
				}
				return values;
			}
		}

		ICollection<TValue> IDictionary<TKey, TValue>.Values => Values;

		bool IDictionary.IsFixedSize => false;

		bool IDictionary.IsReadOnly => false;

		ICollection IDictionary.Keys => Keys;

		ICollection IDictionary.Values => Values;

		object IDictionary.this[object key]
		{
			get
			{
				if (IsCompatibleKey(key) && TryGetValue((TKey)key, out var value))
				{
					return value;
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

		bool ICollection.IsSynchronized => false;

		object ICollection.SyncRoot => ((ICollection)_set).SyncRoot;

		public SortedDictionary()
			: this((IComparer<TKey>)null)
		{
		}

		public SortedDictionary(IDictionary<TKey, TValue> dictionary)
			: this(dictionary, (IComparer<TKey>)null)
		{
		}

		public SortedDictionary(IDictionary<TKey, TValue> dictionary, IComparer<TKey> comparer)
		{
			if (dictionary == null)
			{
				ThrowHelper.ThrowArgumentNullException(ExceptionArgument.dictionary);
			}
			_set = new TreeSet<KeyValuePair<TKey, TValue>>(new KeyValuePairComparer(comparer));
			foreach (KeyValuePair<TKey, TValue> item in dictionary)
			{
				_set.Add(item);
			}
		}

		public SortedDictionary(IComparer<TKey> comparer)
		{
			_set = new TreeSet<KeyValuePair<TKey, TValue>>(new KeyValuePairComparer(comparer));
		}

		void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> keyValuePair)
		{
			_set.Add(keyValuePair);
		}

		bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> keyValuePair)
		{
			TreeSet<KeyValuePair<TKey, TValue>>.Node node = _set.FindNode(keyValuePair);
			if (node == null)
			{
				return false;
			}
			if (keyValuePair.Value == null)
			{
				return node.Item.Value == null;
			}
			return EqualityComparer<TValue>.Default.Equals(node.Item.Value, keyValuePair.Value);
		}

		bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> keyValuePair)
		{
			TreeSet<KeyValuePair<TKey, TValue>>.Node node = _set.FindNode(keyValuePair);
			if (node == null)
			{
				return false;
			}
			if (EqualityComparer<TValue>.Default.Equals(node.Item.Value, keyValuePair.Value))
			{
				_set.Remove(keyValuePair);
				return true;
			}
			return false;
		}

		public void Add(TKey key, TValue value)
		{
			if (key == null)
			{
				ThrowHelper.ThrowArgumentNullException(ExceptionArgument.key);
			}
			_set.Add(new KeyValuePair<TKey, TValue>(key, value));
		}

		public void Clear()
		{
			_set.Clear();
		}

		public bool ContainsKey(TKey key)
		{
			if (key == null)
			{
				ThrowHelper.ThrowArgumentNullException(ExceptionArgument.key);
			}
			return _set.Contains(new KeyValuePair<TKey, TValue>(key, default(TValue)));
		}

		public bool ContainsValue(TValue value)
		{
			bool found = false;
			if (value == null)
			{
				_set.InOrderTreeWalk(delegate(TreeSet<KeyValuePair<TKey, TValue>>.Node node)
				{
					if (node.Item.Value == null)
					{
						found = true;
						return false;
					}
					return true;
				});
			}
			else
			{
				EqualityComparer<TValue> valueComparer = EqualityComparer<TValue>.Default;
				_set.InOrderTreeWalk(delegate(TreeSet<KeyValuePair<TKey, TValue>>.Node node)
				{
					if (valueComparer.Equals(node.Item.Value, value))
					{
						found = true;
						return false;
					}
					return true;
				});
			}
			return found;
		}

		public void CopyTo(KeyValuePair<TKey, TValue>[] array, int index)
		{
			_set.CopyTo(array, index);
		}

		public Enumerator GetEnumerator()
		{
			return new Enumerator(this, 1);
		}

		IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
		{
			return new Enumerator(this, 1);
		}

		public bool Remove(TKey key)
		{
			if (key == null)
			{
				ThrowHelper.ThrowArgumentNullException(ExceptionArgument.key);
			}
			return _set.Remove(new KeyValuePair<TKey, TValue>(key, default(TValue)));
		}

		public bool TryGetValue(TKey key, out TValue value)
		{
			if (key == null)
			{
				ThrowHelper.ThrowArgumentNullException(ExceptionArgument.key);
			}
			TreeSet<KeyValuePair<TKey, TValue>>.Node node = _set.FindNode(new KeyValuePair<TKey, TValue>(key, default(TValue)));
			if (node == null)
			{
				value = default(TValue);
				return false;
			}
			value = node.Item.Value;
			return true;
		}

		void ICollection.CopyTo(Array array, int index)
		{
			((ICollection)_set).CopyTo(array, index);
		}

		void IDictionary.Add(object key, object value)
		{
			VerifyKey(key);
			VerifyValueType(value);
			Add((TKey)key, (TValue)value);
		}

		bool IDictionary.Contains(object key)
		{
			if (IsCompatibleKey(key))
			{
				return ContainsKey((TKey)key);
			}
			return false;
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

		IDictionaryEnumerator IDictionary.GetEnumerator()
		{
			return new Enumerator(this, 2);
		}

		void IDictionary.Remove(object key)
		{
			if (IsCompatibleKey(key))
			{
				Remove((TKey)key);
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return new Enumerator(this, 1);
		}
	}
}
