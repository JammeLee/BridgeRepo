namespace System.Net.NetworkInformation
{
	internal class SystemIPv6InterfaceProperties : IPv6InterfaceProperties
	{
		private uint index;

		private uint mtu;

		public override int Index => (int)index;

		public override int Mtu => (int)mtu;

		internal SystemIPv6InterfaceProperties(uint index, uint mtu)
		{
			this.index = index;
			this.mtu = mtu;
		}
	}
}
