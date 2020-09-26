using System.Globalization;
using System.Reflection.Cache;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Security.Permissions;
using System.Threading;

namespace System
{
	public static class GC
	{
		private static ClearCacheHandler m_cacheHandler;

		private static readonly object locker = new object();

		public static int MaxGeneration => nativeGetMaxGeneration();

		internal static event ClearCacheHandler ClearCache
		{
			add
			{
				lock (locker)
				{
					m_cacheHandler = (ClearCacheHandler)Delegate.Combine(m_cacheHandler, value);
					SetCleanupCache();
				}
			}
			remove
			{
				lock (locker)
				{
					m_cacheHandler = (ClearCacheHandler)Delegate.Remove(m_cacheHandler, value);
				}
			}
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern int nativeGetGCLatencyMode();

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern void nativeSetGCLatencyMode(int newLatencyMode);

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern int GetGenerationWR(IntPtr handle);

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern long nativeGetTotalMemory();

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void nativeCollectGeneration(int generation, int mode);

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern int nativeGetMaxGeneration();

		[MethodImpl(MethodImplOptions.InternalCall)]
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		private static extern int nativeCollectionCount(int generation);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern bool nativeIsServerGC();

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern void nativeAddMemoryPressure(ulong bytesAllocated);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern void nativeRemoveMemoryPressure(ulong bytesAllocated);

		[SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
		public static void AddMemoryPressure(long bytesAllocated)
		{
			if (bytesAllocated <= 0)
			{
				throw new ArgumentOutOfRangeException("bytesAllocated", Environment.GetResourceString("ArgumentOutOfRange_NeedPosNum"));
			}
			if (4 == IntPtr.Size && bytesAllocated > int.MaxValue)
			{
				throw new ArgumentOutOfRangeException("pressure", Environment.GetResourceString("ArgumentOutOfRange_MustBeNonNegInt32"));
			}
			nativeAddMemoryPressure((ulong)bytesAllocated);
		}

		[SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
		public static void RemoveMemoryPressure(long bytesAllocated)
		{
			if (bytesAllocated <= 0)
			{
				throw new ArgumentOutOfRangeException("bytesAllocated", Environment.GetResourceString("ArgumentOutOfRange_NeedPosNum"));
			}
			if (4 == IntPtr.Size && bytesAllocated > int.MaxValue)
			{
				throw new ArgumentOutOfRangeException("bytesAllocated", Environment.GetResourceString("ArgumentOutOfRange_MustBeNonNegInt32"));
			}
			nativeRemoveMemoryPressure((ulong)bytesAllocated);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		public static extern int GetGeneration(object obj);

		public static void Collect(int generation)
		{
			Collect(generation, GCCollectionMode.Default);
		}

		public static void Collect()
		{
			nativeCollectGeneration(-1, 0);
		}

		public static void Collect(int generation, GCCollectionMode mode)
		{
			if (generation < 0)
			{
				throw new ArgumentOutOfRangeException("generation", Environment.GetResourceString("ArgumentOutOfRange_GenericPositive"));
			}
			if (mode < GCCollectionMode.Default || mode > GCCollectionMode.Optimized)
			{
				throw new ArgumentOutOfRangeException(Environment.GetResourceString("ArgumentOutOfRange_Enum"));
			}
			nativeCollectGeneration(generation, (int)mode);
		}

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		public static int CollectionCount(int generation)
		{
			if (generation < 0)
			{
				throw new ArgumentOutOfRangeException("generation", Environment.GetResourceString("ArgumentOutOfRange_GenericPositive"));
			}
			return nativeCollectionCount(generation);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		public static extern void KeepAlive(object obj);

		public static int GetGeneration(WeakReference wo)
		{
			int generationWR = GetGenerationWR(wo.m_handle);
			KeepAlive(wo);
			return generationWR;
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		public static extern void WaitForPendingFinalizers();

		[MethodImpl(MethodImplOptions.InternalCall)]
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		internal static extern void nativeSuppressFinalize(object o);

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		public static void SuppressFinalize(object obj)
		{
			if (obj == null)
			{
				throw new ArgumentNullException("obj");
			}
			nativeSuppressFinalize(obj);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void nativeReRegisterForFinalize(object o);

		public static void ReRegisterForFinalize(object obj)
		{
			if (obj == null)
			{
				throw new ArgumentNullException("obj");
			}
			nativeReRegisterForFinalize(obj);
		}

		public static long GetTotalMemory(bool forceFullCollection)
		{
			long num = nativeGetTotalMemory();
			if (!forceFullCollection)
			{
				return num;
			}
			int num2 = 20;
			long num3 = num;
			float num4;
			do
			{
				WaitForPendingFinalizers();
				Collect();
				num = num3;
				num3 = nativeGetTotalMemory();
				num4 = (float)(num3 - num) / (float)num;
			}
			while (num2-- > 0 && (!(-0.05 < (double)num4) || !((double)num4 < 0.05)));
			return num3;
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern bool nativeRegisterForFullGCNotification(int maxGenerationPercentage, int largeObjectHeapPercentage);

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern bool nativeCancelFullGCNotification();

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern int nativeWaitForFullGCApproach(int millisecondsTimeout);

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern int nativeWaitForFullGCComplete(int millisecondsTimeout);

		[PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
		public static void RegisterForFullGCNotification(int maxGenerationThreshold, int largeObjectHeapThreshold)
		{
			if (maxGenerationThreshold <= 0 || maxGenerationThreshold >= 100)
			{
				throw new ArgumentOutOfRangeException("maxGenerationThreshold", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("ArgumentOutOfRange_Bounds_Lower_Upper"), 1, 99));
			}
			if (largeObjectHeapThreshold <= 0 || largeObjectHeapThreshold >= 100)
			{
				throw new ArgumentOutOfRangeException("largeObjectHeapThreshold", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("ArgumentOutOfRange_Bounds_Lower_Upper"), 1, 99));
			}
			if (!nativeRegisterForFullGCNotification(maxGenerationThreshold, largeObjectHeapThreshold))
			{
				throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_NotWithConcurrentGC"));
			}
		}

		[PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
		public static void CancelFullGCNotification()
		{
			if (!nativeCancelFullGCNotification())
			{
				throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_NotWithConcurrentGC"));
			}
		}

		[PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
		public static GCNotificationStatus WaitForFullGCApproach()
		{
			return (GCNotificationStatus)nativeWaitForFullGCApproach(-1);
		}

		[PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
		public static GCNotificationStatus WaitForFullGCApproach(int millisecondsTimeout)
		{
			if (millisecondsTimeout < -1)
			{
				throw new ArgumentOutOfRangeException("millisecondsTimeout", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegOrNegative1"));
			}
			return (GCNotificationStatus)nativeWaitForFullGCApproach(millisecondsTimeout);
		}

		[PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
		public static GCNotificationStatus WaitForFullGCComplete()
		{
			return (GCNotificationStatus)nativeWaitForFullGCComplete(-1);
		}

		[PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
		public static GCNotificationStatus WaitForFullGCComplete(int millisecondsTimeout)
		{
			if (millisecondsTimeout < -1)
			{
				throw new ArgumentOutOfRangeException("millisecondsTimeout", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegOrNegative1"));
			}
			return (GCNotificationStatus)nativeWaitForFullGCComplete(millisecondsTimeout);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void SetCleanupCache();

		internal static void FireCacheEvent()
		{
			Interlocked.Exchange(ref m_cacheHandler, null)?.Invoke(null, null);
		}
	}
}
