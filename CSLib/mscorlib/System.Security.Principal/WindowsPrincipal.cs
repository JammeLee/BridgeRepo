using System.Collections;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;

namespace System.Security.Principal
{
	[Serializable]
	[ComVisible(true)]
	[HostProtection(SecurityAction.LinkDemand, SecurityInfrastructure = true)]
	public class WindowsPrincipal : IPrincipal
	{
		private WindowsIdentity m_identity;

		private string[] m_roles;

		private Hashtable m_rolesTable;

		private bool m_rolesLoaded;

		public virtual IIdentity Identity => m_identity;

		private WindowsPrincipal()
		{
		}

		public WindowsPrincipal(WindowsIdentity ntIdentity)
		{
			if (ntIdentity == null)
			{
				throw new ArgumentNullException("ntIdentity");
			}
			m_identity = ntIdentity;
		}

		public virtual bool IsInRole(string role)
		{
			if (role == null || role.Length == 0)
			{
				return false;
			}
			NTAccount identity = new NTAccount(role);
			IdentityReferenceCollection identityReferenceCollection = new IdentityReferenceCollection(1);
			identityReferenceCollection.Add(identity);
			IdentityReferenceCollection identityReferenceCollection2 = NTAccount.Translate(identityReferenceCollection, typeof(SecurityIdentifier), forceSuccess: false);
			SecurityIdentifier securityIdentifier = identityReferenceCollection2[0] as SecurityIdentifier;
			if (securityIdentifier == null)
			{
				return false;
			}
			return IsInRole(securityIdentifier);
		}

		public virtual bool IsInRole(WindowsBuiltInRole role)
		{
			if (role < WindowsBuiltInRole.Administrator || role > WindowsBuiltInRole.Replicator)
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_EnumIllegalVal", (int)role), "role");
			}
			return IsInRole((int)role);
		}

		public virtual bool IsInRole(int rid)
		{
			SecurityIdentifier sid = new SecurityIdentifier(IdentifierAuthority.NTAuthority, new int[2]
			{
				32,
				rid
			});
			return IsInRole(sid);
		}

		[ComVisible(false)]
		public virtual bool IsInRole(SecurityIdentifier sid)
		{
			if (sid == null)
			{
				throw new ArgumentNullException("sid");
			}
			if (m_identity.TokenHandle.IsInvalid)
			{
				return false;
			}
			SafeTokenHandle phNewToken = SafeTokenHandle.InvalidHandle;
			if (m_identity.ImpersonationLevel == TokenImpersonationLevel.None && !Win32Native.DuplicateTokenEx(m_identity.TokenHandle, 8u, IntPtr.Zero, 2u, 2u, ref phNewToken))
			{
				throw new SecurityException(Win32Native.GetMessage(Marshal.GetLastWin32Error()));
			}
			bool IsMember = false;
			if (!Win32Native.CheckTokenMembership((m_identity.ImpersonationLevel != 0) ? m_identity.TokenHandle : phNewToken, sid.BinaryForm, ref IsMember))
			{
				throw new SecurityException(Win32Native.GetMessage(Marshal.GetLastWin32Error()));
			}
			phNewToken.Dispose();
			return IsMember;
		}
	}
}
