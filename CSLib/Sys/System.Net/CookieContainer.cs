using System.Collections;
using System.Globalization;
using System.Net.NetworkInformation;
using System.Threading;

namespace System.Net
{
	[Serializable]
	public class CookieContainer
	{
		public const int DefaultCookieLimit = 300;

		public const int DefaultPerDomainCookieLimit = 20;

		public const int DefaultCookieLengthLimit = 4096;

		private static readonly HeaderVariantInfo[] HeaderInfo = new HeaderVariantInfo[2]
		{
			new HeaderVariantInfo("Set-Cookie", CookieVariant.Rfc2109),
			new HeaderVariantInfo("Set-Cookie2", CookieVariant.Rfc2965)
		};

		private Hashtable m_domainTable = new Hashtable();

		private int m_maxCookieSize = 4096;

		private int m_maxCookies = 300;

		private int m_maxCookiesPerDomain = 20;

		private int m_count;

		private string m_fqdnMyDomain = string.Empty;

		public int Capacity
		{
			get
			{
				return m_maxCookies;
			}
			set
			{
				if (value <= 0 || (value < m_maxCookiesPerDomain && m_maxCookiesPerDomain != int.MaxValue))
				{
					throw new ArgumentOutOfRangeException("value", SR.GetString("net_cookie_capacity_range", "Capacity", 0, m_maxCookiesPerDomain));
				}
				if (value < m_maxCookies)
				{
					m_maxCookies = value;
					AgeCookies(null);
				}
				m_maxCookies = value;
			}
		}

		public int Count => m_count;

		public int MaxCookieSize
		{
			get
			{
				return m_maxCookieSize;
			}
			set
			{
				if (value <= 0)
				{
					throw new ArgumentOutOfRangeException("value");
				}
				m_maxCookieSize = value;
			}
		}

		public int PerDomainCapacity
		{
			get
			{
				return m_maxCookiesPerDomain;
			}
			set
			{
				if (value <= 0 || (value > m_maxCookies && value != int.MaxValue))
				{
					throw new ArgumentOutOfRangeException("value");
				}
				if (value < m_maxCookiesPerDomain)
				{
					m_maxCookiesPerDomain = value;
					AgeCookies(null);
				}
				m_maxCookiesPerDomain = value;
			}
		}

		public CookieContainer()
		{
			string domainName = IPGlobalProperties.InternalGetIPGlobalProperties().DomainName;
			if (domainName != null && domainName.Length > 1)
			{
				m_fqdnMyDomain = '.' + domainName;
			}
		}

		public CookieContainer(int capacity)
			: this()
		{
			if (capacity <= 0)
			{
				throw new ArgumentException(SR.GetString("net_toosmall"), "Capacity");
			}
			m_maxCookies = capacity;
		}

		public CookieContainer(int capacity, int perDomainCapacity, int maxCookieSize)
			: this(capacity)
		{
			if (perDomainCapacity != int.MaxValue && (perDomainCapacity <= 0 || perDomainCapacity > capacity))
			{
				throw new ArgumentOutOfRangeException("perDomainCapacity", SR.GetString("net_cookie_capacity_range", "PerDomainCapacity", 0, capacity));
			}
			m_maxCookiesPerDomain = perDomainCapacity;
			if (maxCookieSize <= 0)
			{
				throw new ArgumentException(SR.GetString("net_toosmall"), "MaxCookieSize");
			}
			m_maxCookieSize = maxCookieSize;
		}

		public void Add(Cookie cookie)
		{
			if (cookie == null)
			{
				throw new ArgumentNullException("cookie");
			}
			if (cookie.Domain.Length == 0)
			{
				throw new ArgumentException(SR.GetString("net_emptystringcall"), "cookie.Domain");
			}
			Cookie cookie2 = new Cookie(cookie.Name, cookie.Value);
			cookie2.Version = cookie.Version;
			string str = (cookie.Secure ? Uri.UriSchemeHttps : Uri.UriSchemeHttp) + Uri.SchemeDelimiter;
			if (cookie.Domain[0] == '.')
			{
				str += "0";
				cookie2.Domain = cookie.Domain;
			}
			str += cookie.Domain;
			if (cookie.PortList != null)
			{
				cookie2.Port = cookie.Port;
				str = str + ":" + cookie.PortList[0];
			}
			cookie2.Path = ((cookie.Path.Length == 0) ? "/" : cookie.Path);
			str += cookie.Path;
			if (!Uri.TryCreate(str, UriKind.Absolute, out var result))
			{
				throw new CookieException(SR.GetString("net_cookie_attribute", "Domain", cookie.Domain));
			}
			cookie2.VerifySetDefaults(CookieVariant.Unknown, result, IsLocal(result.Host), m_fqdnMyDomain, set_default: true, isThrow: true);
			Add(cookie2, throwOnError: true);
		}

