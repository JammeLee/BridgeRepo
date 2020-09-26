namespace System.Reflection.Cache
{
	[Serializable]
	internal enum CacheAction
	{
		AllocateCache = 1,
		AddItem,
		ClearCache,
		LookupItemHit,
		LookupItemMiss,
		GrowCache,
		SetItemReplace,
		ReplaceFailed
	}
}
