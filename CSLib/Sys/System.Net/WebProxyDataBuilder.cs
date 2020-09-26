using System.Collections;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace System.Net
{
	internal abstract class WebProxyDataBuilder
	{
		private const string regexReserved = "#$()+.?[\\^{|";

		private static readonly char[] s_AddressListSplitChars = new char[2]
		{
			';',
			'='
		};

		private static readonly char[] s_BypassListDelimiter = new char[1]
		{
			';'
		};

		private WebProxyData m_Result;

		public WebProxyData Build()
		{
			m_Result = new WebProxyData();
			BuildInternal();
			return m_Result;
		}

		protected abstract void BuildInternal();

		protected void SetProxyAndBypassList(string addressString, string bypassListString)
		{
			Uri uri = null;
			Hashtable hashtable = null;
			if (addressString != null)
			{
				uri = ParseProxyUri(addressString, validate: true);
				if (uri == null)
				{
					hashtable = ParseProtocolProxies(addressString);
				}
				if ((uri != null || hashtable != null) && bypassListString != null)
				{
					bool bypassOnLocal = false;
					m_Result.bypassList = ParseBypassList(bypassListString, out bypassOnLocal);
					m_Result.bypassOnLocal = bypassOnLocal;
				}
			}
			if (hashtable != null)
			{
				uri = hashtable["http"] as Uri;
			}
			m_Result.proxyAddress = uri;
		}

		protected void SetAutoProxyUrl(string autoConfigUrl)
		{
			if (!string.IsNullOrEmpty(autoConfigUrl))
			{
				Uri result = null;
				if (Uri.TryCreate(autoConfigUrl, UriKind.Absolute, out result))
				{
					m_Result.scriptLocation = result;
				}
			}
		}

		protected void SetAutoDetectSettings(bool value)
		{
			m_Result.automaticallyDetectSettings = value;
		}

		private static Uri ParseProxyUri(string proxyString, bool validate)
		{
			if (validate)
			{
				if (proxyString.Length == 0)
				{
					return null;
				}
				if (proxyString.IndexOf('=') != -1)
				{
					return null;
				}
			}
			if (proxyString.IndexOf("://") == -1)
			{
				proxyString = "http://" + proxyString;
			}
			try
			{
				return new Uri(proxyString);
			}
			catch (UriFormatException ex)
			{
				if (Logging.On)
				{
					Logging.PrintError(Logging.Web, ex.Message);
				}
			}
			return null;
		}

		private static Hashtable ParseProtocolProxies(string proxyListString)
		{
			if (proxyListString.Length == 0)
			{
				return null;
			}
			string[] array = proxyListString.Split(s_AddressListSplitChars);
			bool flag = true;
			string key = null;
			Hashtable hashtable = new Hashtable(CaseInsensitiveAscii.StaticInstance);
			string[] array2 = array;
			foreach (string text in array2)
			{
				string text2 = text.Trim().ToLower(CultureInfo.InvariantCulture);
				if (flag)
				{
					key = text2;
				}
				else
				{
					hashtable[key] = ParseProxyUri(text2, validate: false);
				}
				flag = !flag;
			}
			if (hashtable.Count == 0)
			{
				return null;
			}
			return hashtable;
		}

		private static string BypassStringEscape(string rawString)
		{
			Regex regex = new Regex("^(?<scheme>.*://)?(?<host>[^:]*)(?<port>:[0-9]{1,5})?$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
			Match match = regex.Match(rawString);
			string rawString2;
			string rawString3;
			string rawString4;
			if (match.Success)
			{
				rawString2 = match.Groups["scheme"].Value;
				rawString3 = match.Groups["host"].Value;
				rawString4 = match.Groups["port"].Value;
			}
			else
			{
				rawString2 = string.Empty;
				rawString3 = rawString;
				rawString4 = string.Empty;
			}
			rawString2 = ConvertRegexReservedChars(rawString2);
			rawString3 = ConvertRegexReservedChars(rawString3);
			rawString4 = ConvertRegexReservedChars(rawString4);
			if (rawString2 == string.Empty)
			{
				rawString2 = "(?:.*://)?";
			}
			if (rawString4 == string.Empty)
			{
				rawString4 = "(?::[0-9]{1,5})?";
			}
			return "^" + rawString2 + rawString3 + rawString4 + "$";
		}

		private static string ConvertRegexReservedChars(string rawString)
		{
			if (rawString.Length == 0)
			{
				return rawString;
			}
			StringBuilder stringBuilder = new StringBuilder();
			foreach (char c in rawString)
			{
				if ("#$()+.?[\\^{|".IndexOf(c) != -1)
				{
					stringBuilder.Append('\\');
				}
				else if (c == '*')
				{
					stringBuilder.Append('.');
				}
				stringBuilder.Append(c);
			}
			return stringBuilder.ToString();
		}

		private static ArrayList ParseBypassList(string bypassListString, out bool bypassOnLocal)
		{
			string[] array = bypassListString.Split(s_BypassListDelimiter);
			bypassOnLocal = false;
			if (array.Length == 0)
			{
				return null;
			}
			ArrayList arrayList = null;
			string[] array2 = array;
			foreach (string text in array2)
			{
				if (text == null)
				{
					continue;
				}
				string text2 = text.Trim();
				if (text2.Length <= 0)
				{
					continue;
				}
				if (string.Compare(text2, "<local>", StringComparison.OrdinalIgnoreCase) == 0)
				{
					bypassOnLocal = true;
					continue;
				}
				text2 = BypassStringEscape(text2);
				if (arrayList == null)
				{
					arrayList = new ArrayList();
				}
				if (!arrayList.Contains(text2))
				{
					arrayList.Add(text2);
				}
			}
			return arrayList;
		}
	}
}
