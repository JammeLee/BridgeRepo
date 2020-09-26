using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace System.Security.Policy
{
	[Serializable]
	[ComVisible(true)]
	public sealed class Zone : IIdentityPermissionFactory, IBuiltInEvidence
	{
		[OptionalField(VersionAdded = 2)]
		private string m_url;

		private SecurityZone m_zone;

		private static readonly string[] s_names = new string[6]
		{
			"MyComputer",
			"Intranet",
			"Trusted",
			"Internet",
			"Untrusted",
			"NoZone"
		};

		public SecurityZone SecurityZone
		{
			get
			{
				if (m_url != null)
				{
					m_zone = _CreateFromUrl(m_url);
				}
				return m_zone;
			}
		}

		internal Zone()
		{
			m_url = null;
			m_zone = SecurityZone.NoZone;
		}

		public Zone(SecurityZone zone)
		{
			if (zone < SecurityZone.NoZone || zone > SecurityZone.Untrusted)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_IllegalZone"));
			}
			m_url = null;
			m_zone = zone;
		}

		private Zone(string url)
		{
			m_url = url;
			m_zone = SecurityZone.NoZone;
		}

		public static Zone CreateFromUrl(string url)
		{
			if (url == null)
			{
				throw new ArgumentNullException("url");
			}
			return new Zone(url);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern SecurityZone _CreateFromUrl(string url);

		public IPermission CreateIdentityPermission(Evidence evidence)
		{
			return new ZoneIdentityPermission(SecurityZone);
		}

		public override bool Equals(object o)
		{
			if (o is Zone)
			{
				Zone zone = (Zone)o;
				return SecurityZone == zone.SecurityZone;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return (int)SecurityZone;
		}

		public object Copy()
		{
			Zone zone = new Zone();
			zone.m_zone = m_zone;
			zone.m_url = m_url;
			return zone;
		}

		internal SecurityElement ToXml()
		{
			SecurityElement securityElement = new SecurityElement("System.Security.Policy.Zone");
			securityElement.AddAttribute("version", "1");
			if (SecurityZone != SecurityZone.NoZone)
			{
				securityElement.AddChild(new SecurityElement("Zone", s_names[(int)SecurityZone]));
			}
			else
			{
				securityElement.AddChild(new SecurityElement("Zone", s_names[s_names.Length - 1]));
			}
			return securityElement;
		}

		int IBuiltInEvidence.OutputToBuffer(char[] buffer, int position, bool verbose)
		{
			buffer[position] = '\u0003';
			BuiltInEvidenceHelper.CopyIntToCharArray((int)SecurityZone, buffer, position + 1);
			return position + 3;
		}

		int IBuiltInEvidence.GetRequiredSize(bool verbose)
		{
			return 3;
		}

		int IBuiltInEvidence.InitFromBuffer(char[] buffer, int position)
		{
			m_url = null;
			m_zone = (SecurityZone)BuiltInEvidenceHelper.GetIntFromCharArray(buffer, position);
			return position + 2;
		}

		public override string ToString()
		{
			return ToXml().ToString();
		}

		internal object Normalize()
		{
			return s_names[(int)SecurityZone];
		}
	}
}
