using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security.AccessControl;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;

namespace System.Security.Principal
{
	[Serializable]
	[ComVisible(true)]
	public class WindowsIdentity : IIdentity, ISerializable, IDeserializationCallback, IDisposable
	{
		private string m_name;

		private SecurityIdentifier m_owner;

		private SecurityIdentifier m_user;

		private object m_groups;

		private SafeTokenHandle m_safeTokenHandle = SafeTokenHandle.InvalidHandle;

		private string m_authType;

		private int m_isAuthenticated = -1;

		private static int s_runningOnWin2K = -1;

		public string AuthenticationType
		{
			get
			{
				if (m_safeTokenHandle.IsInvalid)
				{
					return string.Empty;
				}
				if (m_authType == null)
				{
					Win32Native.LUID LogonId = GetLogonAuthId(m_safeTokenHandle);
					if (LogonId.LowPart == 998)
					{
						return string.Empty;
					}
					SafeLsaReturnBufferHandle ppLogonSessionData = SafeLsaReturnBufferHandle.InvalidHandle;
					int num = Win32Native.LsaGetLogonSessionData(ref LogonId, ref ppLogonSessionData);
					if (num < 0)
					{
						throw GetExceptionFromNtStatus(num);
					}
					string result = Marshal.PtrToStringUni(((Win32Native.SECURITY_LOGON_SESSION_DATA)Marshal.PtrToStructure(ppLogonSessionData.DangerousGetHandle(), typeof(Win32Native.SECURITY_LOGON_SESSION_DATA))).AuthenticationPackage.Buffer);
					ppLogonSessionData.Dispose();
					return result;
				}
				return m_authType;
			}
		}

		[ComVisible(false)]
		public TokenImpersonationLevel ImpersonationLevel
		{
			get
			{
				if (m_safeTokenHandle.IsInvalid)
				{
					return TokenImpersonationLevel.Anonymous;
				}
				uint dwLength = 0u;
				SafeLocalAllocHandle tokenInformation = GetTokenInformation(m_safeTokenHandle, TokenInformationClass.TokenType, out dwLength);
				int num = Marshal.ReadInt32(tokenInformation.DangerousGetHandle());
				if (num == 1)
				{
					return TokenImpersonationLevel.None;
				}
				SafeLocalAllocHandle tokenInformation2 = GetTokenInformation(m_safeTokenHandle, TokenInformationClass.TokenImpersonationLevel, out dwLength);
				num = Marshal.ReadInt32(tokenInformation2.DangerousGetHandle());
				tokenInformation.Dispose();
				tokenInformation2.Dispose();
				return (TokenImpersonationLevel)(num + 1);
			}
		}

		public virtual bool IsAuthenticated
		{
			get
			{
				if (!RunningOnWin2K)
				{
					return false;
				}
				if (m_isAuthenticated == -1)
				{
					WindowsPrincipal windowsPrincipal = new WindowsPrincipal(this);
					SecurityIdentifier sid = new SecurityIdentifier(IdentifierAuthority.NTAuthority, new int[1]
					{
						11
					});
					m_isAuthenticated = (windowsPrincipal.IsInRole(sid) ? 1 : 0);
				}
				return m_isAuthenticated == 1;
			}
		}

		public virtual bool IsGuest
		{
			get
			{
				if (m_safeTokenHandle.IsInvalid)
				{
					return false;
				}
				SecurityIdentifier right = new SecurityIdentifier(IdentifierAuthority.NTAuthority, new int[2]
				{
					32,
					501
				});
				return User == right;
			}
		}

		public virtual bool IsSystem
		{
			get
			{
				if (m_safeTokenHandle.IsInvalid)
				{
					return false;
				}
				SecurityIdentifier right = new SecurityIdentifier(IdentifierAuthority.NTAuthority, new int[1]
				{
					18
				});
				return User == right;
			}
		}

