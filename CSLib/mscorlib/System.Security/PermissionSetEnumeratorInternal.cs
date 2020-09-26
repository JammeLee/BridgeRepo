using System.Security.Util;

namespace System.Security
{
	internal struct PermissionSetEnumeratorInternal
	{
		private PermissionSet m_permSet;

		private TokenBasedSetEnumerator enm;

		public object Current => enm.Current;

		internal PermissionSetEnumeratorInternal(PermissionSet permSet)
		{
			m_permSet = permSet;
			enm = new TokenBasedSetEnumerator(permSet.m_permSet);
		}

		public int GetCurrentIndex()
		{
			return enm.Index;
		}

		public void Reset()
		{
			enm.Reset();
		}

		public bool MoveNext()
		{
			while (enm.MoveNext())
			{
				object current = enm.Current;
				IPermission permission = current as IPermission;
				if (permission != null)
				{
					enm.Current = permission;
					return true;
				}
				SecurityElement securityElement = current as SecurityElement;
				if (securityElement != null)
				{
					permission = m_permSet.CreatePermission(securityElement, enm.Index);
					if (permission != null)
					{
						enm.Current = permission;
						return true;
					}
				}
			}
			return false;
		}
	}
}
