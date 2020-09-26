using System.Runtime.InteropServices;
using System.Runtime.Remoting.Activation;
using System.Security.Permissions;

namespace System.Runtime.Remoting.Contexts
{
	[Serializable]
	[ComVisible(true)]
	[AttributeUsage(AttributeTargets.Class)]
	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
	[SecurityPermission(SecurityAction.InheritanceDemand, Flags = SecurityPermissionFlag.Infrastructure)]
	public class ContextAttribute : Attribute, IContextAttribute, IContextProperty
	{
		protected string AttributeName;

		public virtual string Name => AttributeName;

		public ContextAttribute(string name)
		{
			AttributeName = name;
		}

		public virtual bool IsNewContextOK(Context newCtx)
		{
			return true;
		}

		public virtual void Freeze(Context newContext)
		{
		}

		public override bool Equals(object o)
		{
			IContextProperty contextProperty = o as IContextProperty;
			if (contextProperty != null)
			{
				return AttributeName.Equals(contextProperty.Name);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return AttributeName.GetHashCode();
		}

		public virtual bool IsContextOK(Context ctx, IConstructionCallMessage ctorMsg)
		{
			if (ctx == null)
			{
				throw new ArgumentNullException("ctx");
			}
			if (ctorMsg == null)
			{
				throw new ArgumentNullException("ctorMsg");
			}
			if (!ctorMsg.ActivationType.IsContextful)
			{
				return true;
			}
			object property = ctx.GetProperty(AttributeName);
			if (property != null && Equals(property))
			{
				return true;
			}
			return false;
		}

		public virtual void GetPropertiesForNewContext(IConstructionCallMessage ctorMsg)
		{
			if (ctorMsg == null)
			{
				throw new ArgumentNullException("ctorMsg");
			}
			ctorMsg.ContextProperties.Add(this);
		}
	}
}
