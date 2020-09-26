using System.Runtime.InteropServices;

namespace System.Runtime.CompilerServices
{
	[Serializable]
	[ComVisible(true)]
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Parameter, Inherited = false)]
	public sealed class DateTimeConstantAttribute : CustomConstantAttribute
	{
		private DateTime date;

		public override object Value => date;

		public DateTimeConstantAttribute(long ticks)
		{
			date = new DateTime(ticks);
		}
	}
}
