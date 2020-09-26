using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Permissions;
using System.Text;
using Microsoft.Win32.SafeHandles;

namespace Microsoft.Win32
{
	[ComVisible(true)]
	public sealed class RegistryKey : MarshalByRefObject, IDisposable
	{
		private const int STATE_DIRTY = 1;

		private const int STATE_SYSTEMKEY = 2;

		private const int STATE_WRITEACCESS = 4;

		private const int STATE_PERF_DATA = 8;

		private const int MaxKeyLength = 255;

		private const int FORMAT_MESSAGE_IGNORE_INSERTS = 512;

		private const int FORMAT_MESSAGE_FROM_SYSTEM = 4096;

		private const int FORMAT_MESSAGE_ARGUMENT_ARRAY = 8192;

		internal static readonly IntPtr HKEY_CLASSES_ROOT = new IntPtr(int.MinValue);

		internal static readonly IntPtr HKEY_CURRENT_USER = new IntPtr(-2147483647);

		internal static readonly IntPtr HKEY_LOCAL_MACHINE = new IntPtr(-2147483646);

		internal static readonly IntPtr HKEY_USERS = new IntPtr(-2147483645);

		internal static readonly IntPtr HKEY_PERFORMANCE_DATA = new IntPtr(-2147483644);

		internal static readonly IntPtr HKEY_CURRENT_CONFIG = new IntPtr(-2147483643);

		internal static readonly IntPtr HKEY_DYN_DATA = new IntPtr(-2147483642);

		private static readonly string[] hkeyNames = new string[7]
		{
			"HKEY_CLASSES_ROOT",
			"HKEY_CURRENT_USER",
			"HKEY_LOCAL_MACHINE",
			"HKEY_USERS",
			"HKEY_PERFORMANCE_DATA",
			"HKEY_CURRENT_CONFIG",
			"HKEY_DYN_DATA"
		};

		private SafeRegistryHandle hkey;

		private int state;

		private string keyName;

		private bool remoteKey;

		private RegistryKeyPermissionCheck checkMode;

		private static readonly int _SystemDefaultCharSize = 3 - Win32Native.lstrlen(new sbyte[4]
		{
			65,
			65,
			0,
			0
		});

		public int SubKeyCount
		{
			get
			{
				CheckKeyReadPermission();
				return InternalSubKeyCount();
			}
		}

		public int ValueCount
		{
			get
			{
				CheckKeyReadPermission();
				return InternalValueCount();
			}
		}

		public string Name
		{
			get
			{
				EnsureNotDisposed();
				return keyName;
			}
		}

		private RegistryKey(SafeRegistryHandle hkey, bool writable)
			: this(hkey, writable, systemkey: false, remoteKey: false, isPerfData: false)
		{
		}

		private RegistryKey(SafeRegistryHandle hkey, bool writable, bool systemkey, bool remoteKey, bool isPerfData)
		{
			this.hkey = hkey;
			keyName = "";
			this.remoteKey = remoteKey;
			if (systemkey)
			{
				state |= 2;
			}
			if (writable)
			{
				state |= 4;
			}
			if (isPerfData)
			{
				state |= 8;
			}
		}

		public void Close()
		{
			Dispose(disposing: true);
		}

		private void Dispose(bool disposing)
		{
			if (hkey == null)
			{
				return;
			}
			bool flag = IsPerfDataKey();
			if (!IsSystemKey() || flag)
			{
				try
				{
					hkey.Dispose();
				}
				catch (IOException)
				{
				}
				if (flag)
				{
					hkey = new SafeRegistryHandle(HKEY_PERFORMANCE_DATA, !IsWin9x());
				}
				else
				{
					hkey = null;
				}
			}
		}

		public void Flush()
		{
			if (hkey != null && IsDirty())
			{
				Win32Native.RegFlushKey(hkey);
			}
		}

		void IDisposable.Dispose()
		{
			Dispose(disposing: true);
		}

		public RegistryKey CreateSubKey(string subkey)
		{
			return CreateSubKey(subkey, checkMode);
		}

		[ComVisible(false)]
		public RegistryKey CreateSubKey(string subkey, RegistryKeyPermissionCheck permissionCheck)
		{
			return CreateSubKey(subkey, permissionCheck, null);
		}

