using System.Deployment.Internal.Isolation.Manifest;
using System.Runtime.InteropServices;

namespace System.Deployment.Internal.Isolation
{
	internal class Store
	{
		[Flags]
		public enum EnumAssembliesFlags
		{
			Nothing = 0x0,
			VisibleOnly = 0x1,
			MatchServicing = 0x2,
			ForceLibrarySemantics = 0x4
		}

		[Flags]
		public enum EnumAssemblyFilesFlags
		{
			Nothing = 0x0,
			IncludeInstalled = 0x1,
			IncludeMissing = 0x2
		}

		[Flags]
		public enum EnumApplicationPrivateFiles
		{
			Nothing = 0x0,
			IncludeInstalled = 0x1,
			IncludeMissing = 0x2
		}

		[Flags]
		public enum EnumAssemblyInstallReferenceFlags
		{
			Nothing = 0x0
		}

		public interface IPathLock : IDisposable
		{
			string Path
			{
				get;
			}
		}

		private class AssemblyPathLock : IPathLock, IDisposable
		{
			private IStore _pSourceStore;

			private IntPtr _pLockCookie = IntPtr.Zero;

			private string _path;

			public string Path => _path;

			public AssemblyPathLock(IStore s, IntPtr c, string path)
			{
				_pSourceStore = s;
				_pLockCookie = c;
				_path = path;
			}

			private void Dispose(bool fDisposing)
			{
				if (fDisposing)
				{
					GC.SuppressFinalize(this);
				}
				if (_pLockCookie != IntPtr.Zero)
				{
					_pSourceStore.ReleaseAssemblyPath(_pLockCookie);
					_pLockCookie = IntPtr.Zero;
				}
			}

			~AssemblyPathLock()
			{
				Dispose(fDisposing: false);
			}

			void IDisposable.Dispose()
			{
				Dispose(fDisposing: true);
			}
		}

		private class ApplicationPathLock : IPathLock, IDisposable
		{
			private IStore _pSourceStore;

			private IntPtr _pLockCookie = IntPtr.Zero;

			private string _path;

			public string Path => _path;

			public ApplicationPathLock(IStore s, IntPtr c, string path)
			{
				_pSourceStore = s;
				_pLockCookie = c;
				_path = path;
			}

			private void Dispose(bool fDisposing)
			{
				if (fDisposing)
				{
					GC.SuppressFinalize(this);
				}
				if (_pLockCookie != IntPtr.Zero)
				{
					_pSourceStore.ReleaseApplicationPath(_pLockCookie);
					_pLockCookie = IntPtr.Zero;
				}
			}

			~ApplicationPathLock()
			{
				Dispose(fDisposing: false);
			}

			void IDisposable.Dispose()
			{
				Dispose(fDisposing: true);
			}
		}

		[Flags]
		public enum EnumCategoriesFlags
		{
			Nothing = 0x0
		}

		[Flags]
		public enum EnumSubcategoriesFlags
		{
			Nothing = 0x0
		}

		[Flags]
		public enum EnumCategoryInstancesFlags
		{
			Nothing = 0x0
		}

		[Flags]
		public enum GetPackagePropertyFlags
		{
			Nothing = 0x0
		}

		private IStore _pStore;

		public IStore InternalStore => _pStore;

		public Store(IStore pStore)
		{
			if (pStore == null)
			{
				throw new ArgumentNullException("pStore");
			}
			_pStore = pStore;
		}

		public uint[] Transact(StoreTransactionOperation[] operations)
		{
			if (operations == null || operations.Length == 0)
			{
				throw new ArgumentException("operations");
			}
			uint[] array = new uint[operations.Length];
			int[] rgResults = new int[operations.Length];
			_pStore.Transact(new IntPtr(operations.Length), operations, array, rgResults);
			return array;
		}

		public IDefinitionIdentity BindReferenceToAssemblyIdentity(uint Flags, IReferenceIdentity ReferenceIdentity, uint cDeploymentsToIgnore, IDefinitionIdentity[] DefinitionIdentity_DeploymentsToIgnore)
		{
			Guid riid = IsolationInterop.IID_IDefinitionIdentity;
			object obj = _pStore.BindReferenceToAssembly(Flags, ReferenceIdentity, cDeploymentsToIgnore, DefinitionIdentity_DeploymentsToIgnore, ref riid);
			return (IDefinitionIdentity)obj;
		}

