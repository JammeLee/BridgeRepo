using System.IO;

namespace System.Net.Mime
{
	internal class QuotedPrintableStream : DelegatedStream
	{
		private class ReadStateInfo
		{
			private bool isEscaped;

			private short b1 = -1;

			internal bool IsEscaped
			{
				get
				{
					return isEscaped;
				}
				set
				{
					isEscaped = value;
				}
			}

			internal short Byte
			{
				get
				{
					return b1;
				}
				set
				{
					b1 = value;
				}
			}
		}

		internal class WriteStateInfo
		{
			private int currentLineLength;

			private byte[] buffer;

			private int length;

			internal byte[] Buffer => buffer;

			internal int CurrentLineLength
			{
				get
				{
					return currentLineLength;
				}
				set
				{
					currentLineLength = value;
				}
			}

			internal int Length
			{
				get
				{
					return length;
				}
				set
				{
					length = value;
				}
			}

			internal WriteStateInfo(int bufferSize)
			{
				buffer = new byte[bufferSize];
			}
		}

		private class WriteAsyncResult : LazyAsyncResult
		{
			private QuotedPrintableStream parent;

			private byte[] buffer;

			private int offset;

			private int count;

			private static AsyncCallback onWrite = OnWrite;

			private int written;

			internal WriteAsyncResult(QuotedPrintableStream parent, byte[] buffer, int offset, int count, AsyncCallback callback, object state)
				: base(null, state, callback)
			{
				this.parent = parent;
				this.buffer = buffer;
				this.offset = offset;
				this.count = count;
			}

			private void CompleteWrite(IAsyncResult result)
			{
				parent.BaseStream.EndWrite(result);
				parent.WriteState.Length = 0;
			}

			internal static void End(IAsyncResult result)
			{
				WriteAsyncResult writeAsyncResult = (WriteAsyncResult)result;
				writeAsyncResult.InternalWaitForCompletion();
			}

			private static void OnWrite(IAsyncResult result)
			{
				if (!result.CompletedSynchronously)
				{
					WriteAsyncResult writeAsyncResult = (WriteAsyncResult)result.AsyncState;
					try
					{
						writeAsyncResult.CompleteWrite(result);
						writeAsyncResult.Write();
					}
					catch (Exception result2)
					{
						writeAsyncResult.InvokeCallback(result2);
					}
					catch
					{
						writeAsyncResult.InvokeCallback(new Exception(SR.GetString("net_nonClsCompliantException")));
					}
				}
			}

			internal void Write()
			{
				while (true)
				{
					written += parent.EncodeBytes(buffer, offset + written, count - written);
					if (written >= count)
					{
						break;
					}
					IAsyncResult asyncResult = parent.BaseStream.BeginWrite(parent.WriteState.Buffer, 0, parent.WriteState.Length, onWrite, this);
					if (!asyncResult.CompletedSynchronously)
					{
						return;
					}
					CompleteWrite(asyncResult);
				}
				InvokeCallback();
			}
		}

		private bool encodeCRLF;

		private static int DefaultLineLength = 76;

		private static byte[] hexDecodeMap = new byte[256]
		{
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			0,
			1,
			2,
			3,
			4,
			5,
			6,
			7,
			8,
			9,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			10,
			11,
			12,
			13,
			14,
			15,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			10,
			11,
			12,
			13,
			14,
			15,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255,
			255
		};

		private static byte[] hexEncodeMap = new byte[16]
		{
			48,
			49,
			50,
			51,
			52,
			53,
			54,
			55,
			56,
			57,
			65,
			66,
			67,
			68,
			69,
			70
		};

		private int lineLength;

		private ReadStateInfo readState;

		private WriteStateInfo writeState;

		private ReadStateInfo ReadState
		{
			get
			{
				if (readState == null)
				{
					readState = new ReadStateInfo();
				}
				return readState;
			}
		}

