using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Text;
using System.Threading;

namespace System.IO
{
	[Serializable]
	[ComVisible(true)]
	public abstract class TextWriter : MarshalByRefObject, IDisposable
	{
		[Serializable]
		private sealed class NullTextWriter : TextWriter
		{
			public override Encoding Encoding => Encoding.Default;

			internal NullTextWriter()
				: base(CultureInfo.InvariantCulture)
			{
			}

			public override void Write(char[] buffer, int index, int count)
			{
			}

			public override void Write(string value)
			{
			}

			public override void WriteLine()
			{
			}

			public override void WriteLine(string value)
			{
			}

			public override void WriteLine(object value)
			{
			}
		}

		[Serializable]
		internal sealed class SyncTextWriter : TextWriter, IDisposable
		{
			private TextWriter _out;

			public override Encoding Encoding => _out.Encoding;

			public override IFormatProvider FormatProvider => _out.FormatProvider;

			public override string NewLine
			{
				[MethodImpl(MethodImplOptions.Synchronized)]
				get
				{
					return _out.NewLine;
				}
				[MethodImpl(MethodImplOptions.Synchronized)]
				set
				{
					_out.NewLine = value;
				}
			}

			internal SyncTextWriter(TextWriter t)
				: base(t.FormatProvider)
			{
				_out = t;
			}

			[MethodImpl(MethodImplOptions.Synchronized)]
			public override void Close()
			{
				_out.Close();
			}

			[MethodImpl(MethodImplOptions.Synchronized)]
			protected override void Dispose(bool disposing)
			{
				if (disposing)
				{
					((IDisposable)_out).Dispose();
				}
			}

			[MethodImpl(MethodImplOptions.Synchronized)]
			public override void Flush()
			{
				_out.Flush();
			}

			[MethodImpl(MethodImplOptions.Synchronized)]
			public override void Write(char value)
			{
				_out.Write(value);
			}

			[MethodImpl(MethodImplOptions.Synchronized)]
			public override void Write(char[] buffer)
			{
				_out.Write(buffer);
			}

			[MethodImpl(MethodImplOptions.Synchronized)]
			public override void Write(char[] buffer, int index, int count)
			{
				_out.Write(buffer, index, count);
			}

			[MethodImpl(MethodImplOptions.Synchronized)]
			public override void Write(bool value)
			{
				_out.Write(value);
			}

			[MethodImpl(MethodImplOptions.Synchronized)]
			public override void Write(int value)
			{
				_out.Write(value);
			}

			[MethodImpl(MethodImplOptions.Synchronized)]
			public override void Write(uint value)
			{
				_out.Write(value);
			}

			[MethodImpl(MethodImplOptions.Synchronized)]
			public override void Write(long value)
			{
				_out.Write(value);
			}

			[MethodImpl(MethodImplOptions.Synchronized)]
			public override void Write(ulong value)
			{
				_out.Write(value);
			}

			[MethodImpl(MethodImplOptions.Synchronized)]
			public override void Write(float value)
			{
				_out.Write(value);
			}

			[MethodImpl(MethodImplOptions.Synchronized)]
			public override void Write(double value)
			{
				_out.Write(value);
			}

			[MethodImpl(MethodImplOptions.Synchronized)]
			public override void Write(decimal value)
			{
				_out.Write(value);
			}

			[MethodImpl(MethodImplOptions.Synchronized)]
			public override void Write(string value)
			{
				_out.Write(value);
			}

			[MethodImpl(MethodImplOptions.Synchronized)]
			public override void Write(object value)
			{
				_out.Write(value);
			}

			[MethodImpl(MethodImplOptions.Synchronized)]
			public override void Write(string format, object arg0)
			{
				_out.Write(format, arg0);
			}

			[MethodImpl(MethodImplOptions.Synchronized)]
			public override void Write(string format, object arg0, object arg1)
			{
				_out.Write(format, arg0, arg1);
			}

			[MethodImpl(MethodImplOptions.Synchronized)]
			public override void Write(string format, object arg0, object arg1, object arg2)
			{
				_out.Write(format, arg0, arg1, arg2);
			}

			[MethodImpl(MethodImplOptions.Synchronized)]
			public override void Write(string format, object[] arg)
			{
				_out.Write(format, arg);
			}

			[MethodImpl(MethodImplOptions.Synchronized)]
			public override void WriteLine()
			{
				_out.WriteLine();
			}

			[MethodImpl(MethodImplOptions.Synchronized)]
			public override void WriteLine(char value)
			{
				_out.WriteLine(value);
			}

