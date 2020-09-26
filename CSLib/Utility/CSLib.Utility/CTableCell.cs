using System;

namespace CSLib.Utility
{
	public class CTableCell
	{
		protected string m_value;

		protected bool m_isValid;

		public CTableCell()
		{
			m_isValid = false;
		}

		public bool GetString(ref string value)
		{
			//Discarded unreachable code: IL_000b
			if (!m_isValid)
			{
				if (true)
				{
				}
				return false;
			}
			value = m_value;
			return true;
		}

		public bool GetBoolean(ref bool value)
		{
			//Discarded unreachable code: IL_0045
			int num = 0;
			while (true)
			{
				switch (num)
				{
				default:
					if (m_isValid)
					{
						num = 7;
						break;
					}
					goto case 1;
				case 4:
					if (true)
					{
					}
					value = true;
					num = 6;
					break;
				case 2:
					if (Convert.ToInt32(m_value) != 0)
					{
						num = 4;
						break;
					}
					value = false;
					num = 5;
					break;
				case 7:
					num = 3;
					break;
				case 3:
					num = ((m_value.Length <= 0) ? 1 : 2);
					break;
				case 1:
					return false;
				case 5:
				case 6:
					return true;
				}
			}
		}

		public bool GetDouble(ref double value)
		{
			//Discarded unreachable code: IL_004d
			int num = 0;
			while (true)
			{
				switch (num)
				{
				default:
					if (m_isValid)
					{
						num = 3;
						break;
					}
					goto case 1;
				case 1:
					return false;
				case 3:
					num = 2;
					break;
				case 2:
					if (m_value.Length <= 0)
					{
						if (true)
						{
						}
						num = 1;
						break;
					}
					value = Convert.ToDouble(m_value);
					return true;
				}
			}
		}

		public bool GetFloat(ref float value)
		{
			//Discarded unreachable code: IL_004d
			int num = 2;
			while (true)
			{
				switch (num)
				{
				default:
					if (m_isValid)
					{
						num = 0;
						break;
					}
					goto case 3;
				case 3:
					return false;
				case 0:
					num = 1;
					break;
				case 1:
					if (m_value.Length <= 0)
					{
						if (true)
						{
						}
						num = 3;
						break;
					}
					value = Convert.ToSingle(m_value);
					return true;
				}
			}
		}

		public bool GetInt32(ref int value)
		{
			//Discarded unreachable code: IL_003f
			int num = 2;
			while (true)
			{
				switch (num)
				{
				default:
					if (m_isValid)
					{
						num = 0;
						break;
					}
					goto case 1;
				case 1:
					return false;
				case 0:
					num = 3;
					break;
				case 3:
					if (true)
					{
					}
					if (m_value.Length <= 0)
					{
						num = 1;
						break;
					}
					value = Convert.ToInt32(m_value);
					return true;
				}
			}
		}

