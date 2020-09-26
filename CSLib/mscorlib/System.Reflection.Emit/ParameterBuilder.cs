using System.Runtime.InteropServices;

namespace System.Reflection.Emit
{
	[ClassInterface(ClassInterfaceType.None)]
	[ComDefaultInterface(typeof(_ParameterBuilder))]
	[ComVisible(true)]
	public class ParameterBuilder : _ParameterBuilder
	{
		private string m_strParamName;

		private int m_iPosition;

		private ParameterAttributes m_attributes;

		private MethodBuilder m_methodBuilder;

		private ParameterToken m_pdToken;

		internal virtual int MetadataTokenInternal => m_pdToken.Token;

		public virtual string Name => m_strParamName;

		public virtual int Position => m_iPosition;

		public virtual int Attributes => (int)m_attributes;

		public bool IsIn => (m_attributes & ParameterAttributes.In) != 0;

		public bool IsOut => (m_attributes & ParameterAttributes.Out) != 0;

		public bool IsOptional => (m_attributes & ParameterAttributes.Optional) != 0;

		[Obsolete("An alternate API is available: Emit the MarshalAs custom attribute instead. http://go.microsoft.com/fwlink/?linkid=14202")]
		public virtual void SetMarshal(UnmanagedMarshal unmanagedMarshal)
		{
			if (unmanagedMarshal == null)
			{
				throw new ArgumentNullException("unmanagedMarshal");
			}
			byte[] array = unmanagedMarshal.InternalGetBytes();
			TypeBuilder.InternalSetMarshalInfo(m_methodBuilder.GetModule(), m_pdToken.Token, array, array.Length);
		}

		public virtual void SetConstant(object defaultValue)
		{
			TypeBuilder.SetConstantValue(m_methodBuilder.GetModule(), m_pdToken.Token, (m_iPosition == 0) ? m_methodBuilder.m_returnType : m_methodBuilder.m_parameterTypes[m_iPosition - 1], defaultValue);
		}

		[ComVisible(true)]
		public void SetCustomAttribute(ConstructorInfo con, byte[] binaryAttribute)
		{
			if (con == null)
			{
				throw new ArgumentNullException("con");
			}
			if (binaryAttribute == null)
			{
				throw new ArgumentNullException("binaryAttribute");
			}
			TypeBuilder.InternalCreateCustomAttribute(m_pdToken.Token, ((ModuleBuilder)m_methodBuilder.GetModule()).GetConstructorToken(con).Token, binaryAttribute, m_methodBuilder.GetModule(), toDisk: false);
		}

		public void SetCustomAttribute(CustomAttributeBuilder customBuilder)
		{
			if (customBuilder == null)
			{
				throw new ArgumentNullException("customBuilder");
			}
			customBuilder.CreateCustomAttribute((ModuleBuilder)m_methodBuilder.GetModule(), m_pdToken.Token);
		}

		private ParameterBuilder()
		{
		}

		internal ParameterBuilder(MethodBuilder methodBuilder, int sequence, ParameterAttributes attributes, string strParamName)
		{
			m_iPosition = sequence;
			m_strParamName = strParamName;
			m_methodBuilder = methodBuilder;
			m_strParamName = strParamName;
			m_attributes = attributes;
			m_pdToken = new ParameterToken(TypeBuilder.InternalSetParamInfo(m_methodBuilder.GetModule(), m_methodBuilder.GetToken().Token, sequence, attributes, strParamName));
		}

		public virtual ParameterToken GetToken()
		{
			return m_pdToken;
		}

		void _ParameterBuilder.GetTypeInfoCount(out uint pcTInfo)
		{
			throw new NotImplementedException();
		}

		void _ParameterBuilder.GetTypeInfo(uint iTInfo, uint lcid, IntPtr ppTInfo)
		{
			throw new NotImplementedException();
		}

		void _ParameterBuilder.GetIDsOfNames([In] ref Guid riid, IntPtr rgszNames, uint cNames, uint lcid, IntPtr rgDispId)
		{
			throw new NotImplementedException();
		}

		void _ParameterBuilder.Invoke(uint dispIdMember, [In] ref Guid riid, uint lcid, short wFlags, IntPtr pDispParams, IntPtr pVarResult, IntPtr pExcepInfo, IntPtr puArgErr)
		{
			throw new NotImplementedException();
		}
	}
}
