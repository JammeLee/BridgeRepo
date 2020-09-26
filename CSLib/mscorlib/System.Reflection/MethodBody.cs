using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace System.Reflection
{
	[ComVisible(true)]
	public sealed class MethodBody
	{
		private byte[] m_IL;

		private ExceptionHandlingClause[] m_exceptionHandlingClauses;

		private LocalVariableInfo[] m_localVariables;

		internal MethodBase m_methodBase;

		private int m_localSignatureMetadataToken;

		private int m_maxStackSize;

		private bool m_initLocals;

		public int LocalSignatureMetadataToken => m_localSignatureMetadataToken;

		public IList<LocalVariableInfo> LocalVariables => Array.AsReadOnly(m_localVariables);

		public int MaxStackSize => m_maxStackSize;

		public bool InitLocals => m_initLocals;

		public IList<ExceptionHandlingClause> ExceptionHandlingClauses => Array.AsReadOnly(m_exceptionHandlingClauses);

		private MethodBody()
		{
		}

		public byte[] GetILAsByteArray()
		{
			return m_IL;
		}
	}
}
