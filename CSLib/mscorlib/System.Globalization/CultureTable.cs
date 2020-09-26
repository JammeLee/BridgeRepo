using System.Collections;
using System.IO;
using System.Security;
using System.Security.Permissions;
using System.Text;

namespace System.Globalization
{
	internal class CultureTable : BaseInfoTable
	{
		internal const int ILANGUAGE = 0;

		internal const string TraditionalChineseCultureName = "zh-CHT";

		internal const string SimplifiedChineseCultureName = "zh-CHS";

		internal const string NewTraditionalChineseCultureName = "zh-Hant";

		internal const string NewSimplifiedChineseCultureName = "zh-Hans";

		private const CultureTypes CultureTypesMask = ~(CultureTypes.AllCultures | CultureTypes.UserCustomCulture | CultureTypes.ReplacementCultures | CultureTypes.WindowsOnlyCultures | CultureTypes.FrameworkCultures);

		internal const string TypeLoadExceptionMessage = "Failure has occurred while loading a type.";

		private const uint sizeofNameOffsetItem = 8u;

		private Hashtable hashByName;

		private Hashtable hashByRegionName;

		private Hashtable hashByLcid;

		private unsafe CultureNameOffsetItem* m_pCultureNameIndex;

		private unsafe RegionNameOffsetItem* m_pRegionNameIndex;

		private unsafe IDOffsetItem* m_pCultureIDIndex;

		private static CultureTable m_defaultInstance;

		internal static CultureTable Default
		{
			get
			{
				if (m_defaultInstance == null)
				{
					throw new TypeLoadException("Failure has occurred while loading a type.");
				}
				return m_defaultInstance;
			}
		}

		static CultureTable()
		{
			m_defaultInstance = new CultureTable("culture.nlp", fromAssembly: true);
		}

		internal unsafe CultureTable(string fileName, bool fromAssembly)
			: base(fileName, fromAssembly)
		{
			if (base.IsValid)
			{
				hashByName = Hashtable.Synchronized(new Hashtable());
				hashByLcid = Hashtable.Synchronized(new Hashtable());
				hashByRegionName = Hashtable.Synchronized(new Hashtable());
				m_pCultureNameIndex = (CultureNameOffsetItem*)(m_pDataFileStart + (int)m_pCultureHeader->cultureNameTableOffset);
				m_pRegionNameIndex = (RegionNameOffsetItem*)(m_pDataFileStart + (int)m_pCultureHeader->regionNameTableOffset);
				m_pCultureIDIndex = (IDOffsetItem*)(m_pDataFileStart + (int)m_pCultureHeader->cultureIDTableOffset);
			}
		}

		internal unsafe override void SetDataItemPointers()
		{
			if (Validate())
			{
				m_itemSize = m_pCultureHeader->sizeCultureItem;
				m_numItem = m_pCultureHeader->numCultureItems;
				m_pDataPool = (ushort*)(m_pDataFileStart + (int)m_pCultureHeader->offsetToDataPool);
				m_pItemData = m_pDataFileStart + (int)m_pCultureHeader->offsetToCultureItemData;
			}
			else
			{
				m_valid = false;
			}
		}

		private unsafe static string CheckAndGetTheString(ushort* pDataPool, uint offsetInPool, int poolSize)
		{
			if (offsetInPool + 2 > poolSize)
			{
				return null;
			}
			char* ptr = (char*)(pDataPool + offsetInPool);
			int num = *ptr;
			if (offsetInPool + num + 2 > poolSize)
			{
				return null;
			}
			return new string(ptr + 1, 0, num);
		}

		private unsafe static bool ValidateString(ushort* pDataPool, uint offsetInPool, int poolSize)
		{
			if (offsetInPool + 2 > poolSize)
			{
				return false;
			}
			char* ptr = (char*)(pDataPool + offsetInPool);
			int num = *ptr;
			if (offsetInPool + num + 2 > poolSize)
			{
				return false;
			}
			return true;
		}

