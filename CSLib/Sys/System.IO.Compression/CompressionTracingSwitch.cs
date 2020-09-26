using System.Diagnostics;

namespace System.IO.Compression
{
	internal class CompressionTracingSwitch : Switch
	{
		internal static CompressionTracingSwitch tracingSwitch = new CompressionTracingSwitch("CompressionSwitch", "Compression Library Tracing Switch");

		public static bool Verbose => tracingSwitch.SwitchSetting >= 2;

		public static bool Informational => tracingSwitch.SwitchSetting >= 1;

		internal CompressionTracingSwitch(string displayName, string description)
			: base(displayName, description)
		{
		}
	}
}
