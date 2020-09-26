using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Threading;

namespace System.Reflection.Emit
{
	[ComVisible(true)]
	public sealed class DynamicMethod : MethodInfo
	{
		internal class RTDynamicMethod : MethodInfo
		{
			private class EmptyCAHolder : ICustomAttributeProvider
			{
				internal EmptyCAHolder()
				{
				}

				object[] ICustomAttributeProvider.GetCustomAttributes(Type attributeType, bool inherit)
				{
					return new object[0];
				}

				object[] ICustomAttributeProvider.GetCustomAttributes(bool inherit)
				{
					return new object[0];
				}

				bool ICustomAttributeProvider.IsDefined(Type attributeType, bool inherit)
				{
					return false;
				}
			}

			internal DynamicMethod m_owner;

			private ParameterInfo[] m_parameters;

			private string m_name;

			private MethodAttributes m_attributes;

			private CallingConventions m_callingConvention;

			public override string Name => m_name;

			public override Type DeclaringType => null;

			public override Type ReflectedType => null;

			internal override int MetadataTokenInternal => 0;

			public override Module Module => m_owner.m_module.GetModule();

			public override RuntimeMethodHandle MethodHandle
			{
				get
				{
					throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_NotAllowedInDynamicMethod"));
				}
			}

			public override MethodAttributes Attributes => m_attributes;

			public override CallingConventions CallingConvention => m_callingConvention;

			public override ParameterInfo ReturnParameter => null;

			public override ICustomAttributeProvider ReturnTypeCustomAttributes => GetEmptyCAHolder();

			internal override bool IsOverloaded => false;

			private RTDynamicMethod()
			{
			}

			internal RTDynamicMethod(DynamicMethod owner, string name, MethodAttributes attributes, CallingConventions callingConvention)
			{
				m_owner = owner;
				m_name = name;
				m_attributes = attributes;
				m_callingConvention = callingConvention;
			}

			public override string ToString()
			{
				return ReturnType.SigToString() + " " + RuntimeMethodInfo.ConstructName(this);
			}

			public override MethodInfo GetBaseDefinition()
			{
				return this;
			}

			public override ParameterInfo[] GetParameters()
			{
				ParameterInfo[] array = LoadParameters();
				ParameterInfo[] array2 = new ParameterInfo[array.Length];
				Array.Copy(array, array2, array.Length);
				return array2;
			}

			public override MethodImplAttributes GetMethodImplementationFlags()
			{
				return MethodImplAttributes.NoInlining;
			}

			public override object Invoke(object obj, BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_MustBeRuntimeMethodInfo"), "this");
			}

			public override object[] GetCustomAttributes(Type attributeType, bool inherit)
			{
				if (attributeType == null)
				{
					throw new ArgumentNullException("attributeType");
				}
				if (attributeType.IsAssignableFrom(typeof(MethodImplAttribute)))
				{
					return new object[1]
					{
						new MethodImplAttribute(GetMethodImplementationFlags())
					};
				}
				return new object[0];
			}

			public override object[] GetCustomAttributes(bool inherit)
			{
				return new object[1]
				{
					new MethodImplAttribute(GetMethodImplementationFlags())
				};
			}

			public override bool IsDefined(Type attributeType, bool inherit)
			{
				if (attributeType == null)
				{
					throw new ArgumentNullException("attributeType");
				}
				if (attributeType.IsAssignableFrom(typeof(MethodImplAttribute)))
				{
					return true;
				}
				return false;
			}

			internal override Type GetReturnType()
			{
				return m_owner.m_returnType;
			}

			internal ParameterInfo[] LoadParameters()
			{
				if (m_parameters == null)
				{
					RuntimeType[] parameterTypes = m_owner.m_parameterTypes;
					ParameterInfo[] array = new ParameterInfo[parameterTypes.Length];
					for (int i = 0; i < parameterTypes.Length; i++)
					{
						array[i] = new ParameterInfo(this, null, parameterTypes[i], i);
					}
					if (m_parameters == null)
					{
						m_parameters = array;
					}
				}
				return m_parameters;
			}

			private ICustomAttributeProvider GetEmptyCAHolder()
			{
				return new EmptyCAHolder();
			}
		}

		private RuntimeType[] m_parameterTypes;

		private RuntimeType m_returnType;

		private DynamicILGenerator m_ilGenerator;

