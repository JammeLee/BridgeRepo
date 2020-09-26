using System.IO.IsolatedStorage;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Permissions;
using System.Text;
using Microsoft.Win32;

namespace System.IO
{
	[ComVisible(true)]
	public static class Path
	{
		internal const int MAX_PATH = 260;

		internal const int MAX_DIRECTORY_PATH = 248;

		public static readonly char DirectorySeparatorChar = '\\';

		public static readonly char AltDirectorySeparatorChar = '/';

		public static readonly char VolumeSeparatorChar = ':';

		[Obsolete("Please use GetInvalidPathChars or GetInvalidFileNameChars instead.")]
		public static readonly char[] InvalidPathChars = new char[36]
		{
			'"',
			'<',
			'>',
			'|',
			'\0',
			'\u0001',
			'\u0002',
			'\u0003',
			'\u0004',
			'\u0005',
			'\u0006',
			'\a',
			'\b',
			'\t',
			'\n',
			'\v',
			'\f',
			'\r',
			'\u000e',
			'\u000f',
			'\u0010',
			'\u0011',
			'\u0012',
			'\u0013',
			'\u0014',
			'\u0015',
			'\u0016',
			'\u0017',
			'\u0018',
			'\u0019',
			'\u001a',
			'\u001b',
			'\u001c',
			'\u001d',
			'\u001e',
			'\u001f'
		};

		private static readonly char[] RealInvalidPathChars = new char[36]
		{
			'"',
			'<',
			'>',
			'|',
			'\0',
			'\u0001',
			'\u0002',
			'\u0003',
			'\u0004',
			'\u0005',
			'\u0006',
			'\a',
			'\b',
			'\t',
			'\n',
			'\v',
			'\f',
			'\r',
			'\u000e',
			'\u000f',
			'\u0010',
			'\u0011',
			'\u0012',
			'\u0013',
			'\u0014',
			'\u0015',
			'\u0016',
			'\u0017',
			'\u0018',
			'\u0019',
			'\u001a',
			'\u001b',
			'\u001c',
			'\u001d',
			'\u001e',
			'\u001f'
		};

		private static readonly char[] InvalidFileNameChars = new char[41]
		{
			'"',
			'<',
			'>',
			'|',
			'\0',
			'\u0001',
			'\u0002',
			'\u0003',
			'\u0004',
			'\u0005',
			'\u0006',
			'\a',
			'\b',
			'\t',
			'\n',
			'\v',
			'\f',
			'\r',
			'\u000e',
			'\u000f',
			'\u0010',
			'\u0011',
			'\u0012',
			'\u0013',
			'\u0014',
			'\u0015',
			'\u0016',
			'\u0017',
			'\u0018',
			'\u0019',
			'\u001a',
			'\u001b',
			'\u001c',
			'\u001d',
			'\u001e',
			'\u001f',
			':',
			'*',
			'?',
			'\\',
			'/'
		};

		public static readonly char PathSeparator = ';';

		internal static readonly int MaxPath = 260;

		public static string ChangeExtension(string path, string extension)
		{
			if (path != null)
			{
				CheckInvalidPathChars(path);
				string text = path;
				int num = path.Length;
				while (--num >= 0)
				{
					char c = path[num];
					if (c == '.')
					{
						text = path.Substring(0, num);
						break;
					}
					if (c == DirectorySeparatorChar || c == AltDirectorySeparatorChar || c == VolumeSeparatorChar)
					{
						break;
					}
				}
				if (extension != null && path.Length != 0)
				{
					if (extension.Length == 0 || extension[0] != '.')
					{
						text += ".";
					}
					text += extension;
				}
				return text;
			}
			return null;
		}

		public static string GetDirectoryName(string path)
		{
			if (path != null)
			{
				CheckInvalidPathChars(path);
				path = FixupPath(path);
				int rootLength = GetRootLength(path);
				int length = path.Length;
				if (length > rootLength)
				{
					length = path.Length;
					if (length == rootLength)
					{
						return null;
					}
					while (length > rootLength && path[--length] != DirectorySeparatorChar && path[length] != AltDirectorySeparatorChar)
					{
					}
					return path.Substring(0, length);
				}
			}
			return null;
		}

