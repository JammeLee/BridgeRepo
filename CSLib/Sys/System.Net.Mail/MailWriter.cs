using System.Collections.Specialized;
using System.IO;
using System.Net.Mime;

namespace System.Net.Mail
{
	internal class MailWriter : BaseWriter
	{
		private static byte[] CRLF = new byte[2]
		{
			13,
			10
		};

		private static int DefaultLineLength = 78;

		private Stream contentStream;

		private bool isInContent;

		private int lineLength;

		private EventHandler onCloseHandler;

		private Stream stream;

		private BufferBuilder bufferBuilder = new BufferBuilder();

		private static AsyncCallback onWrite = OnWrite;

		internal MailWriter(Stream stream)
			: this(stream, DefaultLineLength)
		{
		}

		internal MailWriter(Stream stream, int lineLength)
		{
			if (stream == null)
			{
				throw new ArgumentNullException("stream");
			}
			if (lineLength < 0)
			{
				throw new ArgumentOutOfRangeException("lineLength");
			}
			this.stream = stream;
			this.lineLength = lineLength;
			onCloseHandler = OnClose;
		}

		internal override void Close()
		{
			stream.Write(CRLF, 0, 2);
			stream.Close();
		}

		internal IAsyncResult BeginGetContentStream(ContentTransferEncoding contentTransferEncoding, AsyncCallback callback, object state)
		{
			MultiAsyncResult multiAsyncResult = new MultiAsyncResult(this, callback, state);
			Stream result = GetContentStream(contentTransferEncoding, multiAsyncResult);
			if (!(multiAsyncResult.Result is Exception))
			{
				multiAsyncResult.Result = result;
			}
			multiAsyncResult.CompleteSequence();
			return multiAsyncResult;
		}

		internal override IAsyncResult BeginGetContentStream(AsyncCallback callback, object state)
		{
			return BeginGetContentStream(ContentTransferEncoding.SevenBit, callback, state);
		}

		internal override Stream EndGetContentStream(IAsyncResult result)
		{
			object obj = MultiAsyncResult.End(result);
			if (obj is Exception)
			{
				throw (Exception)obj;
			}
			return (Stream)obj;
		}

		internal Stream GetContentStream(ContentTransferEncoding contentTransferEncoding)
		{
			return GetContentStream(contentTransferEncoding, null);
		}

		internal override Stream GetContentStream()
		{
			return GetContentStream(ContentTransferEncoding.SevenBit);
		}

		private Stream GetContentStream(ContentTransferEncoding contentTransferEncoding, MultiAsyncResult multiResult)
		{
			if (isInContent)
			{
				throw new InvalidOperationException(SR.GetString("MailWriterIsInContent"));
			}
			isInContent = true;
			bufferBuilder.Append(CRLF);
			Flush(multiResult);
			Stream stream = this.stream;
			switch (contentTransferEncoding)
			{
			case ContentTransferEncoding.SevenBit:
				stream = new SevenBitStream(stream);
				break;
			case ContentTransferEncoding.QuotedPrintable:
				stream = new QuotedPrintableStream(stream, lineLength);
				break;
			case ContentTransferEncoding.Base64:
				stream = new Base64Stream(stream, lineLength);
				break;
			}
			return contentStream = new ClosableStream(stream, onCloseHandler);
		}

		internal override void WriteHeader(string name, string value)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			if (isInContent)
			{
				throw new InvalidOperationException(SR.GetString("MailWriterIsInContent"));
			}
			bufferBuilder.Append(name);
			bufferBuilder.Append(": ");
			WriteAndFold(value);
			bufferBuilder.Append(CRLF);
		}

		internal override void WriteHeaders(NameValueCollection headers)
		{
			if (headers == null)
			{
				throw new ArgumentNullException("headers");
			}
			if (isInContent)
			{
				throw new InvalidOperationException(SR.GetString("MailWriterIsInContent"));
			}
			foreach (string header in headers)
			{
				string[] values = headers.GetValues(header);
				string[] array = values;
				foreach (string value in array)
				{
					WriteHeader(header, value);
				}
			}
		}

		private void OnClose(object sender, EventArgs args)
		{
			contentStream.Flush();
			contentStream = null;
		}

		private static void OnWrite(IAsyncResult result)
		{
			if (!result.CompletedSynchronously)
			{
				MultiAsyncResult multiAsyncResult = (MultiAsyncResult)result.AsyncState;
				MailWriter mailWriter = (MailWriter)multiAsyncResult.Context;
				try
				{
					mailWriter.stream.EndWrite(result);
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

		private void Flush(MultiAsyncResult multiResult)
		{
			if (bufferBuilder.Length <= 0)
			{
				return;
			}
			if (multiResult != null)
			{
				multiResult.Enter();
				IAsyncResult asyncResult = stream.BeginWrite(bufferBuilder.GetBuffer(), 0, bufferBuilder.Length, onWrite, multiResult);
				if (asyncResult.CompletedSynchronously)
				{
					stream.EndWrite(asyncResult);
					multiResult.Leave();
				}
			}
			else
			{
				stream.Write(bufferBuilder.GetBuffer(), 0, bufferBuilder.Length);
			}
			bufferBuilder.Reset();
		}

		private void WriteAndFold(string value)
		{
			if (value.Length < DefaultLineLength)
			{
				bufferBuilder.Append(value);
				return;
			}
			int num = 0;
			int length = value.Length;
			while (length - num > DefaultLineLength)
			{
				int num2 = value.LastIndexOf(' ', num + DefaultLineLength - 1, DefaultLineLength - 1);
				if (num2 > -1)
				{
					bufferBuilder.Append(value, num, num2 - num);
					bufferBuilder.Append(CRLF);
					num = num2;
				}
				else
				{
					bufferBuilder.Append(value, num, DefaultLineLength);
					num += DefaultLineLength;
				}
			}
			if (num < length)
			{
				bufferBuilder.Append(value, num, length - num);
			}
		}
	}
}
