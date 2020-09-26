using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Metadata;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Permissions;

namespace System.Runtime.Remoting.Messaging
{
	[Serializable]
	[ComVisible(true)]
	[CLSCompliant(false)]
	[SecurityPermission(SecurityAction.InheritanceDemand, Flags = SecurityPermissionFlag.Infrastructure)]
	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
	public class MethodCall : IMethodCallMessage, IMethodMessage, IMessage, ISerializable, IInternalMessage, ISerializationRootObject
	{
		private const BindingFlags LookupAll = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

		private const BindingFlags LookupPublic = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public;

		private string uri;

		private string methodName;

		private MethodBase MI;

		private string typeName;

		private object[] args;

		private Type[] instArgs;

		private LogicalCallContext callContext;

		private Type[] methodSignature;

		protected IDictionary ExternalProperties;

		protected IDictionary InternalProperties;

		private ServerIdentity srvID;

		private Identity identity;

		private bool fSoap;

		private bool fVarArgs;

		private ArgMapper argMapper;

		public int ArgCount
		{
			get
			{
				if (args != null)
				{
					return args.Length;
				}
				return 0;
			}
		}

		public object[] Args => args;

		public int InArgCount
		{
			get
			{
				if (argMapper == null)
				{
					argMapper = new ArgMapper(this, fOut: false);
				}
				return argMapper.ArgCount;
			}
		}

		public object[] InArgs
		{
			get
			{
				if (argMapper == null)
				{
					argMapper = new ArgMapper(this, fOut: false);
				}
				return argMapper.Args;
			}
		}

		public string MethodName => methodName;

		public string TypeName => typeName;

		public object MethodSignature
		{
			get
			{
				if (methodSignature != null)
				{
					return methodSignature;
				}
				if (MI != null)
				{
					methodSignature = Message.GenerateMethodSignature(MethodBase);
				}
				return null;
			}
		}

		public MethodBase MethodBase
		{
			get
			{
				if (MI == null)
				{
					MI = RemotingServices.InternalGetMethodBaseFromMethodMessage(this);
				}
				return MI;
			}
		}

		public string Uri
		{
			get
			{
				return uri;
			}
			set
			{
				uri = value;
			}
		}

		public bool HasVarArgs => fVarArgs;

		public virtual IDictionary Properties
		{
			get
			{
				lock (this)
				{
					if (InternalProperties == null)
					{
						InternalProperties = new Hashtable();
					}
					if (ExternalProperties == null)
					{
						ExternalProperties = new MCMDictionary(this, InternalProperties);
					}
					return ExternalProperties;
				}
			}
		}

		public LogicalCallContext LogicalCallContext => GetLogicalCallContext();

		ServerIdentity IInternalMessage.ServerIdentityObject
		{
			get
			{
				return srvID;
			}
			set
			{
				srvID = value;
			}
		}

		Identity IInternalMessage.IdentityObject
		{
			get
			{
				return identity;
			}
			set
			{
				identity = value;
			}
		}

		public MethodCall(Header[] h1)
		{
			Init();
			fSoap = true;
			FillHeaders(h1);
			ResolveMethod();
		}

		public MethodCall(IMessage msg)
		{
			if (msg == null)
			{
				throw new ArgumentNullException("msg");
			}
			Init();
			IDictionaryEnumerator enumerator = msg.Properties.GetEnumerator();
			while (enumerator.MoveNext())
			{
				FillHeader(enumerator.Key.ToString(), enumerator.Value);
			}
			IMethodCallMessage methodCallMessage = msg as IMethodCallMessage;
			if (methodCallMessage != null)
			{
				MI = methodCallMessage.MethodBase;
			}
			ResolveMethod();
		}

		internal MethodCall(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			Init();
			SetObjectData(info, context);
		}

