using System.Collections;
using System.Globalization;
using System.Net;

namespace System
{
	public abstract class UriParser
	{
		private class BuiltInUriParser : UriParser
		{
			internal BuiltInUriParser(string lwrCaseScheme, int defaultPort, UriSyntaxFlags syntaxFlags)
				: base(syntaxFlags | UriSyntaxFlags.SimpleUserSyntax | UriSyntaxFlags.BuiltInSyntax)
			{
				m_Scheme = lwrCaseScheme;
				m_Port = defaultPort;
			}
		}

		private const UriSyntaxFlags SchemeOnlyFlags = UriSyntaxFlags.MayHavePath;

		internal const int NoDefaultPort = -1;

		private const int c_InitialTableSize = 25;

		private const int c_MaxCapacity = 512;

		private const UriSyntaxFlags UnknownV1SyntaxFlags = UriSyntaxFlags.AllowAnInternetHost | UriSyntaxFlags.OptionalAuthority | UriSyntaxFlags.MayHaveUserInfo | UriSyntaxFlags.MayHavePort | UriSyntaxFlags.MayHavePath | UriSyntaxFlags.MayHaveQuery | UriSyntaxFlags.MayHaveFragment | UriSyntaxFlags.AllowEmptyHost | UriSyntaxFlags.AllowUncHost | UriSyntaxFlags.V1_UnknownUri | UriSyntaxFlags.AllowDOSPath | UriSyntaxFlags.PathIsRooted | UriSyntaxFlags.ConvertPathSlashes | UriSyntaxFlags.CompressPath | UriSyntaxFlags.AllowIdn | UriSyntaxFlags.AllowIriParsing;

		private const UriSyntaxFlags HttpSyntaxFlags = UriSyntaxFlags.AllowAnInternetHost | UriSyntaxFlags.MustHaveAuthority | UriSyntaxFlags.MayHaveUserInfo | UriSyntaxFlags.MayHavePort | UriSyntaxFlags.MayHavePath | UriSyntaxFlags.MayHaveQuery | UriSyntaxFlags.MayHaveFragment | UriSyntaxFlags.AllowUncHost | UriSyntaxFlags.PathIsRooted | UriSyntaxFlags.ConvertPathSlashes | UriSyntaxFlags.CompressPath | UriSyntaxFlags.CanonicalizeAsFilePath | UriSyntaxFlags.UnEscapeDotsAndSlashes | UriSyntaxFlags.AllowIdn | UriSyntaxFlags.AllowIriParsing;

		private const UriSyntaxFlags FtpSyntaxFlags = UriSyntaxFlags.AllowAnInternetHost | UriSyntaxFlags.MustHaveAuthority | UriSyntaxFlags.MayHaveUserInfo | UriSyntaxFlags.MayHavePort | UriSyntaxFlags.MayHavePath | UriSyntaxFlags.MayHaveFragment | UriSyntaxFlags.AllowUncHost | UriSyntaxFlags.PathIsRooted | UriSyntaxFlags.ConvertPathSlashes | UriSyntaxFlags.CompressPath | UriSyntaxFlags.CanonicalizeAsFilePath | UriSyntaxFlags.AllowIdn | UriSyntaxFlags.AllowIriParsing;

		private const UriSyntaxFlags FileSyntaxFlags = UriSyntaxFlags.AllowAnInternetHost | UriSyntaxFlags.MustHaveAuthority | UriSyntaxFlags.MayHavePath | UriSyntaxFlags.MayHaveFragment | UriSyntaxFlags.AllowEmptyHost | UriSyntaxFlags.AllowUncHost | UriSyntaxFlags.FileLikeUri | UriSyntaxFlags.AllowDOSPath | UriSyntaxFlags.PathIsRooted | UriSyntaxFlags.ConvertPathSlashes | UriSyntaxFlags.CompressPath | UriSyntaxFlags.CanonicalizeAsFilePath | UriSyntaxFlags.UnEscapeDotsAndSlashes | UriSyntaxFlags.AllowIdn | UriSyntaxFlags.AllowIriParsing;

