using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Activation;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Permissions;
using System.Text;
using System.Threading;

namespace System
{
	[Serializable]
	internal class RuntimeType : Type, ISerializable, ICloneable
	{
		[Serializable]
		internal class RuntimeTypeCache
		{
			internal enum WhatsCached
			{
				Nothing,
				EnclosingType
			}

			internal enum CacheType
			{
				Method,
				Constructor,
				Field,
				Property,
				Event,
				Interface,
				NestedType
			}

			private struct Filter
			{
				private Utf8String m_name;

				private MemberListType m_listType;

				public unsafe Filter(byte* pUtf8Name, int cUtf8Name, MemberListType listType)
				{
					m_name = new Utf8String(pUtf8Name, cUtf8Name);
					m_listType = listType;
				}

				public bool Match(Utf8String name)
				{
					if (m_listType == MemberListType.CaseSensitive)
					{
						return m_name.Equals(name);
					}
					if (m_listType == MemberListType.CaseInsensitive)
					{
						return m_name.EqualsCaseInsensitive(name);
					}
					return true;
				}
			}

			[Serializable]
			private class MemberInfoCache<T> where T : MemberInfo
			{
				private CerHashtable<string, CerArrayList<T>> m_csMemberInfos;

				private CerHashtable<string, CerArrayList<T>> m_cisMemberInfos;

				private CerArrayList<T> m_root;

				private bool m_cacheComplete;

				private RuntimeTypeCache m_runtimeTypeCache;

				internal RuntimeTypeHandle ReflectedTypeHandle => m_runtimeTypeCache.RuntimeTypeHandle;

				internal RuntimeType ReflectedType => ReflectedTypeHandle.GetRuntimeType();

				static MemberInfoCache()
				{
					PrepareMemberInfoCache(typeof(MemberInfoCache<T>).TypeHandle);
				}

				internal MemberInfoCache(RuntimeTypeCache runtimeTypeCache)
				{
					Mda.MemberInfoCacheCreation();
					m_runtimeTypeCache = runtimeTypeCache;
					m_cacheComplete = false;
				}

				internal MethodBase AddMethod(RuntimeTypeHandle declaringType, RuntimeMethodHandle method, CacheType cacheType)
				{
					object obj = null;
					MethodAttributes attributes = method.GetAttributes();
					bool isPublic = (attributes & MethodAttributes.MemberAccessMask) == MethodAttributes.Public;
					bool isStatic = (attributes & MethodAttributes.Static) != 0;
					bool isInherited = declaringType.Value != ReflectedTypeHandle.Value;
					BindingFlags bindingFlags = FilterPreCalculate(isPublic, isInherited, isStatic);
					switch (cacheType)
					{
					case CacheType.Method:
					{
						List<RuntimeMethodInfo> list2 = new List<RuntimeMethodInfo>(1);
						list2.Add(new RuntimeMethodInfo(method, declaringType, m_runtimeTypeCache, attributes, bindingFlags));
						obj = list2;
						break;
					}
					case CacheType.Constructor:
					{
						List<RuntimeConstructorInfo> list = new List<RuntimeConstructorInfo>(1);
						list.Add(new RuntimeConstructorInfo(method, declaringType, m_runtimeTypeCache, attributes, bindingFlags));
						obj = list;
						break;
					}
					}
					CerArrayList<T> list3 = new CerArrayList<T>((List<T>)obj);
					Insert(ref list3, null, MemberListType.HandleToInfo);
					return (MethodBase)(object)list3[0];
				}

				internal FieldInfo AddField(RuntimeFieldHandle field)
				{
					List<RuntimeFieldInfo> list = new List<RuntimeFieldInfo>(1);
					FieldAttributes attributes = field.GetAttributes();
					bool isPublic = (attributes & FieldAttributes.FieldAccessMask) == FieldAttributes.Public;
					bool isStatic = (attributes & FieldAttributes.Static) != 0;
					bool isInherited = field.GetApproxDeclaringType().Value != ReflectedTypeHandle.Value;
					BindingFlags bindingFlags = FilterPreCalculate(isPublic, isInherited, isStatic);
					list.Add(new RtFieldInfo(field, ReflectedType, m_runtimeTypeCache, bindingFlags));
					CerArrayList<T> list2 = new CerArrayList<T>((List<T>)(object)list);
					Insert(ref list2, null, MemberListType.HandleToInfo);
					return (FieldInfo)(object)list2[0];
				}

				private unsafe CerArrayList<T> Populate(string name, MemberListType listType, CacheType cacheType)
				{
					if (name == null || name.Length == 0 || (cacheType == CacheType.Constructor && name.FirstChar != '.' && name.FirstChar != '*'))
					{
						Filter filter = new Filter(null, 0, listType);
						List<T> list = null;
						switch (cacheType)
						{
						case CacheType.Method:
							list = PopulateMethods(filter) as List<T>;
							break;
						case CacheType.Field:
							list = PopulateFields(filter) as List<T>;
							break;
						case CacheType.Constructor:
							list = PopulateConstructors(filter) as List<T>;
							break;
						case CacheType.Property:
							list = PopulateProperties(filter) as List<T>;
							break;
						case CacheType.Event:
							list = PopulateEvents(filter) as List<T>;
							break;
						case CacheType.NestedType:
							list = PopulateNestedClasses(filter) as List<T>;
							break;
						case CacheType.Interface:
							list = PopulateInterfaces(filter) as List<T>;
							break;
						}
						CerArrayList<T> list2 = new CerArrayList<T>(list);
						Insert(ref list2, name, listType);
						return list2;
					}
					fixed (char* chars = name)
					{
						int byteCount = Encoding.UTF8.GetByteCount(chars, name.Length);
						byte* ptr = stackalloc byte[1 * byteCount];
						Encoding.UTF8.GetBytes(chars, name.Length, ptr, byteCount);
						Filter filter2 = new Filter(ptr, byteCount, listType);
						List<T> list3 = null;
						switch (cacheType)
						{
						case CacheType.Method:
							list3 = PopulateMethods(filter2) as List<T>;
							break;
						case CacheType.Field:
							list3 = PopulateFields(filter2) as List<T>;
							break;
						case CacheType.Constructor:
							list3 = PopulateConstructors(filter2) as List<T>;
							break;
						case CacheType.Property:
							list3 = PopulateProperties(filter2) as List<T>;
							break;
						case CacheType.Event:
							list3 = PopulateEvents(filter2) as List<T>;
							break;
						case CacheType.NestedType:
							list3 = PopulateNestedClasses(filter2) as List<T>;
							break;
						case CacheType.Interface:
							list3 = PopulateInterfaces(filter2) as List<T>;
							break;
						}
						CerArrayList<T> list4 = new CerArrayList<T>(list3);
						Insert(ref list4, name, listType);
						return list4;
					}
				}

