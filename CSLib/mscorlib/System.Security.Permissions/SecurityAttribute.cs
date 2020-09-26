using System.Runtime.InteropServices;

namespace System.Security.Permissions
{
	[Serializable]
	[ComVisible(true)]
	[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Constructor | AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
	public abstract class SecurityAttribute : Attribute
	{
		internal SecurityAction m_action;

		internal bool m_unrestricted;

		public SecurityAction Action
		{
			get
			{
				return m_action;
			}
			set
			{
				m_action = value;
			}
		}

		public bool Unrestricted
		{
			get
			{
				return m_unrestricted;
			}
			set
			{
				m_unrestricted = value;
			}
		}

		protected SecurityAttribute(SecurityAction action)
		{
			m_action = action;
		}

		public abstract IPermission CreatePermission();

		internal static IntPtr FindSecurityAttributeTypeHandle(string typeName)
		{
			PermissionSet.s_fullTrust.Assert();
			return Type.GetType(typeName, throwOnError: false, ignoreCase: false)?.TypeHandle.Value ?? IntPtr.Zero;
		}
	}
}
