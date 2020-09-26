using System.Runtime.InteropServices;

namespace System.Reflection
{
	[AttributeUsage(AttributeTargets.Assembly, Inherited = false)]
	[ComVisible(true)]
	public sealed class AssemblyKeyNameAttribute : Attribute
	{
		private string m_keyName;

		public string KeyName => m_keyName;

		public AssemblyKeyNameAttribute(string keyName)
		{
			m_keyName = keyName;
		}
	}
}
