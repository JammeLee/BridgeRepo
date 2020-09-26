using System.Runtime.InteropServices;

namespace System.Reflection
{
	[ComVisible(true)]
	public class LocalVariableInfo
	{
		private int m_isPinned;

		private int m_localIndex;

		private RuntimeTypeHandle m_typeHandle;

		public virtual Type LocalType => m_typeHandle.GetRuntimeType();

		public virtual bool IsPinned => m_isPinned != 0;

		public virtual int LocalIndex => m_localIndex;

		internal LocalVariableInfo()
		{
		}

		public override string ToString()
		{
			string text = LocalType.ToString() + " (" + LocalIndex + ")";
			if (IsPinned)
			{
				text += " (pinned)";
			}
			return text;
		}
	}
}
