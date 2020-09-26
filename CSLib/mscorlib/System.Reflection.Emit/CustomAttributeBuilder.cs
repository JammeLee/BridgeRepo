using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Text;

namespace System.Reflection.Emit
{
	[ClassInterface(ClassInterfaceType.None)]
	[ComVisible(true)]
	[ComDefaultInterface(typeof(_CustomAttributeBuilder))]
	[HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort = true)]
	public class CustomAttributeBuilder : _CustomAttributeBuilder
	{
		private const byte SERIALIZATION_TYPE_BOOLEAN = 2;

		private const byte SERIALIZATION_TYPE_CHAR = 3;

		private const byte SERIALIZATION_TYPE_I1 = 4;

		private const byte SERIALIZATION_TYPE_U1 = 5;

		private const byte SERIALIZATION_TYPE_I2 = 6;

		private const byte SERIALIZATION_TYPE_U2 = 7;

		private const byte SERIALIZATION_TYPE_I4 = 8;

		private const byte SERIALIZATION_TYPE_U4 = 9;

		private const byte SERIALIZATION_TYPE_I8 = 10;

		private const byte SERIALIZATION_TYPE_U8 = 11;

		private const byte SERIALIZATION_TYPE_R4 = 12;

		private const byte SERIALIZATION_TYPE_R8 = 13;

		private const byte SERIALIZATION_TYPE_STRING = 14;

		private const byte SERIALIZATION_TYPE_SZARRAY = 29;

		private const byte SERIALIZATION_TYPE_TYPE = 80;

		private const byte SERIALIZATION_TYPE_TAGGED_OBJECT = 81;

		private const byte SERIALIZATION_TYPE_FIELD = 83;

		private const byte SERIALIZATION_TYPE_PROPERTY = 84;

		private const byte SERIALIZATION_TYPE_ENUM = 85;

		internal ConstructorInfo m_con;

		internal object[] m_constructorArgs;

		internal byte[] m_blob;

		public CustomAttributeBuilder(ConstructorInfo con, object[] constructorArgs)
		{
			InitCustomAttributeBuilder(con, constructorArgs, new PropertyInfo[0], new object[0], new FieldInfo[0], new object[0]);
		}

		public CustomAttributeBuilder(ConstructorInfo con, object[] constructorArgs, PropertyInfo[] namedProperties, object[] propertyValues)
		{
			InitCustomAttributeBuilder(con, constructorArgs, namedProperties, propertyValues, new FieldInfo[0], new object[0]);
		}

		public CustomAttributeBuilder(ConstructorInfo con, object[] constructorArgs, FieldInfo[] namedFields, object[] fieldValues)
		{
			InitCustomAttributeBuilder(con, constructorArgs, new PropertyInfo[0], new object[0], namedFields, fieldValues);
		}

		public CustomAttributeBuilder(ConstructorInfo con, object[] constructorArgs, PropertyInfo[] namedProperties, object[] propertyValues, FieldInfo[] namedFields, object[] fieldValues)
		{
			InitCustomAttributeBuilder(con, constructorArgs, namedProperties, propertyValues, namedFields, fieldValues);
		}

		private bool ValidateType(Type t)
		{
			if (t.IsPrimitive || t == typeof(string) || t == typeof(Type))
			{
				return true;
			}
			if (t.IsEnum)
			{
				switch (Type.GetTypeCode(Enum.GetUnderlyingType(t)))
				{
				case TypeCode.SByte:
				case TypeCode.Byte:
				case TypeCode.Int16:
				case TypeCode.UInt16:
				case TypeCode.Int32:
				case TypeCode.UInt32:
				case TypeCode.Int64:
				case TypeCode.UInt64:
					return true;
				default:
					return false;
				}
			}
			if (t.IsArray)
			{
				if (t.GetArrayRank() != 1)
				{
					return false;
				}
				return ValidateType(t.GetElementType());
			}
			return t == typeof(object);
		}

		internal void InitCustomAttributeBuilder(ConstructorInfo con, object[] constructorArgs, PropertyInfo[] namedProperties, object[] propertyValues, FieldInfo[] namedFields, object[] fieldValues)
		{
			if (con == null)
			{
				throw new ArgumentNullException("con");
			}
			if (constructorArgs == null)
			{
				throw new ArgumentNullException("constructorArgs");
			}
			if (namedProperties == null)
			{
				throw new ArgumentNullException("constructorArgs");
			}
			if (propertyValues == null)
			{
				throw new ArgumentNullException("propertyValues");
			}
			if (namedFields == null)
			{
				throw new ArgumentNullException("namedFields");
			}
			if (fieldValues == null)
			{
				throw new ArgumentNullException("fieldValues");
			}
			if (namedProperties.Length != propertyValues.Length)
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_ArrayLengthsDiffer"), "namedProperties, propertyValues");
			}
			if (namedFields.Length != fieldValues.Length)
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_ArrayLengthsDiffer"), "namedFields, fieldValues");
			}
			if ((con.Attributes & MethodAttributes.Static) == MethodAttributes.Static || (con.Attributes & MethodAttributes.MemberAccessMask) == MethodAttributes.Private)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_BadConstructor"));
			}
			if ((con.CallingConvention & CallingConventions.Standard) != CallingConventions.Standard)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_BadConstructorCallConv"));
			}
			m_con = con;
			m_constructorArgs = new object[constructorArgs.Length];
			Array.Copy(constructorArgs, m_constructorArgs, constructorArgs.Length);
			Type[] array;
			if (con is ConstructorBuilder)
			{
				array = ((ConstructorBuilder)con).GetParameterTypes();
			}
			else
			{
				ParameterInfo[] parametersNoCopy = con.GetParametersNoCopy();
				array = new Type[parametersNoCopy.Length];
				for (int i = 0; i < parametersNoCopy.Length; i++)
				{
					array[i] = parametersNoCopy[i].ParameterType;
				}
			}
			if (array.Length != constructorArgs.Length)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_BadParameterCountsForConstructor"));
			}
			for (int i = 0; i < array.Length; i++)
			{
				if (!ValidateType(array[i]))
				{
					throw new ArgumentException(Environment.GetResourceString("Argument_BadTypeInCustomAttribute"));
				}
			}
			for (int i = 0; i < array.Length; i++)
			{
				if (constructorArgs[i] != null)
				{
					TypeCode typeCode = Type.GetTypeCode(array[i]);
					if (typeCode != Type.GetTypeCode(constructorArgs[i].GetType()) && (typeCode != TypeCode.Object || !ValidateType(constructorArgs[i].GetType())))
					{
						throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Argument_BadParameterTypeForConstructor"), i));
					}
				}
			}
			MemoryStream output = new MemoryStream();
			BinaryWriter binaryWriter = new BinaryWriter(output);
			binaryWriter.Write((ushort)1);
			for (int i = 0; i < constructorArgs.Length; i++)
			{
				EmitValue(binaryWriter, array[i], constructorArgs[i]);
			}
			binaryWriter.Write((ushort)(namedProperties.Length + namedFields.Length));
			for (int i = 0; i < namedProperties.Length; i++)
			{
				if (namedProperties[i] == null)
				{
					throw new ArgumentNullException("namedProperties[" + i + "]");
				}
				Type propertyType = namedProperties[i].PropertyType;
				if (propertyValues[i] == null && propertyType.IsPrimitive)
				{
					throw new ArgumentNullException("propertyValues[" + i + "]");
				}
				if (!ValidateType(propertyType))
				{
					throw new ArgumentException(Environment.GetResourceString("Argument_BadTypeInCustomAttribute"));
				}
				if (!namedProperties[i].CanWrite)
				{
					throw new ArgumentException(Environment.GetResourceString("Argument_NotAWritableProperty"));
				}
				if (namedProperties[i].DeclaringType != con.DeclaringType && !(con.DeclaringType is TypeBuilderInstantiation) && !con.DeclaringType.IsSubclassOf(namedProperties[i].DeclaringType) && !TypeBuilder.IsTypeEqual(namedProperties[i].DeclaringType, con.DeclaringType) && (!(namedProperties[i].DeclaringType is TypeBuilder) || !con.DeclaringType.IsSubclassOf(((TypeBuilder)namedProperties[i].DeclaringType).m_runtimeType)))
				{
					throw new ArgumentException(Environment.GetResourceString("Argument_BadPropertyForConstructorBuilder"));
				}
				if (propertyValues[i] != null && propertyType != typeof(object) && Type.GetTypeCode(propertyValues[i].GetType()) != Type.GetTypeCode(propertyType))
				{
					throw new ArgumentException(Environment.GetResourceString("Argument_ConstantDoesntMatch"));
				}
				binaryWriter.Write((byte)84);
				EmitType(binaryWriter, propertyType);
				EmitString(binaryWriter, namedProperties[i].Name);
				EmitValue(binaryWriter, propertyType, propertyValues[i]);
			}
			for (int i = 0; i < namedFields.Length; i++)
			{
				if (namedFields[i] == null)
				{
					throw new ArgumentNullException("namedFields[" + i + "]");
				}
				Type fieldType = namedFields[i].FieldType;
				if (fieldValues[i] == null && fieldType.IsPrimitive)
				{
					throw new ArgumentNullException("fieldValues[" + i + "]");
				}
				if (!ValidateType(fieldType))
				{
					throw new ArgumentException(Environment.GetResourceString("Argument_BadTypeInCustomAttribute"));
				}
				if (namedFields[i].DeclaringType != con.DeclaringType && !(con.DeclaringType is TypeBuilderInstantiation) && !con.DeclaringType.IsSubclassOf(namedFields[i].DeclaringType) && !TypeBuilder.IsTypeEqual(namedFields[i].DeclaringType, con.DeclaringType) && (!(namedFields[i].DeclaringType is TypeBuilder) || !con.DeclaringType.IsSubclassOf(((TypeBuilder)namedFields[i].DeclaringType).m_runtimeType)))
				{
					throw new ArgumentException(Environment.GetResourceString("Argument_BadFieldForConstructorBuilder"));
				}
				if (fieldValues[i] != null && fieldType != typeof(object) && Type.GetTypeCode(fieldValues[i].GetType()) != Type.GetTypeCode(fieldType))
				{
					throw new ArgumentException(Environment.GetResourceString("Argument_ConstantDoesntMatch"));
				}
				binaryWriter.Write((byte)83);
				EmitType(binaryWriter, fieldType);
				EmitString(binaryWriter, namedFields[i].Name);
				EmitValue(binaryWriter, fieldType, fieldValues[i]);
			}
			m_blob = ((MemoryStream)binaryWriter.BaseStream).ToArray();
		}

		private void EmitType(BinaryWriter writer, Type type)
		{
			if (type.IsPrimitive)
			{
				switch (Type.GetTypeCode(type))
				{
				case TypeCode.SByte:
					writer.Write((byte)4);
					break;
				case TypeCode.Byte:
					writer.Write((byte)5);
					break;
				case TypeCode.Char:
					writer.Write((byte)3);
					break;
				case TypeCode.Boolean:
					writer.Write((byte)2);
					break;
				case TypeCode.Int16:
					writer.Write((byte)6);
					break;
				case TypeCode.UInt16:
					writer.Write((byte)7);
					break;
				case TypeCode.Int32:
					writer.Write((byte)8);
					break;
				case TypeCode.UInt32:
					writer.Write((byte)9);
					break;
				case TypeCode.Int64:
					writer.Write((byte)10);
					break;
				case TypeCode.UInt64:
					writer.Write((byte)11);
					break;
				case TypeCode.Single:
					writer.Write((byte)12);
					break;
				case TypeCode.Double:
					writer.Write((byte)13);
					break;
				}
			}
			else if (type.IsEnum)
			{
				writer.Write((byte)85);
				EmitString(writer, type.AssemblyQualifiedName);
			}
			else if (type == typeof(string))
			{
				writer.Write((byte)14);
			}
			else if (type == typeof(Type))
			{
				writer.Write((byte)80);
			}
			else if (type.IsArray)
			{
				writer.Write((byte)29);
				EmitType(writer, type.GetElementType());
			}
			else
			{
				writer.Write((byte)81);
			}
		}

		private void EmitString(BinaryWriter writer, string str)
		{
			byte[] bytes = Encoding.UTF8.GetBytes(str);
			uint num = (uint)bytes.Length;
			if (num <= 127)
			{
				writer.Write((byte)num);
			}
			else if (num <= 16383)
			{
				writer.Write((byte)((num >> 8) | 0x80u));
				writer.Write((byte)(num & 0xFFu));
			}
			else
			{
				writer.Write((byte)((num >> 24) | 0xC0u));
				writer.Write((byte)((num >> 16) & 0xFFu));
				writer.Write((byte)((num >> 8) & 0xFFu));
				writer.Write((byte)(num & 0xFFu));
			}
			writer.Write(bytes);
		}

		private void EmitValue(BinaryWriter writer, Type type, object value)
		{
			if (type.IsEnum)
			{
				switch (Type.GetTypeCode(Enum.GetUnderlyingType(type)))
				{
				case TypeCode.SByte:
					writer.Write((sbyte)value);
					break;
				case TypeCode.Byte:
					writer.Write((byte)value);
					break;
				case TypeCode.Int16:
					writer.Write((short)value);
					break;
				case TypeCode.UInt16:
					writer.Write((ushort)value);
					break;
				case TypeCode.Int32:
					writer.Write((int)value);
					break;
				case TypeCode.UInt32:
					writer.Write((uint)value);
					break;
				case TypeCode.Int64:
					writer.Write((long)value);
					break;
				case TypeCode.UInt64:
					writer.Write((ulong)value);
					break;
				}
				return;
			}
			if (type == typeof(string))
			{
				if (value == null)
				{
					writer.Write(byte.MaxValue);
				}
				else
				{
					EmitString(writer, (string)value);
				}
				return;
			}
			if (type == typeof(Type))
			{
				if (value == null)
				{
					writer.Write(byte.MaxValue);
					return;
				}
				string text = TypeNameBuilder.ToString((Type)value, TypeNameBuilder.Format.AssemblyQualifiedName);
				if (text == null)
				{
					throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Argument_InvalidTypeForCA"), value.GetType()));
				}
				EmitString(writer, text);
				return;
			}
			if (type.IsArray)
			{
				if (value == null)
				{
					writer.Write(uint.MaxValue);
					return;
				}
				Array array = (Array)value;
				Type elementType = type.GetElementType();
				writer.Write(array.Length);
				for (int i = 0; i < array.Length; i++)
				{
					EmitValue(writer, elementType, array.GetValue(i));
				}
				return;
			}
			if (type.IsPrimitive)
			{
				switch (Type.GetTypeCode(type))
				{
				case TypeCode.SByte:
					writer.Write((sbyte)value);
					break;
				case TypeCode.Byte:
					writer.Write((byte)value);
					break;
				case TypeCode.Char:
					writer.Write(Convert.ToInt16((char)value));
					break;
				case TypeCode.Boolean:
					writer.Write((byte)(((bool)value) ? 1u : 0u));
					break;
				case TypeCode.Int16:
					writer.Write((short)value);
					break;
				case TypeCode.UInt16:
					writer.Write((ushort)value);
					break;
				case TypeCode.Int32:
					writer.Write((int)value);
					break;
				case TypeCode.UInt32:
					writer.Write((uint)value);
					break;
				case TypeCode.Int64:
					writer.Write((long)value);
					break;
				case TypeCode.UInt64:
					writer.Write((ulong)value);
					break;
				case TypeCode.Single:
					writer.Write((float)value);
					break;
				case TypeCode.Double:
					writer.Write((double)value);
					break;
				}
				return;
			}
			if (type == typeof(object))
			{
				Type type2 = ((value == null) ? typeof(string) : ((value is Type) ? typeof(Type) : value.GetType()));
				EmitType(writer, type2);
				EmitValue(writer, type2, value);
				return;
			}
			string text2 = "null";
			if (value != null)
			{
				text2 = value.GetType().ToString();
			}
			throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Argument_BadParameterTypeForCAB"), text2));
		}

		internal void CreateCustomAttribute(ModuleBuilder mod, int tkOwner)
		{
			CreateCustomAttribute(mod, tkOwner, mod.GetConstructorToken(m_con).Token, toDisk: false);
		}

		internal int PrepareCreateCustomAttributeToDisk(ModuleBuilder mod)
		{
			return mod.InternalGetConstructorToken(m_con, usingRef: true).Token;
		}

		internal void CreateCustomAttribute(ModuleBuilder mod, int tkOwner, int tkAttrib, bool toDisk)
		{
			TypeBuilder.InternalCreateCustomAttribute(tkOwner, tkAttrib, m_blob, mod, toDisk, typeof(DebuggableAttribute) == m_con.DeclaringType);
		}

		void _CustomAttributeBuilder.GetTypeInfoCount(out uint pcTInfo)
		{
			throw new NotImplementedException();
		}

		void _CustomAttributeBuilder.GetTypeInfo(uint iTInfo, uint lcid, IntPtr ppTInfo)
		{
			throw new NotImplementedException();
		}

		void _CustomAttributeBuilder.GetIDsOfNames([In] ref Guid riid, IntPtr rgszNames, uint cNames, uint lcid, IntPtr rgDispId)
		{
			throw new NotImplementedException();
		}

		void _CustomAttributeBuilder.Invoke(uint dispIdMember, [In] ref Guid riid, uint lcid, short wFlags, IntPtr pDispParams, IntPtr pVarResult, IntPtr pExcepInfo, IntPtr puArgErr)
		{
			throw new NotImplementedException();
		}
	}
}
