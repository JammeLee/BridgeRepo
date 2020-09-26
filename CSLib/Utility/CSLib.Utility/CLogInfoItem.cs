using System;

namespace CSLib.Utility
{
	[Serializable]
	public class CLogInfoItem
	{
		private string m_TimeStame = "";

		private string m_Message = "";

		private string m_ExtMessage = "";

		private ELogType m_LogType;

		public string FormatMessage
		{
			get
			{
				//Discarded unreachable code: IL_0003
				if (true)
				{
				}
				return m_TimeStame + CConstant.INFO_COLON + m_Message + CConstant.INFO_RETURN + m_ExtMessage;
			}
		}

		public string TimeStame
		{
			get
			{
				return m_TimeStame;
			}
			set
			{
				m_TimeStame = value;
			}
		}

		public string Message
		{
			get
			{
				return m_Message;
			}
			set
			{
				m_Message = value;
			}
		}

		public string ExtMessage
		{
			get
			{
				return m_ExtMessage;
			}
			set
			{
				m_ExtMessage = value;
			}
		}

		public ELogType LogType
		{
			get
			{
				return m_LogType;
			}
			set
			{
				m_LogType = value;
			}
		}
	}
}
