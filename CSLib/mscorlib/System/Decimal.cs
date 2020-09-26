using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;

namespace System
{
	[Serializable]
	[ComVisible(true)]
	public struct Decimal : IFormattable, IComparable, IConvertible, IComparable<decimal>, IEquatable<decimal>
	{
		private const int SignMask = int.MinValue;

		private const int ScaleMask = 16711680;

		private const int ScaleShift = 16;

		private const int MaxInt32Scale = 9;

		public const decimal Zero = 0m;

		public const decimal One = 1m;

		public const decimal MinusOne = -1m;

		public const decimal MaxValue = decimal.MaxValue;

		public const decimal MinValue = decimal.MinValue;

		private static uint[] Powers10 = new uint[10]
		{
			1u,
			10u,
			100u,
			1000u,
			10000u,
			100000u,
			1000000u,
			10000000u,
			100000000u,
			1000000000u
		};

		private int flags;

		private int hi;

		private int lo;

		private int mid;

		public Decimal(int value)
		{
			int num = value;
			if (num >= 0)
			{
				flags = 0;
			}
			else
			{
				flags = int.MinValue;
				num = -num;
			}
			lo = num;
			mid = 0;
			hi = 0;
		}

		[CLSCompliant(false)]
		public Decimal(uint value)
		{
			flags = 0;
			lo = (int)value;
			mid = 0;
			hi = 0;
		}

		public Decimal(long value)
		{
			long num = value;
			if (num >= 0)
			{
				flags = 0;
			}
			else
			{
				flags = int.MinValue;
				num = -num;
			}
			lo = (int)num;
			mid = (int)(num >> 32);
			hi = 0;
		}

