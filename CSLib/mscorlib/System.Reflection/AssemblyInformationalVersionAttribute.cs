using System.Runtime.InteropServices;

namespace System.Reflection
{
	[AttributeUsage(AttributeTargets.Assembly, Inherited = false)]
	[ComVisible(true)]
	public sealed class AssemblyInformationalVersionAttribute : Attribute
	{
		private string m_informationalVersion;

		public string InformationalVersion => m_informationalVersion;

		public AssemblyInformationalVersionAttribute(string informationalVersion)
		{
			m_informationalVersion = informationalVersion;
		}
	}
}
