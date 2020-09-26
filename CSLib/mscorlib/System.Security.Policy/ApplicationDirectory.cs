using System.Runtime.InteropServices;
using System.Security.Util;

namespace System.Security.Policy
{
	[Serializable]
	[ComVisible(true)]
	public sealed class ApplicationDirectory : IBuiltInEvidence
	{
		private URLString m_appDirectory;

		public string Directory => m_appDirectory.ToString();

		internal ApplicationDirectory()
		{
			m_appDirectory = null;
		}

		public ApplicationDirectory(string name)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			m_appDirectory = new URLString(name);
		}

		public override bool Equals(object o)
		{
			if (o == null)
			{
				return false;
			}
			if (o is ApplicationDirectory)
			{
				ApplicationDirectory applicationDirectory = (ApplicationDirectory)o;
				if (m_appDirectory == null)
				{
					return applicationDirectory.m_appDirectory == null;
				}
				if (applicationDirectory.m_appDirectory == null)
				{
					return false;
				}
				if (m_appDirectory.IsSubsetOf(applicationDirectory.m_appDirectory))
				{
					return applicationDirectory.m_appDirectory.IsSubsetOf(m_appDirectory);
				}
				return false;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return Directory.GetHashCode();
		}

		public object Copy()
		{
			ApplicationDirectory applicationDirectory = new ApplicationDirectory();
			applicationDirectory.m_appDirectory = m_appDirectory;
			return applicationDirectory;
		}

		internal SecurityElement ToXml()
		{
			SecurityElement securityElement = new SecurityElement("System.Security.Policy.ApplicationDirectory");
			securityElement.AddAttribute("version", "1");
			if (m_appDirectory != null)
			{
				securityElement.AddChild(new SecurityElement("Directory", m_appDirectory.ToString()));
			}
			return securityElement;
		}

		int IBuiltInEvidence.OutputToBuffer(char[] buffer, int position, bool verbose)
		{
			buffer[position++] = '\0';
			string directory = Directory;
			int length = directory.Length;
			if (verbose)
			{
				BuiltInEvidenceHelper.CopyIntToCharArray(length, buffer, position);
				position += 2;
			}
			directory.CopyTo(0, buffer, position, length);
			return length + position;
		}

		int IBuiltInEvidence.InitFromBuffer(char[] buffer, int position)
		{
			int intFromCharArray = BuiltInEvidenceHelper.GetIntFromCharArray(buffer, position);
			position += 2;
			m_appDirectory = new URLString(new string(buffer, position, intFromCharArray));
			return position + intFromCharArray;
		}

		int IBuiltInEvidence.GetRequiredSize(bool verbose)
		{
			if (verbose)
			{
				return Directory.Length + 3;
			}
			return Directory.Length + 1;
		}

		public override string ToString()
		{
			return ToXml().ToString();
		}
	}
}
