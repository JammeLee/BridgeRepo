using System.Globalization;
using System.Net.Mail;
using System.Text;

namespace System.Net.Mime
{
	internal static class MailBnfHelper
	{
		private static bool[] s_atext;

		private static bool[] s_qtext;

		private static bool[] s_fqtext;

		private static bool[] s_dtext;

		private static bool[] s_fdtext;

		private static bool[] s_ftext;

		private static bool[] s_ttext;

		private static bool[] s_digits;

		private static string[] s_months;

		private static string[] s_days;

		static MailBnfHelper()
		{
			s_atext = new bool[128];
			s_qtext = new bool[128];
			s_fqtext = new bool[128];
			s_dtext = new bool[128];
			s_fdtext = new bool[128];
			s_ftext = new bool[128];
			s_ttext = new bool[128];
			s_digits = new bool[128];
			s_months = new string[13]
			{
				null,
				"Jan",
				"Feb",
				"Mar",
				"Apr",
				"May",
				"Jun",
				"Jul",
				"Aug",
				"Sep",
				"Oct",
				"Nov",
				"Dec"
			};
			s_days = new string[7]
			{
				"Mon",
				"Tue",
				"Wed",
				"Thu",
				"Fri",
				"Sat",
				"Sun"
			};
			for (int i = 48; i <= 57; i++)
			{
				s_atext[i] = true;
			}
			for (int j = 65; j <= 90; j++)
			{
				s_atext[j] = true;
			}
			for (int k = 97; k <= 122; k++)
			{
				s_atext[k] = true;
			}
			s_atext[33] = true;
			s_atext[35] = true;
			s_atext[36] = true;
			s_atext[37] = true;
			s_atext[38] = true;
			s_atext[39] = true;
			s_atext[42] = true;
			s_atext[43] = true;
			s_atext[45] = true;
			s_atext[47] = true;
			s_atext[61] = true;
			s_atext[63] = true;
			s_atext[94] = true;
			s_atext[95] = true;
			s_atext[96] = true;
			s_atext[123] = true;
			s_atext[124] = true;
			s_atext[125] = true;
			s_atext[126] = true;
			for (int l = 1; l <= 8; l++)
			{
				s_qtext[l] = true;
			}
			s_qtext[11] = true;
			s_qtext[12] = true;
			for (int m = 14; m <= 31; m++)
			{
				s_qtext[m] = true;
			}
			s_qtext[33] = true;
			for (int n = 35; n <= 91; n++)
			{
				s_qtext[n] = true;
			}
			for (int num = 93; num <= 127; num++)
			{
				s_qtext[num] = true;
			}
			for (int num2 = 1; num2 <= 9; num2++)
			{
				s_fqtext[num2] = true;
			}
			s_fqtext[11] = true;
			s_fqtext[12] = true;
			for (int num3 = 14; num3 <= 33; num3++)
			{
				s_fqtext[num3] = true;
			}
			for (int num4 = 35; num4 <= 91; num4++)
			{
				s_fqtext[num4] = true;
			}
			for (int num5 = 93; num5 <= 127; num5++)
			{
				s_fqtext[num5] = true;
			}
			for (int num6 = 1; num6 <= 8; num6++)
			{
				s_dtext[num6] = true;
			}
			s_dtext[11] = true;
			s_dtext[12] = true;
			for (int num7 = 14; num7 <= 31; num7++)
			{
				s_dtext[num7] = true;
			}
			for (int num8 = 33; num8 <= 90; num8++)
			{
				s_dtext[num8] = true;
			}
			for (int num9 = 94; num9 <= 127; num9++)
			{
				s_dtext[num9] = true;
			}
			for (int num10 = 1; num10 <= 9; num10++)
			{
				s_fdtext[num10] = true;
			}
			s_fdtext[11] = true;
			s_fdtext[12] = true;
			for (int num11 = 14; num11 <= 90; num11++)
			{
				s_fdtext[num11] = true;
			}
			for (int num12 = 94; num12 <= 127; num12++)
			{
				s_fdtext[num12] = true;
			}
			for (int num13 = 33; num13 <= 57; num13++)
			{
				s_ftext[num13] = true;
			}
			for (int num14 = 59; num14 <= 126; num14++)
			{
				s_ftext[num14] = true;
			}
			for (int num15 = 33; num15 <= 126; num15++)
			{
				s_ttext[num15] = true;
			}
			s_ttext[40] = false;
			s_ttext[41] = false;
			s_ttext[60] = false;
			s_ttext[62] = false;
			s_ttext[64] = false;
			s_ttext[44] = false;
			s_ttext[59] = false;
			s_ttext[58] = false;
			s_ttext[92] = false;
			s_ttext[34] = false;
			s_ttext[47] = false;
			s_ttext[91] = false;
			s_ttext[93] = false;
			s_ttext[63] = false;
			s_ttext[61] = false;
			for (int num16 = 48; num16 <= 57; num16++)
			{
				s_digits[num16] = true;
			}
		}