		private const UriSyntaxFlags VsmacrosSyntaxFlags = UriSyntaxFlags.AllowAnInternetHost | UriSyntaxFlags.MustHaveAuthority | UriSyntaxFlags.MayHavePath | UriSyntaxFlags.MayHaveFragment | UriSyntaxFlags.AllowEmptyHost | UriSyntaxFlags.AllowUncHost | UriSyntaxFlags.FileLikeUri | UriSyntaxFlags.AllowDOSPath | UriSyntaxFlags.ConvertPathSlashes | UriSyntaxFlags.CompressPath | UriSyntaxFlags.CanonicalizeAsFilePath | UriSyntaxFlags.UnEscapeDotsAndSlashes | UriSyntaxFlags.AllowIdn | UriSyntaxFlags.AllowIriParsing;

		private const UriSyntaxFlags GopherSyntaxFlags = UriSyntaxFlags.AllowAnInternetHost | UriSyntaxFlags.MustHaveAuthority | UriSyntaxFlags.MayHaveUserInfo | UriSyntaxFlags.MayHavePort | UriSyntaxFlags.MayHavePath | UriSyntaxFlags.MayHaveFragment | UriSyntaxFlags.AllowUncHost | UriSyntaxFlags.PathIsRooted | UriSyntaxFlags.AllowIdn | UriSyntaxFlags.AllowIriParsing;

		private const UriSyntaxFlags NewsSyntaxFlags = UriSyntaxFlags.MayHavePath | UriSyntaxFlags.MayHaveFragment | UriSyntaxFlags.AllowIriParsing;

		private const UriSyntaxFlags NntpSyntaxFlags = UriSyntaxFlags.AllowAnInternetHost | UriSyntaxFlags.MustHaveAuthority | UriSyntaxFlags.MayHaveUserInfo | UriSyntaxFlags.MayHavePort | UriSyntaxFlags.MayHavePath | UriSyntaxFlags.MayHaveFragment | UriSyntaxFlags.AllowUncHost | UriSyntaxFlags.PathIsRooted | UriSyntaxFlags.AllowIdn | UriSyntaxFlags.AllowIriParsing;

		private const UriSyntaxFlags TelnetSyntaxFlags = UriSyntaxFlags.AllowAnInternetHost | UriSyntaxFlags.MustHaveAuthority | UriSyntaxFlags.MayHaveUserInfo | UriSyntaxFlags.MayHavePort | UriSyntaxFlags.MayHavePath | UriSyntaxFlags.MayHaveFragment | UriSyntaxFlags.AllowUncHost | UriSyntaxFlags.PathIsRooted | UriSyntaxFlags.AllowIdn | UriSyntaxFlags.AllowIriParsing;

		private const UriSyntaxFlags LdapSyntaxFlags = UriSyntaxFlags.AllowAnInternetHost | UriSyntaxFlags.MustHaveAuthority | UriSyntaxFlags.MayHaveUserInfo | UriSyntaxFlags.MayHavePort | UriSyntaxFlags.MayHavePath | UriSyntaxFlags.MayHaveQuery | UriSyntaxFlags.MayHaveFragment | UriSyntaxFlags.AllowEmptyHost | UriSyntaxFlags.AllowUncHost | UriSyntaxFlags.PathIsRooted | UriSyntaxFlags.AllowIdn | UriSyntaxFlags.AllowIriParsing;

		private const UriSyntaxFlags MailtoSyntaxFlags = UriSyntaxFlags.AllowAnInternetHost | UriSyntaxFlags.MayHaveUserInfo | UriSyntaxFlags.MayHavePort | UriSyntaxFlags.MayHavePath | UriSyntaxFlags.MayHaveQuery | UriSyntaxFlags.MayHaveFragment | UriSyntaxFlags.AllowEmptyHost | UriSyntaxFlags.AllowUncHost | UriSyntaxFlags.MailToLikeUri | UriSyntaxFlags.AllowIdn | UriSyntaxFlags.AllowIriParsing;

