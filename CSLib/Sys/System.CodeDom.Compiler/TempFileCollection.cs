using System.Collections;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Security.Permissions;
using System.Security.Principal;

namespace System.CodeDom.Compiler
{
	[Serializable]
	[PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
	[PermissionSet(SecurityAction.InheritanceDemand, Name = "FullTrust")]
	public class TempFileCollection : ICollection, IEnumerable, IDisposable
	{
		private static RNGCryptoServiceProvider rng;

		private string basePath;

		private string tempDir;

		private bool keepFiles;

		private Hashtable files;

		public int Count => files.Count;

		int ICollection.Count => files.Count;

		object ICollection.SyncRoot => null;

		bool ICollection.IsSynchronized => false;

		public string TempDir
		{
			get
			{
				if (tempDir != null)
				{
					return tempDir;
				}
				return string.Empty;
			}
		}

		public string BasePath
		{
			get
			{
				EnsureTempNameCreated();
				return basePath;
			}
		}

		public bool KeepFiles
		{
			get
			{
				return keepFiles;
			}
			set
			{
				keepFiles = value;
			}
		}

		static TempFileCollection()
		{
			rng = new RNGCryptoServiceProvider();
		}

		public TempFileCollection()
			: this(null, keepFiles: false)
		{
		}

		public TempFileCollection(string tempDir)
			: this(tempDir, keepFiles: false)
		{
		}

		public TempFileCollection(string tempDir, bool keepFiles)
		{
			this.keepFiles = keepFiles;
			this.tempDir = tempDir;
			files = new Hashtable(StringComparer.OrdinalIgnoreCase);
		}

		void IDisposable.Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			Delete();
		}

		~TempFileCollection()
		{
			Dispose(disposing: false);
		}

		public string AddExtension(string fileExtension)
		{
			return AddExtension(fileExtension, keepFiles);
		}

		public string AddExtension(string fileExtension, bool keepFile)
		{
			if (fileExtension == null || fileExtension.Length == 0)
			{
				throw new ArgumentException(SR.GetString("InvalidNullEmptyArgument", "fileExtension"), "fileExtension");
			}
			string text = BasePath + "." + fileExtension;
			AddFile(text, keepFile);
			return text;
		}

		public void AddFile(string fileName, bool keepFile)
		{
			if (fileName == null || fileName.Length == 0)
			{
				throw new ArgumentException(SR.GetString("InvalidNullEmptyArgument", "fileName"), "fileName");
			}
			if (files[fileName] != null)
			{
				throw new ArgumentException(SR.GetString("DuplicateFileName", fileName), "fileName");
			}
			files.Add(fileName, keepFile);
		}

		public IEnumerator GetEnumerator()
		{
			return files.Keys.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return files.Keys.GetEnumerator();
		}

		void ICollection.CopyTo(Array array, int start)
		{
			files.Keys.CopyTo(array, start);
		}

		public void CopyTo(string[] fileNames, int start)
		{
			files.Keys.CopyTo(fileNames, start);
		}

		private void EnsureTempNameCreated()
		{
			if (basePath != null)
			{
				return;
			}
			string text = null;
			bool flag = false;
			int num = 5000;
			do
			{
				try
				{
					basePath = GetTempFileName(TempDir);
					string fullPath = basePath;
					new EnvironmentPermission(PermissionState.Unrestricted).Assert();
					try
					{
						fullPath = Path.GetFullPath(basePath);
					}
					finally
					{
						CodeAccessPermission.RevertAssert();
					}
					new FileIOPermission(FileIOPermissionAccess.AllAccess, fullPath).Demand();
					text = basePath + ".tmp";
					using (new FileStream(text, FileMode.CreateNew, FileAccess.Write))
					{
					}
					flag = true;
				}
				catch (IOException e)
				{
					num--;
					if (num == 0 || Marshal.GetHRForException(e) != 80)
					{
						throw;
					}
					flag = false;
				}
			}
			while (!flag);
			files.Add(text, keepFiles);
		}

		private bool KeepFile(string fileName)
		{
			object obj = files[fileName];
			if (obj == null)
			{
				return false;
			}
			return (bool)obj;
		}

		public void Delete()
		{
			if (files == null)
			{
				return;
			}
			string[] array = new string[files.Count];
			files.Keys.CopyTo(array, 0);
			string[] array2 = array;
			foreach (string text in array2)
			{
				if (!KeepFile(text))
				{
					Delete(text);
					files.Remove(text);
				}
			}
		}

		internal void SafeDelete()
		{
			WindowsImpersonationContext impersonation = Executor.RevertImpersonation();
			try
			{
				Delete();
			}
			finally
			{
				Executor.ReImpersonate(impersonation);
			}
		}

		private void Delete(string fileName)
		{
			try
			{
				File.Delete(fileName);
			}
			catch
			{
			}
		}

		private static string GetTempFileName(string tempDir)
		{
			if (tempDir == null || tempDir.Length == 0)
			{
				tempDir = Path.GetTempPath();
			}
			string text = GenerateRandomFileName();
			if (tempDir.EndsWith("\\", StringComparison.Ordinal))
			{
				return tempDir + text;
			}
			return tempDir + "\\" + text;
		}

		private static string GenerateRandomFileName()
		{
			byte[] array = new byte[6];
			lock (rng)
			{
				rng.GetBytes(array);
			}
			string text = Convert.ToBase64String(array).ToLower(CultureInfo.InvariantCulture);
			text = text.Replace('/', '-');
			return text.Replace('+', '_');
		}
	}
}
