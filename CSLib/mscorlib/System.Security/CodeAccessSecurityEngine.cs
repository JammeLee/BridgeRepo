using System.Collections;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Permissions;
using System.Security.Policy;
using System.Threading;

namespace System.Security
{
	internal class CodeAccessSecurityEngine
	{
		internal static SecurityPermission AssertPermission;

		internal static PermissionToken AssertPermissionToken;

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern void SpecialDemand(PermissionType whatPermission, ref StackCrawlMark stackMark);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern bool DoesFullTrustMeanFullTrust();

		[Conditional("_DEBUG")]
		private static void DEBUG_OUT(string str)
		{
		}

		private CodeAccessSecurityEngine()
		{
		}

		static CodeAccessSecurityEngine()
		{
			AssertPermission = new SecurityPermission(SecurityPermissionFlag.Assertion);
			AssertPermissionToken = PermissionToken.GetToken(AssertPermission);
		}

		private static void ThrowSecurityException(Assembly asm, PermissionSet granted, PermissionSet refused, RuntimeMethodHandle rmh, SecurityAction action, object demand, IPermission permThatFailed)
		{
			AssemblyName asmName = null;
			Evidence asmEvidence = null;
			if (asm != null)
			{
				PermissionSet.s_fullTrust.Assert();
				asmName = asm.GetName();
				if (asm != Assembly.GetExecutingAssembly())
				{
					asmEvidence = asm.Evidence;
				}
			}
			throw SecurityException.MakeSecurityException(asmName, asmEvidence, granted, refused, rmh, action, demand, permThatFailed);
		}

		private static void ThrowSecurityException(object assemblyOrString, PermissionSet granted, PermissionSet refused, RuntimeMethodHandle rmh, SecurityAction action, object demand, IPermission permThatFailed)
		{
			if (assemblyOrString == null || assemblyOrString is Assembly)
			{
				ThrowSecurityException((Assembly)assemblyOrString, granted, refused, rmh, action, demand, permThatFailed);
				return;
			}
			AssemblyName asmName = new AssemblyName((string)assemblyOrString);
			throw SecurityException.MakeSecurityException(asmName, null, granted, refused, rmh, action, demand, permThatFailed);
		}

