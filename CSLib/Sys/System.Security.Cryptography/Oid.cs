using System.Security.Cryptography.X509Certificates;

namespace System.Security.Cryptography
{
	public sealed class Oid
	{
		private const string RsaDataOid = "1.2.840.113549.1.7.1";

		private string m_value;

		private string m_friendlyName;

		private OidGroup m_group;

		public string Value
		{
			get
			{
				return m_value;
			}
			set
			{
				m_value = value;
			}
		}

		public string FriendlyName
		{
			get
			{
				if (m_friendlyName == null && m_value != null)
				{
					m_friendlyName = System.Security.Cryptography.X509Certificates.X509Utils.FindOidInfo(1u, m_value, m_group);
				}
				return m_friendlyName;
			}
			set
			{
				m_friendlyName = value;
				if (m_friendlyName != null)
				{
					string text = System.Security.Cryptography.X509Certificates.X509Utils.FindOidInfo(2u, m_friendlyName, m_group);
					if (text != null)
					{
						m_value = text;
					}
				}
			}
		}

		public Oid()
		{
		}

		public Oid(string oid)
			: this(oid, OidGroup.AllGroups, lookupFriendlyName: true)
		{
		}

		internal Oid(string oid, OidGroup group, bool lookupFriendlyName)
		{
			if (oid != null && string.Equals(oid, "1.2.840.113549.1.7.1", StringComparison.Ordinal))
			{
				Value = oid;
			}
			else if (lookupFriendlyName)
			{
				string text = System.Security.Cryptography.X509Certificates.X509Utils.FindOidInfo(2u, oid, group);
				if (text == null)
				{
					text = oid;
				}
				Value = text;
			}
			else
			{
				Value = oid;
			}
			m_group = group;
		}

		public Oid(string value, string friendlyName)
		{
			m_value = value;
			m_friendlyName = friendlyName;
		}

		public Oid(Oid oid)
		{
			if (oid == null)
			{
				throw new ArgumentNullException("oid");
			}
			m_value = oid.m_value;
			m_friendlyName = oid.m_friendlyName;
			m_group = oid.m_group;
		}
	}
}
