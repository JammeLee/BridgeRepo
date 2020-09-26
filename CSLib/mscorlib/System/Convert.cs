using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

namespace System
{
	public static class Convert
	{
		internal static readonly Type[] ConvertTypes = new Type[19]
		{
			typeof(Empty),
			typeof(object),
			typeof(DBNull),
			typeof(bool),
			typeof(char),
			typeof(sbyte),
			typeof(byte),
			typeof(short),
			typeof(ushort),
			typeof(int),
			typeof(uint),
			typeof(long),
			typeof(ulong),
			typeof(float),
			typeof(double),
			typeof(decimal),
			typeof(DateTime),
			typeof(object),
			typeof(string)
		};

		internal static readonly Type EnumType = typeof(Enum);

		internal static readonly char[] base64Table = new char[65]
		{
			'A',
			'B',
			'C',
			'D',
			'E',
			'F',
			'G',
			'H',
			'I',
			'J',
			'K',
			'L',
			'M',
			'N',
			'O',
			'P',
			'Q',
			'R',
			'S',
			'T',
			'U',
			'V',
			'W',
			'X',
			'Y',
			'Z',
			'a',
			'b',
			'c',
			'd',
			'e',
			'f',
			'g',
			'h',
			'i',
			'j',
			'k',
			'l',
			'm',
			'n',
			'o',
			'p',
			'q',
			'r',
			's',
			't',
			'u',
			'v',
			'w',
			'x',
			'y',
			'z',
			'0',
			'1',
			'2',
			'3',
			'4',
			'5',
			'6',
			'7',
			'8',
			'9',
			'+',
			'/',
			'='
		};

		public static readonly object DBNull = System.DBNull.Value;

		public static TypeCode GetTypeCode(object value)
		{
			if (value == null)
			{
				return TypeCode.Empty;
			}
			return (value as IConvertible)?.GetTypeCode() ?? TypeCode.Object;
		}

		public static bool IsDBNull(object value)
		{
			if (value == System.DBNull.Value)
			{
				return true;
			}
			IConvertible convertible = value as IConvertible;
			if (convertible == null)
			{
				return false;
			}
			return convertible.GetTypeCode() == TypeCode.DBNull;
		}

		public static object ChangeType(object value, TypeCode typeCode)
		{
			return ChangeType(value, typeCode, Thread.CurrentThread.CurrentCulture);
		}

		public static object ChangeType(object value, TypeCode typeCode, IFormatProvider provider)
		{
			if (value == null && (typeCode == TypeCode.Empty || typeCode == TypeCode.String || typeCode == TypeCode.Object))
			{
				return null;
			}
			IConvertible convertible = value as IConvertible;
			if (convertible == null)
			{
				throw new InvalidCastException(Environment.GetResourceString("InvalidCast_IConvertible"));
			}
			return typeCode switch
			{
				TypeCode.Boolean => convertible.ToBoolean(provider), 
				TypeCode.Char => convertible.ToChar(provider), 
				TypeCode.SByte => convertible.ToSByte(provider), 
				TypeCode.Byte => convertible.ToByte(provider), 
				TypeCode.Int16 => convertible.ToInt16(provider), 
				TypeCode.UInt16 => convertible.ToUInt16(provider), 
				TypeCode.Int32 => convertible.ToInt32(provider), 
				TypeCode.UInt32 => convertible.ToUInt32(provider), 
				TypeCode.Int64 => convertible.ToInt64(provider), 
				TypeCode.UInt64 => convertible.ToUInt64(provider), 
				TypeCode.Single => convertible.ToSingle(provider), 
				TypeCode.Double => convertible.ToDouble(provider), 
				TypeCode.Decimal => convertible.ToDecimal(provider), 
				TypeCode.DateTime => convertible.ToDateTime(provider), 
				TypeCode.String => convertible.ToString(provider), 
				TypeCode.Object => value, 
				TypeCode.DBNull => throw new InvalidCastException(Environment.GetResourceString("InvalidCast_DBNull")), 
				TypeCode.Empty => throw new InvalidCastException(Environment.GetResourceString("InvalidCast_Empty")), 
				_ => throw new ArgumentException(Environment.GetResourceString("Arg_UnknownTypeCode")), 
			};
		}