		private unsafe static bool ValidateUintArray(ushort* pDataPool, uint offsetInPool, int poolSize)
		{
			if (offsetInPool == 0)
			{
				return true;
			}
			if (offsetInPool + 2 > poolSize)
			{
				return false;
			}
			ushort* ptr = pDataPool + offsetInPool;
			if (((int)ptr & 2) != 2)
			{
				return false;
			}
			int num = *ptr;
			if (offsetInPool + num * 2 + 2 > poolSize)
			{
				return false;
			}
			return true;
		}

		private unsafe static bool ValidateStringArray(ushort* pDataPool, uint offsetInPool, int poolSize)
		{
			if (!ValidateUintArray(pDataPool, offsetInPool, poolSize))
			{
				return false;
			}
			ushort* ptr = pDataPool + offsetInPool;
			int num = *ptr;
			if (num == 0)
			{
				return true;
			}
			uint* ptr2 = (uint*)(ptr + 1);
			for (int i = 0; i < num; i++)
			{
				if (!ValidateString(pDataPool, ptr2[i], poolSize))
				{
					return false;
				}
			}
			return true;
		}

		private static bool IsValidLcid(int lcid, bool canBeCustomLcid)
		{
			if (canBeCustomLcid && CultureTableRecord.IsCustomCultureId(lcid))
			{
				return true;
			}
			if (Default.IsExistingCulture(lcid))
			{
				return true;
			}
			CultureTableRecord.InitSyntheticMapping();
			if (CultureTableRecord.SyntheticLcidToNameCache[lcid] != null)
			{
				return true;
			}
			return false;
		}

