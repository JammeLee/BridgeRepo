using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;

namespace System.Reflection
{
	[Serializable]
	[ComVisible(true)]
	public sealed class CustomAttributeData
	{
		private ConstructorInfo m_ctor;

		private Module m_scope;

		private MemberInfo[] m_members;

		private CustomAttributeCtorParameter[] m_ctorParams;

		private CustomAttributeNamedParameter[] m_namedParams;

		private IList<CustomAttributeTypedArgument> m_typedCtorArgs;

		private IList<CustomAttributeNamedArgument> m_namedArgs;

		[ComVisible(true)]
		public ConstructorInfo Constructor => m_ctor;

		[ComVisible(true)]
		public IList<CustomAttributeTypedArgument> ConstructorArguments
		{
			get
			{
				if (m_typedCtorArgs == null)
				{
					CustomAttributeTypedArgument[] array = new CustomAttributeTypedArgument[m_ctorParams.Length];
					for (int i = 0; i < array.Length; i++)
					{
						_ = m_ctorParams[i].CustomAttributeEncodedArgument;
						ref CustomAttributeTypedArgument reference = ref array[i];
						reference = new CustomAttributeTypedArgument(m_scope, m_ctorParams[i].CustomAttributeEncodedArgument);
					}
					m_typedCtorArgs = Array.AsReadOnly(array);
				}
				return m_typedCtorArgs;
			}
		}

		public IList<CustomAttributeNamedArgument> NamedArguments
		{
			get
			{
				if (m_namedArgs == null)
				{
					if (m_namedParams == null)
					{
						return null;
					}
					int num = 0;
					for (int i = 0; i < m_namedParams.Length; i++)
					{
						if (m_namedParams[i].EncodedArgument.CustomAttributeType.EncodedType != 0)
						{
							num++;
						}
					}
					CustomAttributeNamedArgument[] array = new CustomAttributeNamedArgument[num];
					int j = 0;
					int num2 = 0;
					for (; j < m_namedParams.Length; j++)
					{
						if (m_namedParams[j].EncodedArgument.CustomAttributeType.EncodedType != 0)
						{
							ref CustomAttributeNamedArgument reference = ref array[num2++];
							reference = new CustomAttributeNamedArgument(m_members[j], new CustomAttributeTypedArgument(m_scope, m_namedParams[j].EncodedArgument));
						}
					}
					m_namedArgs = Array.AsReadOnly(array);
				}
				return m_namedArgs;
			}
		}

		public static IList<CustomAttributeData> GetCustomAttributes(MemberInfo target)
		{
			if (target == null)
			{
				throw new ArgumentNullException("target");
			}
			IList<CustomAttributeData> customAttributes = GetCustomAttributes(target.Module, target.MetadataToken);
			int count = 0;
			Attribute[] array = null;
			if (target is RuntimeType)
			{
				array = PseudoCustomAttribute.GetCustomAttributes((RuntimeType)target, typeof(object), includeSecCa: false, out count);
			}
			else if (target is RuntimeMethodInfo)
			{
				array = PseudoCustomAttribute.GetCustomAttributes((RuntimeMethodInfo)target, typeof(object), includeSecCa: false, out count);
			}
			else if (target is RuntimeFieldInfo)
			{
				array = PseudoCustomAttribute.GetCustomAttributes((RuntimeFieldInfo)target, typeof(object), out count);
			}
			if (count == 0)
			{
				return customAttributes;
			}
			CustomAttributeData[] array2 = new CustomAttributeData[customAttributes.Count + count];
			customAttributes.CopyTo(array2, count);
			for (int i = 0; i < count; i++)
			{
				if (!PseudoCustomAttribute.IsSecurityAttribute(array[i].GetType()))
				{
					array2[i] = new CustomAttributeData(array[i]);
				}
			}
			return Array.AsReadOnly(array2);
		}

