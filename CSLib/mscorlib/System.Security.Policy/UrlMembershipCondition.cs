using System.Collections;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Security.Util;

namespace System.Security.Policy
{
	[Serializable]
	[ComVisible(true)]
	public sealed class UrlMembershipCondition : IConstantMembershipCondition, IReportMatchMembershipCondition, IMembershipCondition, ISecurityEncodable, ISecurityPolicyEncodable
	{
		private URLString m_url;

		private SecurityElement m_element;

		public string Url
		{
			get
			{
				if (m_url == null && m_element != null)
				{
					ParseURL();
				}
				return m_url.ToString();
			}
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException("value");
				}
				m_url = new URLString(value);
			}
		}

		internal UrlMembershipCondition()
		{
			m_url = null;
		}

		public UrlMembershipCondition(string url)
		{
			if (url == null)
			{
				throw new ArgumentNullException("url");
			}
			m_url = new URLString(url, parsed: false, doDeferredParsing: true);
		}

		public bool Check(Evidence evidence)
		{
			object usedEvidence = null;
			return ((IReportMatchMembershipCondition)this).Check(evidence, out usedEvidence);
		}

		bool IReportMatchMembershipCondition.Check(Evidence evidence, out object usedEvidence)
		{
			usedEvidence = null;
			if (evidence == null)
			{
				return false;
			}
			IEnumerator hostEnumerator = evidence.GetHostEnumerator();
			while (hostEnumerator.MoveNext())
			{
				if (hostEnumerator.Current is Url)
				{
					if (m_url == null && m_element != null)
					{
						ParseURL();
					}
					if (((Url)hostEnumerator.Current).GetURLString().IsSubsetOf(m_url))
					{
						usedEvidence = hostEnumerator.Current;
						return true;
					}
				}
			}
			return false;
		}

		public IMembershipCondition Copy()
		{
			if (m_url == null && m_element != null)
			{
				ParseURL();
			}
			UrlMembershipCondition urlMembershipCondition = new UrlMembershipCondition();
			urlMembershipCondition.m_url = new URLString(m_url.ToString());
			return urlMembershipCondition;
		}

		public SecurityElement ToXml()
		{
			return ToXml(null);
		}

		public void FromXml(SecurityElement e)
		{
			FromXml(e, null);
		}

		public SecurityElement ToXml(PolicyLevel level)
		{
			if (m_url == null && m_element != null)
			{
				ParseURL();
			}
			SecurityElement securityElement = new SecurityElement("IMembershipCondition");
			XMLUtil.AddClassAttribute(securityElement, GetType(), "System.Security.Policy.UrlMembershipCondition");
			securityElement.AddAttribute("version", "1");
			if (m_url != null)
			{
				securityElement.AddAttribute("Url", m_url.ToString());
			}
			return securityElement;
		}

		public void FromXml(SecurityElement e, PolicyLevel level)
		{
			if (e == null)
			{
				throw new ArgumentNullException("e");
			}
			if (!e.Tag.Equals("IMembershipCondition"))
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_MembershipConditionElement"));
			}
			lock (this)
			{
				m_element = e;
				m_url = null;
			}
		}

		private void ParseURL()
		{
			lock (this)
			{
				if (m_element != null)
				{
					string text = m_element.Attribute("Url");
					if (text == null)
					{
						throw new ArgumentException(Environment.GetResourceString("Argument_UrlCannotBeNull"));
					}
					m_url = new URLString(text);
					m_element = null;
				}
			}
		}

		public override bool Equals(object o)
		{
			UrlMembershipCondition urlMembershipCondition = o as UrlMembershipCondition;
			if (urlMembershipCondition != null)
			{
				if (m_url == null && m_element != null)
				{
					ParseURL();
				}
				if (urlMembershipCondition.m_url == null && urlMembershipCondition.m_element != null)
				{
					urlMembershipCondition.ParseURL();
				}
				if (object.Equals(m_url, urlMembershipCondition.m_url))
				{
					return true;
				}
			}
			return false;
		}

		public override int GetHashCode()
		{
			if (m_url == null && m_element != null)
			{
				ParseURL();
			}
			if (m_url != null)
			{
				return m_url.GetHashCode();
			}
			return typeof(UrlMembershipCondition).GetHashCode();
		}

		public override string ToString()
		{
			if (m_url == null && m_element != null)
			{
				ParseURL();
			}
			if (m_url != null)
			{
				return string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Url_ToStringArg"), m_url.ToString());
			}
			return Environment.GetResourceString("Url_ToString");
		}
	}
}
