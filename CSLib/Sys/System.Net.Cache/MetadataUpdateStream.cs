using System.Collections.Specialized;
using System.IO;
using System.Threading;

namespace System.Net.Cache
{
	internal class MetadataUpdateStream : Stream, ICloseEx
	{
		private Stream m_ParentStream;

		private RequestCache m_Cache;

		private string m_Key;

		private DateTime m_Expires;

		private DateTime m_LastModified;

		private DateTime m_LastSynchronized;

		private TimeSpan m_MaxStale;

		private StringCollection m_EntryMetadata;

		private StringCollection m_SystemMetadata;

		private bool m_CacheDestroy;

		private bool m_IsStrictCacheErrors;

		private int _Disposed;

		public override bool CanRead => m_ParentStream.CanRead;

		public override bool CanSeek => m_ParentStream.CanSeek;

		public override bool CanWrite => m_ParentStream.CanWrite;

		public override long Length => m_ParentStream.Length;

		public override long Position
		{
			get
			{
				return m_ParentStream.Position;
			}
			set
			{
				m_ParentStream.Position = value;
			}
		}

		public override bool CanTimeout => m_ParentStream.CanTimeout;

		public override int ReadTimeout
		{
			get
			{
				return m_ParentStream.ReadTimeout;
			}
			set
			{
				m_ParentStream.ReadTimeout = value;
			}
		}

		public override int WriteTimeout
		{
			get
			{
				return m_ParentStream.WriteTimeout;
			}
			set
			{
				m_ParentStream.WriteTimeout = value;
			}
		}

		internal MetadataUpdateStream(Stream parentStream, RequestCache cache, string key, DateTime expiresGMT, DateTime lastModifiedGMT, DateTime lastSynchronizedGMT, TimeSpan maxStale, StringCollection entryMetadata, StringCollection systemMetadata, bool isStrictCacheErrors)
		{
			if (parentStream == null)
			{
				throw new ArgumentNullException("parentStream");
			}
			m_ParentStream = parentStream;
			m_Cache = cache;
			m_Key = key;
			m_Expires = expiresGMT;
			m_LastModified = lastModifiedGMT;
			m_LastSynchronized = lastSynchronizedGMT;
			m_MaxStale = maxStale;
			m_EntryMetadata = entryMetadata;
			m_SystemMetadata = systemMetadata;
			m_IsStrictCacheErrors = isStrictCacheErrors;
		}

		private MetadataUpdateStream(Stream parentStream, RequestCache cache, string key, bool isStrictCacheErrors)
		{
			if (parentStream == null)
			{
				throw new ArgumentNullException("parentStream");
			}
			m_ParentStream = parentStream;
			m_Cache = cache;
			m_Key = key;
			m_CacheDestroy = true;
			m_IsStrictCacheErrors = isStrictCacheErrors;
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			return m_ParentStream.Seek(offset, origin);
		}

		public override void SetLength(long value)
		{
			m_ParentStream.SetLength(value);
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			m_ParentStream.Write(buffer, offset, count);
		}

		public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
		{
			return m_ParentStream.BeginWrite(buffer, offset, count, callback, state);
		}

		public override void EndWrite(IAsyncResult asyncResult)
		{
			m_ParentStream.EndWrite(asyncResult);
		}

		public override void Flush()
		{
			m_ParentStream.Flush();
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			return m_ParentStream.Read(buffer, offset, count);
		}

		public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
		{
			return m_ParentStream.BeginRead(buffer, offset, count, callback, state);
		}

		public override int EndRead(IAsyncResult asyncResult)
		{
			return m_ParentStream.EndRead(asyncResult);
		}

		protected sealed override void Dispose(bool disposing)
		{
			Dispose(disposing, CloseExState.Normal);
			GC.SuppressFinalize(this);
		}

		void ICloseEx.CloseEx(CloseExState closeState)
		{
			Dispose(disposing: true, closeState);
		}

		protected virtual void Dispose(bool disposing, CloseExState closeState)
		{
			if (Interlocked.Increment(ref _Disposed) == 1)
			{
				ICloseEx closeEx = m_ParentStream as ICloseEx;
				if (closeEx != null)
				{
					closeEx.CloseEx(closeState);
				}
				else
				{
					m_ParentStream.Close();
				}
				if (m_CacheDestroy)
				{
					if (m_IsStrictCacheErrors)
					{
						m_Cache.Remove(m_Key);
					}
					else
					{
						m_Cache.TryRemove(m_Key);
					}
				}
				else if (m_IsStrictCacheErrors)
				{
					m_Cache.Update(m_Key, m_Expires, m_LastModified, m_LastSynchronized, m_MaxStale, m_EntryMetadata, m_SystemMetadata);
				}
				else
				{
					m_Cache.TryUpdate(m_Key, m_Expires, m_LastModified, m_LastSynchronized, m_MaxStale, m_EntryMetadata, m_SystemMetadata);
				}
				if (!disposing)
				{
					m_Cache = null;
					m_Key = null;
					m_EntryMetadata = null;
					m_SystemMetadata = null;
				}
			}
			base.Dispose(disposing);
		}
	}
}
