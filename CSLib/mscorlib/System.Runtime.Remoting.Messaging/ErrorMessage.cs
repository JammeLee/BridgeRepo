using System.Collections;
using System.Reflection;

namespace System.Runtime.Remoting.Messaging
{
	internal class ErrorMessage : IMethodCallMessage, IMethodMessage, IMessage
	{
		private string m_URI = "Exception";

		private string m_MethodName = "Unknown";

		private string m_TypeName = "Unknown";

		private object m_MethodSignature;

		private int m_ArgCount;

		private string m_ArgName = "Unknown";

		public IDictionary Properties => null;

		public string Uri => m_URI;

		public string MethodName => m_MethodName;

		public string TypeName => m_TypeName;

		public object MethodSignature => m_MethodSignature;

		public MethodBase MethodBase => null;

		public int ArgCount => m_ArgCount;

		public object[] Args => null;

		public bool HasVarArgs => false;

		public int InArgCount => m_ArgCount;

		public object[] InArgs => null;

		public LogicalCallContext LogicalCallContext => null;

		public string GetArgName(int index)
		{
			return m_ArgName;
		}

		public object GetArg(int argNum)
		{
			return null;
		}

		public string GetInArgName(int index)
		{
			return null;
		}

		public object GetInArg(int argNum)
		{
			return null;
		}
	}
}