		internal static bool SkipCFWS(string data, ref int offset)
		{
			int num = 0;
			while (offset < data.Length)
			{
				if (data[offset] > '\u007f')
				{
					throw new FormatException(SR.GetString("MailHeaderFieldInvalidCharacter"));
				}
				if (data[offset] == '\\' && num > 0)
				{
					offset += 2;
				}
				else if (data[offset] == '(')
				{
					num++;
				}
				else if (data[offset] == ')')
				{
					num--;
				}
				else if (data[offset] != ' ' && data[offset] != '\t' && num == 0)
				{
					return true;
				}
				if (num < 0)
				{
					throw new FormatException(SR.GetString("MailHeaderFieldInvalidCharacter"));
				}
				offset++;
			}
			return false;
		}

		internal static bool SkipFWS(string data, ref int offset)
		{
			while (offset < data.Length)
			{
				if (data[offset] != ' ' && data[offset] != '\t')
				{
					return true;
				}
				offset++;
			}
			return false;
		}

		internal static void ValidateHeaderName(string data)
		{
			int i;
			for (i = 0; i < data.Length; i++)
			{
				if (data[i] > s_ftext.Length || !s_ftext[data[i]])
				{
					throw new FormatException(SR.GetString("InvalidHeaderName"));
				}
			}
			if (i == 0)
			{
				throw new FormatException(SR.GetString("InvalidHeaderName"));
			}
		}

		internal static string ReadQuotedString(string data, ref int offset, StringBuilder builder)
		{
			bool containsUnescapedUnicode = false;
			string result = ReadQuotedString(data, ref offset, builder, returnQuotes: false, ref containsUnescapedUnicode);
			ThrowForUnescapedUnicode(containsUnescapedUnicode);
			return result;
		}

		internal static string ReadQuotedString(string data, ref int offset, StringBuilder builder, bool returnQuotes, ref bool containsUnescapedUnicode)
		{
			if (data[offset] != '"')
			{
				throw new FormatException(SR.GetString("MailHeaderFieldInvalidCharacter"));
			}
			offset++;
			int num = offset;
			StringBuilder stringBuilder = ((builder != null) ? builder : new StringBuilder());
			if (returnQuotes)
			{
				stringBuilder.Append('"');
			}
			while (offset < data.Length)
			{
				if (data[offset] == '\\')
				{
					stringBuilder.Append(data, num, offset - num);
					num = ++offset;
				}
				else
				{
					if (data[offset] == '"')
					{
						stringBuilder.Append(data, num, offset - num);
						offset++;
						if (returnQuotes)
						{
							stringBuilder.Append('"');
						}
						if (builder == null)
						{
							return stringBuilder.ToString();
						}
						return null;
					}
					if (data[offset] >= s_fqtext.Length)
					{
						containsUnescapedUnicode = true;
					}
					else if (!s_fqtext[data[offset]])
					{
						throw new FormatException(SR.GetString("MailHeaderFieldInvalidCharacter"));
					}
				}
				offset++;
			}
			throw new FormatException(SR.GetString("MailHeaderFieldMalformedHeader"));
		}

