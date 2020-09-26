using System.Collections;
using System.IO;
using System.Net.Mime;

namespace System.Net.Mail
{
	internal class SendMailAsyncResult : LazyAsyncResult
	{
		private SmtpConnection connection;

		private string from;

		private string deliveryNotify;

		private static AsyncCallback sendMailFromCompleted = SendMailFromCompleted;

		private static AsyncCallback sendToCompleted = SendToCompleted;

		private static AsyncCallback sendToCollectionCompleted = SendToCollectionCompleted;

		private static AsyncCallback sendDataCompleted = SendDataCompleted;

		private ArrayList failedRecipientExceptions = new ArrayList();

		private Stream stream;

		private string to;

		private MailAddressCollection toCollection;

		private int toIndex;

		internal SendMailAsyncResult(SmtpConnection connection, string from, MailAddressCollection toCollection, string deliveryNotify, AsyncCallback callback, object state)
			: base(null, state, callback)
		{
			this.toCollection = toCollection;
			this.connection = connection;
			this.from = from;
			this.deliveryNotify = deliveryNotify;
		}

		internal void Send()
		{
			SendMailFrom();
		}

		internal static MailWriter End(IAsyncResult result)
		{
			SendMailAsyncResult sendMailAsyncResult = (SendMailAsyncResult)result;
			object obj = sendMailAsyncResult.InternalWaitForCompletion();
			if (obj is Exception)
			{
				throw (Exception)obj;
			}
			return new MailWriter(sendMailAsyncResult.stream);
		}

		private void SendMailFrom()
		{
			IAsyncResult asyncResult = MailCommand.BeginSend(connection, SmtpCommands.Mail, from, sendMailFromCompleted, this);
			if (asyncResult.CompletedSynchronously)
			{
				MailCommand.EndSend(asyncResult);
				SendTo();
			}
		}

		private static void SendMailFromCompleted(IAsyncResult result)
		{
			if (!result.CompletedSynchronously)
			{
				SendMailAsyncResult sendMailAsyncResult = (SendMailAsyncResult)result.AsyncState;
				try
				{
					MailCommand.EndSend(result);
					sendMailAsyncResult.SendTo();
				}
				catch (Exception result2)
				{
					sendMailAsyncResult.InvokeCallback(result2);
				}
				catch
				{
					sendMailAsyncResult.InvokeCallback(new Exception(SR.GetString("net_nonClsCompliantException")));
				}
			}
		}

		private void SendTo()
		{
			if (to != null)
			{
				IAsyncResult asyncResult = RecipientCommand.BeginSend(connection, (deliveryNotify != null) ? (to + deliveryNotify) : to, sendToCompleted, this);
				if (asyncResult.CompletedSynchronously)
				{
					if (!RecipientCommand.EndSend(asyncResult, out var response))
					{
						throw new SmtpFailedRecipientException(connection.Reader.StatusCode, to, response);
					}
					SendData();
				}
			}
			else if (SendToCollection())
			{
				SendData();
			}
		}

		private static void SendToCompleted(IAsyncResult result)
		{
			if (result.CompletedSynchronously)
			{
				return;
			}
			SendMailAsyncResult sendMailAsyncResult = (SendMailAsyncResult)result.AsyncState;
			try
			{
				if (RecipientCommand.EndSend(result, out var response))
				{
					sendMailAsyncResult.SendData();
				}
				else
				{
					sendMailAsyncResult.InvokeCallback(new SmtpFailedRecipientException(sendMailAsyncResult.connection.Reader.StatusCode, sendMailAsyncResult.to, response));
				}
			}
			catch (Exception result2)
			{
				sendMailAsyncResult.InvokeCallback(result2);
			}
			catch
			{
				sendMailAsyncResult.InvokeCallback(new Exception(SR.GetString("net_nonClsCompliantException")));
			}
		}

