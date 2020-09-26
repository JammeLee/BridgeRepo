using System.Globalization;
using System.Text;

namespace System
{
	internal static class DateTimeParse
	{
		internal delegate bool MatchNumberDelegate(ref __DTString str, int digitLen, out int result);

		internal enum DTT
		{
			End,
			NumEnd,
			NumAmpm,
			NumSpace,
			NumDatesep,
			NumTimesep,
			MonthEnd,
			MonthSpace,
			MonthDatesep,
			NumDatesuff,
			NumTimesuff,
			DayOfWeek,
			YearSpace,
			YearDateSep,
			YearEnd,
			TimeZone,
			Era,
			NumUTCTimeMark,
			Unk,
			NumLocalTimeMark,
			Max
		}

		internal enum TM
		{
			NotSet = -1,
			AM,
			PM
		}

		internal enum DS
		{
			BEGIN,
			N,
			NN,
			D_Nd,
			D_NN,
			D_NNd,
			D_M,
			D_MN,
			D_NM,
			D_MNd,
			D_NDS,
			D_Y,
			D_YN,
			D_YNd,
			D_YM,
			D_YMd,
			D_S,
			T_S,
			T_Nt,
			T_NNt,
			ERROR,
			DX_NN,
			DX_NNN,
			DX_MN,
			DX_NM,
			DX_MNN,
			DX_DS,
			DX_DSN,
			DX_NDS,
			DX_NNDS,
			DX_YNN,
			DX_YMN,
			DX_YN,
			DX_YM,
			TX_N,
			TX_NN,
			TX_NNN,
			TX_TS,
			DX_NNY
		}

		internal const int MaxDateTimeNumberDigits = 8;

		internal const string GMTName = "GMT";

		internal const string ZuluName = "Z";

		private const int ORDER_YMD = 0;

		private const int ORDER_MDY = 1;

		private const int ORDER_DMY = 2;

		private const int ORDER_YDM = 3;

		private const int ORDER_YM = 4;

		private const int ORDER_MY = 5;

		private const int ORDER_MD = 6;

		private const int ORDER_DM = 7;

		internal static MatchNumberDelegate m_hebrewNumberParser = MatchHebrewDigits;

		private static DS[][] dateParsingStates = new DS[20][]
		{
			new DS[18]
			{
				DS.BEGIN,
				DS.ERROR,
				DS.TX_N,
				DS.N,
				DS.D_Nd,
				DS.T_Nt,
				DS.ERROR,
				DS.D_M,
				DS.D_M,
				DS.D_S,
				DS.T_S,
				DS.BEGIN,
				DS.D_Y,
				DS.D_Y,
				DS.ERROR,
				DS.BEGIN,
				DS.BEGIN,
				DS.ERROR
			},
			new DS[18]
			{
				DS.ERROR,
				DS.DX_NN,
				DS.ERROR,
				DS.NN,
				DS.D_NNd,
				DS.ERROR,
				DS.DX_NM,
				DS.D_NM,
				DS.D_MNd,
				DS.D_NDS,
				DS.ERROR,
				DS.N,
				DS.D_YN,
				DS.D_YNd,
				DS.DX_YN,
				DS.N,
				DS.N,
				DS.ERROR
			},
			new DS[18]
			{
				DS.DX_NN,
				DS.DX_NNN,
				DS.TX_N,
				DS.DX_NNN,
				DS.ERROR,
				DS.T_Nt,
				DS.DX_MNN,
				DS.DX_MNN,
				DS.ERROR,
				DS.ERROR,
				DS.T_S,
				DS.NN,
				DS.DX_NNY,
				DS.ERROR,
				DS.DX_NNY,
				DS.NN,
				DS.NN,
				DS.ERROR
			},
			new DS[18]
			{
				DS.ERROR,
				DS.DX_NN,
				DS.ERROR,
				DS.D_NN,
				DS.D_NNd,
				DS.ERROR,
				DS.DX_NM,
				DS.D_MN,
				DS.D_MNd,
				DS.ERROR,
				DS.ERROR,
				DS.D_Nd,
				DS.D_YN,
				DS.D_YNd,
				DS.DX_YN,
				DS.ERROR,
				DS.D_Nd,
				DS.ERROR
			},
			new DS[18]
			{
				DS.DX_NN,
				DS.DX_NNN,
				DS.TX_N,
				DS.DX_NNN,
				DS.ERROR,
				DS.T_Nt,
				DS.DX_MNN,
				DS.DX_MNN,
				DS.ERROR,
				DS.DX_DS,
				DS.T_S,
				DS.D_NN,
				DS.DX_NNY,
				DS.ERROR,
				DS.DX_NNY,
				DS.ERROR,
				DS.D_NN,
				DS.ERROR
			},
			new DS[18]
			{
				DS.ERROR,
				DS.DX_NNN,
				DS.DX_NNN,
				DS.DX_NNN,
				DS.ERROR,
				DS.ERROR,
				DS.DX_MNN,
				DS.DX_MNN,
				DS.ERROR,
				DS.DX_DS,
				DS.ERROR,
				DS.D_NNd,
				DS.DX_NNY,
				DS.ERROR,
				DS.DX_NNY,
				DS.ERROR,
				DS.D_NNd,
				DS.ERROR
			},
			new DS[18]
			{
				DS.ERROR,
				DS.DX_MN,
				DS.ERROR,
				DS.D_MN,
				DS.D_MNd,
				DS.ERROR,
				DS.ERROR,
				DS.ERROR,
				DS.ERROR,
				DS.ERROR,
				DS.ERROR,
				DS.D_M,
				DS.D_YM,
				DS.D_YMd,
				DS.DX_YM,
				DS.ERROR,
				DS.D_M,
				DS.ERROR
			},
			new DS[18]
			{
				DS.DX_MN,
				DS.DX_MNN,
				DS.DX_MNN,
				DS.DX_MNN,
				DS.ERROR,
				DS.T_Nt,
				DS.ERROR,
				DS.ERROR,
				DS.ERROR,
				DS.DX_DS,
				DS.T_S,
				DS.D_MN,
				DS.DX_YMN,
				DS.ERROR,
				DS.DX_YMN,
				DS.ERROR,
				DS.D_MN,
				DS.ERROR
			},
			new DS[18]
			{
				DS.DX_NM,
				DS.DX_MNN,
				DS.DX_MNN,
				DS.DX_MNN,
				DS.ERROR,
				DS.T_Nt,
				DS.ERROR,
				DS.ERROR,
				DS.ERROR,
				DS.DX_DS,
				DS.T_S,
				DS.D_NM,
				DS.DX_YMN,
				DS.ERROR,
				DS.DX_YMN,
				DS.ERROR,
				DS.D_NM,
				DS.ERROR
			},
			new DS[18]
			{
				DS.ERROR,
				DS.DX_MNN,
				DS.ERROR,
				DS.DX_MNN,
				DS.ERROR,
				DS.ERROR,
				DS.ERROR,
				DS.ERROR,
				DS.ERROR,
				DS.ERROR,
				DS.ERROR,
				DS.D_MNd,
				DS.DX_YMN,
				DS.ERROR,
				DS.DX_YMN,
				DS.ERROR,
				DS.D_MNd,
				DS.ERROR
			},
			new DS[18]
			{
				DS.DX_NDS,
				DS.DX_NNDS,
				DS.DX_NNDS,
				DS.DX_NNDS,
				DS.ERROR,
				DS.T_Nt,
				DS.ERROR,
				DS.ERROR,
				DS.ERROR,
				DS.D_NDS,
				DS.T_S,
				DS.D_NDS,
				DS.ERROR,
				DS.ERROR,
				DS.ERROR,
				DS.ERROR,
				DS.D_NDS,
				DS.ERROR
			},
			new DS[18]
			{
				DS.ERROR,
				DS.DX_YN,
				DS.ERROR,
				DS.D_YN,
				DS.D_YNd,
				DS.ERROR,
				DS.DX_YM,
				DS.D_YM,
				DS.D_YMd,
				DS.D_YM,
				DS.ERROR,
				DS.D_Y,
				DS.ERROR,
				DS.ERROR,
				DS.ERROR,
				DS.ERROR,
				DS.D_Y,
				DS.ERROR
			},
			new DS[18]
			{
				DS.DX_YN,
				DS.DX_YNN,
				DS.DX_YNN,
				DS.DX_YNN,
				DS.ERROR,
				DS.ERROR,
				DS.DX_YMN,
				DS.DX_YMN,
				DS.ERROR,
				DS.ERROR,
				DS.ERROR,
				DS.D_YN,
				DS.ERROR,
				DS.ERROR,
				DS.ERROR,
				DS.ERROR,
				DS.D_YN,
				DS.ERROR
			},
			new DS[18]
			{
				DS.ERROR,
				DS.DX_YNN,
				DS.DX_YNN,
				DS.DX_YNN,
				DS.ERROR,
				DS.ERROR,
				DS.DX_YMN,
				DS.DX_YMN,
				DS.ERROR,
				DS.ERROR,
				DS.ERROR,
				DS.D_YN,
				DS.ERROR,
				DS.ERROR,
				DS.ERROR,
				DS.ERROR,
				DS.D_YN,
				DS.ERROR
			},
			new DS[18]
			{
				DS.DX_YM,
				DS.DX_YMN,
				DS.DX_YMN,
				DS.DX_YMN,
				DS.ERROR,
				DS.ERROR,
				DS.ERROR,
				DS.ERROR,
				DS.ERROR,
				DS.ERROR,
				DS.ERROR,
				DS.D_YM,
				DS.ERROR,
				DS.ERROR,
				DS.ERROR,
				DS.ERROR,
				DS.D_YM,
				DS.ERROR
			},
			new DS[18]
			{
				DS.ERROR,
				DS.DX_YMN,
				DS.DX_YMN,
				DS.DX_YMN,
				DS.ERROR,
				DS.ERROR,
				DS.ERROR,
				DS.ERROR,
				DS.ERROR,
				DS.ERROR,
				DS.ERROR,
				DS.D_YM,
				DS.ERROR,
				DS.ERROR,
				DS.ERROR,
				DS.ERROR,
				DS.D_YM,
				DS.ERROR
			},
			new DS[18]
			{
				DS.DX_DS,
				DS.DX_DSN,
				DS.TX_N,
				DS.T_Nt,
				DS.ERROR,
				DS.T_Nt,
				DS.ERROR,
				DS.ERROR,
				DS.ERROR,
				DS.D_S,
				DS.T_S,
				DS.D_S,
				DS.ERROR,
				DS.ERROR,
				DS.ERROR,
				DS.ERROR,
				DS.D_S,
				DS.ERROR
			},
			new DS[18]
			{
				DS.TX_TS,
				DS.TX_TS,
				DS.TX_TS,
				DS.T_Nt,
				DS.D_Nd,
				DS.ERROR,
				DS.ERROR,
				DS.ERROR,
				DS.ERROR,
				DS.D_S,
				DS.T_S,
				DS.T_S,
				DS.ERROR,
				DS.ERROR,
				DS.ERROR,
				DS.T_S,
				DS.T_S,
				DS.ERROR
			},
			new DS[18]
			{
				DS.ERROR,
				DS.TX_NN,
				DS.TX_NN,
				DS.TX_NN,
				DS.ERROR,
				DS.T_NNt,
				DS.DX_NM,
				DS.D_NM,
				DS.ERROR,
				DS.ERROR,
				DS.T_S,
				DS.ERROR,
				DS.ERROR,
				DS.ERROR,
				DS.ERROR,
				DS.T_Nt,
				DS.T_Nt,
				DS.TX_NN
			},
			new DS[18]
			{
				DS.ERROR,
				DS.TX_NNN,
				DS.TX_NNN,
				DS.TX_NNN,
				DS.ERROR,
				DS.ERROR,
				DS.ERROR,
				DS.ERROR,
				DS.ERROR,
				DS.ERROR,
				DS.T_S,
				DS.T_NNt,
				DS.ERROR,
				DS.ERROR,
				DS.ERROR,
				DS.T_NNt,
				DS.T_NNt,
				DS.TX_NNN
			}
		};

		internal static DateTime ParseExact(string s, string format, DateTimeFormatInfo dtfi, DateTimeStyles style)
		{
			DateTimeResult result = default(DateTimeResult);
			result.Init();
			if (TryParseExact(s, format, dtfi, style, ref result))
			{
				return result.parsedDate;
			}
			throw GetDateTimeParseException(ref result);
		}

		internal static DateTime ParseExact(string s, string format, DateTimeFormatInfo dtfi, DateTimeStyles style, out TimeSpan offset)
		{
			DateTimeResult result = default(DateTimeResult);
			offset = TimeSpan.Zero;
			result.Init();
			result.flags |= ParseFlags.CaptureOffset;
			if (TryParseExact(s, format, dtfi, style, ref result))
			{
				offset = result.timeZoneOffset;
				return result.parsedDate;
			}
			throw GetDateTimeParseException(ref result);
		}

		internal static bool TryParseExact(string s, string format, DateTimeFormatInfo dtfi, DateTimeStyles style, out DateTime result)
		{
			result = DateTime.MinValue;
			DateTimeResult result2 = default(DateTimeResult);
			result2.Init();
			if (TryParseExact(s, format, dtfi, style, ref result2))
			{
				result = result2.parsedDate;
				return true;
			}
			return false;
		}

		internal static bool TryParseExact(string s, string format, DateTimeFormatInfo dtfi, DateTimeStyles style, out DateTime result, out TimeSpan offset)
		{
			result = DateTime.MinValue;
			offset = TimeSpan.Zero;
			DateTimeResult result2 = default(DateTimeResult);
			result2.Init();
			result2.flags |= ParseFlags.CaptureOffset;
			if (TryParseExact(s, format, dtfi, style, ref result2))
			{
				result = result2.parsedDate;
				offset = result2.timeZoneOffset;
				return true;
			}
			return false;
		}

