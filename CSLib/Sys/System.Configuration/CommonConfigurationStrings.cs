using System.Globalization;

namespace System.Configuration
{
	internal static class CommonConfigurationStrings
	{
		internal const string UriSectionName = "uri";

		internal const string IriParsing = "iriParsing";

		internal const string Idn = "idn";

		internal const string Enabled = "enabled";

		internal static string UriSectionPath => GetSectionPath("uri");

		private static string GetSectionPath(string sectionName)
		{
			return string.Format(CultureInfo.InvariantCulture, "{0}", sectionName);
		}

		private static string GetSectionPath(string sectionName, string subSectionName)
		{
			return string.Format(CultureInfo.InvariantCulture, "{0}/{1}", sectionName, subSectionName);
		}
	}
}
