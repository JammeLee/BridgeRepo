using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace System.Collections.Generic
{
	[Serializable]
	[DebuggerDisplay("Count = {Count}")]
	[DebuggerTypeProxy(typeof(System_StackDebugView<>))]
	[ComVisible(false)]
	public class Stack<T> : IEnumerable<T>, ICollection, IEnumerable
	{
		[Serializable]
		public struct Enumerator : IEnumerator<T>, IDisposable, IEnumerator
		{
			private Stack<T> _stack;

			private int _index;

			private int _version;

			private T currentElement;

			public T Current
			{
				get
				{
					if (_index == -2)
					{
						ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_EnumNotStarted);
					}
					if (_index == -1)
					{
						ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_EnumEnded);
					}
					return currentElement;
				}
			}

			object IEnumerator.Current
			{
				get
				{
					if (_index == -2)
					{
						ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_EnumNotStarted);
					}
					if (_index == -1)
					{
						ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_EnumEnded);
					}
					return currentElement;
				}
			}

			internal Enumerator(Stack<T> stack)
			{
				_stack = stack;
				_version = _stack._version;
				_index = -2;
				currentElement = default(T);
			}

			public void Dispose()
			{
				_index = -1;
			}

			public bool MoveNext()
			{
				if (_version != _stack._version)
				{
					ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_EnumFailedVersion);
				}
				bool flag;
				if (_index == -2)
				{
					_index = _stack._size - 1;
					flag = _index >= 0;
					if (flag)
					{
						currentElement = _stack._array[_index];
					}
					return flag;
				}
				if (_index == -1)
				{
					return false;
				}
				flag = --_index >= 0;
				if (flag)
				{
					currentElement = _stack._array[_index];
				}
				else
				{
					currentElement = default(T);
				}
				return flag;
			}

			void IEnumerator.Reset()
			{
				if (_version != _stack._version)
				{
					ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_EnumFailedVersion);
				}
				_index = -2;
				currentElement = default(T);
			}
		}

		private const int _defaultCapacity = 4;

		private T[] _array;

		private int _size;

		private int _version;

		[NonSerialized]
		private object _syncRoot;

		private static T[] _emptyArray = new T[0];

		public int Count => _size;

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

		public Stack()
		{
			_array = _emptyArray;
			_size = 0;
			_version = 0;
		}

		public Stack(int capacity)
		{
			if (capacity < 0)
			{
				ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.capacity, ExceptionResource.ArgumentOutOfRange_NeedNonNegNumRequired);
			}
			_array = new T[capacity];
			_size = 0;
			_version = 0;
		}

		public Stack(IEnumerable<T> collection)
		{
			if (collection == null)
			{
				ThrowHelper.ThrowArgumentNullException(ExceptionArgument.collection);
			}
			ICollection<T> collection2 = collection as ICollection<T>;
			if (collection2 != null)
			{
				int count = collection2.Count;
				_array = new T[count];
				collection2.CopyTo(_array, 0);
				_size = count;
				return;
			}
			_size = 0;
			_array = new T[4];
			foreach (T item in collection)
			{
				Push(item);
			}
		}

		public void Clear()
		{
			Array.Clear(_array, 0, _size);
			_size = 0;
			_version++;
		}

		public bool Contains(T item)
		{
			int size = _size;
			EqualityComparer<T> @default = EqualityComparer<T>.Default;
			while (size-- > 0)
			{
				if (item == null)
				{
					if (_array[size] == null)
					{
						return true;
					}
				}
				else if (_array[size] != null && @default.Equals(_array[size], item))
				{
					return true;
				}
			}
			return false;
		}

		public void CopyTo(T[] array, int arrayIndex)
		{
			if (array == null)
			{
				ThrowHelper.ThrowArgumentNullException(ExceptionArgument.array);
			}
			if (arrayIndex < 0 || arrayIndex > array.Length)
			{
				ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.arrayIndex, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
			}
			if (array.Length - arrayIndex < _size)
			{
				ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_InvalidOffLen);
			}
			Array.Copy(_array, 0, array, arrayIndex, _size);
			Array.Reverse(array, arrayIndex, _size);
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
			if (array.Length - arrayIndex < _size)
			{
				ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_InvalidOffLen);
			}
			try
			{
				Array.Copy(_array, 0, array, arrayIndex, _size);
				Array.Reverse(array, arrayIndex, _size);
			}
			catch (ArrayTypeMismatchException)
			{
				ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_InvalidArrayType);
			}
		}

		public Enumerator GetEnumerator()
		{
			return new Enumerator(this);
		}

		IEnumerator<T> IEnumerable<T>.GetEnumerator()
		{
			return new Enumerator(this);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return new Enumerator(this);
		}

		public void TrimExcess()
		{
			int num = (int)((double)_array.Length * 0.9);
			if (_size < num)
			{
				T[] array = new T[_size];
				Array.Copy(_array, 0, array, 0, _size);
				_array = array;
				_version++;
			}
		}

		public T Peek()
		{
			if (_size == 0)
			{
				ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_EmptyStack);
			}
			return _array[_size - 1];
		}

		public T Pop()
		{
			if (_size == 0)
			{
				ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_EmptyStack);
			}
			_version++;
			T result = _array[--_size];
			_array[_size] = default(T);
			return result;
		}

		public void Push(T item)
		{
			if (_size == _array.Length)
			{
				T[] array = new T[(_array.Length == 0) ? 4 : (2 * _array.Length)];
				Array.Copy(_array, 0, array, 0, _size);
				_array = array;
			}
			_array[_size++] = item;
			_version++;
		}

		public T[] ToArray()
		{
			T[] array = new T[_size];
			for (int i = 0; i < _size; i++)
			{
				array[i] = _array[_size - i - 1];
			}
			return array;
		}
	}
}
