namespace System.Security.Cryptography
{
	internal class RSAPKCS1SHA1SignatureDescription : SignatureDescription
	{
		public RSAPKCS1SHA1SignatureDescription()
		{
			base.KeyAlgorithm = "System.Security.Cryptography.RSACryptoServiceProvider";
			base.DigestAlgorithm = "System.Security.Cryptography.SHA1CryptoServiceProvider";
			base.FormatterAlgorithm = "System.Security.Cryptography.RSAPKCS1SignatureFormatter";
			base.DeformatterAlgorithm = "System.Security.Cryptography.RSAPKCS1SignatureDeformatter";
		}

		public override AsymmetricSignatureDeformatter CreateDeformatter(AsymmetricAlgorithm key)
		{
			AsymmetricSignatureDeformatter asymmetricSignatureDeformatter = (AsymmetricSignatureDeformatter)CryptoConfig.CreateFromName(base.DeformatterAlgorithm);
			asymmetricSignatureDeformatter.SetKey(key);
			asymmetricSignatureDeformatter.SetHashAlgorithm("SHA1");
			return asymmetricSignatureDeformatter;
		}
	}
}
