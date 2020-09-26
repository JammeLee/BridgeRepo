using System.Collections;
using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace System.Runtime.Remoting.Channels
{
	[ComVisible(true)]
	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
	[SecurityPermission(SecurityAction.InheritanceDemand, Flags = SecurityPermissionFlag.Infrastructure)]
	public abstract class BaseChannelWithProperties : BaseChannelObjectWithProperties
	{
		protected IChannelSinkBase SinksWithProperties;

		public override IDictionary Properties
		{
			[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
			get
			{
				ArrayList arrayList = new ArrayList();
				arrayList.Add(this);
				if (SinksWithProperties != null)
				{
					IServerChannelSink serverChannelSink = SinksWithProperties as IServerChannelSink;
					if (serverChannelSink != null)
					{
						while (serverChannelSink != null)
						{
							IDictionary properties = serverChannelSink.Properties;
							if (properties != null)
							{
								arrayList.Add(properties);
							}
							serverChannelSink = serverChannelSink.NextChannelSink;
						}
					}
					else
					{
						for (IClientChannelSink clientChannelSink = (IClientChannelSink)SinksWithProperties; clientChannelSink != null; clientChannelSink = clientChannelSink.NextChannelSink)
						{
							IDictionary properties2 = clientChannelSink.Properties;
							if (properties2 != null)
							{
								arrayList.Add(properties2);
							}
						}
					}
				}
				return new AggregateDictionary(arrayList);
			}
		}
	}
}
