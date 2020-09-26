using System.Runtime.InteropServices;

namespace System.Reflection
{
	[ComVisible(true)]
	[AttributeUsage(AttributeTargets.Assembly, Inherited = false)]
	public sealed class AssemblyTitleAttribute : Attribute
	{
		private string m_title;

		public string Title => m_title;

		public AssemblyTitleAttribute(string title)
		{
			m_title = title;
		}
	}
}