		internal WriteStateInfo WriteState
		{
			get
			{
				if (writeState == null)
				{
					writeState = new WriteStateInfo(1024);
				}
				return writeState;
			}
		}

		internal QuotedPrintableStream(Stream stream, int lineLength)
			: base(stream)
		{
			if (lineLength < 0)
			{
				throw new ArgumentOutOfRangeException("lineLength");
			}
			this.lineLength = lineLength;
		}

		internal QuotedPrintableStream(Stream stream, bool encodeCRLF)
			: this(stream, DefaultLineLength)
		{
			this.encodeCRLF = encodeCRLF;
		}

		internal QuotedPrintableStream()
		{
			lineLength = DefaultLineLength;
		}

		internal QuotedPrintableStream(int lineLength)
		{
			this.lineLength = lineLength;
		}

		public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
		{
			if (buffer == null)
			{
				throw new ArgumentNullException("buffer");
			}
			if (offset < 0 || offset > buffer.Length)
			{
				throw new ArgumentOutOfRangeException("offset");
			}
			if (offset + count > buffer.Length)
			{
				throw new ArgumentOutOfRangeException("count");
			}
			WriteAsyncResult writeAsyncResult = new WriteAsyncResult(this, buffer, offset, count, callback, state);
			writeAsyncResult.Write();
			return writeAsyncResult;
		}

		public override void Close()
		{
			FlushInternal();
			base.Close();
		}

		internal unsafe int DecodeBytes(byte[] buffer, int offset, int count)
		{
			fixed (byte* ptr = buffer)
			{
				byte* ptr2 = ptr + offset;
				byte* ptr3 = ptr2;
				byte* ptr4 = ptr2;
				byte* ptr5 = ptr2 + count;
				if (ReadState.IsEscaped)
				{
					if (ReadState.Byte == -1)
					{
						if (count == 1)
						{
							ReadState.Byte = *ptr3;
							return 0;
						}
						if (*ptr3 != 13 || ptr3[1] != 10)
						{
							byte b = hexDecodeMap[*ptr3];
							byte b2 = hexDecodeMap[ptr3[1]];
							if (b == byte.MaxValue)
							{
								throw new FormatException(SR.GetString("InvalidHexDigit", b));
							}
							if (b2 == byte.MaxValue)
							{
								throw new FormatException(SR.GetString("InvalidHexDigit", b2));
							}
							*(ptr4++) = (byte)((b << 4) + b2);
						}
						ptr3 += 2;
					}
					else
					{
						if (ReadState.Byte != 13 || *ptr3 != 10)
						{
							byte b3 = hexDecodeMap[ReadState.Byte];
							byte b4 = hexDecodeMap[*ptr3];
							if (b3 == byte.MaxValue)
							{
								throw new FormatException(SR.GetString("InvalidHexDigit", b3));
							}
							if (b4 == byte.MaxValue)
							{
								throw new FormatException(SR.GetString("InvalidHexDigit", b4));
							}
							*(ptr4++) = (byte)((b3 << 4) + b4);
						}
						ptr3++;
					}
					ReadState.IsEscaped = false;
					ReadState.Byte = -1;
				}
				while (ptr3 < ptr5)
				{
					if (*ptr3 != 61)
					{
						*(ptr4++) = *(ptr3++);
						continue;
					}
					long num = ptr5 - ptr3;
					if (num <= 2 && num >= 1)
					{
						switch (num - 1)
						{
						case 1L:
							ReadState.Byte = ptr3[1];
							goto case 0L;
						case 0L:
							ReadState.IsEscaped = true;
							goto end_IL_02c5;
						}
					}
					if (ptr3[1] != 13 || ptr3[2] != 10)
					{
						byte b5 = hexDecodeMap[ptr3[1]];
						byte b6 = hexDecodeMap[ptr3[2]];
						if (b5 == byte.MaxValue)
						{
							throw new FormatException(SR.GetString("InvalidHexDigit", b5));
						}
						if (b6 == byte.MaxValue)
						{
							throw new FormatException(SR.GetString("InvalidHexDigit", b6));
						}
						*(ptr4++) = (byte)((b5 << 4) + b6);
					}
					ptr3 += 3;
					continue;
					end_IL_02c5:
					break;
				}
				count = (int)(ptr4 - ptr2);
				return count;
			}
		}