			[MethodImpl(MethodImplOptions.Synchronized)]
			public override void WriteLine(decimal value)
			{
				_out.WriteLine(value);
			}

			[MethodImpl(MethodImplOptions.Synchronized)]
			public override void WriteLine(char[] buffer)
			{
				_out.WriteLine(buffer);
			}

			[MethodImpl(MethodImplOptions.Synchronized)]
			public override void WriteLine(char[] buffer, int index, int count)
			{
				_out.WriteLine(buffer, index, count);
			}

			[MethodImpl(MethodImplOptions.Synchronized)]
			public override void WriteLine(bool value)
			{
				_out.WriteLine(value);
			}

			[MethodImpl(MethodImplOptions.Synchronized)]
			public override void WriteLine(int value)
			{
				_out.WriteLine(value);
			}

			[MethodImpl(MethodImplOptions.Synchronized)]
			public override void WriteLine(uint value)
			{
				_out.WriteLine(value);
			}

			[MethodImpl(MethodImplOptions.Synchronized)]
			public override void WriteLine(long value)
			{
				_out.WriteLine(value);
			}

			[MethodImpl(MethodImplOptions.Synchronized)]
			public override void WriteLine(ulong value)
			{
				_out.WriteLine(value);
			}

			[MethodImpl(MethodImplOptions.Synchronized)]
			public override void WriteLine(float value)
			{
				_out.WriteLine(value);
			}

			[MethodImpl(MethodImplOptions.Synchronized)]
			public override void WriteLine(double value)
			{
				_out.WriteLine(value);
			}

			[MethodImpl(MethodImplOptions.Synchronized)]
			public override void WriteLine(string value)
			{
				_out.WriteLine(value);
			}

			[MethodImpl(MethodImplOptions.Synchronized)]
			public override void WriteLine(object value)
			{
				_out.WriteLine(value);
			}

			[MethodImpl(MethodImplOptions.Synchronized)]
			public override void WriteLine(string format, object arg0)
			{
				_out.WriteLine(format, arg0);
			}

			[MethodImpl(MethodImplOptions.Synchronized)]
			public override void WriteLine(string format, object arg0, object arg1)
			{
				_out.WriteLine(format, arg0, arg1);
			}

			[MethodImpl(MethodImplOptions.Synchronized)]
			public override void WriteLine(string format, object arg0, object arg1, object arg2)
			{
				_out.WriteLine(format, arg0, arg1, arg2);
			}

			[MethodImpl(MethodImplOptions.Synchronized)]
			public override void WriteLine(string format, object[] arg)
			{
				_out.WriteLine(format, arg);
			}
		}

		private const string InitialNewLine = "\r\n";

		public static readonly TextWriter Null = new NullTextWriter();

		protected char[] CoreNewLine = new char[2]
		{
			'\r',
			'\n'
		};

		private IFormatProvider InternalFormatProvider;

		public virtual IFormatProvider FormatProvider
		{
			get
			{
				if (InternalFormatProvider == null)
				{
					return Thread.CurrentThread.CurrentCulture;
				}
				return InternalFormatProvider;
			}
		}

		public abstract Encoding Encoding
		{
			get;
		}

		public virtual string NewLine
		{
			get
			{
				return new string(CoreNewLine);
			}
			set
			{
				if (value == null)
				{
					value = "\r\n";
				}
				CoreNewLine = value.ToCharArray();
			}
		}

		protected TextWriter()
		{
			InternalFormatProvider = null;
		}

		protected TextWriter(IFormatProvider formatProvider)
		{
			InternalFormatProvider = formatProvider;
		}

