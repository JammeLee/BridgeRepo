using System;
using System.IO;

namespace CSLib.Utility
{
	public class CLogInfoList : CSingleton<CLogInfoList>
	{
		private CCollectionContainerListType<CLogInfoItem> m_ᜀ;

		[NonSerialized]
		private LogInfoCallback m_ᜁ;

		private int ᜂ = 5;

		private int ᜃ = 50;

		public int Count => this.m_ᜀ.Count;

		public LogInfoCallback LogInfoCallbackFunc
		{
			set
			{
				this.m_ᜁ = value.Invoke;
			}
		}

		public int MaxLogCount
		{
			get
			{
				return ᜃ;
			}
			set
			{
				ᜃ = value;
			}
		}

		public CLogInfoList()
		{
			this.m_ᜀ = new CCollectionContainerListType<CLogInfoItem>();
		}

		private void ᜀ(CLogInfoItem A_0)
		{
			if (this.m_ᜁ != null)
			{
				this.m_ᜁ(A_0);
			}
		}

		public void Serizlize()
		{
			//Discarded unreachable code: IL_0082
			int a_ = 9;
			try
			{
				switch (0)
				{
				}
				while (true)
				{
					string currentApplicationPath = CPathHelper.GetCurrentApplicationPath();
					FileInfo[] files = new DirectoryInfo(currentApplicationPath).GetFiles(CConstant.STRING_LOG_FILE_FILTER, SearchOption.TopDirectoryOnly);
					string filename = "";
					int num = 0;
					while (true)
					{
						int num2;
						int num3;
						switch (num)
						{
						case 0:
							if (files.Length < ᜂ)
							{
								num = 2;
								continue;
							}
							filename = CIOHelper.GetOldestFile(files);
							num = 1;
							continue;
						case 2:
							if (true)
							{
							}
							num = 7;
							continue;
						case 8:
							num = 6;
							continue;
						case 6:
							num2 = files.Length - 1;
							goto IL_00de;
						case 7:
							num = ((files.Length == 0) ? 5 : 8);
							continue;
						case 5:
							num2 = 0;
							goto IL_00de;
						case 1:
						case 3:
							CSingleton<CSerizlizeHelper>.Instance.Serizlize(filename, this.m_ᜀ);
							num = 4;
							continue;
						case 4:
							return;
							IL_00de:
							num3 = num2;
							filename = currentApplicationPath + CSimpleThreadPool.b("᥄", a_) + CConstant.STRING_LOG_FILE + num3 + CSimpleThreadPool.b("歄㽆⑈❊", a_);
							num = 3;
							continue;
						}
						break;
					}
				}
			}
			catch (Exception ex)
			{
				CSingleton<CLogInfoList>.Instance.WriteLine(ex);
			}
		}

		public void Serizlize(string filename)
		{
			//Discarded unreachable code: IL_0025
			try
			{
				CSingleton<CSerizlizeHelper>.Instance.Serizlize(filename, this.m_ᜀ);
			}
			catch (Exception ex)
			{
				CSingleton<CLogInfoList>.Instance.WriteLine(ex);
			}
			if (1 == 0)
			{
			}
		}

		public void Clear()
		{
			this.m_ᜀ.Clear();
		}

		public void WriteLine(string message)
		{
			//Discarded unreachable code: IL_0003
			if (true)
			{
			}
			ᜁ();
			CLogInfoItem cLogInfoItem = new CLogInfoItem();
			cLogInfoItem.TimeStame = DateTime.Now.ToString();
			cLogInfoItem.Message = message;
			this.m_ᜀ.Add(cLogInfoItem);
			ᜀ(cLogInfoItem);
		}

		public void WriteLine(string message, ELogType logType)
		{
			//Discarded unreachable code: IL_0003
			if (true)
			{
			}
			ᜁ();
			CLogInfoItem cLogInfoItem = new CLogInfoItem();
			cLogInfoItem.TimeStame = DateTime.Now.ToString();
			cLogInfoItem.Message = message;
			cLogInfoItem.LogType = logType;
			this.m_ᜀ.Add(cLogInfoItem);
			ᜀ(cLogInfoItem);
		}

		public void WriteLine(Exception ex)
		{
			//Discarded unreachable code: IL_0003
			if (true)
			{
			}
			ᜁ();
			CLogInfoItem cLogInfoItem = new CLogInfoItem();
			cLogInfoItem.TimeStame = DateTime.Now.ToString();
			cLogInfoItem.Message = ex.Message;
			cLogInfoItem.ExtMessage = ex.StackTrace + CConstant.INFO_RETURN;
			cLogInfoItem.LogType = ELogType.E_LOG_TYPE_ERROR;
			this.m_ᜀ.Add(cLogInfoItem);
			ᜀ(cLogInfoItem);
		}

		private void ᜁ()
		{
			if (this.m_ᜀ.Count > ᜃ)
			{
				this.m_ᜀ.Clear();
			}
		}

		public void SetNullCallback()
		{
			this.m_ᜁ = null;
		}
	}
}
