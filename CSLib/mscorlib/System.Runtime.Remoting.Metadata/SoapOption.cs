using System.Runtime.InteropServices;

namespace System.Runtime.Remoting.Metadata
{
	[Serializable]
	[Flags]
	[ComVisible(true)]
	public enum SoapOption
	{
		None = 0x0,
		AlwaysIncludeTypes = 0x1,
		XsdString = 0x2,
		EmbedAll = 0x4,
		Option1 = 0x8,
		Option2 = 0x10
	}
}
