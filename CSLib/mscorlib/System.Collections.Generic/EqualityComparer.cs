using System.Runtime.CompilerServices;

namespace System.Collections.Generic
{
	[Serializable]
	[TypeDependency("System.Collections.Generic.GenericEqualityComparer`1")]
	public abstract class EqualityComparer<T> : IEqualityComparer, IEqualityComparer<T>
	{
		private static EqualityComparer<T> defaultComparer;

		public static EqualityComparer<T> Default
		{
			get
			{
				EqualityComparer<T> equalityComparer = defaultComparer;
				if (equalityComparer == null)
				{
					equalityComparer = (defaultComparer = CreateComparer());
				}
				return equalityComparer;
			}
		}

		private static EqualityComparer<T> CreateComparer()
		{
			Type typeFromHandle = typeof(T);
			if (typeFromHandle == typeof(byte))
			{
				return (EqualityComparer<T>)(object)new ByteEqualityComparer();
			}
			if (typeof(IEquatable<T>).IsAssignableFrom(typeFromHandle))
			{
				return (EqualityComparer<T>)typeof(GenericEqualityComparer<int>).TypeHandle.CreateInstanceForAnotherGenericParameter(typeFromHandle);
			}
			if (typeFromHandle.IsGenericType && typeFromHandle.GetGenericTypeDefinition() == typeof(Nullable<>))
			{
				Type type = typeFromHandle.GetGenericArguments()[0];
				if (typeof(IEquatable<>).MakeGenericType(type).IsAssignableFrom(type))
				{
					return (EqualityComparer<T>)typeof(NullableEqualityComparer<int>).TypeHandle.CreateInstanceForAnotherGenericParameter(type);
				}
			}
			return new ObjectEqualityComparer<T>();
		}

		public abstract bool Equals(T x, T y);

		public abstract int GetHashCode(T obj);

		internal virtual int IndexOf(T[] array, T value, int startIndex, int count)
		{
			int num = startIndex + count;
			for (int i = startIndex; i < num; i++)
			{
				if (Equals(array[i], value))
				{
					return i;
				}
			}
			return -1;
		}

		internal virtual int LastIndexOf(T[] array, T value, int startIndex, int count)
		{
			int num = startIndex - count + 1;
			for (int num2 = startIndex; num2 >= num; num2--)
			{
				if (Equals(array[num2], value))
				{
					return num2;
				}
			}
			return -1;
		}

		int IEqualityComparer.GetHashCode(object obj)
		{
			if (obj == null)
			{
				return 0;
			}
			if (obj is T)
			{
				return GetHashCode((T)obj);
			}
			ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_InvalidArgumentForComparison);
			return 0;
		}

		bool IEqualityComparer.Equals(object x, object y)
		{
			if (x == y)
			{
				return true;
			}
			if (x == null || y == null)
			{
				return false;
			}
			if (x is T && y is T)
			{
				return Equals((T)x, (T)y);
			}
			ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_InvalidArgumentForComparison);
			return false;
		}
	}
}
