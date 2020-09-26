namespace System.Runtime.Serialization.Formatters.Binary
{
	internal interface IStreamable
	{
		void Read(__BinaryParser input);

		void Write(__BinaryWriter sout);
	}
}
