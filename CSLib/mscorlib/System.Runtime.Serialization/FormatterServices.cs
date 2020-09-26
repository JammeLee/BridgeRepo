using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Contexts;
using System.Runtime.Remoting.Lifetime;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Serialization.Formatters;
using System.Security;
using System.Security.Permissions;
using System.Threading;

namespace System.Runtime.Serialization
{
	[ComVisible(true)]
	public sealed class FormatterServices
	{
		internal static Dictionary<MemberHolder, MemberInfo[]> m_MemberInfoTable = new Dictionary<MemberHolder, MemberInfo[]>(32);

		private static object s_FormatterServicesSyncObject = null;

		private static readonly Type[] advancedTypes = new Type[4]
		{
			typeof(ObjRef),
			typeof(DelegateSerializationHolder),
			typeof(IEnvoyInfo),
			typeof(ISponsor)
		};

		private static Binder s_binder = Type.DefaultBinder;

		private static object formatterServicesSyncObject
		{
			get
			{
				if (s_FormatterServicesSyncObject == null)
				{
					object value = new object();
					Interlocked.CompareExchange(ref s_FormatterServicesSyncObject, value, null);
				}
				return s_FormatterServicesSyncObject;
			}
		}

		private FormatterServices()
		{
			throw new NotSupportedException();
		}

		private static MemberInfo[] GetSerializableMembers(RuntimeType type)
		{
			FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			int num = 0;
			for (int i = 0; i < fields.Length; i++)
			{
				if ((fields[i].Attributes & FieldAttributes.NotSerialized) != FieldAttributes.NotSerialized)
				{
					num++;
				}
			}
			if (num != fields.Length)
			{
				FieldInfo[] array = new FieldInfo[num];
				num = 0;
				for (int j = 0; j < fields.Length; j++)
				{
					if ((fields[j].Attributes & FieldAttributes.NotSerialized) != FieldAttributes.NotSerialized)
					{
						array[num] = fields[j];
						num++;
					}
				}
				return array;
			}
			return fields;
		}

		private static bool CheckSerializable(RuntimeType type)
		{
			if (type.IsSerializable)
			{
				return true;
			}
			return false;
		}

		private static MemberInfo[] InternalGetSerializableMembers(RuntimeType type)
		{
			ArrayList arrayList = null;
			if (type.IsInterface)
			{
				return new MemberInfo[0];
			}
			if (!CheckSerializable(type))
			{
				throw new SerializationException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Serialization_NonSerType"), type.FullName, type.Module.Assembly.FullName));
			}
			MemberInfo[] array = GetSerializableMembers(type);
			RuntimeType runtimeType = (RuntimeType)type.BaseType;
			if (runtimeType != null && runtimeType != typeof(object))
			{
				Type[] parentTypes = null;
				int parentTypeCount = 0;
				bool parentTypes2 = GetParentTypes(runtimeType, out parentTypes, out parentTypeCount);
				if (parentTypeCount > 0)
				{
					arrayList = new ArrayList();
					for (int i = 0; i < parentTypeCount; i++)
					{
						runtimeType = (RuntimeType)parentTypes[i];
						if (!CheckSerializable(runtimeType))
						{
							throw new SerializationException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Serialization_NonSerType"), runtimeType.FullName, runtimeType.Module.Assembly.FullName));
						}
						FieldInfo[] fields = runtimeType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic);
						string namePrefix = (parentTypes2 ? runtimeType.Name : runtimeType.FullName);
						FieldInfo[] array2 = fields;
						foreach (FieldInfo fieldInfo in array2)
						{
							if (!fieldInfo.IsNotSerialized)
							{
								arrayList.Add(new SerializationFieldInfo((RuntimeFieldInfo)fieldInfo, namePrefix));
							}
						}
					}
					if (arrayList != null && arrayList.Count > 0)
					{
						MemberInfo[] array3 = new MemberInfo[arrayList.Count + array.Length];
						Array.Copy(array, array3, array.Length);
						arrayList.CopyTo(array3, array.Length);
						array = array3;
					}
				}
			}
			return array;
		}

