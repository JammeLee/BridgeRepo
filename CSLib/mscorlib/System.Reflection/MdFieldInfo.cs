using System.Diagnostics;
using System.Globalization;
using System.Runtime.ConstrainedExecution;
using System.Runtime.Serialization;

namespace System.Reflection
{
	[Serializable]
	internal sealed class MdFieldInfo : RuntimeFieldInfo, ISerializable
	{
		private int m_tkField;

		private string m_name;

		private Type m_fieldType;

		private FieldAttributes m_fieldAttributes;

		public override string Name
		{
			get
			{
				if (m_name == null)
				{
					m_name = Module.MetadataImport.GetName(m_tkField).ToString();
				}
				return m_name;
			}
		}

		public override int MetadataToken => m_tkField;

		public override Module Module => m_declaringType.Module;

		public override RuntimeFieldHandle FieldHandle
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		public override FieldAttributes Attributes => m_fieldAttributes;

		public unsafe override Type FieldType
		{
			get
			{
				if (m_fieldType == null)
				{
					ConstArray sigOfFieldDef = Module.MetadataImport.GetSigOfFieldDef(m_tkField);
					m_fieldType = new Signature(sigOfFieldDef.Signature.ToPointer(), sigOfFieldDef.Length, m_declaringType.GetTypeHandleInternal()).FieldTypeHandle.GetRuntimeType();
				}
				return m_fieldType;
			}
		}

		internal MdFieldInfo(int tkField, FieldAttributes fieldAttributes, RuntimeTypeHandle declaringTypeHandle, RuntimeType.RuntimeTypeCache reflectedTypeCache, BindingFlags bindingFlags)
			: base(reflectedTypeCache, declaringTypeHandle.GetRuntimeType(), bindingFlags)
		{
			m_tkField = tkField;
			m_name = null;
			m_fieldAttributes = fieldAttributes;
		}

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		internal override bool CacheEquals(object o)
		{
			MdFieldInfo mdFieldInfo = o as MdFieldInfo;
			if (mdFieldInfo == null)
			{
				return false;
			}
			if (mdFieldInfo.m_tkField == m_tkField)
			{
				return m_declaringType.GetTypeHandleInternal().GetModuleHandle().Equals(mdFieldInfo.m_declaringType.GetTypeHandleInternal().GetModuleHandle());
			}
			return false;
		}

		[DebuggerStepThrough]
		[DebuggerHidden]
		public override object GetValueDirect(TypedReference obj)
		{
			return GetValue(null);
		}

		[DebuggerHidden]
		[DebuggerStepThrough]
		public override void SetValueDirect(TypedReference obj, object value)
		{
			throw new FieldAccessException(Environment.GetResourceString("Acc_ReadOnly"));
		}

		[DebuggerStepThrough]
		[DebuggerHidden]
		public override object GetValue(object obj)
		{
			return GetValue(raw: false);
		}

		public override object GetRawConstantValue()
		{
			return GetValue(raw: true);
		}

		internal object GetValue(bool raw)
		{
			object value = MdConstant.GetValue(Module.MetadataImport, m_tkField, FieldType.GetTypeHandleInternal(), raw);
			if (value == DBNull.Value)
			{
				throw new NotSupportedException(Environment.GetResourceString("Arg_EnumLitValueNotFound"));
			}
			return value;
		}

		[DebuggerStepThrough]
		[DebuggerHidden]
		public override void SetValue(object obj, object value, BindingFlags invokeAttr, Binder binder, CultureInfo culture)
		{
			throw new FieldAccessException(Environment.GetResourceString("Acc_ReadOnly"));
		}

		public override Type[] GetRequiredCustomModifiers()
		{
			return new Type[0];
		}

		public override Type[] GetOptionalCustomModifiers()
		{
			return new Type[0];
		}
	}
}