		internal static string ReadUnQuotedString(string data, ref int offset, StringBuilder builder)
		{
			int num = offset;
			StringBuilder stringBuilder = ((builder != null) ? builder : new StringBuilder());
			while (offset < data.Length)
			{
				if (data[offset] == '\\')
				{
					stringBuilder.Append(data, num, offset - num);
					num = ++offset;
				}
				else
				{
					if (data[offset] == '"')
					{
						throw new FormatException(SR.GetString("MailHeaderFieldInvalidCharacter"));
					}
					if (!s_fqtext[data[offset]])
					{
						throw new FormatException(SR.GetString("MailHeaderFieldInvalidCharacter"));
					}
				}
				offset++;
			}
			stringBuilder.Append(data, num, offset - num);
			if (builder == null)
			{
				return stringBuilder.ToString();
			}
			return null;
		}

		internal static string ReadPhrase(string data, ref int offset, StringBuilder builder, ref bool containsUnescapedUnicode)
		{
			StringBuilder stringBuilder = ((builder != null) ? builder : new StringBuilder());
			bool flag = false;
			SkipCFWS(data, ref offset);
			int num = offset;
			if (SkipCFWS(data, ref offset) && data[offset] == '"')
			{
				string text = ReadQuotedString(data, ref offset, null, returnQuotes: true, ref containsUnescapedUnicode);
				if (!SkipCFWS(data, ref offset) || (data[offset] != '"' && !s_atext[data[offset]] && data[offset] != '.'))
				{
					if (builder != null)
					{
						builder.Append(text);
						return null;
					}
					return text;
				}
				offset = num;
			}
			while (SkipCFWS(data, ref offset))
			{
				if (data[offset] == '"')
				{
					if (flag)
					{
						stringBuilder.Append(' ');
					}
					ReadQuotedString(data, ref offset, stringBuilder, returnQuotes: false, ref containsUnescapedUnicode);
					flag = true;
					continue;
				}
				if (!s_atext[data[offset]])
				{
					break;
				}
				if (flag)
				{
					stringBuilder.Append(' ');
				}
				ReadAtom(data, ref offset, stringBuilder);
				flag = true;
			}
			if (num == offset)
			{
				throw new FormatException(SR.GetString("MailHeaderFieldInvalidCharacter"));
			}
			if (builder == null)
			{
				return stringBuilder.ToString();
			}
			return null;
		}

		internal static string ReadAtom(string data, ref int offset, StringBuilder builder)
		{
			int num = offset;
			string text;
			while (offset < data.Length)
			{
				if (data[offset] > s_atext.Length)
				{
					throw new FormatException(SR.GetString("MailHeaderFieldInvalidCharacter"));
				}
				if (!s_atext[data[offset]])
				{
					if (offset == num)
					{
						throw new FormatException(SR.GetString("MailHeaderFieldInvalidCharacter"));
					}
					text = data.Substring(num, offset - num);
					if (builder != null)
					{
						builder.Append(text);
						return null;
					}
					return text;
				}
				offset++;
			}
			if (offset == num)
			{
				throw new FormatException(SR.GetString("MailHeaderFieldInvalidCharacter"));
			}
			text = ((num == 0) ? data : data.Substring(num));
			if (builder != null)
			{
				builder.Append(text);
				return null;
			}
			return text;
		}