		private bool SendToCollection()
		{
			while (toIndex < toCollection.Count)
			{
				MultiAsyncResult multiAsyncResult = (MultiAsyncResult)RecipientCommand.BeginSend(connection, toCollection[toIndex++].SmtpAddress + deliveryNotify, sendToCollectionCompleted, this);
				if (!multiAsyncResult.CompletedSynchronously)
				{
					return false;
				}
				if (!RecipientCommand.EndSend(multiAsyncResult, out var response))
				{
					failedRecipientExceptions.Add(new SmtpFailedRecipientException(connection.Reader.StatusCode, toCollection[toIndex - 1].SmtpAddress, response));
				}
			}
			return true;
		}

		private static void SendToCollectionCompleted(IAsyncResult result)
		{
			if (result.CompletedSynchronously)
			{
				return;
			}
			SendMailAsyncResult sendMailAsyncResult = (SendMailAsyncResult)result.AsyncState;
			try
			{
				if (RecipientCommand.EndSend(result, out var response))
				{
					goto IL_00b7;
				}
				sendMailAsyncResult.failedRecipientExceptions.Add(new SmtpFailedRecipientException(sendMailAsyncResult.connection.Reader.StatusCode, sendMailAsyncResult.toCollection[sendMailAsyncResult.toIndex - 1].SmtpAddress, response));
				if (sendMailAsyncResult.failedRecipientExceptions.Count != sendMailAsyncResult.toCollection.Count)
				{
					goto IL_00b7;
				}
				SmtpFailedRecipientException ex = null;
				ex = ((sendMailAsyncResult.toCollection.Count != 1) ? new SmtpFailedRecipientsException(sendMailAsyncResult.failedRecipientExceptions, allFailed: true) : ((SmtpFailedRecipientException)sendMailAsyncResult.failedRecipientExceptions[0]));
				ex.fatal = true;
				sendMailAsyncResult.InvokeCallback(ex);
				goto end_IL_0017;
				IL_00b7:
				if (sendMailAsyncResult.SendToCollection())
				{
					sendMailAsyncResult.SendData();
				}
				end_IL_0017:;
			}
			catch (Exception result2)
			{
				sendMailAsyncResult.InvokeCallback(result2);
			}
			catch
			{
				sendMailAsyncResult.InvokeCallback(new Exception(SR.GetString("net_nonClsCompliantException")));
			}
		}

		private void SendData()
		{
			IAsyncResult asyncResult = DataCommand.BeginSend(connection, sendDataCompleted, this);
			if (asyncResult.CompletedSynchronously)
			{
				DataCommand.EndSend(asyncResult);
				stream = connection.GetClosableStream();
				if (failedRecipientExceptions.Count > 1)
				{
					InvokeCallback(new SmtpFailedRecipientsException(failedRecipientExceptions, failedRecipientExceptions.Count == toCollection.Count));
				}
				else if (failedRecipientExceptions.Count == 1)
				{
					InvokeCallback(failedRecipientExceptions[0]);
				}
				else
				{
					InvokeCallback();
				}
			}
		}

		private static void SendDataCompleted(IAsyncResult result)
		{
			if (result.CompletedSynchronously)
			{
				return;
			}
			SendMailAsyncResult sendMailAsyncResult = (SendMailAsyncResult)result.AsyncState;
			try
			{
				DataCommand.EndSend(result);
				sendMailAsyncResult.stream = sendMailAsyncResult.connection.GetClosableStream();
				if (sendMailAsyncResult.failedRecipientExceptions.Count > 1)
				{
					sendMailAsyncResult.InvokeCallback(new SmtpFailedRecipientsException(sendMailAsyncResult.failedRecipientExceptions, sendMailAsyncResult.failedRecipientExceptions.Count == sendMailAsyncResult.toCollection.Count));
				}
				else if (sendMailAsyncResult.failedRecipientExceptions.Count == 1)
				{
					sendMailAsyncResult.InvokeCallback(sendMailAsyncResult.failedRecipientExceptions[0]);
				}
				else
				{
					sendMailAsyncResult.InvokeCallback();
				}
			}
			catch (Exception result2)
			{
				sendMailAsyncResult.InvokeCallback(result2);
			}
			catch
			{
				sendMailAsyncResult.InvokeCallback(new Exception(SR.GetString("net_nonClsCompliantException")));
			}
		}
	}
}
