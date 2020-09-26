using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace System.Threading
{
	[ComVisible(true)]
	public class Overlapped
	{
		private OverlappedData m_overlappedData;

		public IAsyncResult AsyncResult
		{
			get
			{
				return m_overlappedData.m_asyncResult;
			}
			set
			{
				m_overlappedData.m_asyncResult = value;
			}
		}

		public int OffsetLow
		{
			get
			{
				return m_overlappedData.m_nativeOverlapped.OffsetLow;
			}
			set
			{
				m_overlappedData.m_nativeOverlapped.OffsetLow = value;
			}
		}

		public int OffsetHigh
		{
			get
			{
				return m_overlappedData.m_nativeOverlapped.OffsetHigh;
			}
			set
			{
				m_overlappedData.m_nativeOverlapped.OffsetHigh = value;
			}
		}

		[Obsolete("This property is not 64-bit compatible.  Use EventHandleIntPtr instead.  http://go.microsoft.com/fwlink/?linkid=14202")]
		public int EventHandle
		{
			get
			{
				return m_overlappedData.UserHandle.ToInt32();
			}
			set
			{
				m_overlappedData.UserHandle = new IntPtr(value);
			}
		}

		[ComVisible(false)]
		public IntPtr EventHandleIntPtr
		{
			get
			{
				return m_overlappedData.UserHandle;
			}
			set
			{
				m_overlappedData.UserHandle = value;
			}
		}

		internal _IOCompletionCallback iocbHelper => m_overlappedData.m_iocbHelper;

		internal unsafe IOCompletionCallback UserCallback => m_overlappedData.m_iocb;

		public Overlapped()
		{
			m_overlappedData = OverlappedDataCache.GetOverlappedData(this);
		}

		public Overlapped(int offsetLo, int offsetHi, IntPtr hEvent, IAsyncResult ar)
		{
			m_overlappedData = OverlappedDataCache.GetOverlappedData(this);
			m_overlappedData.m_nativeOverlapped.OffsetLow = offsetLo;
			m_overlappedData.m_nativeOverlapped.OffsetHigh = offsetHi;
			m_overlappedData.UserHandle = hEvent;
			m_overlappedData.m_asyncResult = ar;
		}

		[Obsolete("This constructor is not 64-bit compatible.  Use the constructor that takes an IntPtr for the event handle.  http://go.microsoft.com/fwlink/?linkid=14202")]
		public Overlapped(int offsetLo, int offsetHi, int hEvent, IAsyncResult ar)
			: this(offsetLo, offsetHi, new IntPtr(hEvent), ar)
		{
		}

		[CLSCompliant(false)]
		[Obsolete("This method is not safe.  Use Pack (iocb, userData) instead.  http://go.microsoft.com/fwlink/?linkid=14202")]
		public unsafe NativeOverlapped* Pack(IOCompletionCallback iocb)
		{
			return Pack(iocb, null);
		}

		[CLSCompliant(false)]
		[ComVisible(false)]
		public unsafe NativeOverlapped* Pack(IOCompletionCallback iocb, object userData)
		{
			return m_overlappedData.Pack(iocb, userData);
		}

		[CLSCompliant(false)]
		[Obsolete("This method is not safe.  Use UnsafePack (iocb, userData) instead.  http://go.microsoft.com/fwlink/?linkid=14202")]
		[SecurityPermission(SecurityAction.LinkDemand, ControlEvidence = true, ControlPolicy = true)]
		public unsafe NativeOverlapped* UnsafePack(IOCompletionCallback iocb)
		{
			return UnsafePack(iocb, null);
		}

		[ComVisible(false)]
		[CLSCompliant(false)]
		[SecurityPermission(SecurityAction.LinkDemand, ControlEvidence = true, ControlPolicy = true)]
		public unsafe NativeOverlapped* UnsafePack(IOCompletionCallback iocb, object userData)
		{
			return m_overlappedData.UnsafePack(iocb, userData);
		}

		[CLSCompliant(false)]
		public unsafe static Overlapped Unpack(NativeOverlapped* nativeOverlappedPtr)
		{
			if (nativeOverlappedPtr == null)
			{
				throw new ArgumentNullException("nativeOverlappedPtr");
			}
			return OverlappedData.GetOverlappedFromNative(nativeOverlappedPtr).m_overlapped;
		}

		[CLSCompliant(false)]
		public unsafe static void Free(NativeOverlapped* nativeOverlappedPtr)
		{
			if (nativeOverlappedPtr == null)
			{
				throw new ArgumentNullException("nativeOverlappedPtr");
			}
			Overlapped overlapped = OverlappedData.GetOverlappedFromNative(nativeOverlappedPtr).m_overlapped;
			OverlappedData.FreeNativeOverlapped(nativeOverlappedPtr);
			OverlappedData overlappedData = overlapped.m_overlappedData;
			overlapped.m_overlappedData = null;
			OverlappedDataCache.CacheOverlappedData(overlappedData);
		}
	}
}
