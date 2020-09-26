namespace System.Runtime.InteropServices
{
	[Guid("496B0ABF-CDEE-11d3-88E8-00902754C43A")]
	[Obsolete("Use System.Runtime.InteropServices.ComTypes.IEnumerator instead. http://go.microsoft.com/fwlink/?linkid=14202", false)]
	internal interface UCOMIEnumerator
	{
		object Current
		{
			get;
		}

		bool MoveNext();

		void Reset();
	}
}
