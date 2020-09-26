using System.Globalization;
using System.Security.Permissions;

namespace System.ComponentModel
{
	[HostProtection(SecurityAction.LinkDemand, SharedState = true)]
	public class SByteConverter : BaseNumberConverter
	{
		internal override Type TargetType => typeof(sbyte);

		internal override object FromString(string value, int radix)
		{
			return Convert.ToSByte(value, radix);
		}

		internal override object FromString(string value, NumberFormatInfo formatInfo)
		{
			return sbyte.Parse(value, NumberStyles.Integer, formatInfo);
		}

		internal override object FromString(string value, CultureInfo culture)
		{
			return sbyte.Parse(value, culture);
		}

		internal override string ToString(object value, NumberFormatInfo formatInfo)
		{
			return ((sbyte)value).ToString("G", formatInfo);
		}
	}
}
