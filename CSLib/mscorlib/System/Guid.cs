using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System
{
	[Serializable]
	[ComVisible(true)]
	public struct Guid : IFormattable, IComparable, IComparable<Guid>, IEquatable<Guid>
	{
		public static readonly Guid Empty = default(Guid);

		private int _a;

		private short _b;

		private short _c;

		private byte _d;

		private byte _e;

		private byte _f;

		private byte _g;

		private byte _h;

		private byte _i;

		private byte _j;

		private byte _k;

		public Guid(byte[] b)
		{
			if (b == null)
			{
				throw new ArgumentNullException("b");
			}
			if (b.Length != 16)
			{
				throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Arg_GuidArrayCtor"), "16"));
			}
			_a = (b[3] << 24) | (b[2] << 16) | (b[1] << 8) | b[0];
			_b = (short)((b[5] << 8) | b[4]);
			_c = (short)((b[7] << 8) | b[6]);
			_d = b[8];
			_e = b[9];
			_f = b[10];
			_g = b[11];
			_h = b[12];
			_i = b[13];
			_j = b[14];
			_k = b[15];
		}

		[CLSCompliant(false)]
		public Guid(uint a, ushort b, ushort c, byte d, byte e, byte f, byte g, byte h, byte i, byte j, byte k)
		{
			_a = (int)a;
			_b = (short)b;
			_c = (short)c;
			_d = d;
			_e = e;
			_f = f;
			_g = g;
			_h = h;
			_i = i;
			_j = j;
			_k = k;
		}

		private Guid(bool blank)
		{
			_a = 0;
			_b = 0;
			_c = 0;
			_d = 0;
			_e = 0;
			_f = 0;
			_g = 0;
			_h = 0;
			_i = 0;
			_j = 0;
			_k = 0;
			if (!blank)
			{
				CompleteGuid();
			}
		}

		public Guid(string g)
		{
			if (g == null)
			{
				throw new ArgumentNullException("g");
			}
			int num = 0;
			int num2 = 0;
			try
			{
				long num4;
				int num3;
				if (g.IndexOf('-', 0) >= 0)
				{
					string text = g.Trim();
					if (text[0] == '{')
					{
						if (text.Length != 38 || text[37] != '}')
						{
							throw new FormatException(Environment.GetResourceString("Format_GuidInvLen"));
						}
						num = 1;
					}
					else if (text[0] == '(')
					{
						if (text.Length != 38 || text[37] != ')')
						{
							throw new FormatException(Environment.GetResourceString("Format_GuidInvLen"));
						}
						num = 1;
					}
					else if (text.Length != 36)
					{
						throw new FormatException(Environment.GetResourceString("Format_GuidInvLen"));
					}
					if (text[8 + num] != '-' || text[13 + num] != '-' || text[18 + num] != '-' || text[23 + num] != '-')
					{
						throw new FormatException(Environment.GetResourceString("Format_GuidDashes"));
					}
					num2 = num;
					_a = TryParse(text, ref num2, 8);
					num2++;
					_b = (short)TryParse(text, ref num2, 4);
					num2++;
					_c = (short)TryParse(text, ref num2, 4);
					num2++;
					num3 = TryParse(text, ref num2, 4);
					num2++;
					num = num2;
					num4 = ParseNumbers.StringToLong(text, 16, 8192, ref num2);
					if (num2 - num != 12)
					{
						throw new FormatException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Format_GuidInvLen")));
					}
					_d = (byte)(num3 >> 8);
					_e = (byte)num3;
					num3 = (int)(num4 >> 32);
					_f = (byte)(num3 >> 8);
					_g = (byte)num3;
					num3 = (int)num4;
					_h = (byte)(num3 >> 24);
					_i = (byte)(num3 >> 16);
					_j = (byte)(num3 >> 8);
					_k = (byte)num3;
					return;
				}
				if (g.IndexOf('{', 0) >= 0)
				{
					int num5 = 0;
					int num6 = 0;
					g = EatAllWhitespace(g);
					if (g[0] != '{')
					{
						throw new FormatException(Environment.GetResourceString("Format_GuidBrace"));
					}
					if (!IsHexPrefix(g, 1))
					{
						throw new FormatException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Format_GuidHexPrefix"), "{0xdddddddd, etc}"));
					}
					num5 = 3;
					num6 = g.IndexOf(',', num5) - num5;
					if (num6 <= 0)
					{
						throw new FormatException(Environment.GetResourceString("Format_GuidComma"));
					}
					_a = ParseNumbers.StringToInt(g.Substring(num5, num6), 16, 4096);
					if (!IsHexPrefix(g, num5 + num6 + 1))
					{
						throw new FormatException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Format_GuidHexPrefix"), "{0xdddddddd, 0xdddd, etc}"));
					}
					num5 = num5 + num6 + 3;
					num6 = g.IndexOf(',', num5) - num5;
					if (num6 <= 0)
					{
						throw new FormatException(Environment.GetResourceString("Format_GuidComma"));
					}
					_b = (short)ParseNumbers.StringToInt(g.Substring(num5, num6), 16, 4096);
					if (!IsHexPrefix(g, num5 + num6 + 1))
					{
						throw new FormatException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Format_GuidHexPrefix"), "{0xdddddddd, 0xdddd, 0xdddd, etc}"));
					}
					num5 = num5 + num6 + 3;
					num6 = g.IndexOf(',', num5) - num5;
					if (num6 <= 0)
					{
						throw new FormatException(Environment.GetResourceString("Format_GuidComma"));
					}
					_c = (short)ParseNumbers.StringToInt(g.Substring(num5, num6), 16, 4096);
					if (g.Length <= num5 + num6 + 1 || g[num5 + num6 + 1] != '{')
					{
						throw new FormatException(Environment.GetResourceString("Format_GuidBrace"));
					}
					num6++;
					byte[] array = new byte[8];
					for (int i = 0; i < 8; i++)
					{
						if (!IsHexPrefix(g, num5 + num6 + 1))
						{
							throw new FormatException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Format_GuidHexPrefix"), "{... { ... 0xdd, ...}}"));
						}
						num5 = num5 + num6 + 3;
						if (i < 7)
						{
							num6 = g.IndexOf(',', num5) - num5;
							if (num6 <= 0)
							{
								throw new FormatException(Environment.GetResourceString("Format_GuidComma"));
							}
						}
						else
						{
							num6 = g.IndexOf('}', num5) - num5;
							if (num6 <= 0)
							{
								throw new FormatException(Environment.GetResourceString("Format_GuidBraceAfterLastNumber"));
							}
						}
						uint num7 = (uint)Convert.ToInt32(g.Substring(num5, num6), 16);
						if (num7 > 255)
						{
							throw new FormatException(Environment.GetResourceString("Overflow_Byte"));
						}
						array[i] = (byte)num7;
					}
					_d = array[0];
					_e = array[1];
					_f = array[2];
					_g = array[3];
					_h = array[4];
					_i = array[5];
					_j = array[6];
					_k = array[7];
					if (num5 + num6 + 1 >= g.Length || g[num5 + num6 + 1] != '}')
					{
						throw new FormatException(Environment.GetResourceString("Format_GuidEndBrace"));
					}
					if (num5 + num6 + 1 == g.Length - 1)
					{
						return;
					}
					throw new FormatException(Environment.GetResourceString("Format_ExtraJunkAtEnd"));
				}
				string text2 = g.Trim();
				if (text2.Length != 32)
				{
					throw new FormatException(Environment.GetResourceString("Format_GuidInvLen"));
				}
				foreach (char c in text2)
				{
					if (c < '0' || c > '9')
					{
						char c2 = char.ToUpper(c, CultureInfo.InvariantCulture);
						if (c2 < 'A' || c2 > 'F')
						{
							throw new FormatException(Environment.GetResourceString("Format_GuidInvalidChar"));
						}
					}
				}
				_a = ParseNumbers.StringToInt(text2.Substring(num, 8), 16, 4096);
				num += 8;
				_b = (short)ParseNumbers.StringToInt(text2.Substring(num, 4), 16, 4096);
				num += 4;
				_c = (short)ParseNumbers.StringToInt(text2.Substring(num, 4), 16, 4096);
				num += 4;
				num3 = (short)ParseNumbers.StringToInt(text2.Substring(num, 4), 16, 4096);
				num += 4;
				num2 = num;
				num4 = ParseNumbers.StringToLong(text2, 16, num, ref num2);
				if (num2 - num != 12)
				{
					throw new FormatException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Format_GuidInvLen")));
				}
				_d = (byte)(num3 >> 8);
				_e = (byte)num3;
				num3 = (int)(num4 >> 32);
				_f = (byte)(num3 >> 8);
				_g = (byte)num3;
				num3 = (int)num4;
				_h = (byte)(num3 >> 24);
				_i = (byte)(num3 >> 16);
				_j = (byte)(num3 >> 8);
				_k = (byte)num3;
			}
			catch (IndexOutOfRangeException)
			{
				throw new FormatException(Environment.GetResourceString("Format_GuidUnrecognized"));
			}
		}

		public Guid(int a, short b, short c, byte[] d)
		{
			if (d == null)
			{
				throw new ArgumentNullException("d");
			}
			if (d.Length != 8)
			{
				throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Arg_GuidArrayCtor"), "8"));
			}
			_a = a;
			_b = b;
			_c = c;
			_d = d[0];
			_e = d[1];
			_f = d[2];
			_g = d[3];
			_h = d[4];
			_i = d[5];
			_j = d[6];
			_k = d[7];
		}

		public Guid(int a, short b, short c, byte d, byte e, byte f, byte g, byte h, byte i, byte j, byte k)
		{
			_a = a;
			_b = b;
			_c = c;
			_d = d;
			_e = e;
			_f = f;
			_g = g;
			_h = h;
			_i = i;
			_j = j;
			_k = k;
		}

		private static int TryParse(string str, ref int parsePos, int requiredLength)
		{
			int num = parsePos;
			int result = ParseNumbers.StringToInt(str, 16, 8192, ref parsePos);
			if (parsePos - num != requiredLength)
			{
				throw new FormatException(Environment.GetResourceString("Format_GuidInvalidChar"));
			}
			return result;
		}

		private static string EatAllWhitespace(string str)
		{
			int length = 0;
			char[] array = new char[str.Length];
			foreach (char c in str)
			{
				if (!char.IsWhiteSpace(c))
				{
					array[length++] = c;
				}
			}
			return new string(array, 0, length);
		}

		private static bool IsHexPrefix(string str, int i)
		{
			if (str[i] == '0' && char.ToLower(str[i + 1], CultureInfo.InvariantCulture) == 'x')
			{
				return true;
			}
			return false;
		}

		public byte[] ToByteArray()
		{
			return new byte[16]
			{
				(byte)_a,
				(byte)(_a >> 8),
				(byte)(_a >> 16),
				(byte)(_a >> 24),
				(byte)_b,
				(byte)(_b >> 8),
				(byte)_c,
				(byte)(_c >> 8),
				_d,
				_e,
				_f,
				_g,
				_h,
				_i,
				_j,
				_k
			};
		}

		public override string ToString()
		{
			return ToString("D", null);
		}

		public override int GetHashCode()
		{
			return _a ^ ((_b << 16) | (ushort)_c) ^ ((_f << 24) | _k);
		}

		public override bool Equals(object o)
		{
			if (o == null || !(o is Guid))
			{
				return false;
			}
			Guid guid = (Guid)o;
			if (guid._a != _a)
			{
				return false;
			}
			if (guid._b != _b)
			{
				return false;
			}
			if (guid._c != _c)
			{
				return false;
			}
			if (guid._d != _d)
			{
				return false;
			}
			if (guid._e != _e)
			{
				return false;
			}
			if (guid._f != _f)
			{
				return false;
			}
			if (guid._g != _g)
			{
				return false;
			}
			if (guid._h != _h)
			{
				return false;
			}
			if (guid._i != _i)
			{
				return false;
			}
			if (guid._j != _j)
			{
				return false;
			}
			if (guid._k != _k)
			{
				return false;
			}
			return true;
		}

		public bool Equals(Guid g)
		{
			if (g._a != _a)
			{
				return false;
			}
			if (g._b != _b)
			{
				return false;
			}
			if (g._c != _c)
			{
				return false;
			}
			if (g._d != _d)
			{
				return false;
			}
			if (g._e != _e)
			{
				return false;
			}
			if (g._f != _f)
			{
				return false;
			}
			if (g._g != _g)
			{
				return false;
			}
			if (g._h != _h)
			{
				return false;
			}
			if (g._i != _i)
			{
				return false;
			}
			if (g._j != _j)
			{
				return false;
			}
			if (g._k != _k)
			{
				return false;
			}
			return true;
		}

		private int GetResult(uint me, uint them)
		{
			if (me < them)
			{
				return -1;
			}
			return 1;
		}

		public int CompareTo(object value)
		{
			if (value == null)
			{
				return 1;
			}
			if (!(value is Guid))
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_MustBeGuid"));
			}
			Guid guid = (Guid)value;
			if (guid._a != _a)
			{
				return GetResult((uint)_a, (uint)guid._a);
			}
			if (guid._b != _b)
			{
				return GetResult((uint)_b, (uint)guid._b);
			}
			if (guid._c != _c)
			{
				return GetResult((uint)_c, (uint)guid._c);
			}
			if (guid._d != _d)
			{
				return GetResult(_d, guid._d);
			}
			if (guid._e != _e)
			{
				return GetResult(_e, guid._e);
			}
			if (guid._f != _f)
			{
				return GetResult(_f, guid._f);
			}
			if (guid._g != _g)
			{
				return GetResult(_g, guid._g);
			}
			if (guid._h != _h)
			{
				return GetResult(_h, guid._h);
			}
			if (guid._i != _i)
			{
				return GetResult(_i, guid._i);
			}
			if (guid._j != _j)
			{
				return GetResult(_j, guid._j);
			}
			if (guid._k != _k)
			{
				return GetResult(_k, guid._k);
			}
			return 0;
		}

		public int CompareTo(Guid value)
		{
			if (value._a != _a)
			{
				return GetResult((uint)_a, (uint)value._a);
			}
			if (value._b != _b)
			{
				return GetResult((uint)_b, (uint)value._b);
			}
			if (value._c != _c)
			{
				return GetResult((uint)_c, (uint)value._c);
			}
			if (value._d != _d)
			{
				return GetResult(_d, value._d);
			}
			if (value._e != _e)
			{
				return GetResult(_e, value._e);
			}
			if (value._f != _f)
			{
				return GetResult(_f, value._f);
			}
			if (value._g != _g)
			{
				return GetResult(_g, value._g);
			}
			if (value._h != _h)
			{
				return GetResult(_h, value._h);
			}
			if (value._i != _i)
			{
				return GetResult(_i, value._i);
			}
			if (value._j != _j)
			{
				return GetResult(_j, value._j);
			}
			if (value._k != _k)
			{
				return GetResult(_k, value._k);
			}
			return 0;
		}

		public static bool operator ==(Guid a, Guid b)
		{
			if (a._a != b._a)
			{
				return false;
			}
			if (a._b != b._b)
			{
				return false;
			}
			if (a._c != b._c)
			{
				return false;
			}
			if (a._d != b._d)
			{
				return false;
			}
			if (a._e != b._e)
			{
				return false;
			}
			if (a._f != b._f)
			{
				return false;
			}
			if (a._g != b._g)
			{
				return false;
			}
			if (a._h != b._h)
			{
				return false;
			}
			if (a._i != b._i)
			{
				return false;
			}
			if (a._j != b._j)
			{
				return false;
			}
			if (a._k != b._k)
			{
				return false;
			}
			return true;
		}

		public static bool operator !=(Guid a, Guid b)
		{
			return !(a == b);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern void CompleteGuid();

		public static Guid NewGuid()
		{
			return new Guid(blank: false);
		}

		public string ToString(string format)
		{
			return ToString(format, null);
		}

		private static char HexToChar(int a)
		{
			a &= 0xF;
			return (char)((a > 9) ? (a - 10 + 97) : (a + 48));
		}

		private static int HexsToChars(char[] guidChars, int offset, int a, int b)
		{
			guidChars[offset++] = HexToChar(a >> 4);
			guidChars[offset++] = HexToChar(a);
			guidChars[offset++] = HexToChar(b >> 4);
			guidChars[offset++] = HexToChar(b);
			return offset;
		}

		public string ToString(string format, IFormatProvider provider)
		{
			if (format == null || format.Length == 0)
			{
				format = "D";
			}
			int offset = 0;
			int length = 38;
			bool flag = true;
			if (format.Length != 1)
			{
				throw new FormatException(Environment.GetResourceString("Format_InvalidGuidFormatSpecification"));
			}
			char[] array;
			switch (format[0])
			{
			case 'D':
			case 'd':
				array = new char[36];
				length = 36;
				break;
			case 'N':
			case 'n':
				array = new char[32];
				length = 32;
				flag = false;
				break;
			case 'B':
			case 'b':
				array = new char[38];
				array[offset++] = '{';
				array[37] = '}';
				break;
			case 'P':
			case 'p':
				array = new char[38];
				array[offset++] = '(';
				array[37] = ')';
				break;
			default:
				throw new FormatException(Environment.GetResourceString("Format_InvalidGuidFormatSpecification"));
			}
			offset = HexsToChars(array, offset, _a >> 24, _a >> 16);
			offset = HexsToChars(array, offset, _a >> 8, _a);
			if (flag)
			{
				array[offset++] = '-';
			}
			offset = HexsToChars(array, offset, _b >> 8, _b);
			if (flag)
			{
				array[offset++] = '-';
			}
			offset = HexsToChars(array, offset, _c >> 8, _c);
			if (flag)
			{
				array[offset++] = '-';
			}
			offset = HexsToChars(array, offset, _d, _e);
			if (flag)
			{
				array[offset++] = '-';
			}
			offset = HexsToChars(array, offset, _f, _g);
			offset = HexsToChars(array, offset, _h, _i);
			offset = HexsToChars(array, offset, _j, _k);
			return new string(array, 0, length);
		}
	}
}
