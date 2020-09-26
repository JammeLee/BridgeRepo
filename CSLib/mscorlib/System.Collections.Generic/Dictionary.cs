using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Threading;

namespace System.Collections.Generic
{
	[Serializable]
	[DebuggerDisplay("Count = {Count}")]
	[ComVisible(false)]
	[DebuggerTypeProxy(typeof(Mscorlib_DictionaryDebugView<, >))]
	public class Dictionary<TKey, TValue> : IDictionary<TKey, TValue>, ICollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>, IDictionary, ICollection, IEnumerable, ISerializable, IDeserializationCallback
	{
		private struct Entry
		{
			public int hashCode;

			public int next;

			public TKey key;

			public TValue value;
		}

		[Serializable]
		public struct Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>, IDisposable, IDictionaryEnumerator, IEnumerator
		{
			internal const int DictEntry = 1;

			internal const int KeyValuePair = 2;

			private Dictionary<TKey, TValue> dictionary;

			private int version;

			private int index;

			private KeyValuePair<TKey, TValue> current;

			private int getEnumeratorRetType;

			public KeyValuePair<TKey, TValue> Current => current;

			object IEnumerator.Current
			{
				get
				{
					if (index == 0 || index == dictionary.count + 1)
					{
						ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_EnumOpCantHappen);
					}
					if (getEnumeratorRetType == 1)
					{
						return new DictionaryEntry(current.Key, current.Value);
					}
					return new KeyValuePair<TKey, TValue>(current.Key, current.Value);
				}
			}

			DictionaryEntry IDictionaryEnumerator.Entry
			{
				get
				{
					if (index == 0 || index == dictionary.count + 1)
					{
						ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_EnumOpCantHappen);
					}
					return new DictionaryEntry(current.Key, current.Value);
				}
			}