		private void AddRemoveDomain(string key, PathList value)
		{
			lock (this)
			{
				if (value == null)
				{
					m_domainTable.Remove(key);
				}
				else
				{
					m_domainTable[key] = value;
				}
			}
		}

		internal void Add(Cookie cookie, bool throwOnError)
		{
			if (cookie.Value.Length > m_maxCookieSize)
			{
				if (throwOnError)
				{
					throw new CookieException(SR.GetString("net_cookie_size", cookie.ToString(), m_maxCookieSize));
				}
				return;
			}
			try
			{
				PathList pathList = (PathList)m_domainTable[cookie.DomainKey];
				if (pathList == null)
				{
					pathList = new PathList();
					AddRemoveDomain(cookie.DomainKey, pathList);
				}
				int cookiesCount = pathList.GetCookiesCount();
				CookieCollection cookieCollection = (CookieCollection)pathList[cookie.Path];
				if (cookieCollection == null)
				{
					cookieCollection = new CookieCollection();
					pathList[cookie.Path] = cookieCollection;
				}
				if (cookie.Expired)
				{
					lock (cookieCollection)
					{
						int num = cookieCollection.IndexOf(cookie);
						if (num != -1)
						{
							cookieCollection.RemoveAt(num);
							m_count--;
						}
					}
				}
				else if ((cookiesCount < m_maxCookiesPerDomain || AgeCookies(cookie.DomainKey)) && (m_count < m_maxCookies || AgeCookies(null)))
				{
					lock (cookieCollection)
					{
						m_count += cookieCollection.InternalAdd(cookie, isStrict: true);
					}
				}
			}
			catch (Exception ex)
			{
				if (ex is ThreadAbortException || ex is StackOverflowException || ex is OutOfMemoryException)
				{
					throw;
				}
				if (throwOnError)
				{
					throw new CookieException(SR.GetString("net_container_add_cookie"), ex);
				}
			}
			catch
			{
				if (throwOnError)
				{
					throw new CookieException(SR.GetString("net_container_add_cookie"), new Exception(SR.GetString("net_nonClsCompliantException")));
				}
			}
		}

		private bool AgeCookies(string domain)
		{
			if (m_maxCookies == 0 || m_maxCookiesPerDomain == 0)
			{
				m_domainTable = new Hashtable();
				m_count = 0;
				return false;
			}
			int num = 0;
			DateTime dateTime = DateTime.MaxValue;
			CookieCollection cookieCollection = null;
			string text = null;
			int num2 = 0;
			int num3 = 0;
			float num4 = 1f;
			if (m_count > m_maxCookies)
			{
				num4 = (float)m_maxCookies / (float)m_count;
			}
			foreach (DictionaryEntry item in m_domainTable)
			{
				PathList pathList;
				if (domain == null)
				{
					text = (string)item.Key;
					pathList = (PathList)item.Value;
				}
				else
				{
					text = domain;
					pathList = (PathList)m_domainTable[domain];
				}
				num2 = 0;
				foreach (CookieCollection value in pathList.Values)
				{
					num3 = ExpireCollection(value);
					num += num3;
					m_count -= num3;
					num2 += value.Count;
					DateTime dateTime2;
					if (value.Count > 0 && (dateTime2 = value.TimeStamp(CookieCollection.Stamp.Check)) < dateTime)
					{
						cookieCollection = value;
						dateTime = dateTime2;
					}
				}
				int num5 = Math.Min((int)((float)num2 * num4), Math.Min(m_maxCookiesPerDomain, m_maxCookies) - 1);
				if (num2 > num5)
				{
					Array array = Array.CreateInstance(typeof(CookieCollection), pathList.Count);
					Array array2 = Array.CreateInstance(typeof(DateTime), pathList.Count);
					foreach (CookieCollection value2 in pathList.Values)
					{
						array2.SetValue(value2.TimeStamp(CookieCollection.Stamp.Check), num3);
						array.SetValue(value2, num3);
						num3++;
					}
					Array.Sort(array2, array);
					num3 = 0;
					for (int i = 0; i < pathList.Count; i++)
					{
						CookieCollection cookieCollection4 = (CookieCollection)array.GetValue(i);
						lock (cookieCollection4)
						{
							while (num2 > num5 && cookieCollection4.Count > 0)
							{
								cookieCollection4.RemoveAt(0);
								num2--;
								m_count--;
								num++;
							}
						}
						if (num2 <= num5)
						{
							break;
						}
					}
					if (num2 > num5 && domain != null)
					{
						return false;
					}
				}
				if (domain != null)
				{
					return true;
				}
			}
			if (num != 0)
			{
				return true;
			}
			if (dateTime == DateTime.MaxValue)
			{
				return false;
			}
			lock (cookieCollection)
			{
				while (m_count >= m_maxCookies && cookieCollection.Count > 0)
				{
					cookieCollection.RemoveAt(0);
					m_count--;
				}
			}
			return true;
		}

