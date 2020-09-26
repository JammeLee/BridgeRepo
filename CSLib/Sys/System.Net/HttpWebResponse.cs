using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization;
using System.Security.Cryptography.X509Certificates;
using System.Security.Permissions;

namespace System.Net
{
	[Serializable]
	public class HttpWebResponse : WebResponse, ISerializable
	{
		private Uri m_Uri;

		private KnownHttpVerb m_Verb;

		private HttpStatusCode m_StatusCode;

		private string m_StatusDescription;

		private Stream m_ConnectStream;

		private WebHeaderCollection m_HttpResponseHeaders;

		private long m_ContentLength;

		private string m_MediaType;

		private string m_CharacterSet;

		private bool m_IsVersionHttp11;

		internal X509Certificate m_Certificate;

		private CookieCollection m_cookies;

		private bool m_disposed;

		private bool m_propertiesDisposed;

		private bool m_UsesProxySemantics;

		private bool m_IsMutuallyAuthenticated;

		internal Stream ResponseStream
		{
			get
			{
				return m_ConnectStream;
			}
			set
			{
				m_ConnectStream = value;
			}
		}

		public override bool IsMutuallyAuthenticated => m_IsMutuallyAuthenticated;

		internal bool InternalSetIsMutuallyAuthenticated
		{
			set
			{
				m_IsMutuallyAuthenticated = value;
			}
		}

		public CookieCollection Cookies
		{
			get
			{
				CheckDisposed();
				if (m_cookies == null)
				{
					m_cookies = new CookieCollection();
				}
				return m_cookies;
			}
			set
			{
				CheckDisposed();
				m_cookies = value;
			}
		}

		public override WebHeaderCollection Headers => m_HttpResponseHeaders;

		public override long ContentLength => m_ContentLength;

		public string ContentEncoding
		{
			get
			{
				CheckDisposed();
				string text = m_HttpResponseHeaders["Content-Encoding"];
				if (text != null)
				{
					return text;
				}
				return string.Empty;
			}
		}

		public override string ContentType
		{
			get
			{
				CheckDisposed();
				string contentType = m_HttpResponseHeaders.ContentType;
				if (contentType != null)
				{
					return contentType;
				}
				return string.Empty;
			}
		}

		public string CharacterSet
		{
			get
			{
				CheckDisposed();
				string contentType = m_HttpResponseHeaders.ContentType;
				if (m_CharacterSet == null && !ValidationHelper.IsBlankString(contentType))
				{
					m_CharacterSet = string.Empty;
					string text = contentType.ToLower(CultureInfo.InvariantCulture);
					if (text.Trim().StartsWith("text/"))
					{
						m_CharacterSet = "ISO-8859-1";
					}
					int i = text.IndexOf(";");
					if (i > 0)
					{
						while ((i = text.IndexOf("charset", i)) >= 0)
						{
							i += 7;
							if (text[i - 8] != ';' && text[i - 8] != ' ')
							{
								continue;
							}
							for (; i < text.Length && text[i] == ' '; i++)
							{
							}
							if (i < text.Length - 1 && text[i] == '=')
							{
								i++;
								int num = text.IndexOf(';', i);
								if (num > i)
								{
									m_CharacterSet = contentType.Substring(i, num - i).Trim();
								}
								else
								{
									m_CharacterSet = contentType.Substring(i).Trim();
								}
								break;
							}
						}
					}
				}
				return m_CharacterSet;
			}
		}

		public string Server
		{
			get
			{
				CheckDisposed();
				string server = m_HttpResponseHeaders.Server;
				if (server != null)
				{
					return server;
				}
				return string.Empty;
			}
		}

		public DateTime LastModified
		{
			get
			{
				CheckDisposed();
				string lastModified = m_HttpResponseHeaders.LastModified;
				if (lastModified == null)
				{
					return DateTime.Now;
				}
				return HttpProtocolUtils.string2date(lastModified);
			}
		}

		public HttpStatusCode StatusCode => m_StatusCode;

		public string StatusDescription
		{
			get
			{
				CheckDisposed();
				return m_StatusDescription;
			}
		}

