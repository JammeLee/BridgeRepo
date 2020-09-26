using System.Runtime.InteropServices;
using System.Threading;

namespace System.Collections
{
	[Serializable]
	[ComVisible(true)]
	public sealed class BitArray : ICollection, IEnumerable, ICloneable
	{
		[Serializable]
		private class BitArrayEnumeratorSimple : IEnumerator, ICloneable
		{
			private BitArray bitarray;

			private int index;

			private int version;

			private bool currentElement;

			public virtual object Current
			{
				get
				{
					if (index == -1)
					{
						throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_EnumNotStarted"));
					}
					if (index >= bitarray.Count)
					{
						throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_EnumEnded"));
					}
					return currentElement;
				}
			}

			internal BitArrayEnumeratorSimple(BitArray bitarray)
			{
				this.bitarray = bitarray;
				index = -1;
				version = bitarray._version;
			}

			public object Clone()
			{
				return MemberwiseClone();
			}

			public virtual bool MoveNext()
			{
				if (version != bitarray._version)
				{
					throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_EnumFailedVersion"));
				}
				if (index < bitarray.Count - 1)
				{
					index++;
					currentElement = bitarray.Get(index);
					return true;
				}
				index = bitarray.Count;
				return false;
			}

			public void Reset()
			{
				if (version != bitarray._version)
				{
					throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_EnumFailedVersion"));
				}
				index = -1;
			}
		}

		private const int _ShrinkThreshold = 256;

		private int[] m_array;

		private int m_length;

		private int _version;

		[NonSerialized]
		private object _syncRoot;

		public bool this[int index]
		{
			get
			{
				return Get(index);
			}
			set
			{
				Set(index, value);
			}
		}

		public int Length
		{
			get
			{
				return m_length;
			}
			set
			{
				if (value < 0)
				{
					throw new ArgumentOutOfRangeException("value", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
				}
				int num = (value + 31) / 32;
				if (num > m_array.Length || num + 256 < m_array.Length)
				{
					int[] array = new int[num];
					Array.Copy(m_array, array, (num > m_array.Length) ? m_array.Length : num);
					m_array = array;
				}
				if (value > m_length)
				{
					int num2 = (m_length + 31) / 32 - 1;
					int num3 = m_length % 32;
					if (num3 > 0)
					{
						m_array[num2] &= (1 << num3) - 1;
					}
					Array.Clear(m_array, num2 + 1, num - num2 - 1);
				}
				m_length = value;
				_version++;
			}
		}

		public int Count => m_length;

		public object SyncRoot
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

		public bool IsReadOnly => false;

		public bool IsSynchronized => false;

		private BitArray()
		{
		}

		public BitArray(int length)
			: this(length, defaultValue: false)
		{
		}

		public BitArray(int length, bool defaultValue)
		{
			if (length < 0)
			{
				throw new ArgumentOutOfRangeException("length", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
			}
			m_array = new int[(length + 31) / 32];
			m_length = length;
			int num = (defaultValue ? (-1) : 0);
			for (int i = 0; i < m_array.Length; i++)
			{
				m_array[i] = num;
			}
			_version = 0;
		}

		public BitArray(byte[] bytes)
		{
			if (bytes == null)
			{
				throw new ArgumentNullException("bytes");
			}
			m_array = new int[(bytes.Length + 3) / 4];
			m_length = bytes.Length * 8;
			int num = 0;
			int i;
			for (i = 0; bytes.Length - i >= 4; i += 4)
			{
				m_array[num++] = (bytes[i] & 0xFF) | ((bytes[i + 1] & 0xFF) << 8) | ((bytes[i + 2] & 0xFF) << 16) | ((bytes[i + 3] & 0xFF) << 24);
			}
			switch (bytes.Length - i)
			{
			case 3:
				m_array[num] = (bytes[i + 2] & 0xFF) << 16;
				goto case 2;
			case 2:
				m_array[num] |= (bytes[i + 1] & 0xFF) << 8;
				goto case 1;
			case 1:
				m_array[num] |= bytes[i] & 0xFF;
				break;
			}
			_version = 0;
		}

		public BitArray(bool[] values)
		{
			if (values == null)
			{
				throw new ArgumentNullException("values");
			}
			m_array = new int[(values.Length + 31) / 32];
			m_length = values.Length;
			for (int i = 0; i < values.Length; i++)
			{
				if (values[i])
				{
					m_array[i / 32] |= 1 << i % 32;
				}
			}
			_version = 0;
		}

		public BitArray(int[] values)
		{
			if (values == null)
			{
				throw new ArgumentNullException("values");
			}
			m_array = new int[values.Length];
			m_length = values.Length * 32;
			Array.Copy(values, m_array, values.Length);
			_version = 0;
		}

		public BitArray(BitArray bits)
		{
			if (bits == null)
			{
				throw new ArgumentNullException("bits");
			}
			m_array = new int[(bits.m_length + 31) / 32];
			m_length = bits.m_length;
			Array.Copy(bits.m_array, m_array, (bits.m_length + 31) / 32);
			_version = bits._version;
		}

		public bool Get(int index)
		{
			if (index < 0 || index >= m_length)
			{
				throw new ArgumentOutOfRangeException("index", Environment.GetResourceString("ArgumentOutOfRange_Index"));
			}
			return (m_array[index / 32] & (1 << index % 32)) != 0;
		}

		public void Set(int index, bool value)
		{
			if (index < 0 || index >= m_length)
			{
				throw new ArgumentOutOfRangeException("index", Environment.GetResourceString("ArgumentOutOfRange_Index"));
			}
			if (value)
			{
				m_array[index / 32] |= 1 << index % 32;
			}
			else
			{
				m_array[index / 32] &= ~(1 << index % 32);
			}
			_version++;
		}

		public void SetAll(bool value)
		{
			int num = (value ? (-1) : 0);
			int num2 = (m_length + 31) / 32;
			for (int i = 0; i < num2; i++)
			{
				m_array[i] = num;
			}
			_version++;
		}

		public BitArray And(BitArray value)
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			if (m_length != value.m_length)
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_ArrayLengthsDiffer"));
			}
			int num = (m_length + 31) / 32;
			for (int i = 0; i < num; i++)
			{
				m_array[i] &= value.m_array[i];
			}
			_version++;
			return this;
		}