		internal MethodCall(SmuggledMethodCallMessage smuggledMsg, ArrayList deserializedArgs)
		{
			uri = smuggledMsg.Uri;
			typeName = smuggledMsg.TypeName;
			methodName = smuggledMsg.MethodName;
			methodSignature = (Type[])smuggledMsg.GetMethodSignature(deserializedArgs);
			args = smuggledMsg.GetArgs(deserializedArgs);
			instArgs = smuggledMsg.GetInstantiation(deserializedArgs);
			callContext = smuggledMsg.GetCallContext(deserializedArgs);
			ResolveMethod();
			if (smuggledMsg.MessagePropertyCount > 0)
			{
				smuggledMsg.PopulateMessageProperties(Properties, deserializedArgs);
			}
		}

		internal MethodCall(object handlerObject, BinaryMethodCallMessage smuggledMsg)
		{
			if (handlerObject != null)
			{
				uri = handlerObject as string;
				if (uri == null)
				{
					MarshalByRefObject marshalByRefObject = handlerObject as MarshalByRefObject;
					if (marshalByRefObject != null)
					{
						srvID = MarshalByRefObject.GetIdentity(marshalByRefObject, out var _) as ServerIdentity;
						uri = srvID.URI;
					}
				}
			}
			typeName = smuggledMsg.TypeName;
			methodName = smuggledMsg.MethodName;
			methodSignature = (Type[])smuggledMsg.MethodSignature;
			args = smuggledMsg.Args;
			instArgs = smuggledMsg.InstantiationArgs;
			callContext = smuggledMsg.LogicalCallContext;
			ResolveMethod();
			if (smuggledMsg.HasProperties)
			{
				smuggledMsg.PopulateMessageProperties(Properties);
			}
		}

		public void RootSetObjectData(SerializationInfo info, StreamingContext ctx)
		{
			SetObjectData(info, ctx);
		}

		internal void SetObjectData(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			if (fSoap)
			{
				SetObjectFromSoapData(info);
				return;
			}
			SerializationInfoEnumerator enumerator = info.GetEnumerator();
			while (enumerator.MoveNext())
			{
				FillHeader(enumerator.Name, enumerator.Value);
			}
			if (context.State != StreamingContextStates.Remoting || context.Context == null)
			{
				return;
			}
			Header[] array = context.Context as Header[];
			if (array != null)
			{
				for (int i = 0; i < array.Length; i++)
				{
					FillHeader(array[i].Name, array[i].Value);
				}
			}
		}

		internal Type ResolveType()
		{
			Type type = null;
			if (srvID == null)
			{
				srvID = IdentityHolder.CasualResolveIdentity(uri) as ServerIdentity;
			}
			if (srvID != null)
			{
				Type lastCalledType = srvID.GetLastCalledType(typeName);
				if (lastCalledType != null)
				{
					return lastCalledType;
				}
				int num = 0;
				if (string.CompareOrdinal(typeName, 0, "clr:", 0, 4) == 0)
				{
					num = 4;
				}
				int num2 = typeName.IndexOf(',', num);
				if (num2 == -1)
				{
					num2 = typeName.Length;
				}
				lastCalledType = srvID.ServerType;
				type = Type.ResolveTypeRelativeTo(typeName, num, num2 - num, lastCalledType);
			}
			if (type == null)
			{
				type = RemotingServices.InternalGetTypeFromQualifiedTypeName(typeName);
			}
			if (srvID != null)
			{
				srvID.SetLastCalledType(typeName, type);
			}
			return type;
		}

		public void ResolveMethod()
		{
			ResolveMethod(bThrowIfNotResolved: true);
		}