		private const UriSyntaxFlags NetPipeSyntaxFlags = UriSyntaxFlags.AllowAnInternetHost | UriSyntaxFlags.MustHaveAuthority | UriSyntaxFlags.MayHavePath | UriSyntaxFlags.MayHaveQuery | UriSyntaxFlags.MayHaveFragment | UriSyntaxFlags.PathIsRooted | UriSyntaxFlags.ConvertPathSlashes | UriSyntaxFlags.CompressPath | UriSyntaxFlags.CanonicalizeAsFilePath | UriSyntaxFlags.UnEscapeDotsAndSlashes | UriSyntaxFlags.AllowIdn | UriSyntaxFlags.AllowIriParsing;

		private const UriSyntaxFlags NetTcpSyntaxFlags = UriSyntaxFlags.AllowAnInternetHost | UriSyntaxFlags.MustHaveAuthority | UriSyntaxFlags.MayHavePort | UriSyntaxFlags.MayHavePath | UriSyntaxFlags.MayHaveQuery | UriSyntaxFlags.MayHaveFragment | UriSyntaxFlags.PathIsRooted | UriSyntaxFlags.ConvertPathSlashes | UriSyntaxFlags.CompressPath | UriSyntaxFlags.CanonicalizeAsFilePath | UriSyntaxFlags.UnEscapeDotsAndSlashes | UriSyntaxFlags.AllowIdn | UriSyntaxFlags.AllowIriParsing;

		private static readonly Hashtable m_Table;

		private static Hashtable m_TempTable;

		private UriSyntaxFlags m_Flags;

		private int m_Port;

		private string m_Scheme;

		internal static UriParser HttpUri;

		internal static UriParser HttpsUri;

		internal static UriParser FtpUri;

		internal static UriParser FileUri;

		internal static UriParser GopherUri;

		internal static UriParser NntpUri;

		internal static UriParser NewsUri;

		internal static UriParser MailToUri;

		internal static UriParser UuidUri;

		internal static UriParser TelnetUri;

		internal static UriParser LdapUri;

		internal static UriParser NetTcpUri;

		internal static UriParser NetPipeUri;

		internal static UriParser VsMacrosUri;

		internal string SchemeName => m_Scheme;

		internal int DefaultPort => m_Port;

		internal UriSyntaxFlags Flags => m_Flags;

		internal bool IsSimple => InFact(UriSyntaxFlags.SimpleUserSyntax);

		protected UriParser()
			: this(UriSyntaxFlags.MayHavePath)
		{
		}

		protected virtual UriParser OnNewUri()
		{
			return this;
		}

		protected virtual void OnRegister(string schemeName, int defaultPort)
		{
		}

		protected virtual void InitializeAndValidate(Uri uri, out UriFormatException parsingError)
		{
			parsingError = uri.ParseMinimal();
		}

		protected virtual string Resolve(Uri baseUri, Uri relativeUri, out UriFormatException parsingError)
		{
			if (baseUri.UserDrivenParsing)
			{
				throw new InvalidOperationException(SR.GetString("net_uri_UserDrivenParsing", GetType().FullName));
			}
			if (!baseUri.IsAbsoluteUri)
			{
				throw new InvalidOperationException(SR.GetString("net_uri_NotAbsolute"));
			}
			string newUriString = null;
			bool userEscaped = false;
			Uri uri = Uri.ResolveHelper(baseUri, relativeUri, ref newUriString, ref userEscaped, out parsingError);
			if (parsingError != null)
			{
				return null;
			}
			if (uri != null)
			{
				return uri.OriginalString;
			}
			return newUriString;
		}

		protected virtual bool IsBaseOf(Uri baseUri, Uri relativeUri)
		{
			return baseUri.IsBaseOfHelper(relativeUri);
		}

		protected virtual string GetComponents(Uri uri, UriComponents components, UriFormat format)
		{
			if (((uint)components & 0x80000000u) != 0 && components != UriComponents.SerializationInfoString)
			{
				throw new ArgumentOutOfRangeException("UriComponents.SerializationInfoString");
			}
			if (((uint)format & 0xFFFFFFFCu) != 0)
			{
				throw new ArgumentOutOfRangeException("format");
			}
			if (uri.UserDrivenParsing)
			{
				throw new InvalidOperationException(SR.GetString("net_uri_UserDrivenParsing", GetType().FullName));
			}
			if (!uri.IsAbsoluteUri)
			{
				throw new InvalidOperationException(SR.GetString("net_uri_NotAbsolute"));
			}
			return uri.GetComponentsHelper(components, format);
		}

