using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace System
{
	[CLSCompliant(false)]
	[ComVisible(true)]
	public struct TypedReference
	{
		private IntPtr Value;

		private IntPtr Type;

		internal bool IsNull
		{
			get
			{
				if (Value.IsNull())
				{
					return Type.IsNull();
				}
				return false;
			}
		}

		[CLSCompliant(false)]
		[ReflectionPermission(SecurityAction.LinkDemand, MemberAccess = true)]
		public unsafe static TypedReference MakeTypedReference(object target, FieldInfo[] flds)
		{
			if (target == null)
			{
				throw new ArgumentNullException("target");
			}
			if (flds == null)
			{
				throw new ArgumentNullException("flds");
			}
			if (flds.Length == 0)
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_ArrayZeroError"));
			}
			RuntimeFieldHandle[] array = new RuntimeFieldHandle[flds.Length];
			Type type = target.GetType();
			for (int i = 0; i < flds.Length; i++)
			{
				FieldInfo fieldInfo = flds[i];
				if (!(fieldInfo is RuntimeFieldInfo))
				{
					throw new ArgumentException(Environment.GetResourceString("Argument_MustBeRuntimeFieldInfo"));
				}
				if (fieldInfo.IsInitOnly || fieldInfo.IsStatic)
				{
					throw new ArgumentException(Environment.GetResourceString("Argument_TypedReferenceInvalidField"));
				}
				if (type != fieldInfo.DeclaringType && !type.IsSubclassOf(fieldInfo.DeclaringType))
				{
					throw new MissingMemberException(Environment.GetResourceString("MissingMemberTypeRef"));
				}
				Type fieldType = fieldInfo.FieldType;
				if (fieldType.IsPrimitive)
				{
					throw new ArgumentException(Environment.GetResourceString("Arg_TypeRefPrimitve"));
				}
				if (i < flds.Length - 1 && !fieldType.IsValueType)
				{
					throw new MissingMemberException(Environment.GetResourceString("MissingMemberNestErr"));
				}
				ref RuntimeFieldHandle reference = ref array[i];
				reference = fieldInfo.FieldHandle;
				type = fieldType;
			}
			TypedReference result = default(TypedReference);
			InternalMakeTypedReference(&result, target, array, type.TypeHandle);
			return result;
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private unsafe static extern void InternalMakeTypedReference(void* result, object target, RuntimeFieldHandle[] flds, RuntimeTypeHandle lastFieldType);

		public override int GetHashCode()
		{
			if (Type == IntPtr.Zero)
			{
				return 0;
			}
			return __reftype(this).GetHashCode();
		}

		public override bool Equals(object o)
		{
			throw new NotSupportedException(Environment.GetResourceString("NotSupported_NYI"));
		}

		public unsafe static object ToObject(TypedReference value)
		{
			return InternalToObject(&value);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal unsafe static extern object InternalToObject(void* value);

		public static Type GetTargetType(TypedReference value)
		{
			return __reftype(value);
		}

		public static RuntimeTypeHandle TargetTypeToken(TypedReference value)
		{
			return __reftype(value).TypeHandle;
		}

		[CLSCompliant(false)]
		public unsafe static void SetTypedReference(TypedReference target, object value)
		{
			InternalSetTypedReference(&target, value);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal unsafe static extern void InternalSetTypedReference(void* target, object value);
	}
}
