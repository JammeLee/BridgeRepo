using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;

namespace System.Globalization
{
	[Serializable]
	[ComVisible(true)]
	public class TextInfo : ICloneable, IDeserializationCallback
	{
		[StructLayout(LayoutKind.Explicit)]
		internal struct TextInfoDataHeader
		{
			[FieldOffset(0)]
			internal char TableName;

			[FieldOffset(32)]
			internal ushort version;

			[FieldOffset(40)]
			internal uint OffsetToUpperCasingTable;

			[FieldOffset(44)]
			internal uint OffsetToLowerCasingTable;

			[FieldOffset(48)]
			internal uint OffsetToTitleCaseTable;

			[FieldOffset(52)]
			internal uint PlaneOffset;

			[FieldOffset(180)]
			internal ushort exceptionCount;

			[FieldOffset(182)]
			internal ushort exceptionLangId;
		}

		[StructLayout(LayoutKind.Sequential, Pack = 2)]
		internal struct ExceptionTableItem
		{
			internal ushort langID;

			internal ushort exceptIndex;
		}

		private const string CASING_FILE_NAME = "l_intl.nlp";

		private const string CASING_EXCEPTIONS_FILE_NAME = "l_except.nlp";

		private const int wordSeparatorMask = 536672256;

		internal const int TurkishAnsiCodepage = 1254;

		[OptionalField(VersionAdded = 2)]
		private string m_listSeparator;

		[OptionalField(VersionAdded = 2)]
		private bool m_isReadOnly;

		[NonSerialized]
		private int m_textInfoID;

		[NonSerialized]
		private string m_name;

		[NonSerialized]
		private CultureTableRecord m_cultureTableRecord;

		[NonSerialized]
		private TextInfo m_casingTextInfo;

		[NonSerialized]
		private unsafe void* m_pNativeTextInfo;

		private unsafe static void* m_pInvariantNativeTextInfo;

		private unsafe static void* m_pDefaultCasingTable;

		private unsafe static byte* m_pDataTable;

		private static int m_exceptionCount;

		private unsafe static ExceptionTableItem* m_exceptionTable;

		private unsafe static byte* m_pExceptionFile;

		private static long[] m_exceptionNativeTextInfo;

		private static object s_InternalSyncObject;

		[OptionalField(VersionAdded = 2)]
		private string customCultureName;

		internal int m_nDataItem;

		internal bool m_useUserOverride;

		internal int m_win32LangID;

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

		internal unsafe static void* InvariantNativeTextInfo
		{
			get
			{
				if (m_pInvariantNativeTextInfo == null)
				{
					lock (InternalSyncObject)
					{
						if (m_pInvariantNativeTextInfo == null)
						{
							m_pInvariantNativeTextInfo = GetNativeTextInfo(127);
						}
					}
				}
				return m_pInvariantNativeTextInfo;
			}
		}

		public virtual int ANSICodePage => m_cultureTableRecord.IDEFAULTANSICODEPAGE;

		public virtual int OEMCodePage => m_cultureTableRecord.IDEFAULTOEMCODEPAGE;

		public virtual int MacCodePage => m_cultureTableRecord.IDEFAULTMACCODEPAGE;

		public virtual int EBCDICCodePage => m_cultureTableRecord.IDEFAULTEBCDICCODEPAGE;

		[ComVisible(false)]
		public int LCID => m_textInfoID;

		[ComVisible(false)]
		public string CultureName
		{
			get
			{
				if (m_name == null)
				{
					m_name = CultureInfo.GetCultureInfo(m_textInfoID).Name;
				}
				return m_name;
			}
		}

		[ComVisible(false)]
		public bool IsReadOnly => m_isReadOnly;

		public virtual string ListSeparator
		{
			get
			{
				if (m_listSeparator == null)
				{
					m_listSeparator = m_cultureTableRecord.SLIST;
				}
				return m_listSeparator;
			}
			[ComVisible(false)]
			set
			{
				VerifyWritable();
				if (value == null)
				{
					throw new ArgumentNullException("value", Environment.GetResourceString("ArgumentNull_String"));
				}
				m_listSeparator = value;
			}
		}

