namespace System.Collections.Generic
{
	[Serializable]
	internal class GenericArraySortHelper<T> : IArraySortHelper<T> where T : IComparable<T>
	{
		public void Sort(T[] keys, int index, int length, IComparer<T> comparer)
		{
			try
			{
				if (comparer == null || comparer == Comparer<T>.Default)
				{
					QuickSort(keys, index, index + (length - 1));
				}
				else
				{
					ArraySortHelper<T>.QuickSort(keys, index, index + (length - 1), comparer);
				}
			}
			catch (IndexOutOfRangeException)
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_BogusIComparer", default(T), typeof(T).Name, null));
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
				if (comparer == null || comparer == Comparer<T>.Default)
				{
					return BinarySearch(array, index, length, value);
				}
				return ArraySortHelper<T>.InternalBinarySearch(array, index, length, value, comparer);
			}
			catch (Exception innerException)
			{
				throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_IComparerFailed"), innerException);
			}
		}

		private static int BinarySearch(T[] array, int index, int length, T value)
		{
			int num = index;
			int num2 = index + length - 1;
			while (num <= num2)
			{
				int num3 = num + (num2 - num >> 1);
				int num4 = ((array[num3] != null) ? array[num3].CompareTo(value) : ((value != null) ? (-1) : 0));
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

		private static void SwapIfGreaterWithItems(T[] keys, int a, int b)
		{
			if (a != b && keys[a] != null && keys[a].CompareTo(keys[b]) > 0)
			{
				T val = keys[a];
				keys[a] = keys[b];
				keys[b] = val;
			}
		}

		private static void QuickSort(T[] keys, int left, int right)
		{
			do
			{
				int i = left;
				int num = right;
				int num2 = i + (num - i >> 1);
				SwapIfGreaterWithItems(keys, i, num2);
				SwapIfGreaterWithItems(keys, i, num);
				SwapIfGreaterWithItems(keys, num2, num);
				T val = keys[num2];
				do
				{
					if (val == null)
					{
						while (keys[num] != null)
						{
							num--;
						}
					}
					else
					{
						for (; val.CompareTo(keys[i]) > 0; i++)
						{
						}
						while (val.CompareTo(keys[num]) < 0)
						{
							num--;
						}
					}
					if (i > num)
					{
						break;
					}
					if (i < num)
					{
						T val2 = keys[i];
						keys[i] = keys[num];
						keys[num] = val2;
					}
					i++;
					num--;
				}
				while (i <= num);
				if (num - left <= right - i)
				{
					if (left < num)
					{
						QuickSort(keys, left, num);
					}
					left = i;
				}
				else
				{
					if (i < right)
					{
						QuickSort(keys, i, right);
					}
					right = num;
				}
			}
			while (left < right);
		}
	}
	internal class GenericArraySortHelper<TKey, TValue> : IArraySortHelper<TKey, TValue> where TKey : IComparable<TKey>
	{
		public void Sort(TKey[] keys, TValue[] values, int index, int length, IComparer<TKey> comparer)
		{
			try
			{
				if (comparer == null || comparer == Comparer<TKey>.Default)
				{
					QuickSort(keys, values, index, index + length - 1);
				}
				else
				{
					ArraySortHelper<TKey, TValue>.QuickSort(keys, values, index, index + length - 1, comparer);
				}
			}
			catch (IndexOutOfRangeException)
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_BogusIComparer", null, typeof(TKey).Name, null));
			}
			catch (Exception innerException)
			{
				throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_IComparerFailed"), innerException);
			}
		}

		private static void SwapIfGreaterWithItems(TKey[] keys, TValue[] values, int a, int b)
		{
			if (a != b && keys[a] != null && keys[a].CompareTo(keys[b]) > 0)
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

		private static void QuickSort(TKey[] keys, TValue[] values, int left, int right)
		{
			do
			{
				int i = left;
				int num = right;
				int num2 = i + (num - i >> 1);
				SwapIfGreaterWithItems(keys, values, i, num2);
				SwapIfGreaterWithItems(keys, values, i, num);
				SwapIfGreaterWithItems(keys, values, num2, num);
				TKey val = keys[num2];
				do
				{
					if (val == null)
					{
						while (keys[num] != null)
						{
							num--;
						}
					}
					else
					{
						for (; val.CompareTo(keys[i]) > 0; i++)
						{
						}
						while (val.CompareTo(keys[num]) < 0)
						{
							num--;
						}
					}
					if (i > num)
					{
						break;
					}
					if (i < num)
					{
						TKey val2 = keys[i];
						keys[i] = keys[num];
						keys[num] = val2;
						if (values != null)
						{
							TValue val3 = values[i];
							values[i] = values[num];
							values[num] = val3;
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
						QuickSort(keys, values, left, num);
					}
					left = i;
				}
				else
				{
					if (i < right)
					{
						QuickSort(keys, values, i, right);
					}
					right = num;
				}
			}
			while (left < right);
		}
	}
}
