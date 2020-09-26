using System;

namespace CSLib.Framework
{
	[Serializable]
	internal class CMessageBlock
	{
		private CMessage m_Msg;

		private CMessageLabel m_Label;

		public CMessage Msg
		{
			get
			{
				return m_Msg;
			}
			set
			{
				m_Msg = value;
			}
		}

		public CMessageLabel Label
		{
			get
			{
				return m_Label;
			}
			set
			{
				m_Label = value;
			}
		}
	}
}
