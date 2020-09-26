using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Security;
using System.Security.Permissions;
using System.Text;
using Microsoft.Win32;

namespace System.Diagnostics
{
	[TypeConverter(typeof(ExpandableObjectConverter))]
	[HostProtection(SecurityAction.LinkDemand, SharedState = true, SelfAffectingProcessMgmt = true)]
	[PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
	public sealed class ProcessStartInfo
	{
		private string fileName;

		private string arguments;

		private string directory;

		private string verb;

		private ProcessWindowStyle windowStyle;

		private bool errorDialog;

		private IntPtr errorDialogParentHandle;

		private bool useShellExecute = true;

		private string userName;

		private string domain;

		private SecureString password;

		private bool loadUserProfile;

		private bool redirectStandardInput;

		private bool redirectStandardOutput;

		private bool redirectStandardError;

		private Encoding standardOutputEncoding;

		private Encoding standardErrorEncoding;

		private bool createNoWindow;

		private WeakReference weakParentProcess;

		internal StringDictionary environmentVariables;

		[NotifyParentProperty(true)]
		[DefaultValue("")]
		[TypeConverter("System.Diagnostics.Design.VerbConverter, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
		[MonitoringDescription("ProcessVerb")]
		public string Verb
		{
			get
			{
				if (verb == null)
				{
					return string.Empty;
				}
				return verb;
			}
			set
			{
				verb = value;
			}
		}

		[DefaultValue("")]
		[TypeConverter("System.Diagnostics.Design.StringValueConverter, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
		[NotifyParentProperty(true)]
		[RecommendedAsConfigurable(true)]
		[MonitoringDescription("ProcessArguments")]
		public string Arguments
		{
			get
			{
				if (arguments == null)
				{
					return string.Empty;
				}
				return arguments;
			}
			set
			{
				arguments = value;
			}
		}

		[NotifyParentProperty(true)]
		[DefaultValue(false)]
		[MonitoringDescription("ProcessCreateNoWindow")]
		public bool CreateNoWindow
		{
			get
			{
				return createNoWindow;
			}
			set
			{
				createNoWindow = value;
			}
		}

		[MonitoringDescription("ProcessEnvironmentVariables")]
		[DefaultValue(null)]
		[NotifyParentProperty(true)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Editor("System.Diagnostics.Design.StringDictionaryEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
		public StringDictionary EnvironmentVariables
		{
			get
			{
				if (environmentVariables == null)
				{
					environmentVariables = new StringDictionary();
					if (weakParentProcess == null || !weakParentProcess.IsAlive || ((Component)weakParentProcess.Target).Site == null || !((Component)weakParentProcess.Target).Site.DesignMode)
					{
						foreach (DictionaryEntry environmentVariable in Environment.GetEnvironmentVariables())
						{
							environmentVariables.Add((string)environmentVariable.Key, (string)environmentVariable.Value);
						}
					}
				}
				return environmentVariables;
			}
		}

		[NotifyParentProperty(true)]
		[DefaultValue(false)]
		[MonitoringDescription("ProcessRedirectStandardInput")]
		public bool RedirectStandardInput
		{
			get
			{
				return redirectStandardInput;
			}
			set
			{
				redirectStandardInput = value;
			}
		}

		[NotifyParentProperty(true)]
		[MonitoringDescription("ProcessRedirectStandardOutput")]
		[DefaultValue(false)]
		public bool RedirectStandardOutput
		{
			get
			{
				return redirectStandardOutput;
			}
			set
			{
				redirectStandardOutput = value;
			}
		}

		[NotifyParentProperty(true)]
		[DefaultValue(false)]
		[MonitoringDescription("ProcessRedirectStandardError")]
		public bool RedirectStandardError
		{
			get
			{
				return redirectStandardError;
			}
			set
			{
				redirectStandardError = value;
			}
		}

		public Encoding StandardErrorEncoding
		{
			get
			{
				return standardErrorEncoding;
			}
			set
			{
				standardErrorEncoding = value;
			}
		}

		public Encoding StandardOutputEncoding
		{
			get
			{
				return standardOutputEncoding;
			}
			set
			{
				standardOutputEncoding = value;
			}
		}

		[DefaultValue(true)]
		[NotifyParentProperty(true)]
		[MonitoringDescription("ProcessUseShellExecute")]
		public bool UseShellExecute
		{
			get
			{
				return useShellExecute;
			}
			set
			{
				useShellExecute = value;
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public string[] Verbs
		{
			get
			{
				ArrayList arrayList = new ArrayList();
				RegistryKey registryKey = null;
				string extension = Path.GetExtension(FileName);
				try
				{
					if (extension != null && extension.Length > 0)
					{
						registryKey = Registry.ClassesRoot.OpenSubKey(extension);
						if (registryKey != null)
						{
							string str = (string)registryKey.GetValue(string.Empty);
							registryKey.Close();
							registryKey = Registry.ClassesRoot.OpenSubKey(str + "\\shell");
							if (registryKey != null)
							{
								string[] subKeyNames = registryKey.GetSubKeyNames();
								for (int i = 0; i < subKeyNames.Length; i++)
								{
									if (string.Compare(subKeyNames[i], "new", StringComparison.OrdinalIgnoreCase) != 0)
									{
										arrayList.Add(subKeyNames[i]);
									}
								}
								registryKey.Close();
								registryKey = null;
							}
						}
					}
				}
				finally
				{
					registryKey?.Close();
				}
				string[] array = new string[arrayList.Count];
				arrayList.CopyTo(array, 0);
				return array;
			}
		}

		[NotifyParentProperty(true)]
		public string UserName
		{
			get
			{
				if (userName == null)
				{
					return string.Empty;
				}
				return userName;
			}
			set
			{
				userName = value;
			}
		}

		public SecureString Password
		{
			get
			{
				return password;
			}
			set
			{
				password = value;
			}
		}

		[NotifyParentProperty(true)]
		public string Domain
		{
			get
			{
				if (domain == null)
				{
					return string.Empty;
				}
				return domain;
			}
			set
			{
				domain = value;
			}
		}

		[NotifyParentProperty(true)]
		public bool LoadUserProfile
		{
			get
			{
				return loadUserProfile;
			}
			set
			{
				loadUserProfile = value;
			}
		}

		[RecommendedAsConfigurable(true)]
		[DefaultValue("")]
		[Editor("System.Diagnostics.Design.StartFileNameEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
		[MonitoringDescription("ProcessFileName")]
		[TypeConverter("System.Diagnostics.Design.StringValueConverter, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
		[NotifyParentProperty(true)]
		public string FileName
		{
			get
			{
				if (fileName == null)
				{
					return string.Empty;
				}
				return fileName;
			}
			set
			{
				fileName = value;
			}
		}

		[DefaultValue("")]
		[NotifyParentProperty(true)]
		[MonitoringDescription("ProcessWorkingDirectory")]
		[Editor("System.Diagnostics.Design.WorkingDirectoryEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
		[RecommendedAsConfigurable(true)]
		[TypeConverter("System.Diagnostics.Design.StringValueConverter, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
		public string WorkingDirectory
		{
			get
			{
				if (directory == null)
				{
					return string.Empty;
				}
				return directory;
			}
			set
			{
				directory = value;
			}
		}

		[NotifyParentProperty(true)]
		[MonitoringDescription("ProcessErrorDialog")]
		[DefaultValue(false)]
		public bool ErrorDialog
		{
			get
			{
				return errorDialog;
			}
			set
			{
				errorDialog = value;
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public IntPtr ErrorDialogParentHandle
		{
			get
			{
				return errorDialogParentHandle;
			}
			set
			{
				errorDialogParentHandle = value;
			}
		}

		[DefaultValue(ProcessWindowStyle.Normal)]
		[MonitoringDescription("ProcessWindowStyle")]
		[NotifyParentProperty(true)]
		public ProcessWindowStyle WindowStyle
		{
			get
			{
				return windowStyle;
			}
			set
			{
				if (!Enum.IsDefined(typeof(ProcessWindowStyle), value))
				{
					throw new InvalidEnumArgumentException("value", (int)value, typeof(ProcessWindowStyle));
				}
				windowStyle = value;
			}
		}

		public ProcessStartInfo()
		{
		}

		internal ProcessStartInfo(Process parent)
		{
			weakParentProcess = new WeakReference(parent);
		}

		public ProcessStartInfo(string fileName)
		{
			this.fileName = fileName;
		}

		public ProcessStartInfo(string fileName, string arguments)
		{
			this.fileName = fileName;
			this.arguments = arguments;
		}
	}
}
