namespace System.Globalization
{
	[Serializable]
	internal class EraInfo
	{
		internal int era;

		internal long ticks;

		internal int yearOffset;

		internal int minEraYear;

		internal int maxEraYear;

		internal EraInfo(int era, long ticks, int yearOffset, int minEraYear, int maxEraYear)
		{
			this.era = era;
			this.ticks = ticks;
			this.yearOffset = yearOffset;
			this.minEraYear = minEraYear;
			this.maxEraYear = maxEraYear;
		}
	}
}
