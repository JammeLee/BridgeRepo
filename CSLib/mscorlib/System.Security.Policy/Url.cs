using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Security.Util;

namespace System.Security.Policy
{
	[Serializable]
	[ComVisible(true)]
	public sealed class Url : IIdentityPermissionFactory, IBuiltInEvidence
	{
		private URLString m_url;

		public string Value
		{
			get
			{
				if (m_url == null)
				{
					return null;
				}
				return m_url.ToString();
			}
		}

		internal Url()
		{
			m_url = null;
		}

		internal Url(SerializationInfo info, StreamingContext context)
		{
			m_url = new URLString((string)info.GetValue("Url", typeof(string)));
		}

		internal Url(string name, bool parsed)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			m_url = new URLString(name, parsed);
		}

		public Url(string name)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			m_url = new URLString(name);
		}

		internal URLString GetURLString()
		{
			return m_url;
		}

		public IPermission CreateIdentityPermission(Evidence evidence)
		{
			return new UrlIdentityPermission(m_url);
		}

		public override bool Equals(object o)
		{
			if (o == null)
			{
				return false;
			}
			if (o is Url)
			{
				Url url = (Url)o;
				if (m_url == null)
				{
					return url.m_url == null;
				}
				if (url.m_url == null)
				{
					return false;
				}
				return m_url.Equals(url.m_url);
			}
			return false;
		}

		public override int GetHashCode()
		{
			if (m_url == null)
			{
				return 0;
			}
			return m_url.GetHashCode();
		}

		public object Copy()
		{
			Url url = new Url();
			url.m_url = m_url;
			return url;
		}

		internal SecurityElement ToXml()
		{
			SecurityElement securityElement = new SecurityElement("System.Security.Policy.Url");
			securityElement.AddAttribute("version", "1");
			if (m_url != null)
			{
				securityElement.AddChild(new SecurityElement("Url", m_url.ToString()));
			}
			return securityElement;
		}

		public override string ToString()
		{
			return ToXml().ToString();
		}

		int IBuiltInEvidence.OutputToBuffer(char[] buffer, int position, bool verbose)
		{
			buffer[position++] = '\u0004';
			string value = Value;
			int length = value.Length;
			if (verbose)
			{
				BuiltInEvidenceHelper.CopyIntToCharArray(length, buffer, position);
				position += 2;
			}
			value.CopyTo(0, buffer, position, length);
			return length + position;
		}

		int IBuiltInEvidence.GetRequiredSize(bool verbose)
		{
			if (verbose)
			{
				return Value.Length + 3;
			}
			return Value.Length + 1;
		}

		int IBuiltInEvidence.InitFromBuffer(char[] buffer, int position)
		{
			int intFromCharArray = BuiltInEvidenceHelper.GetIntFromCharArray(buffer, position);
			position += 2;
			m_url = new URLString(new string(buffer, position, intFromCharArray));
			return position + intFromCharArray;
		}

		internal object Normalize()
		{
			return m_url.NormalizeUrl();
		}
	}
}
