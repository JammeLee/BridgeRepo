using System.Runtime.InteropServices;

namespace System.Runtime.Serialization
{
	[AttributeUsage(AttributeTargets.Field, Inherited = false)]
	[ComVisible(true)]
	public sealed class OptionalFieldAttribute : Attribute
	{
		private int versionAdded = 1;

		public int VersionAdded
		{
			get
			{
				return versionAdded;
			}
			set
			{
				if (value < 1)
				{
					throw new ArgumentException(Environment.GetResourceString("Serialization_OptionalFieldVersionValue"));
				}
				versionAdded = value;
			}
		}
	}
}
