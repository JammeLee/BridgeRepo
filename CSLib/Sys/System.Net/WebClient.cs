using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Net.Cache;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Text;
using System.Threading;

namespace System.Net
{
	[ComVisible(true)]
	public class WebClient : Component
	{
		private class ProgressData
		{
			internal long BytesSent;

			internal long TotalBytesToSend = -1L;

			internal long BytesReceived;

			internal long TotalBytesToReceive = -1L;

			internal bool HasUploadPhase;

			internal void Reset()
			{
				BytesSent = 0L;
				TotalBytesToSend = -1L;
				BytesReceived = 0L;
				TotalBytesToReceive = -1L;
				HasUploadPhase = false;
			}
		}

		private class DownloadBitsState
		{
			internal WebClient WebClient;

			internal Stream WriteStream;

			internal byte[] InnerBuffer;

			internal AsyncOperation AsyncOp;

			internal WebRequest Request;

			internal CompletionDelegate CompletionDelegate;

			internal Stream ReadStream;

			internal ScatterGatherBuffers SgBuffers;

			internal long ContentLength;

			internal long Length;

			internal int Offset;

			internal ProgressData Progress;

			internal bool Async => AsyncOp != null;

			internal DownloadBitsState(WebRequest request, Stream writeStream, CompletionDelegate completionDelegate, AsyncOperation asyncOp, ProgressData progress, WebClient webClient)
			{
				WriteStream = writeStream;
				Request = request;
				AsyncOp = asyncOp;
				CompletionDelegate = completionDelegate;
				WebClient = webClient;
				Progress = progress;
			}

			internal int SetResponse(WebResponse response)
			{
				ContentLength = response.ContentLength;
				if (ContentLength == -1 || ContentLength > 65536)
				{
					Length = 65536L;
				}
				else
				{
					Length = ContentLength;
				}
				if (WriteStream == null)
				{
					if (ContentLength > int.MaxValue)
					{
						throw new WebException(SR.GetString("net_webstatus_MessageLengthLimitExceeded"), WebExceptionStatus.MessageLengthLimitExceeded);
					}
					SgBuffers = new ScatterGatherBuffers(Length);
				}
				InnerBuffer = new byte[(int)Length];
				ReadStream = response.GetResponseStream();
				if (Async && response.ContentLength >= 0)
				{
					Progress.TotalBytesToReceive = response.ContentLength;
				}
				if (Async)
				{
					if (ReadStream == null || ReadStream == Stream.Null)
					{
						DownloadBitsReadCallbackState(this, null);
					}
					else
					{
						ReadStream.BeginRead(InnerBuffer, Offset, (int)Length - Offset, DownloadBitsReadCallback, this);
					}
					return -1;
				}
				if (ReadStream == null || ReadStream == Stream.Null)
				{
					return 0;
				}
				return ReadStream.Read(InnerBuffer, Offset, (int)Length - Offset);
			}

			internal bool RetrieveBytes(ref int bytesRetrieved)
			{
				if (bytesRetrieved > 0)
				{
					if (WriteStream != null)
					{
						WriteStream.Write(InnerBuffer, 0, bytesRetrieved);
					}
					else
					{
						SgBuffers.Write(InnerBuffer, 0, bytesRetrieved);
					}
					if (Async)
					{
						Progress.BytesReceived += bytesRetrieved;
					}
					if (Offset != ContentLength)
					{
						if (Async)
						{
							WebClient.PostProgressChanged(AsyncOp, Progress);
							ReadStream.BeginRead(InnerBuffer, Offset, (int)Length - Offset, DownloadBitsReadCallback, this);
						}
						else
						{
							bytesRetrieved = ReadStream.Read(InnerBuffer, Offset, (int)Length - Offset);
						}
						return false;
					}
				}
				if (Async)
				{
					if (Progress.TotalBytesToReceive < 0)
					{
						Progress.TotalBytesToReceive = Progress.BytesReceived;
					}
					WebClient.PostProgressChanged(AsyncOp, Progress);
				}
				if (ReadStream != null)
				{
					ReadStream.Close();
				}
				if (WriteStream != null)
				{
					WriteStream.Close();
				}
				else if (WriteStream == null)
				{
					byte[] array = new byte[SgBuffers.Length];
					if (SgBuffers.Length > 0)
					{
						BufferOffsetSize[] buffers = SgBuffers.GetBuffers();
						int num = 0;
						foreach (BufferOffsetSize bufferOffsetSize in buffers)
						{
							Buffer.BlockCopy(bufferOffsetSize.Buffer, 0, array, num, bufferOffsetSize.Size);
							num += bufferOffsetSize.Size;
						}
					}
					InnerBuffer = array;
				}
				return true;
			}

			internal void Close()
			{
				if (WriteStream != null)
				{
					WriteStream.Close();
				}
				if (ReadStream != null)
				{
					ReadStream.Close();
				}
			}
		}

		private class UploadBitsState
		{
			internal WebClient WebClient;

			internal Stream WriteStream;

			internal byte[] InnerBuffer;

			internal byte[] Header;

			internal byte[] Footer;

			internal AsyncOperation AsyncOp;

			internal WebRequest Request;

			internal CompletionDelegate CompletionDelegate;

			internal Stream ReadStream;

			internal long Length;

			internal int Offset;

			internal ProgressData Progress;

			internal bool FileUpload => ReadStream != null;

			internal bool Async => AsyncOp != null;

			internal UploadBitsState(WebRequest request, Stream readStream, byte[] buffer, byte[] header, byte[] footer, CompletionDelegate completionDelegate, AsyncOperation asyncOp, ProgressData progress, WebClient webClient)
			{
				InnerBuffer = buffer;
				Header = header;
				Footer = footer;
				ReadStream = readStream;
				Request = request;
				AsyncOp = asyncOp;
				CompletionDelegate = completionDelegate;
				if (AsyncOp != null)
				{
					Progress = progress;
					Progress.HasUploadPhase = true;
					Progress.TotalBytesToSend = ((request.ContentLength < 0) ? (-1) : request.ContentLength);
				}
				WebClient = webClient;
			}

			internal void SetRequestStream(Stream writeStream)
			{
				WriteStream = writeStream;
				byte[] array = null;
				if (Header != null)
				{
					array = Header;
					Header = null;
				}
				else
				{
					array = new byte[0];
				}
				if (Async)
				{
					Progress.BytesSent += array.Length;
					WriteStream.BeginWrite(array, 0, array.Length, UploadBitsWriteCallback, this);
				}
				else
				{
					WriteStream.Write(array, 0, array.Length);
				}
			}

			internal bool WriteBytes()
			{
				byte[] array = null;
				int num = 0;
				if (Async)
				{
					WebClient.PostProgressChanged(AsyncOp, Progress);
				}
				if (FileUpload)
				{
					int num2 = 0;
					if (InnerBuffer != null)
					{
						num2 = ReadStream.Read(InnerBuffer, 0, InnerBuffer.Length);
						if (num2 <= 0)
						{
							ReadStream.Close();
							InnerBuffer = null;
						}
					}
					if (InnerBuffer != null)
					{
						num = num2;
						array = InnerBuffer;
					}
					else
					{
						if (Footer == null)
						{
							return true;
						}
						num = Footer.Length;
						array = Footer;
						Footer = null;
					}
				}
				else
				{
					if (InnerBuffer == null)
					{
						return true;
					}
					num = InnerBuffer.Length;
					array = InnerBuffer;
					InnerBuffer = null;
				}
				if (Async)
				{
					Progress.BytesSent += num;
					WriteStream.BeginWrite(array, 0, num, UploadBitsWriteCallback, this);
				}
				else
				{
					WriteStream.Write(array, 0, num);
				}
				return false;
			}

			internal void Close()
			{
				if (WriteStream != null)
				{
					WriteStream.Close();
				}
				if (ReadStream != null)
				{
					ReadStream.Close();
				}
			}
		}

		private class WebClientWriteStream : Stream
		{
			private WebRequest m_request;

			private Stream m_stream;

			private WebClient m_WebClient;

			public override bool CanRead => m_stream.CanRead;

			public override bool CanSeek => m_stream.CanSeek;

			public override bool CanWrite => m_stream.CanWrite;

			public override bool CanTimeout => m_stream.CanTimeout;

			public override int ReadTimeout
			{
				get
				{
					return m_stream.ReadTimeout;
				}
				set
				{
					m_stream.ReadTimeout = value;
				}
			}

			public override int WriteTimeout
			{
				get
				{
					return m_stream.WriteTimeout;
				}
				set
				{
					m_stream.WriteTimeout = value;
				}
			}

			public override long Length => m_stream.Length;

			public override long Position
			{
				get
				{
					return m_stream.Position;
				}
				set
				{
					m_stream.Position = value;
				}
			}

			public WebClientWriteStream(Stream stream, WebRequest request, WebClient webClient)
			{
				m_request = request;
				m_stream = stream;
				m_WebClient = webClient;
			}

