using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace System.Reflection
{
	[Serializable]
	[ComVisible(true)]
	[ClassInterface(ClassInterfaceType.None)]
	[ComDefaultInterface(typeof(_MethodInfo))]
	[PermissionSet(SecurityAction.InheritanceDemand, Name = "FullTrust")]
	public abstract class MethodInfo : MethodBase, _MethodInfo
	{
		public override MemberTypes MemberType => MemberTypes.Method;

		public virtual Type ReturnType => GetReturnType();

		public virtual ParameterInfo ReturnParameter
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public abstract ICustomAttributeProvider ReturnTypeCustomAttributes
		{
			get;
		}

		public override bool IsGenericMethodDefinition => false;

		public override bool ContainsGenericParameters => false;

		public override bool IsGenericMethod => false;

		internal virtual MethodInfo GetParentDefinition()
		{
			return null;
		}

		internal override Type GetReturnType()
		{
			return ReturnType;
		}

		public abstract MethodInfo GetBaseDefinition();

		[ComVisible(true)]
		public override Type[] GetGenericArguments()
		{
			throw new NotSupportedException(Environment.GetResourceString("NotSupported_SubclassOverride"));
		}

		[ComVisible(true)]
		public virtual MethodInfo GetGenericMethodDefinition()
		{
			throw new NotSupportedException(Environment.GetResourceString("NotSupported_SubclassOverride"));
		}

		public virtual MethodInfo MakeGenericMethod(params Type[] typeArguments)
		{
			throw new NotSupportedException(Environment.GetResourceString("NotSupported_SubclassOverride"));
		}

		Type _MethodInfo.GetType()
		{
			return GetType();
		}

		void _MethodInfo.GetTypeInfoCount(out uint pcTInfo)
		{
			throw new NotImplementedException();
		}

		void _MethodInfo.GetTypeInfo(uint iTInfo, uint lcid, IntPtr ppTInfo)
		{
			throw new NotImplementedException();
		}

		void _MethodInfo.GetIDsOfNames([In] ref Guid riid, IntPtr rgszNames, uint cNames, uint lcid, IntPtr rgDispId)
		{
			throw new NotImplementedException();
		}

		void _MethodInfo.Invoke(uint dispIdMember, [In] ref Guid riid, uint lcid, short wFlags, IntPtr pDispParams, IntPtr pVarResult, IntPtr pExcepInfo, IntPtr puArgErr)
		{
			throw new NotImplementedException();
		}
	}
}
