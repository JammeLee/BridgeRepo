using System.Collections.Specialized;
using System.IO;

namespace System.Net.Mime
{
	internal abstract class BaseWriter
	{
		internal abstract IAsyncResult BeginGetContentStream(AsyncCallback callback, object state);

		internal abstract Stream EndGetContentStream(IAsyncResult result);

		internal abstract Stream GetContentStream();

		internal abstract void WriteHeader(string name, string value);

		internal abstract void WriteHeaders(NameValueCollection headers);

		internal abstract void Close();
	}
}
