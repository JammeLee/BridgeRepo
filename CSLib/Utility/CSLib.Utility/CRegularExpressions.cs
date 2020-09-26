using System;
using System.Text.RegularExpressions;

namespace CSLib.Utility
{
	[Serializable]
	public class CRegularExpressions
	{
		private string m_RegularName = "";

		private string m_RegularText = "";

		private Regex m_Regular;

		public string RegularName
		{
			get
			{
				return m_RegularName;
			}
			set
			{
				m_RegularText = value;
			}
		}

		public string RegularText
		{
			get
			{
				return m_RegularText;
			}
			set
			{
				m_RegularText = value;
			}
		}

		public CRegularExpressions()
		{
		}

		public CRegularExpressions(string regularExpression)
		{
			m_RegularText = regularExpression;
		}

		public void Generate(RegexOptions options)
		{
			m_Regular = new Regex(m_RegularText, options);
		}

		public MatchCollection GetMatchs(string text)
		{
			return m_Regular.Matches(text);
		}

		public bool IsMatch(string text)
		{
			return m_Regular.IsMatch(text);
		}

		public string Replace(string oringal, string replace)
		{
			return m_Regular.Replace(oringal, replace, 1);
		}
	}
}
