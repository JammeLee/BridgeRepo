using System.Runtime.CompilerServices;

namespace System
{
	internal static class Mda
	{
		private enum MdaState
		{
			Unknown,
			Enabled,
			Disabled
		}

		private static MdaState _streamWriterMDAState;

		internal static bool StreamWriterBufferMDAEnabled
		{
			get
			{
				if (_streamWriterMDAState == MdaState.Unknown)
				{
					if (IsStreamWriterBufferedDataLostEnabled())
					{
						_streamWriterMDAState = MdaState.Enabled;
					}
					else
					{
						_streamWriterMDAState = MdaState.Disabled;
					}
				}
				return _streamWriterMDAState == MdaState.Enabled;
			}
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern void MemberInfoCacheCreation();

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern void DateTimeInvalidLocalFormat();

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern void StreamWriterBufferedDataLost(string text);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern bool IsStreamWriterBufferedDataLostEnabled();

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern bool IsInvalidGCHandleCookieProbeEnabled();

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern void FireInvalidGCHandleCookieProbe(IntPtr cookie);
	}
}
