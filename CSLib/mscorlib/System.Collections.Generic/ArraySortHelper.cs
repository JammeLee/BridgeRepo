using System.Runtime.CompilerServices;

namespace System.Collections.Generic
{
	[TypeDependency("System.Collections.Generic.GenericArraySortHelper`1")]
	internal class ArraySortHelper<T> : IArraySortHelper<T>
	{
		private static IArraySortHelper<T> defaultArraySortHelper;

		public static IArraySortHelper<T> Default
		{
			get
			{
				IArraySortHelper<T> arraySortHelper = defaultArraySortHelper;
				if (arraySortHelper == null)
				{
					arraySortHelper = CreateArraySortHelper();
				}
				return arraySortHelper;
			}
		}

		private static IArraySortHelper<T> CreateArraySortHelper()
		{
			if (typeof(IComparable<T>).IsAssignableFrom(typeof(T)))
			{
				defaultArraySortHelper = (IArraySortHelper<T>)typeof(GenericArraySortHelper<string>).TypeHandle.Instantiate(new RuntimeTypeHandle[1]
				{
					typeof(T).TypeHandle
				}).Allocate();
			}
			else
			{
				defaultArraySortHelper = new ArraySortHelper<T>();
			}
			return defaultArraySortHelper;
		}

		public void Sort(T[] keys, int index, int length, IComparer<T> comparer)
		{
			try
			{
				if (comparer == null)
				{
					comparer = Comparer<T>.Default;
				}
				QuickSort(keys, index, index + (length - 1), comparer);
			}
			catch (IndexOutOfRangeException)
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_BogusIComparer", null, typeof(T).Name, comparer));
			}
			catch (Exception innerException)
			{
				throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_IComparerFailed"), innerException);
			}
		}

		public int BinarySearch(T[] array, int index, int length, T value, IComparer<T> comparer)
		{
			try
			{
				if (comparer == null)
				{
					comparer = Comparer<T>.Default;
				}
				return InternalBinarySearch(array, index, length, value, comparer);
			}
			catch (Exception innerException)
			{
				throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_IComparerFailed"), innerException);
			}
		}

		internal static int InternalBinarySearch(T[] array, int index, int length, T value, IComparer<T> comparer)
		{
			int num = index;
			int num2 = index + length - 1;
			while (num <= num2)
			{
				int num3 = num + (num2 - num >> 1);
				int num4 = comparer.Compare(array[num3], value);
				if (num4 == 0)
				{
					return num3;
				}
				if (num4 < 0)
				{
					num = num3 + 1;
				}
				else
				{
					num2 = num3 - 1;
				}
			}
			return ~num;
		}

		private static void SwapIfGreaterWithItems(T[] keys, IComparer<T> comparer, int a, int b)
		{
			if (a != b && comparer.Compare(keys[a], keys[b]) > 0)
			{
				T val = keys[a];
				keys[a] = keys[b];
				keys[b] = val;
			}
		}