			[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
			public override IAsyncResult BeginRead(byte[] buffer, int offset, int size, AsyncCallback callback, object state)
			{
				return m_stream.BeginRead(buffer, offset, size, callback, state);
			}

			[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
			public override IAsyncResult BeginWrite(byte[] buffer, int offset, int size, AsyncCallback callback, object state)
			{
				return m_stream.BeginWrite(buffer, offset, size, callback, state);
			}

			protected override void Dispose(bool disposing)
			{
				try
				{
					if (disposing)
					{
						m_stream.Close();
						m_WebClient.GetWebResponse(m_request).Close();
					}
				}
				finally
				{
					base.Dispose(disposing);
				}
			}

			public override int EndRead(IAsyncResult result)
			{
				return m_stream.EndRead(result);
			}

			public override void EndWrite(IAsyncResult result)
			{
				m_stream.EndWrite(result);
			}

			public override void Flush()
			{
				m_stream.Flush();
			}

			public override int Read(byte[] buffer, int offset, int count)
			{
				return m_stream.Read(buffer, offset, count);
			}

			public override long Seek(long offset, SeekOrigin origin)
			{
				return m_stream.Seek(offset, origin);
			}

			public override void SetLength(long value)
			{
				m_stream.SetLength(value);
			}

			public override void Write(byte[] buffer, int offset, int count)
			{
				m_stream.Write(buffer, offset, count);
			}
		}

		private const int DefaultCopyBufferLength = 8192;

		private const int DefaultDownloadBufferLength = 65536;

		private const string DefaultUploadFileContentType = "application/octet-stream";

		private const string UploadFileContentType = "multipart/form-data";

		private const string UploadValuesContentType = "application/x-www-form-urlencoded";

		private Uri m_baseAddress;

		private ICredentials m_credentials;

		private WebHeaderCollection m_headers;

		private NameValueCollection m_requestParameters;

		private WebResponse m_WebResponse;

		private WebRequest m_WebRequest;

		private Encoding m_Encoding = Encoding.Default;

		private string m_Method;

		private long m_ContentLength = -1L;

		private bool m_InitWebClientAsync;

		private bool m_Cancelled;

		private ProgressData m_Progress;

		private IWebProxy m_Proxy;

		private bool m_ProxySet;

		private RequestCachePolicy m_CachePolicy;

		private int m_CallNesting;

		private AsyncOperation m_AsyncOp;

		private SendOrPostCallback openReadOperationCompleted;

		private SendOrPostCallback openWriteOperationCompleted;

		private SendOrPostCallback downloadStringOperationCompleted;

		private SendOrPostCallback downloadDataOperationCompleted;

		private SendOrPostCallback downloadFileOperationCompleted;

		private SendOrPostCallback uploadStringOperationCompleted;

		private SendOrPostCallback uploadDataOperationCompleted;

		private SendOrPostCallback uploadFileOperationCompleted;

		private SendOrPostCallback uploadValuesOperationCompleted;

		private SendOrPostCallback reportDownloadProgressChanged;

		private SendOrPostCallback reportUploadProgressChanged;

		public Encoding Encoding
		{
			get
			{
				return m_Encoding;
			}
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException("Encoding");
				}
				m_Encoding = value;
			}
		}

		public string BaseAddress
		{
			get
			{
				if (!(m_baseAddress == null))
				{
					return m_baseAddress.ToString();
				}
				return string.Empty;
			}
			set
			{
				if (value == null || value.Length == 0)
				{
					m_baseAddress = null;
					return;
				}
				try
				{
					m_baseAddress = new Uri(value);
				}
				catch (UriFormatException innerException)
				{
					throw new ArgumentException(SR.GetString("net_webclient_invalid_baseaddress"), "value", innerException);
				}
			}
		}

		public ICredentials Credentials
		{
			get
			{
				return m_credentials;
			}
			set
			{
				m_credentials = value;
			}
		}

		public bool UseDefaultCredentials
		{
			get
			{
				if (!(m_credentials is SystemNetworkCredential))
				{
					return false;
				}
				return true;
			}
			set
			{
				m_credentials = (value ? CredentialCache.DefaultCredentials : null);
			}
		}

		public WebHeaderCollection Headers
		{
			get
			{
				if (m_headers == null)
				{
					m_headers = new WebHeaderCollection(WebHeaderCollectionType.WebRequest);
				}
				return m_headers;
			}
			set
			{
				m_headers = value;
			}
		}

		public NameValueCollection QueryString
		{
			get
			{
				if (m_requestParameters == null)
				{
					m_requestParameters = new NameValueCollection();
				}
				return m_requestParameters;
			}
			set
			{
				m_requestParameters = value;
			}
		}

		public WebHeaderCollection ResponseHeaders
		{
			get
			{
				if (m_WebResponse != null)
				{
					return m_WebResponse.Headers;
				}
				return null;
			}
		}

		public IWebProxy Proxy
		{
			get
			{
				ExceptionHelper.WebPermissionUnrestricted.Demand();
				if (!m_ProxySet)
				{
					return WebRequest.InternalDefaultWebProxy;
				}
				return m_Proxy;
			}
			set
			{
				ExceptionHelper.WebPermissionUnrestricted.Demand();
				m_Proxy = value;
				m_ProxySet = true;
			}
		}

		public RequestCachePolicy CachePolicy
		{
			get
			{
				return m_CachePolicy;
			}
			set
			{
				m_CachePolicy = value;
			}
		}

		public bool IsBusy => m_AsyncOp != null;

		public event OpenReadCompletedEventHandler OpenReadCompleted;

		public event OpenWriteCompletedEventHandler OpenWriteCompleted;

		public event DownloadStringCompletedEventHandler DownloadStringCompleted;

		public event DownloadDataCompletedEventHandler DownloadDataCompleted;

		public event AsyncCompletedEventHandler DownloadFileCompleted;

		public event UploadStringCompletedEventHandler UploadStringCompleted;

		public event UploadDataCompletedEventHandler UploadDataCompleted;

		public event UploadFileCompletedEventHandler UploadFileCompleted;

		public event UploadValuesCompletedEventHandler UploadValuesCompleted;

		public event DownloadProgressChangedEventHandler DownloadProgressChanged;

		public event UploadProgressChangedEventHandler UploadProgressChanged;

		private void InitWebClientAsync()
		{
			if (!m_InitWebClientAsync)
			{
				openReadOperationCompleted = OpenReadOperationCompleted;
				openWriteOperationCompleted = OpenWriteOperationCompleted;
				downloadStringOperationCompleted = DownloadStringOperationCompleted;
				downloadDataOperationCompleted = DownloadDataOperationCompleted;
				downloadFileOperationCompleted = DownloadFileOperationCompleted;
				uploadStringOperationCompleted = UploadStringOperationCompleted;
				uploadDataOperationCompleted = UploadDataOperationCompleted;
				uploadFileOperationCompleted = UploadFileOperationCompleted;
				uploadValuesOperationCompleted = UploadValuesOperationCompleted;
				reportDownloadProgressChanged = ReportDownloadProgressChanged;
				reportUploadProgressChanged = ReportUploadProgressChanged;
				m_Progress = new ProgressData();
				m_InitWebClientAsync = true;
			}
		}

		private void ClearWebClientState()
		{
			if (AnotherCallInProgress(Interlocked.Increment(ref m_CallNesting)))
			{
				CompleteWebClientState();
				throw new NotSupportedException(SR.GetString("net_webclient_no_concurrent_io_allowed"));
			}
			m_ContentLength = -1L;
			m_WebResponse = null;
			m_WebRequest = null;
			m_Method = null;
			m_Cancelled = false;
			if (m_Progress != null)
			{
				m_Progress.Reset();
			}
		}

		private void CompleteWebClientState()
		{
			Interlocked.Decrement(ref m_CallNesting);
		}

		protected virtual WebRequest GetWebRequest(Uri address)
		{
			WebRequest webRequest = WebRequest.Create(address);
			CopyHeadersTo(webRequest);
			if (Credentials != null)
			{
				webRequest.Credentials = Credentials;
			}
			if (m_Method != null)
			{
				webRequest.Method = m_Method;
			}
			if (m_ContentLength != -1)
			{
				webRequest.ContentLength = m_ContentLength;
			}
			if (m_ProxySet)
			{
				webRequest.Proxy = m_Proxy;
			}
			if (m_CachePolicy != null)
			{
				webRequest.CachePolicy = m_CachePolicy;
			}
			return webRequest;
		}

		protected virtual WebResponse GetWebResponse(WebRequest request)
		{
			return m_WebResponse = request.GetResponse();
		}

		protected virtual WebResponse GetWebResponse(WebRequest request, IAsyncResult result)
		{
			return m_WebResponse = request.EndGetResponse(result);
		}

		public byte[] DownloadData(string address)
		{
			if (address == null)
			{
				throw new ArgumentNullException("address");
			}
			return DownloadData(GetUri(address));
		}

		public byte[] DownloadData(Uri address)
		{
			if (Logging.On)
			{
				Logging.Enter(Logging.Web, this, "DownloadData", address);
			}
			if (address == null)
			{
				throw new ArgumentNullException("address");
			}
			ClearWebClientState();
			byte[] array = null;
			try
			{
				array = DownloadDataInternal(address, out var _);
				if (Logging.On)
				{
					Logging.Exit(Logging.Web, this, "DownloadData", array);
				}
				return array;
			}
			finally
			{
				CompleteWebClientState();
			}
		}

		private byte[] DownloadDataInternal(Uri address, out WebRequest request)
		{
			if (Logging.On)
			{
				Logging.Enter(Logging.Web, this, "DownloadData", address);
			}
			request = null;
			try
			{
				request = (m_WebRequest = GetWebRequest(GetUri(address)));
				return DownloadBits(request, null, null, null);
			}
			catch (Exception ex)
			{
				if (ex is ThreadAbortException || ex is StackOverflowException || ex is OutOfMemoryException)
				{
					throw;
				}
				if (!(ex is WebException) && !(ex is SecurityException))
				{
					ex = new WebException(SR.GetString("net_webclient"), ex);
				}
				AbortRequest(request);
				throw ex;
			}
			catch
			{
				Exception ex2 = new WebException(SR.GetString("net_webclient"), new Exception(SR.GetString("net_nonClsCompliantException")));
				AbortRequest(request);
				throw ex2;
			}
		}

		public void DownloadFile(string address, string fileName)
		{
			if (address == null)
			{
				throw new ArgumentNullException("address");
			}
			DownloadFile(GetUri(address), fileName);
		}

