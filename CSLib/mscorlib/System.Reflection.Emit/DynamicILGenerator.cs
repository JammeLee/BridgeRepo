using System.Diagnostics.SymbolStore;
using System.Runtime.InteropServices;

namespace System.Reflection.Emit
{
	internal class DynamicILGenerator : ILGenerator
	{
		internal DynamicScope m_scope;

		private int m_methodSigToken;

		internal DynamicILGenerator(DynamicMethod method, byte[] methodSignature, int size)
			: base(method, size)
		{
			m_scope = new DynamicScope();
			m_methodSigToken = m_scope.GetTokenFor(methodSignature);
		}

		internal unsafe RuntimeMethodHandle GetCallableMethod(void* module)
		{
			return new RuntimeMethodHandle(ModuleHandle.GetDynamicMethod(module, m_methodBuilder.Name, (byte[])m_scope[m_methodSigToken], new DynamicResolver(this)));
		}

		public override LocalBuilder DeclareLocal(Type localType, bool pinned)
		{
			if (localType == null)
			{
				throw new ArgumentNullException("localType");
			}
			if (localType.GetType() != typeof(RuntimeType))
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_MustBeRuntimeType"));
			}
			LocalBuilder result = new LocalBuilder(m_localCount, localType, m_methodBuilder);
			m_localSignature.AddArgument(localType, pinned);
			m_localCount++;
			return result;
		}

		public override void Emit(OpCode opcode, MethodInfo meth)
		{
			if (meth == null)
			{
				throw new ArgumentNullException("meth");
			}
			int num = 0;
			int num2 = 0;
			DynamicMethod dynamicMethod = meth as DynamicMethod;
			if (dynamicMethod == null)
			{
				if (!(meth is RuntimeMethodInfo))
				{
					throw new ArgumentException(Environment.GetResourceString("Argument_MustBeRuntimeMethodInfo"), "meth");
				}
				num2 = ((meth.DeclaringType == null || (!meth.DeclaringType.IsGenericType && !meth.DeclaringType.IsArray)) ? m_scope.GetTokenFor(meth.MethodHandle) : m_scope.GetTokenFor(meth.MethodHandle, meth.DeclaringType.TypeHandle));
			}
			else
			{
				if (opcode.Equals(OpCodes.Ldtoken) || opcode.Equals(OpCodes.Ldftn) || opcode.Equals(OpCodes.Ldvirtftn))
				{
					throw new ArgumentException(Environment.GetResourceString("Argument_InvalidOpCodeOnDynamicMethod"));
				}
				num2 = m_scope.GetTokenFor(dynamicMethod);
			}
			EnsureCapacity(7);
			InternalEmit(opcode);
			if (opcode.m_push == StackBehaviour.Varpush && meth.ReturnType != typeof(void))
			{
				num++;
			}
			if (opcode.m_pop == StackBehaviour.Varpop)
			{
				num -= meth.GetParametersNoCopy().Length;
			}
			if (!meth.IsStatic && !opcode.Equals(OpCodes.Newobj) && !opcode.Equals(OpCodes.Ldtoken) && !opcode.Equals(OpCodes.Ldftn))
			{
				num--;
			}
			UpdateStackSize(opcode, num);
			m_length = PutInteger4(num2, m_length, m_ILStream);
		}

		[ComVisible(true)]
		public override void Emit(OpCode opcode, ConstructorInfo con)
		{
			if (con == null || !(con is RuntimeConstructorInfo))
			{
				throw new ArgumentNullException("con");
			}
			if (con.DeclaringType != null && (con.DeclaringType.IsGenericType || con.DeclaringType.IsArray))
			{
				Emit(opcode, con.MethodHandle, con.DeclaringType.TypeHandle);
			}
			else
			{
				Emit(opcode, con.MethodHandle);
			}
		}

		public void Emit(OpCode opcode, RuntimeMethodHandle meth)
		{
			if (meth.IsNullHandle())
			{
				throw new ArgumentNullException("meth");
			}
			int tokenFor = m_scope.GetTokenFor(meth);
			EnsureCapacity(7);
			InternalEmit(opcode);
			UpdateStackSize(opcode, 1);
			m_length = PutInteger4(tokenFor, m_length, m_ILStream);
		}

