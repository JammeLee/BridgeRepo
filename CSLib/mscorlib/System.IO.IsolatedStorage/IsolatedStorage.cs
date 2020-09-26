using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security;
using System.Security.Cryptography;
using System.Security.Permissions;
using System.Security.Policy;
using System.Text;
using System.Threading;

namespace System.IO.IsolatedStorage
{
	[ComVisible(true)]
	public abstract class IsolatedStorage : MarshalByRefObject
	{
		internal const IsolatedStorageScope c_Assembly = IsolatedStorageScope.User | IsolatedStorageScope.Assembly;

		internal const IsolatedStorageScope c_Domain = IsolatedStorageScope.User | IsolatedStorageScope.Domain | IsolatedStorageScope.Assembly;

		internal const IsolatedStorageScope c_AssemblyRoaming = IsolatedStorageScope.User | IsolatedStorageScope.Assembly | IsolatedStorageScope.Roaming;

		internal const IsolatedStorageScope c_DomainRoaming = IsolatedStorageScope.User | IsolatedStorageScope.Domain | IsolatedStorageScope.Assembly | IsolatedStorageScope.Roaming;

		internal const IsolatedStorageScope c_MachineAssembly = IsolatedStorageScope.Assembly | IsolatedStorageScope.Machine;

		internal const IsolatedStorageScope c_MachineDomain = IsolatedStorageScope.Domain | IsolatedStorageScope.Assembly | IsolatedStorageScope.Machine;

		internal const IsolatedStorageScope c_AppUser = IsolatedStorageScope.User | IsolatedStorageScope.Application;

		internal const IsolatedStorageScope c_AppMachine = IsolatedStorageScope.Machine | IsolatedStorageScope.Application;

		internal const IsolatedStorageScope c_AppUserRoaming = IsolatedStorageScope.User | IsolatedStorageScope.Roaming | IsolatedStorageScope.Application;

		private const string s_Publisher = "Publisher";

		private const string s_StrongName = "StrongName";

		private const string s_Site = "Site";

		private const string s_Url = "Url";

		private const string s_Zone = "Zone";

		private static char[] s_Base32Char = new char[32]
		{
			'a',
			'b',
			'c',
			'd',
			'e',
			'f',
			'g',
			'h',
			'i',
			'j',
			'k',
			'l',
			'm',
			'n',
			'o',
			'p',
			'q',
			'r',
			's',
			't',
			'u',
			'v',
			'w',
			'x',
			'y',
			'z',
			'0',
			'1',
			'2',
			'3',
			'4',
			'5'
		};

		private ulong m_Quota;

		private bool m_ValidQuota;

		private object m_DomainIdentity;

		private object m_AssemIdentity;

		private object m_AppIdentity;

		private string m_DomainName;

		private string m_AssemName;

		private string m_AppName;

		private IsolatedStorageScope m_Scope;

		private static IsolatedStorageFilePermission s_PermDomain;

		private static IsolatedStorageFilePermission s_PermMachineDomain;

		private static IsolatedStorageFilePermission s_PermDomainRoaming;

		private static IsolatedStorageFilePermission s_PermAssem;

		private static IsolatedStorageFilePermission s_PermMachineAssem;

		private static IsolatedStorageFilePermission s_PermAssemRoaming;

		private static IsolatedStorageFilePermission s_PermAppUser;

		private static IsolatedStorageFilePermission s_PermAppMachine;

		private static IsolatedStorageFilePermission s_PermAppUserRoaming;

		private static SecurityPermission s_PermControlEvidence;

		private static PermissionSet s_PermReflection;

		private static PermissionSet s_PermUnrestricted;

		private static PermissionSet s_PermExecution;

		protected virtual char SeparatorExternal => '\\';

		protected virtual char SeparatorInternal => '.';

		[CLSCompliant(false)]
		public virtual ulong MaximumSize
		{
			get
			{
				if (m_ValidQuota)
				{
					return m_Quota;
				}
				throw new InvalidOperationException(Environment.GetResourceString("IsolatedStorage_QuotaIsUndefined"));
			}
		}

		[CLSCompliant(false)]
		public virtual ulong CurrentSize
		{
			get
			{
				throw new InvalidOperationException(Environment.GetResourceString("IsolatedStorage_CurrentSizeUndefined"));
			}
		}

