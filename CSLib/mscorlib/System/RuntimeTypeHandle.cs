using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Threading;

namespace System
{
	[Serializable]
	[ComVisible(true)]
	public struct RuntimeTypeHandle : ISerializable
	{
		internal struct IntroducedMethodEnumerator
		{
			private RuntimeMethodHandle _method;

			private bool _firstCall;

			public RuntimeMethodHandle Current => _method;

			internal IntroducedMethodEnumerator(RuntimeTypeHandle type)
			{
				_method = GetFirstIntroducedMethod(type);
				_firstCall = true;
			}

			public bool MoveNext()
			{
				if (_firstCall)
				{
					_firstCall = false;
				}
				else if (!_method.IsNullHandle())
				{
					_method = GetNextIntroducedMethod(_method);
				}
				return !_method.IsNullHandle();
			}

			public IntroducedMethodEnumerator GetEnumerator()
			{
				return this;
			}
		}

		private const int MAX_CLASS_NAME = 1024;

		internal static readonly RuntimeTypeHandle EmptyHandle = new RuntimeTypeHandle(null);

		private IntPtr m_ptr;

		public IntPtr Value => m_ptr;

		internal IntroducedMethodEnumerator IntroducedMethods => new IntroducedMethodEnumerator(this);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal extern bool IsInstanceOfType(object o);

		internal unsafe static IntPtr GetTypeHelper(IntPtr th, IntPtr pGenericArgs, int cGenericArgs, IntPtr pModifiers, int cModifiers)
		{
			Type type = new RuntimeTypeHandle(th.ToPointer()).GetRuntimeType();
			if (type == null)
			{
				return th;
			}
			if (cGenericArgs > 0)
			{
				Type[] array = new Type[cGenericArgs];
				void** value = (void**)pGenericArgs.ToPointer();
				for (int i = 0; i < array.Length; i++)
				{
					RuntimeTypeHandle handle = new RuntimeTypeHandle((void*)Marshal.ReadIntPtr((IntPtr)value, i * sizeof(void*)));
					array[i] = Type.GetTypeFromHandle(handle);
					if (array[i] == null)
					{
						return (IntPtr)0;
					}
				}
				type = type.MakeGenericType(array);
			}
			if (cModifiers > 0)
			{
				int* value2 = (int*)pModifiers.ToPointer();
				for (int j = 0; j < cModifiers; j++)
				{
					type = (((byte)Marshal.ReadInt32((IntPtr)value2, j * 4) != 15) ? (((byte)Marshal.ReadInt32((IntPtr)value2, j * 4) != 16) ? (((byte)Marshal.ReadInt32((IntPtr)value2, j * 4) != 29) ? type.MakeArrayType(Marshal.ReadInt32((IntPtr)value2, ++j * 4)) : type.MakeArrayType()) : type.MakeByRefType()) : type.MakePointerType());
				}
			}
			return type.GetTypeHandleInternal().Value;
		}

		public static bool operator ==(RuntimeTypeHandle left, object right)
		{
			return left.Equals(right);
		}

		public static bool operator ==(object left, RuntimeTypeHandle right)
		{
			return right.Equals(left);
		}

		public static bool operator !=(RuntimeTypeHandle left, object right)
		{
			return !left.Equals(right);
		}

		public static bool operator !=(object left, RuntimeTypeHandle right)
		{
			return !right.Equals(left);
		}

		public override int GetHashCode()
		{
			return ValueType.GetHashCodeOfPtr(m_ptr);
		}

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		public override bool Equals(object obj)
		{
			if (!(obj is RuntimeTypeHandle))
			{
				return false;
			}
			return ((RuntimeTypeHandle)obj).m_ptr == m_ptr;
		}

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		public bool Equals(RuntimeTypeHandle handle)
		{
			return handle.m_ptr == m_ptr;
		}

		internal unsafe RuntimeTypeHandle(void* rth)
		{
			m_ptr = new IntPtr(rth);
		}