		[ComVisible(false)]
		public unsafe RegistryKey CreateSubKey(string subkey, RegistryKeyPermissionCheck permissionCheck, RegistrySecurity registrySecurity)
		{
			ValidateKeyName(subkey);
			ValidateKeyMode(permissionCheck);
			EnsureWriteable();
			subkey = FixupName(subkey);
			if (!remoteKey)
			{
				RegistryKey registryKey = InternalOpenSubKey(subkey, permissionCheck != RegistryKeyPermissionCheck.ReadSubTree);
				if (registryKey != null)
				{
					CheckSubKeyWritePermission(subkey);
					CheckSubTreePermission(subkey, permissionCheck);
					registryKey.checkMode = permissionCheck;
					return registryKey;
				}
			}
			CheckSubKeyCreatePermission(subkey);
			Win32Native.SECURITY_ATTRIBUTES sECURITY_ATTRIBUTES = null;
			if (registrySecurity != null)
			{
				sECURITY_ATTRIBUTES = new Win32Native.SECURITY_ATTRIBUTES();
				sECURITY_ATTRIBUTES.nLength = Marshal.SizeOf(sECURITY_ATTRIBUTES);
				byte[] securityDescriptorBinaryForm = registrySecurity.GetSecurityDescriptorBinaryForm();
				byte* ptr = stackalloc byte[1 * securityDescriptorBinaryForm.Length];
				Buffer.memcpy(securityDescriptorBinaryForm, 0, ptr, 0, securityDescriptorBinaryForm.Length);
				sECURITY_ATTRIBUTES.pSecurityDescriptor = ptr;
			}
			int lpdwDisposition = 0;
			SafeRegistryHandle hkResult = null;
			int num = Win32Native.RegCreateKeyEx(hkey, subkey, 0, null, 0, GetRegistryKeyAccess(permissionCheck != RegistryKeyPermissionCheck.ReadSubTree), sECURITY_ATTRIBUTES, out hkResult, out lpdwDisposition);
			if (num == 0 && !hkResult.IsInvalid)
			{
				RegistryKey registryKey2 = new RegistryKey(hkResult, permissionCheck != RegistryKeyPermissionCheck.ReadSubTree, systemkey: false, remoteKey, isPerfData: false);
				CheckSubTreePermission(subkey, permissionCheck);
				registryKey2.checkMode = permissionCheck;
				if (subkey.Length == 0)
				{
					registryKey2.keyName = keyName;
				}
				else
				{
					registryKey2.keyName = keyName + "\\" + subkey;
				}
				return registryKey2;
			}
			if (num != 0)
			{
				Win32Error(num, keyName + "\\" + subkey);
			}
			return null;
		}

		public void DeleteSubKey(string subkey)
		{
			DeleteSubKey(subkey, throwOnMissingSubKey: true);
		}

