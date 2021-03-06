namespace System.Net.Sockets
{
	public enum SocketOptionName
	{
		Debug = 1,
		AcceptConnection = 2,
		ReuseAddress = 4,
		KeepAlive = 8,
		DontRoute = 0x10,
		Broadcast = 0x20,
		UseLoopback = 0x40,
		Linger = 0x80,
		OutOfBandInline = 0x100,
		DontLinger = -129,
		ExclusiveAddressUse = -5,
		SendBuffer = 4097,
		ReceiveBuffer = 4098,
		SendLowWater = 4099,
		ReceiveLowWater = 4100,
		SendTimeout = 4101,
		ReceiveTimeout = 4102,
		Error = 4103,
		Type = 4104,
		MaxConnections = int.MaxValue,
		IPOptions = 1,
		HeaderIncluded = 2,
		TypeOfService = 3,
		IpTimeToLive = 4,
		MulticastInterface = 9,
		MulticastTimeToLive = 10,
		MulticastLoopback = 11,
		AddMembership = 12,
		DropMembership = 13,
		DontFragment = 14,
		AddSourceMembership = 0xF,
		DropSourceMembership = 0x10,
		BlockSource = 17,
		UnblockSource = 18,
		PacketInformation = 19,
		HopLimit = 21,
		NoDelay = 1,
		BsdUrgent = 2,
		Expedited = 2,
		NoChecksum = 1,
		ChecksumCoverage = 20,
		UpdateAcceptContext = 28683,
		UpdateConnectContext = 28688
	}
}
