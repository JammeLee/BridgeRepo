using System.Collections;
using System.ComponentModel;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using System.Xml;
using Microsoft.Win32;

namespace System
{
	[Serializable]
	[TypeConverter(typeof(UriTypeConverter))]
	public class Uri : ISerializable
	{
		private enum ParsingError
		{
			None = 0,
			BadFormat = 1,
			BadScheme = 2,
			BadAuthority = 3,
			EmptyUriString = 4,
			LastRelativeUriOkErrIndex = 4,
			SchemeLimit = 5,
			SizeLimit = 6,
			MustRootedPath = 7,
			LastFatalErrIndex = 7,
			BadHostName = 8,
			NonEmptyHost = 9,
			BadPort = 10,
			BadAuthorityTerminator = 11,
			CannotCreateRelative = 12
		}

		[Flags]
		private enum Flags : ulong
		{
			Zero = 0x0uL,
			SchemeNotCanonical = 0x1uL,
			UserNotCanonical = 0x2uL,
			HostNotCanonical = 0x4uL,
			PortNotCanonical = 0x8uL,
			PathNotCanonical = 0x10uL,
			QueryNotCanonical = 0x20uL,
			FragmentNotCanonical = 0x40uL,
			CannotDisplayCanonical = 0x7FuL,
			E_UserNotCanonical = 0x80uL,
			E_HostNotCanonical = 0x100uL,
			E_PortNotCanonical = 0x200uL,
			E_PathNotCanonical = 0x400uL,
			E_QueryNotCanonical = 0x800uL,
			E_FragmentNotCanonical = 0x1000uL,
			E_CannotDisplayCanonical = 0x1F80uL,
			ShouldBeCompressed = 0x2000uL,
			FirstSlashAbsent = 0x4000uL,
			BackslashInPath = 0x8000uL,
			IndexMask = 0xFFFFuL,
			HostTypeMask = 0x70000uL,
			HostNotParsed = 0x0uL,
			IPv6HostType = 0x10000uL,
			IPv4HostType = 0x20000uL,
			DnsHostType = 0x30000uL,
			UncHostType = 0x40000uL,
			BasicHostType = 0x50000uL,
			UnusedHostType = 0x60000uL,
			UnknownHostType = 0x70000uL,
			UserEscaped = 0x80000uL,
			AuthorityFound = 0x100000uL,
			HasUserInfo = 0x200000uL,
			LoopbackHost = 0x400000uL,
			NotDefaultPort = 0x800000uL,
			UserDrivenParsing = 0x1000000uL,
			CanonicalDnsHost = 0x2000000uL,
			ErrorOrParsingRecursion = 0x4000000uL,
			DosPath = 0x8000000uL,
			UncPath = 0x10000000uL,
			ImplicitFile = 0x20000000uL,
			MinimalUriInfoSet = 0x40000000uL,
			AllUriInfoSet = 0x80000000uL,
			IdnHost = 0x100000000uL,
			HasUnicode = 0x200000000uL,
			HostUnicodeNormalized = 0x400000000uL,
			RestUnicodeNormalized = 0x800000000uL,
			UnicodeHost = 0x1000000000uL,
			IntranetUri = 0x2000000000uL,
			UseOrigUncdStrOffset = 0x4000000000uL,
			UserIriCanonical = 0x8000000000uL,
			PathIriCanonical = 0x10000000000uL,
			QueryIriCanonical = 0x20000000000uL,
			FragmentIriCanonical = 0x40000000000uL,
			IriCanonical = 0x78000000000uL
		}

		private class UriInfo
		{
			public string Host;

			public string ScopeId;

			public string String;

			public Offset Offset;

			public string DnsSafeHost;

			public MoreInfo MoreInfo;
		}

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		private struct Offset
		{
			public ushort Scheme;

			public ushort User;

			public ushort Host;

			public ushort PortValue;

			public ushort Path;

			public ushort Query;

			public ushort Fragment;

			public ushort End;
		}

		private class MoreInfo
		{
			public string Path;

			public string Query;

			public string Fragment;

			public string AbsoluteUri;

			public int Hash;

			public string RemoteUrl;
		}

		private enum IdnScopeFromConfig
		{
			None = 0,
			AllExceptIntranet = 1,
			All = 2,
			Invalid = 2147483646,
			NotFound = int.MaxValue
		}

		private enum IriParsingFromConfig
		{
			False = 0,
			True = 1,
			Invalid = 2147483646,
			NotFound = int.MaxValue
		}

		[Flags]
		private enum Check
		{
			None = 0x0,
			EscapedCanonical = 0x1,
			DisplayCanonical = 0x2,
			DotSlashAttn = 0x4,
			DotSlashEscaped = 0x80,
			BackslashInPath = 0x10,
			ReservedFound = 0x20,
			NotIriCanonical = 0x40,
			FoundNonAscii = 0x8
		}

		[Flags]
		private enum UnescapeMode
		{
			CopyOnly = 0x0,
			Escape = 0x1,
			Unescape = 0x2,
			EscapeUnescape = 0x3,
			V1ToStringFlag = 0x4,
			UnescapeAll = 0x8,
			UnescapeAllOrThrow = 0x18
		}

		private const int c_Max16BitUtf8SequenceLength = 12;

		private const int c_MaxUriBufferSize = 65520;

		private const int c_MaxUriSchemeName = 1024;

		private const UriFormat V1ToStringUnescape = (UriFormat)32767;

		private const char c_DummyChar = '\uffff';

		private const char c_EOL = '\ufffe';

		private const short c_MaxAsciiCharsReallocate = 40;

		private const short c_MaxUnicodeCharsReallocate = 40;

		private const short c_MaxUTF_8BytesPerUnicodeChar = 4;

		private const short c_EncodedCharsPerByte = 3;

		public static readonly string UriSchemeFile = UriParser.FileUri.SchemeName;

		public static readonly string UriSchemeFtp = UriParser.FtpUri.SchemeName;

		public static readonly string UriSchemeGopher = UriParser.GopherUri.SchemeName;

		public static readonly string UriSchemeHttp = UriParser.HttpUri.SchemeName;

		public static readonly string UriSchemeHttps = UriParser.HttpsUri.SchemeName;

		public static readonly string UriSchemeMailto = UriParser.MailToUri.SchemeName;

		public static readonly string UriSchemeNews = UriParser.NewsUri.SchemeName;

		public static readonly string UriSchemeNntp = UriParser.NntpUri.SchemeName;

		public static readonly string UriSchemeNetTcp = UriParser.NetTcpUri.SchemeName;

		public static readonly string UriSchemeNetPipe = UriParser.NetPipeUri.SchemeName;

		public static readonly string SchemeDelimiter = "://";

		private string m_String;

		private string m_originalUnicodeString;

		private UriParser m_Syntax;

		private string m_DnsSafeHost;

		private Flags m_Flags;

		private UriInfo m_Info;

		private bool m_iriParsing;

		private static IInternetSecurityManager s_ManagerRef = null;

		private static object s_IntranetLock = new object();

		private static bool s_ConfigInitialized;

		private static UriIdnScope s_IdnScope = UriIdnScope.None;

		private static bool s_IriParsing = false;

		private static object s_initLock;

		private static readonly char[] HexUpperChars = new char[16]
		{
			'0',
			'1',
			'2',
			'3',
			'4',
			'5',
			'6',
			'7',
			'8',
			'9',
			'A',
			'B',
			'C',
			'D',
			'E',
			'F'
		};

		internal static readonly char[] HexLowerChars = new char[16]
		{
			'0',
			'1',
			'2',
			'3',
			'4',
			'5',
			'6',
			'7',
			'8',
			'9',
			'a',
			'b',
			'c',
			'd',
			'e',
			'f'
		};

		private static readonly char[] _WSchars = new char[4]
		{
			' ',
			'\n',
			'\r',
			'\t'
		};

		private bool IsImplicitFile => (m_Flags & Flags.ImplicitFile) != 0;

		private bool IsUncOrDosPath => (m_Flags & (Flags.DosPath | Flags.UncPath)) != 0;

		private bool IsDosPath => (m_Flags & Flags.DosPath) != 0;

		private bool IsUncPath => (m_Flags & Flags.UncPath) != 0;

		private Flags HostType => m_Flags & Flags.HostTypeMask;

		private UriParser Syntax => m_Syntax;

		private bool IsNotAbsoluteUri => m_Syntax == null;

		private bool AllowIdn
		{
			get
			{
				if (m_Syntax != null && (m_Syntax.Flags & UriSyntaxFlags.AllowIdn) != 0)
				{
					if (s_IdnScope != UriIdnScope.All)
					{
						if (s_IdnScope == UriIdnScope.AllExceptIntranet)
						{
							return NotAny(Flags.IntranetUri);
						}
						return false;
					}
					return true;
				}
				return false;
			}
		}

		internal bool UserDrivenParsing => (m_Flags & Flags.UserDrivenParsing) != 0;

		private ushort SecuredPathIndex
		{
			get
			{
				if (IsDosPath)
				{
					char c = m_String[m_Info.Offset.Path];
					return (ushort)((c == '/' || c == '\\') ? 3u : 2u);
				}
				return 0;
			}
		}

		public string AbsolutePath
		{
			get
			{
				if (IsNotAbsoluteUri)
				{
					throw new InvalidOperationException(SR.GetString("net_uri_NotAbsolute"));
				}
				string text = PrivateAbsolutePath;
				if (IsDosPath && text[0] == '/')
				{
					text = text.Substring(1);
				}
				return text;
			}
		}

		private string PrivateAbsolutePath
		{
			get
			{
				UriInfo uriInfo = EnsureUriInfo();
				if (uriInfo.MoreInfo == null)
				{
					uriInfo.MoreInfo = new MoreInfo();
				}
				string text = uriInfo.MoreInfo.Path;
				if (text == null)
				{
					text = GetParts(UriComponents.Path | UriComponents.KeepDelimiter, UriFormat.UriEscaped);
					uriInfo.MoreInfo.Path = text;
				}
				return text;
			}
		}

		public string AbsoluteUri
		{
			get
			{
				if (m_Syntax == null)
				{
					throw new InvalidOperationException(SR.GetString("net_uri_NotAbsolute"));
				}
				UriInfo uriInfo = EnsureUriInfo();
				if (uriInfo.MoreInfo == null)
				{
					uriInfo.MoreInfo = new MoreInfo();
				}
				string text = uriInfo.MoreInfo.AbsoluteUri;
				if (text == null)
				{
					text = GetParts(UriComponents.AbsoluteUri, UriFormat.UriEscaped);
					uriInfo.MoreInfo.AbsoluteUri = text;
				}
				return text;
			}
		}

		public string Authority
		{
			get
			{
				if (IsNotAbsoluteUri)
				{
					throw new InvalidOperationException(SR.GetString("net_uri_NotAbsolute"));
				}
				return GetParts(UriComponents.Host | UriComponents.Port, UriFormat.UriEscaped);
			}
		}

		public string Host
		{
			get
			{
				if (IsNotAbsoluteUri)
				{
					throw new InvalidOperationException(SR.GetString("net_uri_NotAbsolute"));
				}
				return GetParts(UriComponents.Host, UriFormat.UriEscaped);
			}
		}

		public UriHostNameType HostNameType
		{
			get
			{
				if (IsNotAbsoluteUri)
				{
					throw new InvalidOperationException(SR.GetString("net_uri_NotAbsolute"));
				}
				if (m_Syntax.IsSimple)
				{
					EnsureUriInfo();
				}
				else
				{
					EnsureHostString(allowDnsOptimization: false);
				}
				return HostType switch
				{
					Flags.DnsHostType => UriHostNameType.Dns, 
					Flags.IPv4HostType => UriHostNameType.IPv4, 
					Flags.IPv6HostType => UriHostNameType.IPv6, 
					Flags.BasicHostType => UriHostNameType.Basic, 
					Flags.UncHostType => UriHostNameType.Basic, 
					Flags.HostTypeMask => UriHostNameType.Unknown, 
					_ => UriHostNameType.Unknown, 
				};
			}
		}

		public bool IsDefaultPort
		{
			get
			{
				if (IsNotAbsoluteUri)
				{
					throw new InvalidOperationException(SR.GetString("net_uri_NotAbsolute"));
				}
				if (m_Syntax.IsSimple)
				{
					EnsureUriInfo();
				}
				else
				{
					EnsureHostString(allowDnsOptimization: false);
				}
				return NotAny(Flags.NotDefaultPort);
			}
		}

		public bool IsFile
		{
			get
			{
				if (IsNotAbsoluteUri)
				{
					throw new InvalidOperationException(SR.GetString("net_uri_NotAbsolute"));
				}
				return (object)m_Syntax.SchemeName == UriSchemeFile;
			}
		}

		public bool IsLoopback
		{
			get
			{
				if (IsNotAbsoluteUri)
				{
					throw new InvalidOperationException(SR.GetString("net_uri_NotAbsolute"));
				}
				EnsureHostString(allowDnsOptimization: false);
				return InFact(Flags.LoopbackHost);
			}
		}

		public bool IsUnc
		{
			get
			{
				if (IsNotAbsoluteUri)
				{
					throw new InvalidOperationException(SR.GetString("net_uri_NotAbsolute"));
				}
				return IsUncPath;
			}
		}

		public string LocalPath
		{
			get
			{
				if (IsNotAbsoluteUri)
				{
					throw new InvalidOperationException(SR.GetString("net_uri_NotAbsolute"));
				}
				return GetLocalPath();
			}
		}

		internal static object InitializeLock
		{
			get
			{
				if (s_initLock == null)
				{
					object value = new object();
					Interlocked.CompareExchange(ref s_initLock, value, null);
				}
				return s_initLock;
			}
		}

		public string PathAndQuery
		{
			get
			{
				if (IsNotAbsoluteUri)
				{
					throw new InvalidOperationException(SR.GetString("net_uri_NotAbsolute"));
				}
				string text = GetParts(UriComponents.PathAndQuery, UriFormat.UriEscaped);
				if (IsDosPath && text[0] == '/')
				{
					text = text.Substring(1);
				}
				return text;
			}
		}

		public int Port
		{
			get
			{
				if (IsNotAbsoluteUri)
				{
					throw new InvalidOperationException(SR.GetString("net_uri_NotAbsolute"));
				}
				if (m_Syntax.IsSimple)
				{
					EnsureUriInfo();
				}
				else
				{
					EnsureHostString(allowDnsOptimization: false);
				}
				if (InFact(Flags.NotDefaultPort))
				{
					return m_Info.Offset.PortValue;
				}
				return m_Syntax.DefaultPort;
			}
		}

		public string Query
		{
			get
			{
				if (IsNotAbsoluteUri)
				{
					throw new InvalidOperationException(SR.GetString("net_uri_NotAbsolute"));
				}
				UriInfo uriInfo = EnsureUriInfo();
				if (uriInfo.MoreInfo == null)
				{
					uriInfo.MoreInfo = new MoreInfo();
				}
				string text = uriInfo.MoreInfo.Query;
				if (text == null)
				{
					text = GetParts(UriComponents.Query | UriComponents.KeepDelimiter, UriFormat.UriEscaped);
					uriInfo.MoreInfo.Query = text;
				}
				return text;
			}
		}

		public string Fragment
		{
			get
			{
				if (IsNotAbsoluteUri)
				{
					throw new InvalidOperationException(SR.GetString("net_uri_NotAbsolute"));
				}
				UriInfo uriInfo = EnsureUriInfo();
				if (uriInfo.MoreInfo == null)
				{
					uriInfo.MoreInfo = new MoreInfo();
				}
				string text = uriInfo.MoreInfo.Fragment;
				if (text == null)
				{
					text = GetParts(UriComponents.Fragment | UriComponents.KeepDelimiter, UriFormat.UriEscaped);
					uriInfo.MoreInfo.Fragment = text;
				}
				return text;
			}
		}

		public string Scheme
		{
			get
			{
				if (IsNotAbsoluteUri)
				{
					throw new InvalidOperationException(SR.GetString("net_uri_NotAbsolute"));
				}
				return m_Syntax.SchemeName;
			}
		}

		private bool OriginalStringSwitched
		{
			get
			{
				if (!m_iriParsing || !InFact(Flags.HasUnicode))
				{
					if (AllowIdn)
					{
						if (!InFact(Flags.IdnHost))
						{
							return InFact(Flags.UnicodeHost);
						}
						return true;
					}
					return false;
				}
				return true;
			}
		}

		public string OriginalString
		{
			get
			{
				if (!OriginalStringSwitched)
				{
					return m_String;
				}
				return m_originalUnicodeString;
			}
		}

		public string DnsSafeHost
		{
			get
			{
				if (IsNotAbsoluteUri)
				{
					throw new InvalidOperationException(SR.GetString("net_uri_NotAbsolute"));
				}
				if (AllowIdn && ((m_Flags & Flags.IdnHost) != 0 || (m_Flags & Flags.UnicodeHost) != 0))
				{
					EnsureUriInfo();
					return m_Info.DnsSafeHost;
				}
				EnsureHostString(allowDnsOptimization: false);
				string text = m_Info.Host;
				if (HostType == Flags.IPv6HostType)
				{
					text = text.Substring(1, text.Length - 2);
					if (m_Info.ScopeId != null)
					{
						text += m_Info.ScopeId;
					}
				}
				else if (HostType == Flags.BasicHostType && InFact(Flags.HostNotCanonical | Flags.E_HostNotCanonical))
				{
					char[] array = new char[text.Length];
					int destPosition = 0;
					UnescapeString(text, 0, text.Length, array, ref destPosition, '\uffff', '\uffff', '\uffff', UnescapeMode.CopyOnly, m_Syntax, isQuery: false, readOnlyConfig: false);
					text = new string(array, 0, destPosition);
				}
				return text;
			}
		}

		public bool IsAbsoluteUri => m_Syntax != null;

		public string[] Segments
		{
			get
			{
				if (IsNotAbsoluteUri)
				{
					throw new InvalidOperationException(SR.GetString("net_uri_NotAbsolute"));
				}
				string[] array = null;
				if (array == null)
				{
					string privateAbsolutePath = PrivateAbsolutePath;
					if (privateAbsolutePath.Length == 0)
					{
						array = new string[0];
					}
					else
					{
						ArrayList arrayList = new ArrayList();
						int num = 0;
						while (num < privateAbsolutePath.Length)
						{
							int num2 = privateAbsolutePath.IndexOf('/', num);
							if (num2 == -1)
							{
								num2 = privateAbsolutePath.Length - 1;
							}
							arrayList.Add(privateAbsolutePath.Substring(num, num2 - num + 1));
							num = num2 + 1;
						}
						array = (string[])arrayList.ToArray(typeof(string));
					}
				}
				return array;
			}
		}

		public bool UserEscaped => InFact(Flags.UserEscaped);

		public string UserInfo
		{
			get
			{
				if (IsNotAbsoluteUri)
				{
					throw new InvalidOperationException(SR.GetString("net_uri_NotAbsolute"));
				}
				return GetParts(UriComponents.UserInfo, UriFormat.UriEscaped);
			}
		}

		internal bool HasAuthority => InFact(Flags.AuthorityFound);

		private static bool IriParsingStatic(UriParser syntax)
		{
			if (s_IriParsing)
			{
				if (syntax == null || !syntax.InFact(UriSyntaxFlags.AllowIriParsing))
				{
					return syntax == null;
				}
				return true;
			}
			return false;
		}

		private bool AllowIdnStatic(UriParser syntax, Flags flags)
		{
			if (syntax != null && (syntax.Flags & UriSyntaxFlags.AllowIdn) != 0)
			{
				if (s_IdnScope != UriIdnScope.All)
				{
					if (s_IdnScope == UriIdnScope.AllExceptIntranet)
					{
						return StaticNotAny(flags, Flags.IntranetUri);
					}
					return false;
				}
				return true;
			}
			return false;
		}

		private bool IsIntranet(string schemeHost)
		{
			bool flag = false;
			int pdwZone = -1;
			int num = -2147467259;
			if (m_Syntax.SchemeName.Length > 32)
			{
				return false;
			}
			if (s_ManagerRef == null)
			{
				lock (s_IntranetLock)
				{
					if (s_ManagerRef == null)
					{
						s_ManagerRef = (IInternetSecurityManager)new InternetSecurityManager();
					}
				}
			}
			try
			{
				s_ManagerRef.MapUrlToZone(schemeHost.TrimStart(_WSchars), out pdwZone, 0);
			}
			catch (COMException ex)
			{
				if (ex.ErrorCode == num)
				{
					flag = true;
				}
			}
			switch (pdwZone)
			{
			case 1:
				return true;
			default:
				if (!flag)
				{
					return false;
				}
				goto case 2;
			case 2:
			case 4:
			{
				for (int i = 0; i < schemeHost.Length; i++)
				{
					if (schemeHost[i] == '.')
					{
						return false;
					}
				}
				return true;
			}
			}
		}

		private void SetUserDrivenParsing()
		{
			m_Flags = Flags.UserDrivenParsing | (m_Flags & Flags.UserEscaped);
		}

		private bool NotAny(Flags flags)
		{
			return (m_Flags & flags) == 0;
		}

		private bool InFact(Flags flags)
		{
			return (m_Flags & flags) != 0;
		}

		private static bool StaticNotAny(Flags allFlags, Flags checkFlags)
		{
			return (allFlags & checkFlags) == 0;
		}

		private static bool StaticInFact(Flags allFlags, Flags checkFlags)
		{
			return (allFlags & checkFlags) != 0;
		}

		private UriInfo EnsureUriInfo()
		{
			Flags flags = m_Flags;
			if ((m_Flags & Flags.MinimalUriInfoSet) == 0)
			{
				CreateUriInfo(flags);
			}
			return m_Info;
		}

		private void EnsureParseRemaining()
		{
			if ((m_Flags & Flags.AllUriInfoSet) == 0)
			{
				ParseRemaining();
			}
		}

		private void EnsureHostString(bool allowDnsOptimization)
		{
			EnsureUriInfo();
			if (m_Info.Host == null && (!allowDnsOptimization || !InFact(Flags.CanonicalDnsHost)))
			{
				CreateHostString();
			}
		}

		public Uri(string uriString)
		{
			if (uriString == null)
			{
				throw new ArgumentNullException("uriString");
			}
			CreateThis(uriString, dontEscape: false, UriKind.Absolute);
		}

		[Obsolete("The constructor has been deprecated. Please use new Uri(string). The dontEscape parameter is deprecated and is always false. http://go.microsoft.com/fwlink/?linkid=14202")]
		public Uri(string uriString, bool dontEscape)
		{
			if (uriString == null)
			{
				throw new ArgumentNullException("uriString");
			}
			CreateThis(uriString, dontEscape, UriKind.Absolute);
		}

		public Uri(string uriString, UriKind uriKind)
		{
			if (uriString == null)
			{
				throw new ArgumentNullException("uriString");
			}
			CreateThis(uriString, dontEscape: false, uriKind);
		}

		public Uri(Uri baseUri, string relativeUri)
		{
			if ((object)baseUri == null)
			{
				throw new ArgumentNullException("baseUri");
			}
			if (!baseUri.IsAbsoluteUri)
			{
				throw new ArgumentOutOfRangeException("baseUri");
			}
			CreateUri(baseUri, relativeUri, dontEscape: false);
		}

		[Obsolete("The constructor has been deprecated. Please new Uri(Uri, string). The dontEscape parameter is deprecated and is always false. http://go.microsoft.com/fwlink/?linkid=14202")]
		public Uri(Uri baseUri, string relativeUri, bool dontEscape)
		{
			if ((object)baseUri == null)
			{
				throw new ArgumentNullException("baseUri");
			}
			if (!baseUri.IsAbsoluteUri)
			{
				throw new ArgumentOutOfRangeException("baseUri");
			}
			CreateUri(baseUri, relativeUri, dontEscape);
		}

