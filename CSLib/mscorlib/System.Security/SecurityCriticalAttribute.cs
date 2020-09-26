namespace System.Security
{
	[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Module | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Event | AttributeTargets.Interface | AttributeTargets.Delegate, AllowMultiple = false, Inherited = false)]
	public sealed class SecurityCriticalAttribute : Attribute
	{
		internal SecurityCriticalScope _val;

		public SecurityCriticalScope Scope => _val;

		public SecurityCriticalAttribute()
		{
		}

		public SecurityCriticalAttribute(SecurityCriticalScope scope)
		{
			_val = scope;
		}
	}
}
