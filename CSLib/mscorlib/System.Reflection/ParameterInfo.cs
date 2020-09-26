using System.Collections.Generic;
using System.Reflection.Cache;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Threading;

namespace System.Reflection
{
	[Serializable]
	[ComDefaultInterface(typeof(_ParameterInfo))]
	[ClassInterface(ClassInterfaceType.None)]
	[ComVisible(true)]
	public class ParameterInfo : _ParameterInfo, ICustomAttributeProvider
	{
		[Flags]
		private enum WhatIsCached
		{
			Nothing = 0x0,
			Name = 0x1,
			ParameterType = 0x2,
			DefaultValue = 0x4,
			All = 0x7
		}

		private static readonly Type s_DecimalConstantAttributeType = typeof(DecimalConstantAttribute);

		private static readonly Type s_CustomConstantAttributeType = typeof(CustomConstantAttribute);

		private static Type ParameterInfoType = typeof(ParameterInfo);

		protected string NameImpl;

		protected Type ClassImpl;

		protected int PositionImpl;

		protected ParameterAttributes AttrsImpl;

		protected object DefaultValueImpl;

		protected MemberInfo MemberImpl;

		private IntPtr _importer;

		private int _token;

		private bool bExtraConstChecked;

		[NonSerialized]
		private int m_tkParamDef;

		[NonSerialized]
		private MetadataImport m_scope;

		[NonSerialized]
		private Signature m_signature;

		[NonSerialized]
		private volatile bool m_nameIsCached;

		[NonSerialized]
		private readonly bool m_noDefaultValue;

		private InternalCache m_cachedData;

		private bool IsLegacyParameterInfo => GetType() != typeof(ParameterInfo);

		public virtual Type ParameterType
		{
			get
			{
				if (ClassImpl == null && GetType() == typeof(ParameterInfo))
				{
					ClassImpl = ((PositionImpl != -1) ? m_signature.Arguments[PositionImpl] : m_signature.ReturnTypeHandle).GetRuntimeType();
				}
				return ClassImpl;
			}
		}

		public virtual string Name
		{
			get
			{
				if (!m_nameIsCached)
				{
					if (!System.Reflection.MetadataToken.IsNullToken(m_tkParamDef))
					{
						string text = (NameImpl = m_scope.GetName(m_tkParamDef).ToString());
					}
					m_nameIsCached = true;
				}
				return NameImpl;
			}
		}

		public virtual object DefaultValue => GetDefaultValue(raw: false);

		public virtual object RawDefaultValue => GetDefaultValue(raw: true);

		public virtual int Position => PositionImpl;

		public virtual ParameterAttributes Attributes => AttrsImpl;

		public virtual MemberInfo Member => MemberImpl;

		public bool IsIn => (Attributes & ParameterAttributes.In) != 0;

		public bool IsOut => (Attributes & ParameterAttributes.Out) != 0;

		public bool IsLcid => (Attributes & ParameterAttributes.Lcid) != 0;

		public bool IsRetval => (Attributes & ParameterAttributes.Retval) != 0;

		public bool IsOptional => (Attributes & ParameterAttributes.Optional) != 0;

		public int MetadataToken => m_tkParamDef;

		internal InternalCache Cache
		{
			get
			{
				InternalCache internalCache = m_cachedData;
				if (internalCache == null)
				{
					internalCache = new InternalCache("ParameterInfo");
					InternalCache internalCache2 = Interlocked.CompareExchange(ref m_cachedData, internalCache, null);
					if (internalCache2 != null)
					{
						internalCache = internalCache2;
					}
					GC.ClearCache += OnCacheClear;
				}
				return internalCache;
			}
		}

		internal static ParameterInfo[] GetParameters(MethodBase method, MemberInfo member, Signature sig)
		{
			ParameterInfo returnParameter;
			return GetParameters(method, member, sig, out returnParameter, fetchReturnParameter: false);
		}

		internal static ParameterInfo GetReturnParameter(MethodBase method, MemberInfo member, Signature sig)
		{
			GetParameters(method, member, sig, out var returnParameter, fetchReturnParameter: true);
			return returnParameter;
		}