		public bool GetUInt32(ref uint value)
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
					if (m_isValid)
					{
						num = 1;
						break;
					}
					goto case 3;
				case 3:
					return false;
				case 1:
					num = 0;
					break;
				case 0:
					if (m_value.Length <= 0)
					{
						num = 3;
						break;
					}
					value = Convert.ToUInt32(m_value);
					return true;
				}
			}
		}

		public bool GetInt64(ref long value)
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
					if (m_isValid)
					{
						num = 1;
						break;
					}
					goto case 0;
				case 0:
					return false;
				case 1:
					num = 3;
					break;
				case 3:
					if (m_value.Length <= 0)
					{
						num = 0;
						break;
					}
					value = Convert.ToInt64(m_value);
					return true;
				}
			}
		}

		public bool GetUInt64(ref ulong value)
		{
			//Discarded unreachable code: IL_0037
			int num = 3;
			while (true)
			{
				switch (num)
				{
				default:
					if (m_isValid)
					{
						num = 2;
						break;
					}
					goto case 0;
				case 0:
					return false;
				case 2:
					if (true)
					{
					}
					num = 1;
					break;
				case 1:
					if (m_value.Length <= 0)
					{
						num = 0;
						break;
					}
					value = Convert.ToUInt64(m_value);
					return true;
				}
			}
		}

		public string GetString()
		{
			return m_value;
		}

		public bool GetBoolean()
		{
			//Discarded unreachable code: IL_0023
			int num = 3;
			while (true)
			{
				switch (num)
				{
				default:
					if (true)
					{
					}
					if (m_isValid)
					{
						num = 0;
						break;
					}
					goto case 1;
				case 1:
					return false;
				case 0:
					num = 2;
					break;
				case 2:
					if (m_value.Length <= 0)
					{
						num = 1;
						break;
					}
					return Convert.ToBoolean(m_value);
				}
			}
		}

		public double GetDouble()
		{
			//Discarded unreachable code: IL_0055
			int num = 3;
			while (true)
			{
				switch (num)
				{
				default:
					if (m_isValid)
					{
						num = 2;
						break;
					}
					goto case 0;
				case 0:
					return 0.0;
				case 2:
					num = 1;
					break;
				case 1:
					if (m_value.Length <= 0)
					{
						if (true)
						{
						}
						num = 0;
						break;
					}
					return Convert.ToDouble(m_value);
				}
			}
		}

		public float GetFloat()
		{
			//Discarded unreachable code: IL_0003
			if (true)
			{
			}
			int num = 3;
			while (true)
			{
				switch (num)
				{
				default:
					if (m_isValid)
					{
						num = 2;
						break;
					}
					goto case 0;
				case 0:
					return 0f;
				case 2:
					num = 1;
					break;
				case 1:
					if (m_value.Length <= 0)
					{
						num = 0;
						break;
					}
					return Convert.ToSingle(m_value);
				}
			}
		}

		public int GetInt32()
		{
			//Discarded unreachable code: IL_0023
			int num = 3;
			while (true)
			{
				switch (num)
				{
				default:
					if (true)
					{
					}
					if (m_isValid)
					{
						num = 0;
						break;
					}
					goto case 2;
				case 2:
					return 0;
				case 0:
					num = 1;
					break;
				case 1:
					if (m_value.Length <= 0)
					{
						num = 2;
						break;
					}
					return Convert.ToInt32(m_value);
				}
			}
		}

		public uint GetUInt32()
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
					if (m_isValid)
					{
						num = 2;
						break;
					}
					goto case 0;
				case 0:
					return 0u;
				case 2:
					num = 3;
					break;
				case 3:
					if (m_value.Length <= 0)
					{
						num = 0;
						break;
					}
					return Convert.ToUInt32(m_value);
				}
			}
		}

		public long GetInt64()
		{
			//Discarded unreachable code: IL_0023
			int num = 3;
			while (true)
			{
				switch (num)
				{
				default:
					if (true)
					{
					}
					if (m_isValid)
					{
						num = 1;
						break;
					}
					goto case 0;
				case 0:
					return 0L;
				case 1:
					num = 2;
					break;
				case 2:
					if (m_value.Length <= 0)
					{
						num = 0;
						break;
					}
					return Convert.ToInt64(m_value);
				}
			}
		}

		public ulong GetUInt64()
		{
			//Discarded unreachable code: IL_0033
			int num = 0;
			while (true)
			{
				switch (num)
				{
				default:
					if (m_isValid)
					{
						num = 3;
						break;
					}
					goto case 1;
				case 3:
					if (true)
					{
					}
					num = 2;
					break;
				case 1:
					return 0uL;
				case 2:
					if (m_value.Length <= 0)
					{
						num = 1;
						break;
					}
					return Convert.ToUInt64(m_value);
				}
			}
		}

		public bool SetString(string value)
		{
			m_isValid = true;
			m_value = value;
			return true;
		}

		public bool SetBoolean(bool value)
		{
			m_isValid = true;
			m_value = Convert.ToString(value);
			return true;
		}

		public bool SetDouble(double value)
		{
			m_isValid = true;
			m_value = Convert.ToString(value);
			return true;
		}

		public bool SetFloat(float value)
		{
			m_isValid = true;
			m_value = Convert.ToString(value);
			return true;
		}

		public bool SetInt32(int value)
		{
			m_isValid = true;
			m_value = Convert.ToString(value);
			return true;
		}

		public bool SetUInt32(uint value)
		{
			m_isValid = true;
			m_value = Convert.ToString(value);
			return true;
		}

		public bool SetInt64(long value)
		{
			m_isValid = true;
			m_value = Convert.ToString(value);
			return true;
		}

		public bool SetUInt64(ulong value)
		{
			m_isValid = true;
			m_value = Convert.ToString(value);
			return true;
		}

		public bool GetValue(ref string value)
		{
			return GetString(ref value);
		}

		public bool GetValue(ref bool value)
		{
			return GetBoolean(ref value);
		}

		public bool GetValue(ref double value)
		{
			return GetDouble(ref value);
		}

		public bool GetValue(ref float value)
		{
			return GetFloat(ref value);
		}

		public bool GetValue(ref sbyte value)
		{
			//Discarded unreachable code: IL_0003
			if (true)
			{
			}
			int value2 = 0;
			if (!GetInt32(ref value2))
			{
				return false;
			}
			value = (sbyte)value2;
			return true;
		}

		public bool GetValue(ref byte value)
		{
			//Discarded unreachable code: IL_0013
			uint value2 = 0u;
			if (!GetUInt32(ref value2))
			{
				return false;
			}
			if (true)
			{
			}
			value = (byte)value2;
			return true;
		}

		public bool GetValue(ref short value)
		{
			//Discarded unreachable code: IL_000f
			int value2 = 0;
			if (!GetInt32(ref value2))
			{
				if (true)
				{
				}
				return false;
			}
			value = (short)value2;
			return true;
		}

		public bool GetValue(ref ushort value)
		{
			//Discarded unreachable code: IL_0013
			uint value2 = 0u;
			if (!GetUInt32(ref value2))
			{
				return false;
			}
			if (true)
			{
			}
			value = (ushort)value2;
			return true;
		}

		public bool GetValue(ref int value)
		{
			return GetInt32(ref value);
		}

		public bool GetValue(ref uint value)
		{
			return GetUInt32(ref value);
		}

		public bool GetValue(ref long value)
		{
			return GetInt64(ref value);
		}

		public bool GetValue(ref ulong value)
		{
			return GetUInt64(ref value);
		}

		public bool SetValue(string value)
		{
			return SetString(value);
		}

		public bool SetValue(bool value)
		{
			return SetBoolean(value);
		}

		public bool SetValue(double value)
		{
			return SetDouble(value);
		}

		public bool SetValue(float value)
		{
			return SetFloat(value);
		}

		public bool SetValue(sbyte value)
		{
			return SetInt32(value);
		}

		public bool SetValue(byte value)
		{
			return SetUInt32(value);
		}

		public bool SetValue(short value)
		{
			return SetInt32(value);
		}

		public bool SetValue(ushort value)
		{
			return SetUInt32(value);
		}

		public bool SetValue(int value)
		{
			return SetInt32(value);
		}

		public bool SetValue(uint value)
		{
			return SetUInt32(value);
		}

		public bool SetValue(long value)
		{
			return SetInt64(value);
		}

		public bool SetValue(ulong value)
		{
			return SetUInt64(value);
		}
	}
}
