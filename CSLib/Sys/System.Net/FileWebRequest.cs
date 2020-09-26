using System.IO;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Threading;

namespace System.Net
{
	[Serializable]
	public class FileWebRequest : WebRequest, ISerializable
	{
		private static WaitCallback s_GetRequestStreamCallback = GetRequestStreamCallback;

		private static WaitCallback s_GetResponseCallback = GetResponseCallback;

		private static ContextCallback s_WrappedGetRequestStreamCallback = GetRequestStreamCallback;

		private static ContextCallback s_WrappedResponseCallback = GetResponseCallback;

		private string m_connectionGroupName;

		private long m_contentLength;

		private ICredentials m_credentials;

		private FileAccess m_fileAccess;

		private WebHeaderCollection m_headers;

		private string m_method = "GET";

		private bool m_preauthenticate;

		private IWebProxy m_proxy;

		private ManualResetEvent m_readerEvent;

		private bool m_readPending;

		private WebResponse m_response;

		private Stream m_stream;

		private bool m_syncHint;

		private int m_timeout = 100000;

		private Uri m_uri;

		private bool m_writePending;

		private bool m_writing;

		private LazyAsyncResult m_WriteAResult;

		private LazyAsyncResult m_ReadAResult;

		private int m_Aborted;

		internal bool Aborted => m_Aborted != 0;

		public override string ConnectionGroupName
		{
			get
			{
				return m_connectionGroupName;
			}
			set
			{
				m_connectionGroupName = value;
			}
		}

		public override long ContentLength
		{
			get
			{
				return m_contentLength;
			}
			set
			{
				if (value < 0)
				{
					throw new ArgumentException(SR.GetString("net_clsmall"), "value");
				}
				m_contentLength = value;
			}
		}

		public override string ContentType
		{
			get
			{
				return m_headers["Content-Type"];
			}
			set
			{
				m_headers["Content-Type"] = value;
			}
		}

		public override ICredentials Credentials
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

		public override WebHeaderCollection Headers => m_headers;

		public override string Method
		{
			get
			{
				return m_method;
			}
			set
			{
				if (ValidationHelper.IsBlankString(value))
				{
					throw new ArgumentException(SR.GetString("net_badmethod"), "value");
				}
				m_method = value;
			}
		}

		public override bool PreAuthenticate
		{
			get
			{
				return m_preauthenticate;
			}
			set
			{
				m_preauthenticate = true;
			}
		}

		public override IWebProxy Proxy
		{
			get
			{
				return m_proxy;
			}
			set
			{
				m_proxy = value;
			}
		}

		public override int Timeout
		{
			get
			{
				return m_timeout;
			}
			set
			{
				if (value < 0 && value != -1)
				{
					throw new ArgumentOutOfRangeException(SR.GetString("net_io_timeout_use_ge_zero"));
				}
				m_timeout = value;
			}
		}

		public override Uri RequestUri => m_uri;

		public override bool UseDefaultCredentials
		{
			get
			{
				throw ExceptionHelper.PropertyNotSupportedException;
			}
			set
			{
				throw ExceptionHelper.PropertyNotSupportedException;
			}
		}

		internal FileWebRequest(Uri uri)
		{
			if ((object)uri.Scheme != Uri.UriSchemeFile)
			{
				throw new ArgumentOutOfRangeException("uri");
			}
			m_uri = uri;
			m_fileAccess = FileAccess.Read;
			m_headers = new WebHeaderCollection(WebHeaderCollectionType.FileWebRequest);
		}

		[Obsolete("Serialization is obsoleted for this type. http://go.microsoft.com/fwlink/?linkid=14202")]
		protected FileWebRequest(SerializationInfo serializationInfo, StreamingContext streamingContext)
			: base(serializationInfo, streamingContext)
		{
			m_headers = (WebHeaderCollection)serializationInfo.GetValue("headers", typeof(WebHeaderCollection));
			m_proxy = (IWebProxy)serializationInfo.GetValue("proxy", typeof(IWebProxy));
			m_uri = (Uri)serializationInfo.GetValue("uri", typeof(Uri));
			m_connectionGroupName = serializationInfo.GetString("connectionGroupName");
			m_method = serializationInfo.GetString("method");
			m_contentLength = serializationInfo.GetInt64("contentLength");
			m_timeout = serializationInfo.GetInt32("timeout");
			m_fileAccess = (FileAccess)serializationInfo.GetInt32("fileAccess");
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter, SerializationFormatter = true)]
		void ISerializable.GetObjectData(SerializationInfo serializationInfo, StreamingContext streamingContext)
		{
			GetObjectData(serializationInfo, streamingContext);
		}

