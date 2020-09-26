namespace System.ComponentModel
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Event)]
	public class DisplayNameAttribute : Attribute
	{
		public static readonly DisplayNameAttribute Default = new DisplayNameAttribute();

		private string _displayName;

		public virtual string DisplayName => DisplayNameValue;

		protected string DisplayNameValue
		{
			get
			{
				return _displayName;
			}
			set
			{
				_displayName = value;
			}
		}

		public DisplayNameAttribute()
			: this(string.Empty)
		{
		}

		public DisplayNameAttribute(string displayName)
		{
			_displayName = displayName;
		}

		public override bool Equals(object obj)
		{
			if (obj == this)
			{
				return true;
			}
			DisplayNameAttribute displayNameAttribute = obj as DisplayNameAttribute;
			if (displayNameAttribute != null)
			{
				return displayNameAttribute.DisplayName == DisplayName;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return DisplayName.GetHashCode();
		}

		public override bool IsDefaultAttribute()
		{
			return Equals(Default);
		}
	}
}
