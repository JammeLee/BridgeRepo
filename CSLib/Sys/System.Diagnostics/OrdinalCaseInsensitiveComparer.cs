using System.Collections;

namespace System.Diagnostics
{
	internal class OrdinalCaseInsensitiveComparer : IComparer
	{
		internal static readonly OrdinalCaseInsensitiveComparer Default = new OrdinalCaseInsensitiveComparer();

		public int Compare(object a, object b)
		{
			string text = a as string;
			string text2 = b as string;
			if (text != null && text2 != null)
			{
				return string.CompareOrdinal(text.ToUpperInvariant(), text2.ToUpperInvariant());
			}
			return Comparer.Default.Compare(a, b);
		}
	}
}
