using System;
using System.IO;
using System.Text;

namespace CSLib.Utility
{
	public class CStream
	{
		private CBuffer ᜀ = new CBuffer();

		public byte[] Buffer => ᜀ.Buffer;

		public int Length => ᜀ.ReadSize;

		public int ReadIndex => ᜀ.ReadIndex;

		public int WriteIndex => ᜀ.WriteIndex;

		public void Reset()
		{
			ᜀ.Reset();
		}

		public bool Write(ref bool bVal)
		{
			//Discarded unreachable code: IL_001d
			try
			{
				bool result = default(bool);
				while (true)
				{
					byte data = 0;
					if (true)
					{
					}
					int num = 1;
					while (true)
					{
						switch (num)
						{
						case 1:
							if (bVal)
							{
								num = 0;
								continue;
							}
							goto case 3;
						case 0:
							data = 1;
							num = 3;
							continue;
						case 3:
							result = ᜀ.WriteByte(ref data);
							num = 2;
							continue;
						case 2:
							return result;
						}
						break;
					}
				}
			}
			catch (Exception)
			{
				return false;
			}
		}

		public bool Write(ref byte btVal)
		{
			//Discarded unreachable code: IL_0017
			bool result;
			try
			{
				result = ᜀ.WriteByte(ref btVal);
			}
			catch (Exception)
			{
				result = false;
			}
			if (true)
			{
			}
			return result;
		}

		public bool Write(ref short sVal)
		{
			//Discarded unreachable code: IL_0023
			bool result;
			try
			{
				byte[] bytes = BitConverter.GetBytes(sVal);
				result = ᜀ.Write(bytes, 0, bytes.Length);
			}
			catch (Exception)
			{
				result = false;
			}
			if (true)
			{
			}
			return result;
		}

		public bool Write(ref ushort sVal)
		{
			//Discarded unreachable code: IL_0023
			bool result;
			try
			{
				byte[] bytes = BitConverter.GetBytes(sVal);
				result = ᜀ.Write(bytes, 0, bytes.Length);
			}
			catch (Exception)
			{
				result = false;
			}
			if (true)
			{
			}
			return result;
		}

		public bool Write(ref int iVal)
		{
			//Discarded unreachable code: IL_0003
			try
			{
				if (true)
				{
				}
				byte[] bytes = BitConverter.GetBytes(iVal);
				return ᜀ.Write(bytes, 0, bytes.Length);
			}
			catch (Exception)
			{
				return false;
			}
		}

		public bool Write(ref uint iVal)
		{
			//Discarded unreachable code: IL_0023
			bool result;
			try
			{
				byte[] bytes = BitConverter.GetBytes(iVal);
				result = ᜀ.Write(bytes, 0, bytes.Length);
			}
			catch (Exception)
			{
				result = false;
			}
			if (true)
			{
			}
			return result;
		}

		public bool Write(ref long lVal)
		{
			//Discarded unreachable code: IL_0023
			bool result;
			try
			{
				byte[] bytes = BitConverter.GetBytes(lVal);
				result = ᜀ.Write(bytes, 0, bytes.Length);
			}
			catch (Exception)
			{
				result = false;
			}
			if (true)
			{
			}
			return result;
		}

		public bool Write(ref ulong lVal)
		{
			//Discarded unreachable code: IL_0003
			try
			{
				if (true)
				{
				}
				byte[] bytes = BitConverter.GetBytes(lVal);
				return ᜀ.Write(bytes, 0, bytes.Length);
			}
			catch (Exception)
			{
				return false;
			}
		}

		public bool Write(ref float fVal)
		{
			//Discarded unreachable code: IL_0003
			try
			{
				if (true)
				{
				}
				byte[] bytes = BitConverter.GetBytes(fVal);
				return ᜀ.Write(bytes, 0, bytes.Length);
			}
			catch (Exception)
			{
				return false;
			}
		}

		public bool Write(ref double dVal)
		{
			//Discarded unreachable code: IL_0023
			bool result;
			try
			{
				byte[] bytes = BitConverter.GetBytes(dVal);
				result = ᜀ.Write(bytes, 0, bytes.Length);
			}
			catch (Exception)
			{
				result = false;
			}
			if (true)
			{
			}
			return result;
		}

