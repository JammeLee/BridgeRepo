using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Permissions;
using System.Text;
using Microsoft.Win32;

namespace System.ComponentModel
{
	[Serializable]
	[SuppressUnmanagedCodeSecurity]
	[HostProtection(SecurityAction.LinkDemand, SharedState = true)]
	public class Win32Exception : ExternalException, ISerializable
	{
		private readonly int nativeErrorCode;

		public int NativeErrorCode => nativeErrorCode;

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public Win32Exception()
			: this(Marshal.GetLastWin32Error())
		{
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public Win32Exception(int error)
			: this(error, GetErrorMessage(error))
		{
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public Win32Exception(int error, string message)
			: base(message)
		{
			nativeErrorCode = error;
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public Win32Exception(string message)
			: this(Marshal.GetLastWin32Error(), message)
		{
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public Win32Exception(string message, Exception innerException)
			: base(message, innerException)
		{
			nativeErrorCode = Marshal.GetLastWin32Error();
		}

		protected Win32Exception(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
			IntSecurity.UnmanagedCode.Demand();
			nativeErrorCode = info.GetInt32("NativeErrorCode");
		}

		private static string GetErrorMessage(int error)
		{
			string text = "";
			StringBuilder stringBuilder = new StringBuilder(256);
			if (Microsoft.Win32.SafeNativeMethods.FormatMessage(12800, NativeMethods.NullHandleRef, error, 0, stringBuilder, stringBuilder.Capacity + 1, IntPtr.Zero) != 0)
			{
				int num;
				for (num = stringBuilder.Length; num > 0; num--)
				{
					char c = stringBuilder[num - 1];
					if (c > ' ' && c != '.')
					{
						break;
					}
				}
				return stringBuilder.ToString(0, num);
			}
			return "Unknown error (0x" + Convert.ToString(error, 16) + ")";
		}

		[SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			info.AddValue("NativeErrorCode", nativeErrorCode);
			base.GetObjectData(info, context);
		}
	}
}
