using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Permissions;

namespace System.Reflection
{
	[Serializable]
	internal sealed class RuntimeConstructorInfo : ConstructorInfo, ISerializable
	{
		private RuntimeMethodHandle m_handle;

		private RuntimeType.RuntimeTypeCache m_reflectedTypeCache;

		private RuntimeType m_declaringType;

		private string m_toString;

		private MethodAttributes m_methodAttributes;

		private BindingFlags m_bindingFlags;

		private ParameterInfo[] m_parameters;

		private uint m_invocationFlags;

		private Signature m_signature;

		private Signature Signature
		{
			get
			{
				if (m_signature == null)
				{
					m_signature = new Signature(m_handle, m_declaringType.GetTypeHandleInternal());
				}
				return m_signature;
			}
		}

		private RuntimeTypeHandle ReflectedTypeHandle => m_reflectedTypeCache.RuntimeTypeHandle;

		internal BindingFlags BindingFlags => m_bindingFlags;

		internal override bool IsOverloaded => m_reflectedTypeCache.GetConstructorList(MemberListType.CaseSensitive, Name).Count > 1;

		public override string Name => m_handle.GetName();

		[ComVisible(true)]
		public override MemberTypes MemberType => MemberTypes.Constructor;

		public override Type DeclaringType
		{
			get
			{
				if (!m_reflectedTypeCache.IsGlobal)
				{
					return m_declaringType;
				}
				return null;
			}
		}

		public override Type ReflectedType
		{
			get
			{
				if (!m_reflectedTypeCache.IsGlobal)
				{
					return m_reflectedTypeCache.RuntimeType;
				}
				return null;
			}
		}

		public override int MetadataToken => m_handle.GetMethodDef();

		public override Module Module => m_declaringType.GetTypeHandleInternal().GetModuleHandle().GetModule();

		public override RuntimeMethodHandle MethodHandle
		{
			get
			{
				Type declaringType = DeclaringType;
				if ((declaringType == null && Module.Assembly.ReflectionOnly) || declaringType is ReflectionOnlyType)
				{
					throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_NotAllowedInReflectionOnly"));
				}
				return m_handle;
			}
		}

		public override MethodAttributes Attributes => m_methodAttributes;

		public override CallingConventions CallingConvention => Signature.CallingConvention;

		internal RuntimeConstructorInfo()
		{
		}

		internal RuntimeConstructorInfo(RuntimeMethodHandle handle, RuntimeTypeHandle declaringTypeHandle, RuntimeType.RuntimeTypeCache reflectedTypeCache, MethodAttributes methodAttributes, BindingFlags bindingFlags)
		{
			m_bindingFlags = bindingFlags;
			m_handle = handle;
			m_reflectedTypeCache = reflectedTypeCache;
			m_declaringType = declaringTypeHandle.GetRuntimeType();
			m_parameters = null;
			m_toString = null;
			m_methodAttributes = methodAttributes;
		}

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		internal override bool CacheEquals(object o)
		{
			return (o as RuntimeConstructorInfo)?.m_handle.Equals(m_handle) ?? false;
		}

		private void CheckConsistency(object target)
		{
			if ((target != null || !base.IsStatic) && !m_declaringType.IsInstanceOfType(target))
			{
				if (target == null)
				{
					throw new TargetException(Environment.GetResourceString("RFLCT.Targ_StatMethReqTarg"));
				}
				throw new TargetException(Environment.GetResourceString("RFLCT.Targ_ITargMismatch"));
			}
		}

		internal override RuntimeMethodHandle GetMethodHandle()
		{
			return m_handle;
		}

		internal override uint GetOneTimeSpecificFlags()
		{
			uint num = 16u;
			if ((DeclaringType != null && DeclaringType.IsAbstract) || base.IsStatic)
			{
				num |= 8u;
			}
			else if (DeclaringType == typeof(void))
			{
				num |= 2u;
			}
			else if (typeof(Delegate).IsAssignableFrom(DeclaringType))
			{
				num |= 0x80u;
			}
			return num;
		}

