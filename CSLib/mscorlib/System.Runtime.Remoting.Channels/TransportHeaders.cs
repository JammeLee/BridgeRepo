using System.Collections;
using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace System.Runtime.Remoting.Channels
{
	[Serializable]
	[ComVisible(true)]
	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
	[SecurityPermission(SecurityAction.InheritanceDemand, Flags = SecurityPermissionFlag.Infrastructure)]
	public class TransportHeaders : ITransportHeaders
	{
		private ArrayList _headerList;

		public object this[object key]
		{
			get
			{
				string strB = (string)key;
				foreach (DictionaryEntry header in _headerList)
				{
					if (string.Compare((string)header.Key, strB, StringComparison.OrdinalIgnoreCase) == 0)
					{
						return header.Value;
					}
				}
				return null;
			}
			set
			{
				if (key == null)
				{
					return;
				}
				string strB = (string)key;
				for (int num = _headerList.Count - 1; num >= 0; num--)
				{
					string strA = (string)((DictionaryEntry)_headerList[num]).Key;
					if (string.Compare(strA, strB, StringComparison.OrdinalIgnoreCase) == 0)
					{
						_headerList.RemoveAt(num);
						break;
					}
				}
				if (value != null)
				{
					_headerList.Add(new DictionaryEntry(key, value));
				}
			}
		}

		public TransportHeaders()
		{
			_headerList = new ArrayList(6);
		}

		public IEnumerator GetEnumerator()
		{
			return _headerList.GetEnumerator();
		}
	}
}