		public Version ProtocolVersion
		{
			get
			{
				CheckDisposed();
				if (!m_IsVersionHttp11)
				{
					return HttpVersion.Version10;
				}
				return HttpVersion.Version11;
			}
		}

		internal bool KeepAlive
		{
			get
			{
				if (m_UsesProxySemantics)
				{
					string text = Headers["Proxy-Connection"];
					if (text != null)
					{
						if (text.ToLower(CultureInfo.InvariantCulture).IndexOf("close") >= 0)
						{
							return text.ToLower(CultureInfo.InvariantCulture).IndexOf("keep-alive") >= 0;
						}
						return true;
					}
				}
				if (ProtocolVersion == HttpVersion.Version10)
				{
					string text2 = Headers["Keep-Alive"];
					return text2 != null;
				}
				if (ProtocolVersion >= HttpVersion.Version11)
				{
					string text3 = Headers["Connection"];
					if (text3 != null && text3.ToLower(CultureInfo.InvariantCulture).IndexOf("close") >= 0)
					{
						return text3.ToLower(CultureInfo.InvariantCulture).IndexOf("keep-alive") >= 0;
					}
					return true;
				}
				return false;
			}
		}

		public override Uri ResponseUri
		{
			get
			{
				CheckDisposed();
				return m_Uri;
			}
		}

		public string Method
		{
			get
			{
				CheckDisposed();
				return m_Verb.Name;
			}
		}

		public override Stream GetResponseStream()
		{
			if (Logging.On)
			{
				Logging.Enter(Logging.Web, this, "GetResponseStream", "");
			}
			CheckDisposed();
			if (!CanGetResponseStream())
			{
				if (Logging.On)
				{
					Logging.Exit(Logging.Web, this, "GetResponseStream", Stream.Null);
				}
				return Stream.Null;
			}
			if (Logging.On)
			{
				Logging.PrintInfo(Logging.Web, "ContentLength=" + m_ContentLength);
			}
			if (Logging.On)
			{
				Logging.Exit(Logging.Web, this, "GetResponseStream", m_ConnectStream);
			}
			return m_ConnectStream;
		}

		public override void Close()
		{
			if (Logging.On)
			{
				Logging.Enter(Logging.Web, this, "Close", "");
			}
			if (!m_disposed)
			{
				m_disposed = true;
				Stream connectStream = m_ConnectStream;
				ICloseEx closeEx = connectStream as ICloseEx;
				if (closeEx != null)
				{
					closeEx.CloseEx(CloseExState.Normal);
				}
				else
				{
					connectStream?.Close();
				}
			}
			if (Logging.On)
			{
				Logging.Exit(Logging.Web, this, "Close", "");
			}
		}

		internal void Abort()
		{
			Stream connectStream = m_ConnectStream;
			ICloseEx closeEx = connectStream as ICloseEx;
			try
			{
				if (closeEx != null)
				{
					closeEx.CloseEx(CloseExState.Abort);
				}
				else
				{
					connectStream?.Close();
				}
			}
			catch
			{
			}
		}

		internal override void OnDispose()
		{
			m_propertiesDisposed = true;
		}

		internal bool CanGetResponseStream()
		{
			return !m_Verb.ExpectNoContentResponse;
		}