		public virtual bool IsAnonymous
		{
			get
			{
				if (m_safeTokenHandle.IsInvalid)
				{
					return true;
				}
				SecurityIdentifier right = new SecurityIdentifier(IdentifierAuthority.NTAuthority, new int[1]
				{
					7
				});
				return User == right;
			}
		}

		public virtual string Name => GetName();

		[ComVisible(false)]
		public SecurityIdentifier Owner
		{
			get
			{
				if (m_safeTokenHandle.IsInvalid)
				{
					return null;
				}
				if (m_owner == null)
				{
					uint dwLength = 0u;
					SafeLocalAllocHandle tokenInformation = GetTokenInformation(m_safeTokenHandle, TokenInformationClass.TokenOwner, out dwLength);
					m_owner = new SecurityIdentifier(Marshal.ReadIntPtr(tokenInformation.DangerousGetHandle()), noDemand: true);
					tokenInformation.Dispose();
				}
				return m_owner;
			}
		}

		[ComVisible(false)]
		public SecurityIdentifier User
		{
			get
			{
				if (m_safeTokenHandle.IsInvalid)
				{
					return null;
				}
				if (m_user == null)
				{
					uint dwLength = 0u;
					SafeLocalAllocHandle tokenInformation = GetTokenInformation(m_safeTokenHandle, TokenInformationClass.TokenUser, out dwLength);
					m_user = new SecurityIdentifier(Marshal.ReadIntPtr(tokenInformation.DangerousGetHandle()), noDemand: true);
					tokenInformation.Dispose();
				}
				return m_user;
			}
		}

		public IdentityReferenceCollection Groups
		{
			get
			{
				if (m_safeTokenHandle.IsInvalid)
				{
					return null;
				}
				if (m_groups == null)
				{
					IdentityReferenceCollection identityReferenceCollection = new IdentityReferenceCollection();
					uint dwLength = 0u;
					using (SafeLocalAllocHandle safeLocalAllocHandle = GetTokenInformation(m_safeTokenHandle, TokenInformationClass.TokenGroups, out dwLength))
					{
						int num = Marshal.ReadInt32(safeLocalAllocHandle.DangerousGetHandle());
						IntPtr intPtr = new IntPtr((long)safeLocalAllocHandle.DangerousGetHandle() + (long)Marshal.OffsetOf(typeof(Win32Native.TOKEN_GROUPS), "Groups"));
						for (int i = 0; i < num; i++)
						{
							Win32Native.SID_AND_ATTRIBUTES sID_AND_ATTRIBUTES = (Win32Native.SID_AND_ATTRIBUTES)Marshal.PtrToStructure(intPtr, typeof(Win32Native.SID_AND_ATTRIBUTES));
							uint num2 = 3221225492u;
							if ((sID_AND_ATTRIBUTES.Attributes & num2) == 4)
							{
								identityReferenceCollection.Add(new SecurityIdentifier(sID_AND_ATTRIBUTES.Sid, noDemand: true));
							}
							intPtr = new IntPtr((long)intPtr + Marshal.SizeOf(typeof(Win32Native.SID_AND_ATTRIBUTES)));
						}
					}
					Interlocked.CompareExchange(ref m_groups, identityReferenceCollection, null);
				}
				return m_groups as IdentityReferenceCollection;
			}
		}

