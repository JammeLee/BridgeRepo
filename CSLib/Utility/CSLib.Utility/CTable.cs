using System;
using System.Collections.Generic;
using System.IO;

namespace CSLib.Utility
{
	public abstract class CTable
	{
		private List<Action> ᜀ = new List<Action>();

		protected bool m_bLoaded;

		public bool IsLoaded => m_bLoaded;

		public bool LoadTable(string pathName, string fileName, Action cbFunc)
		{
			//Discarded unreachable code: IL_008e
			int a_ = 3;
			int num = 11;
			string extension = default(string);
			CCsvExcel cCsvExcel = default(CCsvExcel);
			CXmlExcel cXmlExcel = default(CXmlExcel);
			while (true)
			{
				switch (num)
				{
				case 9:
					if (extension.Equals(CSimpleThreadPool.b("ᄾ≀あ㍄", a_), StringComparison.CurrentCultureIgnoreCase))
					{
						num = 8;
						continue;
					}
					return _PostLoadTable(fileName);
				case 3:
					if (true)
					{
					}
					return false;
				case 7:
					if (cbFunc != null)
					{
						num = 2;
						continue;
					}
					goto case 4;
				case 6:
					return false;
				case 1:
					return true;
				case 8:
					cCsvExcel = new CCsvExcel();
					num = 10;
					continue;
				case 10:
					if (!cCsvExcel.LoadFile(fileName))
					{
						num = 6;
						continue;
					}
					return _PostLoadTable(cCsvExcel);
				case 12:
					cXmlExcel = new CXmlExcel();
					num = 5;
					continue;
				case 5:
					if (!cXmlExcel.LoadFile(fileName))
					{
						num = 3;
						continue;
					}
					return _PostLoadTable(cXmlExcel);
				case 2:
					ᜀ.Add(cbFunc);
					num = 4;
					continue;
				case 4:
					extension = Path.GetExtension(fileName);
					num = 0;
					continue;
				case 0:
					num = (extension.Equals(CSimpleThreadPool.b("ᄾ㥀⹂⥄", a_), StringComparison.CurrentCultureIgnoreCase) ? 12 : 9);
					continue;
				}
				if (m_bLoaded)
				{
					num = 1;
					continue;
				}
				fileName = Path.Combine(pathName, fileName);
				num = 7;
			}
		}

		public bool LoadTable(string fileName, Action cbFunc)
		{
			//Discarded unreachable code: IL_0090
			int a_ = 10;
			int num = 9;
			string extension = default(string);
			CCsvExcel cCsvExcel = default(CCsvExcel);
			CXmlExcel cXmlExcel = default(CXmlExcel);
			while (true)
			{
				switch (num)
				{
				case 4:
					if (extension.Equals(CSimpleThreadPool.b("桅⭇㥉㩋", a_), StringComparison.CurrentCultureIgnoreCase))
					{
						num = 1;
						continue;
					}
					return _PostLoadTable(fileName);
				case 3:
					return false;
				case 11:
					if (cbFunc != null)
					{
						num = 2;
						continue;
					}
					goto case 8;
				case 7:
					return false;
				case 0:
					return true;
				case 1:
					cCsvExcel = new CCsvExcel();
					num = 12;
					continue;
				case 12:
					if (!cCsvExcel.LoadFile(fileName))
					{
						num = 7;
						continue;
					}
					return _PostLoadTable(cCsvExcel);
				case 10:
					cXmlExcel = new CXmlExcel();
					num = 6;
					continue;
				case 6:
					if (!cXmlExcel.LoadFile(fileName))
					{
						num = 3;
						continue;
					}
					return _PostLoadTable(cXmlExcel);
				case 2:
					ᜀ.Add(cbFunc);
					num = 8;
					continue;
				case 8:
					extension = Path.GetExtension(fileName);
					num = 5;
					continue;
				case 5:
					num = (extension.Equals(CSimpleThreadPool.b("桅ぇ❉⁋", a_), StringComparison.CurrentCultureIgnoreCase) ? 10 : 4);
					continue;
				}
				if (m_bLoaded)
				{
					num = 0;
					continue;
				}
				if (true)
				{
				}
				num = 11;
			}
		}

		protected virtual bool _PostLoadTable(CXmlExcel xmlExcel)
		{
			return false;
		}

		protected virtual bool _PostLoadTable(CCsvExcel csvExcel)
		{
			return false;
		}

		protected virtual bool _PostLoadTable(string fileName)
		{
			return false;
		}
	}
	public abstract class CTable<TmplID, CTemplate> : CTable
	{
		protected Dictionary<TmplID, CTemplate> m_dicTmpls = new Dictionary<TmplID, CTemplate>();

		public Dictionary<TmplID, CTemplate> Tmpls => m_dicTmpls;

		public CTemplate GetTmpl(TmplID tmplID)
		{
			//Discarded unreachable code: IL_001b
			CTemplate value = default(CTemplate);
			if (m_dicTmpls.TryGetValue(tmplID, out value))
			{
				if (true)
				{
				}
				return value;
			}
			return default(CTemplate);
		}
	}
}