		public void DownloadFile(Uri address, string fileName)
		{
			if (Logging.On)
			{
				Logging.Enter(Logging.Web, this, "DownloadFile", string.Concat(address, ", ", fileName));
			}
			if (address == null)
			{
				throw new ArgumentNullException("address");
			}
			if (fileName == null)
			{
				throw new ArgumentNullException("fileName");
			}
			WebRequest request = null;
			FileStream fileStream = null;
			bool flag = false;
			ClearWebClientState();
			try
			{
				fileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write);
				request = (m_WebRequest = GetWebRequest(GetUri(address)));
				DownloadBits(request, fileStream, null, null);
				flag = true;
			}
			catch (Exception ex)
			{
				if (ex is ThreadAbortException || ex is StackOverflowException || ex is OutOfMemoryException)
				{
					throw;
				}
				if (!(ex is WebException) && !(ex is SecurityException))
				{
					ex = new WebException(SR.GetString("net_webclient"), ex);
				}
				AbortRequest(request);
				throw ex;
			}
			catch
			{
				Exception ex2 = new WebException(SR.GetString("net_webclient"), new Exception(SR.GetString("net_nonClsCompliantException")));
				AbortRequest(request);
				throw ex2;
			}
			finally
			{
				if (fileStream != null)
				{
					fileStream.Close();
					if (!flag)
					{
						File.Delete(fileName);
					}
					fileStream = null;
				}
				CompleteWebClientState();
			}
			if (Logging.On)
			{
				Logging.Exit(Logging.Web, this, "DownloadFile", "");
			}
		}

		public Stream OpenRead(string address)
		{
			if (address == null)
			{
				throw new ArgumentNullException("address");
			}
			return OpenRead(GetUri(address));
		}

		public Stream OpenRead(Uri address)
		{
			if (Logging.On)
			{
				Logging.Enter(Logging.Web, this, "OpenRead", address);
			}
			if (address == null)
			{
				throw new ArgumentNullException("address");
			}
			WebRequest request = null;
			ClearWebClientState();
			try
			{
				request = (m_WebRequest = GetWebRequest(GetUri(address)));
				Stream responseStream = (m_WebResponse = GetWebResponse(request)).GetResponseStream();
				if (Logging.On)
				{
					Logging.Exit(Logging.Web, this, "OpenRead", responseStream);
				}
				return responseStream;
			}
			catch (Exception ex)
			{
				if (ex is ThreadAbortException || ex is StackOverflowException || ex is OutOfMemoryException)
				{
					throw;
				}
				if (!(ex is WebException) && !(ex is SecurityException))
				{
					ex = new WebException(SR.GetString("net_webclient"), ex);
				}
				AbortRequest(request);
				throw ex;
			}
			catch
			{
				Exception ex2 = new WebException(SR.GetString("net_webclient"), new Exception(SR.GetString("net_nonClsCompliantException")));
				AbortRequest(request);
				throw ex2;
			}
			finally
			{
				CompleteWebClientState();
			}
		}

		public Stream OpenWrite(string address)
		{
			if (address == null)
			{
				throw new ArgumentNullException("address");
			}
			return OpenWrite(GetUri(address), null);
		}

		public Stream OpenWrite(Uri address)
		{
			return OpenWrite(address, null);
		}

		public Stream OpenWrite(string address, string method)
		{
			if (address == null)
			{
				throw new ArgumentNullException("address");
			}
			return OpenWrite(GetUri(address), method);
		}

		public Stream OpenWrite(Uri address, string method)
		{
			if (Logging.On)
			{
				Logging.Enter(Logging.Web, this, "OpenWrite", string.Concat(address, ", ", method));
			}
			if (address == null)
			{
				throw new ArgumentNullException("address");
			}
			if (method == null)
			{
				method = MapToDefaultMethod(address);
			}
			WebRequest webRequest = null;
			ClearWebClientState();
			try
			{
				m_Method = method;
				webRequest = (m_WebRequest = GetWebRequest(GetUri(address)));
				WebClientWriteStream webClientWriteStream = new WebClientWriteStream(webRequest.GetRequestStream(), webRequest, this);
				if (Logging.On)
				{
					Logging.Exit(Logging.Web, this, "OpenWrite", webClientWriteStream);
				}
				return webClientWriteStream;
			}
			catch (Exception ex)
			{
				if (ex is ThreadAbortException || ex is StackOverflowException || ex is OutOfMemoryException)
				{
					throw;
				}
				if (!(ex is WebException) && !(ex is SecurityException))
				{
					ex = new WebException(SR.GetString("net_webclient"), ex);
				}
				AbortRequest(webRequest);
				throw ex;
			}
			catch
			{
				Exception ex2 = new WebException(SR.GetString("net_webclient"), new Exception(SR.GetString("net_nonClsCompliantException")));
				AbortRequest(webRequest);
				throw ex2;
			}
			finally
			{
				CompleteWebClientState();
			}
		}

		public byte[] UploadData(string address, byte[] data)
		{
			if (address == null)
			{
				throw new ArgumentNullException("address");
			}
			return UploadData(GetUri(address), null, data);
		}

		public byte[] UploadData(Uri address, byte[] data)
		{
			return UploadData(address, null, data);
		}

		public byte[] UploadData(string address, string method, byte[] data)
		{
			if (address == null)
			{
				throw new ArgumentNullException("address");
			}
			return UploadData(GetUri(address), method, data);
		}

		public byte[] UploadData(Uri address, string method, byte[] data)
		{
			if (Logging.On)
			{
				Logging.Enter(Logging.Web, this, "UploadData", string.Concat(address, ", ", method));
			}
			if (address == null)
			{
				throw new ArgumentNullException("address");
			}
			if (data == null)
			{
				throw new ArgumentNullException("data");
			}
			if (method == null)
			{
				method = MapToDefaultMethod(address);
			}
			ClearWebClientState();
			try
			{
				WebRequest request;
				byte[] array = UploadDataInternal(address, method, data, out request);
				if (Logging.On)
				{
					Logging.Exit(Logging.Web, this, "UploadData", array);
				}
				return array;
			}
			finally
			{
				CompleteWebClientState();
			}
		}

		private byte[] UploadDataInternal(Uri address, string method, byte[] data, out WebRequest request)
		{
			request = null;
			try
			{
				m_Method = method;
				m_ContentLength = data.Length;
				request = (m_WebRequest = GetWebRequest(GetUri(address)));
				UploadBits(request, null, data, null, null, null, null);
				return DownloadBits(request, null, null, null);
			}
			catch (Exception ex)
			{
				if (ex is ThreadAbortException || ex is StackOverflowException || ex is OutOfMemoryException)
				{
					throw;
				}
				if (!(ex is WebException) && !(ex is SecurityException))
				{
					ex = new WebException(SR.GetString("net_webclient"), ex);
				}
				AbortRequest(request);
				throw ex;
			}
			catch
			{
				Exception ex2 = new WebException(SR.GetString("net_webclient"), new Exception(SR.GetString("net_nonClsCompliantException")));
				AbortRequest(request);
				throw ex2;
			}
		}

		private void OpenFileInternal(bool needsHeaderAndBoundary, string fileName, ref FileStream fs, ref byte[] buffer, ref byte[] formHeaderBytes, ref byte[] boundaryBytes)
		{
			fileName = Path.GetFullPath(fileName);
			if (m_headers == null)
			{
				m_headers = new WebHeaderCollection(WebHeaderCollectionType.WebRequest);
			}
			string text = m_headers["Content-Type"];
			if (text != null)
			{
				if (text.ToLower(CultureInfo.InvariantCulture).StartsWith("multipart/"))
				{
					throw new WebException(SR.GetString("net_webclient_Multipart"));
				}
			}
			else
			{
				text = "application/octet-stream";
			}
			fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
			int num = 8192;
			m_ContentLength = -1L;
			if (m_Method.ToUpper(CultureInfo.InvariantCulture) == "POST")
			{
				if (needsHeaderAndBoundary)
				{
					string text2 = "---------------------" + DateTime.Now.Ticks.ToString("x", NumberFormatInfo.InvariantInfo);
					m_headers["Content-Type"] = "multipart/form-data; boundary=" + text2;
					string s = "--" + text2 + "\r\nContent-Disposition: form-data; name=\"file\"; filename=\"" + Path.GetFileName(fileName) + "\"\r\nContent-Type: " + text + "\r\n\r\n";
					formHeaderBytes = Encoding.UTF8.GetBytes(s);
					boundaryBytes = Encoding.ASCII.GetBytes("\r\n--" + text2 + "--\r\n");
				}
				else
				{
					formHeaderBytes = new byte[0];
					boundaryBytes = new byte[0];
				}
				if (fs.CanSeek)
				{
					m_ContentLength = fs.Length + formHeaderBytes.Length + boundaryBytes.Length;
					num = (int)Math.Min(8192L, fs.Length);
				}
			}
			else
			{
				m_headers["Content-Type"] = text;
				formHeaderBytes = null;
				boundaryBytes = null;
				if (fs.CanSeek)
				{
					m_ContentLength = fs.Length;
					num = (int)Math.Min(8192L, fs.Length);
				}
			}
			buffer = new byte[num];
		}

		public byte[] UploadFile(string address, string fileName)
		{
			if (address == null)
			{
				throw new ArgumentNullException("address");
			}
			return UploadFile(GetUri(address), fileName);
		}

		public byte[] UploadFile(Uri address, string fileName)
		{
			return UploadFile(address, null, fileName);
		}

		public byte[] UploadFile(string address, string method, string fileName)
		{
			return UploadFile(GetUri(address), method, fileName);
		}

