using System.Runtime.InteropServices;

namespace System.CodeDom
{
	[Serializable]
	[ComVisible(true)]
	[ClassInterface(ClassInterfaceType.AutoDispatch)]
	public class CodeConstructor : CodeMemberMethod
	{
		private CodeExpressionCollection baseConstructorArgs = new CodeExpressionCollection();

		private CodeExpressionCollection chainedConstructorArgs = new CodeExpressionCollection();

		public CodeExpressionCollection BaseConstructorArgs => baseConstructorArgs;

		public CodeExpressionCollection ChainedConstructorArgs => chainedConstructorArgs;

		public CodeConstructor()
		{
			base.Name = ".ctor";
		}
	}
}
