using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Runtime.Remoting;
using System.Security.Permissions;

namespace System.Runtime.Serialization.Formatters.Binary
{
	internal sealed class WriteObjectInfo
	{
		private static SecurityPermission serializationPermission = new SecurityPermission(SecurityPermissionFlag.SerializationFormatter);

		internal int objectInfoId;

		internal object obj;

		internal Type objectType;

		internal bool isSi;

		internal bool isNamed;

		internal bool isTyped;

		internal bool isArray;

		internal SerializationInfo si;

		internal SerObjectInfoCache cache;

		internal object[] memberData;

		internal ISerializationSurrogate serializationSurrogate;

		internal StreamingContext context;

		internal SerObjectInfoInit serObjectInfoInit;

		internal long objectId;

		internal long assemId;

		internal WriteObjectInfo()
		{
		}

		internal void ObjectEnd()
		{
			PutObjectInfo(serObjectInfoInit, this);
		}

		private void InternalInit()
		{
			obj = null;
			objectType = null;
			isSi = false;
			isNamed = false;
			isTyped = false;
			isArray = false;
			si = null;
			cache = null;
			memberData = null;
			objectId = 0L;
			assemId = 0L;
		}

		internal static WriteObjectInfo Serialize(object obj, ISurrogateSelector surrogateSelector, StreamingContext context, SerObjectInfoInit serObjectInfoInit, IFormatterConverter converter, ObjectWriter objectWriter)
		{
			WriteObjectInfo objectInfo = GetObjectInfo(serObjectInfoInit);
			objectInfo.InitSerialize(obj, surrogateSelector, context, serObjectInfoInit, converter, objectWriter);
			return objectInfo;
		}

		internal void InitSerialize(object obj, ISurrogateSelector surrogateSelector, StreamingContext context, SerObjectInfoInit serObjectInfoInit, IFormatterConverter converter, ObjectWriter objectWriter)
		{
			this.context = context;
			this.obj = obj;
			this.serObjectInfoInit = serObjectInfoInit;
			if (RemotingServices.IsTransparentProxy(obj))
			{
				objectType = Converter.typeofMarshalByRefObject;
			}
			else
			{
				objectType = obj.GetType();
			}
			if (objectType.IsArray)
			{
				isArray = true;
				InitNoMembers();
				return;
			}
			objectWriter.ObjectManager.RegisterObject(obj);
			if (surrogateSelector != null && (serializationSurrogate = surrogateSelector.GetSurrogate(objectType, context, out var _)) != null)
			{
				si = new SerializationInfo(objectType, converter);
				if (!objectType.IsPrimitive)
				{
					serializationSurrogate.GetObjectData(obj, si, context);
				}
				InitSiWrite();
			}
			else if (obj is ISerializable)
			{
				if (!objectType.IsSerializable)
				{
					throw new SerializationException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Serialization_NonSerType"), objectType.FullName, objectType.Assembly.FullName));
				}
				si = new SerializationInfo(objectType, converter, !FormatterServices.UnsafeTypeForwardersIsEnabled());
				((ISerializable)obj).GetObjectData(si, context);
				InitSiWrite();
			}
			else
			{
				InitMemberInfo();
			}
		}

		[Conditional("SER_LOGGING")]
		private void DumpMemberInfo()
		{
			for (int i = 0; i < cache.memberInfos.Length; i++)
			{
			}
		}

		internal static WriteObjectInfo Serialize(Type objectType, ISurrogateSelector surrogateSelector, StreamingContext context, SerObjectInfoInit serObjectInfoInit, IFormatterConverter converter)
		{
			WriteObjectInfo objectInfo = GetObjectInfo(serObjectInfoInit);
			objectInfo.InitSerialize(objectType, surrogateSelector, context, serObjectInfoInit, converter);
			return objectInfo;
		}

		internal void InitSerialize(Type objectType, ISurrogateSelector surrogateSelector, StreamingContext context, SerObjectInfoInit serObjectInfoInit, IFormatterConverter converter)
		{
			this.objectType = objectType;
			this.context = context;
			this.serObjectInfoInit = serObjectInfoInit;
			if (objectType.IsArray)
			{
				InitNoMembers();
				return;
			}
			ISurrogateSelector selector = null;
			if (surrogateSelector != null)
			{
				serializationSurrogate = surrogateSelector.GetSurrogate(objectType, context, out selector);
			}
			if (serializationSurrogate != null)
			{
				si = new SerializationInfo(objectType, converter);
				cache = new SerObjectInfoCache();
				cache.fullTypeName = si.FullTypeName;
				cache.assemblyString = si.AssemblyName;
				isSi = true;
			}
			else if (objectType != Converter.typeofObject && Converter.typeofISerializable.IsAssignableFrom(objectType))
			{
				si = new SerializationInfo(objectType, converter, !FormatterServices.UnsafeTypeForwardersIsEnabled());
				cache = new SerObjectInfoCache();
				cache.fullTypeName = si.FullTypeName;
				cache.assemblyString = si.AssemblyName;
				isSi = true;
			}
			if (!isSi)
			{
				InitMemberInfo();
			}
		}

