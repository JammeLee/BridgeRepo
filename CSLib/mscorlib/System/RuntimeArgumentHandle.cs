using System.Runtime.InteropServices;

namespace System
{
	[ComVisible(true)]
	public struct RuntimeArgumentHandle
	{
		private IntPtr m_ptr;

		internal IntPtr Value => m_ptr;
	}
}
