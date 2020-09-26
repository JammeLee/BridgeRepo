using System.Collections;
using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace System.Runtime.Remoting.Channels
{
	[Serializable]
	[ComVisible(true)]
	[SecurityPermission(SecurityAction.InheritanceDemand, Flags = SecurityPermissionFlag.Infrastructure)]
	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
	public class ChannelDataStore : IChannelDataStore
	{
		private string[] _channelURIs;

		private DictionaryEntry[] _extraData;

		public string[] ChannelUris
		{
			get
			{
				return _channelURIs;
			}
			set
			{
				_channelURIs = value;
			}
		}

		public object this[object key]
		{
			get
			{
				DictionaryEntry[] extraData = _extraData;
				for (int i = 0; i < extraData.Length; i++)
				{
					DictionaryEntry dictionaryEntry = extraData[i];
					if (dictionaryEntry.Key.Equals(key))
					{
						return dictionaryEntry.Value;
					}
				}
				return null;
			}
			set
			{
				if (_extraData == null)
				{
					_extraData = new DictionaryEntry[1];
					ref DictionaryEntry reference = ref _extraData[0];
					reference = new DictionaryEntry(key, value);
					return;
				}
				int num = _extraData.Length;
				DictionaryEntry[] array = new DictionaryEntry[num + 1];
				int i;
				for (i = 0; i < num; i++)
				{
					ref DictionaryEntry reference2 = ref array[i];
					reference2 = _extraData[i];
				}
				ref DictionaryEntry reference3 = ref array[i];
				reference3 = new DictionaryEntry(key, value);
				_extraData = array;
			}
		}

		private ChannelDataStore(string[] channelUrls, DictionaryEntry[] extraData)
		{
			_channelURIs = channelUrls;
			_extraData = extraData;
		}

		public ChannelDataStore(string[] channelURIs)
		{
			_channelURIs = channelURIs;
			_extraData = null;
		}

		internal ChannelDataStore InternalShallowCopy()
		{
			return new ChannelDataStore(_channelURIs, _extraData);
		}
	}
}
