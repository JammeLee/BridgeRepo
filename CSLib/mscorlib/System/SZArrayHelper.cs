using System.Collections;
using System.Collections.Generic;

namespace System
{
	internal sealed class SZArrayHelper
	{
		[Serializable]
		private sealed class SZGenericArrayEnumerator<T> : IEnumerator<T>, IDisposable, IEnumerator
		{
			private T[] _array;

			private int _index;

			private int _endIndex;

			public T Current
			{
				get
				{
					if (_index < 0)
					{
						throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_EnumNotStarted"));
					}
					if (_index >= _endIndex)
					{
						throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_EnumEnded"));
					}
					return _array[_index];
				}
			}

			object IEnumerator.Current => Current;

			internal SZGenericArrayEnumerator(T[] array)
			{
				_array = array;
				_index = -1;
				_endIndex = array.Length;
			}

			public bool MoveNext()
			{
				if (_index < _endIndex)
				{
					_index++;
					return _index < _endIndex;
				}
				return false;
			}

			void IEnumerator.Reset()
			{
				_index = -1;
			}

			public void Dispose()
			{
			}
		}

		private SZArrayHelper()
		{
		}

		internal IEnumerator<T> GetEnumerator<T>()
		{
			return new SZGenericArrayEnumerator<T>(this as T[]);
		}

		private void CopyTo<T>(T[] array, int index)
		{
			if (array != null && array.Rank != 1)
			{
				throw new ArgumentException(Environment.GetResourceString("Rank_MultiDimNotSupported"));
			}
			T[] array2 = this as T[];
			Array.Copy(array2, 0, array, index, array2.Length);
		}

		internal int get_Count<T>()
		{
			T[] array = this as T[];
			return array.Length;
		}

		internal T get_Item<T>(int index)
		{
			T[] array = this as T[];
			if ((uint)index >= (uint)array.Length)
			{
				ThrowHelper.ThrowArgumentOutOfRangeException();
			}
			return array[index];
		}

		internal void set_Item<T>(int index, T value)
		{
			T[] array = this as T[];
			if ((uint)index >= (uint)array.Length)
			{
				ThrowHelper.ThrowArgumentOutOfRangeException();
			}
			array[index] = value;
		}

		private void Add<T>(T value)
		{
			throw new NotSupportedException(Environment.GetResourceString("NotSupported_FixedSizeCollection"));
		}

		private bool Contains<T>(T value)
		{
			T[] array = this as T[];
			return Array.IndexOf(array, value) != -1;
		}

		private bool get_IsReadOnly<T>()
		{
			return true;
		}

		private void Clear<T>()
		{
			throw new NotSupportedException(Environment.GetResourceString("NotSupported_ReadOnlyCollection"));
		}

		private int IndexOf<T>(T value)
		{
			T[] array = this as T[];
			return Array.IndexOf(array, value);
		}

		private void Insert<T>(int index, T value)
		{
			throw new NotSupportedException(Environment.GetResourceString("NotSupported_FixedSizeCollection"));
		}

		private bool Remove<T>(T value)
		{
			throw new NotSupportedException(Environment.GetResourceString("NotSupported_FixedSizeCollection"));
		}

		private void RemoveAt<T>(int index)
		{
			throw new NotSupportedException(Environment.GetResourceString("NotSupported_FixedSizeCollection"));
		}
	}
}
