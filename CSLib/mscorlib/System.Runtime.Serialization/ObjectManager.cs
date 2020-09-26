using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Reflection.Cache;
using System.Runtime.InteropServices;
using System.Runtime.Remoting;
using System.Security;
using System.Security.Permissions;
using System.Security.Principal;
using System.Text;

namespace System.Runtime.Serialization
{
	[ComVisible(true)]
	public class ObjectManager
	{
		private const int DefaultInitialSize = 16;

		private const int MaxArraySize = 4096;

		private const int ArrayMask = 4095;

		private const int MaxReferenceDepth = 100;

		private DeserializationEventHandler m_onDeserializationHandler;

		private SerializationEventHandler m_onDeserializedHandler;

		private static Type[] SIConstructorTypes;

		private static Type TypeOfWindowsIdentity;

		private static Type[] SIWindowsIdentityConstructorTypes;

		internal ObjectHolder[] m_objects;

		internal object m_topObject;

		internal ObjectHolderList m_specialFixupObjects;

		internal long m_fixupCount;

		internal ISurrogateSelector m_selector;

		internal StreamingContext m_context;

		private bool m_isCrossAppDomain;

		internal object TopObject
		{
			get
			{
				return m_topObject;
			}
			set
			{
				m_topObject = value;
			}
		}

		internal ObjectHolderList SpecialFixupObjects
		{
			get
			{
				if (m_specialFixupObjects == null)
				{
					m_specialFixupObjects = new ObjectHolderList();
				}
				return m_specialFixupObjects;
			}
		}

		public ObjectManager(ISurrogateSelector selector, StreamingContext context)
			: this(selector, context, checkSecurity: true, isCrossAppDomain: false)
		{
		}

		internal ObjectManager(ISurrogateSelector selector, StreamingContext context, bool checkSecurity, bool isCrossAppDomain)
		{
			if (checkSecurity)
			{
				CodeAccessPermission.DemandInternal(PermissionType.SecuritySerialization);
			}
			m_objects = new ObjectHolder[16];
			m_selector = selector;
			m_context = context;
			m_isCrossAppDomain = isCrossAppDomain;
		}

		private bool CanCallGetType(object obj)
		{
			if (RemotingServices.IsTransparentProxy(obj))
			{
				return false;
			}
			return true;
		}

		static ObjectManager()
		{
			SIConstructorTypes = new Type[2];
			SIConstructorTypes[0] = typeof(SerializationInfo);
			SIConstructorTypes[1] = typeof(StreamingContext);
			TypeOfWindowsIdentity = typeof(WindowsIdentity);
			SIWindowsIdentityConstructorTypes = new Type[1];
			SIWindowsIdentityConstructorTypes[0] = typeof(SerializationInfo);
		}

		internal ObjectHolder FindObjectHolder(long objectID)
		{
			int num = (int)(objectID & 0xFFF);
			if (num >= m_objects.Length)
			{
				return null;
			}
			ObjectHolder objectHolder;
			for (objectHolder = m_objects[num]; objectHolder != null; objectHolder = objectHolder.m_next)
			{
				if (objectHolder.m_id == objectID)
				{
					return objectHolder;
				}
			}
			return objectHolder;
		}

		internal ObjectHolder FindOrCreateObjectHolder(long objectID)
		{
			ObjectHolder objectHolder = FindObjectHolder(objectID);
			if (objectHolder == null)
			{
				objectHolder = new ObjectHolder(objectID);
				AddObjectHolder(objectHolder);
			}
			return objectHolder;
		}

		private void AddObjectHolder(ObjectHolder holder)
		{
			if (holder.m_id >= m_objects.Length && m_objects.Length != 4096)
			{
				int num = 4096;
				if (holder.m_id < 2048)
				{
					num = m_objects.Length * 2;
					while (num <= holder.m_id && num < 4096)
					{
						num *= 2;
					}
					if (num > 4096)
					{
						num = 4096;
					}
				}
				ObjectHolder[] array = new ObjectHolder[num];
				Array.Copy(m_objects, array, m_objects.Length);
				m_objects = array;
			}
			int num2 = (int)(holder.m_id & 0xFFF);
			ObjectHolder objectHolder = (holder.m_next = m_objects[num2]);
			m_objects[num2] = holder;
		}

