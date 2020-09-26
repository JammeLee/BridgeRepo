using System.Runtime.CompilerServices;

namespace System.Diagnostics
{
	internal static class Assert
	{
		private static AssertFilter[] ListOfFilters;

		private static int iNumOfFilters;

		private static int iFilterArraySize;

		static Assert()
		{
			AddFilter(new DefaultFilter());
		}

		public static void AddFilter(AssertFilter filter)
		{
			if (iFilterArraySize <= iNumOfFilters)
			{
				AssertFilter[] array = new AssertFilter[iFilterArraySize + 2];
				if (iNumOfFilters > 0)
				{
					Array.Copy(ListOfFilters, array, iNumOfFilters);
				}
				iFilterArraySize += 2;
				ListOfFilters = array;
			}
			ListOfFilters[iNumOfFilters++] = filter;
		}

		public static void Check(bool condition, string conditionString, string message)
		{
			if (!condition)
			{
				Fail(conditionString, message);
			}
		}

		public static void Fail(string conditionString, string message)
		{
			StackTrace location = new StackTrace();
			int num = iNumOfFilters;
			while (num > 0)
			{
				switch (ListOfFilters[--num].AssertFailure(conditionString, message, location))
				{
				case AssertFilters.FailDebug:
					if (Debugger.IsAttached)
					{
						Debugger.Break();
					}
					else if (!Debugger.Launch())
					{
						throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_DebuggerLaunchFailed"));
					}
					return;
				case AssertFilters.FailTerminate:
					Environment.Exit(-1);
					break;
				case AssertFilters.FailIgnore:
					return;
				}
			}
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		public static extern int ShowDefaultAssertDialog(string conditionString, string message);
	}
}
