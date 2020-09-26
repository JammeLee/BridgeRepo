using System.Collections;
using System.Runtime.InteropServices;
using System.Security.Util;

namespace System.Security.Policy
{
	[Serializable]
	[ComVisible(true)]
	public sealed class GacMembershipCondition : IConstantMembershipCondition, IReportMatchMembershipCondition, IMembershipCondition, ISecurityEncodable, ISecurityPolicyEncodable
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
				object current = hostEnumerator.Current;
				if (current is GacInstalled)
				{
					usedEvidence = current;
					return true;
				}
			}
			return false;
		}

		public IMembershipCondition Copy()
		{
			return new GacMembershipCondition();
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
			XMLUtil.AddClassAttribute(securityElement, GetType(), GetType().FullName);
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
			GacMembershipCondition gacMembershipCondition = o as GacMembershipCondition;
			if (gacMembershipCondition != null)
			{
				return true;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return 0;
		}

		public override string ToString()
		{
			return Environment.GetResourceString("GAC_ToString");
		}
	}
}
