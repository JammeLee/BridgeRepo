using System.Threading;

namespace System.Text
{
	[Serializable]
	public abstract class EncoderFallback
	{
		internal bool bIsMicrosoftBestFitFallback;

		private static EncoderFallback replacementFallback;

		private static EncoderFallback exceptionFallback;

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

		public static EncoderFallback ReplacementFallback
		{
			get
			{
				if (replacementFallback == null)
				{
					lock (InternalSyncObject)
					{
						if (replacementFallback == null)
						{
							replacementFallback = new EncoderReplacementFallback();
						}
					}
				}
				return replacementFallback;
			}
		}

		public static EncoderFallback ExceptionFallback
		{
			get
			{
				if (exceptionFallback == null)
				{
					lock (InternalSyncObject)
					{
						if (exceptionFallback == null)
						{
							exceptionFallback = new EncoderExceptionFallback();
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

		public abstract EncoderFallbackBuffer CreateFallbackBuffer();
	}
}
