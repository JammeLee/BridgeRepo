using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;

namespace System.Reflection
{
	[Serializable]
	internal sealed class CerArrayList<V>
	{
		private const int MinSize = 4;

		private V[] m_array;

		private int m_count;

		internal int Count
		{
			[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
			get
			{
				return m_count;
			}
		}

		internal V this[int index]
		{
			[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
			get
			{
				return m_array[index];
			}
		}

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		internal CerArrayList(List<V> list)
		{
			m_array = new V[list.Count];
			for (int i = 0; i < list.Count; i++)
			{
				m_array[i] = list[i];
			}
			m_count = list.Count;
		}

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		internal CerArrayList(int length)
		{
			if (length < 4)
			{
				length = 4;
			}
			m_array = new V[length];
			m_count = 0;
		}

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		internal void Preallocate(int addition)
		{
			if (m_array.Length - m_count <= addition)
			{
				int num = ((m_array.Length * 2 > m_array.Length + addition) ? (m_array.Length * 2) : (m_array.Length + addition));
				V[] array = new V[num];
				for (int i = 0; i < m_count; i++)
				{
					array[i] = m_array[i];
				}
				m_array = array;
			}
		}

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		internal void Add(V value)
		{
			m_array[m_count] = value;
			m_count++;
		}

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		internal void Replace(int index, V value)
		{
			if (index >= m_count)
			{
				throw new InvalidOperationException();
			}
			m_array[index] = value;
		}
	}
}
