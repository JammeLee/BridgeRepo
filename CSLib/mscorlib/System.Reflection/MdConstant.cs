namespace System.Reflection
{
	internal static class MdConstant
	{
		public unsafe static object GetValue(MetadataImport scope, int token, RuntimeTypeHandle fieldTypeHandle, bool raw)
		{
			CorElementType corElementType = CorElementType.End;
			long value = 0L;
			scope.GetDefaultValue(token, out value, out var length, out corElementType);
			Type typeFromHandle = Type.GetTypeFromHandle(fieldTypeHandle);
			if (typeFromHandle.IsEnum && !raw)
			{
				long num = 0L;
				switch (corElementType)
				{
				case CorElementType.Void:
					return DBNull.Value;
				case CorElementType.Char:
					num = *(ushort*)(&value);
					break;
				case CorElementType.I1:
					num = *(sbyte*)(&value);
					break;
				case CorElementType.U1:
					num = *(byte*)(&value);
					break;
				case CorElementType.I2:
					num = *(short*)(&value);
					break;
				case CorElementType.U2:
					num = *(ushort*)(&value);
					break;
				case CorElementType.I4:
					num = *(int*)(&value);
					break;
				case CorElementType.U4:
					num = *(uint*)(&value);
					break;
				case CorElementType.I8:
					num = value;
					break;
				case CorElementType.U8:
					num = value;
					break;
				default:
					throw new FormatException(Environment.GetResourceString("Arg_BadLiteralFormat"));
				}
				return RuntimeType.CreateEnum(fieldTypeHandle, num);
			}
			if (typeFromHandle == typeof(DateTime))
			{
				long num2 = 0L;
				switch (corElementType)
				{
				case CorElementType.Void:
					return DBNull.Value;
				case CorElementType.I8:
					num2 = value;
					break;
				case CorElementType.U8:
					num2 = value;
					break;
				default:
					throw new FormatException(Environment.GetResourceString("Arg_BadLiteralFormat"));
				}
				return new DateTime(num2);
			}
			return corElementType switch
			{
				CorElementType.Void => DBNull.Value, 
				CorElementType.Char => (char)(*(ushort*)(&value)), 
				CorElementType.I1 => *(sbyte*)(&value), 
				CorElementType.U1 => *(byte*)(&value), 
				CorElementType.I2 => *(short*)(&value), 
				CorElementType.U2 => *(ushort*)(&value), 
				CorElementType.I4 => *(int*)(&value), 
				CorElementType.U4 => *(uint*)(&value), 
				CorElementType.I8 => value, 
				CorElementType.U8 => (ulong)value, 
				CorElementType.Boolean => *(byte*)(&value) != 0, 
				CorElementType.R4 => *(float*)(&value), 
				CorElementType.R8 => *(double*)(&value), 
				CorElementType.String => new string((char*)value, 0, length / 2), 
				CorElementType.Class => null, 
				_ => throw new FormatException(Environment.GetResourceString("Arg_BadLiteralFormat")), 
			};
		}
	}
}
