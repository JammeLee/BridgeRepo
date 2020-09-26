using System.Collections;
using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace System.Runtime.Remoting.Lifetime
{
	[ComVisible(true)]
	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
	[SecurityPermission(SecurityAction.InheritanceDemand, Flags = SecurityPermissionFlag.Infrastructure)]
	public class ClientSponsor : MarshalByRefObject, ISponsor
	{
		private Hashtable sponsorTable = new Hashtable(10);

		private TimeSpan m_renewalTime = TimeSpan.FromMinutes(2.0);

		public TimeSpan RenewalTime
		{
			get
			{
				return m_renewalTime;
			}
			set
			{
				m_renewalTime = value;
			}
		}

		public ClientSponsor()
		{
		}

		public ClientSponsor(TimeSpan renewalTime)
		{
			m_renewalTime = renewalTime;
		}

		public bool Register(MarshalByRefObject obj)
		{
			ILease lease = (ILease)obj.GetLifetimeService();
			if (lease == null)
			{
				return false;
			}
			lease.Register(this);
			lock (sponsorTable)
			{
				sponsorTable[obj] = lease;
			}
			return true;
		}

		public void Unregister(MarshalByRefObject obj)
		{
			ILease lease = null;
			lock (sponsorTable)
			{
				lease = (ILease)sponsorTable[obj];
			}
			lease?.Unregister(this);
		}

		public TimeSpan Renewal(ILease lease)
		{
			return m_renewalTime;
		}

		public void Close()
		{
			lock (sponsorTable)
			{
				IDictionaryEnumerator enumerator = sponsorTable.GetEnumerator();
				while (enumerator.MoveNext())
				{
					((ILease)enumerator.Value).Unregister(this);
				}
				sponsorTable.Clear();
			}
		}

		public override object InitializeLifetimeService()
		{
			return null;
		}

		~ClientSponsor()
		{
		}
	}
}
