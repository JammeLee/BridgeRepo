using System.Globalization;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace System
{
	[Serializable]
	[ComVisible(true)]
	public sealed class OperatingSystem : ICloneable, ISerializable
	{
		private Version _version;

		private PlatformID _platform;

		private string _servicePack;

		private string _versionString;

		public PlatformID Platform => _platform;

		public string ServicePack
		{
			get
			{
				if (_servicePack == null)
				{
					return string.Empty;
				}
				return _servicePack;
			}
		}

		public Version Version => _version;

		public string VersionString
		{
			get
			{
				if (_versionString != null)
				{
					return _versionString;
				}
				string str = ((_platform == PlatformID.Win32NT) ? "Microsoft Windows NT " : ((_platform == PlatformID.Win32Windows) ? ((_version.Major <= 4 && (_version.Major != 4 || _version.Minor <= 0)) ? "Microsoft Windows 95 " : "Microsoft Windows 98 ") : ((_platform == PlatformID.Win32S) ? "Microsoft Win32S " : ((_platform != PlatformID.WinCE) ? "<unknown> " : "Microsoft Windows CE "))));
				if (string.IsNullOrEmpty(_servicePack))
				{
					_versionString = str + _version.ToString();
				}
				else
				{
					_versionString = str + _version.ToString(3) + " " + _servicePack;
				}
				return _versionString;
			}
		}

		private OperatingSystem()
		{
		}

		public OperatingSystem(PlatformID platform, Version version)
			: this(platform, version, null)
		{
		}

		internal OperatingSystem(PlatformID platform, Version version, string servicePack)
		{
			if (platform < PlatformID.Win32S || platform > PlatformID.Unix)
			{
				throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Arg_EnumIllegalVal"), (int)platform), "platform");
			}
			if ((object)version == null)
			{
				throw new ArgumentNullException("version");
			}
			_platform = platform;
			_version = (Version)version.Clone();
			_servicePack = servicePack;
		}

		private OperatingSystem(SerializationInfo info, StreamingContext context)
		{
			SerializationInfoEnumerator enumerator = info.GetEnumerator();
			while (enumerator.MoveNext())
			{
				switch (enumerator.Name)
				{
				case "_version":
					_version = (Version)info.GetValue("_version", typeof(Version));
					break;
				case "_platform":
					_platform = (PlatformID)info.GetValue("_platform", typeof(PlatformID));
					break;
				case "_servicePack":
					_servicePack = info.GetString("_servicePack");
					break;
				}
			}
			if (_version == null)
			{
				throw new SerializationException(Environment.GetResourceString("Serialization_MissField", "_version"));
			}
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			info.AddValue("_version", _version);
			info.AddValue("_platform", _platform);
			info.AddValue("_servicePack", _servicePack);
		}

		public object Clone()
		{
			return new OperatingSystem(_platform, _version, _servicePack);
		}

		public override string ToString()
		{
			return VersionString;
		}
	}
}
