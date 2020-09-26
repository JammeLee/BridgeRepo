using System.IO;
using System.Runtime.InteropServices;

namespace System.Security.Cryptography
{
	[ComVisible(true)]
	public abstract class HashAlgorithm : ICryptoTransform, IDisposable
	{
		protected int HashSizeValue;

		protected internal byte[] HashValue;

		protected int State;

		private bool m_bDisposed;

		public virtual int HashSize => HashSizeValue;

		public virtual byte[] Hash
		{
			get
			{
				if (m_bDisposed)
				{
					throw new ObjectDisposedException(null, Environment.GetResourceString("ObjectDisposed_Generic"));
				}
				if (State != 0)
				{
					throw new CryptographicUnexpectedOperationException(Environment.GetResourceString("Cryptography_HashNotYetFinalized"));
				}
				return (byte[])HashValue.Clone();
			}
		}

		public virtual int InputBlockSize => 1;

		public virtual int OutputBlockSize => 1;

		public virtual bool CanTransformMultipleBlocks => true;

		public virtual bool CanReuseTransform => true;

		public static HashAlgorithm Create()
		{
			return Create("System.Security.Cryptography.HashAlgorithm");
		}

		public static HashAlgorithm Create(string hashName)
		{
			return (HashAlgorithm)CryptoConfig.CreateFromName(hashName);
		}

		public byte[] ComputeHash(Stream inputStream)
		{
			if (m_bDisposed)
			{
				throw new ObjectDisposedException(null, Environment.GetResourceString("ObjectDisposed_Generic"));
			}
			byte[] array = new byte[4096];
			int num;
			do
			{
				num = inputStream.Read(array, 0, 4096);
				if (num > 0)
				{
					HashCore(array, 0, num);
				}
			}
			while (num > 0);
			HashValue = HashFinal();
			byte[] result = (byte[])HashValue.Clone();
			Initialize();
			return result;
		}

		public byte[] ComputeHash(byte[] buffer)
		{
			if (m_bDisposed)
			{
				throw new ObjectDisposedException(null, Environment.GetResourceString("ObjectDisposed_Generic"));
			}
			if (buffer == null)
			{
				throw new ArgumentNullException("buffer");
			}
			HashCore(buffer, 0, buffer.Length);
			HashValue = HashFinal();
			byte[] result = (byte[])HashValue.Clone();
			Initialize();
			return result;
		}

		public byte[] ComputeHash(byte[] buffer, int offset, int count)
		{
			if (m_bDisposed)
			{
				throw new ObjectDisposedException(null, Environment.GetResourceString("ObjectDisposed_Generic"));
			}
			if (buffer == null)
			{
				throw new ArgumentNullException("buffer");
			}
			if (offset < 0)
			{
				throw new ArgumentOutOfRangeException("offset", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
			}
			if (count < 0 || count > buffer.Length)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_InvalidValue"));
			}
			if (buffer.Length - count < offset)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_InvalidOffLen"));
			}
			HashCore(buffer, offset, count);
			HashValue = HashFinal();
			byte[] result = (byte[])HashValue.Clone();
			Initialize();
			return result;
		}

		public int TransformBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
		{
			if (m_bDisposed)
			{
				throw new ObjectDisposedException(null, Environment.GetResourceString("ObjectDisposed_Generic"));
			}
			if (inputBuffer == null)
			{
				throw new ArgumentNullException("inputBuffer");
			}
			if (inputOffset < 0)
			{
				throw new ArgumentOutOfRangeException("inputOffset", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
			}
			if (inputCount < 0 || inputCount > inputBuffer.Length)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_InvalidValue"));
			}
			if (inputBuffer.Length - inputCount < inputOffset)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_InvalidOffLen"));
			}
			State = 1;
			HashCore(inputBuffer, inputOffset, inputCount);
			if (outputBuffer != null && (inputBuffer != outputBuffer || inputOffset != outputOffset))
			{
				Buffer.BlockCopy(inputBuffer, inputOffset, outputBuffer, outputOffset, inputCount);
			}
			return inputCount;
		}

		public byte[] TransformFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount)
		{
			if (m_bDisposed)
			{
				throw new ObjectDisposedException(null, Environment.GetResourceString("ObjectDisposed_Generic"));
			}
			if (inputBuffer == null)
			{
				throw new ArgumentNullException("inputBuffer");
			}
			if (inputOffset < 0)
			{
				throw new ArgumentOutOfRangeException("inputOffset", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
			}
			if (inputCount < 0 || inputCount > inputBuffer.Length)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_InvalidValue"));
			}
			if (inputBuffer.Length - inputCount < inputOffset)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_InvalidOffLen"));
			}
			HashCore(inputBuffer, inputOffset, inputCount);
			HashValue = HashFinal();
			byte[] array = new byte[inputCount];
			if (inputCount != 0)
			{
				Buffer.InternalBlockCopy(inputBuffer, inputOffset, array, 0, inputCount);
			}
			State = 0;
			return array;
		}

		void IDisposable.Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}

		public void Clear()
		{
			((IDisposable)this).Dispose();
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (HashValue != null)
				{
					Array.Clear(HashValue, 0, HashValue.Length);
				}
				HashValue = null;
				m_bDisposed = true;
			}
		}

		public abstract void Initialize();

		protected abstract void HashCore(byte[] array, int ibStart, int cbSize);

		protected abstract byte[] HashFinal();
	}
}
