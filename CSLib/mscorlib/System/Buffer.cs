using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System
{
	[ComVisible(true)]
	public static class Buffer
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		public static extern void BlockCopy(Array src, int srcOffset, Array dst, int dstOffset, int count);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern void InternalBlockCopy(Array src, int srcOffset, Array dst, int dstOffset, int count);

		internal unsafe static int IndexOfByte(byte* src, byte value, int index, int count)
		{
			byte* ptr;
			for (ptr = src + index; ((uint)(int)ptr & 3u) != 0; ptr++)
			{
				if (count == 0)
				{
					return -1;
				}
				if (*ptr == value)
				{
					return (int)(ptr - src);
				}
				count--;
			}
			uint num = (uint)((value << 8) + value);
			num = (num << 16) + num;
			while (count > 3)
			{
				uint num2 = *(uint*)ptr;
				num2 ^= num;
				uint num3 = 2130640639 + num2;
				num2 ^= 0xFFFFFFFFu;
				num2 ^= num3;
				if ((num2 & 0x81010100u) != 0)
				{
					int num4 = (int)(ptr - src);
					if (*ptr == value)
					{
						return num4;
					}
					if (ptr[1] == value)
					{
						return num4 + 1;
					}
					if (ptr[2] == value)
					{
						return num4 + 2;
					}
					if (ptr[3] == value)
					{
						return num4 + 3;
					}
				}
				count -= 4;
				ptr += 4;
			}
			while (count > 0)
			{
				if (*ptr == value)
				{
					return (int)(ptr - src);
				}
				count--;
				ptr++;
			}
			return -1;
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		public static extern byte GetByte(Array array, int index);

		[MethodImpl(MethodImplOptions.InternalCall)]
		public static extern void SetByte(Array array, int index, byte value);

		[MethodImpl(MethodImplOptions.InternalCall)]
		public static extern int ByteLength(Array array);

		internal unsafe static void ZeroMemory(byte* src, long len)
		{
			while (len-- > 0)
			{
				src[len] = 0;
			}
		}

		internal unsafe static void memcpy(byte* src, int srcIndex, byte[] dest, int destIndex, int len)
		{
			if (len != 0)
			{
				fixed (byte* ptr = dest)
				{
					memcpyimpl(src + srcIndex, ptr + destIndex, len);
				}
			}
		}

		internal unsafe static void memcpy(byte[] src, int srcIndex, byte* pDest, int destIndex, int len)
		{
			if (len != 0)
			{
				fixed (byte* ptr = src)
				{
					memcpyimpl(ptr + srcIndex, pDest + destIndex, len);
				}
			}
		}

		internal unsafe static void memcpy(char* pSrc, int srcIndex, char* pDest, int destIndex, int len)
		{
			if (len != 0)
			{
				memcpyimpl((byte*)(pSrc + srcIndex), (byte*)(pDest + destIndex), len * 2);
			}
		}

		internal unsafe static void memcpyimpl(byte* src, byte* dest, int len)
		{
			if (len >= 16)
			{
				do
				{
					*(int*)dest = *(int*)src;
					*(int*)(dest + 4) = *(int*)(src + 4);
					*(int*)(dest + 8) = *(int*)(src + 8);
					*(int*)(dest + 12) = *(int*)(src + 12);
					dest += 16;
					src += 16;
				}
				while ((len -= 16) >= 16);
			}
			if (len > 0)
			{
				if (((uint)len & 8u) != 0)
				{
					*(int*)dest = *(int*)src;
					*(int*)(dest + 4) = *(int*)(src + 4);
					dest += 8;
					src += 8;
				}
				if (((uint)len & 4u) != 0)
				{
					*(int*)dest = *(int*)src;
					dest += 4;
					src += 4;
				}
				if (((uint)len & 2u) != 0)
				{
					*(short*)dest = *(short*)src;
					dest += 2;
					src += 2;
				}
				if (((uint)len & (true ? 1u : 0u)) != 0)
				{
					*(dest++) = *(src++);
				}
			}
		}
	}
}
