using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;

namespace System.IO
{
	[Serializable]
	[ComVisible(true)]
	public class StringWriter : TextWriter
	{
		private static UnicodeEncoding m_encoding;

		private StringBuilder _sb;

		private bool _isOpen;

		public override Encoding Encoding
		{
			get
			{
				if (m_encoding == null)
				{
					m_encoding = new UnicodeEncoding(bigEndian: false, byteOrderMark: false);
				}
				return m_encoding;
			}
		}

		public StringWriter()
			: this(new StringBuilder(), CultureInfo.CurrentCulture)
		{
		}

		public StringWriter(IFormatProvider formatProvider)
			: this(new StringBuilder(), formatProvider)
		{
		}

		public StringWriter(StringBuilder sb)
			: this(sb, CultureInfo.CurrentCulture)
		{
		}

		public StringWriter(StringBuilder sb, IFormatProvider formatProvider)
			: base(formatProvider)
		{
			if (sb == null)
			{
				throw new ArgumentNullException("sb", Environment.GetResourceString("ArgumentNull_Buffer"));
			}
			_sb = sb;
			_isOpen = true;
		}

		public override void Close()
		{
			Dispose(disposing: true);
		}

		protected override void Dispose(bool disposing)
		{
			_isOpen = false;
			base.Dispose(disposing);
		}

		public virtual StringBuilder GetStringBuilder()
		{
			return _sb;
		}

		public override void Write(char value)
		{
			if (!_isOpen)
			{
				__Error.WriterClosed();
			}
			_sb.Append(value);
		}

		public override void Write(char[] buffer, int index, int count)
		{
			if (!_isOpen)
			{
				__Error.WriterClosed();
			}
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
			_sb.Append(buffer, index, count);
		}

		public override void Write(string value)
		{
			if (!_isOpen)
			{
				__Error.WriterClosed();
			}
			if (value != null)
			{
				_sb.Append(value);
			}
		}

		public override string ToString()
		{
			return _sb.ToString();
		}
	}
}
