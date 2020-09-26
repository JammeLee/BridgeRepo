using System.Runtime.Serialization;
using System.Security.Permissions;

namespace System.Text
{
	[Serializable]
	internal sealed class SurrogateEncoder : ISerializable, IObjectReference
	{
		[NonSerialized]
		private Encoding realEncoding;

		internal SurrogateEncoder(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			realEncoding = (Encoding)info.GetValue("m_encoding", typeof(Encoding));
		}

		public object GetRealObject(StreamingContext context)
		{
			return realEncoding.GetEncoder();
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
		void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
		{
			throw new ArgumentException(Environment.GetResourceString("Arg_ExecutionEngineException"));
		}
	}
}
