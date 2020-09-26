using System.Text;

namespace System.Collections.Generic
{
	[Serializable]
	public struct KeyValuePair<TKey, TValue>
	{
		private TKey key;

		private TValue value;

		public TKey Key => key;

		public TValue Value => value;

		public KeyValuePair(TKey key, TValue value)
		{
			this.key = key;
			this.value = value;
		}

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append('[');
			if (Key != null)
			{
				stringBuilder.Append(Key.ToString());
			}
			stringBuilder.Append(", ");
			if (Value != null)
			{
				stringBuilder.Append(Value.ToString());
			}
			stringBuilder.Append(']');
			return stringBuilder.ToString();
		}
	}
}
