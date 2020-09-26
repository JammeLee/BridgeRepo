using System.Collections;
using System.IO;
using System.Runtime.Remoting.Channels;

namespace System.Runtime.Remoting.Messaging
{
	internal class SmuggledMethodReturnMessage : MessageSmuggler
	{
		private object[] _args;

		private object _returnValue;

		private byte[] _serializedArgs;

		private SerializedArg _exception;

		private object _callContext;

		private int _propertyCount;

		internal int MessagePropertyCount => _propertyCount;

		internal static SmuggledMethodReturnMessage SmuggleIfPossible(IMessage msg)
		{
			IMethodReturnMessage methodReturnMessage = msg as IMethodReturnMessage;
			if (methodReturnMessage == null)
			{
				return null;
			}
			return new SmuggledMethodReturnMessage(methodReturnMessage);
		}

		private SmuggledMethodReturnMessage()
		{
		}

		private SmuggledMethodReturnMessage(IMethodReturnMessage mrm)
		{
			ArrayList argsToSerialize = null;
			ReturnMessage returnMessage = mrm as ReturnMessage;
			if (returnMessage == null || returnMessage.HasProperties())
			{
				_propertyCount = MessageSmuggler.StoreUserPropertiesForMethodMessage(mrm, ref argsToSerialize);
			}
			Exception exception = mrm.Exception;
			if (exception != null)
			{
				if (argsToSerialize == null)
				{
					argsToSerialize = new ArrayList();
				}
				_exception = new SerializedArg(argsToSerialize.Count);
				argsToSerialize.Add(exception);
			}
			LogicalCallContext logicalCallContext = mrm.LogicalCallContext;
			if (logicalCallContext == null)
			{
				_callContext = null;
			}
			else if (logicalCallContext.HasInfo)
			{
				if (logicalCallContext.Principal != null)
				{
					logicalCallContext.Principal = null;
				}
				if (argsToSerialize == null)
				{
					argsToSerialize = new ArrayList();
				}
				_callContext = new SerializedArg(argsToSerialize.Count);
				argsToSerialize.Add(logicalCallContext);
			}
			else
			{
				_callContext = logicalCallContext.RemotingData.LogicalCallID;
			}
			_returnValue = MessageSmuggler.FixupArg(mrm.ReturnValue, ref argsToSerialize);
			_args = MessageSmuggler.FixupArgs(mrm.Args, ref argsToSerialize);
			if (argsToSerialize != null)
			{
				MemoryStream memoryStream = CrossAppDomainSerializer.SerializeMessageParts(argsToSerialize);
				_serializedArgs = memoryStream.GetBuffer();
			}
		}

		internal ArrayList FixupForNewAppDomain()
		{
			ArrayList result = null;
			if (_serializedArgs != null)
			{
				result = CrossAppDomainSerializer.DeserializeMessageParts(new MemoryStream(_serializedArgs));
				_serializedArgs = null;
			}
			return result;
		}

		internal object GetReturnValue(ArrayList deserializedArgs)
		{
			return MessageSmuggler.UndoFixupArg(_returnValue, deserializedArgs);
		}

		internal object[] GetArgs(ArrayList deserializedArgs)
		{
			return MessageSmuggler.UndoFixupArgs(_args, deserializedArgs);
		}

		internal Exception GetException(ArrayList deserializedArgs)
		{
			if (_exception != null)
			{
				return (Exception)deserializedArgs[_exception.Index];
			}
			return null;
		}

		internal LogicalCallContext GetCallContext(ArrayList deserializedArgs)
		{
			if (_callContext == null)
			{
				return null;
			}
			if (_callContext is string)
			{
				LogicalCallContext logicalCallContext = new LogicalCallContext();
				logicalCallContext.RemotingData.LogicalCallID = (string)_callContext;
				return logicalCallContext;
			}
			return (LogicalCallContext)deserializedArgs[((SerializedArg)_callContext).Index];
		}

		internal void PopulateMessageProperties(IDictionary dict, ArrayList deserializedArgs)
		{
			for (int i = 0; i < _propertyCount; i++)
			{
				DictionaryEntry dictionaryEntry = (DictionaryEntry)deserializedArgs[i];
				dict[dictionaryEntry.Key] = dictionaryEntry.Value;
			}
		}
	}
}