			object IDictionaryEnumerator.Key
			{
				get
				{
					if (index == 0 || index == dictionary.count + 1)
					{
						ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_EnumOpCantHappen);
					}
					return current.Key;
				}
			}

			object IDictionaryEnumerator.Value
			{
				get
				{
					if (index == 0 || index == dictionary.count + 1)
					{
						ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_EnumOpCantHappen);
					}
					return current.Value;
				}
			}

			internal Enumerator(Dictionary<TKey, TValue> dictionary, int getEnumeratorRetType)
			{
				this.dictionary = dictionary;
				version = dictionary.version;
				index = 0;
				this.getEnumeratorRetType = getEnumeratorRetType;
				current = default(KeyValuePair<TKey, TValue>);
			}

			public bool MoveNext()
			{
				if (version != dictionary.version)
				{
					ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_EnumFailedVersion);
				}
				while ((uint)index < (uint)dictionary.count)
				{
					if (dictionary.entries[index].hashCode >= 0)
					{
						current = new KeyValuePair<TKey, TValue>(dictionary.entries[index].key, dictionary.entries[index].value);
						index++;
						return true;
					}
					index++;
				}
				index = dictionary.count + 1;
				current = default(KeyValuePair<TKey, TValue>);
				return false;
			}

			public void Dispose()
			{
			}

			void IEnumerator.Reset()
			{
				if (version != dictionary.version)
				{
					ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_EnumFailedVersion);
				}
				index = 0;
				current = default(KeyValuePair<TKey, TValue>);
			}
		}

		[Serializable]
		[DebuggerDisplay("Count = {Count}")]
		[DebuggerTypeProxy(typeof(Mscorlib_DictionaryKeyCollectionDebugView<, >))]
		public sealed class KeyCollection : ICollection<TKey>, IEnumerable<TKey>, ICollection, IEnumerable
		{
			[Serializable]
			public struct Enumerator : IEnumerator<TKey>, IDisposable, IEnumerator
			{
				private Dictionary<TKey, TValue> dictionary;

				private int index;

				private int version;

				private TKey currentKey;

				public TKey Current => currentKey;

				object IEnumerator.Current
				{
					get
					{
						if (index == 0 || index == dictionary.count + 1)
						{
							ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_EnumOpCantHappen);
						}
						return currentKey;
					}
				}

				internal Enumerator(Dictionary<TKey, TValue> dictionary)
				{
					this.dictionary = dictionary;
					version = dictionary.version;
					index = 0;
					currentKey = default(TKey);
				}

				public void Dispose()
				{
				}

				public bool MoveNext()
				{
					if (version != dictionary.version)
					{
						ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_EnumFailedVersion);
					}
					while ((uint)index < (uint)dictionary.count)
					{
						if (dictionary.entries[index].hashCode >= 0)
						{
							currentKey = dictionary.entries[index].key;
							index++;
							return true;
						}
						index++;
					}
					index = dictionary.count + 1;
					currentKey = default(TKey);
					return false;
				}

				void IEnumerator.Reset()
				{
					if (version != dictionary.version)
					{
						ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_EnumFailedVersion);
					}
					index = 0;
					currentKey = default(TKey);
				}
			}

			private Dictionary<TKey, TValue> dictionary;

			public int Count => dictionary.Count;

			bool ICollection<TKey>.IsReadOnly => true;

			bool ICollection.IsSynchronized => false;

			object ICollection.SyncRoot => ((ICollection)dictionary).SyncRoot;

			public KeyCollection(Dictionary<TKey, TValue> dictionary)
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

			public void CopyTo(TKey[] array, int index)
			{
				if (array == null)
				{
					ThrowHelper.ThrowArgumentNullException(ExceptionArgument.array);
				}
				if (index < 0 || index > array.Length)
				{
					ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.index, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
				}
				if (array.Length - index < dictionary.Count)
				{
					ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_ArrayPlusOffTooSmall);
				}
				int count = dictionary.count;
				Entry[] entries = dictionary.entries;
				for (int i = 0; i < count; i++)
				{
					if (entries[i].hashCode >= 0)
					{
						array[index++] = entries[i].key;
					}
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

			IEnumerator<TKey> IEnumerable<TKey>.GetEnumerator()
			{
				return new Enumerator(dictionary);
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return new Enumerator(dictionary);
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
				if (index < 0 || index > array.Length)
				{
					ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.index, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
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
				object[] array3 = array as object[];
				if (array3 == null)
				{
					ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_InvalidArrayType);
				}
				int count = dictionary.count;
				Entry[] entries = dictionary.entries;
				try
				{
					for (int i = 0; i < count; i++)
					{
						if (entries[i].hashCode >= 0)
						{
							array3[index++] = entries[i].key;
						}
					}
				}
				catch (ArrayTypeMismatchException)
				{
					ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_InvalidArrayType);
				}
			}
		}

		[Serializable]
		[DebuggerTypeProxy(typeof(Mscorlib_DictionaryValueCollectionDebugView<, >))]
		[DebuggerDisplay("Count = {Count}")]
		public sealed class ValueCollection : ICollection<TValue>, IEnumerable<TValue>, ICollection, IEnumerable
		{
			[Serializable]
			public struct Enumerator : IEnumerator<TValue>, IDisposable, IEnumerator
			{
				private Dictionary<TKey, TValue> dictionary;

				private int index;

				private int version;

				private TValue currentValue;

				public TValue Current => currentValue;

				object IEnumerator.Current
				{
					get
					{
						if (index == 0 || index == dictionary.count + 1)
						{
							ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_EnumOpCantHappen);
						}
						return currentValue;
					}
				}

				internal Enumerator(Dictionary<TKey, TValue> dictionary)
				{
					this.dictionary = dictionary;
					version = dictionary.version;
					index = 0;
					currentValue = default(TValue);
				}

				public void Dispose()
				{
				}

				public bool MoveNext()
				{
					if (version != dictionary.version)
					{
						ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_EnumFailedVersion);
					}
					while ((uint)index < (uint)dictionary.count)
					{
						if (dictionary.entries[index].hashCode >= 0)
						{
							currentValue = dictionary.entries[index].value;
							index++;
							return true;
						}
						index++;
					}
					index = dictionary.count + 1;
					currentValue = default(TValue);
					return false;
				}

				void IEnumerator.Reset()
				{
					if (version != dictionary.version)
					{
						ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_EnumFailedVersion);
					}
					index = 0;
					currentValue = default(TValue);
				}
			}

			private Dictionary<TKey, TValue> dictionary;

			public int Count => dictionary.Count;

			bool ICollection<TValue>.IsReadOnly => true;

			bool ICollection.IsSynchronized => false;

			object ICollection.SyncRoot => ((ICollection)dictionary).SyncRoot;

			public ValueCollection(Dictionary<TKey, TValue> dictionary)
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

			public void CopyTo(TValue[] array, int index)
			{
				if (array == null)
				{
					ThrowHelper.ThrowArgumentNullException(ExceptionArgument.array);
				}
				if (index < 0 || index > array.Length)
				{
					ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.index, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
				}
				if (array.Length - index < dictionary.Count)
				{
					ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_ArrayPlusOffTooSmall);
				}
				int count = dictionary.count;
				Entry[] entries = dictionary.entries;
				for (int i = 0; i < count; i++)
				{
					if (entries[i].hashCode >= 0)
					{
						array[index++] = entries[i].value;
					}
				}
			}

			void ICollection<TValue>.Add(TValue item)
			{
				ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_ValueCollectionSet);
			}

			bool ICollection<TValue>.Remove(TValue item)
			{
				ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_ValueCollectionSet);
				return false;
			}

			void ICollection<TValue>.Clear()
			{
				ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_ValueCollectionSet);
			}

			bool ICollection<TValue>.Contains(TValue item)
			{
				return dictionary.ContainsValue(item);
			}

			IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator()
			{
				return new Enumerator(dictionary);
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return new Enumerator(dictionary);
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
				if (index < 0 || index > array.Length)
				{
					ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.index, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
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
				object[] array3 = array as object[];
				if (array3 == null)
				{
					ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_InvalidArrayType);
				}
				int count = dictionary.count;
				Entry[] entries = dictionary.entries;
				try
				{
					for (int i = 0; i < count; i++)
					{
						if (entries[i].hashCode >= 0)
						{
							array3[index++] = entries[i].value;
						}
					}
				}
				catch (ArrayTypeMismatchException)
				{
					ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_InvalidArrayType);
				}
			}
		}

		private const string VersionName = "Version";

		private const string HashSizeName = "HashSize";

		private const string KeyValuePairsName = "KeyValuePairs";

		private const string ComparerName = "Comparer";

		private int[] buckets;

		private Entry[] entries;

		private int count;

		private int version;

		private int freeList;

		private int freeCount;

		private IEqualityComparer<TKey> comparer;

		private KeyCollection keys;

		private ValueCollection values;

		private object _syncRoot;

		private SerializationInfo m_siInfo;

		public IEqualityComparer<TKey> Comparer => comparer;

		public int Count => count - freeCount;

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

		ICollection<TKey> IDictionary<TKey, TValue>.Keys
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

		ICollection<TValue> IDictionary<TKey, TValue>.Values
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

		public TValue this[TKey key]
		{
			get
			{
				int num = FindEntry(key);
				if (num >= 0)
				{
					return entries[num].value;
				}
				ThrowHelper.ThrowKeyNotFoundException();
				return default(TValue);
			}
			set
			{
				Insert(key, value, add: false);
			}
		}

		bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => false;

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

		bool IDictionary.IsFixedSize => false;

		bool IDictionary.IsReadOnly => false;

		ICollection IDictionary.Keys => Keys;

		ICollection IDictionary.Values => Values;

		object IDictionary.this[object key]
		{
			get
			{
				if (IsCompatibleKey(key))
				{
					int num = FindEntry((TKey)key);
					if (num >= 0)
					{
						return entries[num].value;
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

		public Dictionary()
			: this(0, (IEqualityComparer<TKey>)null)
		{
		}

		public Dictionary(int capacity)
			: this(capacity, (IEqualityComparer<TKey>)null)
		{
		}

		public Dictionary(IEqualityComparer<TKey> comparer)
			: this(0, comparer)
		{
		}

		public Dictionary(int capacity, IEqualityComparer<TKey> comparer)
		{
			if (capacity < 0)
			{
				ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.capacity);
			}
			if (capacity > 0)
			{
				Initialize(capacity);
			}
			if (comparer == null)
			{
				comparer = EqualityComparer<TKey>.Default;
			}
			this.comparer = comparer;
		}

		public Dictionary(IDictionary<TKey, TValue> dictionary)
			: this(dictionary, (IEqualityComparer<TKey>)null)
		{
		}

		public Dictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer)
			: this(dictionary?.Count ?? 0, comparer)
		{
			if (dictionary == null)
			{
				ThrowHelper.ThrowArgumentNullException(ExceptionArgument.dictionary);
			}
			foreach (KeyValuePair<TKey, TValue> item in dictionary)
			{
				Add(item.Key, item.Value);
			}
		}

		protected Dictionary(SerializationInfo info, StreamingContext context)
		{
			m_siInfo = info;
		}

		public void Add(TKey key, TValue value)
		{
			Insert(key, value, add: true);
		}

		void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> keyValuePair)
		{
			Add(keyValuePair.Key, keyValuePair.Value);
		}

		bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> keyValuePair)
		{
			int num = FindEntry(keyValuePair.Key);
			if (num >= 0 && EqualityComparer<TValue>.Default.Equals(entries[num].value, keyValuePair.Value))
			{
				return true;
			}
			return false;
		}

		bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> keyValuePair)
		{
			int num = FindEntry(keyValuePair.Key);
			if (num >= 0 && EqualityComparer<TValue>.Default.Equals(entries[num].value, keyValuePair.Value))
			{
				Remove(keyValuePair.Key);
				return true;
			}
			return false;
		}

		public void Clear()
		{
			if (count > 0)
			{
				for (int i = 0; i < buckets.Length; i++)
				{
					buckets[i] = -1;
				}
				Array.Clear(entries, 0, count);
				freeList = -1;
				count = 0;
				freeCount = 0;
				version++;
			}
		}

		public bool ContainsKey(TKey key)
		{
			return FindEntry(key) >= 0;
		}

		public bool ContainsValue(TValue value)
		{
			if (value == null)
			{
				for (int i = 0; i < count; i++)
				{
					if (entries[i].hashCode >= 0 && entries[i].value == null)
					{
						return true;
					}
				}
			}
			else
			{
				EqualityComparer<TValue> @default = EqualityComparer<TValue>.Default;
				for (int j = 0; j < count; j++)
				{
					if (entries[j].hashCode >= 0 && @default.Equals(entries[j].value, value))
					{
						return true;
					}
				}
			}
			return false;
		}

		private void CopyTo(KeyValuePair<TKey, TValue>[] array, int index)
		{
			if (array == null)
			{
				ThrowHelper.ThrowArgumentNullException(ExceptionArgument.array);
			}
			if (index < 0 || index > array.Length)
			{
				ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.index, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
			}
			if (array.Length - index < Count)
			{
				ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_ArrayPlusOffTooSmall);
			}
			int num = count;
			Entry[] array2 = entries;
			for (int i = 0; i < num; i++)
			{
				if (array2[i].hashCode >= 0)
				{
					ref KeyValuePair<TKey, TValue> reference = ref array[index++];
					reference = new KeyValuePair<TKey, TValue>(array2[i].key, array2[i].value);
				}
			}
		}

		public Enumerator GetEnumerator()
		{
			return new Enumerator(this, 2);
		}

		IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
		{
			return new Enumerator(this, 2);
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
		public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
			{
				ThrowHelper.ThrowArgumentNullException(ExceptionArgument.info);
			}
			info.AddValue("Version", version);
			info.AddValue("Comparer", comparer, typeof(IEqualityComparer<TKey>));
			info.AddValue("HashSize", (buckets != null) ? buckets.Length : 0);
			if (buckets != null)
			{
				KeyValuePair<TKey, TValue>[] array = new KeyValuePair<TKey, TValue>[Count];
				CopyTo(array, 0);
				info.AddValue("KeyValuePairs", array, typeof(KeyValuePair<TKey, TValue>[]));
			}
		}

		private int FindEntry(TKey key)
		{
			if (key == null)
			{
				ThrowHelper.ThrowArgumentNullException(ExceptionArgument.key);
			}
			if (buckets != null)
			{
				int num = comparer.GetHashCode(key) & 0x7FFFFFFF;
				for (int num2 = buckets[num % buckets.Length]; num2 >= 0; num2 = entries[num2].next)
				{
					if (entries[num2].hashCode == num && comparer.Equals(entries[num2].key, key))
					{
						return num2;
					}
				}
			}
			return -1;
		}

		private void Initialize(int capacity)
		{
			int prime = HashHelpers.GetPrime(capacity);
			buckets = new int[prime];
			for (int i = 0; i < buckets.Length; i++)
			{
				buckets[i] = -1;
			}
			entries = new Entry[prime];
			freeList = -1;
		}

		private void Insert(TKey key, TValue value, bool add)
		{
			if (key == null)
			{
				ThrowHelper.ThrowArgumentNullException(ExceptionArgument.key);
			}
			if (buckets == null)
			{
				Initialize(0);
			}
			int num = comparer.GetHashCode(key) & 0x7FFFFFFF;
			int num2 = num % buckets.Length;
			for (int num3 = buckets[num2]; num3 >= 0; num3 = entries[num3].next)
			{
				if (entries[num3].hashCode == num && comparer.Equals(entries[num3].key, key))
				{
					if (add)
					{
						ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_AddingDuplicate);
					}
					entries[num3].value = value;
					version++;
					return;
				}
			}
			int num4;
			if (freeCount > 0)
			{
				num4 = freeList;
				freeList = entries[num4].next;
				freeCount--;
			}
			else
			{
				if (count == entries.Length)
				{
					Resize();
					num2 = num % buckets.Length;
				}
				num4 = count;
				count++;
			}
			entries[num4].hashCode = num;
			entries[num4].next = buckets[num2];
			entries[num4].key = key;
			entries[num4].value = value;
			buckets[num2] = num4;
			version++;
		}

		public virtual void OnDeserialization(object sender)
		{
			if (m_siInfo == null)
			{
				return;
			}
			int @int = m_siInfo.GetInt32("Version");
			int int2 = m_siInfo.GetInt32("HashSize");
			comparer = (IEqualityComparer<TKey>)m_siInfo.GetValue("Comparer", typeof(IEqualityComparer<TKey>));
			if (int2 != 0)
			{
				buckets = new int[int2];
				for (int i = 0; i < buckets.Length; i++)
				{
					buckets[i] = -1;
				}
				entries = new Entry[int2];
				freeList = -1;
				KeyValuePair<TKey, TValue>[] array = (KeyValuePair<TKey, TValue>[])m_siInfo.GetValue("KeyValuePairs", typeof(KeyValuePair<TKey, TValue>[]));
				if (array == null)
				{
					ThrowHelper.ThrowSerializationException(ExceptionResource.Serialization_MissingKeyValuePairs);
				}
				for (int j = 0; j < array.Length; j++)
				{
					if (array[j].Key == null)
					{
						ThrowHelper.ThrowSerializationException(ExceptionResource.Serialization_NullKey);
					}
					Insert(array[j].Key, array[j].Value, add: true);
				}
			}
			else
			{
				buckets = null;
			}
			version = @int;
			m_siInfo = null;
		}

		private void Resize()
		{
			int prime = HashHelpers.GetPrime(count * 2);
			int[] array = new int[prime];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = -1;
			}
			Entry[] array2 = new Entry[prime];
			Array.Copy(entries, 0, array2, 0, count);
			for (int j = 0; j < count; j++)
			{
				int num = array2[j].hashCode % prime;
				array2[j].next = array[num];
				array[num] = j;
			}
			buckets = array;
			entries = array2;
		}

		public bool Remove(TKey key)
		{
			if (key == null)
			{
				ThrowHelper.ThrowArgumentNullException(ExceptionArgument.key);
			}
			if (buckets != null)
			{
				int num = comparer.GetHashCode(key) & 0x7FFFFFFF;
				int num2 = num % buckets.Length;
				int num3 = -1;
				for (int num4 = buckets[num2]; num4 >= 0; num4 = entries[num4].next)
				{
					if (entries[num4].hashCode == num && comparer.Equals(entries[num4].key, key))
					{
						if (num3 < 0)
						{
							buckets[num2] = entries[num4].next;
						}
						else
						{
							entries[num3].next = entries[num4].next;
						}
						entries[num4].hashCode = -1;
						entries[num4].next = freeList;
						entries[num4].key = default(TKey);
						entries[num4].value = default(TValue);
						freeList = num4;
						freeCount++;
						version++;
						return true;
					}
					num3 = num4;
				}
			}
			return false;
		}

		public bool TryGetValue(TKey key, out TValue value)
		{
			int num = FindEntry(key);
			if (num >= 0)
			{
				value = entries[num].value;
				return true;
			}
			value = default(TValue);
			return false;
		}

		void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int index)
		{
			CopyTo(array, index);
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
			if (index < 0 || index > array.Length)
			{
				ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.index, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
			}
			if (array.Length - index < Count)
			{
				ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_ArrayPlusOffTooSmall);
			}
			KeyValuePair<TKey, TValue>[] array2 = array as KeyValuePair<TKey, TValue>[];
			if (array2 != null)
			{
				CopyTo(array2, index);
				return;
			}
			if (array is DictionaryEntry[])
			{
				DictionaryEntry[] array3 = array as DictionaryEntry[];
				Entry[] array4 = entries;
				for (int i = 0; i < count; i++)
				{
					if (array4[i].hashCode >= 0)
					{
						ref DictionaryEntry reference = ref array3[index++];
						reference = new DictionaryEntry(array4[i].key, array4[i].value);
					}
				}
				return;
			}
			object[] array5 = array as object[];
			if (array5 == null)
			{
				ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_InvalidArrayType);
			}
			try
			{
				int num = count;
				Entry[] array6 = entries;
				for (int j = 0; j < num; j++)
				{
					if (array6[j].hashCode >= 0)
					{
						array5[index++] = new KeyValuePair<TKey, TValue>(array6[j].key, array6[j].value);
					}
				}
			}
			catch (ArrayTypeMismatchException)
			{
				ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_InvalidArrayType);
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return new Enumerator(this, 2);
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

		IDictionaryEnumerator IDictionary.GetEnumerator()
		{
			return new Enumerator(this, 1);
		}

		void IDictionary.Remove(object key)
		{
			if (IsCompatibleKey(key))
			{
				Remove((TKey)key);
			}
		}
	}
}
