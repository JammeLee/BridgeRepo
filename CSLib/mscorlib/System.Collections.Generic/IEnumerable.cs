using System.Runtime.CompilerServices;

namespace System.Collections.Generic
{
	[TypeDependency("System.SZArrayHelper")]
	public interface IEnumerable<T> : IEnumerable
	{
		new IEnumerator<T> GetEnumerator();
	}
}
