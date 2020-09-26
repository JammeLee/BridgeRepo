using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;

namespace System.Net
{
	internal struct SecureCredential
	{
		[Flags]
		public enum Flags
		{
			Zero = 0x0,
			NoSystemMapper = 0x2,
			NoNameCheck = 0x4,
			ValidateManual = 0x8,
			NoDefaultCred = 0x10,
			ValidateAuto = 0x20,
			SendAuxRecord = 0x200000,
			UseStrongCrypto = 0x400000
		}

		public const int CurrentVersion = 4;

		public int version;

		public int cCreds;

		public IntPtr certContextArray;

		private readonly IntPtr rootStore;

		public int cMappers;

		private readonly IntPtr phMappers;

		public int cSupportedAlgs;

		private readonly IntPtr palgSupportedAlgs;

		public SchProtocols grbitEnabledProtocols;

		public int dwMinimumCipherStrength;

		public int dwMaximumCipherStrength;

		public int dwSessionLifespan;

		public Flags dwFlags;

		public int reserved;

		public SecureCredential(int version, X509Certificate certificate, Flags flags, SchProtocols protocols)
		{
			rootStore = (phMappers = (palgSupportedAlgs = (certContextArray = IntPtr.Zero)));
			cCreds = (cMappers = (cSupportedAlgs = 0));
			dwMinimumCipherStrength = (dwMaximumCipherStrength = 0);
			dwSessionLifespan = (reserved = 0);
			this.version = version;
			dwFlags = flags;
			grbitEnabledProtocols = protocols;
			if (certificate != null)
			{
				certContextArray = certificate.Handle;
				cCreds = 1;
			}
		}

		[Conditional("TRAVE")]
		internal void DebugDump()
		{
		}
	}
}
