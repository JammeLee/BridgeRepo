using System.Globalization;
using System.IO;
using System.Security.Permissions;
using System.Text;

namespace System.CodeDom.Compiler
{
	[PermissionSet(SecurityAction.InheritanceDemand, Name = "FullTrust")]
	[PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
	public class IndentedTextWriter : TextWriter
	{
		public const string DefaultTabString = "    ";

		private TextWriter writer;

		private int indentLevel;

		private bool tabsPending;

		private string tabString;

		public override Encoding Encoding => writer.Encoding;

		public override string NewLine
		{
			get
			{
				return writer.NewLine;
			}
			set
			{
				writer.NewLine = value;
			}
		}

		public int Indent
		{
			get
			{
				return indentLevel;
			}
			set
			{
				if (value < 0)
				{
					value = 0;
				}
				indentLevel = value;
			}
		}

		public TextWriter InnerWriter => writer;

		internal string TabString => tabString;

		public IndentedTextWriter(TextWriter writer)
			: this(writer, "    ")
		{
		}

		public IndentedTextWriter(TextWriter writer, string tabString)
			: base(CultureInfo.InvariantCulture)
		{
			this.writer = writer;
			this.tabString = tabString;
			indentLevel = 0;
			tabsPending = false;
		}

		public override void Close()
		{
			writer.Close();
		}

		public override void Flush()
		{
			writer.Flush();
		}

		protected virtual void OutputTabs()
		{
			if (tabsPending)
			{
				for (int i = 0; i < indentLevel; i++)
				{
					writer.Write(tabString);
				}
				tabsPending = false;
			}
		}

		public override void Write(string s)
		{
			OutputTabs();
			writer.Write(s);
		}

		public override void Write(bool value)
		{
			OutputTabs();
			writer.Write(value);
		}

		public override void Write(char value)
		{
			OutputTabs();
			writer.Write(value);
		}

		public override void Write(char[] buffer)
		{
			OutputTabs();
			writer.Write(buffer);
		}

		public override void Write(char[] buffer, int index, int count)
		{
			OutputTabs();
			writer.Write(buffer, index, count);
		}

		public override void Write(double value)
		{
			OutputTabs();
			writer.Write(value);
		}

		public override void Write(float value)
		{
			OutputTabs();
			writer.Write(value);
		}

		public override void Write(int value)
		{
			OutputTabs();
			writer.Write(value);
		}

		public override void Write(long value)
		{
			OutputTabs();
			writer.Write(value);
		}

		public override void Write(object value)
		{
			OutputTabs();
			writer.Write(value);
		}

		public override void Write(string format, object arg0)
		{
			OutputTabs();
			writer.Write(format, arg0);
		}

		public override void Write(string format, object arg0, object arg1)
		{
			OutputTabs();
			writer.Write(format, arg0, arg1);
		}

		public override void Write(string format, params object[] arg)
		{
			OutputTabs();
			writer.Write(format, arg);
		}

		public void WriteLineNoTabs(string s)
		{
			writer.WriteLine(s);
		}

		public override void WriteLine(string s)
		{
			OutputTabs();
			writer.WriteLine(s);
			tabsPending = true;
		}

		public override void WriteLine()
		{
			OutputTabs();
			writer.WriteLine();
			tabsPending = true;
		}

		public override void WriteLine(bool value)
		{
			OutputTabs();
			writer.WriteLine(value);
			tabsPending = true;
		}

		public override void WriteLine(char value)
		{
			OutputTabs();
			writer.WriteLine(value);
			tabsPending = true;
		}

		public override void WriteLine(char[] buffer)
		{
			OutputTabs();
			writer.WriteLine(buffer);
			tabsPending = true;
		}

		public override void WriteLine(char[] buffer, int index, int count)
		{
			OutputTabs();
			writer.WriteLine(buffer, index, count);
			tabsPending = true;
		}

		public override void WriteLine(double value)
		{
			OutputTabs();
			writer.WriteLine(value);
			tabsPending = true;
		}

		public override void WriteLine(float value)
		{
			OutputTabs();
			writer.WriteLine(value);
			tabsPending = true;
		}

		public override void WriteLine(int value)
		{
			OutputTabs();
			writer.WriteLine(value);
			tabsPending = true;
		}

		public override void WriteLine(long value)
		{
			OutputTabs();
			writer.WriteLine(value);
			tabsPending = true;
		}

		public override void WriteLine(object value)
		{
			OutputTabs();
			writer.WriteLine(value);
			tabsPending = true;
		}

		public override void WriteLine(string format, object arg0)
		{
			OutputTabs();
			writer.WriteLine(format, arg0);
			tabsPending = true;
		}

		public override void WriteLine(string format, object arg0, object arg1)
		{
			OutputTabs();
			writer.WriteLine(format, arg0, arg1);
			tabsPending = true;
		}

		public override void WriteLine(string format, params object[] arg)
		{
			OutputTabs();
			writer.WriteLine(format, arg);
			tabsPending = true;
		}

		[CLSCompliant(false)]
		public override void WriteLine(uint value)
		{
			OutputTabs();
			writer.WriteLine(value);
			tabsPending = true;
		}

		internal void InternalOutputTabs()
		{
			for (int i = 0; i < indentLevel; i++)
			{
				writer.Write(tabString);
			}
		}
	}
}
