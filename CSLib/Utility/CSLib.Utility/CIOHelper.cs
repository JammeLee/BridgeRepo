using System;
using System.Collections;
using System.IO;
using System.Runtime.InteropServices;

namespace CSLib.Utility
{
	public class CIOHelper : CSingleton<CIOHelper>
	{
		public class DirDateCompare : IComparer
		{
			public int Compare(object x, object y)
			{
				//Discarded unreachable code: IL_0003
				if (true)
				{
				}
				DirectoryInfo obj = (DirectoryInfo)x;
				DirectoryInfo directoryInfo = (DirectoryInfo)y;
				return obj.CreationTime.CompareTo(directoryInfo.CreationTime);
			}
		}

		private int m_ᜀ;

		private int ᜁ;

		private int ᜂ;

		private int ᜃ;

		private int ᜄ;

		private long ᜅ;

		public int GetFileCount(ref DirectoryInfo dirInfo, string pattern, string[] filter)
		{
			//Discarded unreachable code: IL_0003
			if (true)
			{
			}
			this.m_ᜀ = 0;
			ᜀ(ref dirInfo, pattern, filter);
			return this.m_ᜀ;
		}

		private void ᜀ(ref DirectoryInfo A_0, string A_1, string[] A_2)
		{
			//Discarded unreachable code: IL_0059
			switch (0)
			{
			default:
			{
				int num = 1;
				int num2 = default(int);
				DirectoryInfo A_3 = default(DirectoryInfo);
				DirectoryInfo[] directories = default(DirectoryInfo[]);
				while (true)
				{
					switch (num)
					{
					default:
						if (A_0 != null)
						{
							num = 3;
							break;
						}
						return;
					case 8:
						if (true)
						{
						}
						num2++;
						num = 2;
						break;
					case 6:
						ᜀ(ref A_3, A_1, A_2);
						num = 8;
						break;
					case 3:
					{
						FileInfo[] files = A_0.GetFiles(A_1);
						this.m_ᜀ += files.Length;
						directories = A_0.GetDirectories();
						num2 = 0;
						num = 7;
						break;
					}
					case 2:
					case 7:
						num = 0;
						break;
					case 0:
						if (num2 >= directories.Length)
						{
							num = 5;
							break;
						}
						A_3 = directories[num2];
						num = 4;
						break;
					case 5:
						return;
					case 4:
						if (!CSingleton<CFilterHelper>.Instance.InFilterList(A_3.Name))
						{
							num = 6;
							break;
						}
						goto case 8;
					}
				}
			}
			}
		}

		public static bool IsTypeOf(string filename, string ext)
		{
			FileInfo fileInfo = new FileInfo(filename);
			return ext.ToLower() == fileInfo.Extension.ToLower();
		}

		public static void CreateDir(string dirname)
		{
			//Discarded unreachable code: IL_0024
			if (!Directory.Exists(dirname))
			{
				try
				{
					Directory.CreateDirectory(dirname);
				}
				catch (Exception ex)
				{
					CSingleton<CLogInfoList>.Instance.WriteLine(ex);
				}
			}
			if (1 == 0)
			{
			}
		}

		public static void CreateDir(DirectoryInfo di)
		{
			if (!di.Exists)
			{
				di.Create();
			}
		}

		public static void DeleteDir(string dir)
		{
			if (Directory.Exists(dir))
			{
				Directory.Delete(dir, recursive: true);
			}
		}

		public static void ForceDeleteDir(string dir)
		{
			//Discarded unreachable code: IL_00b9
			int a_ = 17;
			int num2 = default(int);
			FileInfo[] files = default(FileInfo[]);
			while (true)
			{
				DirectoryInfo directoryInfo = new DirectoryInfo(dir);
				int num = 5;
				while (true)
				{
					switch (num)
					{
					case 5:
						if (directoryInfo.Exists)
						{
							num = 3;
							continue;
						}
						return;
					case 0:
					case 2:
						num = 6;
						continue;
					case 6:
					{
						if (num2 >= files.Length)
						{
							num = 4;
							continue;
						}
						FileInfo obj = files[num2];
						obj.Attributes = FileAttributes.Normal;
						obj.Delete();
						num2++;
						num = 2;
						continue;
					}
					case 3:
						directoryInfo.Attributes = (FileAttributes)0;
						files = directoryInfo.GetFiles(CSimpleThreadPool.b("杌慎筐", a_), SearchOption.AllDirectories);
						num2 = 0;
						num = 0;
						continue;
					case 4:
						if (true)
						{
						}
						directoryInfo.Delete(recursive: true);
						num = 1;
						continue;
					case 1:
						return;
					}
					break;
				}
			}
		}

