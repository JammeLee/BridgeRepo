using System.Collections;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Security.Permissions;
using System.Security.Policy;
using System.Text;
using System.Threading;
using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;

namespace System.IO.IsolatedStorage
{
	[ComVisible(true)]
	public sealed class IsolatedStorageFile : IsolatedStorage, IDisposable
	{
		private const int s_BlockSize = 1024;

		private const int s_DirSize = 1024;

		private const string s_name = "file.store";

		internal const string s_Files = "Files";

		internal const string s_AssemFiles = "AssemFiles";

		internal const string s_AppFiles = "AppFiles";

		internal const string s_IDFile = "identity.dat";

		internal const string s_InfoFile = "info.dat";

		internal const string s_AppInfoFile = "appinfo.dat";

		private static string s_RootDirUser;

		private static string s_RootDirMachine;

		private static string s_RootDirRoaming;

		private static string s_appDataDir;

		private static FileIOPermission s_PermUser;

		private static FileIOPermission s_PermMachine;

		private static FileIOPermission s_PermRoaming;

		private static IsolatedStorageFilePermission s_PermAdminUser;

		private FileIOPermission m_fiop;

		private string m_RootDir;

		private string m_InfoFile;

		private string m_SyncObjectName;

		private IntPtr m_handle;

		private bool m_closed;

		private bool m_bDisposed;

		[CLSCompliant(false)]
		public override ulong CurrentSize
		{
			get
			{
				if (IsRoaming())
				{
					throw new InvalidOperationException(Environment.GetResourceString("IsolatedStorage_CurrentSizeUndefined"));
				}
				lock (this)
				{
					if (m_bDisposed)
					{
						throw new ObjectDisposedException(null, Environment.GetResourceString("IsolatedStorage_StoreNotOpen"));
					}
					if (m_closed)
					{
						throw new InvalidOperationException(Environment.GetResourceString("IsolatedStorage_StoreNotOpen"));
					}
					if (m_handle == Win32Native.NULL)
					{
						m_handle = nOpen(m_InfoFile, GetSyncObjectName());
					}
					return nGetUsage(m_handle);
				}
			}
		}

		[CLSCompliant(false)]
		public override ulong MaximumSize
		{
			get
			{
				if (IsRoaming())
				{
					return 9223372036854775807uL;
				}
				return base.MaximumSize;
			}
		}

		internal string RootDirectory => m_RootDir;

		internal IsolatedStorageFile()
		{
		}

		public static IsolatedStorageFile GetUserStoreForDomain()
		{
			return GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Domain | IsolatedStorageScope.Assembly, null, null);
		}

