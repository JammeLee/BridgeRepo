using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace System
{
	[Serializable]
	[ClassInterface(ClassInterfaceType.AutoDual)]
	[ComVisible(true)]
	public abstract class Delegate : ICloneable, ISerializable
	{
		internal object _target;

		internal MethodBase _methodBase;

		internal IntPtr _methodPtr;

		internal IntPtr _methodPtrAux;

		public MethodInfo Method => GetMethodImpl();

		public object Target => GetTarget();

		protected Delegate(object target, string method)
		{
			if (target == null)
			{
				throw new ArgumentNullException("target");
			}
			if (method == null)
			{
				throw new ArgumentNullException("method");
			}
			if (!BindToMethodName(target, Type.GetTypeHandle(target), method, (DelegateBindingFlags)10))
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_DlgtTargMeth"));
			}
		}

		protected Delegate(Type target, string method)
		{
			if (target == null)
			{
				throw new ArgumentNullException("target");
			}
			if (!(target is RuntimeType))
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_MustBeRuntimeType"), "target");
			}
			if (target.IsGenericType && target.ContainsGenericParameters)
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_UnboundGenParam"), "target");
			}
			if (method == null)
			{
				throw new ArgumentNullException("method");
			}
			BindToMethodName(null, target.TypeHandle, method, (DelegateBindingFlags)37);
		}

		private Delegate()
		{
		}

		public object DynamicInvoke(params object[] args)
		{
			return DynamicInvokeImpl(args);
		}

		protected virtual object DynamicInvokeImpl(object[] args)
		{
			RuntimeMethodHandle methodHandle = new RuntimeMethodHandle(GetInvokeMethod());
			RuntimeTypeHandle typeHandle = Type.GetTypeHandle(this);
			RuntimeMethodInfo runtimeMethodInfo = (RuntimeMethodInfo)RuntimeType.GetMethodBase(typeHandle, methodHandle);
			return runtimeMethodInfo.Invoke(this, BindingFlags.Default, null, args, null, skipVisibilityChecks: true);
		}

		public override bool Equals(object obj)
		{
			if (obj == null || !InternalEqualTypes(this, obj))
			{
				return false;
			}
			Delegate @delegate = (Delegate)obj;
			if (_target == @delegate._target && _methodPtr == @delegate._methodPtr && _methodPtrAux == @delegate._methodPtrAux)
			{
				return true;
			}
			if (_methodPtrAux.IsNull())
			{
				if (!@delegate._methodPtrAux.IsNull())
				{
					return false;
				}
				if (_target != @delegate._target)
				{
					return false;
				}
			}
			else
			{
				if (@delegate._methodPtrAux.IsNull())
				{
					return false;
				}
				if (_methodPtrAux == @delegate._methodPtrAux)
				{
					return true;
				}
			}
			if (_methodBase == null || @delegate._methodBase == null)
			{
				return FindMethodHandle().Equals(@delegate.FindMethodHandle());
			}
			return _methodBase.Equals(@delegate._methodBase);
		}

		public override int GetHashCode()
		{
			return GetType().GetHashCode();
		}

		public static Delegate Combine(Delegate a, Delegate b)
		{
			if ((object)a == null)
			{
				return b;
			}
			return a.CombineImpl(b);
		}

		[ComVisible(true)]
		public static Delegate Combine(params Delegate[] delegates)
		{
			if (delegates == null || delegates.Length == 0)
			{
				return null;
			}
			Delegate @delegate = delegates[0];
			for (int i = 1; i < delegates.Length; i++)
			{
				@delegate = Combine(@delegate, delegates[i]);
			}
			return @delegate;
		}

		public virtual Delegate[] GetInvocationList()
		{
			return new Delegate[1]
			{
				this
			};
		}

		protected virtual MethodInfo GetMethodImpl()
		{
			if (_methodBase == null)
			{
				RuntimeMethodHandle methodHandle = FindMethodHandle();
				RuntimeTypeHandle reflectedTypeHandle = methodHandle.GetDeclaringType();
				if ((reflectedTypeHandle.IsGenericTypeDefinition() || reflectedTypeHandle.HasInstantiation()) && (methodHandle.GetAttributes() & MethodAttributes.Static) == 0)
				{
					if (_methodPtrAux == (IntPtr)0)
					{
						Type type = _target.GetType();
						Type genericTypeDefinition = reflectedTypeHandle.GetRuntimeType().GetGenericTypeDefinition();
						while (!type.IsGenericType || type.GetGenericTypeDefinition() != genericTypeDefinition)
						{
							type = type.BaseType;
						}
						reflectedTypeHandle = type.TypeHandle;
					}
					else
					{
						MethodInfo method = GetType().GetMethod("Invoke");
						reflectedTypeHandle = method.GetParameters()[0].ParameterType.TypeHandle;
					}
				}
				_methodBase = (MethodInfo)RuntimeType.GetMethodBase(reflectedTypeHandle, methodHandle);
			}
			return (MethodInfo)_methodBase;
		}

		public static Delegate Remove(Delegate source, Delegate value)
		{
			if ((object)source == null)
			{
				return null;
			}
			if ((object)value == null)
			{
				return source;
			}
			if (!InternalEqualTypes(source, value))
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_DlgtTypeMis"));
			}
			return source.RemoveImpl(value);
		}

		public static Delegate RemoveAll(Delegate source, Delegate value)
		{
			Delegate @delegate = null;
			do
			{
				@delegate = source;
				source = Remove(source, value);
			}
			while (@delegate != source);
			return @delegate;
		}

		protected virtual Delegate CombineImpl(Delegate d)
		{
			throw new MulticastNotSupportedException(Environment.GetResourceString("Multicast_Combine"));
		}

		protected virtual Delegate RemoveImpl(Delegate d)
		{
			if (!d.Equals(this))
			{
				return this;
			}
			return null;
		}

		public virtual object Clone()
		{
			return MemberwiseClone();
		}

		public static Delegate CreateDelegate(Type type, object target, string method)
		{
			return CreateDelegate(type, target, method, ignoreCase: false, throwOnBindFailure: true);
		}

		public static Delegate CreateDelegate(Type type, object target, string method, bool ignoreCase)
		{
			return CreateDelegate(type, target, method, ignoreCase, throwOnBindFailure: true);
		}

		public static Delegate CreateDelegate(Type type, object target, string method, bool ignoreCase, bool throwOnBindFailure)
		{
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}
			if (!(type is RuntimeType))
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_MustBeRuntimeType"), "type");
			}
			if (target == null)
			{
				throw new ArgumentNullException("target");
			}
			if (method == null)
			{
				throw new ArgumentNullException("method");
			}
			Type baseType = type.BaseType;
			if (baseType == null || baseType != typeof(MulticastDelegate))
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_MustBeDelegate"), "type");
			}
			Delegate @delegate = InternalAlloc(type.TypeHandle);
			if (!@delegate.BindToMethodName(target, Type.GetTypeHandle(target), method, (DelegateBindingFlags)26 | (ignoreCase ? DelegateBindingFlags.CaselessMatching : ((DelegateBindingFlags)0))))
			{
				if (throwOnBindFailure)
				{
					throw new ArgumentException(Environment.GetResourceString("Arg_DlgtTargMeth"));
				}
				@delegate = null;
			}
			return @delegate;
		}

		public static Delegate CreateDelegate(Type type, Type target, string method)
		{
			return CreateDelegate(type, target, method, ignoreCase: false, throwOnBindFailure: true);
		}

		public static Delegate CreateDelegate(Type type, Type target, string method, bool ignoreCase)
		{
			return CreateDelegate(type, target, method, ignoreCase, throwOnBindFailure: true);
		}

		public static Delegate CreateDelegate(Type type, Type target, string method, bool ignoreCase, bool throwOnBindFailure)
		{
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}
			if (!(type is RuntimeType))
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_MustBeRuntimeType"), "type");
			}
			if (target == null)
			{
				throw new ArgumentNullException("target");
			}
			if (!(target is RuntimeType))
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_MustBeRuntimeType"), "target");
			}
			if (target.IsGenericType && target.ContainsGenericParameters)
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_UnboundGenParam"), "target");
			}
			if (method == null)
			{
				throw new ArgumentNullException("method");
			}
			Type baseType = type.BaseType;
			if (baseType == null || baseType != typeof(MulticastDelegate))
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_MustBeDelegate"), "type");
			}
			Delegate @delegate = InternalAlloc(type.TypeHandle);
			if (!@delegate.BindToMethodName(null, target.TypeHandle, method, (DelegateBindingFlags)5 | (ignoreCase ? DelegateBindingFlags.CaselessMatching : ((DelegateBindingFlags)0))))
			{
				if (throwOnBindFailure)
				{
					throw new ArgumentException(Environment.GetResourceString("Arg_DlgtTargMeth"));
				}
				@delegate = null;
			}
			return @delegate;
		}

		public static Delegate CreateDelegate(Type type, MethodInfo method)
		{
			return CreateDelegate(type, method, throwOnBindFailure: true);
		}

		public static Delegate CreateDelegate(Type type, MethodInfo method, bool throwOnBindFailure)
		{
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}
			if (!(type is RuntimeType))
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_MustBeRuntimeType"), "type");
			}
			if (method == null)
			{
				throw new ArgumentNullException("method");
			}
			if (!(method is RuntimeMethodInfo))
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_MustBeRuntimeMethodInfo"), "method");
			}
			Type baseType = type.BaseType;
			if (baseType == null || baseType != typeof(MulticastDelegate))
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_MustBeDelegate"), "type");
			}
			Delegate @delegate = InternalAlloc(type.TypeHandle);
			if (!@delegate.BindToMethodInfo(null, method.MethodHandle, method.DeclaringType.TypeHandle, (DelegateBindingFlags)132))
			{
				if (throwOnBindFailure)
				{
					throw new ArgumentException(Environment.GetResourceString("Arg_DlgtTargMeth"));
				}
				@delegate = null;
			}
			return @delegate;
		}

		public static Delegate CreateDelegate(Type type, object firstArgument, MethodInfo method)
		{
			return CreateDelegate(type, firstArgument, method, throwOnBindFailure: true);
		}

		public static Delegate CreateDelegate(Type type, object firstArgument, MethodInfo method, bool throwOnBindFailure)
		{
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}
			if (!(type is RuntimeType))
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_MustBeRuntimeType"), "type");
			}
			if (method == null)
			{
				throw new ArgumentNullException("method");
			}
			if (!(method is RuntimeMethodInfo))
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_MustBeRuntimeMethodInfo"), "method");
			}
			Type baseType = type.BaseType;
			if (baseType == null || baseType != typeof(MulticastDelegate))
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_MustBeDelegate"), "type");
			}
			Delegate @delegate = InternalAlloc(type.TypeHandle);
			if (!@delegate.BindToMethodInfo(firstArgument, method.MethodHandle, method.DeclaringType.TypeHandle, DelegateBindingFlags.RelaxedSignature))
			{
				if (throwOnBindFailure)
				{
					throw new ArgumentException(Environment.GetResourceString("Arg_DlgtTargMeth"));
				}
				@delegate = null;
			}
			return @delegate;
		}

		public static bool operator ==(Delegate d1, Delegate d2)
		{
			return d1?.Equals(d2) ?? ((object)d2 == null);
		}

		public static bool operator !=(Delegate d1, Delegate d2)
		{
			if ((object)d1 == null)
			{
				return (object)d2 != null;
			}
			return !d1.Equals(d2);
		}

		public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			throw new NotSupportedException();
		}

		internal static Delegate CreateDelegate(Type type, object target, RuntimeMethodHandle method)
		{
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}
			if (!(type is RuntimeType))
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_MustBeRuntimeType"), "type");
			}
			if (method.IsNullHandle())
			{
				throw new ArgumentNullException("method");
			}
			Type baseType = type.BaseType;
			if (baseType == null || baseType != typeof(MulticastDelegate))
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_MustBeDelegate"), "type");
			}
			Delegate @delegate = InternalAlloc(type.TypeHandle);
			if (!@delegate.BindToMethodInfo(target, method, method.GetDeclaringType(), DelegateBindingFlags.RelaxedSignature))
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_DlgtTargMeth"));
			}
			return @delegate;
		}

		internal static Delegate InternalCreateDelegate(Type type, object firstArgument, MethodInfo method)
		{
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}
			if (method == null)
			{
				throw new ArgumentNullException("method");
			}
			Type baseType = type.BaseType;
			if (baseType == null || baseType != typeof(MulticastDelegate))
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_MustBeDelegate"), "type");
			}
			Delegate @delegate = InternalAlloc(type.TypeHandle);
			if (!@delegate.BindToMethodInfo(firstArgument, method.MethodHandle, method.DeclaringType.TypeHandle, (DelegateBindingFlags)192))
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_DlgtTargMeth"));
			}
			return @delegate;
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern bool BindToMethodName(object target, RuntimeTypeHandle methodType, string method, DelegateBindingFlags flags);

		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern bool BindToMethodInfo(object target, RuntimeMethodHandle method, RuntimeTypeHandle methodType, DelegateBindingFlags flags);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern MulticastDelegate InternalAlloc(RuntimeTypeHandle type);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern MulticastDelegate InternalAllocLike(Delegate d);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern bool InternalEqualTypes(object a, object b);

		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern void DelegateConstruct(object target, IntPtr slot);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal extern IntPtr GetMulticastInvoke();

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal extern IntPtr GetInvokeMethod();

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal extern RuntimeMethodHandle FindMethodHandle();

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal extern IntPtr GetUnmanagedCallSite();

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal extern IntPtr AdjustTarget(object target, IntPtr methodPtr);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal extern IntPtr GetCallStub(IntPtr methodPtr);

		internal virtual object GetTarget()
		{
			if (!_methodPtrAux.IsNull())
			{
				return null;
			}
			return _target;
		}
	}
}
