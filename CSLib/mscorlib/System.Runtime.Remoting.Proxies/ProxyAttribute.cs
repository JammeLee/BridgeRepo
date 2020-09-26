using System.Runtime.InteropServices;
using System.Runtime.Remoting.Activation;
using System.Runtime.Remoting.Contexts;
using System.Security.Permissions;

namespace System.Runtime.Remoting.Proxies
{
	[ComVisible(true)]
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
	[SecurityPermission(SecurityAction.InheritanceDemand, Flags = SecurityPermissionFlag.Infrastructure)]
	public class ProxyAttribute : Attribute, IContextAttribute
	{
		public virtual MarshalByRefObject CreateInstance(Type serverType)
		{
			if (!serverType.IsContextful)
			{
				throw new RemotingException(Environment.GetResourceString("Remoting_Activation_MBR_ProxyAttribute"));
			}
			if (serverType.IsAbstract)
			{
				throw new RemotingException(Environment.GetResourceString("Acc_CreateAbst"));
			}
			return CreateInstanceInternal(serverType);
		}

		internal MarshalByRefObject CreateInstanceInternal(Type serverType)
		{
			return ActivationServices.CreateInstance(serverType);
		}

		public virtual RealProxy CreateProxy(ObjRef objRef, Type serverType, object serverObject, Context serverContext)
		{
			RemotingProxy remotingProxy = new RemotingProxy(serverType);
			if (serverContext != null)
			{
				RealProxy.SetStubData(remotingProxy, serverContext.InternalContextID);
			}
			if (objRef != null && objRef.GetServerIdentity().IsAllocated)
			{
				remotingProxy.SetSrvInfo(objRef.GetServerIdentity(), objRef.GetDomainID());
			}
			remotingProxy.Initialized = true;
			if (!serverType.IsContextful && !serverType.IsMarshalByRef && serverContext != null)
			{
				throw new RemotingException(Environment.GetResourceString("Remoting_Activation_MBR_ProxyAttribute"));
			}
			return remotingProxy;
		}

		[ComVisible(true)]
		public bool IsContextOK(Context ctx, IConstructionCallMessage msg)
		{
			return true;
		}

		[ComVisible(true)]
		public void GetPropertiesForNewContext(IConstructionCallMessage msg)
		{
		}
	}
}
