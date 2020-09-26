using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace System.Threading
{
	internal sealed class OverlappedData : CriticalFinalizerObject
	{
		internal IAsyncResult m_asyncResult;

		internal IOCompletionCallback m_iocb;

		internal _IOCompletionCallback m_iocbHelper;

		internal Overlapped m_overlapped;

		private object m_userObject;

		internal OverlappedDataCacheLine m_cacheLine;

		private IntPtr m_pinSelf;

		private IntPtr m_userObjectInternal;

		private int m_AppDomainId;

		internal short m_slot;

		private byte m_isArray;

		private byte m_toBeCleaned;

		internal NativeOverlapped m_nativeOverlapped;

		[ComVisible(false)]
		internal IntPtr UserHandle
		{
			get
			{
				return m_nativeOverlapped.EventHandle;
			}
			set
			{
				m_nativeOverlapped.EventHandle = value;
			}
		}

		internal OverlappedData(OverlappedDataCacheLine cacheLine)
		{
			m_cacheLine = cacheLine;
		}

		~OverlappedData()
		{
			if (m_cacheLine != null && (!m_cacheLine.Removed && (!Environment.HasShutdownStarted && !AppDomain.CurrentDomain.IsFinalizingForUnload())))
			{
				OverlappedDataCache.CacheOverlappedData(this);
				GC.ReRegisterForFinalize(this);
			}
		}

		internal unsafe void ReInitialize()
		{
			m_asyncResult = null;
			m_iocb = null;
			m_iocbHelper = null;
			m_overlapped = null;
			m_userObject = null;
			m_pinSelf = (IntPtr)0;
			m_userObjectInternal = (IntPtr)0;
			m_AppDomainId = 0;
			m_nativeOverlapped.EventHandle = (IntPtr)0;
			m_isArray = 0;
			m_nativeOverlapped.InternalHigh = (IntPtr)0;
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		internal unsafe NativeOverlapped* Pack(IOCompletionCallback iocb, object userData)
		{
			if (!m_pinSelf.IsNull())
			{
				throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_Overlapped_Pack"));
			}
			StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
			if (iocb != null)
			{
				m_iocbHelper = new _IOCompletionCallback(iocb, ref stackMark);
				m_iocb = iocb;
			}
			else
			{
				m_iocbHelper = null;
				m_iocb = null;
			}
			m_userObject = userData;
			if (m_userObject != null)
			{
				if (m_userObject.GetType() == typeof(object[]))
				{
					m_isArray = 1;
				}
				else
				{
					m_isArray = 0;
				}
			}
			return AllocateNativeOverlapped();
		}

		[SecurityPermission(SecurityAction.LinkDemand, ControlEvidence = true, ControlPolicy = true)]
		internal unsafe NativeOverlapped* UnsafePack(IOCompletionCallback iocb, object userData)
		{
			if (!m_pinSelf.IsNull())
			{
				throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_Overlapped_Pack"));
			}
			m_userObject = userData;
			if (m_userObject != null)
			{
				if (m_userObject.GetType() == typeof(object[]))
				{
					m_isArray = 1;
				}
				else
				{
					m_isArray = 0;
				}
			}
			m_iocb = iocb;
			m_iocbHelper = null;
			return AllocateNativeOverlapped();
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private unsafe extern NativeOverlapped* AllocateNativeOverlapped();

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal unsafe static extern void FreeNativeOverlapped(NativeOverlapped* nativeOverlappedPtr);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal unsafe static extern OverlappedData GetOverlappedFromNative(NativeOverlapped* nativeOverlappedPtr);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal unsafe static extern void CheckVMForIOPacket(out NativeOverlapped* pOVERLAP, out uint errorCode, out uint numBytes);
	}
}
