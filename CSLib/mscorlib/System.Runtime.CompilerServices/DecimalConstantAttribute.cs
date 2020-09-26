using System.Runtime.InteropServices;

namespace System.Runtime.CompilerServices
{
	[Serializable]
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Parameter, Inherited = false)]
	[ComVisible(true)]
	public sealed class DecimalConstantAttribute : Attribute
	{
		private decimal dec;

		public decimal Value => dec;

		[CLSCompliant(false)]
		public DecimalConstantAttribute(byte scale, byte sign, uint hi, uint mid, uint low)
		{
			dec = new decimal((int)low, (int)mid, (int)hi, sign != 0, scale);
		}

		public DecimalConstantAttribute(byte scale, byte sign, int hi, int mid, int low)
		{
			dec = new decimal(low, mid, hi, sign != 0, scale);
		}
	}
}