		public bool Write(ref string strVal)
		{
			//Discarded unreachable code: IL_00b6
			bool result = default(bool);
			try
			{
				byte[] bytes = default(byte[]);
				while (true)
				{
					int iVal = 0;
					int num = 2;
					while (true)
					{
						switch (num)
						{
						case 2:
							if (strVal.Length == 0)
							{
								num = 6;
								continue;
							}
							bytes = Encoding.UTF8.GetBytes(strVal);
							iVal = bytes.Length;
							num = 0;
							continue;
						case 6:
							result = Write(ref iVal);
							num = 5;
							continue;
						case 5:
							goto end_IL_0000;
						case 3:
							result = false;
							num = 1;
							continue;
						case 1:
							goto end_IL_0000;
						case 0:
							if (!Write(ref iVal))
							{
								num = 3;
								continue;
							}
							ᜀ.Write(bytes, 0, iVal);
							num = 4;
							continue;
						case 4:
							goto IL_00b1;
						}
						break;
					}
				}
				end_IL_0000:;
			}
			catch (Exception)
			{
				result = false;
			}
			if (true)
			{
			}
			return result;
			IL_00b1:
			return true;
		}

		public bool Write(ref decimal dVal)
		{
			//Discarded unreachable code: IL_003b
			while (true)
			{
				int[] bits = decimal.GetBits(dVal);
				int num = 0;
				int num2 = 2;
				while (true)
				{
					switch (num2)
					{
					case 5:
						return false;
					case 4:
						if (Write(ref bits[num]))
						{
							if (true)
							{
							}
							num++;
							num2 = 3;
						}
						else
						{
							num2 = 5;
						}
						continue;
					case 2:
					case 3:
						num2 = 1;
						continue;
					case 1:
						num2 = ((num < bits.Length) ? 4 : 0);
						continue;
					case 0:
						return true;
					}
					break;
				}
			}
		}

		public bool Write(ref DateTime dtVal)
		{
			long lVal = dtVal.ToBinary();
			return Write(ref lVal);
		}

		public bool Write(ref byte[] bufVal)
		{
			//Discarded unreachable code: IL_0095
			int num = 5;
			int iVal = default(int);
			while (true)
			{
				bool result;
				switch (num)
				{
				default:
					if (bufVal == null)
					{
						num = 1;
						break;
					}
					goto case 3;
				case 2:
					try
					{
						ᜀ.Write(bufVal, 0, iVal);
					}
					catch (Exception)
					{
						result = false;
						goto IL_0092;
					}
					return true;
				case 3:
					iVal = bufVal.Length;
					num = 4;
					break;
				case 4:
					num = (Write(ref iVal) ? 2 : 0);
					break;
				case 1:
					bufVal = new byte[1];
					num = 3;
					break;
				case 0:
					{
						return false;
					}
					IL_0092:
					if (true)
					{
					}
					return result;
				}
			}
		}

		public bool Read(ref bool bVal)
		{
			//Discarded unreachable code: IL_001f
			bool result = default(bool);
			while (true)
			{
				if (true)
				{
				}
				byte data = 0;
				int num = 2;
				while (true)
				{
					switch (num)
					{
					case 1:
						if (data != 1)
						{
							bVal = false;
							num = 0;
						}
						else
						{
							num = 3;
						}
						continue;
					case 2:
						try
						{
							num = 3;
							while (true)
							{
								switch (num)
								{
								default:
									num = (ᜀ.ReadByte(ref data) ? 1 : 2);
									continue;
								case 2:
									result = false;
									num = 0;
									continue;
								case 1:
									break;
								case 0:
									return result;
								}
								break;
							}
						}
						catch (Exception)
						{
							return false;
						}
						num = 1;
						continue;
					case 0:
					case 4:
						return true;
					case 3:
						bVal = true;
						num = 4;
						continue;
					}
					break;
				}
			}
		}

		public bool Read(ref byte btVal)
		{
			//Discarded unreachable code: IL_0017
			bool result;
			try
			{
				result = ᜀ.ReadByte(ref btVal);
			}
			catch (Exception)
			{
				result = false;
			}
			if (true)
			{
			}
			return result;
		}

		public bool Read(ref short sVal)
		{
			//Discarded unreachable code: IL_004f
			try
			{
				bool result = default(bool);
				while (true)
				{
					byte[] array = new byte[2];
					int num = 0;
					while (true)
					{
						switch (num)
						{
						case 0:
							if (ᜀ.Read(array, 0, 2) != 2)
							{
								num = 1;
								continue;
							}
							sVal = BitConverter.ToInt16(array, 0);
							num = 3;
							continue;
						case 1:
							result = false;
							num = 2;
							continue;
						case 2:
							if (1 == 0)
							{
								return result;
							}
							return result;
						case 3:
							goto end_IL_0000;
						}
						break;
					}
				}
				end_IL_0000:;
			}
			catch (Exception)
			{
				return false;
			}
			return true;
		}