		internal static bool TryParseExact(string s, string format, DateTimeFormatInfo dtfi, DateTimeStyles style, ref DateTimeResult result)
		{
			if (s == null)
			{
				result.SetFailure(ParseFailureKind.ArgumentNull, "ArgumentNull_String", null, "s");
				return false;
			}
			if (format == null)
			{
				result.SetFailure(ParseFailureKind.ArgumentNull, "ArgumentNull_String", null, "format");
				return false;
			}
			if (s.Length == 0)
			{
				result.SetFailure(ParseFailureKind.Format, "Format_BadDateTime", null);
				return false;
			}
			if (format.Length == 0)
			{
				result.SetFailure(ParseFailureKind.Format, "Format_BadFormatSpecifier", null);
				return false;
			}
			return DoStrictParse(s, format, style, dtfi, ref result);
		}

		internal static DateTime ParseExactMultiple(string s, string[] formats, DateTimeFormatInfo dtfi, DateTimeStyles style)
		{
			DateTimeResult result = default(DateTimeResult);
			result.Init();
			if (TryParseExactMultiple(s, formats, dtfi, style, ref result))
			{
				return result.parsedDate;
			}
			throw GetDateTimeParseException(ref result);
		}

		internal static DateTime ParseExactMultiple(string s, string[] formats, DateTimeFormatInfo dtfi, DateTimeStyles style, out TimeSpan offset)
		{
			DateTimeResult result = default(DateTimeResult);
			offset = TimeSpan.Zero;
			result.Init();
			result.flags |= ParseFlags.CaptureOffset;
			if (TryParseExactMultiple(s, formats, dtfi, style, ref result))
			{
				offset = result.timeZoneOffset;
				return result.parsedDate;
			}
			throw GetDateTimeParseException(ref result);
		}

		internal static bool TryParseExactMultiple(string s, string[] formats, DateTimeFormatInfo dtfi, DateTimeStyles style, out DateTime result, out TimeSpan offset)
		{
			result = DateTime.MinValue;
			offset = TimeSpan.Zero;
			DateTimeResult result2 = default(DateTimeResult);
			result2.Init();
			result2.flags |= ParseFlags.CaptureOffset;
			if (TryParseExactMultiple(s, formats, dtfi, style, ref result2))
			{
				result = result2.parsedDate;
				offset = result2.timeZoneOffset;
				return true;
			}
			return false;
		}

		internal static bool TryParseExactMultiple(string s, string[] formats, DateTimeFormatInfo dtfi, DateTimeStyles style, out DateTime result)
		{
			result = DateTime.MinValue;
			DateTimeResult result2 = default(DateTimeResult);
			result2.Init();
			if (TryParseExactMultiple(s, formats, dtfi, style, ref result2))
			{
				result = result2.parsedDate;
				return true;
			}
			return false;
		}

		internal static bool TryParseExactMultiple(string s, string[] formats, DateTimeFormatInfo dtfi, DateTimeStyles style, ref DateTimeResult result)
		{
			if (s == null)
			{
				result.SetFailure(ParseFailureKind.ArgumentNull, "ArgumentNull_String", null, "s");
				return false;
			}
			if (formats == null)
			{
				result.SetFailure(ParseFailureKind.ArgumentNull, "ArgumentNull_String", null, "formats");
				return false;
			}
			if (s.Length == 0)
			{
				result.SetFailure(ParseFailureKind.Format, "Format_BadDateTime", null);
				return false;
			}
			if (formats.Length == 0)
			{
				result.SetFailure(ParseFailureKind.Format, "Format_BadFormatSpecifier", null);
				return false;
			}
			for (int i = 0; i < formats.Length; i++)
			{
				if (formats[i] == null || formats[i].Length == 0)
				{
					result.SetFailure(ParseFailureKind.Format, "Format_BadFormatSpecifier", null);
					return false;
				}
				DateTimeResult result2 = default(DateTimeResult);
				result2.Init();
				result2.flags = result.flags;
				if (TryParseExact(s, formats[i], dtfi, style, ref result2))
				{
					result.parsedDate = result2.parsedDate;
					result.timeZoneOffset = result2.timeZoneOffset;
					return true;
				}
			}
			result.SetFailure(ParseFailureKind.Format, "Format_BadDateTime", null);
			return false;
		}

		private static bool MatchWord(ref __DTString str, string target)
		{
			int length = target.Length;
			if (length > str.Value.Length - str.Index)
			{
				return false;
			}
			if (str.CompareInfo.Compare(str.Value, str.Index, length, target, 0, length, CompareOptions.IgnoreCase) != 0)
			{
				return false;
			}
			int num = str.Index + target.Length;
			if (num < str.Value.Length)
			{
				char c = str.Value[num];
				if (char.IsLetter(c))
				{
					return false;
				}
			}
			str.Index = num;
			if (str.Index < str.len)
			{
				str.m_current = str.Value[str.Index];
			}
			return true;
		}

		private static bool GetTimeZoneName(ref __DTString str)
		{
			if (MatchWord(ref str, "GMT"))
			{
				return true;
			}
			if (MatchWord(ref str, "Z"))
			{
				return true;
			}
			return false;
		}

		internal static bool IsDigit(char ch)
		{
			if (ch >= '0')
			{
				return ch <= '9';
			}
			return false;
		}

		private static bool ParseFraction(ref __DTString str, out double result)
		{
			result = 0.0;
			double num = 0.1;
			int num2 = 0;
			char current;
			while (str.GetNext() && IsDigit(current = str.m_current))
			{
				result += (double)(current - 48) * num;
				num *= 0.1;
				num2++;
			}
			return num2 > 0;
		}

		private static bool ParseTimeZone(ref __DTString str, ref TimeSpan result)
		{
			int num = 0;
			int num2 = 0;
			DTSubString subString = str.GetSubString();
			if (subString.length != 1)
			{
				return false;
			}
			char c = subString[0];
			if (c != '+' && c != '-')
			{
				return false;
			}
			str.ConsumeSubString(subString);
			subString = str.GetSubString();
			if (subString.type != DTSubStringType.Number)
			{
				return false;
			}
			int value = subString.value;
			switch (subString.length)
			{
			case 1:
			case 2:
				num = value;
				str.ConsumeSubString(subString);
				subString = str.GetSubString();
				if (subString.length == 1 && subString[0] == ':')
				{
					str.ConsumeSubString(subString);
					subString = str.GetSubString();
					if (subString.type != DTSubStringType.Number || subString.length < 1 || subString.length > 2)
					{
						return false;
					}
					num2 = subString.value;
					str.ConsumeSubString(subString);
				}
				break;
			case 3:
			case 4:
				num = value / 100;
				num2 = value % 100;
				str.ConsumeSubString(subString);
				break;
			default:
				return false;
			}
			if (num2 < 0 || num2 >= 60)
			{
				return false;
			}
			result = new TimeSpan(num, num2, 0);
			if (c == '-')
			{
				result = result.Negate();
			}
			return true;
		}

		private static bool Lex(DS dps, ref __DTString str, ref DateTimeToken dtok, ref DateTimeRawInfo raw, ref DateTimeResult result, ref DateTimeFormatInfo dtfi)
		{
			dtok.dtt = DTT.Unk;
			str.GetRegularToken(out var tokenType, out var tokenValue, dtfi);
			int indexBeforeSeparator;
			char charBeforeSeparator;
			switch (tokenType)
			{
			case TokenType.NumberToken:
			case TokenType.YearNumberToken:
			{
				if (raw.numCount == 3 || tokenValue == -1)
				{
					result.SetFailure(ParseFailureKind.Format, "Format_BadDateTime", null);
					return false;
				}
				if (dps == DS.T_NNt && str.Index < str.len - 1)
				{
					char c = str.Value[str.Index];
					if (c == '.')
					{
						ParseFraction(ref str, out raw.fraction);
					}
				}
				if ((dps == DS.T_NNt || dps == DS.T_Nt) && str.Index < str.len - 1)
				{
					char c2 = str.Value[str.Index];
					int num = 0;
					while (char.IsWhiteSpace(c2) && str.Index + num < str.len - 1)
					{
						num++;
						c2 = str.Value[str.Index + num];
					}
					if (c2 == '+' || c2 == '-')
					{
						str.Index += num;
						if ((result.flags & ParseFlags.TimeZoneUsed) != 0)
						{
							result.SetFailure(ParseFailureKind.Format, "Format_BadDateTime", null);
							return false;
						}
						result.flags |= ParseFlags.TimeZoneUsed;
						if (!ParseTimeZone(ref str, ref result.timeZoneOffset))
						{
							result.SetFailure(ParseFailureKind.Format, "Format_BadDateTime", null);
							return false;
						}
					}
				}
				dtok.num = tokenValue;
				TokenType separatorToken;
				if (tokenType == TokenType.YearNumberToken)
				{
					if (raw.year == -1)
					{
						raw.year = tokenValue;
						switch (separatorToken = str.GetSeparatorToken(dtfi, out indexBeforeSeparator, out charBeforeSeparator))
						{
						case TokenType.SEP_End:
							dtok.dtt = DTT.YearEnd;
							break;
						case TokenType.SEP_Am:
						case TokenType.SEP_Pm:
							if (raw.timeMark == TM.NotSet)
							{
								raw.timeMark = ((separatorToken != TokenType.SEP_Am) ? TM.PM : TM.AM);
								dtok.dtt = DTT.YearSpace;
							}
							else
							{
								result.SetFailure(ParseFailureKind.Format, "Format_BadDateTime", null);
							}
							break;
						case TokenType.SEP_Space:
							dtok.dtt = DTT.YearSpace;
							break;
						case TokenType.SEP_Date:
							dtok.dtt = DTT.YearDateSep;
							break;
						case TokenType.SEP_DateOrOffset:
							if (dateParsingStates[(int)dps][13] == DS.ERROR && dateParsingStates[(int)dps][12] > DS.ERROR)
							{
								str.Index = indexBeforeSeparator;
								str.m_current = charBeforeSeparator;
								dtok.dtt = DTT.YearSpace;
							}
							else
							{
								dtok.dtt = DTT.YearDateSep;
							}
							break;
						case TokenType.SEP_YearSuff:
						case TokenType.SEP_MonthSuff:
						case TokenType.SEP_DaySuff:
							dtok.dtt = DTT.NumDatesuff;
							dtok.suffix = separatorToken;
							break;
						case TokenType.SEP_HourSuff:
						case TokenType.SEP_MinuteSuff:
						case TokenType.SEP_SecondSuff:
							dtok.dtt = DTT.NumTimesuff;
							dtok.suffix = separatorToken;
							break;
						default:
							result.SetFailure(ParseFailureKind.Format, "Format_BadDateTime", null);
							return false;
						}
						return true;
					}
					result.SetFailure(ParseFailureKind.Format, "Format_BadDateTime", null);
					return false;
				}
				switch (separatorToken = str.GetSeparatorToken(dtfi, out indexBeforeSeparator, out charBeforeSeparator))
				{
				case TokenType.SEP_End:
					dtok.dtt = DTT.NumEnd;
					raw.AddNumber(dtok.num);
					break;
				case TokenType.SEP_Am:
				case TokenType.SEP_Pm:
					if (raw.timeMark == TM.NotSet)
					{
						raw.timeMark = ((separatorToken != TokenType.SEP_Am) ? TM.PM : TM.AM);
						dtok.dtt = DTT.NumAmpm;
						raw.AddNumber(dtok.num);
					}
					else
					{
						result.SetFailure(ParseFailureKind.Format, "Format_BadDateTime", null);
					}
					break;
				case TokenType.SEP_Space:
					dtok.dtt = DTT.NumSpace;
					raw.AddNumber(dtok.num);
					break;
				case TokenType.SEP_Date:
					dtok.dtt = DTT.NumDatesep;
					raw.AddNumber(dtok.num);
					break;
				case TokenType.SEP_DateOrOffset:
					if (dateParsingStates[(int)dps][4] == DS.ERROR && dateParsingStates[(int)dps][3] > DS.ERROR)
					{
						str.Index = indexBeforeSeparator;
						str.m_current = charBeforeSeparator;
						dtok.dtt = DTT.NumSpace;
					}
					else
					{
						dtok.dtt = DTT.NumDatesep;
					}
					raw.AddNumber(dtok.num);
					break;
				case TokenType.SEP_Time:
					dtok.dtt = DTT.NumTimesep;
					raw.AddNumber(dtok.num);
					break;
				case TokenType.SEP_YearSuff:
					dtok.num = dtfi.Calendar.ToFourDigitYear(tokenValue);
					dtok.dtt = DTT.NumDatesuff;
					dtok.suffix = separatorToken;
					break;
				case TokenType.SEP_MonthSuff:
				case TokenType.SEP_DaySuff:
					dtok.dtt = DTT.NumDatesuff;
					dtok.suffix = separatorToken;
					break;
				case TokenType.SEP_HourSuff:
				case TokenType.SEP_MinuteSuff:
				case TokenType.SEP_SecondSuff:
					dtok.dtt = DTT.NumTimesuff;
					dtok.suffix = separatorToken;
					break;
				case TokenType.SEP_LocalTimeMark:
					dtok.dtt = DTT.NumLocalTimeMark;
					raw.AddNumber(dtok.num);
					break;
				default:
					result.SetFailure(ParseFailureKind.Format, "Format_BadDateTime", null);
					return false;
				}
				break;
			}
			case TokenType.HebrewNumber:
			{
				TokenType separatorToken;
				if (tokenValue >= 100)
				{
					if (raw.year == -1)
					{
						raw.year = tokenValue;
						TokenType tokenType2 = (separatorToken = str.GetSeparatorToken(dtfi, out indexBeforeSeparator, out charBeforeSeparator));
						if (tokenType2 != TokenType.SEP_End)
						{
							if (tokenType2 != TokenType.SEP_Space)
							{
								if (tokenType2 != TokenType.SEP_DateOrOffset || dateParsingStates[(int)dps][12] <= DS.ERROR)
								{
									result.SetFailure(ParseFailureKind.Format, "Format_BadDateTime", null);
									return false;
								}
								str.Index = indexBeforeSeparator;
								str.m_current = charBeforeSeparator;
								dtok.dtt = DTT.YearSpace;
							}
							else
							{
								dtok.dtt = DTT.YearSpace;
							}
						}
						else
						{
							dtok.dtt = DTT.YearEnd;
						}
						break;
					}
					result.SetFailure(ParseFailureKind.Format, "Format_BadDateTime", null);
					return false;
				}
				dtok.num = tokenValue;
				raw.AddNumber(dtok.num);
				switch (separatorToken = str.GetSeparatorToken(dtfi, out indexBeforeSeparator, out charBeforeSeparator))
				{
				case TokenType.SEP_End:
					dtok.dtt = DTT.NumEnd;
					break;
				case TokenType.SEP_Space:
				case TokenType.SEP_Date:
					dtok.dtt = DTT.NumDatesep;
					break;
				case TokenType.SEP_DateOrOffset:
					if (dateParsingStates[(int)dps][4] == DS.ERROR && dateParsingStates[(int)dps][3] > DS.ERROR)
					{
						str.Index = indexBeforeSeparator;
						str.m_current = charBeforeSeparator;
						dtok.dtt = DTT.NumSpace;
					}
					else
					{
						dtok.dtt = DTT.NumDatesep;
					}
					break;
				default:
					result.SetFailure(ParseFailureKind.Format, "Format_BadDateTime", null);
					return false;
				}
				break;
			}
			case TokenType.DayOfWeekToken:
				if (raw.dayOfWeek == -1)
				{
					raw.dayOfWeek = tokenValue;
					dtok.dtt = DTT.DayOfWeek;
					break;
				}
				result.SetFailure(ParseFailureKind.Format, "Format_BadDateTime", null);
				return false;
			case TokenType.MonthToken:
				if (raw.month == -1)
				{
					TokenType separatorToken;
					switch (separatorToken = str.GetSeparatorToken(dtfi, out indexBeforeSeparator, out charBeforeSeparator))
					{
					case TokenType.SEP_End:
						dtok.dtt = DTT.MonthEnd;
						break;
					case TokenType.SEP_Space:
						dtok.dtt = DTT.MonthSpace;
						break;
					case TokenType.SEP_Date:
						dtok.dtt = DTT.MonthDatesep;
						break;
					case TokenType.SEP_DateOrOffset:
						if (dateParsingStates[(int)dps][8] == DS.ERROR && dateParsingStates[(int)dps][7] > DS.ERROR)
						{
							str.Index = indexBeforeSeparator;
							str.m_current = charBeforeSeparator;
							dtok.dtt = DTT.MonthSpace;
						}
						else
						{
							dtok.dtt = DTT.MonthDatesep;
						}
						break;
					default:
						result.SetFailure(ParseFailureKind.Format, "Format_BadDateTime", null);
						return false;
					}
					raw.month = tokenValue;
					break;
				}
				result.SetFailure(ParseFailureKind.Format, "Format_BadDateTime", null);
				return false;
			case TokenType.EraToken:
				if (result.era != -1)
				{
					result.era = tokenValue;
					dtok.dtt = DTT.Era;
					break;
				}
				result.SetFailure(ParseFailureKind.Format, "Format_BadDateTime", null);
				return false;
			case TokenType.JapaneseEraToken:
				result.calendar = JapaneseCalendar.GetDefaultInstance();
				dtfi = DateTimeFormatInfo.GetJapaneseCalendarDTFI();
				if (result.era != -1)
				{
					result.era = tokenValue;
					dtok.dtt = DTT.Era;
					break;
				}
				result.SetFailure(ParseFailureKind.Format, "Format_BadDateTime", null);
				return false;
			case TokenType.TEraToken:
				result.calendar = TaiwanCalendar.GetDefaultInstance();
				dtfi = DateTimeFormatInfo.GetTaiwanCalendarDTFI();
				if (result.era != -1)
				{
					result.era = tokenValue;
					dtok.dtt = DTT.Era;
					break;
				}
				result.SetFailure(ParseFailureKind.Format, "Format_BadDateTime", null);
				return false;
			case TokenType.TimeZoneToken:
				dtok.dtt = DTT.TimeZone;
				result.flags |= ParseFlags.TimeZoneUsed;
				result.timeZoneOffset = new TimeSpan(0L);
				result.flags |= ParseFlags.TimeZoneUtc;
				break;
			case TokenType.EndOfString:
				dtok.dtt = DTT.End;
				break;
			case TokenType.Am:
			case TokenType.Pm:
				if (raw.timeMark == TM.NotSet)
				{
					raw.timeMark = (TM)tokenValue;
					break;
				}
				result.SetFailure(ParseFailureKind.Format, "Format_BadDateTime", null);
				return false;
			case TokenType.UnknownToken:
				if (char.IsLetter(str.m_current))
				{
					result.SetFailure(ParseFailureKind.FormatWithParameter, "Format_UnknowDateTimeWord", str.Index);
					return false;
				}
				if (Environment.GetCompatibilityFlag(CompatibilityFlag.DateTimeParseIgnorePunctuation) && (result.flags & ParseFlags.CaptureOffset) == 0)
				{
					str.GetNext();
					return true;
				}
				if ((str.m_current == '-' || str.m_current == '+') && (result.flags & ParseFlags.TimeZoneUsed) == 0)
				{
					int index = str.Index;
					if (ParseTimeZone(ref str, ref result.timeZoneOffset))
					{
						result.flags |= ParseFlags.TimeZoneUsed;
						return true;
					}
					str.Index = index;
				}
				if (VerifyValidPunctuation(ref str))
				{
					return true;
				}
				result.SetFailure(ParseFailureKind.Format, "Format_BadDateTime", null);
				return false;
			}
			return true;
		}

