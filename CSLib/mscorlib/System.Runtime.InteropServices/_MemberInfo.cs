using System.Reflection;

namespace System.Runtime.InteropServices
{
	[TypeLibImportClass(typeof(MemberInfo))]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("f7102fa9-cabb-3a74-a6da-b4567ef1b079")]
	[CLSCompliant(false)]
	[ComVisible(true)]
	public interface _MemberInfo
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
	}
}
