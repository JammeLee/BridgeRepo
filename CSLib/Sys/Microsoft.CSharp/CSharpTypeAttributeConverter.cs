using System.Reflection;

namespace Microsoft.CSharp
{
	internal class CSharpTypeAttributeConverter : CSharpModifierAttributeConverter
	{
		private static string[] names;

		private static object[] values;

		private static CSharpTypeAttributeConverter defaultConverter;

		public static CSharpTypeAttributeConverter Default
		{
			get
			{
				if (defaultConverter == null)
				{
					defaultConverter = new CSharpTypeAttributeConverter();
				}
				return defaultConverter;
			}
		}

		protected override string[] Names
		{
			get
			{
				if (names == null)
				{
					names = new string[2]
					{
						"Public",
						"Internal"
					};
				}
				return names;
			}
		}

		protected override object[] Values
		{
			get
			{
				if (values == null)
				{
					values = new object[2]
					{
						TypeAttributes.Public,
						TypeAttributes.NotPublic
					};
				}
				return values;
			}
		}

		protected override object DefaultValue => TypeAttributes.NotPublic;

		private CSharpTypeAttributeConverter()
		{
		}
	}
}
