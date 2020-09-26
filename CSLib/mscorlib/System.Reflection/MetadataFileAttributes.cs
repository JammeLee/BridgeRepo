namespace System.Reflection
{
	[Serializable]
	[Flags]
	internal enum MetadataFileAttributes
	{
		ContainsMetadata = 0x0,
		ContainsNoMetadata = 0x1
	}
}
