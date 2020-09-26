using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace System.Reflection
{
	internal static class PseudoCustomAttribute
	{
		private static Hashtable s_pca;

		private static int s_pcasCount;

		[MethodImpl(MethodImplOptions.InternalCall)]
		private unsafe static extern void _GetSecurityAttributes(void* module, int token, out object[] securityAttributes);

		internal unsafe static void GetSecurityAttributes(ModuleHandle module, int token, out object[] securityAttributes)
		{
			_GetSecurityAttributes(module.Value, token, out securityAttributes);
		}

		static PseudoCustomAttribute()
		{
			Type[] array = new Type[10]
			{
				typeof(FieldOffsetAttribute),
				typeof(SerializableAttribute),
				typeof(MarshalAsAttribute),
				typeof(ComImportAttribute),
				typeof(NonSerializedAttribute),
				typeof(InAttribute),
				typeof(OutAttribute),
				typeof(OptionalAttribute),
				typeof(DllImportAttribute),
				typeof(PreserveSigAttribute)
			};
			s_pcasCount = array.Length;
			s_pca = new Hashtable(s_pcasCount);
			for (int i = 0; i < s_pcasCount; i++)
			{
				s_pca[array[i]] = array[i];
			}
		}

		[Conditional("_DEBUG")]
		private static void VerifyPseudoCustomAttribute(Type pca)
		{
			CustomAttribute.GetAttributeUsage(pca as RuntimeType);
		}

		internal static bool IsSecurityAttribute(Type type)
		{
			if (type != typeof(SecurityAttribute))
			{
				return type.IsSubclassOf(typeof(SecurityAttribute));
			}
			return true;
		}

		internal static Attribute[] GetCustomAttributes(RuntimeType type, Type caType, bool includeSecCa, out int count)
		{
			count = 0;
			bool flag = caType == typeof(object) || caType == typeof(Attribute);
			if (!flag && s_pca[caType] == null && !IsSecurityAttribute(caType))
			{
				return new Attribute[0];
			}
			List<Attribute> list = new List<Attribute>();
			Attribute attribute = null;
			if (flag || caType == typeof(SerializableAttribute))
			{
				attribute = SerializableAttribute.GetCustomAttribute(type);
				if (attribute != null)
				{
					list.Add(attribute);
				}
			}
			if (flag || caType == typeof(ComImportAttribute))
			{
				attribute = ComImportAttribute.GetCustomAttribute(type);
				if (attribute != null)
				{
					list.Add(attribute);
				}
			}
			if (includeSecCa && (flag || IsSecurityAttribute(caType)) && !type.IsGenericParameter)
			{
				if (type.IsGenericType)
				{
					type = (RuntimeType)type.GetGenericTypeDefinition();
				}
				GetSecurityAttributes(type.Module.ModuleHandle, type.MetadataToken, out var securityAttributes);
				if (securityAttributes != null)
				{
					object[] array = securityAttributes;
					foreach (object obj in array)
					{
						if (caType == obj.GetType() || obj.GetType().IsSubclassOf(caType))
						{
							list.Add((Attribute)obj);
						}
					}
				}
			}
			count = list.Count;
			return list.ToArray();
		}

		internal static bool IsDefined(RuntimeType type, Type caType)
		{
			bool flag = caType == typeof(object) || caType == typeof(Attribute);
			if (!flag && s_pca[caType] == null && !IsSecurityAttribute(caType))
			{
				return false;
			}
			if ((flag || caType == typeof(SerializableAttribute)) && SerializableAttribute.IsDefined(type))
			{
				return true;
			}
			if ((flag || caType == typeof(ComImportAttribute)) && ComImportAttribute.IsDefined(type))
			{
				return true;
			}
			if ((flag || IsSecurityAttribute(caType)) && GetCustomAttributes(type, caType, includeSecCa: true, out var _).Length != 0)
			{
				return true;
			}
			return false;
		}

		internal static Attribute[] GetCustomAttributes(RuntimeMethodInfo method, Type caType, bool includeSecCa, out int count)
		{
			count = 0;
			bool flag = caType == typeof(object) || caType == typeof(Attribute);
			if (!flag && s_pca[caType] == null && !IsSecurityAttribute(caType))
			{
				return new Attribute[0];
			}
			List<Attribute> list = new List<Attribute>();
			Attribute attribute = null;
			if (flag || caType == typeof(DllImportAttribute))
			{
				attribute = DllImportAttribute.GetCustomAttribute(method);
				if (attribute != null)
				{
					list.Add(attribute);
				}
			}
			if (flag || caType == typeof(PreserveSigAttribute))
			{
				attribute = PreserveSigAttribute.GetCustomAttribute(method);
				if (attribute != null)
				{
					list.Add(attribute);
				}
			}
			if (includeSecCa && (flag || IsSecurityAttribute(caType)))
			{
				GetSecurityAttributes(method.Module.ModuleHandle, method.MetadataToken, out var securityAttributes);
				if (securityAttributes != null)
				{
					object[] array = securityAttributes;
					foreach (object obj in array)
					{
						if (caType == obj.GetType() || obj.GetType().IsSubclassOf(caType))
						{
							list.Add((Attribute)obj);
						}
					}
				}
			}
			count = list.Count;
			return list.ToArray();
		}

		internal static bool IsDefined(RuntimeMethodInfo method, Type caType)
		{
			bool flag = caType == typeof(object) || caType == typeof(Attribute);
			if (!flag && s_pca[caType] == null)
			{
				return false;
			}
			if ((flag || caType == typeof(DllImportAttribute)) && DllImportAttribute.IsDefined(method))
			{
				return true;
			}
			if ((flag || caType == typeof(PreserveSigAttribute)) && PreserveSigAttribute.IsDefined(method))
			{
				return true;
			}
			if ((flag || IsSecurityAttribute(caType)) && GetCustomAttributes(method, caType, includeSecCa: true, out var _).Length != 0)
			{
				return true;
			}
			return false;
		}

		internal static Attribute[] GetCustomAttributes(ParameterInfo parameter, Type caType, out int count)
		{
			count = 0;
			bool flag = caType == typeof(object) || caType == typeof(Attribute);
			if (!flag && s_pca[caType] == null)
			{
				return null;
			}
			Attribute[] array = new Attribute[s_pcasCount];
			Attribute attribute = null;
			if (flag || caType == typeof(InAttribute))
			{
				attribute = InAttribute.GetCustomAttribute(parameter);
				if (attribute != null)
				{
					array[count++] = attribute;
				}
			}
			if (flag || caType == typeof(OutAttribute))
			{
				attribute = OutAttribute.GetCustomAttribute(parameter);
				if (attribute != null)
				{
					array[count++] = attribute;
				}
			}
			if (flag || caType == typeof(OptionalAttribute))
			{
				attribute = OptionalAttribute.GetCustomAttribute(parameter);
				if (attribute != null)
				{
					array[count++] = attribute;
				}
			}
			if (flag || caType == typeof(MarshalAsAttribute))
			{
				attribute = MarshalAsAttribute.GetCustomAttribute(parameter);
				if (attribute != null)
				{
					array[count++] = attribute;
				}
			}
			return array;
		}

		internal static bool IsDefined(ParameterInfo parameter, Type caType)
		{
			bool flag = caType == typeof(object) || caType == typeof(Attribute);
			if (!flag && s_pca[caType] == null)
			{
				return false;
			}
			if ((flag || caType == typeof(InAttribute)) && InAttribute.IsDefined(parameter))
			{
				return true;
			}
			if ((flag || caType == typeof(OutAttribute)) && OutAttribute.IsDefined(parameter))
			{
				return true;
			}
			if ((flag || caType == typeof(OptionalAttribute)) && OptionalAttribute.IsDefined(parameter))
			{
				return true;
			}
			if ((flag || caType == typeof(MarshalAsAttribute)) && MarshalAsAttribute.IsDefined(parameter))
			{
				return true;
			}
			return false;
		}

		internal static Attribute[] GetCustomAttributes(Assembly assembly, Type caType, out int count)
		{
			count = 0;
			bool flag = caType == typeof(object) || caType == typeof(Attribute);
			if (!flag && s_pca[caType] == null && !IsSecurityAttribute(caType))
			{
				return new Attribute[0];
			}
			List<Attribute> list = new List<Attribute>();
			if (flag || IsSecurityAttribute(caType))
			{
				GetSecurityAttributes(assembly.ManifestModule.ModuleHandle, assembly.AssemblyHandle.GetToken(), out var securityAttributes);
				if (securityAttributes != null)
				{
					object[] array = securityAttributes;
					foreach (object obj in array)
					{
						if (caType == obj.GetType() || obj.GetType().IsSubclassOf(caType))
						{
							list.Add((Attribute)obj);
						}
					}
				}
			}
			count = list.Count;
			return list.ToArray();
		}

		internal static bool IsDefined(Assembly assembly, Type caType)
		{
			int count;
			return GetCustomAttributes(assembly, caType, out count).Length > 0;
		}

		internal static Attribute[] GetCustomAttributes(Module module, Type caType, out int count)
		{
			count = 0;
			return null;
		}

		internal static bool IsDefined(Module module, Type caType)
		{
			return false;
		}

		internal static Attribute[] GetCustomAttributes(RuntimeFieldInfo field, Type caType, out int count)
		{
			count = 0;
			bool flag = caType == typeof(object) || caType == typeof(Attribute);
			if (!flag && s_pca[caType] == null)
			{
				return null;
			}
			Attribute[] array = new Attribute[s_pcasCount];
			Attribute attribute = null;
			if (flag || caType == typeof(MarshalAsAttribute))
			{
				attribute = MarshalAsAttribute.GetCustomAttribute(field);
				if (attribute != null)
				{
					array[count++] = attribute;
				}
			}
			if (flag || caType == typeof(FieldOffsetAttribute))
			{
				attribute = FieldOffsetAttribute.GetCustomAttribute(field);
				if (attribute != null)
				{
					array[count++] = attribute;
				}
			}
			if (flag || caType == typeof(NonSerializedAttribute))
			{
				attribute = NonSerializedAttribute.GetCustomAttribute(field);
				if (attribute != null)
				{
					array[count++] = attribute;
				}
			}
			return array;
		}

		internal static bool IsDefined(RuntimeFieldInfo field, Type caType)
		{
			bool flag = caType == typeof(object) || caType == typeof(Attribute);
			if (!flag && s_pca[caType] == null)
			{
				return false;
			}
			if ((flag || caType == typeof(MarshalAsAttribute)) && MarshalAsAttribute.IsDefined(field))
			{
				return true;
			}
			if ((flag || caType == typeof(FieldOffsetAttribute)) && FieldOffsetAttribute.IsDefined(field))
			{
				return true;
			}
			if ((flag || caType == typeof(NonSerializedAttribute)) && NonSerializedAttribute.IsDefined(field))
			{
				return true;
			}
			return false;
		}

		internal static Attribute[] GetCustomAttributes(RuntimeConstructorInfo ctor, Type caType, bool includeSecCa, out int count)
		{
			count = 0;
			bool flag = caType == typeof(object) || caType == typeof(Attribute);
			if (!flag && s_pca[caType] == null && !IsSecurityAttribute(caType))
			{
				return new Attribute[0];
			}
			List<Attribute> list = new List<Attribute>();
			if (includeSecCa && (flag || IsSecurityAttribute(caType)))
			{
				GetSecurityAttributes(ctor.Module.ModuleHandle, ctor.MetadataToken, out var securityAttributes);
				if (securityAttributes != null)
				{
					object[] array = securityAttributes;
					foreach (object obj in array)
					{
						if (caType == obj.GetType() || obj.GetType().IsSubclassOf(caType))
						{
							list.Add((Attribute)obj);
						}
					}
				}
			}
			count = list.Count;
			return list.ToArray();
		}

		internal static bool IsDefined(RuntimeConstructorInfo ctor, Type caType)
		{
			bool flag = caType == typeof(object) || caType == typeof(Attribute);
			if (!flag && s_pca[caType] == null)
			{
				return false;
			}
			if ((flag || IsSecurityAttribute(caType)) && GetCustomAttributes(ctor, caType, includeSecCa: true, out var _).Length != 0)
			{
				return true;
			}
			return false;
		}

		internal static Attribute[] GetCustomAttributes(RuntimePropertyInfo property, Type caType, out int count)
		{
			count = 0;
			return null;
		}

		internal static bool IsDefined(RuntimePropertyInfo property, Type caType)
		{
			return false;
		}

		internal static Attribute[] GetCustomAttributes(RuntimeEventInfo e, Type caType, out int count)
		{
			count = 0;
			return null;
		}

		internal static bool IsDefined(RuntimeEventInfo e, Type caType)
		{
			return false;
		}
	}
}