		internal static object DefaultToType(IConvertible value, Type targetType, IFormatProvider provider)
		{
			if (targetType == null)
			{
				throw new ArgumentNullException("targetType");
			}
			if (value.GetType() == targetType)
			{
				return value;
			}
			if (targetType == ConvertTypes[3])
			{
				return value.ToBoolean(provider);
			}
			if (targetType == ConvertTypes[4])
			{
				return value.ToChar(provider);
			}
			if (targetType == ConvertTypes[5])
			{
				return value.ToSByte(provider);
			}
			if (targetType == ConvertTypes[6])
			{
				return value.ToByte(provider);
			}
			if (targetType == ConvertTypes[7])
			{
				return value.ToInt16(provider);
			}
			if (targetType == ConvertTypes[8])
			{
				return value.ToUInt16(provider);
			}
			if (targetType == ConvertTypes[9])
			{
				return value.ToInt32(provider);
			}
			if (targetType == ConvertTypes[10])
			{
				return value.ToUInt32(provider);
			}
			if (targetType == ConvertTypes[11])
			{
				return value.ToInt64(provider);
			}
			if (targetType == ConvertTypes[12])
			{
				return value.ToUInt64(provider);
			}
			if (targetType == ConvertTypes[13])
			{
				return value.ToSingle(provider);
			}
			if (targetType == ConvertTypes[14])
			{
				return value.ToDouble(provider);
			}
			if (targetType == ConvertTypes[15])
			{
				return value.ToDecimal(provider);
			}
			if (targetType == ConvertTypes[16])
			{
				return value.ToDateTime(provider);
			}
			if (targetType == ConvertTypes[18])
			{
				return value.ToString(provider);
			}
			if (targetType == ConvertTypes[1])
			{
				return value;
			}
			if (targetType == EnumType)
			{
				return (Enum)value;
			}
			if (targetType == ConvertTypes[2])
			{
				throw new InvalidCastException(Environment.GetResourceString("InvalidCast_DBNull"));
			}
			if (targetType == ConvertTypes[0])
			{
				throw new InvalidCastException(Environment.GetResourceString("InvalidCast_Empty"));
			}
			throw new InvalidCastException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("InvalidCast_FromTo"), value.GetType().FullName, targetType.FullName));
		}

		public static object ChangeType(object value, Type conversionType)
		{
			return ChangeType(value, conversionType, Thread.CurrentThread.CurrentCulture);
		}

		public static object ChangeType(object value, Type conversionType, IFormatProvider provider)
		{
			if (conversionType == null)
			{
				throw new ArgumentNullException("conversionType");
			}
			if (value == null)
			{
				if (conversionType.IsValueType)
				{
					throw new InvalidCastException(Environment.GetResourceString("InvalidCast_CannotCastNullToValueType"));
				}
				return null;
			}
			IConvertible convertible = value as IConvertible;
			if (convertible == null)
			{
				if (value.GetType() == conversionType)
				{
					return value;
				}
				throw new InvalidCastException(Environment.GetResourceString("InvalidCast_IConvertible"));
			}
			if (conversionType == ConvertTypes[3])
			{
				return convertible.ToBoolean(provider);
			}
			if (conversionType == ConvertTypes[4])
			{
				return convertible.ToChar(provider);
			}
			if (conversionType == ConvertTypes[5])
			{
				return convertible.ToSByte(provider);
			}
			if (conversionType == ConvertTypes[6])
			{
				return convertible.ToByte(provider);
			}
			if (conversionType == ConvertTypes[7])
			{
				return convertible.ToInt16(provider);
			}
			if (conversionType == ConvertTypes[8])
			{
				return convertible.ToUInt16(provider);
			}
			if (conversionType == ConvertTypes[9])
			{
				return convertible.ToInt32(provider);
			}
			if (conversionType == ConvertTypes[10])
			{
				return convertible.ToUInt32(provider);
			}
			if (conversionType == ConvertTypes[11])
			{
				return convertible.ToInt64(provider);
			}
			if (conversionType == ConvertTypes[12])
			{
				return convertible.ToUInt64(provider);
			}
			if (conversionType == ConvertTypes[13])
			{
				return convertible.ToSingle(provider);
			}
			if (conversionType == ConvertTypes[14])
			{
				return convertible.ToDouble(provider);
			}
			if (conversionType == ConvertTypes[15])
			{
				return convertible.ToDecimal(provider);
			}
			if (conversionType == ConvertTypes[16])
			{
				return convertible.ToDateTime(provider);
			}
			if (conversionType == ConvertTypes[18])
			{
				return convertible.ToString(provider);
			}
			if (conversionType == ConvertTypes[1])
			{
				return value;
			}
			return convertible.ToType(conversionType, provider);
		}

		public static bool ToBoolean(object value)
		{
			if (value != null)
			{
				return ((IConvertible)value).ToBoolean(null);
			}
			return false;
		}

		public static bool ToBoolean(object value, IFormatProvider provider)
		{
			if (value != null)
			{
				return ((IConvertible)value).ToBoolean(provider);
			}
			return false;
		}

		public static bool ToBoolean(bool value)
		{
			return value;
		}

		[CLSCompliant(false)]
		public static bool ToBoolean(sbyte value)
		{
			return value != 0;
		}

		public static bool ToBoolean(char value)
		{
			return ((IConvertible)value).ToBoolean((IFormatProvider)null);
		}

		public static bool ToBoolean(byte value)
		{
			return value != 0;
		}

		public static bool ToBoolean(short value)
		{
			return value != 0;
		}

		[CLSCompliant(false)]
		public static bool ToBoolean(ushort value)
		{
			return value != 0;
		}

		public static bool ToBoolean(int value)
		{
			return value != 0;
		}

		[CLSCompliant(false)]
		public static bool ToBoolean(uint value)
		{
			return value != 0;
		}

		public static bool ToBoolean(long value)
		{
			return value != 0;
		}

		[CLSCompliant(false)]
		public static bool ToBoolean(ulong value)
		{
			return value != 0;
		}

		public static bool ToBoolean(string value)
		{
			if (value == null)
			{
				return false;
			}
			return bool.Parse(value);
		}

		public static bool ToBoolean(string value, IFormatProvider provider)
		{
			if (value == null)
			{
				return false;
			}
			return bool.Parse(value);
		}

		public static bool ToBoolean(float value)
		{
			return value != 0f;
		}

		public static bool ToBoolean(double value)
		{
			return value != 0.0;
		}

		public static bool ToBoolean(decimal value)
		{
			return value != 0m;
		}

		public static bool ToBoolean(DateTime value)
		{
			return ((IConvertible)value).ToBoolean((IFormatProvider)null);
		}

		public static char ToChar(object value)
		{
			if (value != null)
			{
				return ((IConvertible)value).ToChar(null);
			}
			return '\0';
		}

		public static char ToChar(object value, IFormatProvider provider)
		{
			if (value != null)
			{
				return ((IConvertible)value).ToChar(provider);
			}
			return '\0';
		}

		public static char ToChar(bool value)
		{
			return ((IConvertible)value).ToChar((IFormatProvider)null);
		}

		public static char ToChar(char value)
		{
			return value;
		}

		[CLSCompliant(false)]
		public static char ToChar(sbyte value)
		{
			if (value < 0)
			{
				throw new OverflowException(Environment.GetResourceString("Overflow_Char"));
			}
			return (char)value;
		}

		public static char ToChar(byte value)
		{
			return (char)value;
		}

		public static char ToChar(short value)
		{
			if (value < 0)
			{
				throw new OverflowException(Environment.GetResourceString("Overflow_Char"));
			}
			return (char)value;
		}

		[CLSCompliant(false)]
		public static char ToChar(ushort value)
		{
			return (char)value;
		}

		public static char ToChar(int value)
		{
			if (value < 0 || value > 65535)
			{
				throw new OverflowException(Environment.GetResourceString("Overflow_Char"));
			}
			return (char)value;
		}

		[CLSCompliant(false)]
		public static char ToChar(uint value)
		{
			if (value > 65535)
			{
				throw new OverflowException(Environment.GetResourceString("Overflow_Char"));
			}
			return (char)value;
		}

		public static char ToChar(long value)
		{
			if (value < 0 || value > 65535)
			{
				throw new OverflowException(Environment.GetResourceString("Overflow_Char"));
			}
			return (char)value;
		}

		[CLSCompliant(false)]
		public static char ToChar(ulong value)
		{
			if (value > 65535)
			{
				throw new OverflowException(Environment.GetResourceString("Overflow_Char"));
			}
			return (char)value;
		}

		public static char ToChar(string value)
		{
			return ToChar(value, null);
		}

		public static char ToChar(string value, IFormatProvider provider)
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			if (value.Length != 1)
			{
				throw new FormatException(Environment.GetResourceString("Format_NeedSingleChar"));
			}
			return value[0];
		}

		public static char ToChar(float value)
		{
			return ((IConvertible)value).ToChar((IFormatProvider)null);
		}

		public static char ToChar(double value)
		{
			return ((IConvertible)value).ToChar((IFormatProvider)null);
		}

		public static char ToChar(decimal value)
		{
			return ((IConvertible)value).ToChar((IFormatProvider)null);
		}

		public static char ToChar(DateTime value)
		{
			return ((IConvertible)value).ToChar((IFormatProvider)null);
		}

		[CLSCompliant(false)]
		public static sbyte ToSByte(object value)
		{
			if (value != null)
			{
				return ((IConvertible)value).ToSByte(null);
			}
			return 0;
		}

		[CLSCompliant(false)]
		public static sbyte ToSByte(object value, IFormatProvider provider)
		{
			if (value != null)
			{
				return ((IConvertible)value).ToSByte(provider);
			}
			return 0;
		}

		[CLSCompliant(false)]
		public static sbyte ToSByte(bool value)
		{
			if (!value)
			{
				return 0;
			}
			return 1;
		}

		[CLSCompliant(false)]
		public static sbyte ToSByte(sbyte value)
		{
			return value;
		}

		[CLSCompliant(false)]
		public static sbyte ToSByte(char value)
		{
			if (value > '\u007f')
			{
				throw new OverflowException(Environment.GetResourceString("Overflow_SByte"));
			}
			return (sbyte)value;
		}

		[CLSCompliant(false)]
		public static sbyte ToSByte(byte value)
		{
			if (value > 127)
			{
				throw new OverflowException(Environment.GetResourceString("Overflow_SByte"));
			}
			return (sbyte)value;
		}

		[CLSCompliant(false)]
		public static sbyte ToSByte(short value)
		{
			if (value < -128 || value > 127)
			{
				throw new OverflowException(Environment.GetResourceString("Overflow_SByte"));
			}
			return (sbyte)value;
		}

		[CLSCompliant(false)]
		public static sbyte ToSByte(ushort value)
		{
			if (value > 127)
			{
				throw new OverflowException(Environment.GetResourceString("Overflow_SByte"));
			}
			return (sbyte)value;
		}

		[CLSCompliant(false)]
		public static sbyte ToSByte(int value)
		{
			if (value < -128 || value > 127)
			{
				throw new OverflowException(Environment.GetResourceString("Overflow_SByte"));
			}
			return (sbyte)value;
		}

		[CLSCompliant(false)]
		public static sbyte ToSByte(uint value)
		{
			if ((long)value > 127L)
			{
				throw new OverflowException(Environment.GetResourceString("Overflow_SByte"));
			}
			return (sbyte)value;
		}

		[CLSCompliant(false)]
		public static sbyte ToSByte(long value)
		{
			if (value < -128 || value > 127)
			{
				throw new OverflowException(Environment.GetResourceString("Overflow_SByte"));
			}
			return (sbyte)value;
		}

		[CLSCompliant(false)]
		public static sbyte ToSByte(ulong value)
		{
			if (value > 127)
			{
				throw new OverflowException(Environment.GetResourceString("Overflow_SByte"));
			}
			return (sbyte)value;
		}

		[CLSCompliant(false)]
		public static sbyte ToSByte(float value)
		{
			return ToSByte((double)value);
		}

		[CLSCompliant(false)]
		public static sbyte ToSByte(double value)
		{
			return ToSByte(ToInt32(value));
		}

		[CLSCompliant(false)]
		public static sbyte ToSByte(decimal value)
		{
			return decimal.ToSByte(decimal.Round(value, 0));
		}

		[CLSCompliant(false)]
		public static sbyte ToSByte(string value)
		{
			if (value == null)
			{
				return 0;
			}
			return sbyte.Parse(value, CultureInfo.CurrentCulture);
		}

		[CLSCompliant(false)]
		public static sbyte ToSByte(string value, IFormatProvider provider)
		{
			return sbyte.Parse(value, NumberStyles.Integer, provider);
		}

		[CLSCompliant(false)]
		public static sbyte ToSByte(DateTime value)
		{
			return ((IConvertible)value).ToSByte((IFormatProvider)null);
		}

		public static byte ToByte(object value)
		{
			if (value != null)
			{
				return ((IConvertible)value).ToByte(null);
			}
			return 0;
		}

		public static byte ToByte(object value, IFormatProvider provider)
		{
			if (value != null)
			{
				return ((IConvertible)value).ToByte(provider);
			}
			return 0;
		}

		public static byte ToByte(bool value)
		{
			if (!value)
			{
				return 0;
			}
			return 1;
		}

		public static byte ToByte(byte value)
		{
			return value;
		}

		public static byte ToByte(char value)
		{
			if (value > 'ÿ')
			{
				throw new OverflowException(Environment.GetResourceString("Overflow_Byte"));
			}
			return (byte)value;
		}

		[CLSCompliant(false)]
		public static byte ToByte(sbyte value)
		{
			if (value < 0)
			{
				throw new OverflowException(Environment.GetResourceString("Overflow_Byte"));
			}
			return (byte)value;
		}

		public static byte ToByte(short value)
		{
			if (value < 0 || value > 255)
			{
				throw new OverflowException(Environment.GetResourceString("Overflow_Byte"));
			}
			return (byte)value;
		}

		[CLSCompliant(false)]
		public static byte ToByte(ushort value)
		{
			if (value > 255)
			{
				throw new OverflowException(Environment.GetResourceString("Overflow_Byte"));
			}
			return (byte)value;
		}

		public static byte ToByte(int value)
		{
			if (value < 0 || value > 255)
			{
				throw new OverflowException(Environment.GetResourceString("Overflow_Byte"));
			}
			return (byte)value;
		}

		[CLSCompliant(false)]
		public static byte ToByte(uint value)
		{
			if (value > 255)
			{
				throw new OverflowException(Environment.GetResourceString("Overflow_Byte"));
			}
			return (byte)value;
		}

		public static byte ToByte(long value)
		{
			if (value < 0 || value > 255)
			{
				throw new OverflowException(Environment.GetResourceString("Overflow_Byte"));
			}
			return (byte)value;
		}

		[CLSCompliant(false)]
		public static byte ToByte(ulong value)
		{
			if (value > 255)
			{
				throw new OverflowException(Environment.GetResourceString("Overflow_Byte"));
			}
			return (byte)value;
		}

		public static byte ToByte(float value)
		{
			return ToByte((double)value);
		}

		public static byte ToByte(double value)
		{
			return ToByte(ToInt32(value));
		}

		public static byte ToByte(decimal value)
		{
			return decimal.ToByte(decimal.Round(value, 0));
		}

		public static byte ToByte(string value)
		{
			if (value == null)
			{
				return 0;
			}
			return byte.Parse(value, CultureInfo.CurrentCulture);
		}

		public static byte ToByte(string value, IFormatProvider provider)
		{
			if (value == null)
			{
				return 0;
			}
			return byte.Parse(value, NumberStyles.Integer, provider);
		}

		public static byte ToByte(DateTime value)
		{
			return ((IConvertible)value).ToByte((IFormatProvider)null);
		}

		public static short ToInt16(object value)
		{
			if (value != null)
			{
				return ((IConvertible)value).ToInt16(null);
			}
			return 0;
		}

		public static short ToInt16(object value, IFormatProvider provider)
		{
			if (value != null)
			{
				return ((IConvertible)value).ToInt16(provider);
			}
			return 0;
		}

		public static short ToInt16(bool value)
		{
			if (!value)
			{
				return 0;
			}
			return 1;
		}

		public static short ToInt16(char value)
		{
			if (value > '翿')
			{
				throw new OverflowException(Environment.GetResourceString("Overflow_Int16"));
			}
			return (short)value;
		}

		[CLSCompliant(false)]
		public static short ToInt16(sbyte value)
		{
			return value;
		}

		public static short ToInt16(byte value)
		{
			return value;
		}

		[CLSCompliant(false)]
		public static short ToInt16(ushort value)
		{
			if (value > 32767)
			{
				throw new OverflowException(Environment.GetResourceString("Overflow_Int16"));
			}
			return (short)value;
		}

		public static short ToInt16(int value)
		{
			if (value < -32768 || value > 32767)
			{
				throw new OverflowException(Environment.GetResourceString("Overflow_Int16"));
			}
			return (short)value;
		}

		[CLSCompliant(false)]
		public static short ToInt16(uint value)
		{
			if ((long)value > 32767L)
			{
				throw new OverflowException(Environment.GetResourceString("Overflow_Int16"));
			}
			return (short)value;
		}

		public static short ToInt16(short value)
		{
			return value;
		}

		public static short ToInt16(long value)
		{
			if (value < -32768 || value > 32767)
			{
				throw new OverflowException(Environment.GetResourceString("Overflow_Int16"));
			}
			return (short)value;
		}

		[CLSCompliant(false)]
		public static short ToInt16(ulong value)
		{
			if (value > 32767)
			{
				throw new OverflowException(Environment.GetResourceString("Overflow_Int16"));
			}
			return (short)value;
		}

		public static short ToInt16(float value)
		{
			return ToInt16((double)value);
		}

		public static short ToInt16(double value)
		{
			return ToInt16(ToInt32(value));
		}

		public static short ToInt16(decimal value)
		{
			return decimal.ToInt16(decimal.Round(value, 0));
		}

		public static short ToInt16(string value)
		{
			if (value == null)
			{
				return 0;
			}
			return short.Parse(value, CultureInfo.CurrentCulture);
		}

		public static short ToInt16(string value, IFormatProvider provider)
		{
			if (value == null)
			{
				return 0;
			}
			return short.Parse(value, NumberStyles.Integer, provider);
		}

		public static short ToInt16(DateTime value)
		{
			return ((IConvertible)value).ToInt16((IFormatProvider)null);
		}

		[CLSCompliant(false)]
		public static ushort ToUInt16(object value)
		{
			if (value != null)
			{
				return ((IConvertible)value).ToUInt16(null);
			}
			return 0;
		}

		[CLSCompliant(false)]
		public static ushort ToUInt16(object value, IFormatProvider provider)
		{
			if (value != null)
			{
				return ((IConvertible)value).ToUInt16(provider);
			}
			return 0;
		}

		[CLSCompliant(false)]
		public static ushort ToUInt16(bool value)
		{
			if (!value)
			{
				return 0;
			}
			return 1;
		}

		[CLSCompliant(false)]
		public static ushort ToUInt16(char value)
		{
			return value;
		}

		[CLSCompliant(false)]
		public static ushort ToUInt16(sbyte value)
		{
			if (value < 0)
			{
				throw new OverflowException(Environment.GetResourceString("Overflow_UInt16"));
			}
			return (ushort)value;
		}

		[CLSCompliant(false)]
		public static ushort ToUInt16(byte value)
		{
			return value;
		}

		[CLSCompliant(false)]
		public static ushort ToUInt16(short value)
		{
			if (value < 0)
			{
				throw new OverflowException(Environment.GetResourceString("Overflow_UInt16"));
			}
			return (ushort)value;
		}

		[CLSCompliant(false)]
		public static ushort ToUInt16(int value)
		{
			if (value < 0 || value > 65535)
			{
				throw new OverflowException(Environment.GetResourceString("Overflow_UInt16"));
			}
			return (ushort)value;
		}

		[CLSCompliant(false)]
		public static ushort ToUInt16(ushort value)
		{
			return value;
		}

		[CLSCompliant(false)]
		public static ushort ToUInt16(uint value)
		{
			if (value > 65535)
			{
				throw new OverflowException(Environment.GetResourceString("Overflow_UInt16"));
			}
			return (ushort)value;
		}

		[CLSCompliant(false)]
		public static ushort ToUInt16(long value)
		{
			if (value < 0 || value > 65535)
			{
				throw new OverflowException(Environment.GetResourceString("Overflow_UInt16"));
			}
			return (ushort)value;
		}

		[CLSCompliant(false)]
		public static ushort ToUInt16(ulong value)
		{
			if (value > 65535)
			{
				throw new OverflowException(Environment.GetResourceString("Overflow_UInt16"));
			}
			return (ushort)value;
		}

		[CLSCompliant(false)]
		public static ushort ToUInt16(float value)
		{
			return ToUInt16((double)value);
		}

		[CLSCompliant(false)]
		public static ushort ToUInt16(double value)
		{
			return ToUInt16(ToInt32(value));
		}

		[CLSCompliant(false)]
		public static ushort ToUInt16(decimal value)
		{
			return decimal.ToUInt16(decimal.Round(value, 0));
		}

		[CLSCompliant(false)]
		public static ushort ToUInt16(string value)
		{
			if (value == null)
			{
				return 0;
			}
			return ushort.Parse(value, CultureInfo.CurrentCulture);
		}

		[CLSCompliant(false)]
		public static ushort ToUInt16(string value, IFormatProvider provider)
		{
			if (value == null)
			{
				return 0;
			}
			return ushort.Parse(value, NumberStyles.Integer, provider);
		}

		[CLSCompliant(false)]
		public static ushort ToUInt16(DateTime value)
		{
			return ((IConvertible)value).ToUInt16((IFormatProvider)null);
		}

		public static int ToInt32(object value)
		{
			if (value != null)
			{
				return ((IConvertible)value).ToInt32(null);
			}
			return 0;
		}

		public static int ToInt32(object value, IFormatProvider provider)
		{
			if (value != null)
			{
				return ((IConvertible)value).ToInt32(provider);
			}
			return 0;
		}

		public static int ToInt32(bool value)
		{
			if (!value)
			{
				return 0;
			}
			return 1;
		}

		public static int ToInt32(char value)
		{
			return value;
		}

		[CLSCompliant(false)]
		public static int ToInt32(sbyte value)
		{
			return value;
		}

		public static int ToInt32(byte value)
		{
			return value;
		}

		public static int ToInt32(short value)
		{
			return value;
		}

		[CLSCompliant(false)]
		public static int ToInt32(ushort value)
		{
			return value;
		}

		[CLSCompliant(false)]
		public static int ToInt32(uint value)
		{
			if (value > int.MaxValue)
			{
				throw new OverflowException(Environment.GetResourceString("Overflow_Int32"));
			}
			return (int)value;
		}

		public static int ToInt32(int value)
		{
			return value;
		}

		public static int ToInt32(long value)
		{
			if (value < int.MinValue || value > int.MaxValue)
			{
				throw new OverflowException(Environment.GetResourceString("Overflow_Int32"));
			}
			return (int)value;
		}

		[CLSCompliant(false)]
		public static int ToInt32(ulong value)
		{
			if (value > int.MaxValue)
			{
				throw new OverflowException(Environment.GetResourceString("Overflow_Int32"));
			}
			return (int)value;
		}

		public static int ToInt32(float value)
		{
			return ToInt32((double)value);
		}

		public static int ToInt32(double value)
		{
			if (value >= 0.0)
			{
				if (value < 2147483647.5)
				{
					int num = (int)value;
					double num2 = value - (double)num;
					if (num2 > 0.5 || (num2 == 0.5 && ((uint)num & (true ? 1u : 0u)) != 0))
					{
						num++;
					}
					return num;
				}
			}
			else if (value >= -2147483648.5)
			{
				int num3 = (int)value;
				double num4 = value - (double)num3;
				if (num4 < -0.5 || (num4 == -0.5 && ((uint)num3 & (true ? 1u : 0u)) != 0))
				{
					num3--;
				}
				return num3;
			}
			throw new OverflowException(Environment.GetResourceString("Overflow_Int32"));
		}

		public static int ToInt32(decimal value)
		{
			return decimal.FCallToInt32(value);
		}

		public static int ToInt32(string value)
		{
			if (value == null)
			{
				return 0;
			}
			return int.Parse(value, CultureInfo.CurrentCulture);
		}

		public static int ToInt32(string value, IFormatProvider provider)
		{
			if (value == null)
			{
				return 0;
			}
			return int.Parse(value, NumberStyles.Integer, provider);
		}

		public static int ToInt32(DateTime value)
		{
			return ((IConvertible)value).ToInt32((IFormatProvider)null);
		}

		[CLSCompliant(false)]
		public static uint ToUInt32(object value)
		{
			if (value != null)
			{
				return ((IConvertible)value).ToUInt32(null);
			}
			return 0u;
		}

		[CLSCompliant(false)]
		public static uint ToUInt32(object value, IFormatProvider provider)
		{
			if (value != null)
			{
				return ((IConvertible)value).ToUInt32(provider);
			}
			return 0u;
		}

		[CLSCompliant(false)]
		public static uint ToUInt32(bool value)
		{
			if (!value)
			{
				return 0u;
			}
			return 1u;
		}

		[CLSCompliant(false)]
		public static uint ToUInt32(char value)
		{
			return value;
		}

		[CLSCompliant(false)]
		public static uint ToUInt32(sbyte value)
		{
			if (value < 0)
			{
				throw new OverflowException(Environment.GetResourceString("Overflow_UInt32"));
			}
			return (uint)value;
		}

		[CLSCompliant(false)]
		public static uint ToUInt32(byte value)
		{
			return value;
		}

		[CLSCompliant(false)]
		public static uint ToUInt32(short value)
		{
			if (value < 0)
			{
				throw new OverflowException(Environment.GetResourceString("Overflow_UInt32"));
			}
			return (uint)value;
		}

		[CLSCompliant(false)]
		public static uint ToUInt32(ushort value)
		{
			return value;
		}

		[CLSCompliant(false)]
		public static uint ToUInt32(int value)
		{
			if (value < 0)
			{
				throw new OverflowException(Environment.GetResourceString("Overflow_UInt32"));
			}
			return (uint)value;
		}

		[CLSCompliant(false)]
		public static uint ToUInt32(uint value)
		{
			return value;
		}

		[CLSCompliant(false)]
		public static uint ToUInt32(long value)
		{
			if (value < 0 || value > uint.MaxValue)
			{
				throw new OverflowException(Environment.GetResourceString("Overflow_UInt32"));
			}
			return (uint)value;
		}

		[CLSCompliant(false)]
		public static uint ToUInt32(ulong value)
		{
			if (value > uint.MaxValue)
			{
				throw new OverflowException(Environment.GetResourceString("Overflow_UInt32"));
			}
			return (uint)value;
		}

		[CLSCompliant(false)]
		public static uint ToUInt32(float value)
		{
			return ToUInt32((double)value);
		}

		[CLSCompliant(false)]
		public static uint ToUInt32(double value)
		{
			if (value >= -0.5 && value < 4294967295.5)
			{
				uint num = (uint)value;
				double num2 = value - (double)num;
				if (num2 > 0.5 || (num2 == 0.5 && (num & (true ? 1u : 0u)) != 0))
				{
					num++;
				}
				return num;
			}
			throw new OverflowException(Environment.GetResourceString("Overflow_UInt32"));
		}

		[CLSCompliant(false)]
		public static uint ToUInt32(decimal value)
		{
			return decimal.ToUInt32(decimal.Round(value, 0));
		}

		[CLSCompliant(false)]
		public static uint ToUInt32(string value)
		{
			if (value == null)
			{
				return 0u;
			}
			return uint.Parse(value, CultureInfo.CurrentCulture);
		}

		[CLSCompliant(false)]
		public static uint ToUInt32(string value, IFormatProvider provider)
		{
			if (value == null)
			{
				return 0u;
			}
			return uint.Parse(value, NumberStyles.Integer, provider);
		}

		[CLSCompliant(false)]
		public static uint ToUInt32(DateTime value)
		{
			return ((IConvertible)value).ToUInt32((IFormatProvider)null);
		}

		public static long ToInt64(object value)
		{
			if (value != null)
			{
				return ((IConvertible)value).ToInt64(null);
			}
			return 0L;
		}

		public static long ToInt64(object value, IFormatProvider provider)
		{
			if (value != null)
			{
				return ((IConvertible)value).ToInt64(provider);
			}
			return 0L;
		}

		public static long ToInt64(bool value)
		{
			return value ? 1 : 0;
		}

		public static long ToInt64(char value)
		{
			return value;
		}

		[CLSCompliant(false)]
		public static long ToInt64(sbyte value)
		{
			return value;
		}

		public static long ToInt64(byte value)
		{
			return value;
		}

		public static long ToInt64(short value)
		{
			return value;
		}

		[CLSCompliant(false)]
		public static long ToInt64(ushort value)
		{
			return value;
		}

		public static long ToInt64(int value)
		{
			return value;
		}

		[CLSCompliant(false)]
		public static long ToInt64(uint value)
		{
			return value;
		}

		[CLSCompliant(false)]
		public static long ToInt64(ulong value)
		{
			if (value > long.MaxValue)
			{
				throw new OverflowException(Environment.GetResourceString("Overflow_Int64"));
			}
			return (long)value;
		}

		public static long ToInt64(long value)
		{
			return value;
		}

		public static long ToInt64(float value)
		{
			return ToInt64((double)value);
		}

		public static long ToInt64(double value)
		{
			return checked((long)Math.Round(value));
		}

		public static long ToInt64(decimal value)
		{
			return decimal.ToInt64(decimal.Round(value, 0));
		}

		public static long ToInt64(string value)
		{
			if (value == null)
			{
				return 0L;
			}
			return long.Parse(value, CultureInfo.CurrentCulture);
		}

		public static long ToInt64(string value, IFormatProvider provider)
		{
			if (value == null)
			{
				return 0L;
			}
			return long.Parse(value, NumberStyles.Integer, provider);
		}

		public static long ToInt64(DateTime value)
		{
			return ((IConvertible)value).ToInt64((IFormatProvider)null);
		}

		[CLSCompliant(false)]
		public static ulong ToUInt64(object value)
		{
			if (value != null)
			{
				return ((IConvertible)value).ToUInt64(null);
			}
			return 0uL;
		}

		[CLSCompliant(false)]
		public static ulong ToUInt64(object value, IFormatProvider provider)
		{
			if (value != null)
			{
				return ((IConvertible)value).ToUInt64(provider);
			}
			return 0uL;
		}

		[CLSCompliant(false)]
		public static ulong ToUInt64(bool value)
		{
			if (!value)
			{
				return 0uL;
			}
			return 1uL;
		}

		[CLSCompliant(false)]
		public static ulong ToUInt64(char value)
		{
			return value;
		}

		[CLSCompliant(false)]
		public static ulong ToUInt64(sbyte value)
		{
			if (value < 0)
			{
				throw new OverflowException(Environment.GetResourceString("Overflow_UInt64"));
			}
			return (ulong)value;
		}

		[CLSCompliant(false)]
		public static ulong ToUInt64(byte value)
		{
			return value;
		}

		[CLSCompliant(false)]
		public static ulong ToUInt64(short value)
		{
			if (value < 0)
			{
				throw new OverflowException(Environment.GetResourceString("Overflow_UInt64"));
			}
			return (ulong)value;
		}

		[CLSCompliant(false)]
		public static ulong ToUInt64(ushort value)
		{
			return value;
		}

		[CLSCompliant(false)]
		public static ulong ToUInt64(int value)
		{
			if (value < 0)
			{
				throw new OverflowException(Environment.GetResourceString("Overflow_UInt64"));
			}
			return (ulong)value;
		}

		[CLSCompliant(false)]
		public static ulong ToUInt64(uint value)
		{
			return value;
		}

		[CLSCompliant(false)]
		public static ulong ToUInt64(long value)
		{
			if (value < 0)
			{
				throw new OverflowException(Environment.GetResourceString("Overflow_UInt64"));
			}
			return (ulong)value;
		}

		[CLSCompliant(false)]
		public static ulong ToUInt64(ulong value)
		{
			return value;
		}

		[CLSCompliant(false)]
		public static ulong ToUInt64(float value)
		{
			return ToUInt64((double)value);
		}

		[CLSCompliant(false)]
		public static ulong ToUInt64(double value)
		{
			return checked((ulong)Math.Round(value));
		}

		[CLSCompliant(false)]
		public static ulong ToUInt64(decimal value)
		{
			return decimal.ToUInt64(decimal.Round(value, 0));
		}

		[CLSCompliant(false)]
		public static ulong ToUInt64(string value)
		{
			if (value == null)
			{
				return 0uL;
			}
			return ulong.Parse(value, CultureInfo.CurrentCulture);
		}

		[CLSCompliant(false)]
		public static ulong ToUInt64(string value, IFormatProvider provider)
		{
			if (value == null)
			{
				return 0uL;
			}
			return ulong.Parse(value, NumberStyles.Integer, provider);
		}

		[CLSCompliant(false)]
		public static ulong ToUInt64(DateTime value)
		{
			return ((IConvertible)value).ToUInt64((IFormatProvider)null);
		}

		public static float ToSingle(object value)
		{
			if (value != null)
			{
				return ((IConvertible)value).ToSingle(null);
			}
			return 0f;
		}

		public static float ToSingle(object value, IFormatProvider provider)
		{
			if (value != null)
			{
				return ((IConvertible)value).ToSingle(provider);
			}
			return 0f;
		}

		[CLSCompliant(false)]
		public static float ToSingle(sbyte value)
		{
			return value;
		}

		public static float ToSingle(byte value)
		{
			return (int)value;
		}

		public static float ToSingle(char value)
		{
			return ((IConvertible)value).ToSingle((IFormatProvider)null);
		}

		public static float ToSingle(short value)
		{
			return value;
		}

		[CLSCompliant(false)]
		public static float ToSingle(ushort value)
		{
			return (int)value;
		}

		public static float ToSingle(int value)
		{
			return value;
		}

		[CLSCompliant(false)]
		public static float ToSingle(uint value)
		{
			return value;
		}

		public static float ToSingle(long value)
		{
			return value;
		}

		[CLSCompliant(false)]
		public static float ToSingle(ulong value)
		{
			return value;
		}

		public static float ToSingle(float value)
		{
			return value;
		}

		public static float ToSingle(double value)
		{
			return (float)value;
		}

		public static float ToSingle(decimal value)
		{
			return (float)value;
		}

		public static float ToSingle(string value)
		{
			if (value == null)
			{
				return 0f;
			}
			return float.Parse(value, CultureInfo.CurrentCulture);
		}

		public static float ToSingle(string value, IFormatProvider provider)
		{
			if (value == null)
			{
				return 0f;
			}
			return float.Parse(value, NumberStyles.Float | NumberStyles.AllowThousands, provider);
		}

		public static float ToSingle(bool value)
		{
			return value ? 1 : 0;
		}

		public static float ToSingle(DateTime value)
		{
			return ((IConvertible)value).ToSingle((IFormatProvider)null);
		}

		public static double ToDouble(object value)
		{
			if (value != null)
			{
				return ((IConvertible)value).ToDouble(null);
			}
			return 0.0;
		}

		public static double ToDouble(object value, IFormatProvider provider)
		{
			if (value != null)
			{
				return ((IConvertible)value).ToDouble(provider);
			}
			return 0.0;
		}

		[CLSCompliant(false)]
		public static double ToDouble(sbyte value)
		{
			return value;
		}

		public static double ToDouble(byte value)
		{
			return (int)value;
		}

		public static double ToDouble(short value)
		{
			return value;
		}

		public static double ToDouble(char value)
		{
			return ((IConvertible)value).ToDouble((IFormatProvider)null);
		}

		[CLSCompliant(false)]
		public static double ToDouble(ushort value)
		{
			return (int)value;
		}

		public static double ToDouble(int value)
		{
			return value;
		}

		[CLSCompliant(false)]
		public static double ToDouble(uint value)
		{
			return value;
		}

		public static double ToDouble(long value)
		{
			return value;
		}

		[CLSCompliant(false)]
		public static double ToDouble(ulong value)
		{
			return value;
		}

		public static double ToDouble(float value)
		{
			return value;
		}

		public static double ToDouble(double value)
		{
			return value;
		}

		public static double ToDouble(decimal value)
		{
			return (double)value;
		}

		public static double ToDouble(string value)
		{
			if (value == null)
			{
				return 0.0;
			}
			return double.Parse(value, CultureInfo.CurrentCulture);
		}

		public static double ToDouble(string value, IFormatProvider provider)
		{
			if (value == null)
			{
				return 0.0;
			}
			return double.Parse(value, NumberStyles.Float | NumberStyles.AllowThousands, provider);
		}

		public static double ToDouble(bool value)
		{
			return value ? 1 : 0;
		}

		public static double ToDouble(DateTime value)
		{
			return ((IConvertible)value).ToDouble((IFormatProvider)null);
		}

		public static decimal ToDecimal(object value)
		{
			if (value != null)
			{
				return ((IConvertible)value).ToDecimal(null);
			}
			return 0m;
		}

		public static decimal ToDecimal(object value, IFormatProvider provider)
		{
			if (value != null)
			{
				return ((IConvertible)value).ToDecimal(provider);
			}
			return 0m;
		}

		[CLSCompliant(false)]
		public static decimal ToDecimal(sbyte value)
		{
			return value;
		}

		public static decimal ToDecimal(byte value)
		{
			return value;
		}

		public static decimal ToDecimal(char value)
		{
			return ((IConvertible)value).ToDecimal((IFormatProvider)null);
		}

		public static decimal ToDecimal(short value)
		{
			return value;
		}

		[CLSCompliant(false)]
		public static decimal ToDecimal(ushort value)
		{
			return value;
		}

		public static decimal ToDecimal(int value)
		{
			return value;
		}

		[CLSCompliant(false)]
		public static decimal ToDecimal(uint value)
		{
			return value;
		}

		public static decimal ToDecimal(long value)
		{
			return value;
		}

		[CLSCompliant(false)]
		public static decimal ToDecimal(ulong value)
		{
			return value;
		}

		public static decimal ToDecimal(float value)
		{
			return (decimal)value;
		}

		public static decimal ToDecimal(double value)
		{
			return (decimal)value;
		}

		public static decimal ToDecimal(string value)
		{
			if (value == null)
			{
				return 0m;
			}
			return decimal.Parse(value, CultureInfo.CurrentCulture);
		}

		public static decimal ToDecimal(string value, IFormatProvider provider)
		{
			if (value == null)
			{
				return 0m;
			}
			return decimal.Parse(value, NumberStyles.Number, provider);
		}

		public static decimal ToDecimal(decimal value)
		{
			return value;
		}

		public static decimal ToDecimal(bool value)
		{
			return value ? 1 : 0;
		}

		public static decimal ToDecimal(DateTime value)
		{
			return ((IConvertible)value).ToDecimal((IFormatProvider)null);
		}

		public static DateTime ToDateTime(DateTime value)
		{
			return value;
		}

		public static DateTime ToDateTime(object value)
		{
			if (value != null)
			{
				return ((IConvertible)value).ToDateTime(null);
			}
			return DateTime.MinValue;
		}

		public static DateTime ToDateTime(object value, IFormatProvider provider)
		{
			if (value != null)
			{
				return ((IConvertible)value).ToDateTime(provider);
			}
			return DateTime.MinValue;
		}

		public static DateTime ToDateTime(string value)
		{
			if (value == null)
			{
				return new DateTime(0L);
			}
			return DateTime.Parse(value, CultureInfo.CurrentCulture);
		}

		public static DateTime ToDateTime(string value, IFormatProvider provider)
		{
			if (value == null)
			{
				return new DateTime(0L);
			}
			return DateTime.Parse(value, provider);
		}

		[CLSCompliant(false)]
		public static DateTime ToDateTime(sbyte value)
		{
			return ((IConvertible)value).ToDateTime((IFormatProvider)null);
		}

		public static DateTime ToDateTime(byte value)
		{
			return ((IConvertible)value).ToDateTime((IFormatProvider)null);
		}

		public static DateTime ToDateTime(short value)
		{
			return ((IConvertible)value).ToDateTime((IFormatProvider)null);
		}

		[CLSCompliant(false)]
		public static DateTime ToDateTime(ushort value)
		{
			return ((IConvertible)value).ToDateTime((IFormatProvider)null);
		}

		public static DateTime ToDateTime(int value)
		{
			return ((IConvertible)value).ToDateTime((IFormatProvider)null);
		}

		[CLSCompliant(false)]
		public static DateTime ToDateTime(uint value)
		{
			return ((IConvertible)value).ToDateTime((IFormatProvider)null);
		}

		public static DateTime ToDateTime(long value)
		{
			return ((IConvertible)value).ToDateTime((IFormatProvider)null);
		}

		[CLSCompliant(false)]
		public static DateTime ToDateTime(ulong value)
		{
			return ((IConvertible)value).ToDateTime((IFormatProvider)null);
		}

		public static DateTime ToDateTime(bool value)
		{
			return ((IConvertible)value).ToDateTime((IFormatProvider)null);
		}

		public static DateTime ToDateTime(char value)
		{
			return ((IConvertible)value).ToDateTime((IFormatProvider)null);
		}

		public static DateTime ToDateTime(float value)
		{
			return ((IConvertible)value).ToDateTime((IFormatProvider)null);
		}

		public static DateTime ToDateTime(double value)
		{
			return ((IConvertible)value).ToDateTime((IFormatProvider)null);
		}

		public static DateTime ToDateTime(decimal value)
		{
			return ((IConvertible)value).ToDateTime((IFormatProvider)null);
		}

		public static string ToString(object value)
		{
			return ToString(value, null);
		}

		public static string ToString(object value, IFormatProvider provider)
		{
			IConvertible convertible = value as IConvertible;
			if (convertible != null)
			{
				return convertible.ToString(provider);
			}
			IFormattable formattable = value as IFormattable;
			if (formattable != null)
			{
				return formattable.ToString(null, provider);
			}
			if (value != null)
			{
				return value.ToString();
			}
			return string.Empty;
		}

		public static string ToString(bool value)
		{
			return value.ToString();
		}

		public static string ToString(bool value, IFormatProvider provider)
		{
			return value.ToString(provider);
		}

		public static string ToString(char value)
		{
			return char.ToString(value);
		}

		public static string ToString(char value, IFormatProvider provider)
		{
			return value.ToString(provider);
		}

		[CLSCompliant(false)]
		public static string ToString(sbyte value)
		{
			return value.ToString(CultureInfo.CurrentCulture);
		}

		[CLSCompliant(false)]
		public static string ToString(sbyte value, IFormatProvider provider)
		{
			return value.ToString(provider);
		}

		public static string ToString(byte value)
		{
			return value.ToString(CultureInfo.CurrentCulture);
		}

		public static string ToString(byte value, IFormatProvider provider)
		{
			return value.ToString(provider);
		}

		public static string ToString(short value)
		{
			return value.ToString(CultureInfo.CurrentCulture);
		}

		public static string ToString(short value, IFormatProvider provider)
		{
			return value.ToString(provider);
		}

		[CLSCompliant(false)]
		public static string ToString(ushort value)
		{
			return value.ToString(CultureInfo.CurrentCulture);
		}

		[CLSCompliant(false)]
		public static string ToString(ushort value, IFormatProvider provider)
		{
			return value.ToString(provider);
		}

		public static string ToString(int value)
		{
			return value.ToString(CultureInfo.CurrentCulture);
		}

		public static string ToString(int value, IFormatProvider provider)
		{
			return value.ToString(provider);
		}

		[CLSCompliant(false)]
		public static string ToString(uint value)
		{
			return value.ToString(CultureInfo.CurrentCulture);
		}

		[CLSCompliant(false)]
		public static string ToString(uint value, IFormatProvider provider)
		{
			return value.ToString(provider);
		}

		public static string ToString(long value)
		{
			return value.ToString(CultureInfo.CurrentCulture);
		}

		public static string ToString(long value, IFormatProvider provider)
		{
			return value.ToString(provider);
		}

		[CLSCompliant(false)]
		public static string ToString(ulong value)
		{
			return value.ToString(CultureInfo.CurrentCulture);
		}

		[CLSCompliant(false)]
		public static string ToString(ulong value, IFormatProvider provider)
		{
			return value.ToString(provider);
		}

		public static string ToString(float value)
		{
			return value.ToString(CultureInfo.CurrentCulture);
		}

		public static string ToString(float value, IFormatProvider provider)
		{
			return value.ToString(provider);
		}

		public static string ToString(double value)
		{
			return value.ToString(CultureInfo.CurrentCulture);
		}

		public static string ToString(double value, IFormatProvider provider)
		{
			return value.ToString(provider);
		}

		public static string ToString(decimal value)
		{
			return value.ToString(CultureInfo.CurrentCulture);
		}

		public static string ToString(decimal value, IFormatProvider provider)
		{
			return value.ToString(provider);
		}

		public static string ToString(DateTime value)
		{
			return value.ToString();
		}

		public static string ToString(DateTime value, IFormatProvider provider)
		{
			return value.ToString(provider);
		}

		public static string ToString(string value)
		{
			return value;
		}

		public static string ToString(string value, IFormatProvider provider)
		{
			return value;
		}

		public static byte ToByte(string value, int fromBase)
		{
			if (fromBase != 2 && fromBase != 8 && fromBase != 10 && fromBase != 16)
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_InvalidBase"));
			}
			int num = ParseNumbers.StringToInt(value, fromBase, 4608);
			if (num < 0 || num > 255)
			{
				throw new OverflowException(Environment.GetResourceString("Overflow_Byte"));
			}
			return (byte)num;
		}

		[CLSCompliant(false)]
		public static sbyte ToSByte(string value, int fromBase)
		{
			if (fromBase != 2 && fromBase != 8 && fromBase != 10 && fromBase != 16)
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_InvalidBase"));
			}
			int num = ParseNumbers.StringToInt(value, fromBase, 5120);
			if (fromBase != 10 && num <= 255)
			{
				return (sbyte)num;
			}
			if (num < -128 || num > 127)
			{
				throw new OverflowException(Environment.GetResourceString("Overflow_SByte"));
			}
			return (sbyte)num;
		}

		public static short ToInt16(string value, int fromBase)
		{
			if (fromBase != 2 && fromBase != 8 && fromBase != 10 && fromBase != 16)
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_InvalidBase"));
			}
			int num = ParseNumbers.StringToInt(value, fromBase, 6144);
			if (fromBase != 10 && num <= 65535)
			{
				return (short)num;
			}
			if (num < -32768 || num > 32767)
			{
				throw new OverflowException(Environment.GetResourceString("Overflow_Int16"));
			}
			return (short)num;
		}

		[CLSCompliant(false)]
		public static ushort ToUInt16(string value, int fromBase)
		{
			if (fromBase != 2 && fromBase != 8 && fromBase != 10 && fromBase != 16)
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_InvalidBase"));
			}
			int num = ParseNumbers.StringToInt(value, fromBase, 4608);
			if (num < 0 || num > 65535)
			{
				throw new OverflowException(Environment.GetResourceString("Overflow_UInt16"));
			}
			return (ushort)num;
		}

		public static int ToInt32(string value, int fromBase)
		{
			if (fromBase != 2 && fromBase != 8 && fromBase != 10 && fromBase != 16)
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_InvalidBase"));
			}
			return ParseNumbers.StringToInt(value, fromBase, 4096);
		}

		[CLSCompliant(false)]
		public static uint ToUInt32(string value, int fromBase)
		{
			if (fromBase != 2 && fromBase != 8 && fromBase != 10 && fromBase != 16)
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_InvalidBase"));
			}
			return (uint)ParseNumbers.StringToInt(value, fromBase, 4608);
		}

		public static long ToInt64(string value, int fromBase)
		{
			if (fromBase != 2 && fromBase != 8 && fromBase != 10 && fromBase != 16)
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_InvalidBase"));
			}
			return ParseNumbers.StringToLong(value, fromBase, 4096);
		}

		[CLSCompliant(false)]
		public static ulong ToUInt64(string value, int fromBase)
		{
			if (fromBase != 2 && fromBase != 8 && fromBase != 10 && fromBase != 16)
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_InvalidBase"));
			}
			return (ulong)ParseNumbers.StringToLong(value, fromBase, 4608);
		}

		public static string ToString(byte value, int toBase)
		{
			if (toBase != 2 && toBase != 8 && toBase != 10 && toBase != 16)
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_InvalidBase"));
			}
			return ParseNumbers.IntToString(value, toBase, -1, ' ', 64);
		}

		public static string ToString(short value, int toBase)
		{
			if (toBase != 2 && toBase != 8 && toBase != 10 && toBase != 16)
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_InvalidBase"));
			}
			return ParseNumbers.IntToString(value, toBase, -1, ' ', 128);
		}

		public static string ToString(int value, int toBase)
		{
			if (toBase != 2 && toBase != 8 && toBase != 10 && toBase != 16)
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_InvalidBase"));
			}
			return ParseNumbers.IntToString(value, toBase, -1, ' ', 0);
		}

		public static string ToString(long value, int toBase)
		{
			if (toBase != 2 && toBase != 8 && toBase != 10 && toBase != 16)
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_InvalidBase"));
			}
			return ParseNumbers.LongToString(value, toBase, -1, ' ', 0);
		}

		public static string ToBase64String(byte[] inArray)
		{
			if (inArray == null)
			{
				throw new ArgumentNullException("inArray");
			}
			return ToBase64String(inArray, 0, inArray.Length, Base64FormattingOptions.None);
		}

		[ComVisible(false)]
		public static string ToBase64String(byte[] inArray, Base64FormattingOptions options)
		{
			if (inArray == null)
			{
				throw new ArgumentNullException("inArray");
			}
			return ToBase64String(inArray, 0, inArray.Length, options);
		}

		public static string ToBase64String(byte[] inArray, int offset, int length)
		{
			return ToBase64String(inArray, offset, length, Base64FormattingOptions.None);
		}

		[ComVisible(false)]
		public unsafe static string ToBase64String(byte[] inArray, int offset, int length, Base64FormattingOptions options)
		{
			if (inArray == null)
			{
				throw new ArgumentNullException("inArray");
			}
			if (length < 0)
			{
				throw new ArgumentOutOfRangeException("length", Environment.GetResourceString("ArgumentOutOfRange_Index"));
			}
			if (offset < 0)
			{
				throw new ArgumentOutOfRangeException("offset", Environment.GetResourceString("ArgumentOutOfRange_GenericPositive"));
			}
			int num = inArray.Length;
			if (offset > num - length)
			{
				throw new ArgumentOutOfRangeException("offset", Environment.GetResourceString("ArgumentOutOfRange_OffsetLength"));
			}
			if (options < Base64FormattingOptions.None || options > Base64FormattingOptions.InsertLineBreaks)
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_EnumIllegalVal", (int)options));
			}
			if (num == 0)
			{
				return string.Empty;
			}
			bool insertLineBreaks = options == Base64FormattingOptions.InsertLineBreaks;
			int capacity = CalculateOutputLength(length, insertLineBreaks);
			string stringForStringBuilder = string.GetStringForStringBuilder(string.Empty, capacity);
			fixed (char* outChars = stringForStringBuilder)
			{
				fixed (byte* inData = inArray)
				{
					int length2 = ConvertToBase64Array(outChars, inData, offset, length, insertLineBreaks);
					stringForStringBuilder.SetLength(length2);
					return stringForStringBuilder;
				}
			}
		}

		public static int ToBase64CharArray(byte[] inArray, int offsetIn, int length, char[] outArray, int offsetOut)
		{
			return ToBase64CharArray(inArray, offsetIn, length, outArray, offsetOut, Base64FormattingOptions.None);
		}

		[ComVisible(false)]
		public unsafe static int ToBase64CharArray(byte[] inArray, int offsetIn, int length, char[] outArray, int offsetOut, Base64FormattingOptions options)
		{
			if (inArray == null)
			{
				throw new ArgumentNullException("inArray");
			}
			if (outArray == null)
			{
				throw new ArgumentNullException("outArray");
			}
			if (length < 0)
			{
				throw new ArgumentOutOfRangeException("length", Environment.GetResourceString("ArgumentOutOfRange_Index"));
			}
			if (offsetIn < 0)
			{
				throw new ArgumentOutOfRangeException("offsetIn", Environment.GetResourceString("ArgumentOutOfRange_GenericPositive"));
			}
			if (offsetOut < 0)
			{
				throw new ArgumentOutOfRangeException("offsetOut", Environment.GetResourceString("ArgumentOutOfRange_GenericPositive"));
			}
			if (options < Base64FormattingOptions.None || options > Base64FormattingOptions.InsertLineBreaks)
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_EnumIllegalVal", (int)options));
			}
			int num = inArray.Length;
			if (offsetIn > num - length)
			{
				throw new ArgumentOutOfRangeException("offsetIn", Environment.GetResourceString("ArgumentOutOfRange_OffsetLength"));
			}
			if (num == 0)
			{
				return 0;
			}
			bool insertLineBreaks = options == Base64FormattingOptions.InsertLineBreaks;
			int num2 = outArray.Length;
			int num3 = CalculateOutputLength(length, insertLineBreaks);
			if (offsetOut > num2 - num3)
			{
				throw new ArgumentOutOfRangeException("offsetOut", Environment.GetResourceString("ArgumentOutOfRange_OffsetOut"));
			}
			int result;
			fixed (char* outChars = &outArray[offsetOut])
			{
				fixed (byte* inData = inArray)
				{
					result = ConvertToBase64Array(outChars, inData, offsetIn, length, insertLineBreaks);
				}
			}
			return result;
		}

		private unsafe static int ConvertToBase64Array(char* outChars, byte* inData, int offset, int length, bool insertLineBreaks)
		{
			int num = length % 3;
			int num2 = offset + (length - num);
			int num3 = 0;
			int num4 = 0;
			fixed (char* ptr = base64Table)
			{
				int i;
				for (i = offset; i < num2; i += 3)
				{
					if (insertLineBreaks)
					{
						if (num4 == 76)
						{
							outChars[num3++] = '\r';
							outChars[num3++] = '\n';
							num4 = 0;
						}
						num4 += 4;
					}
					outChars[num3] = ptr[(inData[i] & 0xFC) >> 2];
					outChars[num3 + 1] = ptr[((inData[i] & 3) << 4) | ((inData[i + 1] & 0xF0) >> 4)];
					outChars[num3 + 2] = ptr[((inData[i + 1] & 0xF) << 2) | ((inData[i + 2] & 0xC0) >> 6)];
					outChars[num3 + 3] = ptr[inData[i + 2] & 0x3F];
					num3 += 4;
				}
				i = num2;
				if (insertLineBreaks && num != 0 && num4 == 76)
				{
					outChars[num3++] = '\r';
					outChars[num3++] = '\n';
				}
				switch (num)
				{
				case 2:
					outChars[num3] = ptr[(inData[i] & 0xFC) >> 2];
					outChars[num3 + 1] = ptr[((inData[i] & 3) << 4) | ((inData[i + 1] & 0xF0) >> 4)];
					outChars[num3 + 2] = ptr[(inData[i + 1] & 0xF) << 2];
					outChars[num3 + 3] = ptr[64];
					num3 += 4;
					break;
				case 1:
					outChars[num3] = ptr[(inData[i] & 0xFC) >> 2];
					outChars[num3 + 1] = ptr[(inData[i] & 3) << 4];
					outChars[num3 + 2] = ptr[64];
					outChars[num3 + 3] = ptr[64];
					num3 += 4;
					break;
				}
			}
			return num3;
		}

		private static int CalculateOutputLength(int inputLength, bool insertLineBreaks)
		{
			int num = inputLength / 3 * 4;
			num += ((inputLength % 3 != 0) ? 4 : 0);
			if (num == 0)
			{
				return num;
			}
			if (insertLineBreaks)
			{
				int num2 = num / 76;
				if (num % 76 == 0)
				{
					num2--;
				}
				num += num2 * 2;
			}
			return num;
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		public static extern byte[] FromBase64String(string s);

		[MethodImpl(MethodImplOptions.InternalCall)]
		public static extern byte[] FromBase64CharArray(char[] inArray, int offset, int length);
	}
}
