using System.Threading;

namespace System.Reflection.Emit
{
	internal class DynamicResolver : Resolver
	{
		private class DestroyScout
		{
			internal RuntimeMethodHandle m_method;

			~DestroyScout()
			{
				if (m_method.IsNullHandle())
				{
					return;
				}
				if (m_method.GetResolver() != null)
				{
					if (!Environment.HasShutdownStarted && !AppDomain.CurrentDomain.IsFinalizingForUnload())
					{
						GC.ReRegisterForFinalize(this);
					}
				}
				else
				{
					m_method.Destroy();
				}
			}
		}

		[Flags]
		internal enum SecurityControlFlags
		{
			Default = 0x0,
			SkipVisibilityChecks = 0x1,
			RestrictedSkipVisibilityChecks = 0x2,
			HasCreationContext = 0x4,
			CanSkipCSEvaluation = 0x8
		}

		private __ExceptionInfo[] m_exceptions;

		private byte[] m_exceptionHeader;

		private DynamicMethod m_method;

		private byte[] m_code;

		private byte[] m_localSignature;

		private int m_stackSize;

		private DynamicScope m_scope;

		private int m_methodToken;

		internal DynamicResolver(DynamicILGenerator ilGenerator)
		{
			m_stackSize = ilGenerator.GetMaxStackSize();
			m_exceptions = ilGenerator.GetExceptions();
			m_code = ilGenerator.BakeByteArray();
			m_localSignature = ilGenerator.m_localSignature.InternalGetSignatureArray();
			m_scope = ilGenerator.m_scope;
			m_method = (DynamicMethod)ilGenerator.m_methodBuilder;
			m_method.m_resolver = this;
		}

		internal DynamicResolver(DynamicILInfo dynamicILInfo)
		{
			m_stackSize = dynamicILInfo.MaxStackSize;
			m_code = dynamicILInfo.Code;
			m_localSignature = dynamicILInfo.LocalSignature;
			m_exceptionHeader = dynamicILInfo.Exceptions;
			m_scope = dynamicILInfo.DynamicScope;
			m_method = dynamicILInfo.DynamicMethod;
			m_method.m_resolver = this;
		}

		~DynamicResolver()
		{
			DynamicMethod method = m_method;
			if (method == null || method.m_method.IsNullHandle())
			{
				return;
			}
			DestroyScout destroyScout = null;
			try
			{
				destroyScout = new DestroyScout();
			}
			catch
			{
				if (!Environment.HasShutdownStarted && !AppDomain.CurrentDomain.IsFinalizingForUnload())
				{
					GC.ReRegisterForFinalize(this);
				}
				return;
			}
			destroyScout.m_method = method.m_method;
		}

		internal override void GetJitContext(ref int securityControlFlags, ref RuntimeTypeHandle typeOwner)
		{
			SecurityControlFlags securityControlFlags2 = SecurityControlFlags.Default;
			if (m_method.m_restrictedSkipVisibility)
			{
				securityControlFlags2 |= SecurityControlFlags.RestrictedSkipVisibilityChecks;
			}
			else if (m_method.m_skipVisibility)
			{
				securityControlFlags2 |= SecurityControlFlags.SkipVisibilityChecks;
			}
			typeOwner = ((m_method.m_typeOwner != null) ? m_method.m_typeOwner.TypeHandle : RuntimeTypeHandle.EmptyHandle);
			if (m_method.m_creationContext != null)
			{
				securityControlFlags2 |= SecurityControlFlags.HasCreationContext;
				if (m_method.m_creationContext.CanSkipEvaluation)
				{
					securityControlFlags2 |= SecurityControlFlags.CanSkipCSEvaluation;
				}
			}
			securityControlFlags = (int)securityControlFlags2;
		}

		internal override byte[] GetCodeInfo(ref int stackSize, ref int initLocals, ref int EHCount)
		{
			stackSize = m_stackSize;
			if (m_exceptionHeader != null && m_exceptionHeader.Length != 0)
			{
				if (m_exceptionHeader.Length < 4)
				{
					throw new FormatException();
				}
				byte b = m_exceptionHeader[0];
				if ((b & 0x40u) != 0)
				{
					byte[] array = new byte[4];
					for (int i = 0; i < 3; i++)
					{
						array[i] = m_exceptionHeader[i + 1];
					}
					EHCount = (BitConverter.ToInt32(array, 0) - 4) / 24;
				}
				else
				{
					EHCount = (m_exceptionHeader[1] - 2) / 12;
				}
			}
			else
			{
				EHCount = ILGenerator.CalculateNumberOfExceptions(m_exceptions);
			}
			initLocals = (m_method.InitLocals ? 1 : 0);
			return m_code;
		}

		internal override byte[] GetLocalsSignature()
		{
			return m_localSignature;
		}

		internal override byte[] GetRawEHInfo()
		{
			return m_exceptionHeader;
		}

