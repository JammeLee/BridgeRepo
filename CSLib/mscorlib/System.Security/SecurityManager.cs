using System.Collections;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Security.Policy;
using System.Security.Util;
using System.Threading;

namespace System.Security
{
	[ComVisible(true)]
	public static class SecurityManager
	{
		private const int CheckExecutionRightsDisabledFlag = 256;

		private static Type securityPermissionType = null;

		private static SecurityPermission executionSecurityPermission = null;

		private static int checkExecution = -1;

		private static PolicyManager polmgr = new PolicyManager();

		private static int[][] s_BuiltInPermissionIndexMap = new int[6][]
		{
			new int[2]
			{
				0,
				10
			},
			new int[2]
			{
				1,
				11
			},
			new int[2]
			{
				2,
				12
			},
			new int[2]
			{
				4,
				13
			},
			new int[2]
			{
				6,
				14
			},
			new int[2]
			{
				7,
				9
			}
		};

		private static CodeAccessPermission[] s_UnrestrictedSpecialPermissionMap = new CodeAccessPermission[6]
		{
			new EnvironmentPermission(PermissionState.Unrestricted),
			new FileDialogPermission(PermissionState.Unrestricted),
			new FileIOPermission(PermissionState.Unrestricted),
			new ReflectionPermission(PermissionState.Unrestricted),
			new SecurityPermission(PermissionState.Unrestricted),
			new UIPermission(PermissionState.Unrestricted)
		};

		internal static PolicyManager PolicyManager => polmgr;

		public static bool CheckExecutionRights
		{
			get
			{
				return (GetGlobalFlags() & 0x100) != 256;
			}
			set
			{
				if (value)
				{
					checkExecution = 1;
					SetGlobalFlags(256, 0);
				}
				else
				{
					new SecurityPermission(SecurityPermissionFlag.ControlPolicy).Demand();
					checkExecution = 0;
					SetGlobalFlags(256, 256);
				}
			}
		}

		[Obsolete("Because security can no longer be turned off permanently, setting the SecurityEnabled property no longer has any effect. Reading the property will still indicate whether security has been turned off temporarily.")]
		public static bool SecurityEnabled
		{
			get
			{
				return _IsSecurityOn();
			}
			set
			{
			}
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public static bool IsGranted(IPermission perm)
		{
			if (perm == null)
			{
				return true;
			}
			StackCrawlMark stackmark = StackCrawlMark.LookForMyCaller;
			_GetGrantedPermissions(out var granted, out var denied, ref stackmark);
			if (granted.Contains(perm))
			{
				if (denied != null)
				{
					return !denied.Contains(perm);
				}
				return true;
			}
			return false;
		}

		private static bool CheckExecution()
		{
			if (checkExecution == -1)
			{
				checkExecution = (((GetGlobalFlags() & 0x100) == 0) ? 1 : 0);
			}
			if (checkExecution == 1)
			{
				if (securityPermissionType == null)
				{
					securityPermissionType = typeof(SecurityPermission);
					executionSecurityPermission = new SecurityPermission(SecurityPermissionFlag.Execution);
				}
				return true;
			}
			return false;
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		[StrongNameIdentityPermission(SecurityAction.LinkDemand, Name = "System.Windows.Forms", PublicKey = "0x00000000000000000400000000000000")]
		public static void GetZoneAndOrigin(out ArrayList zone, out ArrayList origin)
		{
			StackCrawlMark mark = StackCrawlMark.LookForMyCaller;
			if (_IsSecurityOn())
			{
				CodeAccessSecurityEngine.GetZoneAndOrigin(ref mark, out zone, out origin);
				return;
			}
			zone = null;
			origin = null;
		}

		[SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.ControlPolicy)]
		public static PolicyLevel LoadPolicyLevelFromFile(string path, PolicyLevelType type)
		{
			if (path == null)
			{
				throw new ArgumentNullException("path");
			}
			if (!File.InternalExists(path))
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_PolicyFileDoesNotExist"));
			}
			string fullPath = Path.GetFullPath(path);
			FileIOPermission fileIOPermission = new FileIOPermission(PermissionState.None);
			fileIOPermission.AddPathList(FileIOPermissionAccess.Read, fullPath);
			fileIOPermission.AddPathList(FileIOPermissionAccess.Write, fullPath);
			fileIOPermission.Demand();
			using FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read);
			using StreamReader streamReader = new StreamReader(stream);
			return LoadPolicyLevelFromStringHelper(streamReader.ReadToEnd(), path, type);
		}

