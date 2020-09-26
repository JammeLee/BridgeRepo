using System.Runtime.InteropServices;

namespace System.Security.Principal
{
	[ComVisible(true)]
	public interface IIdentity
	{
		string Name
		{
			get;
		}

		string AuthenticationType
		{
			get;
		}

		bool IsAuthenticated
		{
			get;
		}
	}
}
