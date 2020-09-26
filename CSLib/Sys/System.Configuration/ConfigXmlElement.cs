using System.Configuration.Internal;
using System.Xml;

namespace System.Configuration
{
	internal sealed class ConfigXmlElement : XmlElement, IConfigErrorInfo
	{
		private int _line;

		private string _filename;

		int IConfigErrorInfo.LineNumber => _line;

		string IConfigErrorInfo.Filename => _filename;

		public ConfigXmlElement(string filename, int line, string prefix, string localName, string namespaceUri, XmlDocument doc)
			: base(prefix, localName, namespaceUri, doc)
		{
			_line = line;
			_filename = filename;
		}

		public override XmlNode CloneNode(bool deep)
		{
			XmlNode xmlNode = base.CloneNode(deep);
			ConfigXmlElement configXmlElement = xmlNode as ConfigXmlElement;
			if (configXmlElement != null)
			{
				configXmlElement._line = _line;
				configXmlElement._filename = _filename;
			}
			return xmlNode;
		}
	}
}
