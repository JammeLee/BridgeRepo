using System.Reflection;
using System.Runtime.CompilerServices;

namespace System
{
	internal class Signature
	{
		internal enum MdSigCallingConvention : byte
		{
			Generics = 0x10,
			HasThis = 0x20,
			ExplicitThis = 0x40,
			CallConvMask = 0xF,
			Default = 0,
			C = 1,
			StdCall = 2,
			ThisCall = 3,
			FastCall = 4,
			Vararg = 5,
			Field = 6,
			LocalSig = 7,
			Property = 8,
			Unmgd = 9,
			GenericInst = 10,
			Max = 11
		}

		internal SignatureStruct m_signature;

		internal CallingConventions CallingConvention => m_signature.m_managedCallingConvention & (CallingConventions)255;

		internal RuntimeTypeHandle[] Arguments => m_signature.m_arguments;

		internal RuntimeTypeHandle ReturnTypeHandle => m_signature.m_returnTypeORfieldType;

		internal RuntimeTypeHandle FieldTypeHandle => m_signature.m_returnTypeORfieldType;

		public static implicit operator SignatureStruct(Signature pThis)
		{
			return pThis.m_signature;
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private unsafe static extern void _GetSignature(ref SignatureStruct signature, void* pCorSig, int cCorSig, IntPtr fieldHandle, IntPtr methodHandle, IntPtr declaringTypeHandle);

		private unsafe static void GetSignature(ref SignatureStruct signature, void* pCorSig, int cCorSig, RuntimeFieldHandle fieldHandle, RuntimeMethodHandle methodHandle, RuntimeTypeHandle declaringTypeHandle)
		{
			_GetSignature(ref signature, pCorSig, cCorSig, fieldHandle.Value, methodHandle.Value, declaringTypeHandle.Value);
		}

		internal static void GetSignatureForDynamicMethod(ref SignatureStruct signature, RuntimeMethodHandle methodHandle)
		{
			_GetSignature(ref signature, null, 0, (IntPtr)0, methodHandle.Value, (IntPtr)0);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void GetCustomModifiers(ref SignatureStruct signature, int parameter, out RuntimeTypeHandle[] required, out RuntimeTypeHandle[] optional);

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern bool CompareSig(ref SignatureStruct left, RuntimeTypeHandle typeLeft, ref SignatureStruct right, RuntimeTypeHandle typeRight);

		public Signature(RuntimeMethodHandle method, RuntimeTypeHandle[] arguments, RuntimeTypeHandle returnType, CallingConventions callingConvention)
		{
			SignatureStruct signature = new SignatureStruct(method, arguments, returnType, callingConvention);
			GetSignatureForDynamicMethod(ref signature, method);
			m_signature = signature;
		}

		public Signature(RuntimeMethodHandle methodHandle, RuntimeTypeHandle declaringTypeHandle)
		{
			SignatureStruct signature = default(SignatureStruct);
			GetSignature(ref signature, null, 0, new RuntimeFieldHandle(null), methodHandle, declaringTypeHandle);
			m_signature = signature;
		}

		public Signature(RuntimeFieldHandle fieldHandle, RuntimeTypeHandle declaringTypeHandle)
		{
			SignatureStruct signature = default(SignatureStruct);
			GetSignature(ref signature, null, 0, fieldHandle, new RuntimeMethodHandle(null), declaringTypeHandle);
			m_signature = signature;
		}

		public unsafe Signature(void* pCorSig, int cCorSig, RuntimeTypeHandle declaringTypeHandle)
		{
			SignatureStruct signature = default(SignatureStruct);
			GetSignature(ref signature, pCorSig, cCorSig, new RuntimeFieldHandle(null), new RuntimeMethodHandle(null), declaringTypeHandle);
			m_signature = signature;
		}

		internal static bool DiffSigs(Signature sig1, RuntimeTypeHandle typeHandle1, Signature sig2, RuntimeTypeHandle typeHandle2)
		{
			SignatureStruct left = sig1;
			SignatureStruct right = sig2;
			return CompareSig(ref left, typeHandle1, ref right, typeHandle2);
		}

		public Type[] GetCustomModifiers(int position, bool required)
		{
			RuntimeTypeHandle[] required2 = null;
			RuntimeTypeHandle[] optional = null;
			SignatureStruct signature = this;
			GetCustomModifiers(ref signature, position, out required2, out optional);
			Type[] array = new Type[required ? required2.Length : optional.Length];
			if (required)
			{
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = required2[i].GetRuntimeType();
				}
			}
			else
			{
				for (int j = 0; j < array.Length; j++)
				{
					array[j] = optional[j].GetRuntimeType();
				}
			}
			return array;
		}
	}
}
