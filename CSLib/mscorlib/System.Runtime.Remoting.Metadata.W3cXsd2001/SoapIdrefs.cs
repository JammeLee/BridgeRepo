using System.Runtime.InteropServices;

namespace System.Runtime.Remoting.Metadata.W3cXsd2001
{
	[Serializable]
	[ComVisible(true)]
	public sealed class SoapIdrefs : ISoapXsd
	{
		private string _value;

		public static string XsdType => "IDREFS";

		public string Value
		{
			get
			{
				return _value;
			}
			set
			{
				_value = value;
			}
		}

		public string GetXsdType()
		{
			return XsdType;
		}

		public SoapIdrefs()
		{
		}

		public SoapIdrefs(string value)
		{
			_value = value;
		}

		public override string ToString()
		{
			return SoapType.Escape(_value);
		}

		public static SoapIdrefs Parse(string value)
		{
			return new SoapIdrefs(value);
		}
	}
}
