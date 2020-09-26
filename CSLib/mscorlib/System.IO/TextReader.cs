using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Text;

namespace System.IO
{
	[Serializable]
	[ComVisible(true)]
	public abstract class TextReader : MarshalByRefObject, IDisposable
	{
		[Serializable]
		private sealed class NullTextReader : TextReader
		{
			public override int Read(char[] buffer, int index, int count)
			{
				return 0;
			}

			public override string ReadLine()
			{
				return null;
			}
		}

		[Serializable]
		internal sealed class SyncTextReader : TextReader
		{
			internal TextReader _in;

			internal SyncTextReader(TextReader t)
			{
				_in = t;
			}

			[MethodImpl(MethodImplOptions.Synchronized)]
			public override void Close()
			{
				_in.Close();
			}

			[MethodImpl(MethodImplOptions.Synchronized)]
			protected override void Dispose(bool disposing)
			{
				if (disposing)
				{
					((IDisposable)_in).Dispose();
				}
			}

			[MethodImpl(MethodImplOptions.Synchronized)]
			public override int Peek()
			{
				return _in.Peek();
			}

			[MethodImpl(MethodImplOptions.Synchronized)]
			public override int Read()
			{
				return _in.Read();
			}

			[MethodImpl(MethodImplOptions.Synchronized)]
			public override int Read([In][Out] char[] buffer, int index, int count)
			{
				return _in.Read(buffer, index, count);
			}

			[MethodImpl(MethodImplOptions.Synchronized)]
			public override int ReadBlock([In][Out] char[] buffer, int index, int count)
			{
				return _in.ReadBlock(buffer, index, count);
			}

			[MethodImpl(MethodImplOptions.Synchronized)]
			public override string ReadLine()
			{
				return _in.ReadLine();
			}

			[MethodImpl(MethodImplOptions.Synchronized)]
			public override string ReadToEnd()
			{
				return _in.ReadToEnd();
			}
		}

		public static readonly TextReader Null = new NullTextReader();

		public virtual void Close()
		{
			Dispose(disposing: true);
		}

		public void Dispose()
		{
			Dispose(disposing: true);
		}

		protected virtual void Dispose(bool disposing)
		{
		}

		public virtual int Peek()
		{
			return -1;
		}

		public virtual int Read()
		{
			return -1;
		}

		public virtual int Read([In][Out] char[] buffer, int index, int count)
		{
			if (buffer == null)
			{
				throw new ArgumentNullException("buffer", Environment.GetResourceString("ArgumentNull_Buffer"));
			}
			if (index < 0)
			{
				throw new ArgumentOutOfRangeException("index", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
			}
			if (count < 0)
			{
				throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
			}
			if (buffer.Length - index < count)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_InvalidOffLen"));
			}
			int num = 0;
			do
			{
				int num2 = Read();
				if (num2 == -1)
				{
					break;
				}
				buffer[index + num++] = (char)num2;
			}
			while (num < count);
			return num;
		}

		public virtual string ReadToEnd()
		{
			char[] array = new char[4096];
			StringBuilder stringBuilder = new StringBuilder(4096);
			int charCount;
			while ((charCount = Read(array, 0, array.Length)) != 0)
			{
				stringBuilder.Append(array, 0, charCount);
			}
			return stringBuilder.ToString();
		}

		public virtual int ReadBlock([In][Out] char[] buffer, int index, int count)
		{
			int num = 0;
			int num2;
			do
			{
				num += (num2 = Read(buffer, index + num, count - num));
			}
			while (num2 > 0 && num < count);
			return num;
		}

		public virtual string ReadLine()
		{
			StringBuilder stringBuilder = new StringBuilder();
			while (true)
			{
				int num = Read();
				switch (num)
				{
				case 10:
				case 13:
					if (num == 13 && Peek() == 10)
					{
						Read();
					}
					return stringBuilder.ToString();
				case -1:
					if (stringBuilder.Length > 0)
					{
						return stringBuilder.ToString();
					}
					return null;
				}
				stringBuilder.Append((char)num);
			}
		}

		[HostProtection(SecurityAction.LinkDemand, Synchronization = true)]
		public static TextReader Synchronized(TextReader reader)
		{
			if (reader == null)
			{
				throw new ArgumentNullException("reader");
			}
			if (reader is SyncTextReader)
			{
				return reader;
			}
			return new SyncTextReader(reader);
		}
	}
}