		public byte[] UploadFile(Uri address, string method, string fileName)
		{
			if (Logging.On)
			{
				Logging.Enter(Logging.Web, this, "UploadFile", string.Concat(address, ", ", method));
			}
			if (address == null)
			{
				throw new ArgumentNullException("address");
			}
			if (fileName == null)
			{
				throw new ArgumentNullException("fileName");
			}
			if (method == null)
			{
				method = MapToDefaultMethod(address);
			}
			FileStream fs = null;
			WebRequest request = null;
			ClearWebClientState();
			try
			{
				m_Method = method;
				byte[] formHeaderBytes = null;
				byte[] boundaryBytes = null;
				byte[] buffer = null;
				Uri uri = GetUri(address);
				bool needsHeaderAndBoundary = uri.Scheme != Uri.UriSchemeFile;
				OpenFileInternal(needsHeaderAndBoundary, fileName, ref fs, ref buffer, ref formHeaderBytes, ref boundaryBytes);
				request = (m_WebRequest = GetWebRequest(uri));
				UploadBits(request, fs, buffer, formHeaderBytes, boundaryBytes, null, null);
				byte[] array = DownloadBits(request, null, null, null);
				if (Logging.On)
				{
					Logging.Exit(Logging.Web, this, "UploadFile", array);
				}
				return array;
			}
			catch (Exception ex)
			{
				if (fs != null)
				{
					fs.Close();
					fs = null;
				}
				if (ex is ThreadAbortException || ex is StackOverflowException || ex is OutOfMemoryException)
				{
					throw;
				}
				if (!(ex is WebException) && !(ex is SecurityException))
				{
					ex = new WebException(SR.GetString("net_webclient"), ex);
				}
				AbortRequest(request);
				throw ex;
			}
			catch
			{
				if (fs != null)
				{
					fs.Close();
					fs = null;
				}
				Exception ex2 = new WebException(SR.GetString("net_webclient"), new Exception(SR.GetString("net_nonClsCompliantException")));
				AbortRequest(request);
				throw ex2;
			}
			finally
			{
				CompleteWebClientState();
			}
		}

		private byte[] UploadValuesInternal(NameValueCollection data)
		{
			if (m_headers == null)
			{
				m_headers = new WebHeaderCollection(WebHeaderCollectionType.WebRequest);
			}
			string text = m_headers["Content-Type"];
			if (text != null && string.Compare(text, "application/x-www-form-urlencoded", StringComparison.OrdinalIgnoreCase) != 0)
			{
				throw new WebException(SR.GetString("net_webclient_ContentType"));
			}
			m_headers["Content-Type"] = "application/x-www-form-urlencoded";
			string value = string.Empty;
			StringBuilder stringBuilder = new StringBuilder();
			string[] allKeys = data.AllKeys;
			foreach (string text2 in allKeys)
			{
				stringBuilder.Append(value);
				stringBuilder.Append(UrlEncode(text2));
				stringBuilder.Append("=");
				stringBuilder.Append(UrlEncode(data[text2]));
				value = "&";
			}
			byte[] bytes = Encoding.ASCII.GetBytes(stringBuilder.ToString());
			m_ContentLength = bytes.Length;
			return bytes;
		}

		public byte[] UploadValues(string address, NameValueCollection data)
		{
			if (address == null)
			{
				throw new ArgumentNullException("address");
			}
			return UploadValues(GetUri(address), null, data);
		}

		public byte[] UploadValues(Uri address, NameValueCollection data)
		{
			return UploadValues(address, null, data);
		}

		public byte[] UploadValues(string address, string method, NameValueCollection data)
		{
			if (address == null)
			{
				throw new ArgumentNullException("address");
			}
			return UploadValues(GetUri(address), method, data);
		}

		public byte[] UploadValues(Uri address, string method, NameValueCollection data)
		{
			if (Logging.On)
			{
				Logging.Enter(Logging.Web, this, "UploadValues", string.Concat(address, ", ", method));
			}
			if (address == null)
			{
				throw new ArgumentNullException("address");
			}
			if (data == null)
			{
				throw new ArgumentNullException("data");
			}
			if (method == null)
			{
				method = MapToDefaultMethod(address);
			}
			WebRequest request = null;
			ClearWebClientState();
			try
			{
				byte[] buffer = UploadValuesInternal(data);
				m_Method = method;
				request = (m_WebRequest = GetWebRequest(GetUri(address)));
				UploadBits(request, null, buffer, null, null, null, null);
				byte[] result = DownloadBits(request, null, null, null);
				if (Logging.On)
				{
					Logging.Exit(Logging.Web, this, "UploadValues", string.Concat(address, ", ", method));
				}
				return result;
			}
			catch (Exception ex)
			{
				if (ex is ThreadAbortException || ex is StackOverflowException || ex is OutOfMemoryException)
				{
					throw;
				}
				if (!(ex is WebException) && !(ex is SecurityException))
				{
					ex = new WebException(SR.GetString("net_webclient"), ex);
				}
				AbortRequest(request);
				throw ex;
			}
			catch
			{
				Exception ex2 = new WebException(SR.GetString("net_webclient"), new Exception(SR.GetString("net_nonClsCompliantException")));
				AbortRequest(request);
				throw ex2;
			}
			finally
			{
				CompleteWebClientState();
			}
		}

		public string UploadString(string address, string data)
		{
			if (address == null)
			{
				throw new ArgumentNullException("address");
			}
			return UploadString(GetUri(address), null, data);
		}

		public string UploadString(Uri address, string data)
		{
			return UploadString(address, null, data);
		}

		public string UploadString(string address, string method, string data)
		{
			if (address == null)
			{
				throw new ArgumentNullException("address");
			}
			return UploadString(GetUri(address), method, data);
		}

		public string UploadString(Uri address, string method, string data)
		{
			if (Logging.On)
			{
				Logging.Enter(Logging.Web, this, "UploadString", string.Concat(address, ", ", method));
			}
			if (address == null)
			{
				throw new ArgumentNullException("address");
			}
			if (data == null)
			{
				throw new ArgumentNullException("data");
			}
			if (method == null)
			{
				method = MapToDefaultMethod(address);
			}
			ClearWebClientState();
			try
			{
				byte[] bytes = Encoding.GetBytes(data);
				WebRequest request;
				byte[] bytes2 = UploadDataInternal(address, method, bytes, out request);
				string @string = GuessDownloadEncoding(request).GetString(bytes2);
				if (Logging.On)
				{
					Logging.Exit(Logging.Web, this, "UploadString", @string);
				}
				return @string;
			}
			finally
			{
				CompleteWebClientState();
			}
		}

		public string DownloadString(string address)
		{
			if (address == null)
			{
				throw new ArgumentNullException("address");
			}
			return DownloadString(GetUri(address));
		}

		public string DownloadString(Uri address)
		{
			if (Logging.On)
			{
				Logging.Enter(Logging.Web, this, "DownloadString", address);
			}
			if (address == null)
			{
				throw new ArgumentNullException("address");
			}
			ClearWebClientState();
			try
			{
				WebRequest request;
				byte[] bytes = DownloadDataInternal(address, out request);
				string @string = GuessDownloadEncoding(request).GetString(bytes);
				if (Logging.On)
				{
					Logging.Exit(Logging.Web, this, "DownloadString", @string);
				}
				return @string;
			}
			finally
			{
				CompleteWebClientState();
			}
		}

		private static void AbortRequest(WebRequest request)
		{
			try
			{
				request?.Abort();
			}
			catch (Exception ex)
			{
				if (ex is OutOfMemoryException || ex is StackOverflowException || ex is ThreadAbortException)
				{
					throw;
				}
			}
			catch
			{
			}
		}

		private void CopyHeadersTo(WebRequest request)
		{
			if (m_headers != null && request is HttpWebRequest)
			{
				string text = m_headers["Accept"];
				string text2 = m_headers["Connection"];
				string text3 = m_headers["Content-Type"];
				string text4 = m_headers["Expect"];
				string text5 = m_headers["Referer"];
				string text6 = m_headers["User-Agent"];
				m_headers.RemoveInternal("Accept");
				m_headers.RemoveInternal("Connection");
				m_headers.RemoveInternal("Content-Type");
				m_headers.RemoveInternal("Expect");
				m_headers.RemoveInternal("Referer");
				m_headers.RemoveInternal("User-Agent");
				request.Headers = m_headers;
				if (text != null && text.Length > 0)
				{
					((HttpWebRequest)request).Accept = text;
				}
				if (text2 != null && text2.Length > 0)
				{
					((HttpWebRequest)request).Connection = text2;
				}
				if (text3 != null && text3.Length > 0)
				{
					((HttpWebRequest)request).ContentType = text3;
				}
				if (text4 != null && text4.Length > 0)
				{
					((HttpWebRequest)request).Expect = text4;
				}
				if (text5 != null && text5.Length > 0)
				{
					((HttpWebRequest)request).Referer = text5;
				}
				if (text6 != null && text6.Length > 0)
				{
					((HttpWebRequest)request).UserAgent = text6;
				}
			}
		}

		private Uri GetUri(string path)
		{
			Uri result;
			if (m_baseAddress != null)
			{
				if (!Uri.TryCreate(m_baseAddress, path, out result))
				{
					return new Uri(Path.GetFullPath(path));
				}
			}
			else if (!Uri.TryCreate(path, UriKind.Absolute, out result))
			{
				return new Uri(Path.GetFullPath(path));
			}
			return GetUri(result);
		}

		private Uri GetUri(Uri address)
		{
			if (address == null)
			{
				throw new ArgumentNullException("address");
			}
			Uri result = address;
			if (!address.IsAbsoluteUri && m_baseAddress != null && !Uri.TryCreate(m_baseAddress, address, out result))
			{
				return address;
			}
			if ((result.Query == null || result.Query == string.Empty) && m_requestParameters != null)
			{
				StringBuilder stringBuilder = new StringBuilder();
				string str = string.Empty;
				for (int i = 0; i < m_requestParameters.Count; i++)
				{
					stringBuilder.Append(str + m_requestParameters.AllKeys[i] + "=" + m_requestParameters[i]);
					str = "&";
				}
				UriBuilder uriBuilder = new UriBuilder(result);
				uriBuilder.Query = stringBuilder.ToString();
				result = uriBuilder.Uri;
			}
			return result;
		}

