using System;
using System.IO;

namespace CSLib.Utility
{
	public class CBuffer
	{
		private int ᜀ;

		private int ᜁ;

		private byte[] ᜂ;

		public int WriteIndex => ᜁ;

		public int ReadIndex
		{
			get
			{
				//Discarded unreachable code: IL_0003
				if (true)
				{
				}
				while (true)
				{
					int num = ᜁ - ᜀ;
					int num2 = 2;
					while (true)
					{
						switch (num2)
						{
						case 2:
							if (num <= ᜀ)
							{
								num2 = 1;
								continue;
							}
							goto case 0;
						case 1:
							Array.Copy(ᜂ, ᜀ, ᜂ, 0, num);
							ᜀ = 0;
							ᜁ = num;
							num2 = 0;
							continue;
						case 0:
							return ᜀ;
						}
						break;
					}
				}
			}
		}

		public byte[] Buffer => ᜂ;

		public bool CanRead => ReadSize > 0;

		public int ReadSize
		{
			get
			{
				//Discarded unreachable code: IL_0011
				if (ᜁ < ᜀ)
				{
					if (true)
					{
					}
					return 0;
				}
				return ᜁ - ᜀ;
			}
		}

		public int WriteSize
		{
			get
			{
				//Discarded unreachable code: IL_0023
				int num = 1;
				while (true)
				{
					switch (num)
					{
					default:
						if (true)
						{
						}
						if (ᜂ != null)
						{
							num = 2;
							break;
						}
						goto case 3;
					case 3:
						return 0;
					case 2:
						num = 0;
						break;
					case 0:
						if (ᜂ.Length < ᜁ)
						{
							num = 3;
							break;
						}
						return ᜂ.Length - ᜁ;
					}
				}
			}
		}

		public CBuffer()
		{
			Init(8);
		}

		public CBuffer(int size)
		{
			Init(size);
		}

		public virtual bool Init(int size)
		{
			//Discarded unreachable code: IL_000b
			if (ᜂ == null)
			{
				if (true)
				{
				}
				ᜂ = new byte[size];
				return true;
			}
			return false;
		}

		public bool Write(byte[] data, int index, int length)
		{
			//Discarded unreachable code: IL_000c
			if (!WriteReserve(length))
			{
				if (true)
				{
				}
				return false;
			}
			Array.Copy(data, index, ᜂ, ᜁ, length);
			ᜁ += length;
			return true;
		}

		public bool Write(Stream stream, int length)
		{
			//Discarded unreachable code: IL_0076
			int num = 0;
			int num2 = default(int);
			while (true)
			{
				switch (num)
				{
				default:
					if (!stream.CanRead)
					{
						num = 5;
						break;
					}
					num2 = length;
					num = 1;
					break;
				case 6:
					num = 4;
					break;
				case 4:
					if (!WriteReserve(num2))
					{
						num = 3;
						break;
					}
					stream.Read(ᜂ, ᜁ, num2);
					ᜁ += num2;
					return true;
				case 2:
					num2 = length;
					num = 6;
					break;
				case 5:
					return false;
				case 3:
					return false;
				case 1:
					if (true)
					{
					}
					if (stream.Length < length)
					{
						num = 2;
						break;
					}
					goto case 6;
				}
			}
		}

		public bool Write(Stream stream)
		{
			//Discarded unreachable code: IL_0058
			int num = 3;
			int num2 = default(int);
			while (true)
			{
				switch (num)
				{
				default:
					if (!stream.CanRead)
					{
						num = 2;
						break;
					}
					num2 = (int)stream.Length;
					num = 1;
					break;
				case 1:
					if (!WriteReserve(num2))
					{
						num = 0;
						break;
					}
					stream.Read(ᜂ, ᜁ, num2);
					ᜁ += num2;
					return true;
				case 0:
					if (true)
					{
					}
					return false;
				case 2:
					return false;
				}
			}
		}