		public object DomainIdentity
		{
			[SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.ControlPolicy)]
			get
			{
				if (IsDomain())
				{
					return m_DomainIdentity;
				}
				throw new InvalidOperationException(Environment.GetResourceString("IsolatedStorage_DomainUndefined"));
			}
		}

		[ComVisible(false)]
		public object ApplicationIdentity
		{
			[SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.ControlPolicy)]
			get
			{
				if (IsApp())
				{
					return m_AppIdentity;
				}
				throw new InvalidOperationException(Environment.GetResourceString("IsolatedStorage_ApplicationUndefined"));
			}
		}

		public object AssemblyIdentity
		{
			[SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.ControlPolicy)]
			get
			{
				if (IsAssembly())
				{
					return m_AssemIdentity;
				}
				throw new InvalidOperationException(Environment.GetResourceString("IsolatedStorage_AssemblyUndefined"));
			}
		}

		public IsolatedStorageScope Scope => m_Scope;

		internal string DomainName
		{
			get
			{
				if (IsDomain())
				{
					return m_DomainName;
				}
				throw new InvalidOperationException(Environment.GetResourceString("IsolatedStorage_DomainUndefined"));
			}
		}

		internal string AssemName
		{
			get
			{
				if (IsAssembly())
				{
					return m_AssemName;
				}
				throw new InvalidOperationException(Environment.GetResourceString("IsolatedStorage_AssemblyUndefined"));
			}
		}

		internal string AppName
		{
			get
			{
				if (IsApp())
				{
					return m_AppName;
				}
				throw new InvalidOperationException(Environment.GetResourceString("IsolatedStorage_ApplicationUndefined"));
			}
		}

		internal static bool IsRoaming(IsolatedStorageScope scope)
		{
			return (scope & IsolatedStorageScope.Roaming) != 0;
		}

		internal bool IsRoaming()
		{
			return (m_Scope & IsolatedStorageScope.Roaming) != 0;
		}

		internal static bool IsDomain(IsolatedStorageScope scope)
		{
			return (scope & IsolatedStorageScope.Domain) != 0;
		}

		internal bool IsDomain()
		{
			return (m_Scope & IsolatedStorageScope.Domain) != 0;
		}

		internal static bool IsMachine(IsolatedStorageScope scope)
		{
			return (scope & IsolatedStorageScope.Machine) != 0;
		}

		internal bool IsAssembly()
		{
			return (m_Scope & IsolatedStorageScope.Assembly) != 0;
		}

		internal static bool IsApp(IsolatedStorageScope scope)
		{
			return (scope & IsolatedStorageScope.Application) != 0;
		}

		internal bool IsApp()
		{
			return (m_Scope & IsolatedStorageScope.Application) != 0;
		}

		private string GetNameFromID(string typeID, string instanceID)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(typeID);
			stringBuilder.Append(SeparatorInternal);
			stringBuilder.Append(instanceID);
			return stringBuilder.ToString();
		}

		private static string GetPredefinedTypeName(object o)
		{
			if (o is Publisher)
			{
				return "Publisher";
			}
			if (o is StrongName)
			{
				return "StrongName";
			}
			if (o is Url)
			{
				return "Url";
			}
			if (o is Site)
			{
				return "Site";
			}
			if (o is Zone)
			{
				return "Zone";
			}
			return null;
		}

		internal static string GetHash(Stream s)
		{
			using SHA1 sHA = new SHA1CryptoServiceProvider();
			byte[] buff = sHA.ComputeHash(s);
			return ToBase32StringSuitableForDirName(buff);
		}

		internal static string ToBase32StringSuitableForDirName(byte[] buff)
		{
			StringBuilder stringBuilder = new StringBuilder();
			int num = buff.Length;
			int num2 = 0;
			do
			{
				byte b = (byte)((num2 < num) ? buff[num2++] : 0);
				byte b2 = (byte)((num2 < num) ? buff[num2++] : 0);
				byte b3 = (byte)((num2 < num) ? buff[num2++] : 0);
				byte b4 = (byte)((num2 < num) ? buff[num2++] : 0);
				byte b5 = (byte)((num2 < num) ? buff[num2++] : 0);
				stringBuilder.Append(s_Base32Char[b & 0x1F]);
				stringBuilder.Append(s_Base32Char[b2 & 0x1F]);
				stringBuilder.Append(s_Base32Char[b3 & 0x1F]);
				stringBuilder.Append(s_Base32Char[b4 & 0x1F]);
				stringBuilder.Append(s_Base32Char[b5 & 0x1F]);
				stringBuilder.Append(s_Base32Char[((b & 0xE0) >> 5) | ((b4 & 0x60) >> 2)]);
				stringBuilder.Append(s_Base32Char[((b2 & 0xE0) >> 5) | ((b5 & 0x60) >> 2)]);
				b3 = (byte)(b3 >> 5);
				if ((b4 & 0x80u) != 0)
				{
					b3 = (byte)(b3 | 8u);
				}
				if ((b5 & 0x80u) != 0)
				{
					b3 = (byte)(b3 | 0x10u);
				}
				stringBuilder.Append(s_Base32Char[b3]);
			}
			while (num2 < num);
			return stringBuilder.ToString();
		}

		private static bool IsValidName(string s)
		{
			for (int i = 0; i < s.Length; i++)
			{
				if (!char.IsLetter(s[i]) && !char.IsDigit(s[i]))
				{
					return false;
				}
			}
			return true;
		}

		private static PermissionSet GetReflectionPermission()
		{
			if (s_PermReflection == null)
			{
				s_PermReflection = new PermissionSet(PermissionState.Unrestricted);
			}
			return s_PermReflection;
		}

		private static SecurityPermission GetControlEvidencePermission()
		{
			if (s_PermControlEvidence == null)
			{
				s_PermControlEvidence = new SecurityPermission(SecurityPermissionFlag.ControlEvidence);
			}
			return s_PermControlEvidence;
		}

		private static PermissionSet GetExecutionPermission()
		{
			if (s_PermExecution == null)
			{
				s_PermExecution = new PermissionSet(PermissionState.None);
				s_PermExecution.AddPermission(new SecurityPermission(SecurityPermissionFlag.Execution));
			}
			return s_PermExecution;
		}

		private static PermissionSet GetUnrestricted()
		{
			if (s_PermUnrestricted == null)
			{
				s_PermUnrestricted = new PermissionSet(PermissionState.Unrestricted);
			}
			return s_PermUnrestricted;
		}

		internal MemoryStream GetIdentityStream(IsolatedStorageScope scope)
		{
			GetReflectionPermission().Assert();
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			MemoryStream memoryStream = new MemoryStream();
			object obj = (IsApp(scope) ? m_AppIdentity : ((!IsDomain(scope)) ? m_AssemIdentity : m_DomainIdentity));
			if (obj != null)
			{
				binaryFormatter.Serialize(memoryStream, obj);
			}
			memoryStream.Position = 0L;
			return memoryStream;
		}

		protected void InitStore(IsolatedStorageScope scope, Type domainEvidenceType, Type assemblyEvidenceType)
		{
			PermissionSet granted = null;
			PermissionSet denied = null;
			Assembly assembly = nGetCaller();
			GetControlEvidencePermission().Assert();
			if (IsDomain(scope))
			{
				AppDomain domain = Thread.GetDomain();
				if (!IsRoaming(scope))
				{
					domain.nGetGrantSet(out granted, out denied);
					if (granted == null)
					{
						throw new IsolatedStorageException(Environment.GetResourceString("IsolatedStorage_DomainGrantSet"));
					}
				}
				_InitStore(scope, domain.Evidence, domainEvidenceType, assembly.Evidence, assemblyEvidenceType, null, null);
			}
			else
			{
				if (!IsRoaming(scope))
				{
					assembly.nGetGrantSet(out granted, out denied);
					if (granted == null)
					{
						throw new IsolatedStorageException(Environment.GetResourceString("IsolatedStorage_AssemblyGrantSet"));
					}
				}
				_InitStore(scope, null, null, assembly.Evidence, assemblyEvidenceType, null, null);
			}
			SetQuota(granted, denied);
		}

		protected void InitStore(IsolatedStorageScope scope, Type appEvidenceType)
		{
			PermissionSet granted = null;
			PermissionSet denied = null;
			nGetCaller();
			GetControlEvidencePermission().Assert();
			if (IsApp(scope))
			{
				AppDomain domain = Thread.GetDomain();
				if (!IsRoaming(scope))
				{
					domain.nGetGrantSet(out granted, out denied);
					if (granted == null)
					{
						throw new IsolatedStorageException(Environment.GetResourceString("IsolatedStorage_DomainGrantSet"));
					}
				}
				ActivationContext activationContext = AppDomain.CurrentDomain.ActivationContext;
				if (activationContext == null)
				{
					throw new IsolatedStorageException(Environment.GetResourceString("IsolatedStorage_ApplicationMissingIdentity"));
				}
				ApplicationSecurityInfo applicationSecurityInfo = new ApplicationSecurityInfo(activationContext);
				_InitStore(scope, null, null, null, null, applicationSecurityInfo.ApplicationEvidence, appEvidenceType);
			}
			SetQuota(granted, denied);
		}

		internal void InitStore(IsolatedStorageScope scope, object domain, object assem, object app)
		{
			PermissionSet newGrant = null;
			PermissionSet newDenied = null;
			Evidence evidence = null;
			Evidence evidence2 = null;
			Evidence evidence3 = null;
			if (IsApp(scope))
			{
				evidence3 = new Evidence();
				evidence3.AddHost(app);
			}
			else
			{
				evidence2 = new Evidence();
				evidence2.AddHost(assem);
				if (IsDomain(scope))
				{
					evidence = new Evidence();
					evidence.AddHost(domain);
				}
			}
			_InitStore(scope, evidence, null, evidence2, null, evidence3, null);
			if (!IsRoaming(scope))
			{
				Assembly assembly = nGetCaller();
				GetControlEvidencePermission().Assert();
				assembly.nGetGrantSet(out newGrant, out newDenied);
				if (newGrant == null)
				{
					throw new IsolatedStorageException(Environment.GetResourceString("IsolatedStorage_AssemblyGrantSet"));
				}
			}
			SetQuota(newGrant, newDenied);
		}

		internal void InitStore(IsolatedStorageScope scope, Evidence domainEv, Type domainEvidenceType, Evidence assemEv, Type assemEvidenceType, Evidence appEv, Type appEvidenceType)
		{
			PermissionSet permissionSet = null;
			PermissionSet denied = null;
			if (!IsRoaming(scope))
			{
				if (IsApp(scope))
				{
					permissionSet = SecurityManager.ResolvePolicy(appEv, GetExecutionPermission(), GetUnrestricted(), null, out denied);
					if (permissionSet == null)
					{
						throw new IsolatedStorageException(Environment.GetResourceString("IsolatedStorage_ApplicationGrantSet"));
					}
				}
				else if (IsDomain(scope))
				{
					permissionSet = SecurityManager.ResolvePolicy(domainEv, GetExecutionPermission(), GetUnrestricted(), null, out denied);
					if (permissionSet == null)
					{
						throw new IsolatedStorageException(Environment.GetResourceString("IsolatedStorage_DomainGrantSet"));
					}
				}
				else
				{
					permissionSet = SecurityManager.ResolvePolicy(assemEv, GetExecutionPermission(), GetUnrestricted(), null, out denied);
					if (permissionSet == null)
					{
						throw new IsolatedStorageException(Environment.GetResourceString("IsolatedStorage_AssemblyGrantSet"));
					}
				}
			}
			_InitStore(scope, domainEv, domainEvidenceType, assemEv, assemEvidenceType, appEv, appEvidenceType);
			SetQuota(permissionSet, denied);
		}

		internal bool InitStore(IsolatedStorageScope scope, Stream domain, Stream assem, Stream app, string domainName, string assemName, string appName)
		{
			try
			{
				GetReflectionPermission().Assert();
				BinaryFormatter binaryFormatter = new BinaryFormatter();
				if (IsApp(scope))
				{
					m_AppIdentity = binaryFormatter.Deserialize(app);
					m_AppName = appName;
				}
				else
				{
					m_AssemIdentity = binaryFormatter.Deserialize(assem);
					m_AssemName = assemName;
					if (IsDomain(scope))
					{
						m_DomainIdentity = binaryFormatter.Deserialize(domain);
						m_DomainName = domainName;
					}
				}
			}
			catch
			{
				return false;
			}
			m_Scope = scope;
			return true;
		}

		private void _InitStore(IsolatedStorageScope scope, Evidence domainEv, Type domainEvidenceType, Evidence assemEv, Type assemblyEvidenceType, Evidence appEv, Type appEvidenceType)
		{
			VerifyScope(scope);
			if (IsApp(scope))
			{
				if (appEv == null)
				{
					throw new IsolatedStorageException(Environment.GetResourceString("IsolatedStorage_ApplicationMissingIdentity"));
				}
			}
			else
			{
				if (assemEv == null)
				{
					throw new IsolatedStorageException(Environment.GetResourceString("IsolatedStorage_AssemblyMissingIdentity"));
				}
				if (IsDomain(scope) && domainEv == null)
				{
					throw new IsolatedStorageException(Environment.GetResourceString("IsolatedStorage_DomainMissingIdentity"));
				}
			}
			DemandPermission(scope);
			string typeName = null;
			string instanceName = null;
			if (IsApp(scope))
			{
				m_AppIdentity = GetAccountingInfo(appEv, appEvidenceType, IsolatedStorageScope.Application, out typeName, out instanceName);
				m_AppName = GetNameFromID(typeName, instanceName);
			}
			else
			{
				m_AssemIdentity = GetAccountingInfo(assemEv, assemblyEvidenceType, IsolatedStorageScope.Assembly, out typeName, out instanceName);
				m_AssemName = GetNameFromID(typeName, instanceName);
				if (IsDomain(scope))
				{
					m_DomainIdentity = GetAccountingInfo(domainEv, domainEvidenceType, IsolatedStorageScope.Domain, out typeName, out instanceName);
					m_DomainName = GetNameFromID(typeName, instanceName);
				}
			}
			m_Scope = scope;
		}

		private static object GetAccountingInfo(Evidence evidence, Type evidenceType, IsolatedStorageScope fAssmDomApp, out string typeName, out string instanceName)
		{
			object oNormalized = null;
			object obj = _GetAccountingInfo(evidence, evidenceType, fAssmDomApp, out oNormalized);
			typeName = GetPredefinedTypeName(obj);
			if (typeName == null)
			{
				GetReflectionPermission().Assert();
				MemoryStream memoryStream = new MemoryStream();
				BinaryFormatter binaryFormatter = new BinaryFormatter();
				binaryFormatter.Serialize(memoryStream, obj.GetType());
				memoryStream.Position = 0L;
				typeName = GetHash(memoryStream);
			}
			instanceName = null;
			if (oNormalized != null)
			{
				if (oNormalized is Stream)
				{
					instanceName = GetHash((Stream)oNormalized);
				}
				else if (oNormalized is string)
				{
					if (IsValidName((string)oNormalized))
					{
						instanceName = (string)oNormalized;
					}
					else
					{
						MemoryStream memoryStream = new MemoryStream();
						BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
						binaryWriter.Write((string)oNormalized);
						memoryStream.Position = 0L;
						instanceName = GetHash(memoryStream);
					}
				}
			}
			else
			{
				oNormalized = obj;
			}
			if (instanceName == null)
			{
				GetReflectionPermission().Assert();
				MemoryStream memoryStream = new MemoryStream();
				BinaryFormatter binaryFormatter = new BinaryFormatter();
				binaryFormatter.Serialize(memoryStream, oNormalized);
				memoryStream.Position = 0L;
				instanceName = GetHash(memoryStream);
			}
			return obj;
		}

		private static object _GetAccountingInfo(Evidence evidence, Type evidenceType, IsolatedStorageScope fAssmDomApp, out object oNormalized)
		{
			object obj = null;
			IEnumerator hostEnumerator = evidence.GetHostEnumerator();
			if (evidenceType == null)
			{
				Publisher publisher = null;
				StrongName strongName = null;
				Url url = null;
				Site site = null;
				Zone zone = null;
				while (hostEnumerator.MoveNext())
				{
					obj = hostEnumerator.Current;
					if (obj is Publisher)
					{
						publisher = (Publisher)obj;
						break;
					}
					if (obj is StrongName)
					{
						strongName = (StrongName)obj;
					}
					else if (obj is Url)
					{
						url = (Url)obj;
					}
					else if (obj is Site)
					{
						site = (Site)obj;
					}
					else if (obj is Zone)
					{
						zone = (Zone)obj;
					}
				}
				if (publisher != null)
				{
					obj = publisher;
				}
				else if (strongName != null)
				{
					obj = strongName;
				}
				else if (url != null)
				{
					obj = url;
				}
				else if (site != null)
				{
					obj = site;
				}
				else
				{
					if (zone == null)
					{
						switch (fAssmDomApp)
						{
						case IsolatedStorageScope.Domain:
							throw new IsolatedStorageException(Environment.GetResourceString("IsolatedStorage_DomainNoEvidence"));
						case IsolatedStorageScope.Application:
							throw new IsolatedStorageException(Environment.GetResourceString("IsolatedStorage_ApplicationNoEvidence"));
						default:
							throw new IsolatedStorageException(Environment.GetResourceString("IsolatedStorage_AssemblyNoEvidence"));
						}
					}
					obj = zone;
				}
			}
			else
			{
				while (hostEnumerator.MoveNext())
				{
					object current = hostEnumerator.Current;
					if (current.GetType().Equals(evidenceType))
					{
						obj = current;
						break;
					}
				}
				if (obj == null)
				{
					switch (fAssmDomApp)
					{
					case IsolatedStorageScope.Domain:
						throw new IsolatedStorageException(Environment.GetResourceString("IsolatedStorage_DomainNoEvidence"));
					case IsolatedStorageScope.Application:
						throw new IsolatedStorageException(Environment.GetResourceString("IsolatedStorage_ApplicationNoEvidence"));
					default:
						throw new IsolatedStorageException(Environment.GetResourceString("IsolatedStorage_AssemblyNoEvidence"));
					}
				}
			}
			if (obj is INormalizeForIsolatedStorage)
			{
				oNormalized = ((INormalizeForIsolatedStorage)obj).Normalize();
			}
			else if (obj is Publisher)
			{
				oNormalized = ((Publisher)obj).Normalize();
			}
			else if (obj is StrongName)
			{
				oNormalized = ((StrongName)obj).Normalize();
			}
			else if (obj is Url)
			{
				oNormalized = ((Url)obj).Normalize();
			}
			else if (obj is Site)
			{
				oNormalized = ((Site)obj).Normalize();
			}
			else if (obj is Zone)
			{
				oNormalized = ((Zone)obj).Normalize();
			}
			else
			{
				oNormalized = null;
			}
			return obj;
		}

		private static void DemandPermission(IsolatedStorageScope scope)
		{
			IsolatedStorageFilePermission isolatedStorageFilePermission = null;
			switch (scope)
			{
			case IsolatedStorageScope.User | IsolatedStorageScope.Domain | IsolatedStorageScope.Assembly:
				if (s_PermDomain == null)
				{
					s_PermDomain = new IsolatedStorageFilePermission(IsolatedStorageContainment.DomainIsolationByUser, 0L, PermanentData: false);
				}
				isolatedStorageFilePermission = s_PermDomain;
				break;
			case IsolatedStorageScope.User | IsolatedStorageScope.Assembly:
				if (s_PermAssem == null)
				{
					s_PermAssem = new IsolatedStorageFilePermission(IsolatedStorageContainment.AssemblyIsolationByUser, 0L, PermanentData: false);
				}
				isolatedStorageFilePermission = s_PermAssem;
				break;
			case IsolatedStorageScope.User | IsolatedStorageScope.Domain | IsolatedStorageScope.Assembly | IsolatedStorageScope.Roaming:
				if (s_PermDomainRoaming == null)
				{
					s_PermDomainRoaming = new IsolatedStorageFilePermission(IsolatedStorageContainment.DomainIsolationByRoamingUser, 0L, PermanentData: false);
				}
				isolatedStorageFilePermission = s_PermDomainRoaming;
				break;
			case IsolatedStorageScope.User | IsolatedStorageScope.Assembly | IsolatedStorageScope.Roaming:
				if (s_PermAssemRoaming == null)
				{
					s_PermAssemRoaming = new IsolatedStorageFilePermission(IsolatedStorageContainment.AssemblyIsolationByRoamingUser, 0L, PermanentData: false);
				}
				isolatedStorageFilePermission = s_PermAssemRoaming;
				break;
			case IsolatedStorageScope.Domain | IsolatedStorageScope.Assembly | IsolatedStorageScope.Machine:
				if (s_PermMachineDomain == null)
				{
					s_PermMachineDomain = new IsolatedStorageFilePermission(IsolatedStorageContainment.DomainIsolationByMachine, 0L, PermanentData: false);
				}
				isolatedStorageFilePermission = s_PermMachineDomain;
				break;
			case IsolatedStorageScope.Assembly | IsolatedStorageScope.Machine:
				if (s_PermMachineAssem == null)
				{
					s_PermMachineAssem = new IsolatedStorageFilePermission(IsolatedStorageContainment.AssemblyIsolationByMachine, 0L, PermanentData: false);
				}
				isolatedStorageFilePermission = s_PermMachineAssem;
				break;
			case IsolatedStorageScope.User | IsolatedStorageScope.Application:
				if (s_PermAppUser == null)
				{
					s_PermAppUser = new IsolatedStorageFilePermission(IsolatedStorageContainment.ApplicationIsolationByUser, 0L, PermanentData: false);
				}
				isolatedStorageFilePermission = s_PermAppUser;
				break;
			case IsolatedStorageScope.Machine | IsolatedStorageScope.Application:
				if (s_PermAppMachine == null)
				{
					s_PermAppMachine = new IsolatedStorageFilePermission(IsolatedStorageContainment.ApplicationIsolationByMachine, 0L, PermanentData: false);
				}
				isolatedStorageFilePermission = s_PermAppMachine;
				break;
			case IsolatedStorageScope.User | IsolatedStorageScope.Roaming | IsolatedStorageScope.Application:
				if (s_PermAppUserRoaming == null)
				{
					s_PermAppUserRoaming = new IsolatedStorageFilePermission(IsolatedStorageContainment.ApplicationIsolationByRoamingUser, 0L, PermanentData: false);
				}
				isolatedStorageFilePermission = s_PermAppUserRoaming;
				break;
			}
			isolatedStorageFilePermission.Demand();
		}

		internal static void VerifyScope(IsolatedStorageScope scope)
		{
			if (scope == (IsolatedStorageScope.User | IsolatedStorageScope.Domain | IsolatedStorageScope.Assembly) || scope == (IsolatedStorageScope.User | IsolatedStorageScope.Assembly) || scope == (IsolatedStorageScope.User | IsolatedStorageScope.Domain | IsolatedStorageScope.Assembly | IsolatedStorageScope.Roaming) || scope == (IsolatedStorageScope.User | IsolatedStorageScope.Assembly | IsolatedStorageScope.Roaming) || scope == (IsolatedStorageScope.Domain | IsolatedStorageScope.Assembly | IsolatedStorageScope.Machine) || scope == (IsolatedStorageScope.Assembly | IsolatedStorageScope.Machine) || scope == (IsolatedStorageScope.User | IsolatedStorageScope.Application) || scope == (IsolatedStorageScope.Machine | IsolatedStorageScope.Application) || scope == (IsolatedStorageScope.User | IsolatedStorageScope.Roaming | IsolatedStorageScope.Application))
			{
				return;
			}
			throw new ArgumentException(Environment.GetResourceString("IsolatedStorage_Scope_Invalid"));
		}

		internal void SetQuota(PermissionSet psAllowed, PermissionSet psDenied)
		{
			IsolatedStoragePermission permission = GetPermission(psAllowed);
			m_Quota = 0uL;
			if (permission != null)
			{
				if (permission.IsUnrestricted())
				{
					m_Quota = 9223372036854775807uL;
				}
				else
				{
					m_Quota = (ulong)permission.UserQuota;
				}
			}
			if (psDenied != null)
			{
				IsolatedStoragePermission permission2 = GetPermission(psDenied);
				if (permission2 != null)
				{
					if (permission2.IsUnrestricted())
					{
						m_Quota = 0uL;
					}
					else
					{
						ulong userQuota = (ulong)permission2.UserQuota;
						if (userQuota > m_Quota)
						{
							m_Quota = 0uL;
						}
						else
						{
							m_Quota -= userQuota;
						}
					}
				}
			}
			m_ValidQuota = true;
		}

		public abstract void Remove();

		protected abstract IsolatedStoragePermission GetPermission(PermissionSet ps);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern Assembly nGetCaller();
	}
}
