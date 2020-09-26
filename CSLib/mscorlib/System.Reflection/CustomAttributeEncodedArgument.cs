using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System.Reflection
{
	[Serializable]
	[StructLayout(LayoutKind.Auto)]
	internal struct CustomAttributeEncodedArgument
	{
		private long m_primitiveValue;

		private CustomAttributeEncodedArgument[] m_arrayValue;

		private string m_stringValue;

		private CustomAttributeType m_type;

		public CustomAttributeType CustomAttributeType => m_type;

		public long PrimitiveValue => m_primitiveValue;

		public CustomAttributeEncodedArgument[] ArrayValue => m_arrayValue;

		public string StringValue => m_stringValue;

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void ParseAttributeArguments(IntPtr pCa, int cCa, ref CustomAttributeCtorParameter[] CustomAttributeCtorParameters, ref CustomAttributeNamedParameter[] CustomAttributeTypedArgument, IntPtr assembly);

		internal unsafe static void ParseAttributeArguments(ConstArray attributeBlob, ref CustomAttributeCtorParameter[] customAttributeCtorParameters, ref CustomAttributeNamedParameter[] customAttributeNamedParameters, Module customAttributeModule)
		{
			if (customAttributeModule == null)
			{
				throw new ArgumentNullException("customAttributeModule");
			}
			if (customAttributeNamedParameters == null)
			{
				customAttributeNamedParameters = new CustomAttributeNamedParameter[0];
			}
			CustomAttributeCtorParameter[] CustomAttributeCtorParameters = customAttributeCtorParameters;
			CustomAttributeNamedParameter[] CustomAttributeTypedArgument = customAttributeNamedParameters;
			ParseAttributeArguments(attributeBlob.Signature, attributeBlob.Length, ref CustomAttributeCtorParameters, ref CustomAttributeTypedArgument, (IntPtr)customAttributeModule.Assembly.AssemblyHandle.Value);
			customAttributeCtorParameters = CustomAttributeCtorParameters;
			customAttributeNamedParameters = CustomAttributeTypedArgument;
		}
	}
}
