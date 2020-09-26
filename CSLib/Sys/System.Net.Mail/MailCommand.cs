namespace System.Net.Mail
{
	internal static class MailCommand
	{
		internal static IAsyncResult BeginSend(SmtpConnection conn, byte[] command, string from, AsyncCallback callback, object state)
		{
			PrepareCommand(conn, command, from);
			return CheckCommand.BeginSend(conn, callback, state);
		}

		private static void CheckResponse(SmtpStatusCode statusCode, string response)
		{
			switch (statusCode)
			{
			case SmtpStatusCode.Ok:
				return;
			}
			if (statusCode < (SmtpStatusCode)400)
			{
				throw new SmtpException(SR.GetString("net_webstatus_ServerProtocolViolation"), response);
			}
			throw new SmtpException(statusCode, response, serverResponse: true);
		}

		internal static void EndSend(IAsyncResult result)
		{
			string response;
			SmtpStatusCode statusCode = (SmtpStatusCode)CheckCommand.EndSend(result, out response);
			CheckResponse(statusCode, response);
		}

		private static void PrepareCommand(SmtpConnection conn, byte[] command, string from)
		{
			if (conn.IsStreamOpen)
			{
				throw new InvalidOperationException(SR.GetString("SmtpDataStreamOpen"));
			}
			conn.BufferBuilder.Append(command);
			conn.BufferBuilder.Append(from);
			conn.BufferBuilder.Append(SmtpCommands.CRLF);
		}

		internal static void Send(SmtpConnection conn, byte[] command, string from)
		{
			PrepareCommand(conn, command, from);
			string response;
			SmtpStatusCode statusCode = CheckCommand.Send(conn, out response);
			CheckResponse(statusCode, response);
		}
	}
}
