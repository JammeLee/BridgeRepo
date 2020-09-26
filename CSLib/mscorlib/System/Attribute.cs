using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;

namespace System
{
	[Serializable]
	[AttributeUsage(AttributeTargets.All, Inherited = true, AllowMultiple = false)]
	[ClassInterface(ClassInterfaceType.None)]
	[ComDefaultInterface(typeof(_Attribute))]
	[ComVisible(true)]
	public abstract class Attribute : _Attribute
	{
		public virtual object TypeId => GetType();

		private static Attribute[] InternalGetCustomAttributes(PropertyInfo element, Type type, bool inherit)
		{
			Attribute[] array = (Attribute[])element.GetCustomAttributes(type, inherit);
			if (!inherit)
			{
				return array;
			}
			Hashtable types = new Hashtable(11);
			ArrayList arrayList = new ArrayList();
			CopyToArrayList(arrayList, array, types);
			for (PropertyInfo parentDefinition = GetParentDefinition(element); parentDefinition != null; parentDefinition = GetParentDefinition(parentDefinition))
			{
				array = GetCustomAttributes(parentDefinition, type, inherit: false);
				AddAttributesToList(arrayList, array, types);
			}
			return (Attribute[])arrayList.ToArray(type);
		}

		private static bool InternalIsDefined(PropertyInfo element, Type attributeType, bool inherit)
		{
			if (element.IsDefined(attributeType, inherit))
			{
				return true;
			}
			if (inherit)
			{
				AttributeUsageAttribute attributeUsageAttribute = InternalGetAttributeUsage(attributeType);
				if (!attributeUsageAttribute.Inherited)
				{
					return false;
				}
				for (PropertyInfo parentDefinition = GetParentDefinition(element); parentDefinition != null; parentDefinition = GetParentDefinition(parentDefinition))
				{
					if (parentDefinition.IsDefined(attributeType, inherit: false))
					{
						return true;
					}
				}
			}
			return false;
		}

		private static PropertyInfo GetParentDefinition(PropertyInfo property)
		{
			MethodInfo methodInfo = property.GetGetMethod(nonPublic: true);
			if (methodInfo == null)
			{
				methodInfo = property.GetSetMethod(nonPublic: true);
			}
			if (methodInfo != null)
			{
				methodInfo = methodInfo.GetParentDefinition();
				if (methodInfo != null)
				{
					return methodInfo.DeclaringType.GetProperty(property.Name, property.PropertyType);
				}
			}
			return null;
		}

		private static Attribute[] InternalGetCustomAttributes(EventInfo element, Type type, bool inherit)
		{
			Attribute[] array = (Attribute[])element.GetCustomAttributes(type, inherit);
			if (inherit)
			{
				Hashtable types = new Hashtable(11);
				ArrayList arrayList = new ArrayList();
				CopyToArrayList(arrayList, array, types);
				for (EventInfo parentDefinition = GetParentDefinition(element); parentDefinition != null; parentDefinition = GetParentDefinition(parentDefinition))
				{
					array = GetCustomAttributes(parentDefinition, type, inherit: false);
					AddAttributesToList(arrayList, array, types);
				}
				return (Attribute[])arrayList.ToArray(type);
			}
			return array;
		}

		private static EventInfo GetParentDefinition(EventInfo ev)
		{
			MethodInfo addMethod = ev.GetAddMethod(nonPublic: true);
			if (addMethod != null)
			{
				addMethod = addMethod.GetParentDefinition();
				if (addMethod != null)
				{
					return addMethod.DeclaringType.GetEvent(ev.Name);
				}
			}
			return null;
		}

		private static bool InternalIsDefined(EventInfo element, Type attributeType, bool inherit)
		{
			if (element.IsDefined(attributeType, inherit))
			{
				return true;
			}
			if (inherit)
			{
				AttributeUsageAttribute attributeUsageAttribute = InternalGetAttributeUsage(attributeType);
				if (!attributeUsageAttribute.Inherited)
				{
					return false;
				}
				for (EventInfo parentDefinition = GetParentDefinition(element); parentDefinition != null; parentDefinition = GetParentDefinition(parentDefinition))
				{
					if (parentDefinition.IsDefined(attributeType, inherit: false))
					{
						return true;
					}
				}
			}
			return false;
		}

