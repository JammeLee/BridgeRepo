using System.Runtime.Remoting.Messaging;

namespace System.Runtime.Remoting
{
	[Serializable]
	internal sealed class EnvoyInfo : IEnvoyInfo
	{
		private IMessageSink envoySinks;

		public IMessageSink EnvoySinks
		{
			get
			{
				return envoySinks;
			}
			set
			{
				envoySinks = value;
			}
		}

		internal static IEnvoyInfo CreateEnvoyInfo(ServerIdentity serverID)
		{
			IEnvoyInfo result = null;
			if (serverID != null)
			{
				if (serverID.EnvoyChain == null)
				{
					serverID.RaceSetEnvoyChain(serverID.ServerContext.CreateEnvoyChain(serverID.TPOrObject));
				}
				IMessageSink messageSink = serverID.EnvoyChain as EnvoyTerminatorSink;
				if (messageSink == null)
				{
					result = new EnvoyInfo(serverID.EnvoyChain);
				}
			}
			return result;
		}

		private EnvoyInfo(IMessageSink sinks)
		{
			EnvoySinks = sinks;
		}
	}
}
