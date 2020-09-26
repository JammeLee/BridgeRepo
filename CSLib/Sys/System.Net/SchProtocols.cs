namespace System.Net
{
	internal enum SchProtocols
	{
		Zero = 0,
		PctClient = 2,
		PctServer = 1,
		Pct = 3,
		Ssl2Client = 8,
		Ssl2Server = 4,
		Ssl2 = 12,
		Ssl3Client = 0x20,
		Ssl3Server = 0x10,
		Ssl3 = 48,
		TlsClient = 0x80,
		TlsServer = 0x40,
		Tls = 192,
		Tls11Client = 0x200,
		Tls11Server = 0x100,
		Tls11 = 768,
		Tls12Client = 0x800,
		Tls12Server = 0x400,
		Tls12 = 3072,
		Ssl3Tls = 240,
		UniClient = int.MinValue,
		UniServer = 0x40000000,
		Unified = -1073741824,
		ClientMask = -2147480918,
		ServerMask = 1073743189
	}
}