		protected virtual bool IsWellFormedOriginalString(Uri uri)
		{
			return uri.InternalIsWellFormedOriginalString();
		}

		public static void Register(UriParser uriParser, string schemeName, int defaultPort)
		{
			ExceptionHelper.InfrastructurePermission.Demand();
			if (uriParser == null)
			{
				throw new ArgumentNullException("uriParser");
			}
			if (schemeName == null)
			{
				throw new ArgumentNullException("schemeName");
			}
			if (schemeName.Length == 1)
			{
				throw new ArgumentOutOfRangeException("uriParser.SchemeName");
			}
			if (!CheckSchemeName(schemeName))
			{
				throw new ArgumentOutOfRangeException("schemeName");
			}
			if ((defaultPort >= 65535 || defaultPort < 0) && defaultPort != -1)
			{
				throw new ArgumentOutOfRangeException("defaultPort");
			}
			schemeName = schemeName.ToLower(CultureInfo.InvariantCulture);
			FetchSyntax(uriParser, schemeName, defaultPort);
		}

		public static bool IsKnownScheme(string schemeName)
		{
			if (schemeName == null)
			{
				throw new ArgumentNullException("schemeName");
			}
			if (!CheckSchemeName(schemeName))
			{
				throw new ArgumentOutOfRangeException("schemeName");
			}
			return GetSyntax(schemeName.ToLower(CultureInfo.InvariantCulture))?.NotAny(UriSyntaxFlags.V1_UnknownUri) ?? false;
		}

