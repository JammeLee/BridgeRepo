using System.Configuration;
using System.Net.Mail;
using System.Threading;

namespace System.Net.Configuration
{
	internal sealed class SmtpSectionInternal
	{
		private SmtpDeliveryMethod deliveryMethod;

		private string from;

		private SmtpNetworkElementInternal network;

		private SmtpSpecifiedPickupDirectoryElementInternal specifiedPickupDirectory;

		private static object classSyncObject;

		internal SmtpDeliveryMethod DeliveryMethod => deliveryMethod;

		internal SmtpNetworkElementInternal Network => network;

		internal string From => from;

		internal SmtpSpecifiedPickupDirectoryElementInternal SpecifiedPickupDirectory => specifiedPickupDirectory;

		internal static object ClassSyncObject
		{
			get
			{
				if (classSyncObject == null)
				{
					Interlocked.CompareExchange(ref classSyncObject, new object(), null);
				}
				return classSyncObject;
			}
		}

		internal SmtpSectionInternal(SmtpSection section)
		{
			deliveryMethod = section.DeliveryMethod;
			from = section.From;
			network = new SmtpNetworkElementInternal(section.Network);
			specifiedPickupDirectory = new SmtpSpecifiedPickupDirectoryElementInternal(section.SpecifiedPickupDirectory);
		}

		internal static SmtpSectionInternal GetSection()
		{
			lock (ClassSyncObject)
			{
				SmtpSection smtpSection = System.Configuration.PrivilegedConfigurationManager.GetSection(ConfigurationStrings.SmtpSectionPath) as SmtpSection;
				if (smtpSection == null)
				{
					return null;
				}
				return new SmtpSectionInternal(smtpSection);
			}
		}
	}
}
