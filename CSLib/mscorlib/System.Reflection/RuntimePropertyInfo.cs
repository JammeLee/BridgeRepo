using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.ConstrainedExecution;
using System.Runtime.Serialization;

namespace System.Reflection
{
	[Serializable]
	internal sealed class RuntimePropertyInfo : PropertyInfo, ISerializable
	{
		private int m_token;

		private string m_name;

		private unsafe void* m_utf8name;

		private PropertyAttributes m_flags;

		private RuntimeType.RuntimeTypeCache m_reflectedTypeCache;

		private RuntimeMethodInfo m_getterMethod;

		private RuntimeMethodInfo m_setterMethod;

		private MethodInfo[] m_otherMethod;

		private RuntimeType m_declaringType;

		private BindingFlags m_bindingFlags;

		private Signature m_signature;

		internal unsafe Signature Signature
		{
			get
			{
				if (m_signature == null)
				{
					Module.MetadataImport.GetPropertyProps(m_token, out var _, out MetadataArgs.Skip.PropertyAttributes, out var signature);
					m_signature = new Signature(signature.Signature.ToPointer(), signature.Length, m_declaringType.GetTypeHandleInternal());
				}
				return m_signature;
			}
		}

		internal BindingFlags BindingFlags => m_bindingFlags;

		public override MemberTypes MemberType => MemberTypes.Property;

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

		public override Type PropertyType => Signature.ReturnTypeHandle.GetRuntimeType();

		public override PropertyAttributes Attributes => m_flags;

		public override bool CanRead => m_getterMethod != null;

		public override bool CanWrite => m_setterMethod != null;

		internal unsafe RuntimePropertyInfo(int tkProperty, RuntimeType declaredType, RuntimeType.RuntimeTypeCache reflectedTypeCache, out bool isPrivate)
		{
			MetadataImport metadataImport = declaredType.Module.MetadataImport;
			m_token = tkProperty;
			m_reflectedTypeCache = reflectedTypeCache;
			m_declaringType = declaredType;
			RuntimeTypeHandle typeHandleInternal = declaredType.GetTypeHandleInternal();
			RuntimeTypeHandle runtimeTypeHandle = reflectedTypeCache.RuntimeTypeHandle;
			metadataImport.GetPropertyProps(tkProperty, out m_utf8name, out m_flags, out MetadataArgs.Skip.ConstArray);
			int associatesCount = metadataImport.GetAssociatesCount(tkProperty);
			AssociateRecord* ptr = (AssociateRecord*)stackalloc byte[sizeof(AssociateRecord) * associatesCount];
			metadataImport.GetAssociates(tkProperty, ptr, associatesCount);
			Associates.AssignAssociates(ptr, associatesCount, typeHandleInternal, runtimeTypeHandle, out var addOn, out addOn, out addOn, out m_getterMethod, out m_setterMethod, out m_otherMethod, out isPrivate, out m_bindingFlags);
		}

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		internal override bool CacheEquals(object o)
		{
			RuntimePropertyInfo runtimePropertyInfo = o as RuntimePropertyInfo;
			if (runtimePropertyInfo == null)
			{
				return false;
			}
			if (runtimePropertyInfo.m_token == m_token)
			{
				return m_declaringType.GetTypeHandleInternal().GetModuleHandle().Equals(runtimePropertyInfo.m_declaringType.GetTypeHandleInternal().GetModuleHandle());
			}
			return false;
		}

		internal bool EqualsSig(RuntimePropertyInfo target)
		{
			return Signature.DiffSigs(Signature, DeclaringType.GetTypeHandleInternal(), target.Signature, target.DeclaringType.GetTypeHandleInternal());
		}

		public override string ToString()
		{
			string text = PropertyType.SigToString() + " " + Name;
			RuntimeTypeHandle[] arguments = Signature.Arguments;
			if (arguments.Length > 0)
			{
				Type[] array = new Type[arguments.Length];
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = arguments[i].GetRuntimeType();
				}
				text = text + " [" + RuntimeMethodInfo.ConstructParameters(array, Signature.CallingConvention) + "]";
			}
			return text;
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

		public override Type[] GetRequiredCustomModifiers()
		{
			return Signature.GetCustomModifiers(0, required: true);
		}