		private void CreateUri(Uri baseUri, string relativeUri, bool dontEscape)
		{
			CreateThis(relativeUri, dontEscape, UriKind.RelativeOrAbsolute);
			UriFormatException e;
			if (baseUri.Syntax.IsSimple)
			{
				Uri uri = ResolveHelper(baseUri, this, ref relativeUri, ref dontEscape, out e);
				if (e != null)
				{
					throw e;
				}
				if (uri != null)
				{
					if ((object)uri != this)
					{
						CreateThisFromUri(uri);
					}
					return;
				}
			}
			else
			{
				dontEscape = false;
				relativeUri = baseUri.Syntax.InternalResolve(baseUri, this, out e);
				if (e != null)
				{
					throw e;
				}
			}
			m_Flags = Flags.Zero;
			m_Info = null;
			m_Syntax = null;
			CreateThis(relativeUri, dontEscape, UriKind.Absolute);
		}

		public Uri(Uri baseUri, Uri relativeUri)
		{
			if ((object)baseUri == null)
			{
				throw new ArgumentNullException("baseUri");
			}
			if (!baseUri.IsAbsoluteUri)
			{
				throw new ArgumentOutOfRangeException("baseUri");
			}
			CreateThisFromUri(relativeUri);
			string newUriString = null;
			bool userEscaped;
			UriFormatException e;
			if (baseUri.Syntax.IsSimple)
			{
				userEscaped = InFact(Flags.UserEscaped);
				relativeUri = ResolveHelper(baseUri, this, ref newUriString, ref userEscaped, out e);
				if (e != null)
				{
					throw e;
				}
				if (relativeUri != null)
				{
					if ((object)relativeUri != this)
					{
						CreateThisFromUri(relativeUri);
					}
					return;
				}
			}
			else
			{
				userEscaped = false;
				newUriString = baseUri.Syntax.InternalResolve(baseUri, this, out e);
				if (e != null)
				{
					throw e;
				}
			}
			m_Flags = Flags.Zero;
			m_Info = null;
			m_Syntax = null;
			CreateThis(newUriString, userEscaped, UriKind.Absolute);
		}

		protected Uri(SerializationInfo serializationInfo, StreamingContext streamingContext)
		{
			string @string = serializationInfo.GetString("AbsoluteUri");
			if (@string.Length != 0)
			{
				CreateThis(@string, dontEscape: false, UriKind.Absolute);
				return;
			}
			@string = serializationInfo.GetString("RelativeUri");
			if (@string == null)
			{
				throw new ArgumentNullException("uriString");
			}
			CreateThis(@string, dontEscape: false, UriKind.Relative);
		}

		private unsafe static ParsingError GetCombinedString(Uri baseUri, string relativeStr, bool dontEscape, ref string result)
		{
			for (int i = 0; i < relativeStr.Length && relativeStr[i] != '/' && relativeStr[i] != '\\' && relativeStr[i] != '?' && relativeStr[i] != '#'; i++)
			{
				if (relativeStr[i] != ':')
				{
					continue;
				}
				if (i < 2)
				{
					break;
				}
				string text = relativeStr.Substring(0, i);
				fixed (char* ptr = text)
				{
					UriParser syntax = null;
					if (CheckSchemeSyntax(ptr, (ushort)text.Length, ref syntax) == ParsingError.None)
					{
						if (baseUri.Syntax != syntax)
						{
							result = relativeStr;
							return ParsingError.None;
						}
						relativeStr = ((i + 1 >= relativeStr.Length) ? string.Empty : relativeStr.Substring(i + 1));
					}
				}
				break;
			}
			if (relativeStr.Length == 0)
			{
				result = baseUri.OriginalString;
				return ParsingError.None;
			}
			result = CombineUri(baseUri, relativeStr, dontEscape ? UriFormat.UriEscaped : UriFormat.SafeUnescaped);
			return ParsingError.None;
		}

		private static UriFormatException GetException(ParsingError err)
		{
			return err switch
			{
				ParsingError.None => null, 
				ParsingError.BadFormat => ExceptionHelper.BadFormatException, 
				ParsingError.BadScheme => ExceptionHelper.BadSchemeException, 
				ParsingError.BadAuthority => ExceptionHelper.BadAuthorityException, 
				ParsingError.EmptyUriString => ExceptionHelper.EmptyUriException, 
				ParsingError.SchemeLimit => ExceptionHelper.SchemeLimitException, 
				ParsingError.SizeLimit => ExceptionHelper.SizeLimitException, 
				ParsingError.MustRootedPath => ExceptionHelper.MustRootedPathException, 
				ParsingError.BadHostName => ExceptionHelper.BadHostNameException, 
				ParsingError.NonEmptyHost => ExceptionHelper.BadFormatException, 
				ParsingError.BadPort => ExceptionHelper.BadPortException, 
				ParsingError.BadAuthorityTerminator => ExceptionHelper.BadAuthorityTerminatorException, 
				ParsingError.CannotCreateRelative => ExceptionHelper.CannotCreateRelativeException, 
				_ => ExceptionHelper.BadFormatException, 
			};
		}

		[SecurityPermission(SecurityAction.LinkDemand, SerializationFormatter = true)]
		void ISerializable.GetObjectData(SerializationInfo serializationInfo, StreamingContext streamingContext)
		{
			GetObjectData(serializationInfo, streamingContext);
		}

		[SecurityPermission(SecurityAction.LinkDemand, SerializationFormatter = true)]
		protected void GetObjectData(SerializationInfo serializationInfo, StreamingContext streamingContext)
		{
			if (IsAbsoluteUri)
			{
				serializationInfo.AddValue("AbsoluteUri", GetParts(UriComponents.SerializationInfoString, UriFormat.UriEscaped));
				return;
			}
			serializationInfo.AddValue("AbsoluteUri", string.Empty);
			serializationInfo.AddValue("RelativeUri", GetParts(UriComponents.SerializationInfoString, UriFormat.UriEscaped));
		}

		private static bool StaticIsFile(UriParser syntax)
		{
			return syntax.InFact(UriSyntaxFlags.FileLikeUri);
		}

		private static void InitializeUriConfig()
		{
			if (s_ConfigInitialized)
			{
				return;
			}
			lock (InitializeLock)
			{
				if (!s_ConfigInitialized)
				{
					s_ConfigInitialized = true;
					GetConfig(ref s_IdnScope, ref s_IriParsing);
				}
			}
		}

		private static void GetConfig(ref UriIdnScope idnScope, ref bool iriParsing)
		{
			string text = null;
			new FileIOPermission(PermissionState.Unrestricted).Assert();
			try
			{
				text = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
			}
			finally
			{
				CodeAccessPermission.RevertAssert();
			}
			if (IsWebConfig(text))
			{
				try
				{
					UriSectionInternal section = UriSectionInternal.GetSection();
					if (section != null)
					{
						idnScope = section.Idn;
						iriParsing = section.IriParsing;
					}
				}
				catch (ConfigurationException)
				{
				}
				return;
			}
			string path = null;
			new FileIOPermission(PermissionState.Unrestricted).Assert();
			try
			{
				path = RuntimeEnvironment.GetRuntimeDirectory();
			}
			finally
			{
				CodeAccessPermission.RevertAssert();
			}
			string file = Path.Combine(Path.Combine(path, "Config"), "machine.config");
			ParseConfigFile(file, out var idnStateConfig, out var iriParsingConfig);
			ParseConfigFile(text, out var idnStateConfig2, out var iriParsingConfig2);
			switch (idnStateConfig2)
			{
			case IdnScopeFromConfig.None:
				idnScope = UriIdnScope.None;
				break;
			case IdnScopeFromConfig.AllExceptIntranet:
				idnScope = UriIdnScope.AllExceptIntranet;
				break;
			case IdnScopeFromConfig.All:
				idnScope = UriIdnScope.All;
				break;
			default:
				switch (idnStateConfig)
				{
				case IdnScopeFromConfig.None:
					idnScope = UriIdnScope.None;
					break;
				case IdnScopeFromConfig.AllExceptIntranet:
					idnScope = UriIdnScope.AllExceptIntranet;
					break;
				case IdnScopeFromConfig.All:
					idnScope = UriIdnScope.All;
					break;
				default:
					idnScope = UriIdnScope.None;
					break;
				}
				break;
			}
			switch (iriParsingConfig2)
			{
			case IriParsingFromConfig.False:
				iriParsing = false;
				return;
			case IriParsingFromConfig.True:
				iriParsing = true;
				return;
			}
			switch (iriParsingConfig)
			{
			case IriParsingFromConfig.False:
				iriParsing = false;
				break;
			case IriParsingFromConfig.True:
				iriParsing = true;
				break;
			default:
				iriParsing = false;
				break;
			}
		}

		private static bool IsWebConfig(string appConfigFile)
		{
			string text = AppDomain.CurrentDomain.GetData(".appVPath") as string;
			if (text != null)
			{
				return true;
			}
			if (appConfigFile != null && (appConfigFile.StartsWith("http://", ignoreCase: true, CultureInfo.InvariantCulture) || appConfigFile.StartsWith("https://", ignoreCase: true, CultureInfo.InvariantCulture)))
			{
				return true;
			}
			return false;
		}

		private static void ParseConfigFile(string file, out IdnScopeFromConfig idnStateConfig, out IriParsingFromConfig iriParsingConfig)
		{
			idnStateConfig = IdnScopeFromConfig.NotFound;
			iriParsingConfig = IriParsingFromConfig.NotFound;
			new FileIOPermission(FileIOPermissionAccess.Read, file).Assert();
			try
			{
				FileStream fileStream = new FileStream(file, FileMode.Open, FileAccess.Read);
				using (fileStream)
				{
					XmlReaderSettings xmlReaderSettings = new XmlReaderSettings();
					xmlReaderSettings.IgnoreComments = true;
					xmlReaderSettings.IgnoreWhitespace = true;
					xmlReaderSettings.IgnoreProcessingInstructions = true;
					XmlReader xmlReader = XmlReader.Create(fileStream, xmlReaderSettings);
					using (xmlReader)
					{
						if (!xmlReader.ReadToFollowing("configuration") || !xmlReader.ReadToFollowing("uri"))
						{
							return;
						}
						while (xmlReader.NodeType != XmlNodeType.EndElement || !ConfigStringEqual(xmlReader.Name, "uri"))
						{
							if (xmlReader.NodeType == XmlNodeType.Element)
							{
								if (ConfigStringEqual(xmlReader.Name, "idn"))
								{
									string attribute = xmlReader.GetAttribute("enabled");
									if (attribute != null)
									{
										if (ConfigStringEqual(attribute, "None"))
										{
											idnStateConfig = IdnScopeFromConfig.None;
										}
										else if (ConfigStringEqual(attribute, "AllExceptIntranet"))
										{
											idnStateConfig = IdnScopeFromConfig.AllExceptIntranet;
										}
										else if (ConfigStringEqual(attribute, "All"))
										{
											idnStateConfig = IdnScopeFromConfig.All;
										}
										else
										{
											idnStateConfig = IdnScopeFromConfig.Invalid;
										}
									}
								}
								else if (ConfigStringEqual(xmlReader.Name, "iriParsing"))
								{
									string attribute = xmlReader.GetAttribute("enabled");
									if (attribute != null)
									{
										if (ConfigStringEqual(attribute, "false"))
										{
											iriParsingConfig = IriParsingFromConfig.False;
										}
										else if (ConfigStringEqual(attribute, "true"))
										{
											iriParsingConfig = IriParsingFromConfig.True;
										}
										else
										{
											iriParsingConfig = IriParsingFromConfig.Invalid;
										}
									}
								}
							}
							if (!xmlReader.Read())
							{
								break;
							}
						}
					}
				}
			}
			catch (Exception exception)
			{
				if (NclUtilities.IsFatal(exception))
				{
					throw;
				}
			}
			finally
			{
				CodeAccessPermission.RevertAssert();
			}
		}

		private static bool ConfigStringEqual(string string1, string string2)
		{
			if (string.Compare(string1, string2, StringComparison.OrdinalIgnoreCase) == 0)
			{
				return true;
			}
			return false;
		}

		private string GetLocalPath()
		{
			EnsureParseRemaining();
			if (IsUncOrDosPath)
			{
				EnsureHostString(allowDnsOptimization: false);
				int num;
				if (NotAny(Flags.HostNotCanonical | Flags.PathNotCanonical | Flags.ShouldBeCompressed))
				{
					num = (IsUncPath ? (m_Info.Offset.Host - 2) : m_Info.Offset.Path);
					string text = ((IsImplicitFile && m_Info.Offset.Host == ((!IsDosPath) ? 2 : 0) && m_Info.Offset.Query == m_Info.Offset.End) ? m_String : ((IsDosPath && (m_String[num] == '/' || m_String[num] == '\\')) ? m_String.Substring(num + 1, m_Info.Offset.Query - num - 1) : m_String.Substring(num, m_Info.Offset.Query - num)));
					if (IsDosPath && text[1] == '|')
					{
						text = text.Remove(1, 1);
						text = text.Insert(1, ":");
					}
					for (int i = 0; i < text.Length; i++)
					{
						if (text[i] == '/')
						{
							text = text.Replace('/', '\\');
							break;
						}
					}
					return text;
				}
				int destPosition = 0;
				num = m_Info.Offset.Path;
				string host = m_Info.Host;
				char[] array = new char[host.Length + 3 + m_Info.Offset.Fragment - m_Info.Offset.Path];
				if (IsUncPath)
				{
					array[0] = '\\';
					array[1] = '\\';
					destPosition = 2;
					UnescapeString(host, 0, host.Length, array, ref destPosition, '\uffff', '\uffff', '\uffff', UnescapeMode.CopyOnly, m_Syntax, isQuery: false, readOnlyConfig: false);
				}
				else if (m_String[num] == '/' || m_String[num] == '\\')
				{
					num++;
				}
				ushort num2 = (ushort)destPosition;
				UnescapeMode unescapeMode = ((InFact(Flags.PathNotCanonical) && !IsImplicitFile) ? (UnescapeMode.Unescape | UnescapeMode.UnescapeAll) : UnescapeMode.CopyOnly);
				UnescapeString(m_String, num, m_Info.Offset.Query, array, ref destPosition, '\uffff', '\uffff', '\uffff', unescapeMode, m_Syntax, isQuery: true, readOnlyConfig: false);
				if (array[1] == '|')
				{
					array[1] = ':';
				}
				if (InFact(Flags.ShouldBeCompressed))
				{
					array = Compress(array, (ushort)(IsDosPath ? (num2 + 2) : num2), ref destPosition, m_Syntax);
				}
				for (ushort num3 = 0; num3 < (ushort)destPosition; num3 = (ushort)(num3 + 1))
				{
					if (array[num3] == '/')
					{
						array[num3] = '\\';
					}
				}
				return new string(array, 0, destPosition);
			}
			return GetUnescapedParts(UriComponents.Path | UriComponents.KeepDelimiter, UriFormat.Unescaped);
		}

		public unsafe static UriHostNameType CheckHostName(string name)
		{
			if (name == null || name.Length == 0 || name.Length > 32767)
			{
				return UriHostNameType.Unknown;
			}
			int end = name.Length;
			fixed (char* name2 = name)
			{
				if (name[0] == '[' && name[name.Length - 1] == ']' && IPv6AddressHelper.IsValid(name2, 1, ref end) && end == name.Length)
				{
					return UriHostNameType.IPv6;
				}
				end = name.Length;
				if (IPv4AddressHelper.IsValid(name2, 0, ref end, allowIPv6: false, notImplicitFile: false) && end == name.Length)
				{
					return UriHostNameType.IPv4;
				}
				end = name.Length;
				bool notCanonical = false;
				if (DomainNameHelper.IsValid(name2, 0, ref end, ref notCanonical, notImplicitFile: false) && end == name.Length)
				{
					return UriHostNameType.Dns;
				}
			}
			end = name.Length + 2;
			name = "[" + name + "]";
			fixed (char* name3 = name)
			{
				if (IPv6AddressHelper.IsValid(name3, 1, ref end) && end == name.Length)
				{
					return UriHostNameType.IPv6;
				}
			}
			return UriHostNameType.Unknown;
		}

		public static bool CheckSchemeName(string schemeName)
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

		public static int FromHex(char digit)
		{
			if ((digit >= '0' && digit <= '9') || (digit >= 'A' && digit <= 'F') || (digit >= 'a' && digit <= 'f'))
			{
				if (digit > '9')
				{
					return ((digit <= 'F') ? (digit - 65) : (digit - 97)) + 10;
				}
				return digit - 48;
			}
			throw new ArgumentException("digit");
		}

		[SecurityPermission(SecurityAction.InheritanceDemand, Flags = SecurityPermissionFlag.Infrastructure)]
		public override int GetHashCode()
		{
			if (IsNotAbsoluteUri)
			{
				return CalculateCaseInsensitiveHashCode(OriginalString);
			}
			UriInfo uriInfo = EnsureUriInfo();
			if (uriInfo.MoreInfo == null)
			{
				uriInfo.MoreInfo = new MoreInfo();
			}
			int num = uriInfo.MoreInfo.Hash;
			if (num == 0)
			{
				string text = uriInfo.MoreInfo.RemoteUrl;
				if (text == null)
				{
					text = GetParts(UriComponents.HttpRequestUrl, UriFormat.SafeUnescaped);
				}
				num = CalculateCaseInsensitiveHashCode(text);
				if (num == 0)
				{
					num = 16777216;
				}
				uriInfo.MoreInfo.Hash = num;
			}
			return num;
		}

		[SecurityPermission(SecurityAction.InheritanceDemand, Flags = SecurityPermissionFlag.Infrastructure)]
		public override string ToString()
		{
			if (m_Syntax == null)
			{
				if (!m_iriParsing || !InFact(Flags.HasUnicode))
				{
					return OriginalString;
				}
				return m_String;
			}
			EnsureUriInfo();
			if (m_Info.String == null)
			{
				if (Syntax.IsSimple)
				{
					m_Info.String = GetComponentsHelper(UriComponents.AbsoluteUri, (UriFormat)32767);
				}
				else
				{
					m_Info.String = GetParts(UriComponents.AbsoluteUri, UriFormat.SafeUnescaped);
				}
			}
			return m_Info.String;
		}

		[SecurityPermission(SecurityAction.InheritanceDemand, Flags = SecurityPermissionFlag.Infrastructure)]
		public static bool operator ==(Uri uri1, Uri uri2)
		{
			if ((object)uri1 == uri2)
			{
				return true;
			}
			if ((object)uri1 == null || (object)uri2 == null)
			{
				return false;
			}
			return uri2.Equals(uri1);
		}

		[SecurityPermission(SecurityAction.InheritanceDemand, Flags = SecurityPermissionFlag.Infrastructure)]
		public static bool operator !=(Uri uri1, Uri uri2)
		{
			if ((object)uri1 == uri2)
			{
				return false;
			}
			if ((object)uri1 == null || (object)uri2 == null)
			{
				return true;
			}
			return !uri2.Equals(uri1);
		}

		[SecurityPermission(SecurityAction.InheritanceDemand, Flags = SecurityPermissionFlag.Infrastructure)]
		public unsafe override bool Equals(object comparand)
		{
			if (comparand == null)
			{
				return false;
			}
			if (this == comparand)
			{
				return true;
			}
			Uri result = comparand as Uri;
			if ((object)result == null)
			{
				string text = comparand as string;
				if (text == null)
				{
					return false;
				}
				if (!TryCreate(text, UriKind.RelativeOrAbsolute, out result))
				{
					return false;
				}
			}
			if ((object)m_String == result.m_String)
			{
				return true;
			}
			if (IsAbsoluteUri != result.IsAbsoluteUri)
			{
				return false;
			}
			if (IsNotAbsoluteUri)
			{
				return OriginalString.Equals(result.OriginalString);
			}
			if (NotAny(Flags.AllUriInfoSet) || result.NotAny(Flags.AllUriInfoSet))
			{
				if (!IsUncOrDosPath)
				{
					if (m_String.Length == result.m_String.Length)
					{
						fixed (char* ptr = m_String)
						{
							fixed (char* ptr2 = result.m_String)
							{
								int num = m_String.Length - 1;
								while (num >= 0 && ptr[num] == ptr2[num])
								{
									num--;
								}
								if (num == -1)
								{
									return true;
								}
							}
						}
					}
				}
				else if (string.Compare(m_String, result.m_String, StringComparison.OrdinalIgnoreCase) == 0)
				{
					return true;
				}
			}
			EnsureUriInfo();
			result.EnsureUriInfo();
			if (!UserDrivenParsing && !result.UserDrivenParsing && Syntax.IsSimple && result.Syntax.IsSimple)
			{
				if (InFact(Flags.CanonicalDnsHost) && result.InFact(Flags.CanonicalDnsHost))
				{
					ushort num2 = m_Info.Offset.Host;
					ushort num3 = m_Info.Offset.Path;
					ushort num4 = result.m_Info.Offset.Host;
					ushort path = result.m_Info.Offset.Path;
					string @string = result.m_String;
					if (num3 - num2 > path - num4)
					{
						num3 = (ushort)(num2 + path - num4);
					}
					while (num2 < num3)
					{
						if (m_String[num2] != @string[num4])
						{
							return false;
						}
						if (@string[num4] == ':')
						{
							break;
						}
						num2 = (ushort)(num2 + 1);
						num4 = (ushort)(num4 + 1);
					}
					if (num2 < m_Info.Offset.Path && m_String[num2] != ':')
					{
						return false;
					}
					if (num4 < path && @string[num4] != ':')
					{
						return false;
					}
				}
				else
				{
					EnsureHostString(allowDnsOptimization: false);
					result.EnsureHostString(allowDnsOptimization: false);
					if (!m_Info.Host.Equals(result.m_Info.Host))
					{
						return false;
					}
				}
				if (Port != result.Port)
				{
					return false;
				}
			}
			UriInfo info = m_Info;
			UriInfo info2 = result.m_Info;
			if (info.MoreInfo == null)
			{
				info.MoreInfo = new MoreInfo();
			}
			if (info2.MoreInfo == null)
			{
				info2.MoreInfo = new MoreInfo();
			}
			string text2 = info.MoreInfo.RemoteUrl;
			if (text2 == null)
			{
				text2 = GetParts(UriComponents.HttpRequestUrl, UriFormat.SafeUnescaped);
				info.MoreInfo.RemoteUrl = text2;
			}
			string text3 = info2.MoreInfo.RemoteUrl;
			if (text3 == null)
			{
				text3 = result.GetParts(UriComponents.HttpRequestUrl, UriFormat.SafeUnescaped);
				info2.MoreInfo.RemoteUrl = text3;
			}
			if (!IsUncOrDosPath)
			{
				if (text2.Length != text3.Length)
				{
					return false;
				}
				fixed (char* ptr3 = text2)
				{
					fixed (char* ptr5 = text3)
					{
						char* ptr4 = ptr3 + text2.Length;
						char* ptr6 = ptr5 + text2.Length;
						while (ptr4 != ptr3)
						{
							if (*(--ptr4) != *(--ptr6))
							{
								return false;
							}
						}
						return true;
					}
				}
			}
			return string.Compare(info.MoreInfo.RemoteUrl, info2.MoreInfo.RemoteUrl, IsUncOrDosPath ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal) == 0;
		}

		public string GetLeftPart(UriPartial part)
		{
			if (IsNotAbsoluteUri)
			{
				throw new InvalidOperationException(SR.GetString("net_uri_NotAbsolute"));
			}
			EnsureUriInfo();
			switch (part)
			{
			case UriPartial.Scheme:
				return GetParts(UriComponents.Scheme | UriComponents.KeepDelimiter, UriFormat.UriEscaped);
			case UriPartial.Authority:
				if (NotAny(Flags.AuthorityFound) || IsDosPath)
				{
					return string.Empty;
				}
				return GetParts(UriComponents.SchemeAndServer | UriComponents.UserInfo, UriFormat.UriEscaped);
			case UriPartial.Path:
				return GetParts(UriComponents.SchemeAndServer | UriComponents.UserInfo | UriComponents.Path, UriFormat.UriEscaped);
			case UriPartial.Query:
				return GetParts(UriComponents.HttpRequestUrl | UriComponents.UserInfo, UriFormat.UriEscaped);
			default:
				throw new ArgumentException("part");
			}
		}

