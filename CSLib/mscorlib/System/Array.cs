using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;

namespace System
{
	[Serializable]
	[ComVisible(true)]
	public abstract class Array : ICloneable, IList, ICollection, IEnumerable
	{
		internal sealed class FunctorComparer<T> : IComparer<T>
		{
			private Comparison<T> comparison;

			private Comparer<T> c = Comparer<T>.Default;

			public FunctorComparer(Comparison<T> comparison)
			{
				this.comparison = comparison;
			}

			public int Compare(T x, T y)
			{
				return comparison(x, y);
			}
		}

		private struct SorterObjectArray
		{
			private object[] keys;

			private object[] items;

			private IComparer comparer;

			internal SorterObjectArray(object[] keys, object[] items, IComparer comparer)
			{
				if (comparer == null)
				{
					comparer = Comparer.Default;
				}
				this.keys = keys;
				this.items = items;
				this.comparer = comparer;
			}

			internal void SwapIfGreaterWithItems(int a, int b)
			{
				if (a == b)
				{
					return;
				}
				try
				{
					if (comparer.Compare(keys[a], keys[b]) > 0)
					{
						object obj = keys[a];
						keys[a] = keys[b];
						keys[b] = obj;
						if (items != null)
						{
							object obj2 = items[a];
							items[a] = items[b];
							items[b] = obj2;
						}
					}
				}
				catch (IndexOutOfRangeException)
				{
					throw new ArgumentException(Environment.GetResourceString("Arg_BogusIComparer", keys[b], keys[b].GetType().Name, comparer));
				}
				catch (Exception innerException)
				{
					throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_IComparerFailed"), innerException);
				}
			}

			internal void QuickSort(int left, int right)
			{
				do
				{
					int i = left;
					int num = right;
					int median = GetMedian(i, num);
					SwapIfGreaterWithItems(i, median);
					SwapIfGreaterWithItems(i, num);
					SwapIfGreaterWithItems(median, num);
					object obj = keys[median];
					do
					{
						try
						{
							for (; comparer.Compare(keys[i], obj) < 0; i++)
							{
							}
							while (comparer.Compare(obj, keys[num]) < 0)
							{
								num--;
							}
						}
						catch (IndexOutOfRangeException)
						{
							throw new ArgumentException(Environment.GetResourceString("Arg_BogusIComparer", obj, obj.GetType().Name, comparer));
						}
						catch (Exception innerException)
						{
							throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_IComparerFailed"), innerException);
						}
						catch
						{
							throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_IComparerFailed"));
						}
						if (i > num)
						{
							break;
						}
						if (i < num)
						{
							object obj3 = keys[i];
							keys[i] = keys[num];
							keys[num] = obj3;
							if (items != null)
							{
								object obj4 = items[i];
								items[i] = items[num];
								items[num] = obj4;
							}
						}
						i++;
						num--;
					}
					while (i <= num);
					if (num - left <= right - i)
					{
						if (left < num)
						{
							QuickSort(left, num);
						}
						left = i;
					}
					else
					{
						if (i < right)
						{
							QuickSort(i, right);
						}
						right = num;
					}
				}
				while (left < right);
			}
		}

		private struct SorterGenericArray
		{
			private Array keys;

			private Array items;

			private IComparer comparer;

			internal SorterGenericArray(Array keys, Array items, IComparer comparer)
			{
				if (comparer == null)
				{
					comparer = Comparer.Default;
				}
				this.keys = keys;
				this.items = items;
				this.comparer = comparer;
			}

			internal void SwapIfGreaterWithItems(int a, int b)
			{
				if (a == b)
				{
					return;
				}
				try
				{
					if (comparer.Compare(keys.GetValue(a), keys.GetValue(b)) > 0)
					{
						object value = keys.GetValue(a);
						keys.SetValue(keys.GetValue(b), a);
						keys.SetValue(value, b);
						if (items != null)
						{
							object value2 = items.GetValue(a);
							items.SetValue(items.GetValue(b), a);
							items.SetValue(value2, b);
						}
					}
				}
				catch (IndexOutOfRangeException)
				{
					throw new ArgumentException(Environment.GetResourceString("Arg_BogusIComparer", keys.GetValue(b), keys.GetValue(b).GetType().Name, comparer));
				}
				catch (Exception innerException)
				{
					throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_IComparerFailed"), innerException);
				}
			}

			internal void QuickSort(int left, int right)
			{
				do
				{
					int i = left;
					int num = right;
					int median = GetMedian(i, num);
					SwapIfGreaterWithItems(i, median);
					SwapIfGreaterWithItems(i, num);
					SwapIfGreaterWithItems(median, num);
					object value = keys.GetValue(median);
					do
					{
						try
						{
							for (; comparer.Compare(keys.GetValue(i), value) < 0; i++)
							{
							}
							while (comparer.Compare(value, keys.GetValue(num)) < 0)
							{
								num--;
							}
						}
						catch (IndexOutOfRangeException)
						{
							throw new ArgumentException(Environment.GetResourceString("Arg_BogusIComparer", value, value.GetType().Name, comparer));
						}
						catch (Exception innerException)
						{
							throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_IComparerFailed"), innerException);
						}
						catch
						{
							throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_IComparerFailed"));
						}
						if (i > num)
						{
							break;
						}
						if (i < num)
						{
							object value2 = keys.GetValue(i);
							keys.SetValue(keys.GetValue(num), i);
							keys.SetValue(value2, num);
							if (items != null)
							{
								object value3 = items.GetValue(i);
								items.SetValue(items.GetValue(num), i);
								items.SetValue(value3, num);
							}
						}
						if (i != int.MaxValue)
						{
							i++;
						}
						if (num != int.MinValue)
						{
							num--;
						}
					}
					while (i <= num);
					if (num - left <= right - i)
					{
						if (left < num)
						{
							QuickSort(left, num);
						}
						left = i;
					}
					else
					{
						if (i < right)
						{
							QuickSort(i, right);
						}
						right = num;
					}
				}
				while (left < right);
			}
		}

		[Serializable]
		private sealed class SZArrayEnumerator : IEnumerator, ICloneable
		{
			private Array _array;

			private int _index;

			private int _endIndex;

			public object Current
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
					return _array.GetValue(_index);
				}
			}

			internal SZArrayEnumerator(Array array)
			{
				_array = array;
				_index = -1;
				_endIndex = array.Length;
			}

			public object Clone()
			{
				return MemberwiseClone();
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

			public void Reset()
			{
				_index = -1;
			}
		}

