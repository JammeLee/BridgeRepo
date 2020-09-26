using System.IO;
using System.Security.Authentication;
using System.Security.Authentication.ExtendedProtection;
using System.Security.Cryptography.X509Certificates;
using System.Security.Permissions;

namespace System.Net.Security
{
	public class SslStream : AuthenticatedStream
	{
		private SslState _SslState;

		private RemoteCertificateValidationCallback _userCertificateValidationCallback;

		private LocalCertificateSelectionCallback _userCertificateSelectionCallback;

		private object m_RemoteCertificateOrBytes;

		public TransportContext TransportContext => new SslStreamContext(this);

		public override bool IsAuthenticated => _SslState.IsAuthenticated;

		public override bool IsMutuallyAuthenticated => _SslState.IsMutuallyAuthenticated;

		public override bool IsEncrypted => IsAuthenticated;

		public override bool IsSigned => IsAuthenticated;

		public override bool IsServer => _SslState.IsServer;

		public virtual SslProtocols SslProtocol => _SslState.SslProtocol;

		public virtual bool CheckCertRevocationStatus => _SslState.CheckCertRevocationStatus;

		public virtual X509Certificate LocalCertificate => _SslState.LocalCertificate;

		public virtual X509Certificate RemoteCertificate
		{
			get
			{
				_SslState.CheckThrow(authSucessCheck: true);
				object remoteCertificateOrBytes = m_RemoteCertificateOrBytes;
				if (remoteCertificateOrBytes != null && remoteCertificateOrBytes.GetType() == typeof(byte[]))
				{
					return (X509Certificate)(m_RemoteCertificateOrBytes = new X509Certificate((byte[])remoteCertificateOrBytes));
				}
				return remoteCertificateOrBytes as X509Certificate;
			}
		}

		public virtual CipherAlgorithmType CipherAlgorithm => _SslState.CipherAlgorithm;

		public virtual int CipherStrength => _SslState.CipherStrength;

		public virtual HashAlgorithmType HashAlgorithm => _SslState.HashAlgorithm;

		public virtual int HashStrength => _SslState.HashStrength;

		public virtual ExchangeAlgorithmType KeyExchangeAlgorithm => _SslState.KeyExchangeAlgorithm;

		public virtual int KeyExchangeStrength => _SslState.KeyExchangeStrength;

		public override bool CanSeek => false;

		public override bool CanRead
		{
			get
			{
				if (_SslState.IsAuthenticated)
				{
					return base.InnerStream.CanRead;
				}
				return false;
			}
		}

		public override bool CanTimeout => base.InnerStream.CanTimeout;

		public override bool CanWrite
		{
			get
			{
				if (_SslState.IsAuthenticated)
				{
					return base.InnerStream.CanWrite;
				}
				return false;
			}
		}

		public override int ReadTimeout
		{
			get
			{
				return base.InnerStream.ReadTimeout;
			}
			set
			{
				base.InnerStream.ReadTimeout = value;
			}
		}

		public override int WriteTimeout
		{
			get
			{
				return base.InnerStream.WriteTimeout;
			}
			set
			{
				base.InnerStream.WriteTimeout = value;
			}
		}

		public override long Length => base.InnerStream.Length;

		public override long Position
		{
			get
			{
				return base.InnerStream.Position;
			}
			set
			{
				throw new NotSupportedException(SR.GetString("net_noseek"));
			}
		}

		public SslStream(Stream innerStream)
			: this(innerStream, leaveInnerStreamOpen: false, null, null)
		{
		}

		public SslStream(Stream innerStream, bool leaveInnerStreamOpen)
			: this(innerStream, leaveInnerStreamOpen, null, null)
		{
		}

		public SslStream(Stream innerStream, bool leaveInnerStreamOpen, RemoteCertificateValidationCallback userCertificateValidationCallback)
			: this(innerStream, leaveInnerStreamOpen, userCertificateValidationCallback, null)
		{
		}

		public SslStream(Stream innerStream, bool leaveInnerStreamOpen, RemoteCertificateValidationCallback userCertificateValidationCallback, LocalCertificateSelectionCallback userCertificateSelectionCallback)
			: base(innerStream, leaveInnerStreamOpen)
		{
			_userCertificateValidationCallback = userCertificateValidationCallback;
			_userCertificateSelectionCallback = userCertificateSelectionCallback;
			RemoteCertValidationCallback certValidationCallback = userCertValidationCallbackWrapper;
			LocalCertSelectionCallback certSelectionCallback = ((userCertificateSelectionCallback == null) ? null : new LocalCertSelectionCallback(userCertSelectionCallbackWrapper));
			_SslState = new SslState(innerStream, certValidationCallback, certSelectionCallback);
		}