		private static bool VerifyValidPunctuation(ref __DTString str)
		{
			switch (str.Value[str.Index])
			{
			case '#':
			{
				bool flag = false;
				bool flag2 = false;
				for (int j = 0; j < str.len; j++)
				{
					char c = str.Value[j];
					switch (c)
					{
					case '#':
						if (flag)
						{
							if (flag2)
							{
								return false;
							}
							flag2 = true;
						}
						else
						{
							flag = true;
						}
						break;
					case '\0':
						if (!flag2)
						{
							return false;
						}
						break;
					default:
						if (!char.IsWhiteSpace(c) && (!flag || flag2))
						{
							return false;
						}
						break;
					}
				}
				if (!flag2)
				{
					return false;
				}
				str.GetNext();
				return true;
			}
			case '\0':
			{
				for (int i = str.Index; i < str.len; i++)
				{
					if (str.Value[i] != 0)
					{
						return false;
					}
				}
				str.Index = str.len;
				return true;
			}
			default:
				return false;
			}
		}

		private static bool GetYearMonthDayOrder(string datePattern, DateTimeFormatInfo dtfi, out int order)
		{
			int num = -1;
			int num2 = -1;
			int num3 = -1;
			int num4 = 0;
			bool flag = false;
			for (int i = 0; i < datePattern.Length; i++)
			{
				if (num4 >= 3)
				{
					break;
				}
				char c = datePattern[i];
				if (c == '\'' || c == '"')
				{
					flag = !flag;
				}
				if (flag)
				{
					continue;
				}
				switch (c)
				{
				case 'y':
					num = num4++;
					for (; i + 1 < datePattern.Length && datePattern[i + 1] == 'y'; i++)
					{
					}
					break;
				case 'M':
					num2 = num4++;
					for (; i + 1 < datePattern.Length && datePattern[i + 1] == 'M'; i++)
					{
					}
					break;
				case 'd':
				{
					int num5 = 1;
					for (; i + 1 < datePattern.Length && datePattern[i + 1] == 'd'; i++)
					{
						num5++;
					}
					if (num5 <= 2)
					{
						num3 = num4++;
					}
					break;
				}
				}
			}
			if (num == 0 && num2 == 1 && num3 == 2)
			{
				order = 0;
				return true;
			}
			if (num2 == 0 && num3 == 1 && num == 2)
			{
				order = 1;
				return true;
			}
			if (num3 == 0 && num2 == 1 && num == 2)
			{
				order = 2;
				return true;
			}
			if (num == 0 && num3 == 1 && num2 == 2)
			{
				order = 3;
				return true;
			}
			order = -1;
			return false;
		}

		private static bool GetYearMonthOrder(string pattern, DateTimeFormatInfo dtfi, out int order)
		{
			int num = -1;
			int num2 = -1;
			int num3 = 0;
			bool flag = false;
			for (int i = 0; i < pattern.Length; i++)
			{
				if (num3 >= 2)
				{
					break;
				}
				char c = pattern[i];
				if (c == '\'' || c == '"')
				{
					flag = !flag;
				}
				if (flag)
				{
					continue;
				}
				switch (c)
				{
				case 'y':
					num = num3++;
					for (; i + 1 < pattern.Length && pattern[i + 1] == 'y'; i++)
					{
					}
					break;
				case 'M':
					num2 = num3++;
					for (; i + 1 < pattern.Length && pattern[i + 1] == 'M'; i++)
					{
					}
					break;
				}
			}
			if (num == 0 && num2 == 1)
			{
				order = 4;
				return true;
			}
			if (num2 == 0 && num == 1)
			{
				order = 5;
				return true;
			}
			order = -1;
			return false;
		}

		private static bool GetMonthDayOrder(string pattern, DateTimeFormatInfo dtfi, out int order)
		{
			int num = -1;
			int num2 = -1;
			int num3 = 0;
			bool flag = false;
			for (int i = 0; i < pattern.Length; i++)
			{
				if (num3 >= 2)
				{
					break;
				}
				char c = pattern[i];
				if (c == '\'' || c == '"')
				{
					flag = !flag;
				}
				if (flag)
				{
					continue;
				}
				switch (c)
				{
				case 'd':
				{
					int num4 = 1;
					for (; i + 1 < pattern.Length && pattern[i + 1] == 'd'; i++)
					{
						num4++;
					}
					if (num4 <= 2)
					{
						num2 = num3++;
					}
					break;
				}
				case 'M':
					num = num3++;
					for (; i + 1 < pattern.Length && pattern[i + 1] == 'M'; i++)
					{
					}
					break;
				}
			}
			if (num == 0 && num2 == 1)
			{
				order = 6;
				return true;
			}
			if (num2 == 0 && num == 1)
			{
				order = 7;
				return true;
			}
			order = -1;
			return false;
		}

		private static int AdjustYear(ref DateTimeResult result, int year)
		{
			if (year < 100)
			{
				year = result.calendar.ToFourDigitYear(year);
			}
			return year;
		}

		private static bool SetDateYMD(ref DateTimeResult result, int year, int month, int day)
		{
			if (result.calendar.IsValidDay(year, month, day, result.era))
			{
				result.SetDate(year, month, day);
				return true;
			}
			return false;
		}

		private static bool SetDateMDY(ref DateTimeResult result, int month, int day, int year)
		{
			return SetDateYMD(ref result, year, month, day);
		}

		private static bool SetDateDMY(ref DateTimeResult result, int day, int month, int year)
		{
			return SetDateYMD(ref result, year, month, day);
		}

		private static bool SetDateYDM(ref DateTimeResult result, int year, int day, int month)
		{
			return SetDateYMD(ref result, year, month, day);
		}

		private static void GetDefaultYear(ref DateTimeResult result, ref DateTimeStyles styles)
		{
			result.Year = result.calendar.GetYear(GetDateTimeNow(ref result, ref styles));
			result.flags |= ParseFlags.YearDefault;
		}

		private static bool GetDayOfNN(ref DateTimeResult result, ref DateTimeStyles styles, ref DateTimeRawInfo raw, DateTimeFormatInfo dtfi)
		{
			if ((result.flags & ParseFlags.HaveDate) != 0)
			{
				result.SetFailure(ParseFailureKind.Format, "Format_BadDateTime", null);
				return false;
			}
			int number = raw.GetNumber(0);
			int number2 = raw.GetNumber(1);
			GetDefaultYear(ref result, ref styles);
			if (!GetMonthDayOrder(dtfi.MonthDayPattern, dtfi, out var order))
			{
				result.SetFailure(ParseFailureKind.FormatWithParameter, "Format_BadDatePattern", dtfi.MonthDayPattern);
				return false;
			}
			if (order == 6)
			{
				if (SetDateYMD(ref result, result.Year, number, number2))
				{
					result.flags |= ParseFlags.HaveDate;
					return true;
				}
			}
			else if (SetDateYMD(ref result, result.Year, number2, number))
			{
				result.flags |= ParseFlags.HaveDate;
				return true;
			}
			result.SetFailure(ParseFailureKind.Format, "Format_BadDateTime", null);
			return false;
		}

		private static bool GetDayOfNNN(ref DateTimeResult result, ref DateTimeRawInfo raw, DateTimeFormatInfo dtfi)
		{
			if ((result.flags & ParseFlags.HaveDate) != 0)
			{
				result.SetFailure(ParseFailureKind.Format, "Format_BadDateTime", null);
				return false;
			}
			int number = raw.GetNumber(0);
			int number2 = raw.GetNumber(1);
			int number3 = raw.GetNumber(2);
			if (!GetYearMonthDayOrder(dtfi.ShortDatePattern, dtfi, out var order))
			{
				result.SetFailure(ParseFailureKind.FormatWithParameter, "Format_BadDatePattern", dtfi.ShortDatePattern);
				return false;
			}
			switch (order)
			{
			case 0:
				if (SetDateYMD(ref result, AdjustYear(ref result, number), number2, number3))
				{
					result.flags |= ParseFlags.HaveDate;
					return true;
				}
				break;
			case 1:
				if (SetDateMDY(ref result, number, number2, AdjustYear(ref result, number3)))
				{
					result.flags |= ParseFlags.HaveDate;
					return true;
				}
				break;
			case 2:
				if (SetDateDMY(ref result, number, number2, AdjustYear(ref result, number3)))
				{
					result.flags |= ParseFlags.HaveDate;
					return true;
				}
				break;
			case 3:
				if (SetDateYDM(ref result, AdjustYear(ref result, number), number2, number3))
				{
					result.flags |= ParseFlags.HaveDate;
					return true;
				}
				break;
			}
			result.SetFailure(ParseFailureKind.Format, "Format_BadDateTime", null);
			return false;
		}

