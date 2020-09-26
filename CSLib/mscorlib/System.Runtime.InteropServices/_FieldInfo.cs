using System.Globalization;
using System.Reflection;

namespace System.Runtime.InteropServices
{
	[CLSCompliant(false)]
	[ComVisible(true)]
	[Guid("8A7C1442-A9FB-366B-80D8-4939FFA6DBE0")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[TypeLibImportClass(typeof(FieldInfo))]
	public interface _FieldInfo
	{
		MemberTypes MemberType
		{
			get;
		}

		string Name
		{
			get;
		}

		Type DeclaringType
		{
			get;
		}

		Type ReflectedType
		{
			get;
		}

		Type FieldType
		{
			get;
		}

		RuntimeFieldHandle FieldHandle
		{
			get;
		}

		FieldAttributes Attributes
		{
			get;
		}

		bool IsPublic
		{
			get;
		}

		bool IsPrivate
		{
			get;
		}

		bool IsFamily
		{
			get;
		}

		bool IsAssembly
		{
			get;
		}

		bool IsFamilyAndAssembly
		{
			get;
		}

		bool IsFamilyOrAssembly
		{
			get;
		}

		bool IsStatic
		{
			get;
		}

		bool IsInitOnly
		{
			get;
		}

		bool IsLiteral
		{
			get;
		}

		bool IsNotSerialized
		{
			get;
		}

		bool IsSpecialName
		{
			get;
		}

		bool IsPinvokeImpl
		{
			get;
		}

		void GetTypeInfoCount(out uint pcTInfo);

		void GetTypeInfo(uint iTInfo, uint lcid, IntPtr ppTInfo);

		void GetIDsOfNames([In] ref Guid riid, IntPtr rgszNames, uint cNames, uint lcid, IntPtr rgDispId);

		void Invoke(uint dispIdMember, [In] ref Guid riid, uint lcid, short wFlags, IntPtr pDispParams, IntPtr pVarResult, IntPtr pExcepInfo, IntPtr puArgErr);

		new string ToString();

		new bool Equals(object other);

		new int GetHashCode();

		new Type GetType();

		object[] GetCustomAttributes(Type attributeType, bool inherit);

		object[] GetCustomAttributes(bool inherit);

		bool IsDefined(Type attributeType, bool inherit);

		object GetValue(object obj);

		object GetValueDirect(TypedReference obj);

		void SetValue(object obj, object value, BindingFlags invokeAttr, Binder binder, CultureInfo culture);

		void SetValueDirect(TypedReference obj, object value);

		void SetValue(object obj, object value);
	}
}
