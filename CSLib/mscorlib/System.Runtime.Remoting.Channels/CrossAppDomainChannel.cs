using System.Runtime.Remoting.Contexts;
using System.Runtime.Remoting.Messaging;
using System.Security;
using System.Security.Permissions;
using System.Threading;

namespace System.Runtime.Remoting.Channels
{
	[Serializable]
	internal class CrossAppDomainChannel : IChannelSender, IChannelReceiver, IChannel
	{
		private const string _channelName = "XAPPDMN";

		private const string _channelURI = "XAPPDMN_URI";

		private static object staticSyncObject = new object();

		private static PermissionSet s_fullTrust = new PermissionSet(PermissionState.Unrestricted);

		private static CrossAppDomainChannel gAppDomainChannel
		{
			get
			{
				return Thread.GetDomain().RemotingData.ChannelServicesData.xadmessageSink;
			}
			set
			{
				Thread.GetDomain().RemotingData.ChannelServicesData.xadmessageSink = value;
			}
		}

		internal static CrossAppDomainChannel AppDomainChannel
		{
			get
			{
				if (CrossAppDomainChannel.gAppDomainChannel == null)
				{
					CrossAppDomainChannel gAppDomainChannel = new CrossAppDomainChannel();
					lock (staticSyncObject)
					{
						if (CrossAppDomainChannel.gAppDomainChannel == null)
						{
							CrossAppDomainChannel.gAppDomainChannel = gAppDomainChannel;
						}
					}
				}
				return CrossAppDomainChannel.gAppDomainChannel;
			}
		}

		public virtual string ChannelName => "XAPPDMN";

		public virtual string ChannelURI => "XAPPDMN_URI";

		public virtual int ChannelPriority => 100;

		public virtual object ChannelData => new CrossAppDomainData(Context.DefaultContext.InternalContextID, Thread.GetDomain().GetId(), Identity.ProcessGuid);

		internal static void RegisterChannel()
		{
			CrossAppDomainChannel appDomainChannel = AppDomainChannel;
			ChannelServices.RegisterChannelInternal(appDomainChannel, ensureSecurity: false);
		}

		public string Parse(string url, out string objectURI)
		{
			objectURI = url;
			return null;
		}

		public virtual IMessageSink CreateMessageSink(string url, object data, out string objectURI)
		{
			objectURI = null;
			IMessageSink result = null;
			if (url != null && data == null)
			{
				if (url.StartsWith("XAPPDMN", StringComparison.Ordinal))
				{
					throw new RemotingException(Environment.GetResourceString("Remoting_AppDomains_NYI"));
				}
			}
			else
			{
				CrossAppDomainData crossAppDomainData = data as CrossAppDomainData;
				if (crossAppDomainData != null && crossAppDomainData.ProcessGuid.Equals(Identity.ProcessGuid))
				{
					result = CrossAppDomainSink.FindOrCreateSink(crossAppDomainData);
				}
			}
			return result;
		}

		public virtual string[] GetUrlsForUri(string objectURI)
		{
			throw new NotSupportedException(Environment.GetResourceString("NotSupported_Method"));
		}

		public virtual void StartListening(object data)
		{
		}

		public virtual void StopListening(object data)
		{
		}
	}
}
