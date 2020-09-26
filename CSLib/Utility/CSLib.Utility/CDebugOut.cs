using System;
using System.IO;

namespace CSLib.Utility
{
	public class CDebugOut
	{
		public enum LEVEL
		{
			DEBUG,
			TRACE,
			INFOR,
			WARNING,
			ERROR,
			SYSTEM,
			STACK,
			MAX
		}

		public enum APPFORMAT
		{
			INVALID_VALUE,
			WINAPP,
			WEBAPP,
			IOSAPP
		}

		public delegate void DDisplayMsg(bool bPopUp, LEVEL iLevel, string strTitle, object strFormat, params object[] aArgs);

		private static CDebugOut m_ᜀ;

		private static string ᜁ = "";

		private static APPFORMAT ᜂ = APPFORMAT.INVALID_VALUE;

		private string ᜃ;

		private bool ᜄ;

		private bool ᜅ;

		private LEVEL ᜆ;

		private _1752 ᜇ;

		private _1752 ᜈ;

		private _1752 ᜉ;

		private DateTime ᜊ;

		private DDisplayMsg ᜋ;

		public static CDebugOut Default
		{
			get
			{
				if (CDebugOut.m_ᜀ == null)
				{
					CDebugOut.m_ᜀ = new CDebugOut();
				}
				return CDebugOut.m_ᜀ;
			}
			set
			{
				CDebugOut.m_ᜀ = value;
			}
		}

		public static string CurDirectory
		{
			get
			{
				return ᜁ;
			}
			set
			{
				ᜁ = value;
				CDirectoryInfo.Standardization(ref ᜁ);
			}
		}

		public static APPFORMAT AppFormat
		{
			get
			{
				return ᜂ;
			}
			set
			{
				ᜂ = value;
			}
		}

		public string FileName
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

		public bool ShowMsgBox
		{
			get
			{
				return ᜄ;
			}
			set
			{
				ᜄ = value;
			}
		}

		public bool WriteFile
		{
			get
			{
				return ᜅ;
			}
			set
			{
				ᜅ = value;
			}
		}

		public LEVEL Level
		{
			get
			{
				return ᜆ;
			}
			set
			{
				ᜆ = value;
			}
		}

		public DDisplayMsg DisplayLog
		{
			set
			{
				ᜋ = value;
			}
		}

		public CDebugOut()
		{
			int a_ = 17;
			ᜃ = "";
			ᜄ = true;
			ᜅ = true;
			ᜊ = DateTime.Today.AddDays(-7.0);
			base._002Ector();
			ᜃ = CSimpleThreadPool.b("\u094c⩎㝐㉒\u2054㭖ⵘ", a_);
			CurDirectory = CSimpleThreadPool.b("捌慎\u0d50ὒ㩔ざ\u0558", a_);
			AppFormat = APPFORMAT.WINAPP;
			_DeleteOverdueFiles();
			ᜋ = _DisplayMsg;
		}

		public CDebugOut(string strFileName)
		{
			int a_ = 4;
			ᜃ = "";
			ᜄ = true;
			ᜅ = true;
			ᜊ = DateTime.Today.AddDays(-7.0);
			base._002Ector();
			ᜃ = strFileName;
			CurDirectory = CSimpleThreadPool.b("渿汁ᡃ\u0a45❇ⵉ။", a_);
			AppFormat = APPFORMAT.WINAPP;
			_DeleteOverdueFiles();
			ᜋ = _DisplayMsg;
		}

		public virtual void SetLineCnt(int nCount)
		{
			//Discarded unreachable code: IL_0003
			if (true)
			{
			}
			ᜇ.ᜁ(nCount);
			ᜈ.ᜁ(nCount);
			ᜉ.ᜁ(nCount);
		}

