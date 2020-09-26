using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using Microsoft.Win32;

namespace System.Resources
{
	[Serializable]
	[ComVisible(true)]
	public class ResourceManager
	{
		internal const string ResFileExtension = ".resources";

		internal const int ResFileExtensionLength = 10;

		protected string BaseNameField;

		protected Hashtable ResourceSets;

		private string moduleDir;

		protected Assembly MainAssembly;

		private Type _locationInfo;

		private Type _userResourceSet;

		private CultureInfo _neutralResourcesCulture;

		private bool _ignoreCase;

		private bool UseManifest;

		private bool UseSatelliteAssem;

		private static Hashtable _installedSatelliteInfo;

		private static bool _checkedConfigFile;

		[OptionalField]
		private UltimateResourceFallbackLocation _fallbackLoc;

		[OptionalField]
		private Version _satelliteContractVersion;

		[OptionalField]
		private bool _lookedForSatelliteContractVersion;

		private Assembly _callingAssembly;

		public static readonly int MagicNumber = -1091581234;

		public static readonly int HeaderVersionNumber = 1;

		private static readonly Type _minResourceSet = typeof(ResourceSet);

		internal static readonly string ResReaderTypeName = typeof(ResourceReader).FullName;

		internal static readonly string ResSetTypeName = typeof(RuntimeResourceSet).FullName;

		internal static readonly string MscorlibName = typeof(ResourceReader).Assembly.FullName;

		internal static readonly int DEBUG = 0;

		public virtual string BaseName => BaseNameField;

		public virtual bool IgnoreCase
		{
			get
			{
				return _ignoreCase;
			}
			set
			{
				_ignoreCase = value;
			}
		}

		public virtual Type ResourceSetType
		{
			get
			{
				if (_userResourceSet != null)
				{
					return _userResourceSet;
				}
				return typeof(RuntimeResourceSet);
			}
		}

