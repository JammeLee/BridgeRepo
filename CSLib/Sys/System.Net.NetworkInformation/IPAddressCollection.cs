using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace System.Net.NetworkInformation
{
	public class IPAddressCollection : ICollection<IPAddress>, IEnumerable<IPAddress>, IEnumerable
	{
		private Collection<IPAddress> addresses = new Collection<IPAddress>();

		public virtual int Count => addresses.Count;

		public virtual bool IsReadOnly => true;

		public virtual IPAddress this[int index] => addresses[index];

		protected internal IPAddressCollection()
		{
		}

		public virtual void CopyTo(IPAddress[] array, int offset)
		{
			addresses.CopyTo(array, offset);
		}

		public virtual void Add(IPAddress address)
		{
			throw new NotSupportedException(SR.GetString("net_collection_readonly"));
		}

		internal void InternalAdd(IPAddress address)
		{
			addresses.Add(address);
		}

		public virtual bool Contains(IPAddress address)
		{
			return addresses.Contains(address);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return null;
		}

		public virtual IEnumerator<IPAddress> GetEnumerator()
		{
			return addresses.GetEnumerator();
		}

		public virtual bool Remove(IPAddress address)
		{
			throw new NotSupportedException(SR.GetString("net_collection_readonly"));
		}

		public virtual void Clear()
		{
			throw new NotSupportedException(SR.GetString("net_collection_readonly"));
		}
	}
}
