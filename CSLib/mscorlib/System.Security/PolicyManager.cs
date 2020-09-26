using System.Collections;
using System.Globalization;
using System.Security.Permissions;
using System.Security.Policy;
using System.Security.Util;
using System.Text;
using System.Threading;

namespace System.Security
{
	internal class PolicyManager
	{
		private object m_policyLevels;

		private IList PolicyLevels
		{
			get
			{
				if (m_policyLevels == null)
				{
					ArrayList arrayList = new ArrayList();
					string locationFromType = PolicyLevel.GetLocationFromType(PolicyLevelType.Enterprise);
					arrayList.Add(new PolicyLevel(PolicyLevelType.Enterprise, locationFromType, ConfigId.EnterprisePolicyLevel));
					string locationFromType2 = PolicyLevel.GetLocationFromType(PolicyLevelType.Machine);
					arrayList.Add(new PolicyLevel(PolicyLevelType.Machine, locationFromType2, ConfigId.MachinePolicyLevel));
					if (Config.UserDirectory != null)
					{
						string locationFromType3 = PolicyLevel.GetLocationFromType(PolicyLevelType.User);
						arrayList.Add(new PolicyLevel(PolicyLevelType.User, locationFromType3, ConfigId.UserPolicyLevel));
					}
					Interlocked.CompareExchange(ref m_policyLevels, arrayList, null);
				}
				return m_policyLevels as ArrayList;
			}
		}

		internal PolicyManager()
		{
		}

		internal void AddLevel(PolicyLevel level)
		{
			PolicyLevels.Add(level);
		}

		[SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.ControlPolicy)]
		internal IEnumerator PolicyHierarchy()
		{
			return PolicyLevels.GetEnumerator();
		}

		internal PermissionSet Resolve(Evidence evidence)
		{
			if (!IsGacAssembly(evidence))
			{
				HostSecurityManager hostSecurityManager = AppDomain.CurrentDomain.HostSecurityManager;
				if ((hostSecurityManager.Flags & HostSecurityManagerOptions.HostResolvePolicy) == HostSecurityManagerOptions.HostResolvePolicy)
				{
					return hostSecurityManager.ResolvePolicy(evidence);
				}
			}
			return ResolveHelper(evidence);
		}

		internal PermissionSet ResolveHelper(Evidence evidence)
		{
			PermissionSet permissionSet = null;
			if (IsGacAssembly(evidence))
			{
				return new PermissionSet(PermissionState.Unrestricted);
			}
			ApplicationTrust applicationTrust = AppDomain.CurrentDomain.ApplicationTrust;
			if (applicationTrust != null)
			{
				if (IsFullTrust(evidence, applicationTrust))
				{
					return new PermissionSet(PermissionState.Unrestricted);
				}
				return applicationTrust.DefaultGrantSet.PermissionSet;
			}
			return CodeGroupResolve(evidence, systemPolicy: false);
		}

		internal PermissionSet CodeGroupResolve(Evidence evidence, bool systemPolicy)
		{
			PermissionSet permissionSet = null;
			PolicyLevel policyLevel = null;
			IEnumerator enumerator = PolicyLevels.GetEnumerator();
			char[] serializedEvidence = MakeEvidenceArray(evidence, verbose: false);
			int count = evidence.Count;
			bool flag = AppDomain.CurrentDomain.GetData("IgnoreSystemPolicy") != null;
			bool flag2 = false;
			while (enumerator.MoveNext())
			{
				policyLevel = (PolicyLevel)enumerator.Current;
				if (systemPolicy)
				{
					if (policyLevel.Type == PolicyLevelType.AppDomain)
					{
						continue;
					}
				}
				else if (flag && policyLevel.Type != PolicyLevelType.AppDomain)
				{
					continue;
				}
				PolicyStatement policyStatement = policyLevel.Resolve(evidence, count, serializedEvidence);
				if (permissionSet == null)
				{
					permissionSet = policyStatement.PermissionSet;
				}
				else
				{
					permissionSet.InplaceIntersect(policyStatement.GetPermissionSetNoCopy());
				}
				if (permissionSet == null || permissionSet.FastIsEmpty())
				{
					break;
				}
				if ((policyStatement.Attributes & PolicyStatementAttribute.LevelFinal) == PolicyStatementAttribute.LevelFinal)
				{
					if (policyLevel.Type != PolicyLevelType.AppDomain)
					{
						flag2 = true;
					}
					break;
				}
			}
			if (permissionSet != null && flag2)
			{
				PolicyLevel policyLevel2 = null;
				for (int num = PolicyLevels.Count - 1; num >= 0; num--)
				{
					policyLevel = (PolicyLevel)PolicyLevels[num];
					if (policyLevel.Type == PolicyLevelType.AppDomain)
					{
						policyLevel2 = policyLevel;
						break;
					}
				}
				if (policyLevel2 != null)
				{
					PolicyStatement policyStatement = policyLevel2.Resolve(evidence, count, serializedEvidence);
					permissionSet.InplaceIntersect(policyStatement.GetPermissionSetNoCopy());
				}
			}
			if (permissionSet == null)
			{
				permissionSet = new PermissionSet(PermissionState.None);
			}
			if (!CodeAccessSecurityEngine.DoesFullTrustMeanFullTrust() || !permissionSet.IsUnrestricted())
			{
				IEnumerator hostEnumerator = evidence.GetHostEnumerator();
				while (hostEnumerator.MoveNext())
				{
					object current = hostEnumerator.Current;
					IIdentityPermissionFactory identityPermissionFactory = current as IIdentityPermissionFactory;
					if (identityPermissionFactory != null)
					{
						IPermission permission = identityPermissionFactory.CreateIdentityPermission(evidence);
						if (permission != null)
						{
							permissionSet.AddPermission(permission);
						}
					}
				}
			}
			permissionSet.IgnoreTypeLoadFailures = true;
			return permissionSet;
		}

