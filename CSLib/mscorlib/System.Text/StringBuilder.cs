using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Threading;

namespace System.Text
{
	[Serializable]
	[ComVisible(true)]
	public sealed class StringBuilder : ISerializable
	{
		internal const int DefaultCapacity = 16;

		private const string CapacityField = "Capacity";

		private const string MaxCapacityField = "m_MaxCapacity";

		private const string StringValueField = "m_StringValue";

		private const string ThreadIDField = "m_currentThread";

		internal IntPtr m_currentThread = Thread.InternalGetCurrentThread();

		internal int m_MaxCapacity;

		internal volatile string m_StringValue;

		public int Capacity
		{
			get
			{
				return m_StringValue.Capacity;
			}
			set
			{
				IntPtr tid;
				string threadSafeString = GetThreadSafeString(out tid);
				if (value < 0)
				{
					throw new ArgumentOutOfRangeException("value", Environment.GetResourceString("ArgumentOutOfRange_NegativeCapacity"));
				}
				if (value < threadSafeString.Length)
				{
					throw new ArgumentOutOfRangeException("value", Environment.GetResourceString("ArgumentOutOfRange_SmallCapacity"));
				}
				if (value > MaxCapacity)
				{
					throw new ArgumentOutOfRangeException("value", Environment.GetResourceString("ArgumentOutOfRange_Capacity"));
				}
				int capacity = threadSafeString.Capacity;
				if (value != capacity)
				{
					string stringForStringBuilder = string.GetStringForStringBuilder(threadSafeString, value);
					ReplaceString(tid, stringForStringBuilder);
				}
			}
		}

		public int MaxCapacity => m_MaxCapacity;

		public int Length
		{
			get
			{
				return m_StringValue.Length;
			}
			set
			{
				IntPtr tid;
				string threadSafeString = GetThreadSafeString(out tid);
				if (value == 0)
				{
					threadSafeString.SetLength(0);
					ReplaceString(tid, threadSafeString);
					return;
				}
				int length = threadSafeString.Length;
				if (value < 0)
				{
					throw new ArgumentOutOfRangeException("newlength", Environment.GetResourceString("ArgumentOutOfRange_NegativeLength"));
				}
				if (value > MaxCapacity)
				{
					throw new ArgumentOutOfRangeException("capacity", Environment.GetResourceString("ArgumentOutOfRange_SmallCapacity"));
				}
				if (value == length)
				{
					return;
				}
				if (value <= threadSafeString.Capacity)
				{
					if (value > length)
					{
						for (int i = length; i < value; i++)
						{
							threadSafeString.InternalSetCharNoBoundsCheck(i, '\0');
						}
					}
					threadSafeString.InternalSetCharNoBoundsCheck(value, '\0');
					threadSafeString.SetLength(value);
					ReplaceString(tid, threadSafeString);
				}
				else
				{
					int capacity = ((value > threadSafeString.Capacity) ? value : threadSafeString.Capacity);
					string stringForStringBuilder = string.GetStringForStringBuilder(threadSafeString, capacity);
					stringForStringBuilder.SetLength(value);
					ReplaceString(tid, stringForStringBuilder);
				}
			}
		}

		[IndexerName("Chars")]
		public char this[int index]
		{
			get
			{
				return m_StringValue[index];
			}
			set
			{
				IntPtr tid;
				string threadSafeString = GetThreadSafeString(out tid);
				threadSafeString.SetChar(index, value);
				ReplaceString(tid, threadSafeString);
			}
		}

		public StringBuilder()
			: this(16)
		{
		}

		public StringBuilder(int capacity)
			: this(string.Empty, capacity)
		{
		}

		public StringBuilder(string value)
			: this(value, 16)
		{
		}

		public StringBuilder(string value, int capacity)
			: this(value, 0, value?.Length ?? 0, capacity)
		{
		}

