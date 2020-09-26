using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Contexts;
using System.Threading;

namespace System.Runtime.Remoting
{
	[ComVisible(true)]
	public class ActivatedServiceTypeEntry : TypeEntry
	{
		private IContextAttribute[] _contextAttributes;

		public Type ObjectType
		{
			[MethodImpl(MethodImplOptions.NoInlining)]
			get
			{
				StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
				return RuntimeType.PrivateGetType(base.TypeName + ", " + base.AssemblyName, throwOnError: false, ignoreCase: false, ref stackMark);
			}
		}

		public IContextAttribute[] ContextAttributes
		{
			get
			{
				return _contextAttributes;
			}
			set
			{
				_contextAttributes = value;
			}
		}

		public ActivatedServiceTypeEntry(string typeName, string assemblyName)
		{
			if (typeName == null)
			{
				throw new ArgumentNullException("typeName");
			}
			if (assemblyName == null)
			{
				throw new ArgumentNullException("assemblyName");
			}
			base.TypeName = typeName;
			base.AssemblyName = assemblyName;
		}

		public ActivatedServiceTypeEntry(Type type)
		{
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}
			base.TypeName = type.FullName;
			base.AssemblyName = type.Module.Assembly.nGetSimpleName();
		}

		public override string ToString()
		{
			return "type='" + base.TypeName + ", " + base.AssemblyName + "'";
		}
	}
}
