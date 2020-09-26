using System.Reflection;

namespace System.Runtime.Serialization.Formatters.Binary
{
	internal sealed class SerObjectInfoCache
	{
		internal string fullTypeName;

		internal string assemblyString;

		internal MemberInfo[] memberInfos;

		internal string[] memberNames;

		internal Type[] memberTypes;
	}
}