		private int ExpireCollection(CookieCollection cc)
		{
			int count = cc.Count;
			int num = count - 1;
			DateTime now = DateTime.Now;
			lock (cc)
			{
				while (num >= 0)
				{
					Cookie cookie = cc[num];
					if (cookie.Expires <= now && cookie.Expires != DateTime.MinValue)
					{
						cc.RemoveAt(num);
					}
					num--;
				}
			}
			return count - cc.Count;
		}

		public void Add(CookieCollection cookies)
		{
			if (cookies == null)
			{
				throw new ArgumentNullException("cookies");
			}
			foreach (Cookie cooky in cookies)
			{
				Add(cooky);
			}
		}

		internal bool IsLocal(string host)
		{
			int num = host.IndexOf('.');
			if (num == -1)
			{
				return true;
			}
			if (host == "127.0.0.1")
			{
				return true;
			}
			if (string.Compare(m_fqdnMyDomain, 0, host, num, m_fqdnMyDomain.Length, StringComparison.OrdinalIgnoreCase) == 0)
			{
				return true;
			}
			string[] array = host.Split('.');
			if (array != null && array.Length == 4 && array[0] == "127")
			{
				int i;
				for (i = 1; i < 4; i++)
				{
					switch (array[i].Length)
					{
					case 3:
						if (array[i][2] < '0' || array[i][2] > '9')
						{
							break;
						}
						goto case 2;
					case 2:
						if (array[i][1] < '0' || array[i][1] > '9')
						{
							break;
						}
						goto case 1;
					case 1:
						if (array[i][0] >= '0' && array[i][0] <= '9')
						{
							continue;
						}
						break;
					}
					break;
				}
				if (i == 4)
				{
					return true;
				}
			}
			return false;
		}

		public void Add(Uri uri, Cookie cookie)
		{
			if (uri == null)
			{
				throw new ArgumentNullException("uri");
			}
			if (cookie == null)
			{
				throw new ArgumentNullException("cookie");
			}
			cookie.VerifySetDefaults(CookieVariant.Unknown, uri, IsLocal(uri.Host), m_fqdnMyDomain, set_default: true, isThrow: true);
			Add(cookie, throwOnError: true);
		}

		public void Add(Uri uri, CookieCollection cookies)
		{
			if (uri == null)
			{
				throw new ArgumentNullException("uri");
			}
			if (cookies == null)
			{
				throw new ArgumentNullException("cookies");
			}
			bool isLocalDomain = IsLocal(uri.Host);
			foreach (Cookie cooky in cookies)
			{
				cooky.VerifySetDefaults(CookieVariant.Unknown, uri, isLocalDomain, m_fqdnMyDomain, set_default: true, isThrow: true);
				Add(cooky, throwOnError: true);
			}
		}

		internal CookieCollection CookieCutter(Uri uri, string headerName, string setCookieHeader, bool isThrow)
		{
			CookieCollection cookieCollection = new CookieCollection();
			CookieVariant variant = CookieVariant.Unknown;
			if (headerName == null)
			{
				variant = CookieVariant.Rfc2109;
			}
			else
			{
				for (int i = 0; i < HeaderInfo.Length; i++)
				{
					if (string.Compare(headerName, HeaderInfo[i].Name, StringComparison.OrdinalIgnoreCase) == 0)
					{
						variant = HeaderInfo[i].Variant;
					}
				}
			}
			bool isLocalDomain = IsLocal(uri.Host);
			try
			{
				CookieParser cookieParser = new CookieParser(setCookieHeader);
				while (true)
				{
					Cookie cookie = cookieParser.Get();
					if (cookie == null)
					{
						break;
					}
					if (ValidationHelper.IsBlankString(cookie.Name))
					{
						if (isThrow)
						{
							throw new CookieException(SR.GetString("net_cookie_format"));
						}
					}
					else if (cookie.VerifySetDefaults(variant, uri, isLocalDomain, m_fqdnMyDomain, set_default: true, isThrow))
					{
						cookieCollection.InternalAdd(cookie, isStrict: true);
					}
				}
			}
			catch (Exception ex)
			{
				if (ex is ThreadAbortException || ex is StackOverflowException || ex is OutOfMemoryException)
				{
					throw;
				}
				if (isThrow)
				{
					throw new CookieException(SR.GetString("net_cookie_parse_header", uri.AbsoluteUri), ex);
				}
			}
			catch
			{
				if (isThrow)
				{
					throw new CookieException(SR.GetString("net_cookie_parse_header", uri.AbsoluteUri), new Exception(SR.GetString("net_nonClsCompliantException")));
				}
			}
			foreach (Cookie item in cookieCollection)
			{
				Add(item, isThrow);
			}
			return cookieCollection;
		}

