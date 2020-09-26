using System.Security.Permissions;

namespace System.Runtime.InteropServices
{
	[Serializable]
	[ComVisible(true)]
	public sealed class BStrWrapper
	{
		private string m_WrappedObject;

		public string WrappedObject => m_WrappedObject;

		[SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public BStrWrapper(string value)
		{
			m_WrappedObject = value;
		}
	}
}
