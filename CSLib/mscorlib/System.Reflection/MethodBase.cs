using System.Diagnostics;
using System.Globalization;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Threading;

namespace System.Reflection
{
	[Serializable]
	[ComVisible(true)]
	[ClassInterface(ClassInterfaceType.None)]
	[ComDefaultInterface(typeof(_MethodBase))]
	[PermissionSet(SecurityAction.InheritanceDemand, Name = "FullTrust")]
	public abstract class MethodBase : MemberInfo, _MethodBase
	{
		internal virtual bool IsOverloaded
		{
			get
			{
				throw new NotSupportedException(Environment.GetResourceString("InvalidOperation_Method"));
			}
		}

		public abstract RuntimeMethodHandle MethodHandle
		{
			get;
		}

		public abstract MethodAttributes Attributes
		{
			get;
		}

		public virtual CallingConventions CallingConvention => CallingConventions.Standard;

		public virtual bool IsGenericMethodDefinition => false;

		public virtual bool ContainsGenericParameters => false;

		public virtual bool IsGenericMethod => false;

		bool _MethodBase.IsPublic => IsPublic;

		bool _MethodBase.IsPrivate => IsPrivate;

		bool _MethodBase.IsFamily => IsFamily;

		bool _MethodBase.IsAssembly => IsAssembly;

		bool _MethodBase.IsFamilyAndAssembly => IsFamilyAndAssembly;

		bool _MethodBase.IsFamilyOrAssembly => IsFamilyOrAssembly;

		bool _MethodBase.IsStatic => IsStatic;

		bool _MethodBase.IsFinal => IsFinal;

		bool _MethodBase.IsVirtual => IsVirtual;

		bool _MethodBase.IsHideBySig => IsHideBySig;

		bool _MethodBase.IsAbstract => IsAbstract;

		bool _MethodBase.IsSpecialName => IsSpecialName;

		bool _MethodBase.IsConstructor => IsConstructor;

		public bool IsPublic => (Attributes & MethodAttributes.MemberAccessMask) == MethodAttributes.Public;

		public bool IsPrivate => (Attributes & MethodAttributes.MemberAccessMask) == MethodAttributes.Private;

		public bool IsFamily => (Attributes & MethodAttributes.MemberAccessMask) == MethodAttributes.Family;

		public bool IsAssembly => (Attributes & MethodAttributes.MemberAccessMask) == MethodAttributes.Assembly;

		public bool IsFamilyAndAssembly => (Attributes & MethodAttributes.MemberAccessMask) == MethodAttributes.FamANDAssem;

		public bool IsFamilyOrAssembly => (Attributes & MethodAttributes.MemberAccessMask) == MethodAttributes.FamORAssem;

		public bool IsStatic => (Attributes & MethodAttributes.Static) != 0;

		public bool IsFinal => (Attributes & MethodAttributes.Final) != 0;

		public bool IsVirtual => (Attributes & MethodAttributes.Virtual) != 0;

		public bool IsHideBySig => (Attributes & MethodAttributes.HideBySig) != 0;

		public bool IsAbstract => (Attributes & MethodAttributes.Abstract) != 0;

		public bool IsSpecialName => (Attributes & MethodAttributes.SpecialName) != 0;

		[ComVisible(true)]
		public bool IsConstructor
		{
			get
			{
				if ((Attributes & MethodAttributes.RTSpecialName) != 0)
				{
					return Name.Equals(ConstructorInfo.ConstructorName);
				}
				return false;
			}
		}

		public static MethodBase GetMethodFromHandle(RuntimeMethodHandle handle)
		{
			if (handle.IsNullHandle())
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_InvalidHandle"));
			}
			MethodBase methodBase = RuntimeType.GetMethodBase(handle);
			if (methodBase.DeclaringType != null && methodBase.DeclaringType.IsGenericType)
			{
				throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Argument_MethodDeclaringTypeGeneric"), methodBase, methodBase.DeclaringType.GetGenericTypeDefinition()));
			}
			return methodBase;
		}

		[ComVisible(false)]
		public static MethodBase GetMethodFromHandle(RuntimeMethodHandle handle, RuntimeTypeHandle declaringType)
		{
			if (handle.IsNullHandle())
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_InvalidHandle"));
			}
			return RuntimeType.GetMethodBase(declaringType, handle);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public static MethodBase GetCurrentMethod()
		{
			StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
			return RuntimeMethodInfo.InternalGetCurrentMethod(ref stackMark);
		}

		internal virtual RuntimeMethodHandle GetMethodHandle()
		{
			return MethodHandle;
		}

		internal virtual Type GetReturnType()
		{
			throw new NotImplementedException();
		}

		internal virtual ParameterInfo[] GetParametersNoCopy()
		{
			return GetParameters();
		}

		public abstract ParameterInfo[] GetParameters();

