using System.Runtime.CompilerServices;
using System.Security.Permissions;
using System.Threading;

namespace System.Runtime.InteropServices
{
	[ComVisible(true)]
	public struct GCHandle
	{
		internal static readonly IntPtr InvalidCookie;

		private IntPtr m_handle;

		private static GCHandleCookieTable s_cookieTable;

		private static bool s_probeIsActive;

		public object Target
		{
			[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
			get
			{
				if (m_handle == IntPtr.Zero)
				{
					throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_HandleIsNotInitialized"));
				}
				return InternalGet(GetHandleValue());
			}
			[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
			set
			{
				if (m_handle == IntPtr.Zero)
				{
					throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_HandleIsNotInitialized"));
				}
				InternalSet(GetHandleValue(), value, IsPinned());
			}
		}

		public bool IsAllocated => m_handle != IntPtr.Zero;

		static GCHandle()
		{
			InvalidCookie = new IntPtr(-1);
			s_cookieTable = null;
			s_probeIsActive = false;
			s_probeIsActive = Mda.IsInvalidGCHandleCookieProbeEnabled();
			if (s_probeIsActive)
			{
				s_cookieTable = new GCHandleCookieTable();
			}
		}

		internal GCHandle(object value, GCHandleType type)
		{
			m_handle = InternalAlloc(value, type);
			if (type == GCHandleType.Pinned)
			{
				SetIsPinned();
			}
		}

		internal GCHandle(IntPtr handle)
		{
			InternalCheckDomain(handle);
			m_handle = handle;
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static GCHandle Alloc(object value)
		{
			return new GCHandle(value, GCHandleType.Normal);
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static GCHandle Alloc(object value, GCHandleType type)
		{
			return new GCHandle(value, type);
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public void Free()
		{
			IntPtr handle = m_handle;
			if (handle != IntPtr.Zero && Interlocked.CompareExchange(ref m_handle, IntPtr.Zero, handle) == handle)
			{
				InternalFree((IntPtr)((int)handle & -2));
				if (s_probeIsActive)
				{
					s_cookieTable.RemoveHandleIfPresent(handle);
				}
				return;
			}
			throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_HandleIsNotInitialized"));
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public IntPtr AddrOfPinnedObject()
		{
			if (!IsPinned())
			{
				if (m_handle == IntPtr.Zero)
				{
					throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_HandleIsNotInitialized"));
				}
				throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_HandleIsNotPinned"));
			}
			return InternalAddrOfPinnedObject(GetHandleValue());
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static explicit operator GCHandle(IntPtr value)
		{
			return FromIntPtr(value);
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public static GCHandle FromIntPtr(IntPtr value)
		{
			if (value == IntPtr.Zero)
			{
				throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_HandleIsNotInitialized"));
			}
			IntPtr intPtr = value;
			if (s_probeIsActive)
			{
				intPtr = s_cookieTable.GetHandle(value);
				if (IntPtr.Zero == intPtr)
				{
					Mda.FireInvalidGCHandleCookieProbe(value);
					return new GCHandle(IntPtr.Zero);
				}
			}
			return new GCHandle(intPtr);
		}

		public static explicit operator IntPtr(GCHandle value)
		{
			return ToIntPtr(value);
		}

		public static IntPtr ToIntPtr(GCHandle value)
		{
			if (s_probeIsActive)
			{
				return s_cookieTable.FindOrAddHandle(value.m_handle);
			}
			return value.m_handle;
		}

		public override int GetHashCode()
		{
			return (int)m_handle;
		}

		public override bool Equals(object o)
		{
			if (o == null || !(o is GCHandle))
			{
				return false;
			}
			GCHandle gCHandle = (GCHandle)o;
			return m_handle == gCHandle.m_handle;
		}

		public static bool operator ==(GCHandle a, GCHandle b)
		{
			return a.m_handle == b.m_handle;
		}

		public static bool operator !=(GCHandle a, GCHandle b)
		{
			return a.m_handle != b.m_handle;
		}

		internal IntPtr GetHandleValue()
		{
			return new IntPtr((int)m_handle & -2);
		}

		internal bool IsPinned()
		{
			return ((int)m_handle & 1) != 0;
		}

		internal void SetIsPinned()
		{
			m_handle = new IntPtr((int)m_handle | 1);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern IntPtr InternalAlloc(object value, GCHandleType type);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern void InternalFree(IntPtr handle);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern object InternalGet(IntPtr handle);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern void InternalSet(IntPtr handle, object value, bool isPinned);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern object InternalCompareExchange(IntPtr handle, object value, object oldValue, bool isPinned);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern IntPtr InternalAddrOfPinnedObject(IntPtr handle);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern void InternalCheckDomain(IntPtr handle);
	}
}