		private static bool GetDayOfMN(ref DateTimeResult result, ref DateTimeStyles styles, ref DateTimeRawInfo raw, DateTimeFormatInfo dtfi)
		{
			if ((result.flags & ParseFlags.HaveDate) != 0)
			{
				result.SetFailure(ParseFailureKind.Format, "Format_BadDateTime", null);
				return false;
			}
			if (!GetMonthDayOrder(dtfi.MonthDayPattern, dtfi, out var order))
			{
				result.SetFailure(ParseFailureKind.FormatWithParameter, "Format_BadDatePattern", dtfi.MonthDayPattern);
				return false;
			}
			if (order == 7)
			{
				if (!GetYearMonthOrder(dtfi.YearMonthPattern, dtfi, out var order2))
				{
					result.SetFailure(ParseFailureKind.FormatWithParameter, "Format_BadDatePattern", dtfi.YearMonthPattern);
					return false;
				}
				if (order2 == 5)
				{
					if (!SetDateYMD(ref result, AdjustYear(ref result, raw.GetNumber(0)), raw.month, 1))
					{
						result.SetFailure(ParseFailureKind.Format, "Format_BadDateTime", null);
						return false;
					}
					return true;
				}
			}
			GetDefaultYear(ref result, ref styles);
			if (!SetDateYMD(ref result, result.Year, raw.month, raw.GetNumber(0)))
			{
				result.SetFailure(ParseFailureKind.Format, "Format_BadDateTime", null);
				return false;
			}
			return true;
		}

		private static bool GetHebrewDayOfNM(ref DateTimeResult result, ref DateTimeRawInfo raw, DateTimeFormatInfo dtfi)
		{
			if (!GetMonthDayOrder(dtfi.MonthDayPattern, dtfi, out var order))
			{
				result.SetFailure(ParseFailureKind.FormatWithParameter, "Format_BadDatePattern", dtfi.MonthDayPattern);
				return false;
			}
			result.Month = raw.month;
			if (order == 7 && result.calendar.IsValidDay(result.Year, result.Month, raw.GetNumber(0), result.era))
			{
				result.Day = raw.GetNumber(0);
				return true;
			}
			result.SetFailure(ParseFailureKind.Format, "Format_BadDateTime", null);
			return false;
		}

		private static bool GetDayOfNM(ref DateTimeResult result, ref DateTimeStyles styles, ref DateTimeRawInfo raw, DateTimeFormatInfo dtfi)
		{
			if ((result.flags & ParseFlags.HaveDate) != 0)
			{
				result.SetFailure(ParseFailureKind.Format, "Format_BadDateTime", null);
				return false;
			}
			if (!GetMonthDayOrder(dtfi.MonthDayPattern, dtfi, out var order))
			{
				result.SetFailure(ParseFailureKind.FormatWithParameter, "Format_BadDatePattern", dtfi.MonthDayPattern);
				return false;
			}
			if (order == 6)
			{
				if (!GetYearMonthOrder(dtfi.YearMonthPattern, dtfi, out var order2))
				{
					result.SetFailure(ParseFailureKind.FormatWithParameter, "Format_BadDatePattern", dtfi.YearMonthPattern);
					return false;
				}
				if (order2 == 4)
				{
					if (!SetDateYMD(ref result, AdjustYear(ref result, raw.GetNumber(0)), raw.month, 1))
					{
						result.SetFailure(ParseFailureKind.Format, "Format_BadDateTime", null);
						return false;
					}
					return true;
				}
			}
			GetDefaultYear(ref result, ref styles);
			if (!SetDateYMD(ref result, result.Year, raw.month, raw.GetNumber(0)))
			{
				result.SetFailure(ParseFailureKind.Format, "Format_BadDateTime", null);
				return false;
			}
			return true;
		}

		private static bool GetDayOfMNN(ref DateTimeResult result, ref DateTimeRawInfo raw, DateTimeFormatInfo dtfi)
		{
			if ((result.flags & ParseFlags.HaveDate) != 0)
			{
				result.SetFailure(ParseFailureKind.Format, "Format_BadDateTime", null);
				return false;
			}
			int number = raw.GetNumber(0);
			int number2 = raw.GetNumber(1);
			if (!GetYearMonthDayOrder(dtfi.ShortDatePattern, dtfi, out var order))
			{
				result.SetFailure(ParseFailureKind.FormatWithParameter, "Format_BadDatePattern", dtfi.ShortDatePattern);
				return false;
			}
			switch (order)
			{
			case 1:
			{
				int year;
				if (result.calendar.IsValidDay(year = AdjustYear(ref result, number2), raw.month, number, result.era))
				{
					result.SetDate(year, raw.month, number);
					result.flags |= ParseFlags.HaveDate;
					return true;
				}
				if (result.calendar.IsValidDay(year = AdjustYear(ref result, number), raw.month, number2, result.era))
				{
					result.SetDate(year, raw.month, number2);
					result.flags |= ParseFlags.HaveDate;
					return true;
				}
				break;
			}
			case 0:
			{
				int year;
				if (result.calendar.IsValidDay(year = AdjustYear(ref result, number), raw.month, number2, result.era))
				{
					result.SetDate(year, raw.month, number2);
					result.flags |= ParseFlags.HaveDate;
					return true;
				}
				if (result.calendar.IsValidDay(year = AdjustYear(ref result, number2), raw.month, number, result.era))
				{
					result.SetDate(year, raw.month, number);
					result.flags |= ParseFlags.HaveDate;
					return true;
				}
				break;
			}
			case 2:
			{
				int year;
				if (result.calendar.IsValidDay(year = AdjustYear(ref result, number2), raw.month, number, result.era))
				{
					result.SetDate(year, raw.month, number);
					result.flags |= ParseFlags.HaveDate;
					return true;
				}
				if (result.calendar.IsValidDay(year = AdjustYear(ref result, number), raw.month, number2, result.era))
				{
					result.SetDate(year, raw.month, number2);
					result.flags |= ParseFlags.HaveDate;
					return true;
				}
				break;
			}
			}
			result.SetFailure(ParseFailureKind.Format, "Format_BadDateTime", null);
			return false;
		}

		private static bool GetDayOfYNN(ref DateTimeResult result, ref DateTimeRawInfo raw, DateTimeFormatInfo dtfi)
		{
			if ((result.flags & ParseFlags.HaveDate) != 0)
			{
				result.SetFailure(ParseFailureKind.Format, "Format_BadDateTime", null);
				return false;
			}
			int number = raw.GetNumber(0);
			int number2 = raw.GetNumber(1);
			string datePattern = dtfi.ShortDatePattern;
			if (dtfi.CultureId == 1079)
			{
				datePattern = dtfi.LongDatePattern;
			}
			if (GetYearMonthDayOrder(datePattern, dtfi, out var order) && order == 3)
			{
				if (SetDateYMD(ref result, raw.year, number2, number))
				{
					result.flags |= ParseFlags.HaveDate;
					return true;
				}
			}
			else if (SetDateYMD(ref result, raw.year, number, number2))
			{
				result.flags |= ParseFlags.HaveDate;
				return true;
			}
			result.SetFailure(ParseFailureKind.Format, "Format_BadDateTime", null);
			return false;
		}

		private static bool GetDayOfNNY(ref DateTimeResult result, ref DateTimeRawInfo raw, DateTimeFormatInfo dtfi)
		{
			if ((result.flags & ParseFlags.HaveDate) != 0)
			{
				result.SetFailure(ParseFailureKind.Format, "Format_BadDateTime", null);
				return false;
			}
			int number = raw.GetNumber(0);
			int number2 = raw.GetNumber(1);
			if (!GetYearMonthDayOrder(dtfi.ShortDatePattern, dtfi, out var order))
			{
				result.SetFailure(ParseFailureKind.FormatWithParameter, "Format_BadDatePattern", dtfi.ShortDatePattern);
				return false;
			}
			if (order == 1 || order == 0)
			{
				if (SetDateYMD(ref result, raw.year, number, number2))
				{
					result.flags |= ParseFlags.HaveDate;
					return true;
				}
			}
			else if (SetDateYMD(ref result, raw.year, number2, number))
			{
				result.flags |= ParseFlags.HaveDate;
				return true;
			}
			result.SetFailure(ParseFailureKind.Format, "Format_BadDateTime", null);
			return false;
		}

		private static bool GetDayOfYMN(ref DateTimeResult result, ref DateTimeRawInfo raw, DateTimeFormatInfo dtfi)
		{
			if ((result.flags & ParseFlags.HaveDate) != 0)
			{
				result.SetFailure(ParseFailureKind.Format, "Format_BadDateTime", null);
				return false;
			}
			if (SetDateYMD(ref result, raw.year, raw.month, raw.GetNumber(0)))
			{
				result.flags |= ParseFlags.HaveDate;
				return true;
			}
			result.SetFailure(ParseFailureKind.Format, "Format_BadDateTime", null);
			return false;
		}

		private static bool GetDayOfYN(ref DateTimeResult result, ref DateTimeRawInfo raw, DateTimeFormatInfo dtfi)
		{
			if ((result.flags & ParseFlags.HaveDate) != 0)
			{
				result.SetFailure(ParseFailureKind.Format, "Format_BadDateTime", null);
				return false;
			}
			if (SetDateYMD(ref result, raw.year, raw.GetNumber(0), 1))
			{
				result.flags |= ParseFlags.HaveDate;
				return true;
			}
			result.SetFailure(ParseFailureKind.Format, "Format_BadDateTime", null);
			return false;
		}

		private static bool GetDayOfYM(ref DateTimeResult result, ref DateTimeRawInfo raw, DateTimeFormatInfo dtfi)
		{
			if ((result.flags & ParseFlags.HaveDate) != 0)
			{
				result.SetFailure(ParseFailureKind.Format, "Format_BadDateTime", null);
				return false;
			}
			if (SetDateYMD(ref result, raw.year, raw.month, 1))
			{
				result.flags |= ParseFlags.HaveDate;
				return true;
			}
			result.SetFailure(ParseFailureKind.Format, "Format_BadDateTime", null);
			return false;
		}

		private static void AdjustTimeMark(DateTimeFormatInfo dtfi, ref DateTimeRawInfo raw)
		{
			if (raw.timeMark == TM.NotSet && dtfi.AMDesignator != null && dtfi.PMDesignator != null)
			{
				if (dtfi.AMDesignator.Length == 0 && dtfi.PMDesignator.Length != 0)
				{
					raw.timeMark = TM.AM;
				}
				if (dtfi.PMDesignator.Length == 0 && dtfi.AMDesignator.Length != 0)
				{
					raw.timeMark = TM.PM;
				}
			}
		}

		private static bool AdjustHour(ref int hour, TM timeMark)
		{
			switch (timeMark)
			{
			case TM.AM:
				if (hour < 0 || hour > 12)
				{
					return false;
				}
				hour = ((hour != 12) ? hour : 0);
				break;
			default:
				if (hour < 0 || hour > 23)
				{
					return false;
				}
				if (hour < 12)
				{
					hour += 12;
				}
				break;
			case TM.NotSet:
				break;
			}
			return true;
		}

		private static bool GetTimeOfN(DateTimeFormatInfo dtfi, ref DateTimeResult result, ref DateTimeRawInfo raw)
		{
			if ((result.flags & ParseFlags.HaveTime) != 0)
			{
				result.SetFailure(ParseFailureKind.Format, "Format_BadDateTime", null);
				return false;
			}
			if (raw.timeMark == TM.NotSet)
			{
				result.SetFailure(ParseFailureKind.Format, "Format_BadDateTime", null);
				return false;
			}
			result.Hour = raw.GetNumber(0);
			result.flags |= ParseFlags.HaveTime;
			return true;
		}

		private static bool GetTimeOfNN(DateTimeFormatInfo dtfi, ref DateTimeResult result, ref DateTimeRawInfo raw)
		{
			if ((result.flags & ParseFlags.HaveTime) != 0)
			{
				result.SetFailure(ParseFailureKind.Format, "Format_BadDateTime", null);
				return false;
			}
			result.Hour = raw.GetNumber(0);
			result.Minute = raw.GetNumber(1);
			result.flags |= ParseFlags.HaveTime;
			return true;
		}

		private static bool GetTimeOfNNN(DateTimeFormatInfo dtfi, ref DateTimeResult result, ref DateTimeRawInfo raw)
		{
			if ((result.flags & ParseFlags.HaveTime) != 0)
			{
				result.SetFailure(ParseFailureKind.Format, "Format_BadDateTime", null);
				return false;
			}
			result.Hour = raw.GetNumber(0);
			result.Minute = raw.GetNumber(1);
			result.Second = raw.GetNumber(2);
			result.flags |= ParseFlags.HaveTime;
			return true;
		}

		private static bool GetDateOfDSN(ref DateTimeResult result, ref DateTimeRawInfo raw)
		{
			if (raw.numCount != 1 || result.Day != -1)
			{
				result.SetFailure(ParseFailureKind.Format, "Format_BadDateTime", null);
				return false;
			}
			result.Day = raw.GetNumber(0);
			return true;
		}

		private static bool GetDateOfNDS(ref DateTimeResult result, ref DateTimeRawInfo raw)
		{
			if (result.Month == -1)
			{
				result.SetFailure(ParseFailureKind.Format, "Format_BadDateTime", null);
				return false;
			}
			if (result.Year != -1)
			{
				result.SetFailure(ParseFailureKind.Format, "Format_BadDateTime", null);
				return false;
			}
			result.Year = AdjustYear(ref result, raw.GetNumber(0));
			result.Day = 1;
			return true;
		}