		public abstract MethodImplAttributes GetMethodImplementationFlags();

		public abstract object Invoke(object obj, BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture);

		[ComVisible(true)]
		public virtual Type[] GetGenericArguments()
		{
			throw new NotSupportedException(Environment.GetResourceString("NotSupported_SubclassOverride"));
		}

		Type _MethodBase.GetType()
		{
			return GetType();
		}

		[DebuggerStepThrough]
		[DebuggerHidden]
		public object Invoke(object obj, object[] parameters)
		{
			return Invoke(obj, BindingFlags.Default, null, parameters, null);
		}

		[ReflectionPermission(SecurityAction.Demand, Flags = ReflectionPermissionFlag.MemberAccess)]
		public virtual MethodBody GetMethodBody()
		{
			throw new InvalidOperationException();
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern uint GetSpecialSecurityFlags(RuntimeMethodHandle method);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern void PerformSecurityCheck(object obj, RuntimeMethodHandle method, IntPtr parent, uint invocationFlags);

		internal virtual Type[] GetParameterTypes()
		{
			ParameterInfo[] parametersNoCopy = GetParametersNoCopy();
			Type[] array = null;
			array = new Type[parametersNoCopy.Length];
			for (int i = 0; i < parametersNoCopy.Length; i++)
			{
				array[i] = parametersNoCopy[i].ParameterType;
			}
			return array;
		}

		internal virtual uint GetOneTimeFlags()
		{
			RuntimeMethodHandle methodHandle = MethodHandle;
			uint num = 0u;
			Type declaringType = DeclaringType;
			if (ContainsGenericParameters || (declaringType != null && declaringType.ContainsGenericParameters) || (CallingConvention & CallingConventions.VarArgs) == CallingConventions.VarArgs || (Attributes & MethodAttributes.RequireSecObject) == MethodAttributes.RequireSecObject)
			{
				num |= 2u;
			}
			else
			{
				AssemblyBuilderData assemblyData = Module.Assembly.m_assemblyData;
				if (assemblyData != null && (assemblyData.m_access & AssemblyBuilderAccess.Run) == 0)
				{
					num |= 2u;
				}
			}
			if (num == 0)
			{
				num |= GetSpecialSecurityFlags(methodHandle);
				if ((num & 4) == 0)
				{
					if ((Attributes & MethodAttributes.MemberAccessMask) != MethodAttributes.Public || (declaringType != null && !declaringType.IsVisible))
					{
						num |= 4u;
					}
					else if (IsGenericMethod)
					{
						Type[] genericArguments = GetGenericArguments();
						for (int i = 0; i < genericArguments.Length; i++)
						{
							if (!genericArguments[i].IsVisible)
							{
								num |= 4u;
								break;
							}
						}
					}
				}
			}
			num |= GetOneTimeSpecificFlags();
			return num | 1u;
		}

		internal virtual uint GetOneTimeSpecificFlags()
		{
			return 0u;
		}

		internal object[] CheckArguments(object[] parameters, Binder binder, BindingFlags invokeAttr, CultureInfo culture, Signature sig)
		{
			int num = ((parameters != null) ? parameters.Length : 0);
			object[] array = new object[num];
			ParameterInfo[] array2 = null;
			for (int i = 0; i < num; i++)
			{
				object obj = parameters[i];
				RuntimeTypeHandle runtimeTypeHandle = sig.Arguments[i];
				if (obj == Type.Missing)
				{
					if (array2 == null)
					{
						array2 = GetParametersNoCopy();
					}
					if (array2[i].DefaultValue == DBNull.Value)
					{
						throw new ArgumentException(Environment.GetResourceString("Arg_VarMissNull"), "parameters");
					}
					obj = array2[i].DefaultValue;
				}
				if (runtimeTypeHandle.IsInstanceOfType(obj))
				{
					array[i] = obj;
				}
				else
				{
					array[i] = runtimeTypeHandle.GetRuntimeType().CheckValue(obj, binder, culture, invokeAttr);
				}
			}
			return array;
		}

		void _MethodBase.GetTypeInfoCount(out uint pcTInfo)
		{
			throw new NotImplementedException();
		}

		void _MethodBase.GetTypeInfo(uint iTInfo, uint lcid, IntPtr ppTInfo)
		{
			throw new NotImplementedException();
		}

		void _MethodBase.GetIDsOfNames([In] ref Guid riid, IntPtr rgszNames, uint cNames, uint lcid, IntPtr rgDispId)
		{
			throw new NotImplementedException();
		}

		void _MethodBase.Invoke(uint dispIdMember, [In] ref Guid riid, uint lcid, short wFlags, IntPtr pDispParams, IntPtr pVarResult, IntPtr pExcepInfo, IntPtr puArgErr)
		{
			throw new NotImplementedException();
		}
	}
}
