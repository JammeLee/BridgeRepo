using System.Collections;
using System.Threading;

namespace System.Runtime.InteropServices
{
	internal class GCHandleCookieTable
	{
		private const int MaxListSize = 16777215;

		private const uint CookieMaskIndex = 16777215u;

		private const uint CookieMaskSentinal = 4278190080u;

		private Hashtable m_HandleToCookieMap;

		private IntPtr[] m_HandleList;

		private byte[] m_CycleCounts;

		private int m_FreeIndex;

		internal GCHandleCookieTable()
		{
			m_HandleList = new IntPtr[10];
			m_CycleCounts = new byte[10];
			m_HandleToCookieMap = new Hashtable();
			m_FreeIndex = 1;
			for (int i = 0; i < 10; i++)
			{
				ref IntPtr reference = ref m_HandleList[i];
				reference = IntPtr.Zero;
				m_CycleCounts[i] = 0;
			}
		}

		internal IntPtr FindOrAddHandle(IntPtr handle)
		{
			if (handle == IntPtr.Zero)
			{
				return IntPtr.Zero;
			}
			object obj = null;
			obj = m_HandleToCookieMap[handle];
			if (obj != null)
			{
				return (IntPtr)obj;
			}
			IntPtr intPtr = IntPtr.Zero;
			int i = m_FreeIndex;
			if (i < m_HandleList.Length && m_HandleList[i] == IntPtr.Zero && Interlocked.CompareExchange(ref m_HandleList[i], handle, IntPtr.Zero) == IntPtr.Zero)
			{
				intPtr = GetCookieFromData((uint)i, m_CycleCounts[i]);
				if (i + 1 < m_HandleList.Length)
				{
					m_FreeIndex = i + 1;
				}
			}
			if (intPtr == IntPtr.Zero)
			{
				for (i = 1; i < 16777215; i++)
				{
					if (m_HandleList[i] == IntPtr.Zero && Interlocked.CompareExchange(ref m_HandleList[i], handle, IntPtr.Zero) == IntPtr.Zero)
					{
						intPtr = GetCookieFromData((uint)i, m_CycleCounts[i]);
						if (i + 1 < m_HandleList.Length)
						{
							m_FreeIndex = i + 1;
						}
						break;
					}
					if (i + 1 < m_HandleList.Length)
					{
						continue;
					}
					lock (this)
					{
						if (i + 1 >= m_HandleList.Length)
						{
							GrowArrays();
						}
					}
				}
			}
			if (intPtr == IntPtr.Zero)
			{
				throw new OutOfMemoryException(Environment.GetResourceString("OutOfMemory_GCHandleMDA"));
			}
			lock (this)
			{
				obj = m_HandleToCookieMap[handle];
				if (obj != null)
				{
					ref IntPtr reference = ref m_HandleList[i];
					reference = IntPtr.Zero;
					return (IntPtr)obj;
				}
				m_HandleToCookieMap[handle] = intPtr;
				return intPtr;
			}
		}

		internal IntPtr GetHandle(IntPtr cookie)
		{
			IntPtr zero = IntPtr.Zero;
			if (!ValidateCookie(cookie))
			{
				return IntPtr.Zero;
			}
			return m_HandleList[GetIndexFromCookie(cookie)];
		}

		internal void RemoveHandleIfPresent(IntPtr handle)
		{
			if (handle == IntPtr.Zero)
			{
				return;
			}
			object obj = m_HandleToCookieMap[handle];
			if (obj != null)
			{
				IntPtr cookie = (IntPtr)obj;
				if (ValidateCookie(cookie))
				{
					int indexFromCookie = GetIndexFromCookie(cookie);
					m_CycleCounts[indexFromCookie]++;
					ref IntPtr reference = ref m_HandleList[indexFromCookie];
					reference = IntPtr.Zero;
					m_HandleToCookieMap.Remove(handle);
					m_FreeIndex = indexFromCookie;
				}
			}
		}

		private bool ValidateCookie(IntPtr cookie)
		{
			GetDataFromCookie(cookie, out var index, out var xorData);
			if (index >= 16777215)
			{
				return false;
			}
			if (index >= m_HandleList.Length)
			{
				return false;
			}
			if (m_HandleList[index] == IntPtr.Zero)
			{
				return false;
			}
			byte b = (byte)(AppDomain.CurrentDomain.Id % 255);
			byte b2 = (byte)(m_CycleCounts[index] ^ b);
			if (xorData != b2)
			{
				return false;
			}
			return true;
		}

		private void GrowArrays()
		{
			int num = m_HandleList.Length;
			IntPtr[] array = new IntPtr[num * 2];
			byte[] array2 = new byte[num * 2];
			Array.Copy(m_HandleList, array, num);
			Array.Copy(m_CycleCounts, array2, num);
			m_HandleList = array;
			m_CycleCounts = array2;
		}

		private IntPtr GetCookieFromData(uint index, byte cycleCount)
		{
			byte b = (byte)(AppDomain.CurrentDomain.Id % 255);
			return (IntPtr)(((cycleCount ^ b) << 24) + index + 1);
		}

		private void GetDataFromCookie(IntPtr cookie, out int index, out byte xorData)
		{
			uint num = (uint)(int)cookie;
			index = (int)((num & 0xFFFFFF) - 1);
			xorData = (byte)((num & 0xFF000000u) >> 24);
		}

		private int GetIndexFromCookie(IntPtr cookie)
		{
			uint num = (uint)(int)cookie;
			return (int)((num & 0xFFFFFF) - 1);
		}
	}
}
