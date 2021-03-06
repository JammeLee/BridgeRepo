namespace System.ComponentModel
{
	[AttributeUsage(AttributeTargets.Property)]
	public sealed class SettingsBindableAttribute : Attribute
	{
		public static readonly SettingsBindableAttribute Yes = new SettingsBindableAttribute(bindable: true);

		public static readonly SettingsBindableAttribute No = new SettingsBindableAttribute(bindable: false);

		private bool _bindable;

		public bool Bindable => _bindable;

		public SettingsBindableAttribute(bool bindable)
		{
			_bindable = bindable;
		}

		public override bool Equals(object obj)
		{
			if (obj == this)
			{
				return true;
			}
			if (obj != null && obj is SettingsBindableAttribute)
			{
				return ((SettingsBindableAttribute)obj).Bindable == _bindable;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return _bindable.GetHashCode();
		}
	}
}
