using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;

namespace System.Security.Principal
{
	internal sealed class Win32
	{
		internal const int FALSE = 0;

		internal const int TRUE = 1;

		private static bool _LsaApisSupported;

		private static bool _LsaLookupNames2Supported;

		private static bool _ConvertStringSidToSidSupported;

		private static bool _WellKnownSidApisSupported;

		internal static bool SddlConversionSupported => _ConvertStringSidToSidSupported;

		internal static bool LsaApisSupported => _LsaApisSupported;

		internal static bool LsaLookupNames2Supported => _LsaLookupNames2Supported;

		internal static bool WellKnownSidApisSupported => _WellKnownSidApisSupported;

		static Win32()
		{
			Win32Native.OSVERSIONINFO oSVERSIONINFO = new Win32Native.OSVERSIONINFO();
			if (!Win32Native.GetVersionEx(oSVERSIONINFO))
			{
				throw new SystemException(Environment.GetResourceString("InvalidOperation_GetVersion"));
			}
			if (oSVERSIONINFO.PlatformId == 2 && oSVERSIONINFO.MajorVersion >= 5)
			{
				_ConvertStringSidToSidSupported = true;
				_LsaApisSupported = true;
				if (oSVERSIONINFO.MajorVersion > 5 || oSVERSIONINFO.MinorVersion > 0)
				{
					_LsaLookupNames2Supported = true;
					_WellKnownSidApisSupported = true;
					return;
				}
				_LsaLookupNames2Supported = false;
				Win32Native.OSVERSIONINFOEX oSVERSIONINFOEX = new Win32Native.OSVERSIONINFOEX();
				if (!Win32Native.GetVersionEx(oSVERSIONINFOEX))
				{
					throw new SystemException(Environment.GetResourceString("InvalidOperation_GetVersion"));
				}
				if (oSVERSIONINFOEX.ServicePackMajor < 3)
				{
					_WellKnownSidApisSupported = false;
				}
				else
				{
					_WellKnownSidApisSupported = true;
				}
			}
			else
			{
				_LsaApisSupported = false;
				_LsaLookupNames2Supported = false;
				_ConvertStringSidToSidSupported = false;
				_WellKnownSidApisSupported = false;
			}
		}

		private Win32()
		{
		}

		internal static SafeLsaPolicyHandle LsaOpenPolicy(string systemName, PolicyRights rights)
		{
			if (!LsaApisSupported)
			{
				throw new PlatformNotSupportedException(Environment.GetResourceString("PlatformNotSupported_Win9x"));
			}
			Win32Native.LSA_OBJECT_ATTRIBUTES attributes = default(Win32Native.LSA_OBJECT_ATTRIBUTES);
			attributes.Length = Marshal.SizeOf(typeof(Win32Native.LSA_OBJECT_ATTRIBUTES));
			attributes.RootDirectory = IntPtr.Zero;
			attributes.ObjectName = IntPtr.Zero;
			attributes.Attributes = 0;
			attributes.SecurityDescriptor = IntPtr.Zero;
			attributes.SecurityQualityOfService = IntPtr.Zero;
			uint num;
			if ((num = Win32Native.LsaOpenPolicy(systemName, ref attributes, (int)rights, out var handle)) == 0)
			{
				return handle;
			}
			switch (num)
			{
			case 3221225506u:
				throw new UnauthorizedAccessException();
			case 3221225495u:
			case 3221225626u:
				throw new OutOfMemoryException();
			default:
			{
				int errorCode = Win32Native.LsaNtStatusToWinError((int)num);
				throw new SystemException(Win32Native.GetMessage(errorCode));
			}
			}
		}

		internal static byte[] ConvertIntPtrSidToByteArraySid(IntPtr binaryForm)
		{
			byte b = Marshal.ReadByte(binaryForm, 0);
			if (b != SecurityIdentifier.Revision)
			{
				throw new ArgumentException(Environment.GetResourceString("IdentityReference_InvalidSidRevision"), "binaryForm");
			}
			byte b2 = Marshal.ReadByte(binaryForm, 1);
			if (b2 < 0 || b2 > SecurityIdentifier.MaxSubAuthorities)
			{
				throw new ArgumentException(Environment.GetResourceString("IdentityReference_InvalidNumberOfSubauthorities", SecurityIdentifier.MaxSubAuthorities), "binaryForm");
			}
			int num = 8 + b2 * 4;
			byte[] array = new byte[num];
			Marshal.Copy(binaryForm, array, 0, num);
			return array;
		}