		public static string HexEscape(char character)
		{
			if (character > 'ÿ')
			{
				throw new ArgumentOutOfRangeException("character");
			}
			char[] array = new char[3];
			int pos = 0;
			EscapeAsciiChar(character, array, ref pos);
			return new string(array);
		}

		public static char HexUnescape(string pattern, ref int index)
		{
			if (index < 0 || index >= pattern.Length)
			{
				throw new ArgumentOutOfRangeException("index");
			}
			if (pattern[index] == '%' && pattern.Length - index >= 3)
			{
				char c = EscapedAscii(pattern[index + 1], pattern[index + 2]);
				if (c != '\uffff')
				{
					index += 3;
					return c;
				}
			}
			return pattern[index++];
		}

		public static bool IsHexDigit(char character)
		{
			if ((character < '0' || character > '9') && (character < 'A' || character > 'F'))
			{
				if (character >= 'a')
				{
					return character <= 'f';
				}
				return false;
			}
			return true;
		}

		public static bool IsHexEncoding(string pattern, int index)
		{
			if (pattern.Length - index < 3)
			{
				return false;
			}
			if (pattern[index] == '%' && EscapedAscii(pattern[index + 1], pattern[index + 1]) != '\uffff')
			{
				return true;
			}
			return false;
		}

		[Obsolete("The method has been deprecated. Please use MakeRelativeUri(Uri uri). http://go.microsoft.com/fwlink/?linkid=14202")]
		public string MakeRelative(Uri toUri)
		{
			if (IsNotAbsoluteUri || toUri.IsNotAbsoluteUri)
			{
				throw new InvalidOperationException(SR.GetString("net_uri_NotAbsolute"));
			}
			if (Scheme == toUri.Scheme && Host == toUri.Host && Port == toUri.Port)
			{
				return PathDifference(AbsolutePath, toUri.AbsolutePath, !IsUncOrDosPath);
			}
			return toUri.ToString();
		}

		public Uri MakeRelativeUri(Uri uri)
		{
			if (IsNotAbsoluteUri || uri.IsNotAbsoluteUri)
			{
				throw new InvalidOperationException(SR.GetString("net_uri_NotAbsolute"));
			}
			if (Scheme == uri.Scheme && Host == uri.Host && Port == uri.Port)
			{
				return new Uri(PathDifference(AbsolutePath, uri.AbsolutePath, !IsUncOrDosPath) + uri.GetParts(UriComponents.Query | UriComponents.Fragment, UriFormat.UriEscaped), UriKind.Relative);
			}
			return uri;
		}

		private unsafe static bool TestForSubPath(char* pMe, ushort meLength, char* pShe, ushort sheLength, bool ignoreCase)
		{
			ushort num = 0;
			bool flag = true;
			for (; num < meLength && num < sheLength; num = (ushort)(num + 1))
			{
				char c = pMe[(int)num];
				char c2 = pShe[(int)num];
				switch (c)
				{
				case '#':
				case '?':
					return true;
				case '/':
					if (c2 != '/')
					{
						return false;
					}
					if (!flag)
					{
						return false;
					}
					flag = true;
					continue;
				default:
					if (c2 == '?' || c2 == '#')
					{
						break;
					}
					if (!ignoreCase)
					{
						if (c != c2)
						{
							flag = false;
						}
					}
					else if (char.ToLower(c, CultureInfo.InvariantCulture) != char.ToLower(c2, CultureInfo.InvariantCulture))
					{
						flag = false;
					}
					continue;
				}
				break;
			}
			for (; num < meLength; num = (ushort)(num + 1))
			{
				char c;
				if ((c = pMe[(int)num]) != '?')
				{
					switch (c)
					{
					case '#':
						break;
					case '/':
						return false;
					default:
						continue;
					}
				}
				return true;
			}
			return true;
		}

		internal static string InternalEscapeString(string rawString)
		{
			if (rawString == null)
			{
				return string.Empty;
			}
			int destPos = 0;
			char[] array = EscapeString(rawString, 0, rawString.Length, null, ref destPos, isUriString: true, '?', '#', '%');
			if (array == null)
			{
				return rawString;
			}
			return new string(array, 0, destPos);
		}

		private unsafe static ParsingError ParseScheme(string uriString, ref Flags flags, ref UriParser syntax)
		{
			int length = uriString.Length;
			if (length == 0)
			{
				return ParsingError.EmptyUriString;
			}
			if (length >= 65520)
			{
				return ParsingError.SizeLimit;
			}
			fixed (char* uriString2 = uriString)
			{
				ParsingError err = ParsingError.None;
				ushort num = ParseSchemeCheckImplicitFile(uriString2, (ushort)length, ref err, ref flags, ref syntax);
				if (err != 0)
				{
					return err;
				}
				flags |= (Flags)num;
			}
			return ParsingError.None;
		}

		internal UriFormatException ParseMinimal()
		{
			ParsingError parsingError = PrivateParseMinimal();
			if (parsingError == ParsingError.None)
			{
				return null;
			}
			m_Flags |= Flags.ErrorOrParsingRecursion;
			return GetException(parsingError);
		}

		private unsafe ParsingError PrivateParseMinimal()
		{
			ushort num = (ushort)(m_Flags & Flags.IndexMask);
			ushort num2 = (ushort)m_String.Length;
			string newHost = null;
			m_Flags &= ~(Flags.IndexMask | Flags.UserDrivenParsing);
			fixed (char* ptr = ((m_iriParsing && (m_Flags & Flags.HasUnicode) != 0 && (m_Flags & Flags.HostUnicodeNormalized) == 0) ? m_originalUnicodeString : m_String))
			{
				if (num2 > num && IsLWS(ptr[num2 - 1]))
				{
					num2 = (ushort)(num2 - 1);
					while (num2 != num && IsLWS(ptr[(int)(num2 = (ushort)(num2 - 1))]))
					{
					}
					num2 = (ushort)(num2 + 1);
				}
				if (m_Syntax.IsAllSet(UriSyntaxFlags.AllowEmptyHost | UriSyntaxFlags.AllowDOSPath) && NotAny(Flags.ImplicitFile) && num + 1 < num2)
				{
					ushort num3 = num;
					char c;
					while (num3 < num2 && ((c = ptr[(int)num3]) == '\\' || c == '/'))
					{
						num3 = (ushort)(num3 + 1);
					}
					if (m_Syntax.InFact(UriSyntaxFlags.FileLikeUri) || num3 - num <= 3)
					{
						if (num3 - num >= 2)
						{
							m_Flags |= Flags.AuthorityFound;
						}
						if (num3 + 1 < num2 && ((c = ptr[num3 + 1]) == ':' || c == '|') && IsAsciiLetter(ptr[(int)num3]))
						{
							if (num3 + 2 >= num2 || ((c = ptr[num3 + 2]) != '\\' && c != '/'))
							{
								if (m_Syntax.InFact(UriSyntaxFlags.FileLikeUri))
								{
									return ParsingError.MustRootedPath;
								}
							}
							else
							{
								m_Flags |= Flags.DosPath;
								if (m_Syntax.InFact(UriSyntaxFlags.MustHaveAuthority))
								{
									m_Flags |= Flags.AuthorityFound;
								}
								num = ((num3 == num || num3 - num == 2) ? num3 : ((ushort)(num3 - 1)));
							}
						}
						else if (m_Syntax.InFact(UriSyntaxFlags.FileLikeUri) && num3 - num >= 2 && num3 - num != 3 && num3 < num2 && ptr[(int)num3] != '?' && ptr[(int)num3] != '#')
						{
							m_Flags |= Flags.UncPath;
							num = num3;
						}
					}
				}
				if ((m_Flags & (Flags.DosPath | Flags.UncPath)) == 0)
				{
					if (num + 2 <= num2)
					{
						char c2 = ptr[(int)num];
						char c3 = ptr[num + 1];
						if (m_Syntax.InFact(UriSyntaxFlags.MustHaveAuthority))
						{
							if ((c2 != '/' && c2 != '\\') || (c3 != '/' && c3 != '\\'))
							{
								return ParsingError.BadAuthority;
							}
							m_Flags |= Flags.AuthorityFound;
							num = (ushort)(num + 2);
						}
						else if (m_Syntax.InFact(UriSyntaxFlags.OptionalAuthority) && (InFact(Flags.AuthorityFound) || (c2 == '/' && c3 == '/')))
						{
							m_Flags |= Flags.AuthorityFound;
							num = (ushort)(num + 2);
						}
						else if (m_Syntax.NotAny(UriSyntaxFlags.MailToLikeUri))
						{
							m_Flags |= (Flags)((ulong)num | 0x70000uL);
							return ParsingError.None;
						}
					}
					else
					{
						if (m_Syntax.InFact(UriSyntaxFlags.MustHaveAuthority))
						{
							return ParsingError.BadAuthority;
						}
						if (m_Syntax.NotAny(UriSyntaxFlags.MailToLikeUri))
						{
							m_Flags |= (Flags)((ulong)num | 0x70000uL);
							return ParsingError.None;
						}
					}
				}
				if (InFact(Flags.DosPath))
				{
					m_Flags |= (Flags)(((m_Flags & Flags.AuthorityFound) != 0) ? 327680 : 458752);
					m_Flags |= (Flags)num;
					return ParsingError.None;
				}
				ParsingError err = ParsingError.None;
				num = CheckAuthorityHelper(ptr, num, num2, ref err, ref m_Flags, m_Syntax, ref newHost);
				if (err != 0)
				{
					return err;
				}
				if (num < num2 && ptr[(int)num] == '\\' && NotAny(Flags.ImplicitFile) && m_Syntax.NotAny(UriSyntaxFlags.AllowDOSPath))
				{
					return ParsingError.BadAuthorityTerminator;
				}
				m_Flags |= (Flags)num;
			}
			if (s_IdnScope != 0 || m_iriParsing)
			{
				PrivateParseMinimalIri(newHost, num);
			}
			return ParsingError.None;
		}

		private void PrivateParseMinimalIri(string newHost, ushort idx)
		{
			if (newHost != null)
			{
				m_String = newHost;
			}
			if ((!m_iriParsing && AllowIdn && ((m_Flags & Flags.IdnHost) != 0 || (m_Flags & Flags.UnicodeHost) != 0)) || (m_iriParsing && (m_Flags & Flags.HasUnicode) == 0 && AllowIdn && (m_Flags & Flags.IdnHost) != 0))
			{
				m_Flags &= ~Flags.IndexMask;
				m_Flags |= (Flags)m_String.Length;
				m_String += m_originalUnicodeString.Substring(idx, m_originalUnicodeString.Length - idx);
			}
			if (m_iriParsing && (m_Flags & Flags.HasUnicode) != 0)
			{
				m_Flags |= Flags.UseOrigUncdStrOffset;
			}
		}

		private unsafe void CreateUriInfo(Flags cF)
		{
			UriInfo uriInfo = new UriInfo();
			uriInfo.Offset.End = (ushort)m_String.Length;
			if (!UserDrivenParsing)
			{
				bool flag = false;
				ushort num;
				if ((cF & Flags.ImplicitFile) != 0)
				{
					num = 0;
					while (IsLWS(m_String[num]))
					{
						num = (ushort)(num + 1);
						uriInfo.Offset.Scheme++;
					}
					if (StaticInFact(cF, Flags.UncPath))
					{
						num = (ushort)(num + 2);
						while (num < (ushort)(cF & Flags.IndexMask) && (m_String[num] == '/' || m_String[num] == '\\'))
						{
							num = (ushort)(num + 1);
						}
					}
				}
				else
				{
					num = (ushort)m_Syntax.SchemeName.Length;
					while (m_String[num++] != ':')
					{
						uriInfo.Offset.Scheme++;
					}
					if ((cF & Flags.AuthorityFound) != 0)
					{
						if (m_String[num] == '\\' || m_String[num + 1] == '\\')
						{
							flag = true;
						}
						num = (ushort)(num + 2);
						if ((cF & (Flags.DosPath | Flags.UncPath)) != 0)
						{
							while (num < (ushort)(cF & Flags.IndexMask) && (m_String[num] == '/' || m_String[num] == '\\'))
							{
								flag = true;
								num = (ushort)(num + 1);
							}
						}
					}
				}
				if (m_Syntax.DefaultPort != -1)
				{
					uriInfo.Offset.PortValue = (ushort)m_Syntax.DefaultPort;
				}
				if ((cF & Flags.HostTypeMask) == Flags.HostTypeMask || StaticInFact(cF, Flags.DosPath))
				{
					uriInfo.Offset.User = (ushort)(cF & Flags.IndexMask);
					uriInfo.Offset.Host = uriInfo.Offset.User;
					uriInfo.Offset.Path = uriInfo.Offset.User;
					cF = (Flags)((ulong)cF & 0xFFFFFFFFFFFF0000uL);
					if (flag)
					{
						cF |= Flags.SchemeNotCanonical;
					}
				}
				else
				{
					uriInfo.Offset.User = num;
					if (HostType == Flags.BasicHostType)
					{
						uriInfo.Offset.Host = num;
						uriInfo.Offset.Path = (ushort)(cF & Flags.IndexMask);
						cF = (Flags)((ulong)cF & 0xFFFFFFFFFFFF0000uL);
					}
					else
					{
						if ((cF & Flags.HasUserInfo) != 0)
						{
							while (m_String[num] != '@')
							{
								num = (ushort)(num + 1);
							}
							num = (ushort)(num + 1);
							uriInfo.Offset.Host = num;
						}
						else
						{
							uriInfo.Offset.Host = num;
						}
						num = (ushort)(cF & Flags.IndexMask);
						cF = (Flags)((ulong)cF & 0xFFFFFFFFFFFF0000uL);
						if (flag)
						{
							cF |= Flags.SchemeNotCanonical;
						}
						uriInfo.Offset.Path = num;
						bool flag2 = false;
						bool flag3 = (cF & Flags.UseOrigUncdStrOffset) != 0;
						cF &= ~Flags.UseOrigUncdStrOffset;
						if (flag3)
						{
							uriInfo.Offset.End = (ushort)m_originalUnicodeString.Length;
						}
						if (num < uriInfo.Offset.End)
						{
							fixed (char* ptr = (flag3 ? m_originalUnicodeString : m_String))
							{
								if (ptr[(int)num] == ':')
								{
									int num2 = 0;
									if ((num = (ushort)(num + 1)) < uriInfo.Offset.End)
									{
										num2 = (ushort)(ptr[(int)num] - 48);
										if (num2 != 65535 && num2 != 15 && num2 != 65523)
										{
											flag2 = true;
											if (num2 == 0)
											{
												cF |= Flags.PortNotCanonical | Flags.E_PortNotCanonical;
											}
											for (num = (ushort)(num + 1); num < uriInfo.Offset.End; num = (ushort)(num + 1))
											{
												ushort num3 = (ushort)(ptr[(int)num] - 48);
												if (num3 == ushort.MaxValue || num3 == 15 || num3 == 65523)
												{
													break;
												}
												num2 = num2 * 10 + num3;
											}
										}
									}
									if (flag2 && uriInfo.Offset.PortValue != (ushort)num2)
									{
										uriInfo.Offset.PortValue = (ushort)num2;
										cF |= Flags.NotDefaultPort;
									}
									else
									{
										cF |= Flags.PortNotCanonical | Flags.E_PortNotCanonical;
									}
									uriInfo.Offset.Path = num;
								}
							}
						}
					}
				}
			}
			cF |= Flags.MinimalUriInfoSet;
			uriInfo.DnsSafeHost = m_DnsSafeHost;
			lock (m_String)
			{
				if ((m_Flags & Flags.MinimalUriInfoSet) == 0)
				{
					m_Info = uriInfo;
					m_Flags = (Flags)(((ulong)m_Flags & 0xFFFFFFFFFFFF0000uL) | (ulong)cF);
				}
			}
		}

		private unsafe void CreateHostString()
		{
			if (!m_Syntax.IsSimple)
			{
				lock (m_Info)
				{
					if (NotAny(Flags.ErrorOrParsingRecursion))
					{
						m_Flags |= Flags.ErrorOrParsingRecursion;
						GetHostViaCustomSyntax();
						m_Flags &= ~Flags.ErrorOrParsingRecursion;
						return;
					}
				}
			}
			Flags flags = m_Flags;
			string text = CreateHostStringHelper(m_String, m_Info.Offset.Host, m_Info.Offset.Path, ref flags, ref m_Info.ScopeId);
			if (text.Length != 0)
			{
				if (HostType == Flags.BasicHostType)
				{
					ushort idx = 0;
					Check check;
					fixed (char* str = text)
					{
						check = CheckCanonical(str, ref idx, (ushort)text.Length, '\uffff');
					}
					if ((check & Check.DisplayCanonical) == 0 && (NotAny(Flags.ImplicitFile) || (check & Check.ReservedFound) != 0))
					{
						flags |= Flags.HostNotCanonical;
					}
					if (InFact(Flags.ImplicitFile) && (check & (Check.EscapedCanonical | Check.ReservedFound)) != 0)
					{
						check &= ~Check.EscapedCanonical;
					}
					if ((check & (Check.EscapedCanonical | Check.BackslashInPath)) != Check.EscapedCanonical)
					{
						flags |= Flags.E_HostNotCanonical;
						if (NotAny(Flags.UserEscaped))
						{
							int destPos = 0;
							char[] array = EscapeString(text, 0, text.Length, null, ref destPos, isUriString: true, '?', '#', IsImplicitFile ? '\uffff' : '%');
							if (array != null)
							{
								text = new string(array, 0, destPos);
							}
						}
					}
				}
				else if (NotAny(Flags.CanonicalDnsHost))
				{
					if (m_Info.ScopeId != null)
					{
						flags |= Flags.HostNotCanonical | Flags.E_HostNotCanonical;
					}
					else
					{
						for (int i = 0; i < text.Length; i++)
						{
							if (m_Info.Offset.Host + i >= m_Info.Offset.End || text[i] != m_String[m_Info.Offset.Host + i])
							{
								flags |= Flags.HostNotCanonical | Flags.E_HostNotCanonical;
								break;
							}
						}
					}
				}
			}
			m_Info.Host = text;
			lock (m_Info)
			{
				m_Flags |= flags;
			}
		}

		private static string CreateHostStringHelper(string str, ushort idx, ushort end, ref Flags flags, ref string scopeId)
		{
			bool loopback = false;
			string text;
			switch (flags & Flags.HostTypeMask)
			{
			case Flags.DnsHostType:
				text = DomainNameHelper.ParseCanonicalName(str, idx, end, ref loopback);
				break;
			case Flags.IPv6HostType:
				text = IPv6AddressHelper.ParseCanonicalName(str, idx, ref loopback, ref scopeId);
				break;
			case Flags.IPv4HostType:
				text = IPv4AddressHelper.ParseCanonicalName(str, idx, end, ref loopback);
				break;
			case Flags.UncHostType:
				text = UncNameHelper.ParseCanonicalName(str, idx, end, ref loopback);
				break;
			case Flags.BasicHostType:
				text = ((!StaticInFact(flags, Flags.DosPath)) ? str.Substring(idx, end - idx) : string.Empty);
				if (text.Length == 0)
				{
					loopback = true;
				}
				break;
			case Flags.HostTypeMask:
				text = string.Empty;
				break;
			default:
				throw GetException(ParsingError.BadHostName);
			}
			if (loopback)
			{
				flags |= Flags.LoopbackHost;
			}
			return text;
		}

		private unsafe void GetHostViaCustomSyntax()
		{
			if (m_Info.Host != null)
			{
				return;
			}
			string text = m_Syntax.InternalGetComponents(this, UriComponents.Host, UriFormat.UriEscaped);
			if (m_Info.Host == null)
			{
				if (text.Length >= 65520)
				{
					throw GetException(ParsingError.SizeLimit);
				}
				ParsingError err = ParsingError.None;
				Flags flags = (Flags)((ulong)m_Flags & 0xFFFFFFFFFFF8FFFFuL);
				fixed (char* pString = text)
				{
					string newHost = null;
					if (CheckAuthorityHelper(pString, 0, (ushort)text.Length, ref err, ref flags, m_Syntax, ref newHost) != (ushort)text.Length)
					{
						flags = (Flags)((ulong)flags & 0xFFFFFFFFFFF8FFFFuL);
						flags |= Flags.HostTypeMask;
					}
				}
				if (err != 0 || (flags & Flags.HostTypeMask) == Flags.HostTypeMask)
				{
					m_Flags = (Flags)(((ulong)m_Flags & 0xFFFFFFFFFFF8FFFFuL) | 0x50000);
				}
				else
				{
					text = CreateHostStringHelper(text, 0, (ushort)text.Length, ref flags, ref m_Info.ScopeId);
					for (int i = 0; i < text.Length; i++)
					{
						if (m_Info.Offset.Host + i >= m_Info.Offset.End || text[i] != m_String[m_Info.Offset.Host + i])
						{
							m_Flags |= Flags.HostNotCanonical | Flags.E_HostNotCanonical;
							break;
						}
					}
					m_Flags = (Flags)(((ulong)m_Flags & 0xFFFFFFFFFFF8FFFFuL) | (ulong)(flags & Flags.HostTypeMask));
				}
			}
			string text2 = m_Syntax.InternalGetComponents(this, UriComponents.StrongPort, UriFormat.UriEscaped);
			int num = 0;
			if (text2 == null || text2.Length == 0)
			{
				m_Flags &= ~Flags.NotDefaultPort;
				m_Flags |= Flags.PortNotCanonical | Flags.E_PortNotCanonical;
				m_Info.Offset.PortValue = 0;
			}
			else
			{
				for (int j = 0; j < text2.Length; j++)
				{
					int num2 = text2[j] - 48;
					if (num2 < 0 || num2 > 9 || (num = num * 10 + num2) > 65535)
					{
						throw new UriFormatException(SR.GetString("net_uri_PortOutOfRange", m_Syntax.GetType().FullName, text2));
					}
				}
				if (num != m_Info.Offset.PortValue)
				{
					if (num == m_Syntax.DefaultPort)
					{
						m_Flags &= ~Flags.NotDefaultPort;
					}
					else
					{
						m_Flags |= Flags.NotDefaultPort;
					}
					m_Flags |= Flags.PortNotCanonical | Flags.E_PortNotCanonical;
					m_Info.Offset.PortValue = (ushort)num;
				}
			}
			m_Info.Host = text;
		}

		internal string GetParts(UriComponents uriParts, UriFormat formatAs)
		{
			return GetComponents(uriParts, formatAs);
		}

		private string GetEscapedParts(UriComponents uriParts)
		{
			ushort num = (ushort)(((ushort)m_Flags & 0x3F80) >> 6);
			if (InFact(Flags.SchemeNotCanonical))
			{
				num = (ushort)(num | 1u);
			}
			if ((uriParts & UriComponents.Path) != 0)
			{
				if (InFact(Flags.ShouldBeCompressed | Flags.FirstSlashAbsent | Flags.BackslashInPath))
				{
					num = (ushort)(num | 0x10u);
				}
				else if (IsDosPath && m_String[m_Info.Offset.Path + SecuredPathIndex - 1] == '|')
				{
					num = (ushort)(num | 0x10u);
				}
			}
			if (((ushort)uriParts & num) == 0)
			{
				string uriPartsFromUserString = GetUriPartsFromUserString(uriParts);
				if (uriPartsFromUserString != null)
				{
					return uriPartsFromUserString;
				}
			}
			return ReCreateParts(uriParts, num, UriFormat.UriEscaped);
		}

