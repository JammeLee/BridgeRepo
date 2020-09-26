namespace System.Net.Mail
{
	internal class SmtpPooledStream : PooledStream
	{
		internal bool previouslyUsed;

		internal bool dsnEnabled;

		internal ICredentialsByHost creds;

		internal SmtpPooledStream(ConnectionPool connectionPool, TimeSpan lifetime, bool checkLifetime)
			: base(connectionPool, lifetime, checkLifetime)
		{
		}
	}
}
