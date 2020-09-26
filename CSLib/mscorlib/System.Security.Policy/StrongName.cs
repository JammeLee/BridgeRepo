using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Security.Util;

namespace System.Security.Policy
{
	[Serializable]
	[ComVisible(true)]
	public sealed class StrongName : IIdentityPermissionFactory, IBuiltInEvidence, IDelayEvaluatedEvidence
	{
		private StrongNamePublicKeyBlob m_publicKeyBlob;

		private string m_name;

		private Version m_version;

		[NonSerialized]
		private Assembly m_assembly;

		[NonSerialized]
		private bool m_wasUsed;

		public StrongNamePublicKeyBlob PublicKey => m_publicKeyBlob;

		public string Name => m_name;

		public Version Version => m_version;

		bool IDelayEvaluatedEvidence.IsVerified
		{
			get
			{
				if (m_assembly == null)
				{
					return true;
				}
				return m_assembly.IsStrongNameVerified();
			}
		}

		bool IDelayEvaluatedEvidence.WasUsed => m_wasUsed;

		internal StrongName()
		{
		}

		public StrongName(StrongNamePublicKeyBlob blob, string name, Version version)
			: this(blob, name, version, null)
		{
		}

		internal StrongName(StrongNamePublicKeyBlob blob, string name, Version version, Assembly assembly)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			if (string.IsNullOrEmpty(name))
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_EmptyStrongName"));
			}
			if (blob == null)
			{
				throw new ArgumentNullException("blob");
			}
			if (version == null)
			{
				throw new ArgumentNullException("version");
			}
			m_publicKeyBlob = blob;
			m_name = name;
			m_version = version;
			m_assembly = assembly;
		}

		void IDelayEvaluatedEvidence.MarkUsed()
		{
			m_wasUsed = true;
		}

		internal static bool CompareNames(string asmName, string mcName)
		{
			if (mcName.Length > 0 && mcName[mcName.Length - 1] == '*' && mcName.Length - 1 <= asmName.Length)
			{
				return string.Compare(mcName, 0, asmName, 0, mcName.Length - 1, StringComparison.OrdinalIgnoreCase) == 0;
			}
			return string.Compare(mcName, asmName, StringComparison.OrdinalIgnoreCase) == 0;
		}

		public IPermission CreateIdentityPermission(Evidence evidence)
		{
			return new StrongNameIdentityPermission(m_publicKeyBlob, m_name, m_version);
		}

		public object Copy()
		{
			return new StrongName(m_publicKeyBlob, m_name, m_version);
		}

		internal SecurityElement ToXml()
		{
			SecurityElement securityElement = new SecurityElement("StrongName");
			securityElement.AddAttribute("version", "1");
			if (m_publicKeyBlob != null)
			{
				securityElement.AddAttribute("Key", Hex.EncodeHexString(m_publicKeyBlob.PublicKey));
			}
			if (m_name != null)
			{
				securityElement.AddAttribute("Name", m_name);
			}
			if (m_version != null)
			{
				securityElement.AddAttribute("Version", m_version.ToString());
			}
			return securityElement;
		}

		internal void FromXml(SecurityElement element)
		{
			if (element == null)
			{
				throw new ArgumentNullException("element");
			}
			if (string.Compare(element.Tag, "StrongName", StringComparison.Ordinal) != 0)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_InvalidXML"));
			}
			m_publicKeyBlob = null;
			m_version = null;
			string text = element.Attribute("Key");
			if (text != null)
			{
				m_publicKeyBlob = new StrongNamePublicKeyBlob(Hex.DecodeHexString(text));
			}
			m_name = element.Attribute("Name");
			string text2 = element.Attribute("Version");
			if (text2 != null)
			{
				m_version = new Version(text2);
			}
		}

		public override string ToString()
		{
			return ToXml().ToString();
		}

		public override bool Equals(object o)
		{
			StrongName strongName = o as StrongName;
			if (strongName != null && object.Equals(m_publicKeyBlob, strongName.m_publicKeyBlob) && object.Equals(m_name, strongName.m_name))
			{
				return object.Equals(m_version, strongName.m_version);
			}
			return false;
		}

		public override int GetHashCode()
		{
			if (m_publicKeyBlob != null)
			{
				return m_publicKeyBlob.GetHashCode();
			}
			if (m_name != null || m_version != null)
			{
				return ((m_name != null) ? m_name.GetHashCode() : 0) + ((!(m_version == null)) ? m_version.GetHashCode() : 0);
			}
			return typeof(StrongName).GetHashCode();
		}

		int IBuiltInEvidence.OutputToBuffer(char[] buffer, int position, bool verbose)
		{
			buffer[position++] = '\u0002';
			int num = m_publicKeyBlob.PublicKey.Length;
			if (verbose)
			{
				BuiltInEvidenceHelper.CopyIntToCharArray(num, buffer, position);
				position += 2;
			}
			Buffer.InternalBlockCopy(m_publicKeyBlob.PublicKey, 0, buffer, position * 2, num);
			position += (num - 1) / 2 + 1;
			BuiltInEvidenceHelper.CopyIntToCharArray(m_version.Major, buffer, position);
			BuiltInEvidenceHelper.CopyIntToCharArray(m_version.Minor, buffer, position + 2);
			BuiltInEvidenceHelper.CopyIntToCharArray(m_version.Build, buffer, position + 4);
			BuiltInEvidenceHelper.CopyIntToCharArray(m_version.Revision, buffer, position + 6);
			position += 8;
			int length = m_name.Length;
			if (verbose)
			{
				BuiltInEvidenceHelper.CopyIntToCharArray(length, buffer, position);
				position += 2;
			}
			m_name.CopyTo(0, buffer, position, length);
			return length + position;
		}

		int IBuiltInEvidence.GetRequiredSize(bool verbose)
		{
			int num = (m_publicKeyBlob.PublicKey.Length - 1) / 2 + 1;
			if (verbose)
			{
				num += 2;
			}
			num += 8;
			num += m_name.Length;
			if (verbose)
			{
				num += 2;
			}
			return num + 1;
		}

		int IBuiltInEvidence.InitFromBuffer(char[] buffer, int position)
		{
			int intFromCharArray = BuiltInEvidenceHelper.GetIntFromCharArray(buffer, position);
			position += 2;
			m_publicKeyBlob = new StrongNamePublicKeyBlob();
			m_publicKeyBlob.PublicKey = new byte[intFromCharArray];
			int num = (intFromCharArray - 1) / 2 + 1;
			Buffer.InternalBlockCopy(buffer, position * 2, m_publicKeyBlob.PublicKey, 0, intFromCharArray);
			position += num;
			int intFromCharArray2 = BuiltInEvidenceHelper.GetIntFromCharArray(buffer, position);
			int intFromCharArray3 = BuiltInEvidenceHelper.GetIntFromCharArray(buffer, position + 2);
			int intFromCharArray4 = BuiltInEvidenceHelper.GetIntFromCharArray(buffer, position + 4);
			int intFromCharArray5 = BuiltInEvidenceHelper.GetIntFromCharArray(buffer, position + 6);
			m_version = new Version(intFromCharArray2, intFromCharArray3, intFromCharArray4, intFromCharArray5);
			position += 8;
			intFromCharArray = BuiltInEvidenceHelper.GetIntFromCharArray(buffer, position);
			position += 2;
			m_name = new string(buffer, position, intFromCharArray);
			return position + intFromCharArray;
		}

		internal object Normalize()
		{
			MemoryStream memoryStream = new MemoryStream();
			BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
			binaryWriter.Write(m_publicKeyBlob.PublicKey);
			binaryWriter.Write(m_version.Major);
			binaryWriter.Write(m_name);
			memoryStream.Position = 0L;
			return memoryStream;
		}
	}
}