		internal HttpWebResponse(Uri responseUri, KnownHttpVerb verb, CoreResponseData coreData, string mediaType, bool usesProxySemantics, DecompressionMethods decompressionMethod)
		{
			m_Uri = responseUri;
			m_Verb = verb;
			m_MediaType = mediaType;
			m_UsesProxySemantics = usesProxySemantics;
			m_ConnectStream = coreData.m_ConnectStream;
			m_HttpResponseHeaders = coreData.m_ResponseHeaders;
			m_ContentLength = coreData.m_ContentLength;
			m_StatusCode = coreData.m_StatusCode;
			m_StatusDescription = coreData.m_StatusDescription;
			m_IsVersionHttp11 = coreData.m_IsVersionHttp11;
			if (m_ContentLength == 0 && m_ConnectStream is ConnectStream)
			{
				((ConnectStream)m_ConnectStream).CallDone();
			}
			string text = m_HttpResponseHeaders["Content-Location"];
			if (text != null)
			{
				try
				{
					m_Uri = new Uri(m_Uri, text);
				}
				catch (UriFormatException)
				{
				}
			}
			if (decompressionMethod == DecompressionMethods.None)
			{
				return;
			}
			string text2 = m_HttpResponseHeaders["Content-Encoding"];
			if (text2 != null)
			{
				if ((decompressionMethod & DecompressionMethods.GZip) != 0 && text2.IndexOf("gzip") != -1)
				{
					m_ConnectStream = new GZipWrapperStream(m_ConnectStream, CompressionMode.Decompress);
					m_ContentLength = -1L;
					m_HttpResponseHeaders["Content-Encoding"] = null;
				}
				else if ((decompressionMethod & DecompressionMethods.Deflate) != 0 && text2.IndexOf("deflate") != -1)
				{
					m_ConnectStream = new DeflateWrapperStream(m_ConnectStream, CompressionMode.Decompress);
					m_ContentLength = -1L;
					m_HttpResponseHeaders["Content-Encoding"] = null;
				}
			}
		}

		[Obsolete("Serialization is obsoleted for this type.  http://go.microsoft.com/fwlink/?linkid=14202")]
		protected HttpWebResponse(SerializationInfo serializationInfo, StreamingContext streamingContext)
			: base(serializationInfo, streamingContext)
		{
			m_HttpResponseHeaders = (WebHeaderCollection)serializationInfo.GetValue("m_HttpResponseHeaders", typeof(WebHeaderCollection));
			m_Uri = (Uri)serializationInfo.GetValue("m_Uri", typeof(Uri));
			m_Certificate = (X509Certificate)serializationInfo.GetValue("m_Certificate", typeof(X509Certificate));
			Version version = (Version)serializationInfo.GetValue("m_Version", typeof(Version));
			m_IsVersionHttp11 = version.Equals(HttpVersion.Version11);
			m_StatusCode = (HttpStatusCode)serializationInfo.GetInt32("m_StatusCode");
			m_ContentLength = serializationInfo.GetInt64("m_ContentLength");
			m_Verb = KnownHttpVerb.Parse(serializationInfo.GetString("m_Verb"));
			m_StatusDescription = serializationInfo.GetString("m_StatusDescription");
			m_MediaType = serializationInfo.GetString("m_MediaType");
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter, SerializationFormatter = true)]
		void ISerializable.GetObjectData(SerializationInfo serializationInfo, StreamingContext streamingContext)
		{
			GetObjectData(serializationInfo, streamingContext);
		}

		[SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
		protected override void GetObjectData(SerializationInfo serializationInfo, StreamingContext streamingContext)
		{
			serializationInfo.AddValue("m_HttpResponseHeaders", m_HttpResponseHeaders, typeof(WebHeaderCollection));
			serializationInfo.AddValue("m_Uri", m_Uri, typeof(Uri));
			serializationInfo.AddValue("m_Certificate", m_Certificate, typeof(X509Certificate));
			serializationInfo.AddValue("m_Version", ProtocolVersion, typeof(Version));
			serializationInfo.AddValue("m_StatusCode", m_StatusCode);
			serializationInfo.AddValue("m_ContentLength", m_ContentLength);
			serializationInfo.AddValue("m_Verb", m_Verb.Name);
			serializationInfo.AddValue("m_StatusDescription", m_StatusDescription);
			serializationInfo.AddValue("m_MediaType", m_MediaType);
			base.GetObjectData(serializationInfo, streamingContext);
		}

		public string GetResponseHeader(string headerName)
		{
			CheckDisposed();
			string text = m_HttpResponseHeaders[headerName];
			if (text != null)
			{
				return text;
			}
			return string.Empty;
		}

		private void CheckDisposed()
		{
			if (m_propertiesDisposed)
			{
				throw new ObjectDisposedException(GetType().FullName);
			}
		}
	}
}