		internal static bool IsGacAssembly(Evidence evidence)
		{
			return new GacMembershipCondition().Check(evidence);
		}

		private static bool IsFullTrust(Evidence evidence, ApplicationTrust appTrust)
		{
			if (appTrust == null)
			{
				return false;
			}
			StrongName[] fullTrustAssemblies = appTrust.FullTrustAssemblies;
			if (fullTrustAssemblies != null)
			{
				for (int i = 0; i < fullTrustAssemblies.Length; i++)
				{
					if (fullTrustAssemblies[i] == null)
					{
						continue;
					}
					StrongNameMembershipCondition strongNameMembershipCondition = new StrongNameMembershipCondition(fullTrustAssemblies[i].PublicKey, fullTrustAssemblies[i].Name, fullTrustAssemblies[i].Version);
					object usedEvidence = null;
					if (((IReportMatchMembershipCondition)strongNameMembershipCondition).Check(evidence, out usedEvidence))
					{
						IDelayEvaluatedEvidence delayEvaluatedEvidence = usedEvidence as IDelayEvaluatedEvidence;
						if (usedEvidence != null)
						{
							delayEvaluatedEvidence.MarkUsed();
						}
						return true;
					}
				}
			}
			return false;
		}

		internal IEnumerator ResolveCodeGroups(Evidence evidence)
		{
			ArrayList arrayList = new ArrayList();
			IEnumerator enumerator = PolicyLevels.GetEnumerator();
			while (enumerator.MoveNext())
			{
				CodeGroup codeGroup = ((PolicyLevel)enumerator.Current).ResolveMatchingCodeGroups(evidence);
				if (codeGroup != null)
				{
					arrayList.Add(codeGroup);
				}
			}
			return arrayList.GetEnumerator(0, arrayList.Count);
		}

		internal static PolicyStatement ResolveCodeGroup(CodeGroup codeGroup, Evidence evidence)
		{
			if (codeGroup.GetType().Assembly != typeof(UnionCodeGroup).Assembly)
			{
				evidence.MarkAllEvidenceAsUsed();
			}
			return codeGroup.Resolve(evidence);
		}

		internal static bool CheckMembershipCondition(IMembershipCondition membershipCondition, Evidence evidence, out object usedEvidence)
		{
			IReportMatchMembershipCondition reportMatchMembershipCondition = membershipCondition as IReportMatchMembershipCondition;
			if (reportMatchMembershipCondition != null)
			{
				return reportMatchMembershipCondition.Check(evidence, out usedEvidence);
			}
			usedEvidence = null;
			evidence.MarkAllEvidenceAsUsed();
			return membershipCondition.Check(evidence);
		}

		internal void Save()
		{
			EncodeLevel(Environment.GetResourceString("Policy_PL_Enterprise"));
			EncodeLevel(Environment.GetResourceString("Policy_PL_Machine"));
			EncodeLevel(Environment.GetResourceString("Policy_PL_User"));
		}

		private void EncodeLevel(string label)
		{
			for (int i = 0; i < PolicyLevels.Count; i++)
			{
				PolicyLevel policyLevel = (PolicyLevel)PolicyLevels[i];
				if (policyLevel.Label.Equals(label))
				{
					EncodeLevel(policyLevel);
					break;
				}
			}
		}

