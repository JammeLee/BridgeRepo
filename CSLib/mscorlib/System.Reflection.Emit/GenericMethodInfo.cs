namespace System.Reflection.Emit
{
	internal class GenericMethodInfo
	{
		internal RuntimeMethodHandle m_method;

		internal RuntimeTypeHandle m_context;

		internal GenericMethodInfo(RuntimeMethodHandle method, RuntimeTypeHandle context)
		{
			m_method = method;
			m_context = context;
		}
	}
}
