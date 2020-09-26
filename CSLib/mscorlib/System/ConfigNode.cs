using System.Collections;
using System.Diagnostics;

namespace System
{
	internal class ConfigNode
	{
		private string m_name;

		private string m_value;

		private ConfigNode m_parent;

		private ArrayList m_children = new ArrayList(5);

		private ArrayList m_attributes = new ArrayList(5);

		internal string Name => m_name;

		internal string Value
		{
			get
			{
				return m_value;
			}
			set
			{
				m_value = value;
			}
		}

		internal ConfigNode Parent => m_parent;

		internal ArrayList Children => m_children;

		internal ArrayList Attributes => m_attributes;

		internal ConfigNode(string name, ConfigNode parent)
		{
			m_name = name;
			m_parent = parent;
		}

		internal void AddChild(ConfigNode child)
		{
			child.m_parent = this;
			m_children.Add(child);
		}

		internal int AddAttribute(string key, string value)
		{
			m_attributes.Add(new DictionaryEntry(key, value));
			return m_attributes.Count - 1;
		}

		internal void ReplaceAttribute(int index, string key, string value)
		{
			m_attributes[index] = new DictionaryEntry(key, value);
		}

		[Conditional("_LOGGING")]
		internal void Trace()
		{
			_ = m_value;
			_ = m_parent;
			for (int i = 0; i < m_attributes.Count; i++)
			{
				_ = (DictionaryEntry)m_attributes[i];
			}
			for (int j = 0; j < m_children.Count; j++)
			{
			}
		}
	}
}
