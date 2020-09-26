using System.Collections.Specialized;
using System.Net.Mime;
using System.Text;

namespace System.Net.Mail
{
	internal class Message
	{
		internal class EmptySendContext
		{
			internal LazyAsyncResult result;

			internal BaseWriter writer;

			internal EmptySendContext(BaseWriter writer, LazyAsyncResult result)
			{
				this.writer = writer;
				this.result = result;
			}
		}

		private MailAddress from;

		private MailAddress sender;

		private MailAddress replyTo;

		private MailAddressCollection to;

		private MailAddressCollection cc;

		private MailAddressCollection bcc;

		private MimeBasePart content;

		private HeaderCollection headers;

		private HeaderCollection envelopeHeaders;

		private string subject;

		private Encoding subjectEncoding;

		private MailPriority priority = (MailPriority)(-1);

		public MailPriority Priority
		{
			get
			{
				if (priority != (MailPriority)(-1))
				{
					return priority;
				}
				return MailPriority.Normal;
			}
			set
			{
				priority = value;
			}
		}

		internal MailAddress From
		{
			get
			{
				return from;
			}
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException("value");
				}
				from = value;
			}
		}

		internal MailAddress Sender
		{
			get
			{
				return sender;
			}
			set
			{
				sender = value;
			}
		}

		internal MailAddress ReplyTo
		{
			get
			{
				return replyTo;
			}
			set
			{
				replyTo = value;
			}
		}

		internal MailAddressCollection To
		{
			get
			{
				if (to == null)
				{
					to = new MailAddressCollection();
				}
				return to;
			}
		}

		internal MailAddressCollection Bcc
		{
			get
			{
				if (bcc == null)
				{
					bcc = new MailAddressCollection();
				}
				return bcc;
			}
		}

		internal MailAddressCollection CC
		{
			get
			{
				if (cc == null)
				{
					cc = new MailAddressCollection();
				}
				return cc;
			}
		}

		internal string Subject
		{
			get
			{
				return subject;
			}
			set
			{
				if (value != null && MailBnfHelper.HasCROrLF(value))
				{
					throw new ArgumentException(SR.GetString("MailSubjectInvalidFormat"));
				}
				subject = value;
				if (subject != null && subjectEncoding == null && !MimeBasePart.IsAscii(subject, permitCROrLF: false))
				{
					subjectEncoding = Encoding.GetEncoding("utf-8");
				}
			}
		}

		internal Encoding SubjectEncoding
		{
			get
			{
				return subjectEncoding;
			}
			set
			{
				subjectEncoding = value;
			}
		}

		internal NameValueCollection Headers
		{
			get
			{
				if (headers == null)
				{
					headers = new HeaderCollection();
					if (Logging.On)
					{
						Logging.Associate(Logging.Web, this, headers);
					}
				}
				return headers;
			}
		}

		internal NameValueCollection EnvelopeHeaders
		{
			get
			{
				if (envelopeHeaders == null)
				{
					envelopeHeaders = new HeaderCollection();
					if (Logging.On)
					{
						Logging.Associate(Logging.Web, this, envelopeHeaders);
					}
				}
				return envelopeHeaders;
			}
		}

		internal virtual MimeBasePart Content
		{
			get
			{
				return content;
			}
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException("value");
				}
				content = value;
			}
		}

		internal Message()
		{
		}

		internal Message(string from, string to)
			: this()
		{
			if (from == null)
			{
				throw new ArgumentNullException("from");
			}
			if (to == null)
			{
				throw new ArgumentNullException("to");
			}
			if (from == string.Empty)
			{
				throw new ArgumentException(SR.GetString("net_emptystringcall", "from"), "from");
			}
			if (to == string.Empty)
			{
				throw new ArgumentException(SR.GetString("net_emptystringcall", "to"), "to");
			}
			this.from = new MailAddress(from);
			this.to = new MailAddressCollection
			{
				to
			};
		}

		internal Message(MailAddress from, MailAddress to)
			: this()
		{
			this.from = from;
			To.Add(to);
		}

		internal void EmptySendCallback(IAsyncResult result)
		{
			Exception result2 = null;
			if (!result.CompletedSynchronously)
			{
				EmptySendContext emptySendContext = (EmptySendContext)result.AsyncState;
				try
				{
					emptySendContext.writer.EndGetContentStream(result).Close();
				}
				catch (Exception ex)
				{
					result2 = ex;
				}
				catch
				{
					result2 = new Exception(SR.GetString("net_nonClsCompliantException"));
				}
				emptySendContext.result.InvokeCallback(result2);
			}
		}

		internal virtual IAsyncResult BeginSend(BaseWriter writer, bool sendEnvelope, AsyncCallback callback, object state)
		{
			PrepareHeaders(sendEnvelope);
			writer.WriteHeaders(Headers);
			if (Content != null)
			{
				return Content.BeginSend(writer, callback, state);
			}
			LazyAsyncResult result = new LazyAsyncResult(this, state, callback);
			IAsyncResult asyncResult = writer.BeginGetContentStream(EmptySendCallback, new EmptySendContext(writer, result));
			if (asyncResult.CompletedSynchronously)
			{
				writer.EndGetContentStream(asyncResult).Close();
			}
			return result;
		}

		internal virtual void EndSend(IAsyncResult asyncResult)
		{
			if (asyncResult == null)
			{
				throw new ArgumentNullException("asyncResult");
			}
			if (Content != null)
			{
				Content.EndSend(asyncResult);
				return;
			}
			LazyAsyncResult lazyAsyncResult = asyncResult as LazyAsyncResult;
			if (lazyAsyncResult == null || lazyAsyncResult.AsyncObject != this)
			{
				throw new ArgumentException(SR.GetString("net_io_invalidasyncresult"));
			}
			if (lazyAsyncResult.EndCalled)
			{
				throw new InvalidOperationException(SR.GetString("net_io_invalidendcall", "EndSend"));
			}
			lazyAsyncResult.InternalWaitForCompletion();
			lazyAsyncResult.EndCalled = true;
			if (!(lazyAsyncResult.Result is Exception))
			{
				return;
			}
			throw (Exception)lazyAsyncResult.Result;
		}

		internal virtual void Send(BaseWriter writer, bool sendEnvelope)
		{
			if (sendEnvelope)
			{
				PrepareEnvelopeHeaders(sendEnvelope);
				writer.WriteHeaders(EnvelopeHeaders);
			}
			PrepareHeaders(sendEnvelope);
			writer.WriteHeaders(Headers);
			if (Content != null)
			{
				Content.Send(writer);
			}
			else
			{
				writer.GetContentStream().Close();
			}
		}

		internal void PrepareEnvelopeHeaders(bool sendEnvelope)
		{
			EnvelopeHeaders[MailHeaderInfo.GetString(MailHeaderID.XSender)] = From.ToEncodedString();
			EnvelopeHeaders.Remove(MailHeaderInfo.GetString(MailHeaderID.XReceiver));
			foreach (MailAddress item in To)
			{
				EnvelopeHeaders.Add(MailHeaderInfo.GetString(MailHeaderID.XReceiver), item.ToEncodedString());
			}
			foreach (MailAddress item2 in CC)
			{
				EnvelopeHeaders.Add(MailHeaderInfo.GetString(MailHeaderID.XReceiver), item2.ToEncodedString());
			}
			foreach (MailAddress item3 in Bcc)
			{
				EnvelopeHeaders.Add(MailHeaderInfo.GetString(MailHeaderID.XReceiver), item3.ToEncodedString());
			}
		}

		internal void PrepareHeaders(bool sendEnvelope)
		{
			Headers[MailHeaderInfo.GetString(MailHeaderID.MimeVersion)] = "1.0";
			Headers[MailHeaderInfo.GetString(MailHeaderID.From)] = From.ToEncodedString();
			if (Sender != null)
			{
				Headers[MailHeaderInfo.GetString(MailHeaderID.Sender)] = Sender.ToEncodedString();
			}
			else
			{
				Headers.Remove(MailHeaderInfo.GetString(MailHeaderID.Sender));
			}
			if (To.Count > 0)
			{
				Headers[MailHeaderInfo.GetString(MailHeaderID.To)] = To.ToEncodedString();
			}
			else
			{
				Headers.Remove(MailHeaderInfo.GetString(MailHeaderID.To));
			}
			if (CC.Count > 0)
			{
				Headers[MailHeaderInfo.GetString(MailHeaderID.Cc)] = CC.ToEncodedString();
			}
			else
			{
				Headers.Remove(MailHeaderInfo.GetString(MailHeaderID.Cc));
			}
			if (replyTo != null)
			{
				Headers[MailHeaderInfo.GetString(MailHeaderID.ReplyTo)] = ReplyTo.ToEncodedString();
			}
			if (priority == MailPriority.High)
			{
				Headers[MailHeaderInfo.GetString(MailHeaderID.XPriority)] = "1";
				Headers[MailHeaderInfo.GetString(MailHeaderID.Priority)] = "urgent";
				Headers[MailHeaderInfo.GetString(MailHeaderID.Importance)] = "high";
			}
			else if (priority == MailPriority.Low)
			{
				Headers[MailHeaderInfo.GetString(MailHeaderID.XPriority)] = "5";
				Headers[MailHeaderInfo.GetString(MailHeaderID.Priority)] = "non-urgent";
				Headers[MailHeaderInfo.GetString(MailHeaderID.Importance)] = "low";
			}
			else if (priority != (MailPriority)(-1))
			{
				Headers.Remove(MailHeaderInfo.GetString(MailHeaderID.XPriority));
				Headers.Remove(MailHeaderInfo.GetString(MailHeaderID.Priority));
				Headers.Remove(MailHeaderInfo.GetString(MailHeaderID.Importance));
			}
			Headers[MailHeaderInfo.GetString(MailHeaderID.Date)] = MailBnfHelper.GetDateTimeString(DateTime.Now, null);
			if (subject != null && subject != string.Empty)
			{
				Headers[MailHeaderInfo.GetString(MailHeaderID.Subject)] = MimeBasePart.EncodeHeaderValue(subject, subjectEncoding, MimeBasePart.ShouldUseBase64Encoding(subjectEncoding));
			}
			else
			{
				Headers.Remove(MailHeaderInfo.GetString(MailHeaderID.Subject));
			}
		}
	}
}
