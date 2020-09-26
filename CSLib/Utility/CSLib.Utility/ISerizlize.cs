namespace CSLib.Utility
{
	public interface ISerizlize
	{
		bool Serizlize();

		bool Deserizlize();

		bool Serizlize(string filename);

		bool Deserizlize(string filename);
	}
}
