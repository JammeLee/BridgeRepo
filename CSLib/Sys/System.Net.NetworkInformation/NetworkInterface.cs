namespace System.Net.NetworkInformation
{
	public abstract class NetworkInterface
	{
		public static int LoopbackInterfaceIndex => SystemNetworkInterface.InternalLoopbackInterfaceIndex;

		public abstract string Id
		{
			get;
		}

		public abstract string Name
		{
			get;
		}

		public abstract string Description
		{
			get;
		}

		public abstract OperationalStatus OperationalStatus
		{
			get;
		}

		public abstract long Speed
		{
			get;
		}

		public abstract bool IsReceiveOnly
		{
			get;
		}

		public abstract bool SupportsMulticast
		{
			get;
		}

		public abstract NetworkInterfaceType NetworkInterfaceType
		{
			get;
		}

		public static NetworkInterface[] GetAllNetworkInterfaces()
		{
			new NetworkInformationPermission(NetworkInformationAccess.Read).Demand();
			return SystemNetworkInterface.GetNetworkInterfaces();
		}

		public static bool GetIsNetworkAvailable()
		{
			return SystemNetworkInterface.InternalGetIsNetworkAvailable();
		}

		public abstract IPInterfaceProperties GetIPProperties();

		public abstract IPv4InterfaceStatistics GetIPv4Statistics();

		public abstract PhysicalAddress GetPhysicalAddress();

		public abstract bool Supports(NetworkInterfaceComponent networkInterfaceComponent);
	}
}