		public int Read(byte[] buffer, int offset, int readLen)
		{
			//Discarded unreachable code: IL_0048
			while (true)
			{
				int num = ReadSize;
				int num2 = 1;
				while (true)
				{
					switch (num2)
					{
					case 1:
						if (num <= 0)
						{
							num2 = 0;
							continue;
						}
						if (true)
						{
						}
						num2 = 2;
						continue;
					case 4:
						num = readLen;
						num2 = 3;
						continue;
					case 2:
						if (readLen < num)
						{
							num2 = 4;
							continue;
						}
						goto case 3;
					case 0:
						return 0;
					case 3:
						Array.Copy(ᜂ, ᜀ, buffer, offset, num);
						ReadFlip(num);
						return num;
					}
					break;
				}
			}
		}

		public int Read(Stream stream, int readLen)
		{
			//Discarded unreachable code: IL_002e
			while (true)
			{
				int num = ReadSize;
				int num2 = 4;
				while (true)
				{
					switch (num2)
					{
					case 4:
						if (true)
						{
						}
						num2 = ((num > 0) ? 1 : 0);
						continue;
					case 2:
						num = readLen;
						num2 = 3;
						continue;
					case 1:
						if (readLen < num)
						{
							num2 = 2;
							continue;
						}
						goto case 3;
					case 0:
						return 0;
					case 3:
						stream.Write(ᜂ, ᜀ, num);
						ReadFlip(num);
						return num;
					}
					break;
				}
			}
		}

		public int Read(Stream stream)
		{
			//Discarded unreachable code: IL_000e
			int readSize = ReadSize;
			if (readSize <= 0)
			{
				if (true)
				{
				}
				return 0;
			}
			stream.Write(ᜂ, ᜀ, readSize);
			ReadFlip(readSize);
			return readSize;
		}

		public bool WriteByte(ref byte data)
		{
			//Discarded unreachable code: IL_000e
			if (!WriteReserve(1))
			{
				if (true)
				{
				}
				return false;
			}
			ᜂ[ᜁ] = data;
			ᜁ++;
			return true;
		}

		public bool ReadByte(ref byte data)
		{
			//Discarded unreachable code: IL_000b
			if (!CanRead)
			{
				if (true)
				{
				}
				return false;
			}
			data = ᜂ[ᜀ];
			ᜀ++;
			return true;
		}

		public virtual bool WriteReserve(int size)
		{
			//Discarded unreachable code: IL_004b
			int num = 1;
			while (true)
			{
				switch (num)
				{
				default:
					if (WriteSize < size)
					{
						num = 0;
						continue;
					}
					break;
				case 0:
				{
					int newSize = (ᜂ.Length + size) * 2;
					Array.Resize(ref ᜂ, newSize);
					if (true)
					{
					}
					num = 2;
					continue;
				}
				case 2:
					break;
				}
				break;
			}
			return true;
		}

		public void Reset()
		{
			ᜁ = 0;
			ᜀ = 0;
		}

		public void ReadFlip(int size)
		{
			//Discarded unreachable code: IL_0017
			while (true)
			{
				if (true)
				{
				}
				ᜀ += size;
				int num = 2;
				while (true)
				{
					switch (num)
					{
					case 2:
						if (ᜀ >= ᜁ)
						{
							num = 0;
							continue;
						}
						return;
					case 0:
						ᜀ = 0;
						ᜁ = 0;
						num = 1;
						continue;
					case 1:
						return;
					}
					break;
				}
			}
		}

		public void WriteFlip(int size)
		{
			//Discarded unreachable code: IL_007a
			int num2 = default(int);
			while (true)
			{
				ᜁ += size;
				int num = 4;
				while (true)
				{
					switch (num)
					{
					case 4:
						if (ᜁ > ᜂ.Length)
						{
							num = 0;
							continue;
						}
						goto case 3;
					case 1:
						Array.Copy(ᜂ, ᜀ, ᜂ, 0, num2);
						ᜀ = 0;
						ᜁ = num2;
						if (true)
						{
						}
						num = 5;
						continue;
					case 5:
						return;
					case 3:
						num2 = ᜁ - ᜀ;
						num = 2;
						continue;
					case 2:
						if (num2 <= ᜀ)
						{
							num = 1;
							continue;
						}
						return;
					case 0:
						ᜁ = ᜂ.Length;
						num = 3;
						continue;
					}
					break;
				}
			}
		}
	}
}
