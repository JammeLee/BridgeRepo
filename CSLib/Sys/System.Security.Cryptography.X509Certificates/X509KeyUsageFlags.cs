namespace System.Security.Cryptography.X509Certificates
{
	[Flags]
	public enum X509KeyUsageFlags
	{
		None = 0x0,
		EncipherOnly = 0x1,
		CrlSign = 0x2,
		KeyCertSign = 0x4,
		KeyAgreement = 0x8,
		DataEncipherment = 0x10,
		KeyEncipherment = 0x20,
		NonRepudiation = 0x40,
		DigitalSignature = 0x80,
		DecipherOnly = 0x8000
	}
}
