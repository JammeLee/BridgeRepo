using System.ComponentModel;
using System.IO;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security.Authentication.ExtendedProtection;
using System.Security.Permissions;
using System.Threading;

namespace System.Net
{
	internal class ConnectStream : Stream, ICloseEx
	{
		private static class Nesting
		{
			public const int Idle = 0;

			public const int IoInProgress = 1;

			public const int Closed = 2;

			public const int InError = 3;

			public const int InternalIO = 4;
		}

		private const long c_MaxDrainBytes = 65536L;

		private const int AlreadyAborted = 777777;

		private int m_CallNesting;

		private ScatterGatherBuffers m_BufferedData;

		private bool m_SuppressWrite;

		private bool m_BufferOnly;

		private long m_BytesLeftToWrite;

		private int m_BytesAlreadyTransferred;

		private Connection m_Connection;

		private byte[] m_ReadBuffer;

		private int m_ReadOffset;

		private int m_ReadBufferSize;

		private long m_ReadBytes;

		private bool m_Chunked;

		private int m_DoneCalled;

		private int m_ShutDown;

		private Exception m_ErrorException;

		private bool m_ChunkEofRecvd;

		private int m_ChunkSize;

		private byte[] m_TempBuffer;

		private bool m_ChunkedNeedCRLFRead;

		private bool m_Draining;

		private HttpWriteMode m_HttpWriteMode;

		private int m_ReadTimeout;

		private int m_WriteTimeout;

		private static readonly WaitCallback m_ReadChunkedCallbackDelegate = ReadChunkedCallback;

		private static readonly AsyncCallback m_ReadCallbackDelegate = ReadCallback;

		private static readonly AsyncCallback m_WriteCallbackDelegate = WriteCallback;

		private static readonly AsyncCallback m_WriteHeadersCallback = WriteHeadersCallback;

		private static readonly object ZeroLengthRead = new object();

		private HttpWebRequest m_Request;

		private bool m_IgnoreSocketErrors;

		private bool m_ErrorResponseStatus;

		internal static byte[] s_DrainingBuffer = new byte[4096];

		public override bool CanTimeout => true;

		public override int ReadTimeout
		{
			get
			{
				return m_ReadTimeout;
			}
			set
			{
				if (value <= 0 && value != -1)
				{
					throw new ArgumentOutOfRangeException(SR.GetString("net_io_timeout_use_gt_zero"));
				}
				m_ReadTimeout = value;
			}
		}

		public override int WriteTimeout
		{
			get
			{
				return m_WriteTimeout;
			}
			set
			{
				if (value <= 0 && value != -1)
				{
					throw new ArgumentOutOfRangeException(SR.GetString("net_io_timeout_use_gt_zero"));
				}
				m_WriteTimeout = value;
			}
		}

		internal bool IgnoreSocketErrors => m_IgnoreSocketErrors;

		internal bool SuppressWrite
		{
			set
			{
				m_SuppressWrite = value;
			}
		}

		internal Connection Connection => m_Connection;

		internal bool BufferOnly => m_BufferOnly;

		internal ScatterGatherBuffers BufferedData
		{
			get
			{
				return m_BufferedData;
			}
			set
			{
				m_BufferedData = value;
			}
		}

		private bool WriteChunked => m_HttpWriteMode == HttpWriteMode.Chunked;

		internal long BytesLeftToWrite => m_BytesLeftToWrite;

		private bool WriteStream => m_HttpWriteMode != HttpWriteMode.Unknown;

		internal bool IsPostStream => m_HttpWriteMode != HttpWriteMode.None;

		internal bool ErrorInStream => m_ErrorException != null;

		internal bool IsClosed => m_ShutDown != 0;

		public override bool CanRead
		{
			get
			{
				if (!WriteStream)
				{
					return !IsClosed;
				}
				return false;
			}
		}

		public override bool CanSeek => false;

		public override bool CanWrite
		{
			get
			{
				if (WriteStream)
				{
					return !IsClosed;
				}
				return false;
			}
		}

		public override long Length
		{
			get
			{
				throw new NotSupportedException(SR.GetString("net_noseek"));
			}
		}

		public override long Position
		{
			get
			{
				throw new NotSupportedException(SR.GetString("net_noseek"));
			}
			set
			{
				throw new NotSupportedException(SR.GetString("net_noseek"));
			}
		}

		private bool Eof
		{
			get
			{
				if (ErrorInStream)
				{
					return true;
				}
				if (m_Chunked)
				{
					return m_ChunkEofRecvd;
				}
				if (m_ReadBytes == 0)
				{
					return true;
				}
				if (m_ReadBytes == -1)
				{
					if (m_DoneCalled > 0)
					{
						return m_ReadBufferSize <= 0;
					}
					return false;
				}
				return false;
			}
		}

		internal void ErrorResponseNotify(bool isKeepAlive)
		{
			m_ErrorResponseStatus = true;
			m_IgnoreSocketErrors |= !isKeepAlive;
		}

		internal void FatalResponseNotify()
		{
			if (m_ErrorException == null)
			{
				Interlocked.CompareExchange(ref m_ErrorException, new IOException(SR.GetString("net_io_readfailure", SR.GetString("net_io_connectionclosed"))), null);
			}
			m_ErrorResponseStatus = false;
		}

		public ConnectStream(Connection connection, HttpWebRequest request)
		{
			m_Connection = connection;
			m_ReadTimeout = (m_WriteTimeout = -1);
			m_Request = request;
			m_HttpWriteMode = request.HttpWriteMode;
			m_BytesLeftToWrite = ((m_HttpWriteMode == HttpWriteMode.ContentLength) ? request.ContentLength : (-1));
			if (request.HttpWriteMode == HttpWriteMode.Buffer)
			{
				m_BufferOnly = true;
				EnableWriteBuffering();
			}
		}

		public ConnectStream(Connection connection, byte[] buffer, int offset, int bufferCount, long readCount, bool chunked, HttpWebRequest request)
		{
			if (Logging.On)
			{
				Logging.PrintInfo(Logging.Web, this, "ConnectStream", SR.GetString("net_log_buffered_n_bytes", readCount));
			}
			m_ReadBuffer = buffer;
			m_ReadOffset = offset;
			m_ReadBufferSize = bufferCount;
			m_ReadBytes = readCount;
			m_ReadTimeout = (m_WriteTimeout = -1);
			m_Chunked = chunked;
			m_Connection = connection;
			m_TempBuffer = new byte[2];
			m_Request = request;
		}

		internal void SwitchToContentLength()
		{
			m_HttpWriteMode = HttpWriteMode.ContentLength;
		}

		internal void CallDone()
		{
			CallDone(null);
		}

		private void CallDone(ConnectionReturnResult returnResult)
		{
			if (Interlocked.Increment(ref m_DoneCalled) != 1)
			{
				return;
			}
			if (!WriteStream)
			{
				if (returnResult == null)
				{
					m_Connection.ReadStartNextRequest(m_Request, ref returnResult);
				}
				else
				{
					ConnectionReturnResult.SetResponses(returnResult);
				}
			}
			else
			{
				m_Request.WriteCallDone(this, returnResult);
			}
		}

