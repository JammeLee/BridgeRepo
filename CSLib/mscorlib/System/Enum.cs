using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace System
{
	[Serializable]
	[ComVisible(true)]
	public abstract class Enum : ValueType, IComparable, IFormattable, IConvertible
	{
		private class HashEntry
		{
			public string[] names;

			public ulong[] values;

			public HashEntry(string[] names, ulong[] values)
			{
				this.names = names;
				this.values = values;
			}
		}

		private const string enumSeperator = ", ";

		private const int maxHashElements = 100;

		private static char[] enumSeperatorCharArray = new char[1]
		{
			','
		};

		private static Type intType = typeof(int);

		private static Type stringType = typeof(string);

		private static Hashtable fieldInfoHash = Hashtable.Synchronized(new Hashtable());

		private static FieldInfo GetValueField(Type type)
		{
			FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (fields == null || fields.Length != 1)
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_EnumMustHaveUnderlyingValueField"));
			}
			return fields[0];
		}

		private static HashEntry GetHashEntry(Type enumType)
		{
			HashEntry hashEntry = (HashEntry)fieldInfoHash[enumType];
			if (hashEntry == null)
			{
				if (fieldInfoHash.Count > 100)
				{
					fieldInfoHash.Clear();
				}
				ulong[] values = null;
				string[] names = null;
				if (enumType.BaseType == typeof(Enum))
				{
					InternalGetEnumValues(enumType, ref values, ref names);
				}
				else
				{
					FieldInfo[] fields = enumType.GetFields(BindingFlags.Static | BindingFlags.Public);
					values = new ulong[fields.Length];
					names = new string[fields.Length];
					for (int i = 0; i < fields.Length; i++)
					{
						names[i] = fields[i].Name;
						values[i] = ToUInt64(fields[i].GetValue(null));
					}
					for (int j = 1; j < values.Length; j++)
					{
						int num = j;
						string text = names[j];
						ulong num2 = values[j];
						bool flag = false;
						while (values[num - 1] > num2)
						{
							names[num] = names[num - 1];
							values[num] = values[num - 1];
							num--;
							flag = true;
							if (num == 0)
							{
								break;
							}
						}
						if (flag)
						{
							names[num] = text;
							values[num] = num2;
						}
					}
				}
				hashEntry = new HashEntry(names, values);
				fieldInfoHash[enumType] = hashEntry;
			}
			return hashEntry;
		}

		private static string InternalGetValueAsString(Type enumType, object value)
		{
			HashEntry hashEntry = GetHashEntry(enumType);
			Type underlyingType = GetUnderlyingType(enumType);
			if (underlyingType == intType || underlyingType == typeof(short) || underlyingType == typeof(long) || underlyingType == typeof(ushort) || underlyingType == typeof(byte) || underlyingType == typeof(sbyte) || underlyingType == typeof(uint) || underlyingType == typeof(ulong))
			{
				ulong value2 = ToUInt64(value);
				int num = BinarySearch(hashEntry.values, value2);
				if (num >= 0)
				{
					return hashEntry.names[num];
				}
			}
			return null;
		}

		private static string InternalFormattedHexString(object value)
		{
			return Convert.GetTypeCode(value) switch
			{
				TypeCode.SByte => ((byte)(sbyte)value).ToString("X2", null), 
				TypeCode.Byte => ((byte)value).ToString("X2", null), 
				TypeCode.Int16 => ((ushort)(short)value).ToString("X4", null), 
				TypeCode.UInt16 => ((ushort)value).ToString("X4", null), 
				TypeCode.UInt32 => ((uint)value).ToString("X8", null), 
				TypeCode.Int32 => ((uint)(int)value).ToString("X8", null), 
				TypeCode.UInt64 => ((ulong)value).ToString("X16", null), 
				TypeCode.Int64 => ((ulong)(long)value).ToString("X16", null), 
				_ => throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_UnknownEnumType")), 
			};
		}

		private static string InternalFormat(Type eT, object value)
		{
			if (!eT.IsDefined(typeof(FlagsAttribute), inherit: false))
			{
				string text = InternalGetValueAsString(eT, value);
				if (text == null)
				{
					return value.ToString();
				}
				return text;
			}
			return InternalFlagsFormat(eT, value);
		}

		private static string InternalFlagsFormat(Type eT, object value)
		{
			ulong num = ToUInt64(value);
			HashEntry hashEntry = GetHashEntry(eT);
			string[] names = hashEntry.names;
			ulong[] values = hashEntry.values;
			int num2 = values.Length - 1;
			StringBuilder stringBuilder = new StringBuilder();
			bool flag = true;
			ulong num3 = num;
			while (num2 >= 0 && (num2 != 0 || values[num2] != 0))
			{
				if ((num & values[num2]) == values[num2])
				{
					num -= values[num2];
					if (!flag)
					{
						stringBuilder.Insert(0, ", ");
					}
					stringBuilder.Insert(0, names[num2]);
					flag = false;
				}
				num2--;
			}
			if (num != 0)
			{
				return value.ToString();
			}
			if (num3 == 0)
			{
				if (values[0] == 0)
				{
					return names[0];
				}
				return "0";
			}
			return stringBuilder.ToString();
		}

		private static ulong ToUInt64(object value)
		{
			switch (Convert.GetTypeCode(value))
			{
			case TypeCode.SByte:
			case TypeCode.Int16:
			case TypeCode.Int32:
			case TypeCode.Int64:
				return (ulong)Convert.ToInt64(value, CultureInfo.InvariantCulture);
			case TypeCode.Byte:
			case TypeCode.UInt16:
			case TypeCode.UInt32:
			case TypeCode.UInt64:
				return Convert.ToUInt64(value, CultureInfo.InvariantCulture);
			default:
				throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_UnknownEnumType"));
			}
		}

		private static int BinarySearch(ulong[] array, ulong value)
		{
			int num = 0;
			int num2 = array.Length - 1;
			while (num <= num2)
			{
				int num3 = num + num2 >> 1;
				ulong num4 = array[num3];
				if (value == num4)
				{
					return num3;
				}
				if (num4 < value)
				{
					num = num3 + 1;
				}
				else
				{
					num2 = num3 - 1;
				}
			}
			return ~num;
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern int InternalCompareTo(object o1, object o2);

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern Type InternalGetUnderlyingType(Type enumType);

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void InternalGetEnumValues(Type enumType, ref ulong[] values, ref string[] names);

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern object InternalBoxEnum(Type enumType, long value);

		[ComVisible(true)]
		public static object Parse(Type enumType, string value)
		{
			return Parse(enumType, value, ignoreCase: false);
		}

		[ComVisible(true)]
		public static object Parse(Type enumType, string value, bool ignoreCase)
		{
			if (enumType == null)
			{
				throw new ArgumentNullException("enumType");
			}
			if (!(enumType is RuntimeType))
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_MustBeType"), "enumType");
			}
			if (!enumType.IsEnum)
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_MustBeEnum"), "enumType");
			}
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			value = value.Trim();
			if (value.Length == 0)
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_MustContainEnumInfo"));
			}
			ulong num = 0uL;
			if (char.IsDigit(value[0]) || value[0] == '-' || value[0] == '+')
			{
				Type underlyingType = GetUnderlyingType(enumType);
				try
				{
					object value2 = Convert.ChangeType(value, underlyingType, CultureInfo.InvariantCulture);
					return ToObject(enumType, value2);
				}
				catch (FormatException)
				{
				}
			}
			string[] array = value.Split(enumSeperatorCharArray);
			HashEntry hashEntry = GetHashEntry(enumType);
			string[] names = hashEntry.names;
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = array[i].Trim();
				bool flag = false;
				for (int j = 0; j < names.Length; j++)
				{
					if (ignoreCase)
					{
						if (string.Compare(names[j], array[i], StringComparison.OrdinalIgnoreCase) != 0)
						{
							continue;
						}
					}
					else if (!names[j].Equals(array[i]))
					{
						continue;
					}
					ulong num2 = hashEntry.values[j];
					num |= num2;
					flag = true;
					break;
				}
				if (!flag)
				{
					throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Arg_EnumValueNotFound"), value));
				}
			}
			return ToObject(enumType, num);
		}

		[ComVisible(true)]
		public static Type GetUnderlyingType(Type enumType)
		{
			if (enumType == null)
			{
				throw new ArgumentNullException("enumType");
			}
			if (enumType is EnumBuilder)
			{
				return ((EnumBuilder)enumType).UnderlyingSystemType;
			}
			if (!(enumType is RuntimeType))
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_MustBeType"), "enumType");
			}
			if (!enumType.IsEnum)
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_MustBeEnum"), "enumType");
			}
			return InternalGetUnderlyingType(enumType);
		}

		[ComVisible(true)]
		public static Array GetValues(Type enumType)
		{
			if (enumType == null)
			{
				throw new ArgumentNullException("enumType");
			}
			if (!(enumType is RuntimeType))
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_MustBeType"), "enumType");
			}
			if (!enumType.IsEnum)
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_MustBeEnum"), "enumType");
			}
			ulong[] values = GetHashEntry(enumType).values;
			Array array = Array.CreateInstance(enumType, values.Length);
			for (int i = 0; i < values.Length; i++)
			{
				object value = ToObject(enumType, values[i]);
				array.SetValue(value, i);
			}
			return array;
		}

		[ComVisible(true)]
		public static string GetName(Type enumType, object value)
		{
			if (enumType == null)
			{
				throw new ArgumentNullException("enumType");
			}
			if (!(enumType is RuntimeType))
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_MustBeType"), "enumType");
			}
			if (!enumType.IsEnum)
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_MustBeEnum"), "enumType");
			}
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			Type type = value.GetType();
			if (type.IsEnum || type == intType || type == typeof(short) || type == typeof(ushort) || type == typeof(byte) || type == typeof(sbyte) || type == typeof(uint) || type == typeof(long) || type == typeof(ulong))
			{
				return InternalGetValueAsString(enumType, value);
			}
			throw new ArgumentException(Environment.GetResourceString("Arg_MustBeEnumBaseTypeOrEnum"), "value");
		}

		[ComVisible(true)]
		public static string[] GetNames(Type enumType)
		{
			if (enumType == null)
			{
				throw new ArgumentNullException("enumType");
			}
			if (!(enumType is RuntimeType))
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_MustBeType"), "enumType");
			}
			if (!enumType.IsEnum)
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_MustBeEnum"), "enumType");
			}
			string[] names = GetHashEntry(enumType).names;
			string[] array = new string[names.Length];
			Array.Copy(names, array, names.Length);
			return array;
		}

		[ComVisible(true)]
		public static object ToObject(Type enumType, object value)
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			return Convert.GetTypeCode(value) switch
			{
				TypeCode.Int32 => ToObject(enumType, (int)value), 
				TypeCode.SByte => ToObject(enumType, (sbyte)value), 
				TypeCode.Int16 => ToObject(enumType, (short)value), 
				TypeCode.Int64 => ToObject(enumType, (long)value), 
				TypeCode.UInt32 => ToObject(enumType, (uint)value), 
				TypeCode.Byte => ToObject(enumType, (byte)value), 
				TypeCode.UInt16 => ToObject(enumType, (ushort)value), 
				TypeCode.UInt64 => ToObject(enumType, (ulong)value), 
				_ => throw new ArgumentException(Environment.GetResourceString("Arg_MustBeEnumBaseTypeOrEnum"), "value"), 
			};
		}

		[ComVisible(true)]
		public static bool IsDefined(Type enumType, object value)
		{
			if (enumType == null)
			{
				throw new ArgumentNullException("enumType");
			}
			if (!(enumType is RuntimeType))
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_MustBeType"), "enumType");
			}
			if (!enumType.IsEnum)
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_MustBeEnum"), "enumType");
			}
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			Type type = value.GetType();
			if (!(type is RuntimeType))
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_MustBeType"), "valueType");
			}
			Type underlyingType = GetUnderlyingType(enumType);
			if (type.IsEnum)
			{
				Type underlyingType2 = GetUnderlyingType(type);
				if (type != enumType)
				{
					throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Arg_EnumAndObjectMustBeSameType"), type.ToString(), enumType.ToString()));
				}
				type = underlyingType2;
			}
			else if (type != underlyingType && type != stringType)
			{
				throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Arg_EnumUnderlyingTypeAndObjectMustBeSameType"), type.ToString(), underlyingType.ToString()));
			}
			if (type == stringType)
			{
				string[] names = GetHashEntry(enumType).names;
				for (int i = 0; i < names.Length; i++)
				{
					if (names[i].Equals((string)value))
					{
						return true;
					}
				}
				return false;
			}
			ulong[] values = GetHashEntry(enumType).values;
			if (type == intType || type == typeof(short) || type == typeof(ushort) || type == typeof(byte) || type == typeof(sbyte) || type == typeof(uint) || type == typeof(long) || type == typeof(ulong))
			{
				ulong value2 = ToUInt64(value);
				return BinarySearch(values, value2) >= 0;
			}
			throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_UnknownEnumType"));
		}

		[ComVisible(true)]
		public static string Format(Type enumType, object value, string format)
		{
			if (enumType == null)
			{
				throw new ArgumentNullException("enumType");
			}
			if (!(enumType is RuntimeType))
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_MustBeType"), "enumType");
			}
			if (!enumType.IsEnum)
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_MustBeEnum"), "enumType");
			}
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			if (format == null)
			{
				throw new ArgumentNullException("format");
			}
			Type type = value.GetType();
			if (!(type is RuntimeType))
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_MustBeType"), "valueType");
			}
			Type underlyingType = GetUnderlyingType(enumType);
			if (type.IsEnum)
			{
				Type underlyingType2 = GetUnderlyingType(type);
				if (type != enumType)
				{
					throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Arg_EnumAndObjectMustBeSameType"), type.ToString(), enumType.ToString()));
				}
				type = underlyingType2;
				value = ((Enum)value).GetValue();
			}
			else if (type != underlyingType)
			{
				throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Arg_EnumFormatUnderlyingTypeAndObjectMustBeSameType"), type.ToString(), underlyingType.ToString()));
			}
			if (format.Length != 1)
			{
				throw new FormatException(Environment.GetResourceString("Format_InvalidEnumFormatSpecification"));
			}
			switch (format[0])
			{
			case 'D':
			case 'd':
				return value.ToString();
			case 'X':
			case 'x':
				return InternalFormattedHexString(value);
			case 'G':
			case 'g':
				return InternalFormat(enumType, value);
			case 'F':
			case 'f':
				return InternalFlagsFormat(enumType, value);
			default:
				throw new FormatException(Environment.GetResourceString("Format_InvalidEnumFormatSpecification"));
			}
		}

		private object GetValue()
		{
			return InternalGetValue();
		}

		private string ToHexString()
		{
			Type type = GetType();
			FieldInfo valueField = GetValueField(type);
			return InternalFormattedHexString(((RtFieldInfo)valueField).InternalGetValue(this, doVisibilityCheck: false));
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern object InternalGetValue();

		[MethodImpl(MethodImplOptions.InternalCall)]
		public override extern bool Equals(object obj);

		public override int GetHashCode()
		{
			return GetValue().GetHashCode();
		}

		public override string ToString()
		{
			Type type = GetType();
			FieldInfo valueField = GetValueField(type);
			object value = ((RtFieldInfo)valueField).InternalGetValue(this, doVisibilityCheck: false);
			return InternalFormat(type, value);
		}

		[Obsolete("The provider argument is not used. Please use ToString(String).")]
		public string ToString(string format, IFormatProvider provider)
		{
			return ToString(format);
		}

		public int CompareTo(object target)
		{
			if (this == null)
			{
				throw new NullReferenceException();
			}
			int num = InternalCompareTo(this, target);
			if (num < 2)
			{
				return num;
			}
			if (num == 2)
			{
				Type type = GetType();
				Type type2 = target.GetType();
				throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Arg_EnumAndObjectMustBeSameType"), type2.ToString(), type.ToString()));
			}
			throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_UnknownEnumType"));
		}

		public string ToString(string format)
		{
			if (format == null || format.Length == 0)
			{
				format = "G";
			}
			if (string.Compare(format, "G", StringComparison.OrdinalIgnoreCase) == 0)
			{
				return ToString();
			}
			if (string.Compare(format, "D", StringComparison.OrdinalIgnoreCase) == 0)
			{
				return GetValue().ToString();
			}
			if (string.Compare(format, "X", StringComparison.OrdinalIgnoreCase) == 0)
			{
				return ToHexString();
			}
			if (string.Compare(format, "F", StringComparison.OrdinalIgnoreCase) == 0)
			{
				return InternalFlagsFormat(GetType(), GetValue());
			}
			throw new FormatException(Environment.GetResourceString("Format_InvalidEnumFormatSpecification"));
		}

		[Obsolete("The provider argument is not used. Please use ToString().")]
		public string ToString(IFormatProvider provider)
		{
			return ToString();
		}

		public TypeCode GetTypeCode()
		{
			Type type = GetType();
			Type underlyingType = GetUnderlyingType(type);
			if (underlyingType == typeof(int))
			{
				return TypeCode.Int32;
			}
			if (underlyingType == typeof(sbyte))
			{
				return TypeCode.SByte;
			}
			if (underlyingType == typeof(short))
			{
				return TypeCode.Int16;
			}
			if (underlyingType == typeof(long))
			{
				return TypeCode.Int64;
			}
			if (underlyingType == typeof(uint))
			{
				return TypeCode.UInt32;
			}
			if (underlyingType == typeof(byte))
			{
				return TypeCode.Byte;
			}
			if (underlyingType == typeof(ushort))
			{
				return TypeCode.UInt16;
			}
			if (underlyingType == typeof(ulong))
			{
				return TypeCode.UInt64;
			}
			throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_UnknownEnumType"));
		}

		bool IConvertible.ToBoolean(IFormatProvider provider)
		{
			return Convert.ToBoolean(GetValue(), CultureInfo.CurrentCulture);
		}

		char IConvertible.ToChar(IFormatProvider provider)
		{
			return Convert.ToChar(GetValue(), CultureInfo.CurrentCulture);
		}

		sbyte IConvertible.ToSByte(IFormatProvider provider)
		{
			return Convert.ToSByte(GetValue(), CultureInfo.CurrentCulture);
		}

		byte IConvertible.ToByte(IFormatProvider provider)
		{
			return Convert.ToByte(GetValue(), CultureInfo.CurrentCulture);
		}

		short IConvertible.ToInt16(IFormatProvider provider)
		{
			return Convert.ToInt16(GetValue(), CultureInfo.CurrentCulture);
		}

		ushort IConvertible.ToUInt16(IFormatProvider provider)
		{
			return Convert.ToUInt16(GetValue(), CultureInfo.CurrentCulture);
		}

		int IConvertible.ToInt32(IFormatProvider provider)
		{
			return Convert.ToInt32(GetValue(), CultureInfo.CurrentCulture);
		}

		uint IConvertible.ToUInt32(IFormatProvider provider)
		{
			return Convert.ToUInt32(GetValue(), CultureInfo.CurrentCulture);
		}

		long IConvertible.ToInt64(IFormatProvider provider)
		{
			return Convert.ToInt64(GetValue(), CultureInfo.CurrentCulture);
		}

		ulong IConvertible.ToUInt64(IFormatProvider provider)
		{
			return Convert.ToUInt64(GetValue(), CultureInfo.CurrentCulture);
		}

		float IConvertible.ToSingle(IFormatProvider provider)
		{
			return Convert.ToSingle(GetValue(), CultureInfo.CurrentCulture);
		}

		double IConvertible.ToDouble(IFormatProvider provider)
		{
			return Convert.ToDouble(GetValue(), CultureInfo.CurrentCulture);
		}

		decimal IConvertible.ToDecimal(IFormatProvider provider)
		{
			return Convert.ToDecimal(GetValue(), CultureInfo.CurrentCulture);
		}

		DateTime IConvertible.ToDateTime(IFormatProvider provider)
		{
			throw new InvalidCastException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("InvalidCast_FromTo"), "Enum", "DateTime"));
		}

		object IConvertible.ToType(Type type, IFormatProvider provider)
		{
			return Convert.DefaultToType(this, type, provider);
		}

		[ComVisible(true)]
		[CLSCompliant(false)]
		public static object ToObject(Type enumType, sbyte value)
		{
			if (enumType == null)
			{
				throw new ArgumentNullException("enumType");
			}
			if (!(enumType is RuntimeType))
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_MustBeType"), "enumType");
			}
			if (!enumType.IsEnum)
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_MustBeEnum"), "enumType");
			}
			return InternalBoxEnum(enumType, value);
		}

		[ComVisible(true)]
		public static object ToObject(Type enumType, short value)
		{
			if (enumType == null)
			{
				throw new ArgumentNullException("enumType");
			}
			if (!(enumType is RuntimeType))
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_MustBeType"), "enumType");
			}
			if (!enumType.IsEnum)
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_MustBeEnum"), "enumType");
			}
			return InternalBoxEnum(enumType, value);
		}

		[ComVisible(true)]
		public static object ToObject(Type enumType, int value)
		{
			if (enumType == null)
			{
				throw new ArgumentNullException("enumType");
			}
			if (!(enumType is RuntimeType))
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_MustBeType"), "enumType");
			}
			if (!enumType.IsEnum)
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_MustBeEnum"), "enumType");
			}
			return InternalBoxEnum(enumType, value);
		}

		[ComVisible(true)]
		public static object ToObject(Type enumType, byte value)
		{
			if (enumType == null)
			{
				throw new ArgumentNullException("enumType");
			}
			if (!(enumType is RuntimeType))
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_MustBeType"), "enumType");
			}
			if (!enumType.IsEnum)
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_MustBeEnum"), "enumType");
			}
			return InternalBoxEnum(enumType, value);
		}

		[CLSCompliant(false)]
		[ComVisible(true)]
		public static object ToObject(Type enumType, ushort value)
		{
			if (enumType == null)
			{
				throw new ArgumentNullException("enumType");
			}
			if (!(enumType is RuntimeType))
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_MustBeType"), "enumType");
			}
			if (!enumType.IsEnum)
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_MustBeEnum"), "enumType");
			}
			return InternalBoxEnum(enumType, value);
		}

		[ComVisible(true)]
		[CLSCompliant(false)]
		public static object ToObject(Type enumType, uint value)
		{
			if (enumType == null)
			{
				throw new ArgumentNullException("enumType");
			}
			if (!(enumType is RuntimeType))
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_MustBeType"), "enumType");
			}
			if (!enumType.IsEnum)
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_MustBeEnum"), "enumType");
			}
			return InternalBoxEnum(enumType, value);
		}

		[ComVisible(true)]
		public static object ToObject(Type enumType, long value)
		{
			if (enumType == null)
			{
				throw new ArgumentNullException("enumType");
			}
			if (!(enumType is RuntimeType))
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_MustBeType"), "enumType");
			}
			if (!enumType.IsEnum)
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_MustBeEnum"), "enumType");
			}
			return InternalBoxEnum(enumType, value);
		}

		[ComVisible(true)]
		[CLSCompliant(false)]
		public static object ToObject(Type enumType, ulong value)
		{
			if (enumType == null)
			{
				throw new ArgumentNullException("enumType");
			}
			if (!(enumType is RuntimeType))
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_MustBeType"), "enumType");
			}
			if (!enumType.IsEnum)
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_MustBeEnum"), "enumType");
			}
			return InternalBoxEnum(enumType, (long)value);
		}
	}
}
