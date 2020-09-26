using System.Globalization;
using System.Net;

namespace System
{
	internal class DomainNameHelper
	{
		private const char c_DummyChar = '\uffff';

		internal const string Localhost = "localhost";

		internal const string Loopback = "loopback";

		private static readonly char[] s_UnsafeForNormalizedHost = new char[8]
		{
			'\\',
			'/',
			'?',
			'@',
			'#',
			':',
			'[',
			']'
		};

		private DomainNameHelper()
		{
		}

		internal static string ParseCanonicalName(string str, int start, int end, ref bool loopback)
		{
			string text = null;
			for (int num = end - 1; num >= start; num--)
			{
				if (str[num] >= 'A' && str[num] <= 'Z')
				{
					text = str.Substring(start, end - start).ToLower(CultureInfo.InvariantCulture);
					break;
				}
				if (str[num] == ':')
				{
					end = num;
				}
			}
			if (text == null)
			{
				text = str.Substring(start, end - start);
			}
			if (text == "localhost" || text == "loopback")
			{
				loopback = true;
				return "localhost";
			}
			return text;
		}

		internal unsafe static bool IsValid(char* name, ushort pos, ref int returnedEnd, ref bool notCanonical, bool notImplicitFile)
		{
			char* ptr = name + (int)pos;
			char* ptr2 = ptr;
			char* ptr3;
			for (ptr3 = name + returnedEnd; ptr2 < ptr3; ptr2++)
			{
				char c = *ptr2;
				if (c > '\u007f')
				{
					return false;
				}
				if (c == '/' || c == '\\' || (notImplicitFile && (c == ':' || c == '?' || c == '#')))
				{
					ptr3 = ptr2;
					break;
				}
			}
			if (ptr3 == ptr)
			{
				return false;
			}
			while (true)
			{
				for (ptr2 = ptr; ptr2 < ptr3 && *ptr2 != '.'; ptr2++)
				{
				}
				if (ptr != ptr2 && ptr2 - ptr <= 63)
				{
					char* num = ptr;
					ptr = num + 1;
					if (IsASCIILetterOrDigit(*num, ref notCanonical))
					{
						while (ptr < ptr2)
						{
							char* num2 = ptr;
							ptr = num2 + 1;
							if (!IsValidDomainLabelCharacter(*num2, ref notCanonical))
							{
								return false;
							}
						}
						ptr++;
						if (ptr >= ptr3)
						{
							break;
						}
						continue;
					}
				}
				return false;
			}
			returnedEnd = (ushort)(ptr3 - name);
			return true;
		}

		internal unsafe static bool IsValidByIri(char* name, ushort pos, ref int returnedEnd, ref bool notCanonical, bool notImplicitFile)
		{
			char* ptr = name + (int)pos;
			char* ptr2 = ptr;
			char* ptr3 = name + returnedEnd;
			int num = 0;
			for (; ptr2 < ptr3; ptr2++)
			{
				char c = *ptr2;
				if (c == '/' || c == '\\' || (notImplicitFile && (c == ':' || c == '?' || c == '#')))
				{
					ptr3 = ptr2;
					break;
				}
			}
			if (ptr3 == ptr)
			{
				return false;
			}
			while (true)
			{
				ptr2 = ptr;
				num = 0;
				bool flag = false;
				for (; ptr2 < ptr3 && *ptr2 != '.' && *ptr2 != '。' && *ptr2 != '．' && *ptr2 != '｡'; ptr2++)
				{
					num++;
					if (*ptr2 > 'ÿ')
					{
						num++;
					}
					if (*ptr2 >= '\u00a0')
					{
						flag = true;
					}
				}
				if (ptr != ptr2 && (flag ? (num + 4) : num) <= 63)
				{
					char* num2 = ptr;
					ptr = num2 + 1;
					if (*num2 >= '\u00a0' || IsASCIILetterOrDigit(*(ptr - 1), ref notCanonical))
					{
						while (ptr < ptr2)
						{
							char* num3 = ptr;
							ptr = num3 + 1;
							if (*num3 < '\u00a0' && !IsValidDomainLabelCharacter(*(ptr - 1), ref notCanonical))
							{
								return false;
							}
						}
						ptr++;
						if (ptr >= ptr3)
						{
							break;
						}
						continue;
					}
				}
				return false;
			}
			returnedEnd = (ushort)(ptr3 - name);
			return true;
		}

