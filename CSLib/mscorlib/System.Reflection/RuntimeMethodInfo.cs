using System.Diagnostics;
using System.Globalization;
using System.Reflection.Emit;
using System.Runtime.ConstrainedExecution;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Permissions;
using System.Threading;

namespace System.Reflection
{
	[Serializable]
	internal sealed class RuntimeMethodInfo : MethodInfo, ISerializable
	{
		private RuntimeMethodHandle m_handle;

		private RuntimeType.RuntimeTypeCache m_reflectedTypeCache;

		private string m_name;

		private string m_toString;

		private ParameterInfo[] m_parameters;

		private ParameterInfo m_returnParameter;

		private BindingFlags m_bindingFlags;

		private MethodAttributes m_methodAttributes;

		private Signature m_signature;

		private RuntimeType m_declaringType;

		private uint m_invocationFlags;

		private RuntimeTypeHandle ReflectedTypeHandle => m_reflectedTypeCache.RuntimeTypeHandle;

		internal Signature Signature
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

		internal BindingFlags BindingFlags => m_bindingFlags;

		public override string Name
		{
			get
			{
				if (m_name == null)
				{
					m_name = m_handle.GetName();
				}
				return m_name;
			}
		}

		public override Type DeclaringType
		{
			get
			{
				if (m_reflectedTypeCache.IsGlobal)
				{
					return null;
				}
				return m_declaringType;
			}
		}

		public override Type ReflectedType
		{
			get
			{
				if (m_reflectedTypeCache.IsGlobal)
				{
					return null;
				}
				return m_reflectedTypeCache.RuntimeType;
			}
		}

		public override MemberTypes MemberType => MemberTypes.Method;

		public override int MetadataToken => m_handle.GetMethodDef();

		public override Module Module => m_declaringType.Module;

		internal override bool IsOverloaded => m_reflectedTypeCache.GetMethodList(MemberListType.CaseSensitive, Name).Count > 1;

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

		public override Type ReturnType => Signature.ReturnTypeHandle.GetRuntimeType();

		public override ICustomAttributeProvider ReturnTypeCustomAttributes => ReturnParameter;

		public override ParameterInfo ReturnParameter
		{
			get
			{
				FetchReturnParameter();
				return m_returnParameter;
			}
		}

		public override bool IsGenericMethod => m_handle.HasMethodInstantiation();

		public override bool IsGenericMethodDefinition => m_handle.IsGenericMethodDefinition();

		public override bool ContainsGenericParameters
		{
			get
			{
				if (DeclaringType != null && DeclaringType.ContainsGenericParameters)
				{
					return true;
				}
				if (!IsGenericMethod)
				{
					return false;
				}
				Type[] genericArguments = GetGenericArguments();
				for (int i = 0; i < genericArguments.Length; i++)
				{
					if (genericArguments[i].ContainsGenericParameters)
					{
						return true;
					}
				}
				return false;
			}
		}

		internal static string ConstructParameters(ParameterInfo[] parameters, CallingConventions callingConvention)
		{
			Type[] array = new Type[parameters.Length];
			for (int i = 0; i < parameters.Length; i++)
			{
				array[i] = parameters[i].ParameterType;
			}
			return ConstructParameters(array, callingConvention);
		}

		internal static string ConstructParameters(Type[] parameters, CallingConventions callingConvention)
		{
			string text = "";
			string str = "";
			foreach (Type type in parameters)
			{
				text += str;
				text += type.SigToString();
				if (type.IsByRef)
				{
					text = text.TrimEnd('&');
					text += " ByRef";
				}
				str = ", ";
			}
			if ((callingConvention & CallingConventions.VarArgs) == CallingConventions.VarArgs)
			{
				text += str;
				text += "...";
			}
			return text;
		}

		internal static string ConstructName(MethodBase mi)
		{
			string str = null;
			str += mi.Name;
			RuntimeMethodInfo runtimeMethodInfo = mi as RuntimeMethodInfo;
			if (runtimeMethodInfo != null && runtimeMethodInfo.IsGenericMethod)
			{
				str += runtimeMethodInfo.m_handle.ConstructInstantiation();
			}
			return str + "(" + ConstructParameters(mi.GetParametersNoCopy(), mi.CallingConvention) + ")";
		}

