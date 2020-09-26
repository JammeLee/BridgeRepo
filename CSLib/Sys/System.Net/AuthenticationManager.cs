using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Net.Configuration;
using System.Reflection;
using System.Security.Authentication.ExtendedProtection;
using System.Threading;

namespace System.Net
{
	public class AuthenticationManager
	{
		private static PrefixLookup s_ModuleBinding = new PrefixLookup();

		private static ArrayList s_ModuleList;

		private static ICredentialPolicy s_ICredentialPolicy;

		private static SpnDictionary m_SpnDictionary = new SpnDictionary();

		private static TriState s_OSSupportsExtendedProtection = TriState.Unspecified;

		private static TriState s_SspSupportsExtendedProtection = TriState.Unspecified;

		public static ICredentialPolicy CredentialPolicy
		{
			get
			{
				return s_ICredentialPolicy;
			}
			set
			{
				ExceptionHelper.ControlPolicyPermission.Demand();
				s_ICredentialPolicy = value;
			}
		}

		public static StringDictionary CustomTargetNameDictionary => m_SpnDictionary;

		internal static SpnDictionary SpnDictionary => m_SpnDictionary;

		internal static bool OSSupportsExtendedProtection
		{
			get
			{
				if (s_OSSupportsExtendedProtection == TriState.Unspecified)
				{
					if (ComNetOS.IsWin7)
					{
						s_OSSupportsExtendedProtection = TriState.True;
					}
					else if (SspSupportsExtendedProtection)
					{
						if (UnsafeNclNativeMethods.HttpApi.ExtendedProtectionSupported)
						{
							s_OSSupportsExtendedProtection = TriState.True;
						}
						else
						{
							s_OSSupportsExtendedProtection = TriState.False;
						}
					}
					else
					{
						s_OSSupportsExtendedProtection = TriState.False;
					}
				}
				return s_OSSupportsExtendedProtection == TriState.True;
			}
		}

		internal static bool SspSupportsExtendedProtection
		{
			get
			{
				if (s_SspSupportsExtendedProtection == TriState.Unspecified)
				{
					if (ComNetOS.IsWin7)
					{
						s_SspSupportsExtendedProtection = TriState.True;
					}
					else
					{
						ContextFlags requestedContextFlags = ContextFlags.ReplayDetect | ContextFlags.SequenceDetect | ContextFlags.Confidentiality | ContextFlags.Connection | ContextFlags.AcceptIntegrity;
						NTAuthentication nTAuthentication = new NTAuthentication(isServer: false, "NTLM", SystemNetworkCredential.defaultCredential, "http/localhost", requestedContextFlags, null);
						try
						{
							NTAuthentication nTAuthentication2 = new NTAuthentication(isServer: true, "NTLM", SystemNetworkCredential.defaultCredential, null, ContextFlags.Connection, null);
							try
							{
								byte[] incomingBlob = null;
								while (!nTAuthentication2.IsCompleted)
								{
									incomingBlob = nTAuthentication.GetOutgoingBlob(incomingBlob, throwOnError: true, out var statusCode);
									incomingBlob = nTAuthentication2.GetOutgoingBlob(incomingBlob, throwOnError: true, out statusCode);
								}
								if (nTAuthentication2.OSSupportsExtendedProtection)
								{
									s_SspSupportsExtendedProtection = TriState.True;
								}
								else
								{
									if (Logging.On)
									{
										Logging.PrintWarning(Logging.Web, SR.GetString("net_ssp_dont_support_cbt"));
									}
									s_SspSupportsExtendedProtection = TriState.False;
								}
							}
							finally
							{
								nTAuthentication2.CloseContext();
							}
						}
						finally
						{
							nTAuthentication.CloseContext();
						}
					}
				}
				return s_SspSupportsExtendedProtection == TriState.True;
			}
		}

		private static ArrayList ModuleList
		{
			get
			{
				if (s_ModuleList == null)
				{
					lock (s_ModuleBinding)
					{
						if (s_ModuleList == null)
						{
							List<Type> authenticationModules = AuthenticationModulesSectionInternal.GetSection().AuthenticationModules;
							ArrayList arrayList = new ArrayList();
							foreach (Type item in authenticationModules)
							{
								try
								{
									IAuthenticationModule authenticationModule = Activator.CreateInstance(item, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.CreateInstance, null, new object[0], CultureInfo.InvariantCulture) as IAuthenticationModule;
									if (authenticationModule != null)
									{
										RemoveAuthenticationType(arrayList, authenticationModule.AuthenticationType);
										arrayList.Add(authenticationModule);
									}
								}
								catch (Exception)
								{
								}
								catch
								{
								}
							}
							s_ModuleList = arrayList;
						}
					}
				}
				return s_ModuleList;
			}
		}