		[Serializable]
		private sealed class ArrayEnumerator : IEnumerator, ICloneable
		{
			private Array array;

			private int index;

			private int endIndex;

			private int startIndex;

			private int[] _indices;

			private bool _complete;

			public object Current
			{
				get
				{
					if (index < startIndex)
					{
						throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_EnumNotStarted"));
					}
					if (_complete)
					{
						throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_EnumEnded"));
					}
					return array.GetValue(_indices);
				}
			}

			internal ArrayEnumerator(Array array, int index, int count)
			{
				this.array = array;
				this.index = index - 1;
				startIndex = index;
				endIndex = index + count;
				_indices = new int[array.Rank];
				int num = 1;
				for (int i = 0; i < array.Rank; i++)
				{
					_indices[i] = array.GetLowerBound(i);
					num *= array.GetLength(i);
				}
				_indices[_indices.Length - 1]--;
				_complete = num == 0;
			}

			private void IncArray()
			{
				int rank = array.Rank;
				_indices[rank - 1]++;
				for (int num = rank - 1; num >= 0; num--)
				{
					if (_indices[num] > array.GetUpperBound(num))
					{
						if (num == 0)
						{
							_complete = true;
							break;
						}
						for (int i = num; i < rank; i++)
						{
							_indices[i] = array.GetLowerBound(i);
						}
						_indices[num - 1]++;
					}
				}
			}

			public object Clone()
			{
				return MemberwiseClone();
			}

			public bool MoveNext()
			{
				if (_complete)
				{
					index = endIndex;
					return false;
				}
				index++;
				IncArray();
				return !_complete;
			}

			public void Reset()
			{
				index = startIndex - 1;
				int num = 1;
				for (int i = 0; i < array.Rank; i++)
				{
					_indices[i] = array.GetLowerBound(i);
					num *= array.GetLength(i);
				}
				_complete = num == 0;
				_indices[_indices.Length - 1]--;
			}
		}

