using System.Runtime.InteropServices;

namespace System.CodeDom
{
	[Serializable]
	[ComVisible(true)]
	[ClassInterface(ClassInterfaceType.AutoDispatch)]
	public class CodeSnippetStatement : CodeStatement
	{
		private string value;

		public string Value
		{
			get
			{
				if (value != null)
				{
					return value;
				}
				return string.Empty;
			}
			set
			{
				this.value = value;
			}
		}

		public CodeSnippetStatement()
		{
		}

		public CodeSnippetStatement(string value)
		{
			Value = value;
		}
	}
}