		public bool Read(ref int iVal)
		{
			//Discarded unreachable code: IL_006b
			bool result = default(bool);
			try
			{
				while (true)
				{
					byte[] array = new byte[4];
					int num = 2;
					while (true)
					{
						switch (num)
						{
						case 2:
							if (ᜀ.Read(array, 0, 4) != 4)
							{
								num = 3;
								continue;
							}
							iVal = BitConverter.ToInt32(array, 0);
							num = 0;
							continue;
						case 3:
							result = false;
							num = 1;
							continue;
						case 1:
							goto end_IL_0000;
						case 0:
							goto IL_0066;
						}
						break;
					}
				}
				end_IL_0000:;
			}
			catch (Exception)
			{
				result = false;
			}
			if (true)
			{
			}
			return result;
			IL_0066:
			return true;
		}

		public bool Read(ref long lVal)
		{
			//Discarded unreachable code: IL_006b
			bool result = default(bool);
			try
			{
				while (true)
				{
					byte[] array = new byte[8];
					int num = 2;
					while (true)
					{
						switch (num)
						{
						case 2:
							if (ᜀ.Read(array, 0, 8) != 8)
							{
								num = 3;
								continue;
							}
							lVal = BitConverter.ToInt64(array, 0);
							num = 0;
							continue;
						case 3:
							result = false;
							num = 1;
							continue;
						case 1:
							goto end_IL_0000;
						case 0:
							goto IL_0066;
						}
						break;
					}
				}
				end_IL_0000:;
			}
			catch (Exception)
			{
				result = false;
			}
			if (true)
			{
			}
			return result;
			IL_0066:
			return true;
		}

		public bool Read(ref ushort sVal)
		{
			//Discarded unreachable code: IL_006a
			try
			{
				bool result = default(bool);
				while (true)
				{
					byte[] array = new byte[2];
					int num = 3;
					while (true)
					{
						switch (num)
						{
						case 3:
							if (ᜀ.Read(array, 0, 2) != 2)
							{
								num = 2;
								continue;
							}
							sVal = (ushort)BitConverter.ToInt16(array, 0);
							num = 1;
							continue;
						case 2:
							result = false;
							num = 0;
							continue;
						case 1:
							goto end_IL_0000;
						case 0:
							return result;
						}
						break;
					}
				}
				end_IL_0000:;
			}
			catch (Exception)
			{
				return false;
			}
			if (true)
			{
			}
			return true;
		}

		public bool Read(ref uint iVal)
		{
			//Discarded unreachable code: IL_006b
			bool result = default(bool);
			try
			{
				while (true)
				{
					byte[] array = new byte[4];
					int num = 3;
					while (true)
					{
						switch (num)
						{
						case 3:
							if (ᜀ.Read(array, 0, 4) != 4)
							{
								num = 0;
								continue;
							}
							iVal = (uint)BitConverter.ToInt32(array, 0);
							num = 1;
							continue;
						case 0:
							result = false;
							num = 2;
							continue;
						case 2:
							goto end_IL_0000;
						case 1:
							goto IL_0066;
						}
						break;
					}
				}
				end_IL_0000:;
			}
			catch (Exception)
			{
				result = false;
			}
			if (true)
			{
			}
			return result;
			IL_0066:
			return true;
		}

		public bool Read(ref ulong lVal)
		{
			//Discarded unreachable code: IL_0003
			try
			{
				if (true)
				{
				}
				bool result = default(bool);
				while (true)
				{
					byte[] array = new byte[8];
					int num = 2;
					while (true)
					{
						switch (num)
						{
						case 2:
							if (ᜀ.Read(array, 0, 8) != 8)
							{
								num = 0;
								continue;
							}
							lVal = (ulong)BitConverter.ToInt64(array, 0);
							num = 3;
							continue;
						case 0:
							result = false;
							num = 1;
							continue;
						case 3:
							goto end_IL_0000;
						case 1:
							return result;
						}
						break;
					}
				}
				end_IL_0000:;
			}
			catch (Exception)
			{
				return false;
			}
			return true;
		}

		public bool Read(ref float fVal)
		{
			//Discarded unreachable code: IL_0069
			try
			{
				bool result = default(bool);
				while (true)
				{
					byte[] array = new byte[4];
					int num = 3;
					while (true)
					{
						switch (num)
						{
						case 3:
							if (ᜀ.Read(array, 0, 4) != 4)
							{
								num = 1;
								continue;
							}
							fVal = BitConverter.ToSingle(array, 0);
							num = 2;
							continue;
						case 1:
							result = false;
							num = 0;
							continue;
						case 2:
							goto end_IL_0000;
						case 0:
							return result;
						}
						break;
					}
				}
				end_IL_0000:;
			}
			catch (Exception)
			{
				return false;
			}
			if (true)
			{
			}
			return true;
		}

