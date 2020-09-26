namespace System.ComponentModel
{
	[AttributeUsage(AttributeTargets.Class)]
	public sealed class DefaultEventAttribute : Attribute
	{
		private readonly string name;

		public static readonly DefaultEventAttribute Default = new DefaultEventAttribute(null);

		public string Name => name;

		public DefaultEventAttribute(string name)
		{
			this.name = name;
		}

		public override bool Equals(object obj)
		{
			DefaultEventAttribute defaultEventAttribute = obj as DefaultEventAttribute;
			if (defaultEventAttribute != null)
			{
				return defaultEventAttribute.Name == name;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
	}
}
