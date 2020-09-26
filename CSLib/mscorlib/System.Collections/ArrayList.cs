using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Threading;

namespace System.Collections
{
	[Serializable]
	[DebuggerDisplay("Count = {Count}")]
	[DebuggerTypeProxy(typeof(ArrayListDebugView))]
	[ComVisible(true)]
	public class ArrayList : IList, ICollection, IEnumerable, ICloneable
	{
		[Serializable]
		private class IListWrapper : ArrayList
		{
			[Serializable]
			private sealed class IListWrapperEnumWrapper : IEnumerator, ICloneable
			{
				private IEnumerator _en;

				private int _remaining;

				private int _initialStartIndex;

				private int _initialCount;

				private bool _firstCall;

				public object Current
				{
					get
					{
						if (_firstCall)
						{
							throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_EnumNotStarted"));
						}
						if (_remaining < 0)
						{
							throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_EnumEnded"));
						}
						return _en.Current;
					}
				}

				private IListWrapperEnumWrapper()
				{
				}

				internal IListWrapperEnumWrapper(IListWrapper listWrapper, int startIndex, int count)
				{
					_en = listWrapper.GetEnumerator();
					_initialStartIndex = startIndex;
					_initialCount = count;
					while (startIndex-- > 0 && _en.MoveNext())
					{
					}
					_remaining = count;
					_firstCall = true;
				}

				public object Clone()
				{
					IListWrapperEnumWrapper listWrapperEnumWrapper = new IListWrapperEnumWrapper();
					listWrapperEnumWrapper._en = (IEnumerator)((ICloneable)_en).Clone();
					listWrapperEnumWrapper._initialStartIndex = _initialStartIndex;
					listWrapperEnumWrapper._initialCount = _initialCount;
					listWrapperEnumWrapper._remaining = _remaining;
					listWrapperEnumWrapper._firstCall = _firstCall;
					return listWrapperEnumWrapper;
				}

				public bool MoveNext()
				{
					if (_firstCall)
					{
						_firstCall = false;
						if (_remaining-- > 0)
						{
							return _en.MoveNext();
						}
						return false;
					}
					if (_remaining < 0)
					{
						return false;
					}
					if (_en.MoveNext())
					{
						return _remaining-- > 0;
					}
					return false;
				}

				public void Reset()
				{
					_en.Reset();
					int initialStartIndex = _initialStartIndex;
					while (initialStartIndex-- > 0 && _en.MoveNext())
					{
					}
					_remaining = _initialCount;
					_firstCall = true;
				}
			}

			private IList _list;

			public override int Capacity
			{
				get
				{
					return _list.Count;
				}
				set
				{
					if (value < _list.Count)
					{
						throw new ArgumentOutOfRangeException("value", Environment.GetResourceString("ArgumentOutOfRange_SmallCapacity"));
					}
				}
			}

			public override int Count => _list.Count;

			public override bool IsReadOnly => _list.IsReadOnly;

			public override bool IsFixedSize => _list.IsFixedSize;

			public override bool IsSynchronized => _list.IsSynchronized;

			public override object this[int index]
			{
				get
				{
					return _list[index];
				}
				set
				{
					_list[index] = value;
					_version++;
				}
			}

			public override object SyncRoot => _list.SyncRoot;

			internal IListWrapper(IList list)
			{
				_list = list;
				_version = 0;
			}

			public override int Add(object obj)
			{
				int result = _list.Add(obj);
				_version++;
				return result;
			}

			public override void AddRange(ICollection c)
			{
				InsertRange(Count, c);
			}

			public override int BinarySearch(int index, int count, object value, IComparer comparer)
			{
				if (index < 0 || count < 0)
				{
					throw new ArgumentOutOfRangeException((index < 0) ? "index" : "count", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
				}
				if (_list.Count - index < count)
				{
					throw new ArgumentException(Environment.GetResourceString("Argument_InvalidOffLen"));
				}
				if (comparer == null)
				{
					comparer = Comparer.Default;
				}
				int num = index;
				int num2 = index + count - 1;
				while (num <= num2)
				{
					int num3 = (num + num2) / 2;
					int num4 = comparer.Compare(value, _list[num3]);
					if (num4 == 0)
					{
						return num3;
					}
					if (num4 < 0)
					{
						num2 = num3 - 1;
					}
					else
					{
						num = num3 + 1;
					}
				}
				return ~num;
			}

			public override void Clear()
			{
				if (_list.IsFixedSize)
				{
					throw new NotSupportedException(Environment.GetResourceString("NotSupported_FixedSizeCollection"));
				}
				_list.Clear();
				_version++;
			}

			public override object Clone()
			{
				return new IListWrapper(_list);
			}

			public override bool Contains(object obj)
			{
				return _list.Contains(obj);
			}

			public override void CopyTo(Array array, int index)
			{
				_list.CopyTo(array, index);
			}

			public override void CopyTo(int index, Array array, int arrayIndex, int count)
			{
				if (array == null)
				{
					throw new ArgumentNullException("array");
				}
				if (index < 0 || arrayIndex < 0)
				{
					throw new ArgumentOutOfRangeException((index < 0) ? "index" : "arrayIndex", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
				}
				if (count < 0)
				{
					throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
				}
				if (array.Length - arrayIndex < count)
				{
					throw new ArgumentException(Environment.GetResourceString("Argument_InvalidOffLen"));
				}
				if (_list.Count - index < count)
				{
					throw new ArgumentException(Environment.GetResourceString("Argument_InvalidOffLen"));
				}
				if (array.Rank != 1)
				{
					throw new ArgumentException(Environment.GetResourceString("Arg_RankMultiDimNotSupported"));
				}
				for (int i = index; i < index + count; i++)
				{
					array.SetValue(_list[i], arrayIndex++);
				}
			}

			public override IEnumerator GetEnumerator()
			{
				return _list.GetEnumerator();
			}

			public override IEnumerator GetEnumerator(int index, int count)
			{
				if (index < 0 || count < 0)
				{
					throw new ArgumentOutOfRangeException((index < 0) ? "index" : "count", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
				}
				if (_list.Count - index < count)
				{
					throw new ArgumentException(Environment.GetResourceString("Argument_InvalidOffLen"));
				}
				return new IListWrapperEnumWrapper(this, index, count);
			}

			public override int IndexOf(object value)
			{
				return _list.IndexOf(value);
			}

			public override int IndexOf(object value, int startIndex)
			{
				return IndexOf(value, startIndex, _list.Count - startIndex);
			}

			public override int IndexOf(object value, int startIndex, int count)
			{
				if (startIndex < 0 || startIndex > _list.Count)
				{
					throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("ArgumentOutOfRange_Index"));
				}
				if (count < 0 || startIndex > _list.Count - count)
				{
					throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("ArgumentOutOfRange_Count"));
				}
				int num = startIndex + count;
				if (value == null)
				{
					for (int i = startIndex; i < num; i++)
					{
						if (_list[i] == null)
						{
							return i;
						}
					}
					return -1;
				}
				for (int j = startIndex; j < num; j++)
				{
					if (_list[j] != null && _list[j].Equals(value))
					{
						return j;
					}
				}
				return -1;
			}

			public override void Insert(int index, object obj)
			{
				_list.Insert(index, obj);
				_version++;
			}

			public override void InsertRange(int index, ICollection c)
			{
				if (c == null)
				{
					throw new ArgumentNullException("c", Environment.GetResourceString("ArgumentNull_Collection"));
				}
				if (index < 0 || index > _list.Count)
				{
					throw new ArgumentOutOfRangeException("index", Environment.GetResourceString("ArgumentOutOfRange_Index"));
				}
				if (c.Count <= 0)
				{
					return;
				}
				ArrayList arrayList = _list as ArrayList;
				if (arrayList != null)
				{
					arrayList.InsertRange(index, c);
				}
				else
				{
					IEnumerator enumerator = c.GetEnumerator();
					while (enumerator.MoveNext())
					{
						_list.Insert(index++, enumerator.Current);
					}
				}
				_version++;
			}

			public override int LastIndexOf(object value)
			{
				return LastIndexOf(value, _list.Count - 1, _list.Count);
			}

			public override int LastIndexOf(object value, int startIndex)
			{
				return LastIndexOf(value, startIndex, startIndex + 1);
			}

			public override int LastIndexOf(object value, int startIndex, int count)
			{
				if (_list.Count == 0)
				{
					return -1;
				}
				if (startIndex < 0 || startIndex >= _list.Count)
				{
					throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("ArgumentOutOfRange_Index"));
				}
				if (count < 0 || count > startIndex + 1)
				{
					throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("ArgumentOutOfRange_Count"));
				}
				int num = startIndex - count + 1;
				if (value == null)
				{
					for (int num2 = startIndex; num2 >= num; num2--)
					{
						if (_list[num2] == null)
						{
							return num2;
						}
					}
					return -1;
				}
				for (int num3 = startIndex; num3 >= num; num3--)
				{
					if (_list[num3] != null && _list[num3].Equals(value))
					{
						return num3;
					}
				}
				return -1;
			}

