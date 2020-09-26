using System.Runtime.Remoting.Channels;

namespace System.Runtime.Remoting
{
	[Serializable]
	internal sealed class ChannelInfo : IChannelInfo
	{
		private object[] channelData;

		public object[] ChannelData
		{
			get
			{
				return channelData;
			}
			set
			{
				channelData = value;
			}
		}

		internal ChannelInfo()
		{
			ChannelData = ChannelServices.CurrentChannelData;
		}
	}
}
