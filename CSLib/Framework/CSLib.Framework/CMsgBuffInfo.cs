namespace CSLib.Framework
{
	public class CMsgBuffInfo
	{
		private ushort ᜀ;

		private byte[] ᜁ;

		private int ᜂ;

		public ushort OwnerID
		{
			get
			{
				return ᜀ;
			}
			set
			{
				ᜀ = value;
			}
		}

		public byte[] MsgBuff
		{
			get
			{
				return ᜁ;
			}
			set
			{
				ᜁ = value;
			}
		}

		public int MsgSize
		{
			get
			{
				return ᜂ;
			}
			set
			{
				ᜂ = value;
			}
		}
	}
}