		private DynamicILInfo m_DynamicILInfo;

		private bool m_fInitLocals;

		internal RuntimeMethodHandle m_method;

		internal ModuleHandle m_module;

		internal bool m_skipVisibility;

		internal RuntimeType m_typeOwner;

		private RTDynamicMethod m_dynMethod;

		internal DynamicResolver m_resolver;

		internal bool m_restrictedSkipVisibility;

		internal CompressedStack m_creationContext;

		private static Module s_anonymouslyHostedDynamicMethodsModule;

		private static readonly object s_anonymouslyHostedDynamicMethodsModuleLock = new object();

		public override string Name => m_dynMethod.Name;

		public override Type DeclaringType => m_dynMethod.DeclaringType;

		public override Type ReflectedType => m_dynMethod.ReflectedType;

		internal override int MetadataTokenInternal => m_dynMethod.MetadataTokenInternal;

		public override Module Module => m_dynMethod.Module;

		public override RuntimeMethodHandle MethodHandle
		{
			get
			{
				throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_NotAllowedInDynamicMethod"));
			}
		}

		public override MethodAttributes Attributes => m_dynMethod.Attributes;

		public override CallingConventions CallingConvention => m_dynMethod.CallingConvention;

		public override Type ReturnType => m_dynMethod.ReturnType;

		public override ParameterInfo ReturnParameter => m_dynMethod.ReturnParameter;

		public override ICustomAttributeProvider ReturnTypeCustomAttributes => m_dynMethod.ReturnTypeCustomAttributes;

		internal override bool IsOverloaded => m_dynMethod.IsOverloaded;

		public bool InitLocals
		{
			get
			{
				return m_fInitLocals;
			}
			set
			{
				m_fInitLocals = value;
			}
		}

		private DynamicMethod()
		{
		}

		public DynamicMethod(string name, Type returnType, Type[] parameterTypes)
		{
			Init(name, MethodAttributes.Public | MethodAttributes.Static, CallingConventions.Standard, returnType, parameterTypes, null, null, skipVisibility: false, transparentMethod: true);
		}

