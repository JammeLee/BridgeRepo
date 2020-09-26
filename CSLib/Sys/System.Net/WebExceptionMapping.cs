namespace System.Net
{
	internal static class WebExceptionMapping
	{
		private static readonly string[] s_Mapping = new string[21];

		internal static string GetWebStatusString(WebExceptionStatus status)
		{
			if ((int)status >= s_Mapping.Length || status < WebExceptionStatus.Success)
			{
				throw new InternalException();
			}
			string text = s_Mapping[(int)status];
			if (text == null)
			{
				text = "net_webstatus_" + status;
				s_Mapping[(int)status] = text;
			}
			return text;
		}
	}
}