		internal unsafe bool Validate()
		{
			if (memoryMapFile == null)
			{
				return true;
			}
			long fileSize = memoryMapFile.FileSize;
			if (sizeof(EndianessHeader) + sizeof(CultureTableHeader) + sizeof(CultureTableData) + 8 > fileSize)
			{
				return false;
			}
			EndianessHeader* pDataFileStart = (EndianessHeader*)m_pDataFileStart;
			if (pDataFileStart->leOffset > fileSize)
			{
				return false;
			}
			if (m_pCultureHeader->offsetToCultureItemData + m_pCultureHeader->sizeCultureItem > fileSize)
			{
				return false;
			}
			if (m_pCultureHeader->cultureIDTableOffset > fileSize)
			{
				return false;
			}
			if (m_pCultureHeader->cultureNameTableOffset + 8 > fileSize)
			{
				return false;
			}
			if (m_pCultureHeader->regionNameTableOffset > fileSize)
			{
				return false;
			}
			if (m_pCultureHeader->offsetToCalendarItemData + m_pCultureHeader->sizeCalendarItem > fileSize)
			{
				return false;
			}
			if (m_pCultureHeader->offsetToDataPool > fileSize)
			{
				return false;
			}
			ushort* ptr = (ushort*)(m_pDataFileStart + (int)m_pCultureHeader->offsetToDataPool);
			int num = (int)(fileSize - ((long)ptr - (long)m_pDataFileStart)) / 2;
			if (num <= 0)
			{
				return false;
			}
			uint num2 = *(ushort*)(m_pDataFileStart + (int)m_pCultureHeader->cultureNameTableOffset);
			CultureTableData* ptr2 = (CultureTableData*)(m_pDataFileStart + (int)m_pCultureHeader->offsetToCultureItemData);
			if (ptr2->iLanguage == 127 || !IsValidLcid(ptr2->iLanguage, canBeCustomLcid: true))
			{
				return false;
			}
			string text = CheckAndGetTheString(ptr, ptr2->sName, num);
			if (string.IsNullOrEmpty(text))
			{
				return false;
			}
			if (num2 != ptr2->sName && !text.Equals(CheckAndGetTheString(ptr, num2, num)))
			{
				return false;
			}
			string text2 = CheckAndGetTheString(ptr, ptr2->sParent, num);
			if (text2 == null || text2.Equals(text, StringComparison.OrdinalIgnoreCase))
			{
				return false;
			}
			if (!IsValidLcid(ptr2->iTextInfo, canBeCustomLcid: false) || !IsValidLcid((int)ptr2->iCompareInfo, canBeCustomLcid: false))
			{
				return false;
			}
			if (!ValidateString(ptr, ptr2->waGrouping, num))
			{
				return false;
			}
			if (!ValidateString(ptr, ptr2->waMonetaryGrouping, num))
			{
				return false;
			}
			if (!ValidateString(ptr, ptr2->sListSeparator, num))
			{
				return false;
			}
			if (!ValidateString(ptr, ptr2->sDecimalSeparator, num))
			{
				return false;
			}
			if (!ValidateString(ptr, ptr2->sThousandSeparator, num))
			{
				return false;
			}
			if (!ValidateString(ptr, ptr2->sCurrency, num))
			{
				return false;
			}
			if (!ValidateString(ptr, ptr2->sMonetaryDecimal, num))
			{
				return false;
			}
			if (!ValidateString(ptr, ptr2->sMonetaryThousand, num))
			{
				return false;
			}
			if (!ValidateString(ptr, ptr2->sPositiveSign, num))
			{
				return false;
			}
			if (!ValidateString(ptr, ptr2->sNegativeSign, num))
			{
				return false;
			}
			if (!ValidateString(ptr, ptr2->sAM1159, num))
			{
				return false;
			}
			if (!ValidateString(ptr, ptr2->sPM2359, num))
			{
				return false;
			}
			if (!ValidateStringArray(ptr, ptr2->saNativeDigits, num))
			{
				return false;
			}
			if (!ValidateStringArray(ptr, ptr2->saTimeFormat, num))
			{
				return false;
			}
			if (!ValidateStringArray(ptr, ptr2->saShortDate, num))
			{
				return false;
			}
			if (!ValidateStringArray(ptr, ptr2->saLongDate, num))
			{
				return false;
			}
			if (!ValidateStringArray(ptr, ptr2->saYearMonth, num))
			{
				return false;
			}
			if (!ValidateStringArray(ptr, ptr2->saDuration, num))
			{
				return false;
			}
			if (!ValidateString(ptr, ptr2->waCalendars, num))
			{
				return false;
			}
			if (!ValidateString(ptr, ptr2->sAbbrevLang, num))
			{
				return false;
			}
			if (!ValidateString(ptr, ptr2->sISO639Language, num))
			{
				return false;
			}
			if (!ValidateString(ptr, ptr2->sEnglishLanguage, num))
			{
				return false;
			}
			if (!ValidateString(ptr, ptr2->sNativeLanguage, num))
			{
				return false;
			}
			if (!ValidateString(ptr, ptr2->sEnglishCountry, num))
			{
				return false;
			}
			if (!ValidateString(ptr, ptr2->sNativeCountry, num))
			{
				return false;
			}
			if (!ValidateString(ptr, ptr2->sAbbrevCountry, num))
			{
				return false;
			}
			if (!ValidateString(ptr, ptr2->sISO3166CountryName, num))
			{
				return false;
			}
			if (!ValidateString(ptr, ptr2->sIntlMonetarySymbol, num))
			{
				return false;
			}
			if (!ValidateString(ptr, ptr2->sEnglishCurrency, num))
			{
				return false;
			}
			if (!ValidateString(ptr, ptr2->sNativeCurrency, num))
			{
				return false;
			}
			if (!ValidateString(ptr, ptr2->waFontSignature, num))
			{
				return false;
			}
			if (!ValidateString(ptr, ptr2->sISO639Language2, num))
			{
				return false;
			}
			if (!ValidateString(ptr, ptr2->sISO3166CountryName2, num))
			{
				return false;
			}
			if (!ValidateStringArray(ptr, ptr2->saDayNames, num))
			{
				return false;
			}
			if (!ValidateStringArray(ptr, ptr2->saAbbrevDayNames, num))
			{
				return false;
			}
			if (!ValidateStringArray(ptr, ptr2->saMonthNames, num))
			{
				return false;
			}
			if (!ValidateStringArray(ptr, ptr2->saAbbrevMonthNames, num))
			{
				return false;
			}
			if (!ValidateStringArray(ptr, ptr2->saMonthGenitiveNames, num))
			{
				return false;
			}
			if (!ValidateStringArray(ptr, ptr2->saAbbrevMonthGenitiveNames, num))
			{
				return false;
			}
			if (!ValidateStringArray(ptr, ptr2->saNativeCalendarNames, num))
			{
				return false;
			}
			if (!ValidateStringArray(ptr, ptr2->saAltSortID, num))
			{
				return false;
			}
			if (!ValidateString(ptr, ptr2->sEnglishDisplayName, num))
			{
				return false;
			}
			if (!ValidateString(ptr, ptr2->sNativeDisplayName, num))
			{
				return false;
			}
			if (!ValidateString(ptr, ptr2->sPercent, num))
			{
				return false;
			}
			if (!ValidateString(ptr, ptr2->sNaN, num))
			{
				return false;
			}
			if (!ValidateString(ptr, ptr2->sPositiveInfinity, num))
			{
				return false;
			}
			if (!ValidateString(ptr, ptr2->sNegativeInfinity, num))
			{
				return false;
			}
			if (!ValidateString(ptr, ptr2->sMonthDay, num))
			{
				return false;
			}
			if (!ValidateString(ptr, ptr2->sAdEra, num))
			{
				return false;
			}
			if (!ValidateString(ptr, ptr2->sAbbrevAdEra, num))
			{
				return false;
			}
			if (!ValidateString(ptr, ptr2->sRegionName, num))
			{
				return false;
			}
			if (!ValidateString(ptr, ptr2->sConsoleFallbackName, num))
			{
				return false;
			}
			if (!ValidateStringArray(ptr, ptr2->saShortTime, num))
			{
				return false;
			}
			if (!ValidateStringArray(ptr, ptr2->saSuperShortDayNames, num))
			{
				return false;
			}
			if (!ValidateStringArray(ptr, ptr2->saDateWords, num))
			{
				return false;
			}
			if (!ValidateString(ptr, ptr2->sSpecificCulture, num))
			{
				return false;
			}
			return true;
		}