		public static IList<CustomAttributeData> GetCustomAttributes(Module target)
		{
			if (target == null)
			{
				throw new ArgumentNullException("target");
			}
			if (target.IsResourceInternal())
			{
				return new List<CustomAttributeData>();
			}
			return GetCustomAttributes(target, target.MetadataToken);
		}

		public static IList<CustomAttributeData> GetCustomAttributes(Assembly target)
		{
			if (target == null)
			{
				throw new ArgumentNullException("target");
			}
			return GetCustomAttributes(target.ManifestModule, target.AssemblyHandle.GetToken());
		}

		public static IList<CustomAttributeData> GetCustomAttributes(ParameterInfo target)
		{
			if (target == null)
			{
				throw new ArgumentNullException("target");
			}
			IList<CustomAttributeData> customAttributes = GetCustomAttributes(target.Member.Module, target.MetadataToken);
			int count = 0;
			Attribute[] customAttributes2 = PseudoCustomAttribute.GetCustomAttributes(target, typeof(object), out count);
			if (count == 0)
			{
				return customAttributes;
			}
			CustomAttributeData[] array = new CustomAttributeData[customAttributes.Count + count];
			customAttributes.CopyTo(array, count);
			for (int i = 0; i < count; i++)
			{
				array[i] = new CustomAttributeData(customAttributes2[i]);
			}
			return Array.AsReadOnly(array);
		}

		private static CustomAttributeEncoding TypeToCustomAttributeEncoding(Type type)
		{
			if (type == typeof(int))
			{
				return CustomAttributeEncoding.Int32;
			}
			if (type.IsEnum)
			{
				return CustomAttributeEncoding.Enum;
			}
			if (type == typeof(string))
			{
				return CustomAttributeEncoding.String;
			}
			if (type == typeof(Type))
			{
				return CustomAttributeEncoding.Type;
			}
			if (type == typeof(object))
			{
				return CustomAttributeEncoding.Object;
			}
			if (type.IsArray)
			{
				return CustomAttributeEncoding.Array;
			}
			if (type == typeof(char))
			{
				return CustomAttributeEncoding.Char;
			}
			if (type == typeof(bool))
			{
				return CustomAttributeEncoding.Boolean;
			}
			if (type == typeof(byte))
			{
				return CustomAttributeEncoding.Byte;
			}
			if (type == typeof(sbyte))
			{
				return CustomAttributeEncoding.SByte;
			}
			if (type == typeof(short))
			{
				return CustomAttributeEncoding.Int16;
			}
			if (type == typeof(ushort))
			{
				return CustomAttributeEncoding.UInt16;
			}
			if (type == typeof(uint))
			{
				return CustomAttributeEncoding.UInt32;
			}
			if (type == typeof(long))
			{
				return CustomAttributeEncoding.Int64;
			}
			if (type == typeof(ulong))
			{
				return CustomAttributeEncoding.UInt64;
			}
			if (type == typeof(float))
			{
				return CustomAttributeEncoding.Float;
			}
			if (type == typeof(double))
			{
				return CustomAttributeEncoding.Double;
			}
			if (type.IsClass)
			{
				return CustomAttributeEncoding.Object;
			}
			if (type.IsInterface)
			{
				return CustomAttributeEncoding.Object;
			}
			if (type.IsValueType)
			{
				return CustomAttributeEncoding.Undefined;
			}
			throw new ArgumentException(Environment.GetResourceString("Argument_InvalidKindOfTypeForCA"), "type");
		}

