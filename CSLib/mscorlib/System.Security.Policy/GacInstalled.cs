using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace System.Security.Policy
{
	[Serializable]
	[ComVisible(true)]
	public sealed class GacInstalled : IIdentityPermissionFactory, IBuiltInEvidence
	{
		public IPermission CreateIdentityPermission(Evidence evidence)
		{
			return new GacIdentityPermission();
		}

		public override bool Equals(object o)
		{
			if (o is GacInstalled)
			{
				return true;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return 0;
		}

		public object Copy()
		{
			return new GacInstalled();
		}

		internal SecurityElement ToXml()
		{
			SecurityElement securityElement = new SecurityElement(GetType().FullName);
			securityElement.AddAttribute("version", "1");
			return securityElement;
		}

		int IBuiltInEvidence.OutputToBuffer(char[] buffer, int position, bool verbose)
		{
			buffer[position] = '\t';
			return position + 1;
		}

		int IBuiltInEvidence.GetRequiredSize(bool verbose)
		{
			return 1;
		}

		int IBuiltInEvidence.InitFromBuffer(char[] buffer, int position)
		{
			return position;
		}

		public override string ToString()
		{
			return ToXml().ToString();
		}
	}
}
