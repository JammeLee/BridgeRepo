using System.Globalization;
using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace System.Reflection.Emit
{
	[ComVisible(true)]
	[ComDefaultInterface(typeof(_FieldBuilder))]
	[ClassInterface(ClassInterfaceType.None)]
	[HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort = true)]
	public sealed class FieldBuilder : FieldInfo, _FieldBuilder
	{
		private int m_fieldTok;

		private FieldToken m_tkField;

		private TypeBuilder m_typeBuilder;

		private string m_fieldName;

		private FieldAttributes m_Attributes;

		private Type m_fieldType;

		internal byte[] m_data;

		internal override int MetadataTokenInternal => m_fieldTok;

		public override Module Module => m_typeBuilder.Module;

		public override string Name => m_fieldName;

		public override Type DeclaringType
		{
			get
			{
				if (m_typeBuilder.m_isHiddenGlobalType)
				{
					return null;
				}
				return m_typeBuilder;
			}
		}

		public override Type ReflectedType
		{
			get
			{
				if (m_typeBuilder.m_isHiddenGlobalType)
				{
					return null;
				}
				return m_typeBuilder;
			}
		}

		public override Type FieldType => m_fieldType;

		public override RuntimeFieldHandle FieldHandle
		{
			get
			{
				throw new NotSupportedException(Environment.GetResourceString("NotSupported_DynamicModule"));
			}
		}

		public override FieldAttributes Attributes => m_Attributes;

		internal FieldBuilder(TypeBuilder typeBuilder, string fieldName, Type type, Type[] requiredCustomModifiers, Type[] optionalCustomModifiers, FieldAttributes attributes)
		{
			if (fieldName == null)
			{
				throw new ArgumentNullException("fieldName");
			}
			if (fieldName.Length == 0)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_EmptyName"), "fieldName");
			}
			if (fieldName[0] == '\0')
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_IllegalName"), "fieldName");
			}
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}
			if (type == typeof(void))
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_BadFieldType"));
			}
			m_fieldName = fieldName;
			m_typeBuilder = typeBuilder;
			m_fieldType = type;
			m_Attributes = attributes & ~FieldAttributes.ReservedMask;
			SignatureHelper fieldSigHelper = SignatureHelper.GetFieldSigHelper(m_typeBuilder.Module);
			fieldSigHelper.AddArgument(type, requiredCustomModifiers, optionalCustomModifiers);
			int length;
			byte[] signature = fieldSigHelper.InternalGetSignature(out length);
			m_fieldTok = TypeBuilder.InternalDefineField(typeBuilder.TypeToken.Token, fieldName, signature, length, m_Attributes, m_typeBuilder.Module);
			m_tkField = new FieldToken(m_fieldTok, type);
		}

		internal void SetData(byte[] data, int size)
		{
			if (data != null)
			{
				m_data = new byte[data.Length];
				Array.Copy(data, m_data, data.Length);
			}
			m_typeBuilder.Module.InternalSetFieldRVAContent(m_tkField.Token, data, size);
		}

		internal TypeBuilder GetTypeBuilder()
		{
			return m_typeBuilder;
		}

		public override object GetValue(object obj)
		{
			throw new NotSupportedException(Environment.GetResourceString("NotSupported_DynamicModule"));
		}

		public override void SetValue(object obj, object val, BindingFlags invokeAttr, Binder binder, CultureInfo culture)
		{
			throw new NotSupportedException(Environment.GetResourceString("NotSupported_DynamicModule"));
		}

		public override object[] GetCustomAttributes(bool inherit)
		{
			throw new NotSupportedException(Environment.GetResourceString("NotSupported_DynamicModule"));
		}

		public override object[] GetCustomAttributes(Type attributeType, bool inherit)
		{
			throw new NotSupportedException(Environment.GetResourceString("NotSupported_DynamicModule"));
		}

		public override bool IsDefined(Type attributeType, bool inherit)
		{
			throw new NotSupportedException(Environment.GetResourceString("NotSupported_DynamicModule"));
		}

		public FieldToken GetToken()
		{
			return m_tkField;
		}

		public void SetOffset(int iOffset)
		{
			m_typeBuilder.ThrowIfCreated();
			TypeBuilder.InternalSetFieldOffset(m_typeBuilder.Module, GetToken().Token, iOffset);
		}

		[Obsolete("An alternate API is available: Emit the MarshalAs custom attribute instead. http://go.microsoft.com/fwlink/?linkid=14202")]
		public void SetMarshal(UnmanagedMarshal unmanagedMarshal)
		{
			m_typeBuilder.ThrowIfCreated();
			if (unmanagedMarshal == null)
			{
				throw new ArgumentNullException("unmanagedMarshal");
			}
			byte[] array = unmanagedMarshal.InternalGetBytes();
			TypeBuilder.InternalSetMarshalInfo(m_typeBuilder.Module, GetToken().Token, array, array.Length);
		}

		public void SetConstant(object defaultValue)
		{
			m_typeBuilder.ThrowIfCreated();
			TypeBuilder.SetConstantValue(m_typeBuilder.Module, GetToken().Token, m_fieldType, defaultValue);
		}

		[ComVisible(true)]
		public void SetCustomAttribute(ConstructorInfo con, byte[] binaryAttribute)
		{
			ModuleBuilder moduleBuilder = m_typeBuilder.Module as ModuleBuilder;
			m_typeBuilder.ThrowIfCreated();
			if (con == null)
			{
				throw new ArgumentNullException("con");
			}
			if (binaryAttribute == null)
			{
				throw new ArgumentNullException("binaryAttribute");
			}
			TypeBuilder.InternalCreateCustomAttribute(m_tkField.Token, moduleBuilder.GetConstructorToken(con).Token, binaryAttribute, moduleBuilder, toDisk: false);
		}

		public void SetCustomAttribute(CustomAttributeBuilder customBuilder)
		{
			m_typeBuilder.ThrowIfCreated();
			if (customBuilder == null)
			{
				throw new ArgumentNullException("customBuilder");
			}
			ModuleBuilder mod = m_typeBuilder.Module as ModuleBuilder;
			customBuilder.CreateCustomAttribute(mod, m_tkField.Token);
		}

		void _FieldBuilder.GetTypeInfoCount(out uint pcTInfo)
		{
			throw new NotImplementedException();
		}

		void _FieldBuilder.GetTypeInfo(uint iTInfo, uint lcid, IntPtr ppTInfo)
		{
			throw new NotImplementedException();
		}

		void _FieldBuilder.GetIDsOfNames([In] ref Guid riid, IntPtr rgszNames, uint cNames, uint lcid, IntPtr rgDispId)
		{
			throw new NotImplementedException();
		}

		void _FieldBuilder.Invoke(uint dispIdMember, [In] ref Guid riid, uint lcid, short wFlags, IntPtr pDispParams, IntPtr pVarResult, IntPtr pExcepInfo, IntPtr puArgErr)
		{
			throw new NotImplementedException();
		}
	}
}
