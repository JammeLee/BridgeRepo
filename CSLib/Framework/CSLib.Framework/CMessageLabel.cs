using System;

namespace CSLib.Framework
{
	[Serializable]
	public class CMessageLabel
	{
		private object m_MsgObject;

		private uint m_uniqueID;

		private ushort m_reqIndex;

		private ushort m_resIndex;

		public object MsgObject
		{
			get
			{
				return m_MsgObject;
			}
			set
			{
				m_MsgObject = value;
			}
		}

		public uint UniqueID
		{
			get
			{
				return m_uniqueID;
			}
			set
			{
				m_uniqueID = value;
			}
		}

		public ushort ReqIndex
		{
			get
			{
				return m_reqIndex;
			}
			set
			{
				m_reqIndex = value;
			}
		}

		public ushort ResIndex
		{
			get
			{
				return m_resIndex;
			}
			set
			{
				m_resIndex = value;
			}
		}

		internal static string b(string A_0, int A_1)
		{
			char[] array = A_0.ToCharArray();
			int num = 597903122 + A_1;
			int num2 = 0;
			if (num2 >= 1)
			{
				goto IL_0014;
			}
			goto IL_0047;
			IL_0047:
			if (num2 >= array.Length)
			{
				return string.Intern(new string(array));
			}
			goto IL_0014;
			IL_0014:
			int num3 = num2;
			char num4 = array[num3];
			byte b = (byte)((num4 & 0xFFu) ^ (uint)num++);
			byte b2 = (byte)(((int)num4 >> 8) ^ num++);
			byte num5 = b2;
			b2 = b;
			b = num5;
			array[num3] = (char)((b2 << 8) | b);
			num2++;
			goto IL_0047;
		}
	}
}
