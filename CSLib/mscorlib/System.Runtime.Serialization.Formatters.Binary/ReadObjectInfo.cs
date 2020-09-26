using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;

namespace System.Runtime.Serialization.Formatters.Binary
{
	internal sealed class ReadObjectInfo
	{
		internal int objectInfoId;

		internal static int readObjectInfoCounter = 1;

		internal Type objectType;

		internal ObjectManager objectManager;

		internal int count;

		internal bool isSi;

		internal bool isNamed;

		internal bool isTyped;

		internal bool bSimpleAssembly;

		internal SerObjectInfoCache cache;

		internal string[] wireMemberNames;

		internal Type[] wireMemberTypes;

		private int lastPosition;

		internal ISurrogateSelector surrogateSelector;

		internal ISerializationSurrogate serializationSurrogate;

		internal StreamingContext context;

		internal ArrayList memberTypesList;

		internal SerObjectInfoInit serObjectInfoInit;

		internal IFormatterConverter formatterConverter;

		internal ReadObjectInfo()
		{
		}

		internal void ObjectEnd()
		{
		}

		internal void PrepareForReuse()
		{
			lastPosition = 0;
		}

		internal static ReadObjectInfo Create(Type objectType, ISurrogateSelector surrogateSelector, StreamingContext context, ObjectManager objectManager, SerObjectInfoInit serObjectInfoInit, IFormatterConverter converter, bool bSimpleAssembly)
		{
			ReadObjectInfo objectInfo = GetObjectInfo(serObjectInfoInit);
			objectInfo.Init(objectType, surrogateSelector, context, objectManager, serObjectInfoInit, converter, bSimpleAssembly);
			return objectInfo;
		}

		internal void Init(Type objectType, ISurrogateSelector surrogateSelector, StreamingContext context, ObjectManager objectManager, SerObjectInfoInit serObjectInfoInit, IFormatterConverter converter, bool bSimpleAssembly)
		{
			this.objectType = objectType;
			this.objectManager = objectManager;
			this.context = context;
			this.serObjectInfoInit = serObjectInfoInit;
			formatterConverter = converter;
			this.bSimpleAssembly = bSimpleAssembly;
			InitReadConstructor(objectType, surrogateSelector, context);
		}

		internal static ReadObjectInfo Create(Type objectType, string[] memberNames, Type[] memberTypes, ISurrogateSelector surrogateSelector, StreamingContext context, ObjectManager objectManager, SerObjectInfoInit serObjectInfoInit, IFormatterConverter converter, bool bSimpleAssembly)
		{
			ReadObjectInfo objectInfo = GetObjectInfo(serObjectInfoInit);
			objectInfo.Init(objectType, memberNames, memberTypes, surrogateSelector, context, objectManager, serObjectInfoInit, converter, bSimpleAssembly);
			return objectInfo;
		}

		internal void Init(Type objectType, string[] memberNames, Type[] memberTypes, ISurrogateSelector surrogateSelector, StreamingContext context, ObjectManager objectManager, SerObjectInfoInit serObjectInfoInit, IFormatterConverter converter, bool bSimpleAssembly)
		{
			this.objectType = objectType;
			this.objectManager = objectManager;
			wireMemberNames = memberNames;
			wireMemberTypes = memberTypes;
			this.context = context;
			this.serObjectInfoInit = serObjectInfoInit;
			formatterConverter = converter;
			this.bSimpleAssembly = bSimpleAssembly;
			if (memberNames != null)
			{
				isNamed = true;
			}
			if (memberTypes != null)
			{
				isTyped = true;
			}
			if (objectType != null)
			{
				InitReadConstructor(objectType, surrogateSelector, context);
			}
		}

		private void InitReadConstructor(Type objectType, ISurrogateSelector surrogateSelector, StreamingContext context)
		{
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
				isSi = true;
			}
			else if (objectType != Converter.typeofObject && Converter.typeofISerializable.IsAssignableFrom(objectType))
			{
				isSi = true;
			}
			if (isSi)
			{
				InitSiRead();
			}
			else
			{
				InitMemberInfo();
			}
		}

		private void InitSiRead()
		{
			if (memberTypesList != null)
			{
				memberTypesList = new ArrayList(20);
			}
		}

		private void InitNoMembers()
		{
			cache = new SerObjectInfoCache();
			cache.fullTypeName = objectType.FullName;
			cache.assemblyString = objectType.Assembly.FullName;
		}

		private void InitMemberInfo()
		{
			cache = new SerObjectInfoCache();
			cache.memberInfos = FormatterServices.GetSerializableMembers(objectType, context);
			count = cache.memberInfos.Length;
			cache.memberNames = new string[count];
			cache.memberTypes = new Type[count];
			for (int i = 0; i < count; i++)
			{
				cache.memberNames[i] = cache.memberInfos[i].Name;
				cache.memberTypes[i] = GetMemberType(cache.memberInfos[i]);
			}
			cache.fullTypeName = objectType.FullName;
			cache.assemblyString = objectType.Assembly.FullName;
			isTyped = true;
			isNamed = true;
		}