		private bool userCertValidationCallbackWrapper(string hostName, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
		{
			m_RemoteCertificateOrBytes = certificate?.GetRawCertData();
			if (_userCertificateValidationCallback == null)
			{
				if (!_SslState.RemoteCertRequired)
				{
					sslPolicyErrors &= ~SslPolicyErrors.RemoteCertificateNotAvailable;
				}
				return sslPolicyErrors == SslPolicyErrors.None;
			}
			return _userCertificateValidationCallback(this, certificate, chain, sslPolicyErrors);
		}

		private X509Certificate userCertSelectionCallbackWrapper(string targetHost, X509CertificateCollection localCertificates, X509Certificate remoteCertificate, string[] acceptableIssuers)
		{
			return _userCertificateSelectionCallback(this, targetHost, localCertificates, remoteCertificate, acceptableIssuers);
		}

		public virtual void AuthenticateAsClient(string targetHost)
		{
			AuthenticateAsClient(targetHost, new X509CertificateCollection(), ServicePointManager.DefaultSslProtocols, checkCertificateRevocation: false);
		}

		public virtual void AuthenticateAsClient(string targetHost, X509CertificateCollection clientCertificates, SslProtocols enabledSslProtocols, bool checkCertificateRevocation)
		{
			_SslState.ValidateCreateContext(isServer: false, targetHost, enabledSslProtocols, null, clientCertificates, remoteCertRequired: true, checkCertificateRevocation);
			_SslState.ProcessAuthentication(null);
		}

		[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
		public virtual IAsyncResult BeginAuthenticateAsClient(string targetHost, AsyncCallback asyncCallback, object asyncState)
		{
			return BeginAuthenticateAsClient(targetHost, new X509CertificateCollection(), ServicePointManager.DefaultSslProtocols, checkCertificateRevocation: false, asyncCallback, asyncState);
		}

		[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
		public virtual IAsyncResult BeginAuthenticateAsClient(string targetHost, X509CertificateCollection clientCertificates, SslProtocols enabledSslProtocols, bool checkCertificateRevocation, AsyncCallback asyncCallback, object asyncState)
		{
			_SslState.ValidateCreateContext(isServer: false, targetHost, enabledSslProtocols, null, clientCertificates, remoteCertRequired: true, checkCertificateRevocation);
			LazyAsyncResult lazyAsyncResult = new LazyAsyncResult(_SslState, asyncState, asyncCallback);
			_SslState.ProcessAuthentication(lazyAsyncResult);
			return lazyAsyncResult;
		}

		public virtual void EndAuthenticateAsClient(IAsyncResult asyncResult)
		{
			_SslState.EndProcessAuthentication(asyncResult);
		}

		public virtual void AuthenticateAsServer(X509Certificate serverCertificate)
		{
			AuthenticateAsServer(serverCertificate, clientCertificateRequired: false, ServicePointManager.DefaultSslProtocols, checkCertificateRevocation: false);
		}

		public virtual void AuthenticateAsServer(X509Certificate serverCertificate, bool clientCertificateRequired, SslProtocols enabledSslProtocols, bool checkCertificateRevocation)
		{
			if (!ComNetOS.IsWin2K)
			{
				throw new PlatformNotSupportedException(SR.GetString("Win2000Required"));
			}
			_SslState.ValidateCreateContext(isServer: true, string.Empty, enabledSslProtocols, serverCertificate, null, clientCertificateRequired, checkCertificateRevocation);
			_SslState.ProcessAuthentication(null);
		}

		[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
		public virtual IAsyncResult BeginAuthenticateAsServer(X509Certificate serverCertificate, AsyncCallback asyncCallback, object asyncState)
		{
			return BeginAuthenticateAsServer(serverCertificate, clientCertificateRequired: false, ServicePointManager.DefaultSslProtocols, checkCertificateRevocation: false, asyncCallback, asyncState);
		}

		[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
		public virtual IAsyncResult BeginAuthenticateAsServer(X509Certificate serverCertificate, bool clientCertificateRequired, SslProtocols enabledSslProtocols, bool checkCertificateRevocation, AsyncCallback asyncCallback, object asyncState)
		{
			if (!ComNetOS.IsWin2K)
			{
				throw new PlatformNotSupportedException(SR.GetString("Win2000Required"));
			}
			_SslState.ValidateCreateContext(isServer: true, string.Empty, enabledSslProtocols, serverCertificate, null, clientCertificateRequired, checkCertificateRevocation);
			LazyAsyncResult lazyAsyncResult = new LazyAsyncResult(_SslState, asyncState, asyncCallback);
			_SslState.ProcessAuthentication(lazyAsyncResult);
			return lazyAsyncResult;
		}

		public virtual void EndAuthenticateAsServer(IAsyncResult asyncResult)
		{
			_SslState.EndProcessAuthentication(asyncResult);
		}

		internal ChannelBinding GetChannelBinding(ChannelBindingKind kind)
		{
			return _SslState.GetChannelBinding(kind);
		}

		public override void SetLength(long value)
		{
			base.InnerStream.SetLength(value);
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new NotSupportedException(SR.GetString("net_noseek"));
		}

		public override void Flush()
		{
			_SslState.Flush();
		}

		protected override void Dispose(bool disposing)
		{
			try
			{
				_SslState.Close();
			}
			finally
			{
				base.Dispose(disposing);
			}
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			return _SslState.SecureStream.Read(buffer, offset, count);
		}

		public void Write(byte[] buffer)
		{
			_SslState.SecureStream.Write(buffer, 0, buffer.Length);
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			_SslState.SecureStream.Write(buffer, offset, count);
		}

		[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
		public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback asyncCallback, object asyncState)
		{
			return _SslState.SecureStream.BeginRead(buffer, offset, count, asyncCallback, asyncState);
		}

		public override int EndRead(IAsyncResult asyncResult)
		{
			return _SslState.SecureStream.EndRead(asyncResult);
		}

		[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
		public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback asyncCallback, object asyncState)
		{
			return _SslState.SecureStream.BeginWrite(buffer, offset, count, asyncCallback, asyncState);
		}

		public override void EndWrite(IAsyncResult asyncResult)
		{
			_SslState.SecureStream.EndWrite(asyncResult);
		}
	}
}
