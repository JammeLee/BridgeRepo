namespace System.Net.NetworkInformation
{
	internal class SystemIPv4InterfaceStatistics : IPv4InterfaceStatistics
	{
		private MibIfRow ifRow = default(MibIfRow);

		public override long OutputQueueLength => ifRow.dwOutQLen;

		public override long BytesSent => ifRow.dwOutOctets;

		public override long BytesReceived => ifRow.dwInOctets;

		public override long UnicastPacketsSent => ifRow.dwOutUcastPkts;

		public override long UnicastPacketsReceived => ifRow.dwInUcastPkts;

		public override long NonUnicastPacketsSent => ifRow.dwOutNUcastPkts;

		public override long NonUnicastPacketsReceived => ifRow.dwInNUcastPkts;

		public override long IncomingPacketsDiscarded => ifRow.dwInDiscards;

		public override long OutgoingPacketsDiscarded => ifRow.dwOutDiscards;

		public override long IncomingPacketsWithErrors => ifRow.dwInErrors;

		public override long OutgoingPacketsWithErrors => ifRow.dwOutErrors;

		public override long IncomingUnknownProtocolPackets => ifRow.dwInUnknownProtos;

		internal long Mtu => ifRow.dwMtu;

		internal OperationalStatus OperationalStatus => ifRow.operStatus switch
		{
			OldOperationalStatus.NonOperational => OperationalStatus.Down, 
			OldOperationalStatus.Unreachable => OperationalStatus.Down, 
			OldOperationalStatus.Disconnected => OperationalStatus.Dormant, 
			OldOperationalStatus.Connecting => OperationalStatus.Dormant, 
			OldOperationalStatus.Connected => OperationalStatus.Up, 
			OldOperationalStatus.Operational => OperationalStatus.Up, 
			_ => OperationalStatus.Unknown, 
		};

		internal long Speed => ifRow.dwSpeed;

		private SystemIPv4InterfaceStatistics()
		{
		}

		internal SystemIPv4InterfaceStatistics(long index)
		{
			GetIfEntry(index);
		}

		private void GetIfEntry(long index)
		{
			if (index != 0)
			{
				ifRow.dwIndex = (uint)index;
				uint ifEntry = UnsafeNetInfoNativeMethods.GetIfEntry(ref ifRow);
				if (ifEntry != 0)
				{
					throw new NetworkInformationException((int)ifEntry);
				}
			}
		}
	}
}