		internal void ProcessWriteCallDone(ConnectionReturnResult returnResult)
		{
			try
			{
				if (returnResult == null)
				{
					m_Connection.WriteStartNextRequest(m_Request, ref returnResult);
					if (!m_Request.Async)
					{
						object obj = m_Request.ConnectionReaderAsyncResult.InternalWaitForCompletion();
						if (obj == null && !m_Request.SawInitialResponse)
						{
							m_Connection.SyncRead(m_Request, userRetrievedStream: true, probeRead: false);
						}
					}
					m_Request.SawInitialResponse = false;
				}
				ConnectionReturnResult.SetResponses(returnResult);
			}
			finally
			{
				if (IsPostStream || m_Request.Async)
				{
					m_Request.CheckWriteSideResponseProcessing();
				}
			}
		}

		internal void ResubmitWrite(ConnectStream oldStream, bool suppressWrite)
		{
			try
			{
				Interlocked.CompareExchange(ref m_CallNesting, 4, 0);
				ScatterGatherBuffers bufferedData = oldStream.BufferedData;
				SafeSetSocketTimeout(SocketShutdown.Send);
				if (!WriteChunked)
				{
					if (!suppressWrite)
					{
						m_Connection.Write(bufferedData);
					}
				}
				else
				{
					m_HttpWriteMode = HttpWriteMode.ContentLength;
					if (bufferedData.Length == 0)
					{
						m_Connection.Write(NclConstants.ChunkTerminator, 0, NclConstants.ChunkTerminator.Length);
					}
					else
					{
						int offset = 0;
						byte[] chunkHeader = GetChunkHeader(bufferedData.Length, out offset);
						BufferOffsetSize[] buffers = bufferedData.GetBuffers();
						BufferOffsetSize[] array = new BufferOffsetSize[buffers.Length + 3];
						array[0] = new BufferOffsetSize(chunkHeader, offset, chunkHeader.Length - offset, copyBuffer: false);
						int num = 0;
						BufferOffsetSize[] array2 = buffers;
						foreach (BufferOffsetSize bufferOffsetSize in array2)
						{
							array[++num] = bufferOffsetSize;
						}
						array[++num] = new BufferOffsetSize(NclConstants.CRLF, 0, NclConstants.CRLF.Length, copyBuffer: false);
						array[++num] = new BufferOffsetSize(NclConstants.ChunkTerminator, 0, NclConstants.ChunkTerminator.Length, copyBuffer: false);
						SplitWritesState splitWritesState = new SplitWritesState(array);
						for (BufferOffsetSize[] nextBuffers = splitWritesState.GetNextBuffers(); nextBuffers != null; nextBuffers = splitWritesState.GetNextBuffers())
						{
							m_Connection.MultipleWrite(nextBuffers);
						}
					}
				}
				if (Logging.On && bufferedData.GetBuffers() != null)
				{
					BufferOffsetSize[] buffers2 = bufferedData.GetBuffers();
					foreach (BufferOffsetSize bufferOffsetSize2 in buffers2)
					{
						if (bufferOffsetSize2 == null)
						{
							Logging.Dump(Logging.Web, this, "ResubmitWrite", null, 0, 0);
						}
						else
						{
							Logging.Dump(Logging.Web, this, "ResubmitWrite", bufferOffsetSize2.Buffer, bufferOffsetSize2.Offset, bufferOffsetSize2.Size);
						}
					}
				}
			}
			catch (Exception ex)
			{
				if (NclUtilities.IsFatal(ex))
				{
					throw;
				}
				WebException exception = new WebException(NetRes.GetWebStatusString("net_connclosed", WebExceptionStatus.SendFailure), WebExceptionStatus.SendFailure, WebExceptionInternalStatus.RequestFatal, ex);
				IOError(exception, willThrow: false);
			}
			finally
			{
				Interlocked.CompareExchange(ref m_CallNesting, 0, 4);
			}
			m_BytesLeftToWrite = 0L;
			CallDone();
		}

		internal void EnableWriteBuffering()
		{
			if (BufferedData == null)
			{
				if (WriteChunked)
				{
					BufferedData = new ScatterGatherBuffers();
				}
				else
				{
					BufferedData = new ScatterGatherBuffers(BytesLeftToWrite);
				}
			}
		}

		private int FillFromBufferedData(byte[] buffer, ref int offset, ref int size)
		{
			if (m_ReadBufferSize == 0)
			{
				return 0;
			}
			int num = Math.Min(size, m_ReadBufferSize);
			Buffer.BlockCopy(m_ReadBuffer, m_ReadOffset, buffer, offset, num);
			m_ReadOffset += num;
			m_ReadBufferSize -= num;
			if (m_ReadBufferSize == 0)
			{
				m_ReadBuffer = null;
			}
			size -= num;
			offset += num;
			return num;
		}

		public override void Write(byte[] buffer, int offset, int size)
		{
			if (Logging.On)
			{
				Logging.Enter(Logging.Web, this, "Write", "");
			}
			if (!WriteStream)
			{
				throw new NotSupportedException(SR.GetString("net_readonlystream"));
			}
			if (buffer == null)
			{
				throw new ArgumentNullException("buffer");
			}
			if (offset < 0 || offset > buffer.Length)
			{
				throw new ArgumentOutOfRangeException("offset");
			}
			if (size < 0 || size > buffer.Length - offset)
			{
				throw new ArgumentOutOfRangeException("size");
			}
			InternalWrite(async: false, buffer, offset, size, null, null);
			if (Logging.On)
			{
				Logging.Dump(Logging.Web, this, "Write", buffer, offset, size);
			}
			if (Logging.On)
			{
				Logging.Exit(Logging.Web, this, "Write", "");
			}
		}

