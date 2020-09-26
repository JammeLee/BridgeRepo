using System.Security.Permissions;

namespace System.Security
{
	[Serializable]
	internal sealed class PermissionSetTriple
	{
		private static RuntimeMethodHandle s_emptyRMH = new RuntimeMethodHandle(null);

		private static PermissionToken s_zoneToken;

		private static PermissionToken s_urlToken;

		internal PermissionSet AssertSet;

		internal PermissionSet GrantSet;

		internal PermissionSet RefusedSet;

		private PermissionToken ZoneToken
		{
			get
			{
				if (s_zoneToken == null)
				{
					s_zoneToken = PermissionToken.GetToken(typeof(ZoneIdentityPermission));
				}
				return s_zoneToken;
			}
		}

		private PermissionToken UrlToken
		{
			get
			{
				if (s_urlToken == null)
				{
					s_urlToken = PermissionToken.GetToken(typeof(UrlIdentityPermission));
				}
				return s_urlToken;
			}
		}

		internal PermissionSetTriple()
		{
			Reset();
		}

		internal PermissionSetTriple(PermissionSetTriple triple)
		{
			AssertSet = triple.AssertSet;
			GrantSet = triple.GrantSet;
			RefusedSet = triple.RefusedSet;
		}

		internal void Reset()
		{
			AssertSet = null;
			GrantSet = null;
			RefusedSet = null;
		}

		internal bool IsEmpty()
		{
			if (AssertSet == null && GrantSet == null)
			{
				return RefusedSet == null;
			}
			return false;
		}

		internal bool Update(PermissionSetTriple psTriple, out PermissionSetTriple retTriple)
		{
			retTriple = null;
			retTriple = UpdateAssert(psTriple.AssertSet);
			if (psTriple.AssertSet != null && psTriple.AssertSet.IsUnrestricted())
			{
				return true;
			}
			UpdateGrant(psTriple.GrantSet);
			UpdateRefused(psTriple.RefusedSet);
			return false;
		}

		internal PermissionSetTriple UpdateAssert(PermissionSet in_a)
		{
			PermissionSetTriple permissionSetTriple = null;
			if (in_a != null)
			{
				if (in_a.IsSubsetOf(AssertSet))
				{
					return null;
				}
				PermissionSet permissionSet;
				if (GrantSet != null)
				{
					permissionSet = in_a.Intersect(GrantSet);
				}
				else
				{
					GrantSet = new PermissionSet(fUnrestricted: true);
					permissionSet = in_a.Copy();
				}
				bool bFailedToCompress = false;
				if (RefusedSet != null)
				{
					permissionSet = PermissionSet.RemoveRefusedPermissionSet(permissionSet, RefusedSet, out bFailedToCompress);
				}
				if (!bFailedToCompress)
				{
					bFailedToCompress = PermissionSet.IsIntersectingAssertedPermissions(permissionSet, AssertSet);
				}
				if (bFailedToCompress)
				{
					permissionSetTriple = new PermissionSetTriple(this);
					Reset();
					GrantSet = permissionSetTriple.GrantSet.Copy();
				}
				if (AssertSet == null)
				{
					AssertSet = permissionSet;
				}
				else
				{
					AssertSet.InplaceUnion(permissionSet);
				}
			}
			return permissionSetTriple;
		}

		internal void UpdateGrant(PermissionSet in_g, out ZoneIdentityPermission z, out UrlIdentityPermission u)
		{
			z = null;
			u = null;
			if (in_g != null)
			{
				if (GrantSet == null)
				{
					GrantSet = in_g.Copy();
				}
				else
				{
					GrantSet.InplaceIntersect(in_g);
				}
				z = (ZoneIdentityPermission)in_g.GetPermission(ZoneToken);
				u = (UrlIdentityPermission)in_g.GetPermission(UrlToken);
			}
		}

		internal void UpdateGrant(PermissionSet in_g)
		{
			if (in_g != null)
			{
				if (GrantSet == null)
				{
					GrantSet = in_g.Copy();
				}
				else
				{
					GrantSet.InplaceIntersect(in_g);
				}
			}
		}

		internal void UpdateRefused(PermissionSet in_r)
		{
			if (in_r != null)
			{
				if (RefusedSet == null)
				{
					RefusedSet = in_r.Copy();
				}
				else
				{
					RefusedSet.InplaceUnion(in_r);
				}
			}
		}

		private static bool CheckAssert(PermissionSet pSet, CodeAccessPermission demand, PermissionToken permToken)
		{
			if (pSet != null)
			{
				pSet.CheckDecoded(demand, permToken);
				CodeAccessPermission asserted = (CodeAccessPermission)pSet.GetPermission(demand);
				try
				{
					if ((pSet.IsUnrestricted() && demand.CanUnrestrictedOverride()) || demand.CheckAssert(asserted))
					{
						return false;
					}
				}
				catch (ArgumentException)
				{
				}
			}
			return true;
		}

		private static bool CheckAssert(PermissionSet assertPset, PermissionSet demandSet, out PermissionSet newDemandSet)
		{
			newDemandSet = null;
			if (assertPset != null)
			{
				assertPset.CheckDecoded(demandSet);
				if (demandSet.CheckAssertion(assertPset))
				{
					return false;
				}
				PermissionSet.RemoveAssertedPermissionSet(demandSet, assertPset, out newDemandSet);
			}
			return true;
		}

		internal bool CheckDemand(CodeAccessPermission demand, PermissionToken permToken, RuntimeMethodHandle rmh)
		{
			if (!CheckAssert(AssertSet, demand, permToken))
			{
				return false;
			}
			CodeAccessSecurityEngine.CheckHelper(GrantSet, RefusedSet, demand, permToken, rmh, null, SecurityAction.Demand, throwException: true);
			return true;
		}

		internal bool CheckSetDemand(PermissionSet demandSet, out PermissionSet alteredDemandset, RuntimeMethodHandle rmh)
		{
			alteredDemandset = null;
			if (!CheckAssert(AssertSet, demandSet, out alteredDemandset))
			{
				return false;
			}
			if (alteredDemandset != null)
			{
				demandSet = alteredDemandset;
			}
			CodeAccessSecurityEngine.CheckSetHelper(GrantSet, RefusedSet, demandSet, rmh, null, SecurityAction.Demand, throwException: true);
			return true;
		}

		internal bool CheckDemandNoThrow(CodeAccessPermission demand, PermissionToken permToken)
		{
			return CodeAccessSecurityEngine.CheckHelper(GrantSet, RefusedSet, demand, permToken, s_emptyRMH, null, SecurityAction.Demand, throwException: false);
		}

		internal bool CheckSetDemandNoThrow(PermissionSet demandSet)
		{
			return CodeAccessSecurityEngine.CheckSetHelper(GrantSet, RefusedSet, demandSet, s_emptyRMH, null, SecurityAction.Demand, throwException: false);
		}

		internal bool CheckFlags(ref int flags)
		{
			if (AssertSet != null)
			{
				int specialFlags = SecurityManager.GetSpecialFlags(AssertSet, null);
				if ((flags & specialFlags) != 0)
				{
					flags &= ~specialFlags;
				}
			}
			return (SecurityManager.GetSpecialFlags(GrantSet, RefusedSet) & flags) == flags;
		}
	}
}
