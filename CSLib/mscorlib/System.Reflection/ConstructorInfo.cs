using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace System.Reflection
{
	[Serializable]
	[ClassInterface(ClassInterfaceType.None)]
	[ComVisible(true)]
	[ComDefaultInterface(typeof(_ConstructorInfo))]
	[PermissionSet(SecurityAction.InheritanceDemand, Name = "FullTrust")]
	public abstract class ConstructorInfo : MethodBase, _ConstructorInfo
	{
		[ComVisible(true)]
		public static readonly string ConstructorName = ".ctor";

		[ComVisible(true)]
		public static readonly string TypeConstructorName = ".cctor";

		[ComVisible(true)]
		public override MemberTypes MemberType => MemberTypes.Constructor;

		public abstract object Invoke(BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture);

		internal override Type GetReturnType()
		{
			return DeclaringType;
		}

		[DebuggerStepThrough]
		[DebuggerHidden]
		public object Invoke(object[] parameters)
		{
			return Invoke(BindingFlags.Default, null, parameters, null);
		}

		Type _ConstructorInfo.GetType()
		{
			return GetType();
		}

		object _ConstructorInfo.Invoke_2(object obj, BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture)
		{
			return Invoke(obj, invokeAttr, binder, parameters, culture);
		}

		object _ConstructorInfo.Invoke_3(object obj, object[] parameters)
		{
			return Invoke(obj, parameters);
		}

		object _ConstructorInfo.Invoke_4(BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture)
		{
			return Invoke(invokeAttr, binder, parameters, culture);
		}

		object _ConstructorInfo.Invoke_5(object[] parameters)
		{
			return Invoke(parameters);
		}

		void _ConstructorInfo.GetTypeInfoCount(out uint pcTInfo)
		{
			throw new NotImplementedException();
		}

		void _ConstructorInfo.GetTypeInfo(uint iTInfo, uint lcid, IntPtr ppTInfo)
		{
			throw new NotImplementedException();
		}

		void _ConstructorInfo.GetIDsOfNames([In] ref Guid riid, IntPtr rgszNames, uint cNames, uint lcid, IntPtr rgDispId)
		{
			throw new NotImplementedException();
		}

		void _ConstructorInfo.Invoke(uint dispIdMember, [In] ref Guid riid, uint lcid, short wFlags, IntPtr pDispParams, IntPtr pVarResult, IntPtr pExcepInfo, IntPtr puArgErr)
		{
			throw new NotImplementedException();
		}
	}
}
