using System.Collections;
using System.Globalization;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;
using System.Security.Permissions;
using System.Threading;

namespace System.Runtime.Remoting.Lifetime
{
	internal class Lease : MarshalByRefObject, ILease
	{
		internal delegate TimeSpan AsyncRenewal(ILease lease);

		[Serializable]
		internal enum SponsorState
		{
			Initial,
			Waiting,
			Completed
		}

		internal sealed class SponsorStateInfo
		{
			internal TimeSpan renewalTime;

			internal SponsorState sponsorState;

			internal SponsorStateInfo(TimeSpan renewalTime, SponsorState sponsorState)
			{
				this.renewalTime = renewalTime;
				this.sponsorState = sponsorState;
			}
		}

		internal int id;

		internal DateTime leaseTime;

		internal TimeSpan initialLeaseTime;

		internal TimeSpan renewOnCallTime;

		internal TimeSpan sponsorshipTimeout;

		internal bool isInfinite;

		internal Hashtable sponsorTable;

		internal int sponsorCallThread;

		internal LeaseManager leaseManager;

		internal MarshalByRefObject managedObject;

		internal LeaseState state;

		internal static int nextId;

		public TimeSpan RenewOnCallTime
		{
			get
			{
				return renewOnCallTime;
			}
			[SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.RemotingConfiguration)]
			set
			{
				if (state == LeaseState.Initial)
				{
					renewOnCallTime = value;
					return;
				}
				throw new RemotingException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Remoting_Lifetime_InitialStateRenewOnCall"), state.ToString()));
			}
		}