		internal unsafe int GetDataItemFromCultureName(string name, out int culture, out string actualName)
		{
			culture = -1;
			actualName = "";
			CultureTableItem cultureTableItem = (CultureTableItem)hashByName[name];
			if (cultureTableItem != null && cultureTableItem.culture != 0)
			{
				culture = cultureTableItem.culture;
				actualName = cultureTableItem.name;
				return cultureTableItem.dataItem;
			}
			int num = 0;
			int num2 = m_pCultureHeader->numCultureNames - 1;
			while (num <= num2)
			{
				int num3 = (num + num2) / 2;
				int num4 = CompareStringToStringPoolStringBinary(name, m_pCultureNameIndex[num3].strOffset);
				if (num4 == 0)
				{
					cultureTableItem = new CultureTableItem();
					int result = (cultureTableItem.dataItem = m_pCultureNameIndex[num3].dataItemIndex);
					culture = (cultureTableItem.culture = m_pCultureNameIndex[num3].actualCultureID);
					actualName = (cultureTableItem.name = GetStringPoolString(m_pCultureNameIndex[num3].strOffset));
					hashByName[name] = cultureTableItem;
					return result;
				}
				if (num4 < 0)
				{
					num2 = num3 - 1;
				}
				else
				{
					num = num3 + 1;
				}
			}
			culture = -1;
			return -1;
		}

