using System.Runtime.CompilerServices;

namespace System.Globalization
{
	internal class CalendarTable : BaseInfoTable
	{
		private static CalendarTable m_defaultInstance;

		private unsafe CalendarTableData* m_calendars;

		internal static CalendarTable Default => m_defaultInstance;

		static CalendarTable()
		{
			m_defaultInstance = new CalendarTable("culture.nlp", fromAssembly: true);
		}

		internal CalendarTable(string fileName, bool fromAssembly)
			: base(fileName, fromAssembly)
		{
		}

		internal unsafe override void SetDataItemPointers()
		{
			m_itemSize = m_pCultureHeader->sizeCalendarItem;
			m_numItem = m_pCultureHeader->numCalendarItems;
			m_pDataPool = (ushort*)(m_pDataFileStart + (int)m_pCultureHeader->offsetToDataPool);
			m_pItemData = m_pDataFileStart + (int)m_pCultureHeader->offsetToCalendarItemData - (int)m_itemSize;
			m_calendars = (CalendarTableData*)(m_pDataFileStart + (int)m_pCultureHeader->offsetToCalendarItemData - sizeof(CalendarTableData));
		}

		internal unsafe int ICURRENTERA(int id)
		{
			if (JapaneseCalendarTable.IsJapaneseCalendar(id))
			{
				return JapaneseCalendarTable.CurrentEra(id);
			}
			return m_calendars[id].iCurrentEra;
		}

		internal unsafe int IFORMATFLAGS(int id)
		{
			return m_calendars[id].iFormatFlags;
		}

		internal unsafe string[] SDAYNAMES(int id)
		{
			return GetStringArray(m_calendars[id].saDayNames);
		}

		internal unsafe string[] SABBREVDAYNAMES(int id)
		{
			return GetStringArray(m_calendars[id].saAbbrevDayNames);
		}

		internal unsafe string[] SSUPERSHORTDAYNAMES(int id)
		{
			return GetStringArray(m_calendars[id].saSuperShortDayNames);
		}

		internal unsafe string[] SMONTHNAMES(int id)
		{
			return GetStringArray(m_calendars[id].saMonthNames);
		}

		internal unsafe string[] SABBREVMONTHNAMES(int id)
		{
			return GetStringArray(m_calendars[id].saAbbrevMonthNames);
		}

		internal unsafe string[] SLEAPYEARMONTHNAMES(int id)
		{
			return GetStringArray(m_calendars[id].saLeapYearMonthNames);
		}

		internal unsafe string[] SSHORTDATE(int id)
		{
			return GetStringArray(m_calendars[id].saShortDate);
		}

		internal unsafe string[] SLONGDATE(int id)
		{
			return GetStringArray(m_calendars[id].saLongDate);
		}

		internal unsafe string[] SYEARMONTH(int id)
		{
			return GetStringArray(m_calendars[id].saYearMonth);
		}

		internal unsafe string SMONTHDAY(int id)
		{
			return GetStringPoolString(m_calendars[id].sMonthDay);
		}

		internal unsafe int[][] SERARANGES(int id)
		{
			if (JapaneseCalendarTable.IsJapaneseCalendar(id))
			{
				return JapaneseCalendarTable.EraRanges(id);
			}
			return GetWordArrayArray(m_calendars[id].waaEraRanges);
		}

		internal unsafe string[] SERANAMES(int id)
		{
			if (JapaneseCalendarTable.IsJapaneseCalendar(id))
			{
				return JapaneseCalendarTable.EraNames(id);
			}
			return GetStringArray(m_calendars[id].saEraNames);
		}

		internal unsafe string[] SABBREVERANAMES(int id)
		{
			if (JapaneseCalendarTable.IsJapaneseCalendar(id))
			{
				return JapaneseCalendarTable.AbbrevEraNames(id);
			}
			return GetStringArray(m_calendars[id].saAbbrevEraNames);
		}

		internal unsafe string[] SABBREVENGERANAMES(int id)
		{
			if (JapaneseCalendarTable.IsJapaneseCalendar(id))
			{
				return JapaneseCalendarTable.EnglishEraNames(id);
			}
			return GetStringArray(m_calendars[id].saAbbrevEnglishEraNames);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern string nativeGetEraName(int culture, int calID);
	}
}
