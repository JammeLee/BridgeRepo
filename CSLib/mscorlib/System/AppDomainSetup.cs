using System.Deployment.Internal.Isolation.Manifest;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.Hosting;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Permissions;
using System.Security.Policy;
using System.Security.Util;
using System.Text;

namespace System
{
	[Serializable]
	[ComVisible(true)]
	[ClassInterface(ClassInterfaceType.None)]
	public sealed class AppDomainSetup : IAppDomainSetup
	{
		[Serializable]
		internal enum LoaderInformation
		{
			ApplicationBaseValue,
			ConfigurationFileValue,
			DynamicBaseValue,
			DevPathValue,
			ApplicationNameValue,
			PrivateBinPathValue,
			PrivateBinPathProbeValue,
			ShadowCopyDirectoriesValue,
			ShadowCopyFilesValue,
			CachePathValue,
			LicenseFileValue,
			DisallowPublisherPolicyValue,
			DisallowCodeDownloadValue,
			DisallowBindingRedirectsValue,
			DisallowAppBaseProbingValue,
			ConfigurationBytesValue,
			LoaderMaximum
		}

		private string[] _Entries;

		private LoaderOptimization _LoaderOptimization;

		private string _AppBase;

		[OptionalField(VersionAdded = 2)]
		private AppDomainInitializer _AppDomainInitializer;

		[OptionalField(VersionAdded = 2)]
		private string[] _AppDomainInitializerArguments;

		[OptionalField(VersionAdded = 2)]
		private ActivationArguments _ActivationArguments;

		[OptionalField(VersionAdded = 2)]
		internal string _ApplicationTrust;

		[OptionalField(VersionAdded = 2)]
		private byte[] _ConfigurationBytes;

		[OptionalField(VersionAdded = 3)]
		private bool _DisableInterfaceCache;

		internal string[] Value
		{
			get
			{
				if (_Entries == null)
				{
					_Entries = new string[16];
				}
				return _Entries;
			}
		}

		public string ApplicationBase
		{
			get
			{
				return VerifyDir(Value[0], normalize: false);
			}
			set
			{
				Value[0] = NormalizePath(value, useAppBase: false);
			}
		}

		internal static string ApplicationBaseKey => "APPBASE";

		public string ConfigurationFile
		{
			get
			{
				return VerifyDir(Value[1], normalize: true);
			}
			set
			{
				Value[1] = value;
			}
		}

		internal string ConfigurationFileInternal => NormalizePath(Value[1], useAppBase: true);

		internal static string ConfigurationFileKey => "APP_CONFIG_FILE";

		private static string ConfigurationBytesKey => "APP_CONFIG_BLOB";

		public string DynamicBase
		{
			get
			{
				return VerifyDir(Value[2], normalize: true);
			}
			set
			{
				if (value == null)
				{
					Value[2] = null;
					return;
				}
				if (ApplicationName == null)
				{
					throw new MemberAccessException(Environment.GetResourceString("AppDomain_RequireApplicationName"));
				}
				StringBuilder stringBuilder = new StringBuilder(NormalizePath(value, useAppBase: false));
				stringBuilder.Append('\\');
				string value2 = ParseNumbers.IntToString(ApplicationName.GetHashCode(), 16, 8, '0', 256);
				stringBuilder.Append(value2);
				Value[2] = stringBuilder.ToString();
			}
		}

		internal static string DynamicBaseKey => "DYNAMIC_BASE";

		public bool DisallowPublisherPolicy
		{
			get
			{
				return Value[11] != null;
			}
			set
			{
				if (value)
				{
					Value[11] = "true";
				}
				else
				{
					Value[11] = null;
				}
			}
		}

		public bool DisallowBindingRedirects
		{
			get
			{
				return Value[13] != null;
			}
			set
			{
				if (value)
				{
					Value[13] = "true";
				}
				else
				{
					Value[13] = null;
				}
			}
		}

