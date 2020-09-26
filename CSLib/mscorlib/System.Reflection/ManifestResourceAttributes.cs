namespace System.Reflection
{
	[Serializable]
	[Flags]
	internal enum ManifestResourceAttributes
	{
		VisibilityMask = 0x7,
		Public = 0x1,
		Private = 0x2
	}
}
