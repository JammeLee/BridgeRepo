using System.Globalization;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;

namespace System
{
	[Serializable]
	[ComVisible(true)]
	public struct Single : IComparable, IFormattable, IConvertible, IComparable<float>, IEquatable<float>
	{
		public const float MinValue = -3.40282347E+38f;

		public const float Epsilon = 1.401298E-45f;

		public const float MaxValue = 3.40282347E+38f;

		public const float PositiveInfinity = 1f / 0f;

		public const float NegativeInfinity = -1f / 0f;

		public const float NaN = 0f / 0f;

		internal float m_value;

		public unsafe static bool IsInfinity(float f)
		{
			return (*(int*)(&f) & 0x7FFFFFFF) == 2139095040;
		}

		public unsafe static bool IsPositiveInfinity(float f)
		{
			return *(int*)(&f) == 2139095040;
		}

		public unsafe static bool IsNegativeInfinity(float f)
		{
			return *(int*)(&f) == -8388608;
		}

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		public static bool IsNaN(float f)
		{
			if (f != f)
			{
				return true;
			}
			return false;
		}

		public int CompareTo(object value)
		{
			if (value == null)
			{
				return 1;
			}
			if (value is float)
			{
				float num = (float)value;
				if (this < num)
				{
					return -1;
				}
				if (this > num)
				{
					return 1;
				}
				if (this == num)
				{
					return 0;
				}
				if (IsNaN(this))
				{
					if (!IsNaN(num))
					{
						return -1;
					}
					return 0;
				}
				return 1;
			}
			throw new ArgumentException(Environment.GetResourceString("Arg_MustBeSingle"));
		}

		public int CompareTo(float value)
		{
			if (this < value)
			{
				return -1;
			}
			if (this > value)
			{
				return 1;
			}
			if (this == value)
			{
				return 0;
			}
			if (IsNaN(this))
			{
				if (!IsNaN(value))
				{
					return -1;
				}
				return 0;
			}
			return 1;
		}

		public override bool Equals(object obj)
		{
			if (!(obj is float))
			{
				return false;
			}
			float num = (float)obj;
			if (num == this)
			{
				return true;
			}
			if (IsNaN(num))
			{
				return IsNaN(this);
			}
			return false;
		}

		public bool Equals(float obj)
		{
			if (obj == this)
			{
				return true;
			}
			if (IsNaN(obj))
			{
				return IsNaN(this);
			}
			return false;
		}

		public unsafe override int GetHashCode()
		{
			float num = this;
			if (num == 0f)
			{
				return 0;
			}
			return *(int*)(&num);
		}

		public override string ToString()
		{
			return Number.FormatSingle(this, null, NumberFormatInfo.CurrentInfo);
		}

		public string ToString(IFormatProvider provider)
		{
			return Number.FormatSingle(this, null, NumberFormatInfo.GetInstance(provider));
		}

		public string ToString(string format)
		{
			return Number.FormatSingle(this, format, NumberFormatInfo.CurrentInfo);
		}

		public string ToString(string format, IFormatProvider provider)
		{
			return Number.FormatSingle(this, format, NumberFormatInfo.GetInstance(provider));
		}

		public static float Parse(string s)
		{
			return Parse(s, NumberStyles.Float | NumberStyles.AllowThousands, NumberFormatInfo.CurrentInfo);
		}

		public static float Parse(string s, NumberStyles style)
		{
			NumberFormatInfo.ValidateParseStyleFloatingPoint(style);
			return Parse(s, style, NumberFormatInfo.CurrentInfo);
		}

		public static float Parse(string s, IFormatProvider provider)
		{
			return Parse(s, NumberStyles.Float | NumberStyles.AllowThousands, NumberFormatInfo.GetInstance(provider));
		}

		public static float Parse(string s, NumberStyles style, IFormatProvider provider)
		{
			NumberFormatInfo.ValidateParseStyleFloatingPoint(style);
			return Parse(s, style, NumberFormatInfo.GetInstance(provider));
		}

		private static float Parse(string s, NumberStyles style, NumberFormatInfo info)
		{
			try
			{
				return Number.ParseSingle(s, style, info);
			}
			catch (FormatException)
			{
				string text = s.Trim();
				if (text.Equals(info.PositiveInfinitySymbol))
				{
					return float.PositiveInfinity;
				}
				if (text.Equals(info.NegativeInfinitySymbol))
				{
					return float.NegativeInfinity;
				}
				if (text.Equals(info.NaNSymbol))
				{
					return float.NaN;
				}
				throw;
			}
		}

		public static bool TryParse(string s, out float result)
		{
			return TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands, NumberFormatInfo.CurrentInfo, out result);
		}

		public static bool TryParse(string s, NumberStyles style, IFormatProvider provider, out float result)
		{
			NumberFormatInfo.ValidateParseStyleFloatingPoint(style);
			return TryParse(s, style, NumberFormatInfo.GetInstance(provider), out result);
		}

		private static bool TryParse(string s, NumberStyles style, NumberFormatInfo info, out float result)
		{
			if (s == null)
			{
				result = 0f;
				return false;
			}
			if (!Number.TryParseSingle(s, style, info, out result))
			{
				string text = s.Trim();
				if (text.Equals(info.PositiveInfinitySymbol))
				{
					result = float.PositiveInfinity;
				}
				else if (text.Equals(info.NegativeInfinitySymbol))
				{
					result = float.NegativeInfinity;
				}
				else
				{
					if (!text.Equals(info.NaNSymbol))
					{
						return false;
					}
					result = float.NaN;
				}
			}
			return true;
		}

		public TypeCode GetTypeCode()
		{
			return TypeCode.Single;
		}

		bool IConvertible.ToBoolean(IFormatProvider provider)
		{
			return Convert.ToBoolean(this);
		}

		char IConvertible.ToChar(IFormatProvider provider)
		{
			throw new InvalidCastException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("InvalidCast_FromTo"), "Single", "Char"));
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
			return this;
		}

		double IConvertible.ToDouble(IFormatProvider provider)
		{
			return Convert.ToDouble(this);
		}

		decimal IConvertible.ToDecimal(IFormatProvider provider)
		{
			return Convert.ToDecimal(this);
		}

		DateTime IConvertible.ToDateTime(IFormatProvider provider)
		{
			throw new InvalidCastException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("InvalidCast_FromTo"), "Single", "DateTime"));
		}

		object IConvertible.ToType(Type type, IFormatProvider provider)
		{
			return Convert.DefaultToType(this, type, provider);
		}
	}
}
