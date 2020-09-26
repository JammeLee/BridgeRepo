using System;
using CSLib.Utility;

internal class ᜇ : CSingleton<ᜇ>
{
	public static int ᜀ<ᜀ>(ᜀ A_0, ᜀ A_1)
	{
		//Discarded unreachable code: IL_003e
		try
		{
			long num = Convert.ToInt64(A_1);
			long num2 = Convert.ToInt64(A_0);
			return (int)(num / num2) + ((num % num2 != 0L) ? 1 : 0);
		}
		catch (Exception ex)
		{
			CSingleton<CLogInfoList>.Instance.WriteLine(ex);
		}
		if (true)
		{
		}
		return 1;
	}

	public static string ᜂ<ᜀ>(ᜀ A_0)
	{
		//Discarded unreachable code: IL_0044
		string result;
		try
		{
			double num = Convert.ToDouble(A_0);
			num /= CConstant.NUM_ONE_MB;
			result = string.Format(CConstant.STRING_FILESIZE_FORMAT_1, num, A_0);
		}
		catch (Exception ex)
		{
			CSingleton<CLogInfoList>.Instance.WriteLine(ex);
			goto IL_003b;
		}
		if (true)
		{
		}
		return result;
		IL_003b:
		return CConstant.STRING_UNKNOW;
	}

	public static string ᜁ<ᜀ>(ᜀ A_0)
	{
		//Discarded unreachable code: IL_003e
		string result;
		try
		{
			double num = Convert.ToDouble(A_0);
			num /= CConstant.NUM_ONE_MB;
			result = string.Format(CConstant.STRING_FILESIZE_FORMAT_2, num);
		}
		catch (Exception ex)
		{
			CSingleton<CLogInfoList>.Instance.WriteLine(ex);
			goto IL_0035;
		}
		if (true)
		{
		}
		return result;
		IL_0035:
		return CConstant.STRING_UNKNOW;
	}

	public static int ᜀ(long A_0, long A_1)
	{
		//Discarded unreachable code: IL_009d
		try
		{
			switch (0)
			{
			}
			int result = default(int);
			while (true)
			{
				double num = Convert.ToDouble(A_0);
				double num2 = Convert.ToDouble(A_1);
				double num3 = 0.0;
				int num4 = 2;
				while (true)
				{
					switch (num4)
					{
					case 2:
						if (num2 != 0.0)
						{
							num4 = 1;
							continue;
						}
						goto case 0;
					case 1:
						num3 = num / num2;
						num4 = 0;
						continue;
					case 0:
						result = Convert.ToInt32(num3 * 100.0);
						num4 = 3;
						continue;
					case 3:
						return result;
					}
					break;
				}
			}
		}
		catch (Exception ex)
		{
			CSingleton<CLogInfoList>.Instance.WriteLine(ex);
		}
		if (true)
		{
		}
		return 0;
	}

	public static string ᜀ<ᜀ>(ᜀ A_0)
	{
		//Discarded unreachable code: IL_0024
		try
		{
			return string.Format(CConstant.STRING_FILEPERCENT_FORMAT, A_0);
		}
		catch (Exception ex)
		{
			CSingleton<CLogInfoList>.Instance.WriteLine(ex);
		}
		if (true)
		{
		}
		return CConstant.STRING_UNKNOW;
	}

	public static string ᜀ<ᜀ>(ᜀ A_0, ᜀ A_1, out double A_2)
	{
		//Discarded unreachable code: IL_003d
		switch (0)
		{
		default:
			A_2 = 0.0;
			try
			{
				string result = default(string);
				while (true)
				{
					if (true)
					{
					}
					double num = Convert.ToDouble(A_0);
					double num2 = Convert.ToDouble(A_1);
					double num3 = 0.0;
					int num4 = 3;
					while (true)
					{
						switch (num4)
						{
						case 3:
							if (num2 != 0.0)
							{
								num4 = 1;
								continue;
							}
							goto case 0;
						case 1:
							num3 = (A_2 = num / num2 * 100.0);
							num4 = 0;
							continue;
						case 0:
							result = string.Format(CConstant.STRING_FILEPERCENT_FORMAT, num3);
							num4 = 2;
							continue;
						case 2:
							return result;
						}
						break;
					}
				}
			}
			catch (Exception ex)
			{
				CSingleton<CLogInfoList>.Instance.WriteLine(ex);
			}
			return CConstant.STRING_UNKNOW;
		}
	}