		internal RuntimeMethodInfo()
		{
		}

		internal RuntimeMethodInfo(RuntimeMethodHandle handle, RuntimeTypeHandle declaringTypeHandle, RuntimeType.RuntimeTypeCache reflectedTypeCache, MethodAttributes methodAttributes, BindingFlags bindingFlags)
		{
			m_toString = null;
			m_bindingFlags = bindingFlags;
			m_handle = handle;
			m_reflectedTypeCache = reflectedTypeCache;
			m_parameters = null;
			m_methodAttributes = methodAttributes;
			m_declaringType = declaringTypeHandle.GetRuntimeType();
		}

		internal ParameterInfo[] FetchNonReturnParameters()
		{
			if (m_parameters == null)
			{
				m_parameters = ParameterInfo.GetParameters(this, this, Signature);
			}
			return m_parameters;
		}

		internal ParameterInfo FetchReturnParameter()
		{
			if (m_returnParameter == null)
			{
				m_returnParameter = ParameterInfo.GetReturnParameter(this, this, Signature);
			}
			return m_returnParameter;
		}

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		internal override bool CacheEquals(object o)
		{
			return (o as RuntimeMethodInfo)?.m_handle.Equals(m_handle) ?? false;
		}

		internal override RuntimeMethodHandle GetMethodHandle()
		{
			return m_handle;
		}

		internal override MethodInfo GetParentDefinition()
		{
			if (!base.IsVirtual || m_declaringType.IsInterface)
			{
				return null;
			}
			Type baseType = m_declaringType.BaseType;
			if (baseType == null)
			{
				return null;
			}
			int slot = m_handle.GetSlot();
			if (baseType.GetTypeHandleInternal().GetNumVirtuals() <= slot)
			{
				return null;
			}
			return (MethodInfo)RuntimeType.GetMethodBase(baseType.GetTypeHandleInternal(), baseType.GetTypeHandleInternal().GetMethodAt(slot));
		}

		internal override uint GetOneTimeFlags()
		{
			uint num = 0u;
			if (ReturnType.IsByRef)
			{
				num = 2u;
			}
			return num | base.GetOneTimeFlags();
		}

		public override string ToString()
		{
			if (m_toString == null)
			{
				m_toString = ReturnType.SigToString() + " " + ConstructName(this);
			}
			return m_toString;
		}

		public override int GetHashCode()
		{
			return GetMethodHandle().GetHashCode();
		}

		public override bool Equals(object obj)
		{
			if (!IsGenericMethod)
			{
				return obj == this;
			}
			RuntimeMethodInfo runtimeMethodInfo = obj as RuntimeMethodInfo;
			RuntimeMethodHandle left = GetMethodHandle().StripMethodInstantiation();
			RuntimeMethodHandle right = runtimeMethodInfo.GetMethodHandle().StripMethodInstantiation();
			if (left != right)
			{
				return false;
			}
			if (runtimeMethodInfo == null || !runtimeMethodInfo.IsGenericMethod)
			{
				return false;
			}
			Type[] genericArguments = GetGenericArguments();
			Type[] genericArguments2 = runtimeMethodInfo.GetGenericArguments();
			if (genericArguments.Length != genericArguments2.Length)
			{
				return false;
			}
			for (int i = 0; i < genericArguments.Length; i++)
			{
				if (genericArguments[i] != genericArguments2[i])
				{
					return false;
				}
			}
			if (runtimeMethodInfo.IsGenericMethod)
			{
				if (DeclaringType != runtimeMethodInfo.DeclaringType)
				{
					return false;
				}
				if (ReflectedType != runtimeMethodInfo.ReflectedType)
				{
					return false;
				}
			}
			return true;
		}

		public override object[] GetCustomAttributes(bool inherit)
		{
			return CustomAttribute.GetCustomAttributes(this, typeof(object) as RuntimeType, inherit);
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
			return CustomAttribute.GetCustomAttributes(this, runtimeType, inherit);
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
			return CustomAttribute.IsDefined(this, runtimeType, inherit);
		}

