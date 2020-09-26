using System.Globalization;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Threading;

namespace System.Runtime.Remoting.Lifetime
{
	[ComVisible(true)]
	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
	public sealed class LifetimeServices
	{
		private static bool isLeaseTime = false;

		private static bool isRenewOnCallTime = false;

		private static bool isSponsorshipTimeout = false;

		private static TimeSpan m_leaseTime = TimeSpan.FromMinutes(5.0);

		private static TimeSpan m_renewOnCallTime = TimeSpan.FromMinutes(2.0);

		private static TimeSpan m_sponsorshipTimeout = TimeSpan.FromMinutes(2.0);

		private static TimeSpan m_pollTime = TimeSpan.FromMilliseconds(10000.0);

		private static object s_LifetimeSyncObject = null;

		private static object LifetimeSyncObject
		{
			get
			{
				if (s_LifetimeSyncObject == null)
				{
					object value = new object();
					Interlocked.CompareExchange(ref s_LifetimeSyncObject, value, null);
				}
				return s_LifetimeSyncObject;
			}
		}

		public static TimeSpan LeaseTime
		{
			get
			{
				return m_leaseTime;
			}
			[SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.RemotingConfiguration)]
			set
			{
				lock (LifetimeSyncObject)
				{
					if (isLeaseTime)
					{
						throw new RemotingException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Remoting_Lifetime_SetOnce"), "LeaseTime"));
					}
					m_leaseTime = value;
					isLeaseTime = true;
				}
			}
		}

		public static TimeSpan RenewOnCallTime
		{
			get
			{
				return m_renewOnCallTime;
			}
			[SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.RemotingConfiguration)]
			set
			{
				lock (LifetimeSyncObject)
				{
					if (isRenewOnCallTime)
					{
						throw new RemotingException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Remoting_Lifetime_SetOnce"), "RenewOnCallTime"));
					}
					m_renewOnCallTime = value;
					isRenewOnCallTime = true;
				}
			}
		}

		public static TimeSpan SponsorshipTimeout
		{
			get
			{
				return m_sponsorshipTimeout;
			}
			[SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.RemotingConfiguration)]
			set
			{
				lock (LifetimeSyncObject)
				{
					if (isSponsorshipTimeout)
					{
						throw new RemotingException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Remoting_Lifetime_SetOnce"), "SponsorshipTimeout"));
					}
					m_sponsorshipTimeout = value;
					isSponsorshipTimeout = true;
				}
			}
		}

		public static TimeSpan LeaseManagerPollTime
		{
			get
			{
				return m_pollTime;
			}
			[SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.RemotingConfiguration)]
			set
			{
				lock (LifetimeSyncObject)
				{
					m_pollTime = value;
					if (LeaseManager.IsInitialized())
					{
						LeaseManager.GetLeaseManager().ChangePollTime(m_pollTime);
					}
				}
			}
		}

		internal static ILease GetLeaseInitial(MarshalByRefObject obj)
		{
			ILease lease = null;
			LeaseManager leaseManager = LeaseManager.GetLeaseManager(LeaseManagerPollTime);
			lease = leaseManager.GetLease(obj);
			if (lease == null)
			{
				lease = CreateLease(obj);
			}
			return lease;
		}

		internal static ILease GetLease(MarshalByRefObject obj)
		{
			ILease lease = null;
			LeaseManager leaseManager = LeaseManager.GetLeaseManager(LeaseManagerPollTime);
			return leaseManager.GetLease(obj);
		}

		internal static ILease CreateLease(MarshalByRefObject obj)
		{
			return CreateLease(LeaseTime, RenewOnCallTime, SponsorshipTimeout, obj);
		}

		internal static ILease CreateLease(TimeSpan leaseTime, TimeSpan renewOnCallTime, TimeSpan sponsorshipTimeout, MarshalByRefObject obj)
		{
			LeaseManager.GetLeaseManager(LeaseManagerPollTime);
			return new Lease(leaseTime, renewOnCallTime, sponsorshipTimeout, obj);
		}
	}
}
