using System.Text;

namespace System.Globalization
{
	internal class HebrewNumber
	{
		private enum HebrewToken
		{
			Invalid = -1,
			Digit400,
			Digit200_300,
			Digit100,
			Digit10,
			Digit1,
			Digit6_7,
			Digit7,
			Digit9,
			SingleQuote,
			DoubleQuote
		}

		private class HebrewValue
		{
			internal HebrewToken token;

			internal int value;

			internal HebrewValue(HebrewToken token, int value)
			{
				this.token = token;
				this.value = value;
			}
		}

		internal enum HS
		{
			_err = -1,
			Start = 0,
			S400 = 1,
			S400_400 = 2,
			S400_X00 = 3,
			S400_X0 = 4,
			X00_DQ = 5,
			S400_X00_X0 = 6,
			X0_DQ = 7,
			X = 8,
			X0 = 9,
			X00 = 10,
			S400_DQ = 11,
			S400_400_DQ = 12,
			S400_400_100 = 13,
			S9 = 14,
			X00_S9 = 0xF,
			S9_DQ = 0x10,
			END = 100
		}

		private const int minHebrewNumberCh = 1488;

		private static HebrewValue[] HebrewValues = new HebrewValue[27]
		{
			new HebrewValue(HebrewToken.Digit1, 1),
			new HebrewValue(HebrewToken.Digit1, 2),
			new HebrewValue(HebrewToken.Digit1, 3),
			new HebrewValue(HebrewToken.Digit1, 4),
			new HebrewValue(HebrewToken.Digit1, 5),
			new HebrewValue(HebrewToken.Digit6_7, 6),
			new HebrewValue(HebrewToken.Digit6_7, 7),
			new HebrewValue(HebrewToken.Digit1, 8),
			new HebrewValue(HebrewToken.Digit9, 9),
			new HebrewValue(HebrewToken.Digit10, 10),
			new HebrewValue(HebrewToken.Invalid, -1),
			new HebrewValue(HebrewToken.Digit10, 20),
			new HebrewValue(HebrewToken.Digit10, 30),
			new HebrewValue(HebrewToken.Invalid, -1),
			new HebrewValue(HebrewToken.Digit10, 40),
			new HebrewValue(HebrewToken.Invalid, -1),
			new HebrewValue(HebrewToken.Digit10, 50),
			new HebrewValue(HebrewToken.Digit10, 60),
			new HebrewValue(HebrewToken.Digit10, 70),
			new HebrewValue(HebrewToken.Invalid, -1),
			new HebrewValue(HebrewToken.Digit10, 80),
			new HebrewValue(HebrewToken.Invalid, -1),
			new HebrewValue(HebrewToken.Digit10, 90),
			new HebrewValue(HebrewToken.Digit100, 100),
			new HebrewValue(HebrewToken.Digit200_300, 200),
			new HebrewValue(HebrewToken.Digit200_300, 300),
			new HebrewValue(HebrewToken.Digit400, 400)
		};

		private static char maxHebrewNumberCh = (char)(1488 + HebrewValues.Length - 1);

		private static HS[][] m_numberPasingState = new HS[17][]
		{
			new HS[10]
			{
				HS.S400,
				HS.X00,
				HS.X00,
				HS.X0,
				HS.X,
				HS.X,
				HS.X,
				HS.S9,
				HS._err,
				HS._err
			},
			new HS[10]
			{
				HS.S400_400,
				HS.S400_X00,
				HS.S400_X00,
				HS.S400_X0,
				HS._err,
				HS._err,
				HS._err,
				HS.X00_S9,
				HS.END,
				HS.S400_DQ
			},
			new HS[10]
			{
				HS._err,
				HS._err,
				HS.S400_400_100,
				HS.S400_X0,
				HS._err,
				HS._err,
				HS._err,
				HS.X00_S9,
				HS._err,
				HS.S400_400_DQ
			},
			new HS[10]
			{
				HS._err,
				HS._err,
				HS._err,
				HS.S400_X00_X0,
				HS._err,
				HS._err,
				HS._err,
				HS.X00_S9,
				HS._err,
				HS.X00_DQ
			},
			new HS[10]
			{
				HS._err,
				HS._err,
				HS._err,
				HS._err,
				HS._err,
				HS._err,
				HS._err,
				HS._err,
				HS._err,
				HS.X0_DQ
			},
			new HS[10]
			{
				HS._err,
				HS._err,
				HS._err,
				HS.END,
				HS.END,
				HS.END,
				HS.END,
				HS.END,
				HS._err,
				HS._err
			},
			new HS[10]
			{
				HS._err,
				HS._err,
				HS._err,
				HS._err,
				HS._err,
				HS._err,
				HS._err,
				HS._err,
				HS._err,
				HS.X0_DQ
			},
			new HS[10]
			{
				HS._err,
				HS._err,
				HS._err,
				HS._err,
				HS.END,
				HS.END,
				HS.END,
				HS.END,
				HS._err,
				HS._err
			},
			new HS[10]
			{
				HS._err,
				HS._err,
				HS._err,
				HS._err,
				HS._err,
				HS._err,
				HS._err,
				HS._err,
				HS.END,
				HS._err
			},
			new HS[10]
			{
				HS._err,
				HS._err,
				HS._err,
				HS._err,
				HS._err,
				HS._err,
				HS._err,
				HS._err,
				HS.END,
				HS.X0_DQ
			},
			new HS[10]
			{
				HS._err,
				HS._err,
				HS._err,
				HS.S400_X0,
				HS._err,
				HS._err,
				HS._err,
				HS.X00_S9,
				HS.END,
				HS.X00_DQ
			},
			new HS[10]
			{
				HS.END,
				HS.END,
				HS.END,
				HS.END,
				HS.END,
				HS.END,
				HS.END,
				HS.END,
				HS._err,
				HS._err
			},
			new HS[10]
			{
				HS._err,
				HS._err,
				HS.END,
				HS.END,
				HS.END,
				HS.END,
				HS.END,
				HS.END,
				HS._err,
				HS._err
			},
			new HS[10]
			{
				HS._err,
				HS._err,
				HS._err,
				HS.S400_X00_X0,
				HS._err,
				HS._err,
				HS._err,
				HS.X00_S9,
				HS._err,
				HS.X00_DQ
			},
			new HS[10]
			{
				HS._err,
				HS._err,
				HS._err,
				HS._err,
				HS._err,
				HS._err,
				HS._err,
				HS._err,
				HS.END,
				HS.S9_DQ
			},
			new HS[10]
			{
				HS._err,
				HS._err,
				HS._err,
				HS._err,
				HS._err,
				HS._err,
				HS._err,
				HS._err,
				HS._err,
				HS.S9_DQ
			},
			new HS[10]
			{
				HS._err,
				HS._err,
				HS._err,
				HS._err,
				HS._err,
				HS.END,
				HS.END,
				HS._err,
				HS._err,
				HS._err
			}
		};