		private static bool GetDateOfNNDS(ref DateTimeResult result, ref DateTimeRawInfo raw, DateTimeFormatInfo dtfi)
		{
			if ((result.flags & ParseFlags.HaveYear) != 0)
			{
				if ((result.flags & ParseFlags.HaveMonth) == 0 && (result.flags & ParseFlags.HaveDay) == 0 && SetDateYMD(ref result, result.Year = AdjustYear(ref result, raw.year), raw.GetNumber(0), raw.GetNumber(1)))
				{
					return true;
				}
			}
			else if ((result.flags & ParseFlags.HaveMonth) != 0 && (result.flags & ParseFlags.HaveYear) == 0 && (result.flags & ParseFlags.HaveDay) == 0)
			{
				if (!GetYearMonthDayOrder(dtfi.ShortDatePattern, dtfi, out var order))
				{
					result.SetFailure(ParseFailureKind.FormatWithParameter, "Format_BadDatePattern", dtfi.ShortDatePattern);
					return false;
				}
				if (order == 0)
				{
					if (SetDateYMD(ref result, AdjustYear(ref result, raw.GetNumber(0)), result.Month, raw.GetNumber(1)))
					{
						return true;
					}
				}
				else if (SetDateYMD(ref result, AdjustYear(ref result, raw.GetNumber(1)), result.Month, raw.GetNumber(0)))
				{
					return true;
				}
			}
			result.SetFailure(ParseFailureKind.Format, "Format_BadDateTime", null);
			return false;
		}

		private static bool ProcessDateTimeSuffix(ref DateTimeResult result, ref DateTimeRawInfo raw, ref DateTimeToken dtok)
		{
			switch (dtok.suffix)
			{
			case TokenType.SEP_YearSuff:
				if ((result.flags & ParseFlags.HaveYear) != 0)
				{
					return false;
				}
				result.flags |= ParseFlags.HaveYear;
				result.Year = (raw.year = dtok.num);
				break;
			case TokenType.SEP_MonthSuff:
				if ((result.flags & ParseFlags.HaveMonth) != 0)
				{
					return false;
				}
				result.flags |= ParseFlags.HaveMonth;
				result.Month = (raw.month = dtok.num);
				break;
			case TokenType.SEP_DaySuff:
				if ((result.flags & ParseFlags.HaveDay) != 0)
				{
					return false;
				}
				result.flags |= ParseFlags.HaveDay;
				result.Day = dtok.num;
				break;
			case TokenType.SEP_HourSuff:
				if ((result.flags & ParseFlags.HaveHour) != 0)
				{
					return false;
				}
				result.flags |= ParseFlags.HaveHour;
				result.Hour = dtok.num;
				break;
			case TokenType.SEP_MinuteSuff:
				if ((result.flags & ParseFlags.HaveMinute) != 0)
				{
					return false;
				}
				result.flags |= ParseFlags.HaveMinute;
				result.Minute = dtok.num;
				break;
			case TokenType.SEP_SecondSuff:
				if ((result.flags & ParseFlags.HaveSecond) != 0)
				{
					return false;
				}
				result.flags |= ParseFlags.HaveSecond;
				result.Second = dtok.num;
				break;
			}
			return true;
		}

		internal static bool ProcessHebrewTerminalState(DS dps, ref DateTimeResult result, ref DateTimeStyles styles, ref DateTimeRawInfo raw, DateTimeFormatInfo dtfi)
		{
			switch (dps)
			{
			case DS.DX_MNN:
				raw.year = raw.GetNumber(1);
				if (!dtfi.YearMonthAdjustment(ref raw.year, ref raw.month, parsedMonthName: true))
				{
					result.SetFailure(ParseFailureKind.FormatBadDateTimeCalendar, "Format_BadDateTimeCalendar", null);
					return false;
				}
				if (!GetDayOfMNN(ref result, ref raw, dtfi))
				{
					return false;
				}
				break;
			case DS.DX_YMN:
				if (!dtfi.YearMonthAdjustment(ref raw.year, ref raw.month, parsedMonthName: true))
				{
					result.SetFailure(ParseFailureKind.FormatBadDateTimeCalendar, "Format_BadDateTimeCalendar", null);
					return false;
				}
				if (!GetDayOfYMN(ref result, ref raw, dtfi))
				{
					return false;
				}
				break;
			case DS.DX_NM:
				GetDefaultYear(ref result, ref styles);
				if (!dtfi.YearMonthAdjustment(ref result.Year, ref raw.month, parsedMonthName: true))
				{
					result.SetFailure(ParseFailureKind.FormatBadDateTimeCalendar, "Format_BadDateTimeCalendar", null);
					return false;
				}
				if (!GetHebrewDayOfNM(ref result, ref raw, dtfi))
				{
					return false;
				}
				break;
			case DS.DX_YM:
				if (!dtfi.YearMonthAdjustment(ref raw.year, ref raw.month, parsedMonthName: true))
				{
					result.SetFailure(ParseFailureKind.FormatBadDateTimeCalendar, "Format_BadDateTimeCalendar", null);
					return false;
				}
				if (!GetDayOfYM(ref result, ref raw, dtfi))
				{
					return false;
				}
				break;
			case DS.TX_N:
				if (!GetTimeOfN(dtfi, ref result, ref raw))
				{
					return false;
				}
				break;
			case DS.TX_NN:
				if (!GetTimeOfNN(dtfi, ref result, ref raw))
				{
					return false;
				}
				break;
			case DS.TX_NNN:
				if (!GetTimeOfNNN(dtfi, ref result, ref raw))
				{
					return false;
				}
				break;
			default:
				result.SetFailure(ParseFailureKind.Format, "Format_BadDateTime", null);
				return false;
			}
			if (dps > DS.ERROR)
			{
				raw.numCount = 0;
			}
			return true;
		}

		internal static bool ProcessTerminaltState(DS dps, ref DateTimeResult result, ref DateTimeStyles styles, ref DateTimeRawInfo raw, DateTimeFormatInfo dtfi)
		{
			switch (dps)
			{
			case DS.DX_NN:
				if (!GetDayOfNN(ref result, ref styles, ref raw, dtfi))
				{
					return false;
				}
				break;
			case DS.DX_NNN:
				if (!GetDayOfNNN(ref result, ref raw, dtfi))
				{
					return false;
				}
				break;
			case DS.DX_MN:
				if (!GetDayOfMN(ref result, ref styles, ref raw, dtfi))
				{
					return false;
				}
				break;
			case DS.DX_NM:
				if (!GetDayOfNM(ref result, ref styles, ref raw, dtfi))
				{
					return false;
				}
				break;
			case DS.DX_MNN:
				if (!GetDayOfMNN(ref result, ref raw, dtfi))
				{
					return false;
				}
				break;
			case DS.DX_YNN:
				if (!GetDayOfYNN(ref result, ref raw, dtfi))
				{
					return false;
				}
				break;
			case DS.DX_NNY:
				if (!GetDayOfNNY(ref result, ref raw, dtfi))
				{
					return false;
				}
				break;
			case DS.DX_YMN:
				if (!GetDayOfYMN(ref result, ref raw, dtfi))
				{
					return false;
				}
				break;
			case DS.DX_YN:
				if (!GetDayOfYN(ref result, ref raw, dtfi))
				{
					return false;
				}
				break;
			case DS.DX_YM:
				if (!GetDayOfYM(ref result, ref raw, dtfi))
				{
					return false;
				}
				break;
			case DS.TX_N:
				if (!GetTimeOfN(dtfi, ref result, ref raw))
				{
					return false;
				}
				break;
			case DS.TX_NN:
				if (!GetTimeOfNN(dtfi, ref result, ref raw))
				{
					return false;
				}
				break;
			case DS.TX_NNN:
				if (!GetTimeOfNNN(dtfi, ref result, ref raw))
				{
					return false;
				}
				break;
			case DS.DX_DSN:
				if (!GetDateOfDSN(ref result, ref raw))
				{
					return false;
				}
				break;
			case DS.DX_NDS:
				if (!GetDateOfNDS(ref result, ref raw))
				{
					return false;
				}
				break;
			case DS.DX_NNDS:
				if (!GetDateOfNNDS(ref result, ref raw, dtfi))
				{
					return false;
				}
				break;
			}
			if (dps > DS.ERROR)
			{
				raw.numCount = 0;
			}
			return true;
		}

		internal static DateTime Parse(string s, DateTimeFormatInfo dtfi, DateTimeStyles styles)
		{
			DateTimeResult result = default(DateTimeResult);
			result.Init();
			if (TryParse(s, dtfi, styles, ref result))
			{
				return result.parsedDate;
			}
			throw GetDateTimeParseException(ref result);
		}

		internal static DateTime Parse(string s, DateTimeFormatInfo dtfi, DateTimeStyles styles, out TimeSpan offset)
		{
			DateTimeResult result = default(DateTimeResult);
			result.Init();
			result.flags |= ParseFlags.CaptureOffset;
			if (TryParse(s, dtfi, styles, ref result))
			{
				offset = result.timeZoneOffset;
				return result.parsedDate;
			}
			throw GetDateTimeParseException(ref result);
		}

		internal static bool TryParse(string s, DateTimeFormatInfo dtfi, DateTimeStyles styles, out DateTime result)
		{
			result = DateTime.MinValue;
			DateTimeResult result2 = default(DateTimeResult);
			result2.Init();
			if (TryParse(s, dtfi, styles, ref result2))
			{
				result = result2.parsedDate;
				return true;
			}
			return false;
		}

		internal static bool TryParse(string s, DateTimeFormatInfo dtfi, DateTimeStyles styles, out DateTime result, out TimeSpan offset)
		{
			result = DateTime.MinValue;
			offset = TimeSpan.Zero;
			DateTimeResult result2 = default(DateTimeResult);
			result2.Init();
			result2.flags |= ParseFlags.CaptureOffset;
			if (TryParse(s, dtfi, styles, ref result2))
			{
				result = result2.parsedDate;
				offset = result2.timeZoneOffset;
				return true;
			}
			return false;
		}

		internal unsafe static bool TryParse(string s, DateTimeFormatInfo dtfi, DateTimeStyles styles, ref DateTimeResult result)
		{
			if (s == null)
			{
				result.SetFailure(ParseFailureKind.ArgumentNull, "ArgumentNull_String", null, "s");
				return false;
			}
			if (s.Length == 0)
			{
				result.SetFailure(ParseFailureKind.Format, "Format_BadDateTime", null);
				return false;
			}
			DS dS = DS.BEGIN;
			bool flag = false;
			DateTimeToken dtok = default(DateTimeToken);
			dtok.suffix = TokenType.SEP_Unk;
			DateTimeRawInfo raw = default(DateTimeRawInfo);
			int* numberBuffer = (int*)stackalloc byte[4 * 3];
			raw.Init(numberBuffer);
			result.calendar = dtfi.Calendar;
			result.era = 0;
			__DTString str = new __DTString(s, dtfi);
			str.GetNext();
			do
			{
				if (!Lex(dS, ref str, ref dtok, ref raw, ref result, ref dtfi))
				{
					return false;
				}
				if (dtok.dtt == DTT.Unk)
				{
					continue;
				}
				if (dtok.suffix != TokenType.SEP_Unk)
				{
					if (!ProcessDateTimeSuffix(ref result, ref raw, ref dtok))
					{
						result.SetFailure(ParseFailureKind.Format, "Format_BadDateTime", null);
						return false;
					}
					dtok.suffix = TokenType.SEP_Unk;
				}
				if (dtok.dtt == DTT.NumLocalTimeMark)
				{
					if (dS == DS.D_YNd || dS == DS.D_YN)
					{
						return ParseISO8601(ref raw, ref str, styles, ref result);
					}
					result.SetFailure(ParseFailureKind.Format, "Format_BadDateTime", null);
					return false;
				}
				dS = dateParsingStates[(int)dS][(int)dtok.dtt];
				if (dS == DS.ERROR)
				{
					result.SetFailure(ParseFailureKind.Format, "Format_BadDateTime", null);
					return false;
				}
				if (dS <= DS.ERROR)
				{
					continue;
				}
				if ((dtfi.FormatFlags & DateTimeFormatFlags.UseHebrewRule) != 0)
				{
					if (!ProcessHebrewTerminalState(dS, ref result, ref styles, ref raw, dtfi))
					{
						return false;
					}
				}
				else if (!ProcessTerminaltState(dS, ref result, ref styles, ref raw, dtfi))
				{
					return false;
				}
				flag = true;
				dS = DS.BEGIN;
			}
			while (dtok.dtt != 0 && dtok.dtt != DTT.NumEnd && dtok.dtt != DTT.MonthEnd);
			if (!flag)
			{
				result.SetFailure(ParseFailureKind.Format, "Format_BadDateTime", null);
				return false;
			}
			AdjustTimeMark(dtfi, ref raw);
			if (!AdjustHour(ref result.Hour, raw.timeMark))
			{
				result.SetFailure(ParseFailureKind.Format, "Format_BadDateTime", null);
				return false;
			}
			bool bTimeOnly = result.Year == -1 && result.Month == -1 && result.Day == -1;
			if (!CheckDefaultDateTime(ref result, ref result.calendar, styles))
			{
				return false;
			}
			if (!result.calendar.TryToDateTime(result.Year, result.Month, result.Day, result.Hour, result.Minute, result.Second, 0, result.era, out var result2))
			{
				result.SetFailure(ParseFailureKind.FormatBadDateTimeCalendar, "Format_BadDateTimeCalendar", null);
				return false;
			}
			if (raw.fraction > 0.0)
			{
				result2 = result2.AddTicks((long)Math.Round(raw.fraction * 10000000.0));
			}
			if (raw.dayOfWeek != -1 && raw.dayOfWeek != (int)result.calendar.GetDayOfWeek(result2))
			{
				result.SetFailure(ParseFailureKind.Format, "Format_BadDayOfWeek", null);
				return false;
			}
			result.parsedDate = result2;
			if (!DetermineTimeZoneAdjustments(ref result, styles, bTimeOnly))
			{
				return false;
			}
			return true;
		}