		public void CalculateDelimiterOfDeploymentsBasedOnQuota(uint dwFlags, uint cDeployments, IDefinitionAppId[] rgpIDefinitionAppId_Deployments, ref StoreApplicationReference InstallerReference, ulong ulonglongQuota, ref uint Delimiter, ref ulong SizeSharedWithExternalDeployment, ref ulong SizeConsumedByInputDeploymentArray)
		{
			IntPtr Delimiter2 = IntPtr.Zero;
			_pStore.CalculateDelimiterOfDeploymentsBasedOnQuota(dwFlags, new IntPtr(cDeployments), rgpIDefinitionAppId_Deployments, ref InstallerReference, ulonglongQuota, ref Delimiter2, ref SizeSharedWithExternalDeployment, ref SizeConsumedByInputDeploymentArray);
			Delimiter = (uint)Delimiter2.ToInt64();
		}

		public ICMS BindReferenceToAssemblyManifest(uint Flags, IReferenceIdentity ReferenceIdentity, uint cDeploymentsToIgnore, IDefinitionIdentity[] DefinitionIdentity_DeploymentsToIgnore)
		{
			Guid riid = IsolationInterop.IID_ICMS;
			object obj = _pStore.BindReferenceToAssembly(Flags, ReferenceIdentity, cDeploymentsToIgnore, DefinitionIdentity_DeploymentsToIgnore, ref riid);
			return (ICMS)obj;
		}

		public ICMS GetAssemblyManifest(uint Flags, IDefinitionIdentity DefinitionIdentity)
		{
			Guid riid = IsolationInterop.IID_ICMS;
			object assemblyInformation = _pStore.GetAssemblyInformation(Flags, DefinitionIdentity, ref riid);
			return (ICMS)assemblyInformation;
		}

		public IDefinitionIdentity GetAssemblyIdentity(uint Flags, IDefinitionIdentity DefinitionIdentity)
		{
			Guid riid = IsolationInterop.IID_IDefinitionIdentity;
			object assemblyInformation = _pStore.GetAssemblyInformation(Flags, DefinitionIdentity, ref riid);
			return (IDefinitionIdentity)assemblyInformation;
		}

		public StoreAssemblyEnumeration EnumAssemblies(EnumAssembliesFlags Flags)
		{
			return EnumAssemblies(Flags, null);
		}

		public StoreAssemblyEnumeration EnumAssemblies(EnumAssembliesFlags Flags, IReferenceIdentity refToMatch)
		{
			Guid riid = IsolationInterop.GetGuidOfType(typeof(IEnumSTORE_ASSEMBLY));
			object obj = _pStore.EnumAssemblies((uint)Flags, refToMatch, ref riid);
			return new StoreAssemblyEnumeration((IEnumSTORE_ASSEMBLY)obj);
		}

		public StoreAssemblyFileEnumeration EnumFiles(EnumAssemblyFilesFlags Flags, IDefinitionIdentity Assembly)
		{
			Guid riid = IsolationInterop.GetGuidOfType(typeof(IEnumSTORE_ASSEMBLY_FILE));
			object obj = _pStore.EnumFiles((uint)Flags, Assembly, ref riid);
			return new StoreAssemblyFileEnumeration((IEnumSTORE_ASSEMBLY_FILE)obj);
		}

		public StoreAssemblyFileEnumeration EnumPrivateFiles(EnumApplicationPrivateFiles Flags, IDefinitionAppId Application, IDefinitionIdentity Assembly)
		{
			Guid riid = IsolationInterop.GetGuidOfType(typeof(IEnumSTORE_ASSEMBLY_FILE));
			object obj = _pStore.EnumPrivateFiles((uint)Flags, Application, Assembly, ref riid);
			return new StoreAssemblyFileEnumeration((IEnumSTORE_ASSEMBLY_FILE)obj);
		}

		public IEnumSTORE_ASSEMBLY_INSTALLATION_REFERENCE EnumInstallationReferences(EnumAssemblyInstallReferenceFlags Flags, IDefinitionIdentity Assembly)
		{
			Guid riid = IsolationInterop.GetGuidOfType(typeof(IEnumSTORE_ASSEMBLY_INSTALLATION_REFERENCE));
			object obj = _pStore.EnumInstallationReferences((uint)Flags, Assembly, ref riid);
			return (IEnumSTORE_ASSEMBLY_INSTALLATION_REFERENCE)obj;
		}

