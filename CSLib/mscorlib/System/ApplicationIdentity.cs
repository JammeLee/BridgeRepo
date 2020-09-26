using System.Deployment.Internal.Isolation;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace System
{
	[Serializable]
	[ComVisible(false)]
	public sealed class ApplicationIdentity : ISerializable
	{
		private IDefinitionAppId _appId;

		public string FullName => IsolationInterop.AppIdAuthority.DefinitionToText(0u, _appId);

		public string CodeBase => _appId.get_Codebase();

		internal IDefinitionAppId Identity => _appId;

		private ApplicationIdentity()
		{
		}

		private ApplicationIdentity(SerializationInfo info, StreamingContext context)
		{
			string text = (string)info.GetValue("FullName", typeof(string));
			if (text == null)
			{
				throw new ArgumentNullException("fullName");
			}
			_appId = IsolationInterop.AppIdAuthority.TextToDefinition(0u, text);
		}

		public ApplicationIdentity(string applicationIdentityFullName)
		{
			if (applicationIdentityFullName == null)
			{
				throw new ArgumentNullException("applicationIdentityFullName");
			}
			_appId = IsolationInterop.AppIdAuthority.TextToDefinition(0u, applicationIdentityFullName);
		}

		internal ApplicationIdentity(IDefinitionAppId applicationIdentity)
		{
			_appId = applicationIdentity;
		}

		public override string ToString()
		{
			return FullName;
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
		void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("FullName", FullName, typeof(string));
		}
	}
}
