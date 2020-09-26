using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Permissions;
using System.Text;

namespace System.Runtime.InteropServices
{
	[ComVisible(true)]
	public class RuntimeEnvironment
	{
		public static string SystemConfigurationFile
		{
			get
			{
				StringBuilder stringBuilder = new StringBuilder(260);
				stringBuilder.Append(GetRuntimeDirectory());
				stringBuilder.Append(AppDomainSetup.RuntimeConfigurationFile);
				string text = stringBuilder.ToString();
				new FileIOPermission(FileIOPermissionAccess.PathDiscovery, text).Demand();
				return text;
			}
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern string GetModuleFileName();

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern string GetDeveloperPath();

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern string GetHostBindingFile();

		[DllImport("mscoree.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
		private static extern int GetCORVersion(StringBuilder sb, int BufferLength, ref int retLength);

		[MethodImpl(MethodImplOptions.InternalCall)]
		public static extern bool FromGlobalAccessCache(Assembly a);

		public static string GetSystemVersion()
		{
			StringBuilder stringBuilder = new StringBuilder(256);
			int retLength = 0;
			if (GetCORVersion(stringBuilder, 256, ref retLength) == 0)
			{
				return stringBuilder.ToString();
			}
			return null;
		}

		public static string GetRuntimeDirectory()
		{
			string runtimeDirectoryImpl = GetRuntimeDirectoryImpl();
			new FileIOPermission(FileIOPermissionAccess.PathDiscovery, runtimeDirectoryImpl).Demand();
			return runtimeDirectoryImpl;
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern string GetRuntimeDirectoryImpl();
	}
}