		public void DeleteSubKey(string subkey, bool throwOnMissingSubKey)
		{
			ValidateKeyName(subkey);
			EnsureWriteable();
			subkey = FixupName(subkey);
			CheckSubKeyWritePermission(subkey);
			RegistryKey registryKey = InternalOpenSubKey(subkey, writable: false);
			if (registryKey != null)
			{
				try
				{
					if (registryKey.InternalSubKeyCount() > 0)
					{
						ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_RegRemoveSubKey);
					}
				}
				finally
				{
					registryKey.Close();
				}
				int num = Win32Native.RegDeleteKey(hkey, subkey);
				switch (num)
				{
				case 2:
					if (throwOnMissingSubKey)
					{
						ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_RegSubKeyAbsent);
					}
					break;
				default:
					Win32Error(num, null);
					break;
				case 0:
					break;
				}
			}
			else if (throwOnMissingSubKey)
			{
				ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_RegSubKeyAbsent);
			}
		}

		public void DeleteSubKeyTree(string subkey)
		{
			ValidateKeyName(subkey);
			if (subkey.Length == 0 && IsSystemKey())
			{
				ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_RegKeyDelHive);
			}
			EnsureWriteable();
			subkey = FixupName(subkey);
			CheckSubTreeWritePermission(subkey);
			RegistryKey registryKey = InternalOpenSubKey(subkey, writable: true);
			if (registryKey != null)
			{
				try
				{
					if (registryKey.InternalSubKeyCount() > 0)
					{
						string[] array = registryKey.InternalGetSubKeyNames();
						for (int i = 0; i < array.Length; i++)
						{
							registryKey.DeleteSubKeyTreeInternal(array[i]);
						}
					}
				}
				finally
				{
					registryKey.Close();
				}
				int num = Win32Native.RegDeleteKey(hkey, subkey);
				if (num != 0)
				{
					Win32Error(num, null);
				}
			}
			else
			{
				ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_RegSubKeyAbsent);
			}
		}

		private void DeleteSubKeyTreeInternal(string subkey)
		{
			RegistryKey registryKey = InternalOpenSubKey(subkey, writable: true);
			if (registryKey != null)
			{
				try
				{
					if (registryKey.InternalSubKeyCount() > 0)
					{
						string[] array = registryKey.InternalGetSubKeyNames();
						for (int i = 0; i < array.Length; i++)
						{
							registryKey.DeleteSubKeyTreeInternal(array[i]);
						}
					}
				}
				finally
				{
					registryKey.Close();
				}
				int num = Win32Native.RegDeleteKey(hkey, subkey);
				if (num != 0)
				{
					Win32Error(num, null);
				}
			}
			else
			{
				ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_RegSubKeyAbsent);
			}
		}

		public void DeleteValue(string name)
		{
			DeleteValue(name, throwOnMissingValue: true);
		}

		public void DeleteValue(string name, bool throwOnMissingValue)
		{
			if (name == null)
			{
				ThrowHelper.ThrowArgumentNullException(ExceptionArgument.name);
			}
			EnsureWriteable();
			CheckValueWritePermission(name);
			int num = Win32Native.RegDeleteValue(hkey, name);
			if ((num == 2 || num == 206) && throwOnMissingValue)
			{
				ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_RegSubKeyValueAbsent);
			}
		}

		internal static RegistryKey GetBaseKey(IntPtr hKey)
		{
			int num = (int)hKey & 0xFFFFFFF;
			bool flag = hKey == HKEY_PERFORMANCE_DATA;
			SafeRegistryHandle safeRegistryHandle = new SafeRegistryHandle(hKey, flag && !IsWin9x());
			RegistryKey registryKey = new RegistryKey(safeRegistryHandle, writable: true, systemkey: true, remoteKey: false, flag);
			registryKey.checkMode = RegistryKeyPermissionCheck.Default;
			registryKey.keyName = hkeyNames[num];
			return registryKey;
		}

		public static RegistryKey OpenRemoteBaseKey(RegistryHive hKey, string machineName)
		{
			if (machineName == null)
			{
				throw new ArgumentNullException("machineName");
			}
			int num = (int)(hKey & (RegistryHive)268435455);
			if (num < 0 || num >= hkeyNames.Length || ((ulong)hKey & 0xFFFFFFF0uL) != 2147483648u)
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_RegKeyOutOfRange"));
			}
			CheckUnmanagedCodePermission();
			SafeRegistryHandle result = null;
			int num2 = Win32Native.RegConnectRegistry(machineName, new SafeRegistryHandle(new IntPtr((int)hKey), ownsHandle: false), out result);
			switch (num2)
			{
			case 1114:
				throw new ArgumentException(Environment.GetResourceString("Arg_DllInitFailure"));
			default:
				Win32ErrorStatic(num2, null);
				break;
			case 0:
				break;
			}
			if (result.IsInvalid)
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_RegKeyNoRemoteConnect", machineName));
			}
			RegistryKey registryKey = new RegistryKey(result, writable: true, systemkey: false, remoteKey: true, (IntPtr)(long)hKey == HKEY_PERFORMANCE_DATA);
			registryKey.checkMode = RegistryKeyPermissionCheck.Default;
			registryKey.keyName = hkeyNames[num];
			return registryKey;
		}

		public RegistryKey OpenSubKey(string name, bool writable)
		{
			ValidateKeyName(name);
			EnsureNotDisposed();
			name = FixupName(name);
			CheckOpenSubKeyPermission(name, writable);
			SafeRegistryHandle hkResult = null;
			int num = Win32Native.RegOpenKeyEx(hkey, name, 0, GetRegistryKeyAccess(writable), out hkResult);
			if (num == 0 && !hkResult.IsInvalid)
			{
				RegistryKey registryKey = new RegistryKey(hkResult, writable, systemkey: false, remoteKey, isPerfData: false);
				registryKey.checkMode = GetSubKeyPermissonCheck(writable);
				registryKey.keyName = keyName + "\\" + name;
				return registryKey;
			}
			if (num == 5 || num == 1346)
			{
				ThrowHelper.ThrowSecurityException(ExceptionResource.Security_RegistryPermission);
			}
			return null;
		}

		[ComVisible(false)]
		public RegistryKey OpenSubKey(string name, RegistryKeyPermissionCheck permissionCheck)
		{
			ValidateKeyMode(permissionCheck);
			return InternalOpenSubKey(name, permissionCheck, GetRegistryKeyAccess(permissionCheck));
		}

		[ComVisible(false)]
		public RegistryKey OpenSubKey(string name, RegistryKeyPermissionCheck permissionCheck, RegistryRights rights)
		{
			return InternalOpenSubKey(name, permissionCheck, (int)rights);
		}

		private RegistryKey InternalOpenSubKey(string name, RegistryKeyPermissionCheck permissionCheck, int rights)
		{
			ValidateKeyName(name);
			ValidateKeyMode(permissionCheck);
			ValidateKeyRights(rights);
			EnsureNotDisposed();
			name = FixupName(name);
			CheckOpenSubKeyPermission(name, permissionCheck);
			SafeRegistryHandle hkResult = null;
			int num = Win32Native.RegOpenKeyEx(hkey, name, 0, rights, out hkResult);
			if (num == 0 && !hkResult.IsInvalid)
			{
				RegistryKey registryKey = new RegistryKey(hkResult, permissionCheck == RegistryKeyPermissionCheck.ReadWriteSubTree, systemkey: false, remoteKey, isPerfData: false);
				registryKey.keyName = keyName + "\\" + name;
				registryKey.checkMode = permissionCheck;
				return registryKey;
			}
			if (num == 5 || num == 1346)
			{
				ThrowHelper.ThrowSecurityException(ExceptionResource.Security_RegistryPermission);
			}
			return null;
		}

		internal RegistryKey InternalOpenSubKey(string name, bool writable)
		{
			ValidateKeyName(name);
			EnsureNotDisposed();
			int registryKeyAccess = GetRegistryKeyAccess(writable);
			SafeRegistryHandle hkResult = null;
			if (Win32Native.RegOpenKeyEx(hkey, name, 0, registryKeyAccess, out hkResult) == 0 && !hkResult.IsInvalid)
			{
				RegistryKey registryKey = new RegistryKey(hkResult, writable, systemkey: false, remoteKey, isPerfData: false);
				registryKey.keyName = keyName + "\\" + name;
				return registryKey;
			}
			return null;
		}

		public RegistryKey OpenSubKey(string name)
		{
			return OpenSubKey(name, writable: false);
		}

		internal int InternalSubKeyCount()
		{
			EnsureNotDisposed();
			int lpcSubKeys = 0;
			int lpcValues = 0;
			int num = Win32Native.RegQueryInfoKey(hkey, null, null, Win32Native.NULL, ref lpcSubKeys, null, null, ref lpcValues, null, null, null, null);
			if (num != 0)
			{
				Win32Error(num, null);
			}
			return lpcSubKeys;
		}

		public string[] GetSubKeyNames()
		{
			CheckKeyReadPermission();
			return InternalGetSubKeyNames();
		}

		internal string[] InternalGetSubKeyNames()
		{
			EnsureNotDisposed();
			int num = InternalSubKeyCount();
			string[] array = new string[num];
			if (num > 0)
			{
				StringBuilder stringBuilder = new StringBuilder(256);
				for (int i = 0; i < num; i++)
				{
					int lpcbName = stringBuilder.Capacity;
					int num2 = Win32Native.RegEnumKeyEx(hkey, i, stringBuilder, out lpcbName, null, null, null, null);
					if (num2 != 0)
					{
						Win32Error(num2, null);
					}
					array[i] = stringBuilder.ToString();
				}
			}
			return array;
		}

		internal int InternalValueCount()
		{
			EnsureNotDisposed();
			int lpcValues = 0;
			int lpcSubKeys = 0;
			int num = Win32Native.RegQueryInfoKey(hkey, null, null, Win32Native.NULL, ref lpcSubKeys, null, null, ref lpcValues, null, null, null, null);
			if (num != 0)
			{
				Win32Error(num, null);
			}
			return lpcValues;
		}

		public string[] GetValueNames()
		{
			CheckKeyReadPermission();
			EnsureNotDisposed();
			int num = InternalValueCount();
			string[] array = new string[num];
			if (num > 0)
			{
				StringBuilder stringBuilder = new StringBuilder(256);
				for (int i = 0; i < num; i++)
				{
					int lpcbValueName = stringBuilder.Capacity;
					int num2 = Win32Native.RegEnumValue(hkey, i, stringBuilder, ref lpcbValueName, Win32Native.NULL, null, null, null);
					if (num2 == 234 && !IsPerfDataKey() && remoteKey)
					{
						int[] array2 = new int[1];
						byte[] lpData = new byte[5];
						array2[0] = 5;
						num2 = Win32Native.RegEnumValueA(hkey, i, stringBuilder, ref lpcbValueName, Win32Native.NULL, null, lpData, array2);
						if (num2 == 234)
						{
							array2[0] = 0;
							num2 = Win32Native.RegEnumValueA(hkey, i, stringBuilder, ref lpcbValueName, Win32Native.NULL, null, null, array2);
						}
					}
					if (num2 != 0 && (!IsPerfDataKey() || num2 != 234))
					{
						Win32Error(num2, null);
					}
					array[i] = stringBuilder.ToString();
				}
			}
			return array;
		}

		public object GetValue(string name)
		{
			CheckValueReadPermission(name);
			return InternalGetValue(name, null, doNotExpand: false, checkSecurity: true);
		}

		public object GetValue(string name, object defaultValue)
		{
			CheckValueReadPermission(name);
			return InternalGetValue(name, defaultValue, doNotExpand: false, checkSecurity: true);
		}

		[ComVisible(false)]
		public object GetValue(string name, object defaultValue, RegistryValueOptions options)
		{
			if (options < RegistryValueOptions.None || options > RegistryValueOptions.DoNotExpandEnvironmentNames)
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_EnumIllegalVal", (int)options), "options");
			}
			bool doNotExpand = options == RegistryValueOptions.DoNotExpandEnvironmentNames;
			CheckValueReadPermission(name);
			return InternalGetValue(name, defaultValue, doNotExpand, checkSecurity: true);
		}

		internal object InternalGetValue(string name, object defaultValue, bool doNotExpand, bool checkSecurity)
		{
			if (checkSecurity)
			{
				EnsureNotDisposed();
			}
			object obj = defaultValue;
			int lpType = 0;
			int lpcbData = 0;
			int num = Win32Native.RegQueryValueEx(hkey, name, (int[])null, ref lpType, (byte[])null, ref lpcbData);
			if (num != 0)
			{
				if (IsPerfDataKey())
				{
					int num2 = 65000;
					int lpcbData2 = num2;
					byte[] array = new byte[num2];
					int num3;
					while (234 == (num3 = Win32Native.RegQueryValueEx(hkey, name, null, ref lpType, array, ref lpcbData2)))
					{
						num2 *= 2;
						lpcbData2 = num2;
						array = new byte[num2];
					}
					if (num3 != 0)
					{
						Win32Error(num3, name);
					}
					return array;
				}
				if (num != 234)
				{
					return obj;
				}
			}
			switch (lpType)
			{
			case 3:
			case 5:
			{
				byte[] array4 = new byte[lpcbData];
				num = Win32Native.RegQueryValueEx(hkey, name, null, ref lpType, array4, ref lpcbData);
				obj = array4;
				break;
			}
			case 11:
			{
				long lpData = 0L;
				num = Win32Native.RegQueryValueEx(hkey, name, null, ref lpType, ref lpData, ref lpcbData);
				obj = lpData;
				break;
			}
			case 4:
			{
				int lpData2 = 0;
				num = Win32Native.RegQueryValueEx(hkey, name, null, ref lpType, ref lpData2, ref lpcbData);
				obj = lpData2;
				break;
			}
			case 1:
				if (_SystemDefaultCharSize != 1)
				{
					StringBuilder stringBuilder = new StringBuilder(lpcbData / 2);
					num = Win32Native.RegQueryValueEx(hkey, name, null, ref lpType, stringBuilder, ref lpcbData);
					obj = stringBuilder.ToString();
				}
				else
				{
					byte[] array5 = new byte[lpcbData];
					num = Win32Native.RegQueryValueEx(hkey, name, null, ref lpType, array5, ref lpcbData);
					obj = Encoding.Default.GetString(array5, 0, array5.Length - 1);
				}
				break;
			case 2:
				if (_SystemDefaultCharSize != 1)
				{
					StringBuilder stringBuilder2 = new StringBuilder(lpcbData / 2);
					num = Win32Native.RegQueryValueEx(hkey, name, null, ref lpType, stringBuilder2, ref lpcbData);
					obj = ((!doNotExpand) ? Environment.ExpandEnvironmentVariables(stringBuilder2.ToString()) : stringBuilder2.ToString());
				}
				else
				{
					byte[] array6 = new byte[lpcbData];
					num = Win32Native.RegQueryValueEx(hkey, name, null, ref lpType, array6, ref lpcbData);
					string @string = Encoding.Default.GetString(array6, 0, array6.Length - 1);
					obj = ((!doNotExpand) ? Environment.ExpandEnvironmentVariables(@string) : @string);
				}
				break;
			case 7:
			{
				bool flag = _SystemDefaultCharSize != 1;
				IList list = new ArrayList();
				if (flag)
				{
					char[] array2 = new char[lpcbData / 2];
					num = Win32Native.RegQueryValueEx(hkey, name, null, ref lpType, array2, ref lpcbData);
					int num4 = 0;
					int num5 = array2.Length;
					while (num == 0 && num4 < num5)
					{
						int i;
						for (i = num4; i < num5 && array2[i] != 0; i++)
						{
						}
						if (i < num5)
						{
							if (i - num4 > 0)
							{
								list.Add(new string(array2, num4, i - num4));
							}
							else if (i != num5 - 1)
							{
								list.Add(string.Empty);
							}
						}
						else
						{
							list.Add(new string(array2, num4, num5 - num4));
						}
						num4 = i + 1;
					}
				}
				else
				{
					byte[] array3 = new byte[lpcbData];
					num = Win32Native.RegQueryValueEx(hkey, name, null, ref lpType, array3, ref lpcbData);
					int num6 = 0;
					int num7 = array3.Length;
					while (num == 0 && num6 < num7)
					{
						int j;
						for (j = num6; j < num7 && array3[j] != 0; j++)
						{
						}
						if (j < num7)
						{
							if (j - num6 > 0)
							{
								list.Add(Encoding.Default.GetString(array3, num6, j - num6));
							}
							else if (j != num7 - 1)
							{
								list.Add(string.Empty);
							}
						}
						else
						{
							list.Add(Encoding.Default.GetString(array3, num6, num7 - num6));
						}
						num6 = j + 1;
					}
				}
				obj = new string[list.Count];
				list.CopyTo((Array)obj, 0);
				break;
			}
			}
			return obj;
		}

		[ComVisible(false)]
		public RegistryValueKind GetValueKind(string name)
		{
			CheckValueReadPermission(name);
			EnsureNotDisposed();
			int lpType = 0;
			int lpcbData = 0;
			int num = Win32Native.RegQueryValueEx(hkey, name, (int[])null, ref lpType, (byte[])null, ref lpcbData);
			if (num != 0)
			{
				Win32Error(num, null);
			}
			if (!Enum.IsDefined(typeof(RegistryValueKind), lpType))
			{
				return RegistryValueKind.Unknown;
			}
			return (RegistryValueKind)lpType;
		}

		private bool IsDirty()
		{
			return (state & 1) != 0;
		}

		private bool IsSystemKey()
		{
			return (state & 2) != 0;
		}

		private bool IsWritable()
		{
			return (state & 4) != 0;
		}

		private bool IsPerfDataKey()
		{
			return (state & 8) != 0;
		}

		private static bool IsWin9x()
		{
			return (Environment.OSInfo & Environment.OSName.Win9x) != 0;
		}

		private void SetDirty()
		{
			state |= 1;
		}

		public void SetValue(string name, object value)
		{
			SetValue(name, value, RegistryValueKind.Unknown);
		}

		[ComVisible(false)]
		public unsafe void SetValue(string name, object value, RegistryValueKind valueKind)
		{
			if (value == null)
			{
				ThrowHelper.ThrowArgumentNullException(ExceptionArgument.value);
			}
			if (name != null && name.Length > 255)
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_RegKeyStrLenBug"));
			}
			if (!Enum.IsDefined(typeof(RegistryValueKind), valueKind))
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_RegBadKeyKind"), "valueKind");
			}
			EnsureWriteable();
			if (!remoteKey && ContainsRegistryValue(name))
			{
				CheckValueWritePermission(name);
			}
			else
			{
				CheckValueCreatePermission(name);
			}
			if (valueKind == RegistryValueKind.Unknown)
			{
				valueKind = CalculateValueKind(value);
			}
			int num = 0;
			try
			{
				switch (valueKind)
				{
				case RegistryValueKind.String:
				case RegistryValueKind.ExpandString:
				{
					string text = value.ToString();
					if (_SystemDefaultCharSize == 1)
					{
						byte[] bytes2 = Encoding.Default.GetBytes(text);
						byte[] array4 = new byte[bytes2.Length + 1];
						Array.Copy(bytes2, 0, array4, 0, bytes2.Length);
						num = Win32Native.RegSetValueEx(hkey, name, 0, valueKind, array4, array4.Length);
					}
					else
					{
						num = Win32Native.RegSetValueEx(hkey, name, 0, valueKind, text, text.Length * 2 + 2);
					}
					break;
				}
				case RegistryValueKind.MultiString:
				{
					string[] array2 = (string[])((string[])value).Clone();
					bool flag = _SystemDefaultCharSize != 1;
					int num2 = 0;
					if (flag)
					{
						for (int i = 0; i < array2.Length; i++)
						{
							if (array2[i] == null)
							{
								ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_RegSetStrArrNull);
							}
							num2 += (array2[i].Length + 1) * 2;
						}
						num2 += 2;
					}
					else
					{
						for (int j = 0; j < array2.Length; j++)
						{
							if (array2[j] == null)
							{
								ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_RegSetStrArrNull);
							}
							num2 += Encoding.Default.GetByteCount(array2[j]) + 1;
						}
						num2++;
					}
					byte[] array3 = new byte[num2];
					try
					{
						fixed (byte* value2 = array3)
						{
							IntPtr intPtr = new IntPtr(value2);
							for (int k = 0; k < array2.Length; k++)
							{
								if (flag)
								{
									string.InternalCopy(array2[k], intPtr, array2[k].Length * 2);
									intPtr = new IntPtr((long)intPtr + array2[k].Length * 2);
									*(short*)intPtr.ToPointer() = 0;
									intPtr = new IntPtr((long)intPtr + 2);
								}
								else
								{
									byte[] bytes = Encoding.Default.GetBytes(array2[k]);
									Buffer.memcpy(bytes, 0, (byte*)intPtr.ToPointer(), 0, bytes.Length);
									intPtr = new IntPtr((long)intPtr + bytes.Length);
									*(sbyte*)intPtr.ToPointer() = 0;
									intPtr = new IntPtr((long)intPtr + 1);
								}
							}
							if (flag)
							{
								*(short*)intPtr.ToPointer() = 0;
								intPtr = new IntPtr((long)intPtr + 2);
							}
							else
							{
								*(sbyte*)intPtr.ToPointer() = 0;
								intPtr = new IntPtr((long)intPtr + 1);
							}
							num = Win32Native.RegSetValueEx(hkey, name, 0, RegistryValueKind.MultiString, array3, num2);
						}
					}
					finally
					{
					}
					break;
				}
				case RegistryValueKind.Binary:
				{
					byte[] array = (byte[])value;
					num = Win32Native.RegSetValueEx(hkey, name, 0, RegistryValueKind.Binary, array, array.Length);
					break;
				}
				case RegistryValueKind.DWord:
				{
					int lpData2 = Convert.ToInt32(value, CultureInfo.InvariantCulture);
					num = Win32Native.RegSetValueEx(hkey, name, 0, RegistryValueKind.DWord, ref lpData2, 4);
					break;
				}
				case RegistryValueKind.QWord:
				{
					long lpData = Convert.ToInt64(value, CultureInfo.InvariantCulture);
					num = Win32Native.RegSetValueEx(hkey, name, 0, RegistryValueKind.QWord, ref lpData, 8);
					break;
				}
				case (RegistryValueKind)5:
				case (RegistryValueKind)6:
				case (RegistryValueKind)8:
				case (RegistryValueKind)9:
				case (RegistryValueKind)10:
					break;
				}
			}
			catch (OverflowException)
			{
				ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_RegSetMismatchedKind);
			}
			catch (InvalidOperationException)
			{
				ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_RegSetMismatchedKind);
			}
			catch (FormatException)
			{
				ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_RegSetMismatchedKind);
			}
			catch (InvalidCastException)
			{
				ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_RegSetMismatchedKind);
			}
			if (num == 0)
			{
				SetDirty();
			}
			else
			{
				Win32Error(num, null);
			}
		}

		private RegistryValueKind CalculateValueKind(object value)
		{
			if (value is int)
			{
				return RegistryValueKind.DWord;
			}
			if (value is Array)
			{
				if (value is byte[])
				{
					return RegistryValueKind.Binary;
				}
				if (value is string[])
				{
					return RegistryValueKind.MultiString;
				}
				throw new ArgumentException(Environment.GetResourceString("Arg_RegSetBadArrType", value.GetType().Name));
			}
			return RegistryValueKind.String;
		}

		public override string ToString()
		{
			EnsureNotDisposed();
			return keyName;
		}

		public RegistrySecurity GetAccessControl()
		{
			return GetAccessControl(AccessControlSections.Access | AccessControlSections.Owner | AccessControlSections.Group);
		}

		public RegistrySecurity GetAccessControl(AccessControlSections includeSections)
		{
			EnsureNotDisposed();
			return new RegistrySecurity(hkey, keyName, includeSections);
		}

		public void SetAccessControl(RegistrySecurity registrySecurity)
		{
			EnsureWriteable();
			if (registrySecurity == null)
			{
				throw new ArgumentNullException("registrySecurity");
			}
			registrySecurity.Persist(hkey, keyName);
		}

		internal void Win32Error(int errorCode, string str)
		{
			switch (errorCode)
			{
			case 5:
				if (str != null)
				{
					throw new UnauthorizedAccessException(Environment.GetResourceString("UnauthorizedAccess_RegistryKeyGeneric_Key", str));
				}
				throw new UnauthorizedAccessException();
			case 6:
				hkey.SetHandleAsInvalid();
				hkey = null;
				break;
			case 234:
				if (remoteKey)
				{
					return;
				}
				break;
			case 2:
				throw new IOException(Environment.GetResourceString("Arg_RegKeyNotFound"), errorCode);
			}
			throw new IOException(Win32Native.GetMessage(errorCode), errorCode);
		}

		internal static void Win32ErrorStatic(int errorCode, string str)
		{
			if (errorCode == 5)
			{
				if (str != null)
				{
					throw new UnauthorizedAccessException(Environment.GetResourceString("UnauthorizedAccess_RegistryKeyGeneric_Key", str));
				}
				throw new UnauthorizedAccessException();
			}
			throw new IOException(Win32Native.GetMessage(errorCode), errorCode);
		}

		internal static string FixupName(string name)
		{
			if (name.IndexOf('\\') == -1)
			{
				return name;
			}
			StringBuilder stringBuilder = new StringBuilder(name);
			FixupPath(stringBuilder);
			int num = stringBuilder.Length - 1;
			if (stringBuilder[num] == '\\')
			{
				stringBuilder.Length = num;
			}
			return stringBuilder.ToString();
		}

		private static void FixupPath(StringBuilder path)
		{
			int length = path.Length;
			bool flag = false;
			char c = '\uffff';
			int i;
			for (i = 1; i < length - 1; i++)
			{
				if (path[i] == '\\')
				{
					i++;
					while (i < length && path[i] == '\\')
					{
						path[i] = c;
						i++;
						flag = true;
					}
				}
			}
			if (!flag)
			{
				return;
			}
			i = 0;
			int num = 0;
			while (i < length)
			{
				if (path[i] == c)
				{
					i++;
					continue;
				}
				path[num] = path[i];
				i++;
				num++;
			}
			path.Length += num - i;
		}

		private void CheckOpenSubKeyPermission(string subkeyName, bool subKeyWritable)
		{
			if (checkMode == RegistryKeyPermissionCheck.Default)
			{
				CheckSubKeyReadPermission(subkeyName);
			}
			if (subKeyWritable && checkMode == RegistryKeyPermissionCheck.ReadSubTree)
			{
				CheckSubTreeReadWritePermission(subkeyName);
			}
		}

		private void CheckOpenSubKeyPermission(string subkeyName, RegistryKeyPermissionCheck subKeyCheck)
		{
			if (subKeyCheck == RegistryKeyPermissionCheck.Default && checkMode == RegistryKeyPermissionCheck.Default)
			{
				CheckSubKeyReadPermission(subkeyName);
			}
			CheckSubTreePermission(subkeyName, subKeyCheck);
		}

		private void CheckSubTreePermission(string subkeyName, RegistryKeyPermissionCheck subKeyCheck)
		{
			switch (subKeyCheck)
			{
			case RegistryKeyPermissionCheck.ReadSubTree:
				if (checkMode == RegistryKeyPermissionCheck.Default)
				{
					CheckSubTreeReadPermission(subkeyName);
				}
				break;
			case RegistryKeyPermissionCheck.ReadWriteSubTree:
				if (checkMode != RegistryKeyPermissionCheck.ReadWriteSubTree)
				{
					CheckSubTreeReadWritePermission(subkeyName);
				}
				break;
			}
		}

		private void CheckSubKeyWritePermission(string subkeyName)
		{
			if (remoteKey)
			{
				CheckUnmanagedCodePermission();
			}
			else if (checkMode == RegistryKeyPermissionCheck.Default)
			{
				new RegistryPermission(RegistryPermissionAccess.Write, keyName + "\\" + subkeyName + "\\.").Demand();
			}
		}

		private void CheckSubKeyReadPermission(string subkeyName)
		{
			if (remoteKey)
			{
				CheckUnmanagedCodePermission();
			}
			else
			{
				new RegistryPermission(RegistryPermissionAccess.Read, keyName + "\\" + subkeyName + "\\.").Demand();
			}
		}

		private void CheckSubKeyCreatePermission(string subkeyName)
		{
			if (remoteKey)
			{
				CheckUnmanagedCodePermission();
			}
			else if (checkMode == RegistryKeyPermissionCheck.Default)
			{
				new RegistryPermission(RegistryPermissionAccess.Create, keyName + "\\" + subkeyName + "\\.").Demand();
			}
		}

		private void CheckSubTreeReadPermission(string subkeyName)
		{
			if (remoteKey)
			{
				CheckUnmanagedCodePermission();
			}
			else if (checkMode == RegistryKeyPermissionCheck.Default)
			{
				new RegistryPermission(RegistryPermissionAccess.Read, keyName + "\\" + subkeyName + "\\").Demand();
			}
		}

		private void CheckSubTreeWritePermission(string subkeyName)
		{
			if (remoteKey)
			{
				CheckUnmanagedCodePermission();
			}
			else if (checkMode == RegistryKeyPermissionCheck.Default)
			{
				new RegistryPermission(RegistryPermissionAccess.Write, keyName + "\\" + subkeyName + "\\").Demand();
			}
		}

		private void CheckSubTreeReadWritePermission(string subkeyName)
		{
			if (remoteKey)
			{
				CheckUnmanagedCodePermission();
			}
			else
			{
				new RegistryPermission(RegistryPermissionAccess.Read | RegistryPermissionAccess.Write, keyName + "\\" + subkeyName).Demand();
			}
		}

		private static void CheckUnmanagedCodePermission()
		{
			new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Demand();
		}

		private void CheckValueWritePermission(string valueName)
		{
			if (remoteKey)
			{
				new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Demand();
			}
			else if (checkMode == RegistryKeyPermissionCheck.Default)
			{
				new RegistryPermission(RegistryPermissionAccess.Write, keyName + "\\" + valueName).Demand();
			}
		}

		private void CheckValueCreatePermission(string valueName)
		{
			if (remoteKey)
			{
				new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Demand();
			}
			else if (checkMode == RegistryKeyPermissionCheck.Default)
			{
				new RegistryPermission(RegistryPermissionAccess.Create, keyName + "\\" + valueName).Demand();
			}
		}

		private void CheckValueReadPermission(string valueName)
		{
			if (checkMode == RegistryKeyPermissionCheck.Default)
			{
				new RegistryPermission(RegistryPermissionAccess.Read, keyName + "\\" + valueName).Demand();
			}
		}

		private void CheckKeyReadPermission()
		{
			if (checkMode == RegistryKeyPermissionCheck.Default)
			{
				new RegistryPermission(RegistryPermissionAccess.Read, keyName + "\\.").Demand();
			}
		}

		private bool ContainsRegistryValue(string name)
		{
			int lpType = 0;
			int lpcbData = 0;
			int num = Win32Native.RegQueryValueEx(hkey, name, (int[])null, ref lpType, (byte[])null, ref lpcbData);
			return num == 0;
		}

		private void EnsureNotDisposed()
		{
			if (hkey == null)
			{
				ThrowHelper.ThrowObjectDisposedException(keyName, ExceptionResource.ObjectDisposed_RegKeyClosed);
			}
		}

		private void EnsureWriteable()
		{
			EnsureNotDisposed();
			if (!IsWritable())
			{
				ThrowHelper.ThrowUnauthorizedAccessException(ExceptionResource.UnauthorizedAccess_RegistryNoWrite);
			}
		}

		private static int GetRegistryKeyAccess(bool isWritable)
		{
			if (!isWritable)
			{
				return 131097;
			}
			return 131103;
		}

		private static int GetRegistryKeyAccess(RegistryKeyPermissionCheck mode)
		{
			int result = 0;
			switch (mode)
			{
			case RegistryKeyPermissionCheck.Default:
			case RegistryKeyPermissionCheck.ReadSubTree:
				result = 131097;
				break;
			case RegistryKeyPermissionCheck.ReadWriteSubTree:
				result = 131103;
				break;
			}
			return result;
		}

		private RegistryKeyPermissionCheck GetSubKeyPermissonCheck(bool subkeyWritable)
		{
			if (checkMode == RegistryKeyPermissionCheck.Default)
			{
				return checkMode;
			}
			if (subkeyWritable)
			{
				return RegistryKeyPermissionCheck.ReadWriteSubTree;
			}
			return RegistryKeyPermissionCheck.ReadSubTree;
		}

		private static void ValidateKeyName(string name)
		{
			if (name == null)
			{
				ThrowHelper.ThrowArgumentNullException(ExceptionArgument.name);
			}
			int num = name.IndexOf("\\", StringComparison.OrdinalIgnoreCase);
			int num2 = 0;
			while (num != -1)
			{
				if (num - num2 > 255)
				{
					ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_RegKeyStrLenBug);
				}
				num2 = num + 1;
				num = name.IndexOf("\\", num2, StringComparison.OrdinalIgnoreCase);
			}
			if (name.Length - num2 > 255)
			{
				ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_RegKeyStrLenBug);
			}
		}

		private static void ValidateKeyMode(RegistryKeyPermissionCheck mode)
		{
			if (mode < RegistryKeyPermissionCheck.Default || mode > RegistryKeyPermissionCheck.ReadWriteSubTree)
			{
				ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_InvalidRegistryKeyPermissionCheck, ExceptionArgument.mode);
			}
		}

		private static void ValidateKeyRights(int rights)
		{
			if (((uint)rights & 0xFFF0FFC0u) != 0)
			{
				ThrowHelper.ThrowSecurityException(ExceptionResource.Security_RegistryPermission);
			}
		}
	}
}