		private void InitSiWrite()
		{
			SerializationInfoEnumerator serializationInfoEnumerator = null;
			isSi = true;
			serializationInfoEnumerator = si.GetEnumerator();
			int num = 0;
			num = si.MemberCount;
			int num2 = num;
			cache = new SerObjectInfoCache();
			cache.memberNames = new string[num2];
			cache.memberTypes = new Type[num2];
			memberData = new object[num2];
			cache.fullTypeName = si.FullTypeName;
			cache.assemblyString = si.AssemblyName;
			serializationInfoEnumerator = si.GetEnumerator();
			int num3 = 0;
			while (serializationInfoEnumerator.MoveNext())
			{
				cache.memberNames[num3] = serializationInfoEnumerator.Name;
				cache.memberTypes[num3] = serializationInfoEnumerator.ObjectType;
				memberData[num3] = serializationInfoEnumerator.Value;
				num3++;
			}
			isNamed = true;
			isTyped = false;
		}

		private void InitNoMembers()
		{
			cache = (SerObjectInfoCache)serObjectInfoInit.seenBeforeTable[objectType];
			if (cache == null)
			{
				cache = new SerObjectInfoCache();
				cache.fullTypeName = objectType.FullName;
				cache.assemblyString = objectType.Assembly.FullName;
				serObjectInfoInit.seenBeforeTable.Add(objectType, cache);
			}
		}

		private void InitMemberInfo()
		{
			cache = (SerObjectInfoCache)serObjectInfoInit.seenBeforeTable[objectType];
			if (cache == null)
			{
				cache = new SerObjectInfoCache();
				cache.memberInfos = FormatterServices.GetSerializableMembers(objectType, context);
				int num = cache.memberInfos.Length;
				cache.memberNames = new string[num];
				cache.memberTypes = new Type[num];
				for (int i = 0; i < num; i++)
				{
					cache.memberNames[i] = cache.memberInfos[i].Name;
					cache.memberTypes[i] = GetMemberType(cache.memberInfos[i]);
				}
				cache.fullTypeName = objectType.FullName;
				cache.assemblyString = objectType.Assembly.FullName;
				serObjectInfoInit.seenBeforeTable.Add(objectType, cache);
			}
			if (obj != null)
			{
				memberData = FormatterServices.GetObjectData(obj, cache.memberInfos);
			}
			isTyped = true;
			isNamed = true;
		}

		internal string GetTypeFullName()
		{
			return cache.fullTypeName;
		}

		internal string GetAssemblyString()
		{
			return cache.assemblyString;
		}

		internal Type GetMemberType(MemberInfo objMember)
		{
			Type type = null;
			if (objMember is FieldInfo)
			{
				return ((FieldInfo)objMember).FieldType;
			}
			if (objMember is PropertyInfo)
			{
				return ((PropertyInfo)objMember).PropertyType;
			}
			throw new SerializationException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Serialization_SerMemberInfo"), objMember.GetType()));
		}

		internal void GetMemberInfo(out string[] outMemberNames, out Type[] outMemberTypes, out object[] outMemberData)
		{
			outMemberNames = cache.memberNames;
			outMemberTypes = cache.memberTypes;
			outMemberData = memberData;
			if (isSi && !isNamed)
			{
				throw new SerializationException(Environment.GetResourceString("Serialization_ISerializableMemberInfo"));
			}
		}

		private static WriteObjectInfo GetObjectInfo(SerObjectInfoInit serObjectInfoInit)
		{
			WriteObjectInfo writeObjectInfo = null;
			if (!serObjectInfoInit.oiPool.IsEmpty())
			{
				writeObjectInfo = (WriteObjectInfo)serObjectInfoInit.oiPool.Pop();
				writeObjectInfo.InternalInit();
			}
			else
			{
				writeObjectInfo = new WriteObjectInfo();
				writeObjectInfo.objectInfoId = serObjectInfoInit.objectInfoIdCount++;
			}
			return writeObjectInfo;
		}

		private static void PutObjectInfo(SerObjectInfoInit serObjectInfoInit, WriteObjectInfo objectInfo)
		{
			serObjectInfoInit.oiPool.Push(objectInfo);
		}
	}
}