		protected UltimateResourceFallbackLocation FallbackLocation
		{
			get
			{
				return _fallbackLoc;
			}
			set
			{
				_fallbackLoc = value;
			}
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		protected ResourceManager()
		{
			StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
			_callingAssembly = Assembly.nGetExecutingAssembly(ref stackMark);
		}

		private ResourceManager(string baseName, string resourceDir, Type usingResourceSet)
		{
			if (baseName == null)
			{
				throw new ArgumentNullException("baseName");
			}
			if (resourceDir == null)
			{
				throw new ArgumentNullException("resourceDir");
			}
			BaseNameField = baseName;
			moduleDir = resourceDir;
			_userResourceSet = usingResourceSet;
			ResourceSets = new Hashtable();
			UseManifest = false;
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public ResourceManager(string baseName, Assembly assembly)
		{
			if (baseName == null)
			{
				throw new ArgumentNullException("baseName");
			}
			if (assembly == null)
			{
				throw new ArgumentNullException("assembly");
			}
			MainAssembly = assembly;
			_locationInfo = null;
			BaseNameField = baseName;
			CommonSatelliteAssemblyInit();
			StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
			_callingAssembly = Assembly.nGetExecutingAssembly(ref stackMark);
			if (assembly == typeof(object).Assembly && _callingAssembly != assembly)
			{
				_callingAssembly = null;
			}
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public ResourceManager(string baseName, Assembly assembly, Type usingResourceSet)
		{
			if (baseName == null)
			{
				throw new ArgumentNullException("baseName");
			}
			if (assembly == null)
			{
				throw new ArgumentNullException("assembly");
			}
			MainAssembly = assembly;
			_locationInfo = null;
			BaseNameField = baseName;
			if (usingResourceSet != null && usingResourceSet != _minResourceSet && !usingResourceSet.IsSubclassOf(_minResourceSet))
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_ResMgrNotResSet"), "usingResourceSet");
			}
			_userResourceSet = usingResourceSet;
			CommonSatelliteAssemblyInit();
			StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
			_callingAssembly = Assembly.nGetExecutingAssembly(ref stackMark);
			if (assembly == typeof(object).Assembly && _callingAssembly != assembly)
			{
				_callingAssembly = null;
			}
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public ResourceManager(Type resourceSource)
		{
			if (resourceSource == null)
			{
				throw new ArgumentNullException("resourceSource");
			}
			_locationInfo = resourceSource;
			MainAssembly = _locationInfo.Assembly;
			BaseNameField = resourceSource.Name;
			CommonSatelliteAssemblyInit();
			StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
			_callingAssembly = Assembly.nGetExecutingAssembly(ref stackMark);
			if (MainAssembly == typeof(object).Assembly && _callingAssembly != MainAssembly)
			{
				_callingAssembly = null;
			}
		}

		private void CommonSatelliteAssemblyInit()
		{
			UseManifest = true;
			UseSatelliteAssem = true;
			ResourceSets = new Hashtable();
			_fallbackLoc = UltimateResourceFallbackLocation.MainAssembly;
		}

		public virtual void ReleaseAllResources()
		{
			IDictionaryEnumerator enumerator = ResourceSets.GetEnumerator();
			ResourceSets = new Hashtable();
			while (enumerator.MoveNext())
			{
				((ResourceSet)enumerator.Value).Close();
			}
		}

		public static ResourceManager CreateFileBasedResourceManager(string baseName, string resourceDir, Type usingResourceSet)
		{
			return new ResourceManager(baseName, resourceDir, usingResourceSet);
		}

		private string FindResourceFile(CultureInfo culture)
		{
			string resourceFileName = GetResourceFileName(culture);
			if (moduleDir != null)
			{
				string text = Path.Combine(moduleDir, resourceFileName);
				if (File.Exists(text))
				{
					return text;
				}
			}
			if (File.Exists(resourceFileName))
			{
				return resourceFileName;
			}
			return null;
		}

		protected virtual string GetResourceFileName(CultureInfo culture)
		{
			StringBuilder stringBuilder = new StringBuilder(255);
			stringBuilder.Append(BaseNameField);
			if (!culture.Equals(CultureInfo.InvariantCulture))
			{
				CultureInfo.VerifyCultureName(culture, throwException: true);
				stringBuilder.Append('.');
				stringBuilder.Append(culture.Name);
			}
			stringBuilder.Append(".resources");
			return stringBuilder.ToString();
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public virtual ResourceSet GetResourceSet(CultureInfo culture, bool createIfNotExists, bool tryParents)
		{
			if (culture == null)
			{
				throw new ArgumentNullException("culture");
			}
			Hashtable resourceSets = ResourceSets;
			if (resourceSets != null)
			{
				ResourceSet resourceSet = (ResourceSet)resourceSets[culture];
				if (resourceSet != null)
				{
					return resourceSet;
				}
			}
			if (UseManifest && culture.Equals(CultureInfo.InvariantCulture))
			{
				StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
				string resourceFileName = GetResourceFileName(culture);
				Stream manifestResourceStream = MainAssembly.GetManifestResourceStream(_locationInfo, resourceFileName, _callingAssembly == MainAssembly, ref stackMark);
				if (createIfNotExists && manifestResourceStream != null)
				{
					ResourceSet resourceSet = CreateResourceSet(manifestResourceStream, MainAssembly);
					lock (resourceSets)
					{
						resourceSets.Add(culture, resourceSet);
						return resourceSet;
					}
				}
			}
			return InternalGetResourceSet(culture, createIfNotExists, tryParents);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		protected virtual ResourceSet InternalGetResourceSet(CultureInfo culture, bool createIfNotExists, bool tryParents)
		{
			Hashtable resourceSets = ResourceSets;
			ResourceSet rs = (ResourceSet)resourceSets[culture];
			if (rs != null)
			{
				return rs;
			}
			Stream stream = null;
			string text = null;
			Assembly assembly = null;
			if (UseManifest)
			{
				text = GetResourceFileName(culture);
				StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
				if (UseSatelliteAssem)
				{
					CultureInfo cultureInfo = culture;
					if (_neutralResourcesCulture == null)
					{
						_neutralResourcesCulture = GetNeutralResourcesLanguage(MainAssembly, ref _fallbackLoc);
					}
					if (culture.Equals(_neutralResourcesCulture) && FallbackLocation == UltimateResourceFallbackLocation.MainAssembly)
					{
						cultureInfo = CultureInfo.InvariantCulture;
						text = GetResourceFileName(cultureInfo);
					}
					if (!cultureInfo.Equals(CultureInfo.InvariantCulture))
					{
						assembly = (TryLookingForSatellite(cultureInfo) ? GetSatelliteAssembly(cultureInfo) : null);
					}
					else if (FallbackLocation == UltimateResourceFallbackLocation.Satellite)
					{
						assembly = GetSatelliteAssembly(_neutralResourcesCulture);
						if (assembly == null)
						{
							string text2 = MainAssembly.nGetSimpleName() + ".resources.dll";
							if (_satelliteContractVersion != null)
							{
								text2 = text2 + ", Version=" + _satelliteContractVersion.ToString();
							}
							AssemblyName assemblyName = new AssemblyName();
							assemblyName.SetPublicKey(MainAssembly.nGetPublicKey());
							byte[] publicKeyToken = assemblyName.GetPublicKeyToken();
							int num = publicKeyToken.Length;
							StringBuilder stringBuilder = new StringBuilder(num * 2);
							for (int i = 0; i < num; i++)
							{
								stringBuilder.Append(publicKeyToken[i].ToString("x", CultureInfo.InvariantCulture));
							}
							text2 = text2 + ", PublicKeyToken=" + stringBuilder;
							string text3 = _neutralResourcesCulture.Name;
							if (text3.Length == 0)
							{
								text3 = "<invariant>";
							}
							throw new MissingSatelliteAssemblyException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("MissingSatelliteAssembly_Culture_Name"), _neutralResourcesCulture, text2), text3);
						}
						text = GetResourceFileName(_neutralResourcesCulture);
					}
					else
					{
						assembly = MainAssembly;
					}
					if (assembly != null)
					{
						rs = (ResourceSet)resourceSets[cultureInfo];
						if (rs != null)
						{
							return rs;
						}
						bool skipSecurityCheck = MainAssembly == assembly && _callingAssembly == MainAssembly;
						stream = assembly.GetManifestResourceStream(_locationInfo, text, skipSecurityCheck, ref stackMark);
						if (stream == null)
						{
							stream = CaseInsensitiveManifestResourceStreamLookup(assembly, text);
						}
					}
				}
				else
				{
					assembly = MainAssembly;
					stream = MainAssembly.GetManifestResourceStream(_locationInfo, text, _callingAssembly == MainAssembly, ref stackMark);
				}
				if (stream == null && tryParents)
				{
					if (culture.Equals(CultureInfo.InvariantCulture))
					{
						if (MainAssembly == typeof(object).Assembly && BaseName.Equals("mscorlib"))
						{
							throw new ExecutionEngineException("mscorlib.resources couldn't be found!  Large parts of the BCL won't work!");
						}
						string str = string.Empty;
						if (_locationInfo != null && _locationInfo.Namespace != null)
						{
							str = _locationInfo.Namespace + Type.Delimiter;
						}
						str += text;
						throw new MissingManifestResourceException(Environment.GetResourceString("MissingManifestResource_NoNeutralAsm", str, MainAssembly.nGetSimpleName()));
					}
					CultureInfo parent = culture.Parent;
					rs = InternalGetResourceSet(parent, createIfNotExists, tryParents);
					if (rs != null)
					{
						AddResourceSet(resourceSets, culture, ref rs);
					}
					return rs;
				}
			}
			else
			{
				new FileIOPermission(PermissionState.Unrestricted).Assert();
				text = FindResourceFile(culture);
				if (text != null)
				{
					rs = CreateResourceSet(text);
					if (rs != null)
					{
						AddResourceSet(resourceSets, culture, ref rs);
					}
					return rs;
				}
				if (tryParents)
				{
					if (culture.Equals(CultureInfo.InvariantCulture))
					{
						throw new MissingManifestResourceException(Environment.GetResourceString("MissingManifestResource_NoNeutralDisk") + Environment.NewLine + "baseName: " + BaseNameField + "  locationInfo: " + ((_locationInfo == null) ? "<null>" : _locationInfo.FullName) + "  fileName: " + GetResourceFileName(culture));
					}
					CultureInfo parent2 = culture.Parent;
					rs = InternalGetResourceSet(parent2, createIfNotExists, tryParents);
					if (rs != null)
					{
						AddResourceSet(resourceSets, culture, ref rs);
					}
					return rs;
				}
			}
			if (createIfNotExists && stream != null && rs == null)
			{
				rs = CreateResourceSet(stream, assembly);
				AddResourceSet(resourceSets, culture, ref rs);
			}
			return rs;
		}

		private static void AddResourceSet(Hashtable localResourceSets, CultureInfo culture, ref ResourceSet rs)
		{
			lock (localResourceSets)
			{
				ResourceSet resourceSet = (ResourceSet)localResourceSets[culture];
				if (resourceSet != null)
				{
					if (!object.Equals(resourceSet, rs))
					{
						if (!localResourceSets.ContainsValue(rs))
						{
							rs.Dispose();
						}
						rs = resourceSet;
					}
				}
				else
				{
					localResourceSets.Add(culture, rs);
				}
			}
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		private Stream CaseInsensitiveManifestResourceStreamLookup(Assembly satellite, string name)
		{
			StringBuilder stringBuilder = new StringBuilder();
			if (_locationInfo != null)
			{
				string @namespace = _locationInfo.Namespace;
				if (@namespace != null)
				{
					stringBuilder.Append(@namespace);
					if (name != null)
					{
						stringBuilder.Append(Type.Delimiter);
					}
				}
			}
			stringBuilder.Append(name);
			string text = stringBuilder.ToString();
			CompareInfo compareInfo = CultureInfo.InvariantCulture.CompareInfo;
			string text2 = null;
			string[] manifestResourceNames = satellite.GetManifestResourceNames();
			foreach (string text3 in manifestResourceNames)
			{
				if (compareInfo.Compare(text3, text, CompareOptions.IgnoreCase) == 0)
				{
					if (text2 != null)
					{
						throw new MissingManifestResourceException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("MissingManifestResource_MultipleBlobs"), text, satellite.ToString()));
					}
					text2 = text3;
				}
			}
			if (text2 == null)
			{
				return null;
			}
			bool skipSecurityCheck = MainAssembly == satellite && _callingAssembly == MainAssembly;
			StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
			return satellite.GetManifestResourceStream(text2, ref stackMark, skipSecurityCheck);
		}

		protected static Version GetSatelliteContractVersion(Assembly a)
		{
			string text = null;
			foreach (CustomAttributeData customAttribute in CustomAttributeData.GetCustomAttributes(a))
			{
				if (customAttribute.Constructor.DeclaringType == typeof(SatelliteContractVersionAttribute))
				{
					text = (string)customAttribute.ConstructorArguments[0].Value;
					break;
				}
			}
			if (text == null)
			{
				return null;
			}
			try
			{
				return new Version(text);
			}
			catch (Exception innerException)
			{
				if (a == typeof(object).Assembly)
				{
					return null;
				}
				throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Arg_InvalidSatelliteContract_Asm_Ver"), a.ToString(), text), innerException);
			}
		}

		protected static CultureInfo GetNeutralResourcesLanguage(Assembly a)
		{
			UltimateResourceFallbackLocation fallbackLocation = UltimateResourceFallbackLocation.MainAssembly;
			return GetNeutralResourcesLanguage(a, ref fallbackLocation);
		}

		private static CultureInfo GetNeutralResourcesLanguage(Assembly a, ref UltimateResourceFallbackLocation fallbackLocation)
		{
			IList<CustomAttributeData> customAttributes = CustomAttributeData.GetCustomAttributes(a);
			CustomAttributeData customAttributeData = null;
			for (int i = 0; i < customAttributes.Count; i++)
			{
				if (customAttributes[i].Constructor.DeclaringType == typeof(NeutralResourcesLanguageAttribute))
				{
					customAttributeData = customAttributes[i];
					break;
				}
			}
			if (customAttributeData == null)
			{
				fallbackLocation = UltimateResourceFallbackLocation.MainAssembly;
				return CultureInfo.InvariantCulture;
			}
			string text = null;
			if (customAttributeData.Constructor.GetParameters().Length == 2)
			{
				fallbackLocation = (UltimateResourceFallbackLocation)customAttributeData.ConstructorArguments[1].Value;
				if (fallbackLocation < UltimateResourceFallbackLocation.MainAssembly || fallbackLocation > UltimateResourceFallbackLocation.Satellite)
				{
					throw new ArgumentException(Environment.GetResourceString("Arg_InvalidNeutralResourcesLanguage_FallbackLoc", fallbackLocation));
				}
			}
			else
			{
				fallbackLocation = UltimateResourceFallbackLocation.MainAssembly;
			}
			text = customAttributeData.ConstructorArguments[0].Value as string;
			try
			{
				return CultureInfo.GetCultureInfo(text);
			}
			catch (ArgumentException innerException)
			{
				if (a == typeof(object).Assembly)
				{
					return CultureInfo.InvariantCulture;
				}
				throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Arg_InvalidNeutralResourcesLanguage_Asm_Culture"), a.ToString(), text), innerException);
			}
		}

		private Assembly GetSatelliteAssembly(CultureInfo lookForCulture)
		{
			if (!_lookedForSatelliteContractVersion)
			{
				_satelliteContractVersion = GetSatelliteContractVersion(MainAssembly);
				_lookedForSatelliteContractVersion = true;
			}
			Assembly result = null;
			try
			{
				result = MainAssembly.InternalGetSatelliteAssembly(lookForCulture, _satelliteContractVersion, throwOnFileNotFound: false);
				return result;
			}
			catch (FileLoadException e)
			{
				int hRForException = Marshal.GetHRForException(e);
				Win32Native.MakeHRFromErrorCode(5);
				return result;
			}
			catch (BadImageFormatException)
			{
				return result;
			}
		}

		private ResourceSet CreateResourceSet(string file)
		{
			if (_userResourceSet == null)
			{
				return new RuntimeResourceSet(file);
			}
			object[] args = new object[1]
			{
				file
			};
			try
			{
				return (ResourceSet)Activator.CreateInstance(_userResourceSet, args);
			}
			catch (MissingMethodException innerException)
			{
				throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("InvalidOperation_ResMgrBadResSet_Type"), _userResourceSet.AssemblyQualifiedName), innerException);
			}
		}

