using System.Runtime.InteropServices;

namespace System.CodeDom
{
	[Serializable]
	[ComVisible(true)]
	[ClassInterface(ClassInterfaceType.AutoDispatch)]
	public class CodeMethodReturnStatement : CodeStatement
	{
		private CodeExpression expression;

		public CodeExpression Expression
		{
			get
			{
				return expression;
			}
			set
			{
				expression = value;
			}
		}

		public CodeMethodReturnStatement()
		{
		}

		public CodeMethodReturnStatement(CodeExpression expression)
		{
			Expression = expression;
		}
	}
}