		public static IEnumerator RegisteredModules => ModuleList.GetEnumerator();

		private AuthenticationManager()
		{
		}

		internal static void EnsureConfigLoaded()
		{
			try
			{
				_ = ModuleList;
			}
			catch (Exception ex)
			{
				if (ex is ThreadAbortException || ex is OutOfMemoryException || ex is StackOverflowException)
				{
					throw;
				}
			}
			catch
			{
			}
		}

		private static void RemoveAuthenticationType(ArrayList list, string typeToRemove)
		{
			for (int i = 0; i < list.Count; i++)
			{
				if (string.Compare(((IAuthenticationModule)list[i]).AuthenticationType, typeToRemove, StringComparison.OrdinalIgnoreCase) == 0)
				{
					list.RemoveAt(i);
					break;
				}
			}
		}

		public static Authorization Authenticate(string challenge, WebRequest request, ICredentials credentials)
		{
			if (request == null)
			{
				throw new ArgumentNullException("request");
			}
			if (credentials == null)
			{
				throw new ArgumentNullException("credentials");
			}
			if (challenge == null)
			{
				throw new ArgumentNullException("challenge");
			}
			Authorization authorization = null;
			HttpWebRequest httpWebRequest = request as HttpWebRequest;
			if (httpWebRequest != null && httpWebRequest.CurrentAuthenticationState.Module != null)
			{
				return httpWebRequest.CurrentAuthenticationState.Module.Authenticate(challenge, request, credentials);
			}
			lock (s_ModuleBinding)
			{
				int num = 0;
				while (true)
				{
					if (num < ModuleList.Count)
					{
						IAuthenticationModule authenticationModule = (IAuthenticationModule)ModuleList[num];
						if (httpWebRequest != null)
						{
							httpWebRequest.CurrentAuthenticationState.Module = authenticationModule;
						}
						authorization = authenticationModule.Authenticate(challenge, request, credentials);
						if (authorization == null)
						{
							num++;
							continue;
						}
						break;
					}
					return authorization;
				}
				return authorization;
			}
		}

		private static bool ModuleRequiresChannelBinding(IAuthenticationModule authenticationModule)
		{
			if (!(authenticationModule is NtlmClient) && !(authenticationModule is KerberosClient) && !(authenticationModule is NegotiateClient))
			{
				return authenticationModule is DigestClient;
			}
			return true;
		}

		public static Authorization PreAuthenticate(WebRequest request, ICredentials credentials)
		{
			if (request == null)
			{
				throw new ArgumentNullException("request");
			}
			if (credentials == null)
			{
				return null;
			}
			HttpWebRequest httpWebRequest = request as HttpWebRequest;
			if (httpWebRequest == null)
			{
				return null;
			}
			string text = s_ModuleBinding.Lookup(httpWebRequest.ChallengedUri.AbsoluteUri) as string;
			if (text == null)
			{
				return null;
			}
			IAuthenticationModule authenticationModule = findModule(text);
			if (authenticationModule == null)
			{
				return null;
			}
			if (httpWebRequest.ChallengedUri.Scheme == Uri.UriSchemeHttps)
			{
				object cachedChannelBinding = httpWebRequest.ServicePoint.CachedChannelBinding;
				ChannelBinding channelBinding = cachedChannelBinding as ChannelBinding;
				if (channelBinding != null)
				{
					httpWebRequest.CurrentAuthenticationState.TransportContext = new CachedTransportContext(channelBinding);
				}
			}
			Authorization authorization = authenticationModule.PreAuthenticate(request, credentials);
			if (authorization != null && !authorization.Complete && httpWebRequest != null)
			{
				httpWebRequest.CurrentAuthenticationState.Module = authenticationModule;
			}
			return authorization;
		}

		public static void Register(IAuthenticationModule authenticationModule)
		{
			ExceptionHelper.UnmanagedPermission.Demand();
			if (authenticationModule == null)
			{
				throw new ArgumentNullException("authenticationModule");
			}
			lock (s_ModuleBinding)
			{
				IAuthenticationModule authenticationModule2 = findModule(authenticationModule.AuthenticationType);
				if (authenticationModule2 != null)
				{
					ModuleList.Remove(authenticationModule2);
				}
				ModuleList.Add(authenticationModule);
			}
		}

		public static void Unregister(IAuthenticationModule authenticationModule)
		{
			ExceptionHelper.UnmanagedPermission.Demand();
			if (authenticationModule == null)
			{
				throw new ArgumentNullException("authenticationModule");
			}
			lock (s_ModuleBinding)
			{
				if (!ModuleList.Contains(authenticationModule))
				{
					throw new InvalidOperationException(SR.GetString("net_authmodulenotregistered"));
				}
				ModuleList.Remove(authenticationModule);
			}
		}