		[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
		public override IAsyncResult BeginWrite(byte[] buffer, int offset, int size, AsyncCallback callback, object state)
		{
			if (Logging.On)
			{
				Logging.Enter(Logging.Web, this, "BeginWrite", "");
			}
			if (!WriteStream)
			{
				throw new NotSupportedException(SR.GetString("net_readonlystream"));
			}
			if (buffer == null)
			{
				throw new ArgumentNullException("buffer");
			}
			if (offset < 0 || offset > buffer.Length)
			{
				throw new ArgumentOutOfRangeException("offset");
			}
			if (size < 0 || size > buffer.Length - offset)
			{
				throw new ArgumentOutOfRangeException("size");
			}
			IAsyncResult asyncResult = InternalWrite(async: true, buffer, offset, size, callback, state);
			if (Logging.On)
			{
				Logging.Exit(Logging.Web, this, "BeginWrite", asyncResult);
			}
			return asyncResult;
		}

		private IAsyncResult InternalWrite(bool async, byte[] buffer, int offset, int size, AsyncCallback callback, object state)
		{
			if (ErrorInStream)
			{
				throw m_ErrorException;
			}
			if (IsClosed && !IgnoreSocketErrors)
			{
				throw new WebException(NetRes.GetWebStatusString("net_requestaborted", WebExceptionStatus.ConnectionClosed), WebExceptionStatus.ConnectionClosed);
			}
			if (m_Request.Aborted && !IgnoreSocketErrors)
			{
				throw new WebException(NetRes.GetWebStatusString("net_requestaborted", WebExceptionStatus.RequestCanceled), WebExceptionStatus.RequestCanceled);
			}
			int num = Interlocked.CompareExchange(ref m_CallNesting, 1, 0);
			if (num != 0 && num != 2)
			{
				throw new NotSupportedException(SR.GetString("net_no_concurrent_io_allowed"));
			}
			if (BufferedData != null && size != 0 && (m_Request.ContentLength != 0 || !IsPostStream || !m_Request.NtlmKeepAlive))
			{
				BufferedData.Write(buffer, offset, size);
			}
			LazyAsyncResult lazyAsyncResult = null;
			bool flag = false;
			try
			{
				if (size == 0 || BufferOnly || m_SuppressWrite || IgnoreSocketErrors)
				{
					if (m_SuppressWrite && m_BytesLeftToWrite > 0 && size > 0)
					{
						m_BytesLeftToWrite -= size;
					}
					if (async)
					{
						lazyAsyncResult = new LazyAsyncResult(this, state, callback);
						flag = true;
					}
					return lazyAsyncResult;
				}
				if (WriteChunked)
				{
					int offset2 = 0;
					byte[] chunkHeader = GetChunkHeader(size, out offset2);
					BufferOffsetSize[] buffers;
					if (!m_ErrorResponseStatus)
					{
						buffers = new BufferOffsetSize[3]
						{
							new BufferOffsetSize(chunkHeader, offset2, chunkHeader.Length - offset2, copyBuffer: false),
							new BufferOffsetSize(buffer, offset, size, copyBuffer: false),
							new BufferOffsetSize(NclConstants.CRLF, 0, NclConstants.CRLF.Length, copyBuffer: false)
						};
					}
					else
					{
						m_IgnoreSocketErrors = true;
						buffers = new BufferOffsetSize[1]
						{
							new BufferOffsetSize(NclConstants.ChunkTerminator, 0, NclConstants.ChunkTerminator.Length, copyBuffer: false)
						};
					}
					lazyAsyncResult = (async ? new NestedMultipleAsyncResult(this, state, callback, buffers) : null);
					try
					{
						if (async)
						{
							m_Connection.BeginMultipleWrite(buffers, m_WriteCallbackDelegate, lazyAsyncResult);
						}
						else
						{
							SafeSetSocketTimeout(SocketShutdown.Send);
							m_Connection.MultipleWrite(buffers);
						}
					}
					catch (Exception ex)
					{
						if (IgnoreSocketErrors && !NclUtilities.IsFatal(ex))
						{
							if (async)
							{
								flag = true;
							}
							return lazyAsyncResult;
						}
						if (m_Request.Aborted && (ex is IOException || ex is ObjectDisposedException))
						{
							throw new WebException(NetRes.GetWebStatusString("net_requestaborted", WebExceptionStatus.RequestCanceled), WebExceptionStatus.RequestCanceled);
						}
						num = 3;
						if (NclUtilities.IsFatal(ex))
						{
							m_ErrorResponseStatus = false;
							IOError(ex);
							throw;
						}
						if (!m_ErrorResponseStatus)
						{
							IOError(ex);
							throw;
						}
						m_IgnoreSocketErrors = true;
						if (async)
						{
							flag = true;
						}
					}
					return lazyAsyncResult;
				}
				lazyAsyncResult = (async ? new NestedSingleAsyncResult(this, state, callback, buffer, offset, size) : null);
				if (BytesLeftToWrite != -1)
				{
					if (BytesLeftToWrite < size)
					{
						throw new ProtocolViolationException(SR.GetString("net_entitytoobig"));
					}
					if (!async)
					{
						m_BytesLeftToWrite -= size;
					}
				}
				try
				{
					if (async)
					{
						if (m_Request.ContentLength == 0 && IsPostStream)
						{
							m_BytesLeftToWrite -= size;
							flag = true;
						}
						else
						{
							m_BytesAlreadyTransferred = size;
							m_Connection.BeginWrite(buffer, offset, size, m_WriteCallbackDelegate, lazyAsyncResult);
						}
					}
					else
					{
						SafeSetSocketTimeout(SocketShutdown.Send);
						if (m_Request.ContentLength != 0 || !IsPostStream || !m_Request.NtlmKeepAlive)
						{
							m_Connection.Write(buffer, offset, size);
						}
					}
				}
				catch (Exception ex2)
				{
					if (IgnoreSocketErrors && !NclUtilities.IsFatal(ex2))
					{
						if (async)
						{
							flag = true;
						}
						return lazyAsyncResult;
					}
					if (m_Request.Aborted && (ex2 is IOException || ex2 is ObjectDisposedException))
					{
						throw new WebException(NetRes.GetWebStatusString("net_requestaborted", WebExceptionStatus.RequestCanceled), WebExceptionStatus.RequestCanceled);
					}
					num = 3;
					if (NclUtilities.IsFatal(ex2))
					{
						m_ErrorResponseStatus = false;
						IOError(ex2);
						throw;
					}
					if (!m_ErrorResponseStatus)
					{
						IOError(ex2);
						throw;
					}
					m_IgnoreSocketErrors = true;
					if (async)
					{
						flag = true;
					}
				}
				return lazyAsyncResult;
			}
			finally
			{
				if (!async || num == 3 || flag)
				{
					num = Interlocked.CompareExchange(ref m_CallNesting, (num == 3) ? 3 : 0, 1);
					if (num == 2)
					{
						ResumeInternalClose(lazyAsyncResult);
					}
					else if (flag)
					{
						lazyAsyncResult?.InvokeCallback();
					}
				}
			}
		}

		private static void WriteCallback(IAsyncResult asyncResult)
		{
			LazyAsyncResult lazyAsyncResult = (LazyAsyncResult)asyncResult.AsyncState;
			((ConnectStream)lazyAsyncResult.AsyncObject).ProcessWriteCallback(asyncResult, lazyAsyncResult);
		}

		private void ProcessWriteCallback(IAsyncResult asyncResult, LazyAsyncResult userResult)
		{
			Exception ex = null;
			try
			{
				NestedSingleAsyncResult nestedSingleAsyncResult = userResult as NestedSingleAsyncResult;
				if (nestedSingleAsyncResult != null)
				{
					try
					{
						m_Connection.EndWrite(asyncResult);
						if (BytesLeftToWrite != -1)
						{
							m_BytesLeftToWrite -= m_BytesAlreadyTransferred;
							m_BytesAlreadyTransferred = 0;
						}
						if (Logging.On)
						{
							Logging.Dump(Logging.Web, this, "WriteCallback", nestedSingleAsyncResult.Buffer, nestedSingleAsyncResult.Offset, nestedSingleAsyncResult.Size);
						}
					}
					catch (Exception ex2)
					{
						ex = ex2;
						if (NclUtilities.IsFatal(ex2))
						{
							m_ErrorResponseStatus = false;
							IOError(ex2);
							throw;
						}
						if (m_ErrorResponseStatus)
						{
							m_IgnoreSocketErrors = true;
							ex = null;
						}
					}
					return;
				}
				NestedMultipleAsyncResult nestedMultipleAsyncResult = (NestedMultipleAsyncResult)userResult;
				try
				{
					m_Connection.EndMultipleWrite(asyncResult);
					if (Logging.On)
					{
						BufferOffsetSize[] buffers = nestedMultipleAsyncResult.Buffers;
						foreach (BufferOffsetSize bufferOffsetSize in buffers)
						{
							Logging.Dump(Logging.Web, nestedMultipleAsyncResult, "WriteCallback", bufferOffsetSize.Buffer, bufferOffsetSize.Offset, bufferOffsetSize.Size);
						}
					}
				}
				catch (Exception ex3)
				{
					ex = ex3;
					if (NclUtilities.IsFatal(ex3))
					{
						m_ErrorResponseStatus = false;
						IOError(ex3);
						throw;
					}
					if (m_ErrorResponseStatus)
					{
						m_IgnoreSocketErrors = true;
						ex = null;
					}
				}
			}
			finally
			{
				if (2 == ExchangeCallNesting((ex != null) ? 3 : 0, 1))
				{
					if (ex != null && m_ErrorException == null)
					{
						Interlocked.CompareExchange(ref m_ErrorException, ex, null);
					}
					ResumeInternalClose(userResult);
				}
				else
				{
					userResult.InvokeCallback(ex);
				}
			}
		}

		private int ExchangeCallNesting(int value, int comparand)
		{
			return Interlocked.CompareExchange(ref m_CallNesting, value, comparand);
		}

		public override void EndWrite(IAsyncResult asyncResult)
		{
			if (Logging.On)
			{
				Logging.Enter(Logging.Web, this, "EndWrite", "");
			}
			if (asyncResult == null)
			{
				throw new ArgumentNullException("asyncResult");
			}
			LazyAsyncResult lazyAsyncResult = asyncResult as LazyAsyncResult;
			if (lazyAsyncResult == null || lazyAsyncResult.AsyncObject != this)
			{
				throw new ArgumentException(SR.GetString("net_io_invalidasyncresult"), "asyncResult");
			}
			if (lazyAsyncResult.EndCalled)
			{
				throw new InvalidOperationException(SR.GetString("net_io_invalidendcall", "EndWrite"));
			}
			lazyAsyncResult.EndCalled = true;
			object obj = lazyAsyncResult.InternalWaitForCompletion();
			if (ErrorInStream)
			{
				throw m_ErrorException;
			}
			Exception ex = obj as Exception;
			if (ex != null)
			{
				if (ex is IOException && m_Request.Aborted)
				{
					throw new WebException(NetRes.GetWebStatusString("net_requestaborted", WebExceptionStatus.RequestCanceled), WebExceptionStatus.RequestCanceled);
				}
				IOError(ex);
				throw ex;
			}
			if (Logging.On)
			{
				Logging.Exit(Logging.Web, this, "EndWrite", "");
			}
		}

		public override int Read([In][Out] byte[] buffer, int offset, int size)
		{
			if (Logging.On)
			{
				Logging.Enter(Logging.Web, this, "Read", "");
			}
			if (WriteStream)
			{
				throw new NotSupportedException(SR.GetString("net_writeonlystream"));
			}
			if (buffer == null)
			{
				throw new ArgumentNullException("buffer");
			}
			if (offset < 0 || offset > buffer.Length)
			{
				throw new ArgumentOutOfRangeException("offset");
			}
			if (size < 0 || size > buffer.Length - offset)
			{
				throw new ArgumentOutOfRangeException("size");
			}
			if (ErrorInStream)
			{
				throw m_ErrorException;
			}
			if (IsClosed)
			{
				throw new WebException(NetRes.GetWebStatusString("net_requestaborted", WebExceptionStatus.ConnectionClosed), WebExceptionStatus.ConnectionClosed);
			}
			if (m_Request.Aborted)
			{
				throw new WebException(NetRes.GetWebStatusString("net_requestaborted", WebExceptionStatus.RequestCanceled), WebExceptionStatus.RequestCanceled);
			}
			if (Interlocked.CompareExchange(ref m_CallNesting, 1, 0) != 0)
			{
				throw new NotSupportedException(SR.GetString("net_no_concurrent_io_allowed"));
			}
			int num = -1;
			try
			{
				SafeSetSocketTimeout(SocketShutdown.Receive);
			}
			catch (Exception exception)
			{
				IOError(exception);
				throw;
			}
			try
			{
				num = ReadWithoutValidation(buffer, offset, size);
			}
			catch (Exception ex)
			{
				Win32Exception ex2 = ex.InnerException as Win32Exception;
				if (ex2 != null && ex2.NativeErrorCode == 10060)
				{
					ex = new WebException(SR.GetString("net_timeout"), WebExceptionStatus.Timeout);
				}
				throw ex;
			}
			Interlocked.CompareExchange(ref m_CallNesting, 0, 1);
			if (Logging.On && num > 0)
			{
				Logging.Dump(Logging.Web, this, "Read", buffer, offset, num);
			}
			if (Logging.On)
			{
				Logging.Exit(Logging.Web, this, "Read", num);
			}
			return num;
		}

		private int ReadWithoutValidation(byte[] buffer, int offset, int size)
		{
			return ReadWithoutValidation(buffer, offset, size, abortOnError: true);
		}

		private int ReadWithoutValidation([In][Out] byte[] buffer, int offset, int size, bool abortOnError)
		{
			int num = 0;
			if (!m_Chunked)
			{
				num = (int)((m_ReadBytes == -1) ? size : Math.Min(m_ReadBytes, size));
			}
			else if (!m_ChunkEofRecvd)
			{
				if (m_ChunkSize == 0)
				{
					try
					{
						num = ReadChunkedSync(buffer, offset, size);
						m_ChunkSize -= num;
						return num;
					}
					catch (Exception exception)
					{
						if (abortOnError)
						{
							IOError(exception);
						}
						throw;
					}
				}
				num = Math.Min(size, m_ChunkSize);
			}
			if (num == 0 || Eof)
			{
				return 0;
			}
			try
			{
				num = InternalRead(buffer, offset, num);
			}
			catch (Exception exception2)
			{
				if (abortOnError)
				{
					IOError(exception2);
				}
				throw;
			}
			int num2 = num;
			if (m_Chunked && m_ChunkSize > 0)
			{
				if (num2 == 0)
				{
					WebException ex = new WebException(NetRes.GetWebStatusString("net_requestaborted", WebExceptionStatus.ConnectionClosed), WebExceptionStatus.ConnectionClosed);
					IOError(ex, willThrow: true);
					throw ex;
				}
				m_ChunkSize -= num2;
			}
			else
			{
				bool flag = false;
				if (num2 <= 0)
				{
					num2 = 0;
					if (m_ReadBytes != -1)
					{
						if (!abortOnError)
						{
							throw m_ErrorException;
						}
						IOError(null, willThrow: false);
					}
					else
					{
						flag = true;
					}
				}
				if (m_ReadBytes != -1)
				{
					m_ReadBytes -= num2;
					if (m_ReadBytes < 0)
					{
						throw new InternalException();
					}
				}
				if (m_ReadBytes == 0 || flag)
				{
					m_ReadBytes = 0L;
					CallDone();
				}
			}
			return num2;
		}

		[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
		public override IAsyncResult BeginRead(byte[] buffer, int offset, int size, AsyncCallback callback, object state)
		{
			if (Logging.On)
			{
				Logging.Enter(Logging.Web, this, "BeginRead", "");
			}
			if (WriteStream)
			{
				throw new NotSupportedException(SR.GetString("net_writeonlystream"));
			}
			if (buffer == null)
			{
				throw new ArgumentNullException("buffer");
			}
			if (offset < 0 || offset > buffer.Length)
			{
				throw new ArgumentOutOfRangeException("offset");
			}
			if (size < 0 || size > buffer.Length - offset)
			{
				throw new ArgumentOutOfRangeException("size");
			}
			if (ErrorInStream)
			{
				throw m_ErrorException;
			}
			if (IsClosed)
			{
				throw new WebException(NetRes.GetWebStatusString("net_requestaborted", WebExceptionStatus.ConnectionClosed), WebExceptionStatus.ConnectionClosed);
			}
			if (m_Request.Aborted)
			{
				throw new WebException(NetRes.GetWebStatusString("net_requestaborted", WebExceptionStatus.RequestCanceled), WebExceptionStatus.RequestCanceled);
			}
			if (Interlocked.CompareExchange(ref m_CallNesting, 1, 0) != 0)
			{
				throw new NotSupportedException(SR.GetString("net_no_concurrent_io_allowed"));
			}
			IAsyncResult asyncResult = BeginReadWithoutValidation(buffer, offset, size, callback, state);
			if (Logging.On)
			{
				Logging.Exit(Logging.Web, this, "BeginRead", asyncResult);
			}
			return asyncResult;
		}

		private IAsyncResult BeginReadWithoutValidation(byte[] buffer, int offset, int size, AsyncCallback callback, object state)
		{
			int size2 = 0;
			if (!m_Chunked)
			{
				size2 = (int)((m_ReadBytes == -1) ? size : Math.Min(m_ReadBytes, size));
			}
			else if (!m_ChunkEofRecvd)
			{
				if (m_ChunkSize == 0)
				{
					NestedSingleAsyncResult nestedSingleAsyncResult = new NestedSingleAsyncResult(this, state, callback, buffer, offset, size);
					ThreadPool.QueueUserWorkItem(m_ReadChunkedCallbackDelegate, nestedSingleAsyncResult);
					return nestedSingleAsyncResult;
				}
				size2 = Math.Min(size, m_ChunkSize);
			}
			if (size2 == 0 || Eof)
			{
				return new NestedSingleAsyncResult(this, state, callback, ZeroLengthRead);
			}
			try
			{
				int num = 0;
				if (m_ReadBufferSize > 0)
				{
					num = FillFromBufferedData(buffer, ref offset, ref size2);
					if (size2 == 0)
					{
						return new NestedSingleAsyncResult(this, state, callback, num);
					}
				}
				if (ErrorInStream)
				{
					throw m_ErrorException;
				}
				m_BytesAlreadyTransferred = num;
				IAsyncResult asyncResult = m_Connection.BeginRead(buffer, offset, size2, callback, state);
				if (asyncResult == null)
				{
					m_BytesAlreadyTransferred = 0;
					m_ErrorException = new WebException(NetRes.GetWebStatusString("net_requestaborted", WebExceptionStatus.RequestCanceled), WebExceptionStatus.RequestCanceled);
					throw m_ErrorException;
				}
				return asyncResult;
			}
			catch (Exception exception)
			{
				IOError(exception);
				throw;
			}
		}

		private int InternalRead(byte[] buffer, int offset, int size)
		{
			int num = FillFromBufferedData(buffer, ref offset, ref size);
			if (num > 0)
			{
				return num;
			}
			if (ErrorInStream)
			{
				throw m_ErrorException;
			}
			return m_Connection.Read(buffer, offset, size);
		}

		private static void ReadCallback(IAsyncResult asyncResult)
		{
			NestedSingleAsyncResult nestedSingleAsyncResult = (NestedSingleAsyncResult)asyncResult.AsyncState;
			ConnectStream connectStream = (ConnectStream)nestedSingleAsyncResult.AsyncObject;
			try
			{
				int num = connectStream.m_Connection.EndRead(asyncResult);
				if (Logging.On)
				{
					Logging.Dump(Logging.Web, connectStream, "ReadCallback", nestedSingleAsyncResult.Buffer, nestedSingleAsyncResult.Offset, Math.Min(nestedSingleAsyncResult.Size, num));
				}
				nestedSingleAsyncResult.InvokeCallback(num);
			}
			catch (Exception ex)
			{
				if (NclUtilities.IsFatal(ex))
				{
					throw;
				}
				nestedSingleAsyncResult.InvokeCallback(ex);
			}
		}

		public override int EndRead(IAsyncResult asyncResult)
		{
			if (Logging.On)
			{
				Logging.Enter(Logging.Web, this, "EndRead", "");
			}
			if (asyncResult == null)
			{
				throw new ArgumentNullException("asyncResult");
			}
			bool zeroLengthRead = false;
			int bytesTransferred;
			if (asyncResult.GetType() == typeof(NestedSingleAsyncResult))
			{
				NestedSingleAsyncResult nestedSingleAsyncResult = (NestedSingleAsyncResult)asyncResult;
				if (nestedSingleAsyncResult.AsyncObject != this)
				{
					throw new ArgumentException(SR.GetString("net_io_invalidasyncresult"), "asyncResult");
				}
				if (nestedSingleAsyncResult.EndCalled)
				{
					throw new InvalidOperationException(SR.GetString("net_io_invalidendcall", "EndRead"));
				}
				nestedSingleAsyncResult.EndCalled = true;
				if (ErrorInStream)
				{
					throw m_ErrorException;
				}
				object obj = nestedSingleAsyncResult.InternalWaitForCompletion();
				Exception ex = obj as Exception;
				if (ex != null)
				{
					IOError(ex, willThrow: false);
					bytesTransferred = -1;
				}
				else if (obj == null)
				{
					bytesTransferred = 0;
				}
				else if (obj == ZeroLengthRead)
				{
					bytesTransferred = 0;
					zeroLengthRead = true;
				}
				else
				{
					try
					{
						bytesTransferred = (int)obj;
					}
					catch (InvalidCastException)
					{
						bytesTransferred = -1;
					}
				}
			}
			else
			{
				try
				{
					bytesTransferred = m_Connection.EndRead(asyncResult);
				}
				catch (Exception exception)
				{
					if (NclUtilities.IsFatal(exception))
					{
						throw;
					}
					IOError(exception, willThrow: false);
					bytesTransferred = -1;
				}
			}
			bytesTransferred = EndReadWithoutValidation(bytesTransferred, zeroLengthRead);
			Interlocked.CompareExchange(ref m_CallNesting, 0, 1);
			if (Logging.On)
			{
				Logging.Exit(Logging.Web, this, "EndRead", bytesTransferred);
			}
			if (m_ErrorException != null)
			{
				throw m_ErrorException;
			}
			return bytesTransferred;
		}

		private int EndReadWithoutValidation(int bytesTransferred, bool zeroLengthRead)
		{
			int bytesAlreadyTransferred = m_BytesAlreadyTransferred;
			m_BytesAlreadyTransferred = 0;
			if (m_Chunked)
			{
				if (bytesTransferred < 0)
				{
					IOError(null, willThrow: false);
					bytesTransferred = 0;
				}
				if (bytesTransferred == 0 && m_ChunkSize > 0)
				{
					WebException ex = new WebException(NetRes.GetWebStatusString("net_requestaborted", WebExceptionStatus.ConnectionClosed), WebExceptionStatus.ConnectionClosed);
					IOError(ex, willThrow: true);
					throw ex;
				}
				bytesTransferred += bytesAlreadyTransferred;
				m_ChunkSize -= bytesTransferred;
			}
			else
			{
				bool flag = false;
				if (bytesTransferred <= 0)
				{
					if (m_ReadBytes != -1 && (bytesTransferred < 0 || !zeroLengthRead))
					{
						IOError(null, willThrow: false);
					}
					else
					{
						flag = true;
						bytesTransferred = 0;
					}
				}
				bytesTransferred += bytesAlreadyTransferred;
				if (m_ReadBytes != -1)
				{
					m_ReadBytes -= bytesTransferred;
				}
				if (m_ReadBytes == 0 || flag)
				{
					m_ReadBytes = 0L;
					CallDone();
				}
			}
			return bytesTransferred;
		}

		internal int ReadSingleByte()
		{
			if (ErrorInStream)
			{
				return -1;
			}
			if (m_ReadBufferSize != 0)
			{
				m_ReadBufferSize--;
				return m_ReadBuffer[m_ReadOffset++];
			}
			int num = m_Connection.Read(m_TempBuffer, 0, 1);
			if (num <= 0)
			{
				return -1;
			}
			return m_TempBuffer[0];
		}

		private int ReadCRLF(byte[] buffer)
		{
			int offset = 0;
			int size = NclConstants.CRLF.Length;
			int num = FillFromBufferedData(buffer, ref offset, ref size);
			if (num >= 0 && num != NclConstants.CRLF.Length)
			{
				do
				{
					int num2 = m_Connection.Read(buffer, offset, size);
					if (num2 <= 0)
					{
						throw new IOException(SR.GetString("net_io_readfailure", SR.GetString("net_io_connectionclosed")));
					}
					size -= num2;
					offset += num2;
				}
				while (size > 0);
			}
			return num;
		}

		private static void ReadChunkedCallback(object state)
		{
			NestedSingleAsyncResult nestedSingleAsyncResult = state as NestedSingleAsyncResult;
			ConnectStream connectStream = nestedSingleAsyncResult.AsyncObject as ConnectStream;
			try
			{
				if (!connectStream.m_Draining && connectStream.IsClosed)
				{
					Exception result = new WebException(NetRes.GetWebStatusString("net_requestaborted", WebExceptionStatus.ConnectionClosed), WebExceptionStatus.ConnectionClosed);
					nestedSingleAsyncResult.InvokeCallback(result);
					return;
				}
				if (connectStream.m_ErrorException != null)
				{
					nestedSingleAsyncResult.InvokeCallback(connectStream.m_ErrorException);
					return;
				}
				if (connectStream.m_ChunkedNeedCRLFRead)
				{
					connectStream.ReadCRLF(connectStream.m_TempBuffer);
					connectStream.m_ChunkedNeedCRLFRead = false;
				}
				StreamChunkBytes readByteBuffer = new StreamChunkBytes(connectStream);
				connectStream.m_ChunkSize = connectStream.ProcessReadChunkedSize(readByteBuffer);
				int size;
				int num;
				if (connectStream.m_ChunkSize != 0)
				{
					connectStream.m_ChunkedNeedCRLFRead = true;
					size = Math.Min(nestedSingleAsyncResult.Size, connectStream.m_ChunkSize);
					num = 0;
					if (connectStream.m_ReadBufferSize <= 0)
					{
						goto IL_00e7;
					}
					num = connectStream.FillFromBufferedData(nestedSingleAsyncResult.Buffer, ref nestedSingleAsyncResult.Offset, ref size);
					if (size != 0)
					{
						goto IL_00e7;
					}
					nestedSingleAsyncResult.InvokeCallback(num);
				}
				else
				{
					connectStream.ReadCRLF(connectStream.m_TempBuffer);
					connectStream.RemoveTrailers(readByteBuffer);
					connectStream.m_ReadBytes = 0L;
					connectStream.m_ChunkEofRecvd = true;
					connectStream.CallDone();
					nestedSingleAsyncResult.InvokeCallback(0);
				}
				goto end_IL_0013;
				IL_00e7:
				if (connectStream.ErrorInStream)
				{
					throw connectStream.m_ErrorException;
				}
				connectStream.m_BytesAlreadyTransferred = num;
				IAsyncResult asyncResult = connectStream.m_Connection.BeginRead(nestedSingleAsyncResult.Buffer, nestedSingleAsyncResult.Offset, size, m_ReadCallbackDelegate, nestedSingleAsyncResult);
				if (asyncResult == null)
				{
					connectStream.m_BytesAlreadyTransferred = 0;
					connectStream.m_ErrorException = new WebException(NetRes.GetWebStatusString("net_requestaborted", WebExceptionStatus.RequestCanceled), WebExceptionStatus.RequestCanceled);
					throw connectStream.m_ErrorException;
				}
				end_IL_0013:;
			}
			catch (Exception ex)
			{
				if (NclUtilities.IsFatal(ex))
				{
					throw;
				}
				nestedSingleAsyncResult.InvokeCallback(ex);
			}
		}

		private int ReadChunkedSync(byte[] buffer, int offset, int size)
		{
			if (!m_Draining && IsClosed)
			{
				Exception ex = new WebException(NetRes.GetWebStatusString("net_requestaborted", WebExceptionStatus.ConnectionClosed), WebExceptionStatus.ConnectionClosed);
				throw ex;
			}
			if (m_ErrorException != null)
			{
				throw m_ErrorException;
			}
			if (m_ChunkedNeedCRLFRead)
			{
				ReadCRLF(m_TempBuffer);
				m_ChunkedNeedCRLFRead = false;
			}
			StreamChunkBytes readByteBuffer = new StreamChunkBytes(this);
			m_ChunkSize = ProcessReadChunkedSize(readByteBuffer);
			if (m_ChunkSize != 0)
			{
				m_ChunkedNeedCRLFRead = true;
				return InternalRead(buffer, offset, Math.Min(size, m_ChunkSize));
			}
			ReadCRLF(m_TempBuffer);
			RemoveTrailers(readByteBuffer);
			m_ReadBytes = 0L;
			m_ChunkEofRecvd = true;
			CallDone();
			return 0;
		}

		private int ProcessReadChunkedSize(StreamChunkBytes ReadByteBuffer)
		{
			int chunkSize2 = ChunkParse.GetChunkSize(ReadByteBuffer, out var chunkSize);
			if (chunkSize2 <= 0)
			{
				throw new IOException(SR.GetString("net_io_readfailure", SR.GetString("net_io_connectionclosed")));
			}
			chunkSize2 = ChunkParse.SkipPastCRLF(ReadByteBuffer);
			if (chunkSize2 <= 0)
			{
				throw new IOException(SR.GetString("net_io_readfailure", SR.GetString("net_io_connectionclosed")));
			}
			return chunkSize;
		}

		private void RemoveTrailers(StreamChunkBytes ReadByteBuffer)
		{
			while (m_TempBuffer[0] != 13 && m_TempBuffer[1] != 10)
			{
				int num = ChunkParse.SkipPastCRLF(ReadByteBuffer);
				if (num <= 0)
				{
					throw new IOException(SR.GetString("net_io_readfailure", SR.GetString("net_io_connectionclosed")));
				}
				ReadCRLF(m_TempBuffer);
			}
		}

		private static void WriteHeadersCallback(IAsyncResult ar)
		{
			if (ar.CompletedSynchronously)
			{
				return;
			}
			WriteHeadersCallbackState writeHeadersCallbackState = (WriteHeadersCallbackState)ar.AsyncState;
			ConnectStream stream = writeHeadersCallbackState.stream;
			HttpWebRequest request = writeHeadersCallbackState.request;
			WebExceptionStatus webExceptionStatus = WebExceptionStatus.SendFailure;
			_ = request.WriteBuffer;
			try
			{
				stream.m_Connection.EndWrite(ar);
				stream.m_Connection.CheckStartReceive(request);
				if (stream.m_Connection.m_InnerException != null)
				{
					throw stream.m_Connection.m_InnerException;
				}
				webExceptionStatus = WebExceptionStatus.Success;
			}
			catch (Exception ex)
			{
				if (NclUtilities.IsFatal(ex))
				{
					throw;
				}
				if (ex is IOException || ex is ObjectDisposedException)
				{
					ex = ((stream.m_Connection.AtLeastOneResponseReceived || request.BodyStarted) ? new WebException(NetRes.GetWebStatusString("net_connclosed", webExceptionStatus), webExceptionStatus, stream.m_Connection.AtLeastOneResponseReceived ? WebExceptionInternalStatus.Isolated : WebExceptionInternalStatus.RequestFatal, ex) : new WebException(NetRes.GetWebStatusString("net_connclosed", webExceptionStatus), webExceptionStatus, WebExceptionInternalStatus.Recoverable, ex));
				}
				stream.IOError(ex, willThrow: false);
			}
			stream.ExchangeCallNesting(0, 4);
			request.WriteHeadersCallback(webExceptionStatus, stream, async: true);
		}

		internal void WriteHeaders(bool async)
		{
			WebExceptionStatus webExceptionStatus = WebExceptionStatus.SendFailure;
			if (!ErrorInStream)
			{
				byte[] writeBuffer = m_Request.WriteBuffer;
				try
				{
					Interlocked.CompareExchange(ref m_CallNesting, 4, 0);
					if (async)
					{
						WriteHeadersCallbackState writeHeadersCallbackState = new WriteHeadersCallbackState(m_Request, this);
						IAsyncResult asyncResult = m_Connection.UnsafeBeginWrite(writeBuffer, 0, writeBuffer.Length, m_WriteHeadersCallback, writeHeadersCallbackState);
						if (asyncResult.CompletedSynchronously)
						{
							m_Connection.EndWrite(asyncResult);
							m_Connection.CheckStartReceive(m_Request);
							webExceptionStatus = WebExceptionStatus.Success;
						}
						else
						{
							webExceptionStatus = WebExceptionStatus.Pending;
						}
					}
					else
					{
						SafeSetSocketTimeout(SocketShutdown.Send);
						m_Connection.Write(writeBuffer, 0, writeBuffer.Length);
						m_Connection.CheckStartReceive(m_Request);
						webExceptionStatus = WebExceptionStatus.Success;
					}
					if (Logging.On)
					{
						Logging.PrintInfo(Logging.Web, this, SR.GetString("net_log_sending_headers", m_Request.Headers.ToString(forTrace: true)));
					}
				}
				catch (Exception ex)
				{
					if (NclUtilities.IsFatal(ex))
					{
						throw;
					}
					if (ex is IOException || ex is ObjectDisposedException)
					{
						ex = ((m_Connection.AtLeastOneResponseReceived || m_Request.BodyStarted) ? new WebException(NetRes.GetWebStatusString("net_connclosed", webExceptionStatus), webExceptionStatus, m_Connection.AtLeastOneResponseReceived ? WebExceptionInternalStatus.Isolated : WebExceptionInternalStatus.RequestFatal, ex) : new WebException(NetRes.GetWebStatusString("net_connclosed", webExceptionStatus), webExceptionStatus, WebExceptionInternalStatus.Recoverable, ex));
					}
					IOError(ex, willThrow: false);
				}
				finally
				{
					if (webExceptionStatus != WebExceptionStatus.Pending)
					{
						Interlocked.CompareExchange(ref m_CallNesting, 0, 4);
					}
				}
			}
			if (webExceptionStatus != WebExceptionStatus.Pending)
			{
				m_Request.WriteHeadersCallback(webExceptionStatus, this, async);
			}
		}

		internal ChannelBinding GetChannelBinding(ChannelBindingKind kind)
		{
			ChannelBinding result = null;
			TlsStream tlsStream = m_Connection.NetworkStream as TlsStream;
			if (tlsStream != null)
			{
				result = tlsStream.GetChannelBinding(kind);
			}
			return result;
		}

		internal void PollAndRead(bool userRetrievedStream)
		{
			m_Connection.PollAndRead(m_Request, userRetrievedStream);
		}

		private void SafeSetSocketTimeout(SocketShutdown mode)
		{
			if (!Eof)
			{
				int timeout = ((mode != 0) ? WriteTimeout : ReadTimeout);
				m_Connection?.NetworkStream?.SetSocketTimeoutOption(mode, timeout, silent: false);
			}
		}

		protected override void Dispose(bool disposing)
		{
			try
			{
				if (Logging.On)
				{
					Logging.Enter(Logging.Web, this, "Close", "");
				}
				((ICloseEx)this).CloseEx(CloseExState.Normal);
				if (Logging.On)
				{
					Logging.Exit(Logging.Web, this, "Close", "");
				}
			}
			finally
			{
				base.Dispose(disposing);
			}
		}

		internal void CloseInternal(bool internalCall)
		{
			((ICloseEx)this).CloseEx(internalCall ? CloseExState.Silent : CloseExState.Normal);
		}

		void ICloseEx.CloseEx(CloseExState closeState)
		{
			CloseInternal((closeState & CloseExState.Silent) != 0, (closeState & CloseExState.Abort) != 0);
			GC.SuppressFinalize(this);
		}

		private void ResumeInternalClose(LazyAsyncResult userResult)
		{
			if (WriteChunked && !ErrorInStream && !m_IgnoreSocketErrors)
			{
				m_IgnoreSocketErrors = true;
				try
				{
					if (userResult != null)
					{
						m_Connection.BeginWrite(NclConstants.ChunkTerminator, 0, NclConstants.ChunkTerminator.Length, ResumeClose_Part2_Wrapper, userResult);
						return;
					}
					SafeSetSocketTimeout(SocketShutdown.Send);
					m_Connection.Write(NclConstants.ChunkTerminator, 0, NclConstants.ChunkTerminator.Length);
				}
				catch (Exception)
				{
				}
			}
			ResumeClose_Part2(userResult);
		}

		private void ResumeClose_Part2_Wrapper(IAsyncResult ar)
		{
			try
			{
				m_Connection.EndWrite(ar);
			}
			catch (Exception)
			{
			}
			ResumeClose_Part2((LazyAsyncResult)ar.AsyncState);
		}

		private void ResumeClose_Part2(LazyAsyncResult userResult)
		{
			try
			{
				try
				{
					if (ErrorInStream)
					{
						m_Connection.AbortSocket(isAbortState: true);
					}
				}
				finally
				{
					CallDone();
				}
			}
			catch
			{
			}
			finally
			{
				userResult?.InvokeCallback();
			}
		}

		private void CloseInternal(bool internalCall, bool aborting)
		{
			bool flag = !aborting;
			Exception ex = null;
			if (aborting)
			{
				if (Interlocked.Exchange(ref m_ShutDown, 777777) >= 777777)
				{
					return;
				}
			}
			else if (Interlocked.Increment(ref m_ShutDown) > 1)
			{
				return;
			}
			int num = ((IsPostStream && internalCall && !IgnoreSocketErrors && !BufferOnly && flag && !NclUtilities.HasShutdownStarted) ? 2 : 3);
			if (Interlocked.Exchange(ref m_CallNesting, num) == 1)
			{
				if (num == 2)
				{
					return;
				}
				flag &= !NclUtilities.HasShutdownStarted;
			}
			if (IgnoreSocketErrors && IsPostStream && !internalCall)
			{
				m_BytesLeftToWrite = 0L;
			}
			if (!IgnoreSocketErrors && flag)
			{
				if (!WriteStream)
				{
					flag = DrainSocket();
				}
				else
				{
					try
					{
						if (!ErrorInStream)
						{
							if (WriteChunked)
							{
								try
								{
									if (!m_IgnoreSocketErrors)
									{
										m_IgnoreSocketErrors = true;
										SafeSetSocketTimeout(SocketShutdown.Send);
										m_Connection.Write(NclConstants.ChunkTerminator, 0, NclConstants.ChunkTerminator.Length);
									}
								}
								catch
								{
								}
								m_BytesLeftToWrite = 0L;
							}
							else
							{
								if (BytesLeftToWrite > 0)
								{
									throw new IOException(SR.GetString("net_io_notenoughbyteswritten"));
								}
								if (BufferOnly)
								{
									m_BytesLeftToWrite = BufferedData.Length;
									m_Request.SwitchToContentLength();
									SafeSetSocketTimeout(SocketShutdown.Send);
									m_Request.NeedEndSubmitRequest();
									return;
								}
							}
						}
						else
						{
							flag = false;
						}
					}
					catch (Exception ex2)
					{
						flag = false;
						if (NclUtilities.IsFatal(ex2))
						{
							m_ErrorException = ex2;
							throw;
						}
						ex = new WebException(NetRes.GetWebStatusString("net_requestaborted", WebExceptionStatus.RequestCanceled), ex2, WebExceptionStatus.RequestCanceled, null);
					}
				}
			}
			if (!flag && m_DoneCalled == 0)
			{
				if (!aborting && Interlocked.Exchange(ref m_ShutDown, 777777) >= 777777)
				{
					return;
				}
				m_ErrorException = new WebException(NetRes.GetWebStatusString("net_requestaborted", WebExceptionStatus.RequestCanceled), WebExceptionStatus.RequestCanceled);
				m_Connection.AbortSocket(isAbortState: true);
				if (WriteStream)
				{
					m_Request?.Abort();
				}
				if (ex != null)
				{
					CallDone();
					if (!internalCall)
					{
						throw ex;
					}
				}
			}
			CallDone();
		}

		public override void Flush()
		{
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new NotSupportedException(SR.GetString("net_noseek"));
		}

		public override void SetLength(long value)
		{
			throw new NotSupportedException(SR.GetString("net_noseek"));
		}

		private bool DrainSocket()
		{
			if (IgnoreSocketErrors)
			{
				return true;
			}
			long readBytes = m_ReadBytes;
			if (!m_Chunked)
			{
				if (m_ReadBufferSize != 0)
				{
					m_ReadOffset += m_ReadBufferSize;
					if (m_ReadBytes != -1)
					{
						m_ReadBytes -= m_ReadBufferSize;
						if (m_ReadBytes < 0)
						{
							m_ReadBytes = 0L;
							return false;
						}
					}
					m_ReadBufferSize = 0;
					m_ReadBuffer = null;
				}
				if (readBytes == -1)
				{
					return true;
				}
			}
			if (Eof)
			{
				return true;
			}
			if (m_ReadBytes > 65536)
			{
				m_Connection.AbortSocket(isAbortState: false);
				return true;
			}
			m_Draining = true;
			int num;
			try
			{
				do
				{
					num = ReadWithoutValidation(s_DrainingBuffer, 0, s_DrainingBuffer.Length, abortOnError: false);
				}
				while (num > 0);
			}
			catch (Exception exception)
			{
				if (NclUtilities.IsFatal(exception))
				{
					throw;
				}
				num = -1;
			}
			return num > 0;
		}

		private void IOError(Exception exception)
		{
			IOError(exception, willThrow: true);
		}

		private void IOError(Exception exception, bool willThrow)
		{
			if (m_ErrorException == null)
			{
				if (exception == null)
				{
					Interlocked.CompareExchange(value: new IOException(WriteStream ? SR.GetString("net_io_writefailure", SR.GetString("net_io_connectionclosed")) : SR.GetString("net_io_readfailure", SR.GetString("net_io_connectionclosed"))), location1: ref m_ErrorException, comparand: null);
				}
				else
				{
					willThrow &= Interlocked.CompareExchange(ref m_ErrorException, exception, null) != null;
				}
			}
			m_ChunkEofRecvd = true;
			ConnectionReturnResult returnResult = null;
			if (WriteStream)
			{
				m_Connection.HandleConnectStreamException(writeDone: true, readDone: false, WebExceptionStatus.SendFailure, ref returnResult, m_ErrorException);
			}
			else
			{
				m_Connection.HandleConnectStreamException(writeDone: false, readDone: true, WebExceptionStatus.ReceiveFailure, ref returnResult, m_ErrorException);
			}
			CallDone(returnResult);
			if (willThrow)
			{
				throw m_ErrorException;
			}
		}

		internal static byte[] GetChunkHeader(int size, out int offset)
		{
			uint num = 4026531840u;
			byte[] array = new byte[10];
			offset = -1;
			int num2 = 0;
			while (num2 < 8)
			{
				if (offset != -1 || (size & num) != 0)
				{
					uint num3 = (uint)size >> 28;
					if (num3 < 10)
					{
						array[num2] = (byte)(num3 + 48);
					}
					else
					{
						array[num2] = (byte)(num3 - 10 + 65);
					}
					if (offset == -1)
					{
						offset = num2;
					}
				}
				num2++;
				size <<= 4;
			}
			array[8] = 13;
			array[9] = 10;
			return array;
		}
	}
}
