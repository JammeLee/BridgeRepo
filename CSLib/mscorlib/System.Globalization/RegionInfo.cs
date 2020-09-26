using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace System.Globalization
{
	[Serializable]
	[ComVisible(true)]
	public class RegionInfo
	{
		internal string m_name;

		[OptionalField(VersionAdded = 2)]
		private int m_cultureId;

		[NonSerialized]
		internal CultureTableRecord m_cultureTableRecord;

		internal static RegionInfo m_currentRegionInfo;

		internal int m_dataItem;

		public static RegionInfo CurrentRegion
		{
			get
			{
				RegionInfo regionInfo = m_currentRegionInfo;
				if (regionInfo == null)
				{
					regionInfo = new RegionInfo(CultureInfo.CurrentCulture.m_cultureTableRecord);
					if (regionInfo.m_cultureTableRecord.IsCustomCulture)
					{
						regionInfo.m_name = regionInfo.m_cultureTableRecord.SNAME;
					}
					m_currentRegionInfo = regionInfo;
				}
				return regionInfo;
			}
		}

		public virtual string Name
		{
			get
			{
				if (m_name == null)
				{
					m_name = m_cultureTableRecord.SREGIONNAME;
				}
				return m_name;
			}
		}

		public virtual string EnglishName => m_cultureTableRecord.SENGCOUNTRY;

		public virtual string DisplayName
		{
			get
			{
				if (m_cultureTableRecord.IsCustomCulture)
				{
					if (m_cultureTableRecord.IsReplacementCulture)
					{
						if (m_cultureTableRecord.IsSynthetic)
						{
							return m_cultureTableRecord.RegionNativeDisplayName;
						}
						return Environment.GetResourceString("Globalization.ri_" + m_cultureTableRecord.SREGIONNAME);
					}
					return m_cultureTableRecord.SNATIVECOUNTRY;
				}
				if (m_cultureTableRecord.IsSynthetic)
				{
					return m_cultureTableRecord.RegionNativeDisplayName;
				}
				return Environment.GetResourceString("Globalization.ri_" + m_cultureTableRecord.SREGIONNAME);
			}
		}

		[ComVisible(false)]
		public virtual string NativeName => m_cultureTableRecord.SNATIVECOUNTRY;

		public virtual string TwoLetterISORegionName => m_cultureTableRecord.SISO3166CTRYNAME;

		public virtual string ThreeLetterISORegionName => m_cultureTableRecord.SISO3166CTRYNAME2;

		public virtual bool IsMetric
		{
			get
			{
				int iMEASURE = m_cultureTableRecord.IMEASURE;
				return iMEASURE == 0;
			}
		}

		[ComVisible(false)]
		public virtual int GeoId => m_cultureTableRecord.IGEOID;

		public virtual string ThreeLetterWindowsRegionName => m_cultureTableRecord.SABBREVCTRYNAME;

		[ComVisible(false)]
		public virtual string CurrencyEnglishName => m_cultureTableRecord.SENGLISHCURRENCY;

		[ComVisible(false)]
		public virtual string CurrencyNativeName => m_cultureTableRecord.SNATIVECURRENCY;

		public virtual string CurrencySymbol => m_cultureTableRecord.SCURRENCY;

		public virtual string ISOCurrencySymbol => m_cultureTableRecord.SINTLSYMBOL;

		public RegionInfo(string name)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			if (name.Length == 0)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_InvalidRegionName", name), "name");
			}
			m_name = name.ToUpper(CultureInfo.InvariantCulture);
			m_cultureId = 0;
			m_cultureTableRecord = CultureTableRecord.GetCultureTableRecordForRegion(name, useUserOverride: true);
			if (m_cultureTableRecord.IsNeutralCulture)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_InvalidNeutralRegionName", name), "name");
			}
		}

		public RegionInfo(int culture)
		{
			if (culture == 127)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_NoRegionInvariantCulture"));
			}
			if (CultureTableRecord.IsCustomCultureId(culture))
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_CustomCultureCannotBePassedByNumber", "culture"));
			}
			if (CultureInfo.GetSubLangID(culture) == 0)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_CultureIsNeutral", culture), "culture");
			}
			m_cultureTableRecord = CultureTableRecord.GetCultureTableRecord(culture, useUserOverride: true);
			if (m_cultureTableRecord.IsNeutralCulture)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_CultureIsNeutral", culture), "culture");
			}
			m_name = m_cultureTableRecord.SREGIONNAME;
			m_cultureId = culture;
		}

		internal RegionInfo(CultureTableRecord table)
		{
			m_cultureTableRecord = table;
			m_name = m_cultureTableRecord.SREGIONNAME;
		}

		[OnDeserialized]
		private void OnDeserialized(StreamingContext ctx)
		{
			if (m_name == null)
			{
				m_cultureTableRecord = CultureTableRecord.GetCultureTableRecord(CultureTableRecord.IdFromEverettRegionInfoDataItem(m_dataItem), useUserOverride: true);
				m_name = m_cultureTableRecord.SREGIONNAME;
			}
			else if (m_cultureId != 0)
			{
				m_cultureTableRecord = CultureTableRecord.GetCultureTableRecord(m_cultureId, useUserOverride: true);
			}
			else
			{
				m_cultureTableRecord = CultureTableRecord.GetCultureTableRecordForRegion(m_name, useUserOverride: true);
			}
		}

		[OnSerializing]
		private void OnSerializing(StreamingContext ctx)
		{
			m_dataItem = m_cultureTableRecord.EverettRegionDataItem();
		}

		public override bool Equals(object value)
		{
			RegionInfo regionInfo = value as RegionInfo;
			if (regionInfo != null)
			{
				return Name.Equals(regionInfo.Name);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return Name.GetHashCode();
		}

		public override string ToString()
		{
			return Name;
		}
	}
}
