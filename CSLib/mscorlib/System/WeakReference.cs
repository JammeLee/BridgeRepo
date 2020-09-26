using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Threading;

namespace System
{
	[Serializable]
	[ComVisible(true)]
	[SecurityPermission(SecurityAction.InheritanceDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
	public class WeakReference : ISerializable
	{
		internal volatile IntPtr m_handle;

		internal bool m_IsLongReference;

		public virtual bool IsAlive
		{
			get
			{
				IntPtr handle = m_handle;
				if (IntPtr.Zero == handle)
				{
					return false;
				}
				bool result = GCHandle.InternalGet(handle) != null;
				if (!(m_handle == IntPtr.Zero))
				{
					return result;
				}
				return false;
			}
		}

		public virtual bool TrackResurrection => m_IsLongReference;

		public virtual object Target
		{
			get
			{
				IntPtr handle = m_handle;
				if (IntPtr.Zero == handle)
				{
					return null;
				}
				object result = GCHandle.InternalGet(handle);
				if (!(m_handle == IntPtr.Zero))
				{
					return result;
				}
				return null;
			}
			set
			{
				IntPtr handle = m_handle;
				if (handle == IntPtr.Zero)
				{
					throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_HandleIsNotInitialized"));
				}
				object oldValue = GCHandle.InternalGet(handle);
				handle = m_handle;
				if (handle == IntPtr.Zero)
				{
					throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_HandleIsNotInitialized"));
				}
				GCHandle.InternalCompareExchange(handle, value, oldValue, isPinned: false);
				GC.KeepAlive(this);
			}
		}

		public WeakReference(object target)
			: this(target, trackResurrection: false)
		{
		}

		public WeakReference(object target, bool trackResurrection)
		{
			m_IsLongReference = trackResurrection;
			m_handle = GCHandle.InternalAlloc(target, trackResurrection ? GCHandleType.WeakTrackResurrection : GCHandleType.Weak);
		}

		protected WeakReference(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			object value = info.GetValue("TrackedObject", typeof(object));
			m_IsLongReference = info.GetBoolean("TrackResurrection");
			m_handle = GCHandle.InternalAlloc(value, m_IsLongReference ? GCHandleType.WeakTrackResurrection : GCHandleType.Weak);
		}

		~WeakReference()
		{
			IntPtr handle = m_handle;
			if (handle != IntPtr.Zero && handle == Interlocked.CompareExchange(ref m_handle, IntPtr.Zero, handle))
			{
				GCHandle.InternalFree(handle);
			}
		}

		public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			info.AddValue("TrackedObject", Target, typeof(object));
			info.AddValue("TrackResurrection", m_IsLongReference);
		}
	}
}
