using System.Runtime.InteropServices;

namespace System.Globalization
{
	[Serializable]
	[ComVisible(true)]
	[Flags]
	public enum CultureTypes
	{
		NeutralCultures = 0x1,
		SpecificCultures = 0x2,
		InstalledWin32Cultures = 0x4,
		AllCultures = 0x7,
		UserCustomCulture = 0x8,
		ReplacementCultures = 0x10,
		WindowsOnlyCultures = 0x20,
		FrameworkCultures = 0x40
	}
}
