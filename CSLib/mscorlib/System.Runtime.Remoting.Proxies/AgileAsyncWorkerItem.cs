using System.Runtime.Remoting.Messaging;

namespace System.Runtime.Remoting.Proxies
{
	internal class AgileAsyncWorkerItem
	{
		private IMethodCallMessage _message;

		private AsyncResult _ar;

		private object _target;

		public AgileAsyncWorkerItem(IMethodCallMessage message, AsyncResult ar, object target)
		{
			_message = new MethodCall(message);
			_ar = ar;
			_target = target;
		}

		public static void ThreadPoolCallBack(object o)
		{
			((AgileAsyncWorkerItem)o).DoAsyncCall();
		}

		public void DoAsyncCall()
		{
			new StackBuilderSink(_target).AsyncProcessMessage(_message, _ar);
		}
	}
}