		public bool Read(ref double dVal)
		{
			//Discarded unreachable code: IL_0069
			try
			{
				bool result = default(bool);
				while (true)
				{
					byte[] array = new byte[8];
					int num = 0;
					while (true)
					{
						switch (num)
						{
						case 0:
							if (ᜀ.Read(array, 0, 8) != 8)
							{
								num = 3;
								continue;
							}
							dVal = BitConverter.ToDouble(array, 0);
							num = 2;
							continue;
						case 3:
							result = false;
							num = 1;
							continue;
						case 2:
							goto end_IL_0000;
						case 1:
							return result;
						}
						break;
					}
				}
				end_IL_0000:;
			}
			catch (Exception)
			{
				return false;
			}
			if (true)
			{
			}
			return true;
		}

		public bool Read(ref string strVal)
		{
			//Discarded unreachable code: IL_00ae
			bool result = default(bool);
			while (true)
			{
				int iVal = 0;
				int num = 3;
				while (true)
				{
					switch (num)
					{
					case 3:
						num = (Read(ref iVal) ? 2 : 0);
						continue;
					case 1:
						try
						{
							while (true)
							{
								byte[] array = new byte[iVal];
								num = 2;
								while (true)
								{
									switch (num)
									{
									case 2:
										if (ᜀ.Read(array, 0, iVal) != iVal)
										{
											num = 0;
											continue;
										}
										strVal = Encoding.UTF8.GetString(array);
										num = 3;
										continue;
									case 0:
										result = false;
										num = 1;
										continue;
									case 3:
										goto end_IL_0040;
									case 1:
										return result;
									}
									break;
								}
							}
							end_IL_0040:;
						}
						catch (Exception)
						{
							return false;
						}
						return true;
					case 2:
						if (iVal != 0)
						{
							if (true)
							{
							}
							num = 1;
						}
						else
						{
							num = 4;
						}
						continue;
					case 0:
						return false;
					case 4:
						strVal = "";
						return true;
					}
					break;
				}
			}
		}

		public bool Read(ref decimal dVal)
		{
			//Discarded unreachable code: IL_0067
			try
			{
				bool result = default(bool);
				while (true)
				{
					int[] array = new int[4];
					int num = 0;
					int num2 = 7;
					while (true)
					{
						switch (num2)
						{
						case 0:
							if (!Read(ref array[num]))
							{
								num2 = 3;
								continue;
							}
							num++;
							num2 = 4;
							continue;
						case 4:
						case 7:
							num2 = 1;
							continue;
						case 1:
							if (true)
							{
							}
							num2 = ((num >= 4) ? 2 : 0);
							continue;
						case 3:
							result = false;
							num2 = 5;
							continue;
						case 2:
							dVal = new decimal(array);
							num2 = 6;
							continue;
						case 6:
							goto end_IL_0000;
						case 5:
							return result;
						}
						break;
					}
				}
				end_IL_0000:;
			}
			catch (Exception)
			{
				return false;
			}
			return true;
		}

		public bool Read(ref DateTime dtVal)
		{
			//Discarded unreachable code: IL_0003
			if (true)
			{
			}
			while (true)
			{
				long lVal = 0L;
				int num = 2;
				while (true)
				{
					switch (num)
					{
					case 2:
						num = (Read(ref lVal) ? 1 : 0);
						continue;
					case 1:
						try
						{
							dtVal = DateTime.FromBinary(lVal);
						}
						catch (Exception)
						{
							return false;
						}
						return true;
					case 0:
						return false;
					}
					break;
				}
			}
		}

		public bool Read(ref byte[] bufVal)
		{
			//Discarded unreachable code: IL_0021
			bool result = default(bool);
			while (true)
			{
				int iVal = 0;
				int num = 2;
				while (true)
				{
					switch (num)
					{
					case 2:
						if (true)
						{
						}
						num = (Read(ref iVal) ? 1 : 0);
						continue;
					case 1:
						try
						{
							while (true)
							{
								bufVal = new byte[iVal];
								num = 1;
								while (true)
								{
									switch (num)
									{
									case 1:
										if (iVal == 0)
										{
											num = 0;
											continue;
										}
										ᜀ.Read(bufVal, 0, iVal);
										num = 3;
										continue;
									case 0:
										result = true;
										num = 2;
										continue;
									case 3:
										goto end_IL_003a;
									case 2:
										return result;
									}
									break;
								}
							}
							end_IL_003a:;
						}
						catch (Exception)
						{
							return false;
						}
						return true;
					case 0:
						return false;
					}
					break;
				}
			}
		}

