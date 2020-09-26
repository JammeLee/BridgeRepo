using System.Collections.Generic;
using System.Threading;

namespace CSLib.Framework
{
	public class CMsgBuffInfoQueue
	{
		private Queue<CMsgBuffInfo> m_ᜀ = new Queue<CMsgBuffInfo>();

		private AutoResetEvent m_ᜁ = new AutoResetEvent(initialState: true);

		public int Count => this.m_ᜀ.Count;

		public void Enqueue(byte[] msgBuff, int msgSize)
		{
			//Discarded unreachable code: IL_0003
			if (true)
			{
			}
			CMsgBuffInfo cMsgBuffInfo = new CMsgBuffInfo();
			cMsgBuffInfo.MsgBuff = msgBuff;
			cMsgBuffInfo.MsgSize = msgSize;
			ᜁ();
			this.m_ᜀ.Enqueue(cMsgBuffInfo);
			ᜀ();
		}

		public void Enqueue(ushort ownerID, byte[] msgBuff, int msgSize)
		{
			//Discarded unreachable code: IL_0003
			if (true)
			{
			}
			CMsgBuffInfo cMsgBuffInfo = new CMsgBuffInfo();
			cMsgBuffInfo.OwnerID = ownerID;
			cMsgBuffInfo.MsgBuff = msgBuff;
			cMsgBuffInfo.MsgSize = msgSize;
			ᜁ();
			this.m_ᜀ.Enqueue(cMsgBuffInfo);
			ᜀ();
		}

		public CMsgBuffInfo Dequeue()
		{
			ᜁ();
			CMsgBuffInfo result = this.m_ᜀ.Dequeue();
			ᜀ();
			return result;
		}

		private void ᜁ()
		{
			this.m_ᜁ.WaitOne();
		}

		private void ᜀ()
		{
			this.m_ᜁ.Set();
		}
	}
}