		private static bool GetParentTypes(Type parentType, out Type[] parentTypes, out int parentTypeCount)
		{
			parentTypes = null;
			parentTypeCount = 0;
			bool flag = true;
			for (Type type = parentType; type != typeof(object); type = type.BaseType)
			{
				if (!type.IsInterface)
				{
					string name = type.Name;
					int num = 0;
					while (flag && num < parentTypeCount)
					{
						string name2 = parentTypes[num].Name;
						if (name2.Length == name.Length && name2[0] == name[0] && name == name2)
						{
							flag = false;
							break;
						}
						num++;
					}
					if (parentTypes == null || parentTypeCount == parentTypes.Length)
					{
						Type[] array = new Type[Math.Max(parentTypeCount * 2, 12)];
						if (parentTypes != null)
						{
							Array.Copy(parentTypes, 0, array, 0, parentTypeCount);
						}
						parentTypes = array;
					}
					parentTypes[parentTypeCount++] = type;
				}
			}
			return flag;
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
		public static MemberInfo[] GetSerializableMembers(Type type)
		{
			return GetSerializableMembers(type, new StreamingContext(StreamingContextStates.All));
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
		public static MemberInfo[] GetSerializableMembers(Type type, StreamingContext context)
		{
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}
			if (!(type is RuntimeType))
			{
				throw new SerializationException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Serialization_InvalidType"), type.ToString()));
			}
			MemberHolder key = new MemberHolder(type, context);
			if (m_MemberInfoTable.ContainsKey(key))
			{
				return m_MemberInfoTable[key];
			}
			lock (formatterServicesSyncObject)
			{
				if (m_MemberInfoTable.ContainsKey(key))
				{
					return m_MemberInfoTable[key];
				}
				MemberInfo[] array = InternalGetSerializableMembers((RuntimeType)type);
				m_MemberInfoTable[key] = array;
				return array;
			}
		}

		public static void CheckTypeSecurity(Type t, TypeFilterLevel securityLevel)
		{
			if (securityLevel != TypeFilterLevel.Low)
			{
				return;
			}
			for (int i = 0; i < advancedTypes.Length; i++)
			{
				if (advancedTypes[i].IsAssignableFrom(t))
				{
					throw new SecurityException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Serialization_TypeSecurity"), advancedTypes[i].FullName, t.FullName));
				}
			}
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
		public static object GetUninitializedObject(Type type)
		{
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}
			if (!(type is RuntimeType))
			{
				throw new SerializationException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Serialization_InvalidType"), type.ToString()));
			}
			return nativeGetUninitializedObject((RuntimeType)type);
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
		public static object GetSafeUninitializedObject(Type type)
		{
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}
			if (!(type is RuntimeType))
			{
				throw new SerializationException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Serialization_InvalidType"), type.ToString()));
			}
			if (type == typeof(ConstructionCall) || type == typeof(LogicalCallContext) || type == typeof(SynchronizationAttribute))
			{
				return nativeGetUninitializedObject((RuntimeType)type);
			}
			try
			{
				return nativeGetSafeUninitializedObject((RuntimeType)type);
			}
			catch (SecurityException innerException)
			{
				throw new SerializationException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Serialization_Security"), type.FullName), innerException);
			}
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern object nativeGetSafeUninitializedObject(RuntimeType type);

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern object nativeGetUninitializedObject(RuntimeType type);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[SecurityCritical]
		private static extern bool GetEnableUnsafeTypeForwarders();

		[SecuritySafeCritical]
		internal static bool UnsafeTypeForwardersIsEnabled()
		{
			return GetEnableUnsafeTypeForwarders();
		}

		internal static void SerializationSetValue(MemberInfo fi, object target, object value)
		{
			RtFieldInfo rtFieldInfo = fi as RtFieldInfo;
			if (rtFieldInfo != null)
			{
				rtFieldInfo.InternalSetValue(target, value, BindingFlags.Default, s_binder, null, doVisibilityCheck: false);
			}
			else
			{
				((SerializationFieldInfo)fi).InternalSetValue(target, value, BindingFlags.Default, s_binder, null, requiresAccessCheck: false, isBinderDefault: true);
			}
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
		public static object PopulateObjectMembers(object obj, MemberInfo[] members, object[] data)
		{
			if (obj == null)
			{
				throw new ArgumentNullException("obj");
			}
			if (members == null)
			{
				throw new ArgumentNullException("members");
			}
			if (data == null)
			{
				throw new ArgumentNullException("data");
			}
			if (members.Length != data.Length)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_DataLengthDifferent"));
			}
			for (int i = 0; i < members.Length; i++)
			{
				MemberInfo memberInfo = members[i];
				if (memberInfo == null)
				{
					throw new ArgumentNullException("members", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("ArgumentNull_NullMember"), i));
				}
				if (data[i] != null)
				{
					if (memberInfo.MemberType != MemberTypes.Field)
					{
						throw new SerializationException(Environment.GetResourceString("Serialization_UnknownMemberInfo"));
					}
					SerializationSetValue(memberInfo, obj, data[i]);
				}
			}
			return obj;
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
		public static object[] GetObjectData(object obj, MemberInfo[] members)
		{
			if (obj == null)
			{
				throw new ArgumentNullException("obj");
			}
			if (members == null)
			{
				throw new ArgumentNullException("members");
			}
			int num = members.Length;
			object[] array = new object[num];
			for (int i = 0; i < num; i++)
			{
				MemberInfo memberInfo = members[i];
				if (memberInfo == null)
				{
					throw new ArgumentNullException("members", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("ArgumentNull_NullMember"), i));
				}
				if (memberInfo.MemberType == MemberTypes.Field)
				{
					RtFieldInfo rtFieldInfo = memberInfo as RtFieldInfo;
					if (rtFieldInfo != null)
					{
						array[i] = rtFieldInfo.InternalGetValue(obj, doVisibilityCheck: false);
					}
					else
					{
						array[i] = ((SerializationFieldInfo)memberInfo).InternalGetValue(obj, requiresAccessCheck: false);
					}
					continue;
				}
				throw new SerializationException(Environment.GetResourceString("Serialization_UnknownMemberInfo"));
			}
			return array;
		}

		public static ISerializationSurrogate GetSurrogateForCyclicalReference(ISerializationSurrogate innerSurrogate)
		{
			if (innerSurrogate == null)
			{
				throw new ArgumentNullException("innerSurrogate");
			}
			return new SurrogateForCyclicalReference(innerSurrogate);
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
		public static Type GetTypeFromAssembly(Assembly assem, string name)
		{
			if (assem == null)
			{
				throw new ArgumentNullException("assem");
			}
			return assem.GetType(name, throwOnError: false, ignoreCase: false);
		}

		internal static Assembly LoadAssemblyFromString(string assemblyName)
		{
			return Assembly.Load(assemblyName);
		}

		internal static Assembly LoadAssemblyFromStringNoThrow(string assemblyName)
		{
			try
			{
				return LoadAssemblyFromString(assemblyName);
			}
			catch (Exception)
			{
			}
			return null;
		}
	}
}