		internal MemberInfo GetMemberInfo(string name)
		{
			if (cache == null)
			{
				return null;
			}
			if (isSi)
			{
				throw new SerializationException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Serialization_MemberInfo"), string.Concat(objectType, " ", name)));
			}
			if (cache.memberInfos == null)
			{
				throw new SerializationException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Serialization_NoMemberInfo"), string.Concat(objectType, " ", name)));
			}
			int num = Position(name);
			if (num != -1)
			{
				return cache.memberInfos[Position(name)];
			}
			return null;
		}

		internal Type GetType(string name)
		{
			Type type = null;
			int num = Position(name);
			if (num == -1)
			{
				return null;
			}
			type = ((!isTyped) ? ((Type)memberTypesList[num]) : cache.memberTypes[num]);
			if (type == null)
			{
				throw new SerializationException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Serialization_ISerializableTypes"), string.Concat(objectType, " ", name)));
			}
			return type;
		}

		internal void AddValue(string name, object value, ref SerializationInfo si, ref object[] memberData)
		{
			if (isSi)
			{
				si.AddValue(name, value);
				return;
			}
			int num = Position(name);
			if (num != -1)
			{
				memberData[num] = value;
			}
		}

		internal void InitDataStore(ref SerializationInfo si, ref object[] memberData)
		{
			if (isSi)
			{
				if (si == null)
				{
					si = new SerializationInfo(objectType, formatterConverter);
				}
			}
			else if (memberData == null && cache != null)
			{
				memberData = new object[cache.memberNames.Length];
			}
		}

		internal void RecordFixup(long objectId, string name, long idRef)
		{
			if (isSi)
			{
				objectManager.RecordDelayedFixup(objectId, name, idRef);
				return;
			}
			int num = Position(name);
			if (num != -1)
			{
				objectManager.RecordFixup(objectId, cache.memberInfos[num], idRef);
			}
		}

		internal void PopulateObjectMembers(object obj, object[] memberData)
		{
			if (!isSi && memberData != null)
			{
				FormatterServices.PopulateObjectMembers(obj, cache.memberInfos, memberData);
			}
		}

		[Conditional("SER_LOGGING")]
		private void DumpPopulate(MemberInfo[] memberInfos, object[] memberData)
		{
			for (int i = 0; i < memberInfos.Length; i++)
			{
			}
		}

		[Conditional("SER_LOGGING")]
		private void DumpPopulateSi()
		{
		}

		private int Position(string name)
		{
			if (cache == null)
			{
				return -1;
			}
			if (cache.memberNames.Length > 0 && cache.memberNames[lastPosition].Equals(name))
			{
				return lastPosition;
			}
			if (++lastPosition < cache.memberNames.Length && cache.memberNames[lastPosition].Equals(name))
			{
				return lastPosition;
			}
			for (int i = 0; i < cache.memberNames.Length; i++)
			{
				if (cache.memberNames[i].Equals(name))
				{
					lastPosition = i;
					return lastPosition;
				}
			}
			lastPosition = 0;
			return -1;
		}

		internal Type[] GetMemberTypes(string[] inMemberNames, Type objectType)
		{
			if (isSi)
			{
				throw new SerializationException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Serialization_ISerializableTypes"), objectType));
			}
			if (cache == null)
			{
				return null;
			}
			if (cache.memberTypes == null)
			{
				cache.memberTypes = new Type[count];
				for (int i = 0; i < count; i++)
				{
					cache.memberTypes[i] = GetMemberType(cache.memberInfos[i]);
				}
			}
			bool flag = false;
			if (inMemberNames.Length < cache.memberInfos.Length)
			{
				flag = true;
			}
			Type[] array = new Type[cache.memberInfos.Length];
			bool flag2 = false;
			for (int j = 0; j < cache.memberInfos.Length; j++)
			{
				if (!flag && inMemberNames[j].Equals(cache.memberInfos[j].Name))
				{
					array[j] = cache.memberTypes[j];
					continue;
				}
				flag2 = false;
				for (int k = 0; k < inMemberNames.Length; k++)
				{
					if (cache.memberInfos[j].Name.Equals(inMemberNames[k]))
					{
						array[j] = cache.memberTypes[j];
						flag2 = true;
						break;
					}
				}
				if (!flag2)
				{
					object[] customAttributes = cache.memberInfos[j].GetCustomAttributes(typeof(OptionalFieldAttribute), inherit: false);
					if ((customAttributes == null || customAttributes.Length == 0) && !bSimpleAssembly)
					{
						throw new SerializationException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Serialization_MissingMember"), cache.memberNames[j], objectType, typeof(OptionalFieldAttribute).FullName));
					}
				}
			}
			return array;
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

		private static ReadObjectInfo GetObjectInfo(SerObjectInfoInit serObjectInfoInit)
		{
			ReadObjectInfo readObjectInfo = new ReadObjectInfo();
			readObjectInfo.objectInfoId = readObjectInfoCounter++;
			return readObjectInfo;
		}
	}
}
