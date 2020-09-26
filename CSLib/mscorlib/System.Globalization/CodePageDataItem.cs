namespace System.Globalization
{
	[Serializable]
	internal class CodePageDataItem
	{
		internal int m_dataIndex;

		internal int m_codePage;

		internal int m_uiFamilyCodePage;

		internal string m_webName;

		internal string m_headerName;

		internal string m_bodyName;

		internal string m_description;

		internal uint m_flags;

		public unsafe virtual string WebName
		{
			get
			{
				if (m_webName == null)
				{
					m_webName = new string(EncodingTable.codePageDataPtr[m_dataIndex].webName);
				}
				return m_webName;
			}
		}

		public virtual int UIFamilyCodePage => m_uiFamilyCodePage;

		public unsafe virtual string HeaderName
		{
			get
			{
				if (m_headerName == null)
				{
					m_headerName = new string(EncodingTable.codePageDataPtr[m_dataIndex].headerName);
				}
				return m_headerName;
			}
		}

		public unsafe virtual string BodyName
		{
			get
			{
				if (m_bodyName == null)
				{
					m_bodyName = new string(EncodingTable.codePageDataPtr[m_dataIndex].bodyName);
				}
				return m_bodyName;
			}
		}

		public virtual uint Flags => m_flags;

		internal unsafe CodePageDataItem(int dataIndex)
		{
			m_dataIndex = dataIndex;
			m_codePage = 0;
			m_uiFamilyCodePage = EncodingTable.codePageDataPtr[dataIndex].uiFamilyCodePage;
			m_webName = null;
			m_headerName = null;
			m_bodyName = null;
			m_description = null;
			m_flags = EncodingTable.codePageDataPtr[dataIndex].flags;
		}
	}
}
