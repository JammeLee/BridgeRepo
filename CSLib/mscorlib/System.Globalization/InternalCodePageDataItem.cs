namespace System.Globalization
{
	internal struct InternalCodePageDataItem
	{
		internal int codePage;

		internal int uiFamilyCodePage;

		internal unsafe char* webName;

		internal unsafe char* headerName;

		internal unsafe char* bodyName;

		internal uint flags;
	}
}
