using System.Globalization;

namespace System
{
	[Serializable]
	internal sealed class OrdinalComparer : StringComparer
	{
		private bool _ignoreCase;

		internal OrdinalComparer(bool ignoreCase)
		{
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
			if (_ignoreCase)
			{
				return TextInfo.CompareOrdinalIgnoreCase(x, y);
			}
			return string.CompareOrdinal(x, y);
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
			if (_ignoreCase)
			{
				if (x.Length != y.Length)
				{
					return false;
				}
				return TextInfo.CompareOrdinalIgnoreCase(x, y) == 0;
			}
			return x.Equals(y);
		}

		public override int GetHashCode(string obj)
		{
			if (obj == null)
			{
				throw new ArgumentNullException("obj");
			}
			if (_ignoreCase)
			{
				return TextInfo.GetHashCodeOrdinalIgnoreCase(obj);
			}
			return obj.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			OrdinalComparer ordinalComparer = obj as OrdinalComparer;
			if (ordinalComparer == null)
			{
				return false;
			}
			return _ignoreCase == ordinalComparer._ignoreCase;
		}

		public override int GetHashCode()
		{
			string text = "OrdinalComparer";
			int hashCode = text.GetHashCode();
			if (!_ignoreCase)
			{
				return hashCode;
			}
			return ~hashCode;
		}
	}
}
