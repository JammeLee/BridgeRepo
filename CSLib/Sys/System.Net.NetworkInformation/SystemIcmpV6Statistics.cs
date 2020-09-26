using System.Net.Sockets;

namespace System.Net.NetworkInformation
{
	internal class SystemIcmpV6Statistics : IcmpV6Statistics
	{
		private MibIcmpInfoEx stats;

		public override long MessagesSent => stats.outStats.dwMsgs;

		public override long MessagesReceived => stats.inStats.dwMsgs;

		public override long ErrorsSent => stats.outStats.dwErrors;

		public override long ErrorsReceived => stats.inStats.dwErrors;

		public override long DestinationUnreachableMessagesSent => stats.outStats.rgdwTypeCount[1L];

		public override long DestinationUnreachableMessagesReceived => stats.inStats.rgdwTypeCount[1L];

		public override long PacketTooBigMessagesSent => stats.outStats.rgdwTypeCount[2L];

		public override long PacketTooBigMessagesReceived => stats.inStats.rgdwTypeCount[2L];

		public override long TimeExceededMessagesSent => stats.outStats.rgdwTypeCount[3L];

		public override long TimeExceededMessagesReceived => stats.inStats.rgdwTypeCount[3L];

		public override long ParameterProblemsSent => stats.outStats.rgdwTypeCount[4L];

		public override long ParameterProblemsReceived => stats.inStats.rgdwTypeCount[4L];

		public override long EchoRequestsSent => stats.outStats.rgdwTypeCount[128L];

		public override long EchoRequestsReceived => stats.inStats.rgdwTypeCount[128L];

		public override long EchoRepliesSent => stats.outStats.rgdwTypeCount[129L];

		public override long EchoRepliesReceived => stats.inStats.rgdwTypeCount[129L];

		public override long MembershipQueriesSent => stats.outStats.rgdwTypeCount[130L];

		public override long MembershipQueriesReceived => stats.inStats.rgdwTypeCount[130L];

		public override long MembershipReportsSent => stats.outStats.rgdwTypeCount[131L];

		public override long MembershipReportsReceived => stats.inStats.rgdwTypeCount[131L];

		public override long MembershipReductionsSent => stats.outStats.rgdwTypeCount[132L];

		public override long MembershipReductionsReceived => stats.inStats.rgdwTypeCount[132L];

		public override long RouterAdvertisementsSent => stats.outStats.rgdwTypeCount[134L];

		public override long RouterAdvertisementsReceived => stats.inStats.rgdwTypeCount[134L];

		public override long RouterSolicitsSent => stats.outStats.rgdwTypeCount[133L];

		public override long RouterSolicitsReceived => stats.inStats.rgdwTypeCount[133L];

		public override long NeighborAdvertisementsSent => stats.outStats.rgdwTypeCount[136L];

		public override long NeighborAdvertisementsReceived => stats.inStats.rgdwTypeCount[136L];

		public override long NeighborSolicitsSent => stats.outStats.rgdwTypeCount[135L];

		public override long NeighborSolicitsReceived => stats.inStats.rgdwTypeCount[135L];

		public override long RedirectsSent => stats.outStats.rgdwTypeCount[137L];

		public override long RedirectsReceived => stats.inStats.rgdwTypeCount[137L];

		internal SystemIcmpV6Statistics()
		{
			if (!ComNetOS.IsPostWin2K)
			{
				throw new PlatformNotSupportedException(SR.GetString("WinXPRequired"));
			}
			uint icmpStatisticsEx = UnsafeNetInfoNativeMethods.GetIcmpStatisticsEx(out stats, AddressFamily.InterNetworkV6);
			if (icmpStatisticsEx != 0)
			{
				throw new NetworkInformationException((int)icmpStatisticsEx);
			}
		}
	}
}
