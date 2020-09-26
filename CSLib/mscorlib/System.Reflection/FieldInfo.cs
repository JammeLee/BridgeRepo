using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace System.Reflection
{
	[Serializable]
	[ComVisible(true)]
	[ComDefaultInterface(typeof(_FieldInfo))]
	[ClassInterface(ClassInterfaceType.None)]
	[PermissionSet(SecurityAction.InheritanceDemand, Name = "FullTrust")]
	public abstract class FieldInfo : MemberInfo, _FieldInfo
	{
		public override MemberTypes MemberType => MemberTypes.Field;

		public abstract RuntimeFieldHandle FieldHandle
		{
			get;
		}

		public abstract Type FieldType
		{
			get;
		}

		public abstract FieldAttributes Attributes
		{
			get;
		}

		public bool IsPublic => (Attributes & FieldAttributes.FieldAccessMask) == FieldAttributes.Public;

		public bool IsPrivate => (Attributes & FieldAttributes.FieldAccessMask) == FieldAttributes.Private;

		public bool IsFamily => (Attributes & FieldAttributes.FieldAccessMask) == FieldAttributes.Family;

		public bool IsAssembly => (Attributes & FieldAttributes.FieldAccessMask) == FieldAttributes.Assembly;

		public bool IsFamilyAndAssembly => (Attributes & FieldAttributes.FieldAccessMask) == FieldAttributes.FamANDAssem;

		public bool IsFamilyOrAssembly => (Attributes & FieldAttributes.FieldAccessMask) == FieldAttributes.FamORAssem;

		public bool IsStatic => (Attributes & FieldAttributes.Static) != 0;

		public bool IsInitOnly => (Attributes & FieldAttributes.InitOnly) != 0;

		public bool IsLiteral => (Attributes & FieldAttributes.Literal) != 0;

		public bool IsNotSerialized => (Attributes & FieldAttributes.NotSerialized) != 0;

		public bool IsSpecialName => (Attributes & FieldAttributes.SpecialName) != 0;

		public bool IsPinvokeImpl => (Attributes & FieldAttributes.PinvokeImpl) != 0;

		public static FieldInfo GetFieldFromHandle(RuntimeFieldHandle handle)
		{
			if (handle.IsNullHandle())
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_InvalidHandle"));
			}
			FieldInfo fieldInfo = RuntimeType.GetFieldInfo(handle);
			if (fieldInfo.DeclaringType != null && fieldInfo.DeclaringType.IsGenericType)
			{
				throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Argument_FieldDeclaringTypeGeneric"), fieldInfo.Name, fieldInfo.DeclaringType.GetGenericTypeDefinition()));
			}
			return fieldInfo;
		}

		[ComVisible(false)]
		public static FieldInfo GetFieldFromHandle(RuntimeFieldHandle handle, RuntimeTypeHandle declaringType)
		{
			if (handle.IsNullHandle())
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_InvalidHandle"));
			}
			return RuntimeType.GetFieldInfo(declaringType, handle);
		}

		public virtual Type[] GetRequiredCustomModifiers()
		{
			throw new NotImplementedException();
		}

		public virtual Type[] GetOptionalCustomModifiers()
		{
			throw new NotImplementedException();
		}

		[CLSCompliant(false)]
		public virtual void SetValueDirect(TypedReference obj, object value)
		{
			throw new NotSupportedException(Environment.GetResourceString("NotSupported_AbstractNonCLS"));
		}

		[CLSCompliant(false)]
		public virtual object GetValueDirect(TypedReference obj)
		{
			throw new NotSupportedException(Environment.GetResourceString("NotSupported_AbstractNonCLS"));
		}

		public abstract object GetValue(object obj);

		public virtual object GetRawConstantValue()
		{
			throw new NotSupportedException(Environment.GetResourceString("NotSupported_AbstractNonCLS"));
		}

		public abstract void SetValue(object obj, object value, BindingFlags invokeAttr, Binder binder, CultureInfo culture);

		[DebuggerStepThrough]
		[DebuggerHidden]
		public void SetValue(object obj, object value)
		{
			SetValue(obj, value, BindingFlags.Default, Type.DefaultBinder, null);
		}

		Type _FieldInfo.GetType()
		{
			return GetType();
		}

		void _FieldInfo.GetTypeInfoCount(out uint pcTInfo)
		{
			throw new NotImplementedException();
		}

		void _FieldInfo.GetTypeInfo(uint iTInfo, uint lcid, IntPtr ppTInfo)
		{
			throw new NotImplementedException();
		}

		void _FieldInfo.GetIDsOfNames([In] ref Guid riid, IntPtr rgszNames, uint cNames, uint lcid, IntPtr rgDispId)
		{
			throw new NotImplementedException();
		}

		void _FieldInfo.Invoke(uint dispIdMember, [In] ref Guid riid, uint lcid, short wFlags, IntPtr pDispParams, IntPtr pVarResult, IntPtr pExcepInfo, IntPtr puArgErr)
		{
			throw new NotImplementedException();
		}
	}
}
