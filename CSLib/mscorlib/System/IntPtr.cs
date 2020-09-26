using System.Globalization;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace System
{
	[Serializable]
	[ComVisible(true)]
	public struct IntPtr : ISerializable
	{
		private unsafe void* m_value;

		public static readonly IntPtr Zero;

		public static int Size
		{
			[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
			get
			{
				return 4;
			}
		}

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		internal unsafe bool IsNull()
		{
			return m_value == null;
		}

		[ReliabilityContract(Consistency.MayCorruptInstance, Cer.MayFail)]
		public unsafe IntPtr(int value)
		{
			m_value = (void*)value;
		}

		[ReliabilityContract(Consistency.MayCorruptInstance, Cer.MayFail)]
		public unsafe IntPtr(long value)
		{
			m_value = (void*)checked((int)value);
		}

		[CLSCompliant(false)]
		[ReliabilityContract(Consistency.MayCorruptInstance, Cer.MayFail)]
		public unsafe IntPtr(void* value)
		{
			m_value = value;
		}

		private unsafe IntPtr(SerializationInfo info, StreamingContext context)
		{
			long @int = info.GetInt64("value");
			if (Size == 4 && (@int > int.MaxValue || @int < int.MinValue))
			{
				throw new ArgumentException(Environment.GetResourceString("Serialization_InvalidPtrValue"));
			}
			m_value = (void*)@int;
		}

		unsafe void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			info.AddValue("value", (long)(int)m_value);
		}

		public unsafe override bool Equals(object obj)
		{
			if (obj is IntPtr)
			{
				return m_value == ((IntPtr)obj).m_value;
			}
			return false;
		}

		public unsafe override int GetHashCode()
		{
			return (int)m_value;
		}

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		public unsafe int ToInt32()
		{
			return (int)m_value;
		}

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		public unsafe long ToInt64()
		{
			return (int)m_value;
		}

		public unsafe override string ToString()
		{
			return ((int)m_value).ToString(CultureInfo.InvariantCulture);
		}

		public unsafe string ToString(string format)
		{
			return ((int)m_value).ToString(format, CultureInfo.InvariantCulture);
		}

		[ReliabilityContract(Consistency.MayCorruptInstance, Cer.MayFail)]
		public static explicit operator IntPtr(int value)
		{
			return new IntPtr(value);
		}

		[ReliabilityContract(Consistency.MayCorruptInstance, Cer.MayFail)]
		public static explicit operator IntPtr(long value)
		{
			return new IntPtr(value);
		}

		[ReliabilityContract(Consistency.MayCorruptInstance, Cer.MayFail)]
		[CLSCompliant(false)]
		public unsafe static explicit operator IntPtr(void* value)
		{
			return new IntPtr(value);
		}

		[CLSCompliant(false)]
		public unsafe static explicit operator void*(IntPtr value)
		{
			return value.ToPointer();
		}

		public unsafe static explicit operator int(IntPtr value)
		{
			return (int)value.m_value;
		}

		public unsafe static explicit operator long(IntPtr value)
		{
			return (int)value.m_value;
		}

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		public unsafe static bool operator ==(IntPtr value1, IntPtr value2)
		{
			return value1.m_value == value2.m_value;
		}

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		public unsafe static bool operator !=(IntPtr value1, IntPtr value2)
		{
			return value1.m_value != value2.m_value;
		}

		[CLSCompliant(false)]
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		public unsafe void* ToPointer()
		{
			return m_value;
		}
	}
}