		protected virtual void _WriteDebugFile(LEVEL iLevel, string strTitle, string strFormat)
		{
			//Discarded unreachable code: IL_00bc
			int a_ = 9;
			int num = 11;
			while (true)
			{
				switch (num)
				{
				default:
					num = (WriteFile ? 8 : 0);
					break;
				case 0:
					return;
				case 14:
					if (iLevel == LEVEL.ERROR)
					{
						num = 12;
						break;
					}
					return;
				case 1:
					ᜈ = new _1752(CurDirectory, FileName, CSimpleThreadPool.b("ቄ", a_));
					num = 9;
					break;
				case 9:
					if (1 == 0)
					{
					}
					goto IL_01c2;
				case 8:
					if (ᜇ == null)
					{
						num = 5;
						break;
					}
					goto case 3;
				case 15:
					ᜉ = new _1752(CurDirectory, FileName, CSimpleThreadPool.b("D", a_));
					num = 6;
					break;
				case 12:
					num = 4;
					break;
				case 4:
					if (ᜉ == null)
					{
						num = 15;
						break;
					}
					goto case 6;
				case 6:
					ᜉ.ᜁ(strTitle + CSimpleThreadPool.b("敄絆楈", a_) + strFormat);
					num = 13;
					break;
				case 13:
					return;
				case 10:
					num = 7;
					break;
				case 7:
					if (ᜈ == null)
					{
						num = 1;
						break;
					}
					goto IL_01c2;
				case 5:
					ᜇ = new _1752(CurDirectory, FileName, CSimpleThreadPool.b("\u0b44", a_));
					num = 3;
					break;
				case 3:
					ᜇ.ᜁ(strTitle + CSimpleThreadPool.b("敄絆楈", a_) + strFormat);
					num = 2;
					break;
				case 2:
					{
						num = ((iLevel == LEVEL.WARNING) ? 10 : 14);
						break;
					}
					IL_01c2:
					ᜈ.ᜁ(strTitle + CSimpleThreadPool.b("敄絆楈", a_) + strFormat);
					return;
				}
			}
		}

		public virtual void DisplayMsg(bool bPopUp, LEVEL iLevel, string strTitle, object strFormat, params object[] aArgs)
		{
			//Discarded unreachable code: IL_004b
			int num = 0;
			while (true)
			{
				switch (num)
				{
				case 1:
					ᜋ(bPopUp, iLevel, strTitle, strFormat, aArgs);
					num = 2;
					continue;
				case 2:
					if (1 == 0)
					{
					}
					return;
				}
				if (ᜋ != null)
				{
					num = 1;
					continue;
				}
				return;
			}
		}

		protected virtual void _DisplayMsg(bool bPopUp, LEVEL iLevel, string strTitle, object strFormat, params object[] aArgs)
		{
			//Discarded unreachable code: IL_0065
			int a_ = 7;
			int num = 0;
			while (true)
			{
				switch (num)
				{
				default:
					if (aArgs != null)
					{
						num = 4;
						continue;
					}
					break;
				case 3:
					strFormat = string.Format(strFormat.ToString(), aArgs);
					num = 2;
					continue;
				case 4:
					num = 1;
					continue;
				case 1:
					if (aArgs.Length != 0)
					{
						if (true)
						{
						}
						num = 3;
						continue;
					}
					break;
				case 2:
					break;
				}
				break;
			}
			CConsole.WriteLine(iLevel, strTitle + CSimpleThreadPool.b("捂罄杆", a_) + strFormat);
			_WriteDebugFile(iLevel, strTitle, strFormat.ToString());
		}

		public static void Log(object strFormat)
		{
			//Discarded unreachable code: IL_003f
			int a_ = 10;
			int num = 0;
			while (true)
			{
				switch (num)
				{
				default:
					if (Default.Level <= LEVEL.DEBUG)
					{
						num = 1;
						break;
					}
					return;
				case 1:
					if (true)
					{
					}
					Default.DisplayMsg(false, LEVEL.DEBUG, CSimpleThreadPool.b("Ʌⵇ⡉㥋⥍", a_), strFormat);
					num = 2;
					break;
				case 2:
					return;
				}
			}
		}

		public static void Log(object strFormat, params object[] aArgs)
		{
			//Discarded unreachable code: IL_0028
			int a_ = 9;
			int num = 2;
			while (true)
			{
				switch (num)
				{
				case 1:
					Default.DisplayMsg(bPopUp: false, LEVEL.DEBUG, CSimpleThreadPool.b("ń≆⭈㹊⩌", a_), strFormat, aArgs);
					num = 0;
					continue;
				case 0:
					return;
				}
				if (true)
				{
				}
				if (Default.Level <= LEVEL.DEBUG)
				{
					num = 1;
					continue;
				}
				return;
			}
		}

		public static void LogTrace(object strFormat)
		{
			//Discarded unreachable code: IL_0028
			int a_ = 10;
			int num = 0;
			while (true)
			{
				switch (num)
				{
				case 2:
					Default.DisplayMsg(false, LEVEL.TRACE, CSimpleThreadPool.b("ቅ㩇⭉⽋⭍", a_), strFormat);
					num = 1;
					continue;
				case 1:
					return;
				}
				if (true)
				{
				}
				if (Default.Level <= LEVEL.TRACE)
				{
					num = 2;
					continue;
				}
				return;
			}
		}