		public IPathLock LockAssemblyPath(IDefinitionIdentity asm)
		{
			IntPtr Cookie;
			string path = _pStore.LockAssemblyPath(0u, asm, out Cookie);
			return new AssemblyPathLock(_pStore, Cookie, path);
		}

		public IPathLock LockApplicationPath(IDefinitionAppId app)
		{
			IntPtr Cookie;
			string path = _pStore.LockApplicationPath(0u, app, out Cookie);
			return new ApplicationPathLock(_pStore, Cookie, path);
		}

		public ulong QueryChangeID(IDefinitionIdentity asm)
		{
			return _pStore.QueryChangeID(asm);
		}

		public StoreCategoryEnumeration EnumCategories(EnumCategoriesFlags Flags, IReferenceIdentity CategoryMatch)
		{
			Guid riid = IsolationInterop.GetGuidOfType(typeof(IEnumSTORE_CATEGORY));
			object obj = _pStore.EnumCategories((uint)Flags, CategoryMatch, ref riid);
			return new StoreCategoryEnumeration((IEnumSTORE_CATEGORY)obj);
		}

		public StoreSubcategoryEnumeration EnumSubcategories(EnumSubcategoriesFlags Flags, IDefinitionIdentity CategoryMatch)
		{
			return EnumSubcategories(Flags, CategoryMatch, null);
		}

		public StoreSubcategoryEnumeration EnumSubcategories(EnumSubcategoriesFlags Flags, IDefinitionIdentity Category, string SearchPattern)
		{
			Guid riid = IsolationInterop.GetGuidOfType(typeof(IEnumSTORE_CATEGORY_SUBCATEGORY));
			object obj = _pStore.EnumSubcategories((uint)Flags, Category, SearchPattern, ref riid);
			return new StoreSubcategoryEnumeration((IEnumSTORE_CATEGORY_SUBCATEGORY)obj);
		}

		public StoreCategoryInstanceEnumeration EnumCategoryInstances(EnumCategoryInstancesFlags Flags, IDefinitionIdentity Category, string SubCat)
		{
			Guid riid = IsolationInterop.GetGuidOfType(typeof(IEnumSTORE_CATEGORY_INSTANCE));
			object obj = _pStore.EnumCategoryInstances((uint)Flags, Category, SubCat, ref riid);
			return new StoreCategoryInstanceEnumeration((IEnumSTORE_CATEGORY_INSTANCE)obj);
		}

		public byte[] GetDeploymentProperty(GetPackagePropertyFlags Flags, IDefinitionAppId Deployment, StoreApplicationReference Reference, Guid PropertySet, string PropertyName)
		{
			BLOB blob = default(BLOB);
			byte[] array = null;
			try
			{
				_pStore.GetDeploymentProperty((uint)Flags, Deployment, ref Reference, ref PropertySet, PropertyName, out blob);
				array = new byte[blob.Size];
				Marshal.Copy(blob.BlobData, array, 0, (int)blob.Size);
				return array;
			}
			finally
			{
				blob.Dispose();
			}
		}

		public StoreDeploymentMetadataEnumeration EnumInstallerDeployments(Guid InstallerId, string InstallerName, string InstallerMetadata, IReferenceAppId DeploymentFilter)
		{
			object obj = null;
			StoreApplicationReference Reference = new StoreApplicationReference(InstallerId, InstallerName, InstallerMetadata);
			obj = _pStore.EnumInstallerDeploymentMetadata(0u, ref Reference, DeploymentFilter, ref IsolationInterop.IID_IEnumSTORE_DEPLOYMENT_METADATA);
			return new StoreDeploymentMetadataEnumeration((IEnumSTORE_DEPLOYMENT_METADATA)obj);
		}

		public StoreDeploymentMetadataPropertyEnumeration EnumInstallerDeploymentProperties(Guid InstallerId, string InstallerName, string InstallerMetadata, IDefinitionAppId Deployment)
		{
			object obj = null;
			StoreApplicationReference Reference = new StoreApplicationReference(InstallerId, InstallerName, InstallerMetadata);
			obj = _pStore.EnumInstallerDeploymentMetadataProperties(0u, ref Reference, Deployment, ref IsolationInterop.IID_IEnumSTORE_DEPLOYMENT_METADATA_PROPERTY);
			return new StoreDeploymentMetadataPropertyEnumeration((IEnumSTORE_DEPLOYMENT_METADATA_PROPERTY)obj);
		}
	}
}
