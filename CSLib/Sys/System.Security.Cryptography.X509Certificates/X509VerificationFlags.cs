namespace System.Security.Cryptography.X509Certificates
{
	[Flags]
	public enum X509VerificationFlags
	{
		NoFlag = 0x0,
		IgnoreNotTimeValid = 0x1,
		IgnoreCtlNotTimeValid = 0x2,
		IgnoreNotTimeNested = 0x4,
		IgnoreInvalidBasicConstraints = 0x8,
		AllowUnknownCertificateAuthority = 0x10,
		IgnoreWrongUsage = 0x20,
		IgnoreInvalidName = 0x40,
		IgnoreInvalidPolicy = 0x80,
		IgnoreEndRevocationUnknown = 0x100,
		IgnoreCtlSignerRevocationUnknown = 0x200,
		IgnoreCertificateAuthorityRevocationUnknown = 0x400,
		IgnoreRootRevocationUnknown = 0x800,
		AllFlags = 0xFFF
	}
}
