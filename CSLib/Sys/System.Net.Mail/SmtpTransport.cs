using System.Collections;
using System.Security.Cryptography.X509Certificates;

namespace System.Net.Mail
{
	internal class SmtpTransport
	{
		internal const int DefaultPort = 25;

		private ISmtpAuthenticationModule[] authenticationModules;

		private SmtpConnection connection;

		private SmtpClient client;

		private ICredentialsByHost credentials;

		private int timeout = 100000;

		private ArrayList failedRecipientExceptions = new ArrayList();

		private bool m_IdentityRequired;

		private bool enableSsl;

		private X509CertificateCollection clientCertificates;

		internal ICredentialsByHost Credentials
		{
			get
			{
				return credentials;
			}
			set
			{
				credentials = value;
			}
		}

		internal bool IdentityRequired
		{
			get
			{
				return m_IdentityRequired;
			}
			set
			{
				m_IdentityRequired = value;
			}
		}

		internal bool IsConnected
		{
			get
			{
				if (connection != null)
				{
					return connection.IsConnected;
				}
				return false;
			}
		}

		internal int Timeout
		{
			get
			{
				return timeout;
			}
			set
			{
				if (value < 0)
				{
					throw new ArgumentOutOfRangeException("value");
				}
				timeout = value;
			}
		}

		internal bool EnableSsl
		{
			get
			{
				return enableSsl;
			}
			set
			{
				enableSsl = value;
			}
		}

		internal X509CertificateCollection ClientCertificates
		{
			get
			{
				if (clientCertificates == null)
				{
					clientCertificates = new X509CertificateCollection();
				}
				return clientCertificates;
			}
		}

		internal SmtpTransport(SmtpClient client)
			: this(client, SmtpAuthenticationManager.GetModules())
		{
		}

		internal SmtpTransport(SmtpClient client, ISmtpAuthenticationModule[] authenticationModules)
		{
			this.client = client;
			if (authenticationModules == null)
			{
				throw new ArgumentNullException("authenticationModules");
			}
			this.authenticationModules = authenticationModules;
		}

		internal void GetConnection(string host, int port)
		{
			if (host == null)
			{
				throw new ArgumentNullException("host");
			}
			if (port < 0 || port > 65535)
			{
				throw new ArgumentOutOfRangeException("port");
			}
			connection = new SmtpConnection(this, client, credentials, authenticationModules);
			connection.Timeout = timeout;
			if (Logging.On)
			{
				Logging.Associate(Logging.Web, this, connection);
			}
			if (EnableSsl)
			{
				connection.EnableSsl = true;
				connection.ClientCertificates = ClientCertificates;
			}
			connection.GetConnection(host, port);
		}

		internal IAsyncResult BeginGetConnection(string host, int port, ContextAwareResult outerResult, AsyncCallback callback, object state)
		{
			if (host == null)
			{
				throw new ArgumentNullException("host");
			}
			if (port < 0 || port > 65535)
			{
				throw new ArgumentOutOfRangeException("port");
			}
			IAsyncResult asyncResult = null;
			try
			{
				connection = new SmtpConnection(this, client, credentials, authenticationModules);
				connection.Timeout = timeout;
				if (Logging.On)
				{
					Logging.Associate(Logging.Web, this, connection);
				}
				if (EnableSsl)
				{
					connection.EnableSsl = true;
					connection.ClientCertificates = ClientCertificates;
				}
				return connection.BeginGetConnection(host, port, outerResult, callback, state);
			}
			catch (Exception innerException)
			{
				throw new SmtpException(SR.GetString("MailHostNotFound"), innerException);
			}
			catch
			{
				throw new SmtpException(SR.GetString("MailHostNotFound"), new Exception(SR.GetString("net_nonClsCompliantException")));
			}
		}

		internal void EndGetConnection(IAsyncResult result)
		{
			connection.EndGetConnection(result);
		}

		internal IAsyncResult BeginSendMail(MailAddress sender, MailAddressCollection recipients, string deliveryNotify, AsyncCallback callback, object state)
		{
			if (sender == null)
			{
				throw new ArgumentNullException("sender");
			}
			if (recipients == null)
			{
				throw new ArgumentNullException("recipients");
			}
			SendMailAsyncResult sendMailAsyncResult = new SendMailAsyncResult(connection, sender.SmtpAddress, recipients, connection.DSNEnabled ? deliveryNotify : null, callback, state);
			sendMailAsyncResult.Send();
			return sendMailAsyncResult;
		}

		internal void ReleaseConnection()
		{
			if (connection != null)
			{
				connection.ReleaseConnection();
			}
		}

		internal void Abort()
		{
			if (connection != null)
			{
				connection.Abort();
			}
		}

		internal MailWriter EndSendMail(IAsyncResult result)
		{
			return SendMailAsyncResult.End(result);
		}

		internal MailWriter SendMail(MailAddress sender, MailAddressCollection recipients, string deliveryNotify, out SmtpFailedRecipientException exception)
		{
			if (sender == null)
			{
				throw new ArgumentNullException("sender");
			}
			if (recipients == null)
			{
				throw new ArgumentNullException("recipients");
			}
			MailCommand.Send(connection, SmtpCommands.Mail, sender.SmtpAddress);
			failedRecipientExceptions.Clear();
			exception = null;
			foreach (MailAddress recipient in recipients)
			{
				if (!RecipientCommand.Send(connection, connection.DSNEnabled ? (recipient.SmtpAddress + deliveryNotify) : recipient.SmtpAddress, out var response))
				{
					failedRecipientExceptions.Add(new SmtpFailedRecipientException(connection.Reader.StatusCode, recipient.SmtpAddress, response));
				}
			}
			if (failedRecipientExceptions.Count > 0)
			{
				if (failedRecipientExceptions.Count == 1)
				{
					exception = (SmtpFailedRecipientException)failedRecipientExceptions[0];
				}
				else
				{
					exception = new SmtpFailedRecipientsException(failedRecipientExceptions, failedRecipientExceptions.Count == recipients.Count);
				}
				if (failedRecipientExceptions.Count == recipients.Count)
				{
					exception.fatal = true;
					throw exception;
				}
			}
			DataCommand.Send(connection);
			return new MailWriter(connection.GetClosableStream());
		}
	}
}
