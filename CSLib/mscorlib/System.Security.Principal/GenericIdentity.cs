using System.Runtime.InteropServices;

namespace System.Security.Principal
{
	[Serializable]
	[ComVisible(true)]
	public class GenericIdentity : IIdentity
	{
		private string m_name;

		private string m_type;

		public virtual string Name => m_name;

		public virtual string AuthenticationType => m_type;

		public virtual bool IsAuthenticated => !m_name.Equals("");

		public GenericIdentity(string name)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			m_name = name;
			m_type = "";
		}

		public GenericIdentity(string name, string type)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}
			m_name = name;
			m_type = type;
		}
	}
}