		private bool GetCompletionInfo(FixupHolder fixup, out ObjectHolder holder, out object member, bool bThrowIfMissing)
		{
			member = fixup.m_fixupInfo;
			holder = FindObjectHolder(fixup.m_id);
			if (!holder.CompletelyFixed && holder.ObjectValue != null && holder.ObjectValue is ValueType)
			{
				SpecialFixupObjects.Add(holder);
				return false;
			}
			if (holder == null || holder.CanObjectValueChange || holder.ObjectValue == null)
			{
				if (bThrowIfMissing)
				{
					if (holder == null)
					{
						throw new SerializationException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Serialization_NeverSeen"), fixup.m_id));
					}
					if (holder.IsIncompleteObjectReference)
					{
						throw new SerializationException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Serialization_IORIncomplete"), fixup.m_id));
					}
					throw new SerializationException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Serialization_ObjectNotSupplied"), fixup.m_id));
				}
				return false;
			}
			return true;
		}

		private void FixupSpecialObject(ObjectHolder holder)
		{
			ISurrogateSelector selector = null;
			if (holder.HasSurrogate)
			{
				ISerializationSurrogate surrogate = holder.Surrogate;
				object obj = surrogate.SetObjectData(holder.ObjectValue, holder.SerializationInfo, m_context, selector);
				if (obj != null)
				{
					if (!holder.CanSurrogatedObjectValueChange && obj != holder.ObjectValue)
					{
						throw new SerializationException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Serialization_NotCyclicallyReferenceableSurrogate"), surrogate.GetType().FullName));
					}
					holder.SetObjectValue(obj, this);
				}
				holder.m_surrogate = null;
				holder.SetFlags();
			}
			else
			{
				CompleteISerializableObject(holder.ObjectValue, holder.SerializationInfo, m_context);
			}
			holder.SerializationInfo = null;
			holder.RequiresSerInfoFixup = false;
			if (holder.RequiresValueTypeFixup && holder.ValueTypeFixupPerformed)
			{
				DoValueTypeFixup(null, holder, holder.ObjectValue);
			}
			DoNewlyRegisteredObjectFixups(holder);
		}

		private bool ResolveObjectReference(ObjectHolder holder)
		{
			int num = 0;
			try
			{
				object objectValue;
				do
				{
					objectValue = holder.ObjectValue;
					holder.SetObjectValue(((IObjectReference)holder.ObjectValue).GetRealObject(m_context), this);
					if (holder.ObjectValue == null)
					{
						holder.SetObjectValue(objectValue, this);
						return false;
					}
					if (num++ == 100)
					{
						throw new SerializationException(Environment.GetResourceString("Serialization_TooManyReferences"));
					}
				}
				while (holder.ObjectValue is IObjectReference && objectValue != holder.ObjectValue);
			}
			catch (NullReferenceException)
			{
				return false;
			}
			holder.IsIncompleteObjectReference = false;
			DoNewlyRegisteredObjectFixups(holder);
			return true;
		}

		private bool DoValueTypeFixup(FieldInfo memberToFix, ObjectHolder holder, object value)
		{
			FieldInfo[] array = new FieldInfo[4];
			FieldInfo[] array2 = null;
			int num = 0;
			int[] array3 = null;
			ValueTypeFixupInfo valueTypeFixupInfo = null;
			object objectValue = holder.ObjectValue;
			while (holder.RequiresValueTypeFixup)
			{
				if (num + 1 >= array.Length)
				{
					FieldInfo[] array4 = new FieldInfo[array.Length * 2];
					Array.Copy(array, array4, array.Length);
					array = array4;
				}
				valueTypeFixupInfo = holder.ValueFixup;
				objectValue = holder.ObjectValue;
				if (valueTypeFixupInfo.ParentField != null)
				{
					FieldInfo parentField = valueTypeFixupInfo.ParentField;
					ObjectHolder objectHolder = FindObjectHolder(valueTypeFixupInfo.ContainerID);
					if (objectHolder.ObjectValue == null)
					{
						break;
					}
					if (Nullable.GetUnderlyingType(parentField.FieldType) != null)
					{
						array[num] = parentField.FieldType.GetField("value", BindingFlags.Instance | BindingFlags.NonPublic);
						num++;
					}
					array[num] = parentField;
					holder = objectHolder;
					num++;
					continue;
				}
				holder = FindObjectHolder(valueTypeFixupInfo.ContainerID);
				array3 = valueTypeFixupInfo.ParentIndex;
				if (holder.ObjectValue != null)
				{
				}
				break;
			}
			if (!(holder.ObjectValue is Array) && holder.ObjectValue != null)
			{
				objectValue = holder.ObjectValue;
			}
			if (num != 0)
			{
				array2 = new FieldInfo[num];
				for (int i = 0; i < num; i++)
				{
					FieldInfo fieldInfo = array[num - 1 - i];
					SerializationFieldInfo serializationFieldInfo = fieldInfo as SerializationFieldInfo;
					array2[i] = ((serializationFieldInfo == null) ? fieldInfo : serializationFieldInfo.FieldInfo);
				}
				TypedReference typedReference = TypedReference.MakeTypedReference(objectValue, array2);
				if (memberToFix != null)
				{
					((RuntimeFieldInfo)memberToFix).SetValueDirect(typedReference, value);
				}
				else
				{
					TypedReference.SetTypedReference(typedReference, value);
				}
			}
			else if (memberToFix != null)
			{
				FormatterServices.SerializationSetValue(memberToFix, objectValue, value);
			}
			if (array3 != null && holder.ObjectValue != null)
			{
				((Array)holder.ObjectValue).SetValue(objectValue, array3);
			}
			return true;
		}