		public void Write(bool bVal)
		{
			Write(ref bVal);
		}

		public void Write(byte btVal)
		{
			Write(ref btVal);
		}

		public void Write(short sVal)
		{
			Write(ref sVal);
		}

		public void Write(int iVal)
		{
			Write(ref iVal);
		}

		public void Write(long lVal)
		{
			Write(ref lVal);
		}

		public void Write(ushort sVal)
		{
			Write(ref sVal);
		}

		public void Write(uint iVal)
		{
			Write(ref iVal);
		}

		public void Write(ulong lVal)
		{
			Write(ref lVal);
		}

		public void Write(float fVal)
		{
			Write(ref fVal);
		}

		public void Write(double dVal)
		{
			Write(ref dVal);
		}

		public void Write(string strVal)
		{
			Write(ref strVal);
		}

		public void Write(decimal dVal)
		{
			Write(ref dVal);
		}

		public void Write(DateTime dtVal)
		{
			Write(ref dtVal);
		}

		public void Write(byte[] bufVal)
		{
			Write(ref bufVal);
		}

		public bool ReadBool()
		{
			bool bVal = false;
			Read(ref bVal);
			return bVal;
		}

		public byte ReadByte()
		{
			byte btVal = 0;
			Read(ref btVal);
			return btVal;
		}

		public short ReadShort()
		{
			short sVal = 0;
			Read(ref sVal);
			return sVal;
		}

		public int ReadInt()
		{
			int iVal = 0;
			Read(ref iVal);
			return iVal;
		}

		public long ReadLong()
		{
			long lVal = 0L;
			Read(ref lVal);
			return lVal;
		}

		public ushort ReadUShort()
		{
			ushort sVal = 0;
			Read(ref sVal);
			return sVal;
		}

		public uint ReadUInt()
		{
			uint iVal = 0u;
			Read(ref iVal);
			return iVal;
		}

		public ulong ReadULong()
		{
			ulong lVal = 0uL;
			Read(ref lVal);
			return lVal;
		}

		public float ReadSingle()
		{
			float fVal = 0f;
			Read(ref fVal);
			return fVal;
		}

		public double ReadDouble()
		{
			double dVal = 0.0;
			Read(ref dVal);
			return dVal;
		}

		public string ReadString()
		{
			string strVal = "";
			Read(ref strVal);
			return strVal;
		}

		public decimal ReadDecimal()
		{
			decimal dVal = default(decimal);
			Read(ref dVal);
			return dVal;
		}

		public DateTime ReadDateTime()
		{
			DateTime dtVal = default(DateTime);
			Read(ref dtVal);
			return dtVal;
		}

		public byte[] ReadBytes()
		{
			byte[] bufVal = new byte[0];
			Read(ref bufVal);
			return bufVal;
		}

		public bool Write(byte[] data, int index, int length)
		{
			//Discarded unreachable code: IL_0019
			bool result;
			try
			{
				result = ᜀ.Write(data, index, length);
			}
			catch (Exception)
			{
				result = false;
			}
			if (true)
			{
			}
			return result;
		}

		public bool Write(Stream stream, int length)
		{
			//Discarded unreachable code: IL_0003
			try
			{
				if (true)
				{
				}
				return ᜀ.Write(stream, length);
			}
			catch (Exception)
			{
				return false;
			}
		}

		public bool Write(Stream stream)
		{
			//Discarded unreachable code: IL_0017
			bool result;
			try
			{
				result = ᜀ.Write(stream);
			}
			catch (Exception)
			{
				result = false;
			}
			if (true)
			{
			}
			return result;
		}

		public int Read(byte[] buffer, int offset, int writeLen)
		{
			//Discarded unreachable code: IL_0009
			int num = 0;
			try
			{
				if (true)
				{
				}
				return ᜀ.Read(buffer, offset, writeLen);
			}
			catch (Exception)
			{
				return -1;
			}
		}

		public int Read(Stream stream)
		{
			//Discarded unreachable code: IL_0007
			int num = 0;
			try
			{
				num = ᜀ.Read(stream);
			}
			catch (Exception)
			{
				return -1;
			}
			if (true)
			{
			}
			return num;
		}

		public int Read(Stream stream, int writeLen)
		{
			//Discarded unreachable code: IL_0003
			if (true)
			{
			}
			int num = 0;
			try
			{
				return ᜀ.Read(stream, writeLen);
			}
			catch (Exception)
			{
				return -1;
			}
		}
	}
}
