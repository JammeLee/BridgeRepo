using System.Collections;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security.Util;

namespace System.Security.Permissions
{
	[Serializable]
	[ComVisible(true)]
	public sealed class SiteIdentityPermission : CodeAccessPermission, IBuiltInPermission
	{
		[OptionalField(VersionAdded = 2)]
		private bool m_unrestricted;

		[OptionalField(VersionAdded = 2)]
		private SiteString[] m_sites;

		[OptionalField(VersionAdded = 2)]
		private string m_serializedPermission;

		private SiteString m_site;

		public string Site
		{
			get
			{
				if (m_sites == null)
				{
					return "";
				}
				if (m_sites.Length == 1)
				{
					return m_sites[0].ToString();
				}
				throw new NotSupportedException(Environment.GetResourceString("NotSupported_AmbiguousIdentity"));
			}
			set
			{
				m_unrestricted = false;
				m_sites = new SiteString[1];
				m_sites[0] = new SiteString(value);
			}
		}

		[OnDeserialized]
		private void OnDeserialized(StreamingContext ctx)
		{
			if (m_serializedPermission != null)
			{
				FromXml(SecurityElement.FromString(m_serializedPermission));
				m_serializedPermission = null;
			}
			else if (m_site != null)
			{
				m_unrestricted = false;
				m_sites = new SiteString[1];
				m_sites[0] = m_site;
				m_site = null;
			}
		}

		[OnSerializing]
		private void OnSerializing(StreamingContext ctx)
		{
			if (((uint)ctx.State & 0xFFFFFF3Fu) != 0)
			{
				m_serializedPermission = ToXml().ToString();
				if (m_sites != null && m_sites.Length == 1)
				{
					m_site = m_sites[0];
				}
			}
		}

		[OnSerialized]
		private void OnSerialized(StreamingContext ctx)
		{
			if (((uint)ctx.State & 0xFFFFFF3Fu) != 0)
			{
				m_serializedPermission = null;
				m_site = null;
			}
		}

		public SiteIdentityPermission(PermissionState state)
		{
			switch (state)
			{
			case PermissionState.Unrestricted:
				if (CodeAccessSecurityEngine.DoesFullTrustMeanFullTrust())
				{
					m_unrestricted = true;
					break;
				}
				throw new ArgumentException(Environment.GetResourceString("Argument_UnrestrictedIdentityPermission"));
			case PermissionState.None:
				m_unrestricted = false;
				break;
			default:
				throw new ArgumentException(Environment.GetResourceString("Argument_InvalidPermissionState"));
			}
		}

		public SiteIdentityPermission(string site)
		{
			Site = site;
		}

		public override IPermission Copy()
		{
			SiteIdentityPermission siteIdentityPermission = new SiteIdentityPermission(PermissionState.None);
			siteIdentityPermission.m_unrestricted = m_unrestricted;
			if (m_sites != null)
			{
				siteIdentityPermission.m_sites = new SiteString[m_sites.Length];
				for (int i = 0; i < m_sites.Length; i++)
				{
					siteIdentityPermission.m_sites[i] = m_sites[i].Copy();
				}
			}
			return siteIdentityPermission;
		}

		public override bool IsSubsetOf(IPermission target)
		{
			if (target == null)
			{
				if (m_unrestricted)
				{
					return false;
				}
				if (m_sites == null)
				{
					return true;
				}
				if (m_sites.Length == 0)
				{
					return true;
				}
				return false;
			}
			SiteIdentityPermission siteIdentityPermission = target as SiteIdentityPermission;
			if (siteIdentityPermission == null)
			{
				throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Argument_WrongType"), GetType().FullName));
			}
			if (siteIdentityPermission.m_unrestricted)
			{
				return true;
			}
			if (m_unrestricted)
			{
				return false;
			}
			if (m_sites != null)
			{
				SiteString[] sites = m_sites;
				foreach (SiteString siteString in sites)
				{
					bool flag = false;
					if (siteIdentityPermission.m_sites != null)
					{
						SiteString[] sites2 = siteIdentityPermission.m_sites;
						foreach (SiteString operand in sites2)
						{
							if (siteString.IsSubsetOf(operand))
							{
								flag = true;
								break;
							}
						}
					}
					if (!flag)
					{
						return false;
					}
				}
			}
			return true;
		}