		internal unsafe int GetDataItemFromRegionName(string name)
		{
			object obj;
			if ((obj = hashByRegionName[name]) != null)
			{
				return (int)obj;
			}
			int num = 0;
			int num2 = m_pCultureHeader->numRegionNames - 1;
			while (num <= num2)
			{
				int num3 = (num + num2) / 2;
				int num4 = CompareStringToStringPoolStringBinary(name, m_pRegionNameIndex[num3].strOffset);
				if (num4 == 0)
				{
					int dataItemIndex = m_pRegionNameIndex[num3].dataItemIndex;
					hashByRegionName[name] = dataItemIndex;
					return dataItemIndex;
				}
				if (num4 < 0)
				{
					num2 = num3 - 1;
				}
				else
				{
					num = num3 + 1;
				}
			}
			return -1;
		}

		internal unsafe int GetDataItemFromCultureID(int cultureID, out string actualName)
		{
			CultureTableItem cultureTableItem = (CultureTableItem)hashByLcid[cultureID];
			if (cultureTableItem != null && cultureTableItem.culture != 0)
			{
				actualName = cultureTableItem.name;
				return cultureTableItem.dataItem;
			}
			int num = 0;
			int num2 = m_pCultureHeader->numCultureNames - 1;
			while (num <= num2)
			{
				int num3 = (num + num2) / 2;
				int num4 = cultureID - m_pCultureIDIndex[num3].actualCultureID;
				if (num4 == 0)
				{
					cultureTableItem = new CultureTableItem();
					int result = (cultureTableItem.dataItem = m_pCultureIDIndex[num3].dataItemIndex);
					cultureTableItem.culture = cultureID;
					actualName = (cultureTableItem.name = GetStringPoolString(m_pCultureIDIndex[num3].strOffset));
					hashByLcid[cultureID] = cultureTableItem;
					return result;
				}
				if (num4 < 0)
				{
					num2 = num3 - 1;
				}
				else
				{
					num = num3 + 1;
				}
			}
			actualName = "";
			return -1;
		}

		internal static bool IsInstalledLCID(int cultureID)
		{
			if ((Environment.OSInfo & Environment.OSName.Win9x) != 0)
			{
				return CultureInfo.IsWin9xInstalledCulture(string.Format(CultureInfo.InvariantCulture, "{0,8:X08}", cultureID), cultureID);
			}
			return CultureInfo.IsValidLCID(cultureID, 1);
		}

		internal bool IsExistingCulture(int lcid)
		{
			if (lcid == 0)
			{
				return false;
			}
			string actualName;
			return GetDataItemFromCultureID(lcid, out actualName) >= 0;
		}

		internal static bool IsOldNeutralChineseCulture(CultureInfo ci)
		{
			if ((ci.LCID == 31748 && ci.Name.Equals("zh-CHT")) || (ci.LCID == 4 && ci.Name.Equals("zh-CHS")))
			{
				return true;
			}
			return false;
		}

		internal static bool IsNewNeutralChineseCulture(CultureInfo ci)
		{
			if ((ci.LCID == 31748 && ci.Name.Equals("zh-Hant")) || (ci.LCID == 4 && ci.Name.Equals("zh-Hans")))
			{
				return true;
			}
			return false;
		}