		internal unsafe static ParameterInfo[] GetParameters(MethodBase method, MemberInfo member, Signature sig, out ParameterInfo returnParameter, bool fetchReturnParameter)
		{
			RuntimeMethodHandle methodHandle = method.GetMethodHandle();
			returnParameter = null;
			int num = sig.Arguments.Length;
			ParameterInfo[] array = (fetchReturnParameter ? null : new ParameterInfo[num]);
			int methodDef = methodHandle.GetMethodDef();
			int num2 = 0;
			if (!System.Reflection.MetadataToken.IsNullToken(methodDef))
			{
				MetadataImport metadataImport = methodHandle.GetDeclaringType().GetModuleHandle().GetMetadataImport();
				num2 = metadataImport.EnumParamsCount(methodDef);
				int* ptr = (int*)stackalloc byte[4 * num2];
				metadataImport.EnumParams(methodDef, ptr, num2);
				for (uint num3 = 0u; num3 < num2; num3++)
				{
					int num4 = ptr[num3];
					metadataImport.GetParamDefProps(num4, out var sequence, out var attributes);
					sequence--;
					if (fetchReturnParameter && sequence == -1)
					{
						returnParameter = new ParameterInfo(sig, metadataImport, num4, sequence, attributes, member);
					}
					else if (!fetchReturnParameter && sequence >= 0)
					{
						array[sequence] = new ParameterInfo(sig, metadataImport, num4, sequence, attributes, member);
					}
				}
			}
			if (fetchReturnParameter)
			{
				if (returnParameter == null)
				{
					returnParameter = new ParameterInfo(sig, MetadataImport.EmptyImport, 0, -1, ParameterAttributes.None, member);
				}
			}
			else if (num2 < array.Length + 1)
			{
				for (int i = 0; i < array.Length; i++)
				{
					if (array[i] == null)
					{
						array[i] = new ParameterInfo(sig, MetadataImport.EmptyImport, 0, i, ParameterAttributes.None, member);
					}
				}
			}
			return array;
		}

		[OnSerializing]
		private void OnSerializing(StreamingContext context)
		{
			_ = ParameterType;
			_ = Name;
			DefaultValueImpl = DefaultValue;
			_importer = IntPtr.Zero;
			_token = m_tkParamDef;
			bExtraConstChecked = false;
		}

		[OnDeserialized]
		private void OnDeserialized(StreamingContext context)
		{
			ParameterInfo parameterInfo = null;
			if (MemberImpl == null)
			{
				throw new SerializationException(Environment.GetResourceString("Serialization_InsufficientState"));
			}
			ParameterInfo[] array = null;
			switch (MemberImpl.MemberType)
			{
			case MemberTypes.Constructor:
			case MemberTypes.Method:
				if (PositionImpl == -1)
				{
					if (MemberImpl.MemberType == MemberTypes.Method)
					{
						parameterInfo = ((MethodInfo)MemberImpl).ReturnParameter;
						break;
					}
					throw new SerializationException(Environment.GetResourceString("Serialization_BadParameterInfo"));
				}
				array = ((MethodBase)MemberImpl).GetParametersNoCopy();
				if (array != null && PositionImpl < array.Length)
				{
					parameterInfo = array[PositionImpl];
					break;
				}
				throw new SerializationException(Environment.GetResourceString("Serialization_BadParameterInfo"));
			case MemberTypes.Property:
				array = ((PropertyInfo)MemberImpl).GetIndexParameters();
				if (array != null && PositionImpl > -1 && PositionImpl < array.Length)
				{
					parameterInfo = array[PositionImpl];
					break;
				}
				throw new SerializationException(Environment.GetResourceString("Serialization_BadParameterInfo"));
			default:
				throw new SerializationException(Environment.GetResourceString("Serialization_NoParameterInfo"));
			}
			m_tkParamDef = parameterInfo.m_tkParamDef;
			m_scope = parameterInfo.m_scope;
			m_signature = parameterInfo.m_signature;
			m_nameIsCached = true;
		}

		protected ParameterInfo()
		{
			m_nameIsCached = true;
			m_noDefaultValue = true;
		}

		internal ParameterInfo(ParameterInfo accessor, RuntimePropertyInfo property)
			: this(accessor, (MemberInfo)property)
		{
			m_signature = property.Signature;
		}

		internal ParameterInfo(ParameterInfo accessor, MethodBuilderInstantiation method)
			: this(accessor, (MemberInfo)method)
		{
			m_signature = accessor.m_signature;
			if (ClassImpl.IsGenericParameter)
			{
				ClassImpl = method.GetGenericArguments()[ClassImpl.GenericParameterPosition];
			}
		}

