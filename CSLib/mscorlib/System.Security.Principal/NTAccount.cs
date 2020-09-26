using System.Runtime.InteropServices;
using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;

namespace System.Security.Principal
{
	[ComVisible(false)]
	public sealed class NTAccount : IdentityReference
	{
		internal const int MaximumAccountNameLength = 256;

		internal const int MaximumDomainNameLength = 255;

		private readonly string _Name;

		public override string Value => ToString();

		public NTAccount(string domainName, string accountName)
		{
			if (accountName == null)
			{
				throw new ArgumentNullException("accountName");
			}
			if (accountName.Length == 0)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_StringZeroLength"), "accountName");
			}
			if (accountName.Length > 256)
			{
				throw new ArgumentException(Environment.GetResourceString("IdentityReference_AccountNameTooLong"), "accountName");
			}
			if (domainName != null && domainName.Length > 255)
			{
				throw new ArgumentException(Environment.GetResourceString("IdentityReference_DomainNameTooLong"), "domainName");
			}
			if (domainName == null || domainName.Length == 0)
			{
				_Name = accountName;
			}
			else
			{
				_Name = domainName + "\\" + accountName;
			}
		}

		public NTAccount(string name)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			if (name.Length == 0)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument_StringZeroLength"), "name");
			}
			if (name.Length > 512)
			{
				throw new ArgumentException(Environment.GetResourceString("IdentityReference_AccountNameTooLong"), "name");
			}
			_Name = name;
		}

		public override bool IsValidTargetType(Type targetType)
		{
			if (targetType == typeof(SecurityIdentifier))
			{
				return true;
			}
			if (targetType == typeof(NTAccount))
			{
				return true;
			}
			return false;
		}

		public override IdentityReference Translate(Type targetType)
		{
			if (targetType == null)
			{
				throw new ArgumentNullException("targetType");
			}
			if (targetType == typeof(NTAccount))
			{
				return this;
			}
			if (targetType == typeof(SecurityIdentifier))
			{
				IdentityReferenceCollection identityReferenceCollection = new IdentityReferenceCollection(1);
				identityReferenceCollection.Add(this);
				IdentityReferenceCollection identityReferenceCollection2 = Translate(identityReferenceCollection, targetType, forceSuccess: true);
				return identityReferenceCollection2[0];
			}
			throw new ArgumentException(Environment.GetResourceString("IdentityReference_MustBeIdentityReference"), "targetType");
		}

		public override bool Equals(object o)
		{
			if (o == null)
			{
				return false;
			}
			NTAccount nTAccount = o as NTAccount;
			if (nTAccount == null)
			{
				return false;
			}
			return this == nTAccount;
		}

		public override int GetHashCode()
		{
			return StringComparer.InvariantCultureIgnoreCase.GetHashCode(_Name);
		}

		public override string ToString()
		{
			return _Name;
		}

		internal static IdentityReferenceCollection Translate(IdentityReferenceCollection sourceAccounts, Type targetType, bool forceSuccess)
		{
			bool someFailed = false;
			IdentityReferenceCollection identityReferenceCollection = Translate(sourceAccounts, targetType, out someFailed);
			if (forceSuccess && someFailed)
			{
				IdentityReferenceCollection identityReferenceCollection2 = new IdentityReferenceCollection();
				foreach (IdentityReference item in identityReferenceCollection)
				{
					if (item.GetType() != targetType)
					{
						identityReferenceCollection2.Add(item);
					}
				}
				throw new IdentityNotMappedException(Environment.GetResourceString("IdentityReference_IdentityNotMapped"), identityReferenceCollection2);
			}
			return identityReferenceCollection;
		}

		internal static IdentityReferenceCollection Translate(IdentityReferenceCollection sourceAccounts, Type targetType, out bool someFailed)
		{
			if (sourceAccounts == null)
			{
				throw new ArgumentNullException("sourceAccounts");
			}
			if (targetType == typeof(SecurityIdentifier))
			{
				return TranslateToSids(sourceAccounts, out someFailed);
			}
			throw new ArgumentException(Environment.GetResourceString("IdentityReference_MustBeIdentityReference"), "targetType");
		}

		public static bool operator ==(NTAccount left, NTAccount right)
		{
			if ((object)left == null && (object)right == null)
			{
				return true;
			}
			if ((object)left == null || (object)right == null)
			{
				return false;
			}
			return left.ToString().Equals(right.ToString(), StringComparison.OrdinalIgnoreCase);
		}

		public static bool operator !=(NTAccount left, NTAccount right)
		{
			return !(left == right);
		}

		private static IdentityReferenceCollection TranslateToSids(IdentityReferenceCollection sourceAccounts, out bool someFailed)
		{
			if (!Win32.LsaApisSupported)
			{
				throw new PlatformNotSupportedException(Environment.GetResourceString("PlatformNotSupported_Win9x"));
			}
			SafeLsaPolicyHandle safeLsaPolicyHandle = SafeLsaPolicyHandle.InvalidHandle;
			SafeLsaMemoryHandle referencedDomains = SafeLsaMemoryHandle.InvalidHandle;
			SafeLsaMemoryHandle sids = SafeLsaMemoryHandle.InvalidHandle;
			int num = 0;
			if (sourceAccounts == null)
			{
				throw new ArgumentNullException("sourceAccounts");
			}
			if (sourceAccounts.Count == 0)
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_EmptyCollection"), "sourceAccounts");
			}
			try
			{
				Win32Native.UNICODE_STRING[] array = new Win32Native.UNICODE_STRING[sourceAccounts.Count];
				foreach (IdentityReference sourceAccount in sourceAccounts)
				{
					NTAccount nTAccount = sourceAccount as NTAccount;
					if (nTAccount == null)
					{
						throw new ArgumentException(Environment.GetResourceString("Argument_ImproperType"), "sourceAccounts");
					}
					array[num].Buffer = nTAccount.ToString();
					if (array[num].Buffer.Length * 2 + 2 > 65535)
					{
						throw new SystemException();
					}
					array[num].Length = (ushort)(array[num].Buffer.Length * 2);
					array[num].MaximumLength = (ushort)(array[num].Length + 2);
					num++;
				}
				safeLsaPolicyHandle = Win32.LsaOpenPolicy(null, PolicyRights.POLICY_LOOKUP_NAMES);
				someFailed = false;
				uint num2 = ((!Win32.LsaLookupNames2Supported) ? Win32Native.LsaLookupNames(safeLsaPolicyHandle, sourceAccounts.Count, array, ref referencedDomains, ref sids) : Win32Native.LsaLookupNames2(safeLsaPolicyHandle, 0, sourceAccounts.Count, array, ref referencedDomains, ref sids));
				switch (num2)
				{
				case 3221225495u:
				case 3221225626u:
					throw new OutOfMemoryException();
				case 3221225506u:
					throw new UnauthorizedAccessException();
				case 3221225587u:
				case 263u:
					someFailed = true;
					break;
				default:
				{
					int errorCode = Win32Native.LsaNtStatusToWinError((int)num2);
					throw new SystemException(Win32Native.GetMessage(errorCode));
				}
				case 0u:
					break;
				}
				IdentityReferenceCollection identityReferenceCollection = new IdentityReferenceCollection(sourceAccounts.Count);
				if (num2 == 0 || num2 == 263)
				{
					if (Win32.LsaLookupNames2Supported)
					{
						for (num = 0; num < sourceAccounts.Count; num++)
						{
							Win32Native.LSA_TRANSLATED_SID2 lSA_TRANSLATED_SID = (Win32Native.LSA_TRANSLATED_SID2)Marshal.PtrToStructure(new IntPtr((long)sids.DangerousGetHandle() + num * Marshal.SizeOf(typeof(Win32Native.LSA_TRANSLATED_SID2))), typeof(Win32Native.LSA_TRANSLATED_SID2));
							switch (lSA_TRANSLATED_SID.Use)
							{
							case 1:
							case 2:
							case 4:
							case 5:
							case 9:
								identityReferenceCollection.Add(new SecurityIdentifier(lSA_TRANSLATED_SID.Sid, noDemand: true));
								break;
							default:
								someFailed = true;
								identityReferenceCollection.Add(sourceAccounts[num]);
								break;
							}
						}
					}
					else
					{
						Win32Native.LSA_REFERENCED_DOMAIN_LIST lSA_REFERENCED_DOMAIN_LIST = (Win32Native.LSA_REFERENCED_DOMAIN_LIST)Marshal.PtrToStructure(referencedDomains.DangerousGetHandle(), typeof(Win32Native.LSA_REFERENCED_DOMAIN_LIST));
						SecurityIdentifier[] array2 = new SecurityIdentifier[lSA_REFERENCED_DOMAIN_LIST.Entries];
						for (num = 0; num < lSA_REFERENCED_DOMAIN_LIST.Entries; num++)
						{
							array2[num] = new SecurityIdentifier(((Win32Native.LSA_TRUST_INFORMATION)Marshal.PtrToStructure(new IntPtr((long)lSA_REFERENCED_DOMAIN_LIST.Domains + num * Marshal.SizeOf(typeof(Win32Native.LSA_TRUST_INFORMATION))), typeof(Win32Native.LSA_TRUST_INFORMATION))).Sid, noDemand: true);
						}
						for (num = 0; num < sourceAccounts.Count; num++)
						{
							Win32Native.LSA_TRANSLATED_SID lSA_TRANSLATED_SID2 = (Win32Native.LSA_TRANSLATED_SID)Marshal.PtrToStructure(new IntPtr((long)sids.DangerousGetHandle() + num * Marshal.SizeOf(typeof(Win32Native.LSA_TRANSLATED_SID))), typeof(Win32Native.LSA_TRANSLATED_SID));
							switch (lSA_TRANSLATED_SID2.Use)
							{
							case 1:
							case 2:
							case 4:
							case 5:
							case 9:
								identityReferenceCollection.Add(new SecurityIdentifier(array2[lSA_TRANSLATED_SID2.DomainIndex], lSA_TRANSLATED_SID2.Rid));
								break;
							default:
								someFailed = true;
								identityReferenceCollection.Add(sourceAccounts[num]);
								break;
							}
						}
					}
				}
				else
				{
					for (num = 0; num < sourceAccounts.Count; num++)
					{
						identityReferenceCollection.Add(sourceAccounts[num]);
					}
				}
				return identityReferenceCollection;
			}
			finally
			{
				safeLsaPolicyHandle.Dispose();
				referencedDomains.Dispose();
				sids.Dispose();
			}
		}
	}
}