		public int Length
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
			get;
		}

		[ComVisible(false)]
		public long LongLength
		{
			[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
			get
			{
				return Length;
			}
		}

		public int Rank
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
			get;
		}

		int ICollection.Count => Length;

		public object SyncRoot => this;

		public bool IsReadOnly => false;

		public bool IsFixedSize => true;

		public bool IsSynchronized => false;

		object IList.this[int index]
		{
			get
			{
				return GetValue(index);
			}
			set
			{
				SetValue(value, index);
			}
		}

		private Array()
		{
		}

		public static ReadOnlyCollection<T> AsReadOnly<T>(T[] array)
		{
			if (array == null)
			{
				throw new ArgumentNullException("array");
			}
			return new ReadOnlyCollection<T>(array);
		}

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		public static void Resize<T>(ref T[] array, int newSize)
		{
			if (newSize < 0)
			{
				throw new ArgumentOutOfRangeException("newSize", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
			}
			T[] array2 = array;
			if (array2 == null)
			{
				array = new T[newSize];
			}
			else if (array2.Length != newSize)
			{
				T[] array3 = new T[newSize];
				Copy(array2, 0, array3, 0, (array2.Length > newSize) ? newSize : array2.Length);
				array = array3;
			}
		}

		public unsafe static Array CreateInstance(Type elementType, int length)
		{
			if (elementType == null)
			{
				throw new ArgumentNullException("elementType");
			}
			RuntimeType runtimeType = elementType.UnderlyingSystemType as RuntimeType;
			if (runtimeType == null)
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_MustBeType"), "elementType");
			}
			if (length < 0)
			{
				throw new ArgumentOutOfRangeException("length", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
			}
			return InternalCreate((void*)runtimeType.TypeHandle.Value, 1, &length, null);
		}

		public unsafe static Array CreateInstance(Type elementType, int length1, int length2)
		{
			if (elementType == null)
			{
				throw new ArgumentNullException("elementType");
			}
			RuntimeType runtimeType = elementType.UnderlyingSystemType as RuntimeType;
			if (runtimeType == null)
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_MustBeType"), "elementType");
			}
			if (length1 < 0 || length2 < 0)
			{
				throw new ArgumentOutOfRangeException((length1 < 0) ? "length1" : "length2", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
			}
			int* ptr = (int*)stackalloc byte[4 * 2];
			*ptr = length1;
			ptr[1] = length2;
			return InternalCreate((void*)runtimeType.TypeHandle.Value, 2, ptr, null);
		}

		public unsafe static Array CreateInstance(Type elementType, int length1, int length2, int length3)
		{
			if (elementType == null)
			{
				throw new ArgumentNullException("elementType");
			}
			RuntimeType runtimeType = elementType.UnderlyingSystemType as RuntimeType;
			if (runtimeType == null)
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_MustBeType"), "elementType");
			}
			if (length1 < 0 || length2 < 0 || length3 < 0)
			{
				string paramName = "length1";
				if (length2 < 0)
				{
					paramName = "length2";
				}
				if (length3 < 0)
				{
					paramName = "length3";
				}
				throw new ArgumentOutOfRangeException(paramName, Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
			}
			int* ptr = (int*)stackalloc byte[4 * 3];
			*ptr = length1;
			ptr[1] = length2;
			ptr[2] = length3;
			return InternalCreate((void*)runtimeType.TypeHandle.Value, 3, ptr, null);
		}

		public unsafe static Array CreateInstance(Type elementType, params int[] lengths)
		{
			if (elementType == null)
			{
				throw new ArgumentNullException("elementType");
			}
			RuntimeType runtimeType = elementType.UnderlyingSystemType as RuntimeType;
			if (runtimeType == null)
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_MustBeType"), "elementType");
			}
			if (lengths == null)
			{
				throw new ArgumentNullException("lengths");
			}
			if (lengths.Length == 0)
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_NeedAtLeast1Rank"));
			}
			for (int i = 0; i < lengths.Length; i++)
			{
				if (lengths[i] < 0)
				{
					throw new ArgumentOutOfRangeException("lengths[" + i + ']', Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
				}
			}
			fixed (int* pLengths = lengths)
			{
				return InternalCreate((void*)runtimeType.TypeHandle.Value, lengths.Length, pLengths, null);
			}
		}

		public static Array CreateInstance(Type elementType, params long[] lengths)
		{
			if (lengths == null)
			{
				throw new ArgumentNullException("lengths");
			}
			int[] array = new int[lengths.Length];
			for (int i = 0; i < lengths.Length; i++)
			{
				long num = lengths[i];
				if (num > int.MaxValue || num < int.MinValue)
				{
					throw new ArgumentOutOfRangeException("len", Environment.GetResourceString("ArgumentOutOfRange_HugeArrayNotSupported"));
				}
				array[i] = (int)num;
			}
			return CreateInstance(elementType, array);
		}

		public unsafe static Array CreateInstance(Type elementType, int[] lengths, int[] lowerBounds)
		{
			if (elementType == null)
			{
				throw new ArgumentNullException("elementType");
			}
			RuntimeType runtimeType = elementType.UnderlyingSystemType as RuntimeType;
			if (runtimeType == null)
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_MustBeType"), "elementType");
			}
			if (lengths == null)
			{
				throw new ArgumentNullException("lengths");
			}
			if (lowerBounds == null)
			{
				throw new ArgumentNullException("lowerBounds");
			}
			for (int i = 0; i < lengths.Length; i++)
			{
				if (lengths[i] < 0)
				{
					throw new ArgumentOutOfRangeException("lengths[" + i + ']', Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
				}
			}
			if (lengths.Length != lowerBounds.Length)
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_RanksAndBounds"));
			}
			if (lengths.Length == 0)
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_NeedAtLeast1Rank"));
			}
			fixed (int* pLengths = lengths)
			{
				fixed (int* pLowerBounds = lowerBounds)
				{
					return InternalCreate((void*)runtimeType.TypeHandle.Value, lengths.Length, pLengths, pLowerBounds);
				}
			}
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private unsafe static extern Array InternalCreate(void* elementType, int rank, int* pLengths, int* pLowerBounds);

		[ReliabilityContract(Consistency.MayCorruptInstance, Cer.MayFail)]
		public static void Copy(Array sourceArray, Array destinationArray, int length)
		{
			if (sourceArray == null)
			{
				throw new ArgumentNullException("sourceArray");
			}
			if (destinationArray == null)
			{
				throw new ArgumentNullException("destinationArray");
			}
			Copy(sourceArray, sourceArray.GetLowerBound(0), destinationArray, destinationArray.GetLowerBound(0), length, reliable: false);
		}

		[ReliabilityContract(Consistency.MayCorruptInstance, Cer.MayFail)]
		public static void Copy(Array sourceArray, int sourceIndex, Array destinationArray, int destinationIndex, int length)
		{
			Copy(sourceArray, sourceIndex, destinationArray, destinationIndex, length, reliable: false);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[ReliabilityContract(Consistency.MayCorruptInstance, Cer.MayFail)]
		internal static extern void Copy(Array sourceArray, int sourceIndex, Array destinationArray, int destinationIndex, int length, bool reliable);

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		public static void ConstrainedCopy(Array sourceArray, int sourceIndex, Array destinationArray, int destinationIndex, int length)
		{
			Copy(sourceArray, sourceIndex, destinationArray, destinationIndex, length, reliable: true);
		}

		[ReliabilityContract(Consistency.MayCorruptInstance, Cer.MayFail)]
		public static void Copy(Array sourceArray, Array destinationArray, long length)
		{
			if (length > int.MaxValue || length < int.MinValue)
			{
				throw new ArgumentOutOfRangeException("length", Environment.GetResourceString("ArgumentOutOfRange_HugeArrayNotSupported"));
			}
			Copy(sourceArray, destinationArray, (int)length);
		}

		[ReliabilityContract(Consistency.MayCorruptInstance, Cer.MayFail)]
		public static void Copy(Array sourceArray, long sourceIndex, Array destinationArray, long destinationIndex, long length)
		{
			if (sourceIndex > int.MaxValue || sourceIndex < int.MinValue)
			{
				throw new ArgumentOutOfRangeException("sourceIndex", Environment.GetResourceString("ArgumentOutOfRange_HugeArrayNotSupported"));
			}
			if (destinationIndex > int.MaxValue || destinationIndex < int.MinValue)
			{
				throw new ArgumentOutOfRangeException("destinationIndex", Environment.GetResourceString("ArgumentOutOfRange_HugeArrayNotSupported"));
			}
			if (length > int.MaxValue || length < int.MinValue)
			{
				throw new ArgumentOutOfRangeException("length", Environment.GetResourceString("ArgumentOutOfRange_HugeArrayNotSupported"));
			}
			Copy(sourceArray, (int)sourceIndex, destinationArray, (int)destinationIndex, (int)length);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		public static extern void Clear(Array array, int index, int length);

		public unsafe object GetValue(params int[] indices)
		{
			if (indices == null)
			{
				throw new ArgumentNullException("indices");
			}
			if (Rank != indices.Length)
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_RankIndices"));
			}
			TypedReference typedReference = default(TypedReference);
			fixed (int* pIndices = indices)
			{
				InternalGetReference(&typedReference, indices.Length, pIndices);
			}
			return TypedReference.InternalToObject(&typedReference);
		}

		public unsafe object GetValue(int index)
		{
			if (Rank != 1)
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_Need1DArray"));
			}
			TypedReference typedReference = default(TypedReference);
			InternalGetReference(&typedReference, 1, &index);
			return TypedReference.InternalToObject(&typedReference);
		}

		public unsafe object GetValue(int index1, int index2)
		{
			if (Rank != 2)
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_Need2DArray"));
			}
			int* ptr = (int*)stackalloc byte[4 * 2];
			*ptr = index1;
			ptr[1] = index2;
			TypedReference typedReference = default(TypedReference);
			InternalGetReference(&typedReference, 2, ptr);
			return TypedReference.InternalToObject(&typedReference);
		}

		public unsafe object GetValue(int index1, int index2, int index3)
		{
			if (Rank != 3)
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_Need3DArray"));
			}
			int* ptr = (int*)stackalloc byte[4 * 3];
			*ptr = index1;
			ptr[1] = index2;
			ptr[2] = index3;
			TypedReference typedReference = default(TypedReference);
			InternalGetReference(&typedReference, 3, ptr);
			return TypedReference.InternalToObject(&typedReference);
		}

		[ComVisible(false)]
		public object GetValue(long index)
		{
			if (index > int.MaxValue || index < int.MinValue)
			{
				throw new ArgumentOutOfRangeException("index", Environment.GetResourceString("ArgumentOutOfRange_HugeArrayNotSupported"));
			}
			return GetValue((int)index);
		}

		[ComVisible(false)]
		public object GetValue(long index1, long index2)
		{
			if (index1 > int.MaxValue || index1 < int.MinValue)
			{
				throw new ArgumentOutOfRangeException("index1", Environment.GetResourceString("ArgumentOutOfRange_HugeArrayNotSupported"));
			}
			if (index2 > int.MaxValue || index2 < int.MinValue)
			{
				throw new ArgumentOutOfRangeException("index2", Environment.GetResourceString("ArgumentOutOfRange_HugeArrayNotSupported"));
			}
			return GetValue((int)index1, (int)index2);
		}

		[ComVisible(false)]
		public object GetValue(long index1, long index2, long index3)
		{
			if (index1 > int.MaxValue || index1 < int.MinValue)
			{
				throw new ArgumentOutOfRangeException("index1", Environment.GetResourceString("ArgumentOutOfRange_HugeArrayNotSupported"));
			}
			if (index2 > int.MaxValue || index2 < int.MinValue)
			{
				throw new ArgumentOutOfRangeException("index2", Environment.GetResourceString("ArgumentOutOfRange_HugeArrayNotSupported"));
			}
			if (index3 > int.MaxValue || index3 < int.MinValue)
			{
				throw new ArgumentOutOfRangeException("index3", Environment.GetResourceString("ArgumentOutOfRange_HugeArrayNotSupported"));
			}
			return GetValue((int)index1, (int)index2, (int)index3);
		}

		[ComVisible(false)]
		public object GetValue(params long[] indices)
		{
			if (indices == null)
			{
				throw new ArgumentNullException("indices");
			}
			if (Rank != indices.Length)
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_RankIndices"));
			}
			int[] array = new int[indices.Length];
			for (int i = 0; i < indices.Length; i++)
			{
				long num = indices[i];
				if (num > int.MaxValue || num < int.MinValue)
				{
					throw new ArgumentOutOfRangeException("index", Environment.GetResourceString("ArgumentOutOfRange_HugeArrayNotSupported"));
				}
				array[i] = (int)num;
			}
			return GetValue(array);
		}

		public unsafe void SetValue(object value, int index)
		{
			if (Rank != 1)
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_Need1DArray"));
			}
			TypedReference typedReference = default(TypedReference);
			InternalGetReference(&typedReference, 1, &index);
			InternalSetValue(&typedReference, value);
		}

		public unsafe void SetValue(object value, int index1, int index2)
		{
			if (Rank != 2)
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_Need2DArray"));
			}
			int* ptr = (int*)stackalloc byte[4 * 2];
			*ptr = index1;
			ptr[1] = index2;
			TypedReference typedReference = default(TypedReference);
			InternalGetReference(&typedReference, 2, ptr);
			InternalSetValue(&typedReference, value);
		}

		public unsafe void SetValue(object value, int index1, int index2, int index3)
		{
			if (Rank != 3)
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_Need3DArray"));
			}
			int* ptr = (int*)stackalloc byte[4 * 3];
			*ptr = index1;
			ptr[1] = index2;
			ptr[2] = index3;
			TypedReference typedReference = default(TypedReference);
			InternalGetReference(&typedReference, 3, ptr);
			InternalSetValue(&typedReference, value);
		}

		public unsafe void SetValue(object value, params int[] indices)
		{
			if (indices == null)
			{
				throw new ArgumentNullException("indices");
			}
			if (Rank != indices.Length)
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_RankIndices"));
			}
			TypedReference typedReference = default(TypedReference);
			fixed (int* pIndices = indices)
			{
				InternalGetReference(&typedReference, indices.Length, pIndices);
			}
			InternalSetValue(&typedReference, value);
		}

		[ComVisible(false)]
		public void SetValue(object value, long index)
		{
			if (index > int.MaxValue || index < int.MinValue)
			{
				throw new ArgumentOutOfRangeException("index", Environment.GetResourceString("ArgumentOutOfRange_HugeArrayNotSupported"));
			}
			SetValue(value, (int)index);
		}

		[ComVisible(false)]
		public void SetValue(object value, long index1, long index2)
		{
			if (index1 > int.MaxValue || index1 < int.MinValue)
			{
				throw new ArgumentOutOfRangeException("index1", Environment.GetResourceString("ArgumentOutOfRange_HugeArrayNotSupported"));
			}
			if (index2 > int.MaxValue || index2 < int.MinValue)
			{
				throw new ArgumentOutOfRangeException("index2", Environment.GetResourceString("ArgumentOutOfRange_HugeArrayNotSupported"));
			}
			SetValue(value, (int)index1, (int)index2);
		}

		[ComVisible(false)]
		public void SetValue(object value, long index1, long index2, long index3)
		{
			if (index1 > int.MaxValue || index1 < int.MinValue)
			{
				throw new ArgumentOutOfRangeException("index1", Environment.GetResourceString("ArgumentOutOfRange_HugeArrayNotSupported"));
			}
			if (index2 > int.MaxValue || index2 < int.MinValue)
			{
				throw new ArgumentOutOfRangeException("index2", Environment.GetResourceString("ArgumentOutOfRange_HugeArrayNotSupported"));
			}
			if (index3 > int.MaxValue || index3 < int.MinValue)
			{
				throw new ArgumentOutOfRangeException("index3", Environment.GetResourceString("ArgumentOutOfRange_HugeArrayNotSupported"));
			}
			SetValue(value, (int)index1, (int)index2, (int)index3);
		}

		[ComVisible(false)]
		public void SetValue(object value, params long[] indices)
		{
			if (indices == null)
			{
				throw new ArgumentNullException("indices");
			}
			if (Rank != indices.Length)
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_RankIndices"));
			}
			int[] array = new int[indices.Length];
			for (int i = 0; i < indices.Length; i++)
			{
				long num = indices[i];
				if (num > int.MaxValue || num < int.MinValue)
				{
					throw new ArgumentOutOfRangeException("index", Environment.GetResourceString("ArgumentOutOfRange_HugeArrayNotSupported"));
				}
				array[i] = (int)num;
			}
			SetValue(value, array);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private unsafe extern void InternalGetReference(void* elemRef, int rank, int* pIndices);

		[MethodImpl(MethodImplOptions.InternalCall)]
		private unsafe static extern void InternalSetValue(void* target, object value);

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		private static int GetMedian(int low, int hi)
		{
			return low + (hi - low >> 1);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		public extern int GetLength(int dimension);

		[ComVisible(false)]
		public long GetLongLength(int dimension)
		{
			return GetLength(dimension);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		public extern int GetUpperBound(int dimension);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		public extern int GetLowerBound(int dimension);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		internal extern int GetDataPtrOffsetInternal();

		int IList.Add(object value)
		{
			throw new NotSupportedException(Environment.GetResourceString("NotSupported_FixedSizeCollection"));
		}

		bool IList.Contains(object value)
		{
			return IndexOf(this, value) >= GetLowerBound(0);
		}

		void IList.Clear()
		{
			Clear(this, 0, Length);
		}

		int IList.IndexOf(object value)
		{
			return IndexOf(this, value);
		}

		void IList.Insert(int index, object value)
		{
			throw new NotSupportedException(Environment.GetResourceString("NotSupported_FixedSizeCollection"));
		}

		void IList.Remove(object value)
		{
			throw new NotSupportedException(Environment.GetResourceString("NotSupported_FixedSizeCollection"));
		}

		void IList.RemoveAt(int index)
		{
			throw new NotSupportedException(Environment.GetResourceString("NotSupported_FixedSizeCollection"));
		}

		public object Clone()
		{
			return MemberwiseClone();
		}

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		public static int BinarySearch(Array array, object value)
		{
			if (array == null)
			{
				throw new ArgumentNullException("array");
			}
			int lowerBound = array.GetLowerBound(0);
			return BinarySearch(array, lowerBound, array.Length, value, null);
		}

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		public static int BinarySearch(Array array, int index, int length, object value)
		{
			return BinarySearch(array, index, length, value, null);
		}

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		public static int BinarySearch(Array array, object value, IComparer comparer)
		{
			if (array == null)
			{
				throw new ArgumentNullException("array");
			}
			int lowerBound = array.GetLowerBound(0);
			return BinarySearch(array, lowerBound, array.Length, value, comparer);
		}

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		public static int BinarySearch(Array array, int index, int length, object value, IComparer comparer)
		{
			if (array == null)
			{
				throw new ArgumentNullException("array");
			}
			int lowerBound = array.GetLowerBound(0);
			if (index < lowerBound || length < 0)
			{
				throw new ArgumentOutOfRangeException((index < lowerBound) ? "index" : "length", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
			}
			if (array.Length - (index - lowerBound) < length)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_InvalidOffLen"));
			}
			if (array.Rank != 1)
			{
				throw new RankException(Environment.GetResourceString("Rank_MultiDimNotSupported"));
			}
			if (comparer == null)
			{
				comparer = Comparer.Default;
			}
			if (comparer == Comparer.Default && TrySZBinarySearch(array, index, length, value, out var retVal))
			{
				return retVal;
			}
			int num = index;
			int num2 = index + length - 1;
			object[] array2 = array as object[];
			if (array2 != null)
			{
				while (num <= num2)
				{
					int median = GetMedian(num, num2);
					int num3;
					try
					{
						num3 = comparer.Compare(array2[median], value);
					}
					catch (Exception innerException)
					{
						throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_IComparerFailed"), innerException);
					}
					catch
					{
						throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_IComparerFailed"));
					}
					if (num3 == 0)
					{
						return median;
					}
					if (num3 < 0)
					{
						num = median + 1;
					}
					else
					{
						num2 = median - 1;
					}
				}
			}
			else
			{
				while (num <= num2)
				{
					int median2 = GetMedian(num, num2);
					int num4;
					try
					{
						num4 = comparer.Compare(array.GetValue(median2), value);
					}
					catch (Exception innerException2)
					{
						throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_IComparerFailed"), innerException2);
					}
					catch
					{
						throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_IComparerFailed"));
					}
					if (num4 == 0)
					{
						return median2;
					}
					if (num4 < 0)
					{
						num = median2 + 1;
					}
					else
					{
						num2 = median2 - 1;
					}
				}
			}
			return ~num;
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		private static extern bool TrySZBinarySearch(Array sourceArray, int sourceIndex, int count, object value, out int retVal);

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		public static int BinarySearch<T>(T[] array, T value)
		{
			if (array == null)
			{
				throw new ArgumentNullException("array");
			}
			return BinarySearch(array, 0, array.Length, value, null);
		}

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		public static int BinarySearch<T>(T[] array, T value, IComparer<T> comparer)
		{
			if (array == null)
			{
				throw new ArgumentNullException("array");
			}
			return BinarySearch(array, 0, array.Length, value, comparer);
		}

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		public static int BinarySearch<T>(T[] array, int index, int length, T value)
		{
			return BinarySearch(array, index, length, value, null);
		}

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		public static int BinarySearch<T>(T[] array, int index, int length, T value, IComparer<T> comparer)
		{
			if (array == null)
			{
				throw new ArgumentNullException("array");
			}
			if (index < 0 || length < 0)
			{
				throw new ArgumentOutOfRangeException((index < 0) ? "index" : "length", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
			}
			if (array.Length - index < length)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_InvalidOffLen"));
			}
			return ArraySortHelper<T>.Default.BinarySearch(array, index, length, value, comparer);
		}

		public static TOutput[] ConvertAll<TInput, TOutput>(TInput[] array, Converter<TInput, TOutput> converter)
		{
			if (array == null)
			{
				throw new ArgumentNullException("array");
			}
			if (converter == null)
			{
				throw new ArgumentNullException("converter");
			}
			TOutput[] array2 = new TOutput[array.Length];
			for (int i = 0; i < array.Length; i++)
			{
				array2[i] = converter(array[i]);
			}
			return array2;
		}

		public void CopyTo(Array array, int index)
		{
			if (array != null && array.Rank != 1)
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_RankMultiDimNotSupported"));
			}
			Copy(this, GetLowerBound(0), array, index, Length);
		}

		[ComVisible(false)]
		public void CopyTo(Array array, long index)
		{
			if (index > int.MaxValue || index < int.MinValue)
			{
				throw new ArgumentOutOfRangeException("index", Environment.GetResourceString("ArgumentOutOfRange_HugeArrayNotSupported"));
			}
			CopyTo(array, (int)index);
		}

		public static bool Exists<T>(T[] array, Predicate<T> match)
		{
			return FindIndex(array, match) != -1;
		}

		public static T Find<T>(T[] array, Predicate<T> match)
		{
			if (array == null)
			{
				throw new ArgumentNullException("array");
			}
			if (match == null)
			{
				throw new ArgumentNullException("match");
			}
			for (int i = 0; i < array.Length; i++)
			{
				if (match(array[i]))
				{
					return array[i];
				}
			}
			return default(T);
		}

		public static T[] FindAll<T>(T[] array, Predicate<T> match)
		{
			if (array == null)
			{
				throw new ArgumentNullException("array");
			}
			if (match == null)
			{
				throw new ArgumentNullException("match");
			}
			List<T> list = new List<T>();
			for (int i = 0; i < array.Length; i++)
			{
				if (match(array[i]))
				{
					list.Add(array[i]);
				}
			}
			return list.ToArray();
		}

		public static int FindIndex<T>(T[] array, Predicate<T> match)
		{
			if (array == null)
			{
				throw new ArgumentNullException("array");
			}
			return FindIndex(array, 0, array.Length, match);
		}

		public static int FindIndex<T>(T[] array, int startIndex, Predicate<T> match)
		{
			if (array == null)
			{
				throw new ArgumentNullException("array");
			}
			return FindIndex(array, startIndex, array.Length - startIndex, match);
		}

		public static int FindIndex<T>(T[] array, int startIndex, int count, Predicate<T> match)
		{
			if (array == null)
			{
				throw new ArgumentNullException("array");
			}
			if (startIndex < 0 || startIndex > array.Length)
			{
				throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("ArgumentOutOfRange_Index"));
			}
			if (count < 0 || startIndex > array.Length - count)
			{
				throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("ArgumentOutOfRange_Count"));
			}
			if (match == null)
			{
				throw new ArgumentNullException("match");
			}
			int num = startIndex + count;
			for (int i = startIndex; i < num; i++)
			{
				if (match(array[i]))
				{
					return i;
				}
			}
			return -1;
		}

		public static T FindLast<T>(T[] array, Predicate<T> match)
		{
			if (array == null)
			{
				throw new ArgumentNullException("array");
			}
			if (match == null)
			{
				throw new ArgumentNullException("match");
			}
			for (int num = array.Length - 1; num >= 0; num--)
			{
				if (match(array[num]))
				{
					return array[num];
				}
			}
			return default(T);
		}

		public static int FindLastIndex<T>(T[] array, Predicate<T> match)
		{
			if (array == null)
			{
				throw new ArgumentNullException("array");
			}
			return FindLastIndex(array, array.Length - 1, array.Length, match);
		}

		public static int FindLastIndex<T>(T[] array, int startIndex, Predicate<T> match)
		{
			if (array == null)
			{
				throw new ArgumentNullException("array");
			}
			return FindLastIndex(array, startIndex, startIndex + 1, match);
		}

		public static int FindLastIndex<T>(T[] array, int startIndex, int count, Predicate<T> match)
		{
			if (array == null)
			{
				throw new ArgumentNullException("array");
			}
			if (match == null)
			{
				throw new ArgumentNullException("match");
			}
			if (array.Length == 0)
			{
				if (startIndex != -1)
				{
					throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("ArgumentOutOfRange_Index"));
				}
			}
			else if (startIndex < 0 || startIndex >= array.Length)
			{
				throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("ArgumentOutOfRange_Index"));
			}
			if (count < 0 || startIndex - count + 1 < 0)
			{
				throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("ArgumentOutOfRange_Count"));
			}
			int num = startIndex - count;
			for (int num2 = startIndex; num2 > num; num2--)
			{
				if (match(array[num2]))
				{
					return num2;
				}
			}
			return -1;
		}

		public static void ForEach<T>(T[] array, Action<T> action)
		{
			if (array == null)
			{
				throw new ArgumentNullException("array");
			}
			if (action == null)
			{
				throw new ArgumentNullException("action");
			}
			for (int i = 0; i < array.Length; i++)
			{
				action(array[i]);
			}
		}

		public IEnumerator GetEnumerator()
		{
			int lowerBound = GetLowerBound(0);
			if (Rank == 1 && lowerBound == 0)
			{
				return new SZArrayEnumerator(this);
			}
			return new ArrayEnumerator(this, lowerBound, Length);
		}

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		public static int IndexOf(Array array, object value)
		{
			if (array == null)
			{
				throw new ArgumentNullException("array");
			}
			int lowerBound = array.GetLowerBound(0);
			return IndexOf(array, value, lowerBound, array.Length);
		}

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		public static int IndexOf(Array array, object value, int startIndex)
		{
			if (array == null)
			{
				throw new ArgumentNullException("array");
			}
			int lowerBound = array.GetLowerBound(0);
			return IndexOf(array, value, startIndex, array.Length - startIndex + lowerBound);
		}

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		public static int IndexOf(Array array, object value, int startIndex, int count)
		{
			if (array == null)
			{
				throw new ArgumentNullException("array");
			}
			int lowerBound = array.GetLowerBound(0);
			if (startIndex < lowerBound || startIndex > array.Length + lowerBound)
			{
				throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("ArgumentOutOfRange_Index"));
			}
			if (count < 0 || count > array.Length - startIndex + lowerBound)
			{
				throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("ArgumentOutOfRange_Count"));
			}
			if (array.Rank != 1)
			{
				throw new RankException(Environment.GetResourceString("Rank_MultiDimNotSupported"));
			}
			if (TrySZIndexOf(array, startIndex, count, value, out var retVal))
			{
				return retVal;
			}
			object[] array2 = array as object[];
			int num = startIndex + count;
			if (array2 != null)
			{
				if (value == null)
				{
					for (int i = startIndex; i < num; i++)
					{
						if (array2[i] == null)
						{
							return i;
						}
					}
				}
				else
				{
					for (int j = startIndex; j < num; j++)
					{
						object obj = array2[j];
						if (obj != null && obj.Equals(value))
						{
							return j;
						}
					}
				}
			}
			else
			{
				for (int k = startIndex; k < num; k++)
				{
					object value2 = array.GetValue(k);
					if (value2 == null)
					{
						if (value == null)
						{
							return k;
						}
					}
					else if (value2.Equals(value))
					{
						return k;
					}
				}
			}
			return lowerBound - 1;
		}

		public static int IndexOf<T>(T[] array, T value)
		{
			if (array == null)
			{
				throw new ArgumentNullException("array");
			}
			return IndexOf(array, value, 0, array.Length);
		}

		public static int IndexOf<T>(T[] array, T value, int startIndex)
		{
			if (array == null)
			{
				throw new ArgumentNullException("array");
			}
			return IndexOf(array, value, startIndex, array.Length - startIndex);
		}

		public static int IndexOf<T>(T[] array, T value, int startIndex, int count)
		{
			if (array == null)
			{
				throw new ArgumentNullException("array");
			}
			if (startIndex < 0 || startIndex > array.Length)
			{
				throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("ArgumentOutOfRange_Index"));
			}
			if (count < 0 || count > array.Length - startIndex)
			{
				throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("ArgumentOutOfRange_Count"));
			}
			return EqualityComparer<T>.Default.IndexOf(array, value, startIndex, count);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		private static extern bool TrySZIndexOf(Array sourceArray, int sourceIndex, int count, object value, out int retVal);

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		public static int LastIndexOf(Array array, object value)
		{
			if (array == null)
			{
				throw new ArgumentNullException("array");
			}
			int lowerBound = array.GetLowerBound(0);
			return LastIndexOf(array, value, array.Length - 1 + lowerBound, array.Length);
		}

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		public static int LastIndexOf(Array array, object value, int startIndex)
		{
			if (array == null)
			{
				throw new ArgumentNullException("array");
			}
			int lowerBound = array.GetLowerBound(0);
			return LastIndexOf(array, value, startIndex, startIndex + 1 - lowerBound);
		}

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		public static int LastIndexOf(Array array, object value, int startIndex, int count)
		{
			if (array == null)
			{
				throw new ArgumentNullException("array");
			}
			int lowerBound = array.GetLowerBound(0);
			if (array.Length == 0)
			{
				return lowerBound - 1;
			}
			if (startIndex < lowerBound || startIndex >= array.Length + lowerBound)
			{
				throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("ArgumentOutOfRange_Index"));
			}
			if (count < 0)
			{
				throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("ArgumentOutOfRange_Count"));
			}
			if (count > startIndex - lowerBound + 1)
			{
				throw new ArgumentOutOfRangeException("endIndex", Environment.GetResourceString("ArgumentOutOfRange_EndIndexStartIndex"));
			}
			if (array.Rank != 1)
			{
				throw new RankException(Environment.GetResourceString("Rank_MultiDimNotSupported"));
			}
			if (TrySZLastIndexOf(array, startIndex, count, value, out var retVal))
			{
				return retVal;
			}
			object[] array2 = array as object[];
			int num = startIndex - count + 1;
			if (array2 != null)
			{
				if (value == null)
				{
					for (int num2 = startIndex; num2 >= num; num2--)
					{
						if (array2[num2] == null)
						{
							return num2;
						}
					}
				}
				else
				{
					for (int num3 = startIndex; num3 >= num; num3--)
					{
						object obj = array2[num3];
						if (obj != null && obj.Equals(value))
						{
							return num3;
						}
					}
				}
			}
			else
			{
				for (int num4 = startIndex; num4 >= num; num4--)
				{
					object value2 = array.GetValue(num4);
					if (value2 == null)
					{
						if (value == null)
						{
							return num4;
						}
					}
					else if (value2.Equals(value))
					{
						return num4;
					}
				}
			}
			return lowerBound - 1;
		}

		public static int LastIndexOf<T>(T[] array, T value)
		{
			if (array == null)
			{
				throw new ArgumentNullException("array");
			}
			return LastIndexOf(array, value, array.Length - 1, array.Length);
		}

		public static int LastIndexOf<T>(T[] array, T value, int startIndex)
		{
			if (array == null)
			{
				throw new ArgumentNullException("array");
			}
			return LastIndexOf(array, value, startIndex, (array.Length != 0) ? (startIndex + 1) : 0);
		}

		public static int LastIndexOf<T>(T[] array, T value, int startIndex, int count)
		{
			if (array == null)
			{
				throw new ArgumentNullException("array");
			}
			if (array.Length == 0)
			{
				if (startIndex != -1 && startIndex != 0)
				{
					throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("ArgumentOutOfRange_Index"));
				}
				if (count != 0)
				{
					throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("ArgumentOutOfRange_Count"));
				}
				return -1;
			}
			if (startIndex < 0 || startIndex >= array.Length)
			{
				throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("ArgumentOutOfRange_Index"));
			}
			if (count < 0 || startIndex - count + 1 < 0)
			{
				throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("ArgumentOutOfRange_Count"));
			}
			return EqualityComparer<T>.Default.LastIndexOf(array, value, startIndex, count);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		private static extern bool TrySZLastIndexOf(Array sourceArray, int sourceIndex, int count, object value, out int retVal);

		[ReliabilityContract(Consistency.MayCorruptInstance, Cer.MayFail)]
		public static void Reverse(Array array)
		{
			if (array == null)
			{
				throw new ArgumentNullException("array");
			}
			Reverse(array, array.GetLowerBound(0), array.Length);
		}

		[ReliabilityContract(Consistency.MayCorruptInstance, Cer.MayFail)]
		public static void Reverse(Array array, int index, int length)
		{
			if (array == null)
			{
				throw new ArgumentNullException("array");
			}
			if (index < array.GetLowerBound(0) || length < 0)
			{
				throw new ArgumentOutOfRangeException((index < 0) ? "index" : "length", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
			}
			if (array.Length - (index - array.GetLowerBound(0)) < length)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_InvalidOffLen"));
			}
			if (array.Rank != 1)
			{
				throw new RankException(Environment.GetResourceString("Rank_MultiDimNotSupported"));
			}
			if (TrySZReverse(array, index, length))
			{
				return;
			}
			int num = index;
			int num2 = index + length - 1;
			object[] array2 = array as object[];
			if (array2 != null)
			{
				while (num < num2)
				{
					object obj = array2[num];
					array2[num] = array2[num2];
					array2[num2] = obj;
					num++;
					num2--;
				}
			}
			else
			{
				while (num < num2)
				{
					object value = array.GetValue(num);
					array.SetValue(array.GetValue(num2), num);
					array.SetValue(value, num2);
					num++;
					num2--;
				}
			}
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[ReliabilityContract(Consistency.MayCorruptInstance, Cer.MayFail)]
		private static extern bool TrySZReverse(Array array, int index, int count);

		[ReliabilityContract(Consistency.MayCorruptInstance, Cer.MayFail)]
		public static void Sort(Array array)
		{
			if (array == null)
			{
				throw new ArgumentNullException("array");
			}
			Sort(array, null, array.GetLowerBound(0), array.Length, null);
		}

		[ReliabilityContract(Consistency.MayCorruptInstance, Cer.MayFail)]
		public static void Sort(Array keys, Array items)
		{
			if (keys == null)
			{
				throw new ArgumentNullException("keys");
			}
			Sort(keys, items, keys.GetLowerBound(0), keys.Length, null);
		}

		[ReliabilityContract(Consistency.MayCorruptInstance, Cer.MayFail)]
		public static void Sort(Array array, int index, int length)
		{
			Sort(array, null, index, length, null);
		}

		[ReliabilityContract(Consistency.MayCorruptInstance, Cer.MayFail)]
		public static void Sort(Array keys, Array items, int index, int length)
		{
			Sort(keys, items, index, length, null);
		}

		[ReliabilityContract(Consistency.MayCorruptInstance, Cer.MayFail)]
		public static void Sort(Array array, IComparer comparer)
		{
			if (array == null)
			{
				throw new ArgumentNullException("array");
			}
			Sort(array, null, array.GetLowerBound(0), array.Length, comparer);
		}

		[ReliabilityContract(Consistency.MayCorruptInstance, Cer.MayFail)]
		public static void Sort(Array keys, Array items, IComparer comparer)
		{
			if (keys == null)
			{
				throw new ArgumentNullException("keys");
			}
			Sort(keys, items, keys.GetLowerBound(0), keys.Length, comparer);
		}

		[ReliabilityContract(Consistency.MayCorruptInstance, Cer.MayFail)]
		public static void Sort(Array array, int index, int length, IComparer comparer)
		{
			Sort(array, null, index, length, comparer);
		}

		[ReliabilityContract(Consistency.MayCorruptInstance, Cer.MayFail)]
		public static void Sort(Array keys, Array items, int index, int length, IComparer comparer)
		{
			if (keys == null)
			{
				throw new ArgumentNullException("keys");
			}
			if (keys.Rank != 1 || (items != null && items.Rank != 1))
			{
				throw new RankException(Environment.GetResourceString("Rank_MultiDimNotSupported"));
			}
			if (items != null && keys.GetLowerBound(0) != items.GetLowerBound(0))
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_LowerBoundsMustMatch"));
			}
			if (index < keys.GetLowerBound(0) || length < 0)
			{
				throw new ArgumentOutOfRangeException((length < 0) ? "length" : "index", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
			}
			if (keys.Length - (index - keys.GetLowerBound(0)) < length || (items != null && index - items.GetLowerBound(0) > items.Length - length))
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_InvalidOffLen"));
			}
			if (length > 1 && ((comparer != Comparer.Default && comparer != null) || !TrySZSort(keys, items, index, index + length - 1)))
			{
				object[] array = keys as object[];
				object[] array2 = null;
				if (array != null)
				{
					array2 = items as object[];
				}
				if (array != null && (items == null || array2 != null))
				{
					new SorterObjectArray(array, array2, comparer).QuickSort(index, index + length - 1);
				}
				else
				{
					new SorterGenericArray(keys, items, comparer).QuickSort(index, index + length - 1);
				}
			}
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[ReliabilityContract(Consistency.MayCorruptInstance, Cer.MayFail)]
		private static extern bool TrySZSort(Array keys, Array items, int left, int right);

		[ReliabilityContract(Consistency.MayCorruptInstance, Cer.MayFail)]
		public static void Sort<T>(T[] array)
		{
			if (array == null)
			{
				throw new ArgumentNullException("array");
			}
			Sort(array, array.GetLowerBound(0), array.Length, null);
		}

		[ReliabilityContract(Consistency.MayCorruptInstance, Cer.MayFail)]
		public static void Sort<TKey, TValue>(TKey[] keys, TValue[] items)
		{
			if (keys == null)
			{
				throw new ArgumentNullException("keys");
			}
			Sort(keys, items, 0, keys.Length, null);
		}

		[ReliabilityContract(Consistency.MayCorruptInstance, Cer.MayFail)]
		public static void Sort<T>(T[] array, int index, int length)
		{
			Sort(array, index, length, null);
		}

		[ReliabilityContract(Consistency.MayCorruptInstance, Cer.MayFail)]
		public static void Sort<TKey, TValue>(TKey[] keys, TValue[] items, int index, int length)
		{
			Sort(keys, items, index, length, null);
		}

		[ReliabilityContract(Consistency.MayCorruptInstance, Cer.MayFail)]
		public static void Sort<T>(T[] array, IComparer<T> comparer)
		{
			if (array == null)
			{
				throw new ArgumentNullException("array");
			}
			Sort(array, 0, array.Length, comparer);
		}

		[ReliabilityContract(Consistency.MayCorruptInstance, Cer.MayFail)]
		public static void Sort<TKey, TValue>(TKey[] keys, TValue[] items, IComparer<TKey> comparer)
		{
			if (keys == null)
			{
				throw new ArgumentNullException("keys");
			}
			Sort(keys, items, 0, keys.Length, comparer);
		}

		[ReliabilityContract(Consistency.MayCorruptInstance, Cer.MayFail)]
		public static void Sort<T>(T[] array, int index, int length, IComparer<T> comparer)
		{
			if (array == null)
			{
				throw new ArgumentNullException("array");
			}
			if (index < 0 || length < 0)
			{
				throw new ArgumentOutOfRangeException((length < 0) ? "length" : "index", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
			}
			if (array.Length - index < length)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_InvalidOffLen"));
			}
			if (length > 1 && ((comparer != null && comparer != Comparer<T>.Default) || !TrySZSort(array, null, index, index + length - 1)))
			{
				ArraySortHelper<T>.Default.Sort(array, index, length, comparer);
			}
		}

		[ReliabilityContract(Consistency.MayCorruptInstance, Cer.MayFail)]
		public static void Sort<TKey, TValue>(TKey[] keys, TValue[] items, int index, int length, IComparer<TKey> comparer)
		{
			if (keys == null)
			{
				throw new ArgumentNullException("keys");
			}
			if (index < 0 || length < 0)
			{
				throw new ArgumentOutOfRangeException((length < 0) ? "length" : "index", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
			}
			if (keys.Length - index < length || (items != null && index > items.Length - length))
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_InvalidOffLen"));
			}
			if (length > 1 && ((comparer != null && comparer != Comparer<TKey>.Default) || !TrySZSort(keys, items, index, index + length - 1)))
			{
				ArraySortHelper<TKey, TValue>.Default.Sort(keys, items, index, length, comparer);
			}
		}

		public static void Sort<T>(T[] array, Comparison<T> comparison)
		{
			if (array == null)
			{
				throw new ArgumentNullException("array");
			}
			if (comparison == null)
			{
				throw new ArgumentNullException("comparison");
			}
			IComparer<T> comparer = new FunctorComparer<T>(comparison);
			Sort(array, comparer);
		}

		public static bool TrueForAll<T>(T[] array, Predicate<T> match)
		{
			if (array == null)
			{
				throw new ArgumentNullException("array");
			}
			if (match == null)
			{
				throw new ArgumentNullException("match");
			}
			for (int i = 0; i < array.Length; i++)
			{
				if (!match(array[i]))
				{
					return false;
				}
			}
			return true;
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		public extern void Initialize();
	}
}
