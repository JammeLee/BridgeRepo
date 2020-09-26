namespace System.Reflection.Emit
{
	internal struct __ExceptionInstance
	{
		internal int m_exceptionClass;

		internal int m_startAddress;

		internal int m_endAddress;

		internal int m_filterAddress;

		internal int m_handleAddress;

		internal int m_handleEndAddress;

		internal int m_type;

		internal __ExceptionInstance(int start, int end, int filterAddr, int handle, int handleEnd, int type, int exceptionClass)
		{
			m_startAddress = start;
			m_endAddress = end;
			m_filterAddress = filterAddr;
			m_handleAddress = handle;
			m_handleEndAddress = handleEnd;
			m_type = type;
			m_exceptionClass = exceptionClass;
		}

		public override bool Equals(object obj)
		{
			if (obj != null && obj is __ExceptionInstance)
			{
				__ExceptionInstance _ExceptionInstance = (__ExceptionInstance)obj;
				if (_ExceptionInstance.m_exceptionClass == m_exceptionClass && _ExceptionInstance.m_startAddress == m_startAddress && _ExceptionInstance.m_endAddress == m_endAddress && _ExceptionInstance.m_filterAddress == m_filterAddress && _ExceptionInstance.m_handleAddress == m_handleAddress)
				{
					return _ExceptionInstance.m_handleEndAddress == m_handleEndAddress;
				}
				return false;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return m_exceptionClass ^ m_startAddress ^ m_endAddress ^ m_filterAddress ^ m_handleAddress ^ m_handleEndAddress ^ m_type;
		}
	}
}
