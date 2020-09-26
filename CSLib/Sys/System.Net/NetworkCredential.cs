using System.IO;
using System.Security.Cryptography;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using Microsoft.Win32;

namespace System.Net
{
	public class NetworkCredential : ICredentials, ICredentialsByHost
	{
		private static EnvironmentPermission m_environmentUserNamePermission;

		private static EnvironmentPermission m_environmentDomainNamePermission;

		private static readonly object lockingObject = new object();

		private static SymmetricAlgorithm s_symmetricAlgorithm;

		private static RNGCryptoServiceProvider s_random;

		private static bool s_useTripleDES = false;

		private byte[] m_userName;

		private byte[] m_password;

		private byte[] m_domain;

		private byte[] m_encryptionIV;

		private bool m_encrypt = true;

		public string UserName
		{
			get
			{
				InitializePart1();
				m_environmentUserNamePermission.Demand();
				return InternalGetUserName();
			}
			set
			{
				m_userName = Encrypt(value);
			}
		}

		public string Password
		{
			get
			{
				ExceptionHelper.UnmanagedPermission.Demand();
				return InternalGetPassword();
			}
			set
			{
				m_password = Encrypt(value);
			}
		}

		public string Domain
		{
			get
			{
				InitializePart1();
				m_environmentDomainNamePermission.Demand();
				return InternalGetDomain();
			}
			set
			{
				m_domain = Encrypt(value);
			}
		}

		public NetworkCredential()
		{
		}

		public NetworkCredential(string userName, string password)
			: this(userName, password, string.Empty)
		{
		}

		public NetworkCredential(string userName, string password, string domain)
			: this(userName, password, domain, encrypt: true)
		{
		}

		internal NetworkCredential(string userName, string password, string domain, bool encrypt)
		{
			m_encrypt = encrypt;
			UserName = userName;
			Password = password;
			Domain = domain;
		}

		private void InitializePart1()
		{
			if (m_environmentUserNamePermission != null)
			{
				return;
			}
			lock (lockingObject)
			{
				if (m_environmentUserNamePermission == null)
				{
					m_environmentDomainNamePermission = new EnvironmentPermission(EnvironmentPermissionAccess.Read, "USERDOMAIN");
					m_environmentUserNamePermission = new EnvironmentPermission(EnvironmentPermissionAccess.Read, "USERNAME");
				}
			}
		}

		private void InitializePart2()
		{
			if (!m_encrypt)
			{
				return;
			}
			if (s_symmetricAlgorithm == null)
			{
				lock (lockingObject)
				{
					if (s_symmetricAlgorithm == null)
					{
						s_useTripleDES = ReadRegFips();
						SymmetricAlgorithm symmetricAlgorithm;
						if (s_useTripleDES)
						{
							symmetricAlgorithm = new TripleDESCryptoServiceProvider();
							symmetricAlgorithm.KeySize = 128;
							symmetricAlgorithm.GenerateKey();
						}
						else
						{
							s_random = new RNGCryptoServiceProvider();
							symmetricAlgorithm = Rijndael.Create();
							byte[] array = new byte[16];
							s_random.GetBytes(array);
							symmetricAlgorithm.Key = array;
						}
						s_symmetricAlgorithm = symmetricAlgorithm;
					}
				}
			}
			if (m_encryptionIV == null)
			{
				if (s_useTripleDES)
				{
					s_symmetricAlgorithm.GenerateIV();
					byte[] iV = s_symmetricAlgorithm.IV;
					Interlocked.CompareExchange(ref m_encryptionIV, iV, null);
				}
				else
				{
					byte[] array2 = new byte[16];
					s_random.GetBytes(array2);
					Interlocked.CompareExchange(ref m_encryptionIV, array2, null);
				}
			}
		}

		internal string InternalGetUserName()
		{
			return Decrypt(m_userName);
		}

		internal string InternalGetPassword()
		{
			return Decrypt(m_password);
		}

		internal string InternalGetDomain()
		{
			return Decrypt(m_domain);
		}

		internal string InternalGetDomainUserName()
		{
			string text = InternalGetDomain();
			if (text.Length != 0)
			{
				text += "\\";
			}
			return text + InternalGetUserName();
		}

		public NetworkCredential GetCredential(Uri uri, string authType)
		{
			return this;
		}

		public NetworkCredential GetCredential(string host, int port, string authenticationType)
		{
			return this;
		}

		internal bool IsEqualTo(object compObject)
		{
			if (compObject == null)
			{
				return false;
			}
			if (this == compObject)
			{
				return true;
			}
			NetworkCredential networkCredential = compObject as NetworkCredential;
			if (networkCredential == null)
			{
				return false;
			}
			if (InternalGetUserName() == networkCredential.InternalGetUserName() && InternalGetPassword() == networkCredential.InternalGetPassword())
			{
				return InternalGetDomain() == networkCredential.InternalGetDomain();
			}
			return false;
		}

		internal string Decrypt(byte[] ciphertext)
		{
			if (ciphertext == null)
			{
				return string.Empty;
			}
			if (!m_encrypt)
			{
				return Encoding.UTF8.GetString(ciphertext);
			}
			InitializePart2();
			MemoryStream memoryStream = new MemoryStream();
			CryptoStream cryptoStream = new CryptoStream(memoryStream, s_symmetricAlgorithm.CreateDecryptor(s_symmetricAlgorithm.Key, m_encryptionIV), CryptoStreamMode.Write);
			cryptoStream.Write(ciphertext, 0, ciphertext.Length);
			cryptoStream.FlushFinalBlock();
			byte[] bytes = memoryStream.ToArray();
			cryptoStream.Close();
			return Encoding.UTF8.GetString(bytes);
		}

		internal byte[] Encrypt(string text)
		{
			if (text == null || text.Length == 0)
			{
				return null;
			}
			if (!m_encrypt)
			{
				return Encoding.UTF8.GetBytes(text);
			}
			InitializePart2();
			MemoryStream memoryStream = new MemoryStream();
			CryptoStream cryptoStream = new CryptoStream(memoryStream, s_symmetricAlgorithm.CreateEncryptor(s_symmetricAlgorithm.Key, m_encryptionIV), CryptoStreamMode.Write);
			byte[] bytes = Encoding.UTF8.GetBytes(text);
			cryptoStream.Write(bytes, 0, bytes.Length);
			cryptoStream.FlushFinalBlock();
			bytes = memoryStream.ToArray();
			cryptoStream.Close();
			return bytes;
		}

		[RegistryPermission(SecurityAction.Assert, Read = "HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Control\\Lsa")]
		private bool ReadRegFips()
		{
			bool flag = false;
			bool pfEnabled = false;
			if (ComNetOS.IsVista)
			{
				uint num = UnsafeNclNativeMethods.BCryptGetFipsAlgorithmMode(out pfEnabled);
				flag = num == 0 || num == 3221225524u;
			}
			else
			{
				RegistryKey registryKey = null;
				object obj = null;
				try
				{
					string name = "SYSTEM\\CurrentControlSet\\Control\\Lsa";
					registryKey = Registry.LocalMachine.OpenSubKey(name);
					if (registryKey != null)
					{
						obj = registryKey.GetValue("fipsalgorithmpolicy");
					}
					flag = true;
					if (obj != null && (int)obj == 1)
					{
						pfEnabled = true;
					}
				}
				catch
				{
				}
				finally
				{
					registryKey?.Close();
				}
			}
			if (!flag || pfEnabled)
			{
				return true;
			}
			return false;
		}
	}
}
