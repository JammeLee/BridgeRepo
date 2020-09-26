namespace System
{
	public static class BitConverter
	{
		public static readonly bool IsLittleEndian = true;

		public static byte[] GetBytes(bool value)
		{
			return new byte[1]
			{
				(byte)(value ? 1 : 0)
			};
		}

		public static byte[] GetBytes(char value)
		{
			return GetBytes((short)value);
		}

		public unsafe static byte[] GetBytes(short value)
		{
			byte[] array = new byte[2];
			fixed (byte* ptr = array)
			{
				*(short*)ptr = value;
			}
			return array;
		}

		public unsafe static byte[] GetBytes(int value)
		{
			byte[] array = new byte[4];
			fixed (byte* ptr = array)
			{
				*(int*)ptr = value;
			}
			return array;
		}

		public unsafe static byte[] GetBytes(long value)
		{
			byte[] array = new byte[8];
			fixed (byte* ptr = array)
			{
				*(long*)ptr = value;
			}
			return array;
		}

		[CLSCompliant(false)]
		public static byte[] GetBytes(ushort value)
		{
			return GetBytes((short)value);
		}

		[CLSCompliant(false)]
		public static byte[] GetBytes(uint value)
		{
			return GetBytes((int)value);
		}

		[CLSCompliant(false)]
		public static byte[] GetBytes(ulong value)
		{
			return GetBytes((long)value);
		}

		public unsafe static byte[] GetBytes(float value)
		{
			return GetBytes(*(int*)(&value));
		}

		public unsafe static byte[] GetBytes(double value)
		{
			return GetBytes(*(long*)(&value));
		}

		public static char ToChar(byte[] value, int startIndex)
		{
			return (char)ToInt16(value, startIndex);
		}

		public unsafe static short ToInt16(byte[] value, int startIndex)
		{
			if (value == null)
			{
				ThrowHelper.ThrowArgumentNullException(ExceptionArgument.value);
			}
			if ((uint)startIndex >= value.Length)
			{
				ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.startIndex, ExceptionResource.ArgumentOutOfRange_Index);
			}
			if (startIndex > value.Length - 2)
			{
				ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_ArrayPlusOffTooSmall);
			}
			fixed (byte* ptr = &value[startIndex])
			{
				if (startIndex % 2 == 0)
				{
					return *(short*)ptr;
				}
				if (IsLittleEndian)
				{
					return (short)(*ptr | (ptr[1] << 8));
				}
				return (short)((*ptr << 8) | ptr[1]);
			}
		}

		public unsafe static int ToInt32(byte[] value, int startIndex)
		{
			if (value == null)
			{
				ThrowHelper.ThrowArgumentNullException(ExceptionArgument.value);
			}
			if ((uint)startIndex >= value.Length)
			{
				ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.startIndex, ExceptionResource.ArgumentOutOfRange_Index);
			}
			if (startIndex > value.Length - 4)
			{
				ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_ArrayPlusOffTooSmall);
			}
			fixed (byte* ptr = &value[startIndex])
			{
				if (startIndex % 4 == 0)
				{
					return *(int*)ptr;
				}
				if (IsLittleEndian)
				{
					return *ptr | (ptr[1] << 8) | (ptr[2] << 16) | (ptr[3] << 24);
				}
				return (*ptr << 24) | (ptr[1] << 16) | (ptr[2] << 8) | ptr[3];
			}
		}

		public unsafe static long ToInt64(byte[] value, int startIndex)
		{
			if (value == null)
			{
				ThrowHelper.ThrowArgumentNullException(ExceptionArgument.value);
			}
			if ((uint)startIndex >= value.Length)
			{
				ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.startIndex, ExceptionResource.ArgumentOutOfRange_Index);
			}
			if (startIndex > value.Length - 8)
			{
				ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_ArrayPlusOffTooSmall);
			}
			fixed (byte* ptr = &value[startIndex])
			{
				if (startIndex % 8 == 0)
				{
					return *(long*)ptr;
				}
				if (IsLittleEndian)
				{
					int num = *ptr | (ptr[1] << 8) | (ptr[2] << 16) | (ptr[3] << 24);
					int num2 = ptr[4] | (ptr[5] << 8) | (ptr[6] << 16) | (ptr[7] << 24);
					return (uint)num | ((long)num2 << 32);
				}
				int num3 = (*ptr << 24) | (ptr[1] << 16) | (ptr[2] << 8) | ptr[3];
				int num4 = (ptr[4] << 24) | (ptr[5] << 16) | (ptr[6] << 8) | ptr[7];
				return (uint)num4 | ((long)num3 << 32);
			}
		}

		[CLSCompliant(false)]
		public static ushort ToUInt16(byte[] value, int startIndex)
		{
			return (ushort)ToInt16(value, startIndex);
		}

		[CLSCompliant(false)]
		public static uint ToUInt32(byte[] value, int startIndex)
		{
			return (uint)ToInt32(value, startIndex);
		}

		[CLSCompliant(false)]
		public static ulong ToUInt64(byte[] value, int startIndex)
		{
			return (ulong)ToInt64(value, startIndex);
		}

		public unsafe static float ToSingle(byte[] value, int startIndex)
		{
			int num = ToInt32(value, startIndex);
			return *(float*)(&num);
		}

		public unsafe static double ToDouble(byte[] value, int startIndex)
		{
			long num = ToInt64(value, startIndex);
			return *(double*)(&num);
		}

		private static char GetHexValue(int i)
		{
			if (i < 10)
			{
				return (char)(i + 48);
			}
			return (char)(i - 10 + 65);
		}

		public static string ToString(byte[] value, int startIndex, int length)
		{
			if (value == null)
			{
				throw new ArgumentNullException("byteArray");
			}
			int num = value.Length;
			if (startIndex < 0 || (startIndex >= num && startIndex > 0))
			{
				throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("ArgumentOutOfRange_StartIndex"));
			}
			if (length < 0)
			{
				throw new ArgumentOutOfRangeException("length", Environment.GetResourceString("ArgumentOutOfRange_GenericPositive"));
			}
			if (startIndex > num - length)
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_ArrayPlusOffTooSmall"));
			}
			if (length == 0)
			{
				return string.Empty;
			}
			char[] array = new char[length * 3];
			int num2 = 0;
			int num3 = startIndex;
			for (num2 = 0; num2 < length * 3; num2 += 3)
			{
				byte b = value[num3++];
				array[num2] = GetHexValue((int)b / 16);
				array[num2 + 1] = GetHexValue((int)b % 16);
				array[num2 + 2] = '-';
			}
			return new string(array, 0, array.Length - 1);
		}

		public static string ToString(byte[] value)
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			return ToString(value, 0, value.Length);
		}

		public static string ToString(byte[] value, int startIndex)
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			return ToString(value, startIndex, value.Length - startIndex);
		}

		public static bool ToBoolean(byte[] value, int startIndex)
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			if (startIndex < 0)
			{
				throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
			}
			if (startIndex > value.Length - 1)
			{
				throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("ArgumentOutOfRange_Index"));
			}
			if (value[startIndex] != 0)
			{
				return true;
			}
			return false;
		}

		public unsafe static long DoubleToInt64Bits(double value)
		{
			return *(long*)(&value);
		}

		public unsafe static double Int64BitsToDouble(long value)
		{
			return *(double*)(&value);
		}
	}
}
