using System.Configuration.Assemblies;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace System.Reflection
{
	[Serializable]
	[ComVisible(true)]
	[ComDefaultInterface(typeof(_AssemblyName))]
	[ClassInterface(ClassInterfaceType.None)]
	public sealed class AssemblyName : _AssemblyName, ICloneable, ISerializable, IDeserializationCallback
	{
		private string _Name;

		private byte[] _PublicKey;

		private byte[] _PublicKeyToken;

		private CultureInfo _CultureInfo;

		private string _CodeBase;

		private Version _Version;

		private StrongNameKeyPair _StrongNameKeyPair;

		private SerializationInfo m_siInfo;

		private byte[] _HashForControl;

		private AssemblyHashAlgorithm _HashAlgorithm;

		private AssemblyHashAlgorithm _HashAlgorithmForControl;

		private AssemblyVersionCompatibility _VersionCompatibility;

		private AssemblyNameFlags _Flags;

		public string Name
		{
			get
			{
				return _Name;
			}
			set
			{
				_Name = value;
			}
		}

		public Version Version
		{
			get
			{
				return _Version;
			}
			set
			{
				_Version = value;
			}
		}

		public CultureInfo CultureInfo
		{
			get
			{
				return _CultureInfo;
			}
			set
			{
				_CultureInfo = value;
			}
		}

		public string CodeBase
		{
			get
			{
				return _CodeBase;
			}
			set
			{
				_CodeBase = value;
			}
		}

		public string EscapedCodeBase
		{
			get
			{
				if (_CodeBase == null)
				{
					return null;
				}
				return EscapeCodeBase(_CodeBase);
			}
		}

		public ProcessorArchitecture ProcessorArchitecture
		{
			get
			{
				int num = (int)(_Flags & (AssemblyNameFlags)112) >> 4;
				if (num > 4)
				{
					num = 0;
				}
				return (ProcessorArchitecture)num;
			}
			set
			{
				int num = (int)(value & (ProcessorArchitecture)7);
				if (num <= 4)
				{
					_Flags = (AssemblyNameFlags)((long)_Flags & 0xFFFFFF0FL);
					_Flags |= (AssemblyNameFlags)(num << 4);
				}
			}
		}

		public AssemblyNameFlags Flags
		{
			get
			{
				return _Flags & (AssemblyNameFlags)(-241);
			}
			set
			{
				_Flags &= (AssemblyNameFlags)240;
				_Flags |= value & (AssemblyNameFlags)(-241);
			}
		}

		public AssemblyHashAlgorithm HashAlgorithm
		{
			get
			{
				return _HashAlgorithm;
			}
			set
			{
				_HashAlgorithm = value;
			}
		}

		public AssemblyVersionCompatibility VersionCompatibility
		{
			get
			{
				return _VersionCompatibility;
			}
			set
			{
				_VersionCompatibility = value;
			}
		}

		public StrongNameKeyPair KeyPair
		{
			get
			{
				return _StrongNameKeyPair;
			}
			set
			{
				_StrongNameKeyPair = value;
			}
		}

		public string FullName => nToString();

		public AssemblyName()
		{
			_HashAlgorithm = AssemblyHashAlgorithm.None;
			_VersionCompatibility = AssemblyVersionCompatibility.SameMachine;
			_Flags = AssemblyNameFlags.None;
		}

		public object Clone()
		{
			AssemblyName assemblyName = new AssemblyName();
			assemblyName.Init(_Name, _PublicKey, _PublicKeyToken, _Version, _CultureInfo, _HashAlgorithm, _VersionCompatibility, _CodeBase, _Flags, _StrongNameKeyPair);
			assemblyName._HashForControl = _HashForControl;
			assemblyName._HashAlgorithmForControl = _HashAlgorithmForControl;
			return assemblyName;
		}

		public static AssemblyName GetAssemblyName(string assemblyFile)
		{
			if (assemblyFile == null)
			{
				throw new ArgumentNullException("assemblyFile");
			}
			string fullPathInternal = Path.GetFullPathInternal(assemblyFile);
			new FileIOPermission(FileIOPermissionAccess.PathDiscovery, fullPathInternal).Demand();
			return nGetFileInformation(fullPathInternal);
		}

		internal void SetHashControl(byte[] hash, AssemblyHashAlgorithm hashAlgorithm)
		{
			_HashForControl = hash;
			_HashAlgorithmForControl = hashAlgorithm;
		}

		public byte[] GetPublicKey()
		{
			return _PublicKey;
		}

		public void SetPublicKey(byte[] publicKey)
		{
			_PublicKey = publicKey;
			if (publicKey == null)
			{
				_Flags ^= AssemblyNameFlags.PublicKey;
			}
			else
			{
				_Flags |= AssemblyNameFlags.PublicKey;
			}
		}

		public byte[] GetPublicKeyToken()
		{
			if (_PublicKeyToken == null)
			{
				_PublicKeyToken = nGetPublicKeyToken();
			}
			return _PublicKeyToken;
		}

		public void SetPublicKeyToken(byte[] publicKeyToken)
		{
			_PublicKeyToken = publicKeyToken;
		}

		public override string ToString()
		{
			string fullName = FullName;
			if (fullName == null)
			{
				return base.ToString();
			}
			return fullName;
		}

		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			info.AddValue("_Name", _Name);
			info.AddValue("_PublicKey", _PublicKey, typeof(byte[]));
			info.AddValue("_PublicKeyToken", _PublicKeyToken, typeof(byte[]));
			info.AddValue("_CultureInfo", (_CultureInfo == null) ? (-1) : _CultureInfo.LCID);
			info.AddValue("_CodeBase", _CodeBase);
			info.AddValue("_Version", _Version);
			info.AddValue("_HashAlgorithm", _HashAlgorithm, typeof(AssemblyHashAlgorithm));
			info.AddValue("_HashAlgorithmForControl", _HashAlgorithmForControl, typeof(AssemblyHashAlgorithm));
			info.AddValue("_StrongNameKeyPair", _StrongNameKeyPair, typeof(StrongNameKeyPair));
			info.AddValue("_VersionCompatibility", _VersionCompatibility, typeof(AssemblyVersionCompatibility));
			info.AddValue("_Flags", _Flags, typeof(AssemblyNameFlags));
			info.AddValue("_HashForControl", _HashForControl, typeof(byte[]));
		}

		public void OnDeserialization(object sender)
		{
			if (m_siInfo != null)
			{
				_Name = m_siInfo.GetString("_Name");
				_PublicKey = (byte[])m_siInfo.GetValue("_PublicKey", typeof(byte[]));
				_PublicKeyToken = (byte[])m_siInfo.GetValue("_PublicKeyToken", typeof(byte[]));
				int @int = m_siInfo.GetInt32("_CultureInfo");
				if (@int != -1)
				{
					_CultureInfo = new CultureInfo(@int);
				}
				_CodeBase = m_siInfo.GetString("_CodeBase");
				_Version = (Version)m_siInfo.GetValue("_Version", typeof(Version));
				_HashAlgorithm = (AssemblyHashAlgorithm)m_siInfo.GetValue("_HashAlgorithm", typeof(AssemblyHashAlgorithm));
				_StrongNameKeyPair = (StrongNameKeyPair)m_siInfo.GetValue("_StrongNameKeyPair", typeof(StrongNameKeyPair));
				_VersionCompatibility = (AssemblyVersionCompatibility)m_siInfo.GetValue("_VersionCompatibility", typeof(AssemblyVersionCompatibility));
				_Flags = (AssemblyNameFlags)m_siInfo.GetValue("_Flags", typeof(AssemblyNameFlags));
				try
				{
					_HashAlgorithmForControl = (AssemblyHashAlgorithm)m_siInfo.GetValue("_HashAlgorithmForControl", typeof(AssemblyHashAlgorithm));
					_HashForControl = (byte[])m_siInfo.GetValue("_HashForControl", typeof(byte[]));
				}
				catch (SerializationException)
				{
					_HashAlgorithmForControl = AssemblyHashAlgorithm.None;
					_HashForControl = null;
				}
				m_siInfo = null;
			}
		}

		public AssemblyName(string assemblyName)
		{
			if (assemblyName == null)
			{
				throw new ArgumentNullException("assemblyName");
			}
			if (assemblyName.Length == 0 || assemblyName[0] == '\0')
			{
				throw new ArgumentException(Environment.GetResourceString("Format_StringZeroLength"));
			}
			_Name = assemblyName;
			nInit();
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		public static extern bool ReferenceMatchesDefinition(AssemblyName reference, AssemblyName definition);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal extern int nInit(out Assembly assembly, bool forIntrospection, bool raiseResolveEvent);

		internal void nInit()
		{
			Assembly assembly = null;
			nInit(out assembly, forIntrospection: false, raiseResolveEvent: false);
		}

		internal AssemblyName(SerializationInfo info, StreamingContext context)
		{
			m_siInfo = info;
		}

		internal void Init(string name, byte[] publicKey, byte[] publicKeyToken, Version version, CultureInfo cultureInfo, AssemblyHashAlgorithm hashAlgorithm, AssemblyVersionCompatibility versionCompatibility, string codeBase, AssemblyNameFlags flags, StrongNameKeyPair keyPair)
		{
			_Name = name;
			if (publicKey != null)
			{
				_PublicKey = new byte[publicKey.Length];
				Array.Copy(publicKey, _PublicKey, publicKey.Length);
			}
			if (publicKeyToken != null)
			{
				_PublicKeyToken = new byte[publicKeyToken.Length];
				Array.Copy(publicKeyToken, _PublicKeyToken, publicKeyToken.Length);
			}
			if (version != null)
			{
				_Version = (Version)version.Clone();
			}
			_CultureInfo = cultureInfo;
			_HashAlgorithm = hashAlgorithm;
			_VersionCompatibility = versionCompatibility;
			_CodeBase = codeBase;
			_Flags = flags;
			_StrongNameKeyPair = keyPair;
		}

		void _AssemblyName.GetTypeInfoCount(out uint pcTInfo)
		{
			throw new NotImplementedException();
		}

		void _AssemblyName.GetTypeInfo(uint iTInfo, uint lcid, IntPtr ppTInfo)
		{
			throw new NotImplementedException();
		}

		void _AssemblyName.GetIDsOfNames([In] ref Guid riid, IntPtr rgszNames, uint cNames, uint lcid, IntPtr rgDispId)
		{
			throw new NotImplementedException();
		}

		void _AssemblyName.Invoke(uint dispIdMember, [In] ref Guid riid, uint lcid, short wFlags, IntPtr pDispParams, IntPtr pVarResult, IntPtr pExcepInfo, IntPtr puArgErr)
		{
			throw new NotImplementedException();
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern AssemblyName nGetFileInformation(string s);

		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern string nToString();

		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern byte[] nGetPublicKeyToken();

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern string EscapeCodeBase(string codeBase);
	}
}
