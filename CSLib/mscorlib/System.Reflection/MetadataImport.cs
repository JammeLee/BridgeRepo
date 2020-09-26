using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System.Reflection
{
	internal struct MetadataImport
	{
		private IntPtr m_metadataImport2;

		internal static readonly MetadataImport EmptyImport = new MetadataImport((IntPtr)0);

		internal static Guid IID_IMetaDataImport = new Guid(3530420970u, 32600, 16771, 134, 190, 48, 174, 41, 167, 93, 141);

		internal static Guid IID_IMetaDataAssemblyImport = new Guid(3999418123u, 59723, 16974, 155, 124, 47, 0, 201, 36, 159, 147);

		internal static Guid IID_IMetaDataTables = new Guid(3639966123u, 16429, 19342, 130, 217, 93, 99, 177, 6, 92, 104);

		public override int GetHashCode()
		{
			return m_metadataImport2.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			if (!(obj is MetadataImport))
			{
				return false;
			}
			return Equals((MetadataImport)obj);
		}

		internal bool Equals(MetadataImport import)
		{
			return import.m_metadataImport2 == m_metadataImport2;
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void _GetMarshalAs(IntPtr pNativeType, int cNativeType, out int unmanagedType, out int safeArraySubType, out string safeArrayUserDefinedSubType, out int arraySubType, out int sizeParamIndex, out int sizeConst, out string marshalType, out string marshalCookie, out int iidParamIndex);

		internal static void GetMarshalAs(ConstArray nativeType, out UnmanagedType unmanagedType, out VarEnum safeArraySubType, out string safeArrayUserDefinedSubType, out UnmanagedType arraySubType, out int sizeParamIndex, out int sizeConst, out string marshalType, out string marshalCookie, out int iidParamIndex)
		{
			_GetMarshalAs(nativeType.Signature, nativeType.Length, out var unmanagedType2, out var safeArraySubType2, out safeArrayUserDefinedSubType, out var arraySubType2, out sizeParamIndex, out sizeConst, out marshalType, out marshalCookie, out iidParamIndex);
			unmanagedType = (UnmanagedType)unmanagedType2;
			safeArraySubType = (VarEnum)safeArraySubType2;
			arraySubType = (UnmanagedType)arraySubType2;
		}

		internal static void ThrowError(int hResult)
		{
			throw new MetadataException(hResult);
		}

		internal MetadataImport(IntPtr metadataImport2)
		{
			m_metadataImport2 = metadataImport2;
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private unsafe static extern void _Enum(IntPtr scope, out MetadataArgs.SkipAddresses skipAddresses, int type, int parent, int* result, int count);

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern int _EnumCount(IntPtr scope, out MetadataArgs.SkipAddresses skipAddresses, int type, int parent, out int count);

		public unsafe void Enum(int type, int parent, int* result, int count)
		{
			_Enum(m_metadataImport2, out MetadataArgs.Skip, type, parent, result, count);
		}

		public int EnumCount(int type, int parent)
		{
			int count = 0;
			_EnumCount(m_metadataImport2, out MetadataArgs.Skip, type, parent, out count);
			return count;
		}

		public unsafe void EnumNestedTypes(int mdTypeDef, int* result, int count)
		{
			Enum(33554432, mdTypeDef, result, count);
		}

		public int EnumNestedTypesCount(int mdTypeDef)
		{
			return EnumCount(33554432, mdTypeDef);
		}

		public unsafe void EnumCustomAttributes(int mdToken, int* result, int count)
		{
			Enum(201326592, mdToken, result, count);
		}

		public int EnumCustomAttributesCount(int mdToken)
		{
			return EnumCount(201326592, mdToken);
		}

		public unsafe void EnumParams(int mdMethodDef, int* result, int count)
		{
			Enum(134217728, mdMethodDef, result, count);
		}

		public int EnumParamsCount(int mdMethodDef)
		{
			return EnumCount(134217728, mdMethodDef);
		}

		public unsafe void GetAssociates(int mdPropEvent, AssociateRecord* result, int count)
		{
			int* ptr = (int*)stackalloc byte[4 * (count * 2)];
			Enum(100663296, mdPropEvent, ptr, count);
			for (int i = 0; i < count; i++)
			{
				result[i].MethodDefToken = ptr[i * 2];
				result[i].Semantics = (MethodSemanticsAttributes)ptr[i * 2 + 1];
			}
		}

		public int GetAssociatesCount(int mdPropEvent)
		{
			return EnumCount(100663296, mdPropEvent);
		}

		public unsafe void EnumFields(int mdTypeDef, int* result, int count)
		{
			Enum(67108864, mdTypeDef, result, count);
		}

		public int EnumFieldsCount(int mdTypeDef)
		{
			return EnumCount(67108864, mdTypeDef);
		}

		public unsafe void EnumProperties(int mdTypeDef, int* result, int count)
		{
			Enum(385875968, mdTypeDef, result, count);
		}

		public int EnumPropertiesCount(int mdTypeDef)
		{
			return EnumCount(385875968, mdTypeDef);
		}

		public unsafe void EnumEvents(int mdTypeDef, int* result, int count)
		{
			Enum(335544320, mdTypeDef, result, count);
		}

		public int EnumEventsCount(int mdTypeDef)
		{
			return EnumCount(335544320, mdTypeDef);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void _GetDefaultValue(IntPtr scope, out MetadataArgs.SkipAddresses skipAddresses, int mdToken, out long value, out int length, out int corElementType);

		public void GetDefaultValue(int mdToken, out long value, out int length, out CorElementType corElementType)
		{
			_GetDefaultValue(m_metadataImport2, out MetadataArgs.Skip, mdToken, out value, out length, out var corElementType2);
			corElementType = (CorElementType)corElementType2;
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private unsafe static extern void _GetUserString(IntPtr scope, out MetadataArgs.SkipAddresses skipAddresses, int mdToken, void** name, out int length);

		public unsafe string GetUserString(int mdToken)
		{
			void* ptr = default(void*);
			_GetUserString(m_metadataImport2, out MetadataArgs.Skip, mdToken, &ptr, out var length);
			if (ptr == null)
			{
				return null;
			}
			char[] array = new char[length];
			for (int i = 0; i < length; i++)
			{
				array[i] = (char)(*(ushort*)((byte*)ptr + (nint)i * (nint)2));
			}
			return new string(array);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private unsafe static extern void _GetName(IntPtr scope, out MetadataArgs.SkipAddresses skipAddresses, int mdToken, void** name);

		public unsafe Utf8String GetName(int mdToken)
		{
			void* pStringHeap = default(void*);
			_GetName(m_metadataImport2, out MetadataArgs.Skip, mdToken, &pStringHeap);
			return new Utf8String(pStringHeap);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private unsafe static extern void _GetNamespace(IntPtr scope, out MetadataArgs.SkipAddresses skipAddresses, int mdToken, void** namesp);

		public unsafe Utf8String GetNamespace(int mdToken)
		{
			void* pStringHeap = default(void*);
			_GetNamespace(m_metadataImport2, out MetadataArgs.Skip, mdToken, &pStringHeap);
			return new Utf8String(pStringHeap);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private unsafe static extern void _GetEventProps(IntPtr scope, out MetadataArgs.SkipAddresses skipAddresses, int mdToken, void** name, out int eventAttributes);

		public unsafe void GetEventProps(int mdToken, out void* name, out EventAttributes eventAttributes)
		{
			void* ptr = default(void*);
			_GetEventProps(m_metadataImport2, out MetadataArgs.Skip, mdToken, &ptr, out var eventAttributes2);
			name = ptr;
			eventAttributes = (EventAttributes)eventAttributes2;
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void _GetFieldDefProps(IntPtr scope, out MetadataArgs.SkipAddresses skipAddresses, int mdToken, out int fieldAttributes);

		public void GetFieldDefProps(int mdToken, out FieldAttributes fieldAttributes)
		{
			_GetFieldDefProps(m_metadataImport2, out MetadataArgs.Skip, mdToken, out var fieldAttributes2);
			fieldAttributes = (FieldAttributes)fieldAttributes2;
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private unsafe static extern void _GetPropertyProps(IntPtr scope, out MetadataArgs.SkipAddresses skipAddresses, int mdToken, void** name, out int propertyAttributes, out ConstArray signature);

		public unsafe void GetPropertyProps(int mdToken, out void* name, out PropertyAttributes propertyAttributes, out ConstArray signature)
		{
			void* ptr = default(void*);
			_GetPropertyProps(m_metadataImport2, out MetadataArgs.Skip, mdToken, &ptr, out var propertyAttributes2, out signature);
			name = ptr;
			propertyAttributes = (PropertyAttributes)propertyAttributes2;
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void _GetParentToken(IntPtr scope, out MetadataArgs.SkipAddresses skipAddresses, int mdToken, out int tkParent);

		public int GetParentToken(int tkToken)
		{
			_GetParentToken(m_metadataImport2, out MetadataArgs.Skip, tkToken, out var tkParent);
			return tkParent;
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void _GetParamDefProps(IntPtr scope, out MetadataArgs.SkipAddresses skipAddresses, int parameterToken, out int sequence, out int attributes);

		public void GetParamDefProps(int parameterToken, out int sequence, out ParameterAttributes attributes)
		{
			_GetParamDefProps(m_metadataImport2, out MetadataArgs.Skip, parameterToken, out sequence, out var attributes2);
			attributes = (ParameterAttributes)attributes2;
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void _GetGenericParamProps(IntPtr scope, out MetadataArgs.SkipAddresses skipAddresses, int genericParameter, out int flags);

		public void GetGenericParamProps(int genericParameter, out GenericParameterAttributes attributes)
		{
			_GetGenericParamProps(m_metadataImport2, out MetadataArgs.Skip, genericParameter, out var flags);
			attributes = (GenericParameterAttributes)flags;
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void _GetScopeProps(IntPtr scope, out MetadataArgs.SkipAddresses skipAddresses, out Guid mvid);

		public void GetScopeProps(out Guid mvid)
		{
			_GetScopeProps(m_metadataImport2, out MetadataArgs.Skip, out mvid);
		}

		public ConstArray GetMethodSignature(MetadataToken token)
		{
			if (token.IsMemberRef)
			{
				return GetMemberRefProps(token);
			}
			return GetSigOfMethodDef(token);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void _GetSigOfMethodDef(IntPtr scope, out MetadataArgs.SkipAddresses skipAddresses, int methodToken, ref ConstArray signature);

		public ConstArray GetSigOfMethodDef(int methodToken)
		{
			ConstArray signature = default(ConstArray);
			_GetSigOfMethodDef(m_metadataImport2, out MetadataArgs.Skip, methodToken, ref signature);
			return signature;
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void _GetSignatureFromToken(IntPtr scope, out MetadataArgs.SkipAddresses skipAddresses, int methodToken, ref ConstArray signature);

		public ConstArray GetSignatureFromToken(int token)
		{
			ConstArray signature = default(ConstArray);
			_GetSignatureFromToken(m_metadataImport2, out MetadataArgs.Skip, token, ref signature);
			return signature;
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void _GetMemberRefProps(IntPtr scope, out MetadataArgs.SkipAddresses skipAddresses, int memberTokenRef, out ConstArray signature);

		public ConstArray GetMemberRefProps(int memberTokenRef)
		{
			ConstArray signature = default(ConstArray);
			_GetMemberRefProps(m_metadataImport2, out MetadataArgs.Skip, memberTokenRef, out signature);
			return signature;
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void _GetCustomAttributeProps(IntPtr scope, out MetadataArgs.SkipAddresses skipAddresses, int customAttributeToken, out int constructorToken, out ConstArray signature);

		public void GetCustomAttributeProps(int customAttributeToken, out int constructorToken, out ConstArray signature)
		{
			_GetCustomAttributeProps(m_metadataImport2, out MetadataArgs.Skip, customAttributeToken, out constructorToken, out signature);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void _GetClassLayout(IntPtr scope, out MetadataArgs.SkipAddresses skipAddresses, int typeTokenDef, out int packSize, out int classSize);

		public void GetClassLayout(int typeTokenDef, out int packSize, out int classSize)
		{
			_GetClassLayout(m_metadataImport2, out MetadataArgs.Skip, typeTokenDef, out packSize, out classSize);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern bool _GetFieldOffset(IntPtr scope, out MetadataArgs.SkipAddresses skipAddresses, int typeTokenDef, int fieldTokenDef, out int offset);

		public bool GetFieldOffset(int typeTokenDef, int fieldTokenDef, out int offset)
		{
			return _GetFieldOffset(m_metadataImport2, out MetadataArgs.Skip, typeTokenDef, fieldTokenDef, out offset);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void _GetSigOfFieldDef(IntPtr scope, out MetadataArgs.SkipAddresses skipAddresses, int fieldToken, ref ConstArray fieldMarshal);

		public ConstArray GetSigOfFieldDef(int fieldToken)
		{
			ConstArray fieldMarshal = default(ConstArray);
			_GetSigOfFieldDef(m_metadataImport2, out MetadataArgs.Skip, fieldToken, ref fieldMarshal);
			return fieldMarshal;
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void _GetFieldMarshal(IntPtr scope, out MetadataArgs.SkipAddresses skipAddresses, int fieldToken, ref ConstArray fieldMarshal);

		public ConstArray GetFieldMarshal(int fieldToken)
		{
			ConstArray fieldMarshal = default(ConstArray);
			_GetFieldMarshal(m_metadataImport2, out MetadataArgs.Skip, fieldToken, ref fieldMarshal);
			return fieldMarshal;
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private unsafe static extern void _GetPInvokeMap(IntPtr scope, out MetadataArgs.SkipAddresses skipAddresses, int token, out int attributes, void** importName, void** importDll);

		public unsafe void GetPInvokeMap(int token, out PInvokeAttributes attributes, out string importName, out string importDll)
		{
			void* pStringHeap = default(void*);
			void* pStringHeap2 = default(void*);
			_GetPInvokeMap(m_metadataImport2, out MetadataArgs.Skip, token, out var attributes2, &pStringHeap, &pStringHeap2);
			importName = new Utf8String(pStringHeap).ToString();
			importDll = new Utf8String(pStringHeap2).ToString();
			attributes = (PInvokeAttributes)attributes2;
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern bool _IsValidToken(IntPtr scope, out MetadataArgs.SkipAddresses skipAddresses, int token);

		public bool IsValidToken(int token)
		{
			return _IsValidToken(m_metadataImport2, out MetadataArgs.Skip, token);
		}
	}
}
