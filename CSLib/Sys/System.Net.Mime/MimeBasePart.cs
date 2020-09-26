using System.Collections.Specialized;
using System.Net.Mail;
using System.Text;

namespace System.Net.Mime
{
	internal class MimeBasePart
	{
		internal class MimePartAsyncResult : LazyAsyncResult
		{
			internal MimePartAsyncResult(MimeBasePart part, object state, AsyncCallback callback)
				: base(part, state, callback)
			{
			}
		}

		internal const string defaultCharSet = "utf-8";

		protected ContentType contentType;

		protected ContentDisposition contentDisposition;

		private HeaderCollection headers;

		internal string ContentID
		{
			get
			{
				return Headers[MailHeaderInfo.GetString(MailHeaderID.ContentID)];
			}
			set
			{
				if (string.IsNullOrEmpty(value))
				{
					Headers.Remove(MailHeaderInfo.GetString(MailHeaderID.ContentID));
				}
				else
				{
					Headers[MailHeaderInfo.GetString(MailHeaderID.ContentID)] = value;
				}
			}
		}

		internal string ContentLocation
		{
			get
			{
				return Headers[MailHeaderInfo.GetString(MailHeaderID.ContentLocation)];
			}
			set
			{
				if (string.IsNullOrEmpty(value))
				{
					Headers.Remove(MailHeaderInfo.GetString(MailHeaderID.ContentLocation));
				}
				else
				{
					Headers[MailHeaderInfo.GetString(MailHeaderID.ContentLocation)] = value;
				}
			}
		}

		internal NameValueCollection Headers
		{
			get
			{
				if (headers == null)
				{
					headers = new HeaderCollection();
				}
				if (contentType == null)
				{
					contentType = new ContentType();
				}
				contentType.PersistIfNeeded(headers, forcePersist: false);
				if (contentDisposition != null)
				{
					contentDisposition.PersistIfNeeded(headers, forcePersist: false);
				}
				return headers;
			}
		}

		internal ContentType ContentType
		{
			get
			{
				if (contentType == null)
				{
					contentType = new ContentType();
				}
				return contentType;
			}
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException("value");
				}
				contentType = value;
				contentType.PersistIfNeeded((HeaderCollection)Headers, forcePersist: true);
			}
		}

		internal MimeBasePart()
		{
		}

		internal static bool ShouldUseBase64Encoding(Encoding encoding)
		{
			if (encoding == Encoding.Unicode || encoding == Encoding.UTF8 || encoding == Encoding.UTF32 || encoding == Encoding.BigEndianUnicode)
			{
				return true;
			}
			return false;
		}

		internal static string EncodeHeaderValue(string value, Encoding encoding, bool base64Encoding)
		{
			StringBuilder stringBuilder = new StringBuilder();
			if (encoding == null && IsAscii(value, permitCROrLF: false))
			{
				return value;
			}
			if (encoding == null)
			{
				encoding = Encoding.GetEncoding("utf-8");
			}
			string value2 = encoding.BodyName;
			if (encoding == Encoding.BigEndianUnicode)
			{
				value2 = "utf-16be";
			}
			stringBuilder.Append("=?");
			stringBuilder.Append(value2);
			stringBuilder.Append("?");
			stringBuilder.Append(base64Encoding ? "B" : "Q");
			stringBuilder.Append("?");
			byte[] bytes = encoding.GetBytes(value);
			if (base64Encoding)
			{
				Base64Stream base64Stream = new Base64Stream(-1);
				base64Stream.EncodeBytes(bytes, 0, bytes.Length, dontDeferFinalBytes: true);
				stringBuilder.Append(Encoding.ASCII.GetString(base64Stream.WriteState.Buffer, 0, base64Stream.WriteState.Length));
			}
			else
			{
				QuotedPrintableStream quotedPrintableStream = new QuotedPrintableStream(-1);
				quotedPrintableStream.EncodeBytes(bytes, 0, bytes.Length);
				stringBuilder.Append(Encoding.ASCII.GetString(quotedPrintableStream.WriteState.Buffer, 0, quotedPrintableStream.WriteState.Length));
			}
			stringBuilder.Append("?=");
			return stringBuilder.ToString();
		}

		internal static string DecodeHeaderValue(string value)
		{
			if (value == null || value.Length == 0)
			{
				return string.Empty;
			}
			string[] array = value.Split('?');
			if (array.Length != 5 || array[0] != "=" || array[4] != "=")
			{
				return value;
			}
			string name = array[1];
			bool flag = array[2] == "B";
			byte[] bytes = Encoding.ASCII.GetBytes(array[3]);
			int count;
			if (flag)
			{
				Base64Stream base64Stream = new Base64Stream();
				count = base64Stream.DecodeBytes(bytes, 0, bytes.Length);
			}
			else
			{
				QuotedPrintableStream quotedPrintableStream = new QuotedPrintableStream();
				count = quotedPrintableStream.DecodeBytes(bytes, 0, bytes.Length);
			}
			Encoding encoding = Encoding.GetEncoding(name);
			return encoding.GetString(bytes, 0, count);
		}

		internal static Encoding DecodeEncoding(string value)
		{
			if (value == null || value.Length == 0)
			{
				return null;
			}
			string[] array = value.Split('?');
			if (array.Length != 5 || array[0] != "=" || array[4] != "=")
			{
				return null;
			}
			string name = array[1];
			return Encoding.GetEncoding(name);
		}

		internal static bool IsAscii(string value, bool permitCROrLF)
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			foreach (char c in value)
			{
				if (c > '\u007f')
				{
					return false;
				}
				if (!permitCROrLF && (c == '\r' || c == '\n'))
				{
					return false;
				}
			}
			return true;
		}

		internal static bool IsAnsi(string value, bool permitCROrLF)
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			foreach (char c in value)
			{
				if (c > 'ÿ')
				{
					return false;
				}
				if (!permitCROrLF && (c == '\r' || c == '\n'))
				{
					return false;
				}
			}
			return true;
		}

		internal virtual void Send(BaseWriter writer)
		{
			throw new NotImplementedException();
		}

		internal virtual IAsyncResult BeginSend(BaseWriter writer, AsyncCallback callback, object state)
		{
			throw new NotImplementedException();
		}

		internal void EndSend(IAsyncResult asyncResult)
		{
			if (asyncResult == null)
			{
				throw new ArgumentNullException("asyncResult");
			}
			LazyAsyncResult lazyAsyncResult = asyncResult as MimePartAsyncResult;
			if (lazyAsyncResult == null || lazyAsyncResult.AsyncObject != this)
			{
				throw new ArgumentException(SR.GetString("net_io_invalidasyncresult"), "asyncResult");
			}
			if (lazyAsyncResult.EndCalled)
			{
				throw new InvalidOperationException(SR.GetString("net_io_invalidendcall", "EndSend"));
			}
			lazyAsyncResult.InternalWaitForCompletion();
			lazyAsyncResult.EndCalled = true;
			if (lazyAsyncResult.Result is Exception)
			{
				throw (Exception)lazyAsyncResult.Result;
			}
		}
	}
}
