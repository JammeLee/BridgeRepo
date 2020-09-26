namespace System.Runtime.Remoting.Channels
{
	internal class DispatchChannelSinkProvider : IServerChannelSinkProvider
	{
		public IServerChannelSinkProvider Next
		{
			get
			{
				return null;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		internal DispatchChannelSinkProvider()
		{
		}

		public void GetChannelData(IChannelDataStore channelData)
		{
		}

		public IServerChannelSink CreateSink(IChannelReceiver channel)
		{
			return new DispatchChannelSink();
		}
	}
}