		public static void Unregister(string authenticationScheme)
		{
			ExceptionHelper.UnmanagedPermission.Demand();
			if (authenticationScheme == null)
			{
				throw new ArgumentNullException("authenticationScheme");
			}
			lock (s_ModuleBinding)
			{
				IAuthenticationModule authenticationModule = findModule(authenticationScheme);
				if (authenticationModule == null)
				{
					throw new InvalidOperationException(SR.GetString("net_authschemenotregistered"));
				}
				ModuleList.Remove(authenticationModule);
			}
		}

		internal static void BindModule(Uri uri, Authorization response, IAuthenticationModule module)
		{
			if (response.ProtectionRealm != null)
			{
				string[] protectionRealm = response.ProtectionRealm;
				for (int i = 0; i < protectionRealm.Length; i++)
				{
					s_ModuleBinding.Add(protectionRealm[i], module.AuthenticationType);
				}
			}
			else
			{
				string prefix = generalize(uri);
				s_ModuleBinding.Add(prefix, module.AuthenticationType);
			}
		}

		private static IAuthenticationModule findModule(string authenticationType)
		{
			IAuthenticationModule result = null;
			ArrayList moduleList = ModuleList;
			for (int i = 0; i < moduleList.Count; i++)
			{
				IAuthenticationModule authenticationModule = (IAuthenticationModule)moduleList[i];
				if (string.Compare(authenticationModule.AuthenticationType, authenticationType, StringComparison.OrdinalIgnoreCase) == 0)
				{
					result = authenticationModule;
					break;
				}
			}
			return result;
		}

		private static string generalize(Uri location)
		{
			string absoluteUri = location.AbsoluteUri;
			int num = absoluteUri.LastIndexOf('/');
			if (num < 0)
			{
				return absoluteUri;
			}
			return absoluteUri.Substring(0, num + 1);
		}

		internal static int FindSubstringNotInQuotes(string challenge, string signature)
		{
			int num = -1;
			if (challenge != null && signature != null && challenge.Length >= signature.Length)
			{
				int num2 = -1;
				int num3 = -1;
				for (int i = 0; i < challenge.Length; i++)
				{
					if (challenge[i] == '"')
					{
						if (num2 <= num3)
						{
							num2 = i;
						}
						else
						{
							num3 = i;
						}
					}
					if (i != challenge.Length - 1 && (challenge[i] != '"' || num2 <= num3))
					{
						continue;
					}
					if (i == challenge.Length - 1)
					{
						num2 = challenge.Length;
					}
					if (num2 < num3 + 3)
					{
						continue;
					}
					num = IndexOf(challenge, signature, num3 + 1, num2 - num3 - 1);
					if (num >= 0)
					{
						if ((num == 0 || challenge[num - 1] == ' ' || challenge[num - 1] == ',') && (num + signature.Length == challenge.Length || challenge[num + signature.Length] == ' ' || challenge[num + signature.Length] == ','))
						{
							break;
						}
						num = -1;
					}
				}
			}
			return num;
		}

		private static int IndexOf(string challenge, string lwrCaseSignature, int start, int count)
		{
			count += start + 1 - lwrCaseSignature.Length;
			while (start < count)
			{
				int i;
				for (i = 0; i < lwrCaseSignature.Length && (challenge[start + i] | 0x20) == lwrCaseSignature[i]; i++)
				{
				}
				if (i == lwrCaseSignature.Length)
				{
					return start;
				}
				start++;
			}
			return -1;
		}

		internal static int SplitNoQuotes(string challenge, ref int offset)
		{
			int num = offset;
			offset = -1;
			if (challenge != null && num < challenge.Length)
			{
				int num2 = -1;
				int num3 = -1;
				for (int i = num; i < challenge.Length; i++)
				{
					if (num2 > num3 && challenge[i] == '\\' && i + 1 < challenge.Length && challenge[i + 1] == '"')
					{
						i++;
					}
					else if (challenge[i] == '"')
					{
						if (num2 <= num3)
						{
							num2 = i;
						}
						else
						{
							num3 = i;
						}
					}
					else if (challenge[i] == '=' && num2 <= num3 && offset < 0)
					{
						offset = i;
					}
					else if (challenge[i] == ',' && num2 <= num3)
					{
						return i;
					}
				}
			}
			return -1;
		}

		internal static Authorization GetGroupAuthorization(IAuthenticationModule thisModule, string token, bool finished, NTAuthentication authSession, bool shareAuthenticatedConnections, bool mutualAuth)
		{
			return new Authorization(token, finished, shareAuthenticatedConnections ? null : (thisModule.GetType().FullName + "/" + authSession.UniqueUserId), mutualAuth);
		}
	}
}
