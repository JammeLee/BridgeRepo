using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace System.Runtime.Remoting.Messaging
{
	[ComVisible(true)]
	[SecurityPermission(SecurityAction.InheritanceDemand, Flags = SecurityPermissionFlag.Infrastructure)]
	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
	public class RemotingSurrogateSelector : ISurrogateSelector
	{
		private static Type s_IMethodCallMessageType = typeof(IMethodCallMessage);

		private static Type s_IMethodReturnMessageType = typeof(IMethodReturnMessage);

		private static Type s_ObjRefType = typeof(ObjRef);

		private object _rootObj;

		private ISurrogateSelector _next;

		private RemotingSurrogate _remotingSurrogate = new RemotingSurrogate();

		private ObjRefSurrogate _objRefSurrogate = new ObjRefSurrogate();

		private ISerializationSurrogate _messageSurrogate;

		private MessageSurrogateFilter _filter;

		public MessageSurrogateFilter Filter
		{
			get
			{
				return _filter;
			}
			set
			{
				_filter = value;
			}
		}

		public RemotingSurrogateSelector()
		{
			_messageSurrogate = new MessageSurrogate(this);
		}

		public void SetRootObject(object obj)
		{
			if (obj == null)
			{
				throw new ArgumentNullException("obj");
			}
			_rootObj = obj;
			(_messageSurrogate as SoapMessageSurrogate)?.SetRootObject(_rootObj);
		}

		public object GetRootObject()
		{
			return _rootObj;
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
		public virtual void ChainSelector(ISurrogateSelector selector)
		{
			_next = selector;
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
		public virtual ISerializationSurrogate GetSurrogate(Type type, StreamingContext context, out ISurrogateSelector ssout)
		{
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}
			if (type.IsMarshalByRef)
			{
				ssout = this;
				return _remotingSurrogate;
			}
			if (s_IMethodCallMessageType.IsAssignableFrom(type) || s_IMethodReturnMessageType.IsAssignableFrom(type))
			{
				ssout = this;
				return _messageSurrogate;
			}
			if (s_ObjRefType.IsAssignableFrom(type))
			{
				ssout = this;
				return _objRefSurrogate;
			}
			if (_next != null)
			{
				return _next.GetSurrogate(type, context, out ssout);
			}
			ssout = null;
			return null;
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
		public virtual ISurrogateSelector GetNextSelector()
		{
			return _next;
		}

		public virtual void UseSoapFormat()
		{
			_messageSurrogate = new SoapMessageSurrogate(this);
			((SoapMessageSurrogate)_messageSurrogate).SetRootObject(_rootObj);
		}
	}
}
