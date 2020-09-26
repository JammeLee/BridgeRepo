using System.ComponentModel;

namespace System.Net
{
	internal delegate void CompletionDelegate(byte[] responseBytes, Exception exception, AsyncOperation asyncOp);
}