		private static void LazyCheckSetHelper(PermissionSet demands, IntPtr asmSecDesc, RuntimeMethodHandle rmh, Assembly assembly, SecurityAction action)
		{
			if (!demands.CanUnrestrictedOverride())
			{
				_GetGrantedPermissionSet(asmSecDesc, out var grants, out var refused);
				CheckSetHelper(grants, refused, demands, rmh, assembly, action, throwException: true);
			}
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern void _GetGrantedPermissionSet(IntPtr secDesc, out PermissionSet grants, out PermissionSet refused);

		internal static void CheckSetHelper(CompressedStack cs, PermissionSet grants, PermissionSet refused, PermissionSet demands, RuntimeMethodHandle rmh, Assembly asm, SecurityAction action)
		{
			if (cs != null)
			{
				cs.CheckSetDemand(demands, rmh);
			}
			else
			{
				CheckSetHelper(grants, refused, demands, rmh, asm, action, throwException: true);
			}
		}

		internal static bool CheckSetHelper(PermissionSet grants, PermissionSet refused, PermissionSet demands, RuntimeMethodHandle rmh, object assemblyOrString, SecurityAction action, bool throwException)
		{
			IPermission firstPermThatFailed = null;
			grants?.CheckDecoded(demands);
			refused?.CheckDecoded(demands);
			bool flag = SecurityManager._SetThreadSecurity(bThreadSecurity: false);
			try
			{
				if (!demands.CheckDemand(grants, out firstPermThatFailed))
				{
					if (!throwException)
					{
						return false;
					}
					ThrowSecurityException(assemblyOrString, grants, refused, rmh, action, demands, firstPermThatFailed);
				}
				if (!demands.CheckDeny(refused, out firstPermThatFailed))
				{
					if (!throwException)
					{
						return false;
					}
					ThrowSecurityException(assemblyOrString, grants, refused, rmh, action, demands, firstPermThatFailed);
				}
			}
			catch (SecurityException)
			{
				throw;
			}
			catch (Exception)
			{
				if (!throwException)
				{
					return false;
				}
				ThrowSecurityException(assemblyOrString, grants, refused, rmh, action, demands, firstPermThatFailed);
			}
			catch
			{
				return false;
			}
			finally
			{
				if (flag)
				{
					SecurityManager._SetThreadSecurity(bThreadSecurity: true);
				}
			}
			return true;
		}

		internal static void CheckHelper(CompressedStack cs, PermissionSet grantedSet, PermissionSet refusedSet, CodeAccessPermission demand, PermissionToken permToken, RuntimeMethodHandle rmh, Assembly asm, SecurityAction action)
		{
			if (cs != null)
			{
				cs.CheckDemand(demand, permToken, rmh);
			}
			else
			{
				CheckHelper(grantedSet, refusedSet, demand, permToken, rmh, asm, action, throwException: true);
			}
		}

		internal static bool CheckHelper(PermissionSet grantedSet, PermissionSet refusedSet, CodeAccessPermission demand, PermissionToken permToken, RuntimeMethodHandle rmh, object assemblyOrString, SecurityAction action, bool throwException)
		{
			if (permToken == null)
			{
				permToken = PermissionToken.GetToken(demand);
			}
			grantedSet?.CheckDecoded(permToken.m_index);
			refusedSet?.CheckDecoded(permToken.m_index);
			bool flag = SecurityManager._SetThreadSecurity(bThreadSecurity: false);
			try
			{
				if (grantedSet == null)
				{
					if (!throwException)
					{
						return false;
					}
					ThrowSecurityException(assemblyOrString, grantedSet, refusedSet, rmh, action, demand, demand);
				}
				else if (!grantedSet.IsUnrestricted() || !demand.CanUnrestrictedOverride())
				{
					CodeAccessPermission grant = (CodeAccessPermission)grantedSet.GetPermission(permToken);
					if (!demand.CheckDemand(grant))
					{
						if (!throwException)
						{
							return false;
						}
						ThrowSecurityException(assemblyOrString, grantedSet, refusedSet, rmh, action, demand, demand);
					}
				}
				if (refusedSet != null)
				{
					CodeAccessPermission codeAccessPermission = (CodeAccessPermission)refusedSet.GetPermission(permToken);
					if (codeAccessPermission != null && !codeAccessPermission.CheckDeny(demand))
					{
						if (!throwException)
						{
							return false;
						}
						ThrowSecurityException(assemblyOrString, grantedSet, refusedSet, rmh, action, demand, demand);
					}
					if (refusedSet.IsUnrestricted() && demand.CanUnrestrictedOverride())
					{
						if (!throwException)
						{
							return false;
						}
						ThrowSecurityException(assemblyOrString, grantedSet, refusedSet, rmh, action, demand, demand);
					}
				}
			}
			catch (SecurityException)
			{
				throw;
			}
			catch (Exception)
			{
				if (!throwException)
				{
					return false;
				}
				ThrowSecurityException(assemblyOrString, grantedSet, refusedSet, rmh, action, demand, demand);
			}
			catch
			{
				return false;
			}
			finally
			{
				if (flag)
				{
					SecurityManager._SetThreadSecurity(bThreadSecurity: true);
				}
			}
			return true;
		}

		private static void CheckGrantSetHelper(PermissionSet grantSet)
		{
			grantSet.CopyWithNoIdentityPermissions().Demand();
		}

		internal static void ReflectionTargetDemandHelper(PermissionType permission, PermissionSet targetGrant)
		{
			ReflectionTargetDemandHelper((int)permission, targetGrant);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		private static void ReflectionTargetDemandHelper(int permission, PermissionSet targetGrant)
		{
			StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
			CompressedStack compressedStack = CompressedStack.GetCompressedStack(ref stackMark);
			ReflectionTargetDemandHelper(permission, targetGrant, compressedStack);
		}

		private static void ReflectionTargetDemandHelper(int permission, PermissionSet targetGrant, Resolver accessContext)
		{
			ReflectionTargetDemandHelper(permission, targetGrant, accessContext.GetSecurityContext());
		}

		private static void ReflectionTargetDemandHelper(int permission, PermissionSet targetGrant, CompressedStack securityContext)
		{
			PermissionSet permissionSet = null;
			if (targetGrant == null)
			{
				permissionSet = new PermissionSet(PermissionState.Unrestricted);
			}
			else
			{
				permissionSet = targetGrant.CopyWithNoIdentityPermissions();
				permissionSet.AddPermission(new ReflectionPermission(ReflectionPermissionFlag.RestrictedMemberAccess));
			}
			securityContext.DemandFlagsOrGrantSet(1 << permission, permissionSet);
		}

		internal static void GetZoneAndOriginHelper(CompressedStack cs, PermissionSet grantSet, PermissionSet refusedSet, ArrayList zoneList, ArrayList originList)
		{
			if (cs != null)
			{
				cs.GetZoneAndOrigin(zoneList, originList, PermissionToken.GetToken(typeof(ZoneIdentityPermission)), PermissionToken.GetToken(typeof(UrlIdentityPermission)));
				return;
			}
			ZoneIdentityPermission zoneIdentityPermission = (ZoneIdentityPermission)grantSet.GetPermission(typeof(ZoneIdentityPermission));
			UrlIdentityPermission urlIdentityPermission = (UrlIdentityPermission)grantSet.GetPermission(typeof(UrlIdentityPermission));
			if (zoneIdentityPermission != null)
			{
				zoneList.Add(zoneIdentityPermission.SecurityZone);
			}
			if (urlIdentityPermission != null)
			{
				originList.Add(urlIdentityPermission.Url);
			}
		}

		internal static void GetZoneAndOrigin(ref StackCrawlMark mark, out ArrayList zone, out ArrayList origin)
		{
			zone = new ArrayList();
			origin = new ArrayList();
			GetZoneAndOriginInternal(zone, origin, ref mark);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void GetZoneAndOriginInternal(ArrayList zoneList, ArrayList originList, ref StackCrawlMark stackMark);

		internal static void CheckAssembly(Assembly asm, CodeAccessPermission demand)
		{
			if (SecurityManager._IsSecurityOn())
			{
				asm.nGetGrantSet(out var newGrant, out var newDenied);
				CheckHelper(newGrant, newDenied, demand, PermissionToken.GetToken(demand), RuntimeMethodHandle.EmptyHandle, asm, SecurityAction.Demand, throwException: true);
			}
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void Check(object demand, ref StackCrawlMark stackMark, bool isPermSet);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern bool QuickCheckForAllDemands();

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern bool AllDomainsHomogeneousWithNoStackModifiers();

		internal static void Check(CodeAccessPermission cap, ref StackCrawlMark stackMark)
		{
			Check(cap, ref stackMark, isPermSet: false);
		}

		internal static void Check(PermissionSet permSet, ref StackCrawlMark stackMark)
		{
			Check(permSet, ref stackMark, isPermSet: true);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern FrameSecurityDescriptor CheckNReturnSO(PermissionToken permToken, CodeAccessPermission demand, ref StackCrawlMark stackMark, int unrestrictedOverride, int create);

		internal static void Assert(CodeAccessPermission cap, ref StackCrawlMark stackMark)
		{
			FrameSecurityDescriptor frameSecurityDescriptor = CheckNReturnSO(AssertPermissionToken, AssertPermission, ref stackMark, 1, 1);
			if (frameSecurityDescriptor == null)
			{
				if (SecurityManager._IsSecurityOn())
				{
					throw new ExecutionEngineException(Environment.GetResourceString("ExecutionEngine_MissingSecurityDescriptor"));
				}
				return;
			}
			if (frameSecurityDescriptor.HasImperativeAsserts())
			{
				throw new SecurityException(Environment.GetResourceString("Security_MustRevertOverride"));
			}
			frameSecurityDescriptor.SetAssert(cap);
		}

		internal static void Deny(CodeAccessPermission cap, ref StackCrawlMark stackMark)
		{
			FrameSecurityDescriptor securityObjectForFrame = SecurityRuntime.GetSecurityObjectForFrame(ref stackMark, create: true);
			if (securityObjectForFrame == null)
			{
				if (SecurityManager._IsSecurityOn())
				{
					throw new ExecutionEngineException(Environment.GetResourceString("ExecutionEngine_MissingSecurityDescriptor"));
				}
				return;
			}
			if (securityObjectForFrame.HasImperativeDenials())
			{
				throw new SecurityException(Environment.GetResourceString("Security_MustRevertOverride"));
			}
			securityObjectForFrame.SetDeny(cap);
		}

		internal static void PermitOnly(CodeAccessPermission cap, ref StackCrawlMark stackMark)
		{
			FrameSecurityDescriptor securityObjectForFrame = SecurityRuntime.GetSecurityObjectForFrame(ref stackMark, create: true);
			if (securityObjectForFrame == null)
			{
				if (SecurityManager._IsSecurityOn())
				{
					throw new ExecutionEngineException(Environment.GetResourceString("ExecutionEngine_MissingSecurityDescriptor"));
				}
				return;
			}
			if (securityObjectForFrame.HasImperativeRestrictions())
			{
				throw new SecurityException(Environment.GetResourceString("Security_MustRevertOverride"));
			}
			securityObjectForFrame.SetPermitOnly(cap);
		}

		private static PermissionListSet UpdateAppDomainPLS(PermissionListSet adPLS, PermissionSet grantedPerms, PermissionSet refusedPerms)
		{
			if (adPLS == null)
			{
				adPLS = new PermissionListSet();
				adPLS.UpdateDomainPLS(grantedPerms, refusedPerms);
				return adPLS;
			}
			PermissionListSet permissionListSet = new PermissionListSet();
			permissionListSet.UpdateDomainPLS(adPLS);
			permissionListSet.UpdateDomainPLS(grantedPerms, refusedPerms);
			return permissionListSet;
		}
	}
}
