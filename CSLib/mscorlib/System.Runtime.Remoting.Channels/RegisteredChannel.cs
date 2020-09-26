namespace System.Runtime.Remoting.Channels
{
	internal class RegisteredChannel
	{
		private const byte SENDER = 1;

		private const byte RECEIVER = 2;

		private IChannel channel;

		private byte flags;

		internal virtual IChannel Channel => channel;

		internal RegisteredChannel(IChannel chnl)
		{
			channel = chnl;
			flags = 0;
			if (chnl is IChannelSender)
			{
				flags |= 1;
			}
			if (chnl is IChannelReceiver)
			{
				flags |= 2;
			}
		}

		internal virtual bool IsSender()
		{
			return (flags & 1) != 0;
		}

		internal virtual bool IsReceiver()
		{
			return (flags & 2) != 0;
		}
	}
}