		public DynamicMethod(string name, Type returnType, Type[] parameterTypes, bool restrictedSkipVisibility)
		{
			Init(name, MethodAttributes.Public | MethodAttributes.Static, CallingConventions.Standard, returnType, parameterTypes, null, null, restrictedSkipVisibility, transparentMethod: true);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public DynamicMethod(string name, Type returnType, Type[] parameterTypes, Module m)
		{
			StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
			PerformSecurityCheck(m, ref stackMark, skipVisibility: false);
			Init(name, MethodAttributes.Public | MethodAttributes.Static, CallingConventions.Standard, returnType, parameterTypes, null, m, skipVisibility: false, transparentMethod: false);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public DynamicMethod(string name, Type returnType, Type[] parameterTypes, Module m, bool skipVisibility)
		{
			StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
			PerformSecurityCheck(m, ref stackMark, skipVisibility);
			Init(name, MethodAttributes.Public | MethodAttributes.Static, CallingConventions.Standard, returnType, parameterTypes, null, m, skipVisibility, transparentMethod: false);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public DynamicMethod(string name, MethodAttributes attributes, CallingConventions callingConvention, Type returnType, Type[] parameterTypes, Module m, bool skipVisibility)
		{
			StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
			PerformSecurityCheck(m, ref stackMark, skipVisibility);
			Init(name, attributes, callingConvention, returnType, parameterTypes, null, m, skipVisibility, transparentMethod: false);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public DynamicMethod(string name, Type returnType, Type[] parameterTypes, Type owner)
		{
			StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
			PerformSecurityCheck(owner, ref stackMark, skipVisibility: false);
			Init(name, MethodAttributes.Public | MethodAttributes.Static, CallingConventions.Standard, returnType, parameterTypes, owner, null, skipVisibility: false, transparentMethod: false);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public DynamicMethod(string name, Type returnType, Type[] parameterTypes, Type owner, bool skipVisibility)
		{
			StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
			PerformSecurityCheck(owner, ref stackMark, skipVisibility);
			Init(name, MethodAttributes.Public | MethodAttributes.Static, CallingConventions.Standard, returnType, parameterTypes, owner, null, skipVisibility, transparentMethod: false);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public DynamicMethod(string name, MethodAttributes attributes, CallingConventions callingConvention, Type returnType, Type[] parameterTypes, Type owner, bool skipVisibility)
		{
			StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
			PerformSecurityCheck(owner, ref stackMark, skipVisibility);
			Init(name, attributes, callingConvention, returnType, parameterTypes, owner, null, skipVisibility, transparentMethod: false);
		}

		private static void CheckConsistency(MethodAttributes attributes, CallingConventions callingConvention)
		{
			if ((attributes & ~MethodAttributes.MemberAccessMask) != MethodAttributes.Static)
			{
				throw new NotSupportedException(Environment.GetResourceString("NotSupported_DynamicMethodFlags"));
			}
			if ((attributes & MethodAttributes.MemberAccessMask) != MethodAttributes.Public)
			{
				throw new NotSupportedException(Environment.GetResourceString("NotSupported_DynamicMethodFlags"));
			}
			if (callingConvention != CallingConventions.Standard && callingConvention != CallingConventions.VarArgs)
			{
				throw new NotSupportedException(Environment.GetResourceString("NotSupported_DynamicMethodFlags"));
			}
			if (callingConvention == CallingConventions.VarArgs)
			{
				throw new NotSupportedException(Environment.GetResourceString("NotSupported_DynamicMethodFlags"));
			}
		}

		private static Module GetDynamicMethodsModule()
		{
			if (s_anonymouslyHostedDynamicMethodsModule != null)
			{
				return s_anonymouslyHostedDynamicMethodsModule;
			}
			lock (s_anonymouslyHostedDynamicMethodsModuleLock)
			{
				if (s_anonymouslyHostedDynamicMethodsModule != null)
				{
					return s_anonymouslyHostedDynamicMethodsModule;
				}
				ConstructorInfo constructor = typeof(SecurityTransparentAttribute).GetConstructor(Type.EmptyTypes);
				CustomAttributeBuilder customAttributeBuilder = new CustomAttributeBuilder(constructor, new object[0]);
				CustomAttributeBuilder[] assemblyAttributes = new CustomAttributeBuilder[1]
				{
					customAttributeBuilder
				};
				AssemblyName name = new AssemblyName("Anonymously Hosted DynamicMethods Assembly");
				AssemblyBuilder assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(name, AssemblyBuilderAccess.Run, assemblyAttributes);
				AppDomain.CurrentDomain.PublishAnonymouslyHostedDynamicMethodsAssembly(assemblyBuilder.InternalAssembly);
				s_anonymouslyHostedDynamicMethodsModule = assemblyBuilder.ManifestModule;
			}
			return s_anonymouslyHostedDynamicMethodsModule;
		}

		private void Init(string name, MethodAttributes attributes, CallingConventions callingConvention, Type returnType, Type[] signature, Type owner, Module m, bool skipVisibility, bool transparentMethod)
		{
			CheckConsistency(attributes, callingConvention);
			if (signature != null)
			{
				m_parameterTypes = new RuntimeType[signature.Length];
				for (int i = 0; i < signature.Length; i++)
				{
					if (signature[i] == null)
					{
						throw new ArgumentException(Environment.GetResourceString("Arg_InvalidTypeInSignature"));
					}
					m_parameterTypes[i] = signature[i].UnderlyingSystemType as RuntimeType;
					if (m_parameterTypes[i] == null || m_parameterTypes[i] == typeof(void))
					{
						throw new ArgumentException(Environment.GetResourceString("Arg_InvalidTypeInSignature"));
					}
				}
			}
			else
			{
				m_parameterTypes = new RuntimeType[0];
			}
			m_returnType = ((returnType == null) ? ((RuntimeType)typeof(void)) : (returnType.UnderlyingSystemType as RuntimeType));
			if (m_returnType == null || m_returnType.IsByRef)
			{
				throw new NotSupportedException(Environment.GetResourceString("Arg_InvalidTypeInRetType"));
			}
			if (transparentMethod)
			{
				m_module = GetDynamicMethodsModule().ModuleHandle;
				if (skipVisibility)
				{
					m_restrictedSkipVisibility = true;
				}
				m_creationContext = CompressedStack.Capture();
			}
			else
			{
				m_typeOwner = ((owner != null) ? (owner.UnderlyingSystemType as RuntimeType) : null);
				if (m_typeOwner != null && (m_typeOwner.HasElementType || m_typeOwner.ContainsGenericParameters || m_typeOwner.IsGenericParameter || m_typeOwner.IsInterface))
				{
					throw new ArgumentException(Environment.GetResourceString("Argument_InvalidTypeForDynamicMethod"));
				}
				m_module = m?.ModuleHandle ?? m_typeOwner.Module.ModuleHandle;
				m_skipVisibility = skipVisibility;
			}
			m_ilGenerator = null;
			m_fInitLocals = true;
			m_method = new RuntimeMethodHandle(null);
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			m_dynMethod = new RTDynamicMethod(this, name, attributes, callingConvention);
		}

		private static void PerformSecurityCheck(Module m, ref StackCrawlMark stackMark, bool skipVisibility)
		{
			if (m == null)
			{
				throw new ArgumentNullException("m");
			}
			if (m.Equals(s_anonymouslyHostedDynamicMethodsModule))
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_InvalidValue"), "m");
			}
			if (skipVisibility)
			{
				new ReflectionPermission(ReflectionPermissionFlag.MemberAccess).Demand();
			}
			RuntimeTypeHandle callerType = ModuleHandle.GetCallerType(ref stackMark);
			if (!m.Assembly.AssemblyHandle.Equals(callerType.GetAssemblyHandle()) || m == typeof(object).Module)
			{
				m.Assembly.nGetGrantSet(out var newGrant, out var _);
				if (newGrant == null)
				{
					newGrant = new PermissionSet(PermissionState.Unrestricted);
				}
				CodeAccessSecurityEngine.ReflectionTargetDemandHelper(PermissionType.SecurityControlEvidence, newGrant);
			}
		}

		private static void PerformSecurityCheck(Type owner, ref StackCrawlMark stackMark, bool skipVisibility)
		{
			if (owner == null || (owner = owner.UnderlyingSystemType as RuntimeType) == null)
			{
				throw new ArgumentNullException("owner");
			}
			RuntimeTypeHandle callerType = ModuleHandle.GetCallerType(ref stackMark);
			if (skipVisibility)
			{
				new ReflectionPermission(ReflectionPermissionFlag.MemberAccess).Demand();
			}
			else if (!callerType.Equals(owner.TypeHandle))
			{
				new ReflectionPermission(ReflectionPermissionFlag.MemberAccess).Demand();
			}
			if (!owner.Assembly.AssemblyHandle.Equals(callerType.GetAssemblyHandle()) || owner.Module == typeof(object).Module)
			{
				owner.Assembly.nGetGrantSet(out var newGrant, out var _);
				if (newGrant == null)
				{
					newGrant = new PermissionSet(PermissionState.Unrestricted);
				}
				CodeAccessSecurityEngine.ReflectionTargetDemandHelper(PermissionType.SecurityControlEvidence, newGrant);
			}
		}

		[ComVisible(true)]
		public Delegate CreateDelegate(Type delegateType)
		{
			if (m_restrictedSkipVisibility)
			{
				RuntimeHelpers._CompileMethod(GetMethodDescriptor().Value);
			}
			MulticastDelegate multicastDelegate = (MulticastDelegate)Delegate.CreateDelegate(delegateType, null, GetMethodDescriptor());
			multicastDelegate.StoreDynamicMethod(GetMethodInfo());
			return multicastDelegate;
		}

		[ComVisible(true)]
		public Delegate CreateDelegate(Type delegateType, object target)
		{
			if (m_restrictedSkipVisibility)
			{
				RuntimeHelpers._CompileMethod(GetMethodDescriptor().Value);
			}
			MulticastDelegate multicastDelegate = (MulticastDelegate)Delegate.CreateDelegate(delegateType, target, GetMethodDescriptor());
			multicastDelegate.StoreDynamicMethod(GetMethodInfo());
			return multicastDelegate;
		}

		internal unsafe RuntimeMethodHandle GetMethodDescriptor()
		{
			if (m_method.IsNullHandle())
			{
				lock (this)
				{
					if (m_method.IsNullHandle())
					{
						if (m_DynamicILInfo != null)
						{
							m_method = m_DynamicILInfo.GetCallableMethod(m_module.Value);
						}
						else
						{
							if (m_ilGenerator == null || m_ilGenerator.m_length == 0)
							{
								throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("InvalidOperation_BadEmptyMethodBody"), Name));
							}
							m_method = m_ilGenerator.GetCallableMethod(m_module.Value);
						}
					}
				}
			}
			return m_method;
		}

		public override string ToString()
		{
			return m_dynMethod.ToString();
		}

		public override MethodInfo GetBaseDefinition()
		{
			return this;
		}

		public override ParameterInfo[] GetParameters()
		{
			return m_dynMethod.GetParameters();
		}

		public override MethodImplAttributes GetMethodImplementationFlags()
		{
			return m_dynMethod.GetMethodImplementationFlags();
		}

		public override object Invoke(object obj, BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture)
		{
			RuntimeMethodHandle methodDescriptor = GetMethodDescriptor();
			if ((CallingConvention & CallingConventions.VarArgs) == CallingConventions.VarArgs)
			{
				throw new NotSupportedException(Environment.GetResourceString("NotSupported_CallToVarArg"));
			}
			RuntimeTypeHandle[] array = new RuntimeTypeHandle[m_parameterTypes.Length];
			for (int i = 0; i < array.Length; i++)
			{
				ref RuntimeTypeHandle reference = ref array[i];
				reference = m_parameterTypes[i].TypeHandle;
			}
			Signature signature = new Signature(methodDescriptor, array, m_returnType.TypeHandle, CallingConvention);
			int num = signature.Arguments.Length;
			int num2 = ((parameters != null) ? parameters.Length : 0);
			if (num != num2)
			{
				throw new TargetParameterCountException(Environment.GetResourceString("Arg_ParmCnt"));
			}
			object obj2 = null;
			if (num2 > 0)
			{
				object[] array2 = CheckArguments(parameters, binder, invokeAttr, culture, signature);
				obj2 = methodDescriptor.InvokeMethodFast(null, array2, signature, Attributes, RuntimeTypeHandle.EmptyHandle);
				for (int j = 0; j < num2; j++)
				{
					parameters[j] = array2[j];
				}
			}
			else
			{
				obj2 = methodDescriptor.InvokeMethodFast(null, null, signature, Attributes, RuntimeTypeHandle.EmptyHandle);
			}
			GC.KeepAlive(this);
			return obj2;
		}

		public override object[] GetCustomAttributes(Type attributeType, bool inherit)
		{
			return m_dynMethod.GetCustomAttributes(attributeType, inherit);
		}

		public override object[] GetCustomAttributes(bool inherit)
		{
			return m_dynMethod.GetCustomAttributes(inherit);
		}

		public override bool IsDefined(Type attributeType, bool inherit)
		{
			return m_dynMethod.IsDefined(attributeType, inherit);
		}

		public ParameterBuilder DefineParameter(int position, ParameterAttributes attributes, string parameterName)
		{
			if (position < 0 || position > m_parameterTypes.Length)
			{
				throw new ArgumentOutOfRangeException(Environment.GetResourceString("ArgumentOutOfRange_ParamSequence"));
			}
			position--;
			if (position >= 0)
			{
				ParameterInfo[] array = m_dynMethod.LoadParameters();
				array[position].SetName(parameterName);
				array[position].SetAttributes(attributes);
			}
			return null;
		}

		public DynamicILInfo GetDynamicILInfo()
		{
			new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Demand();
			if (m_DynamicILInfo != null)
			{
				return m_DynamicILInfo;
			}
			return GetDynamicILInfo(new DynamicScope());
		}

		internal DynamicILInfo GetDynamicILInfo(DynamicScope scope)
		{
			if (m_DynamicILInfo == null)
			{
				byte[] signature = SignatureHelper.GetMethodSigHelper(null, CallingConvention, ReturnType, null, null, m_parameterTypes, null, null).GetSignature(appendEndOfSig: true);
				m_DynamicILInfo = new DynamicILInfo(scope, this, signature);
			}
			return m_DynamicILInfo;
		}

		public ILGenerator GetILGenerator()
		{
			return GetILGenerator(64);
		}

		public ILGenerator GetILGenerator(int streamSize)
		{
			if (m_ilGenerator == null)
			{
				byte[] signature = SignatureHelper.GetMethodSigHelper(null, CallingConvention, ReturnType, null, null, m_parameterTypes, null, null).GetSignature(appendEndOfSig: true);
				m_ilGenerator = new DynamicILGenerator(this, signature, streamSize);
			}
			return m_ilGenerator;
		}

		internal MethodInfo GetMethodInfo()
		{
			return m_dynMethod;
		}
	}
}