		private static CustomAttributeType InitCustomAttributeType(Type parameterType, Module scope)
		{
			CustomAttributeEncoding customAttributeEncoding = TypeToCustomAttributeEncoding(parameterType);
			CustomAttributeEncoding customAttributeEncoding2 = CustomAttributeEncoding.Undefined;
			CustomAttributeEncoding encodedEnumType = CustomAttributeEncoding.Undefined;
			string enumName = null;
			if (customAttributeEncoding == CustomAttributeEncoding.Array)
			{
				parameterType = parameterType.GetElementType();
				customAttributeEncoding2 = TypeToCustomAttributeEncoding(parameterType);
			}
			if (customAttributeEncoding == CustomAttributeEncoding.Enum || customAttributeEncoding2 == CustomAttributeEncoding.Enum)
			{
				encodedEnumType = TypeToCustomAttributeEncoding(Enum.GetUnderlyingType(parameterType));
				enumName = ((parameterType.Module != scope) ? parameterType.AssemblyQualifiedName : parameterType.FullName);
			}
			return new CustomAttributeType(customAttributeEncoding, customAttributeEncoding2, encodedEnumType, enumName);
		}

		private static IList<CustomAttributeData> GetCustomAttributes(Module module, int tkTarget)
		{
			CustomAttributeRecord[] customAttributeRecords = GetCustomAttributeRecords(module, tkTarget);
			CustomAttributeData[] array = new CustomAttributeData[customAttributeRecords.Length];
			for (int i = 0; i < customAttributeRecords.Length; i++)
			{
				array[i] = new CustomAttributeData(module, customAttributeRecords[i]);
			}
			return Array.AsReadOnly(array);
		}

		internal unsafe static CustomAttributeRecord[] GetCustomAttributeRecords(Module module, int targetToken)
		{
			MetadataImport metadataImport = module.MetadataImport;
			int num = metadataImport.EnumCustomAttributesCount(targetToken);
			int* ptr = (int*)stackalloc byte[4 * num];
			metadataImport.EnumCustomAttributes(targetToken, ptr, num);
			CustomAttributeRecord[] array = new CustomAttributeRecord[num];
			for (int i = 0; i < num; i++)
			{
				metadataImport.GetCustomAttributeProps(ptr[i], out array[i].tkCtor.Value, out array[i].blob);
			}
			return array;
		}

		internal static CustomAttributeTypedArgument Filter(IList<CustomAttributeData> attrs, Type caType, string name)
		{
			for (int i = 0; i < attrs.Count; i++)
			{
				if (attrs[i].Constructor.DeclaringType != caType)
				{
					continue;
				}
				IList<CustomAttributeNamedArgument> namedArguments = attrs[i].NamedArguments;
				for (int j = 0; j < namedArguments.Count; j++)
				{
					if (namedArguments[j].MemberInfo.Name.Equals(name))
					{
						return namedArguments[j].TypedValue;
					}
				}
			}
			return default(CustomAttributeTypedArgument);
		}

		internal static CustomAttributeTypedArgument Filter(IList<CustomAttributeData> attrs, Type caType, int parameter)
		{
			for (int i = 0; i < attrs.Count; i++)
			{
				if (attrs[i].Constructor.DeclaringType == caType)
				{
					return attrs[i].ConstructorArguments[parameter];
				}
			}
			return default(CustomAttributeTypedArgument);
		}

		internal CustomAttributeData(Module scope, CustomAttributeRecord caRecord)
		{
			m_scope = scope;
			m_ctor = (ConstructorInfo)RuntimeType.GetMethodBase(scope, caRecord.tkCtor);
			ParameterInfo[] parametersNoCopy = m_ctor.GetParametersNoCopy();
			m_ctorParams = new CustomAttributeCtorParameter[parametersNoCopy.Length];
			for (int i = 0; i < parametersNoCopy.Length; i++)
			{
				ref CustomAttributeCtorParameter reference = ref m_ctorParams[i];
				reference = new CustomAttributeCtorParameter(InitCustomAttributeType(parametersNoCopy[i].ParameterType, scope));
			}
			FieldInfo[] fields = m_ctor.DeclaringType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			PropertyInfo[] properties = m_ctor.DeclaringType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			m_namedParams = new CustomAttributeNamedParameter[properties.Length + fields.Length];
			for (int j = 0; j < fields.Length; j++)
			{
				ref CustomAttributeNamedParameter reference2 = ref m_namedParams[j];
				reference2 = new CustomAttributeNamedParameter(fields[j].Name, CustomAttributeEncoding.Field, InitCustomAttributeType(fields[j].FieldType, scope));
			}
			for (int k = 0; k < properties.Length; k++)
			{
				ref CustomAttributeNamedParameter reference3 = ref m_namedParams[k + fields.Length];
				reference3 = new CustomAttributeNamedParameter(properties[k].Name, CustomAttributeEncoding.Property, InitCustomAttributeType(properties[k].PropertyType, scope));
			}
			m_members = new MemberInfo[fields.Length + properties.Length];
			fields.CopyTo(m_members, 0);
			properties.CopyTo(m_members, fields.Length);
			CustomAttributeEncodedArgument.ParseAttributeArguments(caRecord.blob, ref m_ctorParams, ref m_namedParams, m_scope);
		}

