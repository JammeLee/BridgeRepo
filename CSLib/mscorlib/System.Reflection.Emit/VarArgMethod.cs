namespace System.Reflection.Emit
{
	internal class VarArgMethod
	{
		internal MethodInfo m_method;

		internal SignatureHelper m_signature;

		internal VarArgMethod(MethodInfo method, SignatureHelper signature)
		{
			m_method = method;
			m_signature = signature;
		}
	}
}