		private static bool DetermineTimeZoneAdjustments(ref DateTimeResult result, DateTimeStyles styles, bool bTimeOnly)
		{
			if ((result.flags & ParseFlags.CaptureOffset) != 0)
			{
				return DateTimeOffsetTimeZonePostProcessing(ref result, styles);
			}
			if ((result.flags & ParseFlags.TimeZoneUsed) == 0)
			{
				if ((styles & DateTimeStyles.AssumeLocal) != 0)
				{
					if ((styles & DateTimeStyles.AdjustToUniversal) == 0)
					{
						result.parsedDate = DateTime.SpecifyKind(result.parsedDate, DateTimeKind.Local);
						return true;
					}
					result.flags |= ParseFlags.TimeZoneUsed;
					result.timeZoneOffset = TimeZone.CurrentTimeZone.GetUtcOffset(result.parsedDate);
				}
				else
				{
					if ((styles & DateTimeStyles.AssumeUniversal) == 0)
					{
						return true;
					}
					if ((styles & DateTimeStyles.AdjustToUniversal) != 0)
					{
						result.parsedDate = DateTime.SpecifyKind(result.parsedDate, DateTimeKind.Utc);
						return true;
					}
					result.flags |= ParseFlags.TimeZoneUsed;
					result.timeZoneOffset = TimeSpan.Zero;
				}
			}
			if ((styles & DateTimeStyles.RoundtripKind) != 0 && (result.flags & ParseFlags.TimeZoneUtc) != 0)
			{
				result.parsedDate = DateTime.SpecifyKind(result.parsedDate, DateTimeKind.Utc);
				return true;
			}
			if ((styles & DateTimeStyles.AdjustToUniversal) != 0)
			{
				return AdjustTimeZoneToUniversal(ref result);
			}
			return AdjustTimeZoneToLocal(ref result, bTimeOnly);
		}

		private static bool DateTimeOffsetTimeZonePostProcessing(ref DateTimeResult result, DateTimeStyles styles)
		{
			if ((result.flags & ParseFlags.TimeZoneUsed) == 0)
			{
				if ((styles & DateTimeStyles.AssumeUniversal) != 0)
				{
					result.timeZoneOffset = TimeSpan.Zero;
				}
				else
				{
					result.timeZoneOffset = TimeZone.CurrentTimeZone.GetUtcOffset(result.parsedDate);
				}
			}
			long ticks = result.timeZoneOffset.Ticks;
			long num = result.parsedDate.Ticks - ticks;
			if (num < 0 || num > 3155378975999999999L)
			{
				result.SetFailure(ParseFailureKind.Format, "Format_UTCOutOfRange", null);
				return false;
			}
			if (ticks < -504000000000L || ticks > 504000000000L)
			{
				result.SetFailure(ParseFailureKind.Format, "Format_OffsetOutOfRange", null);
				return false;
			}
			if ((styles & DateTimeStyles.AdjustToUniversal) != 0)
			{
				if ((result.flags & ParseFlags.TimeZoneUsed) == 0 && (styles & DateTimeStyles.AssumeUniversal) == 0)
				{
					bool result2 = AdjustTimeZoneToUniversal(ref result);
					result.timeZoneOffset = TimeSpan.Zero;
					return result2;
				}
				result.parsedDate = new DateTime(num, DateTimeKind.Utc);
				result.timeZoneOffset = TimeSpan.Zero;
			}
			return true;
		}

		private static bool AdjustTimeZoneToUniversal(ref DateTimeResult result)
		{
			long ticks = result.parsedDate.Ticks;
			ticks -= result.timeZoneOffset.Ticks;
			if (ticks < 0)
			{
				ticks += 864000000000L;
			}
			if (ticks < 0 || ticks > 3155378975999999999L)
			{
				result.SetFailure(ParseFailureKind.Format, "Format_DateOutOfRange", null);
				return false;
			}
			result.parsedDate = new DateTime(ticks, DateTimeKind.Utc);
			return true;
		}

		private static bool AdjustTimeZoneToLocal(ref DateTimeResult result, bool bTimeOnly)
		{
			long ticks = result.parsedDate.Ticks;
			CurrentSystemTimeZone currentSystemTimeZone = (CurrentSystemTimeZone)TimeZone.CurrentTimeZone;
			bool isAmbiguousLocalDst = false;
			if (ticks < 864000000000L)
			{
				ticks -= result.timeZoneOffset.Ticks;
				ticks += currentSystemTimeZone.GetUtcOffset(bTimeOnly ? DateTime.Now : result.parsedDate).Ticks;
				if (ticks < 0)
				{
					ticks += 864000000000L;
				}
			}
			else
			{
				ticks -= result.timeZoneOffset.Ticks;
				ticks = ((ticks >= 0 && ticks <= 3155378975999999999L) ? (ticks + currentSystemTimeZone.GetUtcOffsetFromUniversalTime(new DateTime(ticks), ref isAmbiguousLocalDst)) : (ticks + currentSystemTimeZone.GetUtcOffset(result.parsedDate).Ticks));
			}
			if (ticks < 0 || ticks > 3155378975999999999L)
			{
				result.parsedDate = DateTime.MinValue;
				result.SetFailure(ParseFailureKind.Format, "Format_DateOutOfRange", null);
				return false;
			}
			result.parsedDate = new DateTime(ticks, DateTimeKind.Local, isAmbiguousLocalDst);
			return true;
		}

		private static bool ParseISO8601(ref DateTimeRawInfo raw, ref __DTString str, DateTimeStyles styles, ref DateTimeResult result)
		{
			if (raw.year >= 0 && raw.GetNumber(0) >= 0)
			{
				raw.GetNumber(1);
				_ = 0;
			}
			str.Index--;
			int result2 = 0;
			double result3 = 0.0;
			str.SkipWhiteSpaces();
			if (!ParseDigits(ref str, 2, out var result4))
			{
				result.SetFailure(ParseFailureKind.Format, "Format_BadDateTime", null);
				return false;
			}
			str.SkipWhiteSpaces();
			if (!str.Match(':'))
			{
				result.SetFailure(ParseFailureKind.Format, "Format_BadDateTime", null);
				return false;
			}
			str.SkipWhiteSpaces();
			if (!ParseDigits(ref str, 2, out var result5))
			{
				result.SetFailure(ParseFailureKind.Format, "Format_BadDateTime", null);
				return false;
			}
			str.SkipWhiteSpaces();
			if (str.Match(':'))
			{
				str.SkipWhiteSpaces();
				if (!ParseDigits(ref str, 2, out result2))
				{
					result.SetFailure(ParseFailureKind.Format, "Format_BadDateTime", null);
					return false;
				}
				if (str.Match('.'))
				{
					if (!ParseFraction(ref str, out result3))
					{
						result.SetFailure(ParseFailureKind.Format, "Format_BadDateTime", null);
						return false;
					}
					str.Index--;
				}
				str.SkipWhiteSpaces();
			}
			if (str.GetNext())
			{
				switch (str.GetChar())
				{
				case '+':
				case '-':
					result.flags |= ParseFlags.TimeZoneUsed;
					if (!ParseTimeZone(ref str, ref result.timeZoneOffset))
					{
						result.SetFailure(ParseFailureKind.Format, "Format_BadDateTime", null);
						return false;
					}
					break;
				case 'Z':
				case 'z':
					result.flags |= ParseFlags.TimeZoneUsed;
					result.timeZoneOffset = TimeSpan.Zero;
					result.flags |= ParseFlags.TimeZoneUtc;
					break;
				default:
					str.Index--;
					break;
				}
				str.SkipWhiteSpaces();
				if (str.Match('#'))
				{
					if (!VerifyValidPunctuation(ref str))
					{
						result.SetFailure(ParseFailureKind.Format, "Format_BadDateTime", null);
						return false;
					}
					str.SkipWhiteSpaces();
				}
				if (str.Match('\0') && !VerifyValidPunctuation(ref str))
				{
					result.SetFailure(ParseFailureKind.Format, "Format_BadDateTime", null);
					return false;
				}
				if (str.GetNext())
				{
					result.SetFailure(ParseFailureKind.Format, "Format_BadDateTime", null);
					return false;
				}
			}
			Calendar defaultInstance = GregorianCalendar.GetDefaultInstance();
			if (!defaultInstance.TryToDateTime(raw.year, raw.GetNumber(0), raw.GetNumber(1), result4, result5, result2, 0, result.era, out var result6))
			{
				result.SetFailure(ParseFailureKind.FormatBadDateTimeCalendar, "Format_BadDateTimeCalendar", null);
				return false;
			}
			result6 = (result.parsedDate = result6.AddTicks((long)Math.Round(result3 * 10000000.0)));
			if (!DetermineTimeZoneAdjustments(ref result, styles, bTimeOnly: false))
			{
				return false;
			}
			return true;
		}

		internal static bool MatchHebrewDigits(ref __DTString str, int digitLen, out int number)
		{
			number = 0;
			HebrewNumberParsingContext context = new HebrewNumberParsingContext(0);
			HebrewNumberParsingState hebrewNumberParsingState = HebrewNumberParsingState.ContinueParsing;
			while (hebrewNumberParsingState == HebrewNumberParsingState.ContinueParsing && str.GetNext())
			{
				hebrewNumberParsingState = HebrewNumber.ParseByChar(str.GetChar(), ref context);
			}
			if (hebrewNumberParsingState == HebrewNumberParsingState.FoundEndOfHebrewNumber)
			{
				number = context.result;
				return true;
			}
			return false;
		}

		internal static bool ParseDigits(ref __DTString str, int digitLen, out int result)
		{
			if (digitLen == 1)
			{
				return ParseDigits(ref str, 1, 2, out result);
			}
			return ParseDigits(ref str, digitLen, digitLen, out result);
		}

		internal static bool ParseDigits(ref __DTString str, int minDigitLen, int maxDigitLen, out int result)
		{
			result = 0;
			int index = str.Index;
			int i;
			for (i = 0; i < maxDigitLen; i++)
			{
				if (!str.GetNextDigit())
				{
					str.Index--;
					break;
				}
				result = result * 10 + str.GetDigit();
			}
			if (i < minDigitLen)
			{
				str.Index = index;
				return false;
			}
			return true;
		}

		private static bool ParseFractionExact(ref __DTString str, int maxDigitLen, ref double result)
		{
			if (!str.GetNextDigit())
			{
				str.Index--;
				return false;
			}
			result = str.GetDigit();
			int i;
			for (i = 1; i < maxDigitLen; i++)
			{
				if (!str.GetNextDigit())
				{
					str.Index--;
					break;
				}
				result = result * 10.0 + (double)str.GetDigit();
			}
			result /= Math.Pow(10.0, i);
			return i == maxDigitLen;
		}

		private static bool ParseSign(ref __DTString str, ref bool result)
		{
			if (!str.GetNext())
			{
				return false;
			}
			switch (str.GetChar())
			{
			case '+':
				result = true;
				return true;
			case '-':
				result = false;
				return true;
			default:
				return false;
			}
		}

		private static bool ParseTimeZoneOffset(ref __DTString str, int len, ref TimeSpan result)
		{
			bool result2 = true;
			int result3 = 0;
			int result4;
			switch (len)
			{
			case 1:
			case 2:
				if (!ParseSign(ref str, ref result2))
				{
					return false;
				}
				if (!ParseDigits(ref str, len, out result4))
				{
					return false;
				}
				break;
			default:
				if (!ParseSign(ref str, ref result2))
				{
					return false;
				}
				if (!ParseDigits(ref str, 1, out result4))
				{
					return false;
				}
				if (str.Match(":"))
				{
					if (!ParseDigits(ref str, 2, out result3))
					{
						return false;
					}
					break;
				}
				str.Index--;
				if (!ParseDigits(ref str, 2, out result3))
				{
					return false;
				}
				break;
			}
			if (result3 < 0 || result3 >= 60)
			{
				return false;
			}
			result = new TimeSpan(result4, result3, 0);
			if (!result2)
			{
				result = result.Negate();
			}
			return true;
		}

		private static bool MatchAbbreviatedMonthName(ref __DTString str, DateTimeFormatInfo dtfi, ref int result)
		{
			int maxMatchStrLen = 0;
			result = -1;
			if (str.GetNext())
			{
				int num = ((dtfi.GetMonthName(13).Length == 0) ? 12 : 13);
				for (int i = 1; i <= num; i++)
				{
					string abbreviatedMonthName = dtfi.GetAbbreviatedMonthName(i);
					int matchLength = abbreviatedMonthName.Length;
					if ((dtfi.HasSpacesInMonthNames ? str.MatchSpecifiedWords(abbreviatedMonthName, checkWordBoundary: false, ref matchLength) : str.MatchSpecifiedWord(abbreviatedMonthName)) && matchLength > maxMatchStrLen)
					{
						maxMatchStrLen = matchLength;
						result = i;
					}
				}
				if ((dtfi.FormatFlags & DateTimeFormatFlags.UseLeapYearMonth) != 0)
				{
					int num2 = str.MatchLongestWords(dtfi.internalGetLeapYearMonthNames(), ref maxMatchStrLen);
					if (num2 >= 0)
					{
						result = num2 + 1;
					}
				}
			}
			if (result > 0)
			{
				str.Index += maxMatchStrLen - 1;
				return true;
			}
			return false;
		}

		private static bool MatchMonthName(ref __DTString str, DateTimeFormatInfo dtfi, ref int result)
		{
			int maxMatchStrLen = 0;
			result = -1;
			if (str.GetNext())
			{
				int num = ((dtfi.GetMonthName(13).Length == 0) ? 12 : 13);
				for (int i = 1; i <= num; i++)
				{
					string monthName = dtfi.GetMonthName(i);
					int matchLength = monthName.Length;
					if ((dtfi.HasSpacesInMonthNames ? str.MatchSpecifiedWords(monthName, checkWordBoundary: false, ref matchLength) : str.MatchSpecifiedWord(monthName)) && matchLength > maxMatchStrLen)
					{
						maxMatchStrLen = matchLength;
						result = i;
					}
				}
				if ((dtfi.FormatFlags & DateTimeFormatFlags.UseGenitiveMonth) != 0)
				{
					int num2 = str.MatchLongestWords(dtfi.MonthGenitiveNames, ref maxMatchStrLen);
					if (num2 >= 0)
					{
						result = num2 + 1;
					}
				}
				if ((dtfi.FormatFlags & DateTimeFormatFlags.UseLeapYearMonth) != 0)
				{
					int num3 = str.MatchLongestWords(dtfi.internalGetLeapYearMonthNames(), ref maxMatchStrLen);
					if (num3 >= 0)
					{
						result = num3 + 1;
					}
				}
			}
			if (result > 0)
			{
				str.Index += maxMatchStrLen - 1;
				return true;
			}
			return false;
		}

