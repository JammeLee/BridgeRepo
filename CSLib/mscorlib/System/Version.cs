using System.Globalization;
using System.Runtime.InteropServices;

namespace System
{
	[Serializable]
	[ComVisible(true)]
	public sealed class Version : ICloneable, IComparable, IComparable<Version>, IEquatable<Version>
	{
		private int _Major;

		private int _Minor;

		private int _Build = -1;

		private int _Revision = -1;

		public int Major => _Major;

		public int Minor => _Minor;

		public int Build => _Build;

		public int Revision => _Revision;

		public short MajorRevision => (short)(_Revision >> 16);

		public short MinorRevision => (short)(_Revision & 0xFFFF);

		public Version(int major, int minor, int build, int revision)
		{
			if (major < 0)
			{
				throw new ArgumentOutOfRangeException("major", Environment.GetResourceString("ArgumentOutOfRange_Version"));
			}
			if (minor < 0)
			{
				throw new ArgumentOutOfRangeException("minor", Environment.GetResourceString("ArgumentOutOfRange_Version"));
			}
			if (build < 0)
			{
				throw new ArgumentOutOfRangeException("build", Environment.GetResourceString("ArgumentOutOfRange_Version"));
			}
			if (revision < 0)
			{
				throw new ArgumentOutOfRangeException("revision", Environment.GetResourceString("ArgumentOutOfRange_Version"));
			}
			_Major = major;
			_Minor = minor;
			_Build = build;
			_Revision = revision;
		}

		public Version(int major, int minor, int build)
		{
			if (major < 0)
			{
				throw new ArgumentOutOfRangeException("major", Environment.GetResourceString("ArgumentOutOfRange_Version"));
			}
			if (minor < 0)
			{
				throw new ArgumentOutOfRangeException("minor", Environment.GetResourceString("ArgumentOutOfRange_Version"));
			}
			if (build < 0)
			{
				throw new ArgumentOutOfRangeException("build", Environment.GetResourceString("ArgumentOutOfRange_Version"));
			}
			_Major = major;
			_Minor = minor;
			_Build = build;
		}

		public Version(int major, int minor)
		{
			if (major < 0)
			{
				throw new ArgumentOutOfRangeException("major", Environment.GetResourceString("ArgumentOutOfRange_Version"));
			}
			if (minor < 0)
			{
				throw new ArgumentOutOfRangeException("minor", Environment.GetResourceString("ArgumentOutOfRange_Version"));
			}
			_Major = major;
			_Minor = minor;
		}

		public Version(string version)
		{
			if (version == null)
			{
				throw new ArgumentNullException("version");
			}
			string[] array = version.Split('.');
			int num = array.Length;
			if (num < 2 || num > 4)
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_VersionString"));
			}
			_Major = int.Parse(array[0], CultureInfo.InvariantCulture);
			if (_Major < 0)
			{
				throw new ArgumentOutOfRangeException("version", Environment.GetResourceString("ArgumentOutOfRange_Version"));
			}
			_Minor = int.Parse(array[1], CultureInfo.InvariantCulture);
			if (_Minor < 0)
			{
				throw new ArgumentOutOfRangeException("version", Environment.GetResourceString("ArgumentOutOfRange_Version"));
			}
			num -= 2;
			if (num <= 0)
			{
				return;
			}
			_Build = int.Parse(array[2], CultureInfo.InvariantCulture);
			if (_Build < 0)
			{
				throw new ArgumentOutOfRangeException("build", Environment.GetResourceString("ArgumentOutOfRange_Version"));
			}
			num--;
			if (num > 0)
			{
				_Revision = int.Parse(array[3], CultureInfo.InvariantCulture);
				if (_Revision < 0)
				{
					throw new ArgumentOutOfRangeException("revision", Environment.GetResourceString("ArgumentOutOfRange_Version"));
				}
			}
		}

		public Version()
		{
			_Major = 0;
			_Minor = 0;
		}

		public object Clone()
		{
			Version version = new Version();
			version._Major = _Major;
			version._Minor = _Minor;
			version._Build = _Build;
			version._Revision = _Revision;
			return version;
		}

