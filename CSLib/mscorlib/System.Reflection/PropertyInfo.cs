using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace System.Reflection
{
	[Serializable]
	[ClassInterface(ClassInterfaceType.None)]
	[ComDefaultInterface(typeof(_PropertyInfo))]
	[ComVisible(true)]
	[PermissionSet(SecurityAction.InheritanceDemand, Name = "FullTrust")]
	public abstract class PropertyInfo : MemberInfo, _PropertyInfo
	{
		public override MemberTypes MemberType => MemberTypes.Property;

		public abstract Type PropertyType
		{
			get;
		}

		public abstract PropertyAttributes Attributes
		{
			get;
		}

		public abstract bool CanRead
		{
			get;
		}

		public abstract bool CanWrite
		{
			get;
		}

		public bool IsSpecialName => (Attributes & PropertyAttributes.SpecialName) != 0;

		public virtual object GetConstantValue()
		{
			throw new NotImplementedException();
		}

		public virtual object GetRawConstantValue()
		{
			throw new NotImplementedException();
		}

		public abstract void SetValue(object obj, object value, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture);

		public abstract MethodInfo[] GetAccessors(bool nonPublic);

		public abstract MethodInfo GetGetMethod(bool nonPublic);

		public abstract MethodInfo GetSetMethod(bool nonPublic);

		public abstract ParameterInfo[] GetIndexParameters();

		[DebuggerStepThrough]
		[DebuggerHidden]
		public virtual object GetValue(object obj, object[] index)
		{
			return GetValue(obj, BindingFlags.Default, null, index, null);
		}

		public abstract object GetValue(object obj, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture);

		[DebuggerHidden]
		[DebuggerStepThrough]
		public virtual void SetValue(object obj, object value, object[] index)
		{
			SetValue(obj, value, BindingFlags.Default, null, index, null);
		}

		public virtual Type[] GetRequiredCustomModifiers()
		{
			return new Type[0];
		}

		public virtual Type[] GetOptionalCustomModifiers()
		{
			return new Type[0];
		}

		public MethodInfo[] GetAccessors()
		{
			return GetAccessors(nonPublic: false);
		}

		public MethodInfo GetGetMethod()
		{
			return GetGetMethod(nonPublic: false);
		}

		public MethodInfo GetSetMethod()
		{
			return GetSetMethod(nonPublic: false);
		}

		Type _PropertyInfo.GetType()
		{
			return GetType();
		}

		void _PropertyInfo.GetTypeInfoCount(out uint pcTInfo)
		{
			throw new NotImplementedException();
		}

		void _PropertyInfo.GetTypeInfo(uint iTInfo, uint lcid, IntPtr ppTInfo)
		{
			throw new NotImplementedException();
		}

		void _PropertyInfo.GetIDsOfNames([In] ref Guid riid, IntPtr rgszNames, uint cNames, uint lcid, IntPtr rgDispId)
		{
			throw new NotImplementedException();
		}

		void _PropertyInfo.Invoke(uint dispIdMember, [In] ref Guid riid, uint lcid, short wFlags, IntPtr pDispParams, IntPtr pVarResult, IntPtr pExcepInfo, IntPtr puArgErr)
		{
			throw new NotImplementedException();
		}
	}
}
