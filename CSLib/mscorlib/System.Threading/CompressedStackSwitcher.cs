using System.Runtime.ConstrainedExecution;

namespace System.Threading
{
	internal struct CompressedStackSwitcher : IDisposable
	{
		internal CompressedStack curr_CS;

		internal CompressedStack prev_CS;

		internal IntPtr prev_ADStack;

		public override bool Equals(object obj)
		{
			if (obj == null || !(obj is CompressedStackSwitcher))
			{
				return false;
			}
			CompressedStackSwitcher compressedStackSwitcher = (CompressedStackSwitcher)obj;
			if (curr_CS == compressedStackSwitcher.curr_CS && prev_CS == compressedStackSwitcher.prev_CS)
			{
				return prev_ADStack == compressedStackSwitcher.prev_ADStack;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return ToString().GetHashCode();
		}

		public static bool operator ==(CompressedStackSwitcher c1, CompressedStackSwitcher c2)
		{
			return c1.Equals(c2);
		}

		public static bool operator !=(CompressedStackSwitcher c1, CompressedStackSwitcher c2)
		{
			return !c1.Equals(c2);
		}

		void IDisposable.Dispose()
		{
			Undo();
		}

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		internal bool UndoNoThrow()
		{
			try
			{
				Undo();
			}
			catch
			{
				return false;
			}
			return true;
		}

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		public void Undo()
		{
			if (curr_CS != null || prev_CS != null)
			{
				if (prev_ADStack != (IntPtr)0)
				{
					CompressedStack.RestoreAppDomainStack(prev_ADStack);
				}
				CompressedStack.SetCompressedStackThread(prev_CS);
				prev_CS = null;
				curr_CS = null;
				prev_ADStack = (IntPtr)0;
			}
		}
	}
}
