using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System.Reflection
{
	internal static class CustomAttribute
	{
		internal static bool IsDefined(RuntimeType type, RuntimeType caType, bool inherit)
		{
			if (type.GetElementType() != null)
			{
				return false;
			}
			if (PseudoCustomAttribute.IsDefined(type, caType))
			{
				return true;
			}
			if (IsCustomAttributeDefined(type.Module, type.MetadataToken, caType))
			{
				return true;
			}
			if (!inherit)
			{
				return false;
			}
			for (type = type.BaseType as RuntimeType; type != null; type = type.BaseType as RuntimeType)
			{
				if (IsCustomAttributeDefined(type.Module, type.MetadataToken, caType, inherit))
				{
					return true;
				}
			}
			return false;
		}

		internal static bool IsDefined(RuntimeMethodInfo method, RuntimeType caType, bool inherit)
		{
			if (PseudoCustomAttribute.IsDefined(method, caType))
			{
				return true;
			}
			if (IsCustomAttributeDefined(method.Module, method.MetadataToken, caType))
			{
				return true;
			}
			if (!inherit)
			{
				return false;
			}
			for (method = method.GetParentDefinition() as RuntimeMethodInfo; method != null; method = method.GetParentDefinition() as RuntimeMethodInfo)
			{
				if (IsCustomAttributeDefined(method.Module, method.MetadataToken, caType, inherit))
				{
					return true;
				}
			}
			return false;
		}

		internal static bool IsDefined(RuntimeConstructorInfo ctor, RuntimeType caType)
		{
			if (PseudoCustomAttribute.IsDefined(ctor, caType))
			{
				return true;
			}
			return IsCustomAttributeDefined(ctor.Module, ctor.MetadataToken, caType);
		}

		internal static bool IsDefined(RuntimePropertyInfo property, RuntimeType caType)
		{
			if (PseudoCustomAttribute.IsDefined(property, caType))
			{
				return true;
			}
			return IsCustomAttributeDefined(property.Module, property.MetadataToken, caType);
		}

		internal static bool IsDefined(RuntimeEventInfo e, RuntimeType caType)
		{
			if (PseudoCustomAttribute.IsDefined(e, caType))
			{
				return true;
			}
			return IsCustomAttributeDefined(e.Module, e.MetadataToken, caType);
		}

		internal static bool IsDefined(RuntimeFieldInfo field, RuntimeType caType)
		{
			if (PseudoCustomAttribute.IsDefined(field, caType))
			{
				return true;
			}
			return IsCustomAttributeDefined(field.Module, field.MetadataToken, caType);
		}

		internal static bool IsDefined(ParameterInfo parameter, RuntimeType caType)
		{
			if (PseudoCustomAttribute.IsDefined(parameter, caType))
			{
				return true;
			}
			return IsCustomAttributeDefined(parameter.Member.Module, parameter.MetadataToken, caType);
		}

		internal static bool IsDefined(Assembly assembly, RuntimeType caType)
		{
			if (PseudoCustomAttribute.IsDefined(assembly, caType))
			{
				return true;
			}
			return IsCustomAttributeDefined(assembly.ManifestModule, assembly.AssemblyHandle.GetToken(), caType);
		}

		internal static bool IsDefined(Module module, RuntimeType caType)
		{
			if (PseudoCustomAttribute.IsDefined(module, caType))
			{
				return true;
			}
			return IsCustomAttributeDefined(module, module.MetadataToken, caType);
		}

		internal static object[] GetCustomAttributes(RuntimeType type, RuntimeType caType, bool inherit)
		{
			if (type.GetElementType() != null)
			{
				if (!caType.IsValueType)
				{
					return (object[])Array.CreateInstance(caType, 0);
				}
				return new object[0];
			}
			if (type.IsGenericType && !type.IsGenericTypeDefinition)
			{
				type = type.GetGenericTypeDefinition() as RuntimeType;
			}
			int count = 0;
			Attribute[] customAttributes = PseudoCustomAttribute.GetCustomAttributes(type, caType, includeSecCa: true, out count);
			if (!inherit || (caType.IsSealed && !GetAttributeUsage(caType).Inherited))
			{
				object[] customAttributes2 = GetCustomAttributes(type.Module, type.MetadataToken, count, caType);
				if (count > 0)
				{
					Array.Copy(customAttributes, 0, customAttributes2, customAttributes2.Length - count, count);
				}
				return customAttributes2;
			}
			List<object> list = new List<object>();
			bool mustBeInheritable = false;
			Type elementType = ((caType == null || caType.IsValueType || caType.ContainsGenericParameters) ? typeof(object) : caType);
			while (count > 0)
			{
				list.Add(customAttributes[--count]);
			}
			while (type != typeof(object) && type != null)
			{
				object[] customAttributes3 = GetCustomAttributes(type.Module, type.MetadataToken, 0, caType, mustBeInheritable, list);
				mustBeInheritable = true;
				for (int i = 0; i < customAttributes3.Length; i++)
				{
					list.Add(customAttributes3[i]);
				}
				type = type.BaseType as RuntimeType;
			}
			object[] array = Array.CreateInstance(elementType, list.Count) as object[];
			Array.Copy(list.ToArray(), 0, array, 0, list.Count);
			return array;
		}

		internal static object[] GetCustomAttributes(RuntimeMethodInfo method, RuntimeType caType, bool inherit)
		{
			if (method.IsGenericMethod && !method.IsGenericMethodDefinition)
			{
				method = method.GetGenericMethodDefinition() as RuntimeMethodInfo;
			}
			int count = 0;
			Attribute[] customAttributes = PseudoCustomAttribute.GetCustomAttributes(method, caType, includeSecCa: true, out count);
			if (!inherit || (caType.IsSealed && !GetAttributeUsage(caType).Inherited))
			{
				object[] customAttributes2 = GetCustomAttributes(method.Module, method.MetadataToken, count, caType);
				if (count > 0)
				{
					Array.Copy(customAttributes, 0, customAttributes2, customAttributes2.Length - count, count);
				}
				return customAttributes2;
			}
			List<object> list = new List<object>();
			bool mustBeInheritable = false;
			Type elementType = ((caType == null || caType.IsValueType || caType.ContainsGenericParameters) ? typeof(object) : caType);
			while (count > 0)
			{
				list.Add(customAttributes[--count]);
			}
			while (method != null)
			{
				object[] customAttributes3 = GetCustomAttributes(method.Module, method.MetadataToken, 0, caType, mustBeInheritable, list);
				mustBeInheritable = true;
				for (int i = 0; i < customAttributes3.Length; i++)
				{
					list.Add(customAttributes3[i]);
				}
				method = method.GetParentDefinition() as RuntimeMethodInfo;
			}
			object[] array = Array.CreateInstance(elementType, list.Count) as object[];
			Array.Copy(list.ToArray(), 0, array, 0, list.Count);
			return array;
		}

		internal static object[] GetCustomAttributes(RuntimeConstructorInfo ctor, RuntimeType caType)
		{
			int count = 0;
			Attribute[] customAttributes = PseudoCustomAttribute.GetCustomAttributes(ctor, caType, includeSecCa: true, out count);
			object[] customAttributes2 = GetCustomAttributes(ctor.Module, ctor.MetadataToken, count, caType);
			if (count > 0)
			{
				Array.Copy(customAttributes, 0, customAttributes2, customAttributes2.Length - count, count);
			}
			return customAttributes2;
		}

		internal static object[] GetCustomAttributes(RuntimePropertyInfo property, RuntimeType caType)
		{
			int count = 0;
			Attribute[] customAttributes = PseudoCustomAttribute.GetCustomAttributes(property, caType, out count);
			object[] customAttributes2 = GetCustomAttributes(property.Module, property.MetadataToken, count, caType);
			if (count > 0)
			{
				Array.Copy(customAttributes, 0, customAttributes2, customAttributes2.Length - count, count);
			}
			return customAttributes2;
		}

		internal static object[] GetCustomAttributes(RuntimeEventInfo e, RuntimeType caType)
		{
			int count = 0;
			Attribute[] customAttributes = PseudoCustomAttribute.GetCustomAttributes(e, caType, out count);
			object[] customAttributes2 = GetCustomAttributes(e.Module, e.MetadataToken, count, caType);
			if (count > 0)
			{
				Array.Copy(customAttributes, 0, customAttributes2, customAttributes2.Length - count, count);
			}
			return customAttributes2;
		}

		internal static object[] GetCustomAttributes(RuntimeFieldInfo field, RuntimeType caType)
		{
			int count = 0;
			Attribute[] customAttributes = PseudoCustomAttribute.GetCustomAttributes(field, caType, out count);
			object[] customAttributes2 = GetCustomAttributes(field.Module, field.MetadataToken, count, caType);
			if (count > 0)
			{
				Array.Copy(customAttributes, 0, customAttributes2, customAttributes2.Length - count, count);
			}
			return customAttributes2;
		}

		internal static object[] GetCustomAttributes(ParameterInfo parameter, RuntimeType caType)
		{
			int count = 0;
			Attribute[] customAttributes = PseudoCustomAttribute.GetCustomAttributes(parameter, caType, out count);
			object[] customAttributes2 = GetCustomAttributes(parameter.Member.Module, parameter.MetadataToken, count, caType);
			if (count > 0)
			{
				Array.Copy(customAttributes, 0, customAttributes2, customAttributes2.Length - count, count);
			}
			return customAttributes2;
		}

		internal static object[] GetCustomAttributes(Assembly assembly, RuntimeType caType)
		{
			int count = 0;
			Attribute[] customAttributes = PseudoCustomAttribute.GetCustomAttributes(assembly, caType, out count);
			object[] customAttributes2 = GetCustomAttributes(assembly.ManifestModule, assembly.AssemblyHandle.GetToken(), count, caType);
			if (count > 0)
			{
				Array.Copy(customAttributes, 0, customAttributes2, customAttributes2.Length - count, count);
			}
			return customAttributes2;
		}

		internal static object[] GetCustomAttributes(Module module, RuntimeType caType)
		{
			int count = 0;
			Attribute[] customAttributes = PseudoCustomAttribute.GetCustomAttributes(module, caType, out count);
			object[] customAttributes2 = GetCustomAttributes(module, module.MetadataToken, count, caType);
			if (count > 0)
			{
				Array.Copy(customAttributes, 0, customAttributes2, customAttributes2.Length - count, count);
			}
			return customAttributes2;
		}

		internal static bool IsCustomAttributeDefined(Module decoratedModule, int decoratedMetadataToken, RuntimeType attributeFilterType)
		{
			return IsCustomAttributeDefined(decoratedModule, decoratedMetadataToken, attributeFilterType, mustBeInheritable: false);
		}

		internal static bool IsCustomAttributeDefined(Module decoratedModule, int decoratedMetadataToken, RuntimeType attributeFilterType, bool mustBeInheritable)
		{
			if (decoratedModule.Assembly.ReflectionOnly)
			{
				throw new InvalidOperationException(Environment.GetResourceString("Arg_ReflectionOnlyCA"));
			}
			MetadataImport metadataImport = decoratedModule.MetadataImport;
			CustomAttributeRecord[] customAttributeRecords = CustomAttributeData.GetCustomAttributeRecords(decoratedModule, decoratedMetadataToken);
			Assembly lastAptcaOkAssembly = null;
			foreach (CustomAttributeRecord caRecord in customAttributeRecords)
			{
				if (FilterCustomAttributeRecord(caRecord, metadataImport, ref lastAptcaOkAssembly, decoratedModule, decoratedMetadataToken, attributeFilterType, mustBeInheritable, null, null, out var _, out var _, out var _, out var _))
				{
					return true;
				}
			}
			return false;
		}

		internal static object[] GetCustomAttributes(Module decoratedModule, int decoratedMetadataToken, int pcaCount, RuntimeType attributeFilterType)
		{
			return GetCustomAttributes(decoratedModule, decoratedMetadataToken, pcaCount, attributeFilterType, mustBeInheritable: false, null);
		}

		internal unsafe static object[] GetCustomAttributes(Module decoratedModule, int decoratedMetadataToken, int pcaCount, RuntimeType attributeFilterType, bool mustBeInheritable, IList derivedAttributes)
		{
			if (decoratedModule.Assembly.ReflectionOnly)
			{
				throw new InvalidOperationException(Environment.GetResourceString("Arg_ReflectionOnlyCA"));
			}
			MetadataImport metadataImport = decoratedModule.MetadataImport;
			CustomAttributeRecord[] customAttributeRecords = CustomAttributeData.GetCustomAttributeRecords(decoratedModule, decoratedMetadataToken);
			Type elementType = ((attributeFilterType == null || attributeFilterType.IsValueType || attributeFilterType.ContainsGenericParameters) ? typeof(object) : attributeFilterType);
			if (attributeFilterType == null && customAttributeRecords.Length == 0)
			{
				return Array.CreateInstance(elementType, 0) as object[];
			}
			object[] array = Array.CreateInstance(elementType, customAttributeRecords.Length) as object[];
			int num = 0;
			SecurityContextFrame securityContextFrame = default(SecurityContextFrame);
			securityContextFrame.Push(decoratedModule.Assembly.InternalAssembly);
			Assembly lastAptcaOkAssembly = null;
			for (int i = 0; i < customAttributeRecords.Length; i++)
			{
				object obj = null;
				CustomAttributeRecord caRecord = customAttributeRecords[i];
				RuntimeMethodHandle ctor = default(RuntimeMethodHandle);
				RuntimeType attributeType = null;
				int namedArgs = 0;
				IntPtr blob = caRecord.blob.Signature;
				IntPtr intPtr = (IntPtr)((byte*)(void*)blob + caRecord.blob.Length);
				if (!FilterCustomAttributeRecord(caRecord, metadataImport, ref lastAptcaOkAssembly, decoratedModule, decoratedMetadataToken, attributeFilterType, mustBeInheritable, array, derivedAttributes, out attributeType, out ctor, out var ctorHasParameters, out var isVarArg))
				{
					continue;
				}
				if (!ctor.IsNullHandle())
				{
					ctor.CheckLinktimeDemands(decoratedModule, decoratedMetadataToken);
				}
				RuntimeConstructorInfo.CheckCanCreateInstance(attributeType, isVarArg);
				if (ctorHasParameters)
				{
					obj = CreateCaObject(decoratedModule, ctor, ref blob, intPtr, out namedArgs);
				}
				else
				{
					obj = attributeType.TypeHandle.CreateCaInstance(ctor);
					if (Marshal.ReadInt16(blob) != 1)
					{
						throw new CustomAttributeFormatException();
					}
					blob = (IntPtr)((byte*)(void*)blob + 2);
					namedArgs = Marshal.ReadInt16(blob);
					blob = (IntPtr)((byte*)(void*)blob + 2);
				}
				for (int j = 0; j < namedArgs; j++)
				{
					_ = caRecord.blob.Signature;
					GetPropertyOrFieldData(decoratedModule, ref blob, intPtr, out var name, out var isProperty, out var type, out var value);
					try
					{
						if (isProperty)
						{
							if (type == null && value != null)
							{
								type = ((value.GetType() == typeof(RuntimeType)) ? typeof(Type) : value.GetType());
							}
							RuntimePropertyInfo runtimePropertyInfo = null;
							runtimePropertyInfo = ((type != null) ? (attributeType.GetProperty(name, type, Type.EmptyTypes) as RuntimePropertyInfo) : (attributeType.GetProperty(name) as RuntimePropertyInfo));
							RuntimeMethodInfo runtimeMethodInfo = runtimePropertyInfo.GetSetMethod(nonPublic: true) as RuntimeMethodInfo;
							if (runtimeMethodInfo.IsPublic)
							{
								runtimeMethodInfo.MethodHandle.CheckLinktimeDemands(decoratedModule, decoratedMetadataToken);
								runtimeMethodInfo.Invoke(obj, BindingFlags.Default, null, new object[1]
								{
									value
								}, null, skipVisibilityChecks: true);
							}
						}
						else
						{
							RtFieldInfo rtFieldInfo = attributeType.GetField(name) as RtFieldInfo;
							rtFieldInfo.InternalSetValue(obj, value, BindingFlags.Default, Type.DefaultBinder, null, doVisibilityCheck: false);
						}
					}
					catch (Exception inner)
					{
						throw new CustomAttributeFormatException(string.Format(CultureInfo.CurrentUICulture, Environment.GetResourceString(isProperty ? "RFLCT.InvalidPropFail" : "RFLCT.InvalidFieldFail"), name), inner);
					}
				}
				if (!blob.Equals(intPtr))
				{
					throw new CustomAttributeFormatException();
				}
				array[num++] = obj;
			}
			securityContextFrame.Pop();
			if (num == customAttributeRecords.Length && pcaCount == 0)
			{
				return array;
			}
			if (num == 0)
			{
				Array.CreateInstance(elementType, 0);
			}
			object[] array2 = Array.CreateInstance(elementType, num + pcaCount) as object[];
			Array.Copy(array, 0, array2, 0, num);
			return array2;
		}

		internal unsafe static bool FilterCustomAttributeRecord(CustomAttributeRecord caRecord, MetadataImport scope, ref Assembly lastAptcaOkAssembly, Module decoratedModule, MetadataToken decoratedToken, RuntimeType attributeFilterType, bool mustBeInheritable, object[] attributes, IList derivedAttributes, out RuntimeType attributeType, out RuntimeMethodHandle ctor, out bool ctorHasParameters, out bool isVarArg)
		{
			ctor = default(RuntimeMethodHandle);
			attributeType = null;
			ctorHasParameters = false;
			isVarArg = false;
			IntPtr signature = caRecord.blob.Signature;
			_ = (IntPtr)((byte*)(void*)signature + caRecord.blob.Length);
			attributeType = decoratedModule.ResolveType(scope.GetParentToken(caRecord.tkCtor), null, null) as RuntimeType;
			if (!attributeFilterType.IsAssignableFrom(attributeType))
			{
				return false;
			}
			if (!AttributeUsageCheck(attributeType, mustBeInheritable, attributes, derivedAttributes))
			{
				return false;
			}
			if (attributeType.Assembly != lastAptcaOkAssembly && !attributeType.Assembly.AptcaCheck(decoratedModule.Assembly))
			{
				return false;
			}
			lastAptcaOkAssembly = decoratedModule.Assembly;
			ConstArray methodSignature = scope.GetMethodSignature(caRecord.tkCtor);
			isVarArg = (methodSignature[0] & 5) != 0;
			ctorHasParameters = methodSignature[1] != 0;
			if (ctorHasParameters)
			{
				ctor = decoratedModule.ModuleHandle.ResolveMethodHandle(caRecord.tkCtor);
			}
			else
			{
				ctor = attributeType.GetTypeHandleInternal().GetDefaultConstructor();
				if (ctor.IsNullHandle() && !attributeType.IsValueType)
				{
					throw new MissingMethodException(".ctor");
				}
			}
			if (ctor.IsNullHandle())
			{
				if (!attributeType.IsVisible && !attributeType.TypeHandle.IsVisibleFromModule(decoratedModule.ModuleHandle))
				{
					return false;
				}
				return true;
			}
			if (ctor.IsVisibleFromModule(decoratedModule))
			{
				return true;
			}
			MetadataToken token = default(MetadataToken);
			if (decoratedToken.IsParamDef)
			{
				token = new MetadataToken(scope.GetParentToken(decoratedToken));
				token = new MetadataToken(scope.GetParentToken(token));
			}
			else if (decoratedToken.IsMethodDef || decoratedToken.IsProperty || decoratedToken.IsEvent || decoratedToken.IsFieldDef)
			{
				token = new MetadataToken(scope.GetParentToken(decoratedToken));
			}
			else if (decoratedToken.IsTypeDef)
			{
				token = decoratedToken;
			}
			if (token.IsTypeDef)
			{
				return ctor.IsVisibleFromType(decoratedModule.ModuleHandle.ResolveTypeHandle(token));
			}
			return false;
		}

		private static bool AttributeUsageCheck(RuntimeType attributeType, bool mustBeInheritable, object[] attributes, IList derivedAttributes)
		{
			AttributeUsageAttribute attributeUsageAttribute = null;
			if (mustBeInheritable)
			{
				attributeUsageAttribute = GetAttributeUsage(attributeType);
				if (!attributeUsageAttribute.Inherited)
				{
					return false;
				}
			}
			if (derivedAttributes == null)
			{
				return true;
			}
			for (int i = 0; i < derivedAttributes.Count; i++)
			{
				if (derivedAttributes[i].GetType() == attributeType)
				{
					if (attributeUsageAttribute == null)
					{
						attributeUsageAttribute = GetAttributeUsage(attributeType);
					}
					return attributeUsageAttribute.AllowMultiple;
				}
			}
			return true;
		}

		internal static AttributeUsageAttribute GetAttributeUsage(RuntimeType decoratedAttribute)
		{
			Module module = decoratedAttribute.Module;
			MetadataImport metadataImport = module.MetadataImport;
			CustomAttributeRecord[] customAttributeRecords = CustomAttributeData.GetCustomAttributeRecords(module, decoratedAttribute.MetadataToken);
			AttributeUsageAttribute attributeUsageAttribute = null;
			for (int i = 0; i < customAttributeRecords.Length; i++)
			{
				CustomAttributeRecord customAttributeRecord = customAttributeRecords[i];
				RuntimeType runtimeType = module.ResolveType(metadataImport.GetParentToken(customAttributeRecord.tkCtor), null, null) as RuntimeType;
				if (runtimeType == typeof(AttributeUsageAttribute))
				{
					if (attributeUsageAttribute != null)
					{
						throw new FormatException(string.Format(CultureInfo.CurrentUICulture, Environment.GetResourceString("Format_AttributeUsage"), runtimeType));
					}
					ParseAttributeUsageAttribute(customAttributeRecord.blob, out var targets, out var inherited, out var allowMultiple);
					attributeUsageAttribute = new AttributeUsageAttribute(targets, allowMultiple, inherited);
				}
			}
			if (attributeUsageAttribute == null)
			{
				return AttributeUsageAttribute.Default;
			}
			return attributeUsageAttribute;
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void _ParseAttributeUsageAttribute(IntPtr pCa, int cCa, out int targets, out bool inherited, out bool allowMultiple);

		private static void ParseAttributeUsageAttribute(ConstArray ca, out AttributeTargets targets, out bool inherited, out bool allowMultiple)
		{
			_ParseAttributeUsageAttribute(ca.Signature, ca.Length, out var targets2, out inherited, out allowMultiple);
			targets = (AttributeTargets)targets2;
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private unsafe static extern object _CreateCaObject(void* pModule, void* pCtor, byte** ppBlob, byte* pEndBlob, int* pcNamedArgs);

		private unsafe static object CreateCaObject(Module module, RuntimeMethodHandle ctor, ref IntPtr blob, IntPtr blobEnd, out int namedArgs)
		{
			byte* value = (byte*)(void*)blob;
			byte* pEndBlob = (byte*)(void*)blobEnd;
			int num = default(int);
			object result = _CreateCaObject(module.ModuleHandle.Value, (void*)ctor.Value, &value, pEndBlob, &num);
			blob = (IntPtr)value;
			namedArgs = num;
			return result;
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private unsafe static extern void _GetPropertyOrFieldData(IntPtr pModule, byte** ppBlobStart, byte* pBlobEnd, out string name, out bool bIsProperty, out Type type, out object value);

		private unsafe static void GetPropertyOrFieldData(Module module, ref IntPtr blobStart, IntPtr blobEnd, out string name, out bool isProperty, out Type type, out object value)
		{
			byte* value2 = (byte*)(void*)blobStart;
			_GetPropertyOrFieldData((IntPtr)module.ModuleHandle.Value, &value2, (byte*)(void*)blobEnd, out name, out isProperty, out type, out value);
			blobStart = (IntPtr)value2;
		}
	}
}
