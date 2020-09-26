using System.Runtime.InteropServices;

namespace System.CodeDom
{
	[Serializable]
	[ComVisible(true)]
	[ClassInterface(ClassInterfaceType.AutoDispatch)]
	public class CodeTypeConstructor : CodeMemberMethod
	{
		public CodeTypeConstructor()
		{
			base.Name = ".cctor";
		}
	}
}