		internal static string ReadDotAtom(string data, ref int offset, StringBuilder builder)
		{
			bool flag = true;
			if (builder == null)
			{
				flag = false;
				builder = new StringBuilder();
			}
			if (data[offset] != '.')
			{
				ReadAtom(data, ref offset, builder);
			}
			while (offset < data.Length && data[offset] == '.')
			{
				builder.Append(data[offset++]);
				ReadAtom(data, ref offset, builder);
			}
			if (flag)
			{
				return null;
			}
			return builder.ToString();
		}

		internal static string ReadDomainLiteral(string data, ref int offset, StringBuilder builder)
		{
			int num = ++offset;
			StringBuilder stringBuilder = ((builder != null) ? builder : new StringBuilder());
			while (offset < data.Length)
			{
				if (data[offset] == '\\')
				{
					stringBuilder.Append(data, num, offset - num);
					num = ++offset;
				}
				else
				{
					if (data[offset] == ']')
					{
						stringBuilder.Append(data, num, offset - num);
						offset++;
						if (builder == null)
						{
							return stringBuilder.ToString();
						}
						return null;
					}
					if (data[offset] > s_fdtext.Length || !s_fdtext[data[offset]])
					{
						throw new FormatException(SR.GetString("MailHeaderFieldInvalidCharacter"));
					}
				}
				offset++;
			}
			throw new FormatException(SR.GetString("MailHeaderFieldMalformedHeader"));
		}

		internal static string ReadParameterAttribute(string data, ref int offset, StringBuilder builder)
		{
			if (!SkipCFWS(data, ref offset))
			{
				return null;
			}
			return ReadToken(data, ref offset, null);
		}

		internal static string ReadToken(string data, ref int offset, StringBuilder builder)
		{
			int num = offset;
			while (offset < data.Length)
			{
				if (data[offset] > s_ttext.Length)
				{
					throw new FormatException(SR.GetString("MailHeaderFieldInvalidCharacter"));
				}
				if (!s_ttext[data[offset]])
				{
					break;
				}
				offset++;
			}
			if (num == offset)
			{
				throw new FormatException(SR.GetString("MailHeaderFieldInvalidCharacter"));
			}
			return data.Substring(num, offset - num);
		}

		internal static string ReadAngleAddress(string data, ref int offset, StringBuilder builder)
		{
			if (offset >= data.Length)
			{
				throw new FormatException(SR.GetString("MailAddressInvalidFormat"));
			}
			SkipCFWS(data, ref offset);
			if (data[offset] == '"')
			{
				bool containsUnescapedUnicode = false;
				ReadQuotedString(data, ref offset, builder, returnQuotes: true, ref containsUnescapedUnicode);
				ThrowForUnescapedUnicode(containsUnescapedUnicode);
			}
			else
			{
				ReadDotAtom(data, ref offset, builder);
			}
			SkipCFWS(data, ref offset);
			if (offset >= data.Length || data[offset] != '@')
			{
				throw new FormatException(SR.GetString("MailAddressInvalidFormat"));
			}
			offset++;
			SkipCFWS(data, ref offset);
			string result = ReadAddressSpecDomain(data, ref offset, builder);
			if (!SkipCFWS(data, ref offset) || data[offset++] != '>')
			{
				throw new FormatException(SR.GetString("MailAddressInvalidFormat"));
			}
			return result;
		}

		internal static string ReadAddressSpecDomain(string data, ref int offset, StringBuilder builder)
		{
			if (offset >= data.Length)
			{
				throw new FormatException(SR.GetString("MailAddressInvalidFormat"));
			}
			builder.Append('@');
			SkipCFWS(data, ref offset);
			if (data[offset] == '[')
			{
				ReadDomainLiteral(data, ref offset, builder);
			}
			else
			{
				ReadDotAtom(data, ref offset, builder);
			}
			SkipCFWS(data, ref offset);
			return builder.ToString();
		}

		internal static void ThrowForUnescapedUnicode(bool unescapedUnicode)
		{
			if (unescapedUnicode)
			{
				throw new FormatException(SR.GetString("MailHeaderFieldInvalidCharacter"));
			}
		}