		private static Attribute[] InternalParamGetCustomAttributes(MethodInfo method, ParameterInfo param, Type type, bool inherit)
		{
			ArrayList arrayList = new ArrayList();
			if (type == null)
			{
				type = typeof(Attribute);
			}
			object[] customAttributes = param.GetCustomAttributes(type, inherit: false);
			for (int i = 0; i < customAttributes.Length; i++)
			{
				Type type2 = customAttributes[i].GetType();
				AttributeUsageAttribute attributeUsageAttribute = InternalGetAttributeUsage(type2);
				if (!attributeUsageAttribute.AllowMultiple)
				{
					arrayList.Add(type2);
				}
			}
			Attribute[] array = null;
			array = ((customAttributes.Length != 0) ? ((Attribute[])customAttributes) : ((Attribute[])Array.CreateInstance(type, 0)));
			if (method.DeclaringType == null)
			{
				return array;
			}
			if (!inherit)
			{
				return array;
			}
			int position = param.Position;
			for (method = method.GetParentDefinition(); method != null; method = method.GetParentDefinition())
			{
				ParameterInfo[] parameters = method.GetParameters();
				param = parameters[position];
				customAttributes = param.GetCustomAttributes(type, inherit: false);
				int num = 0;
				for (int j = 0; j < customAttributes.Length; j++)
				{
					Type type3 = customAttributes[j].GetType();
					AttributeUsageAttribute attributeUsageAttribute2 = InternalGetAttributeUsage(type3);
					if (attributeUsageAttribute2.Inherited && !arrayList.Contains(type3))
					{
						if (!attributeUsageAttribute2.AllowMultiple)
						{
							arrayList.Add(type3);
						}
						num++;
					}
					else
					{
						customAttributes[j] = null;
					}
				}
				Attribute[] array2 = (Attribute[])Array.CreateInstance(type, num);
				num = 0;
				for (int k = 0; k < customAttributes.Length; k++)
				{
					if (customAttributes[k] != null)
					{
						array2[num] = (Attribute)customAttributes[k];
						num++;
					}
				}
				Attribute[] array3 = array;
				array = (Attribute[])Array.CreateInstance(type, array3.Length + num);
				Array.Copy(array3, array, array3.Length);
				int num2 = array3.Length;
				for (int l = 0; l < array2.Length; l++)
				{
					array[num2 + l] = array2[l];
				}
			}
			return array;
		}

		private static bool InternalParamIsDefined(MethodInfo method, ParameterInfo param, Type type, bool inherit)
		{
			if (param.IsDefined(type, inherit: false))
			{
				return true;
			}
			if (method.DeclaringType == null || !inherit)
			{
				return false;
			}
			int position = param.Position;
			for (method = method.GetParentDefinition(); method != null; method = method.GetParentDefinition())
			{
				ParameterInfo[] parameters = method.GetParameters();
				param = parameters[position];
				object[] customAttributes = param.GetCustomAttributes(type, inherit: false);
				for (int i = 0; i < customAttributes.Length; i++)
				{
					Type type2 = customAttributes[i].GetType();
					AttributeUsageAttribute attributeUsageAttribute = InternalGetAttributeUsage(type2);
					if (customAttributes[i] is Attribute && attributeUsageAttribute.Inherited)
					{
						return true;
					}
				}
			}
			return false;
		}

		private static void CopyToArrayList(ArrayList attributeList, Attribute[] attributes, Hashtable types)
		{
			for (int i = 0; i < attributes.Length; i++)
			{
				attributeList.Add(attributes[i]);
				Type type = attributes[i].GetType();
				if (!types.Contains(type))
				{
					types[type] = InternalGetAttributeUsage(type);
				}
			}
		}

		private static void AddAttributesToList(ArrayList attributeList, Attribute[] attributes, Hashtable types)
		{
			for (int i = 0; i < attributes.Length; i++)
			{
				Type type = attributes[i].GetType();
				AttributeUsageAttribute attributeUsageAttribute = (AttributeUsageAttribute)types[type];
				if (attributeUsageAttribute == null)
				{
					attributeUsageAttribute = (AttributeUsageAttribute)(types[type] = InternalGetAttributeUsage(type));
					if (attributeUsageAttribute.Inherited)
					{
						attributeList.Add(attributes[i]);
					}
				}
				else if (attributeUsageAttribute.Inherited && attributeUsageAttribute.AllowMultiple)
				{
					attributeList.Add(attributes[i]);
				}
			}
		}