	public static void ᜀ(ref byte[] A_0, ref byte[] A_1, int A_2, int A_3)
	{
		//Discarded unreachable code: IL_00cf
		int num = 9;
		int num2 = default(int);
		while (true)
		{
			switch (num)
			{
			default:
				if (A_0 != null)
				{
					num = 6;
					break;
				}
				return;
			case 0:
				num = 5;
				break;
			case 5:
				if (num2 < A_1.Length)
				{
					num = 4;
					break;
				}
				return;
			case 6:
				num = 3;
				break;
			case 3:
				if (A_1 != null)
				{
					num = 2;
					break;
				}
				return;
			case 4:
				num = 11;
				break;
			case 11:
				if (num2 < A_2 + A_3)
				{
					A_0[num2] = A_1[num2 - A_2];
					num2++;
					num = 1;
				}
				else
				{
					num = 10;
				}
				break;
			case 10:
				return;
			case 2:
				num2 = A_2;
				num = 8;
				break;
			case 8:
				if (1 == 0)
				{
				}
				goto case 1;
			case 1:
				num = 7;
				break;
			case 7:
				if (num2 < A_0.Length)
				{
					num = 0;
					break;
				}
				return;
			}
		}
	}

	public static string ᜃ(string A_0)
	{
		//Discarded unreachable code: IL_000c
		int a_ = 4;
		if (true)
		{
		}
		string result = A_0;
		try
		{
			TimeSpan timeSpan = TimeSpan.FromSeconds(Convert.ToDouble(A_0));
			result = string.Format(CSimpleThreadPool.b("㬿牁㥃籅㍇等ㅋ瑍⭏恑⥓", a_), timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
			return result;
		}
		catch (Exception ex)
		{
			CSingleton<CLogInfoList>.Instance.WriteLine(ex);
			return result;
		}
	}

	public static DateTime ᜂ(string A_0)
	{
		//Discarded unreachable code: IL_0069
		try
		{
			return new DateTime(Convert.ToInt32(A_0.Substring(0, 4)), Convert.ToInt32(A_0.Substring(4, 2)), Convert.ToInt32(A_0.Substring(6, 2)), Convert.ToInt32(A_0.Substring(8, 2)), Convert.ToInt32(A_0.Substring(10, 2)), Convert.ToInt32(A_0.Substring(12, 2)));
		}
		catch (Exception ex)
		{
			CSingleton<CLogInfoList>.Instance.WriteLine(ex);
		}
		if (true)
		{
		}
		return DateTime.MaxValue;
	}

	public static string ᜁ()
	{
		int a_ = 17;
		return DateTime.Now.ToString(CSimpleThreadPool.b("㑌㙎⡐⩒ᡔ\u1a56㵘㽚ᕜ\u175eౠ\u0e62ᙤᑦ", a_));
	}

	public static int ᜁ(string A_0)
	{
		//Discarded unreachable code: IL_007f
		try
		{
			DateTime now = DateTime.Now;
			return Math.Abs((int)(new DateTime(now.Year, now.Month, now.Day, Convert.ToInt32(A_0.Substring(0, 2)), Convert.ToInt32(A_0.Substring(2, 2)), Convert.ToInt32(A_0.Substring(4, 2))).Subtract(now).TotalSeconds * 1000.0));
		}
		catch (Exception ex)
		{
			CSingleton<CLogInfoList>.Instance.WriteLine(ex);
		}
		if (true)
		{
		}
		return -1;
	}

	public static int ᜀ(string A_0, out DateTime A_1)
	{
		//Discarded unreachable code: IL_0003
		if (true)
		{
		}
		DateTime now = DateTime.Now;
		try
		{
			DateTime dateTime = (A_1 = new DateTime(Convert.ToInt32(A_0.Substring(0, 4)), Convert.ToInt32(A_0.Substring(4, 2)), Convert.ToInt32(A_0.Substring(6, 2)), Convert.ToInt32(A_0.Substring(8, 2)), Convert.ToInt32(A_0.Substring(10, 2)), Convert.ToInt32(A_0.Substring(12, 2))));
			return Math.Abs((int)dateTime.Subtract(now).TotalSeconds);
		}
		catch (Exception ex)
		{
			CSingleton<CLogInfoList>.Instance.WriteLine(ex);
		}
		A_1 = now;
		return 0;
	}

	public static int ᜀ(string A_0)
	{
		//Discarded unreachable code: IL_001e
		int result = 0;
		try
		{
			result = Convert.ToInt32(A_0);
		}
		catch (Exception ex)
		{
			CSingleton<CLogInfoList>.Instance.WriteLine(ex);
		}
		if (true)
		{
		}
		return result;
	}
}