		private static void DownloadBitsResponseCallback(IAsyncResult result)
		{
			DownloadBitsState downloadBitsState = (DownloadBitsState)result.AsyncState;
			WebRequest request = downloadBitsState.Request;
			Exception ex = null;
			try
			{
				WebResponse webResponse = downloadBitsState.WebClient.GetWebResponse(request, result);
				downloadBitsState.WebClient.m_WebResponse = webResponse;
				downloadBitsState.SetResponse(webResponse);
			}
			catch (Exception ex2)
			{
				if (ex2 is ThreadAbortException || ex2 is StackOverflowException || ex2 is OutOfMemoryException)
				{
					throw;
				}
				ex = ex2;
				if (!(ex2 is WebException) && !(ex2 is SecurityException))
				{
					ex = new WebException(SR.GetString("net_webclient"), ex2);
				}
				AbortRequest(request);
				if (downloadBitsState != null && downloadBitsState.WriteStream != null)
				{
					downloadBitsState.WriteStream.Close();
				}
			}
			finally
			{
				if (ex != null)
				{
					downloadBitsState.CompletionDelegate(null, ex, downloadBitsState.AsyncOp);
				}
			}
		}

		private static void DownloadBitsReadCallback(IAsyncResult result)
		{
			DownloadBitsState state = (DownloadBitsState)result.AsyncState;
			DownloadBitsReadCallbackState(state, result);
		}

		private static void DownloadBitsReadCallbackState(DownloadBitsState state, IAsyncResult result)
		{
			Stream readStream = state.ReadStream;
			Exception ex = null;
			bool flag = false;
			try
			{
				int bytesRetrieved = 0;
				if (readStream != null && readStream != Stream.Null)
				{
					bytesRetrieved = readStream.EndRead(result);
				}
				flag = state.RetrieveBytes(ref bytesRetrieved);
			}
			catch (Exception ex2)
			{
				flag = true;
				if (ex2 is ThreadAbortException || ex2 is StackOverflowException || ex2 is OutOfMemoryException)
				{
					throw;
				}
				ex = ex2;
				state.InnerBuffer = null;
				if (!(ex2 is WebException) && !(ex2 is SecurityException))
				{
					ex = new WebException(SR.GetString("net_webclient"), ex2);
				}
				AbortRequest(state.Request);
				if (state != null && state.WriteStream != null)
				{
					state.WriteStream.Close();
				}
			}
			finally
			{
				if (flag)
				{
					if (ex == null)
					{
						state.Close();
					}
					state.CompletionDelegate(state.InnerBuffer, ex, state.AsyncOp);
				}
			}
		}

		private byte[] DownloadBits(WebRequest request, Stream writeStream, CompletionDelegate completionDelegate, AsyncOperation asyncOp)
		{
			WebResponse webResponse = null;
			DownloadBitsState downloadBitsState = new DownloadBitsState(request, writeStream, completionDelegate, asyncOp, m_Progress, this);
			if (downloadBitsState.Async)
			{
				request.BeginGetResponse(DownloadBitsResponseCallback, downloadBitsState);
				return null;
			}
			int bytesRetrieved = downloadBitsState.SetResponse(m_WebResponse = GetWebResponse(request));
			while (!downloadBitsState.RetrieveBytes(ref bytesRetrieved))
			{
			}
			downloadBitsState.Close();
			return downloadBitsState.InnerBuffer;
		}

		private static void UploadBitsRequestCallback(IAsyncResult result)
		{
			UploadBitsState uploadBitsState = (UploadBitsState)result.AsyncState;
			WebRequest request = uploadBitsState.Request;
			Exception ex = null;
			try
			{
				Stream requestStream = request.EndGetRequestStream(result);
				uploadBitsState.SetRequestStream(requestStream);
			}
			catch (Exception ex2)
			{
				if (ex2 is ThreadAbortException || ex2 is StackOverflowException || ex2 is OutOfMemoryException)
				{
					throw;
				}
				ex = ex2;
				if (!(ex2 is WebException) && !(ex2 is SecurityException))
				{
					ex = new WebException(SR.GetString("net_webclient"), ex2);
				}
				AbortRequest(request);
				if (uploadBitsState != null && uploadBitsState.ReadStream != null)
				{
					uploadBitsState.ReadStream.Close();
				}
			}
			finally
			{
				if (ex != null)
				{
					uploadBitsState.CompletionDelegate(null, ex, uploadBitsState.AsyncOp);
				}
			}
		}

		private static void UploadBitsWriteCallback(IAsyncResult result)
		{
			UploadBitsState uploadBitsState = (UploadBitsState)result.AsyncState;
			Stream writeStream = uploadBitsState.WriteStream;
			Exception ex = null;
			bool flag = false;
			try
			{
				writeStream.EndWrite(result);
				flag = uploadBitsState.WriteBytes();
			}
			catch (Exception ex2)
			{
				flag = true;
				if (ex2 is ThreadAbortException || ex2 is StackOverflowException || ex2 is OutOfMemoryException)
				{
					throw;
				}
				ex = ex2;
				if (!(ex2 is WebException) && !(ex2 is SecurityException))
				{
					ex = new WebException(SR.GetString("net_webclient"), ex2);
				}
				AbortRequest(uploadBitsState.Request);
				if (uploadBitsState != null && uploadBitsState.ReadStream != null)
				{
					uploadBitsState.ReadStream.Close();
				}
			}
			finally
			{
				if (flag)
				{
					if (ex == null)
					{
						uploadBitsState.Close();
					}
					uploadBitsState.CompletionDelegate(null, ex, uploadBitsState.AsyncOp);
				}
			}
		}

		private void UploadBits(WebRequest request, Stream readStream, byte[] buffer, byte[] header, byte[] footer, CompletionDelegate completionDelegate, AsyncOperation asyncOp)
		{
			if (request.RequestUri.Scheme == Uri.UriSchemeFile)
			{
				header = (footer = null);
			}
			UploadBitsState uploadBitsState = new UploadBitsState(request, readStream, buffer, header, footer, completionDelegate, asyncOp, m_Progress, this);
			if (uploadBitsState.Async)
			{
				request.BeginGetRequestStream(UploadBitsRequestCallback, uploadBitsState);
				return;
			}
			Stream requestStream = request.GetRequestStream();
			uploadBitsState.SetRequestStream(requestStream);
			while (!uploadBitsState.WriteBytes())
			{
			}
			uploadBitsState.Close();
		}

		private Encoding GuessDownloadEncoding(WebRequest request)
		{
			try
			{
				string contentType;
				if ((contentType = request.ContentType) == null)
				{
					return Encoding;
				}
				contentType = contentType.ToLower(CultureInfo.InvariantCulture);
				string[] array = contentType.Split(';', '=', ' ');
				bool flag = false;
				string[] array2 = array;
				foreach (string text in array2)
				{
					if (text == "charset")
					{
						flag = true;
					}
					else if (flag)
					{
						return Encoding.GetEncoding(text);
					}
				}
			}
			catch (Exception ex)
			{
				if (ex is ThreadAbortException || ex is StackOverflowException || ex is OutOfMemoryException)
				{
					throw;
				}
			}
			catch
			{
			}
			return Encoding;
		}

		private string MapToDefaultMethod(Uri address)
		{
			Uri uri = ((address.IsAbsoluteUri || !(m_baseAddress != null)) ? address : new Uri(m_baseAddress, address));
			if (uri.Scheme.ToLower(CultureInfo.InvariantCulture) == "ftp")
			{
				return "STOR";
			}
			return "POST";
		}

		private static string UrlEncode(string str)
		{
			if (str == null)
			{
				return null;
			}
			return UrlEncode(str, Encoding.UTF8);
		}

		private static string UrlEncode(string str, Encoding e)
		{
			if (str == null)
			{
				return null;
			}
			return Encoding.ASCII.GetString(UrlEncodeToBytes(str, e));
		}

		private static byte[] UrlEncodeToBytes(string str, Encoding e)
		{
			if (str == null)
			{
				return null;
			}
			byte[] bytes = e.GetBytes(str);
			return UrlEncodeBytesToBytesInternal(bytes, 0, bytes.Length, alwaysCreateReturnValue: false);
		}

		private static byte[] UrlEncodeBytesToBytesInternal(byte[] bytes, int offset, int count, bool alwaysCreateReturnValue)
		{
			int num = 0;
			int num2 = 0;
			for (int i = 0; i < count; i++)
			{
				char c = (char)bytes[offset + i];
				if (c == ' ')
				{
					num++;
				}
				else if (!IsSafe(c))
				{
					num2++;
				}
			}
			if (!alwaysCreateReturnValue && num == 0 && num2 == 0)
			{
				return bytes;
			}
			byte[] array = new byte[count + num2 * 2];
			int num3 = 0;
			for (int j = 0; j < count; j++)
			{
				byte b = bytes[offset + j];
				char c2 = (char)b;
				if (IsSafe(c2))
				{
					array[num3++] = b;
					continue;
				}
				if (c2 == ' ')
				{
					array[num3++] = 43;
					continue;
				}
				array[num3++] = 37;
				array[num3++] = (byte)IntToHex((b >> 4) & 0xF);
				array[num3++] = (byte)IntToHex(b & 0xF);
			}
			return array;
		}

		private static char IntToHex(int n)
		{
			if (n <= 9)
			{
				return (char)(n + 48);
			}
			return (char)(n - 10 + 97);
		}

		private static bool IsSafe(char ch)
		{
			if ((ch >= 'a' && ch <= 'z') || (ch >= 'A' && ch <= 'Z') || (ch >= '0' && ch <= '9'))
			{
				return true;
			}
			switch (ch)
			{
			case '!':
			case '\'':
			case '(':
			case ')':
			case '*':
			case '-':
			case '.':
			case '_':
				return true;
			default:
				return false;
			}
		}

		private void InvokeOperationCompleted(AsyncOperation asyncOp, SendOrPostCallback callback, AsyncCompletedEventArgs eventArgs)
		{
			if (Interlocked.CompareExchange(ref m_AsyncOp, null, asyncOp) == asyncOp)
			{
				CompleteWebClientState();
				asyncOp.PostOperationCompleted(callback, eventArgs);
			}
		}

		private bool AnotherCallInProgress(int callNesting)
		{
			return callNesting > 1;
		}

		protected virtual void OnOpenReadCompleted(OpenReadCompletedEventArgs e)
		{
			if (this.OpenReadCompleted != null)
			{
				this.OpenReadCompleted(this, e);
			}
		}