		internal void ResolveMethod(bool bThrowIfNotResolved)
		{
			if (MI != null || methodName == null)
			{
				return;
			}
			RuntimeType runtimeType = ResolveType() as RuntimeType;
			if (methodName.Equals(".ctor"))
			{
				return;
			}
			if (runtimeType == null)
			{
				throw new RemotingException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Remoting_BadType"), typeName));
			}
			if (methodSignature != null)
			{
				try
				{
					MI = runtimeType.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, CallingConventions.Any, methodSignature, null);
				}
				catch (AmbiguousMatchException)
				{
					MemberInfo[] array = runtimeType.FindMembers(MemberTypes.Method, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, Type.FilterName, methodName);
					int num = ((instArgs != null) ? instArgs.Length : 0);
					int num2 = 0;
					for (int i = 0; i < array.Length; i++)
					{
						MethodInfo methodInfo = (MethodInfo)array[i];
						int num3 = (methodInfo.IsGenericMethod ? methodInfo.GetGenericArguments().Length : 0);
						if (num3 == num)
						{
							if (i > num2)
							{
								array[num2] = array[i];
							}
							num2++;
						}
					}
					MethodInfo[] array2 = new MethodInfo[num2];
					for (int j = 0; j < num2; j++)
					{
						array2[j] = (MethodInfo)array[j];
					}
					MI = Type.DefaultBinder.SelectMethod(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, array2, methodSignature, null);
				}
				if (instArgs != null && instArgs.Length > 0)
				{
					MI = ((MethodInfo)MI).MakeGenericMethod(instArgs);
				}
			}
			else
			{
				RemotingTypeCachedData remotingTypeCachedData = null;
				if (instArgs == null)
				{
					remotingTypeCachedData = InternalRemotingServices.GetReflectionCachedData(runtimeType);
					MI = remotingTypeCachedData.GetLastCalledMethod(methodName);
					if (MI != null)
					{
						return;
					}
				}
				bool flag = false;
				try
				{
					MI = runtimeType.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
					if (instArgs != null && instArgs.Length > 0)
					{
						MI = ((MethodInfo)MI).MakeGenericMethod(instArgs);
					}
				}
				catch (AmbiguousMatchException)
				{
					flag = true;
					ResolveOverloadedMethod(runtimeType);
				}
				if (MI != null && !flag)
				{
					remotingTypeCachedData?.SetLastCalledMethod(methodName, MI);
				}
			}
			if (MI != null || !bThrowIfNotResolved)
			{
				return;
			}
			throw new RemotingException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Remoting_Message_MethodMissing"), methodName, typeName));
		}

		private void ResolveOverloadedMethod(RuntimeType t)
		{
			if (args == null)
			{
				return;
			}
			MemberInfo[] member = t.GetMember(methodName, MemberTypes.Method, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public);
			int num = member.Length;
			switch (num)
			{
			case 1:
				MI = member[0] as MethodBase;
				return;
			case 0:
				return;
			}
			int num2 = args.Length;
			MethodBase methodBase = null;
			for (int i = 0; i < num; i++)
			{
				MethodBase methodBase2 = member[i] as MethodBase;
				if (methodBase2.GetParameters().Length == num2)
				{
					if (methodBase != null)
					{
						throw new RemotingException(Environment.GetResourceString("Remoting_AmbiguousMethod"));
					}
					methodBase = methodBase2;
				}
			}
			if (methodBase != null)
			{
				MI = methodBase;
			}
		}

		private void ResolveOverloadedMethod(RuntimeType t, string methodName, ArrayList argNames, ArrayList argValues)
		{
			MemberInfo[] member = t.GetMember(methodName, MemberTypes.Method, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public);
			int num = member.Length;
			switch (num)
			{
			case 1:
				MI = member[0] as MethodBase;
				return;
			case 0:
				return;
			}
			MethodBase methodBase = null;
			for (int i = 0; i < num; i++)
			{
				MethodBase methodBase2 = member[i] as MethodBase;
				ParameterInfo[] parameters = methodBase2.GetParameters();
				if (parameters.Length != argValues.Count)
				{
					continue;
				}
				bool flag = true;
				for (int j = 0; j < parameters.Length; j++)
				{
					Type type = parameters[j].ParameterType;
					if (type.IsByRef)
					{
						type = type.GetElementType();
					}
					if (type != argValues[j].GetType())
					{
						flag = false;
						break;
					}
				}
				if (flag)
				{
					methodBase = methodBase2;
					break;
				}
			}
			if (methodBase == null)
			{
				throw new RemotingException(Environment.GetResourceString("Remoting_AmbiguousMethod"));
			}
			MI = methodBase;
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			throw new NotSupportedException(Environment.GetResourceString("NotSupported_Method"));
		}