			public override void Remove(object value)
			{
				int num = IndexOf(value);
				if (num >= 0)
				{
					RemoveAt(num);
				}
			}

			public override void RemoveAt(int index)
			{
				_list.RemoveAt(index);
				_version++;
			}

			public override void RemoveRange(int index, int count)
			{
				if (index < 0 || count < 0)
				{
					throw new ArgumentOutOfRangeException((index < 0) ? "index" : "count", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
				}
				if (_list.Count - index < count)
				{
					throw new ArgumentException(Environment.GetResourceString("Argument_InvalidOffLen"));
				}
				if (count > 0)
				{
					_version++;
				}
				while (count > 0)
				{
					_list.RemoveAt(index);
					count--;
				}
			}

			public override void Reverse(int index, int count)
			{
				if (index < 0 || count < 0)
				{
					throw new ArgumentOutOfRangeException((index < 0) ? "index" : "count", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
				}
				if (_list.Count - index < count)
				{
					throw new ArgumentException(Environment.GetResourceString("Argument_InvalidOffLen"));
				}
				int num = index;
				int num2 = index + count - 1;
				while (num < num2)
				{
					object value = _list[num];
					_list[num++] = _list[num2];
					_list[num2--] = value;
				}
				_version++;
			}

			public override void SetRange(int index, ICollection c)
			{
				if (c == null)
				{
					throw new ArgumentNullException("c", Environment.GetResourceString("ArgumentNull_Collection"));
				}
				if (index < 0 || index > _list.Count - c.Count)
				{
					throw new ArgumentOutOfRangeException("index", Environment.GetResourceString("ArgumentOutOfRange_Index"));
				}
				if (c.Count > 0)
				{
					IEnumerator enumerator = c.GetEnumerator();
					while (enumerator.MoveNext())
					{
						_list[index++] = enumerator.Current;
					}
					_version++;
				}
			}

			public override ArrayList GetRange(int index, int count)
			{
				if (index < 0 || count < 0)
				{
					throw new ArgumentOutOfRangeException((index < 0) ? "index" : "count", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
				}
				if (_list.Count - index < count)
				{
					throw new ArgumentException(Environment.GetResourceString("Argument_InvalidOffLen"));
				}
				return new Range(this, index, count);
			}

			public override void Sort(int index, int count, IComparer comparer)
			{
				if (index < 0 || count < 0)
				{
					throw new ArgumentOutOfRangeException((index < 0) ? "index" : "count", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
				}
				if (_list.Count - index < count)
				{
					throw new ArgumentException(Environment.GetResourceString("Argument_InvalidOffLen"));
				}
				object[] array = new object[count];
				CopyTo(index, array, 0, count);
				Array.Sort(array, 0, count, comparer);
				for (int i = 0; i < count; i++)
				{
					_list[i + index] = array[i];
				}
				_version++;
			}

			public override object[] ToArray()
			{
				object[] array = new object[Count];
				_list.CopyTo(array, 0);
				return array;
			}

			public override Array ToArray(Type type)
			{
				if (type == null)
				{
					throw new ArgumentNullException("type");
				}
				Array array = Array.CreateInstance(type, _list.Count);
				_list.CopyTo(array, 0);
				return array;
			}

			public override void TrimToSize()
			{
			}
		}

		[Serializable]
		private class SyncArrayList : ArrayList
		{
			private ArrayList _list;

			private object _root;

			public override int Capacity
			{
				get
				{
					lock (_root)
					{
						return _list.Capacity;
					}
				}
				set
				{
					lock (_root)
					{
						_list.Capacity = value;
					}
				}
			}

			public override int Count
			{
				get
				{
					lock (_root)
					{
						return _list.Count;
					}
				}
			}

			public override bool IsReadOnly => _list.IsReadOnly;

			public override bool IsFixedSize => _list.IsFixedSize;

			public override bool IsSynchronized => true;

			public override object this[int index]
			{
				get
				{
					lock (_root)
					{
						return _list[index];
					}
				}
				set
				{
					lock (_root)
					{
						_list[index] = value;
					}
				}
			}

			public override object SyncRoot => _root;

			internal SyncArrayList(ArrayList list)
				: base(trash: false)
			{
				_list = list;
				_root = list.SyncRoot;
			}

			public override int Add(object value)
			{
				lock (_root)
				{
					return _list.Add(value);
				}
			}

			public override void AddRange(ICollection c)
			{
				lock (_root)
				{
					_list.AddRange(c);
				}
			}

			public override int BinarySearch(object value)
			{
				lock (_root)
				{
					return _list.BinarySearch(value);
				}
			}

			public override int BinarySearch(object value, IComparer comparer)
			{
				lock (_root)
				{
					return _list.BinarySearch(value, comparer);
				}
			}

			public override int BinarySearch(int index, int count, object value, IComparer comparer)
			{
				lock (_root)
				{
					return _list.BinarySearch(index, count, value, comparer);
				}
			}

			public override void Clear()
			{
				lock (_root)
				{
					_list.Clear();
				}
			}

			public override object Clone()
			{
				lock (_root)
				{
					return new SyncArrayList((ArrayList)_list.Clone());
				}
			}

			public override bool Contains(object item)
			{
				lock (_root)
				{
					return _list.Contains(item);
				}
			}

			public override void CopyTo(Array array)
			{
				lock (_root)
				{
					_list.CopyTo(array);
				}
			}

			public override void CopyTo(Array array, int index)
			{
				lock (_root)
				{
					_list.CopyTo(array, index);
				}
			}

			public override void CopyTo(int index, Array array, int arrayIndex, int count)
			{
				lock (_root)
				{
					_list.CopyTo(index, array, arrayIndex, count);
				}
			}

			public override IEnumerator GetEnumerator()
			{
				lock (_root)
				{
					return _list.GetEnumerator();
				}
			}

			public override IEnumerator GetEnumerator(int index, int count)
			{
				lock (_root)
				{
					return _list.GetEnumerator(index, count);
				}
			}

			public override int IndexOf(object value)
			{
				lock (_root)
				{
					return _list.IndexOf(value);
				}
			}

			public override int IndexOf(object value, int startIndex)
			{
				lock (_root)
				{
					return _list.IndexOf(value, startIndex);
				}
			}

			public override int IndexOf(object value, int startIndex, int count)
			{
				lock (_root)
				{
					return _list.IndexOf(value, startIndex, count);
				}
			}

			public override void Insert(int index, object value)
			{
				lock (_root)
				{
					_list.Insert(index, value);
				}
			}

			public override void InsertRange(int index, ICollection c)
			{
				lock (_root)
				{
					_list.InsertRange(index, c);
				}
			}

			public override int LastIndexOf(object value)
			{
				lock (_root)
				{
					return _list.LastIndexOf(value);
				}
			}

			public override int LastIndexOf(object value, int startIndex)
			{
				lock (_root)
				{
					return _list.LastIndexOf(value, startIndex);
				}
			}

			public override int LastIndexOf(object value, int startIndex, int count)
			{
				lock (_root)
				{
					return _list.LastIndexOf(value, startIndex, count);
				}
			}

			public override void Remove(object value)
			{
				lock (_root)
				{
					_list.Remove(value);
				}
			}

			public override void RemoveAt(int index)
			{
				lock (_root)
				{
					_list.RemoveAt(index);
				}
			}

			public override void RemoveRange(int index, int count)
			{
				lock (_root)
				{
					_list.RemoveRange(index, count);
				}
			}

			public override void Reverse(int index, int count)
			{
				lock (_root)
				{
					_list.Reverse(index, count);
				}
			}

			public override void SetRange(int index, ICollection c)
			{
				lock (_root)
				{
					_list.SetRange(index, c);
				}
			}

			public override ArrayList GetRange(int index, int count)
			{
				lock (_root)
				{
					return _list.GetRange(index, count);
				}
			}

			public override void Sort()
			{
				lock (_root)
				{
					_list.Sort();
				}
			}

			public override void Sort(IComparer comparer)
			{
				lock (_root)
				{
					_list.Sort(comparer);
				}
			}

			public override void Sort(int index, int count, IComparer comparer)
			{
				lock (_root)
				{
					_list.Sort(index, count, comparer);
				}
			}

			public override object[] ToArray()
			{
				lock (_root)
				{
					return _list.ToArray();
				}
			}

			public override Array ToArray(Type type)
			{
				lock (_root)
				{
					return _list.ToArray(type);
				}
			}

			public override void TrimToSize()
			{
				lock (_root)
				{
					_list.TrimToSize();
				}
			}
		}

		[Serializable]
		private class SyncIList : IList, ICollection, IEnumerable
		{
			private IList _list;

			private object _root;

			public virtual int Count
			{
				get
				{
					lock (_root)
					{
						return _list.Count;
					}
				}
			}

			public virtual bool IsReadOnly => _list.IsReadOnly;

			public virtual bool IsFixedSize => _list.IsFixedSize;

			public virtual bool IsSynchronized => true;

			public virtual object this[int index]
			{
				get
				{
					lock (_root)
					{
						return _list[index];
					}
				}
				set
				{
					lock (_root)
					{
						_list[index] = value;
					}
				}
			}

			public virtual object SyncRoot => _root;

			internal SyncIList(IList list)
			{
				_list = list;
				_root = list.SyncRoot;
			}

			public virtual int Add(object value)
			{
				lock (_root)
				{
					return _list.Add(value);
				}
			}

			public virtual void Clear()
			{
				lock (_root)
				{
					_list.Clear();
				}
			}

			public virtual bool Contains(object item)
			{
				lock (_root)
				{
					return _list.Contains(item);
				}
			}

			public virtual void CopyTo(Array array, int index)
			{
				lock (_root)
				{
					_list.CopyTo(array, index);
				}
			}

			public virtual IEnumerator GetEnumerator()
			{
				lock (_root)
				{
					return _list.GetEnumerator();
				}
			}

			public virtual int IndexOf(object value)
			{
				lock (_root)
				{
					return _list.IndexOf(value);
				}
			}

			public virtual void Insert(int index, object value)
			{
				lock (_root)
				{
					_list.Insert(index, value);
				}
			}

			public virtual void Remove(object value)
			{
				lock (_root)
				{
					_list.Remove(value);
				}
			}

			public virtual void RemoveAt(int index)
			{
				lock (_root)
				{
					_list.RemoveAt(index);
				}
			}
		}

		[Serializable]
		private class FixedSizeList : IList, ICollection, IEnumerable
		{
			private IList _list;

			public virtual int Count => _list.Count;

			public virtual bool IsReadOnly => _list.IsReadOnly;

			public virtual bool IsFixedSize => true;

			public virtual bool IsSynchronized => _list.IsSynchronized;

			public virtual object this[int index]
			{
				get
				{
					return _list[index];
				}
				set
				{
					_list[index] = value;
				}
			}

			public virtual object SyncRoot => _list.SyncRoot;

			internal FixedSizeList(IList l)
			{
				_list = l;
			}

			public virtual int Add(object obj)
			{
				throw new NotSupportedException(Environment.GetResourceString("NotSupported_FixedSizeCollection"));
			}

			public virtual void Clear()
			{
				throw new NotSupportedException(Environment.GetResourceString("NotSupported_FixedSizeCollection"));
			}

			public virtual bool Contains(object obj)
			{
				return _list.Contains(obj);
			}

			public virtual void CopyTo(Array array, int index)
			{
				_list.CopyTo(array, index);
			}

			public virtual IEnumerator GetEnumerator()
			{
				return _list.GetEnumerator();
			}

			public virtual int IndexOf(object value)
			{
				return _list.IndexOf(value);
			}

			public virtual void Insert(int index, object obj)
			{
				throw new NotSupportedException(Environment.GetResourceString("NotSupported_FixedSizeCollection"));
			}

			public virtual void Remove(object value)
			{
				throw new NotSupportedException(Environment.GetResourceString("NotSupported_FixedSizeCollection"));
			}

			public virtual void RemoveAt(int index)
			{
				throw new NotSupportedException(Environment.GetResourceString("NotSupported_FixedSizeCollection"));
			}
		}

		[Serializable]
		private class FixedSizeArrayList : ArrayList
		{
			private ArrayList _list;

			public override int Count => _list.Count;

			public override bool IsReadOnly => _list.IsReadOnly;

			public override bool IsFixedSize => true;

			public override bool IsSynchronized => _list.IsSynchronized;

			public override object this[int index]
			{
				get
				{
					return _list[index];
				}
				set
				{
					_list[index] = value;
					_version = _list._version;
				}
			}

			public override object SyncRoot => _list.SyncRoot;

			public override int Capacity
			{
				get
				{
					return _list.Capacity;
				}
				set
				{
					throw new NotSupportedException(Environment.GetResourceString("NotSupported_FixedSizeCollection"));
				}
			}

			internal FixedSizeArrayList(ArrayList l)
			{
				_list = l;
				_version = _list._version;
			}

			public override int Add(object obj)
			{
				throw new NotSupportedException(Environment.GetResourceString("NotSupported_FixedSizeCollection"));
			}

			public override void AddRange(ICollection c)
			{
				throw new NotSupportedException(Environment.GetResourceString("NotSupported_FixedSizeCollection"));
			}

			public override int BinarySearch(int index, int count, object value, IComparer comparer)
			{
				return _list.BinarySearch(index, count, value, comparer);
			}

			public override void Clear()
			{
				throw new NotSupportedException(Environment.GetResourceString("NotSupported_FixedSizeCollection"));
			}

			public override object Clone()
			{
				FixedSizeArrayList fixedSizeArrayList = new FixedSizeArrayList(_list);
				fixedSizeArrayList._list = (ArrayList)_list.Clone();
				return fixedSizeArrayList;
			}

			public override bool Contains(object obj)
			{
				return _list.Contains(obj);
			}

			public override void CopyTo(Array array, int index)
			{
				_list.CopyTo(array, index);
			}

			public override void CopyTo(int index, Array array, int arrayIndex, int count)
			{
				_list.CopyTo(index, array, arrayIndex, count);
			}

			public override IEnumerator GetEnumerator()
			{
				return _list.GetEnumerator();
			}

			public override IEnumerator GetEnumerator(int index, int count)
			{
				return _list.GetEnumerator(index, count);
			}

			public override int IndexOf(object value)
			{
				return _list.IndexOf(value);
			}

			public override int IndexOf(object value, int startIndex)
			{
				return _list.IndexOf(value, startIndex);
			}

			public override int IndexOf(object value, int startIndex, int count)
			{
				return _list.IndexOf(value, startIndex, count);
			}

			public override void Insert(int index, object obj)
			{
				throw new NotSupportedException(Environment.GetResourceString("NotSupported_FixedSizeCollection"));
			}

			public override void InsertRange(int index, ICollection c)
			{
				throw new NotSupportedException(Environment.GetResourceString("NotSupported_FixedSizeCollection"));
			}

			public override int LastIndexOf(object value)
			{
				return _list.LastIndexOf(value);
			}

			public override int LastIndexOf(object value, int startIndex)
			{
				return _list.LastIndexOf(value, startIndex);
			}

			public override int LastIndexOf(object value, int startIndex, int count)
			{
				return _list.LastIndexOf(value, startIndex, count);
			}

			public override void Remove(object value)
			{
				throw new NotSupportedException(Environment.GetResourceString("NotSupported_FixedSizeCollection"));
			}

			public override void RemoveAt(int index)
			{
				throw new NotSupportedException(Environment.GetResourceString("NotSupported_FixedSizeCollection"));
			}

			public override void RemoveRange(int index, int count)
			{
				throw new NotSupportedException(Environment.GetResourceString("NotSupported_FixedSizeCollection"));
			}

			public override void SetRange(int index, ICollection c)
			{
				_list.SetRange(index, c);
				_version = _list._version;
			}

			public override ArrayList GetRange(int index, int count)
			{
				if (index < 0 || count < 0)
				{
					throw new ArgumentOutOfRangeException((index < 0) ? "index" : "count", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
				}
				if (Count - index < count)
				{
					throw new ArgumentException(Environment.GetResourceString("Argument_InvalidOffLen"));
				}
				return new Range(this, index, count);
			}

			public override void Reverse(int index, int count)
			{
				_list.Reverse(index, count);
				_version = _list._version;
			}

			public override void Sort(int index, int count, IComparer comparer)
			{
				_list.Sort(index, count, comparer);
				_version = _list._version;
			}

			public override object[] ToArray()
			{
				return _list.ToArray();
			}

			public override Array ToArray(Type type)
			{
				return _list.ToArray(type);
			}

			public override void TrimToSize()
			{
				throw new NotSupportedException(Environment.GetResourceString("NotSupported_FixedSizeCollection"));
			}
		}

		[Serializable]
		private class ReadOnlyList : IList, ICollection, IEnumerable
		{
			private IList _list;

			public virtual int Count => _list.Count;

			public virtual bool IsReadOnly => true;

			public virtual bool IsFixedSize => true;

			public virtual bool IsSynchronized => _list.IsSynchronized;

			public virtual object this[int index]
			{
				get
				{
					return _list[index];
				}
				set
				{
					throw new NotSupportedException(Environment.GetResourceString("NotSupported_ReadOnlyCollection"));
				}
			}

			public virtual object SyncRoot => _list.SyncRoot;

			internal ReadOnlyList(IList l)
			{
				_list = l;
			}

			public virtual int Add(object obj)
			{
				throw new NotSupportedException(Environment.GetResourceString("NotSupported_ReadOnlyCollection"));
			}

			public virtual void Clear()
			{
				throw new NotSupportedException(Environment.GetResourceString("NotSupported_ReadOnlyCollection"));
			}

			public virtual bool Contains(object obj)
			{
				return _list.Contains(obj);
			}

			public virtual void CopyTo(Array array, int index)
			{
				_list.CopyTo(array, index);
			}

			public virtual IEnumerator GetEnumerator()
			{
				return _list.GetEnumerator();
			}

			public virtual int IndexOf(object value)
			{
				return _list.IndexOf(value);
			}

			public virtual void Insert(int index, object obj)
			{
				throw new NotSupportedException(Environment.GetResourceString("NotSupported_ReadOnlyCollection"));
			}

			public virtual void Remove(object value)
			{
				throw new NotSupportedException(Environment.GetResourceString("NotSupported_ReadOnlyCollection"));
			}

			public virtual void RemoveAt(int index)
			{
				throw new NotSupportedException(Environment.GetResourceString("NotSupported_ReadOnlyCollection"));
			}
		}

		[Serializable]
		private class ReadOnlyArrayList : ArrayList
		{
			private ArrayList _list;

			public override int Count => _list.Count;

			public override bool IsReadOnly => true;

			public override bool IsFixedSize => true;

			public override bool IsSynchronized => _list.IsSynchronized;

			public override object this[int index]
			{
				get
				{
					return _list[index];
				}
				set
				{
					throw new NotSupportedException(Environment.GetResourceString("NotSupported_ReadOnlyCollection"));
				}
			}

			public override object SyncRoot => _list.SyncRoot;

			public override int Capacity
			{
				get
				{
					return _list.Capacity;
				}
				set
				{
					throw new NotSupportedException(Environment.GetResourceString("NotSupported_ReadOnlyCollection"));
				}
			}

			internal ReadOnlyArrayList(ArrayList l)
			{
				_list = l;
			}

			public override int Add(object obj)
			{
				throw new NotSupportedException(Environment.GetResourceString("NotSupported_ReadOnlyCollection"));
			}

			public override void AddRange(ICollection c)
			{
				throw new NotSupportedException(Environment.GetResourceString("NotSupported_ReadOnlyCollection"));
			}

			public override int BinarySearch(int index, int count, object value, IComparer comparer)
			{
				return _list.BinarySearch(index, count, value, comparer);
			}

			public override void Clear()
			{
				throw new NotSupportedException(Environment.GetResourceString("NotSupported_ReadOnlyCollection"));
			}

			public override object Clone()
			{
				ReadOnlyArrayList readOnlyArrayList = new ReadOnlyArrayList(_list);
				readOnlyArrayList._list = (ArrayList)_list.Clone();
				return readOnlyArrayList;
			}

			public override bool Contains(object obj)
			{
				return _list.Contains(obj);
			}

			public override void CopyTo(Array array, int index)
			{
				_list.CopyTo(array, index);
			}

			public override void CopyTo(int index, Array array, int arrayIndex, int count)
			{
				_list.CopyTo(index, array, arrayIndex, count);
			}

			public override IEnumerator GetEnumerator()
			{
				return _list.GetEnumerator();
			}

			public override IEnumerator GetEnumerator(int index, int count)
			{
				return _list.GetEnumerator(index, count);
			}

			public override int IndexOf(object value)
			{
				return _list.IndexOf(value);
			}

			public override int IndexOf(object value, int startIndex)
			{
				return _list.IndexOf(value, startIndex);
			}

			public override int IndexOf(object value, int startIndex, int count)
			{
				return _list.IndexOf(value, startIndex, count);
			}

			public override void Insert(int index, object obj)
			{
				throw new NotSupportedException(Environment.GetResourceString("NotSupported_ReadOnlyCollection"));
			}

			public override void InsertRange(int index, ICollection c)
			{
				throw new NotSupportedException(Environment.GetResourceString("NotSupported_ReadOnlyCollection"));
			}

			public override int LastIndexOf(object value)
			{
				return _list.LastIndexOf(value);
			}

			public override int LastIndexOf(object value, int startIndex)
			{
				return _list.LastIndexOf(value, startIndex);
			}

			public override int LastIndexOf(object value, int startIndex, int count)
			{
				return _list.LastIndexOf(value, startIndex, count);
			}

			public override void Remove(object value)
			{
				throw new NotSupportedException(Environment.GetResourceString("NotSupported_ReadOnlyCollection"));
			}

			public override void RemoveAt(int index)
			{
				throw new NotSupportedException(Environment.GetResourceString("NotSupported_ReadOnlyCollection"));
			}

			public override void RemoveRange(int index, int count)
			{
				throw new NotSupportedException(Environment.GetResourceString("NotSupported_ReadOnlyCollection"));
			}

			public override void SetRange(int index, ICollection c)
			{
				throw new NotSupportedException(Environment.GetResourceString("NotSupported_ReadOnlyCollection"));
			}

			public override ArrayList GetRange(int index, int count)
			{
				if (index < 0 || count < 0)
				{
					throw new ArgumentOutOfRangeException((index < 0) ? "index" : "count", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
				}
				if (Count - index < count)
				{
					throw new ArgumentException(Environment.GetResourceString("Argument_InvalidOffLen"));
				}
				return new Range(this, index, count);
			}

			public override void Reverse(int index, int count)
			{
				throw new NotSupportedException(Environment.GetResourceString("NotSupported_ReadOnlyCollection"));
			}

			public override void Sort(int index, int count, IComparer comparer)
			{
				throw new NotSupportedException(Environment.GetResourceString("NotSupported_ReadOnlyCollection"));
			}

			public override object[] ToArray()
			{
				return _list.ToArray();
			}

			public override Array ToArray(Type type)
			{
				return _list.ToArray(type);
			}

			public override void TrimToSize()
			{
				throw new NotSupportedException(Environment.GetResourceString("NotSupported_ReadOnlyCollection"));
			}
		}

		[Serializable]
		private sealed class ArrayListEnumerator : IEnumerator, ICloneable
		{
			private ArrayList list;

			private int index;

			private int endIndex;

			private int version;

			private object currentElement;

			private int startIndex;

			public object Current
			{
				get
				{
					if (index < startIndex)
					{
						throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_EnumNotStarted"));
					}
					if (index > endIndex)
					{
						throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_EnumEnded"));
					}
					return currentElement;
				}
			}

			internal ArrayListEnumerator(ArrayList list, int index, int count)
			{
				this.list = list;
				startIndex = index;
				this.index = index - 1;
				endIndex = this.index + count;
				version = list._version;
				currentElement = null;
			}

			public object Clone()
			{
				return MemberwiseClone();
			}

			public bool MoveNext()
			{
				if (version != list._version)
				{
					throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_EnumFailedVersion"));
				}
				if (index < endIndex)
				{
					currentElement = list[++index];
					return true;
				}
				index = endIndex + 1;
				return false;
			}

			public void Reset()
			{
				if (version != list._version)
				{
					throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_EnumFailedVersion"));
				}
				index = startIndex - 1;
			}
		}

		[Serializable]
		private class Range : ArrayList
		{
			private ArrayList _baseList;

			private int _baseIndex;

			private int _baseSize;

			private int _baseVersion;

			public override int Capacity
			{
				get
				{
					return _baseList.Capacity;
				}
				set
				{
					if (value < Count)
					{
						throw new ArgumentOutOfRangeException("value", Environment.GetResourceString("ArgumentOutOfRange_SmallCapacity"));
					}
				}
			}

			public override int Count
			{
				get
				{
					InternalUpdateRange();
					return _baseSize;
				}
			}

			public override bool IsReadOnly => _baseList.IsReadOnly;

			public override bool IsFixedSize => _baseList.IsFixedSize;

			public override bool IsSynchronized => _baseList.IsSynchronized;

			public override object SyncRoot => _baseList.SyncRoot;

			public override object this[int index]
			{
				get
				{
					InternalUpdateRange();
					if (index < 0 || index >= _baseSize)
					{
						throw new ArgumentOutOfRangeException("index", Environment.GetResourceString("ArgumentOutOfRange_Index"));
					}
					return _baseList[_baseIndex + index];
				}
				set
				{
					InternalUpdateRange();
					if (index < 0 || index >= _baseSize)
					{
						throw new ArgumentOutOfRangeException("index", Environment.GetResourceString("ArgumentOutOfRange_Index"));
					}
					_baseList[_baseIndex + index] = value;
					InternalUpdateVersion();
				}
			}

			internal Range(ArrayList list, int index, int count)
				: base(trash: false)
			{
				_baseList = list;
				_baseIndex = index;
				_baseSize = count;
				_baseVersion = list._version;
				_version = list._version;
			}

			private void InternalUpdateRange()
			{
				if (_baseVersion != _baseList._version)
				{
					throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_UnderlyingArrayListChanged"));
				}
			}

			private void InternalUpdateVersion()
			{
				_baseVersion++;
				_version++;
			}

			public override int Add(object value)
			{
				InternalUpdateRange();
				_baseList.Insert(_baseIndex + _baseSize, value);
				InternalUpdateVersion();
				return _baseSize++;
			}

			public override void AddRange(ICollection c)
			{
				InternalUpdateRange();
				if (c == null)
				{
					throw new ArgumentNullException("c");
				}
				int count = c.Count;
				if (count > 0)
				{
					_baseList.InsertRange(_baseIndex + _baseSize, c);
					InternalUpdateVersion();
					_baseSize += count;
				}
			}

			public override int BinarySearch(int index, int count, object value, IComparer comparer)
			{
				InternalUpdateRange();
				if (index < 0 || count < 0)
				{
					throw new ArgumentOutOfRangeException((index < 0) ? "index" : "count", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
				}
				if (_baseSize - index < count)
				{
					throw new ArgumentException(Environment.GetResourceString("Argument_InvalidOffLen"));
				}
				int num = _baseList.BinarySearch(_baseIndex + index, count, value, comparer);
				if (num >= 0)
				{
					return num - _baseIndex;
				}
				return num + _baseIndex;
			}

			public override void Clear()
			{
				InternalUpdateRange();
				if (_baseSize != 0)
				{
					_baseList.RemoveRange(_baseIndex, _baseSize);
					InternalUpdateVersion();
					_baseSize = 0;
				}
			}

			public override object Clone()
			{
				InternalUpdateRange();
				Range range = new Range(_baseList, _baseIndex, _baseSize);
				range._baseList = (ArrayList)_baseList.Clone();
				return range;
			}

			public override bool Contains(object item)
			{
				InternalUpdateRange();
				if (item == null)
				{
					for (int i = 0; i < _baseSize; i++)
					{
						if (_baseList[_baseIndex + i] == null)
						{
							return true;
						}
					}
					return false;
				}
				for (int j = 0; j < _baseSize; j++)
				{
					if (_baseList[_baseIndex + j] != null && _baseList[_baseIndex + j].Equals(item))
					{
						return true;
					}
				}
				return false;
			}

			public override void CopyTo(Array array, int index)
			{
				InternalUpdateRange();
				if (array == null)
				{
					throw new ArgumentNullException("array");
				}
				if (array.Rank != 1)
				{
					throw new ArgumentException(Environment.GetResourceString("Arg_RankMultiDimNotSupported"));
				}
				if (index < 0)
				{
					throw new ArgumentOutOfRangeException("index", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
				}
				if (array.Length - index < _baseSize)
				{
					throw new ArgumentException(Environment.GetResourceString("Argument_InvalidOffLen"));
				}
				_baseList.CopyTo(_baseIndex, array, index, _baseSize);
			}

			public override void CopyTo(int index, Array array, int arrayIndex, int count)
			{
				InternalUpdateRange();
				if (array == null)
				{
					throw new ArgumentNullException("array");
				}
				if (array.Rank != 1)
				{
					throw new ArgumentException(Environment.GetResourceString("Arg_RankMultiDimNotSupported"));
				}
				if (index < 0 || count < 0)
				{
					throw new ArgumentOutOfRangeException((index < 0) ? "index" : "count", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
				}
				if (array.Length - arrayIndex < count)
				{
					throw new ArgumentException(Environment.GetResourceString("Argument_InvalidOffLen"));
				}
				if (_baseSize - index < count)
				{
					throw new ArgumentException(Environment.GetResourceString("Argument_InvalidOffLen"));
				}
				_baseList.CopyTo(_baseIndex + index, array, arrayIndex, count);
			}

			public override IEnumerator GetEnumerator()
			{
				return GetEnumerator(0, _baseSize);
			}

			public override IEnumerator GetEnumerator(int index, int count)
			{
				InternalUpdateRange();
				if (index < 0 || count < 0)
				{
					throw new ArgumentOutOfRangeException((index < 0) ? "index" : "count", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
				}
				if (_baseSize - index < count)
				{
					throw new ArgumentException(Environment.GetResourceString("Argument_InvalidOffLen"));
				}
				return _baseList.GetEnumerator(_baseIndex + index, count);
			}

			public override ArrayList GetRange(int index, int count)
			{
				InternalUpdateRange();
				if (index < 0 || count < 0)
				{
					throw new ArgumentOutOfRangeException((index < 0) ? "index" : "count", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
				}
				if (_baseSize - index < count)
				{
					throw new ArgumentException(Environment.GetResourceString("Argument_InvalidOffLen"));
				}
				return new Range(this, index, count);
			}

			public override int IndexOf(object value)
			{
				InternalUpdateRange();
				int num = _baseList.IndexOf(value, _baseIndex, _baseSize);
				if (num >= 0)
				{
					return num - _baseIndex;
				}
				return -1;
			}

			public override int IndexOf(object value, int startIndex)
			{
				InternalUpdateRange();
				if (startIndex < 0)
				{
					throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
				}
				if (startIndex > _baseSize)
				{
					throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("ArgumentOutOfRange_Index"));
				}
				int num = _baseList.IndexOf(value, _baseIndex + startIndex, _baseSize - startIndex);
				if (num >= 0)
				{
					return num - _baseIndex;
				}
				return -1;
			}

			public override int IndexOf(object value, int startIndex, int count)
			{
				InternalUpdateRange();
				if (startIndex < 0 || startIndex > _baseSize)
				{
					throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("ArgumentOutOfRange_Index"));
				}
				if (count < 0 || startIndex > _baseSize - count)
				{
					throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("ArgumentOutOfRange_Count"));
				}
				int num = _baseList.IndexOf(value, _baseIndex + startIndex, count);
				if (num >= 0)
				{
					return num - _baseIndex;
				}
				return -1;
			}

			public override void Insert(int index, object value)
			{
				InternalUpdateRange();
				if (index < 0 || index > _baseSize)
				{
					throw new ArgumentOutOfRangeException("index", Environment.GetResourceString("ArgumentOutOfRange_Index"));
				}
				_baseList.Insert(_baseIndex + index, value);
				InternalUpdateVersion();
				_baseSize++;
			}

			public override void InsertRange(int index, ICollection c)
			{
				InternalUpdateRange();
				if (index < 0 || index > _baseSize)
				{
					throw new ArgumentOutOfRangeException("index", Environment.GetResourceString("ArgumentOutOfRange_Index"));
				}
				if (c == null)
				{
					throw new ArgumentNullException("c");
				}
				int count = c.Count;
				if (count > 0)
				{
					_baseList.InsertRange(_baseIndex + index, c);
					_baseSize += count;
					InternalUpdateVersion();
				}
			}

			public override int LastIndexOf(object value)
			{
				InternalUpdateRange();
				int num = _baseList.LastIndexOf(value, _baseIndex + _baseSize - 1, _baseSize);
				if (num >= 0)
				{
					return num - _baseIndex;
				}
				return -1;
			}

			public override int LastIndexOf(object value, int startIndex)
			{
				return LastIndexOf(value, startIndex, startIndex + 1);
			}

			public override int LastIndexOf(object value, int startIndex, int count)
			{
				InternalUpdateRange();
				if (_baseSize == 0)
				{
					return -1;
				}
				if (startIndex >= _baseSize)
				{
					throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("ArgumentOutOfRange_Index"));
				}
				if (startIndex < 0)
				{
					throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
				}
				int num = _baseList.LastIndexOf(value, _baseIndex + startIndex, count);
				if (num >= 0)
				{
					return num - _baseIndex;
				}
				return -1;
			}

			public override void RemoveAt(int index)
			{
				InternalUpdateRange();
				if (index < 0 || index >= _baseSize)
				{
					throw new ArgumentOutOfRangeException("index", Environment.GetResourceString("ArgumentOutOfRange_Index"));
				}
				_baseList.RemoveAt(_baseIndex + index);
				InternalUpdateVersion();
				_baseSize--;
			}

			public override void RemoveRange(int index, int count)
			{
				InternalUpdateRange();
				if (index < 0 || count < 0)
				{
					throw new ArgumentOutOfRangeException((index < 0) ? "index" : "count", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
				}
				if (_baseSize - index < count)
				{
					throw new ArgumentException(Environment.GetResourceString("Argument_InvalidOffLen"));
				}
				if (count > 0)
				{
					_baseList.RemoveRange(_baseIndex + index, count);
					InternalUpdateVersion();
					_baseSize -= count;
				}
			}

			public override void Reverse(int index, int count)
			{
				InternalUpdateRange();
				if (index < 0 || count < 0)
				{
					throw new ArgumentOutOfRangeException((index < 0) ? "index" : "count", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
				}
				if (_baseSize - index < count)
				{
					throw new ArgumentException(Environment.GetResourceString("Argument_InvalidOffLen"));
				}
				_baseList.Reverse(_baseIndex + index, count);
				InternalUpdateVersion();
			}

			public override void SetRange(int index, ICollection c)
			{
				InternalUpdateRange();
				if (index < 0 || index >= _baseSize)
				{
					throw new ArgumentOutOfRangeException("index", Environment.GetResourceString("ArgumentOutOfRange_Index"));
				}
				_baseList.SetRange(_baseIndex + index, c);
				if (c.Count > 0)
				{
					InternalUpdateVersion();
				}
			}

			public override void Sort(int index, int count, IComparer comparer)
			{
				InternalUpdateRange();
				if (index < 0 || count < 0)
				{
					throw new ArgumentOutOfRangeException((index < 0) ? "index" : "count", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
				}
				if (_baseSize - index < count)
				{
					throw new ArgumentException(Environment.GetResourceString("Argument_InvalidOffLen"));
				}
				_baseList.Sort(_baseIndex + index, count, comparer);
				InternalUpdateVersion();
			}

			public override object[] ToArray()
			{
				InternalUpdateRange();
				object[] array = new object[_baseSize];
				Array.Copy(_baseList._items, _baseIndex, array, 0, _baseSize);
				return array;
			}

			public override Array ToArray(Type type)
			{
				InternalUpdateRange();
				if (type == null)
				{
					throw new ArgumentNullException("type");
				}
				Array array = Array.CreateInstance(type, _baseSize);
				_baseList.CopyTo(_baseIndex, array, 0, _baseSize);
				return array;
			}

			public override void TrimToSize()
			{
				throw new NotSupportedException(Environment.GetResourceString("NotSupported_RangeCollection"));
			}
		}

		[Serializable]
		private sealed class ArrayListEnumeratorSimple : IEnumerator, ICloneable
		{
			private ArrayList list;

			private int index;

			private int version;

			private object currentElement;

			[NonSerialized]
			private bool isArrayList;

			private static object dummyObject = new object();

			public object Current
			{
				get
				{
					object obj = currentElement;
					if (dummyObject == obj)
					{
						if (index == -1)
						{
							throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_EnumNotStarted"));
						}
						throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_EnumEnded"));
					}
					return obj;
				}
			}

			internal ArrayListEnumeratorSimple(ArrayList list)
			{
				this.list = list;
				index = -1;
				version = list._version;
				isArrayList = list.GetType() == typeof(ArrayList);
				currentElement = dummyObject;
			}

			public object Clone()
			{
				return MemberwiseClone();
			}

			public bool MoveNext()
			{
				if (version != list._version)
				{
					throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_EnumFailedVersion"));
				}
				if (isArrayList)
				{
					if (index < list._size - 1)
					{
						currentElement = list._items[++index];
						return true;
					}
					currentElement = dummyObject;
					index = list._size;
					return false;
				}
				if (index < list.Count - 1)
				{
					currentElement = list[++index];
					return true;
				}
				index = list.Count;
				currentElement = dummyObject;
				return false;
			}

			public void Reset()
			{
				if (version != list._version)
				{
					throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_EnumFailedVersion"));
				}
				currentElement = dummyObject;
				index = -1;
			}
		}

		internal class ArrayListDebugView
		{
			private ArrayList arrayList;

			[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
			public object[] Items => arrayList.ToArray();

			public ArrayListDebugView(ArrayList arrayList)
			{
				if (arrayList == null)
				{
					throw new ArgumentNullException("arrayList");
				}
				this.arrayList = arrayList;
			}
		}

		private const int _defaultCapacity = 4;

		private object[] _items;

		private int _size;

		private int _version;

		[NonSerialized]
		private object _syncRoot;

		private static readonly object[] emptyArray = new object[0];

		public virtual int Capacity
		{
			get
			{
				return _items.Length;
			}
			set
			{
				if (value == _items.Length)
				{
					return;
				}
				if (value < _size)
				{
					throw new ArgumentOutOfRangeException("value", Environment.GetResourceString("ArgumentOutOfRange_SmallCapacity"));
				}
				if (value > 0)
				{
					object[] array = new object[value];
					if (_size > 0)
					{
						Array.Copy(_items, 0, array, 0, _size);
					}
					_items = array;
				}
				else
				{
					_items = new object[4];
				}
			}
		}

		public virtual int Count => _size;

		public virtual bool IsFixedSize => false;

		public virtual bool IsReadOnly => false;

		public virtual bool IsSynchronized => false;

		public virtual object SyncRoot
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

		public virtual object this[int index]
		{
			get
			{
				if (index < 0 || index >= _size)
				{
					throw new ArgumentOutOfRangeException("index", Environment.GetResourceString("ArgumentOutOfRange_Index"));
				}
				return _items[index];
			}
			set
			{
				if (index < 0 || index >= _size)
				{
					throw new ArgumentOutOfRangeException("index", Environment.GetResourceString("ArgumentOutOfRange_Index"));
				}
				_items[index] = value;
				_version++;
			}
		}

		internal ArrayList(bool trash)
		{
		}

		public ArrayList()
		{
			_items = emptyArray;
		}

		public ArrayList(int capacity)
		{
			if (capacity < 0)
			{
				throw new ArgumentOutOfRangeException("capacity", Environment.GetResourceString("ArgumentOutOfRange_MustBeNonNegNum", "capacity"));
			}
			_items = new object[capacity];
		}

		public ArrayList(ICollection c)
		{
			if (c == null)
			{
				throw new ArgumentNullException("c", Environment.GetResourceString("ArgumentNull_Collection"));
			}
			_items = new object[c.Count];
			AddRange(c);
		}

		public static ArrayList Adapter(IList list)
		{
			if (list == null)
			{
				throw new ArgumentNullException("list");
			}
			return new IListWrapper(list);
		}

		public virtual int Add(object value)
		{
			if (_size == _items.Length)
			{
				EnsureCapacity(_size + 1);
			}
			_items[_size] = value;
			_version++;
			return _size++;
		}

		public virtual void AddRange(ICollection c)
		{
			InsertRange(_size, c);
		}

		public virtual int BinarySearch(int index, int count, object value, IComparer comparer)
		{
			if (index < 0 || count < 0)
			{
				throw new ArgumentOutOfRangeException((index < 0) ? "index" : "count", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
			}
			if (_size - index < count)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_InvalidOffLen"));
			}
			return Array.BinarySearch(_items, index, count, value, comparer);
		}

		public virtual int BinarySearch(object value)
		{
			return BinarySearch(0, Count, value, null);
		}

		public virtual int BinarySearch(object value, IComparer comparer)
		{
			return BinarySearch(0, Count, value, comparer);
		}

		public virtual void Clear()
		{
			if (_size > 0)
			{
				Array.Clear(_items, 0, _size);
				_size = 0;
			}
			_version++;
		}

		public virtual object Clone()
		{
			ArrayList arrayList = new ArrayList(_size);
			arrayList._size = _size;
			arrayList._version = _version;
			Array.Copy(_items, 0, arrayList._items, 0, _size);
			return arrayList;
		}

		public virtual bool Contains(object item)
		{
			if (item == null)
			{
				for (int i = 0; i < _size; i++)
				{
					if (_items[i] == null)
					{
						return true;
					}
				}
				return false;
			}
			for (int j = 0; j < _size; j++)
			{
				if (_items[j] != null && _items[j].Equals(item))
				{
					return true;
				}
			}
			return false;
		}

		public virtual void CopyTo(Array array)
		{
			CopyTo(array, 0);
		}

		public virtual void CopyTo(Array array, int arrayIndex)
		{
			if (array != null && array.Rank != 1)
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_RankMultiDimNotSupported"));
			}
			Array.Copy(_items, 0, array, arrayIndex, _size);
		}

		public virtual void CopyTo(int index, Array array, int arrayIndex, int count)
		{
			if (_size - index < count)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_InvalidOffLen"));
			}
			if (array != null && array.Rank != 1)
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_RankMultiDimNotSupported"));
			}
			Array.Copy(_items, index, array, arrayIndex, count);
		}

		private void EnsureCapacity(int min)
		{
			if (_items.Length < min)
			{
				int num = ((_items.Length == 0) ? 4 : (_items.Length * 2));
				if (num < min)
				{
					num = min;
				}
				Capacity = num;
			}
		}

		public static IList FixedSize(IList list)
		{
			if (list == null)
			{
				throw new ArgumentNullException("list");
			}
			return new FixedSizeList(list);
		}

		public static ArrayList FixedSize(ArrayList list)
		{
			if (list == null)
			{
				throw new ArgumentNullException("list");
			}
			return new FixedSizeArrayList(list);
		}

		public virtual IEnumerator GetEnumerator()
		{
			return new ArrayListEnumeratorSimple(this);
		}

		public virtual IEnumerator GetEnumerator(int index, int count)
		{
			if (index < 0 || count < 0)
			{
				throw new ArgumentOutOfRangeException((index < 0) ? "index" : "count", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
			}
			if (_size - index < count)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_InvalidOffLen"));
			}
			return new ArrayListEnumerator(this, index, count);
		}

		public virtual int IndexOf(object value)
		{
			return Array.IndexOf((Array)_items, value, 0, _size);
		}

		public virtual int IndexOf(object value, int startIndex)
		{
			if (startIndex > _size)
			{
				throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("ArgumentOutOfRange_Index"));
			}
			return Array.IndexOf((Array)_items, value, startIndex, _size - startIndex);
		}

		public virtual int IndexOf(object value, int startIndex, int count)
		{
			if (startIndex > _size)
			{
				throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("ArgumentOutOfRange_Index"));
			}
			if (count < 0 || startIndex > _size - count)
			{
				throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("ArgumentOutOfRange_Count"));
			}
			return Array.IndexOf((Array)_items, value, startIndex, count);
		}

		public virtual void Insert(int index, object value)
		{
			if (index < 0 || index > _size)
			{
				throw new ArgumentOutOfRangeException("index", Environment.GetResourceString("ArgumentOutOfRange_ArrayListInsert"));
			}
			if (_size == _items.Length)
			{
				EnsureCapacity(_size + 1);
			}
			if (index < _size)
			{
				Array.Copy(_items, index, _items, index + 1, _size - index);
			}
			_items[index] = value;
			_size++;
			_version++;
		}

		public virtual void InsertRange(int index, ICollection c)
		{
			if (c == null)
			{
				throw new ArgumentNullException("c", Environment.GetResourceString("ArgumentNull_Collection"));
			}
			if (index < 0 || index > _size)
			{
				throw new ArgumentOutOfRangeException("index", Environment.GetResourceString("ArgumentOutOfRange_Index"));
			}
			int count = c.Count;
			if (count > 0)
			{
				EnsureCapacity(_size + count);
				if (index < _size)
				{
					Array.Copy(_items, index, _items, index + count, _size - index);
				}
				object[] array = new object[count];
				c.CopyTo(array, 0);
				array.CopyTo(_items, index);
				_size += count;
				_version++;
			}
		}

		public virtual int LastIndexOf(object value)
		{
			return LastIndexOf(value, _size - 1, _size);
		}

		public virtual int LastIndexOf(object value, int startIndex)
		{
			if (startIndex >= _size)
			{
				throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("ArgumentOutOfRange_Index"));
			}
			return LastIndexOf(value, startIndex, startIndex + 1);
		}

		public virtual int LastIndexOf(object value, int startIndex, int count)
		{
			if (_size == 0)
			{
				return -1;
			}
			if (startIndex < 0 || count < 0)
			{
				throw new ArgumentOutOfRangeException((startIndex < 0) ? "startIndex" : "count", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
			}
			if (startIndex >= _size || count > startIndex + 1)
			{
				throw new ArgumentOutOfRangeException((startIndex >= _size) ? "startIndex" : "count", Environment.GetResourceString("ArgumentOutOfRange_BiggerThanCollection"));
			}
			return Array.LastIndexOf((Array)_items, value, startIndex, count);
		}

		public static IList ReadOnly(IList list)
		{
			if (list == null)
			{
				throw new ArgumentNullException("list");
			}
			return new ReadOnlyList(list);
		}

		public static ArrayList ReadOnly(ArrayList list)
		{
			if (list == null)
			{
				throw new ArgumentNullException("list");
			}
			return new ReadOnlyArrayList(list);
		}

		public virtual void Remove(object obj)
		{
			int num = IndexOf(obj);
			if (num >= 0)
			{
				RemoveAt(num);
			}
		}

		public virtual void RemoveAt(int index)
		{
			if (index < 0 || index >= _size)
			{
				throw new ArgumentOutOfRangeException("index", Environment.GetResourceString("ArgumentOutOfRange_Index"));
			}
			_size--;
			if (index < _size)
			{
				Array.Copy(_items, index + 1, _items, index, _size - index);
			}
			_items[_size] = null;
			_version++;
		}

		public virtual void RemoveRange(int index, int count)
		{
			if (index < 0 || count < 0)
			{
				throw new ArgumentOutOfRangeException((index < 0) ? "index" : "count", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
			}
			if (_size - index < count)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_InvalidOffLen"));
			}
			if (count > 0)
			{
				int num = _size;
				_size -= count;
				if (index < _size)
				{
					Array.Copy(_items, index + count, _items, index, _size - index);
				}
				while (num > _size)
				{
					_items[--num] = null;
				}
				_version++;
			}
		}

		public static ArrayList Repeat(object value, int count)
		{
			if (count < 0)
			{
				throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
			}
			ArrayList arrayList = new ArrayList((count > 4) ? count : 4);
			for (int i = 0; i < count; i++)
			{
				arrayList.Add(value);
			}
			return arrayList;
		}

		public virtual void Reverse()
		{
			Reverse(0, Count);
		}

		public virtual void Reverse(int index, int count)
		{
			if (index < 0 || count < 0)
			{
				throw new ArgumentOutOfRangeException((index < 0) ? "index" : "count", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
			}
			if (_size - index < count)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_InvalidOffLen"));
			}
			Array.Reverse(_items, index, count);
			_version++;
		}

		public virtual void SetRange(int index, ICollection c)
		{
			if (c == null)
			{
				throw new ArgumentNullException("c", Environment.GetResourceString("ArgumentNull_Collection"));
			}
			int count = c.Count;
			if (index < 0 || index > _size - count)
			{
				throw new ArgumentOutOfRangeException("index", Environment.GetResourceString("ArgumentOutOfRange_Index"));
			}
			if (count > 0)
			{
				c.CopyTo(_items, index);
				_version++;
			}
		}

		public virtual ArrayList GetRange(int index, int count)
		{
			if (index < 0 || count < 0)
			{
				throw new ArgumentOutOfRangeException((index < 0) ? "index" : "count", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
			}
			if (_size - index < count)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_InvalidOffLen"));
			}
			return new Range(this, index, count);
		}

		public virtual void Sort()
		{
			Sort(0, Count, Comparer.Default);
		}

		public virtual void Sort(IComparer comparer)
		{
			Sort(0, Count, comparer);
		}

		public virtual void Sort(int index, int count, IComparer comparer)
		{
			if (index < 0 || count < 0)
			{
				throw new ArgumentOutOfRangeException((index < 0) ? "index" : "count", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
			}
			if (_size - index < count)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_InvalidOffLen"));
			}
			Array.Sort(_items, index, count, comparer);
			_version++;
		}

		[HostProtection(SecurityAction.LinkDemand, Synchronization = true)]
		public static IList Synchronized(IList list)
		{
			if (list == null)
			{
				throw new ArgumentNullException("list");
			}
			return new SyncIList(list);
		}

		[HostProtection(SecurityAction.LinkDemand, Synchronization = true)]
		public static ArrayList Synchronized(ArrayList list)
		{
			if (list == null)
			{
				throw new ArgumentNullException("list");
			}
			return new SyncArrayList(list);
		}

		public virtual object[] ToArray()
		{
			object[] array = new object[_size];
			Array.Copy(_items, 0, array, 0, _size);
			return array;
		}

		public virtual Array ToArray(Type type)
		{
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}
			Array array = Array.CreateInstance(type, _size);
			Array.Copy(_items, 0, array, 0, _size);
			return array;
		}

		public virtual void TrimToSize()
		{
			Capacity = _size;
		}
	}
}
