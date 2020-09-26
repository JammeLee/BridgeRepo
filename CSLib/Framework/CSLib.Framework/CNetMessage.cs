using System;
using CSLib.Utility;

namespace CSLib.Framework
{
	[Serializable]
	public class CNetMessage : CMessage
	{
		public byte Server => CBitHelper.GetHighUInt8(base.MsgType);

		public byte Func => CBitHelper.GetLowUInt8(base.MsgType);

		public CNetMessage(ushort type, ushort id)
			: base(type, id)
		{
		}

		public CNetMessage(byte server, byte func, ushort id)
			: base(CBitHelper.MergeUInt8(server, func), id)
		{
		}

		protected override bool _Deserialize(CStream stream)
		{
			return true;
		}

		protected override bool _Serialize(CStream stream)
		{
			return true;
		}
	}
}
