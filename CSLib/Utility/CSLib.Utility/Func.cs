namespace CSLib.Utility
{
	public delegate TResult Func<TResult>();
	public delegate TResult Func<T1, TResult>(T1 arg1);
	public delegate TResult Func<T1, T2, TResult>(T1 arg1, T2 arg2);
	public delegate TResult Func<T1, T2, T3, TResult>(T1 arg1, T2 arg2, T3 arg3);
	public delegate TResult Func<T1, T2, T3, T4, TResult>(T1 arg1, T2 arg2, T3 arg3, T4 arg4);
	public delegate TResult Func<T1, T2, T3, T4, T5, TResult>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5);
}
