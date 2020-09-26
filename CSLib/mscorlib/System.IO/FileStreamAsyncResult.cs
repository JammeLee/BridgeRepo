using System.Threading;
using Microsoft.Win32.SafeHandles;

namespace System.IO
{
	internal sealed class FileStreamAsyncResult : IAsyncResult
	{
		internal AsyncCallback _userCallback;

		internal object _userStateObject;

		internal ManualResetEvent _waitHandle;

		internal SafeFileHandle _handle;

		internal unsafe NativeOverlapped* _overlapped;

		internal int _EndXxxCalled;

		internal int _numBytes;

		internal int _errorCode;

		internal int _numBufferedBytes;

		internal bool _isWrite;

		internal bool _isComplete;

		internal bool _completedSynchronously;

		public object AsyncState => _userStateObject;

		public bool IsCompleted => _isComplete;

		public unsafe WaitHandle AsyncWaitHandle
		{
			get
			{
				if (_waitHandle == null)
				{
					ManualResetEvent manualResetEvent = new ManualResetEvent(initialState: false);
					if (_overlapped != null && _overlapped->EventHandle != IntPtr.Zero)
					{
						manualResetEvent.SafeWaitHandle = new SafeWaitHandle(_overlapped->EventHandle, ownsHandle: true);
					}
					if (_isComplete)
					{
						manualResetEvent.Set();
					}
					_waitHandle = manualResetEvent;
				}
				return _waitHandle;
			}
		}

		public bool CompletedSynchronously => _completedSynchronously;

		internal static FileStreamAsyncResult CreateBufferedReadResult(int numBufferedBytes, AsyncCallback userCallback, object userStateObject)
		{
			FileStreamAsyncResult fileStreamAsyncResult = new FileStreamAsyncResult();
			fileStreamAsyncResult._userCallback = userCallback;
			fileStreamAsyncResult._userStateObject = userStateObject;
			fileStreamAsyncResult._isWrite = false;
			fileStreamAsyncResult._numBufferedBytes = numBufferedBytes;
			return fileStreamAsyncResult;
		}

		private void CallUserCallbackWorker(object callbackState)
		{
			_isComplete = true;
			if (_waitHandle != null)
			{
				_waitHandle.Set();
			}
			_userCallback(this);
		}

		internal void CallUserCallback()
		{
			if (_userCallback != null)
			{
				_completedSynchronously = false;
				ThreadPool.QueueUserWorkItem(CallUserCallbackWorker);
				return;
			}
			_isComplete = true;
			if (_waitHandle != null)
			{
				_waitHandle.Set();
			}
		}
	}
}
