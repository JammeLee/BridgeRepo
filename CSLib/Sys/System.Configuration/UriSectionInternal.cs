using System.Threading;

namespace System.Configuration
{
	internal sealed class UriSectionInternal
	{
		private bool iriParsing;

		private UriIdnScope idn;

		private static object classSyncObject;

		internal UriIdnScope Idn => idn;

		internal bool IriParsing => iriParsing;

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

		internal UriSectionInternal(UriSection section)
		{
			idn = section.Idn.Enabled;
			iriParsing = section.IriParsing.Enabled;
		}

		internal static UriSectionInternal GetSection()
		{
			lock (ClassSyncObject)
			{
				UriSection uriSection = PrivilegedConfigurationManager.GetSection(CommonConfigurationStrings.UriSectionPath) as UriSection;
				if (uriSection == null)
				{
					return null;
				}
				return new UriSectionInternal(uriSection);
			}
		}
	}
}
