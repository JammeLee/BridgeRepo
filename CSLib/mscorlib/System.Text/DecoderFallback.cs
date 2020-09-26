using System.Threading;

namespace System.Text
{
	[Serializable]
	public abstract class DecoderFallback
	{
		internal bool bIsMicrosoftBestFitFallback;

		private static DecoderFallback replacementFallback;

		private static DecoderFallback exceptionFallback;

		private static object s_InternalSyncObject;

		private static object InternalSyncObject
		{
			get
			{
				if (s_InternalSyncObject == null)
				{
					object value = new object();
					Interlocked.CompareExchange(ref s_InternalSyncObject, value, null);
				}
				return s_InternalSyncObject;
			}
		}

		public static DecoderFallback ReplacementFallback
		{
			get
			{
				if (replacementFallback == null)
				{
					lock (InternalSyncObject)
					{
						if (replacementFallback == null)
						{
							replacementFallback = new DecoderReplacementFallback();
						}
					}
				}
				return replacementFallback;
			}
		}

		public static DecoderFallback ExceptionFallback
		{
			get
			{
				if (exceptionFallback == null)
				{
					lock (InternalSyncObject)
					{
						if (exceptionFallback == null)
						{
							exceptionFallback = new DecoderExceptionFallback();
						}
					}
				}
				return exceptionFallback;
			}
		}

		public abstract int MaxCharCount
		{
			get;
		}

		internal bool IsMicrosoftBestFitFallback => bIsMicrosoftBestFitFallback;

		public abstract DecoderFallbackBuffer CreateFallbackBuffer();
	}
}
