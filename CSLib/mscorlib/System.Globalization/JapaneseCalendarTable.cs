using System.IO;
using System.Security;
using System.Security.Permissions;
using Microsoft.Win32;

namespace System.Globalization
{
	internal class JapaneseCalendarTable
	{
		private class ExtendedEraInfo
		{
			public EraInfo EraInfo;

			public string EraName;

			public string AbbrevEraName;

			public string EnglishEraName;

			public int Era
			{
				get
				{
					return EraInfo.era;
				}
				set
				{
					EraInfo.era = value;
				}
			}

			public long Ticks => EraInfo.ticks;

			public int YearOffset => EraInfo.yearOffset;

			public int MinEraYear => EraInfo.minEraYear;

			public int MaxEraYear
			{
				get
				{
					return EraInfo.maxEraYear;
				}
				set
				{
					EraInfo.maxEraYear = value;
				}
			}

			internal ExtendedEraInfo(int era, long ticks, int yearOffset, int minEraYear, int maxEraYear, string eraName, string abbrevEraName, string englishEraName)
			{
				EraInfo = new EraInfo(era, ticks, yearOffset, minEraYear, maxEraYear);
				EraName = eraName;
				AbbrevEraName = abbrevEraName;
				EnglishEraName = englishEraName;
			}
		}

		private const string c_japaneseErasHive = "System\\CurrentControlSet\\Control\\Nls\\Calendars\\Japanese\\Eras";

		private const string c_japaneseErasHivePermissionList = "HKEY_LOCAL_MACHINE\\System\\CurrentControlSet\\Control\\Nls\\Calendars\\Japanese\\Eras";

		private static ExtendedEraInfo[] s_allEras;

		private static JapaneseCalendarTable s_japanese;

		private static JapaneseCalendarTable s_japaneseLunisolar;

		private ExtendedEraInfo[] _eraInfo;

		private string[] _eraNames;

		private string[] _abbrevEraNames;

		private string[] _englishEraNames;

		private int[][] _eraRanges;

		private JapaneseCalendarTable(ExtendedEraInfo[] eraInfo)
		{
			_eraInfo = eraInfo;
		}

		private static JapaneseCalendarTable GetJapaneseCalendarTableInstance()
		{
			if (s_japanese == null)
			{
				s_japanese = new JapaneseCalendarTable(GetAllEras());
			}
			return s_japanese;
		}

		private static JapaneseCalendarTable GetJapaneseLunisolarCalendarTableInstance()
		{
			if (s_japaneseLunisolar == null)
			{
				s_japaneseLunisolar = new JapaneseCalendarTable(TrimErasForLunisolar(GetAllEras()));
			}
			return s_japaneseLunisolar;
		}

		private static JapaneseCalendarTable GetInstance(int calendarId)
		{
			return calendarId switch
			{
				3 => GetJapaneseCalendarTableInstance(), 
				14 => GetJapaneseLunisolarCalendarTableInstance(), 
				_ => null, 
			};
		}

		internal static bool IsJapaneseCalendar(int calendarId)
		{
			if (calendarId != 3)
			{
				return calendarId == 14;
			}
			return true;
		}

		private static ExtendedEraInfo[] GetAllEras()
		{
			if (s_allEras == null)
			{
				s_allEras = GetErasFromRegistry();
				if (s_allEras == null)
				{
					s_allEras = new ExtendedEraInfo[4]
					{
						new ExtendedEraInfo(4, new DateTime(1989, 1, 8).Ticks, 1988, 1, 8011, "平成", "平", "H"),
						new ExtendedEraInfo(3, new DateTime(1926, 12, 25).Ticks, 1925, 1, 64, "昭和", "昭", "S"),
						new ExtendedEraInfo(2, new DateTime(1912, 7, 30).Ticks, 1911, 1, 15, "大正", "大", "T"),
						new ExtendedEraInfo(1, new DateTime(1868, 1, 1).Ticks, 1867, 1, 45, "明治", "明", "M")
					};
				}
			}
			return s_allEras;
		}

		[SecuritySafeCritical]
		private static ExtendedEraInfo[] GetErasFromRegistry()
		{
			int num = 0;
			ExtendedEraInfo[] array = null;
			try
			{
				PermissionSet permissionSet = new PermissionSet(PermissionState.None);
				permissionSet.AddPermission(new RegistryPermission(RegistryPermissionAccess.Read, "HKEY_LOCAL_MACHINE\\System\\CurrentControlSet\\Control\\Nls\\Calendars\\Japanese\\Eras"));
				permissionSet.Assert();
				RegistryKey registryKey = RegistryKey.GetBaseKey(RegistryKey.HKEY_LOCAL_MACHINE).OpenSubKey("System\\CurrentControlSet\\Control\\Nls\\Calendars\\Japanese\\Eras", writable: false);
				if (registryKey == null)
				{
					return null;
				}
				string[] valueNames = registryKey.GetValueNames();
				if (valueNames != null && valueNames.Length > 0)
				{
					array = new ExtendedEraInfo[valueNames.Length];
					for (int i = 0; i < valueNames.Length; i++)
					{
						ExtendedEraInfo eraFromValue = GetEraFromValue(valueNames[i], registryKey.GetValue(valueNames[i]).ToString());
						if (eraFromValue != null)
						{
							array[num] = eraFromValue;
							num++;
						}
					}
				}
			}
			catch (SecurityException)
			{
				return null;
			}
			catch (IOException)
			{
				return null;
			}
			catch (UnauthorizedAccessException)
			{
				return null;
			}
			if (num < 4)
			{
				return null;
			}
			Array.Resize(ref array, num);
			Array.Sort(array, CompareEraRanges);
			for (int j = 0; j < array.Length; j++)
			{
				array[j].Era = array.Length - j;
				if (j == 0)
				{
					array[0].MaxEraYear = 9999 - array[0].YearOffset;
				}
				else
				{
					array[j].MaxEraYear = array[j - 1].YearOffset + 1 - array[j].YearOffset;
				}
			}
			return array;
		}

