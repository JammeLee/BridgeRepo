using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Security.Util;
using System.Threading;

namespace System
{
	internal sealed class SharedStatics
	{
		internal static SharedStatics _sharedStatics;

		private string _Remoting_Identity_IDGuid;

		private Tokenizer.StringMaker _maker;

		private int _Remoting_Identity_IDSeqNum;

		private long _memFailPointReservedMemory;

		public static string Remoting_Identity_IDGuid
		{
			get
			{
				if (_sharedStatics._Remoting_Identity_IDGuid == null)
				{
					bool tookLock = false;
					RuntimeHelpers.PrepareConstrainedRegions();
					try
					{
						Monitor.ReliableEnter(_sharedStatics, ref tookLock);
						if (_sharedStatics._Remoting_Identity_IDGuid == null)
						{
							_sharedStatics._Remoting_Identity_IDGuid = Guid.NewGuid().ToString().Replace('-', '_');
						}
					}
					finally
					{
						if (tookLock)
						{
							Monitor.Exit(_sharedStatics);
						}
					}
				}
				return _sharedStatics._Remoting_Identity_IDGuid;
			}
		}

		internal static ulong MemoryFailPointReservedMemory => (ulong)_sharedStatics._memFailPointReservedMemory;

		private SharedStatics()
		{
			_Remoting_Identity_IDGuid = null;
			_Remoting_Identity_IDSeqNum = 64;
			_maker = null;
		}

		public static Tokenizer.StringMaker GetSharedStringMaker()
		{
			Tokenizer.StringMaker stringMaker = null;
			bool tookLock = false;
			RuntimeHelpers.PrepareConstrainedRegions();
			try
			{
				Monitor.ReliableEnter(_sharedStatics, ref tookLock);
				if (_sharedStatics._maker != null)
				{
					stringMaker = _sharedStatics._maker;
					_sharedStatics._maker = null;
				}
			}
			finally
			{
				if (tookLock)
				{
					Monitor.Exit(_sharedStatics);
				}
			}
			if (stringMaker == null)
			{
				stringMaker = new Tokenizer.StringMaker();
			}
			return stringMaker;
		}

		public static void ReleaseSharedStringMaker(ref Tokenizer.StringMaker maker)
		{
			bool tookLock = false;
			RuntimeHelpers.PrepareConstrainedRegions();
			try
			{
				Monitor.ReliableEnter(_sharedStatics, ref tookLock);
				_sharedStatics._maker = maker;
				maker = null;
			}
			finally
			{
				if (tookLock)
				{
					Monitor.Exit(_sharedStatics);
				}
			}
		}

		internal static int Remoting_Identity_GetNextSeqNum()
		{
			return Interlocked.Increment(ref _sharedStatics._Remoting_Identity_IDSeqNum);
		}

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		internal static long AddMemoryFailPointReservation(long size)
		{
			return Interlocked.Add(ref _sharedStatics._memFailPointReservedMemory, size);
		}
	}
}
