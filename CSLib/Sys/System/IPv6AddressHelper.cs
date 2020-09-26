using System.Diagnostics;
using System.Net;

namespace System
{
	internal class IPv6AddressHelper
	{
		private const int NumberOfLabels = 8;

		private const string CanonicalNumberFormat = "{0:X4}";

		private IPv6AddressHelper()
		{
		}

		internal unsafe static string ParseCanonicalName(string str, int start, ref bool isLoopback, ref string scopeId)
		{
			ushort* ptr = (ushort*)stackalloc byte[2 * 9];
			*(long*)ptr = 0L;
			*(long*)(ptr + 4) = 0L;
			isLoopback = Parse(str, ptr, start, ref scopeId);
			return CreateCanonicalName(ptr);
		}

		private unsafe static string CreateCanonicalName(ushort* numbers)
		{
			return '[' + $"{*numbers:X4}" + ':' + $"{numbers[1]:X4}" + ':' + $"{numbers[2]:X4}" + ':' + $"{numbers[3]:X4}" + ':' + $"{numbers[4]:X4}" + ':' + $"{numbers[5]:X4}" + ':' + $"{numbers[6]:X4}" + ':' + $"{numbers[7]:X4}" + ']';
		}

		internal unsafe static bool IsValid(char* name, int start, ref int end)
		{
			int num = 0;
			int num2 = 0;
			bool flag = false;
			bool flag2 = false;
			bool flag3 = false;
			bool flag4 = true;
			int start2 = 1;
			if (name[start] == ':' && (start + 1 >= end || name[start + 1] != ':') && ServicePointManager.UseStrictIPv6AddressParsing)
			{
				return false;
			}
			int i;
			for (i = start; i < end; i++)
			{
				if ((!flag3) ? Uri.IsHexDigit(name[i]) : (name[i] >= '0' && name[i] <= '9'))
				{
					num2++;
					flag4 = false;
					continue;
				}
				if (num2 > 4)
				{
					return false;
				}
				if (num2 != 0)
				{
					num++;
					start2 = i - num2;
				}
				switch (name[i])
				{
				case '%':
					while (true)
					{
						if (++i == end)
						{
							return false;
						}
						if (name[i] == ']')
						{
							break;
						}
						if (name[i] != '/')
						{
							continue;
						}
						goto case '/';
					}
					goto case ']';
				case ']':
					start = i;
					i = end;
					continue;
				case ':':
					if (i > 0 && name[i - 1] == ':')
					{
						if (flag)
						{
							return false;
						}
						flag = true;
						flag4 = false;
					}
					else
					{
						flag4 = true;
					}
					break;
				case '/':
					if (num == 0 || flag3)
					{
						return false;
					}
					flag3 = true;
					flag4 = true;
					break;
				case '.':
					if (flag2)
					{
						return false;
					}
					i = end;
					if (!IPv4AddressHelper.IsValid(name, start2, ref i, allowIPv6: true, notImplicitFile: false))
					{
						return false;
					}
					num++;
					flag2 = true;
					i--;
					break;
				default:
					return false;
				}
				num2 = 0;
			}
			if (flag3 && (num2 < 1 || num2 > 2))
			{
				return false;
			}
			int num3 = 8 + (flag3 ? 1 : 0);
			if (!flag4 && num2 <= 4 && (flag ? (num < num3) : (num == num3)))
			{
				if (i == end + 1)
				{
					end = start + 1;
					return true;
				}
				return false;
			}
			return false;
		}

		internal unsafe static bool Parse(string address, ushort* numbers, int start, ref string scopeId)
		{
			int num = 0;
			int num2 = 0;
			int num3 = -1;
			bool flag = true;
			int num4 = 0;
			if (address[start] == '[')
			{
				start++;
			}
			int i = start;
			while (i < address.Length && address[i] != ']')
			{
				switch (address[i])
				{
				case '%':
					if (flag)
					{
						numbers[num2++] = (ushort)num;
						flag = false;
					}
					start = i;
					for (i++; address[i] != ']' && address[i] != '/'; i++)
					{
					}
					scopeId = address.Substring(start, i - start);
					for (; address[i] != ']'; i++)
					{
					}
					break;
				case ':':
				{
					numbers[num2++] = (ushort)num;
					num = 0;
					i++;
					if (address[i] == ':')
					{
						num3 = num2;
						i++;
					}
					else if (num3 < 0 && num2 < 6)
					{
						break;
					}
					for (int j = i; address[j] != ']' && address[j] != ':' && address[j] != '%' && address[j] != '/' && j < i + 4; j++)
					{
						if (address[j] == '.')
						{
							for (; address[j] != ']' && address[j] != '/' && address[j] != '%'; j++)
							{
							}
							num = IPv4AddressHelper.ParseHostNumber(address, i, j);
							numbers[num2++] = (ushort)(num >> 16);
							numbers[num2++] = (ushort)num;
							i = j;
							num = 0;
							flag = false;
							break;
						}
					}
					break;
				}
				case '/':
					if (flag)
					{
						numbers[num2++] = (ushort)num;
						flag = false;
					}
					for (i++; address[i] != ']'; i++)
					{
						num4 = num4 * 10 + (address[i] - 48);
					}
					break;
				default:
					num = num * 16 + Uri.FromHex(address[i++]);
					break;
				}
			}
			if (flag)
			{
				numbers[num2++] = (ushort)num;
			}
			if (num3 > 0)
			{
				int num5 = 7;
				int num6 = num2 - 1;
				for (int num7 = num2 - num3; num7 > 0; num7--)
				{
					numbers[num5--] = numbers[num6];
					numbers[num6--] = 0;
				}
			}
			if (*numbers == 0 && numbers[1] == 0 && numbers[2] == 0 && numbers[3] == 0 && numbers[4] == 0)
			{
				if (numbers[5] != 0 || numbers[6] != 0 || numbers[7] != 1)
				{
					if (numbers[6] == 32512 && numbers[7] == 1)
					{
						if (numbers[5] != 0)
						{
							return numbers[5] == ushort.MaxValue;
						}
						return true;
					}
					return false;
				}
				return true;
			}
			return false;
		}

		[Conditional("DEBUG")]
		private static void ValidateIndex(int index)
		{
			_ = ServicePointManager.UseStrictIPv6AddressParsing;
		}
	}
}
