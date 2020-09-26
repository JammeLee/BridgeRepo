using System.Text;

namespace System.Security.Authentication.ExtendedProtection
{
	public class ExtendedProtectionPolicy
	{
		private ServiceNameCollection customServiceNames;

		private PolicyEnforcement policyEnforcement;

		private ProtectionScenario protectionScenario;

		private ChannelBinding customChannelBinding;

		public ServiceNameCollection CustomServiceNames => customServiceNames;

		public PolicyEnforcement PolicyEnforcement => policyEnforcement;

		public ProtectionScenario ProtectionScenario => protectionScenario;

		public ChannelBinding CustomChannelBinding => customChannelBinding;

		public ExtendedProtectionPolicy(PolicyEnforcement policyEnforcement, ProtectionScenario protectionScenario, ServiceNameCollection customServiceNames)
		{
			if (policyEnforcement == PolicyEnforcement.Never)
			{
				throw new ArgumentException(SR.GetString("security_ExtendedProtectionPolicy_UseDifferentConstructorForNever"), "policyEnforcement");
			}
			if (customServiceNames != null && customServiceNames.Count == 0)
			{
				throw new ArgumentException(SR.GetString("security_ExtendedProtectionPolicy_NoEmptyServiceNameCollection"), "customServiceNames");
			}
			this.policyEnforcement = policyEnforcement;
			this.protectionScenario = protectionScenario;
			this.customServiceNames = customServiceNames;
		}

		public ExtendedProtectionPolicy(PolicyEnforcement policyEnforcement, ChannelBinding customChannelBinding)
		{
			if (policyEnforcement == PolicyEnforcement.Never)
			{
				throw new ArgumentException(SR.GetString("security_ExtendedProtectionPolicy_UseDifferentConstructorForNever"), "policyEnforcement");
			}
			if (customChannelBinding == null)
			{
				throw new ArgumentNullException("customChannelBinding");
			}
			this.policyEnforcement = policyEnforcement;
			protectionScenario = ProtectionScenario.TransportSelected;
			this.customChannelBinding = customChannelBinding;
		}

		public ExtendedProtectionPolicy(PolicyEnforcement policyEnforcement)
		{
			this.policyEnforcement = policyEnforcement;
			protectionScenario = ProtectionScenario.TransportSelected;
		}

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("ProtectionScenario=");
			stringBuilder.Append(protectionScenario.ToString());
			stringBuilder.Append("; PolicyEnforcement=");
			stringBuilder.Append(policyEnforcement.ToString());
			stringBuilder.Append("; CustomChannelBinding=");
			if (customChannelBinding == null)
			{
				stringBuilder.Append("<null>");
			}
			else
			{
				stringBuilder.Append(customChannelBinding.ToString());
			}
			stringBuilder.Append("; ServiceNames=");
			if (customServiceNames == null)
			{
				stringBuilder.Append("<null>");
			}
			else
			{
				bool flag = true;
				foreach (string customServiceName in customServiceNames)
				{
					if (flag)
					{
						flag = false;
					}
					else
					{
						stringBuilder.Append(", ");
					}
					stringBuilder.Append(customServiceName);
				}
			}
			return stringBuilder.ToString();
		}
	}
}
