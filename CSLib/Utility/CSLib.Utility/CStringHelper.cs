using System;
using System.Collections.Generic;
using System.Text;

namespace CSLib.Utility
{
	public static class CStringHelper
	{
		private static readonly char ᜀ;

		private static readonly char ᜁ;

		private static readonly char ᜂ;

		private static readonly char ᜃ;

		private static readonly char[] ᜄ;

		public static byte ToByte(this string strValue, byte defaultValue = 0)
		{
			//Discarded unreachable code: IL_001f
			int num = 2;
			while (true)
			{
				switch (num)
				{
				case 0:
					try
					{
						return byte.Parse(strValue);
					}
					catch
					{
						return defaultValue;
					}
				case 1:
					return defaultValue;
				}
				if (true)
				{
				}
				num = ((strValue == null) ? 1 : 0);
			}
		}

		public static short ToInt16(this string strValue, short defaultValue = 0)
		{
			//Discarded unreachable code: IL_001f
			int num = 2;
			while (true)
			{
				switch (num)
				{
				case 1:
					try
					{
						return short.Parse(strValue);
					}
					catch
					{
						return defaultValue;
					}
				case 0:
					return defaultValue;
				}
				if (true)
				{
				}
				num = ((strValue != null) ? 1 : 0);
			}
		}

		public static ushort ToUInt16(this string strValue, ushort defaultValue = 0)
		{
			//Discarded unreachable code: IL_0043
			int num = 2;
			while (true)
			{
				switch (num)
				{
				default:
					num = ((strValue == null) ? 1 : 0);
					break;
				case 0:
					if (true)
					{
					}
					try
					{
						return ushort.Parse(strValue);
					}
					catch
					{
						return defaultValue;
					}
				case 1:
					return defaultValue;
				}
			}
		}

		public static int ToInt32(this string strValue, int defaultValue = 0)
		{
			//Discarded unreachable code: IL_0043
			int num = 1;
			while (true)
			{
				switch (num)
				{
				default:
					num = ((strValue == null) ? 2 : 0);
					break;
				case 0:
					if (true)
					{
					}
					try
					{
						return int.Parse(strValue);
					}
					catch
					{
						return defaultValue;
					}
				case 2:
					return defaultValue;
				}
			}
		}

		public static uint ToUInt32(this string strValue, uint defaultValue = 0u)
		{
			//Discarded unreachable code: IL_003b
			int num = 1;
			while (true)
			{
				switch (num)
				{
				case 2:
					try
					{
						return uint.Parse(strValue);
					}
					catch
					{
						return defaultValue;
					}
				case 0:
					return defaultValue;
				}
				if (strValue == null)
				{
					num = 0;
					continue;
				}
				if (true)
				{
				}
				num = 2;
			}
		}

		public static long ToInt64(this string strValue, long defaultValue = 0L)
		{
			//Discarded unreachable code: IL_001f
			int num = 2;
			while (true)
			{
				switch (num)
				{
				case 1:
					try
					{
						return long.Parse(strValue);
					}
					catch
					{
						return defaultValue;
					}
				case 0:
					return defaultValue;
				}
				if (true)
				{
				}
				num = ((strValue != null) ? 1 : 0);
			}
		}

		public static ulong ToUInt64(this string strValue, ulong defaultValue = 0uL)
		{
			//Discarded unreachable code: IL_001f
			int num = 1;
			while (true)
			{
				switch (num)
				{
				case 2:
					try
					{
						return ulong.Parse(strValue);
					}
					catch
					{
						return defaultValue;
					}
				case 0:
					return defaultValue;
				}
				if (true)
				{
				}
				num = ((strValue != null) ? 2 : 0);
			}
		}

		public static float ToSingle(this string strValue, float defaultValue = 0f)
		{
			//Discarded unreachable code: IL_003b
			int num = 0;
			while (true)
			{
				switch (num)
				{
				case 2:
					try
					{
						return float.Parse(strValue);
					}
					catch
					{
						return defaultValue;
					}
				case 1:
					return defaultValue;
				}
				if (strValue == null)
				{
					num = 1;
					continue;
				}
				if (true)
				{
				}
				num = 2;
			}
		}