		public virtual IntPtr Token
		{
			[SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
			get
			{
				return m_safeTokenHandle.DangerousGetHandle();
			}
		}

		internal SafeTokenHandle TokenHandle => m_safeTokenHandle;

		internal static bool RunningOnWin2K
		{
			[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
			get
			{
				if (s_runningOnWin2K == -1)
				{
					Win32Native.OSVERSIONINFO oSVERSIONINFO = new Win32Native.OSVERSIONINFO();
					s_runningOnWin2K = ((Win32Native.GetVersionEx(oSVERSIONINFO) && oSVERSIONINFO.PlatformId == 2 && oSVERSIONINFO.MajorVersion >= 5) ? 1 : 0);
				}
				return s_runningOnWin2K == 1;
			}
		}

		private WindowsIdentity()
		{
		}

		internal WindowsIdentity(SafeTokenHandle safeTokenHandle)
			: this(safeTokenHandle.DangerousGetHandle())
		{
			GC.KeepAlive(safeTokenHandle);
		}

		[SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		[SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.ControlPrincipal)]
		public WindowsIdentity(IntPtr userToken)
			: this(userToken, null, -1)
		{
		}

		[SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		[SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.ControlPrincipal)]
		public WindowsIdentity(IntPtr userToken, string type)
			: this(userToken, type, -1)
		{
		}

		[SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		[SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.ControlPrincipal)]
		public WindowsIdentity(IntPtr userToken, string type, WindowsAccountType acctType)
			: this(userToken, type, -1)
		{
		}

		[SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		[SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.ControlPrincipal)]
		public WindowsIdentity(IntPtr userToken, string type, WindowsAccountType acctType, bool isAuthenticated)
			: this(userToken, type, isAuthenticated ? 1 : 0)
		{
		}

		private WindowsIdentity(IntPtr userToken, string authType, int isAuthenticated)
		{
			CreateFromToken(userToken);
			m_authType = authType;
			m_isAuthenticated = isAuthenticated;
		}

		private void CreateFromToken(IntPtr userToken)
		{
			if (userToken == IntPtr.Zero)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_TokenZero"));
			}
			uint ReturnLength = (uint)Marshal.SizeOf(typeof(uint));
			Win32Native.GetTokenInformation(userToken, 8u, SafeLocalAllocHandle.InvalidHandle, 0u, out ReturnLength);
			if (Marshal.GetLastWin32Error() == 6)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_InvalidImpersonationToken"));
			}
			if (!Win32Native.DuplicateHandle(Win32Native.GetCurrentProcess(), userToken, Win32Native.GetCurrentProcess(), ref m_safeTokenHandle, 0u, bInheritHandle: true, 2u))
			{
				throw new SecurityException(Win32Native.GetMessage(Marshal.GetLastWin32Error()));
			}
		}

		[SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.ControlPrincipal)]
		public WindowsIdentity(string sUserPrincipalName)
			: this(sUserPrincipalName, null)
		{
		}