		internal TextInfo CasingTextInfo
		{
			get
			{
				if (m_casingTextInfo == null)
				{
					if (ANSICodePage == 1254)
					{
						m_casingTextInfo = CultureInfo.GetCultureInfo("tr-TR").TextInfo;
					}
					else
					{
						m_casingTextInfo = CultureInfo.GetCultureInfo("en-US").TextInfo;
					}
				}
				return m_casingTextInfo;
			}
		}

		[ComVisible(false)]
		public bool IsRightToLeft => (m_cultureTableRecord.ILINEORIENTATIONS & 0x8000) != 0;

		unsafe static TextInfo()
		{
			byte* globalizationResourceBytePtr = GlobalizationAssembly.GetGlobalizationResourceBytePtr(typeof(TextInfo).Assembly, "l_intl.nlp");
			Thread.MemoryBarrier();
			m_pDataTable = globalizationResourceBytePtr;
			TextInfoDataHeader* pDataTable = (TextInfoDataHeader*)m_pDataTable;
			m_exceptionCount = pDataTable->exceptionCount;
			m_exceptionTable = (ExceptionTableItem*)(&pDataTable->exceptionLangId);
			m_exceptionNativeTextInfo = new long[m_exceptionCount];
			m_pDefaultCasingTable = AllocateDefaultCasingTable(m_pDataTable);
		}

		internal unsafe TextInfo(CultureTableRecord table)
		{
			m_cultureTableRecord = table;
			m_textInfoID = m_cultureTableRecord.ITEXTINFO;
			if (table.IsSynthetic)
			{
				m_pNativeTextInfo = InvariantNativeTextInfo;
			}
			else
			{
				m_pNativeTextInfo = GetNativeTextInfo(m_textInfoID);
			}
		}

		[OnDeserializing]
		private void OnDeserializing(StreamingContext ctx)
		{
			m_cultureTableRecord = null;
			m_win32LangID = 0;
		}

