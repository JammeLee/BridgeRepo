namespace System.ComponentModel
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
	public sealed class DesignTimeVisibleAttribute : Attribute
	{
		private bool visible;

		public static readonly DesignTimeVisibleAttribute Yes = new DesignTimeVisibleAttribute(visible: true);

		public static readonly DesignTimeVisibleAttribute No = new DesignTimeVisibleAttribute(visible: false);

		public static readonly DesignTimeVisibleAttribute Default = Yes;

		public bool Visible => visible;

		public DesignTimeVisibleAttribute(bool visible)
		{
			this.visible = visible;
		}

		public DesignTimeVisibleAttribute()
		{
		}

		public override bool Equals(object obj)
		{
			if (obj == this)
			{
				return true;
			}
			DesignTimeVisibleAttribute designTimeVisibleAttribute = obj as DesignTimeVisibleAttribute;
			if (designTimeVisibleAttribute != null)
			{
				return designTimeVisibleAttribute.Visible == visible;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return typeof(DesignTimeVisibleAttribute).GetHashCode() ^ (visible ? (-1) : 0);
		}

		public override bool IsDefaultAttribute()
		{
			return Visible == Default.Visible;
		}
	}
}
