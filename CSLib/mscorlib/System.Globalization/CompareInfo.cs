using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Threading;
using Microsoft.Win32;

namespace System.Globalization
{
	[Serializable]
	[ComVisible(true)]
	public class CompareInfo : IDeserializationCallback
	{
		private const CompareOptions ValidIndexMaskOffFlags = ~(CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreSymbols | CompareOptions.IgnoreKanaType | CompareOptions.IgnoreWidth);

		private const CompareOptions ValidCompareMaskOffFlags = ~(CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreSymbols | CompareOptions.IgnoreKanaType | CompareOptions.IgnoreWidth | CompareOptions.StringSort);

		private const CompareOptions ValidHashCodeOfStringMaskOffFlags = ~(CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreSymbols | CompareOptions.IgnoreKanaType | CompareOptions.IgnoreWidth);

		private const int TraditionalChineseCultureId = 31748;

		private const int HongKongCultureId = 3076;

		internal const int CHT_CHS_LCID_COMPAREINFO_KEY_FLAG = int.MinValue;

		private const int NORM_IGNORECASE = 1;

		private const int NORM_IGNOREKANATYPE = 65536;

		private const int NORM_IGNORENONSPACE = 2;

		private const int NORM_IGNORESYMBOLS = 4;

		private const int NORM_IGNOREWIDTH = 131072;

		private const int SORT_STRINGSORT = 4096;

		private static object s_InternalSyncObject;

		private int win32LCID;

		private int culture;

		[NonSerialized]
		internal unsafe void* m_pSortingTable;

		[NonSerialized]
		private int m_sortingLCID;

		[NonSerialized]
		private CultureTableRecord m_cultureTableRecord;

		[OptionalField(VersionAdded = 2)]
		private string m_name;

		[NonSerialized]
		private static int fFindNLSStringSupported;

		private static object InternalSyncObject
		{
			get
			{
				if (s_InternalSyncObject == null)
				{
					object value = new object();
					Interlocked.CompareExchange(ref s_InternalSyncObject, value, null);
				}
				return s_InternalSyncObject;
			}
		}

		[ComVisible(false)]
		public virtual string Name
		{
			get
			{
				if (m_name == null)
				{
					m_name = CultureInfo.GetCultureInfo(culture).SortName;
				}
				return m_name;
			}
		}

		internal CultureTableRecord CultureTableRecord
		{
			get
			{
				if (m_cultureTableRecord == null)
				{
					m_cultureTableRecord = CultureInfo.GetCultureInfo(m_sortingLCID).m_cultureTableRecord;
				}
				return m_cultureTableRecord;
			}
		}

		private bool IsSynthetic => CultureTableRecord.IsSynthetic;

		public int LCID => culture;

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal unsafe static extern byte[] nativeCreateSortKey(void* pSortingFile, string pString, int dwFlags, int win32LCID);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal unsafe static extern int nativeGetGlobalizedHashCode(void* pSortingFile, string pString, int dwFlags, int win32LCID);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal unsafe static extern bool nativeIsSortable(void* pSortingFile, string pString);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern int nativeCompareString(int lcid, string string1, int offset1, int length1, string string2, int offset2, int length2, int flags);

		public static CompareInfo GetCompareInfo(int culture, Assembly assembly)
		{
			return GetCompareInfoWithPrefixedLcid(culture, assembly, 0);
		}