		private string GetUnescapedParts(UriComponents uriParts, UriFormat formatAs)
		{
			ushort num = (ushort)((ushort)m_Flags & 0x7Fu);
			if ((uriParts & UriComponents.Path) != 0)
			{
				if ((m_Flags & (Flags.ShouldBeCompressed | Flags.FirstSlashAbsent | Flags.BackslashInPath)) != 0)
				{
					num = (ushort)(num | 0x10u);
				}
				else if (IsDosPath && m_String[m_Info.Offset.Path + SecuredPathIndex - 1] == '|')
				{
					num = (ushort)(num | 0x10u);
				}
			}
			if (((ushort)uriParts & num) == 0)
			{
				string uriPartsFromUserString = GetUriPartsFromUserString(uriParts);
				if (uriPartsFromUserString != null)
				{
					return uriPartsFromUserString;
				}
			}
			return ReCreateParts(uriParts, num, formatAs);
		}

		private string ReCreateParts(UriComponents parts, ushort nonCanonical, UriFormat formatAs)
		{
			EnsureHostString(allowDnsOptimization: false);
			string text = (((parts & UriComponents.Host) == 0) ? string.Empty : m_Info.Host);
			int num = (m_Info.Offset.End - m_Info.Offset.User) * ((formatAs != UriFormat.UriEscaped) ? 1 : 12);
			char[] array = new char[text.Length + num + m_Syntax.SchemeName.Length + 3 + 1];
			num = 0;
			if ((parts & UriComponents.Scheme) != 0)
			{
				m_Syntax.SchemeName.CopyTo(0, array, num, m_Syntax.SchemeName.Length);
				num += m_Syntax.SchemeName.Length;
				if (parts != UriComponents.Scheme)
				{
					array[num++] = ':';
					if (InFact(Flags.AuthorityFound))
					{
						array[num++] = '/';
						array[num++] = '/';
					}
				}
			}
			if ((parts & UriComponents.UserInfo) != 0 && InFact(Flags.HasUserInfo))
			{
				if ((nonCanonical & 2u) != 0)
				{
					switch (formatAs)
					{
					case UriFormat.UriEscaped:
						if (NotAny(Flags.UserEscaped))
						{
							array = EscapeString(m_String, m_Info.Offset.User, m_Info.Offset.Host, array, ref num, isUriString: true, '?', '#', '%');
							break;
						}
						InFact(Flags.E_UserNotCanonical);
						m_String.CopyTo(m_Info.Offset.User, array, num, m_Info.Offset.Host - m_Info.Offset.User);
						num += m_Info.Offset.Host - m_Info.Offset.User;
						break;
					case UriFormat.SafeUnescaped:
						array = UnescapeString(m_String, m_Info.Offset.User, m_Info.Offset.Host - 1, array, ref num, '@', '/', '\\', InFact(Flags.UserEscaped) ? UnescapeMode.Unescape : UnescapeMode.EscapeUnescape, m_Syntax, isQuery: false, readOnlyConfig: false);
						array[num++] = '@';
						break;
					case UriFormat.Unescaped:
						array = UnescapeString(m_String, m_Info.Offset.User, m_Info.Offset.Host, array, ref num, '\uffff', '\uffff', '\uffff', UnescapeMode.Unescape | UnescapeMode.UnescapeAll, m_Syntax, isQuery: false, readOnlyConfig: false);
						break;
					default:
						array = UnescapeString(m_String, m_Info.Offset.User, m_Info.Offset.Host, array, ref num, '\uffff', '\uffff', '\uffff', UnescapeMode.CopyOnly, m_Syntax, isQuery: false, readOnlyConfig: false);
						break;
					}
				}
				else
				{
					UnescapeString(m_String, m_Info.Offset.User, m_Info.Offset.Host, array, ref num, '\uffff', '\uffff', '\uffff', UnescapeMode.CopyOnly, m_Syntax, isQuery: false, readOnlyConfig: false);
				}
				if (parts == UriComponents.UserInfo)
				{
					num--;
				}
			}
			if ((parts & UriComponents.Host) != 0 && text.Length != 0)
			{
				array = UnescapeString(unescapeMode: (formatAs != UriFormat.UriEscaped && HostType == Flags.BasicHostType && (nonCanonical & 4u) != 0) ? ((formatAs == UriFormat.Unescaped) ? (UnescapeMode.Unescape | UnescapeMode.UnescapeAll) : (InFact(Flags.UserEscaped) ? UnescapeMode.Unescape : UnescapeMode.EscapeUnescape)) : UnescapeMode.CopyOnly, input: text, start: 0, end: text.Length, dest: array, destPosition: ref num, rsvd1: '/', rsvd2: '?', rsvd3: '#', syntax: m_Syntax, isQuery: false, readOnlyConfig: false);
				if (((uint)parts & 0x80000000u) != 0 && HostType == Flags.IPv6HostType && m_Info.ScopeId != null)
				{
					m_Info.ScopeId.CopyTo(0, array, num - 1, m_Info.ScopeId.Length);
					num += m_Info.ScopeId.Length;
					array[num - 1] = ']';
				}
			}
			if ((parts & UriComponents.Port) != 0)
			{
				if ((nonCanonical & 8) == 0)
				{
					if (InFact(Flags.NotDefaultPort))
					{
						ushort num2 = m_Info.Offset.Path;
						while (m_String[num2 = (ushort)(num2 - 1)] != ':')
						{
						}
						m_String.CopyTo(num2, array, num, m_Info.Offset.Path - num2);
						num += m_Info.Offset.Path - num2;
					}
					else if ((parts & UriComponents.StrongPort) != 0 && m_Syntax.DefaultPort != -1)
					{
						array[num++] = ':';
						text = m_Info.Offset.PortValue.ToString(CultureInfo.InvariantCulture);
						text.CopyTo(0, array, num, text.Length);
						num += text.Length;
					}
				}
				else if (InFact(Flags.NotDefaultPort) || ((parts & UriComponents.StrongPort) != 0 && m_Syntax.DefaultPort != -1))
				{
					array[num++] = ':';
					text = m_Info.Offset.PortValue.ToString(CultureInfo.InvariantCulture);
					text.CopyTo(0, array, num, text.Length);
					num += text.Length;
				}
			}
			if ((parts & UriComponents.Path) != 0)
			{
				array = GetCanonicalPath(array, ref num, formatAs);
				if (parts == UriComponents.Path)
				{
					ushort startIndex;
					if (InFact(Flags.AuthorityFound) && num != 0 && array[0] == '/')
					{
						startIndex = 1;
						num--;
					}
					else
					{
						startIndex = 0;
					}
					if (num != 0)
					{
						return new string(array, startIndex, num);
					}
					return string.Empty;
				}
			}
			if ((parts & UriComponents.Query) != 0 && m_Info.Offset.Query < m_Info.Offset.Fragment)
			{
				ushort startIndex = (ushort)(m_Info.Offset.Query + 1);
				if (parts != UriComponents.Query)
				{
					array[num++] = '?';
				}
				if ((nonCanonical & 0x20u) != 0)
				{
					switch (formatAs)
					{
					case UriFormat.UriEscaped:
						if (NotAny(Flags.UserEscaped))
						{
							array = EscapeString(m_String, startIndex, m_Info.Offset.Fragment, array, ref num, isUriString: true, '#', '\uffff', '%');
						}
						else
						{
							UnescapeString(m_String, startIndex, m_Info.Offset.Fragment, array, ref num, '\uffff', '\uffff', '\uffff', UnescapeMode.CopyOnly, m_Syntax, isQuery: true, readOnlyConfig: false);
						}
						break;
					case (UriFormat)32767:
						array = UnescapeString(m_String, startIndex, m_Info.Offset.Fragment, array, ref num, '#', '\uffff', '\uffff', (InFact(Flags.UserEscaped) ? UnescapeMode.Unescape : UnescapeMode.EscapeUnescape) | UnescapeMode.V1ToStringFlag, m_Syntax, isQuery: true, readOnlyConfig: false);
						break;
					case UriFormat.Unescaped:
						array = UnescapeString(m_String, startIndex, m_Info.Offset.Fragment, array, ref num, '#', '\uffff', '\uffff', UnescapeMode.Unescape | UnescapeMode.UnescapeAll, m_Syntax, isQuery: true, readOnlyConfig: false);
						break;
					default:
						array = UnescapeString(m_String, startIndex, m_Info.Offset.Fragment, array, ref num, '#', '\uffff', '\uffff', InFact(Flags.UserEscaped) ? UnescapeMode.Unescape : UnescapeMode.EscapeUnescape, m_Syntax, isQuery: true, readOnlyConfig: false);
						break;
					}
				}
				else
				{
					UnescapeString(m_String, startIndex, m_Info.Offset.Fragment, array, ref num, '\uffff', '\uffff', '\uffff', UnescapeMode.CopyOnly, m_Syntax, isQuery: true, readOnlyConfig: false);
				}
			}
			if ((parts & UriComponents.Fragment) != 0 && m_Info.Offset.Fragment < m_Info.Offset.End)
			{
				ushort startIndex = (ushort)(m_Info.Offset.Fragment + 1);
				if (parts != UriComponents.Fragment)
				{
					array[num++] = '#';
				}
				if ((nonCanonical & 0x40u) != 0)
				{
					switch (formatAs)
					{
					case UriFormat.UriEscaped:
						if (NotAny(Flags.UserEscaped))
						{
							array = EscapeString(m_String, startIndex, m_Info.Offset.End, array, ref num, isUriString: true, '#', '\uffff', '%');
						}
						else
						{
							UnescapeString(m_String, startIndex, m_Info.Offset.End, array, ref num, '\uffff', '\uffff', '\uffff', UnescapeMode.CopyOnly, m_Syntax, isQuery: false, readOnlyConfig: false);
						}
						break;
					case (UriFormat)32767:
						array = UnescapeString(m_String, startIndex, m_Info.Offset.End, array, ref num, '#', '\uffff', '\uffff', (InFact(Flags.UserEscaped) ? UnescapeMode.Unescape : UnescapeMode.EscapeUnescape) | UnescapeMode.V1ToStringFlag, m_Syntax, isQuery: false, readOnlyConfig: false);
						break;
					case UriFormat.Unescaped:
						array = UnescapeString(m_String, startIndex, m_Info.Offset.End, array, ref num, '#', '\uffff', '\uffff', UnescapeMode.Unescape | UnescapeMode.UnescapeAll, m_Syntax, isQuery: false, readOnlyConfig: false);
						break;
					default:
						array = UnescapeString(m_String, startIndex, m_Info.Offset.End, array, ref num, '#', '\uffff', '\uffff', InFact(Flags.UserEscaped) ? UnescapeMode.Unescape : UnescapeMode.EscapeUnescape, m_Syntax, isQuery: false, readOnlyConfig: false);
						break;
					}
				}
				else
				{
					UnescapeString(m_String, startIndex, m_Info.Offset.End, array, ref num, '\uffff', '\uffff', '\uffff', UnescapeMode.CopyOnly, m_Syntax, isQuery: false, readOnlyConfig: false);
				}
			}
			return new string(array, 0, num);
		}

		private string GetUriPartsFromUserString(UriComponents uriParts)
		{
			switch (uriParts & ~UriComponents.KeepDelimiter)
			{
			case UriComponents.SchemeAndServer:
				if (!InFact(Flags.HasUserInfo))
				{
					return m_String.Substring(m_Info.Offset.Scheme, m_Info.Offset.Path - m_Info.Offset.Scheme);
				}
				return m_String.Substring(m_Info.Offset.Scheme, m_Info.Offset.User - m_Info.Offset.Scheme) + m_String.Substring(m_Info.Offset.Host, m_Info.Offset.Path - m_Info.Offset.Host);
			case UriComponents.HostAndPort:
				if (InFact(Flags.HasUserInfo))
				{
					if (InFact(Flags.NotDefaultPort) || m_Syntax.DefaultPort == -1)
					{
						return m_String.Substring(m_Info.Offset.Host, m_Info.Offset.Path - m_Info.Offset.Host);
					}
					return m_String.Substring(m_Info.Offset.Host, m_Info.Offset.Path - m_Info.Offset.Host) + ':' + m_Info.Offset.PortValue.ToString(CultureInfo.InvariantCulture);
				}
				goto case UriComponents.StrongAuthority;
			case UriComponents.AbsoluteUri:
				if (m_Info.Offset.Scheme == 0 && m_Info.Offset.End == m_String.Length)
				{
					return m_String;
				}
				return m_String.Substring(m_Info.Offset.Scheme, m_Info.Offset.End - m_Info.Offset.Scheme);
			case UriComponents.HttpRequestUrl:
				if (InFact(Flags.HasUserInfo))
				{
					return m_String.Substring(m_Info.Offset.Scheme, m_Info.Offset.User - m_Info.Offset.Scheme) + m_String.Substring(m_Info.Offset.Host, m_Info.Offset.Fragment - m_Info.Offset.Host);
				}
				if (m_Info.Offset.Scheme == 0 && m_Info.Offset.Fragment == m_String.Length)
				{
					return m_String;
				}
				return m_String.Substring(m_Info.Offset.Scheme, m_Info.Offset.Fragment - m_Info.Offset.Scheme);
			case UriComponents.SchemeAndServer | UriComponents.UserInfo:
				return m_String.Substring(m_Info.Offset.Scheme, m_Info.Offset.Path - m_Info.Offset.Scheme);
			case UriComponents.HttpRequestUrl | UriComponents.UserInfo:
				if (m_Info.Offset.Scheme == 0 && m_Info.Offset.Fragment == m_String.Length)
				{
					return m_String;
				}
				return m_String.Substring(m_Info.Offset.Scheme, m_Info.Offset.Fragment - m_Info.Offset.Scheme);
			case UriComponents.Scheme:
				if (uriParts != UriComponents.Scheme)
				{
					return m_String.Substring(m_Info.Offset.Scheme, m_Info.Offset.User - m_Info.Offset.Scheme);
				}
				return m_Syntax.SchemeName;
			case UriComponents.Host:
			{
				ushort num2 = m_Info.Offset.Path;
				if (InFact(Flags.PortNotCanonical | Flags.NotDefaultPort))
				{
					while (m_String[num2 = (ushort)(num2 - 1)] != ':')
					{
					}
				}
				if (num2 - m_Info.Offset.Host != 0)
				{
					return m_String.Substring(m_Info.Offset.Host, num2 - m_Info.Offset.Host);
				}
				return string.Empty;
			}
			case UriComponents.Path:
			{
				ushort num = ((uriParts != UriComponents.Path || !InFact(Flags.AuthorityFound) || m_Info.Offset.End <= m_Info.Offset.Path || m_String[m_Info.Offset.Path] != '/') ? m_Info.Offset.Path : ((ushort)(m_Info.Offset.Path + 1)));
				if (num >= m_Info.Offset.Query)
				{
					return string.Empty;
				}
				return m_String.Substring(num, m_Info.Offset.Query - num);
			}
			case UriComponents.Query:
			{
				ushort num = ((uriParts != UriComponents.Query) ? m_Info.Offset.Query : ((ushort)(m_Info.Offset.Query + 1)));
				if (num >= m_Info.Offset.Fragment)
				{
					return string.Empty;
				}
				return m_String.Substring(num, m_Info.Offset.Fragment - num);
			}
			case UriComponents.Fragment:
			{
				ushort num = ((uriParts != UriComponents.Fragment) ? m_Info.Offset.Fragment : ((ushort)(m_Info.Offset.Fragment + 1)));
				if (num >= m_Info.Offset.End)
				{
					return string.Empty;
				}
				return m_String.Substring(num, m_Info.Offset.End - num);
			}
			case UriComponents.UserInfo | UriComponents.Host | UriComponents.Port:
				if (m_Info.Offset.Path - m_Info.Offset.User != 0)
				{
					return m_String.Substring(m_Info.Offset.User, m_Info.Offset.Path - m_Info.Offset.User);
				}
				return string.Empty;
			case UriComponents.StrongAuthority:
				if (!InFact(Flags.NotDefaultPort) && m_Syntax.DefaultPort != -1)
				{
					return m_String.Substring(m_Info.Offset.User, m_Info.Offset.Path - m_Info.Offset.User) + ':' + m_Info.Offset.PortValue.ToString(CultureInfo.InvariantCulture);
				}
				goto case UriComponents.UserInfo | UriComponents.Host | UriComponents.Port;
			case UriComponents.PathAndQuery:
				return m_String.Substring(m_Info.Offset.Path, m_Info.Offset.Fragment - m_Info.Offset.Path);
			case UriComponents.HttpRequestUrl | UriComponents.Fragment:
				if (InFact(Flags.HasUserInfo))
				{
					return m_String.Substring(m_Info.Offset.Scheme, m_Info.Offset.User - m_Info.Offset.Scheme) + m_String.Substring(m_Info.Offset.Host, m_Info.Offset.End - m_Info.Offset.Host);
				}
				if (m_Info.Offset.Scheme == 0 && m_Info.Offset.End == m_String.Length)
				{
					return m_String;
				}
				return m_String.Substring(m_Info.Offset.Scheme, m_Info.Offset.End - m_Info.Offset.Scheme);
			case UriComponents.PathAndQuery | UriComponents.Fragment:
				return m_String.Substring(m_Info.Offset.Path, m_Info.Offset.End - m_Info.Offset.Path);
			case UriComponents.UserInfo:
			{
				if (NotAny(Flags.HasUserInfo))
				{
					return string.Empty;
				}
				ushort num = ((uriParts != UriComponents.UserInfo) ? m_Info.Offset.Host : ((ushort)(m_Info.Offset.Host - 1)));
				if (m_Info.Offset.User >= num)
				{
					return string.Empty;
				}
				return m_String.Substring(m_Info.Offset.User, num - m_Info.Offset.User);
			}
			default:
				return null;
			}
		}

		private unsafe void ParseRemaining()
		{
			EnsureUriInfo();
			Flags flags = Flags.Zero;
			if (!UserDrivenParsing)
			{
				bool flag = m_iriParsing && (m_Flags & Flags.HasUnicode) != 0 && (m_Flags & Flags.RestUnicodeNormalized) == 0;
				ushort scheme = m_Info.Offset.Scheme;
				ushort num = (ushort)m_String.Length;
				Check check = Check.None;
				UriSyntaxFlags flags2 = m_Syntax.Flags;
				fixed (char* ptr = m_String)
				{
					if (num > scheme && IsLWS(ptr[num - 1]))
					{
						num = (ushort)(num - 1);
						while (num != scheme && IsLWS(ptr[(int)(num = (ushort)(num - 1))]))
						{
						}
						num = (ushort)(num + 1);
					}
					if (IsImplicitFile)
					{
						flags |= Flags.SchemeNotCanonical;
					}
					else
					{
						ushort num2 = 0;
						ushort num3 = (ushort)m_Syntax.SchemeName.Length;
						while (num2 < num3)
						{
							if (m_Syntax.SchemeName[num2] != ptr[scheme + num2])
							{
								flags |= Flags.SchemeNotCanonical;
							}
							num2 = (ushort)(num2 + 1);
						}
						if ((m_Flags & Flags.AuthorityFound) != 0 && (scheme + num2 + 3 >= num || ptr[scheme + num2 + 1] != '/' || ptr[scheme + num2 + 2] != '/'))
						{
							flags |= Flags.SchemeNotCanonical;
						}
					}
					if ((m_Flags & Flags.HasUserInfo) != 0)
					{
						scheme = m_Info.Offset.User;
						check = CheckCanonical(ptr, ref scheme, m_Info.Offset.Host, '@');
						if ((check & Check.DisplayCanonical) == 0)
						{
							flags |= Flags.UserNotCanonical;
						}
						if ((check & (Check.EscapedCanonical | Check.BackslashInPath)) != Check.EscapedCanonical)
						{
							flags |= Flags.E_UserNotCanonical;
						}
						if (m_iriParsing && (check & (Check.EscapedCanonical | Check.DisplayCanonical | Check.BackslashInPath | Check.NotIriCanonical | Check.FoundNonAscii)) == (Check.DisplayCanonical | Check.FoundNonAscii))
						{
							flags |= Flags.UserIriCanonical;
						}
					}
				}
				scheme = m_Info.Offset.Path;
				ushort idx = m_Info.Offset.Path;
				if (flag)
				{
					m_Info.Offset.Path = (ushort)m_String.Length;
					scheme = m_Info.Offset.Path;
					ushort start = idx;
					if (IsImplicitFile || (flags2 & (UriSyntaxFlags.MayHaveQuery | UriSyntaxFlags.MayHaveFragment)) == 0)
					{
						FindEndOfComponent(m_originalUnicodeString, ref idx, (ushort)m_originalUnicodeString.Length, '\uffff');
					}
					else
					{
						FindEndOfComponent(m_originalUnicodeString, ref idx, (ushort)m_originalUnicodeString.Length, m_Syntax.InFact(UriSyntaxFlags.MayHaveQuery) ? '?' : (m_Syntax.InFact(UriSyntaxFlags.MayHaveFragment) ? '#' : '\ufffe'));
					}
					string text = EscapeUnescapeIri(m_originalUnicodeString, start, idx, UriComponents.Path);
					try
					{
						m_String += text.Normalize(NormalizationForm.FormC);
					}
					catch (ArgumentException)
					{
						UriFormatException exception = GetException(ParsingError.BadFormat);
						throw exception;
					}
					if (!ServicePointManager.AllowAllUriEncodingExpansion && m_String.Length > 65535)
					{
						UriFormatException exception2 = GetException(ParsingError.SizeLimit);
						throw exception2;
					}
					num = (ushort)m_String.Length;
				}
				fixed (char* ptr2 = m_String)
				{
					check = ((!IsImplicitFile && (flags2 & (UriSyntaxFlags.MayHaveQuery | UriSyntaxFlags.MayHaveFragment)) != 0) ? CheckCanonical(ptr2, ref scheme, num, ((flags2 & UriSyntaxFlags.MayHaveQuery) != 0) ? '?' : (m_Syntax.InFact(UriSyntaxFlags.MayHaveFragment) ? '#' : '\ufffe')) : CheckCanonical(ptr2, ref scheme, num, '\uffff'));
					if ((m_Flags & Flags.AuthorityFound) != 0 && (flags2 & UriSyntaxFlags.PathIsRooted) != 0 && (m_Info.Offset.Path == num || (ptr2[(int)m_Info.Offset.Path] != '/' && ptr2[(int)m_Info.Offset.Path] != '\\')))
					{
						flags |= Flags.FirstSlashAbsent;
					}
				}
				bool flag2 = false;
				if (IsDosPath || ((m_Flags & Flags.AuthorityFound) != 0 && (flags2 & (UriSyntaxFlags.ConvertPathSlashes | UriSyntaxFlags.CompressPath | UriSyntaxFlags.UnEscapeDotsAndSlashes)) != 0))
				{
					if ((flags2 & UriSyntaxFlags.UnEscapeDotsAndSlashes) != 0 && (check & Check.DotSlashEscaped) != 0)
					{
						flags |= Flags.PathNotCanonical | Flags.E_PathNotCanonical;
						flag2 = true;
					}
					if ((flags2 & UriSyntaxFlags.ConvertPathSlashes) != 0 && (check & Check.BackslashInPath) != 0)
					{
						flags |= Flags.PathNotCanonical | Flags.E_PathNotCanonical;
						flag2 = true;
					}
					if ((flags2 & UriSyntaxFlags.CompressPath) != 0 && ((flags & Flags.E_PathNotCanonical) != 0 || (check & Check.DotSlashAttn) != 0))
					{
						flags |= Flags.ShouldBeCompressed;
					}
					if ((check & Check.BackslashInPath) != 0)
					{
						flags |= Flags.BackslashInPath;
					}
				}
				else if ((check & Check.BackslashInPath) != 0)
				{
					flags |= Flags.E_PathNotCanonical;
					flag2 = true;
				}
				if ((check & Check.DisplayCanonical) == 0 && ((m_Flags & Flags.ImplicitFile) == 0 || (m_Flags & Flags.UserEscaped) != 0 || (check & Check.ReservedFound) != 0))
				{
					flags |= Flags.PathNotCanonical;
					flag2 = true;
				}
				if ((m_Flags & Flags.ImplicitFile) != 0 && (check & (Check.EscapedCanonical | Check.ReservedFound)) != 0)
				{
					check &= ~Check.EscapedCanonical;
				}
				if ((check & Check.EscapedCanonical) == 0)
				{
					flags |= Flags.E_PathNotCanonical;
				}
				if (m_iriParsing && !flag2 && (check & (Check.EscapedCanonical | Check.DisplayCanonical | Check.NotIriCanonical | Check.FoundNonAscii)) == (Check.DisplayCanonical | Check.FoundNonAscii))
				{
					flags |= Flags.PathIriCanonical;
				}
				if (flag)
				{
					ushort start2 = idx;
					if (idx < m_originalUnicodeString.Length && m_originalUnicodeString[idx] == '?')
					{
						idx = (ushort)(idx + 1);
						FindEndOfComponent(m_originalUnicodeString, ref idx, (ushort)m_originalUnicodeString.Length, ((flags2 & UriSyntaxFlags.MayHaveFragment) != 0) ? '#' : '\ufffe');
						string text2 = EscapeUnescapeIri(m_originalUnicodeString, start2, idx, UriComponents.Query);
						try
						{
							m_String += text2.Normalize(NormalizationForm.FormC);
						}
						catch (ArgumentException)
						{
							UriFormatException exception3 = GetException(ParsingError.BadFormat);
							throw exception3;
						}
						if (!ServicePointManager.AllowAllUriEncodingExpansion && m_String.Length > 65535)
						{
							UriFormatException exception4 = GetException(ParsingError.SizeLimit);
							throw exception4;
						}
						num = (ushort)m_String.Length;
					}
				}
				m_Info.Offset.Query = scheme;
				fixed (char* ptr3 = m_String)
				{
					if (scheme < num && ptr3[(int)scheme] == '?')
					{
						scheme = (ushort)(scheme + 1);
						check = CheckCanonical(ptr3, ref scheme, num, ((flags2 & UriSyntaxFlags.MayHaveFragment) != 0) ? '#' : '\ufffe');
						if ((check & Check.DisplayCanonical) == 0)
						{
							flags |= Flags.QueryNotCanonical;
						}
						if ((check & (Check.EscapedCanonical | Check.BackslashInPath)) != Check.EscapedCanonical)
						{
							flags |= Flags.E_QueryNotCanonical;
						}
						if (m_iriParsing && (check & (Check.EscapedCanonical | Check.DisplayCanonical | Check.BackslashInPath | Check.NotIriCanonical | Check.FoundNonAscii)) == (Check.DisplayCanonical | Check.FoundNonAscii))
						{
							flags |= Flags.QueryIriCanonical;
						}
					}
				}
				if (flag)
				{
					ushort start3 = idx;
					if (idx < m_originalUnicodeString.Length && m_originalUnicodeString[idx] == '#')
					{
						idx = (ushort)(idx + 1);
						FindEndOfComponent(m_originalUnicodeString, ref idx, (ushort)m_originalUnicodeString.Length, '\ufffe');
						string text3 = EscapeUnescapeIri(m_originalUnicodeString, start3, idx, UriComponents.Fragment);
						try
						{
							m_String += text3.Normalize(NormalizationForm.FormC);
						}
						catch (ArgumentException)
						{
							UriFormatException exception5 = GetException(ParsingError.BadFormat);
							throw exception5;
						}
						if (!ServicePointManager.AllowAllUriEncodingExpansion && m_String.Length > 65535)
						{
							UriFormatException exception6 = GetException(ParsingError.SizeLimit);
							throw exception6;
						}
						num = (ushort)m_String.Length;
					}
				}
				m_Info.Offset.Fragment = scheme;
				fixed (char* ptr4 = m_String)
				{
					if (scheme < num && ptr4[(int)scheme] == '#')
					{
						scheme = (ushort)(scheme + 1);
						check = CheckCanonical(ptr4, ref scheme, num, '\ufffe');
						if ((check & Check.DisplayCanonical) == 0)
						{
							flags |= Flags.FragmentNotCanonical;
						}
						if ((check & (Check.EscapedCanonical | Check.BackslashInPath)) != Check.EscapedCanonical)
						{
							flags |= Flags.E_FragmentNotCanonical;
						}
						if (m_iriParsing && (check & (Check.EscapedCanonical | Check.DisplayCanonical | Check.BackslashInPath | Check.NotIriCanonical | Check.FoundNonAscii)) == (Check.DisplayCanonical | Check.FoundNonAscii))
						{
							flags |= Flags.FragmentIriCanonical;
						}
					}
				}
				m_Info.Offset.End = scheme;
			}
			flags |= Flags.AllUriInfoSet;
			lock (m_Info)
			{
				m_Flags |= flags;
			}
			m_Flags |= Flags.RestUnicodeNormalized;
		}

