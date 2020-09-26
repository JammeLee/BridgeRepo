using System.Globalization;
using System.Security.Authentication.ExtendedProtection;

namespace System.Net
{
	internal class NegotiateClient : ISessionAuthenticationModule, IAuthenticationModule
	{
		internal const string AuthType = "Negotiate";

		internal static string Signature = "Negotiate".ToLower(CultureInfo.InvariantCulture);

		internal static int SignatureSize = Signature.Length;

		public bool CanPreAuthenticate => true;

		public string AuthenticationType => "Negotiate";

		public bool CanUseDefaultCredentials => true;

		public NegotiateClient()
		{
			if (!ComNetOS.IsWin2K)
			{
				throw new PlatformNotSupportedException(SR.GetString("Win2000Required"));
			}
		}

		public Authorization Authenticate(string challenge, WebRequest webRequest, ICredentials credentials)
		{
			return DoAuthenticate(challenge, webRequest, credentials, preAuthenticate: false);
		}

		private Authorization DoAuthenticate(string challenge, WebRequest webRequest, ICredentials credentials, bool preAuthenticate)
		{
			if (credentials == null)
			{
				return null;
			}
			HttpWebRequest httpWebRequest = webRequest as HttpWebRequest;
			NTAuthentication nTAuthentication = null;
			string text = null;
			if (!preAuthenticate)
			{
				int num = AuthenticationManager.FindSubstringNotInQuotes(challenge, Signature);
				if (num < 0)
				{
					return null;
				}
				int num2 = num + SignatureSize;
				if (challenge.Length > num2 && challenge[num2] != ',')
				{
					num2++;
				}
				else
				{
					num = -1;
				}
				if (num >= 0 && challenge.Length > num2)
				{
					num = challenge.IndexOf(',', num2);
					text = ((num == -1) ? challenge.Substring(num2) : challenge.Substring(num2, num - num2));
				}
				nTAuthentication = httpWebRequest.CurrentAuthenticationState.GetSecurityContext(this);
			}
			if (nTAuthentication == null)
			{
				NetworkCredential credential = credentials.GetCredential(httpWebRequest.ChallengedUri, Signature);
				string text2 = string.Empty;
				if (credential == null || (!(credential is SystemNetworkCredential) && (text2 = credential.InternalGetUserName()).Length == 0))
				{
					return null;
				}
				if (text2.Length + credential.InternalGetPassword().Length + credential.InternalGetDomain().Length > 527)
				{
					return null;
				}
				ICredentialPolicy credentialPolicy = AuthenticationManager.CredentialPolicy;
				if (credentialPolicy != null && !credentialPolicy.ShouldSendCredential(httpWebRequest.ChallengedUri, httpWebRequest, credential, this))
				{
					return null;
				}
				string computeSpn = httpWebRequest.CurrentAuthenticationState.GetComputeSpn(httpWebRequest);
				ChannelBinding channelBinding = null;
				if (httpWebRequest.CurrentAuthenticationState.TransportContext != null)
				{
					channelBinding = httpWebRequest.CurrentAuthenticationState.TransportContext.GetChannelBinding(ChannelBindingKind.Endpoint);
				}
				nTAuthentication = new NTAuthentication("Negotiate", credential, computeSpn, httpWebRequest, channelBinding);
				httpWebRequest.CurrentAuthenticationState.SetSecurityContext(nTAuthentication, this);
			}
			string outgoingBlob = nTAuthentication.GetOutgoingBlob(text);
			if (outgoingBlob == null)
			{
				return null;
			}
			bool unsafeOrProxyAuthenticatedConnectionSharing = httpWebRequest.UnsafeOrProxyAuthenticatedConnectionSharing;
			if (unsafeOrProxyAuthenticatedConnectionSharing)
			{
				httpWebRequest.LockConnection = true;
			}
			httpWebRequest.NtlmKeepAlive = text == null && nTAuthentication.IsValidContext && !nTAuthentication.IsKerberos;
			return AuthenticationManager.GetGroupAuthorization(this, "Negotiate " + outgoingBlob, nTAuthentication.IsCompleted, nTAuthentication, unsafeOrProxyAuthenticatedConnectionSharing, nTAuthentication.IsKerberos);
		}

		public Authorization PreAuthenticate(WebRequest webRequest, ICredentials credentials)
		{
			return DoAuthenticate(null, webRequest, credentials, preAuthenticate: true);
		}

		public bool Update(string challenge, WebRequest webRequest)
		{
			HttpWebRequest httpWebRequest = webRequest as HttpWebRequest;
			NTAuthentication securityContext = httpWebRequest.CurrentAuthenticationState.GetSecurityContext(this);
			if (securityContext == null)
			{
				return true;
			}
			if (!securityContext.IsCompleted && httpWebRequest.CurrentAuthenticationState.StatusCodeMatch == httpWebRequest.ResponseStatusCode)
			{
				return false;
			}
			if (!httpWebRequest.UnsafeOrProxyAuthenticatedConnectionSharing)
			{
				httpWebRequest.ServicePoint.ReleaseConnectionGroup(httpWebRequest.GetConnectionGroupLine());
			}
			int num = ((challenge == null) ? (-1) : AuthenticationManager.FindSubstringNotInQuotes(challenge, Signature));
			if (num >= 0)
			{
				int num2 = num + SignatureSize;
				string incomingBlob = null;
				if (challenge.Length > num2 && challenge[num2] != ',')
				{
					num2++;
				}
				else
				{
					num = -1;
				}
				if (num >= 0 && challenge.Length > num2)
				{
					incomingBlob = challenge.Substring(num2);
				}
				securityContext.GetOutgoingBlob(incomingBlob);
				httpWebRequest.CurrentAuthenticationState.Authorization.MutuallyAuthenticated = securityContext.IsMutualAuthFlag;
			}
			httpWebRequest.ServicePoint.SetCachedChannelBinding(httpWebRequest.ChallengedUri, securityContext.ChannelBinding);
			ClearSession(httpWebRequest);
			return true;
		}

		public void ClearSession(WebRequest webRequest)
		{
			HttpWebRequest httpWebRequest = webRequest as HttpWebRequest;
			httpWebRequest.CurrentAuthenticationState.ClearSession();
		}
	}
}
