using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace System.Reflection.Emit
{
	[ComVisible(true)]
	[ClassInterface(ClassInterfaceType.None)]
	[ComDefaultInterface(typeof(_EventBuilder))]
	[HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort = true)]
	public sealed class EventBuilder : _EventBuilder
	{
		private string m_name;

		private EventToken m_evToken;

		private Module m_module;

		private EventAttributes m_attributes;

		private TypeBuilder m_type;

		private EventBuilder()
		{
		}

		internal EventBuilder(Module mod, string name, EventAttributes attr, int eventType, TypeBuilder type, EventToken evToken)
		{
			m_name = name;
			m_module = mod;
			m_attributes = attr;
			m_evToken = evToken;
			m_type = type;
		}

		public EventToken GetEventToken()
		{
			return m_evToken;
		}

		public void SetAddOnMethod(MethodBuilder mdBuilder)
		{
			if (mdBuilder == null)
			{
				throw new ArgumentNullException("mdBuilder");
			}
			m_type.ThrowIfCreated();
			TypeBuilder.InternalDefineMethodSemantics(m_module, m_evToken.Token, MethodSemanticsAttributes.AddOn, mdBuilder.GetToken().Token);
		}

		public void SetRemoveOnMethod(MethodBuilder mdBuilder)
		{
			if (mdBuilder == null)
			{
				throw new ArgumentNullException("mdBuilder");
			}
			m_type.ThrowIfCreated();
			TypeBuilder.InternalDefineMethodSemantics(m_module, m_evToken.Token, MethodSemanticsAttributes.RemoveOn, mdBuilder.GetToken().Token);
		}

		public void SetRaiseMethod(MethodBuilder mdBuilder)
		{
			if (mdBuilder == null)
			{
				throw new ArgumentNullException("mdBuilder");
			}
			m_type.ThrowIfCreated();
			TypeBuilder.InternalDefineMethodSemantics(m_module, m_evToken.Token, MethodSemanticsAttributes.Fire, mdBuilder.GetToken().Token);
		}

		public void AddOtherMethod(MethodBuilder mdBuilder)
		{
			if (mdBuilder == null)
			{
				throw new ArgumentNullException("mdBuilder");
			}
			m_type.ThrowIfCreated();
			TypeBuilder.InternalDefineMethodSemantics(m_module, m_evToken.Token, MethodSemanticsAttributes.Other, mdBuilder.GetToken().Token);
		}

		[ComVisible(true)]
		public void SetCustomAttribute(ConstructorInfo con, byte[] binaryAttribute)
		{
			if (con == null)
			{
				throw new ArgumentNullException("con");
			}
			if (binaryAttribute == null)
			{
				throw new ArgumentNullException("binaryAttribute");
			}
			m_type.ThrowIfCreated();
			TypeBuilder.InternalCreateCustomAttribute(m_evToken.Token, ((ModuleBuilder)m_module).GetConstructorToken(con).Token, binaryAttribute, m_module, toDisk: false);
		}

		public void SetCustomAttribute(CustomAttributeBuilder customBuilder)
		{
			if (customBuilder == null)
			{
				throw new ArgumentNullException("customBuilder");
			}
			m_type.ThrowIfCreated();
			customBuilder.CreateCustomAttribute((ModuleBuilder)m_module, m_evToken.Token);
		}

		void _EventBuilder.GetTypeInfoCount(out uint pcTInfo)
		{
			throw new NotImplementedException();
		}

		void _EventBuilder.GetTypeInfo(uint iTInfo, uint lcid, IntPtr ppTInfo)
		{
			throw new NotImplementedException();
		}

		void _EventBuilder.GetIDsOfNames([In] ref Guid riid, IntPtr rgszNames, uint cNames, uint lcid, IntPtr rgDispId)
		{
			throw new NotImplementedException();
		}

		void _EventBuilder.Invoke(uint dispIdMember, [In] ref Guid riid, uint lcid, short wFlags, IntPtr pDispParams, IntPtr pVarResult, IntPtr pExcepInfo, IntPtr puArgErr)
		{
			throw new NotImplementedException();
		}
	}
}
