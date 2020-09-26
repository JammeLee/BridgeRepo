namespace System.Net
{
	internal static class ChunkParse
	{
		internal static int SkipPastCRLF(IReadChunkBytes Source)
		{
			int num = 0;
			bool flag = false;
			bool flag2 = false;
			bool flag3 = false;
			bool flag4 = false;
			int nextByte = Source.NextByte;
			num++;
			while (nextByte != -1)
			{
				if (flag3)
				{
					if (nextByte != 10)
					{
						return 0;
					}
					if (flag)
					{
						return 0;
					}
					if (!flag2)
					{
						return num;
					}
					flag4 = true;
					flag = true;
					flag3 = false;
				}
				else if (flag4)
				{
					if (nextByte != 32 && nextByte != 9)
					{
						return 0;
					}
					flag = true;
					flag4 = false;
				}
				if (!flag)
				{
					switch (nextByte)
					{
					case 34:
						flag2 = ((!flag2) ? true : false);
						break;
					case 92:
						if (flag2)
						{
							flag = true;
						}
						break;
					case 13:
						flag3 = true;
						break;
					case 10:
						return 0;
					}
				}
				else
				{
					flag = false;
				}
				nextByte = Source.NextByte;
				num++;
			}
			return -1;
		}

		internal static int GetChunkSize(IReadChunkBytes Source, out int chunkSize)
		{
			int num = 0;
			int nextByte = Source.NextByte;
			int num2 = 0;
			if (nextByte == 10 || nextByte == 13)
			{
				num2++;
				nextByte = Source.NextByte;
			}
			while (true)
			{
				switch (nextByte)
				{
				case 48:
				case 49:
				case 50:
				case 51:
				case 52:
				case 53:
				case 54:
				case 55:
				case 56:
				case 57:
					nextByte -= 48;
					break;
				default:
					if (nextByte >= 97 && nextByte <= 102)
					{
						nextByte -= 97;
					}
					else
					{
						if (nextByte < 65 || nextByte > 70)
						{
							Source.NextByte = nextByte;
							chunkSize = num;
							return num2;
						}
						nextByte -= 65;
					}
					nextByte += 10;
					break;
				case -1:
					chunkSize = num;
					return -1;
				}
				num *= 16;
				num += nextByte;
				num2++;
				nextByte = Source.NextByte;
			}
		}
	}
}