		internal int EncodeBytes(byte[] buffer, int offset, int count)
		{
			int i;
			for (i = offset; i < count + offset; i++)
			{
				if (lineLength != -1 && WriteState.CurrentLineLength + 5 >= lineLength && (buffer[i] == 32 || buffer[i] == 9 || buffer[i] == 13 || buffer[i] == 10))
				{
					if (WriteState.Buffer.Length - WriteState.Length < 3)
					{
						return i - offset;
					}
					WriteState.CurrentLineLength = 0;
					WriteState.Buffer[WriteState.Length++] = 61;
					WriteState.Buffer[WriteState.Length++] = 13;
					WriteState.Buffer[WriteState.Length++] = 10;
				}
				if (WriteState.CurrentLineLength == 0 && buffer[i] == 46)
				{
					WriteState.Buffer[WriteState.Length++] = 46;
				}
				if (buffer[i] == 13 && i + 1 < count + offset && buffer[i + 1] == 10)
				{
					if (WriteState.Buffer.Length - WriteState.Length < (encodeCRLF ? 6 : 2))
					{
						return i - offset;
					}
					i++;
					if (encodeCRLF)
					{
						WriteState.Buffer[WriteState.Length++] = 61;
						WriteState.Buffer[WriteState.Length++] = 48;
						WriteState.Buffer[WriteState.Length++] = 68;
						WriteState.Buffer[WriteState.Length++] = 61;
						WriteState.Buffer[WriteState.Length++] = 48;
						WriteState.Buffer[WriteState.Length++] = 65;
						WriteState.CurrentLineLength += 6;
					}
					else
					{
						WriteState.Buffer[WriteState.Length++] = 13;
						WriteState.Buffer[WriteState.Length++] = 10;
						WriteState.CurrentLineLength = 0;
					}
				}
				else if ((buffer[i] < 32 && buffer[i] != 9) || buffer[i] == 61 || buffer[i] > 126)
				{
					if (WriteState.Buffer.Length - WriteState.Length < 3)
					{
						return i - offset;
					}
					WriteState.CurrentLineLength += 3;
					WriteState.Buffer[WriteState.Length++] = 61;
					WriteState.Buffer[WriteState.Length++] = hexEncodeMap[buffer[i] >> 4];
					WriteState.Buffer[WriteState.Length++] = hexEncodeMap[buffer[i] & 0xF];
				}
				else
				{
					if (WriteState.Buffer.Length - WriteState.Length < 1)
					{
						return i - offset;
					}
					WriteState.CurrentLineLength++;
					WriteState.Buffer[WriteState.Length++] = buffer[i];
				}
			}
			return i - offset;
		}

		public override void EndWrite(IAsyncResult asyncResult)
		{
			WriteAsyncResult.End(asyncResult);
		}

		public override void Flush()
		{
			FlushInternal();
			base.Flush();
		}

		private void FlushInternal()
		{
			if (writeState != null && writeState.Length > 0)
			{
				base.Write(WriteState.Buffer, 0, WriteState.Length);
				WriteState.Length = 0;
			}
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			if (buffer == null)
			{
				throw new ArgumentNullException("buffer");
			}
			if (offset < 0 || offset > buffer.Length)
			{
				throw new ArgumentOutOfRangeException("offset");
			}
			if (offset + count > buffer.Length)
			{
				throw new ArgumentOutOfRangeException("count");
			}
			int num = 0;
			while (true)
			{
				num += EncodeBytes(buffer, offset + num, count - num);
				if (num < count)
				{
					FlushInternal();
					continue;
				}
				break;
			}
		}
	}
}