		private static bool IsASCIILetter(char character, ref bool notCanonical)
		{
			if (character >= 'a' && character <= 'z')
			{
				return true;
			}
			if (character >= 'A' && character <= 'Z')
			{
				if (!notCanonical)
				{
					notCanonical = true;
				}
				return true;
			}
			return false;
		}

		internal unsafe static string IdnEquivalent(char* hostname, int start, int end, ref bool allAscii, ref bool atLeastOneValidIdn)
		{
			string bidiStrippedHost = null;
			string text = IdnEquivalent(hostname, start, end, ref allAscii, ref bidiStrippedHost);
			if (text != null)
			{
				string text2 = (allAscii ? text : bidiStrippedHost);
				fixed (char* ptr = text2)
				{
					int length = text2.Length;
					int num = 0;
					int num2 = 0;
					bool flag = false;
					bool flag2 = false;
					bool flag3 = false;
					do
					{
						flag = false;
						flag2 = false;
						flag3 = false;
						num = num2;
						while (num < length)
						{
							char c = ptr[num];
							if (!flag2)
							{
								flag2 = true;
								if (num + 3 < length && IsIdnAce(ptr, num))
								{
									num += 4;
									flag = true;
									continue;
								}
							}
							if (c == '.' || c == '。' || c == '．' || c == '｡')
							{
								flag3 = true;
								break;
							}
							num++;
						}
						if (flag)
						{
							try
							{
								IdnMapping idnMapping = new IdnMapping();
								idnMapping.GetUnicode(new string(ptr, num2, num - num2));
								atLeastOneValidIdn = true;
							}
							catch (ArgumentException)
							{
								goto IL_00dc;
							}
							break;
						}
						goto IL_00dc;
						IL_00dc:
						num2 = num + (flag3 ? 1 : 0);
					}
					while (num2 < length);
				}
			}
			else
			{
				atLeastOneValidIdn = false;
			}
			return text;
		}

		internal unsafe static string IdnEquivalent(char* hostname, int start, int end, ref bool allAscii, ref string bidiStrippedHost)
		{
			string result = null;
			if (end <= start)
			{
				return result;
			}
			int i = start;
			allAscii = true;
			for (; i < end; i++)
			{
				if (hostname[i] > '\u007f')
				{
					allAscii = false;
					break;
				}
			}
			if (allAscii)
			{
				return new string(hostname, start, end - start)?.ToLowerInvariant();
			}
			IdnMapping idnMapping = new IdnMapping();
			bidiStrippedHost = Uri.StripBidiControlCharacter(hostname, start, end - start);
			try
			{
				string ascii = idnMapping.GetAscii(bidiStrippedHost);
				if (!ServicePointManager.AllowDangerousUnicodeDecompositions)
				{
					if (ContainsCharactersUnsafeForNormalizedHost(ascii))
					{
						throw new UriFormatException("net_uri_BadUnicodeHostForIdn");
					}
					return ascii;
				}
				return ascii;
			}
			catch (ArgumentException)
			{
				throw new UriFormatException(SR.GetString("net_uri_BadUnicodeHostForIdn"));
			}
		}

		private static bool IsIdnAce(string input, int index)
		{
			if (input[index] == 'x' && input[index + 1] == 'n' && input[index + 2] == '-' && input[index + 3] == '-')
			{
				return true;
			}
			return false;
		}

		private unsafe static bool IsIdnAce(char* input, int index)
		{
			if (input[index] == 'x' && input[index + 1] == 'n' && input[index + 2] == '-' && input[index + 3] == '-')
			{
				return true;
			}
			return false;
		}

