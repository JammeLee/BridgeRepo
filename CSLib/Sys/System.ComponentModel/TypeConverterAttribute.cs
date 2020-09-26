using System.Globalization;

namespace System.ComponentModel
{
	[AttributeUsage(AttributeTargets.All)]
	public sealed class TypeConverterAttribute : Attribute
	{
		private string typeName;

		public static readonly TypeConverterAttribute Default = new TypeConverterAttribute();

		public string ConverterTypeName => typeName;

		public TypeConverterAttribute()
		{
			typeName = string.Empty;
		}

		public TypeConverterAttribute(Type type)
		{
			typeName = type.AssemblyQualifiedName;
		}

		public TypeConverterAttribute(string typeName)
		{
			typeName.ToUpper(CultureInfo.InvariantCulture);
			this.typeName = typeName;
		}

		public override bool Equals(object obj)
		{
			TypeConverterAttribute typeConverterAttribute = obj as TypeConverterAttribute;
			if (typeConverterAttribute != null)
			{
				return typeConverterAttribute.ConverterTypeName == typeName;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return typeName.GetHashCode();
		}
	}
}
