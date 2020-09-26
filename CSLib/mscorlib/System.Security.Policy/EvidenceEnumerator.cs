using System.Collections;

namespace System.Security.Policy
{
	internal sealed class EvidenceEnumerator : IEnumerator
	{
		private bool m_first;

		private Evidence m_evidence;

		private IEnumerator m_enumerator;

		public object Current
		{
			get
			{
				if (m_enumerator == null)
				{
					return null;
				}
				return m_enumerator.Current;
			}
		}

		public EvidenceEnumerator(Evidence evidence)
		{
			m_evidence = evidence;
			Reset();
		}

		public bool MoveNext()
		{
			if (m_enumerator == null)
			{
				return false;
			}
			if (!m_enumerator.MoveNext())
			{
				if (m_first)
				{
					m_enumerator = m_evidence.GetAssemblyEnumerator();
					m_first = false;
					if (m_enumerator != null)
					{
						return m_enumerator.MoveNext();
					}
					return false;
				}
				return false;
			}
			return true;
		}

		public void Reset()
		{
			m_first = true;
			if (m_evidence != null)
			{
				m_enumerator = m_evidence.GetHostEnumerator();
			}
			else
			{
				m_enumerator = null;
			}
		}
	}
}