		internal static string ReadMailAddress(string data, ref int offset, out string displayName)
		{
			string result = null;
			Exception ex = null;
			displayName = string.Empty;
			StringBuilder stringBuilder = new StringBuilder();
			try
			{
				SkipCFWS(data, ref offset);
				if (offset >= data.Length)
				{
					ex = new FormatException(SR.GetString("MailAddressInvalidFormat"));
				}
				else
				{
					if (data[offset] == '<')
					{
						offset++;
						return ReadAngleAddress(data, ref offset, stringBuilder);
					}
					bool containsUnescapedUnicode = false;
					ReadPhrase(data, ref offset, stringBuilder, ref containsUnescapedUnicode);
					if (offset >= data.Length)
					{
						ex = new FormatException(SR.GetString("MailAddressInvalidFormat"));
					}
					else
					{
						switch (data[offset])
						{
						case '@':
							ThrowForUnescapedUnicode(containsUnescapedUnicode);
							offset++;
							result = ReadAddressSpecDomain(data, ref offset, stringBuilder);
							break;
						case '.':
							ReadDotAtom(data, ref offset, stringBuilder);
							SkipCFWS(data, ref offset);
							if (offset >= data.Length)
							{
								ex = new FormatException(SR.GetString("MailAddressInvalidFormat"));
							}
							else
							{
								if (data[offset] == '@')
								{
									ThrowForUnescapedUnicode(containsUnescapedUnicode);
									offset++;
									result = ReadAddressSpecDomain(data, ref offset, stringBuilder);
									break;
								}
								if (data[offset] == '<')
								{
									displayName = stringBuilder.ToString();
									stringBuilder = new StringBuilder();
									offset++;
									result = ReadAngleAddress(data, ref offset, stringBuilder);
									break;
								}
								ex = new FormatException(SR.GetString("MailAddressInvalidFormat"));
							}
							goto end_IL_0011;
						case '"':
							offset++;
							if (offset >= data.Length)
							{
								ex = new FormatException(SR.GetString("MailAddressInvalidFormat"));
							}
							else
							{
								SkipCFWS(data, ref offset);
								if (offset < data.Length)
								{
									if (data[offset] == '<')
									{
										offset++;
										result = ReadAngleAddress(data, ref offset, stringBuilder);
									}
									else
									{
										result = ReadAddressSpecDomain(data, ref offset, stringBuilder);
									}
									break;
								}
								ex = new FormatException(SR.GetString("MailAddressInvalidFormat"));
							}
							goto end_IL_0011;
						case '<':
							displayName = stringBuilder.ToString();
							stringBuilder = new StringBuilder();
							offset++;
							result = ReadAngleAddress(data, ref offset, stringBuilder);
							break;
						case ':':
							ex = new FormatException(SR.GetString("MailAddressUnsupportedFormat"));
							goto end_IL_0011;
						default:
							ex = new FormatException(SR.GetString("MailAddressInvalidFormat"));
							goto end_IL_0011;
						}
						if (offset < data.Length)
						{
							SkipCFWS(data, ref offset);
							if (offset < data.Length && data[offset] != ',')
							{
								ex = new FormatException(SR.GetString("MailAddressInvalidFormat"));
							}
						}
					}
				}
				end_IL_0011:;
			}
			catch (FormatException)
			{
				throw new FormatException(SR.GetString("MailAddressInvalidFormat"));
			}
			if (ex != null)
			{
				throw ex;
			}
			return result;
		}

		internal static MailAddress ReadMailAddress(string data, ref int offset)
		{
			string displayName = null;
			string address = ReadMailAddress(data, ref offset, out displayName);
			return new MailAddress(address, displayName, 0u);
		}