		private ResourceSet CreateResourceSet(Stream store, Assembly assembly)
		{
			if (store.CanSeek && store.Length > 4)
			{
				long position = store.Position;
				BinaryReader binaryReader = new BinaryReader(store);
				int num = binaryReader.ReadInt32();
				if (num == MagicNumber)
				{
					int num2 = binaryReader.ReadInt32();
					string text = null;
					string text2 = null;
					if (num2 == HeaderVersionNumber)
					{
						binaryReader.ReadInt32();
						text = binaryReader.ReadString();
						text2 = binaryReader.ReadString();
					}
					else
					{
						if (num2 <= HeaderVersionNumber)
						{
							throw new NotSupportedException(Environment.GetResourceString("NotSupported_ObsoleteResourcesFile", MainAssembly.nGetSimpleName()));
						}
						int num3 = binaryReader.ReadInt32();
						long offset = binaryReader.BaseStream.Position + num3;
						text = binaryReader.ReadString();
						text2 = binaryReader.ReadString();
						binaryReader.BaseStream.Seek(offset, SeekOrigin.Begin);
					}
					store.Position = position;
					if (CanUseDefaultResourceClasses(text, text2))
					{
						return new RuntimeResourceSet(store);
					}
					Type type = Type.GetType(text, throwOnError: true);
					IResourceReader resourceReader = (IResourceReader)Activator.CreateInstance(type, store);
					object[] args = new object[1]
					{
						resourceReader
					};
					Type type2 = ((_userResourceSet != null) ? _userResourceSet : Type.GetType(text2, throwOnError: true, ignoreCase: false));
					return (ResourceSet)Activator.CreateInstance(type2, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.CreateInstance, null, args, null, null);
				}
				store.Position = position;
			}
			if (_userResourceSet == null)
			{
				return new RuntimeResourceSet(store);
			}
			object[] args2 = new object[2]
			{
				store,
				assembly
			};
			try
			{
				ResourceSet resourceSet = null;
				try
				{
					return (ResourceSet)Activator.CreateInstance(_userResourceSet, args2);
				}
				catch (MissingMethodException)
				{
				}
				return (ResourceSet)Activator.CreateInstance(args: new object[1]
				{
					store
				}, type: _userResourceSet);
			}
			catch (MissingMethodException innerException)
			{
				throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("InvalidOperation_ResMgrBadResSet_Type"), _userResourceSet.AssemblyQualifiedName), innerException);
			}
		}

		private bool CanUseDefaultResourceClasses(string readerTypeName, string resSetTypeName)
		{
			if (_userResourceSet != null)
			{
				return false;
			}
			AssemblyName asmName = new AssemblyName(MscorlibName);
			if (readerTypeName != null && !CompareNames(readerTypeName, ResReaderTypeName, asmName))
			{
				return false;
			}
			if (resSetTypeName != null && !CompareNames(resSetTypeName, ResSetTypeName, asmName))
			{
				return false;
			}
			return true;
		}

		internal static bool CompareNames(string asmTypeName1, string typeName2, AssemblyName asmName2)
		{
			int num = asmTypeName1.IndexOf(',');
			if (((num == -1) ? asmTypeName1.Length : num) != typeName2.Length)
			{
				return false;
			}
			if (string.Compare(asmTypeName1, 0, typeName2, 0, typeName2.Length, StringComparison.Ordinal) != 0)
			{
				return false;
			}
			if (num == -1)
			{
				return true;
			}
			while (char.IsWhiteSpace(asmTypeName1[++num]))
			{
			}
			AssemblyName assemblyName = new AssemblyName(asmTypeName1.Substring(num));
			if (string.Compare(assemblyName.Name, asmName2.Name, StringComparison.OrdinalIgnoreCase) != 0)
			{
				return false;
			}
			if (assemblyName.CultureInfo != null && asmName2.CultureInfo != null && assemblyName.CultureInfo.LCID != asmName2.CultureInfo.LCID)
			{
				return false;
			}
			byte[] publicKeyToken = assemblyName.GetPublicKeyToken();
			byte[] publicKeyToken2 = asmName2.GetPublicKeyToken();
			if (publicKeyToken != null && publicKeyToken2 != null)
			{
				if (publicKeyToken.Length != publicKeyToken2.Length)
				{
					return false;
				}
				for (int i = 0; i < publicKeyToken.Length; i++)
				{
					if (publicKeyToken[i] != publicKeyToken2[i])
					{
						return false;
					}
				}
			}
			return true;
		}

		public virtual string GetString(string name)
		{
			return GetString(name, null);
		}

		public virtual string GetString(string name, CultureInfo culture)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			if (culture == null)
			{
				culture = CultureInfo.CurrentUICulture;
			}
			ResourceSet resourceSet = InternalGetResourceSet(culture, createIfNotExists: true, tryParents: true);
			if (resourceSet != null)
			{
				string @string = resourceSet.GetString(name, _ignoreCase);
				if (@string != null)
				{
					return @string;
				}
			}
			ResourceSet resourceSet2 = null;
			while (!culture.Equals(CultureInfo.InvariantCulture) && !culture.Equals(_neutralResourcesCulture))
			{
				culture = culture.Parent;
				resourceSet = InternalGetResourceSet(culture, createIfNotExists: true, tryParents: true);
				if (resourceSet == null)
				{
					break;
				}
				if (resourceSet != resourceSet2)
				{
					string string2 = resourceSet.GetString(name, _ignoreCase);
					if (string2 != null)
					{
						return string2;
					}
					resourceSet2 = resourceSet;
				}
			}
			return null;
		}

		public virtual object GetObject(string name)
		{
			return GetObject(name, null, wrapUnmanagedMemStream: true);
		}

		public virtual object GetObject(string name, CultureInfo culture)
		{
			return GetObject(name, culture, wrapUnmanagedMemStream: true);
		}

		private object GetObject(string name, CultureInfo culture, bool wrapUnmanagedMemStream)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			if (culture == null)
			{
				culture = CultureInfo.CurrentUICulture;
			}
			ResourceSet resourceSet = InternalGetResourceSet(culture, createIfNotExists: true, tryParents: true);
			if (resourceSet != null)
			{
				object @object = resourceSet.GetObject(name, _ignoreCase);
				if (@object != null)
				{
					UnmanagedMemoryStream unmanagedMemoryStream = @object as UnmanagedMemoryStream;
					if (unmanagedMemoryStream != null && wrapUnmanagedMemStream)
					{
						return new UnmanagedMemoryStreamWrapper(unmanagedMemoryStream);
					}
					return @object;
				}
			}
			ResourceSet resourceSet2 = null;
			while (!culture.Equals(CultureInfo.InvariantCulture) && !culture.Equals(_neutralResourcesCulture))
			{
				culture = culture.Parent;
				resourceSet = InternalGetResourceSet(culture, createIfNotExists: true, tryParents: true);
				if (resourceSet == null)
				{
					break;
				}
				if (resourceSet == resourceSet2)
				{
					continue;
				}
				object object2 = resourceSet.GetObject(name, _ignoreCase);
				if (object2 != null)
				{
					UnmanagedMemoryStream unmanagedMemoryStream2 = object2 as UnmanagedMemoryStream;
					if (unmanagedMemoryStream2 != null && wrapUnmanagedMemStream)
					{
						return new UnmanagedMemoryStreamWrapper(unmanagedMemoryStream2);
					}
					return object2;
				}
				resourceSet2 = resourceSet;
			}
			return null;
		}

		[CLSCompliant(false)]
		[ComVisible(false)]
		public UnmanagedMemoryStream GetStream(string name)
		{
			return GetStream(name, null);
		}

		[ComVisible(false)]
		[CLSCompliant(false)]
		public UnmanagedMemoryStream GetStream(string name, CultureInfo culture)
		{
			object @object = GetObject(name, culture, wrapUnmanagedMemStream: false);
			UnmanagedMemoryStream unmanagedMemoryStream = @object as UnmanagedMemoryStream;
			if (unmanagedMemoryStream == null && @object != null)
			{
				throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_ResourceNotStream_Name", name));
			}
			return unmanagedMemoryStream;
		}

		private bool TryLookingForSatellite(CultureInfo lookForCulture)
		{
			if (!_checkedConfigFile)
			{
				lock (this)
				{
					if (!_checkedConfigFile)
					{
						_checkedConfigFile = true;
						_installedSatelliteInfo = GetSatelliteAssembliesFromConfig();
					}
				}
			}
			if (_installedSatelliteInfo == null)
			{
				return true;
			}
			CultureInfo[] array = (CultureInfo[])_installedSatelliteInfo[MainAssembly.FullName];
			if (array == null)
			{
				return true;
			}
			int num = Array.IndexOf(array, lookForCulture);
			return num >= 0;
		}

		private Hashtable GetSatelliteAssembliesFromConfig()
		{
			string configurationFileInternal = AppDomain.CurrentDomain.FusionStore.ConfigurationFileInternal;
			if (configurationFileInternal == null)
			{
				return null;
			}
			if (configurationFileInternal.Length >= 2 && (configurationFileInternal[1] == Path.VolumeSeparatorChar || (configurationFileInternal[0] == Path.DirectorySeparatorChar && configurationFileInternal[1] == Path.DirectorySeparatorChar)) && !File.InternalExists(configurationFileInternal))
			{
				return null;
			}
			ConfigTreeParser configTreeParser = new ConfigTreeParser();
			string configPath = "/configuration/satelliteassemblies";
			ConfigNode configNode = null;
			try
			{
				configNode = configTreeParser.Parse(configurationFileInternal, configPath, skipSecurityStuff: true);
			}
			catch (Exception)
			{
			}
			if (configNode == null)
			{
				return null;
			}
			Hashtable hashtable = new Hashtable(StringComparer.OrdinalIgnoreCase);
			foreach (ConfigNode child in configNode.Children)
			{
				if (!string.Equals(child.Name, "assembly"))
				{
					throw new ApplicationException(Environment.GetResourceString("XMLSyntax_InvalidSyntaxSatAssemTag", Path.GetFileName(configurationFileInternal), child.Name));
				}
				if (child.Attributes.Count != 1)
				{
					throw new ApplicationException(Environment.GetResourceString("XMLSyntax_InvalidSyntaxSatAssemTagBadAttr", Path.GetFileName(configurationFileInternal)));
				}
				DictionaryEntry dictionaryEntry = (DictionaryEntry)child.Attributes[0];
				string text = (string)dictionaryEntry.Value;
				if (!object.Equals(dictionaryEntry.Key, "name") || text == null || text.Length == 0)
				{
					throw new ApplicationException(Environment.GetResourceString("XMLSyntax_InvalidSyntaxSatAssemTagBadAttr", Path.GetFileName(configurationFileInternal), dictionaryEntry.Key, dictionaryEntry.Value));
				}
				ArrayList arrayList = new ArrayList(5);
				foreach (ConfigNode child2 in child.Children)
				{
					if (child2.Value != null)
					{
						arrayList.Add(child2.Value);
					}
				}
				CultureInfo[] array = new CultureInfo[arrayList.Count];
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = CultureInfo.GetCultureInfo((string)arrayList[i]);
				}
				hashtable.Add(text, array);
			}
			return hashtable;
		}
	}
}
