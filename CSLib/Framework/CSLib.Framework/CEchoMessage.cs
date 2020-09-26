using System;
using CSLib.Utility;

namespace CSLib.Framework
{
	[Serializable]
	public class CEchoMessage : CNetMessage
	{
		private ushort m_echoID;

		public ushort EchoID
		{
			get
			{
				return m_echoID;
			}
			set
			{
				m_echoID = value;
			}
		}

		public CEchoMessage(ushort type, ushort id)
			: base(type, id)
		{
		}

		public CEchoMessage(byte server, byte func, ushort id)
			: base(CBitHelper.MergeUInt8(server, func), id)
		{
		}

		protected override bool _Deserialize(CStream stream)
		{
			stream.Read(ref m_echoID);
			return true;
		}

		protected override bool _Serialize(CStream stream)
		{
			stream.Write(m_echoID);
			return true;
		}
	}
}