		internal static DateTime ReadDateTime(string data, ref int offset)
		{
			if (!SkipCFWS(data, ref offset))
			{
				throw new FormatException(SR.GetString("MailDateInvalidFormat"));
			}
			if (IsValidDOW(data, ref offset))
			{
				if (offset >= data.Length || data[offset] != ',')
				{
					throw new FormatException(SR.GetString("MailDateInvalidFormat"));
				}
				offset++;
			}
			if (!SkipFWS(data, ref offset))
			{
				throw new FormatException(SR.GetString("MailDateInvalidFormat"));
			}
			int day = ReadDateNumber(data, ref offset, 2);
			if (offset >= data.Length || (data[offset] != ' ' && data[offset] != '\t'))
			{
				throw new FormatException(SR.GetString("MailDateInvalidFormat"));
			}
			if (!SkipFWS(data, ref offset))
			{
				throw new FormatException(SR.GetString("MailDateInvalidFormat"));
			}
			int month = ReadMonth(data, ref offset);
			if (offset >= data.Length || (data[offset] != ' ' && data[offset] != '\t'))
			{
				throw new FormatException(SR.GetString("MailDateInvalidFormat"));
			}
			if (!SkipFWS(data, ref offset))
			{
				throw new FormatException(SR.GetString("MailDateInvalidFormat"));
			}
			int year = ReadDateNumber(data, ref offset, 4);
			if (offset >= data.Length || (data[offset] != ' ' && data[offset] != '\t'))
			{
				throw new FormatException(SR.GetString("MailDateInvalidFormat"));
			}
			if (!SkipFWS(data, ref offset))
			{
				throw new FormatException(SR.GetString("MailDateInvalidFormat"));
			}
			int hour = ReadDateNumber(data, ref offset, 2);
			if (offset >= data.Length || data[offset] != ':')
			{
				throw new FormatException(SR.GetString("MailHeaderFieldInvalidCharacter"));
			}
			offset++;
			int minute = ReadDateNumber(data, ref offset, 2);
			int second = 0;
			if (offset < data.Length && data[offset] == ':')
			{
				offset++;
				second = ReadDateNumber(data, ref offset, 2);
			}
			if (!SkipFWS(data, ref offset))
			{
				throw new FormatException(SR.GetString("MailDateInvalidFormat"));
			}
			if (offset >= data.Length || (data[offset] != '-' && data[offset] != '+'))
			{
				throw new FormatException(SR.GetString("MailDateInvalidFormat"));
			}
			offset++;
			ReadDateNumber(data, ref offset, 4);
			return new DateTime(year, month, day, hour, minute, second);
		}

		private static bool IsValidDOW(string data, ref int offset)
		{
			if (offset + 3 >= data.Length)
			{
				throw new FormatException(SR.GetString("MailDateInvalidFormat"));
			}
			for (int i = 0; i < s_days.Length; i++)
			{
				if (string.Compare(s_days[i], 0, data, offset, 3, ignoreCase: true, CultureInfo.InvariantCulture) == 0)
				{
					offset += 3;
					return true;
				}
			}
			return false;
		}

		private static int ReadDateNumber(string data, ref int offset, int maxSize)
		{
			int num = 0;
			int num2 = offset + maxSize;
			if (offset >= data.Length)
			{
				throw new FormatException(SR.GetString("MailDateInvalidFormat"));
			}
			while (offset < data.Length && offset < num2 && data[offset] >= '0' && data[offset] <= '9')
			{
				num = num * 10 + (data[offset] - 48);
				offset++;
			}
			if (num == 0)
			{
				throw new FormatException(SR.GetString("MailDateInvalidFormat"));
			}
			return num;
		}