		[Conditional("SER_LOGGING")]
		private void DumpValueTypeFixup(object obj, FieldInfo[] intermediateFields, FieldInfo memberToFix, object value)
		{
			StringBuilder stringBuilder = new StringBuilder("  " + obj);
			if (intermediateFields != null)
			{
				for (int i = 0; i < intermediateFields.Length; i++)
				{
					stringBuilder.Append("." + intermediateFields[i].Name);
				}
			}
			stringBuilder.Append("." + memberToFix.Name + "=" + value);
		}

		internal void CompleteObject(ObjectHolder holder, bool bObjectFullyComplete)
		{
			FixupHolderList missingElements = holder.m_missingElements;
			object member = null;
			ObjectHolder holder2 = null;
			int num = 0;
			if (holder.ObjectValue == null)
			{
				throw new SerializationException(Environment.GetResourceString("Serialization_MissingObject", holder.m_id));
			}
			if (missingElements == null)
			{
				return;
			}
			if (holder.HasSurrogate || holder.HasISerializable)
			{
				SerializationInfo serInfo = holder.m_serInfo;
				if (serInfo == null)
				{
					throw new SerializationException(Environment.GetResourceString("Serialization_InvalidFixupDiscovered"));
				}
				if (missingElements != null)
				{
					for (int i = 0; i < missingElements.m_count; i++)
					{
						if (missingElements.m_values[i] != null && GetCompletionInfo(missingElements.m_values[i], out holder2, out member, bObjectFullyComplete))
						{
							object objectValue = holder2.ObjectValue;
							if (CanCallGetType(objectValue))
							{
								serInfo.UpdateValue((string)member, objectValue, objectValue.GetType());
							}
							else
							{
								serInfo.UpdateValue((string)member, objectValue, typeof(MarshalByRefObject));
							}
							num++;
							missingElements.m_values[i] = null;
							if (!bObjectFullyComplete)
							{
								holder.DecrementFixupsRemaining(this);
								holder2.RemoveDependency(holder.m_id);
							}
						}
					}
				}
			}
			else
			{
				for (int j = 0; j < missingElements.m_count; j++)
				{
					FixupHolder fixupHolder = missingElements.m_values[j];
					if (fixupHolder == null || !GetCompletionInfo(fixupHolder, out holder2, out member, bObjectFullyComplete))
					{
						continue;
					}
					if (holder2.TypeLoadExceptionReachable)
					{
						holder.TypeLoadException = holder2.TypeLoadException;
						if (holder.Reachable)
						{
							throw new SerializationException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Serialization_TypeLoadFailure"), holder.TypeLoadException.TypeName));
						}
					}
					if (holder.Reachable)
					{
						holder2.Reachable = true;
					}
					switch (fixupHolder.m_fixupType)
					{
					case 1:
						if (holder.RequiresValueTypeFixup)
						{
							throw new SerializationException(Environment.GetResourceString("Serialization_ValueTypeFixup"));
						}
						((Array)holder.ObjectValue).SetValue(holder2.ObjectValue, (int[])member);
						break;
					case 2:
					{
						MemberInfo memberInfo = (MemberInfo)member;
						if (memberInfo.MemberType == MemberTypes.Field)
						{
							if (holder.RequiresValueTypeFixup && holder.ValueTypeFixupPerformed)
							{
								if (!DoValueTypeFixup((FieldInfo)memberInfo, holder, holder2.ObjectValue))
								{
									throw new SerializationException(Environment.GetResourceString("Serialization_PartialValueTypeFixup"));
								}
							}
							else
							{
								FormatterServices.SerializationSetValue(memberInfo, holder.ObjectValue, holder2.ObjectValue);
							}
							if (holder2.RequiresValueTypeFixup)
							{
								holder2.ValueTypeFixupPerformed = true;
							}
							break;
						}
						throw new SerializationException(Environment.GetResourceString("Serialization_UnableToFixup"));
					}
					default:
						throw new SerializationException(Environment.GetResourceString("Serialization_UnableToFixup"));
					}
					num++;
					missingElements.m_values[j] = null;
					if (!bObjectFullyComplete)
					{
						holder.DecrementFixupsRemaining(this);
						holder2.RemoveDependency(holder.m_id);
					}
				}
			}
			m_fixupCount -= num;
			if (missingElements.m_count == num)
			{
				holder.m_missingElements = null;
			}
		}

