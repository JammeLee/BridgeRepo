namespace System.Security.Policy
{
	internal static class BuiltInEvidenceHelper
	{
		internal const char idApplicationDirectory = '\0';

		internal const char idPublisher = '\u0001';

		internal const char idStrongName = '\u0002';

		internal const char idZone = '\u0003';

		internal const char idUrl = '\u0004';

		internal const char idWebPage = '\u0005';

		internal const char idSite = '\u0006';

		internal const char idPermissionRequestEvidence = '\a';

		internal const char idHash = '\b';

		internal const char idGac = '\t';

		internal static void CopyIntToCharArray(int value, char[] buffer, int position)
		{
			buffer[position] = (char)((uint)(value >> 16) & 0xFFFFu);
			buffer[position + 1] = (char)((uint)value & 0xFFFFu);
		}

		internal static int GetIntFromCharArray(char[] buffer, int position)
		{
			int num = buffer[position];
			num <<= 16;
			return num + buffer[position + 1];
		}

		internal static void CopyLongToCharArray(long value, char[] buffer, int position)
		{
			buffer[position] = (char)((value >> 48) & 0xFFFF);
			buffer[position + 1] = (char)((value >> 32) & 0xFFFF);
			buffer[position + 2] = (char)((value >> 16) & 0xFFFF);
			buffer[position + 3] = (char)(value & 0xFFFF);
		}

		internal static long GetLongFromCharArray(char[] buffer, int position)
		{
			long num = buffer[position];
			num <<= 16;
			num += buffer[position + 1];
			num <<= 16;
			num += buffer[position + 2];
			num <<= 16;
			return num + buffer[position + 3];
		}
	}
}
