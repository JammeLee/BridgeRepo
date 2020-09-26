using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace System.Net.NetworkInformation
{
	public class GatewayIPAddressInformationCollection : ICollection<GatewayIPAddressInformation>, IEnumerable<GatewayIPAddressInformation>, IEnumerable
	{
		private Collection<GatewayIPAddressInformation> addresses = new Collection<GatewayIPAddressInformation>();

		public virtual int Count => addresses.Count;

		public virtual bool IsReadOnly => true;

		public virtual GatewayIPAddressInformation this[int index] => addresses[index];

		protected internal GatewayIPAddressInformationCollection()
		{
		}

		public virtual void CopyTo(GatewayIPAddressInformation[] array, int offset)
		{
			addresses.CopyTo(array, offset);
		}

		public virtual void Add(GatewayIPAddressInformation address)
		{
			throw new NotSupportedException(SR.GetString("net_collection_readonly"));
		}

		internal void InternalAdd(GatewayIPAddressInformation address)
		{
			addresses.Add(address);
		}

		public virtual bool Contains(GatewayIPAddressInformation address)
		{
			return addresses.Contains(address);
		}

		public virtual IEnumerator<GatewayIPAddressInformation> GetEnumerator()
		{
			return addresses.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return null;
		}

		public virtual bool Remove(GatewayIPAddressInformation address)
		{
			throw new NotSupportedException(SR.GetString("net_collection_readonly"));
		}

		public virtual void Clear()
		{
			throw new NotSupportedException(SR.GetString("net_collection_readonly"));
		}
	}
}