		internal void SetObjectFromSoapData(SerializationInfo info)
		{
			methodName = info.GetString("__methodName");
			ArrayList arrayList = (ArrayList)info.GetValue("__paramNameList", typeof(ArrayList));
			Hashtable keyToNamespaceTable = (Hashtable)info.GetValue("__keyToNamespaceTable", typeof(Hashtable));
			if (MI == null)
			{
				ArrayList arrayList2 = new ArrayList();
				ArrayList arrayList3 = arrayList;
				for (int i = 0; i < arrayList3.Count; i++)
				{
					arrayList2.Add(info.GetValue((string)arrayList3[i], typeof(object)));
				}
				RuntimeType runtimeType = ResolveType() as RuntimeType;
				if (runtimeType == null)
				{
					throw new RemotingException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Remoting_BadType"), typeName));
				}
				ResolveOverloadedMethod(runtimeType, methodName, arrayList3, arrayList2);
				if (MI == null)
				{
					throw new RemotingException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Remoting_Message_MethodMissing"), methodName, typeName));
				}
			}
			RemotingMethodCachedData reflectionCachedData = InternalRemotingServices.GetReflectionCachedData(MI);
			ParameterInfo[] parameters = reflectionCachedData.Parameters;
			int[] marshalRequestArgMap = reflectionCachedData.MarshalRequestArgMap;
			int[] outOnlyArgMap = reflectionCachedData.OutOnlyArgMap;
			object obj = ((InternalProperties == null) ? null : InternalProperties["__UnorderedParams"]);
			args = new object[parameters.Length];
			if (obj != null && obj is bool && (bool)obj)
			{
				for (int j = 0; j < arrayList.Count; j++)
				{
					string text = (string)arrayList[j];
					int num = -1;
					for (int k = 0; k < parameters.Length; k++)
					{
						if (text.Equals(parameters[k].Name))
						{
							num = parameters[k].Position;
							break;
						}
					}
					if (num == -1)
					{
						if (!text.StartsWith("__param", StringComparison.Ordinal))
						{
							throw new RemotingException(Environment.GetResourceString("Remoting_Message_BadSerialization"));
						}
						num = int.Parse(text.Substring(7), CultureInfo.InvariantCulture);
					}
					if (num >= args.Length)
					{
						throw new RemotingException(Environment.GetResourceString("Remoting_Message_BadSerialization"));
					}
					args[num] = Message.SoapCoerceArg(info.GetValue(text, typeof(object)), parameters[num].ParameterType, keyToNamespaceTable);
				}
				return;
			}
			for (int l = 0; l < arrayList.Count; l++)
			{
				string name = (string)arrayList[l];
				args[marshalRequestArgMap[l]] = Message.SoapCoerceArg(info.GetValue(name, typeof(object)), parameters[marshalRequestArgMap[l]].ParameterType, keyToNamespaceTable);
			}
			int[] array = outOnlyArgMap;
			foreach (int num2 in array)
			{
				Type elementType = parameters[num2].ParameterType.GetElementType();
				if (elementType.IsValueType)
				{
					args[num2] = Activator.CreateInstance(elementType, nonPublic: true);
				}
			}
		}

		public virtual void Init()
		{
		}

		public object GetArg(int argNum)
		{
			return args[argNum];
		}

		public string GetArgName(int index)
		{
			ResolveMethod();
			RemotingMethodCachedData reflectionCachedData = InternalRemotingServices.GetReflectionCachedData(MI);
			return reflectionCachedData.Parameters[index].Name;
		}

		public object GetInArg(int argNum)
		{
			if (argMapper == null)
			{
				argMapper = new ArgMapper(this, fOut: false);
			}
			return argMapper.GetArg(argNum);
		}

