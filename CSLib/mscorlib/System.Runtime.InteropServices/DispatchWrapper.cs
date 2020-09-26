using System.Security.Permissions;

namespace System.Runtime.InteropServices
{
	[Serializable]
	[ComVisible(true)]
	public sealed class DispatchWrapper
	{
		private object m_WrappedObject;

		public object WrappedObject => m_WrappedObject;

		[SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public DispatchWrapper(object obj)
		{
			if (obj != null)
			{
				IntPtr iDispatchForObject = Marshal.GetIDispatchForObject(obj);
				Marshal.Release(iDispatchForObject);
			}
			m_WrappedObject = obj;
		}
	}
}
