using System.Runtime.InteropServices;

namespace System.Reflection
{
	[ComVisible(true)]
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false, Inherited = false)]
	public sealed class ObfuscateAssemblyAttribute : Attribute
	{
		private bool m_assemblyIsPrivate;

		private bool m_strip = true;

		public bool AssemblyIsPrivate => m_assemblyIsPrivate;

		public bool StripAfterObfuscation
		{
			get
			{
				return m_strip;
			}
			set
			{
				m_strip = value;
			}
		}

		public ObfuscateAssemblyAttribute(bool assemblyIsPrivate)
		{
			m_assemblyIsPrivate = assemblyIsPrivate;
		}
	}
}
