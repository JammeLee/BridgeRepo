using System.Reflection;
using System.Runtime.InteropServices.ComTypes;

namespace System.Runtime.InteropServices
{
	internal class ImporterCallback : ITypeLibImporterNotifySink
	{
		public void ReportEvent(ImporterEventKind EventKind, int EventCode, string EventMsg)
		{
		}

		public Assembly ResolveRef(object TypeLib)
		{
			try
			{
				ITypeLibConverter typeLibConverter = new TypeLibConverter();
				return typeLibConverter.ConvertTypeLibToAssembly(TypeLib, Marshal.GetTypeLibName((ITypeLib)TypeLib) + ".dll", TypeLibImporterFlags.None, new ImporterCallback(), null, null, null, null);
			}
			catch (Exception)
			{
				return null;
			}
		}
	}
}