		private HebrewNumber()
		{
		}

		internal static string ToString(int Number)
		{
			char c = '\0';
			StringBuilder stringBuilder = new StringBuilder();
			if (Number > 5000)
			{
				Number -= 5000;
			}
			int num = Number / 100;
			if (num > 0)
			{
				Number -= num * 100;
				for (int i = 0; i < num / 4; i++)
				{
					stringBuilder.Append('ת');
				}
				int num2 = num % 4;
				if (num2 > 0)
				{
					stringBuilder.Append((char)(1510 + num2));
				}
			}
			int num3 = Number / 10;
			Number %= 10;
			switch (num3)
			{
			case 0:
				c = '\0';
				break;
			case 1:
				c = 'י';
				break;
			case 2:
				c = 'כ';
				break;
			case 3:
				c = 'ל';
				break;
			case 4:
				c = 'מ';
				break;
			case 5:
				c = 'נ';
				break;
			case 6:
				c = 'ס';
				break;
			case 7:
				c = 'ע';
				break;
			case 8:
				c = 'פ';
				break;
			case 9:
				c = 'צ';
				break;
			}
			char c2 = (char)((Number > 0) ? (1488 + Number - 1) : 0);
			if (c2 == 'ה' && c == 'י')
			{
				c2 = 'ו';
				c = 'ט';
			}
			if (c2 == 'ו' && c == 'י')
			{
				c2 = 'ז';
				c = 'ט';
			}
			if (c != 0)
			{
				stringBuilder.Append(c);
			}
			if (c2 != 0)
			{
				stringBuilder.Append(c2);
			}
			if (stringBuilder.Length > 1)
			{
				stringBuilder.Insert(stringBuilder.Length - 1, '"');
			}
			else
			{
				stringBuilder.Append('\'');
			}
			return stringBuilder.ToString();
		}

		internal static HebrewNumberParsingState ParseByChar(char ch, ref HebrewNumberParsingContext context)
		{
			HebrewToken hebrewToken;
			switch (ch)
			{
			case '\'':
				hebrewToken = HebrewToken.SingleQuote;
				break;
			case '"':
				hebrewToken = HebrewToken.DoubleQuote;
				break;
			default:
			{
				int num = ch - 1488;
				if (num >= 0 && num < HebrewValues.Length)
				{
					hebrewToken = HebrewValues[num].token;
					if (hebrewToken == HebrewToken.Invalid)
					{
						return HebrewNumberParsingState.NotHebrewDigit;
					}
					context.result += HebrewValues[num].value;
					break;
				}
				return HebrewNumberParsingState.NotHebrewDigit;
			}
			}
			context.state = m_numberPasingState[(int)context.state][(int)hebrewToken];
			if (context.state == HS._err)
			{
				return HebrewNumberParsingState.InvalidHebrewNumber;
			}
			if (context.state == HS.END)
			{
				return HebrewNumberParsingState.FoundEndOfHebrewNumber;
			}
			return HebrewNumberParsingState.ContinueParsing;
		}

		internal static bool IsDigit(char ch)
		{
			if (ch >= 'א' && ch <= maxHebrewNumberCh)
			{
				return HebrewValues[ch - 1488].value >= 0;
			}
			if (ch != '\'')
			{
				return ch == '"';
			}
			return true;
		}
	}
}