		private unsafe static ushort ParseSchemeCheckImplicitFile(char* uriString, ushort length, ref ParsingError err, ref Flags flags, ref UriParser syntax)
		{
			ushort num = 0;
			while (num < length && IsLWS(uriString[(int)num]))
			{
				num = (ushort)(num + 1);
			}
			ushort num2 = num;
			while (num2 < length && uriString[(int)num2] != ':')
			{
				num2 = (ushort)(num2 + 1);
			}
			if (IntPtr.Size == 4 && num2 != length && num2 >= num + 3 && CheckKnownSchemes((long*)(uriString + (int)num), (ushort)(num2 - num), ref syntax))
			{
				return (ushort)(num2 + 1);
			}
			if (num + 2 >= length || num2 == num)
			{
				err = ParsingError.BadFormat;
				return 0;
			}
			char c;
			if ((c = uriString[num + 1]) == ':' || c == '|')
			{
				if (IsAsciiLetter(uriString[(int)num]))
				{
					if ((c = uriString[num + 2]) == '\\' || c == '/')
					{
						flags |= Flags.AuthorityFound | Flags.DosPath | Flags.ImplicitFile;
						syntax = UriParser.FileUri;
						return num;
					}
					err = ParsingError.MustRootedPath;
					return 0;
				}
				if (c == ':')
				{
					err = ParsingError.BadScheme;
				}
				else
				{
					err = ParsingError.BadFormat;
				}
				return 0;
			}
			if ((c = uriString[(int)num]) == '/' || c == '\\')
			{
				if ((c = uriString[num + 1]) == '\\' || c == '/')
				{
					flags |= Flags.AuthorityFound | Flags.UncPath | Flags.ImplicitFile;
					syntax = UriParser.FileUri;
					num = (ushort)(num + 2);
					while (num < length && ((c = uriString[(int)num]) == '/' || c == '\\'))
					{
						num = (ushort)(num + 1);
					}
					return num;
				}
				err = ParsingError.BadFormat;
				return 0;
			}
			if (num2 == length)
			{
				err = ParsingError.BadFormat;
				return 0;
			}
			if (num2 - num > 1024)
			{
				err = ParsingError.SchemeLimit;
				return 0;
			}
			char* ptr = (char*)stackalloc byte[2 * (num2 - num)];
			length = 0;
			while (num < num2)
			{
				ptr[(int)length++] = uriString[(int)num];
				num = (ushort)(num + 1);
			}
			err = CheckSchemeSyntax(ptr, length, ref syntax);
			if (err != 0)
			{
				return 0;
			}
			return (ushort)(num2 + 1);
		}

		private unsafe static bool CheckKnownSchemes(long* lptr, ushort nChars, ref UriParser syntax)
		{
			switch (*lptr | 0x20002000200020L)
			{
			case 31525695615402088L:
				switch (nChars)
				{
				case 4:
					syntax = UriParser.HttpUri;
					return true;
				case 5:
					if ((*(ushort*)(lptr + 1) | 0x20) == 115)
					{
						syntax = UriParser.HttpsUri;
						return true;
					}
					break;
				}
				break;
			case 28429436511125606L:
				if (nChars == 4)
				{
					syntax = UriParser.FileUri;
					return true;
				}
				break;
			case 16326029693157478L:
				if (nChars == 3)
				{
					syntax = UriParser.FtpUri;
					return true;
				}
				break;
			case 32370133429452910L:
				if (nChars == 4)
				{
					syntax = UriParser.NewsUri;
					return true;
				}
				break;
			case 31525695615008878L:
				if (nChars == 4)
				{
					syntax = UriParser.NntpUri;
					return true;
				}
				break;
			case 28147948650299509L:
				if (nChars == 4)
				{
					syntax = UriParser.UuidUri;
					return true;
				}
				break;
			case 29273878621519975L:
				if (nChars == 6 && (*(int*)(lptr + 1) | 0x200020) == 7471205)
				{
					syntax = UriParser.GopherUri;
					return true;
				}
				break;
			case 30399748462674029L:
				if (nChars == 6 && (*(int*)(lptr + 1) | 0x200020) == 7274612)
				{
					syntax = UriParser.MailToUri;
					return true;
				}
				break;
			case 30962711301259380L:
				if (nChars == 6 && (*(int*)(lptr + 1) | 0x200020) == 7602277)
				{
					syntax = UriParser.TelnetUri;
					return true;
				}
				break;
			case 12948347151515758L:
				if (nChars == 8 && (lptr[1] | 0x20002000200020L) == 28429453690994800L)
				{
					syntax = UriParser.NetPipeUri;
					return true;
				}
				if (nChars == 7 && (lptr[1] | 0x20002000200020L) == 16326029692043380L)
				{
					syntax = UriParser.NetTcpUri;
					return true;
				}
				break;
			case 31525614009974892L:
				if (nChars == 4)
				{
					syntax = UriParser.LdapUri;
					return true;
				}
				break;
			}
			return false;
		}

		private unsafe static ParsingError CheckSchemeSyntax(char* ptr, ushort length, ref UriParser syntax)
		{
			char c = *ptr;
			if (c < 'a' || c > 'z')
			{
				if (c < 'A' || c > 'Z')
				{
					return ParsingError.BadScheme;
				}
				*ptr = (char)(c | 0x20u);
			}
			for (ushort num = 1; num < length; num = (ushort)(num + 1))
			{
				char c2 = ptr[(int)num];
				if (c2 < 'a' || c2 > 'z')
				{
					if (c2 >= 'A' && c2 <= 'Z')
					{
						ptr[(int)num] = (char)(c2 | 0x20u);
					}
					else if ((c2 < '0' || c2 > '9') && c2 != '+' && c2 != '-' && c2 != '.')
					{
						return ParsingError.BadScheme;
					}
				}
			}
			string lwrCaseScheme = new string(ptr, 0, length);
			syntax = UriParser.FindOrFetchAsUnknownV1Syntax(lwrCaseScheme);
			return ParsingError.None;
		}

		private unsafe ushort CheckAuthorityHelper(char* pString, ushort idx, ushort length, ref ParsingError err, ref Flags flags, UriParser syntax, ref string newHost)
		{
			int i = length;
			int num = idx;
			ushort num2 = idx;
			newHost = null;
			bool justNormalized = false;
			bool flag = s_IriParsing && IriParsingStatic(syntax);
			bool flag2 = (flags & Flags.HasUnicode) != 0;
			bool flag3 = (flags & Flags.HostUnicodeNormalized) == 0;
			UriSyntaxFlags flags2 = syntax.Flags;
			if (flag2 && flag && flag3)
			{
				newHost = m_originalUnicodeString.Substring(0, num);
			}
			char c;
			if (idx == length || (c = pString[(int)idx]) == '/' || (c == '\\' && StaticIsFile(syntax)) || c == '#' || c == '?')
			{
				if (syntax.InFact(UriSyntaxFlags.AllowEmptyHost))
				{
					flags &= ~Flags.UncPath;
					if (StaticInFact(flags, Flags.ImplicitFile))
					{
						err = ParsingError.BadHostName;
					}
					else
					{
						flags |= Flags.BasicHostType;
					}
				}
				else
				{
					err = ParsingError.BadHostName;
				}
				if (flag2 && flag && flag3)
				{
					flags |= Flags.HostUnicodeNormalized;
				}
				return idx;
			}
			string text = null;
			if ((flags2 & UriSyntaxFlags.MayHaveUserInfo) != 0)
			{
				while (num2 < i)
				{
					if (num2 == i - 1 || pString[(int)num2] == '?' || pString[(int)num2] == '#' || pString[(int)num2] == '\\' || pString[(int)num2] == '/')
					{
						num2 = idx;
						break;
					}
					if (pString[(int)num2] == '@')
					{
						flags |= Flags.HasUserInfo;
						if (flag || s_IdnScope != 0)
						{
							if (flag && flag2 && flag3)
							{
								text = EscapeUnescapeIri(pString, num, num2 + 1, UriComponents.UserInfo);
								try
								{
									text = text.Normalize(NormalizationForm.FormC);
								}
								catch (ArgumentException)
								{
									err = ParsingError.BadFormat;
									return idx;
								}
								newHost += text;
								if (!ServicePointManager.AllowAllUriEncodingExpansion && newHost.Length > 65535)
								{
									err = ParsingError.SizeLimit;
									return idx;
								}
							}
							else
							{
								text = new string(pString, num, num2 - num + 1);
							}
						}
						num2 = (ushort)(num2 + 1);
						c = pString[(int)num2];
						break;
					}
					num2 = (ushort)(num2 + 1);
				}
			}
			bool notCanonical = (flags2 & UriSyntaxFlags.SimpleUserSyntax) == 0;
			if (c == '[' && syntax.InFact(UriSyntaxFlags.AllowIPv6Host) && IPv6AddressHelper.IsValid(pString, num2 + 1, ref i))
			{
				flags |= Flags.IPv6HostType;
				if (!s_ConfigInitialized)
				{
					InitializeUriConfig();
					m_iriParsing = s_IriParsing && IriParsingStatic(syntax);
				}
				if (flag2 && flag && flag3)
				{
					newHost += new string(pString, num2, i - num2);
					flags |= Flags.HostUnicodeNormalized;
					justNormalized = true;
				}
			}
			else if (c <= '9' && c >= '0' && syntax.InFact(UriSyntaxFlags.AllowIPv4Host) && IPv4AddressHelper.IsValid(pString, num2, ref i, allowIPv6: false, StaticNotAny(flags, Flags.ImplicitFile)))
			{
				flags |= Flags.IPv4HostType;
				if (flag2 && flag && flag3)
				{
					newHost += new string(pString, num2, i - num2);
					flags |= Flags.HostUnicodeNormalized;
					justNormalized = true;
				}
			}
			else if ((flags2 & UriSyntaxFlags.AllowDnsHost) != 0 && !flag && DomainNameHelper.IsValid(pString, num2, ref i, ref notCanonical, StaticNotAny(flags, Flags.ImplicitFile)))
			{
				flags |= Flags.DnsHostType;
				if (!notCanonical)
				{
					flags |= Flags.CanonicalDnsHost;
				}
				if (s_IdnScope != 0)
				{
					if (s_IdnScope == UriIdnScope.AllExceptIntranet && IsIntranet(new string(pString, 0, i)))
					{
						flags |= Flags.IntranetUri;
					}
					if (AllowIdnStatic(syntax, flags))
					{
						bool allAscii = true;
						bool atLeastOneValidIdn = false;
						string str = DomainNameHelper.UnicodeEquivalent(pString, num2, i, ref allAscii, ref atLeastOneValidIdn);
						if (atLeastOneValidIdn)
						{
							if (StaticNotAny(flags, Flags.HasUnicode))
							{
								m_originalUnicodeString = m_String;
							}
							flags |= Flags.IdnHost;
							newHost = m_originalUnicodeString.Substring(0, num) + text + str;
							flags |= Flags.CanonicalDnsHost;
							m_DnsSafeHost = new string(pString, num2, i - num2);
							justNormalized = true;
						}
						flags |= Flags.HostUnicodeNormalized;
					}
				}
			}
			else if ((flag || s_IdnScope != 0) && (flags2 & UriSyntaxFlags.AllowDnsHost) != 0 && ((flag && flag3) || AllowIdnStatic(syntax, flags)) && DomainNameHelper.IsValidByIri(pString, num2, ref i, ref notCanonical, StaticNotAny(flags, Flags.ImplicitFile)))
			{
				CheckAuthorityHelperHandleDnsIri(pString, num2, i, num, flag, flag2, syntax, text, ref flags, ref justNormalized, ref newHost, ref err);
			}
			else if (s_IdnScope == UriIdnScope.None && !s_IriParsing && (flags2 & UriSyntaxFlags.AllowUncHost) != 0 && UncNameHelper.IsValid(pString, num2, ref i, StaticNotAny(flags, Flags.ImplicitFile)) && i - num2 <= 256)
			{
				flags |= Flags.UncHostType;
			}
			if (i < length && pString[i] == '\\' && (flags & Flags.HostTypeMask) != 0 && !StaticIsFile(syntax))
			{
				if (syntax.InFact(UriSyntaxFlags.V1_UnknownUri))
				{
					err = ParsingError.BadHostName;
					flags |= Flags.HostTypeMask;
					return (ushort)i;
				}
				flags &= ~Flags.HostTypeMask;
			}
			else if (i < length && pString[i] == ':')
			{
				if (syntax.InFact(UriSyntaxFlags.MayHavePort))
				{
					int num3 = 0;
					int num4 = i;
					idx = (ushort)(i + 1);
					while (idx < length)
					{
						ushort num5 = (ushort)(pString[(int)idx] - 48);
						if (num5 >= 0 && num5 <= 9)
						{
							if ((num3 = num3 * 10 + num5) > 65535)
							{
								break;
							}
							idx = (ushort)(idx + 1);
							continue;
						}
						if (num5 == ushort.MaxValue || num5 == 15 || num5 == 65523)
						{
							break;
						}
						if (syntax.InFact(UriSyntaxFlags.AllowAnyOtherHost) && syntax.NotAny(UriSyntaxFlags.V1_UnknownUri))
						{
							flags &= ~Flags.HostTypeMask;
							break;
						}
						err = ParsingError.BadPort;
						return idx;
					}
					if (num3 > 65535)
					{
						if (!syntax.InFact(UriSyntaxFlags.AllowAnyOtherHost))
						{
							err = ParsingError.BadPort;
							return idx;
						}
						flags &= ~Flags.HostTypeMask;
					}
					if (flag && flag2 && justNormalized)
					{
						newHost += new string(pString, num4, idx - num4);
					}
				}
				else
				{
					flags &= ~Flags.HostTypeMask;
				}
			}
			if ((flags & Flags.HostTypeMask) == 0)
			{
				flags &= ~Flags.HasUserInfo;
				if (syntax.InFact(UriSyntaxFlags.AllowAnyOtherHost))
				{
					flags |= Flags.BasicHostType;
					for (i = idx; i < length && pString[i] != '/' && pString[i] != '?' && pString[i] != '#'; i++)
					{
					}
					CheckAuthorityHelperHandleAnyHostIri(pString, num, i, flag, flag2, syntax, ref flags, ref newHost, ref err);
				}
				else if (syntax.InFact(UriSyntaxFlags.V1_UnknownUri))
				{
					bool flag4 = false;
					int num6 = idx;
					for (i = idx; i < length && (!flag4 || (pString[i] != '/' && pString[i] != '?' && pString[i] != '#')); i++)
					{
						if (i < idx + 2 && pString[i] == '.')
						{
							flag4 = true;
							continue;
						}
						err = ParsingError.BadHostName;
						flags |= Flags.HostTypeMask;
						return idx;
					}
					flags |= Flags.BasicHostType;
					if (flag && flag2 && StaticNotAny(flags, Flags.HostUnicodeNormalized))
					{
						string text2 = new string(pString, num6, num6 - i);
						try
						{
							newHost += text2.Normalize(NormalizationForm.FormC);
						}
						catch (ArgumentException)
						{
							err = ParsingError.BadFormat;
							return idx;
						}
						flags |= Flags.HostUnicodeNormalized;
					}
				}
				else if (syntax.InFact(UriSyntaxFlags.MustHaveAuthority))
				{
					err = ParsingError.BadHostName;
					flags |= Flags.HostTypeMask;
					return idx;
				}
			}
			return (ushort)i;
		}

		private unsafe void CheckAuthorityHelperHandleDnsIri(char* pString, ushort start, int end, int startInput, bool iriParsing, bool hasUnicode, UriParser syntax, string userInfoString, ref Flags flags, ref bool justNormalized, ref string newHost, ref ParsingError err)
		{
			flags |= Flags.DnsHostType;
			if (s_IdnScope == UriIdnScope.AllExceptIntranet && IsIntranet(new string(pString, 0, end)))
			{
				flags |= Flags.IntranetUri;
			}
			if (AllowIdnStatic(syntax, flags))
			{
				bool allAscii = true;
				bool atLeastOneValidIdn = false;
				string text = DomainNameHelper.IdnEquivalent(pString, start, end, ref allAscii, ref atLeastOneValidIdn);
				string text2 = DomainNameHelper.UnicodeEquivalent(text, pString, start, end);
				if (!allAscii)
				{
					flags |= Flags.UnicodeHost;
				}
				if (atLeastOneValidIdn)
				{
					flags |= Flags.IdnHost;
				}
				if (allAscii && atLeastOneValidIdn && StaticNotAny(flags, Flags.HasUnicode))
				{
					m_originalUnicodeString = m_String;
					newHost = m_originalUnicodeString.Substring(0, startInput) + (StaticInFact(flags, Flags.HasUserInfo) ? userInfoString : null);
					justNormalized = true;
				}
				else if (!iriParsing && (StaticInFact(flags, Flags.UnicodeHost) || StaticInFact(flags, Flags.IdnHost)))
				{
					m_originalUnicodeString = m_String;
					newHost = m_originalUnicodeString.Substring(0, startInput) + (StaticInFact(flags, Flags.HasUserInfo) ? userInfoString : null);
					justNormalized = true;
				}
				if (!allAscii || atLeastOneValidIdn)
				{
					m_DnsSafeHost = text;
					newHost += text2;
					justNormalized = true;
				}
				else if (allAscii && !atLeastOneValidIdn && iriParsing && hasUnicode)
				{
					newHost += text2;
					justNormalized = true;
				}
			}
			else if (hasUnicode)
			{
				string text3 = StripBidiControlCharacter(pString, start, end - start);
				try
				{
					newHost += text3?.Normalize(NormalizationForm.FormC);
				}
				catch (ArgumentException)
				{
					err = ParsingError.BadHostName;
				}
				justNormalized = true;
			}
			flags |= Flags.HostUnicodeNormalized;
		}

		private unsafe void CheckAuthorityHelperHandleAnyHostIri(char* pString, int startInput, int end, bool iriParsing, bool hasUnicode, UriParser syntax, ref Flags flags, ref string newHost, ref ParsingError err)
		{
			if (!StaticNotAny(flags, Flags.HostUnicodeNormalized) || (!AllowIdnStatic(syntax, flags) && (!iriParsing || !hasUnicode)))
			{
				return;
			}
			string text = new string(pString, startInput, end - startInput);
			if (AllowIdnStatic(syntax, flags))
			{
				bool allAscii = true;
				bool atLeastOneValidIdn = false;
				string text2 = DomainNameHelper.UnicodeEquivalent(pString, startInput, end, ref allAscii, ref atLeastOneValidIdn);
				if (((allAscii && atLeastOneValidIdn) || !allAscii) && (!iriParsing || !hasUnicode))
				{
					m_originalUnicodeString = m_String;
					newHost = m_originalUnicodeString.Substring(0, startInput);
					flags |= Flags.HasUnicode;
				}
				if (atLeastOneValidIdn || !allAscii)
				{
					newHost += text2;
					string bidiStrippedHost = null;
					m_DnsSafeHost = DomainNameHelper.IdnEquivalent(pString, startInput, end, ref allAscii, ref bidiStrippedHost);
					if (atLeastOneValidIdn)
					{
						flags |= Flags.IdnHost;
					}
					if (!allAscii)
					{
						flags |= Flags.UnicodeHost;
					}
				}
				else if (iriParsing && hasUnicode)
				{
					newHost += text;
				}
			}
			else
			{
				try
				{
					newHost += text.Normalize(NormalizationForm.FormC);
				}
				catch (ArgumentException)
				{
					err = ParsingError.BadHostName;
				}
			}
			flags |= Flags.HostUnicodeNormalized;
		}