		public int CompareTo(object version)
		{
			if (version == null)
			{
				return 1;
			}
			Version version2 = version as Version;
			if (version2 == null)
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_MustBeVersion"));
			}
			if (_Major != version2._Major)
			{
				if (_Major > version2._Major)
				{
					return 1;
				}
				return -1;
			}
			if (_Minor != version2._Minor)
			{
				if (_Minor > version2._Minor)
				{
					return 1;
				}
				return -1;
			}
			if (_Build != version2._Build)
			{
				if (_Build > version2._Build)
				{
					return 1;
				}
				return -1;
			}
			if (_Revision != version2._Revision)
			{
				if (_Revision > version2._Revision)
				{
					return 1;
				}
				return -1;
			}
			return 0;
		}

		public int CompareTo(Version value)
		{
			if (value == null)
			{
				return 1;
			}
			if (_Major != value._Major)
			{
				if (_Major > value._Major)
				{
					return 1;
				}
				return -1;
			}
			if (_Minor != value._Minor)
			{
				if (_Minor > value._Minor)
				{
					return 1;
				}
				return -1;
			}
			if (_Build != value._Build)
			{
				if (_Build > value._Build)
				{
					return 1;
				}
				return -1;
			}
			if (_Revision != value._Revision)
			{
				if (_Revision > value._Revision)
				{
					return 1;
				}
				return -1;
			}
			return 0;
		}

		public override bool Equals(object obj)
		{
			Version version = obj as Version;
			if (version == null)
			{
				return false;
			}
			if (_Major != version._Major || _Minor != version._Minor || _Build != version._Build || _Revision != version._Revision)
			{
				return false;
			}
			return true;
		}

		public bool Equals(Version obj)
		{
			if (obj == null)
			{
				return false;
			}
			if (_Major != obj._Major || _Minor != obj._Minor || _Build != obj._Build || _Revision != obj._Revision)
			{
				return false;
			}
			return true;
		}

		public override int GetHashCode()
		{
			int num = 0;
			num |= (_Major & 0xF) << 28;
			num |= (_Minor & 0xFF) << 20;
			num |= (_Build & 0xFF) << 12;
			return num | (_Revision & 0xFFF);
		}

		public override string ToString()
		{
			if (_Build == -1)
			{
				return ToString(2);
			}
			if (_Revision == -1)
			{
				return ToString(3);
			}
			return ToString(4);
		}

		public string ToString(int fieldCount)
		{
			switch (fieldCount)
			{
			case 0:
				return string.Empty;
			case 1:
				return string.Concat(_Major);
			case 2:
				return _Major + "." + _Minor;
			default:
				if (_Build == -1)
				{
					throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("ArgumentOutOfRange_Bounds_Lower_Upper"), "0", "2"), "fieldCount");
				}
				if (fieldCount == 3)
				{
					return _Major + "." + _Minor + "." + _Build;
				}
				if (_Revision == -1)
				{
					throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("ArgumentOutOfRange_Bounds_Lower_Upper"), "0", "3"), "fieldCount");
				}
				if (fieldCount == 4)
				{
					return Major + "." + _Minor + "." + _Build + "." + _Revision;
				}
				throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("ArgumentOutOfRange_Bounds_Lower_Upper"), "0", "4"), "fieldCount");
			}
		}

		public static bool operator ==(Version v1, Version v2)
		{
			if (object.ReferenceEquals(v1, null))
			{
				return object.ReferenceEquals(v2, null);
			}
			return v1.Equals(v2);
		}

		public static bool operator !=(Version v1, Version v2)
		{
			return !(v1 == v2);
		}

		public static bool operator <(Version v1, Version v2)
		{
			if ((object)v1 == null)
			{
				throw new ArgumentNullException("v1");
			}
			return v1.CompareTo(v2) < 0;
		}

		public static bool operator <=(Version v1, Version v2)
		{
			if ((object)v1 == null)
			{
				throw new ArgumentNullException("v1");
			}
			return v1.CompareTo(v2) <= 0;
		}

		public static bool operator >(Version v1, Version v2)
		{
			return v2 < v1;
		}

		public static bool operator >=(Version v1, Version v2)
		{
			return v2 <= v1;
		}
	}
}
