using System.Reflection;

namespace System
{
	internal struct SignatureStruct
	{
		internal RuntimeTypeHandle[] m_arguments;

		internal unsafe void* m_sig;

		internal unsafe void* m_pCallTarget;

		internal CallingConventions m_managedCallingConvention;

		internal int m_csig;

		internal int m_numVirtualFixedArgs;

		internal int m_64bitpad;

		internal RuntimeMethodHandle m_pMethod;

		internal RuntimeTypeHandle m_declaringType;

		internal RuntimeTypeHandle m_returnTypeORfieldType;

		public unsafe SignatureStruct(RuntimeMethodHandle method, RuntimeTypeHandle[] arguments, RuntimeTypeHandle returnType, CallingConventions callingConvention)
		{
			m_pMethod = method;
			m_arguments = arguments;
			m_returnTypeORfieldType = returnType;
			m_managedCallingConvention = callingConvention;
			m_sig = null;
			m_pCallTarget = null;
			m_csig = 0;
			m_numVirtualFixedArgs = 0;
			m_64bitpad = 0;
			m_declaringType = default(RuntimeTypeHandle);
		}
	}
}
