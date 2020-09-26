using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace System.Net.NetworkInformation
{
	public class IPAddressInformationCollection : ICollection<IPAddressInformation>, IEnumerable<IPAddressInformation>, IEnumerable
	{
		private Collection<IPAddressInformation> addresses = new Collection<IPAddressInformation>();

		public virtual int Count => addresses.Count;

		public virtual bool IsReadOnly => true;

		public virtual IPAddressInformation this[int index] => addresses[index];

		internal IPAddressInformationCollection()
		{
		}

		public virtual void CopyTo(IPAddressInformation[] array, int offset)
		{
			addresses.CopyTo(array, offset);
		}

		public virtual void Add(IPAddressInformation address)
		{
			throw new NotSupportedException(SR.GetString("net_collection_readonly"));
		}

		internal void InternalAdd(IPAddressInformation address)
		{
			addresses.Add(address);
		}

		public virtual bool Contains(IPAddressInformation address)
		{
			return addresses.Contains(address);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return null;
		}

		public virtual IEnumerator<IPAddressInformation> GetEnumerator()
		{
			return addresses.GetEnumerator();
		}

		public virtual bool Remove(IPAddressInformation address)
		{
			throw new NotSupportedException(SR.GetString("net_collection_readonly"));
		}

		public virtual void Clear()
		{
			throw new NotSupportedException(SR.GetString("net_collection_readonly"));
		}
	}
}
