using System.Configuration.Internal;
using System.Xml;

namespace System.Configuration
{
	internal sealed class ConfigXmlCDataSection : XmlCDataSection, IConfigErrorInfo
	{
		private int _line;

		private string _filename;

		int IConfigErrorInfo.LineNumber => _line;

		string IConfigErrorInfo.Filename => _filename;

		public ConfigXmlCDataSection(string filename, int line, string data, XmlDocument doc)
			: base(data, doc)
		{
			_line = line;
			_filename = filename;
		}

		public override XmlNode CloneNode(bool deep)
		{
			XmlNode xmlNode = base.CloneNode(deep);
			ConfigXmlCDataSection configXmlCDataSection = xmlNode as ConfigXmlCDataSection;
			if (configXmlCDataSection != null)
			{
				configXmlCDataSection._line = _line;
				configXmlCDataSection._filename = _filename;
			}
			return xmlNode;
		}
	}
}