		private static int CompareEraRanges(ExtendedEraInfo a, ExtendedEraInfo b)
		{
			return b.Ticks.CompareTo(a.Ticks);
		}

		private static ExtendedEraInfo GetEraFromValue(string value, string data)
		{
			if (value == null || data == null)
			{
				return null;
			}
			if (value.Length != 10)
			{
				return null;
			}
			if (!Number.TryParseInt32(value.Substring(0, 4), NumberStyles.None, NumberFormatInfo.InvariantInfo, out var result) || !Number.TryParseInt32(value.Substring(5, 2), NumberStyles.None, NumberFormatInfo.InvariantInfo, out var result2) || !Number.TryParseInt32(value.Substring(8, 2), NumberStyles.None, NumberFormatInfo.InvariantInfo, out var result3))
			{
				return null;
			}
			string[] array = data.Split('_');
			if (array.Length != 4)
			{
				return null;
			}
			if (array[0].Length == 0 || array[1].Length == 0 || array[2].Length == 0 || array[3].Length == 0)
			{
				return null;
			}
			return new ExtendedEraInfo(0, new DateTime(result, result2, result3).Ticks, result - 1, 1, 0, array[0], array[1], array[3]);
		}

		internal static int CurrentEra(int calendarId)
		{
			return GetAllEras().Length;
		}

		internal static string[] EraNames(int calendarId)
		{
			return GetInstance(calendarId).EraNames();
		}

		private string[] EraNames()
		{
			if (_eraNames == null)
			{
				_eraNames = EraNames(_eraInfo);
			}
			return _eraNames;
		}

		private static string[] EraNames(ExtendedEraInfo[] eras)
		{
			string[] array = new string[eras.Length];
			for (int i = 0; i < eras.Length; i++)
			{
				array[i] = eras[eras.Length - i - 1].EraName;
			}
			return array;
		}

		internal static string[] AbbrevEraNames(int calendarId)
		{
			return GetInstance(calendarId).AbbrevEraNames();
		}

		private string[] AbbrevEraNames()
		{
			if (_abbrevEraNames == null)
			{
				_abbrevEraNames = AbbrevEraNames(_eraInfo);
			}
			return _abbrevEraNames;
		}

		private static string[] AbbrevEraNames(ExtendedEraInfo[] eras)
		{
			string[] array = new string[eras.Length];
			for (int i = 0; i < eras.Length; i++)
			{
				array[i] = eras[eras.Length - i - 1].AbbrevEraName;
			}
			return array;
		}

		internal static string[] EnglishEraNames(int calendarId)
		{
			return GetInstance(calendarId).EnglishEraNames();
		}

		private string[] EnglishEraNames()
		{
			if (_englishEraNames == null)
			{
				_englishEraNames = EnglishEraNames(_eraInfo);
			}
			return _englishEraNames;
		}

		private static string[] EnglishEraNames(ExtendedEraInfo[] eras)
		{
			string[] array = new string[eras.Length];
			for (int i = 0; i < eras.Length; i++)
			{
				array[i] = eras[eras.Length - i - 1].EnglishEraName;
			}
			return array;
		}

		internal static int[][] EraRanges(int calendarId)
		{
			return GetInstance(calendarId).EraRanges();
		}

		private int[][] EraRanges()
		{
			if (_eraRanges == null)
			{
				_eraRanges = EraRanges(_eraInfo);
			}
			return _eraRanges;
		}

		private static int[][] EraRanges(ExtendedEraInfo[] eras)
		{
			int[][] array = new int[eras.Length][];
			for (int i = 0; i < eras.Length; i++)
			{
				ExtendedEraInfo extendedEraInfo = eras[i];
				int[] array2 = (array[i] = new int[6]);
				array2[0] = extendedEraInfo.Era;
				DateTime dateTime = new DateTime(extendedEraInfo.Ticks);
				array2[1] = dateTime.Year;
				array2[2] = dateTime.Month;
				array2[3] = dateTime.Day;
				array2[4] = extendedEraInfo.YearOffset;
				array2[5] = extendedEraInfo.MinEraYear;
			}
			return array;
		}

		private static ExtendedEraInfo[] TrimErasForLunisolar(ExtendedEraInfo[] baseEras)
		{
			ExtendedEraInfo[] array = new ExtendedEraInfo[baseEras.Length];
			int num = 0;
			for (int i = 0; i < baseEras.Length; i++)
			{
				if (baseEras[i].YearOffset + baseEras[i].MinEraYear < 2049)
				{
					if (baseEras[i].YearOffset + baseEras[i].MaxEraYear < 1960)
					{
						break;
					}
					array[num] = baseEras[i];
					num++;
				}
			}
			if (num == 0)
			{
				return baseEras;
			}
			Array.Resize(ref array, num);
			return array;
		}
	}
}
