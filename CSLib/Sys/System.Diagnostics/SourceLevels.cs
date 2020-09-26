using System.ComponentModel;

namespace System.Diagnostics
{
	[Flags]
	public enum SourceLevels
	{
		Off = 0x0,
		Critical = 0x1,
		Error = 0x3,
		Warning = 0x7,
		Information = 0xF,
		Verbose = 0x1F,
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		ActivityTracing = 0xFF00,
		All = -1
	}
}
