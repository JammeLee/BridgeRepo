using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;

namespace System.Security.Cryptography
{
	[ComVisible(true)]
	public class Rfc2898DeriveBytes : DeriveBytes
	{
		private const int BlockSize = 20;

		private byte[] m_buffer;

		private byte[] m_salt;

		private HMACSHA1 m_hmacsha1;

		private uint m_iterations;

		private uint m_block;

		private int m_startIndex;

		private int m_endIndex;

		public int IterationCount
		{
			get
			{
				return (int)m_iterations;
			}
			set
			{
				if (value <= 0)
				{
					throw new ArgumentOutOfRangeException("value", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
				}
				m_iterations = (uint)value;
				Initialize();
			}
		}

		public byte[] Salt
		{
			get
			{
				return (byte[])m_salt.Clone();
			}
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException("value");
				}
				if (value.Length < 8)
				{
					throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Cryptography_PasswordDerivedBytes_FewBytesSalt")));
				}
				m_salt = (byte[])value.Clone();
				Initialize();
			}
		}

		public Rfc2898DeriveBytes(string password, int saltSize)
			: this(password, saltSize, 1000)
		{
		}

		public Rfc2898DeriveBytes(string password, int saltSize, int iterations)
		{
			if (saltSize < 0)
			{
				throw new ArgumentOutOfRangeException("saltSize", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
			}
			byte[] array = new byte[saltSize];
			Utils.StaticRandomNumberGenerator.GetBytes(array);
			Salt = array;
			IterationCount = iterations;
			m_hmacsha1 = new HMACSHA1(new UTF8Encoding(encoderShouldEmitUTF8Identifier: false).GetBytes(password));
			Initialize();
		}

		public Rfc2898DeriveBytes(string password, byte[] salt)
			: this(password, salt, 1000)
		{
		}

		public Rfc2898DeriveBytes(string password, byte[] salt, int iterations)
			: this(new UTF8Encoding(encoderShouldEmitUTF8Identifier: false).GetBytes(password), salt, iterations)
		{
		}

		public Rfc2898DeriveBytes(byte[] password, byte[] salt, int iterations)
		{
			Salt = salt;
			IterationCount = iterations;
			m_hmacsha1 = new HMACSHA1(password);
			Initialize();
		}

		public override byte[] GetBytes(int cb)
		{
			if (cb <= 0)
			{
				throw new ArgumentOutOfRangeException("cb", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
			}
			byte[] array = new byte[cb];
			int i = 0;
			int num = m_endIndex - m_startIndex;
			if (num > 0)
			{
				if (cb < num)
				{
					Buffer.InternalBlockCopy(m_buffer, m_startIndex, array, 0, cb);
					m_startIndex += cb;
					return array;
				}
				Buffer.InternalBlockCopy(m_buffer, m_startIndex, array, 0, num);
				m_startIndex = (m_endIndex = 0);
				i += num;
			}
			for (; i < cb; i += 20)
			{
				byte[] src = Func();
				int num2 = cb - i;
				if (num2 > 20)
				{
					Buffer.InternalBlockCopy(src, 0, array, i, 20);
					continue;
				}
				Buffer.InternalBlockCopy(src, 0, array, i, num2);
				i += num2;
				Buffer.InternalBlockCopy(src, num2, m_buffer, m_startIndex, 20 - num2);
				m_endIndex += 20 - num2;
				return array;
			}
			return array;
		}

		public override void Reset()
		{
			Initialize();
		}

		private void Initialize()
		{
			if (m_buffer != null)
			{
				Array.Clear(m_buffer, 0, m_buffer.Length);
			}
			m_buffer = new byte[20];
			m_block = 1u;
			m_startIndex = (m_endIndex = 0);
		}

		private byte[] Func()
		{
			byte[] array = Utils.Int(m_block);
			m_hmacsha1.TransformBlock(m_salt, 0, m_salt.Length, m_salt, 0);
			m_hmacsha1.TransformFinalBlock(array, 0, array.Length);
			byte[] array2 = m_hmacsha1.Hash;
			m_hmacsha1.Initialize();
			byte[] array3 = array2;
			for (int i = 2; i <= m_iterations; i++)
			{
				array2 = m_hmacsha1.ComputeHash(array2);
				for (int j = 0; j < 20; j++)
				{
					array3[j] ^= array2[j];
				}
			}
			m_block++;
			return array3;
		}
	}
}