				[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
				internal void Insert(ref CerArrayList<T> list, string name, MemberListType listType)
				{
					bool tookLock = false;
					bool flag = false;
					RuntimeHelpers.PrepareConstrainedRegions();
					try
					{
						Monitor.ReliableEnter(this, ref tookLock);
						switch (listType)
						{
						case MemberListType.CaseSensitive:
							if (m_csMemberInfos == null)
							{
								m_csMemberInfos = new CerHashtable<string, CerArrayList<T>>();
							}
							else
							{
								m_csMemberInfos.Preallocate(1);
							}
							break;
						case MemberListType.CaseInsensitive:
							if (m_cisMemberInfos == null)
							{
								m_cisMemberInfos = new CerHashtable<string, CerArrayList<T>>();
							}
							else
							{
								m_cisMemberInfos.Preallocate(1);
							}
							break;
						}
						if (m_root == null)
						{
							m_root = new CerArrayList<T>(list.Count);
						}
						else
						{
							m_root.Preallocate(list.Count);
						}
						flag = true;
					}
					finally
					{
						try
						{
							if (flag)
							{
								switch (listType)
								{
								case MemberListType.CaseSensitive:
								{
									CerArrayList<T> cerArrayList2 = m_csMemberInfos[name];
									if (cerArrayList2 == null)
									{
										MergeWithGlobalList(list);
										m_csMemberInfos[name] = list;
									}
									else
									{
										list = cerArrayList2;
									}
									break;
								}
								case MemberListType.CaseInsensitive:
								{
									CerArrayList<T> cerArrayList = m_cisMemberInfos[name];
									if (cerArrayList == null)
									{
										MergeWithGlobalList(list);
										m_cisMemberInfos[name] = list;
									}
									else
									{
										list = cerArrayList;
									}
									break;
								}
								default:
									MergeWithGlobalList(list);
									break;
								}
								if (listType == MemberListType.All)
								{
									m_cacheComplete = true;
								}
							}
						}
						finally
						{
							if (tookLock)
							{
								Monitor.Exit(this);
							}
						}
					}
				}

				[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
				private void MergeWithGlobalList(CerArrayList<T> list)
				{
					int count = m_root.Count;
					for (int i = 0; i < list.Count; i++)
					{
						T value = list[i];
						T val = null;
						for (int j = 0; j < count; j++)
						{
							val = m_root[j];
							if (value.CacheEquals(val))
							{
								list.Replace(i, val);
								break;
							}
						}
						if (list[i] != val)
						{
							m_root.Add(value);
						}
					}
				}

				private unsafe List<RuntimeMethodInfo> PopulateMethods(Filter filter)
				{
					List<RuntimeMethodInfo> list = new List<RuntimeMethodInfo>();
					RuntimeTypeHandle declaringTypeHandle = ReflectedTypeHandle;
					if ((declaringTypeHandle.GetAttributes() & TypeAttributes.ClassSemanticsMask) == TypeAttributes.ClassSemanticsMask)
					{
						bool flag = declaringTypeHandle.HasInstantiation() && !declaringTypeHandle.IsGenericTypeDefinition();
						RuntimeTypeHandle.IntroducedMethodEnumerator enumerator = declaringTypeHandle.IntroducedMethods.GetEnumerator();
						while (enumerator.MoveNext())
						{
							RuntimeMethodHandle current = enumerator.Current;
							if (filter.Match(current.GetUtf8Name()))
							{
								MethodAttributes attributes = current.GetAttributes();
								bool isPublic = (attributes & MethodAttributes.MemberAccessMask) == MethodAttributes.Public;
								bool isStatic = (attributes & MethodAttributes.Static) != 0;
								bool isInherited = false;
								BindingFlags bindingFlags = FilterPreCalculate(isPublic, isInherited, isStatic);
								if ((attributes & MethodAttributes.RTSpecialName) == 0 && !current.IsILStub())
								{
									RuntimeMethodHandle handle = (flag ? current.GetInstantiatingStubIfNeeded(declaringTypeHandle) : current);
									RuntimeMethodInfo item = new RuntimeMethodInfo(handle, declaringTypeHandle, m_runtimeTypeCache, attributes, bindingFlags);
									list.Add(item);
								}
							}
						}
					}
					else
					{
						while (declaringTypeHandle.IsGenericVariable())
						{
							declaringTypeHandle = declaringTypeHandle.GetRuntimeType().BaseType.GetTypeHandleInternal();
						}
						bool* ptr = stackalloc bool[1 * declaringTypeHandle.GetNumVirtuals()];
						bool isValueType = declaringTypeHandle.GetRuntimeType().IsValueType;
						while (!declaringTypeHandle.IsNullHandle())
						{
							bool flag2 = declaringTypeHandle.HasInstantiation() && !declaringTypeHandle.IsGenericTypeDefinition();
							int numVirtuals = declaringTypeHandle.GetNumVirtuals();
							RuntimeTypeHandle.IntroducedMethodEnumerator enumerator2 = declaringTypeHandle.IntroducedMethods.GetEnumerator();
							while (enumerator2.MoveNext())
							{
								RuntimeMethodHandle current2 = enumerator2.Current;
								if (!filter.Match(current2.GetUtf8Name()))
								{
									continue;
								}
								MethodAttributes attributes2 = current2.GetAttributes();
								MethodAttributes methodAttributes = attributes2 & MethodAttributes.MemberAccessMask;
								if ((attributes2 & MethodAttributes.RTSpecialName) != 0 || current2.IsILStub())
								{
									continue;
								}
								bool flag3 = false;
								int num = 0;
								if ((attributes2 & MethodAttributes.Virtual) != 0)
								{
									num = current2.GetSlot();
									flag3 = num < numVirtuals;
								}
								bool flag4 = methodAttributes == MethodAttributes.Private;
								bool flag5 = flag3 && flag4;
								bool flag6 = declaringTypeHandle.Value != ReflectedTypeHandle.Value;
								if (flag6 && flag4 && !flag5)
								{
									continue;
								}
								if (flag3)
								{
									if (ptr[num])
									{
										continue;
									}
									ptr[num] = true;
								}
								else if (isValueType && (attributes2 & (MethodAttributes.Virtual | MethodAttributes.Abstract)) != 0)
								{
									continue;
								}
								bool isPublic2 = methodAttributes == MethodAttributes.Public;
								bool isStatic2 = (attributes2 & MethodAttributes.Static) != 0;
								BindingFlags bindingFlags2 = FilterPreCalculate(isPublic2, flag6, isStatic2);
								RuntimeMethodHandle handle2 = (flag2 ? current2.GetInstantiatingStubIfNeeded(declaringTypeHandle) : current2);
								RuntimeMethodInfo item2 = new RuntimeMethodInfo(handle2, declaringTypeHandle, m_runtimeTypeCache, attributes2, bindingFlags2);
								list.Add(item2);
							}
							declaringTypeHandle = declaringTypeHandle.GetBaseTypeHandle();
						}
					}
					return list;
				}

				private List<RuntimeConstructorInfo> PopulateConstructors(Filter filter)
				{
					List<RuntimeConstructorInfo> list = new List<RuntimeConstructorInfo>();
					if (ReflectedType.IsGenericParameter)
					{
						return list;
					}
					RuntimeTypeHandle reflectedTypeHandle = ReflectedTypeHandle;
					bool flag = reflectedTypeHandle.HasInstantiation() && !reflectedTypeHandle.IsGenericTypeDefinition();
					RuntimeTypeHandle.IntroducedMethodEnumerator enumerator = reflectedTypeHandle.IntroducedMethods.GetEnumerator();
					while (enumerator.MoveNext())
					{
						RuntimeMethodHandle current = enumerator.Current;
						if (filter.Match(current.GetUtf8Name()))
						{
							MethodAttributes attributes = current.GetAttributes();
							if ((attributes & MethodAttributes.RTSpecialName) != 0 && !current.IsILStub())
							{
								bool isPublic = (attributes & MethodAttributes.MemberAccessMask) == MethodAttributes.Public;
								bool isStatic = (attributes & MethodAttributes.Static) != 0;
								bool isInherited = false;
								BindingFlags bindingFlags = FilterPreCalculate(isPublic, isInherited, isStatic);
								RuntimeMethodHandle handle = (flag ? current.GetInstantiatingStubIfNeeded(reflectedTypeHandle) : current);
								RuntimeConstructorInfo item = new RuntimeConstructorInfo(handle, ReflectedTypeHandle, m_runtimeTypeCache, attributes, bindingFlags);
								list.Add(item);
							}
						}
					}
					return list;
				}

				private List<RuntimeFieldInfo> PopulateFields(Filter filter)
				{
					List<RuntimeFieldInfo> list = new List<RuntimeFieldInfo>();
					RuntimeTypeHandle declaringTypeHandle = ReflectedTypeHandle;
					while (declaringTypeHandle.IsGenericVariable())
					{
						declaringTypeHandle = declaringTypeHandle.GetRuntimeType().BaseType.GetTypeHandleInternal();
					}
					while (!declaringTypeHandle.IsNullHandle())
					{
						PopulateRtFields(filter, declaringTypeHandle, list);
						PopulateLiteralFields(filter, declaringTypeHandle, list);
						declaringTypeHandle = declaringTypeHandle.GetBaseTypeHandle();
					}
					if (ReflectedType.IsGenericParameter)
					{
						Type[] interfaces = ReflectedTypeHandle.GetRuntimeType().BaseType.GetInterfaces();
						for (int i = 0; i < interfaces.Length; i++)
						{
							PopulateLiteralFields(filter, interfaces[i].GetTypeHandleInternal(), list);
							PopulateRtFields(filter, interfaces[i].GetTypeHandleInternal(), list);
						}
					}
					else
					{
						RuntimeTypeHandle[] interfaces2 = ReflectedTypeHandle.GetInterfaces();
						if (interfaces2 != null)
						{
							for (int j = 0; j < interfaces2.Length; j++)
							{
								PopulateLiteralFields(filter, interfaces2[j], list);
								PopulateRtFields(filter, interfaces2[j], list);
							}
						}
					}
					return list;
				}

				private unsafe void PopulateRtFields(Filter filter, RuntimeTypeHandle declaringTypeHandle, List<RuntimeFieldInfo> list)
				{
					int** ptr = (int**)stackalloc byte[sizeof(int*) * 64];
					int num = 64;
					if (!declaringTypeHandle.GetFields(ptr, &num))
					{
						fixed (int** ptr2 = new int*[num])
						{
							declaringTypeHandle.GetFields(ptr2, &num);
							PopulateRtFields(filter, ptr2, num, declaringTypeHandle, list);
						}
					}
					else if (num > 0)
					{
						PopulateRtFields(filter, ptr, num, declaringTypeHandle, list);
					}
				}

				private unsafe void PopulateRtFields(Filter filter, int** ppFieldHandles, int count, RuntimeTypeHandle declaringTypeHandle, List<RuntimeFieldInfo> list)
				{
					bool flag = declaringTypeHandle.HasInstantiation() && !declaringTypeHandle.ContainsGenericVariables();
					bool flag2 = !declaringTypeHandle.Equals(ReflectedTypeHandle);
					for (int i = 0; i < count; i++)
					{
						RuntimeFieldHandle handle = new RuntimeFieldHandle(ppFieldHandles[i]);
						if (!filter.Match(handle.GetUtf8Name()))
						{
							continue;
						}
						FieldAttributes attributes = handle.GetAttributes();
						FieldAttributes fieldAttributes = attributes & FieldAttributes.FieldAccessMask;
						if (!flag2 || fieldAttributes != FieldAttributes.Private)
						{
							bool isPublic = fieldAttributes == FieldAttributes.Public;
							bool flag3 = (attributes & FieldAttributes.Static) != 0;
							BindingFlags bindingFlags = FilterPreCalculate(isPublic, flag2, flag3);
							if (flag && flag3)
							{
								handle = handle.GetStaticFieldForGenericType(declaringTypeHandle);
							}
							RuntimeFieldInfo item = new RtFieldInfo(handle, declaringTypeHandle.GetRuntimeType(), m_runtimeTypeCache, bindingFlags);
							list.Add(item);
						}
					}
				}

				private unsafe void PopulateLiteralFields(Filter filter, RuntimeTypeHandle declaringTypeHandle, List<RuntimeFieldInfo> list)
				{
					int token = declaringTypeHandle.GetToken();
					if (System.Reflection.MetadataToken.IsNullToken(token))
					{
						return;
					}
					MetadataImport metadataImport = declaringTypeHandle.GetModuleHandle().GetMetadataImport();
					int num = metadataImport.EnumFieldsCount(token);
					int* ptr = (int*)stackalloc byte[4 * num];
					metadataImport.EnumFields(token, ptr, num);
					for (int i = 0; i < num; i++)
					{
						int num2 = ptr[i];
						Utf8String name = metadataImport.GetName(num2);
						if (!filter.Match(name))
						{
							continue;
						}
						metadataImport.GetFieldDefProps(num2, out var fieldAttributes);
						FieldAttributes fieldAttributes2 = fieldAttributes & FieldAttributes.FieldAccessMask;
						if ((fieldAttributes & FieldAttributes.Literal) != 0)
						{
							bool flag = !declaringTypeHandle.Equals(ReflectedTypeHandle);
							if (!flag || fieldAttributes2 != FieldAttributes.Private)
							{
								bool isPublic = fieldAttributes2 == FieldAttributes.Public;
								bool isStatic = (fieldAttributes & FieldAttributes.Static) != 0;
								BindingFlags bindingFlags = FilterPreCalculate(isPublic, flag, isStatic);
								RuntimeFieldInfo item = new MdFieldInfo(num2, fieldAttributes, declaringTypeHandle, m_runtimeTypeCache, bindingFlags);
								list.Add(item);
							}
						}
					}
				}

				private static void AddElementTypes(Type template, IList<Type> types)
				{
					if (!template.HasElementType)
					{
						return;
					}
					AddElementTypes(template.GetElementType(), types);
					for (int i = 0; i < types.Count; i++)
					{
						if (template.IsArray)
						{
							if (template.IsSzArray)
							{
								types[i] = types[i].MakeArrayType();
							}
							else
							{
								types[i] = types[i].MakeArrayType(template.GetArrayRank());
							}
						}
						else if (template.IsPointer)
						{
							types[i] = types[i].MakePointerType();
						}
					}
				}

				private List<RuntimeType> PopulateInterfaces(Filter filter)
				{
					List<RuntimeType> list = new List<RuntimeType>();
					RuntimeTypeHandle reflectedTypeHandle = ReflectedTypeHandle;
					if (!reflectedTypeHandle.IsGenericVariable())
					{
						RuntimeTypeHandle[] interfaces = ReflectedTypeHandle.GetInterfaces();
						if (interfaces != null)
						{
							for (int i = 0; i < interfaces.Length; i++)
							{
								RuntimeType runtimeType = interfaces[i].GetRuntimeType();
								if (filter.Match(runtimeType.GetTypeHandleInternal().GetUtf8Name()))
								{
									list.Add(runtimeType);
								}
							}
						}
						if (ReflectedType.IsSzArray)
						{
							Type elementType = ReflectedType.GetElementType();
							if (!elementType.IsPointer)
							{
								Type type = typeof(IList<>).MakeGenericType(elementType);
								if (type.IsAssignableFrom(ReflectedType))
								{
									if (filter.Match(type.GetTypeHandleInternal().GetUtf8Name()))
									{
										list.Add(type as RuntimeType);
									}
									Type[] interfaces2 = type.GetInterfaces();
									for (int j = 0; j < interfaces2.Length; j++)
									{
										Type type2 = interfaces2[j];
										if (type2.IsGenericType && filter.Match(type2.GetTypeHandleInternal().GetUtf8Name()))
										{
											list.Add(interfaces2[j] as RuntimeType);
										}
									}
								}
							}
						}
					}
					else
					{
						List<RuntimeType> list2 = new List<RuntimeType>();
						Type[] genericParameterConstraints = reflectedTypeHandle.GetRuntimeType().GetGenericParameterConstraints();
						foreach (Type type3 in genericParameterConstraints)
						{
							if (type3.IsInterface)
							{
								list2.Add(type3 as RuntimeType);
							}
							Type[] interfaces3 = type3.GetInterfaces();
							for (int l = 0; l < interfaces3.Length; l++)
							{
								list2.Add(interfaces3[l] as RuntimeType);
							}
						}
						Hashtable hashtable = new Hashtable();
						for (int m = 0; m < list2.Count; m++)
						{
							Type type4 = list2[m];
							if (!hashtable.Contains(type4))
							{
								hashtable[type4] = type4;
							}
						}
						Type[] array = new Type[hashtable.Values.Count];
						hashtable.Values.CopyTo(array, 0);
						for (int n = 0; n < array.Length; n++)
						{
							if (filter.Match(array[n].GetTypeHandleInternal().GetUtf8Name()))
							{
								list.Add(array[n] as RuntimeType);
							}
						}
					}
					return list;
				}

				private unsafe List<RuntimeType> PopulateNestedClasses(Filter filter)
				{
					List<RuntimeType> list = new List<RuntimeType>();
					RuntimeTypeHandle runtimeTypeHandle = ReflectedTypeHandle;
					if (runtimeTypeHandle.IsGenericVariable())
					{
						while (runtimeTypeHandle.IsGenericVariable())
						{
							runtimeTypeHandle = runtimeTypeHandle.GetRuntimeType().BaseType.GetTypeHandleInternal();
						}
					}
					int token = runtimeTypeHandle.GetToken();
					if (System.Reflection.MetadataToken.IsNullToken(token))
					{
						return list;
					}
					ModuleHandle moduleHandle = runtimeTypeHandle.GetModuleHandle();
					MetadataImport metadataImport = moduleHandle.GetMetadataImport();
					int num = metadataImport.EnumNestedTypesCount(token);
					int* ptr = (int*)stackalloc byte[4 * num];
					metadataImport.EnumNestedTypes(token, ptr, num);
					for (int i = 0; i < num; i++)
					{
						RuntimeTypeHandle runtimeTypeHandle2 = default(RuntimeTypeHandle);
						try
						{
							runtimeTypeHandle2 = moduleHandle.ResolveTypeHandle(ptr[i]);
						}
						catch (TypeLoadException)
						{
							continue;
						}
						if (filter.Match(runtimeTypeHandle2.GetRuntimeType().GetTypeHandleInternal().GetUtf8Name()))
						{
							list.Add(runtimeTypeHandle2.GetRuntimeType());
						}
					}
					return list;
				}

				private List<RuntimeEventInfo> PopulateEvents(Filter filter)
				{
					Hashtable csEventInfos = new Hashtable();
					RuntimeTypeHandle declaringTypeHandle = ReflectedTypeHandle;
					List<RuntimeEventInfo> list = new List<RuntimeEventInfo>();
					if ((declaringTypeHandle.GetAttributes() & TypeAttributes.ClassSemanticsMask) != TypeAttributes.ClassSemanticsMask)
					{
						while (declaringTypeHandle.IsGenericVariable())
						{
							declaringTypeHandle = declaringTypeHandle.GetRuntimeType().BaseType.GetTypeHandleInternal();
						}
						while (!declaringTypeHandle.IsNullHandle())
						{
							PopulateEvents(filter, declaringTypeHandle, csEventInfos, list);
							declaringTypeHandle = declaringTypeHandle.GetBaseTypeHandle();
						}
					}
					else
					{
						PopulateEvents(filter, declaringTypeHandle, csEventInfos, list);
					}
					return list;
				}

				private unsafe void PopulateEvents(Filter filter, RuntimeTypeHandle declaringTypeHandle, Hashtable csEventInfos, List<RuntimeEventInfo> list)
				{
					int token = declaringTypeHandle.GetToken();
					if (!System.Reflection.MetadataToken.IsNullToken(token))
					{
						MetadataImport metadataImport = declaringTypeHandle.GetModuleHandle().GetMetadataImport();
						int num = metadataImport.EnumEventsCount(token);
						int* ptr = (int*)stackalloc byte[4 * num];
						metadataImport.EnumEvents(token, ptr, num);
						PopulateEvents(filter, declaringTypeHandle, metadataImport, ptr, num, csEventInfos, list);
					}
				}

				private unsafe void PopulateEvents(Filter filter, RuntimeTypeHandle declaringTypeHandle, MetadataImport scope, int* tkAssociates, int cAssociates, Hashtable csEventInfos, List<RuntimeEventInfo> list)
				{
					for (int i = 0; i < cAssociates; i++)
					{
						int num = tkAssociates[i];
						Utf8String name = scope.GetName(num);
						if (filter.Match(name))
						{
							bool isPrivate;
							RuntimeEventInfo runtimeEventInfo = new RuntimeEventInfo(num, declaringTypeHandle.GetRuntimeType(), m_runtimeTypeCache, out isPrivate);
							if ((declaringTypeHandle.Equals(m_runtimeTypeCache.RuntimeTypeHandle) || !isPrivate) && csEventInfos[runtimeEventInfo.Name] == null)
							{
								csEventInfos[runtimeEventInfo.Name] = runtimeEventInfo;
								list.Add(runtimeEventInfo);
							}
						}
					}
				}

				private List<RuntimePropertyInfo> PopulateProperties(Filter filter)
				{
					Hashtable csPropertyInfos = new Hashtable();
					RuntimeTypeHandle declaringTypeHandle = ReflectedTypeHandle;
					List<RuntimePropertyInfo> list = new List<RuntimePropertyInfo>();
					if ((declaringTypeHandle.GetAttributes() & TypeAttributes.ClassSemanticsMask) != TypeAttributes.ClassSemanticsMask)
					{
						while (declaringTypeHandle.IsGenericVariable())
						{
							declaringTypeHandle = declaringTypeHandle.GetRuntimeType().BaseType.GetTypeHandleInternal();
						}
						while (!declaringTypeHandle.IsNullHandle())
						{
							PopulateProperties(filter, declaringTypeHandle, csPropertyInfos, list);
							declaringTypeHandle = declaringTypeHandle.GetBaseTypeHandle();
						}
					}
					else
					{
						PopulateProperties(filter, declaringTypeHandle, csPropertyInfos, list);
					}
					return list;
				}

				private unsafe void PopulateProperties(Filter filter, RuntimeTypeHandle declaringTypeHandle, Hashtable csPropertyInfos, List<RuntimePropertyInfo> list)
				{
					int token = declaringTypeHandle.GetToken();
					if (!System.Reflection.MetadataToken.IsNullToken(token))
					{
						MetadataImport metadataImport = declaringTypeHandle.GetModuleHandle().GetMetadataImport();
						int num = metadataImport.EnumPropertiesCount(token);
						int* ptr = (int*)stackalloc byte[4 * num];
						metadataImport.EnumProperties(token, ptr, num);
						PopulateProperties(filter, declaringTypeHandle, ptr, num, csPropertyInfos, list);
					}
				}

				private unsafe void PopulateProperties(Filter filter, RuntimeTypeHandle declaringTypeHandle, int* tkAssociates, int cProperties, Hashtable csPropertyInfos, List<RuntimePropertyInfo> list)
				{
					for (int i = 0; i < cProperties; i++)
					{
						int num = tkAssociates[i];
						Utf8String name = declaringTypeHandle.GetRuntimeType().Module.MetadataImport.GetName(num);
						if (!filter.Match(name))
						{
							continue;
						}
						bool isPrivate;
						RuntimePropertyInfo runtimePropertyInfo = new RuntimePropertyInfo(num, declaringTypeHandle.GetRuntimeType(), m_runtimeTypeCache, out isPrivate);
						if (!declaringTypeHandle.Equals(m_runtimeTypeCache.RuntimeTypeHandle) && isPrivate)
						{
							continue;
						}
						List<RuntimePropertyInfo> list2 = csPropertyInfos[runtimePropertyInfo.Name] as List<RuntimePropertyInfo>;
						if (list2 == null)
						{
							list2 = new List<RuntimePropertyInfo>();
							csPropertyInfos[runtimePropertyInfo.Name] = list2;
						}
						else
						{
							for (int j = 0; j < list2.Count; j++)
							{
								if (runtimePropertyInfo.EqualsSig(list2[j]))
								{
									list2 = null;
									break;
								}
							}
						}
						if (list2 != null)
						{
							list2.Add(runtimePropertyInfo);
							list.Add(runtimePropertyInfo);
						}
					}
				}

				internal CerArrayList<T> GetMemberList(MemberListType listType, string name, CacheType cacheType)
				{
					CerArrayList<T> cerArrayList = null;
					switch (listType)
					{
					case MemberListType.CaseSensitive:
						if (m_csMemberInfos == null)
						{
							return Populate(name, listType, cacheType);
						}
						cerArrayList = m_csMemberInfos[name];
						if (cerArrayList == null)
						{
							return Populate(name, listType, cacheType);
						}
						return cerArrayList;
					case MemberListType.All:
						if (m_cacheComplete)
						{
							return m_root;
						}
						return Populate(null, listType, cacheType);
					default:
						if (m_cisMemberInfos == null)
						{
							return Populate(name, listType, cacheType);
						}
						cerArrayList = m_cisMemberInfos[name];
						if (cerArrayList == null)
						{
							return Populate(name, listType, cacheType);
						}
						return cerArrayList;
					}
				}
			}

			private WhatsCached m_whatsCached;

			private RuntimeTypeHandle m_runtimeTypeHandle;

			private RuntimeType m_runtimeType;

			private RuntimeType m_enclosingType;

			private TypeCode m_typeCode;

			private string m_name;

			private string m_fullname;

			private string m_toString;

			private string m_namespace;

			private bool m_isGlobal;

			private bool m_bIsDomainInitialized;

			private MemberInfoCache<RuntimeMethodInfo> m_methodInfoCache;

			private MemberInfoCache<RuntimeConstructorInfo> m_constructorInfoCache;

			private MemberInfoCache<RuntimeFieldInfo> m_fieldInfoCache;

			private MemberInfoCache<RuntimeType> m_interfaceCache;

			private MemberInfoCache<RuntimeType> m_nestedClassesCache;

			private MemberInfoCache<RuntimePropertyInfo> m_propertyInfoCache;

			private MemberInfoCache<RuntimeEventInfo> m_eventInfoCache;

			private static CerHashtable<RuntimeMethodInfo, RuntimeMethodInfo> s_methodInstantiations;

			private static bool s_dontrunhack;

			internal bool DomainInitialized
			{
				get
				{
					return m_bIsDomainInitialized;
				}
				set
				{
					m_bIsDomainInitialized = value;
				}
			}

			internal TypeCode TypeCode
			{
				get
				{
					return m_typeCode;
				}
				set
				{
					m_typeCode = value;
				}
			}

			internal bool IsGlobal
			{
				[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
				get
				{
					return m_isGlobal;
				}
			}

			internal RuntimeType RuntimeType => m_runtimeType;

			internal RuntimeTypeHandle RuntimeTypeHandle => m_runtimeTypeHandle;

			internal static void Prejitinit_HACK()
			{
				if (!s_dontrunhack)
				{
					RuntimeHelpers.PrepareConstrainedRegions();
					try
					{
					}
					finally
					{
						MemberInfoCache<RuntimeMethodInfo> memberInfoCache = new MemberInfoCache<RuntimeMethodInfo>(null);
						CerArrayList<RuntimeMethodInfo> list = null;
						memberInfoCache.Insert(ref list, "dummy", MemberListType.All);
						MemberInfoCache<RuntimeConstructorInfo> memberInfoCache2 = new MemberInfoCache<RuntimeConstructorInfo>(null);
						CerArrayList<RuntimeConstructorInfo> list2 = null;
						memberInfoCache2.Insert(ref list2, "dummy", MemberListType.All);
						MemberInfoCache<RuntimeFieldInfo> memberInfoCache3 = new MemberInfoCache<RuntimeFieldInfo>(null);
						CerArrayList<RuntimeFieldInfo> list3 = null;
						memberInfoCache3.Insert(ref list3, "dummy", MemberListType.All);
						MemberInfoCache<RuntimeType> memberInfoCache4 = new MemberInfoCache<RuntimeType>(null);
						CerArrayList<RuntimeType> list4 = null;
						memberInfoCache4.Insert(ref list4, "dummy", MemberListType.All);
						MemberInfoCache<RuntimePropertyInfo> memberInfoCache5 = new MemberInfoCache<RuntimePropertyInfo>(null);
						CerArrayList<RuntimePropertyInfo> list5 = null;
						memberInfoCache5.Insert(ref list5, "dummy", MemberListType.All);
						MemberInfoCache<RuntimeEventInfo> memberInfoCache6 = new MemberInfoCache<RuntimeEventInfo>(null);
						CerArrayList<RuntimeEventInfo> list6 = null;
						memberInfoCache6.Insert(ref list6, "dummy", MemberListType.All);
					}
				}
			}

			internal RuntimeTypeCache(RuntimeType runtimeType)
			{
				m_typeCode = TypeCode.Empty;
				m_runtimeType = runtimeType;
				m_runtimeTypeHandle = runtimeType.GetTypeHandleInternal();
				m_isGlobal = m_runtimeTypeHandle.GetModuleHandle().GetModuleTypeHandle().Equals(m_runtimeTypeHandle);
				s_dontrunhack = true;
				Prejitinit_HACK();
			}

			private string ConstructName(ref string name, bool nameSpace, bool fullinst, bool assembly)
			{
				if (name == null)
				{
					name = RuntimeTypeHandle.ConstructName(nameSpace, fullinst, assembly);
				}
				return name;
			}

			private CerArrayList<T> GetMemberList<T>(ref MemberInfoCache<T> m_cache, MemberListType listType, string name, CacheType cacheType) where T : MemberInfo
			{
				MemberInfoCache<T> memberCache = GetMemberCache(ref m_cache);
				return memberCache.GetMemberList(listType, name, cacheType);
			}

			private MemberInfoCache<T> GetMemberCache<T>(ref MemberInfoCache<T> m_cache) where T : MemberInfo
			{
				MemberInfoCache<T> memberInfoCache = m_cache;
				if (memberInfoCache == null)
				{
					MemberInfoCache<T> memberInfoCache2 = new MemberInfoCache<T>(this);
					memberInfoCache = Interlocked.CompareExchange(ref m_cache, memberInfoCache2, null);
					if (memberInfoCache == null)
					{
						memberInfoCache = memberInfoCache2;
					}
				}
				return memberInfoCache;
			}

			internal string GetName()
			{
				return ConstructName(ref m_name, nameSpace: false, fullinst: false, assembly: false);
			}

			internal string GetNameSpace()
			{
				if (m_namespace == null)
				{
					Type runtimeType = m_runtimeType;
					runtimeType = runtimeType.GetRootElementType();
					while (runtimeType.IsNested)
					{
						runtimeType = runtimeType.DeclaringType;
					}
					m_namespace = runtimeType.GetTypeHandleInternal().GetModuleHandle().GetMetadataImport()
						.GetNamespace(runtimeType.MetadataToken)
						.ToString();
				}
				return m_namespace;
			}

			internal string GetToString()
			{
				return ConstructName(ref m_toString, nameSpace: true, fullinst: false, assembly: false);
			}

			internal string GetFullName()
			{
				if (!m_runtimeType.IsGenericTypeDefinition && m_runtimeType.ContainsGenericParameters)
				{
					return null;
				}
				return ConstructName(ref m_fullname, nameSpace: true, fullinst: true, assembly: false);
			}

			internal RuntimeType GetEnclosingType()
			{
				if ((m_whatsCached & WhatsCached.EnclosingType) == 0)
				{
					m_enclosingType = RuntimeTypeHandle.GetDeclaringType().GetRuntimeType();
					m_whatsCached |= WhatsCached.EnclosingType;
				}
				return m_enclosingType;
			}

			internal void InvalidateCachedNestedType()
			{
				m_nestedClassesCache = null;
			}

			internal MethodInfo GetGenericMethodInfo(RuntimeMethodHandle genericMethod)
			{
				if (s_methodInstantiations == null)
				{
					Interlocked.CompareExchange(ref s_methodInstantiations, new CerHashtable<RuntimeMethodInfo, RuntimeMethodInfo>(), null);
				}
				RuntimeMethodInfo runtimeMethodInfo = new RuntimeMethodInfo(genericMethod, genericMethod.GetDeclaringType(), this, genericMethod.GetAttributes(), (BindingFlags)(-1));
				RuntimeMethodInfo runtimeMethodInfo2 = null;
				runtimeMethodInfo2 = s_methodInstantiations[runtimeMethodInfo];
				if (runtimeMethodInfo2 != null)
				{
					return runtimeMethodInfo2;
				}
				bool tookLock = false;
				bool flag = false;
				RuntimeHelpers.PrepareConstrainedRegions();
				try
				{
					Monitor.ReliableEnter(s_methodInstantiations, ref tookLock);
					runtimeMethodInfo2 = s_methodInstantiations[runtimeMethodInfo];
					if (runtimeMethodInfo2 != null)
					{
						return runtimeMethodInfo2;
					}
					s_methodInstantiations.Preallocate(1);
					flag = true;
				}
				finally
				{
					if (flag)
					{
						s_methodInstantiations[runtimeMethodInfo] = runtimeMethodInfo;
					}
					if (tookLock)
					{
						Monitor.Exit(s_methodInstantiations);
					}
				}
				return runtimeMethodInfo;
			}

			internal CerArrayList<RuntimeMethodInfo> GetMethodList(MemberListType listType, string name)
			{
				return GetMemberList(ref m_methodInfoCache, listType, name, CacheType.Method);
			}

			internal CerArrayList<RuntimeConstructorInfo> GetConstructorList(MemberListType listType, string name)
			{
				return GetMemberList(ref m_constructorInfoCache, listType, name, CacheType.Constructor);
			}

			internal CerArrayList<RuntimePropertyInfo> GetPropertyList(MemberListType listType, string name)
			{
				return GetMemberList(ref m_propertyInfoCache, listType, name, CacheType.Property);
			}

			internal CerArrayList<RuntimeEventInfo> GetEventList(MemberListType listType, string name)
			{
				return GetMemberList(ref m_eventInfoCache, listType, name, CacheType.Event);
			}

			internal CerArrayList<RuntimeFieldInfo> GetFieldList(MemberListType listType, string name)
			{
				return GetMemberList(ref m_fieldInfoCache, listType, name, CacheType.Field);
			}

			internal CerArrayList<RuntimeType> GetInterfaceList(MemberListType listType, string name)
			{
				return GetMemberList(ref m_interfaceCache, listType, name, CacheType.Interface);
			}

			internal CerArrayList<RuntimeType> GetNestedTypeList(MemberListType listType, string name)
			{
				return GetMemberList(ref m_nestedClassesCache, listType, name, CacheType.NestedType);
			}

			internal MethodBase GetMethod(RuntimeTypeHandle declaringType, RuntimeMethodHandle method)
			{
				GetMemberCache(ref m_methodInfoCache);
				return m_methodInfoCache.AddMethod(declaringType, method, CacheType.Method);
			}

			internal MethodBase GetConstructor(RuntimeTypeHandle declaringType, RuntimeMethodHandle constructor)
			{
				GetMemberCache(ref m_constructorInfoCache);
				return m_constructorInfoCache.AddMethod(declaringType, constructor, CacheType.Constructor);
			}

			internal FieldInfo GetField(RuntimeFieldHandle field)
			{
				GetMemberCache(ref m_fieldInfoCache);
				return m_fieldInfoCache.AddField(field);
			}
		}

		private class TypeCacheQueue
		{
			private const int QUEUE_SIZE = 4;

			private object[] liveCache;

			internal TypeCacheQueue()
			{
				liveCache = new object[4];
			}
		}

		private class ActivatorCacheEntry
		{
			internal Type m_type;

			internal CtorDelegate m_ctor;

			internal RuntimeMethodHandle m_hCtorMethodHandle;

			internal bool m_bNeedSecurityCheck;

			internal bool m_bFullyInitialized;

			internal ActivatorCacheEntry(Type t, RuntimeMethodHandle rmh, bool bNeedSecurityCheck)
			{
				m_type = t;
				m_bNeedSecurityCheck = bNeedSecurityCheck;
				m_hCtorMethodHandle = rmh;
			}
		}

		private class ActivatorCache
		{
			private const int CACHE_SIZE = 16;

			private int hash_counter;

			private ActivatorCacheEntry[] cache = new ActivatorCacheEntry[16];

			private ConstructorInfo delegateCtorInfo;

			private PermissionSet delegateCreatePermissions;

			private void InitializeDelegateCreator()
			{
				PermissionSet permissionSet = new PermissionSet(PermissionState.None);
				permissionSet.AddPermission(new ReflectionPermission(ReflectionPermissionFlag.MemberAccess));
				permissionSet.AddPermission(new SecurityPermission(SecurityPermissionFlag.UnmanagedCode));
				Thread.MemoryBarrier();
				delegateCreatePermissions = permissionSet;
				ConstructorInfo constructor = typeof(CtorDelegate).GetConstructor(new Type[2]
				{
					typeof(object),
					typeof(IntPtr)
				});
				Thread.MemoryBarrier();
				delegateCtorInfo = constructor;
			}

			private void InitializeCacheEntry(ActivatorCacheEntry ace)
			{
				if (!ace.m_type.IsValueType)
				{
					if (delegateCtorInfo == null)
					{
						InitializeDelegateCreator();
					}
					delegateCreatePermissions.Assert();
					CtorDelegate ctor = (CtorDelegate)delegateCtorInfo.Invoke(new object[2]
					{
						null,
						ace.m_hCtorMethodHandle.GetFunctionPointer()
					});
					Thread.MemoryBarrier();
					ace.m_ctor = ctor;
				}
				ace.m_bFullyInitialized = true;
			}

			internal ActivatorCacheEntry GetEntry(Type t)
			{
				int num = hash_counter;
				for (int i = 0; i < 16; i++)
				{
					ActivatorCacheEntry activatorCacheEntry = cache[num];
					if (activatorCacheEntry != null && activatorCacheEntry.m_type == t)
					{
						if (!activatorCacheEntry.m_bFullyInitialized)
						{
							InitializeCacheEntry(activatorCacheEntry);
						}
						return activatorCacheEntry;
					}
					num = (num + 1) & 0xF;
				}
				return null;
			}

			internal void SetEntry(ActivatorCacheEntry ace)
			{
				int num = (hash_counter = (hash_counter - 1) & 0xF);
				cache[num] = ace;
			}
		}

		[Flags]
		private enum DispatchWrapperType
		{
			Unknown = 0x1,
			Dispatch = 0x2,
			Record = 0x4,
			Error = 0x8,
			Currency = 0x10,
			BStr = 0x20,
			SafeArray = 0x10000
		}

		private const BindingFlags MemberBindingMask = (BindingFlags)255;

		private const BindingFlags InvocationMask = BindingFlags.InvokeMethod | BindingFlags.CreateInstance | BindingFlags.GetField | BindingFlags.SetField | BindingFlags.GetProperty | BindingFlags.SetProperty | BindingFlags.PutDispProperty | BindingFlags.PutRefDispProperty;

		private const BindingFlags BinderNonCreateInstance = BindingFlags.InvokeMethod | BindingFlags.GetField | BindingFlags.SetField | BindingFlags.GetProperty | BindingFlags.SetProperty;

		private const BindingFlags BinderGetSetProperty = BindingFlags.GetProperty | BindingFlags.SetProperty;

		private const BindingFlags BinderSetInvokeProperty = BindingFlags.InvokeMethod | BindingFlags.SetProperty;

		private const BindingFlags BinderGetSetField = BindingFlags.GetField | BindingFlags.SetField;

		private const BindingFlags BinderSetInvokeField = BindingFlags.InvokeMethod | BindingFlags.SetField;

		private const BindingFlags BinderNonFieldGetSet = (BindingFlags)16773888;

		private const BindingFlags ClassicBindingMask = BindingFlags.InvokeMethod | BindingFlags.GetProperty | BindingFlags.SetProperty | BindingFlags.PutDispProperty | BindingFlags.PutRefDispProperty;

		private IntPtr m_cache;

		private RuntimeTypeHandle m_handle;

		private static TypeCacheQueue s_typeCache = null;

		private static Type s_typedRef = typeof(TypedReference);

		private static bool forceInvokingWithEnUS = ForceEnUSLcidComInvoking();

		private static ActivatorCache s_ActivatorCache;

		private static OleAutBinder s_ForwardCallBinder;

		internal bool DomainInitialized
		{
			get
			{
				return Cache.DomainInitialized;
			}
			set
			{
				Cache.DomainInitialized = value;
			}
		}

		private new RuntimeTypeCache Cache
		{
			get
			{
				if (m_cache.IsNull())
				{
					IntPtr gCHandle = m_handle.GetGCHandle(GCHandleType.WeakTrackResurrection);
					if (!Interlocked.CompareExchange(ref m_cache, gCHandle, (IntPtr)0).IsNull())
					{
						m_handle.FreeGCHandle(gCHandle);
					}
				}
				RuntimeTypeCache runtimeTypeCache = GCHandle.InternalGet(m_cache) as RuntimeTypeCache;
				if (runtimeTypeCache == null)
				{
					runtimeTypeCache = new RuntimeTypeCache(this);
					RuntimeTypeCache runtimeTypeCache2 = GCHandle.InternalCompareExchange(m_cache, runtimeTypeCache, null, isPinned: false) as RuntimeTypeCache;
					if (runtimeTypeCache2 != null)
					{
						runtimeTypeCache = runtimeTypeCache2;
					}
					if (s_typeCache == null)
					{
						s_typeCache = new TypeCacheQueue();
					}
				}
				return runtimeTypeCache;
			}
		}

		public override Module Module => GetTypeHandleInternal().GetModuleHandle().GetModule();

		public override Assembly Assembly => GetTypeHandleInternal().GetAssemblyHandle().GetAssembly();

		public override RuntimeTypeHandle TypeHandle => m_handle;

		public override MethodBase DeclaringMethod
		{
			get
			{
				if (!IsGenericParameter)
				{
					throw new InvalidOperationException(Environment.GetResourceString("Arg_NotGenericParameter"));
				}
				RuntimeMethodHandle declaringMethod = GetTypeHandleInternal().GetDeclaringMethod();
				if (declaringMethod.IsNullHandle())
				{
					return null;
				}
				return GetMethodBase(declaringMethod.GetDeclaringType(), declaringMethod);
			}
		}

		public override Type BaseType
		{
			get
			{
				if (base.IsInterface)
				{
					return null;
				}
				if (m_handle.IsGenericVariable())
				{
					Type[] genericParameterConstraints = GetGenericParameterConstraints();
					Type type = typeof(object);
					foreach (Type type2 in genericParameterConstraints)
					{
						if (type2.IsInterface)
						{
							continue;
						}
						if (type2.IsGenericParameter)
						{
							GenericParameterAttributes genericParameterAttributes = type2.GenericParameterAttributes & GenericParameterAttributes.SpecialConstraintMask;
							if ((genericParameterAttributes & GenericParameterAttributes.ReferenceTypeConstraint) == 0 && (genericParameterAttributes & GenericParameterAttributes.NotNullableValueTypeConstraint) == 0)
							{
								continue;
							}
						}
						type = type2;
					}
					if (type == typeof(object))
					{
						GenericParameterAttributes genericParameterAttributes2 = GenericParameterAttributes & GenericParameterAttributes.SpecialConstraintMask;
						if ((genericParameterAttributes2 & GenericParameterAttributes.NotNullableValueTypeConstraint) != 0)
						{
							type = typeof(ValueType);
						}
					}
					return type;
				}
				return m_handle.GetBaseTypeHandle().GetRuntimeType();
			}
		}

		public override Type UnderlyingSystemType => this;

		public override string FullName => Cache.GetFullName();

		public override string AssemblyQualifiedName
		{
			get
			{
				if (!IsGenericTypeDefinition && ContainsGenericParameters)
				{
					return null;
				}
				return Assembly.CreateQualifiedName(Assembly.FullName, FullName);
			}
		}

		public override string Namespace
		{
			get
			{
				string nameSpace = Cache.GetNameSpace();
				if (nameSpace == null || nameSpace.Length == 0)
				{
					return null;
				}
				return nameSpace;
			}
		}

		public override Guid GUID
		{
			get
			{
				Guid result = default(Guid);
				GetGUID(ref result);
				return result;
			}
		}

		public override GenericParameterAttributes GenericParameterAttributes
		{
			get
			{
				if (!IsGenericParameter)
				{
					throw new InvalidOperationException(Environment.GetResourceString("Arg_NotGenericParameter"));
				}
				GetTypeHandleInternal().GetModuleHandle().GetMetadataImport().GetGenericParamProps(MetadataToken, out var attributes);
				return attributes;
			}
		}

		internal override bool IsSzArray
		{
			get
			{
				CorElementType corElementType = GetTypeHandleInternal().GetCorElementType();
				return corElementType == CorElementType.SzArray;
			}
		}

		public override bool IsGenericTypeDefinition => m_handle.IsGenericTypeDefinition();

		public override bool IsGenericParameter => m_handle.IsGenericVariable();

		public override int GenericParameterPosition
		{
			get
			{
				if (!IsGenericParameter)
				{
					throw new InvalidOperationException(Environment.GetResourceString("Arg_NotGenericParameter"));
				}
				return m_handle.GetGenericVariableIndex();
			}
		}

		public override bool IsGenericType
		{
			get
			{
				if (!base.HasElementType)
				{
					return GetTypeHandleInternal().HasInstantiation();
				}
				return false;
			}
		}

		public override bool ContainsGenericParameters => GetRootElementType().GetTypeHandleInternal().ContainsGenericVariables();

		public override StructLayoutAttribute StructLayoutAttribute => (StructLayoutAttribute)StructLayoutAttribute.GetCustomAttribute(this);

		public override string Name => Cache.GetName();

		public override MemberTypes MemberType
		{
			get
			{
				if (base.IsPublic || base.IsNotPublic)
				{
					return MemberTypes.TypeInfo;
				}
				return MemberTypes.NestedType;
			}
		}

		public override Type DeclaringType => Cache.GetEnclosingType();

		public override Type ReflectedType => DeclaringType;

		public override int MetadataToken => m_handle.GetToken();

		private OleAutBinder ForwardCallBinder
		{
			get
			{
				if (s_ForwardCallBinder == null)
				{
					s_ForwardCallBinder = new OleAutBinder();
				}
				return s_ForwardCallBinder;
			}
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern void PrepareMemberInfoCache(RuntimeTypeHandle rt);

		internal static MethodBase GetMethodBase(ModuleHandle scope, int typeMetadataToken)
		{
			return GetMethodBase(scope.ResolveMethodHandle(typeMetadataToken));
		}

		internal static MethodBase GetMethodBase(Module scope, int typeMetadataToken)
		{
			return GetMethodBase(scope.GetModuleHandle(), typeMetadataToken);
		}

		internal static MethodBase GetMethodBase(RuntimeMethodHandle methodHandle)
		{
			return GetMethodBase(RuntimeTypeHandle.EmptyHandle, methodHandle);
		}

		internal static MethodBase GetMethodBase(RuntimeTypeHandle reflectedTypeHandle, RuntimeMethodHandle methodHandle)
		{
			if (methodHandle.IsDynamicMethod())
			{
				return methodHandle.GetResolver()?.GetDynamicMethod();
			}
			Type type = methodHandle.GetDeclaringType().GetRuntimeType();
			RuntimeType runtimeType = reflectedTypeHandle.GetRuntimeType();
			RuntimeTypeHandle[] methodInstantiation = null;
			bool flag = false;
			if (runtimeType == null)
			{
				runtimeType = type as RuntimeType;
			}
			if (runtimeType.IsArray)
			{
				MethodBase[] array = runtimeType.GetMember(methodHandle.GetName(), MemberTypes.Constructor | MemberTypes.Method, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) as MethodBase[];
				bool flag2 = false;
				for (int i = 0; i < array.Length; i++)
				{
					if (array[i].GetMethodHandle() == methodHandle)
					{
						flag2 = true;
					}
				}
				if (!flag2)
				{
					throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Argument_ResolveMethodHandle"), runtimeType.ToString(), type.ToString()));
				}
				type = runtimeType;
			}
			else if (!type.IsAssignableFrom(runtimeType))
			{
				if (!type.IsGenericType)
				{
					throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Argument_ResolveMethodHandle"), runtimeType.ToString(), type.ToString()));
				}
				Type genericTypeDefinition = type.GetGenericTypeDefinition();
				Type type2;
				for (type2 = runtimeType; type2 != null; type2 = type2.BaseType)
				{
					Type type3 = type2;
					if (type3.IsGenericType && !type2.IsGenericTypeDefinition)
					{
						type3 = type3.GetGenericTypeDefinition();
					}
					if (type3.Equals(genericTypeDefinition))
					{
						break;
					}
				}
				if (type2 == null)
				{
					throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Argument_ResolveMethodHandle"), runtimeType.ToString(), type.ToString()));
				}
				type = type2;
				methodInstantiation = methodHandle.GetMethodInstantiation();
				bool flag3 = methodHandle.IsGenericMethodDefinition();
				methodHandle = methodHandle.GetMethodFromCanonical(type.GetTypeHandleInternal());
				if (!flag3)
				{
					flag = true;
				}
			}
			if (type.IsValueType)
			{
				methodHandle = methodHandle.GetUnboxingStub();
			}
			if (flag || (type.GetTypeHandleInternal().HasInstantiation() && !type.GetTypeHandleInternal().IsGenericTypeDefinition() && !methodHandle.HasMethodInstantiation()))
			{
				methodHandle = methodHandle.GetInstantiatingStub(type.GetTypeHandleInternal(), methodInstantiation);
			}
			if (methodHandle.IsConstructor())
			{
				return runtimeType.Cache.GetConstructor(type.GetTypeHandleInternal(), methodHandle);
			}
			if (methodHandle.HasMethodInstantiation() && !methodHandle.IsGenericMethodDefinition())
			{
				return runtimeType.Cache.GetGenericMethodInfo(methodHandle);
			}
			return runtimeType.Cache.GetMethod(type.GetTypeHandleInternal(), methodHandle);
		}

		internal static FieldInfo GetFieldInfo(RuntimeFieldHandle fieldHandle)
		{
			return GetFieldInfo(fieldHandle.GetApproxDeclaringType(), fieldHandle);
		}

		internal static FieldInfo GetFieldInfo(RuntimeTypeHandle reflectedTypeHandle, RuntimeFieldHandle fieldHandle)
		{
			if (reflectedTypeHandle.IsNullHandle())
			{
				reflectedTypeHandle = fieldHandle.GetApproxDeclaringType();
			}
			else
			{
				RuntimeTypeHandle approxDeclaringType = fieldHandle.GetApproxDeclaringType();
				if (!reflectedTypeHandle.Equals(approxDeclaringType) && (!fieldHandle.AcquiresContextFromThis() || !approxDeclaringType.GetCanonicalHandle().Equals(reflectedTypeHandle.GetCanonicalHandle())))
				{
					throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Argument_ResolveFieldHandle"), reflectedTypeHandle.GetRuntimeType().ToString(), approxDeclaringType.GetRuntimeType().ToString()));
				}
			}
			return reflectedTypeHandle.GetRuntimeType().Cache.GetField(fieldHandle);
		}

		internal static PropertyInfo GetPropertyInfo(RuntimeTypeHandle reflectedTypeHandle, int tkProperty)
		{
			RuntimePropertyInfo runtimePropertyInfo = null;
			CerArrayList<RuntimePropertyInfo> propertyList = reflectedTypeHandle.GetRuntimeType().Cache.GetPropertyList(MemberListType.All, null);
			for (int i = 0; i < propertyList.Count; i++)
			{
				runtimePropertyInfo = propertyList[i];
				if (runtimePropertyInfo.MetadataToken == tkProperty)
				{
					return runtimePropertyInfo;
				}
			}
			throw new SystemException();
		}

		private static void ThrowIfTypeNeverValidGenericArgument(Type type)
		{
			if (type.IsPointer || type.IsByRef || type == typeof(void))
			{
				throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Argument_NeverValidGenericArgument"), type.ToString()));
			}
		}

		internal static void SanityCheckGenericArguments(Type[] genericArguments, Type[] genericParamters)
		{
			if (genericArguments == null)
			{
				throw new ArgumentNullException();
			}
			for (int i = 0; i < genericArguments.Length; i++)
			{
				if (genericArguments[i] == null)
				{
					throw new ArgumentNullException();
				}
				ThrowIfTypeNeverValidGenericArgument(genericArguments[i]);
			}
			if (genericArguments.Length != genericParamters.Length)
			{
				throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Argument_NotEnoughGenArguments", genericArguments.Length, genericParamters.Length)));
			}
		}

		internal static void ValidateGenericArguments(MemberInfo definition, Type[] genericArguments, Exception e)
		{
			RuntimeTypeHandle[] array = null;
			RuntimeTypeHandle[] array2 = null;
			Type[] array3 = null;
			if (definition is Type)
			{
				Type type = (Type)definition;
				array3 = type.GetGenericArguments();
				array = new RuntimeTypeHandle[genericArguments.Length];
				for (int i = 0; i < genericArguments.Length; i++)
				{
					ref RuntimeTypeHandle reference = ref array[i];
					reference = genericArguments[i].GetTypeHandleInternal();
				}
			}
			else
			{
				MethodInfo methodInfo = (MethodInfo)definition;
				array3 = methodInfo.GetGenericArguments();
				array2 = new RuntimeTypeHandle[genericArguments.Length];
				for (int j = 0; j < genericArguments.Length; j++)
				{
					ref RuntimeTypeHandle reference2 = ref array2[j];
					reference2 = genericArguments[j].GetTypeHandleInternal();
				}
				Type declaringType = methodInfo.DeclaringType;
				if (declaringType != null)
				{
					array = declaringType.GetTypeHandleInternal().GetInstantiation();
				}
			}
			for (int k = 0; k < genericArguments.Length; k++)
			{
				Type type2 = genericArguments[k];
				Type type3 = array3[k];
				if (!type3.GetTypeHandleInternal().SatisfiesConstraints(array, array2, type2.GetTypeHandleInternal()))
				{
					throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Argument_GenConstraintViolation"), k.ToString(CultureInfo.CurrentCulture), type2.ToString(), definition.ToString(), type3.ToString()), e);
				}
			}
		}

		private static void SplitName(string fullname, out string name, out string ns)
		{
			name = null;
			ns = null;
			if (fullname == null)
			{
				return;
			}
			int num = fullname.LastIndexOf(".", StringComparison.Ordinal);
			if (num != -1)
			{
				ns = fullname.Substring(0, num);
				int num2 = fullname.Length - ns.Length - 1;
				if (num2 != 0)
				{
					name = fullname.Substring(num + 1, num2);
				}
				else
				{
					name = "";
				}
			}
			else
			{
				name = fullname;
			}
		}

		internal static BindingFlags FilterPreCalculate(bool isPublic, bool isInherited, bool isStatic)
		{
			BindingFlags bindingFlags = (isPublic ? BindingFlags.Public : BindingFlags.NonPublic);
			if (isInherited)
			{
				bindingFlags |= BindingFlags.DeclaredOnly;
				if (isStatic)
				{
					return bindingFlags | (BindingFlags.Static | BindingFlags.FlattenHierarchy);
				}
				return bindingFlags | BindingFlags.Instance;
			}
			if (isStatic)
			{
				return bindingFlags | BindingFlags.Static;
			}
			return bindingFlags | BindingFlags.Instance;
		}

		private static void FilterHelper(BindingFlags bindingFlags, ref string name, bool allowPrefixLookup, out bool prefixLookup, out bool ignoreCase, out MemberListType listType)
		{
			prefixLookup = false;
			ignoreCase = false;
			if (name != null)
			{
				if ((bindingFlags & BindingFlags.IgnoreCase) != 0)
				{
					name = name.ToLower(CultureInfo.InvariantCulture);
					ignoreCase = true;
					listType = MemberListType.CaseInsensitive;
				}
				else
				{
					listType = MemberListType.CaseSensitive;
				}
				if (allowPrefixLookup && name.EndsWith("*", StringComparison.Ordinal))
				{
					name = name.Substring(0, name.Length - 1);
					prefixLookup = true;
					listType = MemberListType.All;
				}
			}
			else
			{
				listType = MemberListType.All;
			}
		}

		private static void FilterHelper(BindingFlags bindingFlags, ref string name, out bool ignoreCase, out MemberListType listType)
		{
			FilterHelper(bindingFlags, ref name, allowPrefixLookup: false, out var _, out ignoreCase, out listType);
		}

		private static bool FilterApplyPrefixLookup(MemberInfo memberInfo, string name, bool ignoreCase)
		{
			if (ignoreCase)
			{
				if (!memberInfo.Name.ToLower(CultureInfo.InvariantCulture).StartsWith(name, StringComparison.Ordinal))
				{
					return false;
				}
			}
			else if (!memberInfo.Name.StartsWith(name, StringComparison.Ordinal))
			{
				return false;
			}
			return true;
		}

		private static bool FilterApplyBase(MemberInfo memberInfo, BindingFlags bindingFlags, bool isPublic, bool isNonProtectedInternal, bool isStatic, string name, bool prefixLookup)
		{
			if (isPublic)
			{
				if ((bindingFlags & BindingFlags.Public) == 0)
				{
					return false;
				}
			}
			else if ((bindingFlags & BindingFlags.NonPublic) == 0)
			{
				return false;
			}
			bool flag = memberInfo.DeclaringType != memberInfo.ReflectedType;
			if ((bindingFlags & BindingFlags.DeclaredOnly) != 0 && flag)
			{
				return false;
			}
			if (memberInfo.MemberType != MemberTypes.TypeInfo && memberInfo.MemberType != MemberTypes.NestedType)
			{
				if (isStatic)
				{
					if ((bindingFlags & BindingFlags.FlattenHierarchy) == 0 && flag)
					{
						return false;
					}
					if ((bindingFlags & BindingFlags.Static) == 0)
					{
						return false;
					}
				}
				else if ((bindingFlags & BindingFlags.Instance) == 0)
				{
					return false;
				}
			}
			if (prefixLookup && !FilterApplyPrefixLookup(memberInfo, name, (bindingFlags & BindingFlags.IgnoreCase) != 0))
			{
				return false;
			}
			if ((bindingFlags & BindingFlags.DeclaredOnly) == 0 && flag && isNonProtectedInternal && (bindingFlags & BindingFlags.NonPublic) != 0 && !isStatic && (bindingFlags & BindingFlags.Instance) != 0)
			{
				MethodInfo methodInfo = memberInfo as MethodInfo;
				if (methodInfo == null)
				{
					return false;
				}
				if (!methodInfo.IsVirtual && !methodInfo.IsAbstract)
				{
					return false;
				}
			}
			return true;
		}

		private static bool FilterApplyType(Type type, BindingFlags bindingFlags, string name, bool prefixLookup, string ns)
		{
			bool isPublic = type.IsNestedPublic || type.IsPublic;
			bool isStatic = false;
			if (!FilterApplyBase(type, bindingFlags, isPublic, type.IsNestedAssembly, isStatic, name, prefixLookup))
			{
				return false;
			}
			if (ns != null && !type.Namespace.Equals(ns))
			{
				return false;
			}
			return true;
		}

		private static bool FilterApplyMethodBaseInfo(MethodBase methodBase, BindingFlags bindingFlags, string name, CallingConventions callConv, Type[] argumentTypes, bool prefixLookup)
		{
			bindingFlags ^= BindingFlags.DeclaredOnly;
			RuntimeMethodInfo runtimeMethodInfo = methodBase as RuntimeMethodInfo;
			BindingFlags bindingFlags2;
			if (runtimeMethodInfo == null)
			{
				RuntimeConstructorInfo runtimeConstructorInfo = methodBase as RuntimeConstructorInfo;
				bindingFlags2 = runtimeConstructorInfo.BindingFlags;
			}
			else
			{
				bindingFlags2 = runtimeMethodInfo.BindingFlags;
			}
			if ((bindingFlags & bindingFlags2) != bindingFlags2 || (prefixLookup && !FilterApplyPrefixLookup(methodBase, name, (bindingFlags & BindingFlags.IgnoreCase) != 0)))
			{
				return false;
			}
			return FilterApplyMethodBaseInfo(methodBase, bindingFlags, callConv, argumentTypes);
		}

		private static bool FilterApplyMethodBaseInfo(MethodBase methodBase, BindingFlags bindingFlags, CallingConventions callConv, Type[] argumentTypes)
		{
			if ((callConv & CallingConventions.Any) == 0)
			{
				if ((callConv & CallingConventions.VarArgs) != 0 && (methodBase.CallingConvention & CallingConventions.VarArgs) == 0)
				{
					return false;
				}
				if ((callConv & CallingConventions.Standard) != 0 && (methodBase.CallingConvention & CallingConventions.Standard) == 0)
				{
					return false;
				}
			}
			if (argumentTypes != null)
			{
				ParameterInfo[] parametersNoCopy = methodBase.GetParametersNoCopy();
				if (argumentTypes.Length != parametersNoCopy.Length)
				{
					if ((bindingFlags & (BindingFlags.InvokeMethod | BindingFlags.CreateInstance | BindingFlags.GetProperty | BindingFlags.SetProperty)) == 0)
					{
						return false;
					}
					bool flag = false;
					if (argumentTypes.Length > parametersNoCopy.Length)
					{
						if ((methodBase.CallingConvention & CallingConventions.VarArgs) == 0)
						{
							flag = true;
						}
					}
					else if ((bindingFlags & BindingFlags.OptionalParamBinding) == 0)
					{
						flag = true;
					}
					else if (!parametersNoCopy[argumentTypes.Length].IsOptional)
					{
						flag = true;
					}
					if (flag)
					{
						if (parametersNoCopy.Length == 0)
						{
							return false;
						}
						if (argumentTypes.Length < parametersNoCopy.Length - 1)
						{
							return false;
						}
						ParameterInfo parameterInfo = parametersNoCopy[parametersNoCopy.Length - 1];
						if (!parameterInfo.ParameterType.IsArray)
						{
							return false;
						}
						if (!parameterInfo.IsDefined(typeof(ParamArrayAttribute), inherit: false))
						{
							return false;
						}
					}
				}
				else if ((bindingFlags & BindingFlags.ExactBinding) != 0 && (bindingFlags & BindingFlags.InvokeMethod) == 0)
				{
					for (int i = 0; i < parametersNoCopy.Length; i++)
					{
						if (argumentTypes[i] != null && parametersNoCopy[i].ParameterType != argumentTypes[i])
						{
							return false;
						}
					}
				}
			}
			return true;
		}

		private RuntimeType(RuntimeTypeHandle typeHandle)
		{
			m_handle = typeHandle;
		}

		internal RuntimeType()
		{
		}

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		internal override bool CacheEquals(object o)
		{
			return (o as RuntimeType)?.m_handle.Equals(m_handle) ?? false;
		}

		private MethodInfo[] GetMethodCandidates(string name, BindingFlags bindingAttr, CallingConventions callConv, Type[] types, bool allowPrefixLookup)
		{
			FilterHelper(bindingAttr, ref name, allowPrefixLookup, out var prefixLookup, out var ignoreCase, out var listType);
			List<MethodInfo> list = new List<MethodInfo>();
			CerArrayList<RuntimeMethodInfo> methodList = Cache.GetMethodList(listType, name);
			bindingAttr ^= BindingFlags.DeclaredOnly;
			for (int i = 0; i < methodList.Count; i++)
			{
				RuntimeMethodInfo runtimeMethodInfo = methodList[i];
				if ((bindingAttr & runtimeMethodInfo.BindingFlags) == runtimeMethodInfo.BindingFlags && FilterApplyMethodBaseInfo(runtimeMethodInfo, bindingAttr, callConv, types) && (!prefixLookup || FilterApplyPrefixLookup(runtimeMethodInfo, name, ignoreCase)))
				{
					list.Add(runtimeMethodInfo);
				}
			}
			return list.ToArray();
		}

		private ConstructorInfo[] GetConstructorCandidates(string name, BindingFlags bindingAttr, CallingConventions callConv, Type[] types, bool allowPrefixLookup)
		{
			FilterHelper(bindingAttr, ref name, allowPrefixLookup, out var prefixLookup, out var ignoreCase, out var listType);
			List<ConstructorInfo> list = new List<ConstructorInfo>();
			CerArrayList<RuntimeConstructorInfo> constructorList = Cache.GetConstructorList(listType, name);
			bindingAttr ^= BindingFlags.DeclaredOnly;
			for (int i = 0; i < constructorList.Count; i++)
			{
				RuntimeConstructorInfo runtimeConstructorInfo = constructorList[i];
				if ((bindingAttr & runtimeConstructorInfo.BindingFlags) == runtimeConstructorInfo.BindingFlags && FilterApplyMethodBaseInfo(runtimeConstructorInfo, bindingAttr, callConv, types) && (!prefixLookup || FilterApplyPrefixLookup(runtimeConstructorInfo, name, ignoreCase)))
				{
					list.Add(runtimeConstructorInfo);
				}
			}
			return list.ToArray();
		}

		private PropertyInfo[] GetPropertyCandidates(string name, BindingFlags bindingAttr, Type[] types, bool allowPrefixLookup)
		{
			FilterHelper(bindingAttr, ref name, allowPrefixLookup, out var prefixLookup, out var ignoreCase, out var listType);
			List<PropertyInfo> list = new List<PropertyInfo>();
			CerArrayList<RuntimePropertyInfo> propertyList = Cache.GetPropertyList(listType, name);
			bindingAttr ^= BindingFlags.DeclaredOnly;
			for (int i = 0; i < propertyList.Count; i++)
			{
				RuntimePropertyInfo runtimePropertyInfo = propertyList[i];
				if ((bindingAttr & runtimePropertyInfo.BindingFlags) == runtimePropertyInfo.BindingFlags && (!prefixLookup || FilterApplyPrefixLookup(runtimePropertyInfo, name, ignoreCase)) && (types == null || runtimePropertyInfo.GetIndexParameters().Length == types.Length))
				{
					list.Add(runtimePropertyInfo);
				}
			}
			return list.ToArray();
		}

		private EventInfo[] GetEventCandidates(string name, BindingFlags bindingAttr, bool allowPrefixLookup)
		{
			FilterHelper(bindingAttr, ref name, allowPrefixLookup, out var prefixLookup, out var ignoreCase, out var listType);
			List<EventInfo> list = new List<EventInfo>();
			CerArrayList<RuntimeEventInfo> eventList = Cache.GetEventList(listType, name);
			bindingAttr ^= BindingFlags.DeclaredOnly;
			for (int i = 0; i < eventList.Count; i++)
			{
				RuntimeEventInfo runtimeEventInfo = eventList[i];
				if ((bindingAttr & runtimeEventInfo.BindingFlags) == runtimeEventInfo.BindingFlags && (!prefixLookup || FilterApplyPrefixLookup(runtimeEventInfo, name, ignoreCase)))
				{
					list.Add(runtimeEventInfo);
				}
			}
			return list.ToArray();
		}

		private FieldInfo[] GetFieldCandidates(string name, BindingFlags bindingAttr, bool allowPrefixLookup)
		{
			FilterHelper(bindingAttr, ref name, allowPrefixLookup, out var prefixLookup, out var ignoreCase, out var listType);
			List<FieldInfo> list = new List<FieldInfo>();
			CerArrayList<RuntimeFieldInfo> fieldList = Cache.GetFieldList(listType, name);
			bindingAttr ^= BindingFlags.DeclaredOnly;
			for (int i = 0; i < fieldList.Count; i++)
			{
				RuntimeFieldInfo runtimeFieldInfo = fieldList[i];
				if ((bindingAttr & runtimeFieldInfo.BindingFlags) == runtimeFieldInfo.BindingFlags && (!prefixLookup || FilterApplyPrefixLookup(runtimeFieldInfo, name, ignoreCase)))
				{
					list.Add(runtimeFieldInfo);
				}
			}
			return list.ToArray();
		}

		private Type[] GetNestedTypeCandidates(string fullname, BindingFlags bindingAttr, bool allowPrefixLookup)
		{
			bindingAttr &= ~BindingFlags.Static;
			SplitName(fullname, out var name, out var ns);
			FilterHelper(bindingAttr, ref name, allowPrefixLookup, out var prefixLookup, out var _, out var listType);
			List<Type> list = new List<Type>();
			CerArrayList<RuntimeType> nestedTypeList = Cache.GetNestedTypeList(listType, name);
			for (int i = 0; i < nestedTypeList.Count; i++)
			{
				RuntimeType runtimeType = nestedTypeList[i];
				if (FilterApplyType(runtimeType, bindingAttr, name, prefixLookup, ns))
				{
					list.Add(runtimeType);
				}
			}
			return list.ToArray();
		}

		public override MethodInfo[] GetMethods(BindingFlags bindingAttr)
		{
			return GetMethodCandidates(null, bindingAttr, CallingConventions.Any, null, allowPrefixLookup: false);
		}

		[ComVisible(true)]
		public override ConstructorInfo[] GetConstructors(BindingFlags bindingAttr)
		{
			return GetConstructorCandidates(null, bindingAttr, CallingConventions.Any, null, allowPrefixLookup: false);
		}

		public override PropertyInfo[] GetProperties(BindingFlags bindingAttr)
		{
			return GetPropertyCandidates(null, bindingAttr, null, allowPrefixLookup: false);
		}

		public override EventInfo[] GetEvents(BindingFlags bindingAttr)
		{
			return GetEventCandidates(null, bindingAttr, allowPrefixLookup: false);
		}

		public override FieldInfo[] GetFields(BindingFlags bindingAttr)
		{
			return GetFieldCandidates(null, bindingAttr, allowPrefixLookup: false);
		}

		public override Type[] GetInterfaces()
		{
			CerArrayList<RuntimeType> interfaceList = Cache.GetInterfaceList(MemberListType.All, null);
			Type[] array = new Type[interfaceList.Count];
			for (int i = 0; i < interfaceList.Count; i++)
			{
				JitHelpers.UnsafeSetArrayElement(array, i, interfaceList[i]);
			}
			return array;
		}

		public override Type[] GetNestedTypes(BindingFlags bindingAttr)
		{
			return GetNestedTypeCandidates(null, bindingAttr, allowPrefixLookup: false);
		}

		public override MemberInfo[] GetMembers(BindingFlags bindingAttr)
		{
			MethodInfo[] methodCandidates = GetMethodCandidates(null, bindingAttr, CallingConventions.Any, null, allowPrefixLookup: false);
			ConstructorInfo[] constructorCandidates = GetConstructorCandidates(null, bindingAttr, CallingConventions.Any, null, allowPrefixLookup: false);
			PropertyInfo[] propertyCandidates = GetPropertyCandidates(null, bindingAttr, null, allowPrefixLookup: false);
			EventInfo[] eventCandidates = GetEventCandidates(null, bindingAttr, allowPrefixLookup: false);
			FieldInfo[] fieldCandidates = GetFieldCandidates(null, bindingAttr, allowPrefixLookup: false);
			Type[] nestedTypeCandidates = GetNestedTypeCandidates(null, bindingAttr, allowPrefixLookup: false);
			MemberInfo[] array = new MemberInfo[methodCandidates.Length + constructorCandidates.Length + propertyCandidates.Length + eventCandidates.Length + fieldCandidates.Length + nestedTypeCandidates.Length];
			int num = 0;
			Array.Copy(methodCandidates, 0, array, num, methodCandidates.Length);
			num += methodCandidates.Length;
			Array.Copy(constructorCandidates, 0, array, num, constructorCandidates.Length);
			num += constructorCandidates.Length;
			Array.Copy(propertyCandidates, 0, array, num, propertyCandidates.Length);
			num += propertyCandidates.Length;
			Array.Copy(eventCandidates, 0, array, num, eventCandidates.Length);
			num += eventCandidates.Length;
			Array.Copy(fieldCandidates, 0, array, num, fieldCandidates.Length);
			num += fieldCandidates.Length;
			Array.Copy(nestedTypeCandidates, 0, array, num, nestedTypeCandidates.Length);
			num += nestedTypeCandidates.Length;
			return array;
		}

		public override InterfaceMapping GetInterfaceMap(Type ifaceType)
		{
			if (IsGenericParameter)
			{
				throw new InvalidOperationException(Environment.GetResourceString("Arg_GenericParameter"));
			}
			if (ifaceType == null)
			{
				throw new ArgumentNullException("ifaceType");
			}
			if (!(ifaceType is RuntimeType))
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_MustBeRuntimeType"), "ifaceType");
			}
			RuntimeType runtimeType = ifaceType as RuntimeType;
			RuntimeTypeHandle typeHandleInternal = runtimeType.GetTypeHandleInternal();
			int firstSlotForInterface = GetTypeHandleInternal().GetFirstSlotForInterface(runtimeType.GetTypeHandleInternal());
			int interfaceMethodSlots = typeHandleInternal.GetInterfaceMethodSlots();
			int num = 0;
			for (int i = 0; i < interfaceMethodSlots; i++)
			{
				if ((typeHandleInternal.GetMethodAt(i).GetAttributes() & MethodAttributes.Static) != 0)
				{
					num++;
				}
			}
			int num2 = interfaceMethodSlots - num;
			InterfaceMapping result = default(InterfaceMapping);
			result.InterfaceType = ifaceType;
			result.TargetType = this;
			result.InterfaceMethods = new MethodInfo[num2];
			result.TargetMethods = new MethodInfo[num2];
			for (int j = 0; j < interfaceMethodSlots; j++)
			{
				RuntimeMethodHandle runtimeMethodHandle = typeHandleInternal.GetMethodAt(j);
				if ((typeHandleInternal.GetMethodAt(j).GetAttributes() & MethodAttributes.Static) != 0)
				{
					continue;
				}
				if (typeHandleInternal.HasInstantiation() && !typeHandleInternal.IsGenericTypeDefinition())
				{
					runtimeMethodHandle = runtimeMethodHandle.GetInstantiatingStubIfNeeded(typeHandleInternal);
				}
				MethodBase methodBase = GetMethodBase(typeHandleInternal, runtimeMethodHandle);
				result.InterfaceMethods[j] = (MethodInfo)methodBase;
				int num3 = ((firstSlotForInterface != -1) ? (firstSlotForInterface + j) : GetTypeHandleInternal().GetInterfaceMethodImplementationSlot(typeHandleInternal, runtimeMethodHandle));
				if (num3 != -1)
				{
					RuntimeTypeHandle typeHandleInternal2 = GetTypeHandleInternal();
					RuntimeMethodHandle methodHandle = typeHandleInternal2.GetMethodAt(num3);
					if (typeHandleInternal2.HasInstantiation() && !typeHandleInternal2.IsGenericTypeDefinition())
					{
						methodHandle = methodHandle.GetInstantiatingStubIfNeeded(typeHandleInternal2);
					}
					MethodBase methodBase2 = GetMethodBase(typeHandleInternal2, methodHandle);
					result.TargetMethods[j] = (MethodInfo)methodBase2;
				}
			}
			return result;
		}

		protected override MethodInfo GetMethodImpl(string name, BindingFlags bindingAttr, Binder binder, CallingConventions callConv, Type[] types, ParameterModifier[] modifiers)
		{
			MethodInfo[] methodCandidates = GetMethodCandidates(name, bindingAttr, callConv, types, allowPrefixLookup: false);
			if (methodCandidates.Length == 0)
			{
				return null;
			}
			if (types == null || types.Length == 0)
			{
				if (methodCandidates.Length == 1)
				{
					return methodCandidates[0];
				}
				if (types == null)
				{
					for (int i = 1; i < methodCandidates.Length; i++)
					{
						MethodInfo m = methodCandidates[i];
						if (!System.DefaultBinder.CompareMethodSigAndName(m, methodCandidates[0]))
						{
							throw new AmbiguousMatchException(Environment.GetResourceString("RFLCT.Ambiguous"));
						}
					}
					return System.DefaultBinder.FindMostDerivedNewSlotMeth(methodCandidates, methodCandidates.Length) as MethodInfo;
				}
			}
			if (binder == null)
			{
				binder = Type.DefaultBinder;
			}
			return binder.SelectMethod(bindingAttr, methodCandidates, types, modifiers) as MethodInfo;
		}

		protected override ConstructorInfo GetConstructorImpl(BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
		{
			ConstructorInfo[] constructorCandidates = GetConstructorCandidates(null, bindingAttr, CallingConventions.Any, types, allowPrefixLookup: false);
			if (binder == null)
			{
				binder = Type.DefaultBinder;
			}
			if (constructorCandidates.Length == 0)
			{
				return null;
			}
			if (types.Length == 0 && constructorCandidates.Length == 1)
			{
				ParameterInfo[] parametersNoCopy = constructorCandidates[0].GetParametersNoCopy();
				if (parametersNoCopy == null || parametersNoCopy.Length == 0)
				{
					return constructorCandidates[0];
				}
			}
			if ((bindingAttr & BindingFlags.ExactBinding) != 0)
			{
				return System.DefaultBinder.ExactBinding(constructorCandidates, types, modifiers) as ConstructorInfo;
			}
			return binder.SelectMethod(bindingAttr, constructorCandidates, types, modifiers) as ConstructorInfo;
		}

		protected override PropertyInfo GetPropertyImpl(string name, BindingFlags bindingAttr, Binder binder, Type returnType, Type[] types, ParameterModifier[] modifiers)
		{
			if (name == null)
			{
				throw new ArgumentNullException();
			}
			PropertyInfo[] propertyCandidates = GetPropertyCandidates(name, bindingAttr, types, allowPrefixLookup: false);
			if (binder == null)
			{
				binder = Type.DefaultBinder;
			}
			if (propertyCandidates.Length == 0)
			{
				return null;
			}
			if (types == null || types.Length == 0)
			{
				if (propertyCandidates.Length == 1)
				{
					if (returnType != null && returnType != propertyCandidates[0].PropertyType)
					{
						return null;
					}
					return propertyCandidates[0];
				}
				if (returnType == null)
				{
					throw new AmbiguousMatchException(Environment.GetResourceString("RFLCT.Ambiguous"));
				}
			}
			if ((bindingAttr & BindingFlags.ExactBinding) != 0)
			{
				return System.DefaultBinder.ExactPropertyBinding(propertyCandidates, returnType, types, modifiers);
			}
			return binder.SelectProperty(bindingAttr, propertyCandidates, returnType, types, modifiers);
		}

		public override EventInfo GetEvent(string name, BindingFlags bindingAttr)
		{
			if (name == null)
			{
				throw new ArgumentNullException();
			}
			FilterHelper(bindingAttr, ref name, out var _, out var listType);
			CerArrayList<RuntimeEventInfo> eventList = Cache.GetEventList(listType, name);
			EventInfo eventInfo = null;
			bindingAttr ^= BindingFlags.DeclaredOnly;
			for (int i = 0; i < eventList.Count; i++)
			{
				RuntimeEventInfo runtimeEventInfo = eventList[i];
				if ((bindingAttr & runtimeEventInfo.BindingFlags) == runtimeEventInfo.BindingFlags)
				{
					if (eventInfo != null)
					{
						throw new AmbiguousMatchException(Environment.GetResourceString("RFLCT.Ambiguous"));
					}
					eventInfo = runtimeEventInfo;
				}
			}
			return eventInfo;
		}

		public override FieldInfo GetField(string name, BindingFlags bindingAttr)
		{
			if (name == null)
			{
				throw new ArgumentNullException();
			}
			FilterHelper(bindingAttr, ref name, out var _, out var listType);
			CerArrayList<RuntimeFieldInfo> fieldList = Cache.GetFieldList(listType, name);
			FieldInfo fieldInfo = null;
			bindingAttr ^= BindingFlags.DeclaredOnly;
			bool flag = false;
			for (int i = 0; i < fieldList.Count; i++)
			{
				RuntimeFieldInfo runtimeFieldInfo = fieldList[i];
				if ((bindingAttr & runtimeFieldInfo.BindingFlags) != runtimeFieldInfo.BindingFlags)
				{
					continue;
				}
				if (fieldInfo != null)
				{
					if (runtimeFieldInfo.DeclaringType == fieldInfo.DeclaringType)
					{
						throw new AmbiguousMatchException(Environment.GetResourceString("RFLCT.Ambiguous"));
					}
					if (fieldInfo.DeclaringType.IsInterface && runtimeFieldInfo.DeclaringType.IsInterface)
					{
						flag = true;
					}
				}
				if (fieldInfo == null || runtimeFieldInfo.DeclaringType.IsSubclassOf(fieldInfo.DeclaringType) || fieldInfo.DeclaringType.IsInterface)
				{
					fieldInfo = runtimeFieldInfo;
				}
			}
			if (flag && fieldInfo.DeclaringType.IsInterface)
			{
				throw new AmbiguousMatchException(Environment.GetResourceString("RFLCT.Ambiguous"));
			}
			return fieldInfo;
		}

		public override Type GetInterface(string fullname, bool ignoreCase)
		{
			if (fullname == null)
			{
				throw new ArgumentNullException();
			}
			BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic;
			bindingFlags &= ~BindingFlags.Static;
			if (ignoreCase)
			{
				bindingFlags |= BindingFlags.IgnoreCase;
			}
			SplitName(fullname, out var name, out var ns);
			FilterHelper(bindingFlags, ref name, out ignoreCase, out var listType);
			CerArrayList<RuntimeType> interfaceList = Cache.GetInterfaceList(listType, name);
			RuntimeType runtimeType = null;
			for (int i = 0; i < interfaceList.Count; i++)
			{
				RuntimeType runtimeType2 = interfaceList[i];
				if (FilterApplyType(runtimeType2, bindingFlags, name, prefixLookup: false, ns))
				{
					if (runtimeType != null)
					{
						throw new AmbiguousMatchException(Environment.GetResourceString("RFLCT.Ambiguous"));
					}
					runtimeType = runtimeType2;
				}
			}
			return runtimeType;
		}

		public override Type GetNestedType(string fullname, BindingFlags bindingAttr)
		{
			if (fullname == null)
			{
				throw new ArgumentNullException();
			}
			bindingAttr &= ~BindingFlags.Static;
			SplitName(fullname, out var name, out var ns);
			FilterHelper(bindingAttr, ref name, out var _, out var listType);
			CerArrayList<RuntimeType> nestedTypeList = Cache.GetNestedTypeList(listType, name);
			RuntimeType runtimeType = null;
			for (int i = 0; i < nestedTypeList.Count; i++)
			{
				RuntimeType runtimeType2 = nestedTypeList[i];
				if (FilterApplyType(runtimeType2, bindingAttr, name, prefixLookup: false, ns))
				{
					if (runtimeType != null)
					{
						throw new AmbiguousMatchException(Environment.GetResourceString("RFLCT.Ambiguous"));
					}
					runtimeType = runtimeType2;
				}
			}
			return runtimeType;
		}

		public override MemberInfo[] GetMember(string name, MemberTypes type, BindingFlags bindingAttr)
		{
			if (name == null)
			{
				throw new ArgumentNullException();
			}
			MethodInfo[] array = new MethodInfo[0];
			ConstructorInfo[] array2 = new ConstructorInfo[0];
			PropertyInfo[] array3 = new PropertyInfo[0];
			EventInfo[] array4 = new EventInfo[0];
			FieldInfo[] array5 = new FieldInfo[0];
			Type[] array6 = new Type[0];
			if ((type & MemberTypes.Method) != 0)
			{
				array = GetMethodCandidates(name, bindingAttr, CallingConventions.Any, null, allowPrefixLookup: true);
			}
			if ((type & MemberTypes.Constructor) != 0)
			{
				array2 = GetConstructorCandidates(name, bindingAttr, CallingConventions.Any, null, allowPrefixLookup: true);
			}
			if ((type & MemberTypes.Property) != 0)
			{
				array3 = GetPropertyCandidates(name, bindingAttr, null, allowPrefixLookup: true);
			}
			if ((type & MemberTypes.Event) != 0)
			{
				array4 = GetEventCandidates(name, bindingAttr, allowPrefixLookup: true);
			}
			if ((type & MemberTypes.Field) != 0)
			{
				array5 = GetFieldCandidates(name, bindingAttr, allowPrefixLookup: true);
			}
			if ((type & (MemberTypes.TypeInfo | MemberTypes.NestedType)) != 0)
			{
				array6 = GetNestedTypeCandidates(name, bindingAttr, allowPrefixLookup: true);
			}
			switch (type)
			{
			case MemberTypes.Constructor | MemberTypes.Method:
			{
				MethodBase[] array8 = new MethodBase[array.Length + array2.Length];
				Array.Copy(array, array8, array.Length);
				Array.Copy(array2, 0, array8, array.Length, array2.Length);
				return array8;
			}
			case MemberTypes.Method:
				return array;
			case MemberTypes.Constructor:
				return array2;
			case MemberTypes.Field:
				return array5;
			case MemberTypes.Property:
				return array3;
			case MemberTypes.Event:
				return array4;
			case MemberTypes.NestedType:
				return array6;
			case MemberTypes.TypeInfo:
				return array6;
			default:
			{
				MemberInfo[] array7 = new MemberInfo[array.Length + array2.Length + array3.Length + array4.Length + array5.Length + array6.Length];
				int num = 0;
				if (array.Length > 0)
				{
					Array.Copy(array, 0, array7, num, array.Length);
				}
				num += array.Length;
				if (array2.Length > 0)
				{
					Array.Copy(array2, 0, array7, num, array2.Length);
				}
				num += array2.Length;
				if (array3.Length > 0)
				{
					Array.Copy(array3, 0, array7, num, array3.Length);
				}
				num += array3.Length;
				if (array4.Length > 0)
				{
					Array.Copy(array4, 0, array7, num, array4.Length);
				}
				num += array4.Length;
				if (array5.Length > 0)
				{
					Array.Copy(array5, 0, array7, num, array5.Length);
				}
				num += array5.Length;
				if (array6.Length > 0)
				{
					Array.Copy(array6, 0, array7, num, array6.Length);
				}
				num += array6.Length;
				return array7;
			}
			}
		}

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		internal override RuntimeTypeHandle GetTypeHandleInternal()
		{
			return m_handle;
		}

		internal override TypeCode GetTypeCodeInternal()
		{
			TypeCode typeCode = Cache.TypeCode;
			if (typeCode != 0)
			{
				return typeCode;
			}
			typeCode = GetTypeHandleInternal().GetCorElementType() switch
			{
				CorElementType.Boolean => TypeCode.Boolean, 
				CorElementType.Char => TypeCode.Char, 
				CorElementType.I1 => TypeCode.SByte, 
				CorElementType.U1 => TypeCode.Byte, 
				CorElementType.I2 => TypeCode.Int16, 
				CorElementType.U2 => TypeCode.UInt16, 
				CorElementType.I4 => TypeCode.Int32, 
				CorElementType.U4 => TypeCode.UInt32, 
				CorElementType.I8 => TypeCode.Int64, 
				CorElementType.U8 => TypeCode.UInt64, 
				CorElementType.R4 => TypeCode.Single, 
				CorElementType.R8 => TypeCode.Double, 
				CorElementType.String => TypeCode.String, 
				CorElementType.ValueType => (this != Convert.ConvertTypes[15]) ? ((this != Convert.ConvertTypes[16]) ? ((!base.IsEnum) ? TypeCode.Object : Type.GetTypeCode(Enum.GetUnderlyingType(this))) : TypeCode.DateTime) : TypeCode.Decimal, 
				_ => (this != Convert.ConvertTypes[2]) ? ((this != Convert.ConvertTypes[18]) ? TypeCode.Object : TypeCode.String) : TypeCode.DBNull, 
			};
			Cache.TypeCode = typeCode;
			return typeCode;
		}

		public override bool IsInstanceOfType(object o)
		{
			return GetTypeHandleInternal().IsInstanceOfType(o);
		}

		[ComVisible(true)]
		public override bool IsSubclassOf(Type type)
		{
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}
			for (Type baseType = BaseType; baseType != null; baseType = baseType.BaseType)
			{
				if (baseType == type)
				{
					return true;
				}
			}
			if (type == typeof(object) && type != this)
			{
				return true;
			}
			return false;
		}

		protected override TypeAttributes GetAttributeFlagsImpl()
		{
			return m_handle.GetAttributes();
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern void GetGUID(ref Guid result);

		protected override bool IsContextfulImpl()
		{
			return GetTypeHandleInternal().IsContextful();
		}

		protected override bool IsByRefImpl()
		{
			CorElementType corElementType = GetTypeHandleInternal().GetCorElementType();
			return corElementType == CorElementType.ByRef;
		}

		protected override bool IsPrimitiveImpl()
		{
			CorElementType corElementType = GetTypeHandleInternal().GetCorElementType();
			if (((int)corElementType < 2 || (int)corElementType > 13) && corElementType != CorElementType.I)
			{
				return corElementType == CorElementType.U;
			}
			return true;
		}

		protected override bool IsPointerImpl()
		{
			CorElementType corElementType = GetTypeHandleInternal().GetCorElementType();
			return corElementType == CorElementType.Ptr;
		}

		protected override bool IsCOMObjectImpl()
		{
			return GetTypeHandleInternal().IsComObject(isGenericCOM: false);
		}

		internal override bool HasProxyAttributeImpl()
		{
			return GetTypeHandleInternal().HasProxyAttribute();
		}

		protected override bool HasElementTypeImpl()
		{
			if (!base.IsArray && !base.IsPointer)
			{
				return base.IsByRef;
			}
			return true;
		}

		protected override bool IsArrayImpl()
		{
			CorElementType corElementType = GetTypeHandleInternal().GetCorElementType();
			if (corElementType != CorElementType.Array)
			{
				return corElementType == CorElementType.SzArray;
			}
			return true;
		}

		public override int GetArrayRank()
		{
			if (!IsArrayImpl())
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_HasToBeArrayClass"));
			}
			return GetTypeHandleInternal().GetArrayRank();
		}

		public override Type GetElementType()
		{
			return GetTypeHandleInternal().GetElementType().GetRuntimeType();
		}

		public override Type[] GetGenericArguments()
		{
			Type[] array = null;
			RuntimeTypeHandle[] instantiation = GetRootElementType().GetTypeHandleInternal().GetInstantiation();
			if (instantiation != null)
			{
				array = new Type[instantiation.Length];
				for (int i = 0; i < instantiation.Length; i++)
				{
					array[i] = instantiation[i].GetRuntimeType();
				}
			}
			else
			{
				array = new Type[0];
			}
			return array;
		}

		public override Type MakeGenericType(Type[] instantiation)
		{
			if (instantiation == null)
			{
				throw new ArgumentNullException("instantiation");
			}
			Type[] array = new Type[instantiation.Length];
			for (int i = 0; i < instantiation.Length; i++)
			{
				array[i] = instantiation[i];
			}
			instantiation = array;
			if (!IsGenericTypeDefinition)
			{
				throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Arg_NotGenericTypeDefinition"), this));
			}
			for (int j = 0; j < instantiation.Length; j++)
			{
				if (instantiation[j] == null)
				{
					throw new ArgumentNullException();
				}
				if (!(instantiation[j] is RuntimeType))
				{
					return new TypeBuilderInstantiation(this, instantiation);
				}
			}
			Type[] genericArguments = GetGenericArguments();
			SanityCheckGenericArguments(instantiation, genericArguments);
			RuntimeTypeHandle[] array2 = new RuntimeTypeHandle[instantiation.Length];
			for (int k = 0; k < instantiation.Length; k++)
			{
				ref RuntimeTypeHandle reference = ref array2[k];
				reference = instantiation[k].GetTypeHandleInternal();
			}
			Type type = null;
			try
			{
				return m_handle.Instantiate(array2).GetRuntimeType();
			}
			catch (TypeLoadException ex)
			{
				ValidateGenericArguments(this, instantiation, ex);
				throw ex;
			}
		}

		public override Type GetGenericTypeDefinition()
		{
			if (!IsGenericType)
			{
				throw new InvalidOperationException();
			}
			return m_handle.GetGenericTypeDefinition().GetRuntimeType();
		}

		public override Type[] GetGenericParameterConstraints()
		{
			if (!IsGenericParameter)
			{
				throw new InvalidOperationException(Environment.GetResourceString("Arg_NotGenericParameter"));
			}
			RuntimeTypeHandle[] constraints = m_handle.GetConstraints();
			Type[] array = new Type[constraints.Length];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = constraints[i].GetRuntimeType();
			}
			return array;
		}

