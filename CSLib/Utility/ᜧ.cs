using CSLib.Utility;

internal class ᜧ : ᜱ
{
	~ᜧ()
	{
		ᜁ();
	}

	public bool ᜁ(string A_0)
	{
		return CSingleton<ᝎ>.Instance.ᜁ(A_0);
	}

	public bool ᜁ(string A_0, EFileLogTime A_1)
	{
		return CSingleton<ᝎ>.Instance.ᜀ(A_0, A_1);
	}

	public bool ᜁ(string A_0, uint A_1)
	{
		return CSingleton<ᝎ>.Instance.ᜀ(A_0, A_1);
	}

	public new void ᜁ()
	{
		CSingleton<ᝎ>.Instance.ᜃ();
	}

	protected override void _171D(ELogLevel A_0, string A_1, int A_2, string A_3, params object[] A_4)
	{
		CSingleton<ᝎ>.Instance.ᜀ(A_0, A_1, A_2, A_3, A_4);
	}
}