		public void Emit(OpCode opcode, RuntimeMethodHandle meth, RuntimeTypeHandle typeContext)
		{
			if (meth.IsNullHandle())
			{
				throw new ArgumentNullException("meth");
			}
			if (typeContext.IsNullHandle())
			{
				throw new ArgumentNullException("typeContext");
			}
			int tokenFor = m_scope.GetTokenFor(meth, typeContext);
			EnsureCapacity(7);
			InternalEmit(opcode);
			UpdateStackSize(opcode, 1);
			m_length = PutInteger4(tokenFor, m_length, m_ILStream);
		}

		public override void Emit(OpCode opcode, Type type)
		{
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}
			Emit(opcode, type.TypeHandle);
		}

		public void Emit(OpCode opcode, RuntimeTypeHandle typeHandle)
		{
			if (typeHandle.IsNullHandle())
			{
				throw new ArgumentNullException("typeHandle");
			}
			int tokenFor = m_scope.GetTokenFor(typeHandle);
			EnsureCapacity(7);
			InternalEmit(opcode);
			m_length = PutInteger4(tokenFor, m_length, m_ILStream);
		}

		public override void Emit(OpCode opcode, FieldInfo field)
		{
			if (field == null)
			{
				throw new ArgumentNullException("field");
			}
			if (!(field is RuntimeFieldInfo))
			{
				throw new ArgumentNullException("field");
			}
			if (field.DeclaringType == null)
			{
				Emit(opcode, field.FieldHandle);
			}
			else
			{
				Emit(opcode, field.FieldHandle, field.DeclaringType.GetTypeHandleInternal());
			}
		}

		public void Emit(OpCode opcode, RuntimeFieldHandle fieldHandle)
		{
			if (fieldHandle.IsNullHandle())
			{
				throw new ArgumentNullException("fieldHandle");
			}
			int tokenFor = m_scope.GetTokenFor(fieldHandle);
			EnsureCapacity(7);
			InternalEmit(opcode);
			m_length = PutInteger4(tokenFor, m_length, m_ILStream);
		}

		public void Emit(OpCode opcode, RuntimeFieldHandle fieldHandle, RuntimeTypeHandle typeContext)
		{
			if (fieldHandle.IsNullHandle())
			{
				throw new ArgumentNullException("fieldHandle");
			}
			int tokenFor = m_scope.GetTokenFor(fieldHandle, typeContext);
			EnsureCapacity(7);
			InternalEmit(opcode);
			m_length = PutInteger4(tokenFor, m_length, m_ILStream);
		}

		public override void Emit(OpCode opcode, string str)
		{
			if (str == null)
			{
				throw new ArgumentNullException("str");
			}
			int num = AddStringLiteral(str);
			num |= 0x70000000;
			EnsureCapacity(7);
			InternalEmit(opcode);
			m_length = PutInteger4(num, m_length, m_ILStream);
		}

		public override void EmitCalli(OpCode opcode, CallingConventions callingConvention, Type returnType, Type[] parameterTypes, Type[] optionalParameterTypes)
		{
			int num = 0;
			if (optionalParameterTypes != null && (callingConvention & CallingConventions.VarArgs) == 0)
			{
				throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_NotAVarArgCallingConvention"));
			}
			SignatureHelper memberRefSignature = GetMemberRefSignature(callingConvention, returnType, parameterTypes, optionalParameterTypes);
			EnsureCapacity(7);
			Emit(OpCodes.Calli);
			if (returnType != typeof(void))
			{
				num++;
			}
			if (parameterTypes != null)
			{
				num -= parameterTypes.Length;
			}
			if (optionalParameterTypes != null)
			{
				num -= optionalParameterTypes.Length;
			}
			if ((callingConvention & CallingConventions.HasThis) == CallingConventions.HasThis)
			{
				num--;
			}
			num--;
			UpdateStackSize(opcode, num);
			int value = AddSignature(memberRefSignature.GetSignature(appendEndOfSig: true));
			m_length = PutInteger4(value, m_length, m_ILStream);
		}

		public override void EmitCalli(OpCode opcode, CallingConvention unmanagedCallConv, Type returnType, Type[] parameterTypes)
		{
			int num = 0;
			int num2 = 0;
			if (parameterTypes != null)
			{
				num2 = parameterTypes.Length;
			}
			SignatureHelper methodSigHelper = SignatureHelper.GetMethodSigHelper(unmanagedCallConv, returnType);
			if (parameterTypes != null)
			{
				for (int i = 0; i < num2; i++)
				{
					methodSigHelper.AddArgument(parameterTypes[i]);
				}
			}
			if (returnType != typeof(void))
			{
				num++;
			}
			if (parameterTypes != null)
			{
				num -= num2;
			}
			num--;
			UpdateStackSize(opcode, num);
			EnsureCapacity(7);
			Emit(OpCodes.Calli);
			int value = AddSignature(methodSigHelper.GetSignature(appendEndOfSig: true));
			m_length = PutInteger4(value, m_length, m_ILStream);
		}

		public override void EmitCall(OpCode opcode, MethodInfo methodInfo, Type[] optionalParameterTypes)
		{
			int num = 0;
			if (methodInfo == null)
			{
				throw new ArgumentNullException("methodInfo");
			}
			if (methodInfo.ContainsGenericParameters)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_GenericsInvalid"), "methodInfo");
			}
			if (methodInfo.DeclaringType != null && methodInfo.DeclaringType.ContainsGenericParameters)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_GenericsInvalid"), "methodInfo");
			}
			int memberRefToken = GetMemberRefToken(methodInfo, optionalParameterTypes);
			EnsureCapacity(7);
			InternalEmit(opcode);
			if (methodInfo.ReturnType != typeof(void))
			{
				num++;
			}
			num -= methodInfo.GetParameterTypes().Length;
			if (!(methodInfo is SymbolMethod) && !methodInfo.IsStatic && !opcode.Equals(OpCodes.Newobj))
			{
				num--;
			}
			if (optionalParameterTypes != null)
			{
				num -= optionalParameterTypes.Length;
			}
			UpdateStackSize(opcode, num);
			m_length = PutInteger4(memberRefToken, m_length, m_ILStream);
		}

		public override void Emit(OpCode opcode, SignatureHelper signature)
		{
			int num = 0;
			if (signature == null)
			{
				throw new ArgumentNullException("signature");
			}
			EnsureCapacity(7);
			InternalEmit(opcode);
			if (opcode.m_pop == StackBehaviour.Varpop)
			{
				num -= signature.ArgumentCount;
				num--;
				UpdateStackSize(opcode, num);
			}
			int value = AddSignature(signature.GetSignature(appendEndOfSig: true));
			m_length = PutInteger4(value, m_length, m_ILStream);
		}

		public override Label BeginExceptionBlock()
		{
			return base.BeginExceptionBlock();
		}

		public override void EndExceptionBlock()
		{
			base.EndExceptionBlock();
		}

		public override void BeginExceptFilterBlock()
		{
			throw new NotSupportedException(Environment.GetResourceString("InvalidOperation_NotAllowedInDynamicMethod"));
		}

		public override void BeginCatchBlock(Type exceptionType)
		{
			if (m_currExcStackCount == 0)
			{
				throw new NotSupportedException(Environment.GetResourceString("Argument_NotInExceptionBlock"));
			}
			__ExceptionInfo _ExceptionInfo = m_currExcStack[m_currExcStackCount - 1];
			if (_ExceptionInfo.GetCurrentState() == 1)
			{
				if (exceptionType != null)
				{
					throw new ArgumentException(Environment.GetResourceString("Argument_ShouldNotSpecifyExceptionType"));
				}
				Emit(OpCodes.Endfilter);
			}
			else
			{
				if (exceptionType == null)
				{
					throw new ArgumentNullException("exceptionType");
				}
				if (exceptionType.GetType() != typeof(RuntimeType))
				{
					throw new ArgumentException(Environment.GetResourceString("Argument_MustBeRuntimeType"));
				}
				Label endLabel = _ExceptionInfo.GetEndLabel();
				Emit(OpCodes.Leave, endLabel);
				UpdateStackSize(OpCodes.Nop, 1);
			}
			_ExceptionInfo.MarkCatchAddr(m_length, exceptionType);
			_ExceptionInfo.m_filterAddr[_ExceptionInfo.m_currentCatch - 1] = m_scope.GetTokenFor(exceptionType.TypeHandle);
		}

		public override void BeginFaultBlock()
		{
			throw new NotSupportedException(Environment.GetResourceString("InvalidOperation_NotAllowedInDynamicMethod"));
		}

		public override void BeginFinallyBlock()
		{
			base.BeginFinallyBlock();
		}

		public override void UsingNamespace(string ns)
		{
			throw new NotSupportedException(Environment.GetResourceString("InvalidOperation_NotAllowedInDynamicMethod"));
		}

		public override void MarkSequencePoint(ISymbolDocumentWriter document, int startLine, int startColumn, int endLine, int endColumn)
		{
			throw new NotSupportedException(Environment.GetResourceString("InvalidOperation_NotAllowedInDynamicMethod"));
		}

		public override void BeginScope()
		{
			throw new NotSupportedException(Environment.GetResourceString("InvalidOperation_NotAllowedInDynamicMethod"));
		}

		public override void EndScope()
		{
			throw new NotSupportedException(Environment.GetResourceString("InvalidOperation_NotAllowedInDynamicMethod"));
		}

		internal override int GetMaxStackSize()
		{
			return m_maxStackSize;
		}

		internal override int GetMemberRefToken(MethodBase methodInfo, Type[] optionalParameterTypes)
		{
			if (optionalParameterTypes != null && (methodInfo.CallingConvention & CallingConventions.VarArgs) == 0)
			{
				throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_NotAVarArgCallingConvention"));
			}
			if (!(methodInfo is RuntimeMethodInfo) && !(methodInfo is DynamicMethod))
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_MustBeRuntimeMethodInfo"), "methodInfo");
			}
			ParameterInfo[] parametersNoCopy = methodInfo.GetParametersNoCopy();
			Type[] array;
			if (parametersNoCopy != null && parametersNoCopy.Length != 0)
			{
				array = new Type[parametersNoCopy.Length];
				for (int i = 0; i < parametersNoCopy.Length; i++)
				{
					array[i] = parametersNoCopy[i].ParameterType;
				}
			}
			else
			{
				array = null;
			}
			SignatureHelper memberRefSignature = GetMemberRefSignature(methodInfo.CallingConvention, methodInfo.GetReturnType(), array, optionalParameterTypes);
			return m_scope.GetTokenFor(new VarArgMethod(methodInfo as MethodInfo, memberRefSignature));
		}

		internal override SignatureHelper GetMemberRefSignature(CallingConventions call, Type returnType, Type[] parameterTypes, Type[] optionalParameterTypes)
		{
			int num = ((parameterTypes != null) ? parameterTypes.Length : 0);
			SignatureHelper methodSigHelper = SignatureHelper.GetMethodSigHelper(call, returnType);
			for (int i = 0; i < num; i++)
			{
				methodSigHelper.AddArgument(parameterTypes[i]);
			}
			if (optionalParameterTypes != null && optionalParameterTypes.Length != 0)
			{
				methodSigHelper.AddSentinel();
				for (int i = 0; i < optionalParameterTypes.Length; i++)
				{
					methodSigHelper.AddArgument(optionalParameterTypes[i]);
				}
			}
			return methodSigHelper;
		}

		private int AddStringLiteral(string s)
		{
			int tokenFor = m_scope.GetTokenFor(s);
			return tokenFor | 0x70000000;
		}

		private int AddSignature(byte[] sig)
		{
			int tokenFor = m_scope.GetTokenFor(sig);
			return tokenFor | 0x11000000;
		}
	}
}