		[SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.ControlPrincipal)]
		public WindowsIdentity(string sUserPrincipalName, string type)
		{
			m_safeTokenHandle = KerbS4ULogon(sUserPrincipalName);
		}

		[SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.ControlPrincipal)]
		public WindowsIdentity(SerializationInfo info, StreamingContext context)
			: this(info)
		{
		}

		private WindowsIdentity(SerializationInfo info)
		{
			IntPtr intPtr = (IntPtr)info.GetValue("m_userToken", typeof(IntPtr));
			if (intPtr != IntPtr.Zero)
			{
				CreateFromToken(intPtr);
			}
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
		void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("m_userToken", m_safeTokenHandle.DangerousGetHandle());
		}

		void IDeserializationCallback.OnDeserialization(object sender)
		{
		}

		[SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.ControlPrincipal)]
		public static WindowsIdentity GetCurrent()
		{
			return GetCurrentInternal(TokenAccessLevels.MaximumAllowed, threadOnly: false);
		}

		[SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.ControlPrincipal)]
		public static WindowsIdentity GetCurrent(bool ifImpersonating)
		{
			return GetCurrentInternal(TokenAccessLevels.MaximumAllowed, ifImpersonating);
		}

		[SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.ControlPrincipal)]
		public static WindowsIdentity GetCurrent(TokenAccessLevels desiredAccess)
		{
			return GetCurrentInternal(desiredAccess, threadOnly: false);
		}

		public static WindowsIdentity GetAnonymous()
		{
			return new WindowsIdentity();
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		internal string GetName()
		{
			StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
			if (m_safeTokenHandle.IsInvalid)
			{
				return string.Empty;
			}
			if (m_name == null)
			{
				using (SafeImpersonate(SafeTokenHandle.InvalidHandle, null, ref stackMark))
				{
					NTAccount nTAccount = User.Translate(typeof(NTAccount)) as NTAccount;
					m_name = nTAccount.ToString();
				}
			}
			return m_name;
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public virtual WindowsImpersonationContext Impersonate()
		{
			StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
			return Impersonate(ref stackMark);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		[SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.ControlPrincipal)]
		public static WindowsImpersonationContext Impersonate(IntPtr userToken)
		{
			StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
			if (userToken == IntPtr.Zero)
			{
				return SafeImpersonate(SafeTokenHandle.InvalidHandle, null, ref stackMark);
			}
			WindowsIdentity windowsIdentity = new WindowsIdentity(userToken);
			return windowsIdentity.Impersonate(ref stackMark);
		}

		internal WindowsImpersonationContext Impersonate(ref StackCrawlMark stackMark)
		{
			if (!RunningOnWin2K)
			{
				return new WindowsImpersonationContext(SafeTokenHandle.InvalidHandle, GetCurrentThreadWI(), isImpersonating: false, null);
			}
			if (m_safeTokenHandle.IsInvalid)
			{
				throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_AnonymousCannotImpersonate"));
			}
			return SafeImpersonate(m_safeTokenHandle, this, ref stackMark);
		}

		[ComVisible(false)]
		protected virtual void Dispose(bool disposing)
		{
			if (disposing && m_safeTokenHandle != null && !m_safeTokenHandle.IsClosed)
			{
				m_safeTokenHandle.Dispose();
			}
			m_name = null;
			m_owner = null;
			m_user = null;
		}

		[ComVisible(false)]
		public void Dispose()
		{
			Dispose(disposing: true);
		}

		internal static WindowsImpersonationContext SafeImpersonate(SafeTokenHandle userToken, WindowsIdentity wi, ref StackCrawlMark stackMark)
		{
			if (!RunningOnWin2K)
			{
				return new WindowsImpersonationContext(SafeTokenHandle.InvalidHandle, GetCurrentThreadWI(), isImpersonating: false, null);
			}
			int hr = 0;
			bool isImpersonating;
			SafeTokenHandle currentToken = GetCurrentToken(TokenAccessLevels.MaximumAllowed, threadOnly: false, out isImpersonating, out hr);
			if (currentToken == null || currentToken.IsInvalid)
			{
				throw new SecurityException(Win32Native.GetMessage(hr));
			}
			FrameSecurityDescriptor securityObjectForFrame = SecurityRuntime.GetSecurityObjectForFrame(ref stackMark, create: true);
			if (securityObjectForFrame == null && SecurityManager._IsSecurityOn())
			{
				throw new SecurityException(Environment.GetResourceString("ExecutionEngine_MissingSecurityDescriptor"));
			}
			WindowsImpersonationContext windowsImpersonationContext = new WindowsImpersonationContext(currentToken, GetCurrentThreadWI(), isImpersonating, securityObjectForFrame);
			if (userToken.IsInvalid)
			{
				hr = Win32.RevertToSelf();
				if (hr < 0)
				{
					throw new SecurityException(Win32Native.GetMessage(hr));
				}
				UpdateThreadWI(wi);
				securityObjectForFrame.SetTokenHandles(currentToken, wi?.TokenHandle);
			}
			else
			{
				hr = Win32.RevertToSelf();
				if (hr < 0)
				{
					throw new SecurityException(Win32Native.GetMessage(hr));
				}
				hr = Win32.ImpersonateLoggedOnUser(userToken);
				if (hr < 0)
				{
					windowsImpersonationContext.Undo();
					throw new SecurityException(Environment.GetResourceString("Argument_ImpersonateUser"));
				}
				UpdateThreadWI(wi);
				securityObjectForFrame.SetTokenHandles(currentToken, wi?.TokenHandle);
			}
			return windowsImpersonationContext;
		}

		internal static WindowsIdentity GetCurrentThreadWI()
		{
			return SecurityContext.GetCurrentWI(Thread.CurrentThread.GetExecutionContextNoCreate());
		}

		internal static void UpdateThreadWI(WindowsIdentity wi)
		{
			SecurityContext securityContext = SecurityContext.GetCurrentSecurityContextNoCreate();
			if (wi != null && securityContext == null)
			{
				securityContext = new SecurityContext();
				Thread.CurrentThread.ExecutionContext.SecurityContext = securityContext;
			}
			if (securityContext != null)
			{
				securityContext.WindowsIdentity = wi;
			}
		}

		internal static WindowsIdentity GetCurrentInternal(TokenAccessLevels desiredAccess, bool threadOnly)
		{
			WindowsIdentity windowsIdentity = null;
			if (!RunningOnWin2K)
			{
				if (!threadOnly)
				{
					windowsIdentity = new WindowsIdentity();
					windowsIdentity.m_name = string.Empty;
				}
				return windowsIdentity;
			}
			int hr = 0;
			bool isImpersonating;
			SafeTokenHandle currentToken = GetCurrentToken(desiredAccess, threadOnly, out isImpersonating, out hr);
			if (currentToken == null || currentToken.IsInvalid)
			{
				if (threadOnly && !isImpersonating)
				{
					return windowsIdentity;
				}
				throw new SecurityException(Win32Native.GetMessage(hr));
			}
			windowsIdentity = new WindowsIdentity();
			windowsIdentity.m_safeTokenHandle.Dispose();
			windowsIdentity.m_safeTokenHandle = currentToken;
			return windowsIdentity;
		}

		private static int GetHRForWin32Error(int dwLastError)
		{
			if ((dwLastError & 0x80000000u) == 2147483648u)
			{
				return dwLastError;
			}
			return (dwLastError & 0xFFFF) | -2147024896;
		}

		private static Exception GetExceptionFromNtStatus(int status)
		{
			switch (status)
			{
			case -1073741790:
				return new UnauthorizedAccessException();
			case -1073741801:
			case -1073741670:
				return new OutOfMemoryException();
			default:
			{
				int errorCode = Win32Native.LsaNtStatusToWinError(status);
				return new SecurityException(Win32Native.GetMessage(errorCode));
			}
			}
		}

		private static SafeTokenHandle GetCurrentToken(TokenAccessLevels desiredAccess, bool threadOnly, out bool isImpersonating, out int hr)
		{
			isImpersonating = true;
			SafeTokenHandle safeTokenHandle = GetCurrentThreadToken(desiredAccess, out hr);
			if (safeTokenHandle == null && hr == GetHRForWin32Error(1008))
			{
				isImpersonating = false;
				if (!threadOnly)
				{
					safeTokenHandle = GetCurrentProcessToken(desiredAccess, out hr);
				}
			}
			return safeTokenHandle;
		}

		private static SafeTokenHandle GetCurrentProcessToken(TokenAccessLevels desiredAccess, out int hr)
		{
			hr = 0;
			SafeTokenHandle TokenHandle = SafeTokenHandle.InvalidHandle;
			if (!Win32Native.OpenProcessToken(Win32Native.GetCurrentProcess(), desiredAccess, ref TokenHandle))
			{
				hr = GetHRForWin32Error(Marshal.GetLastWin32Error());
			}
			return TokenHandle;
		}

		internal static SafeTokenHandle GetCurrentThreadToken(TokenAccessLevels desiredAccess, out int hr)
		{
			hr = Win32.OpenThreadToken(desiredAccess, WinSecurityContext.Both, out var phThreadToken);
			return phThreadToken;
		}

		private static Win32Native.LUID GetLogonAuthId(SafeTokenHandle safeTokenHandle)
		{
			uint dwLength = 0u;
			SafeLocalAllocHandle tokenInformation = GetTokenInformation(safeTokenHandle, TokenInformationClass.TokenStatistics, out dwLength);
			Win32Native.TOKEN_STATISTICS tOKEN_STATISTICS = (Win32Native.TOKEN_STATISTICS)Marshal.PtrToStructure(tokenInformation.DangerousGetHandle(), typeof(Win32Native.TOKEN_STATISTICS));
			tokenInformation.Dispose();
			return tOKEN_STATISTICS.AuthenticationId;
		}

		private static SafeLocalAllocHandle GetTokenInformation(SafeTokenHandle tokenHandle, TokenInformationClass tokenInformationClass, out uint dwLength)
		{
			SafeLocalAllocHandle invalidHandle = SafeLocalAllocHandle.InvalidHandle;
			dwLength = (uint)Marshal.SizeOf(typeof(uint));
			bool tokenInformation = Win32Native.GetTokenInformation(tokenHandle, (uint)tokenInformationClass, invalidHandle, 0u, out dwLength);
			int lastWin32Error = Marshal.GetLastWin32Error();
			switch (lastWin32Error)
			{
			case 24:
			case 122:
			{
				IntPtr sizetdwBytes = new IntPtr(dwLength);
				invalidHandle.Dispose();
				invalidHandle = Win32Native.LocalAlloc(0, sizetdwBytes);
				if (invalidHandle == null || invalidHandle.IsInvalid)
				{
					throw new OutOfMemoryException();
				}
				if (!Win32Native.GetTokenInformation(tokenHandle, (uint)tokenInformationClass, invalidHandle, dwLength, out dwLength))
				{
					throw new SecurityException(Win32Native.GetMessage(Marshal.GetLastWin32Error()));
				}
				return invalidHandle;
			}
			case 6:
				throw new ArgumentException(Environment.GetResourceString("Argument_InvalidImpersonationToken"));
			default:
				throw new SecurityException(Win32Native.GetMessage(lastWin32Error));
			}
		}

		private unsafe static SafeTokenHandle KerbS4ULogon(string upn)
		{
			byte[] array = new byte[3]
			{
				67,
				76,
				82
			};
			IntPtr sizetdwBytes = new IntPtr((uint)(array.Length + 1));
			SafeLocalAllocHandle safeLocalAllocHandle = Win32Native.LocalAlloc(64, sizetdwBytes);
			Marshal.Copy(array, 0, safeLocalAllocHandle.DangerousGetHandle(), array.Length);
			Win32Native.UNICODE_INTPTR_STRING LogonProcessName = new Win32Native.UNICODE_INTPTR_STRING(array.Length, array.Length + 1, safeLocalAllocHandle.DangerousGetHandle());
			SafeLsaLogonProcessHandle LsaHandle = SafeLsaLogonProcessHandle.InvalidHandle;
			Privilege privilege = null;
			RuntimeHelpers.PrepareConstrainedRegions();
			int num;
			try
			{
				try
				{
					privilege = new Privilege("SeTcbPrivilege");
					privilege.Enable();
				}
				catch (PrivilegeNotHeldException)
				{
				}
				IntPtr SecurityMode = IntPtr.Zero;
				num = Win32Native.LsaRegisterLogonProcess(ref LogonProcessName, ref LsaHandle, ref SecurityMode);
				if (5 == Win32Native.LsaNtStatusToWinError(num))
				{
					num = Win32Native.LsaConnectUntrusted(ref LsaHandle);
				}
			}
			catch
			{
				privilege?.Revert();
				throw;
			}
			finally
			{
				privilege?.Revert();
			}
			if (num < 0)
			{
				throw GetExceptionFromNtStatus(num);
			}
			byte[] array2 = new byte["Kerberos".Length + 1];
			Encoding.ASCII.GetBytes("Kerberos", 0, "Kerberos".Length, array2, 0);
			sizetdwBytes = new IntPtr((uint)array2.Length);
			SafeLocalAllocHandle safeLocalAllocHandle2 = Win32Native.LocalAlloc(0, sizetdwBytes);
			if (safeLocalAllocHandle2 == null || safeLocalAllocHandle2.IsInvalid)
			{
				throw new OutOfMemoryException();
			}
			Marshal.Copy(array2, 0, safeLocalAllocHandle2.DangerousGetHandle(), array2.Length);
			Win32Native.UNICODE_INTPTR_STRING PackageName = new Win32Native.UNICODE_INTPTR_STRING("Kerberos".Length, "Kerberos".Length + 1, safeLocalAllocHandle2.DangerousGetHandle());
			uint AuthenticationPackage = 0u;
			num = Win32Native.LsaLookupAuthenticationPackage(LsaHandle, ref PackageName, ref AuthenticationPackage);
			if (num < 0)
			{
				throw GetExceptionFromNtStatus(num);
			}
			Win32Native.TOKEN_SOURCE SourceContext = default(Win32Native.TOKEN_SOURCE);
			if (!Win32Native.AllocateLocallyUniqueId(ref SourceContext.SourceIdentifier))
			{
				throw new SecurityException(Win32Native.GetMessage(Marshal.GetLastWin32Error()));
			}
			SourceContext.Name = new char[8];
			SourceContext.Name[0] = 'C';
			SourceContext.Name[1] = 'L';
			SourceContext.Name[2] = 'R';
			uint ProfileBufferLength = 0u;
			SafeLsaReturnBufferHandle ProfileBuffer = SafeLsaReturnBufferHandle.InvalidHandle;
			Win32Native.LUID LogonId = default(Win32Native.LUID);
			Win32Native.QUOTA_LIMITS Quotas = default(Win32Native.QUOTA_LIMITS);
			int SubStatus = 0;
			SafeTokenHandle Token = SafeTokenHandle.InvalidHandle;
			int num2 = Marshal.SizeOf(typeof(Win32Native.KERB_S4U_LOGON)) + 2 * (upn.Length + 1);
			byte[] array3 = new byte[num2];
			fixed (byte* ptr = array3)
			{
				byte[] array4 = new byte[2 * (upn.Length + 1)];
				Encoding.Unicode.GetBytes(upn, 0, upn.Length, array4, 0);
				Buffer.BlockCopy(array4, 0, array3, Marshal.SizeOf(typeof(Win32Native.KERB_S4U_LOGON)), array4.Length);
				Win32Native.KERB_S4U_LOGON* ptr2 = (Win32Native.KERB_S4U_LOGON*)ptr;
				ptr2->MessageType = 12u;
				ptr2->Flags = 0u;
				ptr2->ClientUpn.Length = (ushort)(2 * upn.Length);
				ptr2->ClientUpn.MaxLength = (ushort)(2 * (upn.Length + 1));
				ptr2->ClientUpn.Buffer = new IntPtr(ptr2 + 1);
				num = Win32Native.LsaLogonUser(LsaHandle, ref LogonProcessName, 3u, AuthenticationPackage, new IntPtr(ptr), (uint)array3.Length, IntPtr.Zero, ref SourceContext, ref ProfileBuffer, ref ProfileBufferLength, ref LogonId, ref Token, ref Quotas, ref SubStatus);
			}
			if (num == -1073741714 && SubStatus < 0)
			{
				num = SubStatus;
			}
			if (num < 0)
			{
				throw GetExceptionFromNtStatus(num);
			}
			if (SubStatus < 0)
			{
				throw GetExceptionFromNtStatus(SubStatus);
			}
			ProfileBuffer.Dispose();
			safeLocalAllocHandle.Dispose();
			safeLocalAllocHandle2.Dispose();
			LsaHandle.Dispose();
			return Token;
		}
	}
}
