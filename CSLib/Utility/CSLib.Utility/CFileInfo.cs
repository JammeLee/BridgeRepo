using System.IO;

namespace CSLib.Utility
{
	public class CFileInfo
	{
		public static void RemoveFileReadOnly(string fileName)
		{
			if ((File.GetAttributes(fileName) & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
			{
				File.SetAttributes(fileName, FileAttributes.Normal);
			}
		}
	}
}
