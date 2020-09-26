using System.Runtime.InteropServices;

namespace System.Reflection
{
	[ComVisible(true)]
	public class ManifestResourceInfo
	{
		private Assembly _containingAssembly;

		private string _containingFileName;

		private ResourceLocation _resourceLocation;

		public virtual Assembly ReferencedAssembly => _containingAssembly;

		public virtual string FileName => _containingFileName;

		public virtual ResourceLocation ResourceLocation => _resourceLocation;

		internal ManifestResourceInfo(Assembly containingAssembly, string containingFileName, ResourceLocation resourceLocation)
		{
			_containingAssembly = containingAssembly;
			_containingFileName = containingFileName;
			_resourceLocation = resourceLocation;
		}
	}
}