		private unsafe void FindEndOfComponent(string input, ref ushort idx, ushort end, char delim)
		{
			fixed (char* str = input)
			{
				FindEndOfComponent(str, ref idx, end, delim);
			}
		}

		private unsafe void FindEndOfComponent(char* str, ref ushort idx, ushort end, char delim)
		{
			char c = '\uffff';
			ushort num;
			for (num = idx; num < end; num = (ushort)(num + 1))
			{
				c = str[(int)num];
				if (c == delim || (delim == '?' && c == '#' && m_Syntax != null && m_Syntax.InFact(UriSyntaxFlags.MayHaveFragment)))
				{
					break;
				}
			}
			idx = num;
		}

		private unsafe Check CheckCanonical(char* str, ref ushort idx, ushort end, char delim)
		{
			Check check = Check.None;
			bool flag = false;
			bool flag2 = false;
			char c = '\uffff';
			ushort num;
			for (num = idx; num < end; num = (ushort)(num + 1))
			{
				c = str[(int)num];
				if (c <= '\u001f' || (c >= '\u007f' && c <= '\u009f'))
				{
					flag = true;
					flag2 = true;
					check |= Check.ReservedFound;
				}
				else if (c > 'z' && c != '~')
				{
					if (m_iriParsing)
					{
						bool flag3 = false;
						check |= Check.FoundNonAscii;
						if (char.IsHighSurrogate(c))
						{
							if (num + 1 < end)
							{
								bool surrogatePair = false;
								flag3 = CheckIriUnicodeRange(c, str[num + 1], ref surrogatePair, isQuery: true);
							}
						}
						else
						{
							flag3 = CheckIriUnicodeRange(c, isQuery: true);
						}
						if (!flag3)
						{
							check |= Check.NotIriCanonical;
						}
					}
					if (!flag)
					{
						flag = true;
					}
				}
				else
				{
					if (c == delim || (delim == '?' && c == '#' && m_Syntax != null && m_Syntax.InFact(UriSyntaxFlags.MayHaveFragment)))
					{
						break;
					}
					switch (c)
					{
					case '?':
						if (IsImplicitFile || (m_Syntax != null && !m_Syntax.InFact(UriSyntaxFlags.MayHaveQuery) && delim != '\ufffe'))
						{
							check |= Check.ReservedFound;
							flag2 = true;
							flag = true;
						}
						break;
					case '#':
						flag = true;
						if (IsImplicitFile || (m_Syntax != null && !m_Syntax.InFact(UriSyntaxFlags.MayHaveFragment)))
						{
							check |= Check.ReservedFound;
							flag2 = true;
						}
						break;
					case '/':
					case '\\':
						if ((check & Check.BackslashInPath) == 0 && c == '\\')
						{
							check |= Check.BackslashInPath;
						}
						if ((check & Check.DotSlashAttn) == 0 && num + 1 != end && (str[num + 1] == '/' || str[num + 1] == '\\'))
						{
							check |= Check.DotSlashAttn;
						}
						break;
					case '.':
						if (((check & Check.DotSlashAttn) == 0 && num + 1 == end) || str[num + 1] == '.' || str[num + 1] == '/' || str[num + 1] == '\\' || str[num + 1] == '?' || str[num + 1] == '#')
						{
							check |= Check.DotSlashAttn;
						}
						break;
					default:
						if (!flag && ((c <= '"' && c != '!') || (c >= '[' && c <= '^') || c == '>' || c == '<' || c == '`'))
						{
							flag = true;
						}
						else
						{
							if (c != '%')
							{
								break;
							}
							if (!flag2)
							{
								flag2 = true;
							}
							if (num + 2 < end && (c = EscapedAscii(str[num + 1], str[num + 2])) != '\uffff')
							{
								if (c == '.' || c == '/' || c == '\\')
								{
									check |= Check.DotSlashEscaped;
								}
								num = (ushort)(num + 2);
							}
							else if (!flag)
							{
								flag = true;
							}
						}
						break;
					}
				}
			}
			if (flag2)
			{
				if (!flag)
				{
					check |= Check.EscapedCanonical;
				}
			}
			else
			{
				check |= Check.DisplayCanonical;
				if (!flag)
				{
					check |= Check.EscapedCanonical;
				}
			}
			idx = num;
			return check;
		}

		private unsafe char[] GetCanonicalPath(char[] dest, ref int pos, UriFormat formatAs)
		{
			if (InFact(Flags.FirstSlashAbsent))
			{
				dest[pos++] = '/';
			}
			if (m_Info.Offset.Path == m_Info.Offset.Query)
			{
				return dest;
			}
			int end = pos;
			int securedPathIndex = SecuredPathIndex;
			if (formatAs == UriFormat.UriEscaped)
			{
				if (InFact(Flags.ShouldBeCompressed))
				{
					m_String.CopyTo(m_Info.Offset.Path, dest, end, m_Info.Offset.Query - m_Info.Offset.Path);
					end += m_Info.Offset.Query - m_Info.Offset.Path;
					if (m_Syntax.InFact(UriSyntaxFlags.UnEscapeDotsAndSlashes) && InFact(Flags.PathNotCanonical) && !IsImplicitFile)
					{
						fixed (char* pch = dest)
						{
							UnescapeOnly(pch, pos, ref end, '.', '/', m_Syntax.InFact(UriSyntaxFlags.ConvertPathSlashes) ? '\\' : '\uffff');
						}
					}
				}
				else if (InFact(Flags.E_PathNotCanonical) && NotAny(Flags.UserEscaped))
				{
					string text = m_String;
					if (securedPathIndex != 0 && text[securedPathIndex + m_Info.Offset.Path - 1] == '|')
					{
						text = text.Remove(securedPathIndex + m_Info.Offset.Path - 1, 1);
						text = text.Insert(securedPathIndex + m_Info.Offset.Path - 1, ":");
					}
					dest = EscapeString(text, m_Info.Offset.Path, m_Info.Offset.Query, dest, ref end, isUriString: true, '?', '#', IsImplicitFile ? '\uffff' : '%');
				}
				else
				{
					m_String.CopyTo(m_Info.Offset.Path, dest, end, m_Info.Offset.Query - m_Info.Offset.Path);
					end += m_Info.Offset.Query - m_Info.Offset.Path;
				}
			}
			else
			{
				m_String.CopyTo(m_Info.Offset.Path, dest, end, m_Info.Offset.Query - m_Info.Offset.Path);
				end += m_Info.Offset.Query - m_Info.Offset.Path;
				if (InFact(Flags.ShouldBeCompressed) && m_Syntax.InFact(UriSyntaxFlags.UnEscapeDotsAndSlashes) && InFact(Flags.PathNotCanonical) && !IsImplicitFile)
				{
					fixed (char* pch2 = dest)
					{
						UnescapeOnly(pch2, pos, ref end, '.', '/', m_Syntax.InFact(UriSyntaxFlags.ConvertPathSlashes) ? '\\' : '\uffff');
					}
				}
			}
			if (securedPathIndex != 0 && dest[securedPathIndex + pos - 1] == '|')
			{
				dest[securedPathIndex + pos - 1] = ':';
			}
			if (InFact(Flags.ShouldBeCompressed))
			{
				dest = Compress(dest, (ushort)(pos + securedPathIndex), ref end, m_Syntax);
				if (dest[pos] == '\\')
				{
					dest[pos] = '/';
				}
				if (formatAs == UriFormat.UriEscaped && NotAny(Flags.UserEscaped) && InFact(Flags.E_PathNotCanonical))
				{
					string input = new string(dest, pos, end - pos);
					dest = EscapeString(input, 0, end - pos, dest, ref pos, isUriString: true, '?', '#', IsImplicitFile ? '\uffff' : '%');
					end = pos;
				}
			}
			else if (m_Syntax.InFact(UriSyntaxFlags.ConvertPathSlashes) && InFact(Flags.BackslashInPath))
			{
				for (int i = pos; i < end; i++)
				{
					if (dest[i] == '\\')
					{
						dest[i] = '/';
					}
				}
			}
			if (formatAs != UriFormat.UriEscaped && InFact(Flags.PathNotCanonical))
			{
				UnescapeMode unescapeMode;
				if (InFact(Flags.PathNotCanonical))
				{
					switch (formatAs)
					{
					case (UriFormat)32767:
						unescapeMode = (InFact(Flags.UserEscaped) ? UnescapeMode.Unescape : UnescapeMode.EscapeUnescape) | UnescapeMode.V1ToStringFlag;
						if (IsImplicitFile)
						{
							unescapeMode &= ~UnescapeMode.Unescape;
						}
						break;
					case UriFormat.Unescaped:
						unescapeMode = ((!IsImplicitFile) ? (UnescapeMode.Unescape | UnescapeMode.UnescapeAll) : UnescapeMode.CopyOnly);
						break;
					default:
						unescapeMode = (InFact(Flags.UserEscaped) ? UnescapeMode.Unescape : UnescapeMode.EscapeUnescape);
						if (IsImplicitFile)
						{
							unescapeMode &= ~UnescapeMode.Unescape;
						}
						break;
					}
				}
				else
				{
					unescapeMode = UnescapeMode.CopyOnly;
				}
				char[] array = new char[dest.Length];
				Buffer.BlockCopy(dest, 0, array, 0, end << 1);
				fixed (char* pStr = array)
				{
					dest = UnescapeString(pStr, pos, end, dest, ref pos, '?', '#', '\uffff', unescapeMode, m_Syntax, isQuery: false, readOnlyConfig: false);
				}
			}
			else
			{
				pos = end;
			}
			return dest;
		}

		private unsafe static void UnescapeOnly(char* pch, int start, ref int end, char ch1, char ch2, char ch3)
		{
			if (end - start < 3)
			{
				return;
			}
			char* ptr = pch + end - 2;
			pch += start;
			char* ptr2 = null;
			while (pch < ptr)
			{
				char* num = pch;
				pch = num + 1;
				if (*num != '%')
				{
					continue;
				}
				char* num2 = pch;
				pch = num2 + 1;
				char digit = *num2;
				char* num3 = pch;
				pch = num3 + 1;
				char c = EscapedAscii(digit, *num3);
				if (c != ch1 && c != ch2 && c != ch3)
				{
					continue;
				}
				ptr2 = pch - 2;
				*(ptr2 - 1) = c;
				while (pch < ptr)
				{
					char* num4 = ptr2;
					ptr2 = num4 + 1;
					char* num5 = pch;
					pch = num5 + 1;
					char c2;
					*num4 = (c2 = *num5);
					if (c2 == '%')
					{
						char* num6 = ptr2;
						ptr2 = num6 + 1;
						char* num7 = pch;
						pch = num7 + 1;
						char digit2;
						*num6 = (digit2 = *num7);
						char* num8 = ptr2;
						ptr2 = num8 + 1;
						char* num9 = pch;
						pch = num9 + 1;
						char next;
						*num8 = (next = *num9);
						c = EscapedAscii(digit2, next);
						if (c == ch1 || c == ch2 || c == ch3)
						{
							ptr2 -= 2;
							*(ptr2 - 1) = c;
						}
					}
				}
				break;
			}
			ptr += 2;
			if (ptr2 == null)
			{
				return;
			}
			if (pch == ptr)
			{
				end -= (int)(pch - ptr2);
				return;
			}
			char* num10 = ptr2;
			ptr2 = num10 + 1;
			char* num11 = pch;
			pch = num11 + 1;
			*num10 = *num11;
			if (pch == ptr)
			{
				end -= (int)(pch - ptr2);
				return;
			}
			char* num12 = ptr2;
			ptr2 = num12 + 1;
			char* num13 = pch;
			pch = num13 + 1;
			*num12 = *num13;
			end -= (int)(pch - ptr2);
		}

		private static char EscapedAscii(char digit, char next)
		{
			if ((digit < '0' || digit > '9') && (digit < 'A' || digit > 'F') && (digit < 'a' || digit > 'f'))
			{
				return '\uffff';
			}
			int num = ((digit <= '9') ? (digit - 48) : (((digit <= 'F') ? (digit - 65) : (digit - 97)) + 10));
			if ((next < '0' || next > '9') && (next < 'A' || next > 'F') && (next < 'a' || next > 'f'))
			{
				return '\uffff';
			}
			return (char)((num << 4) + ((next <= '9') ? (next - 48) : (((next <= 'F') ? (next - 65) : (next - 97)) + 10)));
		}

		private static char[] Compress(char[] dest, ushort start, ref int destLength, UriParser syntax)
		{
			ushort num = 0;
			ushort num2 = 0;
			ushort num3 = 0;
			ushort num4 = 0;
			ushort num5 = (ushort)((ushort)destLength - 1);
			for (start = (ushort)(start - 1); num5 != start; num5 = (ushort)(num5 - 1))
			{
				char c = dest[num5];
				if (c == '\\' && syntax.InFact(UriSyntaxFlags.ConvertPathSlashes))
				{
					c = (dest[num5] = '/');
				}
				if (c == '/')
				{
					num = (ushort)(num + 1);
				}
				else
				{
					if (num > 1)
					{
						num2 = (ushort)(num5 + 1);
					}
					num = 0;
				}
				if (c == '.')
				{
					num3 = (ushort)(num3 + 1);
					continue;
				}
				if (num3 != 0)
				{
					bool flag = syntax.NotAny(UriSyntaxFlags.CanonicalizeAsFilePath) && (num3 > 2 || c != '/' || num5 == start);
					if (!flag && c == '/')
					{
						if (num2 == num5 + num3 + 1 || (num2 == 0 && num5 + num3 + 1 == destLength))
						{
							num2 = (ushort)(num5 + 1 + num3 + ((num2 != 0) ? 1 : 0));
							Buffer.BlockCopy(dest, num2 << 1, dest, num5 + 1 << 1, destLength - num2 << 1);
							destLength -= num2 - num5 - 1;
							num2 = num5;
							if (num3 == 2)
							{
								num4 = (ushort)(num4 + 1);
							}
							num3 = 0;
							continue;
						}
					}
					else if (!flag && num4 == 0 && (num2 == num5 + num3 + 1 || (num2 == 0 && num5 + num3 + 1 == destLength)))
					{
						num3 = (ushort)(num5 + 1 + num3);
						Buffer.BlockCopy(dest, num3 << 1, dest, num5 + 1 << 1, destLength - num3 << 1);
						destLength -= num3 - num5 - 1;
						num2 = 0;
						num3 = 0;
						continue;
					}
					num3 = 0;
				}
				if (c == '/')
				{
					if (num4 != 0)
					{
						num4 = (ushort)(num4 - 1);
						num2 = (ushort)(num2 + 1);
						Buffer.BlockCopy(dest, num2 << 1, dest, num5 + 1 << 1, destLength - num2 << 1);
						destLength -= num2 - num5 - 1;
					}
					num2 = num5;
				}
			}
			start = (ushort)(start + 1);
			if ((ushort)destLength > start && syntax.InFact(UriSyntaxFlags.CanonicalizeAsFilePath) && num <= 1)
			{
				if (num4 != 0 && dest[start] != '/')
				{
					num2 = (ushort)(num2 + 1);
					Buffer.BlockCopy(dest, num2 << 1, dest, start << 1, destLength - num2 << 1);
					destLength -= num2;
				}
				else if (num3 != 0 && (num2 == num3 + 1 || (num2 == 0 && num3 + 1 == destLength)))
				{
					num3 = (ushort)(num3 + ((num2 != 0) ? 1 : 0));
					Buffer.BlockCopy(dest, num3 << 1, dest, start << 1, destLength - num3 << 1);
					destLength -= num3;
				}
			}
			return dest;
		}

		private static void EscapeAsciiChar(char ch, char[] to, ref int pos)
		{
			to[pos++] = '%';
			to[pos++] = HexUpperChars[(ch & 0xF0) >> 4];
			to[pos++] = HexUpperChars[ch & 0xF];
		}

		internal static int CalculateCaseInsensitiveHashCode(string text)
		{
			return StringComparer.InvariantCultureIgnoreCase.GetHashCode(text);
		}

		private static string CombineUri(Uri basePart, string relativePart, UriFormat uriFormat)
		{
			char c = relativePart[0];
			if (basePart.IsDosPath && (c == '/' || c == '\\') && (relativePart.Length == 1 || (relativePart[1] != '/' && relativePart[1] != '\\')))
			{
				int num = basePart.OriginalString.IndexOf(':');
				if (basePart.IsImplicitFile)
				{
					return basePart.OriginalString.Substring(0, num + 1) + relativePart;
				}
				num = basePart.OriginalString.IndexOf(':', num + 1);
				return basePart.OriginalString.Substring(0, num + 1) + relativePart;
			}
			if (StaticIsFile(basePart.Syntax) && (c == '\\' || c == '/'))
			{
				if (relativePart.Length >= 2 && (relativePart[1] == '\\' || relativePart[1] == '/'))
				{
					if (!basePart.IsImplicitFile)
					{
						return "file:" + relativePart;
					}
					return relativePart;
				}
				if (basePart.IsUnc)
				{
					string text = basePart.GetParts(UriComponents.Path | UriComponents.KeepDelimiter, UriFormat.Unescaped);
					for (int i = 1; i < text.Length; i++)
					{
						if (text[i] == '/')
						{
							text = text.Substring(0, i);
							break;
						}
					}
					if (basePart.IsImplicitFile)
					{
						return "\\\\" + basePart.GetParts(UriComponents.Host, UriFormat.Unescaped) + text + relativePart;
					}
					return "file://" + basePart.GetParts(UriComponents.Host, uriFormat) + text + relativePart;
				}
				return "file://" + relativePart;
			}
			bool flag = basePart.Syntax.InFact(UriSyntaxFlags.ConvertPathSlashes);
			string text2 = null;
			if (c == '/' || (c == '\\' && flag))
			{
				if (relativePart.Length >= 2 && relativePart[1] == '/')
				{
					return basePart.Scheme + ':' + relativePart;
				}
				text2 = ((basePart.HostType != Flags.IPv6HostType) ? basePart.GetParts(UriComponents.SchemeAndServer | UriComponents.UserInfo, uriFormat) : (basePart.GetParts(UriComponents.Scheme | UriComponents.UserInfo, uriFormat) + '[' + basePart.DnsSafeHost + ']' + basePart.GetParts(UriComponents.Port | UriComponents.KeepDelimiter, uriFormat)));
				if (flag && c == '\\')
				{
					relativePart = '/' + relativePart.Substring(1);
				}
				return text2 + relativePart;
			}
			text2 = basePart.GetParts(UriComponents.Path | UriComponents.KeepDelimiter, basePart.IsImplicitFile ? UriFormat.Unescaped : uriFormat);
			int num2 = text2.Length;
			char[] array = new char[num2 + relativePart.Length];
			if (num2 > 0)
			{
				text2.CopyTo(0, array, 0, num2);
				while (num2 > 0)
				{
					if (array[--num2] == '/')
					{
						num2++;
						break;
					}
				}
			}
			relativePart.CopyTo(0, array, num2, relativePart.Length);
			c = (basePart.Syntax.InFact(UriSyntaxFlags.MayHaveQuery) ? '?' : '\uffff');
			char c2 = ((!basePart.IsImplicitFile && basePart.Syntax.InFact(UriSyntaxFlags.MayHaveFragment)) ? '#' : '\uffff');
			string text3 = string.Empty;
			if (c != '\uffff' || c2 != '\uffff')
			{
				int j;
				for (j = 0; j < relativePart.Length && array[num2 + j] != c && array[num2 + j] != c2; j++)
				{
				}
				if (j == 0)
				{
					text3 = relativePart;
				}
				else if (j < relativePart.Length)
				{
					text3 = relativePart.Substring(j);
				}
				num2 += j;
			}
			else
			{
				num2 += relativePart.Length;
			}
			if (basePart.HostType == Flags.IPv6HostType)
			{
				text2 = ((!basePart.IsImplicitFile) ? (basePart.GetParts(UriComponents.Scheme | UriComponents.UserInfo, uriFormat) + '[' + basePart.DnsSafeHost + ']' + basePart.GetParts(UriComponents.Port | UriComponents.KeepDelimiter, uriFormat)) : ("\\\\[" + basePart.DnsSafeHost + ']'));
			}
			else if (basePart.IsImplicitFile)
			{
				if (basePart.IsDosPath)
				{
					array = Compress(array, 3, ref num2, basePart.Syntax);
					return new string(array, 1, num2 - 1) + text3;
				}
				text2 = "\\\\" + basePart.GetParts(UriComponents.Host, UriFormat.Unescaped);
			}
			else
			{
				text2 = basePart.GetParts(UriComponents.SchemeAndServer | UriComponents.UserInfo, uriFormat);
			}
			array = Compress(array, basePart.SecuredPathIndex, ref num2, basePart.Syntax);
			return text2 + new string(array, 0, num2) + text3;
		}

		private static string PathDifference(string path1, string path2, bool compareCase)
		{
			int num = -1;
			int i;
			for (i = 0; i < path1.Length && i < path2.Length && (path1[i] == path2[i] || (!compareCase && char.ToLower(path1[i], CultureInfo.InvariantCulture) == char.ToLower(path2[i], CultureInfo.InvariantCulture))); i++)
			{
				if (path1[i] == '/')
				{
					num = i;
				}
			}
			if (i == 0)
			{
				return path2;
			}
			if (i == path1.Length && i == path2.Length)
			{
				return string.Empty;
			}
			StringBuilder stringBuilder = new StringBuilder();
			for (; i < path1.Length; i++)
			{
				if (path1[i] == '/')
				{
					stringBuilder.Append("../");
				}
			}
			return stringBuilder.ToString() + path2.Substring(num + 1);
		}

