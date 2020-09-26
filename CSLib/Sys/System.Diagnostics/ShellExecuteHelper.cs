using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Win32;

namespace System.Diagnostics
{
	internal class ShellExecuteHelper
	{
		private NativeMethods.ShellExecuteInfo _executeInfo;

		private int _errorCode;

		private bool _succeeded;

		public int ErrorCode => _errorCode;

		public ShellExecuteHelper(NativeMethods.ShellExecuteInfo executeInfo)
		{
			_executeInfo = executeInfo;
		}

		public void ShellExecuteFunction()
		{
			if (!(_succeeded = NativeMethods.ShellExecuteEx(_executeInfo)))
			{
				_errorCode = Marshal.GetLastWin32Error();
			}
		}

		public bool ShellExecuteOnSTAThread()
		{
			if (Thread.CurrentThread.GetApartmentState() != 0)
			{
				ThreadStart start = ShellExecuteFunction;
				Thread thread = new Thread(start);
				thread.SetApartmentState(ApartmentState.STA);
				thread.Start();
				thread.Join();
			}
			else
			{
				ShellExecuteFunction();
			}
			return _succeeded;
		}
	}
}
