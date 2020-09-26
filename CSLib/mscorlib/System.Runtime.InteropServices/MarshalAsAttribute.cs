using System.Reflection;

namespace System.Runtime.InteropServices
{
	[ComVisible(true)]
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.ReturnValue, Inherited = false)]
	public sealed class MarshalAsAttribute : Attribute
	{
		internal UnmanagedType _val;

		public VarEnum SafeArraySubType;

		public Type SafeArrayUserDefinedSubType;

		public int IidParameterIndex;

		public UnmanagedType ArraySubType;

		public short SizeParamIndex;

		public int SizeConst;

		[ComVisible(true)]
		public string MarshalType;

		[ComVisible(true)]
		public Type MarshalTypeRef;

		public string MarshalCookie;

		public UnmanagedType Value => _val;

		internal static Attribute GetCustomAttribute(ParameterInfo parameter)
		{
			return GetCustomAttribute(parameter.MetadataToken, parameter.Member.Module);
		}

		internal static bool IsDefined(ParameterInfo parameter)
		{
			return GetCustomAttribute(parameter) != null;
		}

		internal static Attribute GetCustomAttribute(RuntimeFieldInfo field)
		{
			return GetCustomAttribute(field.MetadataToken, field.Module);
		}

		internal static bool IsDefined(RuntimeFieldInfo field)
		{
			return GetCustomAttribute(field) != null;
		}

		internal static Attribute GetCustomAttribute(int token, Module scope)
		{
			int sizeParamIndex = 0;
			int sizeConst = 0;
			string marshalType = null;
			string marshalCookie = null;
			string safeArrayUserDefinedSubType = null;
			int iidParamIndex = 0;
			ConstArray fieldMarshal = scope.ModuleHandle.GetMetadataImport().GetFieldMarshal(token);
			if (fieldMarshal.Length == 0)
			{
				return null;
			}
			MetadataImport.GetMarshalAs(fieldMarshal, out var unmanagedType, out var safeArraySubType, out safeArrayUserDefinedSubType, out var arraySubType, out sizeParamIndex, out sizeConst, out marshalType, out marshalCookie, out iidParamIndex);
			Type safeArrayUserDefinedSubType2 = ((safeArrayUserDefinedSubType == null || safeArrayUserDefinedSubType.Length == 0) ? null : RuntimeTypeHandle.GetTypeByNameUsingCARules(safeArrayUserDefinedSubType, scope));
			Type marshalTypeRef = null;
			try
			{
				marshalTypeRef = ((marshalType == null) ? null : RuntimeTypeHandle.GetTypeByNameUsingCARules(marshalType, scope));
			}
			catch (TypeLoadException)
			{
			}
			return new MarshalAsAttribute(unmanagedType, safeArraySubType, safeArrayUserDefinedSubType2, arraySubType, (short)sizeParamIndex, sizeConst, marshalType, marshalTypeRef, marshalCookie, iidParamIndex);
		}

		internal MarshalAsAttribute(UnmanagedType val, VarEnum safeArraySubType, Type safeArrayUserDefinedSubType, UnmanagedType arraySubType, short sizeParamIndex, int sizeConst, string marshalType, Type marshalTypeRef, string marshalCookie, int iidParamIndex)
		{
			_val = val;
			SafeArraySubType = safeArraySubType;
			SafeArrayUserDefinedSubType = safeArrayUserDefinedSubType;
			IidParameterIndex = iidParamIndex;
			ArraySubType = arraySubType;
			SizeParamIndex = sizeParamIndex;
			SizeConst = sizeConst;
			MarshalType = marshalType;
			MarshalTypeRef = marshalTypeRef;
			MarshalCookie = marshalCookie;
		}

		public MarshalAsAttribute(UnmanagedType unmanagedType)
		{
			_val = unmanagedType;
		}

		public MarshalAsAttribute(short unmanagedType)
		{
			_val = (UnmanagedType)unmanagedType;
		}
	}
}
