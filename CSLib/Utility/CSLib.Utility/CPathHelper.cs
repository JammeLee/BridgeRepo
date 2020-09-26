using System;

namespace CSLib.Utility
{
	public class CPathHelper : CSingleton<CPathHelper>
	{
		public static string GetCurrentApplicationPath()
		{
			return AppDomain.CurrentDomain.BaseDirectory;
		}

		public static string GetFilePathInCurrentDir(string filename)
		{
			return GetCurrentApplicationPath() + filename;
		}

		public static string GetRelativePathInCurrentDir(string dir)
		{
			return GetCurrentApplicationPath() + dir;
		}

		public static string ConvertPathToString(string path, string relpace)
		{
			//Discarded unreachable code: IL_0003
			if (true)
			{
			}
			path = path.Replace(CConstant.STRING_FOLDER_SPLIT_1, relpace);
			path = path.Replace(CConstant.STRING_FOLDER_SPLIT_2, relpace);
			path = path.Replace(CConstant.STRING_FOLDER_SPLIT_3, relpace);
			return path;
		}

		public static string GetSubDirPath(string basepath, string fullpath)
		{
			//Discarded unreachable code: IL_002e
			int num = 3;
			while (true)
			{
				switch (num)
				{
				default:
					if (basepath != null)
					{
						if (true)
						{
						}
						num = 5;
						continue;
					}
					break;
				case 1:
					return fullpath.Substring(basepath.Length);
				case 0:
					num = 2;
					continue;
				case 2:
					if (fullpath.Length > basepath.Length)
					{
						num = 1;
						continue;
					}
					break;
				case 5:
					num = 4;
					continue;
				case 4:
					if (fullpath != null)
					{
						num = 0;
						continue;
					}
					break;
				}
				break;
			}
			return CConstant.STRING_FOLDER_SPLIT_1;
		}

		public static string GetDirPath(string relativefilepath)
		{
			//Discarded unreachable code: IL_0086
			int num = 3;
			int num2 = default(int);
			while (true)
			{
				switch (num)
				{
				default:
					if (relativefilepath != null)
					{
						num = 7;
						continue;
					}
					break;
				case 6:
					if (num2 == 0)
					{
						num = 2;
						continue;
					}
					break;
				case 4:
					return relativefilepath.Substring(0, num2);
				case 7:
					num2 = relativefilepath.LastIndexOf(CConstant.STRING_FOLDER_SPLIT_1);
					num = 5;
					continue;
				case 5:
					if (num2 > 0)
					{
						num = 1;
						continue;
					}
					goto IL_003d;
				case 2:
					return CConstant.STRING_FOLDER_SPLIT_1;
				case 1:
					if (true)
					{
					}
					num = 0;
					continue;
				case 0:
					{
						if (num2 < relativefilepath.Length)
						{
							num = 4;
							continue;
						}
						goto IL_003d;
					}
					IL_003d:
					num = 6;
					continue;
				}
				break;
			}
			return relativefilepath;
		}

		public static string GetFileName(string filename)
		{
			//Discarded unreachable code: IL_0025
			string fileFullName = GetFileFullName(filename);
			int num = fileFullName.LastIndexOf(CConstant.STRING_FILE_EXT_SPLIT);
			if (num > 0)
			{
				return fileFullName.Substring(0, num);
			}
			if (true)
			{
			}
			return "";
		}

		public static string GetFileExt(string filename)
		{
			//Discarded unreachable code: IL_0045
			while (true)
			{
				int num = filename.LastIndexOf(CConstant.STRING_FILE_EXT_SPLIT);
				int num2 = 0;
				while (true)
				{
					switch (num2)
					{
					case 0:
						if (num >= 0)
						{
							num2 = 3;
							continue;
						}
						goto IL_0067;
					case 2:
						return filename.Substring(num);
					case 3:
						if (true)
						{
						}
						num2 = 1;
						continue;
					case 1:
						{
							if (num < filename.Length - 1)
							{
								num2 = 2;
								continue;
							}
							goto IL_0067;
						}
						IL_0067:
						return "";
					}
					break;
				}
			}
		}

		public static string GetFileFullName(string filename)
		{
			//Discarded unreachable code: IL_0027
			while (true)
			{
				int num = filename.LastIndexOf(CConstant.STRING_FOLDER_SPLIT_1);
				if (true)
				{
				}
				int num2 = 0;
				while (true)
				{
					switch (num2)
					{
					case 0:
						if (num >= 0)
						{
							num2 = 2;
							continue;
						}
						goto IL_0069;
					case 1:
						return filename.Substring(num + 1);
					case 2:
						num2 = 3;
						continue;
					case 3:
						{
							if (num < filename.Length - 2)
							{
								num2 = 1;
								continue;
							}
							goto IL_0069;
						}
						IL_0069:
						return "";
					}
					break;
				}
			}
		}

		public static string BuildFilePath(string basepath, string relativepath)
		{
			//Discarded unreachable code: IL_002e
			int num = 1;
			while (true)
			{
				switch (num)
				{
				default:
					if (basepath != null)
					{
						if (true)
						{
						}
						num = 4;
						continue;
					}
					break;
				case 0:
					return basepath + CConstant.STRING_FOLDER_SPLIT_1 + relativepath;
				case 3:
					num = 2;
					continue;
				case 2:
					if (basepath.CompareTo(relativepath) != 0)
					{
						num = 0;
						continue;
					}
					break;
				case 4:
					num = 5;
					continue;
				case 5:
					if (relativepath != null)
					{
						num = 3;
						continue;
					}
					break;
				}
				break;
			}
			return basepath;
		}
	}
}