		internal unsafe bool IsNullHandle()
		{
			return m_ptr.ToPointer() == null;
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern object CreateInstance(RuntimeType type, bool publicOnly, bool noCheck, ref bool canBeCached, ref RuntimeMethodHandle ctor, ref bool bNeedSecurityCheck);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal extern object CreateCaInstance(RuntimeMethodHandle ctor);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal extern object Allocate();

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal extern object CreateInstanceForAnotherGenericParameter(Type genericParameter);

		internal RuntimeType GetRuntimeType()
		{
			if (!IsNullHandle())
			{
				return (RuntimeType)Type.GetTypeFromHandle(this);
			}
			return null;
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal extern CorElementType GetCorElementType();

		[MethodImpl(MethodImplOptions.InternalCall)]
		private unsafe extern void* _GetAssemblyHandle();

		internal unsafe AssemblyHandle GetAssemblyHandle()
		{
			return new AssemblyHandle(_GetAssemblyHandle());
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		private unsafe extern void* _GetModuleHandle();

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		[CLSCompliant(false)]
		public unsafe ModuleHandle GetModuleHandle()
		{
			return new ModuleHandle(_GetModuleHandle());
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private unsafe extern void* _GetBaseTypeHandle();

		internal unsafe RuntimeTypeHandle GetBaseTypeHandle()
		{
			return new RuntimeTypeHandle(_GetBaseTypeHandle());
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal extern TypeAttributes GetAttributes();

		[MethodImpl(MethodImplOptions.InternalCall)]
		private unsafe extern void* _GetElementType();

		internal unsafe RuntimeTypeHandle GetElementType()
		{
			return new RuntimeTypeHandle(_GetElementType());
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal extern RuntimeTypeHandle GetCanonicalHandle();

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal extern int GetArrayRank();

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal extern int GetToken();

		[MethodImpl(MethodImplOptions.InternalCall)]
		private unsafe extern void* _GetMethodAt(int slot);

		internal unsafe RuntimeMethodHandle GetMethodAt(int slot)
		{
			return new RuntimeMethodHandle(_GetMethodAt(slot));
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern RuntimeMethodHandle GetFirstIntroducedMethod(RuntimeTypeHandle type);

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern RuntimeMethodHandle GetNextIntroducedMethod(RuntimeMethodHandle method);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal unsafe extern bool GetFields(int** result, int* count);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal extern RuntimeTypeHandle[] GetInterfaces();

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal extern RuntimeTypeHandle[] GetConstraints();

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal extern IntPtr GetGCHandle(GCHandleType type);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal extern void FreeGCHandle(IntPtr handle);

		[MethodImpl(MethodImplOptions.InternalCall)]
		private unsafe extern void* _GetMethodFromToken(int tkMethodDef);

		internal unsafe RuntimeMethodHandle GetMethodFromToken(int tkMethodDef)
		{
			return new RuntimeMethodHandle(_GetMethodFromToken(tkMethodDef));
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal extern int GetNumVirtuals();

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal extern int GetInterfaceMethodSlots();

		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern int GetFirstSlotForInterface(IntPtr interfaceHandle);

		internal int GetFirstSlotForInterface(RuntimeTypeHandle interfaceHandle)
		{
			return GetFirstSlotForInterface(interfaceHandle.Value);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern int GetInterfaceMethodImplementationSlot(IntPtr interfaceHandle, IntPtr interfaceMethodHandle);

		internal int GetInterfaceMethodImplementationSlot(RuntimeTypeHandle interfaceHandle, RuntimeMethodHandle interfaceMethodHandle)
		{
			return GetInterfaceMethodImplementationSlot(interfaceHandle.Value, interfaceMethodHandle.Value);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal extern bool IsComObject(bool isGenericCOM);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal extern bool IsContextful();

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal extern bool IsInterface();

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal extern bool IsVisible();

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal extern bool _IsVisibleFromModule(IntPtr module);

		internal unsafe bool IsVisibleFromModule(ModuleHandle module)
		{
			return _IsVisibleFromModule((IntPtr)module.Value);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal extern bool HasProxyAttribute();

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal extern string ConstructName(bool nameSpace, bool fullInst, bool assembly);

		[MethodImpl(MethodImplOptions.InternalCall)]
		private unsafe extern void* _GetUtf8Name();

		internal unsafe Utf8String GetUtf8Name()
		{
			return new Utf8String(_GetUtf8Name());
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern bool CanCastTo(IntPtr target);

		internal bool CanCastTo(RuntimeTypeHandle target)
		{
			return CanCastTo(target.Value);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal extern RuntimeTypeHandle GetDeclaringType();

		[MethodImpl(MethodImplOptions.InternalCall)]
		private unsafe extern void* _GetDeclaringMethod();

		internal unsafe RuntimeMethodHandle GetDeclaringMethod()
		{
			return new RuntimeMethodHandle(_GetDeclaringMethod());
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private unsafe extern void* _GetDefaultConstructor();

		internal unsafe RuntimeMethodHandle GetDefaultConstructor()
		{
			return new RuntimeMethodHandle(_GetDefaultConstructor());
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal extern bool SupportsInterface(object target);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal unsafe static extern void* _GetTypeByName(string name, bool throwOnError, bool ignoreCase, bool reflectionOnly, ref StackCrawlMark stackMark, bool loadTypeFromPartialName);

		internal unsafe static RuntimeTypeHandle GetTypeByName(string name, bool throwOnError, bool ignoreCase, bool reflectionOnly, ref StackCrawlMark stackMark)
		{
			if (name == null || name.Length == 0)
			{
				if (throwOnError)
				{
					throw new TypeLoadException(Environment.GetResourceString("Arg_TypeLoadNullStr"));
				}
				return default(RuntimeTypeHandle);
			}
			return new RuntimeTypeHandle(_GetTypeByName(name, throwOnError, ignoreCase, reflectionOnly, ref stackMark, loadTypeFromPartialName: false));
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal unsafe static extern void* _GetTypeByNameUsingCARules(string name, IntPtr scope);

		internal unsafe static Type GetTypeByNameUsingCARules(string name, Module scope)
		{
			if (name == null || name.Length == 0)
			{
				throw new ArgumentException();
			}
			return new RuntimeTypeHandle(_GetTypeByNameUsingCARules(name, (IntPtr)scope.GetModuleHandle().Value)).GetRuntimeType();
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal extern RuntimeTypeHandle[] GetInstantiation();

		[MethodImpl(MethodImplOptions.InternalCall)]
		private unsafe extern void* _Instantiate(RuntimeTypeHandle[] inst);

		internal unsafe RuntimeTypeHandle Instantiate(RuntimeTypeHandle[] inst)
		{
			return new RuntimeTypeHandle(_Instantiate(inst));
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private unsafe extern void* _MakeArray(int rank);

		internal unsafe RuntimeTypeHandle MakeArray(int rank)
		{
			return new RuntimeTypeHandle(_MakeArray(rank));
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private unsafe extern void* _MakeSZArray();

		internal unsafe RuntimeTypeHandle MakeSZArray()
		{
			return new RuntimeTypeHandle(_MakeSZArray());
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private unsafe extern void* _MakeByRef();

		internal unsafe RuntimeTypeHandle MakeByRef()
		{
			return new RuntimeTypeHandle(_MakeByRef());
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private unsafe extern void* _MakePointer();

		internal unsafe RuntimeTypeHandle MakePointer()
		{
			return new RuntimeTypeHandle(_MakePointer());
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal extern bool HasInstantiation();

		[MethodImpl(MethodImplOptions.InternalCall)]
		private unsafe extern void* _GetGenericTypeDefinition();

		internal unsafe RuntimeTypeHandle GetGenericTypeDefinition()
		{
			return new RuntimeTypeHandle(_GetGenericTypeDefinition());
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal extern bool IsGenericTypeDefinition();

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal extern bool IsGenericVariable();

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal extern int GetGenericVariableIndex();

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal extern bool ContainsGenericVariables();

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal extern bool SatisfiesConstraints(RuntimeTypeHandle[] typeContext, RuntimeTypeHandle[] methodContext, RuntimeTypeHandle toType);

		private unsafe RuntimeTypeHandle(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			Type type = (RuntimeType)info.GetValue("TypeObj", typeof(RuntimeType));
			m_ptr = type.TypeHandle.Value;
			if (m_ptr.ToPointer() == null)
			{
				throw new SerializationException(Environment.GetResourceString("Serialization_InsufficientState"));
			}
		}

		public unsafe void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			if (m_ptr.ToPointer() == null)
			{
				throw new SerializationException(Environment.GetResourceString("Serialization_InvalidFieldState"));
			}
			RuntimeType value = (RuntimeType)Type.GetTypeFromHandle(this);
			info.AddValue("TypeObj", value, typeof(RuntimeType));
		}
	}
}
