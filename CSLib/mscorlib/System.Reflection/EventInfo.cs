using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace System.Reflection
{
	[Serializable]
	[ComDefaultInterface(typeof(_EventInfo))]
	[ClassInterface(ClassInterfaceType.None)]
	[ComVisible(true)]
	[PermissionSet(SecurityAction.InheritanceDemand, Name = "FullTrust")]
	public abstract class EventInfo : MemberInfo, _EventInfo
	{
		public override MemberTypes MemberType => MemberTypes.Event;

		public abstract EventAttributes Attributes
		{
			get;
		}

		public Type EventHandlerType
		{
			get
			{
				MethodInfo addMethod = GetAddMethod(nonPublic: true);
				ParameterInfo[] parametersNoCopy = addMethod.GetParametersNoCopy();
				Type typeFromHandle = typeof(Delegate);
				for (int i = 0; i < parametersNoCopy.Length; i++)
				{
					Type parameterType = parametersNoCopy[i].ParameterType;
					if (parameterType.IsSubclassOf(typeFromHandle))
					{
						return parameterType;
					}
				}
				return null;
			}
		}

		public bool IsSpecialName => (Attributes & EventAttributes.SpecialName) != 0;

		public bool IsMulticast
		{
			get
			{
				Type eventHandlerType = EventHandlerType;
				Type typeFromHandle = typeof(MulticastDelegate);
				return typeFromHandle.IsAssignableFrom(eventHandlerType);
			}
		}

		public virtual MethodInfo[] GetOtherMethods(bool nonPublic)
		{
			throw new NotImplementedException();
		}

		public abstract MethodInfo GetAddMethod(bool nonPublic);

		public abstract MethodInfo GetRemoveMethod(bool nonPublic);

		public abstract MethodInfo GetRaiseMethod(bool nonPublic);

		public MethodInfo[] GetOtherMethods()
		{
			return GetOtherMethods(nonPublic: false);
		}

		public MethodInfo GetAddMethod()
		{
			return GetAddMethod(nonPublic: false);
		}

		public MethodInfo GetRemoveMethod()
		{
			return GetRemoveMethod(nonPublic: false);
		}

		public MethodInfo GetRaiseMethod()
		{
			return GetRaiseMethod(nonPublic: false);
		}

		[DebuggerHidden]
		[DebuggerStepThrough]
		public void AddEventHandler(object target, Delegate handler)
		{
			MethodInfo addMethod = GetAddMethod();
			if (addMethod == null)
			{
				throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_NoPublicAddMethod"));
			}
			addMethod.Invoke(target, new object[1]
			{
				handler
			});
		}

		[DebuggerStepThrough]
		[DebuggerHidden]
		public void RemoveEventHandler(object target, Delegate handler)
		{
			MethodInfo removeMethod = GetRemoveMethod();
			if (removeMethod == null)
			{
				throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_NoPublicRemoveMethod"));
			}
			removeMethod.Invoke(target, new object[1]
			{
				handler
			});
		}

		Type _EventInfo.GetType()
		{
			return GetType();
		}

		void _EventInfo.GetTypeInfoCount(out uint pcTInfo)
		{
			throw new NotImplementedException();
		}

		void _EventInfo.GetTypeInfo(uint iTInfo, uint lcid, IntPtr ppTInfo)
		{
			throw new NotImplementedException();
		}

		void _EventInfo.GetIDsOfNames([In] ref Guid riid, IntPtr rgszNames, uint cNames, uint lcid, IntPtr rgDispId)
		{
			throw new NotImplementedException();
		}

		void _EventInfo.Invoke(uint dispIdMember, [In] ref Guid riid, uint lcid, short wFlags, IntPtr pDispParams, IntPtr pVarResult, IntPtr pExcepInfo, IntPtr puArgErr)
		{
			throw new NotImplementedException();
		}
	}
}
