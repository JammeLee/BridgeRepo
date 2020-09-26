using System.Collections;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace System
{
	internal class __ComObject : MarshalByRefObject
	{
		private Hashtable m_ObjectToDataMap;

		private __ComObject()
		{
		}

		internal IntPtr GetIUnknown(out bool fIsURTAggregated)
		{
			fIsURTAggregated = !GetType().IsDefined(typeof(ComImportAttribute), inherit: false);
			return Marshal.GetIUnknownForObject(this);
		}

		internal object GetData(object key)
		{
			object result = null;
			lock (this)
			{
				if (m_ObjectToDataMap != null)
				{
					return m_ObjectToDataMap[key];
				}
				return result;
			}
		}

		internal bool SetData(object key, object data)
		{
			bool result = false;
			lock (this)
			{
				if (m_ObjectToDataMap == null)
				{
					m_ObjectToDataMap = new Hashtable();
				}
				if (m_ObjectToDataMap[key] == null)
				{
					m_ObjectToDataMap[key] = data;
					return true;
				}
				return result;
			}
		}

		internal void ReleaseAllData()
		{
			lock (this)
			{
				if (m_ObjectToDataMap == null)
				{
					return;
				}
				foreach (object value in m_ObjectToDataMap.Values)
				{
					(value as IDisposable)?.Dispose();
					__ComObject _ComObject = value as __ComObject;
					if (_ComObject != null)
					{
						Marshal.ReleaseComObject(_ComObject);
					}
				}
				m_ObjectToDataMap = null;
			}
		}

		internal object GetEventProvider(Type t)
		{
			object obj = GetData(t);
			if (obj == null)
			{
				obj = CreateEventProvider(t);
			}
			return obj;
		}

		internal int ReleaseSelf()
		{
			return Marshal.InternalReleaseComObject(this);
		}

		internal void FinalReleaseSelf()
		{
			Marshal.InternalFinalReleaseComObject(this);
		}

		[ReflectionPermission(SecurityAction.Assert, MemberAccess = true)]
		private object CreateEventProvider(Type t)
		{
			object obj = Activator.CreateInstance(t, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.CreateInstance, null, new object[1]
			{
				this
			}, null);
			if (!SetData(t, obj))
			{
				(obj as IDisposable)?.Dispose();
				obj = GetData(t);
			}
			return obj;
		}
	}
}