		private ParameterInfo(ParameterInfo accessor, MemberInfo member)
		{
			MemberImpl = member;
			NameImpl = accessor.Name;
			m_nameIsCached = true;
			ClassImpl = accessor.ParameterType;
			PositionImpl = accessor.Position;
			AttrsImpl = accessor.Attributes;
			m_tkParamDef = (System.Reflection.MetadataToken.IsNullToken(accessor.MetadataToken) ? 134217728 : accessor.MetadataToken);
			m_scope = accessor.m_scope;
		}

		private ParameterInfo(Signature signature, MetadataImport scope, int tkParamDef, int position, ParameterAttributes attributes, MemberInfo member)
		{
			PositionImpl = position;
			MemberImpl = member;
			m_signature = signature;
			m_tkParamDef = (System.Reflection.MetadataToken.IsNullToken(tkParamDef) ? 134217728 : tkParamDef);
			m_scope = scope;
			AttrsImpl = attributes;
			ClassImpl = null;
			NameImpl = null;
		}

		internal ParameterInfo(MethodInfo owner, string name, RuntimeType parameterType, int position)
		{
			MemberImpl = owner;
			NameImpl = name;
			m_nameIsCached = true;
			m_noDefaultValue = true;
			ClassImpl = parameterType;
			PositionImpl = position;
			AttrsImpl = ParameterAttributes.None;
			m_tkParamDef = 134217728;
			m_scope = MetadataImport.EmptyImport;
		}

		internal void SetName(string name)
		{
			NameImpl = name;
		}

		internal void SetAttributes(ParameterAttributes attributes)
		{
			AttrsImpl = attributes;
		}

		internal object GetDefaultValue(bool raw)
		{
			object obj = null;
			if (!m_noDefaultValue)
			{
				if (ParameterType == typeof(DateTime))
				{
					if (raw)
					{
						CustomAttributeTypedArgument customAttributeTypedArgument = CustomAttributeData.Filter(CustomAttributeData.GetCustomAttributes(this), typeof(DateTimeConstantAttribute), 0);
						if (customAttributeTypedArgument.ArgumentType != null)
						{
							return new DateTime((long)customAttributeTypedArgument.Value);
						}
					}
					else
					{
						object[] customAttributes = GetCustomAttributes(typeof(DateTimeConstantAttribute), inherit: false);
						if (customAttributes != null && customAttributes.Length != 0)
						{
							return ((DateTimeConstantAttribute)customAttributes[0]).Value;
						}
					}
				}
				if (!System.Reflection.MetadataToken.IsNullToken(m_tkParamDef))
				{
					obj = MdConstant.GetValue(m_scope, m_tkParamDef, ParameterType.GetTypeHandleInternal(), raw);
				}
				if (obj == DBNull.Value)
				{
					if (raw)
					{
						IList<CustomAttributeData> customAttributes2 = CustomAttributeData.GetCustomAttributes(this);
						CustomAttributeTypedArgument customAttributeTypedArgument2 = CustomAttributeData.Filter(customAttributes2, s_CustomConstantAttributeType, "Value");
						if (customAttributeTypedArgument2.ArgumentType == null)
						{
							customAttributeTypedArgument2 = CustomAttributeData.Filter(customAttributes2, s_DecimalConstantAttributeType, "Value");
							if (customAttributeTypedArgument2.ArgumentType == null)
							{
								for (int i = 0; i < customAttributes2.Count; i++)
								{
									if (customAttributes2[i].Constructor.DeclaringType != s_DecimalConstantAttributeType)
									{
										continue;
									}
									ParameterInfo[] parameters = customAttributes2[i].Constructor.GetParameters();
									if (parameters.Length != 0)
									{
										if (parameters[2].ParameterType == typeof(uint))
										{
											IList<CustomAttributeTypedArgument> constructorArguments = customAttributes2[i].ConstructorArguments;
											int lo = (int)(uint)constructorArguments[4].Value;
											int mid = (int)(uint)constructorArguments[3].Value;
											int hi = (int)(uint)constructorArguments[2].Value;
											byte b = (byte)constructorArguments[1].Value;
											byte scale = (byte)constructorArguments[0].Value;
											customAttributeTypedArgument2 = new CustomAttributeTypedArgument(new decimal(lo, mid, hi, b != 0, scale));
										}
										else
										{
											IList<CustomAttributeTypedArgument> constructorArguments2 = customAttributes2[i].ConstructorArguments;
											int lo2 = (int)constructorArguments2[4].Value;
											int mid2 = (int)constructorArguments2[3].Value;
											int hi2 = (int)constructorArguments2[2].Value;
											byte b2 = (byte)constructorArguments2[1].Value;
											byte scale2 = (byte)constructorArguments2[0].Value;
											customAttributeTypedArgument2 = new CustomAttributeTypedArgument(new decimal(lo2, mid2, hi2, b2 != 0, scale2));
										}
									}
								}
							}
						}
						if (customAttributeTypedArgument2.ArgumentType != null)
						{
							obj = customAttributeTypedArgument2.Value;
						}
					}
					else
					{
						object[] customAttributes3 = GetCustomAttributes(s_CustomConstantAttributeType, inherit: false);
						if (customAttributes3.Length != 0)
						{
							obj = ((CustomConstantAttribute)customAttributes3[0]).Value;
						}
						else
						{
							customAttributes3 = GetCustomAttributes(s_DecimalConstantAttributeType, inherit: false);
							if (customAttributes3.Length != 0)
							{
								obj = ((DecimalConstantAttribute)customAttributes3[0]).Value;
							}
						}
					}
				}
				if (obj == DBNull.Value && IsOptional)
				{
					obj = Type.Missing;
				}
			}
			return obj;
		}

