using System.Runtime.InteropServices;

namespace System.Reflection.Emit
{
	[ComVisible(true)]
	public class DynamicILInfo
	{
		private DynamicMethod m_method;

		private DynamicScope m_scope;

		private byte[] m_exceptions;

		private byte[] m_code;

		private byte[] m_localSignature;

		private int m_maxStackSize;

		private int m_methodSignature;

		internal byte[] LocalSignature
		{
			get
			{
				if (m_localSignature == null)
				{
					m_localSignature = SignatureHelper.GetLocalVarSigHelper().InternalGetSignatureArray();
				}
				return m_localSignature;
			}
		}

		internal byte[] Exceptions => m_exceptions;

		internal byte[] Code => m_code;

		internal int MaxStackSize => m_maxStackSize;

		public DynamicMethod DynamicMethod => m_method;

		internal DynamicScope DynamicScope => m_scope;

		internal DynamicILInfo(DynamicScope scope, DynamicMethod method, byte[] methodSignature)
		{
			m_method = method;
			m_scope = scope;
			m_methodSignature = m_scope.GetTokenFor(methodSignature);
			m_exceptions = new byte[0];
			m_code = new byte[0];
			m_localSignature = new byte[0];
		}

		internal unsafe RuntimeMethodHandle GetCallableMethod(void* module)
		{
			return new RuntimeMethodHandle(ModuleHandle.GetDynamicMethod(module, m_method.Name, (byte[])m_scope[m_methodSignature], new DynamicResolver(this)));
		}

		public void SetCode(byte[] code, int maxStackSize)
		{
			if (code == null)
			{
				code = new byte[0];
			}
			m_code = (byte[])code.Clone();
			m_maxStackSize = maxStackSize;
		}

		[CLSCompliant(false)]
		public unsafe void SetCode(byte* code, int codeSize, int maxStackSize)
		{
			if (codeSize < 0)
			{
				throw new ArgumentOutOfRangeException("codeSize", Environment.GetResourceString("ArgumentOutOfRange_GenericPositive"));
			}
			m_code = new byte[codeSize];
			for (int i = 0; i < codeSize; i++)
			{
				m_code[i] = *code;
				code++;
			}
			m_maxStackSize = maxStackSize;
		}

		public void SetExceptions(byte[] exceptions)
		{
			if (exceptions == null)
			{
				exceptions = new byte[0];
			}
			m_exceptions = (byte[])exceptions.Clone();
		}

		[CLSCompliant(false)]
		public unsafe void SetExceptions(byte* exceptions, int exceptionsSize)
		{
			if (exceptionsSize < 0)
			{
				throw new ArgumentOutOfRangeException("exceptionsSize", Environment.GetResourceString("ArgumentOutOfRange_GenericPositive"));
			}
			m_exceptions = new byte[exceptionsSize];
			for (int i = 0; i < exceptionsSize; i++)
			{
				m_exceptions[i] = *exceptions;
				exceptions++;
			}
		}

		public void SetLocalSignature(byte[] localSignature)
		{
			if (localSignature == null)
			{
				localSignature = new byte[0];
			}
			m_localSignature = (byte[])localSignature.Clone();
		}

		[CLSCompliant(false)]
		public unsafe void SetLocalSignature(byte* localSignature, int signatureSize)
		{
			if (signatureSize < 0)
			{
				throw new ArgumentOutOfRangeException("signatureSize", Environment.GetResourceString("ArgumentOutOfRange_GenericPositive"));
			}
			m_localSignature = new byte[signatureSize];
			for (int i = 0; i < signatureSize; i++)
			{
				m_localSignature[i] = *localSignature;
				localSignature++;
			}
		}

		public int GetTokenFor(RuntimeMethodHandle method)
		{
			return DynamicScope.GetTokenFor(method);
		}

		public int GetTokenFor(DynamicMethod method)
		{
			return DynamicScope.GetTokenFor(method);
		}

		public int GetTokenFor(RuntimeMethodHandle method, RuntimeTypeHandle contextType)
		{
			return DynamicScope.GetTokenFor(method, contextType);
		}

		public int GetTokenFor(RuntimeFieldHandle field)
		{
			return DynamicScope.GetTokenFor(field);
		}

		public int GetTokenFor(RuntimeTypeHandle type)
		{
			return DynamicScope.GetTokenFor(type);
		}

		public int GetTokenFor(string literal)
		{
			return DynamicScope.GetTokenFor(literal);
		}

		public int GetTokenFor(byte[] signature)
		{
			return DynamicScope.GetTokenFor(signature);
		}
	}
}
