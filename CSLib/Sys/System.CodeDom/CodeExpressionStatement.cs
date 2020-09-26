using System.Runtime.InteropServices;

namespace System.CodeDom
{
	[Serializable]
	[ComVisible(true)]
	[ClassInterface(ClassInterfaceType.AutoDispatch)]
	public class CodeExpressionStatement : CodeStatement
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

		public CodeExpressionStatement()
		{
		}

		public CodeExpressionStatement(CodeExpression expression)
		{
			this.expression = expression;
		}
	}
}