		public virtual Type[] GetRequiredCustomModifiers()
		{
			if (IsLegacyParameterInfo)
			{
				return new Type[0];
			}
			return m_signature.GetCustomModifiers(PositionImpl + 1, required: true);
		}

		public virtual Type[] GetOptionalCustomModifiers()
		{
			if (IsLegacyParameterInfo)
			{
				return new Type[0];
			}
			return m_signature.GetCustomModifiers(PositionImpl + 1, required: false);
		}

		public override string ToString()
		{
			return ParameterType.SigToString() + " " + Name;
		}

		public virtual object[] GetCustomAttributes(bool inherit)
		{
			if (IsLegacyParameterInfo)
			{
				return null;
			}
			if (System.Reflection.MetadataToken.IsNullToken(m_tkParamDef))
			{
				return new object[0];
			}
			return CustomAttribute.GetCustomAttributes(this, typeof(object) as RuntimeType);
		}

		public virtual object[] GetCustomAttributes(Type attributeType, bool inherit)
		{
			if (IsLegacyParameterInfo)
			{
				return null;
			}
			if (attributeType == null)
			{
				throw new ArgumentNullException("attributeType");
			}
			if (System.Reflection.MetadataToken.IsNullToken(m_tkParamDef))
			{
				return new object[0];
			}
			RuntimeType runtimeType = attributeType.UnderlyingSystemType as RuntimeType;
			if (runtimeType == null)
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_MustBeType"), "attributeType");
			}
			return CustomAttribute.GetCustomAttributes(this, runtimeType);
		}

		public virtual bool IsDefined(Type attributeType, bool inherit)
		{
			if (IsLegacyParameterInfo)
			{
				return false;
			}
			if (attributeType == null)
			{
				throw new ArgumentNullException("attributeType");
			}
			if (System.Reflection.MetadataToken.IsNullToken(m_tkParamDef))
			{
				return false;
			}
			RuntimeType runtimeType = attributeType.UnderlyingSystemType as RuntimeType;
			if (runtimeType == null)
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_MustBeType"), "attributeType");
			}
			return CustomAttribute.IsDefined(this, runtimeType);
		}

		internal void OnCacheClear(object sender, ClearCacheEventArgs cacheEventArgs)
		{
			m_cachedData = null;
		}

		void _ParameterInfo.GetTypeInfoCount(out uint pcTInfo)
		{
			throw new NotImplementedException();
		}

		void _ParameterInfo.GetTypeInfo(uint iTInfo, uint lcid, IntPtr ppTInfo)
		{
			throw new NotImplementedException();
		}

		void _ParameterInfo.GetIDsOfNames([In] ref Guid riid, IntPtr rgszNames, uint cNames, uint lcid, IntPtr rgDispId)
		{
			throw new NotImplementedException();
		}

		void _ParameterInfo.Invoke(uint dispIdMember, [In] ref Guid riid, uint lcid, short wFlags, IntPtr pDispParams, IntPtr pVarResult, IntPtr pExcepInfo, IntPtr puArgErr)
		{
			throw new NotImplementedException();
		}
	}
}