		[CLSCompliant(false)]
		public Decimal(ulong value)
		{
			flags = 0;
			lo = (int)value;
			mid = (int)(value >> 32);
			hi = 0;
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		public extern Decimal(float value);

		[MethodImpl(MethodImplOptions.InternalCall)]
		public extern Decimal(double value);

		internal Decimal(Currency value)
		{
			decimal num = Currency.ToDecimal(value);
			lo = num.lo;
			mid = num.mid;
			hi = num.hi;
			flags = num.flags;
		}

		public static long ToOACurrency(decimal value)
		{
			return new Currency(value).ToOACurrency();
		}

		public static decimal FromOACurrency(long cy)
		{
			return Currency.ToDecimal(Currency.FromOACurrency(cy));
		}

		public Decimal(int[] bits)
		{
			if (bits == null)
			{
				throw new ArgumentNullException("bits");
			}
			if (bits.Length == 4)
			{
				int num = bits[3];
				if ((num & 0x7F00FFFF) == 0 && (num & 0xFF0000) <= 1835008)
				{
					lo = bits[0];
					mid = bits[1];
					hi = bits[2];
					flags = num;
					return;
				}
			}
			throw new ArgumentException(Environment.GetResourceString("Arg_DecBitCtor"));
		}

		public Decimal(int lo, int mid, int hi, bool isNegative, byte scale)
		{
			if (scale > 28)
			{
				throw new ArgumentOutOfRangeException("scale", Environment.GetResourceString("ArgumentOutOfRange_DecimalScale"));
			}
			this.lo = lo;
			this.mid = mid;
			this.hi = hi;
			flags = scale << 16;
			if (isNegative)
			{
				flags |= int.MinValue;
			}
		}

		private Decimal(int lo, int mid, int hi, int flags)
		{
			this.lo = lo;
			this.mid = mid;
			this.hi = hi;
			this.flags = flags;
		}

		internal static decimal Abs(decimal d)
		{
			return new decimal(d.lo, d.mid, d.hi, d.flags & 0x7FFFFFFF);
		}

		public static decimal Add(decimal d1, decimal d2)
		{
			decimal result = 0m;
			FCallAdd(ref result, d1, d2);
			return result;
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void FCallAdd(ref decimal result, decimal d1, decimal d2);

		public static decimal Ceiling(decimal d)
		{
			return -Floor(-d);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		public static extern int Compare(decimal d1, decimal d2);

		public int CompareTo(object value)
		{
			if (value == null)
			{
				return 1;
			}
			if (!(value is decimal))
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_MustBeDecimal"));
			}
			return Compare(this, (decimal)value);
		}

		public int CompareTo(decimal value)
		{
			return Compare(this, value);
		}

		public static decimal Divide(decimal d1, decimal d2)
		{
			decimal result = 0m;
			FCallDivide(ref result, d1, d2);
			return result;
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void FCallDivide(ref decimal result, decimal d1, decimal d2);

		public override bool Equals(object value)
		{
			if (value is decimal)
			{
				return Compare(this, (decimal)value) == 0;
			}
			return false;
		}

		public bool Equals(decimal value)
		{
			return Compare(this, value) == 0;
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		public override extern int GetHashCode();

		public static bool Equals(decimal d1, decimal d2)
		{
			return Compare(d1, d2) == 0;
		}

		public static decimal Floor(decimal d)
		{
			decimal result = 0m;
			FCallFloor(ref result, d);
			return result;
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void FCallFloor(ref decimal result, decimal d);

		public override string ToString()
		{
			return Number.FormatDecimal(this, null, NumberFormatInfo.CurrentInfo);
		}

		public string ToString(string format)
		{
			return Number.FormatDecimal(this, format, NumberFormatInfo.CurrentInfo);
		}

		public string ToString(IFormatProvider provider)
		{
			return Number.FormatDecimal(this, null, NumberFormatInfo.GetInstance(provider));
		}

		public string ToString(string format, IFormatProvider provider)
		{
			return Number.FormatDecimal(this, format, NumberFormatInfo.GetInstance(provider));
		}

		public static decimal Parse(string s)
		{
			return Number.ParseDecimal(s, NumberStyles.Number, NumberFormatInfo.CurrentInfo);
		}

		public static decimal Parse(string s, NumberStyles style)
		{
			NumberFormatInfo.ValidateParseStyleFloatingPoint(style);
			return Number.ParseDecimal(s, style, NumberFormatInfo.CurrentInfo);
		}

		public static decimal Parse(string s, IFormatProvider provider)
		{
			return Number.ParseDecimal(s, NumberStyles.Number, NumberFormatInfo.GetInstance(provider));
		}

		public static decimal Parse(string s, NumberStyles style, IFormatProvider provider)
		{
			NumberFormatInfo.ValidateParseStyleFloatingPoint(style);
			return Number.ParseDecimal(s, style, NumberFormatInfo.GetInstance(provider));
		}

		public static bool TryParse(string s, out decimal result)
		{
			return Number.TryParseDecimal(s, NumberStyles.Number, NumberFormatInfo.CurrentInfo, out result);
		}

		public static bool TryParse(string s, NumberStyles style, IFormatProvider provider, out decimal result)
		{
			NumberFormatInfo.ValidateParseStyleFloatingPoint(style);
			return Number.TryParseDecimal(s, style, NumberFormatInfo.GetInstance(provider), out result);
		}

		public static int[] GetBits(decimal d)
		{
			return new int[4]
			{
				d.lo,
				d.mid,
				d.hi,
				d.flags
			};
		}

		internal static void GetBytes(decimal d, byte[] buffer)
		{
			buffer[0] = (byte)d.lo;
			buffer[1] = (byte)(d.lo >> 8);
			buffer[2] = (byte)(d.lo >> 16);
			buffer[3] = (byte)(d.lo >> 24);
			buffer[4] = (byte)d.mid;
			buffer[5] = (byte)(d.mid >> 8);
			buffer[6] = (byte)(d.mid >> 16);
			buffer[7] = (byte)(d.mid >> 24);
			buffer[8] = (byte)d.hi;
			buffer[9] = (byte)(d.hi >> 8);
			buffer[10] = (byte)(d.hi >> 16);
			buffer[11] = (byte)(d.hi >> 24);
			buffer[12] = (byte)d.flags;
			buffer[13] = (byte)(d.flags >> 8);
			buffer[14] = (byte)(d.flags >> 16);
			buffer[15] = (byte)(d.flags >> 24);
		}

		internal static decimal ToDecimal(byte[] buffer)
		{
			int num = buffer[0] | (buffer[1] << 8) | (buffer[2] << 16) | (buffer[3] << 24);
			int num2 = buffer[4] | (buffer[5] << 8) | (buffer[6] << 16) | (buffer[7] << 24);
			int num3 = buffer[8] | (buffer[9] << 8) | (buffer[10] << 16) | (buffer[11] << 24);
			int num4 = buffer[12] | (buffer[13] << 8) | (buffer[14] << 16) | (buffer[15] << 24);
			return new decimal(num, num2, num3, num4);
		}

		private static void InternalAddUInt32RawUnchecked(ref decimal value, uint i)
		{
			uint num = (uint)value.lo;
			uint num2 = (uint)(value.lo = (int)(num + i));
			if (num2 < num || num2 < i)
			{
				num = (uint)value.mid;
				num2 = (uint)(value.mid = (int)(num + 1));
				if (num2 < num || num2 < 1)
				{
					value.hi++;
				}
			}
		}

		private static uint InternalDivRemUInt32(ref decimal value, uint divisor)
		{
			uint num = 0u;
			if (value.hi != 0)
			{
				ulong num2 = (uint)value.hi;
				value.hi = (int)(num2 / divisor);
				num = (uint)(num2 % divisor);
			}
			if (value.mid != 0 || num != 0)
			{
				ulong num2 = ((ulong)num << 32) | (uint)value.mid;
				value.mid = (int)(num2 / divisor);
				num = (uint)(num2 % divisor);
			}
			if (value.lo != 0 || num != 0)
			{
				ulong num2 = ((ulong)num << 32) | (uint)value.lo;
				value.lo = (int)(num2 / divisor);
				num = (uint)(num2 % divisor);
			}
			return num;
		}

		private static void InternalRoundFromZero(ref decimal d, int decimalCount)
		{
			int num = (d.flags & 0xFF0000) >> 16;
			int num2 = num - decimalCount;
			if (num2 > 0)
			{
				uint num4;
				uint num5;
				do
				{
					int num3 = ((num2 > 9) ? 9 : num2);
					num4 = Powers10[num3];
					num5 = InternalDivRemUInt32(ref d, num4);
					num2 -= num3;
				}
				while (num2 > 0);
				if (num5 >= num4 >> 1)
				{
					InternalAddUInt32RawUnchecked(ref d, 1u);
				}
				d.flags = ((decimalCount << 16) & 0xFF0000) | (d.flags & int.MinValue);
			}
		}

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		internal static decimal Max(decimal d1, decimal d2)
		{
			if (Compare(d1, d2) < 0)
			{
				return d2;
			}
			return d1;
		}

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		internal static decimal Min(decimal d1, decimal d2)
		{
			if (Compare(d1, d2) >= 0)
			{
				return d2;
			}
			return d1;
		}

		public static decimal Remainder(decimal d1, decimal d2)
		{
			d2.flags = (d2.flags & 0x7FFFFFFF) | (d1.flags & int.MinValue);
			if (Abs(d1) < Abs(d2))
			{
				return d1;
			}
			d1 -= d2;
			if (d1 == 0m)
			{
				d1.flags = (d1.flags & 0x7FFFFFFF) | (d2.flags & int.MinValue);
			}
			decimal d3 = Truncate(d1 / d2);
			decimal d4 = d3 * d2;
			decimal num = d1 - d4;
			if ((d1.flags & int.MinValue) != (num.flags & int.MinValue))
			{
				if (num == 0m)
				{
					num.flags = (num.flags & 0x7FFFFFFF) | (d1.flags & int.MinValue);
				}
				else
				{
					num += d2;
				}
			}
			return num;
		}

		public static decimal Multiply(decimal d1, decimal d2)
		{
			decimal result = 0m;
			FCallMultiply(ref result, d1, d2);
			return result;
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void FCallMultiply(ref decimal result, decimal d1, decimal d2);

		public static decimal Negate(decimal d)
		{
			return new decimal(d.lo, d.mid, d.hi, d.flags ^ int.MinValue);
		}

		public static decimal Round(decimal d)
		{
			return Round(d, 0);
		}

		public static decimal Round(decimal d, int decimals)
		{
			decimal result = 0m;
			FCallRound(ref result, d, decimals);
			return result;
		}

		public static decimal Round(decimal d, MidpointRounding mode)
		{
			return Round(d, 0, mode);
		}

		public static decimal Round(decimal d, int decimals, MidpointRounding mode)
		{
			if (decimals < 0 || decimals > 28)
			{
				throw new ArgumentOutOfRangeException("decimals", Environment.GetResourceString("ArgumentOutOfRange_DecimalRound"));
			}
			if (mode < MidpointRounding.ToEven || mode > MidpointRounding.AwayFromZero)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_InvalidEnumValue", mode, "MidpointRounding"), "mode");
			}
			decimal result = d;
			if (mode == MidpointRounding.ToEven)
			{
				FCallRound(ref result, d, decimals);
			}
			else
			{
				InternalRoundFromZero(ref result, decimals);
			}
			return result;
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void FCallRound(ref decimal result, decimal d, int decimals);

		public static decimal Subtract(decimal d1, decimal d2)
		{
			decimal result = 0m;
			FCallSubtract(ref result, d1, d2);
			return result;
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void FCallSubtract(ref decimal result, decimal d1, decimal d2);

		public static byte ToByte(decimal value)
		{
			uint num = ToUInt32(value);
			if (num < 0 || num > 255)
			{
				throw new OverflowException(Environment.GetResourceString("Overflow_Byte"));
			}
			return (byte)num;
		}

		[CLSCompliant(false)]
		public static sbyte ToSByte(decimal value)
		{
			int num = ToInt32(value);
			if (num < -128 || num > 127)
			{
				throw new OverflowException(Environment.GetResourceString("Overflow_SByte"));
			}
			return (sbyte)num;
		}

		public static short ToInt16(decimal value)
		{
			int num = ToInt32(value);
			if (num < -32768 || num > 32767)
			{
				throw new OverflowException(Environment.GetResourceString("Overflow_Int16"));
			}
			return (short)num;
		}

		internal static Currency ToCurrency(decimal d)
		{
			Currency result = default(Currency);
			FCallToCurrency(ref result, d);
			return result;
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void FCallToCurrency(ref Currency result, decimal d);

		[MethodImpl(MethodImplOptions.InternalCall)]
		public static extern double ToDouble(decimal d);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern int FCallToInt32(decimal d);

		public static int ToInt32(decimal d)
		{
			if (((uint)d.flags & 0xFF0000u) != 0)
			{
				d = Truncate(d);
			}
			if (d.hi == 0 && d.mid == 0)
			{
				int num = d.lo;
				if (d.flags >= 0)
				{
					if (num >= 0)
					{
						return num;
					}
				}
				else
				{
					num = -num;
					if (num <= 0)
					{
						return num;
					}
				}
			}
			throw new OverflowException(Environment.GetResourceString("Overflow_Int32"));
		}

		public static long ToInt64(decimal d)
		{
			if (((uint)d.flags & 0xFF0000u) != 0)
			{
				d = Truncate(d);
			}
			if (d.hi == 0)
			{
				long num = (d.lo & 0xFFFFFFFFu) | ((long)d.mid << 32);
				if (d.flags >= 0)
				{
					if (num >= 0)
					{
						return num;
					}
				}
				else
				{
					num = -num;
					if (num <= 0)
					{
						return num;
					}
				}
			}
			throw new OverflowException(Environment.GetResourceString("Overflow_Int64"));
		}

		[CLSCompliant(false)]
		public static ushort ToUInt16(decimal value)
		{
			uint num = ToUInt32(value);
			if (num < 0 || num > 65535)
			{
				throw new OverflowException(Environment.GetResourceString("Overflow_UInt16"));
			}
			return (ushort)num;
		}

		[CLSCompliant(false)]
		public static uint ToUInt32(decimal d)
		{
			if (((uint)d.flags & 0xFF0000u) != 0)
			{
				d = Truncate(d);
			}
			if (d.hi == 0 && d.mid == 0)
			{
				uint num = (uint)d.lo;
				if (d.flags >= 0 || num == 0)
				{
					return num;
				}
			}
			throw new OverflowException(Environment.GetResourceString("Overflow_UInt32"));
		}

		[CLSCompliant(false)]
		public static ulong ToUInt64(decimal d)
		{
			if (((uint)d.flags & 0xFF0000u) != 0)
			{
				d = Truncate(d);
			}
			if (d.hi == 0)
			{
				ulong num = (uint)d.lo | ((ulong)(uint)d.mid << 32);
				if (d.flags >= 0 || num == 0)
				{
					return num;
				}
			}
			throw new OverflowException(Environment.GetResourceString("Overflow_UInt64"));
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		public static extern float ToSingle(decimal d);

		public static decimal Truncate(decimal d)
		{
			decimal result = 0m;
			FCallTruncate(ref result, d);
			return result;
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void FCallTruncate(ref decimal result, decimal d);

		public static implicit operator decimal(byte value)
		{
			return new decimal(value);
		}

		[CLSCompliant(false)]
		public static implicit operator decimal(sbyte value)
		{
			return new decimal(value);
		}

		public static implicit operator decimal(short value)
		{
			return new decimal(value);
		}

		[CLSCompliant(false)]
		public static implicit operator decimal(ushort value)
		{
			return new decimal(value);
		}

		public static implicit operator decimal(char value)
		{
			return new decimal(value);
		}

		public static implicit operator decimal(int value)
		{
			return new decimal(value);
		}

		[CLSCompliant(false)]
		public static implicit operator decimal(uint value)
		{
			return new decimal(value);
		}

		public static implicit operator decimal(long value)
		{
			return new decimal(value);
		}

		[CLSCompliant(false)]
		public static implicit operator decimal(ulong value)
		{
			return new decimal(value);
		}

		public static explicit operator decimal(float value)
		{
			return new decimal(value);
		}

		public static explicit operator decimal(double value)
		{
			return new decimal(value);
		}

		public static explicit operator byte(decimal value)
		{
			return ToByte(value);
		}

		[CLSCompliant(false)]
		public static explicit operator sbyte(decimal value)
		{
			return ToSByte(value);
		}

		public static explicit operator char(decimal value)
		{
			return (char)ToUInt16(value);
		}

		public static explicit operator short(decimal value)
		{
			return ToInt16(value);
		}

		[CLSCompliant(false)]
		public static explicit operator ushort(decimal value)
		{
			return ToUInt16(value);
		}

		public static explicit operator int(decimal value)
		{
			return ToInt32(value);
		}

		[CLSCompliant(false)]
		public static explicit operator uint(decimal value)
		{
			return ToUInt32(value);
		}

		public static explicit operator long(decimal value)
		{
			return ToInt64(value);
		}

		[CLSCompliant(false)]
		public static explicit operator ulong(decimal value)
		{
			return ToUInt64(value);
		}

		public static explicit operator float(decimal value)
		{
			return ToSingle(value);
		}

		public static explicit operator double(decimal value)
		{
			return ToDouble(value);
		}

		public static decimal operator +(decimal d)
		{
			return d;
		}

		public static decimal operator -(decimal d)
		{
			return Negate(d);
		}

		public static decimal operator ++(decimal d)
		{
			return Add(d, 1m);
		}

		public static decimal operator --(decimal d)
		{
			return Subtract(d, 1m);
		}

		public static decimal operator +(decimal d1, decimal d2)
		{
			return Add(d1, d2);
		}

		public static decimal operator -(decimal d1, decimal d2)
		{
			return Subtract(d1, d2);
		}

		public static decimal operator *(decimal d1, decimal d2)
		{
			return Multiply(d1, d2);
		}

		public static decimal operator /(decimal d1, decimal d2)
		{
			return Divide(d1, d2);
		}

		public static decimal operator %(decimal d1, decimal d2)
		{
			return Remainder(d1, d2);
		}

		public static bool operator ==(decimal d1, decimal d2)
		{
			return Compare(d1, d2) == 0;
		}

		public static bool operator !=(decimal d1, decimal d2)
		{
			return Compare(d1, d2) != 0;
		}

		public static bool operator <(decimal d1, decimal d2)
		{
			return Compare(d1, d2) < 0;
		}

		public static bool operator <=(decimal d1, decimal d2)
		{
			return Compare(d1, d2) <= 0;
		}

		public static bool operator >(decimal d1, decimal d2)
		{
			return Compare(d1, d2) > 0;
		}

		public static bool operator >=(decimal d1, decimal d2)
		{
			return Compare(d1, d2) >= 0;
		}

		public TypeCode GetTypeCode()
		{
			return TypeCode.Decimal;
		}

		bool IConvertible.ToBoolean(IFormatProvider provider)
		{
			return Convert.ToBoolean(this);
		}

		char IConvertible.ToChar(IFormatProvider provider)
		{
			throw new InvalidCastException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("InvalidCast_FromTo"), "Decimal", "Char"));
		}

		sbyte IConvertible.ToSByte(IFormatProvider provider)
		{
			return Convert.ToSByte(this);
		}

		byte IConvertible.ToByte(IFormatProvider provider)
		{
			return Convert.ToByte(this);
		}

		short IConvertible.ToInt16(IFormatProvider provider)
		{
			return Convert.ToInt16(this);
		}

		ushort IConvertible.ToUInt16(IFormatProvider provider)
		{
			return Convert.ToUInt16(this);
		}

		int IConvertible.ToInt32(IFormatProvider provider)
		{
			return Convert.ToInt32(this);
		}

		uint IConvertible.ToUInt32(IFormatProvider provider)
		{
			return Convert.ToUInt32(this);
		}

		long IConvertible.ToInt64(IFormatProvider provider)
		{
			return Convert.ToInt64(this);
		}

		ulong IConvertible.ToUInt64(IFormatProvider provider)
		{
			return Convert.ToUInt64(this);
		}

		float IConvertible.ToSingle(IFormatProvider provider)
		{
			return Convert.ToSingle(this);
		}

		double IConvertible.ToDouble(IFormatProvider provider)
		{
			return Convert.ToDouble(this);
		}

		decimal IConvertible.ToDecimal(IFormatProvider provider)
		{
			return this;
		}

		DateTime IConvertible.ToDateTime(IFormatProvider provider)
		{
			throw new InvalidCastException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("InvalidCast_FromTo"), "Decimal", "DateTime"));
		}

		object IConvertible.ToType(Type type, IFormatProvider provider)
		{
			return Convert.DefaultToType(this, type, provider);
		}
	}
}
