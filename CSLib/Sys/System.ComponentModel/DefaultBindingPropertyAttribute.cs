namespace System.ComponentModel
{
	[AttributeUsage(AttributeTargets.Class)]
	public sealed class DefaultBindingPropertyAttribute : Attribute
	{
		private readonly string name;

		public static readonly DefaultBindingPropertyAttribute Default = new DefaultBindingPropertyAttribute();

		public string Name => name;

		public DefaultBindingPropertyAttribute()
		{
			name = null;
		}

		public DefaultBindingPropertyAttribute(string name)
		{
			this.name = name;
		}

		public override bool Equals(object obj)
		{
			DefaultBindingPropertyAttribute defaultBindingPropertyAttribute = obj as DefaultBindingPropertyAttribute;
			if (defaultBindingPropertyAttribute != null)
			{
				return defaultBindingPropertyAttribute.Name == name;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
	}
}