		private static int ReadMonth(string data, ref int offset)
		{
			if (offset >= data.Length - 3)
			{
				throw new FormatException(SR.GetString("MailDateInvalidFormat"));
			}
			switch (data[offset++])
			{
			case 'J':
			case 'j':
				switch (data[offset++])
				{
				case 'A':
				case 'a':
				{
					char c13 = data[offset++];
					if (c13 == 'N' || c13 == 'n')
					{
						return 1;
					}
					break;
				}
				case 'U':
				case 'u':
					switch (data[offset++])
					{
					case 'N':
					case 'n':
						return 6;
					case 'L':
					case 'l':
						return 7;
					}
					break;
				}
				break;
			case 'F':
			case 'f':
			{
				char c3 = data[offset++];
				if (c3 == 'E' || c3 == 'e')
				{
					char c4 = data[offset++];
					if (c4 == 'B' || c4 == 'b')
					{
						return 2;
					}
				}
				break;
			}
			case 'M':
			case 'm':
			{
				char c14 = data[offset++];
				if (c14 == 'A' || c14 == 'a')
				{
					switch (data[offset++])
					{
					case 'Y':
					case 'y':
						return 5;
					case 'R':
					case 'r':
						return 3;
					}
				}
				break;
			}
			case 'A':
			case 'a':
				switch (data[offset++])
				{
				case 'P':
				case 'p':
				{
					char c8 = data[offset++];
					if (c8 == 'R' || c8 == 'r')
					{
						return 4;
					}
					break;
				}
				case 'U':
				case 'u':
				{
					char c7 = data[offset++];
					if (c7 == 'G' || c7 == 'g')
					{
						return 8;
					}
					break;
				}
				}
				break;
			case 'S':
			case 's':
			{
				char c5 = data[offset++];
				if (c5 == 'E' || c5 == 'e')
				{
					char c6 = data[offset++];
					if (c6 == 'P' || c6 == 'p')
					{
						return 9;
					}
				}
				break;
			}
			case 'O':
			case 'o':
			{
				char c9 = data[offset++];
				if (c9 == 'C' || c9 == 'c')
				{
					char c10 = data[offset++];
					if (c10 == 'T' || c10 == 't')
					{
						return 10;
					}
				}
				break;
			}
			case 'N':
			case 'n':
			{
				char c11 = data[offset++];
				if (c11 == 'O' || c11 == 'o')
				{
					char c12 = data[offset++];
					if (c12 == 'V' || c12 == 'v')
					{
						return 11;
					}
				}
				break;
			}
			case 'D':
			case 'd':
			{
				char c = data[offset++];
				if (c == 'E' || c == 'e')
				{
					char c2 = data[offset++];
					if (c2 == 'C' || c2 == 'c')
					{
						return 12;
					}
				}
				break;
			}
			}
			throw new FormatException(SR.GetString("MailDateInvalidFormat"));
		}

		internal static string GetDateTimeString(DateTime value, StringBuilder builder)
		{
			StringBuilder stringBuilder = ((builder != null) ? builder : new StringBuilder());
			stringBuilder.Append(value.Day);
			stringBuilder.Append(' ');
			stringBuilder.Append(s_months[value.Month]);
			stringBuilder.Append(' ');
			stringBuilder.Append(value.Year);
			stringBuilder.Append(' ');
			if (value.Hour <= 9)
			{
				stringBuilder.Append('0');
			}
			stringBuilder.Append(value.Hour);
			stringBuilder.Append(':');
			if (value.Minute <= 9)
			{
				stringBuilder.Append('0');
			}
			stringBuilder.Append(value.Minute);
			stringBuilder.Append(':');
			if (value.Second <= 9)
			{
				stringBuilder.Append('0');
			}
			stringBuilder.Append(value.Second);
			string text = TimeZone.CurrentTimeZone.GetUtcOffset(value).ToString();
			if (text[0] != '-')
			{
				stringBuilder.Append(" +");
			}
			else
			{
				stringBuilder.Append(" ");
			}
			string[] array = text.Split(':');
			stringBuilder.Append(array[0]);
			stringBuilder.Append(array[1]);
			if (builder == null)
			{
				return stringBuilder.ToString();
			}
			return null;
		}

