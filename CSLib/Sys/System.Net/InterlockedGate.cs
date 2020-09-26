using System.Threading;

namespace System.Net
{
	internal struct InterlockedGate
	{
		internal const int Open = 0;

		internal const int Held = 1;

		internal const int Triggered = 2;

		internal const int Closed = 3;

		private int m_State;

		internal void Reset()
		{
			m_State = 0;
		}

		internal bool Trigger(bool exclusive)
		{
			int num = Interlocked.CompareExchange(ref m_State, 2, 0);
			if (exclusive && (num == 1 || num == 2))
			{
				throw new InternalException();
			}
			return num == 0;
		}

		internal bool StartTrigger(bool exclusive)
		{
			int num = Interlocked.CompareExchange(ref m_State, 1, 0);
			if (exclusive && (num == 1 || num == 2))
			{
				throw new InternalException();
			}
			return num == 0;
		}

		internal void FinishTrigger()
		{
			int num = Interlocked.CompareExchange(ref m_State, 2, 1);
			if (num != 1)
			{
				throw new InternalException();
			}
		}

		internal bool Complete()
		{
			int num;
			while ((num = Interlocked.CompareExchange(ref m_State, 3, 2)) != 2)
			{
				switch (num)
				{
				case 3:
					return false;
				case 0:
					if (Interlocked.CompareExchange(ref m_State, 3, 0) == 0)
					{
						return false;
					}
					break;
				default:
					Thread.SpinWait(1);
					break;
				}
			}
			return true;
		}
	}
}