		public BitArray Or(BitArray value)
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			if (m_length != value.m_length)
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_ArrayLengthsDiffer"));
			}
			int num = (m_length + 31) / 32;
			for (int i = 0; i < num; i++)
			{
				m_array[i] |= value.m_array[i];
			}
			_version++;
			return this;
		}

		public BitArray Xor(BitArray value)
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			if (m_length != value.m_length)
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_ArrayLengthsDiffer"));
			}
			int num = (m_length + 31) / 32;
			for (int i = 0; i < num; i++)
			{
				m_array[i] ^= value.m_array[i];
			}
			_version++;
			return this;
		}

		public BitArray Not()
		{
			int num = (m_length + 31) / 32;
			for (int i = 0; i < num; i++)
			{
				m_array[i] = ~m_array[i];
			}
			_version++;
			return this;
		}

		public void CopyTo(Array array, int index)
		{
			if (array == null)
			{
				throw new ArgumentNullException("array");
			}
			if (index < 0)
			{
				throw new ArgumentOutOfRangeException("index", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
			}
			if (array.Rank != 1)
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_RankMultiDimNotSupported"));
			}
			if (array is int[])
			{
				Array.Copy(m_array, 0, array, index, (m_length + 31) / 32);
				return;
			}
			if (array is byte[])
			{
				if (array.Length - index < (m_length + 7) / 8)
				{
					throw new ArgumentException(Environment.GetResourceString("Argument_InvalidOffLen"));
				}
				byte[] array2 = (byte[])array;
				for (int i = 0; i < (m_length + 7) / 8; i++)
				{
					array2[index + i] = (byte)((uint)(m_array[i / 4] >> i % 4 * 8) & 0xFFu);
				}
				return;
			}
			if (array is bool[])
			{
				if (array.Length - index < m_length)
				{
					throw new ArgumentException(Environment.GetResourceString("Argument_InvalidOffLen"));
				}
				bool[] array3 = (bool[])array;
				for (int j = 0; j < m_length; j++)
				{
					array3[index + j] = ((m_array[j / 32] >> j % 32) & 1) != 0;
				}
				return;
			}
			throw new ArgumentException(Environment.GetResourceString("Arg_BitArrayTypeUnsupported"));
		}

		public object Clone()
		{
			BitArray bitArray = new BitArray(m_array);
			bitArray._version = _version;
			bitArray.m_length = m_length;
			return bitArray;
		}

		public IEnumerator GetEnumerator()
		{
			return new BitArrayEnumeratorSimple(this);
		}
	}
}