		public virtual void Close()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
		}

		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}

		public virtual void Flush()
		{
		}

		[HostProtection(SecurityAction.LinkDemand, Synchronization = true)]
		public static TextWriter Synchronized(TextWriter writer)
		{
			if (writer == null)
			{
				throw new ArgumentNullException("writer");
			}
			if (writer is SyncTextWriter)
			{
				return writer;
			}
			return new SyncTextWriter(writer);
		}

		public virtual void Write(char value)
		{
		}

		public virtual void Write(char[] buffer)
		{
			if (buffer != null)
			{
				Write(buffer, 0, buffer.Length);
			}
		}

		public virtual void Write(char[] buffer, int index, int count)
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
			for (int i = 0; i < count; i++)
			{
				Write(buffer[index + i]);
			}
		}

		public virtual void Write(bool value)
		{
			Write(value ? "True" : "False");
		}

		public virtual void Write(int value)
		{
			Write(value.ToString(FormatProvider));
		}

		[CLSCompliant(false)]
		public virtual void Write(uint value)
		{
			Write(value.ToString(FormatProvider));
		}

		public virtual void Write(long value)
		{
			Write(value.ToString(FormatProvider));
		}

		[CLSCompliant(false)]
		public virtual void Write(ulong value)
		{
			Write(value.ToString(FormatProvider));
		}

		public virtual void Write(float value)
		{
			Write(value.ToString(FormatProvider));
		}

		public virtual void Write(double value)
		{
			Write(value.ToString(FormatProvider));
		}

		public virtual void Write(decimal value)
		{
			Write(value.ToString(FormatProvider));
		}

		public virtual void Write(string value)
		{
			if (value != null)
			{
				Write(value.ToCharArray());
			}
		}

		public virtual void Write(object value)
		{
			if (value != null)
			{
				IFormattable formattable = value as IFormattable;
				if (formattable != null)
				{
					Write(formattable.ToString(null, FormatProvider));
				}
				else
				{
					Write(value.ToString());
				}
			}
		}

		public virtual void Write(string format, object arg0)
		{
			Write(string.Format(FormatProvider, format, arg0));
		}

		public virtual void Write(string format, object arg0, object arg1)
		{
			Write(string.Format(FormatProvider, format, arg0, arg1));
		}

		public virtual void Write(string format, object arg0, object arg1, object arg2)
		{
			Write(string.Format(FormatProvider, format, arg0, arg1, arg2));
		}

		public virtual void Write(string format, params object[] arg)
		{
			Write(string.Format(FormatProvider, format, arg));
		}

		public virtual void WriteLine()
		{
			Write(CoreNewLine);
		}

		public virtual void WriteLine(char value)
		{
			Write(value);
			WriteLine();
		}

		public virtual void WriteLine(char[] buffer)
		{
			Write(buffer);
			WriteLine();
		}

		public virtual void WriteLine(char[] buffer, int index, int count)
		{
			Write(buffer, index, count);
			WriteLine();
		}

		public virtual void WriteLine(bool value)
		{
			Write(value);
			WriteLine();
		}

		public virtual void WriteLine(int value)
		{
			Write(value);
			WriteLine();
		}

		[CLSCompliant(false)]
		public virtual void WriteLine(uint value)
		{
			Write(value);
			WriteLine();
		}

		public virtual void WriteLine(long value)
		{
			Write(value);
			WriteLine();
		}

		[CLSCompliant(false)]
		public virtual void WriteLine(ulong value)
		{
			Write(value);
			WriteLine();
		}

		public virtual void WriteLine(float value)
		{
			Write(value);
			WriteLine();
		}

		public virtual void WriteLine(double value)
		{
			Write(value);
			WriteLine();
		}

		public virtual void WriteLine(decimal value)
		{
			Write(value);
			WriteLine();
		}

		public virtual void WriteLine(string value)
		{
			if (value == null)
			{
				WriteLine();
				return;
			}
			int length = value.Length;
			int num = CoreNewLine.Length;
			char[] array = new char[length + num];
			value.CopyTo(0, array, 0, length);
			switch (num)
			{
			case 2:
				array[length] = CoreNewLine[0];
				array[length + 1] = CoreNewLine[1];
				break;
			case 1:
				array[length] = CoreNewLine[0];
				break;
			default:
				Buffer.InternalBlockCopy(CoreNewLine, 0, array, length * 2, num * 2);
				break;
			}
			Write(array, 0, length + num);
		}

		public virtual void WriteLine(object value)
		{
			if (value == null)
			{
				WriteLine();
				return;
			}
			IFormattable formattable = value as IFormattable;
			if (formattable != null)
			{
				WriteLine(formattable.ToString(null, FormatProvider));
			}
			else
			{
				WriteLine(value.ToString());
			}
		}

		public virtual void WriteLine(string format, object arg0)
		{
			WriteLine(string.Format(FormatProvider, format, arg0));
		}

		public virtual void WriteLine(string format, object arg0, object arg1)
		{
			WriteLine(string.Format(FormatProvider, format, arg0, arg1));
		}

		public virtual void WriteLine(string format, object arg0, object arg1, object arg2)
		{
			WriteLine(string.Format(FormatProvider, format, arg0, arg1, arg2));
		}

		public virtual void WriteLine(string format, params object[] arg)
		{
			WriteLine(string.Format(FormatProvider, format, arg));
		}
	}
}