		internal static void QuickSort(T[] keys, int left, int right, IComparer<T> comparer)
		{
			do
			{
				int num = left;
				int num2 = right;
				int num3 = num + (num2 - num >> 1);
				SwapIfGreaterWithItems(keys, comparer, num, num3);
				SwapIfGreaterWithItems(keys, comparer, num, num2);
				SwapIfGreaterWithItems(keys, comparer, num3, num2);
				T val = keys[num3];
				while (true)
				{
					if (comparer.Compare(keys[num], val) < 0)
					{
						num++;
						continue;
					}
					while (comparer.Compare(val, keys[num2]) < 0)
					{
						num2--;
					}
					if (num > num2)
					{
						break;
					}
					if (num < num2)
					{
						T val2 = keys[num];
						keys[num] = keys[num2];
						keys[num2] = val2;
					}
					num++;
					num2--;
					if (num > num2)
					{
						break;
					}
				}
				if (num2 - left <= right - num)
				{
					if (left < num2)
					{
						QuickSort(keys, left, num2, comparer);
					}
					left = num;
				}
				else
				{
					if (num < right)
					{
						QuickSort(keys, num, right, comparer);
					}
					right = num2;
				}
			}
			while (left < right);
		}
	}
	[TypeDependency("System.Collections.Generic.GenericArraySortHelper`2")]
	internal class ArraySortHelper<TKey, TValue> : IArraySortHelper<TKey, TValue>
	{
		private static IArraySortHelper<TKey, TValue> defaultArraySortHelper;

		public static IArraySortHelper<TKey, TValue> Default
		{
			get
			{
				IArraySortHelper<TKey, TValue> arraySortHelper = defaultArraySortHelper;
				if (arraySortHelper == null)
				{
					arraySortHelper = CreateArraySortHelper();
				}
				return arraySortHelper;
			}
		}

		public static IArraySortHelper<TKey, TValue> CreateArraySortHelper()
		{
			if (typeof(IComparable<TKey>).IsAssignableFrom(typeof(TKey)))
			{
				defaultArraySortHelper = (IArraySortHelper<TKey, TValue>)typeof(GenericArraySortHelper<string, string>).TypeHandle.Instantiate(new RuntimeTypeHandle[2]
				{
					typeof(TKey).TypeHandle,
					typeof(TValue).TypeHandle
				}).Allocate();
			}
			else
			{
				defaultArraySortHelper = new ArraySortHelper<TKey, TValue>();
			}
			return defaultArraySortHelper;
		}

		public void Sort(TKey[] keys, TValue[] values, int index, int length, IComparer<TKey> comparer)
		{
			try
			{
				if (comparer == null || comparer == Comparer<TKey>.Default)
				{
					comparer = Comparer<TKey>.Default;
				}
				QuickSort(keys, values, index, index + (length - 1), comparer);
			}
			catch (IndexOutOfRangeException)
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_BogusIComparer", null, typeof(TKey).Name, comparer));
			}
			catch (Exception innerException)
			{
				throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_IComparerFailed"), innerException);
			}
		}

		private static void SwapIfGreaterWithItems(TKey[] keys, TValue[] values, IComparer<TKey> comparer, int a, int b)
		{
			if (a != b && comparer.Compare(keys[a], keys[b]) > 0)
			{
				TKey val = keys[a];
				keys[a] = keys[b];
				keys[b] = val;
				if (values != null)
				{
					TValue val2 = values[a];
					values[a] = values[b];
					values[b] = val2;
				}
			}
		}

		internal static void QuickSort(TKey[] keys, TValue[] values, int left, int right, IComparer<TKey> comparer)
		{
			do
			{
				int num = left;
				int num2 = right;
				int num3 = num + (num2 - num >> 1);
				SwapIfGreaterWithItems(keys, values, comparer, num, num3);
				SwapIfGreaterWithItems(keys, values, comparer, num, num2);
				SwapIfGreaterWithItems(keys, values, comparer, num3, num2);
				TKey val = keys[num3];
				while (true)
				{
					if (comparer.Compare(keys[num], val) < 0)
					{
						num++;
						continue;
					}
					while (comparer.Compare(val, keys[num2]) < 0)
					{
						num2--;
					}
					if (num > num2)
					{
						break;
					}
					if (num < num2)
					{
						TKey val2 = keys[num];
						keys[num] = keys[num2];
						keys[num2] = val2;
						if (values != null)
						{
							TValue val3 = values[num];
							values[num] = values[num2];
							values[num2] = val3;
						}
					}
					num++;
					num2--;
					if (num > num2)
					{
						break;
					}
				}
				if (num2 - left <= right - num)
				{
					if (left < num2)
					{
						QuickSort(keys, values, left, num2, comparer);
					}
					left = num;
				}
				else
				{
					if (num < right)
					{
						QuickSort(keys, values, num, right, comparer);
					}
					right = num2;
				}
			}
			while (left < right);
		}
	}
}