		public static double ToDouble(this string strValue, double defaultValue = 0.0)
		{
			//Discarded unreachable code: IL_0043
			int num = 1;
			while (true)
			{
				switch (num)
				{
				default:
					num = ((strValue != null) ? 2 : 0);
					break;
				case 2:
					if (true)
					{
					}
					try
					{
						return double.Parse(strValue);
					}
					catch
					{
						return defaultValue;
					}
				case 0:
					return defaultValue;
				}
			}
		}

		public static decimal ToDecimal(this string strValue, decimal defaultValue = 0m)
		{
			//Discarded unreachable code: IL_0003
			if (true)
			{
			}
			int num = 0;
			while (true)
			{
				switch (num)
				{
				default:
					num = ((strValue == null) ? 1 : 2);
					break;
				case 2:
					try
					{
						return decimal.Parse(strValue);
					}
					catch
					{
						return defaultValue;
					}
				case 1:
					return defaultValue;
				}
			}
		}

		public static T ToValue<T>(this string strValue, T defaultValue = default(T))
		{
			return Convert2Value(strValue, defaultValue);
		}

		public static T ToEnum<T>(this string strValue, T defaultValue = default(T))
		{
			return Convert2Enum(strValue, defaultValue);
		}

		public static bool ContainUnicode(string text)
		{
			//Discarded unreachable code: IL_0047
			while (true)
			{
				byte[] bytes = Encoding.Unicode.GetBytes(text);
				char[] chars = Encoding.Unicode.GetChars(bytes);
				int num = 0;
				int num2 = 3;
				while (true)
				{
					switch (num2)
					{
					case 5:
						return true;
					case 0:
						if (chars[num] <= 'ÿ')
						{
							if (true)
							{
							}
							num++;
							num2 = 2;
						}
						else
						{
							num2 = 5;
						}
						continue;
					case 2:
					case 3:
						num2 = 1;
						continue;
					case 1:
						num2 = ((num >= chars.Length) ? 4 : 0);
						continue;
					case 4:
						return false;
					}
					break;
				}
			}
		}

		public static bool GetStringFromBrackets(string strValue, out string strResult)
		{
			//Discarded unreachable code: IL_001a
			strResult = string.Empty;
			string[] array = strValue.Split(ᜄ);
			if (array.Length == 0)
			{
				if (true)
				{
				}
				return false;
			}
			strResult = array[0];
			return true;
		}

		public static T Convert2Value<T>(string strValue, T defaultValue = default(T))
		{
			//Discarded unreachable code: IL_001f
			int num = 2;
			while (true)
			{
				switch (num)
				{
				case 0:
					try
					{
						return (T)Convert.ChangeType(strValue, typeof(T));
					}
					catch
					{
						return defaultValue;
					}
				case 1:
					return defaultValue;
				}
				if (true)
				{
				}
				num = ((strValue == null) ? 1 : 0);
			}
		}

		public static T Convert2Enum<T>(string strValue, T defaultValue = default(T))
		{
			//Discarded unreachable code: IL_002a
			int num = 1;
			while (true)
			{
				switch (num)
				{
				default:
					num = ((strValue != null) ? 2 : 0);
					break;
				case 0:
					if (true)
					{
					}
					return defaultValue;
				case 2:
					try
					{
						return (T)Enum.Parse(typeof(T), strValue, ignoreCase: true);
					}
					catch
					{
						return defaultValue;
					}
				}
			}
		}

		public static bool Split(string strValue, char cSeparator, out string[] arrResult)
		{
			//Discarded unreachable code: IL_0003
			if (true)
			{
			}
			arrResult = null;
			if (string.IsNullOrEmpty(strValue))
			{
				return false;
			}
			arrResult = strValue.Split(cSeparator);
			return arrResult.Length != 0;
		}

		public static bool Split(string strValue, char cSeparator, out List<string> listResult)
		{
			//Discarded unreachable code: IL_0083
			string[] array = default(string[]);
			int num2 = default(int);
			while (true)
			{
				listResult = new List<string>();
				int num = 1;
				while (true)
				{
					switch (num)
					{
					case 1:
						if (string.IsNullOrEmpty(strValue))
						{
							num = 4;
							continue;
						}
						array = strValue.Split(cSeparator);
						num2 = 0;
						num = 2;
						continue;
					case 4:
						return false;
					case 2:
					case 3:
						num = 5;
						continue;
					case 5:
						if (true)
						{
						}
						if (num2 < array.Length)
						{
							listResult.Add(array[num2]);
							num2++;
							num = 3;
						}
						else
						{
							num = 0;
						}
						continue;
					case 0:
						return listResult.Count > 0;
					}
					break;
				}
			}
		}