		public CookieCollection GetCookies(Uri uri)
		{
			if (uri == null)
			{
				throw new ArgumentNullException("uri");
			}
			return InternalGetCookies(uri);
		}

		internal CookieCollection InternalGetCookies(Uri uri)
		{
			bool isSecure = uri.Scheme == Uri.UriSchemeHttps;
			int port = uri.Port;
			CookieCollection cookieCollection = new CookieCollection();
			ArrayList arrayList = new ArrayList();
			int num = 0;
			string host = uri.Host;
			int num2 = host.IndexOf('.');
			if (num2 == -1)
			{
				arrayList.Add(host);
				if (m_fqdnMyDomain != null && m_fqdnMyDomain.Length != 0)
				{
					arrayList.Add(host + m_fqdnMyDomain);
					arrayList.Add(m_fqdnMyDomain);
					num = 3;
				}
				else
				{
					num = 1;
				}
			}
			else
			{
				arrayList.Add(host);
				arrayList.Add(host.Substring(num2));
				num = 2;
				if (host.Length > 2)
				{
					int num3 = host.LastIndexOf('.', host.Length - 2);
					if (num3 > 0)
					{
						num3 = host.LastIndexOf('.', num3 - 1);
					}
					if (num3 != -1)
					{
						while (num2 < num3 && (num2 = host.IndexOf('.', num2 + 1)) != -1)
						{
							arrayList.Add(host.Substring(num2));
						}
					}
				}
			}
			foreach (string item in arrayList)
			{
				bool flag = false;
				bool flag2 = false;
				PathList pathList = (PathList)m_domainTable[item];
				num--;
				if (pathList == null)
				{
					continue;
				}
				foreach (DictionaryEntry item2 in pathList)
				{
					string text = (string)item2.Key;
					if (uri.AbsolutePath.StartsWith(CookieParser.CheckQuoted(text)))
					{
						flag = true;
						CookieCollection cookieCollection2 = (CookieCollection)item2.Value;
						cookieCollection2.TimeStamp(CookieCollection.Stamp.Set);
						MergeUpdateCollections(cookieCollection, cookieCollection2, port, isSecure, num < 0);
						if (text == "/")
						{
							flag2 = true;
						}
					}
					else if (flag)
					{
						break;
					}
				}
				if (!flag2)
				{
					CookieCollection cookieCollection3 = (CookieCollection)pathList["/"];
					if (cookieCollection3 != null)
					{
						cookieCollection3.TimeStamp(CookieCollection.Stamp.Set);
						MergeUpdateCollections(cookieCollection, cookieCollection3, port, isSecure, num < 0);
					}
				}
				if (pathList.Count == 0)
				{
					AddRemoveDomain(item, null);
				}
			}
			return cookieCollection;
		}

		private void MergeUpdateCollections(CookieCollection destination, CookieCollection source, int port, bool isSecure, bool isPlainOnly)
		{
			lock (source)
			{
				for (int i = 0; i < source.Count; i++)
				{
					bool flag = false;
					Cookie cookie = source[i];
					if (cookie.Expired)
					{
						source.RemoveAt(i);
						m_count--;
						i--;
						continue;
					}
					if (!isPlainOnly || cookie.Variant == CookieVariant.Plain)
					{
						if (cookie.PortList != null)
						{
							int[] portList = cookie.PortList;
							foreach (int num in portList)
							{
								if (num == port)
								{
									flag = true;
									break;
								}
							}
						}
						else
						{
							flag = true;
						}
					}
					if (cookie.Secure && !isSecure)
					{
						flag = false;
					}
					if (flag)
					{
						destination.InternalAdd(cookie, isStrict: false);
					}
				}
			}
		}

		public string GetCookieHeader(Uri uri)
		{
			if (uri == null)
			{
				throw new ArgumentNullException("uri");
			}
			string optCookie;
			return GetCookieHeader(uri, out optCookie);
		}

		internal string GetCookieHeader(Uri uri, out string optCookie2)
		{
			CookieCollection cookieCollection = InternalGetCookies(uri);
			string text = string.Empty;
			string str = string.Empty;
			foreach (Cookie item in cookieCollection)
			{
				text = text + str + item.ToString();
				str = "; ";
			}
			optCookie2 = (cookieCollection.IsOtherVersionSeen ? ("$Version=" + 1.ToString(NumberFormatInfo.InvariantInfo)) : string.Empty);
			return text;
		}

		public void SetCookies(Uri uri, string cookieHeader)
		{
			if (uri == null)
			{
				throw new ArgumentNullException("uri");
			}
			if (cookieHeader == null)
			{
				throw new ArgumentNullException("cookieHeader");
			}
			CookieCutter(uri, null, cookieHeader, isThrow: true);
		}
	}
}
