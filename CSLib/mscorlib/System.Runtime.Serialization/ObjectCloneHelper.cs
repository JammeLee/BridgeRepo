using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;

namespace System.Runtime.Serialization
{
	internal sealed class ObjectCloneHelper
	{
		private static IFormatterConverter s_converter = new FormatterConverter();

		private static StreamingContext s_cloneContext = new StreamingContext(StreamingContextStates.CrossAppDomain);

		private static ISerializationSurrogate s_RemotingSurrogate = new RemotingSurrogate();

		private static ISerializationSurrogate s_ObjRefRemotingSurrogate = new ObjRefSurrogate();

		internal static object GetObjectData(object serObj, out string typeName, out string assemName, out string[] fieldNames, out object[] fieldValues)
		{
			Type type = null;
			object obj = null;
			type = ((!RemotingServices.IsTransparentProxy(serObj)) ? serObj.GetType() : typeof(MarshalByRefObject));
			SerializationInfo serializationInfo = new SerializationInfo(type, s_converter);
			if (serObj is ObjRef)
			{
				s_ObjRefRemotingSurrogate.GetObjectData(serObj, serializationInfo, s_cloneContext);
			}
			else if (RemotingServices.IsTransparentProxy(serObj) || serObj is MarshalByRefObject)
			{
				if (!RemotingServices.IsTransparentProxy(serObj) || RemotingServices.GetRealProxy(serObj) is RemotingProxy)
				{
					ObjRef objRef = RemotingServices.MarshalInternal((MarshalByRefObject)serObj, null, null);
					if (objRef.CanSmuggle())
					{
						if (RemotingServices.IsTransparentProxy(serObj))
						{
							RealProxy realProxy = RemotingServices.GetRealProxy(serObj);
							objRef.SetServerIdentity(realProxy._srvIdentity);
							objRef.SetDomainID(realProxy._domainID);
						}
						else
						{
							ServerIdentity serverIdentity = (ServerIdentity)MarshalByRefObject.GetIdentity((MarshalByRefObject)serObj);
							serverIdentity.SetHandle();
							objRef.SetServerIdentity(serverIdentity.GetHandle());
							objRef.SetDomainID(AppDomain.CurrentDomain.GetId());
						}
						objRef.SetMarshaledObject();
						obj = objRef;
					}
				}
				if (obj == null)
				{
					s_RemotingSurrogate.GetObjectData(serObj, serializationInfo, s_cloneContext);
				}
			}
			else
			{
				if (!(serObj is ISerializable))
				{
					throw new ArgumentException(Environment.GetResourceString("Arg_SerializationException"));
				}
				((ISerializable)serObj).GetObjectData(serializationInfo, s_cloneContext);
			}
			if (obj == null)
			{
				typeName = serializationInfo.FullTypeName;
				assemName = serializationInfo.AssemblyName;
				fieldNames = serializationInfo.MemberNames;
				fieldValues = serializationInfo.MemberValues;
			}
			else
			{
				typeName = null;
				assemName = null;
				fieldNames = null;
				fieldValues = null;
			}
			return obj;
		}

		internal static SerializationInfo PrepareConstructorArgs(object serObj, string[] fieldNames, object[] fieldValues, out StreamingContext context)
		{
			SerializationInfo serializationInfo = null;
			if (serObj is ISerializable)
			{
				serializationInfo = new SerializationInfo(serObj.GetType(), s_converter);
				for (int i = 0; i < fieldNames.Length; i++)
				{
					if (fieldNames[i] != null)
					{
						serializationInfo.AddValue(fieldNames[i], fieldValues[i]);
					}
				}
			}
			else
			{
				Hashtable hashtable = new Hashtable();
				int j = 0;
				int num = 0;
				for (; j < fieldNames.Length; j++)
				{
					if (fieldNames[j] != null)
					{
						hashtable[fieldNames[j]] = fieldValues[j];
						num++;
					}
				}
				MemberInfo[] serializableMembers = FormatterServices.GetSerializableMembers(serObj.GetType());
				for (int k = 0; k < serializableMembers.Length; k++)
				{
					string name = serializableMembers[k].Name;
					if (!hashtable.Contains(name))
					{
						object[] customAttributes = serializableMembers[k].GetCustomAttributes(typeof(OptionalFieldAttribute), inherit: false);
						if (customAttributes == null || customAttributes.Length == 0)
						{
							throw new SerializationException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Serialization_MissingMember"), serializableMembers[k], serObj.GetType(), typeof(OptionalFieldAttribute).FullName));
						}
					}
					else
					{
						object value = hashtable[name];
						FormatterServices.SerializationSetValue(serializableMembers[k], serObj, value);
					}
				}
			}
			context = s_cloneContext;
			return serializationInfo;
		}
	}
}
