using System.IO;
using System.Security.Permissions;

namespace System.CodeDom.Compiler
{
	[PermissionSet(SecurityAction.InheritanceDemand, Name = "FullTrust")]
	[PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
	public abstract class CodeParser : ICodeParser
	{
		public abstract CodeCompileUnit Parse(TextReader codeStream);
	}
}
