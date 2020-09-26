using System.Collections;
using System.Runtime.Remoting.Proxies;

namespace System.Runtime.Remoting.Messaging
{
	internal class MessageSmuggler
	{
		protected class SerializedArg
		{
			private int _index;

			public int Index => _index;

			public SerializedArg(int index)
			{
				_index = index;
			}
		}

		private static bool CanSmuggleObjectDirectly(object obj)
		{
			if (obj is string || obj.GetType() == typeof(void) || obj.GetType().IsPrimitive)
			{
				return true;
			}
			return false;
		}

		protected static object[] FixupArgs(object[] args, ref ArrayList argsToSerialize)
		{
			object[] array = new object[args.Length];
			int num = args.Length;
			for (int i = 0; i < num; i++)
			{
				array[i] = FixupArg(args[i], ref argsToSerialize);
			}
			return array;
		}

		protected static object FixupArg(object arg, ref ArrayList argsToSerialize)
		{
			if (arg == null)
			{
				return null;
			}
			MarshalByRefObject marshalByRefObject = arg as MarshalByRefObject;
			int count;
			if (marshalByRefObject != null)
			{
				if (!RemotingServices.IsTransparentProxy(marshalByRefObject) || RemotingServices.GetRealProxy(marshalByRefObject) is RemotingProxy)
				{
					ObjRef objRef = RemotingServices.MarshalInternal(marshalByRefObject, null, null);
					if (objRef.CanSmuggle())
					{
						if (!RemotingServices.IsTransparentProxy(marshalByRefObject))
						{
							ServerIdentity serverIdentity = (ServerIdentity)MarshalByRefObject.GetIdentity(marshalByRefObject);
							serverIdentity.SetHandle();
							objRef.SetServerIdentity(serverIdentity.GetHandle());
							objRef.SetDomainID(AppDomain.CurrentDomain.GetId());
						}
						ObjRef objRef2 = objRef.CreateSmuggleableCopy();
						objRef2.SetMarshaledObject();
						return new SmuggledObjRef(objRef2);
					}
				}
				if (argsToSerialize == null)
				{
					argsToSerialize = new ArrayList();
				}
				count = argsToSerialize.Count;
				argsToSerialize.Add(arg);
				return new SerializedArg(count);
			}
			if (CanSmuggleObjectDirectly(arg))
			{
				return arg;
			}
			Array array = arg as Array;
			if (array != null)
			{
				Type elementType = array.GetType().GetElementType();
				if (elementType.IsPrimitive || elementType == typeof(string))
				{
					return array.Clone();
				}
			}
			if (argsToSerialize == null)
			{
				argsToSerialize = new ArrayList();
			}
			count = argsToSerialize.Count;
			argsToSerialize.Add(arg);
			return new SerializedArg(count);
		}

		protected static object[] UndoFixupArgs(object[] args, ArrayList deserializedArgs)
		{
			object[] array = new object[args.Length];
			int num = args.Length;
			for (int i = 0; i < num; i++)
			{
				array[i] = UndoFixupArg(args[i], deserializedArgs);
			}
			return array;
		}

		protected static object UndoFixupArg(object arg, ArrayList deserializedArgs)
		{
			SmuggledObjRef smuggledObjRef = arg as SmuggledObjRef;
			if (smuggledObjRef != null)
			{
				return smuggledObjRef.ObjRef.GetRealObjectHelper();
			}
			SerializedArg serializedArg = arg as SerializedArg;
			if (serializedArg != null)
			{
				return deserializedArgs[serializedArg.Index];
			}
			return arg;
		}

		protected static int StoreUserPropertiesForMethodMessage(IMethodMessage msg, ref ArrayList argsToSerialize)
		{
			IDictionary properties = msg.Properties;
			MessageDictionary messageDictionary = properties as MessageDictionary;
			if (messageDictionary != null)
			{
				if (messageDictionary.HasUserData())
				{
					int num = 0;
					{
						foreach (DictionaryEntry item in messageDictionary.InternalDictionary)
						{
							if (argsToSerialize == null)
							{
								argsToSerialize = new ArrayList();
							}
							argsToSerialize.Add(item);
							num++;
						}
						return num;
					}
				}
				return 0;
			}
			int num2 = 0;
			foreach (DictionaryEntry item2 in properties)
			{
				if (argsToSerialize == null)
				{
					argsToSerialize = new ArrayList();
				}
				argsToSerialize.Add(item2);
				num2++;
			}
			return num2;
		}
	}
}
