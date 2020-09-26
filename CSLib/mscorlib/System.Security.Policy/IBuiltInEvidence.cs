namespace System.Security.Policy
{
	internal interface IBuiltInEvidence
	{
		int OutputToBuffer(char[] buffer, int position, bool verbose);

		int InitFromBuffer(char[] buffer, int position);

		int GetRequiredSize(bool verbose);
	}
}
