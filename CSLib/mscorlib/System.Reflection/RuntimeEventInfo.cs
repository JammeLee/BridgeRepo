using System.Collections;
using System.Runtime.ConstrainedExecution;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace System.Reflection
{
	[Serializable]
	internal sealed class RuntimeEventInfo : EventInfo, ISerializable
	{
		private int m_token;

		private EventAttributes m_flags;

		private string m_name;

		private unsafe void* m_utf8name;

		private RuntimeType.RuntimeTypeCache m_reflectedTypeCache;

		private RuntimeMethodInfo m_addMethod;

		private RuntimeMethodInfo m_removeMethod;

		private RuntimeMethodInfo m_raiseMethod;

		private MethodInfo[] m_otherMethod;

		private RuntimeType m_declaringType;

		private BindingFlags m_bindingFlags;

		internal BindingFlags BindingFlags => m_bindingFlags;

		public override MemberTypes MemberType => MemberTypes.Event;

		public unsafe override string Name
		{
			get
			{
				if (m_name == null)
				{
					m_name = new Utf8String(m_utf8name).ToString();
				}
				return m_name;
			}
		}

		public override Type DeclaringType => m_declaringType;

		public override Type ReflectedType => m_reflectedTypeCache.RuntimeType;

		public override int MetadataToken => m_token;

		public override Module Module => m_declaringType.Module;

		public override EventAttributes Attributes => m_flags;

		internal RuntimeEventInfo()
		{
		}

		internal unsafe RuntimeEventInfo(int tkEvent, RuntimeType declaredType, RuntimeType.RuntimeTypeCache reflectedTypeCache, out bool isPrivate)
		{
			MetadataImport metadataImport = declaredType.Module.MetadataImport;
			m_token = tkEvent;
			m_reflectedTypeCache = reflectedTypeCache;
			m_declaringType = declaredType;
			RuntimeTypeHandle typeHandleInternal = declaredType.GetTypeHandleInternal();
			RuntimeTypeHandle runtimeTypeHandle = reflectedTypeCache.RuntimeTypeHandle;
			metadataImport.GetEventProps(tkEvent, out m_utf8name, out m_flags);
			int associatesCount = metadataImport.GetAssociatesCount(tkEvent);
			AssociateRecord* ptr = (AssociateRecord*)stackalloc byte[sizeof(AssociateRecord) * associatesCount];
			metadataImport.GetAssociates(tkEvent, ptr, associatesCount);
			Associates.AssignAssociates(ptr, associatesCount, typeHandleInternal, runtimeTypeHandle, out m_addMethod, out m_removeMethod, out m_raiseMethod, out var getter, out getter, out m_otherMethod, out isPrivate, out m_bindingFlags);
		}

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		internal override bool CacheEquals(object o)
		{
			RuntimeEventInfo runtimeEventInfo = o as RuntimeEventInfo;
			if (runtimeEventInfo == null)
			{
				return false;
			}
			if (runtimeEventInfo.m_token == m_token)
			{
				return m_declaringType.GetTypeHandleInternal().GetModuleHandle().Equals(runtimeEventInfo.m_declaringType.GetTypeHandleInternal().GetModuleHandle());
			}
			return false;
		}

		public override string ToString()
		{
			if (m_addMethod == null || m_addMethod.GetParametersNoCopy().Length == 0)
			{
				throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_NoPublicAddMethod"));
			}
			return m_addMethod.GetParametersNoCopy()[0].ParameterType.SigToString() + " " + Name;
		}

		public override object[] GetCustomAttributes(bool inherit)
		{
			return CustomAttribute.GetCustomAttributes(this, typeof(object) as RuntimeType);
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
			return CustomAttribute.GetCustomAttributes(this, runtimeType);
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
			return CustomAttribute.IsDefined(this, runtimeType);
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			MemberInfoSerializationHolder.GetSerializationInfo(info, Name, ReflectedType, null, MemberTypes.Event);
		}

		public override MethodInfo[] GetOtherMethods(bool nonPublic)
		{
			ArrayList arrayList = new ArrayList();
			if (m_otherMethod == null)
			{
				return new MethodInfo[0];
			}
			for (int i = 0; i < m_otherMethod.Length; i++)
			{
				if (Associates.IncludeAccessor(m_otherMethod[i], nonPublic))
				{
					arrayList.Add(m_otherMethod[i]);
				}
			}
			return arrayList.ToArray(typeof(MethodInfo)) as MethodInfo[];
		}

		public override MethodInfo GetAddMethod(bool nonPublic)
		{
			if (!Associates.IncludeAccessor(m_addMethod, nonPublic))
			{
				return null;
			}
			return m_addMethod;
		}

		public override MethodInfo GetRemoveMethod(bool nonPublic)
		{
			if (!Associates.IncludeAccessor(m_removeMethod, nonPublic))
			{
				return null;
			}
			return m_removeMethod;
		}

		public override MethodInfo GetRaiseMethod(bool nonPublic)
		{
			if (!Associates.IncludeAccessor(m_raiseMethod, nonPublic))
			{
				return null;
			}
			return m_raiseMethod;
		}
	}
}
