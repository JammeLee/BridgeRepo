using System.Runtime.InteropServices;

namespace System.Runtime.Remoting.Metadata.W3cXsd2001
{
	[Serializable]
	[ComVisible(true)]
	public sealed class SoapEntities : ISoapXsd
	{
		private string _value;

		public static string XsdType => "ENTITIES";

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

		public SoapEntities()
		{
		}

		public SoapEntities(string value)
		{
			_value = value;
		}

		public override string ToString()
		{
			return SoapType.Escape(_value);
		}

		public static SoapEntities Parse(string value)
		{
			return new SoapEntities(value);
		}
	}
}
