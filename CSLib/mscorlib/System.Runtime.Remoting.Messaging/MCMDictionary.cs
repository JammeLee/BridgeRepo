using System.Collections;

namespace System.Runtime.Remoting.Messaging
{
	internal class MCMDictionary : MessageDictionary
	{
		public static string[] MCMkeys = new string[6]
		{
			"__Uri",
			"__MethodName",
			"__MethodSignature",
			"__TypeName",
			"__Args",
			"__CallContext"
		};

		internal IMethodCallMessage _mcmsg;

		public MCMDictionary(IMethodCallMessage msg, IDictionary idict)
			: base(MCMkeys, idict)
		{
			_mcmsg = msg;
		}

		internal override object GetMessageValue(int i)
		{
			return i switch
			{
				0 => _mcmsg.Uri, 
				1 => _mcmsg.MethodName, 
				2 => _mcmsg.MethodSignature, 
				3 => _mcmsg.TypeName, 
				4 => _mcmsg.Args, 
				5 => FetchLogicalCallContext(), 
				_ => throw new RemotingException(Environment.GetResourceString("Remoting_Default")), 
			};
		}

		private LogicalCallContext FetchLogicalCallContext()
		{
			Message message = _mcmsg as Message;
			if (message != null)
			{
				return message.GetLogicalCallContext();
			}
			MethodCall methodCall = _mcmsg as MethodCall;
			if (methodCall != null)
			{
				return methodCall.GetLogicalCallContext();
			}
			throw new RemotingException(Environment.GetResourceString("Remoting_Message_BadType"));
		}

		internal override void SetSpecialKey(int keyNum, object value)
		{
			Message message = _mcmsg as Message;
			MethodCall methodCall = _mcmsg as MethodCall;
			switch (keyNum)
			{
			case 0:
				if (message != null)
				{
					message.Uri = (string)value;
					break;
				}
				if (methodCall != null)
				{
					methodCall.Uri = (string)value;
					break;
				}
				throw new RemotingException(Environment.GetResourceString("Remoting_Message_BadType"));
			case 1:
				if (message != null)
				{
					message.SetLogicalCallContext((LogicalCallContext)value);
					break;
				}
				throw new RemotingException(Environment.GetResourceString("Remoting_Message_BadType"));
			default:
				throw new RemotingException(Environment.GetResourceString("Remoting_Default"));
			}
		}
	}
}
