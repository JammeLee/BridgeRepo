using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security.Util;

namespace System.Security.Policy
{
	[Serializable]
	[ComVisible(true)]
	public sealed class PermissionRequestEvidence : IBuiltInEvidence
	{
		private const char idRequest = '\0';

		private const char idOptional = '\u0001';

		private const char idDenied = '\u0002';

		private PermissionSet m_request;

		private PermissionSet m_optional;

		private PermissionSet m_denied;

		private string m_strRequest;

		private string m_strOptional;

		private string m_strDenied;

		public PermissionSet RequestedPermissions => m_request;

		public PermissionSet OptionalPermissions => m_optional;

		public PermissionSet DeniedPermissions => m_denied;

		public PermissionRequestEvidence(PermissionSet request, PermissionSet optional, PermissionSet denied)
		{
			if (request == null)
			{
				m_request = null;
			}
			else
			{
				m_request = request.Copy();
			}
			if (optional == null)
			{
				m_optional = null;
			}
			else
			{
				m_optional = optional.Copy();
			}
			if (denied == null)
			{
				m_denied = null;
			}
			else
			{
				m_denied = denied.Copy();
			}
		}

		internal PermissionRequestEvidence()
		{
		}

		public PermissionRequestEvidence Copy()
		{
			return new PermissionRequestEvidence(m_request, m_optional, m_denied);
		}

		internal SecurityElement ToXml()
		{
			SecurityElement securityElement = new SecurityElement("System.Security.Policy.PermissionRequestEvidence");
			securityElement.AddAttribute("version", "1");
			if (m_request != null)
			{
				SecurityElement securityElement2 = new SecurityElement("Request");
				securityElement2.AddChild(m_request.ToXml());
				securityElement.AddChild(securityElement2);
			}
			if (m_optional != null)
			{
				SecurityElement securityElement2 = new SecurityElement("Optional");
				securityElement2.AddChild(m_optional.ToXml());
				securityElement.AddChild(securityElement2);
			}
			if (m_denied != null)
			{
				SecurityElement securityElement2 = new SecurityElement("Denied");
				securityElement2.AddChild(m_denied.ToXml());
				securityElement.AddChild(securityElement2);
			}
			return securityElement;
		}

		internal void CreateStrings()
		{
			if (m_strRequest == null && m_request != null)
			{
				m_strRequest = m_request.ToXml().ToString();
			}
			if (m_strOptional == null && m_optional != null)
			{
				m_strOptional = m_optional.ToXml().ToString();
			}
			if (m_strDenied == null && m_denied != null)
			{
				m_strDenied = m_denied.ToXml().ToString();
			}
		}

		int IBuiltInEvidence.OutputToBuffer(char[] buffer, int position, bool verbose)
		{
			CreateStrings();
			int num = position;
			int position2 = 0;
			int num2 = 0;
			buffer[num++] = '\a';
			if (verbose)
			{
				position2 = num;
				num += 2;
			}
			if (m_strRequest != null)
			{
				int length = m_strRequest.Length;
				if (verbose)
				{
					buffer[num++] = '\0';
					BuiltInEvidenceHelper.CopyIntToCharArray(length, buffer, num);
					num += 2;
					num2++;
				}
				m_strRequest.CopyTo(0, buffer, num, length);
				num += length;
			}
			if (m_strOptional != null)
			{
				int length = m_strOptional.Length;
				if (verbose)
				{
					buffer[num++] = '\u0001';
					BuiltInEvidenceHelper.CopyIntToCharArray(length, buffer, num);
					num += 2;
					num2++;
				}
				m_strOptional.CopyTo(0, buffer, num, length);
				num += length;
			}
			if (m_strDenied != null)
			{
				int length = m_strDenied.Length;
				if (verbose)
				{
					buffer[num++] = '\u0002';
					BuiltInEvidenceHelper.CopyIntToCharArray(length, buffer, num);
					num += 2;
					num2++;
				}
				m_strDenied.CopyTo(0, buffer, num, length);
				num += length;
			}
			if (verbose)
			{
				BuiltInEvidenceHelper.CopyIntToCharArray(num2, buffer, position2);
			}
			return num;
		}

		int IBuiltInEvidence.GetRequiredSize(bool verbose)
		{
			CreateStrings();
			int num = 1;
			if (m_strRequest != null)
			{
				if (verbose)
				{
					num += 3;
				}
				num += m_strRequest.Length;
			}
			if (m_strOptional != null)
			{
				if (verbose)
				{
					num += 3;
				}
				num += m_strOptional.Length;
			}
			if (m_strDenied != null)
			{
				if (verbose)
				{
					num += 3;
				}
				num += m_strDenied.Length;
			}
			if (verbose)
			{
				num += 2;
			}
			return num;
		}

		int IBuiltInEvidence.InitFromBuffer(char[] buffer, int position)
		{
			int intFromCharArray = BuiltInEvidenceHelper.GetIntFromCharArray(buffer, position);
			position += 2;
			for (int i = 0; i < intFromCharArray; i++)
			{
				char c = buffer[position++];
				int intFromCharArray2 = BuiltInEvidenceHelper.GetIntFromCharArray(buffer, position);
				position += 2;
				string text = new string(buffer, position, intFromCharArray2);
				position += intFromCharArray2;
				Parser parser = new Parser(text);
				PermissionSet permissionSet = new PermissionSet();
				permissionSet.FromXml(parser.GetTopElement());
				switch (c)
				{
				case '\0':
					m_strRequest = text;
					m_request = permissionSet;
					break;
				case '\u0001':
					m_strOptional = text;
					m_optional = permissionSet;
					break;
				case '\u0002':
					m_strDenied = text;
					m_denied = permissionSet;
					break;
				default:
					throw new SerializationException(Environment.GetResourceString("Serialization_UnableToFixup"));
				}
			}
			return position;
		}

		public override string ToString()
		{
			return ToXml().ToString();
		}
	}
}