		internal static void EncodeLevel(PolicyLevel level)
		{
			SecurityElement securityElement = new SecurityElement("configuration");
			SecurityElement securityElement2 = new SecurityElement("mscorlib");
			SecurityElement securityElement3 = new SecurityElement("security");
			SecurityElement securityElement4 = new SecurityElement("policy");
			securityElement.AddChild(securityElement2);
			securityElement2.AddChild(securityElement3);
			securityElement3.AddChild(securityElement4);
			securityElement4.AddChild(level.ToXml());
			try
			{
				StringBuilder stringBuilder = new StringBuilder();
				Encoding uTF = Encoding.UTF8;
				SecurityElement securityElement5 = new SecurityElement("xml");
				securityElement5.m_type = SecurityElementType.Format;
				securityElement5.AddAttribute("version", "1.0");
				securityElement5.AddAttribute("encoding", uTF.WebName);
				stringBuilder.Append(securityElement5.ToString());
				stringBuilder.Append(securityElement.ToString());
				byte[] bytes = uTF.GetBytes(stringBuilder.ToString());
				if (level.Path == null || !Config.SaveDataByte(level.Path, bytes, 0, bytes.Length))
				{
					throw new PolicyException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Policy_UnableToSave"), level.Label));
				}
			}
			catch (Exception ex)
			{
				if (ex is PolicyException)
				{
					throw ex;
				}
				throw new PolicyException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Policy_UnableToSave"), level.Label), ex);
			}
			catch
			{
				throw new PolicyException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Policy_UnableToSave"), level.Label));
			}
			Config.ResetCacheData(level.ConfigId);
			if (CanUseQuickCache(level.RootCodeGroup))
			{
				Config.SetQuickCache(level.ConfigId, GenerateQuickCache(level));
			}
		}

		internal static bool CanUseQuickCache(CodeGroup group)
		{
			ArrayList arrayList = new ArrayList();
			arrayList.Add(group);
			for (int i = 0; i < arrayList.Count; i++)
			{
				group = (CodeGroup)arrayList[i];
				IUnionSemanticCodeGroup unionSemanticCodeGroup = group as IUnionSemanticCodeGroup;
				if (unionSemanticCodeGroup != null)
				{
					if (!TestPolicyStatement(group.PolicyStatement))
					{
						return false;
					}
					IMembershipCondition membershipCondition = group.MembershipCondition;
					if (membershipCondition != null && !(membershipCondition is IConstantMembershipCondition))
					{
						return false;
					}
					IList children = group.Children;
					if (children != null && children.Count > 0)
					{
						IEnumerator enumerator = children.GetEnumerator();
						while (enumerator.MoveNext())
						{
							arrayList.Add(enumerator.Current);
						}
					}
					continue;
				}
				return false;
			}
			return true;
		}

		private static bool TestPolicyStatement(PolicyStatement policy)
		{
			if (policy == null)
			{
				return true;
			}
			return (policy.Attributes & PolicyStatementAttribute.Exclusive) == 0;
		}

		private static QuickCacheEntryType GenerateQuickCache(PolicyLevel level)
		{
			QuickCacheEntryType[] array = new QuickCacheEntryType[5]
			{
				QuickCacheEntryType.FullTrustZoneMyComputer,
				QuickCacheEntryType.FullTrustZoneIntranet,
				QuickCacheEntryType.FullTrustZoneInternet,
				QuickCacheEntryType.FullTrustZoneTrusted,
				QuickCacheEntryType.FullTrustZoneUntrusted
			};
			QuickCacheEntryType quickCacheEntryType = (QuickCacheEntryType)0;
			Evidence evidence = new Evidence();
			PermissionSet permissionSet = null;
			try
			{
				permissionSet = level.Resolve(evidence).PermissionSet;
				if (permissionSet.IsUnrestricted())
				{
					quickCacheEntryType |= QuickCacheEntryType.FullTrustAll;
				}
			}
			catch (PolicyException)
			{
			}
			Array values = Enum.GetValues(typeof(SecurityZone));
			for (int i = 0; i < values.Length; i++)
			{
				if ((SecurityZone)values.GetValue(i) == SecurityZone.NoZone)
				{
					continue;
				}
				Evidence evidence2 = new Evidence();
				evidence2.AddHost(new Zone((SecurityZone)values.GetValue(i)));
				PermissionSet permissionSet2 = null;
				try
				{
					permissionSet2 = level.Resolve(evidence2).PermissionSet;
					if (permissionSet2.IsUnrestricted())
					{
						quickCacheEntryType |= array[i];
					}
				}
				catch (PolicyException)
				{
				}
			}
			return quickCacheEntryType;
		}

		internal static char[] MakeEvidenceArray(Evidence evidence, bool verbose)
		{
			IEnumerator enumerator = evidence.GetEnumerator();
			int num = 0;
			while (enumerator.MoveNext())
			{
				IBuiltInEvidence builtInEvidence = enumerator.Current as IBuiltInEvidence;
				if (builtInEvidence == null)
				{
					return null;
				}
				num += builtInEvidence.GetRequiredSize(verbose);
			}
			enumerator.Reset();
			char[] array = new char[num];
			int position = 0;
			while (enumerator.MoveNext())
			{
				position = ((IBuiltInEvidence)enumerator.Current).OutputToBuffer(array, position, verbose);
			}
			return array;
		}
	}
}
