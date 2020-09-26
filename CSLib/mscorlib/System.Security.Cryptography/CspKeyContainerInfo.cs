using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Permissions;

namespace System.Security.Cryptography
{
	[ComVisible(true)]
	public sealed class CspKeyContainerInfo
	{
		private CspParameters m_parameters;

		private bool m_randomKeyContainer;

		public bool MachineKeyStore
		{
			get
			{
				if ((m_parameters.Flags & CspProviderFlags.UseMachineKeyStore) != CspProviderFlags.UseMachineKeyStore)
				{
					return false;
				}
				return true;
			}
		}

		public string ProviderName => m_parameters.ProviderName;

		public int ProviderType => m_parameters.ProviderType;

		public string KeyContainerName => m_parameters.KeyContainerName;

		public string UniqueKeyContainerName
		{
			get
			{
				if (Utils.Win2KCrypto == 0)
				{
					throw new NotSupportedException(Environment.GetResourceString("NotSupported_Method"));
				}
				SafeProvHandle hProv = SafeProvHandle.InvalidHandle;
				if (Utils._OpenCSP(m_parameters, 64u, ref hProv) != 0)
				{
					throw new CryptographicException(Environment.GetResourceString("Cryptography_CSP_NotFound"));
				}
				string result = (string)Utils._GetProviderParameter(hProv, m_parameters.KeyNumber, 8u);
				hProv.Dispose();
				return result;
			}
		}

		public KeyNumber KeyNumber => (KeyNumber)m_parameters.KeyNumber;

		public bool Exportable
		{
			get
			{
				if (Utils.Win2KCrypto == 0)
				{
					throw new NotSupportedException(Environment.GetResourceString("NotSupported_Method"));
				}
				if (HardwareDevice)
				{
					return false;
				}
				SafeProvHandle hProv = SafeProvHandle.InvalidHandle;
				if (Utils._OpenCSP(m_parameters, 64u, ref hProv) != 0)
				{
					throw new CryptographicException(Environment.GetResourceString("Cryptography_CSP_NotFound"));
				}
				byte[] array = (byte[])Utils._GetProviderParameter(hProv, m_parameters.KeyNumber, 3u);
				hProv.Dispose();
				return array[0] == 1;
			}
		}

		public bool HardwareDevice
		{
			get
			{
				SafeProvHandle hProv = SafeProvHandle.InvalidHandle;
				CspParameters cspParameters = new CspParameters(m_parameters);
				cspParameters.KeyContainerName = null;
				cspParameters.Flags = (((cspParameters.Flags & CspProviderFlags.UseMachineKeyStore) != 0) ? CspProviderFlags.UseMachineKeyStore : CspProviderFlags.NoFlags);
				uint num = 0u;
				if (Utils.Win2KCrypto == 1)
				{
					num |= 0xF0000000u;
				}
				if (Utils._OpenCSP(cspParameters, num, ref hProv) != 0)
				{
					throw new CryptographicException(Environment.GetResourceString("Cryptography_CSP_NotFound"));
				}
				byte[] array = (byte[])Utils._GetProviderParameter(hProv, cspParameters.KeyNumber, 5u);
				hProv.Dispose();
				return array[0] == 1;
			}
		}

		public bool Removable
		{
			get
			{
				SafeProvHandle hProv = SafeProvHandle.InvalidHandle;
				CspParameters cspParameters = new CspParameters(m_parameters);
				cspParameters.KeyContainerName = null;
				cspParameters.Flags = (((cspParameters.Flags & CspProviderFlags.UseMachineKeyStore) != 0) ? CspProviderFlags.UseMachineKeyStore : CspProviderFlags.NoFlags);
				uint num = 0u;
				if (Utils.Win2KCrypto == 1)
				{
					num |= 0xF0000000u;
				}
				if (Utils._OpenCSP(cspParameters, num, ref hProv) != 0)
				{
					throw new CryptographicException(Environment.GetResourceString("Cryptography_CSP_NotFound"));
				}
				byte[] array = (byte[])Utils._GetProviderParameter(hProv, cspParameters.KeyNumber, 4u);
				hProv.Dispose();
				return array[0] == 1;
			}
		}

