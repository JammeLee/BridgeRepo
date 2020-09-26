using System.Runtime.CompilerServices;

namespace System.Collections.Generic
{
	[Serializable]
	[TypeDependency("System.Collections.Generic.GenericComparer`1")]
	public abstract class Comparer<T> : IComparer, IComparer<T>
	{
		private static Comparer<T> defaultComparer;

		public static Comparer<T> Default
		{
			get
			{
				Comparer<T> comparer = defaultComparer;
				if (comparer == null)
				{
					comparer = (defaultComparer = CreateComparer());
				}
				return comparer;
			}
		}

		private static Comparer<T> CreateComparer()
		{
			Type typeFromHandle = typeof(T);
			if (typeof(IComparable<T>).IsAssignableFrom(typeFromHandle))
			{
				return (Comparer<T>)typeof(GenericComparer<int>).TypeHandle.CreateInstanceForAnotherGenericParameter(typeFromHandle);
			}
			if (typeFromHandle.IsGenericType && typeFromHandle.GetGenericTypeDefinition() == typeof(Nullable<>))
			{
				Type type = typeFromHandle.GetGenericArguments()[0];
				if (typeof(IComparable<>).MakeGenericType(type).IsAssignableFrom(type))
				{
					return (Comparer<T>)typeof(NullableComparer<int>).TypeHandle.CreateInstanceForAnotherGenericParameter(type);
				}
			}
			return new ObjectComparer<T>();
		}

		public abstract int Compare(T x, T y);

		int IComparer.Compare(object x, object y)
		{
			if (x == null)
			{
				if (y != null)
				{
					return -1;
				}
				return 0;
			}
			if (y == null)
			{
				return 1;
			}
			if (x is T && y is T)
			{
				return Compare((T)x, (T)y);
			}
			ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_InvalidArgumentForComparison);
			return 0;
		}
	}
}