		private void OpenReadOperationCompleted(object arg)
		{
			OnOpenReadCompleted((OpenReadCompletedEventArgs)arg);
		}

		private void OpenReadAsyncCallback(IAsyncResult result)
		{
			LazyAsyncResult lazyAsyncResult = (LazyAsyncResult)result;
			AsyncOperation asyncOperation = (AsyncOperation)lazyAsyncResult.AsyncState;
			WebRequest request = (WebRequest)lazyAsyncResult.AsyncObject;
			Stream result2 = null;
			Exception exception = null;
			try
			{
				result2 = (m_WebResponse = GetWebResponse(request, result)).GetResponseStream();
			}
			catch (Exception ex)
			{
				if (ex is ThreadAbortException || ex is StackOverflowException || ex is OutOfMemoryException)
				{
					throw;
				}
				exception = ex;
				if (!(ex is WebException) && !(ex is SecurityException))
				{
					exception = new WebException(SR.GetString("net_webclient"), ex);
				}
			}
			catch
			{
				exception = new WebException(SR.GetString("net_webclient"), new Exception(SR.GetString("net_nonClsCompliantException")));
			}
			OpenReadCompletedEventArgs eventArgs = new OpenReadCompletedEventArgs(result2, exception, m_Cancelled, asyncOperation.UserSuppliedState);
			InvokeOperationCompleted(asyncOperation, openReadOperationCompleted, eventArgs);
		}

