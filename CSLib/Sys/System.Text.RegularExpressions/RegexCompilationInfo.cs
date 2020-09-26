namespace System.Text.RegularExpressions
{
	[Serializable]
	public class RegexCompilationInfo
	{
		private string pattern;

		private RegexOptions options;

		private string name;

		private string nspace;

		private bool isPublic;

		public string Pattern
		{
			get
			{
				return pattern;
			}
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException("value");
				}
				pattern = value;
			}
		}

		public RegexOptions Options
		{
			get
			{
				return options;
			}
			set
			{
				options = value;
			}
		}

		public string Name
		{
			get
			{
				return name;
			}
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException("value");
				}
				if (value.Length == 0)
				{
					throw new ArgumentException(SR.GetString("InvalidNullEmptyArgument", "value"), "value");
				}
				name = value;
			}
		}

		public string Namespace
		{
			get
			{
				return nspace;
			}
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException("value");
				}
				nspace = value;
			}
		}

		public bool IsPublic
		{
			get
			{
				return isPublic;
			}
			set
			{
				isPublic = value;
			}
		}

		public RegexCompilationInfo(string pattern, RegexOptions options, string name, string fullnamespace, bool ispublic)
		{
			Pattern = pattern;
			Name = name;
			Namespace = fullnamespace;
			this.options = options;
			isPublic = ispublic;
		}
	}
}