		public StringBuilder(string value, int startIndex, int length, int capacity)
		{
			if (capacity < 0)
			{
				throw new ArgumentOutOfRangeException("capacity", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("ArgumentOutOfRange_MustBePositive"), "capacity"));
			}
			if (length < 0)
			{
				throw new ArgumentOutOfRangeException("length", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("ArgumentOutOfRange_MustBeNonNegNum"), "length"));
			}
			if (startIndex < 0)
			{
				throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("ArgumentOutOfRange_StartIndex"));
			}
			if (value == null)
			{
				value = string.Empty;
			}
			if (startIndex > value.Length - length)
			{
				throw new ArgumentOutOfRangeException("length", Environment.GetResourceString("ArgumentOutOfRange_IndexLength"));
			}
			m_MaxCapacity = int.MaxValue;
			if (capacity == 0)
			{
				capacity = 16;
			}
			while (capacity < length)
			{
				capacity *= 2;
				if (capacity < 0)
				{
					capacity = length;
					break;
				}
			}
			m_StringValue = string.GetStringForStringBuilder(value, startIndex, length, capacity);
		}

		public StringBuilder(int capacity, int maxCapacity)
		{
			if (capacity > maxCapacity)
			{
				throw new ArgumentOutOfRangeException("capacity", Environment.GetResourceString("ArgumentOutOfRange_Capacity"));
			}
			if (maxCapacity < 1)
			{
				throw new ArgumentOutOfRangeException("maxCapacity", Environment.GetResourceString("ArgumentOutOfRange_SmallMaxCapacity"));
			}
			if (capacity < 0)
			{
				throw new ArgumentOutOfRangeException("capacity", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("ArgumentOutOfRange_MustBePositive"), "capacity"));
			}
			if (capacity == 0)
			{
				capacity = Math.Min(16, maxCapacity);
			}
			m_StringValue = string.GetStringForStringBuilder(string.Empty, capacity);
			m_MaxCapacity = maxCapacity;
		}

		private StringBuilder(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			int num = 0;
			string text = null;
			int num2 = int.MaxValue;
			bool flag = false;
			SerializationInfoEnumerator enumerator = info.GetEnumerator();
			while (enumerator.MoveNext())
			{
				switch (enumerator.Name)
				{
				case "m_MaxCapacity":
					num2 = info.GetInt32("m_MaxCapacity");
					break;
				case "m_StringValue":
					text = info.GetString("m_StringValue");
					break;
				case "Capacity":
					num = info.GetInt32("Capacity");
					flag = true;
					break;
				}
			}
			if (text == null)
			{
				text = string.Empty;
			}
			if (num2 < 1 || text.Length > num2)
			{
				throw new SerializationException(Environment.GetResourceString("Serialization_StringBuilderMaxCapacity"));
			}
			if (!flag)
			{
				num = 16;
				if (num < text.Length)
				{
					num = text.Length;
				}
				if (num > num2)
				{
					num = num2;
				}
			}
			if (num < 0 || num < text.Length || num > num2)
			{
				throw new SerializationException(Environment.GetResourceString("Serialization_StringBuilderCapacity"));
			}
			m_MaxCapacity = num2;
			m_StringValue = string.GetStringForStringBuilder(text, 0, text.Length, num);
		}

		void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			info.AddValue("m_MaxCapacity", m_MaxCapacity);
			info.AddValue("Capacity", Capacity);
			info.AddValue("m_StringValue", m_StringValue);
			info.AddValue("m_currentThread", 0);
		}

		[Conditional("_DEBUG")]
		private void VerifyClassInvariant()
		{
		}

		private string GetThreadSafeString(out IntPtr tid)
		{
			string stringValue = m_StringValue;
			tid = Thread.InternalGetCurrentThread();
			if (m_currentThread == tid)
			{
				return stringValue;
			}
			return string.GetStringForStringBuilder(stringValue, stringValue.Capacity);
		}

		public int EnsureCapacity(int capacity)
		{
			if (capacity < 0)
			{
				throw new ArgumentOutOfRangeException("capacity", Environment.GetResourceString("ArgumentOutOfRange_NeedPosCapacity"));
			}
			IntPtr tid;
			string threadSafeString = GetThreadSafeString(out tid);
			if (!NeedsAllocation(threadSafeString, capacity))
			{
				return threadSafeString.Capacity;
			}
			string newString = GetNewString(threadSafeString, capacity);
			ReplaceString(tid, newString);
			return newString.Capacity;
		}

		public override string ToString()
		{
			string stringValue = m_StringValue;
			IntPtr currentThread = m_currentThread;
			if (currentThread != Thread.InternalGetCurrentThread())
			{
				return string.InternalCopy(stringValue);
			}
			if (2 * stringValue.Length < stringValue.ArrayLength)
			{
				return string.InternalCopy(stringValue);
			}
			stringValue.ClearPostNullChar();
			m_currentThread = IntPtr.Zero;
			return stringValue;
		}

		public string ToString(int startIndex, int length)
		{
			return m_StringValue.InternalSubStringWithChecks(startIndex, length, fAlwaysCopy: true);
		}

		public StringBuilder Append(char value, int repeatCount)
		{
			if (repeatCount == 0)
			{
				return this;
			}
			if (repeatCount < 0)
			{
				throw new ArgumentOutOfRangeException("repeatCount", Environment.GetResourceString("ArgumentOutOfRange_NegativeCount"));
			}
			IntPtr tid;
			string threadSafeString = GetThreadSafeString(out tid);
			int length = threadSafeString.Length;
			int num = length + repeatCount;
			if (num < 0)
			{
				throw new OutOfMemoryException();
			}
			if (!NeedsAllocation(threadSafeString, num))
			{
				threadSafeString.AppendInPlace(value, repeatCount, length);
				ReplaceString(tid, threadSafeString);
				return this;
			}
			string newString = GetNewString(threadSafeString, num);
			newString.AppendInPlace(value, repeatCount, length);
			ReplaceString(tid, newString);
			return this;
		}

		public StringBuilder Append(char[] value, int startIndex, int charCount)
		{
			if (value == null)
			{
				if (startIndex == 0 && charCount == 0)
				{
					return this;
				}
				throw new ArgumentNullException("value");
			}
			if (charCount == 0)
			{
				return this;
			}
			if (startIndex < 0)
			{
				throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("ArgumentOutOfRange_GenericPositive"));
			}
			if (charCount < 0)
			{
				throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("ArgumentOutOfRange_GenericPositive"));
			}
			if (charCount > value.Length - startIndex)
			{
				throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("ArgumentOutOfRange_Index"));
			}
			IntPtr tid;
			string threadSafeString = GetThreadSafeString(out tid);
			int length = threadSafeString.Length;
			int requiredLength = length + charCount;
			if (NeedsAllocation(threadSafeString, requiredLength))
			{
				string newString = GetNewString(threadSafeString, requiredLength);
				newString.AppendInPlace(value, startIndex, charCount, length);
				ReplaceString(tid, newString);
			}
			else
			{
				threadSafeString.AppendInPlace(value, startIndex, charCount, length);
				ReplaceString(tid, threadSafeString);
			}
			return this;
		}

		public StringBuilder Append(string value)
		{
			if (value == null)
			{
				return this;
			}
			string text = m_StringValue;
			IntPtr intPtr = Thread.InternalGetCurrentThread();
			if (m_currentThread != intPtr)
			{
				text = string.GetStringForStringBuilder(text, text.Capacity);
			}
			int length = text.Length;
			int requiredLength = length + value.Length;
			if (NeedsAllocation(text, requiredLength))
			{
				string newString = GetNewString(text, requiredLength);
				newString.AppendInPlace(value, length);
				ReplaceString(intPtr, newString);
			}
			else
			{
				text.AppendInPlace(value, length);
				ReplaceString(intPtr, text);
			}
			return this;
		}

		internal unsafe StringBuilder Append(char* value, int count)
		{
			if (value == null)
			{
				return this;
			}
			IntPtr tid;
			string threadSafeString = GetThreadSafeString(out tid);
			int length = threadSafeString.Length;
			int requiredLength = length + count;
			if (NeedsAllocation(threadSafeString, requiredLength))
			{
				string newString = GetNewString(threadSafeString, requiredLength);
				newString.AppendInPlace(value, count, length);
				ReplaceString(tid, newString);
			}
			else
			{
				threadSafeString.AppendInPlace(value, count, length);
				ReplaceString(tid, threadSafeString);
			}
			return this;
		}

		private bool NeedsAllocation(string currentString, int requiredLength)
		{
			return currentString.ArrayLength <= requiredLength;
		}

		private string GetNewString(string currentString, int requiredLength)
		{
			int maxCapacity = m_MaxCapacity;
			if (requiredLength < 0)
			{
				throw new OutOfMemoryException();
			}
			if (requiredLength > maxCapacity)
			{
				throw new ArgumentOutOfRangeException("requiredLength", Environment.GetResourceString("ArgumentOutOfRange_SmallCapacity"));
			}
			int num = currentString.Capacity * 2;
			if (num < requiredLength)
			{
				num = requiredLength;
			}
			if (num > maxCapacity)
			{
				num = maxCapacity;
			}
			if (num <= 0)
			{
				throw new ArgumentOutOfRangeException("newCapacity", Environment.GetResourceString("ArgumentOutOfRange_NegativeCapacity"));
			}
			return string.GetStringForStringBuilder(currentString, num);
		}

		private void ReplaceString(IntPtr tid, string value)
		{
			m_currentThread = tid;
			m_StringValue = value;
		}

		public StringBuilder Append(string value, int startIndex, int count)
		{
			if (value == null)
			{
				if (startIndex == 0 && count == 0)
				{
					return this;
				}
				throw new ArgumentNullException("value");
			}
			if (count <= 0)
			{
				if (count == 0)
				{
					return this;
				}
				throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("ArgumentOutOfRange_GenericPositive"));
			}
			if (startIndex < 0 || startIndex > value.Length - count)
			{
				throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("ArgumentOutOfRange_Index"));
			}
			IntPtr tid;
			string threadSafeString = GetThreadSafeString(out tid);
			int length = threadSafeString.Length;
			int requiredLength = length + count;
			if (NeedsAllocation(threadSafeString, requiredLength))
			{
				string newString = GetNewString(threadSafeString, requiredLength);
				newString.AppendInPlace(value, startIndex, count, length);
				ReplaceString(tid, newString);
			}
			else
			{
				threadSafeString.AppendInPlace(value, startIndex, count, length);
				ReplaceString(tid, threadSafeString);
			}
			return this;
		}

		[ComVisible(false)]
		public StringBuilder AppendLine()
		{
			return Append(Environment.NewLine);
		}

		[ComVisible(false)]
		public StringBuilder AppendLine(string value)
		{
			Append(value);
			return Append(Environment.NewLine);
		}

		[ComVisible(false)]
		public unsafe void CopyTo(int sourceIndex, char[] destination, int destinationIndex, int count)
		{
			if (destination == null)
			{
				throw new ArgumentNullException("destination");
			}
			if (count < 0)
			{
				throw new ArgumentOutOfRangeException(Environment.GetResourceString("Arg_NegativeArgCount"), "count");
			}
			if (destinationIndex < 0)
			{
				throw new ArgumentOutOfRangeException(Environment.GetResourceString("ArgumentOutOfRange_MustBeNonNegNum", "destinationIndex"), "destinationIndex");
			}
			if (destinationIndex > destination.Length - count)
			{
				throw new ArgumentException(Environment.GetResourceString("ArgumentOutOfRange_OffsetOut"));
			}
			IntPtr tid;
			string threadSafeString = GetThreadSafeString(out tid);
			int length = threadSafeString.Length;
			if (sourceIndex < 0 || sourceIndex > length)
			{
				throw new ArgumentOutOfRangeException(Environment.GetResourceString("ArgumentOutOfRange_Index"));
			}
			if (sourceIndex > length - count)
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_LongerThanSrcString"));
			}
			if (count == 0)
			{
				return;
			}
			fixed (char* dest = &destination[destinationIndex])
			{
				fixed (char* ptr = threadSafeString)
				{
					char* src = ptr + sourceIndex;
					Buffer.memcpyimpl((byte*)src, (byte*)dest, count * 2);
				}
			}
		}

		public StringBuilder Insert(int index, string value, int count)
		{
			IntPtr tid;
			string threadSafeString = GetThreadSafeString(out tid);
			int length = threadSafeString.Length;
			if (index < 0 || index > length)
			{
				throw new ArgumentOutOfRangeException("index", Environment.GetResourceString("ArgumentOutOfRange_Index"));
			}
			if (count < 0)
			{
				throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
			}
			if (value == null || value.Length == 0 || count == 0)
			{
				return this;
			}
			int requiredLength;
			try
			{
				requiredLength = checked(length + value.Length * count);
			}
			catch (OverflowException)
			{
				throw new OutOfMemoryException();
			}
			if (NeedsAllocation(threadSafeString, requiredLength))
			{
				string newString = GetNewString(threadSafeString, requiredLength);
				newString.InsertInPlace(index, value, count, length, requiredLength);
				ReplaceString(tid, newString);
			}
			else
			{
				threadSafeString.InsertInPlace(index, value, count, length, requiredLength);
				ReplaceString(tid, threadSafeString);
			}
			return this;
		}

		public StringBuilder Remove(int startIndex, int length)
		{
			IntPtr tid;
			string threadSafeString = GetThreadSafeString(out tid);
			int length2 = threadSafeString.Length;
			if (length < 0)
			{
				throw new ArgumentOutOfRangeException("length", Environment.GetResourceString("ArgumentOutOfRange_NegativeLength"));
			}
			if (startIndex < 0)
			{
				throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("ArgumentOutOfRange_StartIndex"));
			}
			if (length > length2 - startIndex)
			{
				throw new ArgumentOutOfRangeException("index", Environment.GetResourceString("ArgumentOutOfRange_Index"));
			}
			threadSafeString.RemoveInPlace(startIndex, length, length2);
			ReplaceString(tid, threadSafeString);
			return this;
		}

		public StringBuilder Append(bool value)
		{
			return Append(value.ToString());
		}

		[CLSCompliant(false)]
		public StringBuilder Append(sbyte value)
		{
			return Append(value.ToString(CultureInfo.CurrentCulture));
		}

		public StringBuilder Append(byte value)
		{
			return Append(value.ToString(CultureInfo.CurrentCulture));
		}

		public StringBuilder Append(char value)
		{
			string text = m_StringValue;
			IntPtr intPtr = Thread.InternalGetCurrentThread();
			if (m_currentThread != intPtr)
			{
				text = string.GetStringForStringBuilder(text, text.Capacity);
			}
			int length = text.Length;
			if (!NeedsAllocation(text, length + 1))
			{
				text.AppendInPlace(value, length);
				ReplaceString(intPtr, text);
				return this;
			}
			string newString = GetNewString(text, length + 1);
			newString.AppendInPlace(value, length);
			ReplaceString(intPtr, newString);
			return this;
		}

		public StringBuilder Append(short value)
		{
			return Append(value.ToString(CultureInfo.CurrentCulture));
		}

		public StringBuilder Append(int value)
		{
			return Append(value.ToString(CultureInfo.CurrentCulture));
		}

		public StringBuilder Append(long value)
		{
			return Append(value.ToString(CultureInfo.CurrentCulture));
		}

		public StringBuilder Append(float value)
		{
			return Append(value.ToString(CultureInfo.CurrentCulture));
		}

		public StringBuilder Append(double value)
		{
			return Append(value.ToString(CultureInfo.CurrentCulture));
		}

		public StringBuilder Append(decimal value)
		{
			return Append(value.ToString(CultureInfo.CurrentCulture));
		}

		[CLSCompliant(false)]
		public StringBuilder Append(ushort value)
		{
			return Append(value.ToString(CultureInfo.CurrentCulture));
		}

		[CLSCompliant(false)]
		public StringBuilder Append(uint value)
		{
			return Append(value.ToString(CultureInfo.CurrentCulture));
		}

		[CLSCompliant(false)]
		public StringBuilder Append(ulong value)
		{
			return Append(value.ToString(CultureInfo.CurrentCulture));
		}

		public StringBuilder Append(object value)
		{
			if (value == null)
			{
				return this;
			}
			return Append(value.ToString());
		}

		public StringBuilder Append(char[] value)
		{
			if (value == null)
			{
				return this;
			}
			int count = value.Length;
			IntPtr tid;
			string threadSafeString = GetThreadSafeString(out tid);
			int length = threadSafeString.Length;
			int requiredLength = length + value.Length;
			if (NeedsAllocation(threadSafeString, requiredLength))
			{
				string newString = GetNewString(threadSafeString, requiredLength);
				newString.AppendInPlace(value, 0, count, length);
				ReplaceString(tid, newString);
			}
			else
			{
				threadSafeString.AppendInPlace(value, 0, count, length);
				ReplaceString(tid, threadSafeString);
			}
			return this;
		}

		public StringBuilder Insert(int index, string value)
		{
			if (value == null)
			{
				return Insert(index, value, 0);
			}
			return Insert(index, value, 1);
		}

		public StringBuilder Insert(int index, bool value)
		{
			return Insert(index, value.ToString(), 1);
		}

		[CLSCompliant(false)]
		public StringBuilder Insert(int index, sbyte value)
		{
			return Insert(index, value.ToString(CultureInfo.CurrentCulture), 1);
		}

		public StringBuilder Insert(int index, byte value)
		{
			return Insert(index, value.ToString(CultureInfo.CurrentCulture), 1);
		}

		public StringBuilder Insert(int index, short value)
		{
			return Insert(index, value.ToString(CultureInfo.CurrentCulture), 1);
		}

		public StringBuilder Insert(int index, char value)
		{
			return Insert(index, char.ToString(value), 1);
		}

		public StringBuilder Insert(int index, char[] value)
		{
			if (value == null)
			{
				return Insert(index, value, 0, 0);
			}
			return Insert(index, value, 0, value.Length);
		}

		public StringBuilder Insert(int index, char[] value, int startIndex, int charCount)
		{
			IntPtr tid;
			string threadSafeString = GetThreadSafeString(out tid);
			int length = threadSafeString.Length;
			if (index < 0 || index > length)
			{
				throw new ArgumentOutOfRangeException("index", Environment.GetResourceString("ArgumentOutOfRange_Index"));
			}
			if (value == null)
			{
				if (startIndex == 0 && charCount == 0)
				{
					return this;
				}
				throw new ArgumentNullException(Environment.GetResourceString("ArgumentNull_String"));
			}
			if (startIndex < 0)
			{
				throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("ArgumentOutOfRange_StartIndex"));
			}
			if (charCount < 0)
			{
				throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("ArgumentOutOfRange_GenericPositive"));
			}
			if (startIndex > value.Length - charCount)
			{
				throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("ArgumentOutOfRange_Index"));
			}
			if (charCount == 0)
			{
				return this;
			}
			int requiredLength = length + charCount;
			if (NeedsAllocation(threadSafeString, requiredLength))
			{
				string newString = GetNewString(threadSafeString, requiredLength);
				newString.InsertInPlace(index, value, startIndex, charCount, length, requiredLength);
				ReplaceString(tid, newString);
			}
			else
			{
				threadSafeString.InsertInPlace(index, value, startIndex, charCount, length, requiredLength);
				ReplaceString(tid, threadSafeString);
			}
			return this;
		}

		public StringBuilder Insert(int index, int value)
		{
			return Insert(index, value.ToString(CultureInfo.CurrentCulture), 1);
		}

		public StringBuilder Insert(int index, long value)
		{
			return Insert(index, value.ToString(CultureInfo.CurrentCulture), 1);
		}

		public StringBuilder Insert(int index, float value)
		{
			return Insert(index, value.ToString(CultureInfo.CurrentCulture), 1);
		}

		public StringBuilder Insert(int index, double value)
		{
			return Insert(index, value.ToString(CultureInfo.CurrentCulture), 1);
		}

		public StringBuilder Insert(int index, decimal value)
		{
			return Insert(index, value.ToString(CultureInfo.CurrentCulture), 1);
		}

		[CLSCompliant(false)]
		public StringBuilder Insert(int index, ushort value)
		{
			return Insert(index, value.ToString(CultureInfo.CurrentCulture), 1);
		}

		[CLSCompliant(false)]
		public StringBuilder Insert(int index, uint value)
		{
			return Insert(index, value.ToString(CultureInfo.CurrentCulture), 1);
		}

		[CLSCompliant(false)]
		public StringBuilder Insert(int index, ulong value)
		{
			return Insert(index, value.ToString(CultureInfo.CurrentCulture), 1);
		}

		public StringBuilder Insert(int index, object value)
		{
			if (value == null)
			{
				return this;
			}
			return Insert(index, value.ToString(), 1);
		}

		public StringBuilder AppendFormat(string format, object arg0)
		{
			return AppendFormat(null, format, arg0);
		}

		public StringBuilder AppendFormat(string format, object arg0, object arg1)
		{
			return AppendFormat(null, format, arg0, arg1);
		}

		public StringBuilder AppendFormat(string format, object arg0, object arg1, object arg2)
		{
			return AppendFormat(null, format, arg0, arg1, arg2);
		}

		public StringBuilder AppendFormat(string format, params object[] args)
		{
			return AppendFormat(null, format, args);
		}

		private static void FormatError()
		{
			throw new FormatException(Environment.GetResourceString("Format_InvalidString"));
		}

		public StringBuilder AppendFormat(IFormatProvider provider, string format, params object[] args)
		{
			if (format == null || args == null)
			{
				throw new ArgumentNullException((format == null) ? "format" : "args");
			}
			char[] array = format.ToCharArray(0, format.Length);
			int num = 0;
			int num2 = array.Length;
			char c = '\0';
			ICustomFormatter customFormatter = null;
			if (provider != null)
			{
				customFormatter = (ICustomFormatter)provider.GetFormat(typeof(ICustomFormatter));
			}
			while (true)
			{
				int num3 = num;
				int num4 = num;
				while (num < num2)
				{
					c = array[num];
					num++;
					if (c == '}')
					{
						if (num < num2 && array[num] == '}')
						{
							num++;
						}
						else
						{
							FormatError();
						}
					}
					if (c == '{')
					{
						if (num >= num2 || array[num] != '{')
						{
							num--;
							break;
						}
						num++;
					}
					array[num4++] = c;
				}
				if (num4 > num3)
				{
					Append(array, num3, num4 - num3);
				}
				if (num == num2)
				{
					break;
				}
				num++;
				if (num == num2 || (c = array[num]) < '0' || c > '9')
				{
					FormatError();
				}
				int num5 = 0;
				do
				{
					num5 = num5 * 10 + c - 48;
					num++;
					if (num == num2)
					{
						FormatError();
					}
					c = array[num];
				}
				while (c >= '0' && c <= '9' && num5 < 1000000);
				if (num5 >= args.Length)
				{
					throw new FormatException(Environment.GetResourceString("Format_IndexOutOfRange"));
				}
				for (; num < num2; num++)
				{
					if ((c = array[num]) != ' ')
					{
						break;
					}
				}
				bool flag = false;
				int num6 = 0;
				if (c == ',')
				{
					for (num++; num < num2 && array[num] == ' '; num++)
					{
					}
					if (num == num2)
					{
						FormatError();
					}
					c = array[num];
					if (c == '-')
					{
						flag = true;
						num++;
						if (num == num2)
						{
							FormatError();
						}
						c = array[num];
					}
					if (c < '0' || c > '9')
					{
						FormatError();
					}
					do
					{
						num6 = num6 * 10 + c - 48;
						num++;
						if (num == num2)
						{
							FormatError();
						}
						c = array[num];
					}
					while (c >= '0' && c <= '9' && num6 < 1000000);
				}
				for (; num < num2; num++)
				{
					if ((c = array[num]) != ' ')
					{
						break;
					}
				}
				object obj = args[num5];
				string format2 = null;
				if (c == ':')
				{
					num++;
					num3 = num;
					num4 = num;
					while (true)
					{
						if (num == num2)
						{
							FormatError();
						}
						c = array[num];
						num++;
						switch (c)
						{
						case '{':
							if (num < num2 && array[num] == '{')
							{
								num++;
							}
							else
							{
								FormatError();
							}
							goto IL_0232;
						case '}':
							if (num < num2 && array[num] == '}')
							{
								num++;
								goto IL_0232;
							}
							break;
						default:
							goto IL_0232;
						}
						break;
						IL_0232:
						array[num4++] = c;
					}
					num--;
					if (num4 > num3)
					{
						format2 = new string(array, num3, num4 - num3);
					}
				}
				if (c != '}')
				{
					FormatError();
				}
				num++;
				string text = null;
				if (customFormatter != null)
				{
					text = customFormatter.Format(format2, obj, provider);
				}
				if (text == null)
				{
					if (obj is IFormattable)
					{
						text = ((IFormattable)obj).ToString(format2, provider);
					}
					else if (obj != null)
					{
						text = obj.ToString();
					}
				}
				if (text == null)
				{
					text = string.Empty;
				}
				int num7 = num6 - text.Length;
				if (!flag && num7 > 0)
				{
					Append(' ', num7);
				}
				Append(text);
				if (flag && num7 > 0)
				{
					Append(' ', num7);
				}
			}
			return this;
		}

		public StringBuilder Replace(string oldValue, string newValue)
		{
			return Replace(oldValue, newValue, 0, Length);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		public extern StringBuilder Replace(string oldValue, string newValue, int startIndex, int count);

		public bool Equals(StringBuilder sb)
		{
			if (sb == null)
			{
				return false;
			}
			if (Capacity == sb.Capacity && MaxCapacity == sb.MaxCapacity)
			{
				return m_StringValue.Equals(sb.m_StringValue);
			}
			return false;
		}

		public StringBuilder Replace(char oldChar, char newChar)
		{
			return Replace(oldChar, newChar, 0, Length);
		}

		public StringBuilder Replace(char oldChar, char newChar, int startIndex, int count)
		{
			IntPtr tid;
			string threadSafeString = GetThreadSafeString(out tid);
			int length = threadSafeString.Length;
			if ((uint)startIndex > (uint)length)
			{
				throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("ArgumentOutOfRange_Index"));
			}
			if (count < 0 || startIndex > length - count)
			{
				throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("ArgumentOutOfRange_Index"));
			}
			threadSafeString.ReplaceCharInPlace(oldChar, newChar, startIndex, count, length);
			ReplaceString(tid, threadSafeString);
			return this;
		}
	}
}
