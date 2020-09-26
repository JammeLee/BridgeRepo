using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Threading;

namespace System.Security
{
	internal class SecurityRuntime
	{
		internal const bool StackContinue = true;

		internal const bool StackHalt = false;

		private SecurityRuntime()
		{
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern FrameSecurityDescriptor GetSecurityObjectForFrame(ref StackCrawlMark stackMark, bool create);

		private static int OverridesHelper(FrameSecurityDescriptor secDesc)
		{
			int num = OverridesHelper2(secDesc, fDeclarative: false);
			return num + OverridesHelper2(secDesc, fDeclarative: true);
		}

		private static int OverridesHelper2(FrameSecurityDescriptor secDesc, bool fDeclarative)
		{
			int num = 0;
			PermissionSet permitOnly = secDesc.GetPermitOnly(fDeclarative);
			if (permitOnly != null)
			{
				num++;
			}
			permitOnly = secDesc.GetDenials(fDeclarative);
			if (permitOnly != null)
			{
				num++;
			}
			return num;
		}

		internal static MethodInfo GetMethodInfo(RuntimeMethodHandle rmh)
		{
			if (rmh.IsNullHandle())
			{
				return null;
			}
			PermissionSet.s_fullTrust.Assert();
			RuntimeTypeHandle declaringType = rmh.GetDeclaringType();
			return RuntimeType.GetMethodBase(declaringType, rmh) as MethodInfo;
		}

		private static bool FrameDescSetHelper(FrameSecurityDescriptor secDesc, PermissionSet demandSet, out PermissionSet alteredDemandSet, RuntimeMethodHandle rmh)
		{
			return secDesc.CheckSetDemand(demandSet, out alteredDemandSet, rmh);
		}

		private static bool FrameDescHelper(FrameSecurityDescriptor secDesc, IPermission demandIn, PermissionToken permToken, RuntimeMethodHandle rmh)
		{
			return secDesc.CheckDemand((CodeAccessPermission)demandIn, permToken, rmh);
		}

		[SecurityCritical]
		private static bool CheckDynamicMethodSetHelper(DynamicResolver dynamicResolver, PermissionSet demandSet, out PermissionSet alteredDemandSet, RuntimeMethodHandle rmh)
		{
			CompressedStack securityContext = dynamicResolver.GetSecurityContext();
			try
			{
				return securityContext.CheckSetDemandWithModificationNoHalt(demandSet, out alteredDemandSet, rmh);
			}
			catch (SecurityException inner)
			{
				throw new SecurityException(Environment.GetResourceString("Security_AnonymouslyHostedDynamicMethodCheckFailed"), inner);
			}
		}

		[SecurityCritical]
		private static bool CheckDynamicMethodHelper(DynamicResolver dynamicResolver, IPermission demandIn, PermissionToken permToken, RuntimeMethodHandle rmh)
		{
			CompressedStack securityContext = dynamicResolver.GetSecurityContext();
			try
			{
				return securityContext.CheckDemandNoHalt((CodeAccessPermission)demandIn, permToken, rmh);
			}
			catch (SecurityException inner)
			{
				throw new SecurityException(Environment.GetResourceString("Security_AnonymouslyHostedDynamicMethodCheckFailed"), inner);
			}
		}

		internal static void Assert(PermissionSet permSet, ref StackCrawlMark stackMark)
		{
			FrameSecurityDescriptor frameSecurityDescriptor = CodeAccessSecurityEngine.CheckNReturnSO(CodeAccessSecurityEngine.AssertPermissionToken, CodeAccessSecurityEngine.AssertPermission, ref stackMark, 1, 1);
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
			frameSecurityDescriptor.SetAssert(permSet);
		}

		internal static void AssertAllPossible(ref StackCrawlMark stackMark)
		{
			FrameSecurityDescriptor securityObjectForFrame = GetSecurityObjectForFrame(ref stackMark, create: true);
			if (securityObjectForFrame == null)
			{
				if (SecurityManager._IsSecurityOn())
				{
					throw new ExecutionEngineException(Environment.GetResourceString("ExecutionEngine_MissingSecurityDescriptor"));
				}
				return;
			}
			if (securityObjectForFrame.GetAssertAllPossible())
			{
				throw new SecurityException(Environment.GetResourceString("Security_MustRevertOverride"));
			}
			securityObjectForFrame.SetAssertAllPossible();
		}

		internal static void Deny(PermissionSet permSet, ref StackCrawlMark stackMark)
		{
			FrameSecurityDescriptor securityObjectForFrame = GetSecurityObjectForFrame(ref stackMark, create: true);
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
			securityObjectForFrame.SetDeny(permSet);
		}

		internal static void PermitOnly(PermissionSet permSet, ref StackCrawlMark stackMark)
		{
			FrameSecurityDescriptor securityObjectForFrame = GetSecurityObjectForFrame(ref stackMark, create: true);
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
			securityObjectForFrame.SetPermitOnly(permSet);
		}

		internal static void RevertAssert(ref StackCrawlMark stackMark)
		{
			FrameSecurityDescriptor securityObjectForFrame = GetSecurityObjectForFrame(ref stackMark, create: false);
			if (securityObjectForFrame != null)
			{
				securityObjectForFrame.RevertAssert();
			}
			else if (SecurityManager._IsSecurityOn())
			{
				throw new ExecutionEngineException(Environment.GetResourceString("ExecutionEngine_MissingSecurityDescriptor"));
			}
		}

		internal static void RevertDeny(ref StackCrawlMark stackMark)
		{
			FrameSecurityDescriptor securityObjectForFrame = GetSecurityObjectForFrame(ref stackMark, create: false);
			if (securityObjectForFrame != null)
			{
				securityObjectForFrame.RevertDeny();
			}
			else if (SecurityManager._IsSecurityOn())
			{
				throw new ExecutionEngineException(Environment.GetResourceString("ExecutionEngine_MissingSecurityDescriptor"));
			}
		}

		internal static void RevertPermitOnly(ref StackCrawlMark stackMark)
		{
			FrameSecurityDescriptor securityObjectForFrame = GetSecurityObjectForFrame(ref stackMark, create: false);
			if (securityObjectForFrame != null)
			{
				securityObjectForFrame.RevertPermitOnly();
			}
			else if (SecurityManager._IsSecurityOn())
			{
				throw new ExecutionEngineException(Environment.GetResourceString("ExecutionEngine_MissingSecurityDescriptor"));
			}
		}

		internal static void RevertAll(ref StackCrawlMark stackMark)
		{
			FrameSecurityDescriptor securityObjectForFrame = GetSecurityObjectForFrame(ref stackMark, create: false);
			if (securityObjectForFrame != null)
			{
				securityObjectForFrame.RevertAll();
			}
			else if (SecurityManager._IsSecurityOn())
			{
				throw new ExecutionEngineException(Environment.GetResourceString("ExecutionEngine_MissingSecurityDescriptor"));
			}
		}
	}
}
