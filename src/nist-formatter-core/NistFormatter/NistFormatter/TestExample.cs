namespace NistFormatter
{
	struct TestExample
	{
		public string Filename;
		public string CorrectResult;

		public TestExample(string Filename, string CorrectResult)
		{
			this.Filename = Filename;
			this.CorrectResult = CorrectResult;
		}
	}
}