		public bool Accessible
		{
			get
			{
				if (Utils.Win2KCrypto == 0)
				{
					throw new NotSupportedException(Environment.GetResourceString("NotSupported_Method"));
				}
				SafeProvHandle hProv = SafeProvHandle.InvalidHandle;
				if (Utils._OpenCSP(m_parameters, 64u, ref hProv) != 0)
				{
					return false;
				}
				byte[] array = (byte[])Utils._GetProviderParameter(hProv, m_parameters.KeyNumber, 6u);
				hProv.Dispose();
				return array[0] == 1;
			}
		}

		public bool Protected
		{
			get
			{
				if (HardwareDevice)
				{
					return true;
				}
				if (Utils.Win2KCrypto == 0)
				{
					throw new NotSupportedException(Environment.GetResourceString("NotSupported_Method"));
				}
				SafeProvHandle hProv = SafeProvHandle.InvalidHandle;
				if (Utils._OpenCSP(m_parameters, 64u, ref hProv) != 0)
				{
					throw new CryptographicException(Environment.GetResourceString("Cryptography_CSP_NotFound"));
				}
				byte[] array = (byte[])Utils._GetProviderParameter(hProv, m_parameters.KeyNumber, 7u);
				hProv.Dispose();
				return array[0] == 1;
			}
		}

		public CryptoKeySecurity CryptoKeySecurity
		{
			get
			{
				if (Utils.Win2KCrypto == 0)
				{
					throw new NotSupportedException(Environment.GetResourceString("NotSupported_Method"));
				}
				KeyContainerPermission keyContainerPermission = new KeyContainerPermission(KeyContainerPermissionFlags.NoFlags);
				KeyContainerPermissionAccessEntry accessEntry = new KeyContainerPermissionAccessEntry(m_parameters, KeyContainerPermissionFlags.ViewAcl | KeyContainerPermissionFlags.ChangeAcl);
				keyContainerPermission.AccessEntries.Add(accessEntry);
				keyContainerPermission.Demand();
				SafeProvHandle hProv = SafeProvHandle.InvalidHandle;
				if (Utils._OpenCSP(m_parameters, 64u, ref hProv) != 0)
				{
					throw new CryptographicException(Environment.GetResourceString("Cryptography_CSP_NotFound"));
				}
				using (hProv)
				{
					return Utils.GetKeySetSecurityInfo(hProv, AccessControlSections.All);
				}
			}
		}

		public bool RandomlyGenerated => m_randomKeyContainer;

		private CspKeyContainerInfo()
		{
		}

		internal CspKeyContainerInfo(CspParameters parameters, bool randomKeyContainer)
		{
			KeyContainerPermission keyContainerPermission = new KeyContainerPermission(KeyContainerPermissionFlags.NoFlags);
			KeyContainerPermissionAccessEntry accessEntry = new KeyContainerPermissionAccessEntry(parameters, KeyContainerPermissionFlags.Open);
			keyContainerPermission.AccessEntries.Add(accessEntry);
			keyContainerPermission.Demand();
			m_parameters = new CspParameters(parameters);
			if (m_parameters.KeyNumber == -1)
			{
				if (m_parameters.ProviderType == 1 || m_parameters.ProviderType == 24)
				{
					m_parameters.KeyNumber = 1;
				}
				else if (m_parameters.ProviderType == 13)
				{
					m_parameters.KeyNumber = 2;
				}
			}
			m_randomKeyContainer = randomKeyContainer;
		}

		public CspKeyContainerInfo(CspParameters parameters)
			: this(parameters, randomKeyContainer: false)
		{
		}
	}
}