		public override Type MakePointerType()
		{
			return m_handle.MakePointer().GetRuntimeType();
		}

		public override Type MakeByRefType()
		{
			return m_handle.MakeByRef().GetRuntimeType();
		}

		public override Type MakeArrayType()
		{
			return m_handle.MakeSZArray().GetRuntimeType();
		}

		public override Type MakeArrayType(int rank)
		{
			if (rank <= 0)
			{
				throw new IndexOutOfRangeException();
			}
			return m_handle.MakeArray(rank).GetRuntimeType();
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern bool CanValueSpecialCast(IntPtr valueType, IntPtr targetType);

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern object AllocateObjectForByRef(RuntimeTypeHandle type, object value);

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern bool ForceEnUSLcidComInvoking();

		internal object CheckValue(object value, Binder binder, CultureInfo culture, BindingFlags invokeAttr)
		{
			if (IsInstanceOfType(value))
			{
				return value;
			}
			bool isByRef = base.IsByRef;
			if (isByRef)
			{
				Type elementType = GetElementType();
				if (elementType.IsInstanceOfType(value) || value == null)
				{
					return AllocateObjectForByRef(elementType.TypeHandle, value);
				}
			}
			else
			{
				if (value == null)
				{
					return value;
				}
				if (this == s_typedRef)
				{
					return value;
				}
			}
			bool flag = base.IsPointer || base.IsEnum || base.IsPrimitive;
			if (flag)
			{
				Pointer pointer = value as Pointer;
				Type type = ((pointer == null) ? value.GetType() : pointer.GetPointerType());
				if (CanValueSpecialCast(type.TypeHandle.Value, TypeHandle.Value))
				{
					if (pointer != null)
					{
						return pointer.GetPointerValue();
					}
					return value;
				}
			}
			if ((invokeAttr & BindingFlags.ExactBinding) == BindingFlags.ExactBinding)
			{
				throw new ArgumentException(string.Format(CultureInfo.CurrentUICulture, Environment.GetResourceString("Arg_ObjObjEx"), value.GetType(), this));
			}
			if (binder != null && binder != Type.DefaultBinder)
			{
				value = binder.ChangeType(value, this, culture);
				if (IsInstanceOfType(value))
				{
					return value;
				}
				if (isByRef)
				{
					Type elementType2 = GetElementType();
					if (elementType2.IsInstanceOfType(value) || value == null)
					{
						return AllocateObjectForByRef(elementType2.TypeHandle, value);
					}
				}
				else if (value == null)
				{
					return value;
				}
				if (flag)
				{
					Pointer pointer2 = value as Pointer;
					Type type2 = ((pointer2 == null) ? value.GetType() : pointer2.GetPointerType());
					if (CanValueSpecialCast(type2.TypeHandle.Value, TypeHandle.Value))
					{
						if (pointer2 != null)
						{
							return pointer2.GetPointerValue();
						}
						return value;
					}
				}
			}
			throw new ArgumentException(string.Format(CultureInfo.CurrentUICulture, Environment.GetResourceString("Arg_ObjObjEx"), value.GetType(), this));
		}

		[DebuggerStepThrough]
		[DebuggerHidden]
		public override object InvokeMember(string name, BindingFlags bindingFlags, Binder binder, object target, object[] providedArgs, ParameterModifier[] modifiers, CultureInfo culture, string[] namedParams)
		{
			if (IsGenericParameter)
			{
				throw new InvalidOperationException(Environment.GetResourceString("Arg_GenericParameter"));
			}
			if ((bindingFlags & (BindingFlags.InvokeMethod | BindingFlags.CreateInstance | BindingFlags.GetField | BindingFlags.SetField | BindingFlags.GetProperty | BindingFlags.SetProperty | BindingFlags.PutDispProperty | BindingFlags.PutRefDispProperty)) == 0)
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_NoAccessSpec"), "bindingFlags");
			}
			if ((bindingFlags & (BindingFlags)255) == 0)
			{
				bindingFlags |= BindingFlags.Instance | BindingFlags.Public;
				if ((bindingFlags & BindingFlags.CreateInstance) == 0)
				{
					bindingFlags |= BindingFlags.Static;
				}
			}
			if (namedParams != null)
			{
				if (providedArgs != null)
				{
					if (namedParams.Length > providedArgs.Length)
					{
						throw new ArgumentException(Environment.GetResourceString("Arg_NamedParamTooBig"), "namedParams");
					}
				}
				else if (namedParams.Length != 0)
				{
					throw new ArgumentException(Environment.GetResourceString("Arg_NamedParamTooBig"), "namedParams");
				}
			}
			if (target != null && target.GetType().IsCOMObject)
			{
				if ((bindingFlags & (BindingFlags.InvokeMethod | BindingFlags.GetProperty | BindingFlags.SetProperty | BindingFlags.PutDispProperty | BindingFlags.PutRefDispProperty)) == 0)
				{
					throw new ArgumentException(Environment.GetResourceString("Arg_COMAccess"), "bindingFlags");
				}
				if ((bindingFlags & BindingFlags.GetProperty) != 0 && ((uint)(bindingFlags & (BindingFlags.InvokeMethod | BindingFlags.GetProperty | BindingFlags.SetProperty | BindingFlags.PutDispProperty | BindingFlags.PutRefDispProperty)) & 0xFFFFEEFFu) != 0)
				{
					throw new ArgumentException(Environment.GetResourceString("Arg_PropSetGet"), "bindingFlags");
				}
				if ((bindingFlags & BindingFlags.InvokeMethod) != 0 && ((uint)(bindingFlags & (BindingFlags.InvokeMethod | BindingFlags.GetProperty | BindingFlags.SetProperty | BindingFlags.PutDispProperty | BindingFlags.PutRefDispProperty)) & 0xFFFFEEFFu) != 0)
				{
					throw new ArgumentException(Environment.GetResourceString("Arg_PropSetInvoke"), "bindingFlags");
				}
				if ((bindingFlags & BindingFlags.SetProperty) != 0 && ((uint)(bindingFlags & (BindingFlags.InvokeMethod | BindingFlags.GetProperty | BindingFlags.SetProperty | BindingFlags.PutDispProperty | BindingFlags.PutRefDispProperty)) & 0xFFFFDFFFu) != 0)
				{
					throw new ArgumentException(Environment.GetResourceString("Arg_COMPropSetPut"), "bindingFlags");
				}
				if ((bindingFlags & BindingFlags.PutDispProperty) != 0 && ((uint)(bindingFlags & (BindingFlags.InvokeMethod | BindingFlags.GetProperty | BindingFlags.SetProperty | BindingFlags.PutDispProperty | BindingFlags.PutRefDispProperty)) & 0xFFFFBFFFu) != 0)
				{
					throw new ArgumentException(Environment.GetResourceString("Arg_COMPropSetPut"), "bindingFlags");
				}
				if ((bindingFlags & BindingFlags.PutRefDispProperty) != 0 && ((uint)(bindingFlags & (BindingFlags.InvokeMethod | BindingFlags.GetProperty | BindingFlags.SetProperty | BindingFlags.PutDispProperty | BindingFlags.PutRefDispProperty)) & 0xFFFF7FFFu) != 0)
				{
					throw new ArgumentException(Environment.GetResourceString("Arg_COMPropSetPut"), "bindingFlags");
				}
				if (!RemotingServices.IsTransparentProxy(target))
				{
					if (name == null)
					{
						throw new ArgumentNullException("name");
					}
					bool[] byrefModifiers = modifiers?[0].IsByRefArray;
					return InvokeDispMethod(culture: culture?.LCID ?? (forceInvokingWithEnUS ? 1033 : Thread.CurrentThread.CurrentCulture.LCID), name: name, invokeAttr: bindingFlags, target: target, args: providedArgs, byrefModifiers: byrefModifiers, namedParameters: namedParams);
				}
				return ((MarshalByRefObject)target).InvokeMember(name, bindingFlags, binder, providedArgs, modifiers, culture, namedParams);
			}
			if (namedParams != null && Array.IndexOf(namedParams, null) != -1)
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_NamedParamNull"), "namedParams");
			}
			int num = ((providedArgs != null) ? providedArgs.Length : 0);
			if (binder == null)
			{
				binder = Type.DefaultBinder;
			}
			_ = Type.DefaultBinder;
			if ((bindingFlags & BindingFlags.CreateInstance) != 0)
			{
				if ((bindingFlags & BindingFlags.CreateInstance) != 0 && (bindingFlags & (BindingFlags.InvokeMethod | BindingFlags.GetField | BindingFlags.SetField | BindingFlags.GetProperty | BindingFlags.SetProperty)) != 0)
				{
					throw new ArgumentException(Environment.GetResourceString("Arg_CreatInstAccess"), "bindingFlags");
				}
				return Activator.CreateInstance(this, bindingFlags, binder, providedArgs, culture);
			}
			if ((bindingFlags & (BindingFlags.PutDispProperty | BindingFlags.PutRefDispProperty)) != 0)
			{
				bindingFlags |= BindingFlags.SetProperty;
			}
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			if (name.Length == 0 || name.Equals("[DISPID=0]"))
			{
				name = GetDefaultMemberName();
				if (name == null)
				{
					name = "ToString";
				}
			}
			bool flag = (bindingFlags & BindingFlags.GetField) != 0;
			bool flag2 = (bindingFlags & BindingFlags.SetField) != 0;
			if (flag || flag2)
			{
				if (flag)
				{
					if (flag2)
					{
						throw new ArgumentException(Environment.GetResourceString("Arg_FldSetGet"), "bindingFlags");
					}
					if ((bindingFlags & BindingFlags.SetProperty) != 0)
					{
						throw new ArgumentException(Environment.GetResourceString("Arg_FldGetPropSet"), "bindingFlags");
					}
				}
				else
				{
					if (providedArgs == null)
					{
						throw new ArgumentNullException("providedArgs");
					}
					if ((bindingFlags & BindingFlags.GetProperty) != 0)
					{
						throw new ArgumentException(Environment.GetResourceString("Arg_FldSetPropGet"), "bindingFlags");
					}
					if ((bindingFlags & BindingFlags.InvokeMethod) != 0)
					{
						throw new ArgumentException(Environment.GetResourceString("Arg_FldSetInvoke"), "bindingFlags");
					}
				}
				FieldInfo fieldInfo = null;
				FieldInfo[] array = GetMember(name, MemberTypes.Field, bindingFlags) as FieldInfo[];
				if (array.Length == 1)
				{
					fieldInfo = array[0];
				}
				else if (array.Length > 0)
				{
					fieldInfo = binder.BindToField(bindingFlags, array, flag ? Empty.Value : providedArgs[0], culture);
				}
				if (fieldInfo != null)
				{
					if (fieldInfo.FieldType.IsArray || fieldInfo.FieldType == typeof(Array))
					{
						int num2 = (((bindingFlags & BindingFlags.GetField) == 0) ? (num - 1) : num);
						if (num2 > 0)
						{
							int[] array2 = new int[num2];
							for (int i = 0; i < num2; i++)
							{
								try
								{
									array2[i] = ((IConvertible)providedArgs[i]).ToInt32(null);
								}
								catch (InvalidCastException)
								{
									throw new ArgumentException(Environment.GetResourceString("Arg_IndexMustBeInt"));
								}
							}
							Array array3 = (Array)fieldInfo.GetValue(target);
							if ((bindingFlags & BindingFlags.GetField) != 0)
							{
								return array3.GetValue(array2);
							}
							array3.SetValue(providedArgs[num2], array2);
							return null;
						}
					}
					if (flag)
					{
						if (num != 0)
						{
							throw new ArgumentException(Environment.GetResourceString("Arg_FldGetArgErr"), "bindingFlags");
						}
						return fieldInfo.GetValue(target);
					}
					if (num != 1)
					{
						throw new ArgumentException(Environment.GetResourceString("Arg_FldSetArgErr"), "bindingFlags");
					}
					fieldInfo.SetValue(target, providedArgs[0], bindingFlags, binder, culture);
					return null;
				}
				if ((bindingFlags & (BindingFlags)16773888) == 0)
				{
					throw new MissingFieldException(FullName, name);
				}
			}
			bool flag3 = (bindingFlags & BindingFlags.GetProperty) != 0;
			bool flag4 = (bindingFlags & BindingFlags.SetProperty) != 0;
			if (flag3 || flag4)
			{
				if (flag3)
				{
					if (flag4)
					{
						throw new ArgumentException(Environment.GetResourceString("Arg_PropSetGet"), "bindingFlags");
					}
				}
				else if ((bindingFlags & BindingFlags.InvokeMethod) != 0)
				{
					throw new ArgumentException(Environment.GetResourceString("Arg_PropSetInvoke"), "bindingFlags");
				}
			}
			MethodInfo[] array4 = null;
			MethodInfo methodInfo = null;
			if ((bindingFlags & BindingFlags.InvokeMethod) != 0)
			{
				MethodInfo[] array5 = GetMember(name, MemberTypes.Method, bindingFlags) as MethodInfo[];
				ArrayList arrayList = null;
				foreach (MethodInfo methodInfo2 in array5)
				{
					if (!FilterApplyMethodBaseInfo(methodInfo2, bindingFlags, null, CallingConventions.Any, new Type[num], prefixLookup: false))
					{
						continue;
					}
					if (methodInfo == null)
					{
						methodInfo = methodInfo2;
						continue;
					}
					if (arrayList == null)
					{
						arrayList = new ArrayList(array5.Length);
						arrayList.Add(methodInfo);
					}
					arrayList.Add(methodInfo2);
				}
				if (arrayList != null)
				{
					array4 = new MethodInfo[arrayList.Count];
					arrayList.CopyTo(array4);
				}
			}
			if ((methodInfo == null && flag3) || flag4)
			{
				PropertyInfo[] array6 = GetMember(name, MemberTypes.Property, bindingFlags) as PropertyInfo[];
				ArrayList arrayList2 = null;
				for (int k = 0; k < array6.Length; k++)
				{
					MethodInfo methodInfo3 = null;
					methodInfo3 = ((!flag4) ? array6[k].GetGetMethod(nonPublic: true) : array6[k].GetSetMethod(nonPublic: true));
					if (methodInfo3 == null || !FilterApplyMethodBaseInfo(methodInfo3, bindingFlags, null, CallingConventions.Any, new Type[num], prefixLookup: false))
					{
						continue;
					}
					if (methodInfo == null)
					{
						methodInfo = methodInfo3;
						continue;
					}
					if (arrayList2 == null)
					{
						arrayList2 = new ArrayList(array6.Length);
						arrayList2.Add(methodInfo);
					}
					arrayList2.Add(methodInfo3);
				}
				if (arrayList2 != null)
				{
					array4 = new MethodInfo[arrayList2.Count];
					arrayList2.CopyTo(array4);
				}
			}
			if (methodInfo != null)
			{
				if (array4 == null && num == 0 && methodInfo.GetParametersNoCopy().Length == 0 && (bindingFlags & BindingFlags.OptionalParamBinding) == 0)
				{
					return methodInfo.Invoke(target, bindingFlags, binder, providedArgs, culture);
				}
				if (array4 == null)
				{
					array4 = new MethodInfo[1]
					{
						methodInfo
					};
				}
				if (providedArgs == null)
				{
					providedArgs = new object[0];
				}
				object state = null;
				MethodBase methodBase = null;
				try
				{
					methodBase = binder.BindToMethod(bindingFlags, array4, ref providedArgs, modifiers, culture, namedParams, out state);
				}
				catch (MissingMethodException)
				{
				}
				if (methodBase == null)
				{
					throw new MissingMethodException(FullName, name);
				}
				object result = ((MethodInfo)methodBase).Invoke(target, bindingFlags, binder, providedArgs, culture);
				if (state != null)
				{
					binder.ReorderArgumentArray(ref providedArgs, state);
				}
				return result;
			}
			throw new MissingMethodException(FullName, name);
		}