		internal static int CreateSidFromString(string stringSid, out byte[] resultSid)
		{
			IntPtr ByteArray = IntPtr.Zero;
			if (!SddlConversionSupported)
			{
				throw new PlatformNotSupportedException(Environment.GetResourceString("PlatformNotSupported_Win9x"));
			}
			int lastWin32Error;
			try
			{
				if (1 == Win32Native.ConvertStringSidToSid(stringSid, out ByteArray))
				{
					resultSid = ConvertIntPtrSidToByteArraySid(ByteArray);
					goto IL_0042;
				}
				lastWin32Error = Marshal.GetLastWin32Error();
			}
			finally
			{
				Win32Native.LocalFree(ByteArray);
			}
			resultSid = null;
			return lastWin32Error;
			IL_0042:
			return 0;
		}

		internal static int CreateWellKnownSid(WellKnownSidType sidType, SecurityIdentifier domainSid, out byte[] resultSid)
		{
			if (!WellKnownSidApisSupported)
			{
				throw new PlatformNotSupportedException(Environment.GetResourceString("PlatformNotSupported_RequiresW2kSP3"));
			}
			uint resultSidLength = (uint)SecurityIdentifier.MaxBinaryLength;
			resultSid = new byte[resultSidLength];
			if (Win32Native.CreateWellKnownSid((int)sidType, (domainSid == null) ? null : domainSid.BinaryForm, resultSid, ref resultSidLength) != 0)
			{
				return 0;
			}
			resultSid = null;
			return Marshal.GetLastWin32Error();
		}

		internal static bool IsEqualDomainSid(SecurityIdentifier sid1, SecurityIdentifier sid2)
		{
			if (!WellKnownSidApisSupported)
			{
				throw new PlatformNotSupportedException(Environment.GetResourceString("PlatformNotSupported_RequiresW2kSP3"));
			}
			if (sid1 == null || sid2 == null)
			{
				return false;
			}
			byte[] array = new byte[sid1.BinaryLength];
			sid1.GetBinaryForm(array, 0);
			byte[] array2 = new byte[sid2.BinaryLength];
			sid2.GetBinaryForm(array2, 0);
			if (Win32Native.IsEqualDomainSid(array, array2, out var result) != 0)
			{
				return result;
			}
			return false;
		}

		internal static int GetWindowsAccountDomainSid(SecurityIdentifier sid, out SecurityIdentifier resultSid)
		{
			if (!WellKnownSidApisSupported)
			{
				throw new PlatformNotSupportedException(Environment.GetResourceString("PlatformNotSupported_RequiresW2kSP3"));
			}
			byte[] array = new byte[sid.BinaryLength];
			sid.GetBinaryForm(array, 0);
			uint resultSidLength = (uint)SecurityIdentifier.MaxBinaryLength;
			byte[] array2 = new byte[resultSidLength];
			if (Win32Native.GetWindowsAccountDomainSid(array, array2, ref resultSidLength) != 0)
			{
				resultSid = new SecurityIdentifier(array2, 0);
				return 0;
			}
			resultSid = null;
			return Marshal.GetLastWin32Error();
		}

		internal static bool IsWellKnownSid(SecurityIdentifier sid, WellKnownSidType type)
		{
			if (!WellKnownSidApisSupported)
			{
				throw new PlatformNotSupportedException(Environment.GetResourceString("PlatformNotSupported_RequiresW2kSP3"));
			}
			byte[] array = new byte[sid.BinaryLength];
			sid.GetBinaryForm(array, 0);
			if (Win32Native.IsWellKnownSid(array, (int)type) == 0)
			{
				return false;
			}
			return true;
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern int ImpersonateLoggedOnUser(SafeTokenHandle hToken);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern int OpenThreadToken(TokenAccessLevels dwDesiredAccess, WinSecurityContext OpenAs, out SafeTokenHandle phThreadToken);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern int RevertToSelf();

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern int SetThreadToken(SafeTokenHandle hToken);
	}
}
