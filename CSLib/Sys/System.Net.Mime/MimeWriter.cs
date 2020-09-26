using System.Collections.Specialized;
using System.IO;
using System.Text;

namespace System.Net.Mime
{
	internal class MimeWriter : BaseWriter
	{
		private static int DefaultLineLength = 78;

		private static byte[] DASHDASH = new byte[2]
		{
			45,
			45
		};

		private static byte[] CRLF = new byte[2]
		{
			13,
			10
		};

		private byte[] boundaryBytes;

		private BufferBuilder bufferBuilder = new BufferBuilder();

		private Stream contentStream;

		private bool isInContent;

		private int lineLength;

		private EventHandler onCloseHandler;

		private Stream stream;

		private bool writeBoundary = true;

		private string preface;

		private static AsyncCallback onWrite = OnWrite;

		internal MimeWriter(Stream stream, string boundary)
			: this(stream, boundary, null, DefaultLineLength)
		{
		}

		internal MimeWriter(Stream stream, string boundary, string preface, int lineLength)
		{
			if (stream == null)
			{
				throw new ArgumentNullException("stream");
			}
			if (boundary == null)
			{
				throw new ArgumentNullException("boundary");
			}
			if (lineLength < 40)
			{
				throw new ArgumentOutOfRangeException("lineLength", lineLength, SR.GetString("MailWriterLineLengthTooSmall"));
			}
			this.stream = stream;
			this.lineLength = lineLength;
			onCloseHandler = OnClose;
			boundaryBytes = Encoding.ASCII.GetBytes(boundary);
			this.preface = preface;
		}

		internal IAsyncResult BeginClose(AsyncCallback callback, object state)
		{
			MultiAsyncResult multiAsyncResult = new MultiAsyncResult(this, callback, state);
			Close(multiAsyncResult);
			multiAsyncResult.CompleteSequence();
			return multiAsyncResult;
		}

		internal void EndClose(IAsyncResult result)
		{
			MultiAsyncResult.End(result);
			stream.Close();
		}

		internal override void Close()
		{
			Close(null);
			stream.Close();
		}

		private void Close(MultiAsyncResult multiResult)
		{
			bufferBuilder.Append(CRLF);
			bufferBuilder.Append(DASHDASH);
			bufferBuilder.Append(boundaryBytes);
			bufferBuilder.Append(DASHDASH);
			bufferBuilder.Append(CRLF);
			Flush(multiResult);
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
			if (isInContent)
			{
				throw new InvalidOperationException(SR.GetString("MailWriterIsInContent"));
			}
			isInContent = true;
			return GetContentStream(contentTransferEncoding, null);
		}

		internal override Stream GetContentStream()
		{
			return GetContentStream(ContentTransferEncoding.SevenBit);
		}

		private Stream GetContentStream(ContentTransferEncoding contentTransferEncoding, MultiAsyncResult multiResult)
		{
			CheckBoundary();
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
			CheckBoundary();
			bufferBuilder.Append(name);
			bufferBuilder.Append(": ");
			WriteAndFold(value, name.Length + 2);
			bufferBuilder.Append(CRLF);
		}

		internal override void WriteHeaders(NameValueCollection headers)
		{
			if (headers == null)
			{
				throw new ArgumentNullException("headers");
			}
			foreach (string header in headers)
			{
				WriteHeader(header, headers[header]);
			}
		}

		private void OnClose(object sender, EventArgs args)
		{
			if (contentStream == sender)
			{
				contentStream.Flush();
				contentStream = null;
				writeBoundary = true;
				isInContent = false;
			}
		}

		private void CheckBoundary()
		{
			if (preface != null)
			{
				bufferBuilder.Append(preface);
				preface = null;
			}
			if (writeBoundary)
			{
				bufferBuilder.Append(CRLF);
				bufferBuilder.Append(DASHDASH);
				bufferBuilder.Append(boundaryBytes);
				bufferBuilder.Append(CRLF);
				writeBoundary = false;
			}
		}

		private static void OnWrite(IAsyncResult result)
		{
			if (!result.CompletedSynchronously)
			{
				MultiAsyncResult multiAsyncResult = (MultiAsyncResult)result.AsyncState;
				MimeWriter mimeWriter = (MimeWriter)multiAsyncResult.Context;
				try
				{
					mimeWriter.stream.EndWrite(result);
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

		private void WriteAndFold(string value, int startLength)
		{
			int i = 0;
			int num = 0;
			int num2 = 0;
			for (; i != value.Length; i++)
			{
				if (value[i] != ' ' && value[i] != '\t')
				{
					continue;
				}
				if (i - num2 >= lineLength - startLength)
				{
					startLength = 0;
					if (num == num2)
					{
						num = i;
					}
					bufferBuilder.Append(value, num2, num - num2);
					bufferBuilder.Append(CRLF);
					num2 = num;
				}
				num = i;
			}
			if (i - num2 > 0)
			{
				bufferBuilder.Append(value, num2, i - num2);
			}
		}
	}
}
