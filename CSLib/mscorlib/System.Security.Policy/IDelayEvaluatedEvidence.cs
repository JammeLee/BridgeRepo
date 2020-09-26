namespace System.Security.Policy
{
	internal interface IDelayEvaluatedEvidence
	{
		bool IsVerified
		{
			get;
		}

		bool WasUsed
		{
			get;
		}

		void MarkUsed();
	}
}