		public bool DisallowCodeDownload
		{
			get
			{
				return Value[12] != null;
			}
			set
			{
				if (value)
				{
					Value[12] = "true";
				}
				else
				{
					Value[12] = null;
				}
			}
		}

		public bool DisallowApplicationBaseProbing
		{
			get
			{
				return Value[14] != null;
			}
			set
			{
				if (value)
				{
					Value[14] = "true";
				}
				else
				{
					Value[14] = null;
				}
			}
		}

		internal string DeveloperPath
		{
			get
			{
				string text = Value[3];
				VerifyDirList(text);
				return text;
			}
			set
			{
				if (value == null)
				{
					Value[3] = null;
					return;
				}
				string[] array = value.Split(';');
				int num = array.Length;
				StringBuilder stringBuilder = new StringBuilder();
				bool flag = false;
				for (int i = 0; i < num; i++)
				{
					if (array[i].Length != 0)
					{
						if (flag)
						{
							stringBuilder.Append(";");
						}
						else
						{
							flag = true;
						}
						stringBuilder.Append(Path.GetFullPathInternal(array[i]));
					}
				}
				string text = stringBuilder.ToString();
				if (text.Length == 0)
				{
					Value[3] = null;
				}
				else
				{
					Value[3] = stringBuilder.ToString();
				}
			}
		}

		internal static string DisallowPublisherPolicyKey => "DISALLOW_APP";

		internal static string DisallowCodeDownloadKey => "CODE_DOWNLOAD_DISABLED";

		internal static string DisallowBindingRedirectsKey => "DISALLOW_APP_REDIRECTS";

		internal static string DeveloperPathKey => "DEV_PATH";

		internal static string DisallowAppBaseProbingKey => "DISALLOW_APP_BASE_PROBING";

		public string ApplicationName
		{
			get
			{
				return Value[4];
			}
			set
			{
				Value[4] = value;
			}
		}

		internal static string ApplicationNameKey => "APP_NAME";

		[XmlIgnoreMember]
		public AppDomainInitializer AppDomainInitializer
		{
			get
			{
				return _AppDomainInitializer;
			}
			set
			{
				_AppDomainInitializer = value;
			}
		}

		public string[] AppDomainInitializerArguments
		{
			get
			{
				return _AppDomainInitializerArguments;
			}
			set
			{
				_AppDomainInitializerArguments = value;
			}
		}

		[XmlIgnoreMember]
		public ActivationArguments ActivationArguments
		{
			get
			{
				return _ActivationArguments;
			}
			set
			{
				lock (this)
				{
					if (value != null && _ApplicationTrust != null && !CmsUtils.CompareIdentities(ApplicationTrust.ApplicationIdentity, value.ApplicationIdentity, ApplicationVersionMatch.MatchExactVersion))
					{
						throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_ActivationArgsAppTrustMismatch"));
					}
					_ActivationArguments = value;
				}
			}
		}