		public override string ToString()
		{
			if (m_toString == null)
			{
				m_toString = "Void " + RuntimeMethodInfo.ConstructName(this);
			}
			return m_toString;
		}

		public override object[] GetCustomAttributes(bool inherit)
		{
			return CustomAttribute.GetCustomAttributes(this, typeof(object) as RuntimeType);
		}

		public override object[] GetCustomAttributes(Type attributeType, bool inherit)
		{
			if (attributeType == null)
			{
				throw new ArgumentNullException("attributeType");
			}
			RuntimeType runtimeType = attributeType.UnderlyingSystemType as RuntimeType;
			if (runtimeType == null)
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_MustBeType"), "attributeType");
			}
			return CustomAttribute.GetCustomAttributes(this, runtimeType);
		}

		public override bool IsDefined(Type attributeType, bool inherit)
		{
			if (attributeType == null)
			{
				throw new ArgumentNullException("attributeType");
			}
			RuntimeType runtimeType = attributeType.UnderlyingSystemType as RuntimeType;
			if (runtimeType == null)
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_MustBeType"), "attributeType");
			}
			return CustomAttribute.IsDefined(this, runtimeType);
		}

		internal override Type GetReturnType()
		{
			return Signature.ReturnTypeHandle.GetRuntimeType();
		}

		internal override ParameterInfo[] GetParametersNoCopy()
		{
			if (m_parameters == null)
			{
				m_parameters = ParameterInfo.GetParameters(this, this, Signature);
			}
			return m_parameters;
		}

		public override ParameterInfo[] GetParameters()
		{
			ParameterInfo[] parametersNoCopy = GetParametersNoCopy();
			if (parametersNoCopy.Length == 0)
			{
				return parametersNoCopy;
			}
			ParameterInfo[] array = new ParameterInfo[parametersNoCopy.Length];
			Array.Copy(parametersNoCopy, array, parametersNoCopy.Length);
			return array;
		}

		public override MethodImplAttributes GetMethodImplementationFlags()
		{
			return m_handle.GetImplAttributes();
		}

		internal static void CheckCanCreateInstance(Type declaringType, bool isVarArg)
		{
			if (declaringType == null)
			{
				throw new ArgumentNullException("declaringType");
			}
			if (declaringType is ReflectionOnlyType)
			{
				throw new InvalidOperationException(Environment.GetResourceString("Arg_ReflectionOnlyInvoke"));
			}
			if (declaringType.IsInterface)
			{
				throw new MemberAccessException(string.Format(CultureInfo.CurrentUICulture, Environment.GetResourceString("Acc_CreateInterfaceEx"), declaringType));
			}
			if (declaringType.IsAbstract)
			{
				throw new MemberAccessException(string.Format(CultureInfo.CurrentUICulture, Environment.GetResourceString("Acc_CreateAbstEx"), declaringType));
			}
			if (declaringType.GetRootElementType() == typeof(ArgIterator))
			{
				throw new NotSupportedException();
			}
			if (isVarArg)
			{
				throw new NotSupportedException();
			}
			if (declaringType.ContainsGenericParameters)
			{
				throw new MemberAccessException(string.Format(CultureInfo.CurrentUICulture, Environment.GetResourceString("Acc_CreateGenericEx"), declaringType));
			}
			if (declaringType == typeof(void))
			{
				throw new MemberAccessException(Environment.GetResourceString("Access_Void"));
			}
		}

		internal void ThrowNoInvokeException()
		{
			CheckCanCreateInstance(DeclaringType, (CallingConvention & CallingConventions.VarArgs) == CallingConventions.VarArgs);
			if ((Attributes & MethodAttributes.Static) == MethodAttributes.Static)
			{
				throw new MemberAccessException(Environment.GetResourceString("Acc_NotClassInit"));
			}
			throw new TargetException();
		}

		[DebuggerStepThrough]
		[DebuggerHidden]
		public override object Invoke(object obj, BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture)
		{
			if (m_invocationFlags == 0)
			{
				m_invocationFlags = GetOneTimeFlags();
			}
			if ((m_invocationFlags & 2u) != 0)
			{
				ThrowNoInvokeException();
			}
			CheckConsistency(obj);
			if (obj != null)
			{
				new SecurityPermission(SecurityPermissionFlag.SkipVerification).Demand();
			}
			if ((m_invocationFlags & 0x24u) != 0)
			{
				if ((m_invocationFlags & 0x20u) != 0)
				{
					CodeAccessPermission.DemandInternal(PermissionType.ReflectionMemberAccess);
				}
				if ((m_invocationFlags & 4u) != 0)
				{
					MethodBase.PerformSecurityCheck(obj, m_handle, m_declaringType.TypeHandle.Value, m_invocationFlags);
				}
			}
			int num = Signature.Arguments.Length;
			int num2 = ((parameters != null) ? parameters.Length : 0);
			if (num != num2)
			{
				throw new TargetParameterCountException(Environment.GetResourceString("Arg_ParmCnt"));
			}
			if (num2 > 0)
			{
				object[] array = CheckArguments(parameters, binder, invokeAttr, culture, Signature);
				object result = m_handle.InvokeMethodFast(obj, array, Signature, m_methodAttributes, (ReflectedType != null) ? ReflectedType.TypeHandle : RuntimeTypeHandle.EmptyHandle);
				for (int i = 0; i < num2; i++)
				{
					parameters[i] = array[i];
				}
				return result;
			}
			return m_handle.InvokeMethodFast(obj, null, Signature, m_methodAttributes, (DeclaringType != null) ? DeclaringType.TypeHandle : RuntimeTypeHandle.EmptyHandle);
		}

		[ReflectionPermission(SecurityAction.Demand, Flags = ReflectionPermissionFlag.MemberAccess)]
		public override MethodBody GetMethodBody()
		{
			MethodBody methodBody = m_handle.GetMethodBody(ReflectedTypeHandle);
			if (methodBody != null)
			{
				methodBody.m_methodBase = this;
			}
			return methodBody;
		}

		[DebuggerHidden]
		[DebuggerStepThrough]
		public override object Invoke(BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture)
		{
			RuntimeTypeHandle typeHandle = m_declaringType.TypeHandle;
			if (m_invocationFlags == 0)
			{
				m_invocationFlags = GetOneTimeFlags();
			}
			if ((m_invocationFlags & 0x10Au) != 0)
			{
				ThrowNoInvokeException();
			}
			if ((m_invocationFlags & 0xA4u) != 0)
			{
				if ((m_invocationFlags & 0x20u) != 0)
				{
					CodeAccessPermission.DemandInternal(PermissionType.ReflectionMemberAccess);
				}
				if ((m_invocationFlags & 4u) != 0)
				{
					MethodBase.PerformSecurityCheck(null, m_handle, m_declaringType.TypeHandle.Value, m_invocationFlags & 0x10000000u);
				}
				if ((m_invocationFlags & 0x80u) != 0)
				{
					new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Demand();
				}
			}
			int num = Signature.Arguments.Length;
			int num2 = ((parameters != null) ? parameters.Length : 0);
			if (num != num2)
			{
				throw new TargetParameterCountException(Environment.GetResourceString("Arg_ParmCnt"));
			}
			RuntimeHelpers.RunClassConstructor(typeHandle);
			if (num2 > 0)
			{
				object[] array = CheckArguments(parameters, binder, invokeAttr, culture, Signature);
				object result = m_handle.InvokeConstructor(array, Signature, typeHandle);
				for (int i = 0; i < num2; i++)
				{
					parameters[i] = array[i];
				}
				return result;
			}
			return m_handle.InvokeConstructor(null, Signature, typeHandle);
		}

		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			MemberInfoSerializationHolder.GetSerializationInfo(info, Name, ReflectedTypeHandle.GetRuntimeType(), ToString(), MemberTypes.Constructor);
		}

		internal void SerializationInvoke(object target, SerializationInfo info, StreamingContext context)
		{
			MethodHandle.SerializationInvoke(target, Signature, info, context);
		}
	}
}
