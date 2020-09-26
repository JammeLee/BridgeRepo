using System;
using CSLib.Utility;

namespace CSLib.Framework
{
	[Serializable]
	public class CMessage
	{
		private ushort m_Type;

		private ushort m_Id;

		protected byte[] m_Buf;

		protected short m_Result;

		public ushort MsgType
		{
			get
			{
				return m_Type;
			}
			set
			{
				m_Type = value;
			}
		}

		public ushort Id
		{
			get
			{
				return m_Id;
			}
			set
			{
				m_Id = value;
			}
		}

		public uint UniqueID
		{
			get
			{
				return CBitHelper.MergeUInt16(m_Type, m_Id);
			}
			set
			{
				m_Type = CBitHelper.GetHighUInt16(value);
				m_Id = CBitHelper.GetLowUInt16(value);
			}
		}

		public byte[] Buffer
		{
			get
			{
				return m_Buf;
			}
			set
			{
				m_Buf = value;
			}
		}

		public short Result
		{
			get
			{
				return m_Result;
			}
			set
			{
				m_Result = value;
			}
		}

		public CMessage(ushort type, ushort id)
		{
			m_Type = type;
			m_Id = id;
		}

		public bool Serialize(CStream stream)
		{
			//Discarded unreachable code: IL_0003
			if (true)
			{
			}
			stream.Write(m_Type);
			stream.Write(m_Id);
			return _Serialize(stream);
		}

		public bool Deserialize(CStream stream)
		{
			return _Deserialize(stream);
		}

		protected virtual bool _Serialize(CStream stream)
		{
			return true;
		}

		protected virtual bool _Deserialize(CStream stream)
		{
			return true;
		}

		public virtual ushort GetReqIndex()
		{
			return 0;
		}

		public virtual ushort GetResIndex()
		{
			return 0;
		}
	}
}
