using System.Collections;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security.Util;

namespace System.Security.Permissions
{
	[Serializable]
	[ComVisible(true)]
	public sealed class UrlIdentityPermission : CodeAccessPermission, IBuiltInPermission
	{
		[OptionalField(VersionAdded = 2)]
		private bool m_unrestricted;

		[OptionalField(VersionAdded = 2)]
		private URLString[] m_urls;

		[OptionalField(VersionAdded = 2)]
		private string m_serializedPermission;

		private URLString m_url;

		public string Url
		{
			get
			{
				if (m_urls == null)
				{
					return "";
				}
				if (m_urls.Length == 1)
				{
					return m_urls[0].ToString();
				}
				throw new NotSupportedException(Environment.GetResourceString("NotSupported_AmbiguousIdentity"));
			}
			set
			{
				m_unrestricted = false;
				if (value == null || value.Length == 0)
				{
					m_urls = null;
					return;
				}
				m_urls = new URLString[1];
				m_urls[0] = new URLString(value);
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
			else if (m_url != null)
			{
				m_unrestricted = false;
				m_urls = new URLString[1];
				m_urls[0] = m_url;
				m_url = null;
			}
		}

		[OnSerializing]
		private void OnSerializing(StreamingContext ctx)
		{
			if (((uint)ctx.State & 0xFFFFFF3Fu) != 0)
			{
				m_serializedPermission = ToXml().ToString();
				if (m_urls != null && m_urls.Length == 1)
				{
					m_url = m_urls[0];
				}
			}
		}

		[OnSerialized]
		private void OnSerialized(StreamingContext ctx)
		{
			if (((uint)ctx.State & 0xFFFFFF3Fu) != 0)
			{
				m_serializedPermission = null;
				m_url = null;
			}
		}

		public UrlIdentityPermission(PermissionState state)
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

		public UrlIdentityPermission(string site)
		{
			if (site == null)
			{
				throw new ArgumentNullException("site");
			}
			Url = site;
		}

		internal UrlIdentityPermission(URLString site)
		{
			m_unrestricted = false;
			m_urls = new URLString[1];
			m_urls[0] = site;
		}

		public override IPermission Copy()
		{
			UrlIdentityPermission urlIdentityPermission = new UrlIdentityPermission(PermissionState.None);
			urlIdentityPermission.m_unrestricted = m_unrestricted;
			if (m_urls != null)
			{
				urlIdentityPermission.m_urls = new URLString[m_urls.Length];
				for (int i = 0; i < m_urls.Length; i++)
				{
					urlIdentityPermission.m_urls[i] = (URLString)m_urls[i].Copy();
				}
			}
			return urlIdentityPermission;
		}

		public override bool IsSubsetOf(IPermission target)
		{
			if (target == null)
			{
				if (m_unrestricted)
				{
					return false;
				}
				if (m_urls == null)
				{
					return true;
				}
				if (m_urls.Length == 0)
				{
					return true;
				}
				return false;
			}
			UrlIdentityPermission urlIdentityPermission = target as UrlIdentityPermission;
			if (urlIdentityPermission == null)
			{
				throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Argument_WrongType"), GetType().FullName));
			}
			if (urlIdentityPermission.m_unrestricted)
			{
				return true;
			}
			if (m_unrestricted)
			{
				return false;
			}
			if (m_urls != null)
			{
				URLString[] urls = m_urls;
				foreach (URLString uRLString in urls)
				{
					bool flag = false;
					if (urlIdentityPermission.m_urls != null)
					{
						URLString[] urls2 = urlIdentityPermission.m_urls;
						foreach (URLString operand in urls2)
						{
							if (uRLString.IsSubsetOf(operand))
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
			UrlIdentityPermission urlIdentityPermission = target as UrlIdentityPermission;
			if (urlIdentityPermission == null)
			{
				throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Argument_WrongType"), GetType().FullName));
			}
			if (m_unrestricted && urlIdentityPermission.m_unrestricted)
			{
				UrlIdentityPermission urlIdentityPermission2 = new UrlIdentityPermission(PermissionState.None);
				urlIdentityPermission2.m_unrestricted = true;
				return urlIdentityPermission2;
			}
			if (m_unrestricted)
			{
				return urlIdentityPermission.Copy();
			}
			if (urlIdentityPermission.m_unrestricted)
			{
				return Copy();
			}
			if (m_urls == null || urlIdentityPermission.m_urls == null || m_urls.Length == 0 || urlIdentityPermission.m_urls.Length == 0)
			{
				return null;
			}
			ArrayList arrayList = new ArrayList();
			URLString[] urls = m_urls;
			foreach (URLString uRLString in urls)
			{
				URLString[] urls2 = urlIdentityPermission.m_urls;
				foreach (URLString operand in urls2)
				{
					URLString uRLString2 = (URLString)uRLString.Intersect(operand);
					if (uRLString2 != null)
					{
						arrayList.Add(uRLString2);
					}
				}
			}
			if (arrayList.Count == 0)
			{
				return null;
			}
			UrlIdentityPermission urlIdentityPermission3 = new UrlIdentityPermission(PermissionState.None);
			urlIdentityPermission3.m_urls = (URLString[])arrayList.ToArray(typeof(URLString));
			return urlIdentityPermission3;
		}

		public override IPermission Union(IPermission target)
		{
			if (target == null)
			{
				if ((m_urls == null || m_urls.Length == 0) && !m_unrestricted)
				{
					return null;
				}
				return Copy();
			}
			UrlIdentityPermission urlIdentityPermission = target as UrlIdentityPermission;
			if (urlIdentityPermission == null)
			{
				throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Argument_WrongType"), GetType().FullName));
			}
			if (m_unrestricted || urlIdentityPermission.m_unrestricted)
			{
				UrlIdentityPermission urlIdentityPermission2 = new UrlIdentityPermission(PermissionState.None);
				urlIdentityPermission2.m_unrestricted = true;
				return urlIdentityPermission2;
			}
			if (m_urls == null || m_urls.Length == 0)
			{
				if (urlIdentityPermission.m_urls == null || urlIdentityPermission.m_urls.Length == 0)
				{
					return null;
				}
				return urlIdentityPermission.Copy();
			}
			if (urlIdentityPermission.m_urls == null || urlIdentityPermission.m_urls.Length == 0)
			{
				return Copy();
			}
			ArrayList arrayList = new ArrayList();
			URLString[] urls = m_urls;
			foreach (URLString value in urls)
			{
				arrayList.Add(value);
			}
			URLString[] urls2 = urlIdentityPermission.m_urls;
			foreach (URLString uRLString in urls2)
			{
				bool flag = false;
				foreach (URLString item in arrayList)
				{
					if (uRLString.Equals(item))
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					arrayList.Add(uRLString);
				}
			}
			UrlIdentityPermission urlIdentityPermission3 = new UrlIdentityPermission(PermissionState.None);
			urlIdentityPermission3.m_urls = (URLString[])arrayList.ToArray(typeof(URLString));
			return urlIdentityPermission3;
		}

		public override void FromXml(SecurityElement esd)
		{
			m_unrestricted = false;
			m_urls = null;
			CodeAccessPermission.ValidateElement(esd, this);
			string text = esd.Attribute("Unrestricted");
			if (text != null && string.Compare(text, "true", StringComparison.OrdinalIgnoreCase) == 0)
			{
				m_unrestricted = true;
				return;
			}
			string text2 = esd.Attribute("Url");
			ArrayList arrayList = new ArrayList();
			if (text2 != null)
			{
				arrayList.Add(new URLString(text2, parsed: true));
			}
			ArrayList children = esd.Children;
			if (children != null)
			{
				foreach (SecurityElement item in children)
				{
					text2 = item.Attribute("Url");
					if (text2 != null)
					{
						arrayList.Add(new URLString(text2, parsed: true));
					}
				}
			}
			if (arrayList.Count != 0)
			{
				m_urls = (URLString[])arrayList.ToArray(typeof(URLString));
			}
		}

		public override SecurityElement ToXml()
		{
			SecurityElement securityElement = CodeAccessPermission.CreatePermissionElement(this, "System.Security.Permissions.UrlIdentityPermission");
			if (m_unrestricted)
			{
				securityElement.AddAttribute("Unrestricted", "true");
			}
			else if (m_urls != null)
			{
				if (m_urls.Length == 1)
				{
					securityElement.AddAttribute("Url", m_urls[0].ToString());
				}
				else
				{
					for (int i = 0; i < m_urls.Length; i++)
					{
						SecurityElement securityElement2 = new SecurityElement("Url");
						securityElement2.AddAttribute("Url", m_urls[i].ToString());
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
			return 13;
		}
	}
}
