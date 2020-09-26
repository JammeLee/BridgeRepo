using System.Globalization;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Security.Util;

namespace System.Security.Policy
{
	[Serializable]
	[ComVisible(true)]
	public sealed class Site : IIdentityPermissionFactory, IBuiltInEvidence
	{
		private SiteString m_name;

		public string Name
		{
			get
			{
				if (m_name != null)
				{
					return m_name.ToString();
				}
				return null;
			}
		}

		internal Site()
		{
			m_name = null;
		}

		public Site(string name)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			m_name = new SiteString(name);
		}

		internal Site(byte[] id, string name)
		{
			m_name = ParseSiteFromUrl(name);
		}

		public static Site CreateFromUrl(string url)
		{
			Site site = new Site();
			site.m_name = ParseSiteFromUrl(url);
			return site;
		}

		private static SiteString ParseSiteFromUrl(string name)
		{
			URLString uRLString = new URLString(name);
			if (string.Compare(uRLString.Scheme, "file", StringComparison.OrdinalIgnoreCase) == 0)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_InvalidSite"));
			}
			return new SiteString(new URLString(name).Host);
		}

		internal SiteString GetSiteString()
		{
			return m_name;
		}

		public IPermission CreateIdentityPermission(Evidence evidence)
		{
			return new SiteIdentityPermission(Name);
		}

		public override bool Equals(object o)
		{
			if (o is Site)
			{
				Site site = (Site)o;
				if (Name == null)
				{
					return site.Name == null;
				}
				return string.Compare(Name, site.Name, StringComparison.OrdinalIgnoreCase) == 0;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return Name?.GetHashCode() ?? 0;
		}

		public object Copy()
		{
			return new Site(Name);
		}

		internal SecurityElement ToXml()
		{
			SecurityElement securityElement = new SecurityElement("System.Security.Policy.Site");
			securityElement.AddAttribute("version", "1");
			if (m_name != null)
			{
				securityElement.AddChild(new SecurityElement("Name", m_name.ToString()));
			}
			return securityElement;
		}

		int IBuiltInEvidence.OutputToBuffer(char[] buffer, int position, bool verbose)
		{
			buffer[position++] = '\u0006';
			string name = Name;
			int length = name.Length;
			if (verbose)
			{
				BuiltInEvidenceHelper.CopyIntToCharArray(length, buffer, position);
				position += 2;
			}
			name.CopyTo(0, buffer, position, length);
			return length + position;
		}

		int IBuiltInEvidence.GetRequiredSize(bool verbose)
		{
			if (verbose)
			{
				return Name.Length + 3;
			}
			return Name.Length + 1;
		}

		int IBuiltInEvidence.InitFromBuffer(char[] buffer, int position)
		{
			int intFromCharArray = BuiltInEvidenceHelper.GetIntFromCharArray(buffer, position);
			position += 2;
			m_name = new SiteString(new string(buffer, position, intFromCharArray));
			return position + intFromCharArray;
		}

		public override string ToString()
		{
			return ToXml().ToString();
		}

		internal object Normalize()
		{
			return m_name.ToString().ToUpper(CultureInfo.InvariantCulture);
		}
	}
}