		private static bool MatchAbbreviatedDayName(ref __DTString str, DateTimeFormatInfo dtfi, ref int result)
		{
			int num = 0;
			result = -1;
			if (str.GetNext())
			{
				for (DayOfWeek dayOfWeek = DayOfWeek.Sunday; dayOfWeek <= DayOfWeek.Saturday; dayOfWeek++)
				{
					string abbreviatedDayName = dtfi.GetAbbreviatedDayName(dayOfWeek);
					int matchLength = abbreviatedDayName.Length;
					if ((dtfi.HasSpacesInDayNames ? str.MatchSpecifiedWords(abbreviatedDayName, checkWordBoundary: false, ref matchLength) : str.MatchSpecifiedWord(abbreviatedDayName)) && matchLength > num)
					{
						num = matchLength;
						result = (int)dayOfWeek;
					}
				}
			}
			if (result >= 0)
			{
				str.Index += num - 1;
				return true;
			}
			return false;
		}

		private static bool MatchDayName(ref __DTString str, DateTimeFormatInfo dtfi, ref int result)
		{
			int num = 0;
			result = -1;
			if (str.GetNext())
			{
				for (DayOfWeek dayOfWeek = DayOfWeek.Sunday; dayOfWeek <= DayOfWeek.Saturday; dayOfWeek++)
				{
					string dayName = dtfi.GetDayName(dayOfWeek);
					int matchLength = dayName.Length;
					if ((dtfi.HasSpacesInDayNames ? str.MatchSpecifiedWords(dayName, checkWordBoundary: false, ref matchLength) : str.MatchSpecifiedWord(dayName)) && matchLength > num)
					{
						num = matchLength;
						result = (int)dayOfWeek;
					}
				}
			}
			if (result >= 0)
			{
				str.Index += num - 1;
				return true;
			}
			return false;
		}

		private static bool MatchEraName(ref __DTString str, DateTimeFormatInfo dtfi, ref int result)
		{
			if (str.GetNext())
			{
				int[] eras = dtfi.Calendar.Eras;
				if (eras != null)
				{
					for (int i = 0; i < eras.Length; i++)
					{
						string eraName = dtfi.GetEraName(eras[i]);
						if (str.MatchSpecifiedWord(eraName))
						{
							str.Index += eraName.Length - 1;
							result = eras[i];
							return true;
						}
						eraName = dtfi.GetAbbreviatedEraName(eras[i]);
						if (str.MatchSpecifiedWord(eraName))
						{
							str.Index += eraName.Length - 1;
							result = eras[i];
							return true;
						}
					}
				}
			}
			return false;
		}

		private static bool MatchTimeMark(ref __DTString str, DateTimeFormatInfo dtfi, ref TM result)
		{
			result = TM.NotSet;
			if (dtfi.AMDesignator.Length == 0)
			{
				result = TM.AM;
			}
			if (dtfi.PMDesignator.Length == 0)
			{
				result = TM.PM;
			}
			if (str.GetNext())
			{
				string aMDesignator = dtfi.AMDesignator;
				if (aMDesignator.Length > 0 && str.MatchSpecifiedWord(aMDesignator))
				{
					str.Index += aMDesignator.Length - 1;
					result = TM.AM;
					return true;
				}
				aMDesignator = dtfi.PMDesignator;
				if (aMDesignator.Length > 0 && str.MatchSpecifiedWord(aMDesignator))
				{
					str.Index += aMDesignator.Length - 1;
					result = TM.PM;
					return true;
				}
				str.Index--;
			}
			if (result != TM.NotSet)
			{
				return true;
			}
			return false;
		}

		private static bool MatchAbbreviatedTimeMark(ref __DTString str, DateTimeFormatInfo dtfi, ref TM result)
		{
			if (str.GetNext())
			{
				if (str.GetChar() == dtfi.AMDesignator[0])
				{
					result = TM.AM;
					return true;
				}
				if (str.GetChar() == dtfi.PMDesignator[0])
				{
					result = TM.PM;
					return true;
				}
			}
			return false;
		}

		private static bool CheckNewValue(ref int currentValue, int newValue, char patternChar, ref DateTimeResult result)
		{
			if (currentValue == -1)
			{
				currentValue = newValue;
				return true;
			}
			if (newValue != currentValue)
			{
				result.SetFailure(ParseFailureKind.FormatWithParameter, "Format_RepeatDateTimePattern", patternChar);
				return false;
			}
			return true;
		}

		private static DateTime GetDateTimeNow(ref DateTimeResult result, ref DateTimeStyles styles)
		{
			if ((result.flags & ParseFlags.CaptureOffset) != 0)
			{
				if ((result.flags & ParseFlags.TimeZoneUsed) != 0)
				{
					return new DateTime(DateTime.UtcNow.Ticks + result.timeZoneOffset.Ticks, DateTimeKind.Unspecified);
				}
				if ((styles & DateTimeStyles.AssumeUniversal) != 0)
				{
					return DateTime.UtcNow;
				}
			}
			return DateTime.Now;
		}

		private static bool CheckDefaultDateTime(ref DateTimeResult result, ref Calendar cal, DateTimeStyles styles)
		{
			if ((result.flags & ParseFlags.CaptureOffset) != 0 && (result.Month != -1 || result.Day != -1) && (result.Year == -1 || (result.flags & ParseFlags.YearDefault) != 0) && (result.flags & ParseFlags.TimeZoneUsed) != 0)
			{
				result.SetFailure(ParseFailureKind.Format, "Format_MissingIncompleteDate", null);
				return false;
			}
			if (result.Year == -1 || result.Month == -1 || result.Day == -1)
			{
				DateTime dateTimeNow = GetDateTimeNow(ref result, ref styles);
				if (result.Month == -1 && result.Day == -1)
				{
					if (result.Year == -1)
					{
						if ((styles & DateTimeStyles.NoCurrentDateDefault) != 0)
						{
							cal = GregorianCalendar.GetDefaultInstance();
							result.Year = (result.Month = (result.Day = 1));
						}
						else
						{
							result.Year = cal.GetYear(dateTimeNow);
							result.Month = cal.GetMonth(dateTimeNow);
							result.Day = cal.GetDayOfMonth(dateTimeNow);
						}
					}
					else
					{
						result.Month = 1;
						result.Day = 1;
					}
				}
				else
				{
					if (result.Year == -1)
					{
						result.Year = cal.GetYear(dateTimeNow);
					}
					if (result.Month == -1)
					{
						result.Month = 1;
					}
					if (result.Day == -1)
					{
						result.Day = 1;
					}
				}
			}
			if (result.Hour == -1)
			{
				result.Hour = 0;
			}
			if (result.Minute == -1)
			{
				result.Minute = 0;
			}
			if (result.Second == -1)
			{
				result.Second = 0;
			}
			if (result.era == -1)
			{
				result.era = 0;
			}
			return true;
		}

		private static string ExpandPredefinedFormat(string format, ref DateTimeFormatInfo dtfi, ref ParsingInfo parseInfo, ref DateTimeResult result)
		{
			switch (format[0])
			{
			case 'O':
			case 'o':
				parseInfo.calendar = GregorianCalendar.GetDefaultInstance();
				dtfi = DateTimeFormatInfo.InvariantInfo;
				break;
			case 'R':
			case 'r':
				parseInfo.calendar = GregorianCalendar.GetDefaultInstance();
				dtfi = DateTimeFormatInfo.InvariantInfo;
				if ((result.flags & ParseFlags.CaptureOffset) != 0)
				{
					result.flags |= ParseFlags.Rfc1123Pattern;
				}
				break;
			case 's':
				dtfi = DateTimeFormatInfo.InvariantInfo;
				parseInfo.calendar = GregorianCalendar.GetDefaultInstance();
				break;
			case 'u':
				parseInfo.calendar = GregorianCalendar.GetDefaultInstance();
				dtfi = DateTimeFormatInfo.InvariantInfo;
				if ((result.flags & ParseFlags.CaptureOffset) != 0)
				{
					result.flags |= ParseFlags.UtcSortPattern;
				}
				break;
			case 'U':
				parseInfo.calendar = GregorianCalendar.GetDefaultInstance();
				result.flags |= ParseFlags.TimeZoneUsed;
				result.timeZoneOffset = new TimeSpan(0L);
				result.flags |= ParseFlags.TimeZoneUtc;
				if (dtfi.Calendar.GetType() != typeof(GregorianCalendar))
				{
					dtfi = (DateTimeFormatInfo)dtfi.Clone();
					dtfi.Calendar = GregorianCalendar.GetDefaultInstance();
				}
				break;
			}
			return DateTimeFormat.GetRealFormat(format, dtfi);
		}

		private static bool ParseJapaneseEraStart(ref __DTString str, DateTimeFormatInfo dtfi)
		{
			if (GregorianCalendarHelper.EnforceLegacyJapaneseDateParsing || dtfi.Calendar.ID != 3 || !str.GetNext())
			{
				return false;
			}
			if (str.m_current != "元"[0])
			{
				str.Index--;
				return false;
			}
			return true;
		}

