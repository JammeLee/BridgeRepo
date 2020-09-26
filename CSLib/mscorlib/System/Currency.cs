using System.Runtime.CompilerServices;

namespace System
{
	[Serializable]
	internal struct Currency
	{
		internal long m_value;

		public Currency(decimal value)
		{
			m_value = decimal.ToCurrency(value).m_value;
		}

		internal Currency(long value, int ignored)
		{
			m_value = value;
		}

		public static Currency FromOACurrency(long cy)
		{
			return new Currency(cy, 0);
		}

		public long ToOACurrency()
		{
			return m_value;
		}

		public static decimal ToDecimal(Currency c)
		{
			decimal result = 0m;
			FCallToDecimal(ref result, c);
			return result;
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void FCallToDecimal(ref decimal result, Currency c);
	}
}
