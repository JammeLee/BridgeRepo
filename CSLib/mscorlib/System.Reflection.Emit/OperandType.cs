using System.Runtime.InteropServices;

namespace System.Reflection.Emit
{
	[Serializable]
	[ComVisible(true)]
	public enum OperandType
	{
		InlineBrTarget = 0,
		InlineField = 1,
		InlineI = 2,
		InlineI8 = 3,
		InlineMethod = 4,
		InlineNone = 5,
		[Obsolete("This API has been deprecated. http://go.microsoft.com/fwlink/?linkid=14202")]
		InlinePhi = 6,
		InlineR = 7,
		InlineSig = 9,
		InlineString = 10,
		InlineSwitch = 11,
		InlineTok = 12,
		InlineType = 13,
		InlineVar = 14,
		ShortInlineBrTarget = 0xF,
		ShortInlineI = 0x10,
		ShortInlineR = 17,
		ShortInlineVar = 18
	}
}
