using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace System
{
	[Serializable]
	[ComVisible(true)]
	public struct RuntimeFieldHandle : ISerializable
	{
		private IntPtr m_ptr;

		public IntPtr Value
		{
			[SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
			get
			{
				return m_ptr;
			}
		}

		internal unsafe RuntimeFieldHandle(void* pFieldHandle)
		{
			m_ptr = new IntPtr(pFieldHandle);
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
			if (!(obj is RuntimeFieldHandle))
			{
				return false;
			}
			return ((RuntimeFieldHandle)obj).m_ptr == m_ptr;
		}

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		public bool Equals(RuntimeFieldHandle handle)
		{
			return handle.m_ptr == m_ptr;
		}

		public static bool operator ==(RuntimeFieldHandle left, RuntimeFieldHandle right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(RuntimeFieldHandle left, RuntimeFieldHandle right)
		{
			return !left.Equals(right);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal extern string GetName();

		[MethodImpl(MethodImplOptions.InternalCall)]
		private unsafe extern void* _GetUtf8Name();

		internal unsafe Utf8String GetUtf8Name()
		{
			return new Utf8String(_GetUtf8Name());
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal extern FieldAttributes GetAttributes();

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal extern RuntimeTypeHandle GetApproxDeclaringType();

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal extern int GetToken();

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal extern object GetValue(object instance, RuntimeTypeHandle fieldType, RuntimeTypeHandle declaringType, ref bool domainInitialized);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal extern object GetValueDirect(RuntimeTypeHandle fieldType, TypedReference obj, RuntimeTypeHandle contextType);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal extern void SetValue(object obj, object value, RuntimeTypeHandle fieldType, FieldAttributes fieldAttr, RuntimeTypeHandle declaringType, ref bool domainInitialized);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal extern void SetValueDirect(RuntimeTypeHandle fieldType, TypedReference obj, object value, RuntimeTypeHandle contextType);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal extern RuntimeFieldHandle GetStaticFieldForGenericType(RuntimeTypeHandle declaringType);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal extern bool AcquiresContextFromThis();

		private unsafe RuntimeFieldHandle(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			FieldInfo fieldInfo = (RuntimeFieldInfo)info.GetValue("FieldObj", typeof(RuntimeFieldInfo));
			if (fieldInfo == null)
			{
				throw new SerializationException(Environment.GetResourceString("Serialization_InsufficientState"));
			}
			m_ptr = fieldInfo.FieldHandle.Value;
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
			RuntimeFieldInfo value = (RuntimeFieldInfo)RuntimeType.GetFieldInfo(this);
			info.AddValue("FieldObj", value, typeof(RuntimeFieldInfo));
		}
	}
}
