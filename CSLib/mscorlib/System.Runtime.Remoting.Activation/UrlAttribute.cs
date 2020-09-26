using System.Runtime.InteropServices;
using System.Runtime.Remoting.Contexts;
using System.Security.Permissions;

namespace System.Runtime.Remoting.Activation
{
	[Serializable]
	[ComVisible(true)]
	public sealed class UrlAttribute : ContextAttribute
	{
		private string url;

		private static string propertyName = "UrlAttribute";

		public string UrlValue
		{
			[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
			get
			{
				return url;
			}
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
		public UrlAttribute(string callsiteURL)
			: base(propertyName)
		{
			if (callsiteURL == null)
			{
				throw new ArgumentNullException("callsiteURL");
			}
			url = callsiteURL;
		}

		public override bool Equals(object o)
		{
			if (o is IContextProperty && o is UrlAttribute)
			{
				return ((UrlAttribute)o).UrlValue.Equals(url);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return url.GetHashCode();
		}

		[ComVisible(true)]
		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
		public override bool IsContextOK(Context ctx, IConstructionCallMessage msg)
		{
			return false;
		}

		[ComVisible(true)]
		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
		public override void GetPropertiesForNewContext(IConstructionCallMessage ctorMsg)
		{
		}
	}
}