		internal unsafe override void GetEHInfo(int excNumber, void* exc)
		{
			for (int i = 0; i < m_exceptions.Length; i++)
			{
				int numberOfCatches = m_exceptions[i].GetNumberOfCatches();
				if (excNumber < numberOfCatches)
				{
					((CORINFO_EH_CLAUSE*)exc)->Flags = m_exceptions[i].GetExceptionTypes()[excNumber];
					((CORINFO_EH_CLAUSE*)exc)->Flags |= 536870912;
					((CORINFO_EH_CLAUSE*)exc)->TryOffset = m_exceptions[i].GetStartAddress();
					if ((((CORINFO_EH_CLAUSE*)exc)->Flags & 2) != 2)
					{
						((CORINFO_EH_CLAUSE*)exc)->TryLength = m_exceptions[i].GetEndAddress() - ((CORINFO_EH_CLAUSE*)exc)->TryOffset;
					}
					else
					{
						((CORINFO_EH_CLAUSE*)exc)->TryLength = m_exceptions[i].GetFinallyEndAddress() - ((CORINFO_EH_CLAUSE*)exc)->TryOffset;
					}
					((CORINFO_EH_CLAUSE*)exc)->HandlerOffset = m_exceptions[i].GetCatchAddresses()[excNumber];
					((CORINFO_EH_CLAUSE*)exc)->HandlerLength = m_exceptions[i].GetCatchEndAddresses()[excNumber] - ((CORINFO_EH_CLAUSE*)exc)->HandlerOffset;
					((CORINFO_EH_CLAUSE*)exc)->ClassTokenOrFilterOffset = m_exceptions[i].GetFilterAddresses()[excNumber];
					break;
				}
				excNumber -= numberOfCatches;
			}
		}

		internal override string GetStringLiteral(int token)
		{
			return m_scope.GetString(token);
		}

		private int GetMethodToken()
		{
			if (IsValidToken(m_methodToken) == 0)
			{
				int tokenFor = m_scope.GetTokenFor(m_method.GetMethodDescriptor());
				Interlocked.CompareExchange(ref m_methodToken, tokenFor, 0);
			}
			return m_methodToken;
		}

		internal override int IsValidToken(int token)
		{
			if (m_scope[token] == null)
			{
				return 0;
			}
			return 1;
		}

		internal override CompressedStack GetSecurityContext()
		{
			return m_method.m_creationContext;
		}

		internal unsafe override void* ResolveToken(int token)
		{
			object obj = m_scope[token];
			if (obj is RuntimeTypeHandle)
			{
				return (void*)((RuntimeTypeHandle)obj).Value;
			}
			if (obj is RuntimeMethodHandle)
			{
				return (void*)((RuntimeMethodHandle)obj).Value;
			}
			if (obj is RuntimeFieldHandle)
			{
				return (void*)((RuntimeFieldHandle)obj).Value;
			}
			if (obj is DynamicMethod)
			{
				DynamicMethod dynamicMethod = (DynamicMethod)obj;
				return (void*)dynamicMethod.GetMethodDescriptor().Value;
			}
			if (obj is GenericMethodInfo)
			{
				GenericMethodInfo genericMethodInfo = (GenericMethodInfo)obj;
				return (void*)genericMethodInfo.m_method.Value;
			}
			if (obj is GenericFieldInfo)
			{
				GenericFieldInfo genericFieldInfo = (GenericFieldInfo)obj;
				return (void*)genericFieldInfo.m_field.Value;
			}
			if (obj is VarArgMethod)
			{
				VarArgMethod varArgMethod = (VarArgMethod)obj;
				DynamicMethod dynamicMethod2 = varArgMethod.m_method as DynamicMethod;
				if (dynamicMethod2 == null)
				{
					return (void*)varArgMethod.m_method.MethodHandle.Value;
				}
				return (void*)dynamicMethod2.GetMethodDescriptor().Value;
			}
			return null;
		}

		internal override byte[] ResolveSignature(int token, int fromMethod)
		{
			return m_scope.ResolveSignature(token, fromMethod);
		}

		internal override int ParentToken(int token)
		{
			RuntimeTypeHandle type = RuntimeTypeHandle.EmptyHandle;
			object obj = m_scope[token];
			if (obj is RuntimeMethodHandle)
			{
				type = ((RuntimeMethodHandle)obj).GetDeclaringType();
			}
			else if (obj is RuntimeFieldHandle)
			{
				type = ((RuntimeFieldHandle)obj).GetApproxDeclaringType();
			}
			else if (obj is DynamicMethod)
			{
				DynamicMethod dynamicMethod = (DynamicMethod)obj;
				type = dynamicMethod.m_method.GetDeclaringType();
			}
			else if (obj is GenericMethodInfo)
			{
				GenericMethodInfo genericMethodInfo = (GenericMethodInfo)obj;
				type = genericMethodInfo.m_context;
			}
			else if (obj is GenericFieldInfo)
			{
				GenericFieldInfo genericFieldInfo = (GenericFieldInfo)obj;
				type = genericFieldInfo.m_context;
			}
			else if (obj is VarArgMethod)
			{
				VarArgMethod varArgMethod = (VarArgMethod)obj;
				type = (varArgMethod.m_method as DynamicMethod)?.GetMethodDescriptor().GetDeclaringType() ?? ((varArgMethod.m_method.DeclaringType != null) ? varArgMethod.m_method.DeclaringType.TypeHandle : varArgMethod.m_method.MethodHandle.GetDeclaringType());
			}
			if (type.IsNullHandle())
			{
				return -1;
			}
			return m_scope.GetTokenFor(type);
		}

		internal override MethodInfo GetDynamicMethod()
		{
			return m_method.GetMethodInfo();
		}
	}
}