		[XmlIgnoreMember]
		public ApplicationTrust ApplicationTrust
		{
			get
			{
				if (_ApplicationTrust == null)
				{
					return null;
				}
				SecurityElement element = SecurityElement.FromString(_ApplicationTrust);
				ApplicationTrust applicationTrust = new ApplicationTrust();
				applicationTrust.FromXml(element);
				return applicationTrust;
			}
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException("value", Environment.GetResourceString("ArgumentNull_ApplicationTrust"));
				}
				lock (this)
				{
					if (_ActivationArguments != null && !CmsUtils.CompareIdentities(value.ApplicationIdentity, _ActivationArguments.ApplicationIdentity, ApplicationVersionMatch.MatchExactVersion))
					{
						throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_ActivationArgsAppTrustMismatch"));
					}
					_ApplicationTrust = value.ToXml().ToString();
				}
			}
		}

		public string PrivateBinPath
		{
			get
			{
				string text = Value[5];
				VerifyDirList(text);
				return text;
			}
			set
			{
				Value[5] = value;
			}
		}

		internal static string PrivateBinPathKey => "PRIVATE_BINPATH";

		public string PrivateBinPathProbe
		{
			get
			{
				return Value[6];
			}
			set
			{
				Value[6] = value;
			}
		}

		internal static string PrivateBinPathProbeKey => "BINPATH_PROBE_ONLY";

		public string ShadowCopyDirectories
		{
			get
			{
				string text = Value[7];
				VerifyDirList(text);
				return text;
			}
			set
			{
				Value[7] = value;
			}
		}

		internal static string ShadowCopyDirectoriesKey => "SHADOW_COPY_DIRS";

		public string ShadowCopyFiles
		{
			get
			{
				return Value[8];
			}
			set
			{
				if (value != null && string.Compare(value, "true", StringComparison.OrdinalIgnoreCase) == 0)
				{
					Value[8] = value;
				}
				else
				{
					Value[8] = null;
				}
			}
		}

		internal static string ShadowCopyFilesKey => "FORCE_CACHE_INSTALL";

		public string CachePath
		{
			get
			{
				return VerifyDir(Value[9], normalize: false);
			}
			set
			{
				Value[9] = NormalizePath(value, useAppBase: false);
			}
		}

		internal static string CachePathKey => "CACHE_BASE";

		public string LicenseFile
		{
			get
			{
				return VerifyDir(Value[10], normalize: true);
			}
			set
			{
				Value[10] = value;
			}
		}

		public LoaderOptimization LoaderOptimization
		{
			get
			{
				return _LoaderOptimization;
			}
			set
			{
				_LoaderOptimization = value;
			}
		}

		internal static string LoaderOptimizationKey => "LOADER_OPTIMIZATION";

		internal static string ConfigurationExtension => ".config";

		internal static string PrivateBinPathEnvironmentVariable => "RELPATH";

		internal static string RuntimeConfigurationFile => "config\\machine.config";

		internal static string MachineConfigKey => "MACHINE_CONFIG";

		internal static string HostBindingKey => "HOST_CONFIG";

		public bool SandboxInterop
		{
			get
			{
				return _DisableInterfaceCache;
			}
			set
			{
				_DisableInterfaceCache = value;
			}
		}

		internal AppDomainSetup(AppDomainSetup copy, bool copyDomainBoundData)
		{
			string[] value = Value;
			if (copy != null)
			{
				string[] value2 = copy.Value;
				int num = _Entries.Length;
				int num2 = value2.Length;
				int num3 = ((num2 < num) ? num2 : num);
				for (int i = 0; i < num3; i++)
				{
					value[i] = value2[i];
				}
				if (num3 < num)
				{
					for (int j = num3; j < num; j++)
					{
						value[j] = null;
					}
				}
				_LoaderOptimization = copy._LoaderOptimization;
				_AppDomainInitializerArguments = copy.AppDomainInitializerArguments;
				_ActivationArguments = copy.ActivationArguments;
				_ApplicationTrust = copy._ApplicationTrust;
				if (copyDomainBoundData)
				{
					_AppDomainInitializer = copy.AppDomainInitializer;
				}
				else
				{
					_AppDomainInitializer = null;
				}
				_ConfigurationBytes = copy.GetConfigurationBytes();
				_DisableInterfaceCache = copy._DisableInterfaceCache;
			}
			else
			{
				_LoaderOptimization = LoaderOptimization.NotSpecified;
			}
		}

		public AppDomainSetup()
		{
			_LoaderOptimization = LoaderOptimization.NotSpecified;
		}

		public AppDomainSetup(ActivationContext activationContext)
			: this(new ActivationArguments(activationContext))
		{
		}

		public AppDomainSetup(ActivationArguments activationArguments)
		{
			if (activationArguments == null)
			{
				throw new ArgumentNullException("activationArguments");
			}
			_LoaderOptimization = LoaderOptimization.NotSpecified;
			ActivationArguments = activationArguments;
			string entryPointFullPath = CmsUtils.GetEntryPointFullPath(activationArguments);
			if (!string.IsNullOrEmpty(entryPointFullPath))
			{
				SetupDefaultApplicationBase(entryPointFullPath);
			}
			else
			{
				ApplicationBase = activationArguments.ActivationContext.ApplicationDirectory;
			}
		}

		internal void SetupDefaultApplicationBase(string imageLocation)
		{
			StringBuilder stringBuilder = null;
			char[] anyOf = new char[2]
			{
				'\\',
				'/'
			};
			int num = imageLocation.LastIndexOfAny(anyOf);
			string text = null;
			if (num == -1)
			{
				stringBuilder = new StringBuilder(imageLocation);
			}
			else
			{
				text = imageLocation.Substring(0, num + 1);
				stringBuilder = new StringBuilder(imageLocation.Substring(num + 1));
			}
			string text2 = ((num != -1) ? imageLocation.Substring(num + 1) : imageLocation);
			stringBuilder.Append(ConfigurationExtension);
			if (stringBuilder != null)
			{
				ConfigurationFile = stringBuilder.ToString();
			}
			if (text != null)
			{
				ApplicationBase = text;
			}
			if (text2 != null)
			{
				ApplicationName = text2;
			}
		}

		private string NormalizePath(string path, bool useAppBase)
		{
			if (path == null)
			{
				return null;
			}
			if (!useAppBase)
			{
				path = URLString.PreProcessForExtendedPathRemoval(path, isFileUrl: false);
			}
			int num = path.Length;
			if (num == 0)
			{
				return null;
			}
			bool flag = false;
			if (num > 7 && string.Compare(path, 0, "file:", 0, 5, StringComparison.OrdinalIgnoreCase) == 0)
			{
				int num2;
				if (path[6] == '\\')
				{
					if (path[7] == '\\' || path[7] == '/')
					{
						if (num > 8 && (path[8] == '\\' || path[8] == '/'))
						{
							throw new ArgumentException(Environment.GetResourceString("Argument_InvalidPathChars"));
						}
						num2 = 8;
					}
					else
					{
						num2 = 5;
						flag = true;
					}
				}
				else if (path[7] == '/')
				{
					num2 = 8;
				}
				else
				{
					if (num > 8 && path[7] == '\\' && path[8] == '\\')
					{
						num2 = 7;
					}
					else
					{
						num2 = 5;
						StringBuilder stringBuilder = new StringBuilder(num);
						for (int i = 0; i < num; i++)
						{
							char c = path[i];
							if (c == '/')
							{
								stringBuilder.Append('\\');
							}
							else
							{
								stringBuilder.Append(c);
							}
						}
						path = stringBuilder.ToString();
					}
					flag = true;
				}
				path = path.Substring(num2);
				num -= num2;
			}
			bool flag2;
			if (flag || (num > 1 && (path[0] == '/' || path[0] == '\\') && (path[1] == '/' || path[1] == '\\')))
			{
				flag2 = false;
			}
			else
			{
				int num3 = path.IndexOf(':') + 1;
				flag2 = ((num3 == 0 || num <= num3 + 1 || (path[num3] != '/' && path[num3] != '\\') || (path[num3 + 1] != '/' && path[num3 + 1] != '\\')) ? true : false);
			}
			if (flag2)
			{
				if (useAppBase && (num == 1 || path[1] != ':'))
				{
					string text = Value[0];
					if (text == null || text.Length == 0)
					{
						throw new MemberAccessException(Environment.GetResourceString("AppDomain_AppBaseNotSet"));
					}
					StringBuilder stringBuilder2 = new StringBuilder();
					bool flag3 = false;
					if (path[0] == '/' || path[0] == '\\')
					{
						string text2 = Path.GetPathRoot(text);
						if (text2.Length == 0)
						{
							int num4 = text.IndexOf(":/", StringComparison.Ordinal);
							if (num4 == -1)
							{
								num4 = text.IndexOf(":\\", StringComparison.Ordinal);
							}
							int length = text.Length;
							for (num4++; num4 < length && (text[num4] == '/' || text[num4] == '\\'); num4++)
							{
							}
							for (; num4 < length && text[num4] != '/' && text[num4] != '\\'; num4++)
							{
							}
							text2 = text.Substring(0, num4);
						}
						stringBuilder2.Append(text2);
						flag3 = true;
					}
					else
					{
						stringBuilder2.Append(text);
					}
					int num5 = stringBuilder2.Length - 1;
					if (stringBuilder2[num5] != '/' && stringBuilder2[num5] != '\\')
					{
						if (!flag3)
						{
							if (text.IndexOf(":/", StringComparison.Ordinal) == -1)
							{
								stringBuilder2.Append('\\');
							}
							else
							{
								stringBuilder2.Append('/');
							}
						}
					}
					else if (flag3)
					{
						stringBuilder2.Remove(num5, 1);
					}
					stringBuilder2.Append(path);
					path = stringBuilder2.ToString();
				}
				else
				{
					path = Path.GetFullPathInternal(path);
				}
			}
			return path;
		}

		private bool IsFilePath(string path)
		{
			if (path[1] != ':')
			{
				if (path[0] == '\\')
				{
					return path[1] == '\\';
				}
				return false;
			}
			return true;
		}

		public byte[] GetConfigurationBytes()
		{
			if (_ConfigurationBytes == null)
			{
				return null;
			}
			return (byte[])_ConfigurationBytes.Clone();
		}

		public void SetConfigurationBytes(byte[] value)
		{
			_ConfigurationBytes = value;
		}

		private string VerifyDir(string dir, bool normalize)
		{
			if (dir != null)
			{
				if (dir.Length == 0)
				{
					dir = null;
				}
				else
				{
					if (normalize)
					{
						dir = NormalizePath(dir, useAppBase: true);
					}
					if (IsFilePath(dir))
					{
						new FileIOPermission(FileIOPermissionAccess.PathDiscovery, dir).Demand();
					}
				}
			}
			return dir;
		}

		private void VerifyDirList(string dirs)
		{
			if (dirs != null)
			{
				string[] array = dirs.Split(';');
				int num = array.Length;
				for (int i = 0; i < num; i++)
				{
					VerifyDir(array[i], normalize: true);
				}
			}
		}

		internal void SetupFusionContext(IntPtr fusionContext)
		{
			string text = Value[0];
			if (text != null)
			{
				UpdateContextProperty(fusionContext, ApplicationBaseKey, text);
			}
			string text2 = Value[5];
			if (text2 != null)
			{
				UpdateContextProperty(fusionContext, PrivateBinPathKey, text2);
			}
			string text3 = Value[3];
			if (text3 != null)
			{
				UpdateContextProperty(fusionContext, DeveloperPathKey, text3);
			}
			if (DisallowPublisherPolicy)
			{
				UpdateContextProperty(fusionContext, DisallowPublisherPolicyKey, "true");
			}
			if (DisallowCodeDownload)
			{
				UpdateContextProperty(fusionContext, DisallowCodeDownloadKey, "true");
			}
			if (DisallowBindingRedirects)
			{
				UpdateContextProperty(fusionContext, DisallowBindingRedirectsKey, "true");
			}
			if (DisallowApplicationBaseProbing)
			{
				UpdateContextProperty(fusionContext, DisallowAppBaseProbingKey, "true");
			}
			if (ShadowCopyFiles != null)
			{
				UpdateContextProperty(fusionContext, ShadowCopyFilesKey, ShadowCopyFiles);
				if (Value[7] == null)
				{
					ShadowCopyDirectories = BuildShadowCopyDirectories();
				}
				string text4 = Value[7];
				if (text4 != null)
				{
					UpdateContextProperty(fusionContext, ShadowCopyDirectoriesKey, text4);
				}
			}
			string text5 = Value[9];
			if (text5 != null)
			{
				UpdateContextProperty(fusionContext, CachePathKey, text5);
			}
			if (PrivateBinPathProbe != null)
			{
				UpdateContextProperty(fusionContext, PrivateBinPathProbeKey, PrivateBinPathProbe);
			}
			string text6 = Value[1];
			if (text6 != null)
			{
				UpdateContextProperty(fusionContext, ConfigurationFileKey, text6);
			}
			if (_ConfigurationBytes != null)
			{
				UpdateContextProperty(fusionContext, ConfigurationBytesKey, _ConfigurationBytes);
			}
			if (ApplicationName != null)
			{
				UpdateContextProperty(fusionContext, ApplicationNameKey, ApplicationName);
			}
			string text7 = Value[2];
			if (text7 != null)
			{
				UpdateContextProperty(fusionContext, DynamicBaseKey, text7);
			}
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(RuntimeEnvironment.GetRuntimeDirectoryImpl());
			stringBuilder.Append(RuntimeConfigurationFile);
			UpdateContextProperty(fusionContext, MachineConfigKey, stringBuilder.ToString());
			string hostBindingFile = RuntimeEnvironment.GetHostBindingFile();
			if (hostBindingFile != null)
			{
				UpdateContextProperty(fusionContext, HostBindingKey, hostBindingFile);
			}
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern void UpdateContextProperty(IntPtr fusionContext, string key, object value);

		internal static int Locate(string s)
		{
			if (string.IsNullOrEmpty(s))
			{
				return -1;
			}
			switch (s[0])
			{
			case 'A':
				switch (s)
				{
				case "APP_CONFIG_FILE":
					return 1;
				case "APP_NAME":
					return 4;
				case "APPBASE":
					return 0;
				case "APP_CONFIG_BLOB":
					return 15;
				}
				break;
			case 'B':
				if (s == "BINPATH_PROBE_ONLY")
				{
					return 6;
				}
				break;
			case 'C':
				if (s == "CACHE_BASE")
				{
					return 9;
				}
				if (s == "CODE_DOWNLOAD_DISABLED")
				{
					return 12;
				}
				break;
			case 'D':
				switch (s)
				{
				case "DEV_PATH":
					return 3;
				case "DYNAMIC_BASE":
					return 2;
				case "DISALLOW_APP":
					return 11;
				case "DISALLOW_APP_REDIRECTS":
					return 13;
				case "DISALLOW_APP_BASE_PROBING":
					return 14;
				}
				break;
			case 'F':
				if (s == "FORCE_CACHE_INSTALL")
				{
					return 8;
				}
				break;
			case 'L':
				if (s == "LICENSE_FILE")
				{
					return 10;
				}
				break;
			case 'P':
				if (s == "PRIVATE_BINPATH")
				{
					return 5;
				}
				break;
			case 'S':
				if (s == "SHADOW_COPY_DIRS")
				{
					return 7;
				}
				break;
			}
			return -1;
		}

		private string BuildShadowCopyDirectories()
		{
			string text = Value[5];
			if (text == null)
			{
				return null;
			}
			StringBuilder stringBuilder = new StringBuilder();
			string text2 = Value[0];
			if (text2 != null)
			{
				char[] separator = new char[1]
				{
					';'
				};
				string[] array = text.Split(separator);
				int num = array.Length;
				bool flag = text2[text2.Length - 1] != '/' && text2[text2.Length - 1] != '\\';
				if (num == 0)
				{
					stringBuilder.Append(text2);
					if (flag)
					{
						stringBuilder.Append('\\');
					}
					stringBuilder.Append(text);
				}
				else
				{
					for (int i = 0; i < num; i++)
					{
						stringBuilder.Append(text2);
						if (flag)
						{
							stringBuilder.Append('\\');
						}
						stringBuilder.Append(array[i]);
						if (i < num - 1)
						{
							stringBuilder.Append(';');
						}
					}
				}
			}
			return stringBuilder.ToString();
		}
	}
}
