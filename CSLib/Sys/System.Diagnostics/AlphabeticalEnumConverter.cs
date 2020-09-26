using System.Collections;
using System.ComponentModel;

namespace System.Diagnostics
{
	internal class AlphabeticalEnumConverter : EnumConverter
	{
		public AlphabeticalEnumConverter(Type type)
			: base(type)
		{
		}

		public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
		{
			if (base.Values == null)
			{
				Array values = Enum.GetValues(base.EnumType);
				object[] array = new object[values.Length];
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = ConvertTo(context, null, values.GetValue(i), typeof(string));
				}
				Array.Sort(array, values, 0, values.Length, System.Collections.Comparer.Default);
				base.Values = new StandardValuesCollection(values);
			}
			return base.Values;
		}
	}
}