		[SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
		protected override void GetObjectData(SerializationInfo serializationInfo, StreamingContext streamingContext)
		{
			serializationInfo.AddValue("headers", m_headers, typeof(WebHeaderCollection));
			serializationInfo.AddValue("proxy", m_proxy, typeof(IWebProxy));
			serializationInfo.AddValue("uri", m_uri, typeof(Uri));
			serializationInfo.AddValue("connectionGroupName", m_connectionGroupName);
			serializationInfo.AddValue("method", m_method);
			serializationInfo.AddValue("contentLength", m_contentLength);
			serializationInfo.AddValue("timeout", m_timeout);
			serializationInfo.AddValue("fileAccess", m_fileAccess);
			serializationInfo.AddValue("preauthenticate", value: false);
			base.GetObjectData(serializationInfo, streamingContext);
		}

		[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
		public override IAsyncResult BeginGetRequestStream(AsyncCallback callback, object state)
		{
			try
			{
				if (Aborted)
				{
					throw ExceptionHelper.RequestAbortedException;
				}
				if (!CanGetRequestStream())
				{
					Exception ex = new ProtocolViolationException(SR.GetString("net_nouploadonget"));
					throw ex;
				}
				if (m_response != null)
				{
					Exception ex2 = new InvalidOperationException(SR.GetString("net_reqsubmitted"));
					throw ex2;
				}
				lock (this)
				{
					if (m_writePending)
					{
						Exception ex3 = new InvalidOperationException(SR.GetString("net_repcall"));
						throw ex3;
					}
					m_writePending = true;
				}
				m_ReadAResult = new LazyAsyncResult(this, state, callback);
				ThreadPool.QueueUserWorkItem(s_GetRequestStreamCallback, m_ReadAResult);
			}
			catch (Exception e)
			{
				if (Logging.On)
				{
					Logging.Exception(Logging.Web, this, "BeginGetRequestStream", e);
				}
				throw;
			}
			return m_ReadAResult;
		}

		[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
		public override IAsyncResult BeginGetResponse(AsyncCallback callback, object state)
		{
			try
			{
				if (Aborted)
				{
					throw ExceptionHelper.RequestAbortedException;
				}
				lock (this)
				{
					if (m_readPending)
					{
						Exception ex = new InvalidOperationException(SR.GetString("net_repcall"));
						throw ex;
					}
					m_readPending = true;
				}
				m_WriteAResult = new LazyAsyncResult(this, state, callback);
				ThreadPool.QueueUserWorkItem(s_GetResponseCallback, m_WriteAResult);
			}
			catch (Exception e)
			{
				if (Logging.On)
				{
					Logging.Exception(Logging.Web, this, "BeginGetResponse", e);
				}
				throw;
			}
			return m_WriteAResult;
		}

		private bool CanGetRequestStream()
		{
			return !KnownHttpVerb.Parse(m_method).ContentBodyNotAllowed;
		}

		public override Stream EndGetRequestStream(IAsyncResult asyncResult)
		{
			try
			{
				LazyAsyncResult lazyAsyncResult = asyncResult as LazyAsyncResult;
				if (asyncResult == null || lazyAsyncResult == null)
				{
					Exception ex = ((asyncResult == null) ? new ArgumentNullException("asyncResult") : new ArgumentException(SR.GetString("InvalidAsyncResult"), "asyncResult"));
					throw ex;
				}
				object obj = lazyAsyncResult.InternalWaitForCompletion();
				if (obj is Exception)
				{
					throw (Exception)obj;
				}
				Stream result = (Stream)obj;
				m_writePending = false;
				return result;
			}
			catch (Exception e)
			{
				if (Logging.On)
				{
					Logging.Exception(Logging.Web, this, "EndGetRequestStream", e);
				}
				throw;
			}
		}

		public override WebResponse EndGetResponse(IAsyncResult asyncResult)
		{
			try
			{
				LazyAsyncResult lazyAsyncResult = asyncResult as LazyAsyncResult;
				if (asyncResult == null || lazyAsyncResult == null)
				{
					Exception ex = ((asyncResult == null) ? new ArgumentNullException("asyncResult") : new ArgumentException(SR.GetString("InvalidAsyncResult"), "asyncResult"));
					throw ex;
				}
				object obj = lazyAsyncResult.InternalWaitForCompletion();
				if (obj is Exception)
				{
					throw (Exception)obj;
				}
				WebResponse result = (WebResponse)obj;
				m_readPending = false;
				return result;
			}
			catch (Exception e)
			{
				if (Logging.On)
				{
					Logging.Exception(Logging.Web, this, "EndGetResponse", e);
				}
				throw;
			}
		}

		public override Stream GetRequestStream()
		{
			IAsyncResult asyncResult;
			try
			{
				asyncResult = BeginGetRequestStream(null, null);
				if (Timeout != -1 && !asyncResult.IsCompleted && (!asyncResult.AsyncWaitHandle.WaitOne(Timeout, exitContext: false) || !asyncResult.IsCompleted))
				{
					if (m_stream != null)
					{
						m_stream.Close();
					}
					Exception ex = new WebException(NetRes.GetWebStatusString(WebExceptionStatus.Timeout), WebExceptionStatus.Timeout);
					throw ex;
				}
			}
			catch (Exception e)
			{
				if (Logging.On)
				{
					Logging.Exception(Logging.Web, this, "GetRequestStream", e);
				}
				throw;
			}
			return EndGetRequestStream(asyncResult);
		}

		public override WebResponse GetResponse()
		{
			m_syncHint = true;
			IAsyncResult asyncResult;
			try
			{
				asyncResult = BeginGetResponse(null, null);
				if (Timeout != -1 && !asyncResult.IsCompleted && (!asyncResult.AsyncWaitHandle.WaitOne(Timeout, exitContext: false) || !asyncResult.IsCompleted))
				{
					if (m_response != null)
					{
						m_response.Close();
					}
					Exception ex = new WebException(NetRes.GetWebStatusString(WebExceptionStatus.Timeout), WebExceptionStatus.Timeout);
					throw ex;
				}
			}
			catch (Exception e)
			{
				if (Logging.On)
				{
					Logging.Exception(Logging.Web, this, "GetResponse", e);
				}
				throw;
			}
			return EndGetResponse(asyncResult);
		}

		private static void GetRequestStreamCallback(object state)
		{
			LazyAsyncResult lazyAsyncResult = (LazyAsyncResult)state;
			FileWebRequest fileWebRequest = (FileWebRequest)lazyAsyncResult.AsyncObject;
			try
			{
				if (fileWebRequest.m_stream == null)
				{
					fileWebRequest.m_stream = new FileWebStream(fileWebRequest, fileWebRequest.m_uri.LocalPath, FileMode.Create, FileAccess.Write, FileShare.Read);
					fileWebRequest.m_fileAccess = FileAccess.Write;
					fileWebRequest.m_writing = true;
					lazyAsyncResult.InvokeCallback(fileWebRequest.m_stream);
				}
			}
			catch (Exception ex)
			{
				if (lazyAsyncResult.IsCompleted || NclUtilities.IsFatal(ex))
				{
					throw;
				}
				Exception result = new WebException(ex.Message, ex);
				lazyAsyncResult.InvokeCallback(result);
			}
		}

		private static void GetResponseCallback(object state)
		{
			LazyAsyncResult lazyAsyncResult = (LazyAsyncResult)state;
			FileWebRequest fileWebRequest = (FileWebRequest)lazyAsyncResult.AsyncObject;
			if (fileWebRequest.m_writePending || fileWebRequest.m_writing)
			{
				lock (fileWebRequest)
				{
					if (fileWebRequest.m_writePending || fileWebRequest.m_writing)
					{
						fileWebRequest.m_readerEvent = new ManualResetEvent(initialState: false);
					}
				}
			}
			if (fileWebRequest.m_readerEvent != null)
			{
				fileWebRequest.m_readerEvent.WaitOne();
			}
			try
			{
				if (fileWebRequest.m_response == null)
				{
					fileWebRequest.m_response = new FileWebResponse(fileWebRequest, fileWebRequest.m_uri, fileWebRequest.m_fileAccess, !fileWebRequest.m_syncHint);
				}
				lazyAsyncResult.InvokeCallback(fileWebRequest.m_response);
			}
			catch (Exception ex)
			{
				if (lazyAsyncResult.IsCompleted || NclUtilities.IsFatal(ex))
				{
					throw;
				}
				Exception result = new WebException(ex.Message, ex);
				lazyAsyncResult.InvokeCallback(result);
			}
		}

		internal void UnblockReader()
		{
			lock (this)
			{
				if (m_readerEvent != null)
				{
					m_readerEvent.Set();
				}
			}
			m_writing = false;
		}

		public override void Abort()
		{
			if (Logging.On)
			{
				Logging.PrintWarning(Logging.Web, NetRes.GetWebStatusString("net_requestaborted", WebExceptionStatus.RequestCanceled));
			}
			try
			{
				if (Interlocked.Increment(ref m_Aborted) != 1)
				{
					return;
				}
				LazyAsyncResult readAResult = m_ReadAResult;
				LazyAsyncResult writeAResult = m_WriteAResult;
				WebException result = new WebException(NetRes.GetWebStatusString("net_requestaborted", WebExceptionStatus.RequestCanceled), WebExceptionStatus.RequestCanceled);
				Stream stream = m_stream;
				if (readAResult != null && !readAResult.IsCompleted)
				{
					readAResult.InvokeCallback(result);
				}
				if (writeAResult != null && !writeAResult.IsCompleted)
				{
					writeAResult.InvokeCallback(result);
				}
				if (stream != null)
				{
					if (stream is ICloseEx)
					{
						((ICloseEx)stream).CloseEx(CloseExState.Abort);
					}
					else
					{
						stream.Close();
					}
				}
				if (m_response != null)
				{
					((ICloseEx)m_response).CloseEx(CloseExState.Abort);
				}
			}
			catch (Exception e)
			{
				if (Logging.On)
				{
					Logging.Exception(Logging.Web, this, "Abort", e);
				}
				throw;
			}
		}
	}
}
