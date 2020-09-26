using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Threading;

namespace System.Reflection
{
	[Serializable]
	internal sealed class CerHashtable<K, V>
	{
		private const int MinSize = 7;

		private K[] m_key;

		private V[] m_value;

		private int m_count;

		internal V this[K key]
		{
			[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
			get
			{
				bool tookLock = false;
				RuntimeHelpers.PrepareConstrainedRegions();
				try
				{
					Monitor.ReliableEnter(this, ref tookLock);
					int num = key.GetHashCode();
					if (num < 0)
					{
						num = -num;
					}
					int num2 = num % m_key.Length;
					int num3 = num2;
					while (true)
					{
						K val = m_key[num3];
						if (val == null)
						{
							break;
						}
						if (val.Equals(key))
						{
							return m_value[num3];
						}
						num3++;
						num3 %= m_key.Length;
					}
					return default(V);
				}
				finally
				{
					if (tookLock)
					{
						Monitor.Exit(this);
					}
				}
			}
			[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
			set
			{
				bool tookLock = false;
				RuntimeHelpers.PrepareConstrainedRegions();
				try
				{
					Monitor.ReliableEnter(this, ref tookLock);
					Insert(m_key, m_value, ref m_count, key, value);
				}
				finally
				{
					if (tookLock)
					{
						Monitor.Exit(this);
					}
				}
			}
		}

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		internal CerHashtable()
			: this(7)
		{
		}

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		internal CerHashtable(int size)
		{
			size = HashHelpers.GetPrime(size);
			m_key = new K[size];
			m_value = new V[size];
		}

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		internal void Preallocate(int count)
		{
			bool tookLock = false;
			bool flag = false;
			K[] array = null;
			V[] array2 = null;
			RuntimeHelpers.PrepareConstrainedRegions();
			try
			{
				Monitor.ReliableEnter(this, ref tookLock);
				int num = (count + m_count) * 2;
				if (num < m_value.Length)
				{
					return;
				}
				num = HashHelpers.GetPrime(num);
				array = new K[num];
				array2 = new V[num];
				for (int i = 0; i < m_key.Length; i++)
				{
					K val = m_key[i];
					if (val != null)
					{
						int count2 = 0;
						Insert(array, array2, ref count2, val, m_value[i]);
					}
				}
				flag = true;
			}
			finally
			{
				if (flag)
				{
					m_key = array;
					m_value = array2;
				}
				if (tookLock)
				{
					Monitor.Exit(this);
				}
			}
		}

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		private static void Insert(K[] keys, V[] values, ref int count, K key, V value)
		{
			int num = key.GetHashCode();
			if (num < 0)
			{
				num = -num;
			}
			int num2 = num % keys.Length;
			int num3 = num2;
			K val;
			while (true)
			{
				val = keys[num3];
				if (val == null)
				{
					RuntimeHelpers.PrepareConstrainedRegions();
					try
					{
					}
					finally
					{
						keys[num3] = key;
						values[num3] = value;
						count++;
					}
					return;
				}
				if (val.Equals(key))
				{
					break;
				}
				num3++;
				num3 %= keys.Length;
			}
			throw new ArgumentException(Environment.GetResourceString("Argument_AddingDuplicate__", val, key));
		}
	}
}