		public string GetInArgName(int index)
		{
			if (argMapper == null)
			{
				argMapper = new ArgMapper(this, fOut: false);
			}
			return argMapper.GetArgName(index);
		}

		internal LogicalCallContext GetLogicalCallContext()
		{
			if (callContext == null)
			{
				callContext = new LogicalCallContext();
			}
			return callContext;
		}

		internal LogicalCallContext SetLogicalCallContext(LogicalCallContext ctx)
		{
			LogicalCallContext result = callContext;
			callContext = ctx;
			return result;
		}

		void IInternalMessage.SetURI(string val)
		{
			uri = val;
		}

		void IInternalMessage.SetCallContext(LogicalCallContext newCallContext)
		{
			callContext = newCallContext;
		}

		bool IInternalMessage.HasProperties()
		{
			if (ExternalProperties == null)
			{
				return InternalProperties != null;
			}
			return true;
		}

		internal void FillHeaders(Header[] h)
		{
			FillHeaders(h, bFromHeaderHandler: false);
		}

		private void FillHeaders(Header[] h, bool bFromHeaderHandler)
		{
			if (h == null)
			{
				return;
			}
			if (bFromHeaderHandler && fSoap)
			{
				foreach (Header header in h)
				{
					if (header.HeaderNamespace == "http://schemas.microsoft.com/clr/soap/messageProperties")
					{
						FillHeader(header.Name, header.Value);
						continue;
					}
					string propertyKeyForHeader = LogicalCallContext.GetPropertyKeyForHeader(header);
					FillHeader(propertyKeyForHeader, header);
				}
			}
			else
			{
				for (int j = 0; j < h.Length; j++)
				{
					FillHeader(h[j].Name, h[j].Value);
				}
			}
		}

		internal virtual bool FillSpecialHeader(string key, object value)
		{
			if (key != null)
			{
				if (key.Equals("__Uri"))
				{
					uri = (string)value;
				}
				else if (key.Equals("__MethodName"))
				{
					methodName = (string)value;
				}
				else if (key.Equals("__MethodSignature"))
				{
					methodSignature = (Type[])value;
				}
				else if (key.Equals("__TypeName"))
				{
					typeName = (string)value;
				}
				else if (key.Equals("__Args"))
				{
					args = (object[])value;
				}
				else
				{
					if (!key.Equals("__CallContext"))
					{
						return false;
					}
					if (value is string)
					{
						callContext = new LogicalCallContext();
						callContext.RemotingData.LogicalCallID = (string)value;
					}
					else
					{
						callContext = (LogicalCallContext)value;
					}
				}
			}
			return true;
		}

		internal void FillHeader(string key, object value)
		{
			if (!FillSpecialHeader(key, value))
			{
				if (InternalProperties == null)
				{
					InternalProperties = new Hashtable();
				}
				InternalProperties[key] = value;
			}
		}

		public virtual object HeaderHandler(Header[] h)
		{
			SerializationMonkey serializationMonkey = (SerializationMonkey)FormatterServices.GetUninitializedObject(typeof(SerializationMonkey));
			Header[] array = null;
			if (h != null && h.Length > 0 && h[0].Name == "__methodName")
			{
				methodName = (string)h[0].Value;
				if (h.Length > 1)
				{
					array = new Header[h.Length - 1];
					Array.Copy(h, 1, array, 0, h.Length - 1);
				}
				else
				{
					array = null;
				}
			}
			else
			{
				array = h;
			}
			FillHeaders(array, bFromHeaderHandler: true);
			ResolveMethod(bThrowIfNotResolved: false);
			serializationMonkey._obj = this;
			if (MI != null)
			{
				ArgMapper argMapper = new ArgMapper(MI, fOut: false);
				serializationMonkey.fieldNames = argMapper.ArgNames;
				serializationMonkey.fieldTypes = argMapper.ArgTypes;
			}
			return serializationMonkey;
		}
	}
}
