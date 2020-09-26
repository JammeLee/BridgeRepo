using System.Collections;
using System.ComponentModel.Design.Serialization;
using System.Globalization;
using System.Reflection;
using System.Security.Permissions;
using System.Threading;

namespace System.ComponentModel
{
	[HostProtection(SecurityAction.LinkDemand, SharedState = true)]
	public class CultureInfoConverter : TypeConverter
	{
		private class CultureComparer : IComparer
		{
			public int Compare(object item1, object item2)
			{
				if (item1 == null)
				{
					if (item2 == null)
					{
						return 0;
					}
					return -1;
				}
				if (item2 == null)
				{
					return 1;
				}
				string displayName = ((CultureInfo)item1).DisplayName;
				string displayName2 = ((CultureInfo)item2).DisplayName;
				CompareInfo compareInfo = CultureInfo.CurrentCulture.CompareInfo;
				return compareInfo.Compare(displayName, displayName2, CompareOptions.StringSort);
			}
		}

		private StandardValuesCollection values;

		private string DefaultCultureString => SR.GetString("CultureInfoConverterDefaultCultureString");

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
				string text = (string)value;
				CultureInfo cultureInfo = null;
				CultureInfo currentUICulture = Thread.CurrentThread.CurrentUICulture;
				if (culture != null && culture.Equals(CultureInfo.InvariantCulture))
				{
					Thread.CurrentThread.CurrentUICulture = culture;
				}
				try
				{
					if (text == null || text.Length == 0 || string.Compare(text, DefaultCultureString, StringComparison.Ordinal) == 0)
					{
						cultureInfo = CultureInfo.InvariantCulture;
					}
					if (cultureInfo == null)
					{
						ICollection standardValues = GetStandardValues(context);
						IEnumerator enumerator = standardValues.GetEnumerator();
						while (enumerator.MoveNext())
						{
							CultureInfo cultureInfo2 = (CultureInfo)enumerator.Current;
							if (cultureInfo2 != null && string.Compare(cultureInfo2.DisplayName, text, StringComparison.Ordinal) == 0)
							{
								cultureInfo = cultureInfo2;
								break;
							}
						}
					}
					if (cultureInfo == null)
					{
						try
						{
							cultureInfo = new CultureInfo(text);
						}
						catch
						{
						}
					}
					if (cultureInfo == null)
					{
						text = text.ToLower(CultureInfo.CurrentCulture);
						IEnumerator enumerator2 = values.GetEnumerator();
						while (enumerator2.MoveNext())
						{
							CultureInfo cultureInfo3 = (CultureInfo)enumerator2.Current;
							if (cultureInfo3 != null && cultureInfo3.DisplayName.ToLower(CultureInfo.CurrentCulture).StartsWith(text))
							{
								cultureInfo = cultureInfo3;
								break;
							}
						}
					}
				}
				finally
				{
					Thread.CurrentThread.CurrentUICulture = currentUICulture;
				}
				if (cultureInfo == null)
				{
					throw new ArgumentException(SR.GetString("CultureInfoConverterInvalidCulture", (string)value));
				}
				return cultureInfo;
			}
			return base.ConvertFrom(context, culture, value);
		}

		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if (destinationType == null)
			{
				throw new ArgumentNullException("destinationType");
			}
			if (destinationType == typeof(string))
			{
				CultureInfo currentUICulture = Thread.CurrentThread.CurrentUICulture;
				if (culture != null && culture.Equals(CultureInfo.InvariantCulture))
				{
					Thread.CurrentThread.CurrentUICulture = culture;
				}
				try
				{
					if (value == null || value == CultureInfo.InvariantCulture)
					{
						return DefaultCultureString;
					}
					return ((CultureInfo)value).DisplayName;
				}
				finally
				{
					Thread.CurrentThread.CurrentUICulture = currentUICulture;
				}
			}
			if (destinationType == typeof(InstanceDescriptor) && value is CultureInfo)
			{
				CultureInfo cultureInfo = (CultureInfo)value;
				ConstructorInfo constructor = typeof(CultureInfo).GetConstructor(new Type[1]
				{
					typeof(string)
				});
				if (constructor != null)
				{
					return new InstanceDescriptor(constructor, new object[1]
					{
						cultureInfo.Name
					});
				}
			}
			return base.ConvertTo(context, culture, value, destinationType);
		}

		public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
		{
			if (values == null)
			{
				CultureInfo[] cultures = CultureInfo.GetCultures(CultureTypes.NeutralCultures | CultureTypes.SpecificCultures);
				CultureInfo[] array = new CultureInfo[cultures.Length + 1];
				Array.Copy(cultures, array, cultures.Length);
				Array.Sort(array, new CultureComparer());
				if (array[0] == null)
				{
					array[0] = CultureInfo.InvariantCulture;
				}
				values = new StandardValuesCollection(array);
			}
			return values;
		}

		public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
		{
			return false;
		}

		public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
		{
			return true;
		}
	}
}