		public override bool Equals(object obj)
		{
			return obj == this;
		}

		public override int GetHashCode()
		{
			return (int)GetTypeHandleInternal().Value;
		}

		public override string ToString()
		{
			return Cache.GetToString();
		}

		public object Clone()
		{
			return this;
		}

		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			UnitySerializationHolder.GetUnitySerializationInfo(info, this);
		}

		public override object[] GetCustomAttributes(bool inherit)
		{
			return CustomAttribute.GetCustomAttributes(this, typeof(object) as RuntimeType, inherit);
		}

		public override object[] GetCustomAttributes(Type attributeType, bool inherit)
		{
			if (attributeType == null)
			{
				throw new ArgumentNullException("attributeType");
			}
			RuntimeType runtimeType = attributeType.UnderlyingSystemType as RuntimeType;
			if (runtimeType == null)
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_MustBeType"), "attributeType");
			}
			return CustomAttribute.GetCustomAttributes(this, runtimeType, inherit);
		}

		public override bool IsDefined(Type attributeType, bool inherit)
		{
			if (attributeType == null)
			{
				throw new ArgumentNullException("attributeType");
			}
			RuntimeType runtimeType = attributeType.UnderlyingSystemType as RuntimeType;
			if (runtimeType == null)
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_MustBeType"), "attributeType");
			}
			return CustomAttribute.IsDefined(this, runtimeType, inherit);
		}

		internal void CreateInstanceCheckThis()
		{
			if (this is ReflectionOnlyType)
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_ReflectionOnlyInvoke"));
			}
			if (ContainsGenericParameters)
			{
				throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Acc_CreateGenericEx"), this));
			}
			Type rootElementType = GetRootElementType();
			if (rootElementType == typeof(ArgIterator))
			{
				throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Acc_CreateArgIterator")));
			}
			if (rootElementType == typeof(void))
			{
				throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Acc_CreateVoid")));
			}
		}

		internal object CreateInstanceImpl(BindingFlags bindingAttr, Binder binder, object[] args, CultureInfo culture, object[] activationAttributes)
		{
			CreateInstanceCheckThis();
			object obj = null;
			try
			{
				try
				{
					if (activationAttributes != null)
					{
						ActivationServices.PushActivationAttributes(this, activationAttributes);
					}
					if (args == null)
					{
						args = new object[0];
					}
					int num = args.Length;
					if (binder == null)
					{
						binder = Type.DefaultBinder;
					}
					if (num == 0 && (bindingAttr & BindingFlags.Public) != 0 && (bindingAttr & BindingFlags.Instance) != 0 && (IsGenericCOMObjectImpl() || IsSubclassOf(typeof(ValueType))))
					{
						return CreateInstanceImpl(((bindingAttr & BindingFlags.NonPublic) == 0) ? true : false);
					}
					MethodBase[] constructors = GetConstructors(bindingAttr);
					ArrayList arrayList = new ArrayList(constructors.Length);
					Type[] array = new Type[num];
					for (int i = 0; i < num; i++)
					{
						if (args[i] != null)
						{
							array[i] = args[i].GetType();
						}
					}
					for (int j = 0; j < constructors.Length; j++)
					{
						_ = constructors[j];
						if (FilterApplyMethodBaseInfo(constructors[j], bindingAttr, null, CallingConventions.Any, array, prefixLookup: false))
						{
							arrayList.Add(constructors[j]);
						}
					}
					MethodBase[] array2 = new MethodBase[arrayList.Count];
					arrayList.CopyTo(array2);
					if (array2 != null && array2.Length == 0)
					{
						array2 = null;
					}
					if (array2 == null)
					{
						if (activationAttributes != null)
						{
							ActivationServices.PopActivationAttributes(this);
							activationAttributes = null;
						}
						throw new MissingMethodException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("MissingConstructor_Name"), FullName));
					}
					if (num == 0 && array2.Length == 1 && (bindingAttr & BindingFlags.OptionalParamBinding) == 0)
					{
						return Activator.CreateInstance(this, nonPublic: true);
					}
					object state = null;
					MethodBase methodBase;
					try
					{
						methodBase = binder.BindToMethod(bindingAttr, array2, ref args, null, culture, null, out state);
					}
					catch (MissingMethodException)
					{
						methodBase = null;
					}
					if (methodBase == null)
					{
						if (activationAttributes != null)
						{
							ActivationServices.PopActivationAttributes(this);
							activationAttributes = null;
						}
						throw new MissingMethodException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("MissingConstructor_Name"), FullName));
					}
					if (typeof(Delegate).IsAssignableFrom(methodBase.DeclaringType))
					{
						new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Demand();
					}
					obj = ((ConstructorInfo)methodBase).Invoke(bindingAttr, binder, args, culture);
					if (state != null)
					{
						binder.ReorderArgumentArray(ref args, state);
						return obj;
					}
					return obj;
				}
				finally
				{
					if (activationAttributes != null)
					{
						ActivationServices.PopActivationAttributes(this);
						activationAttributes = null;
					}
				}
			}
			catch (Exception)
			{
				throw;
			}
		}

		private object CreateInstanceSlow(bool publicOnly, bool fillCache)
		{
			RuntimeMethodHandle ctor = RuntimeMethodHandle.EmptyHandle;
			bool bNeedSecurityCheck = true;
			bool canBeCached = false;
			bool noCheck = false;
			CreateInstanceCheckThis();
			if (!fillCache)
			{
				noCheck = true;
			}
			object result = RuntimeTypeHandle.CreateInstance(this, publicOnly, noCheck, ref canBeCached, ref ctor, ref bNeedSecurityCheck);
			if (canBeCached && fillCache)
			{
				ActivatorCache activatorCache = s_ActivatorCache;
				if (activatorCache == null)
				{
					activatorCache = new ActivatorCache();
					Thread.MemoryBarrier();
					s_ActivatorCache = activatorCache;
				}
				ActivatorCacheEntry entry = new ActivatorCacheEntry(this, ctor, bNeedSecurityCheck);
				Thread.MemoryBarrier();
				activatorCache.SetEntry(entry);
			}
			return result;
		}

		[DebuggerStepThrough]
		[DebuggerHidden]
		internal object CreateInstanceImpl(bool publicOnly)
		{
			return CreateInstanceImpl(publicOnly, skipVisibilityChecks: false, fillCache: true);
		}

		[DebuggerStepThrough]
		[DebuggerHidden]
		internal object CreateInstanceImpl(bool publicOnly, bool skipVisibilityChecks, bool fillCache)
		{
			RuntimeTypeHandle typeHandle = TypeHandle;
			ActivatorCache activatorCache = s_ActivatorCache;
			if (activatorCache != null)
			{
				ActivatorCacheEntry entry = activatorCache.GetEntry(this);
				if (entry != null)
				{
					if (publicOnly && entry.m_ctor != null && (entry.m_hCtorMethodHandle.GetAttributes() & MethodAttributes.MemberAccessMask) != MethodAttributes.Public)
					{
						throw new MissingMethodException(Environment.GetResourceString("Arg_NoDefCTor"));
					}
					object obj = typeHandle.Allocate();
					if (entry.m_ctor != null)
					{
						if (!skipVisibilityChecks && entry.m_bNeedSecurityCheck)
						{
							MethodBase.PerformSecurityCheck(obj, entry.m_hCtorMethodHandle, TypeHandle.Value, 268435456u);
						}
						try
						{
							entry.m_ctor(obj);
							return obj;
						}
						catch (Exception inner)
						{
							throw new TargetInvocationException(inner);
						}
					}
					return obj;
				}
			}
			return CreateInstanceSlow(publicOnly, fillCache);
		}

		internal bool SupportsInterface(object o)
		{
			return TypeHandle.SupportsInterface(o);
		}

		internal void InvalidateCachedNestedType()
		{
			Cache.InvalidateCachedNestedType();
		}

		internal bool IsGenericCOMObjectImpl()
		{
			return m_handle.IsComObject(isGenericCOM: true);
		}

		internal static bool CanCastTo(RuntimeType fromType, RuntimeType toType)
		{
			return fromType.GetTypeHandleInternal().CanCastTo(toType.GetTypeHandleInternal());
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern object _CreateEnum(IntPtr enumType, long value);

		internal static object CreateEnum(RuntimeTypeHandle enumType, long value)
		{
			return _CreateEnum(enumType.Value, value);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern object InvokeDispMethod(string name, BindingFlags invokeAttr, object target, object[] args, bool[] byrefModifiers, int culture, string[] namedParameters);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern Type GetTypeFromProgIDImpl(string progID, string server, bool throwOnError);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern Type GetTypeFromCLSIDImpl(Guid clsid, string server, bool throwOnError);

		internal static Type PrivateGetType(string typeName, bool throwOnError, bool ignoreCase, ref StackCrawlMark stackMark)
		{
			return PrivateGetType(typeName, throwOnError, ignoreCase, reflectionOnly: false, ref stackMark);
		}

		internal static Type PrivateGetType(string typeName, bool throwOnError, bool ignoreCase, bool reflectionOnly, ref StackCrawlMark stackMark)
		{
			if (typeName == null)
			{
				throw new ArgumentNullException("TypeName");
			}
			return RuntimeTypeHandle.GetTypeByName(typeName, throwOnError, ignoreCase, reflectionOnly, ref stackMark).GetRuntimeType();
		}

		private object ForwardCallToInvokeMember(string memberName, BindingFlags flags, object target, int[] aWrapperTypes, ref MessageData msgData)
		{
			ParameterModifier[] array = null;
			object obj = null;
			Message message = new Message();
			message.InitFields(msgData);
			MethodInfo methodInfo = (MethodInfo)message.GetMethodBase();
			object[] args = message.Args;
			int num = args.Length;
			ParameterInfo[] parametersNoCopy = methodInfo.GetParametersNoCopy();
			if (num > 0)
			{
				ParameterModifier parameterModifier = new ParameterModifier(num);
				for (int i = 0; i < num; i++)
				{
					if (parametersNoCopy[i].ParameterType.IsByRef)
					{
						parameterModifier[i] = true;
					}
				}
				array = new ParameterModifier[1]
				{
					parameterModifier
				};
				if (aWrapperTypes != null)
				{
					WrapArgsForInvokeCall(args, aWrapperTypes);
				}
			}
			if (methodInfo.ReturnType == typeof(void))
			{
				flags |= BindingFlags.IgnoreReturn;
			}
			try
			{
				obj = InvokeMember(memberName, flags, null, target, args, array, null, null);
			}
			catch (TargetInvocationException ex)
			{
				throw ex.InnerException;
			}
			for (int j = 0; j < num; j++)
			{
				if (array[0][j] && args[j] != null)
				{
					Type elementType = parametersNoCopy[j].ParameterType.GetElementType();
					if (elementType != args[j].GetType())
					{
						args[j] = ForwardCallBinder.ChangeType(args[j], elementType, null);
					}
				}
			}
			if (obj != null)
			{
				Type returnType = methodInfo.ReturnType;
				if (returnType != obj.GetType())
				{
					obj = ForwardCallBinder.ChangeType(obj, returnType, null);
				}
			}
			RealProxy.PropagateOutParameters(message, args, obj);
			return obj;
		}

		private void WrapArgsForInvokeCall(object[] aArgs, int[] aWrapperTypes)
		{
			int num = aArgs.Length;
			for (int i = 0; i < num; i++)
			{
				if (aWrapperTypes[i] == 0)
				{
					continue;
				}
				if (((uint)aWrapperTypes[i] & 0x10000u) != 0)
				{
					Type type = null;
					bool flag = false;
					switch (aWrapperTypes[i] & -65537)
					{
					case 1:
						type = typeof(UnknownWrapper);
						break;
					case 2:
						type = typeof(DispatchWrapper);
						break;
					case 8:
						type = typeof(ErrorWrapper);
						break;
					case 16:
						type = typeof(CurrencyWrapper);
						break;
					case 32:
						type = typeof(BStrWrapper);
						flag = true;
						break;
					}
					Array array = (Array)aArgs[i];
					int length = array.Length;
					object[] array2 = (object[])Array.CreateInstance(type, length);
					ConstructorInfo constructorInfo = ((!flag) ? type.GetConstructor(new Type[1]
					{
						typeof(object)
					}) : type.GetConstructor(new Type[1]
					{
						typeof(string)
					}));
					for (int j = 0; j < length; j++)
					{
						if (flag)
						{
							array2[j] = constructorInfo.Invoke(new object[1]
							{
								(string)array.GetValue(j)
							});
						}
						else
						{
							array2[j] = constructorInfo.Invoke(new object[1]
							{
								array.GetValue(j)
							});
						}
					}
					aArgs[i] = array2;
				}
				else
				{
					switch (aWrapperTypes[i])
					{
					case 1:
						aArgs[i] = new UnknownWrapper(aArgs[i]);
						break;
					case 2:
						aArgs[i] = new DispatchWrapper(aArgs[i]);
						break;
					case 8:
						aArgs[i] = new ErrorWrapper(aArgs[i]);
						break;
					case 16:
						aArgs[i] = new CurrencyWrapper(aArgs[i]);
						break;
					case 32:
						aArgs[i] = new BStrWrapper((string)aArgs[i]);
						break;
					}
				}
			}
		}
	}
}