		private static CompareInfo GetCompareInfoWithPrefixedLcid(int cultureKey, Assembly assembly, int prefix)
		{
			if (assembly == null)
			{
				throw new ArgumentNullException("assembly");
			}
			int cultureId = cultureKey & ~prefix;
			if (CultureTableRecord.IsCustomCultureId(cultureId))
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_CustomCultureCannotBePassedByNumber", "culture"));
			}
			if (assembly != typeof(object).Module.Assembly)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_OnlyMscorlib"));
			}
			GlobalizationAssembly globalizationAssembly = GlobalizationAssembly.GetGlobalizationAssembly(assembly);
			object obj = globalizationAssembly.compareInfoCache[cultureKey];
			if (obj == null)
			{
				lock (InternalSyncObject)
				{
					if ((obj = globalizationAssembly.compareInfoCache[cultureKey]) == null)
					{
						obj = new CompareInfo(globalizationAssembly, cultureId);
						Thread.MemoryBarrier();
						globalizationAssembly.compareInfoCache[cultureKey] = obj;
					}
				}
			}
			return (CompareInfo)obj;
		}

		private static CompareInfo GetCompareInfoByName(string name, Assembly assembly)
		{
			CultureInfo cultureInfo = CultureInfo.GetCultureInfo(name);
			if (cultureInfo.IsNeutralCulture && !CultureTableRecord.IsCustomCultureId(cultureInfo.cultureID))
			{
				cultureInfo = ((cultureInfo.cultureID != 31748) ? CultureInfo.GetCultureInfo(cultureInfo.CompareInfoId) : CultureInfo.GetCultureInfo(3076));
			}
			int num = cultureInfo.CompareInfoId;
			if (cultureInfo.Name == "zh-CHS" || cultureInfo.Name == "zh-CHT")
			{
				num |= int.MinValue;
			}
			CompareInfo compareInfo = ((assembly == null) ? GetCompareInfoWithPrefixedLcid(num, int.MinValue) : GetCompareInfoWithPrefixedLcid(num, assembly, int.MinValue));
			compareInfo.m_name = cultureInfo.SortName;
			return compareInfo;
		}

		public static CompareInfo GetCompareInfo(string name, Assembly assembly)
		{
			if (name == null || assembly == null)
			{
				throw new ArgumentNullException((name == null) ? "name" : "assembly");
			}
			if (assembly != typeof(object).Module.Assembly)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_OnlyMscorlib"));
			}
			return GetCompareInfoByName(name, assembly);
		}

		public static CompareInfo GetCompareInfo(int culture)
		{
			return GetCompareInfoWithPrefixedLcid(culture, 0);
		}

		internal static CompareInfo GetCompareInfoWithPrefixedLcid(int cultureKey, int prefix)
		{
			int cultureId = cultureKey & ~prefix;
			if (CultureTableRecord.IsCustomCultureId(cultureId))
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_CustomCultureCannotBePassedByNumber", "culture"));
			}
			object obj = GlobalizationAssembly.DefaultInstance.compareInfoCache[cultureKey];
			if (obj == null)
			{
				lock (InternalSyncObject)
				{
					if ((obj = GlobalizationAssembly.DefaultInstance.compareInfoCache[cultureKey]) == null)
					{
						obj = new CompareInfo(GlobalizationAssembly.DefaultInstance, cultureId);
						Thread.MemoryBarrier();
						GlobalizationAssembly.DefaultInstance.compareInfoCache[cultureKey] = obj;
					}
				}
			}
			return (CompareInfo)obj;
		}

		public static CompareInfo GetCompareInfo(string name)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			return GetCompareInfoByName(name, null);
		}

		[ComVisible(false)]
		public static bool IsSortable(char ch)
		{
			return IsSortable(ch.ToString());
		}

		[ComVisible(false)]
		public unsafe static bool IsSortable(string text)
		{
			if (text == null)
			{
				throw new ArgumentNullException("text");
			}
			if (text.Length == 0)
			{
				return false;
			}
			return nativeIsSortable(CultureInfo.InvariantCulture.CompareInfo.m_pSortingTable, text);
		}

		[OnDeserializing]
		private void OnDeserializing(StreamingContext ctx)
		{
			culture = -1;
			m_sortingLCID = -1;
		}

		private unsafe void OnDeserialized()
		{
			if (m_sortingLCID <= 0)
			{
				m_sortingLCID = GetSortingLCID(culture);
			}
			if (m_pSortingTable == null && !IsSynthetic)
			{
				m_pSortingTable = InitializeCompareInfo(GlobalizationAssembly.DefaultInstance.pNativeGlobalizationAssembly, m_sortingLCID);
			}
		}

		[OnDeserialized]
		private void OnDeserialized(StreamingContext ctx)
		{
			OnDeserialized();
		}

		[OnSerializing]
		private void OnSerializing(StreamingContext ctx)
		{
			win32LCID = m_sortingLCID;
		}

		internal void SetName(string name)
		{
			m_name = name;
		}

		internal static void ClearDefaultAssemblyCache()
		{
			lock (InternalSyncObject)
			{
				GlobalizationAssembly.DefaultInstance.compareInfoCache = new Hashtable(4);
			}
		}

		internal unsafe CompareInfo(GlobalizationAssembly ga, int culture)
		{
			if (culture < 0)
			{
				throw new ArgumentOutOfRangeException("culture", Environment.GetResourceString("ArgumentOutOfRange_NeedPosNum"));
			}
			m_sortingLCID = GetSortingLCID(culture);
			if (!IsSynthetic)
			{
				m_pSortingTable = InitializeCompareInfo(GlobalizationAssembly.DefaultInstance.pNativeGlobalizationAssembly, m_sortingLCID);
			}
			this.culture = culture;
		}

		internal int GetSortingLCID(int culture)
		{
			int num = 0;
			CultureInfo cultureInfo = CultureInfo.GetCultureInfo(culture);
			if (cultureInfo.m_cultureTableRecord.IsSynthetic)
			{
				return culture;
			}
			num = cultureInfo.CompareInfoId;
			int sortID = CultureInfo.GetSortID(culture);
			if (sortID != 0)
			{
				if (!cultureInfo.m_cultureTableRecord.IsValidSortID(sortID))
				{
					throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Argument_CultureNotSupported"), culture), "culture");
				}
				num |= sortID << 16;
			}
			return num;
		}

		internal static int GetNativeCompareFlags(CompareOptions options)
		{
			int num = 0;
			if ((options & CompareOptions.IgnoreCase) != 0)
			{
				num |= 1;
			}
			if ((options & CompareOptions.IgnoreKanaType) != 0)
			{
				num |= 0x10000;
			}
			if ((options & CompareOptions.IgnoreNonSpace) != 0)
			{
				num |= 2;
			}
			if ((options & CompareOptions.IgnoreSymbols) != 0)
			{
				num |= 4;
			}
			if ((options & CompareOptions.IgnoreWidth) != 0)
			{
				num |= 0x20000;
			}
			if ((options & CompareOptions.StringSort) != 0)
			{
				num |= 0x1000;
			}
			return num;
		}

		public virtual int Compare(string string1, string string2)
		{
			return Compare(string1, string2, CompareOptions.None);
		}

		public unsafe virtual int Compare(string string1, string string2, CompareOptions options)
		{
			if (options == CompareOptions.OrdinalIgnoreCase)
			{
				return string.Compare(string1, string2, StringComparison.OrdinalIgnoreCase);
			}
			if ((options & CompareOptions.Ordinal) != 0)
			{
				if (options == CompareOptions.Ordinal)
				{
					if (string1 == null)
					{
						if (string2 == null)
						{
							return 0;
						}
						return -1;
					}
					if (string2 == null)
					{
						return 1;
					}
					return string.nativeCompareOrdinal(string1, string2, bIgnoreCase: false);
				}
				throw new ArgumentException(Environment.GetResourceString("Argument_CompareOptionOrdinal"), "options");
			}
			if (((uint)options & 0xDFFFFFE0u) != 0)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_InvalidFlag"), "options");
			}
			if (string1 == null)
			{
				if (string2 == null)
				{
					return 0;
				}
				return -1;
			}
			if (string2 == null)
			{
				return 1;
			}
			if (IsSynthetic)
			{
				if (options == CompareOptions.Ordinal)
				{
					return Compare(CultureInfo.InvariantCulture.CompareInfo.m_pSortingTable, m_sortingLCID, string1, string2, options);
				}
				return nativeCompareString(m_sortingLCID, string1, 0, string1.Length, string2, 0, string2.Length, GetNativeCompareFlags(options));
			}
			return Compare(m_pSortingTable, m_sortingLCID, string1, string2, options);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private unsafe static extern int Compare(void* pSortingTable, int sortingLCID, string string1, string string2, CompareOptions options);

		public virtual int Compare(string string1, int offset1, int length1, string string2, int offset2, int length2)
		{
			return Compare(string1, offset1, length1, string2, offset2, length2, CompareOptions.None);
		}

		public virtual int Compare(string string1, int offset1, string string2, int offset2, CompareOptions options)
		{
			return Compare(string1, offset1, (string1 != null) ? (string1.Length - offset1) : 0, string2, offset2, (string2 != null) ? (string2.Length - offset2) : 0, options);
		}

		public virtual int Compare(string string1, int offset1, string string2, int offset2)
		{
			return Compare(string1, offset1, string2, offset2, CompareOptions.None);
		}

		public unsafe virtual int Compare(string string1, int offset1, int length1, string string2, int offset2, int length2, CompareOptions options)
		{
			if (options == CompareOptions.OrdinalIgnoreCase)
			{
				int num = string.Compare(string1, offset1, string2, offset2, (length1 < length2) ? length1 : length2, StringComparison.OrdinalIgnoreCase);
				if (length1 != length2 && num == 0)
				{
					if (length1 <= length2)
					{
						return -1;
					}
					return 1;
				}
				return num;
			}
			if (length1 < 0 || length2 < 0)
			{
				throw new ArgumentOutOfRangeException((length1 < 0) ? "length1" : "length2", Environment.GetResourceString("ArgumentOutOfRange_NeedPosNum"));
			}
			if (offset1 < 0 || offset2 < 0)
			{
				throw new ArgumentOutOfRangeException((offset1 < 0) ? "offset1" : "offset2", Environment.GetResourceString("ArgumentOutOfRange_NeedPosNum"));
			}
			if (offset1 > (string1?.Length ?? 0) - length1)
			{
				throw new ArgumentOutOfRangeException("string1", Environment.GetResourceString("ArgumentOutOfRange_OffsetLength"));
			}
			if (offset2 > (string2?.Length ?? 0) - length2)
			{
				throw new ArgumentOutOfRangeException("string2", Environment.GetResourceString("ArgumentOutOfRange_OffsetLength"));
			}
			if ((options & CompareOptions.Ordinal) != 0)
			{
				if (options == CompareOptions.Ordinal)
				{
					if (string1 == null)
					{
						if (string2 == null)
						{
							return 0;
						}
						return -1;
					}
					if (string2 == null)
					{
						return 1;
					}
					int num2 = string.nativeCompareOrdinalEx(string1, offset1, string2, offset2, (length1 < length2) ? length1 : length2);
					if (length1 != length2 && num2 == 0)
					{
						if (length1 <= length2)
						{
							return -1;
						}
						return 1;
					}
					return num2;
				}
				throw new ArgumentException(Environment.GetResourceString("Argument_CompareOptionOrdinal"), "options");
			}
			if (((uint)options & 0xDFFFFFE0u) != 0)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_InvalidFlag"), "options");
			}
			if (string1 == null)
			{
				if (string2 == null)
				{
					return 0;
				}
				return -1;
			}
			if (string2 == null)
			{
				return 1;
			}
			if (IsSynthetic)
			{
				if (options == CompareOptions.Ordinal)
				{
					return CompareRegion(CultureInfo.InvariantCulture.CompareInfo.m_pSortingTable, m_sortingLCID, string1, offset1, length1, string2, offset2, length2, options);
				}
				return nativeCompareString(m_sortingLCID, string1, offset1, length1, string2, offset2, length2, GetNativeCompareFlags(options));
			}
			return CompareRegion(m_pSortingTable, m_sortingLCID, string1, offset1, length1, string2, offset2, length2, options);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private unsafe static extern int CompareRegion(void* pSortingTable, int sortingLCID, string string1, int offset1, int length1, string string2, int offset2, int length2, CompareOptions options);

		private unsafe static int FindNLSStringWrap(int lcid, int flags, string src, int start, int cchSrc, string value, int cchValue)
		{
			int num = -1;
			fixed (char* ptr = src)
			{
				fixed (char* lpStringValue = value)
				{
					if (1 == fFindNLSStringSupported)
					{
						num = Win32Native.FindNLSString(lcid, flags, ptr + start, cchSrc, lpStringValue, cchValue, IntPtr.Zero);
					}
					else
					{
						try
						{
							num = Win32Native.FindNLSString(lcid, flags, ptr + start, cchSrc, lpStringValue, cchValue, IntPtr.Zero);
							fFindNLSStringSupported = 1;
						}
						catch (EntryPointNotFoundException)
						{
							num = (fFindNLSStringSupported = -2);
						}
					}
				}
			}
			return num;
		}

		private bool SyntheticIsPrefix(string source, int start, int length, string prefix, int nativeCompareFlags)
		{
			if (fFindNLSStringSupported >= 0)
			{
				int num = FindNLSStringWrap(m_sortingLCID, nativeCompareFlags | 0x100000, source, start, length, prefix, prefix.Length);
				if (num >= -1)
				{
					return num != -1;
				}
			}
			for (int i = 1; i <= length; i++)
			{
				if (nativeCompareString(m_sortingLCID, prefix, 0, prefix.Length, source, start, i, nativeCompareFlags) == 0)
				{
					return true;
				}
			}
			return false;
		}

		private int SyntheticIsSuffix(string source, int end, int length, string suffix, int nativeCompareFlags)
		{
			if (fFindNLSStringSupported >= 0)
			{
				int num = FindNLSStringWrap(m_sortingLCID, nativeCompareFlags | 0x200000, source, 0, length, suffix, suffix.Length);
				if (num >= -1)
				{
					return num;
				}
			}
			for (int i = 0; i < length; i++)
			{
				if (nativeCompareString(m_sortingLCID, suffix, 0, suffix.Length, source, end - i, i + 1, nativeCompareFlags) == 0)
				{
					return end - i;
				}
			}
			return -1;
		}

		private int SyntheticIndexOf(string source, string value, int start, int length, int nativeCompareFlags)
		{
			if (fFindNLSStringSupported >= 0)
			{
				int num = FindNLSStringWrap(m_sortingLCID, nativeCompareFlags | 0x400000, source, start, length, value, value.Length);
				if (num > -1)
				{
					return num + start;
				}
				if (num == -1)
				{
					return num;
				}
			}
			for (int i = 0; i < length; i++)
			{
				if (SyntheticIsPrefix(source, start + i, length - i, value, nativeCompareFlags))
				{
					return start + i;
				}
			}
			return -1;
		}

		private int SyntheticLastIndexOf(string source, string value, int startIndex, int length, int nativeCompareFlags)
		{
			if (fFindNLSStringSupported >= 0)
			{
				int num = FindNLSStringWrap(m_sortingLCID, nativeCompareFlags | 0x800000, source, startIndex - length + 1, length, value, value.Length);
				if (num > -1)
				{
					return num + (startIndex - length + 1);
				}
				if (num == -1)
				{
					return num;
				}
			}
			for (int i = 0; i < length; i++)
			{
				int num2 = SyntheticIsSuffix(source, startIndex - i, length - i, value, nativeCompareFlags);
				if (num2 >= 0)
				{
					return num2;
				}
			}
			return -1;
		}

		public unsafe virtual bool IsPrefix(string source, string prefix, CompareOptions options)
		{
			if (source == null || prefix == null)
			{
				throw new ArgumentNullException((source == null) ? "source" : "prefix", Environment.GetResourceString("ArgumentNull_String"));
			}
			if (prefix.Length == 0)
			{
				return true;
			}
			if (options == CompareOptions.OrdinalIgnoreCase)
			{
				return source.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);
			}
			if (((uint)options & 0xFFFFFFE0u) != 0 && options != CompareOptions.Ordinal)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_InvalidFlag"), "options");
			}
			if (IsSynthetic)
			{
				if (options == CompareOptions.Ordinal)
				{
					return nativeIsPrefix(CultureInfo.InvariantCulture.CompareInfo.m_pSortingTable, m_sortingLCID, source, prefix, options);
				}
				return SyntheticIsPrefix(source, 0, source.Length, prefix, GetNativeCompareFlags(options));
			}
			return nativeIsPrefix(m_pSortingTable, m_sortingLCID, source, prefix, options);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private unsafe static extern bool nativeIsPrefix(void* pSortingTable, int sortingLCID, string source, string prefix, CompareOptions options);

		public virtual bool IsPrefix(string source, string prefix)
		{
			return IsPrefix(source, prefix, CompareOptions.None);
		}

		public unsafe virtual bool IsSuffix(string source, string suffix, CompareOptions options)
		{
			if (source == null || suffix == null)
			{
				throw new ArgumentNullException((source == null) ? "source" : "suffix", Environment.GetResourceString("ArgumentNull_String"));
			}
			if (suffix.Length == 0)
			{
				return true;
			}
			if (options == CompareOptions.OrdinalIgnoreCase)
			{
				return source.EndsWith(suffix, StringComparison.OrdinalIgnoreCase);
			}
			if (((uint)options & 0xFFFFFFE0u) != 0 && options != CompareOptions.Ordinal)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_InvalidFlag"), "options");
			}
			if (IsSynthetic)
			{
				if (options == CompareOptions.Ordinal)
				{
					return nativeIsSuffix(CultureInfo.InvariantCulture.CompareInfo.m_pSortingTable, m_sortingLCID, source, suffix, options);
				}
				return SyntheticIsSuffix(source, source.Length - 1, source.Length, suffix, GetNativeCompareFlags(options)) >= 0;
			}
			return nativeIsSuffix(m_pSortingTable, m_sortingLCID, source, suffix, options);
		}

		public virtual bool IsSuffix(string source, string suffix)
		{
			return IsSuffix(source, suffix, CompareOptions.None);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private unsafe static extern bool nativeIsSuffix(void* pSortingTable, int sortingLCID, string source, string prefix, CompareOptions options);

		public virtual int IndexOf(string source, char value)
		{
			if (source == null)
			{
				throw new ArgumentNullException("source");
			}
			return IndexOf(source, value, 0, source.Length, CompareOptions.None);
		}

		public virtual int IndexOf(string source, string value)
		{
			if (source == null)
			{
				throw new ArgumentNullException("source");
			}
			return IndexOf(source, value, 0, source.Length, CompareOptions.None);
		}

		public virtual int IndexOf(string source, char value, CompareOptions options)
		{
			if (source == null)
			{
				throw new ArgumentNullException("source");
			}
			return IndexOf(source, value, 0, source.Length, options);
		}

		public virtual int IndexOf(string source, string value, CompareOptions options)
		{
			if (source == null)
			{
				throw new ArgumentNullException("source");
			}
			return IndexOf(source, value, 0, source.Length, options);
		}

		public virtual int IndexOf(string source, char value, int startIndex)
		{
			if (source == null)
			{
				throw new ArgumentNullException("source");
			}
			return IndexOf(source, value, startIndex, source.Length - startIndex, CompareOptions.None);
		}

		public virtual int IndexOf(string source, string value, int startIndex)
		{
			if (source == null)
			{
				throw new ArgumentNullException("source");
			}
			return IndexOf(source, value, startIndex, source.Length - startIndex, CompareOptions.None);
		}

		public virtual int IndexOf(string source, char value, int startIndex, CompareOptions options)
		{
			if (source == null)
			{
				throw new ArgumentNullException("source");
			}
			return IndexOf(source, value, startIndex, source.Length - startIndex, options);
		}

		public virtual int IndexOf(string source, string value, int startIndex, CompareOptions options)
		{
			if (source == null)
			{
				throw new ArgumentNullException("source");
			}
			return IndexOf(source, value, startIndex, source.Length - startIndex, options);
		}

		public virtual int IndexOf(string source, char value, int startIndex, int count)
		{
			return IndexOf(source, value, startIndex, count, CompareOptions.None);
		}

		public virtual int IndexOf(string source, string value, int startIndex, int count)
		{
			return IndexOf(source, value, startIndex, count, CompareOptions.None);
		}

		public unsafe virtual int IndexOf(string source, char value, int startIndex, int count, CompareOptions options)
		{
			if (source == null)
			{
				throw new ArgumentNullException("source");
			}
			if (startIndex < 0 || startIndex > source.Length)
			{
				throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("ArgumentOutOfRange_Index"));
			}
			if (count < 0 || startIndex > source.Length - count)
			{
				throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("ArgumentOutOfRange_Count"));
			}
			if (options == CompareOptions.OrdinalIgnoreCase)
			{
				return TextInfo.nativeIndexOfCharOrdinalIgnoreCase(TextInfo.InvariantNativeTextInfo, source, value, startIndex, count);
			}
			if (((uint)options & 0xFFFFFFE0u) != 0 && options != CompareOptions.Ordinal)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_InvalidFlag"), "options");
			}
			if (IsSynthetic)
			{
				if (options == CompareOptions.Ordinal)
				{
					return IndexOfChar(CultureInfo.InvariantCulture.CompareInfo.m_pSortingTable, m_sortingLCID, source, value, startIndex, count, (int)options);
				}
				return SyntheticIndexOf(source, new string(value, 1), startIndex, count, GetNativeCompareFlags(options));
			}
			return IndexOfChar(m_pSortingTable, m_sortingLCID, source, value, startIndex, count, (int)options);
		}

		public unsafe virtual int IndexOf(string source, string value, int startIndex, int count, CompareOptions options)
		{
			if (source == null)
			{
				throw new ArgumentNullException("source");
			}
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			if (startIndex > source.Length)
			{
				throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("ArgumentOutOfRange_Index"));
			}
			if (source.Length == 0)
			{
				if (value.Length == 0)
				{
					return 0;
				}
				return -1;
			}
			if (startIndex < 0)
			{
				throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("ArgumentOutOfRange_Index"));
			}
			if (count < 0 || startIndex > source.Length - count)
			{
				throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("ArgumentOutOfRange_Count"));
			}
			if (options == CompareOptions.OrdinalIgnoreCase)
			{
				return TextInfo.IndexOfStringOrdinalIgnoreCase(source, value, startIndex, count);
			}
			if (((uint)options & 0xFFFFFFE0u) != 0 && options != CompareOptions.Ordinal)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_InvalidFlag"), "options");
			}
			if (IsSynthetic)
			{
				if (options == CompareOptions.Ordinal)
				{
					return IndexOfString(CultureInfo.InvariantCulture.CompareInfo.m_pSortingTable, m_sortingLCID, source, value, startIndex, count, (int)options);
				}
				return SyntheticIndexOf(source, value, startIndex, count, GetNativeCompareFlags(options));
			}
			return IndexOfString(m_pSortingTable, m_sortingLCID, source, value, startIndex, count, (int)options);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private unsafe static extern int IndexOfChar(void* pSortingTable, int sortingLCID, string source, char value, int startIndex, int count, int options);

		[MethodImpl(MethodImplOptions.InternalCall)]
		private unsafe static extern int IndexOfString(void* pSortingTable, int sortingLCID, string source, string value, int startIndex, int count, int options);

		public virtual int LastIndexOf(string source, char value)
		{
			if (source == null)
			{
				throw new ArgumentNullException("source");
			}
			return LastIndexOf(source, value, source.Length - 1, source.Length, CompareOptions.None);
		}

		public virtual int LastIndexOf(string source, string value)
		{
			if (source == null)
			{
				throw new ArgumentNullException("source");
			}
			return LastIndexOf(source, value, source.Length - 1, source.Length, CompareOptions.None);
		}

		public virtual int LastIndexOf(string source, char value, CompareOptions options)
		{
			if (source == null)
			{
				throw new ArgumentNullException("source");
			}
			return LastIndexOf(source, value, source.Length - 1, source.Length, options);
		}

		public virtual int LastIndexOf(string source, string value, CompareOptions options)
		{
			if (source == null)
			{
				throw new ArgumentNullException("source");
			}
			return LastIndexOf(source, value, source.Length - 1, source.Length, options);
		}

		public virtual int LastIndexOf(string source, char value, int startIndex)
		{
			return LastIndexOf(source, value, startIndex, startIndex + 1, CompareOptions.None);
		}

		public virtual int LastIndexOf(string source, string value, int startIndex)
		{
			return LastIndexOf(source, value, startIndex, startIndex + 1, CompareOptions.None);
		}

		public virtual int LastIndexOf(string source, char value, int startIndex, CompareOptions options)
		{
			return LastIndexOf(source, value, startIndex, startIndex + 1, options);
		}

		public virtual int LastIndexOf(string source, string value, int startIndex, CompareOptions options)
		{
			return LastIndexOf(source, value, startIndex, startIndex + 1, options);
		}

		public virtual int LastIndexOf(string source, char value, int startIndex, int count)
		{
			return LastIndexOf(source, value, startIndex, count, CompareOptions.None);
		}

		public virtual int LastIndexOf(string source, string value, int startIndex, int count)
		{
			return LastIndexOf(source, value, startIndex, count, CompareOptions.None);
		}

		public unsafe virtual int LastIndexOf(string source, char value, int startIndex, int count, CompareOptions options)
		{
			if (source == null)
			{
				throw new ArgumentNullException("source");
			}
			if (((uint)options & 0xFFFFFFE0u) != 0 && options != CompareOptions.Ordinal && options != CompareOptions.OrdinalIgnoreCase)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_InvalidFlag"), "options");
			}
			if (source.Length == 0 && (startIndex == -1 || startIndex == 0))
			{
				return -1;
			}
			if (startIndex < 0 || startIndex > source.Length)
			{
				throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("ArgumentOutOfRange_Index"));
			}
			if (startIndex == source.Length)
			{
				startIndex--;
				if (count > 0)
				{
					count--;
				}
			}
			if (count < 0 || startIndex - count + 1 < 0)
			{
				throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("ArgumentOutOfRange_Count"));
			}
			if (options == CompareOptions.OrdinalIgnoreCase)
			{
				return TextInfo.nativeLastIndexOfCharOrdinalIgnoreCase(TextInfo.InvariantNativeTextInfo, source, value, startIndex, count);
			}
			if (IsSynthetic)
			{
				if (options == CompareOptions.Ordinal)
				{
					return LastIndexOfChar(CultureInfo.InvariantCulture.CompareInfo.m_pSortingTable, m_sortingLCID, source, value, startIndex, count, (int)options);
				}
				return SyntheticLastIndexOf(source, new string(value, 1), startIndex, count, GetNativeCompareFlags(options));
			}
			return LastIndexOfChar(m_pSortingTable, m_sortingLCID, source, value, startIndex, count, (int)options);
		}

		public unsafe virtual int LastIndexOf(string source, string value, int startIndex, int count, CompareOptions options)
		{
			if (source == null)
			{
				throw new ArgumentNullException("source");
			}
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			if (((uint)options & 0xFFFFFFE0u) != 0 && options != CompareOptions.Ordinal && options != CompareOptions.OrdinalIgnoreCase)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_InvalidFlag"), "options");
			}
			if (source.Length == 0 && (startIndex == -1 || startIndex == 0))
			{
				if (value.Length != 0)
				{
					return -1;
				}
				return 0;
			}
			if (startIndex < 0 || startIndex > source.Length)
			{
				throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("ArgumentOutOfRange_Index"));
			}
			if (startIndex == source.Length)
			{
				startIndex--;
				if (count > 0)
				{
					count--;
				}
				if (value.Length == 0 && count >= 0 && startIndex - count + 1 >= 0)
				{
					return startIndex;
				}
			}
			if (count < 0 || startIndex - count + 1 < 0)
			{
				throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("ArgumentOutOfRange_Count"));
			}
			if (options == CompareOptions.OrdinalIgnoreCase)
			{
				return TextInfo.LastIndexOfStringOrdinalIgnoreCase(source, value, startIndex, count);
			}
			if (IsSynthetic)
			{
				if (options == CompareOptions.Ordinal)
				{
					return LastIndexOfString(CultureInfo.InvariantCulture.CompareInfo.m_pSortingTable, m_sortingLCID, source, value, startIndex, count, (int)options);
				}
				return SyntheticLastIndexOf(source, value, startIndex, count, GetNativeCompareFlags(options));
			}
			return LastIndexOfString(m_pSortingTable, m_sortingLCID, source, value, startIndex, count, (int)options);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal unsafe static extern int LastIndexOfChar(void* pSortingTable, int sortingLCID, string source, char value, int startIndex, int count, int options);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal unsafe static extern int LastIndexOfString(void* pSortingTable, int sortingLCID, string source, string value, int startIndex, int count, int options);

		public unsafe virtual SortKey GetSortKey(string source, CompareOptions options)
		{
			if (IsSynthetic)
			{
				return new SortKey(m_sortingLCID, source, options);
			}
			return new SortKey(m_pSortingTable, m_sortingLCID, source, options);
		}

		public unsafe virtual SortKey GetSortKey(string source)
		{
			if (IsSynthetic)
			{
				return new SortKey(m_sortingLCID, source, CompareOptions.None);
			}
			return new SortKey(m_pSortingTable, m_sortingLCID, source, CompareOptions.None);
		}

		public override bool Equals(object value)
		{
			CompareInfo compareInfo = value as CompareInfo;
			if (compareInfo != null)
			{
				if (m_sortingLCID == compareInfo.m_sortingLCID)
				{
					return Name.Equals(compareInfo.Name);
				}
				return false;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return Name.GetHashCode();
		}

		internal unsafe int GetHashCodeOfString(string source, CompareOptions options)
		{
			if (source == null)
			{
				throw new ArgumentNullException("source");
			}
			if (((uint)options & 0xFFFFFFE0u) != 0)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_InvalidFlag"), "options");
			}
			if (source.Length == 0)
			{
				return 0;
			}
			if (IsSynthetic)
			{
				return CultureInfo.InvariantCulture.CompareInfo.GetHashCodeOfString(source, options);
			}
			return nativeGetGlobalizedHashCode(m_pSortingTable, source, (int)options, m_sortingLCID);
		}

		public override string ToString()
		{
			return "CompareInfo - " + culture;
		}

		private unsafe static void* InitializeCompareInfo(void* pNativeGlobalizationAssembly, int sortingLCID)
		{
			void* ptr = null;
			bool tookLock = false;
			RuntimeHelpers.PrepareConstrainedRegions();
			try
			{
				Monitor.ReliableEnter(typeof(CultureTableRecord), ref tookLock);
				return InitializeNativeCompareInfo(pNativeGlobalizationAssembly, sortingLCID);
			}
			finally
			{
				if (tookLock)
				{
					Monitor.Exit(typeof(CultureTableRecord));
				}
			}
		}

		void IDeserializationCallback.OnDeserialization(object sender)
		{
			OnDeserialized();
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private unsafe static extern void* InitializeNativeCompareInfo(void* pNativeGlobalizationAssembly, int sortingLCID);
	}
}
