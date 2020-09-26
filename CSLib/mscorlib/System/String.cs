using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Microsoft.Win32;

namespace System
{
	[Serializable]
	[ComVisible(true)]
	public sealed class String : IComparable, ICloneable, IConvertible, IComparable<string>, IEnumerable<char>, IEnumerable, IEquatable<string>
	{
		private const int TrimHead = 0;

		private const int TrimTail = 1;

		private const int TrimBoth = 2;

		private const int charPtrAlignConst = 1;

		private const int alignConst = 3;

		[NonSerialized]
		private int m_arrayLength;

		[NonSerialized]
		private int m_stringLength;

		[NonSerialized]
		private char m_firstChar;

		public static readonly string Empty = "";

		internal static readonly char[] WhitespaceChars = new char[25]
		{
			'\t',
			'\n',
			'\v',
			'\f',
			'\r',
			' ',
			'\u0085',
			'\u00a0',
			'\u1680',
			'\u2000',
			'\u2001',
			'\u2002',
			'\u2003',
			'\u2004',
			'\u2005',
			'\u2006',
			'\u2007',
			'\u2008',
			'\u2009',
			'\u200a',
			'\u200b',
			'\u2028',
			'\u2029',
			'\u3000',
			'\ufeff'
		};

		internal char FirstChar => m_firstChar;

		[IndexerName("Chars")]
		public char this[int index]
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			get;
		}

