using System.Runtime.InteropServices;

namespace System.Globalization
{
	[Serializable]
	[ComVisible(true)]
	public class SortKey
	{
		private const CompareOptions ValidSortkeyCtorMaskOffFlags = ~(CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreSymbols | CompareOptions.IgnoreKanaType | CompareOptions.IgnoreWidth | CompareOptions.StringSort);

		internal int win32LCID;

		internal CompareOptions options;

		internal string m_String;

		internal byte[] m_KeyData;

		public virtual string OriginalString => m_String;

		public virtual byte[] KeyData => (byte[])m_KeyData.Clone();

		internal unsafe SortKey(void* pSortingFile, int win32LCID, string str, CompareOptions options)
		{
			if (str == null)
			{
				throw new ArgumentNullException("str");
			}
			if (((uint)options & 0xDFFFFFE0u) != 0)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_InvalidFlag"), "options");
			}
			this.win32LCID = win32LCID;
			this.options = options;
			m_String = str;
			m_KeyData = CompareInfo.nativeCreateSortKey(pSortingFile, str, (int)options, win32LCID);
		}

		internal SortKey(int win32LCID, string str, CompareOptions options)
		{
			if (str == null)
			{
				throw new ArgumentNullException("str");
			}
			if (((uint)options & 0xDFFFFFE0u) != 0)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_InvalidFlag"), "options");
			}
			if (CultureInfo.GetNativeSortKey(win32LCID, CompareInfo.GetNativeCompareFlags(options), str, str.Length, out m_KeyData) < 0)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_InvalidFlag"), "str");
			}
			this.win32LCID = win32LCID;
			this.options = options;
			m_String = str;
		}

		public static int Compare(SortKey sortkey1, SortKey sortkey2)
		{
			if (sortkey1 == null || sortkey2 == null)
			{
				throw new ArgumentNullException((sortkey1 == null) ? "sortkey1" : "sortkey2");
			}
			byte[] keyData = sortkey1.m_KeyData;
			byte[] keyData2 = sortkey2.m_KeyData;
			if (keyData.Length == 0)
			{
				if (keyData2.Length == 0)
				{
					return 0;
				}
				return -1;
			}
			if (keyData2.Length == 0)
			{
				return 1;
			}
			int num = ((keyData.Length < keyData2.Length) ? keyData.Length : keyData2.Length);
			for (int i = 0; i < num; i++)
			{
				if (keyData[i] > keyData2[i])
				{
					return 1;
				}
				if (keyData[i] < keyData2[i])
				{
					return -1;
				}
			}
			return 0;
		}

		public override bool Equals(object value)
		{
			SortKey sortKey = value as SortKey;
			if (sortKey != null)
			{
				return Compare(this, sortKey) == 0;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return CompareInfo.GetCompareInfo(win32LCID).GetHashCodeOfString(m_String, options);
		}

		public override string ToString()
		{
			return string.Concat("SortKey - ", win32LCID, ", ", options, ", ", m_String);
		}
	}
}
