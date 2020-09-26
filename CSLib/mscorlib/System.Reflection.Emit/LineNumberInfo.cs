using System.Diagnostics.SymbolStore;

namespace System.Reflection.Emit
{
	internal class LineNumberInfo
	{
		internal const int InitialSize = 16;

		internal int m_DocumentCount;

		internal REDocument[] m_Documents;

		private int m_iLastFound;

		internal LineNumberInfo()
		{
			m_DocumentCount = 0;
			m_iLastFound = 0;
		}

		internal void AddLineNumberInfo(ISymbolDocumentWriter document, int iOffset, int iStartLine, int iStartColumn, int iEndLine, int iEndColumn)
		{
			int num = FindDocument(document);
			m_Documents[num].AddLineNumberInfo(document, iOffset, iStartLine, iStartColumn, iEndLine, iEndColumn);
		}

		internal int FindDocument(ISymbolDocumentWriter document)
		{
			if (m_iLastFound < m_DocumentCount && m_Documents[m_iLastFound] == document)
			{
				return m_iLastFound;
			}
			for (int i = 0; i < m_DocumentCount; i++)
			{
				if (m_Documents[i].m_document == document)
				{
					m_iLastFound = i;
					return m_iLastFound;
				}
			}
			EnsureCapacity();
			m_iLastFound = m_DocumentCount;
			m_Documents[m_DocumentCount++] = new REDocument(document);
			return m_iLastFound;
		}

		internal void EnsureCapacity()
		{
			if (m_DocumentCount == 0)
			{
				m_Documents = new REDocument[16];
			}
			else if (m_DocumentCount == m_Documents.Length)
			{
				REDocument[] array = new REDocument[m_DocumentCount * 2];
				Array.Copy(m_Documents, array, m_DocumentCount);
				m_Documents = array;
			}
		}

		internal void EmitLineNumberInfo(ISymbolWriter symWriter)
		{
			for (int i = 0; i < m_DocumentCount; i++)
			{
				m_Documents[i].EmitLineNumberInfo(symWriter);
			}
		}
	}
}
