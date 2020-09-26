using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;

namespace System.Diagnostics
{
	[ComVisible(true)]
	public sealed class Debugger
	{
		public static readonly string DefaultCategory;

		public static bool IsAttached => IsDebuggerAttached();

		public static void Break()
		{
			if (!IsDebuggerAttached())
			{
				try
				{
					new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Demand();
				}
				catch (SecurityException)
				{
					return;
				}
			}
			BreakInternal();
		}

		private static void BreakCanThrow()
		{
			if (!IsDebuggerAttached())
			{
				new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Demand();
			}
			BreakInternal();
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void BreakInternal();

		public static bool Launch()
		{
			if (IsDebuggerAttached())
			{
				return true;
			}
			try
			{
				new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Demand();
			}
			catch (SecurityException)
			{
				return false;
			}
			return LaunchInternal();
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern bool LaunchInternal();

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern bool IsDebuggerAttached();

		[MethodImpl(MethodImplOptions.InternalCall)]
		public static extern void Log(int level, string category, string message);

		[MethodImpl(MethodImplOptions.InternalCall)]
		public static extern bool IsLogging();
	}
}