		internal override ParameterInfo[] GetParametersNoCopy()
		{
			FetchNonReturnParameters();
			return m_parameters;
		}

		public override ParameterInfo[] GetParameters()
		{
			FetchNonReturnParameters();
			if (m_parameters.Length == 0)
			{
				return m_parameters;
			}
			ParameterInfo[] array = new ParameterInfo[m_parameters.Length];
			Array.Copy(m_parameters, array, m_parameters.Length);
			return array;
		}

		public override MethodImplAttributes GetMethodImplementationFlags()
		{
			return m_handle.GetImplAttributes();
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

		private void CheckConsistency(object target)
		{
			if ((m_methodAttributes & MethodAttributes.Static) != MethodAttributes.Static && !m_declaringType.IsInstanceOfType(target))
			{
				if (target == null)
				{
					throw new TargetException(Environment.GetResourceString("RFLCT.Targ_StatMethReqTarg"));
				}
				throw new TargetException(Environment.GetResourceString("RFLCT.Targ_ITargMismatch"));
			}
		}

		private void ThrowNoInvokeException()
		{
			Type declaringType = DeclaringType;
			if ((declaringType == null && Module.Assembly.ReflectionOnly) || declaringType is ReflectionOnlyType)
			{
				throw new InvalidOperationException(Environment.GetResourceString("Arg_ReflectionOnlyInvoke"));
			}
			if (DeclaringType.GetRootElementType() == typeof(ArgIterator))
			{
				throw new NotSupportedException();
			}
			if ((CallingConvention & CallingConventions.VarArgs) == CallingConventions.VarArgs)
			{
				throw new NotSupportedException();
			}
			if (DeclaringType.ContainsGenericParameters || ContainsGenericParameters)
			{
				throw new InvalidOperationException(Environment.GetResourceString("Arg_UnboundGenParam"));
			}
			if (base.IsAbstract)
			{
				throw new MemberAccessException();
			}
			if (ReturnType.IsByRef)
			{
				throw new NotSupportedException(Environment.GetResourceString("NotSupported_ByRefReturn"));
			}
			throw new TargetException();
		}

		[DebuggerStepThrough]
		[DebuggerHidden]
		public override object Invoke(object obj, BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture)
		{
			return Invoke(obj, invokeAttr, binder, parameters, culture, skipVisibilityChecks: false);
		}

		[DebuggerStepThrough]
		[DebuggerHidden]
		internal object Invoke(object obj, BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture, bool skipVisibilityChecks)
		{
			int num = Signature.Arguments.Length;
			int num2 = ((parameters != null) ? parameters.Length : 0);
			if ((m_invocationFlags & 1) == 0)
			{
				m_invocationFlags = GetOneTimeFlags();
			}
			if ((m_invocationFlags & 2u) != 0)
			{
				ThrowNoInvokeException();
			}
			CheckConsistency(obj);
			if (num != num2)
			{
				throw new TargetParameterCountException(Environment.GetResourceString("Arg_ParmCnt"));
			}
			if (num2 > 65535)
			{
				throw new TargetParameterCountException(Environment.GetResourceString("NotSupported_TooManyArgs"));
			}
			if (!skipVisibilityChecks && (m_invocationFlags & 0x24u) != 0)
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
			RuntimeTypeHandle typeOwner = RuntimeTypeHandle.EmptyHandle;
			if (!m_reflectedTypeCache.IsGlobal)
			{
				typeOwner = m_declaringType.TypeHandle;
			}
			if (num2 == 0)
			{
				return m_handle.InvokeMethodFast(obj, null, Signature, m_methodAttributes, typeOwner);
			}
			object[] array = CheckArguments(parameters, binder, invokeAttr, culture, Signature);
			object result = m_handle.InvokeMethodFast(obj, array, Signature, m_methodAttributes, typeOwner);
			for (int i = 0; i < num2; i++)
			{
				parameters[i] = array[i];
			}
			return result;
		}

		public override MethodInfo GetBaseDefinition()
		{
			if (!base.IsVirtual || base.IsStatic || m_declaringType == null || m_declaringType.IsInterface)
			{
				return this;
			}
			int slot = m_handle.GetSlot();
			Type type = DeclaringType;
			Type type2 = DeclaringType;
			RuntimeMethodHandle methodHandle = default(RuntimeMethodHandle);
			do
			{
				RuntimeTypeHandle typeHandleInternal = type.GetTypeHandleInternal();
				int numVirtuals = typeHandleInternal.GetNumVirtuals();
				if (numVirtuals <= slot)
				{
					break;
				}
				methodHandle = typeHandleInternal.GetMethodAt(slot);
				type2 = type;
				type = type.BaseType;
			}
			while (type != null);
			return (MethodInfo)RuntimeType.GetMethodBase(type2.GetTypeHandleInternal(), methodHandle);
		}

		public override MethodInfo MakeGenericMethod(params Type[] methodInstantiation)
		{
			if (methodInstantiation == null)
			{
				throw new ArgumentNullException("methodInstantiation");
			}
			Type[] array = new Type[methodInstantiation.Length];
			for (int i = 0; i < methodInstantiation.Length; i++)
			{
				array[i] = methodInstantiation[i];
			}
			methodInstantiation = array;
			if (!IsGenericMethodDefinition)
			{
				throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Arg_NotGenericMethodDefinition"), this));
			}
			for (int j = 0; j < methodInstantiation.Length; j++)
			{
				if (methodInstantiation[j] == null)
				{
					throw new ArgumentNullException();
				}
				if (!(methodInstantiation[j] is RuntimeType))
				{
					return MethodBuilderInstantiation.MakeGenericMethod(this, methodInstantiation);
				}
			}
			Type[] genericArguments = GetGenericArguments();
			RuntimeType.SanityCheckGenericArguments(methodInstantiation, genericArguments);
			RuntimeTypeHandle[] array2 = new RuntimeTypeHandle[methodInstantiation.Length];
			for (int k = 0; k < methodInstantiation.Length; k++)
			{
				ref RuntimeTypeHandle reference = ref array2[k];
				reference = methodInstantiation[k].GetTypeHandleInternal();
			}
			MethodInfo methodInfo = null;
			try
			{
				return RuntimeType.GetMethodBase(m_reflectedTypeCache.RuntimeTypeHandle, m_handle.GetInstantiatingStub(m_declaringType.GetTypeHandleInternal(), array2)) as MethodInfo;
			}
			catch (VerificationException ex)
			{
				RuntimeType.ValidateGenericArguments(this, methodInstantiation, ex);
				throw ex;
			}
		}

		public override Type[] GetGenericArguments()
		{
			RuntimeType[] array = null;
			RuntimeTypeHandle[] methodInstantiation = m_handle.GetMethodInstantiation();
			if (methodInstantiation != null)
			{
				array = new RuntimeType[methodInstantiation.Length];
				for (int i = 0; i < methodInstantiation.Length; i++)
				{
					array[i] = methodInstantiation[i].GetRuntimeType();
				}
			}
			else
			{
				array = new RuntimeType[0];
			}
			return array;
		}

		public override MethodInfo GetGenericMethodDefinition()
		{
			if (!IsGenericMethod)
			{
				throw new InvalidOperationException();
			}
			return RuntimeType.GetMethodBase(m_declaringType.GetTypeHandleInternal(), m_handle.StripMethodInstantiation()) as MethodInfo;
		}

		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			if (m_reflectedTypeCache.IsGlobal)
			{
				throw new NotSupportedException(Environment.GetResourceString("NotSupported_GlobalMethodSerialization"));
			}
			MemberInfoSerializationHolder.GetSerializationInfo(info, Name, ReflectedTypeHandle.GetRuntimeType(), ToString(), MemberTypes.Method, (IsGenericMethod & !IsGenericMethodDefinition) ? GetGenericArguments() : null);
		}

		internal static MethodBase InternalGetCurrentMethod(ref StackCrawlMark stackMark)
		{
			RuntimeMethodHandle currentMethod = RuntimeMethodHandle.GetCurrentMethod(ref stackMark);
			if (currentMethod.IsNullHandle())
			{
				return null;
			}
			currentMethod = currentMethod.GetTypicalMethodDefinition();
			return RuntimeType.GetMethodBase(currentMethod);
		}
	}
}