		internal CustomAttributeData(Attribute attribute)
		{
			if (attribute is DllImportAttribute)
			{
				Init((DllImportAttribute)attribute);
			}
			else if (attribute is FieldOffsetAttribute)
			{
				Init((FieldOffsetAttribute)attribute);
			}
			else if (attribute is MarshalAsAttribute)
			{
				Init((MarshalAsAttribute)attribute);
			}
			else
			{
				Init(attribute);
			}
		}

		private void Init(DllImportAttribute dllImport)
		{
			Type typeFromHandle = typeof(DllImportAttribute);
			m_ctor = typeFromHandle.GetConstructors(BindingFlags.Instance | BindingFlags.Public)[0];
			m_typedCtorArgs = Array.AsReadOnly(new CustomAttributeTypedArgument[1]
			{
				new CustomAttributeTypedArgument(dllImport.Value)
			});
			m_namedArgs = Array.AsReadOnly(new CustomAttributeNamedArgument[8]
			{
				new CustomAttributeNamedArgument(typeFromHandle.GetField("EntryPoint"), dllImport.EntryPoint),
				new CustomAttributeNamedArgument(typeFromHandle.GetField("CharSet"), dllImport.CharSet),
				new CustomAttributeNamedArgument(typeFromHandle.GetField("ExactSpelling"), dllImport.ExactSpelling),
				new CustomAttributeNamedArgument(typeFromHandle.GetField("SetLastError"), dllImport.SetLastError),
				new CustomAttributeNamedArgument(typeFromHandle.GetField("PreserveSig"), dllImport.PreserveSig),
				new CustomAttributeNamedArgument(typeFromHandle.GetField("CallingConvention"), dllImport.CallingConvention),
				new CustomAttributeNamedArgument(typeFromHandle.GetField("BestFitMapping"), dllImport.BestFitMapping),
				new CustomAttributeNamedArgument(typeFromHandle.GetField("ThrowOnUnmappableChar"), dllImport.ThrowOnUnmappableChar)
			});
		}

		private void Init(FieldOffsetAttribute fieldOffset)
		{
			m_ctor = typeof(FieldOffsetAttribute).GetConstructors(BindingFlags.Instance | BindingFlags.Public)[0];
			m_typedCtorArgs = Array.AsReadOnly(new CustomAttributeTypedArgument[1]
			{
				new CustomAttributeTypedArgument(fieldOffset.Value)
			});
			m_namedArgs = Array.AsReadOnly(new CustomAttributeNamedArgument[0]);
		}

