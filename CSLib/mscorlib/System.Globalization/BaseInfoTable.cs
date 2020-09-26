namespace System.Globalization
{
	internal abstract class BaseInfoTable
	{
		internal unsafe byte* m_pDataFileStart;

		protected AgileSafeNativeMemoryHandle memoryMapFile;

		protected unsafe CultureTableHeader* m_pCultureHeader;

		internal unsafe byte* m_pItemData;

		internal uint m_numItem;

		internal uint m_itemSize;

		internal unsafe ushort* m_pDataPool;

		internal bool fromAssembly;

		internal string fileName;

		protected bool m_valid = true;

		internal bool IsValid => m_valid;

		internal BaseInfoTable(string fileName, bool fromAssembly)
		{
			this.fileName = fileName;
			this.fromAssembly = fromAssembly;
			InitializeBaseInfoTablePointers(fileName, fromAssembly);
		}

		internal unsafe void InitializeBaseInfoTablePointers(string fileName, bool fromAssembly)
		{
			if (fromAssembly)
			{
				m_pDataFileStart = GlobalizationAssembly.GetGlobalizationResourceBytePtr(typeof(BaseInfoTable).Assembly, fileName);
			}
			else
			{
				memoryMapFile = new AgileSafeNativeMemoryHandle(fileName);
				if (memoryMapFile.FileSize == 0)
				{
					m_valid = false;
					return;
				}
				m_pDataFileStart = memoryMapFile.GetBytePtr();
			}
			EndianessHeader* pDataFileStart = (EndianessHeader*)m_pDataFileStart;
			m_pCultureHeader = (CultureTableHeader*)(m_pDataFileStart + (int)pDataFileStart->leOffset);
			SetDataItemPointers();
		}

		public override bool Equals(object value)
		{
			BaseInfoTable baseInfoTable = value as BaseInfoTable;
			if (baseInfoTable != null)
			{
				if (fromAssembly == baseInfoTable.fromAssembly)
				{
					return CultureInfo.InvariantCulture.CompareInfo.Compare(fileName, baseInfoTable.fileName, CompareOptions.IgnoreCase) == 0;
				}
				return false;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return fileName.GetHashCode();
		}

		internal abstract void SetDataItemPointers();

		internal unsafe string GetStringPoolString(uint offset)
		{
			char* ptr = (char*)(m_pDataPool + offset);
			if (ptr[1] == '\0')
			{
				return string.Empty;
			}
			return new string(ptr + 1, 0, *ptr);
		}

		internal unsafe string[] GetStringArray(uint iOffset)
		{
			if (iOffset == 0)
			{
				return new string[0];
			}
			ushort* ptr = m_pDataPool + iOffset;
			int num = *ptr;
			string[] array = new string[num];
			uint* ptr2 = (uint*)(ptr + 1);
			for (int i = 0; i < num; i++)
			{
				array[i] = GetStringPoolString(ptr2[i]);
			}
			return array;
		}

		internal unsafe int[][] GetWordArrayArray(uint iOffset)
		{
			if (iOffset == 0)
			{
				return new int[0][];
			}
			short* ptr = (short*)(m_pDataPool + iOffset);
			int num = *ptr;
			int[][] array = new int[num][];
			uint* ptr2 = (uint*)(ptr + 1);
			for (int i = 0; i < num; i++)
			{
				ptr = (short*)(m_pDataPool + ptr2[i]);
				int num2 = *ptr;
				ptr++;
				array[i] = new int[num2];
				for (int j = 0; j < num2; j++)
				{
					array[i][j] = ptr[j];
				}
			}
			return array;
		}

		internal unsafe int CompareStringToStringPoolStringBinary(string name, int offset)
		{
			int num = 0;
			char* ptr = (char*)(m_pDataPool + offset);
			if (ptr[1] == '\0')
			{
				if (name.Length == 0)
				{
					return 0;
				}
				return 1;
			}
			for (int i = 0; i < *ptr && i < name.Length; i++)
			{
				num = name[i] - ((ptr[i + 1] <= 'Z' && ptr[i + 1] >= 'A') ? (ptr[i + 1] + 97 - 65) : ptr[i + 1]);
				if (num != 0)
				{
					break;
				}
			}
			if (num != 0)
			{
				return num;
			}
			return name.Length - *ptr;
		}
	}
}
