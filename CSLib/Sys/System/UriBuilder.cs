using System.Globalization;
using System.Net;
using System.Text;
using System.Threading;

namespace System
{
	public class UriBuilder
	{
		private bool m_changed = true;

		private string m_fragment = string.Empty;

		private string m_host = "localhost";

		private string m_password = string.Empty;

		private string m_path = "/";

		private int m_port = -1;

		private string m_query = string.Empty;

		private string m_scheme = "http";

		private string m_schemeDelimiter = Uri.SchemeDelimiter;

		private Uri m_uri;

		private string m_username = string.Empty;

		private string Extra
		{
			set
			{
				if (value == null)
				{
					value = string.Empty;
				}
				if (value.Length > 0)
				{
					if (value[0] == '#')
					{
						Fragment = value.Substring(1);
						return;
					}
					if (value[0] != '?')
					{
						throw new ArgumentException("value");
					}
					int num = value.IndexOf('#');
					if (num == -1)
					{
						num = value.Length;
					}
					else
					{
						Fragment = value.Substring(num + 1);
					}
					Query = value.Substring(1, num - 1);
				}
				else
				{
					Fragment = string.Empty;
					Query = string.Empty;
				}
			}
		}

		public string Fragment
		{
			get
			{
				return m_fragment;
			}
			set
			{
				if (value == null)
				{
					value = string.Empty;
				}
				if (value.Length > 0)
				{
					value = '#' + value;
				}
				m_fragment = value;
				m_changed = true;
			}
		}

		public string Host
		{
			get
			{
				return m_host;
			}
			set
			{
				if (value == null)
				{
					value = string.Empty;
				}
				m_host = value;
				if (m_host.IndexOf(':') >= 0 && m_host[0] != '[')
				{
					m_host = "[" + m_host + "]";
				}
				m_changed = true;
			}
		}

		public string Password
		{
			get
			{
				return m_password;
			}
			set
			{
				if (value == null)
				{
					value = string.Empty;
				}
				m_password = value;
			}
		}

		public string Path
		{
			get
			{
				return m_path;
			}
			set
			{
				if (value == null || value.Length == 0)
				{
					value = "/";
				}
				m_path = Uri.InternalEscapeString(ConvertSlashes(value));
				m_changed = true;
			}
		}

		public int Port
		{
			get
			{
				return m_port;
			}
			set
			{
				if (value < -1 || value > 65535)
				{
					throw new ArgumentOutOfRangeException("value");
				}
				m_port = value;
				m_changed = true;
			}
		}

		public string Query
		{
			get
			{
				return m_query;
			}
			set
			{
				if (value == null)
				{
					value = string.Empty;
				}
				if (value.Length > 0)
				{
					value = '?' + value;
				}
				m_query = value;
				m_changed = true;
			}
		}

		public string Scheme
		{
			get
			{
				return m_scheme;
			}
			set
			{
				if (value == null)
				{
					value = string.Empty;
				}
				int num = value.IndexOf(':');
				if (num != -1)
				{
					value = value.Substring(0, num);
				}
				if (value.Length != 0)
				{
					if (!Uri.CheckSchemeName(value))
					{
						throw new ArgumentException("value");
					}
					value = value.ToLower(CultureInfo.InvariantCulture);
				}
				m_scheme = value;
				m_changed = true;
			}
		}

		public Uri Uri
		{
			get
			{
				if (m_changed)
				{
					m_uri = new Uri(ToString());
					SetFieldsFromUri(m_uri);
					m_changed = false;
				}
				return m_uri;
			}
		}

		public string UserName
		{
			get
			{
				return m_username;
			}
			set
			{
				if (value == null)
				{
					value = string.Empty;
				}
				m_username = value;
			}
		}

		public UriBuilder()
		{
		}

		public UriBuilder(string uri)
		{
			Uri uri2 = new Uri(uri, UriKind.RelativeOrAbsolute);
			if (uri2.IsAbsoluteUri)
			{
				Init(uri2);
				return;
			}
			uri = Uri.UriSchemeHttp + Uri.SchemeDelimiter + uri;
			Init(new Uri(uri));
		}

		public UriBuilder(Uri uri)
		{
			Init(uri);
		}