		internal unsafe CultureInfo[] GetCultures(CultureTypes types)
		{
			if (types <= (CultureTypes)0 || ((uint)types & 0xFFFFFF80u) != 0)
			{
				throw new ArgumentOutOfRangeException("types", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("ArgumentOutOfRange_Range"), CultureTypes.NeutralCultures, CultureTypes.FrameworkCultures));
			}
			ArrayList arrayList = new ArrayList();
			bool flag = (types & CultureTypes.SpecificCultures) != 0;
			bool flag2 = (types & CultureTypes.NeutralCultures) != 0;
			bool flag3 = (types & CultureTypes.InstalledWin32Cultures) != 0;
			bool flag4 = (types & CultureTypes.UserCustomCulture) != 0;
			bool flag5 = (types & CultureTypes.ReplacementCultures) != 0;
			bool flag6 = (types & CultureTypes.FrameworkCultures) != 0;
			bool flag7 = (types & CultureTypes.WindowsOnlyCultures) != 0;
			StringBuilder stringBuilder = new StringBuilder(260);
			stringBuilder.Append(Environment.InternalWindowsDirectory);
			stringBuilder.Append("\\Globalization\\");
			string path = stringBuilder.ToString();
			new FileIOPermission(FileIOPermissionAccess.PathDiscovery, path).Assert();
			try
			{
				if (Directory.Exists(path))
				{
					DirectoryInfo directoryInfo = new DirectoryInfo(path);
					FileInfo[] files = directoryInfo.GetFiles("*.nlp");
					foreach (FileInfo fileInfo in files)
					{
						if (fileInfo.Name.Length <= 4)
						{
							continue;
						}
						try
						{
							CultureInfo cultureInfo = new CultureInfo(fileInfo.Name.Substring(0, fileInfo.Name.Length - 4), useUserOverride: true);
							CultureTypes cultureTypes = cultureInfo.CultureTypes;
							if (!IsNewNeutralChineseCulture(cultureInfo) && ((flag4 && (cultureTypes & CultureTypes.UserCustomCulture) != 0) || (flag5 && (cultureTypes & CultureTypes.ReplacementCultures) != 0) || (flag && (cultureTypes & CultureTypes.SpecificCultures) != 0) || (flag2 && (cultureTypes & CultureTypes.NeutralCultures) != 0) || (flag6 && (cultureTypes & CultureTypes.FrameworkCultures) != 0) || (flag3 && (cultureTypes & CultureTypes.InstalledWin32Cultures) != 0) || (flag7 && (cultureTypes & CultureTypes.WindowsOnlyCultures) != 0)))
							{
								arrayList.Add(cultureInfo);
							}
						}
						catch (ArgumentException)
						{
						}
					}
				}
			}
			finally
			{
				CodeAccessPermission.RevertAssert();
			}
			if (flag2 || flag || flag6 || flag3)
			{
				for (int j = 0; j < m_pCultureHeader->numCultureNames; j++)
				{
					int actualCultureID = m_pCultureIDIndex[j].actualCultureID;
					if (CultureInfo.GetSortID(actualCultureID) == 0 && actualCultureID != 1034)
					{
						CultureInfo cultureInfo2 = new CultureInfo(actualCultureID);
						CultureTypes cultureTypes2 = cultureInfo2.CultureTypes;
						if ((cultureTypes2 & CultureTypes.ReplacementCultures) == 0 && (flag6 || (flag && cultureInfo2.Name.Length > 0 && (cultureTypes2 & CultureTypes.SpecificCultures) != 0) || (flag2 && ((cultureTypes2 & CultureTypes.NeutralCultures) != 0 || cultureInfo2.Name.Length == 0)) || (flag3 && (cultureTypes2 & CultureTypes.InstalledWin32Cultures) != 0)))
						{
							arrayList.Add(cultureInfo2);
						}
					}
					if (actualCultureID == 4 || actualCultureID == 31748)
					{
						j++;
					}
				}
			}
			if (flag7 || flag || flag3)
			{
				CultureTableRecord.InitSyntheticMapping();
				foreach (int key in CultureTableRecord.SyntheticLcidToNameCache.Keys)
				{
					if (CultureInfo.GetSortID(key) == 0)
					{
						CultureInfo cultureInfo3 = new CultureInfo(key);
						if ((cultureInfo3.CultureTypes & CultureTypes.ReplacementCultures) == 0)
						{
							arrayList.Add(cultureInfo3);
						}
					}
				}
			}
			CultureInfo[] array = new CultureInfo[arrayList.Count];
			arrayList.CopyTo(array, 0);
			return array;
		}
	}
}
