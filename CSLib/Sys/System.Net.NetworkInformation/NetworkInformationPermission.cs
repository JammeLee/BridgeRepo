using System.Security;
using System.Security.Permissions;

namespace System.Net.NetworkInformation
{
	[Serializable]
	public sealed class NetworkInformationPermission : CodeAccessPermission, IUnrestrictedPermission
	{
		private NetworkInformationAccess access;

		private bool unrestricted;

		public NetworkInformationAccess Access => access;

		public NetworkInformationPermission(PermissionState state)
		{
			if (state == PermissionState.Unrestricted)
			{
				access = NetworkInformationAccess.Read | NetworkInformationAccess.Ping;
				unrestricted = true;
			}
			else
			{
				access = NetworkInformationAccess.None;
			}
		}

		internal NetworkInformationPermission(bool unrestricted)
		{
			if (unrestricted)
			{
				access = NetworkInformationAccess.Read | NetworkInformationAccess.Ping;
				unrestricted = true;
			}
			else
			{
				access = NetworkInformationAccess.None;
			}
		}

		public NetworkInformationPermission(NetworkInformationAccess access)
		{
			this.access = access;
		}

		public void AddPermission(NetworkInformationAccess access)
		{
			this.access |= access;
		}

		public bool IsUnrestricted()
		{
			return unrestricted;
		}

		public override IPermission Copy()
		{
			if (unrestricted)
			{
				return new NetworkInformationPermission(unrestricted: true);
			}
			return new NetworkInformationPermission(access);
		}

		public override IPermission Union(IPermission target)
		{
			if (target == null)
			{
				return Copy();
			}
			NetworkInformationPermission networkInformationPermission = target as NetworkInformationPermission;
			if (networkInformationPermission == null)
			{
				throw new ArgumentException(SR.GetString("net_perm_target"), "target");
			}
			if (unrestricted || networkInformationPermission.IsUnrestricted())
			{
				return new NetworkInformationPermission(unrestricted: true);
			}
			return new NetworkInformationPermission(access | networkInformationPermission.access);
		}

		public override IPermission Intersect(IPermission target)
		{
			if (target == null)
			{
				return null;
			}
			NetworkInformationPermission networkInformationPermission = target as NetworkInformationPermission;
			if (networkInformationPermission == null)
			{
				throw new ArgumentException(SR.GetString("net_perm_target"), "target");
			}
			if (unrestricted && networkInformationPermission.IsUnrestricted())
			{
				return new NetworkInformationPermission(unrestricted: true);
			}
			return new NetworkInformationPermission(access & networkInformationPermission.access);
		}

		public override bool IsSubsetOf(IPermission target)
		{
			if (target == null)
			{
				return access == NetworkInformationAccess.None;
			}
			NetworkInformationPermission networkInformationPermission = target as NetworkInformationPermission;
			if (networkInformationPermission == null)
			{
				throw new ArgumentException(SR.GetString("net_perm_target"), "target");
			}
			if (unrestricted && !networkInformationPermission.IsUnrestricted())
			{
				return false;
			}
			if ((access & networkInformationPermission.access) == access)
			{
				return true;
			}
			return false;
		}

		public override void FromXml(SecurityElement securityElement)
		{
			access = NetworkInformationAccess.None;
			if (securityElement == null)
			{
				throw new ArgumentNullException("securityElement");
			}
			if (!securityElement.Tag.Equals("IPermission"))
			{
				throw new ArgumentException(SR.GetString("net_not_ipermission"), "securityElement");
			}
			string text = securityElement.Attribute("class");
			if (text == null)
			{
				throw new ArgumentException(SR.GetString("net_no_classname"), "securityElement");
			}
			if (text.IndexOf(GetType().FullName) < 0)
			{
				throw new ArgumentException(SR.GetString("net_no_typename"), "securityElement");
			}
			string text2 = securityElement.Attribute("Unrestricted");
			if (text2 != null && string.Compare(text2, "true", StringComparison.OrdinalIgnoreCase) == 0)
			{
				access = NetworkInformationAccess.Read | NetworkInformationAccess.Ping;
				unrestricted = true;
			}
			else
			{
				if (securityElement.Children == null)
				{
					return;
				}
				foreach (SecurityElement child in securityElement.Children)
				{
					text2 = child.Attribute("Access");
					if (string.Compare(text2, "Read", StringComparison.OrdinalIgnoreCase) == 0)
					{
						access |= NetworkInformationAccess.Read;
					}
					else if (string.Compare(text2, "Ping", StringComparison.OrdinalIgnoreCase) == 0)
					{
						access |= NetworkInformationAccess.Ping;
					}
				}
			}
		}

		public override SecurityElement ToXml()
		{
			SecurityElement securityElement = new SecurityElement("IPermission");
			securityElement.AddAttribute("class", GetType().FullName + ", " + GetType().Module.Assembly.FullName.Replace('"', '\''));
			securityElement.AddAttribute("version", "1");
			if (unrestricted)
			{
				securityElement.AddAttribute("Unrestricted", "true");
				return securityElement;
			}
			if ((access & NetworkInformationAccess.Read) > NetworkInformationAccess.None)
			{
				SecurityElement securityElement2 = new SecurityElement("NetworkInformationAccess");
				securityElement2.AddAttribute("Access", "Read");
				securityElement.AddChild(securityElement2);
			}
			if ((access & NetworkInformationAccess.Ping) > NetworkInformationAccess.None)
			{
				SecurityElement securityElement3 = new SecurityElement("NetworkInformationAccess");
				securityElement3.AddAttribute("Access", "Ping");
				securityElement.AddChild(securityElement3);
			}
			return securityElement;
		}
	}
}
