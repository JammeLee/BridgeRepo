using System.Xml;

namespace CSLib.Utility
{
	public class CXmlHelper : CSingleton<CXmlHelper>
	{
		private XmlDocument ᜀ;

		public XmlDocument XmlDoc
		{
			set
			{
				ᜀ = value;
			}
		}

		public XmlElement AddNode(ref XmlElement parentElement, string nodeName)
		{
			//Discarded unreachable code: IL_003f
			int num = 0;
			XmlElement xmlElement = default(XmlElement);
			while (true)
			{
				switch (num)
				{
				default:
					if (ᜀ == null)
					{
						num = 3;
						continue;
					}
					xmlElement = ᜀ.CreateElement(nodeName);
					num = 5;
					continue;
				case 3:
					if (true)
					{
					}
					return null;
				case 1:
					num = 4;
					continue;
				case 4:
					if (xmlElement != null)
					{
						num = 6;
						continue;
					}
					break;
				case 5:
					if (parentElement != null)
					{
						num = 1;
						continue;
					}
					break;
				case 6:
					parentElement.AppendChild(xmlElement);
					num = 2;
					continue;
				case 2:
					break;
				}
				break;
			}
			return xmlElement;
		}

		public XmlElement AddNode(ref XmlDocument xmlDoc, ref XmlElement parentElement, string nodeName)
		{
			//Discarded unreachable code: IL_003b
			int num = 4;
			XmlElement xmlElement = default(XmlElement);
			while (true)
			{
				switch (num)
				{
				default:
					if (xmlDoc == null)
					{
						num = 1;
						continue;
					}
					xmlElement = xmlDoc.CreateElement(nodeName);
					num = 6;
					continue;
				case 1:
					if (true)
					{
					}
					return null;
				case 3:
					num = 0;
					continue;
				case 0:
					if (xmlElement != null)
					{
						num = 5;
						continue;
					}
					break;
				case 6:
					if (parentElement != null)
					{
						num = 3;
						continue;
					}
					break;
				case 5:
					parentElement.AppendChild(xmlElement);
					num = 2;
					continue;
				case 2:
					break;
				}
				break;
			}
			return xmlElement;
		}

		public void SetNodeAttribute(ref XmlElement nodeElement, string attributeName, string attributeValue)
		{
			if (nodeElement != null)
			{
				nodeElement.SetAttribute(attributeName, attributeValue);
			}
		}

		public void RemoveNodeAttribute(ref XmlElement nodeElement, string attributeName)
		{
			if (nodeElement != null)
			{
				nodeElement.RemoveAttribute(attributeName);
			}
		}

		public string GetNodeAttribute(ref XmlElement nodeElement, string attributeName, string defaultValue)
		{
			//Discarded unreachable code: IL_0058
			int num = 1;
			while (true)
			{
				switch (num)
				{
				default:
					if (nodeElement != null)
					{
						num = 0;
						break;
					}
					return defaultValue;
				case 3:
					return defaultValue;
				case 0:
					num = 2;
					break;
				case 2:
					if (nodeElement.HasAttribute(attributeName))
					{
						return nodeElement.Attributes[attributeName].Value;
					}
					if (true)
					{
					}
					num = 3;
					break;
				}
			}
		}

		public string GetNodeAttribute(ref XmlNode node, string attributeName, string defaultValue)
		{
			//Discarded unreachable code: IL_005d
			while (true)
			{
				XmlElement xmlElement = node as XmlElement;
				int num = 3;
				while (true)
				{
					switch (num)
					{
					case 3:
						if (xmlElement != null)
						{
							num = 0;
							continue;
						}
						return defaultValue;
					case 2:
						return defaultValue;
					case 0:
						num = 1;
						continue;
					case 1:
						if (xmlElement.HasAttribute(attributeName))
						{
							return xmlElement.Attributes[attributeName].Value;
						}
						if (true)
						{
						}
						num = 2;
						continue;
					}
					break;
				}
			}
		}

		public string GetNodeAttribute(XmlElement nodeElement, string attributeName, string defaultValue)
		{
			//Discarded unreachable code: IL_0044
			int num = 0;
			while (true)
			{
				switch (num)
				{
				default:
					if (nodeElement != null)
					{
						num = 3;
						break;
					}
					return defaultValue;
				case 1:
					return defaultValue;
				case 3:
					if (true)
					{
					}
					num = 2;
					break;
				case 2:
					if (nodeElement.HasAttribute(attributeName))
					{
						return nodeElement.Attributes[attributeName].Value;
					}
					num = 1;
					break;
				}
			}
		}

		public string GetNodeAttribute(XmlNode node, string attributeName, string defaultValue)
		{
			//Discarded unreachable code: IL_0022
			while (true)
			{
				XmlElement xmlElement = node as XmlElement;
				if (true)
				{
				}
				int num = 2;
				while (true)
				{
					switch (num)
					{
					case 2:
						if (xmlElement != null)
						{
							num = 1;
							continue;
						}
						return defaultValue;
					case 0:
						return defaultValue;
					case 1:
						num = 3;
						continue;
					case 3:
						if (xmlElement.HasAttribute(attributeName))
						{
							return xmlElement.Attributes[attributeName].Value;
						}
						num = 0;
						continue;
					}
					break;
				}
			}
		}

		public void AppendChildNode(ref XmlElement parentElement, ref XmlElement childElement)
		{
			//Discarded unreachable code: IL_0027
			int num = 3;
			while (true)
			{
				switch (num)
				{
				case 2:
					parentElement.AppendChild(childElement);
					num = 4;
					continue;
				case 4:
					return;
				case 0:
					num = 1;
					continue;
				case 1:
					if (childElement != null)
					{
						num = 2;
						continue;
					}
					return;
				}
				if (true)
				{
				}
				if (parentElement != null)
				{
					num = 0;
					continue;
				}
				return;
			}
		}

		public XmlElement GetElementById(ref XmlDocument xmlDoc, string id)
		{
			//Discarded unreachable code: IL_002d
			while (true)
			{
				XmlElement result = null;
				int num = 2;
				while (true)
				{
					switch (num)
					{
					case 2:
						if (xmlDoc != null)
						{
							num = 0;
							continue;
						}
						goto case 1;
					case 0:
						if (true)
						{
						}
						result = xmlDoc.GetElementById(id);
						num = 1;
						continue;
					case 1:
						return result;
					}
					break;
				}
			}
		}
	}
}