		static UriParser()
		{
			m_Table = new Hashtable(25);
			m_TempTable = new Hashtable(25);
			HttpUri = new BuiltInUriParser("http", 80, UriSyntaxFlags.AllowAnInternetHost | UriSyntaxFlags.MustHaveAuthority | UriSyntaxFlags.MayHaveUserInfo | UriSyntaxFlags.MayHavePort | UriSyntaxFlags.MayHavePath | UriSyntaxFlags.MayHaveQuery | UriSyntaxFlags.MayHaveFragment | UriSyntaxFlags.AllowUncHost | UriSyntaxFlags.PathIsRooted | UriSyntaxFlags.ConvertPathSlashes | UriSyntaxFlags.CompressPath | UriSyntaxFlags.CanonicalizeAsFilePath | UriSyntaxFlags.UnEscapeDotsAndSlashes | UriSyntaxFlags.AllowIdn | UriSyntaxFlags.AllowIriParsing);
			m_Table[HttpUri.SchemeName] = HttpUri;
			HttpsUri = new BuiltInUriParser("https", 443, HttpUri.m_Flags);
			m_Table[HttpsUri.SchemeName] = HttpsUri;
			FtpUri = new BuiltInUriParser("ftp", 21, UriSyntaxFlags.AllowAnInternetHost | UriSyntaxFlags.MustHaveAuthority | UriSyntaxFlags.MayHaveUserInfo | UriSyntaxFlags.MayHavePort | UriSyntaxFlags.MayHavePath | UriSyntaxFlags.MayHaveFragment | UriSyntaxFlags.AllowUncHost | UriSyntaxFlags.PathIsRooted | UriSyntaxFlags.ConvertPathSlashes | UriSyntaxFlags.CompressPath | UriSyntaxFlags.CanonicalizeAsFilePath | UriSyntaxFlags.AllowIdn | UriSyntaxFlags.AllowIriParsing);
			m_Table[FtpUri.SchemeName] = FtpUri;
			FileUri = new BuiltInUriParser("file", -1, UriSyntaxFlags.AllowAnInternetHost | UriSyntaxFlags.MustHaveAuthority | UriSyntaxFlags.MayHavePath | UriSyntaxFlags.MayHaveFragment | UriSyntaxFlags.AllowEmptyHost | UriSyntaxFlags.AllowUncHost | UriSyntaxFlags.FileLikeUri | UriSyntaxFlags.AllowDOSPath | UriSyntaxFlags.PathIsRooted | UriSyntaxFlags.ConvertPathSlashes | UriSyntaxFlags.CompressPath | UriSyntaxFlags.CanonicalizeAsFilePath | UriSyntaxFlags.UnEscapeDotsAndSlashes | UriSyntaxFlags.AllowIdn | UriSyntaxFlags.AllowIriParsing);
			m_Table[FileUri.SchemeName] = FileUri;
			GopherUri = new BuiltInUriParser("gopher", 70, UriSyntaxFlags.AllowAnInternetHost | UriSyntaxFlags.MustHaveAuthority | UriSyntaxFlags.MayHaveUserInfo | UriSyntaxFlags.MayHavePort | UriSyntaxFlags.MayHavePath | UriSyntaxFlags.MayHaveFragment | UriSyntaxFlags.AllowUncHost | UriSyntaxFlags.PathIsRooted | UriSyntaxFlags.AllowIdn | UriSyntaxFlags.AllowIriParsing);
			m_Table[GopherUri.SchemeName] = GopherUri;
			NntpUri = new BuiltInUriParser("nntp", 119, UriSyntaxFlags.AllowAnInternetHost | UriSyntaxFlags.MustHaveAuthority | UriSyntaxFlags.MayHaveUserInfo | UriSyntaxFlags.MayHavePort | UriSyntaxFlags.MayHavePath | UriSyntaxFlags.MayHaveFragment | UriSyntaxFlags.AllowUncHost | UriSyntaxFlags.PathIsRooted | UriSyntaxFlags.AllowIdn | UriSyntaxFlags.AllowIriParsing);
			m_Table[NntpUri.SchemeName] = NntpUri;
			NewsUri = new BuiltInUriParser("news", -1, UriSyntaxFlags.MayHavePath | UriSyntaxFlags.MayHaveFragment | UriSyntaxFlags.AllowIriParsing);
			m_Table[NewsUri.SchemeName] = NewsUri;
			MailToUri = new BuiltInUriParser("mailto", 25, UriSyntaxFlags.AllowAnInternetHost | UriSyntaxFlags.MayHaveUserInfo | UriSyntaxFlags.MayHavePort | UriSyntaxFlags.MayHavePath | UriSyntaxFlags.MayHaveQuery | UriSyntaxFlags.MayHaveFragment | UriSyntaxFlags.AllowEmptyHost | UriSyntaxFlags.AllowUncHost | UriSyntaxFlags.MailToLikeUri | UriSyntaxFlags.AllowIdn | UriSyntaxFlags.AllowIriParsing);
			m_Table[MailToUri.SchemeName] = MailToUri;
			UuidUri = new BuiltInUriParser("uuid", -1, NewsUri.m_Flags);
			m_Table[UuidUri.SchemeName] = UuidUri;
			TelnetUri = new BuiltInUriParser("telnet", 23, UriSyntaxFlags.AllowAnInternetHost | UriSyntaxFlags.MustHaveAuthority | UriSyntaxFlags.MayHaveUserInfo | UriSyntaxFlags.MayHavePort | UriSyntaxFlags.MayHavePath | UriSyntaxFlags.MayHaveFragment | UriSyntaxFlags.AllowUncHost | UriSyntaxFlags.PathIsRooted | UriSyntaxFlags.AllowIdn | UriSyntaxFlags.AllowIriParsing);
			m_Table[TelnetUri.SchemeName] = TelnetUri;
			LdapUri = new BuiltInUriParser("ldap", 389, UriSyntaxFlags.AllowAnInternetHost | UriSyntaxFlags.MustHaveAuthority | UriSyntaxFlags.MayHaveUserInfo | UriSyntaxFlags.MayHavePort | UriSyntaxFlags.MayHavePath | UriSyntaxFlags.MayHaveQuery | UriSyntaxFlags.MayHaveFragment | UriSyntaxFlags.AllowEmptyHost | UriSyntaxFlags.AllowUncHost | UriSyntaxFlags.PathIsRooted | UriSyntaxFlags.AllowIdn | UriSyntaxFlags.AllowIriParsing);
			m_Table[LdapUri.SchemeName] = LdapUri;
			NetTcpUri = new BuiltInUriParser("net.tcp", 808, UriSyntaxFlags.AllowAnInternetHost | UriSyntaxFlags.MustHaveAuthority | UriSyntaxFlags.MayHavePort | UriSyntaxFlags.MayHavePath | UriSyntaxFlags.MayHaveQuery | UriSyntaxFlags.MayHaveFragment | UriSyntaxFlags.PathIsRooted | UriSyntaxFlags.ConvertPathSlashes | UriSyntaxFlags.CompressPath | UriSyntaxFlags.CanonicalizeAsFilePath | UriSyntaxFlags.UnEscapeDotsAndSlashes | UriSyntaxFlags.AllowIdn | UriSyntaxFlags.AllowIriParsing);
			m_Table[NetTcpUri.SchemeName] = NetTcpUri;
			NetPipeUri = new BuiltInUriParser("net.pipe", -1, UriSyntaxFlags.AllowAnInternetHost | UriSyntaxFlags.MustHaveAuthority | UriSyntaxFlags.MayHavePath | UriSyntaxFlags.MayHaveQuery | UriSyntaxFlags.MayHaveFragment | UriSyntaxFlags.PathIsRooted | UriSyntaxFlags.ConvertPathSlashes | UriSyntaxFlags.CompressPath | UriSyntaxFlags.CanonicalizeAsFilePath | UriSyntaxFlags.UnEscapeDotsAndSlashes | UriSyntaxFlags.AllowIdn | UriSyntaxFlags.AllowIriParsing);
			m_Table[NetPipeUri.SchemeName] = NetPipeUri;
			VsMacrosUri = new BuiltInUriParser("vsmacros", -1, UriSyntaxFlags.AllowAnInternetHost | UriSyntaxFlags.MustHaveAuthority | UriSyntaxFlags.MayHavePath | UriSyntaxFlags.MayHaveFragment | UriSyntaxFlags.AllowEmptyHost | UriSyntaxFlags.AllowUncHost | UriSyntaxFlags.FileLikeUri | UriSyntaxFlags.AllowDOSPath | UriSyntaxFlags.ConvertPathSlashes | UriSyntaxFlags.CompressPath | UriSyntaxFlags.CanonicalizeAsFilePath | UriSyntaxFlags.UnEscapeDotsAndSlashes | UriSyntaxFlags.AllowIdn | UriSyntaxFlags.AllowIriParsing);
			m_Table[VsMacrosUri.SchemeName] = VsMacrosUri;
		}

