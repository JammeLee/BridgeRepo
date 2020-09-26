using System.Collections;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Activation;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace System.Runtime.Remoting.Messaging
{
	[Serializable]
	[ComVisible(true)]
	[CLSCompliant(false)]
	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
	[SecurityPermission(SecurityAction.InheritanceDemand, Flags = SecurityPermissionFlag.Infrastructure)]
	public class ConstructionCall : MethodCall, IConstructionCallMessage, IMethodCallMessage, IMethodMessage, IMessage
	{
		internal Type _activationType;

		internal string _activationTypeName;

		internal IList _contextProperties;

		internal object[] _callSiteActivationAttributes;

		internal IActivator _activator;

		public object[] CallSiteActivationAttributes => _callSiteActivationAttributes;

		public Type ActivationType
		{
			get
			{
				if (_activationType == null && _activationTypeName != null)
				{
					_activationType = RemotingServices.InternalGetTypeFromQualifiedTypeName(_activationTypeName, partialFallback: false);
				}
				return _activationType;
			}
		}

		public string ActivationTypeName => _activationTypeName;

		public IList ContextProperties
		{
			get
			{
				if (_contextProperties == null)
				{
					_contextProperties = new ArrayList();
				}
				return _contextProperties;
			}
		}

		public override IDictionary Properties
		{
			get
			{
				lock (this)
				{
					if (InternalProperties == null)
					{
						InternalProperties = new Hashtable();
					}
					if (ExternalProperties == null)
					{
						ExternalProperties = new CCMDictionary(this, InternalProperties);
					}
					return ExternalProperties;
				}
			}
		}

		public IActivator Activator
		{
			get
			{
				return _activator;
			}
			set
			{
				_activator = value;
			}
		}

		public ConstructionCall(Header[] headers)
			: base(headers)
		{
		}

		public ConstructionCall(IMessage m)
			: base(m)
		{
		}

		internal ConstructionCall(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

		internal override bool FillSpecialHeader(string key, object value)
		{
			if (key != null)
			{
				if (key.Equals("__ActivationType"))
				{
					_activationType = null;
				}
				else if (key.Equals("__ContextProperties"))
				{
					_contextProperties = (IList)value;
				}
				else if (key.Equals("__CallSiteActivationAttributes"))
				{
					_callSiteActivationAttributes = (object[])value;
				}
				else if (key.Equals("__Activator"))
				{
					_activator = (IActivator)value;
				}
				else
				{
					if (!key.Equals("__ActivationTypeName"))
					{
						return base.FillSpecialHeader(key, value);
					}
					_activationTypeName = (string)value;
				}
			}
			return true;
		}
	}
}