		public static void LogTrace(object strFormat, params object[] aArgs)
		{
			//Discarded unreachable code: IL_003d
			int a_ = 3;
			int num = 2;
			while (true)
			{
				switch (num)
				{
				default:
					if (Default.Level <= LEVEL.TRACE)
					{
						num = 0;
						break;
					}
					return;
				case 0:
					if (true)
					{
					}
					Default.DisplayMsg(bPopUp: false, LEVEL.TRACE, CSimpleThreadPool.b("款㍀≂♄≆", a_), strFormat, aArgs);
					num = 1;
					break;
				case 1:
					return;
				}
			}
		}

		public static void LogInfor(object strFormat)
		{
			//Discarded unreachable code: IL_0016
			int a_ = 4;
			int num = 0;
			while (true)
			{
				if (true)
				{
				}
				switch (num)
				{
				case 1:
					Default.DisplayMsg(false, LEVEL.INFOR, CSimpleThreadPool.b("\u093fⱁ≃⥅㩇", a_), strFormat);
					num = 2;
					continue;
				case 2:
					return;
				}
				if (Default.Level <= LEVEL.INFOR)
				{
					num = 1;
					continue;
				}
				return;
			}
		}

		public static void LogInfor(object strFormat, params object[] aArgs)
		{
			//Discarded unreachable code: IL_005b
			int a_ = 16;
			int num = 0;
			while (true)
			{
				switch (num)
				{
				default:
					if (Default.Level <= LEVEL.INFOR)
					{
						num = 1;
						break;
					}
					return;
				case 1:
					Default.DisplayMsg(bPopUp: false, LEVEL.INFOR, CSimpleThreadPool.b("Ջ⁍㙏㵑♓", a_), strFormat, aArgs);
					if (true)
					{
					}
					num = 2;
					break;
				case 2:
					return;
				}
			}
		}

		public static void LogWarning(object strFormat)
		{
			//Discarded unreachable code: IL_000c
			int a_ = 11;
			if (true)
			{
			}
			int num = 2;
			while (true)
			{
				switch (num)
				{
				case 0:
					Default.DisplayMsg(false, LEVEL.WARNING, CSimpleThreadPool.b("၆⡈㥊⍌♎㽐㑒", a_), strFormat);
					num = 1;
					continue;
				case 1:
					return;
				}
				if (Default.Level <= LEVEL.WARNING)
				{
					num = 0;
					continue;
				}
				return;
			}
		}

		public static void LogWarning(object strFormat, params object[] aArgs)
		{
			//Discarded unreachable code: IL_0028
			int a_ = 9;
			int num = 2;
			while (true)
			{
				switch (num)
				{
				case 1:
					Default.DisplayMsg(bPopUp: false, LEVEL.WARNING, CSimpleThreadPool.b("ቄ♆㭈╊\u244cⅎ㙐", a_), strFormat, aArgs);
					num = 0;
					continue;
				case 0:
					return;
				}
				if (true)
				{
				}
				if (Default.Level <= LEVEL.WARNING)
				{
					num = 1;
					continue;
				}
				return;
			}
		}

		public static void LogError(object strFormat)
		{
			//Discarded unreachable code: IL_0028
			int a_ = 17;
			int num = 1;
			while (true)
			{
				switch (num)
				{
				case 2:
					Default.DisplayMsg(false, LEVEL.ERROR, CSimpleThreadPool.b("ࡌ㵎⍐㱒❔", a_), strFormat);
					num = 0;
					continue;
				case 0:
					return;
				}
				if (true)
				{
				}
				if (Default.Level <= LEVEL.ERROR)
				{
					num = 2;
					continue;
				}
				return;
			}
		}

		public static void LogError(object strFormat, params object[] aArgs)
		{
			//Discarded unreachable code: IL_0028
			int a_ = 7;
			int num = 2;
			while (true)
			{
				switch (num)
				{
				case 1:
					Default.DisplayMsg(bPopUp: false, LEVEL.ERROR, CSimpleThreadPool.b("ق㝄㕆♈㥊", a_), strFormat, aArgs);
					num = 0;
					continue;
				case 0:
					return;
				}
				if (true)
				{
				}
				if (Default.Level <= LEVEL.ERROR)
				{
					num = 1;
					continue;
				}
				return;
			}
		}

