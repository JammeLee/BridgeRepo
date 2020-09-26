using System.ComponentModel.Design.Serialization;
using System.Globalization;
using System.Reflection;
using System.Security.Permissions;

namespace System.ComponentModel
{
	[HostProtection(SecurityAction.LinkDemand, SharedState = true)]
	public class DateTimeConverter : TypeConverter
	{
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
		{
			if (sourceType == typeof(string))
			{
				return true;
			}
			return base.CanConvertFrom(context, sourceType);
		}

		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
		{
			if (destinationType == typeof(InstanceDescriptor))
			{
				return true;
			}
			return base.CanConvertTo(context, destinationType);
		}

		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			if (value is string)
			{
				string text = ((string)value).Trim();
				if (text.Length == 0)
				{
					return DateTime.MinValue;
				}
				try
				{
					DateTimeFormatInfo dateTimeFormatInfo = null;
					if (culture != null)
					{
						dateTimeFormatInfo = (DateTimeFormatInfo)culture.GetFormat(typeof(DateTimeFormatInfo));
					}
					if (dateTimeFormatInfo != null)
					{
						return DateTime.Parse(text, dateTimeFormatInfo);
					}
					return DateTime.Parse(text, culture);
				}
				catch (FormatException innerException)
				{
					throw new FormatException(SR.GetString("ConvertInvalidPrimitive", (string)value, "DateTime"), innerException);
				}
			}
			return base.ConvertFrom(context, culture, value);
		}

		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if (destinationType == typeof(string) && value is DateTime)
			{
				DateTime d = (DateTime)value;
				if (d == DateTime.MinValue)
				{
					return string.Empty;
				}
				if (culture == null)
				{
					culture = CultureInfo.CurrentCulture;
				}
				DateTimeFormatInfo dateTimeFormatInfo = null;
				dateTimeFormatInfo = (DateTimeFormatInfo)culture.GetFormat(typeof(DateTimeFormatInfo));
				if (culture == CultureInfo.InvariantCulture)
				{
					if (d.TimeOfDay.TotalSeconds == 0.0)
					{
						return d.ToString("yyyy-MM-dd", culture);
					}
					return d.ToString(culture);
				}
				string format = ((d.TimeOfDay.TotalSeconds != 0.0) ? (dateTimeFormatInfo.ShortDatePattern + " " + dateTimeFormatInfo.ShortTimePattern) : dateTimeFormatInfo.ShortDatePattern);
				return d.ToString(format, CultureInfo.CurrentCulture);
			}
			if (destinationType == typeof(InstanceDescriptor) && value is DateTime)
			{
				DateTime dateTime = (DateTime)value;
				if (dateTime.Ticks == 0)
				{
					ConstructorInfo constructor = typeof(DateTime).GetConstructor(new Type[1]
					{
						typeof(long)
					});
					if (constructor != null)
					{
						return new InstanceDescriptor(constructor, new object[1]
						{
							dateTime.Ticks
						});
					}
				}
				ConstructorInfo constructor2 = typeof(DateTime).GetConstructor(new Type[7]
				{
					typeof(int),
					typeof(int),
					typeof(int),
					typeof(int),
					typeof(int),
					typeof(int),
					typeof(int)
				});
				if (constructor2 != null)
				{
					return new InstanceDescriptor(constructor2, new object[7]
					{
						dateTime.Year,
						dateTime.Month,
						dateTime.Day,
						dateTime.Hour,
						dateTime.Minute,
						dateTime.Second,
						dateTime.Millisecond
					});
				}
			}
			return base.ConvertTo(context, culture, value, destinationType);
		}
	}
}
