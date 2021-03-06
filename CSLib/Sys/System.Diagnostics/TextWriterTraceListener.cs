using System.IO;
using System.Security.Permissions;
using System.Text;

namespace System.Diagnostics
{
	[HostProtection(SecurityAction.LinkDemand, Synchronization = true)]
	public class TextWriterTraceListener : TraceListener
	{
		internal TextWriter writer;

		private string fileName;

		public TextWriter Writer
		{
			get
			{
				EnsureWriter();
				return writer;
			}
			set
			{
				writer = value;
			}
		}

		public TextWriterTraceListener()
		{
		}

		public TextWriterTraceListener(Stream stream)
			: this(stream, string.Empty)
		{
		}

		public TextWriterTraceListener(Stream stream, string name)
			: base(name)
		{
			if (stream == null)
			{
				throw new ArgumentNullException("stream");
			}
			writer = new StreamWriter(stream);
		}

		public TextWriterTraceListener(TextWriter writer)
			: this(writer, string.Empty)
		{
		}

		public TextWriterTraceListener(TextWriter writer, string name)
			: base(name)
		{
			if (writer == null)
			{
				throw new ArgumentNullException("writer");
			}
			this.writer = writer;
		}

		public TextWriterTraceListener(string fileName)
		{
			this.fileName = fileName;
		}

		public TextWriterTraceListener(string fileName, string name)
			: base(name)
		{
			this.fileName = fileName;
		}

		public override void Close()
		{
			if (writer != null)
			{
				writer.Close();
			}
			writer = null;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				Close();
			}
		}

		public override void Flush()
		{
			if (EnsureWriter())
			{
				writer.Flush();
			}
		}

		public override void Write(string message)
		{
			if (EnsureWriter())
			{
				if (base.NeedIndent)
				{
					WriteIndent();
				}
				writer.Write(message);
			}
		}

		public override void WriteLine(string message)
		{
			if (EnsureWriter())
			{
				if (base.NeedIndent)
				{
					WriteIndent();
				}
				writer.WriteLine(message);
				base.NeedIndent = true;
			}
		}

		private static Encoding GetEncodingWithFallback(Encoding encoding)
		{
			Encoding encoding2 = (Encoding)encoding.Clone();
			encoding2.EncoderFallback = EncoderFallback.ReplacementFallback;
			encoding2.DecoderFallback = DecoderFallback.ReplacementFallback;
			return encoding2;
		}

		internal bool EnsureWriter()
		{
			bool flag = true;
			if (writer == null)
			{
				flag = false;
				if (fileName == null)
				{
					return flag;
				}
				Encoding encodingWithFallback = GetEncodingWithFallback(new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
				string path = Path.GetFullPath(fileName);
				string directoryName = Path.GetDirectoryName(path);
				string text = Path.GetFileName(path);
				for (int i = 0; i < 2; i++)
				{
					try
					{
						writer = new StreamWriter(path, append: true, encodingWithFallback, 4096);
						flag = true;
					}
					catch (IOException)
					{
						text = Guid.NewGuid().ToString() + text;
						path = Path.Combine(directoryName, text);
						continue;
					}
					catch (UnauthorizedAccessException)
					{
					}
					catch (Exception)
					{
					}
					break;
				}
				if (!flag)
				{
					fileName = null;
				}
			}
			return flag;
		}
	}
}
