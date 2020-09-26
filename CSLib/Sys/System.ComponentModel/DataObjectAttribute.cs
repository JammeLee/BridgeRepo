namespace System.ComponentModel
{
	[AttributeUsage(AttributeTargets.Class)]
	public sealed class DataObjectAttribute : Attribute
	{
		public static readonly DataObjectAttribute DataObject = new DataObjectAttribute(isDataObject: true);

		public static readonly DataObjectAttribute NonDataObject = new DataObjectAttribute(isDataObject: false);

		public static readonly DataObjectAttribute Default = NonDataObject;

		private bool _isDataObject;

		public bool IsDataObject => _isDataObject;

		public DataObjectAttribute()
			: this(isDataObject: true)
		{
		}

		public DataObjectAttribute(bool isDataObject)
		{
			_isDataObject = isDataObject;
		}

		public override bool Equals(object obj)
		{
			if (obj == this)
			{
				return true;
			}
			DataObjectAttribute dataObjectAttribute = obj as DataObjectAttribute;
			if (dataObjectAttribute != null)
			{
				return dataObjectAttribute.IsDataObject == IsDataObject;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return _isDataObject.GetHashCode();
		}

		public override bool IsDefaultAttribute()
		{
			return Equals(Default);
		}
	}
}
