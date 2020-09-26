using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace System.Runtime.Remoting.Contexts
{
	[ComVisible(true)]
	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
	[SecurityPermission(SecurityAction.InheritanceDemand, Flags = SecurityPermissionFlag.Infrastructure)]
	public class ContextProperty
	{
		internal string _name;

		internal object _property;

		public virtual string Name => _name;

		public virtual object Property => _property;

		internal ContextProperty(string name, object prop)
		{
			_name = name;
			_property = prop;
		}
	}
}