		public static bool Split<T>(string strValue, char cSeparator, out T[] arrResult)
		{
			//Discarded unreachable code: IL_006c
			switch (0)
			{
			}
			string[] array = default(string[]);
			int num2 = default(int);
			T val = default(T);
			while (true)
			{
				arrResult = null;
				int num = 1;
				while (true)
				{
					T val2;
					T val3;
					switch (num)
					{
					case 1:
						if (string.IsNullOrEmpty(strValue))
						{
							num = 9;
							continue;
						}
						array = strValue.Split(cSeparator);
						arrResult = new T[array.Length];
						num2 = 0;
						num = 3;
						continue;
					case 2:
						val2 = val;
						goto IL_00e6;
					case 4:
						num = 6;
						continue;
					case 6:
					{
						string strValue2 = array[num2];
						val = default(T);
						val2 = Convert2Value(strValue2, val);
						goto IL_00e6;
					}
					case 0:
					case 3:
						num = 8;
						continue;
					case 8:
						num = ((num2 < array.Length) ? 7 : 5);
						continue;
					case 9:
						return false;
					case 7:
						if (string.IsNullOrEmpty(array[num2]))
						{
							val = default(T);
							if (true)
							{
							}
							num = 2;
						}
						else
						{
							num = 4;
						}
						continue;
					case 5:
						{
							return arrResult.Length != 0;
						}
						IL_00e6:
						val3 = val2;
						arrResult[num2] = val3;
						num2++;
						num = 0;
						continue;
					}
					break;
				}
			}
		}

		public static bool Split<T>(string strValue, char cSeparator, out List<T> listResult)
		{
			//Discarded unreachable code: IL_0120
			switch (0)
			{
			}
			string[] array = default(string[]);
			int num2 = default(int);
			T val = default(T);
			while (true)
			{
				listResult = null;
				int num = 5;
				while (true)
				{
					T val2;
					T item;
					switch (num)
					{
					case 5:
						if (string.IsNullOrEmpty(strValue))
						{
							num = 8;
							continue;
						}
						listResult = new List<T>();
						array = strValue.Split(cSeparator);
						num2 = 0;
						num = 3;
						continue;
					case 2:
						val2 = val;
						goto IL_00d8;
					case 9:
						num = 4;
						continue;
					case 4:
					{
						string strValue2 = array[num2];
						val = default(T);
						val2 = Convert2Value(strValue2, val);
						goto IL_00d8;
					}
					case 1:
					case 3:
						num = 6;
						continue;
					case 6:
						num = ((num2 >= array.Length) ? 7 : 0);
						continue;
					case 8:
						return false;
					case 0:
						if (string.IsNullOrEmpty(array[num2]))
						{
							val = default(T);
							num = 2;
						}
						else
						{
							num = 9;
						}
						continue;
					case 7:
						{
							if (true)
							{
							}
							return listResult.Count > 0;
						}
						IL_00d8:
						item = val2;
						listResult.Add(item);
						num2++;
						num = 1;
						continue;
					}
					break;
				}
			}
		}

		public static bool Split<T>(string strValue, char cSeparator1, char cSeparator2, out Dictionary<int, T[]> dictResult)
		{
			//Discarded unreachable code: IL_0038
			int num2 = default(int);
			T[] arrResult2 = default(T[]);
			while (true)
			{
				string[] arrResult = null;
				dictResult = new Dictionary<int, T[]>();
				if (true)
				{
				}
				int num = 5;
				while (true)
				{
					switch (num)
					{
					case 5:
						if (!Split(strValue, cSeparator1, out arrResult))
						{
							num = 8;
							continue;
						}
						num2 = 0;
						num = 7;
						continue;
					case 3:
						num2++;
						num = 4;
						continue;
					case 0:
						if (Split(arrResult[num2], cSeparator2, out arrResult2))
						{
							num = 2;
							continue;
						}
						goto case 3;
					case 8:
						return false;
					case 4:
					case 7:
						num = 1;
						continue;
					case 1:
						if (num2 < arrResult.Length)
						{
							arrResult2 = null;
							num = 0;
						}
						else
						{
							num = 6;
						}
						continue;
					case 2:
						dictResult.Add(num2, arrResult2);
						num = 3;
						continue;
					case 6:
						return true;
					}
					break;
				}
			}
		}

