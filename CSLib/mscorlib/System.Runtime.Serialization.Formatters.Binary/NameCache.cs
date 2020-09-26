using System.Collections;

namespace System.Runtime.Serialization.Formatters.Binary
{
	internal sealed class NameCache
	{
		private static Hashtable ht = new Hashtable();

		private string name;

		internal object GetCachedValue(string name)
		{
			this.name = name;
			return ht[name];
		}

		internal void SetCachedValue(object value)
		{
			ht[name] = value;
		}
	}
}
