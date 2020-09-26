namespace System.ComponentModel
{
	[AttributeUsage(AttributeTargets.Property)]
	[Obsolete("Use System.ComponentModel.SettingsBindableAttribute instead to work with the new settings model.")]
	public class RecommendedAsConfigurableAttribute : Attribute
	{
		private bool recommendedAsConfigurable;

		public static readonly RecommendedAsConfigurableAttribute No = new RecommendedAsConfigurableAttribute(recommendedAsConfigurable: false);

		public static readonly RecommendedAsConfigurableAttribute Yes = new RecommendedAsConfigurableAttribute(recommendedAsConfigurable: true);

		public static readonly RecommendedAsConfigurableAttribute Default = No;

		public bool RecommendedAsConfigurable => recommendedAsConfigurable;

		public RecommendedAsConfigurableAttribute(bool recommendedAsConfigurable)
		{
			this.recommendedAsConfigurable = recommendedAsConfigurable;
		}

		public override bool Equals(object obj)
		{
			if (obj == this)
			{
				return true;
			}
			RecommendedAsConfigurableAttribute recommendedAsConfigurableAttribute = obj as RecommendedAsConfigurableAttribute;
			if (recommendedAsConfigurableAttribute != null)
			{
				return recommendedAsConfigurableAttribute.RecommendedAsConfigurable == recommendedAsConfigurable;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		public override bool IsDefaultAttribute()
		{
			return !recommendedAsConfigurable;
		}
	}
}
