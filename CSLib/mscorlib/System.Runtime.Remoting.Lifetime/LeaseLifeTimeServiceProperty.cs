using System.Globalization;
using System.Runtime.Remoting.Contexts;
using System.Runtime.Remoting.Messaging;

namespace System.Runtime.Remoting.Lifetime
{
	[Serializable]
	internal class LeaseLifeTimeServiceProperty : IContextProperty, IContributeObjectSink
	{
		public string Name => "LeaseLifeTimeServiceProperty";

		public bool IsNewContextOK(Context newCtx)
		{
			return true;
		}

		public void Freeze(Context newContext)
		{
		}

		public IMessageSink GetObjectSink(MarshalByRefObject obj, IMessageSink nextSink)
		{
			bool fServer;
			ServerIdentity serverIdentity = (ServerIdentity)MarshalByRefObject.GetIdentity(obj, out fServer);
			if (serverIdentity.IsSingleCall())
			{
				return nextSink;
			}
			object obj2 = obj.InitializeLifetimeService();
			if (obj2 == null)
			{
				return nextSink;
			}
			if (!(obj2 is ILease))
			{
				throw new RemotingException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Remoting_Lifetime_ILeaseReturn"), obj2));
			}
			ILease lease = (ILease)obj2;
			if (lease.InitialLeaseTime.CompareTo(TimeSpan.Zero) <= 0)
			{
				if (lease is Lease)
				{
					((Lease)lease).Remove();
				}
				return nextSink;
			}
			Lease lease2 = null;
			lock (serverIdentity)
			{
				if (serverIdentity.Lease != null)
				{
					lease2 = serverIdentity.Lease;
					lease2.Renew(lease2.InitialLeaseTime);
				}
				else
				{
					if (!(lease is Lease))
					{
						lease2 = (Lease)LifetimeServices.GetLeaseInitial(obj);
						if (lease2.CurrentState == LeaseState.Initial)
						{
							lease2.InitialLeaseTime = lease.InitialLeaseTime;
							lease2.RenewOnCallTime = lease.RenewOnCallTime;
							lease2.SponsorshipTimeout = lease.SponsorshipTimeout;
						}
					}
					else
					{
						lease2 = (Lease)lease;
					}
					serverIdentity.Lease = lease2;
					if (serverIdentity.ObjectRef != null)
					{
						lease2.ActivateLease();
					}
				}
			}
			if (lease2.RenewOnCallTime > TimeSpan.Zero)
			{
				return new LeaseSink(lease2, nextSink);
			}
			return nextSink;
		}
	}
}
