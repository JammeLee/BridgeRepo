using System.Net.Mime;

namespace System.Net.Mail
{
	internal static class ReadLinesCommand
	{
		private static AsyncCallback onReadLines = OnReadLines;

		private static AsyncCallback onWrite = OnWrite;

		internal static IAsyncResult BeginSend(SmtpConnection conn, AsyncCallback callback, object state)
		{
			MultiAsyncResult multiAsyncResult = new MultiAsyncResult(conn, callback, state);
			multiAsyncResult.Enter();
			IAsyncResult asyncResult = conn.BeginFlush(onWrite, multiAsyncResult);
			if (asyncResult.CompletedSynchronously)
			{
				conn.EndFlush(asyncResult);
				multiAsyncResult.Leave();
			}
			SmtpReplyReader nextReplyReader = conn.Reader.GetNextReplyReader();
			multiAsyncResult.Enter();
			IAsyncResult asyncResult2 = nextReplyReader.BeginReadLines(onReadLines, multiAsyncResult);
			if (asyncResult2.CompletedSynchronously)
			{
				LineInfo[] result = conn.Reader.CurrentReader.EndReadLines(asyncResult2);
				if (!(multiAsyncResult.Result is Exception))
				{
					multiAsyncResult.Result = result;
				}
				multiAsyncResult.Leave();
			}
			multiAsyncResult.CompleteSequence();
			return multiAsyncResult;
		}

		internal static LineInfo[] EndSend(IAsyncResult result)
		{
			object obj = MultiAsyncResult.End(result);
			if (obj is Exception)
			{
				throw (Exception)obj;
			}
			return (LineInfo[])obj;
		}

		private static void OnReadLines(IAsyncResult result)
		{
			if (result.CompletedSynchronously)
			{
				return;
			}
			MultiAsyncResult multiAsyncResult = (MultiAsyncResult)result.AsyncState;
			try
			{
				SmtpConnection smtpConnection = (SmtpConnection)multiAsyncResult.Context;
				LineInfo[] result2 = smtpConnection.Reader.CurrentReader.EndReadLines(result);
				if (!(multiAsyncResult.Result is Exception))
				{
					multiAsyncResult.Result = result2;
				}
				multiAsyncResult.Leave();
			}
			catch (Exception result3)
			{
				multiAsyncResult.Leave(result3);
			}
			catch
			{
				multiAsyncResult.Leave(new Exception(SR.GetString("net_nonClsCompliantException")));
			}
		}

		private static void OnWrite(IAsyncResult result)
		{
			if (!result.CompletedSynchronously)
			{
				MultiAsyncResult multiAsyncResult = (MultiAsyncResult)result.AsyncState;
				try
				{
					SmtpConnection smtpConnection = (SmtpConnection)multiAsyncResult.Context;
					smtpConnection.EndFlush(result);
					multiAsyncResult.Leave();
				}
				catch (Exception result2)
				{
					multiAsyncResult.Leave(result2);
				}
				catch
				{
					multiAsyncResult.Leave(new Exception(SR.GetString("net_nonClsCompliantException")));
				}
			}
		}

		internal static LineInfo[] Send(SmtpConnection conn)
		{
			conn.Flush();
			return conn.Reader.GetNextReplyReader().ReadLines();
		}
	}
}