		public static void ForceDeleteDir(DirectoryInfo dir)
		{
			//Discarded unreachable code: IL_0086
			int a_ = 7;
			int num = 6;
			int num2 = default(int);
			FileInfo[] files = default(FileInfo[]);
			while (true)
			{
				switch (num)
				{
				default:
					if (dir.Exists)
					{
						num = 4;
						break;
					}
					return;
				case 2:
				case 5:
					num = 3;
					break;
				case 3:
				{
					if (num2 >= files.Length)
					{
						num = 0;
						break;
					}
					FileInfo obj = files[num2];
					obj.Attributes = FileAttributes.Normal;
					obj.Delete();
					num2++;
					num = 5;
					break;
				}
				case 4:
					if (true)
					{
					}
					dir.Attributes = (FileAttributes)0;
					files = dir.GetFiles(CSimpleThreadPool.b("楂歄浆", a_), SearchOption.AllDirectories);
					num2 = 0;
					num = 2;
					break;
				case 0:
					dir.Delete(recursive: true);
					num = 1;
					break;
				case 1:
					return;
				}
			}
		}

		public static void CopyDir(string source, string des)
		{
			//Discarded unreachable code: IL_005c
			int a_ = 7;
			switch (0)
			{
			}
			int num2 = default(int);
			FileInfo[] files = default(FileInfo[]);
			while (true)
			{
				DirectoryInfo directoryInfo = new DirectoryInfo(source);
				int length = directoryInfo.Parent.FullName.Length;
				int num = 2;
				while (true)
				{
					switch (num)
					{
					case 2:
						if (true)
						{
						}
						if (directoryInfo.Exists)
						{
							num = 1;
							continue;
						}
						return;
					case 0:
					case 3:
						num = 4;
						continue;
					case 4:
					{
						if (num2 >= files.Length)
						{
							num = 5;
							continue;
						}
						FileInfo fileInfo = files[num2];
						string str = fileInfo.Directory.FullName.Substring(length);
						string dirname = des + str;
						string destFileName = des + str + CSimpleThreadPool.b("ὂ", a_) + fileInfo.Name;
						CreateDir(dirname);
						File.Copy(fileInfo.FullName, destFileName, overwrite: true);
						num2++;
						num = 0;
						continue;
					}
					case 5:
						return;
					case 1:
						files = directoryInfo.GetFiles(CSimpleThreadPool.b("楂歄浆", a_), SearchOption.AllDirectories);
						num2 = 0;
						num = 3;
						continue;
					}
					break;
				}
			}
		}

		public static void CopyFile(string sourceFile, string des)
		{
			//Discarded unreachable code: IL_0020
			int a_ = 1;
			while (true)
			{
				if (true)
				{
				}
				FileInfo fileInfo = new FileInfo(sourceFile);
				int num = 2;
				while (true)
				{
					switch (num)
					{
					case 2:
						if (fileInfo.Exists)
						{
							num = 1;
							continue;
						}
						return;
					case 1:
					{
						string destFileName = des + CSimpleThreadPool.b("愼", a_) + fileInfo.Name;
						File.Copy(sourceFile, destFileName);
						num = 0;
						continue;
					}
					case 0:
						return;
					}
					break;
				}
			}
		}

		public static string GetOldestFile(FileInfo[] files)
		{
			//Discarded unreachable code: IL_0080
			switch (0)
			{
			}
			FileInfo fileInfo = default(FileInfo);
			while (true)
			{
				string result = "";
				DateTime t = DateTime.Now;
				int num = 0;
				int num2 = 4;
				while (true)
				{
					switch (num2)
					{
					case 5:
						num++;
						num2 = 0;
						continue;
					case 1:
						t = fileInfo.LastWriteTime;
						result = fileInfo.FullName;
						num2 = 5;
						continue;
					case 2:
						if (fileInfo.LastWriteTime < t)
						{
							num2 = 1;
							continue;
						}
						goto case 5;
					case 0:
					case 4:
						num2 = 6;
						continue;
					case 6:
						if (num < files.Length)
						{
							fileInfo = files[num];
							if (true)
							{
							}
							num2 = 2;
						}
						else
						{
							num2 = 3;
						}
						continue;
					case 3:
						return result;
					}
					break;
				}
			}
		}