		public static IsolatedStorageFile GetUserStoreForAssembly()
		{
			return GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null);
		}

		public static IsolatedStorageFile GetUserStoreForApplication()
		{
			return GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Application, null);
		}

		public static IsolatedStorageFile GetMachineStoreForDomain()
		{
			return GetStore(IsolatedStorageScope.Domain | IsolatedStorageScope.Assembly | IsolatedStorageScope.Machine, null, null);
		}

		public static IsolatedStorageFile GetMachineStoreForAssembly()
		{
			return GetStore(IsolatedStorageScope.Assembly | IsolatedStorageScope.Machine, null, null);
		}

		public static IsolatedStorageFile GetMachineStoreForApplication()
		{
			return GetStore(IsolatedStorageScope.Machine | IsolatedStorageScope.Application, null);
		}

		public static IsolatedStorageFile GetStore(IsolatedStorageScope scope, Type domainEvidenceType, Type assemblyEvidenceType)
		{
			if (domainEvidenceType != null)
			{
				DemandAdminPermission();
			}
			IsolatedStorageFile isolatedStorageFile = new IsolatedStorageFile();
			isolatedStorageFile.InitStore(scope, domainEvidenceType, assemblyEvidenceType);
			isolatedStorageFile.Init(scope);
			return isolatedStorageFile;
		}

		public static IsolatedStorageFile GetStore(IsolatedStorageScope scope, object domainIdentity, object assemblyIdentity)
		{
			if (IsolatedStorage.IsDomain(scope) && domainIdentity == null)
			{
				throw new ArgumentNullException("domainIdentity");
			}
			if (assemblyIdentity == null)
			{
				throw new ArgumentNullException("assemblyIdentity");
			}
			DemandAdminPermission();
			IsolatedStorageFile isolatedStorageFile = new IsolatedStorageFile();
			isolatedStorageFile.InitStore(scope, domainIdentity, assemblyIdentity, null);
			isolatedStorageFile.Init(scope);
			return isolatedStorageFile;
		}

		public static IsolatedStorageFile GetStore(IsolatedStorageScope scope, Evidence domainEvidence, Type domainEvidenceType, Evidence assemblyEvidence, Type assemblyEvidenceType)
		{
			if (IsolatedStorage.IsDomain(scope) && domainEvidence == null)
			{
				throw new ArgumentNullException("domainEvidence");
			}
			if (assemblyEvidence == null)
			{
				throw new ArgumentNullException("assemblyEvidence");
			}
			DemandAdminPermission();
			IsolatedStorageFile isolatedStorageFile = new IsolatedStorageFile();
			isolatedStorageFile.InitStore(scope, domainEvidence, domainEvidenceType, assemblyEvidence, assemblyEvidenceType, null, null);
			isolatedStorageFile.Init(scope);
			return isolatedStorageFile;
		}

		public static IsolatedStorageFile GetStore(IsolatedStorageScope scope, Type applicationEvidenceType)
		{
			if (applicationEvidenceType != null)
			{
				DemandAdminPermission();
			}
			IsolatedStorageFile isolatedStorageFile = new IsolatedStorageFile();
			isolatedStorageFile.InitStore(scope, applicationEvidenceType);
			isolatedStorageFile.Init(scope);
			return isolatedStorageFile;
		}

		public static IsolatedStorageFile GetStore(IsolatedStorageScope scope, object applicationIdentity)
		{
			if (applicationIdentity == null)
			{
				throw new ArgumentNullException("applicationIdentity");
			}
			DemandAdminPermission();
			IsolatedStorageFile isolatedStorageFile = new IsolatedStorageFile();
			isolatedStorageFile.InitStore(scope, null, null, applicationIdentity);
			isolatedStorageFile.Init(scope);
			return isolatedStorageFile;
		}

		internal unsafe void Reserve(ulong lReserve)
		{
			if (IsRoaming())
			{
				return;
			}
			ulong maximumSize = MaximumSize;
			ulong num = lReserve;
			lock (this)
			{
				if (m_bDisposed)
				{
					throw new ObjectDisposedException(null, Environment.GetResourceString("IsolatedStorage_StoreNotOpen"));
				}
				if (m_closed)
				{
					throw new InvalidOperationException(Environment.GetResourceString("IsolatedStorage_StoreNotOpen"));
				}
				if (m_handle == Win32Native.NULL)
				{
					m_handle = nOpen(m_InfoFile, GetSyncObjectName());
				}
				nReserve(m_handle, &maximumSize, &num, fFree: false);
			}
		}

		internal unsafe void Unreserve(ulong lFree)
		{
			if (IsRoaming())
			{
				return;
			}
			ulong maximumSize = MaximumSize;
			ulong num = lFree;
			lock (this)
			{
				if (m_bDisposed)
				{
					throw new ObjectDisposedException(null, Environment.GetResourceString("IsolatedStorage_StoreNotOpen"));
				}
				if (m_closed)
				{
					throw new InvalidOperationException(Environment.GetResourceString("IsolatedStorage_StoreNotOpen"));
				}
				if (m_handle == Win32Native.NULL)
				{
					m_handle = nOpen(m_InfoFile, GetSyncObjectName());
				}
				nReserve(m_handle, &maximumSize, &num, fFree: true);
			}
		}

		public void DeleteFile(string file)
		{
			if (file == null)
			{
				throw new ArgumentNullException("file");
			}
			m_fiop.Assert();
			m_fiop.PermitOnly();
			FileInfo fileInfo = new FileInfo(GetFullPath(file));
			long num = 0L;
			Lock();
			try
			{
				try
				{
					num = fileInfo.Length;
					fileInfo.Delete();
				}
				catch
				{
					throw new IsolatedStorageException(Environment.GetResourceString("IsolatedStorage_DeleteFile"));
				}
				Unreserve(RoundToBlockSize((ulong)num));
			}
			finally
			{
				Unlock();
			}
			CodeAccessPermission.RevertAll();
		}

		public void CreateDirectory(string dir)
		{
			if (dir == null)
			{
				throw new ArgumentNullException("dir");
			}
			string fullPath = GetFullPath(dir);
			string fullPathInternal = Path.GetFullPathInternal(fullPath);
			string[] array = DirectoriesToCreate(fullPathInternal);
			if (array == null || array.Length == 0)
			{
				if (!Directory.Exists(fullPath))
				{
					throw new IsolatedStorageException(Environment.GetResourceString("IsolatedStorage_CreateDirectory"));
				}
				return;
			}
			Reserve(1024uL * (ulong)array.Length);
			m_fiop.Assert();
			m_fiop.PermitOnly();
			try
			{
				Directory.CreateDirectory(array[array.Length - 1]);
			}
			catch
			{
				Unreserve(1024uL * (ulong)array.Length);
				Directory.Delete(array[0], recursive: true);
				throw new IsolatedStorageException(Environment.GetResourceString("IsolatedStorage_CreateDirectory"));
			}
			CodeAccessPermission.RevertAll();
		}

		private string[] DirectoriesToCreate(string fullPath)
		{
			ArrayList arrayList = new ArrayList();
			int num = fullPath.Length;
			if (num >= 2 && fullPath[num - 1] == SeparatorExternal)
			{
				num--;
			}
			int i = Path.GetRootLength(fullPath);
			while (i < num)
			{
				for (i++; i < num && fullPath[i] != SeparatorExternal; i++)
				{
				}
				string text = fullPath.Substring(0, i);
				if (!Directory.InternalExists(text))
				{
					arrayList.Add(text);
				}
			}
			if (arrayList.Count != 0)
			{
				return (string[])arrayList.ToArray(typeof(string));
			}
			return null;
		}

		public void DeleteDirectory(string dir)
		{
			if (dir == null)
			{
				throw new ArgumentNullException("dir");
			}
			m_fiop.Assert();
			m_fiop.PermitOnly();
			Lock();
			try
			{
				try
				{
					new DirectoryInfo(GetFullPath(dir)).Delete(recursive: false);
				}
				catch
				{
					throw new IsolatedStorageException(Environment.GetResourceString("IsolatedStorage_DeleteDirectory"));
				}
				Unreserve(1024uL);
			}
			finally
			{
				Unlock();
			}
			CodeAccessPermission.RevertAll();
		}

		public string[] GetFileNames(string searchPattern)
		{
			if (searchPattern == null)
			{
				throw new ArgumentNullException("searchPattern");
			}
			m_fiop.Assert();
			m_fiop.PermitOnly();
			string[] fileDirectoryNames = GetFileDirectoryNames(GetFullPath(searchPattern), searchPattern, file: true);
			CodeAccessPermission.RevertAll();
			return fileDirectoryNames;
		}

		public string[] GetDirectoryNames(string searchPattern)
		{
			if (searchPattern == null)
			{
				throw new ArgumentNullException("searchPattern");
			}
			m_fiop.Assert();
			m_fiop.PermitOnly();
			string[] fileDirectoryNames = GetFileDirectoryNames(GetFullPath(searchPattern), searchPattern, file: false);
			CodeAccessPermission.RevertAll();
			return fileDirectoryNames;
		}

		public override void Remove()
		{
			string text = null;
			RemoveLogicalDir();
			Close();
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(GetRootDir(base.Scope));
			if (IsApp())
			{
				stringBuilder.Append(base.AppName);
				stringBuilder.Append(SeparatorExternal);
			}
			else
			{
				if (IsDomain())
				{
					stringBuilder.Append(base.DomainName);
					stringBuilder.Append(SeparatorExternal);
					text = stringBuilder.ToString();
				}
				stringBuilder.Append(base.AssemName);
				stringBuilder.Append(SeparatorExternal);
			}
			string text2 = stringBuilder.ToString();
			new FileIOPermission(FileIOPermissionAccess.AllAccess, text2).Assert();
			if (ContainsUnknownFiles(text2))
			{
				return;
			}
			try
			{
				Directory.Delete(text2, recursive: true);
			}
			catch
			{
				return;
			}
			if (!IsDomain())
			{
				return;
			}
			CodeAccessPermission.RevertAssert();
			new FileIOPermission(FileIOPermissionAccess.AllAccess, text).Assert();
			if (!ContainsUnknownFiles(text))
			{
				try
				{
					Directory.Delete(text, recursive: true);
				}
				catch
				{
				}
			}
		}

		private void RemoveLogicalDir()
		{
			m_fiop.Assert();
			Lock();
			try
			{
				ulong lFree = (IsRoaming() ? 0 : CurrentSize);
				try
				{
					Directory.Delete(RootDirectory, recursive: true);
				}
				catch
				{
					throw new IsolatedStorageException(Environment.GetResourceString("IsolatedStorage_DeleteDirectories"));
				}
				Unreserve(lFree);
			}
			finally
			{
				Unlock();
			}
		}

		private bool ContainsUnknownFiles(string rootDir)
		{
			string[] fileDirectoryNames;
			string[] fileDirectoryNames2;
			try
			{
				fileDirectoryNames = GetFileDirectoryNames(rootDir + "*", "*", file: true);
				fileDirectoryNames2 = GetFileDirectoryNames(rootDir + "*", "*", file: false);
			}
			catch
			{
				throw new IsolatedStorageException(Environment.GetResourceString("IsolatedStorage_DeleteDirectories"));
			}
			if (fileDirectoryNames2 != null && fileDirectoryNames2.Length > 0)
			{
				if (fileDirectoryNames2.Length > 1)
				{
					return true;
				}
				if (IsApp())
				{
					if (NotAppFilesDir(fileDirectoryNames2[0]))
					{
						return true;
					}
				}
				else if (IsDomain())
				{
					if (NotFilesDir(fileDirectoryNames2[0]))
					{
						return true;
					}
				}
				else if (NotAssemFilesDir(fileDirectoryNames2[0]))
				{
					return true;
				}
			}
			if (fileDirectoryNames == null || fileDirectoryNames.Length == 0)
			{
				return false;
			}
			if (IsRoaming())
			{
				if (fileDirectoryNames.Length > 1 || NotIDFile(fileDirectoryNames[0]))
				{
					return true;
				}
				return false;
			}
			if (fileDirectoryNames.Length > 2 || (NotIDFile(fileDirectoryNames[0]) && NotInfoFile(fileDirectoryNames[0])) || (fileDirectoryNames.Length == 2 && NotIDFile(fileDirectoryNames[1]) && NotInfoFile(fileDirectoryNames[1])))
			{
				return true;
			}
			return false;
		}

		public void Close()
		{
			if (IsRoaming())
			{
				return;
			}
			lock (this)
			{
				if (!m_closed)
				{
					m_closed = true;
					IntPtr handle = m_handle;
					m_handle = Win32Native.NULL;
					nClose(handle);
					GC.nativeSuppressFinalize(this);
				}
			}
		}

		public void Dispose()
		{
			Close();
			m_bDisposed = true;
		}

		~IsolatedStorageFile()
		{
			Dispose();
		}

		private static bool NotIDFile(string file)
		{
			return string.Compare(file, "identity.dat", StringComparison.Ordinal) != 0;
		}

		private static bool NotInfoFile(string file)
		{
			if (string.Compare(file, "info.dat", StringComparison.Ordinal) != 0)
			{
				return string.Compare(file, "appinfo.dat", StringComparison.Ordinal) != 0;
			}
			return false;
		}

		private static bool NotFilesDir(string dir)
		{
			return string.Compare(dir, "Files", StringComparison.Ordinal) != 0;
		}

		internal static bool NotAssemFilesDir(string dir)
		{
			return string.Compare(dir, "AssemFiles", StringComparison.Ordinal) != 0;
		}

		internal static bool NotAppFilesDir(string dir)
		{
			return string.Compare(dir, "AppFiles", StringComparison.Ordinal) != 0;
		}

		public static void Remove(IsolatedStorageScope scope)
		{
			VerifyGlobalScope(scope);
			DemandAdminPermission();
			string rootDir = GetRootDir(scope);
			new FileIOPermission(FileIOPermissionAccess.Write, rootDir).Assert();
			try
			{
				Directory.Delete(rootDir, recursive: true);
				Directory.CreateDirectory(rootDir);
			}
			catch
			{
				throw new IsolatedStorageException(Environment.GetResourceString("IsolatedStorage_DeleteDirectories"));
			}
		}

		public static IEnumerator GetEnumerator(IsolatedStorageScope scope)
		{
			VerifyGlobalScope(scope);
			DemandAdminPermission();
			return new IsolatedStorageFileEnumerator(scope);
		}

		internal string GetFullPath(string path)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(RootDirectory);
			if (path[0] == SeparatorExternal)
			{
				stringBuilder.Append(path.Substring(1));
			}
			else
			{
				stringBuilder.Append(path);
			}
			return stringBuilder.ToString();
		}

		[SecurityPermission(SecurityAction.Assert, Flags = SecurityPermissionFlag.UnmanagedCode)]
		private static string GetDataDirectoryFromActivationContext()
		{
			if (s_appDataDir == null)
			{
				ActivationContext activationContext = AppDomain.CurrentDomain.ActivationContext;
				if (activationContext == null)
				{
					throw new IsolatedStorageException(Environment.GetResourceString("IsolatedStorage_ApplicationMissingIdentity"));
				}
				string text = activationContext.DataDirectory;
				if (text != null && text[text.Length - 1] != '\\')
				{
					text += "\\";
				}
				s_appDataDir = text;
			}
			return s_appDataDir;
		}

		internal void Init(IsolatedStorageScope scope)
		{
			GetGlobalFileIOPerm(scope).Assert();
			StringBuilder stringBuilder = new StringBuilder();
			if (IsolatedStorage.IsApp(scope))
			{
				stringBuilder.Append(GetRootDir(scope));
				if (s_appDataDir == null)
				{
					stringBuilder.Append(base.AppName);
					stringBuilder.Append(SeparatorExternal);
				}
				try
				{
					Directory.CreateDirectory(stringBuilder.ToString());
				}
				catch
				{
				}
				CreateIDFile(stringBuilder.ToString(), scope);
				m_InfoFile = stringBuilder.ToString() + "appinfo.dat";
				stringBuilder.Append("AppFiles");
			}
			else
			{
				stringBuilder.Append(GetRootDir(scope));
				if (IsolatedStorage.IsDomain(scope))
				{
					stringBuilder.Append(base.DomainName);
					stringBuilder.Append(SeparatorExternal);
					try
					{
						Directory.CreateDirectory(stringBuilder.ToString());
						CreateIDFile(stringBuilder.ToString(), scope);
					}
					catch
					{
					}
					m_InfoFile = stringBuilder.ToString() + "info.dat";
				}
				stringBuilder.Append(base.AssemName);
				stringBuilder.Append(SeparatorExternal);
				try
				{
					Directory.CreateDirectory(stringBuilder.ToString());
					CreateIDFile(stringBuilder.ToString(), scope);
				}
				catch
				{
				}
				if (IsolatedStorage.IsDomain(scope))
				{
					stringBuilder.Append("Files");
				}
				else
				{
					m_InfoFile = stringBuilder.ToString() + "info.dat";
					stringBuilder.Append("AssemFiles");
				}
			}
			stringBuilder.Append(SeparatorExternal);
			string text = stringBuilder.ToString();
			try
			{
				Directory.CreateDirectory(text);
			}
			catch
			{
			}
			m_RootDir = text;
			m_fiop = new FileIOPermission(FileIOPermissionAccess.AllAccess, text);
		}

		internal bool InitExistingStore(IsolatedStorageScope scope)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(GetRootDir(scope));
			if (IsolatedStorage.IsApp(scope))
			{
				stringBuilder.Append(base.AppName);
				stringBuilder.Append(SeparatorExternal);
				m_InfoFile = stringBuilder.ToString() + "appinfo.dat";
				stringBuilder.Append("AppFiles");
			}
			else
			{
				if (IsolatedStorage.IsDomain(scope))
				{
					stringBuilder.Append(base.DomainName);
					stringBuilder.Append(SeparatorExternal);
					m_InfoFile = stringBuilder.ToString() + "info.dat";
				}
				stringBuilder.Append(base.AssemName);
				stringBuilder.Append(SeparatorExternal);
				if (IsolatedStorage.IsDomain(scope))
				{
					stringBuilder.Append("Files");
				}
				else
				{
					m_InfoFile = stringBuilder.ToString() + "info.dat";
					stringBuilder.Append("AssemFiles");
				}
			}
			stringBuilder.Append(SeparatorExternal);
			FileIOPermission fileIOPermission = new FileIOPermission(FileIOPermissionAccess.AllAccess, stringBuilder.ToString());
			fileIOPermission.Assert();
			if (!Directory.Exists(stringBuilder.ToString()))
			{
				return false;
			}
			m_RootDir = stringBuilder.ToString();
			m_fiop = fileIOPermission;
			return true;
		}

		protected override IsolatedStoragePermission GetPermission(PermissionSet ps)
		{
			if (ps == null)
			{
				return null;
			}
			if (ps.IsUnrestricted())
			{
				return new IsolatedStorageFilePermission(PermissionState.Unrestricted);
			}
			return (IsolatedStoragePermission)ps.GetPermission(typeof(IsolatedStorageFilePermission));
		}

		internal void UndoReserveOperation(ulong oldLen, ulong newLen)
		{
			oldLen = RoundToBlockSize(oldLen);
			if (newLen > oldLen)
			{
				Unreserve(RoundToBlockSize(newLen - oldLen));
			}
		}

		internal void Reserve(ulong oldLen, ulong newLen)
		{
			oldLen = RoundToBlockSize(oldLen);
			if (newLen > oldLen)
			{
				Reserve(RoundToBlockSize(newLen - oldLen));
			}
		}

		internal void ReserveOneBlock()
		{
			Reserve(1024uL);
		}

		internal void UnreserveOneBlock()
		{
			Unreserve(1024uL);
		}

		internal static ulong RoundToBlockSize(ulong num)
		{
			if (num < 1024)
			{
				return 1024uL;
			}
			ulong num2 = num % 1024uL;
			if (num2 != 0)
			{
				num += 1024 - num2;
			}
			return num;
		}

		internal static string GetRootDir(IsolatedStorageScope scope)
		{
			if (IsolatedStorage.IsRoaming(scope))
			{
				if (s_RootDirRoaming == null)
				{
					s_RootDirRoaming = nGetRootDir(scope);
				}
				return s_RootDirRoaming;
			}
			if (IsolatedStorage.IsMachine(scope))
			{
				if (s_RootDirMachine == null)
				{
					InitGlobalsMachine(scope);
				}
				return s_RootDirMachine;
			}
			if (s_RootDirUser == null)
			{
				InitGlobalsNonRoamingUser(scope);
			}
			return s_RootDirUser;
		}

		private static void InitGlobalsMachine(IsolatedStorageScope scope)
		{
			string text = nGetRootDir(scope);
			new FileIOPermission(FileIOPermissionAccess.AllAccess, text).Assert();
			string text2 = GetMachineRandomDirectory(text);
			if (text2 == null)
			{
				Mutex mutex = CreateMutexNotOwned(text);
				if (!mutex.WaitOne())
				{
					throw new IsolatedStorageException(Environment.GetResourceString("IsolatedStorage_Init"));
				}
				try
				{
					text2 = GetMachineRandomDirectory(text);
					if (text2 == null)
					{
						string randomFileName = Path.GetRandomFileName();
						string randomFileName2 = Path.GetRandomFileName();
						try
						{
							nCreateDirectoryWithDacl(text + randomFileName);
							nCreateDirectoryWithDacl(text + randomFileName + "\\" + randomFileName2);
						}
						catch
						{
							throw new IsolatedStorageException(Environment.GetResourceString("IsolatedStorage_Init"));
						}
						text2 = randomFileName + "\\" + randomFileName2;
					}
				}
				finally
				{
					mutex.ReleaseMutex();
				}
			}
			s_RootDirMachine = text + text2 + "\\";
		}

		private static void InitGlobalsNonRoamingUser(IsolatedStorageScope scope)
		{
			string text = null;
			if (scope == (IsolatedStorageScope.User | IsolatedStorageScope.Application))
			{
				text = GetDataDirectoryFromActivationContext();
				if (text != null)
				{
					s_RootDirUser = text;
					return;
				}
			}
			text = nGetRootDir(scope);
			new FileIOPermission(FileIOPermissionAccess.AllAccess, text).Assert();
			bool bMigrateNeeded = false;
			string sOldStoreLocation = null;
			string text2 = GetRandomDirectory(text, out bMigrateNeeded, out sOldStoreLocation);
			if (text2 == null)
			{
				Mutex mutex = CreateMutexNotOwned(text);
				if (!mutex.WaitOne())
				{
					throw new IsolatedStorageException(Environment.GetResourceString("IsolatedStorage_Init"));
				}
				try
				{
					text2 = GetRandomDirectory(text, out bMigrateNeeded, out sOldStoreLocation);
					if (text2 == null)
					{
						text2 = ((!bMigrateNeeded) ? CreateRandomDirectory(text) : MigrateOldIsoStoreDirectory(text, sOldStoreLocation));
					}
				}
				finally
				{
					mutex.ReleaseMutex();
				}
			}
			s_RootDirUser = text + text2 + "\\";
		}

		internal static string MigrateOldIsoStoreDirectory(string rootDir, string oldRandomDirectory)
		{
			string randomFileName = Path.GetRandomFileName();
			string randomFileName2 = Path.GetRandomFileName();
			string text = rootDir + randomFileName;
			string destDirName = text + "\\" + randomFileName2;
			try
			{
				Directory.CreateDirectory(text);
				Directory.Move(rootDir + oldRandomDirectory, destDirName);
			}
			catch
			{
				throw new IsolatedStorageException(Environment.GetResourceString("IsolatedStorage_Init"));
			}
			return randomFileName + "\\" + randomFileName2;
		}

		internal static string CreateRandomDirectory(string rootDir)
		{
			string text = Path.GetRandomFileName() + "\\" + Path.GetRandomFileName();
			try
			{
				Directory.CreateDirectory(rootDir + text);
				return text;
			}
			catch
			{
				throw new IsolatedStorageException(Environment.GetResourceString("IsolatedStorage_Init"));
			}
		}

		internal static string GetRandomDirectory(string rootDir, out bool bMigrateNeeded, out string sOldStoreLocation)
		{
			bMigrateNeeded = false;
			sOldStoreLocation = null;
			string[] fileDirectoryNames = GetFileDirectoryNames(rootDir + "*", "*", file: false);
			for (int i = 0; i < fileDirectoryNames.Length; i++)
			{
				if (fileDirectoryNames[i].Length != 12)
				{
					continue;
				}
				string[] fileDirectoryNames2 = GetFileDirectoryNames(rootDir + fileDirectoryNames[i] + "\\*", "*", file: false);
				for (int j = 0; j < fileDirectoryNames2.Length; j++)
				{
					if (fileDirectoryNames2[j].Length == 12)
					{
						return fileDirectoryNames[i] + "\\" + fileDirectoryNames2[j];
					}
				}
			}
			for (int k = 0; k < fileDirectoryNames.Length; k++)
			{
				if (fileDirectoryNames[k].Length == 24)
				{
					bMigrateNeeded = true;
					sOldStoreLocation = fileDirectoryNames[k];
					return null;
				}
			}
			return null;
		}

		internal static string GetMachineRandomDirectory(string rootDir)
		{
			string[] fileDirectoryNames = GetFileDirectoryNames(rootDir + "*", "*", file: false);
			for (int i = 0; i < fileDirectoryNames.Length; i++)
			{
				if (fileDirectoryNames[i].Length != 12)
				{
					continue;
				}
				string[] fileDirectoryNames2 = GetFileDirectoryNames(rootDir + fileDirectoryNames[i] + "\\*", "*", file: false);
				for (int j = 0; j < fileDirectoryNames2.Length; j++)
				{
					if (fileDirectoryNames2[j].Length == 12)
					{
						return fileDirectoryNames[i] + "\\" + fileDirectoryNames2[j];
					}
				}
			}
			return null;
		}

		internal static Mutex CreateMutexNotOwned(string pathName)
		{
			return new Mutex(initiallyOwned: false, "Global\\" + GetStrongHashSuitableForObjectName(pathName));
		}

		internal static string GetStrongHashSuitableForObjectName(string name)
		{
			MemoryStream memoryStream = new MemoryStream();
			new BinaryWriter(memoryStream).Write(name.ToUpper(CultureInfo.InvariantCulture));
			memoryStream.Position = 0L;
			return IsolatedStorage.ToBase32StringSuitableForDirName(new SHA1CryptoServiceProvider().ComputeHash(memoryStream));
		}

		private string GetSyncObjectName()
		{
			if (m_SyncObjectName == null)
			{
				m_SyncObjectName = GetStrongHashSuitableForObjectName(m_InfoFile);
			}
			return m_SyncObjectName;
		}

		internal void Lock()
		{
			if (IsRoaming())
			{
				return;
			}
			lock (this)
			{
				if (m_bDisposed)
				{
					throw new ObjectDisposedException(null, Environment.GetResourceString("IsolatedStorage_StoreNotOpen"));
				}
				if (m_closed)
				{
					throw new InvalidOperationException(Environment.GetResourceString("IsolatedStorage_StoreNotOpen"));
				}
				if (m_handle == Win32Native.NULL)
				{
					m_handle = nOpen(m_InfoFile, GetSyncObjectName());
				}
				nLock(m_handle, fLock: true);
			}
		}

		internal void Unlock()
		{
			if (IsRoaming())
			{
				return;
			}
			lock (this)
			{
				if (m_bDisposed)
				{
					throw new ObjectDisposedException(null, Environment.GetResourceString("IsolatedStorage_StoreNotOpen"));
				}
				if (m_closed)
				{
					throw new InvalidOperationException(Environment.GetResourceString("IsolatedStorage_StoreNotOpen"));
				}
				if (m_handle == Win32Native.NULL)
				{
					m_handle = nOpen(m_InfoFile, GetSyncObjectName());
				}
				nLock(m_handle, fLock: false);
			}
		}

		internal static FileIOPermission GetGlobalFileIOPerm(IsolatedStorageScope scope)
		{
			if (IsolatedStorage.IsRoaming(scope))
			{
				if (s_PermRoaming == null)
				{
					s_PermRoaming = new FileIOPermission(FileIOPermissionAccess.AllAccess, GetRootDir(scope));
				}
				return s_PermRoaming;
			}
			if (IsolatedStorage.IsMachine(scope))
			{
				if (s_PermMachine == null)
				{
					s_PermMachine = new FileIOPermission(FileIOPermissionAccess.AllAccess, GetRootDir(scope));
				}
				return s_PermMachine;
			}
			if (s_PermUser == null)
			{
				s_PermUser = new FileIOPermission(FileIOPermissionAccess.AllAccess, GetRootDir(scope));
			}
			return s_PermUser;
		}

		private static void DemandAdminPermission()
		{
			if (s_PermAdminUser == null)
			{
				s_PermAdminUser = new IsolatedStorageFilePermission(IsolatedStorageContainment.AdministerIsolatedStorageByUser, 0L, PermanentData: false);
			}
			s_PermAdminUser.Demand();
		}

		internal static void VerifyGlobalScope(IsolatedStorageScope scope)
		{
			if (scope != IsolatedStorageScope.User && scope != (IsolatedStorageScope.User | IsolatedStorageScope.Roaming) && scope != IsolatedStorageScope.Machine)
			{
				throw new ArgumentException(Environment.GetResourceString("IsolatedStorage_Scope_U_R_M"));
			}
		}

		internal void CreateIDFile(string path, IsolatedStorageScope scope)
		{
			try
			{
				using FileStream fileStream = new FileStream(path + "identity.dat", FileMode.OpenOrCreate);
				MemoryStream identityStream = GetIdentityStream(scope);
				byte[] buffer = identityStream.GetBuffer();
				fileStream.Write(buffer, 0, (int)identityStream.Length);
				identityStream.Close();
			}
			catch
			{
			}
		}

		private static string[] GetFileDirectoryNames(string path, string msg, bool file)
		{
			if (path == null)
			{
				throw new ArgumentNullException("path", Environment.GetResourceString("ArgumentNull_Path"));
			}
			bool flag = false;
			char c = path[path.Length - 1];
			if (c == Path.DirectorySeparatorChar || c == Path.AltDirectorySeparatorChar || c == '.')
			{
				flag = true;
			}
			string text = Path.GetFullPathInternal(path);
			if (flag && text[text.Length - 1] != c)
			{
				text += "\\*";
			}
			string text2 = Path.GetDirectoryName(text);
			if (text2 != null)
			{
				text2 += "\\";
			}
			new FileIOPermission(FileIOPermissionAccess.Read, (text2 == null) ? text : text2).Demand();
			string[] array = new string[10];
			int num = 0;
			Win32Native.WIN32_FIND_DATA wIN32_FIND_DATA = new Win32Native.WIN32_FIND_DATA();
			SafeFindHandle safeFindHandle = Win32Native.FindFirstFile(text, wIN32_FIND_DATA);
			int lastWin32Error;
			if (safeFindHandle.IsInvalid)
			{
				lastWin32Error = Marshal.GetLastWin32Error();
				if (lastWin32Error == 2)
				{
					return new string[0];
				}
				__Error.WinIOError(lastWin32Error, msg);
			}
			int num2 = 0;
			do
			{
				bool flag2;
				if (file)
				{
					flag2 = 0 == (wIN32_FIND_DATA.dwFileAttributes & 0x10);
				}
				else
				{
					flag2 = 0 != (wIN32_FIND_DATA.dwFileAttributes & 0x10);
					if (flag2 && (wIN32_FIND_DATA.cFileName.Equals(".") || wIN32_FIND_DATA.cFileName.Equals("..")))
					{
						flag2 = false;
					}
				}
				if (flag2)
				{
					num2++;
					if (num == array.Length)
					{
						string[] array2 = new string[array.Length * 2];
						Array.Copy(array, 0, array2, 0, num);
						array = array2;
					}
					array[num++] = wIN32_FIND_DATA.cFileName;
				}
			}
			while (Win32Native.FindNextFile(safeFindHandle, wIN32_FIND_DATA));
			lastWin32Error = Marshal.GetLastWin32Error();
			safeFindHandle.Close();
			if (lastWin32Error != 0 && lastWin32Error != 18)
			{
				__Error.WinIOError(lastWin32Error, msg);
			}
			if (!file && num2 == 1 && ((uint)wIN32_FIND_DATA.dwFileAttributes & 0x10u) != 0)
			{
				return new string[1]
				{
					wIN32_FIND_DATA.cFileName
				};
			}
			if (num == array.Length)
			{
				return array;
			}
			string[] array3 = new string[num];
			Array.Copy(array, 0, array3, 0, num);
			return array3;
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern ulong nGetUsage(IntPtr handle);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern IntPtr nOpen(string infoFile, string syncName);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern void nClose(IntPtr handle);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal unsafe static extern void nReserve(IntPtr handle, ulong* plQuota, ulong* plReserve, bool fFree);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern string nGetRootDir(IsolatedStorageScope scope);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern void nLock(IntPtr handle, bool fLock);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern void nCreateDirectoryWithDacl(string path);
	}
}
