using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;

namespace System
{
	[Serializable]
	[ComVisible(true)]
	public abstract class StringComparer : IComparer, IEqualityComparer, IComparer<string>, IEqualityComparer<string>
	{
		private static StringComparer _invariantCulture = new CultureAwareComparer(CultureInfo.InvariantCulture, ignoreCase: false);

		private static StringComparer _invariantCultureIgnoreCase = new CultureAwareComparer(CultureInfo.InvariantCulture, ignoreCase: true);

		private static StringComparer _ordinal = new OrdinalComparer(ignoreCase: false);

		private static StringComparer _ordinalIgnoreCase = new OrdinalComparer(ignoreCase: true);

		public static StringComparer InvariantCulture => _invariantCulture;

		public static StringComparer InvariantCultureIgnoreCase => _invariantCultureIgnoreCase;

		public static StringComparer CurrentCulture => new CultureAwareComparer(CultureInfo.CurrentCulture, ignoreCase: false);

		public static StringComparer CurrentCultureIgnoreCase => new CultureAwareComparer(CultureInfo.CurrentCulture, ignoreCase: true);

		public static StringComparer Ordinal => _ordinal;

		public static StringComparer OrdinalIgnoreCase => _ordinalIgnoreCase;

		public static StringComparer Create(CultureInfo culture, bool ignoreCase)
		{
			if (culture == null)
			{
				throw new ArgumentNullException("culture");
			}
			return new CultureAwareComparer(culture, ignoreCase);
		}

		public int Compare(object x, object y)
		{
			if (x == y)
			{
				return 0;
			}
			if (x == null)
			{
				return -1;
			}
			if (y == null)
			{
				return 1;
			}
			string text = x as string;
			if (text != null)
			{
				string text2 = y as string;
				if (text2 != null)
				{
					return Compare(text, text2);
				}
			}
			IComparable comparable = x as IComparable;
			if (comparable != null)
			{
				return comparable.CompareTo(y);
			}
			throw new ArgumentException(Environment.GetResourceString("Argument_ImplementIComparable"));
		}

		public new bool Equals(object x, object y)
		{
			if (x == y)
			{
				return true;
			}
			if (x == null || y == null)
			{
				return false;
			}
			string text = x as string;
			if (text != null)
			{
				string text2 = y as string;
				if (text2 != null)
				{
					return Equals(text, text2);
				}
			}
			return x.Equals(y);
		}

		public int GetHashCode(object obj)
		{
			if (obj == null)
			{
				throw new ArgumentNullException("obj");
			}
			string text = obj as string;
			if (text != null)
			{
				return GetHashCode(text);
			}
			return obj.GetHashCode();
		}

		public abstract int Compare(string x, string y);

		public abstract bool Equals(string x, string y);

		public abstract int GetHashCode(string obj);
	}
}
