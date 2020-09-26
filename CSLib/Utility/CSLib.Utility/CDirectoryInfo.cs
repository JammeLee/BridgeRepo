using System;
using System.Collections;
using System.IO;

namespace CSLib.Utility
{
	public class CDirectoryInfo
	{
		private DirectoryInfo ᜀ;

		private DirectoryInfoProcessFile ᜁ;

		private Queue ᜂ = new Queue();

		public CDirectoryInfo(string strPath)
		{
			ᜀ = new DirectoryInfo(strPath);
		}

		private CDirectoryInfo(DirectoryInfo A_0)
		{
			ᜀ = A_0;
		}

		public static void Standardization(ref string strPath)
		{
			//Discarded unreachable code: IL_003e
			int a_ = 9;
			int num = 1;
			while (true)
			{
				switch (num)
				{
				case 3:
					return;
				case 4:
					strPath += CSimpleThreadPool.b("᥄", a_);
					num = 0;
					continue;
				case 0:
					return;
				case 2:
					if (strPath[strPath.Length - 1] != '\\')
					{
						num = 4;
						continue;
					}
					return;
				}
				if (strPath == "")
				{
					if (true)
					{
					}
					num = 3;
				}
				else
				{
					strPath = strPath.Replace(CSimpleThreadPool.b("橄", a_), CSimpleThreadPool.b("᥄", a_));
					num = 2;
				}
			}
		}

		public static void DeleteFolder(string strPath)
		{
			//Discarded unreachable code: IL_0099
			int a_ = 14;
			switch (0)
			{
			default:
			{
				int num = 12;
				int num2 = default(int);
				string[] fileSystemEntries = default(string[]);
				string text = default(string);
				FileInfo fileInfo = default(FileInfo);
				FileAttributes attributes = default(FileAttributes);
				while (true)
				{
					switch (num)
					{
					case 0:
						return;
					case 7:
						num = 11;
						continue;
					case 6:
						if (1 == 0)
						{
						}
						goto case 10;
					case 4:
						num2++;
						num = 10;
						continue;
					case 10:
						num = 2;
						continue;
					case 2:
						if (num2 >= fileSystemEntries.Length)
						{
							num = 5;
							continue;
						}
						text = fileSystemEntries[num2];
						num = 3;
						continue;
					case 1:
						fileInfo.Attributes = FileAttributes.Normal;
						num = 7;
						continue;
					case 3:
						if (!File.Exists(text))
						{
							DeleteFolder(text);
							num = 4;
						}
						else
						{
							num = 9;
						}
						continue;
					case 11:
						try
						{
							File.Delete(text);
						}
						catch (Exception strFormat)
						{
							CDebugOut.LogError(strFormat);
						}
						goto case 4;
					case 9:
						fileInfo = new FileInfo(text);
						attributes = fileInfo.Attributes;
						num = 8;
						continue;
					case 8:
						if (attributes.ToString().IndexOf(CSimpleThreadPool.b("ᡉ⥋⽍㑏ᵑ㩓㩕⅗", a_)) != -1)
						{
							num = 1;
							continue;
						}
						goto case 7;
					case 5:
						Directory.Delete(strPath);
						return;
					}
					if (!new DirectoryInfo(strPath).Exists)
					{
						num = 0;
						continue;
					}
					fileSystemEntries = Directory.GetFileSystemEntries(strPath);
					num2 = 0;
					num = 6;
				}
			}
			}
		}

		public void SetProcessFile(DirectoryInfoProcessFile refProcessFile)
		{
			ᜁ = refProcessFile;
		}

		public void SetIgnoreFile(string strFileName)
		{
			if (!ᜂ.Contains(strFileName))
			{
				ᜂ.Enqueue(strFileName);
			}
		}

		public bool ProcessFiles(bool bRecursion, string strPattern)
		{
			//Discarded unreachable code: IL_01a5
			int a_ = 17;
			switch (0)
			{
			default:
			{
				int num = 11;
				int num2 = default(int);
				DirectoryInfo[] directories = default(DirectoryInfo[]);
				FileInfo fileInfo = default(FileInfo);
				FileInfo[] files = default(FileInfo[]);
				while (true)
				{
					switch (num)
					{
					default:
						num = ((ᜁ == null) ? 9 : 0);
						continue;
					case 1:
						num = 12;
						continue;
					case 12:
						if (bRecursion)
						{
							num = 8;
							continue;
						}
						break;
					case 13:
						num2++;
						num = 3;
						continue;
					case 0:
						if (ᜀ.Exists)
						{
							num = 5;
							continue;
						}
						break;
					case 9:
						return false;
					case 8:
						directories = ᜀ.GetDirectories(CSimpleThreadPool.b("杌", a_));
						num2 = 0;
						num = 10;
						continue;
					case 7:
					case 10:
						num = 6;
						continue;
					case 6:
						if (num2 < directories.Length)
						{
							CDirectoryInfo cDirectoryInfo = new CDirectoryInfo(directories[num2]);
							cDirectoryInfo.ᜁ = ᜁ;
							cDirectoryInfo.ProcessFiles(bRecursion, strPattern);
							num2++;
							num = 7;
						}
						else
						{
							num = 15;
						}
						continue;
					case 4:
						ᜁ(fileInfo);
						num = 13;
						continue;
					case 5:
						files = ᜀ.GetFiles(strPattern);
						num2 = 0;
						num = 2;
						continue;
					case 2:
					case 3:
						num = 14;
						continue;
					case 14:
						if (true)
						{
						}
						if (num2 >= files.Length)
						{
							num = 1;
							continue;
						}
						fileInfo = files[num2];
						num = 16;
						continue;
					case 16:
						if (!ᜂ.Contains(fileInfo.Name))
						{
							num = 4;
							continue;
						}
						goto case 13;
					case 15:
						break;
					}
					break;
				}
				return true;
			}
			}
		}
	}
}
