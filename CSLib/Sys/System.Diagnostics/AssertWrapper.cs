using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using Microsoft.Win32;

namespace System.Diagnostics
{
	internal static class AssertWrapper
	{
		private static bool IsRTLResources => SR.GetString("RTL") != "RTL_False";

		public static void ShowAssert(string stackTrace, StackFrame frame, string message, string detailMessage)
		{
			ShowMessageBoxAssert(stackTrace, message, detailMessage);
		}

		private static void ShowMessageBoxAssert(string stackTrace, string message, string detailMessage)
		{
			string message2 = message + "\r\n" + detailMessage + "\r\n" + stackTrace;
			message2 = TruncateMessageToFitScreen(message2);
			int num = 262674;
			if (!Environment.UserInteractive)
			{
				num |= 0x200000;
			}
			if (IsRTLResources)
			{
				num = num | 0x80000 | 0x100000;
			}
			switch (Microsoft.Win32.SafeNativeMethods.MessageBox(NativeMethods.NullHandleRef, message2, SR.GetString("DebugAssertTitle"), num))
			{
			case 3:
				new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Assert();
				try
				{
					Environment.Exit(1);
				}
				finally
				{
					CodeAccessPermission.RevertAssert();
				}
				break;
			case 4:
				if (!Debugger.IsAttached)
				{
					Debugger.Launch();
				}
				Debugger.Break();
				break;
			}
		}

		private static string TruncateMessageToFitScreen(string message)
		{
			IntPtr stockObject = Microsoft.Win32.SafeNativeMethods.GetStockObject(17);
			IntPtr dC = Microsoft.Win32.UnsafeNativeMethods.GetDC(NativeMethods.NullHandleRef);
			NativeMethods.TEXTMETRIC tEXTMETRIC = new NativeMethods.TEXTMETRIC();
			stockObject = Microsoft.Win32.UnsafeNativeMethods.SelectObject(new HandleRef(null, dC), new HandleRef(null, stockObject));
			Microsoft.Win32.SafeNativeMethods.GetTextMetrics(new HandleRef(null, dC), tEXTMETRIC);
			Microsoft.Win32.UnsafeNativeMethods.SelectObject(new HandleRef(null, dC), new HandleRef(null, stockObject));
			Microsoft.Win32.UnsafeNativeMethods.ReleaseDC(NativeMethods.NullHandleRef, new HandleRef(null, dC));
			dC = IntPtr.Zero;
			int systemMetrics = Microsoft.Win32.UnsafeNativeMethods.GetSystemMetrics(1);
			int num = systemMetrics / tEXTMETRIC.tmHeight - 15;
			int num2 = 0;
			int num3 = 0;
			int num4 = 0;
			while (num2 < num && num4 < message.Length - 1)
			{
				char c = message[num4];
				num3++;
				if (c == '\n' || c == '\r' || num3 > 80)
				{
					num2++;
					num3 = 0;
				}
				num4 = ((c == '\r' && message[num4 + 1] == '\n') ? (num4 + 2) : ((c != '\n' || message[num4 + 1] != '\r') ? (num4 + 1) : (num4 + 2)));
			}
			if (num4 < message.Length - 1)
			{
				message = SR.GetString("DebugMessageTruncated", message.Substring(0, num4));
			}
			return message;
		}
	}
}
