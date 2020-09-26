using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

namespace System.Runtime.Remoting
{
	[ComVisible(true)]
	public class WellKnownClientTypeEntry : TypeEntry
	{
		private string _objectUrl;

		private string _appUrl;

		public string ObjectUrl => _objectUrl;

		public Type ObjectType
		{
			[MethodImpl(MethodImplOptions.NoInlining)]
			get
			{
				StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
				return RuntimeType.PrivateGetType(base.TypeName + ", " + base.AssemblyName, throwOnError: false, ignoreCase: false, ref stackMark);
			}
		}

		public string ApplicationUrl
		{
			get
			{
				return _appUrl;
			}
			set
			{
				_appUrl = value;
			}
		}

		public WellKnownClientTypeEntry(string typeName, string assemblyName, string objectUrl)
		{
			if (typeName == null)
			{
				throw new ArgumentNullException("typeName");
			}
			if (assemblyName == null)
			{
				throw new ArgumentNullException("assemblyName");
			}
			if (objectUrl == null)
			{
				throw new ArgumentNullException("objectUrl");
			}
			base.TypeName = typeName;
			base.AssemblyName = assemblyName;
			_objectUrl = objectUrl;
		}

		public WellKnownClientTypeEntry(Type type, string objectUrl)
		{
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}
			if (objectUrl == null)
			{
				throw new ArgumentNullException("objectUrl");
			}
			base.TypeName = type.FullName;
			base.AssemblyName = type.Module.Assembly.nGetSimpleName();
			_objectUrl = objectUrl;
		}

		public override string ToString()
		{
			string text = "type='" + base.TypeName + ", " + base.AssemblyName + "'; url=" + _objectUrl;
			if (_appUrl != null)
			{
				text = text + "; appUrl=" + _appUrl;
			}
			return text;
		}
	}
}