		public static bool Split<T>(string strValue, char cSeparator1, char cSeparator2, out Dictionary<int, List<T>> dictResult)
		{
			//Discarded unreachable code: IL_0086
			int num2 = default(int);
			List<T> listResult = default(List<T>);
			while (true)
			{
				string[] arrResult = null;
				dictResult = new Dictionary<int, List<T>>();
				int num = 4;
				while (true)
				{
					switch (num)
					{
					case 4:
						if (!Split(strValue, cSeparator1, out arrResult))
						{
							num = 6;
							continue;
						}
						num2 = 0;
						num = 0;
						continue;
					case 8:
						num2++;
						num = 2;
						continue;
					case 3:
						if (Split(arrResult[num2], cSeparator2, out listResult))
						{
							num = 5;
							continue;
						}
						goto case 8;
					case 6:
						return false;
					case 0:
					case 2:
						if (true)
						{
						}
						num = 1;
						continue;
					case 1:
						if (num2 < arrResult.Length)
						{
							listResult = null;
							num = 3;
						}
						else
						{
							num = 7;
						}
						continue;
					case 5:
						dictResult.Add(num2, listResult);
						num = 8;
						continue;
					case 7:
						return true;
					}
					break;
				}
			}
		}

		public static bool SplitByComma(string strValue, out string[] arrResult)
		{
			return Split(strValue, ᜀ, out arrResult);
		}

		public static bool SplitByComma(string strValue, out List<string> listResult)
		{
			return Split(strValue, ᜀ, out listResult);
		}

		public static bool SplitByComma<T>(string strValue, out T[] arrResult)
		{
			return Split(strValue, ᜀ, out arrResult);
		}

		public static bool SplitByComma<T>(string strValue, out List<T> listResult)
		{
			return Split(strValue, ᜀ, out listResult);
		}

		public static bool SplitByVerticalLine(string strValue, out string[] arrResult)
		{
			return Split(strValue, ᜁ, out arrResult);
		}

		public static bool SplitByVerticalLine(string strValue, out List<string> listResult)
		{
			return Split(strValue, ᜁ, out listResult);
		}

		public static bool SplitByVerticalLine<T>(string strValue, out T[] arrResult)
		{
			return Split(strValue, ᜁ, out arrResult);
		}

		public static bool SplitByVerticalLine<T>(string strValue, out List<T> listResult)
		{
			return Split(strValue, ᜁ, out listResult);
		}

		public static bool SplitByColon(string strValue, out string[] arrResult)
		{
			return Split(strValue, ᜂ, out arrResult);
		}

		public static bool SplitByColon(string strValue, out List<string> listResult)
		{
			return Split(strValue, ᜂ, out listResult);
		}

		public static bool SplitByColon<T>(string strValue, out T[] arrResult)
		{
			return Split(strValue, ᜂ, out arrResult);
		}

		public static bool SplitByColon<T>(string strValue, out List<T> listResult)
		{
			return Split(strValue, ᜂ, out listResult);
		}

		public static bool SplitBySemicolon(string strValue, out string[] arrResult)
		{
			return Split(strValue, ᜃ, out arrResult);
		}

		public static bool SplitBySemicolon(string strValue, out List<string> listResult)
		{
			return Split(strValue, ᜃ, out listResult);
		}

		public static bool SplitBySemicolon<T>(string strValue, out T[] arrResult)
		{
			return Split(strValue, ᜃ, out arrResult);
		}

		public static bool SplitBySemicolon<T>(string strValue, out List<T> listResult)
		{
			return Split(strValue, ᜃ, out listResult);
		}

		static CStringHelper()
		{
			//Discarded unreachable code: IL_0003
			if (true)
			{
			}
			ᜀ = ',';
			ᜁ = '|';
			ᜂ = ':';
			ᜃ = ';';
			ᜄ = new char[2]
			{
				'(',
				')'
			};
		}
	}
}