		public override IPermission Intersect(IPermission target)
		{
			if (target == null)
			{
				return null;
			}
			SiteIdentityPermission siteIdentityPermission = target as SiteIdentityPermission;
			if (siteIdentityPermission == null)
			{
				throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Argument_WrongType"), GetType().FullName));
			}
			if (m_unrestricted && siteIdentityPermission.m_unrestricted)
			{
				SiteIdentityPermission siteIdentityPermission2 = new SiteIdentityPermission(PermissionState.None);
				siteIdentityPermission2.m_unrestricted = true;
				return siteIdentityPermission2;
			}
			if (m_unrestricted)
			{
				return siteIdentityPermission.Copy();
			}
			if (siteIdentityPermission.m_unrestricted)
			{
				return Copy();
			}
			if (m_sites == null || siteIdentityPermission.m_sites == null || m_sites.Length == 0 || siteIdentityPermission.m_sites.Length == 0)
			{
				return null;
			}
			ArrayList arrayList = new ArrayList();
			SiteString[] sites = m_sites;
			foreach (SiteString siteString in sites)
			{
				SiteString[] sites2 = siteIdentityPermission.m_sites;
				foreach (SiteString operand in sites2)
				{
					SiteString siteString2 = siteString.Intersect(operand);
					if (siteString2 != null)
					{
						arrayList.Add(siteString2);
					}
				}
			}
			if (arrayList.Count == 0)
			{
				return null;
			}
			SiteIdentityPermission siteIdentityPermission3 = new SiteIdentityPermission(PermissionState.None);
			siteIdentityPermission3.m_sites = (SiteString[])arrayList.ToArray(typeof(SiteString));
			return siteIdentityPermission3;
		}

		public override IPermission Union(IPermission target)
		{
			if (target == null)
			{
				if ((m_sites == null || m_sites.Length == 0) && !m_unrestricted)
				{
					return null;
				}
				return Copy();
			}
			SiteIdentityPermission siteIdentityPermission = target as SiteIdentityPermission;
			if (siteIdentityPermission == null)
			{
				throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Argument_WrongType"), GetType().FullName));
			}
			if (m_unrestricted || siteIdentityPermission.m_unrestricted)
			{
				SiteIdentityPermission siteIdentityPermission2 = new SiteIdentityPermission(PermissionState.None);
				siteIdentityPermission2.m_unrestricted = true;
				return siteIdentityPermission2;
			}
			if (m_sites == null || m_sites.Length == 0)
			{
				if (siteIdentityPermission.m_sites == null || siteIdentityPermission.m_sites.Length == 0)
				{
					return null;
				}
				return siteIdentityPermission.Copy();
			}
			if (siteIdentityPermission.m_sites == null || siteIdentityPermission.m_sites.Length == 0)
			{
				return Copy();
			}
			ArrayList arrayList = new ArrayList();
			SiteString[] sites = m_sites;
			foreach (SiteString value in sites)
			{
				arrayList.Add(value);
			}
			SiteString[] sites2 = siteIdentityPermission.m_sites;
			foreach (SiteString siteString in sites2)
			{
				bool flag = false;
				foreach (SiteString item in arrayList)
				{
					if (siteString.Equals(item))
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					arrayList.Add(siteString);
				}
			}
			SiteIdentityPermission siteIdentityPermission3 = new SiteIdentityPermission(PermissionState.None);
			siteIdentityPermission3.m_sites = (SiteString[])arrayList.ToArray(typeof(SiteString));
			return siteIdentityPermission3;
		}

		public override void FromXml(SecurityElement esd)
		{
			m_unrestricted = false;
			m_sites = null;
			CodeAccessPermission.ValidateElement(esd, this);
			string text = esd.Attribute("Unrestricted");
			if (text != null && string.Compare(text, "true", StringComparison.OrdinalIgnoreCase) == 0)
			{
				m_unrestricted = true;
				return;
			}
			string text2 = esd.Attribute("Site");
			ArrayList arrayList = new ArrayList();
			if (text2 != null)
			{
				arrayList.Add(new SiteString(text2));
			}
			ArrayList children = esd.Children;
			if (children != null)
			{
				foreach (SecurityElement item in children)
				{
					text2 = item.Attribute("Site");
					if (text2 != null)
					{
						arrayList.Add(new SiteString(text2));
					}
				}
			}
			if (arrayList.Count != 0)
			{
				m_sites = (SiteString[])arrayList.ToArray(typeof(SiteString));
			}
		}

		public override SecurityElement ToXml()
		{
			SecurityElement securityElement = CodeAccessPermission.CreatePermissionElement(this, "System.Security.Permissions.SiteIdentityPermission");
			if (m_unrestricted)
			{
				securityElement.AddAttribute("Unrestricted", "true");
			}
			else if (m_sites != null)
			{
				if (m_sites.Length == 1)
				{
					securityElement.AddAttribute("Site", m_sites[0].ToString());
				}
				else
				{
					for (int i = 0; i < m_sites.Length; i++)
					{
						SecurityElement securityElement2 = new SecurityElement("Site");
						securityElement2.AddAttribute("Site", m_sites[i].ToString());
						securityElement.AddChild(securityElement2);
					}
				}
			}
			return securityElement;
		}

		int IBuiltInPermission.GetTokenIndex()
		{
			return GetTokenIndex();
		}

		internal static int GetTokenIndex()
		{
			return 11;
		}
	}
}
