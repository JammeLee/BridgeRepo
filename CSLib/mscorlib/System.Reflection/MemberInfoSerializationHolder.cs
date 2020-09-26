using System.Globalization;
using System.Runtime.Serialization;

namespace System.Reflection
{
	[Serializable]
	internal class MemberInfoSerializationHolder : ISerializable, IObjectReference
	{
		private string m_memberName;

		private RuntimeType m_reflectedType;

		private string m_signature;

		private MemberTypes m_memberType;

		private SerializationInfo m_info;

		public static void GetSerializationInfo(SerializationInfo info, string name, Type reflectedClass, string signature, MemberTypes type)
		{
			GetSerializationInfo(info, name, reflectedClass, signature, type, null);
		}

		public static void GetSerializationInfo(SerializationInfo info, string name, Type reflectedClass, string signature, MemberTypes type, Type[] genericArguments)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			string fullName = reflectedClass.Module.Assembly.FullName;
			string fullName2 = reflectedClass.FullName;
			info.SetType(typeof(MemberInfoSerializationHolder));
			info.AddValue("Name", name, typeof(string));
			info.AddValue("AssemblyName", fullName, typeof(string));
			info.AddValue("ClassName", fullName2, typeof(string));
			info.AddValue("Signature", signature, typeof(string));
			info.AddValue("MemberType", (int)type);
			info.AddValue("GenericArguments", genericArguments, typeof(Type[]));
		}

		internal MemberInfoSerializationHolder(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			string @string = info.GetString("AssemblyName");
			string string2 = info.GetString("ClassName");
			if (@string == null || string2 == null)
			{
				throw new SerializationException(Environment.GetResourceString("Serialization_InsufficientState"));
			}
			Assembly assembly = FormatterServices.LoadAssemblyFromString(@string);
			m_reflectedType = assembly.GetType(string2, throwOnError: true, ignoreCase: false) as RuntimeType;
			m_memberName = info.GetString("Name");
			m_signature = info.GetString("Signature");
			m_memberType = (MemberTypes)info.GetInt32("MemberType");
			m_info = info;
		}

		public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			throw new NotSupportedException(Environment.GetResourceString("NotSupported_Method"));
		}

		public virtual object GetRealObject(StreamingContext context)
		{
			if (m_memberName == null || m_reflectedType == null || m_memberType == (MemberTypes)0)
			{
				throw new SerializationException(Environment.GetResourceString("Serialization_InsufficientState"));
			}
			BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.OptionalParamBinding;
			switch (m_memberType)
			{
			case MemberTypes.Field:
			{
				FieldInfo[] array4 = m_reflectedType.GetMember(m_memberName, MemberTypes.Field, bindingAttr) as FieldInfo[];
				if (array4.Length == 0)
				{
					throw new SerializationException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Serialization_UnknownMember"), m_memberName));
				}
				return array4[0];
			}
			case MemberTypes.Event:
			{
				EventInfo[] array3 = m_reflectedType.GetMember(m_memberName, MemberTypes.Event, bindingAttr) as EventInfo[];
				if (array3.Length == 0)
				{
					throw new SerializationException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Serialization_UnknownMember"), m_memberName));
				}
				return array3[0];
			}
			case MemberTypes.Property:
			{
				PropertyInfo[] array6 = m_reflectedType.GetMember(m_memberName, MemberTypes.Property, bindingAttr) as PropertyInfo[];
				if (array6.Length == 0)
				{
					throw new SerializationException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Serialization_UnknownMember"), m_memberName));
				}
				if (array6.Length == 1)
				{
					return array6[0];
				}
				if (array6.Length > 1)
				{
					for (int k = 0; k < array6.Length; k++)
					{
						if (array6[k].ToString().Equals(m_signature))
						{
							return array6[k];
						}
					}
				}
				throw new SerializationException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Serialization_UnknownMember"), m_memberName));
			}
			case MemberTypes.Constructor:
			{
				if (m_signature == null)
				{
					throw new SerializationException(Environment.GetResourceString("Serialization_NullSignature"));
				}
				ConstructorInfo[] array5 = m_reflectedType.GetMember(m_memberName, MemberTypes.Constructor, bindingAttr) as ConstructorInfo[];
				if (array5.Length == 1)
				{
					return array5[0];
				}
				if (array5.Length > 1)
				{
					for (int j = 0; j < array5.Length; j++)
					{
						if (array5[j].ToString().Equals(m_signature))
						{
							return array5[j];
						}
					}
				}
				throw new SerializationException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Serialization_UnknownMember"), m_memberName));
			}
			case MemberTypes.Method:
			{
				MethodInfo methodInfo = null;
				if (m_signature == null)
				{
					throw new SerializationException(Environment.GetResourceString("Serialization_NullSignature"));
				}
				Type[] array = m_info.GetValueNoThrow("GenericArguments", typeof(Type[])) as Type[];
				MethodInfo[] array2 = m_reflectedType.GetMember(m_memberName, MemberTypes.Method, bindingAttr) as MethodInfo[];
				if (array2.Length == 1)
				{
					methodInfo = array2[0];
				}
				else if (array2.Length > 1)
				{
					for (int i = 0; i < array2.Length; i++)
					{
						if (array2[i].ToString().Equals(m_signature))
						{
							methodInfo = array2[i];
							break;
						}
						if (array != null && array2[i].IsGenericMethod && array2[i].GetGenericArguments().Length == array.Length)
						{
							MethodInfo methodInfo2 = array2[i].MakeGenericMethod(array);
							if (methodInfo2.ToString().Equals(m_signature))
							{
								methodInfo = methodInfo2;
								break;
							}
						}
					}
				}
				if (methodInfo == null)
				{
					throw new SerializationException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Serialization_UnknownMember"), m_memberName));
				}
				if (!methodInfo.IsGenericMethodDefinition)
				{
					return methodInfo;
				}
				if (array == null)
				{
					return methodInfo;
				}
				if (array[0] == null)
				{
					return null;
				}
				return methodInfo.MakeGenericMethod(array);
			}
			default:
				throw new ArgumentException(Environment.GetResourceString("Serialization_MemberTypeNotRecognized"));
			}
		}
	}
}