		internal static string GetTokenOrQuotedString(string data, StringBuilder builder)
		{
			int i = 0;
			int num = 0;
			for (; i < data.Length; i++)
			{
				if (data[i] > s_ttext.Length)
				{
					throw new FormatException(SR.GetString("MailHeaderFieldInvalidCharacter"));
				}
				if (s_ttext[data[i]] && data[i] != ' ')
				{
					continue;
				}
				StringBuilder stringBuilder = ((builder != null) ? builder : new StringBuilder());
				builder.Append('"');
				for (; i < data.Length; i++)
				{
					if (data[i] > s_fqtext.Length)
					{
						throw new FormatException(SR.GetString("MailHeaderFieldInvalidCharacter"));
					}
					if (!s_fqtext[data[i]])
					{
						builder.Append(data, num, i - num);
						builder.Append('\\');
						num = i;
					}
				}
				builder.Append(data, num, i - num);
				builder.Append('"');
				if (builder == null)
				{
					return stringBuilder.ToString();
				}
				return null;
			}
			if (data.Length == 0)
			{
				if (builder == null)
				{
					return "\"\"";
				}
				builder.Append("\"\"");
			}
			if (builder != null)
			{
				builder.Append(data);
				return null;
			}
			return data;
		}

		internal static string GetDotAtomOrQuotedString(string data, StringBuilder builder)
		{
			bool flag = data.StartsWith("\"") && data.EndsWith("\"");
			int i = (flag ? 1 : 0);
			int num = 0;
			for (; i < data.Length; i++)
			{
				if (data[i] > s_atext.Length)
				{
					throw new FormatException(SR.GetString("MailHeaderFieldInvalidCharacter"));
				}
				if ((data[i] == '.' || s_atext[data[i]]) && data[i] != ' ')
				{
					continue;
				}
				StringBuilder stringBuilder = ((builder != null) ? builder : new StringBuilder());
				if (!flag)
				{
					builder.Append('"');
				}
				for (; (!flag && i < data.Length) || (flag && i < data.Length - 1); i++)
				{
					if (data[i] > s_fqtext.Length)
					{
						throw new FormatException(SR.GetString("MailHeaderFieldInvalidCharacter"));
					}
					if (!s_fqtext[data[i]])
					{
						builder.Append(data, num, i - num);
						builder.Append('\\');
						num = i;
					}
				}
				builder.Append(data, num, i - num);
				builder.Append('"');
				if (builder == null)
				{
					return stringBuilder.ToString();
				}
				return null;
			}
			if (builder != null)
			{
				builder.Append(data);
				return null;
			}
			return data;
		}

		internal static string GetDotAtomOrDomainLiteral(string data, StringBuilder builder)
		{
			int i = 0;
			int num = 0;
			for (; i < data.Length; i++)
			{
				if (data[i] > s_atext.Length)
				{
					throw new FormatException(SR.GetString("MailHeaderFieldInvalidCharacter"));
				}
				if (data[i] == '.' || s_atext[data[i]])
				{
					continue;
				}
				StringBuilder stringBuilder = ((builder != null) ? builder : new StringBuilder());
				builder.Append('[');
				for (; i < data.Length; i++)
				{
					if (data[i] > s_fdtext.Length)
					{
						throw new FormatException(SR.GetString("MailHeaderFieldInvalidCharacter"));
					}
					if (!s_fdtext[data[i]])
					{
						builder.Append(data, num, i - num);
						builder.Append('\\');
						num = i;
					}
				}
				builder.Append(data, num, i - num);
				builder.Append(']');
				if (builder == null)
				{
					return stringBuilder.ToString();
				}
				return null;
			}
			if (builder != null)
			{
				builder.Append(data);
				return null;
			}
			return data;
		}

		internal static bool HasCROrLF(string data)
		{
			for (int i = 0; i < data.Length; i++)
			{
				if (data[i] == '\r' || data[i] == '\n')
				{
					return true;
				}
			}
			return false;
		}
	}
}