		private static AttributeUsageAttribute InternalGetAttributeUsage(Type type)
		{
			object[] customAttributes = type.GetCustomAttributes(typeof(AttributeUsageAttribute), inherit: false);
			if (customAttributes.Length == 1)
			{
				return (AttributeUsageAttribute)customAttributes[0];
			}
			if (customAttributes.Length == 0)
			{
				return AttributeUsageAttribute.Default;
			}
			throw new FormatException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Format_AttributeUsage"), type));
		}

		public static Attribute[] GetCustomAttributes(MemberInfo element, Type type)
		{
			return GetCustomAttributes(element, type, inherit: true);
		}

		public static Attribute[] GetCustomAttributes(MemberInfo element, Type type, bool inherit)
		{
			if (element == null)
			{
				throw new ArgumentNullException("element");
			}
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}
			if (!type.IsSubclassOf(typeof(Attribute)) && type != typeof(Attribute))
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_MustHaveAttributeBaseClass"));
			}
			return element.MemberType switch
			{
				MemberTypes.Property => InternalGetCustomAttributes((PropertyInfo)element, type, inherit), 
				MemberTypes.Event => InternalGetCustomAttributes((EventInfo)element, type, inherit), 
				_ => element.GetCustomAttributes(type, inherit) as Attribute[], 
			};
		}

		public static Attribute[] GetCustomAttributes(MemberInfo element)
		{
			return GetCustomAttributes(element, inherit: true);
		}

		public static Attribute[] GetCustomAttributes(MemberInfo element, bool inherit)
		{
			if (element == null)
			{
				throw new ArgumentNullException("element");
			}
			return element.MemberType switch
			{
				MemberTypes.Property => InternalGetCustomAttributes((PropertyInfo)element, typeof(Attribute), inherit), 
				MemberTypes.Event => InternalGetCustomAttributes((EventInfo)element, typeof(Attribute), inherit), 
				_ => element.GetCustomAttributes(typeof(Attribute), inherit) as Attribute[], 
			};
		}

		public static bool IsDefined(MemberInfo element, Type attributeType)
		{
			return IsDefined(element, attributeType, inherit: true);
		}

		public static bool IsDefined(MemberInfo element, Type attributeType, bool inherit)
		{
			if (element == null)
			{
				throw new ArgumentNullException("element");
			}
			if (attributeType == null)
			{
				throw new ArgumentNullException("attributeType");
			}
			if (!attributeType.IsSubclassOf(typeof(Attribute)) && attributeType != typeof(Attribute))
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_MustHaveAttributeBaseClass"));
			}
			return element.MemberType switch
			{
				MemberTypes.Property => InternalIsDefined((PropertyInfo)element, attributeType, inherit), 
				MemberTypes.Event => InternalIsDefined((EventInfo)element, attributeType, inherit), 
				_ => element.IsDefined(attributeType, inherit), 
			};
		}

		public static Attribute GetCustomAttribute(MemberInfo element, Type attributeType)
		{
			return GetCustomAttribute(element, attributeType, inherit: true);
		}

		public static Attribute GetCustomAttribute(MemberInfo element, Type attributeType, bool inherit)
		{
			Attribute[] customAttributes = GetCustomAttributes(element, attributeType, inherit);
			if (customAttributes == null || customAttributes.Length == 0)
			{
				return null;
			}
			if (customAttributes.Length == 1)
			{
				return customAttributes[0];
			}
			throw new AmbiguousMatchException(Environment.GetResourceString("RFLCT.AmbigCust"));
		}

		public static Attribute[] GetCustomAttributes(ParameterInfo element)
		{
			return GetCustomAttributes(element, inherit: true);
		}

		public static Attribute[] GetCustomAttributes(ParameterInfo element, Type attributeType)
		{
			return GetCustomAttributes(element, attributeType, inherit: true);
		}

		public static Attribute[] GetCustomAttributes(ParameterInfo element, Type attributeType, bool inherit)
		{
			if (element == null)
			{
				throw new ArgumentNullException("element");
			}
			if (attributeType == null)
			{
				throw new ArgumentNullException("attributeType");
			}
			if (!attributeType.IsSubclassOf(typeof(Attribute)) && attributeType != typeof(Attribute))
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_MustHaveAttributeBaseClass"));
			}
			MemberInfo member = element.Member;
			if (member.MemberType == MemberTypes.Method && inherit)
			{
				return InternalParamGetCustomAttributes((MethodInfo)member, element, attributeType, inherit);
			}
			return element.GetCustomAttributes(attributeType, inherit) as Attribute[];
		}

		public static Attribute[] GetCustomAttributes(ParameterInfo element, bool inherit)
		{
			if (element == null)
			{
				throw new ArgumentNullException("element");
			}
			MemberInfo member = element.Member;
			if (member.MemberType == MemberTypes.Method && inherit)
			{
				return InternalParamGetCustomAttributes((MethodInfo)member, element, null, inherit);
			}
			return element.GetCustomAttributes(typeof(Attribute), inherit) as Attribute[];
		}

		public static bool IsDefined(ParameterInfo element, Type attributeType)
		{
			return IsDefined(element, attributeType, inherit: true);
		}

		public static bool IsDefined(ParameterInfo element, Type attributeType, bool inherit)
		{
			if (element == null)
			{
				throw new ArgumentNullException("element");
			}
			if (attributeType == null)
			{
				throw new ArgumentNullException("attributeType");
			}
			if (!attributeType.IsSubclassOf(typeof(Attribute)) && attributeType != typeof(Attribute))
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_MustHaveAttributeBaseClass"));
			}
			MemberInfo member = element.Member;
			return member.MemberType switch
			{
				MemberTypes.Method => InternalParamIsDefined((MethodInfo)member, element, attributeType, inherit), 
				MemberTypes.Constructor => element.IsDefined(attributeType, inherit: false), 
				MemberTypes.Property => element.IsDefined(attributeType, inherit: false), 
				_ => throw new ArgumentException(Environment.GetResourceString("Argument_InvalidParamInfo")), 
			};
		}

		public static Attribute GetCustomAttribute(ParameterInfo element, Type attributeType)
		{
			return GetCustomAttribute(element, attributeType, inherit: true);
		}

		public static Attribute GetCustomAttribute(ParameterInfo element, Type attributeType, bool inherit)
		{
			Attribute[] customAttributes = GetCustomAttributes(element, attributeType, inherit);
			if (customAttributes == null || customAttributes.Length == 0)
			{
				return null;
			}
			if (customAttributes.Length == 0)
			{
				return null;
			}
			if (customAttributes.Length == 1)
			{
				return customAttributes[0];
			}
			throw new AmbiguousMatchException(Environment.GetResourceString("RFLCT.AmbigCust"));
		}

		public static Attribute[] GetCustomAttributes(Module element, Type attributeType)
		{
			return GetCustomAttributes(element, attributeType, inherit: true);
		}

		public static Attribute[] GetCustomAttributes(Module element)
		{
			return GetCustomAttributes(element, inherit: true);
		}

		public static Attribute[] GetCustomAttributes(Module element, bool inherit)
		{
			if (element == null)
			{
				throw new ArgumentNullException("element");
			}
			return (Attribute[])element.GetCustomAttributes(typeof(Attribute), inherit);
		}

		public static Attribute[] GetCustomAttributes(Module element, Type attributeType, bool inherit)
		{
			if (element == null)
			{
				throw new ArgumentNullException("element");
			}
			if (attributeType == null)
			{
				throw new ArgumentNullException("attributeType");
			}
			if (!attributeType.IsSubclassOf(typeof(Attribute)) && attributeType != typeof(Attribute))
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_MustHaveAttributeBaseClass"));
			}
			return (Attribute[])element.GetCustomAttributes(attributeType, inherit);
		}

		public static bool IsDefined(Module element, Type attributeType)
		{
			return IsDefined(element, attributeType, inherit: false);
		}

		public static bool IsDefined(Module element, Type attributeType, bool inherit)
		{
			if (element == null)
			{
				throw new ArgumentNullException("element");
			}
			if (attributeType == null)
			{
				throw new ArgumentNullException("attributeType");
			}
			if (!attributeType.IsSubclassOf(typeof(Attribute)) && attributeType != typeof(Attribute))
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_MustHaveAttributeBaseClass"));
			}
			return element.IsDefined(attributeType, inherit: false);
		}

		public static Attribute GetCustomAttribute(Module element, Type attributeType)
		{
			return GetCustomAttribute(element, attributeType, inherit: true);
		}

		public static Attribute GetCustomAttribute(Module element, Type attributeType, bool inherit)
		{
			Attribute[] customAttributes = GetCustomAttributes(element, attributeType, inherit);
			if (customAttributes == null || customAttributes.Length == 0)
			{
				return null;
			}
			if (customAttributes.Length == 1)
			{
				return customAttributes[0];
			}
			throw new AmbiguousMatchException(Environment.GetResourceString("RFLCT.AmbigCust"));
		}

		public static Attribute[] GetCustomAttributes(Assembly element, Type attributeType)
		{
			return GetCustomAttributes(element, attributeType, inherit: true);
		}

		public static Attribute[] GetCustomAttributes(Assembly element, Type attributeType, bool inherit)
		{
			if (element == null)
			{
				throw new ArgumentNullException("element");
			}
			if (attributeType == null)
			{
				throw new ArgumentNullException("attributeType");
			}
			if (!attributeType.IsSubclassOf(typeof(Attribute)) && attributeType != typeof(Attribute))
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_MustHaveAttributeBaseClass"));
			}
			return (Attribute[])element.GetCustomAttributes(attributeType, inherit);
		}

		public static Attribute[] GetCustomAttributes(Assembly element)
		{
			return GetCustomAttributes(element, inherit: true);
		}

		public static Attribute[] GetCustomAttributes(Assembly element, bool inherit)
		{
			if (element == null)
			{
				throw new ArgumentNullException("element");
			}
			return (Attribute[])element.GetCustomAttributes(typeof(Attribute), inherit);
		}

		public static bool IsDefined(Assembly element, Type attributeType)
		{
			return IsDefined(element, attributeType, inherit: true);
		}

		public static bool IsDefined(Assembly element, Type attributeType, bool inherit)
		{
			if (element == null)
			{
				throw new ArgumentNullException("element");
			}
			if (attributeType == null)
			{
				throw new ArgumentNullException("attributeType");
			}
			if (!attributeType.IsSubclassOf(typeof(Attribute)) && attributeType != typeof(Attribute))
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_MustHaveAttributeBaseClass"));
			}
			return element.IsDefined(attributeType, inherit: false);
		}

		public static Attribute GetCustomAttribute(Assembly element, Type attributeType)
		{
			return GetCustomAttribute(element, attributeType, inherit: true);
		}

		public static Attribute GetCustomAttribute(Assembly element, Type attributeType, bool inherit)
		{
			Attribute[] customAttributes = GetCustomAttributes(element, attributeType, inherit);
			if (customAttributes == null || customAttributes.Length == 0)
			{
				return null;
			}
			if (customAttributes.Length == 1)
			{
				return customAttributes[0];
			}
			throw new AmbiguousMatchException(Environment.GetResourceString("RFLCT.AmbigCust"));
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return false;
			}
			RuntimeType runtimeType = (RuntimeType)GetType();
			RuntimeType runtimeType2 = (RuntimeType)obj.GetType();
			if (runtimeType2 != runtimeType)
			{
				return false;
			}
			FieldInfo[] fields = runtimeType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			for (int i = 0; i < fields.Length; i++)
			{
				object value = ((RuntimeFieldInfo)fields[i]).GetValue(this);
				object value2 = ((RuntimeFieldInfo)fields[i]).GetValue(obj);
				if (value == null)
				{
					if (value2 != null)
					{
						return false;
					}
				}
				else if (!value.Equals(value2))
				{
					return false;
				}
			}
			return true;
		}

		public override int GetHashCode()
		{
			Type type = GetType();
			FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			object obj = null;
			foreach (FieldInfo fieldInfo in fields)
			{
				obj = fieldInfo.GetValue(this);
				if (obj != null)
				{
					break;
				}
			}
			return obj?.GetHashCode() ?? type.GetHashCode();
		}

		public virtual bool Match(object obj)
		{
			return Equals(obj);
		}

		public virtual bool IsDefaultAttribute()
		{
			return false;
		}

		void _Attribute.GetTypeInfoCount(out uint pcTInfo)
		{
			throw new NotImplementedException();
		}

		void _Attribute.GetTypeInfo(uint iTInfo, uint lcid, IntPtr ppTInfo)
		{
			throw new NotImplementedException();
		}

		void _Attribute.GetIDsOfNames([In] ref Guid riid, IntPtr rgszNames, uint cNames, uint lcid, IntPtr rgDispId)
		{
			throw new NotImplementedException();
		}

		void _Attribute.Invoke(uint dispIdMember, [In] ref Guid riid, uint lcid, short wFlags, IntPtr pDispParams, IntPtr pVarResult, IntPtr pExcepInfo, IntPtr puArgErr)
		{
			throw new NotImplementedException();
		}
	}
}
