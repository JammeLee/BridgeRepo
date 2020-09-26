using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Threading;

namespace System
{
	[Serializable]
	[ComVisible(true)]
	public struct RuntimeMethodHandle : ISerializable
	{
		private IntPtr m_ptr;

		internal static RuntimeMethodHandle EmptyHandle => new RuntimeMethodHandle(null);

		public IntPtr Value
		{
			[SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
			get
			{
				return m_ptr;
			}
		}

		internal unsafe RuntimeMethodHandle(void* pMethod)
		{
			m_ptr = new IntPtr(pMethod);
		}

		internal RuntimeMethodHandle(IntPtr pMethod)
		{
			m_ptr = pMethod;
		}

		private unsafe RuntimeMethodHandle(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			MethodInfo methodInfo = (RuntimeMethodInfo)info.GetValue("MethodObj", typeof(RuntimeMethodInfo));
			m_ptr = methodInfo.MethodHandle.Value;
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
			RuntimeMethodInfo value = (RuntimeMethodInfo)RuntimeType.GetMethodBase(this);
			info.AddValue("MethodObj", value, typeof(RuntimeMethodInfo));
		}

		public override int GetHashCode()
		{
			return ValueType.GetHashCodeOfPtr(m_ptr);
		}

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		public override bool Equals(object obj)
		{
			if (!(obj is RuntimeMethodHandle))
			{
				return false;
			}
			return ((RuntimeMethodHandle)obj).m_ptr == m_ptr;
		}

		public static bool operator ==(RuntimeMethodHandle left, RuntimeMethodHandle right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(RuntimeMethodHandle left, RuntimeMethodHandle right)
		{
			return !left.Equals(right);
		}

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		public bool Equals(RuntimeMethodHandle handle)
		{
			return handle.m_ptr == m_ptr;
		}

		internal unsafe bool IsNullHandle()
		{
			return m_ptr.ToPointer() == null;
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
		public extern IntPtr GetFunctionPointer();

		[MethodImpl(MethodImplOptions.InternalCall)]
		private unsafe extern void _CheckLinktimeDemands(void* module, int metadataToken);

		internal unsafe void CheckLinktimeDemands(Module module, int metadataToken)
		{
			_CheckLinktimeDemands(module.ModuleHandle.Value, metadataToken);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private unsafe extern bool _IsVisibleFromModule(void* source);

		internal unsafe bool IsVisibleFromModule(Module source)
		{
			return _IsVisibleFromModule(source.ModuleHandle.Value);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern bool _IsVisibleFromType(IntPtr source);

		internal bool IsVisibleFromType(RuntimeTypeHandle source)
		{
			return _IsVisibleFromType(source.Value);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private unsafe static extern void* _GetCurrentMethod(ref StackCrawlMark stackMark);

		internal unsafe static RuntimeMethodHandle GetCurrentMethod(ref StackCrawlMark stackMark)
		{
			return new RuntimeMethodHandle(_GetCurrentMethod(ref stackMark));
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal extern MethodAttributes GetAttributes();

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal extern MethodImplAttributes GetImplAttributes();

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal extern string ConstructInstantiation();

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal extern RuntimeTypeHandle GetDeclaringType();

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal extern int GetSlot();

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal extern int GetMethodDef();

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal extern string GetName();

		[MethodImpl(MethodImplOptions.InternalCall)]
		private unsafe extern void* _GetUtf8Name();

		internal unsafe Utf8String GetUtf8Name()
		{
			return new Utf8String(_GetUtf8Name());
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[DebuggerStepThrough]
		[DebuggerHidden]
		private extern object _InvokeMethodFast(object target, object[] arguments, ref SignatureStruct sig, MethodAttributes methodAttributes, RuntimeTypeHandle typeOwner);

		[DebuggerHidden]
		[DebuggerStepThrough]
		internal object InvokeMethodFast(object target, object[] arguments, Signature sig, MethodAttributes methodAttributes, RuntimeTypeHandle typeOwner)
		{
			SignatureStruct sig2 = sig.m_signature;
			object result = _InvokeMethodFast(target, arguments, ref sig2, methodAttributes, typeOwner);
			sig.m_signature = sig2;
			return result;
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[DebuggerHidden]
		[DebuggerStepThrough]
		private extern object _InvokeConstructor(object[] args, ref SignatureStruct signature, IntPtr declaringType);

		[DebuggerHidden]
		[DebuggerStepThrough]
		internal object InvokeConstructor(object[] args, SignatureStruct signature, RuntimeTypeHandle declaringType)
		{
			return _InvokeConstructor(args, ref signature, declaringType.Value);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[DebuggerHidden]
		[DebuggerStepThrough]
		private extern void _SerializationInvoke(object target, ref SignatureStruct declaringTypeSig, SerializationInfo info, StreamingContext context);

		[DebuggerStepThrough]
		[DebuggerHidden]
		internal void SerializationInvoke(object target, SignatureStruct declaringTypeSig, SerializationInfo info, StreamingContext context)
		{
			_SerializationInvoke(target, ref declaringTypeSig, info, context);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal extern bool IsILStub();

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal extern RuntimeTypeHandle[] GetMethodInstantiation();

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal extern bool HasMethodInstantiation();

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal extern RuntimeMethodHandle GetInstantiatingStub(RuntimeTypeHandle declaringTypeHandle, RuntimeTypeHandle[] methodInstantiation);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal extern RuntimeMethodHandle GetUnboxingStub();

		internal RuntimeMethodHandle GetInstantiatingStubIfNeeded(RuntimeTypeHandle declaringTypeHandle)
		{
			return GetInstantiatingStub(declaringTypeHandle, null);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal extern RuntimeMethodHandle GetMethodFromCanonical(RuntimeTypeHandle declaringTypeHandle);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal extern bool IsGenericMethodDefinition();

		[MethodImpl(MethodImplOptions.InternalCall)]
		private unsafe extern void* _GetTypicalMethodDefinition();

		internal unsafe RuntimeMethodHandle GetTypicalMethodDefinition()
		{
			return new RuntimeMethodHandle(_GetTypicalMethodDefinition());
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private unsafe extern void* _StripMethodInstantiation();

		internal unsafe RuntimeMethodHandle StripMethodInstantiation()
		{
			return new RuntimeMethodHandle(_StripMethodInstantiation());
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal extern bool IsDynamicMethod();

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal extern Resolver GetResolver();

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal extern void Destroy();

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal extern MethodBody _GetMethodBody(IntPtr declaringType);

		internal MethodBody GetMethodBody(RuntimeTypeHandle declaringType)
		{
			return _GetMethodBody(declaringType.Value);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal extern bool IsConstructor();
	}
}