		[SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.ControlPolicy)]
		public static PolicyLevel LoadPolicyLevelFromString(string str, PolicyLevelType type)
		{
			return LoadPolicyLevelFromStringHelper(str, null, type);
		}

		private static PolicyLevel LoadPolicyLevelFromStringHelper(string str, string path, PolicyLevelType type)
		{
			if (str == null)
			{
				throw new ArgumentNullException("str");
			}
			PolicyLevel policyLevel = new PolicyLevel(type, path);
			Parser parser = new Parser(str);
			SecurityElement topElement = parser.GetTopElement();
			if (topElement == null)
			{
				throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Policy_BadXml"), "configuration"));
			}
			SecurityElement securityElement = topElement.SearchForChildByTag("mscorlib");
			if (securityElement == null)
			{
				throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Policy_BadXml"), "mscorlib"));
			}
			SecurityElement securityElement2 = securityElement.SearchForChildByTag("security");
			if (securityElement2 == null)
			{
				throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Policy_BadXml"), "security"));
			}
			SecurityElement securityElement3 = securityElement2.SearchForChildByTag("policy");
			if (securityElement3 == null)
			{
				throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Policy_BadXml"), "policy"));
			}
			SecurityElement securityElement4 = securityElement3.SearchForChildByTag("PolicyLevel");
			if (securityElement4 != null)
			{
				policyLevel.FromXml(securityElement4);
				return policyLevel;
			}
			throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Policy_BadXml"), "PolicyLevel"));
		}

		[SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.ControlPolicy)]
		public static void SavePolicyLevel(PolicyLevel level)
		{
			PolicyManager.EncodeLevel(level);
		}

		private static PermissionSet ResolvePolicy(Evidence evidence, PermissionSet reqdPset, PermissionSet optPset, PermissionSet denyPset, out PermissionSet denied, out int securitySpecialFlags, bool checkExecutionPermission)
		{
			CodeAccessPermission.AssertAllPossible();
			PermissionSet permissionSet = ResolvePolicy(evidence, reqdPset, optPset, denyPset, out denied, checkExecutionPermission);
			securitySpecialFlags = GetSpecialFlags(permissionSet, denied);
			return permissionSet;
		}

		public static PermissionSet ResolvePolicy(Evidence evidence, PermissionSet reqdPset, PermissionSet optPset, PermissionSet denyPset, out PermissionSet denied)
		{
			return ResolvePolicy(evidence, reqdPset, optPset, denyPset, out denied, checkExecutionPermission: true);
		}

		private static PermissionSet ResolvePolicy(Evidence evidence, PermissionSet reqdPset, PermissionSet optPset, PermissionSet denyPset, out PermissionSet denied, bool checkExecutionPermission)
		{
			PermissionSet permissionSet = null;
			Exception exception = null;
			permissionSet = ((reqdPset != null) ? ((optPset == null) ? null : reqdPset.Union(optPset)) : optPset);
			if (permissionSet != null && !permissionSet.IsUnrestricted() && CheckExecution())
			{
				permissionSet.AddPermission(executionSecurityPermission);
			}
			evidence = ((evidence != null) ? evidence.ShallowCopy() : new Evidence());
			evidence.AddHost(new PermissionRequestEvidence(reqdPset, optPset, denyPset));
			PermissionSet permissionSet2 = polmgr.Resolve(evidence);
			if (permissionSet != null)
			{
				permissionSet2.InplaceIntersect(permissionSet);
			}
			if (checkExecutionPermission && CheckExecution() && (!permissionSet2.Contains(executionSecurityPermission) || (denyPset != null && denyPset.Contains(executionSecurityPermission))))
			{
				throw new PolicyException(Environment.GetResourceString("Policy_NoExecutionPermission"), -2146233320, exception);
			}
			if (reqdPset != null && !reqdPset.IsSubsetOf(permissionSet2))
			{
				throw new PolicyException(Environment.GetResourceString("Policy_NoRequiredPermission"), -2146233321, exception);
			}
			if (denyPset != null)
			{
				denied = denyPset.Copy();
				permissionSet2.MergeDeniedSet(denied);
				if (denied.IsEmpty())
				{
					denied = null;
				}
			}
			else
			{
				denied = null;
			}
			permissionSet2.IgnoreTypeLoadFailures = true;
			return permissionSet2;
		}

		public static PermissionSet ResolvePolicy(Evidence evidence)
		{
			evidence = ((evidence != null) ? evidence.ShallowCopy() : new Evidence());
			evidence.AddHost(new PermissionRequestEvidence(null, null, null));
			return polmgr.Resolve(evidence);
		}

		public static PermissionSet ResolvePolicy(Evidence[] evidences)
		{
			if (evidences == null || evidences.Length == 0)
			{
				Evidence[] array = new Evidence[1];
				evidences = array;
			}
			PermissionSet permissionSet = ResolvePolicy(evidences[0]);
			if (permissionSet == null)
			{
				return null;
			}
			for (int i = 1; i < evidences.Length; i++)
			{
				permissionSet = permissionSet.Intersect(ResolvePolicy(evidences[i]));
				if (permissionSet == null || permissionSet.IsEmpty())
				{
					return permissionSet;
				}
			}
			return permissionSet;
		}

		public static PermissionSet ResolveSystemPolicy(Evidence evidence)
		{
			if (PolicyManager.IsGacAssembly(evidence))
			{
				return new PermissionSet(PermissionState.Unrestricted);
			}
			return polmgr.CodeGroupResolve(evidence, systemPolicy: true);
		}

		public static IEnumerator ResolvePolicyGroups(Evidence evidence)
		{
			return polmgr.ResolveCodeGroups(evidence);
		}

		public static IEnumerator PolicyHierarchy()
		{
			return polmgr.PolicyHierarchy();
		}

		[SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.ControlPolicy)]
		public static void SavePolicy()
		{
			polmgr.Save();
			SaveGlobalFlags();
		}

		internal static int GetSpecialFlags(PermissionSet grantSet, PermissionSet deniedSet)
		{
			if (grantSet != null && grantSet.IsUnrestricted() && (deniedSet == null || deniedSet.IsEmpty()))
			{
				return -1;
			}
			SecurityPermission securityPermission = null;
			SecurityPermissionFlag securityPermissionFlag = SecurityPermissionFlag.NoFlags;
			ReflectionPermission reflectionPermission = null;
			ReflectionPermissionFlag reflectionPermissionFlag = ReflectionPermissionFlag.NoFlags;
			CodeAccessPermission[] array = new CodeAccessPermission[6];
			if (grantSet != null)
			{
				if (grantSet.IsUnrestricted())
				{
					securityPermissionFlag = SecurityPermissionFlag.AllFlags;
					reflectionPermissionFlag = ReflectionPermissionFlag.AllFlags | ReflectionPermissionFlag.RestrictedMemberAccess;
					for (int i = 0; i < array.Length; i++)
					{
						array[i] = s_UnrestrictedSpecialPermissionMap[i];
					}
				}
				else
				{
					securityPermission = grantSet.GetPermission(6) as SecurityPermission;
					if (securityPermission != null)
					{
						securityPermissionFlag = securityPermission.Flags;
					}
					reflectionPermission = grantSet.GetPermission(4) as ReflectionPermission;
					if (reflectionPermission != null)
					{
						reflectionPermissionFlag = reflectionPermission.Flags;
					}
					for (int j = 0; j < array.Length; j++)
					{
						array[j] = grantSet.GetPermission(s_BuiltInPermissionIndexMap[j][0]) as CodeAccessPermission;
					}
				}
			}
			if (deniedSet != null)
			{
				if (deniedSet.IsUnrestricted())
				{
					securityPermissionFlag = SecurityPermissionFlag.NoFlags;
					reflectionPermissionFlag = ReflectionPermissionFlag.NoFlags;
					for (int k = 0; k < s_BuiltInPermissionIndexMap.Length; k++)
					{
						array[k] = null;
					}
				}
				else
				{
					securityPermission = deniedSet.GetPermission(6) as SecurityPermission;
					if (securityPermission != null)
					{
						securityPermissionFlag &= ~securityPermission.Flags;
					}
					reflectionPermission = deniedSet.GetPermission(4) as ReflectionPermission;
					if (reflectionPermission != null)
					{
						reflectionPermissionFlag &= ~reflectionPermission.Flags;
					}
					for (int l = 0; l < s_BuiltInPermissionIndexMap.Length; l++)
					{
						CodeAccessPermission codeAccessPermission = deniedSet.GetPermission(s_BuiltInPermissionIndexMap[l][0]) as CodeAccessPermission;
						if (codeAccessPermission != null && !codeAccessPermission.IsSubsetOf(null))
						{
							array[l] = null;
						}
					}
				}
			}
			int num = MapToSpecialFlags(securityPermissionFlag, reflectionPermissionFlag);
			if (num != -1)
			{
				for (int m = 0; m < array.Length; m++)
				{
					if (array[m] != null && ((IUnrestrictedPermission)array[m]).IsUnrestricted())
					{
						num |= 1 << s_BuiltInPermissionIndexMap[m][1];
					}
				}
			}
			return num;
		}

		private static int MapToSpecialFlags(SecurityPermissionFlag securityPermissionFlags, ReflectionPermissionFlag reflectionPermissionFlags)
		{
			int num = 0;
			if ((securityPermissionFlags & SecurityPermissionFlag.UnmanagedCode) == SecurityPermissionFlag.UnmanagedCode)
			{
				num |= 1;
			}
			if ((securityPermissionFlags & SecurityPermissionFlag.SkipVerification) == SecurityPermissionFlag.SkipVerification)
			{
				num |= 2;
			}
			if ((securityPermissionFlags & SecurityPermissionFlag.Assertion) == SecurityPermissionFlag.Assertion)
			{
				num |= 8;
			}
			if ((securityPermissionFlags & SecurityPermissionFlag.SerializationFormatter) == SecurityPermissionFlag.SerializationFormatter)
			{
				num |= 0x20;
			}
			if ((securityPermissionFlags & SecurityPermissionFlag.BindingRedirects) == SecurityPermissionFlag.BindingRedirects)
			{
				num |= 0x100;
			}
			if ((securityPermissionFlags & SecurityPermissionFlag.ControlEvidence) == SecurityPermissionFlag.ControlEvidence)
			{
				num |= 0x10000;
			}
			if ((securityPermissionFlags & SecurityPermissionFlag.ControlPrincipal) == SecurityPermissionFlag.ControlPrincipal)
			{
				num |= 0x20000;
			}
			if ((reflectionPermissionFlags & ReflectionPermissionFlag.RestrictedMemberAccess) == ReflectionPermissionFlag.RestrictedMemberAccess)
			{
				num |= 0x40;
			}
			if ((reflectionPermissionFlags & ReflectionPermissionFlag.MemberAccess) == ReflectionPermissionFlag.MemberAccess)
			{
				num |= 0x10;
			}
			return num;
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern bool _IsSameType(string strLeft, string strRight);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern bool _SetThreadSecurity(bool bThreadSecurity);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern bool _IsSecurityOn();

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern int GetGlobalFlags();

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern void SetGlobalFlags(int mask, int flags);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern void SaveGlobalFlags();

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern void _GetGrantedPermissions(out PermissionSet granted, out PermissionSet denied, ref StackCrawlMark stackmark);
	}
}