		private static bool IsLWS(char ch)
		{
			if (ch <= ' ')
			{
				if (ch != ' ' && ch != '\n' && ch != '\r')
				{
					return ch == '\t';
				}
				return true;
			}
			return false;
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

		internal static bool IsGenDelim(char ch)
		{
			if (ch != ':' && ch != '/' && ch != '?' && ch != '#' && ch != '[' && ch != ']')
			{
				return ch == '@';
			}
			return true;
		}

		internal static bool IsBidiControlCharacter(char ch)
		{
			if (ch != '\u200e' && ch != '\u200f' && ch != '\u202a' && ch != '\u202b' && ch != '\u202c' && ch != '\u202d')
			{
				return ch == '\u202e';
			}
			return true;
		}

		internal unsafe static string StripBidiControlCharacter(char* strToClean, int start, int length)
		{
			if (length <= 0)
			{
				return "";
			}
			char[] array = new char[length];
			int length2 = 0;
			for (int i = 0; i < length; i++)
			{
				char c = strToClean[start + i];
				if (c < '\u200e' || c > '\u202e' || !IsBidiControlCharacter(c))
				{
					array[length2++] = c;
				}
			}
			return new string(array, 0, length2);
		}

		[Obsolete("The method has been deprecated. It is not used by the system. http://go.microsoft.com/fwlink/?linkid=14202")]
		protected virtual void Parse()
		{
		}

		[Obsolete("The method has been deprecated. It is not used by the system. http://go.microsoft.com/fwlink/?linkid=14202")]
		protected virtual void Canonicalize()
		{
		}

		[Obsolete("The method has been deprecated. It is not used by the system. http://go.microsoft.com/fwlink/?linkid=14202")]
		protected virtual void Escape()
		{
		}

		[Obsolete("The method has been deprecated. Please use GetComponents() or static UnescapeDataString() to unescape a Uri component or a string. http://go.microsoft.com/fwlink/?linkid=14202")]
		protected virtual string Unescape(string path)
		{
			char[] dest = new char[path.Length];
			int destPosition = 0;
			dest = UnescapeString(path, 0, path.Length, dest, ref destPosition, '\uffff', '\uffff', '\uffff', UnescapeMode.Unescape | UnescapeMode.UnescapeAll, null, isQuery: false, readOnlyConfig: true);
			return new string(dest, 0, destPosition);
		}

		[Obsolete("The method has been deprecated. Please use GetComponents() or static EscapeUriString() to escape a Uri component or a string. http://go.microsoft.com/fwlink/?linkid=14202")]
		protected static string EscapeString(string str)
		{
			if (str == null)
			{
				return string.Empty;
			}
			int destPos = 0;
			char[] array = EscapeString(str, 0, str.Length, null, ref destPos, isUriString: true, '?', '#', '%');
			if (array == null)
			{
				return str;
			}
			return new string(array, 0, destPos);
		}

		[Obsolete("The method has been deprecated. It is not used by the system. http://go.microsoft.com/fwlink/?linkid=14202")]
		protected virtual void CheckSecurity()
		{
			_ = Scheme == "telnet";
		}

		[Obsolete("The method has been deprecated. It is not used by the system. http://go.microsoft.com/fwlink/?linkid=14202")]
		protected virtual bool IsReservedCharacter(char character)
		{
			if (character != ';' && character != '/' && character != ':' && character != '@' && character != '&' && character != '=' && character != '+' && character != '$')
			{
				return character == ',';
			}
			return true;
		}

		[Obsolete("The method has been deprecated. It is not used by the system. http://go.microsoft.com/fwlink/?linkid=14202")]
		protected static bool IsExcludedCharacter(char character)
		{
			if (character > ' ' && character < '\u007f' && character != '<' && character != '>' && character != '#' && character != '%' && character != '"' && character != '{' && character != '}' && character != '|' && character != '\\' && character != '^' && character != '[' && character != ']')
			{
				return character == '`';
			}
			return true;
		}

		[Obsolete("The method has been deprecated. It is not used by the system. http://go.microsoft.com/fwlink/?linkid=14202")]
		protected virtual bool IsBadFileSystemCharacter(char character)
		{
			if (character >= ' ' && character != ';' && character != '/' && character != '?' && character != ':' && character != '&' && character != '=' && character != ',' && character != '*' && character != '<' && character != '>' && character != '"' && character != '|' && character != '\\')
			{
				return character == '^';
			}
			return true;
		}

		private void CreateThis(string uri, bool dontEscape, UriKind uriKind)
		{
			if (uriKind < UriKind.RelativeOrAbsolute || uriKind > UriKind.Relative)
			{
				throw new ArgumentException(SR.GetString("net_uri_InvalidUriKind", uriKind));
			}
			m_String = ((uri == null) ? string.Empty : uri);
			if (dontEscape)
			{
				m_Flags |= Flags.UserEscaped;
			}
			ParsingError err = ParseScheme(m_String, ref m_Flags, ref m_Syntax);
			InitializeUri(err, uriKind, out var e);
			if (e != null)
			{
				throw e;
			}
		}

		private void InitializeUri(ParsingError err, UriKind uriKind, out UriFormatException e)
		{
			if (err == ParsingError.None)
			{
				if (IsImplicitFile)
				{
					if (NotAny(Flags.DosPath) && uriKind != UriKind.Absolute && (uriKind == UriKind.Relative || (m_String.Length >= 2 && (m_String[0] != '\\' || m_String[1] != '\\'))))
					{
						m_Syntax = null;
						m_Flags &= Flags.UserEscaped;
						e = null;
						return;
					}
					if (uriKind == UriKind.Relative && InFact(Flags.DosPath))
					{
						m_Syntax = null;
						m_Flags &= Flags.UserEscaped;
						e = null;
						return;
					}
				}
			}
			else if (err > ParsingError.EmptyUriString)
			{
				m_String = null;
				e = GetException(err);
				return;
			}
			bool flag = false;
			if (!s_ConfigInitialized && CheckForConfigLoad(m_String))
			{
				InitializeUriConfig();
			}
			m_iriParsing = s_IriParsing && (m_Syntax == null || m_Syntax.InFact(UriSyntaxFlags.AllowIriParsing));
			if (m_iriParsing && CheckForUnicode(m_String))
			{
				m_Flags |= Flags.HasUnicode;
				flag = true;
				m_originalUnicodeString = m_String;
			}
			if (m_Syntax != null)
			{
				if (m_Syntax.IsSimple)
				{
					if ((err = PrivateParseMinimal()) != 0)
					{
						if (uriKind != UriKind.Absolute && err <= ParsingError.EmptyUriString)
						{
							m_Syntax = null;
							e = null;
							m_Flags &= Flags.UserEscaped;
						}
						else
						{
							e = GetException(err);
						}
					}
					else if (uriKind == UriKind.Relative)
					{
						e = GetException(ParsingError.CannotCreateRelative);
					}
					else
					{
						e = null;
					}
					if (!m_iriParsing || !flag)
					{
						return;
					}
					try
					{
						EnsureParseRemaining();
					}
					catch (UriFormatException ex)
					{
						if (ServicePointManager.AllowAllUriEncodingExpansion)
						{
							throw;
						}
						e = ex;
					}
					return;
				}
				m_Syntax = m_Syntax.InternalOnNewUri();
				m_Flags |= Flags.UserDrivenParsing;
				m_Syntax.InternalValidate(this, out e);
				if (e != null)
				{
					if (uriKind != UriKind.Absolute && err != 0 && err <= ParsingError.EmptyUriString)
					{
						m_Syntax = null;
						e = null;
						m_Flags &= Flags.UserEscaped;
					}
					return;
				}
				if (err != 0 || InFact(Flags.ErrorOrParsingRecursion))
				{
					SetUserDrivenParsing();
				}
				else if (uriKind == UriKind.Relative)
				{
					e = GetException(ParsingError.CannotCreateRelative);
				}
				if (!m_iriParsing || !flag)
				{
					return;
				}
				try
				{
					EnsureParseRemaining();
				}
				catch (UriFormatException ex2)
				{
					if (ServicePointManager.AllowAllUriEncodingExpansion)
					{
						throw;
					}
					e = ex2;
				}
			}
			else if (err != 0 && uriKind != UriKind.Absolute && err <= ParsingError.EmptyUriString)
			{
				e = null;
				m_Flags &= Flags.UserEscaped | Flags.HasUnicode;
				if (m_iriParsing && flag)
				{
					m_String = EscapeUnescapeIri(m_originalUnicodeString, 0, m_originalUnicodeString.Length, (UriComponents)0);
					try
					{
						m_String = m_String.Normalize(NormalizationForm.FormC);
					}
					catch (ArgumentException)
					{
						e = GetException(ParsingError.BadFormat);
					}
				}
			}
			else
			{
				m_String = null;
				e = GetException(err);
			}
		}

		private unsafe bool CheckForConfigLoad(string data)
		{
			bool result = false;
			int length = data.Length;
			fixed (char* ptr = data)
			{
				for (int i = 0; i < length; i++)
				{
					if (ptr[i] > '\u007f' || ptr[i] == '%' || (ptr[i] == 'x' && i + 3 < length && ptr[i + 1] == 'n' && ptr[i + 2] == '-' && ptr[i + 3] == '-'))
					{
						result = true;
						break;
					}
				}
			}
			return result;
		}

		private unsafe bool CheckForUnicode(string data)
		{
			bool result = false;
			char[] dest = new char[data.Length];
			int destPosition = 0;
			dest = UnescapeString(data, 0, data.Length, dest, ref destPosition, '\uffff', '\uffff', '\uffff', UnescapeMode.Unescape | UnescapeMode.UnescapeAll, null, isQuery: false, readOnlyConfig: false);
			string text = new string(dest, 0, destPosition);
			int length = text.Length;
			fixed (char* ptr = text)
			{
				for (int i = 0; i < length; i++)
				{
					if (ptr[i] > '\u007f')
					{
						result = true;
						break;
					}
				}
			}
			return result;
		}

		internal static bool CheckIriUnicodeRange(string uri, int offset, ref bool surrogatePair, bool isQuery)
		{
			char c = '\uffff';
			return CheckIriUnicodeRange(uri[offset], (offset + 1 < uri.Length) ? uri[offset + 1] : c, ref surrogatePair, isQuery);
		}

		internal static bool CheckIriUnicodeRange(char unicode, bool isQuery)
		{
			if ((unicode >= '\u00a0' && unicode <= '\ud7ff') || (unicode >= '豈' && unicode <= '\ufdcf') || (unicode >= 'ﷰ' && unicode <= '\uffef') || (isQuery && unicode >= '\ue000' && unicode <= '\uf8ff'))
			{
				return true;
			}
			return false;
		}

		internal static bool CheckIriUnicodeRange(char highSurr, char lowSurr, ref bool surrogatePair, bool isQuery)
		{
			bool result = false;
			surrogatePair = false;
			if (CheckIriUnicodeRange(highSurr, isQuery))
			{
				result = true;
			}
			else if (char.IsHighSurrogate(highSurr) && char.IsSurrogatePair(highSurr, lowSurr))
			{
				surrogatePair = true;
				char[] value = new char[2]
				{
					highSurr,
					lowSurr
				};
				string text = new string(value);
				if ((text.CompareTo("\ud800\udc00") >= 0 && text.CompareTo("\ud83f\udffd") <= 0) || (text.CompareTo("\ud840\udc00") >= 0 && text.CompareTo("\ud87f\udffd") <= 0) || (text.CompareTo("\ud880\udc00") >= 0 && text.CompareTo("\ud8bf\udffd") <= 0) || (text.CompareTo("\ud8c0\udc00") >= 0 && text.CompareTo("\ud8ff\udffd") <= 0) || (text.CompareTo("\ud900\udc00") >= 0 && text.CompareTo("\ud93f\udffd") <= 0) || (text.CompareTo("\ud940\udc00") >= 0 && text.CompareTo("\ud97f\udffd") <= 0) || (text.CompareTo("\ud980\udc00") >= 0 && text.CompareTo("\ud9bf\udffd") <= 0) || (text.CompareTo("\ud9c0\udc00") >= 0 && text.CompareTo("\ud9ff\udffd") <= 0) || (text.CompareTo("\uda00\udc00") >= 0 && text.CompareTo("\uda3f\udffd") <= 0) || (text.CompareTo("\uda40\udc00") >= 0 && text.CompareTo("\uda7f\udffd") <= 0) || (text.CompareTo("\uda80\udc00") >= 0 && text.CompareTo("\udabf\udffd") <= 0) || (text.CompareTo("\udac0\udc00") >= 0 && text.CompareTo("\udaff\udffd") <= 0) || (text.CompareTo("\udb00\udc00") >= 0 && text.CompareTo("\udb3f\udffd") <= 0) || (text.CompareTo("\udb40\udc00") >= 0 && text.CompareTo("\udb7f\udffd") <= 0) || (isQuery && ((text.CompareTo("\udb80\udc00") >= 0 && text.CompareTo("\udbbf\udffd") <= 0) || (text.CompareTo("\udbc0\udc00") >= 0 && text.CompareTo("\udbff\udffd") <= 0))))
				{
					result = true;
				}
			}
			return result;
		}

		public static bool TryCreate(string uriString, UriKind uriKind, out Uri result)
		{
			if (uriString == null)
			{
				result = null;
				return false;
			}
			UriFormatException e = null;
			result = CreateHelper(uriString, dontEscape: false, uriKind, ref e);
			if (e == null)
			{
				return result != null;
			}
			return false;
		}

		public static bool TryCreate(Uri baseUri, string relativeUri, out Uri result)
		{
			if (TryCreate(relativeUri, UriKind.RelativeOrAbsolute, out var result2))
			{
				if (!result2.IsAbsoluteUri)
				{
					return TryCreate(baseUri, result2, out result);
				}
				result = result2;
				return true;
			}
			result = null;
			return false;
		}

		public static bool TryCreate(Uri baseUri, Uri relativeUri, out Uri result)
		{
			result = null;
			if ((object)baseUri == null)
			{
				return false;
			}
			if (baseUri.IsNotAbsoluteUri)
			{
				return false;
			}
			string newUriString = null;
			bool userEscaped;
			UriFormatException e;
			if (baseUri.Syntax.IsSimple)
			{
				userEscaped = relativeUri.UserEscaped;
				result = ResolveHelper(baseUri, relativeUri, ref newUriString, ref userEscaped, out e);
			}
			else
			{
				userEscaped = false;
				newUriString = baseUri.Syntax.InternalResolve(baseUri, relativeUri, out e);
			}
			if (e != null)
			{
				return false;
			}
			if ((object)result == null)
			{
				result = CreateHelper(newUriString, userEscaped, UriKind.Absolute, ref e);
			}
			if (e == null && result != null)
			{
				return result.IsAbsoluteUri;
			}
			return false;
		}

		public bool IsBaseOf(Uri uri)
		{
			if (!IsAbsoluteUri)
			{
				return false;
			}
			if (Syntax.IsSimple)
			{
				return IsBaseOfHelper(uri);
			}
			return Syntax.InternalIsBaseOf(this, uri);
		}

		public string GetComponents(UriComponents components, UriFormat format)
		{
			if (((uint)components & 0x80000000u) != 0 && components != UriComponents.SerializationInfoString)
			{
				throw new ArgumentOutOfRangeException("UriComponents.SerializationInfoString");
			}
			if (((uint)format & 0xFFFFFFFCu) != 0)
			{
				throw new ArgumentOutOfRangeException("format");
			}
			if (IsNotAbsoluteUri)
			{
				if (components == UriComponents.SerializationInfoString)
				{
					return GetRelativeSerializationString(format);
				}
				throw new InvalidOperationException(SR.GetString("net_uri_NotAbsolute"));
			}
			if (Syntax.IsSimple)
			{
				return GetComponentsHelper(components, format);
			}
			return Syntax.InternalGetComponents(this, components, format);
		}

		public bool IsWellFormedOriginalString()
		{
			if (IsNotAbsoluteUri || Syntax.IsSimple)
			{
				return InternalIsWellFormedOriginalString();
			}
			return Syntax.InternalIsWellFormedOriginalString(this);
		}

		public static bool IsWellFormedUriString(string uriString, UriKind uriKind)
		{
			if (!TryCreate(uriString, uriKind, out var result))
			{
				return false;
			}
			return result.IsWellFormedOriginalString();
		}

		public static int Compare(Uri uri1, Uri uri2, UriComponents partsToCompare, UriFormat compareFormat, StringComparison comparisonType)
		{
			if ((object)uri1 == null)
			{
				if (uri2 == null)
				{
					return 0;
				}
				return -1;
			}
			if ((object)uri2 == null)
			{
				return 1;
			}
			if (!uri1.IsAbsoluteUri || !uri2.IsAbsoluteUri)
			{
				if (!uri1.IsAbsoluteUri)
				{
					if (!uri2.IsAbsoluteUri)
					{
						return string.Compare(uri1.OriginalString, uri2.OriginalString, comparisonType);
					}
					return -1;
				}
				return 1;
			}
			return string.Compare(uri1.GetParts(partsToCompare, compareFormat), uri2.GetParts(partsToCompare, compareFormat), comparisonType);
		}

		public unsafe static string UnescapeDataString(string stringToUnescape)
		{
			if (stringToUnescape == null)
			{
				throw new ArgumentNullException("stringToUnescape");
			}
			if (stringToUnescape.Length == 0)
			{
				return string.Empty;
			}
			fixed (char* ptr = stringToUnescape)
			{
				int i;
				for (i = 0; i < stringToUnescape.Length && ptr[i] != '%'; i++)
				{
				}
				if (i == stringToUnescape.Length)
				{
					return stringToUnescape;
				}
				i = 0;
				char[] dest = new char[stringToUnescape.Length];
				dest = UnescapeString(stringToUnescape, 0, stringToUnescape.Length, dest, ref i, '\uffff', '\uffff', '\uffff', UnescapeMode.UnescapeAllOrThrow | UnescapeMode.Unescape, null, isQuery: false, readOnlyConfig: true);
				return new string(dest, 0, i);
			}
		}

		public static string EscapeUriString(string stringToEscape)
		{
			if (stringToEscape == null)
			{
				throw new ArgumentNullException("stringToUnescape");
			}
			if (stringToEscape.Length == 0)
			{
				return string.Empty;
			}
			int destPos = 0;
			char[] array = EscapeString(stringToEscape, 0, stringToEscape.Length, null, ref destPos, isUriString: true, '\uffff', '\uffff', '\uffff');
			if (array == null)
			{
				return stringToEscape;
			}
			return new string(array, 0, destPos);
		}

		public static string EscapeDataString(string stringToEscape)
		{
			if (stringToEscape == null)
			{
				throw new ArgumentNullException("stringToUnescape");
			}
			if (stringToEscape.Length == 0)
			{
				return string.Empty;
			}
			int destPos = 0;
			char[] array = EscapeString(stringToEscape, 0, stringToEscape.Length, null, ref destPos, isUriString: false, '\uffff', '\uffff', '\uffff');
			if (array == null)
			{
				return stringToEscape;
			}
			return new string(array, 0, destPos);
		}

		private unsafe static char[] EscapeString(string input, int start, int end, char[] dest, ref int destPos, bool isUriString, char force1, char force2, char rsvd)
		{
			if (end - start >= 65520)
			{
				throw GetException(ParsingError.SizeLimit);
			}
			int i = start;
			int num = start;
			byte* ptr = stackalloc byte[1 * 160];
			fixed (char* ptr2 = input)
			{
				for (; i < end; i++)
				{
					char c = ptr2[i];
					if (c > '\u007f')
					{
						short num2 = (short)Math.Min(end - i, 39);
						short num3 = 1;
						while (num3 < num2 && ptr2[i + num3] > '\u007f')
						{
							num3 = (short)(num3 + 1);
						}
						if (ptr2[i + num3 - 1] >= '\ud800' && ptr2[i + num3 - 1] <= '\udbff')
						{
							if (num3 == 1 || num3 == end - i)
							{
								throw new UriFormatException(SR.GetString("net_uri_BadString"));
							}
							num3 = (short)(num3 + 1);
						}
						dest = EnsureDestinationSize(ptr2, dest, i, (short)(num3 * 4 * 3), 480, ref destPos, num);
						short num4 = (short)Encoding.UTF8.GetBytes(ptr2 + i, num3, ptr, 160);
						if (num4 == 0)
						{
							throw new UriFormatException(SR.GetString("net_uri_BadString"));
						}
						i += num3 - 1;
						for (num3 = 0; num3 < num4; num3 = (short)(num3 + 1))
						{
							EscapeAsciiChar((char)ptr[num3], dest, ref destPos);
						}
						num = i + 1;
					}
					else if (c == '%' && rsvd == '%')
					{
						dest = EnsureDestinationSize(ptr2, dest, i, 3, 120, ref destPos, num);
						if (i + 2 < end && EscapedAscii(ptr2[i + 1], ptr2[i + 2]) != '\uffff')
						{
							dest[destPos++] = '%';
							dest[destPos++] = ptr2[i + 1];
							dest[destPos++] = ptr2[i + 2];
							i += 2;
						}
						else
						{
							EscapeAsciiChar('%', dest, ref destPos);
						}
						num = i + 1;
					}
					else if (c == force1 || c == force2)
					{
						dest = EnsureDestinationSize(ptr2, dest, i, 3, 120, ref destPos, num);
						EscapeAsciiChar(c, dest, ref destPos);
						num = i + 1;
					}
					else if (c != rsvd && (isUriString ? IsNotReservedNotUnreservedNotHash(c) : IsNotUnreserved(c)))
					{
						dest = EnsureDestinationSize(ptr2, dest, i, 3, 120, ref destPos, num);
						EscapeAsciiChar(c, dest, ref destPos);
						num = i + 1;
					}
				}
				if (num != i && (num != start || dest != null))
				{
					dest = EnsureDestinationSize(ptr2, dest, i, 0, 0, ref destPos, num);
				}
			}
			return dest;
		}

		private unsafe static char[] EnsureDestinationSize(char* pStr, char[] dest, int currentInputPos, short charsToAdd, short minReallocateChars, ref int destPos, int prevInputPos)
		{
			if (dest == null || dest.Length < destPos + (currentInputPos - prevInputPos) + charsToAdd)
			{
				char[] array = new char[destPos + (currentInputPos - prevInputPos) + minReallocateChars];
				if (dest != null && destPos != 0)
				{
					Buffer.BlockCopy(dest, 0, array, 0, destPos << 1);
				}
				dest = array;
			}
			while (prevInputPos != currentInputPos)
			{
				dest[destPos++] = pStr[prevInputPos++];
			}
			return dest;
		}

		private static bool IsNotReservedNotUnreservedNotHash(char c)
		{
			if (c > 'z' && c != '~')
			{
				return true;
			}
			if (c > 'Z' && c < 'a' && c != '_')
			{
				return true;
			}
			if (c < '!')
			{
				return true;
			}
			if (c == '>' || c == '<' || c == '%' || c == '"' || c == '`')
			{
				return true;
			}
			return false;
		}

		private static bool IsNotUnreserved(char c)
		{
			if (c > 'z' && c != '~')
			{
				return true;
			}
			if ((c > '9' && c < 'A') || (c > 'Z' && c < 'a' && c != '_'))
			{
				return true;
			}
			if (c < '\'' && c != '!')
			{
				return true;
			}
			if (c == '+' || c == ',' || c == '/')
			{
				return true;
			}
			return false;
		}

		private unsafe static char[] UnescapeString(string input, int start, int end, char[] dest, ref int destPosition, char rsvd1, char rsvd2, char rsvd3, UnescapeMode unescapeMode, UriParser syntax, bool isQuery, bool readOnlyConfig)
		{
			fixed (char* pStr = input)
			{
				return UnescapeString(pStr, start, end, dest, ref destPosition, rsvd1, rsvd2, rsvd3, unescapeMode, syntax, isQuery, readOnlyConfig);
			}
		}

		private unsafe static char[] UnescapeString(char* pStr, int start, int end, char[] dest, ref int destPosition, char rsvd1, char rsvd2, char rsvd3, UnescapeMode unescapeMode, UriParser syntax, bool isQuery, bool readOnlyConfig)
		{
			byte[] array = null;
			byte b = 0;
			bool flag = false;
			int i = start;
			bool flag2 = s_IriParsing && (readOnlyConfig || (!readOnlyConfig && IriParsingStatic(syntax))) && (unescapeMode & UnescapeMode.EscapeUnescape) == UnescapeMode.EscapeUnescape;
			while (true)
			{
				try
				{
					fixed (char* ptr = dest)
					{
						if ((unescapeMode & UnescapeMode.EscapeUnescape) == 0)
						{
							while (start < end)
							{
								ptr[destPosition++] = pStr[start++];
							}
							return dest;
						}
						while (true)
						{
							char c = '\0';
							for (; i < end; i++)
							{
								if ((c = pStr[i]) == '%')
								{
									if ((unescapeMode & UnescapeMode.Unescape) == 0)
									{
										flag = true;
										break;
									}
									if (i + 2 < end)
									{
										c = EscapedAscii(pStr[i + 1], pStr[i + 2]);
										if (unescapeMode < UnescapeMode.UnescapeAll)
										{
											switch (c)
											{
											case '\uffff':
												if ((unescapeMode & UnescapeMode.Escape) == 0)
												{
													continue;
												}
												flag = true;
												break;
											case '%':
												i += 2;
												continue;
											default:
												if (c == rsvd1 || c == rsvd2 || c == rsvd3)
												{
													i += 2;
													continue;
												}
												if ((unescapeMode & UnescapeMode.V1ToStringFlag) == 0 && IsNotSafeForUnescape(c))
												{
													i += 2;
													continue;
												}
												if (flag2 && ((c <= '\u009f' && IsNotSafeForUnescape(c)) || (c > '\u009f' && !CheckIriUnicodeRange(c, isQuery))))
												{
													i += 2;
													continue;
												}
												break;
											}
											break;
										}
										if (c != '\uffff')
										{
											break;
										}
										if (unescapeMode >= UnescapeMode.UnescapeAllOrThrow)
										{
											throw new UriFormatException(SR.GetString("net_uri_BadString"));
										}
									}
									else
									{
										if (unescapeMode < UnescapeMode.UnescapeAll)
										{
											flag = true;
											break;
										}
										if (unescapeMode >= UnescapeMode.UnescapeAllOrThrow)
										{
											throw new UriFormatException(SR.GetString("net_uri_BadString"));
										}
									}
								}
								else if ((unescapeMode & (UnescapeMode.Unescape | UnescapeMode.UnescapeAll)) != (UnescapeMode.Unescape | UnescapeMode.UnescapeAll) && (unescapeMode & UnescapeMode.Escape) != 0)
								{
									if (c == rsvd1 || c == rsvd2 || c == rsvd3)
									{
										flag = true;
										break;
									}
									if ((unescapeMode & UnescapeMode.V1ToStringFlag) == 0 && (c <= '\u001f' || (c >= '\u007f' && c <= '\u009f')))
									{
										flag = true;
										break;
									}
								}
							}
							while (start < i)
							{
								ptr[destPosition++] = pStr[start++];
							}
							if (i != end)
							{
								if (flag)
								{
									if (b == 0)
									{
										break;
									}
									b = (byte)(b - 1);
									EscapeAsciiChar(pStr[i], dest, ref destPosition);
									flag = false;
									start = ++i;
									continue;
								}
								if (c <= '\u007f')
								{
									dest[destPosition++] = c;
									i += 3;
									start = i;
									continue;
								}
								int byteCount = 1;
								if (array == null)
								{
									array = new byte[end - i];
								}
								array[0] = (byte)c;
								for (i += 3; i < end; i += 3)
								{
									if ((c = pStr[i]) != '%')
									{
										break;
									}
									if (i + 2 >= end)
									{
										break;
									}
									c = EscapedAscii(pStr[i + 1], pStr[i + 2]);
									if (c == '\uffff' || c < '\u0080')
									{
										break;
									}
									array[byteCount++] = (byte)c;
								}
								Encoding encoding = Encoding.GetEncoding("utf-8", new EncoderReplacementFallback(""), new DecoderReplacementFallback(""));
								char[] array2 = new char[array.Length];
								int chars = encoding.GetChars(array, 0, byteCount, array2, 0);
								if (chars != 0)
								{
									start = i;
									MatchUTF8Sequence(ptr, dest, ref destPosition, array2, chars, array, isQuery, flag2);
								}
								else
								{
									if (unescapeMode >= UnescapeMode.UnescapeAllOrThrow)
									{
										throw new UriFormatException(SR.GetString("net_uri_BadString"));
									}
									i = start + 3;
									start = i;
									dest[destPosition++] = (char)array[0];
								}
							}
							if (i == end)
							{
								return dest;
							}
						}
						b = 30;
						char[] array3 = new char[dest.Length + b * 3];
						fixed (char* ptr2 = array3)
						{
							for (int j = 0; j < destPosition; j++)
							{
								ptr2[j] = ptr[j];
							}
						}
						dest = array3;
					}
				}
				finally
				{
				}
			}
		}

		private unsafe static void MatchUTF8Sequence(char* pDest, char[] dest, ref int destOffset, char[] unescapedChars, int charCount, byte[] bytes, bool isQuery, bool iriParsing)
		{
			int num = 0;
			fixed (char* ptr = unescapedChars)
			{
				for (int i = 0; i < charCount; i++)
				{
					bool flag = char.IsHighSurrogate(ptr[i]);
					byte[] bytes2 = Encoding.UTF8.GetBytes(unescapedChars, i, (!flag) ? 1 : 2);
					int num2 = bytes2.Length;
					bool flag2 = false;
					if (iriParsing)
					{
						if (!flag)
						{
							flag2 = CheckIriUnicodeRange(unescapedChars[i], isQuery);
						}
						else
						{
							bool surrogatePair = false;
							flag2 = CheckIriUnicodeRange(unescapedChars[i], unescapedChars[i + 1], ref surrogatePair, isQuery);
						}
					}
					while (true)
					{
						if (bytes[num] != bytes2[0])
						{
							EscapeAsciiChar((char)bytes[num++], dest, ref destOffset);
							continue;
						}
						bool flag3 = true;
						int j;
						for (j = 0; j < num2; j++)
						{
							if (bytes[num + j] != bytes2[j])
							{
								flag3 = false;
								break;
							}
						}
						if (flag3)
						{
							break;
						}
						for (int k = 0; k < j; k++)
						{
							EscapeAsciiChar((char)bytes[num++], dest, ref destOffset);
						}
					}
					num += num2;
					if (iriParsing)
					{
						if (!flag2)
						{
							for (int l = 0; l < bytes2.Length; l++)
							{
								EscapeAsciiChar((char)bytes2[l], dest, ref destOffset);
							}
						}
						else if (!IsBidiControlCharacter(ptr[i]))
						{
							pDest[destOffset++] = ptr[i];
							if (flag)
							{
								pDest[destOffset++] = ptr[i + 1];
							}
						}
					}
					else
					{
						pDest[destOffset++] = ptr[i];
						if (flag)
						{
							pDest[destOffset++] = ptr[i + 1];
						}
					}
					if (flag)
					{
						i++;
					}
				}
			}
		}

		internal bool CheckIsReserved(char ch)
		{
			char[] array = new char[7]
			{
				':',
				'/',
				'?',
				'#',
				'[',
				']',
				'@'
			};
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i] == ch)
				{
					return true;
				}
			}
			return false;
		}

		internal bool CheckIsReserved(char ch, UriComponents component)
		{
			if (component != UriComponents.Scheme || component != UriComponents.UserInfo || component != UriComponents.Host || component != UriComponents.Port || component != UriComponents.Path || component != UriComponents.Query || component != UriComponents.Fragment)
			{
				if (component != 0)
				{
					return false;
				}
				return CheckIsReserved(ch);
			}
			switch (component)
			{
			case UriComponents.UserInfo:
				if (ch == '/' || ch == '?' || ch == '#' || ch == '[' || ch == ']' || ch == '@')
				{
					return true;
				}
				break;
			case UriComponents.Host:
				if (ch == ':' || ch == '/' || ch == '?' || ch == '#' || ch == '[' || ch == ']' || ch == '@')
				{
					return true;
				}
				break;
			case UriComponents.Path:
				if (ch == '/' || ch == '?' || ch == '#' || ch == '[' || ch == ']')
				{
					return true;
				}
				break;
			case UriComponents.Query:
				if (ch == '#' || ch == '[' || ch == ']')
				{
					return true;
				}
				break;
			case UriComponents.Fragment:
				if (ch == '#' || ch == '[' || ch == ']')
				{
					return true;
				}
				break;
			}
			return false;
		}

		internal unsafe string EscapeUnescapeIri(string input, int start, int end, UriComponents component)
		{
			fixed (char* pInput = input)
			{
				return EscapeUnescapeIri(pInput, start, end, component);
			}
		}

		internal unsafe string EscapeUnescapeIri(char* pInput, int start, int end, UriComponents component)
		{
			char[] array = new char[end - start];
			byte[] array2 = null;
			GCHandle gCHandle = GCHandle.Alloc(array, GCHandleType.Pinned);
			char* ptr = (char*)(void*)gCHandle.AddrOfPinnedObject();
			int num = 0;
			int i = start;
			int destOffset = 0;
			bool flag = false;
			bool flag2 = false;
			for (; i < end; i++)
			{
				flag = false;
				flag2 = false;
				char c;
				if ((c = pInput[i]) == '%')
				{
					if (i + 2 < end)
					{
						c = EscapedAscii(pInput[i + 1], pInput[i + 2]);
						if (c == '\uffff' || c == '%' || CheckIsReserved(c, component) || IsNotSafeForUnescape(c))
						{
							ptr[destOffset++] = pInput[i++];
							ptr[destOffset++] = pInput[i++];
							ptr[destOffset++] = pInput[i];
							continue;
						}
						if (c <= '\u007f')
						{
							ptr[destOffset++] = c;
							i += 2;
							continue;
						}
						int num2 = i;
						int byteCount = 1;
						if (array2 == null)
						{
							array2 = new byte[end - i];
						}
						array2[0] = (byte)c;
						for (i += 3; i < end; i += 3)
						{
							if ((c = pInput[i]) != '%')
							{
								break;
							}
							if (i + 2 >= end)
							{
								break;
							}
							c = EscapedAscii(pInput[i + 1], pInput[i + 2]);
							if (c == '\uffff' || c < '\u0080')
							{
								break;
							}
							array2[byteCount++] = (byte)c;
						}
						i--;
						Encoding encoding = Encoding.GetEncoding("utf-8", new EncoderReplacementFallback(""), new DecoderReplacementFallback(""));
						char[] array3 = new char[array2.Length];
						int chars = encoding.GetChars(array2, 0, byteCount, array3, 0);
						if (chars != 0)
						{
							MatchUTF8Sequence(ptr, array, ref destOffset, array3, chars, array2, component == UriComponents.Query, iriParsing: true);
						}
						else
						{
							for (int j = num2; j <= i; j++)
							{
								ptr[destOffset++] = pInput[j];
							}
						}
					}
					else
					{
						ptr[destOffset++] = pInput[i];
					}
				}
				else if (c > '\u007f')
				{
					if (char.IsHighSurrogate(c) && i + 1 < end)
					{
						char lowSurr = pInput[i + 1];
						flag = !CheckIriUnicodeRange(c, lowSurr, ref flag2, component == UriComponents.Query);
						if (!flag)
						{
							ptr[destOffset++] = pInput[i++];
							ptr[destOffset++] = pInput[i];
						}
					}
					else if (CheckIriUnicodeRange(c, component == UriComponents.Query))
					{
						if (!IsBidiControlCharacter(c))
						{
							ptr[destOffset++] = pInput[i];
						}
					}
					else
					{
						flag = true;
					}
				}
				else
				{
					ptr[destOffset++] = pInput[i];
				}
				if (!flag)
				{
					continue;
				}
				if (num < 12)
				{
					int num3 = 0;
					char[] array4;
					checked
					{
						num3 = array.Length + 90;
						num += 90;
						array4 = new char[num3];
					}
					fixed (char* ptr2 = array4)
					{
						for (int k = 0; k < destOffset; k++)
						{
							ptr2[k] = ptr[k];
						}
					}
					if (gCHandle.IsAllocated)
					{
						gCHandle.Free();
					}
					array = array4;
					gCHandle = GCHandle.Alloc(array, GCHandleType.Pinned);
					ptr = (char*)(void*)gCHandle.AddrOfPinnedObject();
				}
				byte[] array5 = new byte[4];
				fixed (byte* bytes = array5)
				{
					int bytes2 = Encoding.UTF8.GetBytes(pInput + i, (!flag2) ? 1 : 2, bytes, 4);
					num -= bytes2 * 3;
					for (int l = 0; l < bytes2; l++)
					{
						EscapeAsciiChar((char)array5[l], array, ref destOffset);
					}
				}
			}
			if (gCHandle.IsAllocated)
			{
				gCHandle.Free();
			}
			return new string(array, 0, destOffset);
		}

		private static bool IsNotSafeForUnescape(char ch)
		{
			if (ch <= '\u001f' || (ch >= '\u007f' && ch <= '\u009f'))
			{
				return true;
			}
			if ((ch >= ';' && ch <= '@' && (ch | 2) != 62) || (ch >= '#' && ch <= '&') || ch == '+' || ch == ',' || ch == '/' || ch == '\\')
			{
				return true;
			}
			return false;
		}

		internal unsafe bool InternalIsWellFormedOriginalString()
		{
			if (UserDrivenParsing)
			{
				throw new InvalidOperationException(SR.GetString("net_uri_UserDrivenParsing", GetType().FullName));
			}
			fixed (char* ptr = m_String)
			{
				ushort idx = 0;
				if (!IsAbsoluteUri)
				{
					return (CheckCanonical(ptr, ref idx, (ushort)m_String.Length, '\ufffe') & (Check.EscapedCanonical | Check.BackslashInPath)) == Check.EscapedCanonical;
				}
				if (IsImplicitFile)
				{
					return false;
				}
				EnsureParseRemaining();
				Flags flags = m_Flags & (Flags.E_CannotDisplayCanonical | Flags.IriCanonical);
				if ((flags & Flags.E_CannotDisplayCanonical & (Flags.E_UserNotCanonical | Flags.E_PathNotCanonical | Flags.E_QueryNotCanonical | Flags.E_FragmentNotCanonical)) != 0 && (!m_iriParsing || (m_iriParsing && ((flags & Flags.E_UserNotCanonical) == 0 || (flags & Flags.UserIriCanonical) == 0) && ((flags & Flags.E_PathNotCanonical) == 0 || (flags & Flags.PathIriCanonical) == 0) && ((flags & Flags.E_QueryNotCanonical) == 0 || (flags & Flags.QueryIriCanonical) == 0) && ((flags & Flags.E_FragmentNotCanonical) == 0 || (flags & Flags.FragmentIriCanonical) == 0))))
				{
					return false;
				}
				if (InFact(Flags.AuthorityFound))
				{
					idx = (ushort)(m_Info.Offset.Scheme + m_Syntax.SchemeName.Length + 2);
					if (idx >= m_Info.Offset.User || m_String[idx - 1] == '\\' || m_String[idx] == '\\')
					{
						return false;
					}
					if (InFact(Flags.DosPath | Flags.UncPath) && (idx = (ushort)(idx + 1)) < m_Info.Offset.User && (m_String[idx] == '/' || m_String[idx] == '\\'))
					{
						return false;
					}
				}
				if (InFact(Flags.FirstSlashAbsent) && m_Info.Offset.Query > m_Info.Offset.Path)
				{
					return false;
				}
				if (InFact(Flags.BackslashInPath))
				{
					return false;
				}
				if (IsDosPath && m_String[m_Info.Offset.Path + SecuredPathIndex - 1] == '|')
				{
					return false;
				}
				if ((m_Flags & Flags.CanonicalDnsHost) == 0)
				{
					idx = m_Info.Offset.User;
					if (!m_iriParsing || HostType != Flags.IPv6HostType)
					{
						Check check = CheckCanonical(ptr, ref idx, m_Info.Offset.Path, '/');
						if ((check & (Check.EscapedCanonical | Check.BackslashInPath | Check.ReservedFound)) != Check.EscapedCanonical && (!m_iriParsing || (m_iriParsing && (check & (Check.DisplayCanonical | Check.NotIriCanonical | Check.FoundNonAscii)) != (Check.DisplayCanonical | Check.FoundNonAscii))))
						{
							return false;
						}
					}
				}
				if ((m_Flags & (Flags.SchemeNotCanonical | Flags.AuthorityFound)) == (Flags.SchemeNotCanonical | Flags.AuthorityFound))
				{
					idx = (ushort)m_Syntax.SchemeName.Length;
					while (ptr[(int)idx++] != ':')
					{
					}
					if (idx + 1 >= m_String.Length || ptr[(int)idx] != '/' || ptr[idx + 1] != '/')
					{
						return false;
					}
				}
			}
			return true;
		}

		private Uri(Flags flags, UriParser uriParser, string uri)
		{
			m_Flags = flags;
			m_Syntax = uriParser;
			m_String = uri;
		}

		internal static Uri CreateHelper(string uriString, bool dontEscape, UriKind uriKind, ref UriFormatException e)
		{
			if (uriKind < UriKind.RelativeOrAbsolute || uriKind > UriKind.Relative)
			{
				throw new ArgumentException(SR.GetString("net_uri_InvalidUriKind", uriKind));
			}
			UriParser syntax = null;
			Flags flags = Flags.Zero;
			ParsingError parsingError = ParseScheme(uriString, ref flags, ref syntax);
			if (dontEscape)
			{
				flags |= Flags.UserEscaped;
			}
			if (parsingError != 0)
			{
				if (uriKind != UriKind.Absolute && parsingError <= ParsingError.EmptyUriString)
				{
					return new Uri(flags & Flags.UserEscaped, null, uriString);
				}
				return null;
			}
			Uri uri = new Uri(flags, syntax, uriString);
			try
			{
				uri.InitializeUri(parsingError, uriKind, out e);
				if (e == null)
				{
					return uri;
				}
				return null;
			}
			catch (UriFormatException ex)
			{
				UriFormatException ex2 = (e = ex);
				return null;
			}
		}

		internal static Uri ResolveHelper(Uri baseUri, Uri relativeUri, ref string newUriString, ref bool userEscaped, out UriFormatException e)
		{
			e = null;
			string empty = string.Empty;
			if ((object)relativeUri != null)
			{
				if (relativeUri.IsAbsoluteUri)
				{
					return relativeUri;
				}
				empty = relativeUri.OriginalString;
				userEscaped = relativeUri.UserEscaped;
			}
			else
			{
				empty = string.Empty;
			}
			if (empty.Length > 0 && (IsLWS(empty[0]) || IsLWS(empty[empty.Length - 1])))
			{
				empty = empty.Trim(_WSchars);
			}
			if (empty.Length == 0)
			{
				newUriString = baseUri.GetParts(UriComponents.AbsoluteUri, baseUri.UserEscaped ? UriFormat.UriEscaped : UriFormat.SafeUnescaped);
				return null;
			}
			if (empty[0] == '#' && !baseUri.IsImplicitFile && baseUri.Syntax.InFact(UriSyntaxFlags.MayHaveFragment))
			{
				newUriString = baseUri.GetParts(UriComponents.HttpRequestUrl | UriComponents.UserInfo, UriFormat.UriEscaped) + empty;
				return null;
			}
			if (empty.Length >= 3 && (empty[1] == ':' || empty[1] == '|') && IsAsciiLetter(empty[0]) && (empty[2] == '\\' || empty[2] == '/'))
			{
				if (baseUri.IsImplicitFile)
				{
					newUriString = empty;
					return null;
				}
				if (baseUri.Syntax.InFact(UriSyntaxFlags.AllowDOSPath))
				{
					newUriString = string.Concat(str1: (!baseUri.InFact(Flags.AuthorityFound)) ? (baseUri.Syntax.InFact(UriSyntaxFlags.PathIsRooted) ? ":/" : ":") : (baseUri.Syntax.InFact(UriSyntaxFlags.PathIsRooted) ? ":///" : "://"), str0: baseUri.Scheme, str2: empty);
					return null;
				}
			}
			ParsingError combinedString = GetCombinedString(baseUri, empty, userEscaped, ref newUriString);
			if (combinedString != 0)
			{
				e = GetException(combinedString);
				return null;
			}
			if ((object)newUriString == baseUri.m_String)
			{
				return baseUri;
			}
			return null;
		}

		private string GetRelativeSerializationString(UriFormat format)
		{
			switch (format)
			{
			case UriFormat.UriEscaped:
			{
				if (m_String.Length == 0)
				{
					return string.Empty;
				}
				int destPos = 0;
				char[] array = EscapeString(m_String, 0, m_String.Length, null, ref destPos, isUriString: true, '\uffff', '\uffff', '%');
				if (array == null)
				{
					return m_String;
				}
				return new string(array, 0, destPos);
			}
			case UriFormat.Unescaped:
				return UnescapeDataString(m_String);
			case UriFormat.SafeUnescaped:
			{
				if (m_String.Length == 0)
				{
					return string.Empty;
				}
				char[] dest = new char[m_String.Length];
				int destPosition = 0;
				dest = UnescapeString(m_String, 0, m_String.Length, dest, ref destPosition, '\uffff', '\uffff', '\uffff', UnescapeMode.EscapeUnescape, null, isQuery: false, readOnlyConfig: true);
				return new string(dest, 0, destPosition);
			}
			default:
				throw new ArgumentOutOfRangeException("format");
			}
		}

		internal string GetComponentsHelper(UriComponents uriComponents, UriFormat uriFormat)
		{
			if (uriComponents == UriComponents.Scheme)
			{
				return m_Syntax.SchemeName;
			}
			if (((uint)uriComponents & 0x80000000u) != 0)
			{
				uriComponents |= UriComponents.AbsoluteUri;
			}
			EnsureParseRemaining();
			if ((uriComponents & UriComponents.Host) != 0)
			{
				EnsureHostString(allowDnsOptimization: true);
			}
			if (uriComponents == UriComponents.Port || uriComponents == UriComponents.StrongPort)
			{
				if ((m_Flags & Flags.NotDefaultPort) != 0 || (uriComponents == UriComponents.StrongPort && m_Syntax.DefaultPort != -1))
				{
					return m_Info.Offset.PortValue.ToString(CultureInfo.InvariantCulture);
				}
				return string.Empty;
			}
			if ((uriComponents & UriComponents.StrongPort) != 0)
			{
				uriComponents |= UriComponents.Port;
			}
			if (uriComponents == UriComponents.Host && (uriFormat == UriFormat.UriEscaped || (m_Flags & (Flags.HostNotCanonical | Flags.E_HostNotCanonical)) == 0))
			{
				EnsureHostString(allowDnsOptimization: false);
				return m_Info.Host;
			}
			switch (uriFormat)
			{
			case UriFormat.UriEscaped:
				return GetEscapedParts(uriComponents);
			case UriFormat.Unescaped:
			case UriFormat.SafeUnescaped:
			case (UriFormat)32767:
				return GetUnescapedParts(uriComponents, uriFormat);
			default:
				throw new ArgumentOutOfRangeException("uriFormat");
			}
		}

		internal unsafe bool IsBaseOfHelper(Uri uriLink)
		{
			if (!IsAbsoluteUri || UserDrivenParsing)
			{
				return false;
			}
			if (!uriLink.IsAbsoluteUri)
			{
				string newUriString = null;
				bool userEscaped = false;
				uriLink = ResolveHelper(this, uriLink, ref newUriString, ref userEscaped, out var e);
				if (e != null)
				{
					return false;
				}
				if ((object)uriLink == null)
				{
					uriLink = CreateHelper(newUriString, userEscaped, UriKind.Absolute, ref e);
				}
				if (e != null)
				{
					return false;
				}
			}
			if (Syntax.SchemeName != uriLink.Syntax.SchemeName)
			{
				return false;
			}
			string parts = GetParts(UriComponents.HttpRequestUrl | UriComponents.UserInfo, UriFormat.SafeUnescaped);
			string parts2 = uriLink.GetParts(UriComponents.HttpRequestUrl | UriComponents.UserInfo, UriFormat.SafeUnescaped);
			fixed (char* pMe = parts)
			{
				fixed (char* pShe = parts2)
				{
					return TestForSubPath(pMe, (ushort)parts.Length, pShe, (ushort)parts2.Length, IsUncOrDosPath || uriLink.IsUncOrDosPath);
				}
			}
		}

		private void CreateThisFromUri(Uri otherUri)
		{
			m_Info = null;
			m_Flags = otherUri.m_Flags;
			if (InFact(Flags.MinimalUriInfoSet))
			{
				m_Flags &= ~(Flags.IndexMask | Flags.MinimalUriInfoSet | Flags.AllUriInfoSet);
				m_Flags |= (Flags)otherUri.m_Info.Offset.Path;
			}
			m_Syntax = otherUri.m_Syntax;
			m_String = otherUri.m_String;
			m_iriParsing = otherUri.m_iriParsing;
			if (otherUri.OriginalStringSwitched)
			{
				m_originalUnicodeString = otherUri.m_originalUnicodeString;
			}
			if (otherUri.AllowIdn && (otherUri.InFact(Flags.IdnHost) || otherUri.InFact(Flags.UnicodeHost)))
			{
				m_DnsSafeHost = otherUri.m_DnsSafeHost;
			}
		}
	}
}