		private void DoNewlyRegisteredObjectFixups(ObjectHolder holder)
		{
			if (holder.CanObjectValueChange)
			{
				return;
			}
			LongList dependentObjects = holder.DependentObjects;
			if (dependentObjects == null)
			{
				return;
			}
			dependentObjects.StartEnumeration();
			while (dependentObjects.MoveNext())
			{
				ObjectHolder objectHolder = FindObjectHolder(dependentObjects.Current);
				objectHolder.DecrementFixupsRemaining(this);
				if (objectHolder.DirectlyDependentObjects == 0)
				{
					if (objectHolder.ObjectValue != null)
					{
						CompleteObject(objectHolder, bObjectFullyComplete: true);
					}
					else
					{
						objectHolder.MarkForCompletionWhenAvailable();
					}
				}
			}
		}

		public virtual object GetObject(long objectID)
		{
			if (objectID <= 0)
			{
				throw new ArgumentOutOfRangeException("objectID", Environment.GetResourceString("ArgumentOutOfRange_ObjectID"));
			}
			ObjectHolder objectHolder = FindObjectHolder(objectID);
			if (objectHolder == null || objectHolder.CanObjectValueChange)
			{
				return null;
			}
			return objectHolder.ObjectValue;
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
		public virtual void RegisterObject(object obj, long objectID)
		{
			RegisterObject(obj, objectID, null, 0L, null);
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
		public void RegisterObject(object obj, long objectID, SerializationInfo info)
		{
			RegisterObject(obj, objectID, info, 0L, null);
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
		public void RegisterObject(object obj, long objectID, SerializationInfo info, long idOfContainingObj, MemberInfo member)
		{
			RegisterObject(obj, objectID, info, idOfContainingObj, member, null);
		}

		internal void RegisterString(string obj, long objectID, SerializationInfo info, long idOfContainingObj, MemberInfo member)
		{
			ObjectHolder holder = new ObjectHolder(obj, objectID, info, null, idOfContainingObj, (FieldInfo)member, null);
			AddObjectHolder(holder);
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
		public void RegisterObject(object obj, long objectID, SerializationInfo info, long idOfContainingObj, MemberInfo member, int[] arrayIndex)
		{
			ISerializationSurrogate surrogate = null;
			if (obj == null)
			{
				throw new ArgumentNullException("obj");
			}
			if (objectID <= 0)
			{
				throw new ArgumentOutOfRangeException("objectID", Environment.GetResourceString("ArgumentOutOfRange_ObjectID"));
			}
			if (member != null && !(member is RuntimeFieldInfo) && !(member is SerializationFieldInfo))
			{
				throw new SerializationException(Environment.GetResourceString("Serialization_UnknownMemberInfo"));
			}
			if (m_selector != null)
			{
				Type type = null;
				type = ((!CanCallGetType(obj)) ? typeof(MarshalByRefObject) : obj.GetType());
				surrogate = m_selector.GetSurrogate(type, m_context, out var _);
			}
			if (obj is IDeserializationCallback)
			{
				DeserializationEventHandler handler = ((IDeserializationCallback)obj).OnDeserialization;
				AddOnDeserialization(handler);
			}
			if (arrayIndex != null)
			{
				arrayIndex = (int[])arrayIndex.Clone();
			}
			ObjectHolder objectHolder = FindObjectHolder(objectID);
			if (objectHolder == null)
			{
				objectHolder = new ObjectHolder(obj, objectID, info, surrogate, idOfContainingObj, (FieldInfo)member, arrayIndex);
				AddObjectHolder(objectHolder);
				if (objectHolder.RequiresDelayedFixup)
				{
					SpecialFixupObjects.Add(objectHolder);
				}
				AddOnDeserialized(obj);
				return;
			}
			if (objectHolder.ObjectValue != null)
			{
				throw new SerializationException(Environment.GetResourceString("Serialization_RegisterTwice"));
			}
			objectHolder.UpdateData(obj, info, surrogate, idOfContainingObj, (FieldInfo)member, arrayIndex, this);
			if (objectHolder.DirectlyDependentObjects > 0)
			{
				CompleteObject(objectHolder, bObjectFullyComplete: false);
			}
			if (objectHolder.RequiresDelayedFixup)
			{
				SpecialFixupObjects.Add(objectHolder);
			}
			if (objectHolder.CompletelyFixed)
			{
				DoNewlyRegisteredObjectFixups(objectHolder);
				objectHolder.DependentObjects = null;
			}
			if (objectHolder.TotalDependentObjects > 0)
			{
				AddOnDeserialized(obj);
			}
			else
			{
				RaiseOnDeserializedEvent(obj);
			}
		}

		internal void CompleteISerializableObject(object obj, SerializationInfo info, StreamingContext context)
		{
			RuntimeConstructorInfo runtimeConstructorInfo = null;
			if (obj == null)
			{
				throw new ArgumentNullException("obj");
			}
			if (!(obj is ISerializable))
			{
				throw new ArgumentException(Environment.GetResourceString("Serialization_NotISer"));
			}
			Type type = obj.GetType();
			try
			{
				runtimeConstructorInfo = ((type != TypeOfWindowsIdentity || !m_isCrossAppDomain) ? GetConstructor(type) : GetConstructor(type, SIWindowsIdentityConstructorTypes));
			}
			catch (Exception innerException)
			{
				throw new SerializationException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Serialization_ConstructorNotFound"), type), innerException);
			}
			runtimeConstructorInfo.SerializationInvoke(obj, info, context);
		}

		internal static RuntimeConstructorInfo GetConstructor(Type t)
		{
			return GetConstructor(t, SIConstructorTypes);
		}

		internal static RuntimeConstructorInfo GetConstructor(Type t, Type[] ctorParams)
		{
			RuntimeConstructorInfo runtimeConstructorInfo;
			if ((runtimeConstructorInfo = (RuntimeConstructorInfo)t.Cache[CacheObjType.ConstructorInfo]) == null)
			{
				RuntimeType runtimeType = (RuntimeType)t;
				runtimeConstructorInfo = runtimeType.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, CallingConventions.Any, ctorParams, null) as RuntimeConstructorInfo;
				if (runtimeConstructorInfo == null)
				{
					throw new SerializationException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Serialization_ConstructorNotFound"), t.FullName));
				}
				t.Cache[CacheObjType.ConstructorInfo] = runtimeConstructorInfo;
			}
			return runtimeConstructorInfo;
		}

		public virtual void DoFixups()
		{
			int num = -1;
			while (num != 0)
			{
				num = 0;
				ObjectHolderListEnumerator fixupEnumerator = SpecialFixupObjects.GetFixupEnumerator();
				while (fixupEnumerator.MoveNext())
				{
					ObjectHolder current = fixupEnumerator.Current;
					if (current.ObjectValue == null)
					{
						throw new SerializationException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Serialization_ObjectNotSupplied"), current.m_id));
					}
					if (current.TotalDependentObjects == 0)
					{
						if (current.RequiresSerInfoFixup)
						{
							FixupSpecialObject(current);
							num++;
						}
						else if (!current.IsIncompleteObjectReference)
						{
							CompleteObject(current, bObjectFullyComplete: true);
						}
						if (current.IsIncompleteObjectReference && ResolveObjectReference(current))
						{
							num++;
						}
					}
				}
			}
			if (m_fixupCount == 0)
			{
				if (TopObject is TypeLoadExceptionHolder)
				{
					throw new SerializationException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Serialization_TypeLoadFailure"), ((TypeLoadExceptionHolder)TopObject).TypeName));
				}
				return;
			}
			for (int i = 0; i < m_objects.Length; i++)
			{
				for (ObjectHolder current = m_objects[i]; current != null; current = current.m_next)
				{
					if (current.TotalDependentObjects > 0)
					{
						CompleteObject(current, bObjectFullyComplete: true);
					}
				}
				if (m_fixupCount == 0)
				{
					return;
				}
			}
			throw new SerializationException(Environment.GetResourceString("Serialization_IncorrectNumberOfFixups"));
		}

		private void RegisterFixup(FixupHolder fixup, long objectToBeFixed, long objectRequired)
		{
			ObjectHolder objectHolder = FindOrCreateObjectHolder(objectToBeFixed);
			if (objectHolder.RequiresSerInfoFixup && fixup.m_fixupType == 2)
			{
				throw new SerializationException(Environment.GetResourceString("Serialization_InvalidFixupType"));
			}
			objectHolder.AddFixup(fixup, this);
			ObjectHolder objectHolder2 = FindOrCreateObjectHolder(objectRequired);
			objectHolder2.AddDependency(objectToBeFixed);
			m_fixupCount++;
		}

		public virtual void RecordFixup(long objectToBeFixed, MemberInfo member, long objectRequired)
		{
			if (objectToBeFixed <= 0 || objectRequired <= 0)
			{
				throw new ArgumentOutOfRangeException((objectToBeFixed <= 0) ? "objectToBeFixed" : "objectRequired", Environment.GetResourceString("Serialization_IdTooSmall"));
			}
			if (member == null)
			{
				throw new ArgumentNullException("member");
			}
			if (!(member is RuntimeFieldInfo) && !(member is SerializationFieldInfo))
			{
				throw new SerializationException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Serialization_InvalidType"), member.GetType().ToString()));
			}
			FixupHolder fixup = new FixupHolder(objectRequired, member, 2);
			RegisterFixup(fixup, objectToBeFixed, objectRequired);
		}

		public virtual void RecordDelayedFixup(long objectToBeFixed, string memberName, long objectRequired)
		{
			if (objectToBeFixed <= 0 || objectRequired <= 0)
			{
				throw new ArgumentOutOfRangeException((objectToBeFixed <= 0) ? "objectToBeFixed" : "objectRequired", Environment.GetResourceString("Serialization_IdTooSmall"));
			}
			if (memberName == null)
			{
				throw new ArgumentNullException("memberName");
			}
			FixupHolder fixup = new FixupHolder(objectRequired, memberName, 4);
			RegisterFixup(fixup, objectToBeFixed, objectRequired);
		}

		public virtual void RecordArrayElementFixup(long arrayToBeFixed, int index, long objectRequired)
		{
			RecordArrayElementFixup(arrayToBeFixed, new int[1]
			{
				index
			}, objectRequired);
		}

		public virtual void RecordArrayElementFixup(long arrayToBeFixed, int[] indices, long objectRequired)
		{
			if (arrayToBeFixed <= 0 || objectRequired <= 0)
			{
				throw new ArgumentOutOfRangeException((arrayToBeFixed <= 0) ? "objectToBeFixed" : "objectRequired", Environment.GetResourceString("Serialization_IdTooSmall"));
			}
			if (indices == null)
			{
				throw new ArgumentNullException("indices");
			}
			FixupHolder fixup = new FixupHolder(objectRequired, indices, 1);
			RegisterFixup(fixup, arrayToBeFixed, objectRequired);
		}

		public virtual void RaiseDeserializationEvent()
		{
			if (m_onDeserializedHandler != null)
			{
				m_onDeserializedHandler(m_context);
			}
			if (m_onDeserializationHandler != null)
			{
				m_onDeserializationHandler(null);
			}
		}

		internal virtual void AddOnDeserialization(DeserializationEventHandler handler)
		{
			m_onDeserializationHandler = (DeserializationEventHandler)Delegate.Combine(m_onDeserializationHandler, handler);
		}

		internal virtual void RemoveOnDeserialization(DeserializationEventHandler handler)
		{
			m_onDeserializationHandler = (DeserializationEventHandler)Delegate.Remove(m_onDeserializationHandler, handler);
		}

		internal virtual void AddOnDeserialized(object obj)
		{
			SerializationEvents serializationEventsForType = SerializationEventsCache.GetSerializationEventsForType(obj.GetType());
			m_onDeserializedHandler = serializationEventsForType.AddOnDeserialized(obj, m_onDeserializedHandler);
		}

		internal virtual void RaiseOnDeserializedEvent(object obj)
		{
			SerializationEvents serializationEventsForType = SerializationEventsCache.GetSerializationEventsForType(obj.GetType());
			serializationEventsForType.InvokeOnDeserialized(obj, m_context);
		}

		public void RaiseOnDeserializingEvent(object obj)
		{
			SerializationEvents serializationEventsForType = SerializationEventsCache.GetSerializationEventsForType(obj.GetType());
			serializationEventsForType.InvokeOnDeserializing(obj, m_context);
		}
	}
}
