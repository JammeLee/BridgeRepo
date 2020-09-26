using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Security.Permissions;
using System.Security.Util;
using Microsoft.Win32.SafeHandles;

namespace System.Security.Policy
{
	[Serializable]
	[ComVisible(true)]
	public sealed class Hash : ISerializable, IBuiltInEvidence
	{
		private SafePEFileHandle m_peFile = SafePEFileHandle.InvalidHandle;

		private byte[] m_rawData;

		private byte[] m_sha1;

		private byte[] m_md5;

		internal byte[] RawData
		{
			get
			{
				if (m_rawData == null)
				{
					if (m_peFile.IsInvalid)
					{
						throw new SecurityException(Environment.GetResourceString("Security_CannotGetRawData"));
					}
					byte[] array = _GetRawData(m_peFile);
					if (array == null)
					{
						throw new SecurityException(Environment.GetResourceString("Security_CannotGenerateHash"));
					}
					m_rawData = array;
				}
				return m_rawData;
			}
		}

		public byte[] SHA1
		{
			get
			{
				if (m_sha1 == null)
				{
					SHA1 sHA = new SHA1Managed();
					m_sha1 = sHA.ComputeHash(RawData);
				}
				byte[] array = new byte[m_sha1.Length];
				Array.Copy(m_sha1, array, m_sha1.Length);
				return array;
			}
		}

		public byte[] MD5
		{
			get
			{
				if (m_md5 == null)
				{
					MD5 mD = new MD5CryptoServiceProvider();
					m_md5 = mD.ComputeHash(RawData);
				}
				byte[] array = new byte[m_md5.Length];
				Array.Copy(m_md5, array, m_md5.Length);
				return array;
			}
		}

		internal Hash()
		{
		}

		internal Hash(SerializationInfo info, StreamingContext context)
		{
			m_md5 = (byte[])info.GetValueNoThrow("Md5", typeof(byte[]));
			m_sha1 = (byte[])info.GetValueNoThrow("Sha1", typeof(byte[]));
			m_peFile = SafePEFileHandle.InvalidHandle;
			m_rawData = (byte[])info.GetValue("RawData", typeof(byte[]));
			if (m_rawData == null)
			{
				IntPtr intPtr = (IntPtr)info.GetValue("PEFile", typeof(IntPtr));
				if (intPtr != IntPtr.Zero)
				{
					_SetPEFileHandle(intPtr, ref m_peFile);
				}
			}
		}

		public Hash(Assembly assembly)
		{
			if (assembly == null)
			{
				throw new ArgumentNullException("assembly");
			}
			_GetPEFileFromAssembly(assembly.InternalAssembly, ref m_peFile);
		}

		public static Hash CreateSHA1(byte[] sha1)
		{
			if (sha1 == null)
			{
				throw new ArgumentNullException("sha1");
			}
			Hash hash = new Hash();
			hash.m_sha1 = new byte[sha1.Length];
			Array.Copy(sha1, hash.m_sha1, sha1.Length);
			return hash;
		}

		public static Hash CreateMD5(byte[] md5)
		{
			if (md5 == null)
			{
				throw new ArgumentNullException("md5");
			}
			Hash hash = new Hash();
			hash.m_md5 = new byte[md5.Length];
			Array.Copy(md5, hash.m_md5, md5.Length);
			return hash;
		}

		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Demand();
			info.AddValue("Md5", m_md5);
			info.AddValue("Sha1", m_sha1);
			if (context.State == StreamingContextStates.Clone || context.State == StreamingContextStates.CrossAppDomain)
			{
				info.AddValue("PEFile", m_peFile.DangerousGetHandle());
				if (m_peFile.IsInvalid)
				{
					info.AddValue("RawData", m_rawData);
				}
				else
				{
					info.AddValue("RawData", null);
				}
				return;
			}
			if (!m_peFile.IsInvalid)
			{
				m_rawData = RawData;
			}
			info.AddValue("PEFile", IntPtr.Zero);
			info.AddValue("RawData", m_rawData);
		}

		public byte[] GenerateHash(HashAlgorithm hashAlg)
		{
			if (hashAlg == null)
			{
				throw new ArgumentNullException("hashAlg");
			}
			if (hashAlg is SHA1)
			{
				return SHA1;
			}
			if (hashAlg is MD5)
			{
				return MD5;
			}
			return hashAlg.ComputeHash(RawData);
		}

		int IBuiltInEvidence.OutputToBuffer(char[] buffer, int position, bool verbose)
		{
			if (!verbose)
			{
				return position;
			}
			buffer[position++] = '\b';
			IntPtr value = IntPtr.Zero;
			if (!m_peFile.IsInvalid)
			{
				value = m_peFile.DangerousGetHandle();
			}
			BuiltInEvidenceHelper.CopyLongToCharArray((long)value, buffer, position);
			return position + 4;
		}

		int IBuiltInEvidence.GetRequiredSize(bool verbose)
		{
			if (verbose)
			{
				return 5;
			}
			return 0;
		}

		int IBuiltInEvidence.InitFromBuffer(char[] buffer, int position)
		{
			m_peFile = SafePEFileHandle.InvalidHandle;
			IntPtr inHandle = (IntPtr)BuiltInEvidenceHelper.GetLongFromCharArray(buffer, position);
			_SetPEFileHandle(inHandle, ref m_peFile);
			return position + 4;
		}

		private SecurityElement ToXml()
		{
			SecurityElement securityElement = new SecurityElement("System.Security.Policy.Hash");
			securityElement.AddAttribute("version", "1");
			securityElement.AddChild(new SecurityElement("RawData", Hex.EncodeHexString(RawData)));
			return securityElement;
		}

		public override string ToString()
		{
			return ToXml().ToString();
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern byte[] _GetRawData(SafePEFileHandle handle);

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void _GetPEFileFromAssembly(Assembly assembly, ref SafePEFileHandle handle);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		internal static extern void _ReleasePEFile(IntPtr handle);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern void _SetPEFileHandle(IntPtr inHandle, ref SafePEFileHandle outHandle);
	}
}
