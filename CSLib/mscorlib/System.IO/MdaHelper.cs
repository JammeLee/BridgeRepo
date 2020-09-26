namespace System.IO
{
	internal sealed class MdaHelper
	{
		private StreamWriter streamWriter;

		private string allocatedCallstack;

		internal MdaHelper(StreamWriter sw, string cs)
		{
			streamWriter = sw;
			allocatedCallstack = cs;
		}

		~MdaHelper()
		{
			if (streamWriter.charPos != 0 && (streamWriter.stream != null && streamWriter.stream != Stream.Null))
			{
				string text = ((streamWriter.stream is FileStream) ? ((FileStream)streamWriter.stream).NameInternal : "<unknown>");
				string text2 = Environment.GetResourceString("IO_StreamWriterBufferedDataLost", streamWriter.stream.GetType().FullName, text);
				if (allocatedCallstack != null)
				{
					string text3 = text2;
					text2 = text3 + Environment.NewLine + Environment.GetResourceString("AllocatedFrom") + Environment.NewLine + allocatedCallstack;
				}
				Mda.StreamWriterBufferedDataLost(text2);
			}
		}
	}
}