		public int Length
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			get;
		}

		internal int ArrayLength => m_arrayLength;

		internal int Capacity => m_arrayLength - 1;

		public static string Join(string separator, string[] value)
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			return Join(separator, value, 0, value.Length);
		}

		public unsafe static string Join(string separator, string[] value, int startIndex, int count)
		{
			if (separator == null)
			{
				separator = Empty;
			}
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			if (startIndex < 0)
			{
				throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("ArgumentOutOfRange_StartIndex"));
			}
			if (count < 0)
			{
				throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("ArgumentOutOfRange_NegativeCount"));
			}
			if (startIndex > value.Length - count)
			{
				throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("ArgumentOutOfRange_IndexCountBuffer"));
			}
			if (count == 0)
			{
				return Empty;
			}
			int num = 0;
			int num2 = startIndex + count - 1;
			for (int i = startIndex; i <= num2; i++)
			{
				if (value[i] != null)
				{
					num += value[i].Length;
				}
			}
			num += (count - 1) * separator.Length;
			if (num < 0 || num + 1 < 0)
			{
				throw new OutOfMemoryException();
			}
			if (num == 0)
			{
				return Empty;
			}
			string text = FastAllocateString(num);
			fixed (char* buffer = &text.m_firstChar)
			{
				UnSafeCharBuffer unSafeCharBuffer = new UnSafeCharBuffer(buffer, num);
				unSafeCharBuffer.AppendString(value[startIndex]);
				for (int j = startIndex + 1; j <= num2; j++)
				{
					unSafeCharBuffer.AppendString(separator);
					unSafeCharBuffer.AppendString(value[j]);
				}
			}
			return text;
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern int nativeCompareOrdinal(string strA, string strB, bool bIgnoreCase);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern int nativeCompareOrdinalEx(string strA, int indexA, string strB, int indexB, int count);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal unsafe static extern int nativeCompareOrdinalWC(string strA, char* strBChars, bool bIgnoreCase, out bool success);

		internal unsafe static string SmallCharToUpper(string strIn)
		{
			int length = strIn.Length;
			string text = FastAllocateString(length);
			fixed (char* ptr = &strIn.m_firstChar)
			{
				fixed (char* ptr2 = &text.m_firstChar)
				{
					int num = -33;
					for (int i = 0; i < length; i++)
					{
						char c = ptr[i];
						if (c >= 'a' && c <= 'z')
						{
							c = (char)(c & num);
						}
						ptr2[i] = c;
					}
				}
			}
			return text;
		}

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		private unsafe static bool EqualsHelper(string strA, string strB)
		{
			int num = strA.Length;
			if (num != strB.Length)
			{
				return false;
			}
			fixed (char* ptr = strA)
			{
				fixed (char* ptr3 = strB)
				{
					char* ptr2 = ptr;
					char* ptr4 = ptr3;
					while (num >= 10 && *(int*)ptr2 == *(int*)ptr4 && *(int*)(ptr2 + 2) == *(int*)(ptr4 + 2) && *(int*)(ptr2 + 4) == *(int*)(ptr4 + 4) && *(int*)(ptr2 + 6) == *(int*)(ptr4 + 6) && *(int*)(ptr2 + 8) == *(int*)(ptr4 + 8))
					{
						ptr2 += 10;
						ptr4 += 10;
						num -= 10;
					}
					while (num > 0 && *(int*)ptr2 == *(int*)ptr4)
					{
						ptr2 += 2;
						ptr4 += 2;
						num -= 2;
					}
					return num <= 0;
				}
			}
		}

		private unsafe static int CompareOrdinalHelper(string strA, string strB)
		{
			int num = Math.Min(strA.Length, strB.Length);
			int num2 = -1;
			fixed (char* ptr = strA)
			{
				fixed (char* ptr3 = strB)
				{
					char* ptr2 = ptr;
					char* ptr4 = ptr3;
					while (num >= 10)
					{
						if (*(int*)ptr2 != *(int*)ptr4)
						{
							num2 = 0;
							break;
						}
						if (*(int*)(ptr2 + 2) != *(int*)(ptr4 + 2))
						{
							num2 = 2;
							break;
						}
						if (*(int*)(ptr2 + 4) != *(int*)(ptr4 + 4))
						{
							num2 = 4;
							break;
						}
						if (*(int*)(ptr2 + 6) != *(int*)(ptr4 + 6))
						{
							num2 = 6;
							break;
						}
						if (*(int*)(ptr2 + 8) != *(int*)(ptr4 + 8))
						{
							num2 = 8;
							break;
						}
						ptr2 += 10;
						ptr4 += 10;
						num -= 10;
					}
					if (num2 != -1)
					{
						ptr2 += num2;
						ptr4 += num2;
						int result;
						if ((result = *ptr2 - *ptr4) != 0)
						{
							return result;
						}
						return ptr2[1] - ptr4[1];
					}
					while (num > 0 && *(int*)ptr2 == *(int*)ptr4)
					{
						ptr2 += 2;
						ptr4 += 2;
						num -= 2;
					}
					if (num > 0)
					{
						int result2;
						if ((result2 = *ptr2 - *ptr4) != 0)
						{
							return result2;
						}
						return ptr2[1] - ptr4[1];
					}
					return strA.Length - strB.Length;
				}
			}
		}

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		public override bool Equals(object obj)
		{
			string text = obj as string;
			if (text == null && this != null)
			{
				return false;
			}
			return EqualsHelper(this, text);
		}

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		public bool Equals(string value)
		{
			if (value == null && this != null)
			{
				return false;
			}
			return EqualsHelper(this, value);
		}

		public bool Equals(string value, StringComparison comparisonType)
		{
			if (comparisonType < StringComparison.CurrentCulture || comparisonType > StringComparison.OrdinalIgnoreCase)
			{
				throw new ArgumentException(Environment.GetResourceString("NotSupported_StringComparison"), "comparisonType");
			}
			if ((object)this == value)
			{
				return true;
			}
			if (value == null)
			{
				return false;
			}
			switch (comparisonType)
			{
			case StringComparison.CurrentCulture:
				return CultureInfo.CurrentCulture.CompareInfo.Compare(this, value, CompareOptions.None) == 0;
			case StringComparison.CurrentCultureIgnoreCase:
				return CultureInfo.CurrentCulture.CompareInfo.Compare(this, value, CompareOptions.IgnoreCase) == 0;
			case StringComparison.InvariantCulture:
				return CultureInfo.InvariantCulture.CompareInfo.Compare(this, value, CompareOptions.None) == 0;
			case StringComparison.InvariantCultureIgnoreCase:
				return CultureInfo.InvariantCulture.CompareInfo.Compare(this, value, CompareOptions.IgnoreCase) == 0;
			case StringComparison.Ordinal:
				return Equals(value);
			case StringComparison.OrdinalIgnoreCase:
				if (Length != value.Length)
				{
					return false;
				}
				if (IsAscii() && value.IsAscii())
				{
					return nativeCompareOrdinal(this, value, bIgnoreCase: true) == 0;
				}
				return TextInfo.CompareOrdinalIgnoreCase(this, value) == 0;
			default:
				throw new ArgumentException(Environment.GetResourceString("NotSupported_StringComparison"), "comparisonType");
			}
		}

		public static bool Equals(string a, string b)
		{
			if ((object)a == b)
			{
				return true;
			}
			if (a == null || b == null)
			{
				return false;
			}
			return EqualsHelper(a, b);
		}

		public static bool Equals(string a, string b, StringComparison comparisonType)
		{
			if (comparisonType < StringComparison.CurrentCulture || comparisonType > StringComparison.OrdinalIgnoreCase)
			{
				throw new ArgumentException(Environment.GetResourceString("NotSupported_StringComparison"), "comparisonType");
			}
			if ((object)a == b)
			{
				return true;
			}
			if (a == null || b == null)
			{
				return false;
			}
			switch (comparisonType)
			{
			case StringComparison.CurrentCulture:
				return CultureInfo.CurrentCulture.CompareInfo.Compare(a, b, CompareOptions.None) == 0;
			case StringComparison.CurrentCultureIgnoreCase:
				return CultureInfo.CurrentCulture.CompareInfo.Compare(a, b, CompareOptions.IgnoreCase) == 0;
			case StringComparison.InvariantCulture:
				return CultureInfo.InvariantCulture.CompareInfo.Compare(a, b, CompareOptions.None) == 0;
			case StringComparison.InvariantCultureIgnoreCase:
				return CultureInfo.InvariantCulture.CompareInfo.Compare(a, b, CompareOptions.IgnoreCase) == 0;
			case StringComparison.Ordinal:
				return EqualsHelper(a, b);
			case StringComparison.OrdinalIgnoreCase:
				if (a.Length != b.Length)
				{
					return false;
				}
				if (a.IsAscii() && b.IsAscii())
				{
					return nativeCompareOrdinal(a, b, bIgnoreCase: true) == 0;
				}
				return TextInfo.CompareOrdinalIgnoreCase(a, b) == 0;
			default:
				throw new ArgumentException(Environment.GetResourceString("NotSupported_StringComparison"), "comparisonType");
			}
		}

		public static bool operator ==(string a, string b)
		{
			return Equals(a, b);
		}

		public static bool operator !=(string a, string b)
		{
			return !Equals(a, b);
		}

		public unsafe void CopyTo(int sourceIndex, char[] destination, int destinationIndex, int count)
		{
			if (destination == null)
			{
				throw new ArgumentNullException("destination");
			}
			if (count < 0)
			{
				throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("ArgumentOutOfRange_NegativeCount"));
			}
			if (sourceIndex < 0)
			{
				throw new ArgumentOutOfRangeException("sourceIndex", Environment.GetResourceString("ArgumentOutOfRange_Index"));
			}
			if (count > Length - sourceIndex)
			{
				throw new ArgumentOutOfRangeException("sourceIndex", Environment.GetResourceString("ArgumentOutOfRange_IndexCount"));
			}
			if (destinationIndex > destination.Length - count || destinationIndex < 0)
			{
				throw new ArgumentOutOfRangeException("destinationIndex", Environment.GetResourceString("ArgumentOutOfRange_IndexCount"));
			}
			if (count <= 0)
			{
				return;
			}
			fixed (char* ptr2 = &m_firstChar)
			{
				fixed (char* ptr = destination)
				{
					wstrcpy(ptr + destinationIndex, ptr2 + sourceIndex, count);
				}
			}
		}

		public unsafe char[] ToCharArray()
		{
			int length = Length;
			char[] array = new char[length];
			if (length > 0)
			{
				fixed (char* smem = &m_firstChar)
				{
					fixed (char* dmem = array)
					{
						wstrcpyPtrAligned(dmem, smem, length);
					}
				}
			}
			return array;
		}

		public unsafe char[] ToCharArray(int startIndex, int length)
		{
			if (startIndex < 0 || startIndex > Length || startIndex > Length - length)
			{
				throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("ArgumentOutOfRange_Index"));
			}
			if (length < 0)
			{
				throw new ArgumentOutOfRangeException("length", Environment.GetResourceString("ArgumentOutOfRange_Index"));
			}
			char[] array = new char[length];
			if (length > 0)
			{
				fixed (char* ptr = &m_firstChar)
				{
					fixed (char* dmem = array)
					{
						wstrcpy(dmem, ptr + startIndex, length);
					}
				}
			}
			return array;
		}

		public static bool IsNullOrEmpty(string value)
		{
			if (value != null)
			{
				return value.Length == 0;
			}
			return true;
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern int InternalMarvin32HashString(string s, int sLen, long additionalEntropy);

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		public unsafe override int GetHashCode()
		{
			fixed (char* ptr = this)
			{
				int num = 352654597;
				int num2 = num;
				int* ptr2 = (int*)ptr;
				for (int num3 = Length; num3 > 0; num3 -= 4)
				{
					num = ((num << 5) + num + (num >> 27)) ^ *ptr2;
					if (num3 <= 2)
					{
						break;
					}
					num2 = ((num2 << 5) + num2 + (num2 >> 27)) ^ ptr2[1];
					ptr2 += 2;
				}
				return num + num2 * 1566083941;
			}
		}

		public string[] Split(params char[] separator)
		{
			return Split(separator, int.MaxValue, StringSplitOptions.None);
		}

		public string[] Split(char[] separator, int count)
		{
			return Split(separator, count, StringSplitOptions.None);
		}

		[ComVisible(false)]
		public string[] Split(char[] separator, StringSplitOptions options)
		{
			return Split(separator, int.MaxValue, options);
		}

		[ComVisible(false)]
		public string[] Split(char[] separator, int count, StringSplitOptions options)
		{
			if (count < 0)
			{
				throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("ArgumentOutOfRange_NegativeCount"));
			}
			if (options < StringSplitOptions.None || options > StringSplitOptions.RemoveEmptyEntries)
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_EnumIllegalVal", (int)options));
			}
			bool flag = options == StringSplitOptions.RemoveEmptyEntries;
			if (count == 0 || (flag && Length == 0))
			{
				return new string[0];
			}
			int[] sepList = new int[Length];
			int num = MakeSeparatorList(separator, ref sepList);
			if (num == 0 || count == 1)
			{
				return new string[1]
				{
					this
				};
			}
			if (flag)
			{
				return InternalSplitOmitEmptyEntries(sepList, null, num, count);
			}
			return InternalSplitKeepEmptyEntries(sepList, null, num, count);
		}

		[ComVisible(false)]
		public string[] Split(string[] separator, StringSplitOptions options)
		{
			return Split(separator, int.MaxValue, options);
		}

		[ComVisible(false)]
		public string[] Split(string[] separator, int count, StringSplitOptions options)
		{
			if (count < 0)
			{
				throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("ArgumentOutOfRange_NegativeCount"));
			}
			if (options < StringSplitOptions.None || options > StringSplitOptions.RemoveEmptyEntries)
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_EnumIllegalVal", (int)options));
			}
			bool flag = options == StringSplitOptions.RemoveEmptyEntries;
			if (separator == null || separator.Length == 0)
			{
				return Split((char[])null, count, options);
			}
			if (count == 0 || (flag && Length == 0))
			{
				return new string[0];
			}
			int[] sepList = new int[Length];
			int[] lengthList = new int[Length];
			int num = MakeSeparatorList(separator, ref sepList, ref lengthList);
			if (num == 0 || count == 1)
			{
				return new string[1]
				{
					this
				};
			}
			if (flag)
			{
				return InternalSplitOmitEmptyEntries(sepList, lengthList, num, count);
			}
			return InternalSplitKeepEmptyEntries(sepList, lengthList, num, count);
		}

		private string[] InternalSplitKeepEmptyEntries(int[] sepList, int[] lengthList, int numReplaces, int count)
		{
			int num = 0;
			int num2 = 0;
			count--;
			int num3 = ((numReplaces < count) ? numReplaces : count);
			string[] array = new string[num3 + 1];
			for (int i = 0; i < num3; i++)
			{
				if (num >= Length)
				{
					break;
				}
				array[num2++] = Substring(num, sepList[i] - num);
				num = sepList[i] + ((lengthList == null) ? 1 : lengthList[i]);
			}
			if (num < Length && num3 >= 0)
			{
				array[num2] = Substring(num);
			}
			else if (num2 == num3)
			{
				array[num2] = Empty;
			}
			return array;
		}

		private string[] InternalSplitOmitEmptyEntries(int[] sepList, int[] lengthList, int numReplaces, int count)
		{
			int num = ((numReplaces < count) ? (numReplaces + 1) : count);
			string[] array = new string[num];
			int num2 = 0;
			int num3 = 0;
			for (int i = 0; i < numReplaces; i++)
			{
				if (num2 >= Length)
				{
					break;
				}
				if (sepList[i] - num2 > 0)
				{
					array[num3++] = Substring(num2, sepList[i] - num2);
				}
				num2 = sepList[i] + ((lengthList == null) ? 1 : lengthList[i]);
				if (num3 == count - 1)
				{
					while (i < numReplaces - 1 && num2 == sepList[++i])
					{
						num2 += ((lengthList == null) ? 1 : lengthList[i]);
					}
					break;
				}
			}
			if (num2 < Length)
			{
				array[num3++] = Substring(num2);
			}
			string[] array2 = array;
			if (num3 != num)
			{
				array2 = new string[num3];
				for (int j = 0; j < num3; j++)
				{
					array2[j] = array[j];
				}
			}
			return array2;
		}

		private unsafe int MakeSeparatorList(char[] separator, ref int[] sepList)
		{
			int num = 0;
			if (separator == null || separator.Length == 0)
			{
				fixed (char* ptr = &m_firstChar)
				{
					for (int i = 0; i < Length; i++)
					{
						if (num >= sepList.Length)
						{
							break;
						}
						if (char.IsWhiteSpace(ptr[i]))
						{
							sepList[num++] = i;
						}
					}
				}
			}
			else
			{
				int num2 = sepList.Length;
				int num3 = separator.Length;
				fixed (char* ptr4 = &m_firstChar)
				{
					fixed (char* ptr2 = separator)
					{
						for (int j = 0; j < Length; j++)
						{
							if (num >= num2)
							{
								break;
							}
							char* ptr3 = ptr2;
							int num4 = 0;
							while (num4 < num3)
							{
								if (ptr4[j] == *ptr3)
								{
									sepList[num++] = j;
									break;
								}
								num4++;
								ptr3++;
							}
						}
					}
				}
			}
			return num;
		}

		private unsafe int MakeSeparatorList(string[] separators, ref int[] sepList, ref int[] lengthList)
		{
			int num = 0;
			int num2 = sepList.Length;
			_ = separators.Length;
			fixed (char* ptr = &m_firstChar)
			{
				for (int i = 0; i < Length; i++)
				{
					if (num >= num2)
					{
						break;
					}
					foreach (string text in separators)
					{
						if (!IsNullOrEmpty(text))
						{
							int length = text.Length;
							if (ptr[i] == text[0] && length <= Length - i && (length == 1 || CompareOrdinal(this, i, text, 0, length) == 0))
							{
								sepList[num] = i;
								lengthList[num] = length;
								num++;
								i += length - 1;
								break;
							}
						}
					}
				}
			}
			return num;
		}

		public string Substring(int startIndex)
		{
			return Substring(startIndex, Length - startIndex);
		}

		public string Substring(int startIndex, int length)
		{
			return InternalSubStringWithChecks(startIndex, length, fAlwaysCopy: false);
		}

		internal string InternalSubStringWithChecks(int startIndex, int length, bool fAlwaysCopy)
		{
			int length2 = Length;
			if (startIndex < 0)
			{
				throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("ArgumentOutOfRange_StartIndex"));
			}
			if (startIndex > length2)
			{
				throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("ArgumentOutOfRange_StartIndexLargerThanLength"));
			}
			if (length < 0)
			{
				throw new ArgumentOutOfRangeException("length", Environment.GetResourceString("ArgumentOutOfRange_NegativeLength"));
			}
			if (startIndex > length2 - length)
			{
				throw new ArgumentOutOfRangeException("length", Environment.GetResourceString("ArgumentOutOfRange_IndexLength"));
			}
			if (length == 0)
			{
				return Empty;
			}
			return InternalSubString(startIndex, length, fAlwaysCopy);
		}

		private unsafe string InternalSubString(int startIndex, int length, bool fAlwaysCopy)
		{
			if (startIndex == 0 && length == Length && !fAlwaysCopy)
			{
				return this;
			}
			string text = FastAllocateString(length);
			fixed (char* dmem = &text.m_firstChar)
			{
				fixed (char* ptr = &m_firstChar)
				{
					wstrcpy(dmem, ptr + startIndex, length);
				}
			}
			return text;
		}

		public string Trim(params char[] trimChars)
		{
			if (trimChars == null || trimChars.Length == 0)
			{
				trimChars = WhitespaceChars;
			}
			return TrimHelper(trimChars, 2);
		}

		public string TrimStart(params char[] trimChars)
		{
			if (trimChars == null || trimChars.Length == 0)
			{
				trimChars = WhitespaceChars;
			}
			return TrimHelper(trimChars, 0);
		}

		public string TrimEnd(params char[] trimChars)
		{
			if (trimChars == null || trimChars.Length == 0)
			{
				trimChars = WhitespaceChars;
			}
			return TrimHelper(trimChars, 1);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[CLSCompliant(false)]
		public unsafe extern String(char* value);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[CLSCompliant(false)]
		public unsafe extern String(char* value, int startIndex, int length);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[CLSCompliant(false)]
		public unsafe extern String(sbyte* value);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[CLSCompliant(false)]
		public unsafe extern String(sbyte* value, int startIndex, int length);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[CLSCompliant(false)]
		public unsafe extern String(sbyte* value, int startIndex, int length, Encoding enc);

		private unsafe static string CreateString(sbyte* value, int startIndex, int length, Encoding enc)
		{
			if (enc == null)
			{
				return new string(value, startIndex, length);
			}
			if (length < 0)
			{
				throw new ArgumentOutOfRangeException("length", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
			}
			if (startIndex < 0)
			{
				throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("ArgumentOutOfRange_StartIndex"));
			}
			if (value + startIndex < value)
			{
				throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("ArgumentOutOfRange_PartialWCHAR"));
			}
			byte[] array = new byte[length];
			try
			{
				Buffer.memcpy((byte*)value, startIndex, array, 0, length);
			}
			catch (NullReferenceException)
			{
				throw new ArgumentOutOfRangeException("value", Environment.GetResourceString("ArgumentOutOfRange_PartialWCHAR"));
			}
			return enc.GetString(array);
		}

		internal unsafe static string CreateStringFromEncoding(byte* bytes, int byteLength, Encoding encoding)
		{
			int charCount = encoding.GetCharCount(bytes, byteLength, null);
			if (charCount == 0)
			{
				return Empty;
			}
			string text = FastAllocateString(charCount);
			fixed (char* chars = &text.m_firstChar)
			{
				encoding.GetChars(bytes, byteLength, chars, charCount, null);
			}
			return text;
		}

		internal unsafe byte[] ConvertToAnsi_BestFit_Throw(int iMaxDBCSCharByteSize)
		{
			int num = (Length + 3) * iMaxDBCSCharByteSize;
			byte[] array = new byte[num];
			uint flags = 0u;
			uint num2 = 0u;
			int num3;
			fixed (byte* pbDestBuffer = array)
			{
				fixed (char* pwzSource = &m_firstChar)
				{
					num3 = Win32Native.WideCharToMultiByte(0u, flags, pwzSource, Length, pbDestBuffer, num, IntPtr.Zero, new IntPtr(&num2));
				}
			}
			if (num2 != 0)
			{
				throw new ArgumentException(Environment.GetResourceString("Interop_Marshal_Unmappable_Char"));
			}
			array[num3] = 0;
			return array;
		}

		public bool IsNormalized()
		{
			return IsNormalized(NormalizationForm.FormC);
		}

		public bool IsNormalized(NormalizationForm normalizationForm)
		{
			if (IsFastSort() && (normalizationForm == NormalizationForm.FormC || normalizationForm == NormalizationForm.FormKC || normalizationForm == NormalizationForm.FormD || normalizationForm == NormalizationForm.FormKD))
			{
				return true;
			}
			return Normalization.IsNormalized(this, normalizationForm);
		}

		public string Normalize()
		{
			return Normalize(NormalizationForm.FormC);
		}

		public string Normalize(NormalizationForm normalizationForm)
		{
			if (IsAscii() && (normalizationForm == NormalizationForm.FormC || normalizationForm == NormalizationForm.FormKC || normalizationForm == NormalizationForm.FormD || normalizationForm == NormalizationForm.FormKD))
			{
				return this;
			}
			return Normalization.Normalize(this, normalizationForm);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern string FastAllocateString(int length);

		private unsafe static void FillStringChecked(string dest, int destPos, string src)
		{
			int length = src.Length;
			if (length > dest.Length - destPos)
			{
				throw new IndexOutOfRangeException();
			}
			fixed (char* ptr = &dest.m_firstChar)
			{
				fixed (char* smem = &src.m_firstChar)
				{
					wstrcpy(ptr + destPos, smem, length);
				}
			}
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		public extern String(char[] value, int startIndex, int length);

		[MethodImpl(MethodImplOptions.InternalCall)]
		public extern String(char[] value);

		private unsafe static void wstrcpyPtrAligned(char* dmem, char* smem, int charCount)
		{
			while (charCount >= 8)
			{
				*(uint*)dmem = *(uint*)smem;
				*(uint*)(dmem + 2) = *(uint*)(smem + 2);
				*(uint*)(dmem + 4) = *(uint*)(smem + 4);
				*(uint*)(dmem + 6) = *(uint*)(smem + 6);
				dmem += 8;
				smem += 8;
				charCount -= 8;
			}
			if (((uint)charCount & 4u) != 0)
			{
				*(uint*)dmem = *(uint*)smem;
				*(uint*)(dmem + 2) = *(uint*)(smem + 2);
				dmem += 4;
				smem += 4;
			}
			if (((uint)charCount & 2u) != 0)
			{
				*(uint*)dmem = *(uint*)smem;
				dmem += 2;
				smem += 2;
			}
			if (((uint)charCount & (true ? 1u : 0u)) != 0)
			{
				*dmem = *smem;
			}
		}

		private unsafe static void wstrcpy(char* dmem, char* smem, int charCount)
		{
			if (charCount > 0)
			{
				if (((uint)(int)dmem & 2u) != 0)
				{
					*dmem = *smem;
					dmem++;
					smem++;
					charCount--;
				}
				while (charCount >= 8)
				{
					*(uint*)dmem = *(uint*)smem;
					*(uint*)(dmem + 2) = *(uint*)(smem + 2);
					*(uint*)(dmem + 4) = *(uint*)(smem + 4);
					*(uint*)(dmem + 6) = *(uint*)(smem + 6);
					dmem += 8;
					smem += 8;
					charCount -= 8;
				}
				if (((uint)charCount & 4u) != 0)
				{
					*(uint*)dmem = *(uint*)smem;
					*(uint*)(dmem + 2) = *(uint*)(smem + 2);
					dmem += 4;
					smem += 4;
				}
				if (((uint)charCount & 2u) != 0)
				{
					*(uint*)dmem = *(uint*)smem;
					dmem += 2;
					smem += 2;
				}
				if (((uint)charCount & (true ? 1u : 0u)) != 0)
				{
					*dmem = *smem;
				}
			}
		}

		private unsafe string CtorCharArray(char[] value)
		{
			if (value != null && value.Length != 0)
			{
				string text = FastAllocateString(value.Length);
				fixed (char* dmem = text)
				{
					fixed (char* smem = value)
					{
						wstrcpyPtrAligned(dmem, smem, value.Length);
					}
				}
				return text;
			}
			return Empty;
		}

		private unsafe string CtorCharArrayStartLength(char[] value, int startIndex, int length)
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			if (startIndex < 0)
			{
				throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("ArgumentOutOfRange_StartIndex"));
			}
			if (length < 0)
			{
				throw new ArgumentOutOfRangeException("length", Environment.GetResourceString("ArgumentOutOfRange_NegativeLength"));
			}
			if (startIndex > value.Length - length)
			{
				throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("ArgumentOutOfRange_Index"));
			}
			if (length > 0)
			{
				string text = FastAllocateString(length);
				fixed (char* dmem = text)
				{
					fixed (char* ptr = value)
					{
						wstrcpy(dmem, ptr + startIndex, length);
					}
				}
				return text;
			}
			return Empty;
		}

		private unsafe string CtorCharCount(char c, int count)
		{
			if (count > 0)
			{
				string text = FastAllocateString(count);
				fixed (char* ptr = text)
				{
					char* ptr2 = ptr;
					while (((uint)(int)ptr2 & 3u) != 0 && count > 0)
					{
						char* num = ptr2;
						ptr2 = num + 1;
						*num = c;
						count--;
					}
					uint num2 = ((uint)c << 16) | c;
					if (count >= 4)
					{
						count -= 4;
						do
						{
							*(uint*)ptr2 = num2;
							*(uint*)(ptr2 + 2) = num2;
							ptr2 += 4;
							count -= 4;
						}
						while (count >= 0);
					}
					if (((uint)count & 2u) != 0)
					{
						*(uint*)ptr2 = num2;
						ptr2 += 2;
					}
					if (((uint)count & (true ? 1u : 0u)) != 0)
					{
						*ptr2 = c;
					}
				}
				return text;
			}
			if (count == 0)
			{
				return Empty;
			}
			throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("ArgumentOutOfRange_MustBeNonNegNum", "count"));
		}

		private unsafe static int wcslen(char* ptr)
		{
			char* ptr2;
			for (ptr2 = ptr; ((uint)(int)ptr2 & 3u) != 0 && *ptr2 != 0; ptr2++)
			{
			}
			if (*ptr2 != 0)
			{
				for (; (*ptr2 & ptr2[1]) != 0 || (*ptr2 != 0 && ptr2[1] != 0); ptr2 += 2)
				{
				}
			}
			for (; *ptr2 != 0; ptr2++)
			{
			}
			return (int)(ptr2 - ptr);
		}

		private unsafe string CtorCharPtr(char* ptr)
		{
			if ((nuint)ptr >= (nuint)64000u)
			{
				try
				{
					int num = wcslen(ptr);
					string text = FastAllocateString(num);
					try
					{
						fixed (char* dmem = text)
						{
							wstrcpy(dmem, ptr, num);
						}
					}
					finally
					{
					}
					return text;
				}
				catch (NullReferenceException)
				{
					throw new ArgumentOutOfRangeException("ptr", Environment.GetResourceString("ArgumentOutOfRange_PartialWCHAR"));
				}
			}
			if (ptr == null)
			{
				return Empty;
			}
			throw new ArgumentException(Environment.GetResourceString("Arg_MustBeStringPtrNotAtom"));
		}

		private unsafe string CtorCharPtrStartLength(char* ptr, int startIndex, int length)
		{
			if (length < 0)
			{
				throw new ArgumentOutOfRangeException("length", Environment.GetResourceString("ArgumentOutOfRange_NegativeLength"));
			}
			if (startIndex < 0)
			{
				throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("ArgumentOutOfRange_StartIndex"));
			}
			char* ptr2 = ptr + startIndex;
			if (ptr2 < ptr)
			{
				throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("ArgumentOutOfRange_PartialWCHAR"));
			}
			string text = FastAllocateString(length);
			try
			{
				try
				{
					fixed (char* dmem = text)
					{
						wstrcpy(dmem, ptr2, length);
					}
				}
				finally
				{
				}
				return text;
			}
			catch (NullReferenceException)
			{
				throw new ArgumentOutOfRangeException("ptr", Environment.GetResourceString("ArgumentOutOfRange_PartialWCHAR"));
			}
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		public extern String(char c, int count);

		public static int Compare(string strA, string strB)
		{
			return CultureInfo.CurrentCulture.CompareInfo.Compare(strA, strB, CompareOptions.None);
		}

		public static int Compare(string strA, string strB, bool ignoreCase)
		{
			if (ignoreCase)
			{
				return CultureInfo.CurrentCulture.CompareInfo.Compare(strA, strB, CompareOptions.IgnoreCase);
			}
			return CultureInfo.CurrentCulture.CompareInfo.Compare(strA, strB, CompareOptions.None);
		}

		public static int Compare(string strA, string strB, CultureInfo culture, CompareOptions options)
		{
			if (culture == null)
			{
				throw new ArgumentNullException("culture");
			}
			return culture.CompareInfo.Compare(strA, strB, options);
		}

		public static int Compare(string strA, int indexA, string strB, int indexB, int length, CultureInfo culture, CompareOptions options)
		{
			if (culture == null)
			{
				throw new ArgumentNullException("culture");
			}
			int num = length;
			int num2 = length;
			if (strA != null && strA.Length - indexA < num)
			{
				num = strA.Length - indexA;
			}
			if (strB != null && strB.Length - indexB < num2)
			{
				num2 = strB.Length - indexB;
			}
			return culture.CompareInfo.Compare(strA, indexA, num, strB, indexB, num2, options);
		}

		public static int Compare(string strA, string strB, StringComparison comparisonType)
		{
			if (comparisonType < StringComparison.CurrentCulture || comparisonType > StringComparison.OrdinalIgnoreCase)
			{
				throw new ArgumentException(Environment.GetResourceString("NotSupported_StringComparison"), "comparisonType");
			}
			if ((object)strA == strB)
			{
				return 0;
			}
			if (strA == null)
			{
				return -1;
			}
			if (strB == null)
			{
				return 1;
			}
			switch (comparisonType)
			{
			case StringComparison.CurrentCulture:
				return CultureInfo.CurrentCulture.CompareInfo.Compare(strA, strB, CompareOptions.None);
			case StringComparison.CurrentCultureIgnoreCase:
				return CultureInfo.CurrentCulture.CompareInfo.Compare(strA, strB, CompareOptions.IgnoreCase);
			case StringComparison.InvariantCulture:
				return CultureInfo.InvariantCulture.CompareInfo.Compare(strA, strB, CompareOptions.None);
			case StringComparison.InvariantCultureIgnoreCase:
				return CultureInfo.InvariantCulture.CompareInfo.Compare(strA, strB, CompareOptions.IgnoreCase);
			case StringComparison.Ordinal:
				return CompareOrdinalHelper(strA, strB);
			case StringComparison.OrdinalIgnoreCase:
				if (strA.IsAscii() && strB.IsAscii())
				{
					return nativeCompareOrdinal(strA, strB, bIgnoreCase: true);
				}
				return TextInfo.CompareOrdinalIgnoreCase(strA, strB);
			default:
				throw new NotSupportedException(Environment.GetResourceString("NotSupported_StringComparison"));
			}
		}

		public static int Compare(string strA, string strB, bool ignoreCase, CultureInfo culture)
		{
			if (culture == null)
			{
				throw new ArgumentNullException("culture");
			}
			if (ignoreCase)
			{
				return culture.CompareInfo.Compare(strA, strB, CompareOptions.IgnoreCase);
			}
			return culture.CompareInfo.Compare(strA, strB, CompareOptions.None);
		}

		public static int Compare(string strA, int indexA, string strB, int indexB, int length)
		{
			int num = length;
			int num2 = length;
			if (strA != null && strA.Length - indexA < num)
			{
				num = strA.Length - indexA;
			}
			if (strB != null && strB.Length - indexB < num2)
			{
				num2 = strB.Length - indexB;
			}
			return CultureInfo.CurrentCulture.CompareInfo.Compare(strA, indexA, num, strB, indexB, num2, CompareOptions.None);
		}

		public static int Compare(string strA, int indexA, string strB, int indexB, int length, bool ignoreCase)
		{
			int num = length;
			int num2 = length;
			if (strA != null && strA.Length - indexA < num)
			{
				num = strA.Length - indexA;
			}
			if (strB != null && strB.Length - indexB < num2)
			{
				num2 = strB.Length - indexB;
			}
			if (ignoreCase)
			{
				return CultureInfo.CurrentCulture.CompareInfo.Compare(strA, indexA, num, strB, indexB, num2, CompareOptions.IgnoreCase);
			}
			return CultureInfo.CurrentCulture.CompareInfo.Compare(strA, indexA, num, strB, indexB, num2, CompareOptions.None);
		}

		public static int Compare(string strA, int indexA, string strB, int indexB, int length, bool ignoreCase, CultureInfo culture)
		{
			if (culture == null)
			{
				throw new ArgumentNullException("culture");
			}
			int num = length;
			int num2 = length;
			if (strA != null && strA.Length - indexA < num)
			{
				num = strA.Length - indexA;
			}
			if (strB != null && strB.Length - indexB < num2)
			{
				num2 = strB.Length - indexB;
			}
			if (ignoreCase)
			{
				return culture.CompareInfo.Compare(strA, indexA, num, strB, indexB, num2, CompareOptions.IgnoreCase);
			}
			return culture.CompareInfo.Compare(strA, indexA, num, strB, indexB, num2, CompareOptions.None);
		}

		public static int Compare(string strA, int indexA, string strB, int indexB, int length, StringComparison comparisonType)
		{
			if (comparisonType < StringComparison.CurrentCulture || comparisonType > StringComparison.OrdinalIgnoreCase)
			{
				throw new ArgumentException(Environment.GetResourceString("NotSupported_StringComparison"), "comparisonType");
			}
			if (strA == null || strB == null)
			{
				if ((object)strA == strB)
				{
					return 0;
				}
				if (strA != null)
				{
					return 1;
				}
				return -1;
			}
			if (length < 0)
			{
				throw new ArgumentOutOfRangeException("length", Environment.GetResourceString("ArgumentOutOfRange_NegativeLength"));
			}
			if (indexA < 0)
			{
				throw new ArgumentOutOfRangeException("indexA", Environment.GetResourceString("ArgumentOutOfRange_Index"));
			}
			if (indexB < 0)
			{
				throw new ArgumentOutOfRangeException("indexB", Environment.GetResourceString("ArgumentOutOfRange_Index"));
			}
			if (strA.Length - indexA < 0)
			{
				throw new ArgumentOutOfRangeException("indexA", Environment.GetResourceString("ArgumentOutOfRange_Index"));
			}
			if (strB.Length - indexB < 0)
			{
				throw new ArgumentOutOfRangeException("indexB", Environment.GetResourceString("ArgumentOutOfRange_Index"));
			}
			if (length == 0 || (strA == strB && indexA == indexB))
			{
				return 0;
			}
			int num = length;
			int num2 = length;
			if (strA != null && strA.Length - indexA < num)
			{
				num = strA.Length - indexA;
			}
			if (strB != null && strB.Length - indexB < num2)
			{
				num2 = strB.Length - indexB;
			}
			return comparisonType switch
			{
				StringComparison.CurrentCulture => CultureInfo.CurrentCulture.CompareInfo.Compare(strA, indexA, num, strB, indexB, num2, CompareOptions.None), 
				StringComparison.CurrentCultureIgnoreCase => CultureInfo.CurrentCulture.CompareInfo.Compare(strA, indexA, num, strB, indexB, num2, CompareOptions.IgnoreCase), 
				StringComparison.InvariantCulture => CultureInfo.InvariantCulture.CompareInfo.Compare(strA, indexA, num, strB, indexB, num2, CompareOptions.None), 
				StringComparison.InvariantCultureIgnoreCase => CultureInfo.InvariantCulture.CompareInfo.Compare(strA, indexA, num, strB, indexB, num2, CompareOptions.IgnoreCase), 
				StringComparison.Ordinal => nativeCompareOrdinalEx(strA, indexA, strB, indexB, length), 
				StringComparison.OrdinalIgnoreCase => TextInfo.CompareOrdinalIgnoreCaseEx(strA, indexA, strB, indexB, length), 
				_ => throw new ArgumentException(Environment.GetResourceString("NotSupported_StringComparison")), 
			};
		}

		public int CompareTo(object value)
		{
			if (value == null)
			{
				return 1;
			}
			if (!(value is string))
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_MustBeString"));
			}
			return Compare(this, (string)value, StringComparison.CurrentCulture);
		}

		public int CompareTo(string strB)
		{
			if (strB == null)
			{
				return 1;
			}
			return CultureInfo.CurrentCulture.CompareInfo.Compare(this, strB, CompareOptions.None);
		}

		public static int CompareOrdinal(string strA, string strB)
		{
			if ((object)strA == strB)
			{
				return 0;
			}
			if (strA == null)
			{
				return -1;
			}
			if (strB == null)
			{
				return 1;
			}
			return CompareOrdinalHelper(strA, strB);
		}

		public static int CompareOrdinal(string strA, int indexA, string strB, int indexB, int length)
		{
			if (strA == null || strB == null)
			{
				if ((object)strA == strB)
				{
					return 0;
				}
				if (strA != null)
				{
					return 1;
				}
				return -1;
			}
			return nativeCompareOrdinalEx(strA, indexA, strB, indexB, length);
		}

		public bool Contains(string value)
		{
			return IndexOf(value, StringComparison.Ordinal) >= 0;
		}

		public bool EndsWith(string value)
		{
			return EndsWith(value, ignoreCase: false, null);
		}

		[ComVisible(false)]
		public bool EndsWith(string value, StringComparison comparisonType)
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			if (comparisonType < StringComparison.CurrentCulture || comparisonType > StringComparison.OrdinalIgnoreCase)
			{
				throw new ArgumentException(Environment.GetResourceString("NotSupported_StringComparison"), "comparisonType");
			}
			if ((object)this == value)
			{
				return true;
			}
			if (value.Length == 0)
			{
				return true;
			}
			switch (comparisonType)
			{
			case StringComparison.CurrentCulture:
				return CultureInfo.CurrentCulture.CompareInfo.IsSuffix(this, value, CompareOptions.None);
			case StringComparison.CurrentCultureIgnoreCase:
				return CultureInfo.CurrentCulture.CompareInfo.IsSuffix(this, value, CompareOptions.IgnoreCase);
			case StringComparison.InvariantCulture:
				return CultureInfo.InvariantCulture.CompareInfo.IsSuffix(this, value, CompareOptions.None);
			case StringComparison.InvariantCultureIgnoreCase:
				return CultureInfo.InvariantCulture.CompareInfo.IsSuffix(this, value, CompareOptions.IgnoreCase);
			case StringComparison.Ordinal:
				if (Length >= value.Length)
				{
					return nativeCompareOrdinalEx(this, Length - value.Length, value, 0, value.Length) == 0;
				}
				return false;
			case StringComparison.OrdinalIgnoreCase:
				if (Length >= value.Length)
				{
					return TextInfo.CompareOrdinalIgnoreCaseEx(this, Length - value.Length, value, 0, value.Length) == 0;
				}
				return false;
			default:
				throw new ArgumentException(Environment.GetResourceString("NotSupported_StringComparison"), "comparisonType");
			}
		}

		public bool EndsWith(string value, bool ignoreCase, CultureInfo culture)
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			if ((object)this == value)
			{
				return true;
			}
			CultureInfo cultureInfo = ((culture == null) ? CultureInfo.CurrentCulture : culture);
			return cultureInfo.CompareInfo.IsSuffix(this, value, ignoreCase ? CompareOptions.IgnoreCase : CompareOptions.None);
		}

		internal bool EndsWith(char value)
		{
			int length = Length;
			if (length != 0 && this[length - 1] == value)
			{
				return true;
			}
			return false;
		}

		public int IndexOf(char value)
		{
			return IndexOf(value, 0, Length);
		}

		public int IndexOf(char value, int startIndex)
		{
			return IndexOf(value, startIndex, Length - startIndex);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		public extern int IndexOf(char value, int startIndex, int count);

		public int IndexOfAny(char[] anyOf)
		{
			return IndexOfAny(anyOf, 0, Length);
		}

		public int IndexOfAny(char[] anyOf, int startIndex)
		{
			return IndexOfAny(anyOf, startIndex, Length - startIndex);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		public extern int IndexOfAny(char[] anyOf, int startIndex, int count);

		public int IndexOf(string value)
		{
			return CultureInfo.CurrentCulture.CompareInfo.IndexOf(this, value);
		}

		public int IndexOf(string value, int startIndex)
		{
			return CultureInfo.CurrentCulture.CompareInfo.IndexOf(this, value, startIndex);
		}

		public int IndexOf(string value, int startIndex, int count)
		{
			if (startIndex < 0 || startIndex > Length)
			{
				throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("ArgumentOutOfRange_Index"));
			}
			if (count < 0 || count > Length - startIndex)
			{
				throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("ArgumentOutOfRange_Count"));
			}
			return CultureInfo.CurrentCulture.CompareInfo.IndexOf(this, value, startIndex, count, CompareOptions.None);
		}

		public int IndexOf(string value, StringComparison comparisonType)
		{
			return IndexOf(value, 0, Length, comparisonType);
		}

		public int IndexOf(string value, int startIndex, StringComparison comparisonType)
		{
			return IndexOf(value, startIndex, Length - startIndex, comparisonType);
		}

		public int IndexOf(string value, int startIndex, int count, StringComparison comparisonType)
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			if (startIndex < 0 || startIndex > Length)
			{
				throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("ArgumentOutOfRange_Index"));
			}
			if (count < 0 || startIndex > Length - count)
			{
				throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("ArgumentOutOfRange_Count"));
			}
			return comparisonType switch
			{
				StringComparison.CurrentCulture => CultureInfo.CurrentCulture.CompareInfo.IndexOf(this, value, startIndex, count, CompareOptions.None), 
				StringComparison.CurrentCultureIgnoreCase => CultureInfo.CurrentCulture.CompareInfo.IndexOf(this, value, startIndex, count, CompareOptions.IgnoreCase), 
				StringComparison.InvariantCulture => CultureInfo.InvariantCulture.CompareInfo.IndexOf(this, value, startIndex, count, CompareOptions.None), 
				StringComparison.InvariantCultureIgnoreCase => CultureInfo.InvariantCulture.CompareInfo.IndexOf(this, value, startIndex, count, CompareOptions.IgnoreCase), 
				StringComparison.Ordinal => CultureInfo.InvariantCulture.CompareInfo.IndexOf(this, value, startIndex, count, CompareOptions.Ordinal), 
				StringComparison.OrdinalIgnoreCase => TextInfo.IndexOfStringOrdinalIgnoreCase(this, value, startIndex, count), 
				_ => throw new ArgumentException(Environment.GetResourceString("NotSupported_StringComparison"), "comparisonType"), 
			};
		}

		public int LastIndexOf(char value)
		{
			return LastIndexOf(value, Length - 1, Length);
		}

		public int LastIndexOf(char value, int startIndex)
		{
			return LastIndexOf(value, startIndex, startIndex + 1);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		public extern int LastIndexOf(char value, int startIndex, int count);

		public int LastIndexOfAny(char[] anyOf)
		{
			return LastIndexOfAny(anyOf, Length - 1, Length);
		}

		public int LastIndexOfAny(char[] anyOf, int startIndex)
		{
			return LastIndexOfAny(anyOf, startIndex, startIndex + 1);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		public extern int LastIndexOfAny(char[] anyOf, int startIndex, int count);

		public int LastIndexOf(string value)
		{
			return LastIndexOf(value, Length - 1, Length, StringComparison.CurrentCulture);
		}

		public int LastIndexOf(string value, int startIndex)
		{
			return LastIndexOf(value, startIndex, startIndex + 1, StringComparison.CurrentCulture);
		}

		public int LastIndexOf(string value, int startIndex, int count)
		{
			if (count < 0)
			{
				throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("ArgumentOutOfRange_Count"));
			}
			return CultureInfo.CurrentCulture.CompareInfo.LastIndexOf(this, value, startIndex, count, CompareOptions.None);
		}

		public int LastIndexOf(string value, StringComparison comparisonType)
		{
			return LastIndexOf(value, Length - 1, Length, comparisonType);
		}

		public int LastIndexOf(string value, int startIndex, StringComparison comparisonType)
		{
			return LastIndexOf(value, startIndex, startIndex + 1, comparisonType);
		}

		public int LastIndexOf(string value, int startIndex, int count, StringComparison comparisonType)
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			if (Length == 0 && (startIndex == -1 || startIndex == 0))
			{
				if (value.Length != 0)
				{
					return -1;
				}
				return 0;
			}
			if (startIndex < 0 || startIndex > Length)
			{
				throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("ArgumentOutOfRange_Index"));
			}
			if (startIndex == Length)
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
			return comparisonType switch
			{
				StringComparison.CurrentCulture => CultureInfo.CurrentCulture.CompareInfo.LastIndexOf(this, value, startIndex, count, CompareOptions.None), 
				StringComparison.CurrentCultureIgnoreCase => CultureInfo.CurrentCulture.CompareInfo.LastIndexOf(this, value, startIndex, count, CompareOptions.IgnoreCase), 
				StringComparison.InvariantCulture => CultureInfo.InvariantCulture.CompareInfo.LastIndexOf(this, value, startIndex, count, CompareOptions.None), 
				StringComparison.InvariantCultureIgnoreCase => CultureInfo.InvariantCulture.CompareInfo.LastIndexOf(this, value, startIndex, count, CompareOptions.IgnoreCase), 
				StringComparison.Ordinal => CultureInfo.InvariantCulture.CompareInfo.LastIndexOf(this, value, startIndex, count, CompareOptions.Ordinal), 
				StringComparison.OrdinalIgnoreCase => TextInfo.LastIndexOfStringOrdinalIgnoreCase(this, value, startIndex, count), 
				_ => throw new ArgumentException(Environment.GetResourceString("NotSupported_StringComparison"), "comparisonType"), 
			};
		}

		public string PadLeft(int totalWidth)
		{
			return PadHelper(totalWidth, ' ', isRightPadded: false);
		}

		public string PadLeft(int totalWidth, char paddingChar)
		{
			return PadHelper(totalWidth, paddingChar, isRightPadded: false);
		}

		public string PadRight(int totalWidth)
		{
			return PadHelper(totalWidth, ' ', isRightPadded: true);
		}

		public string PadRight(int totalWidth, char paddingChar)
		{
			return PadHelper(totalWidth, paddingChar, isRightPadded: true);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern string PadHelper(int totalWidth, char paddingChar, bool isRightPadded);

		public bool StartsWith(string value)
		{
			return StartsWith(value, ignoreCase: false, null);
		}

		[ComVisible(false)]
		public bool StartsWith(string value, StringComparison comparisonType)
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			if (comparisonType < StringComparison.CurrentCulture || comparisonType > StringComparison.OrdinalIgnoreCase)
			{
				throw new ArgumentException(Environment.GetResourceString("NotSupported_StringComparison"), "comparisonType");
			}
			if ((object)this == value)
			{
				return true;
			}
			if (value.Length == 0)
			{
				return true;
			}
			switch (comparisonType)
			{
			case StringComparison.CurrentCulture:
				return CultureInfo.CurrentCulture.CompareInfo.IsPrefix(this, value, CompareOptions.None);
			case StringComparison.CurrentCultureIgnoreCase:
				return CultureInfo.CurrentCulture.CompareInfo.IsPrefix(this, value, CompareOptions.IgnoreCase);
			case StringComparison.InvariantCulture:
				return CultureInfo.InvariantCulture.CompareInfo.IsPrefix(this, value, CompareOptions.None);
			case StringComparison.InvariantCultureIgnoreCase:
				return CultureInfo.InvariantCulture.CompareInfo.IsPrefix(this, value, CompareOptions.IgnoreCase);
			case StringComparison.Ordinal:
				if (Length < value.Length)
				{
					return false;
				}
				return nativeCompareOrdinalEx(this, 0, value, 0, value.Length) == 0;
			case StringComparison.OrdinalIgnoreCase:
				if (Length < value.Length)
				{
					return false;
				}
				return TextInfo.CompareOrdinalIgnoreCaseEx(this, 0, value, 0, value.Length) == 0;
			default:
				throw new ArgumentException(Environment.GetResourceString("NotSupported_StringComparison"), "comparisonType");
			}
		}

		public bool StartsWith(string value, bool ignoreCase, CultureInfo culture)
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			if ((object)this == value)
			{
				return true;
			}
			CultureInfo cultureInfo = ((culture == null) ? CultureInfo.CurrentCulture : culture);
			return cultureInfo.CompareInfo.IsPrefix(this, value, ignoreCase ? CompareOptions.IgnoreCase : CompareOptions.None);
		}

		public string ToLower()
		{
			return ToLower(CultureInfo.CurrentCulture);
		}

		public string ToLower(CultureInfo culture)
		{
			if (culture == null)
			{
				throw new ArgumentNullException("culture");
			}
			return culture.TextInfo.ToLower(this);
		}

		public string ToLowerInvariant()
		{
			return ToLower(CultureInfo.InvariantCulture);
		}

		public string ToUpper()
		{
			return ToUpper(CultureInfo.CurrentCulture);
		}

		public string ToUpper(CultureInfo culture)
		{
			if (culture == null)
			{
				throw new ArgumentNullException("culture");
			}
			return culture.TextInfo.ToUpper(this);
		}

		public string ToUpperInvariant()
		{
			return ToUpper(CultureInfo.InvariantCulture);
		}

		public override string ToString()
		{
			return this;
		}

		public string ToString(IFormatProvider provider)
		{
			return this;
		}

		public object Clone()
		{
			return this;
		}

		public string Trim()
		{
			return TrimHelper(WhitespaceChars, 2);
		}

		private string TrimHelper(char[] trimChars, int trimType)
		{
			int num = Length - 1;
			int i = 0;
			if (trimType != 1)
			{
				for (i = 0; i < Length; i++)
				{
					int num2 = 0;
					char c = this[i];
					for (num2 = 0; num2 < trimChars.Length && trimChars[num2] != c; num2++)
					{
					}
					if (num2 == trimChars.Length)
					{
						break;
					}
				}
			}
			if (trimType != 0)
			{
				for (num = Length - 1; num >= i; num--)
				{
					int num3 = 0;
					char c2 = this[num];
					for (num3 = 0; num3 < trimChars.Length && trimChars[num3] != c2; num3++)
					{
					}
					if (num3 == trimChars.Length)
					{
						break;
					}
				}
			}
			int num4 = num - i + 1;
			if (num4 == Length)
			{
				return this;
			}
			if (num4 == 0)
			{
				return Empty;
			}
			return InternalSubString(i, num4, fAlwaysCopy: false);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		public extern string Insert(int startIndex, string value);

		[MethodImpl(MethodImplOptions.InternalCall)]
		public extern string Replace(char oldChar, char newChar);

		[MethodImpl(MethodImplOptions.InternalCall)]
		public extern string Replace(string oldValue, string newValue);

		[MethodImpl(MethodImplOptions.InternalCall)]
		public extern string Remove(int startIndex, int count);

		public string Remove(int startIndex)
		{
			if (startIndex < 0)
			{
				throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("ArgumentOutOfRange_StartIndex"));
			}
			if (startIndex >= Length)
			{
				throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("ArgumentOutOfRange_StartIndexLessThanLength"));
			}
			return Substring(0, startIndex);
		}

		public static string Format(string format, object arg0)
		{
			return Format(null, format, arg0);
		}

		public static string Format(string format, object arg0, object arg1)
		{
			return Format(null, format, arg0, arg1);
		}

		public static string Format(string format, object arg0, object arg1, object arg2)
		{
			return Format(null, format, arg0, arg1, arg2);
		}

		public static string Format(string format, params object[] args)
		{
			return Format(null, format, args);
		}

		public static string Format(IFormatProvider provider, string format, params object[] args)
		{
			if (format == null || args == null)
			{
				throw new ArgumentNullException((format == null) ? "format" : "args");
			}
			StringBuilder stringBuilder = new StringBuilder(format.Length + args.Length * 8);
			stringBuilder.AppendFormat(provider, format, args);
			return stringBuilder.ToString();
		}

		public unsafe static string Copy(string str)
		{
			if (str == null)
			{
				throw new ArgumentNullException("str");
			}
			int length = str.Length;
			string text = FastAllocateString(length);
			fixed (char* dmem = &text.m_firstChar)
			{
				fixed (char* smem = &str.m_firstChar)
				{
					wstrcpyPtrAligned(dmem, smem, length);
				}
			}
			return text;
		}

		internal unsafe static string InternalCopy(string str)
		{
			int length = str.Length;
			string text = FastAllocateString(length);
			fixed (char* dmem = &text.m_firstChar)
			{
				fixed (char* smem = &str.m_firstChar)
				{
					wstrcpyPtrAligned(dmem, smem, length);
				}
			}
			return text;
		}

		public static string Concat(object arg0)
		{
			if (arg0 == null)
			{
				return Empty;
			}
			return arg0.ToString();
		}

		public static string Concat(object arg0, object arg1)
		{
			if (arg0 == null)
			{
				arg0 = Empty;
			}
			if (arg1 == null)
			{
				arg1 = Empty;
			}
			return arg0.ToString() + arg1.ToString();
		}

		public static string Concat(object arg0, object arg1, object arg2)
		{
			if (arg0 == null)
			{
				arg0 = Empty;
			}
			if (arg1 == null)
			{
				arg1 = Empty;
			}
			if (arg2 == null)
			{
				arg2 = Empty;
			}
			return arg0.ToString() + arg1.ToString() + arg2.ToString();
		}

		[CLSCompliant(false)]
		public static string Concat(object arg0, object arg1, object arg2, object arg3, __arglist)
		{
			ArgIterator argIterator = new ArgIterator(__arglist);
			int num = argIterator.GetRemainingCount() + 4;
			object[] array = new object[num];
			array[0] = arg0;
			array[1] = arg1;
			array[2] = arg2;
			array[3] = arg3;
			for (int i = 4; i < num; i++)
			{
				array[i] = TypedReference.ToObject(argIterator.GetNextArg());
			}
			return Concat(array);
		}

		public static string Concat(params object[] args)
		{
			if (args == null)
			{
				throw new ArgumentNullException("args");
			}
			string[] array = new string[args.Length];
			int num = 0;
			for (int i = 0; i < args.Length; i++)
			{
				object obj = args[i];
				array[i] = ((obj == null) ? Empty : obj.ToString());
				num += array[i].Length;
				if (num < 0)
				{
					throw new OutOfMemoryException();
				}
			}
			return ConcatArray(array, num);
		}

		public static string Concat(string str0, string str1)
		{
			if (IsNullOrEmpty(str0))
			{
				if (IsNullOrEmpty(str1))
				{
					return Empty;
				}
				return str1;
			}
			if (IsNullOrEmpty(str1))
			{
				return str0;
			}
			int length = str0.Length;
			string text = FastAllocateString(length + str1.Length);
			FillStringChecked(text, 0, str0);
			FillStringChecked(text, length, str1);
			return text;
		}

		public static string Concat(string str0, string str1, string str2)
		{
			if (str0 == null && str1 == null && str2 == null)
			{
				return Empty;
			}
			if (str0 == null)
			{
				str0 = Empty;
			}
			if (str1 == null)
			{
				str1 = Empty;
			}
			if (str2 == null)
			{
				str2 = Empty;
			}
			int length = str0.Length + str1.Length + str2.Length;
			string text = FastAllocateString(length);
			FillStringChecked(text, 0, str0);
			FillStringChecked(text, str0.Length, str1);
			FillStringChecked(text, str0.Length + str1.Length, str2);
			return text;
		}

		public static string Concat(string str0, string str1, string str2, string str3)
		{
			if (str0 == null && str1 == null && str2 == null && str3 == null)
			{
				return Empty;
			}
			if (str0 == null)
			{
				str0 = Empty;
			}
			if (str1 == null)
			{
				str1 = Empty;
			}
			if (str2 == null)
			{
				str2 = Empty;
			}
			if (str3 == null)
			{
				str3 = Empty;
			}
			int length = str0.Length + str1.Length + str2.Length + str3.Length;
			string text = FastAllocateString(length);
			FillStringChecked(text, 0, str0);
			FillStringChecked(text, str0.Length, str1);
			FillStringChecked(text, str0.Length + str1.Length, str2);
			FillStringChecked(text, str0.Length + str1.Length + str2.Length, str3);
			return text;
		}

		private static string ConcatArray(string[] values, int totalLength)
		{
			string text = FastAllocateString(totalLength);
			int num = 0;
			for (int i = 0; i < values.Length; i++)
			{
				FillStringChecked(text, num, values[i]);
				num += values[i].Length;
			}
			return text;
		}

		public static string Concat(params string[] values)
		{
			int num = 0;
			if (values == null)
			{
				throw new ArgumentNullException("values");
			}
			string[] array = new string[values.Length];
			for (int i = 0; i < values.Length; i++)
			{
				string text = values[i];
				array[i] = ((text == null) ? Empty : text);
				num += array[i].Length;
				if (num < 0)
				{
					throw new OutOfMemoryException();
				}
			}
			return ConcatArray(array, num);
		}

		public static string Intern(string str)
		{
			if (str == null)
			{
				throw new ArgumentNullException("str");
			}
			return Thread.GetDomain().GetOrInternString(str);
		}

		public static string IsInterned(string str)
		{
			if (str == null)
			{
				throw new ArgumentNullException("str");
			}
			return Thread.GetDomain().IsStringInterned(str);
		}

		public TypeCode GetTypeCode()
		{
			return TypeCode.String;
		}

		bool IConvertible.ToBoolean(IFormatProvider provider)
		{
			return Convert.ToBoolean(this, provider);
		}

		char IConvertible.ToChar(IFormatProvider provider)
		{
			return Convert.ToChar(this, provider);
		}

		sbyte IConvertible.ToSByte(IFormatProvider provider)
		{
			return Convert.ToSByte(this, provider);
		}

		byte IConvertible.ToByte(IFormatProvider provider)
		{
			return Convert.ToByte(this, provider);
		}

		short IConvertible.ToInt16(IFormatProvider provider)
		{
			return Convert.ToInt16(this, provider);
		}

		ushort IConvertible.ToUInt16(IFormatProvider provider)
		{
			return Convert.ToUInt16(this, provider);
		}

		int IConvertible.ToInt32(IFormatProvider provider)
		{
			return Convert.ToInt32(this, provider);
		}

		uint IConvertible.ToUInt32(IFormatProvider provider)
		{
			return Convert.ToUInt32(this, provider);
		}

		long IConvertible.ToInt64(IFormatProvider provider)
		{
			return Convert.ToInt64(this, provider);
		}

		ulong IConvertible.ToUInt64(IFormatProvider provider)
		{
			return Convert.ToUInt64(this, provider);
		}

		float IConvertible.ToSingle(IFormatProvider provider)
		{
			return Convert.ToSingle(this, provider);
		}

		double IConvertible.ToDouble(IFormatProvider provider)
		{
			return Convert.ToDouble(this, provider);
		}

		decimal IConvertible.ToDecimal(IFormatProvider provider)
		{
			return Convert.ToDecimal(this, provider);
		}

		DateTime IConvertible.ToDateTime(IFormatProvider provider)
		{
			return Convert.ToDateTime(this, provider);
		}

		object IConvertible.ToType(Type type, IFormatProvider provider)
		{
			return Convert.DefaultToType(this, type, provider);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal extern bool IsFastSort();

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal extern bool IsAscii();

		internal unsafe void SetChar(int index, char value)
		{
			if ((uint)index >= (uint)Length)
			{
				throw new ArgumentOutOfRangeException("index", Environment.GetResourceString("ArgumentOutOfRange_Index"));
			}
			fixed (char* ptr = &m_firstChar)
			{
				ptr[index] = value;
			}
		}

		internal unsafe void AppendInPlace(char value, int currentLength)
		{
			fixed (char* ptr = &m_firstChar)
			{
				ptr[currentLength] = value;
				currentLength++;
				ptr[currentLength] = '\0';
				m_stringLength = currentLength;
			}
		}

		internal unsafe void AppendInPlace(char value, int repeatCount, int currentLength)
		{
			int num = currentLength + repeatCount;
			fixed (char* ptr = &m_firstChar)
			{
				int i;
				for (i = currentLength; i < num; i++)
				{
					ptr[i] = value;
				}
				ptr[i] = '\0';
			}
			m_stringLength = num;
		}

		internal unsafe void AppendInPlace(string value, int currentLength)
		{
			int length = value.Length;
			int num = currentLength + length;
			fixed (char* ptr = &m_firstChar)
			{
				fixed (char* smem = &value.m_firstChar)
				{
					wstrcpy(ptr + currentLength, smem, length);
				}
				ptr[num] = '\0';
			}
			m_stringLength = num;
		}

		internal unsafe void AppendInPlace(string value, int startIndex, int count, int currentLength)
		{
			int num = currentLength + count;
			fixed (char* ptr = &m_firstChar)
			{
				fixed (char* ptr2 = &value.m_firstChar)
				{
					wstrcpy(ptr + currentLength, ptr2 + startIndex, count);
				}
				ptr[num] = '\0';
			}
			m_stringLength = num;
		}

		internal unsafe void AppendInPlace(char* value, int count, int currentLength)
		{
			int num = currentLength + count;
			fixed (char* ptr = &m_firstChar)
			{
				wstrcpy(ptr + currentLength, value, count);
				ptr[num] = '\0';
			}
			m_stringLength = num;
		}

		internal unsafe void AppendInPlace(char[] value, int start, int count, int currentLength)
		{
			int num = currentLength + count;
			fixed (char* ptr = &m_firstChar)
			{
				if (count > 0)
				{
					fixed (char* ptr2 = value)
					{
						wstrcpy(ptr + currentLength, ptr2 + start, count);
					}
				}
				ptr[num] = '\0';
			}
			m_stringLength = num;
		}

		internal unsafe void ReplaceCharInPlace(char oldChar, char newChar, int startIndex, int count, int currentLength)
		{
			int num = startIndex + count;
			fixed (char* ptr = &m_firstChar)
			{
				for (int i = startIndex; i < num; i++)
				{
					if (ptr[i] == oldChar)
					{
						ptr[i] = newChar;
					}
				}
			}
		}

		internal static string GetStringForStringBuilder(string value, int capacity)
		{
			return GetStringForStringBuilder(value, 0, value.Length, capacity);
		}

		internal unsafe static string GetStringForStringBuilder(string value, int startIndex, int length, int capacity)
		{
			string text = FastAllocateString(capacity);
			if (value.Length == 0)
			{
				text.SetLength(0);
				return text;
			}
			fixed (char* dmem = &text.m_firstChar)
			{
				fixed (char* ptr = &value.m_firstChar)
				{
					wstrcpy(dmem, ptr + startIndex, length);
				}
			}
			text.SetLength(length);
			return text;
		}

		private unsafe void NullTerminate()
		{
			fixed (char* ptr = &m_firstChar)
			{
				ptr[m_stringLength] = '\0';
			}
		}

		internal unsafe void ClearPostNullChar()
		{
			int num = Length + 1;
			if (num < m_arrayLength)
			{
				fixed (char* ptr = &m_firstChar)
				{
					ptr[num] = '\0';
				}
			}
		}

		internal void SetLength(int newLength)
		{
			m_stringLength = newLength;
		}

		public CharEnumerator GetEnumerator()
		{
			return new CharEnumerator(this);
		}

		IEnumerator<char> IEnumerable<char>.GetEnumerator()
		{
			return new CharEnumerator(this);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return new CharEnumerator(this);
		}

		internal unsafe void InternalSetCharNoBoundsCheck(int index, char value)
		{
			fixed (char* ptr = &m_firstChar)
			{
				ptr[index] = value;
			}
		}

		internal unsafe static void InternalCopy(string src, IntPtr dest, int len)
		{
			if (len != 0)
			{
				fixed (char* ptr = &src.m_firstChar)
				{
					byte* src2 = (byte*)ptr;
					byte* dest2 = (byte*)dest.ToPointer();
					Buffer.memcpyimpl(src2, dest2, len);
				}
			}
		}

		internal unsafe static void InternalMemCpy(string src, int srcOffset, string dst, int destOffset, int len)
		{
			if (len == 0)
			{
				return;
			}
			fixed (char* ptr = &src.m_firstChar)
			{
				fixed (char* ptr2 = &dst.m_firstChar)
				{
					Buffer.memcpyimpl((byte*)(ptr + srcOffset), (byte*)(ptr2 + destOffset), len);
				}
			}
		}

		internal unsafe static void revmemcpyimpl(byte* src, byte* dest, int len)
		{
			if (len == 0)
			{
				return;
			}
			dest += len;
			src += len;
			if (((ulong)src & 3uL) != 0)
			{
				do
				{
					dest--;
					src--;
					len--;
					*dest = *src;
				}
				while (len > 0 && ((ulong)src & 3uL) != 0);
			}
			if (len >= 16)
			{
				len -= 16;
				do
				{
					dest = (byte*)(dest - (byte*)16);
					src = (byte*)(src - (byte*)16);
					*(int*)(dest + 12) = *(int*)(src + 12);
					*(int*)(dest + 8) = *(int*)(src + 8);
					*(int*)(dest + 4) = *(int*)(src + 4);
					*(int*)dest = *(int*)src;
				}
				while ((len -= 16) >= 0);
			}
			if ((len & 8) > 0)
			{
				dest = (byte*)(dest - (byte*)8);
				src = (byte*)(src - (byte*)8);
				*(int*)(dest + 4) = *(int*)(src + 4);
				*(int*)dest = *(int*)src;
			}
			if ((len & 4) > 0)
			{
				dest = (byte*)(dest - (byte*)4);
				src = (byte*)(src - (byte*)4);
				*(int*)dest = *(int*)src;
			}
			if (((uint)len & 2u) != 0)
			{
				dest = (byte*)(dest - (byte*)2);
				src = (byte*)(src - (byte*)2);
				*(short*)dest = *(short*)src;
			}
			if (((uint)len & (true ? 1u : 0u)) != 0)
			{
				dest--;
				src--;
				*dest = *src;
			}
		}

		internal unsafe void InsertInPlace(int index, string value, int repeatCount, int currentLength, int requiredLength)
		{
			fixed (char* ptr = &m_firstChar)
			{
				fixed (char* src = &value.m_firstChar)
				{
					revmemcpyimpl((byte*)(ptr + index), (byte*)(ptr + index + value.Length * repeatCount), (currentLength - index) * 2);
					for (int i = 0; i < repeatCount; i++)
					{
						Buffer.memcpyimpl((byte*)src, (byte*)(ptr + index + i * value.Length), value.Length * 2);
					}
				}
			}
			SetLength(requiredLength);
			NullTerminate();
		}

		internal unsafe void InsertInPlace(int index, char[] value, int startIndex, int charCount, int currentLength, int requiredLength)
		{
			fixed (char* ptr = &m_firstChar)
			{
				fixed (char* ptr2 = value)
				{
					revmemcpyimpl((byte*)(ptr + index), (byte*)(ptr + index + charCount), (currentLength - index) * 2);
					Buffer.memcpyimpl((byte*)(ptr2 + startIndex), (byte*)(ptr + index), charCount * 2);
				}
			}
			SetLength(requiredLength);
			NullTerminate();
		}

		internal void RemoveInPlace(int index, int charCount, int currentLength)
		{
			InternalMemCpy(this, index + charCount, this, index, (currentLength - charCount - index) * 2);
			int length = currentLength - charCount;
			SetLength(length);
			NullTerminate();
		}
	}
}
