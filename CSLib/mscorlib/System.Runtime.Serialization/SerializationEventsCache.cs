using System.Collections;

namespace System.Runtime.Serialization
{
	internal static class SerializationEventsCache
	{
		private static Hashtable cache = new Hashtable();

		internal static SerializationEvents GetSerializationEventsForType(Type t)
		{
			SerializationEvents result;
			if ((result = (SerializationEvents)cache[t]) == null)
			{
				lock (cache.SyncRoot)
				{
					if ((result = (SerializationEvents)cache[t]) != null)
					{
						return result;
					}
					result = new SerializationEvents(t);
					cache[t] = result;
					return result;
				}
			}
			return result;
		}
	}
}
