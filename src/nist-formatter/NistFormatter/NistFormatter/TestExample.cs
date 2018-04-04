using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
