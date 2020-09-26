using System.Globalization;

namespace System
{
	[Serializable]
	internal sealed class CultureAwareComparer : StringComparer
	{
		private CompareInfo _compareInfo;

		private bool _ignoreCase;

		internal CultureAwareComparer(CultureInfo culture, bool ignoreCase)
		{
			_compareInfo = culture.CompareInfo;
			_ignoreCase = ignoreCase;
		}

		public override int Compare(string x, string y)
		{
			if (object.ReferenceEquals(x, y))
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
			return _compareInfo.Compare(x, y, _ignoreCase ? CompareOptions.IgnoreCase : CompareOptions.None);
		}

		public override bool Equals(string x, string y)
		{
			if (object.ReferenceEquals(x, y))
			{
				return true;
			}
			if (x == null || y == null)
			{
				return false;
			}
			return _compareInfo.Compare(x, y, _ignoreCase ? CompareOptions.IgnoreCase : CompareOptions.None) == 0;
		}

		public override int GetHashCode(string obj)
		{
			if (obj == null)
			{
				throw new ArgumentNullException("obj");
			}
			if (_ignoreCase)
			{
				return _compareInfo.GetHashCodeOfString(obj, CompareOptions.IgnoreCase);
			}
			return _compareInfo.GetHashCodeOfString(obj, CompareOptions.None);
		}

		public override bool Equals(object obj)
		{
			CultureAwareComparer cultureAwareComparer = obj as CultureAwareComparer;
			if (cultureAwareComparer == null)
			{
				return false;
			}
			if (_ignoreCase == cultureAwareComparer._ignoreCase)
			{
				return _compareInfo.Equals(cultureAwareComparer._compareInfo);
			}
			return false;
		}

		public override int GetHashCode()
		{
			int hashCode = _compareInfo.GetHashCode();
			if (!_ignoreCase)
			{
				return hashCode;
			}
			return ~hashCode;
		}
	}
}
