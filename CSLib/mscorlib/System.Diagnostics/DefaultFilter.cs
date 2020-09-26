namespace System.Diagnostics
{
	internal class DefaultFilter : AssertFilter
	{
		internal DefaultFilter()
		{
		}

		public override AssertFilters AssertFailure(string condition, string message, StackTrace location)
		{
			return (AssertFilters)Assert.ShowDefaultAssertDialog(condition, message);
		}
	}
}
