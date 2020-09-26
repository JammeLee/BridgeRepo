namespace System.Net
{
	internal enum CertificateEncoding
	{
		Zero = 0,
		X509AsnEncoding = 1,
		X509NdrEncoding = 2,
		Pkcs7AsnEncoding = 0x10000,
		Pkcs7NdrEncoding = 0x20000,
		AnyAsnEncoding = 65537
	}
}
