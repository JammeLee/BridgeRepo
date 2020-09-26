using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System.Globalization
{
	public sealed class CharUnicodeInfo
	{
		[StructLayout(LayoutKind.Explicit)]
		internal struct UnicodeDataHeader
		{
			[FieldOffset(0)]
			internal char TableName;

			[FieldOffset(32)]
			internal ushort version;

			[FieldOffset(40)]
			internal uint OffsetToCategoriesIndex;

			[FieldOffset(44)]
			internal uint OffsetToCategoriesValue;

			[FieldOffset(48)]
			internal uint OffsetToNumbericIndex;

			[FieldOffset(52)]
			internal uint OffsetToDigitValue;

			[FieldOffset(56)]
			internal uint OffsetToNumbericValue;
		}

		[StructLayout(LayoutKind.Sequential, Pack = 2)]
		internal struct DigitValues
		{
			internal sbyte decimalDigit;

			internal sbyte digit;
		}

		internal const char HIGH_SURROGATE_START = '\ud800';

		internal const char HIGH_SURROGATE_END = '\udbff';

		internal const char LOW_SURROGATE_START = '\udc00';

		internal const char LOW_SURROGATE_END = '\udfff';

		internal const int UNICODE_CATEGORY_OFFSET = 0;

		internal const int BIDI_CATEGORY_OFFSET = 1;

		internal const string UNICODE_INFO_FILE_NAME = "charinfo.nlp";

		internal const int UNICODE_PLANE01_START = 65536;

		private unsafe static byte* m_pDataTable;

		private unsafe static ushort* m_pCategoryLevel1Index;

		private unsafe static byte* m_pCategoriesValue;

		private unsafe static ushort* m_pNumericLevel1Index;

		private unsafe static byte* m_pNumericValues;

		private unsafe static DigitValues* m_pDigitValues;

		unsafe static CharUnicodeInfo()
		{
			m_pDataTable = GlobalizationAssembly.GetGlobalizationResourceBytePtr(typeof(CharUnicodeInfo).Assembly, "charinfo.nlp");
			UnicodeDataHeader* pDataTable = (UnicodeDataHeader*)m_pDataTable;
			m_pCategoryLevel1Index = (ushort*)(m_pDataTable + (int)pDataTable->OffsetToCategoriesIndex);
			m_pCategoriesValue = m_pDataTable + (int)pDataTable->OffsetToCategoriesValue;
			m_pNumericLevel1Index = (ushort*)(m_pDataTable + (int)pDataTable->OffsetToNumbericIndex);
			m_pNumericValues = m_pDataTable + (int)pDataTable->OffsetToNumbericValue;
			m_pDigitValues = (DigitValues*)(m_pDataTable + (int)pDataTable->OffsetToDigitValue);
			nativeInitTable(m_pDataTable);
		}

		private CharUnicodeInfo()
		{
		}

		internal static int InternalConvertToUtf32(string s, int index)
		{
			if (index < s.Length - 1)
			{
				int num = s[index] - 55296;
				if (num >= 0 && num <= 1023)
				{
					int num2 = s[index + 1] - 56320;
					if (num2 >= 0 && num2 <= 1023)
					{
						return num * 1024 + num2 + 65536;
					}
				}
			}
			return s[index];
		}

		internal static int InternalConvertToUtf32(string s, int index, out int charLength)
		{
			charLength = 1;
			if (index < s.Length - 1)
			{
				int num = s[index] - 55296;
				if (num >= 0 && num <= 1023)
				{
					int num2 = s[index + 1] - 56320;
					if (num2 >= 0 && num2 <= 1023)
					{
						charLength++;
						return num * 1024 + num2 + 65536;
					}
				}
			}
			return s[index];
		}

		internal static bool IsWhiteSpace(string s, int index)
		{
			switch (GetUnicodeCategory(s, index))
			{
			case UnicodeCategory.SpaceSeparator:
			case UnicodeCategory.LineSeparator:
			case UnicodeCategory.ParagraphSeparator:
				return true;
			default:
				return false;
			}
		}

		internal static bool IsWhiteSpace(char c)
		{
			switch (GetUnicodeCategory(c))
			{
			case UnicodeCategory.SpaceSeparator:
			case UnicodeCategory.LineSeparator:
			case UnicodeCategory.ParagraphSeparator:
				return true;
			default:
				return false;
			}
		}

		internal unsafe static double InternalGetNumericValue(int ch)
		{
			ushort num = m_pNumericLevel1Index[ch >> 8];
			num = m_pNumericLevel1Index[num + ((ch >> 4) & 0xF)];
			byte* ptr = (byte*)(m_pNumericLevel1Index + (int)num);
			return *(double*)(m_pNumericValues + (nint)(int)ptr[ch & 0xF] * (nint)8);
		}

		internal unsafe static DigitValues* InternalGetDigitValues(int ch)
		{
			ushort num = m_pNumericLevel1Index[ch >> 8];
			num = m_pNumericLevel1Index[num + ((ch >> 4) & 0xF)];
			byte* ptr = (byte*)(m_pNumericLevel1Index + (int)num);
			return m_pDigitValues + (int)ptr[ch & 0xF];
		}

		internal unsafe static sbyte InternalGetDecimalDigitValue(int ch)
		{
			return InternalGetDigitValues(ch)->decimalDigit;
		}

		internal unsafe static sbyte InternalGetDigitValue(int ch)
		{
			return InternalGetDigitValues(ch)->digit;
		}

		public static double GetNumericValue(char ch)
		{
			return InternalGetNumericValue(ch);
		}

		public static double GetNumericValue(string s, int index)
		{
			if (s == null)
			{
				throw new ArgumentNullException("s");
			}
			if (index < 0 || index >= s.Length)
			{
				throw new ArgumentOutOfRangeException("index", Environment.GetResourceString("ArgumentOutOfRange_Index"));
			}
			return InternalGetNumericValue(InternalConvertToUtf32(s, index));
		}

		public static int GetDecimalDigitValue(char ch)
		{
			return InternalGetDecimalDigitValue(ch);
		}

		public static int GetDecimalDigitValue(string s, int index)
		{
			if (s == null)
			{
				throw new ArgumentNullException("s");
			}
			if (index < 0 || index >= s.Length)
			{
				throw new ArgumentOutOfRangeException("index", Environment.GetResourceString("ArgumentOutOfRange_Index"));
			}
			return InternalGetDecimalDigitValue(InternalConvertToUtf32(s, index));
		}

		public static int GetDigitValue(char ch)
		{
			return InternalGetDigitValue(ch);
		}

		public static int GetDigitValue(string s, int index)
		{
			if (s == null)
			{
				throw new ArgumentNullException("s");
			}
			if (index < 0 || index >= s.Length)
			{
				throw new ArgumentOutOfRangeException("index", Environment.GetResourceString("ArgumentOutOfRange_Index"));
			}
			return InternalGetDigitValue(InternalConvertToUtf32(s, index));
		}

		public static UnicodeCategory GetUnicodeCategory(char ch)
		{
			return InternalGetUnicodeCategory(ch);
		}

		public static UnicodeCategory GetUnicodeCategory(string s, int index)
		{
			if (s == null)
			{
				throw new ArgumentNullException("s");
			}
			if ((uint)index >= (uint)s.Length)
			{
				throw new ArgumentOutOfRangeException("index");
			}
			return InternalGetUnicodeCategory(s, index);
		}

		internal static UnicodeCategory InternalGetUnicodeCategory(int ch)
		{
			return (UnicodeCategory)InternalGetCategoryValue(ch, 0);
		}

		internal unsafe static byte InternalGetCategoryValue(int ch, int offset)
		{
			ushort num = m_pCategoryLevel1Index[ch >> 8];
			num = m_pCategoryLevel1Index[num + ((ch >> 4) & 0xF)];
			byte* ptr = (byte*)(m_pCategoryLevel1Index + (int)num);
			byte b = ptr[ch & 0xF];
			return m_pCategoriesValue[b * 2 + offset];
		}

		internal static BidiCategory GetBidiCategory(string s, int index)
		{
			if (s == null)
			{
				throw new ArgumentNullException("s");
			}
			if ((uint)index >= (uint)s.Length)
			{
				throw new ArgumentOutOfRangeException("index");
			}
			return (BidiCategory)InternalGetCategoryValue(InternalConvertToUtf32(s, index), 1);
		}

		internal static UnicodeCategory InternalGetUnicodeCategory(string value, int index)
		{
			return InternalGetUnicodeCategory(InternalConvertToUtf32(value, index));
		}

		internal static UnicodeCategory InternalGetUnicodeCategory(string str, int index, out int charLength)
		{
			return InternalGetUnicodeCategory(InternalConvertToUtf32(str, index, out charLength));
		}

		internal static bool IsCombiningCategory(UnicodeCategory uc)
		{
			if (uc != UnicodeCategory.NonSpacingMark && uc != UnicodeCategory.SpacingCombiningMark)
			{
				return uc == UnicodeCategory.EnclosingMark;
			}
			return true;
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private unsafe static extern void nativeInitTable(byte* bytePtr);
	}
}