		internal unsafe static string UnicodeEquivalent(string idnHost, char* hostname, int start, int end)
		{
			IdnMapping idnMapping = new IdnMapping();
			try
			{
				return idnMapping.GetUnicode(idnHost);
			}
			catch (ArgumentException)
			{
			}
			bool allAscii = true;
			return UnicodeEquivalent(hostname, start, end, ref allAscii, ref allAscii);
		}

		internal unsafe static string UnicodeEquivalent(char* hostname, int start, int end, ref bool allAscii, ref bool atLeastOneValidIdn)
		{
			IdnMapping idnMapping = new IdnMapping();
			allAscii = true;
			atLeastOneValidIdn = false;
			string result = null;
			if (end <= start)
			{
				return result;
			}
			string text = Uri.StripBidiControlCharacter(hostname, start, end - start);
			string text2 = null;
			int num = 0;
			int num2 = 0;
			int length = text.Length;
			bool flag = true;
			bool flag2 = false;
			bool flag3 = false;
			bool flag4 = false;
			do
			{
				flag = true;
				flag2 = false;
				flag3 = false;
				flag4 = false;
				for (num2 = num; num2 < length; num2++)
				{
					char c = text[num2];
					if (!flag3)
					{
						flag3 = true;
						if (num2 + 3 < length && c == 'x' && IsIdnAce(text, num2))
						{
							flag2 = true;
						}
					}
					if (flag && c > '\u007f')
					{
						flag = false;
						allAscii = false;
					}
					if (c == '.' || c == '。' || c == '．' || c == '｡')
					{
						flag4 = true;
						break;
					}
				}
				if (!flag)
				{
					string unicode = text.Substring(num, num2 - num);
					try
					{
						unicode = idnMapping.GetAscii(unicode);
					}
					catch (ArgumentException)
					{
						throw new UriFormatException(SR.GetString("net_uri_BadUnicodeHostForIdn"));
					}
					text2 += idnMapping.GetUnicode(unicode);
					if (flag4)
					{
						text2 += ".";
					}
				}
				else
				{
					bool flag5 = false;
					if (flag2)
					{
						try
						{
							text2 += idnMapping.GetUnicode(text.Substring(num, num2 - num));
							if (flag4)
							{
								text2 += ".";
							}
							flag5 = true;
							atLeastOneValidIdn = true;
						}
						catch (ArgumentException)
						{
						}
					}
					if (!flag5)
					{
						text2 += text.Substring(num, num2 - num).ToLowerInvariant();
						if (flag4)
						{
							text2 += ".";
						}
					}
				}
				num = num2 + (flag4 ? 1 : 0);
			}
			while (num < length);
			return text2;
		}

		private static bool IsASCIILetterOrDigit(char character, ref bool notCanonical)
		{
			if ((character >= 'a' && character <= 'z') || (character >= '0' && character <= '9'))
			{
				return true;
			}
			if (character >= 'A' && character <= 'Z')
			{
				notCanonical = true;
				return true;
			}
			return false;
		}

		private static bool IsValidDomainLabelCharacter(char character, ref bool notCanonical)
		{
			if ((character < 'a' || character > 'z') && (character < '0' || character > '9'))
			{
				switch (character)
				{
				case '-':
				case '_':
					break;
				case 'A':
				case 'B':
				case 'C':
				case 'D':
				case 'E':
				case 'F':
				case 'G':
				case 'H':
				case 'I':
				case 'J':
				case 'K':
				case 'L':
				case 'M':
				case 'N':
				case 'O':
				case 'P':
				case 'Q':
				case 'R':
				case 'S':
				case 'T':
				case 'U':
				case 'V':
				case 'W':
				case 'X':
				case 'Y':
				case 'Z':
					notCanonical = true;
					return true;
				default:
					return false;
				}
			}
			return true;
		}

		internal static bool ContainsCharactersUnsafeForNormalizedHost(string host)
		{
			return host.IndexOfAny(s_UnsafeForNormalizedHost) != -1;
		}
	}
}