		private static bool ParseByFormat(ref __DTString str, ref __DTString format, ref ParsingInfo parseInfo, DateTimeFormatInfo dtfi, ref DateTimeResult result)
		{
			int returnValue = 0;
			int result2 = 0;
			int result3 = 0;
			int result4 = 0;
			int result5 = 0;
			int result6 = 0;
			int result7 = 0;
			int result8 = 0;
			double result9 = 0.0;
			TM result10 = TM.AM;
			char @char = format.GetChar();
			switch (@char)
			{
			case 'y':
			{
				returnValue = format.GetRepeatCount();
				bool flag;
				if (ParseJapaneseEraStart(ref str, dtfi))
				{
					result2 = 1;
					flag = true;
				}
				else if (dtfi.HasForceTwoDigitYears)
				{
					flag = ParseDigits(ref str, 1, 4, out result2);
				}
				else
				{
					if (returnValue <= 2)
					{
						parseInfo.fUseTwoDigitYear = true;
					}
					flag = ParseDigits(ref str, returnValue, out result2);
				}
				if (!flag && parseInfo.fCustomNumberParser)
				{
					flag = parseInfo.parseNumberDelegate(ref str, returnValue, out result2);
				}
				if (!flag)
				{
					result.SetFailure(ParseFailureKind.Format, "Format_BadDateTime", null);
					return false;
				}
				if (!CheckNewValue(ref result.Year, result2, @char, ref result))
				{
					return false;
				}
				break;
			}
			case 'M':
				returnValue = format.GetRepeatCount();
				if (returnValue <= 2)
				{
					if (!ParseDigits(ref str, returnValue, out result3) && (!parseInfo.fCustomNumberParser || !parseInfo.parseNumberDelegate(ref str, returnValue, out result3)))
					{
						result.SetFailure(ParseFailureKind.Format, "Format_BadDateTime", null);
						return false;
					}
				}
				else
				{
					if (returnValue == 3)
					{
						if (!MatchAbbreviatedMonthName(ref str, dtfi, ref result3))
						{
							result.SetFailure(ParseFailureKind.Format, "Format_BadDateTime", null);
							return false;
						}
					}
					else if (!MatchMonthName(ref str, dtfi, ref result3))
					{
						result.SetFailure(ParseFailureKind.Format, "Format_BadDateTime", null);
						return false;
					}
					result.flags |= ParseFlags.ParsedMonthName;
				}
				if (!CheckNewValue(ref result.Month, result3, @char, ref result))
				{
					return false;
				}
				break;
			case 'd':
				returnValue = format.GetRepeatCount();
				if (returnValue <= 2)
				{
					if (!ParseDigits(ref str, returnValue, out result4) && (!parseInfo.fCustomNumberParser || !parseInfo.parseNumberDelegate(ref str, returnValue, out result4)))
					{
						result.SetFailure(ParseFailureKind.Format, "Format_BadDateTime", null);
						return false;
					}
					if (!CheckNewValue(ref result.Day, result4, @char, ref result))
					{
						return false;
					}
					break;
				}
				if (returnValue == 3)
				{
					if (!MatchAbbreviatedDayName(ref str, dtfi, ref result5))
					{
						result.SetFailure(ParseFailureKind.Format, "Format_BadDateTime", null);
						return false;
					}
				}
				else if (!MatchDayName(ref str, dtfi, ref result5))
				{
					result.SetFailure(ParseFailureKind.Format, "Format_BadDateTime", null);
					return false;
				}
				if (!CheckNewValue(ref parseInfo.dayOfWeek, result5, @char, ref result))
				{
					return false;
				}
				break;
			case 'g':
				returnValue = format.GetRepeatCount();
				if (!MatchEraName(ref str, dtfi, ref result.era))
				{
					result.SetFailure(ParseFailureKind.Format, "Format_BadDateTime", null);
					return false;
				}
				break;
			case 'h':
				parseInfo.fUseHour12 = true;
				returnValue = format.GetRepeatCount();
				if (!ParseDigits(ref str, (returnValue < 2) ? 1 : 2, out result6))
				{
					result.SetFailure(ParseFailureKind.Format, "Format_BadDateTime", null);
					return false;
				}
				if (!CheckNewValue(ref result.Hour, result6, @char, ref result))
				{
					return false;
				}
				break;
			case 'H':
				returnValue = format.GetRepeatCount();
				if (!ParseDigits(ref str, (returnValue < 2) ? 1 : 2, out result6))
				{
					result.SetFailure(ParseFailureKind.Format, "Format_BadDateTime", null);
					return false;
				}
				if (!CheckNewValue(ref result.Hour, result6, @char, ref result))
				{
					return false;
				}
				break;
			case 'm':
				returnValue = format.GetRepeatCount();
				if (!ParseDigits(ref str, (returnValue < 2) ? 1 : 2, out result7))
				{
					result.SetFailure(ParseFailureKind.Format, "Format_BadDateTime", null);
					return false;
				}
				if (!CheckNewValue(ref result.Minute, result7, @char, ref result))
				{
					return false;
				}
				break;
			case 's':
				returnValue = format.GetRepeatCount();
				if (!ParseDigits(ref str, (returnValue < 2) ? 1 : 2, out result8))
				{
					result.SetFailure(ParseFailureKind.Format, "Format_BadDateTime", null);
					return false;
				}
				if (!CheckNewValue(ref result.Second, result8, @char, ref result))
				{
					return false;
				}
				break;
			case 'F':
			case 'f':
				returnValue = format.GetRepeatCount();
				if (returnValue <= 7)
				{
					if (!ParseFractionExact(ref str, returnValue, ref result9) && @char == 'f')
					{
						result.SetFailure(ParseFailureKind.Format, "Format_BadDateTime", null);
						return false;
					}
					if (result.fraction < 0.0)
					{
						result.fraction = result9;
					}
					else if (result9 != result.fraction)
					{
						result.SetFailure(ParseFailureKind.FormatWithParameter, "Format_RepeatDateTimePattern", @char);
						return false;
					}
					break;
				}
				result.SetFailure(ParseFailureKind.Format, "Format_BadDateTime", null);
				return false;
			case 't':
				returnValue = format.GetRepeatCount();
				if (returnValue == 1)
				{
					if (!MatchAbbreviatedTimeMark(ref str, dtfi, ref result10))
					{
						result.SetFailure(ParseFailureKind.Format, "Format_BadDateTime", null);
						return false;
					}
				}
				else if (!MatchTimeMark(ref str, dtfi, ref result10))
				{
					result.SetFailure(ParseFailureKind.Format, "Format_BadDateTime", null);
					return false;
				}
				if (parseInfo.timeMark == TM.NotSet)
				{
					parseInfo.timeMark = result10;
				}
				else if (parseInfo.timeMark != result10)
				{
					result.SetFailure(ParseFailureKind.FormatWithParameter, "Format_RepeatDateTimePattern", @char);
					return false;
				}
				break;
			case 'z':
			{
				returnValue = format.GetRepeatCount();
				TimeSpan result12 = new TimeSpan(0L);
				if (!ParseTimeZoneOffset(ref str, returnValue, ref result12))
				{
					result.SetFailure(ParseFailureKind.Format, "Format_BadDateTime", null);
					return false;
				}
				if ((result.flags & ParseFlags.TimeZoneUsed) != 0 && result12 != result.timeZoneOffset)
				{
					result.SetFailure(ParseFailureKind.FormatWithParameter, "Format_RepeatDateTimePattern", 'z');
					return false;
				}
				result.timeZoneOffset = result12;
				result.flags |= ParseFlags.TimeZoneUsed;
				break;
			}
			case 'Z':
				if ((result.flags & ParseFlags.TimeZoneUsed) != 0 && result.timeZoneOffset != TimeSpan.Zero)
				{
					result.SetFailure(ParseFailureKind.FormatWithParameter, "Format_RepeatDateTimePattern", 'Z');
					return false;
				}
				result.flags |= ParseFlags.TimeZoneUsed;
				result.timeZoneOffset = new TimeSpan(0L);
				result.flags |= ParseFlags.TimeZoneUtc;
				str.Index++;
				if (!GetTimeZoneName(ref str))
				{
					result.SetFailure(ParseFailureKind.Format, "Format_BadDateTime", null);
					return false;
				}
				str.Index--;
				break;
			case 'K':
				if (str.Match('Z'))
				{
					if ((result.flags & ParseFlags.TimeZoneUsed) != 0 && result.timeZoneOffset != TimeSpan.Zero)
					{
						result.SetFailure(ParseFailureKind.FormatWithParameter, "Format_RepeatDateTimePattern", 'K');
						return false;
					}
					result.flags |= ParseFlags.TimeZoneUsed;
					result.timeZoneOffset = new TimeSpan(0L);
					result.flags |= ParseFlags.TimeZoneUtc;
				}
				else if (str.Match('+') || str.Match('-'))
				{
					str.Index--;
					TimeSpan result11 = new TimeSpan(0L);
					if (!ParseTimeZoneOffset(ref str, 3, ref result11))
					{
						result.SetFailure(ParseFailureKind.Format, "Format_BadDateTime", null);
						return false;
					}
					if ((result.flags & ParseFlags.TimeZoneUsed) != 0 && result11 != result.timeZoneOffset)
					{
						result.SetFailure(ParseFailureKind.FormatWithParameter, "Format_RepeatDateTimePattern", 'K');
						return false;
					}
					result.timeZoneOffset = result11;
					result.flags |= ParseFlags.TimeZoneUsed;
				}
				break;
			case ':':
				if (!str.Match(dtfi.TimeSeparator))
				{
					result.SetFailure(ParseFailureKind.Format, "Format_BadDateTime", null);
					return false;
				}
				break;
			case '/':
				if (!str.Match(dtfi.DateSeparator))
				{
					result.SetFailure(ParseFailureKind.Format, "Format_BadDateTime", null);
					return false;
				}
				break;
			case '"':
			case '\'':
			{
				StringBuilder stringBuilder = new StringBuilder();
				if (!TryParseQuoteString(format.Value, format.Index, stringBuilder, out returnValue))
				{
					result.SetFailure(ParseFailureKind.FormatWithParameter, "Format_BadQuote", @char);
					return false;
				}
				format.Index += returnValue - 1;
				string text = stringBuilder.ToString();
				for (int i = 0; i < text.Length; i++)
				{
					if (text[i] == ' ' && parseInfo.fAllowInnerWhite)
					{
						str.SkipWhiteSpaces();
					}
					else if (!str.Match(text[i]))
					{
						result.SetFailure(ParseFailureKind.Format, "Format_BadDateTime", null);
						return false;
					}
				}
				if ((result.flags & ParseFlags.CaptureOffset) != 0)
				{
					if ((result.flags & ParseFlags.Rfc1123Pattern) != 0 && text == "GMT")
					{
						result.flags |= ParseFlags.TimeZoneUsed;
						result.timeZoneOffset = TimeSpan.Zero;
					}
					else if ((result.flags & ParseFlags.UtcSortPattern) != 0 && text == "Z")
					{
						result.flags |= ParseFlags.TimeZoneUsed;
						result.timeZoneOffset = TimeSpan.Zero;
					}
				}
				break;
			}
			case '%':
				if (format.Index >= format.Value.Length - 1 || format.Value[format.Index + 1] == '%')
				{
					result.SetFailure(ParseFailureKind.Format, "Format_BadFormatSpecifier", null);
					return false;
				}
				break;
			case '\\':
				if (format.GetNext())
				{
					if (!str.Match(format.GetChar()))
					{
						result.SetFailure(ParseFailureKind.Format, "Format_BadDateTime", null);
						return false;
					}
					break;
				}
				result.SetFailure(ParseFailureKind.Format, "Format_BadFormatSpecifier", null);
				return false;
			case '.':
				if (!str.Match(@char))
				{
					if (!format.GetNext() || !format.Match('F'))
					{
						result.SetFailure(ParseFailureKind.Format, "Format_BadDateTime", null);
						return false;
					}
					format.GetRepeatCount();
				}
				break;
			default:
				if (@char == ' ')
				{
					if (!parseInfo.fAllowInnerWhite && !str.Match(@char))
					{
						if (parseInfo.fAllowTrailingWhite && format.GetNext() && ParseByFormat(ref str, ref format, ref parseInfo, dtfi, ref result))
						{
							return true;
						}
						result.SetFailure(ParseFailureKind.Format, "Format_BadDateTime", null);
						return false;
					}
				}
				else if (format.MatchSpecifiedWord("GMT"))
				{
					format.Index += "GMT".Length - 1;
					result.flags |= ParseFlags.TimeZoneUsed;
					result.timeZoneOffset = TimeSpan.Zero;
					if (!str.Match("GMT"))
					{
						result.SetFailure(ParseFailureKind.Format, "Format_BadDateTime", null);
						return false;
					}
				}
				else if (!str.Match(@char))
				{
					result.SetFailure(ParseFailureKind.Format, "Format_BadDateTime", null);
					return false;
				}
				break;
			}
			return true;
		}

		internal static bool TryParseQuoteString(string format, int pos, StringBuilder result, out int returnValue)
		{
			returnValue = 0;
			int length = format.Length;
			int num = pos;
			char c = format[pos++];
			bool flag = false;
			while (pos < length)
			{
				char c2 = format[pos++];
				if (c2 == c)
				{
					flag = true;
					break;
				}
				if (c2 == '\\')
				{
					if (pos >= length)
					{
						return false;
					}
					result.Append(format[pos++]);
				}
				else
				{
					result.Append(c2);
				}
			}
			if (!flag)
			{
				return false;
			}
			returnValue = pos - num;
			return true;
		}

		private static bool DoStrictParse(string s, string formatParam, DateTimeStyles styles, DateTimeFormatInfo dtfi, ref DateTimeResult result)
		{
			bool flag = false;
			ParsingInfo parseInfo = default(ParsingInfo);
			parseInfo.Init();
			parseInfo.calendar = dtfi.Calendar;
			parseInfo.fAllowInnerWhite = (styles & DateTimeStyles.AllowInnerWhite) != 0;
			parseInfo.fAllowTrailingWhite = (styles & DateTimeStyles.AllowTrailingWhite) != 0;
			if (formatParam.Length == 1)
			{
				if ((result.flags & ParseFlags.CaptureOffset) != 0 && formatParam[0] == 'U')
				{
					result.SetFailure(ParseFailureKind.Format, "Format_BadFormatSpecifier", null);
					return false;
				}
				formatParam = ExpandPredefinedFormat(formatParam, ref dtfi, ref parseInfo, ref result);
			}
			result.calendar = parseInfo.calendar;
			if (parseInfo.calendar.ID == 8)
			{
				parseInfo.parseNumberDelegate = m_hebrewNumberParser;
				parseInfo.fCustomNumberParser = true;
			}
			result.Hour = (result.Minute = (result.Second = -1));
			__DTString format = new __DTString(formatParam, dtfi, checkDigitToken: false);
			__DTString str = new __DTString(s, dtfi, checkDigitToken: false);
			if (parseInfo.fAllowTrailingWhite)
			{
				format.TrimTail();
				format.RemoveTrailingInQuoteSpaces();
				str.TrimTail();
			}
			if ((styles & DateTimeStyles.AllowLeadingWhite) != 0)
			{
				format.SkipWhiteSpaces();
				format.RemoveLeadingInQuoteSpaces();
				str.SkipWhiteSpaces();
			}
			while (format.GetNext())
			{
				if (parseInfo.fAllowInnerWhite)
				{
					str.SkipWhiteSpaces();
				}
				if (!ParseByFormat(ref str, ref format, ref parseInfo, dtfi, ref result))
				{
					return false;
				}
			}
			if (str.Index < str.Value.Length - 1)
			{
				result.SetFailure(ParseFailureKind.Format, "Format_BadDateTime", null);
				return false;
			}
			if (parseInfo.fUseTwoDigitYear && (dtfi.FormatFlags & DateTimeFormatFlags.UseHebrewRule) == 0)
			{
				if (result.Year >= 100)
				{
					result.SetFailure(ParseFailureKind.Format, "Format_BadDateTime", null);
					return false;
				}
				result.Year = parseInfo.calendar.ToFourDigitYear(result.Year);
			}
			if (parseInfo.fUseHour12)
			{
				if (parseInfo.timeMark == TM.NotSet)
				{
					parseInfo.timeMark = TM.AM;
				}
				if (result.Hour > 12)
				{
					result.SetFailure(ParseFailureKind.Format, "Format_BadDateTime", null);
					return false;
				}
				if (parseInfo.timeMark == TM.AM)
				{
					if (result.Hour == 12)
					{
						result.Hour = 0;
					}
				}
				else
				{
					result.Hour = ((result.Hour == 12) ? 12 : (result.Hour + 12));
				}
			}
			flag = result.Year == -1 && result.Month == -1 && result.Day == -1;
			if (!CheckDefaultDateTime(ref result, ref parseInfo.calendar, styles))
			{
				return false;
			}
			if (!flag && dtfi.HasYearMonthAdjustment && !dtfi.YearMonthAdjustment(ref result.Year, ref result.Month, (result.flags & ParseFlags.ParsedMonthName) != 0))
			{
				result.SetFailure(ParseFailureKind.FormatBadDateTimeCalendar, "Format_BadDateTimeCalendar", null);
				return false;
			}
			if (!parseInfo.calendar.TryToDateTime(result.Year, result.Month, result.Day, result.Hour, result.Minute, result.Second, 0, result.era, out result.parsedDate))
			{
				result.SetFailure(ParseFailureKind.FormatBadDateTimeCalendar, "Format_BadDateTimeCalendar", null);
				return false;
			}
			if (result.fraction > 0.0)
			{
				result.parsedDate = result.parsedDate.AddTicks((long)Math.Round(result.fraction * 10000000.0));
			}
			if (parseInfo.dayOfWeek != -1 && parseInfo.dayOfWeek != (int)parseInfo.calendar.GetDayOfWeek(result.parsedDate))
			{
				result.SetFailure(ParseFailureKind.Format, "Format_BadDayOfWeek", null);
				return false;
			}
			if (!DetermineTimeZoneAdjustments(ref result, styles, flag))
			{
				return false;
			}
			return true;
		}

		private static Exception GetDateTimeParseException(ref DateTimeResult result)
		{
			return result.failure switch
			{
				ParseFailureKind.ArgumentNull => new ArgumentNullException(result.failureArgumentName, Environment.GetResourceString(result.failureMessageID)), 
				ParseFailureKind.Format => new FormatException(Environment.GetResourceString(result.failureMessageID)), 
				ParseFailureKind.FormatWithParameter => new FormatException(Environment.GetResourceString(result.failureMessageID, result.failureMessageFormatArgument)), 
				ParseFailureKind.FormatBadDateTimeCalendar => new FormatException(Environment.GetResourceString(result.failureMessageID, result.calendar)), 
				_ => null, 
			};
		}
	}
}