		private unsafe void OnDeserialized()
		{
			if (m_cultureTableRecord == null)
			{
				if (m_win32LangID == 0)
				{
					m_win32LangID = CultureTableRecord.IdFromEverettDataItem(m_nDataItem);
				}
				if (customCultureName != null)
				{
					m_cultureTableRecord = CultureTableRecord.GetCultureTableRecord(customCultureName, m_useUserOverride);
				}
				else
				{
					m_cultureTableRecord = CultureTableRecord.GetCultureTableRecord(m_win32LangID, m_useUserOverride);
				}
				m_textInfoID = m_cultureTableRecord.ITEXTINFO;
				if (m_cultureTableRecord.IsSynthetic)
				{
					m_pNativeTextInfo = InvariantNativeTextInfo;
				}
				else
				{
					m_pNativeTextInfo = GetNativeTextInfo(m_textInfoID);
				}
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
			m_nDataItem = m_cultureTableRecord.EverettDataItem();
			m_useUserOverride = m_cultureTableRecord.UseUserOverride;
			if (CultureTableRecord.IsCustomCultureId(m_cultureTableRecord.CultureID))
			{
				customCultureName = m_cultureTableRecord.SNAME;
				m_win32LangID = m_textInfoID;
			}
			else
			{
				customCultureName = null;
				m_win32LangID = m_cultureTableRecord.CultureID;
			}
		}

		internal unsafe static void* GetNativeTextInfo(int cultureID)
		{
			if (cultureID == 127 && Environment.OSVersion.Platform == PlatformID.Win32NT)
			{
				void* ptr = nativeGetInvariantTextInfo();
				if (ptr != null)
				{
					return ptr;
				}
				throw new TypeInitializationException(typeof(TextInfo).ToString(), null);
			}
			void* result = m_pDefaultCasingTable;
			for (int i = 0; i < m_exceptionCount; i++)
			{
				if (m_exceptionTable[i].langID != cultureID)
				{
					continue;
				}
				if (m_exceptionNativeTextInfo[i] == 0)
				{
					lock (InternalSyncObject)
					{
						if (m_pExceptionFile == null)
						{
							m_pExceptionFile = GlobalizationAssembly.GetGlobalizationResourceBytePtr(typeof(TextInfo).Assembly, "l_except.nlp");
						}
						long num = (long)InternalAllocateCasingTable(m_pExceptionFile, m_exceptionTable[i].exceptIndex);
						Thread.MemoryBarrier();
						m_exceptionNativeTextInfo[i] = num;
					}
				}
				result = (void*)m_exceptionNativeTextInfo[i];
				break;
			}
			return result;
		}

		internal unsafe static int CompareOrdinalIgnoreCase(string str1, string str2)
		{
			return nativeCompareOrdinalIgnoreCase(InvariantNativeTextInfo, str1, str2);
		}

		internal unsafe static int CompareOrdinalIgnoreCaseEx(string strA, int indexA, string strB, int indexB, int length)
		{
			return nativeCompareOrdinalIgnoreCaseEx(InvariantNativeTextInfo, strA, indexA, strB, indexB, length);
		}

		internal unsafe static int GetHashCodeOrdinalIgnoreCase(string s)
		{
			return nativeGetHashCodeOrdinalIgnoreCase(InvariantNativeTextInfo, s);
		}

		internal unsafe static int IndexOfStringOrdinalIgnoreCase(string source, string value, int startIndex, int count)
		{
			if (source == null)
			{
				throw new ArgumentNullException("source");
			}
			return nativeIndexOfStringOrdinalIgnoreCase(InvariantNativeTextInfo, source, value, startIndex, count);
		}

		internal unsafe static int LastIndexOfStringOrdinalIgnoreCase(string source, string value, int startIndex, int count)
		{
			if (source == null)
			{
				throw new ArgumentNullException("source");
			}
			return nativeLastIndexOfStringOrdinalIgnoreCase(InvariantNativeTextInfo, source, value, startIndex, count);
		}

		[ComVisible(false)]
		public virtual object Clone()
		{
			object obj = MemberwiseClone();
			((TextInfo)obj).SetReadOnlyState(readOnly: false);
			return obj;
		}

		[ComVisible(false)]
		public static TextInfo ReadOnly(TextInfo textInfo)
		{
			if (textInfo == null)
			{
				throw new ArgumentNullException("textInfo");
			}
			if (textInfo.IsReadOnly)
			{
				return textInfo;
			}
			TextInfo textInfo2 = (TextInfo)textInfo.MemberwiseClone();
			textInfo2.SetReadOnlyState(readOnly: true);
			return textInfo2;
		}

		private void VerifyWritable()
		{
			if (m_isReadOnly)
			{
				throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_ReadOnly"));
			}
		}

		internal void SetReadOnlyState(bool readOnly)
		{
			m_isReadOnly = readOnly;
		}

		public unsafe virtual char ToLower(char c)
		{
			if (m_cultureTableRecord.IsSynthetic)
			{
				return CasingTextInfo.ToLower(c);
			}
			return nativeChangeCaseChar(m_textInfoID, m_pNativeTextInfo, c, isToUpper: false);
		}

		public unsafe virtual string ToLower(string str)
		{
			if (str == null)
			{
				throw new ArgumentNullException("str");
			}
			if (m_cultureTableRecord.IsSynthetic)
			{
				return CasingTextInfo.ToLower(str);
			}
			return nativeChangeCaseString(m_textInfoID, m_pNativeTextInfo, str, isToUpper: false);
		}

		public unsafe virtual char ToUpper(char c)
		{
			if (m_cultureTableRecord.IsSynthetic)
			{
				return CasingTextInfo.ToUpper(c);
			}
			return nativeChangeCaseChar(m_textInfoID, m_pNativeTextInfo, c, isToUpper: true);
		}

		public unsafe virtual string ToUpper(string str)
		{
			if (str == null)
			{
				throw new ArgumentNullException("str");
			}
			if (m_cultureTableRecord.IsSynthetic)
			{
				return CasingTextInfo.ToUpper(str);
			}
			return nativeChangeCaseString(m_textInfoID, m_pNativeTextInfo, str, isToUpper: true);
		}

		public override bool Equals(object obj)
		{
			TextInfo textInfo = obj as TextInfo;
			if (textInfo != null)
			{
				return CultureName.Equals(textInfo.CultureName);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return CultureName.GetHashCode();
		}

		public override string ToString()
		{
			return "TextInfo - " + m_textInfoID;
		}

		private bool IsWordSeparator(UnicodeCategory category)
		{
			return (0x1FFCF800 & (1 << (int)category)) != 0;
		}

		public unsafe string ToTitleCase(string str)
		{
			if (str == null)
			{
				throw new ArgumentNullException("str");
			}
			if (m_cultureTableRecord.IsSynthetic)
			{
				if (ANSICodePage == 1254)
				{
					return CultureInfo.GetCultureInfo("tr-TR").TextInfo.ToTitleCase(str);
				}
				return CultureInfo.GetCultureInfo("en-US").TextInfo.ToTitleCase(str);
			}
			int length = str.Length;
			if (length == 0)
			{
				return str;
			}
			StringBuilder stringBuilder = new StringBuilder();
			string text = null;
			for (int i = 0; i < length; i++)
			{
				UnicodeCategory unicodeCategory = CharUnicodeInfo.InternalGetUnicodeCategory(str, i, out var charLength);
				if (char.CheckLetter(unicodeCategory))
				{
					if (charLength == 1)
					{
						stringBuilder.Append(nativeGetTitleCaseChar(m_pNativeTextInfo, str[i]));
					}
					else
					{
						ChangeCaseSurrogate(str[i], str[i + 1], out var resultHighSurrogate, out var resultLowSurrogate, isToUpper: true);
						stringBuilder.Append(resultHighSurrogate);
						stringBuilder.Append(resultLowSurrogate);
					}
					i += charLength;
					int num = i;
					bool flag = unicodeCategory == UnicodeCategory.LowercaseLetter;
					while (i < length)
					{
						unicodeCategory = CharUnicodeInfo.InternalGetUnicodeCategory(str, i, out charLength);
						if (IsLetterCategory(unicodeCategory))
						{
							if (unicodeCategory == UnicodeCategory.LowercaseLetter)
							{
								flag = true;
							}
							i += charLength;
						}
						else if (str[i] == '\'')
						{
							i++;
							if (flag)
							{
								if (text == null)
								{
									text = ToLower(str);
								}
								stringBuilder.Append(text, num, i - num);
							}
							else
							{
								stringBuilder.Append(str, num, i - num);
							}
							num = i;
							flag = true;
						}
						else
						{
							if (IsWordSeparator(unicodeCategory))
							{
								break;
							}
							i += charLength;
						}
					}
					int num2 = i - num;
					if (num2 > 0)
					{
						if (flag)
						{
							if (text == null)
							{
								text = ToLower(str);
							}
							stringBuilder.Append(text, num, num2);
						}
						else
						{
							stringBuilder.Append(str, num, num2);
						}
					}
					if (i < length)
					{
						if (charLength == 1)
						{
							stringBuilder.Append(str[i]);
							continue;
						}
						stringBuilder.Append(str[i++]);
						stringBuilder.Append(str[i]);
					}
				}
				else if (charLength == 1)
				{
					stringBuilder.Append(str[i]);
				}
				else
				{
					stringBuilder.Append(str[i++]);
					stringBuilder.Append(str[i]);
				}
			}
			return stringBuilder.ToString();
		}

		private bool IsLetterCategory(UnicodeCategory uc)
		{
			if (uc != 0 && uc != UnicodeCategory.LowercaseLetter && uc != UnicodeCategory.TitlecaseLetter && uc != UnicodeCategory.ModifierLetter)
			{
				return uc == UnicodeCategory.OtherLetter;
			}
			return true;
		}

		void IDeserializationCallback.OnDeserialization(object sender)
		{
			OnDeserialized();
		}

		internal unsafe int GetCaseInsensitiveHashCode(string str)
		{
			if (str == null)
			{
				throw new ArgumentNullException("str");
			}
			if (m_pNativeTextInfo == null)
			{
				OnDeserialized();
			}
			int textInfoID = m_textInfoID;
			if (textInfoID == 1055 || textInfoID == 1068)
			{
				str = nativeChangeCaseString(m_textInfoID, m_pNativeTextInfo, str, isToUpper: true);
			}
			return nativeGetCaseInsHash(str, m_pNativeTextInfo);
		}

		internal unsafe void ChangeCaseSurrogate(char highSurrogate, char lowSurrogate, out char resultHighSurrogate, out char resultLowSurrogate, bool isToUpper)
		{
			fixed (char* resultHighSurrogate2 = &resultHighSurrogate)
			{
				fixed (char* resultLowSurrogate2 = &resultLowSurrogate)
				{
					nativeChangeCaseSurrogate(m_pNativeTextInfo, highSurrogate, lowSurrogate, resultHighSurrogate2, resultLowSurrogate2, isToUpper);
				}
			}
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private unsafe static extern void* AllocateDefaultCasingTable(byte* ptr);

		[MethodImpl(MethodImplOptions.InternalCall)]
		private unsafe static extern void* nativeGetInvariantTextInfo();

		[MethodImpl(MethodImplOptions.InternalCall)]
		private unsafe static extern void* InternalAllocateCasingTable(byte* ptr, int exceptionIndex);

		[MethodImpl(MethodImplOptions.InternalCall)]
		private unsafe static extern int nativeGetCaseInsHash(string str, void* pNativeTextInfo);

		[MethodImpl(MethodImplOptions.InternalCall)]
		private unsafe static extern char nativeGetTitleCaseChar(void* pNativeTextInfo, char ch);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal unsafe static extern char nativeChangeCaseChar(int win32LangID, void* pNativeTextInfo, char ch, bool isToUpper);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal unsafe static extern string nativeChangeCaseString(int win32LangID, void* pNativeTextInfo, string str, bool isToUpper);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal unsafe static extern void nativeChangeCaseSurrogate(void* pNativeTextInfo, char highSurrogate, char lowSurrogate, char* resultHighSurrogate, char* resultLowSurrogate, bool isToUpper);

		[MethodImpl(MethodImplOptions.InternalCall)]
		private unsafe static extern int nativeCompareOrdinalIgnoreCase(void* pNativeTextInfo, string str1, string str2);

		[MethodImpl(MethodImplOptions.InternalCall)]
		private unsafe static extern int nativeCompareOrdinalIgnoreCaseEx(void* pNativeTextInfo, string strA, int indexA, string strB, int indexB, int length);

		[MethodImpl(MethodImplOptions.InternalCall)]
		private unsafe static extern int nativeGetHashCodeOrdinalIgnoreCase(void* pNativeTextInfo, string s);

		[MethodImpl(MethodImplOptions.InternalCall)]
		private unsafe static extern int nativeIndexOfStringOrdinalIgnoreCase(void* pNativeTextInfo, string str, string value, int startIndex, int count);

		[MethodImpl(MethodImplOptions.InternalCall)]
		private unsafe static extern int nativeLastIndexOfStringOrdinalIgnoreCase(void* pNativeTextInfo, string str, string value, int startIndex, int count);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal unsafe static extern int nativeIndexOfCharOrdinalIgnoreCase(void* pNativeTextInfo, string str, char value, int startIndex, int count);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal unsafe static extern int nativeLastIndexOfCharOrdinalIgnoreCase(void* pNativeTextInfo, string str, char value, int startIndex, int count);
	}
}
