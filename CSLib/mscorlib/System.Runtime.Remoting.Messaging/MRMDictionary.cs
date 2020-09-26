using System.Collections;

namespace System.Runtime.Remoting.Messaging
{
	internal class MRMDictionary : MessageDictionary
	{
		public static string[] MCMkeysFault = new string[1]
		{
			"__CallContext"
		};

		public static string[] MCMkeysNoFault = new string[7]
		{
			"__Uri",
			"__MethodName",
			"__MethodSignature",
			"__TypeName",
			"__Return",
			"__OutArgs",
			"__CallContext"
		};

		internal IMethodReturnMessage _mrmsg;

		internal bool fault;

		public MRMDictionary(IMethodReturnMessage msg, IDictionary idict)
			: base((msg.Exception != null) ? MCMkeysFault : MCMkeysNoFault, idict)
		{
			fault = msg.Exception != null;
			_mrmsg = msg;
		}

		internal override object GetMessageValue(int i)
		{
			switch (i)
			{
			case 0:
				if (fault)
				{
					return FetchLogicalCallContext();
				}
				return _mrmsg.Uri;
			case 1:
				return _mrmsg.MethodName;
			case 2:
				return _mrmsg.MethodSignature;
			case 3:
				return _mrmsg.TypeName;
			case 4:
				if (fault)
				{
					return _mrmsg.Exception;
				}
				return _mrmsg.ReturnValue;
			case 5:
				return _mrmsg.Args;
			case 6:
				return FetchLogicalCallContext();
			default:
				throw new RemotingException(Environment.GetResourceString("Remoting_Default"));
			}
		}

		private LogicalCallContext FetchLogicalCallContext()
		{
			ReturnMessage returnMessage = _mrmsg as ReturnMessage;
			if (returnMessage != null)
			{
				return returnMessage.GetLogicalCallContext();
			}
			MethodResponse methodResponse = _mrmsg as MethodResponse;
			if (methodResponse != null)
			{
				return methodResponse.GetLogicalCallContext();
			}
			StackBasedReturnMessage stackBasedReturnMessage = _mrmsg as StackBasedReturnMessage;
			if (stackBasedReturnMessage != null)
			{
				return stackBasedReturnMessage.GetLogicalCallContext();
			}
			throw new RemotingException(Environment.GetResourceString("Remoting_Message_BadType"));
		}

		internal override void SetSpecialKey(int keyNum, object value)
		{
			ReturnMessage returnMessage = _mrmsg as ReturnMessage;
			MethodResponse methodResponse = _mrmsg as MethodResponse;
			switch (keyNum)
			{
			case 0:
				if (returnMessage != null)
				{
					returnMessage.Uri = (string)value;
					break;
				}
				if (methodResponse != null)
				{
					methodResponse.Uri = (string)value;
					break;
				}
				throw new RemotingException(Environment.GetResourceString("Remoting_Message_BadType"));
			case 1:
				if (returnMessage != null)
				{
					returnMessage.SetLogicalCallContext((LogicalCallContext)value);
					break;
				}
				if (methodResponse != null)
				{
					methodResponse.SetLogicalCallContext((LogicalCallContext)value);
					break;
				}
				throw new RemotingException(Environment.GetResourceString("Remoting_Message_BadType"));
			default:
				throw new RemotingException(Environment.GetResourceString("Remoting_Default"));
			}
		}
	}
}
