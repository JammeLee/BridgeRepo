namespace System
{
	internal class IPv4AddressHelper
	{
		private const int NumberOfLabels = 4;

		private IPv4AddressHelper()
		{
		}

		internal unsafe static string ParseCanonicalName(string str, int start, int end, ref bool isLoopback)
		{
			byte* ptr = stackalloc byte[1 * 4];
			isLoopback = Parse(str, ptr, start, end);
			return *ptr + "." + ptr[1] + "." + ptr[2] + "." + ptr[3];
		}

		internal unsafe static int ParseHostNumber(string str, int start, int end)
		{
			byte* ptr = stackalloc byte[1 * 4];
			Parse(str, ptr, start, end);
			return (*ptr << 24) + (ptr[1] << 16) + (ptr[2] << 8) + ptr[3];
		}

		internal unsafe static bool IsValid(char* name, int start, ref int end, bool allowIPv6, bool notImplicitFile)
		{
			int num = 0;
			int num2 = 0;
			bool flag = false;
			while (start < end)
			{
				char c = name[start];
				if (allowIPv6)
				{
					if (c == ']' || c == '/' || c == '%')
					{
						break;
					}
				}
				else if (c == '/' || c == '\\' || (notImplicitFile && (c == ':' || c == '?' || c == '#')))
				{
					break;
				}
				if (c <= '9' && c >= '0')
				{
					flag = true;
					num2 = num2 * 10 + (name[start] - 48);
					if (num2 > 255)
					{
						return false;
					}
				}
				else
				{
					if (c != '.')
					{
						return false;
					}
					if (!flag)
					{
						return false;
					}
					num++;
					flag = false;
					num2 = 0;
				}
				start++;
			}
			bool flag2 = num == 3 && flag;
			if (flag2)
			{
				end = start;
			}
			return flag2;
		}

		private unsafe static bool Parse(string name, byte* numbers, int start, int end)
		{
			for (int i = 0; i < 4; i++)
			{
				byte b = 0;
				char c;
				while (start < end && (c = name[start]) != '.' && c != ':')
				{
					b = (byte)(b * 10 + (byte)(c - 48));
					start++;
				}
				numbers[i] = b;
				start++;
			}
			return *numbers == 127;
		}
	}
}