		private void Init(MarshalAsAttribute marshalAs)
		{
			Type typeFromHandle = typeof(MarshalAsAttribute);
			m_ctor = typeFromHandle.GetConstructors(BindingFlags.Instance | BindingFlags.Public)[0];
			m_typedCtorArgs = Array.AsReadOnly(new CustomAttributeTypedArgument[1]
			{
				new CustomAttributeTypedArgument(marshalAs.Value)
			});
			int num = 3;
			if (marshalAs.MarshalType != null)
			{
				num++;
			}
			if (marshalAs.MarshalTypeRef != null)
			{
				num++;
			}
			if (marshalAs.MarshalCookie != null)
			{
				num++;
			}
			num++;
			num++;
			if (marshalAs.SafeArrayUserDefinedSubType != null)
			{
				num++;
			}
			CustomAttributeNamedArgument[] array = new CustomAttributeNamedArgument[num];
			num = 0;
			ref CustomAttributeNamedArgument reference = ref array[num++];
			reference = new CustomAttributeNamedArgument(typeFromHandle.GetField("ArraySubType"), marshalAs.ArraySubType);
			ref CustomAttributeNamedArgument reference2 = ref array[num++];
			reference2 = new CustomAttributeNamedArgument(typeFromHandle.GetField("SizeParamIndex"), marshalAs.SizeParamIndex);
			ref CustomAttributeNamedArgument reference3 = ref array[num++];
			reference3 = new CustomAttributeNamedArgument(typeFromHandle.GetField("SizeConst"), marshalAs.SizeConst);
			ref CustomAttributeNamedArgument reference4 = ref array[num++];
			reference4 = new CustomAttributeNamedArgument(typeFromHandle.GetField("IidParameterIndex"), marshalAs.IidParameterIndex);
			ref CustomAttributeNamedArgument reference5 = ref array[num++];
			reference5 = new CustomAttributeNamedArgument(typeFromHandle.GetField("SafeArraySubType"), marshalAs.SafeArraySubType);
			if (marshalAs.MarshalType != null)
			{
				ref CustomAttributeNamedArgument reference6 = ref array[num++];
				reference6 = new CustomAttributeNamedArgument(typeFromHandle.GetField("MarshalType"), marshalAs.MarshalType);
			}
			if (marshalAs.MarshalTypeRef != null)
			{
				ref CustomAttributeNamedArgument reference7 = ref array[num++];
				reference7 = new CustomAttributeNamedArgument(typeFromHandle.GetField("MarshalTypeRef"), marshalAs.MarshalTypeRef);
			}
			if (marshalAs.MarshalCookie != null)
			{
				ref CustomAttributeNamedArgument reference8 = ref array[num++];
				reference8 = new CustomAttributeNamedArgument(typeFromHandle.GetField("MarshalCookie"), marshalAs.MarshalCookie);
			}
			if (marshalAs.SafeArrayUserDefinedSubType != null)
			{
				ref CustomAttributeNamedArgument reference9 = ref array[num++];
				reference9 = new CustomAttributeNamedArgument(typeFromHandle.GetField("SafeArrayUserDefinedSubType"), marshalAs.SafeArrayUserDefinedSubType);
			}
			m_namedArgs = Array.AsReadOnly(array);
		}

		private void Init(object pca)
		{
			m_ctor = pca.GetType().GetConstructors(BindingFlags.Instance | BindingFlags.Public)[0];
			m_typedCtorArgs = Array.AsReadOnly(new CustomAttributeTypedArgument[0]);
			m_namedArgs = Array.AsReadOnly(new CustomAttributeNamedArgument[0]);
		}

		public override string ToString()
		{
			string text = "";
			for (int i = 0; i < ConstructorArguments.Count; i++)
			{
				text += string.Format(CultureInfo.CurrentCulture, (i == 0) ? "{0}" : ", {0}", ConstructorArguments[i]);
			}
			string text2 = "";
			for (int j = 0; j < NamedArguments.Count; j++)
			{
				text2 += string.Format(CultureInfo.CurrentCulture, (j == 0 && text.Length == 0) ? "{0}" : ", {0}", NamedArguments[j]);
			}
			return string.Format(CultureInfo.CurrentCulture, "[{0}({1}{2})]", Constructor.DeclaringType.FullName, text, text2);
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			return obj == this;
		}
	}
}
