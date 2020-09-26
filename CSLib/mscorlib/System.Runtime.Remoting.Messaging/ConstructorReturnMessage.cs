using System.Collections;
using System.Runtime.Remoting.Activation;
using System.Threading;

namespace System.Runtime.Remoting.Messaging
{
	internal class ConstructorReturnMessage : ReturnMessage, IConstructionReturnMessage, IMethodReturnMessage, IMethodMessage, IMessage
	{
		private const int Intercept = 1;

		private MarshalByRefObject _o;

		private int _iFlags;

		public override object ReturnValue
		{
			get
			{
				if (_iFlags == 1)
				{
					return RemotingServices.MarshalInternal(_o, null, null);
				}
				return base.ReturnValue;
			}
		}

		public override IDictionary Properties
		{
			get
			{
				if (_properties == null)
				{
					object value = new CRMDictionary(this, new Hashtable());
					Interlocked.CompareExchange(ref _properties, value, null);
				}
				return (IDictionary)_properties;
			}
		}

		public ConstructorReturnMessage(MarshalByRefObject o, object[] outArgs, int outArgsCount, LogicalCallContext callCtx, IConstructionCallMessage ccm)
			: base(o, outArgs, outArgsCount, callCtx, ccm)
		{
			_o = o;
			_iFlags = 1;
		}

		public ConstructorReturnMessage(Exception e, IConstructionCallMessage ccm)
			: base(e, ccm)
		{
		}

		internal object GetObject()
		{
			return _o;
		}
	}
}
