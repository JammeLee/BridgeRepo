namespace System
{
	[Serializable]
	public struct ArraySegment<T>
	{
		private T[] _array;

		private int _offset;

		private int _count;

		public T[] Array => _array;

		public int Offset => _offset;

		public int Count => _count;

		public ArraySegment(T[] array)
		{
			if (array == null)
			{
				throw new ArgumentNullException("array");
			}
			_array = array;
			_offset = 0;
			_count = array.Length;
		}

		public ArraySegment(T[] array, int offset, int count)
		{
			if (array == null)
			{
				throw new ArgumentNullException("array");
			}
			if (offset < 0)
			{
				throw new ArgumentOutOfRangeException("offset", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
			}
			if (count < 0)
			{
				throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
			}
			if (array.Length - offset < count)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_InvalidOffLen"));
			}
			_array = array;
			_offset = offset;
			_count = count;
		}

		public override int GetHashCode()
		{
			return _array.GetHashCode() ^ _offset ^ _count;
		}

		public override bool Equals(object obj)
		{
			if (obj is ArraySegment<T>)
			{
				return Equals((ArraySegment<T>)obj);
			}
			return false;
		}

		public bool Equals(ArraySegment<T> obj)
		{
			if (obj._array == _array && obj._offset == _offset)
			{
				return obj._count == _count;
			}
			return false;
		}

		public static bool operator ==(ArraySegment<T> a, ArraySegment<T> b)
		{
			return a.Equals(b);
		}

		public static bool operator !=(ArraySegment<T> a, ArraySegment<T> b)
		{
			return !(a == b);
		}
	}
}