		public static void LogSystem(object strFormat)
		{
			//Discarded unreachable code: IL_0035
			int a_ = 10;
			int num = 0;
			while (true)
			{
				switch (num)
				{
				case 2:
					Default.DisplayMsg(false, LEVEL.SYSTEM, CSimpleThreadPool.b("ᕅㅇ㥉㡋⭍㵏", a_), strFormat);
					num = 1;
					continue;
				case 1:
					return;
				}
				if (Default.Level <= LEVEL.SYSTEM)
				{
					if (true)
					{
					}
					num = 2;
					continue;
				}
				return;
			}
		}

		public static void LogSystem(object strFormat, params object[] aArgs)
		{
			//Discarded unreachable code: IL_000c
			int a_ = 10;
			if (true)
			{
			}
			int num = 2;
			while (true)
			{
				switch (num)
				{
				case 0:
					Default.DisplayMsg(bPopUp: false, LEVEL.SYSTEM, CSimpleThreadPool.b("ᕅㅇ㥉㡋⭍㵏", a_), strFormat, aArgs);
					num = 1;
					continue;
				case 1:
					return;
				}
				if (Default.Level <= LEVEL.SYSTEM)
				{
					num = 0;
					continue;
				}
				return;
			}
		}

		public static void LogStack(object strFormat)
		{
			//Discarded unreachable code: IL_0068
			int a_ = 9;
			int num = 0;
			while (true)
			{
				switch (num)
				{
				case 1:
					Default.DisplayMsg(false, LEVEL.STACK, CSimpleThreadPool.b("ᙄ㍆⡈⡊♌", a_), strFormat);
					num = 2;
					continue;
				case 2:
					if (1 == 0)
					{
					}
					return;
				}
				if (Default.Level <= LEVEL.STACK)
				{
					num = 1;
					continue;
				}
				return;
			}
		}

		public static void LogStack(object strFormat, params object[] aArgs)
		{
			//Discarded unreachable code: IL_0028
			int a_ = 3;
			int num = 0;
			while (true)
			{
				switch (num)
				{
				case 1:
					Default.DisplayMsg(bPopUp: false, LEVEL.STACK, CSimpleThreadPool.b("氾㕀≂♄ⱆ", a_), strFormat, aArgs);
					num = 2;
					continue;
				case 2:
					return;
				}
				if (true)
				{
				}
				if (Default.Level <= LEVEL.STACK)
				{
					num = 1;
					continue;
				}
				return;
			}
		}

		private void ᜀ(FileInfo A_0)
		{
			//Discarded unreachable code: IL_01cb
			int a_ = 19;
			switch (0)
			{
			}
			int num = 4;
			string[] array = default(string[]);
			string text = default(string);
			while (true)
			{
				switch (num)
				{
				default:
					if (!A_0.Exists)
					{
						num = 6;
						continue;
					}
					array = A_0.Name.Split('_');
					num = 3;
					continue;
				case 6:
					return;
				case 5:
					if (Convert.ToDateTime(CSimpleThreadPool.b("絎慐", a_) + text[0] + text[1] + CSimpleThreadPool.b("扎", a_) + text[2] + text[3] + CSimpleThreadPool.b("扎", a_) + text[4] + text[5]) < ᜊ)
					{
						num = 7;
						continue;
					}
					break;
				case 1:
					return;
				case 3:
					if (array.Length <= 1)
					{
						num = 0;
						continue;
					}
					text = array[1];
					num = 2;
					continue;
				case 0:
					return;
				case 7:
					try
					{
						A_0.Delete();
					}
					catch (Exception strFormat)
					{
						DisplayMsg(false, LEVEL.ERROR, CSimpleThreadPool.b("\u0a4e⍐⅒㩔╖", a_), strFormat);
					}
					break;
				case 2:
					num = ((text.Length != 6) ? 1 : 5);
					continue;
				}
				break;
			}
			if (1 == 0)
			{
			}
		}

		protected void _DeleteOverdueFiles()
		{
			//Discarded unreachable code: IL_000c
			int a_ = 17;
			if (true)
			{
			}
			CDirectoryInfo cDirectoryInfo = new CDirectoryInfo(CurDirectory);
			cDirectoryInfo.SetProcessFile(ᜀ);
			cDirectoryInfo.ProcessFiles(bRecursion: false, CSimpleThreadPool.b("杌", a_));
		}
	}
}