		private void Init(Uri uri)
		{
			m_fragment = uri.Fragment;
			m_query = uri.Query;
			m_host = uri.Host;
			m_path = uri.AbsolutePath;
			m_port = uri.Port;
			m_scheme = uri.Scheme;
			m_schemeDelimiter = (uri.HasAuthority ? Uri.SchemeDelimiter : ":");
			string userInfo = uri.UserInfo;
			if (!ValidationHelper.IsBlankString(userInfo))
			{
				int num = userInfo.IndexOf(':');
				if (num != -1)
				{
					m_password = userInfo.Substring(num + 1);
					m_username = userInfo.Substring(0, num);
				}
				else
				{
					m_username = userInfo;
				}
			}
			SetFieldsFromUri(uri);
		}

		public UriBuilder(string schemeName, string hostName)
		{
			Scheme = schemeName;
			Host = hostName;
		}

		public UriBuilder(string scheme, string host, int portNumber)
			: this(scheme, host)
		{
			Port = portNumber;
		}

		public UriBuilder(string scheme, string host, int port, string pathValue)
			: this(scheme, host, port)
		{
			Path = pathValue;
		}

		public UriBuilder(string scheme, string host, int port, string path, string extraValue)
			: this(scheme, host, port, path)
		{
			try
			{
				Extra = extraValue;
			}
			catch (Exception ex)
			{
				if (ex is ThreadAbortException || ex is StackOverflowException || ex is OutOfMemoryException)
				{
					throw;
				}
				throw new ArgumentException("extraValue");
			}
		}

		private string ConvertSlashes(string path)
		{
			StringBuilder stringBuilder = new StringBuilder(path.Length);
			for (int i = 0; i < path.Length; i++)
			{
				char c = path[i];
				if (c == '\\')
				{
					c = '/';
				}
				stringBuilder.Append(c);
			}
			return stringBuilder.ToString();
		}

		public override bool Equals(object rparam)
		{
			if (rparam == null)
			{
				return false;
			}
			return Uri.Equals(rparam.ToString());
		}

		public override int GetHashCode()
		{
			return Uri.GetHashCode();
		}

		private void SetFieldsFromUri(Uri uri)
		{
			m_fragment = uri.Fragment;
			m_query = uri.Query;
			m_host = uri.Host;
			m_path = uri.AbsolutePath;
			m_port = uri.Port;
			m_scheme = uri.Scheme;
			m_schemeDelimiter = (uri.HasAuthority ? Uri.SchemeDelimiter : ":");
			string userInfo = uri.UserInfo;
			if (userInfo.Length > 0)
			{
				int num = userInfo.IndexOf(':');
				if (num != -1)
				{
					m_password = userInfo.Substring(num + 1);
					m_username = userInfo.Substring(0, num);
				}
				else
				{
					m_username = userInfo;
				}
			}
		}

		public override string ToString()
		{
			if (m_username.Length == 0 && m_password.Length > 0)
			{
				throw new UriFormatException(SR.GetString("net_uri_BadUserPassword"));
			}
			if (m_scheme.Length != 0)
			{
				UriParser syntax = UriParser.GetSyntax(m_scheme);
				if (syntax != null)
				{
					m_schemeDelimiter = ((syntax.InFact(UriSyntaxFlags.MustHaveAuthority) || (m_host.Length != 0 && syntax.NotAny(UriSyntaxFlags.MailToLikeUri) && syntax.InFact(UriSyntaxFlags.OptionalAuthority))) ? Uri.SchemeDelimiter : ":");
				}
				else
				{
					m_schemeDelimiter = ((m_host.Length != 0) ? Uri.SchemeDelimiter : ":");
				}
			}
			string text = ((m_scheme.Length != 0) ? (m_scheme + m_schemeDelimiter) : string.Empty);
			return text + m_username + ((m_password.Length > 0) ? (":" + m_password) : string.Empty) + ((m_username.Length > 0) ? "@" : string.Empty) + m_host + ((m_port != -1 && m_host.Length > 0) ? (":" + m_port) : string.Empty) + ((m_host.Length > 0 && m_path.Length != 0 && m_path[0] != '/') ? "/" : string.Empty) + m_path + m_query + m_fragment;
		}
	}
}
