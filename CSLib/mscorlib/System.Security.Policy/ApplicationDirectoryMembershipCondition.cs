using System.Collections;
using System.Runtime.InteropServices;
using System.Security.Util;

namespace System.Security.Policy
{
	[Serializable]
	[ComVisible(true)]
	public sealed class ApplicationDirectoryMembershipCondition : IConstantMembershipCondition, IReportMatchMembershipCondition, IMembershipCondition, ISecurityEncodable, ISecurityPolicyEncodable
	{
		public bool Check(Evidence evidence)
		{
			object usedEvidence = null;
			return ((IReportMatchMembershipCondition)this).Check(evidence, out usedEvidence);
		}

		bool IReportMatchMembershipCondition.Check(Evidence evidence, out object usedEvidence)
		{
			usedEvidence = null;
			if (evidence == null)
			{
				return false;
			}
			IEnumerator hostEnumerator = evidence.GetHostEnumerator();
			while (hostEnumerator.MoveNext())
			{
				ApplicationDirectory applicationDirectory = hostEnumerator.Current as ApplicationDirectory;
				if (applicationDirectory == null)
				{
					continue;
				}
				IEnumerator hostEnumerator2 = evidence.GetHostEnumerator();
				while (hostEnumerator2.MoveNext())
				{
					Url url = hostEnumerator2.Current as Url;
					if (url == null)
					{
						continue;
					}
					string directory = applicationDirectory.Directory;
					if (directory != null && directory.Length > 1)
					{
						directory = ((directory[directory.Length - 1] != '/') ? (directory + "/*") : (directory + "*"));
						URLString operand = new URLString(directory);
						if (url.GetURLString().IsSubsetOf(operand))
						{
							usedEvidence = applicationDirectory;
							return true;
						}
					}
				}
			}
			return false;
		}

		public IMembershipCondition Copy()
		{
			return new ApplicationDirectoryMembershipCondition();
		}

		public SecurityElement ToXml()
		{
			return ToXml(null);
		}

		public void FromXml(SecurityElement e)
		{
			FromXml(e, null);
		}

		public SecurityElement ToXml(PolicyLevel level)
		{
			SecurityElement securityElement = new SecurityElement("IMembershipCondition");
			XMLUtil.AddClassAttribute(securityElement, GetType(), "System.Security.Policy.ApplicationDirectoryMembershipCondition");
			securityElement.AddAttribute("version", "1");
			return securityElement;
		}

		public void FromXml(SecurityElement e, PolicyLevel level)
		{
			if (e == null)
			{
				throw new ArgumentNullException("e");
			}
			if (!e.Tag.Equals("IMembershipCondition"))
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_MembershipConditionElement"));
			}
		}

		public override bool Equals(object o)
		{
			return o is ApplicationDirectoryMembershipCondition;
		}

		public override int GetHashCode()
		{
			return typeof(ApplicationDirectoryMembershipCondition).GetHashCode();
		}

		public override string ToString()
		{
			return Environment.GetResourceString("ApplicationDirectory_ToString");
		}
	}
}
