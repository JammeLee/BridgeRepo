namespace System
{
	internal static class LOGIC
	{
		internal static bool IMPLIES(bool p, bool q)
		{
			if (p)
			{
				return q;
			}
			return true;
		}

		internal static bool BIJECTION(bool p, bool q)
		{
			if (IMPLIES(p, q))
			{
				return IMPLIES(q, p);
			}
			return false;
		}
	}
}
