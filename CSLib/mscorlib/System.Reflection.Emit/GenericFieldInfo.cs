namespace System.Reflection.Emit
{
	internal class GenericFieldInfo
	{
		internal RuntimeFieldHandle m_field;

		internal RuntimeTypeHandle m_context;

		internal GenericFieldInfo(RuntimeFieldHandle field, RuntimeTypeHandle context)
		{
			m_field = field;
			m_context = context;
		}
	}
}