		public TimeSpan SponsorshipTimeout
		{
			get
			{
				return sponsorshipTimeout;
			}
			[SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.RemotingConfiguration)]
			set
			{
				if (state == LeaseState.Initial)
				{
					sponsorshipTimeout = value;
					return;
				}
				throw new RemotingException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Remoting_Lifetime_InitialStateSponsorshipTimeout"), state.ToString()));
			}
		}

		public TimeSpan InitialLeaseTime
		{
			get
			{
				return initialLeaseTime;
			}
			[SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.RemotingConfiguration)]
			set
			{
				if (state == LeaseState.Initial)
				{
					initialLeaseTime = value;
					if (TimeSpan.Zero.CompareTo(value) >= 0)
					{
						state = LeaseState.Null;
					}
					return;
				}
				throw new RemotingException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Remoting_Lifetime_InitialStateInitialLeaseTime"), state.ToString()));
			}
		}

		public TimeSpan CurrentLeaseTime => leaseTime.Subtract(DateTime.UtcNow);

		public LeaseState CurrentState => state;

		internal Lease(TimeSpan initialLeaseTime, TimeSpan renewOnCallTime, TimeSpan sponsorshipTimeout, MarshalByRefObject managedObject)
		{
			id = nextId++;
			this.renewOnCallTime = renewOnCallTime;
			this.sponsorshipTimeout = sponsorshipTimeout;
			this.initialLeaseTime = initialLeaseTime;
			this.managedObject = managedObject;
			leaseManager = LeaseManager.GetLeaseManager();
			sponsorTable = new Hashtable(10);
			state = LeaseState.Initial;
		}

		internal void ActivateLease()
		{
			leaseTime = DateTime.UtcNow.Add(initialLeaseTime);
			state = LeaseState.Active;
			leaseManager.ActivateLease(this);
		}

		public override object InitializeLifetimeService()
		{
			return null;
		}

		[SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.RemotingConfiguration)]
		public void Register(ISponsor obj)
		{
			Register(obj, TimeSpan.Zero);
		}

		[SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.RemotingConfiguration)]
		public void Register(ISponsor obj, TimeSpan renewalTime)
		{
			lock (this)
			{
				if (state == LeaseState.Expired || sponsorshipTimeout == TimeSpan.Zero)
				{
					return;
				}
				object sponsorId = GetSponsorId(obj);
				lock (sponsorTable)
				{
					if (renewalTime > TimeSpan.Zero)
					{
						AddTime(renewalTime);
					}
					if (!sponsorTable.ContainsKey(sponsorId))
					{
						sponsorTable[sponsorId] = new SponsorStateInfo(renewalTime, SponsorState.Initial);
					}
				}
			}
		}

		[SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.RemotingConfiguration)]
		public void Unregister(ISponsor sponsor)
		{
			lock (this)
			{
				if (state == LeaseState.Expired)
				{
					return;
				}
				object sponsorId = GetSponsorId(sponsor);
				lock (sponsorTable)
				{
					if (sponsorId != null)
					{
						leaseManager.DeleteSponsor(sponsorId);
						_ = (SponsorStateInfo)sponsorTable[sponsorId];
						sponsorTable.Remove(sponsorId);
					}
				}
			}
		}

		private object GetSponsorId(ISponsor obj)
		{
			object result = null;
			if (obj != null)
			{
				result = ((!RemotingServices.IsTransparentProxy(obj)) ? ((object)obj) : ((object)RemotingServices.GetRealProxy(obj)));
			}
			return result;
		}

		private ISponsor GetSponsorFromId(object sponsorId)
		{
			object obj = null;
			RealProxy realProxy = sponsorId as RealProxy;
			obj = ((realProxy == null) ? sponsorId : realProxy.GetTransparentProxy());
			return (ISponsor)obj;
		}

		[SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.RemotingConfiguration)]
		public TimeSpan Renew(TimeSpan renewalTime)
		{
			return RenewInternal(renewalTime);
		}

		internal TimeSpan RenewInternal(TimeSpan renewalTime)
		{
			lock (this)
			{
				if (state == LeaseState.Expired)
				{
					return TimeSpan.Zero;
				}
				AddTime(renewalTime);
				return leaseTime.Subtract(DateTime.UtcNow);
			}
		}

		internal void Remove()
		{
			if (state != LeaseState.Expired)
			{
				state = LeaseState.Expired;
				leaseManager.DeleteLease(this);
			}
		}

		internal void Cancel()
		{
			lock (this)
			{
				if (state != LeaseState.Expired)
				{
					Remove();
					RemotingServices.Disconnect(managedObject, bResetURI: false);
					RemotingServices.Disconnect(this);
				}
			}
		}

		internal void RenewOnCall()
		{
			lock (this)
			{
				if (state != LeaseState.Initial && state != LeaseState.Expired)
				{
					AddTime(renewOnCallTime);
				}
			}
		}

		internal void LeaseExpired(DateTime now)
		{
			lock (this)
			{
				if (state != LeaseState.Expired && leaseTime.CompareTo(now) < 0)
				{
					ProcessNextSponsor();
				}
			}
		}

		internal void SponsorCall(ISponsor sponsor)
		{
			bool flag = false;
			if (state == LeaseState.Expired)
			{
				return;
			}
			lock (sponsorTable)
			{
				try
				{
					object sponsorId = GetSponsorId(sponsor);
					sponsorCallThread = Thread.CurrentThread.GetHashCode();
					AsyncRenewal asyncRenewal = sponsor.Renewal;
					SponsorStateInfo sponsorStateInfo = (SponsorStateInfo)sponsorTable[sponsorId];
					sponsorStateInfo.sponsorState = SponsorState.Waiting;
					asyncRenewal.BeginInvoke(this, SponsorCallback, null);
					if (sponsorStateInfo.sponsorState == SponsorState.Waiting && state != LeaseState.Expired)
					{
						leaseManager.RegisterSponsorCall(this, sponsorId, sponsorshipTimeout);
					}
					sponsorCallThread = 0;
				}
				catch (Exception)
				{
					flag = true;
					sponsorCallThread = 0;
				}
			}
			if (flag)
			{
				Unregister(sponsor);
				ProcessNextSponsor();
			}
		}

		internal void SponsorTimeout(object sponsorId)
		{
			lock (this)
			{
				if (!sponsorTable.ContainsKey(sponsorId))
				{
					return;
				}
				lock (sponsorTable)
				{
					SponsorStateInfo sponsorStateInfo = (SponsorStateInfo)sponsorTable[sponsorId];
					if (sponsorStateInfo.sponsorState == SponsorState.Waiting)
					{
						Unregister(GetSponsorFromId(sponsorId));
						ProcessNextSponsor();
					}
				}
			}
		}

		private void ProcessNextSponsor()
		{
			object obj = null;
			TimeSpan timeSpan = TimeSpan.Zero;
			lock (sponsorTable)
			{
				IDictionaryEnumerator enumerator = sponsorTable.GetEnumerator();
				while (enumerator.MoveNext())
				{
					object key = enumerator.Key;
					SponsorStateInfo sponsorStateInfo = (SponsorStateInfo)enumerator.Value;
					if (sponsorStateInfo.sponsorState == SponsorState.Initial && timeSpan == TimeSpan.Zero)
					{
						timeSpan = sponsorStateInfo.renewalTime;
						obj = key;
					}
					else if (sponsorStateInfo.renewalTime > timeSpan)
					{
						timeSpan = sponsorStateInfo.renewalTime;
						obj = key;
					}
				}
			}
			if (obj != null)
			{
				SponsorCall(GetSponsorFromId(obj));
			}
			else
			{
				Cancel();
			}
		}

		internal void SponsorCallback(object obj)
		{
			SponsorCallback((IAsyncResult)obj);
		}

		internal void SponsorCallback(IAsyncResult iar)
		{
			if (state == LeaseState.Expired)
			{
				return;
			}
			int hashCode = Thread.CurrentThread.GetHashCode();
			if (hashCode == sponsorCallThread)
			{
				WaitCallback callBack = SponsorCallback;
				ThreadPool.QueueUserWorkItem(callBack, iar);
				return;
			}
			AsyncResult asyncResult = (AsyncResult)iar;
			AsyncRenewal asyncRenewal = (AsyncRenewal)asyncResult.AsyncDelegate;
			ISponsor sponsor = (ISponsor)asyncRenewal.Target;
			SponsorStateInfo sponsorStateInfo = null;
			if (iar.IsCompleted)
			{
				bool flag = false;
				TimeSpan renewalTime = TimeSpan.Zero;
				try
				{
					renewalTime = asyncRenewal.EndInvoke(iar);
				}
				catch (Exception)
				{
					flag = true;
				}
				if (flag)
				{
					Unregister(sponsor);
					ProcessNextSponsor();
					return;
				}
				object sponsorId = GetSponsorId(sponsor);
				lock (sponsorTable)
				{
					if (sponsorTable.ContainsKey(sponsorId))
					{
						sponsorStateInfo = (SponsorStateInfo)sponsorTable[sponsorId];
						sponsorStateInfo.sponsorState = SponsorState.Completed;
						sponsorStateInfo.renewalTime = renewalTime;
					}
				}
				if (sponsorStateInfo == null)
				{
					ProcessNextSponsor();
				}
				else if (sponsorStateInfo.renewalTime == TimeSpan.Zero)
				{
					Unregister(sponsor);
					ProcessNextSponsor();
				}
				else
				{
					RenewInternal(sponsorStateInfo.renewalTime);
				}
			}
			else
			{
				Unregister(sponsor);
				ProcessNextSponsor();
			}
		}

		private void AddTime(TimeSpan renewalSpan)
		{
			if (state != LeaseState.Expired)
			{
				DateTime dateTime = DateTime.UtcNow.Add(renewalSpan);
				if (leaseTime.CompareTo(dateTime) < 0)
				{
					leaseManager.ChangedLeaseTime(this, dateTime);
					leaseTime = dateTime;
					state = LeaseState.Active;
				}
			}
		}
	}
}