		public override Type[] GetOptionalCustomModifiers()
		{
			return Signature.GetCustomModifiers(0, required: false);
		}

		internal object GetConstantValue(bool raw)
		{
			object value = MdConstant.GetValue(Module.MetadataImport, m_token, PropertyType.GetTypeHandleInternal(), raw);
			if (value == DBNull.Value)
			{
				throw new InvalidOperationException(Environment.GetResourceString("Arg_EnumLitValueNotFound"));
			}
			return value;
		}

		public override object GetConstantValue()
		{
			return GetConstantValue(raw: false);
		}

		public override object GetRawConstantValue()
		{
			return GetConstantValue(raw: true);
		}

		public override MethodInfo[] GetAccessors(bool nonPublic)
		{
			ArrayList arrayList = new ArrayList();
			if (Associates.IncludeAccessor(m_getterMethod, nonPublic))
			{
				arrayList.Add(m_getterMethod);
			}
			if (Associates.IncludeAccessor(m_setterMethod, nonPublic))
			{
				arrayList.Add(m_setterMethod);
			}
			if (m_otherMethod != null)
			{
				for (int i = 0; i < m_otherMethod.Length; i++)
				{
					if (Associates.IncludeAccessor(m_otherMethod[i], nonPublic))
					{
						arrayList.Add(m_otherMethod[i]);
					}
				}
			}
			return arrayList.ToArray(typeof(MethodInfo)) as MethodInfo[];
		}

		public override MethodInfo GetGetMethod(bool nonPublic)
		{
			if (!Associates.IncludeAccessor(m_getterMethod, nonPublic))
			{
				return null;
			}
			return m_getterMethod;
		}

		public override MethodInfo GetSetMethod(bool nonPublic)
		{
			if (!Associates.IncludeAccessor(m_setterMethod, nonPublic))
			{
				return null;
			}
			return m_setterMethod;
		}

		public override ParameterInfo[] GetIndexParameters()
		{
			int num = 0;
			ParameterInfo[] array = null;
			MethodInfo getMethod = GetGetMethod(nonPublic: true);
			if (getMethod != null)
			{
				array = getMethod.GetParametersNoCopy();
				num = array.Length;
			}
			else
			{
				getMethod = GetSetMethod(nonPublic: true);
				if (getMethod != null)
				{
					array = getMethod.GetParametersNoCopy();
					num = array.Length - 1;
				}
			}
			if (array != null && array.Length == 0)
			{
				return array;
			}
			ParameterInfo[] array2 = new ParameterInfo[num];
			for (int i = 0; i < num; i++)
			{
				array2[i] = new ParameterInfo(array[i], this);
			}
			return array2;
		}

		[DebuggerStepThrough]
		[DebuggerHidden]
		public override object GetValue(object obj, object[] index)
		{
			return GetValue(obj, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, index, null);
		}

		[DebuggerHidden]
		[DebuggerStepThrough]
		public override object GetValue(object obj, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture)
		{
			MethodInfo getMethod = GetGetMethod(nonPublic: true);
			if (getMethod == null)
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_GetMethNotFnd"));
			}
			return getMethod.Invoke(obj, invokeAttr, binder, index, null);
		}

		[DebuggerHidden]
		[DebuggerStepThrough]
		public override void SetValue(object obj, object value, object[] index)
		{
			SetValue(obj, value, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, index, null);
		}

		[DebuggerStepThrough]
		[DebuggerHidden]
		public override void SetValue(object obj, object value, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture)
		{
			MethodInfo setMethod = GetSetMethod(nonPublic: true);
			if (setMethod == null)
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_SetMethNotFnd"));
			}
			object[] array = null;
			if (index == null)
			{
				array = new object[1]
				{
					value
				};
			}
			else
			{
				array = new object[index.Length + 1];
				for (int i = 0; i < index.Length; i++)
				{
					array[i] = index[i];
				}
				array[index.Length] = value;
			}
			setMethod.Invoke(obj, invokeAttr, binder, array, culture);
		}

		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			MemberInfoSerializationHolder.GetSerializationInfo(info, Name, ReflectedType, ToString(), MemberTypes.Property);
		}
	}
}
