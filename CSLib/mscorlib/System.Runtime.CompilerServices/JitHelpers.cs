namespace System.Runtime.CompilerServices
{
	internal static class JitHelpers
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern void UnsafeSetArrayElement(object[] target, int index, object element);
	}
}