		public static long GetDirectorySize(string dir, out int filecount)
		{
			//Discarded unreachable code: IL_0048
			int a_ = 11;
			switch (0)
			{
			}
			int num3 = default(int);
			FileInfo[] array = default(FileInfo[]);
			while (true)
			{
				long num = 0L;
				filecount = 0;
				DirectoryInfo directoryInfo = new DirectoryInfo(dir);
				if (true)
				{
				}
				int num2 = 3;
				while (true)
				{
					switch (num2)
					{
					case 3:
						if (directoryInfo.Exists)
						{
							num2 = 2;
							continue;
						}
						goto case 0;
					case 1:
					case 4:
						num2 = 5;
						continue;
					case 5:
					{
						if (num3 >= array.Length)
						{
							num2 = 0;
							continue;
						}
						FileInfo fileInfo = array[num3];
						num += fileInfo.Length;
						num3++;
						num2 = 4;
						continue;
					}
					case 2:
					{
						FileInfo[] files = directoryInfo.GetFiles(CSimpleThreadPool.b("浆杈慊", a_), SearchOption.AllDirectories);
						filecount = files.Length;
						array = files;
						num3 = 0;
						num2 = 1;
						continue;
					}
					case 0:
						return num;
					}
					break;
				}
			}
		}

		[DllImport("kernel32.dll", CharSet = CharSet.Auto)]
		public static extern bool GetDiskFreeSpace([MarshalAs(UnmanagedType.LPTStr)] string rootParhName, ref int sectorsPerCluster, ref int bytesPerSector, ref int numberOfFreeClusters, ref int totalNumberOfCluster);

		public void GetDiskInfo(string path)
		{
			//Discarded unreachable code: IL_0003
			if (true)
			{
			}
			GetDiskFreeSpace(path, ref ᜁ, ref ᜂ, ref ᜃ, ref ᜄ);
			ᜅ = ᜂ * ᜁ;
		}

		public long GetFileSpaceSize(long length)
		{
			//Discarded unreachable code: IL_0003
			if (true)
			{
			}
			int num = 0;
			while (true)
			{
				switch (num)
				{
				default:
					if (ᜅ > 0)
					{
						num = 2;
						continue;
					}
					break;
				case 1:
				{
					decimal d = length / ᜅ;
					return ᜅ * Convert.ToInt32(Math.Ceiling(d) + 1m);
				}
				case 2:
					num = 3;
					continue;
				case 3:
					if (length % ᜅ != 0L)
					{
						num = 1;
						continue;
					}
					break;
				}
				break;
			}
			return length;
		}

		public long GetDirSpaceSize(string dir, string filter)
		{
			DirectoryInfo dir2 = new DirectoryInfo(dir);
			return GetDirSpaceSize(dir2, filter);
		}

		public long GetDirSpaceSize(DirectoryInfo dir, string filter)
		{
			//Discarded unreachable code: IL_0099
			switch (0)
			{
			default:
			{
				int num = 12;
				int num3 = default(int);
				FileInfo[] files = default(FileInfo[]);
				long num2 = default(long);
				DirectoryInfo[] directories = default(DirectoryInfo[]);
				while (true)
				{
					switch (num)
					{
					default:
						if (dir != null)
						{
							num = 0;
							continue;
						}
						break;
					case 3:
					case 10:
						num = 4;
						continue;
					case 4:
					{
						if (true)
						{
						}
						if (num3 >= files.Length)
						{
							num = 1;
							continue;
						}
						FileInfo fileInfo = files[num3];
						num2 += GetFileSpaceSize(fileInfo.Length);
						num3++;
						num = 10;
						continue;
					}
					case 13:
						num = 9;
						continue;
					case 9:
						if (dir.Name.ToLower() != filter.ToLower())
						{
							num = 11;
							continue;
						}
						break;
					case 2:
					case 6:
						num = 8;
						continue;
					case 8:
						if (num3 < directories.Length)
						{
							DirectoryInfo dir2 = directories[num3];
							num2 += GetDirSpaceSize(dir2, filter);
							num3++;
							num = 2;
						}
						else
						{
							num = 5;
						}
						continue;
					case 0:
						num = 7;
						continue;
					case 7:
						if (dir.Exists)
						{
							num = 13;
							continue;
						}
						break;
					case 1:
						directories = dir.GetDirectories();
						num3 = 0;
						num = 6;
						continue;
					case 5:
						return num2;
					case 11:
						num2 = 0L;
						files = dir.GetFiles();
						num3 = 0;
						num = 3;
						continue;
					}
					break;
				}
				return 0L;
			}
			}
		}

		public static ArrayList SortDirByDate(DirectoryInfo[] dirs)
		{
			//Discarded unreachable code: IL_0023
			int num = 2;
			while (true)
			{
				switch (num)
				{
				default:
					if (true)
					{
					}
					if (dirs != null)
					{
						num = 0;
						break;
					}
					goto case 1;
				case 1:
					return null;
				case 0:
					num = 3;
					break;
				case 3:
				{
					if (dirs.Length < 1)
					{
						num = 1;
						break;
					}
					ArrayList arrayList = new ArrayList(dirs);
					arrayList.Sort(new DirDateCompare());
					return arrayList;
				}
				}
			}
		}
	}
}