		internal bool NotAny(UriSyntaxFlags flags)
		{
			return (m_Flags & flags) == 0;
		}

		internal bool InFact(UriSyntaxFlags flags)
		{
			return (m_Flags & flags) != 0;
		}

		internal bool IsAllSet(UriSyntaxFlags flags)
		{
			return (m_Flags & flags) == flags;
		}

		internal UriParser(UriSyntaxFlags flags)
		{
			m_Flags = flags;
			m_Scheme = string.Empty;
		}

		private static void FetchSyntax(UriParser syntax, string lwrCaseSchemeName, int defaultPort)
		{
			if (syntax.SchemeName.Length != 0)
			{
				throw new InvalidOperationException(SR.GetString("net_uri_NeedFreshParser", syntax.SchemeName));
			}
			lock (m_Table)
			{
				syntax.m_Flags &= ~UriSyntaxFlags.V1_UnknownUri;
				UriParser uriParser = (UriParser)m_Table[lwrCaseSchemeName];
				if (uriParser != null)
				{
					throw new InvalidOperationException(SR.GetString("net_uri_AlreadyRegistered", uriParser.SchemeName));
				}
				uriParser = (UriParser)m_TempTable[syntax.SchemeName];
				if (uriParser != null)
				{
					lwrCaseSchemeName = uriParser.m_Scheme;
					m_TempTable.Remove(lwrCaseSchemeName);
				}
				syntax.OnRegister(lwrCaseSchemeName, defaultPort);
				syntax.m_Scheme = lwrCaseSchemeName;
				syntax.CheckSetIsSimpleFlag();
				syntax.m_Port = defaultPort;
				m_Table[syntax.SchemeName] = syntax;
			}
		}