		internal static int GetRootLength(string path)
		{
			CheckInvalidPathChars(path);
			int i = 0;
			int length = path.Length;
			if (length >= 1 && IsDirectorySeparator(path[0]))
			{
				i = 1;
				if (length >= 2 && IsDirectorySeparator(path[1]))
				{
					i = 2;
					int num = 2;
					for (; i < length; i++)
					{
						if ((path[i] == DirectorySeparatorChar || path[i] == AltDirectorySeparatorChar) && --num <= 0)
						{
							break;
						}
					}
				}
			}
			else if (length >= 2 && path[1] == VolumeSeparatorChar)
			{
				i = 2;
				if (length >= 3 && IsDirectorySeparator(path[2]))
				{
					i++;
				}
			}
			return i;
		}

		internal static bool IsDirectorySeparator(char c)
		{
			if (c != DirectorySeparatorChar)
			{
				return c == AltDirectorySeparatorChar;
			}
			return true;
		}

		public static char[] GetInvalidPathChars()
		{
			return (char[])RealInvalidPathChars.Clone();
		}

		public static char[] GetInvalidFileNameChars()
		{
			return (char[])InvalidFileNameChars.Clone();
		}

		public static string GetExtension(string path)
		{
			if (path == null)
			{
				return null;
			}
			CheckInvalidPathChars(path);
			int length = path.Length;
			int num = length;
			while (--num >= 0)
			{
				char c = path[num];
				if (c == '.')
				{
					if (num != length - 1)
					{
						return path.Substring(num, length - num);
					}
					return string.Empty;
				}
				if (c == DirectorySeparatorChar || c == AltDirectorySeparatorChar || c == VolumeSeparatorChar)
				{
					break;
				}
			}
			return string.Empty;
		}

		public static string GetFullPath(string path)
		{
			string fullPathInternal = GetFullPathInternal(path);
			new FileIOPermission(FileIOPermissionAccess.PathDiscovery, new string[1]
			{
				fullPathInternal
			}, checkForDuplicates: false, needFullPath: false).Demand();
			return fullPathInternal;
		}

		internal static string GetFullPathInternal(string path)
		{
			if (path == null)
			{
				throw new ArgumentNullException("path");
			}
			return NormalizePath(path, fullCheck: true);
		}

		internal static string NormalizePath(string path, bool fullCheck)
		{
			if (Environment.nativeIsWin9x())
			{
				return NormalizePathSlow(path, fullCheck);
			}
			return NormalizePathFast(path, fullCheck);
		}

