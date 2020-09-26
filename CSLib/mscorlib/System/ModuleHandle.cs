using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Threading;

namespace System
{
	[ComVisible(true)]
	public struct ModuleHandle
	{
		public static readonly ModuleHandle EmptyHandle = new ModuleHandle(null);

		private IntPtr m_ptr;

		internal unsafe void* Value => m_ptr.ToPointer();

		public int MDStreamVersion => _GetMDStreamVersion();

		internal unsafe ModuleHandle(void* pModule)
		{
			m_ptr = new IntPtr(pModule);
		}

		internal unsafe bool IsNullHandle()
		{
			return m_ptr.ToPointer() == null;
		}

		public override int GetHashCode()
		{
			return ValueType.GetHashCodeOfPtr(m_ptr);
		}

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		public override bool Equals(object obj)
		{
			if (!(obj is ModuleHandle))
			{
				return false;
			}
			return ((ModuleHandle)obj).m_ptr == m_ptr;
		}

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		public bool Equals(ModuleHandle handle)
		{
			return handle.m_ptr == m_ptr;
		}

		public static bool operator ==(ModuleHandle left, ModuleHandle right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(ModuleHandle left, ModuleHandle right)
		{
			return !left.Equals(right);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern RuntimeTypeHandle GetCallerType(ref StackCrawlMark stackMark);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal unsafe static extern void* GetDynamicMethod(void* module, string name, byte[] sig, Resolver resolver);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal extern int GetToken();

		internal static RuntimeTypeHandle[] CopyRuntimeTypeHandles(RuntimeTypeHandle[] inHandles)
		{
			if (inHandles == null || inHandles.Length == 0)
			{
				return inHandles;
			}
			RuntimeTypeHandle[] array = new RuntimeTypeHandle[inHandles.Length];
			for (int i = 0; i < inHandles.Length; i++)
			{
				ref RuntimeTypeHandle reference = ref array[i];
				reference = inHandles[i];
			}
			return array;
		}

		private void ValidateModulePointer()
		{
			if (IsNullHandle())
			{
				throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_NullModuleHandle"));
			}
		}

		public RuntimeTypeHandle GetRuntimeTypeHandleFromMetadataToken(int typeToken)
		{
			return ResolveTypeHandle(typeToken);
		}

		public RuntimeTypeHandle ResolveTypeHandle(int typeToken)
		{
			return ResolveTypeHandle(typeToken, null, null);
		}

		public unsafe RuntimeTypeHandle ResolveTypeHandle(int typeToken, RuntimeTypeHandle[] typeInstantiationContext, RuntimeTypeHandle[] methodInstantiationContext)
		{
			ValidateModulePointer();
			if (!GetMetadataImport().IsValidToken(typeToken))
			{
				throw new ArgumentOutOfRangeException("metadataToken", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Argument_InvalidToken", typeToken, this)));
			}
			typeInstantiationContext = CopyRuntimeTypeHandles(typeInstantiationContext);
			methodInstantiationContext = CopyRuntimeTypeHandles(methodInstantiationContext);
			if (typeInstantiationContext == null || typeInstantiationContext.Length == 0)
			{
				if (methodInstantiationContext == null || methodInstantiationContext.Length == 0)
				{
					return ResolveType(typeToken, null, 0, null, 0);
				}
				int methodInstCount = methodInstantiationContext.Length;
				fixed (RuntimeTypeHandle* methodInstArgs = methodInstantiationContext)
				{
					return ResolveType(typeToken, null, 0, methodInstArgs, methodInstCount);
				}
			}
			if (methodInstantiationContext == null || methodInstantiationContext.Length == 0)
			{
				int typeInstCount = typeInstantiationContext.Length;
				fixed (RuntimeTypeHandle* typeInstArgs = typeInstantiationContext)
				{
					return ResolveType(typeToken, typeInstArgs, typeInstCount, null, 0);
				}
			}
			int typeInstCount2 = typeInstantiationContext.Length;
			int methodInstCount2 = methodInstantiationContext.Length;
			fixed (RuntimeTypeHandle* typeInstArgs2 = typeInstantiationContext)
			{
				fixed (RuntimeTypeHandle* methodInstArgs2 = methodInstantiationContext)
				{
					return ResolveType(typeToken, typeInstArgs2, typeInstCount2, methodInstArgs2, methodInstCount2);
				}
			}
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private unsafe extern RuntimeTypeHandle ResolveType(int typeToken, RuntimeTypeHandle* typeInstArgs, int typeInstCount, RuntimeTypeHandle* methodInstArgs, int methodInstCount);

		public RuntimeMethodHandle GetRuntimeMethodHandleFromMetadataToken(int methodToken)
		{
			return ResolveMethodHandle(methodToken);
		}

		public RuntimeMethodHandle ResolveMethodHandle(int methodToken)
		{
			return ResolveMethodHandle(methodToken, null, null);
		}

		public unsafe RuntimeMethodHandle ResolveMethodHandle(int methodToken, RuntimeTypeHandle[] typeInstantiationContext, RuntimeTypeHandle[] methodInstantiationContext)
		{
			ValidateModulePointer();
			if (!GetMetadataImport().IsValidToken(methodToken))
			{
				throw new ArgumentOutOfRangeException("metadataToken", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Argument_InvalidToken", methodToken, this)));
			}
			typeInstantiationContext = CopyRuntimeTypeHandles(typeInstantiationContext);
			methodInstantiationContext = CopyRuntimeTypeHandles(methodInstantiationContext);
			if (typeInstantiationContext == null || typeInstantiationContext.Length == 0)
			{
				if (methodInstantiationContext == null || methodInstantiationContext.Length == 0)
				{
					return ResolveMethod(methodToken, null, 0, null, 0);
				}
				int methodInstCount = methodInstantiationContext.Length;
				fixed (RuntimeTypeHandle* methodInstArgs = methodInstantiationContext)
				{
					return ResolveMethod(methodToken, null, 0, methodInstArgs, methodInstCount);
				}
			}
			if (methodInstantiationContext == null || methodInstantiationContext.Length == 0)
			{
				int typeInstCount = typeInstantiationContext.Length;
				fixed (RuntimeTypeHandle* typeInstArgs = typeInstantiationContext)
				{
					return ResolveMethod(methodToken, typeInstArgs, typeInstCount, null, 0);
				}
			}
			int typeInstCount2 = typeInstantiationContext.Length;
			int methodInstCount2 = methodInstantiationContext.Length;
			fixed (RuntimeTypeHandle* typeInstArgs2 = typeInstantiationContext)
			{
				fixed (RuntimeTypeHandle* methodInstArgs2 = methodInstantiationContext)
				{
					return ResolveMethod(methodToken, typeInstArgs2, typeInstCount2, methodInstArgs2, methodInstCount2);
				}
			}
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private unsafe extern RuntimeMethodHandle ResolveMethod(int methodToken, RuntimeTypeHandle* typeInstArgs, int typeInstCount, RuntimeTypeHandle* methodInstArgs, int methodInstCount);

		public RuntimeFieldHandle GetRuntimeFieldHandleFromMetadataToken(int fieldToken)
		{
			return ResolveFieldHandle(fieldToken);
		}

		public RuntimeFieldHandle ResolveFieldHandle(int fieldToken)
		{
			return ResolveFieldHandle(fieldToken, null, null);
		}

		public unsafe RuntimeFieldHandle ResolveFieldHandle(int fieldToken, RuntimeTypeHandle[] typeInstantiationContext, RuntimeTypeHandle[] methodInstantiationContext)
		{
			ValidateModulePointer();
			if (!GetMetadataImport().IsValidToken(fieldToken))
			{
				throw new ArgumentOutOfRangeException("metadataToken", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Argument_InvalidToken", fieldToken, this)));
			}
			typeInstantiationContext = CopyRuntimeTypeHandles(typeInstantiationContext);
			methodInstantiationContext = CopyRuntimeTypeHandles(methodInstantiationContext);
			if (typeInstantiationContext == null || typeInstantiationContext.Length == 0)
			{
				if (methodInstantiationContext == null || methodInstantiationContext.Length == 0)
				{
					return ResolveField(fieldToken, null, 0, null, 0);
				}
				int methodInstCount = methodInstantiationContext.Length;
				fixed (RuntimeTypeHandle* methodInstArgs = methodInstantiationContext)
				{
					return ResolveField(fieldToken, null, 0, methodInstArgs, methodInstCount);
				}
			}
			if (methodInstantiationContext == null || methodInstantiationContext.Length == 0)
			{
				int typeInstCount = typeInstantiationContext.Length;
				fixed (RuntimeTypeHandle* typeInstArgs = typeInstantiationContext)
				{
					return ResolveField(fieldToken, typeInstArgs, typeInstCount, null, 0);
				}
			}
			int typeInstCount2 = typeInstantiationContext.Length;
			int methodInstCount2 = methodInstantiationContext.Length;
			fixed (RuntimeTypeHandle* typeInstArgs2 = typeInstantiationContext)
			{
				fixed (RuntimeTypeHandle* methodInstArgs2 = methodInstantiationContext)
				{
					return ResolveField(fieldToken, typeInstArgs2, typeInstCount2, methodInstArgs2, methodInstCount2);
				}
			}
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private unsafe extern RuntimeFieldHandle ResolveField(int fieldToken, RuntimeTypeHandle* typeInstArgs, int typeInstCount, RuntimeTypeHandle* methodInstArgs, int methodInstCount);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal extern Module GetModule();

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal unsafe extern void* _GetModuleTypeHandle();

		internal unsafe RuntimeTypeHandle GetModuleTypeHandle()
		{
			return new RuntimeTypeHandle(_GetModuleTypeHandle());
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal extern void _GetPEKind(out int peKind, out int machine);

		internal void GetPEKind(out PortableExecutableKinds peKind, out ImageFileMachine machine)
		{
			_GetPEKind(out var peKind2, out var machine2);
			peKind = (PortableExecutableKinds)peKind2;
			machine = (ImageFileMachine)machine2;
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal extern int _GetMDStreamVersion();

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal unsafe extern void* _GetMetadataImport();

		internal unsafe MetadataImport GetMetadataImport()
		{
			return new MetadataImport((IntPtr)_GetMetadataImport());
		}
	}
}