		internal static UriParser FindOrFetchAsUnknownV1Syntax(string lwrCaseScheme)
		{
			UriParser uriParser = (UriParser)m_Table[lwrCaseScheme];
			if (uriParser != null)
			{
				return uriParser;
			}
			uriParser = (UriParser)m_TempTable[lwrCaseScheme];
			if (uriParser != null)
			{
				return uriParser;
			}
			lock (m_Table)
			{
				if (m_TempTable.Count >= 512)
				{
					m_TempTable = new Hashtable(25);
				}
				uriParser = new BuiltInUriParser(lwrCaseScheme, -1, UriSyntaxFlags.AllowAnInternetHost | UriSyntaxFlags.OptionalAuthority | UriSyntaxFlags.MayHaveUserInfo | UriSyntaxFlags.MayHavePort | UriSyntaxFlags.MayHavePath | UriSyntaxFlags.MayHaveQuery | UriSyntaxFlags.MayHaveFragment | UriSyntaxFlags.AllowEmptyHost | UriSyntaxFlags.AllowUncHost | UriSyntaxFlags.V1_UnknownUri | UriSyntaxFlags.AllowDOSPath | UriSyntaxFlags.PathIsRooted | UriSyntaxFlags.ConvertPathSlashes | UriSyntaxFlags.CompressPath | UriSyntaxFlags.AllowIdn | UriSyntaxFlags.AllowIriParsing);
				m_TempTable[lwrCaseScheme] = uriParser;
				return uriParser;
			}
		}

		internal static UriParser GetSyntax(string lwrCaseScheme)
		{
			object obj = m_Table[lwrCaseScheme];
			if (obj == null)
			{
				obj = m_TempTable[lwrCaseScheme];
			}
			return (UriParser)obj;
		}

		internal void CheckSetIsSimpleFlag()
		{
			Type type = GetType();
			if (type == typeof(GenericUriParser) || type == typeof(HttpStyleUriParser) || type == typeof(FtpStyleUriParser) || type == typeof(FileStyleUriParser) || type == typeof(NewsStyleUriParser) || type == typeof(GopherStyleUriParser) || type == typeof(NetPipeStyleUriParser) || type == typeof(NetTcpStyleUriParser) || type == typeof(LdapStyleUriParser))
			{
				m_Flags |= UriSyntaxFlags.SimpleUserSyntax;
			}
		}

		private static bool CheckSchemeName(string schemeName)
		{
			if (schemeName == null || schemeName.Length == 0 || !IsAsciiLetter(schemeName[0]))
			{
				return false;
			}
			for (int num = schemeName.Length - 1; num > 0; num--)
			{
				if (!IsAsciiLetterOrDigit(schemeName[num]) && schemeName[num] != '+' && schemeName[num] != '-' && schemeName[num] != '.')
				{
					return false;
				}
			}
			return true;
		}

		private static bool IsAsciiLetter(char character)
		{
			if (character < 'a' || character > 'z')
			{
				if (character >= 'A')
				{
					return character <= 'Z';
				}
				return false;
			}
			return true;
		}

		private static bool IsAsciiLetterOrDigit(char character)
		{
			if (!IsAsciiLetter(character))
			{
				if (character >= '0')
				{
					return character <= '9';
				}
				return false;
			}
			return true;
		}

		internal UriParser InternalOnNewUri()
		{
			UriParser uriParser = OnNewUri();
			if (this != uriParser)
			{
				uriParser.m_Scheme = m_Scheme;
				uriParser.m_Port = m_Port;
				uriParser.m_Flags = m_Flags;
			}
			return uriParser;
		}

		internal void InternalValidate(Uri thisUri, out UriFormatException parsingError)
		{
			InitializeAndValidate(thisUri, out parsingError);
		}

		internal string InternalResolve(Uri thisBaseUri, Uri uriLink, out UriFormatException parsingError)
		{
			return Resolve(thisBaseUri, uriLink, out parsingError);
		}

		internal bool InternalIsBaseOf(Uri thisBaseUri, Uri uriLink)
		{
			return IsBaseOf(thisBaseUri, uriLink);
		}

		internal string InternalGetComponents(Uri thisUri, UriComponents uriComponents, UriFormat uriFormat)
		{
			return GetComponents(thisUri, uriComponents, uriFormat);
		}

		internal bool InternalIsWellFormedOriginalString(Uri thisUri)
		{
			return IsWellFormedOriginalString(thisUri);
		}
	}
}
