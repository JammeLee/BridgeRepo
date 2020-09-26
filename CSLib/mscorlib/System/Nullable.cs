using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System
{
	[Serializable]
	[TypeDependency("System.Collections.Generic.NullableComparer`1")]
	[TypeDependency("System.Collections.Generic.NullableEqualityComparer`1")]
	public struct Nullable<T> where T : struct
	{
		private bool hasValue;

		internal T value;

		public bool HasValue => hasValue;

		public T Value
		{
			get
			{
				if (!HasValue)
				{
					ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_NoValue);
				}
				return value;
			}
		}

		public Nullable(T value)
		{
			this.value = value;
			hasValue = true;
		}

		public T GetValueOrDefault()
		{
			return value;
		}

		public T GetValueOrDefault(T defaultValue)
		{
			if (!HasValue)
			{
				return defaultValue;
			}
			return value;
		}

		public override bool Equals(object other)
		{
			if (!HasValue)
			{
				return other == null;
			}
			if (other == null)
			{
				return false;
			}
			return value.Equals(other);
		}

		public override int GetHashCode()
		{
			if (!HasValue)
			{
				return 0;
			}
			return value.GetHashCode();
		}

		public override string ToString()
		{
			if (!HasValue)
			{
				return "";
			}
			return value.ToString();
		}

		public static implicit operator T?(T value)
		{
			return value;
		}

		public static explicit operator T(T? value)
		{
			return value.Value;
		}
	}
	[ComVisible(true)]
	public static class Nullable
	{
		[ComVisible(true)]
		public static int Compare<T>(T? n1, T? n2) where T : struct
		{
			if (n1.HasValue)
			{
				if (n2.HasValue)
				{
					return Comparer<T>.Default.Compare(n1.value, n2.value);
				}
				return 1;
			}
			if (n2.HasValue)
			{
				return -1;
			}
			return 0;
		}

		[ComVisible(true)]
		public static bool Equals<T>(T? n1, T? n2) where T : struct
		{
			if (n1.HasValue)
			{
				if (n2.HasValue)
				{
					return EqualityComparer<T>.Default.Equals(n1.value, n2.value);
				}
				return false;
			}
			if (n2.HasValue)
			{
				return false;
			}
			return true;
		}

		public static Type GetUnderlyingType(Type nullableType)
		{
			if (nullableType == null)
			{
				throw new ArgumentNullException("nullableType");
			}
			Type result = null;
			if (nullableType.IsGenericType && !nullableType.IsGenericTypeDefinition)
			{
				Type genericTypeDefinition = nullableType.GetGenericTypeDefinition();
				if (genericTypeDefinition == typeof(Nullable<>))
				{
					result = nullableType.GetGenericArguments()[0];
				}
			}
			return result;
		}
	}
}