		internal static string NormalizePathSlow(string path, bool fullCheck)
		{
			if (fullCheck)
			{
				path = path.TrimEnd();
				CheckInvalidPathChars(path);
			}
			int i = 0;
			char[] array = new char[MaxPath];
			int bufferLength = 0;
			char[] array2 = null;
			uint num = 0u;
			uint num2 = 0u;
			bool flag = false;
			uint num3 = 0u;
			int num4 = -1;
			bool flag2 = false;
			bool flag3 = true;
			bool flag4 = false;
			if (path.Length > 0 && (path[0] == DirectorySeparatorChar || path[0] == AltDirectorySeparatorChar))
			{
				array[bufferLength++] = '\\';
				i++;
				num4 = 0;
			}
			for (; i < path.Length; i++)
			{
				char c = path[i];
				if (c == DirectorySeparatorChar || c == AltDirectorySeparatorChar)
				{
					if (num3 == 0)
					{
						if (num2 != 0)
						{
							int num5 = num4 + 1;
							if (path[num5] != '.')
							{
								throw new ArgumentException(Environment.GetResourceString("Arg_PathIllegal"));
							}
							if (num2 >= 2)
							{
								if (flag2 && num2 > 2)
								{
									throw new ArgumentException(Environment.GetResourceString("Arg_PathIllegal"));
								}
								if (path[num5 + 1] == '.')
								{
									for (int j = num5 + 2; j < num5 + num2; j++)
									{
										if (path[j] != '.')
										{
											throw new ArgumentException(Environment.GetResourceString("Arg_PathIllegal"));
										}
									}
									num2 = 2u;
								}
								else
								{
									if (num2 > 1)
									{
										throw new ArgumentException(Environment.GetResourceString("Arg_PathIllegal"));
									}
									num2 = 1u;
								}
							}
							if (bufferLength + num2 + 1 >= MaxPath)
							{
								throw new PathTooLongException(Environment.GetResourceString("IO.PathTooLong"));
							}
							if (num2 == 2)
							{
								array[bufferLength++] = '.';
							}
							array[bufferLength++] = '.';
							flag = false;
						}
						if (num != 0 && flag3 && i + 1 < path.Length && (path[i + 1] == DirectorySeparatorChar || path[i + 1] == AltDirectorySeparatorChar))
						{
							array[bufferLength++] = DirectorySeparatorChar;
						}
					}
					num2 = 0u;
					num = 0u;
					if (!flag)
					{
						flag = true;
						if (bufferLength + 1 >= MaxPath)
						{
							throw new PathTooLongException(Environment.GetResourceString("IO.PathTooLong"));
						}
						array[bufferLength++] = DirectorySeparatorChar;
					}
					num3 = 0u;
					num4 = i;
					flag2 = false;
					flag3 = false;
					if (flag4)
					{
						array[bufferLength] = '\0';
						TryExpandShortFileName(array, ref bufferLength, 260);
						flag4 = false;
					}
					continue;
				}
				switch (c)
				{
				case '.':
					num2++;
					continue;
				case ' ':
					num++;
					continue;
				case '~':
					flag4 = true;
					break;
				}
				flag = false;
				if (flag3 && c == VolumeSeparatorChar)
				{
					char c2 = ((i > 0) ? path[i - 1] : ' ');
					if (num2 != 0 || num3 < 1 || c2 == ' ')
					{
						throw new ArgumentException(Environment.GetResourceString("Arg_PathIllegal"));
					}
					flag2 = true;
					if (num3 > 1)
					{
						uint num6;
						for (num6 = 0u; num6 < bufferLength && array[num6] == ' '; num6++)
						{
						}
						if (num3 - num6 == 1)
						{
							array[0] = c2;
							bufferLength = 1;
						}
					}
					num3 = 0u;
				}
				else
				{
					num3 += 1 + num2 + num;
				}
				if (num2 != 0 || num != 0)
				{
					int num7 = ((num4 >= 0) ? (i - num4 - 1) : i);
					if (num7 > 0)
					{
						path.CopyTo(num4 + 1, array, bufferLength, num7);
						bufferLength += num7;
					}
					num2 = 0u;
					num = 0u;
				}
				if (bufferLength + 1 >= MaxPath)
				{
					throw new PathTooLongException(Environment.GetResourceString("IO.PathTooLong"));
				}
				array[bufferLength++] = c;
				num4 = i;
			}
			if (num3 == 0 && num2 != 0)
			{
				int num8 = num4 + 1;
				if (path[num8] != '.')
				{
					throw new ArgumentException(Environment.GetResourceString("Arg_PathIllegal"));
				}
				if (num2 >= 2)
				{
					if (flag2 && num2 > 2)
					{
						throw new ArgumentException(Environment.GetResourceString("Arg_PathIllegal"));
					}
					if (path[num8 + 1] == '.')
					{
						for (int k = num8 + 2; k < num8 + num2; k++)
						{
							if (path[k] != '.')
							{
								throw new ArgumentException(Environment.GetResourceString("Arg_PathIllegal"));
							}
						}
						num2 = 2u;
					}
					else
					{
						if (num2 > 1)
						{
							throw new ArgumentException(Environment.GetResourceString("Arg_PathIllegal"));
						}
						num2 = 1u;
					}
				}
				if (bufferLength + num2 >= MaxPath)
				{
					throw new PathTooLongException(Environment.GetResourceString("IO.PathTooLong"));
				}
				if (num2 == 2)
				{
					array[bufferLength++] = '.';
				}
				array[bufferLength++] = '.';
			}
			if (bufferLength == 0)
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_PathIllegal"));
			}
			array[bufferLength] = '\0';
			if (fullCheck && (CharArrayStartsWithOrdinal(array, bufferLength, "http:", ignoreCase: false) || CharArrayStartsWithOrdinal(array, bufferLength, "file:", ignoreCase: false)))
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_PathUriFormatNotSupported"));
			}
			if (flag4)
			{
				TryExpandShortFileName(array, ref bufferLength, MaxPath);
			}
			int num9 = 1;
			char[] array3;
			int bufferLength2;
			if (fullCheck)
			{
				array2 = new char[MaxPath + 1];
				num9 = Win32Native.GetFullPathName(array, MaxPath + 1, array2, IntPtr.Zero);
				if (num9 > MaxPath)
				{
					array2 = new char[num9];
					num9 = Win32Native.GetFullPathName(array, num9, array2, IntPtr.Zero);
					if (num9 > MaxPath)
					{
						throw new PathTooLongException(Environment.GetResourceString("IO.PathTooLong"));
					}
				}
				if (num9 == 0 && array[0] != 0)
				{
					__Error.WinIOError();
				}
				else if (num9 < MaxPath)
				{
					array2[num9] = '\0';
				}
				if (Environment.nativeIsWin9x())
				{
					for (int l = 0; l < 260; l++)
					{
						if (array2[l] == '\0')
						{
							num9 = l;
							break;
						}
					}
				}
				array3 = array2;
				bufferLength2 = num9;
				flag4 = false;
				for (uint num10 = 0u; num10 < bufferLength2; num10++)
				{
					if (flag4)
					{
						break;
					}
					if (array2[num10] == '~')
					{
						flag4 = true;
					}
				}
				if (flag4 && !TryExpandShortFileName(array2, ref bufferLength2, MaxPath))
				{
					int bufferLength3 = Array.LastIndexOf(array2, DirectorySeparatorChar, bufferLength2 - 1, bufferLength2);
					if (bufferLength3 >= 0)
					{
						char[] array4 = new char[bufferLength2 - bufferLength3 - 1];
						Array.Copy(array2, bufferLength3 + 1, array4, 0, bufferLength2 - bufferLength3 - 1);
						array2[bufferLength3] = '\0';
						bool flag5 = TryExpandShortFileName(array2, ref bufferLength3, MaxPath);
						array2[bufferLength3] = DirectorySeparatorChar;
						Array.Copy(array4, 0, array2, bufferLength3 + 1, array4.Length);
						if (flag5)
						{
							bufferLength2 = bufferLength3 + 1 + array4.Length;
						}
					}
				}
			}
			else
			{
				array3 = array;
				bufferLength2 = bufferLength;
			}
			if (num9 != 0 && array3[0] == '\\' && array3[1] == '\\')
			{
				int m;
				for (m = 2; m < num9; m++)
				{
					if (array3[m] == '\\')
					{
						m++;
						break;
					}
				}
				if (m == num9)
				{
					throw new ArgumentException(Environment.GetResourceString("Arg_PathIllegalUNC"));
				}
				if (CharArrayStartsWithOrdinal(array3, bufferLength2, "\\\\?\\globalroot", ignoreCase: true))
				{
					throw new ArgumentException(Environment.GetResourceString("Arg_PathGlobalRoot"));
				}
			}
			if (bufferLength2 >= MaxPath)
			{
				throw new PathTooLongException(Environment.GetResourceString("IO.PathTooLong"));
			}
			if (num9 == 0)
			{
				int num11 = Marshal.GetLastWin32Error();
				if (num11 == 0)
				{
					num11 = 161;
				}
				__Error.WinIOError(num11, path);
				return null;
			}
			return new string(array3, 0, bufferLength2);
		}

		private static bool CharArrayStartsWithOrdinal(char[] array, int numChars, string compareTo, bool ignoreCase)
		{
			if (numChars < compareTo.Length)
			{
				return false;
			}
			if (ignoreCase)
			{
				string value = new string(array, 0, compareTo.Length);
				return compareTo.Equals(value, StringComparison.OrdinalIgnoreCase);
			}
			for (int i = 0; i < compareTo.Length; i++)
			{
				if (array[i] != compareTo[i])
				{
					return false;
				}
			}
			return true;
		}

		private static bool TryExpandShortFileName(char[] buffer, ref int bufferLength, int maxBufferSize)
		{
			char[] array = new char[MaxPath + 1];
			int num = Win32Native.GetLongPathName(buffer, array, MaxPath);
			if (num >= MaxPath)
			{
				throw new PathTooLongException(Environment.GetResourceString("IO.PathTooLong"));
			}
			if (num == 0)
			{
				return false;
			}
			if (Environment.nativeIsWin9x())
			{
				for (int i = 0; i < 260; i++)
				{
					if (array[i] == '\0')
					{
						num = i;
						break;
					}
				}
			}
			Buffer.BlockCopy(array, 0, buffer, 0, 2 * num);
			bufferLength = num;
			buffer[bufferLength] = '\0';
			return true;
		}

		private unsafe static void SafeSetStackPointerValue(char* buffer, int index, char value)
		{
			if (index >= MaxPath)
			{
				throw new PathTooLongException(Environment.GetResourceString("IO.PathTooLong"));
			}
			buffer[index] = value;
		}

		internal unsafe static string NormalizePathFast(string path, bool fullCheck)
		{
			if (fullCheck)
			{
				path = path.TrimEnd();
				CheckInvalidPathChars(path);
			}
			int i = 0;
			char* ptr = (char*)stackalloc byte[2 * MaxPath];
			int bufferLength = 0;
			uint num = 0u;
			uint num2 = 0u;
			bool flag = false;
			uint num3 = 0u;
			int num4 = -1;
			bool flag2 = false;
			bool flag3 = true;
			bool flag4 = false;
			if (path.Length > 0 && (path[0] == DirectorySeparatorChar || path[0] == AltDirectorySeparatorChar))
			{
				SafeSetStackPointerValue(ptr, bufferLength++, '\\');
				i++;
				num4 = 0;
			}
			for (; i < path.Length; i++)
			{
				char c = path[i];
				if (c == DirectorySeparatorChar || c == AltDirectorySeparatorChar)
				{
					if (num3 == 0)
					{
						if (num2 != 0)
						{
							int num5 = num4 + 1;
							if (path[num5] != '.')
							{
								throw new ArgumentException(Environment.GetResourceString("Arg_PathIllegal"));
							}
							if (num2 >= 2)
							{
								if (flag2 && num2 > 2)
								{
									throw new ArgumentException(Environment.GetResourceString("Arg_PathIllegal"));
								}
								if (path[num5 + 1] == '.')
								{
									for (int j = num5 + 2; j < num5 + num2; j++)
									{
										if (path[j] != '.')
										{
											throw new ArgumentException(Environment.GetResourceString("Arg_PathIllegal"));
										}
									}
									num2 = 2u;
								}
								else
								{
									if (num2 > 1)
									{
										throw new ArgumentException(Environment.GetResourceString("Arg_PathIllegal"));
									}
									num2 = 1u;
								}
							}
							if (num2 == 2)
							{
								SafeSetStackPointerValue(ptr, bufferLength++, '.');
							}
							SafeSetStackPointerValue(ptr, bufferLength++, '.');
							flag = false;
						}
						if (num != 0 && flag3 && i + 1 < path.Length && (path[i + 1] == DirectorySeparatorChar || path[i + 1] == AltDirectorySeparatorChar))
						{
							SafeSetStackPointerValue(ptr, bufferLength++, DirectorySeparatorChar);
						}
					}
					num2 = 0u;
					num = 0u;
					if (!flag)
					{
						flag = true;
						SafeSetStackPointerValue(ptr, bufferLength++, DirectorySeparatorChar);
					}
					num3 = 0u;
					num4 = i;
					flag2 = false;
					flag3 = false;
					if (flag4)
					{
						SafeSetStackPointerValue(ptr, bufferLength, '\0');
						TryExpandShortFileName(ptr, ref bufferLength, 260);
						flag4 = false;
					}
					continue;
				}
				switch (c)
				{
				case '.':
					num2++;
					continue;
				case ' ':
					num++;
					continue;
				case '~':
					flag4 = true;
					break;
				}
				flag = false;
				if (flag3 && c == VolumeSeparatorChar)
				{
					char c2 = ((i > 0) ? path[i - 1] : ' ');
					if (num2 != 0 || num3 < 1 || c2 == ' ')
					{
						throw new ArgumentException(Environment.GetResourceString("Arg_PathIllegal"));
					}
					flag2 = true;
					if (num3 > 1)
					{
						uint num6;
						for (num6 = 0u; num6 < bufferLength && ptr[num6] == ' '; num6++)
						{
						}
						if (num3 - num6 == 1)
						{
							*ptr = c2;
							bufferLength = 1;
						}
					}
					num3 = 0u;
				}
				else
				{
					num3 += 1 + num2 + num;
				}
				if (num2 != 0 || num != 0)
				{
					int num7 = ((num4 >= 0) ? (i - num4 - 1) : i);
					if (num7 > 0)
					{
						for (int k = 0; k < num7; k++)
						{
							SafeSetStackPointerValue(ptr, bufferLength++, path[num4 + 1 + k]);
						}
					}
					num2 = 0u;
					num = 0u;
				}
				SafeSetStackPointerValue(ptr, bufferLength++, c);
				num4 = i;
			}
			if (num3 == 0 && num2 != 0)
			{
				int num8 = num4 + 1;
				if (path[num8] != '.')
				{
					throw new ArgumentException(Environment.GetResourceString("Arg_PathIllegal"));
				}
				if (num2 >= 2)
				{
					if (flag2 && num2 > 2)
					{
						throw new ArgumentException(Environment.GetResourceString("Arg_PathIllegal"));
					}
					if (path[num8 + 1] == '.')
					{
						for (int l = num8 + 2; l < num8 + num2; l++)
						{
							if (path[l] != '.')
							{
								throw new ArgumentException(Environment.GetResourceString("Arg_PathIllegal"));
							}
						}
						num2 = 2u;
					}
					else
					{
						if (num2 > 1)
						{
							throw new ArgumentException(Environment.GetResourceString("Arg_PathIllegal"));
						}
						num2 = 1u;
					}
				}
				if (num2 == 2)
				{
					SafeSetStackPointerValue(ptr, bufferLength++, '.');
				}
				SafeSetStackPointerValue(ptr, bufferLength++, '.');
			}
			if (bufferLength == 0)
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_PathIllegal"));
			}
			SafeSetStackPointerValue(ptr, bufferLength, '\0');
			if (fullCheck && (CharArrayStartsWithOrdinal(ptr, bufferLength, "http:", ignoreCase: false) || CharArrayStartsWithOrdinal(ptr, bufferLength, "file:", ignoreCase: false)))
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_PathUriFormatNotSupported"));
			}
			if (flag4)
			{
				TryExpandShortFileName(ptr, ref bufferLength, MaxPath);
			}
			int num9 = 1;
			char* ptr4;
			int bufferLength2;
			if (fullCheck)
			{
				char* ptr2 = (char*)stackalloc byte[2 * (MaxPath + 1)];
				num9 = Win32Native.GetFullPathName(ptr, MaxPath + 1, ptr2, IntPtr.Zero);
				if (num9 > MaxPath)
				{
					char* ptr3 = (char*)stackalloc byte[2 * num9];
					ptr2 = ptr3;
					num9 = Win32Native.GetFullPathName(ptr, num9, ptr2, IntPtr.Zero);
				}
				if (num9 >= MaxPath)
				{
					throw new PathTooLongException(Environment.GetResourceString("IO.PathTooLong"));
				}
				if (num9 == 0 && *ptr != 0)
				{
					__Error.WinIOError();
				}
				else if (num9 < MaxPath)
				{
					ptr2[num9] = '\0';
				}
				ptr4 = ptr2;
				bufferLength2 = num9;
				flag4 = false;
				for (uint num10 = 0u; num10 < bufferLength2; num10++)
				{
					if (flag4)
					{
						break;
					}
					if (ptr2[num10] == '~')
					{
						flag4 = true;
					}
				}
				if (flag4 && !TryExpandShortFileName(ptr2, ref bufferLength2, MaxPath))
				{
					int bufferLength3 = -1;
					for (int num11 = bufferLength2 - 1; num11 >= 0; num11--)
					{
						if (ptr2[num11] == DirectorySeparatorChar)
						{
							bufferLength3 = num11;
							break;
						}
					}
					if (bufferLength3 >= 0)
					{
						if (bufferLength2 >= MaxPath)
						{
							throw new PathTooLongException(Environment.GetResourceString("IO.PathTooLong"));
						}
						int num12 = bufferLength2 - bufferLength3 - 1;
						char* ptr5 = (char*)stackalloc byte[2 * num12];
						Buffer.memcpy(ptr2, bufferLength3 + 1, ptr5, 0, num12);
						SafeSetStackPointerValue(ptr2, bufferLength3, '\0');
						bool flag5 = TryExpandShortFileName(ptr2, ref bufferLength3, MaxPath);
						SafeSetStackPointerValue(ptr2, bufferLength3, DirectorySeparatorChar);
						if (bufferLength3 + 1 + num12 >= MaxPath)
						{
							throw new PathTooLongException(Environment.GetResourceString("IO.PathTooLong"));
						}
						Buffer.memcpy(ptr5, 0, ptr2, bufferLength3 + 1, num12);
						if (flag5)
						{
							bufferLength2 = bufferLength3 + 1 + num12;
						}
					}
				}
			}
			else
			{
				ptr4 = ptr;
				bufferLength2 = bufferLength;
			}
			if (num9 != 0 && *ptr4 == '\\' && ptr4[1] == '\\')
			{
				int m;
				for (m = 2; m < num9; m++)
				{
					if (ptr4[m] == '\\')
					{
						m++;
						break;
					}
				}
				if (m == num9)
				{
					throw new ArgumentException(Environment.GetResourceString("Arg_PathIllegalUNC"));
				}
				if (CharArrayStartsWithOrdinal(ptr4, bufferLength2, "\\\\?\\globalroot", ignoreCase: true))
				{
					throw new ArgumentException(Environment.GetResourceString("Arg_PathGlobalRoot"));
				}
			}
			if (bufferLength2 >= MaxPath)
			{
				throw new PathTooLongException(Environment.GetResourceString("IO.PathTooLong"));
			}
			if (num9 == 0)
			{
				int num13 = Marshal.GetLastWin32Error();
				if (num13 == 0)
				{
					num13 = 161;
				}
				__Error.WinIOError(num13, path);
				return null;
			}
			return new string(ptr4, 0, bufferLength2);
		}

		private unsafe static bool CharArrayStartsWithOrdinal(char* array, int numChars, string compareTo, bool ignoreCase)
		{
			if (numChars < compareTo.Length)
			{
				return false;
			}
			if (ignoreCase)
			{
				string value = new string(array, 0, compareTo.Length);
				return compareTo.Equals(value, StringComparison.OrdinalIgnoreCase);
			}
			for (int i = 0; i < compareTo.Length; i++)
			{
				if (array[i] != compareTo[i])
				{
					return false;
				}
			}
			return true;
		}

		private unsafe static bool TryExpandShortFileName(char* buffer, ref int bufferLength, int maxBufferSize)
		{
			char* ptr = (char*)stackalloc byte[2 * (MaxPath + 1)];
			int longPathName = Win32Native.GetLongPathName(buffer, ptr, MaxPath);
			if (longPathName >= MaxPath)
			{
				throw new PathTooLongException(Environment.GetResourceString("IO.PathTooLong"));
			}
			if (longPathName == 0)
			{
				return false;
			}
			Buffer.memcpy(ptr, 0, buffer, 0, longPathName);
			bufferLength = longPathName;
			buffer[bufferLength] = '\0';
			return true;
		}

		internal static string FixupPath(string path)
		{
			return NormalizePath(path, fullCheck: false);
		}

		public static string GetFileName(string path)
		{
			if (path != null)
			{
				CheckInvalidPathChars(path);
				int length = path.Length;
				int num = length;
				while (--num >= 0)
				{
					char c = path[num];
					if (c == DirectorySeparatorChar || c == AltDirectorySeparatorChar || c == VolumeSeparatorChar)
					{
						return path.Substring(num + 1, length - num - 1);
					}
				}
			}
			return path;
		}

		public static string GetFileNameWithoutExtension(string path)
		{
			path = GetFileName(path);
			if (path != null)
			{
				int length;
				if ((length = path.LastIndexOf('.')) == -1)
				{
					return path;
				}
				return path.Substring(0, length);
			}
			return null;
		}

		public static string GetPathRoot(string path)
		{
			if (path == null)
			{
				return null;
			}
			path = FixupPath(path);
			return path.Substring(0, GetRootLength(path));
		}

		public static string GetTempPath()
		{
			new EnvironmentPermission(PermissionState.Unrestricted).Demand();
			StringBuilder stringBuilder = new StringBuilder(260);
			uint tempPath = Win32Native.GetTempPath(260, stringBuilder);
			string path = stringBuilder.ToString();
			if (tempPath == 0)
			{
				__Error.WinIOError();
			}
			return GetFullPathInternal(path);
		}

		public static string GetRandomFileName()
		{
			byte[] array = new byte[10];
			RNGCryptoServiceProvider rNGCryptoServiceProvider = new RNGCryptoServiceProvider();
			rNGCryptoServiceProvider.GetBytes(array);
			char[] array2 = System.IO.IsolatedStorage.IsolatedStorage.ToBase32StringSuitableForDirName(array).ToCharArray();
			array2[8] = '.';
			return new string(array2, 0, 12);
		}

		public static string GetTempFileName()
		{
			string tempPath = GetTempPath();
			new FileIOPermission(FileIOPermissionAccess.Write, tempPath).Demand();
			StringBuilder stringBuilder = new StringBuilder(260);
			if (Win32Native.GetTempFileName(tempPath, "tmp", 0u, stringBuilder) == 0)
			{
				__Error.WinIOError();
			}
			return stringBuilder.ToString();
		}

		public static bool HasExtension(string path)
		{
			if (path != null)
			{
				CheckInvalidPathChars(path);
				int num = path.Length;
				while (--num >= 0)
				{
					char c = path[num];
					if (c == '.')
					{
						if (num != path.Length - 1)
						{
							return true;
						}
						return false;
					}
					if (c == DirectorySeparatorChar || c == AltDirectorySeparatorChar || c == VolumeSeparatorChar)
					{
						break;
					}
				}
			}
			return false;
		}

		public static bool IsPathRooted(string path)
		{
			if (path != null)
			{
				CheckInvalidPathChars(path);
				int length = path.Length;
				if ((length >= 1 && (path[0] == DirectorySeparatorChar || path[0] == AltDirectorySeparatorChar)) || (length >= 2 && path[1] == VolumeSeparatorChar))
				{
					return true;
				}
			}
			return false;
		}

		public static string Combine(string path1, string path2)
		{
			if (path1 == null || path2 == null)
			{
				throw new ArgumentNullException((path1 == null) ? "path1" : "path2");
			}
			CheckInvalidPathChars(path1);
			CheckInvalidPathChars(path2);
			if (path2.Length == 0)
			{
				return path1;
			}
			if (path1.Length == 0)
			{
				return path2;
			}
			if (IsPathRooted(path2))
			{
				return path2;
			}
			char c = path1[path1.Length - 1];
			if (c != DirectorySeparatorChar && c != AltDirectorySeparatorChar && c != VolumeSeparatorChar)
			{
				return path1 + DirectorySeparatorChar + path2;
			}
			return path1 + path2;
		}

		internal static void CheckSearchPattern(string searchPattern)
		{
			if ((Environment.OSInfo & Environment.OSName.Win9x) != 0 && CanPathCircumventSecurityNative(searchPattern))
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_InvalidSearchPattern"));
			}
			int num;
			while ((num = searchPattern.IndexOf("..", StringComparison.Ordinal)) != -1)
			{
				if (num + 2 == searchPattern.Length)
				{
					throw new ArgumentException(Environment.GetResourceString("Arg_InvalidSearchPattern"));
				}
				if (searchPattern[num + 2] == DirectorySeparatorChar || searchPattern[num + 2] == AltDirectorySeparatorChar)
				{
					throw new ArgumentException(Environment.GetResourceString("Arg_InvalidSearchPattern"));
				}
				searchPattern = searchPattern.Substring(num + 2);
			}
		}

		internal static void CheckInvalidPathChars(string path)
		{
			foreach (int num in path)
			{
				if (num == 34 || num == 60 || num == 62 || num == 124 || num < 32)
				{
					throw new ArgumentException(Environment.GetResourceString("Argument_InvalidPathChars"));
				}
			}
		}

		internal static string InternalCombine(string path1, string path2)
		{
			if (path1 == null || path2 == null)
			{
				throw new ArgumentNullException((path1 == null) ? "path1" : "path2");
			}
			CheckInvalidPathChars(path1);
			CheckInvalidPathChars(path2);
			if (path2.Length == 0)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_PathEmpty"), "path2");
			}
			if (IsPathRooted(path2))
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_Path2IsRooted"), "path2");
			}
			int length = path1.Length;
			if (length == 0)
			{
				return path2;
			}
			char c = path1[length - 1];
			if (c != DirectorySeparatorChar && c != AltDirectorySeparatorChar && c != VolumeSeparatorChar)
			{
				return path1 + DirectorySeparatorChar + path2;
			}
			return path1 + path2;
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern bool CanPathCircumventSecurityNative(string partOfPath);
	}
}