		[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
		public void OpenReadAsync(Uri address)
		{
			OpenReadAsync(address, null);
		}

		[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
		public void OpenReadAsync(Uri address, object userToken)
		{
			if (Logging.On)
			{
				Logging.Enter(Logging.Web, this, "OpenReadAsync", address);
			}
			if (address == null)
			{
				throw new ArgumentNullException("address");
			}
			InitWebClientAsync();
			ClearWebClientState();
			AsyncOperation asyncOperation = (m_AsyncOp = AsyncOperationManager.CreateOperation(userToken));
			try
			{
				(m_WebRequest = GetWebRequest(GetUri(address))).BeginGetResponse(OpenReadAsyncCallback, asyncOperation);
			}
			catch (Exception ex)
			{
				if (ex is ThreadAbortException || ex is StackOverflowException || ex is OutOfMemoryException)
				{
					throw;
				}
				if (!(ex is WebException) && !(ex is SecurityException))
				{
					ex = new WebException(SR.GetString("net_webclient"), ex);
				}
				OpenReadCompletedEventArgs eventArgs = new OpenReadCompletedEventArgs(null, ex, m_Cancelled, asyncOperation.UserSuppliedState);
				InvokeOperationCompleted(asyncOperation, openReadOperationCompleted, eventArgs);
			}
			catch
			{
				Exception exception = new WebException(SR.GetString("net_webclient"), new Exception(SR.GetString("net_nonClsCompliantException")));
				OpenReadCompletedEventArgs eventArgs2 = new OpenReadCompletedEventArgs(null, exception, m_Cancelled, asyncOperation.UserSuppliedState);
				InvokeOperationCompleted(asyncOperation, openReadOperationCompleted, eventArgs2);
			}
			if (Logging.On)
			{
				Logging.Exit(Logging.Web, this, "OpenReadAsync", null);
			}
		}

		protected virtual void OnOpenWriteCompleted(OpenWriteCompletedEventArgs e)
		{
			if (this.OpenWriteCompleted != null)
			{
				this.OpenWriteCompleted(this, e);
			}
		}

		private void OpenWriteOperationCompleted(object arg)
		{
			OnOpenWriteCompleted((OpenWriteCompletedEventArgs)arg);
		}

		private void OpenWriteAsyncCallback(IAsyncResult result)
		{
			LazyAsyncResult lazyAsyncResult = (LazyAsyncResult)result;
			AsyncOperation asyncOperation = (AsyncOperation)lazyAsyncResult.AsyncState;
			WebRequest webRequest = (WebRequest)lazyAsyncResult.AsyncObject;
			WebClientWriteStream result2 = null;
			Exception exception = null;
			try
			{
				result2 = new WebClientWriteStream(webRequest.EndGetRequestStream(result), webRequest, this);
			}
			catch (Exception ex)
			{
				if (ex is ThreadAbortException || ex is StackOverflowException || ex is OutOfMemoryException)
				{
					throw;
				}
				exception = ex;
				if (!(ex is WebException) && !(ex is SecurityException))
				{
					exception = new WebException(SR.GetString("net_webclient"), ex);
				}
			}
			catch
			{
				exception = new WebException(SR.GetString("net_webclient"), new Exception(SR.GetString("net_nonClsCompliantException")));
			}
			OpenWriteCompletedEventArgs eventArgs = new OpenWriteCompletedEventArgs(result2, exception, m_Cancelled, asyncOperation.UserSuppliedState);
			InvokeOperationCompleted(asyncOperation, openWriteOperationCompleted, eventArgs);
		}

		[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
		public void OpenWriteAsync(Uri address)
		{
			OpenWriteAsync(address, null, null);
		}

		[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
		public void OpenWriteAsync(Uri address, string method)
		{
			OpenWriteAsync(address, method, null);
		}

		[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
		public void OpenWriteAsync(Uri address, string method, object userToken)
		{
			if (Logging.On)
			{
				Logging.Enter(Logging.Web, this, "OpenWriteAsync", string.Concat(address, ", ", method));
			}
			if (address == null)
			{
				throw new ArgumentNullException("address");
			}
			if (method == null)
			{
				method = MapToDefaultMethod(address);
			}
			InitWebClientAsync();
			ClearWebClientState();
			AsyncOperation asyncOperation = (m_AsyncOp = AsyncOperationManager.CreateOperation(userToken));
			try
			{
				m_Method = method;
				(m_WebRequest = GetWebRequest(GetUri(address))).BeginGetRequestStream(OpenWriteAsyncCallback, asyncOperation);
			}
			catch (Exception ex)
			{
				if (ex is ThreadAbortException || ex is StackOverflowException || ex is OutOfMemoryException)
				{
					throw;
				}
				if (!(ex is WebException) && !(ex is SecurityException))
				{
					ex = new WebException(SR.GetString("net_webclient"), ex);
				}
				OpenWriteCompletedEventArgs eventArgs = new OpenWriteCompletedEventArgs(null, ex, m_Cancelled, asyncOperation.UserSuppliedState);
				InvokeOperationCompleted(asyncOperation, openWriteOperationCompleted, eventArgs);
			}
			catch
			{
				Exception exception = new WebException(SR.GetString("net_webclient"), new Exception(SR.GetString("net_nonClsCompliantException")));
				OpenWriteCompletedEventArgs eventArgs2 = new OpenWriteCompletedEventArgs(null, exception, m_Cancelled, asyncOperation.UserSuppliedState);
				InvokeOperationCompleted(asyncOperation, openWriteOperationCompleted, eventArgs2);
			}
			if (Logging.On)
			{
				Logging.Exit(Logging.Web, this, "OpenWriteAsync", null);
			}
		}

		protected virtual void OnDownloadStringCompleted(DownloadStringCompletedEventArgs e)
		{
			if (this.DownloadStringCompleted != null)
			{
				this.DownloadStringCompleted(this, e);
			}
		}

		private void DownloadStringOperationCompleted(object arg)
		{
			OnDownloadStringCompleted((DownloadStringCompletedEventArgs)arg);
		}

		private void DownloadStringAsyncCallback(byte[] returnBytes, Exception exception, AsyncOperation asyncOp)
		{
			string result = null;
			try
			{
				if (returnBytes != null)
				{
					result = GuessDownloadEncoding(m_WebRequest).GetString(returnBytes);
				}
			}
			catch (Exception ex)
			{
				if (ex is ThreadAbortException || ex is StackOverflowException || ex is OutOfMemoryException)
				{
					throw;
				}
				exception = ex;
			}
			catch
			{
				exception = new Exception(SR.GetString("net_nonClsCompliantException"));
			}
			DownloadStringCompletedEventArgs eventArgs = new DownloadStringCompletedEventArgs(result, exception, m_Cancelled, asyncOp.UserSuppliedState);
			InvokeOperationCompleted(asyncOp, downloadStringOperationCompleted, eventArgs);
		}

		[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
		public void DownloadStringAsync(Uri address)
		{
			DownloadStringAsync(address, null);
		}

		[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
		public void DownloadStringAsync(Uri address, object userToken)
		{
			if (Logging.On)
			{
				Logging.Enter(Logging.Web, this, "DownloadStringAsync", address);
			}
			if (address == null)
			{
				throw new ArgumentNullException("address");
			}
			InitWebClientAsync();
			ClearWebClientState();
			AsyncOperation asyncOp = (m_AsyncOp = AsyncOperationManager.CreateOperation(userToken));
			try
			{
				DownloadBits(m_WebRequest = GetWebRequest(GetUri(address)), null, DownloadStringAsyncCallback, asyncOp);
			}
			catch (Exception ex)
			{
				if (ex is ThreadAbortException || ex is StackOverflowException || ex is OutOfMemoryException)
				{
					throw;
				}
				if (!(ex is WebException) && !(ex is SecurityException))
				{
					ex = new WebException(SR.GetString("net_webclient"), ex);
				}
				DownloadStringAsyncCallback(null, ex, asyncOp);
			}
			catch
			{
				Exception exception = new WebException(SR.GetString("net_webclient"), new Exception(SR.GetString("net_nonClsCompliantException")));
				DownloadStringAsyncCallback(null, exception, asyncOp);
			}
			if (Logging.On)
			{
				Logging.Exit(Logging.Web, this, "DownloadStringAsync", "");
			}
		}

		protected virtual void OnDownloadDataCompleted(DownloadDataCompletedEventArgs e)
		{
			if (this.DownloadDataCompleted != null)
			{
				this.DownloadDataCompleted(this, e);
			}
		}

		private void DownloadDataOperationCompleted(object arg)
		{
			OnDownloadDataCompleted((DownloadDataCompletedEventArgs)arg);
		}

		private void DownloadDataAsyncCallback(byte[] returnBytes, Exception exception, AsyncOperation asyncOp)
		{
			DownloadDataCompletedEventArgs eventArgs = new DownloadDataCompletedEventArgs(returnBytes, exception, m_Cancelled, asyncOp.UserSuppliedState);
			InvokeOperationCompleted(asyncOp, downloadDataOperationCompleted, eventArgs);
		}

		[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
		public void DownloadDataAsync(Uri address)
		{
			DownloadDataAsync(address, null);
		}

		[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
		public void DownloadDataAsync(Uri address, object userToken)
		{
			if (Logging.On)
			{
				Logging.Enter(Logging.Web, this, "DownloadDataAsync", address);
			}
			if (address == null)
			{
				throw new ArgumentNullException("address");
			}
			InitWebClientAsync();
			ClearWebClientState();
			AsyncOperation asyncOp = (m_AsyncOp = AsyncOperationManager.CreateOperation(userToken));
			try
			{
				DownloadBits(m_WebRequest = GetWebRequest(GetUri(address)), null, DownloadDataAsyncCallback, asyncOp);
			}
			catch (Exception ex)
			{
				if (ex is ThreadAbortException || ex is StackOverflowException || ex is OutOfMemoryException)
				{
					throw;
				}
				if (!(ex is WebException) && !(ex is SecurityException))
				{
					ex = new WebException(SR.GetString("net_webclient"), ex);
				}
				DownloadDataAsyncCallback(null, ex, asyncOp);
			}
			catch
			{
				Exception exception = new WebException(SR.GetString("net_webclient"), new Exception(SR.GetString("net_nonClsCompliantException")));
				DownloadDataAsyncCallback(null, exception, asyncOp);
			}
			if (Logging.On)
			{
				Logging.Exit(Logging.Web, this, "DownloadDataAsync", null);
			}
		}

		protected virtual void OnDownloadFileCompleted(AsyncCompletedEventArgs e)
		{
			if (this.DownloadFileCompleted != null)
			{
				this.DownloadFileCompleted(this, e);
			}
		}

		private void DownloadFileOperationCompleted(object arg)
		{
			OnDownloadFileCompleted((AsyncCompletedEventArgs)arg);
		}

		private void DownloadFileAsyncCallback(byte[] returnBytes, Exception exception, AsyncOperation asyncOp)
		{
			AsyncCompletedEventArgs eventArgs = new AsyncCompletedEventArgs(exception, m_Cancelled, asyncOp.UserSuppliedState);
			InvokeOperationCompleted(asyncOp, downloadFileOperationCompleted, eventArgs);
		}

		[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
		public void DownloadFileAsync(Uri address, string fileName)
		{
			DownloadFileAsync(address, fileName, null);
		}

		[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
		public void DownloadFileAsync(Uri address, string fileName, object userToken)
		{
			if (Logging.On)
			{
				Logging.Enter(Logging.Web, this, "DownloadFileAsync", address);
			}
			if (address == null)
			{
				throw new ArgumentNullException("address");
			}
			if (fileName == null)
			{
				throw new ArgumentNullException("fileName");
			}
			FileStream fileStream = null;
			InitWebClientAsync();
			ClearWebClientState();
			AsyncOperation asyncOp = (m_AsyncOp = AsyncOperationManager.CreateOperation(userToken));
			try
			{
				fileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write);
				DownloadBits(m_WebRequest = GetWebRequest(GetUri(address)), fileStream, DownloadFileAsyncCallback, asyncOp);
			}
			catch (Exception ex)
			{
				if (ex is ThreadAbortException || ex is StackOverflowException || ex is OutOfMemoryException)
				{
					throw;
				}
				fileStream?.Close();
				if (!(ex is WebException) && !(ex is SecurityException))
				{
					ex = new WebException(SR.GetString("net_webclient"), ex);
				}
				DownloadFileAsyncCallback(null, ex, asyncOp);
			}
			if (Logging.On)
			{
				Logging.Exit(Logging.Web, this, "DownloadFileAsync", null);
			}
		}

		protected virtual void OnUploadStringCompleted(UploadStringCompletedEventArgs e)
		{
			if (this.UploadStringCompleted != null)
			{
				this.UploadStringCompleted(this, e);
			}
		}

		private void UploadStringOperationCompleted(object arg)
		{
			OnUploadStringCompleted((UploadStringCompletedEventArgs)arg);
		}

		private void UploadStringAsyncWriteCallback(byte[] returnBytes, Exception exception, AsyncOperation asyncOp)
		{
			if (exception != null)
			{
				UploadStringCompletedEventArgs eventArgs = new UploadStringCompletedEventArgs(null, exception, m_Cancelled, asyncOp.UserSuppliedState);
				InvokeOperationCompleted(asyncOp, uploadStringOperationCompleted, eventArgs);
			}
		}

		private void UploadStringAsyncReadCallback(byte[] returnBytes, Exception exception, AsyncOperation asyncOp)
		{
			string result = null;
			try
			{
				if (returnBytes != null)
				{
					result = GuessDownloadEncoding(m_WebRequest).GetString(returnBytes);
				}
			}
			catch (Exception ex)
			{
				if (ex is ThreadAbortException || ex is StackOverflowException || ex is OutOfMemoryException)
				{
					throw;
				}
				exception = ex;
			}
			catch
			{
				exception = new Exception(SR.GetString("net_nonClsCompliantException"));
			}
			UploadStringCompletedEventArgs eventArgs = new UploadStringCompletedEventArgs(result, exception, m_Cancelled, asyncOp.UserSuppliedState);
			InvokeOperationCompleted(asyncOp, uploadStringOperationCompleted, eventArgs);
		}

		[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
		public void UploadStringAsync(Uri address, string data)
		{
			UploadStringAsync(address, null, data, null);
		}

		[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
		public void UploadStringAsync(Uri address, string method, string data)
		{
			UploadStringAsync(address, method, data, null);
		}

		[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
		public void UploadStringAsync(Uri address, string method, string data, object userToken)
		{
			if (Logging.On)
			{
				Logging.Enter(Logging.Web, this, "UploadStringAsync", address);
			}
			if (address == null)
			{
				throw new ArgumentNullException("address");
			}
			if (data == null)
			{
				throw new ArgumentNullException("data");
			}
			if (method == null)
			{
				method = MapToDefaultMethod(address);
			}
			InitWebClientAsync();
			ClearWebClientState();
			AsyncOperation asyncOp = (m_AsyncOp = AsyncOperationManager.CreateOperation(userToken));
			try
			{
				byte[] bytes = Encoding.GetBytes(data);
				m_Method = method;
				m_ContentLength = bytes.Length;
				WebRequest request = (m_WebRequest = GetWebRequest(GetUri(address)));
				UploadBits(request, null, bytes, null, null, UploadStringAsyncWriteCallback, asyncOp);
				DownloadBits(request, null, UploadStringAsyncReadCallback, asyncOp);
			}
			catch (Exception ex)
			{
				if (ex is ThreadAbortException || ex is StackOverflowException || ex is OutOfMemoryException)
				{
					throw;
				}
				if (!(ex is WebException) && !(ex is SecurityException))
				{
					ex = new WebException(SR.GetString("net_webclient"), ex);
				}
				UploadStringAsyncWriteCallback(null, ex, asyncOp);
			}
			catch
			{
				Exception exception = new WebException(SR.GetString("net_webclient"), new Exception(SR.GetString("net_nonClsCompliantException")));
				UploadStringAsyncWriteCallback(null, exception, asyncOp);
			}
			if (Logging.On)
			{
				Logging.Exit(Logging.Web, this, "UploadStringAsync", null);
			}
		}

		protected virtual void OnUploadDataCompleted(UploadDataCompletedEventArgs e)
		{
			if (this.UploadDataCompleted != null)
			{
				this.UploadDataCompleted(this, e);
			}
		}

		private void UploadDataOperationCompleted(object arg)
		{
			OnUploadDataCompleted((UploadDataCompletedEventArgs)arg);
		}

		private void UploadDataAsyncWriteCallback(byte[] returnBytes, Exception exception, AsyncOperation asyncOp)
		{
			if (exception != null)
			{
				UploadDataCompletedEventArgs eventArgs = new UploadDataCompletedEventArgs(returnBytes, exception, m_Cancelled, asyncOp.UserSuppliedState);
				InvokeOperationCompleted(asyncOp, uploadDataOperationCompleted, eventArgs);
			}
		}

		private void UploadDataAsyncReadCallback(byte[] returnBytes, Exception exception, AsyncOperation asyncOp)
		{
			UploadDataCompletedEventArgs eventArgs = new UploadDataCompletedEventArgs(returnBytes, exception, m_Cancelled, asyncOp.UserSuppliedState);
			InvokeOperationCompleted(asyncOp, uploadDataOperationCompleted, eventArgs);
		}

		[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
		public void UploadDataAsync(Uri address, byte[] data)
		{
			UploadDataAsync(address, null, data, null);
		}

		[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
		public void UploadDataAsync(Uri address, string method, byte[] data)
		{
			UploadDataAsync(address, method, data, null);
		}

		[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
		public void UploadDataAsync(Uri address, string method, byte[] data, object userToken)
		{
			if (Logging.On)
			{
				Logging.Enter(Logging.Web, this, "UploadDataAsync", string.Concat(address, ", ", method));
			}
			if (address == null)
			{
				throw new ArgumentNullException("address");
			}
			if (data == null)
			{
				throw new ArgumentNullException("data");
			}
			if (method == null)
			{
				method = MapToDefaultMethod(address);
			}
			InitWebClientAsync();
			ClearWebClientState();
			AsyncOperation asyncOp = (m_AsyncOp = AsyncOperationManager.CreateOperation(userToken));
			try
			{
				m_Method = method;
				m_ContentLength = data.Length;
				WebRequest request = (m_WebRequest = GetWebRequest(GetUri(address)));
				UploadBits(request, null, data, null, null, UploadDataAsyncWriteCallback, asyncOp);
				DownloadBits(request, null, UploadDataAsyncReadCallback, asyncOp);
			}
			catch (Exception ex)
			{
				if (ex is ThreadAbortException || ex is StackOverflowException || ex is OutOfMemoryException)
				{
					throw;
				}
				if (!(ex is WebException) && !(ex is SecurityException))
				{
					ex = new WebException(SR.GetString("net_webclient"), ex);
				}
				UploadDataAsyncWriteCallback(null, ex, asyncOp);
			}
			catch
			{
				Exception exception = new WebException(SR.GetString("net_webclient"), new Exception(SR.GetString("net_nonClsCompliantException")));
				UploadDataAsyncWriteCallback(null, exception, asyncOp);
			}
			if (Logging.On)
			{
				Logging.Exit(Logging.Web, this, "UploadDataAsync", null);
			}
		}

		protected virtual void OnUploadFileCompleted(UploadFileCompletedEventArgs e)
		{
			if (this.UploadFileCompleted != null)
			{
				this.UploadFileCompleted(this, e);
			}
		}

		private void UploadFileOperationCompleted(object arg)
		{
			OnUploadFileCompleted((UploadFileCompletedEventArgs)arg);
		}

		private void UploadFileAsyncWriteCallback(byte[] returnBytes, Exception exception, AsyncOperation asyncOp)
		{
			if (exception != null)
			{
				UploadFileCompletedEventArgs eventArgs = new UploadFileCompletedEventArgs(returnBytes, exception, m_Cancelled, asyncOp.UserSuppliedState);
				InvokeOperationCompleted(asyncOp, uploadFileOperationCompleted, eventArgs);
			}
		}

		private void UploadFileAsyncReadCallback(byte[] returnBytes, Exception exception, AsyncOperation asyncOp)
		{
			UploadFileCompletedEventArgs eventArgs = new UploadFileCompletedEventArgs(returnBytes, exception, m_Cancelled, asyncOp.UserSuppliedState);
			InvokeOperationCompleted(asyncOp, uploadFileOperationCompleted, eventArgs);
		}

		[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
		public void UploadFileAsync(Uri address, string fileName)
		{
			UploadFileAsync(address, null, fileName, null);
		}

		[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
		public void UploadFileAsync(Uri address, string method, string fileName)
		{
			UploadFileAsync(address, method, fileName, null);
		}

		[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
		public void UploadFileAsync(Uri address, string method, string fileName, object userToken)
		{
			if (Logging.On)
			{
				Logging.Enter(Logging.Web, this, "UploadFileAsync", string.Concat(address, ", ", method));
			}
			if (address == null)
			{
				throw new ArgumentNullException("address");
			}
			if (fileName == null)
			{
				throw new ArgumentNullException("fileName");
			}
			if (method == null)
			{
				method = MapToDefaultMethod(address);
			}
			InitWebClientAsync();
			ClearWebClientState();
			AsyncOperation asyncOp = (m_AsyncOp = AsyncOperationManager.CreateOperation(userToken));
			FileStream fs = null;
			try
			{
				m_Method = method;
				byte[] formHeaderBytes = null;
				byte[] boundaryBytes = null;
				byte[] buffer = null;
				Uri uri = GetUri(address);
				bool needsHeaderAndBoundary = uri.Scheme != Uri.UriSchemeFile;
				OpenFileInternal(needsHeaderAndBoundary, fileName, ref fs, ref buffer, ref formHeaderBytes, ref boundaryBytes);
				WebRequest request = (m_WebRequest = GetWebRequest(uri));
				UploadBits(request, fs, buffer, formHeaderBytes, boundaryBytes, UploadFileAsyncWriteCallback, asyncOp);
				DownloadBits(request, null, UploadFileAsyncReadCallback, asyncOp);
			}
			catch (Exception ex)
			{
				if (ex is ThreadAbortException || ex is StackOverflowException || ex is OutOfMemoryException)
				{
					throw;
				}
				fs?.Close();
				if (!(ex is WebException) && !(ex is SecurityException))
				{
					ex = new WebException(SR.GetString("net_webclient"), ex);
				}
				UploadFileAsyncWriteCallback(null, ex, asyncOp);
			}
			if (Logging.On)
			{
				Logging.Exit(Logging.Web, this, "UploadFileAsync", null);
			}
		}

		protected virtual void OnUploadValuesCompleted(UploadValuesCompletedEventArgs e)
		{
			if (this.UploadValuesCompleted != null)
			{
				this.UploadValuesCompleted(this, e);
			}
		}

		private void UploadValuesOperationCompleted(object arg)
		{
			OnUploadValuesCompleted((UploadValuesCompletedEventArgs)arg);
		}

		private void UploadValuesAsyncWriteCallback(byte[] returnBytes, Exception exception, AsyncOperation asyncOp)
		{
			if (exception != null)
			{
				UploadValuesCompletedEventArgs eventArgs = new UploadValuesCompletedEventArgs(returnBytes, exception, m_Cancelled, asyncOp.UserSuppliedState);
				InvokeOperationCompleted(asyncOp, uploadValuesOperationCompleted, eventArgs);
			}
		}

		private void UploadValuesAsyncReadCallback(byte[] returnBytes, Exception exception, AsyncOperation asyncOp)
		{
			UploadValuesCompletedEventArgs eventArgs = new UploadValuesCompletedEventArgs(returnBytes, exception, m_Cancelled, asyncOp.UserSuppliedState);
			InvokeOperationCompleted(asyncOp, uploadValuesOperationCompleted, eventArgs);
		}

		[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
		public void UploadValuesAsync(Uri address, NameValueCollection data)
		{
			UploadValuesAsync(address, null, data, null);
		}

		[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
		public void UploadValuesAsync(Uri address, string method, NameValueCollection data)
		{
			UploadValuesAsync(address, method, data, null);
		}

		[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
		public void UploadValuesAsync(Uri address, string method, NameValueCollection data, object userToken)
		{
			if (Logging.On)
			{
				Logging.Enter(Logging.Web, this, "UploadValuesAsync", string.Concat(address, ", ", method));
			}
			if (address == null)
			{
				throw new ArgumentNullException("address");
			}
			if (data == null)
			{
				throw new ArgumentNullException("data");
			}
			if (method == null)
			{
				method = MapToDefaultMethod(address);
			}
			InitWebClientAsync();
			ClearWebClientState();
			AsyncOperation asyncOp = (m_AsyncOp = AsyncOperationManager.CreateOperation(userToken));
			try
			{
				byte[] buffer = UploadValuesInternal(data);
				m_Method = method;
				WebRequest request = (m_WebRequest = GetWebRequest(GetUri(address)));
				UploadBits(request, null, buffer, null, null, UploadValuesAsyncWriteCallback, asyncOp);
				DownloadBits(request, null, UploadValuesAsyncReadCallback, asyncOp);
			}
			catch (Exception ex)
			{
				if (ex is ThreadAbortException || ex is StackOverflowException || ex is OutOfMemoryException)
				{
					throw;
				}
				if (!(ex is WebException) && !(ex is SecurityException))
				{
					ex = new WebException(SR.GetString("net_webclient"), ex);
				}
				UploadValuesAsyncWriteCallback(null, ex, asyncOp);
			}
			catch
			{
				Exception exception = new WebException(SR.GetString("net_webclient"), new Exception(SR.GetString("net_nonClsCompliantException")));
				UploadValuesAsyncWriteCallback(null, exception, asyncOp);
			}
			if (Logging.On)
			{
				Logging.Exit(Logging.Web, this, "UploadValuesAsync", null);
			}
		}

		public void CancelAsync()
		{
			WebRequest webRequest = m_WebRequest;
			m_Cancelled = true;
			AbortRequest(webRequest);
		}

		protected virtual void OnDownloadProgressChanged(DownloadProgressChangedEventArgs e)
		{
			if (this.DownloadProgressChanged != null)
			{
				this.DownloadProgressChanged(this, e);
			}
		}

		protected virtual void OnUploadProgressChanged(UploadProgressChangedEventArgs e)
		{
			if (this.UploadProgressChanged != null)
			{
				this.UploadProgressChanged(this, e);
			}
		}

		private void ReportDownloadProgressChanged(object arg)
		{
			OnDownloadProgressChanged((DownloadProgressChangedEventArgs)arg);
		}

		private void ReportUploadProgressChanged(object arg)
		{
			OnUploadProgressChanged((UploadProgressChangedEventArgs)arg);
		}

		private void PostProgressChanged(AsyncOperation asyncOp, ProgressData progress)
		{
			if (asyncOp != null && progress.BytesSent + progress.BytesReceived > 0)
			{
				if (progress.HasUploadPhase)
				{
					asyncOp.Post(arg: new UploadProgressChangedEventArgs((int)((progress.TotalBytesToReceive >= 0 || progress.BytesReceived != 0) ? ((progress.TotalBytesToSend < 0) ? 50 : ((progress.TotalBytesToReceive == 0) ? 100 : (50 * progress.BytesReceived / progress.TotalBytesToReceive + 50))) : ((progress.TotalBytesToSend >= 0) ? ((progress.TotalBytesToSend == 0) ? 50 : (50 * progress.BytesSent / progress.TotalBytesToSend)) : 0)), asyncOp.UserSuppliedState, progress.BytesSent, progress.TotalBytesToSend, progress.BytesReceived, progress.TotalBytesToReceive), d: reportUploadProgressChanged);
					return;
				}
				int progressPercentage = (int)((progress.TotalBytesToReceive >= 0) ? ((progress.TotalBytesToReceive == 0) ? 100 : (100 * progress.BytesReceived / progress.TotalBytesToReceive)) : 0);
				asyncOp.Post(reportDownloadProgressChanged, new DownloadProgressChangedEventArgs(progressPercentage, asyncOp.UserSuppliedState, progress.BytesReceived, progress.TotalBytesToReceive));
			}
		}
	}
}
